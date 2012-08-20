// 
// AbstractDomainExtensionsTests.cs
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

using System;

using Mono.CodeContracts.Static.Lattices;

using NUnit.Framework;

namespace MonoTests.Mono.CodeContracts {
        [TestFixture]
        public class AbstractDomainExtensionsTests {
                readonly FlatDomain<int> top = FlatDomain<int>.TopValue;
                readonly FlatDomain<int> bottom = FlatDomain<int>.BottomValue;
                readonly FlatDomain<int> normal = 1;

                const bool Dummy = false;

                /// <summary>
                /// Checks that Meet(l, r, out result) == trivialSuccess && check(result);
                /// </summary>
                static void AssertMeetResultFor (FlatDomain<int> l, FlatDomain<int> r, bool trivialSuccess, Func<FlatDomain<int>, bool> check)
                {
                        FlatDomain<int> result;
                        Assert.That (l.TryTrivialMeet (r, out result), Is.EqualTo (trivialSuccess));
                        if (trivialSuccess)
                                Assert.That (check (result));
                }

                /// <summary>
                /// Checks that Join(l, r, out result) == trivialSuccess && check(result);
                /// </summary>
                static void AssertJoinResultFor (FlatDomain<int> l, FlatDomain<int> r, bool trivialSuccess, Func<FlatDomain<int>, bool> check)
                {
                        FlatDomain<int> result;
                        Assert.That (l.TryTrivialJoin (r, out result), Is.EqualTo (trivialSuccess));
                        if (trivialSuccess)
                                Assert.That (check (result));
                }

                /// <summary>
                /// Checks that LessEqual(l, r, out result) == trivialSuccess && result == resultCheck;
                /// </summary>
                static void AssertLessEqualResultFor (FlatDomain<int> l, FlatDomain<int> r, bool trivialSuccess, bool resultCheck)
                {
                        bool result;
                        Assert.That (l.TryTrivialLessEqual (r, out result), Is.EqualTo (trivialSuccess));
                        if (trivialSuccess)
                                Assert.That (result, Is.EqualTo (resultCheck));
                }

                [Test]
                public void TestCasesIsNormal ()
                {
                        Assert.That (this.top.IsNormal (), Is.False);
                        Assert.That (this.bottom.IsNormal (), Is.False);
                        Assert.That (this.normal.IsNormal (), Is.True);
                }

                [Test]
                public void TestCasesTrivialJoin ()
                {
                        AssertJoinResultFor (this.top, this.top, true, (r) => r.IsTop);
                        AssertJoinResultFor (this.top, this.bottom, true, (r) => r.IsTop);
                        AssertJoinResultFor (this.top, this.normal, true, (r) => r.IsTop);

                        AssertJoinResultFor (this.bottom, this.top, true, (r) => r.IsTop);
                        AssertJoinResultFor (this.bottom, this.bottom, true, (r) => r.IsBottom);
                        AssertJoinResultFor (this.bottom, this.normal, true, (r) => r.IsNormal ());

                        AssertJoinResultFor (this.normal, this.top, true, (r) => r.IsTop);
                        AssertJoinResultFor (this.normal, this.bottom, true, (r) => r.IsNormal ());
                        AssertJoinResultFor (this.normal, this.normal, false, (r) => Dummy);
                }

                [Test]
                public void TestCasesTrivialLessEqual ()
                {
                        AssertLessEqualResultFor (this.top, this.top, true, true);
                        AssertLessEqualResultFor (this.top, this.bottom, true, false);
                        AssertLessEqualResultFor (this.top, this.normal, true, false);

                        AssertLessEqualResultFor (this.bottom, this.top, true, true);
                        AssertLessEqualResultFor (this.bottom, this.bottom, true, true);
                        AssertLessEqualResultFor (this.bottom, this.normal, true, true);

                        AssertLessEqualResultFor (this.normal, this.top, true, true);
                        AssertLessEqualResultFor (this.normal, this.bottom, true, false);
                        AssertLessEqualResultFor (this.normal, this.normal, false, Dummy);
                }

                [Test]
                public void TestCasesTrivialMeet ()
                {
                        AssertMeetResultFor (this.top, this.top, true, (r) => r.IsTop);
                        AssertMeetResultFor (this.top, this.bottom, true, (r) => r.IsBottom);
                        AssertMeetResultFor (this.top, this.normal, true, (r) => r.IsNormal ());

                        AssertMeetResultFor (this.bottom, this.top, true, (r) => r.IsBottom);
                        AssertMeetResultFor (this.bottom, this.bottom, true, (r) => r.IsBottom);
                        AssertMeetResultFor (this.bottom, this.normal, true, (r) => r.IsBottom);

                        AssertMeetResultFor (this.normal, this.top, true, (r) => r.IsNormal ());
                        AssertMeetResultFor (this.normal, this.bottom, true, (r) => r.IsBottom);
                        AssertMeetResultFor (this.normal, this.normal, false, (r) => Dummy);
                }
        }
}