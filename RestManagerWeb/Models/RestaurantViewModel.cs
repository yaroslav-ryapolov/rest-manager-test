using System;
using System.Collections.Generic;

namespace RestManagerWeb.Models
{
    public class RestaurantViewModel
    {
        public readonly List<TableViewModel> Tables = new();

        public TableViewModel NewTable = new TableViewModel(Guid.NewGuid(), 1) {Name = "Some new table"};
    }
}
