//
// System.IO.StreamReader.cs
//
// Author:
//   Dietmar Maurer (dietmar@ximian.com)
//   Miguel de Icaza (miguel@ximian.com) 
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

		//
		// The input buffer
		//
		private byte [] input_buffer;

		//
		// The decoded buffer from the above input buffer
		//
		private char [] decoded_buffer;

		//
		// Decoded bytes in decoded_buffer.
		//
		private int decoded_count;

		//
		// Current position in the decoded_buffer
		//
		private int pos;

		//
		// The buffer size that we are using
		//
		private int buffer_size;

		int do_checks;
		
		private Encoding encoding;
		private Decoder decoder;

		private Stream base_stream;
		private bool mayBlock;

		private class NullStreamReader : StreamReader {
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

		public new static readonly StreamReader Null =  (StreamReader)(new NullStreamReader());
		
		internal StreamReader() {}

		public StreamReader(Stream stream)
			: this (stream, Encoding.UTF8Unmarked, true, DefaultBufferSize) { }

		public StreamReader(Stream stream, bool detect_encoding_from_bytemarks)
			: this (stream, Encoding.UTF8Unmarked, detect_encoding_from_bytemarks, DefaultBufferSize) { }

		public StreamReader(Stream stream, Encoding encoding)
			: this (stream, encoding, true, DefaultBufferSize) { }

		public StreamReader(Stream stream, Encoding encoding, bool detect_encoding_from_bytemarks)
			: this (stream, encoding, detect_encoding_from_bytemarks, DefaultBufferSize) { }
		
		public StreamReader(Stream stream, Encoding encoding, bool detect_encoding_from_bytemarks, int buffer_size)
		{
			Initialize (stream, encoding, detect_encoding_from_bytemarks, buffer_size);
		}

		public StreamReader(string path)
			: this (path, Encoding.UTF8Unmarked, true, DefaultFileBufferSize) { }

		public StreamReader(string path, bool detect_encoding_from_bytemarks)
			: this (path, Encoding.UTF8Unmarked, detect_encoding_from_bytemarks, DefaultFileBufferSize) { }

		public StreamReader(string path, Encoding encoding)
			: this (path, encoding, true, DefaultFileBufferSize) { }

		public StreamReader(string path, Encoding encoding, bool detect_encoding_from_bytemarks)
			: this (path, encoding, detect_encoding_from_bytemarks, DefaultFileBufferSize) { }
		
		public StreamReader(string path, Encoding encoding, bool detect_encoding_from_bytemarks, int buffer_size)
		{
			if (null == path)
				throw new ArgumentNullException("path");
			if (String.Empty == path)
				throw new ArgumentException("Empty path not allowed");
			if (path.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException("path contains invalid characters");

			string DirName = Path.GetDirectoryName(path);
			if (DirName != String.Empty && !Directory.Exists(DirName))
				throw new DirectoryNotFoundException ("Directory '" + DirName + "' not found.");
			if (!File.Exists(path))
				throw new FileNotFoundException("File not found.", path);

			Stream stream = (Stream) File.OpenRead (path);
			Initialize (stream, encoding, detect_encoding_from_bytemarks, buffer_size);
		}

		internal void Initialize (Stream stream, Encoding encoding, bool detect_encoding_from_bytemarks, int buffer_size)
		{
			if (null == stream)
				throw new ArgumentNullException("stream");
			if (!stream.CanRead)
				throw new ArgumentException("Cannot read stream");

			if (buffer_size < MinimumBufferSize)
				buffer_size = MinimumBufferSize;

			base_stream = stream;
			input_buffer = new byte [buffer_size];
			this.buffer_size = buffer_size;
			this.encoding = encoding;
			decoder = encoding.GetDecoder ();

			byte [] preamble = encoding.GetPreamble ();
			do_checks = detect_encoding_from_bytemarks ? 1 : 0;
			do_checks += (preamble.Length == 0) ? 0 : 2;
			
			decoded_buffer = new char [encoding.GetMaxCharCount (buffer_size)];
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

				if (input_buffer [0] == 0xff && input_buffer [1] == 0xfe){
					this.encoding = Encoding.Unicode;
					return 2;
				}

				if (count < 3)
					return 0;

				if (input_buffer [0] == 0xef && input_buffer [1] == 0xbb && input_buffer [2] == 0xbf){
					this.encoding = Encoding.UTF8Unmarked;
					return 3;
				}
			}

			return 0;
		}

		public void DiscardBufferedData ()
		{
			pos = decoded_count = 0;
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
				if (do_checks > 0){
					Encoding old = encoding;
					parse_start = DoChecks (cbEncoded);
					if (old != encoding){
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

		public override int Peek ()
		{
			if (pos >= decoded_count && (mayBlock || ReadBuffer () == 0))
				return -1;

			return decoded_buffer [pos];
		}

		public override int Read ()
		{
			if (pos >= decoded_count && ReadBuffer () == 0)
				return -1;

			return decoded_buffer [pos++];
		}

		public override int Read (char[] dest_buffer, int index, int count)
		{
			if (dest_buffer == null)
				throw new ArgumentException ();

			if ((index < 0) || (count < 0))
				throw new ArgumentOutOfRangeException ();

			if (index + count > dest_buffer.Length)
				throw new ArgumentException ();

			int chars_read = 0;
			while (count > 0)
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

		public override string ReadLine()
		{
			bool foundCR = false;
			StringBuilder text = new StringBuilder ();

			while (true) {
				int c = Read ();

				if (c == -1) {				// end of stream
					if (text.Length == 0)
						return null;

					if (foundCR)
						text.Length--;

					break;
				}

				if (c == '\n') {			// newline
					if ((text.Length > 0) && (text [text.Length - 1] == '\r'))
						text.Length--;

					foundCR = false;
					break;
				} else if (foundCR) {
					pos--;
					text.Length--;
					break;
				}

				if (c == '\r')
					foundCR = true;
					

				text.Append ((char) c);
			}

			return text.ToString ();
		}

		public override string ReadToEnd()
		{
			StringBuilder text = new StringBuilder ();

			int size = decoded_buffer.Length;
			char [] buffer = new char [size];
			int len;
			
			while ((len = Read (buffer, 0, size)) != 0)
				text.Append (buffer, 0, len);

			return text.ToString ();
		}
	}
}
