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

namespace Test
{
    [TestFixture]
    public class AbstractDomainExtensionsTests
    {
        private readonly FlatDomain<int> top = FlatDomain<int>.TopValue;
        private readonly FlatDomain<int> bottom = FlatDomain<int>.BottomValue;
        private readonly FlatDomain<int> normal = 1;

        private const bool dummy = false;

        [Test]
        public void TestCasesIsNormal ()
        {
            Assert.That (top.IsNormal(), Is.False);
            Assert.That (bottom.IsNormal(), Is.False);
            Assert.That (normal.IsNormal(), Is.True);
        }

        [Test]
        public void TestCasesTrivialMeet()
        {
            AssertMeetResultFor (top, top, true, (r) => r.IsTop);
            AssertMeetResultFor (top, bottom, true, (r) => r.IsBottom);
            AssertMeetResultFor (top, normal, true, (r) => r.IsNormal);
            
            AssertMeetResultFor (bottom, top, true, (r) => r.IsBottom);
            AssertMeetResultFor (bottom, bottom, true, (r) => r.IsBottom);
            AssertMeetResultFor (bottom, normal, true, (r) => r.IsBottom);

            AssertMeetResultFor (normal, top, true, (r) => r.IsNormal);
            AssertMeetResultFor (normal, bottom, true, (r) => r.IsBottom);
            AssertMeetResultFor (normal, normal, false, (r) => dummy);
        }

        [Test]
        public void TestCasesTrivialJoin()
        {
            AssertJoinResultFor(top, top, true, (r) => r.IsTop);
            AssertJoinResultFor(top, bottom, true, (r) => r.IsTop);
            AssertJoinResultFor(top, normal, true, (r) => r.IsTop);

            AssertJoinResultFor(bottom, top, true, (r) => r.IsTop);
            AssertJoinResultFor(bottom, bottom, true, (r) => r.IsBottom);
            AssertJoinResultFor(bottom, normal, true, (r) => r.IsNormal);

            AssertJoinResultFor(normal, top, true, (r) => r.IsTop);
            AssertJoinResultFor(normal, bottom, true, (r) => r.IsNormal);
            AssertJoinResultFor(normal, normal, false, (r) => dummy);
        }

        [Test]
        public void TestCasesTrivialLessEqual()
        {
            AssertLessEqualResultFor(top,    top,    true,  true);
            AssertLessEqualResultFor(top,    bottom, true,  false);
            AssertLessEqualResultFor(top,    normal, true,  false);

            AssertLessEqualResultFor(bottom, top,    true,  true);
            AssertLessEqualResultFor(bottom, bottom, true,  true);
            AssertLessEqualResultFor(bottom, normal, true,  true);

            AssertLessEqualResultFor(normal, top,    true,  true);
            AssertLessEqualResultFor(normal, bottom, true,  false);
            AssertLessEqualResultFor(normal, normal, false, dummy);
        }

        /// <summary>
        /// Checks that Meet(l, r, out result) == trivialSuccess && check(result);
        /// </summary>
        private static void AssertMeetResultFor(FlatDomain<int> l, FlatDomain<int> r, bool trivialSuccess, Func<FlatDomain<int>, bool> check)
        {
            FlatDomain<int> result;
            Assert.That(l.TryTrivialMeet(r, out result), Is.EqualTo (trivialSuccess));
            if (trivialSuccess)
                Assert.That(check(result));
        }

        /// <summary>
        /// Checks that Join(l, r, out result) == trivialSuccess && check(result);
        /// </summary>
        private static void AssertJoinResultFor(FlatDomain<int> l, FlatDomain<int> r, bool trivialSuccess, Func<FlatDomain<int>, bool> check)
        {
            FlatDomain<int> result;
            Assert.That(l.TryTrivialJoin(r, out result), Is.EqualTo(trivialSuccess));
            if (trivialSuccess)
                Assert.That(check(result));
        }

        /// <summary>
        /// Checks that LessEqual(l, r, out result) == trivialSuccess && result == resultCheck;
        /// </summary>
        private static void AssertLessEqualResultFor(FlatDomain<int> l, FlatDomain<int> r, bool trivialSuccess, bool resultCheck)
        {
            bool result;
            Assert.That(l.TryTrivialLessEqual(r, out result), Is.EqualTo(trivialSuccess));
            if (trivialSuccess)
                Assert.That(result, Is.EqualTo(resultCheck));
        }
    }
}