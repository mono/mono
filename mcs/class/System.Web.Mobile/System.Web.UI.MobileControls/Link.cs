/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : Link
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System;
using System.Collections;
using System.Web.UI;
using System.Web.Mobile;

namespace System.Web.UI.MobileControls
{
	public class Link : MobileControl, IPostBackEventHandler
	{
		public Link()
		{
		}

		void IPostBackEventHandler.RaisePostBackEvent(string argument)
		{
			MobilePage.ActiveForm = MobilePage.GetForm(argument);
		}

		public override void AddLinkedForms(IList linkedForms)
		{
			string url = NavigateUrl;
			string pref = Constants.FormIDPrefix;
			if(url.StartsWith(pref))
			{
				url = url.Substring(pref.Length);
				Form toAdd = ResolveFormReference(url);
				if(toAdd != null && !toAdd.HasActiveHandler())
					linkedForms.Add(toAdd);
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
