// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

namespace NUnit.Core
{
	using System;
	using System.Text;

	/// <summary>
	/// The TestResult abstract class represents
	/// the result of a test and is used to
	/// communicate results across AppDomains.
	/// </summary>
	/// 
	[Serializable]
	public abstract class TestResult
	{
		#region Fields
		/// <summary>
		/// Indicates whether the test was executed or not
		/// </summary>
		private RunState runState;

		/// <summary>
		/// Indicates the result of the test
		/// </summary>
		private ResultState resultState;

		/// <summary>
		/// Indicates the location of a failure
		/// </summary>
        private FailureSite failureSite;

		/// <summary>
		/// The elapsed time for executing this test
		/// </summary>
		private double time = 0.0;

		/// <summary>
		/// The name of the test
		/// </summary>
		private string name;

		/// <summary>
		/// The test that this result pertains to
		/// </summary>
		private TestInfo test;

		/// <summary>
		/// The stacktrace at the point of failure
		/// </summary>
		private string stackTrace;

		/// <summary>
		/// Description of this test
		/// </summary>
		private string description;

		/// <summary>
		/// Message giving the reason for failure
		/// </summary>
		protected string messageString;

		/// <summary>
		/// Number of asserts executed by this test
		/// </summary>
		private int assertCount = 0;

		#endregion

		#region Protected Constructor
		/// <summary>
		/// Protected constructor constructs a test result given
		/// a test and a name.
		/// </summary>
		/// <param name="test">The test to be used</param>
		/// <param name="name">Name for this result</param>
		protected TestResult(TestInfo test, string name)
		{
			this.name = name;
			this.test = test;
            this.RunState = RunState.Runnable;
            if (test != null)
            {
                this.description = test.Description;
                this.runState = test.RunState;
                this.messageString = test.IgnoreReason;
            }
        }
		#endregion

        #region Properties

		/// <summary>
		/// Gets the RunState of the result, which indicates
		/// whether or not it has executed and why.
		/// </summary>
        public RunState RunState
        {
            get { return runState; }
            set { runState = value; }
        }

		/// <summary>
		/// Gets the ResultState of the test result, which 
		/// indicates the success or failure of the test.
		/// </summary>
        public ResultState ResultState
        {
            get { return resultState; }
        }

		/// <summary>
		/// Gets the stage of the test in which a failure
		/// or error occured.
		/// </summary>
        public FailureSite FailureSite
        {
            get { return failureSite; }
        }

		/// <summary>
		/// Indicates whether the test executed
		/// </summary>
        public bool Executed
        {
            get { return runState == RunState.Executed; }
        }

		/// <summary>
		/// Gets the name of the test result
		/// </summary>
        public virtual string Name
        {
            get { return name; }
        }

		/// <summary>
		/// Gets the test associated with this result
		/// </summary>
        public ITest Test
        {
            get { return test; }
        }

		/// <summary>
		/// Indicates whether the test ran successfully
		/// </summary>
        public virtual bool IsSuccess
        {
            // TODO: Redefine this more precisely
            get { return !IsFailure; }
            //get { return resultState == ResultState.Success; }
        }

        /// <summary>
        /// Indicates whether the test failed
        /// </summary>
		// TODO: Distinguish errors from failures
        public virtual bool IsFailure
        {
            get { return resultState == ResultState.Failure || resultState == ResultState.Error; }
        }

		/// <summary>
		/// Gets a description associated with the test
		/// </summary>
        public virtual string Description
        {
            get { return description; }
            set { description = value; }
        }

		/// <summary>
		/// Gets the elapsed time for running the test
		/// </summary>
        public double Time
        {
            get { return time; }
            set { time = value; }
        }

		/// <summary>
		/// Gets the message associated with a test
		/// failure or with not running the test
		/// </summary>
        public string Message
        {
            get { return messageString; }
        }

		/// <summary>
		/// Gets any stacktrace associated with an
		/// error or failure.
		/// </summary>
        public virtual string StackTrace
        {
            get
            {
                return stackTrace;
            }
            set
            {
                stackTrace = value;
            }
        }

		/// <summary>
		/// Gets or sets the count of asserts executed
		/// when running the test.
		/// </summary>
        public int AssertCount
        {
            get { return assertCount; }
            set { assertCount = value; }
        }

        #endregion

        #region Public Methods
        /// <summary>
		/// Mark the test as succeeding
		/// </summary>
		public void Success() 
		{ 
			this.runState = RunState.Executed;
			this.resultState = ResultState.Success; 
		}

		/// <summary>
		/// Mark the test as ignored.
		/// </summary>
		/// <param name="reason">The reason the test was not run</param>
		public void Ignore(string reason)
		{
			Ignore( reason, null );
		}

		/// <summary>
		/// Mark the test as ignored.
		/// </summary>
		/// <param name="ex">The ignore exception that was thrown</param>
		public void Ignore( Exception ex )
		{
			Ignore( ex.Message, BuildStackTrace( ex ) );
		}

