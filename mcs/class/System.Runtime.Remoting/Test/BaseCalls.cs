//
// MonoTests.System.Runtime.Remoting.BaseCalls.cs
//
// Author: Lluis Sanchez Gual (lluis@ximian.com)
//
// 2003 (C) Copyright, Ximian, Inc.
//

using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Lifetime;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Contexts;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace MonoTests.System.Runtime.Remoting
{
	public abstract class BaseCallTest : Assertion
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

		protected virtual int CreateServer ()
		{
			ChannelManager cm = CreateChannelManager ();
			chs = cm.CreateClientChannel ();
			ChannelServices.RegisterChannel (chs);

			AppDomain domain = AppDomain.CreateDomain ("testdomain");
			server = (CallsDomainServer) domain.CreateInstanceAndUnwrap(GetType().Assembly.FullName,"MonoTests.System.Runtime.Remoting.CallsDomainServer");
			remoteUris = server.Start (cm);
			return server.GetDomId ();
		}
		
		protected virtual void ShutdownServer ()
		{
			server.Stop ();
			if (chs != null)
				ChannelServices.UnregisterChannel (chs);
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
		public void TestInstanceComplexParamsInOut ()
		{
			RunTestComplexParamsInOut (InternalGetInstanceSurrogate());
		}

		[Test]
		public void TestAbstractComplexParamsInOut ()
		{
			RunTestComplexParamsInOut (InternalGetAbstractSurrogate());
		}

		[Test]
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
			AssertEquals ("ReturnValue", 130772 + remoteDomId, testerSurrogate.Simple ());
		}

		public void RunTestPrimitiveParams (IRemoteObject testerSurrogate)
		{
			AssertEquals ("ReturnValue", "11-22-L-SG@"+remoteDomId, testerSurrogate.PrimitiveParams (11, 22, 'L', "SG"));
		}

		public void RunTestPrimitiveParamsInOut (IRemoteObject testerSurrogate)
		{
			int a2, a1 = 9876543;
			float b2, b1 = 82437.83f;
			char c2, c1 = 's';
			string d2, d1 = "asdASDzxcZXC";

			string res = testerSurrogate.PrimitiveParamsInOut (ref a1, out a2, ref b1, out b2, ref c1, out c2, ref d1, out d2);

			AssertEquals ("ReturnValue", "9876543-82437.83-s-asdASDzxcZXC@" + remoteDomId, res);

			AssertEquals ("a2", 12345678, a2);
			AssertEquals ("b2", 53455.345f, b2);
			AssertEquals ("c2", 'g', c2);
			AssertEquals ("d2", "sfARREG$5345DGDfgY7656gDFG>><<dasdasd", d2);

			AssertEquals ("a1", 65748392, a1);
			AssertEquals ("b1", 98395.654f, b1);
			AssertEquals ("c1", 'l', c1);
			AssertEquals ("d1", "aasbasbdyhasbduybo234243", d1);
		}

		public void RunTestComplexParams (IRemoteObject testerSurrogate)
		{
			ArrayList list = new ArrayList ();
			list.Add (new Complex (11,"first"));
			Complex c = new Complex (22,"second");

			Complex r = testerSurrogate.ComplexParams (list, c, "third");

			AssertNotNull ("ReturnValue is null", r);
			AssertNotNull ("ReturnValue.Child is null", r.Child);
			AssertNotNull ("ReturnValue.Child.Child is null", r.Child.Child);
			
			AssertEquals ("ReturnValue.Id", 33, r.Id);
			AssertEquals ("ReturnValue.Name", "third@"+remoteDomId, r.Name);
			AssertEquals ("ReturnValue.Child.Id", 22, r.Child.Id);
			AssertEquals ("ReturnValue.Child.Name", "second", r.Child.Name);
			AssertEquals ("ReturnValue.Child.Child.Id", 11, r.Child.Child.Id);
			AssertEquals ("ReturnValue.Child.Child.Name", "first", r.Child.Child.Name);
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

			AssertNotNull ("ReturnValue is null", r);
			AssertNotNull ("c is null", c);
			AssertNotNull ("list is null", list);
			Assert ("Invalid list count", list.Count == 3);
			AssertNotNull ("list[0] is null", list[0]);
			AssertNotNull ("list[1] is null", list[1]);
			AssertNotNull ("list[2] is null", list[2]);
			AssertNotNull ("bytes is null", bytes);
			AssertNotNull ("sb is null", sb);
			
			AssertEquals ("ReturnValue.Id", 33, r.Id);
			AssertEquals ("ReturnValue.Name", "third@"+remoteDomId, r.Name);
			AssertEquals ("c.Id", 33, c.Id);
			AssertEquals ("c.Name", "third@"+remoteDomId, c.Name);

			AssertEquals ("list[2].Id", 33, ((Complex)list[2]).Id);
			AssertEquals ("list[2].Name", "third@"+remoteDomId, ((Complex)list[2]).Name);
			AssertEquals ("list[1].Id", 22, ((Complex)list[1]).Id);
			AssertEquals ("list[1].Name", "second", ((Complex)list[1]).Name);
			AssertEquals ("list[0].Id", 11, ((Complex)list[0]).Id);
			AssertEquals ("list[0].Name", "first", ((Complex)list[0]).Name);
			
			AssertEquals ("sb", "hello from client and from server", sb.ToString ());
			for (int n=0; n<100; n++) 
				AssertEquals ("bytes["+n+"]", n+1, bytes[n]);
		}
		
		public void RunTestProcessContextData (IRemoteObject testerSurrogate)
		{
			CallContext.FreeNamedDataSlot ("clientData");
			CallContext.FreeNamedDataSlot ("serverData");
			CallContext.FreeNamedDataSlot ("mustNotPass");

			ContextData cdata = new ContextData ();
			cdata.data = "hi from client";
			cdata.id = 1123;
			CallContext.SetData ("clientData", cdata);
			CallContext.SetData ("mustNotPass", "more data");
			
			testerSurrogate.ProcessContextData ();
			
			cdata = CallContext.GetData ("clientData") as ContextData;
			AssertNotNull ("clientData is null", cdata);
			AssertEquals ("clientData.data", "hi from client", cdata.data);
			AssertEquals ("clientData.id", 1123, cdata.id);
			
			cdata = CallContext.GetData ("serverData") as ContextData;
			AssertNotNull ("serverData is null", cdata);
			AssertEquals ("serverData.data", "hi from server", cdata.data);
			AssertEquals ("serverData.id", 3211, cdata.id);
			
			string mdata = CallContext.GetData ("mustNotPass") as string;
			AssertNotNull ("mustNotPass is null", mdata);
			AssertEquals ("mustNotPass", "more data", mdata);
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
		string PrimitiveParamsInOut (ref int a1, out int a2, ref float b1, out float b2, ref char c1, out char c2, ref string d1, out string d2);
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
		public abstract string PrimitiveParamsInOut (ref int a1, out int a2, ref float b1, out float b2, ref char c1, out char c2, ref string d1, out string d2);
		public abstract Complex ComplexParams (ArrayList a, Complex b, string c);
		public abstract Complex ComplexParamsInOut (ref ArrayList a, out Complex b, [In,Out] byte[] bytes, [In,Out] StringBuilder sb, string c);
		public abstract void ProcessContextData ();
	}
	
	public abstract class AbstractSurrogate : IRemoteObject
	{
		public AbstractRemoteObject RemoteObject;
		public abstract int Simple ();
		public abstract string PrimitiveParams (int a, uint b, char c, string d);
		public abstract string PrimitiveParamsInOut (ref int a1, out int a2, ref float b1, out float b2, ref char c1, out char c2, ref string d1, out string d2);
		public abstract Complex ComplexParams (ArrayList a, Complex b, string c);
		public abstract Complex ComplexParamsInOut (ref ArrayList a, out Complex b, [In,Out] byte[] bytes, [In,Out] StringBuilder sb, string c);
		public abstract void ProcessContextData ();
	}

	public abstract class InterfaceSurrogate : IRemoteObject
	{
		public IRemoteObject RemoteObject;
		public abstract int Simple ();
		public abstract string PrimitiveParams (int a, uint b, char c, string d);
		public abstract string PrimitiveParamsInOut (ref int a1, out int a2, ref float b1, out float b2, ref char c1, out char c2, ref string d1, out string d2);
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
		public abstract string PrimitiveParamsInOut (ref int a1, out int a2, ref float b1, out float b2, ref char c1, out char c2, ref string d1, out string d2);
		public abstract Complex ComplexParams (ArrayList a, Complex b, string c);
		public abstract Complex ComplexParamsInOut (ref ArrayList a, out Complex b, [In,Out] byte[] bytes, [In,Out] StringBuilder sb, string c);
		public abstract void ProcessContextData ();
	}

	//
	// Test class
	//
	
	public class RemoteObject : AbstractRemoteObject, IRemoteObject
	{
		public override int Simple ()
		{
			return 130772 + Thread.GetDomainID();
		}

		public override string PrimitiveParams (int a, uint b, char c, string d)
		{
			return "" + a + "-" + b + "-" + c + "-" + d + "@" + Thread.GetDomainID();
		}

		public override string PrimitiveParamsInOut (ref int a1, out int a2, ref float b1, out float b2, ref char c1, out char c2, ref string d1, out string d2)
		{
			string res = "" + a1 + "-" + b1 + "-" + c1 + "-" + d1 + "@" + Thread.GetDomainID();
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
			ContextData cdata = CallContext.GetData ("clientData") as ContextData;
			if (cdata == null) 
				throw new Exception ("server: clientData is null");
			if (cdata.data != "hi from client" || cdata.id != 1123)
				throw new Exception ("server: clientData is not valid");
			
			cdata = new ContextData ();
			cdata.data = "hi from server";
			cdata.id = 3211;
			CallContext.SetData ("serverData", cdata);
			
			string mdata = CallContext.GetData ("mustNotPass") as string;
			if (mdata != null) throw new Exception ("mustNotPass is not null");
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
