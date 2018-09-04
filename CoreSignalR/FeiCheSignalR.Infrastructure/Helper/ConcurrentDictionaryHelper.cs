using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace FeiCheSignalR.Infrastructure.Helper
{
    public static class ConcurrentDictionaryHelper
    {
        public static bool TryRemove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> self, TKey key)
        {
            TValue ignored;
            return self.TryRemove(key, out ignored);
        }
    }
}
