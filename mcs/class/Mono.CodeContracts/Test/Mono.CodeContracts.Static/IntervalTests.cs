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

namespace MonoTests.Mono.CodeContracts {
        [TestFixture (typeof (Interval))]
        class IntervalTests : DomainTestBase<Interval> {
                readonly Interval _1__2 = Interval.For (1, 2);
                readonly Interval _2__4 = Interval.For (2, 4);
                readonly Interval _3__4 = Interval.For (3, 4);
                readonly Interval _1__4 = Interval.For (1, 4);

                readonly Interval zero_to_one = Interval.For (Rational.Zero, Rational.One);
                readonly Interval minus_one_to_zero = Interval.For (Rational.MinusOne, Rational.Zero);

                protected override Interval Top { get { return Interval.TopValue; } }
                protected override Interval Bottom { get { return Interval.BottomValue; } }
                protected override Interval Normal { get { return this._1__2; } }

                [Test]
                public void ConsecutiveIntegers ()
                {
                        Assert.That (Interval.AreConsecutiveIntegers (this._1__2, this._3__4), Is.True);
                        Assert.That (Interval.AreConsecutiveIntegers (this._1__2, this._1__2), Is.False);
                        Assert.That (Interval.AreConsecutiveIntegers (this._3__4, this._1__2), Is.False);
                        Assert.That (Interval.AreConsecutiveIntegers (Interval.For (Rational.For (1, 3)), this._1__2),
                                     Is.False);
                }

                [Test]
                public void OnTheLeftOf ()
                {
                        Assert.IsTrue (this._1__2.OnTheLeftOf (this._3__4));
                        Assert.IsTrue (this._1__2.OnTheLeftOf (this._2__4));

                        Assert.IsFalse (this._2__4.OnTheLeftOf (this._1__2));
                        Assert.IsFalse (this._1__4.OnTheLeftOf (this._1__2));
                        Assert.IsFalse (this._1__2.OnTheLeftOf (this._1__4));
                }

                [Test]
                public void ShouldAddIntervalsByEachBound ()
                {
                        Assert.That (this.Bottom + this._1__2, Is.EqualTo (this.Bottom), "bottom + normal = bottom");
                        Assert.That (this.Bottom + this.Top, Is.EqualTo (this.Bottom), "bottom + top = bottom");
                        Assert.That (this.Bottom + this.Bottom, Is.EqualTo (this.Bottom), "bottom + bottom = bottom");

                        Assert.That (this.Top + this.Top, Is.EqualTo (this.Top), "top + top = top");
                        Assert.That (this.Top + this._1__2, Is.EqualTo (this.Top), "top + normal = top");
                        Assert.That (this.Top + this.Bottom, Is.EqualTo (this.Bottom), "top + bottom = bottom");

                        Assert.That (this._1__2 + this.Bottom, Is.EqualTo (this.Bottom), "normal + bottom = bottom");
                        Assert.That (this._1__2 + this.Top, Is.EqualTo (this.Top), "normal + top = top");
                        Assert.That (this._1__2 + this._3__4, Is.EqualTo (Interval.For (1 + 3, 2 + 4)));
                }

                [Test]
                public void ShouldDivIntervalsByMaxMin ()
                {
                        Assert.That (this.Bottom / this._1__2, Is.EqualTo (this.Bottom), "bottom / normal = bottom");
                        Assert.That (this.Bottom / this.Top, Is.EqualTo (this.Bottom), "bottom / top = bottom");
                        Assert.That (this.Bottom / this.Bottom, Is.EqualTo (this.Bottom), "bottom / bottom = bottom");

                        Assert.That (this.Top / this.Top, Is.EqualTo (this.Top), "top / top = top");
                        Assert.That (this.Top / this._1__2, Is.EqualTo (this.Top), "top / normal = top");
                        Assert.That (this.Top / this.Bottom, Is.EqualTo (this.Bottom), "top / bottom = bottom");

                        Assert.That (this._1__2 / this.Bottom, Is.EqualTo (this.Bottom), "normal / bottom = bottom");
                        Assert.That (this._1__2 / this.Top, Is.EqualTo (this.Top), "normal / top = top");

                        Assert.That (this._1__2 / this.zero_to_one, Is.EqualTo (this.Top), "normal / zeroToOne = top");
                        Assert.That (this._1__2 / this.minus_one_to_zero, Is.EqualTo (this.Top),
                                     "normal / minusOneToZero = top");
                        Assert.That (
                                this._1__2 / this._3__4,
                                Is.EqualTo (Interval.For (Rational.For (1, 4), Rational.For (2, 3))),
                                "normal / normal = normal");
                }

