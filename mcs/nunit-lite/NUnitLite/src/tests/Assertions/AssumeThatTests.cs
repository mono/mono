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
    public class AssumeThatTests
    {
        [Test]
        public void AssumptionPasses_Boolean()
        {
            Assume.That(2 + 2 == 4);
        }

        [Test]
        public void AssumptionPasses_BooleanWithMessage()
        {
            Assume.That(2 + 2 == 4, "Not Equal");
        }

        [Test]
        public void AssumptionPasses_BooleanWithMessageAndArgs()
        {
            Assume.That(2 + 2 == 4, "Not Equal to {0}", 4);
        }

        [Test]
        public void AssumptionPasses_ActualAndConstraint()
        {
            Assume.That(2 + 2, Is.EqualTo(4));
        }

        [Test]
        public void AssumptionPasses_ActualAndConstraintWithMessage()
        {
            Assume.That(2 + 2, Is.EqualTo(4), "Should be 4");
        }

        [Test]
        public void AssumptionPasses_ActualAndConstraintWithMessageAndArgs()
        {
            Assume.That(2 + 2, Is.EqualTo(4), "Should be {0}", 4);
        }

        [Test]
        public void AssumptionPasses_ReferenceAndConstraint()
        {
            bool value = true;
            Assume.That(ref value, Is.True);
        }

        [Test]
        public void AssumptionPasses_ReferenceAndConstraintWithMessage()
        {
            bool value = true;
            Assume.That(ref value, Is.True, "Message");
        }

        [Test]
        public void AssumptionPasses_ReferenceAndConstraintWithMessageAndArgs()
        {
            bool value = true;
            Assume.That(ref value, Is.True, "Message", 42);
        }

        [Test]
        public void AssumptionPasses_DelegateAndConstraint()
        {
            Assume.That(new ActualValueDelegate(ReturnsFour), Is.EqualTo(4));
        }

        [Test]
        public void AssumptionPasses_DelegateAndConstraintWithMessage()
        {
            Assume.That(new ActualValueDelegate(ReturnsFour), Is.EqualTo(4), "Message");
        }

        [Test]
        public void AssumptionPasses_DelegateAndConstraintWithMessageAndArgs()
        {
            Assume.That(new ActualValueDelegate(ReturnsFour), Is.EqualTo(4), "Should be {0}", 4);
        }

        private object ReturnsFour()
        {
            return 4;
        }

        [Test, ExpectedException(typeof(InconclusiveException))]
        public void FailureThrowsInconclusiveException_Boolean()
        {
            Assume.That(2 + 2 == 5);
        }

        [Test, ExpectedException(typeof(InconclusiveException), ExpectedMessage="message", MatchType=MessageMatch.Contains)]
        public void FailureThrowsInconclusiveException_BooleanWithMessage()
        {
            Assume.That(2 + 2 == 5, "message");
        }

        [Test, ExpectedException(typeof(InconclusiveException), ExpectedMessage= "got 5", MatchType=MessageMatch.Contains)]
        public void FailureThrowsInconclusiveException_BooleanWithMessageAndArgs()
        {
            Assume.That(2 + 2 == 5, "got {0}", 5);
        }

        [Test, ExpectedException(typeof(InconclusiveException))]
        public void FailureThrowsInconclusiveException_ActualAndConstraint()
        {
            Assume.That(2 + 2, Is.EqualTo(5));
        }

        [Test, ExpectedException(typeof(InconclusiveException), ExpectedMessage="Error", MatchType=MessageMatch.Contains)]
        public void FailureThrowsInconclusiveException_ActualAndConstraintWithMessage()
        {
            Assume.That(2 + 2, Is.EqualTo(5), "Error");
        }

        [Test, ExpectedException(typeof(InconclusiveException), ExpectedMessage="Should be 5", MatchType=MessageMatch.Contains)]
        public void FailureThrowsInconclusiveException_ActualAndConstraintWithMessageAndArgs()
        {
            Assume.That(2 + 2, Is.EqualTo(5), "Should be {0}", 5);
        }

        [Test, ExpectedException(typeof(InconclusiveException))]
        public void FailureThrowsInconclusiveException_ReferenceAndConstraint()
        {
            bool value = false;
            Assume.That(ref value, Is.True);
        }

        [Test, ExpectedException(typeof(InconclusiveException), ExpectedMessage="message", MatchType=MessageMatch.Contains)]
        public void FailureThrowsInconclusiveException_ReferenceAndConstraintWithMessage()
        {
            bool value = false;
            Assume.That(ref value, Is.True, "message");
        }

        [Test, ExpectedException(typeof(InconclusiveException), ExpectedMessage="message is 42", MatchType=MessageMatch.Contains)]
        public void FailureThrowsInconclusiveException_ReferenceAndConstraintWithMessageAndArgs()
        {
            bool value = false;
            Assume.That(ref value, Is.True, "message is {0}", 42);
        }

        [Test, ExpectedException(typeof(InconclusiveException))]
        public void FailureThrowsInconclusiveException_DelegateAndConstraint()
        {
            Assume.That(new ActualValueDelegate(ReturnsFive), Is.EqualTo(4));
        }

        [Test, ExpectedException(typeof(InconclusiveException), ExpectedMessage = "Error", MatchType = MessageMatch.Contains)]
        public void FailureThrowsInconclusiveException_DelegateAndConstraintWithMessage()
        {
            Assume.That(new ActualValueDelegate(ReturnsFive), Is.EqualTo(4), "Error");
        }

        [Test, ExpectedException(typeof(InconclusiveException), ExpectedMessage="Should be 4", MatchType=MessageMatch.Contains)]
        public void FailureThrowsInconclusiveException_DelegateAndConstraintWithMessageAndArgs()
        {
            Assume.That(new ActualValueDelegate(ReturnsFive), Is.EqualTo(4), "Should be {0}", 4);
        }

        private object ReturnsFive()
        {
            return 5;
        }

#if NET_4_5
        [Test]
        public void AssumeThatSuccess()
        {
            Assume.That(async () => await One(), Is.EqualTo(1));
        }

        [Test]
        public void AssumeThatFailure()
        {
            var exception = Assert.Throws<InconclusiveException>(() =>
                Assume.That(async () => await One(), Is.EqualTo(2)));
        }

        [Test]
        public void AssumeThatError()
        {
            var exception = Assert.Throws<InvalidOperationException>(() =>
                Assume.That(async () => await ThrowExceptionGenericTask(), Is.EqualTo(1)));

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
#endif
    }
}
