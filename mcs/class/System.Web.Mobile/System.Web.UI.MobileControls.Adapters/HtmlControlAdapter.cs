/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls.Adapters
 * Class     : HtmlControlAdapter
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System;
using System.Web.Mobile;

namespace System.Web.UI.MobileControls.Adapters
{
	public class HtmlControlAdapter : ControlAdapter
	{
		protected static readonly int NotSecondaryUI = -1;

		[MonoTODO("Whould_like_to_keep_it_FFFFFFFF")]
		internal  const int NotSecondaryUIInitial = 0x7FFFFFFF;

		private static string[] multimediaAttrs = {
			"src",
			"soundstart",
			"loop",
			"volume",
			"vibration",
			"viblength"
		};

		public HtmlControlAdapter()
		{
		}

		protected HtmlFormAdapter FormAdapter
		{
			get
			{
				return (HtmlFormAdapter)Control.Form.Adapter;
			}
		}

		protected HtmlPageAdapter PageAdapter
		{
			get
			{
				return (HtmlPageAdapter)Page.Adapter;
			}
		}

		[MonoTODO]
		protected int SecondaryUIMode
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

		public virtual bool RequiresFormTag
		{
			get
			{
				return false;
			}
		}
	}
}
