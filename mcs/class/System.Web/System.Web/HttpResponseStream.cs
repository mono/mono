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
	
namespace System.Web {

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
	internal class HttpResponseStream : Stream {
		Bucket first_bucket;
		Bucket cur_bucket;
		HttpResponse response;
		internal long total;
		Stream filter;
		byte [] chunk_buffer = new byte [24];

		public HttpResponseStream (HttpResponse response)
		{
			this.response = response;
		}

		internal bool HaveFilter {
			get { return filter != null; }
		}

		public Stream Filter {
			get {
				if (filter == null)
					filter = new OutputFilterStream (this);
				return filter;
			}
			set {
				filter = value;
			}
		}
#if TARGET_JVM

		class BlockManager {
			const int PreferredLength = 16 * 1024;
			static readonly byte[] EmptyBuffer = new byte[0];

			byte[] buffer = EmptyBuffer;
			int position;

			public BlockManager () {
			}

			public int Position {
				get { return position; }
			}

			void EnsureCapacity (int capacity) {
				if (buffer.Length >= capacity)
					return;

				capacity += PreferredLength;
				capacity = (capacity / PreferredLength) * PreferredLength;
				byte[] temp = new byte[capacity];
				Array.Copy(buffer, 0, temp, 0, buffer.Length);
				buffer = temp;
			}

			public void Write (byte [] buffer, int offset, int count) {
				if (count == 0)
					return;

				EnsureCapacity (position + count);
				Array.Copy(buffer, offset, this.buffer, position, count);
				position += count;
			}

			public void Send (HttpWorkerRequest wr, int start, int end) {
				int length = end - start;
				if (length <= 0)
					return;

				if (length > buffer.Length - start)
					length = buffer.Length - start;

				if (start > 0) {
					byte[] temp = new byte[length];
					Array.Copy(buffer, start, temp, 0, length);
					buffer = temp;
				}
				wr.SendResponseFromMemory(buffer, length);
			}

			public void Send (Stream stream, int start, int end) {
				int length = end - start;
				if (length <= 0)
					return;

				if (length > buffer.Length - start)
					length = buffer.Length - start;

				stream.Write(buffer, start, length);
			}

			public void Dispose () {
				buffer = null;
			}
		}

#else // TARGET_JVM
		unsafe sealed class BlockManager {
			const int PreferredLength = 128 * 1024;
			byte *data;
			int position;
			int block_size;

			public BlockManager ()
			{
			}

			public int Position {
				get { return position; }
			}

			void EnsureCapacity (int capacity)
			{
				if (block_size >= capacity)
					return;

				capacity += PreferredLength;
				capacity = (capacity / PreferredLength) * PreferredLength;

				data = data == null
					? (byte *) Marshal.AllocHGlobal (capacity)
					: (byte *) Marshal.ReAllocHGlobal ((IntPtr) data, (IntPtr) capacity);
				block_size = capacity;
			}

			public void Write (byte [] buffer, int offset, int count)
			{
				if (count == 0)
					return;
				
				EnsureCapacity (position + count);
				Marshal.Copy (buffer, offset, (IntPtr) (data + position), count);
				position += count;
			}

			public void Write (IntPtr ptr, int count)
			{
				if (count == 0)
					return;
				
				EnsureCapacity (position + count);
				byte *src = (byte *) ptr.ToPointer ();
				if (count < 32) {
					byte *dest = (data + position);
					for (int i = 0; i < count; i++)
						*dest++ = *src++;
				} else {
					memcpy (data + position, src, count);
				}
				position += count;
			}

			public void Send (HttpWorkerRequest wr, int start, int end)
			{
				if (end - start <= 0)
					return;

				wr.SendResponseFromMemory ((IntPtr) (data + start), end - start);
			}

			public void Send (Stream stream, int start, int end)
			{
				int len = end - start;
				if (len <= 0)
					return;

				byte [] buffer = new byte [Math.Min (len, 32 * 1024)];
				int size = buffer.Length;
				while (len > 0) {
					Marshal.Copy ((IntPtr) (data + start), buffer, 0, size);
					stream.Write (buffer, 0, size);
					start += size;
					len -= size;
					if (len > 0 && len < size)
						size = len;
				}
			}
			
