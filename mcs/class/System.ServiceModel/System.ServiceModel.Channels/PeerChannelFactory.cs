//
// PeerChannelFactory.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc.  http://www.novell.com
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
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Text;

namespace System.ServiceModel.Channels
{
	internal interface IPeerChannelManager
	{
		PeerTransportBindingElement Source { get; }
		PeerResolver Resolver { get; }
		MessageEncoder MessageEncoder { get; }
	}

	internal class PeerChannelFactory<TChannel> : TransportChannelFactoryBase<TChannel>, IPeerChannelManager
	{
		PeerTransportBindingElement source;
		
		public PeerChannelFactory (PeerTransportBindingElement source, BindingContext ctx)
			: base (source, ctx)
		{
			this.source = source;
			foreach (BindingElement be in ctx.Binding.Elements) {
				MessageEncodingBindingElement mbe = be as MessageEncodingBindingElement;
				if (mbe != null) {
					MessageEncoder = CreateEncoder<TChannel> (mbe);
					break;
				}
			}
			if (MessageEncoder == null)
				MessageEncoder = new BinaryMessageEncoder ();
		}

		public PeerResolver Resolver { get; set; }

		public PeerTransportBindingElement Source {
			get { return source; }
		}

		protected override TChannel OnCreateChannel (
			EndpointAddress address, Uri via)
		{
			ThrowIfDisposedOrNotOpen ();

			if (source.Scheme != address.Uri.Scheme)
				throw new ArgumentException (String.Format ("Argument EndpointAddress has unsupported URI scheme: {0}", address.Uri.Scheme));

			Type t = typeof (TChannel);
			if (t == typeof (IOutputChannel))
				return (TChannel) (object) new PeerDuplexChannel (this, address, via, Resolver);
			if (t == typeof (IDuplexChannel))
				return (TChannel) (object) new PeerDuplexChannel (this, address, via, Resolver);
			throw new InvalidOperationException (String.Format ("channel type {0} is not supported.", typeof (TChannel).Name));
		}
	}
}
