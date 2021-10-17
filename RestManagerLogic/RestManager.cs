using System;
using System.Collections.Generic;
using System.Linq;

namespace RestManagerLogic
{
    public class RestManager
    {
        private readonly TablesManager _tablesManager;
        private readonly ClientsManager _clientsManager = new();
        private readonly ReadWriteLockSlimDisposableWrap _readerWriterLock = new();

        public IEnumerable<Table> Tables => _tablesManager.GetTables();

        public RestManager(List<Table> tables)
        {
            _tablesManager = new TablesManager(tables);
        }

        public void OnArrive(ClientsGroup group)
        {
            using (_readerWriterLock.TakeWriterDisposableLock())
            {
                ProcessNewGroup(group);
            }
        }

        private void ProcessNewGroup(ClientsGroup group)
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
                ProcessGroupLeaving(group);
            }
        }

        private void ProcessGroupLeaving(ClientsGroup group)
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

        private class GroupInRest
        {
            public LinkedListNode<ClientsGroup> Node { get; private set; }
            public Table Table { get; private set; }

            public GroupInRest(ClientsGroup group)
            {
                Node = new LinkedListNode<ClientsGroup>(group);
            }

            public void SetTable(Table table)
            {
                Node = null;
                Table = table;
            }
        }

        private class TablesManager
        {
            public readonly int MaxTableSize;
            private readonly Dictionary<Guid, LinkedListNode<Table>> _tables;
            private readonly List<LinkedList<Table>> _tablesBySeats;

            public TablesManager(List<Table> tables)
            {
                _tables = new Dictionary<Guid, LinkedListNode<Table>>(tables.Count);

                MaxTableSize = tables.Max((t) => t.Size);
                _tablesBySeats = new List<LinkedList<Table>>(MaxTableSize + 1);
                for (int i = 0; i < _tablesBySeats.Capacity; i++)
                {
                    _tablesBySeats.Add(new LinkedList<Table>());
                }

                foreach (var table in tables)
                {
                    var tableNode = _tablesBySeats[table.AvailableChairs].AddLast(table);
                    _tables.Add(table.Guid, tableNode);
                }
            }

            public IEnumerable<Table> GetTables()
            {
                return _tables.Values.Select((t) => t.Value);
            }

            public Table AssignGroupToTable(ClientsGroup group)
            {
                var enoughRoomTables = _tablesBySeats[group.Size];
                int i = group.Size + 1;
                while (IsEmptyListOrAllTablesOccupied(enoughRoomTables) && i < _tablesBySeats.Count)
                {
                    if (!enoughRoomTables.Any() || HasFreeTable(_tablesBySeats[i]))
                    {
                        enoughRoomTables = _tablesBySeats[i];
                    }

                    i++;
                }

                var tableNode = enoughRoomTables.First;
                if (tableNode == null)
                {
                    return null;
                }

                // move table to new list
                ChangeTableIndex(tableNode.Value, tableNode.Value.AvailableChairs - group.Size);

                tableNode.Value.SeatClientsGroup(group);
                return tableNode.Value;
            }

            public void ReleaseTableFromGroup(Table table, ClientsGroup group)
            {
                ChangeTableIndex(table, table.AvailableChairs + group.Size);
                table.ReleaseChairs(group);
            }
            
            private void ChangeTableIndex(Table table, int newAvailableSeatsValue)
            {
                var tableNode = _tables[table.Guid];

                _tablesBySeats[table.AvailableChairs].Remove(tableNode);
                var newTableHead = _tablesBySeats[newAvailableSeatsValue];

                bool isStillOccupied = table.Size > newAvailableSeatsValue;
                if (isStillOccupied)
                {
                    // put occupied tables at the end in case there is no free tables
                    newTableHead.AddLast(tableNode);
                }
                else
                {
                    // put free tables as first possible options at the beginning
                    newTableHead.AddFirst(tableNode);
                }
            }

            private bool HasFreeTable(LinkedList<Table> tables)
            {
                var first = tables.First;
                if (first == null)
                {
                    return false;
                }

                return !first.Value.IsOccupied;
            }

            private bool IsEmptyListOrAllTablesOccupied(LinkedList<Table> tables)
            {
                var first = tables.First;
                if (first == null)
                {
                    return true;
                }

                return first.Value.IsOccupied;
            }
        }

        private class ClientsManager
        {
            private readonly LinkedList<ClientsGroup> _clientsQueue = new();
            private readonly Dictionary<Guid, GroupInRest> _clients = new();

            public void AddGroup(ClientsGroup group)
            {
                if (!_clients.TryAdd(group.Guid, new GroupInRest(group)))
                {
                    throw new ArgumentException("Cannot add already presented group", nameof(group));
                }
            }

            public void RemoveGroup(ClientsGroup group)
            {
                DequeueGroup(group);
                if (!_clients.Remove(group.Guid))
                {
                    throw new ArgumentException("Cannot remove not presented group", nameof(group));
                }
            }

            public void EnqueueGroup(ClientsGroup group)
            {
                var groupInRest = _clients[group.Guid];
                _clientsQueue.AddLast(groupInRest.Node);
            }

            public bool DequeueGroup(ClientsGroup group)
            {
                var groupNode = _clients[group.Guid].Node;
                if (groupNode?.List != null)
                {
                    _clientsQueue.Remove(groupNode);
                    return true;
                }

                return false;
            }

            public void SeatGroupAtTable(ClientsGroup group, Table table)
            {
                var groupInRest = _clients[group.Guid];
                if (groupInRest.Table != null)
                {
                    throw new InvalidOperationException("Cannot re-assign already seated group");
                }

                DequeueGroup(group);
                groupInRest.SetTable(table);
            }

            public Table GetGroupTable(ClientsGroup group)
            {
                return _clients[group.Guid].Table;
            }

            public QueueItem GetCurrentAndMoveNext(QueueItem current = null)
            {
                var groupNode = _clientsQueue.First;
                if (current != null)
                {
                    groupNode = current.Next;
                }

                if (groupNode == null)
                {
                    return null;
                }

                return new QueueItem
                {
                    Next = groupNode.Next,
                    Current = groupNode.Value,
                };
            }
            
            public class QueueItem
            {
                public LinkedListNode<ClientsGroup> Next;
                public ClientsGroup Current;
            }
        }
    }
}