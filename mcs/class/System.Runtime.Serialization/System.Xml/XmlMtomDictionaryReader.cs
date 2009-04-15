//
// XmlMtomDictionaryReader.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Xml;

namespace System.Xml
{
	internal class XmlMtomDictionaryReader : XmlDictionaryReader
	{
		public XmlMtomDictionaryReader (
			Stream stream, Encoding encoding,
			XmlDictionaryReaderQuotas quotas)
		{
			this.stream = stream;
			this.encoding = encoding;
			this.quotas = quotas;

			Initialize ();
		}

		public XmlMtomDictionaryReader (
			Stream stream, Encoding [] encodings, string contentType,
			XmlDictionaryReaderQuotas quotas,
			int maxBufferSize,
			OnXmlDictionaryReaderClose onClose)
		{
			this.stream = stream;
			this.encodings = encodings;
			content_type = contentType != null ? CreateContentType (contentType) : null;
			this.quotas = quotas;
			this.max_buffer_size = maxBufferSize;
			on_close = onClose;

			Initialize ();
		}

		Stream stream;
		Encoding encoding;
		Encoding [] encodings;
		ContentType content_type;
		XmlDictionaryReaderQuotas quotas;
		int max_buffer_size;
		OnXmlDictionaryReaderClose on_close;

		Dictionary<string,MimeEncodedStream> readers = new Dictionary<string,MimeEncodedStream> ();

		void Initialize ()
		{
			var nt = new NameTable ();
			initial_reader = new NonInteractiveStateXmlReader (String.Empty, nt, ReadState.Initial);
			eof_reader = new NonInteractiveStateXmlReader (String.Empty, nt, ReadState.EndOfFile);
			xml_reader = initial_reader;
		}

		ContentType CreateContentType (string contentTypeString)
		{
			ContentType c = null;
			foreach (var s_ in contentTypeString.Split (';')) {
				var s = s_.Trim ();
				if (c == null) {
					// first one
					c = new ContentType (s);
					continue;
				}
				int idx = s.IndexOf ('=');
				if (idx < 0)
					throw new XmlException ("Invalid content type header");
				var val = StripBraces (s.Substring (idx + 1));
				c.Parameters [s.Substring (0, idx)] = val;
			}
			return c;
		}

		XmlReader xml_reader, initial_reader, eof_reader, part_reader;
		XmlReader Reader {
			get { return part_reader ?? xml_reader; }
		}

		public override bool EOF {
			get { return Reader == eof_reader; }
		}

		public override void Close ()
		{
			if (!EOF && on_close != null)
				on_close (this);
			xml_reader = eof_reader;
		}

		public override bool Read ()
		{
			if (EOF)
				return false;

			if (Reader == initial_reader)
				SetupPrimaryReader ();

			if (part_reader != null)
				part_reader = null;

			if (!Reader.Read ()) {
				xml_reader = eof_reader;
				return false;
			}
			if (Reader.LocalName == "Include" && Reader.NamespaceURI == "http://www.w3.org/2004/08/xop/include") {
				string cid = Reader.GetAttribute ("href");
				if (!cid.StartsWith ("cid:"))
					throw new XmlException ("Cannot resolve non-cid href attribute value in XOP Include element");
				cid = cid.Substring (4);
				if (!readers.ContainsKey (cid))
					ReadToIdentifiedStream (cid);
				part_reader = new MultiPartedXmlReader (Reader, readers [cid]);
			}
			return true;
		}

		void SetupPrimaryReader ()
		{
			ReadOptionalMimeHeaders ();
			if (current_content_type != null)
				content_type = current_content_type;

			if (content_type == null)
				throw new XmlException ("Content-Type header for the MTOM message was not found");
			if (content_type.Boundary == null)
				throw new XmlException ("Content-Type header for the MTOM message must contain 'boundary' parameter");

			if (encoding == null && content_type.CharSet != null)
				encoding = Encoding.GetEncoding (content_type.CharSet);
			if (encoding == null && encodings == null)
				throw new XmlException ("Encoding specification is required either in the constructor argument or the content-type header");

			// consume the first identifier.
			string ident = "--" + content_type.Boundary;
			string idline;

			while (true) {
				idline = ReadAsciiLine ().Trim ();
				if (idline == null)
					return;
				if (idline.Length != 0)
					break;
			}
			if (!idline.StartsWith (ident, StringComparison.Ordinal))
				throw new XmlException (String.Format ("Unexpected boundary line was found. Expected boundary is '{0}' but it was '{1}'", content_type.Boundary, idline));

			string start = content_type.Parameters ["start"];
			ReadToIdentifiedStream (start);

			xml_reader = XmlReader.Create (readers [start].CreateTextReader ());
		}

		int buffer_length;
		byte [] buffer;
		int peek_char;

