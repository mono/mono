//
// System.IO.FileStream.cs
//
// Authors:
// 	Dietmar Maurer (dietmar@ximian.com)
// 	Dan Lewis (dihlewis@yahoo.co.uk)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//  Marek Safar (marek.safar@gmail.com)
//
// (C) 2001-2003 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2005, 2008, 2010 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Security;
using System.Security.Permissions;
using System.Threading;

using Microsoft.Win32.SafeHandles;
#if NET_2_1
using System.IO.IsolatedStorage;
#else
using System.Security.AccessControl;
#endif

namespace System.IO
{
	[ComVisible (true)]
	public class FileStream : Stream
	{
		// construct from handle
		
		[Obsolete ("Use FileStream(SafeFileHandle handle, FileAccess access) instead")]
		public FileStream (IntPtr handle, FileAccess access)
			: this (handle, access, true, DefaultBufferSize, false) {}

		[Obsolete ("Use FileStream(SafeFileHandle handle, FileAccess access) instead")]
		public FileStream (IntPtr handle, FileAccess access, bool ownsHandle)
			: this (handle, access, ownsHandle, DefaultBufferSize, false) {}
		
		[Obsolete ("Use FileStream(SafeFileHandle handle, FileAccess access, int bufferSize) instead")]
		public FileStream (IntPtr handle, FileAccess access, bool ownsHandle, int bufferSize)
			: this (handle, access, ownsHandle, bufferSize, false) {}

		[Obsolete ("Use FileStream(SafeFileHandle handle, FileAccess access, int bufferSize, bool isAsync) instead")]
		public FileStream (IntPtr handle, FileAccess access, bool ownsHandle, int bufferSize, bool isAsync)
			: this (handle, access, ownsHandle, bufferSize, isAsync, false) {}

