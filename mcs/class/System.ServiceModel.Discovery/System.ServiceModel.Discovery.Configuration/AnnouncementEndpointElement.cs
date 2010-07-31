using System;
using System.ComponentModel;
using System.Configuration;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;

namespace System.ServiceModel.Discovery.Configuration
{
	public class AnnouncementEndpointElement : StandardEndpointElement
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty discovery_version, max_announcement_delay;
		
		static AnnouncementEndpointElement ()
		{
			discovery_version = new ConfigurationProperty ("discoveryVersion", typeof (DiscoveryVersion), "WSDiscovery11", new DiscoveryVersionConverter (), null, ConfigurationPropertyOptions.None);
			max_announcement_delay = new ConfigurationProperty ("maxAnnouncementDelay", typeof (TimeSpan), "00:00:00", new TimeSpanConverter (), null, ConfigurationPropertyOptions.None);
			properties = new ConfigurationPropertyCollection ();
			properties.Add (discovery_version);
			properties.Add (max_announcement_delay);
		}
		
		public AnnouncementEndpointElement ()
		{
		}

		[TypeConverter (typeof (DiscoveryVersionConverter))]
		[ConfigurationProperty ("discoveryVersion", DefaultValue = "WSDiscovery11")]
		public DiscoveryVersion DiscoveryVersion {
			get { return (DiscoveryVersion) base [discovery_version]; }
			set { base [discovery_version] = value; }
		}

		protected override Type EndpointType {
			get { return typeof (AnnouncementEndpoint); }
		}
		
		[TypeConverter (typeof (TimeSpanConverter))]
		[ConfigurationProperty ("maxAnnouncementDelay", DefaultValue = "00:00:00")]
		public TimeSpan MaxAnnouncementDelay {
			get { return (TimeSpan) base [max_announcement_delay]; }
			set { base [max_announcement_delay] = value; }
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

