//
// System.Web.UI.ValidatorCollection.cs
//
// Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.

using System;
using System.Collections;

namespace System.Web.UI {

	public sealed class ValidatorCollection : ICollection, IEnumerable
	{

		public ValidatorCollection ()
		{
		}

		public int Count {
			get { return 1; }
		}

		public bool IsReadOnly {
			get { return false; }
		}

		public bool IsSynchronized {
			get { return false; }
		}

		public IValidator this [int index] {
			get { return null; }
		}

		public object SyncRoot {
			get { return null; }
		}

		public void Add (IValidator validator)
		{
		}

		public bool Contains (IValidator validator)
		{
			return false;
		}

		public void CopyTo (Array array, int index)
		{
		}

		public IEnumerator GetEnumerator ()
		{
			return null;
		}

		public void Remove (IValidator validator)
		{
		}
	}
}
