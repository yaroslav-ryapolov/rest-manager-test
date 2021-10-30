using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using RestManagerLogic;

namespace RestManagerWeb.Models
{
    public record RestManagerRunnerVm
    {
        public Guid? Guid { get; set; }

        [BindRequired]
        public string Name { get; set; }

        public List<TableVm> Tables { get; set; } = new();

        public TableVm NewTable { get; set; } = new()
        {
            Guid = System.Guid.NewGuid(),
            Size = 2,
            Name = "New table"
        };

        public static RestManagerRunnerVm From(RestManagerRunner runner)
        {
            return new()
            {
                Guid = runner.Guid,
                Name = runner.Name,
                Tables = runner.Tables.Select(TableVm.From).ToList(),
            };
        }
    }
}
