//
// Author: Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Discovery
{
	internal class DiscoveryViaUriBehavior : IEndpointBehavior
	{
		public DiscoveryViaUriBehavior (DiscoveryVersion version, Uri via)
		{
			this.version = version;
			this.via = via;
		}
		
		DiscoveryVersion version;

		Uri via;
		
		public void AddBindingParameters (ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
		{
		}
		
		public void ApplyClientBehavior (ServiceEndpoint endpoint, ClientRuntime clientRuntime)
		{
			if (endpoint == null)
				throw new ArgumentNullException ("endpoint");
			if (clientRuntime == null)
				throw new ArgumentNullException ("clientRuntime");

			clientRuntime.Via = via;
			clientRuntime.MessageInspectors.Add (new ClientMessageInspector (version));
		}

		class ClientMessageInspector : IClientMessageInspector
		{
			public ClientMessageInspector (DiscoveryVersion version)
			{
				this.version = version;
			}
			
			DiscoveryVersion version;
			
			public object BeforeSendRequest (ref Message request, IClientChannel channel)
			{
				// overwrite To header with version-specific URN.
				request.Headers.To = version.AdhocAddress;
				return null;
			}

			public void AfterReceiveReply (ref Message reply, object correlationState)
			{
				// do nothing
			}
		}

		public void ApplyDispatchBehavior (ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
		{
		}
		
		public void Validate (ServiceEndpoint endpoint)
		{
		}
	}
}
