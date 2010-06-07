//
// System.IO.IsolatedStorage.IsolatedStorageFileStream
//
// Authors
//	Sean MacIsaac (macisaac@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2001 Ximian, Inc.
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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
#if !MOONLIGHT
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace System.IO.IsolatedStorage {

	[ComVisible (true)]
	public class IsolatedStorageFileStream : FileStream {

		[ReflectionPermission (SecurityAction.Assert, TypeInformation = true)]
		private static string CreateIsolatedPath (IsolatedStorageFile isf, string path, FileMode mode)
		{
			if (path == null)
				throw new ArgumentNullException ("path");

			if (!Enum.IsDefined (typeof (FileMode), mode))
				throw new ArgumentException ("mode");

			if (isf == null) {
				// we can't call GetUserStoreForDomain here because it depends on 
				// Assembly.GetCallingAssembly (), which would be our constructor,
				// i.e. the result would always be mscorlib.dll. So we need to do 
				// a small stack walk to find who's calling the constructor

				StackFrame sf = new StackFrame (3); // skip self and constructor
				isf = IsolatedStorageFile.GetStore (IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly,
					IsolatedStorageFile.GetDomainIdentityFromEvidence (AppDomain.CurrentDomain.Evidence), 
					IsolatedStorageFile.GetAssemblyIdentityFromEvidence (sf.GetMethod ().ReflectedType.Assembly.UnprotectedGetEvidence ()));
			}

#if NET_4_0
			if (isf.IsDisposed)
				throw new ObjectDisposedException ("IsolatedStorageFile");
			if (isf.IsClosed)
				throw new InvalidOperationException ("Storage needs to be open for this operation.");
#endif

			// ensure that the _root_ isolated storage can be (and is) created.
			FileInfo fi = new FileInfo (isf.Root);
			if (!fi.Directory.Exists)
				fi.Directory.Create ();

			// remove the root path character(s) if they exist
			if (Path.IsPathRooted (path)) {
				string root = Path.GetPathRoot (path);
				path = path.Remove (0, root.Length);
			}

			// other directories (provided by 'path') must already exists
			string file = Path.Combine (isf.Root, path);

			string full = Path.GetFullPath (file);
			full = Path.GetFullPath (file);
			if (!full.StartsWith (isf.Root))
				throw new IsolatedStorageException ();

			fi = new FileInfo (file);
			if (!fi.Directory.Exists) {
				// don't leak the path information for isolated storage
				string msg = Locale.GetText ("Could not find a part of the path \"{0}\".");
				throw new DirectoryNotFoundException (String.Format (msg, path));
			}

			// FIXME: this is probably a good place to Assert our security
			// needs (once Mono supports imperative security stack modifiers)

			return file;
		}

		public IsolatedStorageFileStream (string path, FileMode mode)
			: this (path, mode, (mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite), FileShare.Read, DefaultBufferSize, null)
		{
		}	

		public IsolatedStorageFileStream (string path, FileMode mode, FileAccess access)
			: this (path, mode, access, access == FileAccess.Write ? FileShare.None : FileShare.Read, DefaultBufferSize, null)
		{
		}

		public IsolatedStorageFileStream (string path, FileMode mode, FileAccess access, FileShare share)
			: this (path, mode, access, share, DefaultBufferSize, null)
		{
		}

		public IsolatedStorageFileStream (string path, FileMode mode, FileAccess access, FileShare share, int bufferSize)
			: this (path, mode, access, share, bufferSize, null)
		{
		}

		// FIXME: Further limit the assertion when imperative Assert is implemented
		[FileIOPermission (SecurityAction.Assert, Unrestricted = true)]
		public IsolatedStorageFileStream (string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, IsolatedStorageFile isf)
			: base (CreateIsolatedPath (isf, path, mode), mode, access, share, bufferSize, false, true)
		{
		}

		public IsolatedStorageFileStream (string path, FileMode mode, FileAccess access, FileShare share, IsolatedStorageFile isf)
			: this (path, mode, access, share, DefaultBufferSize, isf)
		{
		}

		public IsolatedStorageFileStream (string path, FileMode mode, FileAccess access, IsolatedStorageFile isf)
			: this (path, mode, access, access == FileAccess.Write ? FileShare.None : FileShare.Read, DefaultBufferSize, isf)
		{
		}

		public IsolatedStorageFileStream (string path, FileMode mode, IsolatedStorageFile isf)
			: this (path, mode, (mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite), FileShare.Read, DefaultBufferSize, isf)
		{
		}

		public override bool CanRead {
			get {return base.CanRead;}
		}

		public override bool CanSeek {
			get {return base.CanSeek;}
		}

		public override bool CanWrite {
			get {return base.CanWrite;}
		}

		public override SafeFileHandle SafeFileHandle {
			[SecurityPermission (SecurityAction.LinkDemand, UnmanagedCode = true)]
			get {
				throw new IsolatedStorageException (
					Locale.GetText ("Information is restricted"));
			}
		}

		[Obsolete ("Use SafeFileHandle - once available")]
		public override IntPtr Handle {
			[SecurityPermission (SecurityAction.LinkDemand, UnmanagedCode = true)]
			get {
				throw new IsolatedStorageException (
					Locale.GetText ("Information is restricted"));
			}
		}

		public override bool IsAsync {
			get {return base.IsAsync;}
		}

		public override long Length {
			get {return base.Length;}
		}

		public override long Position {
			get {return base.Position;}
			set {base.Position = value;}
		}

		public override IAsyncResult BeginRead (byte[] buffer, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
		{
			return base.BeginRead (buffer, offset, numBytes, userCallback, stateObject);
		}

		public override IAsyncResult BeginWrite (byte[] buffer, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
		{
			return base.BeginWrite (buffer, offset, numBytes, userCallback, stateObject);
		}

		public override int EndRead (IAsyncResult asyncResult)
		{
			return base.EndRead (asyncResult);
		}

		public override void EndWrite (IAsyncResult asyncResult)
		{
			base.EndWrite (asyncResult);
		}

		public override void Flush ()
		{
			base.Flush ();
		}

#if NET_4_0
		public override void Flush (bool flushToDisk)
		{
			base.Flush (flushToDisk);
		}
#endif

		public override int Read (byte[] buffer, int offset, int count)
		{
			return base.Read (buffer, offset, count);
		}

		public override int ReadByte ()
		{
			return base.ReadByte ();
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			return base.Seek (offset, origin);
		}

		public override void SetLength (long value)
		{
			base.SetLength (value);
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			base.Write (buffer, offset, count);
		}

		public override void WriteByte (byte value)
		{
			base.WriteByte (value);
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}
	}
}
#endif
