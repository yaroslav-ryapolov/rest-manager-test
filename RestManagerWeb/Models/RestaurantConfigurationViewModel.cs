using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace RestManagerWeb.Models
{
    public record RestaurantConfigurationViewModel
    {
        public bool IsInitialized { get; set; }

        [BindRequired]
        public string ConfigurationName { get; set; }

        public List<TableViewModel> Tables { get; set; } = new();

        public TableViewModel NewTable { get; set; } = new() { Guid = Guid.NewGuid(), Size = 1, Name = "Some new table" };
    }
}
