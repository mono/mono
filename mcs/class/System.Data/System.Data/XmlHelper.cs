// 
// System.Data/XmlHelper.cs
//
// Author:
//   Senganal T <tsenganal@novell.com>
//
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System;
using System.Collections.Generic;
using System.Xml;
using System.Collections;
#if WINDOWS_PHONE || NETFX_CORE
using System.Xml.Linq;
using System.Linq;
using XmlAttribute = System.Xml.Linq.XAttribute;
using XmlElement = System.Xml.Linq.XElement;
using XmlNode = System.Xml.Linq.XNode;
using XmlDocument = System.Xml.Linq.XDocument;
using XmlNodeList = System.Collections.Generic.IEnumerable<System.Xml.Linq.XNode>;
using XmlAttributeCollection = System.Collections.Generic.IEnumerable<System.Xml.Linq.XAttribute>;
#endif

static class XmlHelper {

	static Hashtable localSchemaNameCache = new Hashtable ();
	static Hashtable localXmlNameCache = new Hashtable ();

	internal static string Decode (string xmlName)
	{
		string s = (string) localSchemaNameCache [xmlName];
		if (s == null) {
			s = XmlConvert.DecodeName (xmlName);
			localSchemaNameCache [xmlName] = s;
		}
		return s;
	}

	internal static string Encode (string schemaName)
	{
		string s = (string) localXmlNameCache [schemaName];
		if (s == null) {
			s = XmlConvert.EncodeLocalName (schemaName);
			localXmlNameCache [schemaName] = s;
		}
		return s;
	}

	internal static void ClearCache ()
	{
		localSchemaNameCache.Clear ();
		localXmlNameCache.Clear ();
	}
	
	internal static string GetLocalName (this XmlElement el)
	{
#if !WINDOWS_PHONE && !NETFX_CORE
		return el.LocalName;
#else
		return el.Name.LocalName;
#endif
	}
	internal static string GetNamespaceUri (this XmlElement el)
	{
#if !WINDOWS_PHONE && !NETFX_CORE
		return el.NamespaceURI;
#else
		return el.Name.NamespaceName;
#endif
	}
	internal static string GetPrefix (this XmlElement el)
	{
#if !WINDOWS_PHONE && !NETFX_CORE
		return el.Prefix;
#else
		return el.GetPrefixOfNamespace (el.Name.Namespace);
#endif
	}
	internal static bool IsNamespaceAttribute (this XmlAttribute attr)
	{
#if !WINDOWS_PHONE && !NETFX_CORE
		return attr.Prefix.Equals ("xmlns");
#else
		return attr.IsNamespaceDeclaration;
#endif
	}
	internal static string GetLocalName (this XmlAttribute attr)
	{
#if !WINDOWS_PHONE && !NETFX_CORE
		return attr.LocalName;
#else
		return attr.Name.LocalName;
#endif
	}
	internal static string GetNamespaceUri (this XmlAttribute attr)
	{
#if !WINDOWS_PHONE && !NETFX_CORE
		return attr.NamespaceURI;
#else
		return attr.Name.NamespaceName;
#endif
	}
	internal static string GetPrefix (this XmlElement el, XmlAttribute attr)
	{
		if (attr == null)
			return String.Empty;
#if !WINDOWS_PHONE && !NETFX_CORE
		return attr.Prefix;
#else
		return el.GetPrefixOfNamespace (attr.Name.Namespace);
#endif
	}
	internal static XmlElement GetRootElement (this XmlDocument doc)
	{
#if !WINDOWS_PHONE && !NETFX_CORE
		return doc.DocumentElement;
#else
		return doc.Root;
#endif
	}
	internal static XmlAttributeCollection GetAttributes (this XmlElement el)
	{
#if !WINDOWS_PHONE && !NETFX_CORE
		return el.Attributes;
#else
		return el.Attributes ();
#endif
	}
	internal static XmlNodeList GetChildNodes (this XmlElement el)
	{
#if !WINDOWS_PHONE && !NETFX_CORE
		return el.ChildNodes;
#else
		return el.Nodes ();
#endif
	}
	internal static XmlNode GetNextSibling (this XmlNode n)
	{
#if !WINDOWS_PHONE && !NETFX_CORE
		return n.NextSibling;
#else
		return n.NextNode;
#endif
	}
	internal static XmlElement GetFirstElement (this XmlElement n)
	{
#if !WINDOWS_PHONE && !NETFX_CORE
		return n.FirstChild as XmlElement;
#else
		return n.FirstNode as XElement;
#endif
	}
	internal static string GetInnerText (this XmlElement el)
	{
#if !WINDOWS_PHONE && !NETFX_CORE
		return el.InnerText;
#else
		return el.Value;
#endif
	}
	internal static XmlNodeList GetSiblings (this XmlElement n)
	{
#if !WINDOWS_PHONE && !NETFX_CORE
		return n.ParentNode.ChildNodes;
#else
		return n.Parent.Nodes ();
#endif
	}
	internal static XmlDocument CreateXmlDocument (XmlReader reader)
	{
#if !WINDOWS_PHONE && !NETFX_CORE
		XmlDocument doc = new XmlDocument ();
		doc.Load (reader);
#else
		XDocument doc = XDocument.Load (reader);
#endif
		return doc;
	}
	internal static XElement DeepClone (this XDocument doc, XElement element)
	{
#if !WINDOWS_PHONE && !NETFX_CORE
		return doc.ImportNode (element, true);
#else
		return new XElement (element);
#endif
	}
	
	// extension methods to polyfil interface

#if !WINDOWS_PHONE && !NETFX_CORE
	internal static XmlReader CreateReader (this XmlDocument doc) 
	{
		return new XmlNodeReader (doc);
	}
#else
	internal static string GetAttribute (this XmlElement el, string name)
	{
		XmlAttribute attr = el.Attribute (name);
		if (attr == null)
			return null;
		return attr.Value;
	}
	internal static string GetAttribute(this XmlElement el, string name, string ns)
	{
		XmlAttribute attr = el.Attribute (XNamespace.Get (ns) + name);
		if (attr == null)
			return null;
		return attr.Value;
	}
	internal static void AppendChild (this XContainer doc, XNode node)
	{
		doc.Add (node);
	}
	internal static XElement CreateElement (this XDocument doc, string localName)
	{
		return new XElement (localName);
	}
	internal static void Load (this XDocument doc, XmlReader reader)
	{
		doc.Add (XNode.ReadFrom (reader));
	}
	internal static string ReadElementString (this XmlReader reader)
	{
		return reader.ReadElementContentAsString ();
	}
	internal static string ReadString (this XmlReader reader)
	{
		return reader.ReadContentAsString ();
	}
#endif
}
