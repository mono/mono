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
	public class RemotingServicesTest : Assertion
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

			Assert ("#A01", objRef.URI != null);

			MarshalObject objRem = (MarshalObject) RemotingServices.Unmarshal (objRef);
			AssertEquals ("#A02", objMarshal.Id, objRem.Id);

			objRem.Id = 2;
			AssertEquals ("#A03", objMarshal.Id, objRem.Id);

			// TODO: uncomment when RemotingServices.Disconnect is implemented
			//RemotingServices.Disconnect(objMarshal);

			objMarshal = NewMarshalObject ();

			objRef = RemotingServices.Marshal (objMarshal, objMarshal.Uri);

			Assert ("#A04", objRef.URI.EndsWith (objMarshal.Uri));
			// TODO: uncomment when RemotingServices.Disconnect is implemented
			//RemotingServices.Disconnect(objMarshal);
		}

		[Test]
		public void Marshal2 ()
		{
			DerivedMarshalObject derivedObjMarshal = NewDerivedMarshalObject ();

			ObjRef objRef = RemotingServices.Marshal (derivedObjMarshal, derivedObjMarshal.Uri, typeof (MarshalObject));

			// Check that the type of the marshaled object is MarshalObject
			Assert ("#A05", objRef.TypeInfo.TypeName.StartsWith ((typeof (MarshalObject)).ToString ()));

			// TODO: uncomment when RemotingServices.Disconnect is implemented
			//RemotingServices.Disconnect(derivedObjMarshal);
		}

		// Tests RemotingServices.GetObjectUri()
		[Test]
		public void GetObjectUri ()
		{
			MarshalObject objMarshal = NewMarshalObject ();

			Assert ("#A06", RemotingServices.GetObjectUri (objMarshal) == null);

			RemotingServices.Marshal (objMarshal);

			Assert ("#A07", RemotingServices.GetObjectUri (objMarshal) != null);
			// TODO: uncomment when RemotingServices.Disconnect is implemented
			//RemotingServices.Disconnect(objMarshal);
		}

		// Tests RemotingServices.Connect
		[Test]
		public void Connect ()
		{
			MarshalObject objMarshal = NewMarshalObject ();

			IDictionary props = new Hashtable ();
			props ["name"] = objMarshal.Uri;
			props ["port"] = 1236;
			TcpChannel chn = new TcpChannel (props, null, null);
			ChannelServices.RegisterChannel (chn);

			try {
				RemotingServices.Marshal (objMarshal, objMarshal.Uri);
				MarshalObject objRem = (MarshalObject) RemotingServices.Connect (typeof (MarshalObject), "tcp://localhost:1236/" + objMarshal.Uri);
				Assert ("#A08", RemotingServices.IsTransparentProxy (objRem));
			} finally {
				ChannelServices.UnregisterChannel (chn);
				RemotingServices.Disconnect (objMarshal);
			}
		}

		// Tests RemotingServices.Marshal()
		[Test]
		public void MarshalThrowException ()
		{
			MarshalObject objMarshal = NewMarshalObject ();

			IDictionary props = new Hashtable ();
			props ["name"] = objMarshal.Uri;
			props ["port"] = 1237;
			TcpChannel chn = new TcpChannel (props, null, null);
			ChannelServices.RegisterChannel (chn);

			try {
				RemotingServices.Marshal (objMarshal, objMarshal.Uri);
				MarshalObject objRem = (MarshalObject) RemotingServices.Connect (typeof (MarshalObject), "tcp://localhost:1237/" + objMarshal.Uri);
				// This line should throw a RemotingException
				// It is forbidden to export an object which is not
				// a real object
				try {
					RemotingServices.Marshal (objRem, objMarshal.Uri);
					Fail ("#1");
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
			TcpChannel chn = new TcpChannel (1235);
			ChannelServices.RegisterChannel (chn);
			try {
				MarshalObject objMarshal = NewMarshalObject ();
				RemotingConfiguration.RegisterWellKnownServiceType (typeof (MarshalObject), objMarshal.Uri, WellKnownObjectMode.SingleCall);

				// use a proxy to catch the Message
				MyProxy proxy = new MyProxy (typeof (MarshalObject), (MarshalObject) Activator.GetObject (typeof (MarshalObject), "tcp://localhost:1235/" + objMarshal.Uri));

				MarshalObject objRem = (MarshalObject) proxy.GetTransparentProxy ();

				objRem.Method1 ();

				// Tests RemotingServices.GetMethodBaseFromMethodMessage()
				AssertEquals ("#A09", "Method1", proxy.MthBase.Name);
				Assert ("#A09.1", !proxy.IsMethodOverloaded);

				objRem.Method2 ();
				Assert ("#A09.2", proxy.IsMethodOverloaded);

				// Tests RemotingServices.ExecuteMessage();
				// If ExecuteMessage does it job well, Method1 should be called 2 times
				AssertEquals ("#A10", 2, MarshalObject.Called);
			} finally {
				ChannelServices.UnregisterChannel (chn);
			}
		}

		// Tests the IsOneWay method
		[Test]
		public void IsOneWay ()
		{
			TcpChannel chn = new TcpChannel (1238);
			ChannelServices.RegisterChannel (chn);
			try {
				RemotingConfiguration.RegisterWellKnownServiceType (typeof (MarshalObject), "MarshalObject.rem", WellKnownObjectMode.Singleton);

				MarshalObject objRem = (MarshalObject) Activator.GetObject (typeof (MarshalObject), "tcp://localhost:1238/MarshalObject.rem");

				Assert ("#A10.1", RemotingServices.IsTransparentProxy (objRem));

				objRem.Method1 ();
				Thread.Sleep (20);
				Assert ("#A10.2", !MarshalObject.IsMethodOneWay);
				objRem.Method3 ();
				Thread.Sleep (20);
				Assert ("#A10.3", MarshalObject.IsMethodOneWay);
			} finally {
				ChannelServices.UnregisterChannel (chn);
			}
		}

		[Test]
		public void GetObjRefForProxy ()
		{
			TcpChannel chn = new TcpChannel (1239);
			ChannelServices.RegisterChannel (chn);
			try {
				// Register le factory as a SAO
				RemotingConfiguration.RegisterWellKnownServiceType (typeof (MarshalObjectFactory), "MonoTests.System.Runtime.Remoting.RemotingServicesTest.Factory.soap", WellKnownObjectMode.Singleton);

				MarshalObjectFactory objFactory = (MarshalObjectFactory) Activator.GetObject (typeof (MarshalObjectFactory), "tcp://localhost:1239/MonoTests.System.Runtime.Remoting.RemotingServicesTest.Factory.soap");

				// Get a new "CAO"
				MarshalObject objRem = objFactory.GetNewMarshalObject ();

				ObjRef objRefRem = RemotingServices.GetObjRefForProxy ((MarshalByRefObject) objRem);

				Assert ("#A11", objRefRem != null);
			} finally {
				ChannelServices.UnregisterChannel (chn);
			}
		}

		// Tests GetRealProxy
		[Test]
		public void GetRealProxy ()
		{
			TcpChannel chn = new TcpChannel (1241);
			ChannelServices.RegisterChannel (chn);
			try {
				RemotingConfiguration.RegisterWellKnownServiceType (typeof (MarshalObject), "MonoTests.System.Runtime.Remoting.RemotingServicesTest.MarshalObject.soap", WellKnownObjectMode.Singleton);

				MyProxy proxy = new MyProxy (typeof (MarshalObject), (MarshalByRefObject) Activator.GetObject (typeof (MarshalObject), "tcp://localhost:1241/MonoTests.System.Runtime.Remoting.RemotingServicesTest.MarshalObject.soap"));
				MarshalObject objRem = (MarshalObject) proxy.GetTransparentProxy ();

				RealProxy rp = RemotingServices.GetRealProxy (objRem);

				Assert ("#A12", rp != null);
				AssertEquals ("#A13", "MonoTests.System.Runtime.Remoting.RemotingServicesInternal.MyProxy", rp.GetType ().ToString ());
			} finally {
				ChannelServices.UnregisterChannel (chn);
			}
		}

		// Tests SetObjectUriForMarshal()
		[Test]
		public void SetObjectUriForMarshal ()
		{
			TcpChannel chn = new TcpChannel (1242);
			ChannelServices.RegisterChannel (chn);
			try {
				MarshalObject objRem = NewMarshalObject ();
				RemotingServices.SetObjectUriForMarshal (objRem, objRem.Uri);
				RemotingServices.Marshal (objRem);

				objRem = (MarshalObject) Activator.GetObject (typeof (MarshalObject), "tcp://localhost:1242/" + objRem.Uri);
				Assert ("#A14", objRem != null);
			} finally {
				ChannelServices.UnregisterChannel (chn);
			}

		}

		// Tests GetServeurTypeForUri()
		[Test]
		public void GetServeurTypeForUri ()
		{
			TcpChannel chn = new TcpChannel (1243);
			Type type = typeof (MarshalObject);
			ChannelServices.RegisterChannel (chn);
			try {
				MarshalObject objRem = NewMarshalObject ();
				RemotingServices.SetObjectUriForMarshal (objRem, objRem.Uri);
				RemotingServices.Marshal (objRem);

				Type typeRem = RemotingServices.GetServerTypeForUri (RemotingServices.GetObjectUri (objRem));
				AssertEquals ("#A15", type, typeRem);
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
			TcpChannel chn = new TcpChannel (1245);
			ChannelServices.RegisterChannel (chn);
			try {
				RemotingConfiguration.RegisterWellKnownServiceType (typeof (MarshalObject), "MarshalObject2.rem", WellKnownObjectMode.Singleton);

				MarshalObject objRem = (MarshalObject) Activator.GetObject (typeof (MarshalObject), "tcp://localhost:1245/MarshalObject2.rem");

				Assert ("#A16", RemotingServices.IsObjectOutOfAppDomain (objRem));
				Assert ("#A17", RemotingServices.IsObjectOutOfContext (objRem));

				MarshalObject objMarshal = new MarshalObject ();
				Assert ("#A18", !RemotingServices.IsObjectOutOfAppDomain (objMarshal));
				Assert ("#A19", !RemotingServices.IsObjectOutOfContext (objMarshal));
			} finally {
				ChannelServices.UnregisterChannel (chn);
			}
		}

		[Test]
		public void ApplicationNameTest ()
		{
			RemotingConfiguration.ApplicationName = "app";
			TcpChannel chn = new TcpChannel (1246);
			ChannelServices.RegisterChannel (chn);
			try {
				RemotingConfiguration.RegisterWellKnownServiceType (typeof (MarshalObject), "obj3.rem", WellKnownObjectMode.Singleton);

				MarshalObject objRem = (MarshalObject) Activator.GetObject (typeof (MarshalObject), "tcp://localhost:1246/app/obj3.rem");
				MarshalObject objRem2 = (MarshalObject) Activator.GetObject (typeof (MarshalObject), "tcp://localhost:1246/obj3.rem");

				Assert ("#AN1", RemotingServices.IsTransparentProxy (objRem));
				Assert ("#AN2", RemotingServices.IsTransparentProxy (objRem2));

				AssertNotNull ("#AN3", RemotingServices.GetServerTypeForUri ("obj3.rem"));
				AssertNotNull ("#AN4", RemotingServices.GetServerTypeForUri ("/app/obj3.rem"));
				AssertNull ("#AN5", RemotingServices.GetServerTypeForUri ("//app/obj3.rem"));
				AssertNull ("#AN6", RemotingServices.GetServerTypeForUri ("app/obj3.rem"));
				AssertNull ("#AN7", RemotingServices.GetServerTypeForUri ("/whatever/obj3.rem"));
				AssertNotNull ("#AN8", RemotingServices.GetServerTypeForUri ("/obj3.rem"));
				AssertNull ("#AN9", RemotingServices.GetServerTypeForUri ("//obj3.rem"));
			} finally {
				ChannelServices.UnregisterChannel (chn);
			}
		}

		[Test]
		public void GetObjectWithChannelDataTest ()
		{
			TcpChannel chn = new TcpChannel (1247);
			ChannelServices.RegisterChannel (chn);
			try {
				RemotingConfiguration.RegisterWellKnownServiceType (typeof (MarshalObject), "getobjectwithchanneldata.rem", WellKnownObjectMode.Singleton);

				string channelData = "test";
				AssertNotNull ("#01", Activator.GetObject (typeof (MarshalObject), "tcp://localhost:1247/getobjectwithchanneldata.rem", channelData));
			} finally {
				ChannelServices.UnregisterChannel (chn);
			}
		}

		[Test]
		[Ignore ("We cannot test RemotingConfiguration.Configure() because it keeps channels registered. If we really need to test it, do it as a standalone case")]
		public void ConnectProxyCast ()
		{
			object o;
			RemotingConfiguration.Configure (null);

			o = RemotingServices.Connect (typeof (MarshalByRefObject), "tcp://localhost:3434/ff1.rem");
			Assert ("#m1", o is DD);
			Assert ("#m2", o is A);
			Assert ("#m3", o is B);
			Assert ("#m4", !(o is CC));

			o = RemotingServices.Connect (typeof (A), "tcp://localhost:3434/ff3.rem");
			Assert ("#a1", o is DD);
			Assert ("#a2", o is A);
			Assert ("#a3", o is B);
			Assert ("#a4", !(o is CC));

			o = RemotingServices.Connect (typeof (DD), "tcp://localhost:3434/ff4.rem");
			Assert ("#d1", o is DD);
			Assert ("#d2", o is A);
			Assert ("#d3", o is B);
			Assert ("#d4", !(o is CC));

			o = RemotingServices.Connect (typeof (CC), "tcp://localhost:3434/ff5.rem");
			Assert ("#c1", !(o is DD));
			Assert ("#c2", o is A);
			Assert ("#c3", o is B);
			Assert ("#c4", o is CC);
		}
		// Don't add any tests that must create channels
		// after ConnectProxyCast (), because this test calls
		// RemotingConfiguration.Configure ().
	} // end class RemotingServicesTest
} // end of namespace MonoTests.Remoting
