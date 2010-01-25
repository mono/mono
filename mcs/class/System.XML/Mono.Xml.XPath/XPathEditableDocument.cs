//
// Mono.Xml.XPath.XPathEditableDocument
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
//
// Yet another implementation of editable XPathNavigator.
// (Even runnable under MS.NET 2.0)
//
// By rewriting XPathEditableDocument.CreateNavigator() as just to 
// create XmlDocumentNavigator, XmlDocumentEditableNavigator could be used 
// as to implement XmlDocument.CreateNavigator().
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
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Serialization;

namespace Mono.Xml.XPath
{
	internal class XPathEditableDocument : IXPathNavigable
	{
		XmlNode node;

		public XPathEditableDocument (XmlNode node)
		{
			this.node = node;
		}

		public XmlNode Node {
			get { return node; }
		}

		public XPathNavigator CreateNavigator ()
		{
			return new XmlDocumentEditableNavigator (this);
		}
	}

	internal delegate void XmlWriterClosedEventHandler (
		XmlWriter writer);

	internal class XmlDocumentInsertionWriter : XmlWriter
	{
		XmlNode parent;
		XmlNode current;
		XmlNode nextSibling;
		WriteState state;
		XmlAttribute attribute;

		public XmlDocumentInsertionWriter (XmlNode owner, XmlNode nextSibling)
		{
			this.parent = (XmlNode) owner;
			if (parent == null)
				throw new InvalidOperationException ();
			switch (parent.NodeType) {
			case XmlNodeType.Document:
				current = ((XmlDocument) parent).CreateDocumentFragment ();
				break;
			case XmlNodeType.DocumentFragment:
			case XmlNodeType.Element:
				current = parent.OwnerDocument.CreateDocumentFragment ();
				break;
			default:
				throw new InvalidOperationException (String.Format ("Insertion into {0} node is not allowed.", parent.NodeType));
			}
			this.nextSibling = nextSibling;
			state = WriteState.Content;
		}

		public override WriteState WriteState {
			get { return state; }
		}

		public override void Close ()
		{
			while (current.ParentNode != null)
				current = current.ParentNode;

			parent.InsertBefore ((XmlDocumentFragment) current, nextSibling);
			if (Closed != null)
				Closed (this);
		}

		internal event XmlWriterClosedEventHandler Closed;

		public override void Flush ()
		{
		}

		public override string LookupPrefix (string ns)
		{
			return current.GetPrefixOfNamespace (ns);
		}

		public override void WriteStartAttribute (string prefix, string name, string ns)
		{
			if (state != WriteState.Content)
				throw new InvalidOperationException ("Current state is not inside element. Cannot start attribute.");
			if (prefix == null && ns != null && ns.Length > 0)
				prefix = LookupPrefix (ns);
			attribute = current.OwnerDocument.CreateAttribute (prefix, name, ns);
			state = WriteState.Attribute;
		}

		public override void WriteProcessingInstruction (string name, string value)
		{
			XmlProcessingInstruction pi = current.OwnerDocument.CreateProcessingInstruction (name, value);
			current.AppendChild (pi);
		}

		public override void WriteComment (string text)
		{
			XmlComment comment = current.OwnerDocument.CreateComment (text);
			current.AppendChild (comment);
		}

		public override void WriteCData (string text)
		{
			XmlCDataSection cdata = current.OwnerDocument.CreateCDataSection (text);
			current.AppendChild (cdata);
		}

		public override void WriteStartElement (string prefix, string name, string ns)
		{
			if (prefix == null && ns != null && ns.Length > 0)
				prefix = LookupPrefix (ns);
			XmlElement el = current.OwnerDocument.CreateElement (prefix, name, ns);
			current.AppendChild (el);
			current = el;
		}

		public override void WriteEndElement ()
		{
			current = current.ParentNode;
			if (current == null)
				throw new InvalidOperationException ("No element is opened.");
		}

		public override void WriteFullEndElement ()
		{
			XmlElement el = current as XmlElement;
			if (el != null)
				el.IsEmpty = false;
			WriteEndElement ();
		}

		public override void WriteDocType (string name, string pubid, string systemId, string intsubset)
		{
			throw new NotSupportedException ();
		}

