#if NET_2_0

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

using XPI = System.Xml.XLinq.XProcessingInstruction;


namespace System.Xml.XLinq
{
	#region XMLdecl, Doctype, Comment, PI

	public class XDeclaration : XNode
	{
		string encoding, standalone, version;

		public XDeclaration (XmlReader reader)
			: this (reader.GetAttribute ("version"),
				reader.GetAttribute ("encoding"),
				reader.GetAttribute ("standalone"))
		{
		}

		public XDeclaration (string version, string encoding, string standalone)
		{
			this.version = version;
			this.encoding = encoding;
			this.standalone = standalone;
		}

		public string Encoding {
			get { return encoding; }
			set { encoding = value; }
		}

		public override XmlNodeType NodeType {
			get { return XmlNodeType.XmlDeclaration; }
		}

		public string Standalone {
			get { return standalone; }
			set { standalone = value; }
		}

		public string Version {
			get { return version; }
			set { version = value; }
		}

		public override void WriteTo (XmlWriter w)
		{
			StringBuilder sb = new StringBuilder ();
			sb.AppendFormat ("version=\"{0}\"", version);
			if (encoding != null)
				sb.AppendFormat (" encoding=\"{0}\"", encoding);
			if (standalone != null)
				sb.AppendFormat (" standalone=\"{0}\"", standalone);
			// "xml" is not allowed PI, but because of nasty
			// XmlWriter API design it must pass.
			w.WriteProcessingInstruction ("xml", sb.ToString ());
		}
	}

	public class XDocumentType : XNode
	{
		string pubid, sysid, intSubset;

		public XDocumentType ()
		{
		}

		public XDocumentType (XmlReader reader)
		{
			if (reader.NodeType != XmlNodeType.DocumentType)
				throw new ArgumentException ();

			pubid = reader.GetAttribute ("PUBLIC");
			sysid = reader.GetAttribute ("SYSTEM");
			intSubset = reader.Value;
		}

		public override XmlNodeType NodeType {
			get { return XmlNodeType.DocumentType; }
		}

		public override void WriteTo (XmlWriter w)
		{
			XDocument doc = Document;
			XElement root = doc.Root;
			if (root != null)
				w.WriteDocType (root.Name.LocalName, pubid, sysid, intSubset);
		}
	}

	public class XProcessingInstruction : XNode
	{
		string name;
		string value;

		public XProcessingInstruction (XmlReader r)
		{
			if (r.NodeType != XmlNodeType.ProcessingInstruction)
				throw new ArgumentException ();

			name = r.Name;
			value = r.Value;
		}

		public XProcessingInstruction (string name, string value)
		{
			this.name = name;
			this.value = value;
		}

		public string Data {
			get { return value; }
			set { this.value = value; }
		}

		public override XmlNodeType NodeType {
			get { return XmlNodeType.ProcessingInstruction; }
		}

		public string Target {
			get { return name; }
			set { name = value; }
		}

		public override void WriteTo (XmlWriter w)
		{
			w.WriteProcessingInstruction (name, value);
		}
	}

	public class XComment : XNode
	{
		string value;

		public XComment (string value)
		{
			this.value = value;
		}

		public XComment (XmlReader r)
		{
			if (r.NodeType != XmlNodeType.Comment)
				throw new ArgumentException ();
			value = r.Value;
		}

		public override XmlNodeType NodeType {
			get { return XmlNodeType.Comment; }
		}

		public string Value {
			get { return value; }
			set { this.value = value; }
		}

		public override bool Equals (object obj)
		{
			XComment c = obj as XComment;
			return c != null && c.value == value;
		}

		public override int GetHashCode ()
		{
			return value.GetHashCode () ^ (int) NodeType;
		}

		public override void WriteTo (XmlWriter w)
		{
			w.WriteComment (value);
		}
	}

	#endregion


	#region CharacterData

	public abstract class XCharacterNode : XNode
	{
		public override bool Equals (object obj)
		{
			XCharacterNode n = obj as XCharacterNode;
			if (n == null)
				return false;
			return n.NodeType == NodeType &&
				ToString () == n.ToString ();
		}

		public override int GetHashCode ()
		{
			return ((int) NodeType) ^ ToString ().GetHashCode ();
		}
	}