		[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
		internal FileStream (IntPtr handle, FileAccess access, bool ownsHandle, int bufferSize, bool isAsync, bool isZeroSize)
		{
			this.handle = MonoIO.InvalidHandle;
			if (handle == this.handle)
				throw new ArgumentException ("handle", Locale.GetText ("Invalid."));

			if (access < FileAccess.Read || access > FileAccess.ReadWrite)
				throw new ArgumentOutOfRangeException ("access");

			MonoIOError error;
			MonoFileType ftype = MonoIO.GetFileType (handle, out error);

			if (error != MonoIOError.ERROR_SUCCESS) {
				throw MonoIO.GetException (name, error);
			}
			
			if (ftype == MonoFileType.Unknown) {
				throw new IOException ("Invalid handle.");
			} else if (ftype == MonoFileType.Disk) {
				this.canseek = true;
			} else {
				this.canseek = false;
			}

			this.handle = handle;
			this.access = access;
			this.owner = ownsHandle;
			this.async = isAsync;
#if MOONLIGHT
			// default the browser to 'all' anonymous files and let other usage (like smcs) with 'normal'
			// (i.e. non-anonymous except for isolated storage) files and paths
			this.anonymous = SecurityManager.SecurityEnabled;
#else
			this.anonymous = false;
#endif
			if (isZeroSize)
				bufferSize = 1;

			InitBuffer (bufferSize);

			if (canseek) {
				buf_start = MonoIO.Seek (handle, 0, SeekOrigin.Current, out error);
				if (error != MonoIOError.ERROR_SUCCESS) {
					throw MonoIO.GetException (name, error);
				}
			}

			/* Can't set append mode */
			this.append_startpos=0;
		}

		// construct from filename
		
		public FileStream (string path, FileMode mode)
			: this (path, mode, (mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite), FileShare.Read, DefaultBufferSize, false, FileOptions.None)
		{
		}

		public FileStream (string path, FileMode mode, FileAccess access)
			: this (path, mode, access, access == FileAccess.Write ? FileShare.None : FileShare.Read, DefaultBufferSize, false, false)
		{
		}

		public FileStream (string path, FileMode mode, FileAccess access, FileShare share)
			: this (path, mode, access, share, DefaultBufferSize, false, FileOptions.None)
		{
		}

		public FileStream (string path, FileMode mode, FileAccess access, FileShare share, int bufferSize)
			: this (path, mode, access, share, bufferSize, false, FileOptions.None)
		{
		}

		public FileStream (string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync)
			: this (path, mode, access, share, bufferSize, useAsync ? FileOptions.Asynchronous : FileOptions.None)
		{
		}

		public FileStream (string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options)
			: this (path, mode, access, share, bufferSize, false, options)
		{
		}

#if !NET_2_1
		public FileStream (SafeFileHandle handle, FileAccess access)
			:this(handle, access, DefaultBufferSize, false)
		{
		}
		
		public FileStream (SafeFileHandle handle, FileAccess access,
				   int bufferSize)
			:this(handle, access, bufferSize, false)
		{
		}
		
		[MonoLimitationAttribute("Need to use SafeFileHandle instead of underlying handle")]
		public FileStream (SafeFileHandle handle, FileAccess access,
				   int bufferSize, bool isAsync)
			:this (handle.DangerousGetHandle (), access, false, bufferSize, isAsync)
		{
			this.safeHandle = handle;
		}

		[MonoLimitation ("This ignores the rights parameter")]
		public FileStream (string path, FileMode mode,
				   FileSystemRights rights, FileShare share,
				   int bufferSize, FileOptions options)
			: this (path, mode, (mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite), share, bufferSize, false, options)
		{
		}
		
		[MonoLimitation ("This ignores the rights and fileSecurity parameters")]
		public FileStream (string path, FileMode mode,
				   FileSystemRights rights, FileShare share,
				   int bufferSize, FileOptions options,
				   FileSecurity fileSecurity)
			: this (path, mode, (mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite), share, bufferSize, false, options)
		{
		}
#endif

		internal FileStream (string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool isAsync, bool anonymous)
			: this (path, mode, access, share, bufferSize, anonymous, isAsync ? FileOptions.Asynchronous : FileOptions.None)
		{
		}

		internal FileStream (string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool anonymous, FileOptions options)
		{
			if (path == null) {
				throw new ArgumentNullException ("path");
			}

			if (path.Length == 0) {
				throw new ArgumentException ("Path is empty");
			}

			// ignore the Inheritable flag
			share &= ~FileShare.Inheritable;

			if (bufferSize <= 0)
				throw new ArgumentOutOfRangeException ("bufferSize", "Positive number required.");

			if (mode < FileMode.CreateNew || mode > FileMode.Append) {
#if NET_2_1
				if (anonymous)
					throw new ArgumentException ("mode", "Enum value was out of legal range.");
				else
#endif
				throw new ArgumentOutOfRangeException ("mode", "Enum value was out of legal range.");
			}

			if (access < FileAccess.Read || access > FileAccess.ReadWrite) {
#if NET_2_1
				if (anonymous)
					throw new IsolatedStorageException ("Enum value for FileAccess was out of legal range.");
				else
#endif
				throw new ArgumentOutOfRangeException ("access", "Enum value was out of legal range.");
			}

			if (share < FileShare.None || share > (FileShare.ReadWrite | FileShare.Delete)) {
#if NET_2_1
				if (anonymous)
					throw new IsolatedStorageException ("Enum value for FileShare was out of legal range.");
				else
#endif
				throw new ArgumentOutOfRangeException ("share", "Enum value was out of legal range.");
			}

			if (path.IndexOfAny (Path.InvalidPathChars) != -1) {
				throw new ArgumentException ("Name has invalid chars");
			}

			if (Directory.Exists (path)) {
				// don't leak the path information for isolated storage
				string msg = Locale.GetText ("Access to the path '{0}' is denied.");
				throw new UnauthorizedAccessException (String.Format (msg, GetSecureFileName (path, false)));
			}

			/* Append streams can't be read (see FileMode
			 * docs)
			 */
			if (mode==FileMode.Append &&
				(access&FileAccess.Read)==FileAccess.Read) {
				throw new ArgumentException("Append access can be requested only in write-only mode.");
			}

			if ((access & FileAccess.Write) == 0 &&
				(mode != FileMode.Open && mode != FileMode.OpenOrCreate)) {
				string msg = Locale.GetText ("Combining FileMode: {0} with " +
					"FileAccess: {1} is invalid.");
				throw new ArgumentException (string.Format (msg, access, mode));
			}

			SecurityManager.EnsureElevatedPermissions (); // this is a no-op outside moonlight

			string dname;
			if (Path.DirectorySeparatorChar != '/' && path.IndexOf ('/') >= 0)
				dname = Path.GetDirectoryName (Path.GetFullPath (path));
			else
				dname = Path.GetDirectoryName (path);
			if (dname.Length > 0) {
				string fp = Path.GetFullPath (dname);
				if (!Directory.Exists (fp)) {
					// don't leak the path information for isolated storage
					string msg = Locale.GetText ("Could not find a part of the path \"{0}\".");
					string fname = (anonymous) ? dname : Path.GetFullPath (path);
#if NET_2_1
					// don't use GetSecureFileName for the directory name
					throw new IsolatedStorageException (String.Format (msg, fname));
#else
					throw new DirectoryNotFoundException (String.Format (msg, fname));
#endif
				}
			}

			if (access == FileAccess.Read && mode != FileMode.Create && mode != FileMode.OpenOrCreate &&
					mode != FileMode.CreateNew && !File.Exists (path)) {
				// don't leak the path information for isolated storage
				string msg = Locale.GetText ("Could not find file \"{0}\".");
				string fname = GetSecureFileName (path);
#if NET_2_1
				throw new IsolatedStorageException (String.Format (msg, fname));
#else
				throw new FileNotFoundException (String.Format (msg, fname), fname);
#endif
			}

			// IsolatedStorage needs to keep the Name property to the default "[Unknown]"
			if (!anonymous)
				this.name = path;

			// TODO: demand permissions

			MonoIOError error;

			this.handle = MonoIO.Open (path, mode, access, share, options, out error);
			if (handle == MonoIO.InvalidHandle) {
				// don't leak the path information for isolated storage
				throw MonoIO.GetException (GetSecureFileName (path), error);
			}

			this.access = access;
			this.owner = true;
			this.anonymous = anonymous;

			/* Can we open non-files by name? */
			
			if (MonoIO.GetFileType (handle, out error) == MonoFileType.Disk) {
				this.canseek = true;
				this.async = (options & FileOptions.Asynchronous) != 0;
			} else {
				this.canseek = false;
				this.async = false;
			}


			if (access == FileAccess.Read && canseek && (bufferSize == DefaultBufferSize)) {
				/* Avoid allocating a large buffer for small files */
				long len = Length;
				if (bufferSize > len) {
					bufferSize = (int)(len < 1000 ? 1000 : len);
				}
			}

			InitBuffer (bufferSize);

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
#if MOONLIGHT
				return SecurityManager.CheckElevatedPermissions () ? name : "[Unknown]";
#else
				return name;
#endif
			}
		}

