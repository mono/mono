//
// System.Xml.XmlInputStream 
//	encoding-specification-wise XML input stream and reader
//
// Author:
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
//	(C)2003 Atsushi Enomoto
//
using System;
using System.IO;
using System.Text;

namespace System.Xml
{
	#region XmlStreamReader
	internal class XmlStreamReader : StreamReader
	{
		public XmlStreamReader (XmlInputStream input)
			: base (input, input.ActualEncoding != null ? input.ActualEncoding : Encoding.UTF8)
		{
		}

		public XmlStreamReader (Stream stream)
			: this (new XmlInputStream (stream))
		{
		}

		public XmlStreamReader (string url)
			: this (new XmlInputStream (url))
		{
		}
	}
	#endregion

	public class XmlInputStream : Stream
	{
		Encoding enc;
		Stream stream;
		byte[] buffer = new byte[256];
		int bufLength;
		int bufPos;

		static XmlException encodingException = new XmlException ("invalid encoding specification.");

		public XmlInputStream (string url)
		{
			Stream stream = null;
			try {
				Uri uriObj = new Uri (url);
				stream = new System.Net.WebClient ().OpenRead (url);
			} catch (UriFormatException) {
			}
			if (stream == null)
				stream = File.OpenRead (url);
			Initialize (stream);
		}

		public XmlInputStream (Stream stream)
		{
			Initialize (stream);
		}

		private void Initialize (Stream stream)
		{
			// FIXME: seems too waste...
			MemoryStream ms = new MemoryStream ();
			this.stream = stream;
			int c = stream.ReadByte ();
			switch (c) {
			case 0xFF:
				c = stream.ReadByte ();
				if (c == 0xFE) {
					// BOM-ed little endian utf-16
					enc = Encoding.Unicode;
				} else {
					// It doesn't start from "<?xml" then its encoding is utf-8
					enc = Encoding.UTF8;
					ms.WriteByte ((byte)0xFF);
					ms.WriteByte ((byte)c);
				}
				break;
			case 0xFE:
				c = stream.ReadByte ();
				if (c == 0xFF) {
					// BOM-ed big endian utf-16
					enc = Encoding.BigEndianUnicode;
					return;
				} else {
					// It doesn't start from "<?xml" then its encoding is utf-8
					enc = Encoding.UTF8;
					ms.WriteByte ((byte)0xFE);
					ms.WriteByte ((byte)c);
				}
				break;
			case 0xEF:
				enc = Encoding.UTF8;
				c = ReadByte ();
				if (c == 0xBB) {
					c = ReadByte ();
					if (c != 0xBF) {
						ms.WriteByte ((byte)0xEF);
						ms.WriteByte ((byte)0xBB);
						ms.WriteByte ((byte)c);
					}
				} else {
					ms.WriteByte ((byte)0xEF);
				}
				break;
			case '<':
				// try to get encoding name from XMLDecl.
				ms.WriteByte ((byte)'<');
				int size = stream.Read (buffer, 1, 4);
				ms.Write (buffer, 1, 4);
				if (Encoding.ASCII.GetString (buffer, 1, 4) == "?xml") {
					int loop = 0;
					c = SkipWhitespace (ms);
					// version
					if (c != 'v' || stream.ReadByte () != 'e')
						throw new XmlException ("invalid xml declaration.");
					ms.WriteByte ((byte)'v');
					ms.WriteByte ((byte)'e');
					while (loop++ >= 0) {
						c = stream.ReadByte ();
						ms.WriteByte ((byte)c);
						if (c == '0') {
							ms.WriteByte ((byte)stream.ReadByte ());
							break;
						}
					}
					c = SkipWhitespace (ms);
					if (c == 'e') {
						ms.WriteByte ((byte)'e');
						size = stream.Read (buffer, 0, 7);
						ms.Write (buffer, 0, size);
						if (size == 7 && Encoding.ASCII.GetString(buffer, 0, 7) == "ncoding") {
							c = this.SkipWhitespace(ms);
							if (c != '=')
								throw encodingException;
							ms.WriteByte ((byte)'=');
							c = this.SkipWhitespace (ms);
							int quoteChar = c;
							ms.WriteByte ((byte)c);
							int start = (int)ms.Position;
							while (loop++ >= 0) {
								c = stream.ReadByte ();
								if (c == quoteChar)
									break;
								else if (c < 0)
									throw encodingException;
								ms.WriteByte ((byte)c);
							}
							string encodingName = Encoding.UTF8.GetString (ms.GetBuffer (), start, (int)ms.Position - start);
							if (!XmlConstructs.IsValidIANAEncoding (encodingName))
								throw encodingException;
							ms.WriteByte ((byte)quoteChar);
							enc = Encoding.GetEncoding (encodingName);
						}
					}
					else
						ms.WriteByte ((byte)c);
				}

				if (enc == null)
					enc = Encoding.Default;

				buffer = ms.ToArray ();
				bufLength = buffer.Length;
				bufPos = 0;
				break;
			default:
				buffer [0] = (byte)c;
				bufLength = 1;
				enc = Encoding.UTF8;
				break;
			}
		}

		// skips whitespace and returns misc char that was read from stream
		private int SkipWhitespace (MemoryStream ms)	// ms may be null
		{
			int loop = 0;
			int c;
			while (loop++ >= 0) { // defends infinite loop (expecting overflow)
				c = stream.ReadByte ();
				switch (c) {
				case '\r': goto case ' ';
				case '\n': goto case ' ';
				case '\t': goto case ' ';
				case ' ':
					if (ms != null)
						ms.WriteByte ((byte)c);
					continue;
				default:
					return c;
				}
			}
			throw new InvalidOperationException ();
		}

		public Encoding ActualEncoding {
			get { return enc; }
		}

		#region Public Overrides
		public override bool CanRead {
			get { return stream.CanRead; }
		}

		public override bool CanSeek {
			get { return stream.CanSeek; }
		}

		public override bool CanWrite {
			get { return false; }
		}

		public override long Length {
			get {
				return stream.Length;
			}
		}

		public override long Position {
			get {
				return stream.Position + bufLength;
			}
			set {
				if(value < bufLength)
					bufPos = (int)value;
				else
					stream.Position = value - bufLength;
			}
		}

		public override void Flush()
		{
			stream.Flush ();
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			int ret;
			if (count <= bufLength - bufPos)	{	// all from buffer
				Array.Copy (this.buffer, bufPos, buffer, offset, count);
				bufPos += count;
				ret = count;
			} else {
				int bufRest = bufLength - bufPos;
				if (bufLength > bufPos) {
					Array.Copy (this.buffer, bufPos, buffer, offset, bufRest);
					bufPos += bufRest;
				}
				ret = bufRest +
					stream.Read (buffer, offset + bufRest, count - bufRest);
			}
			return ret;
		}

		public override int ReadByte ()
		{
			if (bufLength > bufPos) {
				return buffer [bufPos++];
			}
			return stream.ReadByte ();
		}

		public override long Seek (long offset, System.IO.SeekOrigin origin)
		{
			int bufRest = bufLength - bufPos;
			if (origin == SeekOrigin.Current)
				if (offset < bufRest)
					return buffer [bufPos + offset];
				else
					return stream.Seek (offset - bufRest, origin);
			else
				return stream.Seek (offset, origin);
		}

		public override void SetLength (long value)
		{
			stream.SetLength (value);
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException ();
		}
		#endregion
	}
}
