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

using System;
using System.Xml;

namespace System.Xml.Linq
{
	internal class XNodeWriter : XmlWriter
	{
		public XNodeWriter (XContainer fragment)
		{
			root = fragment;
			state = XmlNodeType.None;
			current = fragment;
		}

		XContainer root;
		bool is_closed;
		// If it is not null, then we are now inside the element.
		XContainer current;
		// If it is not null, then we are now inside the attribute.
		XAttribute attribute;

		// None: started or closed.
		// XmlDeclaration: after xmldecl. Never allow xmldecl.
		// DocumentType: after doctype. Never allow xmldecl and doctype.
		// Element: inside document element.
		// 
		XmlNodeType state;

		// Properties

		public override WriteState WriteState {
			get {
				if (is_closed)
					return WriteState.Closed;
				if (attribute != null)
					return WriteState.Attribute;

				switch (state) {
				case XmlNodeType.None:
					return WriteState.Start;
				case XmlNodeType.XmlDeclaration:
					return WriteState.Prolog;
				case XmlNodeType.DocumentType:
					return WriteState.Element;
				default:
					return WriteState.Content;
				}
			}
		}

		/*
		public override string XmlLang {
			get {
				for (XElement n = current as XElement; n != null; n = n.Parent as XElement)
					if (n.HasAttribute ("xml:lang"))
						return n.GetAttribute ("xml:lang");
				return String.Empty;
			}
		}

		public override XmlSpace XmlSpace {
			get {
				for (XElement n = current as XElement; n != null; n = n.Parent as XElement) {
					string xs = n.GetAttribute ("xml:space");
					switch (xs) {
					case "preserve":
						return XmlSpace.Preserve;
					case "default":
						return XmlSpace.Default;
					case "":
						continue;
					default:
						throw new InvalidOperationException (String.Format ("Invalid xml:space {0}.", xs));
					}
				}
				return XmlSpace.None;
			}
		}
		*/

		// Private Methods

		void CheckState ()
		{
			if (is_closed)
				throw new InvalidOperationException ();

		}

		void WritePossiblyTopLevelNode (XNode n, bool possiblyAttribute)
		{
			CheckState ();
			if (!possiblyAttribute && attribute != null)
				throw new InvalidOperationException (String.Format ("Current state is not acceptable for {0}.", n.NodeType));

			if (state != XmlNodeType.Element)
				root.Add (n);
			else if (attribute != null)
				attribute.Value += XUtil.ToString (n);
			else
				current.Add (n);
			if (state == XmlNodeType.None)
				state = XmlNodeType.XmlDeclaration;
		}

		// unlike other XmlWriters the callers must set xmlns
		// attribute to overwrite prefix.
		void FillXmlns (XElement el, string prefix, XNamespace xns)
		{
			if (xns == XNamespace.Xmlns)
				// do nothing for xmlns attributes
				return;
			if (prefix == null)
				return;

			if (xns == XNamespace.None)
				if (el.GetPrefixOfNamespace (xns) != prefix)
					el.SetAttributeValue (prefix == String.Empty ? XNamespace.None.GetName ("xmlns") : XNamespace.Xmlns.GetName (prefix), xns.NamespaceName);
			else if (el.GetDefaultNamespace () != XNamespace.None)
				el.SetAttributeValue (XNamespace.None.GetName ("xmlns"), xns.NamespaceName);
		}

		// Public Methods

		public override void Close ()
		{
			CheckState ();
			is_closed = true;
		}

		public override void Flush ()
		{
		}

		public override string LookupPrefix (string ns)
		{
			CheckState ();
			if (current == null)
				throw new InvalidOperationException ();
			XElement el = (current as XElement) ?? current.Parent;
			return el != null ? el.GetPrefixOfNamespace (XNamespace.Get (ns)) : null;
		}

		// StartDocument

		public override void WriteStartDocument ()
		{
			WriteStartDocument (null);
		}

		public override void WriteStartDocument (bool standalone)
		{
			WriteStartDocument (standalone ? "yes" : "no");
		}

		void WriteStartDocument (string sddecl)
		{
			CheckState ();
			if (state != XmlNodeType.None)
				throw new InvalidOperationException ("Current state is not acceptable for xmldecl.");
			XDocument doc = current as XDocument;
			if (doc == null)
				throw new InvalidOperationException ("Only document node can accept xml declaration");

			doc.Declaration = new XDeclaration ("1.0", null, sddecl);
			state = XmlNodeType.XmlDeclaration;
		}
		
		// EndDocument
		
		public override void WriteEndDocument ()
		{
			CheckState ();

			is_closed = true;
		}

