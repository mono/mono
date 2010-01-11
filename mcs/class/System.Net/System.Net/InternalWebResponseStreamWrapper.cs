/*
 * InternalWebResponseStreamWrapper.cs.
 *
 * Contact:
 *   Moonlight List (moonlight-list@lists.ximian.com)
 *
 * Copyright 2008,2010 Novell, Inc. (http://www.novell.com)
 *
 * See the LICENSE file included with the distribution for details.
 * 
 */

using System.IO;

namespace System.Net {

	// simply a read-only wrapper around a stream + a no-op Close
	internal sealed class InternalWebResponseStreamWrapper : Stream {

		private Stream stream;
		
		internal InternalWebResponseStreamWrapper (Stream s)
		{
			stream = s;
		}

		public override bool CanRead {
			get {
				 return stream.CanRead;
			}
		}

		public override bool CanSeek {
			get {
				 return stream.CanSeek;
			}
		}

		public override bool CanWrite {
			get {
				 return false;
			}
		}

		public override long Length {
			get {
				 return stream.Length;
			}
		}

		public override long Position {
			get {
				 return stream.Position;
			}
			set {
				stream.Position = value;
			}
		}

		public override void Flush ()
		{
			throw new NotSupportedException ();
		}

		public override void Close ()
		{
			// We cannot call "stream.Close" on a memory stream since it deletes the data
		}

		public override void SetLength (long value)
		{
			throw new NotSupportedException ();
		}

		public override int Read (byte [] buffer, int offset, int count)
		{
			return stream.Read (buffer, offset, count);
		}

		public override void Write (byte [] buffer, int offset, int count)
		{
			throw new NotSupportedException ();
		}

		public override void WriteByte (byte value)
		{
			throw new NotSupportedException ();
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			return stream.Seek (offset, origin);
		}

		internal Stream InnerStream {
			get { return stream; }
		}
	}
}

