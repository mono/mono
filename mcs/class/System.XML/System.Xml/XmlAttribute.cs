//
// System.Xml.XmlAttribute
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//
// (C) 2002 Jason Diamond  http://injektilo.org/
//

using System;

namespace System.Xml
{
	public class XmlAttribute : XmlNode
	{
		#region Fields

		private XmlElement ownerElement;
		private XmlLinkedNode lastChild;
		private string localName;
		private string namespaceURI;
		private string prefix;

		#endregion

		#region Constructor

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

		[MonoTODO]
		public override string BaseURI {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public override string InnerText {
			get {
				throw new NotImplementedException ();
			}

			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public override string InnerXml {
			get {
				throw new NotImplementedException ();
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

		[MonoTODO]
		public override XmlNode ParentNode {
			get {
				return null;
			}
		}

		public override string Prefix {
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

		[MonoTODO]
		public override XmlNode CloneNode (bool deep)
		{
			throw new NotImplementedException ();
		}

		internal void SetOwnerElement (XmlElement ownerElement)
		{
			this.ownerElement = ownerElement;
		}

		[MonoTODO]
		public override void WriteContentTo(XmlWriter w)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteTo(XmlWriter w)
		{
			throw new NotImplementedException ();
		}

		#endregion

		internal override XmlLinkedNode LastLinkedChild {
			get	{
				return lastChild;
			}

			set {
				lastChild = value;
			}
		}
	}
}
