using System;

using Mono.CodeContracts.Static.Lattices;

using NUnit.Framework;

namespace MonoTests.Mono.CodeContracts {
        [TestFixture]
        abstract class DomainTestBase<T> where T : IAbstractDomain<T> {
                protected abstract T Top { get; }
                protected abstract T Bottom { get; }
                protected abstract T Normal { get; }

                /// <summary>
                /// Checks that Meet(l, r, out result) == trivialSuccess && check(result);
                /// </summary>
                static void AssertMeetResultFor (T l, T r, Func<T, bool> check)
                {
                        T result = l.Meet (r);
                        Assert.That (check (result));
                }

                /// <summary>
                /// Checks that Join(l, r, out result) == trivialSuccess && check(result);
                /// </summary>
                static void AssertJoinResultFor (T l, T r, Func<T, bool> check)
                {
                        bool weaker;
                        T result = l.Join (r, true, out weaker);
                        Assert.That (check (result));
                }

                /// <summary>
                /// Checks that LessEqual(l, r, out result) == trivialSuccess && result == expectedResult;
                /// </summary>
                static void AssertLessEqualResultFor (T l, T r, bool expectedResult)
                {
                        bool result = l.LessEqual (r);
                        Assert.That (result, Is.EqualTo (expectedResult));
                }

                [Test]
                public void IsNormal ()
                {
                        Assert.That (this.Top.IsNormal (), Is.False);
                        Assert.That (this.Bottom.IsNormal (), Is.False);
                        Assert.That (this.Normal.IsNormal (), Is.True);
                }

                [Test]
                public void Join ()
                {
                        AssertJoinResultFor (this.Top, this.Top, (r) => r.IsTop);
                        AssertJoinResultFor (this.Top, this.Bottom, (r) => r.IsTop);
                        AssertJoinResultFor (this.Top, this.Normal, (r) => r.IsTop);

                        AssertJoinResultFor (this.Bottom, this.Top, (r) => r.IsTop);
                        AssertJoinResultFor (this.Bottom, this.Bottom, (r) => r.IsBottom);
                        AssertJoinResultFor (this.Bottom, this.Normal, (r) => r.IsNormal ());

                        AssertJoinResultFor (this.Normal, this.Top, (r) => r.IsTop);
                        AssertJoinResultFor (this.Normal, this.Bottom, (r) => r.IsNormal ());
                }

                [Test]
                public void LessEqual ()
                {
                        AssertLessEqualResultFor (this.Top, this.Top, true);
                        AssertLessEqualResultFor (this.Top, this.Bottom, false);
                        AssertLessEqualResultFor (this.Top, this.Normal, false);

                        AssertLessEqualResultFor (this.Bottom, this.Top, true);
                        AssertLessEqualResultFor (this.Bottom, this.Bottom, true);
                        AssertLessEqualResultFor (this.Bottom, this.Normal, true);

                        AssertLessEqualResultFor (this.Normal, this.Top, true);
                        AssertLessEqualResultFor (this.Normal, this.Bottom, false);
                }

                [Test]
                public void Meet ()
                {
                        AssertMeetResultFor (this.Top, this.Top, r => r.IsTop);
                        AssertMeetResultFor (this.Top, this.Bottom, r => r.IsBottom);
                        AssertMeetResultFor (this.Top, this.Normal, r => r.IsNormal ());

                        AssertMeetResultFor (this.Bottom, this.Top, r => r.IsBottom);
                        AssertMeetResultFor (this.Bottom, this.Bottom, r => r.IsBottom);
                        AssertMeetResultFor (this.Bottom, this.Normal, r => r.IsBottom);

                        AssertMeetResultFor (this.Normal, this.Top, r => r.IsNormal ());
                        AssertMeetResultFor (this.Normal, this.Bottom, r => r.IsBottom);
                }
        }
}