//
// Mono.Unix/StdioFileStream.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2005-2006 Jonathan Pryor
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
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Mono.Unix;

namespace Mono.Unix {

	public class StdioFileStream : Stream
	{
		public static readonly IntPtr InvalidFileStream  = IntPtr.Zero;
		public static readonly IntPtr StandardInput  = Native.Stdlib.stdin;
		public static readonly IntPtr StandardOutput = Native.Stdlib.stdout;
		public static readonly IntPtr StandardError  = Native.Stdlib.stderr;

		public StdioFileStream (IntPtr fileStream)
			: this (fileStream, true) {}

		public StdioFileStream (IntPtr fileStream, bool ownsHandle)
		{
			InitStream (fileStream, ownsHandle);
		}

		public StdioFileStream (IntPtr fileStream, FileAccess access)
			: this (fileStream, access, true) {}

		public StdioFileStream (IntPtr fileStream, FileAccess access, bool ownsHandle)
		{
			InitStream (fileStream, ownsHandle);
			InitCanReadWrite (access);
		}

		public StdioFileStream (string path)
		{
			InitStream (Fopen (path, "rb"), true);
		}

		public StdioFileStream (string path, string mode)
		{
			InitStream (Fopen (path, mode), true);
		}

		public StdioFileStream (string path, FileMode mode)
		{
			InitStream (Fopen (path, ToFopenMode (path, mode)), true);
		}

		public StdioFileStream (string path, FileAccess access)
		{
			InitStream (Fopen (path, ToFopenMode (path, access)), true);
			InitCanReadWrite (access);
		}

		public StdioFileStream (string path, FileMode mode, FileAccess access)
		{
			InitStream (Fopen (path, ToFopenMode (path, mode, access)), true);
			InitCanReadWrite (access);
		}

		private static IntPtr Fopen (string path, string mode)
		{
			if (path == null)
				throw new ArgumentNullException ("path");
			if (path.Length == 0)
				throw new ArgumentException ("path");
			if (mode == null)
				throw new ArgumentNullException ("mode");
			IntPtr f = Native.Stdlib.fopen (path, mode);
			if (f == IntPtr.Zero)
				throw new DirectoryNotFoundException ("path", 
						UnixMarshal.CreateExceptionForLastError ());
			return f;
		}

		private void InitStream (IntPtr fileStream, bool ownsHandle)
		{
			if (InvalidFileStream == fileStream)
				throw new ArgumentException (Locale.GetText ("Invalid file stream"), "fileStream");
			
			this.file = fileStream;
			this.owner = ownsHandle;
			
			try {
				long offset = Native.Stdlib.fseek (file, 0, Native.SeekFlags.SEEK_CUR);
				if (offset != -1)
					canSeek = true;
				Native.Stdlib.fread (IntPtr.Zero, 0, 0, file);
				if (Native.Stdlib.ferror (file) == 0)
					canRead = true;
				Native.Stdlib.fwrite (IntPtr.Zero, 0, 0, file);
				if (Native.Stdlib.ferror (file) == 0)
					canWrite = true;  
				Native.Stdlib.clearerr (file);
			}
			catch (Exception) {
				throw new ArgumentException (Locale.GetText ("Invalid file stream"), "fileStream");
			}
			GC.KeepAlive (this);
		}

		private void InitCanReadWrite (FileAccess access)
		{
			canRead = canRead && 
				(access == FileAccess.Read || access == FileAccess.ReadWrite);
			canWrite = canWrite &&
				(access == FileAccess.Write || access == FileAccess.ReadWrite);
		}

		private static string ToFopenMode (string file, FileMode mode)
		{
			string cmode = Native.NativeConvert.ToFopenMode (mode);
			AssertFileMode (file, mode);
			return cmode;
		}

		private static string ToFopenMode (string file, FileAccess access)
		{
			return Native.NativeConvert.ToFopenMode (access);
		}

		private static string ToFopenMode (string file, FileMode mode, FileAccess access)
		{
			string cmode = Native.NativeConvert.ToFopenMode (mode, access);
			bool exists = AssertFileMode (file, mode);
			// HACK: for open-or-create & read, mode is "rb", which doesn't create
			// files.  If the file doesn't exist, we need to use "w+b" to ensure
			// file creation.
			if (mode == FileMode.OpenOrCreate && access == FileAccess.Read && !exists)
				cmode = "w+b";
			return cmode;
		}

		private static bool AssertFileMode (string file, FileMode mode)
		{
			bool exists = FileExists (file);
			if (mode == FileMode.CreateNew && exists)
				throw new IOException ("File exists and FileMode.CreateNew specified");
			if ((mode == FileMode.Open || mode == FileMode.Truncate) && !exists)
				throw new FileNotFoundException ("File doesn't exist and FileMode.Open specified", file);
			return exists;
		}

		private static bool FileExists (string file)
		{
			bool found = false;
			IntPtr f = Native.Stdlib.fopen (file, "r");
			found = f != IntPtr.Zero;
			if (f != IntPtr.Zero)
				Native.Stdlib.fclose (f);
			return found;
		}

		private void AssertNotDisposed ()
		{
			if (file == InvalidFileStream)
				throw new ObjectDisposedException ("Invalid File Stream");
			GC.KeepAlive (this);
		}

		public IntPtr Handle {
			get {
				AssertNotDisposed (); 
				GC.KeepAlive (this);
				return file;
			}
		}

		public override bool CanRead {
			get {return canRead;}
		}

		public override bool CanSeek {
			get {return canSeek;}
		}

		public override bool CanWrite {
			get {return canWrite;}
		}

