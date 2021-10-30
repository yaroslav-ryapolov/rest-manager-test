using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace RestManagerWeb.Models
{
    public record RestaurantConfigurationVm
    {
        public Guid? Guid { get; set; }

        [BindRequired]
        public string Name { get; set; }

        public List<TableVm> Tables { get; set; } = new();

        public TableVm NewTable { get; set; } = new()
        {
            Guid = System.Guid.NewGuid(),
            Size = 1,
            Name = "Some new table"
        };
    }
}
