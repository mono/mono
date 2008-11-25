// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;
using System.Diagnostics;
using NUnit.Core;

namespace NUnit.Util
{
	#region TestEventHandler delegate

	/// <summary>
	/// The delegates for all events related to running tests
	/// </summary>
	public delegate void TestEventHandler ( object sender, TestEventArgs args );

	#endregion

	#region TestAction enumeration

	/// <summary>
	/// Enumeration used to distiguish test events
	/// </summary>
	public enum TestAction
	{
		// Project Load Events
		ProjectLoading,
		ProjectLoaded,
		ProjectLoadFailed,
		ProjectUnloading,
		ProjectUnloaded,
		ProjectUnloadFailed,
		// Test Load Events
		TestLoading,
		TestLoaded,
		TestLoadFailed,
		TestReloading,
		TestReloaded,
		TestReloadFailed,
		TestUnloading,
		TestUnloaded,
		TestUnloadFailed,
		// Test Run Events
		RunStarting,
		RunFinished,
		SuiteStarting,
		SuiteFinished,
		TestStarting,
		TestFinished,
		TestException,
		TestOutput
}

	#endregion

	/// <summary>
	/// Argument used for all test events
	/// </summary>
	public class TestEventArgs : EventArgs
	{
		#region Instance Variables

		// The action represented by the event
		private TestAction action;

		// The name of the test or other item
		private string name;
		
		// The name of the test we are running
		private ITest test;

		private TestName testName;

		// The results from our tests
		private TestResult testResult;
		
		// The exception causing a failure
		private Exception exception;

		// The test output
		private TestOutput testOutput;

		// The number of tests we are running
		private int testCount;

		#endregion

		#region Constructors

		// TestLoaded, TestReloaded
		public TestEventArgs( TestAction action, 
			string name, ITest test )
		{
			Debug.Assert( 
				action == TestAction.TestLoaded || action == TestAction.TestReloaded,
				"Invalid TestAction argument to TestEventArgs constructor" );

			this.action = action;
			this.name = name;
			this.test = test;
			if ( test != null )
				this.testCount = test.TestCount;
		}

		// ProjectLoading, ProjectLoaded, ProjectUnloading, ProjectUnloaded,
		// TestLoading, TestUnloading, TestUnloaded, TestReloading
		public TestEventArgs( TestAction action, string name )
		{
			Debug.Assert( 
				action == TestAction.ProjectLoading || action == TestAction.ProjectLoaded ||
				action == TestAction.ProjectUnloading || action == TestAction.ProjectUnloaded ||
				action == TestAction.TestLoading || action == TestAction.TestUnloading ||
				action == TestAction.TestUnloaded || action == TestAction.TestReloading,
				"Invalid TestAction argument to TestEventArgs constructor" );

			this.action = action;
			this.name = name;
		}

		public TestEventArgs( TestAction action, string name, int testCount )
		{
			Debug.Assert( action == TestAction.RunStarting,
				"Invalid TestAction argument to TestEventArgs constructor" );

			this.action = action;
			this.name = name;
			this.testCount = testCount;
		}

		// ProjectLoadFailed, ProjectUnloadFailed, TestLoadFailed, TestUnloadFailed, TestReloadFailed, TestException
		public TestEventArgs( TestAction action,
			string name, Exception exception )
		{
			Debug.Assert(
				action == TestAction.ProjectLoadFailed || action == TestAction.ProjectUnloadFailed ||
				action == TestAction.TestLoadFailed || action == TestAction.TestUnloadFailed ||
				action == TestAction.TestReloadFailed || action == TestAction.TestException, 
				"Invalid TestAction argument to TestEventArgs constructor" );

			this.action = action;
			this.name = name;
			this.exception = exception;
		}

		// TestStarting, SuiteStarting
		public TestEventArgs( TestAction action, TestName testName )
		{
			Debug.Assert( action == TestAction.TestStarting || action == TestAction.SuiteStarting,
				"Invalid TestAction argument to TestEventArgs constructor" );

			this.action = action;
			this.testName = testName;
		}

		// TestFinished, SuiteFinished, RunFinished
		public TestEventArgs( TestAction action, TestResult testResult )
		{
			Debug.Assert( action == TestAction.TestFinished || action == TestAction.SuiteFinished ||
				action == TestAction.RunFinished,
				"Invalid TestAction argument to TestEventArgs constructor" );

			this.action = action;
			this.testResult = testResult;
		}

		// RunFinished
		public TestEventArgs( TestAction action, Exception exception )
		{
			Debug.Assert( action == TestAction.RunFinished,
				"Invalid TestAction argument to TestEventArgs constructor" );

			this.action = action;
			this.exception = exception;
		}

		// TestOutput
		public TestEventArgs( TestAction action, TestOutput testOutput )
		{
			Debug.Assert( action == TestAction.TestOutput,
				"Invalid TestAction argument to TestEventArgs constructor" );

			this.action = action;
			this.testOutput = testOutput;
		}
		#endregion

		#region Properties

		public TestAction Action
		{
			get { return action; }
		}

		public string Name
		{
			get { return name; }
		}

		public ITest Test
		{
			get { return test; }
		}

		public TestName TestName
		{
			get { return testName; }
		}

		public int TestCount 
		{
			get	{ return testCount;	}
		}

		public TestResult Result
		{
			get { return testResult; }
		}

		public Exception Exception
		{
			get { return exception; }
		}

		public TestOutput TestOutput
		{
			get { return testOutput; }
		}

		#endregion
	}
}
