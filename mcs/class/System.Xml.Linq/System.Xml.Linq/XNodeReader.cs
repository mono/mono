//
// Authors:
//   Atsushi Enomoto
//
// Copyright 2007 Novell (http://www.novell.com)
// Copyright 2011 Xamarin Inc (http://www.xamarin.com).
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

using System;
using System.Xml;

using XPI = System.Xml.Linq.XProcessingInstruction;

namespace System.Xml.Linq
{
	internal class XNodeReader : XmlReader, IXmlLineInfo
	{
		ReadState state = ReadState.Initial;
		XNode node, start;
		int attr = -1;
		bool attr_value;
		bool end_element;
		NameTable name_table = new NameTable ();

		public XNodeReader (XNode node)
		{
			this.node = node;
			start = node;
		}

		int IXmlLineInfo.LineNumber {
			get {
				var o = (XObject) GetCurrentAttribute () ?? node;
				return o != null ? o.LineNumber : 0;
			}
		}
		int IXmlLineInfo.LinePosition {
			get {
				var o = (XObject) GetCurrentAttribute () ?? node;
				return o != null ? o.LinePosition : 0;
			}
		}
		bool IXmlLineInfo.HasLineInfo ()
		{
				var o = (XObject) GetCurrentAttribute () ?? node;
				return o != null ? ((IXmlLineInfo) o).HasLineInfo () : false;
		}
	
		public override int AttributeCount {
			get {
				if (state != ReadState.Interactive || end_element)
					return 0;
				int i = 0;
				switch (node.NodeType) {
				case XmlNodeType.Document: // this means xmldecl
					XDeclaration xd = ((XDocument) node).Declaration;
					i = (xd.Version != null ? 1 : 0) +
					    (xd.Encoding != null ? 1 : 0) +
					    (xd.Standalone != null ? 1 : 0);
					return i;
				case XmlNodeType.DocumentType:
					XDocumentType dtd = (XDocumentType) node;
					i = (dtd.PublicId != null ? 1 : 0) +
					    (dtd.SystemId != null ? 1 : 0) +
					    (dtd.InternalSubset != null ? 1 : 0);
					return i;
				case XmlNodeType.Element:
					XElement el = (XElement) node;
					for (XAttribute a = el.FirstAttribute; a != null; a = a.NextAttribute)
						i++;
					return i;
				}
				return 0;
			}
		}

		public override string BaseURI {
			get { return node.BaseUri ?? String.Empty; }
		}

		public override int Depth {
			get {
				if (EOF)
					return 0;
				int i = 0;
				// document.Depth = 0, root.Depth = 0, others.Depth = they depend
				for (XNode n = node.Parent; n != null; n = n.Parent)
					i++;
				if (attr >= 0)
					i++;
				if (attr_value)
					i++;
				return i;
			}
		}

		public override bool EOF {
			get { return state == ReadState.EndOfFile || state == ReadState.Error; }
		}

		public override bool HasAttributes {
			get {
				if (EOF || end_element || node == null)
					return false;

				if (node is XElement)
					return ((XElement) node).HasAttributes;
				return AttributeCount > 0;
			}
		}

		public override bool HasValue {
			get {
				if (EOF)
					return false;
				if (attr >= 0)
					return true;
				switch (node.NodeType) {
				case XmlNodeType.Element:
				case XmlNodeType.Document:
				case XmlNodeType.EndElement:
					return false;
				default:
					return true;
				}
			}
		}

		public override bool IsEmptyElement {
			get { return !EOF && attr < 0 && node is XElement ? ((XElement) node).IsEmpty : false; }
		}

		internal XAttribute GetCurrentAttribute ()
		{
			return GetXAttribute (attr);
		}

		XAttribute GetXAttribute (int idx)
		{
			if (EOF)
				return null;
			XElement el = node as XElement;
			if (el == null)
				return null;
			int i = 0;
			foreach (XAttribute a in el.Attributes ())
				if (i++ == idx)
					return a;
			return null;
		}

		// XName for element and attribute, string for xmldecl attributes, doctype attribute, doctype name and PI, null for empty.
		object GetCurrentName ()
		{
			if (EOF || attr_value)
				return null;
			return GetName (attr);
		}

