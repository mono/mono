// ***********************************************************************
// Copyright (c) 2007 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.Collections;
using NUnit.Framework.Internal;
using NUnit.TestUtilities;

#if CLR_2_0 || CLR_4_0
using System.Collections.Generic;
#endif

namespace NUnit.Framework.Constraints.Tests
{
    [TestFixture]
    public class CollectionContainsConstraintTests
    {
        [Test]
        public void CanTestContentsOfArray()
        {
            object item = "xyz";
            object[] c = new object[] { 123, item, "abc" };
            Assert.That(c, new CollectionContainsConstraint(item));
        }

#if !SILVERLIGHT
        [Test]
        public void CanTestContentsOfArrayList()
        {
            object item = "xyz";
            ArrayList list = new ArrayList(new object[] { 123, item, "abc" });
            Assert.That(list, new CollectionContainsConstraint(item));
        }

        [Test]
        public void CanTestContentsOfSortedList()
        {
            object item = "xyz";
            SortedList list = new SortedList();
            list.Add("a", 123);
            list.Add("b", item);
            list.Add("c", "abc");
            Assert.That(list.Values, new CollectionContainsConstraint(item));
            Assert.That(list.Keys, new CollectionContainsConstraint("b"));
        }
#endif

        [Test]
        public void CanTestContentsOfCollectionNotImplementingIList()
        {
            SimpleObjectCollection ints = new SimpleObjectCollection(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            Assert.That(ints, new CollectionContainsConstraint(9));
        }

        [Test]
        public void IgnoreCaseIsHonored()
        {
            Assert.That(new string[] { "Hello", "World" },
                new CollectionContainsConstraint("WORLD").IgnoreCase);
        }
        [Test]
        public void UsesProvidedIComparer()
        {
            MyComparer comparer = new MyComparer();
            Assert.That(new string[] { "Hello", "World" },
                new CollectionContainsConstraint("World").Using(comparer));
            Assert.That(comparer.Called, "Comparer was not called");
        }

        class MyComparer : IComparer
        {
            public bool Called;

            public int Compare(object x, object y)
            {
                Called = true;
                return 0;
            }
        }

#if CLR_2_0 || CLR_4_0
        [Test]
        public void UsesProvidedEqualityComparer()
        {
            MyEqualityComparer comparer = new MyEqualityComparer();
            Assert.That(new string[] { "Hello", "World" },
                new CollectionContainsConstraint("World").Using(comparer));
            Assert.That(comparer.Called, "Comparer was not called");
        }

        class MyEqualityComparer : IEqualityComparer
        {
            public bool Called;

            bool IEqualityComparer.Equals(object x, object y)
            {
                Called = true;
                return x == y;
            }

            int IEqualityComparer.GetHashCode(object x)
            {
                return x.GetHashCode();
            }
        }

        [Test]
        public void UsesProvidedEqualityComparerOfT()
        {
            MyEqualityComparerOfT<string> comparer = new MyEqualityComparerOfT<string>();
            Assert.That(new string[] { "Hello", "World" },
                new CollectionContainsConstraint("World").Using(comparer));
            Assert.That(comparer.Called, "Comparer was not called");
        }

        class MyEqualityComparerOfT<T> : IEqualityComparer<T>
        {
            public bool Called;

            bool IEqualityComparer<T>.Equals(T x, T y)
            {
                Called = true;
                return Comparer<T>.Default.Compare(x, y) == 0;
            }

            int IEqualityComparer<T>.GetHashCode(T x)
            {
                return x.GetHashCode();
            }
        }

        [Test]
        public void UsesProvidedComparerOfT()
        {
            MyComparer<string> comparer = new MyComparer<string>();
            Assert.That(new string[] { "Hello", "World" },
                new CollectionContainsConstraint("World").Using(comparer));
            Assert.That(comparer.Called, "Comparer was not called");
        }

        class MyComparer<T> : IComparer<T>
        {
            public bool Called;

            public int Compare(T x, T y)
            {
                Called = true;
                return Comparer<T>.Default.Compare(x, y);
            }
        }

        [Test]
        public void UsesProvidedComparisonOfT()
        {
            MyComparison<string> comparer = new MyComparison<string>();
            Assert.That(new string[] { "Hello", "World" },
                new CollectionContainsConstraint("World").Using(new Comparison<string>(comparer.Compare)));
            Assert.That(comparer.Called, "Comparer was not called");
        }

        class MyComparison<T>
        {
            public bool Called;

            public int Compare(T x, T y)
            {
                Called = true;
                return Comparer<T>.Default.Compare(x, y);
            }
        }

        [Test]
        public void ContainsWithRecursiveStructure()
        {
            SelfRecursiveEnumerable item = new SelfRecursiveEnumerable();
            SelfRecursiveEnumerable[] container = new SelfRecursiveEnumerable[] { new SelfRecursiveEnumerable(), item };

            Assert.That(container, new CollectionContainsConstraint(item));
        }

        class SelfRecursiveEnumerable : IEnumerable
        {
            public IEnumerator GetEnumerator()
            {
                yield return this;
            }
        }


#if !NETCF_2_0
        [Test]
        public void UsesProvidedLambdaExpression()
        {
            Assert.That(new string[] { "Hello", "World" },
                new CollectionContainsConstraint("WORLD").Using<string>((x, y) => StringUtil.Compare(x, y, true)));
        }
#endif
#endif
    }
}