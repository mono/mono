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
		private int pageCount;

		public ControlPager(Form form, int pageWeight)
		{
			throw new NotImplementedException();
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
	}
}
