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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Discovery
{
	public abstract class DiscoveryEndpointProvider
	{
		internal static DiscoveryEndpointProvider CreateDefault ()
		{
			return new UdpDiscoveryEndpointProvider ();
		}

		public abstract DiscoveryEndpoint GetDiscoveryEndpoint ();
	}

	internal class SimpleDiscoveryEndpointProvider : DiscoveryEndpointProvider
	{
		public SimpleDiscoveryEndpointProvider (DiscoveryEndpoint value)
		{
			this.value = value;
		}
		
		DiscoveryEndpoint value;
		
		public override DiscoveryEndpoint GetDiscoveryEndpoint ()
		{
			return value;
		}
	}

	internal class UdpDiscoveryEndpointProvider : DiscoveryEndpointProvider
	{
		public override DiscoveryEndpoint GetDiscoveryEndpoint ()
		{
			var binding = new CustomBinding (new TextMessageEncodingBindingElement (), new UdpTransportBindingElement ());
			// FIXME: Name might not be set here (but needs to be modified somewhere anyways).
			return new UdpDiscoveryEndpoint () { Binding = binding };
		}
	}
}
