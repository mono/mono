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
			if (access < FileAccess.Read || access > FileAccess.ReadWrite)
				throw new ArgumentOutOfRangeException ("access");

			this.handle = handle;
			this.access = access;
			this.owner = ownsHandle;
			this.async = isAsync;

			MonoIOError error;
			MonoFileType ftype = MonoIO.GetFileType (handle, out error);
			
			if (ftype == MonoFileType.Unknown) {
				throw new IOException ("Invalid handle.");
			} else if (ftype == MonoFileType.Disk) {
				this.canseek = true;
			} else {
				this.canseek = false;
			}
			
			InitBuffer (bufferSize);

			/* Can't set append mode */
			this.append_startpos=0;
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

			if (mode < FileMode.CreateNew || mode > FileMode.Append)
				throw new ArgumentOutOfRangeException ("mode");

			if (access < FileAccess.Read || access > FileAccess.ReadWrite)
				throw new ArgumentOutOfRangeException ("access");

			if (share < FileShare.None || share > FileShare.ReadWrite)
				throw new ArgumentOutOfRangeException ("share");

			if (name.IndexOfAny (Path.InvalidPathChars) != -1) {
				throw new ArgumentException ("Name has invalid chars");
			}

			if (Directory.Exists (name)) {
				throw new UnauthorizedAccessException ("Access to the path '" + Path.GetFullPath (name) + "' is denied.");
			}

			InitBuffer (bufferSize);

			/* Append streams can't be read (see FileMode
			 * docs)
			 */
			if (mode==FileMode.Append &&
			    (access&FileAccess.Read)==FileAccess.Read) {
				throw new ArgumentException("Append streams can not be read");
			}

			if ((access & FileAccess.Write) == 0 &&
			    (mode != FileMode.Open && mode != FileMode.OpenOrCreate))
				throw new ArgumentException ("access and mode not compatible");

			this.name = name;

			// TODO: demand permissions

			MonoIOError error;

			this.handle = MonoIO.Open (name, mode, access, share,
						   out error);
			if (handle == MonoIO.InvalidHandle) {
				throw MonoIO.GetException (name, error);
			}

			this.access = access;
			this.owner = true;
			this.async = isAsync;

			/* Can we open non-files by name? */
			
			if (MonoIO.GetFileType (handle, out error) ==
			    MonoFileType.Disk) {
				this.canseek = true;
			} else {
				this.canseek = false;
			}


			if (mode==FileMode.Append) {
				this.Seek (0, SeekOrigin.End);
				this.append_startpos=this.Position;
			} else {
				this.append_startpos=0;
			}
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

		public virtual bool IsAsync {
			get {
				return (async);
			}
		}

		public string Name {
			get {
				return name; 
			}
		}

		public override long Length {
			get {
				if (handle == MonoIO.InvalidHandle)
					throw new ObjectDisposedException ("Stream has been closed");

				if (!canseek)
					throw new NotSupportedException ("The stream does not support seeking");

				// Buffered data might change the length of the stream
				FlushBufferIfDirty ();

				MonoIOError error;
				
				return MonoIO.GetLength (handle, out error);
			}
		}

		public override long Position {
			get {
				if (handle == MonoIO.InvalidHandle)
					throw new ObjectDisposedException ("Stream has been closed");

				if(CanSeek == false)
					throw new NotSupportedException("The stream does not support seeking");
				
				lock(this) {
					return(buf_start + buf_offset);
				}
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
			get {
				return handle;
			}
		}

		// methods

		public override int ReadByte ()
		{
			if (handle == MonoIO.InvalidHandle)
				throw new ObjectDisposedException ("Stream has been closed");

			if (!CanRead)
				throw new NotSupportedException ("Stream does not support reading");
			
			lock(this) {
				if (buf_offset >= buf_length) {
					RefillBuffer ();

					if (buf_length == 0) {
						return -1;
					}
				}
				
				return(buf [buf_offset ++]);
			}
		}

		public override void WriteByte (byte value)
		{
			if (handle == MonoIO.InvalidHandle)
				throw new ObjectDisposedException ("Stream has been closed");

			if (!CanWrite)
				throw new NotSupportedException ("Stream does not support writing");

			lock(this) {
				if (buf_offset == buf_size) {
					FlushBuffer ();
				}

				buf [buf_offset ++] = value;
				if (buf_offset > buf_length) {
					buf_length = buf_offset;
				}

				buf_dirty = true;
			}
		}

		public override int Read (byte[] dest, int dest_offset, int count)
		{
			if (handle == MonoIO.InvalidHandle)
				throw new ObjectDisposedException ("Stream has been closed");
			if (dest == null)
				throw new ArgumentNullException ("destination array is null");
			if (!CanRead)
				throw new NotSupportedException ("Stream does not support reading");
			int len = dest.Length;
			if (dest_offset < 0 || count < 0)
				throw new ArgumentException ("dest or count is negative");
			if (dest_offset > len)
				throw new ArgumentException ("destination offset is beyond array size");
			if ((dest_offset + count) > len)
				throw new ArgumentException ("Reading would overrun buffer");

			
			int copied = 0;

			lock(this) {
				int n = ReadSegment (dest, dest_offset, count);
				copied += n;
				count -= n;
			
				if (count == 0) {
					/* If there was already enough
					 * buffered, no need to read
					 * more from the file.
					 */
					return (copied);
				}

				if (count > buf_size) {
					/* Read as much as we can, up
					 * to count bytes
					 */
					FlushBuffer();
					n = ReadData (handle, dest,
						      dest_offset+copied,
						      count);
				
					/* Make the next buffer read
					 * start from the right place
					 */
					buf_start += n;
				} else {
					RefillBuffer ();
					n = ReadSegment (dest,
							 dest_offset+copied,
							 count);
				}

				copied += n;

				return(copied);
			}
		}

		public override void Write (byte[] src, int src_offset, int count)
		{
			if (handle == MonoIO.InvalidHandle)
				throw new ObjectDisposedException ("Stream has been closed");

			if (src == null)
				throw new ArgumentNullException ("src");

			if (src_offset < 0)
				throw new ArgumentOutOfRangeException ("src_offset");

			if (count < 0)
				throw new ArgumentOutOfRangeException ("count");

			if (!CanWrite)
				throw new NotSupportedException ("Stream does not support writing");

			if (count > buf_size) {
				// shortcut for long writes
				MonoIOError error;

				FlushBuffer ();

				MonoIO.Write (handle, src, src_offset, count, out error);
				buf_start += count;
			} else {

				int copied = 0;
				while (count > 0) {
					
					int n = WriteSegment (src, src_offset + copied, count);
					copied += n;
					count -= n;

					if (count == 0) {
						break;
					}

					FlushBuffer ();
				}
			}
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			long pos;

			if (handle == MonoIO.InvalidHandle)
				throw new ObjectDisposedException ("Stream has been closed");
			
			// make absolute

			if(CanSeek == false) {
				throw new NotSupportedException("The stream does not support seeking");
			}

			lock(this) {

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
					throw new IOException("Attempted to Seek before the beginning of the stream");
				}

				if(pos < this.append_startpos) {
					/* More undocumented crap */
					throw new IOException("Can't seek back over pre-existing data in append mode");
				}

				if (buf_length > 0) {
					if (pos >= buf_start &&
						pos <= buf_start + buf_length) {
						buf_offset = (int) (pos - buf_start);
						return pos;
					}
				}

				FlushBuffer ();

				MonoIOError error;
			
				buf_start = MonoIO.Seek (handle, pos,
							 SeekOrigin.Begin,
							 out error);
				
				return(buf_start);
			}
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

			MonoIOError error;
			
			MonoIO.SetLength (handle, length, out error);
		}

		public override void Flush ()
		{
			if (handle == MonoIO.InvalidHandle)
				throw new ObjectDisposedException ("Stream has been closed");

			lock(this) {
				FlushBuffer ();
			}
			
			// The flushing is not actually required, in
			//the mono runtime we were mapping flush to
			//`fsync' which is not the same.
			//
			//MonoIO.Flush (handle);
		}

		public override void Close ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);	// remove from finalize queue
		}

		[MonoTODO]
		public virtual void Lock (long position, long length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Unlock (long position, long length)
		{
			throw new NotImplementedException ();
		}

		// protected

		~FileStream ()
		{
			Dispose (false);
		}

		protected virtual void Dispose (bool disposing) {
			if (owner && handle != MonoIO.InvalidHandle) {
				lock(this) {
					FlushBuffer ();
				}

				MonoIOError error;
				
				MonoIO.Close (handle, out error);

				handle = MonoIO.InvalidHandle;
			}

			canseek = false;
			access = 0;
			if (disposing) {
				buf = null;
			}
		}

		// private.

		// ReadSegment, WriteSegment, FlushBuffer,
		// RefillBuffer and ReadData should only be called
		// when the Monitor lock is held, but these methods
		// grab it again just to be safe.

		private int ReadSegment (byte [] dest, int dest_offset,
					 int count)
		{
			if (count > buf_length - buf_offset) {
				count = buf_length - buf_offset;
			}
			
			if (count > 0) {
				Buffer.BlockCopy (buf, buf_offset,
						  dest, dest_offset,
						  count);
				buf_offset += count;
			}
			
			return(count);
		}

		private int WriteSegment (byte [] src, int src_offset,
					  int count)
		{
			if (count > buf_size - buf_offset) {
				count = buf_size - buf_offset;
			}
			
			if (count > 0) {
				Buffer.BlockCopy (src, src_offset,
						  buf, buf_offset,
						  count);
				buf_offset += count;
				if (buf_offset > buf_length) {
					buf_length = buf_offset;
				}
				
				buf_dirty = true;
			}
			
			return(count);
		}

		private void FlushBuffer ()
		{
			if (buf_dirty) {
				MonoIOError error;
				
				if (CanSeek == true) {
					MonoIO.Seek (handle, buf_start,
						     SeekOrigin.Begin,
						     out error);
				}
				MonoIO.Write (handle, buf, 0,
					      buf_length, out error);
			}

			buf_start += buf_offset;
			buf_offset = buf_length = 0;
			buf_dirty = false;
		}

		private void FlushBufferIfDirty ()
		{
			if (buf_dirty)
				FlushBuffer ();
		}

		private void RefillBuffer ()
		{
			FlushBuffer();
			
			buf_length = ReadData (handle, buf, 0,
					       buf_size);
		}

		private int ReadData (IntPtr handle, byte[] buf, int offset,
				      int count)
		{
			MonoIOError error;
			
			int amount = MonoIO.Read (handle, buf, offset,
						  count, out error);
			
			/* Check for read error */
			if(amount == -1) {
				/* Kludge around broken pipes */
				if(error == MonoIOError.ERROR_BROKEN_PIPE) {
					amount = 0;
				} else {
					throw new IOException ();
				}
			}
			
			return(amount);
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
		private long append_startpos;
		

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
