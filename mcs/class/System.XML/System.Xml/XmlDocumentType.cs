//
// System.Xml.XmlDocumentType.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

namespace System.Xml
{
	public class XmlDocumentType  : XmlLinkedNode
	{
		// Fields
		string name;            // name of the document type
		string publicId;        // public identifier on the DOCTYPE
		string systemId;        // system identifier on the DOCTYPE
		string internalSubset;  // value of the DTD internal subset
		
		// Constructor
		protected internal XmlDocumentType (string name, string publicId,
						    string systemId, string internalSubset,
						    XmlDocument doc)
			: base (doc)
		{
			this.name = name;
			this.publicId = publicId;
			this.systemId = systemId;
			this.internalSubset = internalSubset;
		}


		// Properties
		[MonoTODO]
		public XmlNamedNodeMap Entities
		{
			get { return null; }
		}
			
		public string InternalSubset
		{
			get { return internalSubset; }
		}

		public override bool IsReadOnly
		{
			get { return true; } // always return true
		}

		public override string LocalName
		{
			get { return name; }
		}

		public override string Name
		{
			get { return name; }
		}

		public override XmlNodeType NodeType
		{
			get { return XmlNodeType.DocumentType; }
		}

		[MonoTODO]
		public XmlNamedNodeMap Notations
		{
			get { return null; }
		}

		public string PublicId
		{
			get { return publicId; }
		}

		public string SystemId
		{
			get { return systemId; }
		}

		// Methods
		public override XmlNode CloneNode (bool deep)
		{
			// deep is ignored
			return new XmlDocumentType (name, publicId, systemId,
						    internalSubset, OwnerDocument);
		}
		
		public override void WriteContentTo (XmlWriter w)
		{
			// No effect
		}

		[MonoTODO]
		public override void WriteTo (XmlWriter w)
		{
		}
	}
}
