using System;
using System.Collections;
using System.IO;
using System.Threading;

namespace NUnit.Core
{
	/// <summary>
	/// The TestRunner Interface is allows client code, such as the NUnit console and
	/// gui runners, to load and run tests. This is the lowest level interface generally
	/// supported for running tests and is implemented by the RemoteTestRunner class in
	/// the NUnit core as well as by other classes running on the client side.
	/// 
	/// The Load family of methods is used to load a suite of tests from one or more 
	/// assemblies, returning the resulting test suite to the caller.
	/// 
	/// The CountTestCases family of methods returns the number of test cases in the
	/// loaded suite, either in its entirety or by taking a subset of tests as roots.
	/// 
	/// The Run family of methods performs a test run synchronously, returning a TestResult
	/// or TestResult[] to the caller. If provided, an EventListener interface will be 
	/// notified of significant events in the running of the tests.
	/// 
	/// The RunTest family of methods uses the same set of signatures as Run but operates
	/// asynchronously. The final result of the run may be obtained through the user of an
	/// EventListener or through the Results property.
	/// </summary>
	public interface TestRunner
	{
		#region Properties

		/// <summary>
		/// IsTestRunning indicates whether a test is in progress. MayTo retrieve the
		/// results from an asynchronous test run, wait till IsTestRunning is false.
		/// </summary>
		//		bool IsTestRunning
		//		{
		//			get;
		//		}

		Version FrameworkVersion
		{
			get;
		}

		/// <summary>
		/// Setting to show a header line for each test case in
		/// the console output.
		/// </summary>
		bool DisplayTestLabels
		{
			get; set;
		}

		/// <summary>
		/// Results from the last test run
		/// </summary>
		TestResult[] Results
		{
			get;
		}

		/// <summary>
		/// First (or only) result from the last test run
		/// </summary>
		TestResult Result
		{
			get;
		}
		
		#endregion

		/// <summary>
		/// Load all tests from an assembly
		/// </summary>
		/// <param name="assemblyName">The assembly from which tests are to be loaded</param>
		Test Load( string assemblyName );

		/// <summary>
		/// Load a particular test in an assembly
		/// </summary>
		/// <param name="assemblyName">The assembly from which tests are to be loaded</param>
		/// <param name="testName">The name of the test fixture or suite to be loaded</param>
		Test Load( string assemblyName, string testName );

		/// <summary>
		/// Load multiple assemblies
		/// </summary>
		/// <param name="projectName">The project name to use for the root test node</param>
		/// <param name="assemblies">The assemblies from which tests are to be loaded</param>
		Test Load( string projectName, string[] assemblies );

		/// <summary>
		/// Load a particular test in a set of assemblies
		/// </summary>
		/// <param name="projectName">The project name to use for the root test node</param>
		/// <param name="assemblies">The assemblies from which tests are to be loaded</param>
		/// <param name="testName">The name of the test fixture or suite to be loaded</param>
		Test Load( string projectName, string[] assemblies, string testName );

		/// <summary>
		/// Unload all tests previously loaded
		/// </summary>
		void Unload();

		void SetFilter( IFilter filter );

		/// <summary>
		/// Count test cases previously loaded
		/// </summary>
		/// <returns>The number of test cases found</returns>
		int CountTestCases();

		/// <summary>
		/// Count Test Cases under a given test name
		/// </summary>
		/// <param name="testName">The name of a test case, fixture or suite</param>
		/// <returns>The number of test cases found</returns>
		int CountTestCases(string testName );

		/// <summary>
		/// Count test cases starting at a set of roots
		/// </summary>
		/// <param name="testNames">An array of names of test cases, fixtures or suites</param>
		/// <returns>The number of test cases found</returns>
		int CountTestCases(string[] testNames);

		/// <summary>
		/// Get the collectiion of categories used by the runner;
		/// </summary>
		/// <returns></returns>
		ICollection GetCategories(); 

		/// <summary>
		/// Run the loaded tests using a test filter
		/// </summary>
//		TestResult Run(NUnit.Core.EventListener listener, IFilter filter);

		/// <summary>
		/// Run all loaded tests and return a test result. The test is run synchronously,
		/// and the listener interface is notified as it progresses.
		/// </summary>
		/// <param name="listener">Interface to receive EventListener notifications.</param>
		TestResult Run(NUnit.Core.EventListener listener);
		
		/// <summary>
		/// Run a particular loaded test and return a test result. The test is run
		/// synchronously and the listener interface is notified as it progresses.
		/// </summary>
		/// <param name="listener">Interface to receive EventListener notifications</param>
		/// <param name="testName">The name of the test case, fixture or suite to be run</param>
		TestResult Run(NUnit.Core.EventListener listener, string testName);

		/// <summary>
		/// Run a set of loaded tests and return a set of results.  The test is run
		/// synchronously and the listener interface is notified as it progresses.
		/// </summary>
		/// <param name="listener">Interface to receive EventListener notifications</param>
		/// <param name="testNames">The names of the test cases, fixtures or suites to be run</param>
		TestResult[] Run(NUnit.Core.EventListener listener, string[] testNames);

		/// <summary>
		/// Run all loaded tests. The test is run asynchronously and the listener
		/// interface is notified as it progresses.
		/// </summary>
		/// <param name="listener">Interface to an object to receive EventListener notifications</param>
		void RunTest(NUnit.Core.EventListener listener);
		
		/// <summary>
		/// Run a particular loaded test. The test is run asynchronously and the 
		/// listener interface is notified as it progresses.
		/// </summary>
		/// <param name="listener">Interface to an object to receive EventListener notifications</param>
		/// <param name="testName">The name of the test case, fixture or suite to be run</param>
		void RunTest(NUnit.Core.EventListener listener, string testName);

		/// <summary>
		/// Run a set of loaded tests. The tests are run asynchronously and the
		/// listener interface is notified as it progresses.
		/// </summary>
		/// <param name="listener">Interface to an object to receive EventListener notifications</param>
		/// <param name="testNames">The names of the test cases, fixtures or suites to be run</param>
		void RunTest(NUnit.Core.EventListener listener, string[] testNames);

		void CancelRun();

		void Wait();
	}
}
