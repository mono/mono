// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

namespace NUnit.Core
{
	using System;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Reflection;

	/// <summary>
	/// The TestMethod class represents a TestCase implemented as a method
	/// call on a fixture object. At the moment, this is the only way we 
	/// implement a TestCase, but others are expected in the future.
	/// 
	/// Because of how exceptions are handled internally, this class
	/// must incorporate processing of expected exceptions. A change to
	/// the TestCase interface might make it easier to process exceptions
	/// in an object that aggregates a TestMethod in the future.
	/// </summary>
	public abstract class TestMethod : TestCase
	{
		#region Fields
		/// <summary>
		/// The test method
		/// </summary>
		private MethodInfo method;

		/// <summary>
		/// The SetUp method.
		/// </summary>
		protected MethodInfo setUpMethod;

		/// <summary>
		/// The teardown method
		/// </summary>
		protected MethodInfo tearDownMethod;

		/// <summary>
		/// The exception handler method
		/// </summary>
		internal MethodInfo exceptionHandler;

		/// <summary>
		/// True if an exception is expected
		/// </summary>
		internal bool exceptionExpected;

		/// <summary>
		/// The type of any expected exception
		/// </summary>
		internal Type expectedExceptionType;
        
		/// <summary>
		/// The full name of any expected exception type
		/// </summary>
		internal string expectedExceptionName;
        
		/// <summary>
		/// The value of any message associated with an expected exception
		/// </summary>
		internal string expectedMessage;
        
		/// <summary>
		/// A string indicating how to match the expected message
		/// </summary>
		internal string matchType;

		/// <summary>
		/// A string containing any user message specified for the expected exception
		/// </summary>
		internal string userMessage;

		#endregion

		#region Constructors
		public TestMethod( MethodInfo method ) 
			: base( method ) 
		{
			this.method = method;
		}
		#endregion

		#region Properties
		public MethodInfo Method
		{
			get { return method; }
		}

		public bool ExceptionExpected
		{
			get { return exceptionExpected; }
			set { exceptionExpected = value; }
		}

		public MethodInfo ExceptionHandler
		{
			get { return exceptionHandler; }
			set { exceptionHandler = value; }
		}

		public Type ExpectedExceptionType
		{
			get { return expectedExceptionType; }
			set 
			{ 
				expectedExceptionType = value;
				expectedExceptionName = expectedExceptionType != null
					? expectedExceptionType.FullName
					: null;
			}
		}

		public string ExpectedExceptionName
		{
			get { return expectedExceptionName; }
			set
			{
				expectedExceptionType = null;
				expectedExceptionName = value;
			}
		}

		public string ExpectedMessage
		{
			get { return expectedMessage; }
			set { expectedMessage = value; }
		}

		public string MatchType
		{
			get { return matchType; }
			set { matchType = value; }
		}

		public string UserMessage
		{
			get { return userMessage; }
			set { userMessage = value; }
		}
		#endregion

		#region Run Methods
		public override void Run(TestCaseResult testResult)
		{ 
			try
			{
				if ( this.Parent != null)
					Fixture = this.Parent.Fixture;

				if (!testResult.IsFailure)
				{
					// Temporary... to allow for tests that directly execute a test case
					if (Fixture == null)
						Fixture = Reflect.Construct(this.FixtureType);

                    if (this.Properties["_SETCULTURE"] != null)
                        TestContext.CurrentCulture =
                            new System.Globalization.CultureInfo((string)Properties["_SETCULTURE"]);
                    
                    doRun(testResult);
				}
			}
			catch (Exception ex)
			{
				if (ex is NUnitException)
					ex = ex.InnerException;

				RecordException(ex, testResult);
			}
			finally
			{
				Fixture = null;
			}
		}

		/// <summary>
		/// The doRun method is used to run a test internally.
		/// It assumes that the caller is taking care of any 
		/// TestFixtureSetUp and TestFixtureTearDown needed.
		/// </summary>
		/// <param name="testResult">The result in which to record success or failure</param>
		public virtual void doRun( TestCaseResult testResult )
		{
			DateTime start = DateTime.Now;

			try 
			{
				if ( setUpMethod != null )
					Reflect.InvokeMethod( setUpMethod, this.Fixture );

				doTestCase( testResult );
			}
			catch(Exception ex)
			{
				if ( ex is NUnitException )
					ex = ex.InnerException;

				RecordException( ex, testResult );
			}
			finally 
			{
				doTearDown( testResult );

				DateTime stop = DateTime.Now;
				TimeSpan span = stop.Subtract(start);
				testResult.Time = (double)span.Ticks / (double)TimeSpan.TicksPerSecond;
			}
		}
		#endregion

