//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;

    [Serializable]
    class ContextDictionary : IDictionary<string, string>
    {
        static ContextDictionary empty;

        IDictionary<string, string> dictionaryStore;

        public ContextDictionary()
        {
            this.dictionaryStore = new Dictionary<string, string>();
        }

        public ContextDictionary(IDictionary<string, string> context)
        {
            this.dictionaryStore = new Dictionary<string, string>();

            if (context != null)
            {
                bool ignoreValidation = context is ContextDictionary;

                foreach (KeyValuePair<string, string> pair in context)
                {
                    if (ignoreValidation)
                    {
                        this.dictionaryStore.Add(pair);
                    }
                    else
                    {
                        this.Add(pair);
                    }
                }
            }
        }

        internal static ContextDictionary Empty
        {
            get
            {
                if (empty == null)
                {
                    ContextDictionary localEmpty = new ContextDictionary();
                    localEmpty.dictionaryStore = new ReadOnlyDictionaryInternal<string, string>(new Dictionary<string, string>(0));
                    empty = localEmpty;
                }

                return empty;
            }
        }

        public int Count
        {
            get { return this.dictionaryStore.Count; }
        }

        public bool IsReadOnly
        {
            get { return this.dictionaryStore.IsReadOnly; }
        }

        public ICollection<string> Keys
        {
            get { return this.dictionaryStore.Keys; }
        }

        public ICollection<string> Values
        {
            get { return this.dictionaryStore.Values; }
        }

        public string this[string key]
        {
            get
            {
                ValidateKeyValueSpace(key);
                return this.dictionaryStore[key];
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                ValidateKeyValueSpace(key);
                this.dictionaryStore[key] = value;
            }
        }

        public void Add(string key, string value)
        {
            if (value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
            }

            ValidateKeyValueSpace(key);
            this.dictionaryStore.Add(key, value);
        }


        public void Add(KeyValuePair<string, string> item)
        {
            if (item.Key == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item.Key");
            }
            if (item.Value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item.Value");
            }
            ValidateKeyValueSpace(item.Key);
            this.dictionaryStore.Add(item);
        }

        public void Clear()
        {
            this.dictionaryStore.Clear();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            if (item.Key == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item.Key");
            }
            if (item.Value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item.Value");
            }

            ValidateKeyValueSpace(item.Key);
            return this.dictionaryStore.Contains(item);
        }


        public bool ContainsKey(string key)
        {
            ValidateKeyValueSpace(key);
            return this.dictionaryStore.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            this.dictionaryStore.CopyTo(array, arrayIndex);
        }


        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return this.dictionaryStore.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.dictionaryStore).GetEnumerator();
        }

        public bool Remove(string key)
        {
            ValidateKeyValueSpace(key);
            return this.dictionaryStore.Remove(key);
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            if (item.Key == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item.Key");
            }
            if (item.Value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item.Value");
            }

            ValidateKeyValueSpace(item.Key);
            return this.dictionaryStore.Remove(item);
        }

        public bool TryGetValue(string key, out string value)
        {
            ValidateKeyValueSpace(key);
            return this.dictionaryStore.TryGetValue(key, out value);
        }

        internal static bool TryValidateKeyValueSpace(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
            }

            for (int counter = 0; counter < key.Length; ++counter)
            {
                char currentCharacter = key[counter];

                if (!IsLetterOrDigit(currentCharacter) && currentCharacter != '-' && currentCharacter != '_' && currentCharacter != '.')
                {
                    return false;
                }
            }
            return true;
        }

        static bool IsLetterOrDigit(char c)
        {
            return (('A' <= c) && (c <= 'Z')) ||
                (('a' <= c) && (c <= 'z')) ||
                (('0' <= c) && (c <= '9'));
        }

        static void ValidateKeyValueSpace(string key)
        {
            if (!TryValidateKeyValueSpace(key))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("key", SR.GetString(SR.InvalidCookieContent, key)));
            }
        }
    }
}
