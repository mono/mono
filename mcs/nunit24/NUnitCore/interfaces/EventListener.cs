// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

namespace NUnit.Core
{
	using System;

	/// <summary>
	/// The EventListener interface is used within the NUnit core to receive 
	/// notifications of significant events while a test is being run. These
	/// events are propogated to any client, which may choose to convert them
	/// to .NET events or to use them directly.
	/// </summary>
	public interface EventListener
	{
		/// <summary>
		/// Called when a test run is starting
		/// </summary>
		/// <param name="name">The name of the test being started</param>
		/// <param name="testCount">The number of test cases under this test</param>
		void RunStarted( string name, int testCount );

		/// <summary>
		/// Called when a run finishes normally
		/// </summary>
		/// <param name="result">The result of the test</param>
		void RunFinished( TestResult result );

		/// <summary>
		/// Called when a run is terminated due to an exception
		/// </summary>
		/// <param name="exception">Exception that was thrown</param>
		void RunFinished( Exception exception );

		/// <summary>
		/// Called when a test case is starting
		/// </summary>
		/// <param name="testName">The name of the test case</param>
		void TestStarted(TestName testName);
			
		/// <summary>
		/// Called when a test case has finished
		/// </summary>
		/// <param name="result">The result of the test</param>
		void TestFinished(TestCaseResult result);

		/// <summary>
		/// Called when a suite is starting
		/// </summary>
		/// <param name="testName">The name of the suite</param>
		void SuiteStarted(TestName testName);

		/// <summary>
		/// Called when a suite has finished
		/// </summary>
		/// <param name="result">The result of the suite</param>
		void SuiteFinished(TestSuiteResult result);

		/// <summary>
		/// Called when an unhandled exception is detected during
		/// the execution of a test run.
		/// </summary>
		/// <param name="exception">The exception thta was detected</param>
		void UnhandledException( Exception exception );

		/// <summary>
		/// Called when the test direts output to the console.
		/// </summary>
		/// <param name="testOutput">A console message</param>
		void TestOutput(TestOutput testOutput);
	}
}
