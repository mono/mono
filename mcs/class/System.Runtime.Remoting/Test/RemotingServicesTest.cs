//
// System.Runtime.Remoting.RemotingServices NUnit V2.0 test class
//
// Author Jean-Marc ANDRE (jean-marc.andre@polymtl.ca)
//
// ToDo: I didn't write test functions for the method not yep
// implemented by Mono

using System;
using System.Collections;
using NUnit.Framework;
using System.Reflection;
using System.Runtime.Remoting;
using System.Threading;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

using MonoTests.Helpers;

namespace MonoTests.System.Runtime.Remoting.RemotingServicesInternal
{
	// We need our own proxy to intercept messages to remote object
	// and forward them using RemotingServices.ExecuteMessage
	public class MyProxy : RealProxy
	{
		MarshalByRefObject target;
		IMessageSink _sink;
		MethodBase _mthBase;
		bool methodOverloaded = false;

		public MethodBase MthBase {
			get { return _mthBase; }
		}

		public bool IsMethodOverloaded {
			get { return methodOverloaded; }
		}

		public MyProxy (Type serverType, MarshalByRefObject target)
			: base (serverType)
		{
			this.target = target;

			IChannel [] registeredChannels = ChannelServices.RegisteredChannels;
			string ObjectURI;

			// A new IMessageSink chain has to be created
			// since the RemotingServices.GetEnvoyChainForProxy() is not yet
			// implemented.
			foreach (IChannel channel in registeredChannels) {
				IChannelSender channelSender = channel as IChannelSender;
				if (channelSender != null) {
					_sink = (IMessageSink) channelSender.CreateMessageSink (RemotingServices.GetObjectUri (target), null, out ObjectURI);
				}
			}

		}

		// Messages will be intercepted here and redirected
		// to another object.
		public override IMessage Invoke (IMessage msg)
		{
			try {
				if (msg is IConstructionCallMessage) {
					IActivator remActivator = (IActivator) RemotingServices.Connect (typeof (IActivator), "tcp://localhost:1234/RemoteActivationService.rem");
					IConstructionReturnMessage crm = remActivator.Activate ((IConstructionCallMessage) msg);
					return crm;
				} else {
					methodOverloaded = RemotingServices.IsMethodOverloaded ((IMethodMessage) msg);

					_mthBase = RemotingServices.GetMethodBaseFromMethodMessage ((IMethodMessage) msg);
					MethodCallMessageWrapper mcm = new MethodCallMessageWrapper ((IMethodCallMessage) msg);
					mcm.Uri = RemotingServices.GetObjectUri ((MarshalByRefObject) target);
					MarshalByRefObject objRem = (MarshalByRefObject) Activator.CreateInstance (GetProxiedType ());
					RemotingServices.ExecuteMessage ((MarshalByRefObject) objRem, (IMethodCallMessage) msg);
					IMessage rtnMsg = null;

					try {
						rtnMsg = _sink.SyncProcessMessage (msg);
					} catch (Exception e) {
						Console.WriteLine (e.Message);
					}

					Console.WriteLine ("RR:" + rtnMsg);
					return rtnMsg;
				}
			} catch (Exception ex) {
				Console.WriteLine (ex);
				return null;
			}
		}
	} // end MyProxy

	// This class is used to create "CAO"
	public class MarshalObjectFactory : MarshalByRefObject
	{
		public MarshalObject GetNewMarshalObject ()
		{
			return new MarshalObject ();
		}
	}

	// A class used by the tests
	public class MarshalObject : ContextBoundObject
	{
		public MarshalObject ()
		{

		}

		public MarshalObject (int id, string uri)
		{
			this.id = id;
			this.uri = uri;
		}

		public int Id {
			get { return id; }
			set { id = value; }
		}

		public string Uri {
			get { return uri; }
		}

		public void Method1 ()
		{
			_called++;
			methodOneWay = RemotingServices.IsOneWay (MethodBase.GetCurrentMethod ());
		}

		public void Method2 ()
		{
			methodOneWay = RemotingServices.IsOneWay (MethodBase.GetCurrentMethod ());
		}

		public void Method2 (int i)
		{
			methodOneWay = RemotingServices.IsOneWay (MethodBase.GetCurrentMethod ());
		}

