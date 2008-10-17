//
// System.Web.UI.HtmlControls.HtmlInputPassword.cs
//
// Author:
//	Chris Toshok  <toshok@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System.ComponentModel;
using System.Collections.Specialized;
using System.Security.Permissions;

namespace System.Web.UI.HtmlControls {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[DefaultEvent ("ServerChange")]
	[ValidationProperty ("Value")]
	[SupportsEventValidation]
	public class HtmlInputPassword : HtmlInputText, IPostBackDataHandler
	{
		public HtmlInputPassword ()
			: base ("password")
		{
		}

		protected override void RenderAttributes (HtmlTextWriter writer)
		{
			// make sure we don't render the password
			Attributes.Remove ("value");

			base.RenderAttributes (writer);
		}

		bool IPostBackDataHandler.LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			string s = postCollection [postDataKey];
			if (Attributes ["value"] != s) {
				Attributes ["value"] = s;
				return true;
			}
			return false;
		}

		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
			// We registered in the base class
			ValidateEvent (UniqueID, String.Empty);
			OnServerChange (EventArgs.Empty);
		}
	}

}
#endif
