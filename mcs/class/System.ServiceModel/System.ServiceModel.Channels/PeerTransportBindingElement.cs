//
// PeerTransportBindingElement.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.Net;
using System.Net.Security;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace System.ServiceModel.Channels
{
	[MonoTODO]
	public sealed class PeerTransportBindingElement
		: TransportBindingElement, ISecurityCapabilities
	{
		long max_recv_message_size;
		bool auth_msg;
		int port;

		public PeerTransportBindingElement ()
		{
			throw new NotImplementedException ();
		}

		private PeerTransportBindingElement (
			PeerTransportBindingElement other)
			: base (other)
		{
		}

		public IPAddress ListenIPAddress { get; set; }

		public override long MaxReceivedMessageSize {
			get { return max_recv_message_size; }
			set { max_recv_message_size = value; }
		}

		public bool MessageAuthentication {
			get { return auth_msg; }
			set { auth_msg = value; }
		}

		public int Port {
			get { return port; }
			set { port = value; }
		}

		public override string Scheme {
			get { return "net.p2p"; }
		}

		public override IChannelFactory<TChannel> BuildChannelFactory<TChannel> (
			BindingContext context)
		{
			throw new NotImplementedException ();
		}

		public override BindingElement Clone ()
		{
			return new PeerTransportBindingElement (this);
		}

		[MonoTODO]
		public override T GetProperty<T> (BindingContext context)
		{
			throw new NotImplementedException ();
		}

		ProtectionLevel ISecurityCapabilities.SupportedRequestProtectionLevel {
			get { throw new NotImplementedException (); }
		}

		ProtectionLevel ISecurityCapabilities.SupportedResponseProtectionLevel {
			get { throw new NotImplementedException (); }
		}

		bool ISecurityCapabilities.SupportsClientAuthentication {
			get { throw new NotImplementedException (); }
		}

		bool ISecurityCapabilities.SupportsClientWindowsIdentity {
			get { throw new NotImplementedException (); }
		}

		bool ISecurityCapabilities.SupportsServerAuthentication {
			get { throw new NotImplementedException (); }
		}
	}
}
