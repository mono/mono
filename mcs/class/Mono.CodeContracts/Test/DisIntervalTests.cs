using System;
using System.Collections.Generic;

using Mono.CodeContracts.Static.Analysis.Numerical;

using NUnit.Framework;

namespace Test
{
    [TestFixture]
    internal class DisIntervalTests : DomainTestBase<DisInterval>
    {
        protected override DisInterval top { get { return DisInterval.TopValue; } }
        protected override DisInterval bot { get { return DisInterval.BottomValue; } }
        protected override DisInterval normal { get { return DisInterval.For (_1__2); } }

        private readonly Interval _0__1 = Interval.For (0, 1);
        private readonly Interval _0__4 = Interval.For (0, 4);
        private readonly Interval _1__2 = Interval.For (1, 2);
        private readonly Interval _1__3 = Interval.For (1, 3);
        private readonly Interval _1__4 = Interval.For (1, 4);
        private readonly Interval _1__5 = Interval.For (1, 5);

        private readonly Interval _2__4 = Interval.For (2, 4);
        private readonly Interval _2__5 = Interval.For (2, 5);

        private readonly Interval _3__4 = Interval.For (3, 4);


        [Test]
        public void ForSingleInterval ()
        {
            DisInterval disInterval = DisInterval.For (_1__2);

            Assert.That (disInterval.AsInterval, Is.EqualTo (_1__2));
        }

        [Test]
        public void JoinAllForIntervals()
        {
            Assert.That (JoinAll (_1__2),        Is.EqualTo (_1__2));
            Assert.That (JoinAll (_1__2, _3__4), Is.EqualTo (_1__4));
            Assert.That (JoinAll (),             Is.EqualTo (Interval.TopValue));
        }

        private Interval JoinAll (params Interval[] intervals)
        {
            return DisInterval.JoinAll(new List<Interval>(intervals));
        }

        [Test]
        public void NormalizeTests()
        {
            bool isBottom;
            CollectionAssert.AreEqual (
                DisInterval.Normalize (new List<Interval> (new[] { _1__2, _3__4 }), out isBottom), new[] { _1__4 });

            CollectionAssert.AreEqual(
                DisInterval.Normalize(new List<Interval>(new[] { _1__4, _1__2 }), out isBottom), new[] { _1__4 });

            CollectionAssert.AreEqual(
               DisInterval.Normalize(new List<Interval>(new[] { _1__2, _1__4 }), out isBottom), new[] { _1__4 });

            CollectionAssert.AreEqual(
               DisInterval.Normalize(new List<Interval>(new[] { _1__4, _2__4 }), out isBottom), new[] { _1__4 });
            
            CollectionAssert.AreEqual(
               DisInterval.Normalize(new List<Interval>(new[] { _1__3, _2__5 }), out isBottom), new[] { _1__5 });

            CollectionAssert.AreEqual(
               DisInterval.Normalize(new List<Interval>(new[] { Interval.BottomValue, Interval.BottomValue }), out isBottom), new Interval[0] );
            Assert.IsTrue (isBottom);

            CollectionAssert.AreEqual(
               DisInterval.Normalize(new List<Interval>(new[] { Interval.BottomValue }), out isBottom), new Interval[0]);
            Assert.IsTrue(isBottom);
        }

        [Test]
        public void ShouldHaveAddOperation()
        {
            Assert.That (DisInterval.For(_1__2) + DisInterval.For(_3__4), Is.EqualTo (DisInterval.For (Interval.For (4, 6))));
        }
        
        [Test]
        public void ShouldHaveSubOperation()
        {
            Assert.That(DisInterval.For(_1__2) - DisInterval.For(_3__4), Is.EqualTo(DisInterval.For(Interval.For(-3, -1))));
        }

        [Test]
        public void ShouldHaveMeetOperation ()
        {
            Assert.That (DisInterval.For(_1__4).Meet(DisInterval.For(_1__2)), Is.EqualTo (DisInterval.For(_1__2)));
        }

        [Test]
        public void ShouldHaveJoinOperation()
        {
            var left = DisInterval.For (_0__1).Join (DisInterval.For (_3__4));
            var right = DisInterval.For (_1__2).Join (DisInterval.For (_1__4));
            Assert.That(left.Join(right), Is.EqualTo(DisInterval.For(_0__4)));
        }
    }
}