		[OneWay ()]
		public void Method3 ()
		{
			methodOneWay = RemotingServices.IsOneWay (MethodBase.GetCurrentMethod ());
		}

		public static int Called {
			get { return _called; }
		}

		public static bool IsMethodOneWay {
			get { return methodOneWay; }
		}

		private static int _called;
		private int id = 0;
		private string uri;
		private static bool methodOneWay = false;
	}

	// Another class used by the tests
	public class DerivedMarshalObject : MarshalObject
	{
		public DerivedMarshalObject () { }

		public DerivedMarshalObject (int id, string uri) : base (id, uri) { }
	}

	interface A
	{
	}

	interface B : A
	{
	}

	public class CC : MarshalByRefObject
	{
	}

	public class DD : MarshalByRefObject
	{
	}


} // namespace MonoTests.System.Runtime.Remoting.RemotingServicesInternal

namespace MonoTests.Remoting
{
	using MonoTests.System.Runtime.Remoting.RemotingServicesInternal;

	// The main test class
	[TestFixture]
	public class RemotingServicesTest
	{
		private static int MarshalObjectId = 0;

		public RemotingServicesTest ()
		{
			MarshalObjectId = 0;
		}

		// Helper function that create a new
		// MarshalObject with an unique ID
		private static MarshalObject NewMarshalObject ()
		{
			string uri = "MonoTests.System.Runtime.Remoting.RemotingServicesTest.MarshalObject" + MarshalObjectId.ToString ();
			MarshalObject objMarshal = new MarshalObject (MarshalObjectId, uri);

			MarshalObjectId++;

			return objMarshal;
		}

		// Another helper function
		private DerivedMarshalObject NewDerivedMarshalObject ()
		{
			string uri = "MonoTests.System.Runtime.Remoting.RemotingServicesTest.DerivedMarshalObject" + MarshalObjectId.ToString ();
			DerivedMarshalObject objMarshal = new DerivedMarshalObject (MarshalObjectId, uri);

			MarshalObjectId++;

			return objMarshal;
		}

		// The two folling method test RemotingServices.Marshal()
		[Test]
		public void Marshal1 ()
		{

			MarshalObject objMarshal = NewMarshalObject ();
			ObjRef objRef = RemotingServices.Marshal (objMarshal);

			Assert.IsNotNull (objRef.URI, "#A01");

			MarshalObject objRem = (MarshalObject) RemotingServices.Unmarshal (objRef);
			Assert.AreEqual (objMarshal.Id, objRem.Id, "#A02");

			objRem.Id = 2;
			Assert.AreEqual (objMarshal.Id, objRem.Id, "#A03");

			// TODO: uncomment when RemotingServices.Disconnect is implemented
			//RemotingServices.Disconnect(objMarshal);

			objMarshal = NewMarshalObject ();

			objRef = RemotingServices.Marshal (objMarshal, objMarshal.Uri);

			Assert.IsTrue (objRef.URI.EndsWith (objMarshal.Uri), "#A04");
			// TODO: uncomment when RemotingServices.Disconnect is implemented
			//RemotingServices.Disconnect(objMarshal);
		}

		[Test]
		public void Marshal2 ()
		{
			DerivedMarshalObject derivedObjMarshal = NewDerivedMarshalObject ();

			ObjRef objRef = RemotingServices.Marshal (derivedObjMarshal, derivedObjMarshal.Uri, typeof (MarshalObject));

			// Check that the type of the marshaled object is MarshalObject
			Assert.IsTrue (objRef.TypeInfo.TypeName.StartsWith ((typeof (MarshalObject)).ToString ()), "#A05");

			// TODO: uncomment when RemotingServices.Disconnect is implemented
			//RemotingServices.Disconnect(derivedObjMarshal);
		}

		// Tests RemotingServices.GetObjectUri()
		[Test]
		public void GetObjectUri ()
		{
			MarshalObject objMarshal = NewMarshalObject ();

			Assert.IsNull (RemotingServices.GetObjectUri (objMarshal), "#A06");

			RemotingServices.Marshal (objMarshal);

			Assert.IsNotNull (RemotingServices.GetObjectUri (objMarshal), "#A07");
			// TODO: uncomment when RemotingServices.Disconnect is implemented
			//RemotingServices.Disconnect(objMarshal);
		}