		// DocumentType
		public override void WriteDocType (string name, string publicId, string systemId, string internalSubset)
		{
			CheckState ();
			switch (state) {
			case XmlNodeType.None:
			case XmlNodeType.XmlDeclaration:
				XDocument doc = current as XDocument;
				if (doc == null)
					throw new InvalidOperationException ("Only document node can accept doctype declaration");
				doc.Add (new XDocumentType (name, publicId, systemId, internalSubset));
				state = XmlNodeType.DocumentType;
				break;
			default:
				throw new InvalidOperationException ("Current state is not acceptable for doctype.");
			}
		}

		// StartElement

		public override void WriteStartElement (string prefix, string name, string ns)
		{
			CheckState ();

			XNamespace xns = XNamespace.Get (ns ?? String.Empty);
			XElement el = new XElement (xns.GetName (name));
			if (current == null) {
				root.Add (el);
				state = XmlNodeType.Element;
			} else {
				current.Add (el);
				state = XmlNodeType.Element;
			}

			FillXmlns (el, prefix, xns);

			current = el;
		}

		// EndElement

		public override void WriteEndElement ()
		{
			WriteEndElementInternal (false);
		}
		
		public override void WriteFullEndElement ()
		{
			WriteEndElementInternal (true);
		}

		void WriteEndElementInternal (bool forceFull)
		{
			CheckState ();
			if (current == null)
				throw new InvalidOperationException ("Current state is not acceptable for endElement.");

			XElement el = current as XElement;
			if (forceFull)
				el.IsEmpty = false;

			current = current.Parent;
		}

		// StartAttribute

		public override void WriteStartAttribute (string prefix, string name, string ns)
		{
			CheckState ();
			if (attribute != null)
				throw new InvalidOperationException ("There is an open attribute.");
			XElement el = current as XElement;
			if (el == null)
				throw new InvalidOperationException ("Current state is not acceptable for startAttribute.");
			if (prefix == null)
				prefix = String.Empty;

			// special case: in XmlWriter context, xmlns="blah" is
			// passeed as localName = "xmlns", ns = w3c_xmlns.
			if (prefix.Length == 0 && name == "xmlns" && ns == XNamespace.Xmlns.NamespaceName)
				ns = String.Empty;

			XNamespace xns = ns == null ? XNamespace.None : XNamespace.Get (ns);
			el.SetAttributeValue (xns.GetName (name), String.Empty);
			attribute = el.LastAttribute;
			FillXmlns (el, ns != null ? prefix : null, xns);
		}

		public override void WriteEndAttribute ()
		{
			CheckState ();
			if (attribute == null)
				throw new InvalidOperationException ("Current state is not acceptable for startAttribute.");

			attribute = null;
		}

		public override void WriteCData (string data)
		{
			CheckState ();
			if (current == null)
				throw new InvalidOperationException ("Current state is not acceptable for CDATAsection.");

			current.Add (new XCData (data));
		}

		public override void WriteComment (string comment)
		{
			WritePossiblyTopLevelNode (new XComment (comment), false);
		}

		public override void WriteProcessingInstruction (string name, string value)
		{
			WritePossiblyTopLevelNode (
				new XProcessingInstruction (name, value), false);
		}

		public override void WriteEntityRef (string name)
		{
			throw new NotSupportedException ();
		}

		public override void WriteCharEntity (char c)
		{
			throw new NotSupportedException ();
		}

		public override void WriteWhitespace (string ws)
		{
			// FIXME: check whitespace
			WritePossiblyTopLevelNode (new XText (ws), true);
		}

		public override void WriteString (string data)
		{
			CheckState ();
			if (current == null)
				throw new InvalidOperationException ("Current state is not acceptable for Text.");

			if (attribute != null)
				attribute.Value += data;
			else
				current.Add (data);
		}

		public override void WriteName (string name)
		{
			WriteString (name);
		}

		public override void WriteNmToken (string nmtoken)
		{
			WriteString (nmtoken);
		}

		public override void WriteQualifiedName (string name, string ns)
		{
			string prefix = LookupPrefix (ns);
			if (prefix == null)
				throw new ArgumentException (String.Format ("Invalid namespace {0}", ns));
			if (prefix != String.Empty)
				WriteString (name);
			else
				WriteString (prefix + ":" + name);
		}

		public override void WriteChars (char [] chars, int start, int len)
		{
			WriteString (new string (chars, start, len));
		}

		public override void WriteRaw (string data)
		{
			// It never supports raw string.
			WriteString (data);
		}

		public override void WriteRaw (char [] chars, int start, int len)
		{
			// It never supports raw string.
			WriteChars (chars, start, len);
		}

		public override void WriteBase64 (byte [] data, int start, int len)
		{
			// It never supports raw string.
			WriteString (Convert.ToBase64String (data, start, len));
		}
		
		public override void WriteBinHex (byte [] data, int start, int len)
		{
			throw new NotImplementedException ();
		}

		public override void WriteSurrogateCharEntity (char c1, char c2)
		{
			throw new NotImplementedException ();
		}
	}
}
