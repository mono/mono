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
	public class MyProxy: RealProxy
	{
		MarshalByRefObject target;
		IMessageSink _sink;
		MethodBase _mthBase;
		bool methodOverloaded = false;
		
		public MethodBase MthBase
		{
			get{ return _mthBase;}
		}
		
		public bool IsMethodOverloaded
		{
			get{return methodOverloaded;}
		}
		
		public MyProxy(Type serverType, MarshalByRefObject target): base(serverType)
		{
			this.target = target;
			
			IChannel[] registeredChannels = ChannelServices.RegisteredChannels;
			string ObjectURI;
			
			// A new IMessageSink chain has to be created
			// since the RemotingServices.GetEnvoyChainForProxy() is not yet
			// implemented.
			foreach(IChannel channel in registeredChannels)
			{
				IChannelSender channelSender = channel as IChannelSender;
				if(channelSender != null)
				{
					_sink = (IMessageSink) channelSender.CreateMessageSink(RemotingServices.GetObjectUri(target), null, out ObjectURI);
				}
			}
			
		}
		
		// Messages will be intercepted here and redirected
		// to another object.
		public override IMessage Invoke(IMessage msg)
		{
			if(msg is IConstructionCallMessage)
			{
				IActivator remActivator = (IActivator) RemotingServices.Connect(typeof(IActivator), "tcp://localhost:1234/RemoteActivationService.rem");
				IConstructionReturnMessage crm = remActivator.Activate((IConstructionCallMessage)msg);
				return crm;
			}
			else
			{
				methodOverloaded = RemotingServices.IsMethodOverloaded((IMethodMessage)msg);
				
				_mthBase = RemotingServices.GetMethodBaseFromMethodMessage((IMethodMessage)msg);
				MethodCallMessageWrapper mcm = new MethodCallMessageWrapper((IMethodCallMessage) msg);
				mcm.Uri = RemotingServices.GetObjectUri((MarshalByRefObject)target);
				MarshalByRefObject objRem = (MarshalByRefObject)Activator.CreateInstance(GetProxiedType());
				RemotingServices.ExecuteMessage((MarshalByRefObject)objRem, (IMethodCallMessage)msg);
				IMessage rtnMsg = null;
				
				try
				{
					rtnMsg = _sink.SyncProcessMessage(msg);
				}
				catch(Exception e)
				{
					Console.WriteLine(e.Message);
				}
				
				return rtnMsg;
			}
		}
	} // end MyProxy
	
	// This class is used to create "CAO"
	public class MarshalObjectFactory: MarshalByRefObject
	{
		public MarshalObject GetNewMarshalObject()
		{
			return new MarshalObject();
		}
	}
	
	// A class used by the tests
	public class MarshalObject: ContextBoundObject
	{
		public MarshalObject()
		{
			
		}
		
		public MarshalObject(int id, string uri)
		{
			this.id = id;
			this.uri = uri;
		}
		public int Id
		{
			get{return id;}
			set{id = value;}
		}
		public string Uri
		{
			get{return uri;}
		}
		
		public void Method1()
		{
			_called++;
			methodOneWay = RemotingServices.IsOneWay(MethodBase.GetCurrentMethod());			
		}
		
		public void Method2()
		{
			methodOneWay = RemotingServices.IsOneWay(MethodBase.GetCurrentMethod());			
		}
		
		public void Method2(int i)
		{
			methodOneWay = RemotingServices.IsOneWay(MethodBase.GetCurrentMethod());			
			
		}
		
		[OneWay()]
		public void Method3()
		{
			methodOneWay = RemotingServices.IsOneWay(MethodBase.GetCurrentMethod());			
			
		}
		
		public static int Called
		{
			get{return _called;}
		}
		
		public static bool IsMethodOneWay
		{
			get{return methodOneWay;}
		}
		
		
		private static int _called;
		private int id = 0;
		private string uri;
		private static bool methodOneWay = false;
	}
	
	// Another class used by the tests
	public class DerivedMarshalObject: MarshalObject
	{
		public DerivedMarshalObject(){}
		
		public DerivedMarshalObject(int id, string uri): base(id, uri) {}
	}
} // namespace MonoTests.System.Runtime.Remoting.RemotingServicesInternal

namespace MonoTests.System.Runtime.Remoting
{
	using MonoTests.System.Runtime.Remoting.RemotingServicesInternal;
	
	// The main test class
	[TestFixture]
	public class RemotingServicesTest 
	{
		private static int MarshalObjectId = 0;
			
