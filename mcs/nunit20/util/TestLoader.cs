#region Copyright (c) 2002-2003, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole, Philip A. Craig
/************************************************************************************
'
' Copyright  2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' Copyright  2000-2002 Philip A. Craig
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
' Portions Copyright  2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' or Copyright  2000-2002 Philip A. Craig
'
' 2. Altered source versions must be plainly marked as such, and must not be 
' misrepresented as being the original software.
'
' 3. This notice may not be removed or altered from any source distribution.
'
'***********************************************************************************/
#endregion

namespace NUnit.Util
{
	using System;
	using System.IO;
	using System.Collections;
	using System.Configuration;
	using System.Threading;
	using NUnit.Core;


	/// <summary>
	/// TestLoader handles interactions between a test runner and a 
	/// client program - typically the user interface - for the 
	/// purpose of loading, unloading and running tests.
	/// 
	/// It implemements the EventListener interface which is used by 
	/// the test runner and repackages those events, along with
	/// others as individual events that clients may subscribe to
	/// in collaboration with a TestEventDispatcher helper object.
	/// 
	/// TestLoader is quite handy for use with a gui client because
	/// of the large number of events it supports. However, it has
	/// no dependencies on ui components and can be used independently.
	/// </summary>
	public class TestLoader : LongLivingMarshalByRefObject, NUnit.Core.EventListener, ITestLoader
	{
		#region Instance Variables

		/// <summary>
		/// StdOut stream for use by the TestRunner
		/// </summary>
		private TextWriter stdOutWriter;

		/// <summary>
		/// StdErr stream for use by the TestRunner
		/// </summary>
		private TextWriter stdErrWriter;

		/// <summary>
		/// Our event dispatiching helper object
		/// </summary>
		private ProjectEventDispatcher events;

		/// <summary>
		/// Loads and executes tests. Non-null when
		/// we have loaded a test.
		/// </summary>
		private TestDomain testDomain = null;

		/// <summary>
		/// Our current test project, if we have one.
		/// </summary>
		private NUnitProject testProject = null;

		/// <summary>
		/// The currently loaded test, returned by the testrunner
		/// </summary>
		private Test loadedTest = null;

		/// <summary>
		/// The test name that was specified when loading
		/// </summary>
		private string loadedTestName = null;

		/// <summary>
		/// The tests that are running
		/// </summary>
		private ITest[] runningTests = null;

		/// <summary>
		/// Result of the last test run
		/// </summary>
		private TestResult[] results = null;

		/// <summary>
		/// The last exception received when trying to load, unload or run a test
		/// </summary>
		private Exception lastException = null;

		/// <summary>
		/// Watcher fires when the assembly changes
		/// </summary>
		private AssemblyWatcher watcher;

		/// <summary>
		/// Assembly changed during a test and
		/// needs to be reloaded later
		/// </summary>
		private bool reloadPending = false;

		/// <summary>
		/// Indicates whether to watch for changes
		/// and reload the tests when a change occurs.
		/// </summary>
		private bool reloadOnChange = false;

		/// <summary>
		/// Indicates whether to reload the tests
		/// before each run.
		/// </summary>
		private bool reloadOnRun = false;

		private IFilter filter;

		#endregion

		#region Constructor

		public TestLoader(TextWriter stdOutWriter, TextWriter stdErrWriter )
		{
			this.stdOutWriter = stdOutWriter;
			this.stdErrWriter = stdErrWriter;
			this.events = new ProjectEventDispatcher();
		}

		#endregion

		#region Properties

		public bool IsProjectLoaded
		{
			get { return testProject != null; }
		}

		public bool IsTestLoaded
		{
			get { return loadedTest != null; }
		}

		public bool IsTestRunning
		{
			get { return runningTests != null; }
		}

		public NUnitProject TestProject
		{
			get { return testProject; }
			set	{ OnProjectLoad( value ); }
		}

		public IProjectEvents Events
		{
			get { return events; }
		}

		public string TestFileName
		{
			get { return testProject.ProjectPath; }
		}

		public TestResult[] Results
		{
			get { return results; }
		}

		public Exception LastException
		{
			get { return lastException; }
		}

