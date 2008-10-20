//
// System.Web.UI.WebControls.LoginStatus class
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

using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Security.Permissions;
using System.Web.Security;

namespace System.Web.UI.WebControls {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[Bindable (false)]
	[DefaultEvent ("LoggingOut")]
	[Designer ("System.Web.UI.Design.WebControls.LoginStatusDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class LoginStatus : CompositeControl 
	{
		static readonly object loggedOutEvent = new object ();
		static readonly object loggingOutEvent = new object ();

		LinkButton logoutLinkButton;
		ImageButton logoutImageButton;
		LinkButton loginLinkButton;
		ImageButton loginImageButton;

		public LoginStatus ()
		{
		}

		[DefaultValue ("")]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[UrlProperty]
		public virtual string LoginImageUrl {
			get {
				object o = ViewState ["LoginImageUrl"];
				return (o == null) ? String.Empty : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("LoginImageUrl");
				else
					ViewState ["LoginImageUrl"] = value;
			}
		}

		[Localizable (true)]
		public virtual string LoginText {
			get {
				object o = ViewState ["LoginText"];
				return (o == null) ? Locale.GetText ("Login") : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("LoginText");
				else
					ViewState ["LoginText"] = value;
			}
		}

		[DefaultValue (LogoutAction.Refresh)]
		[Themeable (false)]
		public virtual LogoutAction LogoutAction {
			get {
				object o = ViewState ["LogoutAction"];
				return (o == null) ? LogoutAction.Refresh : (LogoutAction) o;
			}
			set {
				if ((value < LogoutAction.Refresh) || (value > LogoutAction.RedirectToLoginPage))
					throw new ArgumentOutOfRangeException ("LogoutAction");
				ViewState ["LogoutAction"] = (int) value;
			}
		}

		[DefaultValue ("")]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[UrlProperty]
		public virtual string LogoutImageUrl {
			get {
				object o = ViewState ["LogoutImageUrl"];
				return (o == null) ? String.Empty : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("LogoutImageUrl");
				else
					ViewState ["LogoutImageUrl"] = value;
			}
		}


		[DefaultValue ("")]
		[Editor ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[Themeable (false)]
		[UrlProperty]
		public virtual string LogoutPageUrl {
			get {
				object o = ViewState ["LogoutPageUrl"];
				return (o == null) ? String.Empty : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("LogoutPageUrl");
				else
					ViewState ["LogoutPageUrl"] = value;
			}
		}

		[Localizable (true)]
		public virtual string LogoutText {
			get {
				object o = ViewState ["LogoutText"];
				return (o == null) ? Locale.GetText ("Logout") : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("LogoutText");
				else
					ViewState ["LogoutText"] = value;
			}
		}

		protected override HtmlTextWriterTag TagKey {
			get { return HtmlTextWriterTag.A; }
		}

		// methods
		protected internal override void CreateChildControls ()
		{
			Controls.Clear ();

			// we create controls for all possibilities
			logoutLinkButton = new LinkButton ();
			logoutLinkButton.CausesValidation = false;
			logoutLinkButton.Command += new CommandEventHandler (LogoutClick);
			logoutImageButton = new ImageButton ();
			logoutImageButton.CausesValidation = false;
			logoutImageButton.Command += new CommandEventHandler (LogoutClick);
			loginLinkButton = new LinkButton ();
			loginLinkButton.CausesValidation = false;
			loginLinkButton.Command += new CommandEventHandler (LoginClick);
			loginImageButton = new ImageButton ();
			loginImageButton.CausesValidation = false;
			loginImageButton.Command += new CommandEventHandler (LoginClick);

			// adds controls at the end (after setting their properties)
			Controls.Add (logoutLinkButton);
			Controls.Add (logoutImageButton);
			Controls.Add (loginLinkButton);
			Controls.Add (loginImageButton);
		}

		protected virtual void OnLoggedOut (EventArgs e)
		{
			// this gets called only if the authentication was successful
			EventHandler loggedOut = (EventHandler) Events [loggedOutEvent];
			if (loggedOut != null)
				loggedOut (this, e);
		}

		protected virtual void OnLoggingOut (LoginCancelEventArgs e)
		{
			// this gets called before OnAuthenticate so we can abort the authentication process
			LoginCancelEventHandler loggingOut = (LoginCancelEventHandler) Events [loggingOutEvent];
			if (loggingOut != null)
				loggingOut (this, e);
		}

		protected internal override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
			// documentation says we select Login*|Logout* here
			// but tests shows that the selection is done even 
			// if OnPreRender is never called
		}

		protected internal override void Render (HtmlTextWriter writer)
		{
			if (writer == null)
				return;

			RenderContents (writer);
		}

		protected internal override void RenderContents (HtmlTextWriter writer)
		{
			if (writer == null)
				return;

			EnsureChildControls ();

			bool authenticated = false;
			if (Page != null) {
				Page.VerifyRenderingInServerForm (this);
				authenticated = Page.Request.IsAuthenticated;
			}

			bool logoutImage = (LogoutImageUrl.Length > 0);
			logoutLinkButton.Visible = authenticated && !logoutImage;
			logoutImageButton.Visible = authenticated && logoutImage;

			bool loginImage = (LoginImageUrl.Length > 0);
			loginLinkButton.Visible = !authenticated && !loginImage;
			loginImageButton.Visible = !authenticated && loginImage;

			if (logoutLinkButton.Visible) {
				logoutLinkButton.Text = LogoutText;
				logoutLinkButton.CssClass = this.CssClass;
				logoutLinkButton.Render (writer);
			} else if (logoutImageButton.Visible) {
				logoutImageButton.AlternateText = LogoutText;
				logoutImageButton.CssClass = this.CssClass;
				logoutImageButton.ImageUrl = LogoutImageUrl;
				writer.AddAttribute(HtmlTextWriterAttribute.Name, logoutImageButton.UniqueID);
				logoutImageButton.Render (writer);
			} else if (loginLinkButton.Visible) {
				loginLinkButton.Text = LoginText;
				loginLinkButton.CssClass = this.CssClass;
				loginLinkButton.Render (writer);
			} else if (loginImageButton.Visible) {
				loginImageButton.AlternateText = LoginText;
				loginImageButton.CssClass = this.CssClass;
				loginImageButton.ImageUrl = LoginImageUrl;
				writer.AddAttribute(HtmlTextWriterAttribute.Name, loginImageButton.UniqueID);
				loginImageButton.Render (writer);
			}
		}

		[MonoTODO ("for design-time usage - no more details available")]
		protected override void SetDesignModeState (IDictionary data)
		{
			base.SetDesignModeState (data);
		}

		// events
		public event EventHandler LoggedOut {
			add { Events.AddHandler (loggedOutEvent, value); }
			remove { Events.RemoveHandler (loggedOutEvent, value); }
		}

		public event LoginCancelEventHandler LoggingOut {
			add { Events.AddHandler (loggingOutEvent, value); }
			remove { Events.RemoveHandler (loggingOutEvent, value); }
		}

		// private stuff
		void LogoutClick (object sender, CommandEventArgs e)
		{
			LoginCancelEventArgs lcea = new LoginCancelEventArgs (false);
			OnLoggingOut (lcea);
			if (lcea.Cancel)
				return;

			FormsAuthentication.SignOut ();
			OnLoggedOut (e);

			switch (LogoutAction) {
			case LogoutAction.Refresh:
				HttpContext.Current.Response.Redirect (Page.Request.Url.AbsoluteUri);
				break;
			case LogoutAction.RedirectToLoginPage:
				FormsAuthentication.RedirectToLoginPage ();
				break;
			case LogoutAction.Redirect:
				string url = LogoutPageUrl;
				if (url.Length == 0)
					url = Page.Request.Url.AbsoluteUri;
				HttpContext.Current.Response.Redirect (url);
				break;
			}
		}

		void LoginClick (object sender, CommandEventArgs e)
		{
			FormsAuthentication.RedirectToLoginPage ();
		}
	}
}

#endif
