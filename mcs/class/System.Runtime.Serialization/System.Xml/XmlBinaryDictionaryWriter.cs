//
// XmlBinaryDictionaryWriter.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2005, 2007 Novell, Inc.  http://www.novell.com
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
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using BF = System.Xml.XmlBinaryFormat;

namespace System.Xml
{
	internal partial class XmlBinaryDictionaryWriter : XmlDictionaryWriter
	{
		class MyBinaryWriter : BinaryWriter
		{
			public MyBinaryWriter (Stream s)
				: base (s)
			{
			}

			public void WriteFlexibleInt (int value)
			{
				Write7BitEncodedInt (value);
			}
		}

		#region Fields
		MyBinaryWriter original, writer, buffer_writer;
		IXmlDictionary dict_ext;
		XmlDictionary dict_int = new XmlDictionary ();
		XmlBinaryWriterSession session;
		bool owns_stream;
		Encoding utf8Enc = new UTF8Encoding ();
		MemoryStream buffer = new MemoryStream ();

		const string XmlNamespace = "http://www.w3.org/XML/1998/namespace";
		const string XmlnsNamespace = "http://www.w3.org/2000/xmlns/";

		WriteState state = WriteState.Start;
		bool open_start_element = false;
		// transient current node info
		List<KeyValuePair<string,object>> namespaces = new List<KeyValuePair<string,object>> ();
		string xml_lang = null;
		XmlSpace xml_space = XmlSpace.None;
		int ns_index = 0;
		// stacked info
		Stack<int> ns_index_stack = new Stack<int> ();
		Stack<string> xml_lang_stack = new Stack<string> ();
		Stack<XmlSpace> xml_space_stack = new Stack<XmlSpace> ();
		Stack<string> element_ns_stack = new Stack<string> ();
		string element_ns = String.Empty;
		int element_count;
		string element_prefix; // only meaningful at Element state
		// current attribute info
		string attr_value;
		string current_attr_prefix;
		object current_attr_name, current_attr_ns;
		bool attr_typed_value;
		SaveTarget save_target;

		enum SaveTarget {
			None,
			Namespaces,
			XmlLang,
			XmlSpace
		}

		// XmlWriterSettings support

		#endregion

		#region Constructors

		public XmlBinaryDictionaryWriter (Stream stream,
			IXmlDictionary dictionary,
			XmlBinaryWriterSession session, bool ownsStream)
		{
			if (dictionary == null)
				dictionary = new XmlDictionary ();
			if (session == null)
				session = new XmlBinaryWriterSession ();

			original = new MyBinaryWriter (stream);
			this.writer = original;
			buffer_writer = new MyBinaryWriter (buffer);
			this.dict_ext = dictionary;
			this.session = session;
			owns_stream = ownsStream;

			AddNamespace ("xml", "http://www.w3.org/XML/1998/namespace");
			AddNamespace ("xml", "http://www.w3.org/2000/xmlns/");
			ns_index = 2;
		}

		#endregion

		#region Properties

		public override WriteState WriteState {
			get { return state; }
		}
		
		public override string XmlLang {
			get { return xml_lang; }
		}

		public override XmlSpace XmlSpace {
			get { return xml_space; }
		}

		#endregion

		#region Methods

		private void AddMissingElementXmlns ()
		{
			// push new namespaces to manager.
			for (int i = ns_index; i < namespaces.Count; i++) {
				var ent = namespaces [i];
				string prefix = (string) ent.Key;
				string ns = ent.Value as string;
				XmlDictionaryString dns = ent.Value as XmlDictionaryString;
				if (ns != null) {
					if (prefix.Length > 0) {
						writer.Write (BF.PrefixNSString);
						writer.Write (prefix);
					}
					else
						writer.Write (BF.DefaultNSString);
					writer.Write (ns);
				} else {
					if (prefix.Length > 0) {
						writer.Write (BF.PrefixNSIndex);
						writer.Write (prefix);
					}
					else
						writer.Write (BF.DefaultNSIndex);
					WriteDictionaryIndex (dns);
				}
			}
			ns_index = namespaces.Count;
		}

		private void CheckState ()
		{
			if (state == WriteState.Closed) {
				throw new InvalidOperationException ("The Writer is closed.");
			}
		}

