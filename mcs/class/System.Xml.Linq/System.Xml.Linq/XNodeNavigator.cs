//
// Authors:
//   Atsushi Enomoto
//
// Copyright 2007 Novell (http://www.novell.com)
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

#if !MOONLIGHT

using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

using XPI = System.Xml.Linq.XProcessingInstruction;

namespace System.Xml.Linq
{
	internal class XNodeNavigator : XPathNavigator
	{
		static readonly XAttribute attr_ns_xml = new XAttribute (XNamespace.Xmlns.GetName ("xml"), XNamespace.Xml.NamespaceName);

		XNode node;
		XAttribute attr;
		XmlNameTable name_table;

		public XNodeNavigator (XNode node, XmlNameTable nameTable)
		{
			this.node = node;
			this.name_table = nameTable;
		}

		public XNodeNavigator (XNodeNavigator other)
		{
			this.node = other.node;
			this.attr = other.attr;
			this.name_table = other.name_table;
		}

		public override string BaseURI {
			get { return node.BaseUri ?? String.Empty; }
		}

		public override bool CanEdit {
			get { return true; }
		}

		public override bool HasAttributes {
			get {
				if (attr != null)
					return false;
				XElement el = node as XElement;
				if (el == null)
					return false;
				foreach (var at in el.Attributes ())
					if (!at.IsNamespaceDeclaration)
						return true;
				return false;
			}
		}

		public override bool HasChildren {
			get {
				if (attr != null)
					return false;
				XContainer c = node as XContainer;
				return c != null && c.FirstNode != null;
			}
		}

		public override bool IsEmptyElement {
			get {
				if (attr != null)
					return false;
				XElement el = node as XElement;
				return el != null && el.IsEmpty;
			}
		}

		public override string LocalName {
			get {
				switch (NodeType) {
				case XPathNodeType.Namespace:
					return attr.Name.Namespace == XNamespace.None ? String.Empty : attr.Name.LocalName;
				case XPathNodeType.Attribute:
					return attr.Name.LocalName;
				case XPathNodeType.Element:
					return ((XElement) node).Name.LocalName;
				case XPathNodeType.ProcessingInstruction:
					return ((XPI) node).Target;
				default:
					return String.Empty;
				}
			}
		}

		public override string Name {
			get {
				XName name = null;
				switch (NodeType) {
				case XPathNodeType.Attribute:
					name = attr.Name;
					break;
				case XPathNodeType.Element:
					name = ((XElement) node).Name;
					break;
				default:
					return LocalName;
				}
				if (name.Namespace == XNamespace.None)
					return name.LocalName;
				XElement el = (node as XElement) ?? node.Parent;
				if (el == null)
					return name.LocalName;
				string prefix = el.GetPrefixOfNamespace (name.Namespace);
				return prefix.Length > 0 ? String.Concat (prefix, ":", name.LocalName) : name.LocalName;
			}
		}

		public override string NamespaceURI {
			get {
				switch (NodeType) {
				case XPathNodeType.ProcessingInstruction:
				case XPathNodeType.Namespace:
					return String.Empty;
				case XPathNodeType.Attribute:
					return attr.Name.NamespaceName;
				case XPathNodeType.Element:
					return ((XElement) node).Name.NamespaceName;
				default:
					return String.Empty;
				}
			}
		}

		public override XmlNameTable NameTable {
			get { return name_table; }
		}

		public override XPathNodeType NodeType {
			get {
				if (attr != null)
					return  attr.IsNamespaceDeclaration ?
						XPathNodeType.Namespace :
						XPathNodeType.Attribute;
				switch (node.NodeType) {
				case XmlNodeType.Element:
					return XPathNodeType.Element;
				case XmlNodeType.Document:
					return XPathNodeType.Root;
				case XmlNodeType.Comment:
					return XPathNodeType.Comment;
				case XmlNodeType.ProcessingInstruction:
					return XPathNodeType.ProcessingInstruction;
				default:
					return XPathNodeType.Text;
				}
			}
		}

		public override string Prefix {
			get {
				XName name = null;
				switch (NodeType) {
				case XPathNodeType.ProcessingInstruction:
				case XPathNodeType.Namespace:
					return String.Empty;
				case XPathNodeType.Attribute:
					name = attr.Name;
					break;
				case XPathNodeType.Element:
					name = ((XElement) node).Name;
					break;
				default:
					return LocalName;
				}
				if (name.Namespace == XNamespace.None)
					return String.Empty;
				XElement el = (node as XElement) ?? node.Parent;
				if (el == null)
					return String.Empty;
				return el.GetPrefixOfNamespace (name.Namespace);
			}
		}

		public override IXmlSchemaInfo SchemaInfo {
			get { return null; }
		}

		public override object UnderlyingObject {
			get { return attr != null ? (object) attr : node; }
		}

