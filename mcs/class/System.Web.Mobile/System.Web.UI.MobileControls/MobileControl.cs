/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : MobileControl
 * Author    : Gaurav Vaish
 *
 * Copyright : 2002 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.Web.UI;
using System.Web.Mobile;

namespace System.Web.UI.MobileControls
{
	public abstract class MobileControl : Control
	{
		protected MobileControl()
		{
		}

		[MonoTODO]
		public IControlAdapter Adapter
		{
			get
			{
				throw new NotImplementedException();
			}
		}
	}
}
