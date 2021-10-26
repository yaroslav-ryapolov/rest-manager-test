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
    public class RestaurantConfigurationController : Controller
    {
        private static ConcurrentDictionary<string, IRestManager> _restManagers = new();

        private readonly ILogger<RestaurantConfigurationController> _logger;

        public RestaurantConfigurationController(ILogger<RestaurantConfigurationController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var restaurantConfigurations = _restManagers
                .Select((v) =>
                    new RestaurantListItemViewModel
                    {
                        ConfigurationName = v.Key,
                        TablesCount = v.Value.Tables.Count(),
                    })
                .AsEnumerable();

            return View(restaurantConfigurations);
        }

        public IActionResult New()
        {
            return View("Details", new RestaurantConfigurationViewModel());
        }

        public IActionResult Details(string id)
        {
            var restManager = _restManagers[id];
            if (restManager == null)
            {
                return NotFound();
            }

            RestaurantConfigurationViewModel restaurant = new()
            {
                IsInitialized = true,
                ConfigurationName = id,
                Tables = restManager.Tables.Select((t) =>
                    new TableViewModel() {
                        Guid = t.Guid,
                        Name = "",
                        Size = t.Size,
                    })
                    .ToList()
            };
            return View(restaurant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddOrUpdate(RestaurantConfigurationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!model.IsInitialized && HttpContext.Session.ContainsRestaurant(model.ConfigurationName))
            {
                return BadRequest("Restaurant with such name already stored in memory");
            }

            IRestManager restManager = new RestManagerSimple(new List<Table>{ new Table(6), new Table(6) });
            if (model.IsInitialized)
            {
                restManager = _restManagers[model.ConfigurationName];
            }
            else
            {
                _restManagers[model.ConfigurationName] = restManager;
            }

            // HttpContext.Session.UpdateRestaurant(model.ConfigurationName, (r) =>
            // {
            //     r.IsInitialized = true;
            //     r.ConfigurationName = model.ConfigurationName;
            //     r.Tables.Add(model.NewTable);
            //     r.NewTable = new TableViewModel{ Guid = Guid.NewGuid(), Size = 1, Name = "Some another new table"};
            // });
            return RedirectToAction("Details", new {id = model.ConfigurationName});
        }
    }
}
