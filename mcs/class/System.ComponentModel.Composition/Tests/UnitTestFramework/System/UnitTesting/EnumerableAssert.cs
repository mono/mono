// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Internal.Collections;

namespace System.UnitTesting
{
    public static class EnumerableAssert
    {
        public static void IsTrueForAll<T>(IEnumerable<T> source, Predicate<T> predicate)
        {
            IsTrueForAll(source, predicate, "IsTrueForAll Failed");
        }

        public static void IsTrueForAll<T>(IEnumerable<T> source, Predicate<T> predicate, string message)
        {
            Assert.IsNotNull(source, "Source should not be null!");

            foreach (T t in source)
            {
                Assert.IsTrue(predicate(t), message);
            }
        }

        // Needed to prevent strings from matching to the plain IEnumerable overload
        public static void AreEqual(IEnumerable actual, params string[] expected)
        {
            AreEqual((IEnumerable)expected, (IEnumerable)actual);
        }

        public static void AreEqual(IEnumerable actual, params object[] expected)
        {
            AreEqual((IEnumerable)expected, (IEnumerable)actual);
        }

        public static void AreEqual<T>(IEnumerable<T> actual, params T[] expected)
        {
            AreEqual<T>((IEnumerable<T>)expected, (IEnumerable<T>)actual);
        }

        public static void AreEqual(IEnumerable expected, IEnumerable actual)
        {
            Assert.AreEqual(expected.Count(), actual.Count(), "Enumerable should contain the correct number of items");

            List<object> actualList = actual.ToList();

            foreach (object value in expected)
            {
                bool removed = actualList.Remove(value);

                Assert.IsTrue(removed, "Enumerable does not contain value {0}.", value);
            }

            Assert.AreEqual(0, actualList.Count, "Enumerable contains extra values.");
        }

        public static void AreEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            // First, test the IEnumerable implementation
            AreEqual((IEnumerable)expected, (IEnumerable)actual);

            // Second, test the IEnumerable<T> implementation
            Assert.AreEqual(expected.Count(), actual.Count(), "Enumerable should contain the correct number of items");

            List<T> actualList = actual.ToList();

            foreach (T value in expected)
            {
                bool removed = actualList.Remove(value);

                Assert.IsTrue(removed, "Enumerable does not contain value {0}.", value);
            }

            Assert.AreEqual(0, actualList.Count, "Enumerable contains extra values.");
        }

        // Needed to prevent strings from matching to the plain IEnumerable overload
        public static void AreSequenceEqual(IEnumerable actual, params string[] expected)
        {   
            AreEqual((IEnumerable)expected, (IEnumerable)actual);
        }

        public static void AreSequenceEqual(IEnumerable actual, params object[] expected)
        {
            AreEqual((IEnumerable)expected, (IEnumerable)actual);
        }

        public static void AreSequenceEqual(IEnumerable expected, IEnumerable actual)
        {
            AreSequenceEqual(expected, actual, (Action<int, object, object>)null);
        }

        public static void AreSequenceEqual(IEnumerable expected, IEnumerable actual, Action<int, object, object> comparer)
        {
            if (comparer == null)
            {
                comparer = (i, left, right) => Assert.AreEqual(left, right, "Enumerable at index {0} should have same value", i);
            }

            int expectedCount = expected.Count();

            Assert.AreEqual(expectedCount, actual.Count(), "Enumerable should contain the correct number of items");

            IEnumerator actualEnumerator = actual.GetEnumerator();
            IEnumerator expectedEnumerator = expected.GetEnumerator();

            int index = 0;
            while (index < expectedCount)
            {
                Assert.IsTrue(actualEnumerator.MoveNext());
                Assert.IsTrue(expectedEnumerator.MoveNext());

                comparer(index, expectedEnumerator.Current, actualEnumerator.Current);
                index++;
            }
        }

        public static void AreSequenceEqual<T>(IEnumerable<T> actual, params T[] expected)
        {
            AreSequenceEqual<T>((IEnumerable<T>)expected, (IEnumerable<T>)actual);
        }

        public static void AreSequenceEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            AreSequenceEqual<T>(expected, actual, (Action<int, T, T>)null);
        }

        public static void AreSequenceEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual, Action<int, T, T> comparer)
        {
            if (comparer == null)
            {
                comparer = (i, left, right) => Assert.AreEqual(left, right, "Enumerable at index {0} should have same value", i);
            }

            // First, test the IEnumerable implementation
            AreSequenceEqual((IEnumerable)expected, (IEnumerable)actual, (Action<int, object, object>)((currentIndex, left, right) => comparer(currentIndex, (T)left, (T)right)));

            // Second, test the IEnumerable<T> implementation
            int expectedCount = expected.Count();

            IEnumerator<T> actualEnumerator = actual.GetEnumerator();
            IEnumerator<T> expectedEnumerator = expected.GetEnumerator();

            int index = 0;
            while (index < expectedCount)
            {
                Assert.IsTrue(actualEnumerator.MoveNext());
                Assert.IsTrue(expectedEnumerator.MoveNext());

                comparer(index, expectedEnumerator.Current, actualEnumerator.Current);                
                index++;
            }
        }

        public static void AreSequenceSame<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            AreSequenceEqual<T>(expected, actual, (index, left, right) =>
            {
                Assert.AreSame(left, right, "Enumerable at index {0} should have same value", index);
            });
        }

        public static void AreEqual<TKey, TValue>(IDictionary<TKey, TValue> expected, IDictionary<TKey, TValue> actual)
        {
            Assert.AreEqual(expected.Count, actual.Count, "Dictionaries are different : first has '{0} elements, whereas second has '{1}", expected.Count, actual.Count);

            foreach (KeyValuePair<TKey, TValue> kvp in expected)
            {
                TValue firstValue = kvp.Value;
                TValue secondValue = default(TValue);
                if (!actual.TryGetValue(kvp.Key, out secondValue))
                {
                    Assert.Fail("Dictionaries are different : There is no item with key '{0}' in the second dictionary", kvp.Key);
                }

                if ((firstValue is IDictionary<TKey, TValue>) && (secondValue is IDictionary<TKey, TValue>))
                {
                    AreEqual((IDictionary<TKey, TValue>)firstValue, (IDictionary<TKey, TValue>)secondValue);
                    continue;
                }

                Assert.AreEqual(kvp.Value, secondValue, "Dictionaries are different : values for key '{0}' are different - '{1}' vs '{2}'", kvp.Key, firstValue, secondValue);
            }
        }

        /// <summary>
        ///     Verifies that the specified enumerable is empty.
        /// </summary>
        public static void IsEmpty(IEnumerable source)
        {
            IsEmpty(source, null);
        }

        public static void IsEmpty(IEnumerable source, string message)
        {
            Assert.AreEqual(0, source.Count(), message);
        }
    }
}
