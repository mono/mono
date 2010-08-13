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
			endpoints = new Collection<EndpointDiscoveryMetadata> ();
			PublishedEndpoints = new ReadOnlyCollection<EndpointDiscoveryMetadata> (endpoints);
		}

		Collection<EndpointDiscoveryMetadata> endpoints;
		
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