		public override string Value {
			get {
				if (attr != null)
					return attr.Value;
				else
				switch (NodeType) {
				case XPathNodeType.Comment:
					return ((XComment) node).Value;
				case XPathNodeType.ProcessingInstruction:
					return ((XPI) node).Data;
				case XPathNodeType.Text:
					string s = String.Empty;
					for (var xn = node as XText; xn != null; xn = xn.NextNode as XText)
						s += xn.Value;
					return s;
				case XPathNodeType.Element:
				case XPathNodeType.Root:
					return GetInnerText ((XContainer) node);
				}
				return String.Empty;
			}
		}

		string GetInnerText (XContainer node)
		{
			StringBuilder sb = null;
			foreach (XNode n in node.Nodes ())
				GetInnerText (n, ref sb);
			return sb != null ? sb.ToString () : String.Empty;
		}

		void GetInnerText (XNode n, ref StringBuilder sb)
		{
			switch (n.NodeType) {
			case XmlNodeType.Element:
				foreach (XNode c in ((XElement) n).Nodes ())
					GetInnerText (c, ref sb);
				break;
			case XmlNodeType.Text:
			case XmlNodeType.CDATA:
				if (sb == null)
					sb = new StringBuilder ();
				sb.Append (((XText) n).Value);
				break;
			}
		}

		public override XPathNavigator Clone ()
		{
			return new XNodeNavigator (this);
		}

		public override bool IsSamePosition (XPathNavigator other)
		{
			XNodeNavigator nav = other as XNodeNavigator;
			if (nav == null || nav.node.Owner != node.Owner)
				return false;
			return node == nav.node && attr == nav.attr;
		}

		public override bool MoveTo (XPathNavigator other)
		{
			XNodeNavigator nav = other as XNodeNavigator;
			if (nav == null || nav.node.Document != node.Document)
				return false;
			node = nav.node;
			attr = nav.attr;
			return true;
		}

		public override bool MoveToFirstAttribute ()
		{
			if (attr != null)
				return false;
			XElement el = node as XElement;
			if (el == null || !el.HasAttributes)
				return false;
			foreach (XAttribute a in el.Attributes ())
				if (!a.IsNamespaceDeclaration) {
					attr = a;
					return true;
				}
			return false;
		}

		public override bool MoveToFirstChild ()
		{
			if (attr != null)
				return false;
			XContainer c = node as XContainer;
			if (c == null || c.FirstNode == null)
				return false;
			node = c.FirstNode;
			attr = null;
			return true;
		}

		public override bool MoveToFirstNamespace (XPathNamespaceScope scope)
		{
			if (NodeType != XPathNodeType.Element)
				return false;
			for (XElement el = node as XElement; el != null; el = el.Parent) {
				foreach (XAttribute a in el.Attributes ())
					if (a.IsNamespaceDeclaration) {
						attr = a;
						return true;
					}
				if (scope == XPathNamespaceScope.Local)
					return false;
			}
			if (scope != XPathNamespaceScope.All)
				return false;
			attr = attr_ns_xml;
			return true;
		}

		public override bool MoveToId (string id)
		{
			throw new NotSupportedException ("This XPathNavigator does not support IDs");
		}

		public override bool MoveToNext ()
		{
			XNode xn = node.NextNode;
			if (node is XText)
				for (; xn != null; xn = xn.NextNode)
					if (!(xn.NextNode is XText))
						break;
			if (xn == null)
				return false;
			node = xn;
			attr = null;
			return true;
		}

		public override bool MoveToNextAttribute ()
		{
			if (attr == null)
				return false;
			if (attr.NextAttribute == null)
				return false;
			for (XAttribute a = attr.NextAttribute; a != null; a = a.NextAttribute)
				if (!a.IsNamespaceDeclaration) {
					attr = a;
					return true;
				}
			return false;
		}

		public override bool MoveToNextNamespace (XPathNamespaceScope scope)
		{
			if (attr == null)
				return false;
			for (XAttribute a = attr.NextAttribute; a != null; a = a.NextAttribute)
				if (a.IsNamespaceDeclaration) {
					attr = a;
					return true;
				}

			if (scope == XPathNamespaceScope.Local)
				return false;

			if (attr == attr_ns_xml)
				return false; // no next attribute

			for (XElement el = ((XElement) attr.Parent).Parent; el != null; el = el.Parent) {
				foreach (XAttribute a in el.Attributes ())
					if (a.IsNamespaceDeclaration) {
						attr = a;
						return true;
					}
			}
			if (scope != XPathNamespaceScope.All)
				return false;
			attr = attr_ns_xml;
			return true;
		}

		public override bool MoveToParent ()
		{
			if (attr != null) {
				attr = null;
				return true;
			}
			if (node.Owner == null)
				return false;
			node = node.Owner;
			return true;
		}

		public override bool MoveToPrevious ()
		{
			if (node.PreviousNode == null)
				return false;
			node = node.PreviousNode;
			attr = null;
			return true;
		}

		public override void MoveToRoot ()
		{
			node = node.Document;
			attr = null;
		}
	}
}

#endif
