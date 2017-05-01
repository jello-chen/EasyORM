using System;
using System.Collections.Generic;

namespace EasyORM.Utils
{
    /// <summary>
    /// Represents a dictionary cache of thread-safely
    /// </summary>
    public class ObjectDictionaryCache
    {
        static Dictionary<string, object> _caches = new Dictionary<string, object>();
        public static T GetObject<T>(string key, Func<T> func)
        {
            var value = (T)_caches.GetOrDefault(key);
            if (value == null)
            {
                lock (_caches)
                {
                    value = (T)_caches.GetOrDefault(key);
                    if (value == null)
                    {
                        value = func();
                        _caches.Add(key, value);
                    }
                }
            }
            return value;
        }
    }
}
