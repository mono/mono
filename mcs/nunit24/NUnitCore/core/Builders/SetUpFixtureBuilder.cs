// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System;

namespace NUnit.Core.Builders
{
	/// <summary>
	/// SetUpFixtureBuilder knows how to build a SetUpFixture.
	/// </summary>
	public class SetUpFixtureBuilder : Extensibility.ISuiteBuilder
	{	
		public SetUpFixtureBuilder()
		{
			//
			// TODO: Add constructor logic here	//
		}

		#region ISuiteBuilder Members

		public Test BuildFrom(Type type)
		{
			return new SetUpFixture( type );
		}

		public bool CanBuildFrom(Type type)
		{
			return Reflect.HasAttribute( type, NUnitFramework.SetUpFixtureAttribute, false );
		}
		#endregion
	}
}
