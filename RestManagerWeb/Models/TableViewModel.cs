using System;
using System.Collections.Generic;

namespace RestManagerWeb.Models
{
    public class TableViewModel
    {
        public Guid Guid;
        public int Size;
        public string Name;

        public readonly List<ClientsGroupViewModel> _seatedClientsGroups = new();

        public int AvailableChairs;
        public bool IsOccupied;

        public TableViewModel()
        {
        }

        public TableViewModel(Guid guid, int size)
        {
            Guid = guid;
            Size = size;
        }
    }
}