		public override void WriteStartDocument ()
		{
			throw new NotSupportedException ();
		}

		public override void WriteStartDocument (bool standalone)
		{
			throw new NotSupportedException ();
		}

		public override void WriteEndDocument ()
		{
			throw new NotSupportedException ();
		}

		public override void WriteBase64 (byte [] data, int start, int length)
		{
			WriteString (Convert.ToBase64String (data, start, length));
		}

		public override void WriteRaw (char [] raw, int start, int length)
		{
			throw new NotSupportedException ();
		}

		public override void WriteRaw (string raw)
		{
			XmlReader reader = new XmlTextReader(new System.IO.StringReader(raw));
			WriteRaw(reader);
		}

		private void WriteRaw(XmlReader reader)
		{
			if (reader != null && reader.NodeType == XmlNodeType.Element)
			{
				WriteStartElement (reader.Prefix, reader.LocalName, reader.NamespaceURI);
				WriteAttributes (reader, true);
				WriteRaw (reader.ReadSubtree ());
				WriteEndElement ();

			}
		}

		public override void WriteSurrogateCharEntity (char msb, char lsb)
		{
			throw new NotSupportedException ();
		}

		public override void WriteCharEntity (char c)
		{
			throw new NotSupportedException ();
		}

		public override void WriteEntityRef (string entname)
		{
			if (state != WriteState.Attribute)
				throw new InvalidOperationException ("Current state is not inside attribute. Cannot write attribute value.");
			attribute.AppendChild (attribute.OwnerDocument.CreateEntityReference (entname));
		}

		public override void WriteChars (char [] data, int start, int length)
		{
			WriteString (new string (data, start, length));
		}

		public override void WriteString (string text)
		{
			if (attribute != null)
				attribute.Value += text;
			else {
				XmlText t = current.OwnerDocument.CreateTextNode (text);
				current.AppendChild (t);
			}
		}

		public override void WriteWhitespace (string text)
		{
			if (state != WriteState.Attribute)
				current.AppendChild (current.OwnerDocument.CreateTextNode (text));
			else if (attribute.ChildNodes.Count == 0)
				attribute.AppendChild (attribute.OwnerDocument.CreateWhitespace (text));
			else
				attribute.Value += text;
		}

		public override void WriteEndAttribute ()
		{
			// when the writer is for AppendChild() and the root 
			// node is element, it allows to write attributes
			// (IMHO incorrectly: isn't it append "child" ???)
			XmlElement element = (current as XmlElement) ?? (nextSibling == null ? parent as XmlElement : null);
			if (state != WriteState.Attribute || element == null)
				throw new InvalidOperationException ("Current state is not inside attribute. Cannot close attribute.");
			element.SetAttributeNode (attribute);
			attribute = null;
			state = WriteState.Content;
		}
	}

	internal class XmlDocumentAttributeWriter : XmlWriter
	{
		XmlElement element;
		WriteState state;
		XmlAttribute attribute;

		public XmlDocumentAttributeWriter (XmlNode owner)
		{
			element = owner as XmlElement;
			if (element == null)
				throw new ArgumentException ("To write attributes, current node must be an element.");
			state = WriteState.Content;
		}

		public override WriteState WriteState {
			get { return state; }
		}

		public override void Close ()
		{
		}

		public override void Flush ()
		{
		}

		public override string LookupPrefix (string ns)
		{
			return element.GetPrefixOfNamespace (ns);
		}

		public override void WriteStartAttribute (string prefix, string name, string ns)
		{
			if (state != WriteState.Content)
				throw new InvalidOperationException ("Current state is not inside element. Cannot start attribute.");
			if (prefix == null && ns != null && ns.Length > 0)
				prefix = LookupPrefix (ns);
			attribute = element.OwnerDocument.CreateAttribute (prefix, name, ns);
			state = WriteState.Attribute;
		}

		public override void WriteProcessingInstruction (string name, string value)
		{
			throw new NotSupportedException ();
		}

		public override void WriteComment (string text)
		{
			throw new NotSupportedException ();
		}

		public override void WriteCData (string text)
		{
			throw new NotSupportedException ();
		}

