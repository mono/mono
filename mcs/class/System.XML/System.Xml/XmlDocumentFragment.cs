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

namespace System.Xml
{
	public class XmlDocumentFragment : XmlNode
	{
		#region Fields

		private XmlLinkedNode lastLinkedChild;

		#endregion

		#region Constructor

		protected internal XmlDocumentFragment (XmlDocument doc)
			: base (doc)
		{
		}
		
		#endregion
		#region Properties

		[MonoTODO("Setter is as incomplete as that of XmlElement.InnerXml")]
		public override string InnerXml {
			set {
				// Copied from XmlElement.InnerXml (in the meantime;-))
				foreach(XmlNode n in ChildNodes)
				{
					this.RemoveChild (n);
				}		  

				// I hope there are any well-performance logic...
				XmlNameTable nt = this.OwnerDocument.NameTable;
				XmlNamespaceManager nsmgr = this.ConstructNamespaceManager ();
				XmlParserContext ctx = new XmlParserContext (nt, nsmgr, XmlLang, this.XmlSpace);
				XmlTextReader xmlReader = OwnerDocument.ReusableReader;
				xmlReader.SetReaderContext (String.Empty, ctx);
				xmlReader.SetReaderFragment (new StringReader (value), XmlNodeType.DocumentFragment);

				do {
					XmlNode n = OwnerDocument.ReadNode (xmlReader);
					if(n == null) break;
					AppendChild (n);
				} while (true);
			}
			get {
				StringBuilder sb = new StringBuilder ();
				foreach(XmlNode n in ChildNodes)
					sb.Append (n.OuterXml);
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

		// It is really not a type of XmlLinkedNode,
		//   but I copied this way from XmlElement. I looks good.
		internal override XmlLinkedNode LastLinkedChild
		{
			get { return lastLinkedChild; }
			set { lastLinkedChild = value; }
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
			foreach(XmlNode n in ChildNodes)
				n.WriteContentTo (w);
		}

		public override void WriteTo (XmlWriter w)
		{
			foreach(XmlNode n in ChildNodes)
				n.WriteTo (w);
		}

		#endregion
	}
}
