//
// XmlBinaryDictionaryReader.cs
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
using System.Text;

using QName = System.Xml.XmlQualifiedName;
using BF = System.Xml.XmlBinaryFormat;

namespace System.Xml
{
	// FIXME:
	//	- support XmlDictionaryReaderQuotas.

	internal class XmlBinaryDictionaryReader : XmlDictionaryReader, IXmlNamespaceResolver
	{
		internal interface ISource
		{
			int Position { get; }
			int ReadByte ();
			int Read (byte [] data, int offset, int count);
			BinaryReader Reader { get; }
		}

		internal class StreamSource : ISource
		{
			BinaryReader reader;

			public StreamSource (Stream stream)
			{
				this.reader = new BinaryReader (stream);
			}

			public int Position {
				get { return (int) reader.BaseStream.Position; }
			}

			public BinaryReader Reader {
				get { return reader; }
			}

			public int ReadByte ()
			{
				return reader.BaseStream.ReadByte ();
			}

			public int Read (byte [] data, int offset, int count)
			{
				return reader.BaseStream.Read (data, offset, count);
			}
		}

		class NodeInfo
		{
			public NodeInfo ()
			{
			}

			public NodeInfo (bool isAttr)
			{
				IsAttributeValue = isAttr;
			}

			public bool IsAttributeValue;
			public int Position;
			public string Prefix;
			public XmlDictionaryString DictLocalName;
			public XmlDictionaryString DictNS;
			public XmlDictionaryString DictValue; // BF.TextIndex
			public XmlNodeType NodeType;
			public object TypedValue;
			public byte ValueType;

			// -1 for nothing,
			// -2 for that of element (only for attribute),
			// 0 or more to fill later
			public int NSSlot;

			string name = String.Empty;
			string local_name = String.Empty;
			string ns = String.Empty;
			string value;

			public string LocalName {
				get { return DictLocalName != null ? DictLocalName.Value : local_name; }
				set {
					DictLocalName = null;
					local_name = value;
				}
			}

			public string NS {
				get { return DictNS != null ? DictNS.Value : ns; }
				set {
					DictNS = null;
					ns = value;
				}
			}

			public string Name {
				get {
					if (name.Length == 0)
						name = Prefix.Length > 0 ?
							String.Concat (Prefix, ":", LocalName) :
							LocalName;
					return name;
				}
			}

			public virtual string Value {
				get {
					switch (ValueType) {
					case 0:
					case BF.Comment:
					case BF.Chars8:
					case BF.Chars16:
					case BF.Chars32:
					case BF.EmptyText:
					case BF.Utf16_8:
					case BF.Utf16_16:
					case BF.Utf16_32:
						return value;
					case BF.TextIndex:
						return DictValue.Value;
					case BF.Zero:
						return "0";
					case BF.One:
						return "1";
					case BF.BoolTrue:
						return "true";
					case BF.BoolFalse:
						return "false";
					case BF.Int8:
						return XmlConvert.ToString ((byte) TypedValue);
					case BF.Int16:
						return XmlConvert.ToString ((short) TypedValue);
					case BF.Int32:
						return XmlConvert.ToString ((int) TypedValue);
					case BF.Int64:
						return XmlConvert.ToString ((long) TypedValue);
					case BF.Single:
						return XmlConvert.ToString ((float) TypedValue);
					case BF.Double:
						return XmlConvert.ToString ((double) TypedValue);
					case BF.DateTime:
						return XmlConvert.ToString ((DateTime) TypedValue, XmlDateTimeSerializationMode.RoundtripKind);
					case BF.TimeSpan:
						return XmlConvert.ToString ((TimeSpan) TypedValue);
					case BF.Guid:
						return XmlConvert.ToString ((Guid) TypedValue);
					case BF.UniqueId:
						return TypedValue.ToString ();
					case BF.Bytes8:
					case BF.Bytes16:
					case BF.Bytes32:
						return Convert.ToBase64String ((byte []) TypedValue);
					case BF.QNameIndex:
						return Name;
					default:
						throw new NotImplementedException ("ValueType " + ValueType + " on node " + NodeType);
					}
				}
				set { this.value = value; }
			}

