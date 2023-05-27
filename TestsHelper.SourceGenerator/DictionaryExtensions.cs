using System;
using System.Collections.Generic;

namespace TestsHelper.SourceGenerator;

public static class DictionaryExtensions
{
    public static void AddKeysIfNotExists<TKey, TValue, TInput>(
        this Dictionary<TKey, TValue> me,
        IEnumerable<TInput> inputs,
        Func<TInput, TKey> keySelector,
        Func<TInput, TValue> valueSelector
    )
    {
        foreach (TInput input in inputs)
        {
            TKey key = keySelector(input);
            if (!me.ContainsKey(key))
            {
                me[key] = valueSelector(input);
            }
        }
    }
}