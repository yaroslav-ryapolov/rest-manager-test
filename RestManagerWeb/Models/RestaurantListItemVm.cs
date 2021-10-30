using System;

namespace RestManagerWeb.Models
{
    public class RestaurantListItemVm
    {
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public int TablesCount { get; set; }
    }
}
