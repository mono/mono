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
	[ConfigurationCollection (typeof(FilterTableEntryElement))]
	public class FilterTableEntryCollection : ConfigurationElementCollection
	{
		[ConfigurationProperty ("name", DefaultValue = null, Options = ConfigurationPropertyOptions.None | ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		public string Name {
			get { return (string) base ["name"]; }
			set { base ["name"] = value; }
		}

		public void Add (FilterTableEntryElement element)
		{
			BaseAdd (element);
		}

		public void Clear ()
		{
			BaseClear ();
		}

		protected override ConfigurationElement CreateNewElement ()
		{
			return new FilterTableEntryElement ();
		}

		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((FilterTableEntryElement) element).EndpointName;
		}

		public void Remove (FilterTableEntryElement element)
		{
			BaseRemove (element);
		}
	}

}
