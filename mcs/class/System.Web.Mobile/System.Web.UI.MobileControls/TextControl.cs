/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : TextControl
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.Web.UI;

namespace System.Web.UI.MobileControls
{
	public class TextControl : MobileControl
	{
		public TextControl()
		{
		}

		/*
		 * Document speaks of
		 * public IControlAdapter Adapter { get; }
		 * but this is not available (no such property).
		 * So, I am not keeping it.
		 */

		public string Text
		{
			get
			{
				return InnerText;
			}
			set
			{
				InnerText = value;
			}
		}
	}
}
