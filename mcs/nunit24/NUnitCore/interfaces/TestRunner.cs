// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System;
using System.Collections;
using System.IO;

namespace NUnit.Core
{
	/// <summary>
	/// The TestRunner Interface allows client code, such as the NUnit console and
	/// gui runners, to load and run tests. This is the lowest level interface generally
	/// supported for running tests and is implemented by the RemoteTestRunner class in
	/// the NUnit core as well as by other classes running on the client side.
	/// 
	/// The Load method is used to load a suite of tests from one or more 
	/// assemblies, returning a tree of TestNodes to the caller.
	/// 
	/// The CountTestCases family of methods returns the number of test cases in the
	/// loaded suite, either in its entirety or by using a filter to count a subset of tests.
	/// 
	/// The Run family of methods performs a test run synchronously, returning a TestResult
	/// or TestResult[] to the caller. If provided, an EventListener interface will be 
	/// notified of significant events in the running of the tests. A filter may be used
    /// to run a subset of the tests.
    ///
    /// BeginRun and EndRun provide a simplified form of the asynchronous invocation
	/// pattern used in many places within the .NET framework. Because the current
	/// implementation allows only one run to be in process at a time, an IAsyncResult
	/// is not used at this time.
    /// 
    /// Methods to cancel a run and to wait for a run to complete are also provided. The 
    /// result of the last run may be obtained by querying the TestResult property.
    /// 
    /// </summary>
	public interface TestRunner
	{
		#region Properties
		/// <summary>
		/// TestRunners are identified by an ID. So long as there
		/// is only one test runner or a single chain of test runners,
		/// the default id of 0 may be used. However, any client that
		/// creates multiple runners must ensure that each one has a
		/// unique ID in order to locate and run specific tests.
		/// </summary>
		int ID
		{
			get;
		}

		/// <summary>
		/// IsTestRunning indicates whether a test is in progress. To retrieve the
		/// results from an asynchronous test run, wait till IsTestRunning is false.
		/// </summary>
		bool Running
		{
			get;
		}

		/// <summary>
		/// Returns information about loaded assemblies
		/// </summary>
		IList AssemblyInfo
		{
			get;
		}

		/// <summary>
		/// The loaded test, converted to a tree of TestNodes so they can be
		/// serialized and marshalled to a remote client.
		/// </summary>
		ITest Test
		{
			get;
		}

		/// <summary>
		/// Result of the last test run.
		/// </summary>
		TestResult TestResult
		{
			get;
		}
		#endregion

		#region Load and Unload Methods
		/// <summary>
		/// Load the assemblies in a test package
		/// </summary>
		/// <param name="package">The test package to be loaded</param>
		/// <returns>True if the tests were loaded successfully, otherwise false</returns>
		bool Load( TestPackage package );

		/// <summary>
		/// Unload all tests previously loaded
		/// </summary>
		void Unload();
		#endregion

		#region CountTestCases Methods
		/// <summary>
		/// Count Test Cases using a filter
		/// </summary>
		/// <param name="filter">The filter to apply</param>
		/// <returns>The number of test cases found</returns>
		int CountTestCases(ITestFilter filter );
		#endregion

		#region Run Methods
		/// <summary>
		/// Run all loaded tests and return a test result. The test is run synchronously,
		/// and the listener interface is notified as it progresses.
		/// </summary>
		/// <param name="listener">Interface to receive EventListener notifications.</param>
		TestResult Run(NUnit.Core.EventListener listener);

		/// <summary>
		/// Run selected tests and return a test result. The test is run synchronously,
		/// and the listener interface is notified as it progresses.
		/// </summary>
		/// <param name="listener">Interface to receive EventListener notifications.</param>
		/// <param name="filter">The filter to apply when running the tests</param>
		TestResult Run(NUnit.Core.EventListener listener, ITestFilter filter);
		
		/// <summary>
		/// Start a run of all loaded tests. The tests are run aynchronously and the 
		/// listener interface is notified as it progresses.
		/// </summary>
		/// <param name="listener">Interface to receive EventListener notifications.</param>
		void BeginRun(NUnit.Core.EventListener listener);

		/// <summary>
		/// Start a run of selected tests. The tests are run aynchronously and the 
		/// listener interface is notified as it progresses.
		/// </summary>
		/// <param name="listener">Interface to receive EventListener notifications.</param>
		/// <param name="filter">The filter to apply when running the tests</param>
		void BeginRun(NUnit.Core.EventListener listener, ITestFilter filter);
		
		/// <summary>
		/// Wait for an asynchronous run to complete and return the result.
		/// </summary>
		/// <returns>A TestResult for the entire run</returns>
		TestResult EndRun();

		/// <summary>
		///  Cancel the test run that is in progress. For a synchronous run,
		///  a client wanting to call this must create a separate run thread.
		/// </summary>
		void CancelRun();

		/// <summary>
		/// Wait for the test run in progress to complete. For a synchronous run,
		/// a client wanting to call this must create a separate run thread. In
		/// particular, a gui client calling this method is likely to hang, since
		/// events will not be able to invoke methods on the gui thread.
		/// </summary>
		void Wait();
		#endregion
	}
}

