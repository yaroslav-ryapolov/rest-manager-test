using System;
using System.Collections.Generic;
using System.Linq;

namespace RestManagerLogic.RestManagerLinkedList
{
    public class TablesManager
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
}