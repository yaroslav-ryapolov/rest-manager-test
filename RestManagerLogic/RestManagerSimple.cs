using System;
using System.Collections.Generic;
using System.Linq;

namespace RestManagerLogic
{
    public class RestManagerSimple: IRestManager
    {
        private readonly ReadWriteLockSlimDisposableWrap _readerWriterLock = new();
        private readonly List<Table> _tablesSorted;
        private readonly List<ClientsGroup> _clientsQueue = new();

        public IEnumerable<Table> Tables => _tablesSorted;

        public RestManagerSimple(List<Table> tables)
        {
            _tablesSorted = tables
                .OrderBy((t) => t.Size)
                .ToList();
        }

        public void OnArrive(ClientsGroup group)
        {
            using (_readerWriterLock.TakeWriterDisposableLock())
            {
                DoOnArrive(group);
            }
        }

        private void DoOnArrive(ClientsGroup group)
        {
            var tableToSeatAt = DoSeatsLookup(group);
            if (tableToSeatAt != null)
            {
                tableToSeatAt.SeatClientsGroup(group);
            }
            else
            {
                _clientsQueue.Add(group);
            }
        }

        private Table DoSeatsLookup(ClientsGroup group)
        {
            var possibleTables = _tablesSorted.Where((t) => t.AvailableChairs >= group.Size);
            Table resultTable = null;
            foreach (var table in possibleTables)
            {
                if (!table.IsOccupied)
                {
                    return table;
                }

                resultTable ??= table;
            }

            return resultTable;
        }

        public void OnLeave(ClientsGroup group)
        {
            using (_readerWriterLock.TakeWriterDisposableLock())
            {
                DoOnLeave(group);
            }
        }

        private void DoOnLeave(ClientsGroup group)
        {
            if (_clientsQueue.Remove(group))
            {
                return;
            }

            var table = DoSeatedGroupLookup(group);
            if (table == null)
            {
                throw new ArgumentException("No such group found", nameof(group));
            }
            table.ReleaseChairs(group);

            // trying to dequeue some waiting groups
            var groupToSeat = _clientsQueue.FirstOrDefault((g) => g.Size <= table.AvailableChairs);
            while (groupToSeat != null)
            {
                table.SeatClientsGroup(groupToSeat);
                _clientsQueue.Remove(groupToSeat);
                groupToSeat = _clientsQueue.FirstOrDefault((g) => g.Size <= table.AvailableChairs);
            }
        }

        public Table Lookup(ClientsGroup group)
        {
            using (_readerWriterLock.TakeReaderDisposableLock())
            {
                return DoSeatedGroupLookup(group);
            }
        }

        private Table DoSeatedGroupLookup(ClientsGroup group)
        {
            return _tablesSorted.FirstOrDefault((t) => t.HasGroup(group));
        }
    }
}
