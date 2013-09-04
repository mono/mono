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
		// We store hints in an int
		int hints;

		int count;
		// The container area is where bag are added foreach thread
		ConcurrentDictionary<int, CyclicDeque<T>> container = new ConcurrentDictionary<int, CyclicDeque<T>> ();
		// The staging area is where non-empty bag are located for fast iteration
		ConcurrentDictionary<int, CyclicDeque<T>> staging = new ConcurrentDictionary<int, CyclicDeque<T>> ();
		
		public ConcurrentBag ()
		{
		}
		
		public ConcurrentBag (IEnumerable<T> collection) : this ()
		{
			foreach (T item in collection)
				Add (item);
		}
		
		public void Add (T item)
		{
			int index;
			CyclicDeque<T> bag = GetBag (out index);
			bag.PushBottom (item);
			AddHint (index);
			Interlocked.Increment (ref count);
		}

		bool IProducerConsumerCollection<T>.TryAdd (T element)
		{
			Add (element);
			return true;
		}
		
		public bool TryTake (out T result)
		{
			result = default (T);

			if (count == 0)
				return false;

			int hintIndex;
			CyclicDeque<T> bag = GetBag (out hintIndex, false);
			bool ret = true;
			
			if (bag == null || bag.PopBottom (out result) != PopResult.Succeed) {
				var self = bag;
				foreach (var other in staging) {
					// Try to retrieve something based on a hint
					ret = TryGetHint (out hintIndex) && (bag = container[hintIndex]).PopTop (out result) == PopResult.Succeed;

					// We fall back to testing our slot
					if (!ret && other.Value != self) {
						var status = other.Value.PopTop (out result);
						while (status == PopResult.Abort)
							status = other.Value.PopTop (out result);
						ret = status == PopResult.Succeed;
						hintIndex = other.Key;
						bag = other.Value;
					}
					
					// If we found something, stop
					if (ret)
						break;
				}
			}

			if (ret) {
				TidyBag (hintIndex, bag);
				Interlocked.Decrement (ref count);
			}

			return ret;
		}

		public bool TryPeek (out T result)
		{
			result = default (T);

			if (count == 0)
				return false;

			int hintIndex;
			CyclicDeque<T> bag = GetBag (out hintIndex, false);
			bool ret = true;

			if (bag == null || !bag.PeekBottom (out result)) {
				var self = bag;
				foreach (var other in staging) {
					// Try to retrieve something based on a hint
					ret = TryGetHint (out hintIndex) && container[hintIndex].PeekTop (out result);

					// We fall back to testing our slot
					if (!ret && other.Value != self)
						ret = other.Value.PeekTop (out result);

					// If we found something, stop
					if (ret)
						break;
				}
			}

			return ret;
		}

		void AddHint (int index)
		{
			// We only take thread index that can be stored in 5 bits (i.e. thread ids 1-15)
			if (index > 0xF)
				return;
			var hs = hints;
			// If cas failed then we don't retry
			Interlocked.CompareExchange (ref hints, (int)(((uint)hs) << 4 | (uint)index), (int)hs);
		}

		bool TryGetHint (out int index)
		{
			/* Funny little thing to know, since hints is signed (because CAS has no uint overload),
			 * a shift-right operation is an arithmetic shift which might set high-order right bits
			 * to 1 instead of 0 if the number turns negative.
			 */
			var hs = hints;
			index = 0;

			if (Interlocked.CompareExchange (ref hints, (int)(((uint)hs) >> 4), hs) == hs)
				index = (int)(hs & 0xF);

			return index > 0;
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
		
		public IEnumerator<T> GetEnumerator ()
		{
			return GetEnumeratorInternal ();
		}
		
		IEnumerator<T> GetEnumeratorInternal ()
		{
			foreach (var bag in container)
				foreach (T item in bag.Value.GetEnumerable ())
					yield return item;
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
			return Thread.CurrentThread.ManagedThreadId;
		}
				
		CyclicDeque<T> GetBag (out int index, bool createBag = true)
		{
			index = GetIndex ();
			CyclicDeque<T> value;
			if (container.TryGetValue (index, out value))
				return value;

			var bag = createBag ? container.GetOrAdd (index, new CyclicDeque<T> ()) : null;
			if (bag != null)
				staging.TryAdd (index, bag);
			return bag;
		}

		void TidyBag (int index, CyclicDeque<T> bag)
		{
			if (bag != null && bag.IsEmpty) {
				if (staging.TryRemove (index, out bag) && !bag.IsEmpty)
					staging.TryAdd (index, bag);
			}
		}
	}
}
#endif