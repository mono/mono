//
// System.IO.StreamReader.cs
//
// Author:
//   Dietmar Maurer (dietmar@ximian.com)
//   Miguel de Icaza (miguel@ximian.com) 
//   Marek Safar (marek.safar@gmail.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004 Novell (http://www.novell.com)
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

using System;
using System.Text;
using System.Runtime.InteropServices;

namespace System.IO {
	[Serializable]
	[ComVisible (true)]
	public class StreamReader : TextReader {

		const int DefaultBufferSize = 1024;
		const int DefaultFileBufferSize = 4096;
		const int MinimumBufferSize = 128;

		//
		// The input buffer
		//
		byte [] input_buffer;
		
		// Input buffer ready for recycling
		static byte [] input_buffer_recycle;
		static object input_buffer_recycle_lock = new object ();

		//
		// The decoded buffer from the above input buffer
		//
		char [] decoded_buffer;
		static char[] decoded_buffer_recycle;

		Encoding encoding;
		Decoder decoder;
		StringBuilder line_builder;
		Stream base_stream;

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

		int do_checks;
		
		bool mayBlock;

		private class NullStreamReader : StreamReader {
			public override int Peek ()
			{
				return -1;
			}

			public override int Read ()
			{
				return -1;
			}

			public override int Read ([In, Out] char[] buffer, int index, int count)
			{
				return 0;
			}

			public override string ReadLine ()
			{
				return null;
			}

			public override string ReadToEnd ()
			{
				return String.Empty;
			}

			public override Stream BaseStream
			{
				get { return Stream.Null; }
			}

			public override Encoding CurrentEncoding
			{
				get { return Encoding.Unicode; }
			}
		}

		public new static readonly StreamReader Null =  new NullStreamReader ();
		
		internal StreamReader() {}

		public StreamReader(Stream stream)
			: this (stream, Encoding.UTF8Unmarked, true, DefaultBufferSize) { }

		public StreamReader(Stream stream, bool detectEncodingFromByteOrderMarks)
			: this (stream, Encoding.UTF8Unmarked, detectEncodingFromByteOrderMarks, DefaultBufferSize) { }

		public StreamReader(Stream stream, Encoding encoding)
			: this (stream, encoding, true, DefaultBufferSize) { }

		public StreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks)
			: this (stream, encoding, detectEncodingFromByteOrderMarks, DefaultBufferSize) { }

#if NET_4_5
		readonly bool leave_open;

		public StreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
			: this (stream, encoding, detectEncodingFromByteOrderMarks, bufferSize, false)
		{
		}

		public StreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize, bool leaveOpen)
#else
		const bool leave_open = false;

		public StreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
