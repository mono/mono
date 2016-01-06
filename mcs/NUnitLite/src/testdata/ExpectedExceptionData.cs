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

namespace NUnit.TestData.ExpectedExceptionData
{
	[TestFixture]
	public class BaseException
	{
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void BaseExceptionTest()
		{
			throw new Exception();
		}
	}

	[TestFixture]
	public class DerivedException
	{
		[Test]
		[ExpectedException(typeof(Exception))]
		public void DerivedExceptionTest()
		{
			throw new ArgumentException();
		}
	}

	[TestFixture]
	public class MismatchedException
	{
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void MismatchedExceptionType()
        {
            throw new ArgumentOutOfRangeException();
        }

        [Test]
        [ExpectedException(ExpectedException=typeof(ArgumentException))]
        public void MismatchedExceptionTypeAsNamedParameter()
        {
            throw new ArgumentOutOfRangeException();
        }

        [Test]
		[ExpectedException(typeof(ArgumentException), UserMessage="custom message")]
		public void MismatchedExceptionTypeWithUserMessage()
		{
			throw new ArgumentOutOfRangeException();
		}

		[Test]
		[ExpectedException("System.ArgumentException")]
		public void MismatchedExceptionName()
		{
			throw new ArgumentOutOfRangeException();
		}

		[Test]
		[ExpectedException("System.ArgumentException", UserMessage="custom message")]
		public void MismatchedExceptionNameWithUserMessage()
		{
			throw new ArgumentOutOfRangeException();
		}
	}

	[TestFixture]
	public class SetUpExceptionTests  
	{
		[SetUp]
		public void Init()
		{
			throw new ArgumentException("SetUp Exception");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void Test() 
		{
		}
	}

	[TestFixture]
	public class TearDownExceptionTests
	{
		[TearDown]
		public void CleanUp()
		{
			throw new ArgumentException("TearDown Exception");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void Test() 
		{}
	}

	[TestFixture]
	public class TestThrowsExceptionFixture
	{
		[Test]
		public void TestThrow()
		{
			throw new Exception();
		}
	}

	[TestFixture]
	public class TestDoesNotThrowExceptionFixture
	{
		[Test, ExpectedException("System.ArgumentException")]
		public void TestDoesNotThrowExceptionName()
		{
		}

		[Test, ExpectedException("System.ArgumentException", UserMessage="custom message")]
		public void TestDoesNotThrowExceptionNameWithUserMessage()
		{
		}

		[Test, ExpectedException( typeof( System.ArgumentException ) )]
		public void TestDoesNotThrowExceptionType()
		{
		}

		[Test, ExpectedException( typeof( System.ArgumentException ), UserMessage="custom message" )]
		public void TestDoesNotThrowExceptionTypeWithUserMessage()
		{
		}

		[Test, ExpectedException]
		public void TestDoesNotThrowUnspecifiedException()
		{
		}

		[Test, ExpectedException( UserMessage="custom message" )]
		public void TestDoesNotThrowUnspecifiedExceptionWithUserMessage()
		{
		}
	}

	[TestFixture]
	public class TestThrowsExceptionWithRightMessage
	{
		[Test]
		[ExpectedException(typeof(Exception), ExpectedMessage="the message")]
		public void TestThrow()
		{
			throw new Exception("the message");
		}
	}

	[TestFixture]
	public class TestThrowsArgumentOutOfRangeException
	{
		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException)) ]
		public void TestThrow()
		{
#if NETCF || SILVERLIGHT
			throw new ArgumentOutOfRangeException("param", "the message");
#else
			throw new ArgumentOutOfRangeException("param", "actual value", "the message");
#endif
		}
	}

	[TestFixture]
	public class TestThrowsExceptionWithWrongMessage
	{
		[Test]
		[ExpectedException(typeof(Exception), ExpectedMessage="not the message")]
		public void TestThrow()
		{
			throw new Exception("the message");
		}

		[Test]
		[ExpectedException( typeof(Exception), ExpectedMessage="not the message", UserMessage="custom message" )]
		public void TestThrowWithUserMessage()
		{
			throw new Exception("the message");
		}
	}

	[TestFixture]
	public class TestAssertsBeforeThrowingException
	{
		[Test]
		[ExpectedException(typeof(Exception))]
		public void TestAssertFail()
		{
			Assert.Fail( "private message" );
		}
	}

    public class ExceptionHandlerCalledClass : IExpectException
    {
        public bool HandlerCalled = false;
        public bool AlternateHandlerCalled = false;

        [Test, ExpectedException(typeof(ArgumentException))]
        public void ThrowsArgumentException()
        {
            throw new ArgumentException();
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void ThrowsCustomException()
        {
            throw new CustomException();
        }

        class CustomException : Exception { }

        [Test, ExpectedException(typeof(ArgumentException), Handler = "AlternateExceptionHandler")]
        public void ThrowsArgumentException_AlternateHandler()
        {
            throw new ArgumentException();
        }

        [Test, ExpectedException(typeof(ArgumentException), Handler = "AlternateExceptionHandler")]
        public void ThrowsCustomException_AlternateHandler()
        {
            throw new CustomException();
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void ThrowsSystemException()
        {
            throw new Exception();
        }

        [Test, ExpectedException(typeof(ArgumentException), Handler = "AlternateExceptionHandler")]
        public void ThrowsSystemException_AlternateHandler()
        {
            throw new Exception();
        }

        [Test, ExpectedException(typeof(ArgumentException), Handler = "DeliberatelyMissingHandler")]
        public void MethodWithBadHandler()
        {
            throw new ArgumentException();
        }

        public void HandleException(Exception ex)
        {
            HandlerCalled = true;
        }

        public void AlternateExceptionHandler(Exception ex)
        {
            AlternateHandlerCalled = true;
        }
    }

#if CLR_2_0 || CLR_4_0
    public static class StaticClassWithExpectedExceptions
    {
        [Test, ExpectedException(typeof(ArgumentException))]
        public static void TestSucceedsInStaticClass()
        {
            throw new ArgumentException("argument exception");
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public static void TestFailsInStaticClass_NoExceptionThrown()
        {
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public static void TestFailsInStaticClass_WrongExceptionThrown()
        {
            throw new InvalidOperationException("wrong exception");
        }
    }
#endif
}
