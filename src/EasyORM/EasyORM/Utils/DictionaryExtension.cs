using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyORM.Utils
{
    public static class DictionaryExtension
    {
        /// <summary>
        /// 获取指定Key对应的Value，若未找到将获取默认值
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
        {
            TValue value = default(TValue);
            dict.TryGetValue(key, out value);
            return value;
        }

        /// <summary>
        /// 获取指定Key对应的Value，若未找到将抛异常
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
        {
            TValue value = default(TValue);
            if (!dict.TryGetValue(key, out value))
            {
                throw new KeyNotFoundException("没有找到key:" + key.ToString());
            }
            return value;
        }

        /// <summary>
        /// 获取指定Key对应的Value，若未找到将使用指定的委托增加值
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="setValue"></param>
        /// <returns></returns>
        public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue> setValue)
        {
            TValue value = default(TValue);
            if (!dict.TryGetValue(key, out value))
            {
                value = setValue(key);
                dict.Add(key, value);
            }
            return value;
        }
    }
}
