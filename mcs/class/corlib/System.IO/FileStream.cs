//
// System.IO/FileStream.cs
//
// Authors:
//   Dietmar Maurer (dietmar@ximian.com)
//   Dan Lewis (dihlewis@yahoo.co.uk)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Runtime.CompilerServices;

// FIXME: emit the correct exceptions everywhere. add error handling.

namespace System.IO
{

	public class FileStream : Stream
	{
		// construct from handle
		
		public FileStream (IntPtr handle, FileAccess access)
			: this (handle, access, true, DefaultBufferSize, false) {}

		public FileStream (IntPtr handle, FileAccess access, bool ownsHandle)
			: this (handle, access, ownsHandle, DefaultBufferSize, false) {}
		
		public FileStream (IntPtr handle, FileAccess access, bool ownsHandle, int bufferSize)
			: this (handle, access, ownsHandle, bufferSize, false) {}

		public FileStream (IntPtr handle, FileAccess access, bool ownsHandle, int bufferSize, bool isAsync)
		{
			this.handle = handle;
			this.access = access;
			this.owner = ownsHandle;
			this.async = isAsync;

			if(MonoIO.GetFileType(handle)==MonoFileType.Disk) {
				this.canseek = true;
			} else {
				this.canseek = false;
			}
			
			InitBuffer (bufferSize);
		}

		// construct from filename
		
		public FileStream (string name, FileMode mode)
			: this (name, mode, FileAccess.ReadWrite, FileShare.ReadWrite, DefaultBufferSize, false) { }

		public FileStream (string name, FileMode mode, FileAccess access)
			: this (name, mode, access, FileShare.ReadWrite, DefaultBufferSize, false) { }

		public FileStream (string name, FileMode mode, FileAccess access, FileShare share)
			: this (name, mode, access, share, DefaultBufferSize, false) { }
		
		public FileStream (string name, FileMode mode, FileAccess access, FileShare share, int bufferSize)
			: this (name, mode, access, share, bufferSize, false) { }

		public FileStream (string name, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool isAsync)
		{
			if (name == null) {
				throw new ArgumentNullException ("Name is null");
			}
			
			if (name == "") {
				throw new ArgumentException ("Name is empty");
			}

			if (name.IndexOfAny (Path.InvalidPathChars) != -1) {
				throw new ArgumentException ("Name has invalid chars");
			}

			if (Directory.Exists (name)) {
				throw new UnauthorizedAccessException ("Access to the path '" + Path.GetFullPath (name) + "' is denied.");
			}

			this.name = name;

			// TODO: demand permissions

			this.handle = MonoIO.Open (name, mode, access, share);
			if (handle == MonoIO.InvalidHandle)
				throw MonoIO.GetException (name);

			this.access = access;
			this.owner = true;
			this.async = isAsync;

			/* Can we open non-files by name? */
			if(MonoIO.GetFileType(handle)==MonoFileType.Disk) {
				this.canseek = true;
			} else {
				this.canseek = false;
			}

			InitBuffer (bufferSize);
		}

		// properties
		
		public override bool CanRead {
			get {
				return access == FileAccess.Read ||
				       access == FileAccess.ReadWrite;
			}
		}

                public override bool CanWrite {
                        get {
				return access == FileAccess.Write ||
				       access == FileAccess.ReadWrite;
                        }
                }
		
		public override bool CanSeek {
                        get {
                                return(canseek);
                        }
                }

		public string Name {
			get {
				return name; 
			}
		}

		public override long Length {
			get { return MonoIO.GetLength (handle); }
		}

		public override long Position {
			get {
				if(CanSeek == false) {
					throw new NotSupportedException("The stream does not support seeking");
				}
				
				return(buf_start + buf_offset);
			}
			set {
				if(CanSeek == false) {
					throw new NotSupportedException("The stream does not support seeking");
				}

				if(value < 0) {
					throw new ArgumentOutOfRangeException("Attempt to set the position to a negative value");
				}
				
				Seek (value, SeekOrigin.Begin);
			}
		}

		public virtual IntPtr Handle {
			get { return handle; }
		}

		// methods

		public override int ReadByte ()
		{
			if (buf_offset >= buf_length) {
				RefillBuffer ();

				if (buf_length == 0)
					return -1;
			}

			return buf [buf_offset ++];
		}

		public override void WriteByte (byte value)
		{
			if (buf_offset == buf_size)
				FlushBuffer ();

			buf [buf_offset ++] = value;
			if (buf_offset > buf_length)
				buf_length = buf_offset;

			buf_dirty = true;
		}

		public override int Read (byte[] dest, int dest_offset, int count)
		{
			int copied = 0;

			int n = ReadSegment (dest, dest_offset, count);
			copied += n;
			count -= n;

			if(count == 0) {
				/* If there was already enough
				 * buffered, no need to read more from
				 * the file.
				 */
				return (copied);
			}

			if(count > buf_size) {
				/* Read as much as we can, up to count
				 * bytes
				 */
				FlushBuffer ();
				if (CanSeek == true) {
					MonoIO.Seek (handle, buf_start,
						     SeekOrigin.Begin);
				}
				n=MonoIO.Read (handle, dest,
					       dest_offset+copied, count);
			} else {
				RefillBuffer();
				n = ReadSegment (dest, dest_offset, count);
			}
			
			copied += n;
			buf_start += n;

			return (copied);
		}

