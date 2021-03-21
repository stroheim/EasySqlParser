using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EasySqlParser.SqlGenerator
{
    /// <summary>
    /// code base
    /// https://github.com/neuecc/MessagePack-CSharp/blob/master/src/MessagePack.UnityClient/Assets/Scripts/MessagePack/Internal/ThreadsafeTypeKeyHashTable.cs
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    internal class TypeHashDictionary<TValue>
    {
        private Entry[] _buckets;
        private int _size;
        private readonly float _loadFactor;

        internal static TypeHashDictionary<TValue> Create(KeyValuePair<Type, TValue>[] values)
        {
            var result = new TypeHashDictionary<TValue>(values.Length);
            foreach (var pair in values)
            {
                if (!result.TryAdd(pair.Key, pair.Value))
                {
                    // TODO:
                    throw new ArgumentException("Key was already exists");
                }
            }

            return result;

        }


        private TypeHashDictionary(int capacity = 4, float loadFactor = 0.75f)
        {
            var tableSize = CalculateCapacity(capacity, loadFactor);
            _buckets = new Entry[tableSize];
            _loadFactor = loadFactor;

        }

        public bool TryAdd(Type key, TValue value)
        {
            return TryAdd(key, _ => value); // create lambda capture
        }

        public bool TryAdd(Type key, Func<Type, TValue> valueFactory)
        {
            return TryAddInternal(key, valueFactory, out _);
        }

        private bool TryAddInternal(Type key, Func<Type, TValue> valueFactory, out TValue resultingValue)
        {
            var nextCapacity = CalculateCapacity(_size + 1, _loadFactor);
            if (_buckets.Length < nextCapacity)
            {
                // rehash
                var nextBucket = new Entry[nextCapacity];
                for (var i = 0; i < _buckets.Length; i++)
                {
                    var e = _buckets[i];
                    while (e != null)
                    {
                        var newEntry = new Entry { Key = e.Key, Value = e.Value, Hash = e.Hash };
                        AddToBuckets(nextBucket, key, newEntry, null, out resultingValue);
                        e = e.Next;
                    }
                }

                var successAdd = AddToBuckets(nextBucket, key, null, valueFactory, out resultingValue);
                _buckets = nextBucket;

                if (successAdd)
                {
                    _size++;
                }

                return successAdd;
            }
            else
            {
                // add entry(insert last is thread safe for read)
                var successAdd = AddToBuckets(_buckets, key, null, valueFactory, out resultingValue);
                if (successAdd)
                {
                    _size++;
                }

                return successAdd;
            }

        }

        private static bool AddToBuckets(Entry[] buckets, Type newKey, Entry newEntry, Func<Type, TValue> valueFactory,
            out TValue resultingValue)
        {
            var hash = newEntry?.Hash ?? newKey.GetHashCode();
            var index = hash & (buckets.Length - 1);
            if (buckets[index] == null)
            {
                if (newEntry == null)
                {
                    resultingValue = valueFactory(newKey);
                    buckets[index] = new Entry { Key = newKey, Value = resultingValue, Hash = hash };
                }
                else
                {
                    resultingValue = newEntry.Value;
                    buckets[index] = newEntry;
                }
            }
            else
            {
                var lastEntry = buckets[index];
                while (true)
                {
                    if (lastEntry.Key == newKey)
                    {
                        resultingValue = lastEntry.Value;
                        return false;
                    }

                    if (lastEntry.Next == null)
                    {
                        if (newEntry == null)
                        {
                            resultingValue = valueFactory(newKey);
                            lastEntry.Next = new Entry { Key = newKey, Value = resultingValue, Hash = hash };
                        }
                        else
                        {
                            resultingValue = newEntry.Value;
                            lastEntry.Next = newEntry;
                        }
                        break;
                    }

                    lastEntry = lastEntry.Next;
                }
            }

            return true;
        }

        public TValue Get(Type key)
        {
            if (TryGetValue(key, out var value))
            {
                return value;
            }
            // error
            throw new KeyNotFoundException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(Type key, out TValue value)
        {
            var table = _buckets;
            var hash = key.GetHashCode();
            var entry = table[hash & table.Length - 1];

            while (entry != null)
            {
                if (entry.Key == key)
                {
                    value = entry.Value;
                    return true;
                }

                entry = entry.Next;
            }

            value = default;
            return false;
        }

        //public TValue GetOrAdd(Type key, Func<Type, TValue> valueFactory)
        //{
        //    if (TryGetValue(key, out var v))
        //    {
        //        return v;
        //    }

        //    TryAddInternal(key, valueFactory, out v);
        //    return v;
        //}


        private static int CalculateCapacity(int collectionSize, float loadFactor)
        {
            var initialCapacity = (int)(collectionSize / loadFactor);
            var capacity = 1;
            while (capacity < initialCapacity)
            {
                capacity <<= 1;
            }

            if (capacity < 8)
            {
                return 8;
            }

            return capacity;
        }

        internal int Count => _size;

        private class Entry
        {
            public Type Key;
            public TValue Value;
            public int Hash;

            public Entry Next;

        }
    }
}
