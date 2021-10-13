using System;
using System.Collections.Generic;
using System.Linq;

namespace RestManagerLogic
{
    public class RestManager
    {
        private readonly int _maxTableSize;
        private readonly List<TableWithNode> _tables;
        private readonly LinkedList<ClientsGroup> _clientsQueue = new LinkedList<ClientsGroup>();
        
        private readonly List<LinkedList<TableWithNode>> _tablesByAvailableChairs;

        public IEnumerable<Table> Tables => _tables.Select((t) => t.Table);

        public RestManager(List<Table> tables)
        {
            // doing the copy to avoid effects from external list changes
            _tables = new List<TableWithNode>(tables.Count);

            _maxTableSize = tables.Max((t) => t.Size);
            _tablesByAvailableChairs = new List<LinkedList<TableWithNode>>(_maxTableSize + 1);
            for (int i = 0; i < _tablesByAvailableChairs.Capacity; i++)
            {
                _tablesByAvailableChairs.Add(new LinkedList<TableWithNode>());
            }

            foreach (var table in tables)
            {
                var tableWithNode = new TableWithNode(table);
                tableWithNode.Node = _tablesByAvailableChairs[table.AvailableChairs].AddLast(tableWithNode);
                
                _tables.Add(tableWithNode);
            }
        }

        public void OnArrive(ClientsGroup group)
        {
            // TO LOCK Queue
            _clientsQueue.AddLast(group);
            
            this.TryToSeatSomebodyFromQueue();
        }

        public void OnLeave(ClientsGroup group)
        {
            // TO LOCK Queue
            if (_clientsQueue.Remove(group))
            {
                return;
            }

            // TO LOCK Tables Matrix
            var table = this.Lookup(group);
            if (table == null)
            {
                throw new ArgumentOutOfRangeException(nameof(group), "Group is not found at any table");
            }

            var tableWithNode = _tables.Single((t) => t.Table == table);
            
            // todo: receive LinkedList and use it to remove faster
            _tablesByAvailableChairs[table.AvailableChairs].Remove(tableWithNode);
            var newTableHead = _tablesByAvailableChairs[table.AvailableChairs + group.Size];
            if (table.Size == table.AvailableChairs + group.Size)
            {
                // put free tables as first possible options at the beginning
                newTableHead.AddFirst(tableWithNode);
            }
            else
            {
                // put occupied tables at the end in case there is no free tables
                newTableHead.AddLast(tableWithNode);
            }

            table.ReleaseChairs(group);

            this.TryToSeatSomebodyFromQueue();
        }
        
        public Table Lookup(ClientsGroup group)
        {
            // TO LOCK Tables Matrix
            
            // нужно причесать чуток и оптимизировать (хешиком, где хранится линкед лист на стол, который также обновляется в случае необходимости)
            return _tables
                .SelectMany((t) => t.Table.SeatedClientGroups
                    .Select((g) => new
                    {
                        table = t.Table,
                        group = g
                    })
                )
                .Where((t) => t.group == group)
                .Select((t) => t.table)
                .SingleOrDefault();
        }

        public void TryToSeatSomebodyFromQueue()
        {
            // TO LOCK Queue
            int minimumNotSeatedSize = _maxTableSize + 1;
            
            var queueItem = _clientsQueue.First;
            while (queueItem != null && minimumNotSeatedSize > 1)
            {
                var current = queueItem;
                queueItem = queueItem.Next;

                
                if (current.Value.Size >= minimumNotSeatedSize)
                {
                    continue;
                }

                if (this.TrySeatClientsGroup(current.Value))
                {
                    _clientsQueue.Remove(current);
                }
                else
                {
                    minimumNotSeatedSize = current.Value.Size;
                }
            }
        }

        public bool TrySeatClientsGroup(ClientsGroup group)
        {
            // TO LOCK Tables Matrix
            
            var tablesWithEnoughRoom = _tablesByAvailableChairs[group.Size];
            int i = group.Size + 1;
            while ((!tablesWithEnoughRoom.Any() || tablesWithEnoughRoom.First?.Value.Table.IsOccupied == true)
                   && i < _tablesByAvailableChairs.Count)
            {
                if (tablesWithEnoughRoom.First?.Value.Table.IsOccupied != true
                    || _tablesByAvailableChairs[i].First?.Value.Table.IsOccupied == false)
                {
                    tablesWithEnoughRoom = _tablesByAvailableChairs[i];
                }

                i++;
            }

            var table = tablesWithEnoughRoom.First?.Value;
            if (table == null)
            {
                return false;
            }

            // move table to new list
            tablesWithEnoughRoom.RemoveFirst();
            _tablesByAvailableChairs[table.Table.AvailableChairs - group.Size].AddLast(table);

            table.Table.SeatClientsGroup(group);
            return true;
        }
        
        private class TableWithNode
        {
            public readonly Table Table;
            public LinkedListNode<TableWithNode> Node;

            public TableWithNode(Table table)
            {
                this.Table = table;
            }
        }
    }
}