		void ProcessStateForContent ()
		{
			CheckState ();

			if (state == WriteState.Element)
				CloseStartElement ();

			ProcessPendingBuffer (false, false);
			if (state != WriteState.Attribute)
				writer = buffer_writer;
		}

		void ProcessTypedValue ()
		{
			ProcessStateForContent ();
			if (state == WriteState.Attribute) {
				if (attr_typed_value)
					throw new InvalidOperationException (String.Format ("A typed value for the attribute '{0}' in namespace '{1}' was already written", current_attr_name, current_attr_ns));
				attr_typed_value = true;
			}
		}

		void ProcessPendingBuffer (bool last, bool endElement)
		{
			if (buffer.Position > 0) {
				byte [] arr = buffer.GetBuffer ();
				if (endElement)
					arr [0]++;
				original.Write (arr, 0, (int) buffer.Position);
				buffer.SetLength (0);
			}
			if (last)
				writer = original;
		}

		public override void Close ()
		{
			CloseOpenAttributeAndElements ();

			if (owns_stream)
				writer.Close ();
			else if (state != WriteState.Closed)
				writer.Flush ();
			state = WriteState.Closed;
		}

		private void CloseOpenAttributeAndElements ()
		{
			CloseStartElement ();

			 while (element_count > 0)
				WriteEndElement ();
		}

		private void CloseStartElement ()
		{
			if (!open_start_element)
				return;

			if (state == WriteState.Attribute)
				WriteEndAttribute ();

			AddMissingElementXmlns ();

			state = WriteState.Content;
			open_start_element = false;
		}

		public override void Flush ()
		{
			writer.Flush ();
		}

		public override string LookupPrefix (string ns)
		{
			if (ns == null || ns == String.Empty)
				throw new ArgumentException ("The Namespace cannot be empty.");

			var de = namespaces.LastOrDefault (i => i.Value.ToString () == ns);
			return de.Key; // de is KeyValuePair and its default key is null.
		}

		public override void WriteBase64 (byte[] buffer, int index, int count)
		{
			if (count < 0)
				throw new IndexOutOfRangeException ("Negative count");
			ProcessStateForContent ();

			if (count < 0x100) {
				writer.Write (BF.Bytes8);
				writer.Write ((byte) count);
				writer.Write (buffer, index, count);
			} else if (count < 0x10000) {
				writer.Write (BF.Bytes8);
				writer.Write ((ushort) count);
				writer.Write (buffer, index, count);
			} else {
				writer.Write (BF.Bytes32);
				writer.Write (count);
				writer.Write (buffer, index, count);
			}
		}

		public override void WriteCData (string text)
		{
			if (text.IndexOf ("]]>") >= 0)
				throw new ArgumentException ("CDATA section cannot contain text \"]]>\".");

			ProcessStateForContent ();

			WriteTextBinary (text);
		}

		public override void WriteCharEntity (char ch)
		{
			WriteChars (new char [] {ch}, 0, 1);
		}

		public override void WriteChars (char[] buffer, int index, int count)
		{
			ProcessStateForContent ();

			int blen = Encoding.UTF8.GetByteCount (buffer, index, count);

			if (blen == 0)
				writer.Write (BF.EmptyText);
			else if (count == 1 && buffer [0] == '0')
				writer.Write (BF.Zero);
			else if (count == 1 && buffer [0] == '1')
				writer.Write (BF.One);
			else if (blen < 0x100) {
				writer.Write (BF.Chars8);
				writer.Write ((byte) blen);
				writer.Write (buffer, index, count);
			} else if (blen < 0x10000) {
				writer.Write (BF.Chars16);
				writer.Write ((ushort) blen);
				writer.Write (buffer, index, count);
			} else {
				writer.Write (BF.Chars32);
				writer.Write (blen);
				writer.Write (buffer, index, count);
			}
		}

		public override void WriteComment (string text)
		{
			if (text.EndsWith("-"))
				throw new ArgumentException ("An XML comment cannot contain \"--\" inside.");
			else if (text.IndexOf("--") > 0)
				throw new ArgumentException ("An XML comment cannot end with \"-\".");

			ProcessStateForContent ();

			if (state == WriteState.Attribute)
				throw new InvalidOperationException ("Comment node is not allowed inside an attribute");

			writer.Write (BF.Comment);
			writer.Write (text);
		}

