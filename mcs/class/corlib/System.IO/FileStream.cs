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
			if (name == null)
				throw new ArgumentNullException ();
			if (name == "" || name.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ();

			// TODO: demand permissions

			this.handle = FileOpen (name, mode, access, share);
			this.access = access;
			this.owner = true;
			this.async = isAsync;
			
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
                                return true;	// FIXME: false for pipes & streams
                        }
                }

		public override long Length {
			get { return FileGetLength (handle); }
		}

		public override long Position {
			get { return buf_start + buf_offset; }
			set { Seek (value, SeekOrigin.Begin); }
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
			while (count > 0) {
				int n = ReadSegment (dest, dest_offset + copied, count);
				copied += n;
				count -= n;

				if (count == 0)
					break;

				if (count > buf_size) {	// shortcut for long reads
					FlushBuffer ();

					FileSeek (handle, buf_start, SeekOrigin.Begin);
					n = FileRead (handle, dest, dest_offset + copied, count);

					copied += n;
					buf_start += n;
					break;
				}

				RefillBuffer ();
				if (buf_length == 0)
					break;
			}

			return copied;
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
					FileWrite (handle, src, src_offset + copied, count);
					buf_start += count;
					break;
				}
			}
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			long pos;

			// make absolute
		
			switch (origin) {
			case SeekOrigin.End:
				pos = Length - offset;
				break;
			
			case SeekOrigin.Current:
				pos = Position + offset;
				break;
				
			case SeekOrigin.Begin: default:
				pos = offset;
				break;
			}

			if (pos >= buf_start && pos <= buf_start + buf_length) {
				buf_offset = (int) (pos - buf_start);
				return pos;
			}

			FlushBuffer ();
			buf_start = FileSeek (handle, pos, SeekOrigin.Begin);

			return buf_start;
		}

		public override void SetLength (long length)
		{
			FileSetLength (handle, length);
		}

		public override void Flush ()
		{
			FlushBuffer ();
			FileFlush (handle);
		}

		public override void Close ()
		{
			if (owner)
				FileClose (handle);
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
				FileSeek (handle, buf_start, SeekOrigin.Begin);
				FileWrite (handle, buf, 0, buf_length);
			}

			buf_start += buf_length;
			buf_offset = buf_length = 0;
			buf_dirty = false;
		}

		private void RefillBuffer ()
		{
			FlushBuffer ();
			
			FileSeek (handle, buf_start, SeekOrigin.Begin);
			buf_length = FileRead (handle, buf, 0, buf_size);
		}

		private void InitBuffer (int size)
		{
			buf = new byte [size];
			buf_size = size;
			buf_start = 0;
			buf_offset = buf_length = 0;
			buf_dirty = false;
		}

		// internal calls
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static IntPtr FileOpen (string filename, FileMode mode, FileAccess access, FileShare share);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static void FileClose (IntPtr handle);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static int FileRead (IntPtr handle, byte [] dest, int dest_offset, int count);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static int FileWrite (IntPtr handle, byte [] src, int src_offset, int count);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static long FileSeek (IntPtr handle, long offset, SeekOrigin origin);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static long FileGetLength (IntPtr handle);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static void FileSetLength (IntPtr handle, long length);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static void FileFlush (IntPtr handle);

		// fields

		private static int DefaultBufferSize = 8192;

		private FileAccess access;
		private bool owner;
		private bool async;

		private byte [] buf;			// the buffer
		private int buf_size;			// capacity in bytes
		private int buf_length;			// number of valid bytes in buffer
		private int buf_offset;			// position of next byte
		private bool buf_dirty;			// true if buffer has been written to
		private long buf_start;			// location of buffer in file

		IntPtr handle;				// handle to underlying file
	}
}
