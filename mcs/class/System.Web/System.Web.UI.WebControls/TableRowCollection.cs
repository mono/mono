
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
/**
 * Namespace: System.Web.UI.WebControls
 * Class:     TableRowCollection
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.ComponentModel;
using System.Web;
using System.Collections;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	[Editor ("System.Web.UI.Design.WebControls.TableRowsCollectionEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
	public sealed class TableRowCollection: IList, ICollection, IEnumerable
	{
		Table owner;

		internal TableRowCollection(Table owner)
		{
			if(owner == null)
			{
				throw new ArgumentNullException();
			}
			this.owner = owner;
		}

		public int Count
		{
			get
			{
				return owner.Controls.Count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public bool IsSynchronized
		{
			get
			{
				return false;
			}
		}

		public TableRow this[int index]
		{
			get
			{
				return (TableRow)owner.Controls[index];
			}
		}

		public object SyncRoot
		{
			get
			{
				return this;
			}
		}

		public int Add(TableRow row)
		{
			AddAt(-1, row);
			return owner.Controls.Count - 1;
		}

		public void AddAt(int index, TableRow row)
		{
			owner.Controls.AddAt(index, row);
		}

		public void AddRange(TableRow[] rows)
		{
			foreach(TableRow row in rows)
			{
				Add(row);
			}
		}

		public void Clear()
		{
			if(owner.HasControls())
			{
				owner.Controls.Clear();
			}
		}

		public void CopyTo(Array array, int index)
		{
			foreach(object current in this)
			{
				array.SetValue(current, index++);
			}
		}

		public int GetRowIndex(TableRow row)
		{
			if(!owner.HasControls())
			{
				return -1;
			}
			return owner.Controls.IndexOf(row);
		}

		public IEnumerator GetEnumerator()
		{
			return owner.Controls.GetEnumerator();
		}

		public void Remove(TableRow row)
		{
			owner.Controls.Remove(row);
		}

		public void RemoveAt(int index)
		{
			owner.Controls.RemoveAt(index);
		}

		int IList.Add(object o)
		{
			return Add((TableRow)o);
		}

		bool IList.Contains(object o)
		{
			return owner.Controls.Contains((TableRow)o);
		}

		int IList.IndexOf(object o)
		{
			return owner.Controls.IndexOf((TableRow)o);
		}

		void IList.Insert(int index, object o)
		{
			owner.Controls.AddAt(index, (TableRow)o);
		}

		void IList.Remove(object o)
		{
			owner.Controls.Remove((TableRow)o);
		}

		bool IList.IsFixedSize
		{
			get
			{
				return false;
			}
		}

		object IList.this[int index]
		{
			get
			{
				return this[index];
			}
			set
			{
				RemoveAt(index);
				AddAt(index, (TableRow)value);
			}
		}
	}
}
