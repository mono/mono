using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Routing
{
	public sealed class RoutingExtension : IExtension<ServiceHostBase>
	{
		class InstanceProvider : IInstanceProvider
		{
			public InstanceProvider (RoutingConfiguration config)
			{
				this.config = config;
			}

			RoutingConfiguration config;

			public object GetInstance (InstanceContext instanceContext)
			{
				return new RoutingService () { Configuration = config };
			}

			public object GetInstance (InstanceContext instanceContext, Message message)
			{
				return GetInstance (instanceContext);
			}

			public void ReleaseInstance (InstanceContext instanceContext, object instance)
			{
			}
		}

		internal RoutingExtension ()
		{
		}

		ServiceHostBase host;
		RoutingConfiguration configuration;

		public void ApplyConfiguration (RoutingConfiguration routingConfiguration)
		{
			if (routingConfiguration == null)
				throw new ArgumentNullException ("routingConfiguration");
			configuration = routingConfiguration;

			if (host == null)
				return;

			host.Opened += delegate {
				foreach (ChannelDispatcher cd in host.ChannelDispatchers)
					foreach (var ed in cd.Endpoints)
						if (ed.ContractNamespace == "http://schemas.microsoft.com/netfx/2009/05/routing")
							ed.DispatchRuntime.InstanceProvider = new InstanceProvider (configuration);
				};
		}

		void IExtension<ServiceHostBase>.Attach (ServiceHostBase owner)
		{
			host = owner;
			if (configuration != null)
				ApplyConfiguration (configuration);
		}

		void IExtension<ServiceHostBase>.Detach (ServiceHostBase owner)
		{
			host = null;
		}
	}
}
