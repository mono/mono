/**
 * Namespace: System.Web.UI.WebControls
 * Class:     RepeaterItemCollection
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
using System.Collection;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public sealed class RepeaterItemCollection : ICollection, IEnumerable
	{
		private ArrayList items;
		
		public RepeaterItemCollection(ArrayList items)
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
		
		public RepeaterItem this[int index]
		{
			get
			{
				return (RepeaterItem)(items[index]);
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
			foreach(RepeaterItem current in this)
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
