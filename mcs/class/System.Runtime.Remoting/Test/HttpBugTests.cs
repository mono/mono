
using System;
using System.Xml;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using NUnit.Framework;

namespace MonoTests.Remoting.Http
{
	//Test for Bug 324362 - SoapFormatter cannot deserialize the same MBR twice
	[TestFixture]
	public class Bug324362
	{
		[Test]
//		[Category ("NotWorking")] // the assertion fails, and if it's removed, there's an exception
		[Ignore ("This test somehow keeps http channel registered and then blocks any further http tests working. This also happens under .NET, so this test itself is wrong with nunit 2.4.8.")]
		public void Test ()
		{
			new HttpChannel (8086);
			RemotingServices.Marshal (new Service (), "test");

			Service remObj = (Service) RemotingServices.Connect (
				typeof (Service), "http://localhost:8086/test");

			ArrayList list;
			remObj.Test (out list);
			// it's of type 'ObjRef' instead of 'Service':
			Assert.IsTrue (list [0] is Service);

			Service [] array;
			remObj.Test (out array);
		}
	
		public class Service : MarshalByRefObject
		{
			public Service Test (out Service[] a)
			{
				Service obj = new Service ();
				a = new Service [] { obj };
				return obj;
				// return null or return otherObj works
			}
		
			public Service Test (out ArrayList a)
			{
				a = new ArrayList ();
				Service obj = new Service ();
				a.Add (obj);
				return obj; 
				// return null or return otherObj works
			}
		}
	}
	
	//Bug 321420 - SoapReader fails to deserialize some method calls
	[TestFixture]
	public class Bug321420 : MarshalByRefObject
	{
		HttpChannel channel;
		
		public void Method (string p1, string p2)
		{
		}
		
		[Test]
		public void Main ()
		{
			channel = new HttpChannel (3344);
			ChannelServices.RegisterChannel (channel);
			RemotingConfiguration.RegisterWellKnownServiceType
				(typeof (Bug321420),"Server.soap", WellKnownObjectMode.Singleton);
			
			Bug321420 s = (Bug321420) Activator.GetObject (typeof
				(Bug321420), "http://localhost:3344/Server.soap");
			
			// this works: s.Method ("a", "b");
			s.Method ("a", "a");
		}
		
		[TestFixtureTearDown]
		public void Stop ()
		{
			if (channel != null)
				ChannelServices.UnregisterChannel (channel);
		}
	}
	
	//Bug 315570 - Remoting over HTTP fails when returning a null reference.
	[TestFixture]	
	public class Bug315570
	{
		Server server;
		
		[Test]
		[Ignore ("This test somehow keeps http channel registered and then blocks any further http tests working. This also happens under .NET, so this test itself is wrong with nunit 2.4.8.")]
		public void Main ()
		{
			Foo foo = (Foo) Activator.GetObject (typeof (Foo),
				"http://localhost:4321/Test");

			Bar bar = foo.Login ();
			if (bar != null)
				bar.Foobar ();
		}
		
		[TestFixtureSetUp]
		public void Start ()
		{
			AppDomain domain = BaseCallTest.CreateDomain ("testdomain");
			server = (Server) domain.CreateInstanceAndUnwrap
				(typeof (Server).Assembly.FullName, typeof (Server).FullName);
			server.Start ();
		}

		[TestFixtureTearDown]
		public void Stop ()
		{
			server.Stop ();
		}
		
		public class Foo: MarshalByRefObject
		{
			public Bar Login ()
			{
				return null;
			}
		}
		
		public class Bar: MarshalByRefObject
		{
			public void Foobar ()
			{
			//	Console.WriteLine("Bar::foo()");
			}
		}
		
		public class Server : MarshalByRefObject
		{
			HttpChannel c;
			
			public void Start ()
			{
				c = new HttpChannel (4321);
				ChannelServices.RegisterChannel (c);
				
				Type t = typeof(Foo);
				RemotingConfiguration.RegisterWellKnownServiceType (t, "Test",
					WellKnownObjectMode.SingleCall);
			}
			
			public void Stop ()
			{
				c.StopListening (null);
				ChannelServices.UnregisterChannel (c);
			}
		}
	}
	
	//Bug 315170 - exception thrown in remoting if interface parameter names differ from the impelmentation method parameter names
	[TestFixture]
	public class Bug315170
	{
		Server server;
		HttpChannel channel;
		
		[Test]
		public void Main ()
		{
			channel = new HttpChannel (0);
			ChannelServices.RegisterChannel (channel);
			MarshalByRefObject obj = (MarshalByRefObject) RemotingServices.Connect (
				typeof (IFactorial),
				"http://localhost:60000/MyEndPoint");
			IFactorial cal = (IFactorial) obj;
			Assert.AreEqual (cal.CalculateFactorial (4), 24);
		}
		
		[TestFixtureSetUp]
		public void Start ()
		{
			AppDomain domain = BaseCallTest.CreateDomain ("testdomain");
			server = (Server) domain.CreateInstanceAndUnwrap
				(typeof (Server).Assembly.FullName, typeof (Server).FullName);
			server.Start ();
		}

		[TestFixtureTearDown]
		public void Stop ()
		{
			server.Stop ();
			if (channel != null)
				ChannelServices.UnregisterChannel (channel);
		}
		
		public interface IFactorial
		{
			// With this line everything works
			//ulong CalculateFactorial(uint a);
			// With this line it doesn't
			ulong CalculateFactorial(uint b);
		}

		
		public class Server : MarshalByRefObject
		{
			HttpChannel c;
			
			public void Start ()
			{
				c = new HttpChannel (60000);
				ChannelServices.RegisterChannel (c);
				
				Type t = typeof(Calculator);
				RemotingConfiguration.RegisterWellKnownServiceType (t, "MyEndPoint",
					WellKnownObjectMode.Singleton);
			}
			
			public void Stop ()
			{
				c.StopListening (null);
				ChannelServices.UnregisterChannel (c);
			}
		}
		
		public class Calculator : MarshalByRefObject, IFactorial
		{
			public ulong CalculateFactorial (uint a)
			{
				ulong res = 1;
				for (uint i=1 ; i<=a; i++)
					res = res * i;
				return res;
			}
		}
	}
}


