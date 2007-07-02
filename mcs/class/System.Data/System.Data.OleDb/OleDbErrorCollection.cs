//
// System.Data.OleDb.OleDbErrorCollection
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Rodrigo Moya, 2002
// Copyright (C) Tim Coleman, 2002
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

namespace System.Data.OleDb
{
	[ListBindableAttribute ( false)]
	[Serializable]
	public sealed class OleDbErrorCollection : ICollection, IEnumerable
	{
		#region Fields

		ArrayList items;
	
		#endregion // Fields

		#region Constructors

		internal OleDbErrorCollection() {
		}

		#endregion Constructors

		#region Properties 

		public int Count {
			get {
				return items.Count;
			}
		}

		public OleDbError this[int index] {
			get {
				return (OleDbError) items[index];
			}
		}

		object ICollection.SyncRoot {
			get {
				return items.SyncRoot;
			}
		}

		bool ICollection.IsSynchronized {
			get {
				return items.IsSynchronized;
			}
		}

		#endregion // Properties

		#region Methods

		internal void Add (OleDbError error)
		{
			items.Add ((object) error);
		}
		
		public void CopyTo (Array array, int index) 
		{
			if (array == null)
				throw new ArgumentNullException("array");

			if ((index < array.GetLowerBound (0)) || (index > array.GetUpperBound (0)))
				throw new ArgumentOutOfRangeException("index");

			// is the check for IsFixedSize required?
			if ((array.IsFixedSize) || (index + this.Count > array.GetUpperBound (0)))
				throw new ArgumentException("array");

			((OleDbError[]) (items.ToArray ())).CopyTo (array, index);
		}

#if NET_2_0
		public void CopyTo (OleDbError [] array, int index)
		{
			items.CopyTo (array, index);
		}
#endif

		public IEnumerator GetEnumerator ()
		{
			return items.GetEnumerator ();
		}

		#endregion // Methods
	}
}
