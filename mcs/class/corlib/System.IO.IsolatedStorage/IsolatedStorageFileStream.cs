// System.IO.IsolatedStorage.IsolatedStorageFileStream
//
// Sean MacIsaac (macisaac@ximian.com)
//
// (C) 2001 Ximian, Inc.
// (C) 2004 Novell (http://www.novell.com)

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Globalization;
using System.IO;
using System.Reflection;

namespace System.IO.IsolatedStorage {

	public class IsolatedStorageFileStream : FileStream
	{
		private static string CreateIsolatedPath (string path)
		{
			string dir = IsolatedStorageInfo.CreateAssemblyFilename (Assembly.GetEntryAssembly());

			string file = Path.Combine (dir, path);

			// Ensure that the file can be created.
			FileInfo fi = new FileInfo (file);
			if (!fi.Directory.Exists)
				fi.Directory.Create ();

			return file;
		}

		public IsolatedStorageFileStream (string path, FileMode mode)
			: base(CreateIsolatedPath (path), mode)
		{
		}	

		public IsolatedStorageFileStream (string path, FileMode mode, FileAccess access)
			: base (CreateIsolatedPath (path), mode, access)
		{
		}

		public IsolatedStorageFileStream (string path, FileMode mode, FileAccess access, FileShare share)
			: base (CreateIsolatedPath (path), mode, access, share)
		{
		}

		public IsolatedStorageFileStream (string path, FileMode mode, FileAccess access, FileShare share, int bufferSize)
			: base (CreateIsolatedPath (path), mode, access, share, bufferSize)
		{
		}

		public IsolatedStorageFileStream (string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, IsolatedStorageFile isf)
			: base (CreateIsolatedPath (path), mode, access, share, bufferSize)
		{
		}

		public IsolatedStorageFileStream (string path, FileMode mode, FileAccess access, FileShare share, IsolatedStorageFile isf)
			: base (CreateIsolatedPath (path), mode, access, share)
		{
		}

		public IsolatedStorageFileStream (string path, FileMode mode, FileAccess access, IsolatedStorageFile isf)
			: base (CreateIsolatedPath (path), mode, access)
		{
		}

		public IsolatedStorageFileStream (string path, FileMode mode, IsolatedStorageFile isf)
			: base (CreateIsolatedPath (path), mode)
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

		public override IntPtr Handle {
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

		public override void Close ()
		{
			base.Close ();
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

