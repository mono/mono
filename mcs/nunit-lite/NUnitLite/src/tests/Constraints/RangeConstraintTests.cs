// ***********************************************************************
// Copyright (c) 2008 Charlie Poole
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
using NUnit.TestUtilities;

namespace NUnit.Framework.Constraints.Tests
{
    [TestFixture]
    public class RangeConstraintTest : ConstraintTestBaseWithArgumentException
    {
#if CLR_2_0 || CLR_4_0
        RangeConstraint<int> rangeConstraint;
#else
        RangeConstraint rangeConstraint;
#endif

        [SetUp]
        public void SetUp()
        {
#if CLR_2_0 || CLR_4_0
            theConstraint = rangeConstraint = new RangeConstraint<int>(5, 42);
#else
            theConstraint = rangeConstraint = new RangeConstraint(5, 42);
#endif
            expectedDescription = "in range (5,42)";
            stringRepresentation = "<range 5 42>";
        }

        internal object[] SuccessData = new object[] { 5, 23, 42 };

        internal object[] FailureData = new object[] { new object[] { 4, "4" }, new object[] { 43, "43" } };

        internal object[] InvalidData = new object[] { null, "xxx" };

        [Test]
        public void UsesProvidedIComparer()
        {
            SimpleObjectComparer comparer = new SimpleObjectComparer();
            Assert.That(rangeConstraint.Using(comparer).Matches(19));
            Assert.That(comparer.Called, "Comparer was not called");
        }

#if CLR_2_0 || CLR_4_0
        [Test]
        public void UsesProvidedComparerOfT()
        {
            MyComparer<int> comparer = new MyComparer<int>();
            Assert.That(rangeConstraint.Using(comparer).Matches(19));
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
            MyComparison<int> comparer = new MyComparison<int>();
            Assert.That(rangeConstraint.Using(new Comparison<int>(comparer.Compare)).Matches(19));
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
            Comparison<int> comparer = (x, y) => x.CompareTo(y);
            Assert.That(rangeConstraint.Using(comparer).Matches(19));
        }
#endif
#endif
    }
}