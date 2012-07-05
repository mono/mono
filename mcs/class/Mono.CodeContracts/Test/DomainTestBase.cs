using System;

using Mono.CodeContracts.Static.Lattices;

using NUnit.Framework;

namespace Test
{
    [TestFixture]
    internal abstract class DomainTestBase<T>
        where T : IAbstractDomain<T>
    {
        protected abstract T top { get; }
        protected abstract T bot { get; }
        protected abstract T normal { get; }

        [Test]
        public void IsNormal()
        {
            Assert.That(top.IsNormal(), Is.False);
            Assert.That(bot.IsNormal(), Is.False);
            Assert.That(normal.IsNormal(), Is.True);
        }

        [Test]
        public void Meet()
        {
            AssertMeetResultFor (top, top, r => r.IsTop);
            AssertMeetResultFor (top, bot, r => r.IsBottom);
            AssertMeetResultFor (top, normal, r => r.IsNormal ());

            AssertMeetResultFor (bot, top, r => r.IsBottom);
            AssertMeetResultFor (bot, bot, r => r.IsBottom);
            AssertMeetResultFor (bot, normal, r => r.IsBottom);

            AssertMeetResultFor (normal, top, r => r.IsNormal ());
            AssertMeetResultFor (normal, bot, r => r.IsBottom);
        }

        [Test]
        public void Join()
        {
            AssertJoinResultFor(top, top, (r) => r.IsTop);
            AssertJoinResultFor(top, bot, (r) => r.IsTop);
            AssertJoinResultFor(top, normal, (r) => r.IsTop);

            AssertJoinResultFor(bot, top, (r) => r.IsTop);
            AssertJoinResultFor(bot, bot, (r) => r.IsBottom);
            AssertJoinResultFor(bot, normal, (r) => r.IsNormal ());

            AssertJoinResultFor(normal, top, (r) => r.IsTop);
            AssertJoinResultFor(normal, bot, (r) => r.IsNormal ());
        }

        [Test]
        public void LessEqual()
        {
            AssertLessEqualResultFor(top, top, true);
            AssertLessEqualResultFor(top, bot, false);
            AssertLessEqualResultFor(top, normal, false);

            AssertLessEqualResultFor(bot, top, true);
            AssertLessEqualResultFor(bot, bot, true);
            AssertLessEqualResultFor(bot, normal, true);

            AssertLessEqualResultFor(normal, top, true);
            AssertLessEqualResultFor(normal, bot, false);
        }

        /// <summary>
        /// Checks that Meet(l, r, out result) == trivialSuccess && check(result);
        /// </summary>
        private static void AssertMeetResultFor(T l, T r, Func<T, bool> check)
        {
            T result = l.Meet (r);
            Assert.That (check (result));
        }

        /// <summary>
        /// Checks that Join(l, r, out result) == trivialSuccess && check(result);
        /// </summary>
        private static void AssertJoinResultFor(T l, T r, Func<T, bool> check)
        {
            bool weaker;
            T result = l.Join (r, true, out weaker);
            Assert.That (check (result));
        }

        /// <summary>
        /// Checks that LessEqual(l, r, out result) == trivialSuccess && result == expectedResult;
        /// </summary>
        private static void AssertLessEqualResultFor(T l, T r, bool expectedResult)
        {
            bool result = l.LessEqual (r);
            Assert.That (result, Is.EqualTo (expectedResult));
        }
    }

    [TestFixture(Category = "FlatDomainTrivialTests")]
    class FlatDomainTests : DomainTestBase<FlatDomain<int>>
    {
        protected override FlatDomain<int> top { get { return FlatDomain<int>.TopValue; } }

        protected override FlatDomain<int> bot { get { return FlatDomain<int>.BottomValue; } }

        protected override FlatDomain<int> normal { get { return 1; } }
    }
}