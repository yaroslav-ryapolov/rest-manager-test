using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RestManagerWeb.Helpers;
using RestManagerWeb.Models;

namespace RestManagerWeb.Controllers
{
    public class RestaurantConfigurationController : Controller
    {
        private readonly ILogger<RestaurantConfigurationController> _logger;

        public RestaurantConfigurationController(ILogger<RestaurantConfigurationController> logger)
        {
            _logger = logger;
        }

        public IActionResult New()
        {
            return View("Details", new RestaurantConfigurationViewModel());
        }

        public IActionResult Details(string id)
        {
            RestaurantConfigurationViewModel restaurant = HttpContext.Session.GetRestaurant(id);
            return View(restaurant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(RestaurantConfigurationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!model.IsInitialized && HttpContext.Session.ContainsRestaurant(model.ConfigurationName))
            {
                return BadRequest("Restaurant with such name already stored in session");
            }

            HttpContext.Session.UpdateRestaurant(model.ConfigurationName, (r) =>
            {
                r.IsInitialized = true;
                r.ConfigurationName = model.ConfigurationName;
                r.Tables.Add(model.NewTable);
                r.NewTable = new TableViewModel{ Guid = Guid.NewGuid(), Size = 1, Name = "Some another new table"};
            });
            return RedirectToAction("Details", new {id = model.ConfigurationName});
        }
    }
}
