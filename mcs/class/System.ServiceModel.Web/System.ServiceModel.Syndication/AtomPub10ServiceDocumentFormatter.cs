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
	public class AtomPub10ServiceDocumentFormatter : ServiceDocumentFormatter, IXmlSerializable
	{
		public AtomPub10ServiceDocumentFormatter ()
		{
		}

		public AtomPub10ServiceDocumentFormatter (ServiceDocument documentToWrite)
			: base (documentToWrite)
		{
		}

		public AtomPub10ServiceDocumentFormatter (Type documentTypeToCreate)
		{
			doc_type = documentTypeToCreate;
		}

		Type doc_type;

		public override string Version {
			get { return "http://www.w3.org/2007/app"; }
		}

		[MonoTODO]
		public override bool CanRead (XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		protected override ServiceDocument CreateDocumentInstance ()
		{
			return doc_type != null ? (ServiceDocument) Activator.CreateInstance (doc_type, new object [0]) : base.CreateDocumentInstance ();
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
