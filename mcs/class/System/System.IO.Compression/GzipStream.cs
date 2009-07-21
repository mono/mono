/* -*- Mode: csharp; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
// 
// GZipStream.cs
//
// Authors:
//	Christopher James Lahey <clahey@ximian.com>
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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
			if (disposing)
				deflateStream.Dispose ();
			base.Dispose (disposing);
		}

		public override int Read (byte[] dest, int dest_offset, int count)
		{
			return deflateStream.Read(dest, dest_offset, count);
		}


		public override void Write (byte[] src, int src_offset, int count)
		{
			deflateStream.Write (src, src_offset, count);
		}

		public override void Flush() {
			deflateStream.Flush();
		}

		public override long Seek (long offset, SeekOrigin origin) {
			return deflateStream.Seek (offset, origin);
		}

		public override void SetLength (long value) {
			deflateStream.SetLength (value);
		}

		public override IAsyncResult BeginRead (byte [] buffer, int offset, int count,
							AsyncCallback cback, object state)
		{
			return deflateStream.BeginRead (buffer, offset, count, cback, state);
		}

		public override IAsyncResult BeginWrite (byte [] buffer, int offset, int count,
							AsyncCallback cback, object state)
		{
			return deflateStream.BeginWrite (buffer, offset, count, cback, state);
		}

		public override int EndRead(IAsyncResult async_result) {
			return deflateStream.EndRead (async_result);
		}

		public override void EndWrite (IAsyncResult async_result)
		{
			deflateStream.EndWrite (async_result);
		}

		public Stream BaseStream {
			get {
				return deflateStream.BaseStream;
			}
		}
		public override bool CanRead {
			get {
				return deflateStream.CanRead;
			}
		}
		public override bool CanSeek {
			get {
				return deflateStream.CanSeek;
			}
		}
		public override bool CanWrite {
			get {
				return deflateStream.CanWrite;
			}
		}
		public override long Length {
			get {
				return deflateStream.Length;
			}
		}
		public override long Position {
			get {
				return deflateStream.Position;
			}
			set {
				deflateStream.Position = value;
			}
		}
	}
}

#endif
