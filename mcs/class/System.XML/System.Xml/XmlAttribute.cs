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

		#endregion

		#region Constructor

		[MonoTODO("need to set namespaceURI if prefix is recognized built-in ones like xmlns")]
		protected internal XmlAttribute (
			string prefix, 
			string localName, 
			string namespaceURI, 
			XmlDocument doc) : base (doc)
		{
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

		[MonoTODO ("Setter")]
		public override string InnerText {
			get {
				StringBuilder builder = new StringBuilder ();
				AppendChildValues (this, builder);
				return builder.ToString ();
                        }

			set {
				throw new NotImplementedException ();
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
		
		[MonoTODO ("Setter.")]
		public override string InnerXml {
			get {
				// Not sure why this is an override.  Passing through for now.
				return base.InnerXml;
			}

			set {
				throw new NotImplementedException ();
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
				return base.ParentNode as XmlElement;
			}
		}

		[MonoTODO]
		public override XmlNode ParentNode {
			get {
				return null;
			}
		}

		[MonoTODO]
		// We gotta do more in the set block here
		// We need to do the proper tests and throw
		// the correct Exceptions
		public override string Prefix {
			set {
				prefix = value;
			}
			
			get {
				return prefix;
			}
		}

		[MonoTODO]
		public virtual bool Specified {
			get {
				throw new NotImplementedException ();
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
