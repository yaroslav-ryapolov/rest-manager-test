using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestManagerWeb.Models;

namespace RestManagerWeb.Controllers
{
    public class HomeController : Controller
    {
        private const string SessionKey = "restaurant";

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            RestaurantViewModel restaurant;
            string restaurantSerialized = HttpContext.Session.GetString(SessionKey);
            if (restaurantSerialized == null)
            {
                restaurant = new RestaurantViewModel();
                HttpContext.Session.SetString(SessionKey, JsonConvert.SerializeObject(restaurant));
            }
            else
            {
                restaurant = JsonConvert.DeserializeObject<RestaurantViewModel>(restaurantSerialized);
            }

            return View(restaurant);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult CreateTable(RestaurantViewModel restaurant)
        {
            // var restaurantInSession = JsonConvert.DeserializeObject<RestaurantViewModel>(
            //     HttpContext.Session.GetString(SessionKey));

            RestaurantViewModel restaurantInSession;
            string restaurantSerialized = HttpContext.Session.GetString(SessionKey);
            if (restaurantSerialized == null)
            {
                restaurantInSession = new RestaurantViewModel();
                HttpContext.Session.SetString(SessionKey, JsonConvert.SerializeObject(restaurantInSession));
            }
            else
            {
                restaurantInSession = JsonConvert.DeserializeObject<RestaurantViewModel>(restaurantSerialized);
            }

            restaurantInSession.Tables.Add(restaurant.NewTable);
            restaurantInSession.NewTable = new TableViewModel(Guid.NewGuid(), 1){Name = "Some another new table"};
            HttpContext.Session.SetString(SessionKey, JsonConvert.SerializeObject(restaurantInSession));

            return View("Index", restaurantInSession);
        }
    }
}