			public virtual void Reset ()
			{
				Position = 0;
				DictLocalName = DictNS = null;
				LocalName = NS = Prefix = Value = name = String.Empty;
				NodeType = XmlNodeType.None;
				TypedValue = null;
				ValueType = 0;
				NSSlot = -1;
			}
		}

		class AttrNodeInfo : NodeInfo
		{
			public AttrNodeInfo (XmlBinaryDictionaryReader owner)
			{
				this.owner = owner;
			}

			XmlBinaryDictionaryReader owner;
			public int ValueIndex;

			public override void Reset ()
			{
				base.Reset ();
				ValueIndex = -1;
				NodeType = XmlNodeType.Attribute;
			}

			public override string Value {
				get { return owner.attr_values [ValueIndex].Value; }
			}
		}

		ISource source;
		IXmlDictionary dictionary;
		XmlDictionaryReaderQuotas quota;
		XmlBinaryReaderSession session;
		OnXmlDictionaryReaderClose on_close;
		XmlParserContext context;

		ReadState state = ReadState.Initial;
		NodeInfo node;
		NodeInfo current;
		List<AttrNodeInfo> attributes = new List<AttrNodeInfo> ();
		List<NodeInfo> attr_values = new List<NodeInfo> ();
		List<NodeInfo> node_stack = new List<NodeInfo> ();
		List<QName> ns_store = new List<QName> ();
		Dictionary<int,XmlDictionaryString> ns_dict_store =
			new Dictionary<int,XmlDictionaryString> ();
		int attr_count;
		int attr_value_count;
		int current_attr = -1;
		int depth = 0;
		// used during Read()
		int ns_slot;
		// next byte in the source (one byte token ahead always
		// happens because there is no "end of start element" mark).
		int next = -1;
		bool is_next_end_element;
		// temporary buffer for utf8enc.GetString()
		byte [] tmp_buffer = new byte [128];
		UTF8Encoding utf8enc = new UTF8Encoding ();

		// See comment at Read()
		int array_item_remaining;
		byte array_item_type;
		XmlNodeType array_state;

		public XmlBinaryDictionaryReader (byte [] buffer, int offset,
			int count, IXmlDictionary dictionary,
			XmlDictionaryReaderQuotas quota,
			XmlBinaryReaderSession session,
			OnXmlDictionaryReaderClose onClose)
		{
			source = /*new ArraySource (buffer, offset, count);*/
				new StreamSource (new MemoryStream (buffer, offset, count));
			Initialize (dictionary, quota, session, onClose);
		}

		public XmlBinaryDictionaryReader (Stream stream,
			IXmlDictionary dictionary,
			XmlDictionaryReaderQuotas quota,
			XmlBinaryReaderSession session,
			OnXmlDictionaryReaderClose onClose)
		{
			source = new StreamSource (stream);
			Initialize (dictionary, quota, session, onClose);
		}

		private void Initialize (IXmlDictionary dictionary,
			XmlDictionaryReaderQuotas quotas,
			XmlBinaryReaderSession session,
			OnXmlDictionaryReaderClose onClose)
		{
			if (quotas == null)
				throw new ArgumentNullException ("quotas");
			if (dictionary == null)
				dictionary = new XmlDictionary ();
			this.dictionary = dictionary;

			this.quota = quotas;

			if (session == null)
				session = new XmlBinaryReaderSession ();
			this.session = session;

			on_close = onClose;
			NameTable nt = new NameTable ();
			this.context = new XmlParserContext (nt,
				new XmlNamespaceManager (nt),
				null, XmlSpace.None);

			current = node = new NodeInfo ();
			current.Reset ();
			node_stack.Add (node);
		}

		public override int AttributeCount {
			get { return attr_count; }
		}

		public override string BaseURI {
			get { return context.BaseURI; }
		}

		public override int Depth {
			get { return current == node ? depth : NodeType == XmlNodeType.Attribute ? depth + 1 : depth + 2; }
		}

		public override bool EOF {
			get { return state == ReadState.EndOfFile || state == ReadState.Error; }
		}

		public override bool HasValue {
			get { return Value.Length > 0; }
		}

		public override bool IsEmptyElement {
			get { return false; }
		}

		public override XmlNodeType NodeType {
			get { return current.NodeType; }
		}

		public override string Prefix {
			get { return current_attr >= 0 ? attributes [current_attr].Prefix : current.Prefix; }
		}

