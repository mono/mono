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
	public class EndpointDiscoveryBehaviorTest
	{
		[Test]
		public void Use ()
		{
			// Without ServiceDiscoveryBehavior.
			var b = new EndpointDiscoveryBehavior ();
			IEndpointBehavior eb = b;
			var host = new ServiceHost (typeof (TestService));
			var se = host.AddServiceEndpoint (typeof (ITestService), new BasicHttpBinding (), new Uri ("http://localhost:37564"));
			se.Behaviors.Add (b);

			var bc = new BindingParameterCollection ();
			eb.AddBindingParameters (se, bc);
			Assert.AreEqual (0, bc.Count, "#1");
			Assert.AreEqual (0, host.Extensions.Count, "#1-2");

			eb.Validate (se);
			// This behavior itself does not populate discovery extension. It just configures the extension.
			Assert.AreEqual (0, host.Extensions.Count, "#2-2");
		}

		[Test]
		public void Use2 ()
		{
			// This time with ServiceDiscoveryBehavior.
			var b = new EndpointDiscoveryBehavior ();
			IEndpointBehavior eb = b;
			var host = new ServiceHost (typeof (TestService));
			var se = host.AddServiceEndpoint (typeof (ITestService), new BasicHttpBinding (), new Uri ("http://localhost:37564"));
			var sdb = new ServiceDiscoveryBehavior ();
			sdb.AnnouncementEndpoints.Add (new UdpAnnouncementEndpoint ());
			IServiceBehavior sb = sdb;
			se.Behaviors.Add (b);

			var bc = new BindingParameterCollection ();
			sb.AddBindingParameters (host.Description, host, host.Description.Endpoints, bc);
			eb.AddBindingParameters (se, bc);
			Assert.AreEqual (0, bc.Count, "#1");
			Assert.AreEqual (0, host.Extensions.Count, "#1-2");

			sb.Validate (host.Description, host);
			eb.Validate (se);
			// ... should "validate" not "apply dispatch behavior" do "add host extension" job? I doubt that.
			Assert.AreEqual (1, host.Extensions.Count, "#2-2");
			var dse = host.Extensions.Find<DiscoveryServiceExtension> ();

			Assert.IsNotNull (dse, "#2-3");
			Assert.AreEqual (0, dse.PublishedEndpoints.Count, "#2-4");
			Assert.AreEqual (2, se.Behaviors.Count, "#2-5"); // EndpointDiscoveryBehavior + discovery initializer.

			Assert.AreEqual (0, host.ChannelDispatchers.Count, "#3-1");
			Assert.AreEqual (1, host.Description.Endpoints.Count, "#3-2");
			Assert.AreEqual (0, dse.PublishedEndpoints.Count, "#3-4");

			// The IEndpointBehavior from EndpointDiscoveryBehavior, when ApplyDispatchBehavior() is invoked, publishes an endpoint.
			sb.ApplyDispatchBehavior (host.Description, host);
			Assert.AreEqual (0, dse.PublishedEndpoints.Count, "#3-5"); // not yet published
			eb.ApplyDispatchBehavior (se, new EndpointDispatcher (new EndpointAddress ("http://localhost:37564"), "ITestService", "http://tempuri.org/"));
			Assert.AreEqual (2, host.ChannelDispatchers.Count, "#3-6-1"); // for online and offline announcements
			Assert.AreEqual (0, dse.PublishedEndpoints.Count, "#3-6-2"); // still not published.

			host.Open ();
			try {
				Assert.AreEqual (3, host.ChannelDispatchers.Count, "#4-1"); // for online and offline announcements
				Assert.AreEqual (1, dse.PublishedEndpoints.Count, "#4-2"); // The endpoint is published again. (Not sure if it's worthy of testing.)
			} finally {
				host.Close ();
			}
		}
	}
}
