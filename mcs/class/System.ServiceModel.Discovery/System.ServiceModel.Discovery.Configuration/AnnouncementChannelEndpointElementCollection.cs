using System;
using System.ComponentModel;
using System.Configuration;
using System.ServiceModel.Configuration;

namespace System.ServiceModel.Discovery.Configuration
{
	[ConfigurationCollection (typeof (ChannelEndpointElement), AddItemName = "endpoint")]
	public sealed class AnnouncementChannelEndpointElementCollection : ServiceModelConfigurationElementCollection<ChannelEndpointElement>
	{
		public AnnouncementChannelEndpointElementCollection ()
		{
		}
	}
}