		public override void Write (byte[] src, int src_offset, int count)
		{
			int copied = 0;
			while (count > 0) {
				int n = WriteSegment (src, src_offset + copied, count);
				copied += n;
				count -= n;

				if (count == 0)
					break;

				FlushBuffer ();

				if (count > buf_size) {	// shortcut for long writes
					MonoIO.Write (handle, src, src_offset + copied, count);
					buf_start += count;
					break;
				}
			}
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			long pos;

			// make absolute

			if(CanSeek == false) {
				throw new NotSupportedException("The stream does not support seeking");
			}
			
			switch (origin) {
			case SeekOrigin.End:
				pos = Length + offset;
				break;

			case SeekOrigin.Current:
				pos = Position + offset;
				break;

			case SeekOrigin.Begin: default:
				pos = offset;
				break;
			}

			if (pos < 0) {
				/* LAMESPEC: shouldn't this be
				 * ArgumentOutOfRangeException?
				 */
				throw new ArgumentException("Attempted to Seek before the beginning of the stream");
			}
			
			if (pos >= buf_start && pos <= buf_start + buf_length) {
				buf_offset = (int) (pos - buf_start);
				return pos;
			}

			FlushBuffer ();
			buf_start = MonoIO.Seek (handle, pos, SeekOrigin.Begin);

			return buf_start;
		}

		public override void SetLength (long length)
		{
			if(CanSeek == false) {
				throw new NotSupportedException("The stream does not support seeking");
			}

			if(CanWrite == false) {
				throw new NotSupportedException("The stream does not support writing");
			}

			if(length < 0) {
				throw new ArgumentOutOfRangeException("Length is less than 0");
			}
			
			Flush ();
			MonoIO.SetLength (handle, length);
		}

		public override void Flush ()
		{
			FlushBuffer ();

			//
			// The flushing is not actually required, in the mono runtime we were
			// mapping flush to `fsync' which is not the same.
			//
			//MonoIO.Flush (handle);
		}

		public override void Close ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);	// remove from finalize queue
		}

		// protected

		~FileStream ()
		{
			Dispose (false);
		}

		protected virtual void Dispose (bool disposing) {
			if (handle != MonoIO.InvalidHandle) {
				FlushBuffer ();
				MonoIO.Close (handle);

				handle = MonoIO.InvalidHandle;
			}

			if (disposing)
				buf = null;
		}

		// private

		private int ReadSegment (byte [] dest, int dest_offset, int count)
		{
			if (count > buf_length - buf_offset)
				count = buf_length - buf_offset;

			if (count > 0) {
				Buffer.BlockCopy (buf, buf_offset, dest, dest_offset, count);
				buf_offset += count;
			}

			return count;
		}

		private int WriteSegment (byte [] src, int src_offset, int count)
		{
			if (count > buf_size - buf_offset)
				count = buf_size - buf_offset;

			if (count > 0) {
				Buffer.BlockCopy (src, src_offset, buf, buf_offset, count);
				buf_offset += count;
				if (buf_offset > buf_length)
					buf_length = buf_offset;

				buf_dirty = true;
			}

			return count;
		}

		private void FlushBuffer ()
		{
			if (buf_dirty) {
				if (CanSeek == true) {
					MonoIO.Seek (handle, buf_start,
						     SeekOrigin.Begin);
				}
				MonoIO.Write (handle, buf, 0, buf_length);
			}

			buf_start += buf_length;
			buf_offset = buf_length = 0;
			buf_dirty = false;
		}

		private void RefillBuffer ()
		{
			FlushBuffer ();

			if (CanSeek == true) {
				MonoIO.Seek (handle, buf_start,
					     SeekOrigin.Begin);
			}
			
			buf_length = MonoIO.Read (handle, buf, 0, buf_size);
		}
		
		private void InitBuffer (int size)
		{
			if (size < 0)
				throw new ArgumentOutOfRangeException ("Buffer size cannot be negative.");
			if (size < 8)
				size = 8;
		
			buf = new byte [size];
			buf_size = size;
			buf_start = 0;
			buf_offset = buf_length = 0;
			buf_dirty = false;
		}

		// fields

		private static int DefaultBufferSize = 8192;

		private FileAccess access;
		private bool owner;
		private bool async;
		private bool canseek;

		private byte [] buf;			// the buffer
		private int buf_size;			// capacity in bytes
		private int buf_length;			// number of valid bytes in buffer
		private int buf_offset;			// position of next byte
		private bool buf_dirty;			// true if buffer has been written to
		private long buf_start;			// location of buffer in file
		private string name = "[Unknown]";	// name of file.

		IntPtr handle;				// handle to underlying file
	}
}
