//
// ClientBaseTest.cs
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
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class ClientBaseTest
	{
/*
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void GenericTypeArgumentIsServiceContract ()
		{
			new MyClientBase<ICloneable> (new BasicHttpBinding (), new EndpointAddress ("http://localhost:4126"));
		}
*/

/*
		public class MyClientBase<T> : ClientBase<T>
		{
			public MyClientBase (Binding binding, EndpointAddress address)
				: base (binding, address)
			{
			}
		}

		public class MyClientBase1 : ClientBase<TestService>
		{
			public MyClientBase1 (Binding binding, EndpointAddress address)
				: base (binding, address)
			{
			}
		}

		[Test]
		public void ClassTypeArg ()
		{
			new MyClientBase1 (new BasicHttpBinding (), new EndpointAddress ("urn:dummy"));
		}
*/

		[ServiceContract]
		public interface ITestService
		{
			[OperationContract]
			string Foo (string arg);
		}

		public class TestService : ITestService
		{
			public string Foo (string arg)
			{
				return arg;
			}
		}

		[ServiceContract]
		public interface ITestService2
		{
			[OperationContract]
			void Bar (string arg);
		}

		public class TestService2 : ITestService2
		{
			public void Bar (string arg)
			{
			}
		}

		[Test]
		[Ignore ("hmm, this test shows that MSDN documentation does not match the fact.")]
		public void Foo ()
		{
			Type t = typeof (ClientBase<ITestService>).GetGenericTypeDefinition ().GetGenericArguments () [0];
			Assert.IsTrue (t.IsGenericParameter);
			Assert.AreEqual (GenericParameterAttributes.None, t.GenericParameterAttributes);
		}

		class MyChannelFactory<T> : ChannelFactory<T>
		{
			public MyChannelFactory (Binding b, EndpointAddress e)
				: base (b, e)
			{
			}

			public IChannelFactory GimmeFactory ()
			{
				return CreateFactory ();
			}
		}

		#region UseCase1

		ServiceHost host;

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ClientBaseCtorArgsTest1 ()
		{
			new CtorUseCase1 (null, new BasicHttpBinding (), new EndpointAddress ("http://test"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ClientBaseCtorArgsTest2 ()
		{
			new CtorUseCase1 (null, new EndpointAddress ("http://test"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ClientBaseCtorArgsTest3 ()
		{
			new CtorUseCase1 (null, "http://test");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ClientBaseCtorArgsTest4 ()
		{
			new CtorUseCase1 ("CtorUseCase1_1", null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ClientBaseCtorArgsTest5 ()
		{
			new CtorUseCase1 (new BasicHttpBinding (), null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ClientBaseCtorArgsTest6 ()
		{
			new CtorUseCase1 ("CtorUseCase1_Incorrect");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ClientBaseCtorArgsTest7 ()
		{
			new CtorUseCase3 ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ClientBaseCtorConfigAmbiguityTest ()
		{
			new CtorUseCase2 ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ClientBaseCtorConfigAmbiguityTest2 ()
		{
			new CtorUseCase2 ("*");
		}

		[Test]
		[Ignore ("fails under .NET; I never bothered to fix the test")]
		public void ClientBaseConfigEmptyCtor ()
		{
			new CtorUseCase1 ();
		}

		[Test]
		[Ignore ("fails under .NET; I never bothered to fix the test")]
		public void ClientBaseConfigCtor ()
		{
			new CtorUseCase1 ("CtorUseCase1_1");
		}

		[Test]
		[Ignore ("fails under .NET; I never bothered to fix the test")]
		public void ClientBaseConfigCtorUsingDefault ()
		{
			new CtorUseCase1 ("*");
		}

		[Test]
		[Ignore ("With Orcas it does not work fine")]
		public void UseCase1Test ()
		{
			// almost equivalent to samples/clientbase/samplesvc.cs
			using (host = new ServiceHost (typeof (UseCase1))) {
				Binding binding = new BasicHttpBinding ();
				binding.ReceiveTimeout = TimeSpan.FromSeconds (15);
				host.AddServiceEndpoint (typeof (IUseCase1).FullName, binding, new Uri ("http://localhost:37564"));

				host.Open ();
				// almost equivalent to samples/clientbase/samplecli.cs
				using (UseCase1Proxy proxy = new UseCase1Proxy (
					new BasicHttpBinding (),
					new EndpointAddress ("http://localhost:37564"))) {
					proxy.Open ();
					Assert.AreEqual ("TEST FOR ECHOTEST FOR ECHO", proxy.Echo ("TEST FOR ECHO"));
				}
			}
			EnsurePortNonBlocking (37564);
		}

		void EnsurePortNonBlocking (int port)
		{
			TcpListener l = new TcpListener (port);
			l.ExclusiveAddressUse = true;
			l.Start ();
			l.Stop ();
		}

		[ServiceContract]
		public interface IUseCase1
		{
			[OperationContract]
			string Echo (string msg);
		}

		public class UseCase1 : IUseCase1
		{
			public string Echo (string msg)
			{
				return msg + msg;
			}
		}

		public class UseCase1Proxy : ClientBase<IUseCase1>, IUseCase1
		{
			public UseCase1Proxy (Binding binding, EndpointAddress address)
				: base (binding, address)
			{
			}

			public string Echo (string msg)
			{
				return Channel.Echo (msg);
			}
		}

		public class CtorUseCase1 : ClientBase<ICtorUseCase1>, ICtorUseCase1
		{
			public CtorUseCase1 ()
				: base ()
			{
			}

			public CtorUseCase1 (string configName)
				: base (configName)
			{
			}

			public CtorUseCase1 (string configName, string address)
				: base (configName, address)
			{
			}

			public CtorUseCase1 (Binding binding, EndpointAddress address)
				: base (binding, address)
			{
			}

			public CtorUseCase1 (InstanceContext i, Binding binding, EndpointAddress address)
				: base (i, binding, address)
			{
			}

			public string Echo (string msg)
			{
				return Channel.Echo (msg);
			}
		}

		public class CtorUseCase2 : ClientBase<ICtorUseCase2>, ICtorUseCase2
		{
			public CtorUseCase2 ()
				: base ()
			{
			}

			public CtorUseCase2 (string configName)
				: base (configName)
			{
			}

			public CtorUseCase2 (string configName, string address)
				: base (configName, address)
			{
			}

			public CtorUseCase2 (Binding binding, EndpointAddress address)
				: base (binding, address)
			{
			}

			public CtorUseCase2 (InstanceContext i, Binding binding, EndpointAddress address)
				: base (i, binding, address)
			{
			}

			public string Echo (string msg)
			{
				return Channel.Echo (msg);
			}
		}

		public class CtorUseCase3 : ClientBase<ICtorUseCase3>, ICtorUseCase3
		{
			public string Echo (string msg)
			{
				return Channel.Echo (msg);
			}
		}

		#endregion

		// For contract that directly receives and sends Message instances.
		#region UseCase2
		[Test]
		[Ignore ("With Orcas it does not work fine")]
		public void UseCase2Test ()
		{
			// almost equivalent to samples/clientbase/samplesvc2.cs
			ServiceHost host = new ServiceHost (typeof (UseCase2));
			Binding binding = new BasicHttpBinding ();
			binding.ReceiveTimeout = TimeSpan.FromSeconds (15);
			host.AddServiceEndpoint (typeof (IUseCase2).FullName,
			binding, new Uri ("http://localhost:37564"));

			try {
				host.Open ();
				// almost equivalent to samples/clientbase/samplecli2.cs
				Binding b = new BasicHttpBinding ();
				b.SendTimeout = TimeSpan.FromSeconds (15);
				b.ReceiveTimeout = TimeSpan.FromSeconds (15);
				UseCase2Proxy proxy = new UseCase2Proxy (
					b,
					new EndpointAddress ("http://localhost:37564/"));
				proxy.Open ();
				Message req = Message.CreateMessage (MessageVersion.Soap11, "http://tempuri.org/IUseCase2/Echo");
				Message res = proxy.Echo (req);
				using (XmlWriter w = XmlWriter.Create (TextWriter.Null)) {
					res.WriteMessage (w);
				}
			} finally {
				if (host.State == CommunicationState.Opened)
					host.Close ();
				EnsurePortNonBlocking (37564);
			}
		}

		[ServiceContract]
		public interface IUseCase2
		{
			[OperationContract]
			Message Echo (Message request);
		}

		class UseCase2 : IUseCase2
		{
			public Message Echo (Message request)
			{
				Message msg = Message.CreateMessage (request.Version, request.Headers.Action + "Response");
				msg.Headers.Add (MessageHeader.CreateHeader ("hoge", "urn:hoge", "heh"));
				//msg.Headers.Add (MessageHeader.CreateHeader ("test", "http://schemas.microsoft.com/ws/2005/05/addressing/none", "testing"));
				return msg;
			}
		}

		public class UseCase2Proxy : ClientBase<IUseCase2>, IUseCase2
		{
			public UseCase2Proxy (Binding binding, EndpointAddress address)
				: base (binding, address)
			{
			}

			public Message Echo (Message request)
			{
				return Channel.Echo (request);
			}
		}

		#endregion

		[Test]
		[Ignore ("With Orcas it does not work fine")]
		public void UseCase3 ()
		{
			// almost equivalent to samples/clientbase/samplesvc3.cs
			ServiceHost host = new ServiceHost (typeof (MetadataExchange));
			host.Description.Behaviors.Find<ServiceDebugBehavior> ()
				.IncludeExceptionDetailInFaults = true;
			Binding bs = new BasicHttpBinding ();
			bs.SendTimeout = TimeSpan.FromSeconds (5);
			bs.ReceiveTimeout = TimeSpan.FromSeconds (5);
			// magic name that does not require fully qualified name ...
			host.AddServiceEndpoint ("IMetadataExchange",
			        bs, new Uri ("http://localhost:37564"));
			try {
				host.Open ();
				// almost equivalent to samples/clientbase/samplecli3.cs
				Binding bc = new BasicHttpBinding ();
				bc.SendTimeout = TimeSpan.FromSeconds (5);
				bc.ReceiveTimeout = TimeSpan.FromSeconds (5);
				MetadataExchangeProxy proxy = new MetadataExchangeProxy (
					bc,
					new EndpointAddress ("http://localhost:37564/"));
				proxy.Open ();

				Message req = Message.CreateMessage (MessageVersion.Soap11, "http://schemas.xmlsoap.org/ws/2004/09/transfer/Get");
				Message res = proxy.Get (req);
				using (XmlWriter w = XmlWriter.Create (TextWriter.Null)) {
					res.WriteMessage (w);
				}
			} finally {
				if (host.State == CommunicationState.Opened)
					host.Close ();
				EnsurePortNonBlocking (37564);
			}
		}

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

#if NET_4_0
		[Test]
		public void ConstructorServiceEndpoint ()
		{
			// It is okay to pass ServiceEndpoint that does not have Binding or EndpointAddress.
			new MyClient (new ServiceEndpoint (ContractDescription.GetContract (typeof (IMetadataExchange)), null, null));
			try {
				new MyClient ((Binding) null, (EndpointAddress) null);
				Assert.Fail ("ArgumentNullException is expected");
			} catch (ArgumentNullException) {
			}
		}

		class MyClient : ClientBase<IMetadataExchange>
		{
			public MyClient (ServiceEndpoint endpoint)
				: base (endpoint)
			{
			}
			
			public MyClient (Binding binding, EndpointAddress address)
				: base (binding, address)
			{
			}
		}
#endif
	}
}
