// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System;
using NUnit.Core;

namespace NUnit.Util
{
	/// <summary>
	/// ITestEvents interface defines events related to loading
	/// and unloading of test projects and loading, unloading and
	/// running tests.
	/// </summary>
	public interface ITestEvents
	{
		// Events related to the loading and unloading
		// of projects - including wrapper projects
		// created in order to load assemblies. This
		// occurs separately from the loading of tests
		// for the assemblies in the project.
		event TestEventHandler ProjectLoading;
		event TestEventHandler ProjectLoaded;
		event TestEventHandler ProjectLoadFailed;
		event TestEventHandler ProjectUnloading;
		event TestEventHandler ProjectUnloaded;
		event TestEventHandler ProjectUnloadFailed;

		// Events related to loading and unloading tests.
		event TestEventHandler TestLoading;
		event TestEventHandler TestLoaded;
		event TestEventHandler TestLoadFailed;
		
		event TestEventHandler TestReloading;
		event TestEventHandler TestReloaded;
		event TestEventHandler TestReloadFailed;
		
		event TestEventHandler TestUnloading;
		event TestEventHandler TestUnloaded;
		event TestEventHandler TestUnloadFailed;
	
		// Events related to a running a set of tests
		event TestEventHandler RunStarting;	
		event TestEventHandler RunFinished;

		// Events that arise while a test is running
		// These are translated from calls to the runner on the
		// EventListener interface.
		event TestEventHandler SuiteStarting;
		event TestEventHandler SuiteFinished;
		event TestEventHandler TestStarting;
		event TestEventHandler TestFinished;

		/// <summary>
		/// An unhandled exception was thrown during a test run,
		/// and it cannot be associated with a particular test failure.
		/// </summary>
		event TestEventHandler TestException;

		/// <summary>
		/// Console Out/Error
		/// </summary>
		event TestEventHandler TestOutput;
	}
}
