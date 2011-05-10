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

		[MonoTODO ("MetadataExchangeClientMode is not considered")]
		public MetadataExchangeClient (Uri address, MetadataExchangeClientMode mode)
		{
			this.address = new EndpointAddress (address.AbsoluteUri);
			this.mode = mode;
		}

		[MonoTODO]
		public ICredentials HttpCredentials { get; set; }
		[MonoTODO]
		public int MaximumResolvedReferences { get; set; }

		public TimeSpan OperationTimeout { get; set; }

		[MonoTODO]
		public bool ResolveMetadataReferences { get; set; }

		public ClientCredentials SoapCredentials { get; set; }

		[MonoTODO ("use dialect and identifier (but how?)")]
		protected internal virtual ChannelFactory<IMetadataExchange> GetChannelFactory (EndpointAddress metadataAddress, string dialect, string identifier)
		{
			if (metadataAddress == null)
				throw new ArgumentNullException ("metadataAddress");

			var se = new ServiceEndpoint (ContractDescription.GetContract (typeof (IMetadataExchange)), CreateBinding (metadataAddress), metadataAddress);
			if (SoapCredentials != null) {
				se.Behaviors.RemoveAll<ClientCredentials> ();
				se.Behaviors.Add (SoapCredentials);
			}
			return new ChannelFactory<IMetadataExchange> (se);
		}

		[MonoTODO]
		protected internal virtual HttpWebRequest GetWebRequest (Uri location, string dialect, string identifier)
		{
			throw new NotImplementedException ();
		}

		SMBinding CreateBinding (EndpointAddress address)
		{
			return address.Uri.Scheme == Uri.UriSchemeHttps ?
				MetadataExchangeBindings.CreateMexHttpsBinding () :
				MetadataExchangeBindings.CreateMexHttpBinding ();
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
			// FIXME: give dialect and identifier
			var cf = GetChannelFactory (address, null, null);
			cf.Open ();
			var proxy = cf.CreateChannel ();
			var asClientChannel = proxy as IClientChannel;
			if (asClientChannel == null)
				throw new InvalidOperationException ("The channel factory must return an IClientChannel implementation");
			asClientChannel.OperationTimeout = OperationTimeout;
			asClientChannel.Open ();

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

	interface IMetadataExchangeClient : IMetadataExchange, IClientChannel
	{
	}
}
