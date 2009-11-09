// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Internal.Collections
{
    [TestClass]
    public class ReadOnlyDictionaryTests
    {
        [TestMethod]
        public void Constructor_NullAsDictionaryArgument_ShouldCreateEmptyDictionary()
        {
            var dictionary = new ReadOnlyDictionary<string, object>(null);

            EnumerableAssert.IsEmpty(dictionary);
        }

        [TestMethod]
        public void Constructor_WritableDictionaryAsDictionaryArgument_ShouldPopulateCollection()
        {
            var dictionary = GetWritableDictionaryWithData();
            var readOnlyDictionary = new ReadOnlyDictionary<string, object>(dictionary);

            EnumerableAssert.AreEqual(dictionary, readOnlyDictionary);
        }

        [TestMethod]
        public void Add1_ShouldThrowNotSupported()
        {
            var dictionary = GetReadOnlyDictionaryWithData();

            ExceptionAssert.Throws<NotSupportedException>(() =>
            {
                dictionary.Add(new KeyValuePair<string, object>("Key", "Value"));
            });
        }

        [TestMethod]
        public void Add2_ShouldThrowNotSupported()
        {
            var dictionary = GetReadOnlyDictionaryWithData();

            ExceptionAssert.Throws<NotSupportedException>(() =>
            {
                dictionary.Add("Key", "Value");
            });
        }

        [TestMethod]
        public void Clear_ShouldThrowNotSupported()
        {
            var dictionary = GetReadOnlyDictionaryWithData();

            ExceptionAssert.Throws<NotSupportedException>(() =>
            {
                dictionary.Clear();
            });
        }

        [TestMethod]
        public void Remove1_ShouldThrowNotSupported()
        {
            var dictionary = GetReadOnlyDictionaryWithData();

            ExceptionAssert.Throws<NotSupportedException>(() =>
            {
                dictionary.Remove("Value");
            });
        }

        [TestMethod]
        public void Remove2_ShouldThrowNotSupported()
        {
            var dictionary = GetReadOnlyDictionaryWithData();

            ExceptionAssert.Throws<NotSupportedException>(() =>
            {
                dictionary.Remove(new KeyValuePair<string, object>("Key", "Value"));
            });
        }

        [TestMethod]
        public void ItemSet_ShouldThrowNotSupported()
        {
            var dictionary = GetReadOnlyDictionaryWithData();

            ExceptionAssert.Throws<NotSupportedException>(() =>
            {
                dictionary["Key"] = "Value";
            });
        }

        [TestMethod]
        public void Keys_ShouldReturnWrappedDictionaryKeys()
        {
            var dictionary = GetWritableDictionaryWithData();
            var readOnlyDictionary = GetReadOnlyDictionary(dictionary);

            EnumerableAssert.AreEqual(readOnlyDictionary.Keys, dictionary.Keys);
        }

        [TestMethod]
        public void Values_ShouldReturnWrappedDictionaryValues()
        {
            var dictionary = GetWritableDictionaryWithData();
            var readOnlyDictionary = GetReadOnlyDictionary(dictionary);

            EnumerableAssert.AreEqual(readOnlyDictionary.Values, readOnlyDictionary.Values);
        }

        [TestMethod]
        public void IsReadOnly_ShouldAlwaysBeTrue()
        {
            var dictionary = GetWritableDictionaryWithData();
            var readOnlyDictionary = GetReadOnlyDictionary(dictionary);
            
            Assert.IsFalse(dictionary.IsReadOnly);
            Assert.IsTrue(readOnlyDictionary.IsReadOnly);
        }

        [TestMethod]
        public void Count_ShouldReturnWrappedDictionaryCount()
        {
            var dictionary = GetWritableDictionaryWithData();
            var readOnlyDictionary = GetReadOnlyDictionary(dictionary);

            Assert.AreEqual(dictionary.Count, readOnlyDictionary.Count);
        }

        [TestMethod]
        public void ContainsKey()
        {
            var dictionary = GetWritableDictionaryWithData();
            var readOnlyDictionary = GetReadOnlyDictionary(dictionary);

            Assert.IsTrue(readOnlyDictionary.ContainsKey("Key1"));
            Assert.IsFalse(readOnlyDictionary.ContainsKey("InvalidKey"));
        }

        [TestMethod]
        public void Contains()
        {
            var dictionary = GetWritableDictionaryWithData();
            var readOnlyDictionary = GetReadOnlyDictionary(dictionary);

            Assert.IsTrue(readOnlyDictionary.Contains(new KeyValuePair<string,object>("Key1", "Value1")));
            Assert.IsFalse(readOnlyDictionary.Contains(new KeyValuePair<string,object>("InvalidKey", "Value1")));
            Assert.IsFalse(readOnlyDictionary.Contains(new KeyValuePair<string,object>("Key1", "InvalidValue")));
        }

        [TestMethod]
        public void CopyTo()
        {
            var dictionary = GetWritableDictionaryWithData();
            var readOnlyDictionary = GetReadOnlyDictionary(dictionary);
            KeyValuePair<string, object>[] destination = new KeyValuePair<string, object> [readOnlyDictionary.Count];
            readOnlyDictionary.CopyTo(destination, 0);
            EnumerableAssert.AreEqual(readOnlyDictionary, destination);
        }

        [TestMethod]
        public void GetEnumerator()
        {
            var dictionary = GetWritableDictionaryWithData();
            var readOnlyDictionary = GetReadOnlyDictionary(dictionary);
            IEnumerable<KeyValuePair<string, object>> genericEnumerable = readOnlyDictionary;
            EnumerableAssert.AreEqual(genericEnumerable, dictionary);
            IEnumerable weakEnumerable = (IEnumerable)readOnlyDictionary;
            EnumerableAssert.AreEqual(weakEnumerable, dictionary);
        }

        [TestMethod]
        public void Item()
        {
            var dictionary = GetWritableDictionaryWithData();
            var readOnlyDictionary = GetReadOnlyDictionary(dictionary);
            Assert.AreEqual("Value1", readOnlyDictionary["Key1"], "Expecting to read wrapped value");
        }

        [TestMethod]
        public void TryGetValue()
        {
            var dictionary = GetWritableDictionaryWithData();
            var readOnlyDictionary = GetReadOnlyDictionary(dictionary);
            object result;
            bool ret = readOnlyDictionary.TryGetValue("Key1", out result);
            Assert.IsTrue(ret, "Expecting TryGetExportedValue to return true for wrapped key");
            Assert.AreEqual("Value1", result, "Expecting TryGetExportedValue to return wrapped value");
        }

        private static IDictionary<String, object> GetReadOnlyDictionaryWithData()
        {
            return GetReadOnlyDictionary(GetWritableDictionaryWithData());
        }

        private static IDictionary<TKey, TValue> GetReadOnlyDictionary<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
        {
            return new ReadOnlyDictionary<TKey, TValue>(dictionary);
        }

        private static IDictionary<String, object> GetWritableDictionaryWithData()
        {
            IDictionary<String, object> dictionary = new Dictionary<String, object>();
            dictionary.Add("Key1", "Value1");
            dictionary.Add("Key2", 42);
            return dictionary;
        }
    }
}