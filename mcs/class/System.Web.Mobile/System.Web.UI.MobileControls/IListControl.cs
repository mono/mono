/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : IListControl
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

namespace System.Web.UI.MobileControls
{
	interface IListControl
	{
		void OnItemDataBind(ListDataBindEventArgs e);

		bool TrackingViewState { get; }
	}
}
