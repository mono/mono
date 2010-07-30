using System;
using System.ComponentModel;
using System.Configuration;
using System.ServiceModel.Configuration;

namespace System.ServiceModel.Discovery.Configuration
{
	public sealed class DiscoveryClientElement : BindingElementExtensionElement
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty endpoint, find_criteria;
		
		static DiscoveryClientElement ()
		{
			endpoint = new ConfigurationProperty ("endpoint", typeof (ChannelEndpointElement), null, null, null, ConfigurationPropertyOptions.None);
			find_criteria = new ConfigurationProperty ("findCriteria", typeof (FindCriteriaElement), null, null, null, ConfigurationPropertyOptions.None);
			properties = new ConfigurationPropertyCollection ();
			properties.Add (endpoint);
			properties.Add (find_criteria);
		}

		public DiscoveryClientElement ()
		{
		}
		
		public override Type BindingElementType {
			get { return typeof (DiscoveryClientBindingElement); }
		}

		[ConfigurationProperty ("endpoint")]
		public ChannelEndpointElement DiscoveryEndpoint {
			get { return (ChannelEndpointElement) Properties [endpoint]; }
		}

		[ConfigurationProperty ("findCriteria")]
		public FindCriteriaElement FindCriteria {
			get { return (FindCriteriaElement) Properties [find_criteria]; }
		}
	}
}

