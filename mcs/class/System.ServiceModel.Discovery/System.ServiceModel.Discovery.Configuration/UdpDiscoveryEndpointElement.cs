using System;
using System.ComponentModel;
using System.Configuration;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;

namespace System.ServiceModel.Discovery.Configuration
{
	public class UdpDiscoveryEndpointElement : DiscoveryEndpointElement
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty discovery_mode, max_response_delay, multicast_address, transport_settings;
		
		static UdpDiscoveryEndpointElement ()
		{
			discovery_mode = new ConfigurationProperty ("discoveryMode", typeof (ServiceDiscoveryMode), ServiceDiscoveryMode.Adhoc, null, null, ConfigurationPropertyOptions.None);
			max_response_delay = new ConfigurationProperty ("maxResponseDelay", typeof (TimeSpan), "00:00:00.500", new TimeSpanConverter (), null, ConfigurationPropertyOptions.None);
			multicast_address = new ConfigurationProperty ("multicastAddress", typeof (Uri), "soap.udp://239.255.255.250:3702", new UriTypeConverter (), null, ConfigurationPropertyOptions.None);
			transport_settings = new ConfigurationProperty ("transportSettings", typeof (UdpTransportSettingsElement), null, null, null, ConfigurationPropertyOptions.None);
			properties = new ConfigurationPropertyCollection ();
			properties.Add (discovery_mode);
			properties.Add (max_response_delay);
			properties.Add (multicast_address);
			properties.Add (transport_settings);
		}

		public UdpDiscoveryEndpointElement ()
		{
		}

		[ConfigurationProperty ("discoveryMode", DefaultValue = ServiceDiscoveryMode.Adhoc)]
		public ServiceDiscoveryMode DiscoveryMode {
			get { return (ServiceDiscoveryMode) base [discovery_mode]; }
			set { base [discovery_mode] = value; }
		}

		protected override Type EndpointType {
			get { return typeof (UdpDiscoveryEndpoint); }
		}

		[ConfigurationProperty ("maxResponseDelay", DefaultValue = "00:00:00.500")]
		public TimeSpan MaxResponseDelay {
			get { return (TimeSpan) base [max_response_delay]; }
			set { base [max_response_delay] = value; }
		}

		[ConfigurationProperty ("multicastAddress", DefaultValue = "soap.udp://239.255.255.250:3702")]
		public Uri MulticastAddress {
			get { return (Uri) base [multicast_address]; }
			set { base [multicast_address] = value; }
		}

		[ConfigurationProperty ("transportSettings")]
		public UdpTransportSettingsElement TransportSettings {
			get { return (UdpTransportSettingsElement) base [transport_settings]; }
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

