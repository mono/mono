//
// System.Xml.XmlAttribute
//
// Authors:
//   Jason Diamond (jason@injektilo.org)
//   Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
// (C) 2002 Jason Diamond  http://injektilo.org/
// (C) 2003 Atsushi Enomoto
//

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
using System.Text;
using System.Xml.XPath;
using Mono.Xml;
#if NET_2_0
using System.Xml.Schema;
#endif

namespace System.Xml
{
	public class XmlAttribute : XmlNode, IHasXmlChildNode
	{
		#region Fields

		private XmlNameEntry name;
		internal bool isDefault;
		XmlLinkedNode lastLinkedChild;
#if NET_2_0
		private IXmlSchemaInfo schemaInfo;
#endif

		#endregion

		#region Constructor

		protected internal XmlAttribute (
			string prefix, 
			string localName, 
			string namespaceURI,
			XmlDocument doc) : this (prefix, localName, namespaceURI, doc, false, true)
		{
		}

		internal XmlAttribute (
			string prefix, 
			string localName, 
			string namespaceURI,
			XmlDocument doc,
			bool atomizedNames, bool checkNamespace) : base (doc)
		{
			if (!atomizedNames) {
				if (prefix == null)
					prefix = String.Empty;
				if (namespaceURI == null)
					namespaceURI = String.Empty;
			}

			// Prefix "xml" should be also checked (http://www.w3.org/XML/xml-names-19990114-errata#NE05)
			// but MS.NET ignores such case
			if (checkNamespace) {
				if (prefix == "xmlns" || (prefix == "" && localName == "xmlns"))
					if (namespaceURI != XmlNamespaceManager.XmlnsXmlns)
						throw new ArgumentException ("Invalid attribute namespace for namespace declaration.");
				else if (prefix == "xml" && namespaceURI != XmlNamespaceManager.XmlnsXml)
					throw new ArgumentException ("Invalid attribute namespace for namespace declaration.");
			}

			if (!atomizedNames) {
				// There are no means to identify the DOM is
				// namespace-aware or not, so we can only 
				// check Name validity.
				if (prefix != "" && !XmlChar.IsName (prefix))
					throw new ArgumentException ("Invalid attribute prefix.");
				else if (!XmlChar.IsName (localName))
					throw new ArgumentException ("Invalid attribute local name.");

				prefix = doc.NameTable.Add (prefix);
				localName = doc.NameTable.Add (localName);
				namespaceURI = doc.NameTable.Add (namespaceURI);
			}
			name = doc.NameCache.Add (prefix, localName, namespaceURI, true);
		}

		#endregion

		#region Properties

		XmlLinkedNode IHasXmlChildNode.LastLinkedChild {
			get { return lastLinkedChild; }
			set { lastLinkedChild = value; }
		}

		public override string BaseURI {
			get { return OwnerElement != null ? OwnerElement.BaseURI : String.Empty; }
		}

		public override string InnerText {
			set {
				Value = value;
			}
		}

		public override string InnerXml {
			set {
				RemoveAll ();
				XmlNamespaceManager nsmgr = ConstructNamespaceManager ();
				XmlParserContext ctx = new XmlParserContext (OwnerDocument.NameTable, nsmgr,
					OwnerDocument.DocumentType != null ? OwnerDocument.DocumentType.DTD : null,
					BaseURI, XmlLang, XmlSpace, null);
				XmlTextReader xtr = new XmlTextReader (value, XmlNodeType.Attribute, ctx);
				xtr.XmlResolver = OwnerDocument.Resolver;
				xtr.Read ();
				OwnerDocument.ReadAttributeNodeValue (xtr, this);
			}
		}

		public override string LocalName {
			get {
				return name.LocalName;
			}
		}

		public override string Name {
			get { return name.GetPrefixedName (OwnerDocument.NameCache); }
		}

		public override string NamespaceURI {
			get {
				return name.NS;
			}
		}

		public override XmlNodeType NodeType {
			get {
				return XmlNodeType.Attribute;
			}
		}

		internal override XPathNodeType XPathNodeType {
			get {
				return XPathNodeType.Attribute;
			}
		}

		public override XmlDocument OwnerDocument {
			get {
				return base.OwnerDocument;
			}
		}

		public virtual XmlElement OwnerElement {
			get { return AttributeOwnerElement; }
		}

