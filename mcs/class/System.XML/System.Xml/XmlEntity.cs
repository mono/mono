//
// System.Xml.XmlEntity.cs
//
// Author:
// 	Duncan Mak  (duncan@ximian.com)
//	Atsushi Enomoto  (atsushi@ximian.com)
//
// (C) Ximian, Inc.
// (C) 2004 Novell Inc.
//
using Mono.Xml;

namespace System.Xml
{
	public class XmlEntity : XmlNode
	{
		#region Constructors

		internal XmlEntity (string name, string NDATA, string publicId, string systemId,
				    XmlDocument doc)
			: base (doc)
		{
			this.name = doc.NameTable.Add (name);
			this.NDATA = NDATA;
			this.publicId = publicId;
			this.systemId = systemId;
			this.baseUri = doc.BaseURI;
		}

		#endregion
		
		#region Fields

		string name;
		string NDATA;
		string publicId;
		string systemId;
		string baseUri;
		XmlLinkedNode lastChild;

		#endregion

		#region Properties

		public override string BaseURI {
			get {  return baseUri; }
		}

		public override string InnerText {
			get { return base.InnerText; }
			set { throw new InvalidOperationException ("This operation is not supported."); }
		}

		public override string InnerXml {
			get { return base.InnerXml; }
			set { throw new InvalidOperationException ("This operation is not supported."); }
		}

		public override bool IsReadOnly {
			get { return true; } // always read-only.
		}

		public override string LocalName {
			get { return name; }
		}

		public override string Name {
			get { return name; }
		}

		public override XmlNodeType NodeType {
			get { return XmlNodeType.Entity; }
		}

		public string NotationName {
			get {
				if (NDATA == null)
					return null;
				else
					return NDATA;
			}
		}

		public override string OuterXml {
			get { return String.Empty; }
		}

		public string PublicId {
			get {
				if (publicId == null)
					return null;
				else
					return publicId;
			}
		}

		public string SystemId {
			get {
				if (publicId == null)
					return null;
				else
					return systemId;
			}
		}
		#endregion

		#region Methods

		public override XmlNode CloneNode (bool deep)
		{
			throw new InvalidOperationException ("This operation is not supported.");
		}

		public override void WriteContentTo (XmlWriter w)
		{
			// No effect.
		}

		public override void WriteTo (XmlWriter w)
		{
			// No effect.
		}

		internal void SetEntityContent ()
		{
			if (FirstChild != null)
				return;

			XmlDocumentType doctype = OwnerDocument.DocumentType;

			if (doctype == null)
				return;

			DTDEntityDeclaration decl = doctype.DTD.EntityDecls [name];
			if (decl == null)
				return;

			XmlNameTable nt = this.OwnerDocument.NameTable;
			XmlNamespaceManager nsmgr = this.ConstructNamespaceManager ();
			XmlParserContext ctx = new XmlParserContext (OwnerDocument.NameTable, nsmgr,
				doctype != null ? doctype.DTD : null,
				BaseURI, XmlLang, XmlSpace, null);
			XmlTextReader xmlReader = new XmlTextReader (decl.EntityValue, XmlNodeType.Element, ctx);
			xmlReader.XmlResolver = OwnerDocument.Resolver;

			do {
				XmlNode n = OwnerDocument.ReadNode (xmlReader);
				if(n == null) break;
				InsertBefore (n, null, false, false);
			} while (true);
		}
		#endregion
	}
}