		// Tests RemotingServices.Connect
		[Test]
		public void Connect ()
		{
			var port = NetworkHelpers.FindFreePort ();
			MarshalObject objMarshal = NewMarshalObject ();

			IDictionary props = new Hashtable ();
			props ["name"] = objMarshal.Uri;
			props ["port"] = port;
			props ["bindTo"] = "127.0.0.1";
			TcpChannel chn = new TcpChannel (props, null, null);
			ChannelServices.RegisterChannel (chn);

			try {
				RemotingServices.Marshal (objMarshal, objMarshal.Uri);
				MarshalObject objRem = (MarshalObject) RemotingServices.Connect (typeof (MarshalObject), $"tcp://localhost:{port}/" + objMarshal.Uri);
				Assert.IsTrue (RemotingServices.IsTransparentProxy (objRem), "#A08");
			} finally {
				ChannelServices.UnregisterChannel (chn);
				RemotingServices.Disconnect (objMarshal);
			}
		}

		// Tests RemotingServices.Marshal()
		[Test]
		public void MarshalThrowException ()
		{
			var port = NetworkHelpers.FindFreePort ();
			MarshalObject objMarshal = NewMarshalObject ();

			IDictionary props = new Hashtable ();
			props ["name"] = objMarshal.Uri;
			props ["port"] = port;
			props ["bindTo"] = "127.0.0.1";
			TcpChannel chn = new TcpChannel (props, null, null);
			ChannelServices.RegisterChannel (chn);

			try {
				RemotingServices.Marshal (objMarshal, objMarshal.Uri);
				MarshalObject objRem = (MarshalObject) RemotingServices.Connect (typeof (MarshalObject), $"tcp://localhost:{port}/" + objMarshal.Uri);
				// This line should throw a RemotingException
				// It is forbidden to export an object which is not
				// a real object
				try {
					RemotingServices.Marshal (objRem, objMarshal.Uri);
					Assert.Fail ("#1");
				} catch (RemotingException e) {
				}
			} finally {
				ChannelServices.UnregisterChannel (chn);

				// TODO: uncomment when RemotingServices.Disconnect is implemented
				//RemotingServices.Disconnect(objMarshal);
			}
		}

		// Tests RemotingServices.ExecuteMessage()
		// also tests GetMethodBaseFromMessage()
		// IsMethodOverloaded()
		[Test]
		public void ExecuteMessage ()
		{
			var port = NetworkHelpers.FindFreePort ();
			IDictionary props = new Hashtable ();
			props ["port"] = port;
			props ["bindTo"] = "127.0.0.1";
			TcpChannel chn = new TcpChannel (props, null, null);
			ChannelServices.RegisterChannel (chn);
			try {
				MarshalObject objMarshal = NewMarshalObject ();
				RemotingConfiguration.RegisterWellKnownServiceType (typeof (MarshalObject), objMarshal.Uri, WellKnownObjectMode.SingleCall);

				// use a proxy to catch the Message
				MyProxy proxy = new MyProxy (typeof (MarshalObject), (MarshalObject) Activator.GetObject (typeof (MarshalObject), $"tcp://localhost:{port}/" + objMarshal.Uri));

				MarshalObject objRem = (MarshalObject) proxy.GetTransparentProxy ();

				objRem.Method1 ();

				// Tests RemotingServices.GetMethodBaseFromMethodMessage()
				Assert.AreEqual ("Method1", proxy.MthBase.Name, "#A09");
				Assert.IsFalse (proxy.IsMethodOverloaded, "#A09.1");

				objRem.Method2 ();
				Assert.IsTrue (proxy.IsMethodOverloaded, "#A09.2");

				// Tests RemotingServices.ExecuteMessage();
				// If ExecuteMessage does it job well, Method1 should be called 2 times
				Assert.AreEqual (2, MarshalObject.Called, "#A10");
			} finally {
				ChannelServices.UnregisterChannel (chn);
			}
		}

