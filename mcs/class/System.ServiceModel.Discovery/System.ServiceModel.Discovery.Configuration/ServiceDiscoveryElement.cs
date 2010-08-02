using System;
using System.ComponentModel;
using System.Configuration;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;

namespace System.ServiceModel.Discovery.Configuration
{
	public sealed class ServiceDiscoveryElement : BehaviorExtensionElement
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty announcement_endpoints;
		
		static ServiceDiscoveryElement ()
		{
			announcement_endpoints = new ConfigurationProperty ("announcementEndpoints", typeof (AnnouncementChannelEndpointElementCollection), null, null, null, ConfigurationPropertyOptions.None);
			properties = new ConfigurationPropertyCollection ();
			properties.Add (announcement_endpoints);
		}

		public ServiceDiscoveryElement ()
		{
		}

		[ConfigurationProperty ("announcementEndpoints")]
		public AnnouncementChannelEndpointElementCollection AnnouncementEndpoints {
			get { return (AnnouncementChannelEndpointElementCollection) base [announcement_endpoints]; }
		}
		public override Type BehaviorType {
			get { return typeof (ServiceDiscoveryBehavior); }
		}
		
		protected override object CreateBehavior ()
		{
			throw new NotImplementedException ();
		}
	}
}

