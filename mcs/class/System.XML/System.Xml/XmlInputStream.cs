//
// System.Xml.XmlInputStream 
//	encoding-specification-wise XML input stream and reader
//
// Author:
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
//	(C)2003 Atsushi Enomoto
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
using System.IO;
using System.Text;
using System.Xml;

namespace System.Xml
{
	#region XmlStreamReader
	internal class XmlStreamReader : StreamReader
	{
		XmlInputStream input;

		XmlStreamReader (XmlInputStream input)
			: base (input, input.ActualEncoding != null ? input.ActualEncoding : Encoding.UTF8)
		{
			this.input = input;
		}

		public XmlStreamReader (Stream input)
			: this (new XmlInputStream (input))
		{
		}

		public override void Close ()
		{
			this.input.Close ();
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			if (disposing) {
				Close ();
			}
		}

	}
	#endregion

	class XmlInputStream : Stream
	{
		Encoding enc;
		Stream stream;
		byte[] buffer;
		int bufLength;
		int bufPos;

		static XmlException encodingException = new XmlException ("invalid encoding specification.");

		public XmlInputStream (Stream stream)
		{
			Initialize (stream);
		}

		private void Initialize (Stream stream)
		{
			buffer = new byte [64];
			this.stream = stream;
			enc = Encoding.UTF8; // Default to UTF8 if we can't guess it
			bufLength = stream.Read (buffer, 0, buffer.Length);
			if (bufLength == -1 || bufLength == 0) {
				return;
			}

			int c = ReadByteSpecial ();
			switch (c) {
			case 0xFF:
				c = ReadByteSpecial ();
				if (c == 0xFE) {
					// BOM-ed little endian utf-16
					enc = Encoding.Unicode;
				} else {
					// It doesn't start from "<?xml" then its encoding is utf-8
					bufPos = 0;
				}
				break;
			case 0xFE:
				c = ReadByteSpecial ();
				if (c == 0xFF) {
					// BOM-ed big endian utf-16
					enc = Encoding.BigEndianUnicode;
					return;
				} else {
					// It doesn't start from "<?xml" then its encoding is utf-8
					bufPos = 0;
				}
				break;
			case 0xEF:
				c = ReadByteSpecial ();
				if (c == 0xBB) {
					c = ReadByteSpecial ();
					if (c != 0xBF) {
						bufPos = 0;
					}
				} else {
					buffer [--bufPos] = 0xEF;
				}
				break;
			case '<':
				// try to get encoding name from XMLDecl.
				if (bufLength >= 5 && Encoding.ASCII.GetString (buffer, 1, 4) == "?xml") {
					bufPos += 4;
					c = SkipWhitespace ();

					// version. It is optional here.
					if (c == 'v') {
						while (c >= 0) {
							c = ReadByteSpecial ();
							if (c == '0') { // 0 of 1.0
								ReadByteSpecial ();
								break;
							}
						}
						c = SkipWhitespace ();
					}

					if (c == 'e') {
						int remaining = bufLength - bufPos;
						if (remaining >= 7 && Encoding.ASCII.GetString(buffer, bufPos, 7) == "ncoding") {
							bufPos += 7;
							c = SkipWhitespace();
							if (c != '=')
								throw encodingException;
							c = SkipWhitespace ();
							int quoteChar = c;
							StringBuilder sb = new StringBuilder ();
							while (true) {
								c = ReadByteSpecial ();
								if (c == quoteChar)
									break;
								else if (c < 0)
									throw encodingException;

								sb.Append ((char) c);
							}
							string encodingName = sb.ToString ();
							if (!XmlChar.IsValidIANAEncoding (encodingName))
								throw encodingException;
							enc = Encoding.GetEncoding (encodingName);
						}
					}
				}
				bufPos = 0;
				break;
			default:
				bufPos = 0;
				break;
			}
		}

		// Just like readbyte, but grows the buffer too.
		int ReadByteSpecial ()
		{
			if (bufLength > bufPos)
				return buffer [bufPos++];

			byte [] newbuf = new byte [buffer.Length * 2];
			Buffer.BlockCopy (buffer, 0, newbuf, 0, bufLength);
			int nbytes = stream.Read (newbuf, bufLength, buffer.Length);
			if (nbytes == -1 || nbytes == 0)
				return -1;
				
			bufLength += nbytes;
			buffer = newbuf;
			return buffer [bufPos++];
		}

		// skips whitespace and returns misc char that was read from stream
		private int SkipWhitespace ()
		{
			int c;
			while (true) {
				c = ReadByteSpecial ();
				switch ((char) c) {
				case '\r': goto case ' ';
				case '\n': goto case ' ';
				case '\t': goto case ' ';
				case ' ':
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
			get {
				if (bufLength > bufPos)
					return true;
				else
					return stream.CanRead; 
			}
		}

		// FIXME: It should support base stream's CanSeek.
		public override bool CanSeek {
			get { return false; } // stream.CanSeek; }
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
				return stream.Position - bufLength + bufPos;
			}
			set {
				if(value < bufLength)
					bufPos = (int)value;
				else
					stream.Position = value - bufLength;
			}
		}

		public override void Close ()
		{
			stream.Close ();
		}

		public override void Flush ()
		{
			stream.Flush ();
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			int ret;
			if (count <= bufLength - bufPos)	{	// all from buffer
				Buffer.BlockCopy (this.buffer, bufPos, buffer, offset, count);
				bufPos += count;
				ret = count;
			} else {
				int bufRest = bufLength - bufPos;
				if (bufLength > bufPos) {
					Buffer.BlockCopy (this.buffer, bufPos, buffer, offset, bufRest);
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