		public RemotingServicesTest()
		{
			MarshalObjectId = 0;
		}
		
		// Helper function that create a new
		// MarshalObject with an unique ID
		private static MarshalObject NewMarshalObject()
		{
			string uri = "MonoTests.System.Runtime.Remoting.RemotingServicesTest.MarshalObject" + MarshalObjectId.ToString();
			MarshalObject objMarshal = new MarshalObject(MarshalObjectId, uri);
			
			MarshalObjectId++;
			
			return objMarshal;
		}
		
		// Another helper function
		private DerivedMarshalObject NewDerivedMarshalObject()
		{
			string uri = "MonoTests.System.Runtime.Remoting.RemotingServicesTest.DerivedMarshalObject" + MarshalObjectId.ToString();
			DerivedMarshalObject objMarshal = new DerivedMarshalObject(MarshalObjectId, uri);
			
			MarshalObjectId++;
			
			return objMarshal;
		}
		
		// The two folling method test RemotingServices.Marshal()
		[Test]
		public void Marshal1()
		{
			
			MarshalObject objMarshal = NewMarshalObject();
			ObjRef objRef = RemotingServices.Marshal(objMarshal);
			
			Assert.IsTrue(objRef.URI != null, "#A01");
			
			MarshalObject objRem = (MarshalObject) RemotingServices.Unmarshal(objRef);
			Assert.AreEqual(objMarshal.Id, objRem.Id, "#A02");
			
			objRem.Id = 2;
			Assert.AreEqual(objMarshal.Id, objRem.Id, "#A03");
			
			// TODO: uncomment when RemotingServices.Disconnect is implemented
			//RemotingServices.Disconnect(objMarshal);
			
			objMarshal = NewMarshalObject();
			
			objRef = RemotingServices.Marshal(objMarshal, objMarshal.Uri);
			
			Assert.IsTrue(objRef.URI.EndsWith(objMarshal.Uri), "#A04");
			// TODO: uncomment when RemotingServices.Disconnect is implemented
			//RemotingServices.Disconnect(objMarshal);		
		}
		
		[Test]
		public void Marshal2()
		{
			DerivedMarshalObject derivedObjMarshal = NewDerivedMarshalObject();
			
			ObjRef objRef = RemotingServices.Marshal(derivedObjMarshal, derivedObjMarshal.Uri, typeof(MarshalObject));
			
			// Check that the type of the marshaled object is MarshalObject
			Assert.IsTrue(objRef.TypeInfo.TypeName.StartsWith((typeof(MarshalObject)).ToString()), "#A05");
			
			// TODO: uncomment when RemotingServices.Disconnect is implemented
			//RemotingServices.Disconnect(derivedObjMarshal);
		}
		
		// Tests RemotingServices.GetObjectUri()
		[Test]
		public void GetObjectUri()
		{
			MarshalObject objMarshal = NewMarshalObject();
			
			Assert.IsTrue(RemotingServices.GetObjectUri(objMarshal) == null, "#A06");
			
			ObjRef objRef = RemotingServices.Marshal(objMarshal);
			
			Assert.IsTrue(RemotingServices.GetObjectUri(objMarshal) != null, "#A07");
			// TODO: uncomment when RemotingServices.Disconnect is implemented
			//RemotingServices.Disconnect(objMarshal);
		}
		
		// Tests RemotingServices.Connect
		[Test]
		public void Connect()
		{
			MarshalObject objMarshal = NewMarshalObject();
			
			IDictionary props = new Hashtable();
			props["name"] = objMarshal.Uri;
			props["port"] = 1236;
			TcpChannel chn = new TcpChannel(props, null, null);
			ChannelServices.RegisterChannel(chn);
			
			RemotingServices.Marshal(objMarshal,objMarshal.Uri);
			
			MarshalObject objRem = (MarshalObject) RemotingServices.Connect(typeof(MarshalObject), "tcp://localhost:1236/" + objMarshal.Uri);
			
			Assert.IsTrue(RemotingServices.IsTransparentProxy(objRem), "#A08");
			
			ChannelServices.UnregisterChannel(chn);
			
			// TODO: uncomment when RemotingServices.Disconnect is implemented
			//RemotingServices.Disconnect(objMarshal);
		}
		
