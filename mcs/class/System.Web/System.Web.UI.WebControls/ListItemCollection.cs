/**
 * Namespace: System.Web.UI.WebControls
 * Class:     ListItemCollection
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 *
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Collections;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class ListItemCollection : IList, ICollection, IEnumerable, IStateManager
	{
		private ArrayList items;
		private bool      saveAll;
		private bool      marked;
		
		public ListItemCollection()
		{
			items   = new ArrayList();
			saveAll = false;
			marked  = false;
		}
		
		public int Capacity
		{
			get
			{
				return items.Capacity;
			}
			set
			{
				items.Capacity = value;
			}
		}
		
		public int Count
		{
			get
			{
				return items.Capacity;
			}
		}
		
		public bool IsReadOnly
		{
			get
			{
				return items.IsReadOnly;
			}
		}
		
		public bool IsSynchronized
		{
			get
			{
				return items.IsSynchronized;
			}
		}
		
		public ListItem this[int index]
		{
			get
			{
				return (ListItem)(items[index]);
			}
		}
		
		public object SyncRoot
		{
			get
			{
				return this;
			}
		}
		
		public void Add(ListItem listItem)
		{
			throw new NotImplementedException();
		}
	}
}
