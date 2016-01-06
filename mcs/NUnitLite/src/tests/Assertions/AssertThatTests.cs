// ***********************************************************************
// Copyright (c) 2008 Charlie Poole
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
using NUnit.Framework.Constraints;
using NUnit.Framework.Internal;
using NUnit.TestData;
using NUnit.TestUtilities;
#if NET_4_5
using System.Threading.Tasks;
#endif

#if CLR_2_0 || CLR_4_0
using ActualValueDelegate = NUnit.Framework.Constraints.ActualValueDelegate<object>;
#else
using ActualValueDelegate = NUnit.Framework.Constraints.ActualValueDelegate;
#endif

namespace NUnit.Framework.Assertions
{
    [TestFixture]
    public class AssertThatTests
    {
        [Test]
        public void AssertionPasses_Boolean()
        {
            Assert.That(2 + 2 == 4);
        }

        [Test]
        public void AssertionPasses_BooleanWithMessage()
        {
            Assert.That(2 + 2 == 4, "Not Equal");
        }

        [Test]
        public void AssertionPasses_BooleanWithMessageAndArgs()
        {
            Assert.That(2 + 2 == 4, "Not Equal to {0}", 4);
        }

        [Test]
        public void AssertionPasses_ActualAndConstraint()
        {
            Assert.That(2 + 2, Is.EqualTo(4));
        }

        [Test]
        public void AssertionPasses_ActualAndConstraintWithMessage()
        {
            Assert.That(2 + 2, Is.EqualTo(4), "Should be 4");
        }

        [Test]
        public void AssertionPasses_ActualAndConstraintWithMessageAndArgs()
        {
            Assert.That(2 + 2, Is.EqualTo(4), "Should be {0}", 4);
        }

        [Test]
        public void AssertionPasses_ReferenceAndConstraint()
        {
            bool value = true;
            Assert.That(ref value, Is.True);
        }

        [Test]
        public void AssertionPasses_ReferenceAndConstraintWithMessage()
        {
            bool value = true;
            Assert.That(ref value, Is.True, "Message");
        }

        [Test]
        public void AssertionPasses_ReferenceAndConstraintWithMessageAndArgs()
        {
            bool value = true;
            Assert.That(ref value, Is.True, "Message", 42);
        }

        [Test]
        public void AssertionPasses_DelegateAndConstraint()
        {
            Assert.That(new ActualValueDelegate(ReturnsFour), Is.EqualTo(4));
        }

        [Test]
        public void AssertionPasses_DelegateAndConstraintWithMessage()
        {
            Assert.That(new ActualValueDelegate(ReturnsFour), Is.EqualTo(4), "Message");
        }

        [Test]
        public void AssertionPasses_DelegateAndConstraintWithMessageAndArgs()
        {
            Assert.That(new ActualValueDelegate(ReturnsFour), Is.EqualTo(4), "Should be {0}", 4);
        }

        private object ReturnsFour()
        {
            return 4;
        }

        [Test, ExpectedException(typeof(AssertionException))]
        public void FailureThrowsAssertionException_Boolean()
        {
            Assert.That(2 + 2 == 5);
        }

        [Test, ExpectedException(typeof(AssertionException), ExpectedMessage = "message", MatchType = MessageMatch.Contains)]
        public void FailureThrowsAssertionException_BooleanWithMessage()
        {
            Assert.That(2 + 2 == 5, "message");
        }

        [Test, ExpectedException(typeof(AssertionException), ExpectedMessage = "got 5", MatchType = MessageMatch.Contains)]
        public void FailureThrowsAssertionException_BooleanWithMessageAndArgs()
        {
            Assert.That(2 + 2 == 5, "got {0}", 5);
        }

        [Test, ExpectedException(typeof(AssertionException))]
        public void FailureThrowsAssertionException_ActualAndConstraint()
        {
            Assert.That(2 + 2, Is.EqualTo(5));
        }

        [Test, ExpectedException(typeof(AssertionException), ExpectedMessage = "Error", MatchType = MessageMatch.Contains)]
        public void FailureThrowsAssertionException_ActualAndConstraintWithMessage()
        {
            Assert.That(2 + 2, Is.EqualTo(5), "Error");
        }

