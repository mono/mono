//
// MonoTests.Remoting.BaseCalls.cs
//
// Author: Lluis Sanchez Gual (lluis@ximian.com)
//
// 2003 (C) Copyright, Ximian, Inc.
//

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Collections;
using System.Globalization;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Lifetime;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Contexts;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace MonoTests.Remoting
{
	public abstract class BaseCallTest
	{
		IChannelSender chs;
		string[] remoteUris;
		CallsDomainServer server;
		int remoteDomId;

		[TestFixtureSetUp]
		public void Run()
		{
			remoteDomId = CreateServer ();
		}

		[TestFixtureTearDown]
		public void End ()
		{
			ShutdownServer ();
		}

		public static AppDomain CreateDomain (string friendlyName)
		{
			// return AppDomain.CreateDomain (friendlyName);
			return AppDomain.CreateDomain (friendlyName, null, Directory.GetCurrentDirectory (), ".", false);
		}

		protected virtual int CreateServer ()
		{
			ChannelManager cm = CreateChannelManager ();
			chs = cm.CreateClientChannel ();
			ChannelServices.RegisterChannel (chs);

			AppDomain domain = BaseCallTest.CreateDomain ("testdomain");
			server = (CallsDomainServer) domain.CreateInstanceAndUnwrap(GetType().Assembly.FullName,"MonoTests.Remoting.CallsDomainServer");
			remoteUris = server.Start (cm);
			return server.GetDomId ();
		}
		
		protected virtual void ShutdownServer ()
		{
			if (server != null) {
				server.Stop ();
				if (chs != null)
					ChannelServices.UnregisterChannel (chs);
			}
		}

		protected virtual RemoteObject CreateRemoteInstance ()
		{
			return (RemoteObject) Activator.GetObject (typeof(RemoteObject), remoteUris[0]);
		}

		protected virtual AbstractRemoteObject CreateRemoteAbstract ()
		{
			return (AbstractRemoteObject) Activator.GetObject (typeof(AbstractRemoteObject), remoteUris[1]);
		}

		protected virtual IRemoteObject CreateRemoteInterface ()
		{
			return (IRemoteObject) Activator.GetObject (typeof(IRemoteObject), remoteUris[2]);
		}

		public InstanceSurrogate InternalGetInstanceSurrogate ()
		{
			InstanceSurrogate s = GetInstanceSurrogate ();
			s.RemoteObject = CreateRemoteInstance ();
			return s;
		}
		
		public AbstractSurrogate InternalGetAbstractSurrogate ()
		{
			AbstractSurrogate s = GetAbstractSurrogate ();
			s.RemoteObject = CreateRemoteAbstract ();
			return s;
		}
		
		public InterfaceSurrogate InternalGetInterfaceSurrogate ()
		{
			InterfaceSurrogate s = GetInterfaceSurrogate ();
			s.RemoteObject = CreateRemoteInterface ();
			return s;
		}

		public abstract InstanceSurrogate GetInstanceSurrogate ();
		public abstract AbstractSurrogate GetAbstractSurrogate ();
		public abstract InterfaceSurrogate GetInterfaceSurrogate ();
		
		public virtual ChannelManager CreateChannelManager ()
		{
			return null;
		}

		//
		// The tests
		//

		[Test]
		public void TestInstanceSimple ()
		{
			RunTestSimple (InternalGetInstanceSurrogate());
		}

		[Test]
		public void TestAbstractSimple ()
		{
			RunTestSimple (InternalGetAbstractSurrogate());
		}

		[Test]
		public void TestInterfaceSimple ()
		{
			RunTestSimple (InternalGetInterfaceSurrogate());
		}

		[Test]
		public void TestInstancePrimitiveParams ()
		{
			RunTestPrimitiveParams (InternalGetInstanceSurrogate());
		}

		[Test]
		public void TestAbstractPrimitiveParams ()
		{
			RunTestPrimitiveParams (InternalGetAbstractSurrogate());
		}

		[Test]
		public void TestInterfacePrimitiveParams ()
		{
			RunTestPrimitiveParams (InternalGetInterfaceSurrogate());
		}

		[Test]
		public void TestInstancePrimitiveParamsInOut ()
		{
			RunTestPrimitiveParamsInOut (InternalGetInstanceSurrogate());
		}

		[Test]
		public void TestAbstractPrimitiveParamsInOut ()
		{
			RunTestPrimitiveParamsInOut (InternalGetAbstractSurrogate());
		}

		[Test]
		public void TestInterfacePrimitiveParamsInOut ()
		{
			RunTestPrimitiveParamsInOut (InternalGetInterfaceSurrogate());
		}

		[Test]
		public void TestInstanceComplexParams ()
		{
			RunTestComplexParams (InternalGetInstanceSurrogate());
		}

		[Test]
		public void TestAbstractComplexParams ()
		{
			RunTestComplexParams (InternalGetAbstractSurrogate());
		}

		[Test]
		public void TestInterfaceComplexParams ()
		{
			RunTestComplexParams (InternalGetInterfaceSurrogate());
		}

		[Test]
		[Category ("NotDotNet")]
		public void TestInstanceComplexParamsInOut ()
		{
			RunTestComplexParamsInOut (InternalGetInstanceSurrogate());
		}

		[Test]
		[Category ("NotDotNet")]
		public void TestAbstractComplexParamsInOut ()
		{
			RunTestComplexParamsInOut (InternalGetAbstractSurrogate());
		}

		[Test]
		[Category ("NotDotNet")]
		public void TestInterfaceComplexParamsInOut ()
		{
			RunTestComplexParamsInOut (InternalGetInterfaceSurrogate());
		}

		[Test]
		public void TestInstanceProcessContextData ()
		{
			RunTestProcessContextData (InternalGetInstanceSurrogate());
		}

		[Test]
		public void TestAbstractProcessContextData ()
		{
			RunTestProcessContextData (InternalGetAbstractSurrogate());
		}

		[Test]
		public void TestInterfaceProcessContextData ()
		{
			RunTestProcessContextData (InternalGetInterfaceSurrogate());
		}

		//
		// The tests runners
		//

		public void RunTestSimple (IRemoteObject testerSurrogate)
		{
			Assert.AreEqual (130772 + remoteDomId, testerSurrogate.Simple (), "ReturnValue");
		}

		public void RunTestPrimitiveParams (IRemoteObject testerSurrogate)
		{
			Assert.AreEqual ("11-22-L-SG@"+remoteDomId, testerSurrogate.PrimitiveParams (11, 22, 'L', "SG"), "ReturnValue");
		}

		public void RunTestPrimitiveParamsInOut (IRemoteObject testerSurrogate)
		{
			int a2, a1 = 9876543;
			float b2, b1 = 82437.83f;
			char c2, c1 = 's';
			string d2, d1 = "asdASDzxcZXC";

			string res = testerSurrogate.PrimitiveParamsInOut (ref a1, out a2, ref b1, out b2, 9821, ref c1, out c2, ref d1, out d2);

			Assert.AreEqual ("9876543-82437.83-s-asdASDzxcZXC@" + remoteDomId, res, "ReturnValue");

			Assert.AreEqual (12345678, a2, "a2");
			Assert.AreEqual (53455.345f, b2, "b2");
			Assert.AreEqual ('g', c2, "c2");
			Assert.AreEqual ("sfARREG$5345DGDfgY7656gDFG>><<dasdasd", d2, "d2");

			Assert.AreEqual (65748392, a1, "a1");
			Assert.AreEqual (98395.654f, b1, "b1");
			Assert.AreEqual ('l', c1, "c1");
			Assert.AreEqual ("aasbasbdyhasbduybo234243", d1, "d1");
		}

		public void RunTestComplexParams (IRemoteObject testerSurrogate)
		{
			ArrayList list = new ArrayList ();
			list.Add (new Complex (11,"first"));
			Complex c = new Complex (22,"second");

			Complex r = testerSurrogate.ComplexParams (list, c, "third");

			Assert.IsNotNull (r, "ReturnValue is null");
			Assert.IsNotNull (r.Child, "ReturnValue.Child is null");
			Assert.IsNotNull (r.Child.Child, "ReturnValue.Child.Child is null");
			
			Assert.AreEqual (33, r.Id, "ReturnValue.Id");
			Assert.AreEqual ("third@"+remoteDomId, r.Name, "ReturnValue.Name");
			Assert.AreEqual (22, r.Child.Id, "ReturnValue.Child.Id");
			Assert.AreEqual ("second", r.Child.Name, "ReturnValue.Child.Name");
			Assert.AreEqual (11, r.Child.Child.Id, "ReturnValue.Child.Child.Id");
			Assert.AreEqual ("first", r.Child.Child.Name, "ReturnValue.Child.Child.Name");
		}

		public void RunTestComplexParamsInOut (IRemoteObject testerSurrogate)
		{
			ArrayList list = new ArrayList ();
			list.Add (new Complex (11,"first"));
			list.Add (new Complex (22,"second"));
			
			byte[] bytes = new byte [100];
			for (byte n=0; n<100; n++) bytes[n] = n;
			StringBuilder sb = new StringBuilder ("hello from client");

			Complex c;
			Complex r = testerSurrogate.ComplexParamsInOut (ref list, out c, bytes, sb, "third");

			Assert.IsNotNull (r, "ReturnValue is null");
			Assert.IsNotNull (c, "c is null");
			Assert.IsNotNull (list, "list is null");
			Assert.IsTrue (list.Count == 3, "Invalid list count");
			Assert.IsNotNull (list[0], "list[0] is null");
			Assert.IsNotNull (list[1], "list[1] is null");
			Assert.IsNotNull (list[2], "list[2] is null");
			Assert.IsNotNull (bytes, "bytes is null");
			Assert.IsNotNull (sb, "sb is null");
			
			Assert.AreEqual (33, r.Id, "ReturnValue.Id");
			Assert.AreEqual ("third@"+remoteDomId, r.Name, "ReturnValue.Name");
			Assert.AreEqual (33, c.Id, "c.Id");
			Assert.AreEqual ("third@"+remoteDomId, c.Name, "c.Name");

			Assert.AreEqual (33, ((Complex)list[2]).Id, "list[2].Id");
			Assert.AreEqual ("third@"+remoteDomId, ((Complex)list[2]).Name, "list[2].Name");
			Assert.AreEqual (22, ((Complex)list[1]).Id, "list[1].Id");
			Assert.AreEqual ("second", ((Complex)list[1]).Name, "list[1].Name");
			Assert.AreEqual (11, ((Complex)list[0]).Id, "list[0].Id");
			Assert.AreEqual ("first", ((Complex)list[0]).Name, "list[0].Name");
			
			Assert.AreEqual ("hello from client", sb.ToString (), "sb");
			for (int n=0; n<100; n++) 
				Assert.AreEqual (n+1, bytes[n], "bytes["+n+"]");
		}
		
		public void RunTestProcessContextData (IRemoteObject testerSurrogate)
		{
			CallContext.FreeNamedDataSlot ("clientData");
			CallContext.FreeNamedDataSlot ("serverData");
			CallContext.FreeNamedDataSlot ("mustNotPass");
			
			// First step

			ContextData cdata = new ContextData ();
			cdata.data = "hi from client";
			cdata.id = 1123;
			cdata.testStep = 1;
			CallContext.SetData ("clientData", cdata);
			CallContext.SetData ("mustNotPass", "more data");
			
			testerSurrogate.ProcessContextData ();
			
			cdata = CallContext.GetData ("clientData") as ContextData;
			Assert.IsNotNull (cdata, "clientData is null");
			Assert.AreEqual ("hi from client", cdata.data, "clientData.data");
			Assert.AreEqual (1123, cdata.id, "clientData.id");
			
			cdata = CallContext.GetData ("serverData") as ContextData;
			Assert.IsNotNull (cdata, "serverData is null");
			Assert.AreEqual ("hi from server", cdata.data, "serverData.data");
			Assert.AreEqual (3211, cdata.id, "serverData.id");
			
			string mdata = CallContext.GetData ("mustNotPass") as string;
			Assert.IsNotNull (mdata, "mustNotPass is null");
			Assert.AreEqual ("more data", mdata, "mustNotPass");
			
			// Second step. Test that exceptions return the call context.
			
			CallContext.FreeNamedDataSlot ("clientData");
			CallContext.FreeNamedDataSlot ("serverData");
			
			cdata = new ContextData ();
			cdata.data = "hi from client";
			cdata.id = 1123;
			cdata.testStep = 2;
			CallContext.SetData ("clientData", cdata);
			
			try {
				testerSurrogate.ProcessContextData ();
				Assert.Fail ("Exception not thrown");
			} catch (Exception ex) {
				if (ex.InnerException != null)
					ex = ex.InnerException;
				if (ex.Message != "exception from server")
					throw;
			}
			
			cdata = CallContext.GetData ("clientData") as ContextData;
			Assert.IsNotNull (cdata, "clientData is null (2)");
			Assert.AreEqual ("hi from client", cdata.data, "clientData.data (2)");
			Assert.AreEqual (1123, cdata.id, "clientData.id (2)");
			
			mdata = CallContext.GetData ("mustNotPass") as string;
			Assert.IsNotNull (mdata, "mustNotPass is null");
			Assert.AreEqual ("more data", mdata, "mustNotPass");
		}
	}

	//
	// The server running in the remote domain
	//

	class CallsDomainServer: MarshalByRefObject
	{
		IChannelReceiver ch;

		public string[] Start(ChannelManager cm)
		{
			try
			{
				ch = cm.CreateServerChannel ();
				ChannelServices.RegisterChannel ((IChannel)ch);
				RemotingConfiguration.RegisterWellKnownServiceType (typeof (RemoteObject), "test1", WellKnownObjectMode.SingleCall);
				RemotingConfiguration.RegisterWellKnownServiceType (typeof (RemoteObject), "test2", WellKnownObjectMode.SingleCall);
				RemotingConfiguration.RegisterWellKnownServiceType (typeof (RemoteObject), "test3", WellKnownObjectMode.SingleCall);
				string[] uris = new string[3];
				uris[0] = ch.GetUrlsForUri ("test1")[0];
				uris[1] = ch.GetUrlsForUri ("test2")[0];
				uris[2] = ch.GetUrlsForUri ("test3")[0];
				return uris;
			}
			catch (Exception ex)
			{
				Console.WriteLine (ex.ToString());
				throw;
			}
		}

		public void Stop ()
		{
			if (ch != null)
				ChannelServices.UnregisterChannel (ch);
		}

		public int GetDomId ()
		{
			return Thread.GetDomainID();
		}
	}
	
	[Serializable]
	public class ContextData : ILogicalThreadAffinative
	{
		public string data;
		public int id;
		public int testStep;
	}

	[Serializable]
	public abstract class ChannelManager
	{
		public abstract IChannelSender CreateClientChannel ();
		public abstract IChannelReceiver CreateServerChannel ();
	}


	//
	// Test interface
	//
	public interface IRemoteObject
	{
		int Simple ();
		string PrimitiveParams (int a, uint b, char c, string d);
		string PrimitiveParamsInOut (ref int a1, out int a2, ref float b1, out float b2, int filler, ref char c1, out char c2, ref string d1, out string d2);
		Complex ComplexParams (ArrayList a, Complex b, string c);
		Complex ComplexParamsInOut (ref ArrayList a, out Complex b, [In,Out] byte[] bytes, [In,Out] StringBuilder sb, string c);
		void ProcessContextData ();
	}

	// Base classes for tester surrogates
	
	public abstract class InstanceSurrogate : IRemoteObject
	{
		public RemoteObject RemoteObject;
		public abstract int Simple ();
		public abstract string PrimitiveParams (int a, uint b, char c, string d);
		public abstract string PrimitiveParamsInOut (ref int a1, out int a2, ref float b1, out float b2, int filler, ref char c1, out char c2, ref string d1, out string d2);
		public abstract Complex ComplexParams (ArrayList a, Complex b, string c);
		public abstract Complex ComplexParamsInOut (ref ArrayList a, out Complex b, [In,Out] byte[] bytes, [In,Out] StringBuilder sb, string c);
		public abstract void ProcessContextData ();
	}
	
	public abstract class AbstractSurrogate : IRemoteObject
	{
		public AbstractRemoteObject RemoteObject;
		public abstract int Simple ();
		public abstract string PrimitiveParams (int a, uint b, char c, string d);
		public abstract string PrimitiveParamsInOut (ref int a1, out int a2, ref float b1, out float b2, int filler, ref char c1, out char c2, ref string d1, out string d2);
		public abstract Complex ComplexParams (ArrayList a, Complex b, string c);
		public abstract Complex ComplexParamsInOut (ref ArrayList a, out Complex b, [In,Out] byte[] bytes, [In,Out] StringBuilder sb, string c);
		public abstract void ProcessContextData ();
	}

	public abstract class InterfaceSurrogate : IRemoteObject
	{
		public IRemoteObject RemoteObject;
		public abstract int Simple ();
		public abstract string PrimitiveParams (int a, uint b, char c, string d);
		public abstract string PrimitiveParamsInOut (ref int a1, out int a2, ref float b1, out float b2, int filler, ref char c1, out char c2, ref string d1, out string d2);
		public abstract Complex ComplexParams (ArrayList a, Complex b, string c);
		public abstract Complex ComplexParamsInOut (ref ArrayList a, out Complex b, [In,Out] byte[] bytes, [In,Out] StringBuilder sb, string c);
		public abstract void ProcessContextData ();
	}

	
	//
	// Test abstract base class
	//

	public abstract class AbstractRemoteObject : MarshalByRefObject
	{
		public abstract int Simple ();
		public abstract string PrimitiveParams (int a, uint b, char c, string d);
		public abstract string PrimitiveParamsInOut (ref int a1, out int a2, ref float b1, out float b2, int filler, ref char c1, out char c2, ref string d1, out string d2);
		public abstract Complex ComplexParams (ArrayList a, Complex b, string c);
		public abstract Complex ComplexParamsInOut (ref ArrayList a, out Complex b, [In,Out] byte[] bytes, [In,Out] StringBuilder sb, string c);
		public abstract void ProcessContextData ();
	}

	//
	// Test class
	//
	
	public class RemoteObject : AbstractRemoteObject, IRemoteObject
	{
		int inc = 0;
		
		public override int Simple ()
		{
			return 130772 + Thread.GetDomainID();
		}
		
		public int ReturnOne ()
		{
			return 1;
		}
		
		public int Increment ()
		{
			return inc++;
		}
		
		public override string PrimitiveParams (int a, uint b, char c, string d)
		{
			return "" + a + "-" + b + "-" + c + "-" + d + "@" + Thread.GetDomainID();
		}

		// declare an overload for bug #77191
		public void PrimitiveParams ()
		{
		}

		public override string PrimitiveParamsInOut (ref int a1, out int a2, ref float b1, out float b2, int filler, ref char c1, out char c2, ref string d1, out string d2)
		{
			string res = "" + a1 + "-" + b1.ToString(CultureInfo.InvariantCulture) + "-" + c1 + "-" + d1 + "@" + Thread.GetDomainID();
			a2 = 12345678;
			b2 = 53455.345f;
			c2 = 'g';
			d2 = "sfARREG$5345DGDfgY7656gDFG>><<dasdasd";
			a1 = 65748392;
			b1 = 98395.654f;
			c1 = 'l';
			d1 = "aasbasbdyhasbduybo234243";
			return res;
		}

		public override Complex ComplexParams (ArrayList a, Complex b, string c)
		{
			Complex cp = new Complex (33,c+ "@" + Thread.GetDomainID());
			cp.Child = b;
			cp.Child.Child = (Complex)a[0];
			return cp;
		}

		public override Complex ComplexParamsInOut (ref ArrayList a, out Complex b, [In,Out] byte[] bytes, [In,Out] StringBuilder sb, string c)
		{
			b = new Complex (33,c+ "@" + Thread.GetDomainID());
			a.Add (b);
			for (byte n=0; n<100; n++) bytes[n] = (byte)(bytes[n] + 1);
			sb.Append (" and from server");
			return b;
		}

		public override void ProcessContextData ()
		{
			string mdata = CallContext.GetData ("mustNotPass") as string;
			if (mdata != null)
				throw new Exception ("mustNotPass is not null");
			
			ContextData cdata = CallContext.GetData ("clientData") as ContextData;
			if (cdata == null) 
				throw new Exception ("server: clientData is null");
			if (cdata.data != "hi from client" || cdata.id != 1123)
				throw new Exception ("server: clientData is not valid");
			
			if (cdata.testStep == 2)
				throw new Exception ("exception from server");

			if (cdata.testStep != 1)
				throw new Exception ("invalid test step");
				
			cdata = new ContextData ();
			cdata.data = "hi from server";
			cdata.id = 3211;
			CallContext.SetData ("serverData", cdata);
		}
	}

	[Serializable]
	public class Complex
	{
		public Complex (int id, string name)
		{
			Id = id;
			Name = name;
		}

		public string Name;
		public int Id;
		public Complex Child;
	}
}
