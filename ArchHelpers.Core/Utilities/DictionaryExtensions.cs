using System;
using System.Collections.Generic;

namespace ArchHelpers.Core.Utilities
{
    public static class DictionaryExtensions
    {
        public static void CreateOrUpdateValue<TKey, TValue>(
            this Dictionary<TKey, TValue> dict,
            TKey key,
            Func<TValue> create,
            Func<TValue, TValue> update)
            where TKey : notnull
        {
            if (dict.TryGetValue(key, out var value))
            {
                dict[key] = update(value);
                return;
            }

            dict[key] = create();
        }
    }
}