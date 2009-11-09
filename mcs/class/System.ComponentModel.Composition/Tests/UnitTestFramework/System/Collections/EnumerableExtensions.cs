// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Internal.Collections
{
    public static class EnumerableExtensions
    {
        public static int Count(this IEnumerable source)
        {
            int count = 0;

            foreach (object o in source)
            {
                count++;
            }

            return count;
        }

        public static IEnumerable<T> ToEnumerable<T>(this IEnumerable source)
        {
            foreach (object value in source)
            {
                yield return (T)value;
            }
        }

        public static List<object> ToList(this IEnumerable source)
        {
            var enumerable = source.ToEnumerable<object>();

            return System.Linq.Enumerable.ToList(enumerable);
        }

        public static T AssertSingle<T>(this IEnumerable<T> source)
        {
            return AssertSingle(source, t => true);
        }

        public static T AssertSingle<T>(this IEnumerable<T> source, string message)
        {
            return AssertSingle(source, t => true, message);
        }

        public static T AssertSingle<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            return AssertSingle(source, predicate, "Expecting a single item matching the predicate in the collection.");
        }

        public static T AssertSingle<T>(this IEnumerable<T> source, Func<T, bool> predicate, string message)
        {
            int count = 0;
            T ret = default(T);
            foreach (T t in source)
            {
                if (predicate(t))
                {
                    count++;
                    ret = t;
                }
            }

            Assert.AreEqual(1, count, message);
            return ret;
        }
    }
}
