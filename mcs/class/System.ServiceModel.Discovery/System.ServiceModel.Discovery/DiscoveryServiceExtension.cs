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
	public abstract class DiscoveryServiceExtension : IExtension<ServiceHostBase>
	{
		protected abstract DiscoveryService GetDiscoveryService ();

		void IExtension<ServiceHostBase>.Attach (ServiceHostBase owner)
		{
		}

		void IExtension<ServiceHostBase>.Detach (ServiceHostBase owner)
		{
		}
	}
}