		// Tests the IsOneWay method
		[Test]
		public void IsOneWay ()
		{
			var port = NetworkHelpers.FindFreePort ();
			IDictionary props = new Hashtable ();
			props ["port"] = port;
			props ["bindTo"] = "127.0.0.1";
			TcpChannel chn = new TcpChannel (props, null, null);
			ChannelServices.RegisterChannel (chn);
			try {
				RemotingConfiguration.RegisterWellKnownServiceType (typeof (MarshalObject), "MarshalObject.rem", WellKnownObjectMode.Singleton);

				MarshalObject objRem = (MarshalObject) Activator.GetObject (typeof (MarshalObject), $"tcp://localhost:{port}/MarshalObject.rem");

				Assert.IsTrue (RemotingServices.IsTransparentProxy (objRem), "#A10.1");

				objRem.Method1 ();
				Thread.Sleep (20);
				Assert.IsFalse (MarshalObject.IsMethodOneWay, "#A10.2");
				objRem.Method3 ();
				Thread.Sleep (20);
				Assert.IsTrue (MarshalObject.IsMethodOneWay, "#A10.3");
			} finally {
				ChannelServices.UnregisterChannel (chn);
			}
		}

		[Test]
		public void GetObjRefForProxy ()
		{
			var port = NetworkHelpers.FindFreePort ();
			IDictionary props = new Hashtable ();
			props ["port"] = port;
			props ["bindTo"] = "127.0.0.1";
			TcpChannel chn = new TcpChannel (props, null, null);
			ChannelServices.RegisterChannel (chn);
			try {
				// Register le factory as a SAO
				RemotingConfiguration.RegisterWellKnownServiceType (typeof (MarshalObjectFactory), "MonoTests.System.Runtime.Remoting.RemotingServicesTest.Factory.soap", WellKnownObjectMode.Singleton);

				MarshalObjectFactory objFactory = (MarshalObjectFactory) Activator.GetObject (typeof (MarshalObjectFactory), $"tcp://localhost:{port}/MonoTests.System.Runtime.Remoting.RemotingServicesTest.Factory.soap");

				// Get a new "CAO"
				MarshalObject objRem = objFactory.GetNewMarshalObject ();

				ObjRef objRefRem = RemotingServices.GetObjRefForProxy ((MarshalByRefObject) objRem);

				Assert.IsNotNull (objRefRem, "#A11");
			} finally {
				ChannelServices.UnregisterChannel (chn);
			}
		}

		// Tests GetRealProxy
		[Test]
		public void GetRealProxy ()
		{
			var port = NetworkHelpers.FindFreePort ();
			IDictionary props = new Hashtable ();
			props ["port"] = port;
			props ["bindTo"] = "127.0.0.1";
			TcpChannel chn = new TcpChannel (props, null, null);
			ChannelServices.RegisterChannel (chn);
			try {
				RemotingConfiguration.RegisterWellKnownServiceType (typeof (MarshalObject), "MonoTests.System.Runtime.Remoting.RemotingServicesTest.MarshalObject.soap", WellKnownObjectMode.Singleton);

				MyProxy proxy = new MyProxy (typeof (MarshalObject), (MarshalByRefObject) Activator.GetObject (typeof (MarshalObject), $"tcp://localhost:{port}/MonoTests.System.Runtime.Remoting.RemotingServicesTest.MarshalObject.soap"));
				MarshalObject objRem = (MarshalObject) proxy.GetTransparentProxy ();

				RealProxy rp = RemotingServices.GetRealProxy (objRem);

				Assert.IsNotNull (rp, "#A12");
				Assert.AreEqual ("MonoTests.System.Runtime.Remoting.RemotingServicesInternal.MyProxy", rp.GetType ().ToString (), "#A13");
			} finally {
				ChannelServices.UnregisterChannel (chn);
			}
		}

		// Tests SetObjectUriForMarshal()
		[Test]
		public void SetObjectUriForMarshal ()
		{
			var port = NetworkHelpers.FindFreePort ();
			IDictionary props = new Hashtable ();
			props ["port"] = port;
			props ["bindTo"] = "127.0.0.1";
			TcpChannel chn = new TcpChannel (props, null, null);
			ChannelServices.RegisterChannel (chn);
			try {
				MarshalObject objRem = NewMarshalObject ();
				RemotingServices.SetObjectUriForMarshal (objRem, objRem.Uri);
				RemotingServices.Marshal (objRem);

				objRem = (MarshalObject) Activator.GetObject (typeof (MarshalObject), $"tcp://localhost:{port}/" + objRem.Uri);
				Assert.IsNotNull (objRem, "#A14");
			} finally {
				ChannelServices.UnregisterChannel (chn);
			}

		}