		// looks like it may return attribute's name even if it is on its value node.
		public override string LocalName {
			get { return current_attr >= 0 ? attributes [current_attr].LocalName : current.LocalName; }
		}

		public override string Name {
			get { return current_attr >= 0 ? attributes [current_attr].Name : current.Name; }
		}

		public override string NamespaceURI {
			get { return current_attr >= 0 ? attributes [current_attr].NS : current.NS; }
		}

		public override XmlNameTable NameTable {
			get { return context.NameTable; }
		}

		public override XmlDictionaryReaderQuotas Quotas {
			get { return quota; }
		}

		public override ReadState ReadState {
			get { return state; }
		}

		public override string Value {
			get { return current.Value; }
		}

		public override void Close ()
		{
			if (on_close != null)
				on_close (this);
		}

		public override string GetAttribute (int i)
		{
			if (i >= attr_count)
				throw new ArgumentOutOfRangeException (String.Format ("Specified attribute index is {0} and should be less than {1}", i, attr_count));
			return attributes [i].Value;
		}

		public override string GetAttribute (string name)
		{
			for (int i = 0; i < attr_count; i++)
				if (attributes [i].Name == name)
					return attributes [i].Value;
			return null;
		}

		public override string GetAttribute (string localName, string ns)
		{
			for (int i = 0; i < attr_count; i++)
				if (attributes [i].LocalName == localName &&
					attributes [i].NS == ns)
					return attributes [i].Value;
			return null;
		}

		public IDictionary<string,string> GetNamespacesInScope (
			XmlNamespaceScope scope)
		{
			return context.NamespaceManager.GetNamespacesInScope (scope);
		}

		public string LookupPrefix (string ns)
		{
			return context.NamespaceManager.LookupPrefix (NameTable.Get (ns));
		}

		public override string LookupNamespace (string prefix)
		{
			return context.NamespaceManager.LookupNamespace (
				NameTable.Get (prefix));
		}

		public override bool IsArray (out Type type)
		{
			if (array_state == XmlNodeType.Element) {
				type = GetArrayType (array_item_type);
				return true;
			} else {
				type = null;
				return false;
			}
		}

		public override bool MoveToElement ()
		{
			bool ret = current_attr >= 0;
			current_attr = -1;
			current = node;
			return ret;
		}

		public override bool MoveToFirstAttribute ()
		{
			if (attr_count == 0)
				return false;
			current_attr = 0;
			current = attributes [current_attr];
			return true;
		}

		public override bool MoveToNextAttribute ()
		{
			if (++current_attr < attr_count) {
				current = attributes [current_attr];
				return true;
			} else {
				--current_attr;
				return false;
			}
		}

		public override void MoveToAttribute (int i)
		{
			if (i >= attr_count)
				throw new ArgumentOutOfRangeException (String.Format ("Specified attribute index is {0} and should be less than {1}", i, attr_count));
			current_attr = i;
			current = attributes [i];
		}

		public override bool MoveToAttribute (string name)
		{
			for (int i = 0; i < attributes.Count; i++) {
				if (attributes [i].Name == name) {
					MoveToAttribute (i);
					return true;
				}
			}
			return false;
		}

		public override bool MoveToAttribute (string localName, string ns)
		{
			for (int i = 0; i < attributes.Count; i++) {
				if (attributes [i].LocalName == localName &&
					attributes [i].NS == ns) {
					MoveToAttribute (i);
					return true;
				}
			}
			return false;
		}

		public override bool ReadAttributeValue ()
		{
			if (current_attr < 0)
				return false;
			int start = attributes [current_attr].ValueIndex;
			int end = current_attr + 1 == attr_count ? attr_value_count : attributes [current_attr + 1].ValueIndex;
			if (start == end)
				return false;
			if (!current.IsAttributeValue) {
				current = attr_values [start];
				return true;
			}
			// Actually there is no case for attribute whose value is split to more than two nodes. We could simplify the node structure.
			return false;
		}

