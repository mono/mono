//
// MetadataResolverTest.cs
//
// Author:
//	Ankit Jain <JAnkit@novell.com>
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using System.ServiceModel;

using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel.Description
{
	[TestFixture]
	public class MetadataResolverTest
	{
		string url;
		//string url = "http://192.168.0.1:8080/echo/mex";

		static HttpListener listener;
		IAsyncResult current_request;
		int remaining, port;

		static readonly string mex = File.ReadAllText (TestResourceHelper.GetFullPathOfResource ("Test/Resources/dump.xml"));

		[SetUp]
		public void StartupServer ()
		{
			if (listener != null)
				listener.Stop ();
			listener = new HttpListener ();
			port = NetworkHelpers.FindFreePort ();
			url = "http://localhost:" + port + "/echo/mex";
			listener.Prefixes.Add ("http://*:" + port + "/echo/");
			listener.Start ();
			current_request = listener.BeginGetContext (OnReceivedRequest, null);
			remaining = 1;
		}
		
		void OnReceivedRequest (IAsyncResult result)
		{
			try {
				var ctx = listener.EndGetContext (result);
				current_request = null;
				ctx.Response.ContentType = "application/soap+xml";
				ctx.Response.ContentLength64 = mex.Length;
				using (var sw = new StreamWriter (ctx.Response.OutputStream))
					sw.Write (mex);
				ctx.Response.Close ();
				if (--remaining > 0)
					current_request = listener.BeginGetContext (OnReceivedRequest, null);
			} catch (Exception ex) {
				// ignore server errors in this test.
			}
		}
		
		[TearDown]
		public void ShutdownServer ()
		{
			listener.Stop ();
			listener = null;
		}

		[Test]
		[Category ("NotWorking")]
		public void ResolveNoEndpoint ()
		{
			ServiceEndpointCollection endpoints = MetadataResolver.Resolve (
				typeof (NonExistantContract),
				new EndpointAddress (url));

			Assert.IsNotNull (endpoints);
			Assert.AreEqual (0, endpoints.Count);
		}

		[Test]
		[Category ("NotWorking")]
		public void Resolve1 ()
		{
			ServiceEndpointCollection endpoints = MetadataResolver.Resolve (
				typeof (IEchoService), new EndpointAddress (url));

			CheckIEchoServiceEndpoint (endpoints);
		}

		[Test]
		[Category ("NotWorking")]
		public void Resolve2 ()
		{
			ServiceEndpointCollection endpoints = MetadataResolver.Resolve (
				typeof (IEchoService),
				new Uri (url),
				MetadataExchangeClientMode.MetadataExchange);

			CheckIEchoServiceEndpoint (endpoints);
		}

		[Test]
		[Category ("NotWorking")]
		public void Resolve3 ()
		{
			ContractDescription contract = ContractDescription.GetContract (typeof (IEchoService));
			List<ContractDescription> contracts = new List<ContractDescription> ();
			contracts.Add (contract);

			ServiceEndpointCollection endpoints = MetadataResolver.Resolve (
				contracts,
				new Uri (url),
				MetadataExchangeClientMode.MetadataExchange);

			CheckIEchoServiceEndpoint (endpoints);
		}

		[Test]
		[Category ("NotWorking")]
		public void Resolve4 ()
		{
			ContractDescription contract = ContractDescription.GetContract (typeof (IEchoService));
			List<ContractDescription> contracts = new List<ContractDescription> ();
			contracts.Add (contract);
			contracts.Add (ContractDescription.GetContract (typeof (NonExistantContract)));

			ServiceEndpointCollection endpoints = MetadataResolver.Resolve (
				contracts,
				new Uri (url),
				MetadataExchangeClientMode.MetadataExchange);

			CheckIEchoServiceEndpoint (endpoints);
		}

		[Test]
		[Category ("NotWorking")]
		public void Resolve5 ()
		{
			ContractDescription contract = ContractDescription.GetContract (typeof (IEchoService));
			List<ContractDescription> contracts = new List<ContractDescription> ();
			contracts.Add (contract);
			contracts.Add (ContractDescription.GetContract (typeof (NonExistantContract)));

			//FIXME: What is the 'client' param used for?
			//TODO: Write test cases for the related overloads of Resolve
			MetadataResolver.Resolve (
				contracts,
				new EndpointAddress (url),
				new MetadataExchangeClient (new EndpointAddress ("http://localhost")));
		}

		[Test]
		[Category ("NotWorking")]
		public void Resolve6 ()
		{
			ContractDescription contract = ContractDescription.GetContract (typeof (IEchoService));
			List<ContractDescription> contracts = new List<ContractDescription> ();
			contracts.Add (contract);
			contracts.Add (ContractDescription.GetContract (typeof (NonExistantContract)));

			//FIXME: What is the 'client' param used for?
			//TODO: Write test cases for the related overloads of Resolve
			MetadataResolver.Resolve (
				contracts,
				new Uri (url),
				MetadataExchangeClientMode.MetadataExchange,
				new MetadataExchangeClient (new EndpointAddress ("http://localhost")));
		}


		//Negative tests

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ErrResolve1 ()
		{
			MetadataResolver.Resolve (
				typeof (IEchoService),
				null,
				MetadataExchangeClientMode.MetadataExchange);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		[Ignore ("does not fail on .NET either")]
		public void ErrResolve2 ()
		{
			//Mex cannot be fetched with HttpGet from the given url
			MetadataResolver.Resolve (
				typeof (IEchoService),
				new Uri (url),
				MetadataExchangeClientMode.HttpGet);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ErrResolve3 ()
		{
			ContractDescription contract = ContractDescription.GetContract (typeof (IEchoService));
			List<ContractDescription> contracts = new List<ContractDescription> ();
			contracts.Add (contract);
			contracts.Add (ContractDescription.GetContract (typeof (NonExistantContract)));

			MetadataResolver.Resolve (contracts, new EndpointAddress (url),
				(MetadataExchangeClient) null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ErrResolve4 ()
		{
			ContractDescription contract = ContractDescription.GetContract (typeof (IEchoService));
			List<ContractDescription> contracts = new List<ContractDescription> ();
			contracts.Add (contract);
			contracts.Add (ContractDescription.GetContract (typeof (NonExistantContract)));

			//FIXME: What is the 'client' param used for?
			//TODO: Write test cases for the related overloads of Resolve
			MetadataResolver.Resolve (
				contracts,
				new EndpointAddress ("http://localhost"),
				new MetadataExchangeClient (new EndpointAddress (url)));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		[Ignore ("does not fail on .NET either")]
		public void ErrResolve5 ()
		{
			ContractDescription contract = ContractDescription.GetContract (typeof (IEchoService));
			List<ContractDescription> contracts = new List<ContractDescription> ();
			contracts.Add (contract);
			contracts.Add (ContractDescription.GetContract (typeof (NonExistantContract)));

			//FIXME: What is the 'client' param used for?
			//TODO: Write test cases for the related overloads of Resolve
			MetadataResolver.Resolve (
				contracts,
				new Uri (url),
				MetadataExchangeClientMode.HttpGet,
				new MetadataExchangeClient (new EndpointAddress ("http://localhost")));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ErrResolve6 ()
		{
			ContractDescription contract = ContractDescription.GetContract (typeof (IEchoService));
			List<ContractDescription> contracts = new List<ContractDescription> ();

			//FIXME: What is the 'client' param used for?
			//TODO: Write test cases for the related overloads of Resolve
			MetadataResolver.Resolve (
				contracts,
				new Uri (url),
				MetadataExchangeClientMode.HttpGet,
				new MetadataExchangeClient (new EndpointAddress ("http://localhost")));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ErrResolve7 ()
		{
			MetadataResolver.Resolve (
				null,
				new Uri (url),
				MetadataExchangeClientMode.HttpGet,
				new MetadataExchangeClient (new EndpointAddress ("http://localhost")));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ErrResolve8 ()
		{
			ContractDescription contract = ContractDescription.GetContract (typeof (IEchoService));
			List<ContractDescription> contracts = new List<ContractDescription> ();
			contracts.Add (contract);

			MetadataResolver.Resolve (contracts, null);
		}

		/* Test for bad endpoint address */
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ErrResolve9 ()
		{
			ContractDescription contract = ContractDescription.GetContract (typeof (IEchoService));
			List<ContractDescription> contracts = new List<ContractDescription> ();
			contracts.Add (contract);

			MetadataResolver.Resolve (contracts, new EndpointAddress ("http://localhost"));
		}

		private void CheckIEchoServiceEndpoint (ServiceEndpointCollection endpoints)
		{
			Assert.IsNotNull (endpoints);
			Assert.AreEqual (1, endpoints.Count);

			ServiceEndpoint ep = endpoints [0];

			//URI Dependent
			//Assert.AreEqual ("http://localhost:8080/echo/svc", ep.Address.Uri.AbsoluteUri, "#R1");
			Assert.AreEqual ("IEchoService", ep.Contract.Name, "#R3");
			Assert.AreEqual ("http://myns/echo", ep.Contract.Namespace, "#R4");
			Assert.AreEqual ("BasicHttpBinding_IEchoService", ep.Name, "#R5");

			Assert.AreEqual (typeof (BasicHttpBinding), ep.Binding.GetType (), "#R2");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ResolveNonContract ()
		{
			MetadataResolver.Resolve (
				typeof (Int32), new EndpointAddress (url));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ResolveBadUri ()
		{
			MetadataResolver.Resolve (
				typeof (IEchoService), new EndpointAddress ("http://localhost"));
		}

		[DataContract]
		public class dc
		{
			[DataMember]
			string field;
		}

		[ServiceContract (Namespace = "http://myns/echo")]
		public interface IEchoService
		{

			[OperationContract]
			string Echo (string msg, int num, dc d);

			[OperationContract]
			string DoubleIt (int it, string prefix);

		}

		[ServiceContract]
		public class NonExistantContract
		{
		}
	}
}
#endif
