//
// System.Xml.XmlDocumentType.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//	   Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
// (C) Ximian, Inc.
//
using System;
using System.Collections;

namespace System.Xml
{
	public class XmlDocumentType  : XmlLinkedNode
	{
		// Fields
		string name;            // name of the document type
		string publicId;        // public identifier on the DOCTYPE
		string systemId;        // system identifier on the DOCTYPE
		string internalSubset;  // value of the DTD internal subset
		internal XmlNamedNodeMap entities;
		internal XmlNamedNodeMap notations;
		internal Hashtable elementDecls;
		internal Hashtable attListDecls;

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
			entities = new XmlNamedNodeMap (this);
			notations = new XmlNamedNodeMap (this);
			elementDecls = new Hashtable ();
			attListDecls = new Hashtable ();
		}


		// Properties
		public XmlNamedNodeMap Entities
		{
			get { return entities; }
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

		public XmlNamedNodeMap Notations
		{
			get { return notations; }
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

		public override void WriteTo (XmlWriter w)
		{
			w.WriteDocType (name, publicId, systemId, internalSubset);
		}
	}
}