		// When reading an array (0x03), it requires extraneously
		// complex procedure for XmlReader. First, it reads element,
		// type of operation and length of the items. And this XmlReader
		// has to return Element state. On the next Read(), it proceeds
		// to the value node of the first item of the array, so it
		// reads the value stream. On the next Read(), it proceeds to
		// EndElement, so it should not read anything from stream while
		// it has to move to the node state to EndElement.
		public override bool Read ()
		{
			switch (state) {
			case ReadState.Closed:
			case ReadState.EndOfFile:
			case ReadState.Error:
				return false;
			}

			// clear.
			state = ReadState.Interactive;
			MoveToElement ();
			attr_count = 0;
			attr_value_count = 0;
			ns_slot = 0;

			if (node.NodeType == XmlNodeType.Element) {
				// push element scope
				if (node_stack.Count <= ++depth) {
					if (depth == quota.MaxDepth)
						throw new XmlException (String.Format ("Binary XML stream quota exceeded. Depth must be less than {0}", quota.MaxDepth));
					node = new NodeInfo ();
					node_stack.Add (node);
				} else {
					node = node_stack [depth]; // reuse
					node.Reset ();
				}
			}
			current = node;

			if (is_next_end_element) {
				is_next_end_element = false;
				node.Reset ();
				ProcessEndElement ();
				return true;
			}

			// process array node after preparing node stack.
			switch (array_state) {
			case XmlNodeType.Element:
				ReadArrayItem ();
				return true;
			case XmlNodeType.Text:
				ShiftToArrayItemEndElement ();
				return true;
			case XmlNodeType.EndElement:
				if (--array_item_remaining == 0) {
					array_state = XmlNodeType.None;
					break;
				} else {
					ShiftToArrayItemElement ();
					return true;
				}
			}

			// array consumer does not expect Reset whlie it's on reading. So call it later than array check.
			node.Reset ();

			int ident = next >= 0 ? next : source.ReadByte ();
			next = -1;

			// check end of source.
			if (ident < 0) {
				state = ReadState.EndOfFile;
				current.Reset ();
				return false;
			}

			is_next_end_element = ident > 0x80 && (ident & 1) == 1;
			ident -= is_next_end_element ? 1 : 0;

			switch (ident) {
			case BF.EndElement:
				ProcessEndElement ();
				break;
			case BF.Comment:
				node.Value = ReadUTF8 ();
				node.ValueType = BF.Comment;
				node.NodeType = XmlNodeType.Comment;
				break;
			case BF.ElemString:
			case BF.ElemStringPrefix:
			case BF.ElemIndex:
			case BF.ElemIndexPrefix:
				ReadElementBinary ((byte) ident);
				break;

			case BF.Array:
				ident = ReadByteOrError ();
				ReadElementBinary ((byte) ident);
				ident = ReadByteOrError ();
				if (ident != 0x01)
					throw new XmlException (String.Format ("EndElement is expected after element in an array. The actual byte was {0:X} in hexadecimal", ident));
				ident = ReadByteOrError () - 1; // -1 becauseit contains EndElement
				VerifyValidArrayItemType (ident);
				if (ident < 0)
					throw new XmlException ("The stream has ended where the array item type is expected");
				array_item_type = (byte) ident;
				array_item_remaining = ReadVariantSize ();
				if (array_item_remaining > quota.MaxArrayLength)
					throw new Exception (String.Format ("Binary xml stream exceeded max array length quota. Items are {0} and should be less than quota.MaxArrayLength", quota.MaxArrayLength));
				array_state = XmlNodeType.Element;
				break;

			default:
				if (BF.PrefixNElemIndexStart <= ident && ident <= BF.PrefixNElemIndexEnd ||
				    BF.PrefixNElemStringStart <= ident && ident <= BF.PrefixNElemStringEnd)
					goto case BF.ElemString;
				ReadTextOrValue ((byte) ident, node, false);
				break;
			}

			return true;
		}

		void ReadArrayItem ()
		{
			ReadTextOrValue (array_item_type, node, false);
			array_state = XmlNodeType.Text;
		}

		void ShiftToArrayItemEndElement ()
		{
			ProcessEndElement ();
			array_state = XmlNodeType.EndElement;
		}

		void ShiftToArrayItemElement ()
		{
			node.NodeType = XmlNodeType.Element;
			context.NamespaceManager.PushScope ();
			array_state = XmlNodeType.Element;
		}

		void VerifyValidArrayItemType (int ident)
		{
			if (GetArrayType (ident) == null)
				throw new XmlException (String.Format ("Unexpected array item type {0:X} in hexadecimal", ident));
		}
		
