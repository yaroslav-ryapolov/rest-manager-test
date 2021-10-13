using System;
using System.Collections.Generic;
using System.Linq;

namespace RestManagerLogic
{
    public class RestManager
    {
        private readonly List<Table> _tables;
        private readonly List<ClientsGroup> _clientsQueue = new List<ClientsGroup>();

        private readonly List<LinkedList<Table>> _tablesByAvailableChairs;

        public IReadOnlyCollection<Table> Tables => _tables.AsReadOnly();
        public IReadOnlyCollection<ClientsGroup> ClientsQueue => _clientsQueue;

        public RestManager(List<Table> tables)
        {
            // doing the copy to avoid effects from external list changes
            _tables = tables.ToList();

            // todo: вынести в отдельный внутренний класс, где всё чуть более явно обозвать
            var maxTableSize = _tables.Max((t) => t.Size);
            _tablesByAvailableChairs = new List<LinkedList<Table>>(maxTableSize + 1);
            for (int i = 0; i < _tablesByAvailableChairs.Capacity; i++)
            {
                _tablesByAvailableChairs.Add(new LinkedList<Table>());
            }

            foreach (var table in _tables)
            {
                _tablesByAvailableChairs[table.AvailableChairs].AddLast(table);
            }
        }

        public void OnArrive(ClientsGroup group)
        {
            // TO LOCK Queue
            
            // todo: чуток ускорить нужно, чтобы не двигать всех а снова использовать что-то вроде LinkedList
            _clientsQueue.Insert(0, group);
            
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

            // todo: receive LinkedList and use it to remove faster
            _tablesByAvailableChairs[table.AvailableChairs].Remove(table);
            _tablesByAvailableChairs[table.AvailableChairs + group.Size].AddLast(table);
            
            table.ReleaseChairs(group);

            this.TryToSeatSomebodyFromQueue();
        }
        
        public Table Lookup(ClientsGroup group)
        {
            // TO LOCK Tables Matrix
            
            // нужно причесать чуток и оптимизировать (хешиком, где хранится линкед лист на стол, который также обновляется в случае необходимости)
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
                if (this.SeatClientsGroup(group))
                {
                    _clientsQueue.Remove(group);
                }

                // остановить "поиск места" для групп в очереди, которые больше либо равны той, для которой не нашли
                // стол на прошлом проходе цикла
            }
        }

        public bool SeatClientsGroup(ClientsGroup group)
        {
            // TO LOCK Tables Matrix
            
            var tablesWithEnoughRoom = _tablesByAvailableChairs[group.Size];
            int i = group.Size + 1;
            while (!tablesWithEnoughRoom.Any() && i < _tablesByAvailableChairs.Count)
            {
                tablesWithEnoughRoom = _tablesByAvailableChairs[i];
            }

            var table = tablesWithEnoughRoom.First?.Value;
            if (table == null)
            {
                return false;
            }

            // move table to new list
            tablesWithEnoughRoom.RemoveFirst();
            _tablesByAvailableChairs[table.AvailableChairs - group.Size].AddLast(table);

            table.SeatClientsGroup(group);
            return true;
        }
    }
}