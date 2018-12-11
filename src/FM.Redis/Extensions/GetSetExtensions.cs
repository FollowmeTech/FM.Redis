using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace FM.Redis
{
    public static class IDataBaseExtensions
    {
        /// <summary>
        /// Sets the specified key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="database">The database.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="expiry">The expiry.</param>
        /// <param name="when">The when.</param>
        /// <param name="flags">The flags.</param>
        /// <returns></returns>
        public static bool Set<T>(this IDatabase database, string key, T value, TimeSpan? expiry = default(TimeSpan?), When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            return database.StringSet(key,
                 JsonConvert.SerializeObject(value), expiry, when, flags);
        }

        /// <summary>
        /// Sets the asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="database">The database.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="expiry">The expiry.</param>
        /// <param name="when">The when.</param>
        /// <param name="flags">The flags.</param>
        /// <returns></returns>
        public static Task<bool> SetAsync<T>(this IDatabase database, string key, T value, TimeSpan? expiry = default(TimeSpan?), When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            return database.StringSetAsync(key,
                 JsonConvert.SerializeObject(value), expiry, when, flags);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="database"></param>
        /// <param name="key"></param>
        /// <param name="expiry"></param>
        /// <param name="when"></param>
        /// <param name="flags"></param>
        /// <param name="acquire"></param>
        /// <returns></returns>
        public static T Get<T>(this IDatabase database, string key, TimeSpan? expiry = default(TimeSpan?), When when = When.Always, CommandFlags flags = CommandFlags.None, Func<T> acquire = null)
        {
            var result = database.StringGet(key, flags);
            if (result.HasValue)
            {
                return JsonConvert.DeserializeObject<T>(result);
            }
            if (acquire == null)
            {
                return default(T);
            }
            var acquireResult = acquire();
            database.Set<T>(key, acquireResult, expiry,when,flags);
            return acquireResult;
        }
        public static async Task<T> GetAsync<T>(this IDatabase database, string key,
            TimeSpan? expiry = default(TimeSpan?), When when = When.Always, CommandFlags flags = CommandFlags.None,
            Func<Task<T>> acquire = null)
        {
            var result = await database.StringGetAsync(key, flags);
            if (result.HasValue)
            {
                return JsonConvert.DeserializeObject<T>(result);
            }

            if (acquire == null)
            {
                return default(T);
            }

            var acquireResult = await acquire();
            await database.SetAsync<T>(key, acquireResult, expiry, when, flags);
            return acquireResult;
        }

    }
}
