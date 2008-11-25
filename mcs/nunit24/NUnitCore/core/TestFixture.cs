// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System;
using System.IO;

namespace NUnit.Core
{
	/// <summary>
	/// TestFixture is a surrogate for a user test fixture class,
	/// containing one or more tests.
	/// </summary>
	public class TestFixture : TestSuite
	{
		#region Constructors
		public TestFixture( Type fixtureType )
			: base( fixtureType ) { }
		#endregion

		#region Properties
		public override string TestType
		{
			get	{ return "Test Fixture"; }
		}
		#endregion

		#region TestSuite Overrides
        public override TestResult Run(EventListener listener, ITestFilter filter)
        {
            using ( new DirectorySwapper( Path.GetDirectoryName( TestFixtureBuilder.GetAssemblyPath( FixtureType ) ) ) )
            {
                return base.Run(listener, filter);
            }
        }
		#endregion
	}
}