			public void Dispose ()
			{
				if ((IntPtr) data != IntPtr.Zero) {
					Marshal.FreeHGlobal ((IntPtr) data);
					data = (byte *) IntPtr.Zero;
				}
			}
		}

#endif
		abstract class Bucket {
			public Bucket Next;

			public virtual void Dispose ()
			{
			}

			public abstract void Send (HttpWorkerRequest wr);
			public abstract void Send (Stream stream);
			public abstract int Length { get; }
		}

#if !TARGET_JVM
		unsafe
#endif
		class ByteBucket : Bucket {
			int start;
			int length;
			public BlockManager blocks;
			public bool Expandable = true;

			public ByteBucket () : this (null)
			{
			}

			public ByteBucket (BlockManager blocks)
			{
				if (blocks == null)
					blocks = new BlockManager ();

				this.blocks = blocks;
				start = blocks.Position;
			}

			public override int Length {
				get { return length; }
			}

			public unsafe int Write (byte [] buf, int offset, int count)
			{
				if (Expandable == false)
					throw new Exception ("This should not happen.");

				fixed (byte *p = &buf[0]) {
					IntPtr p2 = new IntPtr (p + offset);
					blocks.Write (p2, count);
				}

				length += count;
				return count;
			}

			public int Write (IntPtr ptr, int count)
			{
				if (Expandable == false)
					throw new Exception ("This should not happen.");

				blocks.Write (ptr, count);
				length += count;
				return count;
			}

			public override void Dispose ()
			{
				blocks.Dispose ();
			}

			public override void Send (HttpWorkerRequest wr)
			{
				if (length == 0)
					return;

				blocks.Send (wr, start, length);
			}

			public override void Send (Stream stream)
			{
				if (length == 0)
					return;

				blocks.Send (stream, start, length);
			}
		}

		class BufferedFileBucket : Bucket {
			string file;
			long offset;
			long length;
	
			public BufferedFileBucket (string f, long off, long len)
			{
				file = f;
				offset = off;
				length = len;
			}

			public override int Length {
				get { return (int) length; }
			}

			public override void Send (HttpWorkerRequest wr)
			{
				wr.SendResponseFromFile (file, offset, length);
			}

			public override void Send (Stream stream)
			{
				using (FileStream fs = File.OpenRead (file)) {
					byte [] buffer = new byte [Math.Min (fs.Length, 32*1024)];

					long remain = fs.Length;
					int n;
					while (remain > 0 && (n = fs.Read (buffer, 0, (int) Math.Min (remain, 32*1024))) != 0){
						remain -= n;
						stream.Write (buffer, 0, n);
					}
				}
			}

