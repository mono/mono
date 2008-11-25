// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************
using System;

namespace NUnit.Core
{
	/// <summary>
	/// QueuingEventListener uses an EventQueue to store any
	/// events received on its EventListener interface.
	/// </summary>
	public class QueuingEventListener : EventListener
	{
		private EventQueue events = new EventQueue();

		/// <summary>
		/// The EvenQueue created and filled by this listener
		/// </summary>
		public EventQueue Events
		{
			get { return events; }
		}

		#region EventListener Methods
		/// <summary>
		/// Run is starting
		/// </summary>
		/// <param name="tests">Array of tests to be run</param>
		public void RunStarted( string name, int testCount )
		{
			events.Enqueue( new RunStartedEvent( name, testCount ) );
		}

		/// <summary>
		/// Run finished successfully
		/// </summary>
		/// <param name="results">Array of test results</param>
		public void RunFinished( TestResult result )
		{
			events.Enqueue( new RunFinishedEvent( result ) );
		}

		/// <summary>
		/// Run was terminated due to an exception
		/// </summary>
		/// <param name="exception">Exception that was thrown</param>
		public void RunFinished( Exception exception )
		{
			events.Enqueue( new RunFinishedEvent( exception ) );
		}

		/// <summary>
		/// A single test case is starting
		/// </summary>
		/// <param name="testCase">The test case</param>
		public void TestStarted(TestName testName)
		{
			events.Enqueue( new TestStartedEvent( testName ) );
		}

		/// <summary>
		/// A test case finished
		/// </summary>
		/// <param name="result">Result of the test case</param>
		public void TestFinished(TestCaseResult result)
		{
			events.Enqueue( new TestFinishedEvent( result ) );
		}

		/// <summary>
		/// A suite is starting
		/// </summary>
		/// <param name="suite">The suite that is starting</param>
		public void SuiteStarted(TestName testName)
		{
			events.Enqueue( new SuiteStartedEvent( testName ) );
		}

		/// <summary>
		/// A suite finished
		/// </summary>
		/// <param name="result">Result of the suite</param>
		public void SuiteFinished(TestSuiteResult result)
		{
			events.Enqueue( new SuiteFinishedEvent( result ) );
		}

		/// <summary>
		/// An unhandled exception occured while running a test,
		/// but the test was not terminated.
		/// </summary>
		/// <param name="exception"></param>
		public void UnhandledException( Exception exception )
		{
			events.Enqueue( new UnhandledExceptionEvent( exception ) );
		}

		/// <summary>
		/// A message has been output to the console.
		/// </summary>
		/// <param name="testOutput">A console message</param>
		public void TestOutput( TestOutput output )
		{
			events.Enqueue( new OutputEvent( output ) );
		}
		#endregion
	}
}