		Type GetArrayType (int ident)
		{
			switch (ident) {
			case BF.Bool:
				return typeof (bool);
			case BF.Int16:
				return typeof (short);
			case BF.Int32:
				return typeof (int);
			case BF.Int64:
				return typeof (long);
			case BF.Single:
				return typeof (float);
			case BF.Double:
				return typeof (double);
			case BF.Decimal:
				return typeof (decimal);
			case BF.DateTime:
				return typeof (DateTime);
			case BF.TimeSpan:
				return typeof (TimeSpan);
			case BF.Guid:
				return typeof (Guid);
			}
			return null;
		}

		private void ProcessEndElement ()
		{
			if (depth == 0)
				throw new XmlException ("Unexpected end of element while there is no element started.");
			current = node = node_stack [--depth];
			node.NodeType = XmlNodeType.EndElement;
			context.NamespaceManager.PopScope ();
		}

		private void ReadElementBinary (int ident)
		{
			// element
			node.NodeType = XmlNodeType.Element;
			node.Prefix = String.Empty;
			context.NamespaceManager.PushScope ();
			switch (ident) {
			case BF.ElemString:
				node.LocalName = ReadUTF8 ();
				break;
			case BF.ElemStringPrefix:
				node.Prefix = ReadUTF8 ();
				node.NSSlot = ns_slot++;
				goto case BF.ElemString;
			case BF.ElemIndex:
				node.DictLocalName = ReadDictName ();
				break;
			case BF.ElemIndexPrefix:
				node.Prefix = ReadUTF8 ();
				node.NSSlot = ns_slot++;
				goto case BF.ElemIndex;
			default:
				if (BF.PrefixNElemIndexStart <= ident && ident <= BF.PrefixNElemIndexEnd) {
					node.Prefix = ((char) (ident - BF.PrefixNElemIndexStart + 'a')).ToString ();
					node.DictLocalName = ReadDictName ();
				} else if (BF.PrefixNElemStringStart <= ident && ident <= BF.PrefixNElemStringEnd) {
					node.Prefix = ((char) (ident - BF.PrefixNElemStringStart + 'a')).ToString ();
					node.LocalName = ReadUTF8 ();
				}
				else
					throw new XmlException (String.Format ("Invalid element node type {0:X02} in hexadecimal", ident));
				break;
			}

			bool loop = true;
			do {
				ident = ReadByteOrError ();

				switch (ident) {
				case BF.AttrString:
				case BF.AttrStringPrefix:
				case BF.AttrIndex:
				case BF.AttrIndexPrefix:
					ReadAttribute ((byte) ident);
					break;
				case BF.DefaultNSString:
				case BF.PrefixNSString:
				case BF.DefaultNSIndex:
				case BF.PrefixNSIndex:
					ReadNamespace ((byte) ident);
					break;
				default:
					if (BF.PrefixNAttrStringStart <= ident && ident <= BF.PrefixNAttrStringEnd ||
					    BF.PrefixNAttrIndexStart <= ident && ident <= BF.PrefixNAttrIndexEnd)
						ReadAttribute ((byte) ident);
					else {
						next = ident;
						loop = false;
					}
					break;
				}
			} while (loop);

			node.NS = context.NamespaceManager.LookupNamespace (node.Prefix) ?? String.Empty;
			foreach (AttrNodeInfo a in attributes)
				if (a.Prefix.Length > 0)
					a.NS = context.NamespaceManager.LookupNamespace (a.Prefix);

			ns_store.Clear ();
			ns_dict_store.Clear ();
		}

		private void ReadAttribute (byte ident)
		{
			if (attributes.Count == attr_count)
				attributes.Add (new AttrNodeInfo (this));
			AttrNodeInfo a = attributes [attr_count++];
			a.Reset ();
			a.Position = source.Position;

			switch (ident) {
			case BF.AttrString:
				a.LocalName = ReadUTF8 ();
				break;
			case BF.AttrStringPrefix:
				a.Prefix = ReadUTF8 ();
				a.NSSlot = ns_slot++;
				goto case BF.AttrString;
			case BF.AttrIndex:
				a.DictLocalName = ReadDictName ();
				break;
			case BF.AttrIndexPrefix:
				a.Prefix = ReadUTF8 ();
				a.NSSlot = ns_slot++;
				goto case BF.AttrIndex;
			default:
				if (BF.PrefixNAttrStringStart <= ident && ident <= BF.PrefixNAttrStringEnd) {
					a.Prefix = ((char) ('a' + ident - BF.PrefixNAttrStringStart)).ToString ();
					a.LocalName = ReadUTF8 ();
					break;
				}
				else if (BF.PrefixNAttrIndexStart <= ident && ident <= BF.PrefixNAttrIndexEnd) {
					a.Prefix = ((char) ('a' + ident - BF.PrefixNAttrIndexStart)).ToString ();
					a.DictLocalName = ReadDictName ();
					break;
				}
				else throw new XmlException (String.Format ("Unexpected attribute node type: 0x{0:X02}", ident));
			}
			ReadAttributeValueBinary (a);
		}