                [Test]
                public void ShouldJoinByInclusion ()
                {
                        Assert.That (this._1__2.Join (this._3__4), Is.EqualTo (Interval.For (1, 4)));
                        Assert.That (this._3__4.Join (this._1__2), Is.EqualTo (Interval.For (1, 4)));
                }

                [Test]
                public void ShouldLessEqualByInclusion ()
                {
                        Assert.That (this._1__2.LessEqual (this._1__2), Is.True);

                        Assert.That (this._1__2.LessEqual (this._1__4), Is.True);
                        Assert.That (this._3__4.LessEqual (this._1__4), Is.True);

                        Assert.That (this._1__2.LessEqual (this._3__4), Is.False);
                        Assert.That (this._3__4.LessEqual (this._1__2), Is.False);

                        Assert.That (this._1__4.LessEqual (this._1__2), Is.False);
                        Assert.That (this._1__4.LessEqual (this._3__4), Is.False);
                }

                [Test]
                public void ShouldMultIntervalsByMaxMin ()
                {
                        Assert.That (this.Bottom * this._1__2, Is.EqualTo (this.Bottom), "bottom * normal = bottom");
                        Assert.That (this.Bottom * this.Top, Is.EqualTo (this.Bottom), "bottom * top = bottom");
                        Assert.That (this.Bottom * this.Bottom, Is.EqualTo (this.Bottom), "bottom * bottom = bottom");

                        Assert.That (this.Top * this.Top, Is.EqualTo (this.Top), "top * top = top");
                        Assert.That (this.Top * this._1__2, Is.EqualTo (this.Top), "top * normal = top");
                        Assert.That (this.Top * this.Bottom, Is.EqualTo (this.Bottom), "top * bottom = bottom");

                        Assert.That (this._1__2 * this.Bottom, Is.EqualTo (this.Bottom), "normal * bottom = bottom");
                        Assert.That (this._1__2 * this.Top, Is.EqualTo (this.Top), "normal * top = top");

                        Assert.That (this._1__2 * this._3__4, Is.EqualTo (Interval.For (3, 8)),
                                     "normal * normal = normal");
                }

                [Test]
                public void ShouldSubIntervalsByMaxMin ()
                {
                        Assert.That (this.Bottom - this._1__2, Is.EqualTo (this.Bottom), "bottom - normal = bottom");
                        Assert.That (this.Bottom - this.Top, Is.EqualTo (this.Bottom), "bottom - top = bottom");
                        Assert.That (this.Bottom - this.Bottom, Is.EqualTo (this.Bottom), "bottom - bottom = bottom");

                        Assert.That (this.Top - this.Top, Is.EqualTo (this.Top), "top - top = top");
                        Assert.That (this.Top - this._1__2, Is.EqualTo (this.Top), "top - normal = top");
                        Assert.That (this.Top - this.Bottom, Is.EqualTo (this.Bottom), "top - bottom = bottom");

                        Assert.That (this._1__2 - this.Bottom, Is.EqualTo (this.Bottom), "normal - bottom = bottom");
                        Assert.That (this._1__2 - this.Top, Is.EqualTo (this.Top), "normal - top = top");
                        Assert.That (this._1__2 - this._3__4, Is.EqualTo (Interval.For (1 - 4, 2 - 3)),
                                     "normal - normal = normal");
                }

                [Test]
                public void ShouldUnaryMinusIntervals ()
                {
                        Assert.That (-this.Bottom, Is.EqualTo (this.Bottom), "-bottom = bottom");
                        Assert.That (-this.Top, Is.EqualTo (this.Top), "-top = top");
                        Assert.That (-this._1__2, Is.EqualTo (Interval.For (-2, -1)), "normal: -[l,r] = [-r,-l]");
                }
        }
}