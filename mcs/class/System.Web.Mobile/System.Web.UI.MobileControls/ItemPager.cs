
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
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : ItemPager
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.Web.Mobile;

namespace System.Web.UI.MobileControls
{
	public class ItemPager
	{
		private MobileControl control;

		private int firstPage;
		private int lastPage;

		private int firstPageItemCount;
		private int fullPageItemCount;
		private int lastPageItemCount;

		public ItemPager()
		{
		}

		public ItemPager(ControlPager pager, MobileControl control,
		                 int itemCount, int itemsPerPage, int itemWeight)
		{
			this.control = control;
			if(itemsPerPage > 0)
			{
				if(itemCount < itemsPerPage)
				{
					firstPageItemCount = itemCount;
					firstPage = pager.GetPage(itemWeight * itemCount);
					lastPage  = firstPage;
				} else
				{
					int ppic = (itemCount - 1)/itemsPerPage + 1;
					firstPageItemCount = itemsPerPage;
					fullPageItemCount  = itemsPerPage;
					lastPageItemCount = ppic - (ppic - 1)*itemsPerPage;
					firstPage = pager.GetPage(itemsPerPage * itemWeight);
					pager.PageCount += (ppic - 1);
					if(ppic > 1)
					{
						pager.RemainingWeight = pager.PageWeight
						                        - (itemsPerPage * itemWeight);
						lastPage = firstPage + ppic - 1;
					}
				}
			} else
			{
				int totalWt = itemWeight * itemCount;
				if(totalWt <= pager.RemainingWeight)
				{
					firstPageItemCount = itemCount;
					firstPage = pager.GetPage(totalWt);
					lastPage  = firstPage;
				} else
				{
					firstPageItemCount = pager.RemainingWeight / itemWeight;
					int rem = itemCount - firstPageItemCount;
					fullPageItemCount  = Math.Max(itemWeight, pager.PageWeight);
					int pages = rem / fullPageItemCount;
					lastPageItemCount = rem % fullPageItemCount;
					firstPage = pager.PageCount;
					pager.PageCount += 1;
					pager.RemainingWeight = pager.PageWeight;
					pager.PageCount += pages;
					pager.RemainingWeight -= lastPageItemCount * itemWeight;
					if(firstPageItemCount == 0)
					{
						firstPage += 1;
						firstPageItemCount = Math.Min(fullPageItemCount,
						                              itemCount);
					}
					if(lastPageItemCount == 0)
					{
						pager.PageCount -= 1;
						lastPageItemCount = Math.Min(fullPageItemCount,
						                             itemCount);
						pager.RemainingWeight = 0;
					}
					lastPage = pager.PageCount;
				}
				control.FirstPage = firstPage;
				control.LastPage  = lastPage;
			}
		}

		public int ItemCount
		{
			get
			{
				return GetItemCount();
			}
		}

		public int ItemIndex
		{
			get
			{
				return GetItemIndex();
			}
		}

		private int GetItemCount()
		{
			int cp = control.Form.CurrentPage;
			int retVal;
			if(cp >= firstPage && cp <= lastPage)
			{
				if(cp == firstPage)
					retVal = firstPageItemCount;
				else if(cp == lastPage)
					retVal = lastPageItemCount;
				else
					retVal = fullPageItemCount;
			} else
			{
				retVal = -1;
			}
			return retVal;
		}

		private int GetItemIndex()
		{
			int cp = control.Form.CurrentPage;
			int retVal;
			if(cp >= firstPage && cp <= lastPage)
			{
				if(cp == firstPage)
					retVal = 0;
				else
				{
					retVal = (cp - firstPage - 1)* fullPageItemCount
					         + firstPageItemCount;
				}
			} else
			{
				retVal = -1;
			}
			return retVal;
		}
	}
}
