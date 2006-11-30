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
#if NET_2_0

using System.Collections;

namespace System.Windows.Forms.Layout
{
	public class ArrangedElementCollection : IList, ICollection, IEnumerable
	{
		private ArrayList list;

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

		public override bool Equals (object other)
		{
			if (other is ArrangedElementCollection && this == other)
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
		public int Add (object value)
		{
			return list.Add (value);
		}

		public void Clear ()
		{
			list.Clear ();
		}

		public bool Contains (object value)
		{
			return list.Contains (value);
		}

		public int IndexOf (object value)
		{
			return list.IndexOf (value);
		}

		public void Insert (int index, object value)
		{
			list.Insert (index, value);
		}

		public bool IsFixedSize {
			get { return list.IsFixedSize; }
		}

		public void Remove (object value)
		{
			list.Remove (value);
		}

		public void RemoveAt (int index)
		{
			list.RemoveAt (index);
		}

		public object this[int index] {
			get { return list[index]; }
			set { list[index] = value; }
		}
		#endregion

		#region ICollection Members
		public bool IsSynchronized {
			get { return list.IsSynchronized; }
		}

		public object SyncRoot {
			get { return list.IsSynchronized; }
		}
		#endregion
	}
}
#endif