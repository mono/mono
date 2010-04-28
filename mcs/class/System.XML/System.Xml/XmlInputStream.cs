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
using System.Runtime.InteropServices;

namespace System.Xml
{
	#region XmlStreamReader
	internal class XmlStreamReader : NonBlockingStreamReader
	{
		XmlInputStream input;

		XmlStreamReader (XmlInputStream input)
			: base (input, input.ActualEncoding != null ? input.ActualEncoding : XmlInputStream.StrictUTF8)
		{
			this.input = input;
		}

		public XmlStreamReader (Stream input)
			: this (new XmlInputStream (input))
		{
		}

		static XmlException invalidDataException = new XmlException ("invalid data.");

		public override void Close ()
		{
			this.input.Close ();
		}

		public override int Read ([In, Out] char[] dest_buffer, int index, int count)
		{
			try {
				return base.Read (dest_buffer, index, count);
			}
#if NET_1_1
			catch (System.ArgumentException) {
				throw invalidDataException;
			}
#else
			catch (System.Text.DecoderFallbackException) {
				throw invalidDataException;
			}
#endif
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

	#region NonBlockingStreamReader
	// mostly copied from StreamReader, removing BOM checks, ctor
	// parameter checks and some extra public members.
	internal class NonBlockingStreamReader : TextReader {

		const int DefaultBufferSize = 1024;
		const int DefaultFileBufferSize = 4096;
		const int MinimumBufferSize = 128;

		//
		// The input buffer
		//
		byte [] input_buffer;

		//
		// The decoded buffer from the above input buffer
		//
		char [] decoded_buffer;

		//
		// Decoded bytes in decoded_buffer.
		//
		int decoded_count;

		//
		// Current position in the decoded_buffer
		//
		int pos;

		//
		// The buffer size that we are using
		//
		int buffer_size;

		Encoding encoding;
		Decoder decoder;

		Stream base_stream;
		bool mayBlock;
		StringBuilder line_builder;

		public NonBlockingStreamReader(Stream stream, Encoding encoding)
		{
			int buffer_size = DefaultBufferSize;
			base_stream = stream;
			input_buffer = new byte [buffer_size];
			this.buffer_size = buffer_size;
			this.encoding = encoding;
			decoder = encoding.GetDecoder ();

			decoded_buffer = new char [encoding.GetMaxCharCount (buffer_size)];
			decoded_count = 0;
			pos = 0;
		}

		public Encoding Encoding {
			get { return encoding; }
		}

		public override void Close ()
		{
			Dispose (true);
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing && base_stream != null)
				base_stream.Close ();
			
			input_buffer = null;
			decoded_buffer = null;
			encoding = null;
			decoder = null;
			base_stream = null;
			base.Dispose (disposing);
		}

		public void DiscardBufferedData ()
		{
			pos = decoded_count = 0;
			mayBlock = false;
#if NET_2_0
			decoder.Reset ();
#else
			decoder = encoding.GetDecoder ();
#endif
		}
		
		// the buffer is empty, fill it again
		private int ReadBuffer ()
		{
			pos = 0;
			int cbEncoded = 0;

			// keep looping until the decoder gives us some chars
			decoded_count = 0;
			int parse_start = 0;
			do	
			{
				cbEncoded = base_stream.Read (input_buffer, 0, buffer_size);
				
				if (cbEncoded == 0)
					return 0;

				mayBlock = (cbEncoded < buffer_size);
				decoded_count += decoder.GetChars (input_buffer, parse_start, cbEncoded, decoded_buffer, 0);
				parse_start = 0;
			} while (decoded_count == 0);

			return decoded_count;
		}

		public override int Peek ()
		{
			if (base_stream == null)
				throw new ObjectDisposedException ("StreamReader", "Cannot read from a closed StreamReader");
			if (pos >= decoded_count && (mayBlock || ReadBuffer () == 0))
				return -1;

			return decoded_buffer [pos];
		}

		public override int Read ()
		{
			if (base_stream == null)
				throw new ObjectDisposedException ("StreamReader", "Cannot read from a closed StreamReader");
			if (pos >= decoded_count && ReadBuffer () == 0)
				return -1;

			return decoded_buffer [pos++];
		}

		public override int Read ([In, Out] char[] dest_buffer, int index, int count)
		{
			if (base_stream == null)
				throw new ObjectDisposedException ("StreamReader", "Cannot read from a closed StreamReader");
			if (dest_buffer == null)
				throw new ArgumentNullException ("dest_buffer");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index", "< 0");
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "< 0");
			// re-ordered to avoid possible integer overflow
			if (index > dest_buffer.Length - count)
				throw new ArgumentException ("index + count > dest_buffer.Length");

			int chars_read = 0;
//			while (count > 0)
			{
				if (pos >= decoded_count && ReadBuffer () == 0)
					return chars_read > 0 ? chars_read : 0;

				int cch = Math.Min (decoded_count - pos, count);
				Array.Copy (decoded_buffer, pos, dest_buffer, index, cch);
				pos += cch;
				index += cch;
				count -= cch;
				chars_read += cch;
			}
			return chars_read;
		}

