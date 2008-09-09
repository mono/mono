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
	//	- native value data (7B-82, 8D-A0) are not implemented.
	//	- support XmlDictionaryReaderQuotas.
	//	- support XmlBinaryReaderSession.
	//	- handle namespaces as expected.

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
			BinaryReader stream;
			int position;

			public StreamSource (Stream stream)
			{
				this.stream = new BinaryReader (stream);
			}

			public int Position {
				get { return position - 1; }
			}

			public BinaryReader Reader {
				get { return stream; }
			}

			public int ReadByte ()
			{
				if (stream.PeekChar () < 0)
					return -1;
				position++;
				return stream.ReadByte ();
			}

			public int Read (byte [] data, int offset, int count)
			{
				int ret = stream.Read (data, offset, count);
				position += ret;
				return ret;
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
			public XmlNodeType NodeType;
			public object TypedValue;
			public byte ValueType;

			// -1 for nothing,
			// -2 for that of element (only for attribute),
			// 0 or more to fill later
			public int NSSlot;

			string name;
			string local_name;
			string ns;
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
					if (name == null)
						name = Prefix.Length > 0 ?
							String.Concat (Prefix, ":", LocalName) :
							LocalName;
					return name;
				}
			}

			public string Value {
				get {
					if (BF.AttrString <= ValueType && ValueType <= BF.GlobalAttrIndexInElemNS)
						return value; // attribute
					switch (ValueType) {
					case 0:
					case BF.Comment:
					case BF.Text:
					case BF.EmptyText:
						return value;
					case BF.Zero:
					case BF.One:
						return XmlConvert.ToString ((int) TypedValue);
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
					case BF.UniqueIdFromGuid:
						return TypedValue.ToString ();
					case BF.Base64:
					case BF.Base64Fixed:
						return Convert.ToBase64String ((byte []) TypedValue);
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
				LocalName = NS = Prefix = Value = String.Empty;
				NodeType = XmlNodeType.None;
				TypedValue = null;
				ValueType = 0;
				NSSlot = -1;
			}
		}

		class AttrNodeInfo : NodeInfo
		{
			public int ValueIndex;

			public override void Reset ()
			{
				base.Reset ();
				ValueIndex = -1;
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
		}

		public override int AttributeCount {
			get { return attr_count; }
		}

		public override string BaseURI {
			get { return context.BaseURI; }
		}

		public override int Depth {
			get { return depth; }
		}

		public override bool EOF {
			get { return state == ReadState.EndOfFile || state == ReadState.Error; }
		}

#if !NET_2_1
		public override bool HasValue {
			get { return current.Value.Length > 0; }
		}
#endif

		public override bool IsEmptyElement {
			get { return false; }
		}

		public override XmlNodeType NodeType {
			get { return current.NodeType; }
		}

		public override string Prefix {
			get { return current.Prefix; }
		}

		public override string LocalName {
			get { return current.LocalName; }
		}

		public override string NamespaceURI {
			get { return current.NS; }
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
			for (int i = 0; i < attributes.Count; i++)
				if (attributes [i].Name == name)
					return attributes [i].Value;
			return null;
		}

		public override string GetAttribute (string localName, string ns)
		{
			for (int i = 0; i < attributes.Count; i++)
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

#if !NET_2_1
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
			for (int i = start; i < end; i++) {
				if (current == attr_values [i] && i + 1 < end) {
					current = attr_values [i + 1];
					return true;
				}
			}
			return false;
		}
#endif

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
			attr_count = 0;
			attr_value_count = 0;
			ns_slot = 0;
			current = node;

			if (node.NodeType == XmlNodeType.Element) {
				// push element scope
				depth++;
				if (node_stack.Count <= depth) {
					node_stack.Add (node);
					node = new NodeInfo ();
				}
				else
					node = node_stack [depth];
				node.Reset ();
				current = node;
			}

			if (is_next_end_element) {
				is_next_end_element = false;
				ProcessEndElement ();
				return true;
			}
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
/*
			if (0x3F <= ident && ident <= 0x42)
				ReadElementBinary ((byte) ident);
			else {
				switch (ident) {
				case 0x3C: // end element
					ProcessEndElement ();
					break;
				case 0x3D: // comment
					node.Value = ReadUTF8 ();
					node.NodeType = XmlNodeType.Comment;
					break;
				default:
					ReadTextOrValue ((byte) ident, node, false);
					break;
				}
			}
*/
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

			default:
				ReadTextOrValue ((byte) ident, node, false);
				break;
			}

			return true;
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
			}

			bool loop = true;
			do {
				ident = next < 0 ? ReadByteOrError () : next;
				next = -1;

				switch (ident) {
				case BF.AttrString:
				case BF.AttrStringPrefix:
				case BF.AttrIndex:
				case BF.AttrIndexPrefix:
				case BF.GlobalAttrIndex:
				case BF.GlobalAttrIndexInElemNS:
					ReadAttribute ((byte) ident);
					break;
				case BF.DefaultNSString:
				case BF.PrefixNSString:
				case BF.DefaultNSIndex:
				case BF.PrefixNSIndex:
					ReadNamespace ((byte) ident);
					break;
				default:
					next = ident;
					loop = false;
					break;
				}
/*
				if (ident < 4) {
					// attributes
					if (attributes.Count == attr_count)
						attributes.Add (new AttrNodeInfo ());
					AttrNodeInfo a = attributes [attr_count++];
					a.Reset ();
					a.Position = source.Position;
					switch (ident) {
					case 0:
						a.LocalName = ReadUTF8 ();
						break;
					case 1:
						a.Prefix = ReadUTF8 ();
						goto case 0;
					case 2:
						a.DictLocalName = ReadDictName ();
						break;
					case 3:
						a.Prefix = ReadUTF8 ();
						goto case 2;
					}
					ReadAttributeValueBinary (a);
				}
				else if (ident < 6) {
					// namespaces
					string prefix = ident == 4 ?
						String.Empty : ReadUTF8 ();
					string ns = ReadUTF8 ();
					ns_store.Add (new QName (prefix, ns));
					context.NamespaceManager.AddNamespace (prefix, ns);
				}
				else if (0x22 <= ident && ident < 0x3C) {
					// attributes with predefined ns index
					if (attributes.Count == attr_count)
						attributes.Add (new AttrNodeInfo ());
					AttrNodeInfo a = attributes [attr_count++];
					a.Reset ();
					a.Position = source.Position;
					a.NSSlot = ident - 0x22;
					a.LocalName = ReadUTF8 ();
					ReadAttributeValueBinary (a);
				}
				else {
					next = ident;
					break;
				}
*/
			} while (loop);

