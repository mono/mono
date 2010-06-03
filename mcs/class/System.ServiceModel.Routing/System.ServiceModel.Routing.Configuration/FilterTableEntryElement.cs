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
		public string BackupList {
			get { return (string) base ["backupList"]; }
			set { base ["backupList"] = value; }
		}

		[ConfigurationProperty ("endpointName", DefaultValue = null, Options = ConfigurationPropertyOptions.None | ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		public string EndpointName {
			get { return (string) base ["endpointName"]; }
			set { base ["endpointName"] = value; }
		}

		[ConfigurationProperty ("filterName", DefaultValue = null, Options = ConfigurationPropertyOptions.None | ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		public string FilterName {
			get { return (string) base ["filterName"]; }
			set { base ["filterName"] = value; }
		}

		[ConfigurationProperty ("priority", Options = ConfigurationPropertyOptions.None)]
		public int Priority {
			get { return (int) base ["priority"]; }
			set { base ["priority"] = value; }
		}
	}
}