	// It is documented to exist, but I use it only in !LIST_BASED mode.
	internal class XText : XCharacterNode
	{
		string value;

		public XText (string value)
		{
			this.value = value;
		}

		public XText (XmlReader r)
		{
			if (r.NodeType != XmlNodeType.Text)
				throw new ArgumentException ();
			value = r.Value;
		}

		public override XmlNodeType NodeType {
			get { return XmlNodeType.Text; }
		}

		public string Value {
			get { return value; }
		}

		public override void WriteTo (XmlWriter w)
		{
			w.WriteString (value);
		}
	}

	public class XCData : XCharacterNode
	{
		string value;

		public XCData (string value)
		{
			this.value = value;
		}

		public XCData (XmlReader r)
		{
			if (r.NodeType != XmlNodeType.CDATA)
				throw new ArgumentException ();
			value = r.Value;
		}

		public override XmlNodeType NodeType {
			get { return XmlNodeType.CDATA; }
		}

		public string Value {
			get { return value; }
		}

		public override void WriteTo (XmlWriter w)
		{
			int start = 0;
			StringBuilder sb = null;
			for (int i = 0; i < value.Length - 2; i++) {
				if (value [i] == ']' && value [i + 1] == ']'
					&& value [i + 2] == '>') {
					if (sb == null)
						sb = new StringBuilder ();
					sb.Append (value, start, i - start);
					sb.Append ("]]&gt;");
					start = i + 3;
				}
			}
			if (start != 0 && start != value.Length)
				sb.Append (value, start, value.Length - start);
			w.WriteCData (sb == null ? value : sb.ToString ());
		}
	}

	#endregion


	#region Tree structures

	public abstract class XContainer : XNode
	{
#if LIST_BASED
		List <object> list = new List <object> ();

		internal object FirstChild {
			get { return list.Count > 0 ? list [0] : null; }
		}

		internal object LastChild {
			get { return list.Count > 0 ? list [list.Count - 1] : null; }
		}
#else
		XNode lastChild;

		internal XNode FirstChild {
			get { return lastChild != null ? lastChild.InternalNext : null; }
		}

		internal XNode LastChild {
			get { return lastChild; }
			set { lastChild = value; }
		}
#endif

		void CheckChildType (object o)
		{
			if (o == null || o is string || o is XNode)
				return;
			if (o is IEnumerable) {
				foreach (object oc in ((IEnumerable) o))
					CheckChildType (oc);
				return;
			}
			else
				throw new ArgumentException ("Invalid child type: " + o.GetType ());
		}

		private void AddAttribute (XAttribute attr)
		{
			if (this is XElement)
				((XElement) this).SetAttributeNode (attr);
			else
				throw new ArgumentException ("Attribute is not allowed here.");
		}

		public void Add (object content)
		{
			if (content == null)
				return;
			if (content is XAttribute) {
				AddAttribute ((XAttribute) content);
				return;
			}
			CheckChildType (content);
#if LIST_BASED
			if (content is XNode)
				((XNode) content).Parent = this;
			list.Add (content);
#else
			XNode n = XUtil.ToNode (content);
			n.Parent = this;
			if (lastChild == null) {
				lastChild = n;
				n.InternalNext = n;
			}
			else {
				XNode firstChild = lastChild != null ? lastChild.InternalNext : null;
				lastChild.UpdateTree (this, n);
				n.UpdateTree (this, firstChild);
			}
#endif
		}

		public void Add (params object [] content)
		{
			for (int i = 0; i < content.Length; i++)
				Add (content [i]);
		}

		public void AddFirst (object content)
		{
			if (content == null)
				return;
			if (content is XAttribute) {
				AddAttribute ((XAttribute) content);
				return;
			}
			CheckChildType (content);
#if LIST_BASED
			if (content is XNode)
				((XNode) content).Parent = this;
			list.Insert (0, content);
#else
			XNode n = XUtil.ToNode (content);
			n.Parent = this;
			if (lastChild == null) {
				lastChild = n;
				n.InternalNext = n;
			}
			else {
				n.UpdateTree (this, lastChild.InternalNext);
				lastChild.UpdateTree (this, n);
			}
#endif
		}

