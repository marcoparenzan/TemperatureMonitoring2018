using Microsoft.Azure.Devices.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebManagement.Models
{
    public static class TwinExtension
    {
        public static T Desired<T>(this Twin xx, string key)
        {
            try
            {
                return (T)xx.Properties.Desired[key];
            }
            catch
            {
                return default(T);
            }
        }

        public static T Reported<T>(this Twin xx, string key)
        {
            try
            {
                return (T)xx.Properties.Reported[key];
            }
            catch
            {
                return default(T);
            }
        }

        public static string Tag(this Twin xx, string key)
        {
            try
            {
                var value = (string) xx.Tags[key];
                return value.ToString();
            }
            catch
            {
                return default(string);
            }
        }

        public static T Tag<T>(this Twin xx, string key)
        {
            try
            {
                var value = (TwinCollection) xx.Tags[key];
                return (T) Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return default(T);
            }
        }
    }
}
