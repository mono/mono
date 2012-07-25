// ObjectPool.cs
//
// Copyright (c) 2011 Novell
//
// Authors: 
//      Jérémie "garuma" Laval
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
//
//

#if NET_4_0 || MOBILE || BOOTSTRAP_NET_4_0 || INSIDE_SYSTEM_WEB

using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace System.Collections.Concurrent
{
	internal abstract class ObjectPool<T> where T : class
	{
		// This is the number of objects we are going to cache
		const int capacity = 20;
		/* We use this bit in addIndex to synchronize the array and the index itself
		 * Namely when we update addIndex we also set that bit for the time the value
		 * in the array hasn't still been updated to the object we are returning to the cache
		 */
		const int bit = 0x8000000;

		readonly T[] buffer;
		int addIndex;
		int removeIndex;

		public ObjectPool ()
		{
			buffer = new T[capacity];
			for (int i = 0; i < capacity; i++)
				buffer[i] = Creator ();
			addIndex = capacity - 1;
		}

		/* Code that want to use a pool subclass it and
		 * implement that method. In most case 'new T ()'.
		 */
		protected abstract T Creator ();

		public T Take ()
		{
			// If no element in the cache, we return a new object
			if ((addIndex & ~bit) - 1 == removeIndex)
				return Creator ();

			int i;
			T result;
			int tries = 3;

			do {
				i = removeIndex;
				// We return a new element when looping becomes too costly
				if ((addIndex & ~bit) - 1 == i || tries == 0)
					return Creator ();
				result = buffer[i % capacity];
			} while (Interlocked.CompareExchange (ref removeIndex, i + 1, i) != i && --tries > -1);

			return result;
		}

		public void Release (T obj)
		{
			if (obj == null || addIndex - removeIndex >= capacity - 1)
				return;

			int i;
			int tries = 3;
			do {
				// While an array update is ongoing (i.e. an extra write op) we loop
				do {
					i = addIndex;
				} while ((i & bit) > 0);
				// If no more room or too busy just forget about the object altogether
				if (i - removeIndex >= capacity - 1 || tries == 0)
					return;
				// We update addIndex and notify that we are going to set buffer correctly
			} while (Interlocked.CompareExchange (ref addIndex, i + 1 + bit, i) != i && --tries > -1);

			buffer[i % capacity] = obj;
			Thread.MemoryBarrier ();
			// Since bit essentialy acts like a lock, we simply use an atomic read/write combo
			addIndex = addIndex - bit;
		}
	}
}

#endif