		// Tests RemotingServices.Marshal()
		[Test]
		[ExpectedException(typeof(RemotingException))]	
		public void MarshalThrowException()
		{
			MarshalObject objMarshal = NewMarshalObject();
			
			IDictionary props = new Hashtable();
			props["name"] = objMarshal.Uri;
			props["port"] = 1237;
			TcpChannel chn = new TcpChannel(props, null, null);
			ChannelServices.RegisterChannel(chn);
			
			RemotingServices.Marshal(objMarshal,objMarshal.Uri);
			
			MarshalObject objRem = (MarshalObject) RemotingServices.Connect(typeof(MarshalObject), "tcp://localhost:1237/" + objMarshal.Uri);
			// This line sould throw a RemotingException
			// It is forbidden to export an object which is not
			// a real object
			try
			{
				RemotingServices.Marshal(objRem, objMarshal.Uri);
			}
			catch(Exception e)
			{
				ChannelServices.UnregisterChannel(chn);
			
			// TODO: uncomment when RemotingServices.Disconnect is implemented
			//RemotingServices.Disconnect(objMarshal);
			
				throw e;
			}		
		}
		
		// Tests RemotingServices.ExecuteMessage()
		// also tests GetMethodBaseFromMessage()
		// IsMethodOverloaded()
		[Test]
		public void ExecuteMessage()
		{
			TcpChannel chn = null;
			try
			{
				chn = new TcpChannel(1235);
				ChannelServices.RegisterChannel(chn);
				
				MarshalObject objMarshal = NewMarshalObject();
				RemotingConfiguration.RegisterWellKnownServiceType(typeof(MarshalObject), objMarshal.Uri, WellKnownObjectMode.SingleCall);
				
				// use a proxy to catch the Message
				MyProxy proxy = new MyProxy(typeof(MarshalObject), (MarshalObject) Activator.GetObject(typeof(MarshalObject), "tcp://localhost:1235/" + objMarshal.Uri));
				
				MarshalObject objRem = (MarshalObject) proxy.GetTransparentProxy();
				
				objRem.Method1();
				
				// Tests RemotingServices.GetMethodBaseFromMethodMessage()
				AssertEquals("#A09","Method1",proxy.MthBase.Name);
				Assert.IsTrue(!proxy.IsMethodOverloaded, "#A09.1");
				
				objRem.Method2();
				Assert.IsTrue(proxy.IsMethodOverloaded, "#A09.2");
			
				// Tests RemotingServices.ExecuteMessage();
				// If ExecuteMessage does it job well, Method1 should be called 2 times
				Assert.AreEqual(2, MarshalObject.Called, "#A10");
			}
			finally
			{
				if(chn != null) ChannelServices.UnregisterChannel(chn);
			}
		}
		
		// Tests the IsOneWay method
		[Test]
		public void IsOneWay()
		{
			TcpChannel chn = null;
			try
			{
				chn = new TcpChannel(1238);
				ChannelServices.RegisterChannel(chn);
				RemotingConfiguration.RegisterWellKnownServiceType(typeof(MarshalObject), "MarshalObject.rem", WellKnownObjectMode.Singleton);
				
				MarshalObject objRem = (MarshalObject) Activator.GetObject(typeof(MarshalObject), "tcp://localhost:1238/MarshalObject.rem");
				
				Assert.IsTrue(RemotingServices.IsTransparentProxy(objRem), "#A10.1");
				
				objRem.Method1();
				Thread.Sleep(20);
				Assert.IsTrue(!MarshalObject.IsMethodOneWay, "#A10.2");
				objRem.Method3();
				Thread.Sleep(20);
				Assert.IsTrue(MarshalObject.IsMethodOneWay, "#A10.2");
			}
			finally
			{
				if(chn != null) ChannelServices.UnregisterChannel(chn);
			}
		}
		
		[Test]
		public void GetObjRefForProxy()
		{
			TcpChannel chn = null;
			try
			{
				chn = new TcpChannel(1239);
				ChannelServices.RegisterChannel(chn);
				
				// Register le factory as a SAO
				RemotingConfiguration.RegisterWellKnownServiceType(typeof(MarshalObjectFactory), "MonoTests.System.Runtime.Remoting.RemotingServicesTest.Factory.soap", WellKnownObjectMode.Singleton);
				
				MarshalObjectFactory objFactory = (MarshalObjectFactory) Activator.GetObject(typeof(MarshalObjectFactory), "tcp://localhost:1239/MonoTests.System.Runtime.Remoting.RemotingServicesTest.Factory.soap");
				
				// Get a new "CAO"
				MarshalObject objRem = objFactory.GetNewMarshalObject();
				
				ObjRef objRefRem = RemotingServices.GetObjRefForProxy((MarshalByRefObject)objRem);
				
				Assert.IsTrue(objRefRem != null, "#A11");
			}
			finally
			{
				if(chn != null) ChannelServices.UnregisterChannel(chn);				
			}
		}
		
