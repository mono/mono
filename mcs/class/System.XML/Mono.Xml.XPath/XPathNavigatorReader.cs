//
// XPathNavigatorReader.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// 2003 Atsushi Enomoto. No rights reserved.
// Copyright (C) 2004 Novell Inc.

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
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace Mono.Xml.XPath
{

	internal class XPathNavigatorReader : XmlReader
	{
		public XPathNavigatorReader (XPathNavigator nav)
		{
			// It seems that this class have only to support linked
			// node as its parameter
			switch (nav.NodeType) {
			case XPathNodeType.Attribute:
			case XPathNodeType.Namespace:
				throw new InvalidOperationException (String.Format ("NodeType {0} is not supported to read as a subtree of an XPathNavigator.", nav.NodeType));
			}
			root = nav.Clone ();
			current = nav.Clone ();
		}

		XPathNavigator root;
		XPathNavigator current;
		bool started;
		bool closed;
		bool endElement;
		bool attributeValueConsumed;
		StringBuilder readStringBuffer = new StringBuilder ();
		StringBuilder innerXmlBuilder = new StringBuilder ();

		int depth = 0;
		int attributeCount = 0;
		bool eof;
		bool nextIsEOF;

		#region Properties
		public override XmlNodeType NodeType 
		{
			get {
				if (ReadState != ReadState.Interactive)
					return XmlNodeType.None;
				if (endElement)
					return XmlNodeType.EndElement;
				if (attributeValueConsumed)
					// Is there any way to get other kind of nodes than Text?
					return XmlNodeType.Text;

				switch (current.NodeType) {
				case XPathNodeType.Namespace:
				case XPathNodeType.Attribute:
					return XmlNodeType.Attribute;
				case XPathNodeType.Comment:
					return XmlNodeType.Comment;
				case XPathNodeType.Element:
					return XmlNodeType.Element;
				case XPathNodeType.ProcessingInstruction:
					return XmlNodeType.ProcessingInstruction;
				case XPathNodeType.Root:
					// It is actually Document, but in XmlReader there is no such situation to return Document.
					return XmlNodeType.None;
				case XPathNodeType.SignificantWhitespace:
					return XmlNodeType.SignificantWhitespace;
				case XPathNodeType.Text:
					return XmlNodeType.Text;
				case XPathNodeType.Whitespace:
					return XmlNodeType.Whitespace;
				default:
					throw new InvalidOperationException (String.Format ("Current XPathNavigator status is {0} which is not acceptable to XmlReader.", current.NodeType));
				}
			}
		}

		public override string Name {
			get {
				if (eof)
					return String.Empty;
				else if (current.NodeType == XPathNodeType.Namespace)
					return current.Name == String.Empty ? "xmlns" : "xmlns:" + current.Name;
				else
					return current.Name;
			}
		}

		public override string LocalName {
			get {
				if (eof)
					return String.Empty;
				else if (current.NodeType == XPathNodeType.Namespace && current.LocalName == String.Empty)
					return "xmlns";
				else
					return current.LocalName;
			}
		}

		public override string NamespaceURI {
			get {
				if (eof)
					return String.Empty;
				else if (current.NodeType == XPathNodeType.Namespace)
					return XmlNamespaceManager.XmlnsXmlns;
				else
					return current.NamespaceURI;
			}
		}

		public override string Prefix {
			get {
				if (eof)
					return String.Empty;
				else if (current.NodeType == XPathNodeType.Namespace && current.LocalName != String.Empty)
					return "xmlns";
				else
					return current.Prefix;
			}
		}

		public override bool HasValue {
			get {
				switch (current.NodeType) {
				case XPathNodeType.Namespace:
				case XPathNodeType.Attribute:
				case XPathNodeType.Comment:
				case XPathNodeType.ProcessingInstruction:
				case XPathNodeType.SignificantWhitespace:
				case XPathNodeType.Text:
				case XPathNodeType.Whitespace:
					return true;
				}
				return false;
			}
		}

		public override int Depth {
			get {
				switch (ReadState) {
				case ReadState.EndOfFile:
				case ReadState.Initial:
				case ReadState.Closed:
					return 0;
				}
				return depth;
			}
		}

		public override string Value {
			get {
				switch (current.NodeType) {
				case XPathNodeType.Namespace:
				case XPathNodeType.Attribute:
				case XPathNodeType.Comment:
				case XPathNodeType.ProcessingInstruction:
				case XPathNodeType.SignificantWhitespace:
				case XPathNodeType.Text:
				case XPathNodeType.Whitespace:
					return current.Value;
				case XPathNodeType.Element:
				case XPathNodeType.Root:
					return String.Empty;
				default:
					throw new InvalidOperationException ("Current XPathNavigator status is {0} which is not acceptable to XmlReader.");
				}
			}
		}

		public override string BaseURI {
			get { return current.BaseURI; }
		}

		public override bool IsEmptyElement {
			get { return current.IsEmptyElement; }
		}

		// It makes no sense.
		public override bool IsDefault {
			get { return false; }
		}

		// It makes no sense.
		public override char QuoteChar {
			get { return '\"'; }
		}

		public override string XmlLang {
			get { return current.XmlLang; }
		}

		// It is meaningless.
		public override XmlSpace XmlSpace {
			get { return XmlSpace.None; }
		}

		public override int AttributeCount {
			get { return attributeCount; }
		}

		private int GetAttributeCount ()
		{
			int count = 0;
			if (current.MoveToFirstAttribute ()) {
				do {
					count++;
				} while (current.MoveToNextAttribute ());
				current.MoveToParent ();
			}
			if (current.MoveToFirstNamespace (XPathNamespaceScope.Local)) {
				do {
					count++;
				} while (current.MoveToNextNamespace (XPathNamespaceScope.Local));
				current.MoveToParent ();
			}
			return count;
		}
		
		private bool MoveToAttributeNavigator (int i)
		{
			switch (current.NodeType) {
			case XPathNodeType.Namespace:
			case XPathNodeType.Attribute:
				this.MoveToElement ();
				goto case XPathNodeType.Element;
			case XPathNodeType.Element:
				int count = 0;
				if (MoveToFirstAttribute ()) {
					if (i == 0)
						return true;
				}
				for (count++; this.MoveToNextAttribute (); count++) {
					if (count == i)
						return true;
				}
				break;
			}
			return false;
		}

		public override string this [int i] {
			get {
				XPathNavigator backup = current.Clone ();
				try {
					if (MoveToAttributeNavigator (i))
						return Value;
					else
						throw new ArgumentOutOfRangeException ();
				} finally {
					current.MoveTo (backup);
				}
			}
		}

		private void SplitName (string name, out string localName, out string ns)
		{
			localName = name;
			ns = String.Empty;
			int colon = name.IndexOf (':');
			if (colon > 0) {
				localName = name.Substring (colon + 1, name.Length - colon - 1);
				ns = this.LookupNamespace (name.Substring (0, colon));
			}
		}

		public override string this [string name] {
			get {
				string localName;
				string ns;
				SplitName (name, out localName, out ns);
				return this [localName, ns];
			}
		}

		public override string this [string localName, string namespaceURI] {
			get {
				string v = current.GetAttribute (localName, namespaceURI);
				if (v != String.Empty)
					return v;
				XPathNavigator tmp = current.Clone ();
				return tmp.MoveToAttribute (localName, namespaceURI) ? String.Empty : null;
			}
		}

		public override bool EOF {
			get { return ReadState == ReadState.EndOfFile; }
		}

		public override ReadState ReadState {
			get {
				if (eof)
					return ReadState.EndOfFile;
				if (closed)
					return ReadState.Closed;
				else if (!started)
					return ReadState.Initial;
				return ReadState.Interactive;
			}
		}

		public override XmlNameTable NameTable {
			get { return current.NameTable; }
		}
		#endregion

		#region Methods

		public override string GetAttribute (string name)
		{
			string localName;
			string ns;
			SplitName (name, out localName, out ns);
			return this [localName, ns];
		}

		public override string GetAttribute (string localName, string namespaceURI)
		{
			return this [localName, namespaceURI];
		}

		public override string GetAttribute (int i)
		{
			return this [i];
		}

		private bool CheckAttributeMove (bool b)
		{
			if (b)
				attributeValueConsumed = false;
			return b;
		}

		public override bool MoveToAttribute (string name)
		{
			string localName;
			string ns;
			SplitName (name, out localName, out ns);
			return CheckAttributeMove (MoveToAttribute (localName, ns));
		}

		public override bool MoveToAttribute (string localName, string namespaceURI)
		{
			XPathNavigator backup = null;
			switch (current.NodeType) {
			case XPathNodeType.Attribute:
				backup = current.Clone ();
				this.MoveToElement ();
				goto case XPathNodeType.Element;
			case XPathNodeType.Element:
				while (MoveToNextAttribute ())
					if (current.LocalName == localName && current.NamespaceURI == namespaceURI) {
						attributeValueConsumed = false;
						return true;
					}
				break;
			}
			if (backup != null)
				current = backup;
			return false;
		}

		public override void MoveToAttribute (int i)
		{
			if (!MoveToAttributeNavigator (i))
				throw new ArgumentOutOfRangeException ();
		}

		public override bool MoveToFirstAttribute ()
		{
			bool b = CheckAttributeMove (current.MoveToFirstNamespace (XPathNamespaceScope.Local));
			if (b)
				return true;
			return CheckAttributeMove (current.MoveToFirstAttribute ());
		}

		public override bool MoveToNextAttribute ()
		{
			if (current.NodeType == XPathNodeType.Namespace) {
				bool b = CheckAttributeMove (current.MoveToNextNamespace (XPathNamespaceScope.Local));
				if (b)
					return true;
				current.MoveToParent ();
				b = CheckAttributeMove (current.MoveToFirstAttribute ());
				if (b)
					return true;
			}
			return CheckAttributeMove (current.MoveToNextAttribute ());
		}

		public override bool MoveToElement ()
		{
			if (current.NodeType == XPathNodeType.Attribute ||
				current.NodeType == XPathNodeType.Namespace) {
				attributeValueConsumed = false;
				return current.MoveToParent ();
			}
			return false;
		}

		public override void Close ()
		{
			closed = true;
			eof = true;
		}

		public override bool Read ()
		{
			switch (ReadState) {
			case ReadState.EndOfFile:
			case ReadState.Closed:
			case ReadState.Error:
				return false;
			case ReadState.Initial:
				started = true;
				switch (current.NodeType) {
				case XPathNodeType.Root:
					// recurse, but as Interactive
					return Read ();
				case XPathNodeType.Element:
					if (current.IsEmptyElement)
						nextIsEOF = true;
					attributeCount = GetAttributeCount ();
					return true;
				default:
					nextIsEOF = true;
					return true;
				}
				break;
			}

			if (nextIsEOF) {
				nextIsEOF = false;
				eof = true;
				return false;
			}

			MoveToElement ();

			if (endElement || !current.MoveToFirstChild ()) {
				if (current.IsSamePosition (root)) {	// It should happen only when the root node was empty.
					eof = true;
					return false;
				}
				if (!current.MoveToNext ()) {
					current.MoveToParent ();
					depth--;
					endElement = (current.NodeType == XPathNodeType.Element);
					if (current.IsSamePosition (root)) {
						if (current.NodeType == XPathNodeType.Element)
							nextIsEOF = true;
						else {
							endElement = false;
							eof = true;
							return false;
						}
					}
				} else
					endElement = false;
			}
			else if (!endElement)
				depth++;
			attributeCount = GetAttributeCount ();
			return true;
		}

		public override string ReadString ()
		{
			readStringBuffer.Length = 0;

			switch (NodeType) {
			default:
				return String.Empty;
			case XmlNodeType.Element:
				if (IsEmptyElement)
					return String.Empty;
				do {
					Read ();
					switch (NodeType) {
					case XmlNodeType.Text:
					case XmlNodeType.CDATA:
					case XmlNodeType.Whitespace:
					case XmlNodeType.SignificantWhitespace:
						readStringBuffer.Append (Value);
						continue;
					}
					break;
				} while (true);
				break;
			case XmlNodeType.Text:
			case XmlNodeType.CDATA:
			case XmlNodeType.Whitespace:
			case XmlNodeType.SignificantWhitespace:
				do {
					switch (NodeType) {
					case XmlNodeType.Text:
					case XmlNodeType.CDATA:
					case XmlNodeType.Whitespace:
					case XmlNodeType.SignificantWhitespace:
						readStringBuffer.Append (Value);
						Read ();
						continue;
					}
					break;
				} while (true);
				break;
			}
			string ret = readStringBuffer.ToString ();
			readStringBuffer.Length = 0;
			return ret;
		}

#if NET_1_1
#else
		public override string ReadInnerXml ()
		{
			if (ReadState != ReadState.Interactive)
				return String.Empty;

			switch (NodeType) {
			case XmlNodeType.Attribute:
				return Value;
			case XmlNodeType.Element:
				if (IsEmptyElement)
					return String.Empty;

				int startDepth = Depth;

				innerXmlBuilder.Length = 0;
				bool loop = true;
				do {
					Read ();
					if (NodeType ==XmlNodeType.None)
						throw new InvalidOperationException ("unexpected end of xml.");
					else if (NodeType == XmlNodeType.EndElement && Depth == startDepth) {
						loop = false;
						Read ();
					}
					else
						innerXmlBuilder.Append (GetCurrentTagMarkup ());
				} while (loop);
				string xml = innerXmlBuilder.ToString ();
				innerXmlBuilder.Length = 0;
				return xml;
			case XmlNodeType.None:
				// MS document is incorrect. Seems not to progress.
				return String.Empty;
			default:
				Read ();
				return String.Empty;
			}
		}
		
		StringBuilder atts = new StringBuilder ();
		private string GetCurrentTagMarkup ()
		{
			switch (NodeType) {
			case XmlNodeType.CDATA:
				return String.Format ("<![CDATA[{0}]]>", Value.Replace ("]]>", "]]&gt;"));
			case XmlNodeType.Text:
				return Value.Replace ("<", "&lt;");
			case XmlNodeType.Comment:
				return String.Format ("<!--{0}-->", Value);
			case XmlNodeType.SignificantWhitespace:
			case XmlNodeType.Whitespace:
				return Value;
			case XmlNodeType.EndElement:
				return String.Format ("</{0}>", Name);
			}

			bool isEmpty = IsEmptyElement;
			string name = Name;
			atts.Length = 0;
			XPathNavigator temp = current.Clone ();
			while (temp.MoveToNextAttribute ())
				atts.AppendFormat (" {0}='{1}'", temp.Name, temp.Value.Replace ("'", "&apos;"));
			if (!IsEmptyElement)
				return String.Format ("<{0}{1}>", name, atts);
			else
				return String.Format ("<{0}{1} />", name, atts);
		}

		// Arranged copy of XmlTextReader.ReadOuterXml()
		public override string ReadOuterXml ()
		{
			if (ReadState != ReadState.Interactive)
				return String.Empty;

			switch (NodeType) {
			case XmlNodeType.Attribute:
				// strictly incompatible with MS... (it holds spaces attribute between name, value and "=" char (very trivial).
				return String.Format ("{0}={1}{2}{1}", Name, QuoteChar, ReadInnerXml ());
			case XmlNodeType.Element:
				bool isEmpty = IsEmptyElement;
				string name = Name;
				StringBuilder atts = new StringBuilder ();
				XPathNavigator temp = current.Clone ();
				while (temp.MoveToNextAttribute ())
					atts.AppendFormat (" {0}='{1}'", temp.Name, temp.Value.Replace ("'", "&apos;"));

				if (!isEmpty)
					return String.Format ("{0}{1}</{2}>", GetCurrentTagMarkup (), atts, ReadInnerXml (), name);
				else
					return String.Format ("{0}", GetCurrentTagMarkup ());
			case XmlNodeType.None:
				// MS document is incorrect. Seems not to progress.
				return String.Empty;
			default:
				Read ();
				return String.Empty;
			}
		}
#endif

		public override string LookupNamespace (string prefix)
		{
			XPathNavigator backup = current.Clone ();
			try {
				this.MoveToElement ();
				if (current.MoveToFirstNamespace ()) {
					do {
						if (current.LocalName == prefix)
							return current.Value;
					} while (current.MoveToNextNamespace ());
				}
				return null;
			} finally {
				current = backup;
			}
		}

		// It does not support entity resolution.
		public override void ResolveEntity ()
		{
			throw new InvalidOperationException ();
		}

		public override bool ReadAttributeValue () {
			if (NodeType != XmlNodeType.Attribute)
				return false;
			if (attributeValueConsumed)
				return false;
			attributeValueConsumed = true;
			return true;
		}
		#endregion
	}
}
