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
		Bucket split_after_file;
		HttpResponse response;
		internal long total;

		public HttpResponseStream (HttpResponse response)
		{
			this.response = response;
		}
		
#if TARGET_JVM
		class Chunk
		{
			public byte[] data;
			public int size;
			public const int Length = 32 * 1024;
			
			public Chunk Next, Prev;

			public Chunk () 
			{
				data = new byte[Length];
				size = Length;
			}
	
			public void Dispose ()
			{
			}
	
			public static Chunk Unlink (ref Chunk head, Chunk b)
			{
				if (head == b)
					head = head.Next;
				if (b.Prev != null)
					b.Prev.Next = b.Next;
				if (b.Next != null)
					b.Next.Prev = b.Prev;

				b.Next = b.Prev = null;
				return b;
			}
	
			public static Chunk Link (Chunk head, Chunk b)
			{
				b.Next = head;
				if (head != null)
					head.Prev = b;
				return b;
			}

			public static void Copy(byte[] buff, int offset, Chunk c, int pos, int len)
			{
				Array.Copy(buff, offset, c.data, pos, len);
			}

			public static void Copy(Chunk c, int pos, byte[] buff, int offset, int len)
			{
				Array.Copy(c.data, pos, buff, offset, len);
			}
		}

		sealed class BufferManager {
			static Chunk filled;
			static Chunk empty;
	
			// TODO: if cb > chunk size, try to get a larger chunk
			public static Chunk GetChunk (int cb)
			{
				Chunk c;
				if (empty == null)
					empty = new Chunk ();
				
				c = empty;
				filled = Chunk.Link (filled, Chunk.Unlink (ref empty, c));
				return c;
			}
	
			public static void DisposeChunk (Chunk c)
			{
				empty = Chunk.Link (empty, Chunk.Unlink (ref filled, c));
			}
	
			public static void DisposeEmptyChunks ()
			{
				for (Chunk c = empty; c != null; c = c.Next)
					c.Dispose ();
				empty = null;
			}
			
	
			public static void PrintState ()
			{
				Console.WriteLine ("Filled blocks:");
				for (Chunk c = filled; c != null; c = c.Next)
					Console.WriteLine ("\t{0}", c);
				Console.WriteLine ("Empty blocks:");
				for (Chunk c = empty; c != null; c = c.Next)
					Console.WriteLine ("\t{0}", c);	
				
			}
		}

