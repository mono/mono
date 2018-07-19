//------------------------------------------------------------------------------
// <copyright file="LoginStatus.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System.Collections;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Security.Permissions;
    using System.Web.Security;
    using System.Web.UI;


    /// <devdoc>
    /// Displays a link or button that allows the user to login or logout of the site.
    /// Shows whether the user is currently logged in.
    /// </devdoc>
    [
    Bindable(false),
    DefaultEvent("LoggingOut"),
    Designer("System.Web.UI.Design.WebControls.LoginStatusDesigner, " + AssemblyRef.SystemDesign),
    ]
    public class LoginStatus : CompositeControl {

        private static readonly object EventLoggingOut = new object();
        private static readonly object EventLoggedOut = new object();

        private LinkButton _logInLinkButton;
        private ImageButton _logInImageButton;
        private LinkButton _logOutLinkButton;
        private ImageButton _logOutImageButton;

        private bool _loggedIn;

        /// <devdoc>
        /// Whether a user is currently logged in.
        /// NOTE: We should not need to save this in ControlState or ViewState.  At one point, we
        /// were using server controls with event handlers to control logging in and logging out.
        /// In that scenario, we needed to save the LoggedIn boolean in ViewState to hookup the
        /// correct event listener on postback.  Currently, we use a hyperlink for logging in and server
        /// controls for logging out, so we should not need to persist this property.  The property is
        /// always set in OnPreRender, and is only used after that point in the lifecycle.
        /// (VSWhidbey 81266)
        /// </devdoc>
        private bool LoggedIn {
            get {
                return _loggedIn;
            }
            set {
                _loggedIn = value;
            }
        }


        /// <devdoc>
        /// The URL of the image to be shown for the login button.
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescription(SR.LoginStatus_LoginImageUrl),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty()
        ]
        public virtual string LoginImageUrl {
            get {
                object obj = ViewState["LoginImageUrl"];
                return (obj == null) ? String.Empty : (string) obj;
            }
            set {
                ViewState["LoginImageUrl"] = value;
            }
        }


        /// <devdoc>
        /// The text to be shown for the login button.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.LoginStatus_DefaultLoginText),
        WebSysDescription(SR.LoginStatus_LoginText)
        ]
        public virtual string LoginText {
            get {
                object obj = ViewState["LoginText"];
                return (obj == null) ? SR.GetString(SR.LoginStatus_DefaultLoginText) : (string) obj;
            }
            set {
                ViewState["LoginText"] = value;
            }
        }


        /// <devdoc>
        /// The action to perform after logging out.
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(LogoutAction.Refresh),
        Themeable(false),
        WebSysDescription(SR.LoginStatus_LogoutAction)
        ]
        public virtual LogoutAction LogoutAction {
            get {
                object obj = ViewState["LogoutAction"];
                return (obj == null) ? LogoutAction.Refresh : (LogoutAction) obj;
            }
            set {
                if (value < LogoutAction.Refresh || value > LogoutAction.RedirectToLoginPage) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["LogoutAction"] = value;
            }
        }


        /// <devdoc>
        /// The URL of the image to be shown for the logout button.
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescription(SR.LoginStatus_LogoutImageUrl),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty()
        ]
        public virtual string LogoutImageUrl {
            get {
                object obj = ViewState["LogoutImageUrl"];
                return (obj == null) ? String.Empty : (string) obj;
            }
            set {
                ViewState["LogoutImageUrl"] = value;
            }
        }


        /// <devdoc>
        /// The URL redirected to after logging out.
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(""),
        WebSysDescription(SR.LoginStatus_LogoutPageUrl),
        Editor("System.Web.UI.Design.UrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        Themeable(false),
        UrlProperty()
        ]
        public virtual string LogoutPageUrl {
            get {
                object obj = ViewState["LogoutPageUrl"];
                return (obj == null) ? String.Empty : (string) obj;
            }
            set {
                ViewState["LogoutPageUrl"] = value;
            }
        }


        /// <devdoc>
        /// The text to be shown for the logout button.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.LoginStatus_DefaultLogoutText),
        WebSysDescription(SR.LoginStatus_LogoutText)
        ]
        public virtual string LogoutText {
            get {
                object obj = ViewState["LogoutText"];
                return (obj == null) ? SR.GetString(SR.LoginStatus_DefaultLogoutText) : (string) obj;
            }
            set {
                ViewState["LogoutText"] = value;
            }
        }

        private string NavigateUrl {
            get {
                if (!DesignMode) {
                    return FormsAuthentication.GetLoginPage(null, true);
                }
                // For the designer to render a hyperlink
                return "url";
            }
        }

        protected override HtmlTextWriterTag TagKey {
            get {
                return HtmlTextWriterTag.A;
            }
        }


        /// <devdoc>
        /// Raised after the user is logged out.
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.LoginStatus_LoggedOut)
        ]
        public event EventHandler LoggedOut {
            add {
                Events.AddHandler(EventLoggedOut, value);
            }
            remove {
                Events.RemoveHandler(EventLoggedOut, value);
            }
        }


        /// <devdoc>
        /// Raised before the user is logged out.
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.LoginStatus_LoggingOut)
        ]
        public event LoginCancelEventHandler LoggingOut {
            add {
                Events.AddHandler(EventLoggingOut, value);
            }
            remove {
                Events.RemoveHandler(EventLoggingOut, value);
            }
        }


        /// <devdoc>
        /// Creates all the child controls that may be rendered.
        /// </devdoc>
        protected internal override void CreateChildControls() {
            Controls.Clear();

            _logInLinkButton = new LinkButton();
            _logInImageButton = new ImageButton();
            _logOutLinkButton = new LinkButton();
            _logOutImageButton = new ImageButton();

            _logInLinkButton.EnableViewState = false;
            _logInImageButton.EnableViewState = false;
            _logOutLinkButton.EnableViewState = false;
            _logOutImageButton.EnableViewState = false;

            // Disable theming of child controls (VSWhidbey 86010)
            _logInLinkButton.EnableTheming = false;
            _logInImageButton.EnableTheming = false;

            _logInLinkButton.CausesValidation = false;
            _logInImageButton.CausesValidation = false;

            _logOutLinkButton.EnableTheming = false;
            _logOutImageButton.EnableTheming = false;

            _logOutLinkButton.CausesValidation = false;
            _logOutImageButton.CausesValidation = false;

            CommandEventHandler handler = new CommandEventHandler(LogoutClicked);
            _logOutLinkButton.Command += handler;
            _logOutImageButton.Command += handler;

            handler = new CommandEventHandler(LoginClicked);
            _logInLinkButton.Command += handler;
            _logInImageButton.Command += handler;

            Controls.Add(_logOutLinkButton);
            Controls.Add(_logOutImageButton);
            Controls.Add(_logInLinkButton);
            Controls.Add(_logInImageButton);
        }

        /// <devdoc>
        /// Logs out and redirects the user when the logout button is clicked.
        /// </devdoc>
        private void LogoutClicked(object Source, CommandEventArgs e) {
            LoginCancelEventArgs cancelEventArgs = new LoginCancelEventArgs();
            OnLoggingOut(cancelEventArgs);
            if (cancelEventArgs.Cancel) {
                return;
            }

            FormsAuthentication.SignOut();
            // BugBug: revert to old behavior after SignOut.
            Page.Response.Clear();
            Page.Response.StatusCode = 200;

            OnLoggedOut(EventArgs.Empty);

            // Redirect the user as appropriate
            switch (LogoutAction) {
                case LogoutAction.RedirectToLoginPage:
                    // We do not want the ReturnUrl in the query string, since this is an information
                    // disclosure.  So we must use this instead of FormsAuthentication.RedirectToLoginPage().
                    // (VSWhidbey 438091)
                    Page.Response.Redirect(FormsAuthentication.LoginUrl, false);
                    break;
                case LogoutAction.Refresh:
                    // If the form method is GET, then we must not include the query string, since
                    // it will cause an infinite redirect loop.  If the form method is POST (or there
                    // is no form), then we must include the query string, since the developer could
                    // be using the query string to drive the logic of their page. (VSWhidbey 304531)
                    if (Page.Form != null && String.Equals(Page.Form.Method, "get", StringComparison.OrdinalIgnoreCase)) {
                        Page.Response.Redirect(Page.Request.ClientFilePath.VirtualPathString, false);
                    }
                    else {
                        Page.Response.Redirect(Page.Request.RawUrl, false);
                    }                    
                    break;
                case LogoutAction.Redirect:
                    string url = LogoutPageUrl;
                    if (!String.IsNullOrEmpty(url)) {
                        url = ResolveClientUrl(url);
                    }
                    else {
                        // Use FormsAuthentication.LoginUrl as a fallback
                        url = FormsAuthentication.LoginUrl;
                    }
                    Page.Response.Redirect(url, false);
                    break;
            }
        }

        private void LoginClicked(object Source, CommandEventArgs e) {
            Page.Response.Redirect(ResolveClientUrl(NavigateUrl), false);
        }


        /// <devdoc>
        /// Raises the LoggedOut event.
        /// </devdoc>
        protected virtual void OnLoggedOut(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventLoggedOut];
            if (handler != null) {
                handler(this, e);
            }
        }


        /// <devdoc>
        /// Raises the LoggingOut event.
        /// </devdoc>
        protected virtual void OnLoggingOut(LoginCancelEventArgs e) {
            LoginCancelEventHandler handler = (LoginCancelEventHandler)Events[EventLoggingOut];
            if (handler != null) {
                handler(this, e);
            }
        }


        /// <devdoc>
        /// Determines whether a user is logged in, and gets the URL of the login page.
        /// </devdoc>
        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            // Must be set in PreRender instead of Render, because Page.Request.IsAuthenticated is not
            // valid at design time.
            LoggedIn = Page.Request.IsAuthenticated;
        }

        protected internal override void Render(HtmlTextWriter writer) {
            RenderContents(writer);
        }


        protected internal override void RenderContents(HtmlTextWriter writer) {
            if (Page != null) {
                Page.VerifyRenderingInServerForm(this);
            }

            SetChildProperties();
            if ((ID != null) && (ID.Length != 0)) {
                // NOTE: Adding the attribute here is somewhat hacky... we're assuming
                //       the next tag that gets rendered is the one that needs to
                //       have the id on it.
                writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
            }

            base.RenderContents(writer);
        }

        /// <devdoc>
        /// Sets the visiblity, style, and other properties of child controls.
        /// </devdoc>
        private void SetChildProperties() {
            EnsureChildControls();

            // Set all buttons to nonvisible, then later set the selected button to visible
            _logInLinkButton.Visible = false;
            _logInImageButton.Visible = false;
            _logOutLinkButton.Visible = false;
            _logOutImageButton.Visible = false;

            WebControl visibleControl = null;
            bool loggedIn = LoggedIn;
            if (loggedIn) {
                string logoutImageUrl = LogoutImageUrl;
                if (logoutImageUrl.Length > 0) {
                    _logOutImageButton.AlternateText = LogoutText;
                    _logOutImageButton.ImageUrl = logoutImageUrl;
                    visibleControl = _logOutImageButton;
                }
                else {
                    _logOutLinkButton.Text = LogoutText;
                    visibleControl = _logOutLinkButton;
                }
            }
            else {
                string loginImageUrl = LoginImageUrl;
                if (loginImageUrl.Length > 0) {
                    _logInImageButton.AlternateText = LoginText;
                    _logInImageButton.ImageUrl = loginImageUrl;
                    visibleControl = _logInImageButton;
                }
                else {
                    _logInLinkButton.Text = LoginText;
                    visibleControl = _logInLinkButton;
                }
            }

            visibleControl.CopyBaseAttributes(this);
            visibleControl.ApplyStyle(ControlStyle);
            visibleControl.Visible = true;
        }


        /// <internalonly/>
        /// <devdoc>
        /// Allows the designer to set the LoggedIn and NavigateUrl properties for proper rendering in the designer.
        /// </devdoc>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        protected override void SetDesignModeState(IDictionary data) {
            if (data != null) {
                object o = data["LoggedIn"];
                if (o != null) {
                    LoggedIn = (bool)o;
                }
            }
        }
    }
}
