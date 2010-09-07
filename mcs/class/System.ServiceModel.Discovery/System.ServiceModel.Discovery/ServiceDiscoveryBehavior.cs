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
	public class ServiceDiscoveryBehavior : IServiceBehavior
	{
		public ServiceDiscoveryBehavior ()
		{
			AnnouncementEndpoints = new Collection<AnnouncementEndpoint> ();
		}
		
		public Collection<AnnouncementEndpoint> AnnouncementEndpoints { get; private set; }

		void IServiceBehavior.AddBindingParameters (ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
		{
			if (serviceDescription == null)
				throw new ArgumentNullException ("serviceDescription");
			if (serviceHostBase == null)
				throw new ArgumentNullException ("serviceHostBase");
			if (endpoints == null)
				throw new ArgumentNullException ("endpoints");
			if (bindingParameters == null)
				throw new ArgumentNullException ("bindingParameters");

			// do nothing
		}

		void IServiceBehavior.ApplyDispatchBehavior (ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
			if (serviceDescription == null)
				throw new ArgumentNullException ("serviceDescription");
			if (serviceHostBase == null)
				throw new ArgumentNullException ("serviceHostBase");

			// FIXME: add ChannelDispatchers (via extension)
			foreach (var ae in AnnouncementEndpoints) {
				serviceHostBase.ChannelDispatchers.Add (new DiscoveryChannelDispatcher (ae, true));
				serviceHostBase.ChannelDispatchers.Add (new DiscoveryChannelDispatcher (ae, false));
			}

			// BTW, on .NET, calling this method again adds endpoints more than one time...
		}

		void IServiceBehavior.Validate (ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
			if (serviceHostBase == null)
				throw new ArgumentNullException ("serviceHostBase");
			var dse = serviceHostBase.Extensions.Find<DiscoveryServiceExtension> ();
			if (dse == null) {
				dse = new DiscoveryServiceExtension.DefaultDiscoveryServiceExtension ();
				serviceHostBase.Extensions.Add (dse);
			}
			
			foreach (var se in serviceDescription.Endpoints)
				se.Behaviors.Add (new DiscoveryEndpointPublisherBehavior (dse));
		}
	}
}
