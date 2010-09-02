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
using System.Net.Sockets;
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
		void AssertTcpPortOpen (int port)
		{
			try {
				var l = new TcpListener (port);
				l.Start ();
				l.Stop ();
			} catch (Exception ex) {
				Assert.Fail (String.Format ("TCP port {0} is not freed", port));
			}
		}

		[Test]
		public void UseCase1 ()
		{
			RunCodeUnderDiscoveryHost1 (new Uri ("http://localhost:37564"), new Uri ("http://localhost:4949"), new Uri ("http://localhost:4989"), UseCase1Core);
			AssertTcpPortOpen (4949);
			AssertTcpPortOpen (4989);
			AssertTcpPortOpen (37564);
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
				var be = new DiscoveryClientBindingElement () { DiscoveryEndpointProvider = new SimpleDiscoveryEndpointProvider (dEndpoint) };
				var clientBinding = new CustomBinding (new BasicHttpBinding ());
				clientBinding.Elements.Insert (0, be);
				var cf = new ChannelFactory<ITestService> (clientBinding, DiscoveryClientBindingElement.DiscoveryEndpointAddress);
				var ch = cf.CreateChannel ();
				Assert.AreEqual ("TEST", ch.Echo ("TEST"), "#1");
				cf.Close ();
			} finally {
				host.Close ();
			}
		}

		void RunCodeUnderDiscoveryHost1 (Uri serviceUri, Uri dHostUri, Uri aHostUri, Action<Uri,AnnouncementEndpoint,DiscoveryEndpoint> action)
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
			using (var inst = new AnnouncementBoundDiscoveryService (aEndpoint)) {
				var host = new ServiceHost (inst);
				host.AddServiceEndpoint (dEndpoint);
				try {
					host.Open ();
					action (serviceUri, aEndpoint, dEndpoint);
				} finally {
					host.Close ();
				}
			}
		}

		[Test] // Announcement: UDP, Discovery: HTTP
		public void UseCase2 ()
		{
			RunCodeUnderDiscoveryHost2 (new Uri ("http://localhost:37564"), new Uri ("http://localhost:4949"), UseCase2Core);
			AssertTcpPortOpen (4949);
			AssertTcpPortOpen (37564);
		}

		void UseCase2Core (Uri serviceUri, AnnouncementEndpoint aEndpoint, DiscoveryEndpoint dEndpoint)
		{
			// actual service, announcing to UDP
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
				var be = new DiscoveryClientBindingElement () { DiscoveryEndpointProvider = new SimpleDiscoveryEndpointProvider (dEndpoint) };
				var clientBinding = new CustomBinding (new BasicHttpBinding ());
				clientBinding.Elements.Insert (0, be);
				var cf = new ChannelFactory<ITestService> (clientBinding, DiscoveryClientBindingElement.DiscoveryEndpointAddress);
				var ch = cf.CreateChannel ();
				Assert.AreEqual ("TEST", ch.Echo ("TEST"), "#1");
			} finally {
				host.Close ();
			}
		}

		void RunCodeUnderDiscoveryHost2 (Uri serviceUri, Uri dHostUri, Action<Uri,AnnouncementEndpoint,DiscoveryEndpoint> action)
		{
			// announcement service
			var aEndpoint = new UdpAnnouncementEndpoint (new Uri ("soap.udp://239.255.255.250:3802/"));
			
			// discovery service
			var dbinding = new CustomBinding (new HttpTransportBindingElement ());
			var dAddress = new EndpointAddress (dHostUri);
			var dEndpoint = new DiscoveryEndpoint (dbinding, dAddress);
			// Without this, .NET rejects the host as if it had no service.
			dEndpoint.IsSystemEndpoint = false;

			// it internally hosts an AnnouncementService
			using (var inst = new AnnouncementBoundDiscoveryService (aEndpoint)) {
				var host = new ServiceHost (inst);
				host.AddServiceEndpoint (dEndpoint);
				try {
					host.Open ();
					action (serviceUri, aEndpoint, dEndpoint);
				} finally {
					host.Close ();
				}
			}
		}

		class SimpleDiscoveryEndpointProvider : DiscoveryEndpointProvider
		{
			public SimpleDiscoveryEndpointProvider (DiscoveryEndpoint endpoint)
			{
				this.endpoint = endpoint;
			}
			
			DiscoveryEndpoint endpoint;
			
			public override DiscoveryEndpoint GetDiscoveryEndpoint ()
			{
				return endpoint;
			}
		}
	}
}