		public override long Length {
			get {
				if (handle == MonoIO.InvalidHandle)
					throw new ObjectDisposedException ("Stream has been closed");

				if (!CanSeek)
					throw new NotSupportedException ("The stream does not support seeking");

				// Buffered data might change the length of the stream
				FlushBufferIfDirty ();

				MonoIOError error;
				long length;
				
				length = MonoIO.GetLength (handle, out error);
				if (error != MonoIOError.ERROR_SUCCESS) {
					// don't leak the path information for isolated storage
					throw MonoIO.GetException (GetSecureFileName (name), error);
				}

				return(length);
			}
		}

		public override long Position {
			get {
				if (handle == MonoIO.InvalidHandle)
					throw new ObjectDisposedException ("Stream has been closed");

				if(CanSeek == false)
					throw new NotSupportedException("The stream does not support seeking");
				
				return(buf_start + buf_offset);
			}
			set {
				if (handle == MonoIO.InvalidHandle)
					throw new ObjectDisposedException ("Stream has been closed");

				if(CanSeek == false) {
					throw new NotSupportedException("The stream does not support seeking");
				}

				if(value < 0) {
					throw new ArgumentOutOfRangeException("Attempt to set the position to a negative value");
				}
				
				Seek (value, SeekOrigin.Begin);
			}
		}

		[Obsolete ("Use SafeFileHandle instead")]
		public virtual IntPtr Handle {
			[SecurityPermission (SecurityAction.LinkDemand, UnmanagedCode = true)]
			[SecurityPermission (SecurityAction.InheritanceDemand, UnmanagedCode = true)]
			get {
				return handle;
			}
		}

