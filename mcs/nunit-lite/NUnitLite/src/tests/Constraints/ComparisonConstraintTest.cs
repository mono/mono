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
using NUnit.TestUtilities;

namespace NUnit.Framework.Constraints.Tests
{
    #region ComparisonConstraintTest

    public abstract class ComparisonConstraintTest : ConstraintTestBaseWithArgumentException
    {
        protected ComparisonConstraint comparisonConstraint;

        [Test]
        public void UsesProvidedIComparer()
        {
            SimpleObjectComparer comparer = new SimpleObjectComparer();
            comparisonConstraint.Using(comparer).Matches(0);
            Assert.That(comparer.Called, "Comparer was not called");
        }

#if CLR_2_0 || CLR_4_0
        [Test]
        public void UsesProvidedComparerOfT()
        {
            MyComparer<int> comparer = new MyComparer<int>();
            comparisonConstraint.Using(comparer).Matches(0);
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
            comparisonConstraint.Using(new Comparison<int>(comparer.Compare)).Matches(0);
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
            comparisonConstraint.Using(comparer).Matches(0);
        }
#endif
#endif
    }

    #endregion

    #region Comparison Test Classes

    class ClassWithIComparable : IComparable
    {
        private int val;

        public ClassWithIComparable(int val)
        {
            this.val = val;
        }

        public int CompareTo(object x)
        {
            ClassWithIComparable other = x as ClassWithIComparable;
            if (x is ClassWithIComparable)
                return val.CompareTo(other.val);

            throw new ArgumentException();
        }
    }

#if CLR_2_0 || CLR_4_0
    class ClassWithIComparableOfT : IComparable<ClassWithIComparableOfT>
    {
        private int val;

        public ClassWithIComparableOfT(int val)
        {
            this.val = val;
        }

        public int CompareTo(ClassWithIComparableOfT other)
        {
            return val.CompareTo(other.val);
        }
    }
#endif

    #endregion
}