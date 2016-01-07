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
using NUnit.TestUtilities;
using NUnit.TestData.ExpectedExceptionData;
#if !NETCF
using System.Runtime.Serialization;
#endif

namespace NUnit.Framework.Attributes
{
	/// <summary>
	/// 
	/// </summary>
	[TestFixture]
	public class ExpectedExceptionTests 
	{
		[Test, ExpectedException]
		public void CanExpectUnspecifiedException()
		{
			throw new ArgumentException();
		}

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestSucceedsWithSpecifiedExceptionType()
        {
            throw new ArgumentException("argument exception");
        }

        [Test]
        [ExpectedException(ExpectedException=typeof(ArgumentException))]
        public void TestSucceedsWithSpecifiedExceptionTypeAsNamedParameter()
        {
            throw new ArgumentException("argument exception");
        }

        [Test]
        [ExpectedException("System.ArgumentException")]
        public void TestSucceedsWithSpecifiedExceptionName()
        {
            throw new ArgumentException("argument exception");
        }

        [Test]
        [ExpectedException(ExpectedExceptionName="System.ArgumentException")]
        public void TestSucceedsWithSpecifiedExceptionNameAsNamedParameter()
        {
            throw new ArgumentException("argument exception");
        }

        [Test]
		[ExpectedException(typeof(ArgumentException),ExpectedMessage="argument exception")]
		public void TestSucceedsWithSpecifiedExceptionTypeAndMessage()
		{
			throw new ArgumentException("argument exception");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException), ExpectedMessage="argument exception", MatchType=MessageMatch.Exact)]
		public void TestSucceedsWithSpecifiedExceptionTypeAndExactMatch()
		{
			throw new ArgumentException("argument exception");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException),ExpectedMessage="invalid", MatchType=MessageMatch.Contains)]
		public void TestSucceedsWithSpecifiedExceptionTypeAndContainsMatch()
		{
			throw new ArgumentException("argument invalid exception");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException),ExpectedMessage="exception$", MatchType=MessageMatch.Regex)]
		public void TestSucceedsWithSpecifiedExceptionTypeAndRegexMatch()
		{
			throw new ArgumentException("argument invalid exception");
		}

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "argument invalid", MatchType = MessageMatch.StartsWith)]
        public void TestSucceedsWithSpecifiedExceptionTypeAndStartsWithMatch()
        {
            throw new ArgumentException("argument invalid exception");
        }

