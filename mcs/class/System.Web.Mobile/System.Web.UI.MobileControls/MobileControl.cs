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
		private Style style;
		private IControlAdapter adapter;

		protected MobileControl()
		{
		}

		public IControlAdapter Adapter
		{
			get
			{
				IControlAdapter retVal = null;
				if(adapter != null)
					retVal = adapter;
				else if(MobilePage != null)
					retVal = MobilePage.GetControlAdapter(this);
				return retVal;
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

		public virtual bool BreakAfter
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

		public DeviceSpecific DeviceSpecific
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

		public MobilePage MobilePage
		{
			get
			{
				throw new NotImplementedException();
			}
		}
	}
}
