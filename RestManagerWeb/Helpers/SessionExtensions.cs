using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using RestManagerWeb.Models;

namespace RestManagerWeb.Helpers
{
    public static class SessionExtensions
    {
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