		public virtual SafeFileHandle SafeFileHandle {
			[SecurityPermission (SecurityAction.LinkDemand, UnmanagedCode = true)]
			[SecurityPermission (SecurityAction.InheritanceDemand, UnmanagedCode = true)]
			get {
				SafeFileHandle ret;

				if (safeHandle != null)
					ret = safeHandle;
				else
					ret = new SafeFileHandle (handle, owner);

				FlushBuffer ();
				return ret;
			}
		}

		// methods

		public override int ReadByte ()
		{
			if (handle == MonoIO.InvalidHandle)
				throw new ObjectDisposedException ("Stream has been closed");

			if (!CanRead)
				throw new NotSupportedException ("Stream does not support reading");
			
			if (buf_size == 0) {
				int n = ReadData (handle, buf, 0, 1);
				if (n == 0) return -1;
				else return buf[0];
			}
			else if (buf_offset >= buf_length) {
				RefillBuffer ();

				if (buf_length == 0)
					return -1;
			}
			
			return buf [buf_offset ++];
		}

		public override void WriteByte (byte value)
		{
			if (handle == MonoIO.InvalidHandle)
				throw new ObjectDisposedException ("Stream has been closed");

			if (!CanWrite)
				throw new NotSupportedException ("Stream does not support writing");

			if (buf_offset == buf_size)
				FlushBuffer ();

			if (buf_size == 0) { // No buffering
				buf [0] = value;
				buf_dirty = true;
				buf_length = 1;
				FlushBuffer ();
				return;
			}

			buf [buf_offset ++] = value;
			if (buf_offset > buf_length)
				buf_length = buf_offset;

			buf_dirty = true;
		}

		public override int Read ([In,Out] byte[] array, int offset, int count)
		{
			if (handle == MonoIO.InvalidHandle)
				throw new ObjectDisposedException ("Stream has been closed");
			if (array == null)
				throw new ArgumentNullException ("array");
			if (!CanRead)
				throw new NotSupportedException ("Stream does not support reading");
			int len = array.Length;
			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset", "< 0");
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "< 0");
			if (offset > len)
				throw new ArgumentException ("destination offset is beyond array size");
			// reordered to avoid possible integer overflow
			if (offset > len - count)
				throw new ArgumentException ("Reading would overrun buffer");

			if (async) {
				IAsyncResult ares = BeginRead (array, offset, count, null, null);
				return EndRead (ares);
			}

			return ReadInternal (array, offset, count);
		}

		int ReadInternal (byte [] dest, int offset, int count)
		{
			int copied = 0;

			int n = ReadSegment (dest, offset, count);
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
					      offset+copied,
					      count);
			
