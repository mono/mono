// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System;
using System.IO;
using System.Reflection;

namespace NUnit.Core
{
	/// <summary>
	/// SetUpFixture extends TestSuite and supports
	/// Setup and TearDown methods.
	/// </summary>
	public class SetUpFixture : TestSuite
	{
		#region Constructor
		public SetUpFixture( Type type ) : base( type )
		{
            this.TestName.Name = type.Namespace;
            if (this.TestName.Name == null)
                this.TestName.Name = "[default namespace]";
            int index = TestName.Name.LastIndexOf('.');
            if (index > 0)
                this.TestName.Name = this.TestName.Name.Substring(index + 1);
            
			this.fixtureSetUp = NUnitFramework.GetSetUpMethod( type );
			this.fixtureTearDown = NUnitFramework.GetTearDownMethod( type );
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