			public override string ToString ()
			{
				return "file " + file + " " + length.ToString () + " bytes from position " + offset.ToString ();
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

		void SendChunkSize (long l, bool last)
		{
			if (l == 0 && !last)
				return;

			int i = 0;
			if (l >= 0) {
				string s = l.ToString ("x");
				for (; i < s.Length; i++)
					chunk_buffer [i] = (byte) s [i];
			}

			chunk_buffer [i++] = 13;
			chunk_buffer [i++] = 10;
			if (last) {
				chunk_buffer [i++] = 13;
				chunk_buffer [i++] = 10;
			}

			response.WorkerRequest.SendResponseFromMemory (chunk_buffer, i);
		}

		internal void Flush (HttpWorkerRequest wr, bool final_flush)
		{
			if (total == 0 && !final_flush)
				return;

			if (response.use_chunked) 
				SendChunkSize (total, false);

			for (Bucket b = first_bucket; b != null; b = b.Next) {
				b.Send (wr);
			}

			if (response.use_chunked) {
				SendChunkSize (-1, false);
				if (final_flush)
					SendChunkSize (0, true);
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

			ByteBucket bb = cur_bucket as ByteBucket;

			if (bb != null) {
				bb.Expandable = false;
				bb = new ByteBucket (bb.blocks);
			}

			total += length;
			
			AppendBucket (new BufferedFileBucket (f, offset, length));
			if (bb != null)
				AppendBucket (bb);
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
			total = 0;
			for (Bucket b = one; b != null; b = b.Next)
				b.Send (filter);

			for (Bucket b = one; b != null; b = b.Next)
				b.Dispose ();

			if (close) {
				filter.Flush ();
				filter.Close ();
				filter = null;
			} else {
				filter.Flush ();
			}
			filtering = false;
		}

		public void WritePtr (IntPtr ptr, int length)
		{
			if (length == 0)
				return;

			bool buffering = response.BufferOutput;

			if (buffering) {
				// It does not matter whether we're in ApplyFilter or not
				AppendBuffer (ptr, length);
			} else if (filter == null || filtering) {
				response.WriteHeaders (false);
				HttpWorkerRequest wr = response.WorkerRequest;
				// Direct write because not buffering
				wr.SendResponseFromMemory (ptr, length);
				wr.FlushResponse (false);
			} else {
				// Write to the filter, which will call us back, and then Flush
				filtering = true;
				try {
					byte [] bytes = new byte [length];
					Marshal.Copy (ptr, bytes, 0, length);
					filter.Write (bytes, 0, length);
					bytes = null;
				} finally {
					filtering = false;
				}
				Flush (response.WorkerRequest, false);
			}

		}

		public override void Write (byte [] buffer, int offset, int count)
		{
			bool buffering = response.BufferOutput;

#if NET_2_0
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
#endif

			int max_count = buffer.Length - offset;
#if NET_2_0
			if (offset < 0 || max_count <= 0)
#else
			if (offset < 0)
#endif
				throw new ArgumentOutOfRangeException ("offset");
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count");
			if (count > max_count)
				count = max_count;
#if ONLY_1_1
			if (max_count <= 0)
				return;
#endif

			if (buffering) {
				// It does not matter whether we're in ApplyFilter or not
				AppendBuffer (buffer, offset, count);
			} else if (filter == null || filtering) {
				response.WriteHeaders (false);
				HttpWorkerRequest wr = response.WorkerRequest;
				// Direct write because not buffering
				if (offset == 0) {
					wr.SendResponseFromMemory (buffer, count);
				} else {
					UnsafeWrite (wr, buffer, offset, count);
				}
				wr.FlushResponse (false);
			} else {
				// Write to the filter, which will call us back, and then Flush
				filtering = true;
				try {
					filter.Write (buffer, offset, count);
				} finally {
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

			byte[] copy = new byte[count];
			Array.Copy(buffer, offset, copy, 0, count);
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

			total += count;
			((ByteBucket) cur_bucket).Write (buffer, offset, count);
		}

		void AppendBuffer (IntPtr ptr, int count)
		{
			if (!(cur_bucket is ByteBucket))
				AppendBucket (new ByteBucket ());

			total += count;
			((ByteBucket) cur_bucket).Write (ptr, count);
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
			total = 0;
		}
		
		// Do not use directly. Use memcpy.
		static unsafe void memcpy4 (byte *dest, byte *src, int size) {
			/*while (size >= 32) {
				// using long is better than int and slower than double
				// FIXME: enable this only on correct alignment or on platforms
				// that can tolerate unaligned reads/writes of doubles
				((double*)dest) [0] = ((double*)src) [0];
				((double*)dest) [1] = ((double*)src) [1];
				((double*)dest) [2] = ((double*)src) [2];
				((double*)dest) [3] = ((double*)src) [3];
				dest += 32;
				src += 32;
				size -= 32;
			}*/
			while (size >= 16) {
				((int*)dest) [0] = ((int*)src) [0];
				((int*)dest) [1] = ((int*)src) [1];
				((int*)dest) [2] = ((int*)src) [2];
				((int*)dest) [3] = ((int*)src) [3];
				dest += 16;
				src += 16;
				size -= 16;
			}
			while (size >= 4) {
				((int*)dest) [0] = ((int*)src) [0];
				dest += 4;
				src += 4;
				size -= 4;
			}
			while (size > 0) {
				((byte*)dest) [0] = ((byte*)src) [0];
				dest += 1;
				src += 1;
				--size;
			}
		}

		// Do not use directly. Use memcpy.
		static unsafe void memcpy2 (byte *dest, byte *src, int size) {
			while (size >= 8) {
				((short*)dest) [0] = ((short*)src) [0];
				((short*)dest) [1] = ((short*)src) [1];
				((short*)dest) [2] = ((short*)src) [2];
				((short*)dest) [3] = ((short*)src) [3];
				dest += 8;
				src += 8;
				size -= 8;
			}
			while (size >= 2) {
				((short*)dest) [0] = ((short*)src) [0];
				dest += 2;
				src += 2;
				size -= 2;
			}
			if (size > 0)
				((byte*)dest) [0] = ((byte*)src) [0];
		}

		// Do not use directly. Use memcpy.
		static unsafe void memcpy1 (byte *dest, byte *src, int size) {
			while (size >= 8) {
				((byte*)dest) [0] = ((byte*)src) [0];
				((byte*)dest) [1] = ((byte*)src) [1];
				((byte*)dest) [2] = ((byte*)src) [2];
				((byte*)dest) [3] = ((byte*)src) [3];
				((byte*)dest) [4] = ((byte*)src) [4];
				((byte*)dest) [5] = ((byte*)src) [5];
				((byte*)dest) [6] = ((byte*)src) [6];
				((byte*)dest) [7] = ((byte*)src) [7];
				dest += 8;
				src += 8;
				size -= 8;
			}
			while (size >= 2) {
				((byte*)dest) [0] = ((byte*)src) [0];
				((byte*)dest) [1] = ((byte*)src) [1];
				dest += 2;
				src += 2;
				size -= 2;
			}
			if (size > 0)
				((byte*)dest) [0] = ((byte*)src) [0];
		}

		static unsafe void memcpy (byte *dest, byte *src, int size) {
			// FIXME: if pointers are not aligned, try to align them
			// so a faster routine can be used. Handle the case where
			// the pointers can't be reduced to have the same alignment
			// (just ignore the issue on x86?)
			if ((((int)dest | (int)src) & 3) != 0) {
				if (((int)dest & 1) != 0 && ((int)src & 1) != 0 && size >= 1) {
					dest [0] = src [0];
					++dest;
					++src;
					--size;
				}
				if (((int)dest & 2) != 0 && ((int)src & 2) != 0 && size >= 2) {
					((short*)dest) [0] = ((short*)src) [0];
					dest += 2;
					src += 2;
					size -= 2;
				}
				if ((((int)dest | (int)src) & 1) != 0) {
					memcpy1 (dest, src, size);
					return;
				}
				if ((((int)dest | (int)src) & 2) != 0) {
					memcpy2 (dest, src, size);
					return;
				}
			}
			memcpy4 (dest, src, size);
		}

		public override bool CanRead {
			get {
				return false;
			}
		}

		public override bool CanSeek {
			get {
				return false;
			}
		}

		public override bool CanWrite {
			get {
				return true;
			}
		}

		const string notsupported = "HttpResponseStream is a forward, write-only stream";

		public override long Length {
			get {
				throw new NotSupportedException (notsupported);
			}
		}
	
		public override long Position {
			get {
				throw new NotSupportedException (notsupported);
			}
			set {
				throw new NotSupportedException (notsupported);
			}
		}
		
		public override long Seek (long offset, SeekOrigin origin)
		{
			throw new NotSupportedException (notsupported);
		}

		public override void SetLength (long value) 
		{
			throw new NotSupportedException (notsupported);
		}
	
		public override int Read (byte [] buffer, int offset, int count)
		{
			throw new NotSupportedException (notsupported);
		}
	}
}
