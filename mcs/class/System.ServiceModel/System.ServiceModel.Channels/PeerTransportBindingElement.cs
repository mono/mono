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
		: TransportBindingElement, IPolicyExportExtension, IWsdlExportExtension
	{
		long max_recv_message_size = 0x10000;
		int port;
		PeerSecuritySettings security = new PeerSecuritySettings ();

		public PeerTransportBindingElement ()
		{
		}

		private PeerTransportBindingElement (
			PeerTransportBindingElement other)
			: base (other)
		{
			max_recv_message_size = other.max_recv_message_size;
			port = other.port;
			other.security.CopyTo (security);
		}

		public IPAddress ListenIPAddress { get; set; }

		public override long MaxReceivedMessageSize {
			get { return max_recv_message_size; }
			set { max_recv_message_size = value; }
		}

		public int Port {
			get { return port; }
			set { port = value; }
		}

		public override string Scheme {
			get { return "net.p2p"; }
		}

		public override bool CanBuildChannelFactory<TChannel> (
			BindingContext context)
		{
			return  typeof (TChannel) == typeof (IOutputChannel) ||
				typeof (TChannel) == typeof (IDuplexChannel);
		}

		public override bool CanBuildChannelListener<TChannel> (
			BindingContext context)
		{
			return  typeof (TChannel) == typeof (IInputChannel) ||
				typeof (TChannel) == typeof (IDuplexChannel);
		}

		public override IChannelFactory<TChannel> BuildChannelFactory<TChannel> (
			BindingContext context)
		{
			if (!CanBuildChannelFactory<TChannel> (context))
				throw new ArgumentException (String.Format ("Not supported channel type '{0}'", typeof (TChannel)));
			if (typeof (TChannel) == typeof (IOutputChannel))
				return (IChannelFactory<TChannel>) (object) new PeerChannelFactory<IOutputChannel> (this, context);
			else if (typeof (TChannel) == typeof (IDuplexChannel))
				return (IChannelFactory<TChannel>) (object) new PeerChannelFactory<IDuplexChannel> (this, context);
			throw new InvalidOperationException (String.Format ("Not supported channel '{0}' (is incorrectly allowed at construction time)", typeof (TChannel)));
		}

		public override IChannelListener<TChannel> BuildChannelListener<TChannel> (
			BindingContext context)
		{
			if (!CanBuildChannelListener<TChannel> (context))
				throw new ArgumentException (String.Format ("Not supported channel type '{0}'", typeof (TChannel)));

			// FIXME: check LocalIPAddress.

			if (typeof (TChannel) == typeof (IInputChannel))
				return (IChannelListener<TChannel>) (object) new PeerChannelListener<IInputChannel> (this, context);
			else if (typeof (TChannel) == typeof (IDuplexChannel))
				return (IChannelListener<TChannel>) (object) new PeerChannelListener<IDuplexChannel> (this, context);
			throw new InvalidOperationException (String.Format ("Not supported channel '{0}' (is incorrectly allowed at construction time)", typeof (TChannel)));
		}

		public override BindingElement Clone ()
		{
			return new PeerTransportBindingElement (this);
		}

		public override T GetProperty<T> (BindingContext context)
		{
			if (typeof (T) == typeof (IBindingMulticastCapabilities))
				return (T) (object) this;
			if (typeof (T) == typeof (ISecurityCapabilities))
				return (T) (object) this;
			if (typeof (T) == typeof (IBindingDeliveryCapabilities))
				return (T) (object) this;

			return base.GetProperty<T> (context);
		}

		public PeerSecuritySettings Security {
			get { return security; }
		}

		[MonoTODO]
		void IPolicyExportExtension.ExportPolicy (MetadataExporter exporter, PolicyConversionContext contxt)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IWsdlExportExtension.ExportEndpoint (WsdlExporter exporter, WsdlEndpointConversionContext context)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IWsdlExportExtension.ExportContract (WsdlExporter exporter, WsdlContractConversionContext context)
		{
			throw new NotImplementedException ();
		}
	}
}
