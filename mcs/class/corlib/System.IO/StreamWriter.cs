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
// Copyright 2011, 2013 Xamarin Inc.
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
using System.Runtime.InteropServices;
#if NET_4_5
using System.Threading.Tasks;
#endif

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
		private bool preamble_done;

#if NET_4_5
		readonly bool leave_open;
		IDecoupledTask async_task;
#endif

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
			if (byte_buf == null || !disposing)
				return;

			try {
				Flush ();
			} finally {
				byte_buf = null;
				internalEncoding = null;
				decode_buf = null;

				if (!leave_open) {
					internalStream.Close ();
				}

				internalStream = null;
			}
		}

		public override void Flush ()
		{
			CheckState ();
			FlushCore ();
		}

		// Keep in sync with FlushCoreAsync
		void FlushCore ()
		{
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

#if NET_4_5
		async Task FlushCoreAsync ()
		{
			await DecodeAsync ().ConfigureAwait (false);
			if (byte_pos > 0) {
				await FlushBytesAsync ().ConfigureAwait (false);
				await internalStream.FlushAsync ().ConfigureAwait (false);
			}
		}

		async Task FlushBytesAsync ()
		{
			// write the encoding preamble only at the start of the stream
			if (!preamble_done && byte_pos > 0) {
				byte[] preamble = internalEncoding.GetPreamble ();
				if (preamble.Length > 0)
					await internalStream.WriteAsync (preamble, 0, preamble.Length).ConfigureAwait (false);
				preamble_done = true;
			}

			await internalStream.WriteAsync (byte_buf, 0, byte_pos).ConfigureAwait (false);
			byte_pos = 0;
		}

		async Task DecodeAsync () 
		{
			if (byte_pos > 0)
				await FlushBytesAsync ().ConfigureAwait (false);
			if (decode_pos > 0) {
				int len = internalEncoding.GetBytes (decode_buf, 0, decode_pos, byte_buf, byte_pos);
				byte_pos += len;
				decode_pos = 0;
			}
		}		

		async Task LowLevelWriteAsync (char[] buffer, int index, int count)
		{
			while (count > 0) {
				int todo = decode_buf.Length - decode_pos;
				if (todo == 0) {
					await DecodeAsync ().ConfigureAwait (false);
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
		
		async Task LowLevelWriteAsync (string s)
		{
			int count = s.Length;
			int index = 0;
			while (count > 0) {
				int todo = decode_buf.Length - decode_pos;
				if (todo == 0) {
					await DecodeAsync ().ConfigureAwait (false);
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
#endif

		public override void Write (char[] buffer, int index, int count) 
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index", "< 0");
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "< 0");
			// re-ordered to avoid possible integer overflow
			if (index > buffer.Length - count)
				throw new ArgumentException ("index + count > buffer.Length");

			CheckState ();

			LowLevelWrite (buffer, index, count);
			if (iflush)
				FlushCore ();
		}
		
		public override void Write (char value)
		{
			CheckState ();

			// the size of decode_buf is always > 0 and
			// we check for overflow right away
			if (decode_pos >= decode_buf.Length)
				Decode ();
			decode_buf [decode_pos++] = value;
			if (iflush)
				FlushCore ();
		}

		public override void Write (char[] buffer)
		{
			CheckState ();

			if (buffer != null)
				LowLevelWrite (buffer, 0, buffer.Length);
			if (iflush)
				FlushCore ();
		}

		public override void Write (string value) 
		{
			CheckState ();

			if (value == null)
				return;
			
			LowLevelWrite (value);
			
			if (iflush)
				FlushCore ();
		}

		public override void Close()
		{
			Dispose (true);
		}

		void CheckState ()
		{
			if (byte_buf == null)
				throw new ObjectDisposedException ("StreamWriter");

#if NET_4_5
			if (async_task != null && !async_task.IsCompleted)
				throw new InvalidOperationException ();
#endif
		}

#if NET_4_5
		public override Task FlushAsync ()
		{
			CheckState ();
			DecoupledTask res;
			async_task = res = new DecoupledTask (FlushCoreAsync ());
			return res.Task;
		}

		public override Task WriteAsync (char value)
		{
			CheckState ();

			DecoupledTask res;
			async_task = res = new DecoupledTask (WriteAsyncCore (value));
			return res.Task;
		}

		async Task WriteAsyncCore (char value)
		{
			// the size of decode_buf is always > 0 and
			// we check for overflow right away
			if (decode_pos >= decode_buf.Length)
				await DecodeAsync ().ConfigureAwait (false);
			decode_buf [decode_pos++] = value;

			if (iflush)
				await FlushCoreAsync ().ConfigureAwait (false);
		}

		public override Task WriteAsync (char[] buffer, int index, int count)
		{
			CheckState ();
			if (buffer == null)
				return TaskConstants.Finished;

			DecoupledTask res;
			async_task = res = new DecoupledTask (WriteAsyncCore (buffer, index, count));
			return res.Task;
		}

		async Task WriteAsyncCore (char[] buffer, int index, int count)
		{
			// Debug.Assert (buffer == null);

			await LowLevelWriteAsync (buffer, index, count).ConfigureAwait (false);

			if (iflush)
				await FlushCoreAsync ().ConfigureAwait (false);
		}

		public override Task WriteAsync (string value)
		{
			CheckState ();

			if (value == null)
				return TaskConstants.Finished;

			DecoupledTask res;			
			async_task = res = new DecoupledTask (WriteAsyncCore (value, false));
			return res.Task;
		}

		async Task WriteAsyncCore (string value, bool appendNewLine)
		{
			// Debug.Assert (value == null);

			await LowLevelWriteAsync (value).ConfigureAwait (false);
			if (appendNewLine)
				await LowLevelWriteAsync (CoreNewLine, 0, CoreNewLine.Length).ConfigureAwait (false);
			
			if (iflush)
				await FlushCoreAsync ().ConfigureAwait (false);
		}		

		public override Task WriteLineAsync ()
		{
			CheckState ();

			DecoupledTask res;
			async_task = res = new DecoupledTask (WriteAsyncCore (CoreNewLine, 0, CoreNewLine.Length));
			return res.Task;
		}

		public override Task WriteLineAsync (char value)
		{
			CheckState ();
			DecoupledTask res;
			async_task = res = new DecoupledTask (WriteLineAsyncCore (value));
			return res.Task;
		}

		async Task WriteLineAsyncCore (char value)
		{
			await WriteAsyncCore (value).ConfigureAwait (false);
			await LowLevelWriteAsync (CoreNewLine, 0, CoreNewLine.Length).ConfigureAwait (false);
			
			if (iflush)
				await FlushCoreAsync ().ConfigureAwait (false);
		}		

		public override Task WriteLineAsync (char[] buffer, int index, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index", "< 0");
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "< 0");
			// re-ordered to avoid possible integer overflow
			if (index > buffer.Length - count)
				throw new ArgumentException ("index + count > buffer.Length");

			CheckState ();
			DecoupledTask res;
			async_task = res = new DecoupledTask (WriteLineAsyncCore (buffer, index, count));
			return res.Task;
		}

		async Task WriteLineAsyncCore (char[] buffer, int index, int count)
		{
			// Debug.Assert (buffer == null);

			await LowLevelWriteAsync (buffer, index, count).ConfigureAwait (false);
			await LowLevelWriteAsync (CoreNewLine, 0, CoreNewLine.Length).ConfigureAwait (false);
			
			if (iflush)
				await FlushCoreAsync ().ConfigureAwait (false);
		}		

		public override Task WriteLineAsync (string value)
		{
			if (value == null)
				return WriteLineAsync ();

			CheckState ();
			DecoupledTask res;			
			async_task = res = new DecoupledTask (WriteAsyncCore (value, true));
			return res.Task;
		}
#endif
	}
}
