using System;
using System.Collections.Generic;

namespace RestManagerLogic
{
    public class RestManagerRunner
    {
        private readonly Func<List<Table>, IRestManager> _createRestManagerFunc;
        private List<Table> _tables = new();
        private IRestManager _activeRestManager;

        public Guid Guid { get; }
        public string Name { get; }

        public RestManagerRunner(string name, Func<List<Table>, IRestManager> createRestManagerFunc)
        {
            Guid = Guid.NewGuid();
            Name = name;
            _createRestManagerFunc = createRestManagerFunc;
        }

        public IEnumerable<Table> Tables => _tables;

        public IRestManager ActiveRestManager
        {
            get
            {
                if (_activeRestManager == null)
                {
                    throw new InvalidOperationException("Cannot access to non-started Rest Manager");
                }

                return _activeRestManager;
            }
        }

        public void StartSimulation()
        {
            if (_activeRestManager != null)
            {
                throw new InvalidOperationException("Simulation is already active");
            }

            _activeRestManager = _createRestManagerFunc(_tables);
        }

        public void StopSimulation()
        {
            _activeRestManager = null;
        }

        public void AddTable(int size)
        {
            AddTable(new Table(size));
        }

        public void AddTable(Table table)
        {
            if (_tables.Contains(table))
            {
                throw new InvalidOperationException("Cannot add same Table twice");
            }

            _tables.Add(table);
        }

        public void RemoveTable(Guid guid)
        {
            var tableToRemove = _tables.Find((t) => t.Guid == guid);
            RemoveTable(tableToRemove);
        }

        public void RemoveTable(Table table)
        {
            _tables.Remove(table);
        }
    }
}
