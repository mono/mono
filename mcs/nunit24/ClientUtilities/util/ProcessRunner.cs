// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Services;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using NUnit.Core;

namespace NUnit.Util
{
	/// <summary>
	/// Summary description for ProcessRunner.
	/// </summary>
	public class ProcessRunner : ProxyTestRunner, IDisposable
	{
		private TestAgent agent;

		#region Constructors
		public ProcessRunner() : base( 0 ) { }

		public ProcessRunner( int runnerID ) : base( runnerID ) { }
		#endregion

		public override bool Load(TestPackage package)
		{
			if ( this.agent == null )
				this.agent = Services.TestAgency.GetAgent( AgentType.ProcessAgent, 5000 );		
	
			if ( this.TestRunner == null )
				this.TestRunner = agent.CreateRunner(this.runnerID);

			return base.Load (package);
		}

		#region IDisposable Members
		public void Dispose()
		{
			if ( TestRunner != null )
				this.TestRunner.Unload();

			if ( this.agent != null )
				Services.TestAgency.ReleaseAgent(this.agent);

			this.TestRunner = null;
			this.agent = null;
		}
		#endregion
	}
}
