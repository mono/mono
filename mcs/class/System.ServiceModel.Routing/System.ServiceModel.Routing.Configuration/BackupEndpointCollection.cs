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
	[ConfigurationCollection (typeof (BackupEndpointElement))]
	public class BackupEndpointCollection : ConfigurationElementCollection
	{
		[ConfigurationProperty ("name", DefaultValue = null, Options = ConfigurationPropertyOptions.IsRequired)]
		public string Name {
			get { return (string) base ["name"]; }
			set { base ["name"] = value; }
		}

		public BackupEndpointCollection ()
		{
		}

		public void Add (BackupEndpointElement element)
		{
			BaseAdd (element);
		}

		public void Clear ()
		{
			BaseClear ();
		}

		protected override ConfigurationElement CreateNewElement ()
		{
			return new BackupEndpointElement ();
		}

		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((BackupEndpointElement) element).EndpointName;
		}

		public void Remove (BackupEndpointElement element)
		{
			BaseRemove (element);
		}
	}
}
