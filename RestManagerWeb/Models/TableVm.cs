using System;
using System.Collections.Generic;
using System.Linq;
using RestManagerLogic;

namespace RestManagerWeb.Models
{
    public record TableVm
    {
        public Guid Guid { get; set; }
        public int Size { get; set; }
        public string Name { get; set; }

        public IEnumerable<ClientsGroupVm> SeatedClientsGroups  { get; set; }

        public int AvailableChairs;
        public bool IsOccupied;

        public static TableVm From(Table table)
        {
            return new TableVm
            {
                Guid = table.Guid,
                Name = table.Name,
                Size = table.Size,

                SeatedClientsGroups = table.SeatedClientGroups.Select(ClientsGroupVm.From),

                AvailableChairs = table.AvailableChairs,
                IsOccupied = table.IsOccupied,
            };
        }

        public static void To(TableVm source, Table dest)
        {
            dest.Name = source.Name;
        }
    }
}
