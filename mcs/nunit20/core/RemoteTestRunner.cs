#region Copyright (c) 2002-2003, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole, Philip A. Craig
/************************************************************************************
'
' Copyright © 2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' Copyright © 2000-2003 Philip A. Craig
'
' This software is provided 'as-is', without any express or implied warranty. In no 
' event will the authors be held liable for any damages arising from the use of this 
' software.
' 
' Permission is granted to anyone to use this software for any purpose, including 
' commercial applications, and to alter it and redistribute it freely, subject to the 
' following restrictions:
'
' 1. The origin of this software must not be misrepresented; you must not claim that 
' you wrote the original software. If you use this software in a product, an 
' acknowledgment (see the following) in the product documentation is required.
'
' Portions Copyright © 2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' or Copyright © 2000-2003 Philip A. Craig
'
' 2. Altered source versions must be plainly marked as such, and must not be 
' misrepresented as being the original software.
'
' 3. This notice may not be removed or altered from any source distribution.
'
'***********************************************************************************/
#endregion

namespace NUnit.Core
{
	using System;
	using System.IO;
	using System.Collections;
	using System.Reflection;
	using System.Threading;
	using System.Runtime.Remoting;

	/// <summary>
	/// Summary description for RemoteTestRunner.
	/// </summary>
	/// 
	[Serializable]
	public class RemoteTestRunner : LongLivingMarshalByRefObject, TestRunner, EventListener
	{
		#region Instance variables

		/// <summary>
		/// The loaded test suite
		/// </summary>
		private TestSuite suite;

		/// <summary>
		/// TestRunner thread used for asynchronous running
		/// </summary>
		private TestRunnerThread runningThread;

		/// <summary>
		/// Our writer for standard output
		/// </summary>
		private TextWriter outText;

		/// <summary>
		/// Our writer for error output
		/// </summary>
		private TextWriter errorText;

		/// <summary>
		/// Buffered standard output writer created for each test run
		/// </summary>
		private BufferedStringTextWriter outBuffer;

		/// <summary>
		/// Buffered error writer created for each test run
		/// </summary>
		private BufferedStringTextWriter errorBuffer;

		/// <summary>
		/// Console standard output to restore after a run
		/// </summary>
		private TextWriter saveOut;

		/// <summary>
		/// Console error output to restore after a run
		/// </summary>
		private TextWriter saveError;

		/// <summary>
		/// Saved current directory to restore after a run
		/// </summary>
		private string currentDirectory;

		/// <summary>
		/// Saved paths of the assemblies we loaded - used to set 
		/// current directory when we are running the tests.
		/// </summary>
		private string[] assemblies;

		/// <summary>
		/// Dispatcher used to put out runner's test events
		/// </summary>
		private TestEventDispatcher events = new TestEventDispatcher();

		private EventListener listener; // Temp

		private Version frameworkVersion;

		private IFilter filter;

		private bool displayTestLabels;

		/// <summary>
		/// Results from the last test run
		/// </summary>
		private TestResult[] results;

		#endregion

		#region Constructors

		/// <summary>
		/// Construct with stdOut and stdErr writers
		/// </summary>
		public RemoteTestRunner( TextWriter outText, TextWriter errorText )
		{
			this.outText = outText;
			this.errorText = errorText;
		}

		/// <summary>
		/// Default constructor uses Null writers.
		/// </summary>
		public RemoteTestRunner() : this( TextWriter.Null, TextWriter.Null ) { }

		#endregion

		#region Properties

		/// <summary>
		/// Writer for standard output - this is a public property
		/// so that we can set it when creating an instance
		/// in another AppDomain.
		/// </summary>
		public TextWriter Out
		{
			get { return outText; }
			set { outText = value; }
		}

		/// <summary>
		/// Writer for error output - this is a public property
		/// so that we can set it when creating an instance
		/// in another AppDomain.
		/// </summary>
		public TextWriter Error
		{
			get { return errorText; }
			set { errorText = value; }
		}

		/// <summary>
		/// Interface to the events sourced by the runner
		/// </summary>
		public ITestEvents Events
		{
			get { return events; }
		}

		public Version FrameworkVersion
		{
			get { return frameworkVersion; }
		}

		public bool DisplayTestLabels
		{
			get { return displayTestLabels; }
			set { displayTestLabels = value; }
		}

		/// <summary>
		/// Results from the last test run
		/// </summary>
		public TestResult[] Results
		{
			get { return results; }
		}

