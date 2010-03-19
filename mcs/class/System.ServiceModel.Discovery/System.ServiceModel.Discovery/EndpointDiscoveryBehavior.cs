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
	public class EndpointDiscoveryBehavior : IEndpointBehavior
	{
		void IEndpointBehavior.AddBindingParameters (ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
		{
		}

		void IEndpointBehavior.ApplyClientBehavior (ServiceEndpoint endpoint, ClientRuntime clientRuntime)
		{
		}

		void IEndpointBehavior.ApplyDispatchBehavior (ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
		{
		}

		void IEndpointBehavior.Validate (ServiceEndpoint endpoint)
		{
		}
	}
}
