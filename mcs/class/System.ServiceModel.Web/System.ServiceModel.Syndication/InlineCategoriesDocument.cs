using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;

namespace System.ServiceModel.Syndication
{
	public class InlineCategoriesDocument : CategoriesDocument
	{
		public InlineCategoriesDocument ()
		{
			Categories = new Collection<SyndicationCategory> ();
		}

		public InlineCategoriesDocument (IEnumerable<SyndicationCategory> categories)
			: this ()
		{
			foreach (var i in categories)
				Categories.Add (i);
		}

		public InlineCategoriesDocument (IEnumerable<SyndicationCategory> categories, bool isFixed, string scheme)
			: this (categories)
		{
			IsFixed = isFixed;
			Scheme = scheme;
		}

		protected internal virtual SyndicationCategory CreateCategory ()
		{
			return new SyndicationCategory ();
		}

		public Collection<SyndicationCategory> Categories { get; private set; }

		public bool IsFixed { get; set; }

		public string Scheme { get; set; }
	}
}
