/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : List
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.Web.UI;
using System.Web.Mobile;

namespace System.Web.UI.MobileControls
{
	public class List : PagedControl//, INamingContainer, IListControl,
//	                    ITemplateable, IPostBackEventHandler
	{
		private static readonly object ItemDataBindEvent = new object();
		private static readonly object ItemCommandEvent  = new object();

		public List()
		{
		}

		protected override int InternalItemCount
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		protected override bool OnBubbleEvent(object sender, EventArgs e)
		{
			if(e is ListCommandEventArgs)
			{
				OnItemCommand((ListCommandEventArgs)e);
				return true;
			}
			return false;
		}

		protected override void OnDataBinding(EventArgs e)
		{
			base.OnDataBinding(e);
			throw new NotImplementedException();
		}

		protected void OnItemDataBind(ListDataBindEventArgs e)
		{
			ListDataBindEventHandler ldbeh = (ListDataBindEventHandler)(Events[ItemDataBindEvent]);
			if(ldbeh != null)
				ldbeh(this, e);
		}

		protected virtual void OnItemCommand(ListCommandEventArgs e)
		{
			ListCommandEventHandler lceh = (ListCommandEventHandler)(Events[ItemCommandEvent]);
			if(lceh != null)
				lceh(this, e);
		}
	}
}
