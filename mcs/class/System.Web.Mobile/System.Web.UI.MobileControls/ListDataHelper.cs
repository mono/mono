/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : ListDataHelper
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.Web.UI;
using System.Web.Mobile;

namespace System.Web.UI.MobileControls
{
	class ListDataHelper
	{
		private int dataSrcCount = -1;
		private IListControl parent;
		private StateBag parentViewState;

		public ListDataHelper(IListControl parent, StateBag parentViewState)
		{
			this.parent = parent;
			this.parentViewState = parentViewState;
		}
	}
}
