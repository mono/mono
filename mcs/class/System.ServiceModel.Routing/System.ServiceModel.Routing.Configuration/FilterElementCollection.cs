using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Routing.Configuration
{
	[ConfigurationCollection (typeof(FilterElement), AddItemName = "filter")]
	public class FilterElementCollection : ConfigurationElementCollection
	{
		public void Add (FilterElement element)
		{
			BaseAdd (element);
		}

		public void Clear ()
		{
			BaseClear ();
		}

		protected override ConfigurationElement CreateNewElement ()
		{
			return new FilterElement ();
		}

		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((FilterElement) element).Name;
		}

		public new FilterElement this [string name] {
			get {
				foreach (FilterElement fe in this)
					if (fe.Name == name)
						return fe;
				return null;
			}
		}

		public FilterElement this [int index] {
			get { return (FilterElement) BaseGet (index); }
		}

		public override bool IsReadOnly ()
		{
			return base.IsReadOnly ();
		}

		public void Remove (FilterElement element)
		{
			BaseRemove (element);
		}

	}
}
