using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Discovery
{
	[MonoTODO]
	public class ServiceDiscoveryBehavior : IServiceBehavior
	{
		void IServiceBehavior.AddBindingParameters (ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
		{
		}

		void IServiceBehavior.ApplyDispatchBehavior (ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
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
		}
	}
}
