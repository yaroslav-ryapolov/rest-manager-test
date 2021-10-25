using System;
using System.Collections.Generic;

namespace RestManagerWeb.Models
{
    public record TableViewModel
    {
        public Guid Guid { get; set; }
        public int Size { get; set; }
        public string Name { get; set; }

        public List<ClientsGroupViewModel> _seatedClientsGroups  { get; set; } = new();

        public int AvailableChairs;
        public bool IsOccupied;
    }
}
