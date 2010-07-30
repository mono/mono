using System;
using System.ComponentModel;
using System.Configuration;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;

namespace System.ServiceModel.Discovery.Configuration
{
	public class DiscoveryEndpointElement : StandardEndpointElement
	{
		public DiscoveryEndpointElement ()
		{
		}

		protected override ServiceEndpoint CreateServiceEndpoint (ContractDescription contractDescription)
		{
			throw new NotImplementedException ();
		}

		protected override void InitializeFrom (ServiceEndpoint endpoint)
		{
			throw new NotImplementedException ();
		}
		
		protected override void OnApplyConfiguration (ServiceEndpoint endpoint, ChannelEndpointElement serviceEndpointElement)
		{
			throw new NotImplementedException ();
		}
		
		protected override void OnApplyConfiguration (ServiceEndpoint endpoint, ServiceEndpointElement serviceEndpointElement)
		{
			throw new NotImplementedException ();
		}
		
		protected override void OnInitializeAndValidate (ChannelEndpointElement channelEndpointElement)
		{
			throw new NotImplementedException ();
		}
		
		protected override void OnInitializeAndValidate (ServiceEndpointElement channelEndpointElement)
		{
			throw new NotImplementedException ();
		}
	}
}

