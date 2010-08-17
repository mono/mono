//
// Author: Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.ServiceModel.Dispatcher;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Discovery
{
	[TestFixture]
	public class ServiceDiscoveryBehaviorTest
	{
		[Test]
		public void Use ()
		{
			var b = new ServiceDiscoveryBehavior ();
			b.AnnouncementEndpoints.Add (new UdpAnnouncementEndpoint ());
			IServiceBehavior sb = b;
			var host = new ServiceHost (new Uri ("http://localhost:37564"));

			var bc = new BindingParameterCollection ();
			sb.AddBindingParameters (host.Description, host, host.Description.Endpoints, bc);
			Assert.AreEqual (0, bc.Count, "#1");

			Assert.AreEqual (0, host.Extensions.Count, "#2-1");
			sb.Validate (host.Description, host);
			// ... should "validate" not "apply dispatch behavior" do "add host extension" job? I doubt that.
			Assert.AreEqual (1, host.Extensions.Count, "#2-2");
			var dse = host.Extensions.Find<DiscoveryServiceExtension> ();
			Assert.IsNotNull (dse, "#2-3");
			Assert.AreEqual (0, dse.PublishedEndpoints.Count, "#2-4");

			Assert.AreEqual (0, host.ChannelDispatchers.Count, "#3-1");
			sb.ApplyDispatchBehavior (host.Description, host);
			Assert.AreEqual (0, host.Description.Endpoints.Count, "#3-2");
			Assert.AreEqual (2, host.ChannelDispatchers.Count, "#3-3"); // for online and offline announcements
			Assert.AreEqual (0, dse.PublishedEndpoints.Count, "#3-4"); // discovery endpoints are not "published"
			int idx = 0;
			foreach (var cdisb in host.ChannelDispatchers) {
				var cdis = cdisb as ChannelDispatcher;
				string head = "#4." + idx + ".";
				Assert.IsNull (cdis, head + "dispatcher");
				if (cdisb.Listener != null)
					Assert.AreEqual ("urn:schemas-microsoft-org:ws:2008:07:discovery", cdisb.Listener.Uri.ToString (), head + "uri");
				// else ... WHOA! .NET "OnlineAnnouncementChannelDispatcher" type does not seem to provide the listener.
				idx++;
			}
		}

		[Test]
		public void Use2 ()
		{
			var b = new ServiceDiscoveryBehavior ();
			b.AnnouncementEndpoints.Add (new UdpAnnouncementEndpoint ());
			IServiceBehavior sb = b;
			var host = new ServiceHost (typeof (TestService));
			var se = host.AddServiceEndpoint (typeof (ITestService), new BasicHttpBinding (), new Uri ("http://localhost:37564"));

			var bc = new BindingParameterCollection ();
			sb.AddBindingParameters (host.Description, host, host.Description.Endpoints, bc);
			Assert.AreEqual (0, bc.Count, "#1");
			Assert.AreEqual (0, host.Extensions.Count, "#1-2");
			Assert.AreEqual (0, se.Behaviors.Count, "#1-3");

			sb.Validate (host.Description, host);
			// ... should "validate" not "apply dispatch behavior" do "add host extension" job? I doubt that.
			Assert.AreEqual (1, host.Extensions.Count, "#2-2");
			var dse = host.Extensions.Find<DiscoveryServiceExtension> ();
			Assert.IsNotNull (dse, "#2-3");
			Assert.AreEqual (0, dse.PublishedEndpoints.Count, "#2-4");
			Assert.AreEqual (1, se.Behaviors.Count, "#2-5"); // Validate() adds endpoint initialization behavior.

			Assert.AreEqual (0, host.ChannelDispatchers.Count, "#3-1");
			sb.ApplyDispatchBehavior (host.Description, host);
			Assert.AreEqual (1, host.Description.Endpoints.Count, "#3-2");
			Assert.AreEqual (2, host.ChannelDispatchers.Count, "#3-3"); // for online and offline announcements
			Assert.AreEqual (0, dse.PublishedEndpoints.Count, "#3-4"); // discovery endpoints are not "published"
			Assert.AreEqual (1, se.Behaviors.Count, "#3-5");

			// The IEndpointBehavior from ServiceDiscoveryBehavior, when ApplyDispatchBehavior() is invoked, publishes an endpoint.
			se.Behaviors [0].ApplyDispatchBehavior (se, new EndpointDispatcher (new EndpointAddress ("http://localhost:37564"), "ITestService", "http://tempuri.org/"));
			Assert.AreEqual (2, host.ChannelDispatchers.Count, "#3-6-1"); // for online and offline announcements
			Assert.AreEqual (1, dse.PublishedEndpoints.Count, "#3-6-2"); // The endpoint is newly published.

			host.Open ();
			try {
				Assert.AreEqual (3, host.ChannelDispatchers.Count, "#4-1"); // for online and offline announcements
				Assert.AreEqual (2, dse.PublishedEndpoints.Count, "#4-2"); // The endpoint is published again. (Not sure if it's worthy of testing.)
			} finally {
				host.Close ();
			}
		}
	}
}
