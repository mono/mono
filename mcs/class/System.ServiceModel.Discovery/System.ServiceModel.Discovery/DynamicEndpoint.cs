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
	public class DynamicEndpoint : ServiceEndpoint
	{
		public DynamicEndpoint (ContractDescription contract, Binding binding)
			: base (contract, binding, new EndpointAddress ("http://schemas.microsoft.com/discovery/dynamic"))
		{
			if (binding == null)
				throw new ArgumentNullException ("binding");
			DiscoveryEndpointProvider = new UdpDiscoveryEndpointProvider ();
		}

		public DiscoveryEndpointProvider DiscoveryEndpointProvider { get; set; }
		public FindCriteria FindCriteria { get; set; }
	}
}