#else // TARGET_JVM
		unsafe class Block {
			byte* data;

			const int PreferredLength = 128 * 1024;
			public const int ChunkSize = 32 * 1024;
			const int PreferredChunks = PreferredLength / ChunkSize;

			int actual_length;
			int nchunks;
			int taken;
	
			public Block Next, Prev;
		
	
			public Block () : this (PreferredLength)
			{
			}

			public Block (int initial_size)
			{
				if (initial_size <= PreferredLength) {
					actual_length = PreferredLength;
					nchunks = PreferredChunks;
				} else {
					actual_length = initial_size;
					nchunks = 1;
				}
				data = (byte*) Marshal.AllocHGlobal (actual_length);
			}
	
			public void Dispose ()
			{
				Marshal.FreeHGlobal ((IntPtr) data);
			}
	
			public Chunk GetChunk (int size)
			{
				Chunk c = new Chunk (size);
				for (int i = 0; i < nchunks; i ++) {
					if ((taken & (1 << i)) == 0) {
						c.block = this;
						c.block_area = 1 << i;
						c.data = data + i * ChunkSize;
						taken |= c.block_area;
						return c;
					}
				}
	
				throw new Exception ("Internal error: shouldn't get here");
			}
	
			public static Block Unlink (ref Block list, Block b)
			{
				if (b.Prev != null)
					b.Prev.Next = b.Next;
				if (b.Next != null)
					b.Next.Prev = b.Prev;
				if (b == list)
					list = list.Next;
				
				b.Next = b.Prev = null;
				return b;
			}
	
			public static void Link (ref Block head, Block b)
			{
				b.Next = head;
				if (head != null)
					head.Prev = b;
				head = b;
			}
	
			public void Return (Chunk c)
			{
				taken &= ~c.block_area;
			}
			
			public bool IsEmpty {
				get {
					return taken == 0;
				}
			}
	
			public bool IsFull {
				get {				
					return taken == (1 << nchunks) - 1;
				}
			}
	
			public override string ToString ()
			{
				string bitmap = "";
				for (int i = 0; i < nchunks; i ++) {
					if ((taken & (1 << i)) == 0)
						bitmap += ".";
					else
						bitmap += "x";
				}
	
				return String.Format ("0x{0:x} {1}", (IntPtr) data, bitmap);
			}
			
		}
	
		unsafe struct Chunk {
			public byte* data;
			public int size;
			public int block_area;
			public Block block;

			public Chunk (int size)
			{
				this.size = size;
				data = (byte *) 0;
				block_area = 0;
				block = null;
			}

			public static void Copy(byte[] buff, int offset, Chunk c, int pos, int len)
			{
				Marshal.Copy (buff, offset, (IntPtr) (c.data + pos), len);
			}

			public static void Copy(Chunk c, int pos, byte[] buff, int offset, int len)
			{
				Marshal.Copy ((IntPtr) (c.data + pos), buff, offset, len);
			}
	
			static Block filled;
			static Block part_filled;
			static Block empty;
	
			// TODO: if cb > chunk size, try to get a larger chunk
			public static Chunk GetChunk (int cb)
			{
				if (cb < Block.ChunkSize)
					cb = Block.ChunkSize;

				Chunk c;
				if (part_filled != null) {
					c = part_filled.GetChunk (cb);
					if (part_filled.IsFull)
						Block.Link (ref filled, Block.Unlink (ref part_filled, part_filled));
					
					return c;
				}
	
				if (empty == null)
					Block.Link (ref empty, new Block (cb));
				
				c = empty.GetChunk (cb);
				if (empty.IsFull) // account for the case where we have 1 chunk/block
					Block.Link (ref filled, Block.Unlink (ref empty, empty));
				else
					Block.Link (ref part_filled, Block.Unlink (ref empty, empty));
				return c;
			}
	
			public static void DisposeChunk (Chunk c)
			{
				Block b = c.block;
				bool was_full = b.IsFull;
	
				b.Return (c);
	
				if (was_full)
					Block.Link (ref part_filled, Block.Unlink (ref filled, b));
				if (b.IsEmpty)
					Block.Link (ref empty, Block.Unlink (ref part_filled, b));
			}
	
			public static void DisposeEmptyChunks ()
			{
				for (Block b = empty; b != null; b = b.Next)
					b.Dispose ();
				empty = null;
			}
			
			/*	
			public static void PrintState ()
			{
				Console.WriteLine ("Filled blocks:");
				for (Block b = filled; b != null; b = b.Next)
					Console.WriteLine ("\t{0}", b);
				Console.WriteLine ("Part Filled blocks:");
				for (Block b = part_filled; b != null; b = b.Next)
					Console.WriteLine ("\t{0}", b);
				Console.WriteLine ("Empty blocks:");
				for (Block b = empty; b != null; b = b.Next)
					Console.WriteLine ("\t{0}", b);	
				
			}
			*/
			
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
			Chunk c;
			int start;
			int pos;
			int rem;
			bool partial;
	
			ByteBucket () {}
			
			public ByteBucket (int cb)
			{
				c = Chunk.GetChunk (cb);
				start = pos = 0;
				rem = c.size;
			}

			public override int Length {
				get { return pos - start; }
			}

			public int Write (byte [] buf, int offset, int count)
			{
				int copy = Math.Min (rem, count);
				if (copy == 0)
					return copy;
				
				Chunk.Copy (buf, offset, c, pos, copy);
				
				pos += copy;
				rem -= copy;
	
				return copy;
			}
	
			public ByteBucket SplitOff ()
			{
				// Don't give people a really short bucket.
				if (rem < 4 * 1024)
					return null;
	
				ByteBucket b = new ByteBucket ();
				b.partial = true;
				b.c = c;
				b.start = b.pos = pos;
				b.rem = rem;
				return b;
			}
			
			
			public override void Dispose ()
			{
				if (!partial)
					Chunk.DisposeChunk (c);
			}

			public override void Send (HttpWorkerRequest wr)
			{
				int len = pos - start;
#if false
				for (int i = 0; i < len; i++)
					Console.Write ("[{0}:{1}]", start [i], start [i]);
				Console.WriteLine ("Sending {0} bytes", len);
				Console.WriteLine (Environment.StackTrace);
#endif
				
#if TARGET_JVM
				if (start == 0)
					wr.SendResponseFromMemory (c.data, len);
				else
				{
					byte[] buf = new byte[len];
					Chunk.Copy(c, start, buf, 0, len);
				}
#else
				wr.SendResponseFromMemory ((IntPtr) (c.data + start), len);
#endif
			}

			public override void Send (Stream stream)
			{
				int len = (int) (pos - start);
				byte [] copy = new byte [len];
				Chunk.Copy (c, start, copy, 0, len);
				stream.Write (copy, 0, len);
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
			if (response.use_chunked != null)
				response.SendSize (total);
			
			for (Bucket b = first_bucket; b != null; b = b.Next) 
				b.Send (wr);

			if (response.use_chunked != null)
				wr.SendResponseFromMemory (HttpResponse.ChunkedNewline, 2);

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

		internal byte [] GetData ()
		{
			MemoryStream stream = new MemoryStream ();
			for (Bucket b = first_bucket; b != null; b = b.Next)
				b.Send (stream);
			return stream.GetBuffer ();
		}

		public void WriteFile (string f, long offset, long length)
		{
			ByteBucket bb = cur_bucket as ByteBucket;

			if (bb != null)
				split_after_file = bb.SplitOff ();

			total += length;
			
			AppendBucket (new BufferedFileBucket (f, offset, length));
		}

	        public override void Write (byte [] buffer, int offset, int count) 
		{
			if (cur_bucket == null)
				AppendBucket (new ByteBucket (count));

			total += count;
			while (count > 0) {
				if (cur_bucket is ByteBucket) {
					int n = ((ByteBucket) cur_bucket).Write (buffer, offset, count);
					offset += n;
					count -= n;
				}
	
				if (split_after_file != null) {
					AppendBucket (split_after_file);
					split_after_file = null;
					continue;
				}
				
				if (count != 0)
					AppendBucket (new ByteBucket (count));
			}
		}

		//
		// This should not flush/close or anything else, its called
		// just to free any memory we might have allocated (when we later
		// implement something with unmanaged memory).
		//
		internal void ReleaseResources ()
		{
			for (Bucket b = first_bucket; b != null; b = b.Next)
				b.Dispose ();

			first_bucket = null;
			cur_bucket = null;
			split_after_file = null;
		}

		public void Clear ()
		{
			//
			// IMPORTANT: you must dispose *AFTER* using all the buckets Byte chunks might be
			// split across two buckets if there is a file between the data.
			//
			ReleaseResources ();
			total = 0;
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
				throw new InvalidOperationException (notsupported);
			}
	        }
	
		public override long Position {
	                get {
				throw new InvalidOperationException (notsupported);
			}
	                set {
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