		/// <summary>
		/// First (or only) result from the last test run
		/// </summary>
		public TestResult Result
		{
			get { return results == null ? null : results[0]; }
		}

		#endregion

		#region Methods for Loading Tests

		/// <summary>
		/// Load an assembly
		/// </summary>
		/// <param name="assemblyName"></param>
		public Test Load( string assemblyName )
		{
			this.assemblies = new string[] { assemblyName };
			TestSuiteBuilder builder = new TestSuiteBuilder();
			suite = builder.Build( assemblyName );
			frameworkVersion = builder.FrameworkVersion;
			return suite;
		}

		/// <summary>
		/// Load a particular test in an assembly
		/// </summary>
		public Test Load( string assemblyName, string testName )
		{
			this.assemblies = new string[] { assemblyName };
			TestSuiteBuilder builder = new TestSuiteBuilder();
			suite = builder.Build( assemblyName, testName );
			frameworkVersion = builder.FrameworkVersion;
			return suite;
		}

		/// <summary>
		/// Load multiple assemblies
		/// </summary>
		public Test Load( string projectName, string[] assemblies )
		{
			this.assemblies = (string[])assemblies.Clone();
			TestSuiteBuilder builder = new TestSuiteBuilder();
			suite = builder.Build( projectName, assemblies );
			frameworkVersion = builder.FrameworkVersion;
			return suite;
		}

		public Test Load( string projectName, string[] assemblies, string testName )
		{
			this.assemblies = (string[])assemblies.Clone();
			TestSuiteBuilder builder = new TestSuiteBuilder();
			suite = builder.Build( assemblies, testName );
			frameworkVersion = builder.FrameworkVersion;
			return suite;
		}

		public void Unload()
		{
			suite = null; // All for now
			frameworkVersion = null;
		}

		#endregion

		#region Methods for Counting TestCases

		public int CountTestCases()
		{
			return suite.CountTestCases();
		}

		public int CountTestCases( string testName )
		{
			Test test = FindTest( suite, testName );
			return test == null ? 0 : test.CountTestCases();
		}

		public int CountTestCases(string[] testNames ) 
		{
			int count = 0;
			foreach( string testName in testNames)
				count += CountTestCases( testName );

			return count;
		}

		public ICollection GetCategories()
		{
			return CategoryManager.Categories;
		}

		#endregion

		#region Methods for Running Tests

		public void SetFilter( IFilter filter )
		{
			this.filter = filter;
		}

		public TestResult Run( EventListener listener )
		{
			return Run( listener, suite );
		}

		public TestResult Run(NUnit.Core.EventListener listener, string testName )
		{
			if ( testName == null || testName.Length == 0 )
				return Run( listener, suite );
			else
				return Run( listener, FindTest( suite, testName ) );
		}

		public TestResult[] Run(NUnit.Core.EventListener listener, string[] testNames)
		{
			if ( testNames == null || testNames.Length == 0 )
				return new TestResult[] { Run( listener, suite ) };
			else
				return Run( listener, FindTests( suite, testNames ) );
		}

		public void RunTest(NUnit.Core.EventListener listener )
		{
			runningThread = new TestRunnerThread( this );
			runningThread.Run( listener );
		}
		
		public void RunTest(NUnit.Core.EventListener listener, string testName )
		{
			runningThread = new TestRunnerThread( this );
			runningThread.Run( listener, testName );
		}

		public void RunTest(NUnit.Core.EventListener listener, string[] testNames)
		{
			runningThread = new TestRunnerThread( this );
			runningThread.Run( listener, testNames );
		}

		public void CancelRun()
		{
			if ( runningThread != null )
				runningThread.Cancel();

			CleanUpAfterTestRun();
		}

		public void Wait()
		{
			if ( runningThread != null )
				runningThread.Wait();
		}

		#endregion

		#region Helper Routines

		/// <summary>
		/// Private method to run a single test
		/// </summary>
		private TestResult Run( EventListener listener, Test test )
		{
			// Create array with the one test
			Test[] tests = new Test[] { test };
			// Call our workhorse method
			results = Run( listener, tests );
			// Return the first result we got
			return results[0];
		}

