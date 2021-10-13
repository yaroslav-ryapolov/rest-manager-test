using System.Collections.Generic;
using System.Linq;

namespace RestManagerLogic
{
    public class RestManager
    {
        private List<TableInRest> _tables;
        private List<ClientsGroup> _clientsQueue = new List<ClientsGroup>();

        private Dictionary<int, List<TableInRest>> _freeTablesBySeats;
        private Dictionary<int, List<TableInRest>> _occupiedTablesBySeats = new Dictionary<int, List<TableInRest>>();

        public RestManager(List<Table> tables)
        {
            _tables = tables.Select((t) => new TableInRest(t))
                .ToList();

            _freeTablesBySeats = _tables.GroupBy((t) => t.AvailableChairs)
                .ToDictionary((t) => t.Key, (x) => x.ToList());
        }

        public void OnArrive(ClientsGroup group)
        {
            // todo: чуток ускорить нужно, чтобы не двигать всех инсертом
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
            if (tableWithGroup.IsOccupied)
            {
                
            }
            
            this.TryToSeatSomebodyFromQueue();
        }

        private TableInRest GetTableByGroup(ClientsGroup group)
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

            for (int i = _clientsQueue.Count - 1; i > 0; i--)
            {
                
            }
        }

        public Table Lookup(ClientsGroup group)
        {
            return null;
        }

        private class TableInRest
        {
            public Table Table { get; private set; }
            private List<ClientsGroup> _seatedClientsGroups = new List<ClientsGroup>();

            public TableInRest(Table table)
            {
                this.Table = table;
            }
            
            public int AvailableChairs => 0;
            public int TotalChairs => this.Table.Size;
            public IReadOnlyCollection<ClientsGroup> SeatedClientGroups => _seatedClientsGroups.AsReadOnly();
            public bool IsOccupied => _seatedClientsGroups.Any();

            public bool TryToSeatClientsGroup(ClientsGroup group)
            {
                // попробовать посадить группу за стол
                
                return false;
            }

            public void ReleaseChairs(ClientsGroup group)
            {
                // освободить стулья
            }
        } 
    }
}