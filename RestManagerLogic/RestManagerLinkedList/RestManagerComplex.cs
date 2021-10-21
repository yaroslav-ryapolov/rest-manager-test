using System;
using System.Collections.Generic;

namespace RestManagerLogic.RestManagerLinkedList
{
    public class RestManagerComplex: IRestManager
    {
        private readonly TablesManager _tablesManager;
        private readonly ClientsManager _clientsManager;
        private readonly ReadWriteLockSlimDisposableWrap _readerWriterLock = new();

        public IEnumerable<Table> Tables => _tablesManager.GetTables();

        public RestManagerComplex(List<Table> tables)
        {
            _tablesManager = new TablesManager(tables);
            _clientsManager = new ClientsManager(_tablesManager.MaxTableSize);
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
            _clientsManager.AddGroup(group);

            if (!TrySeatClientsGroup(group))
            {
                _clientsManager.EnqueueGroup(group);
            }
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
            if (_clientsManager.DequeueGroup(group))
            {
                _clientsManager.RemoveGroup(group);
                return;
            }

            var table = DoTableLookup(group);
            _clientsManager.RemoveGroup(group);
            if (table == null)
            {
                throw new ArgumentOutOfRangeException(nameof(group), "Group is not found neither in queue or at any table");
            }

            _tablesManager.ReleaseTableFromGroup(table, group);

            //  TODO: it makes sense to seat at released table as there is no new seats appeared
            TrySeatSomebodyFromQueue(table.AvailableChairs);
        }

        private void TrySeatSomebodyFromQueue(int freeSeatsSize)
        {
            // int minimumNotSeatedSize = _tablesManager.MaxTableSize + 1;
            //
            // var queueItem = _clientsManager.GetCurrentAndMoveNext();
            // while (queueItem != null && minimumNotSeatedSize > 1)
            // {
            //     if (queueItem.Current.Group.Size < minimumNotSeatedSize && !TrySeatClientsGroup(queueItem.Current.Group))
            //     {
            //         minimumNotSeatedSize = queueItem.Current.Group.Size;
            //     }
            //
            //     queueItem = _clientsManager.GetCurrentAndMoveNext(queueItem);
            // }

            var groupToSeat = _clientsManager.FindNextSmallerOrEqualGroupInQueue(freeSeatsSize);
            while (groupToSeat != null && freeSeatsSize > 0)
            {
                if (TrySeatClientsGroup(groupToSeat))
                {
                    freeSeatsSize -= groupToSeat.Size;
                }

                if (freeSeatsSize > 0)
                {
                    groupToSeat = _clientsManager.FindNextSmallerOrEqualGroupInQueue(freeSeatsSize);
                }
            }
        }

        public Table Lookup(ClientsGroup group)
        {
            using (_readerWriterLock.TakeReaderDisposableLock())
            {
                return DoTableLookup(group);
            }
        }

        private Table DoTableLookup(ClientsGroup group)
        {
            return _clientsManager.GetGroupTable(group);
        }

        private bool TrySeatClientsGroup(ClientsGroup group)
        {
            var table = _tablesManager.AssignGroupToTable(group);
            if (table == null)
            {
                return false;
            }

            _clientsManager.SeatGroupAtTable(group, table);
            return true;
        }
    }
}
