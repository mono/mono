//
// System.IO.StreamWriter.cs
//
// Authors:
//   Dietmar Maurer (dietmar@ximian.com)
//   Paolo Molaro (lupus@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Text;
using System;

namespace System.IO {
	
	[Serializable]
	public class StreamWriter : TextWriter {

		private Encoding internalEncoding;

		private Stream internalStream;
		private bool closed = false;

		private bool iflush;
		
		private const int DefaultBufferSize = 1024;
		private const int DefaultFileBufferSize = 4096;
		private const int MinimumBufferSize = 256;

		private byte[] byte_buf;
		private int byte_pos;
		private char[] decode_buf;
		private int decode_pos;

		private bool DisposedAlready = false;
		private bool preamble_done = false;

		public new static readonly StreamWriter Null = new StreamWriter (Stream.Null, Encoding.UTF8Unmarked, 0);

		public StreamWriter (Stream stream)
			: this (stream, Encoding.UTF8Unmarked, DefaultBufferSize) {}

		public StreamWriter (Stream stream, Encoding encoding)
			: this (stream, encoding, DefaultBufferSize) {}

		internal void Initialize(Encoding encoding, int bufferSize) {
			internalEncoding = encoding;
			decode_pos = byte_pos = 0;
			int BufferSize = Math.Max(bufferSize, MinimumBufferSize);
			decode_buf = new char [BufferSize];
			byte_buf = new byte [encoding.GetMaxByteCount (BufferSize)];
		}

		//[MonoTODO("Nothing is done with bufferSize")]
		public StreamWriter (Stream stream, Encoding encoding, int bufferSize) {
			if (null == stream)
				throw new ArgumentNullException("stream");
			if (null == encoding)
				throw new ArgumentNullException("encoding");
			if (bufferSize < 0)
				throw new ArgumentOutOfRangeException("bufferSize");
			if (!stream.CanWrite)
				throw new ArgumentException("Can not write to stream", "stream");

			internalStream = stream;

			Initialize(encoding, bufferSize);
		}

		public StreamWriter (string path)
			: this (path, false, Encoding.UTF8Unmarked, DefaultFileBufferSize) {}

		public StreamWriter (string path, bool append)
			: this (path, append, Encoding.UTF8Unmarked, DefaultFileBufferSize) {}

		public StreamWriter (string path, bool append, Encoding encoding)
			: this (path, append, encoding, DefaultFileBufferSize) {}
		
		public StreamWriter (string path, bool append, Encoding encoding, int bufferSize) {
			if (null == path)
				throw new ArgumentNullException("path");
			if (String.Empty == path)
				throw new ArgumentException("path cannot be empty string");
			if (path.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException("path contains invalid characters");

			if (null == encoding)
				throw new ArgumentNullException("encoding");
			if (bufferSize < 0)
				throw new ArgumentOutOfRangeException("bufferSize");

			string DirName = Path.GetDirectoryName(path);
			if (DirName != String.Empty && !Directory.Exists(DirName))
				throw new DirectoryNotFoundException();

			FileMode mode;

			if (append)
				mode = FileMode.Append;
			else
				mode = FileMode.Create;
			
			internalStream = new FileStream (path, mode, FileAccess.Write);

			if (append)
				internalStream.Position = internalStream.Length;
			else
				internalStream.SetLength (0);

			Initialize(encoding, bufferSize);
		}

		public virtual bool AutoFlush {
			get {
				return iflush;
			}
			set {
				iflush = value;
			}
		}

		public virtual Stream BaseStream {
			get {
				return internalStream;
			}
		}

		public override Encoding Encoding {
			get {
				return internalEncoding;
			}
		}

		protected override void Dispose (bool disposing) {
			if (!DisposedAlready && disposing && internalStream != null) {
				Flush();
				DisposedAlready = true;
				internalStream.Close ();
			}

			internalStream = null;
			byte_buf = null;
			internalEncoding = null;
			decode_buf = null;
		}

		public override void Flush () {
			if (DisposedAlready)
				throw new ObjectDisposedException("StreamWriter");

			Decode ();
			if (byte_pos > 0) {
				FlushBytes ();
				internalStream.Flush ();
			}
		}

		// how the speedup works:
		// the Write () methods simply copy the characters in a buffer of chars (decode_buf)
		// Decode () is called when the buffer is full or we need to flash.
		// Decode () will use the encoding to get the bytes and but them inside
		// byte_buf. From byte_buf the data is finally outputted to the stream.
		void FlushBytes () {
			// write the encoding preamble only at the start of the stream
			if (!preamble_done && byte_pos > 0) {
				byte[] preamble = internalEncoding.GetPreamble ();
				if (preamble.Length > 0)
					internalStream.Write (preamble, 0, preamble.Length);
				preamble_done = true;
			}
			internalStream.Write (byte_buf, 0, byte_pos);
			byte_pos = 0;
		}
		
		void Decode () {
			if (byte_pos > 0)
				FlushBytes ();
			if (decode_pos > 0) {
				int len = internalEncoding.GetBytes (decode_buf, 0, decode_pos, byte_buf, byte_pos);
				byte_pos += len;
				decode_pos = 0;
			}
		}
		
		public override void Write (char[] buffer, int index, int count) {
			if (DisposedAlready)
				throw new ObjectDisposedException("StreamWriter");

			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (index < 0 || index > buffer.Length)
				throw new ArgumentOutOfRangeException ("index");
			if (count < 0 || (index + count) > buffer.Length)
				throw new ArgumentOutOfRangeException ("count");

			LowLevelWrite (buffer, index, count);
			if (iflush)
				Flush();
		}
		
		void LowLevelWrite (char[] buffer, int index, int count) {
			while (count > 0) {
				int todo = decode_buf.Length - decode_pos;
				if (todo == 0) {
					Decode ();
					todo = decode_buf.Length;
				}
				if (todo > count)
					todo = count;
				Buffer.BlockCopy (buffer, index * 2, decode_buf, decode_pos * 2, todo * 2);
				count -= todo;
				index += todo;
				decode_pos += todo;
			}
		}

		public override void Write (char value)
		{
			// the size of decode_buf is always > 0 and
			// we check for overflow right away
			if (decode_pos >= decode_buf.Length)
				Decode ();
			decode_buf [decode_pos++] = value;
			if (iflush)
				Flush ();
		}

		public override void Write (char [] value)
		{
			LowLevelWrite (value, 0, value.Length);
			if (iflush)
				Flush ();
		}

		public override void Write (string value) {
			if (DisposedAlready)
				throw new ObjectDisposedException("StreamWriter");

			if (value != null)
				LowLevelWrite (value.ToCharArray (), 0, value.Length);
			if (iflush)
				Flush ();
		}

		public override void WriteLine (string value) {
			if (DisposedAlready)
				throw new ObjectDisposedException("StreamWriter");

			if (value != null)
				LowLevelWrite (value.ToCharArray (), 0, value.Length);
			string nl = NewLine;
				LowLevelWrite (nl.ToCharArray (), 0, nl.Length);
			if (iflush)
				Flush ();
		}

		public override void Close()
		{
			Dispose (true);
		}

		~StreamWriter() {
			Dispose(false);
		}
	}
}
