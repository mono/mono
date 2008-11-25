// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System;
using System.Collections;

namespace NUnit.Core.Filters
{
	/// <summary>
	/// Summary description for NameFilter.
	/// </summary>
	/// 
	[Serializable]
	public class NameFilter : TestFilter
	{
		private ArrayList testNames = new ArrayList();

		/// <summary>
		/// Construct an empty NameFilter
		/// </summary>
		public NameFilter() { }

		/// <summary>
		/// Construct a NameFilter for a single TestName
		/// </summary>
		/// <param name="testName"></param>
		public NameFilter( TestName testName )
		{
			testNames.Add( testName );
		}

		/// <summary>
		/// Add a TestName to a NameFilter
		/// </summary>
		/// <param name="testName"></param>
		public void Add( TestName testName )
		{
			testNames.Add( testName );
		}

		/// <summary>
		/// Check if a test matches the filter
		/// </summary>
		/// <param name="test">The test to match</param>
		/// <returns>True if it matches, false if not</returns>
		public override bool Match( ITest test )
		{
			foreach( TestName testName in testNames )
				if ( test.TestName == testName )
					return true;

			return false;
		}
	}
}
