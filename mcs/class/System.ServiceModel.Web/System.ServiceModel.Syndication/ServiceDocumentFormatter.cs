using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;

namespace System.ServiceModel.Syndication
{
	[DataContract]
	public abstract class ServiceDocumentFormatter
	{
		[MonoTODO]
		protected static SyndicationCategory CreateCategory (InlineCategoriesDocument inlineCategories)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected static ResourceCollectionInfo CreateCollection (Workspace workspace)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected static InlineCategoriesDocument CreateInlineCategories (ResourceCollectionInfo collection)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected static ReferencedCategoriesDocument CreateReferencedCategories (ResourceCollectionInfo collection)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected static Workspace CreateWorkspace (ServiceDocument document)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected static void LoadElementExtensions (XmlReader reader, CategoriesDocument categories, int maxExtensionSize)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected static void LoadElementExtensions (XmlReader reader,ResourceCollectionInfo collection, int maxExtensionSize)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected static void LoadElementExtensions (XmlReader reader, ServiceDocument document, int maxExtensionSize)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected static void LoadElementExtensions (XmlReader reader, Workspace workspace, int maxExtensionSize)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected static bool TryParseAttribute (string name, string ns, string value, CategoriesDocument categories, string version)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected static bool TryParseAttribute (string name, string ns, string value, ResourceCollectionInfo collection, string version)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected static bool TryParseAttribute (string name, string ns, string value, ServiceDocument document, string version)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected static bool TryParseAttribute (string name, string ns, string value, Workspace workspace, string version)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected static bool TryParseElement (XmlReader reader, CategoriesDocument categories, string version)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected static bool TryParseElement (XmlReader reader, ResourceCollectionInfo collection, string version)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected static bool TryParseElement (XmlReader reader, ServiceDocument document, string version)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected static bool TryParseElement (XmlReader reader, Workspace workspace, string version)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected static void WriteAttributeExtensions (XmlWriter writer, CategoriesDocument categories, string version)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected static void WriteAttributeExtensions (XmlWriter writer, ResourceCollectionInfo collection, string version)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected static void WriteAttributeExtensions (XmlWriter writer, ServiceDocument document, string version)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected static void WriteAttributeExtensions (XmlWriter writer, Workspace workspace, string version)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected static void WriteElementExtensions (XmlWriter writer, CategoriesDocument categories, string version)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected static void WriteElementExtensions (XmlWriter writer, ResourceCollectionInfo collection, string version)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected static void WriteElementExtensions (XmlWriter writer, ServiceDocument document, string version)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected static void WriteElementExtensions (XmlWriter writer, Workspace workspace, string version)
		{
			throw new NotImplementedException ();
		}

		// instance members

		protected ServiceDocumentFormatter ()
			: this (new ServiceDocument ())
		{
		}

		protected ServiceDocumentFormatter (ServiceDocument documentToWrite)
		{
			SetDocument (documentToWrite);
		}

		public ServiceDocument Document { get; private set; }

		public abstract string Version { get; }


		public abstract bool CanRead (XmlReader reader);

		protected virtual ServiceDocument CreateDocumentInstance ()
		{
			return new ServiceDocument ();
		}

		public abstract void ReadFrom (XmlReader reader);

		protected virtual void SetDocument (ServiceDocument document)
		{
			if (document == null)
				throw new ArgumentNullException ("document");
			Document = document;
		}

		public abstract void WriteTo (XmlWriter writer);
	}
}
