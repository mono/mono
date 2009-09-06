// ZipWriteStream.cs created with MonoDevelop
// User: alan at 16:54 20/10/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.IO;

namespace zipsharp
{
	class ZipWriteStream : Stream
	{
		ZipArchive Archive { get; set; }
		
		public override bool CanRead {
			get { return false; }
		}
		
		public override bool CanSeek {
			get { return false; }
		}

		public override bool CanWrite {
			get { return true; }
		}

		public override bool CanTimeout {
			get { return false; }
		}

		public override long Length {
			get {
				throw new NotSupportedException ();
			}
		}

		public override long Position {
			get {
				 throw new NotSupportedException ();
			}
			set {
				throw new NotSupportedException ();
			}
		}
		
		public ZipWriteStream (ZipArchive archive)
		{
			Archive = archive;
			Archive.FileActive = true;
		}

		public override void Close()
		{
			NativeZip.CloseFile (Archive.Handle);
			Archive.FileActive = false;
		}

		public override void Flush()
		{
			
		}
 
		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException ();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException ();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException ();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			NativeZip.Write (Archive.Handle, buffer, offset, (uint)count);
		}
	}
}
