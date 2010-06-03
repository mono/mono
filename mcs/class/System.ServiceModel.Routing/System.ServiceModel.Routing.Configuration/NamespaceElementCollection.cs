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
	[ConfigurationCollection (typeof(NamespaceElement))]
	public class NamespaceElementCollection : ConfigurationElementCollection
	{
		public void Add (NamespaceElement element)
		{
			BaseAdd (element);
		}

		public void Clear ()
		{
			BaseClear ();
		}

		protected override ConfigurationElement CreateNewElement ()
		{
			return new NamespaceElement ();
		}

		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((NamespaceElement) element).Prefix;
		}

		public new NamespaceElement this [string name] {
			get {
				foreach (NamespaceElement ne in this)
					if (ne.Namespace == name)
						return ne;
				return null;
			}
		}

		public NamespaceElement this [int index] {
			get { return (NamespaceElement) BaseGet (index); }
		}

		public override bool IsReadOnly ()
		{
			return base.IsReadOnly ();
		}

		public void Remove (NamespaceElement element)
		{
			BaseRemove (element);
		}
	}
}
