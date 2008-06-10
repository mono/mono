using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Json
{
	class MergedEnumerable<T> : IEnumerable<T>
	{
		IEnumerable<T> l1, l2;

		public MergedEnumerable (IEnumerable<T> l1, IEnumerable<T> l2)
		{
			if (l1 == null)
				throw new ArgumentNullException ("l1");
			if (l2 == null)
				throw new ArgumentNullException ("l2");
			this.l1 = l1;
			this.l2 = l2;
		}

		public IEnumerator<T> GetEnumerator ()
		{
			foreach (T v in l1)
				yield return v;
			foreach (T v in l2)
				yield return v;
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
	}
}
