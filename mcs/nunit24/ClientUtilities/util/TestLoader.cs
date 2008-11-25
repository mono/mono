// ****************************************************************
// Copyright 2002-2003, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

namespace NUnit.Util
{
	using System;
	using System.IO;
	using System.Collections;
	using System.Threading;
	using System.Configuration;
	using NUnit.Core;
	using NUnit.Core.Filters;


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
	public class TestLoader : MarshalByRefObject, NUnit.Core.EventListener, ITestLoader, IService
	{
		#region Instance Variables

		/// <summary>
		/// Our event dispatching helper object
		/// </summary>
		private TestEventDispatcher events;

		/// <summary>
		/// Use MuiltipleTestDomainRunner if true
		/// </summary>
		private bool multiDomain;

		/// <summary>
		/// Merge namespaces across multiple assemblies
		/// </summary>
		private bool mergeAssemblies;

		/// <summary>
		/// Generate suites for each level of namespace containing tests
		/// </summary>
		private bool autoNamespaceSuites;

		private bool shadowCopyFiles;

		/// <summary>
		/// Loads and executes tests. Non-null when
		/// we have loaded a test.
		/// </summary>
		private TestRunner testRunner = null;

		/// <summary>
		/// Our current test project, if we have one.
		/// </summary>
		private NUnitProject testProject = null;

		/// <summary>
		/// The currently loaded test, returned by the testrunner
		/// </summary>
		private ITest loadedTest = null;

		/// <summary>
		/// The test name that was specified when loading
		/// </summary>
		private string loadedTestName = null;

		/// <summary>
		/// The currently executing test
		/// </summary>
		private string currentTestName;

		/// <summary>
		/// Result of the last test run
		/// </summary>
		private TestResult testResult = null;

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
		/// Indicates whether to automatically rerun
		/// the tests when a change occurs.
		/// </summary>
		private bool rerunOnChange = false;

		/// <summary>
		/// The last filter used for a run - used to 
		/// rerun tests when a change occurs
		/// </summary>
		private ITestFilter lastFilter;

		/// <summary>
		/// Indicates whether to reload the tests
		/// before each run.
		/// </summary>
		private bool reloadOnRun = false;

		#endregion

		#region Constructors

		public TestLoader()
			: this( new TestEventDispatcher() ) { }

