//
// Mono.Xml.XPath.XPathDocument2.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
//
// Another Document tree model.
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

/*
	public class Driver
	{
		public static void Main ()
		{
			try {
				DateTime start = DateTime.Now;
				Console.WriteLine (DateTime.Now.Ticks);
#if false
				XmlDocument doc = new XmlDocument ();
				doc.PreserveWhitespace = true;
				doc.Load (new XmlTextReader ("../TestResult.xml"));
#else
				XPathDocument2 doc = new XPathDocument2 ();
				doc.Load (new XmlTextReader ("../TestResult.xml"));
//				XPathDocument doc = new XPathDocument ("../TestResult.xml", XmlSpace.Preserve);
//				XPathDocument doc = new XPathDocument ("test.xml", XmlSpace.Preserve);
#endif
//				doc.Load (new XmlTextReader ("test.xml"));

//				doc.WriteTo (new XmlTextWriter (Console.Out));
//return;
				Console.WriteLine (DateTime.Now.Ticks);

				XPathNavigator nav = doc.CreateNavigator ();
//Console.WriteLine (nav.MoveToFirstChild ());
//Console.WriteLine (nav.LocalName + nav.NodeType);
//Console.WriteLine (nav.MoveToNext ());
//Console.WriteLine (nav.LocalName + nav.NodeType);
//Console.WriteLine (nav.MoveToNext ());
//Console.WriteLine (nav.LocalName + nav.NodeType);
//Console.WriteLine (nav.MoveToNext ());
//Console.WriteLine (nav.LocalName + nav.NodeType);
//nav.MoveToRoot ();


				XmlReader reader = nav.ReadSubtree ();
				XmlTextWriter w = new XmlTextWriter (new StringWriter ());
//				XmlTextWriter w = new XmlTextWriter (Console.Out);
				w.WriteNode (reader, false);
				Console.WriteLine (DateTime.Now.Ticks);
				Console.WriteLine (DateTime.Now.Ticks - start.Ticks);
			} catch (Exception ex) {
				Console.WriteLine (ex);
			}
		}
	}
*/

	// Wrapper

	public class XPathDocument2 : IXPathNavigable
	{
		XomRoot root;

		public XPathDocument2 ()
			: this (new NameTable ())
		{
		}
		
		public XPathDocument2 (XmlNameTable nameTable)
		{
			root = new XomRoot (nameTable);
		}

		public XmlNameTable NameTable {
			get { return root.NameTable; }
		}

		public void LoadXml (string xml)
		{
			Load (new XmlTextReader (xml, XmlNodeType.Document, null));
		}

		public void Load (TextReader reader)
		{
			Load (new XmlTextReader (reader));
		}

		public void Load (XmlReader reader)
		{
			Load (reader, XmlSpace.None);
		}

		public void Load (XmlReader reader, XmlSpace space)
		{
			root.Load (reader, space);
		}

		public void Save (TextWriter writer)
		{
			XmlTextWriter xtw = new XmlTextWriter (writer);
			xtw.Formatting = Formatting.Indented;
			WriteTo (xtw);
		}

		public void Save (XmlWriter writer)
		{
			WriteTo (writer);
		}

		public void WriteTo (XmlWriter writer)
		{
			root.WriteTo (writer);
		}

		public XPathNavigator CreateNavigator ()
		{
			return new XomNavigator (root);
		}
	}

	// Xom part

	public struct XmlName
	{
		public string Prefix;
		public string LocalName;
		public string Namespace;
		string fullName;

		public XmlName (string prefix, string name, string ns)
		{
			this.Prefix = prefix == null ? "" : prefix;
			this.LocalName = name;
			this.Namespace = ns == null ? "" : ns;
			fullName = null;
		}

		public override bool Equals (object o)
		{
			if ( !(o is XmlName))
				return false;
			XmlName other = (XmlName) o;
			return LocalName == other.LocalName && Namespace == other.Namespace;
		}

		public static bool operator == (XmlName n1, XmlName n2)
		{
			return n1.LocalName == n2.LocalName && n1.Namespace == n2.Namespace;
		}

		public static bool operator != (XmlName n1, XmlName n2)
		{
			return n1.LocalName != n2.LocalName || n1.Namespace != n2.Namespace;
		}

		public override int GetHashCode ()
		{
			if (fullName == null)
				fullName = String.Concat (LocalName, "/", Namespace);
			return fullName.GetHashCode ();
		}

		public override string ToString ()
		{
			if (fullName == null)
				fullName = String.Concat (LocalName, "/", Namespace);
			return fullName;
		}
	}

	public abstract class XomNode
	{
		XomParentNode parent;
		string prefixedName;
		XomNode previousSibling;
		XomNode nextSibling;

		public XomRoot Root {
			get {
				XomNode n = this;
				while (n.parent != null)
					n = n.parent;
				return (XomRoot) n;
			}
		}

		public string PrefixedName {
			get {
				if (prefixedName == null) {
					if (Prefix.Length > 0)
						prefixedName = Prefix + ':' + LocalName;
					else
						prefixedName = LocalName;
				}
				return prefixedName;
			}
		}

		public virtual string BaseURI {
			get { return Root.BaseURI; }
		}

		public virtual string XmlLang {
			get { return String.Empty; }
		}

		public XomParentNode Parent {
			get { return parent; }
		}

		public XomNode PreviousSibling {
			get { return previousSibling; }
		}

		public XomNode NextSibling {
			get { return nextSibling; }
		}

		internal void SetParent (XomParentNode parent)
		{
			this.parent = parent;
		}

		internal void SetPreviousSibling (XomNode previous)
		{
			if (previous.parent != parent || this == previous)
				throw new InvalidOperationException ();
			nextSibling = previous.nextSibling;
			previousSibling = previous;
			previous.nextSibling = this;
		}

		internal void SetNextSibling (XomNode next)
		{
			if (next.parent != parent || this == next)
				throw new InvalidOperationException ();
			previousSibling = next.previousSibling;
			nextSibling = next;
			next.previousSibling = this;
		}

		internal void RemoveItself ()
		{
			if (previousSibling != null)
				previousSibling.nextSibling = nextSibling;
			if (nextSibling != null)
				nextSibling.previousSibling = previousSibling;
			parent = null;
		}

		public string LookupPrefix (string ns)
		{
			XomElement n = this as XomElement;
			if (n == null)
				n = Parent as XomElement;
			while (n != null) {
				int len = n.NamespaceCount;
				for (int i = 0; i < len; i++) {
					XomNamespace nn = n.GetLocalNamespace (i);
					if (nn.Value == ns)
						return nn.LocalName;
				}
			}
			return null;
		}

		public string LookupNamespace (string ns)
		{
			XomElement n = this as XomElement;
			if (n == null)
				n = Parent as XomElement;
			while (n != null) {
				int len = n.NamespaceCount;
				for (int i = 0; i < len; i++) {
					XomNamespace nn = n.GetLocalNamespace (i);
					if (nn.Namespace== ns)
						return nn.Prefix;
				}
			}
			return null;
		}

		public virtual bool IsEmptyElement {
			get { return false; }
		}

		public abstract string LocalName { get; }

		public abstract string Namespace { get; }

		public abstract string Prefix { get; }

		public abstract string Value { get; set; }

		public abstract XPathNodeType NodeType { get; }

		public abstract int ChildCount { get; }

		public virtual XomNode FirstChild { get { return null; } }

		public virtual XomNode LastChild { get { return null; } }

		internal abstract void BuildValue (StringBuilder sb);

		public string OuterXml {
			get {
				StringWriter sw = new StringWriter ();
				XmlTextWriter xtw = new XmlTextWriter (sw);
				WriteTo (xtw);
				return sw.ToString ();
			}
		}

		public string InnerXml {
			get {
				StringWriter sw = new StringWriter ();
				XmlTextWriter xtw = new XmlTextWriter (sw);
				for (XomNode n = FirstChild; n != null; n = n.NextSibling)
					n.WriteTo (xtw);
				return sw.ToString ();
			}
		}

		public abstract void WriteTo (XmlWriter writer);
	}

	public interface IHasXomNode
	{
		XomNode GetNode ();
	}

	public abstract class XomParentNode : XomNode
	{
		XomNode firstChild;
		XomNode lastChild;
		int childCount;

		public void ReadNode (XmlReader reader, XmlSpace space)
		{
			switch (reader.ReadState) {
			case ReadState.Initial:
				reader.Read ();
				break;
			case ReadState.Error:
			case ReadState.Closed:
			case ReadState.EndOfFile:
				throw new ArgumentException ("Argument XmlReader is not readable.");
			}

			switch (reader.NodeType) {
			case XmlNodeType.Element:
				XomElement el = new XomElement (reader.Prefix, reader.LocalName, reader.NamespaceURI, this);
				if (reader.MoveToFirstAttribute ()) {
					do {
						new XomAttribute (reader.Prefix, reader.LocalName, reader.NamespaceURI, reader.Value, el);
					} while (reader.MoveToNextAttribute ());
					reader.MoveToContent ();
				}
				if (reader.IsEmptyElement) {
					el.SetIsEmpty (true);
					reader.Read ();
				}
				else {
					reader.Read ();
					while (reader.NodeType != XmlNodeType.EndElement)
						el.ReadNode (reader, space);
					reader.ReadEndElement ();
				}
				return;
			case XmlNodeType.Text:
			case XmlNodeType.CDATA:
				XomText text = new XomText (reader.Value, this);
				reader.Read ();
				return;
			case XmlNodeType.SignificantWhitespace:
				XomSignificantWhitespace sws = new XomSignificantWhitespace (reader.Value, this);
				reader.Read ();
				return;
			case XmlNodeType.Whitespace:
				if (space == XmlSpace.Default) {
					reader.Skip ();
					return;
				}
				XomWhitespace ws = new XomWhitespace (reader.Value, this);
				reader.Read ();
				return;
			case XmlNodeType.ProcessingInstruction:
				XomPI pi = new XomPI (reader.LocalName, reader.Value, this);
				reader.Read ();
				return;
			case XmlNodeType.Comment:
				XomComment comment = new XomComment (reader.Value, this);
				reader.Read ();
				return;
			default:
				reader.Skip ();
				return;
			}
		}

		public void AppendChild (XomNode child)
		{
			InsertBefore (child, null);
		}

		public void InsertBefore (XomNode child, XomNode nextNode)
		{
			if (child.Parent != null)
				throw new InvalidOperationException ("The child already has a parent.");
			if (nextNode == null) {
				child.SetParent (this);
				if (firstChild == null)
					firstChild = lastChild = child;
				else {
					child.SetPreviousSibling (lastChild);
					lastChild = child;
				}
			} else {
				if (nextNode.Parent != this)
					throw new ArgumentException ("Argument nextNode is not a child of this node.");
				child.SetNextSibling (nextNode);
			}
			childCount++;
		}

		public abstract void Clear ();

		public override XomNode FirstChild {
			get { return firstChild; }
		}

		public override XomNode LastChild {
			get { return lastChild; }
		}

		public override int ChildCount {
			get { return childCount; }
		}

		internal void ClearChildren ()
		{
			firstChild = lastChild = null;
			childCount = 0;
		}

		public void RemoveChild (XomNode child)
		{
			if (child == firstChild)
				firstChild = child.NextSibling;
			if (child == lastChild)
				lastChild = child.PreviousSibling;
			child.RemoveItself ();
			childCount--;
		}

		public override string Value {
			get {
				StringBuilder sb = new StringBuilder ();
				BuildValue (sb);
				return sb.ToString ();
			}
			set {
				ClearChildren ();
				AppendChild (new XomText (value));
			}
		}

		internal override void BuildValue (StringBuilder sb)
		{
			for (XomNode n = FirstChild; n != null; n = n.NextSibling)
				n.BuildValue (sb);
		}
	}

	public class XomRoot : XomParentNode
	{
		XmlNameTable nameTable;
		Hashtable identicalElements;
		string baseUri;

		public XomRoot (XmlNameTable nameTable)
		{
			this.nameTable = nameTable;
			identicalElements = new Hashtable ();
		}

		public override string BaseURI {
			get { return baseUri; }
		}

		public void Load (XmlReader reader)
		{
			Load (reader, XmlSpace.None);
		}

		public void Load (XmlReader reader, XmlSpace space)
		{
			baseUri = reader.BaseURI;
			while (!reader.EOF)
				ReadNode (reader, space);
		}

		public XmlNameTable NameTable {
			get { return nameTable; }
		}

		public override string Prefix {
			get { return ""; }
		}

		public override string LocalName {
			get { return ""; }
		}

		public override string Namespace {
			get { return ""; }
		}

		public override XPathNodeType NodeType {
			get { return XPathNodeType.Root; }
		}

		public override void Clear ()
		{
			ClearChildren ();
		}

		public override void WriteTo (XmlWriter writer)
		{
			for (XomNode n = FirstChild; n != null; n = n.NextSibling)
				n.WriteTo (writer);
		}

		public XomElement GetIdenticalNode (string id)
		{
			return identicalElements [id] as XomElement;
		}

		internal void GetIdenticalNode (string id, XomElement element)
		{
			identicalElements.Add (id, element);
		}
	}

	public class XomElement : XomParentNode
	{
		XmlName qname;
		bool isEmptyElement;

		ArrayList attributes;
		ArrayList namespaces;

		public XomElement (string name)
			: this ("", name, "", null)
		{
		}

		public XomElement (string name, XomRoot root)
			: this ("", name, "", root)
		{
		}

		public XomElement (string name, string ns)
			: this ("", name, ns, null)
		{
		}

		public XomElement (string name, string ns, XomRoot root)
			: this ("", name, ns, root)
		{
		}

		public XomElement (string prefix, string name, string ns)
			: this (prefix, name, ns, null)
		{
		}

		public XomElement (string prefix, string name, string ns, XomParentNode parent)
		{
			qname.LocalName = name;
			qname.Namespace = ns == null ? "" : ns;
			qname.Prefix = prefix == null ? "" : prefix;
			if (parent != null)
				parent.AppendChild (this);
		}

		public override bool IsEmptyElement {
			get { return isEmptyElement; }
		}

		internal void SetIsEmpty (bool value)
		{
			isEmptyElement = value;
		}

		public override string Prefix {
			get { return qname.Prefix; }
		}

		public override string LocalName {
			get { return qname.LocalName; }
		}

		public override string Namespace {
			get { return qname.Namespace; }
		}

		public override XPathNodeType NodeType {
			get { return XPathNodeType.Element; }
		}

		public override void Clear ()
		{
			if (attributes != null)
				attributes.Clear ();
			if (namespaces != null)
				namespaces.Clear ();
			ClearChildren ();
		}

		public void AppendAttribute (XomAttribute attr)
		{
			if (attr.Parent != null)
				throw new InvalidOperationException ("The argument attribute already have another element owner.");
			attr.SetParent (this);
			if (attributes == null)
				attributes = new ArrayList ();
			attributes.Add (attr);
		}

/*
		public void UpdateAttribute (XomAttribute attr)
		{
			if (attr.Parent != null)
				throw new InvalidOperationException ("The argument attribute already have another element owner.");
			XomAttribute existing = GetAttribute (attr.LocalName, attr.Namespace);
			if (existing != null)
				RemoveAttribute (existing);
			if (attributes == null)
				attributes = new ArrayList ();
			attr.SetParent (this);
			attributes.Add (attr);
		}
*/

		public XomAttribute GetAttribute (int index)
		{
			return attributes == null ? null : attributes [index] as XomAttribute;
		}

		public XomAttribute GetAttribute (string name, string ns)
		{
			if (attributes == null)
				return null;
			for (int i = 0; i < attributes.Count; i++) {
				XomAttribute a = attributes [i] as XomAttribute;
				if (a.LocalName == name && a.Namespace == ns)
					return a;
			}
			return null;
		}

		public XomAttribute GetNextAttribute (XomAttribute attr)
		{
			if (attributes == null || attributes.Count == 0)
				return null;
			if (attributes [attributes.Count - 1] == attr)
				return null;
			// It is not efficient, but usually there won't be so many attributes in an element.
			int index = attributes.IndexOf (attr);
			if (index < 0)
				return null;
			return attributes [index + 1] as XomAttribute;
		}

		public int AttributeCount {
			get { return attributes == null ? 0 : attributes.Count; }
		}

		public void RemoveAttribute (XomAttribute attr)
		{
			if (attributes == null)
				return;
			attributes.Remove (attr);
			attr.SetParent (null);
		}

		public void AppendNamespace (string prefix, string ns)
		{
			if (namespaces == null)
				namespaces = new ArrayList ();
			namespaces.Add (new XomNamespace (prefix, ns));
		}

		public XomNamespace GetLocalNamespace (int index)
		{
			if (namespaces == null || namespaces.Count <= index)
				return null;
			return namespaces [index] as XomNamespace;
		}

		public XomNamespace GetLocalNamespace (string prefix)
		{
			if (namespaces == null)
				return null;
			for (int i = 0; i < namespaces.Count; i++) {
				XomNamespace qname = namespaces [i] as XomNamespace;
				if (qname.LocalName == prefix)
					return qname;
			}
			return null;
		}

		public XomNamespace GetNextLocalNamespace (XomNamespace n)
		{
			if (namespaces == null || namespaces.Count == 0)
				return null;
			if (namespaces [namespaces.Count - 1] == n)
				return null;
			// It is not efficient, but usually there won't be so many attributes in an element.
			int index = namespaces.IndexOf (n);
			if (index < 0)
				return null;
			return namespaces [index + 1] as XomNamespace;
		}

		public int NamespaceCount {
			get { return namespaces == null ? 0 : namespaces.Count; }
		}

		public void RemoveNamespace (string prefix)
		{
			if (namespaces == null)
				return;
			for (int i = 0; i < namespaces.Count; i++) {
				XomNamespace qname = namespaces [i] as XomNamespace;
				if (qname.LocalName == prefix) {
					namespaces.RemoveAt (i);
					return;
				}
			}
		}

		public override void WriteTo (XmlWriter writer)
		{
			writer.WriteStartElement (Prefix, LocalName, Namespace);
			if (namespaces != null) {
				foreach (XomNamespace n in namespaces)
					n.WriteTo (writer);
			}
			if (attributes != null) {
				foreach (XomAttribute a in attributes)
					a.WriteTo (writer);
			}

			for (XomNode n = FirstChild; n != null; n = n.NextSibling)
				n.WriteTo (writer);

			writer.WriteEndElement ();
		}
	}

	public class XomAttribute : XomNode
	{
		XmlName qname;
		string value;

		public XomAttribute (string name, string value)
			: this ("", name, "", value, null)
		{
		}

		public XomAttribute (string name, string value, XomElement owner)
			: this ("", name, "", value, owner)
		{
		}

		public XomAttribute (string name, string ns, string value)
			: this ("", name, ns, value, null)
		{
		}

		public XomAttribute (string name, string ns, string value, XomElement owner)
			: this ("", name, ns, value, owner)
		{
		}

		public XomAttribute (string prefix, string name, string ns, string value)
			: this ("", name, ns, value, null)
		{
		}

		public XomAttribute (string prefix, string name, string ns, string value, XomElement owner)
		{
			qname.LocalName = name;
			qname.Namespace = ns;
			qname.Prefix = prefix;
			this.value = value;
			if (owner != null)
				owner.AppendAttribute (this);
		}

		public override string Prefix {
			get { return qname.Prefix; }
		}

		public override string LocalName {
			get { return qname.LocalName; }
		}

		public override string Namespace {
			get { return qname.Namespace; }
		}

		public override XPathNodeType NodeType {
			get { return XPathNodeType.Attribute; }
		}

		public override int ChildCount { get { return 0; } }

		public override string Value {
			get { return value; }
			set { this.value = value; }
		}

		internal override void BuildValue (StringBuilder sb)
		{
			sb.Append (value);
		}

		public override void WriteTo (XmlWriter writer)
		{
			writer.WriteAttributeString (Prefix, LocalName, Namespace, Value);
		}
	}

	public class XomNamespace : XomNode
	{
		#region static members
		static XomNamespace xml;

		static XomNamespace ()
		{
			xml = new XomNamespace ("xml", "http://www.w3.org/XML/1998/namespace");
		}

		public static XomNamespace Xml {
			get { return xml; }
		}
		#endregion

		XmlName qname;

		public XomNamespace (string prefix, string ns)
		{
			qname.LocalName = prefix;
			qname.Namespace = ns;
		}

		public override int ChildCount {
			get { return 0; }
		}

		public override string LocalName {
			get { return qname.LocalName; }
		}

		public override string Prefix {
			get { return LocalName; }
		}

		public override string Namespace {
			get { return Value; }
		}

		public override string Value {
			get { return qname.Namespace; }
			set { qname.Namespace = value; }
		}

		public override XPathNodeType NodeType {
			get { return XPathNodeType.Namespace; }
		}

		public override void WriteTo (XmlWriter writer)
		{
			if (LocalName != "")
				writer.WriteAttributeString ("xmlns", LocalName, "http://www.w3.org/2000/xmlns/", Namespace);
			else
				writer.WriteAttributeString ("", "xmlns", "http://www.w3.org/2000/xmlns/", Namespace);
		}

		internal override void BuildValue (StringBuilder sb)
		{
			sb.Append (Value);
		}
	}

	public class XomComment : XomNode
	{
		string value;

		public XomComment (string value)
			: this (value, null)
		{
		}

		public XomComment (string value, XomParentNode parent)
		{
			this.value = value;
			if (parent != null)
				parent.AppendChild (this);
		}

		public override string Prefix {
			get { return ""; }
		}

		public override string LocalName {
			get { return ""; }
		}

		public override string Namespace {
			get { return ""; }
		}

		public override XPathNodeType NodeType {
			get { return XPathNodeType.Comment; }
		}

		public override string Value {
			get { return value; }
			set { this.value += value; }
		}

		internal override void BuildValue (StringBuilder sb)
		{
			sb.Append (value);
		}

		public override int ChildCount { get { return 0; } }

		public override void WriteTo (XmlWriter writer)
		{
			writer.WriteComment (Value);
		}
	}

	public class XomPI : XomNode
	{
		string value;
		string name;

		public XomPI (string name, string value)
			: this (name, value, null)
		{
		}

		public XomPI (string name, string value, XomParentNode parent)
		{
			this.name = name;
			if (value == null)
				value = "";
			this.value = value;
			if (parent != null)
				parent.AppendChild (this);
		}

		public override string Prefix {
			get { return ""; }
		}

		public override string LocalName {
			get { return name; }
		}

		public override string Namespace {
			get { return ""; }
		}

		public override XPathNodeType NodeType {
			get { return XPathNodeType.ProcessingInstruction; }
		}

		public override string Value {
			get { return value; }
			set { this.value += value; }
		}

		internal override void BuildValue (StringBuilder sb)
		{
			sb.Append (value);
		}

		public override int ChildCount { get { return 0; } }

		public override void WriteTo (XmlWriter writer)
		{
			writer.WriteProcessingInstruction (LocalName, Value);
		}
	}

	public class XomText : XomNode
	{
		string value;

		public XomText (string value)
			: this (value, null)
		{
		}

		public XomText (string value, XomParentNode parent)
		{
			this.value = value;
			if (parent != null)
				parent.AppendChild (this);
		}

		public override string Prefix {
			get { return ""; }
		}

		public override string LocalName {
			get { return ""; }
		}

		public override string Namespace {
			get { return ""; }
		}

		public override string Value {
			get { return value; }
			set { this.value += value; }
		}

		public override XPathNodeType NodeType {
			get { return XPathNodeType.Text; }
		}

		internal override void BuildValue (StringBuilder sb)
		{
			sb.Append (value);
		}

		public override int ChildCount { get { return 0; } }

		public override void WriteTo (XmlWriter writer)
		{
			writer.WriteString (Value);
		}
	}

	public class XomWhitespace : XomNode
	{
		string value;

		public XomWhitespace (string value)
			: this (value, null)
		{
		}

		public XomWhitespace (string value, XomParentNode parent)
		{
			this.value = value;
			if (parent != null)
				parent.AppendChild (this);
		}

		public override string Prefix {
			get { return ""; }
		}

		public override string LocalName {
			get { return ""; }
		}

		public override string Namespace {
			get { return ""; }
		}

		public override string Value {
			get { return value; }
			set { this.value += value; }
		}

		public override XPathNodeType NodeType {
			get { return XPathNodeType.Whitespace; }
		}

		internal override void BuildValue (StringBuilder sb)
		{
			sb.Append (value);
		}

		public override int ChildCount { get { return 0; } }

		public override void WriteTo (XmlWriter writer)
		{
			writer.WriteString (Value);
		}
	}

	public class XomSignificantWhitespace : XomNode
	{
		string value;

		public XomSignificantWhitespace (string value)
			: this (value, null)
		{
		}

		public XomSignificantWhitespace (string value, XomParentNode parent)
		{
			this.value = value;
			if (parent != null)
				parent.AppendChild (this);
		}

		public override string Prefix {
			get { return ""; }
		}

		public override string LocalName {
			get { return ""; }
		}

		public override string Namespace {
			get { return ""; }
		}

		public override string Value {
			get { return value; }
			set { this.value += value; }
		}

		public override XPathNodeType NodeType {
			get { return XPathNodeType.SignificantWhitespace; }
		}

		internal override void BuildValue (StringBuilder sb)
		{
			sb.Append (value);
		}

		public override int ChildCount { get { return 0; } }

		public override void WriteTo (XmlWriter writer)
		{
			writer.WriteString (Value);
		}
	}
}
#endif
