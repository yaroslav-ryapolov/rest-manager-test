using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RestManagerLogic;
using RestManagerWeb.Helpers;
using RestManagerWeb.Models;

namespace RestManagerWeb.Controllers
{
    public class RestaurantRunnerController : Controller
    {
        private static readonly ConcurrentDictionary<Guid, RestManagerRunner> _restManagerRunners = new();

        private readonly ILogger<RestaurantRunnerController> _logger;

        public RestaurantRunnerController(ILogger<RestaurantRunnerController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(RestManagerRunnerListItemVm.From(_restManagerRunners.Values));
        }

        public IActionResult New()
        {
            return View("Details", new RestManagerRunnerVm());
        }

        public IActionResult Details(Guid id)
        {
            var runner = _restManagerRunners[id];
            if (runner == null)
            {
                return NotFound();
            }

            return View(RestManagerRunnerVm.From(runner));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddOrUpdate(RestManagerRunnerVm model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (model.Guid == null && _restManagerRunners.Any((r) => r.Value.Name == model.Name))
            {
                return BadRequest("Restaurant with such name already stored in memory");
            }

            RestManagerRunner runner;
            if (model.Guid == null)
            {
                runner = new RestManagerRunner(model.Name, tables => new RestManagerSimple(tables));
                _restManagerRunners[runner.Guid] = runner;
            }
            else
            {
                runner = _restManagerRunners[model.Guid.Value];
            }

            if (model.NewTable != null)
            {
                runner.AddTable(new Table(model.NewTable.Guid, model.NewTable.Size) { Name = model.NewTable.Name });
            }
            return RedirectToAction("Details", new {id = runner.Guid});
        }
    }
}