		public TestLoader(TestEventDispatcher eventDispatcher )
		{
			this.events = eventDispatcher;

			ISettings settings = Services.UserSettings;
			this.ReloadOnRun = settings.GetSetting( "Options.TestLoader.ReloadOnRun", true );
			this.ReloadOnChange = settings.GetSetting( "Options.TestLoader.ReloadOnChange", true );
			this.RerunOnChange = settings.GetSetting( "Options.TestLoader.RerunOnChange", false );
			this.MultiDomain = settings.GetSetting( "Options.TestLoader.MultiDomain", false );
			this.MergeAssemblies = settings.GetSetting( "Options.TestLoader.MergeAssemblies", false );
			this.AutoNamespaceSuites = settings.GetSetting( "Options.TestLoader.AutoNamespaceSuites", true );
			this.ShadowCopyFiles = settings.GetSetting( "Options.TestLoader.ShadowCopyFiles", true );

			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler( OnUnhandledException );
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

		public bool Running
		{
			get { return testRunner != null && testRunner.Running; }
		}

		public NUnitProject TestProject
		{
			get { return testProject; }
			set	{ OnProjectLoad( value ); }
		}

		public ITestEvents Events
		{
			get { return events; }
		}

		public string TestFileName
		{
			get { return testProject.ProjectPath; }
		}

		public TestResult TestResult
		{
			get { return testResult; }
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

		public bool RerunOnChange
		{
			get { return rerunOnChange; }
			set { rerunOnChange = value; }
		}

		public bool ReloadOnRun
		{
			get { return reloadOnRun; }
			set { reloadOnRun = value; }
		}

		public bool MultiDomain
		{
			get { return multiDomain; }
			set { multiDomain = value; }
		}

		public bool MergeAssemblies
		{
			get { return mergeAssemblies; }
			set { mergeAssemblies = value; }
		}

		public bool AutoNamespaceSuites
		{
			get { return autoNamespaceSuites; }
			set { autoNamespaceSuites = value; }
		}

		public bool ShadowCopyFiles
		{
			get { return shadowCopyFiles; }
			set { shadowCopyFiles = value; }
		}

		public IList AssemblyInfo
		{
			get { return testRunner == null ? null : testRunner.AssemblyInfo; }
		}

		public int TestCount
		{
			get { return loadedTest == null ? 0 : loadedTest.TestCount; }
		}
		#endregion

		#region EventListener Handlers

		void EventListener.RunStarted(string name, int testCount)
		{
			events.FireRunStarting( name, testCount );
		}

		void EventListener.RunFinished(NUnit.Core.TestResult testResult)
		{
			this.testResult = testResult;

			try
			{
				this.SaveLastResult( 
					Path.Combine( Path.GetDirectoryName( this.TestFileName ), "TestResult.xml" ) );
				events.FireRunFinished( testResult );
			}
			catch( Exception ex )
			{
				this.lastException = ex;
				events.FireRunFinished( ex );
			}
		}

		void EventListener.RunFinished(Exception exception)
		{
			this.lastException = exception;
			events.FireRunFinished( exception );
		}

		/// <summary>
		/// Trigger event when each test starts
		/// </summary>
		/// <param name="testCase">TestCase that is starting</param>
		void EventListener.TestStarted(TestName testName)
		{
			this.currentTestName = testName.FullName;
			events.FireTestStarting( testName );
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
		void EventListener.SuiteStarted(TestName suiteName)
		{
			events.FireSuiteStarting( suiteName );
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
		/// Trigger event when an unhandled exception (other than ThreadAbordException) occurs during a test
		/// </summary>
		/// <param name="exception">The unhandled exception</param>
		void EventListener.UnhandledException(Exception exception)
		{
			events.FireTestException( this.currentTestName, exception );
		}

		void OnUnhandledException( object sender, UnhandledExceptionEventArgs args )
		{
			switch( args.ExceptionObject.GetType().FullName )
			{
				case "System.Threading.ThreadAbortException":
					break;
				case "NUnit.Framework.AssertionException":
				default:
					events.FireTestException( this.currentTestName, (Exception)args.ExceptionObject );
					break;
			}
		}

		/// <summary>
		/// Trigger event when output occurs during a test
		/// </summary>
		/// <param name="testOutput">The test output</param>
		void EventListener.TestOutput(TestOutput testOutput)
		{
			events.FireTestOutput( testOutput );
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
			}
			catch( Exception exception )
			{
				lastException = exception;
				events.FireProjectLoadFailed( filePath, exception );
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
			}
			catch( Exception exception )
			{
				lastException = exception;
				events.FireProjectLoadFailed( "New Project", exception );
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
				case ProjectChangeType.Other:
					if( TestProject.IsLoadable )
						TryToLoadOrReloadTest();
					break;

				case ProjectChangeType.AddConfig:
				case ProjectChangeType.UpdateConfig:
					if ( e.configName == TestProject.ActiveConfigName && TestProject.IsLoadable )
						TryToLoadOrReloadTest();
					break;

				case ProjectChangeType.RemoveConfig:
					if ( IsTestLoaded && TestProject.Configs.Count == 0 )
						UnloadTest();
					break;

				default:
					break;
			}
		}

		private void TryToLoadOrReloadTest()
		{
			if ( IsTestLoaded ) 
				ReloadTest();
			else 
				LoadTest();
		}

		#endregion

		#region Methods for Loading and Unloading Tests

		public void LoadTest()
		{
			LoadTest( null );
		}
		
		public void LoadTest( string testName )
		{
            long startTime = DateTime.Now.Ticks;

			try
			{
				events.FireTestLoading( TestFileName );

				testRunner = CreateRunner();

				bool loaded = testRunner.Load( MakeTestPackage( testName ) );

				loadedTest = testRunner.Test;
				loadedTestName = testName;
				testResult = null;
				reloadPending = false;
			
				if ( ReloadOnChange )
					InstallWatcher( );

				if ( loaded )
					events.FireTestLoaded( TestFileName, loadedTest );
				else
				{
					lastException = new ApplicationException( string.Format ( "Unable to find test {0} in assembly", testName ) );
					events.FireTestLoadFailed( TestFileName, lastException );
				}
			}
			catch( FileNotFoundException exception )
			{
				lastException = exception;

				foreach( string assembly in TestProject.ActiveConfig.Assemblies )
				{
					if ( Path.GetFileNameWithoutExtension( assembly ) == exception.FileName &&
						!PathUtils.SamePathOrUnder( testProject.ActiveConfig.BasePath, assembly ) )
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

            double loadTime = (double)(DateTime.Now.Ticks - startTime) / (double)TimeSpan.TicksPerSecond;
            System.Diagnostics.Trace.WriteLine(string.Format("TestLoader: Loaded in {0} seconds", loadTime)); 
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
					events.FireTestUnloading( fileName );

					RemoveWatcher();

					testRunner.Unload();

					testRunner = null;

					loadedTest = null;
					loadedTestName = null;
					testResult = null;
					reloadPending = false;

					events.FireTestUnloaded( fileName );
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
			try
			{
				events.FireTestReloading( TestFileName );

				testRunner.Load( MakeTestPackage( loadedTestName ) );

				loadedTest = testRunner.Test;
				reloadPending = false;

				events.FireTestReloaded( TestFileName, loadedTest );				
			}
			catch( Exception exception )
			{
				lastException = exception;
				events.FireTestReloadFailed( TestFileName, exception );
			}
		}

		/// <summary>
		/// Handle watcher event that signals when the loaded assembly
		/// file has changed. Make sure it's a real change before
		/// firing the SuiteChangedEvent. Since this all happens
		/// asynchronously, we use an event to let ui components
		/// know that the failure happened.
		/// </summary>
		public void OnTestChanged( string testFileName )
		{
			if ( Running )
				reloadPending = true;
			else
			{
				ReloadTest();

				if ( rerunOnChange && lastFilter != null )
					testRunner.BeginRun( this, lastFilter );
			}
		}
		#endregion

		#region Methods for Running Tests
		/// <summary>
		/// Run all the tests
		/// </summary>
		public void RunTests()
		{
			RunTests( TestFilter.Empty );
		}

		/// <summary>
		/// Run selected tests using a filter
		/// </summary>
		/// <param name="filter">The filter to be used</param>
		public void RunTests( ITestFilter filter )
		{
			if ( !Running )
			{
				if ( reloadPending || ReloadOnRun )
					ReloadTest();

				this.lastFilter = filter;
				testRunner.BeginRun( this, filter );
			}
		}

		/// <summary>
		/// Cancel the currently running test.
		/// Fail silently if there is none to
		/// allow for latency in the UI.
		/// </summary>
		public void CancelTestRun()
		{
			if ( Running )
				testRunner.CancelRun();
		}

		public IList GetCategories() 
		{
			CategoryManager categoryManager = new CategoryManager();
			categoryManager.AddAllCategories( this.loadedTest );
			ArrayList list = new ArrayList( categoryManager.Categories );
			list.Sort();
			return list;
		}
		#endregion

		public void SaveLastResult( string fileName )
		{
			XmlResultVisitor resultVisitor 
				= new XmlResultVisitor( fileName, this.testResult );
			this.testResult.Accept(resultVisitor);
			resultVisitor.Write();
		}

		#region Helper Methods

		/// <summary>
		/// Install our watcher object so as to get notifications
		/// about changes to a test.
		/// </summary>
		private void InstallWatcher()
		{
			if(watcher!=null) watcher.Stop();

			watcher = new AssemblyWatcher( 1000, TestProject.ActiveConfig.Assemblies.ToArray() );
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

		private TestRunner CreateRunner()
		{
			TestRunner runner = multiDomain
				? (TestRunner)new MultipleTestDomainRunner()
				: (TestRunner)new TestDomain();
				
			return runner;
		}

		private TestPackage MakeTestPackage( string testName )
		{
			TestPackage package = TestProject.ActiveConfig.MakeTestPackage();
			package.TestName = testName;
			package.Settings["MergeAssemblies"] = mergeAssemblies;
			package.Settings["AutoNamespaceSuites"] = autoNamespaceSuites;
			package.Settings["ShadowCopyFiles"] = shadowCopyFiles;
			return package;
		}
		#endregion

		#region InitializeLifetimeService Override
		public override object InitializeLifetimeService()
		{
			return null;
		}
		#endregion

		#region IService Members

		public void UnloadService()
		{
			// TODO:  Add TestLoader.UnloadService implementation
		}

		public void InitializeService()
		{
			// TODO:  Add TestLoader.InitializeService implementation
		}

		#endregion
	}
}
