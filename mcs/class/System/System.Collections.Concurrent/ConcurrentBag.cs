// 
// ConcurrentBag.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

using System.Threading;
using System.Threading.Tasks;

namespace System.Collections.Concurrent
{
	[ComVisible (false)]
	[DebuggerDisplay ("Count={Count}")]
	[DebuggerTypeProxy (typeof (CollectionDebuggerView<>))]
	public class ConcurrentBag<T> : IProducerConsumerCollection<T>, IEnumerable<T>, IEnumerable
	{
		const int multiplier = 2;
		int size = Environment.ProcessorCount + 1;
		int count;
		
		CyclicDeque<T>[] container;
		
		object syncLock = new object ();
		
		public ConcurrentBag ()
		{
			container = new CyclicDeque<T>[size];
			for (int i = 0; i < container.Length; i++)
				container[i] = new CyclicDeque<T> ();
		}
		
		public ConcurrentBag (IEnumerable<T> enumerable) : this ()
		{
			foreach (T item in enumerable)
				Add (item);
		}
		
		public bool TryAdd (T item)
		{
			Add (item);
			
			return true;
		}
		
		public void Add (T item)
		{
			Interlocked.Increment (ref count);
			GrowIfNecessary ();
			
			CyclicDeque<T> bag = GetBag ();
			bag.PushBottom (item);
		}
		
		public bool TryTake (out T item)
		{
			item = default (T);
			CyclicDeque<T> bag = GetBag ();
			
			if (bag == null || bag.PopBottom (out item) != PopResult.Succeed) {
				for (int i = 0; i < container.Length; i++) {
					if (container[i].PopTop (out item) == PopResult.Succeed) {
						Interlocked.Decrement (ref count);
						return true;
					}
				}
			} else {
				Interlocked.Decrement (ref count);
				return true;
			}
			
			return false;
		}
		
		public int Count {
			get {
				return count;
			}
		}
		
		public bool IsEmpty {
			get {
				return count == 0;
			}
		}
		
		object System.Collections.ICollection.SyncRoot  {
			get {
				return this;
			}
		}
		
		bool System.Collections.ICollection.IsSynchronized  {
			get {
				return true;
			}
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumeratorInternal ();
		}
		
		IEnumerator<T> IEnumerable<T>.GetEnumerator ()
		{
			return GetEnumeratorInternal ();
		}
		
		IEnumerator<T> GetEnumeratorInternal ()
		{
			for (int i = 0; i < size; i++) {
				CyclicDeque<T> bag = container[i];
				foreach (T item in bag.GetEnumerable ()) {
					yield return item;
				}
			}
		}
		
		void System.Collections.ICollection.CopyTo (Array array, int index)
		{
			T[] a = array as T[];
			if (a == null)
				return;
			
			CopyTo (a, index);
		}
		
		public void CopyTo (T[] array, int index)
		{
			int c = count;
			if (array.Length < c + index)
				throw new InvalidOperationException ("Array is not big enough");
			
			CopyTo (array, index, c);
		}
		
		void CopyTo (T[] array, int index, int num)
		{
			int i = index;
			
			foreach (T item in this) {
				if (i >= num)
					break;
				
				array[i++] = item;
			}
		}
		
		public T[] ToArray ()
		{
			int c = count;
			T[] temp = new T[c];
			
			CopyTo (temp, 0, c);
			
			return temp;
		}
			
		int GetIndex ()
		{
			return Thread.CurrentThread.ManagedThreadId - 1;
		}
		
		void GrowIfNecessary ()
		{
			int index = GetIndex ();
			int currentSize = size;
			
			while (index > currentSize - 1) {
				currentSize = size;
				Grow (currentSize);
			}
		}
		
		CyclicDeque<T> GetBag ()
		{			
			int i = GetIndex ();
			
			return i < container.Length ? container[i] : null;
		}
		
		void Grow (int referenceSize)
		{
			lock (syncLock) {
				if (referenceSize != size)
					return;
				
				CyclicDeque<T>[] slice = new CyclicDeque<T>[size * multiplier];
				int i = 0;
				for (i = 0; i < container.Length; i++)
					slice[i] = container[i];
				for (; i < slice.Length; i++)
					slice[i] = new CyclicDeque<T> ();
				
				container = slice;
				size = slice.Length;
			}
		}
	}
}
#endif