//        [Test]
//        [ExpectedException("System.ArgumentException", "argument exception")]
//        public void TestSucceedsWithSpecifiedExceptionNameAndMessage_OldFormat()
//        {
//            throw new ArgumentException("argument exception");
//        }

        [Test]
        [ExpectedException("System.ArgumentException", ExpectedMessage = "argument exception")]
        public void TestSucceedsWithSpecifiedExceptionNameAndMessage_NewFormat()
        {
            throw new ArgumentException("argument exception");
        }

        [Test]
		[ExpectedException("System.ArgumentException",ExpectedMessage="argument exception",MatchType=MessageMatch.Exact)]
		public void TestSucceedsWithSpecifiedExceptionNameAndExactMatch()
		{
			throw new ArgumentException("argument exception");
		}

		[Test]
		[ExpectedException("System.ArgumentException",ExpectedMessage="invalid", MatchType=MessageMatch.Contains)]
		public void TestSucceedsWhenSpecifiedExceptionNameAndContainsMatch()
		{
			throw new ArgumentException("argument invalid exception");
		}

		[Test]
		[ExpectedException("System.ArgumentException",ExpectedMessage="exception$", MatchType=MessageMatch.Regex)]
		public void TestSucceedsWhenSpecifiedExceptionNameAndRegexMatch()
		{
			throw new ArgumentException("argument invalid exception");
		}

		[Test]
		public void TestFailsWhenBaseExceptionIsThrown()
		{
			Type fixtureType = typeof(BaseException);
			ITestResult result = TestBuilder.RunTestCase( fixtureType, "BaseExceptionTest" );
			Assert.IsTrue(result.ResultState == ResultState.Failure, "BaseExceptionTest should have failed");
			Assert.That(result.Message, Is.StringStarting(
				"An unexpected exception type was thrown" + Env.NewLine +
				"Expected: System.ArgumentException" + Env.NewLine +
				" but was: System.Exception"));
		}

		[Test]
		public void TestFailsWhenDerivedExceptionIsThrown()
		{
			Type fixtureType = typeof(DerivedException);
            ITestResult result = TestBuilder.RunTestCase(fixtureType, "DerivedExceptionTest");
			Assert.IsTrue(result.ResultState == ResultState.Failure, "DerivedExceptionTest should have failed");
			Assert.That(result.Message, Is.StringStarting( 
				"An unexpected exception type was thrown" + Env.NewLine +
				"Expected: System.Exception" + Env.NewLine +
				" but was: System.ArgumentException"));
		}

        [Test]
        public void TestMismatchedExceptionType()
        {
            Type fixtureType = typeof(MismatchedException);
            ITestResult result = TestBuilder.RunTestCase(fixtureType, "MismatchedExceptionType");
            Assert.IsTrue(result.ResultState == ResultState.Failure, "MismatchedExceptionType should have failed");
            Assert.That(result.Message, Is.StringStarting(
                "An unexpected exception type was thrown" + Env.NewLine +
                "Expected: System.ArgumentException" + Env.NewLine +
                " but was: System.ArgumentOutOfRangeException"));
        }

        [Test]
        public void TestMismatchedExceptionTypeAsNamedParameter()
        {
            Type fixtureType = typeof(MismatchedException);
            ITestResult result = TestBuilder.RunTestCase(fixtureType, "MismatchedExceptionTypeAsNamedParameter");
            Assert.IsTrue(result.ResultState == ResultState.Failure, "MismatchedExceptionType should have failed");
            Assert.That(result.Message, Is.StringStarting(
                "An unexpected exception type was thrown" + Env.NewLine +
                "Expected: System.ArgumentException" + Env.NewLine +
                " but was: System.ArgumentOutOfRangeException"));
        }

        [Test]
		public void TestMismatchedExceptionTypeWithUserMessage()
		{
			Type fixtureType = typeof(MismatchedException);
			ITestResult result = TestBuilder.RunTestCase( fixtureType, "MismatchedExceptionTypeWithUserMessage" );
			Assert.IsTrue(result.ResultState == ResultState.Failure, "Test method should have failed");
			Assert.That(result.Message, Is.StringStarting(
				"custom message" + Env.NewLine +
				"An unexpected exception type was thrown" + Env.NewLine +
				"Expected: System.ArgumentException" + Env.NewLine +
				" but was: System.ArgumentOutOfRangeException"));
		}

		[Test]
		public void TestMismatchedExceptionName()
		{
			Type fixtureType = typeof(MismatchedException);
			ITestResult result = TestBuilder.RunTestCase( fixtureType, "MismatchedExceptionName" );
			Assert.IsTrue(result.ResultState == ResultState.Failure, "MismatchedExceptionName should have failed");
			Assert.That(result.Message, Is.StringStarting(
				"An unexpected exception type was thrown" + Env.NewLine +
				"Expected: System.ArgumentException" + Env.NewLine +
				" but was: System.ArgumentOutOfRangeException")); 
		}

		[Test]
		public void TestMismatchedExceptionNameWithUserMessage()
		{
			Type fixtureType = typeof(MismatchedException);
            ITestResult result = TestBuilder.RunTestCase(fixtureType, "MismatchedExceptionNameWithUserMessage");
			Assert.IsTrue(result.ResultState == ResultState.Failure, "Test method should have failed");
			Assert.That(result.Message, Is.StringStarting(
				"custom message" + Env.NewLine +
				"An unexpected exception type was thrown" + Env.NewLine +
				"Expected: System.ArgumentException" + Env.NewLine +
				" but was: System.ArgumentOutOfRangeException")); 
		}

		[Test]
		public void TestMismatchedExceptionMessage()
		{
			Type fixtureType = typeof(TestThrowsExceptionWithWrongMessage);
			ITestResult result = TestBuilder.RunTestCase( fixtureType, "TestThrow" );
			Assert.IsTrue(result.ResultState == ResultState.Failure, "TestThrow should have failed");
			Assert.AreEqual(
				"The exception message text was incorrect" + Env.NewLine +
				"Expected: not the message" + Env.NewLine +
				" but was: the message", 
				result.Message);
		}

		[Test]
		public void TestMismatchedExceptionMessageWithUserMessage()
		{
			Type fixtureType = typeof(TestThrowsExceptionWithWrongMessage);
			ITestResult result = TestBuilder.RunTestCase( fixtureType, "TestThrowWithUserMessage" );
			Assert.IsTrue(result.ResultState == ResultState.Failure, "TestThrow should have failed");
			Assert.AreEqual(
				"custom message" + Env.NewLine +
				"The exception message text was incorrect" + Env.NewLine +
				"Expected: not the message" + Env.NewLine +
				" but was: the message", 
				result.Message);
		}

		[Test]
		public void TestUnspecifiedExceptionNotThrown()
		{
			Type fixtureType = typeof(TestDoesNotThrowExceptionFixture);
			ITestResult result = TestBuilder.RunTestCase( fixtureType, "TestDoesNotThrowUnspecifiedException" );
			Assert.AreEqual(ResultState.Failure, result.ResultState);
			Assert.AreEqual("An Exception was expected", result.Message);
		}

		[Test]
		public void TestUnspecifiedExceptionNotThrownWithUserMessage()
		{
			Type fixtureType = typeof(TestDoesNotThrowExceptionFixture);
            ITestResult result = TestBuilder.RunTestCase(fixtureType, "TestDoesNotThrowUnspecifiedExceptionWithUserMessage");
            Assert.IsTrue(result.ResultState == ResultState.Failure, "Test method should have failed");
			Assert.AreEqual("custom message" + Env.NewLine + "An Exception was expected", result.Message);
		}

		[Test]
		public void TestExceptionTypeNotThrown()
		{
			Type fixtureType = typeof(TestDoesNotThrowExceptionFixture);
			ITestResult result = TestBuilder.RunTestCase( fixtureType, "TestDoesNotThrowExceptionType" );
            Assert.IsTrue(result.ResultState == ResultState.Failure, "Test method should have failed");
			Assert.AreEqual("System.ArgumentException was expected", result.Message);
		}

		[Test]
		public void TestExceptionTypeNotThrownWithUserMessage()
		{
			Type fixtureType = typeof(TestDoesNotThrowExceptionFixture);
			ITestResult result = TestBuilder.RunTestCase( fixtureType, "TestDoesNotThrowExceptionTypeWithUserMessage" );
            Assert.IsTrue(result.ResultState == ResultState.Failure, "Test method should have failed");
			Assert.AreEqual("custom message" + Env.NewLine + "System.ArgumentException was expected", result.Message);
		}

		[Test]
		public void TestExceptionNameNotThrown()
		{
			Type fixtureType = typeof(TestDoesNotThrowExceptionFixture);
			ITestResult result = TestBuilder.RunTestCase( fixtureType, "TestDoesNotThrowExceptionName" );
            Assert.IsTrue(result.ResultState == ResultState.Failure, "Test method should have failed");
			Assert.AreEqual("System.ArgumentException was expected", result.Message);
		}

		[Test]
		public void TestExceptionNameNotThrownWithUserMessage()
		{
			Type fixtureType = typeof(TestDoesNotThrowExceptionFixture);
			ITestResult result = TestBuilder.RunTestCase( fixtureType, "TestDoesNotThrowExceptionNameWithUserMessage" );
            Assert.IsTrue(result.ResultState == ResultState.Failure, "Test method should have failed");
			Assert.AreEqual("custom message" + Env.NewLine + "System.ArgumentException was expected", result.Message);
		}

		[Test] 
		public void MethodThrowsException()
		{
			TestResult result = TestBuilder.RunTestFixture( typeof( TestThrowsExceptionFixture ) );
            Assert.AreEqual(true, result.ResultState == ResultState.Failure);
		}

		[Test] 
		public void MethodThrowsRightExceptionMessage()
		{
			TestResult result = TestBuilder.RunTestFixture( typeof( TestThrowsExceptionWithRightMessage ) );
            Assert.AreEqual(true, result.ResultState == ResultState.Success);
		}

		[Test]
		public void MethodThrowsArgumentOutOfRange()
		{
			TestResult result = TestBuilder.RunTestFixture( typeof( TestThrowsArgumentOutOfRangeException ) );
            Assert.AreEqual(true, result.ResultState == ResultState.Success);
		}

		[Test] 
		public void MethodThrowsWrongExceptionMessage()
		{
			TestResult result = TestBuilder.RunTestFixture( typeof( TestThrowsExceptionWithWrongMessage ) );
            Assert.AreEqual(true, result.ResultState == ResultState.Failure);
		}

		[Test]
		public void SetUpThrowsSameException()
		{
			TestResult result = TestBuilder.RunTestFixture( typeof( SetUpExceptionTests ) );
            Assert.AreEqual(true, result.ResultState == ResultState.Failure);
		}

		[Test]
		public void TearDownThrowsSameException()
		{
			TestResult result = TestBuilder.RunTestFixture( typeof( TearDownExceptionTests ) );
            Assert.AreEqual(true, result.ResultState == ResultState.Failure);
		}

		[Test]
		public void AssertFailBeforeException() 
		{ 
			TestResult suiteResult = TestBuilder.RunTestFixture( typeof (TestAssertsBeforeThrowingException) );
			Assert.AreEqual( ResultState.Failure, suiteResult.ResultState );
            TestResult result = (TestResult)suiteResult.Children[0];
			Assert.AreEqual( "private message", result.Message );
		} 

		internal class MyAppException : System.Exception
		{
			public MyAppException (string message) : base(message) 
			{}

			public MyAppException(string message, Exception inner) :
				base(message, inner) 
			{}

#if !NETCF && !SILVERLIGHT
			protected MyAppException(SerializationInfo info, 
				StreamingContext context) : base(info,context)
			{}
#endif
		}

		[Test]
		[ExpectedException(typeof(MyAppException))] 
		public void ThrowingMyAppException() 
		{ 
			throw new MyAppException("my app");
		}

		[Test]
		[ExpectedException(typeof(MyAppException), ExpectedMessage="my app")] 
		public void ThrowingMyAppExceptionWithMessage() 
		{ 
			throw new MyAppException("my app");
		}

		[Test]
		[ExpectedException(typeof(NUnitException))]
		public void ThrowNUnitException()
		{
			throw new NUnitException("Nunit exception");
		}

		[Test]
		public void ExceptionHandlerIsCalledWhenExceptionMatches_AlternateHandler()
		{
			ExceptionHandlerCalledClass fixture = new ExceptionHandlerCalledClass();
			TestBuilder.RunTestCase( fixture, "ThrowsArgumentException_AlternateHandler" );
			Assert.IsFalse(fixture.HandlerCalled, "Base Handler should not be called" );
			Assert.IsTrue(fixture.AlternateHandlerCalled, "Alternate Handler should be called" );
		}
	
		[Test]
		public void ExceptionHandlerIsCalledWhenExceptionMatches()
		{
			ExceptionHandlerCalledClass fixture = new ExceptionHandlerCalledClass();
			TestBuilder.RunTestCase( fixture, "ThrowsArgumentException" );
            Assert.IsTrue(fixture.HandlerCalled, "Base Handler should be called");
			Assert.IsFalse(fixture.AlternateHandlerCalled, "Alternate Handler should not be called");
		}
	
		[Test]
		public void ExceptionHandlerIsNotCalledWhenExceptionDoesNotMatch()
		{
			ExceptionHandlerCalledClass fixture = new ExceptionHandlerCalledClass();
			TestBuilder.RunTestCase( fixture, "ThrowsCustomException" );
			Assert.IsFalse( fixture.HandlerCalled, "Base Handler should not be called" );
			Assert.IsFalse( fixture.AlternateHandlerCalled, "Alternate Handler should not be called" );
		}

		[Test]
		public void ExceptionHandlerIsNotCalledWhenExceptionDoesNotMatch_AlternateHandler()
		{
			ExceptionHandlerCalledClass fixture = new ExceptionHandlerCalledClass();
            TestBuilder.RunTestCase(fixture, "ThrowsCustomException_AlternateHandler");
			Assert.IsFalse(fixture.HandlerCalled, "Base Handler should not be called");
			Assert.IsFalse(fixture.AlternateHandlerCalled, "Alternate Handler should not be called");
		}

		[Test]
		public void TestIsNotRunnableWhenAlternateHandlerIsNotFound()
		{
			ExceptionHandlerCalledClass fixture = new ExceptionHandlerCalledClass();
			Test test = TestBuilder.MakeTestCase( fixture, "MethodWithBadHandler" );
			Assert.AreEqual( RunState.NotRunnable, test.RunState );
			Assert.AreEqual(
				"The specified exception handler DeliberatelyMissingHandler was not found",
				test.Properties.Get(PropertyNames.SkipReason) );
		}

#if CLR_2_0 || CLR_4_0
        [Test]
        public void TestSucceedsInStaticClass()
        {
            ITestResult result = TestBuilder.RunTestCase(typeof(StaticClassWithExpectedExceptions), "TestSucceedsInStaticClass");
            Assert.That(result.ResultState, Is.EqualTo(ResultState.Success));
        }

        [Test]
        public void TestFailsInStaticClass_NoExceptionThrown()
        {
            ITestResult result = TestBuilder.RunTestCase(typeof(StaticClassWithExpectedExceptions), "TestFailsInStaticClass_NoExceptionThrown");
            Assert.That(result.ResultState, Is.EqualTo(ResultState.Failure));
        }

        [Test]
        public void TestFailsInStaticClass_WrongExceptionThrown()
        {
            ITestResult result = TestBuilder.RunTestCase(typeof(StaticClassWithExpectedExceptions), "TestFailsInStaticClass_WrongExceptionThrown");
            Assert.That(result.ResultState, Is.EqualTo(ResultState.Failure));
        }
#endif
    }
}
