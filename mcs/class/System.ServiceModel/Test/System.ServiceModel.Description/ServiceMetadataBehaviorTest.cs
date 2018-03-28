//
// ServiceMetadataBehaviorTest.cs
//
// Author:
//	Igor Zelmanovich <igorz@mainsoft.com>
//
// Copyright (C) 2008 Mainsoft, Inc.  http://www.mainsoft.com
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
using System.Text;
using NUnit.Framework;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;

using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel.Description
{
	[TestFixture]
	public class ServiceMetadataBehaviorTest
	{
		[ServiceContract]
		interface IMyContract
		{
			[OperationContract]
			string GetData ();
		}

		class MyService : IMyContract
		{
			public string GetData () {
				return "Hello World";
			}
		}

		[Test]
		public void InitializeRuntime1 () {
			var port = NetworkHelpers.FindFreePort ();
			using (ServiceHost host = new ServiceHost (typeof (MyService), new Uri ("http://localhost:" + port))) {
				host.AddServiceEndpoint (typeof (IMyContract), new BasicHttpBinding (), "e1");
				host.Description.Behaviors.Add (new ServiceMetadataBehavior () { HttpGetEnabled = true });

				Assert.AreEqual (0, host.ChannelDispatchers.Count, "ChannelDispatchers.Count #1");

				host.Open ();

				Assert.AreEqual (2, host.ChannelDispatchers.Count, "ChannelDispatchers.Count #2");

				ChannelDispatcher cd = (ChannelDispatcher) host.ChannelDispatchers [1];
				Assert.AreEqual (1, cd.Endpoints.Count, "Endpoints.Count");
				Assert.AreEqual ("ServiceMetadataBehaviorHttpGetBinding", cd.BindingName, "BindingName");
				Assert.AreEqual (host, cd.Host, "Host");
				//Assert.AreEqual (false, cd.IsTransactedAccept, "IsTransactedAccept");
				//Assert.AreEqual (false, cd.IsTransactedReceive, "IsTransactedReceive");

				EndpointDispatcher ed = cd.Endpoints [0];
				Assert.AreEqual (typeof (EndpointAddressMessageFilter), ed.AddressFilter.GetType (), "AddressFilter");
				Assert.AreEqual (cd, ed.ChannelDispatcher, "ChannelDispatcher");
				Assert.AreEqual (typeof (MatchAllMessageFilter), ed.ContractFilter.GetType (), "ContractFilter");
				Assert.AreEqual ("IHttpGetHelpPageAndMetadataContract", ed.ContractName, "ContractName");
				Assert.AreEqual ("http://schemas.microsoft.com/2006/04/http/metadata", ed.ContractNamespace, "ContractNamespace");
				Assert.AreEqual (0, ed.FilterPriority, "FilterPriority");

				EndpointAddress ea = ed.EndpointAddress;
				// TODO

				DispatchRuntime dr = ed.DispatchRuntime;
				// TODO

				host.Close ();
			}
		}

		[Test]
		public void InitializeRuntime2 () {
			var port = NetworkHelpers.FindFreePort ();
			using (ServiceHost host = new ServiceHost (typeof (MyService), new Uri ("http://localhost:" + port))) {
				host.AddServiceEndpoint (typeof (IMyContract), new BasicHttpBinding (), "");
				host.Description.Behaviors.Add (new ServiceMetadataBehavior () { HttpGetEnabled = true, HttpGetUrl = new Uri ("http://localhost:" + port + "/mex_and_help") });
				host.Description.Behaviors.Find<ServiceDebugBehavior> ().HttpHelpPageUrl = new Uri ("http://localhost:" + port + "/mex_and_help");

				Assert.AreEqual (0, host.ChannelDispatchers.Count, "ChannelDispatchers.Count #1");

				host.Open ();

				Assert.AreEqual (2, host.ChannelDispatchers.Count, "ChannelDispatchers.Count #2");

				ChannelDispatcher cd = (ChannelDispatcher) host.ChannelDispatchers [1];
				Assert.AreEqual (1, cd.Endpoints.Count, "Endpoints.Count");
				Assert.AreEqual ("ServiceMetadataBehaviorHttpGetBinding", cd.BindingName, "BindingName");
				Assert.AreEqual (host, cd.Host, "Host");
				//Assert.AreEqual (false, cd.IsTransactedAccept, "IsTransactedAccept");
				//Assert.AreEqual (false, cd.IsTransactedReceive, "IsTransactedReceive");

				EndpointDispatcher ed = cd.Endpoints [0];
				Assert.AreEqual (typeof (EndpointAddressMessageFilter), ed.AddressFilter.GetType (), "AddressFilter");
				Assert.AreEqual (cd, ed.ChannelDispatcher, "ChannelDispatcher");
				Assert.AreEqual (typeof (MatchAllMessageFilter), ed.ContractFilter.GetType (), "ContractFilter");
				Assert.AreEqual ("IHttpGetHelpPageAndMetadataContract", ed.ContractName, "ContractName");
				Assert.AreEqual ("http://schemas.microsoft.com/2006/04/http/metadata", ed.ContractNamespace, "ContractNamespace");
				Assert.AreEqual (0, ed.FilterPriority, "FilterPriority");

				host.Close ();
			}
		}

		[Test]
		public void InitializeRuntime3 () {
			var port = NetworkHelpers.FindFreePort ();
			using (ServiceHost host = new ServiceHost (typeof (MyService), new Uri ("http://localhost:" + port))) {
				host.AddServiceEndpoint (typeof (IMyContract), new BasicHttpBinding (), "");
				host.Description.Behaviors.Add (new ServiceMetadataBehavior () { HttpGetEnabled = true, HttpGetUrl = new Uri ("http://localhost:" + port + "/mex") });
				host.Description.Behaviors.Find<ServiceDebugBehavior> ().HttpHelpPageUrl = new Uri ("http://localhost:" + port + "/help");

				Assert.AreEqual (0, host.ChannelDispatchers.Count, "ChannelDispatchers.Count #1");

				host.Open ();

				Assert.AreEqual (3, host.ChannelDispatchers.Count, "ChannelDispatchers.Count #2");

				ChannelDispatcher cd = (ChannelDispatcher) host.ChannelDispatchers [1];
				Assert.AreEqual (1, cd.Endpoints.Count, "Endpoints.Count");

				EndpointDispatcher ed = cd.Endpoints [0];
				Assert.AreEqual (typeof (EndpointAddressMessageFilter), ed.AddressFilter.GetType (), "AddressFilter #1");
				Assert.AreEqual (cd, ed.ChannelDispatcher, "ChannelDispatcher #1");
				Assert.AreEqual (typeof (MatchAllMessageFilter), ed.ContractFilter.GetType (), "ContractFilter #1");
				Assert.AreEqual ("IHttpGetHelpPageAndMetadataContract", ed.ContractName, "ContractName #1");
				Assert.AreEqual ("http://schemas.microsoft.com/2006/04/http/metadata", ed.ContractNamespace, "ContractNamespace #1");
				Assert.AreEqual (0, ed.FilterPriority, "FilterPriority #1");

				EndpointAddress ea = ed.EndpointAddress;
				// TODO

				DispatchRuntime dr = ed.DispatchRuntime;
				// TODO

				cd = (ChannelDispatcher) host.ChannelDispatchers [2];
				Assert.AreEqual (1, cd.Endpoints.Count, "Endpoints.Count");

				ed = cd.Endpoints [0];
				Assert.AreEqual (typeof (EndpointAddressMessageFilter), ed.AddressFilter.GetType (), "AddressFilter #2");
				Assert.AreEqual (cd, ed.ChannelDispatcher, "ChannelDispatcher #2");
				Assert.AreEqual (typeof (MatchAllMessageFilter), ed.ContractFilter.GetType (), "ContractFilter #2");
				Assert.AreEqual ("IHttpGetHelpPageAndMetadataContract", ed.ContractName, "ContractName #2");
				Assert.AreEqual ("http://schemas.microsoft.com/2006/04/http/metadata", ed.ContractNamespace, "ContractNamespace #2");
				Assert.AreEqual (0, ed.FilterPriority, "FilterPriority #2");

				ea = ed.EndpointAddress;
				// TODO

				dr = ed.DispatchRuntime;
				// TODO

				host.Close ();
			}
		}

		[Test]
		public void InitializeRuntime4 () {
			var port = NetworkHelpers.FindFreePort ();
			using (ServiceHost host = new ServiceHost (typeof (MyService), new Uri ("http://localhost:" + port))) {
				host.AddServiceEndpoint (typeof (IMyContract), new BasicHttpBinding (), "");
				host.Description.Behaviors.Add (new ServiceMetadataBehavior () { HttpGetEnabled = true, HttpGetUrl = new Uri ("http://localhost:" + port + "/mex") });
				host.Description.Behaviors.Remove<ServiceDebugBehavior> ();

				Assert.AreEqual (0, host.ChannelDispatchers.Count, "ChannelDispatchers.Count #1");

				host.Open ();

				Assert.AreEqual (2, host.ChannelDispatchers.Count, "ChannelDispatchers.Count #2");

				ChannelDispatcher cd = (ChannelDispatcher) host.ChannelDispatchers [1];
				Assert.AreEqual (1, cd.Endpoints.Count, "Endpoints.Count");
				Assert.AreEqual ("ServiceMetadataBehaviorHttpGetBinding", cd.BindingName, "BindingName");
				Assert.AreEqual (host, cd.Host, "Host");
				//Assert.AreEqual (false, cd.IsTransactedAccept, "IsTransactedAccept");
				//Assert.AreEqual (false, cd.IsTransactedReceive, "IsTransactedReceive");
				Assert.AreEqual (MessageVersion.None, cd.MessageVersion, "MessageVersion");

				EndpointDispatcher ed = cd.Endpoints [0];
				Assert.AreEqual (typeof (EndpointAddressMessageFilter), ed.AddressFilter.GetType (), "AddressFilter");
				Assert.AreEqual (cd, ed.ChannelDispatcher, "ChannelDispatcher");
				Assert.AreEqual (typeof (MatchAllMessageFilter), ed.ContractFilter.GetType (), "ContractFilter");
				Assert.AreEqual ("IHttpGetHelpPageAndMetadataContract", ed.ContractName, "ContractName");
				Assert.AreEqual ("http://schemas.microsoft.com/2006/04/http/metadata", ed.ContractNamespace, "ContractNamespace");
				Assert.AreEqual (0, ed.FilterPriority, "FilterPriority");

				EndpointAddress ea = ed.EndpointAddress;
				Assert.AreEqual (new Uri ("http://localhost:" + port + "/mex"), ea.Uri, "Uri");

				DispatchRuntime dr = ed.DispatchRuntime;
				Assert.AreEqual (1, dr.Operations.Count, "Operations.Count");

				DispatchOperation dispOp = dr.Operations [0];
				Assert.AreEqual ("*", dispOp.Action, "Operation.Action");
				Assert.AreEqual ("*", dispOp.ReplyAction, "Operation.ReplyAction");
				Assert.AreEqual ("Get", dispOp.Name, "Operation.Name");
				//Assert.IsNotNull (dispOp.Invoker, "Operation.Invoker");

				host.Close ();
			}
		}

		[Test]
		public void ServiceMetadataExtension1 () {
			var port = NetworkHelpers.FindFreePort ();
			using (ServiceHost host = new ServiceHost (typeof (MyService), new Uri ("http://localhost:" + port))) {
				host.AddServiceEndpoint (typeof (IMyContract), new BasicHttpBinding (), "");
				host.Description.Behaviors.Add (new ServiceMetadataBehavior () { HttpGetEnabled = true, HttpGetUrl = new Uri ("http://localhost:" + port + "/mex") });
				host.Description.Behaviors.Remove<ServiceDebugBehavior> ();

				host.Open ();

				Assert.IsNotNull (host.Extensions.Find<ServiceMetadataExtension> (), "ServiceMetadataExtension #1");
				Assert.AreEqual (1, host.Extensions.FindAll<ServiceMetadataExtension> ().Count, "ServiceMetadataExtension #2");

				host.Close ();
			}
		}

		[Test]
		public void ServiceMetadataExtension2 () {
			var port = NetworkHelpers.FindFreePort ();
			using (ServiceHost host = new ServiceHost (typeof (MyService), new Uri ("http://localhost:" + port))) {
				host.AddServiceEndpoint (typeof (IMyContract), new BasicHttpBinding (), "");
				host.Description.Behaviors.Add (new ServiceMetadataBehavior () { HttpGetEnabled = true, HttpGetUrl = new Uri ("http://localhost:" + port + "/mex") });
				host.Description.Behaviors.Remove<ServiceDebugBehavior> ();

				ServiceMetadataExtension extension = new ServiceMetadataExtension ();
				host.Extensions.Add (extension);

				host.Open ();

				Assert.IsNotNull (host.Extensions.Find<ServiceMetadataExtension> (), "ServiceMetadataExtension #1");
				Assert.AreEqual (1, host.Extensions.FindAll<ServiceMetadataExtension> ().Count, "ServiceMetadataExtension #2");
				Assert.AreEqual (extension, host.Extensions.Find<ServiceMetadataExtension> (), "ServiceMetadataExtension #3");

				host.Close ();
			}
		}

		[Test]
		public void Defaults () {
			ServiceMetadataBehavior behavior = new ServiceMetadataBehavior ();
			Assert.IsNull (behavior.ExternalMetadataLocation, "ExternalMetadataLocation");
			Assert.AreEqual (false, behavior.HttpGetEnabled, "HttpGetEnabled");
			Assert.IsNull (behavior.HttpGetUrl, "HttpGetUrl");
			Assert.AreEqual (false, behavior.HttpsGetEnabled, "HttpsGetEnabled");
			Assert.IsNull (behavior.HttpsGetUrl, "HttpsGetUrl");
			Assert.IsNotNull (behavior.MetadataExporter, "MetadataExporter #1");
			Assert.AreEqual (typeof (WsdlExporter), behavior.MetadataExporter.GetType (), "MetadataExporter #2");

			Assert.AreEqual ("IMetadataExchange", ServiceMetadataBehavior.MexContractName, "MexContractName");
		}
	}
}
#endif