//
// ArrangedElementCollection.cs
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
// Copyright (c) 2006 Jonathan Pobst
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//

using System.Collections;

namespace System.Windows.Forms.Layout
{
	public class ArrangedElementCollection : IList, ICollection, IEnumerable
	{
		internal ArrayList list;

		internal ArrangedElementCollection ()
		{
			this.list = new ArrayList ();
		}

		#region Public Properties
		public virtual int Count { get { return list.Count; } }
		public virtual bool IsReadOnly { get { return list.IsReadOnly; } }
		#endregion

		#region Public Methods
		public void CopyTo (Array array, int index)
		{
			list.CopyTo (array, index);
		}

		public override bool Equals (object obj)
		{
			if (obj is ArrangedElementCollection && this == obj)
				return (true);
			else
				return (false);
		}

		public virtual IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
		#endregion

		#region IList Members
		int IList.Add (object value)
		{
			return Add (value);
		}

		internal int Add (object value)
		{
			return list.Add (value);
		}

		void IList.Clear ()
		{
			Clear ();
		}

		internal void Clear ()
		{
			list.Clear ();
		}

		bool IList.Contains (object value)
		{
			return Contains (value);
		}

		internal bool Contains (object value)
		{
			return list.Contains (value);
		}

		int IList.IndexOf (object value)
		{
			return IndexOf (value);
		}

		internal int IndexOf (object value)
		{
			return list.IndexOf (value);
		}

		void IList.Insert (int index, object value)
		{
			throw new NotSupportedException ();
		}

		internal void Insert (int index, object value)
		{
			list.Insert (index, value);
		}

		bool IList.IsFixedSize {
			get { return this.IsFixedSize; }
		}

		internal bool IsFixedSize {
			get { return list.IsFixedSize; }
		}

		void IList.Remove (object value)
		{
			Remove (value);
		}

		internal void Remove (object value)
		{
			list.Remove (value);
		}

		void IList.RemoveAt (int index)
		{
			list.RemoveAt (index);
		}

		internal void InternalRemoveAt (int index)
		{
			list.RemoveAt (index);
		}

		object IList.this[int index] {
			get { return this[index]; }
			set { this[index] = value; }
		}

		internal object this[int index] {
			get { return list[index]; }
			set { list[index] = value; }
		}
		#endregion

		#region ICollection Members
		bool ICollection.IsSynchronized {
			get { return list.IsSynchronized; }
		}

		object ICollection.SyncRoot {
			get { return list.IsSynchronized; }
		}
		#endregion
	}
}
