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
	[Serializable]
	public class StreamReader : TextReader {

		private const int DefaultBufferSize = 1024;
		private const int DefaultFileBufferSize = 4096;
		private const int MinimumBufferSize = 128;

		// buffering members
		private byte [] rgbEncoded;
//		private int cbEncoded;
		private char [] rgchDecoded;
		private int cchDecoded;

		private int pos;


		private Encoding internalEncoding;
		private Decoder decoder;

		private Stream internalStream;

		[MonoTODO("Make Read methods return 0, etc.")]
		private class NullStreamReader : StreamReader {
		}

		public new static readonly StreamReader Null = (StreamReader)(new NullStreamReader());

		internal StreamReader() {}

		public StreamReader(Stream stream)
			: this (stream, null, false, DefaultBufferSize) { }

		public StreamReader(Stream stream, bool detectEncodingFromByteOrderMarks)
			: this (stream, null, detectEncodingFromByteOrderMarks, DefaultBufferSize) { }

		public StreamReader(Stream stream, Encoding encoding)
			: this (stream, encoding, false, DefaultBufferSize) { }

		public StreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks)
			: this (stream, encoding, detectEncodingFromByteOrderMarks, DefaultBufferSize) { }
		
		public StreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
		{
			Initialize (stream, encoding, detectEncodingFromByteOrderMarks, bufferSize);
		}

		public StreamReader(string path)
			: this (path, null, false, DefaultFileBufferSize) { }

		public StreamReader(string path, bool detectEncodingFromByteOrderMarks)
			: this (path, null, detectEncodingFromByteOrderMarks, DefaultFileBufferSize) { }

		public StreamReader(string path, Encoding encoding)
			: this (path, encoding, false, DefaultFileBufferSize) { }

		public StreamReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks)
			: this (path, encoding, detectEncodingFromByteOrderMarks, DefaultFileBufferSize) { }
		
		[MonoTODO]
		public StreamReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
		{
			if (null == path)
				throw new ArgumentNullException();
			if (String.Empty == path)
				throw new ArgumentException();
			if (path.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException("path contains invalid characters");

			string DirName = Path.GetDirectoryName(path);
			if (DirName != String.Empty && !Directory.Exists(DirName))
				throw new DirectoryNotFoundException();
			if (!File.Exists(path))
				throw new FileNotFoundException(path);

			Stream stream = (Stream) File.OpenRead (path);
			Initialize (stream, encoding, detectEncodingFromByteOrderMarks, bufferSize);
		}

		[MonoTODO]
		protected void Initialize (Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
		{
			if (null == stream)
				throw new ArgumentNullException();
			if (!stream.CanRead)
				throw new ArgumentException("Cannot read stream");

			internalStream = stream;

			// use detect encoding flag
			if (encoding == null) {
				internalEncoding = Encoding.UTF8;
				decoder = Encoding.UTF8.GetDecoder ();
			} else {
				internalEncoding = encoding;
				decoder = encoding.GetDecoder ();
			}

			if (bufferSize < MinimumBufferSize)
				bufferSize = MinimumBufferSize;

			rgbEncoded = new byte [bufferSize];
			rgchDecoded = new char [internalEncoding.GetMaxCharCount (bufferSize)];
			pos = 0;
			cchDecoded = 0;
		}

		public virtual Stream BaseStream
		{
			get {
				return internalStream;
			}
		}

		public virtual Encoding CurrentEncoding
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
			pos = 0;
			cchDecoded = 0;

/* I'm sure there's no need to do all this
			if ((cchDecoded == null) || (pos == cchDecoded.Length))
				return;

			if (!internalStream.CanSeek)
				return;

			int seek_back = pos - cchDecoded.Length;
			internalStream.Seek (seek_back, SeekOrigin.Current);
*/
		}


		// the buffer is empty, fill it again
		[MonoTODO ("handle byte order marks here")]
		private int ReadBuffer ()
		{
			pos = 0;
			int cbEncoded = 0;
			cchDecoded = 0;
			do	// keep looping until the decoder gives us some chars
			{
				cbEncoded = internalStream.Read (rgbEncoded, 0, rgbEncoded.Length);
				// TODO: remove this line when iconv is fixed
				int bufcnt = decoder.GetCharCount (rgbEncoded, 0, cbEncoded);

				if (cbEncoded == 0)
					return 0;
				// TODO: remove byte order marks here
				cchDecoded += decoder.GetChars (rgbEncoded, 0, cbEncoded, rgchDecoded, 0);
			} while (cchDecoded == 0);

			return cchDecoded;
		}

		public override int Peek ()
		{
			if (pos >= cchDecoded && ReadBuffer () == 0)
				return -1;

			return rgchDecoded [pos];
		}

		public override int Read ()
		{
			if (pos >= cchDecoded && ReadBuffer () == 0)
				return -1;

			return rgchDecoded [pos++];
		}

		public override int Read (char[] dest_buffer, int index, int count)
		{
			if (dest_buffer == null)
				throw new ArgumentException ();

			if ((index < 0) || (count < 0))
				throw new ArgumentOutOfRangeException ();

			if (index + count > dest_buffer.Length)
				throw new ArgumentException ();

			int cchRead = 0;
			while (count > 0)
			{
				if (pos >= cchDecoded && ReadBuffer () == 0)
					return cchRead > 0? cchRead: -1;

				int cch = Math.Min (cchDecoded - pos, count);
				Array.Copy (rgchDecoded, pos, dest_buffer, index, cch);
				pos += cch;
				index += cch;
				count -= cch;
				cchRead += cch;
			}
			return cchRead;
		}

		public override string ReadLine()
		{
			StringBuilder text = new StringBuilder ();

			while (true) {
				int c = Read ();

				if (c == -1) {				// end of stream
					if (text.Length == 0)
						return null;

					break;
				}

				if (c == '\n')				// newline
					break;

				if (c == '\r' && Peek () == '\n') {	// cr, newline
					Read ();
					break;
				}

				text.Append ((char) c);
			}

			return text.ToString ();
		}

		public override string ReadToEnd()
		{
			StringBuilder text = new StringBuilder ();

			int c;
			while ((c = Read ()) != -1) {
				text.Append ((char) c);
			}

			if (text.Length == 0)
				return String.Empty;
			return text.ToString ();
		}
	}
}
