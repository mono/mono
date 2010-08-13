//
// Author: Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Discovery
{
	public class UdpDiscoveryEndpoint : DiscoveryEndpoint
	{
		public static readonly Uri DefaultIPv4MulticastAddress = new Uri ("soap.udp://239.255.255.250:3702/");
		public static readonly Uri DefaultIPv6MulticastAddress = new Uri ("soap.udp://[FF02:0000:0000:0000:0000:0000:0000:000C]:3702/");

		internal static Uri DefaultMulticastAddress {
			get { return Socket.SupportsIPv4 ? DefaultIPv4MulticastAddress : DefaultIPv6MulticastAddress; }
		}
		
		// (1)->(2)
		public UdpDiscoveryEndpoint ()
			: this (DiscoveryVersion.WSDiscovery11)
		{
		}

		// (2)->(6)
		public UdpDiscoveryEndpoint (DiscoveryVersion discoveryVersion)
			: this (discoveryVersion, DefaultMulticastAddress)
		{
		}

		// (3)->(4)
		public UdpDiscoveryEndpoint (string multicastAddress)
			: this (new Uri (multicastAddress))
		{
		}

		// (4)->(5)
		public UdpDiscoveryEndpoint (Uri multicastAddress)
			: this (DiscoveryVersion.WSDiscovery11, multicastAddress)
		{
		}

		// (5)->(6)
		public UdpDiscoveryEndpoint (DiscoveryVersion discoveryVersion, string multicastAddress)
			: this (discoveryVersion, new Uri (multicastAddress))
		{
		}

		// (6), everything falls to here.
		public UdpDiscoveryEndpoint (DiscoveryVersion discoveryVersion, Uri multicastAddress)
			: base (discoveryVersion, ServiceDiscoveryMode.Adhoc, CreateBinding (), new EndpointAddress (discoveryVersion.AdhocAddress))
		{
			ListenUri = multicastAddress;
			TransportSettings = new UdpTransportSettings ();
			MulticastAddress = multicastAddress;
			MaxResponseDelay = TimeSpan.FromMilliseconds (500);
		}

		static Binding CreateBinding ()
		{
			return new CustomBinding (new TextMessageEncodingBindingElement (), new UdpTransportBindingElement ());
		}

		public Uri MulticastAddress { get; set; }
		public UdpTransportSettings TransportSettings { get; private set; }
	}
}
