/**
 * Namespace: System.Web.UI.WebControls
 * Class:     DataListItemCollection
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

namespace System.Web.UI.WebControls
{
	public sealed class DataListItemCollection : ICollection, IEnumerable
	{
		private ArrayList items;

		public DataListItemCollection(ArrayList items)
		{
			this.items = items;
		}

		public int Count
		{
			get
			{
				return items.Count;
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

		public DataListItem this[int index]
		{
			get
			{
				return (DataListItem)(items[index]);
			}
		}

		public object SyncRoot
		{
			get
			{
				return this;
			}
		}

		public void CopyTo(Array array, int index)
		{
			foreach(DataListItem current in this)
			{
				array.SetValue(current, index++);
			}
		}

		public IEnumerator GetEnumerator()
		{
			return items.GetEnumerator();
		}
	}
}
