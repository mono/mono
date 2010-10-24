/* -*- Mode: csharp; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
// 
// GZipStream.cs
//
// Authors:
//	Christopher James Lahey <clahey@ximian.com>
//	Gonzalo Paniagua Javier <gonzalo@novell.com>
//
// Copyright (C) 2004-2010 Novell, Inc (http://www.novell.com)
//
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

#if NET_2_0
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;

namespace System.IO.Compression {

	public class GZipStream : Stream
	{
		private DeflateStream deflateStream;

		public GZipStream (Stream compressedStream, CompressionMode mode) :
			this (compressedStream, mode, false) {
		}

		public GZipStream (Stream compressedStream, CompressionMode mode, bool leaveOpen) {
			this.deflateStream = new DeflateStream (compressedStream, mode, leaveOpen, true);
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				if (deflateStream != null) {
					deflateStream.Dispose ();
					deflateStream = null;
				}
			}
			base.Dispose (disposing);
		}

		public override int Read (byte[] dest, int dest_offset, int count)
		{
			if (deflateStream == null)
				throw new ObjectDisposedException (GetType ().FullName);

			return deflateStream.Read(dest, dest_offset, count);
		}


		public override void Write (byte[] src, int src_offset, int count)
		{
			if (deflateStream == null)
				throw new ObjectDisposedException (GetType ().FullName);

			deflateStream.Write (src, src_offset, count);
		}

		public override void Flush()
		{
			if (deflateStream == null)
				throw new ObjectDisposedException (GetType ().FullName);

			deflateStream.Flush();
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength (long value)
		{
			throw new NotSupportedException();
		}

		public override IAsyncResult BeginRead (byte [] buffer, int offset, int count,
							AsyncCallback cback, object state)
		{
			if (deflateStream == null)
				throw new ObjectDisposedException (GetType ().FullName);

			return deflateStream.BeginRead (buffer, offset, count, cback, state);
		}

		public override IAsyncResult BeginWrite (byte [] buffer, int offset, int count,
							AsyncCallback cback, object state)
		{
			if (deflateStream == null)
				throw new ObjectDisposedException (GetType ().FullName);

			return deflateStream.BeginWrite (buffer, offset, count, cback, state);
		}

		public override int EndRead(IAsyncResult async_result) {
			if (deflateStream == null)
				throw new ObjectDisposedException (GetType ().FullName);

			return deflateStream.EndRead (async_result);
		}

		public override void EndWrite (IAsyncResult async_result)
		{
			if (deflateStream == null)
				throw new ObjectDisposedException (GetType ().FullName);

			deflateStream.EndWrite (async_result);
		}

		public Stream BaseStream {
			get {
				if (deflateStream == null)
					return null;

				return deflateStream.BaseStream;
			}
		}

		public override bool CanRead {
			get {
				if (deflateStream == null)
					return false;

				return deflateStream.CanRead;
			}
		}

		public override bool CanSeek {
			get {
				if (deflateStream == null)
					return false;

				return deflateStream.CanSeek;
			}
		}

		public override bool CanWrite {
			get {
				if (deflateStream == null)
					return false;

				return deflateStream.CanWrite;
			}
		}

		public override long Length {
			get { throw new NotSupportedException(); }
		}

		public override long Position {
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
	}
}

#endif

