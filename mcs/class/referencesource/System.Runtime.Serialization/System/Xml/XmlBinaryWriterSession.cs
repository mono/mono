//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.Xml
{
    using System;
    using System.Xml;
    using System.Collections;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class XmlBinaryWriterSession
    {
        PriorityDictionary<string, int> strings;
        PriorityDictionary<IXmlDictionary, IntArray> maps;
        int nextKey;

        public XmlBinaryWriterSession()
        {
            this.nextKey = 0;
            this.maps = new PriorityDictionary<IXmlDictionary, IntArray>();
            this.strings = new PriorityDictionary<string, int>();
        }

        public virtual bool TryAdd(XmlDictionaryString value, out int key)
        {
            IntArray keys;
            if (value == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");

            if (maps.TryGetValue(value.Dictionary, out keys))
            {
                key = (keys[value.Key] - 1);

                if (key != -1)
                {
                    // If the key is already set, then something is wrong
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlKeyAlreadyExists)));
                }

                key = Add(value.Value);
                keys[value.Key] = (key + 1);
                return true;
            }

            key = Add(value.Value);
            keys = AddKeys(value.Dictionary, value.Key + 1);
            keys[value.Key] = (key + 1);
            return true;
        }

        int Add(string s)
        {
            int key = this.nextKey++;
            strings.Add(s, key);
            return key;
        }

        IntArray AddKeys(IXmlDictionary dictionary, int minCount)
        {
            IntArray keys = new IntArray(Math.Max(minCount, 16));
            maps.Add(dictionary, keys);
            return keys;
        }

        public void Reset()
        {
            nextKey = 0;
            maps.Clear();
            strings.Clear();
        }

        internal bool TryLookup(XmlDictionaryString s, out int key)
        {
            IntArray keys;
            if (maps.TryGetValue(s.Dictionary, out keys))
            {
                key = (keys[s.Key] - 1);

                if (key != -1)
                {
                    return true;
                }
            }

            if (strings.TryGetValue(s.Value, out key))
            {
                if (keys == null)
                {
                    keys = AddKeys(s.Dictionary, s.Key + 1);
                }

                keys[s.Key] = (key + 1);
                return true;
            }

            key = -1;
            return false;
        }

        class PriorityDictionary<K, V> where K : class
        {
            Dictionary<K, V> dictionary;
            Entry[] list;
            int listCount;
            int now;

            public PriorityDictionary()
            {
                list = new Entry[16];
            }

            public void Clear()
            {
                now = 0;
                listCount = 0;
                Array.Clear(list, 0, list.Length);
                if (dictionary != null)
                    dictionary.Clear();
            }

            public bool TryGetValue(K key, out V value)
            {
                for (int i = 0; i < listCount; i++)
                {
                    if (list[i].Key == key)
                    {
                        value = list[i].Value;
                        list[i].Time = Now;
                        return true;
                    }
                }

                for (int i = 0; i < listCount; i++)
                {
                    if (list[i].Key.Equals(key))
                    {
                        value = list[i].Value;
                        list[i].Time = Now;
                        return true;
                    }
                }

                if (dictionary == null)
                {
                    value = default(V);
                    return false;
                }

                if (!dictionary.TryGetValue(key, out value))
                {
                    return false;
                }

                int minIndex = 0;
                int minTime = list[0].Time;
                for (int i = 1; i < listCount; i++)
                {
                    if (list[i].Time < minTime)
                    {
                        minIndex = i;
                        minTime = list[i].Time;
                    }
                }

                list[minIndex].Key = key;
                list[minIndex].Value = value;
                list[minIndex].Time = Now;
                return true;
            }

            public void Add(K key, V value)
            {
                if (listCount < list.Length)
                {
                    list[listCount].Key = key;
                    list[listCount].Value = value;
                    listCount++;
                }
                else
                {
                    if (dictionary == null)
                    {
                        dictionary = new Dictionary<K, V>();
                        for (int i = 0; i < listCount; i++)
                        {
                            dictionary.Add(list[i].Key, list[i].Value);
                        }
                    }

                    dictionary.Add(key, value);
                }
            }

            int Now
            {
                get
                {
                    if (++now == int.MaxValue)
                    {
                        DecreaseAll();
                    }

                    return now;
                }
            }

            void DecreaseAll()
            {
                for (int i = 0; i < listCount; i++)
                {
                    list[i].Time /= 2;
                }

                now /= 2;
            }

            struct Entry
            {
                public K Key;
                public V Value;
                public int Time;
            }
        }

        class IntArray
        {
            int[] array;

            public IntArray(int size)
            {
                this.array = new int[size];
            }

            public int this[int index]
            {
                get
                {
                    if (index >= array.Length)
                        return 0;

                    return array[index];
                }
                set
                {
                    if (index >= array.Length)
                    {
                        int[] newArray = new int[Math.Max(index + 1, array.Length * 2)];
                        Array.Copy(array, newArray, array.Length);
                        array = newArray;
                    }

                    array[index] = value;
                }
            }
        }
    }
}
