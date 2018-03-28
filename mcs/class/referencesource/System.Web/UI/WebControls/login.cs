//------------------------------------------------------------------------------
// <copyright file="Login.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing.Design;
    using System.Security.Permissions;
    using System.Web.Configuration;
    using System.Web.Security;
    using System.Web.Util;

    /// <devdoc>
    ///     Displays UI that allows a user to login to the site.  Uses a Membership provider
    ///     or custom authentication logic in the OnAuthenticate event.  UI can be customized
    ///     using control properties or a template.
    /// </devdoc>
    [
    Bindable(false),
    DefaultEvent("Authenticate"),
    Designer("System.Web.UI.Design.WebControls.LoginDesigner, " + AssemblyRef.SystemDesign)
    ]
    public class Login : CompositeControl, IBorderPaddingControl, IRenderOuterTableControl {
        public static readonly string LoginButtonCommandName = "Login";

        private ITemplate _loginTemplate;
        private LoginContainer _templateContainer;
        private string _password;
        private bool _convertingToTemplate = false;
        private bool _renderDesignerRegion = false;

        // Needed for user template feature
        private const string _userNameID = "UserName";
        private const string _passwordID = "Password";
        private const string _rememberMeID = "RememberMe";
        private const string _failureTextID = "FailureText";

        // Needed only for "convert to template" feature, otherwise unnecessary
        private const string _userNameRequiredID = "UserNameRequired";
        private const string _passwordRequiredID = "PasswordRequired";
        private const string _pushButtonID = "LoginButton";
        private const string _imageButtonID = "LoginImageButton";
        private const string _linkButtonID = "LoginLinkButton";
        private const string _passwordRecoveryLinkID = "PasswordRecoveryLink";
        private const string _helpLinkID = "HelpLink";
        private const string _createUserLinkID = "CreateUserLink";

        private const string _failureParameterName = "loginfailure";
        private const ValidatorDisplay _requiredFieldValidatorDisplay = ValidatorDisplay.Static;

        private const int _viewStateArrayLength = 10;
        private Style _loginButtonStyle;
        private TableItemStyle _labelStyle;
        private Style _textBoxStyle;
        private TableItemStyle _hyperLinkStyle;
        private TableItemStyle _instructionTextStyle;
        private TableItemStyle _titleTextStyle;
        private TableItemStyle _checkBoxStyle;
        private TableItemStyle _failureTextStyle;
        private Style _validatorTextStyle;

        private static readonly object EventLoggingIn = new object();
        private static readonly object EventAuthenticate = new object();
        private static readonly object EventLoggedIn = new object();
        private static readonly object EventLoginError = new object();

        [
        WebCategory("Appearance"),
        DefaultValue(1),
        WebSysDescription(SR.Login_BorderPadding),
        SuppressMessage("Microsoft.Security", "CA2119:SealMethodsThatSatisfyPrivateInterfaces",
            Justification = "Interface denotes existence of property, not used for security.")
        ]
        public virtual int BorderPadding {
            get {
                object obj = ViewState["BorderPadding"];
                return (obj == null) ? 1 : (int)obj;
            }
            set {
                if (value < -1) {
                    throw new ArgumentOutOfRangeException("value", SR.GetString(SR.Login_InvalidBorderPadding));
                }
                ViewState["BorderPadding"] = value;
            }
        }


        /// <devdoc>
        ///     Gets the style of the checkbox.
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.Login_CheckBoxStyle)
        ]
        public TableItemStyle CheckBoxStyle {
            get {
                if (_checkBoxStyle == null) {
                    _checkBoxStyle = new TableItemStyle();
                    if (IsTrackingViewState) {
                        ((IStateManager) _checkBoxStyle).TrackViewState();
                    }
                }
                return _checkBoxStyle;
            }
        }


        /// <devdoc>
        ///     Gets or sets the text to be shown for the create user link.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Links"),
        DefaultValue(""),
        WebSysDescription(SR.ChangePassword_CreateUserText)
        ]
        public virtual string CreateUserText {
            get {
                object obj = ViewState["CreateUserText"];
                return (obj == null) ? String.Empty : (string) obj;
            }
            set {
                ViewState["CreateUserText"] = value;
            }
        }

        private bool ConvertingToTemplate {
            get {
                return (DesignMode && _convertingToTemplate);
            }
        }


        /// <devdoc>
        ///     Gets or sets the URL of the create user page.
        /// </devdoc>
        [
        WebCategory("Links"),
        DefaultValue(""),
        WebSysDescription(SR.Login_CreateUserUrl),
        Editor("System.Web.UI.Design.UrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty()
        ]
        public virtual string CreateUserUrl {
            get {
                object obj = ViewState["CreateUserUrl"];
                return (obj == null) ? String.Empty : (string) obj;
            }
            set {
                ViewState["CreateUserUrl"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the URL that the user is directed to upon successful login.
        ///     If DestinationPageUrl is non-null, always redirect the user to this page after
        ///     successful login.  Else, use FormsAuthentication.RedirectFromLoginPage.
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(""),
        WebSysDescription(SR.Login_DestinationPageUrl),
        Editor("System.Web.UI.Design.UrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        Themeable(false),
        UrlProperty()
        ]
        public virtual string DestinationPageUrl {
            get {
                object obj = ViewState["DestinationPageUrl"];
                return (obj == null) ? String.Empty : (string) obj;
            }
            set {
                ViewState["DestinationPageUrl"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets whether the remember me checkbox is displayed.
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(true),
        Themeable(false),
        WebSysDescription(SR.Login_DisplayRememberMe)
        ]
        public virtual bool DisplayRememberMe {
            get {
                object obj = ViewState["DisplayRememberMe"];
                return (obj == null) ? true : (bool) obj;
            }
            set {
                ViewState["DisplayRememberMe"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the text to be shown for the help link.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Links"),
        DefaultValue(""),
        WebSysDescription(SR.ChangePassword_HelpPageText)
        ]
        public virtual string HelpPageText {
            get {
                object obj = ViewState["HelpPageText"];
                return (obj == null) ? String.Empty : (string) obj;
            }
            set {
                ViewState["HelpPageText"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the URL of the help page.
        /// </devdoc>
        [
        WebCategory("Links"),
        DefaultValue(""),
        WebSysDescription(SR.LoginControls_HelpPageUrl),
        Editor("System.Web.UI.Design.UrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty()
        ]
        public virtual string HelpPageUrl {
            get {
                object obj = ViewState["HelpPageUrl"];
                return (obj == null) ? String.Empty : (string) obj;
            }
            set {
                ViewState["HelpPageUrl"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the URL of an icon to be displayed for the create user link.
        /// </devdoc>
        [
        WebCategory("Links"),
        DefaultValue(""),
        WebSysDescription(SR.Login_CreateUserIconUrl),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty()
        ]
        public virtual string CreateUserIconUrl {
            get {
                object obj = ViewState["CreateUserIconUrl"];
                return (obj == null) ? String.Empty : (string) obj;
            }
            set {
                ViewState["CreateUserIconUrl"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the URL of an icon to be displayed for the help link.
        /// </devdoc>
        [
        WebCategory("Links"),
        DefaultValue(""),
        WebSysDescription(SR.Login_HelpPageIconUrl),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty()
        ]
        public virtual string HelpPageIconUrl {
            get {
                object obj = ViewState["HelpPageIconUrl"];
                return (obj == null) ? String.Empty : (string) obj;
            }
            set {
                ViewState["HelpPageIconUrl"] = value;
            }
        }


        /// <devdoc>
        ///     Gets the style of the hyperlinks.
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.WebControl_HyperLinkStyle)
        ]
        public TableItemStyle HyperLinkStyle {
            get {
                if (_hyperLinkStyle == null) {
                    _hyperLinkStyle = new TableItemStyle();
                    if (IsTrackingViewState) {
                        ((IStateManager) _hyperLinkStyle).TrackViewState();
                    }
                }
                return _hyperLinkStyle;
            }
        }


        /// <devdoc>
        ///     Gets or sets the text that is displayed to give instructions.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescription(SR.WebControl_InstructionText)
        ]
        public virtual string InstructionText {
            get {
                object obj = ViewState["InstructionText"];
                return (obj == null) ? String.Empty : (string) obj;
            }
            set {
                ViewState["InstructionText"] = value;
            }
        }


        /// <devdoc>
        ///     Gets the style of the instructions.
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.WebControl_InstructionTextStyle)
        ]
        public TableItemStyle InstructionTextStyle {
            get {
                if (_instructionTextStyle == null) {
                    _instructionTextStyle = new TableItemStyle();
                    if (IsTrackingViewState) {
                        ((IStateManager) _instructionTextStyle).TrackViewState();
                    }
                }
                return _instructionTextStyle;
            }
        }


        /// <devdoc>
        ///     Gets the style of the textbox labels.
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.LoginControls_LabelStyle)
        ]
        public TableItemStyle LabelStyle {
            get {
                if (_labelStyle == null) {
                    _labelStyle = new TableItemStyle();
                    if (IsTrackingViewState) {
                        ((IStateManager) _labelStyle).TrackViewState();
                    }
                }
                return _labelStyle;
            }
        }


        /// <devdoc>
        ///     Gets or sets the template that is used to render the control.  If null, a
        ///     default template is used.
        /// </devdoc>
        [
        Browsable(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(Login))
        ]
        public virtual ITemplate LayoutTemplate {
            get {
                return _loginTemplate;
            }
            set {
                _loginTemplate = value;
                ChildControlsCreated = false;
            }
        }


        /// <devdoc>
        ///     Gets or sets the action to take when a login attempt fails.
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(LoginFailureAction.Refresh),
        Themeable(false),
        WebSysDescription(SR.Login_FailureAction)
        ]
        public virtual LoginFailureAction FailureAction {
            get {
                object obj = ViewState["FailureAction"];
                return (obj == null) ? LoginFailureAction.Refresh : (LoginFailureAction) obj;
            }
            set {
                if (value < LoginFailureAction.Refresh || value > LoginFailureAction.RedirectToLoginPage) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["FailureAction"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the text to be shown when a login attempt fails.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.Login_DefaultFailureText),
        WebSysDescription(SR.Login_FailureText)
        ]
        public virtual string FailureText {
            get {
                object obj = ViewState["FailureText"];
                return (obj == null) ? SR.GetString(SR.Login_DefaultFailureText) : (string) obj;
            }
            set {
                ViewState["FailureText"] = value;
            }
        }


        /// <devdoc>
        ///     Gets the style of the failure text.
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.WebControl_FailureTextStyle)
        ]
        public TableItemStyle FailureTextStyle {
            get {
                if (_failureTextStyle == null) {
                    _failureTextStyle = new ErrorTableItemStyle();
                    if (IsTrackingViewState) {
                        ((IStateManager) _failureTextStyle).TrackViewState();
                    }
                }
                return _failureTextStyle;
            }
        }


        /// <devdoc>
        ///     Gets or sets the URL of an image to be displayed for the submit button.
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescription(SR.Login_LoginButtonImageUrl),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty()
        ]
        public virtual string LoginButtonImageUrl {
            get {
                object obj = ViewState["LoginButtonImageUrl"];
                return (obj == null) ? String.Empty : (string) obj;
            }
            set {
                ViewState["LoginButtonImageUrl"] = value;
            }
        }


        /// <devdoc>
        ///     Gets the style of the submit button.
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.Login_LoginButtonStyle)
        ]
        public Style LoginButtonStyle {
            get {
                if (_loginButtonStyle == null) {
                    _loginButtonStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager) _loginButtonStyle).TrackViewState();
                    }
                }
                return _loginButtonStyle;
            }
        }


        /// <devdoc>
        ///     Gets or sets the text to be shown for the submit button.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.Login_DefaultLoginButtonText),
        WebSysDescription(SR.Login_LoginButtonText)
        ]
        public virtual string LoginButtonText {
            get {
                object obj = ViewState["LoginButtonText"];
                return (obj == null) ? SR.GetString(SR.Login_DefaultLoginButtonText) : (string) obj;
            }
            set {
                ViewState["LoginButtonText"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the type of the submit button.
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(ButtonType.Button),
        WebSysDescription(SR.Login_LoginButtonType)
        ]
        public virtual ButtonType LoginButtonType {
            get {
                object obj = ViewState["LoginButtonType"];
                return (obj == null) ? ButtonType.Button : (ButtonType) obj;
            }
            set {
                if (value < ButtonType.Button || value > ButtonType.Link) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["LoginButtonType"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the general layout of the control.
        /// </devdoc>
        [
        DefaultValue(Orientation.Vertical),
        WebCategory("Layout"),
        WebSysDescription(SR.Login_Orientation)
        ]
        public virtual Orientation Orientation {
            get {
                object obj = ViewState["Orientation"];
                return (obj == null) ? Orientation.Vertical : (Orientation) obj;
            }
            set {
                if (value < Orientation.Horizontal || value > Orientation.Vertical) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["Orientation"] = value;
                ChildControlsCreated = false;
            }
        }


        /// <devdoc>
        ///     Gets or sets the name of the membership provider.  If null or empty, the default provider is used.
        /// </devdoc>
        [
        WebCategory("Data"),
        DefaultValue(""),
        Themeable(false),
        WebSysDescription(SR.MembershipProvider_Name)
        ]
        public virtual string MembershipProvider {
            get {
                object obj = ViewState["MembershipProvider"];
                return (obj == null) ? String.Empty : (string) obj;
            }
            set {
                ViewState["MembershipProvider"] = value;
            }
        }


        /// <devdoc>
        ///     Gets the password entered by the user.
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public virtual string Password {
            get {
                return (_password == null) ? String.Empty : _password;
            }
        }

        private string PasswordInternal {
            get {
                string password = Password;
                if (String.IsNullOrEmpty(password) && _templateContainer != null) {
                    ITextControl passwordTextBox = (ITextControl)_templateContainer.PasswordTextBox;
                    if (passwordTextBox != null && passwordTextBox.Text != null) {
                        return passwordTextBox.Text;
                    }
                }
                return password;
            }
        }

        /// <devdoc>
        ///     Gets or sets the text that identifies the password textbox.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.LoginControls_DefaultPasswordLabelText),
        WebSysDescription(SR.LoginControls_PasswordLabelText)
        ]
        public virtual string PasswordLabelText {
            get {
                object obj = ViewState["PasswordLabelText"];
                return (obj == null) ? SR.GetString(SR.LoginControls_DefaultPasswordLabelText) : (string) obj;
            }
            set {
                ViewState["PasswordLabelText"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the text to be shown for the password recovery link.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Links"),
        DefaultValue(""),
        WebSysDescription(SR.ChangePassword_PasswordRecoveryText)
        ]
        public virtual string PasswordRecoveryText {
            get {
                object obj = ViewState["PasswordRecoveryText"];
                return (obj == null) ? String.Empty : (string) obj;
            }
            set {
                ViewState["PasswordRecoveryText"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the URL of the password recovery page.
        /// </devdoc>
        [
        WebCategory("Links"),
        DefaultValue(""),
        WebSysDescription(SR.Login_PasswordRecoveryUrl),
        Editor("System.Web.UI.Design.UrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty()
        ]
        public virtual string PasswordRecoveryUrl {
            get {
                object obj = ViewState["PasswordRecoveryUrl"];
                return (obj == null) ? String.Empty : (string) obj;
            }
            set {
                ViewState["PasswordRecoveryUrl"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the URL of an image to be displayed for the password recovery link.
        /// </devdoc>
        [
        WebCategory("Links"),
        DefaultValue(""),
        WebSysDescription(SR.Login_PasswordRecoveryIconUrl),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty()
        ]
        public virtual string PasswordRecoveryIconUrl {
            get {
                object obj = ViewState["PasswordRecoveryIconUrl"];
                return (obj == null) ? String.Empty : (string) obj;
            }
            set {
                ViewState["PasswordRecoveryIconUrl"] = value;
            }
        }



        /// <devdoc>
        ///     Gets or sets the text to be shown in the validation summary when the password is empty.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Validation"),
        WebSysDefaultValue(SR.Login_DefaultPasswordRequiredErrorMessage),
        WebSysDescription(SR.Login_PasswordRequiredErrorMessage)
        ]
        public virtual string PasswordRequiredErrorMessage {
            get {
                object obj = ViewState["PasswordRequiredErrorMessage"];
                return (obj == null) ? SR.GetString(SR.Login_DefaultPasswordRequiredErrorMessage) : (string) obj;
            }
            set {
                ViewState["PasswordRequiredErrorMessage"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets whether the remember me checkbox is initially checked.
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(false),
        Themeable(false),
        WebSysDescription(SR.Login_RememberMeSet)
        ]
        public virtual bool RememberMeSet {
            get {
                object obj = ViewState["RememberMeSet"];
                return (obj == null) ? false : (bool) obj;
            }
            set {
                ViewState["RememberMeSet"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the text to be shown for the remember me checkbox.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.Login_DefaultRememberMeText),
        WebSysDescription(SR.Login_RememberMeText)
        ]
        public virtual string RememberMeText {
            get {
                object obj = ViewState["RememberMeText"];
                return (obj == null) ? SR.GetString(SR.Login_DefaultRememberMeText) : (string) obj;
            }
            set {
                ViewState["RememberMeText"] = value;
            }
        }

        [
        WebCategory("Layout"),
        DefaultValue(true),
        WebSysDescription(SR.LoginControls_RenderOuterTable),
        SuppressMessage("Microsoft.Security", "CA2119:SealMethodsThatSatisfyPrivateInterfaces",
            Justification = "Interface denotes existence of property, not used for security.")
        ]
        public virtual bool RenderOuterTable {
            get {
                object obj = ViewState["RenderOuterTable"];
                return (obj == null) ? true : (bool)obj;
            }
            set {
                ViewState["RenderOuterTable"] = value;
            }
        }

        protected override HtmlTextWriterTag TagKey {
            get {
                return HtmlTextWriterTag.Table;
            }
        }

        /// <devdoc>
        ///     Gets the container into which the template is instantiated.
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        private LoginContainer TemplateContainer {
            get {
                EnsureChildControls();
                return _templateContainer;
            }
        }


        /// <devdoc>
        ///     Gets or sets the style of the textboxes.
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.LoginControls_TextBoxStyle)
        ]
        public Style TextBoxStyle {
            get {
                if (_textBoxStyle == null) {
                    _textBoxStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager) _textBoxStyle).TrackViewState();
                    }
                }
                return _textBoxStyle;
            }
        }


        /// <devdoc>
        ///     Gets or sets the layout of the labels in relation to the textboxes.
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(LoginTextLayout.TextOnLeft),
        WebSysDescription(SR.LoginControls_TextLayout)
        ]
        public virtual LoginTextLayout TextLayout {
            get {
                object obj = ViewState["TextLayout"];
                return (obj == null) ? LoginTextLayout.TextOnLeft : (LoginTextLayout) obj;
            }
            set {
                if (value < LoginTextLayout.TextOnLeft || value > LoginTextLayout.TextOnTop) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["TextLayout"] = value;
                ChildControlsCreated = false;
            }
        }


        /// <devdoc>
        ///     Gets or sets the text to be shown for the title.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.Login_DefaultTitleText),
        WebSysDescription(SR.LoginControls_TitleText)
        ]
        public virtual string TitleText {
            get {
                object obj = ViewState["TitleText"];
                return (obj == null) ? SR.GetString(SR.Login_DefaultTitleText) : (string) obj;
            }
            set {
                ViewState["TitleText"] = value;
            }
        }


        /// <devdoc>
        ///     Gets the style of the title.
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.LoginControls_TitleTextStyle)
        ]
        public TableItemStyle TitleTextStyle {
            get {
                if (_titleTextStyle == null) {
                    _titleTextStyle = new TableItemStyle();
                    if (IsTrackingViewState) {
                        ((IStateManager) _titleTextStyle).TrackViewState();
                    }
                }
                return _titleTextStyle;
            }
        }


        /// <devdoc>
        ///     Gets or sets the initial value in the user name textbox.
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescription(SR.UserName_InitialValue)
        ]
        public virtual string UserName {
            get {
                object obj = ViewState["UserName"];
                return (obj == null) ? String.Empty : (string)obj;
            }
            set {
                ViewState["UserName"] = value;
            }
        }

        private string UserNameInternal {
            get {
                string userName = UserName;
                if (String.IsNullOrEmpty(userName) && _templateContainer != null) {
                    ITextControl userNameTextBox = (ITextControl)_templateContainer.UserNameTextBox;
                    if (userNameTextBox != null && userNameTextBox.Text != null) {
                        return userNameTextBox.Text;
                    }
                }
                return userName;
            }
        }

        /// <devdoc>
        ///     Gets or sets the text that identifies the user name textbox.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.Login_DefaultUserNameLabelText),
        WebSysDescription(SR.LoginControls_UserNameLabelText)
        ]
        public virtual string UserNameLabelText {
            get {
                object obj = ViewState["UserNameLabelText"];
                return (obj == null) ? SR.GetString(SR.Login_DefaultUserNameLabelText) : (string) obj;
            }
            set {
                ViewState["UserNameLabelText"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the text to be shown in the validation summary when the user name is empty.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Validation"),
        WebSysDefaultValue(SR.Login_DefaultUserNameRequiredErrorMessage),
        WebSysDescription(SR.ChangePassword_UserNameRequiredErrorMessage)
        ]
        public virtual string UserNameRequiredErrorMessage {
            get {
                object obj = ViewState["UserNameRequiredErrorMessage"];
                return (obj == null) ? SR.GetString(SR.Login_DefaultUserNameRequiredErrorMessage) : (string) obj;
            }
            set {
                ViewState["UserNameRequiredErrorMessage"] = value;
            }
        }

        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.Login_ValidatorTextStyle)
        ]
        public Style ValidatorTextStyle {
            get {
                if (_validatorTextStyle == null) {
                    _validatorTextStyle = new ErrorStyle();
                    if (IsTrackingViewState) {
                        ((IStateManager) _validatorTextStyle).TrackViewState();
                    }
                }
                return _validatorTextStyle;
            }
        }


        /// <devdoc>
        ///     Gets or sets whether the control remains visible when a user is logged in.
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(true),
        Themeable(false),
        WebSysDescription(SR.Login_VisibleWhenLoggedIn)
        ]
        public virtual bool VisibleWhenLoggedIn {
            get {
                object obj = ViewState["VisibleWhenLoggedIn"];
                return (obj == null) ? true : (bool) obj;
            }
            set {
                ViewState["VisibleWhenLoggedIn"] = value;
            }
        }


        /// <devdoc>
        ///     Raised after the user is authenticated.
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.Login_LoggedIn)
        ]
        public event EventHandler LoggedIn {
            add {
                Events.AddHandler(EventLoggedIn, value);
            }
            remove {
                Events.RemoveHandler(EventLoggedIn, value);
            }
        }


        /// <devdoc>
        ///     Raised to authenticate the user.
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.Login_Authenticate)
        ]
        public event AuthenticateEventHandler Authenticate {
            add {
                Events.AddHandler(EventAuthenticate, value);
            }
            remove {
                Events.RemoveHandler(EventAuthenticate, value);
            }
        }


        /// <devdoc>
        ///     Raised before the user is authenticated.
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.Login_LoggingIn)
        ]
        public event LoginCancelEventHandler LoggingIn {
            add {
                Events.AddHandler(EventLoggingIn, value);
            }
            remove {
                Events.RemoveHandler(EventLoggingIn, value);
            }
        }


        /// <devdoc>
        ///     Raised if the authentication fails.
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.Login_LoginError)
        ]
        public event EventHandler LoginError {
            add {
                Events.AddHandler(EventLoginError, value);
            }
            remove {
                Events.RemoveHandler(EventLoginError, value);
            }
        }

        /// <devdoc>
        ///     Attempts to authenticate the user.  Sets auth cookie if successful, else shows failure text in control.
        /// </devdoc>
        private void AttemptLogin() {
            if (Page != null && !Page.IsValid) {
                return;
            }

            LoginCancelEventArgs cancelEventArgs = new LoginCancelEventArgs();
            OnLoggingIn(cancelEventArgs);
            if (cancelEventArgs.Cancel) {
                return;
            }

            AuthenticateEventArgs authenticateEventArgs = new AuthenticateEventArgs();
            OnAuthenticate(authenticateEventArgs);

            if (authenticateEventArgs.Authenticated) {
                System.Web.Security.FormsAuthentication.SetAuthCookie(UserNameInternal, RememberMeSet);

                OnLoggedIn(EventArgs.Empty);

                Page.Response.Redirect(GetRedirectUrl(), false);
            }
            else {
                OnLoginError(EventArgs.Empty);

                if (FailureAction == LoginFailureAction.RedirectToLoginPage) {
                    System.Web.Security.FormsAuthentication.RedirectToLoginPage(_failureParameterName + "=1");
                }

                ITextControl failureTextLabel = (ITextControl) TemplateContainer.FailureTextLabel;
                if (failureTextLabel != null) {
                    failureTextLabel.Text = FailureText;
                }
            }
        }

        private void AuthenticateUsingMembershipProvider(AuthenticateEventArgs e) {
            MembershipProvider provider = LoginUtil.GetProvider(MembershipProvider);
            // ValidateUser() should not throw an exception.
            e.Authenticated = provider.ValidateUser(UserNameInternal, PasswordInternal);
        }


        /// <devdoc>
        ///     Instantiates the template in the template container, and wires up necessary events.
        /// </devdoc>
        protected internal override void CreateChildControls() {
            Controls.Clear();
            // 

            _templateContainer = new LoginContainer(this);
            _templateContainer.RenderDesignerRegion = _renderDesignerRegion;

            ITemplate template = LayoutTemplate;
            if (template == null) {
                // Only disable viewstate if using default template
                _templateContainer.EnableViewState = false;

                // Disable theming if using default template (VSWhidbey 86010)
                _templateContainer.EnableTheming = false;

                template = new LoginTemplate(this);
            }

            template.InstantiateIn(_templateContainer);
            // 
            _templateContainer.Visible = true;
            Controls.Add(_templateContainer);

            // Set the editable child control properties here for two reasons:
            // - So change events will be raised if viewstate is disabled on the child controls
            //   - Viewstate is always disabled for default template, and might be for user template
            // - So the controls render correctly in the designer
            SetEditableChildProperties();

            // 
            IEditableTextControl userNameTextBox = _templateContainer.UserNameTextBox as IEditableTextControl;
            if (userNameTextBox != null) {
                userNameTextBox.TextChanged += new EventHandler(UserNameTextChanged);
            }
            IEditableTextControl passwordTextBox = _templateContainer.PasswordTextBox as IEditableTextControl;
            if (passwordTextBox != null) {
                passwordTextBox.TextChanged += new EventHandler(PasswordTextChanged);
            }
            ICheckBoxControl rememberMeCheckBox = (ICheckBoxControl)_templateContainer.RememberMeCheckBox;
            if (rememberMeCheckBox != null) {
                rememberMeCheckBox.CheckedChanged += new EventHandler(RememberMeCheckedChanged);
            }
        }

        /// <devdoc>
        ///     The Url we should redirect to after successful authentication.
        /// </devdoc>
        private string GetRedirectUrl() {
            if (OnLoginPage()) {
                string returnUrl = FormsAuthentication.GetReturnUrl(false);
                if (!String.IsNullOrEmpty(returnUrl)) {
                    return returnUrl;
                }
                string destinationPageUrl = DestinationPageUrl;
                if (!String.IsNullOrEmpty(destinationPageUrl)) {
                    // Need to call ResolveClientUrl on DestinationPageUrl, since we may be inside a UserControl.
                    return ResolveClientUrl(destinationPageUrl);
                }
                return System.Web.Security.FormsAuthentication.DefaultUrl;
            }
            else {
                string destinationPageUrl = DestinationPageUrl;
                if (!String.IsNullOrEmpty(destinationPageUrl)) {
                    // Need to call ResolveClientUrl on DestinationPageUrl, since we may be inside a UserControl.
                    return ResolveClientUrl(destinationPageUrl);
                }
                // If the form method is GET, then we must not include the query string, since
                // it will cause an infinite redirect loop.  If the form method is POST (or there
                // is no form), then we must include the query string, since the developer could
                // be using the query string to drive the logic of their page. (VSWhidbey 392183)
                if (Page.Form != null && String.Equals(Page.Form.Method, "get", StringComparison.OrdinalIgnoreCase)) {
                    return Page.Request.ClientFilePath.VirtualPathString;
                }
                else {
                    return Page.Request.RawUrl;
                }
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///     Loads a saved state of the <see cref='System.Web.UI.WebControls.Login'/>.
        /// </devdoc>
        protected override void LoadViewState(object savedState) {
            if (savedState == null) {
                base.LoadViewState(null);
            }
            else {
                object[] myState = (object[]) savedState;
                if (myState.Length != _viewStateArrayLength) {
                    throw new ArgumentException(SR.GetString(SR.ViewState_InvalidViewState));
                }

                base.LoadViewState(myState[0]);
                if (myState[1] != null) {
                    ((IStateManager) LoginButtonStyle).LoadViewState(myState[1]);
                }
                if (myState[2] != null) {
                    ((IStateManager) LabelStyle).LoadViewState(myState[2]);
                }
                if (myState[3] != null) {
                    ((IStateManager) TextBoxStyle).LoadViewState(myState[3]);
                }
                if (myState[4] != null) {
                    ((IStateManager) HyperLinkStyle).LoadViewState(myState[4]);
                }
                if (myState[5] != null) {
                    ((IStateManager) InstructionTextStyle).LoadViewState(myState[5]);
                }
                if (myState[6] != null) {
                    ((IStateManager) TitleTextStyle).LoadViewState(myState[6]);
                }
                if (myState[7] != null) {
                    ((IStateManager) CheckBoxStyle).LoadViewState(myState[7]);
                }
                if (myState[8] != null) {
                    ((IStateManager) FailureTextStyle).LoadViewState(myState[8]);
                }
                if (myState[9] != null) {
                    ((IStateManager) ValidatorTextStyle).LoadViewState(myState[9]);
                }
            }
        }


        /// <devdoc>
        ///     Raises the LoggedIn event.
        /// </devdoc>
        protected virtual void OnLoggedIn(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventLoggedIn];
            if (handler != null) {
                handler(this, e);
            }
        }


        /// <devdoc>
        ///     Raises the Authenticate event.
        /// </devdoc>
        protected virtual void OnAuthenticate(AuthenticateEventArgs e) {
            AuthenticateEventHandler handler = (AuthenticateEventHandler)Events[EventAuthenticate];
            if (handler != null) {
                handler(this, e);
            }
            else {
                AuthenticateUsingMembershipProvider(e);
            }
        }


        /// <devdoc>
        ///     Raises the LoggingIn event.
        /// </devdoc>
        protected virtual void OnLoggingIn(LoginCancelEventArgs e) {
            LoginCancelEventHandler handler = (LoginCancelEventHandler)Events[EventLoggingIn];
            if (handler != null) {
                handler(this, e);
            }
        }


        /// <devdoc>
        /// Called when an event is raised by a control inside our template.  Attempts to login
        /// if the event was raised by the submit button.
        /// </devdoc>
        protected override bool OnBubbleEvent(object source, EventArgs e) {
            bool handled = false;
            if (e is CommandEventArgs) {
                CommandEventArgs ce = (CommandEventArgs) e;
                if (String.Equals(ce.CommandName, LoginButtonCommandName, StringComparison.OrdinalIgnoreCase)) {
                    AttemptLogin();
                    handled = true;
                }
            }
            return handled;
        }


        /// <devdoc>
        ///     Raises the LoginError event.
        /// </devdoc>
        protected virtual void OnLoginError(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventLoginError];
            if (handler != null) {
                handler(this, e);
            }
        }

        /// <devdoc>
        ///     Returns true if this control is on the login page.
        /// </devdoc>
        private bool OnLoginPage() {
            return AuthenticationConfig.AccessingLoginPage(Context, System.Web.Security.FormsAuthentication.LoginUrl);
        }



        /// <devdoc>
        ///     Overridden to set the editable child control properteries and hide the control when appropriate.
        /// </devdoc>
        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            // Set the editable child control properties here instead of Render, so they get into viewstate for the user template.
            SetEditableChildProperties();

            TemplateContainer.Visible = (VisibleWhenLoggedIn || !Page.Request.IsAuthenticated || OnLoginPage());
        }

        private void PasswordTextChanged(object source, EventArgs e) {
            _password = ((ITextControl) source).Text;
        }

        /// <devdoc>
        ///     Returns true on the first page load after a redirect from a failed login on another login control.
        /// </devdoc>
        private bool RedirectedFromFailedLogin() {
            bool redirectedFromFailedLogin;

            if (!DesignMode && (Page != null)) {
                redirectedFromFailedLogin =
                    ((!Page.IsPostBack) && (Page.Request.QueryString[_failureParameterName] != null));
            }
            else {
                redirectedFromFailedLogin = false;
            }

            return redirectedFromFailedLogin;
        }

        private void RememberMeCheckedChanged(object source, EventArgs e) {
            RememberMeSet = ((ICheckBoxControl) source).Checked;
        }


        /// <devdoc>
        ///     Adds the ClientID and renders contents, because we don't want the outer <span>.
        /// </devdoc>
        protected internal override void Render(HtmlTextWriter writer) {
            if (Page != null) {
                Page.VerifyRenderingInServerForm(this);
            }

            // Copied from CompositeControl.cs
            if (DesignMode) {
                ChildControlsCreated = false;
                EnsureChildControls();
            }

            if (TemplateContainer.Visible) {
                SetChildProperties();
                RenderContents(writer);
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///     Saves the state of the <see cref='System.Web.UI.WebControls.Login'/>.
        /// </devdoc>
        protected override object SaveViewState() {
            object[] myState = new object[_viewStateArrayLength];

            myState[0] = base.SaveViewState();
            myState[1] = (_loginButtonStyle != null) ? ((IStateManager)_loginButtonStyle).SaveViewState() : null;
            myState[2] = (_labelStyle != null) ? ((IStateManager)_labelStyle).SaveViewState() : null;
            myState[3] = (_textBoxStyle != null) ? ((IStateManager)_textBoxStyle).SaveViewState() : null;
            myState[4] = (_hyperLinkStyle != null) ? ((IStateManager)_hyperLinkStyle).SaveViewState() : null;
            myState[5] =
                (_instructionTextStyle != null) ? ((IStateManager)_instructionTextStyle).SaveViewState() : null;
            myState[6] = (_titleTextStyle != null) ? ((IStateManager)_titleTextStyle).SaveViewState() : null;
            myState[7] = (_checkBoxStyle != null) ? ((IStateManager)_checkBoxStyle).SaveViewState() : null;
            myState[8] =
                (_failureTextStyle != null) ? ((IStateManager)_failureTextStyle).SaveViewState() : null;
            myState[9] = (_validatorTextStyle != null) ? ((IStateManager)_validatorTextStyle).SaveViewState() : null;

            for (int i=0; i < _viewStateArrayLength; i++) {
                if (myState[i] != null) {
                    return myState;
                }
            }

            // More performant to return null than an array of null values
            return null;
        }

        /// <devdoc>
        ///     Internal for access from LoginAdapter
        /// </devdoc>
        internal void SetChildProperties() {
            SetCommonChildProperties();
            if (LayoutTemplate == null) {
                SetDefaultTemplateChildProperties();
            }
        }

        /// <devdoc>
        ///     Sets child control properties that apply to both default and user templates.
        /// </devdoc>
        private void SetCommonChildProperties() {
            LoginContainer container = TemplateContainer;

            // Clear out the tab index/access key so it doesn't get applied to the tables in the container
            Util.CopyBaseAttributesToInnerControl(this, container);

            container.ApplyStyle(ControlStyle);

            ITextControl failureTextLabel = (ITextControl) container.FailureTextLabel;
            string failureText = FailureText;

            if ((failureTextLabel != null) && (failureText.Length > 0) && RedirectedFromFailedLogin()) {
                // Ideally, we would remove the failure parameter from the query string, but this is not easy.
                failureTextLabel.Text = failureText;
            }
        }

        /// <devdoc>
        ///     Sets child control properties that apply only to the default template.
        /// </devdoc>
        private void SetDefaultTemplateChildProperties() {
            LoginContainer container = TemplateContainer;

            // Need to set the BorderPadding on the BorderTable instead of the LayoutTable, since
            // setting it on the LayoutTable would cause all of the controls inside the Login to be
            // separated by the BorderPadding amount.
            container.BorderTable.CellPadding = BorderPadding;
            container.BorderTable.CellSpacing = 0;

            Literal title = container.Title;
            string titleText = TitleText;
            if (titleText.Length > 0) {
                title.Text = titleText;
                if (_titleTextStyle != null) {
                    LoginUtil.SetTableCellStyle(title, TitleTextStyle);
                }
                LoginUtil.SetTableCellVisible(title, true);
            }
            else {
                LoginUtil.SetTableCellVisible(title, false);
            }

            Literal instruction = container.Instruction;
            string instructionText = InstructionText;
            if (instructionText.Length > 0) {
                instruction.Text = instructionText;
                if (_instructionTextStyle != null) {
                    LoginUtil.SetTableCellStyle(instruction, InstructionTextStyle);
                }
                LoginUtil.SetTableCellVisible(instruction, true);
            }
            else {
                LoginUtil.SetTableCellVisible(instruction, false);
            }

            Control userNameLabel = container.UserNameLabel;
            string userNameLabelText = UserNameLabelText;
            if (userNameLabelText.Length > 0) {
                ((ITextControl)userNameLabel).Text = userNameLabelText;
                if (_labelStyle != null) {
                    LoginUtil.SetTableCellStyle(userNameLabel, LabelStyle);
                }
                userNameLabel.Visible = true;
            }
            else {
                // DO NOT make the whole table cell invisible, because in some layouts it must exist for things
                // to align correctly.  Uncommon that this property will be empty anyway.
                userNameLabel.Visible = false;
            }

            WebControl userNameTextBox = (WebControl)container.UserNameTextBox;
            if (_textBoxStyle != null) {
                userNameTextBox.ApplyStyle(TextBoxStyle);
            }
            userNameTextBox.TabIndex = TabIndex;
            userNameTextBox.AccessKey = AccessKey;

            bool enableValidation = true;
            RequiredFieldValidator userNameRequired = container.UserNameRequired;
            userNameRequired.ErrorMessage = UserNameRequiredErrorMessage;
            userNameRequired.ToolTip = UserNameRequiredErrorMessage;
            userNameRequired.Enabled = enableValidation;
            userNameRequired.Visible = enableValidation;
            if (_validatorTextStyle != null) {
                userNameRequired.ApplyStyle(_validatorTextStyle);
            }

            Control passwordLabel = container.PasswordLabel;
            string passwordLabelText = PasswordLabelText;
            if (passwordLabelText.Length > 0) {
                ((ITextControl)passwordLabel).Text = passwordLabelText;
                if (_labelStyle != null) {
                    LoginUtil.SetTableCellStyle(passwordLabel, LabelStyle);
                }
                passwordLabel.Visible = true;
            }
            else {
                // DO NOT make the whole table cell invisible, because in some layouts it must exist for things
                // to align correctly.  Uncommon that this property will be empty anyway.
                passwordLabel.Visible = false;
            }

            WebControl passwordTextBox = (WebControl)container.PasswordTextBox;
            if (_textBoxStyle != null) {
                passwordTextBox.ApplyStyle(TextBoxStyle);
            }
            passwordTextBox.TabIndex = TabIndex;

            RequiredFieldValidator passwordRequired = container.PasswordRequired;
            passwordRequired.ErrorMessage = PasswordRequiredErrorMessage;
            passwordRequired.ToolTip = PasswordRequiredErrorMessage;
            passwordRequired.Enabled = enableValidation;
            passwordRequired.Visible = enableValidation;
            if (_validatorTextStyle != null) {
                passwordRequired.ApplyStyle(_validatorTextStyle);
            }

            CheckBox rememberMeCheckBox = (CheckBox)container.RememberMeCheckBox;
            if (DisplayRememberMe) {
                rememberMeCheckBox.Text = RememberMeText;
                if (_checkBoxStyle != null) {
                    LoginUtil.SetTableCellStyle(rememberMeCheckBox, CheckBoxStyle);
                }
                LoginUtil.SetTableCellVisible(rememberMeCheckBox, true);
            }
            else {
                LoginUtil.SetTableCellVisible(rememberMeCheckBox, false);
            }
            rememberMeCheckBox.TabIndex = TabIndex;

            LinkButton linkButton = container.LinkButton;
            ImageButton imageButton = container.ImageButton;
            Button pushButton = container.PushButton;

            WebControl button = null;
            switch (LoginButtonType) {
                case ButtonType.Link:
                    linkButton.Text = LoginButtonText;
                    button = linkButton;
                    break;
                case ButtonType.Image:
                    imageButton.ImageUrl = LoginButtonImageUrl;
                    imageButton.AlternateText = LoginButtonText;
                    button = imageButton;
                    break;
                case ButtonType.Button:
                    pushButton.Text = LoginButtonText;
                    button = pushButton;
                    break;
            }

            // Set all buttons to nonvisible, then set the selected button to visible
            linkButton.Visible = false;
            imageButton.Visible = false;
            pushButton.Visible = false;
            button.Visible = true;
            button.TabIndex = TabIndex;

            if (_loginButtonStyle != null) {
                button.ApplyStyle(LoginButtonStyle);
            }

            // Link Setup
            Image createUserIcon = container.CreateUserIcon;
            HyperLink createUserLink = container.CreateUserLink;
            LiteralControl createUserLinkSeparator = container.CreateUserLinkSeparator;
            HyperLink passwordRecoveryLink = container.PasswordRecoveryLink;
            Image passwordRecoveryIcon = container.PasswordRecoveryIcon;
            HyperLink helpPageLink = container.HelpPageLink;
            Image helpPageIcon = container.HelpPageIcon;
            LiteralControl helpPageLinkSeparator = container.PasswordRecoveryLinkSeparator;
            string createUserText = CreateUserText;
            string createUserIconUrl = CreateUserIconUrl;
            string passwordRecoveryText = PasswordRecoveryText;
            string passwordRecoveryIconUrl = PasswordRecoveryIconUrl;
            string helpPageText = HelpPageText;
            string helpPageIconUrl = HelpPageIconUrl;
            bool createUserTextVisible = (createUserText.Length > 0);
            bool passwordRecoveryTextVisible = (passwordRecoveryText.Length > 0);
            bool helpPageTextVisible = (helpPageText.Length > 0);
            bool helpPageIconVisible = (helpPageIconUrl.Length > 0);
            bool createUserIconVisible = (createUserIconUrl.Length > 0);
            bool passwordRecoveryIconVisible = (passwordRecoveryIconUrl.Length > 0);
            bool helpPageLineVisible = helpPageTextVisible || helpPageIconVisible;
            bool createUserLineVisible = createUserTextVisible || createUserIconVisible;
            bool passwordRecoveryLineVisible = passwordRecoveryTextVisible || passwordRecoveryIconVisible;

            helpPageLink.Visible = helpPageTextVisible;
            helpPageLinkSeparator.Visible = helpPageLineVisible && (passwordRecoveryLineVisible || createUserLineVisible);
            if (helpPageTextVisible) {
                helpPageLink.Text = helpPageText;
                helpPageLink.NavigateUrl = HelpPageUrl;
                helpPageLink.TabIndex = TabIndex;
            }
            helpPageIcon.Visible = helpPageIconVisible;
            if (helpPageIconVisible) {
                helpPageIcon.ImageUrl = helpPageIconUrl;
                helpPageIcon.AlternateText = HelpPageText;
            }

            createUserLink.Visible = createUserTextVisible;
            createUserLinkSeparator.Visible = (createUserLineVisible && passwordRecoveryLineVisible);
            if (createUserTextVisible) {
                createUserLink.Text = createUserText;
                createUserLink.NavigateUrl = CreateUserUrl;
                createUserLink.TabIndex = TabIndex;
            }
            createUserIcon.Visible = createUserIconVisible;
            if (createUserIconVisible) {
                createUserIcon.ImageUrl = createUserIconUrl;
                createUserIcon.AlternateText = CreateUserText;
            }

            passwordRecoveryLink.Visible = passwordRecoveryTextVisible;
            if (passwordRecoveryTextVisible) {
                passwordRecoveryLink.Text = passwordRecoveryText;
                passwordRecoveryLink.NavigateUrl = PasswordRecoveryUrl;
                passwordRecoveryLink.TabIndex = TabIndex;
            }
            passwordRecoveryIcon.Visible = passwordRecoveryIconVisible;
            if (passwordRecoveryIconVisible) {
                passwordRecoveryIcon.ImageUrl = passwordRecoveryIconUrl;
                passwordRecoveryIcon.AlternateText = PasswordRecoveryText;
            }

            if (createUserLineVisible || passwordRecoveryLineVisible || helpPageLineVisible) {
                if (_hyperLinkStyle != null) {
                    // Apply style except font to table cell, then apply font and forecolor to HyperLinks
                    // VSWhidbey 81289
                    TableItemStyle hyperLinkStyleExceptFont = new TableItemStyle();
                    hyperLinkStyleExceptFont.CopyFrom(HyperLinkStyle);
                    hyperLinkStyleExceptFont.Font.Reset();
                    LoginUtil.SetTableCellStyle(createUserLink, hyperLinkStyleExceptFont);
                    createUserLink.Font.CopyFrom(HyperLinkStyle.Font);
                    createUserLink.ForeColor = HyperLinkStyle.ForeColor;
                    passwordRecoveryLink.Font.CopyFrom(HyperLinkStyle.Font);
                    passwordRecoveryLink.ForeColor = HyperLinkStyle.ForeColor;
                    helpPageLink.Font.CopyFrom(HyperLinkStyle.Font);
                    helpPageLink.ForeColor = HyperLinkStyle.ForeColor;
                }
                LoginUtil.SetTableCellVisible(helpPageLink, true);
            }
            else {
                LoginUtil.SetTableCellVisible(helpPageLink, false);
            }

            Control failureTextLabel = container.FailureTextLabel;
            if (((ITextControl)failureTextLabel).Text.Length > 0) {
                LoginUtil.SetTableCellStyle(failureTextLabel, FailureTextStyle);
                LoginUtil.SetTableCellVisible(failureTextLabel, true);
            }
            else {
                LoginUtil.SetTableCellVisible(failureTextLabel, false);
            }
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        protected override void SetDesignModeState(IDictionary data) {
            if (data != null) {
                object o = data["ConvertToTemplate"];
                if (o != null) {
                    _convertingToTemplate = (bool)o;
                }

                o = data["RegionEditing"];
                if (o != null) {
                    _renderDesignerRegion = (bool)o;
                }
            }
        }

        /// <devdoc>
        ///     Sets the properties of child controls that are editable by the client.
        /// </devdoc>
        private void SetEditableChildProperties() {
            LoginContainer container = TemplateContainer;

            string userName = UserNameInternal;
            if (!String.IsNullOrEmpty(userName)) {
                ITextControl userNameTextBox = (ITextControl) container.UserNameTextBox;
                if (userNameTextBox != null) {
                    userNameTextBox.Text = userName;
                }
            }

            ICheckBoxControl rememberMeCheckBox = (ICheckBoxControl) container.RememberMeCheckBox;
            if (rememberMeCheckBox != null) {
                // Must set visibility of RememberMeCheckBox before its PreRender phase, so it will not
                // register for post data if it is not visible. (VSWhidbey 81284)
                if (LayoutTemplate == null) {
                    LoginUtil.SetTableCellVisible(container.RememberMeCheckBox, DisplayRememberMe);
                }
                rememberMeCheckBox.Checked = RememberMeSet;
            }
        }

        /// <devdoc>
        ///     Marks the starting point to begin tracking and saving changes to the
        ///     control as part of the control viewstate.
        /// </devdoc>
        protected override void TrackViewState() {
            base.TrackViewState();

            if (_loginButtonStyle != null) {
                ((IStateManager) _loginButtonStyle).TrackViewState();
            }
            if (_labelStyle != null) {
                ((IStateManager) _labelStyle).TrackViewState();
            }
            if (_textBoxStyle != null) {
                ((IStateManager) _textBoxStyle).TrackViewState();
            }
            if (_hyperLinkStyle != null) {
                ((IStateManager) _hyperLinkStyle).TrackViewState();
            }
            if (_instructionTextStyle != null) {
                ((IStateManager) _instructionTextStyle).TrackViewState();
            }
            if (_titleTextStyle != null) {
                ((IStateManager) _titleTextStyle).TrackViewState();
            }
            if (_checkBoxStyle != null) {
                ((IStateManager) _checkBoxStyle).TrackViewState();
            }
            if (_failureTextStyle != null) {
                ((IStateManager) _failureTextStyle).TrackViewState();
            }
            if (_validatorTextStyle != null) {
                ((IStateManager) _validatorTextStyle).TrackViewState();
            }
        }

        private void UserNameTextChanged(object source, EventArgs e) {
            UserName = ((ITextControl) source).Text;
        }

        /// <devdoc>
        ///     The default template for the control, used if LayoutTemplate is null.
        /// </devdoc>
        private sealed class LoginTemplate : ITemplate {
            private Login _owner;

            public LoginTemplate(Login owner) {
                _owner = owner;
            }

            /// <devdoc>
            ///     Creates the child controls, sets certain properties (mostly static properties)
            /// </devdoc>
            private void CreateControls(LoginContainer loginContainer) {
                string validationGroup = _owner.UniqueID;

                Literal title = new Literal();
                loginContainer.Title = title;

                Literal instruction = new Literal();
                loginContainer.Instruction = instruction;

                TextBox userNameTextBox = new TextBox();
                // Must explicitly set the ID of controls that raise postback events
                userNameTextBox.ID = _userNameID;
                loginContainer.UserNameTextBox = userNameTextBox;

                LabelLiteral userNameLabel = new LabelLiteral(userNameTextBox);
                loginContainer.UserNameLabel = userNameLabel;

                bool enableValidation = true;
                RequiredFieldValidator userNameRequired = new RequiredFieldValidator();
                userNameRequired.ID = _userNameRequiredID;
                userNameRequired.ValidationGroup = validationGroup;
                userNameRequired.ControlToValidate = userNameTextBox.ID;
                userNameRequired.Display = _requiredFieldValidatorDisplay;
                userNameRequired.Text = SR.GetString(SR.LoginControls_DefaultRequiredFieldValidatorText);
                userNameRequired.Enabled = enableValidation;
                userNameRequired.Visible = enableValidation;
                loginContainer.UserNameRequired = userNameRequired;

                TextBox passwordTextBox = new TextBox();
                // Must explicitly set the ID of controls that raise postback events
                passwordTextBox.ID = _passwordID;
                passwordTextBox.TextMode = TextBoxMode.Password;
                loginContainer.PasswordTextBox = passwordTextBox;

                LabelLiteral passwordLabel = new LabelLiteral(passwordTextBox);
                loginContainer.PasswordLabel = passwordLabel;

                RequiredFieldValidator passwordRequired = new RequiredFieldValidator();
                passwordRequired.ID = _passwordRequiredID;
                passwordRequired.ValidationGroup = validationGroup;
                passwordRequired.ControlToValidate = passwordTextBox.ID;
                passwordRequired.Display = _requiredFieldValidatorDisplay;
                passwordRequired.Text = SR.GetString(SR.LoginControls_DefaultRequiredFieldValidatorText);
                passwordRequired.Enabled = enableValidation;
                passwordRequired.Visible = enableValidation;
                loginContainer.PasswordRequired = passwordRequired;

                CheckBox rememberMeCheckBox = new CheckBox();
                rememberMeCheckBox.ID = _rememberMeID;
                loginContainer.RememberMeCheckBox = rememberMeCheckBox;

                LinkButton linkButton = new LinkButton();
                linkButton.ID = _linkButtonID;
                linkButton.ValidationGroup = validationGroup;
                linkButton.CommandName = LoginButtonCommandName;
                loginContainer.LinkButton = linkButton;

                ImageButton imageButton = new ImageButton();
                imageButton.ID = _imageButtonID;
                imageButton.ValidationGroup = validationGroup;
                imageButton.CommandName = LoginButtonCommandName;
                loginContainer.ImageButton = imageButton;

                Button pushButton = new Button();
                pushButton.ID = _pushButtonID;
                pushButton.ValidationGroup = validationGroup;
                pushButton.CommandName = LoginButtonCommandName;
                loginContainer.PushButton = pushButton;

                HyperLink passwordRecoveryLink = new HyperLink();
                loginContainer.PasswordRecoveryLink = passwordRecoveryLink;

                LiteralControl passwordRecoveryLinkSeparator = new LiteralControl();
                passwordRecoveryLink.ID = _passwordRecoveryLinkID;
                loginContainer.PasswordRecoveryLinkSeparator = passwordRecoveryLinkSeparator;

                HyperLink createUserLink = new HyperLink();
                loginContainer.CreateUserLink = createUserLink;
                createUserLink.ID = _createUserLinkID;

                LiteralControl createUserLinkSeparator = new LiteralControl();
                loginContainer.CreateUserLinkSeparator = createUserLinkSeparator;

                HyperLink helpPageLink = new HyperLink();
                helpPageLink.ID = _helpLinkID;
                loginContainer.HelpPageLink = helpPageLink;

                Literal failureTextLabel = new Literal();
                failureTextLabel.ID = _failureTextID;
                loginContainer.FailureTextLabel = failureTextLabel;

                loginContainer.PasswordRecoveryIcon = new Image();
                loginContainer.HelpPageIcon = new Image();
                loginContainer.CreateUserIcon = new Image();
            }

            /// <devdoc>
            ///     Adds the controls to a table for layout.  Layout depends on Orientation
            ///     and TextLayout properties.
            /// </devdoc>
            private void LayoutControls(LoginContainer loginContainer) {
                Orientation orientation = _owner.Orientation;
                LoginTextLayout textLayout = _owner.TextLayout;

                if ((orientation == Orientation.Vertical) &&
                    (textLayout == LoginTextLayout.TextOnLeft)) {
                    LayoutVerticalTextOnLeft(loginContainer);
                }
                else if ((orientation == Orientation.Vertical) &&
                         (textLayout == LoginTextLayout.TextOnTop)) {
                    LayoutVerticalTextOnTop(loginContainer);
                }
                else if ((orientation == Orientation.Horizontal) &&
                         (textLayout == LoginTextLayout.TextOnLeft)) {
                    LayoutHorizontalTextOnLeft(loginContainer);
                }
                else {
                    LayoutHorizontalTextOnTop(loginContainer);
                }
            }

            private void LayoutHorizontalTextOnLeft(LoginContainer loginContainer) {
                Table table = new Table();
                table.CellPadding = 0;
                TableRow r;
                TableCell c;

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.ColumnSpan = 6;
                c.HorizontalAlign = HorizontalAlign.Center;
                c.Controls.Add(loginContainer.Title);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.ColumnSpan = 6;
                c.HorizontalAlign = HorizontalAlign.Center;
                c.Controls.Add(loginContainer.Instruction);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();

                if (_owner.ConvertingToTemplate) {
                    loginContainer.UserNameLabel.RenderAsLabel = true;
                }

                c.Controls.Add(loginContainer.UserNameLabel);
                r.Cells.Add(c);

                c = new TableCell();
                c.Controls.Add(loginContainer.UserNameTextBox);
                c.Controls.Add(loginContainer.UserNameRequired);
                r.Cells.Add(c);

                c = new TableCell();
                if (_owner.ConvertingToTemplate) {
                    loginContainer.PasswordLabel.RenderAsLabel = true;
                }
                c.Controls.Add(loginContainer.PasswordLabel);
                r.Cells.Add(c);

                c = new TableCell();
                c.Controls.Add(loginContainer.PasswordTextBox);
                c.Controls.Add(loginContainer.PasswordRequired);
                r.Cells.Add(c);

                c = new TableCell();
                c.Controls.Add(loginContainer.RememberMeCheckBox);
                r.Cells.Add(c);

                c = new TableCell();
                c.Controls.Add(loginContainer.LinkButton);
                c.Controls.Add(loginContainer.ImageButton);
                c.Controls.Add(loginContainer.PushButton);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.ColumnSpan = 6;
                c.Controls.Add(loginContainer.FailureTextLabel);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.ColumnSpan = 6;
                c.Controls.Add(loginContainer.CreateUserIcon);
                c.Controls.Add(loginContainer.CreateUserLink);
                loginContainer.CreateUserLinkSeparator.Text = " ";
                c.Controls.Add(loginContainer.CreateUserLinkSeparator);
                c.Controls.Add(loginContainer.PasswordRecoveryIcon);
                c.Controls.Add(loginContainer.PasswordRecoveryLink);
                loginContainer.PasswordRecoveryLinkSeparator.Text = " ";
                c.Controls.Add(loginContainer.PasswordRecoveryLinkSeparator);
                c.Controls.Add(loginContainer.HelpPageIcon);
                c.Controls.Add(loginContainer.HelpPageLink);
                r.Cells.Add(c);
                table.Rows.Add(r);

                Table table2 = LoginUtil.CreateChildTable(_owner.ConvertingToTemplate);
                r = new TableRow();
                c = new TableCell();
                c.Controls.Add(table);
                r.Cells.Add(c);
                table2.Rows.Add(r);

                loginContainer.LayoutTable = table;
                loginContainer.BorderTable = table2;
                loginContainer.Controls.Add(table2);
            }

            private void LayoutHorizontalTextOnTop(LoginContainer loginContainer) {
                Table table = new Table();
                table.CellPadding = 0;
                TableRow r;
                TableCell c;

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.ColumnSpan = 4;
                c.HorizontalAlign = HorizontalAlign.Center;
                c.Controls.Add(loginContainer.Title);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.ColumnSpan = 4;
                c.HorizontalAlign = HorizontalAlign.Center;
                c.Controls.Add(loginContainer.Instruction);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                if (_owner.ConvertingToTemplate) {
                    loginContainer.UserNameLabel.RenderAsLabel = true;
                }
                c.Controls.Add(loginContainer.UserNameLabel);
                r.Cells.Add(c);

                c = new TableCell();
                if (_owner.ConvertingToTemplate) {
                    loginContainer.PasswordLabel.RenderAsLabel = true;
                }
                c.Controls.Add(loginContainer.PasswordLabel);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.Controls.Add(loginContainer.UserNameTextBox);
                c.Controls.Add(loginContainer.UserNameRequired);
                r.Cells.Add(c);

                c = new TableCell();
                c.Controls.Add(loginContainer.PasswordTextBox);
                c.Controls.Add(loginContainer.PasswordRequired);
                r.Cells.Add(c);

                c = new TableCell();
                c.Controls.Add(loginContainer.RememberMeCheckBox);
                r.Cells.Add(c);

                c = new TableCell();
                c.HorizontalAlign = HorizontalAlign.Right;
                c.Controls.Add(loginContainer.LinkButton);
                c.Controls.Add(loginContainer.ImageButton);
                c.Controls.Add(loginContainer.PushButton);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.ColumnSpan = 4;
                c.Controls.Add(loginContainer.FailureTextLabel);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.ColumnSpan = 4;
                c.Controls.Add(loginContainer.CreateUserIcon);
                c.Controls.Add(loginContainer.CreateUserLink);
                loginContainer.CreateUserLinkSeparator.Text = " ";
                c.Controls.Add(loginContainer.CreateUserLinkSeparator);
                c.Controls.Add(loginContainer.PasswordRecoveryIcon);
                c.Controls.Add(loginContainer.PasswordRecoveryLink);
                loginContainer.PasswordRecoveryLinkSeparator.Text = " ";
                c.Controls.Add(loginContainer.PasswordRecoveryLinkSeparator);
                c.Controls.Add(loginContainer.HelpPageIcon);
                c.Controls.Add(loginContainer.HelpPageLink);
                r.Cells.Add(c);
                table.Rows.Add(r);

                Table table2 = LoginUtil.CreateChildTable(_owner.ConvertingToTemplate);
                r = new TableRow();
                c = new TableCell();
                c.Controls.Add(table);
                r.Cells.Add(c);
                table2.Rows.Add(r);

                loginContainer.LayoutTable = table;
                loginContainer.BorderTable = table2;
                loginContainer.Controls.Add(table2);
            }

            private void LayoutVerticalTextOnLeft(LoginContainer loginContainer) {
                Table table = new Table();
                table.CellPadding = 0;
                TableRow r;
                TableCell c;

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.ColumnSpan = 2;
                c.HorizontalAlign = HorizontalAlign.Center;
                c.Controls.Add(loginContainer.Title);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.ColumnSpan = 2;
                c.HorizontalAlign = HorizontalAlign.Center;
                c.Controls.Add(loginContainer.Instruction);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.HorizontalAlign = HorizontalAlign.Right;
                if (_owner.ConvertingToTemplate) {
                    loginContainer.UserNameLabel.RenderAsLabel = true;
                }
                c.Controls.Add(loginContainer.UserNameLabel);
                r.Cells.Add(c);

                c = new TableCell();
                c.Controls.Add(loginContainer.UserNameTextBox);
                c.Controls.Add(loginContainer.UserNameRequired);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.HorizontalAlign = HorizontalAlign.Right;
                if (_owner.ConvertingToTemplate) {
                    loginContainer.PasswordLabel.RenderAsLabel = true;
                }
                c.Controls.Add(loginContainer.PasswordLabel);
                r.Cells.Add(c);

                c = new TableCell();
                c.Controls.Add(loginContainer.PasswordTextBox);
                c.Controls.Add(loginContainer.PasswordRequired);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.ColumnSpan = 2;
                c.Controls.Add(loginContainer.RememberMeCheckBox);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.ColumnSpan = 2;
                c.HorizontalAlign = HorizontalAlign.Center;
                c.Controls.Add(loginContainer.FailureTextLabel);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.ColumnSpan = 2;
                c.HorizontalAlign = HorizontalAlign.Right;
                c.Controls.Add(loginContainer.LinkButton);
                c.Controls.Add(loginContainer.ImageButton);
                c.Controls.Add(loginContainer.PushButton);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.ColumnSpan = 2;
                c.Controls.Add(loginContainer.CreateUserIcon);
                c.Controls.Add(loginContainer.CreateUserLink);
                c.Controls.Add(loginContainer.CreateUserLinkSeparator);
                c.Controls.Add(loginContainer.PasswordRecoveryIcon);
                c.Controls.Add(loginContainer.PasswordRecoveryLink);
                loginContainer.PasswordRecoveryLinkSeparator.Text = "<br />";
                loginContainer.CreateUserLinkSeparator.Text = "<br />";
                c.Controls.Add(loginContainer.PasswordRecoveryLinkSeparator);
                c.Controls.Add(loginContainer.HelpPageIcon);
                c.Controls.Add(loginContainer.HelpPageLink);
                r.Cells.Add(c);
                table.Rows.Add(r);

                Table table2 = LoginUtil.CreateChildTable(_owner.ConvertingToTemplate);
                r = new TableRow();
                c = new TableCell();
                c.Controls.Add(table);
                r.Cells.Add(c);
                table2.Rows.Add(r);

                loginContainer.LayoutTable = table;
                loginContainer.BorderTable = table2;
                loginContainer.Controls.Add(table2);
            }

            private void LayoutVerticalTextOnTop(LoginContainer loginContainer) {
                Table table = new Table();
                table.CellPadding = 0;
                TableRow r;
                TableCell c;

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.HorizontalAlign = HorizontalAlign.Center;
                c.Controls.Add(loginContainer.Title);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.HorizontalAlign = HorizontalAlign.Center;
                c.Controls.Add(loginContainer.Instruction);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                if (_owner.ConvertingToTemplate) {
                    loginContainer.UserNameLabel.RenderAsLabel = true;
                }
                c.Controls.Add(loginContainer.UserNameLabel);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.Controls.Add(loginContainer.UserNameTextBox);
                c.Controls.Add(loginContainer.UserNameRequired);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                if (_owner.ConvertingToTemplate) {
                    loginContainer.PasswordLabel.RenderAsLabel = true;
                }
                c.Controls.Add(loginContainer.PasswordLabel);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.Controls.Add(loginContainer.PasswordTextBox);
                c.Controls.Add(loginContainer.PasswordRequired);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.Controls.Add(loginContainer.RememberMeCheckBox);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.HorizontalAlign = HorizontalAlign.Center;
                c.Controls.Add(loginContainer.FailureTextLabel);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.HorizontalAlign = HorizontalAlign.Right;
                c.Controls.Add(loginContainer.LinkButton);
                c.Controls.Add(loginContainer.ImageButton);
                c.Controls.Add(loginContainer.PushButton);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.Controls.Add(loginContainer.CreateUserIcon);
                c.Controls.Add(loginContainer.CreateUserLink);
                loginContainer.CreateUserLinkSeparator.Text = "<br />";
                c.Controls.Add(loginContainer.CreateUserLinkSeparator);
                c.Controls.Add(loginContainer.PasswordRecoveryIcon);
                c.Controls.Add(loginContainer.PasswordRecoveryLink);
                loginContainer.PasswordRecoveryLinkSeparator.Text = "<br />";
                c.Controls.Add(loginContainer.PasswordRecoveryLinkSeparator);
                c.Controls.Add(loginContainer.HelpPageIcon);
                c.Controls.Add(loginContainer.HelpPageLink);
                r.Cells.Add(c);
                table.Rows.Add(r);

                Table table2 = LoginUtil.CreateChildTable(_owner.ConvertingToTemplate);
                r = new TableRow();
                c = new TableCell();
                c.Controls.Add(table);
                r.Cells.Add(c);
                table2.Rows.Add(r);

                loginContainer.LayoutTable = table;
                loginContainer.BorderTable = table2;
                loginContainer.Controls.Add(table2);
            }

            #region ITemplate implementation
            void ITemplate.InstantiateIn(Control container) {
                LoginContainer loginContainer = (LoginContainer) container;
                CreateControls(loginContainer);
                LayoutControls(loginContainer);
            }
            #endregion
        }

        /// <devdoc>
        ///     Container for the layout template.  Contains properties that reference each child control.
        ///     For the default template, the properties are set when the child controls are created.
        ///     For the user template, the controls are looked up dynamically by ID.  Some controls are required,
        ///     and an exception is thrown if they are missing.  Other controls are optional, and an exception is
        ///     thrown if they have the wrong type.
        ///     Internal instead of private because it must be used by LoginAdapter.
        /// </devdoc>
        internal sealed class LoginContainer : LoginUtil.GenericContainer<Login> {
            private HyperLink _createUserLink;
            private LiteralControl _createUserLinkSeparator;
            private Control _failureTextLabel;
            private HyperLink _helpPageLink;
            private ImageButton _imageButton;
            private Literal _instruction;
            private LinkButton _linkButton;
            private LabelLiteral _passwordLabel;
            private HyperLink _passwordRecoveryLink;
            private LiteralControl _passwordRecoveryLinkSeparator;
            private RequiredFieldValidator _passwordRequired;
            private Control _passwordTextBox;
            private Button _pushButton;
            private Control _rememberMeCheckBox;
            private Literal _title;
            private LabelLiteral _userNameLabel;
            private RequiredFieldValidator _userNameRequired;
            private Control _userNameTextBox;
            private Image _createUserIcon;
            private Image _helpPageIcon;
            private Image _passwordRecoveryIcon;

            public LoginContainer(Login owner) : base(owner) {
            }

            protected override bool ConvertingToTemplate {
                get {
                    return Owner.ConvertingToTemplate;
                }
            }

            internal HyperLink CreateUserLink {
                get {
                    return _createUserLink;
                }
                set {
                    _createUserLink = value;
                }
            }

            internal LiteralControl CreateUserLinkSeparator {
                get {
                    return _createUserLinkSeparator;
                }
                set {
                    _createUserLinkSeparator = value;
                }
            }

            internal Image PasswordRecoveryIcon {
                get {
                    return _passwordRecoveryIcon;
                }
                set {
                    _passwordRecoveryIcon = value;
                }
            }

            internal Image HelpPageIcon {
                get {
                    return _helpPageIcon;
                }
                set {
                    _helpPageIcon = value;
                }
            }

            internal Image CreateUserIcon {
                get {
                    return _createUserIcon;
                }
                set {
                    _createUserIcon = value;
                }
            }

            internal Control FailureTextLabel {
                get {
                    if (_failureTextLabel != null) {
                        return _failureTextLabel;
                    }
                    else {
                        return FindOptionalControl<ITextControl>(_failureTextID);
                    }
                }
                set {
                    _failureTextLabel = value;
                }
            }

            internal HyperLink HelpPageLink {
                get {
                    return _helpPageLink;
                }
                set {
                    _helpPageLink = value;
                }
            }

            internal ImageButton ImageButton {
                get {
                    return _imageButton;
                }
                set {
                    _imageButton = value;
                }
            }

            internal Literal Instruction {
                get {
                    return _instruction;
                }
                set {
                    _instruction = value;
                }
            }

            internal LinkButton LinkButton {
                get {
                    return _linkButton;
                }
                set {
                    _linkButton = value;
                }
            }

            internal LabelLiteral PasswordLabel {
                get {
                    return _passwordLabel;
                }
                set {
                    _passwordLabel = value;
                }
            }

            internal HyperLink PasswordRecoveryLink {
                get {
                    return _passwordRecoveryLink;
                }
                set {
                    _passwordRecoveryLink = value;
                }
            }

            internal LiteralControl PasswordRecoveryLinkSeparator {
                get {
                    return _passwordRecoveryLinkSeparator;
                }
                set {
                    _passwordRecoveryLinkSeparator = value;
                }
            }

            internal RequiredFieldValidator PasswordRequired {
                get {
                    return _passwordRequired;
                }
                set {
                    _passwordRequired = value;
                }
            }

            internal Control PasswordTextBox {
                get {
                    if (_passwordTextBox != null) {
                        return _passwordTextBox;
                    }
                    else {
                        return FindRequiredControl<IEditableTextControl>(_passwordID, SR.Login_NoPasswordTextBox);
                    }
                }
                set {
                    _passwordTextBox = value;
                }
            }

            internal Button PushButton {
                get {
                    return _pushButton;
                }
                set {
                    _pushButton = value;
                }
            }

            internal Control RememberMeCheckBox {
                get {
                    if (_rememberMeCheckBox != null) {
                        return _rememberMeCheckBox;
                    }
                    else {
                        return FindOptionalControl<ICheckBoxControl>(_rememberMeID);
                    }
                }
                set {
                    _rememberMeCheckBox = value;
                }
            }

            internal Literal Title {
                get {
                    return _title;
                }
                set {
                    _title = value;
                }
            }

            internal LabelLiteral UserNameLabel {
                get {
                    return _userNameLabel;
                }
                set {
                    _userNameLabel = value;
                }
            }

            internal RequiredFieldValidator UserNameRequired {
                get {
                    return _userNameRequired;
                }

                set {
                    _userNameRequired = value;
                }
            }

            /// <devdoc>
            ///     Required control, must have type ITextControl
            /// </devdoc>
            internal Control UserNameTextBox {
                get {
                    if (_userNameTextBox != null) {
                        return _userNameTextBox;
                    }
                    else {
                        return FindRequiredControl<IEditableTextControl>(_userNameID, SR.Login_NoUserNameTextBox);
                    }
                }
                set {
                    _userNameTextBox = value;
                }
            }
        }
    }
}

