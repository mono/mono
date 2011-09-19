//
// System.IO.StreamWriter.cs
//
// Authors:
//   Dietmar Maurer (dietmar@ximian.com)
//   Paolo Molaro (lupus@ximian.com)
//   Marek Safar (marek.safar@gmail.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// Copyright 2011 Xamarin Inc.
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

using System.Text;
using System;

using System.Runtime.InteropServices;

namespace System.IO {
	
	[Serializable]
	[ComVisible (true)]
	public class StreamWriter : TextWriter {

		private Encoding internalEncoding;

		private Stream internalStream;

		private const int DefaultBufferSize = 1024;
		private const int DefaultFileBufferSize = 4096;
		private const int MinimumBufferSize = 256;

		private byte[] byte_buf;
		private char[] decode_buf;
		private int byte_pos;
		private int decode_pos;

		private bool iflush;
		private bool DisposedAlready;
		private bool preamble_done;

		public new static readonly StreamWriter Null = new StreamWriter (Stream.Null, Encoding.UTF8Unmarked, 1);

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

			// Fixes bug http://bugzilla.ximian.com/show_bug.cgi?id=74513
			if (internalStream.CanSeek && internalStream.Position > 0)
				preamble_done = true;
		}

#if NET_4_5
		readonly bool leave_open;

		public StreamWriter (Stream stream, Encoding encoding, int bufferSize)
			: this (stream, encoding, bufferSize, false)
		{
		}
		
		public StreamWriter (Stream stream, Encoding encoding, int bufferSize, bool leaveOpen)
#else
		const bool leave_open = false;

		public StreamWriter (Stream stream, Encoding encoding, int bufferSize)
#endif
		{
			if (null == stream)
				throw new ArgumentNullException("stream");
			if (null == encoding)
				throw new ArgumentNullException("encoding");
			if (bufferSize <= 0)
				throw new ArgumentOutOfRangeException("bufferSize");
			if (!stream.CanWrite)
				throw new ArgumentException ("Can not write to stream");

#if NET_4_5
			leave_open = leaveOpen;
#endif
			internalStream = stream;

			Initialize(encoding, bufferSize);
		}

		public StreamWriter (string path)
			: this (path, false, Encoding.UTF8Unmarked, DefaultFileBufferSize) {}

		public StreamWriter (string path, bool append)
			: this (path, append, Encoding.UTF8Unmarked, DefaultFileBufferSize) {}

		public StreamWriter (string path, bool append, Encoding encoding)
			: this (path, append, encoding, DefaultFileBufferSize) {}

		public StreamWriter (string path, bool append, Encoding encoding, int bufferSize)
		{
			if (null == encoding)
				throw new ArgumentNullException("encoding");
			if (bufferSize <= 0)
				throw new ArgumentOutOfRangeException("bufferSize");

			FileMode mode;

			if (append)
				mode = FileMode.Append;
			else
				mode = FileMode.Create;
			
			internalStream = new FileStream (path, mode, FileAccess.Write, FileShare.Read);

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
				if (iflush)
					Flush ();
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

		protected override void Dispose (bool disposing) 
		{
			Exception exc = null;
			if (!DisposedAlready && disposing && internalStream != null && !leave_open) {
				try {
					Flush();
				} catch (Exception e) {
					exc = e;
				}
				DisposedAlready = true;
				try {
					internalStream.Close ();
				} catch (Exception e) {
					if (exc == null)
						exc = e;
				}
			}

			internalStream = null;
			byte_buf = null;
			internalEncoding = null;
			decode_buf = null;
			if (exc != null)
				throw exc;
		}

		public override void Flush ()
		{
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
		void FlushBytes () 
		{
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
		
		void Decode () 
		{
			if (byte_pos > 0)
				FlushBytes ();
			if (decode_pos > 0) {
				int len = internalEncoding.GetBytes (decode_buf, 0, decode_pos, byte_buf, byte_pos);
				byte_pos += len;
				decode_pos = 0;
			}
		}
		
		public override void Write (char[] buffer, int index, int count) 
		{
			if (DisposedAlready)
				throw new ObjectDisposedException("StreamWriter");
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index", "< 0");
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "< 0");
			// re-ordered to avoid possible integer overflow
			if (index > buffer.Length - count)
				throw new ArgumentException ("index + count > buffer.Length");

			LowLevelWrite (buffer, index, count);
			if (iflush)
				Flush();
		}
		
		void LowLevelWrite (char[] buffer, int index, int count)
		{
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
		
		void LowLevelWrite (string s)
		{
			int count = s.Length;
			int index = 0;
			while (count > 0) {
				int todo = decode_buf.Length - decode_pos;
				if (todo == 0) {
					Decode ();
					todo = decode_buf.Length;
				}
				if (todo > count)
					todo = count;
				
				for (int i = 0; i < todo; i ++)
					decode_buf [i + decode_pos] = s [i + index];
				
				count -= todo;
				index += todo;
				decode_pos += todo;
			}
		}

		public override void Write (char value)
		{
			if (DisposedAlready)
				throw new ObjectDisposedException("StreamWriter");

			// the size of decode_buf is always > 0 and
			// we check for overflow right away
			if (decode_pos >= decode_buf.Length)
				Decode ();
			decode_buf [decode_pos++] = value;
			if (iflush)
				Flush ();
		}

		public override void Write (char[] buffer)
		{
			if (DisposedAlready)
				throw new ObjectDisposedException ("StreamWriter");

			if (buffer != null)
				LowLevelWrite (buffer, 0, buffer.Length);
			if (iflush)
				Flush ();
		}

		public override void Write (string value) 
		{
			if (DisposedAlready)
				throw new ObjectDisposedException("StreamWriter");

			if (value != null)
				LowLevelWrite (value);
			
			if (iflush)
				Flush ();
		}

		public override void Close()
		{
			Dispose (true);
		}

		~StreamWriter ()
		{
			Dispose(false);
		}
	}
}
