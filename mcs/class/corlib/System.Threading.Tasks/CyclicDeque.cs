// 
// CyclicDeque.cs
//  
// Author:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
// 
// Copyright (c) 2009 Jérémie "Garuma" Laval
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

#if NET_4_0 || MOBILE

using System;
using System.Collections.Generic;
using System.Threading;

#if INSIDE_MONO_PARALLEL
namespace Mono.Threading.Tasks
#else
namespace System.Threading.Tasks
#endif
{
#if INSIDE_MONO_PARALLEL
	public
#endif
	class CyclicDeque<T> : IConcurrentDeque<T>
	{
		const int BaseSize = 11;
		
		long bottom;
		long top;
		long upperBound;
		CircularArray<T> array = new CircularArray<T> (BaseSize);
		
		public void PushBottom (T obj)
		{
			/* Read is implemented as a simple load operation on 64bits
			 * so no need to make the distinction ourselves
			 */
			long b = Interlocked.Read (ref bottom);
			var a = array;
			
			// Take care of growing
			if (b - upperBound >= a.Size - 1) {
				upperBound = Interlocked.Read (ref top);
				a = a.Grow (b, upperBound);
				array = a;
			}
			
			// Register the new value
			a.segment[b % a.size] = obj;
			Interlocked.Increment (ref bottom);
		}
		
		public PopResult PopBottom (out T obj)
		{
			obj = default (T);
			
			long b = Interlocked.Decrement (ref bottom);
			var a = array;
			long t = Interlocked.Read (ref top);
			long size = b - t;
			
			if (size < 0) {
				// Set bottom to t
				Interlocked.Add (ref bottom, t - b);
				return PopResult.Empty;
			}
			
			obj = a.segment[b % a.size];
			if (size > 0)
				return PopResult.Succeed;
			Interlocked.Add (ref bottom, t + 1 - b);
			
			if (Interlocked.CompareExchange (ref top, t + 1, t) != t)
				return PopResult.Empty;
			
			return PopResult.Succeed;
		}
		
		public PopResult PopTop (out T obj)
		{
			obj = default (T);
			
			long t = Interlocked.Read (ref top);
			long b = Interlocked.Read (ref bottom);
			
			if (b - t <= 0)
				return PopResult.Empty;
			
			if (Interlocked.CompareExchange (ref top, t + 1, t) != t)
				return PopResult.Abort;
			
			var a = array;
			obj = a.segment[t % a.size];
			
			return PopResult.Succeed;
		}
		
		public IEnumerable<T> GetEnumerable ()
		{
			var a = array;
			return a.GetEnumerable (bottom, ref top);
		}
		
		internal bool PeekTop (out T obj)
		{
			obj = default (T);
			
			long t = Interlocked.Read (ref top);
			long b = Interlocked.Read (ref bottom);
			
			if (b - t <= 0)
				return false;
			
			var a = array;
			obj = a.segment[t % a.size];
			
			return true;
		}
	}
	
	internal class CircularArray<T>
	{
		readonly int baseSize;
		public readonly int size;
		public readonly T[] segment;
		
		public CircularArray (int baseSize)
		{
			this.baseSize = baseSize;
			this.size = 1 << baseSize;
			this.segment = new T[size];
		}
		
		public long Size {
			get {
				return size;
			}
		}
		
		public T this[long index] {
			get {
				return segment[index % size];
			}
			set {
				segment[index % size] = value;
			}
		}
		
		public CircularArray<T> Grow (long bottom, long top)
		{
			var grow = new CircularArray<T> (baseSize + 1);
			
			for (long i = top; i < bottom; i++) {
				grow.segment[i] = segment[i % size];
			}
			
			return grow;
		}
		
		public IEnumerable<T> GetEnumerable (long bottom, ref long top)
		{
			long instantTop = top;
			T[] slice = new T[bottom - instantTop];
			int destIndex = -1;
			for (long i = instantTop; i < bottom; i++)
				slice[++destIndex] = segment[i % size];

			return RealGetEnumerable (slice, bottom, top, instantTop);
		}

		IEnumerable<T> RealGetEnumerable (T[] slice, long bottom, long realTop, long initialTop)
		{
			int destIndex = (int)(realTop - initialTop - 1);
			for (long i = realTop; i < bottom; ++i)
				yield return slice[++destIndex];
		}
	}
}
#endif
