//
// Mono.Xml.XPath.XPathDocument2Editable
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
//
// Yet another implementation of XPathEditableNavigator.
// (Even runnable under MS.NET 2.0)
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
/*
	public class Driver
	{
		public static void Main ()
		{
			try {
#if true
				XPathDocument2 doc = new XPathDocument2 ();
				XPathDocument2Editable pd = new XPathDocument2Editable (doc);
				XPathEditableNavigator nav = pd.CreateEditor ();
				IChangeTracking xp = pd;
#else
				XPathDocument doc = new XPathDocument ();
				XPathEditableNavigator nav = doc.CreateEditor ();
				IChangeTracking xp = doc;
#endif
				doc.LoadXml ("<root/>");
				nav.MoveToFirstChild (); // root
				XmlWriter w = nav.AppendChild ();
				Console.WriteLine (((IChangeTracking) xp).IsChanged);
				w.WriteElementString ("foo", "foo_text");
				w.WriteElementString ("bar", "bar_text");
				w.WriteStartElement ("hoge");
				w.WriteAttributeString ("fuga", "fugafuga");
				w.WriteAttributeString ("unya", "unyaunya");
				w.WriteFullEndElement ();
				w.Close ();

				w = nav.CreateAttributes ();
				w.WriteStartAttribute ("namara");
				w.WriteString ("mokera");
				w.WriteEndAttribute ();
				w.WriteAttributeString ("beccho", "giccho");
				w.Close ();

				nav.MoveToRoot ();
				nav.MoveToFirstChild ();
				nav.MoveToFirstChild ();
				nav.DeleteCurrent (); // delete foo
				Console.WriteLine (nav.Name);
				nav.MoveToNext ();
				Console.WriteLine (nav.Name);
				Console.WriteLine (nav.MoveToFirstAttribute ());
				nav.DeleteCurrent (); // delete fuga

				doc.Save (Console.Out);
			} catch (Exception ex) {
				Console.WriteLine (ex);
			}
		}
	}
*/

	public class XPathDocument2Editable
		: IXPathNavigable, IXPathEditable,
		IRevertibleChangeTracking, IChangeTracking, IXmlSerializable
	{

		XPathDocument2 document;

		ArrayList changes = new ArrayList ();
		bool enableChangeTracking;

		public XPathDocument2Editable (XPathDocument2 doc)
		{
			document = doc;
		}

#region Events

		public event NodeChangedEventHandler ChangeRejected;

		public event NodeChangedEventHandler ItemUpdated;

		public event NodeChangedEventHandler ItemUpdating;

		public event NodeChangedEventHandler ItemInserted;

		public event NodeChangedEventHandler ItemInserting;

		public event NodeChangedEventHandler ItemDeleted;

		public event NodeChangedEventHandler ItemDeleting;

		public event NodeChangedEventHandler RejectingChange;

#endregion // Events

		public XmlNameTable NameTable {
			get { return document.NameTable; }
		}

		public XPathNavigator CreateNavigator ()
		{
			return document.CreateNavigator ();
		}

		public XPathEditableNavigator CreateEditor ()
		{
			return new XomEditableNavigator (this);
		}

		public XmlWriter CreateWriter ()
		{
			return CreateEditor ().AppendChild ();
		}

		public bool HasChanges ()
		{
			return IsChanged;
		}

		public bool EnableChangeTracking {
			get { return enableChangeTracking; }
			set { enableChangeTracking = value; }
		}

		#region IRevertibleChangeTracking/IChangeTracking
		public bool IsChanged {
			get { return changes.Count != 0; }
		}

		public void AcceptChanges ()
		{
			changes.Clear ();
		}

		public void RejectChanges ()
		{
			for (int i = changes.Count - 1; i >= 0; i--) {
				Insertion2 ins = changes [i] as Insertion2;
				if (ins != null) {
					ins.ParentNode.RemoveChild (ins.InsertedNode);
					continue;
				}
				
				Removal2 rem = changes [i] as Removal2;
				if (rem != null) {
					if (rem.RemovedNode.NodeType == XPathNodeType.Attribute) {
						XomElement el = (XomElement) rem.OwnerNode;
						el.AppendAttribute ((XomAttribute) rem.RemovedNode);
					}
					else
						rem.OwnerNode.InsertBefore (rem.RemovedNode, rem.NextSibling);
					continue;
				}
				AttributeUpdate2 au = changes [i] as AttributeUpdate2;
				if (au != null) {
					au.Element.RemoveAttribute (au.NewAttribute);
					if (au.OldAttribute != null)
						au.Element.AppendAttribute (au.OldAttribute);
					continue;
				}
			}
			changes.Clear ();
		}
		#endregion

		#region IXmlSerializable
		public void WriteXml (XmlWriter writer)
		{
			throw new NotImplementedException ();
		}

		public void ReadXml (XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		public XmlSchema GetSchema ()
		{
			throw new NotImplementedException ();
		}
		#endregion

		internal bool DeleteNode (XomNode node)
		{
			Removal2 rem = new Removal2 ();
			if (node.NodeType == XPathNodeType.Attribute) {
				XomAttribute attr = node as XomAttribute;
				rem.OwnerNode = attr.Parent;
				rem.RemovedNode = node;
				((XomElement) attr.Parent).RemoveAttribute (attr);
				return false;
			} else {
				rem.OwnerNode = node.Parent;
				rem.NextSibling = node.NextSibling;
				rem.RemovedNode = node;
				node.Parent.RemoveChild (node);
				return rem.NextSibling != null;
			}
		}

		internal XmlWriter CreateInsertionWriter (XomNode owner, XomNode previousSibling)
		{
			return new XPathDocument2InsertionWriter (owner, previousSibling, this);
		}

		internal XmlWriter CreateAttributesWriter (XomNode owner)
		{
			return new XPathDocument2AttributeWriter (owner, this);
		}

		internal void AttributeUpdate2 (XomElement element, XomAttribute oldAttr, XomAttribute newAttr)
		{
			AttributeUpdate2 au = new AttributeUpdate2 ();
			au.Element = element;
			au.OldAttribute = oldAttr;
			au.NewAttribute = newAttr;
			changes.Add (au);
		}
		
		internal void AppendChild (XomParentNode parent, XomNode child)
		{
			Insertion2 ins = new Insertion2 ();
			ins.ParentNode = parent;
			ins.InsertedNode = child;
			changes.Add (ins);
		}
	}

	public class XPathDocument2InsertionWriter : XmlWriter
	{
		XomNode current;
		XomNode previousSibling;
		XPathDocument2Editable document;
		Stack nodeStack = new Stack ();

		public XPathDocument2InsertionWriter (XomNode owner, XomNode previousSibling, XPathDocument2Editable doc)
		{
			this.current = (XomNode) owner;
			if (current == null)
				throw new InvalidOperationException ();
			this.previousSibling = previousSibling;
			this.document = doc;
			state = WriteState.Content;
		}

		WriteState state;
		XomAttribute attribute;

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
			return current.LookupPrefix (ns);
		}

		public override void WriteStartAttribute (string prefix, string name, string ns)
		{
			if (state != WriteState.Content)
				throw new InvalidOperationException ("Current state is not inside element. Cannot start attribute.");
			attribute = new XomAttribute (prefix, name, ns, String.Empty);
			state = WriteState.Attribute;
		}

		public override void WriteProcessingInstruction (string name, string value)
		{
			XomParentNode p = current as XomParentNode;
			if (p == null)
				throw new InvalidOperationException ("Current writer node cannot have a child.");
			XomPI pi = new XomPI (name, value);
			p.AppendChild (pi);
			document.AppendChild (p, pi);
		}

		public override void WriteComment (string text)
		{
			XomParentNode p = current as XomParentNode;
			if (p == null)
				throw new InvalidOperationException ("Current writer node cannot have a child.");
			XomComment comment = new XomComment (text);
			p.AppendChild (comment);
			document.AppendChild (p, comment);
		}

		public override void WriteCData (string text)
		{
			if (attribute != null)
				throw new InvalidOperationException ("Current writer node is attribute. It cannot accept CDATA section.");
			/*
			XomParentNode p = current as XomParentNode;
			if (p == null)
				throw new InvalidOperationException ("Current writer node cannot have a child.");
			XomText cdata = new XomText (text);
			p.AppendChild (cdata);
			document.AppendChild (p, cdata);
			*/
			WriteString (text);
		}

		public override void WriteStartElement (string prefix, string name, string ns)
		{
			XomParentNode p = current as XomParentNode;
			if (p == null)
				throw new InvalidOperationException ("Current writer node cannot have a child.");
			XomElement el = new XomElement (prefix, name, ns);
			p.AppendChild (el);
			document.AppendChild (p, el);
			nodeStack.Push (current);
			current = el;
		}

		public override void WriteEndElement ()
		{
			WriteFullEndElement ();
			XomElement el = current as XomElement;
			if (el != null && el.ChildCount == 0)
				el.SetIsEmpty (true);
		}

		public override void WriteFullEndElement ()
		{
			if (nodeStack.Count == 0)
				throw new InvalidOperationException ("No element is opened.");
			current = nodeStack.Pop () as XomNode;
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
			throw new NotSupportedException ();
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
				XomParentNode p = current as XomParentNode;
				if (p == null)
					throw new InvalidOperationException ("Current writer node cannot have a child.");

				XomText xt = new XomText (text);
				p.AppendChild (xt);
				document.AppendChild (p, xt);
			}
		}

		public override void WriteWhitespace (string text)
		{
			if (attribute != null)
				attribute.Value += text;
			else {
				XomParentNode p = current as XomParentNode;
				if (p == null)
					throw new InvalidOperationException ("Current writer node cannot have a child.");

				XomWhitespace ws = new XomWhitespace (text);
				p.AppendChild (ws);
				document.AppendChild (p, ws);
			}
		}

		public override void WriteEndAttribute ()
		{
			XomElement element = current as XomElement;
			if (state != WriteState.Attribute || element == null)
				throw new InvalidOperationException ("Current state is not inside attribute. Cannot close attribute.");
			XomAttribute old = element.GetAttribute (attribute.LocalName, attribute.Namespace);
			element.AppendAttribute (attribute);
			document.AttributeUpdate2 (element, old, attribute);
			attribute = null;
			state = WriteState.Content;
		}
	}

	public class XPathDocument2AttributeWriter : XmlWriter
	{
		XomElement element;
		XPathDocument2Editable document;

		public XPathDocument2AttributeWriter (XomNode owner, XPathDocument2Editable doc)
		{
			element = owner as XomElement;
			if (element == null)
				throw new ArgumentException ("To write attributes, current node must be an element.");
			state = WriteState.Content;
			document = doc;
		}

		WriteState state;
		XomAttribute attribute;

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
			return element.LookupPrefix (ns);
		}

		public override void WriteStartAttribute (string prefix, string name, string ns)
		{
			if (state != WriteState.Content)
				throw new InvalidOperationException ("Current state is not inside element. Cannot start attribute.");
			attribute = new XomAttribute (prefix, name, ns, String.Empty);
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
			throw new NotSupportedException ();
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
			else
				attribute.Value += text;
		}

		public override void WriteEndAttribute ()
		{
			if (state != WriteState.Attribute)
				throw new InvalidOperationException ("Current state is not inside attribute. Cannot close attribute.");
			XomAttribute old = element.GetAttribute (attribute.LocalName, attribute.Namespace);
			element.AppendAttribute (attribute);
			document.AttributeUpdate2 (element, old, attribute);
			attribute = null;
			state = WriteState.Content;
		}
	}

	public class Insertion2
	{
		// AppendChild : last child / true
		// InsertBefore : current node / false
		// InsertAfter : current node / true
		// PrependChild : first child / false
		public XomParentNode ParentNode;
		public XomNode InsertedNode;
		public bool Afterward;
	}

	public class Removal2
	{
		public XomParentNode OwnerNode;
		public XomNode NextSibling;
		public XomNode RemovedNode;
	}

	public class AttributeUpdate2
	{
		public XomElement Element;
		public XomAttribute NewAttribute;
		public XomAttribute OldAttribute;
	}

	public class XomEditableNavigator : XPathEditableNavigator, IHasXomNode
	{
		XPathDocument2Editable document;
		XPathNavigator navigator;

		public XomEditableNavigator (XPathDocument2Editable doc)
		{
			document = doc;
			navigator = doc.CreateNavigator ();
		}

		public XomEditableNavigator (XomEditableNavigator nav)
		{
			document = nav.document;
			navigator = nav.navigator.Clone ();
		}

		public override string BaseURI {
			get { return navigator.BaseURI; }
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

		public override string Value {
			get { return navigator.Value; }
		}

		public override XPathNavigator Clone ()
		{
			return new XomEditableNavigator (this);
		}

		public override XPathNavigator CreateNavigator ()
		{
			return navigator.Clone ();
		}

		public XomNode GetNode ()
		{
			return ((IHasXomNode) navigator).GetNode ();
		}

		public override bool IsSamePosition (XPathNavigator other)
		{
			XomEditableNavigator nav = other as XomEditableNavigator;
			if (nav != null)
				return navigator.IsSamePosition (nav.navigator);
			else
				return navigator.IsSamePosition (nav);
		}

		public override bool MoveTo (XPathNavigator other)
		{
			XomEditableNavigator nav = other as XomEditableNavigator;
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

		public override void Validate (XmlSchemaSet schemas, ValidationEventHandler handler)
		{
			throw new NotImplementedException ();
/*
			// FIXME: use handler
			XmlReaderSettings settings = new XmlReaderSettings ();
			settings.Schemas.Add (schemas);
			settings.NameTable = this.NameTable;
			settings.XsdValidate = true;
			settings.DtdValidate = false;
			XmlReader xvr = XmlReader.Create (new XPathNavigatorReader (this), settings);
			while (!xvr.EOF)
				xvr.Read ();
*/
		}

		public override XmlWriter AppendChild ()
		{
			XomNode n = ((IHasXomNode) navigator).GetNode ();
			if (n == null)
				throw new InvalidOperationException ("Should not happen.");
			return document.CreateInsertionWriter (n, null);
		}

		public override XmlWriter InsertBefore ()
		{
			XomNode n = ((IHasXomNode) navigator).GetNode ();
			return document.CreateInsertionWriter (n.Parent, n.PreviousSibling);
		}

		public override XmlWriter CreateAttributes ()
		{
			XomNode n = ((IHasXomNode) navigator).GetNode ();
			return document.CreateInsertionWriter (n, null);
		}

		public override bool DeleteCurrent ()
		{
			XomNode n = ((IHasXomNode) navigator).GetNode ();
			if (!navigator.MoveToNext ())
				navigator.MoveToParent ();
			return document.DeleteNode (n);
		}

		public override void SetValue (object value)
		{
			XomNode n = ((IHasXomNode) navigator).GetNode ();
			int count = n.ChildCount;
			while (n.FirstChild != null)
				document.DeleteNode (n.FirstChild);
			XmlWriter w = document.CreateInsertionWriter (n, null);
			// FIXME: Hmm, it does not look like using it.
			w.WriteFromObject (value);
			w.Close ();
		}
	}
}

#endif
