// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************
using System;

namespace NUnit.Core
{
	/// <summary>
	/// The ResultState enum indicates the result of running a test
	/// </summary>
	public enum ResultState
	{
        /// <summary>
        /// The test succeeded
        /// </summary>
		Success,

        /// <summary>
        /// The test failed
        /// </summary>
		Failure,

        /// <summary>
        /// The test encountered an unexpected exception
        /// </summary>
		Error
	}

    /// <summary>
    /// The FailureSite enum indicates the stage of a test
    /// in which an error or failure occured.
    /// </summary>
    public enum FailureSite
    {
		/// <summary>
		/// The location of the failure is not known
		/// </summary>
		Unknown,

        /// <summary>
        /// Failure in the test itself
        /// </summary>
        Test,

        /// <summary>
        /// Failure in the SetUp method
        /// </summary>
        SetUp,

        /// <summary>
        /// Failure in the TearDown method
        /// </summary>
        TearDown,

        /// <summary>
        /// Failure of a parent test
        /// </summary>
        Parent,

        /// <summary>
        /// Failure of a child test
        /// </summary>
        Child
    }

}
