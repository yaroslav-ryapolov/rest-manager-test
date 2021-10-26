using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using RestManagerWeb.Models;

namespace RestManagerWeb.Helpers
{
    public static class SessionExtensions
    {
        private const string RestaurantKeyPrefix = "restaurant_";

        public static RestaurantConfigurationViewModel GetRestaurant(this ISession session, string name)
        {
            return session.GetValue<RestaurantConfigurationViewModel>($"{RestaurantKeyPrefix}_{name}");
        }

        public static void UpdateRestaurant(this ISession session, string name,
            Action<RestaurantConfigurationViewModel> updater)
        {
            session.UpdateValue($"{RestaurantKeyPrefix}_{name}", updater, new RestaurantConfigurationViewModel());
        }

        public static bool ContainsRestaurant(this ISession session, string name)
        {
            return session.Keys.Contains($"{RestaurantKeyPrefix}_{name}");
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

        private static void UpdateValue<T>(this ISession session, string key, Action<T> updater,
            T defaultValue = default(T))
        {
            var value = session.GetValue<T>(key, defaultValue);
            updater(value);

            session.SetString(key, JsonConvert.SerializeObject(value));
        }
    }
}
