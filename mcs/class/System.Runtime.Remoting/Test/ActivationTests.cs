//
// MonoTests.Remoting.TcpCalls.cs
//
// Author: Lluis Sanchez Gual (lluis@ximian.com)
//
// 2003 (C) Copyright, Ximian, Inc.
//

using System;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels.Http;
using NUnit.Framework;

namespace MonoTests.Remoting
{
	[TestFixture]
	public class ActivationTests
	{
		ActivationServer server;
		TcpChannel tcp;
		HttpClientChannel http;
			
		[TestFixtureSetUp]
		public void Run()
		{
			try
			{
				tcp =  new TcpChannel (0);
				
				Hashtable options = new Hashtable ();
				options ["timeout"] = 10000; // 10s
				http = new HttpClientChannel (options, null);
			
				ChannelServices.RegisterChannel (tcp);
				ChannelServices.RegisterChannel (http);
			
				AppDomain domain = BaseCallTest.CreateDomain ("testdomain_activation");
				server = (ActivationServer) domain.CreateInstanceAndUnwrap(GetType().Assembly.FullName,"MonoTests.Remoting.ActivationServer");
				
				RemotingConfiguration.RegisterActivatedClientType (typeof(CaObject1), "tcp://localhost:9433");
				RemotingConfiguration.RegisterActivatedClientType (typeof(CaObject2), "http://localhost:9434");
				RemotingConfiguration.RegisterWellKnownClientType (typeof(WkObjectSinglecall1), "tcp://localhost:9433/wkoSingleCall1");
				RemotingConfiguration.RegisterWellKnownClientType (typeof(WkObjectSingleton1), "tcp://localhost:9433/wkoSingleton1");
				RemotingConfiguration.RegisterWellKnownClientType (typeof(WkObjectSinglecall2), "http://localhost:9434/wkoSingleCall2");
				RemotingConfiguration.RegisterWellKnownClientType (typeof(WkObjectSingleton2), "http://localhost:9434/wkoSingleton2");
			}
			catch (Exception ex)
			{
				Console.WriteLine (ex);
			}
		}
		
		[Test]
		public void TestCreateTcpCao ()
		{
			CaObject1 ca = new CaObject1 ();
			CaObject1 ca2 = new CaObject1 ();
			Assert.IsTrue (BaseObject.CreationCount == 0, "Objects created locally");
			RunTestCreateCao (ca, ca2);
		}
		
		[Test]
		public void TestCreateHttpCao ()
		{
			CaObject2 ca = new CaObject2 ();
			CaObject2 ca2 = new CaObject2 ();
			Assert.IsTrue (BaseObject.CreationCount == 0, "Objects created locally");
			RunTestCreateCao (ca, ca2);
		}
		
		public void RunTestCreateCao (BaseObject ca, BaseObject ca2)
		{
			Assert.AreEqual (0, ca.counter, "#1");
			
			ca.counter++;
			Assert.AreEqual (1, ca.counter, "#2");
			
			Assert.AreEqual (0, ca2.counter, "#3");
			
			ca2.counter++;
			Assert.AreEqual (1, ca2.counter, "#4");
			
			Assert.AreEqual (1, ca.counter, "#5");
		}

		[Test]
		public void TestCreateTcpWkoSingleCall ()
		{
			WkObjectSinglecall1 ca = new WkObjectSinglecall1 ();
			WkObjectSinglecall1 ca2 = new WkObjectSinglecall1 ();
			Assert.IsTrue (BaseObject.CreationCount == 0, "Objects created locally");
			RunTestCreateWkoSingleCall (ca, ca2);
		}
		
		[Test]
		public void TestCreateTcpWkoSingleton ()
		{
			WkObjectSingleton1 ca = new WkObjectSingleton1 ();
			WkObjectSingleton1 ca2 = new WkObjectSingleton1 ();
			Assert.IsTrue (BaseObject.CreationCount == 0, "Objects created locally");
			RunTestCreateWkoSingleton (ca, ca2);
		}

