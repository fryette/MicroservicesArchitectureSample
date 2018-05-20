using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ShoppingCart
{
    public class Cache : ICache
    {
        private static readonly IDictionary<string, Tuple<DateTimeOffset, object>> InternalCache = new ConcurrentDictionary<string, Tuple<DateTimeOffset, object>>();

        public void Add(string key, object value, TimeSpan ttl)
        {
            InternalCache[key] = Tuple.Create(DateTimeOffset.UtcNow.Add(ttl), value);
        }

        public object Get(string productsResource)
        {
            if (InternalCache.TryGetValue(productsResource, out var value) && value.Item1 > DateTimeOffset.UtcNow)
            {
                return value;
            }

            InternalCache.Remove(productsResource);
            return null;
        }
    }
}