/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : Image
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System;
using System.Web.UI;
using System.Web.Mobile;

namespace System.Web.UI.MobileControls
{
	public class Image : MobileControl, IPostBackEventHandler
	{
		public Image()
		{
		}

		void IPostBackEventHandler.RaisePostBackEvent(string argument)
		{
			MobilePage.ActiveForm = MobilePage.GetForm(argument);
		}

		public string AlternateText
		{
			get
			{
				object o = ViewState["AlternateText"];
				if(o != null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["AlternateText"] = value;
			}
		}

		public string ImageUrl
		{
			get
			{
				object o = ViewState["ImageUrl"];
				if(o != null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["ImageUrl"] = value;
			}
		}

		public string NavigateUrl
		{
			get
			{
				object o = ViewState["NavigateUrl"];
				if(o != null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["NavigateUrl"] = value;
			}
		}

		public string SoftkeyLabel
		{
			get
			{
				object o = ViewState["SoftkeyLabel"];
				if(o != null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["SoftkeyLabel"] = value;
			}
		}
	}
}
