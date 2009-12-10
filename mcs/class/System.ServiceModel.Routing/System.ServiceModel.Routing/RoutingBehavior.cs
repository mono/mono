using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Routing
{
	public sealed class RoutingBehavior : IServiceBehavior
	{
		// seealso http://msdn.microsoft.com/en-us/library/ee517422%28VS.100%29.aspx

		[MonoTODO]
		public static Type GetContractForDescription (ContractDescription description)
		{
			throw new NotImplementedException ();
		}

		// instance members

		public RoutingBehavior (RoutingConfiguration routingConfiguration)
		{
			if (routingConfiguration == null)
				throw new ArgumentNullException ("routingConfiguration");
			config = routingConfiguration;
		}

		RoutingConfiguration config;

		void IServiceBehavior.AddBindingParameters (ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
		{
			// nothing to do here.
		}

		void IServiceBehavior.ApplyDispatchBehavior (ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
			// FIXME: not sure if this is correct.
			if (config.SoapProcessingEnabled)
				foreach (var ses in config.FilterTable.Values)
					foreach (var se in ses)
						se.Behaviors.Add (new SoapProcessingBehavior ());

			var ext = new RoutingExtension ();
			((IExtension<ServiceHostBase>) ext).Attach (serviceHostBase);
			ext.ApplyConfiguration (config);
		}

		void IServiceBehavior.Validate (ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
			if (!serviceDescription.ServiceType.IsAssignableFrom (typeof (RoutingService)))
				throw new InvalidOperationException ("RoutingBehavior can be applied only to RoutingService");
		}
	}
}