#endif
		{
#if NET_4_5
			leave_open = leaveOpen;
#endif
			Initialize (stream, encoding, detectEncodingFromByteOrderMarks, bufferSize);
		}

		public StreamReader(string path)
			: this (path, Encoding.UTF8Unmarked, true, DefaultFileBufferSize) { }

		public StreamReader(string path, bool detectEncodingFromByteOrderMarks)
			: this (path, Encoding.UTF8Unmarked, detectEncodingFromByteOrderMarks, DefaultFileBufferSize) { }

		public StreamReader(string path, Encoding encoding)
			: this (path, encoding, true, DefaultFileBufferSize) { }

		public StreamReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks)
			: this (path, encoding, detectEncodingFromByteOrderMarks, DefaultFileBufferSize) { }
		
		public StreamReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
		{
			if (null == path)
				throw new ArgumentNullException("path");
			if (String.Empty == path)
				throw new ArgumentException("Empty path not allowed");
			if (path.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException("path contains invalid characters");
			if (null == encoding)
				throw new ArgumentNullException ("encoding");
			if (bufferSize <= 0)
				throw new ArgumentOutOfRangeException ("bufferSize", "The minimum size of the buffer must be positive");

			Stream stream = (Stream) File.OpenRead (path);
			Initialize (stream, encoding, detectEncodingFromByteOrderMarks, bufferSize);
		}

		internal void Initialize (Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
		{
			if (null == stream)
				throw new ArgumentNullException ("stream");
			if (null == encoding)
				throw new ArgumentNullException ("encoding");
			if (!stream.CanRead)
				throw new ArgumentException ("Cannot read stream");
			if (bufferSize <= 0)
				throw new ArgumentOutOfRangeException ("bufferSize", "The minimum size of the buffer must be positive");

			if (bufferSize < MinimumBufferSize)
				bufferSize = MinimumBufferSize;
			
			// since GetChars() might add flushed character, it 
			// should have additional char buffer for extra 1 
			// (probably 1 is ok, but might be insufficient. I'm not sure)
			var decoded_buffer_size = encoding.GetMaxCharCount (bufferSize) + 1;

			//
			// Instead of allocating a new default buffer use the
			// last one if there is any available
			//
			if (bufferSize <= DefaultBufferSize && input_buffer_recycle != null) {
				lock (input_buffer_recycle_lock) {
					if (input_buffer_recycle != null) {
						input_buffer = input_buffer_recycle;
						input_buffer_recycle = null;
					}
					
					if (decoded_buffer_recycle != null && decoded_buffer_size <= decoded_buffer_recycle.Length) {
						decoded_buffer = decoded_buffer_recycle;
						decoded_buffer_recycle = null;
					}
				}
			}
			
			if (input_buffer == null)
				input_buffer = new byte [bufferSize];
			else
				Array.Clear (input_buffer, 0, bufferSize);
			
			if (decoded_buffer == null)
				decoded_buffer = new char [decoded_buffer_size];
			else
				Array.Clear (decoded_buffer, 0, decoded_buffer_size);

			base_stream = stream;		
			this.buffer_size = bufferSize;
			this.encoding = encoding;
			decoder = encoding.GetDecoder ();

			byte [] preamble = encoding.GetPreamble ();
			do_checks = detectEncodingFromByteOrderMarks ? 1 : 0;
			do_checks += (preamble.Length == 0) ? 0 : 2;
			
			decoded_count = 0;
			pos = 0;
		}

		public virtual Stream BaseStream
		{
			get {
				return base_stream;
			}
		}

		public virtual Encoding CurrentEncoding
		{
			get {
				if (encoding == null)
					throw new Exception ();
				return encoding;
			}
		}

		public bool EndOfStream {
			get { return Peek () < 0; }
		}

		public override void Close ()
		{
			Dispose (true);
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing && base_stream != null && !leave_open)
				base_stream.Close ();
			
			if (input_buffer != null && input_buffer.Length == DefaultBufferSize && input_buffer_recycle == null) {
				lock (input_buffer_recycle_lock) {
					if (input_buffer_recycle == null) {
						input_buffer_recycle = input_buffer;
					}
					
					if (decoded_buffer_recycle == null) {
						decoded_buffer_recycle = decoded_buffer;
					}
				}
			}
			
			input_buffer = null;
			decoded_buffer = null;
			encoding = null;
			decoder = null;
			base_stream = null;
			base.Dispose (disposing);
		}

		//
		// Provides auto-detection of the encoding, as well as skipping over
		// byte marks at the beginning of a stream.
		//
		int DoChecks (int count)
		{
			if ((do_checks & 2) == 2){
				byte [] preamble = encoding.GetPreamble ();
				int c = preamble.Length;
				if (count >= c){
					int i;
					
					for (i = 0; i < c; i++)
						if (input_buffer [i] != preamble [i])
							break;

					if (i == c)
						return i;
				}
			}

			if ((do_checks & 1) == 1){
				if (count < 2)
					return 0;

				if (input_buffer [0] == 0xfe && input_buffer [1] == 0xff){
					this.encoding = Encoding.BigEndianUnicode;
					return 2;
				}
				if (input_buffer [0] == 0xff && input_buffer [1] == 0xfe && count < 4) {
					// If we don't have enough bytes we can't check for UTF32, so use Unicode
					this.encoding = Encoding.Unicode;
					return 2;
				}

				if (count < 3)
					return 0;

				if (input_buffer [0] == 0xef && input_buffer [1] == 0xbb && input_buffer [2] == 0xbf){
					this.encoding = Encoding.UTF8Unmarked;
					return 3;
				}

				if (count < 4) {
					if (input_buffer [0] == 0xff && input_buffer [1] == 0xfe && input_buffer [2] != 0) {
						this.encoding = Encoding.Unicode;
						return 2;
					}
					return 0;
				}

				if (input_buffer [0] == 0 && input_buffer [1] == 0
					&& input_buffer [2] == 0xfe && input_buffer [3] == 0xff)
				{
					this.encoding = Encoding.BigEndianUTF32;
					return 4;
				}

				if (input_buffer [0] == 0xff && input_buffer [1] == 0xfe) {
					if (input_buffer [2] == 0 && input_buffer[3] == 0) {
						this.encoding = Encoding.UTF32;
						return 4;
					}

					this.encoding = Encoding.Unicode;
					return 2;
				}
			}

			return 0;
		}

		public void DiscardBufferedData ()
		{
			pos = decoded_count = 0;
			mayBlock = false;
			// Discard internal state of the decoder too.
			decoder = encoding.GetDecoder ();
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
				
				if (cbEncoded <= 0)
					return 0;

				mayBlock = (cbEncoded < buffer_size);
				if (do_checks > 0){
					Encoding old = encoding;
					parse_start = DoChecks (cbEncoded);
					if (old != encoding){
						int old_decoded_size = old.GetMaxCharCount (buffer_size) + 1;
						int new_decoded_size = encoding.GetMaxCharCount (buffer_size) + 1;
						if (old_decoded_size != new_decoded_size)
							decoded_buffer = new char [new_decoded_size];
						decoder = encoding.GetDecoder ();
					}
					do_checks = 0;
					cbEncoded -= parse_start;
				}
				
				decoded_count += decoder.GetChars (input_buffer, parse_start, cbEncoded, decoded_buffer, 0);
				parse_start = 0;
			} while (decoded_count == 0);

			return decoded_count;
		}

		//
		// Peek can block:
		// http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=96484
		//
		public override int Peek ()
		{
			if (base_stream == null)
				throw new ObjectDisposedException ("StreamReader", "Cannot read from a closed StreamReader");
			if (pos >= decoded_count && ReadBuffer () == 0)
				return -1;

			return decoded_buffer [pos];
		}

		//
		// Used internally by our console, as it previously depended on Peek() being a
		// routine that would not block.
		//
		internal bool DataAvailable ()
		{
			return pos < decoded_count;
		}
		
		public override int Read ()
		{
			if (base_stream == null)
				throw new ObjectDisposedException ("StreamReader", "Cannot read from a closed StreamReader");
			if (pos >= decoded_count && ReadBuffer () == 0)
				return -1;

			return decoded_buffer [pos++];
		}

		public override int Read ([In, Out] char[] buffer, int index, int count)
		{
			if (base_stream == null)
				throw new ObjectDisposedException ("StreamReader", "Cannot read from a closed StreamReader");
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index", "< 0");
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "< 0");
			// re-ordered to avoid possible integer overflow
			if (index > buffer.Length - count)
				throw new ArgumentException ("index + count > buffer.Length");

			int chars_read = 0;
			while (count > 0)
			{
				if (pos >= decoded_count && ReadBuffer () == 0)
					return chars_read > 0 ? chars_read : 0;

				int cch = Math.Min (decoded_count - pos, count);
				Array.Copy (decoded_buffer, pos, buffer, index, cch);
				pos += cch;
				index += cch;
				count -= cch;
				chars_read += cch;
				if (mayBlock)
					break;
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
					if (pos == 0)
						return -2; // Need to flush the current buffered line.
							   // This is a \r at the end of the previous decoded buffer that
							   // is not followed by a \n in the current decoded buffer.
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
			else if (end == -2)
				return line_builder.ToString (0, line_builder.Length);

			if (line_builder == null)
				line_builder = new StringBuilder ();
			else
				line_builder.Length = 0;

			while (true) {
				if (foundCR) // don't include the trailing CR if present
					decoded_count--;

				line_builder.Append (decoded_buffer, begin, decoded_count - begin);
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
					line_builder.Append (decoded_buffer, begin, end - begin);
					if (line_builder.Capacity > 32768) {
						StringBuilder sb = line_builder;
						line_builder = null;
						return sb.ToString (0, sb.Length);
					}
					return line_builder.ToString (0, line_builder.Length);
				} else if (end == -2)
					return line_builder.ToString (0, line_builder.Length);
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
			
			while ((len = Read (buffer, 0, size)) > 0)
				text.Append (buffer, 0, len);

			return text.ToString ();
		}
	}
}
