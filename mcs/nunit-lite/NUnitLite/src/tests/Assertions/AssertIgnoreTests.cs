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
using NUnit.Framework.Api;
using NUnit.Framework.Internal;
using NUnit.TestData.AssertIgnoreData;
using NUnit.TestUtilities;

namespace NUnit.Framework.Assertions
{
    /// <summary>
    /// Tests of IgnoreException and Assert.Ignore
    /// </summary>
    [TestFixture]
    public class AssertIgnoreTests
    {
        [Test, ExpectedException(typeof(IgnoreException))]
        public void ThrowsIgnoreException()
        {
            Assert.Ignore();
        }

        [Test, ExpectedException(typeof(IgnoreException), ExpectedMessage = "MESSAGE")]
        public void ThrowsIgnoreExceptionWithMessage()
        {
            Assert.Ignore("MESSAGE");
        }

        [Test, ExpectedException(typeof(IgnoreException), ExpectedMessage = "MESSAGE: 2+2=4")]
        public void ThrowsIgnoreExceptionWithMessageAndArgs()
        {
            Assert.Ignore("MESSAGE: {0}+{1}={2}", 2, 2, 4);
        }

        [Test]
        public void IgnoreWorksForTestCase()
        {
            Type fixtureType = typeof(IgnoredTestCaseFixture);
            ITestResult result = TestBuilder.RunTestCase(fixtureType, "CallsIgnore");
            Assert.AreEqual(ResultState.Ignored, result.ResultState);
            Assert.AreEqual("Ignore me", result.Message);
        }

        [Test]
        public void IgnoreTakesPrecedenceOverExpectedException()
        {
            Type fixtureType = typeof(IgnoredTestCaseFixture);
            ITestResult result = TestBuilder.RunTestCase(fixtureType, "CallsIgnoreWithExpectedException");
            Assert.AreEqual(ResultState.Ignored, result.ResultState);
            Assert.AreEqual("Ignore me", result.Message);
        }

        [Test]
        public void IgnoreWorksForTestSuite()
        {
            TestSuite suite = new TestSuite("IgnoredTestFixture");
            suite.Add(TestBuilder.MakeFixture(typeof(IgnoredTestSuiteFixture)));
            ITestResult fixtureResult = (ITestResult)TestBuilder.RunTest(suite, null).Children[0];

            Assert.AreEqual(ResultState.Ignored, fixtureResult.ResultState);

            foreach (ITestResult testResult in fixtureResult.Children)
                Assert.AreEqual(ResultState.Ignored, testResult.ResultState);
        }

        [Test]
        public void IgnoreWorksFromSetUp()
        {
            ITestResult fixtureResult = TestBuilder.RunTestFixture(typeof(IgnoreInSetUpFixture));

            // TODO: Decide how Ignored tests impact the containing suite result
            //Assert.AreEqual(ResultState.Ignored, fixtureResult.ResultState);

            foreach (TestResult testResult in fixtureResult.Children)
                Assert.AreEqual(ResultState.Ignored, testResult.ResultState);
        }

        [Test]
        public void IgnoreWithUserMessage()
        {
            try
            {
                Assert.Ignore("my message");
            }
            catch (IgnoreException ex)
            {
                Assert.AreEqual("my message", ex.Message);
            }
        }

        [Test]
        public void IgnoreWithUserMessage_OneArg()
        {
            try
            {
                Assert.Ignore("The number is {0}", 5);
            }
            catch (IgnoreException ex)
            {
                Assert.AreEqual("The number is 5", ex.Message);
            }
        }

        [Test]
        public void IgnoreWithUserMessage_ThreeArgs()
        {
            try
            {
                Assert.Ignore("The numbers are {0}, {1} and {2}", 1, 2, 3);
            }
            catch (IgnoreException ex)
            {
                Assert.AreEqual("The numbers are 1, 2 and 3", ex.Message);
            }
        }

        [Test]
        public void IgnoreWithUserMessage_ArrayOfArgs()
        {
            try
            {
                Assert.Ignore("The numbers are {0}, {1} and {2}", new object[] { 1, 2, 3 });
            }
            catch (IgnoreException ex)
            {
                Assert.AreEqual("The numbers are 1, 2 and 3", ex.Message);
            }
        }
    }
}
