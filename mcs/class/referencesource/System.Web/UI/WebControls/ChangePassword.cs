//------------------------------------------------------------------------------
// <copyright file="ChangePassword.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Net.Mail;
    using System.Security.Permissions;
    using System.Web.Configuration;
    using System.Web.Management;
    using System.Web.Security;
    using System.Web.UI;

    /// <devdoc>
    ///     Displays UI that allows a user to change his password.  Uses a Membership provider
    ///     or custom authentication logic in the OnAuthenticate event.  UI can be customized
    ///     using control properties or a template.
    /// </devdoc>
    [
    Bindable(false),
    DefaultEvent("ChangedPassword"),
    Designer("System.Web.UI.Design.WebControls.ChangePasswordDesigner, " + AssemblyRef.SystemDesign)
    ]
    public class ChangePassword : CompositeControl, IBorderPaddingControl, INamingContainer, IRenderOuterTableControl {
        public static readonly string ChangePasswordButtonCommandName = "ChangePassword";
        public static readonly string CancelButtonCommandName = "Cancel";
        public static readonly string ContinueButtonCommandName = "Continue";

        private ITemplate _changePasswordTemplate;
        private ChangePasswordContainer _changePasswordContainer;
        private ITemplate _successTemplate;
        private SuccessContainer _successContainer;

        private string _userName;
        private string _password;
        private string _newPassword;
        private string _confirmNewPassword;

        private bool _convertingToTemplate = false;
        private bool _renderDesignerRegion = false;
        private View _currentView = View.ChangePassword;

        // Needed for user template feature
        private const string _userNameID = "UserName";
        private const string _currentPasswordID = "CurrentPassword";
        private const string _newPasswordID = "NewPassword";
        private const string _confirmNewPasswordID = "ConfirmNewPassword";
        private const string _failureTextID = "FailureText";

        // Needed only for "convert to template" feature, otherwise unnecessary
        private const string _userNameRequiredID = "UserNameRequired";
        private const string _currentPasswordRequiredID = "CurrentPasswordRequired";
        private const string _newPasswordRequiredID = "NewPasswordRequired";
        private const string _confirmNewPasswordRequiredID = "ConfirmNewPasswordRequired";
        private const string _newPasswordCompareID = "NewPasswordCompare";
        private const string _newPasswordRegExpID = "NewPasswordRegExp";
        private const string _changePasswordPushButtonID = "ChangePasswordPushButton";
        private const string _changePasswordImageButtonID = "ChangePasswordImageButton";
        private const string _changePasswordLinkButtonID = "ChangePasswordLinkButton";
        private const string _cancelPushButtonID = "CancelPushButton";
        private const string _cancelImageButtonID = "CancelImageButton";
        private const string _cancelLinkButtonID = "CancelLinkButton";
        private const string _continuePushButtonID = "ContinuePushButton";
        private const string _continueImageButtonID = "ContinueImageButton";
        private const string _continueLinkButtonID = "ContinueLinkButton";
        private const string _passwordRecoveryLinkID = "PasswordRecoveryLink";
        private const string _helpLinkID = "HelpLink";
        private const string _createUserLinkID = "CreateUserLink";
        private const string _editProfileLinkID = "EditProfileLink";
        private const string _editProfileSuccessLinkID = "EditProfileLinkSuccess";

        private const string _changePasswordViewContainerID = "ChangePasswordContainerID";
        private const string _successViewContainerID = "SuccessContainerID";

        private const ValidatorDisplay _requiredFieldValidatorDisplay = ValidatorDisplay.Static;
        private const ValidatorDisplay _compareFieldValidatorDisplay = ValidatorDisplay.Dynamic;
        private const ValidatorDisplay _regexpFieldValidatorDisplay = ValidatorDisplay.Dynamic;

        private const string _userNameReplacementKey = "<%\\s*UserName\\s*%>";
        private const string _passwordReplacementKey = "<%\\s*Password\\s*%>";

        private const int _viewStateArrayLength = 14;
        private Style _changePasswordButtonStyle;
        private TableItemStyle _labelStyle;
        private Style _textBoxStyle;
        private TableItemStyle _hyperLinkStyle;
        private TableItemStyle _instructionTextStyle;
        private TableItemStyle _titleTextStyle;
        private TableItemStyle _failureTextStyle;
        private TableItemStyle _successTextStyle;
        private TableItemStyle _passwordHintStyle;
        private Style _cancelButtonStyle;
        private Style _continueButtonStyle;
        private Style _validatorTextStyle;

        private MailDefinition _mailDefinition;

        private Control _validatorRow;
        private Control _passwordHintTableRow;
        private Control _userNameTableRow;

        private static readonly object EventChangePasswordError = new object();
        private static readonly object EventCancelButtonClick = new object();
        private static readonly object EventContinueButtonClick = new object();
        private static readonly object EventChangingPassword = new object();
        private static readonly object EventChangedPassword = new object();
        private static readonly object EventSendingMail = new object();
        private static readonly object EventSendMailError = new object();

        
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
                    throw new ArgumentOutOfRangeException("value", SR.GetString(SR.ChangePassword_InvalidBorderPadding));
                }
                ViewState["BorderPadding"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the URL of an image to be displayed for the cancel button.
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescription(SR.ChangePassword_CancelButtonImageUrl),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty()
        ]
        public virtual string CancelButtonImageUrl {
            get {
                object obj = ViewState["CancelButtonImageUrl"];
                return (obj == null) ? String.Empty : (string) obj;
            }
            set {
                ViewState["CancelButtonImageUrl"] = value;
            }
        }


        /// <devdoc>
        ///     Gets the style of cancel button.
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.ChangePassword_CancelButtonStyle)
        ]
        public Style CancelButtonStyle {
            get {
                if (_cancelButtonStyle == null) {
                    _cancelButtonStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager) _cancelButtonStyle).TrackViewState();
                    }
                }
                return _cancelButtonStyle;
            }
        }


        /// <devdoc>
        ///     Gets or sets the text to be shown for the cancel button.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.ChangePassword_DefaultCancelButtonText),
        WebSysDescription(SR.ChangePassword_CancelButtonText)
        ]
        public virtual string CancelButtonText {
            get {
                object obj = ViewState["CancelButtonText"];
                return (obj == null) ? SR.GetString(SR.ChangePassword_DefaultCancelButtonText) : (string) obj;
            }
            set {
                ViewState["CancelButtonText"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the type of the cancel button.
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(ButtonType.Button),
        WebSysDescription(SR.ChangePassword_CancelButtonType)
        ]
        public virtual ButtonType CancelButtonType {
            get {
                object obj = ViewState["CancelButtonType"];
                return (obj == null) ? ButtonType.Button : (ButtonType) obj;
            }
            set {
                if (value < ButtonType.Button || value > ButtonType.Link) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["CancelButtonType"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the URL of an image to be displayed for the continue button.
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(""),
        WebSysDescription(SR.ChangePassword_CancelDestinationPageUrl),
        Editor("System.Web.UI.Design.UrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        Themeable(false),
        UrlProperty()
        ]
        public virtual string CancelDestinationPageUrl {
            get {
                object obj = ViewState["CancelDestinationPageUrl"];
                return (obj == null) ? String.Empty : (string) obj;
            }
            set {
                ViewState["CancelDestinationPageUrl"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the URL of an image to be displayed for the change password button.
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescription(SR.ChangePassword_ChangePasswordButtonImageUrl),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty()
        ]
        public virtual string ChangePasswordButtonImageUrl {
            get {
                object obj = ViewState["ChangePasswordButtonImageUrl"];
                return (obj == null) ? String.Empty : (string) obj;
            }
            set {
                ViewState["ChangePasswordButtonImageUrl"] = value;
            }
        }


        /// <devdoc>
        ///     Gets the style of change password button.
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.ChangePassword_ChangePasswordButtonStyle)
        ]
        public Style ChangePasswordButtonStyle {
            get {
                if (_changePasswordButtonStyle == null) {
                    _changePasswordButtonStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager) _changePasswordButtonStyle).TrackViewState();
                    }
                }
                return _changePasswordButtonStyle;
            }
        }


        /// <devdoc>
        ///     Gets or sets the text to be shown for the change password button.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.ChangePassword_DefaultChangePasswordButtonText),
        WebSysDescription(SR.ChangePassword_ChangePasswordButtonText)
        ]
        public virtual string ChangePasswordButtonText {
            get {
                object obj = ViewState["ChangePasswordButtonText"];
                return (obj == null) ? SR.GetString(SR.ChangePassword_DefaultChangePasswordButtonText) : (string) obj;
            }
            set {
                ViewState["ChangePasswordButtonText"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the type of the create user button.
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(ButtonType.Button),
        WebSysDescription(SR.ChangePassword_ChangePasswordButtonType)
        ]
        public virtual ButtonType ChangePasswordButtonType {
            get {
                object obj = ViewState["ChangePasswordButtonType"];
                return (obj == null) ? ButtonType.Button : (ButtonType) obj;
            }
            set {
                if (value < ButtonType.Button || value > ButtonType.Link) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["ChangePasswordButtonType"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the text to be shown when a change password attempt fails.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.ChangePassword_DefaultChangePasswordFailureText),
        WebSysDescription(SR.ChangePassword_ChangePasswordFailureText)
        ]
        public virtual string ChangePasswordFailureText {
            get {
                object obj = ViewState["ChangePasswordFailureText"];
                return (obj == null) ? SR.GetString(SR.ChangePassword_DefaultChangePasswordFailureText) : (string) obj;
            }
            set {
                ViewState["ChangePasswordFailureText"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the template that is used to render the control.  If null, a
        ///     default template is used.
        /// </devdoc>
        [
        Browsable(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(ChangePassword))
        ]
        public virtual ITemplate ChangePasswordTemplate {
            get {
                return _changePasswordTemplate;
            }
            set {
                _changePasswordTemplate = value;
                ChildControlsCreated = false;
            }
        }

        /// <devdoc>
        ///     Gets the container into which the template is instantiated.
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public Control ChangePasswordTemplateContainer {
            get {
                EnsureChildControls();
                return _changePasswordContainer;
            }
        }


        /// <devdoc>
        ///     Gets or sets the title text to be shown for the change password view
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.ChangePassword_DefaultChangePasswordTitleText),
        WebSysDescription(SR.LoginControls_TitleText)
        ]
        public virtual string ChangePasswordTitleText {
            get {
                object obj = ViewState["ChangePasswordTitleText"];
                return (obj == null) ? SR.GetString(SR.ChangePassword_DefaultChangePasswordTitleText) : (string) obj;
            }
            set {
                ViewState["ChangePasswordTitleText"] = value;
            }
        }


        /// <devdoc>
        ///     Gets the confirm new password entered by the user.
        /// </devdoc>
        [
        Browsable(false),
        Themeable(false),
        Filterable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public virtual string ConfirmNewPassword {
            get {
                return (_confirmNewPassword == null) ? String.Empty : _confirmNewPassword;
            }
        }


        /// <devdoc>
        ///     Gets or sets the text that identifies the new password textbox.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.ChangePassword_DefaultConfirmNewPasswordLabelText),
        WebSysDescription(SR.ChangePassword_ConfirmNewPasswordLabelText)
        ]
        public virtual string ConfirmNewPasswordLabelText {
            get {
                object obj = ViewState["ConfirmNewPasswordLabelText"];
                return (obj == null) ? SR.GetString(SR.ChangePassword_DefaultConfirmNewPasswordLabelText) : (string) obj;
            }
            set {
                ViewState["ConfirmNewPasswordLabelText"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the text that is displayed when the new password does not match the confirm password.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Validation"),
        WebSysDefaultValue(SR.ChangePassword_DefaultConfirmPasswordCompareErrorMessage),
        WebSysDescription(SR.ChangePassword_ConfirmPasswordCompareErrorMessage)
        ]
        public virtual string ConfirmPasswordCompareErrorMessage {
            get {
                object obj = ViewState["ConfirmPasswordCompareErrorMessage"];
                return (obj == null) ? SR.GetString(SR.ChangePassword_DefaultConfirmPasswordCompareErrorMessage) : (string) obj;
            }
            set {
                ViewState["ConfirmPasswordCompareErrorMessage"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the text to be shown in the validation summary when the confirm password is empty.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Validation"),
        WebSysDefaultValue(SR.ChangePassword_DefaultConfirmPasswordRequiredErrorMessage),
        WebSysDescription(SR.LoginControls_ConfirmPasswordRequiredErrorMessage)
        ]
        public virtual string ConfirmPasswordRequiredErrorMessage {
            get {
                object obj = ViewState["ConfirmPasswordRequiredErrorMessage"];
                return (obj == null) ? SR.GetString(SR.ChangePassword_DefaultConfirmPasswordRequiredErrorMessage) : (string) obj;
            }
            set {
                ViewState["ConfirmPasswordRequiredErrorMessage"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the URL of an image to be displayed for the continue button.
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescription(SR.ChangePassword_ContinueButtonImageUrl),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty()
        ]
        public virtual string ContinueButtonImageUrl {
            get {
                object obj = ViewState["ContinueButtonImageUrl"];
                return (obj == null) ? String.Empty : (string) obj;
            }
            set {
                ViewState["ContinueButtonImageUrl"] = value;
            }
        }


        /// <devdoc>
        ///     Gets the style of change password button.
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.ChangePassword_ContinueButtonStyle)
        ]
        public Style ContinueButtonStyle {
            get {
                if (_continueButtonStyle == null) {
                    _continueButtonStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager) _continueButtonStyle).TrackViewState();
                    }
                }
                return _continueButtonStyle;
            }
        }


        /// <devdoc>
        ///     Gets or sets the text to be shown for the continue button.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.ChangePassword_DefaultContinueButtonText),
        WebSysDescription(SR.ChangePassword_ContinueButtonText)
        ]
        public virtual string ContinueButtonText {
            get {
                object obj = ViewState["ContinueButtonText"];
                return (obj == null) ? SR.GetString(SR.ChangePassword_DefaultContinueButtonText) : (string) obj;
            }
            set {
                ViewState["ContinueButtonText"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the type of the continue button.
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(ButtonType.Button),
        WebSysDescription(SR.ChangePassword_ContinueButtonType)
        ]
        public virtual ButtonType ContinueButtonType {
            get {
                object obj = ViewState["ContinueButtonType"];
                return (obj == null) ? ButtonType.Button : (ButtonType) obj;
            }
            set {
                if (value < ButtonType.Button || value > ButtonType.Link) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["ContinueButtonType"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the URL for the continue button.
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(""),
        WebSysDescription(SR.LoginControls_ContinueDestinationPageUrl),
        Editor("System.Web.UI.Design.UrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        Themeable(false),
        UrlProperty()
        ]
        public virtual string ContinueDestinationPageUrl {
            get {
                object obj = ViewState["ContinueDestinationPageUrl"];
                return (obj == null) ? String.Empty : (string) obj;
            }
            set {
                ViewState["ContinueDestinationPageUrl"] = value;
            }
        }

        private bool ConvertingToTemplate {
            get {
                return (DesignMode && _convertingToTemplate);
            }
        }


        /// <devdoc>
        ///     Gets or sets the URL of an icon to be displayed for the create user link.
        /// </devdoc>
        [
        WebCategory("Links"),
        DefaultValue(""),
        WebSysDescription(SR.ChangePassword_CreateUserIconUrl),
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


        /// <devdoc>
        ///     Gets or sets the URL of the create user page.
        /// </devdoc>
        [
        WebCategory("Links"),
        DefaultValue(""),
        WebSysDescription(SR.ChangePassword_CreateUserUrl),
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
        ///     Gets the current password entered by the user.
        /// </devdoc>
        [
        Browsable(false),
        Themeable(false),
        Filterable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public virtual string CurrentPassword {
            get {
                return (_password == null) ? String.Empty : _password;
            }
        }

        private string CurrentPasswordInternal {
            get {
                string password = CurrentPassword;
                if (String.IsNullOrEmpty(password) && _changePasswordContainer != null) {
                    ITextControl passwordTextBox = (ITextControl)_changePasswordContainer.CurrentPasswordTextBox;
                    if (passwordTextBox != null) {
                        return passwordTextBox.Text;
                    }
                }
                return password;
            }
        }

        /// <devdoc>
        /// Internal because used from ChangePasswordAdapter.
        /// </devdoc>
        internal View CurrentView {
            get {
                return _currentView;
            }
            set {
                if (value < View.ChangePassword || value > View.Success) {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (value != CurrentView) {
                    _currentView = value;
                }
            }
        }


        /// <devdoc>
        ///     Gets or sets whether the user name is shown
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(false),
        WebSysDescription(SR.ChangePassword_DisplayUserName)
        ]
        public virtual bool DisplayUserName {
            get {
                object obj = ViewState["DisplayUserName"];
                return (obj == null) ? false : (bool) obj;
            }
            set {
                if (DisplayUserName != value) {
                    ViewState["DisplayUserName"] = value;
                    UpdateValidators();
                }
            }
        }


        /// <devdoc>
        ///     Gets or sets the URL for the image shown next to the profile page.
        /// </devdoc>
        [
        WebCategory("Links"),
        DefaultValue(""),
        WebSysDescription(SR.LoginControls_EditProfileIconUrl),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty()
        ]
        public virtual string EditProfileIconUrl {
            get {
                object obj = ViewState["EditProfileIconUrl"];
                return (obj == null) ? String.Empty : (string) obj;
            }
            set {
                ViewState["EditProfileIconUrl"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the text to be shown for the edit profile page
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Links"),
        DefaultValue(""),
        WebSysDescription(SR.ChangePassword_EditProfileText)
        ]
        public virtual string EditProfileText {
            get {
                object obj = ViewState["EditProfileText"];
                return (obj == null) ? String.Empty : (string) obj;
            }
            set {
                ViewState["EditProfileText"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the URL of the edit profile page.
        /// </devdoc>
        [
        WebCategory("Links"),
        DefaultValue(""),
        WebSysDescription(SR.ChangePassword_EditProfileUrl),
        Editor("System.Web.UI.Design.UrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty()
        ]
        public virtual string EditProfileUrl {
            get {
                object obj = ViewState["EditProfileUrl"];
                return (obj == null) ? String.Empty : (string) obj;
            }
            set {
                ViewState["EditProfileUrl"] = value;
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
        ///     Gets or sets the URL of an icon to be displayed for the help link.
        /// </devdoc>
        [
        WebCategory("Links"),
        DefaultValue(""),
        WebSysDescription(SR.LoginControls_HelpPageIconUrl),
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
        ///     Gets the new password entered by the user.
        /// </devdoc>
        [
        Browsable(false),
        Themeable(false),
        Filterable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public virtual string NewPassword {
            get {
                return (_newPassword == null) ? String.Empty : _newPassword;
            }
        }

        private string NewPasswordInternal {
            get {
                string password = NewPassword;
                if (String.IsNullOrEmpty(password) && _changePasswordContainer != null) {
                    ITextControl passwordTextBox = (ITextControl)_changePasswordContainer.NewPasswordTextBox;
                    if (passwordTextBox != null) {
                        return passwordTextBox.Text;
                    }
                }
                return password;
            }
        }


        /// <devdoc>
        ///     The text that is displayed when the new password regular expression fails
        /// </devdoc>
        [
        WebCategory("Validation"),
        WebSysDefaultValue(SR.Password_InvalidPasswordErrorMessage),
        WebSysDescription(SR.ChangePassword_NewPasswordRegularExpressionErrorMessage)
        ]
        public virtual string NewPasswordRegularExpressionErrorMessage {
            get {
                object obj = ViewState["NewPasswordRegularExpressionErrorMessage"];
                return (obj == null) ?  SR.GetString(SR.Password_InvalidPasswordErrorMessage) : (string) obj;
            }
            set {
                ViewState["NewPasswordRegularExpressionErrorMessage"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the text that identifies the new password textbox.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.ChangePassword_DefaultNewPasswordLabelText),
        WebSysDescription(SR.ChangePassword_NewPasswordLabelText)
        ]
        public virtual string NewPasswordLabelText {
            get {
                object obj = ViewState["NewPasswordLabelText"];
                return (obj == null) ? SR.GetString(SR.ChangePassword_DefaultNewPasswordLabelText) : (string) obj;
            }
            set {
                ViewState["NewPasswordLabelText"] = value;
            }
        }


        /// <devdoc>
        ///     Regular expression used to validate the new password
        /// </devdoc>
        [
        WebCategory("Validation"),
        WebSysDefaultValue(""),
        WebSysDescription(SR.ChangePassword_NewPasswordRegularExpression)
        ]
        public virtual string NewPasswordRegularExpression {
            get {
                object obj = ViewState["NewPasswordRegularExpression"];
                return (obj == null) ? String.Empty : (string) obj;
            }
            set {
                if (NewPasswordRegularExpression != value) {
                    ViewState["NewPasswordRegularExpression"] = value;
                    UpdateValidators();
                }
            }
        }


        /// <devdoc>
        ///     Gets or sets the text to be shown in the validation summary when the new password is empty.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Validation"),
        WebSysDefaultValue(SR.ChangePassword_DefaultNewPasswordRequiredErrorMessage),
        WebSysDescription(SR.ChangePassword_NewPasswordRequiredErrorMessage)
        ]
        public virtual string NewPasswordRequiredErrorMessage {
            get {
                object obj = ViewState["NewPasswordRequiredErrorMessage"];
                return (obj == null) ? SR.GetString(SR.ChangePassword_DefaultNewPasswordRequiredErrorMessage) : (string) obj;
            }
            set {
                ViewState["NewPasswordRequiredErrorMessage"] = value;
            }
        }


        /// <devdoc>
        /// The style of the password hint text.
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.ChangePassword_PasswordHintStyle)
        ]
        public TableItemStyle PasswordHintStyle {
            get {
                if (_passwordHintStyle == null) {
                    _passwordHintStyle = new TableItemStyle();
                    if (IsTrackingViewState) {
                        ((IStateManager) _passwordHintStyle).TrackViewState();
                    }
                }
                return _passwordHintStyle;
            }
        }


        /// <devdoc>
        ///     Gets or sets the text to be shown for the password hint.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescription(SR.ChangePassword_PasswordHintText)
        ]
        public virtual string PasswordHintText {
            get {
                object obj = ViewState["PasswordHintText"];
                return (obj == null) ? String.Empty : (string) obj;
            }
            set {
                ViewState["PasswordHintText"] = value;
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
        ///     Gets or sets the URL of an icon to be displayed for the password recovery link.
        /// </devdoc>
        [
        WebCategory("Links"),
        DefaultValue(""),
        WebSysDescription(SR.ChangePassword_PasswordRecoveryIconUrl),
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
        WebSysDescription(SR.ChangePassword_PasswordRecoveryUrl),
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
        ///     Gets or sets the text to be shown in the validation summary when the password is empty.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Validation"),
        WebSysDefaultValue(SR.ChangePassword_DefaultPasswordRequiredErrorMessage),
        WebSysDescription(SR.ChangePassword_PasswordRequiredErrorMessage)
        ]
        public virtual string PasswordRequiredErrorMessage {
            get {
                object obj = ViewState["PasswordRequiredErrorMessage"];
                return (obj == null) ? SR.GetString(SR.ChangePassword_DefaultPasswordRequiredErrorMessage) : (string) obj;
            }
            set {
                ViewState["PasswordRequiredErrorMessage"] = value;
            }
        }

        /// <devdoc>
        /// Determines whether we create the regular expression validator
        /// </devdoc>
        private bool RegExpEnabled {
            get {
                return (NewPasswordRegularExpression.Length > 0);
            }
        }


        /// <devdoc>
        /// The content and format of the e-mail message that contains a successful change password notification.
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        Themeable(false),
        WebSysDescription(SR.ChangePassword_MailDefinition)
        ]
        public MailDefinition MailDefinition {
            get {
                if (_mailDefinition == null) {
                    _mailDefinition = new MailDefinition();
                    if (IsTrackingViewState) {
                        ((IStateManager) _mailDefinition).TrackViewState();
                    }
                }
                return _mailDefinition;
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

        /// <devdoc>
        /// The URL that the user is directed to after the password has been changed.
        /// If non-null, always redirect the user to this page after successful password change.  Else, perform the refresh action.
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(""),
        WebSysDescription(SR.LoginControls_SuccessPageUrl),
        Editor("System.Web.UI.Design.UrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        Themeable(false),
        UrlProperty()
        ]
        public virtual string SuccessPageUrl {
            get {
                object obj = ViewState["SuccessPageUrl"];
                return (obj == null) ? String.Empty : (string) obj;
            }
            set {
                ViewState["SuccessPageUrl"] = value;
            }
        }


        /// <devdoc>
        /// Template rendered after the password has been changed.
        /// </devdoc>
        [
        Browsable(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(ChangePassword))
        ]
        public virtual ITemplate SuccessTemplate {
            get {
                return _successTemplate;
            }
            set {
                _successTemplate = value;
                ChildControlsCreated = false;
            }
        }

        /// <devdoc>
        /// Internal because used from ChangePasswordAdapter.
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public Control SuccessTemplateContainer {
            get {
                EnsureChildControls();
                return _successContainer;
            }
        }


        /// <devdoc>
        /// The text to be shown after the password has been changed.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.ChangePassword_DefaultSuccessText),
        WebSysDescription(SR.ChangePassword_SuccessText)
        ]
        public virtual string SuccessText {
            get {
                object obj = ViewState["SuccessText"];
                return (obj == null) ? SR.GetString(SR.ChangePassword_DefaultSuccessText) : (string) obj;
            }
            set {
                ViewState["SuccessText"] = value;
            }
        }




        /// <devdoc>
        /// The style of the success text.
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.ChangePassword_SuccessTextStyle)
        ]
        public TableItemStyle SuccessTextStyle {
            get {
                if (_successTextStyle == null) {
                    _successTextStyle = new TableItemStyle();
                    if (IsTrackingViewState) {
                        ((IStateManager) _successTextStyle).TrackViewState();
                    }
                }
                return _successTextStyle;
            }
        }


        /// <devdoc>
        ///     Gets or sets the title text to be shown for success
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.ChangePassword_DefaultSuccessTitleText),
        WebSysDescription(SR.ChangePassword_SuccessTitleText)
        ]
        public virtual string SuccessTitleText {
            get {
                object obj = ViewState["SuccessTitleText"];
                return (obj == null) ? SR.GetString(SR.ChangePassword_DefaultSuccessTitleText) : (string) obj;
            }
            set {
                ViewState["SuccessTitleText"] = value;
            }
        }

        protected override HtmlTextWriterTag TagKey {
            get {
                return HtmlTextWriterTag.Table;
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
                return (_userName == null) ? String.Empty : _userName;
            }
            set {
                _userName = value;
            }
        }

        private string UserNameInternal {
            get {
                string userName = UserName;
                if (String.IsNullOrEmpty(userName) && _changePasswordContainer != null && DisplayUserName) {
                    ITextControl userTextBox = (ITextControl)_changePasswordContainer.UserNameTextBox;
                    if (userTextBox != null) {
                        return userTextBox.Text;
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
        WebSysDefaultValue(SR.ChangePassword_DefaultUserNameLabelText),
        WebSysDescription(SR.LoginControls_UserNameLabelText)
        ]
        public virtual string UserNameLabelText {
            get {
                object obj = ViewState["UserNameLabelText"];
                return (obj == null) ? SR.GetString(SR.ChangePassword_DefaultUserNameLabelText) : (string) obj;
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
        WebSysDefaultValue(SR.ChangePassword_DefaultUserNameRequiredErrorMessage),
        WebSysDescription(SR.ChangePassword_UserNameRequiredErrorMessage)
        ]
        public virtual string UserNameRequiredErrorMessage {
            get {
                object obj = ViewState["UserNameRequiredErrorMessage"];
                return (obj == null) ? SR.GetString(SR.ChangePassword_DefaultUserNameRequiredErrorMessage) : (string) obj;
            }
            set {
                ViewState["UserNameRequiredErrorMessage"] = value;
            }
        }

        internal Control ValidatorRow {
            get {
                return _validatorRow;
            }
            set {
                _validatorRow = value;
            }
        }

        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.ChangePassword_ValidatorTextStyle)
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

        ///////////////////// EVENTS //////////////////////////////


        /// <devdoc>
        ///     Raised on the click of the cancel button.
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.ChangePassword_CancelButtonClick)
        ]
        public event EventHandler CancelButtonClick {
            add {
                Events.AddHandler(EventCancelButtonClick, value);
            }
            remove {
                Events.RemoveHandler(EventCancelButtonClick, value);
            }
        }


        /// <devdoc>
        ///     Raised after the password has been changed successfully.
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.ChangePassword_ChangedPassword)
        ]
        public event EventHandler ChangedPassword {
            add {
                Events.AddHandler(EventChangedPassword, value);
            }
            remove {
                Events.RemoveHandler(EventChangedPassword, value);
            }
        }


        /// <devdoc>
        ///     Raised if the change password attempt fails.
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.ChangePassword_ChangePasswordError)
        ]
        public event EventHandler ChangePasswordError {
            add {
                Events.AddHandler(EventChangePasswordError, value);
            }
            remove {
                Events.RemoveHandler(EventChangePasswordError, value);
            }
        }


        /// <devdoc>
        ///     Raised before changing the password.
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.ChangePassword_ChangingPassword)
        ]
        public event LoginCancelEventHandler ChangingPassword {
            add {
                Events.AddHandler(EventChangingPassword, value);
            }
            remove {
                Events.RemoveHandler(EventChangingPassword, value);
            }
        }


        /// <devdoc>
        ///     Raised on the click of the continue button.
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.ChangePassword_ContinueButtonClick)
        ]
        public event EventHandler ContinueButtonClick {
            add {
                Events.AddHandler(EventContinueButtonClick, value);
            }
            remove {
                Events.RemoveHandler(EventContinueButtonClick, value);
            }
        }


        /// <devdoc>
        /// Raised before the e-mail is sent.
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.ChangePassword_SendingMail)
        ]
        public event MailMessageEventHandler SendingMail {
            add {
                Events.AddHandler(EventSendingMail, value);
            }
            remove {
                Events.RemoveHandler(EventSendingMail, value);
            }
        }


        /// <devdoc>
        ///     Raised when there is an error sending mail.
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.ChangePassword_SendMailError)
        ]
        public event SendMailErrorEventHandler SendMailError {
            add {
                Events.AddHandler(EventSendMailError, value);
            }
            remove {
                Events.RemoveHandler(EventSendMailError, value);
            }
        }

        /// <devdoc>
        ///     Attempts to change the password for the user.
        ///     Raises appropriate events along the way for login, changing password, mail.
        /// </devdoc>
        private void AttemptChangePassword() {
            if (Page != null && !Page.IsValid) {
                return;
            }

            LoginCancelEventArgs cancelArgs = new LoginCancelEventArgs();
            OnChangingPassword(cancelArgs);
            if (cancelArgs.Cancel) {
                return;
            }

            MembershipProvider provider = LoginUtil.GetProvider(MembershipProvider);
            MembershipUser user = provider.GetUser(UserNameInternal, /*userIsOnline*/ false, /*throwOnError*/ false);
            string newPassword = NewPasswordInternal;
            if (user != null && user.ChangePassword(CurrentPasswordInternal, newPassword, /*throwOnError*/ false)) {
                // Only log the user in if approved and not locked out
                if (user.IsApproved && !user.IsLockedOut) {
                    System.Web.Security.FormsAuthentication.SetAuthCookie(UserNameInternal, false);
                }

                OnChangedPassword(EventArgs.Empty);
                PerformSuccessAction(user.Email, user.UserName, newPassword);
            }
            else {
                OnChangePasswordError(EventArgs.Empty);
                string failureText = ChangePasswordFailureText;
                if (!String.IsNullOrEmpty(failureText)) {
                    failureText = String.Format(CultureInfo.CurrentCulture, failureText, provider.MinRequiredPasswordLength,
                        provider.MinRequiredNonAlphanumericCharacters);
                }
                SetFailureTextLabel(_changePasswordContainer, failureText);
            }
        }

        private void ConfirmNewPasswordTextChanged(object source, EventArgs e) {
            _confirmNewPassword = ((ITextControl) source).Text;
        }

        /// <devdoc>
        /// Creates the controls needed for the change password view
        /// </devdoc>
        private void CreateChangePasswordViewControls() {
            _changePasswordContainer = new ChangePasswordContainer(this);
            _changePasswordContainer.ID = _changePasswordViewContainerID;
            _changePasswordContainer.RenderDesignerRegion = _renderDesignerRegion;

            ITemplate template = ChangePasswordTemplate;
            bool defaultTemplate = (template == null);
            if (defaultTemplate) {
                // Only disable viewstate if using default template
                _changePasswordContainer.EnableViewState = false;

                // Disable theming if using default template (VSWhidbey 86010)
                _changePasswordContainer.EnableTheming = false;

                template = new DefaultChangePasswordTemplate(this);
           }

            template.InstantiateIn(_changePasswordContainer);
            Controls.Add(_changePasswordContainer);

            IEditableTextControl userNameTextBox = _changePasswordContainer.UserNameTextBox as IEditableTextControl;
            if (userNameTextBox != null) {
                userNameTextBox.TextChanged += new EventHandler(UserNameTextChanged);
            }

            IEditableTextControl passwordTextBox = _changePasswordContainer.CurrentPasswordTextBox as IEditableTextControl;
            if (passwordTextBox != null) {
                passwordTextBox.TextChanged += new EventHandler(PasswordTextChanged);
            }

            IEditableTextControl newPasswordTextBox = _changePasswordContainer.NewPasswordTextBox as IEditableTextControl;
            if (newPasswordTextBox != null) {
                newPasswordTextBox.TextChanged += new EventHandler(NewPasswordTextChanged);
            }

            IEditableTextControl confirmNewPasswordTextBox = _changePasswordContainer.ConfirmNewPasswordTextBox as IEditableTextControl;
            if (confirmNewPasswordTextBox != null) {
                confirmNewPasswordTextBox.TextChanged += new EventHandler(ConfirmNewPasswordTextChanged);
            }

            // Set the editable child control properties here for two reasons:
            // - So change events will be raised if viewstate is disabled on the child controls
            //   - Viewstate is always disabled for default template, and might be for user template
            // - So the controls render correctly in the designer
            SetEditableChildProperties();
        }


        /// <devdoc>
        ///     Instantiates the template in the template container, and wires up necessary events.
        /// </devdoc>
        protected internal override void CreateChildControls() {
            Controls.Clear();

            CreateChangePasswordViewControls();
            CreateSuccessViewControls();
            UpdateValidators();
        }

        private void CreateSuccessViewControls() {
            ITemplate template = null;
            _successContainer = new SuccessContainer(this);
            _successContainer.ID = _successViewContainerID;
            _successContainer.RenderDesignerRegion = _renderDesignerRegion;
            if (SuccessTemplate != null) {
                template = SuccessTemplate;
            }
            else {
                // 
                template = new DefaultSuccessTemplate(this);
                _successContainer.EnableViewState = false;

                // Disable theming if using default template (VSWhidbey 86010)
                _successContainer.EnableTheming = false;
            }
            template.InstantiateIn(_successContainer);
            Controls.Add(_successContainer);
        }

        /// <devdoc>
        /// Loads the control state for those properties that should persist across postbacks
        /// even when EnableViewState=false.
        /// </devdoc>
        protected internal override void LoadControlState(object savedState) {
            if (savedState != null) {
                Triplet state = (Triplet)savedState;
                if (state.First != null) {
                    base.LoadControlState(state.First);
                }
                if (state.Second != null) {
                    _currentView = (View)(int)state.Second;
                }
                if (state.Third != null) {
                    _userName = (string)state.Third;
                }
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///     Loads a saved state of the <see cref='System.Web.UI.WebControls.ChangePassword'/>.
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
                    ((IStateManager) ChangePasswordButtonStyle).LoadViewState(myState[1]);
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
                    ((IStateManager) PasswordHintStyle).LoadViewState(myState[7]);
                }
                if (myState[8] != null) {
                    ((IStateManager) FailureTextStyle).LoadViewState(myState[8]);
                }
                if (myState[9] != null) {
                    ((IStateManager) MailDefinition).LoadViewState(myState[9]);
                }
                if (myState[10] != null) {
                    ((IStateManager) CancelButtonStyle).LoadViewState(myState[10]);
                }
                if (myState[11] != null) {
                    ((IStateManager) ContinueButtonStyle).LoadViewState(myState[11]);
                }
                if (myState[12] != null) {
                    ((IStateManager) SuccessTextStyle).LoadViewState(myState[12]);
                }
                if (myState[13] != null) {
                    ((IStateManager) ValidatorTextStyle).LoadViewState(myState[13]);
                }
            }

            UpdateValidators();
        }

        private void NewPasswordTextChanged(object source, EventArgs e) {
            _newPassword = ((ITextControl) source).Text;
        }


        /// <devdoc>
        /// Called when an event is raised by a control inside our template.  Attempts to login
        /// if the event was raised by the submit button.
        /// </devdoc>
        protected override bool OnBubbleEvent(object source, EventArgs e) {
            bool handled = false;
            if (e is CommandEventArgs) {
                CommandEventArgs ce = (CommandEventArgs) e;
                if (ce.CommandName.Equals(ChangePasswordButtonCommandName, StringComparison.CurrentCultureIgnoreCase)) {
                    AttemptChangePassword();
                    handled = true;
                }
                else if (ce.CommandName.Equals(CancelButtonCommandName, StringComparison.CurrentCultureIgnoreCase)) {
                    OnCancelButtonClick(ce);
                    handled = true;
                }
                else if (ce.CommandName.Equals(ContinueButtonCommandName, StringComparison.CurrentCultureIgnoreCase)) {
                    OnContinueButtonClick(ce);
                    handled = true;
                }
            }
            return handled;
        }


        /// <devdoc>
        ///     Raises the CancelClick event.
        /// </devdoc>
        protected virtual void OnCancelButtonClick(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventCancelButtonClick];
            if (handler != null) {
                handler(this, e);
            }

            string cancelPageUrl = CancelDestinationPageUrl;
            if (!String.IsNullOrEmpty(cancelPageUrl)) {
                // [....] suggested that we should not terminate execution of current page, to give
                // page a chance to cleanup its resources.  This may be less performant though.
                // [....] suggested that we need to call ResolveClientUrl before redirecting.
                // Example is this control inside user control, want redirect relative to user control dir.
                Page.Response.Redirect(ResolveClientUrl(cancelPageUrl), false);
            }
        }


        /// <devdoc>
        ///     Raises the ChangedPassword event.
        /// </devdoc>
        protected virtual void OnChangedPassword(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventChangedPassword];
            if (handler != null) {
                handler(this, e);
            }
        }


        /// <devdoc>
        ///     Raises the ChangePasswordError event.
        /// </devdoc>
        protected virtual void OnChangePasswordError(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventChangePasswordError];
            if (handler != null) {
                handler(this, e);
            }
        }


        /// <devdoc>
        ///     Raises the ChangingPassword event.
        /// </devdoc>
        protected virtual void OnChangingPassword(LoginCancelEventArgs e) {
            LoginCancelEventHandler handler = (LoginCancelEventHandler)Events[EventChangingPassword];
            if (handler != null) {
                handler(this, e);
            }
        }


        /// <devdoc>
        ///     Raises the ContinueButtonClick event.
        /// </devdoc>
        protected virtual void OnContinueButtonClick(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventContinueButtonClick];
            if (handler != null) {
                handler(this, e);
            }

            string continuePageUrl = ContinueDestinationPageUrl;
            if (!String.IsNullOrEmpty(continuePageUrl)) {

                // [....] suggested that we should not terminate execution of current page, to give
                // page a chance to cleanup its resources.  This may be less performant though.
                // [....] suggested that we need to call ResolveClientUrl before redirecting.
                // Example is this control inside user control, want redirect relative to user control dir.
                Page.Response.Redirect(ResolveClientUrl(continuePageUrl), false);
            }
       }


        protected internal override void OnInit(EventArgs e) {
            // Fill in the User name if authenticated
            if (!DesignMode) {
                string userName = LoginUtil.GetUserName(this);
                if (!String.IsNullOrEmpty(userName)) {
                    UserName = userName;
                }
            }

            base.OnInit(e);
            Page.RegisterRequiresControlState(this);
        }


        /// <devdoc>
        ///     Overridden to set the editable child control properteries and hide the control when appropriate.
        /// </devdoc>
        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            // Set the editable child control properties here instead of Render, so they get into viewstate for the user template.
            switch (CurrentView) {
                case View.ChangePassword:
                    SetEditableChildProperties();
                    break;
            }
        }


        /// <devdoc>
        /// Raises the SendingMail event.
        /// </devdoc>
        protected virtual void OnSendingMail(MailMessageEventArgs e) {
            MailMessageEventHandler handler = (MailMessageEventHandler)Events[EventSendingMail];
            if (handler != null) {
                handler(this, e);
            }
        }


        /// <devdoc>
        /// Raises the SendMailError event.
        /// </devdoc>
        protected virtual void OnSendMailError(SendMailErrorEventArgs e) {
            SendMailErrorEventHandler handler = (SendMailErrorEventHandler)Events[EventSendMailError];
            if (handler != null) {
                handler(this, e);
            }
        }

        private void PasswordTextChanged(object source, EventArgs e) {
            _password = ((ITextControl) source).Text;
        }

        private void PerformSuccessAction(string email, string userName, string newPassword) {
            // Try to send mail only if a MailDefinition is specified for success,
            // and the user has an email address.
            if (_mailDefinition != null && !String.IsNullOrEmpty(email)) {
                LoginUtil.SendPasswordMail(email, userName, newPassword, MailDefinition, /*defaultSubject*/ null,
                                           /*defaultBody*/ null, OnSendingMail, OnSendMailError, this);
            }

            string successPageUrl = SuccessPageUrl;
            if (!String.IsNullOrEmpty(successPageUrl)) {
                // [....] suggested that we should not terminate execution of current page, to give
                // page a chance to cleanup its resources.  This may be less performant though.
                // [....] suggested that we need to call ResolveClientUrl before redirecting.
                // Example is this control inside user control, want redirect relative to user control dir.
                Page.Response.Redirect(ResolveClientUrl(successPageUrl), false);
            }
            else {
                CurrentView = View.Success;
            }
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
            }
            EnsureChildControls();

            SetChildProperties();
            RenderContents(writer);
        }


        /// <devdoc>
        /// Saves the control state for those properties that should persist across postbacks
        /// even when EnableViewState=false.
        /// </devdoc>
        protected internal override object SaveControlState() {
            object baseState = base.SaveControlState();

            // Save the int value of the enum (instead of the enum value itself)
            // to save space in ControlState
            object currentViewState = null;
            object userNameState = null;

            currentViewState = (int)_currentView;

            // Don't save _userName once we have reached the success view (VSWhidbey 81327)
            if (_userName != null && _currentView != View.Success) {
                userNameState = _userName;
            }
            return new Triplet(baseState, currentViewState, userNameState);
        }


        /// <internalonly/>
        /// <devdoc>
        ///     Saves the state of the <see cref='System.Web.UI.WebControls.ChangePassword'/>.
        /// </devdoc>
        protected override object SaveViewState() {
            object[] myState = new object[_viewStateArrayLength];

            myState[0] = base.SaveViewState();
            myState[1] = (_changePasswordButtonStyle != null) ? ((IStateManager)_changePasswordButtonStyle).SaveViewState() : null;
            myState[2] = (_labelStyle != null) ? ((IStateManager)_labelStyle).SaveViewState() : null;
            myState[3] = (_textBoxStyle != null) ? ((IStateManager)_textBoxStyle).SaveViewState() : null;
            myState[4] = (_hyperLinkStyle != null) ? ((IStateManager)_hyperLinkStyle).SaveViewState() : null;
            myState[5] =
                (_instructionTextStyle != null) ? ((IStateManager)_instructionTextStyle).SaveViewState() : null;
            myState[6] = (_titleTextStyle != null) ? ((IStateManager)_titleTextStyle).SaveViewState() : null;
            myState[7] = (_passwordHintStyle != null) ? ((IStateManager)_passwordHintStyle).SaveViewState() : null;
            myState[8] =
                (_failureTextStyle != null) ? ((IStateManager)_failureTextStyle).SaveViewState() : null;
            myState[9] = (_mailDefinition != null) ? ((IStateManager)_mailDefinition).SaveViewState() : null;
            myState[10] = (_cancelButtonStyle != null) ? ((IStateManager)_cancelButtonStyle).SaveViewState() : null;
            myState[11] = (_continueButtonStyle != null) ? ((IStateManager)_continueButtonStyle).SaveViewState() : null;
            myState[12] = (_successTextStyle != null) ? ((IStateManager)_successTextStyle).SaveViewState() : null;
            myState[13] = (_validatorTextStyle != null) ? ((IStateManager)_validatorTextStyle).SaveViewState() : null;

            for (int i=0; i < _viewStateArrayLength; i++) {
                if (myState[i] != null) {
                    return myState;
                }
            }

            // More performant to return null than an array of null values
            return null;
        }

        /// <devdoc>
        /// Used frequently, so extracted into method.
        /// </devdoc>
        private void SetFailureTextLabel(ChangePasswordContainer container, string failureText) {
            ITextControl failureTextLabel = (ITextControl)container.FailureTextLabel;
            if (failureTextLabel != null) {
                failureTextLabel.Text = failureText;
            }
        }

        /// <devdoc>
        ///     Internal for access from LoginAdapter
        /// </devdoc>
        internal void SetChildProperties() {
            switch (CurrentView) {
            case View.ChangePassword:
                SetCommonChangePasswordViewProperties();
                if (ChangePasswordTemplate == null) {
                    SetDefaultChangePasswordViewProperties();
                }
                break;
            case View.Success:
                SetCommonSuccessViewProperties();
                if (SuccessTemplate == null) {
                    SetDefaultSuccessViewProperties();
                }
                break;
            }
        }

        /// <devdoc>
        ///     Sets change password view control properties that apply to both default and user templates.
        /// </devdoc>
        private void SetCommonChangePasswordViewProperties() {
            // Clear out the access key/tab index so it doesn't get applied to the tables in the container
            Util.CopyBaseAttributesToInnerControl(this, _changePasswordContainer);

            _changePasswordContainer.ApplyStyle(ControlStyle);
            _successContainer.Visible = false;
        }

        /// <devdoc>
        ///     Sets success view control properties that apply to both default and user templates.
        /// </devdoc>
        private void SetCommonSuccessViewProperties() {
            // Clear out the tab index so it doesn't get applied to the tables in the container
            Util.CopyBaseAttributesToInnerControl(this, _successContainer);

            _successContainer.ApplyStyle(ControlStyle);
            _changePasswordContainer.Visible = false;
        }


        /// <internalonly/>
        /// <devdoc>
        /// Allows the designer to set the CurrentView, so the different templates can be shown in the designer.
        /// </devdoc>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        protected override void SetDesignModeState(IDictionary data) {
            if (data != null) {
                object o = data["CurrentView"];
                if (o != null) {
                    CurrentView = (View)o;
                }
                o = data["ConvertToTemplate"];
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
        ///     Sets child control properties that apply only to the default template.
        /// </devdoc>
        private void SetDefaultChangePasswordViewProperties() {
            ChangePasswordContainer container = _changePasswordContainer;

            // Need to set the BorderPadding on the BorderTable instead of the LayoutTable, since
            // setting it on the LayoutTable would cause all of the controls inside the Login to be
            // separated by the BorderPadding amount.
            container.BorderTable.CellPadding = BorderPadding;
            container.BorderTable.CellSpacing = 0;

            LoginUtil.ApplyStyleToLiteral(container.Title, ChangePasswordTitleText, TitleTextStyle, true);
            LoginUtil.ApplyStyleToLiteral(container.Instruction, InstructionText, InstructionTextStyle, true);
            LoginUtil.ApplyStyleToLiteral(container.UserNameLabel, UserNameLabelText, LabelStyle, false);
            LoginUtil.ApplyStyleToLiteral(container.CurrentPasswordLabel, PasswordLabelText, LabelStyle, false);
            LoginUtil.ApplyStyleToLiteral(container.NewPasswordLabel, NewPasswordLabelText, LabelStyle, false);
            LoginUtil.ApplyStyleToLiteral(container.ConfirmNewPasswordLabel, ConfirmNewPasswordLabelText, LabelStyle, false);
            LoginUtil.ApplyStyleToLiteral(container.PasswordHintLabel, PasswordHintText, PasswordHintStyle, false);

            // Apply style to all the text boxes if necessary
            if (_textBoxStyle != null) {
                if (DisplayUserName) ((WebControl)container.UserNameTextBox).ApplyStyle(TextBoxStyle);
                ((WebControl)container.CurrentPasswordTextBox).ApplyStyle(TextBoxStyle);
                ((WebControl)container.NewPasswordTextBox).ApplyStyle(TextBoxStyle);
                ((WebControl)container.ConfirmNewPasswordTextBox).ApplyStyle(TextBoxStyle);
            }

            _passwordHintTableRow.Visible = !String.IsNullOrEmpty(PasswordHintText);
            _userNameTableRow.Visible = DisplayUserName;

            // Tab Index
            if (DisplayUserName) {
                ((WebControl)container.UserNameTextBox).TabIndex = TabIndex;
                ((WebControl)container.UserNameTextBox).AccessKey = AccessKey;
            } else {
                ((WebControl)container.CurrentPasswordTextBox).AccessKey = AccessKey;
            }
            ((WebControl)container.CurrentPasswordTextBox).TabIndex = TabIndex;
            ((WebControl)container.NewPasswordTextBox).TabIndex = TabIndex;
            ((WebControl)container.ConfirmNewPasswordTextBox).TabIndex = TabIndex;

            // Validator setup
            bool enableValidation = true;
            ValidatorRow.Visible = enableValidation;

            RequiredFieldValidator userNameRequired = container.UserNameRequired;
            userNameRequired.ErrorMessage = UserNameRequiredErrorMessage;
            userNameRequired.ToolTip = UserNameRequiredErrorMessage;
            userNameRequired.Enabled = enableValidation;
            userNameRequired.Visible = enableValidation;
            if (_validatorTextStyle != null) {
                userNameRequired.ApplyStyle(_validatorTextStyle);
            }

            RequiredFieldValidator passwordRequired = container.PasswordRequired;
            passwordRequired.ErrorMessage = PasswordRequiredErrorMessage;
            passwordRequired.ToolTip = PasswordRequiredErrorMessage;
            passwordRequired.Enabled = enableValidation;
            passwordRequired.Visible = enableValidation;

            RequiredFieldValidator newPasswordRequired = container.NewPasswordRequired;
            newPasswordRequired.ErrorMessage = NewPasswordRequiredErrorMessage;
            newPasswordRequired.ToolTip = NewPasswordRequiredErrorMessage;
            newPasswordRequired.Enabled = enableValidation;
            newPasswordRequired.Visible = enableValidation;

            RequiredFieldValidator confirmNewPasswordRequired = container.ConfirmNewPasswordRequired;
            confirmNewPasswordRequired.ErrorMessage = ConfirmPasswordRequiredErrorMessage;
            confirmNewPasswordRequired.ToolTip = ConfirmPasswordRequiredErrorMessage;
            confirmNewPasswordRequired.Enabled = enableValidation;
            confirmNewPasswordRequired.Visible = enableValidation;

            CompareValidator newPasswordCompareValidator = container.NewPasswordCompareValidator;
            newPasswordCompareValidator.ErrorMessage = ConfirmPasswordCompareErrorMessage;
            newPasswordCompareValidator.Enabled = enableValidation;
            newPasswordCompareValidator.Visible = enableValidation;

            if (_validatorTextStyle != null) {
                passwordRequired.ApplyStyle(_validatorTextStyle);
                newPasswordRequired.ApplyStyle(_validatorTextStyle);
                confirmNewPasswordRequired.ApplyStyle(_validatorTextStyle);
                newPasswordCompareValidator.ApplyStyle(_validatorTextStyle);
            }

            RegularExpressionValidator regExpValidator = container.RegExpValidator;
            regExpValidator.ErrorMessage = NewPasswordRegularExpressionErrorMessage;
            regExpValidator.Enabled = enableValidation;
            regExpValidator.Visible = enableValidation;
            if (_validatorTextStyle != null) {
                regExpValidator.ApplyStyle(_validatorTextStyle);
            }

            //Button setup
            LinkButton linkButton = container.ChangePasswordLinkButton;
            LinkButton cancelLinkButton = container.CancelLinkButton;
            ImageButton imageButton = container.ChangePasswordImageButton;
            ImageButton cancelImageButton = container.CancelImageButton;
            Button pushButton = container.ChangePasswordPushButton;
            Button cancelPushButton = container.CancelPushButton;

            WebControl changePasswordButton = null;
            WebControl cancelButton = null;
            switch (CancelButtonType) {
                case ButtonType.Link:
                    cancelLinkButton.Text = CancelButtonText;
                    cancelButton = cancelLinkButton;
                    break;
                case ButtonType.Image:
                    cancelImageButton.ImageUrl = CancelButtonImageUrl;
                    cancelImageButton.AlternateText = CancelButtonText;
                    cancelButton = cancelImageButton;
                    break;
                case ButtonType.Button:
                    cancelPushButton.Text = CancelButtonText;
                    cancelButton = cancelPushButton;
                    break;
            }
            switch (ChangePasswordButtonType) {
                case ButtonType.Link:
                    linkButton.Text = ChangePasswordButtonText;
                    changePasswordButton = linkButton;
                    break;
                case ButtonType.Image:
                    imageButton.ImageUrl = ChangePasswordButtonImageUrl;
                    imageButton.AlternateText = ChangePasswordButtonText;
                    changePasswordButton = imageButton;
                    break;
                case ButtonType.Button:
                    pushButton.Text = ChangePasswordButtonText;
                    changePasswordButton = pushButton;
                    break;
            }

            // Set all buttons to nonvisible, then set the selected button to visible
            linkButton.Visible = false;
            imageButton.Visible = false;
            pushButton.Visible = false;
            cancelLinkButton.Visible = false;
            cancelImageButton.Visible = false;
            cancelPushButton.Visible = false;
            changePasswordButton.Visible = true;
            cancelButton.Visible = true;
            cancelButton.TabIndex = TabIndex;
            changePasswordButton.TabIndex = TabIndex;

            if (CancelButtonStyle != null) cancelButton.ApplyStyle(CancelButtonStyle);
            if (ChangePasswordButtonStyle != null) changePasswordButton.ApplyStyle(ChangePasswordButtonStyle);

            // Link Setup
            Image createUserIcon = container.CreateUserIcon;
            HyperLink createUserLink = container.CreateUserLink;
            LiteralControl createUserLinkSeparator = container.CreateUserLinkSeparator;
            HyperLink passwordRecoveryLink = container.PasswordRecoveryLink;
            Image passwordRecoveryIcon = container.PasswordRecoveryIcon;
            HyperLink helpPageLink = container.HelpPageLink;
            Image helpPageIcon = container.HelpPageIcon;
            LiteralControl helpPageLinkSeparator = container.HelpPageLinkSeparator;
            LiteralControl editProfileLinkSeparator = container.EditProfileLinkSeparator;
            HyperLink editProfileLink = container.EditProfileLink;
            Image editProfileIcon = container.EditProfileIcon;
            string createUserText = CreateUserText;
            string createUserIconUrl = CreateUserIconUrl;
            string passwordRecoveryText = PasswordRecoveryText;
            string passwordRecoveryIconUrl = PasswordRecoveryIconUrl;
            string helpPageText = HelpPageText;
            string helpPageIconUrl = HelpPageIconUrl;
            string editProfileText = EditProfileText;
            string editProfileIconUrl = EditProfileIconUrl;
            bool createUserTextVisible = (createUserText.Length > 0);
            bool passwordRecoveryTextVisible = (passwordRecoveryText.Length > 0);
            bool helpPageTextVisible = (helpPageText.Length > 0);
            bool helpPageIconVisible = (helpPageIconUrl.Length > 0);
            bool createUserIconVisible = (createUserIconUrl.Length > 0);
            bool passwordRecoveryIconVisible = (passwordRecoveryIconUrl.Length > 0);
            bool helpPageLineVisible = helpPageTextVisible || helpPageIconVisible;
            bool createUserLineVisible = createUserTextVisible || createUserIconVisible;
            bool passwordRecoveryLineVisible = passwordRecoveryTextVisible || passwordRecoveryIconVisible;
            bool editProfileTextVisible = (editProfileText.Length > 0);
            bool editProfileIconVisible = (editProfileIconUrl.Length > 0);
            bool editProfileLineVisible = (editProfileTextVisible || editProfileIconVisible);

            helpPageLink.Visible = helpPageTextVisible;
            helpPageLinkSeparator.Visible = helpPageLineVisible && (passwordRecoveryLineVisible || createUserLineVisible || editProfileLineVisible);
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
            createUserLinkSeparator.Visible = (createUserLineVisible && (passwordRecoveryLineVisible || editProfileLineVisible));
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
            editProfileLinkSeparator.Visible = (passwordRecoveryLineVisible && editProfileLineVisible);

            editProfileLink.Visible = editProfileTextVisible;
            editProfileIcon.Visible = editProfileIconVisible;
            if (editProfileTextVisible) {
                editProfileLink.Text = editProfileText;
                editProfileLink.NavigateUrl = EditProfileUrl;
                editProfileLink.TabIndex = TabIndex;
            }
            if (editProfileIconVisible) {
                editProfileIcon.ImageUrl = editProfileIconUrl;
                editProfileIcon.AlternateText = EditProfileText;
            }

            if (createUserLineVisible || passwordRecoveryLineVisible || helpPageLineVisible || editProfileLineVisible) {
                if (_hyperLinkStyle != null) {
                    // Apply style except font to table cell, then apply font and forecolor to HyperLinks
                    // VSWhidbey 81289
                    TableItemStyle hyperLinkStyleExceptFont = new TableItemStyle();
                    hyperLinkStyleExceptFont.CopyFrom(_hyperLinkStyle);
                    hyperLinkStyleExceptFont.Font.Reset();
                    LoginUtil.SetTableCellStyle(createUserLink, hyperLinkStyleExceptFont);
                    createUserLink.Font.CopyFrom(_hyperLinkStyle.Font);
                    createUserLink.ForeColor = _hyperLinkStyle.ForeColor;
                    passwordRecoveryLink.Font.CopyFrom(_hyperLinkStyle.Font);
                    passwordRecoveryLink.ForeColor = _hyperLinkStyle.ForeColor;
                    helpPageLink.Font.CopyFrom(_hyperLinkStyle.Font);
                    helpPageLink.ForeColor = _hyperLinkStyle.ForeColor;
                    editProfileLink.Font.CopyFrom(_hyperLinkStyle.Font);
                    editProfileLink.ForeColor = _hyperLinkStyle.ForeColor;
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

        /// <devdoc>
        /// Internal because called from ChangePasswordAdapter.
        /// </devdoc>
        internal void SetDefaultSuccessViewProperties() {
            SuccessContainer container = _successContainer;
            LinkButton linkButton = container.ContinueLinkButton;
            ImageButton imageButton = container.ContinueImageButton;
            Button pushButton = container.ContinuePushButton;

            container.BorderTable.CellPadding = BorderPadding;
            container.BorderTable.CellSpacing = 0;

            WebControl button = null;
            switch (ContinueButtonType) {
                case ButtonType.Link:
                    linkButton.Text = ContinueButtonText;
                    button = linkButton;
                    break;
                case ButtonType.Image:
                    imageButton.ImageUrl = ContinueButtonImageUrl;
                    imageButton.AlternateText = ContinueButtonText;
                    button = imageButton;
                    break;
                case ButtonType.Button:
                    pushButton.Text = ContinueButtonText;
                    button = pushButton;
                    break;
            }

            linkButton.Visible = false;
            imageButton.Visible = false;
            pushButton.Visible = false;
            button.Visible = true;
            button.TabIndex = TabIndex;
            button.AccessKey = AccessKey;

            if (ContinueButtonStyle != null) button.ApplyStyle(ContinueButtonStyle);

            LoginUtil.ApplyStyleToLiteral(container.Title, SuccessTitleText, _titleTextStyle, true);
            LoginUtil.ApplyStyleToLiteral(container.SuccessTextLabel, SuccessText, _successTextStyle, true);

            string editProfileText = EditProfileText;
            string editProfileIconUrl = EditProfileIconUrl;
            bool editProfileVisible = (editProfileText.Length > 0);
            bool editProfileIconVisible = (editProfileIconUrl.Length > 0);
            HyperLink editProfileLink = container.EditProfileLink;
            Image editProfileIcon = container.EditProfileIcon;
            editProfileIcon.Visible = editProfileIconVisible;
            editProfileLink.Visible = editProfileVisible;
            if (editProfileVisible) {
                editProfileLink.Text = editProfileText;
                editProfileLink.NavigateUrl = EditProfileUrl;
                editProfileLink.TabIndex = TabIndex;
                if (_hyperLinkStyle != null) {
                    // Apply style except font to table cell, then apply font and forecolor to HyperLinks
                    // VSWhidbey 81289
                    Style hyperLinkStyleExceptFont = new TableItemStyle();
                    hyperLinkStyleExceptFont.CopyFrom(_hyperLinkStyle);
                    hyperLinkStyleExceptFont.Font.Reset();
                    LoginUtil.SetTableCellStyle(editProfileLink, hyperLinkStyleExceptFont);
                    editProfileLink.Font.CopyFrom(_hyperLinkStyle.Font);
                    editProfileLink.ForeColor = _hyperLinkStyle.ForeColor;
                }
            }
            if (editProfileIconVisible) {
                editProfileIcon.ImageUrl = editProfileIconUrl;
                editProfileIcon.AlternateText = EditProfileText;
            }
            LoginUtil.SetTableCellVisible(editProfileLink, editProfileVisible || editProfileIconVisible);
        }

        /// <devdoc>
        ///     Sets the properties of child controls that are editable by the client.
        /// </devdoc>
        private void SetEditableChildProperties() {
            // We need to use UserNameInternal for the DropDownList case where it won't fire a TextChanged for the first item
            if (UserNameInternal.Length > 0 && DisplayUserName) {
                ITextControl userNameTextBox = (ITextControl)_changePasswordContainer.UserNameTextBox;
                if (userNameTextBox != null) {
                    userNameTextBox.Text = UserNameInternal;
                }
            }
        }

        /// <devdoc>
        ///     Marks the starting point to begin tracking and saving changes to the
        ///     control as part of the control viewstate.
        /// </devdoc>
        protected override void TrackViewState() {
            base.TrackViewState();

            if (_changePasswordButtonStyle != null) {
                ((IStateManager) _changePasswordButtonStyle).TrackViewState();
            }
            if (_labelStyle != null) {
                ((IStateManager) _labelStyle).TrackViewState();
            }
            if (_textBoxStyle != null) {
                ((IStateManager) _textBoxStyle).TrackViewState();
            }
            if (_successTextStyle != null) {
                ((IStateManager) _successTextStyle).TrackViewState();
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
            if (_passwordHintStyle != null) {
                ((IStateManager) _passwordHintStyle).TrackViewState();
            }
            if (_failureTextStyle != null) {
                ((IStateManager) _failureTextStyle).TrackViewState();
            }
            if (_mailDefinition != null) {
                ((IStateManager) _mailDefinition).TrackViewState();
            }
            if (_cancelButtonStyle != null) {
                ((IStateManager) _cancelButtonStyle).TrackViewState();
            }
            if (_continueButtonStyle != null) {
                ((IStateManager) _continueButtonStyle).TrackViewState();
            }
            if (_validatorTextStyle != null) {
                ((IStateManager) _validatorTextStyle).TrackViewState();
            }
        }

        private void UpdateValidators() {
            if (DesignMode) {
                return;
            }

            ChangePasswordContainer container = _changePasswordContainer;
            
            if (container != null) {
                bool displayUserName = DisplayUserName;
                RequiredFieldValidator userNameRequired = container.UserNameRequired;
                if (userNameRequired != null) {
                    userNameRequired.Enabled = displayUserName;
                    userNameRequired.Visible = displayUserName;
                }

                bool regExpEnabled = RegExpEnabled;
                RegularExpressionValidator regExpValidator = container.RegExpValidator;
                if (regExpValidator != null) {
                    regExpValidator.Enabled = regExpEnabled;
                    regExpValidator.Visible = regExpEnabled;
                }
            }
        }

        private void UserNameTextChanged(object source, EventArgs e) {
            string userName = ((ITextControl) source).Text;
            if (!String.IsNullOrEmpty(userName)) {
                UserName = userName;
            }
        }

        /// <devdoc>
        /// The default success template for the control, used if SuccessTemplate is null.
        /// </devdoc>
        private sealed class DefaultSuccessTemplate : ITemplate {
            private ChangePassword _owner;

            public DefaultSuccessTemplate(ChangePassword owner) {
                _owner = owner;
            }

            private void CreateControls(SuccessContainer successContainer) {
                successContainer.Title = new Literal();
                successContainer.SuccessTextLabel = new Literal();
                successContainer.EditProfileLink = new HyperLink();
                successContainer.EditProfileLink.ID = _editProfileSuccessLinkID;
                successContainer.EditProfileIcon = new Image();

                LinkButton linkButton = new LinkButton();
                linkButton.ID = _continueLinkButtonID;
                linkButton.CommandName = ContinueButtonCommandName;
                linkButton.CausesValidation = false;
                successContainer.ContinueLinkButton = linkButton;

                ImageButton imageButton = new ImageButton();
                imageButton.ID = _continueImageButtonID;
                imageButton.CommandName = ContinueButtonCommandName;
                imageButton.CausesValidation = false;
                successContainer.ContinueImageButton = imageButton;

                Button pushButton = new Button();
                pushButton.ID = _continuePushButtonID;
                pushButton.CommandName = ContinueButtonCommandName;
                pushButton.CausesValidation = false;
                successContainer.ContinuePushButton = pushButton;
             }

            private void LayoutControls(SuccessContainer successContainer) {
                Table table = new Table();
                table.CellPadding = 0;
                TableRow r;
                TableCell c;

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.ColumnSpan = 2;
                c.HorizontalAlign = HorizontalAlign.Center;
                c.Controls.Add(successContainer.Title);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.Controls.Add(successContainer.SuccessTextLabel);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.ColumnSpan = 2;
                c.HorizontalAlign = HorizontalAlign.Right;
                c.Controls.Add(successContainer.ContinuePushButton);
                c.Controls.Add(successContainer.ContinueLinkButton);
                c.Controls.Add(successContainer.ContinueImageButton);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.ColumnSpan = 2;
                c.Controls.Add(successContainer.EditProfileIcon);
                c.Controls.Add(successContainer.EditProfileLink);
                r.Cells.Add(c);
                table.Rows.Add(r);

                Table table2 = LoginUtil.CreateChildTable(_owner.ConvertingToTemplate);
                r = new TableRow();
                c = new TableCell();
                c.Controls.Add(table);
                r.Cells.Add(c);
                table2.Rows.Add(r);

                successContainer.LayoutTable = table;
                successContainer.BorderTable = table2;
                successContainer.Controls.Add(table2);
            }

            void ITemplate.InstantiateIn(Control container) {
                SuccessContainer successContainer = (SuccessContainer) container;
                CreateControls(successContainer);
                LayoutControls(successContainer);
            }
        }

        /// <devdoc>
        ///     Container for the success template.
        ///     Internal instead of private because it must be used by ChangePasswordAdapter.
        /// </devdoc>
        internal sealed class SuccessContainer : LoginUtil.GenericContainer<ChangePassword>, INonBindingContainer {
            private Literal _successTextLabel;
            private Button _continuePushButton;
            private LinkButton _continueLinkButton;
            private ImageButton _continueImageButton;
            private Image _editProfileIcon;
            private HyperLink _editProfileLink;
            private Literal _title;

            public SuccessContainer(ChangePassword owner) : base(owner) {
            }

            internal ImageButton ContinueImageButton {
                get {
                    return _continueImageButton;
                }
                set {
                    _continueImageButton = value;
                }
            }

            internal LinkButton ContinueLinkButton {
                get {
                    return _continueLinkButton;
                }
                set {
                    _continueLinkButton = value;
                }
            }

            internal Button ContinuePushButton {
                get {
                    return _continuePushButton;
                }
                set {
                    _continuePushButton = value;
                }
            }

            protected override bool ConvertingToTemplate {
                get {
                    return Owner.ConvertingToTemplate;
                }
            }

            internal Image EditProfileIcon {
                get {
                    return _editProfileIcon;
                }
                set {
                    _editProfileIcon = value;
                }
            }

            internal HyperLink EditProfileLink {
                get {
                    return _editProfileLink;
                }
                set {
                    _editProfileLink = value;
                }
            }

            public Literal SuccessTextLabel {
                get {
                    return _successTextLabel;
                }
                set {
                    _successTextLabel = value;
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
        }

        /// <devdoc>
        ///     The default template for the control, used if ChangePasswordTemplate is null.
        /// </devdoc>
        private sealed class DefaultChangePasswordTemplate : ITemplate {
            private ChangePassword _owner;

            public DefaultChangePasswordTemplate(ChangePassword owner) {
                _owner = owner;
            }

            /// <devdoc>
            ///     Helper function to create and set properties for a required field validator
            /// </devdoc>
            private RequiredFieldValidator CreateRequiredFieldValidator(string id, TextBox textBox, string validationGroup, bool enableValidation) {
                RequiredFieldValidator validator = new RequiredFieldValidator();
                validator.ID = id;
                validator.ValidationGroup = validationGroup;
                validator.ControlToValidate = textBox.ID;
                validator.Display = _requiredFieldValidatorDisplay;
                validator.Text = SR.GetString(SR.LoginControls_DefaultRequiredFieldValidatorText);
                validator.Enabled = enableValidation;
                validator.Visible = enableValidation;
                return validator;
            }

            /// <devdoc>
            ///     Creates the child controls, sets certain properties (mostly static properties)
            /// </devdoc>
            private void CreateControls(ChangePasswordContainer container) {
                string validationGroup = _owner.UniqueID;

                container.Title = new Literal();
                container.Instruction = new Literal();
                container.PasswordHintLabel = new Literal();

                TextBox userNameTextBox = new TextBox();
                // Must explicitly set the ID of controls that raise postback events
                userNameTextBox.ID = _userNameID;
                container.UserNameTextBox = userNameTextBox;
                container.UserNameLabel = new LabelLiteral(userNameTextBox);

                bool enableValidation = (_owner.CurrentView == View.ChangePassword);

                container.UserNameRequired = CreateRequiredFieldValidator(_userNameRequiredID, userNameTextBox, validationGroup, enableValidation);

                TextBox currentPasswordTextBox = new TextBox();
                // Must explicitly set the ID of controls that raise postback events
                currentPasswordTextBox.ID = _currentPasswordID;
                currentPasswordTextBox.TextMode = TextBoxMode.Password;
                container.CurrentPasswordTextBox = currentPasswordTextBox;
                container.CurrentPasswordLabel = new LabelLiteral(currentPasswordTextBox);

                container.PasswordRequired = CreateRequiredFieldValidator(_currentPasswordRequiredID, currentPasswordTextBox, validationGroup, enableValidation);

                TextBox newPasswordTextBox = new TextBox();
                // Must explicitly set the ID of controls that raise postback events
                newPasswordTextBox.ID = _newPasswordID;
                newPasswordTextBox.TextMode = TextBoxMode.Password;
                container.NewPasswordTextBox = newPasswordTextBox;
                container.NewPasswordLabel = new LabelLiteral(newPasswordTextBox);

                container.NewPasswordRequired = CreateRequiredFieldValidator(_newPasswordRequiredID, newPasswordTextBox, validationGroup, enableValidation);

                TextBox confirmNewPasswordTextBox = new TextBox();
                // Must explicitly set the ID of controls that raise postback events
                confirmNewPasswordTextBox.ID = _confirmNewPasswordID;
                confirmNewPasswordTextBox.TextMode = TextBoxMode.Password;
                container.ConfirmNewPasswordTextBox = confirmNewPasswordTextBox;
                container.ConfirmNewPasswordLabel = new LabelLiteral(confirmNewPasswordTextBox);

                container.ConfirmNewPasswordRequired = CreateRequiredFieldValidator(_confirmNewPasswordRequiredID, confirmNewPasswordTextBox, validationGroup, enableValidation);

                // Setup compare validator for new/confirmNewPassword values
                CompareValidator compareValidator = new CompareValidator();
                compareValidator.ID = _newPasswordCompareID;
                compareValidator.ValidationGroup = validationGroup;
                compareValidator.ControlToValidate = _confirmNewPasswordID;
                compareValidator.ControlToCompare = _newPasswordID;
                compareValidator.Operator = ValidationCompareOperator.Equal;
                compareValidator.ErrorMessage = _owner.ConfirmPasswordCompareErrorMessage;
                compareValidator.Display = _compareFieldValidatorDisplay;
                compareValidator.Enabled = enableValidation;
                compareValidator.Visible = enableValidation;
                container.NewPasswordCompareValidator = compareValidator;

                // Reg exp validator
                RegularExpressionValidator regExpValidator = new RegularExpressionValidator();
                regExpValidator.ID = _newPasswordRegExpID;
                regExpValidator.ValidationGroup = validationGroup;
                regExpValidator.ControlToValidate = _newPasswordID;
                regExpValidator.ErrorMessage = _owner.NewPasswordRegularExpressionErrorMessage;
                regExpValidator.ValidationExpression = _owner.NewPasswordRegularExpression;
                regExpValidator.Display = _regexpFieldValidatorDisplay;
                regExpValidator.Enabled = enableValidation;
                regExpValidator.Visible = enableValidation;
                container.RegExpValidator = regExpValidator;

                // Buttons
                LinkButton linkButton = new LinkButton();
                linkButton.ID = _changePasswordLinkButtonID;
                linkButton.ValidationGroup = validationGroup;
                linkButton.CommandName = ChangePasswordButtonCommandName;
                container.ChangePasswordLinkButton = linkButton;

                linkButton = new LinkButton();
                linkButton.ID = _cancelLinkButtonID;
                linkButton.CausesValidation = false;
                linkButton.CommandName = CancelButtonCommandName;
                container.CancelLinkButton = linkButton;

                ImageButton imageButton = new ImageButton();
                imageButton.ID = _changePasswordImageButtonID;
                imageButton.ValidationGroup = validationGroup;
                imageButton.CommandName = ChangePasswordButtonCommandName;
                container.ChangePasswordImageButton = imageButton;

                imageButton = new ImageButton();
                imageButton.ID = _cancelImageButtonID;
                imageButton.CommandName = CancelButtonCommandName;
                imageButton.CausesValidation = false;
                container.CancelImageButton = imageButton;

                Button pushButton = new Button();
                pushButton.ID = _changePasswordPushButtonID;
                pushButton.ValidationGroup = validationGroup;
                pushButton.CommandName = ChangePasswordButtonCommandName;
                container.ChangePasswordPushButton = pushButton;

                pushButton = new Button();
                pushButton.ID = _cancelPushButtonID;
                pushButton.CommandName = CancelButtonCommandName;
                pushButton.CausesValidation = false;
                container.CancelPushButton = pushButton;

                container.PasswordRecoveryIcon = new Image();
                container.PasswordRecoveryLink = new HyperLink();
                container.PasswordRecoveryLink.ID = _passwordRecoveryLinkID;

                container.CreateUserIcon = new Image();
                container.CreateUserLink = new HyperLink();
                container.CreateUserLink.ID = _createUserLinkID;
                container.CreateUserLinkSeparator = new LiteralControl();

                container.HelpPageIcon = new Image();
                container.HelpPageLink = new HyperLink();
                container.HelpPageLink.ID = _helpLinkID;
                container.HelpPageLinkSeparator = new LiteralControl();

                container.EditProfileLink = new HyperLink();
                container.EditProfileLink.ID = _editProfileLinkID;
                container.EditProfileIcon = new Image();
                container.EditProfileLinkSeparator = new LiteralControl();

                Literal failureTextLabel = new Literal();
                failureTextLabel.ID = _failureTextID;
                container.FailureTextLabel = failureTextLabel;
            }

            /// <devdoc>
            ///     Adds the controls to a table for layout.  Layout depends on TextLayout properties.
            /// </devdoc>
            private void LayoutControls(ChangePasswordContainer container) {
                Table table = new Table();
                table.CellPadding = 0;
                TableRow r;
                TableCell c;

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.ColumnSpan = 2;
                c.HorizontalAlign = HorizontalAlign.Center;
                c.Controls.Add(container.Title);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.ColumnSpan = 2;
                c.HorizontalAlign = HorizontalAlign.Center;
                c.Controls.Add(container.Instruction);
                r.Cells.Add(c);
                table.Rows.Add(r);

                // UserName is only visible if enabled
                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.HorizontalAlign = HorizontalAlign.Right;
                if (_owner.ConvertingToTemplate) {
                    container.UserNameLabel.RenderAsLabel = true;
                }
                c.Controls.Add(container.UserNameLabel);
                r.Cells.Add(c);

                c = new TableCell();
                c.Controls.Add(container.UserNameTextBox);
                c.Controls.Add(container.UserNameRequired);
                r.Cells.Add(c);
                table.Rows.Add(r);
                _owner._userNameTableRow = r;

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.HorizontalAlign = HorizontalAlign.Right;
                c.Controls.Add(container.CurrentPasswordLabel);
                if (_owner.ConvertingToTemplate) {
                    container.CurrentPasswordLabel.RenderAsLabel = true;
                }
                r.Cells.Add(c);

                c = new TableCell();
                c.Controls.Add(container.CurrentPasswordTextBox);
                c.Controls.Add(container.PasswordRequired);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.HorizontalAlign = HorizontalAlign.Right;
                c.Controls.Add(container.NewPasswordLabel);
                if (_owner.ConvertingToTemplate) {
                    container.NewPasswordLabel.RenderAsLabel = true;
                }
                r.Cells.Add(c);

                c = new TableCell();
                c.Controls.Add(container.NewPasswordTextBox);
                c.Controls.Add(container.NewPasswordRequired);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                r.Cells.Add(c);
                c = new TableCell();
                c.Controls.Add(container.PasswordHintLabel);
                r.Cells.Add(c);
                table.Rows.Add(r);
                _owner._passwordHintTableRow = r;

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.HorizontalAlign = HorizontalAlign.Right;
                c.Controls.Add(container.ConfirmNewPasswordLabel);
                if (_owner.ConvertingToTemplate) {
                    container.ConfirmNewPasswordLabel.RenderAsLabel = true;
                }
                r.Cells.Add(c);

                c = new TableCell();
                c.Controls.Add(container.ConfirmNewPasswordTextBox);
                c.Controls.Add(container.ConfirmNewPasswordRequired);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.HorizontalAlign = HorizontalAlign.Center;
                c.ColumnSpan = 2;
                c.Controls.Add(container.NewPasswordCompareValidator);
                r.Cells.Add(c);
                table.Rows.Add(r);

                if (_owner.RegExpEnabled) {
                    r = new LoginUtil.DisappearingTableRow();
                    c = new TableCell();
                    c.HorizontalAlign = HorizontalAlign.Center;
                    c.ColumnSpan = 2;
                    c.Controls.Add(container.RegExpValidator);
                    r.Cells.Add(c);
                    table.Rows.Add(r);
                }
                _owner.ValidatorRow = r;

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.HorizontalAlign = HorizontalAlign.Center;
                c.ColumnSpan = 2;
                c.Controls.Add(container.FailureTextLabel);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.HorizontalAlign = HorizontalAlign.Right;
                c.Controls.Add(container.ChangePasswordLinkButton);
                c.Controls.Add(container.ChangePasswordImageButton);
                c.Controls.Add(container.ChangePasswordPushButton);
                r.Cells.Add(c);
                c = new TableCell();
                c.Controls.Add(container.CancelLinkButton);
                c.Controls.Add(container.CancelImageButton);
                c.Controls.Add(container.CancelPushButton);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.ColumnSpan = 2;
                c.Controls.Add(container.HelpPageIcon);
                c.Controls.Add(container.HelpPageLink);
                c.Controls.Add(container.HelpPageLinkSeparator);
                c.Controls.Add(container.CreateUserIcon);
                c.Controls.Add(container.CreateUserLink);
                container.HelpPageLinkSeparator.Text = "<br />";
                container.CreateUserLinkSeparator.Text = "<br />";
                container.EditProfileLinkSeparator.Text = "<br />";
                c.Controls.Add(container.CreateUserLinkSeparator);
                c.Controls.Add(container.PasswordRecoveryIcon);
                c.Controls.Add(container.PasswordRecoveryLink);
                c.Controls.Add(container.EditProfileLinkSeparator);
                c.Controls.Add(container.EditProfileIcon);
                c.Controls.Add(container.EditProfileLink);
                r.Cells.Add(c);
                table.Rows.Add(r);

                Table table2 = LoginUtil.CreateChildTable(_owner.ConvertingToTemplate);
                r = new TableRow();
                c = new TableCell();
                c.Controls.Add(table);
                r.Cells.Add(c);
                table2.Rows.Add(r);

                container.LayoutTable = table;
                container.BorderTable = table2;
                container.Controls.Add(table2);
            }

            #region ITemplate implementation
            void ITemplate.InstantiateIn(Control container) {
                ChangePasswordContainer cpContainer = (ChangePasswordContainer) container;
                CreateControls(cpContainer);
                LayoutControls(cpContainer);
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
        internal sealed class ChangePasswordContainer : LoginUtil.GenericContainer<ChangePassword>, INonBindingContainer {
            private LiteralControl _createUserLinkSeparator;
            private LiteralControl _helpPageLinkSeparator;
            private LiteralControl _editProfileLinkSeparator;
            private Control _failureTextLabel;
            private ImageButton _changePasswordImageButton;
            private LinkButton _changePasswordLinkButton;
            private Button _changePasswordPushButton;
            private ImageButton _cancelImageButton;
            private LinkButton _cancelLinkButton;
            private Button _cancelPushButton;

            private Image _createUserIcon;
            private Image _helpPageIcon;
            private Image _passwordRecoveryIcon;
            private Image _editProfileIcon;

            private RequiredFieldValidator _passwordRequired;
            private RequiredFieldValidator _userNameRequired;
            private RequiredFieldValidator _confirmNewPasswordRequired;
            private RequiredFieldValidator _newPasswordRequired;
            private CompareValidator _newPasswordCompareValidator;
            private RegularExpressionValidator _regExpValidator;

            private Literal _title;
            private Literal _instruction;
            private LabelLiteral _userNameLabel;
            private LabelLiteral _currentPasswordLabel;
            private LabelLiteral _newPasswordLabel;
            private LabelLiteral _confirmNewPasswordLabel;
            private Literal _passwordHintLabel;
            private Control _userNameTextBox;
            private Control _currentPasswordTextBox;
            private Control _newPasswordTextBox;
            private Control _confirmNewPasswordTextBox;
            private HyperLink _helpPageLink;
            private HyperLink _passwordRecoveryLink;
            private HyperLink _createUserLink;
            private HyperLink _editProfileLink;

            public ChangePasswordContainer(ChangePassword owner) : base(owner) {
            }

            internal ImageButton CancelImageButton {
                get {
                    return _cancelImageButton;
                }
                set {
                    _cancelImageButton = value;
                }
            }

            internal LinkButton CancelLinkButton {
                get {
                    return _cancelLinkButton;
                }
                set {
                    _cancelLinkButton = value;
                }
            }

            internal Button CancelPushButton {
                get {
                    return _cancelPushButton;
                }
                set {
                    _cancelPushButton = value;
                }
            }

            internal ImageButton ChangePasswordImageButton {
                get {
                    return _changePasswordImageButton;
                }
                set {
                    _changePasswordImageButton = value;
                }
            }

            internal LinkButton ChangePasswordLinkButton {
                get {
                    return _changePasswordLinkButton;
                }
                set {
                    _changePasswordLinkButton = value;
                }
            }

            internal Button ChangePasswordPushButton {
                get {
                    return _changePasswordPushButton;
                }
                set {
                    _changePasswordPushButton = value;
                }
            }

            internal LabelLiteral ConfirmNewPasswordLabel {
                get {
                    return _confirmNewPasswordLabel;
                }
                set {
                    _confirmNewPasswordLabel = value;
                }
            }

            internal RequiredFieldValidator ConfirmNewPasswordRequired {
                get {
                    return _confirmNewPasswordRequired;
                }

                set {
                    _confirmNewPasswordRequired = value;
                }
            }

            internal Control ConfirmNewPasswordTextBox {
                get {
                    if (_confirmNewPasswordTextBox != null) {
                        return _confirmNewPasswordTextBox;
                    }
                    else {
                        return FindOptionalControl<IEditableTextControl>(_confirmNewPasswordID);
                    }
                }
                set {
                    _confirmNewPasswordTextBox = value;
                }
            }

            protected override bool ConvertingToTemplate {
                get {
                    return Owner.ConvertingToTemplate;
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

            internal LabelLiteral CurrentPasswordLabel {
                get {
                    return _currentPasswordLabel;
                }
                set {
                    _currentPasswordLabel = value;
                }
            }

            internal Control CurrentPasswordTextBox {
                get {
                    if (_currentPasswordTextBox != null) {
                        return _currentPasswordTextBox;
                    }
                    else {
                        return FindRequiredControl<IEditableTextControl>(_currentPasswordID,
                            SR.ChangePassword_NoCurrentPasswordTextBox);
                    }
                }
                set {
                    _currentPasswordTextBox = value;
                }
            }

            internal Image EditProfileIcon {
                get {
                    return _editProfileIcon;
                }
                set {
                    _editProfileIcon = value;
                }
            }

            internal HyperLink EditProfileLink {
                get {
                    return _editProfileLink;
                }
                set {
                    _editProfileLink = value;
                }
            }

            internal LiteralControl EditProfileLinkSeparator {
                get {
                    return _editProfileLinkSeparator;
                }
                set {
                    _editProfileLinkSeparator = value;
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

            internal Image HelpPageIcon {
                get {
                    return _helpPageIcon;
                }
                set {
                    _helpPageIcon = value;
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

            internal LiteralControl HelpPageLinkSeparator {
                get {
                    return _helpPageLinkSeparator;
                }
                set {
                    _helpPageLinkSeparator = value;
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

            internal CompareValidator NewPasswordCompareValidator {
                get {
                    return _newPasswordCompareValidator;
                }
                set {
                    _newPasswordCompareValidator = value;
                }
            }

            internal LabelLiteral NewPasswordLabel {
                get {
                    return _newPasswordLabel;
                }
                set {
                    _newPasswordLabel = value;
                }
            }

            internal RequiredFieldValidator NewPasswordRequired {
                get {
                    return _newPasswordRequired;
                }

                set {
                    _newPasswordRequired = value;
                }
            }

            internal Control NewPasswordTextBox {
                get {
                    if (_newPasswordTextBox != null) {
                        return _newPasswordTextBox;
                    }
                    else {
                        return FindRequiredControl<IEditableTextControl>(_newPasswordID, SR.ChangePassword_NoNewPasswordTextBox);
                    }
                }
                set {
                    _newPasswordTextBox = value;
                }
            }

            internal Literal PasswordHintLabel {
                get {
                    return _passwordHintLabel;
                }
                set {
                    _passwordHintLabel = value;
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

            internal HyperLink PasswordRecoveryLink {
                get {
                    return _passwordRecoveryLink;
                }
                set {
                    _passwordRecoveryLink = value;
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

            internal RegularExpressionValidator RegExpValidator {
                get {
                    return _regExpValidator;
                }
                set {
                    _regExpValidator = value;
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

            internal Control UserNameTextBox {
                get {
                    if (_userNameTextBox != null) {
                        return _userNameTextBox;
                    }
                    else {
                        // UserNameTextBox is required if DisplayUserName is true, but the control *must not* be
                        // present if DisplayUserName is false. (VSWhidbey 393444)
                        if (Owner.DisplayUserName) {
                            return FindRequiredControl<IEditableTextControl>(_userNameID, SR.ChangePassword_NoUserNameTextBox);
                        }
                        else {
                            VerifyControlNotPresent<IEditableTextControl>(_userNameID, SR.ChangePassword_UserNameTextBoxNotAllowed);
                            return null;
                        }
                    }
                }
                set {
                    _userNameTextBox = value;
                }
            }
        }

        /// <devdoc>
        /// Internal because used from ChangePasswordAdapter
        /// </devdoc>
        internal enum View {
            ChangePassword,
            Success
        }
    }
}