		public override void WriteDocType (string name, string pubid, string sysid, string subset)
		{
			throw new NotSupportedException ("This XmlWriter implementation does not support document type.");
		}

		public override void WriteEndAttribute ()
		{
			if (state != WriteState.Attribute)
				throw new InvalidOperationException("Token EndAttribute in state Start would result in an invalid XML document.");

			CheckState ();

			if (attr_value == null)
				attr_value = String.Empty;

			switch (save_target) {
			case SaveTarget.XmlLang:
				xml_lang = attr_value;
				goto default;
			case SaveTarget.XmlSpace:
				switch (attr_value) {
				case "preserve":
					xml_space = XmlSpace.Preserve;
					break;
				case "default":
					xml_space = XmlSpace.Default;
					break;
				default:
					throw new ArgumentException (String.Format ("Invalid xml:space value: '{0}'", attr_value));
				}
				goto default;
			case SaveTarget.Namespaces:
				if (current_attr_name.ToString ().Length > 0 && attr_value.Length == 0)
					throw new ArgumentException ("Cannot use prefix with an empty namespace.");

				AddNamespaceChecked (current_attr_name.ToString (), attr_value);
				break;
			default:
				if (!attr_typed_value)
					WriteTextBinary (attr_value);
				break;
			}

			if (current_attr_prefix.Length > 0 && save_target != SaveTarget.Namespaces)
				AddNamespaceChecked (current_attr_prefix, current_attr_ns);

			state = WriteState.Element;
			current_attr_prefix = null;
			current_attr_name = null;
			current_attr_ns = null;
			attr_value = null;
			attr_typed_value = false;
		}

		public override void WriteEndDocument ()
		{
			CloseOpenAttributeAndElements ();

			switch (state) {
			case WriteState.Start:
				throw new InvalidOperationException ("Document has not started.");
			case WriteState.Prolog:
				throw new ArgumentException ("This document does not have a root element.");
			}

			state = WriteState.Start;
		}

		bool SupportsCombinedEndElementSupport (byte operation)
		{
			switch (operation) {
			case BF.Comment:
				return false;
			}
			return true;
		}

		public override void WriteEndElement ()
		{
			if (element_count-- == 0)
				throw new InvalidOperationException("There was no XML start tag open.");

			if (state == WriteState.Attribute)
				WriteEndAttribute ();

			// Comment+EndElement does not exist
			bool needExplicitEndElement = buffer.Position == 0 || !SupportsCombinedEndElementSupport (buffer.GetBuffer () [0]);
			ProcessPendingBuffer (true, !needExplicitEndElement);
			CheckState ();
			AddMissingElementXmlns ();

			if (needExplicitEndElement)
				writer.Write (BF.EndElement);

			element_ns = element_ns_stack.Pop ();
			xml_lang = xml_lang_stack.Pop ();
			xml_space = xml_space_stack.Pop ();
			int cur = namespaces.Count;
			ns_index = ns_index_stack.Pop ();
			namespaces.RemoveRange (ns_index, cur - ns_index);
			open_start_element = false;

			Depth--;
		}

		public override void WriteEntityRef (string name)
		{
			throw new NotSupportedException ("This XmlWriter implementation does not support entity references.");
		}

		public override void WriteFullEndElement ()
		{
			WriteEndElement ();
		}

		public override void WriteProcessingInstruction (string name, string text)
		{
			if (name != "xml")
				throw new ArgumentException ("Processing instructions are not supported. ('xml' is allowed for XmlDeclaration; this is because of design problem of ECMA XmlWriter)");
			// Otherwise, silently ignored. WriteStartDocument()
			// is still callable after this method(!)
		}

		public override void WriteQualifiedName (XmlDictionaryString local, XmlDictionaryString ns)
		{
			string prefix = namespaces.LastOrDefault (i => i.Value.ToString () == ns.ToString ()).Key;
			bool use_dic = prefix != null;
			if (prefix == null)
				prefix = LookupPrefix (ns.Value);
			if (prefix == null)
				throw new ArgumentException (String.Format ("Namespace URI '{0}' is not bound to any of the prefixes", ns));

			ProcessTypedValue ();

			if (use_dic && prefix.Length == 1) {
				writer.Write (BF.QNameIndex);
				writer.Write ((byte) (prefix [0] - 'a'));
				WriteDictionaryIndex (local);
			} else {
				// QNameIndex is not applicable.
				WriteString (prefix);
				WriteString (":");
				WriteString (local);
			}
		}

