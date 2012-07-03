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
        [Test]
        public void ShouldHaveInfinitiesAndNormalValuesWhichArentEqual()
        {
            Rational infPlus = Rational.PlusInfinity;
            Rational infMinus = Rational.MinusInfinity;
            Rational zero = Rational.For (0L);
            Rational zero1 = Rational.For (0L);
            Rational one = Rational.For (1L);

            Assert.IsFalse (infPlus == infMinus);
            Assert.IsFalse (infPlus == zero);
            Assert.IsFalse (infMinus == zero);
            Assert.IsTrue (zero == zero1);
            Assert.IsFalse (one == zero);
        }

        [Test]
        public void ShouldBeEqualByModuloOfDenominator ()
        {
            Rational threeFourth = Rational.For (3,4);
            Rational sixEighth = Rational.For(6, 8);

            Assert.IsTrue (threeFourth == sixEighth);
        }
    }
}