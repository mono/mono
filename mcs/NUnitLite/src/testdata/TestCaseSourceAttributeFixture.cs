// ***********************************************************************
// Copyright (c) 2009 Charlie Poole
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
// ***********************************************************************

using System;
using System.Collections;
using NUnit.Framework;

namespace NUnit.TestData.TestCaseSourceAttributeFixture
{
    [TestFixture]
    public class TestCaseSourceAttributeFixture
    {
        [TestCaseSource("source")]
        public void MethodThrowsExpectedException(int x, int y, int z)
        {
            throw new ArgumentNullException();
        }

        [TestCaseSource("source")]
        public void MethodThrowsWrongException(int x, int y, int z)
        {
            throw new ArgumentException();
        }

        [TestCaseSource("source")]
        public void MethodThrowsNoException(int x, int y, int z)
        {
        }

        [TestCaseSource("source")]
        public void MethodCallsIgnore(int x, int y, int z)
        {
            Assert.Ignore("Ignore this");
        }

        internal static object[] source = new object[] {
            new TestCaseData( 2, 3, 4 ).Throws(typeof(ArgumentNullException)) };

        [TestCaseSource("ignored_source")]
        public void MethodWithIgnoredTestCases(int num)
        {
        }

        [TestCaseSource("explicit_source")]
        public void MethodWithExplicitTestCases(int num)
        {
        }

        internal static IEnumerable ignored_source
        {
            get
            {
                return new object[] {
                    new TestCaseData(1),
                    new TestCaseData(2).Ignore(),
                    new TestCaseData(3).Ignore("Don't Run Me!")
                };
            }
        }

        internal static IEnumerable explicit_source
        {
            get
            {
                return new object[] {
                    new TestCaseData(1),
                    new TestCaseData(2).Explicit(),
                    new TestCaseData(3).Explicit("Connection failing")
                };
            }
        }

#if CLR_2_0 || CLR_4_0
        [TestCaseSource("exception_source")]
        public void MethodWithSourceThrowingException(string lhs, string rhs)
        {
        }

        internal static IEnumerable exception_source
        {
            get
            {
                yield return new TestCaseData("a", "a");
                yield return new TestCaseData("b", "b");

                throw new System.Exception("my message");
            }
        }
#endif
    }
}