		[Test]
		public void TestCreateHttpWkoSingleCall ()
		{
			WkObjectSinglecall2 ca = new WkObjectSinglecall2 ();
			WkObjectSinglecall2 ca2 = new WkObjectSinglecall2 ();
			Assert.IsTrue (BaseObject.CreationCount == 0, "Objects created locally");
			RunTestCreateWkoSingleCall (ca, ca2);
		}
		
		[Test]
		public void TestCreateHttpWkoSingleton ()
		{
			WkObjectSingleton2 ca = new WkObjectSingleton2 ();
			WkObjectSingleton2 ca2 = new WkObjectSingleton2 ();
			Assert.IsTrue (BaseObject.CreationCount == 0, "Objects created locally");
			RunTestCreateWkoSingleton (ca, ca2);
		}
		
		public void RunTestCreateWkoSingleCall (BaseObject ca, BaseObject ca2)
		{
			Assert.AreEqual (0, ca.counter, "#1");
			ca.counter++;
			Assert.AreEqual (0, ca.counter, "#2");

			Assert.AreEqual (0, ca2.counter, "#3");
			ca2.counter++;
			Assert.AreEqual (0, ca2.counter, "#4");
		}

		public void RunTestCreateWkoSingleton (BaseObject ca, BaseObject ca2)
		{
			Assert.AreEqual (0, ca.counter, "#1");
			ca.counter++;
			Assert.AreEqual (1, ca.counter, "#2");
			ca.counter++;
			Assert.AreEqual (2, ca2.counter, "#3");
			ca2.counter++;
			Assert.AreEqual (3, ca2.counter, "#4");
		}

		[TestFixtureTearDown]
		public void End ()
		{
			ChannelServices.UnregisterChannel (tcp);
			ChannelServices.UnregisterChannel (http);
			if (server != null)
				server.Stop ();
		}
	}
	
	public class ActivationServer: MarshalByRefObject
	{
		TcpChannel tcp;
		HttpChannel http;
		
		public ActivationServer ()
		{
			tcp =  new TcpChannel (9433);
			http =  new HttpChannel (9434);
			
			ChannelServices.RegisterChannel (tcp);
			ChannelServices.RegisterChannel (http);
			
			RemotingConfiguration.RegisterActivatedServiceType (typeof(CaObject1));
			RemotingConfiguration.RegisterActivatedServiceType (typeof(CaObject2));
			RemotingConfiguration.RegisterWellKnownServiceType (typeof(WkObjectSinglecall1), "wkoSingleCall1", WellKnownObjectMode.SingleCall);
			RemotingConfiguration.RegisterWellKnownServiceType (typeof(WkObjectSingleton1), "wkoSingleton1", WellKnownObjectMode.Singleton);
			RemotingConfiguration.RegisterWellKnownServiceType (typeof(WkObjectSinglecall2), "wkoSingleCall2", WellKnownObjectMode.SingleCall);
			RemotingConfiguration.RegisterWellKnownServiceType (typeof(WkObjectSingleton2), "wkoSingleton2", WellKnownObjectMode.Singleton);
		}
		
		public void Stop ()
		{
			ChannelServices.UnregisterChannel (tcp);
			ChannelServices.UnregisterChannel (http);
		}
	}
	
	public class BaseObject: MarshalByRefObject
	{
		public int counter;
		public static int CreationCount;
		
		public BaseObject ()
		{
			CreationCount++;
		}
	}
	
	public class CaObject1: BaseObject
	{
	}
	
	public class CaObject2: BaseObject
	{
	}
	
	public class WkObjectSinglecall1: BaseObject
	{
	}
	
	public class WkObjectSingleton1: BaseObject
	{
	}
	
	public class WkObjectSinglecall2: BaseObject
	{
	}
	
	public class WkObjectSingleton2: BaseObject
	{
	}
}
