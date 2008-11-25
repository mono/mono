// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System;

namespace NUnit.Core.Builders
{
	/// <summary>
	/// Built-in SuiteBuilder for LegacySuite
	/// </summary>
	public class LegacySuiteBuilder : Extensibility.ISuiteBuilder
	{
		public bool CanBuildFrom( Type type )
		{
			return LegacySuite.GetSuiteProperty( type ) != null;
		}

		public Test BuildFrom( Type type )
		{
			return new LegacySuite( type );
		}
	}
}
