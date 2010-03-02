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
	[ConfigurationCollection (typeof (FilterTableEntryCollection), AddItemName = "filterTable")]
	public class FilterTableCollection : ConfigurationElementCollection
	{
		public void Add (FilterTableEntryCollection element)
		{
			BaseAdd (element);
		}

		public void Clear ()
		{
			BaseClear ();
		}

		protected override ConfigurationElement CreateNewElement ()
		{
			return new FilterTableEntryCollection ();
		}

		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((FilterTableEntryCollection) element).Name;
		}

		public void Remove (FilterTableEntryCollection element)
		{
			BaseRemove (element);
		}

		public new FilterTableEntryCollection this [string name] {
			get { return (FilterTableEntryCollection) BaseGet (name); }
		}
	}
}