		public void AddFirst (params object [] content)
		{
			for (int i = content.Length - 1; i >= 0; i--)
				AddFirst (content [i]);
		}

#if LIST_BASED
		internal object GetNextSibling (object target)
		{
			int i = list.IndexOf (target);
			return i + 1 == list.Count ? null : list [i + 1];
		}

		internal void InsertBefore (object from, object target)
		{
			if (target is XAttribute) {
				AddAttribute ((XAttribute) target);
				return;
			}
			CheckChildType (target);
			int index = list.IndexOf (from);
			if (target is XNode)
				((XNode) target).Parent = this;
			list.Insert (index, target);
		}

		internal void InsertBefore (object from, params object [] target)
		{
			foreach (object o in target)
				CheckChildType (o);
			if (target.Length == 0)
				return;
			int index = list.IndexOf (from);
			if (index == 0) {
				List <object> tmp = list;
				list = new List <object> (list.Count + target.Length);
				Add (target);
				list.AddRange (tmp);
			} else {
				InsertAfter (list [index - 1], target);
			}
		}

		internal void InsertAfter (object from, object target)
		{
			if (target is XAttribute) {
				AddAttribute ((XAttribute) target);
				return;
			}
			CheckChildType (target);
			int index = list.IndexOf (from);
			if (target is XNode)
				((XNode) target).Parent = this;
			list.Insert (index + 1, target);
		}

		internal void InsertAfter (object from, params object [] target)
		{
			for (int i = 0; i < target.Length; i++) {
				CheckChildType (target [i]);
				InsertAfter (from, target [i]);
				from = target [i];
			}
		}

		internal void RemoveChild (object target)
		{
			list.Remove (target);
		}
#endif

		public IEnumerable <object> Content ()
		{
#if LIST_BASED
			return list;
#else
			return new XChildrenIterator (this);
#endif
		}

		public IEnumerable <T> Content<T> ()
		{
			return new XFilterIterator <T> (Content (), null);
		}

		public IEnumerable <XElement> Descendants ()
		{
			return new XFilterIterator <XElement> (
				new XDescendantIterator <object> (Content ()), null);
		}

		public IEnumerable <XElement> Descendants (XName name)
		{
			return new XFilterIterator <XElement> (
				new XDescendantIterator <object> (
					Content ()), name);
		}

		public IEnumerable <T> Descendants <T> ()
		{
			return new XFilterIterator <T> (
				new XDescendantIterator <object> (Content ()), null);
		}

		public IEnumerable <XElement> Elements ()
		{
			return new XFilterIterator <XElement> (Content (), null);
		}

		public IEnumerable <XElement> Elements (XName name)
		{
			return new XFilterIterator <XElement> (Content (), name);
		}

		public void ReadContentFrom (XmlReader reader)
		{
			if (reader.IsEmptyElement)
				reader.Read ();
			else {
				reader.Read ();
				do {
					if (reader.NodeType == XmlNodeType.EndElement)
						// end of the element.
						break;
					if (reader.NodeType == XmlNodeType.Text)
						Add (reader.Value);
					else
						Add (XNode.ReadFrom (reader));
				} while (reader.Read ());
				reader.Read ();
			}
		}

		public void RemoveContent ()
		{
#if LIST_BASED
			foreach (object o in list)
				if (o is XNode)
					((XNode) o).Parent = null;
#else
			foreach (XNode n in Content ())
				n.Remove ();
#endif
		}

		public void ReplaceContent (object content)
		{
			RemoveContent ();
			Add (content);
		}

		public void ReplaceContent (params object [] content)
		{
			RemoveContent ();
			Add (content);
		}

		public void WriteContentTo (XmlWriter writer)
		{
			foreach (object o in Content ()) {
				if (o is string)
					writer.WriteString ((string) o);
				else if (o is XNode)
					((XNode) o).WriteTo (writer);
				else
					throw new SystemException ("INTERNAL ERROR: list content item was " + o.GetType ());
			}
		}
	}

	public class XDocument : XContainer
	{
		public XDocument ()
		{
		}

		public XDocument (params object [] content)
		{
			Add (content);
		}

		public XDocument (XDocument other)
		{
			foreach (object o in other.Content ())
				Add (XUtil.Clone (o));
		}

		public XDeclaration Declaration {
			get {
				IEnumerator<XDeclaration> e = Content <XDeclaration> ().GetEnumerator ();
				return e.MoveNext () ? e.Current : null;
			}
		}

