//
//  AutoCompleteStringCollection.cs
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
// Copyright (c) 2006 Daniel Nauck
//
// Author:
//      Daniel Nauck    (dna(at)mono-project(dot)de)


using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace System.Windows.Forms
{
	public class AutoCompleteStringCollection : IList, ICollection, IEnumerable
	{
		private ArrayList list = null;

		public AutoCompleteStringCollection ()
		{
			list = new ArrayList ();
		}

		public event CollectionChangeEventHandler CollectionChanged;

		protected void OnCollectionChanged (CollectionChangeEventArgs e)
		{
			if(CollectionChanged == null)
				return;

			CollectionChanged (this, e);
		}

		#region IEnumerable Members

		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		#endregion

		#region ICollection Members

		void ICollection.CopyTo (Array array, int index)
		{
			list.CopyTo (array, index);
		}

		public void CopyTo (string[] array, int index)
		{
			list.CopyTo (array, index);
		}

		public int Count
		{
			get { return list.Count; }
		}

		public bool IsSynchronized
		{
			get { return false; }
		}

		public object SyncRoot
		{
			get { return this; }
		}

		#endregion

		#region IList Members

		int IList.Add (object value)
		{
			return Add ((string)value);
		}

		public int Add (string value)
		{
			int index = list.Add (value);
			OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Add, value));
			return index;
		}

		public void AddRange (string[] value)
		{
			if (value == null)
				throw new ArgumentNullException ("value", "Argument cannot be null!");

			list.AddRange (value);
			OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Refresh, null));
		}

		public void Clear ()
		{
			list.Clear ();
			OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Refresh, null));
		}

		bool IList.Contains (object value)
		{
			return Contains ((string)value);
		}

		public bool Contains (string value)
		{
			return list.Contains (value);
		}

		int IList.IndexOf (object value)
		{
			return IndexOf ((string)value);
		}

		public int IndexOf (string value)
		{
			return list.IndexOf (value);
		}

		void IList.Insert (int index, object value)
		{
			Insert (index, (string)value);
		}

		public void Insert (int index, string value)
		{
			list.Insert (index, value);
			OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Add, value));
		}

		bool IList.IsFixedSize
		{
			get { return false; }
		}

		bool IList.IsReadOnly
		{
			get { return false; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		void IList.Remove (object value)
		{
			Remove((string)value);
		}

		public void Remove (string value)
		{
			list.Remove (value);
			OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Remove, value));
		}

		public void RemoveAt (int index)
		{
			string value = this[index];
			list.RemoveAt (index);
			OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Remove, value));
		}

		object IList.this[int index]
		{
			get { return this[index]; }
			set { this[index] = (string)value; }
		}

		public string this[int index]
		{
			get { return (string)list[index]; }
			set {
				OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Remove, list[index]));
				list[index] = value;
				OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Add, value));
			}
		}
		#endregion
	}
}