		// Tests GetServeurTypeForUri()
		[Test]
		public void GetServeurTypeForUri ()
		{
			var port = NetworkHelpers.FindFreePort ();
			IDictionary props = new Hashtable ();
			props ["port"] = port;
			props ["bindTo"] = "127.0.0.1";
			TcpChannel chn = new TcpChannel (props, null, null);
			Type type = typeof (MarshalObject);
			ChannelServices.RegisterChannel (chn);
			try {
				MarshalObject objRem = NewMarshalObject ();
				RemotingServices.SetObjectUriForMarshal (objRem, objRem.Uri);
				RemotingServices.Marshal (objRem);

				Type typeRem = RemotingServices.GetServerTypeForUri (RemotingServices.GetObjectUri (objRem));
				Assert.AreEqual (type, typeRem, "#A15");
			} finally {
				ChannelServices.UnregisterChannel (chn);
			}
		}

		// Tests IsObjectOutOfDomain
		// Tests IsObjectOutOfContext
		[Test]
		[Category ("NotWorking")]
		public void IsObjectOutOf ()
		{
			var port = NetworkHelpers.FindFreePort ();
			IDictionary props = new Hashtable ();
			props ["port"] = port;
			props ["bindTo"] = "127.0.0.1";
			TcpChannel chn = new TcpChannel (props, null, null);
			ChannelServices.RegisterChannel (chn);
			try {
				RemotingConfiguration.RegisterWellKnownServiceType (typeof (MarshalObject), "MarshalObject2.rem", WellKnownObjectMode.Singleton);

				MarshalObject objRem = (MarshalObject) Activator.GetObject (typeof (MarshalObject), $"tcp://localhost:{port}/MarshalObject2.rem");

				Assert.IsTrue (RemotingServices.IsObjectOutOfAppDomain (objRem), "#A16");
				Assert.IsTrue (RemotingServices.IsObjectOutOfContext (objRem), "#A17");

				MarshalObject objMarshal = new MarshalObject ();
				Assert.IsFalse (RemotingServices.IsObjectOutOfAppDomain (objMarshal), "#A18");
				Assert.IsFalse (RemotingServices.IsObjectOutOfContext (objMarshal), "#A19");
			} finally {
				ChannelServices.UnregisterChannel (chn);
			}
		}

		[Test]
		public void ApplicationNameTest ()
		{
			var port = NetworkHelpers.FindFreePort ();
			RemotingConfiguration.ApplicationName = "app";
			IDictionary props = new Hashtable ();
			props ["port"] = port;
			props ["bindTo"] = "127.0.0.1";
			TcpChannel chn = new TcpChannel (props, null, null);
			ChannelServices.RegisterChannel (chn);
			try {
				RemotingConfiguration.RegisterWellKnownServiceType (typeof (MarshalObject), "obj3.rem", WellKnownObjectMode.Singleton);

				MarshalObject objRem = (MarshalObject) Activator.GetObject (typeof (MarshalObject), $"tcp://localhost:{port}/app/obj3.rem");
				MarshalObject objRem2 = (MarshalObject) Activator.GetObject (typeof (MarshalObject), $"tcp://localhost:{port}/obj3.rem");

				Assert.IsTrue (RemotingServices.IsTransparentProxy (objRem), "#AN1");
				Assert.IsTrue (RemotingServices.IsTransparentProxy (objRem2), "#AN2");

				Assert.IsNotNull (RemotingServices.GetServerTypeForUri ("obj3.rem"), "#AN3");
				Assert.IsNotNull (RemotingServices.GetServerTypeForUri ("/app/obj3.rem"), "#AN4");
				Assert.IsNull (RemotingServices.GetServerTypeForUri ("//app/obj3.rem"), "#AN5");
				Assert.IsNull (RemotingServices.GetServerTypeForUri ("app/obj3.rem"), "#AN6");
				Assert.IsNull (RemotingServices.GetServerTypeForUri ("/whatever/obj3.rem"), "#AN7");
				Assert.IsNotNull (RemotingServices.GetServerTypeForUri ("/obj3.rem"), "#AN8");
				Assert.IsNull (RemotingServices.GetServerTypeForUri ("//obj3.rem"), "#AN9");
			} finally {
				ChannelServices.UnregisterChannel (chn);
			}
		}

