#if NET_4_0
//
// OrderingEnumerator.cs
//
// Author:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
//
// Copyright (c) 2010 Jérémie "Garuma" Laval
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
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace System.Linq
{
	// Remove use of KeyedBuffer
	/* Instead, we use a fixed array of nullables. The Add method put the object in its right slot based on the index.
	 * We use a barrier for synchronisation, each add check that the current range of index accepted in the array is correct
	 * and if not wait on the Barrier. The Barrier's generation index is used to calculate the next range.
	 * The IEnumerator interface simply skim the array in order and return the objects.
	 */
	internal class OrderingEnumerator<T> : IEnumerator<T>
	{
		readonly int num;

		public BlockingCollection<KeyValuePair<long, T>> KeyedBuffer;
		KeyValuePair<long, T>?[] store;

		IEnumerator<KeyValuePair<long, T>?> currEnum;
		KeyValuePair<long, T> curr;

		internal OrderingEnumerator (int num)
		{
			this.num = num;
			KeyedBuffer = new BlockingCollection<KeyValuePair<long, T>> ();
			store = new KeyValuePair<long, T>?[num];
		}

		public void Dispose ()
		{

		}

		public void Reset ()
		{

		}

		public bool MoveNext ()
		{
			if (currEnum == null || !currEnum.MoveNext () || !currEnum.Current.HasValue) {
				if (!UpdateCurrent ())
					return false;
			}

			if (!currEnum.Current.HasValue)
				return false;

			curr = currEnum.Current.Value;

			return true;
		}

		public T Current {
			get {
				return curr.Value;
			}
		}

		object IEnumerator.Current {
			get {
				return curr.Value;
			}
		}

		bool UpdateCurrent ()
		{
			if (KeyedBuffer.IsCompleted)
				return false;

			if (KeyedBuffer.Count != num) {
				SpinWait sw = new SpinWait ();
				while (KeyedBuffer.Count < num && !KeyedBuffer.IsAddingCompleted) {
					sw.SpinOnce ();
				}
			}

			// We gather the lot without removing it
			int i = 0;
			foreach (KeyValuePair<long, T> item in KeyedBuffer.GetConsumingEnumerable ()) {
				store[i] = item;

				if (++i == num || KeyedBuffer.IsCompleted)
					break;
			}

			for (int k = i; k < num; k++) {
				store[k] = null;
			}

			Array.Sort (store, ArraySort);

			currEnum = ((IEnumerable<KeyValuePair<long, T>?>)store).GetEnumerator ();

			return currEnum.MoveNext ();
		}

		int ArraySort (KeyValuePair<long, T>? e1, KeyValuePair<long, T>? e2)
		{
			if (!e1.HasValue) {
				if (!e2.HasValue)
					return 0;

				if (e2.HasValue)
					return 1;
			}
			if (!e2.HasValue && e1.HasValue)
				return -1;

			return e1.Value.Key.CompareTo (e2.Value.Key);
		}
	}
}
#endif
