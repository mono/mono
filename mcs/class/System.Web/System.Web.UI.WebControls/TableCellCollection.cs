//
// System.Web.UI.WebControls.TableCellCollection.cs
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.ComponentModel;
using System.Collections;

namespace System.Web.UI.WebControls {

#if NET_2_0
	[Editor ("System.Web.UI.Design.WebControls.TableCellsCollectionEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
#else
	[Editor ("System.Web.UI.Design.WebControls.TableCellsCollectionEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
#endif
	public sealed class TableCellCollection : IList, ICollection, IEnumerable 
	{
		ControlCollection cc;

		internal TableCellCollection (TableRow tr)
		{
			cc = tr.Controls;
		}


		public int Count {
			get { return cc.Count; }
		}

		public bool IsReadOnly {
			get { return false; }	// documented as always false
		}

		public bool IsSynchronized {
			get { return false; }	// documented as always false
		}

		public TableCell this [int index] {
			get { return (TableCell) cc [index]; }
		}

		public object SyncRoot {
			get { return this; }	// as documented
		}


		public int Add (TableCell cell)
		{
			int index = cc.IndexOf (cell);
			if (index < 0) {
				cc.Add (cell);
				index = cc.Count;
			}
			return index;
		}

		public void AddAt (int index, TableCell cell)
		{
			if (cc.IndexOf (cell) < 0)
				cc.AddAt (index, cell);
		}

		public void AddRange (TableCell[] cells)
		{
			foreach (TableCell td in cells) {
				if (cc.IndexOf (td) < 0)
					cc.Add (td);
			}
		}

		public void Clear ()
		{
			cc.Clear ();
		}

		public void CopyTo (Array array, int index)
		{
			cc.CopyTo (array, index);
		}

		public int GetCellIndex (TableCell cell)
		{
			return cc.IndexOf (cell);
		}

		public IEnumerator GetEnumerator ()
		{
			return cc.GetEnumerator ();
		}

		public void Remove (TableCell cell)
		{
			cc.Remove (cell);
		}

		public void RemoveAt (int index)
		{
			cc.RemoveAt (index);
		}


		// implements IList but doesn't make some members public

		bool IList.IsFixedSize {
			get { return false; }
		}

		object IList.this [int index] {
			get { return cc [index]; }
			set {
				cc.AddAt (index, (TableRow)value);
				cc.RemoveAt (index + 1);
			}
		}


		int IList.Add (object value)
		{
			cc.Add ((TableRow)value);
			return cc.IndexOf ((TableRow)value);
		}

		bool IList.Contains (object value)
		{
			return cc.Contains ((TableRow)value);
		}

		int IList.IndexOf (object value)
		{
			return cc.IndexOf ((TableRow)value);
		}

		void IList.Insert (int index, object value)
		{
			cc.AddAt (index, (TableRow)value);
		}

		void IList.Remove (object value)
		{
			cc.Remove ((TableRow)value);
		}
	}
}
