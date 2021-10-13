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

        public List<Table> Tables => _tables;
        public List<ClientsGroup> ClientsQueue => _clientsQueue;

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

            var tableWithGroup = this.Lookup(group);
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
        
        public Table Lookup(ClientsGroup group)
        {
            // нужно причесать чуток и оптимизировать (хешиком)
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