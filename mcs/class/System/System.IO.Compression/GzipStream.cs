/* -*- Mode: csharp; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
// 
// GzipStream.cs
//
// Authors:
//	Christopher James Lahey <clahey@ximian.com>
//
// (c) 2004 Novell, Inc. <http://www.novell.com>
//

#if NET_2_0
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;

namespace System.IO.Compression {
	public class GzipStream : Stream
	{
		private DeflateStream deflateStream;

		public GzipStream (Stream compressedStream, CompressionMode mode) :
			this (compressedStream, mode, false) {
		}

		public GzipStream (Stream compressedStream, CompressionMode mode, bool leaveOpen) {
			this.deflateStream = new DeflateStream (compressedStream, mode, leaveOpen, true);
		}

		public override void Close () {
			deflateStream.Close();
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
