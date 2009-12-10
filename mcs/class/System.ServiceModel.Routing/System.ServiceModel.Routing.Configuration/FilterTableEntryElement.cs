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
	public class FilterTableEntryElement : ConfigurationElement
	{
		[ConfigurationProperty ("backupList", DefaultValue = null, Options = ConfigurationPropertyOptions.None)]
		public string BackupList { get; set; }

		[ConfigurationProperty ("endpointName", DefaultValue = null, Options = ConfigurationPropertyOptions.None | ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		public string EndpointName { get; set; }

		[ConfigurationProperty ("filterName", DefaultValue = null, Options = ConfigurationPropertyOptions.None | ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		public string FilterName { get; set; }

		[ConfigurationProperty ("priority", Options = ConfigurationPropertyOptions.None)]
		public int Priority { get; set; }
	}
}

