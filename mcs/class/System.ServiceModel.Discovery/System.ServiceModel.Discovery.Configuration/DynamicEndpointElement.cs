using System;
using System.ComponentModel;
using System.Configuration;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;

namespace System.ServiceModel.Discovery.Configuration
{
	public sealed class DynamicEndpointElement : StandardEndpointElement
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty discovery_client_settings;
		
		static DynamicEndpointElement ()
		{
			discovery_client_settings = new ConfigurationProperty ("discoveryClientSettings", typeof (DiscoveryClientSettingsElement), null, null, null, ConfigurationPropertyOptions.None);
			properties = new ConfigurationPropertyCollection ();
			properties.Add (discovery_client_settings);
		}
		
		public DynamicEndpointElement ()
		{
		}

		[ConfigurationProperty ("discoveryClientSettings")]
		public DiscoveryClientSettingsElement DiscoveryClientSettings {
			get { return (DiscoveryClientSettingsElement) base [discovery_client_settings]; }
		}
		
		protected override Type EndpointType {
			get { return typeof (DynamicEndpoint); }
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
		
		protected override void OnInitializeAndValidate (ServiceEndpointElement serviceEndpointElement)
		{
			throw new NotImplementedException ();
		}
	}
}

