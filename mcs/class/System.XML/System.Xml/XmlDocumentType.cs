//
// System.Xml.XmlDocumentType.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//	   Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
// (C) Ximian, Inc.
//
using System;
using System.IO;
using System.Collections;
using Mono.Xml;

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
		DTDObjectModel dtd;

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

			XmlTextReader xtr = new XmlTextReader (BaseURI, new StringReader (""), doc.NameTable);
			xtr.GenerateDTDObjectModel (name, publicId, systemId, internalSubset);
			this.dtd = xtr.DTD;

			ImportFromDTD ();
		}

		internal XmlDocumentType (XmlTextReader reader, XmlDocument doc)
			: base (doc)
		{
			this.name = reader.Name;
			this.publicId = reader ["PUBLIC"];
			this.systemId = reader ["SYSTEM"];
			this.internalSubset = reader.Value;
			this.dtd = reader.DTD;

			ImportFromDTD ();
		}

		private void ImportFromDTD ()
		{
			entities = new XmlNamedNodeMap (this);
			notations = new XmlNamedNodeMap (this);

			foreach (DTDEntityDeclaration decl in DTD.EntityDecls.Values) {
				XmlNode n = new XmlEntity (decl.Name, decl.NotationName,
					decl.PublicId, decl.SystemId, OwnerDocument);
				// FIXME: Value is more complex, similar to Attribute.
				n.insertBeforeIntern (OwnerDocument.CreateTextNode (decl.LiteralEntityValue), null);
				entities.Nodes.Add (n);
			}
			foreach (DTDNotationDeclaration decl in DTD.NotationDecls.Values) {
				XmlNode n = new XmlNotation (decl.LocalName, decl.Prefix,
					decl.PublicId, decl.SystemId, OwnerDocument);
				notations.Nodes.Add (n);
			}
		}

		// Properties
		internal DTDObjectModel DTD {
			get { return dtd; }
		}

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
