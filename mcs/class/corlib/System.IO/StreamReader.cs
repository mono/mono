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

		[MonoTODO]
		public override void Close ()
		{
			Dispose (true);
		}

		[MonoTODO]
		public void DiscardBufferedData ()
		{
		}

		public override int Peek ()
		{
			if (!internalStream.CanSeek)
				return -1;

			if ((buffer == null) || ((pos + 1) == buffer.Length)) {
				int cnt = internalEncoding.GetMaxByteCount (1);
				byte[] bytes = new byte[cnt];
				int actcnt = internalStream.Read (bytes, 0, cnt);
				internalStream.Seek (-actcnt, SeekOrigin.Current);

				if (actcnt <= 0) 
					return -1;

				int bufcnt = decoder.GetCharCount (bytes, 0, cnt);
				char[] chars = new char [bufcnt];
				bufcnt = decoder.GetChars (bytes, 0, cnt, chars, 0);
				return chars [0];
			}

			return buffer [pos + 1];
		}

		public override int Read ()
		{
			if ((buffer == null) || (++pos == buffer.Length)) {
				byte[] bytes =  new byte [8192];
				int cnt = internalStream.Read (bytes, 0, 8192);

				if (cnt <= 0) 
					return -1;

				int bufcnt = decoder.GetCharCount (bytes, 0, cnt);
				buffer = new char [bufcnt];
				bufcnt = decoder.GetChars (bytes, 0, cnt, buffer, 0);
				pos = 0;
			}

			return buffer[pos];
		}

		[MonoTODO]
		public override int Read (char[] buffer, int index, int count)
		{
			return 0;
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