		private void ReadNamespace (byte ident)
		{
			// create attrubute slot.
			if (attributes.Count == attr_count)
				attributes.Add (new AttrNodeInfo (this));
			AttrNodeInfo a = attributes [attr_count++];
			a.Reset ();
			a.Position = source.Position;

			string prefix = null, ns = null;
			XmlDictionaryString dns = null;

			switch (ident) {
			case BF.DefaultNSString:
				prefix = String.Empty;
				ns = ReadUTF8 ();
				break;
			case BF.PrefixNSString:
				prefix = ReadUTF8 ();
				ns = ReadUTF8 ();
				break;
			case BF.DefaultNSIndex:
				prefix = String.Empty;
				dns = ReadDictName ();
				ns_dict_store.Add (ns_store.Count, dns);
				ns = dns.Value;
				break;
			case BF.PrefixNSIndex:
				prefix = ReadUTF8 ();
				dns = ReadDictName ();
				ns_dict_store.Add (ns_store.Count, dns);
				ns = dns.Value;
				break;
			}

			// fill attribute slot.
			a.Prefix = prefix.Length > 0 ? "xmlns" : String.Empty;
			a.LocalName = prefix.Length > 0 ? prefix : "xmlns";
			a.NS = "http://www.w3.org/2000/xmlns/";
			a.ValueIndex = attr_value_count;
			if (attr_value_count == attr_values.Count)
				attr_values.Add (new NodeInfo (true));
			NodeInfo v = attr_values [attr_value_count++];
			v.Reset ();
			v.Value = ns;
			v.ValueType = BF.Chars8;
			v.NodeType = XmlNodeType.Text;

			ns_store.Add (new QName (prefix, ns));
			context.NamespaceManager.AddNamespace (prefix, ns);
		}

		private void ReadAttributeValueBinary (AttrNodeInfo a)
		{
			a.ValueIndex = attr_value_count;
			if (attr_value_count == attr_values.Count)
				attr_values.Add (new NodeInfo (true));
			NodeInfo v = attr_values [attr_value_count++];
			v.Reset ();
			int ident = ReadByteOrError ();
			bool end = ident > 0x80 && (ident & 1) == 1;
			ident -= end ? 1 : 0;
			ReadTextOrValue ((byte) ident, v, true);
		}

