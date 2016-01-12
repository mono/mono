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
#if CLR_2_0 || CLR_4_0
using System.Collections.Generic;
#endif
using NUnit.Framework.Internal;
using NUnit.TestUtilities;

namespace NUnit.Framework.Constraints.Tests
{
    [TestFixture]
    public class CollectionOrderedConstraintTests : NUnit.Framework.Assertions.MessageChecker
    {
        [Test]
        public void IsOrdered()
        {
            ICollection collection = new SimpleObjectCollection("x", "y", "z");
            Assert.That(collection, Is.Ordered);
        }

        [Test]
        public void IsOrderedDescending()
        {
            ICollection collection = new SimpleObjectCollection("z", "y", "x");
            Assert.That(collection, Is.Ordered.Descending);
        }

        [Test, ExpectedException(typeof(AssertionException))]
        public void IsOrdered_Fails()
        {
            ICollection collection = new SimpleObjectCollection("x", "z", "y");
            expectedMessage =
                "  Expected: collection ordered" + NL +
                "  But was:  < \"x\", \"z\", \"y\" >" + NL;

            Assert.That(collection, Is.Ordered);
        }

        [Test]
        public void IsOrdered_Allows_adjacent_equal_values()
        {
            ICollection collection = new SimpleObjectCollection("x", "x", "z");
            Assert.That(collection, Is.Ordered);
        }

        [Test, ExpectedException(typeof(ArgumentNullException),
            ExpectedMessage = "index 1", MatchType = MessageMatch.Contains)]
        public void IsOrdered_Handles_null()
        {
            ICollection collection = new SimpleObjectCollection("x", null, "z");
            Assert.That(collection, Is.Ordered);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void IsOrdered_TypesMustBeComparable()
        {
            ICollection collection = new SimpleObjectCollection(1, "x");
            Assert.That(collection, Is.Ordered);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void IsOrdered_AtLeastOneArgMustImplementIComparable()
        {
            ICollection collection = new SimpleObjectCollection(new object(), new object());
            Assert.That(collection, Is.Ordered);
        }

        [Test]
        public void IsOrdered_Handles_custom_comparison()
        {
            ICollection collection = new SimpleObjectCollection(new object(), new object());

            AlwaysEqualComparer comparer = new AlwaysEqualComparer();
            Assert.That(collection, Is.Ordered.Using(comparer));
            Assert.That(comparer.Called, "TestComparer was not called");
        }

        [Test]
        public void IsOrdered_Handles_custom_comparison2()
        {
            ICollection collection = new SimpleObjectCollection(2, 1);

            TestComparer comparer = new TestComparer();
            Assert.That(collection, Is.Ordered.Using(comparer));
            Assert.That(comparer.Called, "TestComparer was not called");
        }

#if CLR_2_0 || CLR_4_0
        [Test]
        public void UsesProvidedComparerOfT()
        {
            ICollection al = new SimpleObjectCollection(1, 2);

            MyComparer<int> comparer = new MyComparer<int>();
            Assert.That(al, Is.Ordered.Using(comparer));
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
            ICollection al = new SimpleObjectCollection(1, 2);

            MyComparison<int> comparer = new MyComparison<int>();
            Assert.That(al, Is.Ordered.Using(new Comparison<int>(comparer.Compare)));
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

#if !NETCF_2_0
        [Test]
        public void UsesProvidedLambda()
        {
            ICollection al = new SimpleObjectCollection(1, 2);

            Comparison<int> comparer = (x, y) => x.CompareTo(y);
            Assert.That(al, Is.Ordered.Using(comparer));
        }
#endif
#endif

        [Test]
        public void IsOrderedBy()
        {
            ICollection collection = new SimpleObjectCollection(
                new OrderedByTestClass(1),
                new OrderedByTestClass(2));

            Assert.That(collection, Is.Ordered.By("Value"));
        }

        [Test]
        public void IsOrderedBy_Comparer()
        {
            ICollection collection = new SimpleObjectCollection(
                new OrderedByTestClass(1),
                new OrderedByTestClass(2));

            Assert.That(collection, Is.Ordered.By("Value").Using(new SimpleObjectComparer()));
        }

        [Test]
        public void IsOrderedBy_Handles_heterogeneous_classes_as_long_as_the_property_is_of_same_type()
        {
            ICollection al = new SimpleObjectCollection(
                new OrderedByTestClass(1),
                new OrderedByTestClass2(2));

            Assert.That(al, Is.Ordered.By("Value"));
        }

        public class OrderedByTestClass
        {
            private int myValue;

            public int Value
            {
                get { return myValue; }
                set { myValue = value; }
            }

            public OrderedByTestClass(int value)
            {
                Value = value;
            }
        }

        public class OrderedByTestClass2
        {
            private int myValue;
            public int Value
            {
                get { return myValue; }
                set { myValue = value; }
            }

            public OrderedByTestClass2(int value)
            {
                Value = value;
            }
        }
    }
}