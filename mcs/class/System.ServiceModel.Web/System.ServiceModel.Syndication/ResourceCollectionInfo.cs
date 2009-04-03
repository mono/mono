using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;

namespace System.ServiceModel.Syndication
{
	public class ResourceCollectionInfo
	{
		public ResourceCollectionInfo ()
		{
			Accepts = new Collection<string> ();
			AttributeExtensions = new Dictionary<XmlQualifiedName, string> ();
			Categories = new Collection<CategoriesDocument> ();
			ElementExtensions = new SyndicationElementExtensionCollection ();
		}

		public ResourceCollectionInfo (TextSyndicationContent title, Uri link)
			: this ()
		{
			Title = title;
			Link = link;
		}

		public ResourceCollectionInfo (string title, Uri link)
			: this (new TextSyndicationContent (title), link)
		{
		}

		public ResourceCollectionInfo (TextSyndicationContent title, Uri link, IEnumerable<CategoriesDocument> categories, bool allowsNewEntries)
			: this (title, link)
		{
			if (categories == null)
				throw new ArgumentNullException ("categories");

			foreach (var c in categories)
				Categories.Add (c);
			allow_new_entries = allowsNewEntries;
		}

		public ResourceCollectionInfo (TextSyndicationContent title, Uri link, IEnumerable<CategoriesDocument> categories, IEnumerable<string> accepts)
			: this (title, link, categories, true)
		{
			if (accepts == null)
				throw new ArgumentNullException ("accepts");
			foreach (var a in accepts)
				Accepts.Add (a);
		}

		bool allow_new_entries;

		public Collection<string> Accepts { get; private set; }

		public Dictionary<XmlQualifiedName, string> AttributeExtensions { get; private set; }

		public Uri BaseUri { get; set; }

		public Collection<CategoriesDocument> Categories { get; private set; }

		public SyndicationElementExtensionCollection ElementExtensions { get; private set; }

		public Uri Link { get; set; }

		public TextSyndicationContent Title { get; set; }


		protected internal virtual InlineCategoriesDocument CreateInlineCategoriesDocument ()
		{
			return new InlineCategoriesDocument ();
		}

		protected internal virtual ReferencedCategoriesDocument CreateReferencedCategoriesDocument ()
		{
			return new ReferencedCategoriesDocument ();
		}

		[MonoTODO]
		protected internal virtual bool TryParseAttribute (string name, string ns, string value, string version)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual bool TryParseElement (XmlReader reader, string version)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual void WriteAttributeExtensions (XmlWriter writer, string version)
		{
			foreach (var p in AttributeExtensions)
				writer.WriteAttributeString (p.Key.Name, p.Key.Namespace, p.Value);

			throw new NotImplementedException ();
		}
	}
}
