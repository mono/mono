// 
// IntervalTests.cs
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
    public class IntervalTests
    {
        private readonly Interval l = Interval.For(1, 2);
        private readonly Interval r = Interval.For(3, 4);
        private readonly Interval bot = Interval.BottomValue;
        private readonly Interval top = Interval.TopValue;
        private readonly Interval zeroToOne = Interval.For (Rational.Zero, Rational.One);
        private readonly Interval minusOneToZero = Interval.For (Rational.MinusOne, Rational.Zero);

        [Test]
        public void ShouldAddIntervalsByEachBound()
        {
            Assert.That (bot + l, Is.EqualTo (bot), "bottom + normal = bottom");
            Assert.That (bot + top, Is.EqualTo (bot), "bottom + top = bottom");
            Assert.That (bot + bot, Is.EqualTo (bot), "bottom + bottom = bottom");

            Assert.That (top + top, Is.EqualTo (top), "top + top = top");
            Assert.That (top + l, Is.EqualTo (top), "top + normal = top");
            Assert.That (top + bot, Is.EqualTo (bot), "top + bottom = bottom");

            Assert.That (l + bot, Is.EqualTo (bot), "normal + bottom = bottom");
            Assert.That (l + top, Is.EqualTo (top), "normal + top = top");
            Assert.That (l + r, Is.EqualTo (Interval.For (1 + 3, 2 + 4)));
        }

        [Test]
        public void ShouldSubIntervalsByMaxMin()
        {
            Assert.That (bot - l, Is.EqualTo (bot),   "bottom - normal = bottom");
            Assert.That (bot - top, Is.EqualTo (bot), "bottom - top = bottom");
            Assert.That (bot - bot, Is.EqualTo (bot), "bottom - bottom = bottom");

            Assert.That (top - top, Is.EqualTo (top), "top - top = top");
            Assert.That (top - l, Is.EqualTo (top),   "top - normal = top");
            Assert.That (top - bot, Is.EqualTo (bot), "top - bottom = bottom");

            Assert.That (l - bot, Is.EqualTo (bot), "normal - bottom = bottom");
            Assert.That (l - top, Is.EqualTo (top), "normal - top = top");
            Assert.That (l - r, Is.EqualTo (Interval.For (1 - 4, 2 - 3)), "normal - normal = normal");
        }

        [Test]
        public void ShouldDivIntervalsByMaxMin()
        {
            Assert.That (bot / l, Is.EqualTo (bot), "bottom / normal = bottom");
            Assert.That (bot / top, Is.EqualTo (bot), "bottom / top = bottom");
            Assert.That (bot / bot, Is.EqualTo (bot), "bottom / bottom = bottom");

            Assert.That (top / top, Is.EqualTo (top), "top / top = top");
            Assert.That (top / l, Is.EqualTo (top), "top / normal = top");
            Assert.That (top / bot, Is.EqualTo (bot), "top / bottom = bottom");

            Assert.That (l / bot, Is.EqualTo (bot), "normal / bottom = bottom");
            Assert.That (l / top, Is.EqualTo (top), "normal / top = top");

            Assert.That (l / zeroToOne, Is.EqualTo (top), "normal / zeroToOne = top");
            Assert.That (l / minusOneToZero, Is.EqualTo (top), "normal / minusOneToZero = top");
            Assert.That (l / r, 
                Is.EqualTo (Interval.For (Rational.For (1, 4), Rational.For (2, 3))), "normal / normal = normal");
        }

        [Test]
        public void ShouldMultIntervalsByMaxMin()
        {
            Assert.That(bot * l, Is.EqualTo(bot),   "bottom * normal = bottom");
            Assert.That(bot * top, Is.EqualTo(bot), "bottom * top = bottom");
            Assert.That(bot * bot, Is.EqualTo(bot), "bottom * bottom = bottom");

            Assert.That(top * top, Is.EqualTo(top), "top * top = top");
            Assert.That(top * l, Is.EqualTo(top),   "top * normal = top");
            Assert.That(top * bot, Is.EqualTo(bot), "top * bottom = bottom");

            Assert.That(l * bot, Is.EqualTo(bot), "normal * bottom = bottom");
            Assert.That(l * top, Is.EqualTo(top), "normal * top = top");
            
            Assert.That(l * r, Is.EqualTo(Interval.For(3,8)), "normal * normal = normal");
        }

        [Test]
        public void ShouldUnaryMinusIntervals()
        {
            Assert.That(-bot, Is.EqualTo(bot), "-bottom = bottom");
            Assert.That(-top, Is.EqualTo(top), "-top = top");
            Assert.That(-l,   Is.EqualTo(Interval.For (-2,-1)), "normal: -[l,r] = [-r,-l]");
        }
    }
}