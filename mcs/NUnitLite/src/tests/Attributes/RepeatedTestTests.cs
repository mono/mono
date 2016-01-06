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
using System.Reflection;
using NUnit.Framework.Api;
using NUnit.Framework.Internal;
using NUnit.TestData.RepeatedTestFixture;
using NUnit.TestUtilities;

namespace NUnit.Framework.Attributes
{
    [TestFixture]
    public class RepeatedTestTests
	{
		private MethodInfo successMethod;
		private MethodInfo failOnFirstMethod;
		private MethodInfo failOnThirdMethod;

		[SetUp]
		public void SetUp()
		{
			Type testType = typeof(RepeatSuccessFixture);
			successMethod = testType.GetMethod ("RepeatSuccess");
			testType = typeof(RepeatFailOnFirstFixture);
			failOnFirstMethod = testType.GetMethod("RepeatFailOnFirst");
			testType = typeof(RepeatFailOnThirdFixture);
			failOnThirdMethod = testType.GetMethod("RepeatFailOnThird");
		}

		[Test]
		public void RepeatSuccess()
		{
			Assert.IsNotNull (successMethod);
			RepeatSuccessFixture fixture = new RepeatSuccessFixture();
            ITestResult result = TestBuilder.RunTestFixture(fixture);

            Assert.IsTrue(result.ResultState == ResultState.Success);
			Assert.AreEqual(1, fixture.FixtureSetupCount);
			Assert.AreEqual(1, fixture.FixtureTeardownCount);
			Assert.AreEqual(3, fixture.SetupCount);
			Assert.AreEqual(3, fixture.TeardownCount);
			Assert.AreEqual(3, fixture.Count);
		}

		[Test]
		public void RepeatFailOnFirst()
		{
			Assert.IsNotNull (failOnFirstMethod);
			RepeatFailOnFirstFixture fixture = new RepeatFailOnFirstFixture();
            ITestResult result = TestBuilder.RunTestFixture(fixture);

            Assert.IsFalse(result.ResultState == ResultState.Success);
			Assert.AreEqual(1, fixture.SetupCount);
			Assert.AreEqual(1, fixture.TeardownCount);
			Assert.AreEqual(1, fixture.Count);
		}

		[Test]
		public void RepeatFailOnThird()
		{
			Assert.IsNotNull (failOnThirdMethod);
			RepeatFailOnThirdFixture fixture = new RepeatFailOnThirdFixture();
            ITestResult result = TestBuilder.RunTestFixture(fixture);

            Assert.IsFalse(result.ResultState == ResultState.Success);
			Assert.AreEqual(3, fixture.SetupCount);
			Assert.AreEqual(3, fixture.TeardownCount);
			Assert.AreEqual(3, fixture.Count);
		}

		[Test]
		public void IgnoreWorksWithRepeatedTest()
		{
			RepeatedTestWithIgnore fixture = new RepeatedTestWithIgnore();
            TestBuilder.RunTestFixture(fixture);

			Assert.AreEqual( 0, fixture.SetupCount );
			Assert.AreEqual( 0, fixture.TeardownCount );
			Assert.AreEqual( 0, fixture.Count );
		}

        [Test]
        public void CategoryWorksWithRepeatedTest()
        {
            TestSuite suite = TestBuilder.MakeFixture(typeof(RepeatedTestWithCategory));
            Test test = suite.Tests[0] as Test;
            System.Collections.IList categories = test.Properties["Category"];
            Assert.IsNotNull(categories);
            Assert.AreEqual(1, categories.Count);
            Assert.AreEqual("SAMPLE", categories[0]);
        }
	}
}
#endif