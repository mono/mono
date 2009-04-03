using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;

namespace System.ServiceModel.Syndication
{
	public class ServiceDocument
	{
		[MonoTODO]
		public static TServiceDocument Load<TServiceDocument> (XmlReader reader)
			where TServiceDocument : ServiceDocument, new()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ServiceDocument Load (XmlReader reader)
		{
			throw new NotImplementedException ();
		}


		public ServiceDocument ()
		{
			AttributeExtensions = new Dictionary<XmlQualifiedName, string> ();
			ElementExtensions = new SyndicationElementExtensionCollection ();
			Workspaces = new Collection<Workspace> ();
		}

		public ServiceDocument (IEnumerable<Workspace> workspaces)
			: this ()
		{
			if (workspaces == null)
				throw new ArgumentNullException ("workspaces");

			foreach (var w in workspaces)
				Workspaces.Add (w);
		}

		ServiceDocumentFormatter formatter;

		public Dictionary<XmlQualifiedName, string> AttributeExtensions { get; private set; }

		public Uri BaseUri { get; set; }

		public SyndicationElementExtensionCollection ElementExtensions { get; private set; }

		public string Language { get; set; }

		public Collection<Workspace> Workspaces { get; private set; }


		protected internal virtual Workspace CreateWorkspace ()
		{
			return new Workspace ();
		}

		public ServiceDocumentFormatter GetFormatter ()
		{
			if (formatter == null)
				formatter = new AtomPub10ServiceDocumentFormatter (this);
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
