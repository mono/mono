//
// System.Web.UI.WebControls.LoginName class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
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
using System.Security.Permissions;

namespace System.Web.UI.WebControls {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[Bindable (false)]
	[DefaultProperty ("FormatString")]
	[Designer ("System.Web.UI.Design.WebControls.LoginNameDesigner," + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class LoginName : WebControl 
	{
		public LoginName ()
		{
		}

		[DefaultValue ("{0}")]
		[Localizable (true)]
		public virtual string FormatString {
			get {
				object o = ViewState ["FormatString"];
				return (o == null) ? "{0}" : (string)o;
			}
			set {
				if (value == null)
					ViewState.Remove ("FormatString");
				else
					ViewState ["FormatString"] = value;
			}
		}
#if NET_4_0
		public override bool SupportsDisabledAttribute {
			get { return RenderingCompatibilityLessThan40; }
		}
#endif
		protected internal override void Render (HtmlTextWriter writer)
		{
			if (!Anonymous) {
				RenderBeginTag (writer);
				RenderContents (writer);
				RenderEndTag (writer);
			}
		}

		public override void RenderBeginTag (HtmlTextWriter writer)
		{
			if (!Anonymous)
				base.RenderBeginTag (writer);
		}

		protected internal override void RenderContents (HtmlTextWriter writer)
		{
			if (!Anonymous) {
				string format = (string) ViewState ["FormatString"];
				if ((format == null) || (format.Length == 0))
					writer.Write (User);
				else
					writer.Write (format, User);
			}
		}

		public override void RenderEndTag (HtmlTextWriter writer)
		{
			if (!Anonymous)
				base.RenderEndTag (writer);
		}

		// private stuff

		bool Anonymous {
			get { return (User.Length == 0); }
		}

		string User {
			get {
				if ((Page == null) || (Page.User == null))
					return String.Empty;
				return Page.User.Identity.Name;
			}
		}
	}
}

#endif
