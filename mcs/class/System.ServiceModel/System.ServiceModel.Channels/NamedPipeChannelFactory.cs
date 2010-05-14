//
// NamedPipeChannelFactory.cs
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
using System.Xml;

namespace System.ServiceModel.Channels
{
	internal class NamedPipeChannelFactory<TChannel> : TransportChannelFactoryBase<TChannel>
	{
		NamedPipeTransportBindingElement source;
		XmlDictionaryReaderQuotas quotas;

		public NamedPipeChannelFactory (NamedPipeTransportBindingElement source, BindingContext ctx)
			: base (source, ctx)
		{
			foreach (BindingElement be in ctx.Binding.Elements) {
				MessageEncodingBindingElement mbe = be as MessageEncodingBindingElement;
				if (mbe != null) {
					MessageEncoder = CreateEncoder<TChannel> (mbe);
					quotas = mbe.GetProperty<XmlDictionaryReaderQuotas> (ctx);
					break;
				}
			}
			if (MessageEncoder == null)
				MessageEncoder = new BinaryMessageEncoder ();

			this.source = source;
		}

		protected override TChannel OnCreateChannel (
			EndpointAddress address, Uri via)
		{
			ThrowIfDisposedOrNotOpen ();

			var targetUri = via ?? address.Uri;
			if (source.Scheme != targetUri.Scheme)
				throw new ArgumentException (String.Format ("Argument EndpointAddress has unsupported URI scheme: {0}", targetUri.Scheme));
			if (!targetUri.IsLoopback)
				throw new NotSupportedException ("Only local namde pipes are supported in this binding");

			// FIXME: implement duplex session channel.
//			if (typeof (TChannel) == typeof (IDuplexSessionChannel))
//				return (TChannel) (object) new NamedPipeDuplexSessionChannel (this, address, via);

			if (typeof (TChannel) == typeof (IRequestChannel))
				return (TChannel) (object) new NamedPipeRequestChannel (this, MessageEncoder, address, targetUri);

			throw new InvalidOperationException (String.Format ("Channel type {0} is not supported.", typeof (TChannel).Name));
		}
	}
}
