// ZipStream.cs created with MonoDevelop
// User: alan at 15:16 20/10/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace zipsharp
{
	class ZipStream : Stream
	{
		const int ZLIB_FILEFUNC_SEEK_CUR = 1;
		const int ZLIB_FILEFUNC_SEEK_END = 2;
		const int ZLIB_FILEFUNC_SEEK_SET = 0;
		
		bool canRead;
		bool canSeek;
		bool canWrite;
		
		public override bool CanRead {
			get { return canRead; }
		}

		public override bool CanSeek {
			get { return canSeek; }
		}

		public override bool CanWrite {
			get { return canWrite; }
		}

		public override bool CanTimeout {
			get { return false; }
		}
		
		private Stream DataStream {
			get; set;
		}

		public ZlibFileFuncDef IOFunctions {
			get; set;
		}

		public override long Length {
			get { return DataStream.Length; }
		}

		bool OwnsStream {
			get; set;
		}
		
		public override long Position {
			get { return DataStream.Position; }
			set { DataStream.Position = value; }
		}
		
		public ZipStream (Stream dataStream, bool ownsStream)
		{
			// FIXME: Not necessarily true
			canRead = true;
			canSeek = true;
			canWrite = true;

			DataStream = dataStream;
			OwnsStream = ownsStream;
			
			ZlibFileFuncDef f = new ZlibFileFuncDef();
			
			f.opaque = IntPtr.Zero;
			f.zclose_file = CloseFile_Native;
			f.zerror_file = TestError_Native;
			f.zopen_file = OpenFile_Native;
			f.zread_file = ReadFile_Native;
			f.zseek_file = SeekFile_Native;
			f.ztell_file = TellFile_Native;
			f.zwrite_file = WriteFile_Native;

			IOFunctions = f;
		}

		protected override void Dispose(bool disposing)
		{
			if (!disposing)
				return;

			DataStream.Flush ();
			if (OwnsStream)
				DataStream.Dispose ();
		}

		public override void Flush()
		{
			DataStream.Flush ();
		}
		
		public override int Read(byte[] buffer, int offset, int count)
		{
			return DataStream.Read (buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			DataStream.Seek (offset, origin);
			return DataStream.Position;
		}

		public override void SetLength(long value)
		{
			DataStream.SetLength (value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			DataStream.Write (buffer, offset, count);
			Flush ();
		}

		int CloseFile_Native (IntPtr opaque, IntPtr stream)
		{
			Close ();
			return 0;
		}
		
		IntPtr OpenFile_Native (IntPtr opaque, string filename, int mode)
		{
			// always success. The stream is opened in managed code
			return new IntPtr (1);
		}

		unsafe IntPtr ReadFile_Native (IntPtr opaque, IntPtr stream, IntPtr buffer, IntPtr size)
		{
			int count = size.ToInt32 ();
			byte[] b = new byte[count];
			int read;
			
			try {
				read = Math.Max (0, Read (b, 0, count));
				byte* ptrBuffer = (byte*) buffer.ToPointer ();
				for (int i = 0; i < count && i < read; i ++)
					ptrBuffer[i] = b[i];
			} catch {
				read = -1;
			}

			return new IntPtr (read);
		}

		IntPtr SeekFile_Native (IntPtr opaque, IntPtr stream, IntPtr offset, int origin)
		{
			SeekOrigin seek;
			if (origin == ZipStream.ZLIB_FILEFUNC_SEEK_CUR)
				seek = SeekOrigin.Current;
			else if (origin == ZLIB_FILEFUNC_SEEK_END)
				seek = SeekOrigin.End;
			else if (origin == ZLIB_FILEFUNC_SEEK_SET)
				seek = SeekOrigin.Begin;
			else
				return new IntPtr (-1);

			Seek (offset.ToInt64 (), seek);
			
			return new IntPtr (0);
		}

		IntPtr TellFile_Native (IntPtr opaque, IntPtr stream)
		{
			if (IntPtr.Size == 4)
				return new IntPtr ((int)Position);
			else if (IntPtr.Size == 8)
				return new IntPtr (Position);
			else
				return new IntPtr (-1);
		}

		int TestError_Native (IntPtr opaque, IntPtr stream)
		{
			// No errors here.
			return 0;
		}

		unsafe IntPtr WriteFile_Native (IntPtr opaque, IntPtr stream, IntPtr buffer, /* ulong */ IntPtr size)
		{
			int count = size.ToInt32 ();
			byte[] b = new byte[count];

			byte* ptrBuffer = (byte*) buffer.ToPointer ();
			for (int i = 0; i < count; i ++)
				b[i] = ptrBuffer[i];

			try {
				Write (b, 0, count);
			} catch {
				
			}

			return new IntPtr (count);
		}
	}
}
