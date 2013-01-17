//
// System.Xml.XmlDocumentFragment
//
// Author:
//   Duncan Mak  (duncan@ximian.com)
//   Atsushi Enomoto  (ginga@kit.hi-ho.ne.jp)
//
// (C), Ximian, Inc
// (C)2002 Atsushi Enomoto

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.IO;
using System.Text;
using System.Xml.XPath;

namespace System.Xml
{
	public class XmlDocumentFragment : XmlNode, IHasXmlChildNode
	{
		XmlLinkedNode lastLinkedChild;

		#region Constructor

		protected internal XmlDocumentFragment (XmlDocument ownerDocument)
			: base (ownerDocument)
		{
		}
		
		#endregion

		#region Properties

		XmlLinkedNode IHasXmlChildNode.LastLinkedChild {
			get { return lastLinkedChild; }
			set { lastLinkedChild = value; }
		}

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