		public override void WriteStartElement (string prefix, string name, string ns)
		{
			throw new NotSupportedException ();
		}

		public override void WriteEndElement ()
		{
			throw new NotSupportedException ();
		}

		public override void WriteFullEndElement ()
		{
			throw new NotSupportedException ();
		}

		public override void WriteDocType (string name, string pubid, string systemId, string intsubset)
		{
			throw new NotSupportedException ();
		}

		public override void WriteStartDocument ()
		{
			throw new NotSupportedException ();
		}

		public override void WriteStartDocument (bool standalone)
		{
			throw new NotSupportedException ();
		}

		public override void WriteEndDocument ()
		{
			throw new NotSupportedException ();
		}

		public override void WriteBase64 (byte [] data, int start, int length)
		{
			throw new NotSupportedException ();
		}

		public override void WriteRaw (char [] raw, int start, int length)
		{
			throw new NotSupportedException ();
		}

		public override void WriteRaw (string raw)
		{
			throw new NotSupportedException ();
		}

		public override void WriteSurrogateCharEntity (char msb, char lsb)
		{
			throw new NotSupportedException ();
		}

		public override void WriteCharEntity (char c)
		{
			throw new NotSupportedException ();
		}

		public override void WriteEntityRef (string entname)
		{
			if (state != WriteState.Attribute)
				throw new InvalidOperationException ("Current state is not inside attribute. Cannot write attribute value.");
			attribute.AppendChild (attribute.OwnerDocument.CreateEntityReference (entname));
		}

		public override void WriteChars (char [] data, int start, int length)
		{
			WriteString (new string (data, start, length));
		}

		public override void WriteString (string text)
		{
			if (state != WriteState.Attribute)
				throw new InvalidOperationException ("Current state is not inside attribute. Cannot write attribute value.");
			attribute.Value += text;
		}

		public override void WriteWhitespace (string text)
		{
			if (state != WriteState.Attribute)
				throw new InvalidOperationException ("Current state is not inside attribute. Cannot write attribute value.");
			if (attribute.ChildNodes.Count == 0)
				attribute.AppendChild (attribute.OwnerDocument.CreateWhitespace (text));
			else
				attribute.Value += text;
		}

		public override void WriteEndAttribute ()
		{
			if (state != WriteState.Attribute)
				throw new InvalidOperationException ("Current state is not inside attribute. Cannot close attribute.");
			element.SetAttributeNode (attribute);
			attribute = null;
			state = WriteState.Content;
		}
	}

	internal class XmlDocumentEditableNavigator : XPathNavigator, IHasXmlNode
	{
		static readonly bool isXmlDocumentNavigatorImpl;
		
		static XmlDocumentEditableNavigator ()
		{
			isXmlDocumentNavigatorImpl =
				(typeof (XmlDocumentEditableNavigator).Assembly 
				== typeof (XmlDocument).Assembly);
		}

		XPathEditableDocument document;
		XPathNavigator navigator;

		public XmlDocumentEditableNavigator (XPathEditableDocument doc)
		{
			document = doc;
			if (isXmlDocumentNavigatorImpl)
				navigator = new XmlDocumentNavigator (doc.Node);
			else
				navigator = doc.CreateNavigator ();
		}

		public XmlDocumentEditableNavigator (XmlDocumentEditableNavigator nav)
		{
			document = nav.document;
			navigator = nav.navigator.Clone ();
		}

		public override string BaseURI {
			get { return navigator.BaseURI; }
		}

		public override bool CanEdit {
			get { return true; }
		}

		public override bool IsEmptyElement {
			get { return navigator.IsEmptyElement; }
		}

		public override string LocalName {
			get { return navigator.LocalName; }
		}

		public override XmlNameTable NameTable {
			get { return navigator.NameTable; }
		}

		public override string Name {
			get { return navigator.Name; }
		}

		public override string NamespaceURI {
			get { return navigator.NamespaceURI; }
		}

		public override XPathNodeType NodeType {
			get { return navigator.NodeType; }
		}

		public override string Prefix {
			get { return navigator.Prefix; }
		}

		public override IXmlSchemaInfo SchemaInfo {
			get { return navigator.SchemaInfo; }
		}

		public override object UnderlyingObject {
			get { return navigator.UnderlyingObject; }
		}

