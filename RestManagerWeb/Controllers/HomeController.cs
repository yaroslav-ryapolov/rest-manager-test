using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RestManagerWeb.Helpers;
using RestManagerWeb.Models;

namespace RestManagerWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            RestaurantViewModel restaurant = HttpContext.Session.GetRestaurant();

            return View(restaurant);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateTable(RestaurantViewModel model)
        {
            if (ModelState.IsValid)
            {
                HttpContext.Session.UpdateRestaurant((r) =>
                {
                    r.Test = model.Test;
                    r.Tables.Add(model.NewTable);
                    r.NewTable = new TableViewModel{ Guid = Guid.NewGuid(), Size = 1, Name = "Some another new table"};
                });
                return RedirectToAction("Index");
            }

            return BadRequest(ModelState);
        }
    }
}
