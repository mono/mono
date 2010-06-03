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
	public class BackupEndpointElement : ConfigurationElement
	{
		[ConfigurationProperty ("endpointName", DefaultValue = null, Options = ConfigurationPropertyOptions.IsRequired)]
		public string EndpointName {
			get { return (string) base ["endpointName"]; }
			set { base ["endpointName"] = value; }
		}
	}
}
