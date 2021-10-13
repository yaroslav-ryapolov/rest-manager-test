using System;
using System.Collections.Generic;
using System.Linq;

namespace RestManagerLogic
{
    public class RestManager
    {
        protected List<TableInRest> _tables;
        protected List<ClientsGroup> _clientsQueue = new List<ClientsGroup>();

        // todo: можно будет чуток добавить "оптимизации"
        // private Dictionary<int, List<TableInRest>> _freeTablesBySeats;
        // private Dictionary<int, List<TableInRest>> _occupiedTablesBySeats = new Dictionary<int, List<TableInRest>();

        // todo: use capacity as (biggest table Size + 1) on constructor
        private List<LinkedList<TableInRest>> _tablesByAvailableChairs = new List<LinkedList<TableInRest>>(7);

        public List<TableInRest> Tables => _tables;
        public List<ClientsGroup> ClientsQueue => _clientsQueue;

        public RestManager(List<Table> tables)
        {
            _tables = tables.Select((t) => new TableInRest(t))
                .ToList();

            // _freeTablesBySeats = _tables.GroupBy((t) => t.AvailableChairs)
            //     .ToDictionary((t) => t.Key, (x) => x.ToList());

            // todo: вынести в отдельный внутренний класс, где всё чуть более явно обозвать
            for (int i = 0; i < _tablesByAvailableChairs.Capacity; i++)
            {
                _tablesByAvailableChairs.Add(new LinkedList<TableInRest>());
            }

            foreach (var tableInRest in _tables.OrderBy((t) => t.TotalChairs))
            {
                _tablesByAvailableChairs[tableInRest.AvailableChairs].AddLast(tableInRest);
            }
        }

        public void OnArrive(ClientsGroup group)
        {
            // todo: чуток ускорить нужно, чтобы не двигать всех а снова использовать что-то вроде LinkedList
            _clientsQueue.Insert(0, group);
            
            this.TryToSeatSomebodyFromQueue();
        }

        public void OnLeave(ClientsGroup group)
        {
            if (_clientsQueue.Remove(group))
            {
                return;
            }

            var tableWithGroup = this.GetTableByGroup(group);
            if (tableWithGroup == null)
            {
                return;
            }
            
            tableWithGroup.ReleaseChairs(group);
            // if (tableWithGroup.IsOccupied)
            // {
            //     определить в каком из словарей оставить стол
            // }
            
            this.TryToSeatSomebodyFromQueue();
        }

        // WhereSeated?
        protected TableInRest GetTableByGroup(ClientsGroup group)
        {
            // нужно причесать чуток и оптимизировать
            return _tables
                .SelectMany((t) => t.SeatedClientGroups
                    .Select((g) => new
                    {
                        table = t,
                        group = g
                    })
                )
                .Where((t) => t.group == group)
                .Select((t) => t.table)
                .SingleOrDefault();
        }

        public void TryToSeatSomebodyFromQueue()
        {
            // как-то "упростить" формат посадки (не искать сквозь всю очередь всегда, а разбить на кол-во вариаций
            // мест и их уже сравнивать)
            // использовать Set для определения того какие остались максимальные свободные "слоты"

            for (int i = _clientsQueue.Count - 1; i >= 0; i--)
            {
                var group = _clientsQueue[i];
                var table = this.Lookup(group);
                if (table != null)
                {
                    var tableInRest = _tables.Single((t) => t.Table == table);
                    tableInRest.SeatClientsGroup(group);

                    _clientsQueue.Remove(group);
                }

                // остановить "поиск места" для групп в очереди, которые больше либо равны той, для которой не нашли
                // стол на прошлом проходе цикла
            }
        }

        public Table Lookup(ClientsGroup group)
        {
            // 1. найти свободный стол (самый близкий по количеству человек)
            var freeTable = _tables.Where((t) => !t.IsOccupied)
                .Where((t) => t.AvailableChairs >= group.Size)
                .OrderBy((t) => t.AvailableChairs)
                .FirstOrDefault();
            if (freeTable != null)
            {
                return freeTable.Table;
            }

            // 2. поискать среди занятых столов
            var occupiedTable = _tables.Where((t) => t.IsOccupied)
                .Where((t) => t.AvailableChairs >= group.Size)
                .OrderBy((t) => t.AvailableChairs)
                .FirstOrDefault();
            if (occupiedTable != null)
            {
                return occupiedTable.Table;
            }

            return null;
        }

        public class TableInRest
        {
            public Table Table { get; private set; }
            private List<ClientsGroup> _seatedClientsGroups = new List<ClientsGroup>();

            public TableInRest(Table table)
            {
                this.Table = table;
            }
            
            // есть смысл "кешировать" доступное количество стульев, чтобы не суммировать группу каждый раз (при посадке и наоборотт)
            public int AvailableChairs => this.Table.Size - _seatedClientsGroups.Sum((g) => g.Size);
            public int TotalChairs => this.Table.Size;
            public IReadOnlyCollection<ClientsGroup> SeatedClientGroups => _seatedClientsGroups.AsReadOnly();
            public bool IsOccupied => _seatedClientsGroups.Any();

            // public bool TryToSeatClientsGroup(ClientsGroup group)
            public void SeatClientsGroup(ClientsGroup group)
            {
                // попробовать посадить группу за стол
                if (group.Size > this.AvailableChairs)
                {
                    throw new ArgumentOutOfRangeException(nameof(@group), group.Size, "Group does not fit in table");
                }

                _seatedClientsGroups.Add(group);
                
                // return false;
            }

            public void ReleaseChairs(ClientsGroup group)
            {
                // освободить стулья
                if (!_seatedClientsGroups.Remove(group))
                {
                    throw new ArgumentOutOfRangeException(nameof(@group), "Group was not at this table");
                }
            }
        } 
    }
}