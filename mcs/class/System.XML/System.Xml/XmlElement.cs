//
// System.Xml.XmlElement
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//
// (C) 2002 Jason Diamond  http://injektilo.org/
//

using System;
using System.Collections;
using System.Xml.XPath;
using System.Text;

namespace System.Xml
{
	public class XmlElement : XmlLinkedNode
	{
		#region Fields

		private XmlAttributeCollection attributes;
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

			// TODO: Adds default attributes
			if(doc.DocumentType != null)
			{
			}
		}

		#endregion

		#region Properties

		public override XmlAttributeCollection Attributes {
			get { return attributes; }
		}

		public virtual bool HasAttributes {
			get { return attributes.Count > 0; }
		}

		public override string InnerText {
			get {
				return base.InnerText;
			}
			set {
				foreach(XmlNode n in ChildNodes)
				{
					this.RemoveChild(n);
				}
				// creates new Text node
				AppendChild(OwnerDocument.CreateTextNode(value));
			}
		}

		[MonoTODO ("Setter is immature")]
		public override string InnerXml {
			get {
				// Not sure why this is an override.  Passing through for now.
				return base.InnerXml;
			}
			set {
				foreach(XmlNode n in ChildNodes)
				{
					this.RemoveChild(n);
				}		  

				// How to get xml:lang and xml:space? Create logic as ConstructNamespaceManager()?
				XmlNameTable nt = this.OwnerDocument.NameTable;
				XmlNamespaceManager nsmgr = this.ConstructNamespaceManager(); //new XmlNamespaceManager(nt);
				string lang = "";
				XmlSpace space = XmlSpace.Default;

				XmlParserContext ctx = new XmlParserContext(nt, nsmgr, lang, space);
				XmlTextReader xmlReader = new XmlTextReader(value, this.NodeType, ctx);
				this.ConstructDOM(xmlReader, this);
			}
		}

