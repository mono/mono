using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Routing.Configuration
{
	public class FilterElement : ConfigurationElement
	{
		[ConfigurationProperty ("customType", DefaultValue = null, Options = ConfigurationPropertyOptions.None)]
		public string CustomType {
			get { return (string) base ["customType"]; }
			set { base ["customType"] = value; }
		}

		[ConfigurationProperty ("filter1", DefaultValue = null, Options = ConfigurationPropertyOptions.None)]
		public string Filter1 {
			get { return (string) base ["filter1"]; }
			set { base ["filter1"] = value; }
		}

		[ConfigurationProperty ("filter2", DefaultValue = null, Options = ConfigurationPropertyOptions.None)]
		public string Filter2 {
			get { return (string) base ["filter2"]; }
			set { base ["filter2"] = value; }
		}

		[ConfigurationProperty ("filterData", DefaultValue = null, Options = ConfigurationPropertyOptions.None)]
		public string FilterData {
			get { return (string) base ["filterData"]; }
			set { base ["filterData"] = value; }
		}

		[ConfigurationProperty ("filterType", DefaultValue = null, Options = ConfigurationPropertyOptions.IsRequired)]
		public FilterType FilterType {
			get { return (FilterType) base ["filterType"]; }
			set { base ["filterType"] = value; }
		}

		[ConfigurationProperty ("name", DefaultValue = null, Options = ConfigurationPropertyOptions.None | ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		public string Name {
			get { return (string) base ["name"]; }
			set { base ["name"] = value; }
		}
	}
}
