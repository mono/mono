//
// ServiceDebugBehaviorTest.cs
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
using System.Net.Sockets;
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
	public class ServiceDebugBehaviorTest
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
			using (ServiceHost host = new ServiceHost (typeof (MyService), new Uri ("http://localhost:" + NetworkHelpers.FindFreePort ()))) {
				host.AddServiceEndpoint (typeof (IMyContract), new BasicHttpBinding (), "e1");

				Assert.AreEqual (0, host.ChannelDispatchers.Count, "ChannelDispatchers.Count #1");
				try {
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
				} finally {
				host.Close ();
				}
			}
		}

		[Test]
		public void InitializeRuntime2 () {
			using (ServiceHost host = new ServiceHost (typeof (MyService), new Uri ("http://localhost:" + NetworkHelpers.FindFreePort ()))) {
				host.AddServiceEndpoint (typeof (IMyContract), new BasicHttpBinding (), "");
				host.Description.Behaviors.Remove<ServiceDebugBehavior> ();

				Assert.AreEqual (0, host.ChannelDispatchers.Count, "ChannelDispatchers.Count #1");

				try {
				host.Open ();

				Assert.AreEqual (1, host.ChannelDispatchers.Count, "ChannelDispatchers.Count #2");

				} finally {
				host.Close ();
				}
			}
		}

		[Test]
		public void InitializeRuntime3 () {
			using (ServiceHost host = new ServiceHost (typeof (MyService), new Uri ("http://localhost:" + NetworkHelpers.FindFreePort ()))) {
				host.AddServiceEndpoint (typeof (IMyContract), new BasicHttpBinding (), "");
				host.Description.Behaviors.Find<ServiceDebugBehavior> ().HttpHelpPageEnabled = false;

				Assert.AreEqual (0, host.ChannelDispatchers.Count, "ChannelDispatchers.Count #1");

				try {
				host.Open ();

				Assert.AreEqual (1, host.ChannelDispatchers.Count, "ChannelDispatchers.Count #2");
				} finally {
				host.Close ();
				}
			}
		}

		[Test]
		public void InitializeRuntime4 () {
			int port = NetworkHelpers.FindFreePort ();
			using (ServiceHost host = new ServiceHost (typeof (MyService), new Uri ("http://localhost:" + port))) {
				host.AddServiceEndpoint (typeof (IMyContract), new BasicHttpBinding (), "");
				host.Description.Behaviors.Find<ServiceDebugBehavior> ().HttpHelpPageUrl = new Uri ("http://localhost:" + port + "/help");

				Assert.AreEqual (0, host.ChannelDispatchers.Count, "ChannelDispatchers.Count #1");

				try {
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
				Assert.AreEqual (new Uri ("http://localhost:" + port + "/help"), ea.Uri, "Uri");

				DispatchRuntime dr = ed.DispatchRuntime;
				Assert.AreEqual (1, dr.Operations.Count, "Operations.Count");

				DispatchOperation dispOp = dr.Operations [0];
				Assert.AreEqual ("*", dispOp.Action, "Operation.Action");
				Assert.AreEqual ("*", dispOp.ReplyAction, "Operation.ReplyAction");
				Assert.AreEqual ("Get", dispOp.Name, "Operation.Name");
				//Assert.IsNotNull (dispOp.Invoker, "Operation.Invoker");
				} finally {
				host.Close ();
				}
			}
		}

		[Test]
		public void ServiceMetadataExtension1 () {
			int port = NetworkHelpers.FindFreePort ();
			using (ServiceHost host = new ServiceHost (typeof (MyService), new Uri ("http://localhost:" + port))) {
				host.AddServiceEndpoint (typeof (IMyContract), new BasicHttpBinding (), "");
				host.Description.Behaviors.Find<ServiceDebugBehavior> ().HttpHelpPageUrl = new Uri ("http://localhost:" + port + "/help");
				try {
				host.Open ();

				Assert.IsNotNull (host.Extensions.Find<ServiceMetadataExtension> (), "ServiceMetadataExtension #1");
				Assert.AreEqual (1, host.Extensions.FindAll<ServiceMetadataExtension> ().Count, "ServiceMetadataExtension #2");
				} finally {
				host.Close ();
				}
			}
		}

		[Test]
		public void ServiceMetadataExtension2 () {
			int port = NetworkHelpers.FindFreePort ();
			using (ServiceHost host = new ServiceHost (typeof (MyService), new Uri ("http://localhost:" + port))) {
				host.AddServiceEndpoint (typeof (IMyContract), new BasicHttpBinding (), "");
				host.Description.Behaviors.Find<ServiceDebugBehavior> ().HttpHelpPageUrl = new Uri ("http://localhost:" + port + "/help");

				ServiceMetadataExtension extension = new ServiceMetadataExtension ();
				host.Extensions.Add (extension);

				try {
				host.Open ();

				Assert.IsNotNull (host.Extensions.Find<ServiceMetadataExtension> (), "ServiceMetadataExtension #1");
				Assert.AreEqual (1, host.Extensions.FindAll<ServiceMetadataExtension> ().Count, "ServiceMetadataExtension #2");
				Assert.AreEqual (extension, host.Extensions.Find<ServiceMetadataExtension> (), "ServiceMetadataExtension #3");
				} finally {
				host.Close ();
				}
			}
		}

		[Test]
		public void Defaults () {
			ServiceDebugBehavior behavior = new ServiceDebugBehavior ();
			Assert.AreEqual (true, behavior.HttpHelpPageEnabled, "HttpHelpPageEnabled");
			Assert.IsNull (behavior.HttpHelpPageUrl, "HttpHelpPageUrl");
			Assert.AreEqual (true, behavior.HttpsHelpPageEnabled, "HttpsHelpPageEnabled");
			Assert.IsNull (behavior.HttpsHelpPageUrl, "HttpsHelpPageUrl");
			Assert.AreEqual (false, behavior.IncludeExceptionDetailInFaults, "IncludeExceptionDetailInFaults");
		}
	}
}
#endif
