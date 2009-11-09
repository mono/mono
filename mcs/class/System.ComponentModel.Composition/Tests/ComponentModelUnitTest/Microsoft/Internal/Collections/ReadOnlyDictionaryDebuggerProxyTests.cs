// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Internal.Collections
{
    [TestClass]
    public class ReadOnlyDictionaryDebuggerProxyTests
    {
        [TestMethod]
        public void Constructor_NullAsDictionaryArgument_ShouldThrowArgumentNull()
        {
            ExceptionAssert.ThrowsArgument<ArgumentNullException>("dictionary", () =>
            {
                new ReadOnlyDictionaryDebuggerProxy<string, object>((ReadOnlyDictionary<string, object>)null);
            });
        }

        [TestMethod]
        public void Constructor_EmptyDictionaryAsDictionaryArgument_ShouldSetItemsPropertyToEmptyEnumerable()
        {
            var dictionary = CreateReadOnlyDictionary<string, object>();

            var proxy = new ReadOnlyDictionaryDebuggerProxy<string, object>(dictionary);

            EnumerableAssert.IsEmpty(proxy.Items);
        }

        [TestMethod]
        public void Constructor_ValueAsDictionaryArgument_ShouldSetItemsProperty()
        {
            var expectations = Expectations.GetMetadata();

            foreach (var e in expectations)
            {
                var proxy = new ReadOnlyDictionaryDebuggerProxy<string, object>(CreateReadOnlyDictionary(e));

                EnumerableAssert.AreEqual(e, proxy.Items);
            }
        }

        [TestMethod]
        public void Items_ShouldNotCacheUnderlyingItems()
        {
            var dictionary = new Dictionary<string, object>();
            dictionary.Add("Name", "Value");

            var proxy = new ReadOnlyDictionaryDebuggerProxy<string, object>(CreateReadOnlyDictionary(dictionary));

            EnumerableAssert.AreEqual(dictionary, proxy.Items);

            dictionary.Add("AnotherName", "Value");

            EnumerableAssert.AreEqual(dictionary, proxy.Items);

            dictionary.Add("AndAnotherName", "Value");

            EnumerableAssert.AreEqual(dictionary, proxy.Items);
        }

        private static ReadOnlyDictionary<TKey, TValue> CreateReadOnlyDictionary<TKey, TValue>()
        {
            return CreateReadOnlyDictionary<TKey, TValue>(null);
        }

        private static ReadOnlyDictionary<TKey, TValue> CreateReadOnlyDictionary<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
        {
            return new ReadOnlyDictionary<TKey, TValue>(dictionary);
        }
    }
}