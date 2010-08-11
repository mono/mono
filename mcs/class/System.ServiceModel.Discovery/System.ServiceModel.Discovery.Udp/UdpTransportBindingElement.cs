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
using System.IO;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace System.ServiceModel.Discovery
{
	internal class UdpTransportBindingElement : TransportBindingElement
	{
		public UdpTransportBindingElement ()
		{
		}
		
		private UdpTransportBindingElement (UdpTransportBindingElement other)
		{
		}
		
		public override string Scheme {
			get { return "soap.udp"; }
		}
		
		public override BindingElement Clone ()
		{
			return new UdpTransportBindingElement (this);
		}
		
		public override bool CanBuildChannelFactory<TChannel> (BindingContext ctx)
		{
			return typeof (TChannel) == typeof (IDuplexChannel);
		}
		
		public override bool CanBuildChannelListener<TChannel> (BindingContext ctx)
		{
			return typeof (TChannel) == typeof (IDuplexChannel);
		}
		
		public override IChannelFactory<TChannel> BuildChannelFactory<TChannel> (BindingContext ctx)
		{
			if (!CanBuildChannelFactory<TChannel> (ctx))
				throw new InvalidOperationException (String.Format ("Not supported type of channel: {0}", typeof (TChannel)));

			return (IChannelFactory<TChannel>) (object) new UdpChannelFactory (this, ctx);
		}
		
		public override IChannelListener<TChannel> BuildChannelListener<TChannel> (BindingContext ctx)
		{
			if (!CanBuildChannelListener<TChannel> (ctx))
				throw new InvalidOperationException (String.Format ("Not supported type of channel: {0}", typeof (TChannel)));

			return (IChannelListener<TChannel>) (object) new UdpChannelListener (this, ctx);
		}
		
		public override T GetProperty<T> (BindingContext ctx)
		{
			return ctx.GetInnerProperty<T> ();
		}
	}
}
