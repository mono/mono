/**
 * Namespace: System.Web.UI.WebControls
 * Class:     SelectedDatesCollection
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
	public sealed class SelectedDatesCollection : ICollection, IEnumerable
	{
		ArrayList dateList;

		public SelectedDatesCollection(ArrayList dateList)
		{
			this.dateList = dateList;
		}

		public int Count
		{
			get
			{
				return dateList.Count;
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

		public DateTime this[int index]
		{
			get
			{
				return (DateTime)(dateList[index]);
			}
		}

		public object SyncRoot
		{
			get
			{
				return this;
			}
		}

		public void Add(DateTime date)
		{
			dateList.Add(date);
		}

		public void Clear()
		{
			dateList.Clear();
		}

		public bool Contains(DateTime date)
		{
			return dateList.Contains(date);
		}

		public void CopyTo(Array array, int index)
		{
			foreach(DateTime current in this)
			{
				array.SetValue(current, index++);
			}
		}

		public IEnumerator GetEnumerator()
		{
			return dateList.GetEnumerator();
		}

		public void Remove(DateTime date)
		{
			dateList.Remove(date);
		}

		public void SelectRange(DateTime fromDate, DateTime toDate)
		{
			dateList.Clear();
			//FIXME: Probable bug in MS implementation. It SHOULD NOT
			// clear the list if fromDate > toDate
			if(fromDate > toDate)
			{
				return;
			}
			DateTime local = fromDate;
			do
			{
				dateList.Add(local);
				local = local.AddDays(1);
			} while(local < toDate);
		}

		internal ArrayList GetDateList ()
		{
			return dateList;
		}
	}
}
