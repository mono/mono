
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
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
