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

		private Encoding internalEncoding;

		private Stream internalStream;

                // new public static readonly StreamReader Null;

		public StreamReader(Stream stream)
			: this (stream, null, false, 0) { }

		public StreamReader(Stream stream, bool detectEncodingFromByteOrderMarks)
			: this (stream, null, detectEncodingFromByteOrderMarks, 0) { }

		public StreamReader(Stream stream, Encoding encoding)
			: this (stream, encoding, false, 0) { }

		public StreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks)
			: this (stream, encoding, detectEncodingFromByteOrderMarks, 0) { }
		
		public StreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
		{
			internalStream = stream;

			if (encoding == null)
				internalEncoding = Encoding.UTF8;
			else
				internalEncoding = encoding;

		}

		public StreamReader(string path)
			: this (path, null, false, 0) { }

		public StreamReader(string path, bool detectEncodingFromByteOrderMarks)
			: this (path, null, detectEncodingFromByteOrderMarks, 0) { }

		public StreamReader(string path, Encoding encoding)
			: this (path, encoding, false, 0) { }

		public StreamReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks)
			: this (path, encoding, detectEncodingFromByteOrderMarks, 0) { }
		
		public StreamReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
		{
			//internalStream = stream;

			if (encoding == null)
				internalEncoding = Encoding.UTF8;
			else
				internalEncoding = encoding;

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
	
		public override int Peek ()
		{
			return -1;
		}

		public override int Read ()
		{
			return -1;
		}

		public override int Read (char[] buffer, int index, int count)
		{
			return 0;
		}

		public override string ReadLine()
		{
			return String.Empty;
		}

                public override string ReadToEnd()
		{
			return String.Empty;
                }

	}
}
