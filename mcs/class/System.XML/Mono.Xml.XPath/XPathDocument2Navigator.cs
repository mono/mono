//
// Mono.Xml.XPath.XPathDocument2Navigator.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
//
// read-only XPathNavigator for editable XPathDocument.
//

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
#if NET_2_0
using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace Mono.Xml.XPath
{
	public class XomNavigator : XPathNavigator, IHasXomNode
	{
		XomRoot root;
		XomNode current;
		XomNamespace currentNS;

		public XomNavigator (XomRoot root)
		{
			this.root = root;
			current = root;
		}

		public override string BaseURI {
			get { return current.BaseURI; }
		}

		public override bool IsEmptyElement {
			get { return currentNS == null && current.IsEmptyElement; }
		}

		public override bool HasChildren {
			get { return currentNS == null && current.ChildCount != 0; }
		}

		public override bool HasAttributes {
			get {
				if (currentNS != null || current.NodeType != XPathNodeType.Element)
					return false;
				XomElement el = current as XomElement;
				return el.AttributeCount != 0;
			}
		}

		XomNode IHasXomNode.GetNode ()
		{
			return currentNS != null ? currentNS : current;
		}

		public override string GetAttribute (string name, string ns)
		{
			if (currentNS != null || current.NodeType != XPathNodeType.Element)
				return String.Empty;
			XomElement el = current as XomElement;
			XomAttribute attr = el.GetAttribute (name, ns);
			return attr != null ? attr.Value : String.Empty;
		}

		public override XmlNameTable NameTable {
			get { return current.Root.NameTable; }
		}

		public override string LocalName {
			get { return current.LocalName; }
		}

		public override string Name {
			get { return current.PrefixedName; }
		}

		public override string NamespaceURI {
			get { return current.Namespace; }
		}

		public override string Prefix {
			get { return current.Prefix; }
		}

		public override string Value {
			get {
				switch (NodeType) {
				case XPathNodeType.Attribute:
				case XPathNodeType.Comment:
				case XPathNodeType.ProcessingInstruction:
					return current.Value;
				case XPathNodeType.Text:
				case XPathNodeType.Whitespace:
				case XPathNodeType.SignificantWhitespace:
					string value = current.Value;
					for (XomNode n = current.NextSibling; n != null; n = n.NextSibling) {
						switch (n.NodeType) {
						case XPathNodeType.Text:
						case XPathNodeType.Whitespace:
						case XPathNodeType.SignificantWhitespace:
							value += n.Value;
							continue;
						}
						break;
					}
					return value;
				case XPathNodeType.Element:
				case XPathNodeType.Root:
					return current.Value;
				case XPathNodeType.Namespace:
					return currentNS.Value;
				}
				return String.Empty;
			}
		}

		public override string XmlLang {
			get { return current.XmlLang; }
		}

		public override XPathNodeType NodeType {
			get { return current.NodeType; }
		}

		public override XPathNavigator Clone ()
		{
			XomNavigator n = new XomNavigator (root);
			n.current = current;
			n.currentNS = currentNS;
			return n;
		}

		public override void MoveToRoot ()
		{
			current = root;
			currentNS = null;
		}

		public override bool MoveToParent ()
		{
			if (currentNS != null) {
				currentNS = null;
				return true;
			}
			XomParentNode parent = current.Parent;
			if (parent == null)
				return false;
			current = parent;
			return true;
		}

		public override bool MoveToFirstChild ()
		{
			if (currentNS != null || current.NodeType == XPathNodeType.Attribute || current.ChildCount == 0)
				return false;
			current = current.FirstChild;
			return true;
		}

		public override bool MoveToFirst ()
		{
			if (currentNS != null || current.NodeType == XPathNodeType.Attribute)
				return false;
			XomParentNode pn = current.Parent;
			if (pn == null)
				return false;
			XomNode n = pn.FirstChild;
			if (n == current)
				return false;
			current = n;
			return true;
		}

		public override bool MoveToFirstAttribute ()
		{
			if (currentNS != null)
				return false;
			XomElement el = current as XomElement;
			if (el == null)
				return false;
			if (el.AttributeCount == 0)
				return false;
			current = el.GetAttribute (0);
			return true;
		}

		public override bool MoveToNextAttribute ()
		{
			XomAttribute attr = current as XomAttribute;
			if (attr == null)
				return false;
			XomElement owner = attr.Parent as XomElement;
			if (owner == null)
				return false;
			XomAttribute next = owner.GetNextAttribute (attr);
			if (next == null)
				return false;
			current = next;
			return true;
		}

		public override bool MoveToAttribute (string name, string ns)
		{
			if (currentNS != null || current.NodeType != XPathNodeType.Element)
				return false;
			XomElement el = current as XomElement;
			XomAttribute attr = el.GetAttribute (name, ns);
			if (attr == null)
				return false;
			current = attr;
			return true;
		}

		public override string GetNamespace (string prefix)
		{
			if (currentNS != null)
				return String.Empty;
			XomNamespace n = GetXomNamespace (prefix);
			if (n == null)
				return String.Empty;
			else
				return n.Namespace;
		}
		
		private XomNamespace GetXomNamespace (string prefix)
		{
			if (current.NodeType != XPathNodeType.Element)
				return null;
			if (prefix == "xml")
				return XomNamespace.Xml;
			XomElement el = current as XomElement;
			do {
				XomNamespace n = el.GetLocalNamespace (prefix);
				if (n != null)
					return n;
				el = el.Parent as XomElement;
			} while (el != null);
			return null;
		}

		public override bool MoveToNamespace (string prefix)
		{
			if (currentNS != null)
				return false;
			XomNamespace n = GetXomNamespace (prefix);
			if (n == null)
				return false;
			currentNS = n;
			return true;
		}

		public override bool MoveToFirstNamespace (XPathNamespaceScope scope)
		{
			if (currentNS != null || current.NodeType != XPathNodeType.Element)
				return false;
			XomElement el = current as XomElement;
			XomNamespace n = null;
			switch (scope) {
			case XPathNamespaceScope.Local:
				if (el.NamespaceCount > 0)
					n = el.GetLocalNamespace (0);
				break;
			default:
				do {
					if (el.NamespaceCount > 0) {
						n = el.GetLocalNamespace (0);
						break;
					}
					el = el.Parent as XomElement;
				} while (el != null);
				break;
			}
			if (n != null) {
				currentNS = n;
				return true;
			}
			else if (scope == XPathNamespaceScope.All) {
				currentNS = XomNamespace.Xml;
				return true;
			}
			return false;
		}

		public override bool MoveToNextNamespace (XPathNamespaceScope scope)
		{
			if (currentNS == null || currentNS == XomNamespace.Xml)
				return false;
			XomElement el = currentNS.Parent as XomElement;
			XomNamespace n = el.GetNextLocalNamespace (currentNS);
			if (n != null) {
				currentNS = n;
				return true;
			}
			switch (scope) {
			case XPathNamespaceScope.Local:
				return false;
			default:
				el = el.Parent as XomElement;
				while (el != null) {
					if (el.NamespaceCount > 0) {
						n = el.GetLocalNamespace (0);
						break;
					}
					el = el.Parent as XomElement;
				}
				break;
			}
			if (n != null) {
				currentNS = n;
				return true;
			}
			else if (scope == XPathNamespaceScope.All) {
				currentNS = XomNamespace.Xml;
				return true;
			}
			return false;
		}

		public override bool MoveToNext ()
		{
			if (currentNS != null)
				return false;
			switch (NodeType) {
			case XPathNodeType.Text:
			case XPathNodeType.SignificantWhitespace:
			case XPathNodeType.Whitespace:
				for (XomNode t = current.NextSibling; t != null; t = t.NextSibling) {
					switch (t.NodeType) {
					case XPathNodeType.Text:
					case XPathNodeType.SignificantWhitespace:
					case XPathNodeType.Whitespace:
						continue;
					}
					current = t;
					return true;
				}
				return false;
			default:
				XomNode n = current.NextSibling;
				if (n == null)
					return false;
				current = n;
				return true;
			}
		}

		public override bool MoveToPrevious ()
		{
			if (currentNS != null)
				return false;
			XomNode n = current.PreviousSibling;
			if (n == null)
				return false;
			current = n;

			switch (NodeType) {
			case XPathNodeType.Text:
			case XPathNodeType.SignificantWhitespace:
			case XPathNodeType.Whitespace:
				for (XomNode t = current.PreviousSibling; t != null; t = t.NextSibling) {
					switch (t.NodeType) {
					case XPathNodeType.Text:
					case XPathNodeType.SignificantWhitespace:
					case XPathNodeType.Whitespace:
						current = t;
						continue;
					}
					break;
				}
				break;
			}

			return true;
		}

		public override bool IsSamePosition (XPathNavigator other)
		{
			XomNavigator xom = other as XomNavigator;
			if (xom == null || root != xom.root)
				return false;
			return current == xom.current && currentNS == xom.currentNS;
		}

		public override bool MoveTo (XPathNavigator other)
		{
			XomNavigator xom = other as XomNavigator;
			if (xom == null)
				return false;
			current = xom.current;
			currentNS = xom.currentNS;
			return true;
		}

		public override bool MoveToId (string id)
		{
			XomElement el = root.GetIdenticalNode (id);
			if (el == null)
				return false;
			current = el;
			return true;
		}
	}
}
#endif
