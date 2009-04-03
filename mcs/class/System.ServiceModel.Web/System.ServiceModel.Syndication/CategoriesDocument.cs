using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;

namespace System.ServiceModel.Syndication
{
	public abstract class CategoriesDocument
	{
		[MonoTODO]
		public static InlineCategoriesDocument Create (Collection<SyndicationCategory> categories)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ReferencedCategoriesDocument Create (Uri linkToCategoriesDocument)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static InlineCategoriesDocument Create (Collection<SyndicationCategory> categories, bool isFixed, string scheme)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static CategoriesDocument Load (XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		public CategoriesDocument ()
		{
			AttributeExtensions = new Dictionary<XmlQualifiedName, string> ();
			ElementExtensions = new SyndicationElementExtensionCollection ();
		}

		CategoriesDocumentFormatter formatter;

		public Dictionary<XmlQualifiedName, string> AttributeExtensions { get; private set; }

		public Uri BaseUri { get; set; }

		public SyndicationElementExtensionCollection ElementExtensions { get; private set; }

		public string Language { get; set; }

		public CategoriesDocumentFormatter GetFormatter ()
		{
			if (formatter == null)
				formatter = new AtomPub10CategoriesDocumentFormatter (this);
			return formatter;
		}

		[MonoTODO]
		public void Save (XmlWriter writer)
		{
			throw new NotImplementedException ();
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
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual void WriteElementExtensions (XmlWriter writer, string version)
		{
			throw new NotImplementedException ();
		}
	}
}
