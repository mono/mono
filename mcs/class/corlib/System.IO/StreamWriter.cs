//
// System.IO.StreamWriter.cs
//
// Author:
//   Dietmar Maurer (dietmar@ximian.com)
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
		private const int MinimumBufferSize = 2;

		private int pos;
		private int BufferSize;
		private byte[] TheBuffer;

		private bool DisposedAlready = false;

		public new static readonly StreamWriter Null = new StreamWriter (Stream.Null, Encoding.UTF8Unmarked, 0);

		public StreamWriter (Stream stream)
			: this (stream, new UTF8Encoding (false, true), DefaultBufferSize) {}

		public StreamWriter (Stream stream, Encoding encoding)
			: this (stream, encoding, DefaultBufferSize) {}

		internal void Initialize(Encoding encoding, int bufferSize) {
			internalEncoding = encoding;
			pos = 0;
			BufferSize = Math.Max(bufferSize, MinimumBufferSize);
			TheBuffer = new byte[BufferSize];
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
				throw new ArgumentException("bufferSize");

			internalStream = stream;

			Initialize(encoding, bufferSize);
		}

		public StreamWriter (string path)
			: this (path, false, Encoding.UTF8, DefaultFileBufferSize) {}

		public StreamWriter (string path, bool append)
			: this (path, append, Encoding.UTF8, DefaultFileBufferSize) {}

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
			TheBuffer = null;
			internalEncoding = null;
		}

		public override void Flush () {
			if (DisposedAlready)
				throw new ObjectDisposedException("StreamWriter");

			if (pos > 0) {
				internalStream.Write (TheBuffer, 0, pos);
				internalStream.Flush ();
				pos = 0;
			}
		}
		
		public override void Write (char[] buffer, int index, int count) {
			if (DisposedAlready)
				throw new ObjectDisposedException("StreamWriter");

			byte[] res = new byte [internalEncoding.GetByteCount (buffer)];
			int len;
			int BytesToBuffer;
			int resPos = 0;

			len = internalEncoding.GetBytes (buffer, index, count, res, 0);

			// if they want AutoFlush, don't bother buffering
			if (iflush) {
				Flush();
				internalStream.Write (res, 0, len);
				internalStream.Flush ();
			} else {
				// otherwise use the buffer.
				// NOTE: this logic is not optimized for performance.
				while (resPos < len) {
					// fill the buffer if we've got more bytes than will fit
					BytesToBuffer = Math.Min(BufferSize - pos, len - resPos);
					Array.Copy(res, resPos, TheBuffer, pos, BytesToBuffer);
					resPos += BytesToBuffer;
					pos += BytesToBuffer;
					// if the buffer is full, flush it out.
					if (pos == BufferSize) Flush();
				}
			}
		}

		public override void Write (char value)
		{
			Write (new char [] {value}, 0, 1);
		}

		public override void Write (char [] value)
		{
			Write (value, 0, value.Length);
		}

		public override void Write(string value) {
			if (DisposedAlready)
				throw new ObjectDisposedException("StreamWriter");

			if (value != null)
				Write (value.ToCharArray (), 0, value.Length);
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