		ContentType current_content_type;
		int content_index;
		string current_content_id, current_content_encoding;

		void ReadToIdentifiedStream (string id)
		{
			while (true) {
				if (!ReadNextStream ())
					throw new XmlException (String.Format ("The stream '{0}' did not appear", id));
				if (current_content_id == id || id == null)
					break;
			}
		}

		bool ReadNextStream ()
		{
			ReadOptionalMimeHeaders ();
			string ident = "--" + content_type.Boundary;

			StringBuilder sb = new StringBuilder ();
			while (true) {
				string n = ReadAsciiLine ();
				if (n == null && sb.Length == 0)
					return false;
				else if (n == null || n.StartsWith (ident, StringComparison.Ordinal))
					break;
				sb.Append (n);
			}
			readers.Add (current_content_id, new MimeEncodedStream (current_content_id, current_content_encoding, sb.ToString ()));
			return true;
		}

		void ReadOptionalMimeHeaders ()
		{
			peek_char = stream.ReadByte ();
			if (peek_char == '-') // no header
				return;
			ReadMimeHeaders ();
		}

		string ReadAllHeaderLines ()
		{
			string s = String.Empty;

			while (true) {
				var n = ReadAsciiLine ();
				if (n.Length == 0)
					return s;
				n = n.TrimEnd ();
				s += n;
				if (n [n.Length - 1] != ';')
					s += '\n';
			}
		}

		void ReadMimeHeaders ()
		{
			foreach (var s in ReadAllHeaderLines ().Split ('\n')) {
				if (s.Length == 0)
					continue;
				int idx = s.IndexOf (':');
				if (idx < 0)
					throw new XmlException (String.Format ("Unexpected header string: {0}", s));
				string v = StripBraces (s.Substring (idx + 1).Trim ());
				switch (s.Substring (0, idx).ToLower ()) {
				case "content-type":
					current_content_type = CreateContentType (v);
					break;
				case "content-id":
					current_content_id = v;
					break;
				case "content-transfer-encoding":
					current_content_encoding = v;
					break;
				}
			}
		}

		string StripBraces (string s)
		{
			// could be foo, <foo>, "foo" and "<foo>".
			if (s.Length >= 2 && s [0] == '"' && s [s.Length - 1] == '"')
				s = s.Substring (1, s.Length - 2);
			if (s.Length >= 2 && s [0] == '<' && s [s.Length - 1] == '>')
				s = s.Substring (1, s.Length - 2);
			return s;
		}

		string ReadAsciiLine ()
		{
			if (buffer == null)
				buffer = new byte [1024];
			int bpos = 0;
			int b = peek_char;
			bool skipRead = b >= 0;
			peek_char = -1;
			while (true) {
				if (skipRead)
					skipRead = false;
				else
					b = stream.ReadByte ();
				if (b < 0) {
					if (bpos > 0)
						throw new XmlException ("The stream ends without end of line");
					return null;
				}
				if (b == '\r') {
					b = stream.ReadByte ();
					if (b < 0) {
						buffer [bpos++] = (byte) '\r';
						break;
					}
					else if (b == '\n')
						break;
					buffer [bpos++] = (byte) '\r';
					skipRead = true;
				}
				else
					buffer [bpos++] = (byte) b;
				if (bpos == buffer.Length) {
					var newbuf = new byte [buffer.Length << 1];
					Array.Copy (buffer, 0, newbuf, 0, buffer.Length);
					buffer = newbuf;
				}
			}
			return Encoding.ASCII.GetString (buffer, 0, bpos);
		}

		// The rest are just reader delegation.

		public override int AttributeCount {
			get { return Reader.AttributeCount; }
		}

		public override string BaseURI {
			get { return Reader.BaseURI; }
		}

		public override int Depth {
			get { return Reader.Depth; }
		}

		public override bool HasValue {
			get { return Reader.HasValue; }
		}

		public override bool IsEmptyElement {
			get { return Reader.IsEmptyElement; }
		}

		public override string LocalName {
			get { return Reader.LocalName; }
		}

		public override string NamespaceURI {
			get { return Reader.NamespaceURI; }
		}

		public override XmlNameTable NameTable {
			get { return Reader.NameTable; }
		}

		public override XmlNodeType NodeType {
			get { return Reader.NodeType; }
		}

		public override string Prefix {
			get { return Reader.Prefix; }
		}

		public override ReadState ReadState {
			get { return Reader.ReadState; }
		}

		public override string Value {
			get { return Reader.Value; }
		}

		public override bool MoveToElement ()
		{
			return Reader.MoveToElement ();
		}

		public override string GetAttribute (int index)
		{
			return Reader.GetAttribute (index);
		}

		public override string GetAttribute (string name)
		{
			return Reader.GetAttribute (name);
		}

		public override string GetAttribute (string localName, string namespaceURI)
		{
			return Reader.GetAttribute (localName, namespaceURI);
		}