		private bool ReadTextOrValue (byte ident, NodeInfo node, bool canSkip)
		{
			node.Value = null;
			node.ValueType = ident;
			node.NodeType = XmlNodeType.Text;
			switch (ident) {
			case BF.Zero:
				node.TypedValue = 0;
				break;
			case BF.One:
				node.TypedValue = 1;
				break;
			case BF.BoolFalse:
				node.TypedValue = false;
				break;
			case BF.BoolTrue:
				node.TypedValue = true;
				break;
			case BF.Int8:
				node.TypedValue = ReadByteOrError ();
				break;
			case BF.Int16:
				node.TypedValue = source.Reader.ReadInt16 ();
				break;
			case BF.Int32:
				node.TypedValue = source.Reader.ReadInt32 ();
				break;
			case BF.Int64:
				node.TypedValue = source.Reader.ReadInt64 ();
				break;
			case BF.Single:
				node.TypedValue = source.Reader.ReadSingle ();
				break;
			case BF.Double:
				node.TypedValue = source.Reader.ReadDouble ();
				break;
			case BF.Decimal:
				int [] bits = new int [4];
				bits [3] = source.Reader.ReadInt32 ();
				bits [2] = source.Reader.ReadInt32 ();
				bits [0] = source.Reader.ReadInt32 ();
				bits [1] = source.Reader.ReadInt32 ();
				node.TypedValue = new Decimal (bits);
				break;
			case BF.DateTime:
				node.TypedValue = DateTime.FromBinary (source.Reader.ReadInt64 ());
				break;
			//case BF.UniqueId: // identical to .Text
			case BF.Bytes8:
			case BF.Bytes16:
			case BF.Bytes32:
				int size =
					(ident == BF.Bytes8) ? source.Reader.ReadByte () :
					(ident == BF.Bytes16) ? source.Reader.ReadUInt16 () :
					source.Reader.ReadInt32 ();
				byte [] base64 = Alloc (size);
				source.Reader.Read (base64, 0, base64.Length);
				node.TypedValue = base64;
				break;
			case BF.TimeSpan:
				node.TypedValue = new TimeSpan (source.Reader.ReadInt64 ());
				break;
			case BF.UniqueId:
				byte [] guid = new byte [16];
				source.Reader.Read (guid, 0, guid.Length);
				node.TypedValue = new UniqueId (new Guid (guid));
				break;
			case BF.Guid:
				guid = new byte [16];
				source.Reader.Read (guid, 0, guid.Length);
				node.TypedValue = new Guid (guid);
				break;
			case BF.Chars8:
			case BF.Chars16:
			case BF.Chars32:
			case BF.Utf16_8:
			case BF.Utf16_16:
			case BF.Utf16_32:
				Encoding enc = ident <= BF.Chars32 ? Encoding.UTF8 : Encoding.Unicode;
				size =
					(ident == BF.Chars8 || ident == BF.Utf16_8) ? source.Reader.ReadByte () :
					(ident == BF.Chars16 || ident == BF.Utf16_16) ? source.Reader.ReadUInt16 () :
					source.Reader.ReadInt32 ();
				byte [] bytes = Alloc (size);
				source.Reader.Read (bytes, 0, size);
				node.Value = enc.GetString (bytes, 0, size);
				node.NodeType = XmlNodeType.Text;
				break;
			case BF.EmptyText:
				node.Value = String.Empty;
				node.NodeType = XmlNodeType.Text;
				break;
			case BF.TextIndex:
				node.DictValue = ReadDictName ();
				node.NodeType = XmlNodeType.Text;
				break;
			case BF.QNameIndex:
				node.Prefix = ((char) (ReadByteOrError () + 'a')).ToString ();
				node.DictLocalName = ReadDictName();
				break;	
			default:
				if (!canSkip)
					throw new ArgumentException (String.Format ("Unexpected binary XML data at position {1}: {0:X}", ident + (is_next_end_element ? 1 : 0), source.Position));
				next = ident;
				return false;
			}
			return true;
		}

		byte [] Alloc (int size)
		{
			if (size > quota.MaxStringContentLength || size < 0)
				throw new XmlException (String.Format ("Text content buffer exceeds the quota limitation at {2}. {0} bytes and should be less than {1} bytes", size, quota.MaxStringContentLength, source.Position));
			return new byte [size];
		}

		private int ReadVariantSize ()
		{
			int size = 0;
			// If sizeSpec < 0, then it is variant size specifier.
			// Otherwise it is fixed size s = sizeSpec + 1 byte(s).
			int d = 0;
			do {
				byte got = ReadByteOrError ();
				size += (got & 0x7F) << d;
				d += 7;
				if (got < 0x80)
					break;
			} while (true);
			return size;
		}

		private string ReadUTF8 ()
		{
			int size = ReadVariantSize ();
			if (size == 0)
				return String.Empty;
			if (tmp_buffer.Length < size) {
				int extlen = tmp_buffer.Length * 2;
				tmp_buffer = Alloc (size < extlen ? extlen : size);
			}
			size = source.Read (tmp_buffer, 0, size);
			return utf8enc.GetString (tmp_buffer, 0, size);
		}

		private XmlDictionaryString ReadDictName ()
		{
			int key = ReadVariantSize ();
			XmlDictionaryString s;
			if ((key & 1) == 1) {
				if (session.TryLookup (key >> 1, out s))
					return s;
			} else {
				if (dictionary.TryLookup (key >> 1, out s))
					return s;
			}
			throw new XmlException (String.Format ("Input XML binary stream is invalid. No matching XML dictionary string entry at {0}. Binary stream position at {1}", key, source.Position));
		}

