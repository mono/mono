//
// System.Data.Odbc.OdbcErrorCollection
//
// Author:
//   Brian Ritchie (brianlritchie@hotmail.com) 
//
// Copyright (C) Brian Ritchie, 2002
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

using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace System.Data.Odbc
{
	[Serializable]
	public sealed class OdbcErrorCollection : ICollection, IEnumerable
	{
		#region Fields

		readonly ArrayList _items = new ArrayList ();
	
		#endregion // Fields

		#region Constructors

		internal OdbcErrorCollection ()
		{
		}

		#endregion Constructors

		#region Properties 

		public int Count {
			get {
				return _items.Count;
			}
		}

		public OdbcError this[int i] {
			get {
				return (OdbcError) _items [i];
			}
		}

		object ICollection.SyncRoot {
			get {
				return _items.SyncRoot;
			}
		}

		bool ICollection.IsSynchronized {
			get {
				return _items.IsSynchronized;
			}
		}

		#endregion // Properties

		#region Methods

		internal void Add (OdbcError error)
		{
			_items.Add ((object) error);
		}
		
		public void CopyTo (Array array, int i)
		{
			if (array == null)
				throw new ArgumentNullException("array");

			if ((i < array.GetLowerBound (0)) || (i > array.GetUpperBound (0)))
				throw new ArgumentOutOfRangeException("index");
		
			// is the check for IsFixedSize required?
			if ((array.IsFixedSize) || (i + this.Count > array.GetUpperBound (0)))
				throw new ArgumentException("array");

			((OdbcError[]) (_items.ToArray ())).CopyTo (array, i);
		}

		public IEnumerator GetEnumerator ()
		{
			return _items.GetEnumerator ();
		}

#if NET_2_0
		public void CopyTo (OdbcError [] array, int i)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			if ((i < array.GetLowerBound (0)) || (i > array.GetUpperBound (0)))
				throw new ArgumentOutOfRangeException ("index");
			((OdbcError[]) (_items.ToArray ())).CopyTo (array, i);
		}
#endif

		#endregion // Methods
	}
}
