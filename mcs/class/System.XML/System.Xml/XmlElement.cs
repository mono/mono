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
	public class XmlElement : XmlLinkedNode
	{
		#region Fields

		private XmlAttributeCollection attributes;
		private XmlLinkedNode lastChild;
		private string localName;
		private string namespaceURI;
		private string prefix;

		#endregion

		#region Constructor

		protected internal XmlElement (
			string prefix, 
			string localName, 
			string namespaceURI, 
			XmlDocument doc) : base (doc)
		{
			this.prefix = prefix;
			this.localName = localName;
			this.namespaceURI = namespaceURI;

			attributes = new XmlAttributeCollection (this);
		}

		#endregion

		#region Properties

		public override XmlAttributeCollection Attributes {
			get { 
				return attributes; 
			}
		}

		public virtual bool HasAttributes {
			get { 
				return attributes.Count > 0; 
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

		[MonoTODO]
		public bool IsEmpty	{
			get { 
				throw new NotImplementedException (); 
			}

			set { 
				throw new NotImplementedException (); 
			}
		}

		internal override XmlLinkedNode LastLinkedChild
		{
			get	
			{
				return lastChild;
			}
			/// set'er Should only be called by XmlNode.AppendChild().
			set 
			{
				// This is our special case for clearing out all children.
				// XmlNode.RemoveAll() will call this method passing in
				// a null node.
				if (value == null)
				{
					// This should allow the GC to collect up our circular list
					// that we no longer have a reference to.
					lastChild = null;
					return;
				}

				if (LastChild == null) 
				{
					lastChild = value;
					LastLinkedChild.NextLinkedSibling = null;
				}
				
				value.NextLinkedSibling = LastLinkedChild.NextLinkedSibling;
				LastLinkedChild.NextLinkedSibling = value;
				lastChild = value;

				SetParentNode(this);
			}
		}
		
		public override string LocalName 
		{
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

		[MonoTODO]
		public override XmlNode NextSibling {
			get { 
				return base.NextSibling; 
			}
		}

		public override XmlNodeType NodeType {
			get { 
				return XmlNodeType.Element; 
			}
		}

		[MonoTODO]
		public override XmlDocument OwnerDocument {
			get { 
				return base.OwnerDocument; 
			}
		}

		public override string Prefix {
			get { 
				return prefix; 
			}
		}

		#endregion

		#region Methods

		[MonoTODO]
		public override XmlNode CloneNode (bool deep)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string GetAttribute (string name)
		{
			XmlNode attributeNode = Attributes.GetNamedItem (name);
			return attributeNode != null ? attributeNode.Value : String.Empty;
		}

		[MonoTODO]
		public virtual string GetAttribute (string localName, string namespaceURI)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlAttribute GetAttributeNode (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlAttribute GetAttributeNode (string localName, string namespaceURI)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlNodeList GetElementsByTagName (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlNodeList GetElementsByTagName (string localName, string namespaceURI)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool HasAttribute (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool HasAttribute (string localName, string namespaceURI)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Don't remove default attributes.")]
		public override void RemoveAll ()
		{
			// Remove the child nodes.
			base.RemoveAll ();

			// Remove all attributes.
			attributes.RemoveAll ();
		}

		[MonoTODO]
		public virtual void RemoveAllAttributes ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void RemoveAttribute (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void RemoveAttribute (string localName, string namespaceURI)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlNode RemoveAttributeAt (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlAttribute RemoveAttributeNode (XmlAttribute oldAttr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlAttribute RemoveAttributeNode (string localName, string namespaceURI)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void SetAttribute (string name, string value)
		{
			XmlAttribute attribute = OwnerDocument.CreateAttribute (name);
			attribute.SetOwnerElement (this);
			attribute.Value = value;
			Attributes.SetNamedItem (attribute);
		}

		[MonoTODO]
		public virtual void SetAttribute (string localName, string namespaceURI, string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlAttribute SetAttributeNode (XmlAttribute newAttr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlAttribute SetAttributeNode (string localName, string namespaceURI)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteContentTo (XmlWriter w)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteTo (XmlWriter w)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}
