using System;
using System.Collections.Generic;
using System.Linq;
using RestManagerLogic;

namespace RestManagerWeb.Models
{
    public class RestManagerRunnerListItemVm
    {
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public int TablesCount { get; set; }

        public static RestManagerRunnerListItemVm From(RestManagerRunner runner)
        {
            return new RestManagerRunnerListItemVm
            {
                Guid = runner.Guid,
                Name = runner.Name,
                TablesCount = runner.Tables.Count(),
            };
        }

        public static IEnumerable<RestManagerRunnerListItemVm> From(IEnumerable<RestManagerRunner> runners)
        {
            return runners.Select(From);
        }
    }
}