		public bool ReloadOnChange
		{
			get { return reloadOnChange; }
			set { reloadOnChange = value; }
		}

		public bool ReloadOnRun
		{
			get { return reloadOnRun; }
			set { reloadOnRun = value; }
		}

		public Version FrameworkVersion
		{
			get { return this.testDomain.FrameworkVersion; }
		}

		#endregion

		#region EventListener Handlers

		void EventListener.RunStarted(Test[] tests)
		{
			int count = 0;
			foreach( Test test in tests )
				count += filter == null ? test.CountTestCases() : test.CountTestCases( filter );

			events.FireRunStarting( tests, count );
		}

		void EventListener.RunFinished(NUnit.Core.TestResult[] results)
		{
			this.results = results;
			events.FireRunFinished( results );
			runningTests = null;
		}

		void EventListener.RunFinished(Exception exception)
		{
			this.lastException = exception;
			events.FireRunFinished( exception );
			runningTests = null;
		}

		/// <summary>
		/// Trigger event when each test starts
		/// </summary>
		/// <param name="testCase">TestCase that is starting</param>
		void EventListener.TestStarted(NUnit.Core.TestCase testCase)
		{
			events.FireTestStarting( testCase );
		}

		/// <summary>
		/// Trigger event when each test finishes
		/// </summary>
		/// <param name="result">Result of the case that finished</param>
		void EventListener.TestFinished(TestCaseResult result)
		{
			events.FireTestFinished( result );
		}

		/// <summary>
		/// Trigger event when each suite starts
		/// </summary>
		/// <param name="suite">Suite that is starting</param>
		void EventListener.SuiteStarted(TestSuite suite)
		{
			events.FireSuiteStarting( suite );
		}

		/// <summary>
		/// Trigger event when each suite finishes
		/// </summary>
		/// <param name="result">Result of the suite that finished</param>
		void EventListener.SuiteFinished(TestSuiteResult result)
		{
			events.FireSuiteFinished( result );
		}

		/// <summary>
		/// Trigger event when an unhandled exception occurs during a test
		/// </summary>
		/// <param name="exception">The unhandled exception</param>
		void EventListener.UnhandledException(Exception exception)
		{
			events.FireTestException( exception );
		}

		#endregion

		#region Methods for Loading and Unloading Projects
		
		/// <summary>
		/// Create a new project with default naming
		/// </summary>
		public void NewProject()
		{
			try
			{
				events.FireProjectLoading( "New Project" );

				OnProjectLoad( NUnitProject.NewProject() );
			}
			catch( Exception exception )
			{
				lastException = exception;
				events.FireProjectLoadFailed( "New Project", exception );
			}
		}

		/// <summary>
		/// Create a new project using a given path
		/// </summary>
		public void NewProject( string filePath )
		{
			try
			{
				events.FireProjectLoading( filePath );

				NUnitProject project = new NUnitProject( filePath );

				project.Configs.Add( "Debug" );
				project.Configs.Add( "Release" );			
				project.IsDirty = false;

				OnProjectLoad( project );
			}
			catch( Exception exception )
			{
				lastException = exception;
				events.FireProjectLoadFailed( filePath, exception );
			}
		}

		/// <summary>
		/// Load a new project, optionally selecting the config and fire events
		/// </summary>
		public void LoadProject( string filePath, string configName )
		{
			try
			{
				events.FireProjectLoading( filePath );

				NUnitProject newProject = NUnitProject.LoadProject( filePath );
				if ( configName != null ) 
				{
					newProject.SetActiveConfig( configName );
					newProject.IsDirty = false;
				}

				OnProjectLoad( newProject );

//				return true;
			}
			catch( Exception exception )
			{
				lastException = exception;
				events.FireProjectLoadFailed( filePath, exception );

//				return false;
			}
		}

		/// <summary>
		/// Load a new project using the default config and fire events
		/// </summary>
		public void LoadProject( string filePath )
		{
			LoadProject( filePath, null );
		}