		public override long Length {
			get {
				AssertNotDisposed ();
				if (!CanSeek)
					throw new NotSupportedException ("File Stream doesn't support seeking");
				long curPos = Native.Stdlib.ftell (file);
				if (curPos == -1)
					throw new NotSupportedException ("Unable to obtain current file position");
				int r = Native.Stdlib.fseek (file, 0, Native.SeekFlags.SEEK_END);
				UnixMarshal.ThrowExceptionForLastErrorIf (r);

				long endPos = Native.Stdlib.ftell (file);
				if (endPos == -1)
					UnixMarshal.ThrowExceptionForLastError ();

				r = Native.Stdlib.fseek (file, curPos, Native.SeekFlags.SEEK_SET);
				UnixMarshal.ThrowExceptionForLastErrorIf (r);

				GC.KeepAlive (this);
				return endPos;
			}
		}

		public override long Position {
			get {
				AssertNotDisposed ();
				if (!CanSeek)
					throw new NotSupportedException ("The stream does not support seeking");
				long pos = Native.Stdlib.ftell (file);
				if (pos == -1)
					UnixMarshal.ThrowExceptionForLastError ();
				GC.KeepAlive (this);
				return (long) pos;
			}
			set {
				AssertNotDisposed ();
				Seek (value, SeekOrigin.Begin);
			}
		}

		public void SaveFilePosition (Native.FilePosition pos)
		{
			AssertNotDisposed ();
			int r = Native.Stdlib.fgetpos (file, pos);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
			GC.KeepAlive (this);
		}

		public void RestoreFilePosition (Native.FilePosition pos)
		{
			AssertNotDisposed ();
			if (pos == null)
				throw new ArgumentNullException ("value");
			int r = Native.Stdlib.fsetpos (file, pos);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
			GC.KeepAlive (this);
		}

		public override void Flush ()
		{
			AssertNotDisposed ();
			int r = Native.Stdlib.fflush (file);
			if (r != 0)
				UnixMarshal.ThrowExceptionForLastError ();
			GC.KeepAlive (this);
		}

		public override unsafe int Read ([In, Out] byte[] buffer, int offset, int count)
		{
			AssertNotDisposed ();
			AssertValidBuffer (buffer, offset, count);
			if (!CanRead)
				throw new NotSupportedException ("Stream does not support reading");
				 
			ulong r = 0;
			fixed (byte* buf = &buffer[offset]) {
				r = Native.Stdlib.fread (buf, 1, (ulong) count, file);
			}
			if (r != (ulong) count) {
				if (Native.Stdlib.ferror (file) != 0)
					throw new IOException ();
			}
			GC.KeepAlive (this);
			return (int) r;
		}

		private void AssertValidBuffer (byte[] buffer, int offset, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset", "< 0");
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "< 0");
			if (offset > buffer.Length)
				throw new ArgumentException ("destination offset is beyond array size");
			if (offset > (buffer.Length - count))
				throw new ArgumentException ("would overrun buffer");
		}

		public void Rewind ()
		{
			AssertNotDisposed ();
			Native.Stdlib.rewind (file);
			GC.KeepAlive (this);
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			AssertNotDisposed ();
			if (!CanSeek)
				throw new NotSupportedException ("The File Stream does not support seeking");

			Native.SeekFlags sf = Native.SeekFlags.SEEK_CUR;
			switch (origin) {
				case SeekOrigin.Begin:   sf = Native.SeekFlags.SEEK_SET; break;
				case SeekOrigin.Current: sf = Native.SeekFlags.SEEK_CUR; break;
				case SeekOrigin.End:     sf = Native.SeekFlags.SEEK_END; break;
				default: throw new ArgumentException ("origin");
			}

			int r = Native.Stdlib.fseek (file, offset, sf);
			if (r != 0)
				throw new IOException ("Unable to seek",
						UnixMarshal.CreateExceptionForLastError ());

			long pos = Native.Stdlib.ftell (file);
			if (pos == -1)
				throw new IOException ("Unable to get current file position",
						UnixMarshal.CreateExceptionForLastError ());

			GC.KeepAlive (this);
			return pos;
		}

		public override void SetLength (long value)
		{
			throw new NotSupportedException ("ANSI C doesn't provide a way to truncate a file");
		}

		public override unsafe void Write (byte[] buffer, int offset, int count)
		{
			AssertNotDisposed ();
			AssertValidBuffer (buffer, offset, count);
			if (!CanWrite)
				throw new NotSupportedException ("File Stream does not support writing");

			ulong r = 0;
			fixed (byte* buf = &buffer[offset]) {
				r = Native.Stdlib.fwrite (buf, (ulong) 1, (ulong) count, file);
			}
			if (r != (ulong) count)
				UnixMarshal.ThrowExceptionForLastError ();
			GC.KeepAlive (this);
		}
		
		~StdioFileStream ()
		{
			Close ();
		}

		public override void Close ()
		{
			if (file == InvalidFileStream)
				return;

			if (owner) {
				int r = Native.Stdlib.fclose (file);
				if (r != 0)
					UnixMarshal.ThrowExceptionForLastError ();
			} else
				Flush ();
				
			file = InvalidFileStream;
			canRead = false;
			canSeek = false;
			canWrite = false;

			GC.SuppressFinalize (this);
			GC.KeepAlive (this);
		}
		
		private bool canSeek  = false;
		private bool canRead  = false;
		private bool canWrite = false;
		private bool owner    = true;
		private IntPtr file   = InvalidFileStream;
	}
}

// vim: noexpandtab
