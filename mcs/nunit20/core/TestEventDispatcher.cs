using System;

namespace NUnit.Core
{
	/// <summary>
	/// Helper class used by runner to dispatch test events
	/// </summary>
	public class TestEventDispatcher : ITestEvents
	{
		#region Events

		// Test loading events
		public event TestEventHandler TestLoading;	
		public event TestEventHandler TestLoaded;	
		public event TestEventHandler TestLoadFailed;

		public event TestEventHandler TestReloading;
		public event TestEventHandler TestReloaded;
		public event TestEventHandler TestReloadFailed;

		public event TestEventHandler TestUnloading;
		public event TestEventHandler TestUnloaded;
		public event TestEventHandler TestUnloadFailed;

		// Test running events
		public event TestEventHandler RunStarting;	
		public event TestEventHandler RunFinished;
		
		public event TestEventHandler SuiteStarting;
		public event TestEventHandler SuiteFinished;

		public event TestEventHandler TestStarting;
		public event TestEventHandler TestFinished;

		public event TestEventHandler TestException;

		#endregion

		#region Methods for Firing Events
		
		private void Fire( 
			TestEventHandler handler, TestEventArgs e )
		{
			if ( handler != null )
				handler( this, e );
		}

		public void FireTestLoading( string fileName )
		{
			Fire( 
				TestLoading,
				new TestEventArgs( TestAction.TestLoading, fileName ) );
		}

		public void FireTestLoaded( string fileName, Test test )
		{
			Fire( 
				TestLoaded,
				new TestEventArgs( TestAction.TestLoaded, fileName, test ) );
		}

		public void FireTestLoadFailed( string fileName, Exception exception )
		{
			Fire(
				TestLoadFailed,
				new TestEventArgs( TestAction.TestLoadFailed, fileName, exception ) );
		}

		public void FireTestUnloading( string fileName, Test test )
		{
			Fire(
				TestUnloading,
				new TestEventArgs( TestAction.TestUnloading, fileName, test ) );
		}

		public void FireTestUnloaded( string fileName, Test test )
		{
			Fire(
				TestUnloaded,
				new TestEventArgs( TestAction.TestUnloaded, fileName, test ) );
		}

		public void FireTestUnloadFailed( string fileName, Exception exception )
		{
			Fire(
				TestUnloadFailed, 
				new TestEventArgs( TestAction.TestUnloadFailed, fileName, exception ) );
		}

		public void FireTestReloading( string fileName, Test test )
		{
			Fire(
				TestReloading,
				new TestEventArgs( TestAction.TestReloading, fileName, test ) );
		}

		public void FireTestReloaded( string fileName, Test test )
		{
			Fire(
				TestReloaded,
				new TestEventArgs( TestAction.TestReloaded, fileName, test ) );
		}

		public void FireTestReloadFailed( string fileName, Exception exception )
		{
			Fire(
				TestReloadFailed, 
				new TestEventArgs( TestAction.TestReloadFailed, fileName, exception ) );
		}

		public void FireRunStarting( Test[] tests, int count )
		{
			Fire(
				RunStarting,
				new TestEventArgs( TestAction.RunStarting, tests, count ) );
		}

		public void FireRunFinished( TestResult[] results )
		{	
			Fire(
				RunFinished,
				new TestEventArgs( TestAction.RunFinished, results ) );
		}

		public void FireRunFinished( Exception exception )
		{
			Fire(
				RunFinished,
				new TestEventArgs( TestAction.RunFinished, exception ) );
		}

		public void FireTestStarting( Test test )
		{
			Fire(
				TestStarting,
				new TestEventArgs( TestAction.TestStarting, test ) );
		}

		public void FireTestFinished( TestResult result )
		{	
			Fire(
				TestFinished,
				new TestEventArgs( TestAction.TestFinished, result ) );
		}

		public void FireSuiteStarting( Test test )
		{
			Fire(
				SuiteStarting,
				new TestEventArgs( TestAction.SuiteStarting, test ) );
		}

		public void FireSuiteFinished( TestResult result )
		{	
			Fire(
				SuiteFinished,
				new TestEventArgs( TestAction.SuiteFinished, result ) );
		}

		public void FireTestException( Exception exception )
		{
			Fire(
				TestException,
				new TestEventArgs( TestAction.TestException, exception ) );
		}

		#endregion
	}
}
