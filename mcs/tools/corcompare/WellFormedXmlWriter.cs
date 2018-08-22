//
// WellFormedXmlWriter.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc. http://www.novell.com
//
using System;
using System.Globalization;
using System.Collections;
using System.Xml;

namespace Mono.ApiTools {

	class WellFormedXmlWriter : DefaultXmlWriter
	{
		public static bool IsInvalid (int ch)
		{
			switch (ch) {
			case 9:
			case 10:
			case 13:
				return false;
			}
			if (ch < 32)
				return true;
			if (ch < 0xD800)
				return false;
			if (ch < 0xE000)
				return true;
			if (ch < 0xFFFE)
				return false;
			if (ch < 0x10000)
				return true;
			if (ch < 0x110000)
				return false;
			else
				return true;
		}

		public static int IndexOfInvalid (string s, bool allowSurrogate)
		{
			for (int i = 0; i < s.Length; i++)
				if (IsInvalid (s [i])) {
					if (!allowSurrogate ||
					    i + 1 == s.Length ||
					    s [i] < '\uD800' ||
					    s [i] >= '\uDC00' ||
					    s [i + 1] < '\uDC00' ||
					    s [i + 1] >= '\uE000')
						return i;
					i++;
				}
			return -1;
		}

		public static int IndexOfInvalid (char [] s, int start, int length, bool allowSurrogate)
		{
			int end = start + length;
			if (s.Length < end)
				throw new ArgumentOutOfRangeException ("length");
			for (int i = start; i < end; i++)
				if (IsInvalid (s [i])) {
					if (!allowSurrogate ||
					    i + 1 == end ||
					    s [i] < '\uD800' ||
					    s [i] >= '\uDC00' ||
					    s [i + 1] < '\uDC00' ||
					    s [i + 1] >= '\uE000')
						return i;
					i++;
				}
			return -1;
		}

		public WellFormedXmlWriter (XmlWriter writer) : base (writer)
		{
		}

		public override void WriteString (string text)
		{
			int i = IndexOfInvalid (text, true);
			if (i >= 0) {
				char [] arr = text.ToCharArray ();
				Writer.WriteChars (arr, 0, i);
				WriteChars (arr, i, arr.Length - i);
			} else {
				// no invalid character.
				Writer.WriteString (text);
			}
		}

		public override void WriteChars (char [] text, int idx, int length)
		{
			int start = idx;
			int end = idx + length;
			while ((idx = IndexOfInvalid (text, start, length, true)) >= 0) {
				if (start < idx)
					Writer.WriteChars (text, start, idx - start);
				Writer.WriteString (String.Format (CultureInfo.InvariantCulture,
					text [idx] < 0x80 ? "\\x{0:X02}" : "\\u{0:X04}",
					(int) text [idx]));
				length -= idx - start + 1;
				start = idx + 1;
			}
			if (start < end)
				Writer.WriteChars (text, start, end - start);
		}

	}

	class DefaultXmlWriter : XmlWriter
	{
		XmlWriter writer;

		public DefaultXmlWriter (XmlWriter writer)
		{
			this.writer = writer;
		}

		protected XmlWriter Writer {
			get { return writer; }
		}

		public override void Close ()
		{
			writer.Close ();
		}

		public override void Flush ()
		{
			writer.Flush ();
		}

		public override string LookupPrefix (string ns)
		{
			return writer.LookupPrefix (ns);
		}

		public override void WriteBase64 (byte [] buffer, int index, int count)
		{
			writer.WriteBase64 (buffer, index, count);
		}

		public override void WriteBinHex (byte [] buffer, int index, int count)
		{
			writer.WriteBinHex (buffer, index, count);
		}

		public override void WriteCData (string text)
		{
			writer.WriteCData (text);
		}

		public override void WriteCharEntity (char ch)
		{
			writer.WriteCharEntity (ch);
		}

		public override void WriteChars (char [] buffer, int index, int count)
		{
			writer.WriteChars (buffer, index, count);
		}

		public override void WriteComment (string text)
		{
			writer.WriteComment (text);
		}

		public override void WriteDocType (string name, string pubid, string sysid, string subset)
		{
			writer.WriteDocType (name, pubid, sysid, subset);
		}

		public override void WriteEndAttribute ()
		{
			writer.WriteEndAttribute ();
		}

		public override void WriteEndDocument ()
		{
			writer.WriteEndDocument ();
		}

		public override void WriteEndElement ()
		{
			writer.WriteEndElement ();
		}

		public override void WriteEntityRef (string name)
		{
			writer.WriteEntityRef (name);
		}

		public override void WriteFullEndElement ()
		{
			writer.WriteFullEndElement ();
		}

		public override void WriteName (string name)
		{
			writer.WriteName (name);
		}

		public override void WriteNmToken (string name)
		{
			writer.WriteNmToken (name);
		}

		public override void WriteNode (XmlReader reader, bool defattr)
		{
			writer.WriteNode (reader, defattr);
		}

		public override void WriteProcessingInstruction (string name, string text)
		{
			writer.WriteProcessingInstruction (name, text);
		}

		public override void WriteQualifiedName (string localName, string ns)
		{
			writer.WriteQualifiedName (localName, ns);
		}

		public override void WriteRaw (string data)
		{
			writer.WriteRaw (data);
		}

		public override void WriteRaw (char [] buffer, int index, int count)
		{
			writer.WriteRaw (buffer, index, count);
		}

		public override void WriteStartAttribute (string prefix, string localName, string ns)
		{
			writer.WriteStartAttribute (prefix, localName, ns);
		}

		public override void WriteStartDocument (bool standalone)
		{
			writer.WriteStartDocument (standalone);
		}

		public override void WriteStartDocument ()
		{
			writer.WriteStartDocument ();
		}

		public override void WriteStartElement (string prefix, string localName, string ns)
		{
			writer.WriteStartElement (prefix, localName, ns);
		}

		public override void WriteString (string text)
		{
			writer.WriteString (text);
		}

		public override void WriteSurrogateCharEntity (char lowChar, char highChar)
		{
			writer.WriteSurrogateCharEntity (lowChar, highChar);
		}

		public override void WriteWhitespace (string ws)
		{
			writer.WriteWhitespace (ws);
		}

		public override WriteState WriteState {
			get {
				return writer.WriteState;
			}
		}

		public override string XmlLang {
			get {
				return writer.XmlLang;
			}
		}

		public override XmlSpace XmlSpace {
			get {
				return writer.XmlSpace;
			}
		}
	}
}