		public XDocumentType DocumentType {
			get {
				IEnumerator<XDocumentType> e = Content <XDocumentType> ().GetEnumerator ();
				return e.MoveNext () ? e.Current : null;
			}
		}

		public override XmlNodeType NodeType {
			get { return XmlNodeType.Document; }
		}

		public XElement Root {
			get {
				IEnumerator<XElement> e = Elements ().GetEnumerator ();
				return e.MoveNext () ? e.Current : null;
			}
		}

		public static XDocument Load (string uri)
		{
			return Load (uri, false);
		}

		public static XDocument Load (string uri, bool preserveWhitespaces)
		{
			XmlReaderSettings s = new XmlReaderSettings ();
			s.IgnoreWhitespace = !preserveWhitespaces;
			using (XmlReader r = XmlReader.Create (uri, s)) {
				return Load (r);
			}
		}

		public static XDocument Load (TextReader reader)
		{
			return Load (reader, false);
		}

		public static XDocument Load (TextReader reader, bool preserveWhitespaces)
		{
			XmlReaderSettings s = new XmlReaderSettings ();
			s.IgnoreWhitespace = !preserveWhitespaces;
			using (XmlReader r = XmlReader.Create (reader, s)) {
				return Load (r);
			}
		}

		public static XDocument Load (XmlReader reader)
		{
			XDocument doc = new XDocument ();
			if (reader.ReadState == ReadState.Initial)
				reader.Read ();
			for (; !reader.EOF; reader.Read ())
				if (reader.NodeType == XmlNodeType.Text)
					doc.Add (reader.Value);
				else
					doc.Add (XNode.ReadFrom (reader));
			return doc;
		}

		public static XDocument Parse (string s)
		{
			return Parse (s, false);
		}

		public static XDocument Parse (string s, bool preserveWhitespaces)
		{
			return Load (new StringReader (s), preserveWhitespaces);
		}

		public void Save (string filename)
		{
			Save (filename, false);
		}

		public void Save (string filename, bool ignoreWhitespaces)
		{
			XmlWriterSettings s = new XmlWriterSettings ();
			if (ignoreWhitespaces) {
				// hacky!
				s.Indent = true;
				s.IndentChars = String.Empty;
				s.NewLineChars = String.Empty;
			}
			using (XmlWriter w = XmlWriter.Create (filename)) {
				Save (w);
			}
		}

		public void Save (TextWriter tw)
		{
			Save (tw, false);
		}

		public void Save (TextWriter tw, bool ignoreWhitespaces)
		{
			XmlWriterSettings s = new XmlWriterSettings ();
			if (ignoreWhitespaces) {
				// hacky!
				s.Indent = true;
				s.IndentChars = String.Empty;
				s.NewLineChars = String.Empty;
			}
			using (XmlWriter w = XmlWriter.Create (tw)) {
				Save (w);
			}
		}

		public void Save (XmlWriter w)
		{
			WriteTo (w);
		}

		public override void WriteTo (XmlWriter w)
		{
			WriteContentTo (w);
		}
	}

	public class XElement : XContainer
	{
		static IEnumerable <XElement> emptySequence =
			new List <XElement> ();

		public static IEnumerable <XElement> EmptySequence {
			get { return emptySequence; }
		}

		XName name;
		List <XAttribute> attributes;

		public XElement (XElement source)
		{
			name = source.name;
			Add (source.Content<object> ());
		}

		public XElement (XmlReader source)
		{
			if (source.NodeType != XmlNodeType.Element)
				throw new InvalidOperationException ();
			name = XName.Get (source.LocalName, source.NamespaceURI);
			if (source.MoveToFirstAttribute ()) {
				do {
					SetAttribute (XName.Get (source.LocalName, source.NamespaceURI), source.Value);
				} while (source.MoveToNextAttribute ());
				source.MoveToElement ();
			}
		}

		public XElement (XName name)
		{
			this.name = name;
		}

		public XElement (XName name, params object [] contents)
		{
			this.name = name;
			Add (contents);
		}

		internal List <XAttribute> SafeAttributes {
			get {
				if (attributes == null)
					attributes = new List <XAttribute> ();
				return attributes;
			}
		}

