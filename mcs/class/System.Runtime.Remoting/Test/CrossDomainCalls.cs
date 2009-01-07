//
// MonoTests.Remoting.CrossDomainCalls.cs
//
// Author: Lluis Sanchez Gual (lluis@ximian.com)
//
// 2003 (C) Copyright, Novell, Inc.
//

using System;
using System.Threading;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using NUnit.Framework;

namespace MonoTests.Remoting
{
	class CrossDomainServer: MarshalByRefObject
	{
		public RemoteObject CreateRemoteInstance ()
		{
			return new RemoteObject();
		}

		public AbstractRemoteObject CreateRemoteAbstract ()
		{
			return new RemoteObject();
		}

		public IRemoteObject CreateRemoteInterface ()
		{
			return new RemoteObject();
		}	
		
		public int GetDomId ()
		{
			return Thread.GetDomainID();
		}
	}
	
	[TestFixture]
	public class CrossDomainSyncCallTest : SyncCallTest
	{
		CrossDomainServer server;
		
		protected override int CreateServer ()
		{
			AppDomain domain = BaseCallTest.CreateDomain ("testdomain");
			server = (CrossDomainServer) domain.CreateInstanceAndUnwrap(GetType().Assembly.FullName,"MonoTests.Remoting.CrossDomainServer");
			return server.GetDomId ();
		}
		
		protected override RemoteObject CreateRemoteInstance ()
		{
			return server.CreateRemoteInstance ();
		}

		protected override AbstractRemoteObject CreateRemoteAbstract ()
		{
			return server.CreateRemoteAbstract ();
		}

		protected override IRemoteObject CreateRemoteInterface ()
		{
			return server.CreateRemoteInterface ();
		}	
	}

	[TestFixture]
	public class CrossDomainAsyncCallTest : AsyncCallTest
	{
		CrossDomainServer server;
		
		protected override int CreateServer ()
		{
			AppDomain domain = BaseCallTest.CreateDomain ("testdomain");
			server = (CrossDomainServer) domain.CreateInstanceAndUnwrap(GetType().Assembly.FullName,"MonoTests.Remoting.CrossDomainServer");
			return server.GetDomId ();
		}
		
		protected override RemoteObject CreateRemoteInstance ()
		{
			return server.CreateRemoteInstance ();
		}

		protected override AbstractRemoteObject CreateRemoteAbstract ()
		{
			return server.CreateRemoteAbstract ();
		}

		protected override IRemoteObject CreateRemoteInterface ()
		{
			return server.CreateRemoteInterface ();
		}	
	}

	[TestFixture]
	public class CrossDomainReflectionCallTest : ReflectionCallTest
	{
		CrossDomainServer server;
		
		protected override int CreateServer ()
		{
			AppDomain domain = BaseCallTest.CreateDomain ("testdomain");
			server = (CrossDomainServer) domain.CreateInstanceAndUnwrap(GetType().Assembly.FullName,"MonoTests.Remoting.CrossDomainServer");
			return server.GetDomId ();
		}
		
		protected override RemoteObject CreateRemoteInstance ()
		{
			return server.CreateRemoteInstance ();
		}

		protected override AbstractRemoteObject CreateRemoteAbstract ()
		{
			return server.CreateRemoteAbstract ();
		}

		protected override IRemoteObject CreateRemoteInterface ()
		{
			return server.CreateRemoteInterface ();
		}	
	}

	[TestFixture]
	public class CrossDomainDelegateCallTest : DelegateCallTest
	{
		CrossDomainServer server;
		
		protected override int CreateServer ()
		{
			AppDomain domain = BaseCallTest.CreateDomain ("testdomain");
			server = (CrossDomainServer) domain.CreateInstanceAndUnwrap(GetType().Assembly.FullName,"MonoTests.Remoting.CrossDomainServer");
			return server.GetDomId ();
		}
		
		protected override RemoteObject CreateRemoteInstance ()
		{
			return server.CreateRemoteInstance ();
		}

		protected override AbstractRemoteObject CreateRemoteAbstract ()
		{
			return server.CreateRemoteAbstract ();
		}

		protected override IRemoteObject CreateRemoteInterface ()
		{
			return server.CreateRemoteInterface ();
		}	
	}
}

