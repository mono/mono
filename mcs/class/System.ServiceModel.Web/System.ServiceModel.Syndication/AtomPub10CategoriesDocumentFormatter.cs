using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.ServiceModel.Syndication
{
	public class AtomPub10CategoriesDocumentFormatter : CategoriesDocumentFormatter, IXmlSerializable
	{
		public AtomPub10CategoriesDocumentFormatter ()
		{
		}

		public AtomPub10CategoriesDocumentFormatter (CategoriesDocument documentToWrite)
			: base (documentToWrite)
		{
		}

		public AtomPub10CategoriesDocumentFormatter (Type inlineDocumentType, Type referencedDocumentType)
		{
			if (inlineDocumentType == null)
				throw new ArgumentNullException ("inlineDocumentType");
			if (referencedDocumentType == null)
				throw new ArgumentNullException ("referencedDocumentType");

			inline_type = inlineDocumentType;
			ref_type = referencedDocumentType;
		}

		Type inline_type, ref_type;

		public override string Version {
			get { return "http://www.w3.org/2007/app"; }
		}

		[MonoTODO]
		public override bool CanRead (XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		protected override InlineCategoriesDocument CreateInlineCategoriesDocument ()
		{
			return (InlineCategoriesDocument) Activator.CreateInstance (inline_type, new object [0]);
		}

		protected override ReferencedCategoriesDocument CreateReferencedCategoriesDocument ()
		{
			return (ReferencedCategoriesDocument) Activator.CreateInstance (ref_type, new object [0]);
		}

		[MonoTODO]
		public override void ReadFrom (XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteTo (XmlWriter writer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		XmlSchema IXmlSerializable.GetSchema ()
		{
			throw new NotImplementedException ();
		}

		void IXmlSerializable.ReadXml (XmlReader reader)
		{
			ReadFrom (reader);
		}

		void IXmlSerializable.WriteXml (XmlWriter writer)
		{
			WriteTo (writer);
		}
	}
}
