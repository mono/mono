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
#if false
using System;
using NUnit.Framework;

namespace NUnit.TestData.RepeatedTestFixture
{
	[TestFixture]
	public class RepeatingTestsBase
	{
		private int fixtureSetupCount;
		private int fixtureTeardownCount;
		private int setupCount;
		private int teardownCount;
		protected int count;

		[TestFixtureSetUp]
		public void FixtureSetUp()
		{
			fixtureSetupCount++;
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			fixtureTeardownCount++;
		}

		[SetUp]
		public void SetUp()
		{
			setupCount++;
		}

		[TearDown]
		public void TearDown()
		{
			teardownCount++;
		}

		public int FixtureSetupCount
		{
			get { return fixtureSetupCount; }
		}
		public int FixtureTeardownCount
		{
			get { return fixtureTeardownCount; }
		}
		public int SetupCount
		{
			get { return setupCount; }
		}
		public int TeardownCount
		{
			get { return teardownCount; }
		}
		public int Count
		{
			get { return count; }
		}
	}

	public class RepeatSuccessFixture : RepeatingTestsBase
	{
		[Test, Repeat(3)]
		public void RepeatSuccess()
		{
			count++;
			Assert.IsTrue (true);
		}
	}

	public class RepeatFailOnFirstFixture : RepeatingTestsBase
	{
		[Test, Repeat(3)]
		public void RepeatFailOnFirst()
		{
			count++;
			Assert.IsFalse (true);
		}
	}

	public class RepeatFailOnThirdFixture : RepeatingTestsBase
	{
		[Test, Repeat(3)]
		public void RepeatFailOnThird()
		{
			count++;

			if (count == 3)
				Assert.IsTrue (false);
		}
	}

    public class RepeatedTestWithIgnore : RepeatingTestsBase
    {
        [Test, Repeat(3), Ignore("Ignore this test")]
        public void RepeatShouldIgnore()
        {
            Assert.Fail("Ignored test executed");
        }
    }

    public class RepeatedTestWithCategory : RepeatingTestsBase
    {
        [Test, Repeat(3), Category("SAMPLE")]
        public void TestWithCategory()
        {
            count++;
            Assert.IsTrue(true);
        }
    }
}
#endif