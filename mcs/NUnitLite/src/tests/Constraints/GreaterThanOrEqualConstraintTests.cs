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

namespace NUnit.Framework.Constraints.Tests
{
    [TestFixture]
    public class GreaterThanOrEqualConstraintTests : ComparisonConstraintTest
    {
        [SetUp]
        public void SetUp()
        {
            theConstraint = comparisonConstraint = new GreaterThanOrEqualConstraint(5);
            expectedDescription = "greater than or equal to 5";
            stringRepresentation = "<greaterthanorequal 5>";
        }

        internal object[] SuccessData = new object[] { 6, 5 };

        internal object[] FailureData = new object[] { new object[] { 4, "4" } };

        internal object[] InvalidData = new object[] { null, "xxx" };

        [Test]
        public void CanCompareIComparables()
        {
            ClassWithIComparable expected = new ClassWithIComparable(0);
            ClassWithIComparable actual = new ClassWithIComparable(42);
            Assert.That(actual, Is.GreaterThanOrEqualTo(expected));
        }

#if CLR_2_0 || CLR_4_0
        [Test]
        public void CanCompareIComparablesOfT()
        {
            ClassWithIComparableOfT expected = new ClassWithIComparableOfT(0);
            ClassWithIComparableOfT actual = new ClassWithIComparableOfT(42);
            Assert.That(actual, Is.GreaterThanOrEqualTo(expected));
        }
#endif
    }
}