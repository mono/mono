// 
// TcpChannelFactory.cs
// 
// Author: 
//     Marcos Cobena (marcoscobena@gmail.com)
// 
// Copyright 2007 Marcos Cobena (http://www.youcannoteatbits.org/)
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
	internal class TcpChannelInfo
	{
		public TcpChannelInfo (TransportBindingElement element, MessageEncoder encoder, XmlDictionaryReaderQuotas readerQuotas)
		{
			this.BindingElement = element;
			this.MessageEncoder = encoder;
			this.ReaderQuotas = readerQuotas ?? new XmlDictionaryReaderQuotas ();
		}

		public TransportBindingElement BindingElement { get; private set; }

		public MessageEncoder MessageEncoder { get; private set; }

		public XmlDictionaryReaderQuotas ReaderQuotas { get; private set; }
	}

	internal class TcpChannelFactory<TChannel> : TransportChannelFactoryBase<TChannel>
	{
		TcpChannelInfo info;

		public TcpChannelFactory (TcpTransportBindingElement source, BindingContext ctx)
			: base (source, ctx)
		{
			XmlDictionaryReaderQuotas quotas = null;
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
			info = new TcpChannelInfo (source, MessageEncoder, quotas);
		}

		protected override TChannel OnCreateChannel (
			EndpointAddress address, Uri via)
		{			
			ThrowIfDisposedOrNotOpen ();

			var targetUri = via ?? address.Uri;
			if (info.BindingElement.Scheme != targetUri.Scheme)
				throw new ArgumentException (String.Format ("Argument EndpointAddress has unsupported URI scheme: {0}", targetUri.Scheme));

			Type t = typeof (TChannel);
			
			if (t == typeof (IDuplexSessionChannel))
				return (TChannel) (object) new TcpDuplexSessionChannel (this, info, address, targetUri);
			
			if (t == typeof (IRequestChannel))
				return (TChannel) (object) new TcpRequestChannel (this, info, address, targetUri);

			throw new InvalidOperationException (String.Format ("Channel type {0} is not supported.", typeof (TChannel).Name));
		}
	}
}
