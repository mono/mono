//
// System.IO.StreamWriter.cs
//
// Author:
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Text;

namespace System.IO {
	
        public class StreamWriter : TextWriter {

		private Encoding internalEncoding;

		private Stream internalStream;

		private bool iflush;
		
                // new public static readonly StreamWriter Null;

		public StreamWriter (Stream stream)
			: this (stream, null, 0) {}

		public StreamWriter (Stream stream, Encoding encoding)
			: this (stream, encoding, 0) {}

		public StreamWriter (Stream stream, Encoding encoding, int bufferSize)
		{
			internalStream = stream;

			if (encoding == null)
				internalEncoding = Encoding.UTF8;
			else
				internalEncoding = encoding;
		}

		public StreamWriter (string path)
			: this (path, true, null, 0) {}

		public StreamWriter (string path, bool append)
			: this (path, append, null, 0) {}

		public StreamWriter (string path, bool append, Encoding encoding)
			: this (path, append, encoding, 0) {}
		
		public StreamWriter (string path, bool append, Encoding encoding, int bufferSize)
		{
			FileMode mode;

			if (append)
				mode = FileMode.Append;
			else
				mode = FileMode.Create;
			
			internalStream = new FileStream (path, mode, FileAccess.Write);

			if (encoding == null)
				internalEncoding = Encoding.UTF8;
			else
				internalEncoding = encoding;
			
		}

		public virtual bool AutoFlush
		{

			get {
				return iflush;
			}

			set {
				iflush = value;
			}
		}

		public virtual Stream BaseStream
		{
			get {
				return internalStream;
			}
		}

		public override Encoding Encoding
		{
			get {
				return internalEncoding;
			}
		}

		protected override void Dispose( bool disposing )
		{
			// fixme: implement me			
		}

		public override void Flush ()
		{
			// fixme: implement me
		}
		
		public override void Write (char[] buffer, int index, int count)
		{
			byte[] res = new byte [internalEncoding.GetMaxByteCount (buffer.Length)];
			int len;
			
			len = internalEncoding.GetBytes (buffer, index, count, res, 0);

			internalStream.Write (res, 0, len);
			
		}

		public override void Write(string value)
		{
			Write (value.ToCharArray (), 0, value.Length);
		}

                         
        }
}
                        
                        
