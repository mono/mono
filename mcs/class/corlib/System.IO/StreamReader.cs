//
// System.IO.StreamReader.cs
//
// Author:
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Text;

namespace System.IO {
	
	public class StreamReader : TextReader {

		// buffering members
		private char[] buffer;
		private int pos;

		private Encoding internalEncoding;
		private Decoder decoder;

		private Stream internalStream;

                public new static readonly StreamReader Null = new StreamReader((Stream)null);

		public StreamReader(Stream stream)
			: this (stream, null, false, 0) { }

		public StreamReader(Stream stream, bool detectEncodingFromByteOrderMarks)
			: this (stream, null, detectEncodingFromByteOrderMarks, 0) { }

		public StreamReader(Stream stream, Encoding encoding)
			: this (stream, encoding, false, 0) { }

		public StreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks)
			: this (stream, encoding, detectEncodingFromByteOrderMarks, 0) { }
		
		[MonoTODO]
		public StreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
		{
			internalStream = stream;

			// use detect encoding flag
			if (encoding == null) {
				internalEncoding = Encoding.UTF8;
				decoder = Encoding.UTF8.GetDecoder ();
			} else {
				internalEncoding = encoding;
				decoder = encoding.GetDecoder ();
 			}

			buffer = null;
		}

		public StreamReader(string path)
			: this (path, null, false, 0) { }

		public StreamReader(string path, bool detectEncodingFromByteOrderMarks)
			: this (path, null, detectEncodingFromByteOrderMarks, 0) { }

		public StreamReader(string path, Encoding encoding)
			: this (path, encoding, false, 0) { }

		public StreamReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks)
			: this (path, encoding, detectEncodingFromByteOrderMarks, 0) { }
		
		[MonoTODO]
		public StreamReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
		{
			// use detect encoding flag
			if (encoding == null) {
				internalEncoding = Encoding.UTF8;
				decoder = Encoding.UTF8.GetDecoder ();
			} else {
				internalEncoding = encoding;
				decoder = encoding.GetDecoder ();
 			}

			internalStream = (Stream) File.OpenRead (path);

			buffer = null;
		}

		public virtual Stream BaseStream
		{
			get {
				return internalStream;
			}
		}

		public Encoding CurrentEncoding
		{
			get {
				return internalEncoding;
			}
		}

		public override void Close ()
		{
			Dispose (true);
		}

		public void DiscardBufferedData ()
		{
			if ((buffer == null) || (pos == buffer.Length))
				return;

			if (!internalStream.CanSeek)
				return;

			int seek_back = pos - buffer.Length;
			internalStream.Seek (seek_back, SeekOrigin.Current);
		}

		private int GetRemaining ()
		{
			return (buffer != null) ? buffer.Length - pos : 0;
		}

		private int RoundUpTo (int number, int roundto)
		{
			if ((number % roundto) == 0)
				return number;
			else
				return ((number / roundto) + 1) * roundto;
		}

		private bool ReadBuffer (int count)
		{
			// There are still enough bytes in the buffer.
			if ((buffer != null) && (pos + count <= buffer.Length))
				return true;

			// Number of bytes remaining in the buffer.
			int remaining = GetRemaining ();

			// Round up to block size
			int size = RoundUpTo (count, 4096);
			byte[] bytes = new byte [size];
			int cnt = internalStream.Read (bytes, 0, size);

			if (cnt <= 0) 
				return false;

			int bufcnt = decoder.GetCharCount (bytes, 0, cnt);
			char[] newbuffer = new char [remaining + bufcnt];
			if (remaining > 0)
				Array.Copy (buffer, pos, newbuffer, 0, remaining);
			buffer = newbuffer;

			bufcnt = decoder.GetChars (bytes, 0, cnt, buffer, remaining);
			pos = 0;

			return true;
		}

		public override int Peek ()
		{
			if (!internalStream.CanSeek)
				return -1;

			if (!ReadBuffer (1))
				return -1;

			return buffer [pos];
		}

		public override int Read ()
		{
			if (!ReadBuffer (1))
				return -1;

			return buffer[pos++];
		}

		public override int Read (char[] dest_buffer, int index, int count)
		{
			if (dest_buffer == null)
				throw new ArgumentException ();

			if (index + count >= dest_buffer.Length)
				throw new ArgumentException ();

			if ((index < 0) || (count < 0))
				throw new ArgumentOutOfRangeException ();

			if (!ReadBuffer (count))
				return -1;

			int remaining = buffer.Length - pos;
			int size = Math.Min (remaining, count);

			Array.Copy (buffer, pos, dest_buffer, index, size);

			return size;
		}

		[MonoTODO]
		public override string ReadLine()
		{
			return String.Empty;
		}

		[MonoTODO]
                public override string ReadToEnd()
		{
			return String.Empty;
                }

	}
}
