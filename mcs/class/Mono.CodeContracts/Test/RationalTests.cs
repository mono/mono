// 
// RationalTests.cs.cs
// 
// Authors:
// 	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2012 Alexander Chebaturkin
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
// 

using Mono.CodeContracts.Static.Analysis.Numerical;

using NUnit.Framework;

namespace Test
{
    [TestFixture]
    public class RationalTests
    {
        private static readonly Rational zero = Rational.For(0L);
        private static readonly Rational zero1 = Rational.For(0L);
        private static readonly Rational one = Rational.For(1L);
        private static readonly Rational threeFourth = Rational.For(3L, 4L);
        private static readonly Rational sixEighth = Rational.For(6, 8);
        
        private static readonly Rational plusInf = Rational.PlusInfinity;
        private static readonly Rational minusInf = Rational.MinusInfinity;

        [Test]
        public void ShouldHaveInfinitiesAndNormalValuesWhichArentEqual()
        {
            Assert.IsFalse (plusInf == minusInf);
            Assert.IsFalse (plusInf == zero);
            Assert.IsFalse (minusInf == zero);
            Assert.IsTrue (zero == zero1);
            Assert.IsFalse (one == zero);
        }

        [Test]
        public void ShouldBeEqualByModuloOfDenominator ()
        {
            Assert.IsTrue (threeFourth == sixEighth);
        }

        [Test]
        public void ShouldHaveNextInt32()
        {
            Assert.That (threeFourth.NextInt32, Is.EqualTo (Rational.For (1)));
        }

        [Test]
        public void ShouldHaveGreaterThanOperatorsWithLongs()
        {
            Assert.That (threeFourth < 1L);
            Assert.That (1L > threeFourth);

            Assert.That(0L < threeFourth);
            Assert.That(threeFourth > 0L);
        }

        [Test]
        public void ShouldHaveLessEqualOperator()
        {
            minusInf.ShouldBeLessEqualThan (minusInf);
            minusInf.ShouldBeLessEqualThan (plusInf);
            minusInf.ShouldBeLessEqualThan (one);

            one.ShouldNotBeLessEqualThan (minusInf);
            one.ShouldNotBeLessEqualThan (zero);
            one.ShouldBeLessEqualThan (one);
            one.ShouldBeLessEqualThan (plusInf);

            plusInf.ShouldNotBeLessEqualThan (minusInf);
            plusInf.ShouldBeLessEqualThan (plusInf);
            plusInf.ShouldNotBeLessEqualThan (one);
        }

        [Test]
        public void ShouldHaveAddOperation ()
        {
            Rational seven20 = Rational.For (7, 20);
            Rational eleven15 = Rational.For (11, 15);

            Assert.That (seven20 + eleven15, Is.EqualTo (Rational.For (325,300)));
        }

        [Test]
        public void ShouldHaveSubOperation()
        {
            Rational seven20 = Rational.For(7, 20);
            Rational eleven15 = Rational.For(11, 15);

            Assert.That(seven20 - eleven15, Is.EqualTo(Rational.For(-23, 60)));
        }

        [Test]
        public void ShouldHaveMulOperation()
        {
            Rational seven22 = Rational.For(7, 22);
            Rational eleven21 = Rational.For(11, 21);

            Assert.That(seven22 * eleven21, Is.EqualTo(Rational.For(1, 6)));
        }

        [Test]
        public void ShouldHaveDivOperation()
        {
            Rational seven22 = Rational.For(7, 22);
            Rational eleven21 = Rational.For(21, 11);

            Assert.That(seven22 / eleven21, Is.EqualTo(Rational.For(1, 6)));
            
            Rational result;
            Assert.That(Rational.TryDiv(one, zero, out result), Is.False, "shouldn't div by zero");
            Assert.That(Rational.TryDiv(seven22, zero, out result), Is.False, "shouldn't div by zero");
        }

        [Test]
        public void LessThanOperator()
        {
            minusInf.ShouldNotBeLessThan (minusInf);
            minusInf.ShouldBeLessThan (plusInf);
            minusInf.ShouldBeLessThan (one);

            zero.ShouldNotBeLessThan(minusInf);
            zero.ShouldBeLessThan(plusInf);
            zero.ShouldBeLessThan(one);
         
            plusInf.ShouldNotBeLessThan (minusInf);
            plusInf.ShouldNotBeLessThan (plusInf);
            plusInf.ShouldNotBeLessThan (one);
        }
    }

    static class RationalTestExtensions
    {
        public static void ShouldBeLessEqualThan(this Rational l, Rational r)
        {
            Assert.IsTrue(l <= r);
        }

        public static void ShouldNotBeLessEqualThan(this Rational l, Rational r)
        {
            Assert.IsFalse(l <= r);
        }

        public static void ShouldBeLessThan(this Rational l, Rational r)
        {
            Assert.IsTrue (l < r);
        }

        public static void ShouldNotBeLessThan(this Rational l, Rational r)
        {
            Assert.IsFalse(l < r);
        }
    }
}