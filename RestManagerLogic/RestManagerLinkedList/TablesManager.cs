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

        public Table DoSeatsLookup(int size)
        {
            var possibleTables = _tablesBySeats
                .Skip(size)
                .Where((a) => a.First != null)
                .Select((a) => a.First.Value);

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

        public void SeatGroupAtTable(ClientsGroup group, Table table)
        {
            table.SeatClientsGroup(group);
            UpdateTableIndex(table);
        }

        public void ReleaseTableFromGroup(Table table, ClientsGroup group)
        {
            table.ReleaseChairs(group);
            UpdateTableIndex(table);
        }

        private void UpdateTableIndex(Table table)
        {
            var tableNode = _tables[table.Guid];

            tableNode.List?.Remove(tableNode);
            var newTableHead = _tablesBySeats[table.AvailableChairs];

            if (table.IsOccupied)
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
    }
}