		/// <summary>
		/// Private method to run a set of tests. This routine is the workhorse
		/// that is called anytime tests are run.
		/// </summary>
		private TestResult[] Run( EventListener listener, Test[] tests )
		{
			// Create buffered writers for efficiency
			outBuffer = new BufferedStringTextWriter( outText );
			errorBuffer = new BufferedStringTextWriter( errorText );

			// Save previous state of Console. This is needed because Console.Out and
			// Console.Error are static. In the case where the test itself calls this
			// method, we can lose output if we don't save and restore their values.
			// This is exactly what happens when we are testing NUnit itself.
			saveOut = Console.Out;
			saveError = Console.Error;

			// Set Console to go to our buffers. Note that any changes made by
			// the user in the test code or the code it calls will defeat this.
			Console.SetOut( outBuffer );
			Console.SetError( errorBuffer ); 

			// Save the current directory so we can run each test in
			// the same directory as its assembly
			currentDirectory = Environment.CurrentDirectory;
			
			try
			{
				// Create an array for the resuls
				results = new TestResult[ tests.Length ];

				// Signal that we are starting the run
				this.listener = listener;
				listener.RunStarted( tests );
				
				// TODO: Get rid of count
				int count = 0;
				foreach( Test test in tests )
					count += filter == null ? test.CountTestCases() : test.CountTestCases( filter );

				events.FireRunStarting( tests, count );
				
				// Run each test, saving the results
				int index = 0;
				foreach( Test test in tests )
				{
					string assemblyDirectory = Path.GetDirectoryName( this.assemblies[test.AssemblyKey] );

					if ( assemblyDirectory != null && assemblyDirectory != string.Empty )
						Environment.CurrentDirectory = assemblyDirectory;

					results[index++] = test.Run( this, filter );
				}

				// Signal that we are done
				listener.RunFinished( results );
				events.FireRunFinished( results );

				// Return result array
				return results;
			}
			catch( Exception exception )
			{
				// Signal that we finished with an exception
				listener.RunFinished( exception );
				events.FireRunFinished( exception );
				// Rethrow - should we do this?
				throw;
			}
			finally
			{
				CleanUpAfterTestRun();
			}
		}

		private Test FindTest(Test test, string fullName)
		{
			if(test.UniqueName.Equals(fullName)) return test;
			if(test.FullName.Equals(fullName)) return test;
			
			Test result = null;
			if(test is TestSuite)
			{
				TestSuite suite = (TestSuite)test;
				foreach(Test testCase in suite.Tests)
				{
					result = FindTest(testCase, fullName);
					if(result != null) break;
				}
			}

			return result;
		}

		private Test[] FindTests( Test test, string[] names )
		{
			Test[] tests = new Test[ names.Length ];

			int index = 0;
			foreach( string name in names )
				tests[index++] = FindTest( test, name );

			return tests;
		}

		private void CleanUpAfterTestRun()
		{
			// Restore the directory we saved
			if ( currentDirectory != null )
			{
				Environment.CurrentDirectory = currentDirectory;
				currentDirectory = null;
			}

			// Close our output buffers
			if ( outBuffer != null )
			{
				outBuffer.Close();
				outBuffer = null;
			}

			if ( errorBuffer != null )
			{
				errorBuffer.Close();
				errorBuffer = null;
			}

			// Restore previous console values
			if ( saveOut != null )
			{
				Console.SetOut( saveOut );
				saveOut = null;
			}

			if ( saveError != null )
			{
				Console.SetError( saveError );
				saveError = null;
			}
		}

		#endregion

		#region EventListener Members

		public void RunStarted(Test[] tests)
		{
			// TODO:  Remove
		}

		void NUnit.Core.EventListener.RunFinished(TestResult[] results)
		{
			// TODO:  Remove
			outText.Close();
		}

		void NUnit.Core.EventListener.RunFinished(Exception exception)
		{
			// TODO:  Remove
			outText.Close();
		}

		public void TestStarted(TestCase testCase)
		{
			if ( displayTestLabels )
				outText.WriteLine("***** {0}", testCase.FullName );
			
			this.listener.TestStarted( testCase );
			events.FireTestStarting( testCase );
		}

		void NUnit.Core.EventListener.TestFinished(TestCaseResult result)
		{
			listener.TestFinished( result );
			events.FireTestFinished( result );
		}

		public void SuiteStarted(TestSuite suite)
		{
			listener.SuiteStarted( suite );
			events.FireSuiteStarting( suite );
		}

		void NUnit.Core.EventListener.SuiteFinished(TestSuiteResult result)
		{
			listener.SuiteFinished( result );
			events.FireSuiteFinished( result );
		}

		public void UnhandledException(Exception exception)
		{
			listener.UnhandledException( exception );
			events.FireTestException( exception );
		}

		#endregion
	}
}

