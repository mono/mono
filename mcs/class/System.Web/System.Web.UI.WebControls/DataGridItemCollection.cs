/**
 * Namespace: System.Web.UI.WebControls
 * Class:     DataGridItemCollection
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
	public class DataGridItemCollection : ICollection, IEnumerable
	{
		private ArrayList items;
		
		public DataGridItemCollection(ArrayList items)
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
		
		public DataGridItem this[int index]
		{
			get
			{
				return (DataGridItem)(items[index]);
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
			foreach(DataGridItem current in this)
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