		public override void WriteRaw (string data)
		{
			WriteString (data);
		}

		public override void WriteRaw (char[] buffer, int index, int count)
		{
			WriteChars (buffer, index, count);
		}

		void CheckStateForAttribute ()
		{
			CheckState ();

			if (state != WriteState.Element)
				throw new InvalidOperationException ("Token StartAttribute in state " + WriteState + " would result in an invalid XML document.");
		}

		string CreateNewPrefix ()
		{
			return CreateNewPrefix (String.Empty);
		}
		
		string CreateNewPrefix (string p)
		{
			for (char c = 'a'; c <= 'z'; c++)
				if (!namespaces.Any (iter => iter.Key == p + c))
					return p + c;
			for (char c = 'a'; c <= 'z'; c++) {
				var s = CreateNewPrefix (c.ToString ());
				if (s != null)
					return s;
			}
			throw new InvalidOperationException ("too many prefix population");
		}

		bool CollectionContains (ICollection col, string value)
		{
			foreach (string v in col)
				if (v == value)
					return true;
			return false;
		}

		void ProcessStartAttributeCommon (ref string prefix, string localName, string ns, object nameObj, object nsObj)
		{
			// dummy prefix is created here, while the caller
			// still uses empty string as the prefix there.
			if (prefix.Length == 0 && ns.Length > 0) {
				prefix = LookupPrefix (ns);
				// Not only null but also ""; when the returned
				// string is empty, then it still needs a dummy
				// prefix, since default namespace does not
				// apply to attribute
				if (String.IsNullOrEmpty (prefix))
					prefix = CreateNewPrefix ();
			}
			else if (prefix.Length > 0 && ns.Length == 0) {
				switch (prefix) {
				case "xml":
					nsObj = ns = XmlNamespace;
					break;
				case "xmlns":
					nsObj = ns = XmlnsNamespace;
					break;
				default:
					throw new ArgumentException ("Cannot use prefix with an empty namespace.");
				}
			}
			// here we omit such cases that it is used for writing
			// namespace-less xml, unlike XmlTextWriter.
			if (prefix == "xmlns" && ns != XmlnsNamespace)
				throw new ArgumentException (String.Format ("The 'xmlns' attribute is bound to the reserved namespace '{0}'", XmlnsNamespace));

			CheckStateForAttribute ();

			state = WriteState.Attribute;

			save_target = SaveTarget.None;
			switch (prefix) {
			case "xml":
				// MS.NET looks to allow other names than 
				// lang and space (e.g. xml:link, xml:hack).
				ns = XmlNamespace;
				switch (localName) {
				case "lang":
					save_target = SaveTarget.XmlLang;
					break;
				case "space":
					save_target = SaveTarget.XmlSpace;
					break;
				}
				break;
			case "xmlns":
				save_target = SaveTarget.Namespaces;
				break;
			}

			current_attr_prefix = prefix;
			current_attr_name = nameObj;
			current_attr_ns = nsObj;
		}

		public override void WriteStartAttribute (string prefix, string localName, string ns)
		{
			if (prefix == null)
				prefix = String.Empty;
			if (ns == null)
				ns = String.Empty;
			if (localName == "xmlns" && prefix.Length == 0) {
				prefix = "xmlns";
				localName = String.Empty;
			}

			ProcessStartAttributeCommon (ref prefix, localName, ns, localName, ns);

			// for namespace nodes we don't write attribute node here.
			if (save_target == SaveTarget.Namespaces)
				return;

			byte op = prefix.Length == 1 && 'a' <= prefix [0] && prefix [0] <= 'z' ?
				  (byte) (prefix [0] - 'a' + BF.PrefixNAttrStringStart) :
				  prefix.Length == 0 ? BF.AttrString :
				  BF.AttrStringPrefix;

			if (BF.PrefixNAttrStringStart <= op && op <= BF.PrefixNAttrStringEnd) {
				writer.Write (op);
				writer.Write (localName);
			} else {
				writer.Write (op);
				if (prefix.Length > 0)
					writer.Write (prefix);
				writer.Write (localName);
			}
		}

		public override void WriteStartDocument ()
		{
			WriteStartDocument (false);
		}

