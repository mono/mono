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
	// This class is for custom implementation.
	// It is used by ServiceDiscoveryBehavior to find an extension of this
	// type, to call GetDiscoveryService().
	// See http://msdn.microsoft.com/en-us/library/system.servicemodel.discovery.discoveryserviceextension.aspx
	public abstract class DiscoveryServiceExtension : IExtension<ServiceHostBase>
	{
		protected DiscoveryServiceExtension ()
		{
			PublishedInternalEndpoints = new Collection<EndpointDiscoveryMetadata> ();
			PublishedEndpoints = new ReadOnlyCollection<EndpointDiscoveryMetadata> (PublishedInternalEndpoints);
		}

		internal Collection<EndpointDiscoveryMetadata> PublishedInternalEndpoints { get; private set; }
		
		internal DiscoveryService Service { get; private set; }

		public ReadOnlyCollection<EndpointDiscoveryMetadata> PublishedEndpoints { get; private set; }

		protected abstract DiscoveryService GetDiscoveryService ();

		void IExtension<ServiceHostBase>.Attach (ServiceHostBase owner)
		{
			// FIXME: use it somewhere
			Service = GetDiscoveryService ();
		}

		void IExtension<ServiceHostBase>.Detach (ServiceHostBase owner)
		{
		}

		internal class DefaultDiscoveryServiceExtension : DiscoveryServiceExtension
		{
			protected override DiscoveryService GetDiscoveryService ()
			{
				return new DefaultDiscoveryService ();
			}
		}
	}
}