		bool foundCR;
		int FindNextEOL ()
		{
			char c = '\0';
			for (; pos < decoded_count; pos++) {
				c = decoded_buffer [pos];
				if (c == '\n') {
					pos++;
					int res = (foundCR) ? (pos - 2) : (pos - 1);
					if (res < 0)
						res = 0; // if a new buffer starts with a \n and there was a \r at
							// the end of the previous one, we get here.
					foundCR = false;
					return res;
				} else if (foundCR) {
					foundCR = false;
					return pos - 1;
				}

				foundCR = (c == '\r');
			}

			return -1;
		}

		public override string ReadLine()
		{
			if (base_stream == null)
				throw new ObjectDisposedException ("StreamReader", "Cannot read from a closed StreamReader");

			if (pos >= decoded_count && ReadBuffer () == 0)
				return null;

			int begin = pos;
			int end = FindNextEOL ();
			if (end < decoded_count && end >= begin)
				return new string (decoded_buffer, begin, end - begin);

			if (line_builder == null)
				line_builder = new StringBuilder ();
			else
				line_builder.Length = 0;

			while (true) {
				if (foundCR) // don't include the trailing CR if present
					decoded_count--;

				line_builder.Append (new string (decoded_buffer, begin, decoded_count - begin));
				if (ReadBuffer () == 0) {
					if (line_builder.Capacity > 32768) {
						StringBuilder sb = line_builder;
						line_builder = null;
						return sb.ToString (0, sb.Length);
					}
					return line_builder.ToString (0, line_builder.Length);
				}

				begin = pos;
				end = FindNextEOL ();
				if (end < decoded_count && end >= begin) {
					line_builder.Append (new string (decoded_buffer, begin, end - begin));
					if (line_builder.Capacity > 32768) {
						StringBuilder sb = line_builder;
						line_builder = null;
						return sb.ToString (0, sb.Length);
					}
					return line_builder.ToString (0, line_builder.Length);
				}
			}
		}

		public override string ReadToEnd()
		{
			if (base_stream == null)
				throw new ObjectDisposedException ("StreamReader", "Cannot read from a closed StreamReader");

			StringBuilder text = new StringBuilder ();

			int size = decoded_buffer.Length;
			char [] buffer = new char [size];
			int len;
			
			while ((len = Read (buffer, 0, size)) != 0)
				text.Append (buffer, 0, len);

			return text.ToString ();
		}
	}
	#endregion

	class XmlInputStream : Stream
	{
		public static readonly Encoding StrictUTF8;

		static XmlInputStream ()
		{
			StrictUTF8 = new UTF8Encoding (false, true);
		}

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

		static string GetStringFromBytes (byte [] bytes, int index, int count)
		{
#if MOONLIGHT
			char [] chars = new char [count];
			for (int i = index; i < count; i++)
				chars [i] = (char) bytes [i];

			return new string (chars);
#else
			return Encoding.ASCII.GetString (bytes, index, count);
#endif
		}

		private void Initialize (Stream stream)
		{
			buffer = new byte [6];
			this.stream = stream;
			enc = StrictUTF8; // Default to UTF8 if we can't guess it
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
				if (bufLength >= 5 && GetStringFromBytes (buffer, 1, 4) == "?xml") {
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
						if (remaining >= 7 && GetStringFromBytes (buffer, bufPos, 7) == "ncoding") {
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
#if TARGET_JVM
				else {
					if (bufLength >= 10 && Encoding.Unicode.GetString (buffer, 2, 8) == "?xml")
						enc = Encoding.Unicode;
				}
#endif
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
