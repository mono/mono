// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************
using System;

namespace NUnit.Core
{
	/// <summary>
	/// Interface to be implemented by filters applied to tests.
	/// The filter applies when running the test, after it has been
	/// loaded, since this is the only time an ITest exists.
	/// </summary>
	public interface ITestFilter
	{
		/// <summary>
		/// Indicates whether this is the EmptyFilter
		/// </summary>
		bool IsEmpty { get; }

		/// <summary>
		/// Determine if a particular test passes the filter criteria. Pass
		/// may examine the parents and/or descendants of a test, depending
		/// on the semantics of the particular filter
		/// </summary>
		/// <param name="test">The test to which the filter is applied</param>
		/// <returns>True if the test passes the filter, otherwise false</returns>
		bool Pass( ITest test );

		/// <summary>
		/// Determine whether the test itself matches the filter criteria,
		/// without examining either parents or descendants.
		/// </summary>
		/// <param name="test">The test to which the filter is applied</param>
		/// <returns>True if the filter matches the any parent of the test</returns>
		bool Match( ITest test );
	}
}