		public override XmlNode ParentNode {
			get {
				// It always returns null (by specification).
				return null;
			}
		}

		// We gotta do more in the set block here
		// We need to do the proper tests and throw
		// the correct Exceptions
		//
		// Wrong cases are: (1)check readonly, (2)check character validity,
		// (3)check format validity, (4)this is attribute and qualifiedName != "xmlns"
		public override string Prefix {
			set {
				if (IsReadOnly)
					throw new XmlException ("This node is readonly.");
				if (name.Prefix == "xmlns" && value != "xmlns")
					throw new ArgumentException ("Cannot bind to the reserved namespace.");

				value = OwnerDocument.NameTable.Add (value);
				name = OwnerDocument.NameCache.Add (value,
					name.LocalName, name.NS, true);
			}
			
			get {
				return name.Prefix;
			}
		}

#if NET_2_0
		public override IXmlSchemaInfo SchemaInfo {
			get { return schemaInfo; }
			internal set { schemaInfo = value; }
		}
#endif

		public virtual bool Specified {
			get {
				return !isDefault;
			}
		}

		public override string Value {
			get { return InnerText; }

			set {
				if (this.IsReadOnly)
					throw new ArgumentException ("Attempt to modify a read-only node.");
				OwnerDocument.CheckIdTableUpdate (this, InnerText, value);

				XmlNode textChild = FirstChild as XmlCharacterData;
				if (textChild == null) {
					this.RemoveAll ();
					AppendChild (OwnerDocument.CreateTextNode (value), false);
				}
				else if (FirstChild.NextSibling != null) {
					this.RemoveAll ();
					AppendChild (OwnerDocument.CreateTextNode (value), false);
				}
				else
					textChild.Value = value;
				isDefault = false;
			}
		}

		internal override string XmlLang {
			get { return OwnerElement != null ? OwnerElement.XmlLang : String.Empty; }
		}

		internal override XmlSpace XmlSpace {
			get { return OwnerElement != null ? OwnerElement.XmlSpace : XmlSpace.None; }
		}

		#endregion

		#region Methods

#if NET_2_0
		public override XmlNode AppendChild (XmlNode newChild)
		{
			return base.AppendChild (newChild);
		}

		public override XmlNode InsertBefore (XmlNode newChild, XmlNode refChild)
		{
			return base.InsertBefore (newChild, refChild);
		}

		public override XmlNode InsertAfter (XmlNode newChild, XmlNode refChild)
		{
			return base.InsertAfter (newChild, refChild);
		}

		public override XmlNode PrependChild (XmlNode newChild)
		{
			return base.PrependChild (newChild);
		}

		public override XmlNode RemoveChild (XmlNode oldChild)
		{
			return base.RemoveChild (oldChild);
		}

		public override XmlNode ReplaceChild (XmlNode newChild, XmlNode oldChild)
		{
			return base.ReplaceChild (newChild, oldChild);
		}
#endif

		public override XmlNode CloneNode (bool deep)
		{
			XmlNode node = OwnerDocument.CreateAttribute (name.Prefix, name.LocalName, name.NS, true, false);
			if (deep) {
				for (XmlNode n = FirstChild; n != null; n = n.NextSibling)
					node.AppendChild (n.CloneNode (deep), false);
			}

			return node;
		}

		internal void SetDefault ()
		{
			isDefault = true;
		}

		public override void WriteContentTo (XmlWriter w)
		{
			for (XmlNode n = FirstChild; n != null; n = n.NextSibling)
				n.WriteTo (w);
		}

		public override void WriteTo (XmlWriter w)
		{
			if (isDefault)
				return; // Write nothing.
			w.WriteStartAttribute (name.NS.Length > 0 ? name.Prefix : String.Empty, name.LocalName, name.NS);
			WriteContentTo (w);
			w.WriteEndAttribute ();
		}

		internal DTDAttributeDefinition GetAttributeDefinition ()
		{
			if (OwnerElement == null)
				return null;

			// If it is default, then directly create new attribute.
			DTDAttListDeclaration attList = OwnerDocument.DocumentType != null ? OwnerDocument.DocumentType.DTD.AttListDecls [OwnerElement.Name] : null;
			return attList != null ? attList [Name] : null;
		}
		#endregion
	}
}
