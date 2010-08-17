//
// Author: Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009,2010 Novell, Inc (http://www.novell.com)
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
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Discovery
{
	internal class DiscoveryEndpointPublisherBehavior : IEndpointBehavior
	{
		DiscoveryServiceExtension extension;

		internal DiscoveryEndpointPublisherBehavior (DiscoveryServiceExtension extension)
		{
			this.extension = extension;
		}

		void IEndpointBehavior.AddBindingParameters (ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
		{
			if (endpoint == null)
				throw new ArgumentNullException ("endpoint");
			if (bindingParameters == null)
				throw new ArgumentNullException ("bindingParameters");
		}

		void IEndpointBehavior.ApplyClientBehavior (ServiceEndpoint endpoint, ClientRuntime clientRuntime)
		{
			if (endpoint == null)
				throw new ArgumentNullException ("endpoint");
			if (clientRuntime == null)
				throw new ArgumentNullException ("clientRuntime");
		}

		void IEndpointBehavior.ApplyDispatchBehavior (ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
		{
			if (endpoint == null)
				throw new ArgumentNullException ("endpoint");
			if (endpointDispatcher == null)
				throw new ArgumentNullException ("endpointDispatcher");

			var edb = endpoint.Behaviors.Find<EndpointDiscoveryBehavior> ();
			if (edb != null && !edb.Enabled)
				return;

			var edm = EndpointDiscoveryMetadata.FromServiceEndpoint (endpoint);
			extension.PublishedInternalEndpoints.Add (edm);
		}

		void IEndpointBehavior.Validate (ServiceEndpoint endpoint)
		{
			if (endpoint == null)
				throw new ArgumentNullException ("endpoint");
		}
	}
}