#if true
			node.NS = context.NamespaceManager.LookupNamespace (node.Prefix) ?? String.Empty;
			foreach (AttrNodeInfo a in attributes)
				if (a.Prefix.Length > 0)
					a.NS = context.NamespaceManager.LookupNamespace (a.Prefix);
//Console.WriteLine ("[{0}-{1}->{3}/{2:X02}]", node.Prefix, node.LocalName, ident, node.NS);
#else
			if (node.Prefix.Length == 0)
				foreach (QName q in ns_store)
					if (q.Name.Length == 0) {
						node.NS = q.Namespace;
						break;
					}
			else if (node.NSSlot >= 0)
				FillNamespaceBySlot (node);
			foreach (AttrNodeInfo a in attributes) {
				if (a.NSSlot >= 0) {
					/*
					if (a.NSSlot >= ns_store.Count)
						throw new XmlException (String.Format ("Binary XML data is not valid. An attribute node has an invalid index at position {0}. Index is {1}.", a.Position, a.NSSlot));
					a.NS = ns_store [a.NSSlot].Namespace;
					a.Prefix = ns_store [a.NSSlot].Name;
					*/
					FillNamespaceBySlot (a);
				}
				else if (a.NSSlot == -2) {
					a.NS = node.NS;
					a.Prefix = node.Prefix;
				}
			}
