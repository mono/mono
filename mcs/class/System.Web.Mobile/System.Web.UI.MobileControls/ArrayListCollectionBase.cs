/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : ArrayListCollectionBase
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.Collections;

namespace System.Web.UI.MobileControls
{
	public class ArrayListCollectionBase : ICollection, IEnumerable
	{
		private ArrayList items;
		internal ArrayListCollectionBase()
		{
		}

		internal ArrayListCollectionBase(ArrayList items)
		{
			this.items = items;
		}

		public int Count
		{
			get
			{
				return (items == null ? 0 : items.Count);
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return (items == null ? false : items.IsReadOnly);
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

		protected ArrayList Items
		{
			get
			{
				if(items == null)
					items = new ArrayList();
				return items;
			}
			set
			{
				items = value;
			}
		}

		public void CopyTo(Array array, int index)
		{
			Items.CopyTo(array, index);
		}

		public IEnumerator GetEnumerator()
		{
			return Items.GetEnumerator();
		}
	}
}
