//
// System.Net.WebConnectionStream
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

using System.IO;
using System.Threading;

namespace System.Net
{
	class WebConnectionStream : Stream
	{
		bool isRead;
		WebConnection cnc;
		HttpWebRequest request;
		byte [] readBuffer;
		int readBufferOffset;
		int readBufferSize;
		int contentLength;
		int totalRead;
		bool nextReadCalled;
		int pendingReads;
		ManualResetEvent pending;
		bool allowBuffering;
		bool sendChunked;
		MemoryStream writeBuffer;
		bool requestWritten;
		byte [] headers;

		public WebConnectionStream (WebConnection cnc)
		{
			isRead = true;
			pending = new ManualResetEvent (true);
			this.cnc = cnc;
			try {
				contentLength = Int32.Parse (cnc.Data.Headers ["Content-Length"]);
			} catch {
				contentLength = Int32.MaxValue;
			}
		}

		public WebConnectionStream (WebConnection cnc, HttpWebRequest request)
		{
			isRead = false;
			this.cnc = cnc;
			this.request = request;
			allowBuffering = request.InternalAllowBuffering;
			sendChunked = request.SendChunked;
			if (allowBuffering)
				writeBuffer = new MemoryStream ();
		}

		internal byte [] ReadBuffer {
			set { readBuffer = value; }
		}

		internal int ReadBufferOffset {
			set { readBufferOffset = value;}
		}
		
		internal int ReadBufferSize {
			set { readBufferSize = value; }
		}
		
		internal void CheckComplete ()
		{
			if (readBufferSize - readBufferOffset == contentLength) {
				nextReadCalled = true;
				cnc.NextRead ();
			}
		}

		internal void ReadAll ()
		{
			if (!isRead || totalRead >= contentLength || nextReadCalled)
				return;

			pending.WaitOne ();
			lock (this) {
				if (totalRead >= contentLength)
					return;
				
				byte [] b = null;
				int diff = readBufferSize - readBufferOffset;
				int new_size;

				if (contentLength == Int32.MaxValue) {
					MemoryStream ms = new MemoryStream ();
					if (readBuffer != null && diff > 0)
						ms.Write (readBuffer, readBufferOffset, diff);

					byte [] buffer = new byte [2048];
					int read;
					while ((read = cnc.Read (buffer, 0, 2048)) != 0)
						ms.Write (buffer, 0, read);

					b = ms.GetBuffer ();
					new_size = (int) ms.Length;
				} else {
					new_size = contentLength - totalRead;
					b = new byte [new_size];
					if (readBuffer != null && diff > 0)
						Buffer.BlockCopy (readBuffer, readBufferOffset, b, 0, diff);
					
					int remaining = new_size - diff;
					int r = -1;
					while (remaining > 0 && r != 0) {
						r = cnc.Read (b, diff, remaining);
						remaining -= r;
						diff += r;
					}
				}

				readBuffer = b;
				readBufferOffset = 0;
				readBufferSize = new_size;
				contentLength = new_size;
				totalRead = 0;
				nextReadCalled = true;
			}

			cnc.NextRead ();
		}
		
		public override int Read (byte [] buffer, int offset, int size)
		{
			if (!isRead)
				throw new NotSupportedException ("this stream does not allow reading");

			if (totalRead >= contentLength)
				return 0;

			IAsyncResult res = BeginRead (buffer, offset, size, null, null);
			return EndRead (res);
		}

		public override IAsyncResult BeginRead (byte [] buffer, int offset, int size,
							AsyncCallback cb, object state)
		{
			if (!isRead)
				throw new NotSupportedException ("this stream does not allow reading");

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			int length = buffer.Length;
			if (size < 0 || offset < 0 || length < offset || length - offset < size)
				throw new ArgumentOutOfRangeException ();

			lock (this) {
				pendingReads++;
				pending.Reset ();
			}
			
			WebAsyncResult result = new WebAsyncResult (cb, state, buffer, offset, size);
			if (totalRead >= contentLength) {
				result.SetCompleted (true, 0);
				result.DoCallback ();
				return result;
			}
			
			int remaining = readBufferSize - readBufferOffset;
			if (remaining > 0) {
				int copy = (remaining > size) ? size : remaining;
				Buffer.BlockCopy (readBuffer, readBufferOffset, buffer, offset, copy);
				totalRead += copy;
				readBufferOffset += copy;
				offset += copy;
				size -= copy;
				if (size == 0 || totalRead >= contentLength) {
					result.SetCompleted (true, copy);
					result.DoCallback ();
					return result;
				}
				result.NBytes = copy;
			}

			result.InnerAsyncResult = cnc.BeginRead (buffer, offset, size, null, null);
			return result;
		}