        [Test, ExpectedException(typeof(AssertionException), ExpectedMessage = "Should be 5", MatchType = MessageMatch.Contains)]
        public void FailureThrowsAssertionException_ActualAndConstraintWithMessageAndArgs()
        {
            Assert.That(2 + 2, Is.EqualTo(5), "Should be {0}", 5);
        }

        [Test, ExpectedException(typeof(AssertionException))]
        public void FailureThrowsAssertionException_ReferenceAndConstraint()
        {
            bool value = false;
            Assert.That(ref value, Is.True);
        }

        [Test, ExpectedException(typeof(AssertionException), ExpectedMessage = "message", MatchType = MessageMatch.Contains)]
        public void FailureThrowsAssertionException_ReferenceAndConstraintWithMessage()
        {
            bool value = false;
            Assert.That(ref value, Is.True, "message");
        }

        [Test, ExpectedException(typeof(AssertionException), ExpectedMessage = "message is 42", MatchType = MessageMatch.Contains)]
        public void FailureThrowsAssertionException_ReferenceAndConstraintWithMessageAndArgs()
        {
            bool value = false;
            Assert.That(ref value, Is.True, "message is {0}", 42);
        }

        [Test, ExpectedException(typeof(AssertionException))]
        public void FailureThrowsAssertionException_DelegateAndConstraint()
        {
            Assert.That(new ActualValueDelegate(ReturnsFive), Is.EqualTo(4));
        }

        [Test, ExpectedException(typeof(AssertionException), ExpectedMessage = "Error", MatchType = MessageMatch.Contains)]
        public void FailureThrowsAssertionException_DelegateAndConstraintWithMessage()
        {
            Assert.That(new ActualValueDelegate(ReturnsFive), Is.EqualTo(4), "Error");
        }

        [Test, ExpectedException(typeof(AssertionException), ExpectedMessage = "Should be 4", MatchType = MessageMatch.Contains)]
        public void FailureThrowsAssertionException_DelegateAndConstraintWithMessageAndArgs()
        {
            Assert.That(new ActualValueDelegate(ReturnsFive), Is.EqualTo(4), "Should be {0}", 4);
        }

        [Test]
        public void AssertionsAreCountedCorrectly()
        {
            TestResult result = TestBuilder.RunTestFixture(typeof(AssertCountFixture));

            int totalCount = 0;
            foreach (TestResult childResult in result.Children)
            {
                int expectedCount = childResult.Name == "ThreeAsserts" ? 3 : 1;
                Assert.That(childResult.AssertCount, Is.EqualTo(expectedCount), "Bad count for {0}", childResult.Name);
                totalCount += expectedCount;
            }

            Assert.That(result.AssertCount, Is.EqualTo(totalCount), "Fixture count is not correct");
        }

        private object ReturnsFive()
        {
            return 5;
        }

#if NET_4_5
        [Test]
        public void AssertThatSuccess()
        {
            Assert.That(async () => await One(), Is.EqualTo(1));
        }

        [Test]
        public void AssertThatFailure()
        {
            var exception = Assert.Throws<AssertionException>(() =>
                Assert.That(async () => await One(), Is.EqualTo(2)));
        }

        [Test]
        public void AssertThatErrorTask()
        {
            var exception = Assert.Throws<InvalidOperationException>(() =>
                Assert.That(async () => await ThrowExceptionTask(), Is.EqualTo(1)));

            Assert.That(exception.StackTrace, Contains.Substring("ThrowExceptionTask"));
        }

        [Test]
        public void AssertThatErrorGenericTask()
        {
            var exception = Assert.Throws<InvalidOperationException>(() =>
                Assert.That(async () => await ThrowExceptionGenericTask(), Is.EqualTo(1)));

            Assert.That(exception.StackTrace, Contains.Substring("ThrowExceptionGenericTask"));
        }

        [Test]
        public void AssertThatErrorVoid()
        {
            var exception = Assert.Throws<InvalidOperationException>(() =>
                Assert.That(async () => { await ThrowExceptionGenericTask(); }, Is.EqualTo(1)));

            Assert.That(exception.StackTrace, Contains.Substring("ThrowExceptionGenericTask"));
        }

        private static Task<int> One()
        {
            return Task.Run(() => 1);
        }

        private static async Task<int> ThrowExceptionGenericTask()
        {
            await One();
            throw new InvalidOperationException();
        }

        private static async Task ThrowExceptionTask()
        {
            await One();
            throw new InvalidOperationException();
        }
#endif
    }
}
