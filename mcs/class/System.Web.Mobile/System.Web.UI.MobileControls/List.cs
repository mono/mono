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
	}
}
