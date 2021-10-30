using System;
using System.Collections.Generic;

namespace RestManagerWeb.Models
{
    public record TableVm
    {
        public Guid Guid { get; set; }
        public int Size { get; set; }
        public string Name { get; set; }

        public List<ClientsGroupVm> _seatedClientsGroups  { get; set; } = new();

        public int AvailableChairs;
        public bool IsOccupied;
    }
}
