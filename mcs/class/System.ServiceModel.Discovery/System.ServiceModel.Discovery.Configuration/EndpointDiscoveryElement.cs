using System;
using System.ComponentModel;
using System.Configuration;
using System.ServiceModel.Configuration;

namespace System.ServiceModel.Discovery.Configuration
{
	public sealed class EndpointDiscoveryElement : BehaviorExtensionElement
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty types, enabled, extensions, scopes;
		
		static EndpointDiscoveryElement ()
		{
			types = new ConfigurationProperty ("types", typeof (ContractTypeNameElementCollection), null, null, null, ConfigurationPropertyOptions.None);
			enabled = new ConfigurationProperty ("enabled", typeof (bool), null, null, null, ConfigurationPropertyOptions.None);
			extensions = new ConfigurationProperty ("extensions", typeof (XmlElementElementCollection), null, null, null, ConfigurationPropertyOptions.None);
			scopes = new ConfigurationProperty ("scopes", typeof (ScopeElementCollection), null, null, null, ConfigurationPropertyOptions.None);
			properties = new ConfigurationPropertyCollection ();
			properties.Add (types);
			properties.Add (enabled);
			properties.Add (extensions);
			properties.Add (scopes);
		}

		public EndpointDiscoveryElement ()
		{
		}
		
		public override Type BehaviorType {
			get { return typeof (EndpointDiscoveryBehavior); }
		}

		[ConfigurationProperty ("types")]
		public ContractTypeNameElementCollection ContractTypeNames {
			get { return (ContractTypeNameElementCollection) base [types]; }
		}
		
		[ConfigurationPropertyAttribute("enabled", DefaultValue = true)]
		public bool Enabled {
			get { return (bool) base [enabled]; }
			set { base [enabled] = value; }
		}
		
		[ConfigurationPropertyAttribute("extensions")]
		public XmlElementElementCollection Extensions {
			get { return (XmlElementElementCollection) base [extensions]; }
		}
		
		[ConfigurationPropertyAttribute("scopes")]
		public ScopeElementCollection Scopes {
			get { return (ScopeElementCollection) base [scopes]; }
		}
		
		protected override object CreateBehavior ()
		{
			throw new NotImplementedException ();
		}
	}
}

