//
// JsonWriter.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace System.Runtime.Serialization.Json
{
	class JsonWriter : XmlDictionaryWriter, IXmlJsonWriterInitializer
	{
		enum ElementType
		{
			None,
			Object,
			Array,
			String,
			Number,
			Boolean,
		}

		Stream output;
		bool close_output;
		WriteState state;
		Stack<ElementType> element_kinds = new Stack<ElementType> ();
		Stack<bool> first_content_flags = new Stack<bool> ();
		string attr_name, attr_value, runtime_type;
		Encoding encoding;
		byte [] encbuf = new byte [1024];
		bool no_string_yet = true, is_null, is_ascii_single;

		public JsonWriter (Stream stream, Encoding encoding, bool closeOutput)
		{
			SetOutput (stream, encoding, closeOutput);
		}

		public void SetOutput (Stream stream, Encoding encoding, bool ownsStream)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");
			if (encoding == null)
				throw new ArgumentNullException ("encoding");
			output = stream;
			this.encoding = encoding;
			close_output = ownsStream;
#if MOONLIGHT
			is_ascii_single = encoding is UTF8Encoding;
#else
			is_ascii_single = encoding is UTF8Encoding || encoding.IsSingleByte;
#endif
		}

		void CheckState ()
		{
			switch (state) {
			case WriteState.Closed:
			case WriteState.Error:
				throw new InvalidOperationException (String.Format ("This XmlDictionaryReader is already at '{0}' state", state));
			}
		}

		// copied from System.Silverlight JavaScriptSerializer.
		static string EscapeStringLiteral (string input)
		{
			StringBuilder sb = null;
			int i = 0, start = 0;
			for (; i < input.Length; i++) {
				switch (input [i]) {
				case '"':
					AppendBuffer (ref sb, input, start, i, @"\""");
					break;
				case '\\':
					AppendBuffer (ref sb, input, start, i, @"\\");
					break;
				case '/':
					AppendBuffer (ref sb, input, start, i, @"\/");
					break;
				case '\x8':
					AppendBuffer (ref sb, input, start, i, @"\b");
					break;
				case '\f':
					AppendBuffer (ref sb, input, start, i, @"\f");
					break;
				case '\n':
					AppendBuffer (ref sb, input, start, i, /*@"\n"*/@"\u000a");
					break;
				case '\r':
					AppendBuffer (ref sb, input, start, i, /*@"\r"*/@"\u000d");
					break;
				case '\t':
					AppendBuffer (ref sb, input, start, i, /*@"\t"*/@"\u0009");
					break;
				default:
					continue;
				}
				start = i + 1;
			}
			string remaining = input.Substring (start, i - start);
			if (sb != null)
				return sb.Append (remaining).ToString ();
			else
				return remaining;
		}

		static void AppendBuffer (ref StringBuilder sb, string input, int start, int i, string append)
		{
			if (sb == null)
				sb = new StringBuilder ();
			if (i != start)
				sb.Append (input, start, i - start);
			sb.Append (append);
		}

		public override WriteState WriteState {
			get { return state; }
		}

		public override void Close ()
		{
			// close all open elements
			while (element_kinds.Count > 0)
				WriteEndElement ();

			if (close_output)
				output.Close ();
			else
				output.Flush ();

			state = WriteState.Closed;
		}

		public override void Flush ()
		{
			output.Flush ();
		}

		public override void WriteStartElement (string prefix, string localName, string ns)
		{
			CheckState ();

			if (localName == null)
				throw new ArgumentNullException ("localName");
			else if (localName.Length == 0)
				throw new ArgumentException ("Empty string is not a valid localName in this XmlDictionaryWriter");

			if (!String.IsNullOrEmpty (ns))
				throw new ArgumentException ("Non-empty namespace URI is not allowed in this XmlDictionaryWriter");
			if (!String.IsNullOrEmpty (prefix))
				throw new ArgumentException ("Non-empty prefix is not allowed in this XmlDictionaryWriter");

			if (state == WriteState.Attribute)
				WriteEndAttribute ();
			if (state == WriteState.Element)
				CloseStartElement ();

			else if (state != WriteState.Start && element_kinds.Count == 0)
				throw new XmlException ("This XmlDictionaryWriter does not support multiple top-level elements");

			if (element_kinds.Count == 0) {
				if (localName != "root")
					throw new XmlException ("Only 'root' is allowed for the name of the top-level element");
			} else {
				switch (element_kinds.Peek ()) {
				case ElementType.Array:
					if (localName != "item")
						throw new XmlException ("Only 'item' is allowed as a content element of an array");
					break;
				case ElementType.String:
					throw new XmlException ("Mixed content is not allowed in this XmlDictionaryWriter");
				case ElementType.None:
					throw new XmlException ("Before writing a child element, an element needs 'type' attribute to indicate whether the element is a JSON array or a JSON object in this XmlDictionaryWriter");
				}

				if (first_content_flags.Peek ()) {
					first_content_flags.Pop ();
					first_content_flags.Push (false);
				}
				else
					OutputAsciiChar (',');

				if (element_kinds.Peek () != ElementType.Array) {
					OutputAsciiChar ('"');
					OutputString (localName);
					OutputAsciiChar ('\"');
					OutputAsciiChar (':');
				}
			}

			element_kinds.Push (ElementType.None); // undetermined yet

			state = WriteState.Element;
		}

		public override void WriteEndElement ()
		{
			CheckState ();

			if (state == WriteState.Attribute)
				throw new XmlException ("Cannot end element when an attribute is being written");
			if (state == WriteState.Element)
				CloseStartElement ();

			if (element_kinds.Count == 0)
				throw new XmlException ("There is no open element to close");
			switch (element_kinds.Pop ()) {
			case ElementType.String:
				if (!is_null) {
					if (no_string_yet)
						OutputAsciiChar ('"');
					OutputAsciiChar ('"');
				}
				no_string_yet = true;
				is_null = false;
				break;
			case ElementType.Array:
				OutputAsciiChar (']');
				break;
			case ElementType.Object:
				OutputAsciiChar ('}');
				break;
			}

			// not sure if it is correct though ...
			state = WriteState.Content;
			first_content_flags.Pop ();
		}

		public override void WriteFullEndElement ()
		{
			WriteEndElement (); // no such difference in JSON.
		}

		public override void WriteStartAttribute (string prefix, string localName, string ns)
		{
			CheckState ();

			if (state != WriteState.Element)
				throw new XmlException ("Cannot write attribute as this XmlDictionaryWriter is not at element state");

			if (!String.IsNullOrEmpty (ns))
				throw new ArgumentException ("Non-empty namespace URI is not allowed in this XmlDictionaryWriter");
			if (!String.IsNullOrEmpty (prefix))
				throw new ArgumentException ("Non-empty prefix is not allowed in this XmlDictionaryWriter");

			if (localName != "type" && localName != "__type")
				throw new ArgumentException ("Only 'type' and '__type' are allowed as an attribute name in this XmlDictionaryWriter");

			if (state != WriteState.Element)
				throw new InvalidOperationException (String.Format ("Attribute cannot be written in {0} mode", state));

			attr_name = localName;
			state = WriteState.Attribute;
		}

		public override void WriteEndAttribute ()
		{
			CheckState ();

			if (state != WriteState.Attribute)
				throw new XmlException ("Cannot close attribute, as this XmlDictionaryWriter is not at attribute state");

			if (attr_name == "type") {
				switch (attr_value) {
				case "object":
					element_kinds.Pop ();
					element_kinds.Push (ElementType.Object);
					OutputAsciiChar ('{');
					break;
				case "array":
					element_kinds.Pop ();
					element_kinds.Push (ElementType.Array);
					OutputAsciiChar ('[');
					break;
				case "number":
					element_kinds.Pop ();
					element_kinds.Push (ElementType.Number);
					break;
				case "boolean":
					element_kinds.Pop ();
					element_kinds.Push (ElementType.Boolean);
					break;
				case "string":
					element_kinds.Pop ();
					element_kinds.Push (ElementType.String);
					break;
				default:
					throw new XmlException (String.Format ("Unexpected type attribute value '{0}'", attr_value));
				}
			}
			else
				runtime_type = attr_value;

			state = WriteState.Element;
			attr_value = null;
		}

		void CloseStartElement ()
		{
			if (element_kinds.Peek () == ElementType.None) {
				element_kinds.Pop ();
				element_kinds.Push (ElementType.String);
				no_string_yet = true;
				is_null = false;
			}

			first_content_flags.Push (true);

			if (runtime_type != null) {
				OutputString ("\"__type\":\"");
				OutputString (runtime_type);
				OutputAsciiChar ('\"');
				runtime_type = null;
				first_content_flags.Pop ();
				first_content_flags.Push (false);
			}
		}

		public override void WriteString (string text)
		{
			CheckState ();

			if (state == WriteState.Start)
				throw new InvalidOperationException ("Top-level content string is not allowed in this XmlDictionaryWriter");

			if (state == WriteState.Element) {
				CloseStartElement ();
				state = WriteState.Content;
			}

			if (state == WriteState.Attribute)
				attr_value += text;
			else if (text == null) {
				no_string_yet = false;
				is_null = true;
				OutputString ("null");
			} else {
				switch (element_kinds.Peek ()) {
				case ElementType.String:
					if (no_string_yet) {
						OutputAsciiChar ('"');
						no_string_yet = false;
					}
					break;
				case ElementType.Number:
					// .NET is buggy here, it just outputs raw string, which results in invalid JSON format.
					bool isString = false;
					switch (text) {
					case "INF":
					case "-INF":
					case "NaN":
						isString = true;
						break;
					}
					if (isString) {
						element_kinds.Pop ();
						element_kinds.Push (ElementType.String);
						goto case ElementType.String;
					}
					break;
				case ElementType.Boolean:
					break;
				default:
					throw new XmlException (String.Format ("Simple content string is allowed only for string, number and boolean types and not for {0} type", element_kinds.Peek ()));
				}

				OutputString (EscapeStringLiteral (text));
			}
		}

		#region mostly-ignored operations

		public override string LookupPrefix (string ns)
		{
			// Since there is no way to declare namespaces in
			// this writer, it always returns fixed results.
			if (ns == null)
				throw new ArgumentNullException ("ns");
			else if (ns.Length == 0)
				return String.Empty;
			else if (ns == "http://www.w3.org/2000/xmlns/")
				return "xmlns";
			else if (ns == "http://www.w3.org/XML/1998/namespace")
				return "xml";
			return null;
		}

		public override void WriteStartDocument ()
		{
			CheckState ();
		}

		public override void WriteStartDocument (bool standalone)
		{
			CheckState ();
		}

		public override void WriteEndDocument ()
		{
			CheckState ();
		}

		#endregion

		#region unsupported operations

		public override void WriteDocType (string name, string pubid, string sysid, string intSubset)
		{
			CheckState ();

			throw new NotSupportedException ("This XmlDictionaryWriter does not support writing doctype declaration");
		}

		public override void WriteComment (string text)
		{
			CheckState ();

			throw new NotSupportedException ("This XmlDictionaryWriter does not support writing comment");
		}

		public override void WriteEntityRef (string text)
		{
			CheckState ();

			throw new NotSupportedException ("This XmlDictionaryWriter does not support writing entity reference");
		}

		public override void WriteProcessingInstruction (string target, string data)
		{
			CheckState ();

			if (String.Compare (target, "xml", StringComparison.OrdinalIgnoreCase) != 0)
				throw new ArgumentException ("This XmlDictionaryWriter does not support writing processing instruction");
		}

		#endregion

		#region WriteString() variants

		public override void WriteRaw (string text)
		{
#if MOONLIGHT
			OutputString (text);
#else
			WriteString (text);
#endif
		}

		public override void WriteRaw (char [] chars, int start, int length)
		{
			WriteChars (chars, start, length);
		}

		public override void WriteCData (string text)
		{
			WriteString (text);
		}

		public override void WriteCharEntity (char entity)
		{
			WriteString (entity.ToString ());
		}

		public override void WriteChars (char [] chars, int start, int length)
		{
			WriteString (new string (chars, start, length));
		}

		public override void WriteSurrogateCharEntity (char high, char low)
		{
			WriteChars (new char [] {high, low}, 0, 2);
		}

		public override void WriteBase64 (byte [] bytes, int start, int length)
		{
			WriteString (Convert.ToBase64String (bytes, start, length));
		}

		public override void WriteWhitespace (string text)
		{
			if (text == null)
				throw new ArgumentNullException ("text");
			for (int i = 0; i < text.Length; i++) {
				if (text [i] != ' ') {
					for (int j = i; j < text.Length; j++) {
						switch (text [j]) {
						case '\t':
						case ' ':
						case '\n':
						case '\r':
							continue;
						default:
							throw new ArgumentException (String.Format ("WriteWhitespace() does not accept non-whitespace character '{0}'", text [j]));
						}
					}
					break;
				}
			}
			WriteString (text);
		}

		char [] char_buf = new char [1];
		void OutputAsciiChar (char c)
		{
			if (is_ascii_single)
				output.WriteByte ((byte) c);
			else {
				char_buf [0] = c;
				int size = encoding.GetBytes (char_buf, 0, 1, encbuf, 0);
				output.Write (encbuf, 0, size);
			}
		}

		void OutputString (string s)
		{
			int size = encoding.GetByteCount (s);
			if (encbuf.Length < size)
				encbuf = new byte [size];
			size = encoding.GetBytes (s, 0, s.Length, encbuf, 0);
			output.Write (encbuf, 0, size);
		}

		#endregion
	}
}
