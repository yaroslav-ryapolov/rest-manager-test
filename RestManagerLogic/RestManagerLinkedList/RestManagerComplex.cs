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
            _clientsManager.AddAndEnqueueGroup(group);

            var tableToSeatAt = _tablesManager.DoSeatsLookup(group.Size);
            if (tableToSeatAt != null)
            {
                _tablesManager.SeatGroupAtTable(group, tableToSeatAt);
                _clientsManager.SeatGroupAtTable(group, tableToSeatAt);
            }
        }

        public void OnLeave(ClientsGroup group)
        {
            using (_readerWriterLock.TakeWriterDisposableLock())
            {
                DoOnLeave(group);
            }
        }

        private void DoOnLeave(ClientsGroup leavingGroup)
        {
            var table = _clientsManager.GetGroupTable(leavingGroup);
            _clientsManager.RemoveGroup(leavingGroup);

            if (table == null)
            {
                return;
            }

            _tablesManager.ReleaseTableFromGroup(table, leavingGroup);

            // try to seat at released chairs somebody from queue
            var groupFromQueue = _clientsManager.FindNextSmallerOrEqualGroupInQueue(table.AvailableChairs);
            while (groupFromQueue != null && table.AvailableChairs > 0)
            {
                SeatGroupAtTable(groupFromQueue, table);

                groupFromQueue = _clientsManager.FindNextSmallerOrEqualGroupInQueue(table.AvailableChairs);
            }
        }

        private void SeatGroupAtTable(ClientsGroup group, Table table)
        {
            _tablesManager.SeatGroupAtTable(group, table);
            _clientsManager.SeatGroupAtTable(group, table);
        }

        public Table Lookup(ClientsGroup group)
        {
            using (_readerWriterLock.TakeReaderDisposableLock())
            {
                return _clientsManager.GetGroupTable(group);
            }
        }
    }
}
