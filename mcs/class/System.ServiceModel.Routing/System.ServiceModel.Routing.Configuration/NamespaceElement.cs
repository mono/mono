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
	public class NamespaceElement : ConfigurationElement
	{
		[ConfigurationProperty ("namespace", DefaultValue = null, Options = ConfigurationPropertyOptions.IsRequired)]
		public string Namespace {
			get { return (string) base ["namespace"]; }
			set { base ["namespace"] = value; }
		}

		[ConfigurationProperty ("prefix", DefaultValue = null, Options = ConfigurationPropertyOptions.None | ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		public string Prefix {
			get { return (string) base ["prefix"]; }
			set { base ["prefix"] = value; }
		}
	}
}
