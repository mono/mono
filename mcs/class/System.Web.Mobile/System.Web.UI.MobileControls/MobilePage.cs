/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : MobilePage
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.Web.UI;
using System.Web.Mobile;

namespace System.Web.UI.MobileControls
{
	public class MobilePage : Page
	{
		public MobilePage()
		{
		}

		public virtual IControlAdapter GetControlAdapter(MobileControl control)
		{
			throw new NotImplementedException();
		}

		public Form ActiveForm
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public Form GetForm(string id)
		{
			throw new NotImplementedException();
		}
		
		public IPageAdapter Adapter
		{
			get
			{
				throw new NotImplementedException();
			}
		}
	}
}