		#region Invoke Methods by Reflection, Recording Errors

		private void doTearDown( TestCaseResult testResult )
		{
			try
			{
				if ( tearDownMethod != null )
					tearDownMethod.Invoke( this.Fixture, new object[0] );
			}
			catch(Exception ex)
			{
				if ( ex is NUnitException )
					ex = ex.InnerException;
				// TODO: What about ignore exceptions in teardown?
				testResult.Error( ex,FailureSite.TearDown );
			}
		}

		private void doTestCase( TestCaseResult testResult )
		{
			try
			{
				RunTestMethod(testResult);
				ProcessNoException(testResult);
			}
			catch( Exception ex )
			{
				if ( ex is NUnitException )
					ex = ex.InnerException;

				if ( IsIgnoreException( ex ) )
					testResult.Ignore( ex );
				else
					ProcessException(ex, testResult);
			}
		}

		public virtual void RunTestMethod(TestCaseResult testResult)
		{
			Reflect.InvokeMethod( this.method, this.Fixture );
		}

		#endregion

		#region Record Info About An Exception

		protected void RecordException( Exception ex, TestResult testResult )
		{
			if ( IsIgnoreException( ex ) )
				testResult.Ignore( ex.Message );
			else if ( IsAssertException( ex ) )
				testResult.Failure( ex.Message, ex.StackTrace );
			else	
				testResult.Error( ex );
		}

		protected string GetStackTrace(Exception exception)
		{
			try
			{
				return exception.StackTrace;
			}
			catch( Exception )
			{
				return "No stack trace available";
			}
		}

		#endregion

		#region Exception Processing
		protected internal virtual void ProcessNoException(TestCaseResult testResult)
		{
			if ( ExceptionExpected )
				testResult.Failure(NoExceptionMessage(), null);
			else
				testResult.Success();
		}
		
		protected internal virtual void ProcessException(Exception exception, TestCaseResult testResult)
		{
			if (!ExceptionExpected)
			{
				RecordException(exception, testResult); 
				return;
			}

			if (IsExpectedExceptionType(exception))
			{
				if (IsExpectedMessageMatch(exception))
				{
					if ( exceptionHandler != null )
						Reflect.InvokeMethod( exceptionHandler, this.Fixture, exception );

					testResult.Success();
				}
				else
				{
					testResult.Failure(WrongTextMessage(exception), GetStackTrace(exception));
				}
			}
			else if (IsAssertException(exception))
			{
				testResult.Failure(exception.Message, exception.StackTrace);
			}
			else
			{
				testResult.Failure(WrongTypeMessage(exception), GetStackTrace(exception));
			}
		}
		#endregion

		#region Abstract Methods
		protected abstract bool IsAssertException(Exception ex);

		protected abstract bool IsIgnoreException(Exception ex);
		#endregion

		#region Helper Methods
		protected bool IsExpectedExceptionType(Exception exception)
		{
			return expectedExceptionName == null || expectedExceptionName.Equals(exception.GetType().FullName);
		}

		protected bool IsExpectedMessageMatch(Exception exception)
		{
			if (expectedMessage == null)
				return true;

			switch (matchType)
			{
				case "Exact":
				default:
					return expectedMessage.Equals(exception.Message);
				case "Contains":
					return exception.Message.IndexOf(expectedMessage) >= 0;
				case "Regex":
					return Regex.IsMatch(exception.Message, expectedMessage);
			}
		}

		protected string NoExceptionMessage()
		{
			string expectedType = expectedExceptionName == null ? "An Exception" : expectedExceptionName;
			return CombineWithUserMessage( expectedType + " was expected" );
		}

		protected string WrongTypeMessage(Exception exception)
		{
			return CombineWithUserMessage(
				"An unexpected exception type was thrown" + Environment.NewLine +
				"Expected: " + expectedExceptionName + Environment.NewLine +
				" but was: " + exception.GetType().FullName + " : " + exception.Message );
		}

		protected string WrongTextMessage(Exception exception)
		{
			string expectedText;
			switch (matchType)
			{
				default:
				case "Exact":
					expectedText = "Expected: ";
					break;
				case "Contains":
					expectedText = "Expected message containing: ";
					break;
				case "Regex":
					expectedText = "Expected message matching: ";
					break;
			}

			return CombineWithUserMessage(
				"The exception message text was incorrect" + Environment.NewLine +
				expectedText + expectedMessage + Environment.NewLine +
				" but was: " + exception.Message );
		}

		private string CombineWithUserMessage( string message )
		{
			if ( userMessage == null )
				return message;
			return userMessage + Environment.NewLine + message;
		}
        #endregion
    }
}