		public override string Value {
			get { return navigator.Value; }
		}

		public override string XmlLang {
			get { return navigator.XmlLang; }
		}

		public override bool HasChildren {
			get { return navigator.HasChildren; }
		}

		public override bool HasAttributes {
			get { return navigator.HasAttributes; }
		}

		public override XPathNavigator Clone ()
		{
			return new XmlDocumentEditableNavigator (this);
		}

		public override XPathNavigator CreateNavigator ()
		{
			return navigator.Clone ();
		}

		public XmlNode GetNode ()
		{
			return ((IHasXmlNode) navigator).GetNode ();
		}

		public override bool IsSamePosition (XPathNavigator other)
		{
			XmlDocumentEditableNavigator nav = other as XmlDocumentEditableNavigator;
			if (nav != null)
				return navigator.IsSamePosition (nav.navigator);
			else
				return navigator.IsSamePosition (nav);
		}

		public override bool MoveTo (XPathNavigator other)
		{
			XmlDocumentEditableNavigator nav = other as XmlDocumentEditableNavigator;
			if (nav != null)
				return navigator.MoveTo (nav.navigator);
			else
				return navigator.MoveTo (nav);
		}

		public override bool MoveToFirstAttribute ()
		{
			return navigator.MoveToFirstAttribute ();
		}

		public override bool MoveToFirstChild ()
		{
			return navigator.MoveToFirstChild ();
		}

		public override bool MoveToFirstNamespace (XPathNamespaceScope scope)
		{
			return navigator.MoveToFirstNamespace (scope);
		}

		public override bool MoveToId (string id)
		{
			return navigator.MoveToId (id);
		}

		public override bool MoveToNext ()
		{
			return navigator.MoveToNext ();
		}

		public override bool MoveToNextAttribute ()
		{
			return navigator.MoveToNextAttribute ();
		}

		public override bool MoveToNextNamespace (XPathNamespaceScope scope)
		{
			return navigator.MoveToNextNamespace (scope);
		}

		public override bool MoveToParent ()
		{
			return navigator.MoveToParent ();
		}

		public override bool MoveToPrevious ()
		{
			return navigator.MoveToPrevious ();
		}

		public override XmlWriter AppendChild ()
		{
			XmlNode n = ((IHasXmlNode) navigator).GetNode ();
			if (n == null)
				throw new InvalidOperationException ("Should not happen.");
			return new XmlDocumentInsertionWriter (n, null);
		}

		public override void DeleteRange (XPathNavigator lastSiblingToDelete)
		{
			if (lastSiblingToDelete == null)
				throw new ArgumentNullException ();

			XmlNode start = ((IHasXmlNode) navigator).GetNode ();
			XmlNode end = null;
			if (lastSiblingToDelete is IHasXmlNode)
				end = ((IHasXmlNode) lastSiblingToDelete).GetNode ();
			// After removal, it moves to parent node.
			if (!navigator.MoveToParent ())
				throw new InvalidOperationException ("There is no parent to remove current node.");

			if (end == null || start.ParentNode != end.ParentNode)
				throw new InvalidOperationException ("Argument XPathNavigator has different parent node.");

			XmlNode parent = start.ParentNode;
			XmlNode next;
			bool loop = true;
			for (XmlNode n = start; loop; n = next) {
				loop = n != end;
				next = n.NextSibling;
				parent.RemoveChild (n);
			}
		}

		public override XmlWriter ReplaceRange (XPathNavigator nav)
		{
			if (nav == null)
				throw new ArgumentNullException ();

			XmlNode start = ((IHasXmlNode) navigator).GetNode ();
			XmlNode end = null;
			if (nav is IHasXmlNode)
				end = ((IHasXmlNode) nav).GetNode ();
			if (end == null || start.ParentNode != end.ParentNode)
				throw new InvalidOperationException ("Argument XPathNavigator has different parent node.");

			XmlDocumentInsertionWriter w =
				(XmlDocumentInsertionWriter) InsertBefore ();

			// local variables to anonymous delegate
			XPathNavigator prev = Clone ();
			if (!prev.MoveToPrevious ())
				prev = null;
			XPathNavigator parentNav = Clone ();
			parentNav.MoveToParent ();

			w.Closed += delegate (XmlWriter writer) {
				XmlNode parent = start.ParentNode;
				XmlNode next;
				bool loop = true;
				for (XmlNode n = start; loop; n = next) {
					loop = n != end;
					next = n.NextSibling;
					parent.RemoveChild (n);
				}
				if (prev != null) {
					MoveTo (prev);
					MoveToNext ();
				} else {
					MoveTo (parentNav);
					MoveToFirstChild ();
				}
			};

			return w;
		}

