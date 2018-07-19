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

		public ZlibFileFuncDef32 IOFunctions32 {
			get; set;
		}

		public ZlibFileFuncDef64 IOFunctions64 {
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
			
			ZlibFileFuncDef32 f32 = new ZlibFileFuncDef32 ();
			f32.opaque = IntPtr.Zero;
			f32.zclose_file = CloseFile_Native;
			f32.zerror_file = TestError_Native;
			f32.zopen_file = OpenFile_Native;
			f32.zread_file = ReadFile_Native32;
			f32.zseek_file = SeekFile_Native32;
			f32.ztell_file = TellFile_Native32;
			f32.zwrite_file = WriteFile_Native32;
			IOFunctions32 = f32;

			ZlibFileFuncDef64 f64 = new ZlibFileFuncDef64 ();
			f64.opaque = IntPtr.Zero;
			f64.zclose_file = CloseFile_Native;
			f64.zerror_file = TestError_Native;
			f64.zopen_file = OpenFile_Native;
			f64.zread_file = ReadFile_Native64;
			f64.zseek_file = SeekFile_Native64;
			f64.ztell_file = TellFile_Native64;
			f64.zwrite_file = WriteFile_Native64;
			IOFunctions64 = f64;
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

		unsafe uint ReadFile_Native32 (IntPtr opaque, IntPtr stream, IntPtr buffer, uint size)
		{
			return (uint) ReadFile_Native64 (opaque, stream, buffer, size);
		}

		unsafe ulong ReadFile_Native64 (IntPtr opaque, IntPtr stream, IntPtr buffer, ulong size)
		{
			int count = (int) size;
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

			return (ulong) read;
		}

		int SeekFile_Native32 (IntPtr opaque, IntPtr stream, uint offset, int origin)
		{
			return (int) SeekFile_Native64 (opaque, stream, offset, origin);
		}

		long SeekFile_Native64 (IntPtr opaque, IntPtr stream, ulong offset, int origin)
		{
			SeekOrigin seek;
			if (origin == ZipStream.ZLIB_FILEFUNC_SEEK_CUR)
				seek = SeekOrigin.Current;
			else if (origin == ZLIB_FILEFUNC_SEEK_END)
				seek = SeekOrigin.End;
			else if (origin == ZLIB_FILEFUNC_SEEK_SET)
				seek = SeekOrigin.Begin;
			else
				return -1;

			Seek ((long) offset, seek);
			
			return 0;
		}

		int TellFile_Native32 (IntPtr opaque, IntPtr stream)
		{
			return (int) TellFile_Native64 (opaque, stream);
		}

		long TellFile_Native64 (IntPtr opaque, IntPtr stream)
		{
			return Position;
		}

		int TestError_Native (IntPtr opaque, IntPtr stream)
		{
			// No errors here.
			return 0;
		}

		unsafe uint WriteFile_Native32 (IntPtr opaque, IntPtr stream, IntPtr buffer, /* ulong */ uint size)
		{
			return (uint) WriteFile_Native64 (opaque, stream, buffer, size);
		}

		unsafe ulong WriteFile_Native64 (IntPtr opaque, IntPtr stream, IntPtr buffer, /* ulong */ ulong size)
		{
			int count = (int) size;
			byte[] b = new byte[count];

			byte* ptrBuffer = (byte*) buffer.ToPointer ();
			for (int i = 0; i < count; i ++)
				b[i] = ptrBuffer[i];

			try {
				Write (b, 0, count);
			} catch {
				
			}

			return (ulong) count;
		}
	}
}
