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

namespace NUnit.Framework.Assertions
{
    [TestFixture]
    public class AssertPassTests
    {
        [Test, ExpectedException(typeof(SuccessException))]
        public void ThrowsSuccessException()
        {
            Assert.Pass();
        }

        [Test, ExpectedException(typeof(SuccessException), ExpectedMessage = "MESSAGE")]
        public void ThrowsSuccessExceptionWithMessage()
        {
            Assert.Pass("MESSAGE");
        }

        [Test, ExpectedException(typeof(SuccessException), ExpectedMessage = "MESSAGE: 2+2=4")]
        public void ThrowsSuccessExceptionWithMessageAndArgs()
        {
            Assert.Pass("MESSAGE: {0}+{1}={2}", 2, 2, 4);
        }

        [Test]
        public void AssertPassReturnsSuccess()
        {
            Assert.Pass("This test is OK!");
        }

        [Test]
        public void SubsequentFailureIsIrrelevant()
        {
            Assert.Pass("This test is OK!");
            Assert.Fail("No it's NOT!");
        }
    }
}