		[Test]
		public void GetObjectWithChannelDataTest ()
		{
			var port = NetworkHelpers.FindFreePort ();
			IDictionary props = new Hashtable ();
			props ["port"] = port;
			props ["bindTo"] = "127.0.0.1";
			TcpChannel chn = new TcpChannel (props, null, null);
			ChannelServices.RegisterChannel (chn);
			try {
				RemotingConfiguration.RegisterWellKnownServiceType (typeof (MarshalObject), "getobjectwithchanneldata.rem", WellKnownObjectMode.Singleton);

				string channelData = "test";
				Assert.IsNotNull (Activator.GetObject (typeof (MarshalObject), $"tcp://localhost:{port}/getobjectwithchanneldata.rem", channelData), "#01");
			} finally {
				ChannelServices.UnregisterChannel (chn);
			}
		}

		public interface IMarshalTest
		{
			void DoSomething();
		}

		public interface ITest2 : IMarshalTest
		{
			void DoSomethingElse();
		}

		public class TestClass : MarshalByRefObject, ITest2
		{
			public void DoSomething()
			{
			}

			public void DoSomethingElse()
			{
			}
		}

		[Test]
		public void MarshalBaseInterfaces()
		{
			TestClass test = new TestClass();
			ObjRef obj = RemotingServices.Marshal(test, "TestClass", typeof(ITest2));
			FieldInfo interfacesImplemented = obj.TypeInfo.GetType().GetField("interfacesImplemented", BindingFlags.NonPublic | BindingFlags.Instance);
			string[] interfaces = (string[])interfacesImplemented.GetValue(obj.TypeInfo);
			Assert.AreEqual(2, interfaces.Length);
			Assert.IsTrue(interfaces[0].Contains("IMarshalTest"));
			Assert.IsTrue(interfaces[1].Contains("ITest2"));
		}

		[Test]
		[Ignore ("We cannot test RemotingConfiguration.Configure() because it keeps channels registered. If we really need to test it, do it as a standalone case")]
		public void ConnectProxyCast ()
		{
			var port = NetworkHelpers.FindFreePort ();
			object o;
			RemotingConfiguration.Configure (null);

			o = RemotingServices.Connect (typeof (MarshalByRefObject), $"tcp://localhost:{port}/ff1.rem");
			Assert.IsInstanceOfType (typeof (DD), o, "#m1");
			Assert.IsInstanceOfType (typeof (A), o, "#m2");
			Assert.IsInstanceOfType (typeof (B), o, "#m3");
			AssertHelper.IsNotInstanceOfType (typeof (CC), !(o is CC), "#m4");

			o = RemotingServices.Connect (typeof (A), $"tcp://localhost:{port}/ff3.rem");
			Assert.IsInstanceOfType (typeof (DD), o, "#a1");
			Assert.IsInstanceOfType (typeof (A), o, "#a2");
			Assert.IsInstanceOfType (typeof (B), o, "#a3");
			AssertHelper.IsNotInstanceOfType (typeof (CC), o, "#a4");

			o = RemotingServices.Connect (typeof (DD), $"tcp://localhost:{port}/ff4.rem");
			Assert.IsInstanceOfType (typeof (DD), o, "#d1");
			Assert.IsInstanceOfType (typeof (A), o, "#d2");
			Assert.IsInstanceOfType (typeof (B), o, "#d3");
			AssertHelper.IsNotInstanceOfType (typeof (CC), o, "#d4");

			o = RemotingServices.Connect (typeof (CC), $"tcp://localhost:{port}/ff5.rem");
			AssertHelper.IsNotInstanceOfType (typeof (DD), o, "#c1");
			Assert.IsInstanceOfType (typeof (A), o, "#c2");
			Assert.IsInstanceOfType (typeof (B), o, "#c3");
			Assert.IsInstanceOfType (typeof (CC), o, "#c4");
		}
		// Don't add any tests that must create channels
		// after ConnectProxyCast (), because this test calls
		// RemotingConfiguration.Configure ().
	} // end class RemotingServicesTest
} // end of namespace MonoTests.Remoting
