// ***********************************************************************
// Copyright (c) 2007 Charlie Poole
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
using NUnit.Framework;

namespace NUnit.TestData.AssertIgnoreData
{
	[TestFixture]
	public class IgnoredTestCaseFixture
	{
        [Test]
        public void CallsIgnore()
        {
            Assert.Ignore("Ignore me");
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void CallsIgnoreWithExpectedException()
        {
            Assert.Ignore("Ignore me");
        }
    }

	[TestFixture]
	public class IgnoredTestSuiteFixture
	{
		[TestFixtureSetUp]
		public void FixtureSetUp()
		{
			Assert.Ignore("Ignore this fixture");
		}

		[Test]
		public void ATest()
		{
		}

		[Test]
		public void AnotherTest()
		{
		}
	}

	[TestFixture]
	public class IgnoreInSetUpFixture
	{
		[SetUp]
		public void SetUp()
		{
			Assert.Ignore( "Ignore this test" );
		}

		[Test]
		public void Test1()
		{
		}

		[Test]
		public void Test2()
		{
		}
	}
}
