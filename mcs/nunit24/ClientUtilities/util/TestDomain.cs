// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System;

namespace NUnit.Util
{
	using System.Diagnostics;
	using System.Security.Policy;
	using System.Reflection;
	using System.Collections;
	using System.Configuration;
	using System.IO;

	using NUnit.Core;

	public class TestDomain : ProxyTestRunner, TestRunner
	{
		#region Instance Variables

		/// <summary>
		/// The appdomain used  to load tests
		/// </summary>
		private AppDomain domain; 

		#endregion

		#region Constructors
		public TestDomain() : base( 0 ) { }

		public TestDomain( int runnerID ) : base( runnerID ) { }
		#endregion

		#region Properties
		public AppDomain AppDomain
		{
			get { return domain; }
		}
		#endregion

		#region Loading and Unloading Tests
		public override bool Load( TestPackage package )
		{
			Unload();

			try
			{
				if ( this.domain == null )
					this.domain = Services.DomainManager.CreateDomain( package );
            
				if ( this.TestRunner == null )
					this.TestRunner = MakeRemoteTestRunner( domain );

				return TestRunner.Load( package );
			}
			catch
			{
				Unload();
				throw;
			}
		}

		public override void Unload()
		{
			this.TestRunner = null;

			if(domain != null) 
			{
				Services.DomainManager.Unload(domain);
				domain = null;
			}
		}
		#endregion

		#region MakeRemoteTestRunner Helper
		private TestRunner MakeRemoteTestRunner( AppDomain runnerDomain )
		{
			Type runnerType = typeof( RemoteTestRunner );
			object obj = runnerDomain.CreateInstanceAndUnwrap(
				runnerType.Assembly.FullName, 
				runnerType.FullName,
				false, BindingFlags.Default,null,new object[] { this.ID },null,null,null);
			
			RemoteTestRunner runner = (RemoteTestRunner) obj;

			return runner;
		}
		#endregion
	}
}
