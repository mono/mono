//
// System.Collections.DictionaryBase.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Runtime.InteropServices;

namespace System.Collections {

#if NET_2_0
	[ComVisible(true)]
#endif
	[Serializable]
	public abstract class DictionaryBase : IDictionary, ICollection, IEnumerable {

		Hashtable hashtable;
		
		protected DictionaryBase ()
		{
			hashtable = new Hashtable ();
		}

		public void Clear ()
		{
			OnClear ();
			hashtable.Clear ();
			OnClearComplete ();
		}

		public int Count {
			get {
				return hashtable.Count;
			}
		}

		protected IDictionary Dictionary {
			get {
				return this;
			}
		}

		protected Hashtable InnerHashtable {
			get {
				return hashtable;
			}
		}

		public void CopyTo (Array array, int index)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index must be possitive");
			if (array.Rank > 1)
				throw new ArgumentException ("array is multidimensional");
			int size = array.Length;
			if (index > size)
				throw new ArgumentException ("index is larger than array size");
			if (index + Count > size)
				throw new ArgumentException ("Copy will overlflow array");

			DoCopy (array, index);
		}

		private void DoCopy (Array array, int index)
		{
			foreach (DictionaryEntry de in hashtable)
				array.SetValue (de, index++);
		}

		public IDictionaryEnumerator GetEnumerator ()
		{
			return hashtable.GetEnumerator ();
		}

		protected virtual void OnClear ()
		{
		}

		protected virtual void OnClearComplete ()
		{
		}

		protected virtual object OnGet (object key, object currentValue)
		{
			return currentValue;
		}

		protected virtual void OnInsert (object key, object value)
		{
		}
		
		protected virtual void OnInsertComplete (object key, object value)
		{
		}

		protected virtual void OnSet (object key, object oldValue, object newValue)
		{
		}
		
		protected virtual void OnSetComplete (object key, object oldValue, object newValue)
		{
		}

		protected virtual void OnRemove (object key, object value)
		{
		}
		
		protected virtual void OnRemoveComplete (object key, object value)
		{
		}
		
		protected virtual void OnValidate (object key, object value)
		{
		}

		bool IDictionary.IsFixedSize {
			get {
				return false;
			}
		}

		bool IDictionary.IsReadOnly {
			get {
				return false;
			}
		}

		object IDictionary.this [object key] {
			get {
#if NET_2_0
				object value = hashtable [key];
				OnGet (key, value);
				return value;
#else
				OnGet (key, hashtable [key]);
				return hashtable [key];
#endif
			}
			set {
				OnValidate (key, value);
				object current_value = hashtable [key];
				OnSet (key, current_value, value);
				hashtable [key] = value;
				try {
					OnSetComplete (key, current_value, value);
				} catch {
					hashtable [key] = current_value;
					throw;
				}
			}
		}

		ICollection IDictionary.Keys {
			get {
				return hashtable.Keys;
			}
		}

		ICollection IDictionary.Values {
			get {
				return hashtable.Values;
			}
		}

		void IDictionary.Add (object key, object value)
		{
			OnValidate (key, value);
			OnInsert (key, value);
			hashtable.Add (key, value);
			try {
				OnInsertComplete (key, value);
			} catch {
				hashtable.Remove (key);
				throw;
			}
		}

		void IDictionary.Remove (object key)
		{
#if NET_2_0
			if (!hashtable.Contains (key))
				return;
#endif

			object value = hashtable [key];
			OnValidate (key, value);
			OnRemove (key, value);
			hashtable.Remove (key);
#if NET_2_0
			try {
				OnRemoveComplete (key, value);
			} catch {
				hashtable [key] = value;
				throw;
			}
#else
			OnRemoveComplete (key, value);
#endif
		}

		bool IDictionary.Contains (object key)
		{
			return hashtable.Contains (key);
		}

		bool ICollection.IsSynchronized {
			get {
				return hashtable.IsSynchronized;
			}
		}

		object ICollection.SyncRoot {
			get {
				return hashtable.SyncRoot;
			}
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return hashtable.GetEnumerator ();
		}
	}
}
