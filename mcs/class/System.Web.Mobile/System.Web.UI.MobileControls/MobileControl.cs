/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : MobileControl
 * Author    : Gaurav Vaish
 *
 * Copyright : 2002 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.Drawing;
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

		public Alignment Alignment
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

		public virtual Color BackColor
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
	}
}
