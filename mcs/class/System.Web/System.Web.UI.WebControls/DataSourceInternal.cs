/**
 * Namespace:   System.Web.UI.WebControls
 * Class:       DataSourceInternal
 *
 * Author:      Gaurav Vaish
 * Maintainer:  gvaish_mono@lycos.com
 * Contact:     gvaish_mono@lycos.com
 * Implementation: Yes
 * Status:      100%
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Collections;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	internal class DataSourceInternal : ICollection, IEnumerable
	{
		private int itemCount;

		public DataSourceInternal(int itemCount)
		{
			this.itemCount = itemCount;
		}

		public int Count
		{
			get
			{
				return itemCount;
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

		public object SyncRoot
		{
			get
			{
				return this;
			}
		}

		public void CopyTo(Array array, int index)
		{
			IEnumerator e = GetEnumerator();
			while(e.MoveNext())
			{
				array.SetValue(e.Current, index);
				index++;
			}
		}
		
		public IEnumerator GetEnumerator()
		{
			return new DataSourceEnumeratorInternal(itemCount);
		}
		
		private class DataSourceEnumeratorInternal : IEnumerator
		{
			private int count;
			private int index;
			
			public DataSourceEnumeratorInternal(int count)
			{
				this.count = count;
				this.index = -1;
			}
			
			public bool MoveNext()
			{
				index++;
				return (index < count);
			}
			
			public object Current
			{
				get
				{
					return null;
				}
			}
			
			public void Reset()
			{
				this.index = -1;
			}
		}
	}
}
