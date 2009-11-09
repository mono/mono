// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.UnitTesting
{
    public static class TestUtilities
    {

        public static void CheckICollectionOfTConformance<T>(ICollection<T> list, T a, T b, T c, T d)
        {
            list.Clear();
            EnumerableAssert.AreEqual(list);

            Assert.IsFalse(list.IsReadOnly, "The list should not report being read-only for these tests to work");
            Assert.IsFalse(list.Contains(a), "Contains should fail for anything when the collection is empty");
            Assert.IsFalse(list.Remove(a), "Remove should fail on anything when the collection is empty");

            list.Add(a);
            EnumerableAssert.AreEqual(list, a);

            list.Add(b);
            EnumerableAssert.AreEqual(list, a, b);

            list.Add(c);
            EnumerableAssert.AreEqual(list, a, b, c);

            list.Remove(b);
            EnumerableAssert.AreEqual(list, a, c);

            list.Remove(c);
            EnumerableAssert.AreEqual(list, a);

            list.Remove(a);
            EnumerableAssert.AreEqual(list);

            list.Add(a); list.Add(b); list.Add(c);

            list.Clear();
            EnumerableAssert.AreEqual(list);

            list.Clear();
            EnumerableAssert.AreEqual(list);

            list.Add(d); list.Add(c); list.Add(b); list.Add(a);

            T[] destination = new T[5];
            list.CopyTo(destination, 0);
            EnumerableAssert.AreEqual(destination, d, c, b, a, default(T));
        }

        public static void CheckIListOfTConformance<T>(IList<T> list, T a, T b, T c, T d)
        {
            CheckICollectionOfTConformance(list, a, b, c, d);

            list.Clear();
            list.Insert(0, d);
            list.Insert(0, a);
            list.Insert(1, c);
            list.Insert(1, b);
            CompareListContents(list, a, b, c, d);

            list[1] = a;
            CompareListContents(list, a, a, c, d);

            list.RemoveAt(2);
            CompareListContents(list, a, a, d);

            Assert.AreEqual(2, list.IndexOf(d), "Expected indexof to return the correct location of {0}", d);
            Assert.AreEqual(-1, list.IndexOf(b), "{0} should not be found in the collection", b);
            Assert.AreEqual(-1, list.IndexOf(c), "{0} should not be found in the collection", c);
        }

        

        public static void CompareListContents<T>(IList<T> list, params object[] values)
        {
            EnumerableAssert.AreEqual(list, values);

            for (var index = 0; index < values.Length; index++)
            {
                Assert.AreEqual(values[index], list[index],
                    "List should return true for Contains on every element, index {0}, length {1}", index, values[index]);
            }
        }
    }
}
