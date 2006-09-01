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
// Authors:
//
//	Copyright (C) 2006 Jordi Mas i Hernandez <jordimash@gmail.com>
//


using System;
using System.Collections;
using System.Collections.Generic;
using System.Workflow.ComponentModel;


namespace System.Workflow.Runtime
{
	[Serializable]
	public class TimerEventSubscriptionCollection : ICollection, IEnumerable
	{
		public static readonly DependencyProperty TimerCollectionProperty;
		private ArrayList list;

		static TimerEventSubscriptionCollection ()
		{

		}

		// Constructor is private in MS Net
		internal TimerEventSubscriptionCollection ()
		{
			list = new ArrayList ();
		}

		// Properties
		public int Count {
			get { return list.Count; }
		}

		bool ICollection.IsSynchronized {
			get { return false; }
		}

		object ICollection.SyncRoot {
			get { return this; }
		}

		// Private properties
		internal TimerEventSubscription this [int index] {
			get {return (TimerEventSubscription) list [index];}
		}

		// Methods
		public void Add (TimerEventSubscription item)
		{
			int range = list.Count;
			int middle, cmp;
			int left = 0;
			int right = range-1;

			// Adds items always in a stored order
			while (left <= right) {

				middle = (left + right) >> 1;
				cmp = Comparer.Default.Compare (item.ExpiresAt,
					((TimerEventSubscription) list [middle]).ExpiresAt);

				if (cmp == 0) {
					break;
				}

				if (cmp >  0) {
					left = middle + 1;
				}
				else {
					right = middle - 1;
				}
			}

			list.Insert (left, item);
		}

		public void CopyTo (Array array, int index)
		{
			list.CopyTo (array, index);
		}

		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		public TimerEventSubscription Peek ()
		{
			if (Count == 0)
				return null;

			return (TimerEventSubscription) list[0];
		}

		public void Remove (Guid timerSubscriptionId)
		{
			TimerEventSubscription te;

			for (IEnumerator enumerator = GetEnumerator (); enumerator.MoveNext (); ) {
				te = (TimerEventSubscription) enumerator.Current;
				if (timerSubscriptionId == te.SubscriptionId)  {
					Remove (te);
					return;
				}
			}
		}

		public void Remove (TimerEventSubscription item)
		{
			list.Remove (item);
		}

	}
}


