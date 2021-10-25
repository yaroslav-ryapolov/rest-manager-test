using System;
using System.Collections.Generic;

namespace RestManagerWeb.Models
{
    public record RestaurantViewModel
    {
        public string Test { get; set; } = "TEST";

        public List<TableViewModel> Tables { get; set; } = new();

        public TableViewModel NewTable { get; set; } = new() { Guid = Guid.NewGuid(), Size = 1, Name = "Some new table" };
    }
}
