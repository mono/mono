//
// System.Xml.XmlAttribute
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//
// (C) 2002 Jason Diamond  http://injektilo.org/
//

using System;
using System.Collections;

namespace System.Xml
{
	public class XmlElement : XmlLinkedNode
	{
		#region Fields

		private XmlAttributeCollection attributes;
		private XmlLinkedNode lastLinkedChild;
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
			get { return attributes; }
		}

		public virtual bool HasAttributes {
			get { return attributes.Count > 0; }
		}

		[MonoTODO ("Setter.")]
		public override string InnerXml {
			get {
				// Not sure why this is an override.  Passing through for now.
				return base.InnerXml;
			}
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public bool IsEmpty {
			get { throw new NotImplementedException (); }

			set { throw new NotImplementedException (); }
		}

		internal override XmlLinkedNode LastLinkedChild {
			get { return lastLinkedChild; }

			set { lastLinkedChild = value; }
		}
		
		public override string LocalName {
			get { return localName; }
		}

		public override string Name {
			get { 
				return prefix != String.Empty ? prefix + ":" + localName : localName; 
			}
		}

		public override string NamespaceURI {
			get { return namespaceURI; }
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
			XmlNode node =  new XmlElement (prefix, localName, namespaceURI,
							OwnerDocument);

			for (int i = 0; i < node.Attributes.Count; i++)
				node.AppendChild (node.Attributes [i].CloneNode (false));
			
			if (deep) {
				while ((node != null) && (node.HasChildNodes)) {					
					AppendChild (node.NextSibling.CloneNode (true));
					node = node.NextSibling;
				}
			} // shallow cloning
				
			//
			// Reminder: Also look into Default attributes.
			//
			return node;
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
			XmlNode attributeNode = Attributes.GetNamedItem (localName, namespaceURI);
			return attributeNode != null ? attributeNode.Value : String.Empty;
		}

		[MonoTODO]
		public virtual XmlAttribute GetAttributeNode (string name)
		{
			XmlNode attributeNode = Attributes.GetNamedItem (name);
			return attributeNode != null ? attributeNode as XmlAttribute : null;
		}

		[MonoTODO]
		public virtual XmlAttribute GetAttributeNode (string localName, string namespaceURI)
		{
			XmlNode attributeNode = Attributes.GetNamedItem (localName, namespaceURI);
			return attributeNode != null ? attributeNode as XmlAttribute : null;
		}

		public virtual XmlNodeList GetElementsByTagName (string name)
		{
			ArrayList nodeArrayList = new ArrayList ();
			this.searchNodesRecursively (this, name, String.Empty, nodeArrayList);
			return new XmlNodeArrayList (nodeArrayList);
		}

		private void searchNodesRecursively (XmlNode argNode, string argName, string argNamespaceURI, 
			ArrayList argArrayList)
		{
			XmlNodeList xmlNodeList = argNode.ChildNodes;
			foreach (XmlNode node in xmlNodeList)
			{
				if (node.LocalName.Equals (argName) && node.NamespaceURI.Equals (argNamespaceURI))
					argArrayList.Add (node);
				else	
					this.searchNodesRecursively (node, argName, argNamespaceURI, argArrayList);
			}
		}

		public virtual XmlNodeList GetElementsByTagName (string localName, string namespaceURI)
		{
			ArrayList nodeArrayList = new ArrayList ();
			this.searchNodesRecursively (this, localName, namespaceURI, nodeArrayList);
			return new XmlNodeArrayList (nodeArrayList);
		}

		[MonoTODO]
		public virtual bool HasAttribute (string name)
		{
			XmlNode attributeNode = Attributes.GetNamedItem (name);
			return attributeNode != null;
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
			attribute.SetParentNode (this);
			attribute.Value = value;
			Attributes.SetNamedItem (attribute);
		}

		[MonoTODO]
		public virtual string SetAttribute (string localName, string namespaceURI, string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlAttribute SetAttributeNode (XmlAttribute newAttr)
		{
			newAttr.SetParentNode(this);
			XmlNode oldAttr = Attributes.SetNamedItem(newAttr);
			return oldAttr != null ? oldAttr as XmlAttribute : null;
		}

		[MonoTODO]
		public virtual XmlAttribute SetAttributeNode (string localName, string namespaceURI)
		{
			throw new NotImplementedException ();
		}

		public override void WriteContentTo (XmlWriter w)
		{
			foreach(XmlNode childNode in ChildNodes)
				childNode.WriteTo(w);
		}

		public override void WriteTo (XmlWriter w)
		{
			w.WriteStartElement(Prefix, LocalName, NamespaceURI);

			foreach(XmlNode attributeNode in Attributes)
				attributeNode.WriteTo(w);

			WriteContentTo(w);

			w.WriteEndElement();
		}

		#endregion
	}
}