		public override int EndRead (IAsyncResult r)
		{
			WebAsyncResult result = (WebAsyncResult) r;

			int nbytes = -1;
			if (result.IsCompleted) {
				nbytes = result.NBytes;
			} else {
				nbytes = cnc.EndRead (result.InnerAsyncResult);
				lock (this) {
					pendingReads--;
					if (pendingReads == 0)
						pending.Set ();
				}

				nbytes += result.NBytes; // partially filled from the read buffer
				result.SetCompleted (false, nbytes);
				totalRead += nbytes;
			}

			if (totalRead >= contentLength && !nextReadCalled) {
				nextReadCalled = true;
				cnc.NextRead ();
			}

			return nbytes;
		}
		
		public override IAsyncResult BeginWrite (byte [] buffer, int offset, int size,
							AsyncCallback cb, object state)
		{
			if (isRead)
				throw new NotSupportedException ("this stream does not allow writing");

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			int length = buffer.Length;
			if (size < 0 || offset < 0 || length < offset || length - offset < size)
				throw new ArgumentOutOfRangeException ();

			WebAsyncResult result = new WebAsyncResult (cb, state);
			if (allowBuffering) {
				writeBuffer.Write (buffer, offset, size);
				result.SetCompleted (true, 0);
				result.DoCallback ();
			} else {
				result.InnerAsyncResult = cnc.BeginWrite (buffer, offset, size, cb, state);
				if (result.InnerAsyncResult == null)
					throw new WebException ("Aborted");
			}

			return result;
		}

		public override void EndWrite (IAsyncResult r)
		{
			if (r == null)
				throw new ArgumentNullException ("r");

			if (allowBuffering)
				return;

			WebAsyncResult result = r as WebAsyncResult;
			if (result == null)
				throw new ArgumentException ("Invalid IAsyncResult");

			cnc.EndWrite (result.InnerAsyncResult);
			return;
		}
		
		public override void Write (byte [] buffer, int offset, int size)
		{
			if (isRead)
				throw new NotSupportedException ("this stream does not allow writing");

			IAsyncResult res = BeginWrite (buffer, offset, size, null, null);
			EndWrite (res);
		}

		public override void Flush ()
		{
		}

		internal void SetHeaders (byte [] buffer, int offset, int size)
		{
			if (!allowBuffering) {
				Write (buffer, offset, size);
			} else {
				headers = new byte [size];
				Buffer.BlockCopy (buffer, offset, headers, 0, size);
			}
		}

		internal void WriteRequest ()
		{
			if (!allowBuffering || writeBuffer == null || requestWritten)
				return;

			byte [] bytes = writeBuffer.GetBuffer ();
			int length = (int) writeBuffer.Length;
			if (request.ContentLength != -1 && request.ContentLength < length) {
				throw new ProtocolViolationException ("Specified Content-Length is less than the " +
								      "number of bytes to write");
			}

			request.InternalContentLength = length;
			request.SendRequestHeaders ();
			cnc.WaitForContinue (headers, 0, headers.Length);
			if (cnc.Data.StatusCode != 0 && cnc.Data.StatusCode != 100)
				return;

			cnc.Write (bytes, 0, length);
			requestWritten = true;
			cnc.dataAvailable.Set ();
		}

		public override void Close ()
		{
			if (!allowBuffering)
				return;

			// may be ReadAll is isRead?
			long length = request.ContentLength;
			if (length != -1 && length > writeBuffer.Length)
				throw new IOException ("Cannot close the stream until all bytes are written");

			WriteRequest ();
		}

		internal void ResetWriteBuffer ()
		{
			if (!allowBuffering)
				return;

			writeBuffer = new MemoryStream ();
			requestWritten = false;
		}
		
		public override long Seek (long a, SeekOrigin b)
		{
			throw new NotSupportedException ();
		}
		
		public override void SetLength (long a)
		{
			throw new NotSupportedException ();
		}
		
		public override bool CanSeek {
			get { return false; }
		}

		public override bool CanRead {
			get { return isRead && (contentLength == Int32.MaxValue || totalRead < contentLength); }
		}

		public override bool CanWrite {
			get { return !isRead; }
		}

		public override long Length {
			get { throw new NotSupportedException (); }
		}

		public override long Position {
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}
	}
}

