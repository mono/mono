//
// System.Web.HttpResponseStream.cs 
//
// 
// Author:
//	Miguel de Icaza (miguel@novell.com)
//	Ben Maurer (bmaurer@ximian.com)
//
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.IO;
using System.Text;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Web
{

	//
	// HttpResponseStream implements the "OutputStream" from HttpResponse
	//
	// The MS implementation is broken in that it does not hook up this
	// to HttpResponse, so calling "Flush" on Response.OutputStream does not
	// flush the contents and produce the headers.
	//
	// You must call HttpResponse.Flush which does the actual header generation
	// and actual data flushing
	//
	internal class HttpResponseStream : Stream
	{
		Bucket first_bucket;
		Bucket cur_bucket;
		HttpResponse response;
		bool dirty = false;

		Stream filter;

		public HttpResponseStream (HttpResponse response)
		{
			this.response = response;
		}

		internal bool HaveFilter
		{
			get { return filter != null; }
		}

		public Stream Filter
		{
			get
			{
				if (filter == null)
					filter = new OutputFilterStream (this);
				return filter;
			}
			set
			{
				filter = value;
			}
		}
		abstract class Bucket
		{
			public Bucket Next;

			public virtual void Dispose ()
			{
			}

			public abstract void Send (HttpWorkerRequest wr);
			public abstract void Send (Stream stream);
			public abstract int Length { get; }
			public abstract int FreeSpace { get; }
		}

		class ByteBucket : Bucket
		{

			const int _preferredLength = 16 * 1024;

			int _position = 0;
			int _freeSpace = _preferredLength;
			byte [] buffer = new byte [_preferredLength];

			public ByteBucket ()
			{
			}

			public override int Length
			{
				get { return _position; }
			}

			public override int FreeSpace
			{
				get { return _freeSpace; }
			}

			public int Write (byte [] buf, int offset, int count)
			{
				if (count > _freeSpace)
					throw new InvalidOperationException ("Out of bucket space");

				Array.Copy (buf, offset, buffer, _position, count);
				_position += count;
				_freeSpace = _preferredLength - _position;
				return count;
			}

			public override void Dispose ()
			{
				buffer = null;
			}

			public override void Send (HttpWorkerRequest wr)
			{
				if (_position == 0)
					return;

				wr.SendResponseFromMemory (buffer, _position);
			}

			public override void Send (Stream stream)
			{
				if (_position == 0)
					return;

				stream.Write (buffer, 0, _position);
			}
		}

		class CharBucket : Bucket
		{
			const int _preferredLength = 8 * 1024;

			int _position = 0;
			int _freeSpace = _preferredLength;
			char [] buffer = new char [_preferredLength];
			readonly Encoding _encoding;

			public CharBucket (Encoding encoding)
			{
				_encoding = encoding;
			}

			public override int Length
			{
				get
				{
					HttpContext current = HttpContext.Current;
					Encoding enc = (current != null) ? current.Response.ContentEncoding : Encoding.UTF8;
					return enc.GetByteCount (buffer, 0, _position);
				}
			}

			public override int FreeSpace
			{
				get { return _freeSpace; }
			}

			public int Write (string buf, int offset, int count)
			{
				if (count > _freeSpace)
					throw new InvalidOperationException ("Out of bucket space");

				buf.CopyTo (offset, buffer, _position, count);
				_position += count;

				_freeSpace = _preferredLength - _position;
				return count;
			}

			public int Write (char [] buf, int offset, int count)
			{
				if (count > _freeSpace)
					throw new InvalidOperationException ("Out of bucket space");

				if (count == 1)
					buffer [_position] = buf [offset];
				else
					Array.Copy (buf, offset, buffer, _position, count);

				_position += count;
				_freeSpace = _preferredLength - _position;
				return count;
			}

			public override void Dispose ()
			{
				buffer = null;
			}

			public override void Send (HttpWorkerRequest wr)
			{
				if (_position == 0)
					return;

				wr.SendResponseFromMemory (buffer, 0, _position, _encoding);
			}

			public override void Send (Stream stream)
			{
				if (_position == 0)
					return;

				byte[] bytesToWrite =_encoding.GetBytes (buffer, 0, _position);
				stream.Write (bytesToWrite, 0, bytesToWrite.Length);
			}
		}

		class BufferedFileBucket : Bucket
		{
			string file;
			long offset;
			long length;

			public BufferedFileBucket (string f, long off, long len)
			{
				file = f;
				offset = off;
				length = len;
			}

			public override int Length
			{
				get { return (int) length; }
			}

			public override int FreeSpace
			{
				get { return int.MaxValue; }
			}

			public override void Send (HttpWorkerRequest wr)
			{
				wr.SendResponseFromFile (file, offset, length);
			}

			public override void Send (Stream stream)
			{
				using (FileStream fs = File.OpenRead (file)) {
					byte [] buffer = new byte [Math.Min (fs.Length, 32 * 1024)];

					long remain = fs.Length;
					int n;
					while (remain > 0 && (n = fs.Read (buffer, 0, (int) Math.Min (remain, 32 * 1024))) != 0) {
						remain -= n;
						stream.Write (buffer, 0, n);
					}
				}
			}

			public override string ToString ()
			{
				return String.Format ("file {0} {1} bytes from position {2}", file, length, offset);
			}
		}

		void AppendBucket (Bucket b)
		{
			if (first_bucket == null) {
				cur_bucket = first_bucket = b;
				return;
			}

			cur_bucket.Next = b;
			cur_bucket = b;
		}

		//
		// Nothing happens here, broken by requirement.
		// See note at the start
		//
		public override void Flush ()
		{
		}

		internal void Flush (HttpWorkerRequest wr, bool final_flush)
		{
			if (!dirty && !final_flush)
				return;

			for (Bucket b = first_bucket; b != null; b = b.Next) {
				b.Send (wr);
			}

			wr.FlushResponse (final_flush);
			Clear ();
		}

		internal int GetTotalLength ()
		{
			int size = 0;
			for (Bucket b = first_bucket; b != null; b = b.Next)
				size += b.Length;

			return size;
		}

		internal MemoryStream GetData ()
		{
			MemoryStream stream = new MemoryStream ();
			for (Bucket b = first_bucket; b != null; b = b.Next)
				b.Send (stream);
			return stream;
		}

		public void WriteFile (string f, long offset, long length)
		{
			if (length == 0)
				return;

			dirty = true;

			AppendBucket (new BufferedFileBucket (f, offset, length));
			// Flush () is called from HttpResponse if needed (WriteFile/TransmitFile)
		}

		bool filtering;
		internal void ApplyFilter (bool close)
		{
			if (filter == null)
				return;

			filtering = true;
			Bucket one = first_bucket;
			first_bucket = null; // This will recreate new buckets for the filtered content
			cur_bucket = null;
			dirty = false;
			for (Bucket b = one; b != null; b = b.Next)
				b.Send (filter);

			for (Bucket b = one; b != null; b = b.Next)
				b.Dispose ();

			if (close) {
				filter.Flush ();
				filter.Close ();
				filter = null;
			}
			else {
				filter.Flush ();
			}
			filtering = false;
		}

		public void Write (char [] buffer, int offset, int count)
		{
			bool buffering = response.BufferOutput;

			if (buffering) {
				// It does not matter whether we're in ApplyFilter or not
				AppendBuffer (buffer, offset, count);
			}
			else if (filter == null || filtering) {
				response.WriteHeaders (false);
				HttpWorkerRequest wr = response.WorkerRequest;
				// Direct write because not buffering
				wr.SendResponseFromMemory (buffer, offset, count, response.ContentEncoding);
				wr.FlushResponse (false);
			}
			else {
				// Write to the filter, which will call us back, and then Flush
				filtering = true;
				try {
					byte [] bytesToWrite = response.ContentEncoding.GetBytes (buffer, offset, count);
					filter.Write (bytesToWrite, 0, bytesToWrite.Length);
				}
				finally {
					filtering = false;
				}
				Flush (response.WorkerRequest, false);
			}
		}

		public void Write (string s, int offset, int count)
		{
			bool buffering = response.BufferOutput;

			if (buffering) {
				// It does not matter whether we're in ApplyFilter or not
				AppendBuffer (s, offset, count);
			}
			else if (filter == null || filtering) {
				response.WriteHeaders (false);
				HttpWorkerRequest wr = response.WorkerRequest;
				// Direct write because not buffering
				wr.SendResponseFromMemory (s, offset, count, response.ContentEncoding);
				wr.FlushResponse (false);
			}
			else {
				// Write to the filter, which will call us back, and then Flush
				filtering = true;
				try {
					byte [] bytesToWrite = response.ContentEncoding.GetBytes (s.ToCharArray (), offset, count);
					filter.Write (bytesToWrite, 0, bytesToWrite.Length);
				}
				finally {
					filtering = false;
				}
				Flush (response.WorkerRequest, false);
			}
		}

		public override void Write (byte [] buffer, int offset, int count)
		{
			bool buffering = response.BufferOutput;

			if (buffering) {
				// It does not matter whether we're in ApplyFilter or not
				AppendBuffer (buffer, offset, count);
			}
			else if (filter == null || filtering) {
				response.WriteHeaders (false);
				HttpWorkerRequest wr = response.WorkerRequest;
				// Direct write because not buffering
				if (offset == 0) {
					wr.SendResponseFromMemory (buffer, count);
				}
				else {
					UnsafeWrite (wr, buffer, offset, count);
				}
				wr.FlushResponse (false);
			}
			else {
				// Write to the filter, which will call us back, and then Flush
				filtering = true;
				try {
					filter.Write (buffer, offset, count);
				}
				finally {
					filtering = false;
				}
				Flush (response.WorkerRequest, false);
			}
		}

#if TARGET_JVM
		void UnsafeWrite (HttpWorkerRequest wr, byte [] buffer, int offset, int count)
		{
			if (count <= 0)
				return;

			byte [] copy = new byte [count];
			Array.Copy (buffer, offset, copy, 0, count);
			wr.SendResponseFromMemory (copy, count);
		}
#else
		unsafe void UnsafeWrite (HttpWorkerRequest wr, byte [] buffer, int offset, int count)
		{
			fixed (byte *ptr = buffer) {
				wr.SendResponseFromMemory ((IntPtr) (ptr + offset), count);
			}
		}
#endif
		void AppendBuffer (byte [] buffer, int offset, int count)
		{
			if (!(cur_bucket is ByteBucket))
				AppendBucket (new ByteBucket ());

			dirty = true;

			while (count > 0) {
				if (cur_bucket.FreeSpace == 0)
					AppendBucket (new ByteBucket ());

				int len = count;
				int freeSpace = cur_bucket.FreeSpace;

				if (len > freeSpace)
					len = freeSpace;

				((ByteBucket) cur_bucket).Write (buffer, offset, len);
				offset += len;
				count -= len;
			}

		}

		void AppendBuffer (char [] buffer, int offset, int count)
		{
			if (!(cur_bucket is CharBucket))
				AppendBucket (new CharBucket (response.ContentEncoding));

			dirty = true;

			while (count > 0) {
				if (cur_bucket.FreeSpace == 0)
					AppendBucket (new CharBucket (response.ContentEncoding));

				int len = count;
				int freeSpace = cur_bucket.FreeSpace;

				if (len > freeSpace)
					len = freeSpace;

				((CharBucket) cur_bucket).Write (buffer, offset, len);
				offset += len;
				count -= len;
			}
		}

		void AppendBuffer (string buffer, int offset, int count)
		{
			if (!(cur_bucket is CharBucket))
				AppendBucket (new CharBucket (response.ContentEncoding));

			dirty = true;

			while (count > 0) {
				if (cur_bucket.FreeSpace == 0)
					AppendBucket (new CharBucket (response.ContentEncoding));

				int len = count;
				int freeSpace = cur_bucket.FreeSpace;

				if (len > freeSpace)
					len = freeSpace;

				((CharBucket) cur_bucket).Write (buffer, offset, len);
				offset += len;
				count -= len;
			}
		}

		//
		// This should not flush/close or anything else, its called
		// just to free any memory we might have allocated (when we later
		// implement something with unmanaged memory).
		//
		internal void ReleaseResources (bool close_filter)
		{
			if (close_filter && filter != null) {
				filter.Close ();
				filter = null;
			}

			for (Bucket b = first_bucket; b != null; b = b.Next)
				b.Dispose ();

			first_bucket = null;
			cur_bucket = null;
		}

		public void Clear ()
		{
			//
			// IMPORTANT: you must dispose *AFTER* using all the buckets Byte chunks might be
			// split across two buckets if there is a file between the data.
			//
			ReleaseResources (false);
			dirty = false;
		}

		public override bool CanRead
		{
			get
			{
				return false;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}
		public override bool CanWrite
		{
			get
			{
				return true;
			}
		}

		const string notsupported = "HttpResponseStream is a forward, write-only stream";

		public override long Length
		{
			get
			{
				throw new InvalidOperationException (notsupported);
			}
		}

		public override long Position
		{
			get
			{
				throw new InvalidOperationException (notsupported);
			}
			set
			{
				throw new InvalidOperationException (notsupported);
			}
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			throw new InvalidOperationException (notsupported);
		}

		public override void SetLength (long value)
		{
			throw new InvalidOperationException (notsupported);
		}

		public override int Read (byte [] buffer, int offset, int count)
		{
			throw new InvalidOperationException (notsupported);
		}
	}
}

