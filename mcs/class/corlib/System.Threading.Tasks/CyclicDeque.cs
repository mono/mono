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

#if NET_4_0

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
		
		int bottom;
		int top;
		int upperBound;
		CircularArray<T> array = new CircularArray<T> (BaseSize);
		
		public void PushBottom (T obj)
		{
			int b = bottom;
			var a = array;
			
			// Take care of growing
			var size = b - top - upperBound;
			if (size >= a.Size) {
				upperBound = top;
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
			
			int b = Interlocked.Decrement (ref bottom);
			var a = array;
			int t = top;
			int size = b - t;
			
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

		public bool PeekBottom (out T obj)
		{
			obj = default (T);

			int b = Interlocked.Decrement (ref bottom);
			var a = array;
			int t = top;
			int size = b - t;

			if (size < 0)
				return false;

			obj = a.segment[b % a.size];
			return true;
		}
		
		public PopResult PopTop (out T obj)
		{
			obj = default (T);
			
			int t = top;
			int b = bottom;
			
			if (b - t <= 0)
				return PopResult.Empty;
			
			if (Interlocked.CompareExchange (ref top, t + 1, t) != t)
				return PopResult.Abort;
			
			var a = array;
			obj = a.segment[t % a.size];
			
			return PopResult.Succeed;
		}

		internal bool PeekTop (out T obj)
		{
			obj = default (T);

			int t = top;
			int b = bottom;

			if (b - t <= 0)
				return false;

			var a = array;
			obj = a.segment[t % a.size];

			return true;
		}
		
		public IEnumerable<T> GetEnumerable ()
		{
			var a = array;
			return a.GetEnumerable (bottom, ref top);
		}

		public bool IsEmpty {
			get {
				int t = top;
				int b = bottom;
				return b - t <= 0;
			}
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
		
		public int Size {
			get {
				return size;
			}
		}
		
		public T this[int index] {
			get {
				return segment[index % size];
			}
			set {
				segment[index % size] = value;
			}
		}
		
		public CircularArray<T> Grow (int bottom, int top)
		{
			var grow = new CircularArray<T> (baseSize + 1);
			
			for (int i = top; i < bottom; i++) {
				grow.segment[i] = segment[i % size];
			}
			
			return grow;
		}
		
		public IEnumerable<T> GetEnumerable (int bottom, ref int top)
		{
			int instantTop = top;
			T[] slice = new T[bottom - instantTop];
			int destIndex = -1;
			for (int i = instantTop; i < bottom; i++)
				slice[++destIndex] = segment[i % size];

			return RealGetEnumerable (slice, bottom, top, instantTop);
		}

		IEnumerable<T> RealGetEnumerable (T[] slice, int bottom, int realTop, int initialTop)
		{
			int destIndex = (int)(realTop - initialTop - 1);
			for (int i = realTop; i < bottom; ++i)
				yield return slice[++destIndex];
		}
	}
}
#endif