		public override void WriteStartDocument (bool standalone)
		{
			if (state != WriteState.Start)
				throw new InvalidOperationException("WriteStartDocument should be the first call.");

			CheckState ();

			// write nothing to stream.

			state = WriteState.Prolog;
		}

		void PrepareStartElement ()
		{
			ProcessPendingBuffer (true, false);
			CheckState ();
			CloseStartElement ();

			Depth++;

			element_ns_stack.Push (element_ns);
			xml_lang_stack.Push (xml_lang);
			xml_space_stack.Push (xml_space);
			ns_index_stack.Push (ns_index);
		}

		public override void WriteStartElement (string prefix, string localName, string ns)
		{
			PrepareStartElement ();

			if ((prefix != null && prefix != String.Empty) && ((ns == null) || (ns == String.Empty)))
				throw new ArgumentException ("Cannot use a prefix with an empty namespace.");

			if (ns == null)
				ns = String.Empty;
			if (ns == String.Empty)
				prefix = String.Empty;
			if (prefix == null)
				prefix = String.Empty;

			byte op = prefix.Length == 1 && 'a' <= prefix [0] && prefix [0] <= 'z' ?
				  (byte) (prefix [0] - 'a' + BF.PrefixNElemStringStart) :
				  prefix.Length == 0 ? BF.ElemString :
				  BF.ElemStringPrefix;

			if (BF.PrefixNElemStringStart <= op && op <= BF.PrefixNElemStringEnd) {
				writer.Write (op);
				writer.Write (localName);
			} else {
				writer.Write (op);
				if (prefix.Length > 0)
					writer.Write (prefix);
				writer.Write (localName);
			}

			OpenElement (prefix, ns);
		}

		void OpenElement (string prefix, object nsobj)
		{
			string ns = nsobj.ToString ();

			state = WriteState.Element;
			open_start_element = true;
			element_prefix = prefix;
			element_count++;
			element_ns = nsobj.ToString ();

			if (element_ns != String.Empty && LookupPrefix (element_ns) != prefix)
				AddNamespace (prefix, nsobj);
		}

		void AddNamespace (string prefix, object nsobj)
		{
			namespaces.Add (new KeyValuePair<string,object> (prefix, nsobj));
		}

		void CheckIfTextAllowed ()
		{
			switch (state) {
			case WriteState.Start:
			case WriteState.Prolog:
				throw new InvalidOperationException ("Token content in state Prolog would result in an invalid XML document.");
			}
		}

		public override void WriteString (string text)
		{
			if (text == null)
				throw new ArgumentNullException ("text");
			CheckIfTextAllowed ();

			if (text == null)
				text = String.Empty;

			ProcessStateForContent ();

			if (state == WriteState.Attribute)
				attr_value += text;
			else
				WriteTextBinary (text);
		}

		public override void WriteString (XmlDictionaryString text)
		{
			if (text == null)
				throw new ArgumentNullException ("text");
			CheckIfTextAllowed ();

			if (text == null)
				text = XmlDictionaryString.Empty;

			ProcessStateForContent ();

			if (state == WriteState.Attribute)
				attr_value += text.Value;
			else if (text.Equals (XmlDictionary.Empty))
				writer.Write (BF.EmptyText);
			else {
				writer.Write (BF.TextIndex);
				WriteDictionaryIndex (text);
			}
		}

		public override void WriteSurrogateCharEntity (char lowChar, char highChar)
		{
			WriteChars (new char [] {highChar, lowChar}, 0, 2);
		}

		public override void WriteWhitespace (string ws)
		{
			for (int i = 0; i < ws.Length; i++) {
				switch (ws [i]) {
				case ' ': case '\t': case '\r': case '\n':
					continue;
				default:
					throw new ArgumentException ("Invalid Whitespace");
				}
			}

			ProcessStateForContent ();

			WriteTextBinary (ws);
		}

		public override void WriteXmlnsAttribute (string prefix, string namespaceUri)
		{
			if (namespaceUri == null)
				throw new ArgumentNullException ("namespaceUri");

			if (String.IsNullOrEmpty (prefix))
				prefix = CreateNewPrefix ();

			CheckStateForAttribute ();

			AddNamespaceChecked (prefix, namespaceUri);

			state = WriteState.Element;
		}