		public override XmlWriter InsertBefore ()
		{
			XmlNode n = ((IHasXmlNode) navigator).GetNode ();
			return new XmlDocumentInsertionWriter (n.ParentNode, n);
		}

		public override XmlWriter CreateAttributes ()
		{
			XmlNode n = ((IHasXmlNode) navigator).GetNode ();
			return new XmlDocumentAttributeWriter (n);
		}

		public override void DeleteSelf ()
		{
			XmlNode n = ((IHasXmlNode) navigator).GetNode ();
			XmlAttribute a = n as XmlAttribute;
			if (a != null) {
				if (a.OwnerElement == null)
					throw new InvalidOperationException ("This attribute node cannot be removed since it has no owner element.");
				navigator.MoveToParent ();
				a.OwnerElement.RemoveAttributeNode (a);
			} else {
				if (n.ParentNode == null)
					throw new InvalidOperationException ("This node cannot be removed since it has no parent.");
				navigator.MoveToParent ();
				n.ParentNode.RemoveChild (n);
			}
		}

		public override void ReplaceSelf (XmlReader reader)
		{
			XmlNode n = ((IHasXmlNode) navigator).GetNode ();
			XmlNode p = n.ParentNode;
			if (p == null)
				throw new InvalidOperationException ("This node cannot be removed since it has no parent.");

			bool movenext = false;
			if (!MoveToPrevious ())
				MoveToParent ();
			else
				movenext = true;

			XmlDocument doc = p.NodeType == XmlNodeType.Document ?
				p as XmlDocument : p.OwnerDocument;
			bool error = false;
			if (reader.ReadState == ReadState.Initial) {
				reader.Read ();
				if (reader.EOF)
					error = true;
				else
					while (!reader.EOF)
						p.AppendChild (doc.ReadNode (reader));
			} else {
				if (reader.EOF)
					error = true;
				else
					p.AppendChild (doc.ReadNode (reader));
			}
			if (error)
				throw new InvalidOperationException ("Content is required in argument XmlReader to replace current node.");

			p.RemoveChild (n);

			if (movenext)
				MoveToNext ();
			else
				MoveToFirstChild ();
		}

		public override void SetValue (string value)
		{
			XmlNode n = ((IHasXmlNode) navigator).GetNode ();
			while (n.FirstChild != null)
				n.RemoveChild (n.FirstChild);
			n.InnerText = value;
		}

		public override void MoveToRoot ()
		{
			navigator.MoveToRoot ();
		}

		public override bool MoveToNamespace (string name)
		{
			return navigator.MoveToNamespace (name);
		}

		public override bool MoveToFirst ()
		{
			return navigator.MoveToFirst ();
		}

		public override bool MoveToAttribute (string localName, string namespaceURI)
		{
			return navigator.MoveToAttribute (localName, namespaceURI);
		}

		public override bool IsDescendant (XPathNavigator nav)
		{
			XmlDocumentEditableNavigator e = nav as XmlDocumentEditableNavigator;
			if (e != null)
				return navigator.IsDescendant (e.navigator);
			else
				return navigator.IsDescendant (nav);
		}

		public override string GetNamespace (string name)
		{
			return navigator.GetNamespace (name);
		}

		public override string GetAttribute (string localName, string namespaceURI)
		{
			return navigator.GetAttribute (localName, namespaceURI);
		}

		public override XmlNodeOrder ComparePosition (XPathNavigator nav)
		{
			XmlDocumentEditableNavigator e = nav as XmlDocumentEditableNavigator;
			if (e != null)
				return navigator.ComparePosition (e.navigator);
			else
				return navigator.ComparePosition (nav);
		}
	}
}

#endif
