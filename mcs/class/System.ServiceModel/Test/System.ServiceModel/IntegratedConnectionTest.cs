//
// IntegratedConnectionTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Threading;
using System.Xml;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel
{
	// FIXME: uncomment when NUnit tests on HTTP connection got as
	// stable as non-nunit samples.
	
	[TestFixture]
	public class IntegratedConnectionTest
	{
		[Test]
		[Ignore ("With Orcas it does not work fine")]
		// It is almost identical to samples/basic-http-binding
		public void SimpleHttpConnection ()
		{
			// Service
			ServiceHost host = new ServiceHost (typeof (Foo));
			int port = NetworkHelpers.FindFreePort ();
			try {
				Binding binding = new BasicHttpBinding ();
				binding.SendTimeout = binding.ReceiveTimeout = TimeSpan.FromSeconds (5);
				ServiceEndpoint se = host.AddServiceEndpoint ("MonoTests.System.ServiceModel.IFoo",
					binding, new Uri ("http://localhost:" + port));
				host.Open ();

				// Client
				ChannelFactory<IFoo> cf = new ChannelFactory<IFoo> (
					new BasicHttpBinding (),
					new EndpointAddress ("http://localhost:" + port + "/"));
				IFoo foo  = cf.CreateChannel ();
				Assert.AreEqual ("Test for EchoTest for Echo", foo.Echo ("Test for Echo"));
			} finally {
				if (host.State == CommunicationState.Opened)
					host.Close ();
			}
		}

		[Test]
		[Ignore ("With Orcas it does not work fine")]
		public void SimpleClientBase ()
		{
			// Service
			ServiceHost host = new ServiceHost (typeof (Foo2));
			int port = NetworkHelpers.FindFreePort ();
			try {
				Binding binding = new BasicHttpBinding ();
				binding.SendTimeout = binding.ReceiveTimeout = TimeSpan.FromSeconds (5);
				host.AddServiceEndpoint ("MonoTests.System.ServiceModel.IFoo2",
				binding, new Uri ("http://localhost:" + port));
				host.Open ();

				// Client
				Foo2Proxy proxy = new Foo2Proxy (
					new BasicHttpBinding (),
					new EndpointAddress ("http://localhost:" + port + "/"));
				proxy.Open ();
				try {
					Assert.AreEqual ("TEST FOR ECHOTEST FOR ECHO",
						proxy.Echo ("TEST FOR ECHO"));
				} finally {
					proxy.Close ();
				}
			} finally {
				// Service
				if (host.State == CommunicationState.Opened)
					host.Close ();
			}
		}

		[Test]
		[Ignore ("With Orcas it does not work fine")]
		public void ExchangeMetadata ()
		{
			// Service
			ServiceHost host = new ServiceHost (typeof (MetadataExchange));
			int port = NetworkHelpers.FindFreePort ();
			try {
				Binding binding = new BasicHttpBinding ();
				binding.ReceiveTimeout = TimeSpan.FromSeconds (5);
				host.AddServiceEndpoint ("IMetadataExchange",
				binding, new Uri ("http://localhost:" + port));
				host.Open ();
				// Client

				MetadataExchangeProxy proxy = new MetadataExchangeProxy (
					new BasicHttpBinding (),
					new EndpointAddress ("http://localhost:" + port + "/"));
				proxy.Open ();

				try {
					Message req = Message.CreateMessage (MessageVersion.Soap11, "http://schemas.xmlsoap.org/ws/2004/09/transfer/Get");
					Message res = proxy.Get (req);
				} finally {
					proxy.Close ();
				}
			} finally {
				// Service
				if (host.State == CommunicationState.Opened)
					host.Close ();
			}
		}

		[ServiceContract(SessionMode = SessionMode.Required)]
		interface IFoo3
		{
			[OperationContract]
			int GetInstanceCounter ();
		}

			[ServiceBehavior (InstanceContextMode = InstanceContextMode.PerSession)]
		class Foo3 : IFoo3, IDisposable
		{
			public static int CreatedInstances { get; set; }
			public static int DisposedInstances { get; set; }

			public static int InstanceCounter {
				get { return CreatedInstances - DisposedInstances; }
			}

			public Foo3 ()
			{
				CreatedInstances++;
			}

			public int GetInstanceCounter ()
			{
				return InstanceCounter;
			}

			public virtual void Dispose ()
			{
				DisposedInstances++;
			}

		}

		private void TestSessionbehaviour (Binding binding, Uri address)
		{
			ServiceHost host = new ServiceHost (typeof (Foo3), address);
			host.AddServiceEndpoint (typeof (IFoo3), binding, address);
			host.Description.Behaviors.Add (new ServiceThrottlingBehavior () { MaxConcurrentSessions = 1 });
			Foo3.CreatedInstances = Foo3.DisposedInstances = 0; // Reset
			host.Open ();
			Assert.AreEqual (0, Foo3.InstanceCounter, "Initial state wrong"); // just to be sure
			var cd = (ChannelDispatcher) host.ChannelDispatchers [0];
			for (int i = 1; i <= 3; ++i) {
				var factory = new ChannelFactory<IFoo3Client> (binding, address.ToString ());
				var proxy = factory.CreateChannel ();
				Assert.AreEqual (1, proxy.GetInstanceCounter (), "One server instance after first call in session #" + i);
				Assert.AreEqual (1, proxy.GetInstanceCounter (), "One server instance after second call in session #" + i);
				factory.Close (); // should close session even when no IsTerminating method has been invoked
				Thread.Sleep (500); // give WCF time to dispose service object
				Assert.AreEqual (0, Foo3.InstanceCounter, "Service instances must be disposed after channel is closed, in session #" + i);
				Assert.AreEqual (i, Foo3.CreatedInstances, "One new instance per session, in session #" + i);
			}
			host.Close ();
		}

		interface IFoo3Client : IFoo3, IClientChannel {}

		[Test (Description = "Tests InstanceContextMode.PerSession behavior for NetTcp binding")]
		public void TestSessionInstancesNetTcp ()
		{
			Binding binding = new NetTcpBinding (SecurityMode.None, false);
			Uri address = new Uri (binding.Scheme + "://localhost:" + NetworkHelpers.FindFreePort () + "/test");
			TestSessionbehaviour (binding, address);
		}

		[Test (Description = "Tests InstanceContextMode.PerSession behavior for WsHttp binding")]
		[Category ("NotWorking")] // no working WSHttpBinding in mono.
		public void TestSessionInstancesWsHttp ()
		{
			Binding binding = new WSHttpBinding (SecurityMode.None, true);
			Uri address = new Uri (binding.Scheme + "://localhost:" + NetworkHelpers.FindFreePort () + "/test");
			TestSessionbehaviour(binding, address);
		}
	}

	#region SimpleConnection classes

	[ServiceContract]
	public interface IFoo
	{
		[OperationContract]
		string Echo (string msg);
	}

	class Foo : IFoo
	{
		public string Echo (string msg)
		{
			return msg + msg;
		}
	}

	// This is manually created type for strongly typed request.
	[DataContract (Name = "Echo", Namespace = "http://tempuri.org/")]
	public class EchoType
	{
		public EchoType (string msg)
		{
			this.msg = msg;
		}

		[DataMember]
		public string msg = "test";
	}

	#endregion

	#region SampleClientBase classes

	[ServiceContract]
	public interface IFoo2
	{
		[OperationContract]
		string Echo (string msg);
	}

	class Foo2 : IFoo2
	{
		public string Echo (string msg)
		{
			return msg + msg;
		}
	}

	public class Foo2Proxy : ClientBase<IFoo2>, IFoo2
	{
		public Foo2Proxy (Binding binding, EndpointAddress address)
			: base (binding, address)
		{
		}

		public string Echo (string msg)
		{
			return Channel.Echo (msg);
		}
	}

	#endregion

	#region ExchangeMetadata classes

	class MetadataExchange : IMetadataExchange
	{
		public Message Get (Message request)
		{
			XmlDocument doc = new XmlDocument ();
			doc.AppendChild (doc.CreateElement ("Metadata", "http://schemas.xmlsoap.org/ws/2004/09/mex"));
			return Message.CreateMessage (request.Version,
				"http://schemas.xmlsoap.org/ws/2004/09/transfer/GetResponse",
				new XmlNodeReader (doc));
		}

		public IAsyncResult BeginGet (Message request, AsyncCallback cb, object state)
		{
			throw new NotImplementedException ();
		}

		public Message EndGet (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}
	}

	public class MetadataExchangeProxy : ClientBase<IMetadataExchange>, IMetadataExchange
	{
		public MetadataExchangeProxy (Binding binding, EndpointAddress address)
			: base (binding, address)
		{
		}

		public Message Get (Message request)
		{
			return Channel.Get (request);
		}

		public IAsyncResult BeginGet (Message request, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		public Message EndGet (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}
	}

	#endregion
}
#endif
