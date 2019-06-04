//
// ServiceHostTest.cs
//
// Author:
//	Ankit Jain  <jankit@novell.com>
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2005-2006 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class ServiceHostTest
	{
		class MyHost : ServiceHost
		{
			public MyHost (Type type, Uri uri)
				: base (type, uri)
			{
			}

			public IDictionary<string,ContractDescription> ExposedContracts {
				get { return ImplementedContracts; }
			}
		}

		[Test]
		public void Ctor ()
		{
			MyHost host = new MyHost (typeof (Foo), new Uri ("http://localhost"));
			Assert.IsNotNull (host.Description, "#1");
			Assert.AreEqual (typeof (Foo), host.Description.ServiceType, "#1-2");
			Assert.IsNotNull (host.BaseAddresses, "#2");
			Assert.AreEqual (1, host.BaseAddresses.Count, "#3");

			Assert.IsNotNull (host.ChannelDispatchers, "#4");
			Assert.AreEqual (0, host.ChannelDispatchers.Count, "#5");
			Assert.IsNotNull (host.Authorization, "#6");
			Assert.IsNotNull (host.ExposedContracts, "#7");
			// Foo is already in the contracts.
			Assert.AreEqual (1, host.ExposedContracts.Count, "#8");
			// this loop iterates only once.
			foreach (KeyValuePair<string,ContractDescription> e in host.ExposedContracts) {
				// hmm... so, seems like the key is just the full name of the contract type.
				Assert.AreEqual ("MonoTests.System.ServiceModel.ServiceHostTest+Foo", e.Key, "#9");
				ContractDescription cd = e.Value;
				Assert.AreEqual ("Foo", cd.Name, "#10");
				Assert.AreEqual ("http://tempuri.org/", cd.Namespace, "#11");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CtorNull ()
		{
			new ServiceHost (typeof (Foo), null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CtorServiceTypeNotClass ()
		{
			new ServiceHost (typeof (IBar), new Uri ("http://localhost"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CtorRelativeBaseAddress ()
		{
			new ServiceHost (typeof (Foo), new Uri ("test", UriKind.Relative));
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CtorMultipleAddressPerScheme ()
		{
			new ServiceHost ( typeof (Foo), 
					new Uri ("http://localhost", UriKind.Absolute),
					new Uri ("http://someotherhost", UriKind.Absolute));
		}

		[Test]
		public void AddServiceEndpoint ()
		{
			ServiceHost host = new ServiceHost (typeof (Foo), new Uri ("http://localhost/echo"));
			host.AddServiceEndpoint (typeof (Foo), new BasicHttpBinding (), "rel");
			host.AddServiceEndpoint (typeof (Foo), new BasicHttpBinding (), "svc");

			Assert.IsNotNull (host.Description, "#6");
			Assert.IsNotNull (host.Description.Endpoints, "#7");
			Assert.AreEqual (host.Description.Endpoints.Count, 2, "#8");
			Assert.AreEqual ("http://localhost/echo/rel", host.Description.Endpoints [0].Address.Uri.AbsoluteUri,  "#9");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AddServiceEndpoint1 ()
		{
			ServiceHost host = new ServiceHost (typeof (Foo), new Uri ("ftp://localhost/echo"));
			// ftp does not match BasicHttpBinding
			host.AddServiceEndpoint (typeof (Foo), new BasicHttpBinding (), "rel");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AddServiceEndpoint2 ()
		{
			ServiceHost host = new ServiceHost (typeof (Foo), new Uri ("http://localhost/echo"));
			host.AddServiceEndpoint (typeof (Foo), new BasicHttpBinding (), "rel");
			host.AddServiceEndpoint (typeof (Foo), new BasicHttpBinding (), "rel"); // duplicate URI

			host.Open ();
			host.Close (); // should not reach here. It is to make sure to close unexpectedly opened host.
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AddServiceEndpoint2_2 ()
		{
			ServiceHost host = new ServiceHost (typeof (Foo), new Uri ("http://localhost/echo"));
			// same as above, but through Endpoints.Add()
			host.Description.Endpoints.Add (new ServiceEndpoint (ContractDescription.GetContract (typeof (Foo)), new BasicHttpBinding (), new EndpointAddress ("http://localhost/echo/rel")));
			host.Description.Endpoints.Add (new ServiceEndpoint (ContractDescription.GetContract (typeof (Foo)), new BasicHttpBinding (), new EndpointAddress ("http://localhost/echo/rel")));

			host.Open ();
			host.Close (); // should not reach here. It is to make sure to close unexpectedly opened host.
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AddServiceEndpoint2_3 ()
		{
			ServiceHost host = new ServiceHost (typeof (HogeFuga), new Uri ("http://localhost/echo"));
			host.Description.Endpoints.Add (new ServiceEndpoint (ContractDescription.GetContract (typeof (IHoge)), new BasicHttpBinding (), new EndpointAddress ("http://localhost/echo")));
			host.Description.Endpoints.Add (new ServiceEndpoint (ContractDescription.GetContract (typeof (IFuga)), new BasicHttpBinding (), new EndpointAddress ("http://localhost/echo")));

			// Different contracts unlike previous two cases.
			// If two or more endpoints are bound to the same listen
			// URI, then they must share the same instance.

			host.Open ();
			host.Close (); // should not reach here. It is to make sure to close unexpectedly opened host.
		}

		[Test]
		public void AddServiceEndpoint2_4 ()
		{
			var ep = "http://" + NetworkHelpers.LocalEphemeralEndPoint().ToString();
			ServiceHost host = new ServiceHost (typeof (HogeFuga), new Uri (ep));
			var binding = new BasicHttpBinding ();
			host.AddServiceEndpoint (typeof (IHoge), binding, new Uri (ep));
			host.AddServiceEndpoint (typeof (IFuga), binding, new Uri (ep));

			// Use the same binding, results in one ChannelDispatcher (actually two, for metadata/debug behavior).
			host.Open ();
			try {
				Assert.AreEqual (2, host.ChannelDispatchers.Count, "#1");
				foreach (ChannelDispatcher cd in host.ChannelDispatchers) {
					if (cd.BindingName != binding.Name)
						continue; // mex
					Assert.AreEqual (2, cd.Endpoints.Count, "#2");
				}
			} finally {
				host.Close ();
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AddServiceEndpoint3 ()
		{
			ServiceHost host = new ServiceHost (typeof (Foo), new Uri ("http://localhost/echo"));
			host.AddServiceEndpoint (typeof (Foo), new BasicHttpBinding (), "rel");
			host.AddServiceEndpoint (typeof (Foo), new BasicHttpBinding (), "http://localhost/echo/rel"); // duplicate URI when resolved

			host.Open ();
			host.Close (); // should not reach here. It is to make sure to close unexpectedly opened host.
		}

		[Test]
		public void Open ()
		{
			ServiceHost host = new ServiceHost (typeof (ZeroOperationsImpl));
			host.AddServiceEndpoint (typeof (IHaveZeroOperarationsContract), new BasicHttpBinding (), "http://localhost/echo");

			try {
				host.Open ();
				Assert.Fail ("InvalidOperationException expected");
			} 
			catch (InvalidOperationException e) {
				//"ContractDescription 'IHaveZeroOperarationsContract' has zero operations; a contract must have at least one operation."
				StringAssert.Contains ("IHaveZeroOperarationsContract", e.Message);
			}
			finally {
				if (host.State == CommunicationState.Opened)
					host.Close (); // It is to make sure to close unexpectedly opened host if the test fail.
			}
		}

		[Test]
		public void AddServiceEndpoint4 ()
		{
			ServiceHost host = new ServiceHost (typeof (Baz), new Uri ("http://localhost/echo"));
			host.AddServiceEndpoint ("MonoTests.System.ServiceModel.ServiceHostTest+IBaz", new BasicHttpBinding (), "rel");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AddServiceEndpoint5 ()
		{
			ServiceHost host = new ServiceHost (typeof (Baz), new Uri ("http://localhost/echo"));

			// Full type name is expected here (see AddServiceEndpoint4).
			host.AddServiceEndpoint ("IBaz", new BasicHttpBinding (), "rel");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AddServiceEndpoint6 ()
		{
			ServiceHost host = new ServiceHost (typeof (Foo), new Uri ("http://localhost/echo"));
			host.AddServiceEndpoint ("ISuchTypeDoesNotExist", new BasicHttpBinding (), "rel");
		}

		[Test]
		public void AddServiceEndpoint7 ()
		{
			ServiceHost host = new ServiceHost (typeof (Foo), new Uri ("http://localhost/echo"));
			var a = host.AddServiceEndpoint (typeof (Foo), new BasicHttpBinding (), "a");
			Console.WriteLine (a.Address);
			Assert.AreEqual ("http", a.Address.Uri.Scheme, "#1");
			Assert.AreEqual ("http://localhost/echo/a", a.Address.Uri.AbsoluteUri, "#2");

			var b = host.AddServiceEndpoint (typeof (Foo), new BasicHttpBinding (), "/b");
			Console.WriteLine (b.Address);
			Assert.AreEqual ("http", b.Address.Uri.Scheme, "#3");
			Assert.AreEqual ("http://localhost/echo/b", b.Address.Uri.AbsoluteUri, "#4");
		}
		
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AddServiceEndpointMexWithNoImpl ()
		{
			var port = NetworkHelpers.FindFreePort ();
			using (ServiceHost h = new ServiceHost (typeof (Foo), new Uri ("http://localhost:" + port))) {
				// it expects ServiceMetadataBehavior
				h.AddServiceEndpoint (ServiceMetadataBehavior.MexContractName, MetadataExchangeBindings.CreateMexHttpBinding (), "mex");
			}
		}

		[Test]
		public void AddServiceEndpointMetadataExchange ()
		{
			var port = NetworkHelpers.FindFreePort ();
			// MyMetadataExchange implements IMetadataExchange
			ServiceHost host = new ServiceHost (typeof (MyMetadataExchange));
			host.AddServiceEndpoint ("IMetadataExchange",
						 new BasicHttpBinding (),
						 "http://localhost:" + port + "/");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AddServiceEndpointMetadataExchangeFullNameFails ()
		{
			var port = NetworkHelpers.FindFreePort ();
			ServiceHost host = new ServiceHost (typeof (MyMetadataExchange));
			host.AddServiceEndpoint ("System.ServiceModel.Description.IMetadataExchange",
						 new BasicHttpBinding (),
						 "http://localhost:" + port);
		}

		[Test]
		public void InstanceWithNonSingletonMode ()
		{
			var ep = NetworkHelpers.LocalEphemeralEndPoint().ToString();
			ServiceHost host = new ServiceHost (
				new NonSingletonService ());
			Assert.IsNotNull (host.Description.Behaviors.Find<ServiceBehaviorAttribute> ().GetWellKnownSingleton (), "premise1");
			host.AddServiceEndpoint (
				typeof (NonSingletonService),
				new BasicHttpBinding (),
				new Uri ("http://" + ep + "/s1"));

			// in case Open() didn't fail, we need to close the host.
			// And even if Close() caused the expected exception,
			// the test should still fail.
			try {
				host.Open ();
				try {
					if (host.State == CommunicationState.Opened)
						host.Close ();
				} catch (InvalidOperationException) {
				}
				Assert.Fail ("InstanceContextMode was not checked");
			} catch (InvalidOperationException) {
			}
		}


		[Test]
		public void InstanceWithSingletonMode ()
		{
            var ep = NetworkHelpers.LocalEphemeralEndPoint().ToString();
			SingletonService instance = new SingletonService ();
			ServiceHost host = new ServiceHost (instance);
			Assert.IsNotNull (host.Description.Behaviors.Find<ServiceBehaviorAttribute> ().GetWellKnownSingleton (), "#1");
			host.AddServiceEndpoint (
				typeof (SingletonService),
				new BasicHttpBinding (),
				new Uri ("http://" + ep + "/s2"));

			// in case Open() didn't fail, we need to close the host.
			// And even if Close() caused the expected exception,
			// the test should still fail.
			try {
				host.Open ();
				ChannelDispatcher cd = (ChannelDispatcher) host.ChannelDispatchers [0];
				DispatchRuntime dr = cd.Endpoints [0].DispatchRuntime;
				Assert.IsNotNull (dr.InstanceContextProvider, "#2");
				InstanceContext ctx = dr.InstanceContextProvider.GetExistingInstanceContext (null, null);
				Assert.IsNotNull (ctx, "#3");
				Assert.AreEqual (instance, ctx.GetServiceInstance (), "#4");
			} finally {
				if (host.State == CommunicationState.Opened)
					host.Close ();
			}
		}

		[Test]
		public void InstanceWithSingletonMode_InheritServiceBehavior ()
		{
			// # 37035

			var ep = NetworkHelpers.LocalEphemeralEndPoint ().ToString ();

			ChildSingletonService instance = new ChildSingletonService ();
			ServiceHost host = new ServiceHost (instance);

			host.AddServiceEndpoint (typeof (SingletonService),
						 new BasicHttpBinding (),
						 new Uri ("http://" + ep + "/s3"));

			try {
				host.Open ();
			} catch (InvalidOperationException ex) {
				Assert.Fail ("InstanceContextMode was not inherited from parent, exception was: {0}", ex);
			} finally {
				host.Close ();
			}
		}

		[ServiceContract]
		interface IBar
		{
		}

		[ServiceContract]
		class Foo
		{
			[OperationContract]
			public void SayWhat () { }
		}

		[ServiceContract]
		interface IBaz
		{
			[OperationContract]
			string Echo (string source);
		}
		
		[ServiceContract]
		interface IHoge
		{
			[OperationContract]
			void DoX ();
		}

		[ServiceContract]
		interface IFuga
		{
			[OperationContract]
			void DoY ();
		}

		[ServiceContract]
		interface IHaveZeroOperarationsContract
		{
			string Echo (string source);
		}

		class ZeroOperationsImpl : IHaveZeroOperarationsContract
		{
			public string Echo(string source)
			{
				return null;
			}
		}

		class HogeFuga : IHoge, IFuga
		{
			public void DoX () {}
			public void DoY () {}
		}

		class Baz : IBaz
		{
			public string Echo (string source)
			{
				return source;
			}
		}

		class MyMetadataExchange : IMetadataExchange
		{
			public Message Get (Message req)
			{
				throw new NotImplementedException ();
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

		[ServiceContract]
		public class NonSingletonService
		{
			[OperationContract]
			public void Process (string input)
			{
			}
		}

		[ServiceContract]
		[ServiceBehavior (InstanceContextMode = InstanceContextMode.Single)]
		public class SingletonService
		{
			[OperationContract]
			public virtual void Process (string input)
			{
			}
		}

		public class ChildSingletonService : SingletonService
		{
			public override void Process (string input)
			{
			}
		}
	}
}
#endif
