
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
 * Class     : ControlPager
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.Web.Mobile;

namespace System.Web.UI.MobileControls
{
	public class ControlPager
	{
		private int pageCount = 0;
		private int maxPage   = -1;
		private int pageWt;
		private int remainingWt = 0;

		private Form form;

		// To ponder: will const be better?
		public static readonly int DefaultWeight = 100;
		public static readonly int UseDefaultWeight = -1;

		public ControlPager(Form form, int pageWeight)
		{
			this.form   = form;
			this.pageWt = pageWeight;
		}

		public int PageCount
		{
			get
			{
				return pageCount;
			}
			set
			{
				pageCount = value;
			}
		}

		public int MaximumPage
		{
			get
			{
				return maxPage;
			}
			set
			{
				maxPage = value;
			}
		}

		public int PageWeight
		{
			get
			{
				return pageWt;
			}
			set
			{
				pageWt = value;
			}
		}

		public int RemainingWeight
		{
			get
			{
				return remainingWt;
			}
			set
			{
				remainingWt = value;
			}
		}

		public ItemPager GetItemPager(MobileControl control, int itemCount,
		                              int itemsPerPage, int itemWeight)
		{
			return new ItemPager(this, control, itemCount,
			                     itemsPerPage, itemWeight);
		}

		public int GetPage(int weight)
		{
			if(weight > remainingWt)
			{
				PageCount += 1;
				RemainingWeight = PageWeight;
			}
			if(remainingWt > weight)
			{
				remainingWt -= weight;
			} else
			{
				remainingWt = 0;
			}
			return PageCount;
		}
	}
}
