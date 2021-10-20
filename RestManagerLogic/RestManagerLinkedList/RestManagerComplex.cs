using System;
using System.Collections.Generic;

namespace RestManagerLogic.RestManagerLinkedList
{
    public class RestManagerComplex
    {
        private readonly TablesManager _tablesManager;
        private readonly ClientsManager _clientsManager = new();
        private readonly ReadWriteLockSlimDisposableWrap _readerWriterLock = new();

        public IEnumerable<Table> Tables => _tablesManager.GetTables();

        public RestManagerComplex(List<Table> tables)
        {
            _tablesManager = new TablesManager(tables);
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
            TrySeatSomebodyFromQueue();
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

        private void TrySeatSomebodyFromQueue()
        {
            int minimumNotSeatedSize = _tablesManager.MaxTableSize + 1;

            var queueItem = _clientsManager.GetCurrentAndMoveNext();
            while (queueItem != null && minimumNotSeatedSize > 1)
            {
                if (queueItem.Current.Size < minimumNotSeatedSize && !TrySeatClientsGroup(queueItem.Current))
                {
                    minimumNotSeatedSize = queueItem.Current.Size;
                }

                queueItem = _clientsManager.GetCurrentAndMoveNext(queueItem);
            }
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