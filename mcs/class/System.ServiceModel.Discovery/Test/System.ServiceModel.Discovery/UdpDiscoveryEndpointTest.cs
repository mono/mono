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
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.ServiceModel.Dispatcher;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Discovery
{
	[TestFixture]
	public class UdpDiscoveryEndpointTest
	{
		[Test]
		public void DefaultValues ()
		{
			var de = new UdpDiscoveryEndpoint ();
			Assert.AreEqual (DiscoveryVersion.WSDiscovery11, de.DiscoveryVersion, "#1");
			Assert.AreEqual (ServiceDiscoveryMode.Adhoc, de.DiscoveryMode, "#2");
			Assert.AreEqual (TimeSpan.FromMilliseconds (500), de.MaxResponseDelay, "#3");
			var cd = de.Contract;
			Assert.IsNotNull (cd, "#11");
			Assert.IsNotNull (de.Binding, "#12");
			TransportBindingElement tbe;
			Assert.IsTrue (de.Binding.CreateBindingElements ().Any (be => (tbe = be as TransportBindingElement) != null && tbe.Scheme == "soap.udp"), "#12-2");
			Assert.IsNotNull (de.Address, "#13");
			Assert.AreEqual (DiscoveryVersion.WSDiscovery11.AdhocAddress, de.Address.Uri, "#13-2");
			Assert.AreEqual (Socket.SupportsIPv4 ? UdpDiscoveryEndpoint.DefaultIPv4MulticastAddress : UdpDiscoveryEndpoint.DefaultIPv6MulticastAddress, de.ListenUri, "#14");
		}
	}
}
