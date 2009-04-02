//
// XmlMtomDictionaryWriter.cs
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
using System.IO;
using System.Net.Mime;
using System.Text;

namespace System.Xml
{
	internal class XmlMtomDictionaryWriter : XmlDictionaryWriter
	{
		public XmlMtomDictionaryWriter (Stream stream, Encoding encoding, int maxSizeInBytes, string startInfo, string boundary, string startUri, bool writeMessageHeaders, bool ownsStream)
		{
			writer = new StreamWriter (stream, encoding);
			max_bytes = maxSizeInBytes;
			write_headers = writeMessageHeaders;
			owns_stream = ownsStream;

			var settings = new XmlWriterSettings ();
			settings.Encoding = encoding;
			settings.OmitXmlDeclaration = true;
			xml_writer_settings = settings;

			// FIXME: actually it does not likely use ContentType.ToString() but writes those header items by own.
			// (so that it could generate "start" header dynamically)
			var c = new ContentType ("multipart/related");
			c.Parameters ["type"] = "application/xop+xml";
			c.Boundary = boundary;
			c.Parameters ["start"] = "<" + startUri + ">";
			c.Parameters ["start-info"] = startInfo;
			content_type = c;
		}

		// constructor arguments
		TextWriter writer;
		XmlWriterSettings xml_writer_settings;
		Encoding encoding;
		int max_bytes;
		bool write_headers;
		bool owns_stream;
		ContentType content_type;

		// state
		XmlWriter w;
		int depth;
		int section_count;

		XmlWriter CreateWriter ()
		{
			return XmlWriter.Create (writer, xml_writer_settings);
		}

		public override void Close ()
		{
			w.Close ();
			if (owns_stream)
				writer.Close ();
		}

		public override void Flush ()
		{
			w.Flush ();
		}

		public override string LookupPrefix (string namespaceUri)
		{
			return w.LookupPrefix (namespaceUri);
		}

		public override void WriteBase64 (byte [] bytes, int start, int length)
		{
			CheckState ();
			w.WriteBase64 (bytes, start, length);
		}

		public override void WriteCData (string text)
		{
			CheckState ();
			w.WriteCData (text);
		}

		public override void WriteCharEntity (char c)
		{
			CheckState ();
			w.WriteCharEntity (c);
		}

		public override void WriteChars (char [] buffer, int index, int count)
		{
			CheckState ();
			w.WriteChars (buffer, index, count);
		}

		public override void WriteComment (string comment)
		{
			CheckState ();
			w.WriteComment (comment);
		}

		public override void WriteDocType (string name, string pubid, string sysid, string intSubset)
		{
			throw new NotSupportedException (); // indeed
		}

		public override void WriteEndAttribute ()
		{
			w.WriteEndAttribute ();
		}

		public override void WriteEndDocument ()
		{
			w.WriteEndDocument ();
		}

		public override void WriteEndElement ()
		{
			w.WriteEndElement ();
			if (--depth == 0)
				WriteEndOfMimeSection ();
		}

		public override void WriteEntityRef (string name)
		{
			w.WriteEntityRef (name);
		}

		public override void WriteFullEndElement ()
		{
			w.WriteFullEndElement ();
			if (--depth == 0)
				WriteEndOfMimeSection ();
		}

		public override void WriteProcessingInstruction (string name, string data)
		{
			throw new NotSupportedException ();
		}

		public override void WriteRaw (string raw)
		{
			CheckState ();
			w.WriteRaw (raw);
		}

		public override void WriteRaw (char [] chars, int index, int count)
		{
			CheckState ();
			w.WriteRaw (chars, index, count);
		}

		public override void WriteStartAttribute (string prefix, string localName, string namespaceURI)
		{
			CheckState ();
			w.WriteStartAttribute (prefix, localName, namespaceURI);
		}

		public override void WriteStartDocument ()
		{
			CheckState ();
			w.WriteStartDocument ();
		}

		public override void WriteStartDocument (bool standalone)
		{
			CheckState ();
			w.WriteStartDocument (standalone);
		}

		public override void WriteStartElement (string prefix, string localName, string namespaceURI)
		{
			CheckState ();

			if (depth == 0)
				WriteStartOfMimeSection ();

			w.WriteStartElement (prefix, localName, namespaceURI);
			depth++;
		}

		public override WriteState WriteState {
			get { return w.WriteState; }
		}

		public override void WriteString (string text)
		{
			CheckState ();

			int i1, i2 = 0;
			do {
				i1 = text.IndexOf ('\r', i2);
				if (i1 >= 0) {
					w.WriteString (text.Substring (i2, i1 - i2));
					WriteCharEntity ('\r');
					i2 = i1 + 1;
				} else {
					w.WriteString (text.Substring (i2));
					break;
				}
			} while (true);
		}

		public override void WriteSurrogateCharEntity (char low, char high)
		{
			CheckState ();
			w.WriteSurrogateCharEntity (low, high);
		}

		public override void WriteWhitespace (string text)
		{
			CheckState ();
			w.WriteWhitespace (text);
		}

		public override string XmlLang {
			get { return w.XmlLang; }
		}

		public override XmlSpace XmlSpace {
			get { return w.XmlSpace; }
		}

		void CheckState ()
		{
			if (w == null && write_headers)
				WriteMimeHeaders ();
			if (w == null || w.WriteState == WriteState.Closed || w.WriteState == WriteState.Error)
				w = CreateWriter ();
		}

		void WriteMimeHeaders ()
		{
			writer.Write ("MIME-Version: 1.0\r\n");
			writer.Write ("Content-Type: ");
			writer.Write (content_type.ToString ());
			writer.Write ("\r\n\r\n\r\n");
		}

		void WriteStartOfMimeSection ()
		{
			section_count++;

			// I'm not sure what's the expected behavior of this
			// strange XmlWriter, but so far - it outputs only one
			// section.
			if (section_count > 1)
				return;

			writer.Write ("\r\n");
			writer.Write ("--");
			writer.Write (content_type.Boundary);
			writer.Write ("\r\n");
			writer.Write ("Content-ID: ");
			writer.Write (content_type.Parameters ["start"]);
			writer.Write ("\r\n");
			writer.Write ("Content-Transfer-Encoding: 8bit\r\n");
			writer.Write ("Content-Type: application/xop+xml;charset=");
			writer.Write (xml_writer_settings.Encoding.HeaderName);
			writer.Write (";type=\"");
			writer.Write (content_type.Parameters ["start-info"].Replace ("\"", "\\\""));
			writer.Write ("\"\r\n\r\n");
		}

		void WriteEndOfMimeSection ()
		{
			// I'm not sure what's the expected behavior of this
			// strange XmlWriter, but so far - it outputs only one
			// section.
			if (section_count > 1)
				return;

			writer.Write ("\r\n");
			writer.Write ("--");
			writer.Write (content_type.Boundary);
			writer.Write ("--\r\n");
		}
	}
}

