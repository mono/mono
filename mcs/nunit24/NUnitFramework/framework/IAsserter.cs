// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;

namespace NUnit.Framework
{
	/// <summary>
	/// NOTE: The use of asserters for extending NUnit has
	/// now been replaced by the use of constraints. This
	/// interface is marked obsolete.
	/// 
	/// The interface implemented by an asserter. Asserters
	/// encapsulate a condition test and generation of an
	/// AssertionException with a tailored message. They
	/// are used by the Assert class as helper objects.
	/// 
	/// User-defined asserters may be passed to the
	/// Assert.DoAssert method in order to implement
	/// extended asserts.
	/// </summary>
	[Obsolete("Use Constraints rather than Asserters for new work")]
	public interface IAsserter
	{
		/// <summary>
		/// Test the condition for the assertion.
		/// </summary>
		/// <returns>True if the test succeeds</returns>
		bool Test();

		/// <summary>
		/// Return the message giving the failure reason.
		/// The return value is unspecified if no failure
		/// has occured.
		/// </summary>
		string Message { get; }
	}
}
