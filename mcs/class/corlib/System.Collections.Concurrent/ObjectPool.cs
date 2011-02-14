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

#if NET_4_0 || MOBILE

using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace System.Collections.Concurrent
{
	internal abstract class ObjectPool<T> where T : class
	{
		const int capacity = 20;
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

		protected abstract T Creator ();

		public T Take ()
		{
			if ((addIndex & ~bit) - 1 == removeIndex)
				return Creator ();

			int i;
			T result;

			do {
				i = removeIndex;
				if ((addIndex & ~bit) - 1 == i)
					return Creator ();
				result = buffer[i % capacity];
			} while (Interlocked.CompareExchange (ref removeIndex, i + 1, i) != i);

			return result;
		}

		public void Release (T obj)
		{
			if (obj == null || addIndex - removeIndex >= capacity - 1)
				return;

			int i;
			int tries = 3;
			do {
				do {
					i = addIndex;
				} while ((i & bit) > 0);
				if (i - removeIndex >= capacity - 1)
					return;
			} while (Interlocked.CompareExchange (ref addIndex, i + 1 + bit, i) != i && --tries > 0);

			buffer[i % capacity] = obj;
			addIndex = addIndex - bit;
		}
	}
}

#endif
