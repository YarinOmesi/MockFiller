using System.Collections.Generic;

namespace TestsHelper.SourceGenerator.Tests;

public static class DictionaryHelper
{
    public static Dictionary<TKey, TValue> MergeRight<TKey, TValue>(params Dictionary<TKey, TValue>[] dictionaries) 
        where TKey : notnull
    {
        Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

        foreach (Dictionary<TKey,TValue> d in dictionaries)
        {
            foreach ((TKey key, TValue value) in d)
            {
                dictionary[key] = value;
            }
        }
        return dictionary;
    }
}