		[MonoTODO]
		public bool IsEmpty {
			get { throw new NotImplementedException (); }

			set { throw new NotImplementedException (); }
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

		internal override XPathNodeType XPathNodeType {
			get {
				return XPathNodeType.Element;
			}
		}

		[MonoTODO]
		public override XmlDocument OwnerDocument {
			get { 
				return base.OwnerDocument; 
			}
		}

		public override string Prefix {
			get { return prefix; }
			set { prefix = value; }
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
			this.searchNodesRecursively (this, name, nodeArrayList);
			return new XmlNodeArrayList (nodeArrayList);
		}

		private void searchNodesRecursively (XmlNode argNode, string argName, 
			ArrayList argArrayList)
		{
			XmlNodeList xmlNodeList = argNode.ChildNodes;
			foreach (XmlNode node in xmlNodeList){
				if (node.Name.Equals (argName))
					argArrayList.Add (node);
				else	
					this.searchNodesRecursively (node, argName, argArrayList);
			}
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
			XmlNode attributeNode = Attributes.GetNamedItem (localName, namespaceURI);
			return attributeNode != null;
		}

		[MonoTODO ("confirm not removing default attributes [when DTD feature was implemented.")]
		public override void RemoveAll ()
		{
			// Remove the child nodes.
			base.RemoveAll ();

			// Remove all attributes.
			attributes.RemoveAll ();
		}

		[MonoTODO ("confirm not removing default attributes [when DTD feature was implemented.")]
		public virtual void RemoveAllAttributes ()
		{
			attributes.RemoveAll ();
		}

		[MonoTODO ("confirm not resetting default attributes [when DTD feature was implemented.")]
		public virtual void RemoveAttribute (string name)
		{
			attributes.Remove((XmlAttribute)attributes.GetNamedItem(name));
		}

		[MonoTODO ("confirm not resetting default attributes [when DTD feature was implemented.")]
		public virtual void RemoveAttribute (string localName, string namespaceURI)
		{
			attributes.Remove((XmlAttribute)attributes.GetNamedItem(localName, namespaceURI));
		}

		[MonoTODO ("confirm not resetting default attributes [when DTD feature was implemented.")]
		public virtual XmlNode RemoveAttributeAt (int i)
		{
			return attributes.Remove(attributes[i]);
		}

		[MonoTODO ("confirm not resetting default attributes [when DTD feature was implemented.")]
		public virtual XmlAttribute RemoveAttributeNode (XmlAttribute oldAttr)
		{
			return attributes.Remove(oldAttr);
		}

		[MonoTODO ("confirm not resetting default attributes [when DTD feature was implemented.")]
		public virtual XmlAttribute RemoveAttributeNode (string localName, string namespaceURI)
		{
			return attributes.Remove(attributes[localName, namespaceURI]);
		}

		[MonoTODO]
		public virtual void SetAttribute (string name, string value)
		{
			XmlAttribute attribute = OwnerDocument.CreateAttribute (name);
			attribute.SetOwnerElement(this);
			attribute.Value = value;
			Attributes.SetNamedItem (attribute);
		}

//		[MonoTODO]
		public virtual string SetAttribute (string localName, string namespaceURI, string value)
		{
			XmlAttribute attr = attributes[localName, namespaceURI];
			if(attr == null)
			{
				attr = OwnerDocument.CreateAttribute(localName, namespaceURI);
				attr.Value = value;
				attributes.SetNamedItem(attr);
			}
			else
				attr.Value = value;
			return attr.Value;
		}

//		[MonoTODO]
		public virtual XmlAttribute SetAttributeNode (XmlAttribute newAttr)
		{
			newAttr.SetOwnerElement(this);
			XmlNode oldAttr = Attributes.SetNamedItem(newAttr);
			return oldAttr != null ? oldAttr as XmlAttribute : null;
		}

		public virtual XmlAttribute SetAttributeNode (string localName, string namespaceURI)
		{
			XmlDocument xmlDoc = this.OwnerDocument;
			XmlAttribute xmlAttribute = new XmlAttribute (String.Empty, localName, namespaceURI, xmlDoc);
			return this.attributes.Append (xmlAttribute);
		}

		public override void WriteContentTo (XmlWriter w)
		{
			foreach(XmlNode childNode in ChildNodes)
				childNode.WriteTo(w);
		}

		[MonoTODO("indenting feature is incomplete.")]
		public override void WriteTo (XmlWriter w)
		{
			w.WriteStartElement(Prefix, LocalName, NamespaceURI);

			// write namespace declarations(if not exist)
			if(Prefix != null && w.LookupPrefix(Prefix) != NamespaceURI)
				w.WriteAttributeString("xmlns", Prefix, "http://www.w3.org/2000/xmlns/", NamespaceURI);

			foreach(XmlNode attributeNode in Attributes)
			{
				attributeNode.WriteTo(w);
				// write namespace declarations(if not exist)
				if(attributeNode.Prefix != null && w.LookupPrefix(attributeNode.Prefix) != attributeNode.NamespaceURI)
					w.WriteAttributeString("xmlns", attributeNode.Prefix, "http://www.w3.org/2000/xmlns/", attributeNode.NamespaceURI);
			}

			// indent(when PreserveWhitespace = false)
			// Only XmlWriter has this XmlElement's xml:space information;-)
			if(!OwnerDocument.PreserveWhitespace && w.XmlSpace != XmlSpace.Preserve)
			{
				XmlNode n = this;
				StringBuilder sb = new StringBuilder();
				while(n != OwnerDocument)
				{
					sb.Append('\t');
					n = n.ParentNode;
				}
				w.WriteWhitespace(sb.ToString());
			}

			WriteContentTo(w);

			// indent (linefeeding)
			if(!OwnerDocument.PreserveWhitespace && w.XmlSpace != XmlSpace.Preserve)
				w.WriteWhitespace("\n");

			w.WriteEndElement();
		}

		#endregion
	}
}
