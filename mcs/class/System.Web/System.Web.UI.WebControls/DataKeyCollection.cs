/**
 * Namespace: System.Web.UI.WebControls
 * Class:     DataKeyCollection
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
	public sealed class DataKeyCollection : ICollection, IEnumerable
	{
		private ArrayList keys;

		public DataKeyCollection(ArrayList keys)
		{
			this.keys = keys;
		}

		public int Count
		{
			get
			{
				return keys.Count;
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

		public object this[int index]
		{
			get
			{
				return keys[index];
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
			foreach(object current in this)
			{
				array.SetValue(current, index++);
			}
		}

		public IEnumerator GetEnumerator()
		{
			return keys.GetEnumerator();
		}
	}
}
