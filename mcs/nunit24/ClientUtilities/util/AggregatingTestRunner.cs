// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

//#define RUN_IN_PARALLEL

namespace NUnit.Util
{
	using System;
	using System.Collections;
	using System.IO;
	using NUnit.Core;

	/// <summary>
	/// AggregatingTestRunner allows running multiple TestRunners
	/// and combining the results.
	/// </summary>
	public abstract class AggregatingTestRunner : MarshalByRefObject, TestRunner, EventListener
	{
		static int AggregateTestID = 1000;

		#region Instance Variables

		/// <summary>
		/// Our runner ID
		/// </summary>
		protected int runnerID;

		/// <summary>
		/// The downstream TestRunners
		/// </summary>
		protected ArrayList runners;

		/// <summary>
		/// The loaded test suite
		/// </summary>
		protected TestNode aggregateTest;

		/// <summary>
		/// The result of the last run
		/// </summary>
		private TestResult testResult;

		/// <summary>
		/// The event listener for the currently running test
		/// </summary>
		protected EventListener listener;

		protected string projectName;

		protected TestName testName;

		#endregion

		#region Constructors
		public AggregatingTestRunner() : this( 0 ) { }
		public AggregatingTestRunner( int runnerID )
		{
			this.runnerID = runnerID;
			this.testName = new TestName();
			testName.TestID = new TestID( AggregateTestID );
			testName.RunnerID = this.runnerID;
			testName.FullName = testName.Name = "Not Loaded";
		}
		#endregion

		#region Properties

		public virtual int ID
		{
			get { return runnerID; }
		}

		public virtual bool Running
		{
			get 
			{ 
				foreach( TestRunner runner in runners )
					if ( runner.Running )
						return true;
			
				return false;
			}
		}

		public virtual IList AssemblyInfo
		{
			get
			{
				ArrayList info = new ArrayList();
				foreach( TestRunner runner in runners )
					info.AddRange( runner.AssemblyInfo );
				return info;
			}
		}

		public virtual ITest Test
		{
			get
			{
				if ( aggregateTest == null && runners != null )
				{
					// Count non-null tests, in case we specified a fixture
					int count = 0;
					foreach( TestRunner runner in runners )
						if ( runner.Test != null )
							++count;  

					// Copy non-null tests to an array
					int index = 0;
					ITest[] tests = new ITest[count];
					foreach( TestRunner runner in runners )
						if ( runner.Test != null )
							tests[index++] = runner.Test;

					// Return master node containing all the tests
					aggregateTest = new TestNode( testName, tests );
				}

				return aggregateTest;
			}
		}

		public virtual TestResult TestResult
		{
			get { return testResult; }
		}
		#endregion

		#region Load and Unload Methods       
		public abstract bool Load(TestPackage package);

		public virtual void Unload()
		{
			foreach( TestRunner runner in runners )
				runner.Unload();
			aggregateTest = null;
		}
		#endregion

		#region CountTestCases
		public virtual int CountTestCases( ITestFilter filter )
		{
			int count = 0;
			foreach( TestRunner runner in runners )
				count += runner.CountTestCases( filter );
			return count;
		}
		#endregion

		#region Methods for Running Tests
		public virtual TestResult Run( EventListener listener )
		{
			return Run( listener, TestFilter.Empty );
		}

		public virtual TestResult Run(EventListener listener, ITestFilter filter )
		{
			// Save active listener for derived classes
			this.listener = listener;

			ITest[] tests = new ITest[runners.Count];
			for( int index = 0; index < runners.Count; index++ )
				tests[index] = ((TestRunner)runners[index]).Test;

			this.listener.RunStarted( this.Test.TestName.Name, this.CountTestCases( filter ) );

			this.listener.SuiteStarted( this.Test.TestName );
			long startTime = DateTime.Now.Ticks;

			TestSuiteResult result = new TestSuiteResult( new TestInfo( testName, tests ), projectName );
			result.RunState = RunState.Executed;
			foreach( TestRunner runner in runners )
				if ( filter.Pass( runner.Test ) )
					result.AddResult( runner.Run( this, filter ) );
			
			long stopTime = DateTime.Now.Ticks;
			double time = ((double)(stopTime - startTime)) / (double)TimeSpan.TicksPerSecond;
			result.Time = time;

			this.listener.SuiteFinished( result );

			this.listener.RunFinished( result );

			this.testResult = result;

			return result;
		}

		public virtual void BeginRun( EventListener listener )
		{
			BeginRun( listener, TestFilter.Empty );
		}

		public virtual void BeginRun( EventListener listener, ITestFilter filter )
		{
			// Save active listener for derived classes
			this.listener = listener;

#if RUN_IN_PARALLEL
			this.listener.RunStarted( this.Test.Name, this.CountTestCases( filter ) );

			foreach( TestRunner runner in runners )
				if ( runner.Test != null )
					runner.BeginRun( this, filter );

			//this.listener.RunFinished( this.Results );
#else
			ThreadedTestRunner threadedRunner = new ThreadedTestRunner( this );
			threadedRunner.BeginRun( listener, filter );
#endif
		}

		public virtual TestResult EndRun()
		{
			TestSuiteResult suiteResult = new TestSuiteResult( aggregateTest, Test.TestName.FullName );
			foreach( TestRunner runner in runners )
				suiteResult.Results.Add( runner.EndRun() );

			return suiteResult;
		}

		public virtual void CancelRun()
		{
			foreach( TestRunner runner in runners )
				runner.CancelRun();
		}

		public virtual void Wait()
		{
			foreach( TestRunner runner in runners )
				runner.Wait();
		}
		#endregion

		#region EventListener Members
		public void TestStarted(TestName testName)
		{
			this.listener.TestStarted( testName );
		}

		public void RunStarted(string name, int testCount)
		{
			// TODO: We may want to count how many runs are started
			// Ignore - we provide our own
		}

		public void RunFinished(Exception exception)
		{
			// Ignore - we provide our own
		}

		void NUnit.Core.EventListener.RunFinished(TestResult result)
		{
			// TODO: Issue combined RunFinished when all runs are done
		}

		public void SuiteFinished(TestSuiteResult result)
		{
			this.listener.SuiteFinished( result );
		}

		public void TestFinished(TestCaseResult result)
		{
			this.listener.TestFinished( result );
		}

		public void UnhandledException(Exception exception)
		{
			this.listener.UnhandledException( exception );
		}

		public void TestOutput(TestOutput testOutput)
		{
			this.listener.TestOutput( testOutput );
		}

		public void SuiteStarted(TestName suiteName)
		{
			this.listener.SuiteStarted( suiteName );
		}
		#endregion

		#region InitializeLifetimeService Override
		public override object InitializeLifetimeService()
		{
			return null;
		}
		#endregion
	}
}
