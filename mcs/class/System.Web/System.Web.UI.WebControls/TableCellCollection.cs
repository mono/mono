/**
 * Namespace: System.Web.UI.WebControls
 * Class:     TableCellCollection
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
using System.Collections;
using System.Web;
using System.Web.UI;
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	//[Editor("??")]
	public sealed class TableCellCollection: IList, ICollection, IEnumerable
	{
		private TableRow owner;

		internal TableCellCollection(TableRow owner)
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

		public TableCell this[int index]
		{
			get
			{
				return (TableCell)owner.Controls[index];
			}
		}

		public object SyncRoot
		{
			get
			{
				return this;
			}
		}

		public int Add(TableCell cell)
		{
			AddAt(-1, cell);
			return owner.Controls.Count - 1;
		}

		public void AddAt(int index, TableCell cell)
		{
			owner.Controls.AddAt(index, cell);
		}

		public void AddRange(TableCell[] cells)
		{
			foreach(TableCell cell in cells)
			{
				Add(cell);
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
			foreach(object cell in this)
			{
				array.SetValue(cell, index++);
			}
		}

		public int GetCellIndex(TableCell cell)
		{
			if(!owner.HasControls())
			{
				return -1;
			}
			return owner.Controls.IndexOf(cell);
		}

		public IEnumerator GetEnumerator()
		{
			return owner.Controls.GetEnumerator();
		}

		public void Remove(TableCell cell)
		{
			owner.Controls.Remove(cell);
		}

		public void RemoveAt(int index)
		{
			owner.Controls.RemoveAt(index);
		}

		int IList.Add(object o)
		{
			return Add((TableCell)o);
		}

		bool IList.Contains(object o)
		{
			return owner.Controls.Contains((TableCell)o);
		}

		int IList.IndexOf(object o)
		{
			return owner.Controls.IndexOf((TableCell)o);
		}

		void IList.Insert(int index, object o)
		{
			owner.Controls.AddAt(index, (TableCell)o);
		}

		void IList.Remove(object o)
		{
			owner.Controls.Remove((TableCell)o);
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
				AddAt(index, (TableCell)value);
			}
		}
	}
}
