// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************
using System;

namespace NUnit.Core
{
	/// <summary>
	/// The RunState enum indicates whether a test
    /// has been or can be executed.
	/// </summary>
	public enum RunState
	{
        /// <summary>
        /// The test is not runnable
        /// </summary>
		NotRunnable,

        /// <summary>
        /// The test is runnable
        /// </summary>
		Runnable,

        /// <summary>
        /// The test can only be run explicitly
        /// </summary>
		Explicit,

        /// <summary>
        /// The test has been skipped
        /// </summary>
		Skipped,

        /// <summary>
        /// The test has been ignored
        /// </summary>
		Ignored,

        /// <summary>
        /// The test has been executed
        /// </summary>
		Executed
	}
}
