//
// WebServiceHostTest.cs
//
// Author:
//	Igor Zelmanovich  <igorz@mainsoft.com>
//
// Copyright (C) 2008 Mainsoft, Inc (http://www.mainsoft.com)
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace MonoTests.System.ServiceModel.Web
{
	[TestFixture]
	public class WebServiceHostTest
	{
		[Test]
		[Category("NotWorking")]
		public void ServiceDebugBehaviorTest () {

			var host = new WebServiceHost (typeof (MyService), new Uri ("http://localhost:8080/"));
			ServiceEndpoint webHttp = host.AddServiceEndpoint ("MonoTests.System.ServiceModel.Web.WebServiceHostTest+MyService", new WebHttpBinding (), "WebHttpBinding");

			Assert.AreEqual (true, host.Description.Behaviors.Find<ServiceDebugBehavior> ().HttpHelpPageEnabled, "HttpHelpPageEnabled #1");
			Assert.AreEqual (true, host.Description.Behaviors.Find<ServiceDebugBehavior> ().HttpsHelpPageEnabled, "HttpsHelpPageEnabled #1");

			host.Open ();

			Assert.AreEqual (false, host.Description.Behaviors.Find<ServiceDebugBehavior> ().HttpHelpPageEnabled, "HttpHelpPageEnabled #2");
			Assert.AreEqual (false, host.Description.Behaviors.Find<ServiceDebugBehavior> ().HttpsHelpPageEnabled, "HttpsHelpPageEnabled #2");

			host.Close ();
		}

		[Test]
		[Category ("NotWorking")]
		public void WebHttpBehaviorTest1 () {

			var host = new WebServiceHost (typeof (MyService), new Uri ("http://localhost:8080/"));
			ServiceEndpoint webHttp = host.AddServiceEndpoint ("MonoTests.System.ServiceModel.Web.WebServiceHostTest+MyService", new WebHttpBinding (), "WebHttpBinding");
			ServiceEndpoint basicHttp = host.AddServiceEndpoint ("MonoTests.System.ServiceModel.Web.WebServiceHostTest+MyService", new BasicHttpBinding (), "BasicHttpBinding");

			Assert.AreEqual (0, webHttp.Behaviors.Count, "webHttp.Behaviors.Count #1");
			Assert.AreEqual (0, basicHttp.Behaviors.Count, "basicHttp.Behaviors.Count #1");

			host.Open ();

			Assert.AreEqual (1, webHttp.Behaviors.Count, "webHttp.Behaviors.Count #2");
			Assert.AreEqual (typeof (WebHttpBehavior), webHttp.Behaviors [0].GetType (), "behavior type");
			Assert.AreEqual (0, basicHttp.Behaviors.Count, "basicHttp.Behaviors.Count #2");

			host.Close ();
		}

		[Test]
		[Category("NotWorking")]
		public void WebHttpBehaviorTest2 () {

			var host = new WebServiceHost (typeof (MyService), new Uri ("http://localhost:8080/"));
			ServiceEndpoint webHttp = host.AddServiceEndpoint ("MonoTests.System.ServiceModel.Web.WebServiceHostTest+MyService", new WebHttpBinding (), "WebHttpBinding");
			MyWebHttpBehavior behavior = new MyWebHttpBehavior ();
			behavior.ApplyDispatchBehaviorBegin += delegate {
				Assert.AreEqual (typeof (EndpointAddressMessageFilter), ((ChannelDispatcher) host.ChannelDispatchers [0]).Endpoints [0].AddressFilter.GetType (), "AddressFilter.GetType #1");
				Assert.AreEqual (typeof (ActionMessageFilter), ((ChannelDispatcher) host.ChannelDispatchers [0]).Endpoints [0].ContractFilter.GetType (), "ContractFilter.GetType #1");
			};
			behavior.ApplyDispatchBehaviorEnd += delegate {
				Assert.AreEqual (typeof (PrefixEndpointAddressMessageFilter), ((ChannelDispatcher) host.ChannelDispatchers [0]).Endpoints [0].AddressFilter.GetType (), "AddressFilter.GetType #2");
				Assert.AreEqual (typeof (MatchAllMessageFilter), ((ChannelDispatcher) host.ChannelDispatchers [0]).Endpoints [0].ContractFilter.GetType (), "ContractFilter.GetType #2");
			};
			webHttp.Behaviors.Add (behavior);

			host.Open ();
			host.Close ();
		}

		[Test]
		public void ServiceBaseUriTest () {

			var host = new WebServiceHost (typeof (MyService), new Uri ("http://localhost:8080/"));
			Assert.AreEqual (0, host.Description.Endpoints.Count, "no endpoints yet");
			host.Open ();
			Assert.AreEqual (1, host.Description.Endpoints.Count, "default endpoint after open");
			host.Close ();
		}

		class MyWebHttpBehavior : WebHttpBehavior
		{
			public event EventHandler ApplyDispatchBehaviorBegin;
			public event EventHandler ApplyDispatchBehaviorEnd;

			public override void ApplyDispatchBehavior (ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) {
				if (ApplyDispatchBehaviorBegin != null)
					ApplyDispatchBehaviorBegin (this, EventArgs.Empty);
				base.ApplyDispatchBehavior (endpoint, endpointDispatcher);
				if (ApplyDispatchBehaviorEnd != null)
					ApplyDispatchBehaviorEnd (this, EventArgs.Empty);
			}
		}

		[ServiceContract]
		public class MyService
		{
			[OperationContract]
			[WebGet (RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
			public string Greet (string input) {
				return "huh? " + input;
			}
		}

	}
}
