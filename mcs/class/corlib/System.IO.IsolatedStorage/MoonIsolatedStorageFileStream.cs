//
// System.IO.IsolatedStorage.MoonIsolatedStorageFileStream
//
// Moonlight's implementation for the IsolatedStorageFileStream
// 
// Authors
//      Miguel de Icaza (miguel@novell.com)
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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
#if NET_2_1
using System;
using System.IO;

namespace System.IO.IsolatedStorage {

	public class IsolatedStorageFileStream : FileStream {
		IsolatedStorageFile container;
			
		public IsolatedStorageFileStream (string path, FileMode mode, IsolatedStorageFile isf)
			: base (IsolatedStorageFile.Verify (path), mode)
		{
			container = isf;
		}

		public IsolatedStorageFileStream (string path, FileMode mode, FileAccess access, IsolatedStorageFile isf)
			: base (IsolatedStorageFile.Verify (path), mode, access)
		{
			container = isf;
		}

		public IsolatedStorageFileStream (string path, FileMode mode, FileAccess access, FileShare share, IsolatedStorageFile isf)
			: base (IsolatedStorageFile.Verify (path), mode, access, share)
		{
			container = isf;
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}

		public override void Flush ()
		{
			base.Flush ();
		}

		public override int Read (byte [] buffer, int offset, int count)
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

		public override void Write (byte [] buffer, int offset, int count)
		{
			base.Write (buffer, offset, count);
		}

		public override void WriteByte (byte value)
		{
			base.WriteByte (value);
		}

		public override bool CanRead {
			get {
				return base.CanRead;
			}
		}

		public override bool CanSeek {
			get {
				return base.CanSeek;
			}
		}

		public override bool CanWrite {
			get {
				return base.CanWrite;
			}
		}

		public override bool IsAsync {
			get {
				return base.IsAsync;
			}
		}

		public override long Length {
			get {
				return base.Length;
			}
		}

		public override long Position {
			get {
				return base.Position;
			}

			set {
				base.Position = value;
			}
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
	}
}
#endif
