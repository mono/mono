// 
// System.Web.Services.Protocols.DiscoveryDocument.cs
//
// Author:
//   Dave Bettin (javabettin@yahoo.com)
//   Tim Coleman (tim@timcoleman.com)  
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Dave Bettin, 2002
// Copyright (C) Tim Coleman, 2002
//

using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace System.Web.Services.Discovery {
	[XmlRoot ("discovery", Namespace = "http://schemas.xmlsoap.org/disco/")]
	public sealed class DiscoveryDocument {

		#region Fields
		
		public const string Namespace = "http://schemas.xmlsoap.org/disco/";
		
		[XmlElement(typeof(ContractReference), Namespace="http://schemas.xmlsoap.org/disco/scl/")]
		[XmlElement(typeof(DiscoveryDocumentReference))]
		[XmlElement(typeof(SchemaReference))]
		internal ArrayList references = new ArrayList();
		
		[XmlElement(typeof(SoapBinding), ElementName="soap", Namespace="http://schemas/xmlsoap.org/disco/schema/soap/")]
		internal ArrayList additionalInfo = new ArrayList();
		
		#endregion // Fields
		
		#region Constructors

		public DiscoveryDocument () 
		{
		}
		
		#endregion // Constructors

		#region Properties
	
		[XmlIgnore]
		public IList References {
			get { return references; }
		}
		
		[XmlIgnore]
		internal IList AdditionalInfo {
			get { return additionalInfo; }
		}
		
		#endregion // Properties

		#region Methods

		public static bool CanRead (XmlReader xmlReader)
		{
			xmlReader.MoveToContent ();
			return xmlReader.NodeType == XmlNodeType.Element &&
					xmlReader.LocalName == "discovery" && 
					xmlReader.NamespaceURI == Namespace;
		}

		public static DiscoveryDocument Read (Stream stream)
		{
			return Read (new XmlTextReader (stream));
		}
		
		public static DiscoveryDocument Read (TextReader textReader)
		{
			return Read (new XmlTextReader (textReader));
		}
		
		public static DiscoveryDocument Read (XmlReader xmlReader)
		{
			DiscoveryDocumentSerializer ser = new DiscoveryDocumentSerializer();
			return (DiscoveryDocument) ser.Deserialize (xmlReader);
		}
		
		public void Write (Stream stream)
		{
			DiscoveryDocumentSerializer ser = new DiscoveryDocumentSerializer();
			ser.Serialize (stream, this, GetNamespaceList());
		}
		
		public void Write (TextWriter textWriter)
		{
			DiscoveryDocumentSerializer ser = new DiscoveryDocumentSerializer();
			ser.Serialize (textWriter, this, GetNamespaceList());
		}
		
		public void Write (XmlWriter xmlWriter)
		{
			DiscoveryDocumentSerializer ser = new DiscoveryDocumentSerializer();
			ser.Serialize (xmlWriter, this, GetNamespaceList());
		}

		XmlSerializerNamespaces GetNamespaceList ()
		{
			XmlSerializerNamespaces ns = new XmlSerializerNamespaces ();
			ns.Add ("scl", ContractReference.Namespace);
			return ns;
		}
		
		#endregion // Methods
	}

	internal class DiscoveryDocumentSerializer : XmlSerializer 
	{
		protected override void Serialize (object o, XmlSerializationWriter writer)
		{
			DiscoveryDocumentWriter xsWriter = writer as DiscoveryDocumentWriter;
			xsWriter.WriteRoot_DiscoveryDocument (o);
		}
		
		protected override object Deserialize (XmlSerializationReader reader)
		{
			DiscoveryDocumentReader xsReader = reader as DiscoveryDocumentReader;
			return xsReader.ReadRoot_DiscoveryDocument ();
		}
		
		protected override XmlSerializationWriter CreateWriter ()
		{
			return new DiscoveryDocumentWriter ();
		}
		
		protected override XmlSerializationReader CreateReader ()
		{
			return new DiscoveryDocumentReader ();
		}
	}	
}
