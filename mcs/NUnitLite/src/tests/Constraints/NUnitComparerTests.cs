// ***********************************************************************
// Copyright (c) 2009 Charlie Poole
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

namespace NUnit.Framework.Constraints.Tests
{
    [TestFixture]
    public class NUnitComparerTests
    {
        private Tolerance tolerance;
        private NUnitComparer comparer;

        [SetUp]
        public void SetUp()
        {
            tolerance = Tolerance.Empty;
            comparer = new NUnitComparer();
        }

        [TestCase(4, 4)]
        [TestCase(4.0d, 4.0d)]
        [TestCase(4.0f, 4.0f)]
        [TestCase(4, 4.0d)]
        [TestCase(4, 4.0f)]
        [TestCase(4.0d, 4)]
        [TestCase(4.0d, 4.0f)]
        [TestCase(4.0f, 4)]
        [TestCase(4.0f, 4.0d)]
        [TestCase(SpecialValue.Null, SpecialValue.Null)]
#if CLR_2_0 || CLR_4_0
        [TestCase(null, null)]
#endif
        public void EqualItems(object x, object y)
        {
            Assert.That(comparer.Compare(x, y) == 0);
        }

        [TestCase(4, 2)]
        [TestCase(4.0d, 2.0d)]
        [TestCase(4.0f, 2.0f)]
        [TestCase(4, 2.0d)]
        [TestCase(4, 2.0f)]
        [TestCase(4.0d, 2)]
        [TestCase(4.0d, 2.0f)]
        [TestCase(4.0f, 2)]
        [TestCase(4.0f, 2.0d)]
        [TestCase(4, SpecialValue.Null)]
#if CLR_2_0 || CLR_4_0
        [TestCase(4, null)]
#endif
        public void UnequalItems(object greater, object lesser)
        {
            Assert.That(comparer.Compare(greater, lesser) > 0);
            Assert.That(comparer.Compare(lesser, greater) < 0);
        }
    }
}
