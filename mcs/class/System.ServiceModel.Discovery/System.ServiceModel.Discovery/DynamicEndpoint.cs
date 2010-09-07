using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Discovery
{
	public class DynamicEndpoint : ServiceEndpoint
	{
		public DynamicEndpoint (ContractDescription contract, Binding binding)
			: base (contract, CreateBinding (binding), new EndpointAddress ("http://schemas.microsoft.com/discovery/dynamic"))
		{
			if (binding == null)
				throw new ArgumentNullException ("binding");
			DiscoveryEndpointProvider = DiscoveryEndpointProvider.CreateDefault ();
			FindCriteria = new FindCriteria (contract.ContractType);
			
			IsSystemEndpoint = true;
		}

		static CustomBinding CreateBinding (Binding source)
		{
			var bec = source.CreateBindingElements ();
			bec.Insert (0, new DiscoveryClientBindingElement ());
			return new CustomBinding (bec);
		}

		public DiscoveryEndpointProvider DiscoveryEndpointProvider { get; set; }
		public FindCriteria FindCriteria { get; set; }
	}
}