		// Tests GetRealProxy
		[Test]
		public void GetRealProxy()
		{
			TcpChannel chn = null;
			try
			{
				chn = new TcpChannel(1241);
				ChannelServices.RegisterChannel(chn);
				
				RemotingConfiguration.RegisterWellKnownServiceType(typeof(MarshalObject), "MonoTests.System.Runtime.Remoting.RemotingServicesTest.MarshalObject.soap", WellKnownObjectMode.Singleton);
				
				MyProxy proxy = new  MyProxy(typeof(MarshalObject), (MarshalByRefObject)Activator.GetObject(typeof(MarshalObject), "tcp://localhost:1241/MonoTests.System.Runtime.Remoting.RemotingServicesTest.MarshalObject.soap"));
				MarshalObject objRem = (MarshalObject) proxy.GetTransparentProxy();
				
				RealProxy rp = RemotingServices.GetRealProxy(objRem);
				
				Assert.IsTrue(rp != null, "#A12");
				Assert.AreEqual("MonoTests.System.Runtime.Remoting.RemotingServicesInternal.MyProxy", rp.GetType().ToString(), "#A13");
			}
			finally
			{
				if(chn != null) ChannelServices.UnregisterChannel(chn);
			}
		}
		
		// Tests SetObjectUriForMarshal()
		[Test]
		public void SetObjectUriForMarshal()
		{
			TcpChannel chn = null;
			try
			{
				chn = new TcpChannel(1242);
				ChannelServices.RegisterChannel(chn);
				
				MarshalObject objRem = NewMarshalObject();
				RemotingServices.SetObjectUriForMarshal(objRem, objRem.Uri);
				RemotingServices.Marshal(objRem);
				
				objRem = (MarshalObject) Activator.GetObject(typeof(MarshalObject), "tcp://localhost:1242/"+objRem.Uri);
				Assert.IsTrue(objRem != null, "#A14");
			}
			finally
			{
				if(chn != null) ChannelServices.UnregisterChannel(chn);
			}			
			
		}
		
		// Tests GetServeurTypeForUri()
		[Test]
		public void GetServeurTypeForUri()
		{
			TcpChannel chn = null;
			Type type = typeof(MarshalObject);
			try
			{
				chn = new TcpChannel(1243);
				ChannelServices.RegisterChannel(chn);
				
				MarshalObject objRem = NewMarshalObject();
				RemotingServices.SetObjectUriForMarshal(objRem, objRem.Uri);
				RemotingServices.Marshal(objRem);
				
				Type typeRem = RemotingServices.GetServerTypeForUri(RemotingServices.GetObjectUri(objRem));
				Assert.AreEqual(type, typeRem, "#A15");
			}
			finally
			{
				if(chn != null) ChannelServices.UnregisterChannel(chn);
			}			
		}
		
		// Tests IsObjectOutOfDomain
		// Tests IsObjectOutOfContext
		[Test]
		public void IsObjectOutOf()
		{
			TcpChannel chn = null;
			try
			{
				chn = new TcpChannel(1245);
				ChannelServices.RegisterChannel(chn);
				
				RemotingConfiguration.RegisterWellKnownServiceType(typeof(MarshalObject), "MarshalObject.rem", WellKnownObjectMode.Singleton);
				
				MarshalObject objRem = (MarshalObject) Activator.GetObject(typeof(MarshalObject), "tcp://localhost:1245/MarshalObject.rem");
				
				Assert.IsTrue(RemotingServices.IsObjectOutOfAppDomain(objRem), "#A16");
				Assert.IsTrue(RemotingServices.IsObjectOutOfContext(objRem), "#A17");
				
				MarshalObject objMarshal = new MarshalObject();
				Assert.IsTrue(!RemotingServices.IsObjectOutOfAppDomain(objMarshal), "#A18");
				Assert.IsTrue(!RemotingServices.IsObjectOutOfContext(objMarshal), "#A19");
			}
			finally
			{
				ChannelServices.UnregisterChannel(chn);
			}
		}
	} // end class RemotingServicesTest
} // end of namespace MonoTests.System.Runtime.Remoting.RemotingServicesTest
