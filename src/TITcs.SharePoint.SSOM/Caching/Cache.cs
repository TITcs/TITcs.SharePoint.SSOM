using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using TITcs.SharePoint.SSOM.Utils;

namespace TITcs.SharePoint.SSOM.Caching
{
    public class Cache
    {
        private const string SEPARATOR = "$";
        private static readonly int _cacheDurationInMinutes;

        static Cache()
        {
            if (_cacheDurationInMinutes == 0)
            {
                _cacheDurationInMinutes = AppSettingsUtils.CacheDurationInMinutes;
            }
        }

        public static void Insert<T>(string key, string subKey, T value)
        {

            lock (MemoryCache.Default)
            {
                MemoryCache.Default.Remove(FormatKey(key, subKey));
                var expires = new DateTimeOffset(DateTime.Now.AddMinutes(_cacheDurationInMinutes));
                MemoryCache.Default.Add(FormatKey(key, subKey), value, expires);
            }
        }

        public static T Get<T>(string key, string subKey)
        {
            if (MemoryCache.Default.Contains(FormatKey(key, subKey)))
            {
                return (T)MemoryCache.Default.Get(FormatKey(key, subKey));
            }
            else
            {
                return default(T);
            }
        }

        private static string FormatKey(string key, string subKey)
        {
            return string.Format("{0}{1}{2}", key, SEPARATOR, subKey);
        }

        public static bool Contains(string key, string subKey)
        {
            return MemoryCache.Default.Contains(FormatKey(key, subKey));
        }

        public static void Remove(string key)
        {
            List<string> cacheKeys = MemoryCache.Default.Select(kvp => kvp.Key).ToList();
            foreach (string cacheKey in cacheKeys)
            {
                if (cacheKey.IndexOf(string.Format("{0}{1}", key, SEPARATOR)) == 0)
                    MemoryCache.Default.Remove(cacheKey);
            }

            MemoryCache.Default.Remove(key);
        }

        public static void Remove(string key, string subKey)
        {
            MemoryCache.Default.Remove(FormatKey(key, subKey));
        }

        public static void InvalidateCache(params string[] cacheKeys)
        {
            foreach (var cacheKey in cacheKeys)
            {
                InvalidateCache(cacheKey, null);
            }


        }

        public static void InvalidateCache(Type cacheKey, string cacheSubKey, params object[] args)
        {
            string key = cacheKey.ToString();

            if (cacheSubKey == null)
                Cache.Remove(key);
            else
                Cache.Remove(key, string.Format(cacheSubKey, args));
        }
    }
}
