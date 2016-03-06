using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyORM.Utils
{
    public static class IEnumberableExtension
    {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                var tmp = item;
                action(tmp);
                yield return item;
            }
        }

        public static T FirstOrDefault<T>(this NameObjectCollectionBase collection, Func<T, bool> filter)
        {
            foreach (T item in collection)
            {
               if(filter(item))
               {
                   return item;
               }
            }
            return default(T);
        }
    }
}
