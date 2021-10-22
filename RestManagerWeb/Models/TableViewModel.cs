using System;
using System.Collections.Generic;

namespace RestManagerWeb.Models
{
    public class TableViewModel
    {
        public readonly Guid Guid;
        public readonly int Size;

        public readonly List<ClientsGroupViewModel> _seatedClientsGroups = new();

        public int AvailableChairs;
        public bool IsOccupied;

        public TableViewModel(Guid guid, int size)
        {
            Guid = guid;
            Size = size;
        }
    }
}
