//
// System.Web.UI.ValidatorCollection.cs
//
// Authors:
// 	Duncan Mak  (duncan@ximian.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc. (http://www.ximian.com)

using System;
using System.Collections;

namespace System.Web.UI {

	public sealed class ValidatorCollection : ICollection, IEnumerable
	{
		private ArrayList _validators;

		public ValidatorCollection ()
		{
			_validators = new ArrayList ();
		}

		public int Count {
			get { return _validators.Count; }
		}

		public bool IsReadOnly {
			get { return _validators.IsReadOnly; }
		}

		public bool IsSynchronized {
			get { return _validators.IsSynchronized; }
		}

		public IValidator this [int index] {
			get { return (IValidator) _validators [index]; }
		}

		public object SyncRoot {
			get { return this; }
		}

		public void Add (IValidator validator)
		{
			_validators.Add (validator);
		}

		public bool Contains (IValidator validator)
		{
			return _validators.Contains (validator);
		}

		public void CopyTo (Array array, int index)
		{
			_validators.CopyTo (array, index);
		}

		public IEnumerator GetEnumerator ()
		{
			return _validators.GetEnumerator ();
		}

		public void Remove (IValidator validator)
		{
			_validators.Remove (validator);
		}
	}
}
