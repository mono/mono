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

using System;
using System.Text;
using System.Xml.XPath;

namespace System.Xml
{
	public class XmlAttribute : XmlNode
	{
		#region Fields

		private string localName;
		private string namespaceURI;
		private string prefix;
		internal bool isDefault;
		private XmlElement ownerElement;

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
			if (prefix == null)
				prefix = String.Empty;
			if (namespaceURI == null)
				namespaceURI = String.Empty;

			// Prefix "xml" should be also checked (http://www.w3.org/XML/xml-names-19990114-errata#NE05)
			// but MS.NET ignores such case.
			if (prefix == "xmlns" || (prefix == "" && localName == "xmlns"))
				if (namespaceURI != XmlNamespaceManager.XmlnsXmlns)
					throw new ArgumentException ("Invalid attribute namespace for namespace declaration.");
			else if (prefix == "xml" && namespaceURI != XmlNamespaceManager.XmlnsXml)
					throw new ArgumentException ("Invalid attribute namespace for namespace declaration.");

			// There are no means to identify the DOM is namespace-
			// aware or not, so we can only check Name validity.
			if (prefix != "" && !XmlChar.IsName (prefix))
				throw new ArgumentException ("Invalid attribute prefix.");
			else if (!XmlChar.IsName (localName))
				throw new ArgumentException ("Invalid attribute local name.");

			if (atomizedNames) {
				this.prefix = prefix;
				this.localName = localName;
				this.namespaceURI = namespaceURI;
			} else {
				this.prefix = doc.NameTable.Add (prefix);
				this.localName = doc.NameTable.Add (localName);
				this.namespaceURI = doc.NameTable.Add (namespaceURI);
			}
		}

		#endregion

		#region Properties

		public override string BaseURI {
			get {
				return OwnerElement.BaseURI;
			}
		}

		public override string InnerText {
			get {
				return base.InnerText;
			}

			set {
				Value = value;
			}
		}

		public override string InnerXml {
			get {
				// Not sure why this is an override.  Passing through for now.
				return base.InnerXml;
			}

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
				return localName;
			}
		}

		public override string Name {
			get { 
				return prefix != String.Empty ? prefix + ":" + localName : localName; 
			}
		}

		public override string NamespaceURI {
			get {
				return namespaceURI;
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
			get {
				return ownerElement;
			}
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
				if (!XmlChar.IsNCName (value))
					throw new ArgumentException ("Specified name is not a valid NCName: " + value);
				if (prefix == "xmlns" && value != "xmlns")
					throw new ArgumentException ("Cannot bind to the reserved namespace.");

				prefix = OwnerDocument.NameTable.Add (value);
			}
			
			get {
				return prefix;
			}
		}

		public virtual bool Specified {
			get {
				return !isDefault;
			}
		}

		public override string Value {
			get {
				XmlNode firstChild = FirstChild;
				if (firstChild == null)
					return String.Empty;
				return firstChild.Value;
			}

			set {
				XmlNode firstChild = FirstChild;
				if (firstChild == null)
					AppendChild (OwnerDocument.CreateTextNode (value));
				else if (FirstChild.NextSibling != null) {
					this.RemoveAll ();
					AppendChild (OwnerDocument.CreateTextNode (value));
				}
				else
					firstChild.Value = value;
				isDefault = false;
			}
		}

		internal override string XmlLang {
			get { return OwnerElement.XmlLang; }
		}

		internal override XmlSpace XmlSpace {
			get { return OwnerElement.XmlSpace; }
		}

		#endregion

		#region Methods

		public override XmlNode CloneNode (bool deep)
		{
			XmlNode node = new XmlAttribute (prefix, localName, namespaceURI,
							 OwnerDocument, true, false);
			if (deep) {
				foreach (XmlNode child in this.ChildNodes)
					node.AppendChild (child.CloneNode (deep));
			}

			return node;
		}

		internal void SetDefault ()
		{
			isDefault = true;
		}

		// Parent of XmlAttribute must be null
		internal void SetOwnerElement (XmlElement el) {
			ownerElement = el;
		}

		public override void WriteContentTo (XmlWriter w)
		{
			foreach (XmlNode n in ChildNodes)
				n.WriteTo (w);
		}

		public override void WriteTo (XmlWriter w)
		{
			w.WriteStartAttribute (prefix, localName, namespaceURI);
			WriteContentTo (w);
			w.WriteEndAttribute ();
		}

		#endregion
	}
}
