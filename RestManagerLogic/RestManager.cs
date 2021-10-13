using System;
using System.Collections.Generic;
using System.Linq;

namespace RestManagerLogic
{
    public class RestManager
    {
        private readonly int _maxTableSize;
        private readonly Dictionary<Guid, LinkedListNode<Table>> _tables;
        private readonly List<LinkedList<Table>> _tablesBySeats;
        private readonly LinkedList<GroupWithNode> _clientsQueue = new();
        private readonly Dictionary<Guid, GroupWithNode> _clients = new();

        public IEnumerable<Table> Tables => _tables.Values.Select((t) => t.Value);

        public RestManager(List<Table> tables)
        {
            _tables = new Dictionary<Guid, LinkedListNode<Table>>(tables.Count);

            _maxTableSize = tables.Max((t) => t.Size);
            _tablesBySeats = new List<LinkedList<Table>>(_maxTableSize + 1);
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

        public void OnArrive(ClientsGroup group)
        {
            // TO LOCK Queue
            var groupWithNode = new GroupWithNode(group);
            groupWithNode.Node = _clientsQueue.AddLast(groupWithNode);

            _clients[group.Guid] = groupWithNode;

            TrySeatSomebodyFromQueue();
        }

        public void OnLeave(ClientsGroup group)
        {
            try
            {
                // TO LOCK Queue
                var groupWithNode = _clients[group.Guid];
                if (groupWithNode == null)
                {
                    throw new ArgumentOutOfRangeException(nameof(group), "Group is not found");
                }

                if (groupWithNode.Node != null)
                {
                    _clientsQueue.Remove(groupWithNode.Node);
                    return;
                }

                // TO LOCK Tables Matrix
                var table = Lookup(group);
                if (table == null)
                {
                    throw new ArgumentOutOfRangeException(nameof(group), "Group is not found at any table");
                }

                var tableNode = _tables[table.Guid];

                _tablesBySeats[table.AvailableChairs].Remove(tableNode);
                var newTableHead = _tablesBySeats[table.AvailableChairs + group.Size];
                if (table.Size == table.AvailableChairs + group.Size)
                {
                    // put free tables as first possible options at the beginning
                    newTableHead.AddFirst(tableNode);
                }
                else
                {
                    // put occupied tables at the end in case there is no free tables
                    newTableHead.AddLast(tableNode);
                }

                table.ReleaseChairs(group);

                TrySeatSomebodyFromQueue();
            }
            finally
            {
                _clients.Remove(group.Guid);
            }
        }
        
        public Table Lookup(ClientsGroup group)
        {
            // TO LOCK Tables Matrix
            // TO LOCK Queue

            return _clients[group.Guid].TableNode?.Value;
        }

        public void TrySeatSomebodyFromQueue()
        {
            // TO LOCK Queue
            int minimumNotSeatedSize = _maxTableSize + 1;
            
            var queueItem = _clientsQueue.First;
            while (queueItem != null && minimumNotSeatedSize > 1)
            {
                var current = queueItem;
                queueItem = queueItem.Next;

                
                if (current.Value.Group.Size >= minimumNotSeatedSize)
                {
                    continue;
                }

                if (TrySeatClientsGroup(current.Value.Group))
                {
                    _clientsQueue.Remove(current);
                    current.Value.Node = null;
                }
                else
                {
                    minimumNotSeatedSize = current.Value.Group.Size;
                }
            }
        }

        private bool TrySeatClientsGroup(ClientsGroup group)
        {
            // TO LOCK Tables Matrix
            
            var enoughRoomTables = _tablesBySeats[group.Size];
            int i = group.Size + 1;
            while (IsEmptyOrAllOccupied(enoughRoomTables) && i < _tablesBySeats.Count)
            {
                if (HasFreeTable(_tablesBySeats[i]))
                {
                    enoughRoomTables = _tablesBySeats[i];
                }

                i++;
            }

            var tableNode = enoughRoomTables.First;
            if (tableNode == null)
            {
                return false;
            }

            // move table to new list
            enoughRoomTables.RemoveFirst();
            _tablesBySeats[tableNode.Value.AvailableChairs - group.Size].AddLast(tableNode);

            // assign group to table
            var groupWithNode = _clients[group.Guid];
            groupWithNode.TableNode = tableNode;
            
            tableNode.Value.SeatClientsGroup(group);
            return true;
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

        private bool IsEmptyOrAllOccupied(LinkedList<Table> tables)
        {
            var first = tables.First;
            if (first == null)
            {
                return true;
            }

            return first.Value.IsOccupied;
        }

        // TODO: Есть смысл переименовать в GroupInRest
        private class GroupWithNode
        {
            public readonly ClientsGroup Group;
            public LinkedListNode<Table> TableNode;
            public LinkedListNode<GroupWithNode> Node;

            public GroupWithNode(ClientsGroup group)
            {
                Group = group;
            }
        }
    }
}