		void AddNamespaceChecked (string prefix, object ns)
		{
			switch (ns.ToString ()) {
			case XmlnsNamespace:
			case XmlNamespace:
				return;
			}

			if (prefix == null)
				throw new InvalidOperationException ();
			var o = namespaces.LastOrDefault (i => i.Key == prefix);
			if (o.Key != null) { // i.e. exists
				if (o.Value.ToString () != ns.ToString ()) {
					if (namespaces.LastIndexOf (o) >= ns_index)
						throw new ArgumentException (String.Format ("The prefix '{0}' is already mapped to another namespace URI '{1}' in this element scope and cannot be mapped to '{2}'", prefix ?? "(null)", o.Value ?? "(null)", ns.ToString ()));
					else
						AddNamespace  (prefix, ns);
				}
			}
			else
				AddNamespace  (prefix, ns);
		}

		#region DictionaryString

		void WriteDictionaryIndex (XmlDictionaryString ds)
		{
			XmlDictionaryString ds2;
			bool isSession = false;
			int idx = ds.Key;
			if (ds.Dictionary != dict_ext) {
				isSession = true;
				if (dict_int.TryLookup (ds.Value, out ds2))
					ds = ds2;
				if (!session.TryLookup (ds, out idx))
					session.TryAdd (dict_int.Add (ds.Value), out idx);
			}
			if (idx >= 0x80) {
				writer.Write ((byte) (0x80 + ((idx % 0x80) << 1) + (isSession ? 1 : 0)));
				writer.Write ((byte) ((byte) (idx / 0x80) << 1));
			}
			else
				writer.Write ((byte) (((idx % 0x80) << 1) + (isSession ? 1 : 0)));
		}

		public override void WriteStartElement (string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri)
		{
			PrepareStartElement ();

			if (prefix == null)
				prefix = String.Empty;

			byte op = prefix.Length == 1 && 'a' <= prefix [0] && prefix [0] <= 'z' ?
				  (byte) (prefix [0] - 'a' + BF.PrefixNElemIndexStart) :
				  prefix.Length == 0 ? BF.ElemIndex :
				  BF.ElemIndexPrefix;

			if (BF.PrefixNElemIndexStart <= op && op <= BF.PrefixNElemIndexEnd) {
				writer.Write (op);
				WriteDictionaryIndex (localName);
			} else {
				writer.Write (op);
				if (prefix.Length > 0)
					writer.Write (prefix);
				WriteDictionaryIndex (localName);
			}

			OpenElement (prefix, namespaceUri);
		}

		public override void WriteStartAttribute (string prefix, XmlDictionaryString localName, XmlDictionaryString ns)
		{
			if (localName == null)
				throw new ArgumentNullException ("localName");
			if (prefix == null)
				prefix = String.Empty;
			if (ns == null)
				ns = XmlDictionaryString.Empty;
			if (localName.Value == "xmlns" && prefix.Length == 0) {
				prefix = "xmlns";
				localName = XmlDictionaryString.Empty;
			}

			ProcessStartAttributeCommon (ref prefix, localName.Value, ns.Value, localName, ns);

			if (save_target == SaveTarget.Namespaces)
				return;

			if (prefix.Length == 1 && 'a' <= prefix [0] && prefix [0] <= 'z') {
				writer.Write ((byte) (prefix [0] - 'a' + BF.PrefixNAttrIndexStart));
				WriteDictionaryIndex (localName);
			} else {
				byte op = ns.Value.Length == 0 ? BF.AttrIndex :BF.AttrIndexPrefix;
				// Write to Stream
				writer.Write (op);
				if (prefix.Length > 0)
					writer.Write (prefix);
				WriteDictionaryIndex (localName);
			}
		}

		public override void WriteXmlnsAttribute (string prefix, XmlDictionaryString namespaceUri)
		{
			if (namespaceUri == null)
				throw new ArgumentNullException ("namespaceUri");

			if (String.IsNullOrEmpty (prefix))
				prefix = CreateNewPrefix ();

			CheckStateForAttribute ();

			AddNamespaceChecked (prefix, namespaceUri);

			state = WriteState.Element;
		}
		#endregion

		#region WriteValue
		public override void WriteValue (bool value)
		{
			ProcessTypedValue ();
			writer.Write ((byte) (value ? BF.BoolTrue : BF.BoolFalse));
		}

		public override void WriteValue (int value)
		{
			WriteValue ((long) value);
		}

