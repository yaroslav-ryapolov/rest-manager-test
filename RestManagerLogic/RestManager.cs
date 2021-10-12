using System.Collections.Generic;
using System.Linq;

namespace RestManagerLogic
{
    public class RestManager
    {
        private List<Table> _tables;
        private Queue<ClientsGroup> _clientsQueue = new Queue<ClientsGroup>();

        public RestManager(List<Table> tables)
        {
            _tables = tables.ToList();
        }

        public void OnArrive(ClientsGroup group)
        {
            _clientsQueue.Enqueue(group);
        }

        public void OnLeave(ClientsGroup group)
        {
            
        }

        public Table Lookup(ClientsGroup group)
        {
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
            
            public int AvailableChairs => 0;
            public int TotalChairs => this.Table.Size;
            public IReadOnlyCollection<ClientsGroup> SeatedClientGroups => this._seatedClientsGroups.AsReadOnly();

            public bool TryToSeatClientsGroup(ClientsGroup group)
            {
                return false;
            }

            public void LeaveClientsGroup()
            {
                
            }
        } 
    }
}