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
#endif

internal class XmlHelper {

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
	
#if !WINDOWS_PHONE && !NETFX_CORE
		internal static string GetLocalName (XmlElement el)
		{
			return el.LocalName;
		}
		internal static string GetNamespaceUri (XmlElement el)
		{
			return el.NamespaceURI;
		}
		internal static string GetPrefix (XmlElement el)
		{
			return el.Prefix;
		}
		internal static string GetLocalName (XmlAttribute attr)
		{
			return attr.LocalName;
		}
		internal static string GetNamespaceUri (XmlAttribute attr)
		{
			return attr.NamespaceURI;
		}
		internal static string GetPrefix (XmlElement el, XmlAttribute attr)
		{
			return attr.Prefix;
		}
		internal static XmlElement GetRootElement (XmlDocument doc)
		{
			return doc.DocumentElement;
		}
		internal static XmlAttributeCollection GetAttributes (XmlElement el)
		{
			return el.Attributes;
		}
		internal static string GetAttribute (XmlElement el, string name)
		{
			return el.GetAttribute (name);
		}
		internal static string GetAttribute (XmlElement el, string name, string ns)
		{
			return el.GetAttribute (name, ns);
		}
		internal static XmlNodeList GetChildNodes (XmlElement el)
		{
			return el.ChildNodes;
		}
		internal static XmlNode GetNextSibling (XmlNode n)
		{
			return n.NextSibling;
		}
		internal static XmlElement GetFirstElement (XmlElement n)
		{
			return n.FirstChild as XmlElement;
		}
		internal static XmlNodeList GetSiblings(XmlElement n)
		{
			if (n.ParentNode == null)
			{
				return null;
			}
			return n.ParentNode.ChildNodes;
		}
#else
		internal static string GetLocalName (XElement el)
		{
			return el.Name.LocalName;
		}
		internal static string GetNamespaceUri (XElement el)
		{
			return el.Name.NamespaceName;
		}
		internal static string GetPrefix (XElement el)
		{
			return el.GetPrefixOfNamespace(el.Name.Namespace);
		}
		internal static string GetLocalName (XAttribute attr)
		{
			return attr.Name.LocalName;
		}
		internal static string GetNamespaceUri (XAttribute attr)
		{
			return attr.Name.NamespaceName;
		}
		internal static string GetPrefix (XElement el, XAttribute attr)
		{
			return el.GetPrefixOfNamespace (attr.Name.Namespace);
		}
		internal static XElement GetRootElement (XDocument doc)
		{
			return doc.Root;
		}
		internal static IEnumerable<XAttribute> GetAttributes (XElement el)
		{
			return el.Attributes ();
		}
		internal static string GetAttribute (XElement el, string name)
		{
			// TODO: possible null checks
			return el.Attribute (name).Value;
		}
		internal static string GetAttribute (XElement el, string name, string ns)
		{
			// TODO: possible null checks
			return el.Attribute(XNamespace.Get(ns) + name).Value;
		}
		internal static IEnumerable<XNode> GetChildNodes (XElement el)
		{
			return el.Nodes ();
		}
		internal static XNode GetNextSibling (XNode n)
		{
			return n.NextNode;
		}
		internal static XElement GetFirstElement (XElement n)
		{
			return n.FirstNode as XElement;
		}
		internal static IEnumerable<XNode> GetSiblings(XElement n)
		{
			if (n.Parent == null)
			{
				return null;
			}
			return n.Parent.Nodes();
		}
#endif
}