		object GetName (int attr)
		{
			if (attr >= 0) {
				switch (node.NodeType) {
				case XmlNodeType.Element:
					XAttribute a = GetXAttribute (attr);
					return a.Name;
				case XmlNodeType.DocumentType:
					if (attr == 0)
						return ((XDocumentType) node).PublicId != null ? "PUBLIC" : "SYSTEM";
					return "SYSTEM";
				case XmlNodeType.Document:
					XDeclaration xd = ((XDocument) node).Declaration;
					switch (attr) {
					case 0:
						return xd.Version != null ? "version" : xd.Encoding != null ? "encoding" : "standalone";
					case 1:
						return xd.Version != null ? (xd.Encoding != null ? "encoding" : "standalone") : "standalone";
					}
					return "standalone";
				}
			} else {
				switch (node.NodeType) {
				case XmlNodeType.Document:
					return "xml"; // xmldecl
				case XmlNodeType.Element:
					return ((XElement) node).Name;
				case XmlNodeType.ProcessingInstruction:
					return ((XPI) node).Target;
				case XmlNodeType.DocumentType:
					return ((XDocumentType) node).Name;
				}
			}
			return null;
		}

		public override string LocalName {
			get {
				object name = GetCurrentName ();
				if (name == null)
					return String.Empty;
				if (name is string)
					return (string) name;
				return ((XName) name).LocalName;
			}
		}

		public override string NamespaceURI {
			get {
				XName name = GetCurrentName () as XName;
				if (name != null)
					// XName for "xmlns" has NamespaceName as "", so we have to return w3c xmlns as a special case.
					return name.LocalName == "xmlns" && name.Namespace == XNamespace.None ?
						XNamespace.Xmlns.NamespaceName :
						name.NamespaceName;
				return String.Empty;
			}
		}

		public override XmlNameTable NameTable {
			get { return name_table; }
		}

		public override XmlNodeType NodeType {
			get {
				return  state != ReadState.Interactive ? XmlNodeType.None :
					end_element ? XmlNodeType.EndElement :
					attr_value ? XmlNodeType.Text :
					attr >= 0 ? XmlNodeType.Attribute :
					node.NodeType == XmlNodeType.Document ? XmlNodeType.XmlDeclaration :
					node.NodeType;
			}
		}

		public override string Prefix {
			get {
				XName name = GetCurrentName () as XName;
				if (name == null || name.Namespace == XNamespace.None)
					return String.Empty;
				XElement el = (node as XElement) ?? node.Parent;
				if (el == null)
					return String.Empty;
				return el.GetPrefixOfNamespace (name.Namespace) ?? String.Empty;
			}
		}

		public override ReadState ReadState {
			get { return state; }
		}

		public override string Value {
			get {
				if (ReadState != ReadState.Interactive)
					return String.Empty;
				XAttribute a = GetCurrentAttribute ();
				if (a != null)
					return a.Value;
				switch (node.NodeType) {
				case XmlNodeType.Document:
					XDeclaration xd = ((XDocument) node).Declaration;
					if (attr >= 0) {
						switch (LocalName) {
						case "version":
							return xd.Version;
						case "encoding":
							return xd.Encoding;
						default:
							return xd.Standalone;
						}
					} else {
						string s = xd.ToString ();
						return s.Substring (6, s.Length - 6 - 2);
					}
				case XmlNodeType.DocumentType:
					XDocumentType dtd = (XDocumentType) node;
					switch (LocalName) {
					case "PUBLIC":
						return dtd.PublicId;
					case "SYSTEM":
						return dtd.SystemId;
					default:
						return dtd.InternalSubset;
					}
				case XmlNodeType.ProcessingInstruction:
					return ((XPI) node).Data;
				case XmlNodeType.CDATA:
				case XmlNodeType.Text:
					return ((XText) node).Value;
				case XmlNodeType.Comment:
					return ((XComment) node).Value;
				}
				return String.Empty;
			}
		}

		public override void Close ()
		{
			state = ReadState.Closed;
		}

		public override string LookupNamespace (string prefix)
		{
			if (EOF)
				return null;
			XElement el = (node as XElement) ?? node.Parent;
			if (el == null)
				return null;
			var xn = el.GetNamespaceOfPrefix (prefix);
			return xn != XNamespace.None ? xn.NamespaceName : null;
		}

		public override bool MoveToElement ()
		{
			if (attr >= 0) {
				attr_value = false;
				attr = -1;
				return true;
			}
			return false;
		}

