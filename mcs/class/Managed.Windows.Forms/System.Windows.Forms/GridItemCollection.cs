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
// Copyright (c) 2004-2005 Novell, Inc.
//
// Authors:
//	Jonathan Chambers (jonathan.chambers@ansys.com)
//

// COMPLETE

using System;
using System.Collections;
using System.Windows.Forms.PropertyGridInternal;

namespace System.Windows.Forms
{
	public class GridItemCollection : IEnumerable, ICollection
	{
		#region	Local Variables
		private ArrayList list;
		#endregion	// Local Variables

		#region Public Static Fields
		public static GridItemCollection Empty = new GridItemCollection();
		#endregion	// Public Static Fields

		#region	Constructors
		internal GridItemCollection()
		{
			list = new ArrayList();
		}
		#endregion	// Constructors

		#region Internal Properties and Methods
		internal void Add (GridItem grid_item)
		{
			list.Add (grid_item);
		}

		internal void AddRange (GridItemCollection items)
		{
			foreach (GridItem item in items)
				Add (item);
		}

		internal int IndexOf (GridItem grid_item)
		{

			for (int i=0; i < list.Count; i++)
				if (list[i] == grid_item)
					return i;
			return -1;
		}

		private int IndexOf (string label)
		{
			for (int i=0; i < list.Count; i++)
				if (((GridItem)list[i]).Label == label)
					return i;
			return -1;
		}

		#endregion	// Internal Properties and Methods

		#region	Public Instance Properties
		public int Count {
			get { return list.Count; }
		}

		public GridItem this [int index] {
			get { return (GridItem)list[index]; }
		}

		public GridItem this [string label] {
			get {
				int index = IndexOf (label);
				if (index != -1)
					return (GridItem)list[index];
				return null;
			}
		}
		#endregion	// Public Instance Properties

		#region IEnumerable Members
		public IEnumerator GetEnumerator()
		{
			return new GridItemEnumerator (this);
		}
		#endregion

		#region Enumerator Class
		internal class GridItemEnumerator : IEnumerator{
			int nIndex;
			GridItemCollection collection;

			public GridItemEnumerator(GridItemCollection coll)
			{
				collection = coll;
				nIndex = -1;
			}

			public bool MoveNext ()
			{
				nIndex++;
				return (nIndex < collection.Count);
			}

			public void Reset ()
			{
				nIndex = -1;
			}

			object System.Collections.IEnumerator.Current {
				get {
					return collection [nIndex];
				}
			}
		}
		#endregion

		#region ICollection Members

		bool ICollection.IsSynchronized {
			get {
				return list.IsSynchronized;
			}
		}

		void ICollection.CopyTo(Array dest, int index)
		{
			list.CopyTo (dest, index);
		}

		object ICollection.SyncRoot {
			get {
				return list.SyncRoot;
			}
		}

		#endregion

		internal void Clear ()
		{
			list.Clear ();
		}
	}
}
