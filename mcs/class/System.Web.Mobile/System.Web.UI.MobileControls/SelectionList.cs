/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : SelectionList
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.Collections;
using System.Web.UI;
using System.Web.Mobile;

namespace System.Web.UI.MobileControls
{
	public class SelectionList : MobileControl, IListControl//, IPostBackDataHandler
	{
		ListDataHelper dataHelper;

		private static readonly object ItemDataBindEvent = new object();
		private static readonly object SelectionIndexChangedEvent = new object();

		public SelectionList()
		{
			dataHelper = new ListDataHelper(this, ViewState);
		}
		
		protected virtual void OnItemDataBind(ListDataBindEventArgs e)
		{
			ListDataBindEventHandler hdbeh = Events[ItemDataBindEvent] as ListDataBindEventHandler;
			if(hdbeh != null)
			{
				hdbeh(this, e);
			}
		}
		
		void IListControl.OnItemDataBind(ListDataBindEventArgs e)
		{
			OnItemDataBind(e);
		}
		
		bool IListControl.TrackingViewState
		{
			get
			{
				return IsTrackingViewState;
			}
		}
	}
}