				/* Make the next buffer read
				 * start from the right place
				 */
				buf_start += n;
			} else {
				RefillBuffer ();
				n = ReadSegment (dest,
						 offset+copied,
						 count);
			}

			copied += n;

			return copied;
		}

		delegate int ReadDelegate (byte [] buffer, int offset, int count);

		public override IAsyncResult BeginRead (byte [] array, int offset, int numBytes,
							AsyncCallback userCallback, object stateObject)
		{
			if (handle == MonoIO.InvalidHandle)
				throw new ObjectDisposedException ("Stream has been closed");

			if (!CanRead)
				throw new NotSupportedException ("This stream does not support reading");

			if (array == null)
				throw new ArgumentNullException ("array");

			if (numBytes < 0)
				throw new ArgumentOutOfRangeException ("numBytes", "Must be >= 0");

			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset", "Must be >= 0");

			// reordered to avoid possible integer overflow
			if (numBytes > array.Length - offset)
				throw new ArgumentException ("Buffer too small. numBytes/offset wrong.");

			if (!async)
				return base.BeginRead (array, offset, numBytes, userCallback, stateObject);

			ReadDelegate r = new ReadDelegate (ReadInternal);
			return r.BeginInvoke (array, offset, numBytes, userCallback, stateObject);
		}
		
		public override int EndRead (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			if (!async)
				return base.EndRead (asyncResult);

			AsyncResult ares = asyncResult as AsyncResult;
			if (ares == null)
				throw new ArgumentException ("Invalid IAsyncResult", "asyncResult");

			ReadDelegate r = ares.AsyncDelegate as ReadDelegate;
			if (r == null)
				throw new ArgumentException ("Invalid IAsyncResult", "asyncResult");

			return r.EndInvoke (asyncResult);
		}

		public override void Write (byte[] array, int offset, int count)
		{
			if (handle == MonoIO.InvalidHandle)
				throw new ObjectDisposedException ("Stream has been closed");
			if (array == null)
				throw new ArgumentNullException ("array");
			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset", "< 0");
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "< 0");
			// ordered to avoid possible integer overflow
			if (offset > array.Length - count)
				throw new ArgumentException ("Reading would overrun buffer");
			if (!CanWrite)
				throw new NotSupportedException ("Stream does not support writing");

			if (async) {
				IAsyncResult ares = BeginWrite (array, offset, count, null, null);
				EndWrite (ares);
				return;
			}

			WriteInternal (array, offset, count);
		}

		void WriteInternal (byte [] src, int offset, int count)
		{
			if (count > buf_size) {
				// shortcut for long writes
				MonoIOError error;

				FlushBuffer ();
				int wcount = count;
				
				while (wcount > 0){
					int n = MonoIO.Write (handle, src, offset, wcount, out error);
					if (error != MonoIOError.ERROR_SUCCESS)
						throw MonoIO.GetException (GetSecureFileName (name), error);
					
					wcount -= n;
					offset += n;
				} 
				buf_start += count;
			} else {

				int copied = 0;
				while (count > 0) {
					
					int n = WriteSegment (src, offset + copied, count);
					copied += n;
					count -= n;

					if (count == 0) {
						break;
					}

					FlushBuffer ();
				}
			}
		}

		delegate void WriteDelegate (byte [] buffer, int offset, int count);

		public override IAsyncResult BeginWrite (byte [] array, int offset, int numBytes,
							AsyncCallback userCallback, object stateObject)
		{
			if (handle == MonoIO.InvalidHandle)
				throw new ObjectDisposedException ("Stream has been closed");

			if (!CanWrite)
				throw new NotSupportedException ("This stream does not support writing");

			if (array == null)
				throw new ArgumentNullException ("array");

			if (numBytes < 0)
				throw new ArgumentOutOfRangeException ("numBytes", "Must be >= 0");

			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset", "Must be >= 0");

			// reordered to avoid possible integer overflow
			if (numBytes > array.Length - offset)
				throw new ArgumentException ("array too small. numBytes/offset wrong.");

			if (!async)
				return base.BeginWrite (array, offset, numBytes, userCallback, stateObject);

			FileStreamAsyncResult result = new FileStreamAsyncResult (userCallback, stateObject);
			result.BytesRead = -1;
			result.Count = numBytes;
			result.OriginalCount = numBytes;

			if (buf_dirty) {
				MemoryStream ms = new MemoryStream ();
				FlushBuffer (ms);
				ms.Write (array, offset, numBytes);
				offset = 0;
				numBytes = (int) ms.Length;
			}

			WriteDelegate w = new WriteDelegate (WriteInternal);
			return w.BeginInvoke (array, offset, numBytes, userCallback, stateObject);			
		}
		
		public override void EndWrite (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			if (!async) {
				base.EndWrite (asyncResult);
				return;
			}

			AsyncResult ares = asyncResult as AsyncResult;
			if (ares == null)
				throw new ArgumentException ("Invalid IAsyncResult", "asyncResult");

			WriteDelegate w = ares.AsyncDelegate as WriteDelegate;
			if (w == null)
				throw new ArgumentException ("Invalid IAsyncResult", "asyncResult");

			w.EndInvoke (asyncResult);
			return;
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

			switch (origin) {
			case SeekOrigin.End:
				pos = Length + offset;
				break;

			case SeekOrigin.Current:
				pos = Position + offset;
				break;

			case SeekOrigin.Begin:
				pos = offset;
				break;

			default:
				throw new ArgumentException ("origin", "Invalid SeekOrigin");
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

			FlushBuffer ();

			MonoIOError error;
		
			buf_start = MonoIO.Seek (handle, pos,
						 SeekOrigin.Begin,
						 out error);

			if (error != MonoIOError.ERROR_SUCCESS) {
				// don't leak the path information for isolated storage
				throw MonoIO.GetException (GetSecureFileName (name), error);
			}
			
			return(buf_start);
		}

		public override void SetLength (long value)
		{
			if (handle == MonoIO.InvalidHandle)
				throw new ObjectDisposedException ("Stream has been closed");

			if(CanSeek == false)
				throw new NotSupportedException("The stream does not support seeking");

			if(CanWrite == false)
				throw new NotSupportedException("The stream does not support writing");

			if(value < 0)
				throw new ArgumentOutOfRangeException("value is less than 0");
			
			Flush ();

			MonoIOError error;
			
			MonoIO.SetLength (handle, value, out error);
			if (error != MonoIOError.ERROR_SUCCESS) {
				// don't leak the path information for isolated storage
				throw MonoIO.GetException (GetSecureFileName (name), error);
			}

			if (Position > value)
				Position = value;
		}

		public override void Flush ()
		{
			if (handle == MonoIO.InvalidHandle)
				throw new ObjectDisposedException ("Stream has been closed");

			FlushBuffer ();
		}

#if NET_4_0
		public virtual void Flush (bool flushToDisk)
		{
			FlushBuffer ();

			// This does the fsync
			if (flushToDisk){
				MonoIOError error;
				MonoIO.Flush (handle, out error);
			}
		}
#endif

		public virtual void Lock (long position, long length)
		{
			if (handle == MonoIO.InvalidHandle)
				throw new ObjectDisposedException ("Stream has been closed");
			if (position < 0) {
				throw new ArgumentOutOfRangeException ("position must not be negative");
			}
			if (length < 0) {
				throw new ArgumentOutOfRangeException ("length must not be negative");
			}
			if (handle == MonoIO.InvalidHandle) {
				throw new ObjectDisposedException ("Stream has been closed");
			}
				
			MonoIOError error;

			MonoIO.Lock (handle, position, length, out error);
			if (error != MonoIOError.ERROR_SUCCESS) {
				// don't leak the path information for isolated storage
				throw MonoIO.GetException (GetSecureFileName (name), error);
			}
		}

		public virtual void Unlock (long position, long length)
		{
			if (handle == MonoIO.InvalidHandle)
				throw new ObjectDisposedException ("Stream has been closed");
			if (position < 0) {
				throw new ArgumentOutOfRangeException ("position must not be negative");
			}
			if (length < 0) {
				throw new ArgumentOutOfRangeException ("length must not be negative");
			}
				
			MonoIOError error;

			MonoIO.Unlock (handle, position, length, out error);
			if (error != MonoIOError.ERROR_SUCCESS) {
				// don't leak the path information for isolated storage
				throw MonoIO.GetException (GetSecureFileName (name), error);
			}
		}

		// protected

		~FileStream ()
		{
			Dispose (false);
		}

		protected override void Dispose (bool disposing)
		{
			Exception exc = null;
			if (handle != MonoIO.InvalidHandle) {
				try {
					FlushBuffer ();
				} catch (Exception e) {
					exc = e;
				}

				if (owner) {
					MonoIOError error;
				
					MonoIO.Close (handle, out error);
					if (error != MonoIOError.ERROR_SUCCESS) {
						// don't leak the path information for isolated storage
						throw MonoIO.GetException (GetSecureFileName (name), error);
					}

					handle = MonoIO.InvalidHandle;
				}
			}

			canseek = false;
			access = 0;
			
			if (disposing && buf != null) {
				if (buf.Length == DefaultBufferSize && buf_recycle == null) {
					lock (buf_recycle_lock) {
						if (buf_recycle == null) {
							buf_recycle = buf;
						}
					}
				}
				
				buf = null;
				GC.SuppressFinalize (this);
			}
			if (exc != null)
				throw exc;
		}

#if !NET_2_1
		public FileSecurity GetAccessControl ()
		{
			throw new NotImplementedException ();
		}
		
		public void SetAccessControl (FileSecurity fileSecurity)
		{
			throw new NotImplementedException ();
		}
#endif

		// private.

		// ReadSegment, WriteSegment, FlushBuffer,
		// RefillBuffer and ReadData should only be called
		// when the Monitor lock is held, but these methods
		// grab it again just to be safe.

		private int ReadSegment (byte [] dest, int dest_offset, int count)
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

		void FlushBuffer (Stream st)
		{
			if (buf_dirty) {
				MonoIOError error;

				if (CanSeek == true) {
					MonoIO.Seek (handle, buf_start,
						     SeekOrigin.Begin,
						     out error);
					if (error != MonoIOError.ERROR_SUCCESS) {
						// don't leak the path information for isolated storage
						throw MonoIO.GetException (GetSecureFileName (name), error);
					}
				}
				if (st == null) {
					int wcount = buf_length;
					int offset = 0;
					while (wcount > 0){
						int n = MonoIO.Write (handle, buf, 0, buf_length, out error);
						if (error != MonoIOError.ERROR_SUCCESS) {
							// don't leak the path information for isolated storage
							throw MonoIO.GetException (GetSecureFileName (name), error);
						}
						wcount -= n;
						offset += n;
					}
				} else {
					st.Write (buf, 0, buf_length);
				}
			}

			buf_start += buf_offset;
			buf_offset = buf_length = 0;
			buf_dirty = false;
		}

		private void FlushBuffer ()
		{
			FlushBuffer (null);
		}

		private void FlushBufferIfDirty ()
		{
			if (buf_dirty)
				FlushBuffer (null);
		}

		private void RefillBuffer ()
		{
			FlushBuffer (null);
			
			buf_length = ReadData (handle, buf, 0,
					       buf_size);
		}

		private int ReadData (IntPtr handle, byte[] buf, int offset,
				      int count)
		{
			MonoIOError error;
			int amount = 0;

			/* when async == true, if we get here we don't suport AIO or it's disabled
			 * and we're using the threadpool */
			amount = MonoIO.Read (handle, buf, offset, count, out error);
			if (error == MonoIOError.ERROR_BROKEN_PIPE) {
				amount = 0; // might not be needed, but well...
			} else if (error != MonoIOError.ERROR_SUCCESS) {
				// don't leak the path information for isolated storage
				throw MonoIO.GetException (GetSecureFileName (name), error);
			}
			
			/* Check for read error */
			if(amount == -1) {
				throw new IOException ();
			}
			
			return(amount);
		}
				
		void InitBuffer (int size)
		{
			if (size <= 0)
				throw new ArgumentOutOfRangeException ("bufferSize", "Positive number required.");
			
			size = Math.Max (size, 8);
			
			//
			// Instead of allocating a new default buffer use the
			// last one if there is any available
			//		
			if (size <= DefaultBufferSize && buf_recycle != null) {
				lock (buf_recycle_lock) {
					if (buf_recycle != null) {
						buf = buf_recycle;
						buf_recycle = null;
					}
				}
			}
			
			if (buf == null)
				buf = new byte [size];
			else
				Array.Clear (buf, 0, size);
					
			buf_size = size;
//			buf_start = 0;
//			buf_offset = buf_length = 0;
//			buf_dirty = false;
		}

		private string GetSecureFileName (string filename)
		{
			return (anonymous) ? Path.GetFileName (filename) : Path.GetFullPath (filename);
		}

		private string GetSecureFileName (string filename, bool full)
		{
			return (anonymous) ? Path.GetFileName (filename) : 
				(full) ? Path.GetFullPath (filename) : filename;
		}

		// fields

		internal const int DefaultBufferSize = 8192;

		// Input buffer ready for recycling				
		static byte[] buf_recycle;
		static readonly object buf_recycle_lock = new object ();

		private FileAccess access;
		private bool owner;
		private bool async;
		private bool canseek;
		private long append_startpos;
		private bool anonymous;

		private byte [] buf;			// the buffer
		private int buf_size;			// capacity in bytes
		private int buf_length;			// number of valid bytes in buffer
		private int buf_offset;			// position of next byte
		private bool buf_dirty;			// true if buffer has been written to
		private long buf_start;			// location of buffer in file
		private string name = "[Unknown]";	// name of file.

		IntPtr handle;				// handle to underlying file
		SafeFileHandle safeHandle;              // set only when using one of the
							// constructors taking SafeFileHandle
	}
}
