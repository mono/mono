// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************
namespace NUnit.Core
{
	using System;
	using System.Threading;
	using System.Collections.Specialized;

	/// <summary>
	/// ThreadedTestRunner overrides the Run and BeginRun methods 
	/// so that they are always run on a separate thread. The actual
	/// </summary>
	public class ThreadedTestRunner : ProxyTestRunner
	{
		#region Instance Variables
		private TestRunnerThread testRunnerThread;
		#endregion

		#region Constructors
		public ThreadedTestRunner( TestRunner testRunner ) : base ( testRunner ) { }
		#endregion

		#region Overrides
		public override TestResult Run( EventListener listener )
		{
			BeginRun( listener );
			return EndRun();
		}

		public override TestResult Run( EventListener listener, ITestFilter filter )
		{
			BeginRun( listener, filter );
			return EndRun();
		}

		public override void BeginRun( EventListener listener )
		{
			testRunnerThread = new TestRunnerThread( this.TestRunner );

			testRunnerThread.StartRun( listener );
		}

		public override void BeginRun( EventListener listener, ITestFilter filter )
		{
			testRunnerThread = new TestRunnerThread( this.TestRunner );

			testRunnerThread.StartRun( listener, filter );
		}

		public override TestResult EndRun()
		{
			this.Wait();
			return this.TestRunner.TestResult;
		}


		public override void Wait()
		{
			if ( testRunnerThread != null )
				testRunnerThread.Wait();
		}

		public override void CancelRun()
		{
			if ( testRunnerThread != null )
				testRunnerThread.Cancel();
		}

		#endregion
	}
}
