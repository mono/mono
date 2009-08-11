//
// MetadataExchangeClient.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Ankit Jain <jankit@novell.com>
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
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Web.Services.Description;
using System.Web.Services.Discovery;
using System.Web.Services.Protocols;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Net;
using System.Text;

using SMBinding = System.ServiceModel.Channels.Binding;
using SMMessage = System.ServiceModel.Channels.Message;

namespace System.ServiceModel.Description
{
	[MonoTODO ("MetadataExchangeClientMode is not considered")]
	public class MetadataExchangeClient
	{
		string scheme;

		EndpointAddress address;
		SMBinding binding;
		MetadataExchangeClientMode mode = MetadataExchangeClientMode.MetadataExchange;

		// constructors

		[MonoTODO ("use empty configuration")]
		public MetadataExchangeClient ()
		{
		}

		public MetadataExchangeClient (SMBinding mexBinding)
		{
			binding = mexBinding;
		}

		public MetadataExchangeClient (EndpointAddress address)
		{
			this.address = address;
		}

		public MetadataExchangeClient (string endpointConfigurationName)
		{
			throw new NotImplementedException ();
		}

		public MetadataExchangeClient (Uri address, MetadataExchangeClientMode mode)
		{
			this.address = new EndpointAddress (address.AbsoluteUri);
			this.mode = mode;
		}

		// sync methods

		public MetadataSet GetMetadata ()
		{
			return GetMetadata (address);
		}

		public MetadataSet GetMetadata (EndpointAddress address)
		{
			//FIXME: default mode?
			return GetMetadataInternal (address, mode);
		}

		public MetadataSet GetMetadata (Uri address, MetadataExchangeClientMode mode)
		{
			return GetMetadataInternal (new EndpointAddress (address.AbsoluteUri), mode);
		}

		internal MetadataSet GetMetadataInternal (EndpointAddress address, MetadataExchangeClientMode mode)
		{
			if (binding == null)
				binding = MetadataExchangeBindings.CreateMexHttpBinding ();

			MetadataProxy proxy = new MetadataProxy (binding, address);
			proxy.Open ();

			SMMessage msg = SMMessage.CreateMessage ( 
					MessageVersion.Soap12WSAddressing10, 
					"http://schemas.xmlsoap.org/ws/2004/09/transfer/Get");

			msg.Headers.ReplyTo = new EndpointAddress (
					"http://www.w3.org/2005/08/addressing/anonymous");
			//msg.Headers.From = new EndpointAddress ("http://localhost");
			msg.Headers.To = address.Uri;
			msg.Headers.MessageId = new UniqueId ();

			SMMessage ret;
			try {
				ret = proxy.Get (msg);
			} catch (Exception e) {
				throw new InvalidOperationException (
						"Metadata contains a reference that cannot be resolved : " + address.Uri.AbsoluteUri, e);
			}

			return MetadataSet.ReadFrom (ret.GetReaderAtBodyContents ());
		}

		// async methods

		Func<Func<MetadataSet>,MetadataSet> getter;

		void PrepareGetter ()
		{
			if (getter == null)
				getter = new Func<Func<MetadataSet>,MetadataSet> (GetMetadata);
		}

		public MetadataSet EndGetMetadata (IAsyncResult result)
		{
			return getter.EndInvoke (result);
		}

		MetadataSet GetMetadata (Func<MetadataSet> func)
		{
			return func ();
		}

		public IAsyncResult BeginGetMetadata (AsyncCallback callback, object asyncState)
		{
			PrepareGetter ();
			return getter.BeginInvoke (() => GetMetadata (), callback, asyncState);
		}

		public IAsyncResult BeginGetMetadata (EndpointAddress address, AsyncCallback callback, object asyncState)
		{
			PrepareGetter ();
			return getter.BeginInvoke (() => GetMetadata (address), callback, asyncState);
		}

		public IAsyncResult BeginGetMetadata (Uri address, MetadataExchangeClientMode mode, AsyncCallback callback, object asyncState)
		{
			PrepareGetter ();
			return getter.BeginInvoke (() => GetMetadata (address, mode), callback, asyncState);
		}
	}
	
	internal class MetadataProxy : ClientBase<IMetadataExchange>, IMetadataExchange
	{
		public MetadataProxy (SMBinding binding, EndpointAddress address)
			: base (binding, address)
		{
		}

		public SMMessage Get (SMMessage msg)
		{
			return Channel.Get (msg);
		}

		public IAsyncResult BeginGet (SMMessage request, AsyncCallback callback , object state)
		{
			throw new NotImplementedException ();
		}

		public SMMessage EndGet (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}
	}

}
