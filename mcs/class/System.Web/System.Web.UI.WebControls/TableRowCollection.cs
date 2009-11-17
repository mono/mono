//
// System.Web.UI.WebControls.TableRowCollection.cs
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
	[Editor ("System.Web.UI.Design.WebControls.TableRowsCollectionEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
#else
	[Editor ("System.Web.UI.Design.WebControls.TableRowsCollectionEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
#endif
	public sealed class TableRowCollection : IList, ICollection, IEnumerable
	{
		ControlCollection cc;
#if NET_2_0
		Table owner;
#endif
		
		internal TableRowCollection (Table table)
		{
			if (table == null)
				throw new ArgumentNullException ("table");
			
			cc = table.Controls;
#if NET_2_0
			owner = table;
#endif
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

		public TableRow this [int index] {
			get { return (TableRow) cc [index]; }
		}

		public object SyncRoot {
			get { return this; }	// as documented
		}

		public int Add (TableRow row)
		{
			if (row == null)
				throw new NullReferenceException (); // .NET compatibility
#if NET_2_0
			if (row.TableRowSectionSet)
				owner.GenerateTableSections = true;
			row.Container = this;
#endif
			int index = cc.IndexOf (row);
			if (index < 0) {
				cc.Add (row);
				index = cc.Count;
			}
			return index;
		}

		public void AddAt (int index, TableRow row)
		{
			if (row == null)
				throw new NullReferenceException (); // .NET compatibility
			
			if (cc.IndexOf (row) < 0) {
#if NET_2_0
				if (row.TableRowSectionSet)
					owner.GenerateTableSections = true;
				row.Container = this;
#endif
				cc.AddAt (index, row);
			}
		}

		public void AddRange (TableRow[] rows)
		{
			foreach (TableRow tr in rows) {
				if (tr == null)
					throw new NullReferenceException (); // .NET compatibility
				
				if (cc.IndexOf (tr) < 0) {
#if NET_2_0
					if (tr.TableRowSectionSet)
						owner.GenerateTableSections = true;
					tr.Container = this;
#endif
					cc.Add (tr);
				}
			}
		}

		public void Clear ()
		{
#if NET_2_0
			owner.GenerateTableSections = false;
#endif
			cc.Clear ();
		}

		public void CopyTo (Array array, int index)
		{
			cc.CopyTo (array, index);
		}

		public IEnumerator GetEnumerator ()
		{
			return cc.GetEnumerator ();
		}

		public int GetRowIndex (TableRow row)
		{
			return cc.IndexOf (row);
		}

#if NET_2_0
		internal void RowTableSectionSet ()
		{
			owner.GenerateTableSections = true;
		}
#endif
		
		public void Remove (TableRow row)
		{
#if NET_2_0
			if (row != null)
				row.Container = null;
#endif
			cc.Remove (row);
		}

		public void RemoveAt (int index)
		{
#if NET_2_0
			TableRow row = this [index] as TableRow;
			if (row != null)
				row.Container = null;
#endif
			
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
			return Add (value as TableRow);
		}

		bool IList.Contains (object value)
		{
			return cc.Contains (value as TableRow);
		}

		int IList.IndexOf (object value)
		{
			return cc.IndexOf (value as TableRow);
		}

		void IList.Insert (int index, object value)
		{
			AddAt (index, value as TableRow);
		}

		void IList.Remove (object value)
		{
			Remove (value as TableRow);
		}
	}
}
