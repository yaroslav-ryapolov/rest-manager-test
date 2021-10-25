using System;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using RestManagerWeb.Models;

namespace RestManagerWeb.Helpers
{
    public static class SessionExtensions
    {
        private const string RestaurantKey = "restaurant";

        public static RestaurantViewModel GetRestaurant(this ISession session)
        {
            return session.GetValue<RestaurantViewModel>(RestaurantKey, new RestaurantViewModel());
        }

        public static void UpdateRestaurant(this ISession session, Action<RestaurantViewModel> updater)
        {
            session.UpdateValue(RestaurantKey, updater);
        }

        private static T GetValue<T>(this ISession session, string key, T defaultValue = default)
        {
            var serializedValue = session.GetString(key);
            if (serializedValue == null)
            {
                Console.WriteLine($"There was no value with key {key} in the Session cache. Using DEFAULT");

                session.SetString(key, JsonConvert.SerializeObject(defaultValue));
                return defaultValue;
            }

            Console.WriteLine($"Using value from the Session cache for key {key}");
            return JsonConvert.DeserializeObject<T>(serializedValue);
        }

        private static void UpdateValue<T>(this ISession session, string key, Action<T> updater)
        {
            var value = session.GetValue<T>(key);
            updater(value);

            session.SetString(key, JsonConvert.SerializeObject(value));
        }
    }
}
