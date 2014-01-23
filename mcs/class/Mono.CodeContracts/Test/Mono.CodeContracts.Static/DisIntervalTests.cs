// 
// DisIntervalTests.cs
// 
// Authors:
//	Alexander Chebaturkin (chebaturkin@gmail.com)
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
using Mono.CodeContracts.Static.DataStructures;

using NUnit.Framework;

namespace MonoTests.Mono.CodeContracts {
        [TestFixture]
        class DisIntervalTests : DomainTestBase<DisInterval> {
                protected override DisInterval Top { get { return DisInterval.TopValue; } }
                protected override DisInterval Bottom { get { return DisInterval.BottomValue; } }
                protected override DisInterval Normal { get { return DisInterval.For (this._1__2); } }

                readonly Interval _0__1 = Interval.For (0, 1);
                readonly Interval _0__4 = Interval.For (0, 4);
                readonly Interval _1__2 = Interval.For (1, 2);
                readonly Interval _1__3 = Interval.For (1, 3);
                readonly Interval _1__4 = Interval.For (1, 4);
                readonly Interval _1__5 = Interval.For (1, 5);
                readonly Interval _2__4 = Interval.For (2, 4);
                readonly Interval _2__5 = Interval.For (2, 5);
                readonly Interval _3__4 = Interval.For (3, 4);

                static Interval JoinAll (params Interval[] intervals)
                {
                        return DisInterval.JoinAll (Sequence<Interval>.From (intervals));
                }

                [Test]
                public void ForSingleInterval ()
                {
                        DisInterval disInterval = DisInterval.For (this._1__2);

                        Assert.That (disInterval.AsInterval, Is.EqualTo (this._1__2));
                }

                [Test]
                public void JoinAllForIntervals ()
                {
                        Assert.That (JoinAll (this._1__2), Is.EqualTo (this._1__2));
                        Assert.That (JoinAll (this._1__2, this._3__4), Is.EqualTo (this._1__4));
                        Assert.That (JoinAll (), Is.EqualTo (Interval.TopValue));
                }

                [Test]
                public void NormalizeTests ()
                {
                        bool isBottom;
                        Assert.AreEqual (
                                DisInterval.Normalize (Sequence<Interval>.From (this._1__2, this._3__4), out isBottom),
                                Sequence<Interval>.From (this._1__4));

                        Assert.AreEqual (
                                DisInterval.Normalize (Sequence<Interval>.From (this._1__4, this._1__2), out isBottom),
                                Sequence<Interval>.From (this._1__4));

                        Assert.AreEqual (
                                DisInterval.Normalize (Sequence<Interval>.From (this._1__2, this._1__4), out isBottom),
                                Sequence<Interval>.From (this._1__4));

                        Assert.AreEqual (
                                DisInterval.Normalize (Sequence<Interval>.From (this._1__4, this._2__4), out isBottom),
                                Sequence<Interval>.From (this._1__4));

                        Assert.AreEqual (
                                DisInterval.Normalize (Sequence<Interval>.From (this._1__3, this._2__5), out isBottom),
                                Sequence<Interval>.From (this._1__5));

                        Assert.AreEqual (
                                DisInterval.Normalize (Sequence<Interval>.From (Interval.BottomValue, Interval.BottomValue), out isBottom),
                                Sequence<Interval>.Empty);
                        Assert.IsTrue (isBottom);

                        Assert.AreEqual (
                                DisInterval.Normalize (Sequence<Interval>.From (Interval.BottomValue), out isBottom),
                                Sequence<Interval>.Empty);
                        Assert.IsTrue (isBottom);
                }

                [Test]
                public void ShouldHaveAddOperation ()
                {
                        Assert.That (DisInterval.For (this._1__2) + DisInterval.For (this._3__4),
                                     Is.EqualTo (DisInterval.For (Interval.For (4, 6))));
                }

                [Test]
                public void ShouldHaveJoinOperation ()
                {
                        DisInterval left = DisInterval.For (this._0__1).Join (DisInterval.For (this._3__4));
                        DisInterval right = DisInterval.For (this._1__2).Join (DisInterval.For (this._1__4));
                        Assert.That (left.Join (right), Is.EqualTo (DisInterval.For (this._0__4)));
                }

                [Test]
                public void ShouldHaveMeetOperation ()
                {
                        Assert.That (DisInterval.For (this._1__4).Meet (DisInterval.For (this._1__2)),
                                     Is.EqualTo (DisInterval.For (this._1__2)));
                }

                [Test]
                public void ShouldHaveSubOperation ()
                {
                        Assert.That (DisInterval.For (this._1__2) - DisInterval.For (this._3__4),
                                     Is.EqualTo (DisInterval.For (Interval.For (-3, -1))));
                }
        }
}