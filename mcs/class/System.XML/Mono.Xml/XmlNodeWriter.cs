//
// Mono.Xml.XmlNodeWriter
//
// Author:
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
//	(C)2003 Atsushi Enomoto
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
using System;
using System.Xml;

namespace System.Xml
{
	internal class XmlNodeWriter : XmlWriter
	{
		public XmlNodeWriter () : this (true)
		{
		}

		// It should be public after some tests are done :-)
		public XmlNodeWriter (bool isDocumentEntity)
		{
			doc = new XmlDocument ();
			state = XmlNodeType.None;
			this.isDocumentEntity = isDocumentEntity;
			if (!isDocumentEntity)
				current = fragment = doc.CreateDocumentFragment ();
		}

		XmlDocument doc;
		bool isClosed;
		// If it is not null, then we are now inside the element.
		XmlNode current;
		// If it is not null, then we are now inside the attribute.
		XmlAttribute attribute;
		// If it is false, then allow to contain multiple document elements.
		bool isDocumentEntity;
		XmlDocumentFragment fragment;

		// None: started or closed.
		// XmlDeclaration: after xmldecl. Never allow xmldecl.
		// DocumentType: after doctype. Never allow xmldecl and doctype.
		// Element: inside document element.
		// 
		XmlNodeType state;

		// Properties
		public XmlNode Document {
			get { return isDocumentEntity ? (XmlNode)doc : (XmlNode)fragment; }
		}

		public override WriteState WriteState {
			get {
				if (isClosed)
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

		public override string XmlLang {
			get {
				for (XmlElement n = current as XmlElement; n != null; n = n.ParentNode as XmlElement)
					if (n.HasAttribute ("xml:lang"))
						return n.GetAttribute ("xml:lang");
				return String.Empty;
			}
		}

		public override XmlSpace XmlSpace {
			get {
				for (XmlElement n = current as XmlElement; n != null; n = n.ParentNode as XmlElement) {
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

		// Private Methods

		private void CheckState ()
		{
			if (isClosed)
				throw new InvalidOperationException ();

		}

		private void WritePossiblyTopLevelNode (XmlNode n, bool possiblyAttribute)
		{
			CheckState ();
			if (!possiblyAttribute && attribute != null)
				throw new InvalidOperationException (String.Format ("Current state is not acceptable for {0}.", n.NodeType));

			if (state != XmlNodeType.Element)
				Document.AppendChild (n);
			else if (attribute != null)
				attribute.AppendChild (n);
			else
				current.AppendChild (n);
			if (state == XmlNodeType.None)
				state = XmlNodeType.XmlDeclaration;
		}

		// Public Methods

		public override void Close ()
		{
			CheckState ();
			isClosed = true;
		}

		public override void Flush ()
		{
		}

		public override string LookupPrefix (string ns)
		{
			CheckState ();
			if (current == null)
				throw new InvalidOperationException ();
			return current.GetPrefixOfNamespace (ns);
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

		private void WriteStartDocument (string sddecl)
		{
			CheckState ();
			if (state != XmlNodeType.None)
				throw new InvalidOperationException ("Current state is not acceptable for xmldecl.");

			doc.AppendChild (doc.CreateXmlDeclaration ("1.0", null, sddecl));
			state = XmlNodeType.XmlDeclaration;
		}
		
		// EndDocument
		
		public override void WriteEndDocument ()
		{
			CheckState ();

			isClosed = true;
		}

		// DocumentType
		public override void WriteDocType (string name, string publicId, string systemId, string internalSubset)
		{
			CheckState ();
			switch (state) {
			case XmlNodeType.None:
			case XmlNodeType.XmlDeclaration:
				doc.AppendChild (doc.CreateDocumentType (name, publicId, systemId, internalSubset));
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
			if (isDocumentEntity && state == XmlNodeType.EndElement && doc.DocumentElement != null)
				throw new InvalidOperationException ("Current state is not acceptable for startElement.");

			XmlElement el = doc.CreateElement (prefix, name, ns);
			if (current == null) {
				Document.AppendChild (el);
				state = XmlNodeType.Element;
			} else {
				current.AppendChild (el);
				state = XmlNodeType.Element;
			}

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

		private void WriteEndElementInternal (bool forceFull)
		{
			CheckState ();
			if (current == null)
				throw new InvalidOperationException ("Current state is not acceptable for endElement.");

			if (!forceFull && current.FirstChild == null)
				((XmlElement) current).IsEmpty = true;

			if (isDocumentEntity && current.ParentNode == doc)
				state = XmlNodeType.EndElement;
			else
				current = current.ParentNode;
		}

		// StartAttribute

		public override void WriteStartAttribute (string prefix, string name, string ns)
		{
			CheckState ();
			if (attribute != null)
				throw new InvalidOperationException ("There is an open attribute.");
			if (!(current is XmlElement))
				throw new InvalidOperationException ("Current state is not acceptable for startAttribute.");

			attribute = doc.CreateAttribute (prefix, name, ns);
			((XmlElement)current).SetAttributeNode (attribute);
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

			current.AppendChild (doc.CreateCDataSection (data));
		}

		public override void WriteComment (string comment)
		{
			WritePossiblyTopLevelNode (doc.CreateComment (comment), false);
		}

		public override void WriteProcessingInstruction (string name, string value)
		{
			WritePossiblyTopLevelNode (
				doc.CreateProcessingInstruction (name, value), false);
		}

		public override void WriteEntityRef (string name)
		{
			WritePossiblyTopLevelNode (doc.CreateEntityReference (name), true);
		}

		public override void WriteCharEntity (char c)
		{
			WritePossiblyTopLevelNode (doc.CreateTextNode (new string (new char [] {c}, 0, 1)), true);
		}

		public override void WriteWhitespace (string ws)
		{
			WritePossiblyTopLevelNode (doc.CreateWhitespace (ws), true);
		}

		public override void WriteString (string data)
		{
			CheckState ();
			if (current == null)
				throw new InvalidOperationException ("Current state is not acceptable for Text.");

			if (attribute != null)
				attribute.AppendChild (doc.CreateTextNode (data));
			else {
				XmlText last = current.LastChild as XmlText;
				if (last == null)
					current.AppendChild(doc.CreateTextNode(data));
				else 
					last.AppendData(data);
			}
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
