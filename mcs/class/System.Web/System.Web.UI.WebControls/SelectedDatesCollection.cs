
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
			} while(local <= toDate);
		}

		internal ArrayList GetDateList ()
		{
			return dateList;
		}
	}
}