		public override void MoveToAttribute (int index)
		{
			Reader.MoveToAttribute (index);
		}

		public override bool MoveToAttribute (string name)
		{
			return Reader.MoveToAttribute (name);
		}

		public override bool MoveToAttribute (string localName, string namespaceURI)
		{
			return Reader.MoveToAttribute (localName, namespaceURI);
		}

		public override bool MoveToFirstAttribute ()
		{
			return Reader.MoveToFirstAttribute ();
		}

		public override bool MoveToNextAttribute ()
		{
			return Reader.MoveToNextAttribute ();
		}

		public override string LookupNamespace (string prefix)
		{
			return Reader.LookupNamespace (prefix);
		}

		public override bool ReadAttributeValue ()
		{
			return Reader.ReadAttributeValue ();
		}

		public override void ResolveEntity ()
		{
			Reader.ResolveEntity ();
		}
	}

	class NonInteractiveStateXmlReader : DummyStateXmlReader
	{
		public NonInteractiveStateXmlReader (string baseUri, XmlNameTable nameTable, ReadState readState)
			: base (baseUri, nameTable, readState)
		{
		}

		public override int Depth {
			get { return 0; }
		}

		public override bool HasValue {
			get { return false; }
		}

		public override string Value {
			get { return String.Empty; }
		}

		public override XmlNodeType NodeType {
			get { return XmlNodeType.None; }
		}
	}

	class MultiPartedXmlReader : DummyStateXmlReader
	{
		public MultiPartedXmlReader (XmlReader reader, MimeEncodedStream value)
			: base (reader.BaseURI, reader.NameTable, reader.ReadState)
		{
			this.owner = reader;
			this.value = value.CreateTextReader ().ReadToEnd ();
		}

		XmlReader owner;
		string value;

		public override int Depth {
			get { return owner.Depth; }
		}

		public override bool HasValue {
			get { return true; }
		}

		public override string Value {
			get { return value; }
		}

		public override XmlNodeType NodeType {
			get { return XmlNodeType.Text; }
		}
	}

	abstract class DummyStateXmlReader : XmlReader
	{
		protected DummyStateXmlReader (string baseUri, XmlNameTable nameTable, ReadState readState)
		{
			base_uri = baseUri;
			name_table = nameTable;
			read_state = readState;
		}

		string base_uri;
		XmlNameTable name_table;
		ReadState read_state;

		public override string BaseURI {
			get { return base_uri; }
		}

		public override bool EOF {
			get { return false; }
		}

		public override void Close ()
		{
			throw new NotSupportedException ();
		}

		public override bool Read ()
		{
			throw new NotSupportedException ();
		}

		// The rest are just reader delegation.

		public override int AttributeCount {
			get { return 0; }
		}

		public override bool IsEmptyElement {
			get { return false; }
		}

		public override string LocalName {
			get { return String.Empty; }
		}

		public override string NamespaceURI {
			get { return String.Empty; }
		}

		public override XmlNameTable NameTable {
			get { return name_table; }
		}

		public override string Prefix {
			get { return String.Empty; }
		}

		public override ReadState ReadState {
			get { return read_state; }
		}

		public override bool MoveToElement ()
		{
			return false;
		}

		public override string GetAttribute (int index)
		{
			return null;
		}

		public override string GetAttribute (string name)
		{
			return null;
		}

		public override string GetAttribute (string localName, string namespaceURI)
		{
			return null;
		}

		public override void MoveToAttribute (int index)
		{
			throw new ArgumentOutOfRangeException ();
		}

		public override bool MoveToAttribute (string name)
		{
			return false;
		}

		public override bool MoveToAttribute (string localName, string namespaceURI)
		{
			return false;
		}

		public override bool MoveToFirstAttribute ()
		{
			return false;
		}

		public override bool MoveToNextAttribute ()
		{
			return false;
		}

		public override string LookupNamespace (string prefix)
		{
			return null;
		}

		public override bool ReadAttributeValue ()
		{
			return false;
		}

		public override void ResolveEntity ()
		{
			throw new InvalidOperationException ();
		}
	}

	class MimeEncodedStream
	{
		public MimeEncodedStream (string id, string contentEncoding, string value)
		{
			Id = id;
			ContentEncoding = contentEncoding;
			EncodedString = value;
		}

		public string Id { get; set; }
		public string ContentEncoding { get; set; }
		public string EncodedString { get; set; }

		public string DecodedBase64String {
			get { return Convert.ToBase64String (Encoding.ASCII.GetBytes (EncodedString)); }
		}

		public TextReader CreateTextReader ()
		{
			switch (ContentEncoding) {
			case "7bit":
			case "8bit":
				return new StringReader (EncodedString);
			default:
				return new StringReader (DecodedBase64String);
			}
		}
	}
}