		public bool HasAttributes {
			get { return attributes != null && attributes.Count > 0; }
		}

		public bool HasElements {
			get { return Elements ().GetEnumerator ().MoveNext (); }
		}

		public bool IsEmpty {
			get { return !Content <object> ().GetEnumerator ().MoveNext (); }
		}

		public XName Name {
			get { return name; }
		}

		public override XmlNodeType NodeType {
			get { return XmlNodeType.Element; }
		}

		public string Value {
			get {
				StringBuilder sb = null;
				foreach (string s in Content <string> ()) {
					if (sb == null)
						sb = new StringBuilder ();
					sb.Append (s);
				}
				return sb == null ? String.Empty : sb.ToString ();
			}
			set {
				RemoveContent ();
				Add (value);
			}
		}

		IEnumerable <XElement> GetAncestorList (XName name, bool getMeIn)
		{
			List <XElement> list = new List <XElement> ();
			if (getMeIn)
				list.Add (this);
			for (XElement el = Parent as XElement; el != null; el = el.Parent as XElement)
				if (name == null || el.Name == name)
					list.Add (el);
			return list;
		}

		public XAttribute Attribute (XName name)
		{
			if (attributes == null)
				return null;
			foreach (XAttribute a in attributes)
				if (a.Name == name)
					return a;
			return null;
		}

		public IEnumerable <XElement> Ancestors ()
		{
			return GetAncestorList (null, false);
		}

		public IEnumerable <XElement> Ancestors (XName name)
		{
			return GetAncestorList (name, false);
		}

		public IEnumerable <XAttribute> Attributes ()
		{
			return attributes != null ? attributes : XAttribute.EmptySequence;
		}

		public IEnumerable <XAttribute> Attributes (XName name)
		{
			XAttribute a = Attribute (name);
			if (a == null)
				return XAttribute.EmptySequence;
			List <XAttribute> list = new List <XAttribute> ();
			list.Add (a);
			return list;
		}

		public override bool Equals (object obj)
		{
			XElement e = obj as XElement;
			if (e == null || name != e.name)
				return false;
			IEnumerator e1 = Content ().GetEnumerator ();
			IEnumerator e2 = e.Content ().GetEnumerator ();
			do {
				if (e1.MoveNext ()) {
					if (e2.MoveNext ()) {
						if (!e1.Equals (e2.Current))
							return false;
					}
					else
						return false;
				}
				else if (e2.MoveNext ())
					return false;
			} while (true);
		}

		public override int GetHashCode ()
		{
			int i = name.GetHashCode ();
			foreach (XAttribute a in Attributes ())
				i ^= a.GetHashCode ();
			foreach (object o in Content ())
				i ^= o.GetHashCode ();
			return i;
		}

		// Only XAttribute.set_Parent() can invoke this.
		internal void InternalAppendAttribute (XAttribute attr)
		{
			if (attr.Parent != this)
				throw new SystemException ("INTERNAL ERROR: should not happen.");
			SafeAttributes.Add (attr);
		}

		internal void InternalRemoveAttribute (XAttribute attr)
		{
			if (attr.Parent != this)
				throw new SystemException ("INTERNAL ERROR: should not happen.");
			attributes.Remove (attr);
		}

		public static XElement Load (string uri)
		{
			return Load (uri, false);
		}

		public static XElement Load (string uri, bool preserveWhitespaces)
		{
			XmlReaderSettings s = new XmlReaderSettings ();
			s.IgnoreWhitespace = !preserveWhitespaces;
			using (XmlReader r = XmlReader.Create (uri, s)) {
				return Load (r);
			}
		}

		public static XElement Load (TextReader tr)
		{
			return Load (tr, false);
		}

		public static XElement Load (TextReader tr, bool preserveWhitespaces)
		{
			XmlReaderSettings s = new XmlReaderSettings ();
			s.IgnoreWhitespace = !preserveWhitespaces;
			using (XmlReader r = XmlReader.Create (tr, s)) {
				return Load (r);
			}
		}

		public static XElement Load (XmlReader r)
		{
			XElement e = new XElement (r);
			e.ReadContentFrom (r);
			return e;
		}

		public static explicit operator bool (XElement e)
		{
			return e.Value == "true";
		}

		// FIXME: similar operator overloads should go here.

