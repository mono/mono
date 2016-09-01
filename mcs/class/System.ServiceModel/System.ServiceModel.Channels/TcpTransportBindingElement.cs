//
// TcpTransportBindingElement.cs
//
// Authors:
//	Atsushi Enomoto <atsushi@ximian.com>
//      Marcos Cobena (marcoscobena@gmail.com)
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
using System.ServiceModel.Channels;
#if !MOBILE && !XAMMAC_4_5
using System.ServiceModel.Channels.NetTcp;
#endif
using System.ServiceModel.Description;

namespace System.ServiceModel.Channels
{
	public class TcpTransportBindingElement
		: ConnectionOrientedTransportBindingElement
	{
		internal const int DefaultPort = 808;

		int listen_backlog = 10;
		bool port_sharing_enabled = false;
		bool teredo_enabled = false;
		TcpConnectionPoolSettings pool = new TcpConnectionPoolSettings ();

		public TcpTransportBindingElement ()
		{
		}

		protected TcpTransportBindingElement (
			TcpTransportBindingElement elementToBeCloned)
			: base (elementToBeCloned)
		{
			listen_backlog = elementToBeCloned.listen_backlog;
			port_sharing_enabled = elementToBeCloned.port_sharing_enabled;
			pool.CopyPropertiesFrom (elementToBeCloned.pool);
		}
		
		public TcpConnectionPoolSettings ConnectionPoolSettings {
			get { return pool; }
		}

		public int ListenBacklog {
			get { return listen_backlog; }
			set { listen_backlog = value; }
		}

		public bool PortSharingEnabled {
			get { return port_sharing_enabled; }
			set { port_sharing_enabled = value; }
		}

		public override string Scheme {
			get { return "net.tcp"; }
		}
		
		// As MSDN exposes, this' only available on Windows XP SP2 and Windows Server 2003
		public bool TeredoEnabled {
			get { return teredo_enabled; }
			set { teredo_enabled = value; }
		}

		public override IChannelFactory<TChannel> BuildChannelFactory<TChannel> (
			BindingContext context)
		{
			if (!CanBuildChannelFactory<TChannel> (context))
				throw new InvalidOperationException (String.Format ("Not supported channel factory type '{0}'", typeof (TChannel)));

#if !MOBILE && !XAMMAC_4_5
			return new TcpChannelFactory<TChannel> (this, context);
#else
			throw new NotImplementedException ();
#endif
		}

#if !MOBILE && !XAMMAC_4_5
		public override IChannelListener<TChannel>
			BuildChannelListener<TChannel> (
			BindingContext context)
		{
			if (!CanBuildChannelListener<TChannel> (context))
				throw new InvalidOperationException (String.Format ("Not supported channel listener type '{0}'", typeof (TChannel)));
			return new TcpChannelListener<TChannel> (this, context);
		}
#endif

		public override BindingElement Clone ()
		{
			return new TcpTransportBindingElement (this);
		}

		public override T GetProperty<T> (BindingContext context)
		{
			if (typeof (T) == typeof (IBindingDeliveryCapabilities))
				return (T) (object) new TcpBindingProperties (this);
			return base.GetProperty<T> (context);
		}
	}

	class TcpBindingProperties : IBindingDeliveryCapabilities
	{
		TcpTransportBindingElement source;

		public TcpBindingProperties (TcpTransportBindingElement source)
		{
			this.source = source;
		}

		public bool AssuresOrderedDelivery {
			get { return true; }
		}

		public bool QueuedDelivery {
			get { return false; }
		}
	}
}
