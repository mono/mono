// 
// PriorityQueue.cs
// 
// Authors:
// 	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2011 Alexander Chebaturkin
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

namespace Mono.CodeContracts.Static.DataStructures {
	class PriorityQueue<T> {
		private readonly Comparison<T> comparison;
		private readonly List<T> values;

		public PriorityQueue (Comparison<T> compare)
		{
			this.comparison = compare;
			this.values = new List<T> ();
			this.values.Add (default(T)); //fake cell for start from 1
		}

		public int Count
		{
			get { return this.values.Count - 1; }
		}

		public T Peek ()
		{
			if (Count == 0)
				return default(T);

			return this.values [1];
		}

		public T Dequeue ()
		{
			if (Count == 0)
				return default(T);

			T res = this.values [1];
			this.values [1] = this.values [this.values.Count - 1];
			this.values.RemoveAt (this.values.Count - 1);

			BubbleDown (1);

			return res;
		}

		public void Enqueue (T item)
		{
			this.values.Add (item);
			BubbleUp (this.values.Count - 1);
		}

		private void BubbleDown (int i)
		{
			int l = i << 1;
			int r = (i << 1) + 1;

			int min = i;
			if (l < this.values.Count && this.comparison (this.values [l], this.values [i]) < 0)
				min = l;
			if (r < this.values.Count && this.comparison (this.values [r], this.values [min]) < 0)
				min = r;

			if (min != i) {
				T tmp = this.values [min];
				this.values [min] = this.values [i];
				this.values [i] = tmp;
				BubbleDown (min);
			}
		}

		private void BubbleUp (int i)
		{
			int c = i;

			while (c/2 != 0 &&
			       this.comparison (this.values [c >> 1], this.values [c]) > 0) {
				T tmp = this.values [c >> 1];
				this.values [c >> 1] = this.values [c];
				this.values [c] = tmp;

				c >>= 1;
			}
		}
	}
}
