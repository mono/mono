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
using NUnit.Framework.Constraints;

namespace NUnit.Framework.Constraints.Tests
{
    [TestFixture]
    public class NumericsTest
    {
        private Tolerance tenPercent, zeroTolerance;

        [SetUp]
        public void SetUp()
        {
            tenPercent = new Tolerance(10.0).Percent;
            zeroTolerance = new Tolerance(0);
        }

        [TestCase(123456789)]
        [TestCase(123456789U)]
        [TestCase(123456789L)]
        [TestCase(123456789UL)]
        [TestCase(1234.5678f)]
        [TestCase(1234.5678)]
        [Test]
        public void CanMatchWithoutToleranceMode(object value)
        {
            Assert.IsTrue(Numerics.AreEqual(value, value, ref zeroTolerance));
        }

        // Separate test case because you can't use decimal in an attribute (24.1.3)
        [Test]
        public void CanMatchDecimalWithoutToleranceMode()
        {
            Assert.IsTrue(Numerics.AreEqual(123m, 123m, ref zeroTolerance));
        }

        [TestCase((int)9500)]
        [TestCase((int)10000)]
        [TestCase((int)10500)]
        [TestCase((uint)9500)]
        [TestCase((uint)10000)]
        [TestCase((uint)10500)]
        [TestCase((long)9500)]
        [TestCase((long)10000)]
        [TestCase((long)10500)]
        [TestCase((ulong)9500)]
        [TestCase((ulong)10000)]
        [TestCase((ulong)10500)]
        [Test]
        public void CanMatchIntegralsWithPercentage(object value)
        {
            Assert.IsTrue(Numerics.AreEqual(10000, value, ref tenPercent));
        }

        [Test]
        public void CanMatchDecimalWithPercentage()
        {
            Assert.IsTrue(Numerics.AreEqual(10000m, 9500m, ref tenPercent));
            Assert.IsTrue(Numerics.AreEqual(10000m, 10000m, ref tenPercent));
            Assert.IsTrue(Numerics.AreEqual(10000m, 10500m, ref tenPercent));
        }

        [TestCase((int)8500)]
        [TestCase((int)11500)]
        [TestCase((uint)8500)]
        [TestCase((uint)11500)]
        [TestCase((long)8500)]
        [TestCase((long)11500)]
        [TestCase((ulong)8500)]
        [TestCase((ulong)11500)]
        [Test, ExpectedException(typeof(AssertionException))]
        public void FailsOnIntegralsOutsideOfPercentage(object value)
        {
            Assert.IsTrue(Numerics.AreEqual(10000, value, ref tenPercent));
        }

        [Test, ExpectedException(typeof(AssertionException))]
        public void FailsOnDecimalBelowPercentage()
        {
            Assert.IsTrue(Numerics.AreEqual(10000m, 8500m, ref tenPercent));
        }

        [Test, ExpectedException(typeof(AssertionException))]
        public void FailsOnDecimalAbovePercentage()
        {
            Assert.IsTrue(Numerics.AreEqual(10000m, 11500m, ref tenPercent));
        }
    }
}