		public override bool MoveToFirstAttribute ()
		{
			if (AttributeCount > 0) {
				attr = 0;
				attr_value = false;
				return true;
			}
			return false;
		}

		public override bool MoveToNextAttribute ()
		{
			int c = AttributeCount;
			if (attr + 1 < c) {
				attr++;
				attr_value = false;
				return true;
			}
			return false;
		}

		public override bool MoveToAttribute (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			int c = AttributeCount;
			bool match = false;
			for (int i = 0; i < c; i++) {
				object o = GetName (i);
				if (o == null)
					continue;
				if ((o as string) == name)
					match = true;
				XName n = (XName) o;
				if (name.EndsWith (n.LocalName, StringComparison.Ordinal) && name == GetPrefixedName ((XName) o))
					match = true;
				if (match) {
					attr = i;
					attr_value = false;
					return true;
				}
			}
			return false;
		}

		string GetPrefixedName (XName name)
		{
			XElement el = (node as XElement) ?? node.Parent;
			if (el == null ||
			    name.Namespace == XNamespace.None ||
			    el.GetPrefixOfNamespace (name.Namespace) == String.Empty)
				return name.LocalName;
			return String.Concat (el.GetPrefixOfNamespace (name.Namespace), ":", name.LocalName);
		}

		public override bool MoveToAttribute (string local, string ns)
		{
			if (local == null)
				throw new ArgumentNullException ("local");
			if (ns == null)
				throw new ArgumentNullException ("ns");

			int c = AttributeCount;
			bool match = false;
			for (int i = 0; i < c; i++) {
				object o = GetName (i);
				if (o == null)
					continue;
				if ((o as string) == local && ns.Length == 0)
					match = true;
				XName n = (XName) o;
				if (local == n.LocalName && ns == n.NamespaceName)
					match = true;
				if (match) {
					attr = i;
					attr_value = false;
					return true;
				}
			}
			return false;
		}

		public override string GetAttribute (int i)
		{
			int a_bak = attr;
			bool av_bak = attr_value;
			try {
				MoveToElement ();
				MoveToAttribute (i);
				return Value;
			} finally {
				attr = a_bak;
				attr_value = av_bak;
			}
		}

		public override string GetAttribute (string name)
		{
			int a_bak = attr;
			bool av_bak = attr_value;
			try {
				MoveToElement ();
				return MoveToAttribute (name) ? Value : null;
			} finally {
				attr = a_bak;
				attr_value = av_bak;
			}
		}

		public override string GetAttribute (string local, string ns)
		{
			int a_bak = attr;
			bool av_bak = attr_value;
			try {
				MoveToElement ();
				return MoveToAttribute (local, ns) ? Value : null;
			} finally {
				attr = a_bak;
				attr_value = av_bak;
			}
		}

		public override bool Read ()
		{
			// clear attribute state on element/xmldecl/dtd.
			attr = -1;
			attr_value = false;

			switch (state) {
			case ReadState.Initial:
				state = ReadState.Interactive;
				XDocument doc = node as XDocument;
				if (doc != null) {
					if (doc.Declaration != null)
						return true;
				}
				else
					return true; // any other root
				break;
			case ReadState.Interactive:
				break;
			default:
				return false;
			}

			// when positioned on xmldecl, move to children
			if (node is XDocument) {
				XDocument doc = node as XDocument;
				node = doc.FirstNode;
				if (node == null) {
					state = ReadState.EndOfFile;
					return false;
				}
				node = doc.FirstNode;
				return true;
			}

			XElement c = node as XElement;
			if (c != null && !end_element) {
				if (c.FirstNode != null) {
					node = c.FirstNode;
					return true;
				} else if (!c.IsEmpty) {
					// empty but full EndElement
					end_element = true;
					return true;
				}
			}
			end_element = false;
			if (node.NextNode != null && node != start) {
				node = node.NextNode;
				return true;
			}
			if (node.Parent == null || node == start) {
				state = ReadState.EndOfFile;
				return false;
			}
			node = node.Parent;
			end_element = true;
			return true;
		}

		public
		override
		bool ReadAttributeValue ()
		{
			if (attr < 0 || attr_value)
				return false;
			attr_value = true;
			return true;
		}

		public override void ResolveEntity ()
		{
			throw new NotSupportedException ();
		}
		
		// Note that this does not return attribute node.
		internal XNode CurrentNode {
			get { return node; }
		}
	}
}
