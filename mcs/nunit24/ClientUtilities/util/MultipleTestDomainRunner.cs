// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;
using System.Collections;
using NUnit.Core;

namespace NUnit.Util
{
	/// <summary>
	/// Summary description for MultipleTestDomainRunner.
	/// </summary>
	public class MultipleTestDomainRunner : AggregatingTestRunner
	{
		#region Constructors
		public MultipleTestDomainRunner() : base( 0 ) { }

		public MultipleTestDomainRunner( int runnerID ) : base( runnerID ) { }
		#endregion

		#region Load Method Overrides
		public override bool Load(TestPackage package)
		{
			this.projectName = package.FullName;
			this.testName.FullName = this.testName.Name = projectName;
			runners = new ArrayList();

			int nfound = 0;
			int index = 0;

			string targetAssemblyName = null;
			if( package.TestName != null && package.Assemblies.Contains( package.TestName ) )
			{
				targetAssemblyName = package.TestName;
				package.TestName = null;
			}
			
			foreach( string assembly in package.Assemblies )
			{
				if ( targetAssemblyName == null || targetAssemblyName == assembly )
				{
					TestDomain runner = new TestDomain( this.runnerID * 100 + index + 1 );

					TestPackage p = new TestPackage( assembly );
					p.AutoBinPath = package.AutoBinPath;
					p.ConfigurationFile = package.ConfigurationFile;
					p.BasePath = package.BasePath;
					p.PrivateBinPath = package.PrivateBinPath;
					p.TestName = package.TestName;
					foreach( object key in package.Settings.Keys )
						p.Settings[key] = package.Settings[key];

					if ( package.TestName == null )
					{
						runners.Add( runner );
						if ( runner.Load( p ) )
							nfound++;
					}
					else if ( runner.Load( p ) )
					{
						runners.Add( runner );
						nfound++;
					}
				}
			}

			if ( package.TestName == null && targetAssemblyName == null )
				return nfound == package.Assemblies.Count;
			else
				return nfound > 0;
		}

		private void CreateRunners( int count )
		{
			runners = new ArrayList();
			for( int index = 0; index < count; index++ )
			{
				TestDomain runner = new TestDomain( this.runnerID * 100 + index + 1 );
				runners.Add( runner );
			}
		}
		#endregion
	}
}
