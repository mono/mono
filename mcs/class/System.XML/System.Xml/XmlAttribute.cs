//
// System.Xml.XmlAttribute
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//
// (C) 2002 Jason Diamond  http://injektilo.org/
//

using System;
using System.Text;
using System.Xml.XPath;

namespace System.Xml
{
	public class XmlAttribute : XmlNode
	{
		#region Fields

		private XmlLinkedNode lastChild;
		private string localName;
		private string namespaceURI;
		private string prefix;
		internal bool isDefault;
		private XmlElement ownerElement;

		#endregion

		#region Constructor

		[MonoTODO("need to set namespaceURI if prefix is recognized built-in ones like xmlns")]
		protected internal XmlAttribute (
			string prefix, 
			string localName, 
			string namespaceURI, 
			XmlDocument doc) : base (doc)
		{
			// What to be recognized is: xml:space, xml:lang, xml:base, and
			// xmlns and xmlns:* (when XmlDocument.Namespaces = true only)
			this.prefix = prefix;
			this.localName = localName;
			this.namespaceURI = namespaceURI;
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
				StringBuilder builder = new StringBuilder ();
				AppendChildValues (this, builder);
				return builder.ToString ();
                        }

			set {
				Value = value;
			}
		}

		private void AppendChildValues (XmlNode parent, StringBuilder builder)
		{
			XmlNode node = parent.FirstChild;
			
			while (node != null) {
				builder.Append (node.Value);
				AppendChildValues (node, builder);
				node = node.NextSibling;
                        }
                }
		
		[MonoTODO ("Setter is incomplete(XmlTextReader.ReadAttribute is incomplete;No resolution for xml:lang/space")]
		public override string InnerXml {
			get {
				// Not sure why this is an override.  Passing through for now.
				return base.InnerXml;
			}

			set {
				XmlNamespaceManager nsmgr = ConstructNamespaceManager ();
				XmlParserContext ctx = new XmlParserContext (OwnerDocument.NameTable, nsmgr, XmlLang, this.XmlSpace);
				XmlTextReader xtr = OwnerDocument.ReusableReader;
				xtr.SetReaderContext (BaseURI, ctx);
				xtr.SetReaderFragment (new System.IO.StringReader ("'" + value.Replace ("'", "&apos;") + "'"), XmlNodeType.Attribute);
				xtr.ReadAttributeValue ();
				Value = xtr.Value;
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

		[MonoTODO("setter incomplete (name character check, format check, wrong prefix&nsURI)")]
		// We gotta do more in the set block here
		// We need to do the proper tests and throw
		// the correct Exceptions
		//
		// Wrong cases are: (1)check readonly, (2)check character validity,
		// (3)check format validity, (4)this is attribute and qualifiedName != "xmlns"
		// (5)when argument is 'xml' or 'xmlns' and namespaceURI doesn't match
		public override string Prefix {
			set {
				if(IsReadOnly)
					throw new XmlException ("This node is readonly.");

				XmlNamespaceManager nsmgr = ConstructNamespaceManager ();
				string nsuri = nsmgr.LookupNamespace (value);
				if(nsuri == null)
					throw new XmlException ("Namespace URI not found for this prefix");

				prefix = value;
			}
			
			get {
				return prefix;
			}
		}

		[MonoTODO("There are no code which sets 'specified = true', so this logic is without checking.")]
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
				else
					firstChild.Value = value;
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
							 OwnerDocument);
			if (deep) {
				while ((node != null) && (node.HasChildNodes)) {
					AppendChild (node.NextSibling.CloneNode (true));
					node = node.NextSibling;
				}
			}

			return node;
		}

		// Parent of XmlAttribute must be null
		internal void SetOwnerElement (XmlElement el) {
			ownerElement = el;
		}

		public override void WriteContentTo (XmlWriter w)
		{
			w.WriteString (Value);
		}

		public override void WriteTo (XmlWriter w)
		{
			w.WriteAttributeString (prefix, localName, namespaceURI, Value);
		}

		#endregion

		internal override XmlLinkedNode LastLinkedChild {
			get { return lastChild; }

			set { lastChild = value; }
		}
	}
}
