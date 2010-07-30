using System;
using System.ComponentModel;
using System.Configuration;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;

namespace System.ServiceModel.Discovery.Configuration
{
	public class UdpAnnouncementEndpointElement : AnnouncementEndpointElement
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty max_announcement_delay, multicast_address, transport_settings;
		
		static UdpAnnouncementEndpointElement ()
		{
			max_announcement_delay = new ConfigurationProperty ("maxAnnouncementDelay", typeof (TimeSpan), "00:00:00.500", new TimeSpanConverter (), null, ConfigurationPropertyOptions.None);
			multicast_address = new ConfigurationProperty ("multicastAddress", typeof (Uri), "soap.udp://239.255.255.250:3702", new UriTypeConverter (), null, ConfigurationPropertyOptions.None);
			transport_settings = new ConfigurationProperty ("transportSettings", typeof (UdpTransportSettingsElement), null, null, null, ConfigurationPropertyOptions.None);
			properties = new ConfigurationPropertyCollection ();
			properties.Add (max_announcement_delay);
			properties.Add (multicast_address);
			properties.Add (transport_settings);
		}
		
		public UdpAnnouncementEndpointElement ()
		{
		}
		
		protected override Type EndpointType {
			get { return typeof (UdpAnnouncementEndpoint); }
		}
		
		[TypeConverter (typeof (TimeSpanConverter))]
		[ConfigurationPropertyAttribute("maxAnnouncementDelay", DefaultValue = "00:00:00.500")]
		public TimeSpan MaxAnnouncementDelay {
			get { return Properties [max_announcement_delay]; }
			set { Properties [max_announcement_delay] = value; }
		}
		
		[TypeConverter (typeof (UriTypeConverter))]
		[ConfigurationPropertyAttribute("multicastAddress", DefaultValue = "soap.udp://239.255.255.250:3702")]
		public Uri MulticastAddress {
			get { return Properties [multicast_address]; }
			set { Properties [multicast_address] = value; }
		}
		
		[ConfigurationPropertyAttribute("transportSettings")]
		public UdpTransportSettingsElement TransportSettings {
			get { return Properties [transport_settings]; }
		}
	}
}

