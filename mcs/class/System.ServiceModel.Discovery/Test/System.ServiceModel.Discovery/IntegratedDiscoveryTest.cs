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
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.ServiceModel.Dispatcher;
using System.Threading;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Discovery
{
	[TestFixture]
	public class IntegratedDiscoveryTest
	{
		[Test]
		public void UseCase1 ()
		{
			RunCodeUnderDiscoveryHost (new Uri ("http://localhost:37564"), new Uri ("http://localhost:4949"), new Uri ("http://localhost:4989"), UseCase1Core);
		}

		void UseCase1Core (Uri serviceUri, AnnouncementEndpoint aEndpoint, DiscoveryEndpoint dEndpoint)
		{
			// actual service, announcing to 4989
			var host = new ServiceHost (typeof (TestService));
			var sdb = new ServiceDiscoveryBehavior ();
			sdb.AnnouncementEndpoints.Add (aEndpoint);
			host.Description.Behaviors.Add (sdb);
			host.AddServiceEndpoint (typeof (ITestService), new BasicHttpBinding (), serviceUri);
			host.Open ();
			// It does not start announcement very soon, so wait for a while.
			Thread.Sleep (1000);
			foreach (var edm in host.Extensions.Find<DiscoveryServiceExtension> ().PublishedEndpoints)
				TextWriter.Null.WriteLine ("Published Endpoint: " + edm.Address);
			try {
				// actual client, with DiscoveryClientBindingElement
				var be = new DiscoveryClientBindingElement () { DiscoveryEndpointProvider = new ManagedDiscoveryEndpointProvider (dEndpoint) };
				var clientBinding = new CustomBinding (new BasicHttpBinding ());
				clientBinding.Elements.Insert (0, be);
				var cf = new ChannelFactory<ITestService> (clientBinding, DiscoveryClientBindingElement.DiscoveryEndpointAddress);
				var ch = cf.CreateChannel ();
				Assert.AreEqual ("TEST", ch.Echo ("TEST"), "#1");
			} finally {
				host.Close ();
			}
		}

		class ManagedDiscoveryEndpointProvider : DiscoveryEndpointProvider
		{
			public ManagedDiscoveryEndpointProvider (DiscoveryEndpoint endpoint)
			{
				this.endpoint = endpoint;
			}
			
			DiscoveryEndpoint endpoint;
			
			public override DiscoveryEndpoint GetDiscoveryEndpoint ()
			{
				return endpoint;
			}
		}

		void RunCodeUnderDiscoveryHost (Uri serviceUri, Uri dHostUri, Uri aHostUri, Action<Uri,AnnouncementEndpoint,DiscoveryEndpoint> action)
		{
			// announcement service
			var abinding = new CustomBinding (new HttpTransportBindingElement ());
			var aAddress = new EndpointAddress (aHostUri);
			var aEndpoint = new AnnouncementEndpoint (abinding, aAddress);
			
			// discovery service
			var dbinding = new CustomBinding (new HttpTransportBindingElement ());
			var dAddress = new EndpointAddress (dHostUri);
			var dEndpoint = new DiscoveryEndpoint (dbinding, dAddress);
			// Without this, .NET rejects the host as if it had no service.
			dEndpoint.IsSystemEndpoint = false;

			// it internally hosts an AnnouncementService
			var host = new ServiceHost (new AnnouncementBoundDiscoveryService (aEndpoint));
			host.AddServiceEndpoint (dEndpoint);
			try {
				host.Open ();
				action (serviceUri, aEndpoint, dEndpoint);
			} finally {
				host.Close ();
			}
		}
	}
}
