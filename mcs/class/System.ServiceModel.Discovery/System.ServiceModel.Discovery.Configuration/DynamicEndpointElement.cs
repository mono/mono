using System;
using System.ComponentModel;
using System.Configuration;
using System.ServiceModel.Configuration;

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
			get { return (DiscoveryClientSettingsElement) Prperties [discovery_client_settings]; }
		}
	}
}

