/*
 * InternalWebRequestStreamWrapper.cs.
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

	internal sealed class InternalWebRequestStreamWrapper : Stream {

		MemoryStream stream;
		byte[] data;
		
		internal InternalWebRequestStreamWrapper (MemoryStream s)
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
				 return stream.CanWrite;
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
			stream.Flush ();
		}

		public override void Close ()
		{
			try {
				// When we POST data then the actual bytes are needed after the stream is closed (e.g. DRT287)
				// However a MemoryStream will clear itself and throw ObjectDisposedException in such case
				data = stream.ToArray ();
				stream.Close ();
			}
			finally {
				// if used from WebClient then notify that the stream was closed
				if (WebClient != null) {
					WebClient.WriteStreamClosedCallback (WebClientData, data.Length);
					WebClient = null; // notify only once
				}
			}
		}

		public override void SetLength (long value)
		{
			stream.SetLength (value);
		}

		public override int Read (byte [] buffer, int offset, int count)
		{
			return stream.Read (buffer, offset, count);
		}

		public override void Write (byte [] buffer, int offset, int count)
		{
			stream.Write (buffer, offset, count);
		}

		public override void WriteByte (byte value)
		{
			stream.WriteByte (value);
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			return stream.Seek (offset, origin);
		}

		internal byte[] GetData ()
		{
			// hold a copy of the data if the stream was closed
			return data;
		}

		internal MemoryStream InnerStream {
			get { return stream; }
		}

		internal WebClient WebClient {
			get; set;
		}

		internal object WebClientData {
			get; set;
		}
	}
}

