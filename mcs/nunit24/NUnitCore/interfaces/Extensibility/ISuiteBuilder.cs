// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System;

namespace NUnit.Core.Extensibility
{
	/// <summary>
	/// The ISuiteBuilder interface is exposed by a class that knows how to
	/// build a suite from one or more Types. 
	/// </summary>
	public interface ISuiteBuilder
	{
		/// <summary>
		/// Examine the type and determine if it is suitable for
		/// this builder to use in building a TestSuite.
        /// 
        /// Note that returning false will cause the type to be ignored 
        /// in loading the tests. If it is desired to load the suite
        /// but label it as non-runnable, ignored, etc., then this
        /// method must return true.
        /// </summary>
		/// <param name="type">The type of the fixture to be used</param>
		/// <returns>True if the type can be used to build a TestSuite</returns>
		bool CanBuildFrom( Type type );

		/// <summary>
		/// Build a TestSuite from type provided.
		/// </summary>
		/// <param name="type">The type of the fixture to be used</param>
		/// <returns>A TestSuite</returns>
		Test BuildFrom( Type type );
	}
}
