//
// BufferManager.cs:
//    This class suffers from an engineering problem in its
//    design: when this API is used to limit the total pool
//    size it will throw, but no user code is designed to
//    cope with that.
//
//    Instead of the Microsoft strategy, we allow allocation
//    to go as far as it wants to go and merely allow this
//    to be a pool that can be used recycle buffers.
//
//    This still gives us the main benefit of this class, while
//    avoiding the potential crashing scenarios and simplifies
//    the implementation significantly from what has been
//    document in the blogosphere.
//
//    There are a few problems: for example, if we do not find
//    a buffer of the proper size in the expected slot, say
//    a 31k buffer in the slot for [32k-64k] values, we will
//    allocate a new buffer, even if there might have been a
//    buffer for 128k.
//
// A few considerations:
//
//    The size of an empty array is either 16 on 32 bit systems
//    and 32 bytes in 64 bit systems.
//
//    We take this information into account for the minimum allocation
//    pools.
//
// Authors:
//   Atsushi Enomoto (atsushi@ximian.com)
//   Miguel de Icaza (miguel@gnome.org)
//
// Copyright (C) 2005, 2010 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.IO;
using System.Collections.ObjectModel;
using System.ServiceModel;

namespace System.ServiceModel.Channels
{
	public abstract class BufferManager
	{
		protected BufferManager ()
		{
		}

		public abstract void Clear ();

		public static BufferManager CreateBufferManager (
			long maxBufferPoolSize, int maxBufferSize)
		{
			return new DefaultBufferManager (maxBufferPoolSize, maxBufferSize);
		}

		public abstract void ReturnBuffer (byte[] buffer);

		public abstract byte[] TakeBuffer (int bufferSize);

#if DEBUG_BUFFER
		internal abstract void DumpStats ();
#endif
		
		class DefaultBufferManager : BufferManager
		{
			const int log_min = 5;   // Anything smaller than 1 << log_cut goes into the first bucket
			long max_pool_size;
			int max_size;
			List<byte []> [] buffers = new List<byte []> [32-log_min];

#if DEBUG_BUFFER
			internal override void DumpStats ()
			{
				Console.WriteLine ("- hit={0} miss={1}-", hits, miss);
				for (int i = 0; i < buffers.Length; i++){
					if (buffers [i] == null)
						continue;
					
					Console.Write ("Slot {0} - {1} [", i, buffers [i].Count);
					byte [][] arr = buffers [i].ToArray ();
					
					for (int j = 0; j < Math.Min (3, arr.Length); j++)
						Console.Write ("{0} ", arr [j].Length);
					Console.WriteLine ("]");
				}
			}
#endif
			
			static int log2 (uint n)
			{
				int pos = 0;
				if (n >= 1<<16) {
					n >>= 16;
					pos += 16;
				}
				if (n >= 1<< 8) {
					n >>=  8;
					pos +=  8;
				}
				if (n >= 1<< 4) {
					n >>=  4;
					pos +=  4;
				}
				if (n >= 1<< 2) {
					n >>=  2;
					pos +=  2;
				}
				if (n >= 1<< 1) 
					pos +=  1;

				return ((n == 0) ? (-1) : pos);
			}
			
			public DefaultBufferManager (long maxBufferPoolSize, int maxBufferSize)
			{
				this.max_pool_size = maxBufferPoolSize;
				this.max_size = maxBufferSize;
			}

			public override void Clear ()
			{
				foreach (var stack in buffers){
					if (stack == null)
						continue;
					stack.Clear ();
				}
				Array.Clear (buffers, 0, buffers.Length);
			}

			public override void ReturnBuffer (byte [] buffer)
			{
				if (buffer == null)
					return;

				uint size = (uint) buffer.Length;
				int l2 = log2 (size);
				if (l2 > log_min)
					l2 -= log_min;

				List<byte []> returned = buffers [l2];
				if (returned == null)
					returned = buffers [l2] = new List<byte []> ();

				returned.Add (buffer);
			}

			int hits, miss;
			
			public override byte [] TakeBuffer (int bufferSize)
			{
				if (bufferSize < 0 || (max_size >= 0 && bufferSize > max_size))
					throw new ArgumentOutOfRangeException ();

				int l2 = log2 ((uint) bufferSize);
				if (l2 > log_min)
					l2 -= log_min;

				List<byte []> returned = buffers [l2];
				if (returned == null || returned.Count == 0)
					return new byte [bufferSize];
				
				foreach (var e in returned){
					if (e.Length >= bufferSize){
						hits++;
						returned.Remove (e);
						return e;
					}
				}
				return new byte [bufferSize];
			}
		}
	}

#if DEBUG_BUFFER
	class Foo {
		static void Main ()
		{
			var a = BufferManager.CreateBufferManager (1024*1024, 1024*1024);
			var rand = new Random (0);
			
			var buffs = new List<byte []> ();
			for (int i = 0; i < 4096; i++){
				a.DumpStats ();
				var request = rand.Next (1,1024*1024);
				if ((i % 2) == 0)
					request = rand.Next (1024, 4096);
				
				var x = a.TakeBuffer (request);
				if (x.Length < request)
					throw new Exception ();
				Console.WriteLine ("Delta={2} Requested {0} got={1} bytes ", request, x.Length, x.Length-request);
				if ((i % 3) == 0){
					Console.WriteLine ("Return: {0}", x.Length);
					a.ReturnBuffer (x);
				}
				else
					buffs.Add (x);
			}
			a.DumpStats ();
		}
	}
#endif
}