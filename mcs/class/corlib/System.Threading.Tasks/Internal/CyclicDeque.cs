#if NET_4_0 || BOOTSTRAP_NET_4_0
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

using System;
using System.Collections.Generic;
using System.Threading;

namespace System.Threading.Tasks
{
  internal enum PopResult	{
		Succeed,
		Empty,
		Abort
	}

	internal interface IDequeOperations<T>
	{
		void PushBottom (T obj);
		PopResult PopBottom (out T obj);
		PopResult PopTop (out T obj);
	}
	
	internal interface IValueReader
	{
		long Read (ref long value);
	}
	
	internal class CyclicDeque<T> : IDequeOperations<T>
	{
		readonly IValueReader reader;
		const int BaseSize = 11;
		
		class Reader32 : IValueReader
		{
			public long Read (ref long value)
			{
				return Interlocked.Read (ref value);
			}
		}
		
		class Reader64 : IValueReader
		{
			public long Read (ref long value)
			{
				return value;
			}
		}
		
		public CyclicDeque ()
		{
			// We do the distinction between 32bits and 64bits
			if (IntPtr.Size == 4)
				reader = new Reader32 ();
			else
				reader = new Reader64 ();
		}
		
		long bottom;
		long top;
		long upperBound;
		CircularArray<T> array = new CircularArray<T> (BaseSize);
		
		public void PushBottom (T obj)
		{
			long b = reader.Read (ref bottom);
			var a = array;
			
			// Take care of growing
			if (b - upperBound >= a.Size - 1) {
				upperBound = reader.Read (ref top);
				a = a.Grow (b, upperBound);
				Interlocked.Exchange (ref array, a);
			}
			
			// Register the new value
			a[b] = obj;
			Interlocked.Increment (ref bottom);
		}
		
		public PopResult PopBottom (out T obj)
		{
			obj = default (T);
			
			long b = Interlocked.Decrement (ref bottom);
			var a = array;
			long t = reader.Read (ref top);
			long size = b - t;
			
			if (size < 0) {
				// Set bottom to t
				Interlocked.Add (ref bottom, t - b);
				return PopResult.Empty;
			}
			
			obj = a[b];
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
			
			long t = reader.Read (ref top);
			long b = reader.Read (ref bottom);
			
			if (b - t <= 0)
				return PopResult.Empty;
			
			if (Interlocked.CompareExchange (ref top, t + 1, t) != t)
				return PopResult.Abort;
			
			var a = array;
			obj = a[t];
			
			return PopResult.Succeed;
		}
		
		public IEnumerable<T> GetEnumerable ()
		{
			var a = array;
			return a.GetEnumerable ();
		}
	}
	
	internal class CircularArray<T>
	{
		readonly int baseSize;
		readonly int size;
		readonly T[] segment;
		
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
				return segment[index % Size];
			}
			set {
				segment[index % Size] = value;
			}
		}
		
		public CircularArray<T> Grow (long bottom, long top)
		{
			var grow = new CircularArray<T> (baseSize + 1);
			
			for (long i = top; i < bottom; i++) {
				grow[i] = this[i];
			}
			
			return grow;
		}
		
		public IEnumerable<T> GetEnumerable ()
		{
			return ((IEnumerable<T>)segment);
		}
	}
}
#endif