		/// <summary>
		/// Mark the test as ignored.
		/// </summary>
		/// <param name="reason">The reason the test was not run</param>
		/// <param name="stackTrace">Stack trace giving the location of the command</param>
		public void Ignore(string reason, string stackTrace)
		{
			NotRun( RunState.Ignored, reason, stackTrace );
		}

		/// <summary>
		/// Mark the test as skipped.
		/// </summary>
		/// <param name="reason">The reason the test was not run</param>
		public void Skip(string reason)
		{
			Skip( reason, null );
		}

		/// <summary>
		/// Mark the test as ignored.
		/// </summary>
		/// <param name="ex">The ignore exception that was thrown</param>
		public void Skip( Exception ex )
		{
			Skip( ex.Message, BuildStackTrace( ex ) );
		}

		/// <summary>
		/// Mark the test as skipped.
		/// </summary>
		/// <param name="reason">The reason the test was not run</param>
		/// <param name="stackTrace">Stack trace giving the location of the command</param>
		public void Skip(string reason, string stackTrace)
		{
			NotRun( RunState.Skipped, reason, stackTrace );
		}

		/// <summary>
		/// Mark the test as Not Run - either skipped or ignored
		/// </summary>
		/// <param name="runState">The RunState to use in the result</param>
		/// <param name="reason">The reason the test was not run</param>
		/// <param name="stackTrace">Stack trace giving the location of the command</param>
		public void NotRun(RunState runState, string reason, string stackTrace)
		{
			this.runState = runState;
			this.messageString = reason;
			this.stackTrace = stackTrace;
		}


		/// <summary>
		/// Mark the test as a failure due to an
		/// assertion having failed.
		/// </summary>
		/// <param name="message">Message to display</param>
		/// <param name="stackTrace">Stack trace giving the location of the failure</param>
		public void Failure(string message, string stackTrace)
        {
            Failure(message, stackTrace, FailureSite.Test);
        }

		/// <summary>
		/// Mark the test as a failure due to an
		/// assertion having failed.
		/// </summary>
		/// <param name="message">Message to display</param>
		/// <param name="stackTrace">Stack trace giving the location of the failure</param>
		/// <param name="failureSite">The site of the failure</param>
		public void Failure(string message, string stackTrace, FailureSite failureSite )
		{
			this.runState = RunState.Executed;
			this.resultState = ResultState.Failure;
            this.failureSite = failureSite;
			this.messageString = message;
			this.stackTrace = stackTrace;
		}

		/// <summary>
		/// Marks the result as an error due to an exception thrown
		/// by the test.
		/// </summary>
		/// <param name="exception">The exception that was caught</param>
        public void Error(Exception exception)
        {
            Error(exception, FailureSite.Test);
        }

		/// <summary>
		/// Marks the result as an error due to an exception thrown
		/// from the indicated FailureSite.
		/// </summary>
		/// <param name="exception">The exception that was caught</param>
		/// <param name="failureSite">The site from which it was thrown</param>
		public void Error( Exception exception, FailureSite failureSite )
		{
			this.runState = RunState.Executed;
			this.resultState = ResultState.Error;
            this.failureSite = failureSite;

            string message = BuildMessage(exception);
            string stackTrace = BuildStackTrace(exception);

            if (failureSite == FailureSite.TearDown)
            {
                message = "TearDown : " + message;
                stackTrace = "--TearDown" + Environment.NewLine + stackTrace;

                if (this.messageString != null)
                    message = this.messageString + Environment.NewLine + message;
                if (this.stackTrace != null)
                    stackTrace = this.stackTrace + Environment.NewLine + stackTrace;
            }

            this.messageString = message;
            this.stackTrace = stackTrace;
		}
		#endregion

		#region Exception Helpers

		private string BuildMessage(Exception exception)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat( "{0} : {1}", exception.GetType().ToString(), exception.Message );

			Exception inner = exception.InnerException;
			while( inner != null )
			{
				sb.Append( Environment.NewLine );
				sb.AppendFormat( "  ----> {0} : {1}", inner.GetType().ToString(), inner.Message );
				inner = inner.InnerException;
			}

			return sb.ToString();
		}
		
		private string BuildStackTrace(Exception exception)
		{
            StringBuilder sb = new StringBuilder( GetStackTrace( exception ) );

            Exception inner = exception.InnerException;
            while( inner != null )
            {
                sb.Append( Environment.NewLine );
                sb.Append( "--" );
                sb.Append( inner.GetType().Name );
                sb.Append( Environment.NewLine );
                sb.Append( GetStackTrace( inner ) );

                inner = inner.InnerException;
            }

            return sb.ToString();
		}

		private string GetStackTrace(Exception exception)
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

		/// <summary>
		/// Abstract method that accepts a ResultVisitor
		/// </summary>
		/// <param name="visitor">The visitor</param>
		public abstract void Accept(ResultVisitor visitor);
	}
}