#endif

			ns_store.Clear ();
			ns_dict_store.Clear ();
		}

		void FillNamespaceBySlot (NodeInfo n)
		{
			if (n.NSSlot >= ns_store.Count)
				throw new XmlException (String.Format ("Binary XML data is not valid. The '{2}' node has an invalid index. Index is {1}. The position in the stream is at {0}.", n.Position, n.NSSlot, n.NodeType));
			n.NS = ns_store [n.NSSlot].Namespace;
			//n.Prefix = ns_store [n.NSSlot].Name;
		}

		private void ReadAttribute (byte ident)
		{
			if (attributes.Count == attr_count)
				attributes.Add (new AttrNodeInfo ());
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
			case BF.GlobalAttrIndex:
				a.NSSlot = ns_slot++;
				a.DictLocalName = ReadDictName ();
				// FIXME: retrieve namespace
				break;
			case BF.GlobalAttrIndexInElemNS:
				a.Prefix = node.Prefix;
				a.DictLocalName = ReadDictName ();
				a.NSSlot = -2;
				break;
			}
			ReadAttributeValueBinary (a);
		}

		private void ReadNamespace (byte ident)
		{
			string prefix = null, ns = null;
				XmlDictionaryString dns;
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
			ns_store.Add (new QName (prefix, ns));
			context.NamespaceManager.AddNamespace (prefix, ns);
		}

		private void ReadAttributeValueBinary (AttrNodeInfo a)
		{
			a.ValueIndex = attr_value_count;
			do {
				if (attr_value_count == attr_values.Count)
					attr_values.Add (new NodeInfo (true));
				NodeInfo v = attr_values [attr_value_count++];
				v.Reset ();
				int ident = ReadByteOrError ();
				is_next_end_element = ident > 0x80 && (ident & 1) == 1;
				ident -= is_next_end_element ? 1 : 0;
				if (!ReadTextOrValue ((byte) ident, v, true) || is_next_end_element)
					break;
			} while (true);
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
				node.TypedValue = new DateTime (source.Reader.ReadInt64 ());
				break;
			//case BF.UniqueId: // identical to .Text
			case BF.Base64:
				byte [] base64 = new byte [ReadVariantSize ()];
				source.Reader.Read (base64, 0, base64.Length);
				node.TypedValue = base64;
				break;
			case BF.Base64Fixed:
				base64 = new byte [source.Reader.ReadInt16 ()];
				source.Reader.Read (base64, 0, base64.Length);
				node.TypedValue = base64;
				break;
			case BF.TimeSpan:
				node.TypedValue = new TimeSpan (source.Reader.ReadInt64 ());
				break;
			case BF.UniqueIdFromGuid:
				byte [] guid = new byte [16];
				source.Reader.Read (guid, 0, guid.Length);
				node.TypedValue = new UniqueId (new Guid (guid));
				break;
			case BF.Guid:
				guid = new byte [16];
				source.Reader.Read (guid, 0, guid.Length);
				node.TypedValue = new Guid (guid);
				break;
			case BF.Text:
				node.Value = ReadUTF8 ();
				node.NodeType = XmlNodeType.Text;
				break;
			case BF.EmptyText:
				node.Value = String.Empty;
				node.NodeType = XmlNodeType.Text;
				break;
			default:
				if (!canSkip)
					throw new ArgumentException (String.Format ("Unexpected binary XML data at position {1}: {0:X}", ident + (is_next_end_element ? 1 : 0), source.Position));
				next = ident;
				return false;
			}
			return true;
/*
			if (ident == 0x8B) {
				// empty text
				node.Value = String.Empty;
				node.NodeType = XmlNodeType.Text;
			}
			else if (0x83 <= ident && ident <= 0x85 ||
				0x9D <= ident && ident <= 0x9F) {
				// text
				int sizeSpec = ident > 0x90 ? ident - 0x9D : ident - 0x83;
				node.Value = ReadUTF8 (sizeSpec);
				node.NodeType = XmlNodeType.Text;
				is_next_end_element = ident > 0x90;
			}
			else {
				switch (ident) {
				case 0x7B: // byte
				case 0x7C: // short
				case 0x7D: // int
				case 0x7E: // long
				case 0x7F: // float
				case 0x80: // double
				case 0x81: // decimal
				case 0x82: // DateTime
				case 0x8D: // UniqueId
				case 0x8E: // TimeSpan
				case 0x8F: // Guid
				case 0xA0: // base64Binary
					Console.WriteLine ("At position {0}({0:X})", source.Position);
					throw new NotImplementedException ();
				default:
					if (!canSkip)
						throw new ArgumentException (String.Format ("Unexpected binary XML data at position {1}: {0:X}", ident, source.Position));
					next = ident;
					return false;
				}
			}
			return true;
*/
		}

		private int ReadVariantSize ()
		{
			int size = 0;
			// If sizeSpec < 0, then it is variant size specifier.
			// Otherwise it is fixed size s = sizeSpec + 1 byte(s).
			do {
				size <<= 7;
				byte got = ReadByteOrError ();
				size += got;
				if (got < 0x80)
					break;
				size -= 0x80;
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
				tmp_buffer = new byte [size < extlen ? extlen : size];
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
			if (current.ValueType != BF.Base64 &&
			    current.ValueType != BF.Base64Fixed)
				return false;
			length = ((byte []) current.TypedValue).Length;
			return true;
		}

		public override string ReadContentAsString ()
		{
			string value = Value;
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
			throw new InvalidOperationException ("Current content is not an integer");
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

		// FIXME: this is not likely to consume sequential base64 nodes.
		public override byte [] ReadContentAsBase64 ()
		{
			byte [] ret = null;
			if (node.ValueType != BF.Base64 &&
			    node.ValueType != BF.Base64Fixed)
				throw new InvalidOperationException ("Current content is not base64");
			while (NodeType == XmlNodeType.Text &&
			       (node.ValueType == BF.Base64 || node.ValueType == BF.Base64Fixed)) {
				if (ret == null)
					ret = (byte []) node.TypedValue;
				else {
					byte [] tmp = (byte []) node.TypedValue;
					byte [] tmp2 = new byte [ret.Length + tmp.Length];
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
			case BF.Text:
				UniqueId ret = new UniqueId (node.Value);
				Read ();
				return ret;
			case BF.UniqueIdFromGuid:
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