		public override void WriteValue (long value)
		{
			ProcessTypedValue ();

			if (value == 0)
				writer.Write (BF.Zero);
			else if (value == 1)
				writer.Write (BF.One);
			else if (value < 0 || value > uint.MaxValue) {
				writer.Write (BF.Int64);
				for (int i = 0; i < 8; i++) {
					writer.Write ((byte) (value & 0xFF));
					value >>= 8;
				}
			} else if (value <= byte.MaxValue) {
				writer.Write (BF.Int8);
				writer.Write ((byte) value);
			} else if (value <= short.MaxValue) {
				writer.Write (BF.Int16);
				writer.Write ((byte) (value & 0xFF));
				writer.Write ((byte) (value >> 8));
			} else if (value <= int.MaxValue) {
				writer.Write (BF.Int32);
				for (int i = 0; i < 4; i++) {
					writer.Write ((byte) (value & 0xFF));
					value >>= 8;
				}
			}
		}

		public override void WriteValue (float value)
		{
			ProcessTypedValue ();
			writer.Write (BF.Single);
			WriteValueContent (value);
		}

		void WriteValueContent (float value)
		{
			writer.Write (value);
		}

		public override void WriteValue (double value)
		{
			ProcessTypedValue ();
			writer.Write (BF.Double);
			WriteValueContent (value);
		}

		void WriteValueContent (double value)
		{
			writer.Write (value);
		}

		public override void WriteValue (decimal value)
		{
			ProcessTypedValue ();
			writer.Write (BF.Decimal);
			WriteValueContent (value);
		}

		void WriteValueContent (decimal value)
		{
			int [] bits = Decimal.GetBits (value);
			// so, looks like it is saved as its internal form,
			// not the returned order.
			// BinaryWriter.Write(Decimal) is useless here.
			writer.Write (bits [3]);
			writer.Write (bits [2]);
			writer.Write (bits [0]);
			writer.Write (bits [1]);
		}

		public override void WriteValue (DateTime value)
		{
			ProcessTypedValue ();
			writer.Write (BF.DateTime);
			WriteValueContent (value);
		}

		void WriteValueContent (DateTime value)
		{
			writer.Write (value.ToBinary ());
		}

		public override void WriteValue (Guid value)
		{
			ProcessTypedValue ();
			writer.Write (BF.Guid);
			WriteValueContent (value);
		}

		void WriteValueContent (Guid value)
		{
			byte [] bytes = value.ToByteArray ();
			writer.Write (bytes, 0, bytes.Length);
		}

		public override void WriteValue (UniqueId value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			Guid guid;
			if (value.TryGetGuid (out guid)) {
				// this conditional branching is required for
				// attr_typed_value not being true.
				ProcessTypedValue ();

				writer.Write (BF.UniqueId);
				byte [] bytes = guid.ToByteArray ();
				writer.Write (bytes, 0, bytes.Length);
			} else {
				WriteValue (value.ToString ());
			}
		}

		public override void WriteValue (TimeSpan value)
		{
			ProcessTypedValue ();

			writer.Write (BF.TimeSpan);
			WriteValueContent (value);
		}

		void WriteValueContent (TimeSpan value)
		{
			WriteBigEndian (value.Ticks, 8);
		}
		#endregion

		private void WriteBigEndian (long value, int digits)
		{
			long v = 0;
			for (int i = 0; i < digits; i++) {
				v = (v << 8) + (value & 0xFF);
				value >>= 8;
			}
			for (int i = 0; i < digits; i++) {
				writer.Write ((byte) (v & 0xFF));
				v >>= 8;
			}
		}

		private void WriteTextBinary (string text)
		{
			if (text.Length == 0)
				writer.Write (BF.EmptyText);
			else {
				char [] arr = text.ToCharArray ();
				WriteChars (arr, 0, arr.Length);
			}
		}

		#endregion

		#region Write typed array content

		// they are packed in WriteValue(), so we cannot reuse
		// them for array content.

		void WriteValueContent (bool value)
		{
			writer.Write (value ? (byte) 1 : (byte) 0);
		}

		void WriteValueContent (short value)
		{
			writer.Write (value);
		}

		void WriteValueContent (int value)
		{
			writer.Write (value);
		}

		void WriteValueContent (long value)
		{
			writer.Write (value);
		}

		#endregion
	}
}