		/// <summary>
		/// Load a project from a list of assemblies and fire events
		/// </summary>
		public void LoadProject( string[] assemblies )
		{
			try
			{
				events.FireProjectLoading( "New Project" );

				NUnitProject newProject = NUnitProject.FromAssemblies( assemblies );

				OnProjectLoad( newProject );

//				return true;
			}
			catch( Exception exception )
			{
				lastException = exception;
				events.FireProjectLoadFailed( "New Project", exception );

//				return false;
			}
		}

		/// <summary>
		/// Unload the current project and fire events
		/// </summary>
		public void UnloadProject()
		{
			string testFileName = TestFileName;

			try
			{
				events.FireProjectUnloading( testFileName );

//				if ( testFileName != null && File.Exists( testFileName ) )
//					UserSettings.RecentProjects.RecentFile = testFileName;

				if ( IsTestLoaded )
					UnloadTest();

				testProject.Changed -= new ProjectEventHandler( OnProjectChanged );
				testProject = null;

				events.FireProjectUnloaded( testFileName );
			}
			catch (Exception exception )
			{
				lastException = exception;
				events.FireProjectUnloadFailed( testFileName, exception );
			}

		}

		/// <summary>
		/// Common operations done each time a project is loaded
		/// </summary>
		/// <param name="testProject">The newly loaded project</param>
		private void OnProjectLoad( NUnitProject testProject )
		{
			if ( IsProjectLoaded )
				UnloadProject();

			this.testProject = testProject;
			testProject.Changed += new ProjectEventHandler( OnProjectChanged );

			events.FireProjectLoaded( TestFileName );
		}

		private void OnProjectChanged( object sender, ProjectEventArgs e )
		{
			switch ( e.type )
			{
				case ProjectChangeType.ActiveConfig:
					if( TestProject.IsLoadable )
						LoadTest();
					break;

				case ProjectChangeType.AddConfig:
				case ProjectChangeType.UpdateConfig:
					if ( e.configName == TestProject.ActiveConfigName && TestProject.IsLoadable )
						LoadTest();
					break;

				case ProjectChangeType.RemoveConfig:
					if ( IsTestLoaded && TestProject.Configs.Count == 0 )
						UnloadTest();
					break;

				default:
					break;
			}
		}

		#endregion

		#region Methods for Loading and Unloading Tests

		public void LoadTest()
		{
			LoadTest( null );
		}
		
		public void LoadTest( string testName )
		{
			try
			{
				events.FireTestLoading( TestFileName );

				testDomain = new TestDomain( stdOutWriter, stdErrWriter );		
				Test test = testDomain.Load( TestProject, testName );

				TestSuite suite = test as TestSuite;
				if ( suite != null )
					suite.Sort();
			
				loadedTest = test;
				loadedTestName = testName;
				results = null;
				reloadPending = false;
			
				if ( ReloadOnChange )
					InstallWatcher( );

				if ( suite != null )
					events.FireTestLoaded( TestFileName, this.loadedTest );
				else
				{
					lastException = new ApplicationException( string.Format ( "Unable to find test {0} in assembly", testName ) );
					events.FireTestLoadFailed( TestFileName, lastException );
				}
			}
			catch( FileNotFoundException exception )
			{
				lastException = exception;

				foreach( string assembly in TestProject.ActiveConfig.AbsolutePaths )
				{
					if ( Path.GetFileNameWithoutExtension( assembly ) == exception.FileName &&
						!ProjectPath.SamePathOrUnder( testProject.ActiveConfig.BasePath, assembly ) )
					{
						lastException = new ApplicationException( string.Format( "Unable to load {0} because it is not located under the AppBase", exception.FileName ), exception );
						break;
					}
				}

				events.FireTestLoadFailed( TestFileName, lastException );
			}
			catch( Exception exception )
			{
				lastException = exception;
				events.FireTestLoadFailed( TestFileName, exception );
			}
		}

		/// <summary>
		/// Unload the current test suite and fire the Unloaded event
		/// </summary>
		public void UnloadTest( )
		{
			if( IsTestLoaded )
			{
				// Hold the name for notifications after unload
				string fileName = TestFileName;

				try
				{
					events.FireTestUnloading( TestFileName, this.loadedTest );

					RemoveWatcher();

					testDomain.Unload();

					testDomain = null;

					loadedTest = null;
					loadedTestName = null;
					results = null;
					reloadPending = false;

					events.FireTestUnloaded( fileName, this.loadedTest );
				}
				catch( Exception exception )
				{
					lastException = exception;
					events.FireTestUnloadFailed( fileName, exception );
				}
			}
		}

