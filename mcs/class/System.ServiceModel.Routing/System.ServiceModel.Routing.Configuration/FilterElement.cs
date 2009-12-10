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
		public string CustomType { get; set; }

		[ConfigurationProperty ("filter1", DefaultValue = null, Options = ConfigurationPropertyOptions.None)]
		public string Filter1 { get; set; }

		[ConfigurationProperty ("filter2", DefaultValue = null, Options = ConfigurationPropertyOptions.None)]
		public string Filter2 { get; set; }

		[ConfigurationProperty ("filterData", DefaultValue = null, Options = ConfigurationPropertyOptions.None)]
		public string FilterData { get; set; }

		[ConfigurationProperty ("filterType", DefaultValue = null, Options = ConfigurationPropertyOptions.IsRequired)]
		public FilterType FilterType { get; set; }

		[ConfigurationProperty ("name", DefaultValue = null, Options = ConfigurationPropertyOptions.None | ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		public string Name { get; set; }
	}
}