		public static XElement Parse (string s)
		{
			return Parse (s, false);
		}

		public static XElement Parse (string s, bool preserveWhitespaces)
		{
			return Load (new StringReader (s), preserveWhitespaces);
		}

		public void RemoveAll ()
		{
			RemoveAttributes ();
			RemoveContent ();
		}

		public void RemoveAttributes ()
		{
			if (attributes != null)
				foreach (XAttribute a in attributes)
					a.Parent = null;
			attributes = null;
		}

		public void Save (string filename)
		{
			Save (filename, false);
		}

		public void Save (string filename, bool ignoreWhitespaces)
		{
			XmlWriterSettings s = new XmlWriterSettings ();
			if (ignoreWhitespaces) {
				// hacky!
				s.Indent = true;
				s.IndentChars = String.Empty;
				s.NewLineChars = String.Empty;
			}
			using (XmlWriter w = XmlWriter.Create (filename)) {
				Save (w);
			}
		}

		public void Save (TextWriter tw)
		{
			Save (tw, false);
		}

		public void Save (TextWriter tw, bool ignoreWhitespaces)
		{
			XmlWriterSettings s = new XmlWriterSettings ();
			if (ignoreWhitespaces) {
				// hacky!
				s.Indent = true;
				s.IndentChars = String.Empty;
				s.NewLineChars = String.Empty;
			}
			using (XmlWriter w = XmlWriter.Create (tw)) {
				Save (w);
			}
		}

		public void Save (XmlWriter w)
		{
			WriteTo (w);
		}

		public IEnumerable <XElement> SelfAndAncestors ()
		{
			return GetAncestorList (null, true);
		}

		public IEnumerable <XElement> SelfAndAncestors (XName name)
		{
			return GetAncestorList (name, true);
		}

		public IEnumerable <XElement> SelfAndDescendants ()
		{
			List <XElement> list = new List <XElement> ();
			list.Add (this);
			list.AddRange (Descendants ());
			return list;
		}

		public IEnumerable <XElement> SelfAndDescendants (XName name)
		{
			List <XElement> list = new List <XElement> ();
			if (name == this.name)
				list.Add (this);
			list.AddRange (Descendants (name));
			return list;
		}

		public IEnumerable <T> SelfAndDescendants <T> ()
		{
			List <T> list = new List <T> ();
			// since "this" is regarded as never castable to T,
			// we need to anonymize this.
			object o = this;
			if (o is T)
				list.Add ((T) o);
			list.AddRange (Descendants <T> ());
			return list;
		}

		public void SetAttribute (XName name, object value)
		{
			XAttribute a = Attribute (name);
			if (value == null) {
				if (a != null)
					a.Remove ();
			} else {
				if (a == null) {
					new XAttribute (name, value).Parent = this;
				}
				else
					a.Value = XUtil.ToString (value);
			}
		}

		public void SetAttributeNode (XAttribute attr)
		{
			foreach (XAttribute a in Attributes (attr.Name))
				a.Remove ();
			attr.Parent = this;
		}

		public void SetElement (XName name, object value)
		{
			IEnumerator <XElement> en = Elements (name).GetEnumerator ();
			XElement e = en.MoveNext () ? en.Current : null;
			if (value == null) {
				if (e != null)
					e.Remove ();
			} else {
				if (e == null)
					Add (new XElement (name, value));
				else
					e.Value = XUtil.ToString (value);
			}
		}

		public override void WriteTo (XmlWriter w)
		{
			w.WriteStartElement (name.LocalName, name.NamespaceName);

			if (attributes != null) {
				foreach (XAttribute a in attributes) {
					if (a.Name.NamespaceName == XUtil.XmlnsNamespace && a.Name.LocalName != String.Empty)
						w.WriteAttributeString ("xmlns", a.Name.LocalName, XUtil.XmlnsNamespace, a.Value);
					else
						w.WriteAttributeString (a.Name.LocalName, a.Name.NamespaceName, a.Value);
				}
			}

			WriteContentTo (w);

			w.WriteEndElement ();
		}
	}

	public abstract class XNode
	{
		XContainer parent;
#if !LIST_BASED
		XNode next;
#endif

		public XDocument Document {
			get {
				XContainer e = Parent;
				if (e == null)
					return null;
				do {
					XContainer p = e.Parent;
					if (p == null)
						return e as XDocument; // might be XElement
				} while (true);
			}
		}

