//
// System.Xml.XmlDocumentFragment
//
// Author:
//   Duncan Mak  (duncan@ximian.com)
//   Atsushi Enomoto  (ginga@kit.hi-ho.ne.jp)
//
// (C), Ximian, Inc
// (C)2002 Atsushi Enomoto
using System;
using System.IO;
using System.Text;
using System.Xml.XPath;

namespace System.Xml
{
	public class XmlDocumentFragment : XmlNode
	{

		#region Constructor

		protected internal XmlDocumentFragment (XmlDocument doc)
			: base (doc)
		{
		}
		
		#endregion

		#region Properties

		public override string InnerXml {
			set {
				// Copied from XmlElement.InnerXml (in the meantime;-))
				for (int i = 0; i < ChildNodes.Count; i++)
					this.RemoveChild (ChildNodes [i]);

				// I hope there are any well-performance logic...
				XmlNamespaceManager nsmgr = this.ConstructNamespaceManager ();
				XmlParserContext ctx = new XmlParserContext (OwnerDocument.NameTable, nsmgr,
					OwnerDocument.DocumentType != null ? OwnerDocument.DocumentType.DTD : null,
					BaseURI, XmlLang, XmlSpace, null);
				XmlTextReader xmlReader = new XmlTextReader (value, XmlNodeType.Element, ctx);
				xmlReader.XmlResolver = OwnerDocument.Resolver;

				do {
					XmlNode n = OwnerDocument.ReadNode (xmlReader);
					if(n == null) break;
					AppendChild (n);
				} while (true);
			}
			get {
				StringBuilder sb = new StringBuilder ();
				for (int i = 0; i < ChildNodes.Count; i++)
					sb.Append (ChildNodes [i].OuterXml);
				return sb.ToString ();
			}
		}
		
		public override string LocalName {
			get { return "#document-fragment"; }
		}


		public override string Name { 
			get { return "#document-fragment"; }
		}

		public override XmlNodeType NodeType {
			get { return XmlNodeType.DocumentFragment; }
		}

		public override XmlDocument OwnerDocument {
			get { return base.OwnerDocument; }
		}

		public override XmlNode ParentNode {
			get { return null; } // it's always null here.
		}

		internal override XPathNodeType XPathNodeType
		{
			get { return XPathNodeType.Root; }
		}
		#endregion

		#region Methods		
		public override XmlNode CloneNode (bool deep)
		{
			if (deep) { // clone document + child nodes
				XmlNode node = FirstChild;

				while ((node != null) && (node.HasChildNodes)) {
					AppendChild (node.NextSibling.CloneNode (false));
					node = node.NextSibling;
				}

				return node;
			} else
				return new XmlDocumentFragment (OwnerDocument);
		}

		public override void WriteContentTo (XmlWriter w)
		{
			for (int i = 0; i < ChildNodes.Count; i++)
				ChildNodes [i].WriteContentTo (w);
		}

		public override void WriteTo (XmlWriter w)
		{
			for (int i = 0; i < ChildNodes.Count; i++)
				ChildNodes [i].WriteTo (w);
		}

		#endregion
	}
}
