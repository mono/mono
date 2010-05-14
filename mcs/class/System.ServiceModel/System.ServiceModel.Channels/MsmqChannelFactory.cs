//
// MsmqChannelFactory.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.  http://www.novell.com
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
	internal class MsmqChannelFactory<TChannel> : ChannelFactoryBase<TChannel>
	{
		// not sure if they are required.
		MsmqTransportBindingElement source;
		MessageEncoder encoder;

		public MsmqChannelFactory (MsmqTransportBindingElement source, BindingContext ctx)
		{
			this.source = source;
			foreach (BindingElement be in ctx.Binding.Elements) {
				MessageEncodingBindingElement mbe = be as MessageEncodingBindingElement;
				if (mbe != null) {
					encoder = CreateEncoder<TChannel> (mbe);
					break;
				}
			}
			if (encoder == null)
				encoder = new BinaryMessageEncoder ();
		}

		public MessageEncoder MessageEncoder {
			get { return encoder; }
		}

		protected override TChannel OnCreateChannel (
			EndpointAddress address, Uri via)
		{
			ThrowIfDisposedOrNotOpen ();

			if (source.Scheme != address.Uri.Scheme)
				throw new ArgumentException (String.Format ("Argument EndpointAddress has unsupported URI scheme: {0}", address.Uri.Scheme));

			Type t = typeof (TChannel);
			if (t == typeof (IOutputChannel))
				return (TChannel) (object) new MsmqOutputChannel ((MsmqChannelFactory<IOutputChannel>) (object) this, address, via);
			else if (t == typeof (IOutputSessionChannel))
				// FIXME: implement
				throw new NotImplementedException ();
			throw new InvalidOperationException (String.Format ("channel type {0} is not supported.", typeof (TChannel).Name));
		}

		protected override IAsyncResult OnBeginOpen (TimeSpan timeout,
			AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		protected override void OnOpen (TimeSpan timeout)
		{
		}
	}
}
