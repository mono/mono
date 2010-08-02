using System;
using System.ComponentModel;
using System.Configuration;
using System.ServiceModel.Configuration;

namespace System.ServiceModel.Discovery.Configuration
{
	public sealed class FindCriteriaElement : ConfigurationElement
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty types, duration, extensions, max_results, scope_match_by, scopes;
		
		static FindCriteriaElement ()
		{
			types = new ConfigurationProperty ("types", typeof (ContractTypeNameElementCollection), null, null, null, ConfigurationPropertyOptions.None);
			duration = new ConfigurationProperty ("duration", typeof (TimeSpan), "00:00:20", new TimeSpanConverter (), null, ConfigurationPropertyOptions.None);
			extensions = new ConfigurationProperty ("extensions", typeof (XmlElementElementCollection), null, null, null, ConfigurationPropertyOptions.None);
			max_results = new ConfigurationProperty ("maxResults", typeof (TimeSpan), "00:00:20", new TimeSpanConverter (), null, ConfigurationPropertyOptions.None);
			scope_match_by = new ConfigurationProperty ("scopeMatchBy", typeof (Uri), null, null, null, ConfigurationPropertyOptions.None);
			scopes = new ConfigurationProperty ("scopes", typeof (ScopeElementCollection), null, null, null, ConfigurationPropertyOptions.None);
			properties = new ConfigurationPropertyCollection ();
			properties.Add (types);
			properties.Add (duration);
			properties.Add (extensions);
			properties.Add (max_results);
			properties.Add (scope_match_by);
			properties.Add (scopes);
		}

		public FindCriteriaElement ()
		{
		}

		[ConfigurationProperty ("types")]
		public ContractTypeNameElementCollection ContractTypeNames {
			get { return (ContractTypeNameElementCollection) base [types]; }
		}
		
		[ConfigurationProperty ("duration", DefaultValue = "00:00:20")]
		[TypeConverter (typeof (TimeSpanConverter))]
		public TimeSpan Duration {
			get { return (TimeSpan) base [duration]; }
			set { base [duration] = value; }
		}
		
		[ConfigurationProperty ("extensions")]
		public XmlElementElementCollection Extensions {
			get { return (XmlElementElementCollection) base [extensions]; }
		}
		
		[ConfigurationProperty ("maxResults", DefaultValue = 0)]
		[IntegerValidator (MinValue = 0, MaxValue = int.MaxValue)]
		public int MaxResults  {
			get { return (int) base [max_results]; }
			set { base [max_results] = value; }
		}
		
		[ConfigurationProperty ("scopeMatchBy")]
		[TypeConverter (typeof (UriTypeConverter))]
		public Uri ScopeMatchBy {
			get { return (Uri) base [scope_match_by]; }
			set { base [scope_match_by] = value; }
		}
		
		[ConfigurationProperty ("scopes")]
		public ScopeElementCollection Scopes {
			get { return (ScopeElementCollection) base [scopes]; }
		}
	}
}

