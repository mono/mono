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

using NUnit.Framework.Api;
using NUnit.TestData.AssertFailFixture;
using NUnit.TestUtilities;

namespace NUnit.Framework.Assertions
{
    [TestFixture]
    public class AssertFailTests
    {
        [Test, ExpectedException(typeof(AssertionException))]
        public void ThrowsAssertionException()
        {
            Assert.Fail();
        }

        [Test, ExpectedException(typeof(AssertionException), ExpectedMessage = "MESSAGE")]
        public void ThrowsAssertionExceptionWithMessage()
        {
            Assert.Fail("MESSAGE");
        }

        [Test, ExpectedException(typeof(AssertionException), ExpectedMessage = "MESSAGE: 2+2=4")]
        public void ThrowsAssertionExceptionWithMessageAndArgs()
        {
            Assert.Fail("MESSAGE: {0}+{1}={2}", 2, 2, 4);
        }

        [Test]
        public void AssertFailWorks()
        {
            ITestResult result = TestBuilder.RunTestCase(
                typeof(AssertFailFixture),
                "CallAssertFail");

            Assert.AreEqual(ResultState.Failure, result.ResultState);
        }

        [Test]
        public void AssertFailWorksWithMessage()
        {
            ITestResult result = TestBuilder.RunTestCase(
                typeof(AssertFailFixture),
                "CallAssertFailWithMessage");

            Assert.AreEqual(ResultState.Failure, result.ResultState);
            Assert.AreEqual("MESSAGE", result.Message);
        }

        [Test]
        public void AssertFailWorksWithMessageAndArgs()
        {
            ITestResult result = TestBuilder.RunTestCase(
                typeof(AssertFailFixture),
                "CallAssertFailWithMessageAndArgs");

            Assert.AreEqual(ResultState.Failure, result.ResultState);
            Assert.AreEqual("MESSAGE: 2+2=4", result.Message);
        }
    }
}
