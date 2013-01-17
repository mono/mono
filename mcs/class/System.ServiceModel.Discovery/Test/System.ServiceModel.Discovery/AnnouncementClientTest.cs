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
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.ServiceModel.Dispatcher;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Discovery
{
	[TestFixture]
	public class AnnouncementClientTest
	{
		[Test]
		public void DefaultValues ()
		{
			// default constructor causes IOE unless there is default configuration...
			var ac = new AnnouncementClient (new AnnouncementEndpoint ());
			Assert.AreEqual (ac.ChannelFactory.Endpoint, ac.Endpoint, "#1");
			Assert.AreEqual ("http://docs.oasis-open.org/ws-dd/ns/discovery/2009/01", ac.Endpoint.Contract.Namespace, "#2");
			Assert.AreEqual (2, ac.Endpoint.Contract.Operations.Count, "#2-2");
			Assert.IsTrue (ac.Endpoint.Contract.Operations.Any (od => od.Messages.Any (md => md.Action == "http://docs.oasis-open.org/ws-dd/ns/discovery/2009/01/Hello")), "#2-3");
			Assert.IsTrue (ac.Endpoint.Contract.Operations.Any (od => od.Messages.Any (md => md.Action == "http://docs.oasis-open.org/ws-dd/ns/discovery/2009/01/Bye")), "#2-4");
			Assert.IsNull (ac.Endpoint.Binding, "#3");
			Assert.IsNull (ac.Endpoint.Address, "#4");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		[Category ("NotWorking")]
		public void AnnonceOnlineOfflineNoEndpointAddress ()
		{
			var ac = new AnnouncementClient (new AnnouncementEndpoint () { Binding = new BasicHttpBinding () });
			var edm = new EndpointDiscoveryMetadata ();
			try {
				ac.AnnounceOnline (edm);
			} finally {
				ac.Close ();
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		[Category ("NotWorking")]
		public void AnnonceOnlineOfflineNoBinding ()
		{
			var ac = new AnnouncementClient (new AnnouncementEndpoint () { Address = new EndpointAddress ("http://localhost:37564")});
			var edm = new EndpointDiscoveryMetadata ();
			ac.AnnounceOnline (edm);
			// attempt to close the client causes another CommunicationObjectFaultedException - looks like it fails to allow Close() at faulted state unlike other objects.
		}

		[Test]
		// looks like EndpointAddress is *ignored*
		[Category ("NotOnMac")]
		public void AnnonceOnlineOfflineAddressSchemeMismatch ()
		{
			var ac = new AnnouncementClient (new UdpAnnouncementEndpoint () { Address = new EndpointAddress ("http://localhost:37564")});
			var edm = new EndpointDiscoveryMetadata ();
			try {
				ac.AnnounceOnline (edm);
				ac.AnnounceOffline (edm);
			} finally {
				ac.Close ();
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		[Category ("NotWorking")]
		public void AnnonceOnlineOfflineAddressSchemeMismatch2 ()
		{
			var ac = new AnnouncementClient (new AnnouncementEndpoint () { Binding = new BasicHttpBinding (), Address = new EndpointAddress ("soap.udp://localhost:37564")});
			var edm = new EndpointDiscoveryMetadata ();
			try {
				ac.AnnounceOnline (edm);
			} finally {
				ac.Close ();
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		[Category ("NotWorking")]
		public void AnnonceOnlineOfflineHttpMessageVersionMismatch ()
		{
			var ac = new AnnouncementClient (new AnnouncementEndpoint () { Binding = new BasicHttpBinding () { SendTimeout = TimeSpan.FromSeconds (10), ReceiveTimeout = TimeSpan.FromSeconds (10) }, Address = http_address });
			var edm = new EndpointDiscoveryMetadata ();
			try {
				ac.AnnounceOnline (edm);
			} finally {
				ac.Close ();
			}
		}

		EndpointAddress http_address = new EndpointAddress ("http://localhost:37564");

		[Test]
		[ExpectedException (typeof (EndpointNotFoundException))]
		[Category ("NotWorking")]
		public void AnnonceOnlineOfflineHttpWSA10 ()
		{
			var binding = new CustomBinding (new HttpTransportBindingElement ()) { SendTimeout = TimeSpan.FromSeconds (10), ReceiveTimeout = TimeSpan.FromSeconds (10) };
			var ac = new AnnouncementClient (new AnnouncementEndpoint () { Binding = binding, Address = http_address });
			var edm = new EndpointDiscoveryMetadata ();
			try {
				ac.AnnounceOnline (edm);
			} finally {
				ac.Close ();
			}
		}
	}
}