		/// <summary>
		/// Reload the current test on command
		/// </summary>
		public void ReloadTest()
		{
			OnTestChanged( TestFileName );
		}

		/// <summary>
		/// Handle watcher event that signals when the loaded assembly
		/// file has changed. Make sure it's a real change before
		/// firing the SuiteChangedEvent. Since this all happens
		/// asynchronously, we use an event to let ui components
		/// know that the failure happened.
		/// </summary>
		/// <param name="assemblyFileName">Assembly file that changed</param>
		public void OnTestChanged( string testFileName )
		{
			if ( IsTestRunning )
				reloadPending = true;
			else 
				try
				{
					events.FireTestReloading( testFileName, this.loadedTest );

					// Don't unload the old domain till after the event
					// handlers get a chance to compare the trees.
					TestDomain newDomain = new TestDomain( stdOutWriter, stdErrWriter );
					Test newTest = newDomain.Load( testProject, loadedTestName );
					TestSuite suite = newTest as TestSuite;
					if ( suite != null )
						suite.Sort();

					testDomain.Unload();

					testDomain = newDomain;
					loadedTest = newTest;
					reloadPending = false;

					events.FireTestReloaded( testFileName, newTest );				
				}
				catch( Exception exception )
				{
					lastException = exception;
					events.FireTestReloadFailed( testFileName, exception );
				}
		}

		#endregion

		#region Methods for Running Tests

		public void SetFilter( IFilter filter )
		{
			this.filter = filter;
		}

		/// <summary>
		/// Run the currently loaded top level test suite
		/// </summary>
		public void RunLoadedTest()
		{
			RunTest( loadedTest );
		}

		/// <summary>
		/// Run a testcase or testsuite from the currrent tree
		/// firing the RunStarting and RunFinished events.
		/// Silently ignore the call if a test is running
		/// to allow for latency in the UI.
		/// </summary>
		/// <param name="testName">The test to be run</param>
		public void RunTest( ITest test )
		{
			RunTests( new ITest[] { test } );
		}

		public void RunTests( ITest[] tests )
		{
			if ( !IsTestRunning )
			{
				if ( reloadPending || ReloadOnRun )
					ReloadTest();

				runningTests = tests;

				//kind of silly
				string[] testNames = new string[ runningTests.Length ];
				int index = 0; 
				foreach (ITest node in runningTests) 
					testNames[index++] = node.UniqueName;

				testDomain.SetFilter( filter );
//				testDomain.DisplayTestLabels = UserSettings.Options.TestLabels;
				testDomain.RunTest( this, testNames );
			}
		}

		/// <summary>
		/// Cancel the currently running test.
		/// Fail silently if there is none to
		/// allow for latency in the UI.
		/// </summary>
		public void CancelTestRun()
		{
			if ( IsTestRunning )
				testDomain.CancelRun();
		}

		public IList GetCategories() 
		{
			ArrayList list = new ArrayList();
			list.AddRange(testDomain.GetCategories());
			return list;
		}

		#endregion

		#region Helper Methods

		/// <summary>
		/// Install our watcher object so as to get notifications
		/// about changes to a test.
		/// </summary>
		/// <param name="assemblyFileName">Full path of the assembly to watch</param>
		private void InstallWatcher()
		{
			if(watcher!=null) watcher.Stop();

			watcher = new AssemblyWatcher( 1000, TestProject.ActiveConfig.AbsolutePaths );
			watcher.AssemblyChangedEvent += new AssemblyWatcher.AssemblyChangedHandler( OnTestChanged );
			watcher.Start();
		}

		/// <summary>
		/// Stop and remove our current watcher object.
		/// </summary>
		private void RemoveWatcher()
		{
			if ( watcher != null )
			{
				watcher.Stop();
				watcher = null;
			}
		}

		#endregion
	}
}
