// 
// DomainTestBase.cs
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