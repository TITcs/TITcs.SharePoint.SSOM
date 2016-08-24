using System;
using TITcs.SharePoint.SSOM.Caching;

namespace TITcs.SharePoint.SSOM.Utils
{
    public class CacheUtils
    {
        public static T CacheResult<T>(Func<T> method, Type cacheKey, string cacheSubKey, params object[] args)
        {
            string key = cacheKey.ToString();
            string subKey = string.Format(cacheSubKey, args);

            if (!Cache.Contains(key, subKey))
            {
                T result = MethodUtils.Call(method);

                if (result == null)
                    return default(T);

                Cache.Insert(key, subKey, result);

                return result;
            }

            return Cache.Get<T>(key, subKey);
        }

        public static void InvalidateCache(Type cacheKey)
        {
            InvalidateCache(cacheKey, null);
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