		private byte ReadByteOrError ()
		{
			if (next >= 0) {
				byte b = (byte) next;
				next = -1;
				return b;
			}
			int ret = source.ReadByte ();
			if (ret < 0)
				throw new XmlException (String.Format ("Unexpected end of binary stream. Position is at {0}", source.Position));
			return (byte) ret;
		}

		public override void ResolveEntity ()
		{
			throw new NotSupportedException ("this XmlReader does not support ResolveEntity.");
		}

		public override bool TryGetBase64ContentLength (out int length)
		{
			length = 0;
			switch (current.ValueType) {
			case BF.Bytes8:
			case BF.Bytes16:
			case BF.Bytes32:
				length = ((byte []) current.TypedValue).Length;
				return true;
			}
			return false;
		}

		public override string ReadContentAsString ()
		{
			string value = String.Empty;
			do {
				switch (NodeType) {
				case XmlNodeType.Element:
				case XmlNodeType.EndElement:
					return value;
				case XmlNodeType.Text:
					value += Value;
					break;
				}
			} while (Read ());
			return value;
		}

		#region read typed content

		public override int ReadContentAsInt ()
		{
			int ret = GetIntValue ();
			Read ();
			return ret;
		}
		
		int GetIntValue ()
		{
			switch (node.ValueType) {
			case BF.Zero:
				return 0;
			case BF.One:
				return 1;
			case BF.Int8:
				return (byte) current.TypedValue;
			case BF.Int16:
				return (short) current.TypedValue;
			case BF.Int32:
				return (int) current.TypedValue;
			}
			throw new InvalidOperationException (String.Format ("Current content is not an integer. (Internal value type:{0:X02})", (int) node.ValueType));
		}

		public override long ReadContentAsLong ()
		{
			if (node.ValueType == BF.Int64) {
				long v = (long) current.TypedValue;
				Read ();
				return v;
			}
			return ReadContentAsInt ();
		}

		public override float ReadContentAsFloat ()
		{
			if (node.ValueType != BF.Single)
				throw new InvalidOperationException ("Current content is not a single");
			float v = (float) current.TypedValue;
			Read ();
			return v;
		}

		public override double ReadContentAsDouble ()
		{
			if (node.ValueType != BF.Double)
				throw new InvalidOperationException ("Current content is not a double");
			double v = (double) current.TypedValue;
			Read ();
			return v;
		}

		bool IsBase64Node (byte b)
		{
			switch (b) {
			case BF.Bytes8:
			case BF.Bytes16:
			case BF.Bytes32:
				return true;
			}
			return false;
		}

		// FIXME: this is not likely to consume sequential base64 nodes.
		public override byte [] ReadContentAsBase64 ()
		{
			byte [] ret = null;
			if (!IsBase64Node (node.ValueType))
				throw new InvalidOperationException ("Current content is not base64");

			while (NodeType == XmlNodeType.Text && IsBase64Node (node.ValueType)) {
				if (ret == null)
					ret = (byte []) node.TypedValue;
				else {
					byte [] tmp = (byte []) node.TypedValue;
					byte [] tmp2 = Alloc (ret.Length + tmp.Length);
					Array.Copy (ret, tmp2, ret.Length);
					Array.Copy (tmp, 0, tmp2, ret.Length, tmp.Length);
					ret = tmp2;
				}
				Read ();
				//MoveToContent ();
			}
			return ret;
		}

		public override Guid ReadContentAsGuid ()
		{
			if (node.ValueType != BF.Guid)
				throw new InvalidOperationException ("Current content is not a Guid");
			Guid ret = (Guid) node.TypedValue;
			Read ();
			return ret;
		}

		public override UniqueId ReadContentAsUniqueId ()
		{
			switch (node.ValueType) {
			case BF.Chars8:
			case BF.Chars16:
			case BF.Chars32:
			case BF.Utf16_8:
			case BF.Utf16_16:
			case BF.Utf16_32:
				UniqueId ret = new UniqueId (node.Value);
				Read ();
				return ret;
			case BF.UniqueId:
				ret = (UniqueId) node.TypedValue;
				Read ();
				return ret;
			default:
				throw new InvalidOperationException ("Current content is not a UniqueId");
			}
		}

		#endregion
	}
}