		public abstract XmlNodeType NodeType { get; }

#if LIST_BASED
		internal object
#else
		internal XNode
#endif
		PreviousSibling {
			get {
				if (parent == null || object.ReferenceEquals (parent.FirstChild, this))
					return null;
#if LIST_BASED
				IEnumerator e = Parent.Content ().GetEnumerator ();
				for (object o = null; e.MoveNext (); o = e.Current)
					if (object.ReferenceEquals (e.Current, this))
						return o;
				return null;
#else
				for (XNode n = Parent.LastChild.next; n != null; n = n.next)
					if (n.next == this)
						return n;
				return null;
#endif
			}
		}

#if LIST_BASED
		internal object
#else
		internal XNode
#endif
		NextSibling {
			get {
				if (parent == null || object.ReferenceEquals (parent.LastChild, this))
					return null;
#if LIST_BASED
				return parent.GetNextSibling (this);
#else
				return next;
#endif
			}
		}

#if !LIST_BASED
		internal XNode InternalNext {
			get { return next; }
			set { next = value; }
		}
#endif

		public XContainer Parent {
			get { return parent; }
			internal set { parent = value; }
		}

#if !LIST_BASED
		internal void UpdateTree (XContainer parent, XNode next)
		{
			this.parent = parent;
			this.next = next;
		}
#endif

		public string Xml {
			get {
				StringWriter sw = new StringWriter ();
				XmlWriter xw = XmlWriter.Create (sw);
				WriteTo (xw);
				xw.Close ();
				return sw.ToString ();
			}
		}

		public void AddAfterThis (object content)
		{
			if (Parent == null)
				throw new InvalidOperationException ();
#if LIST_BASED
			Parent.InsertAfter (this, content);
#else
			XNode n = XUtil.ToNode (content);
			n.parent = Parent;
			n.next = next;
			next = n;
			if (Parent.LastChild == null || object.ReferenceEquals (Parent.LastChild, this))
				Parent.LastChild = n;
#endif
		}

		public void AddAfterThis (params object [] content)
		{
			if (Parent == null)
				throw new InvalidOperationException ();
#if LIST_BASED
			Parent.InsertAfter (this, content);
#else
			foreach (object o in new XFilterIterator <object> (content, null))
				AddAfterThis (o);
#endif
		}

		public void AddBeforeThis (object content)
		{
			if (Parent == null)
				throw new InvalidOperationException ();
#if LIST_BASED
			Parent.InsertBefore (this, content);
#else
			XNode n = XUtil.ToNode (content);
			n.parent = Parent;
			n.next = this;
			XNode p = PreviousSibling;
			if (p != null)
				PreviousSibling.next = n;
			else
				Parent.LastChild.next = this;
			if (Parent.LastChild == null || object.ReferenceEquals (Parent.LastChild, this))
				Parent.LastChild = n;
#endif
		}

		public void AddBeforeThis (params object [] content)
		{
			if (Parent == null)
				throw new InvalidOperationException ();
#if LIST_BASED
			Parent.InsertBefore (this, content);
#else
			foreach (object o in new XFilterIterator <object> (content, null))
				AddBeforeThis (o);
#endif
		}

		public static XNode ReadFrom (XmlReader r)
		{
			switch (r.NodeType) {
			case XmlNodeType.Element:
				return XElement.Load (r);
			case XmlNodeType.Text:
				throw new InvalidOperationException ();
			case XmlNodeType.CDATA:
				return new XCData (r);
			case XmlNodeType.ProcessingInstruction:
				return new XPI (r);
			case XmlNodeType.Comment:
				return new XComment (r);
			case XmlNodeType.XmlDeclaration:
				return new XDeclaration (r);
			case XmlNodeType.DocumentType:
				return new XDocumentType (r);
			default:
				throw new NotSupportedException ();
			}
		}

		public void Remove ()
		{
#if LIST_BASED
			Parent.RemoveChild (this);
#else
			PreviousSibling.next = NextSibling;
			parent = null;
#endif
		}

		public override string ToString ()
		{
			return Xml;
		}

		public abstract void WriteTo (XmlWriter w);
	}

	#endregion
}

#endif
