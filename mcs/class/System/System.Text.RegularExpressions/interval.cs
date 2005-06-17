//
// assembly:	System
// namespace:	System.Text.RegularExpressions
// file:	interval.cs
//
// author:	Dan Lewis (dlewis@gmx.co.uk)
// 		(c) 2002

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
using System.Collections;

namespace System.Text.RegularExpressions {

	struct Interval : IComparable {
		public int low;
		public int high;
		public bool contiguous;

		public static Interval Empty {
			get {
				Interval i;
				i.low = 0;
				i.high = i.low - 1;
				i.contiguous = true;

				return i;
			}
		}

		public static Interval Entire {
			get { return new Interval (Int32.MinValue, Int32.MaxValue); }
		}

		public Interval (int low, int high) {
			if (low > high) {
				int t = low;
				low = high;
				high = t;
			}
		
			this.low = low;
			this.high = high;
			this.contiguous = true;
		}

		public bool IsDiscontiguous {
			get { return !contiguous; }
		}
		
		public bool IsSingleton {
			get { return contiguous && low == high; }
		}

		public bool IsRange {
			get { return !IsSingleton && !IsEmpty; }
		}

		public bool IsEmpty {
			get { return low > high; }
		}

		public int Size {
			get {
				if (IsEmpty)
					return 0;
				
				return high - low + 1;
			}
		}

		public bool IsDisjoint (Interval i) {
			if (IsEmpty || i.IsEmpty)
				return true;
			
			return !(low <= i.high && i.low <= high);
		}

		public bool IsAdjacent (Interval i) {
			if (IsEmpty || i.IsEmpty)
				return false;
		
			return low == i.high + 1 || high == i.low - 1;
		}

		public bool Contains (Interval i) {
			if (!IsEmpty && i.IsEmpty)
				return true;
			if (IsEmpty)
				return false;
		
			return low <= i.low && i.high <= high;
		}

		public bool Contains (int i) {
			return low <= i && i <= high;
		}

		public bool Intersects (Interval i) {
 			if (IsEmpty || i.IsEmpty)
 				return false;
 			
 			return ((Contains (i.low) && !Contains (i.high)) ||
				(Contains (i.high) && !Contains (i.low)));
 		}	

		public void Merge (Interval i) {
			if (i.IsEmpty)
				return;
			if (IsEmpty) {
				this.low = i.low;
				this.high = i.high;
			}
		
			if (i.low < low)
				low = i.low;
			if (i.high > high)
				high = i.high;
		}

		public void Intersect (Interval i) {
			if (IsDisjoint (i)) {
				low = 0;
				high = low - 1;
				return;
			}
		
			if (i.low > low)
				low = i.low;
			if (i.high > high)
				high = i.high;
		}

		public int CompareTo (object o) {
			return low - ((Interval)o).low;
		}

		public new string ToString () {
			if (IsEmpty)
				return "(EMPTY)";
			else if (!contiguous)
				return "{" + low + ", " + high + "}";
			else if (IsSingleton)
				return "(" + low + ")";
			else
				return "(" + low + ", " + high + ")";
		}
	}

	class IntervalCollection : ICollection, IEnumerable {
		public IntervalCollection () {
			intervals = new ArrayList ();
		}

		public Interval this[int i] {
			get { return (Interval)intervals[i]; }
			set { intervals[i] = value; }
		}

		public void Add (Interval i) {
			intervals.Add (i);
		}
			
		public void Clear () {
			intervals.Clear ();
		}

		public void Sort () {
			intervals.Sort ();
		}
		
		public void Normalize () {
			intervals.Sort ();

			int j = 0;
			while (j < intervals.Count - 1) {
				Interval a = (Interval)intervals[j];
				Interval b = (Interval)intervals[j + 1];

				if (!a.IsDisjoint (b) || a.IsAdjacent (b)) {
					a.Merge (b);
					intervals[j] = a;
					intervals.RemoveAt (j + 1);
				}
				else
					++ j;
			}

		}

		public delegate double CostDelegate (Interval i);

		public IntervalCollection GetMetaCollection (CostDelegate cost_del) {
			IntervalCollection meta = new IntervalCollection ();
		
			Normalize ();
			Optimize (0, Count - 1, meta, cost_del);
			meta.intervals.Sort ();

			return meta;
		}

		private void Optimize (int begin, int end, IntervalCollection meta, CostDelegate cost_del) {
			Interval set;
			set.contiguous = false;
		
			int best_set_begin = -1;
			int best_set_end = -1;
			double best_set_cost = 0;

			for (int i = begin; i <= end; ++ i) {
				set.low = this[i].low;

				double cost = 0.0;
				for (int j = i; j <= end; ++ j) {
					set.high = this[j].high;
					cost += cost_del (this[j]);
					
					double set_cost = cost_del (set);
					if (set_cost < cost && cost > best_set_cost) {
						best_set_begin = i;
						best_set_end = j;
						best_set_cost = cost;
					}
				}
			}

			if (best_set_begin < 0) {
				// didn't find an optimal set: add original members

				for (int i = begin; i <= end; ++ i)
					meta.Add (this[i]);
			}
			else {
				// found set: add it ...

				set.low = this[best_set_begin].low;
				set.high = this[best_set_end].high;
				
				meta.Add (set);

				// ... and optimize to the left and right

				if (best_set_begin > begin)
					Optimize (begin, best_set_begin - 1, meta, cost_del);
				if (best_set_end < end)
					Optimize (best_set_end + 1, end, meta, cost_del);
			}
		}

		// ICollection implementation

		public int Count {
			get { return intervals.Count; }
		}

		public bool IsSynchronized {
			get { return false; }
		}

		public object SyncRoot {
			get { return intervals; }
		}

		public void CopyTo (Array array, int index) {
			foreach (Interval i in intervals) {
				if (index > array.Length)
					break;
				
				array.SetValue (i, index ++);
			}
		}

		// IEnumerator implementation

		public IEnumerator GetEnumerator () {
			return new Enumerator (intervals);
		}

		private class Enumerator : IEnumerator {
			public Enumerator (IList list) {
				this.list = list;
				Reset ();
			}

			public object Current {
				get {
					if (ptr >= list.Count)
						throw new InvalidOperationException ();

					return list[ptr];
				}
			}

			public bool MoveNext () {
				if (ptr > list.Count)
					throw new InvalidOperationException ();
				
				return ++ ptr < list.Count;
			}

			public void Reset () {
				ptr = -1;
			}

			private IList list;
			private int ptr;
		}

		// private fields

		private ArrayList intervals;
	}
}
