using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;

namespace System.ServiceModel.Syndication
{
	public class Workspace
	{
		public Workspace ()
		{
			AttributeExtensions = new Dictionary<XmlQualifiedName, string> ();
			Collections = new Collection<ResourceCollectionInfo> ();
			ElementExtensions = new SyndicationElementExtensionCollection ();
		}

		public Workspace (TextSyndicationContent title, IEnumerable<ResourceCollectionInfo> collections)
		{
			Title = title;
			Collections = new Collection<ResourceCollectionInfo> ();
			foreach (var i in collections)
				Collections.Add (i);
		}

		public Workspace (string title, IEnumerable<ResourceCollectionInfo> collections)
			: this (new TextSyndicationContent (title), collections)
		{
		}

		public Dictionary<XmlQualifiedName, string> AttributeExtensions { get; private set; }

		public Uri BaseUri { get; set; }

		public Collection<ResourceCollectionInfo> Collections { get; private set; }

		public SyndicationElementExtensionCollection ElementExtensions { get; private set; }

		public TextSyndicationContent Title { get; set; }

		protected internal virtual ResourceCollectionInfo CreateResourceCollection ()
		{
			return new ResourceCollectionInfo ();
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
