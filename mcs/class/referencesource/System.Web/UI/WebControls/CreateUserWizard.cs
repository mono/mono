//------------------------------------------------------------------------------
// <copyright file="CreateUserWizard.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Linq;
    using System.Security.Permissions;
    using System.Web.Security;
    using System.Web.Configuration;
    using System.Web.UI;
    using System.Web.Util;
    using System.Net.Mail;
    using System.Text;
    using System.Web.Management;


    /// <devdoc>
    ///     Displays UI that allows creating a user.
    /// </devdoc>
    [
    Bindable(false),
    DefaultEvent("CreatedUser"),
    Designer("System.Web.UI.Design.WebControls.CreateUserWizardDesigner, " + AssemblyRef.SystemDesign),
    ToolboxData("<{0}:CreateUserWizard runat=\"server\"> <WizardSteps> <asp:CreateUserWizardStep runat=\"server\"/> <asp:CompleteWizardStep runat=\"server\"/> </WizardSteps> </{0}:CreateUserWizard>")
    ]
    public class CreateUserWizard : Wizard {
        public static readonly string ContinueButtonCommandName = "Continue";

        private string _password;
        private string _confirmPassword;
        private string _answer;
        private string _unknownErrorMessage;
        private string _validationGroup;
        private CreateUserWizardStep _createUserStep;
        private CompleteWizardStep _completeStep;
        private CreateUserStepContainer _createUserStepContainer;
        private CompleteStepContainer _completeStepContainer;

        private const string _userNameReplacementKey = "<%\\s*UserName\\s*%>";
        private const string _passwordReplacementKey = "<%\\s*Password\\s*%>";

        private bool _failure;
        private bool _convertingToTemplate;

        private DefaultCreateUserNavigationTemplate _defaultCreateUserNavigationTemplate;

        private const int _viewStateArrayLength = 13;
        private Style _createUserButtonStyle;
        private TableItemStyle _labelStyle;
        private Style _textBoxStyle;
        private TableItemStyle _hyperLinkStyle;
        private TableItemStyle _instructionTextStyle;
        private TableItemStyle _titleTextStyle;
        private TableItemStyle _errorMessageStyle;
        private TableItemStyle _passwordHintStyle;
        private Style _continueButtonStyle;
        private TableItemStyle _completeSuccessTextStyle;
        private Style _validatorTextStyle;
        private MailDefinition _mailDefinition;

        private static readonly object EventCreatingUser = new object();
        private static readonly object EventCreateUserError = new object();
        private static readonly object EventCreatedUser = new object();
        private static readonly object EventButtonContinueClick = new object();
        private static readonly object EventSendingMail = new object();
        private static readonly object EventSendMailError = new object();

        private const string _createUserNavigationTemplateName = "CreateUserNavigationTemplate";

        // Needed for user template feature
        private const string _userNameID = "UserName";
        private const string _passwordID = "Password";
        private const string _confirmPasswordID = "ConfirmPassword";
        private const string _errorMessageID = "ErrorMessage";
        private const string _emailID = "Email";
        private const string _questionID = "Question";
        private const string _answerID = "Answer";

        // Needed only for "convert to template" feature, otherwise unnecessary
        private const string _userNameRequiredID = "UserNameRequired";
        private const string _passwordRequiredID = "PasswordRequired";
        private const string _confirmPasswordRequiredID = "ConfirmPasswordRequired";
        private const string _passwordRegExpID = "PasswordRegExp";
        private const string _emailRegExpID = "EmailRegExp";
        private const string _emailRequiredID = "EmailRequired";
        private const string _questionRequiredID = "QuestionRequired";
        private const string _answerRequiredID = "AnswerRequired";
        private const string _passwordCompareID = "PasswordCompare";
        private const string _continueButtonID = "ContinueButton";
        private const string _helpLinkID = "HelpLink";
        private const string _editProfileLinkID = "EditProfileLink";
        private const string _createUserStepContainerID = "CreateUserStepContainer";
        private const string _completeStepContainerID = "CompleteStepContainer";
        private const string _sideBarLabelID = "SideBarLabel";
        private const ValidatorDisplay _requiredFieldValidatorDisplay = ValidatorDisplay.Static;
        private const ValidatorDisplay _compareFieldValidatorDisplay = ValidatorDisplay.Dynamic;
        private const ValidatorDisplay _regexpFieldValidatorDisplay = ValidatorDisplay.Dynamic;

        private TableRow _passwordHintTableRow;
        private TableRow _questionRow;
        private TableRow _answerRow;
        private TableRow _emailRow;
        private TableRow _passwordCompareRow;
        private TableRow _passwordRegExpRow;
        private TableRow _emailRegExpRow;
        private TableRow _passwordTableRow;
        private TableRow _confirmPasswordTableRow;

        private const bool _displaySideBarDefaultValue = false;


        /// <devdoc>
        ///     Creates a new instance of a CreateUserWizard.
        /// </devdoc>
        public CreateUserWizard()
            : base(_displaySideBarDefaultValue) {
        }

        #region Public Properties

        [
        DefaultValue(0),
        ]
        public override int ActiveStepIndex {
            get {
                return base.ActiveStepIndex;
            }
            set {
                base.ActiveStepIndex = value;
            }
        }



        /// <devdoc>
        ///     Gets or sets the initial value in the answer textbox.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        DefaultValue(""),
        Themeable(false),
        WebSysDescription(SR.CreateUserWizard_Answer)
        ]
        public virtual string Answer {
            get {
                return (_answer == null) ? String.Empty : _answer;
            }
            set {
                _answer = value;
            }
        }

        private string AnswerInternal {
            get {
                string answer = Answer;
                if (String.IsNullOrEmpty(Answer) && _createUserStepContainer != null) {
                    ITextControl answerTextBox = (ITextControl)_createUserStepContainer.AnswerTextBox;
                    if (answerTextBox != null) {
                        answer = answerTextBox.Text;
                    }
                }
                // Pass Null instead of Empty into Membership
                if (String.IsNullOrEmpty(answer)) {
                    answer = null;
                }

                return answer;
            }
        }


        /// <devdoc>
        /// Gets or sets the text that identifies the question textbox.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.CreateUserWizard_DefaultAnswerLabelText),
        WebSysDescription(SR.CreateUserWizard_AnswerLabelText)
        ]
        public virtual string AnswerLabelText {
            get {
                object obj = ViewState["AnswerLabelText"];
                return (obj == null) ? SR.GetString(SR.CreateUserWizard_DefaultAnswerLabelText) : (string)obj;
            }
            set {
                ViewState["AnswerLabelText"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the text to be shown in the validation summary when the answer is empty.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Validation"),
        WebSysDefaultValue(SR.CreateUserWizard_DefaultAnswerRequiredErrorMessage),
        WebSysDescription(SR.LoginControls_AnswerRequiredErrorMessage)
        ]
        public virtual string AnswerRequiredErrorMessage {
            get {
                object obj = ViewState["AnswerRequiredErrorMessage"];
                return (obj == null) ? SR.GetString(SR.CreateUserWizard_DefaultAnswerRequiredErrorMessage) : (string)obj;
            }
            set {
                ViewState["AnswerRequiredErrorMessage"] = value;
            }
        }

        [
        WebCategory("Behavior"),
        DefaultValue(false),
        Themeable(false),
        WebSysDescription(SR.CreateUserWizard_AutoGeneratePassword)
        ]
        public virtual bool AutoGeneratePassword {
            get {
                object obj = ViewState["AutoGeneratePassword"];
                return (obj == null) ? false : (bool)obj;
            }
            set {
                if (AutoGeneratePassword != value) {
                    ViewState["AutoGeneratePassword"] = value;
                    RequiresControlsRecreation();
                }
            }
        }


        /// <devdoc>
        ///     Gets the complete step
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebCategory("Appearance"),
        WebSysDescription(SR.CreateUserWizard_CompleteStep)
        ]
        public CompleteWizardStep CompleteStep {
            get {
                EnsureChildControls();
                return _completeStep;
            }
        }


        /// <devdoc>
        /// The text to be shown after the password has been changed.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.CreateUserWizard_DefaultCompleteSuccessText),
        WebSysDescription(SR.CreateUserWizard_CompleteSuccessText)
        ]
        public virtual string CompleteSuccessText {
            get {
                object obj = ViewState["CompleteSuccessText"];
                return (obj == null) ? SR.GetString(SR.CreateUserWizard_DefaultCompleteSuccessText) : (string)obj;
            }
            set {
                ViewState["CompleteSuccessText"] = value;
            }
        }

        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.CreateUserWizard_CompleteSuccessTextStyle)
        ]
        public TableItemStyle CompleteSuccessTextStyle {
            get {
                if (_completeSuccessTextStyle == null) {
                    _completeSuccessTextStyle = new TableItemStyle();
                    if (IsTrackingViewState) {
                        ((IStateManager)_completeSuccessTextStyle).TrackViewState();
                    }
                }
                return _completeSuccessTextStyle;
            }
        }


        /// <devdoc>
        ///     Gets the confirm new password entered by the user.
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public virtual string ConfirmPassword {
            get {
                return (_confirmPassword == null) ? String.Empty : _confirmPassword;
            }
        }


        /// <devdoc>
        ///     Gets or sets the message that is displayed for confirm password errors
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Validation"),
        WebSysDefaultValue(SR.CreateUserWizard_DefaultConfirmPasswordCompareErrorMessage),
        WebSysDescription(SR.ChangePassword_ConfirmPasswordCompareErrorMessage)
        ]
        public virtual string ConfirmPasswordCompareErrorMessage {
            get {
                object obj = ViewState["ConfirmPasswordCompareErrorMessage"];
                return (obj == null) ? SR.GetString(SR.CreateUserWizard_DefaultConfirmPasswordCompareErrorMessage) : (string)obj;
            }
            set {
                ViewState["ConfirmPasswordCompareErrorMessage"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the text that identifies the new password textbox.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.CreateUserWizard_DefaultConfirmPasswordLabelText),
        WebSysDescription(SR.CreateUserWizard_ConfirmPasswordLabelText)
        ]
        public virtual string ConfirmPasswordLabelText {
            get {
                object obj = ViewState["ConfirmPasswordLabelText"];
                return (obj == null) ? SR.GetString(SR.CreateUserWizard_DefaultConfirmPasswordLabelText) : (string)obj;
            }
            set {
                ViewState["ConfirmPasswordLabelText"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the text to be shown in the validation summary when the confirm password is empty.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Validation"),
        WebSysDefaultValue(SR.CreateUserWizard_DefaultConfirmPasswordRequiredErrorMessage),
        WebSysDescription(SR.LoginControls_ConfirmPasswordRequiredErrorMessage)
        ]
        public virtual string ConfirmPasswordRequiredErrorMessage {
            get {
                object obj = ViewState["ConfirmPasswordRequiredErrorMessage"];
                return (obj == null) ? SR.GetString(SR.CreateUserWizard_DefaultConfirmPasswordRequiredErrorMessage) : (string)obj;
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
                return (obj == null) ? String.Empty : (string)obj;
            }
            set {
                ViewState["ContinueButtonImageUrl"] = value;
            }
        }


        /// <devdoc>
        ///     Gets the style of the continue button.
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.CreateUserWizard_ContinueButtonStyle)
        ]
        public Style ContinueButtonStyle {
            get {
                if (_continueButtonStyle == null) {
                    _continueButtonStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager)_continueButtonStyle).TrackViewState();
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
        WebSysDefaultValue(SR.CreateUserWizard_DefaultContinueButtonText),
        WebSysDescription(SR.CreateUserWizard_ContinueButtonText)
        ]
        public virtual string ContinueButtonText {
            get {
                object obj = ViewState["ContinueButtonText"];
                return (obj == null) ? SR.GetString(SR.CreateUserWizard_DefaultContinueButtonText) : (string)obj;
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
        WebSysDescription(SR.CreateUserWizard_ContinueButtonType)
        ]
        public virtual ButtonType ContinueButtonType {
            get {
                object obj = ViewState["ContinueButtonType"];
                return (obj == null) ? ButtonType.Button : (ButtonType)obj;
            }
            set {
                if (value < ButtonType.Button || value > ButtonType.Link) {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (value != ContinueButtonType) {
                    ViewState["ContinueButtonType"] = value;
                }
            }
        }


        /// <devdoc>
        ///     Gets or sets the URL of an image to be displayed for the continue button.
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
                return (obj == null) ? String.Empty : (string)obj;
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
        ///     Gets the create user step
        /// </devdoc>
        [
        WebCategory("Appearance"),
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.CreateUserWizard_CreateUserStep)
        ]
        public CreateUserWizardStep CreateUserStep {
            get {
                EnsureChildControls();
                return _createUserStep;
            }
        }


        /// <devdoc>
        ///     Gets or sets the URL of an image to be displayed for the create user button.
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescription(SR.CreateUserWizard_CreateUserButtonImageUrl),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty()
        ]
        public virtual string CreateUserButtonImageUrl {
            get {
                object obj = ViewState["CreateUserButtonImageUrl"];
                return (obj == null) ? String.Empty : (string)obj;
            }
            set {
                ViewState["CreateUserButtonImageUrl"] = value;
            }
        }


        /// <devdoc>
        ///     Gets the style of the createUser button.
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.CreateUserWizard_CreateUserButtonStyle)
        ]
        public Style CreateUserButtonStyle {
            get {
                if (_createUserButtonStyle == null) {
                    _createUserButtonStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager)_createUserButtonStyle).TrackViewState();
                    }
                }
                return _createUserButtonStyle;
            }
        }


        /// <devdoc>
        ///     Gets or sets the text to be shown for the continue button.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.CreateUserWizard_DefaultCreateUserButtonText),
        WebSysDescription(SR.CreateUserWizard_CreateUserButtonText)
        ]
        public virtual string CreateUserButtonText {
            get {
                object obj = ViewState["CreateUserButtonText"];
                return (obj == null) ? SR.GetString(SR.CreateUserWizard_DefaultCreateUserButtonText) : (string)obj;
            }
            set {
                ViewState["CreateUserButtonText"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the type of the continue button.
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(ButtonType.Button),
        WebSysDescription(SR.CreateUserWizard_CreateUserButtonType)
        ]
        public virtual ButtonType CreateUserButtonType {
            get {
                object obj = ViewState["CreateUserButtonType"];
                return (obj == null) ? ButtonType.Button : (ButtonType)obj;
            }
            set {
                if (value < ButtonType.Button || value > ButtonType.Link) {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (value != CreateUserButtonType) {
                    ViewState["CreateUserButtonType"] = value;
                }
            }
        }

        private bool DefaultCreateUserStep {
            get {
                CreateUserWizardStep step = CreateUserStep;
                return (step == null) ? false : step.ContentTemplate == null;
            }
        }

        private bool DefaultCompleteStep {
            get {
                CompleteWizardStep step = CompleteStep;
                return (step == null) ? false : step.ContentTemplate == null;
            }
        }


        /// <devdoc>
        ///     Gets or sets whether the created user will be disabled
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(false),
        Themeable(false),
        WebSysDescription(SR.CreateUserWizard_DisableCreatedUser)
        ]
        public virtual bool DisableCreatedUser {
            get {
                object obj = ViewState["DisableCreatedUser"];
                return (obj == null) ? false : (bool)obj;
            }
            set {
                ViewState["DisableCreatedUser"] = value;
            }
        }


        [
        DefaultValue(false)
        ]
        public override bool DisplaySideBar {
            get {
                return base.DisplaySideBar;
            }
            set {
                base.DisplaySideBar = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the message that is displayed for duplicate emails
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.CreateUserWizard_DefaultDuplicateEmailErrorMessage),
        WebSysDescription(SR.CreateUserWizard_DuplicateEmailErrorMessage)
        ]
        public virtual string DuplicateEmailErrorMessage {
            get {
                object obj = ViewState["DuplicateEmailErrorMessage"];
                return (obj == null) ? SR.GetString(SR.CreateUserWizard_DefaultDuplicateEmailErrorMessage) : (string)obj;
            }
            set {
                ViewState["DuplicateEmailErrorMessage"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the message that is displayed for email errors
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.CreateUserWizard_DefaultDuplicateUserNameErrorMessage),
        WebSysDescription(SR.CreateUserWizard_DuplicateUserNameErrorMessage)
        ]
        public virtual string DuplicateUserNameErrorMessage {
            get {
                object obj = ViewState["DuplicateUserNameErrorMessage"];
                return (obj == null) ? SR.GetString(SR.CreateUserWizard_DefaultDuplicateUserNameErrorMessage) : (string)obj;
            }
            set {
                ViewState["DuplicateUserNameErrorMessage"] = value;
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
                return (obj == null) ? String.Empty : (string)obj;
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
        WebSysDescription(SR.CreateUserWizard_EditProfileText)
        ]
        public virtual string EditProfileText {
            get {
                object obj = ViewState["EditProfileText"];
                return (obj == null) ? String.Empty : (string)obj;
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
        WebSysDescription(SR.CreateUserWizard_EditProfileUrl),
        Editor("System.Web.UI.Design.UrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty()
        ]
        public virtual string EditProfileUrl {
            get {
                object obj = ViewState["EditProfileUrl"];
                return (obj == null) ? String.Empty : (string)obj;
            }
            set {
                ViewState["EditProfileUrl"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the initial value in the Email textbox.
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescription(SR.CreateUserWizard_Email)
        ]
        public virtual string Email {
            get {
                object obj = ViewState["Email"];
                return (obj == null) ? String.Empty : (string)obj;
            }
            set {
                ViewState["Email"] = value;
            }
        }

        private string EmailInternal {
            get {
                string email = Email;
                if (String.IsNullOrEmpty(email) && _createUserStepContainer != null) {
                    ITextControl emailTextBox = (ITextControl)_createUserStepContainer.EmailTextBox;
                    if (emailTextBox != null) {
                        return emailTextBox.Text;
                    }
                }
                return email;
            }
        }


        /// <devdoc>
        /// Gets or sets the text that identifies the email textbox.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.CreateUserWizard_DefaultEmailLabelText),
        WebSysDescription(SR.CreateUserWizard_EmailLabelText)
        ]
        public virtual string EmailLabelText {
            get {
                object obj = ViewState["EmailLabelText"];
                return (obj == null) ? SR.GetString(SR.CreateUserWizard_DefaultEmailLabelText) : (string)obj;
            }
            set {
                ViewState["EmailLabelText"] = value;
            }
        }


        /// <devdoc>
        ///     Regular expression used to validate the email address
        /// </devdoc>
        [
        WebCategory("Validation"),
        WebSysDefaultValue(""),
        WebSysDescription(SR.CreateUserWizard_EmailRegularExpression)
        ]
        public virtual string EmailRegularExpression {
            get {
                object obj = ViewState["EmailRegularExpression"];
                return (obj == null) ? String.Empty : (string)obj;
            }
            set {
                ViewState["EmailRegularExpression"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the text to be shown in the validation summary when the email fails the reg exp.
        /// </devdoc>
        [
        WebCategory("Validation"),
        WebSysDefaultValue(SR.CreateUserWizard_DefaultEmailRegularExpressionErrorMessage),
        WebSysDescription(SR.CreateUserWizard_EmailRegularExpressionErrorMessage)
        ]
        public virtual string EmailRegularExpressionErrorMessage {
            get {
                object obj = ViewState["EmailRegularExpressionErrorMessage"];
                return (obj == null) ? SR.GetString(SR.CreateUserWizard_DefaultEmailRegularExpressionErrorMessage) : (string)obj;
            }
            set {
                ViewState["EmailRegularExpressionErrorMessage"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the text to be shown in the validation summary when the email is empty.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Validation"),
        WebSysDefaultValue(SR.CreateUserWizard_DefaultEmailRequiredErrorMessage),
        WebSysDescription(SR.CreateUserWizard_EmailRequiredErrorMessage)
        ]
        public virtual string EmailRequiredErrorMessage {
            get {
                object obj = ViewState["EmailRequiredErrorMessage"];
                return (obj == null) ? SR.GetString(SR.CreateUserWizard_DefaultEmailRequiredErrorMessage) : (string)obj;
            }
            set {
                ViewState["EmailRequiredErrorMessage"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the text that is displayed for unknown errors
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.CreateUserWizard_DefaultUnknownErrorMessage),
        WebSysDescription(SR.CreateUserWizard_UnknownErrorMessage)
        ]
        public virtual string UnknownErrorMessage {
            get {
                object obj = ViewState["UnknownErrorMessage"];
                return (obj == null) ? SR.GetString(SR.CreateUserWizard_DefaultUnknownErrorMessage) : (string)obj;
            }
            set {
                ViewState["UnknownErrorMessage"] = value;
            }
        }

        /// <devdoc>
        ///     Gets the style of the error message.
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.CreateUserWizard_ErrorMessageStyle)
        ]
        public TableItemStyle ErrorMessageStyle {
            get {
                if (_errorMessageStyle == null) {
                    _errorMessageStyle = new ErrorTableItemStyle();
                    if (IsTrackingViewState) {
                        ((IStateManager)_errorMessageStyle).TrackViewState();
                    }
                }
                return _errorMessageStyle;
            }
        }


        /// <devdoc>
        /// Gets or sets the URL of an image to be displayed for the help link.
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
                return (obj == null) ? String.Empty : (string)obj;
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
                return (obj == null) ? String.Empty : (string)obj;
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
                return (obj == null) ? String.Empty : (string)obj;
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
                        ((IStateManager)_hyperLinkStyle).TrackViewState();
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
                return (obj == null) ? String.Empty : (string)obj;
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
                        ((IStateManager)_instructionTextStyle).TrackViewState();
                    }
                }
                return _instructionTextStyle;
            }
        }


        /// <devdoc>
        ///     Gets or sets the message that is displayed for answer errors
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.CreateUserWizard_DefaultInvalidAnswerErrorMessage),
        WebSysDescription(SR.CreateUserWizard_InvalidAnswerErrorMessage)
        ]
        public virtual string InvalidAnswerErrorMessage {
            get {
                object obj = ViewState["InvalidAnswerErrorMessage"];
                return (obj == null) ? SR.GetString(SR.CreateUserWizard_DefaultInvalidAnswerErrorMessage) : (string)obj;
            }
            set {
                ViewState["InvalidAnswerErrorMessage"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the message that is displayed for email errors
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.CreateUserWizard_DefaultInvalidEmailErrorMessage),
        WebSysDescription(SR.CreateUserWizard_InvalidEmailErrorMessage)
        ]
        public virtual string InvalidEmailErrorMessage {
            get {
                object obj = ViewState["InvalidEmailErrorMessage"];
                return (obj == null) ? SR.GetString(SR.CreateUserWizard_DefaultInvalidEmailErrorMessage) : (string)obj;
            }
            set {
                ViewState["InvalidEmailErrorMessage"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the text to be shown there is a problem with the password.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.CreateUserWizard_DefaultInvalidPasswordErrorMessage),
        WebSysDescription(SR.CreateUserWizard_InvalidPasswordErrorMessage)
        ]
        public virtual string InvalidPasswordErrorMessage {
            get {
                object obj = ViewState["InvalidPasswordErrorMessage"];
                return (obj == null) ? SR.GetString(SR.CreateUserWizard_DefaultInvalidPasswordErrorMessage) : (string)obj;
            }
            set {
                ViewState["InvalidPasswordErrorMessage"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the message that is displayed for question errors
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.CreateUserWizard_DefaultInvalidQuestionErrorMessage),
        WebSysDescription(SR.CreateUserWizard_InvalidQuestionErrorMessage)
        ]
        public virtual string InvalidQuestionErrorMessage {
            get {
                object obj = ViewState["InvalidQuestionErrorMessage"];
                return (obj == null) ? SR.GetString(SR.CreateUserWizard_DefaultInvalidQuestionErrorMessage) : (string)obj;
            }
            set {
                ViewState["InvalidQuestionErrorMessage"] = value;
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
                        ((IStateManager)_labelStyle).TrackViewState();
                    }
                }
                return _labelStyle;
            }
        }


        /// <devdoc>
        ///     Gets or sets whether the created user will be logged into the site
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(true),
        Themeable(false),
        WebSysDescription(SR.CreateUserWizard_LoginCreatedUser)
        ]
        public virtual bool LoginCreatedUser {
            get {
                object obj = ViewState["LoginCreatedUser"];
                return (obj == null) ? true : (bool)obj;
            }
            set {
                ViewState["LoginCreatedUser"] = value;
            }
        }


        /// <devdoc>
        /// The content and format of the e-mail message that contains the new password.
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        Themeable(false),
        WebSysDescription(SR.CreateUserWizard_MailDefinition)
        ]
        public MailDefinition MailDefinition {
            get {
                if (_mailDefinition == null) {
                    _mailDefinition = new MailDefinition();
                    if (IsTrackingViewState) {
                        ((IStateManager)_mailDefinition).TrackViewState();
                    }
                }
                return _mailDefinition;
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
                return (obj == null) ? String.Empty : (string)obj;
            }
            set {
                if (MembershipProvider != value) {
                    ViewState["MembershipProvider"] = value;
                    RequiresControlsRecreation();
                }
            }
        }


        /// <devdoc>
        ///     Gets the new password entered by the user.
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
                if (String.IsNullOrEmpty(password) && !AutoGeneratePassword && _createUserStepContainer != null) {
                    ITextControl passwordTextBox = (ITextControl)_createUserStepContainer.PasswordTextBox;
                    if (passwordTextBox != null) {
                        return passwordTextBox.Text;
                    }
                }
                return password;
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
        WebSysDescription(SR.CreateUserWizard_PasswordHintStyle)
        ]
        public TableItemStyle PasswordHintStyle {
            get {
                if (_passwordHintStyle == null) {
                    _passwordHintStyle = new TableItemStyle();
                    if (IsTrackingViewState) {
                        ((IStateManager)_passwordHintStyle).TrackViewState();
                    }
                }
                return _passwordHintStyle;
            }
        }


        /// <devdoc>
        ///     Gets or sets the text for the password hint.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(""),
        WebSysDescription(SR.ChangePassword_PasswordHintText)
        ]
        public virtual string PasswordHintText {
            get {
                object obj = ViewState["PasswordHintText"];
                return (obj == null) ? String.Empty : (string)obj;
            }
            set {
                ViewState["PasswordHintText"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the text that identifies the new password textbox.
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
                return (obj == null) ? SR.GetString(SR.LoginControls_DefaultPasswordLabelText) : (string)obj;
            }
            set {
                ViewState["PasswordLabelText"] = value;
            }
        }


        /// <devdoc>
        ///     Regular expression used to validate the new password
        /// </devdoc>
        [
        WebCategory("Validation"),
        WebSysDefaultValue(""),
        WebSysDescription(SR.CreateUserWizard_PasswordRegularExpression)
        ]
        public virtual string PasswordRegularExpression {
            get {
                object obj = ViewState["PasswordRegularExpression"];
                return (obj == null) ? String.Empty : (string)obj;
            }
            set {
                ViewState["PasswordRegularExpression"] = value;
            }
        }



        /// <devdoc>
        ///     Gets or sets the text to be shown in the validation summary when the password fails the reg exp.
        /// </devdoc>
        [
        WebCategory("Validation"),
        WebSysDefaultValue(SR.Password_InvalidPasswordErrorMessage),
        WebSysDescription(SR.CreateUserWizard_PasswordRegularExpressionErrorMessage)
        ]
        public virtual string PasswordRegularExpressionErrorMessage {
            get {
                object obj = ViewState["PasswordRegularExpressionErrorMessage"];
                return (obj == null) ? SR.GetString(SR.Password_InvalidPasswordErrorMessage) : (string)obj;
            }
            set {
                ViewState["PasswordRegularExpressionErrorMessage"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the text to be shown in the validation summary when the new password is empty.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Validation"),
        WebSysDefaultValue(SR.CreateUserWizard_DefaultPasswordRequiredErrorMessage),
        WebSysDescription(SR.CreateUserWizard_PasswordRequiredErrorMessage)
        ]
        public virtual string PasswordRequiredErrorMessage {
            get {
                object obj = ViewState["PasswordRequiredErrorMessage"];
                return (obj == null) ? SR.GetString(SR.CreateUserWizard_DefaultPasswordRequiredErrorMessage) : (string)obj;
            }
            set {
                ViewState["PasswordRequiredErrorMessage"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the initial value in the question textbox.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        DefaultValue(""),
        Themeable(false),
        WebSysDescription(SR.CreateUserWizard_Question)
        ]
        public virtual string Question {
            get {
                object obj = ViewState["Question"];
                return (obj == null) ? String.Empty : (string)obj;
            }
            set {
                ViewState["Question"] = value;
            }
        }

        private string QuestionInternal {
            get {
                string question = Question;
                if (String.IsNullOrEmpty(question) && _createUserStepContainer != null) {
                    ITextControl questionTextBox = (ITextControl)_createUserStepContainer.QuestionTextBox;
                    if (questionTextBox != null) {
                        question = questionTextBox.Text;
                    }
                }
                // Pass Null instead of Empty into Membership
                if (String.IsNullOrEmpty(question)) question = null;
                return question;
            }
        }


        /// <devdoc>
        ///     Gets whether an security question and answer is required to create the user
        /// </devdoc>
        [
        WebCategory("Validation"),
        DefaultValue(true),
        WebSysDescription(SR.CreateUserWizard_QuestionAndAnswerRequired)
        ]
        protected internal bool QuestionAndAnswerRequired {
            get {
                if (DesignMode) {
                    // Don't require question and answer if the CreateUser step is templated in the designer
                    if (CreateUserStep != null && CreateUserStep.ContentTemplate != null) {
                        return false;
                    }
                    return true;
                }
                return LoginUtil.GetProvider(MembershipProvider).RequiresQuestionAndAnswer;
            }
        }


        /// <devdoc>
        /// Gets or sets the text that identifies the question textbox.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.CreateUserWizard_DefaultQuestionLabelText),
        WebSysDescription(SR.CreateUserWizard_QuestionLabelText)
        ]
        public virtual string QuestionLabelText {
            get {
                object obj = ViewState["QuestionLabelText"];
                return (obj == null) ? SR.GetString(SR.CreateUserWizard_DefaultQuestionLabelText) : (string)obj;
            }
            set {
                ViewState["QuestionLabelText"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the text to be shown in the validation summary when the question is empty.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Validation"),
        WebSysDefaultValue(SR.CreateUserWizard_DefaultQuestionRequiredErrorMessage),
        WebSysDescription(SR.CreateUserWizard_QuestionRequiredErrorMessage)
        ]
        public virtual string QuestionRequiredErrorMessage {
            get {
                object obj = ViewState["QuestionRequiredErrorMessage"];
                return (obj == null) ? SR.GetString(SR.CreateUserWizard_DefaultQuestionRequiredErrorMessage) : (string)obj;
            }
            set {
                ViewState["QuestionRequiredErrorMessage"] = value;
            }
        }



        /// <devdoc>
        ///     Gets whether an email address is required to create the user
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(true),
        Themeable(false),
        WebSysDescription(SR.CreateUserWizard_RequireEmail)
        ]
        public virtual bool RequireEmail {
            get {
                object obj = ViewState["RequireEmail"];
                return (obj == null) ? true : (bool)obj;
            }
            set {
                if (RequireEmail != value) {
                    ViewState["RequireEmail"] = value;
                }
            }
        }

        internal override bool ShowCustomNavigationTemplate {
            get {
                if (base.ShowCustomNavigationTemplate) return true;
                return (ActiveStep == CreateUserStep);
            }
        }

        [
        DefaultValue(""),
        ]
        public override string SkipLinkText {
            get {
                string s = SkipLinkTextInternal;
                return s == null ? String.Empty : s;
            }
            set {
                base.SkipLinkText = value;
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
                        ((IStateManager)_textBoxStyle).TrackViewState();
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
                        ((IStateManager)_titleTextStyle).TrackViewState();
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
                if (String.IsNullOrEmpty(userName) && _createUserStepContainer != null) {
                    ITextControl userNameTextBox = (ITextControl)_createUserStepContainer.UserNameTextBox;
                    if (userNameTextBox != null) {
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
        WebSysDefaultValue(SR.CreateUserWizard_DefaultUserNameLabelText),
        WebSysDescription(SR.LoginControls_UserNameLabelText)
        ]
        public virtual string UserNameLabelText {
            get {
                object obj = ViewState["UserNameLabelText"];
                return (obj == null) ? SR.GetString(SR.CreateUserWizard_DefaultUserNameLabelText) : (string)obj;
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
        WebSysDefaultValue(SR.CreateUserWizard_DefaultUserNameRequiredErrorMessage),
        WebSysDescription(SR.ChangePassword_UserNameRequiredErrorMessage)
        ]
        public virtual string UserNameRequiredErrorMessage {
            get {
                object obj = ViewState["UserNameRequiredErrorMessage"];
                return (obj == null) ? SR.GetString(SR.CreateUserWizard_DefaultUserNameRequiredErrorMessage) : (string)obj;
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
        WebSysDescription(SR.CreateUserWizard_ValidatorTextStyle)
        ]
        public Style ValidatorTextStyle {
            get {
                if (_validatorTextStyle == null) {
                    _validatorTextStyle = new ErrorStyle();
                    if (IsTrackingViewState) {
                        ((IStateManager)_validatorTextStyle).TrackViewState();
                    }
                }
                return _validatorTextStyle;
            }
        }

        private string ValidationGroup {
            get {
                if (_validationGroup == null) {
                    EnsureID();
                    _validationGroup = ID;
                }

                return _validationGroup;
            }
        }


        [
        Editor("System.Web.UI.Design.WebControls.CreateUserWizardStepCollectionEditor," + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        ]
        public override WizardStepCollection WizardSteps {
            get {
                return base.WizardSteps;
            }
        }

        #endregion

        #region Public Events

        /// <devdoc>
        ///     Raised on the click of the continue button.
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.CreateUserWizard_ContinueButtonClick)
        ]
        public event EventHandler ContinueButtonClick {
            add {
                Events.AddHandler(EventButtonContinueClick, value);
            }
            remove {
                Events.RemoveHandler(EventButtonContinueClick, value);
            }
        }


        /// <devdoc>
        ///     Raised before a user is created.
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.CreateUserWizard_CreatingUser)
        ]
        public event LoginCancelEventHandler CreatingUser {
            add {
                Events.AddHandler(EventCreatingUser, value);
            }
            remove {
                Events.RemoveHandler(EventCreatingUser, value);
            }
        }


        /// <devdoc>
        ///     Raised after a user is created.
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.CreateUserWizard_CreatedUser)
        ]
        public event EventHandler CreatedUser {
            add {
                Events.AddHandler(EventCreatedUser, value);
            }
            remove {
                Events.RemoveHandler(EventCreatedUser, value);
            }
        }


        /// <devdoc>
        ///     Raised on a create user error
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.CreateUserWizard_CreateUserError)
        ]
        public event CreateUserErrorEventHandler CreateUserError {
            add {
                Events.AddHandler(EventCreateUserError, value);
            }
            remove {
                Events.RemoveHandler(EventCreateUserError, value);
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
        WebSysDescription(SR.CreateUserWizard_SendMailError)
        ]
        public event SendMailErrorEventHandler SendMailError {
            add {
                Events.AddHandler(EventSendMailError, value);
            }
            remove {
                Events.RemoveHandler(EventSendMailError, value);
            }
        }

        #endregion

        private void AnswerTextChanged(object source, EventArgs e) {
            Answer = ((ITextControl)source).Text;
        }

        /// <devdoc>
        ///     Sets the properties of child controls that are editable by the client.
        /// </devdoc>
        private void ApplyCommonCreateUserValues() {
            // We need to use Internal for the DropDownList case where it won't fire a TextChanged for the first item
            if (!String.IsNullOrEmpty(UserNameInternal)) {
                ITextControl userNameTextBox = (ITextControl)_createUserStepContainer.UserNameTextBox;
                if (userNameTextBox != null) {
                    userNameTextBox.Text = UserNameInternal;
                }
            }

            if (!String.IsNullOrEmpty(EmailInternal)) {
                ITextControl emailTextBox = (ITextControl)_createUserStepContainer.EmailTextBox;
                if (emailTextBox != null) {
                    emailTextBox.Text = EmailInternal;
                }
            }

            if (!String.IsNullOrEmpty(QuestionInternal)) {
                ITextControl questionTextBox = (ITextControl)_createUserStepContainer.QuestionTextBox;
                if (questionTextBox != null) {
                    questionTextBox.Text = QuestionInternal;
                }
            }

            if (!String.IsNullOrEmpty(AnswerInternal)) {
                ITextControl answerTextBox = (ITextControl)_createUserStepContainer.AnswerTextBox;
                if (answerTextBox != null) {
                    answerTextBox.Text = AnswerInternal;
                }
            }
        }

        private void ApplyDefaultCreateUserValues() {
            _createUserStepContainer.UserNameLabel.Text = UserNameLabelText;
            WebControl userTextBox = (WebControl)_createUserStepContainer.UserNameTextBox;
            userTextBox.TabIndex = TabIndex;
            userTextBox.AccessKey = AccessKey;

            _createUserStepContainer.PasswordLabel.Text = PasswordLabelText;
            WebControl passwordTextBox = (WebControl)_createUserStepContainer.PasswordTextBox;
            passwordTextBox.TabIndex = TabIndex;

            _createUserStepContainer.ConfirmPasswordLabel.Text = ConfirmPasswordLabelText;
            WebControl confirmTextBox = (WebControl)_createUserStepContainer.ConfirmPasswordTextBox;
            confirmTextBox.TabIndex = TabIndex;

            if (_textBoxStyle != null) {
                userTextBox.ApplyStyle(_textBoxStyle);
                passwordTextBox.ApplyStyle(_textBoxStyle);
                confirmTextBox.ApplyStyle(_textBoxStyle);
            }

            LoginUtil.ApplyStyleToLiteral(_createUserStepContainer.Title, CreateUserStep.Title, TitleTextStyle, true);
            LoginUtil.ApplyStyleToLiteral(_createUserStepContainer.InstructionLabel, InstructionText, InstructionTextStyle, true);
            LoginUtil.ApplyStyleToLiteral(_createUserStepContainer.UserNameLabel, UserNameLabelText, LabelStyle, false);
            LoginUtil.ApplyStyleToLiteral(_createUserStepContainer.PasswordLabel, PasswordLabelText, LabelStyle, false);
            LoginUtil.ApplyStyleToLiteral(_createUserStepContainer.ConfirmPasswordLabel, ConfirmPasswordLabelText, LabelStyle, false);

            // VSWhidbey 447805 Do not render PasswordHintText if AutoGeneratePassword is false.
            if (!String.IsNullOrEmpty(PasswordHintText) && !AutoGeneratePassword) {
                LoginUtil.ApplyStyleToLiteral(_createUserStepContainer.PasswordHintLabel, PasswordHintText, PasswordHintStyle, false);
            } else {
                _passwordHintTableRow.Visible = false;
            }

            bool enableValidation = true;

            WebControl emailTextBox = null;
            if (RequireEmail) {
                LoginUtil.ApplyStyleToLiteral(_createUserStepContainer.EmailLabel, EmailLabelText, LabelStyle, false);
                emailTextBox = (WebControl)_createUserStepContainer.EmailTextBox;
                ((ITextControl)emailTextBox).Text = Email;
                RequiredFieldValidator emailRequired = _createUserStepContainer.EmailRequired;
                emailRequired.ToolTip = EmailRequiredErrorMessage;
                emailRequired.ErrorMessage = EmailRequiredErrorMessage;
                emailRequired.Enabled = enableValidation;
                emailRequired.Visible = enableValidation;
                if (_validatorTextStyle != null) {
                    emailRequired.ApplyStyle(_validatorTextStyle);
                }

                emailTextBox.TabIndex = TabIndex;
                if (_textBoxStyle != null) {
                    emailTextBox.ApplyStyle(_textBoxStyle);
                }
            } else {
                _emailRow.Visible = false;
            }

            WebControl questionTextBox = null;
            WebControl answerTextBox = null;
            RequiredFieldValidator questionRequired = _createUserStepContainer.QuestionRequired;
            RequiredFieldValidator answerRequired = _createUserStepContainer.AnswerRequired;
            bool qaValidatorsEnabled = enableValidation && QuestionAndAnswerRequired;
            questionRequired.Enabled = qaValidatorsEnabled;
            questionRequired.Visible = qaValidatorsEnabled;
            answerRequired.Enabled = qaValidatorsEnabled;
            answerRequired.Visible = qaValidatorsEnabled;
            if (QuestionAndAnswerRequired) {
                LoginUtil.ApplyStyleToLiteral(_createUserStepContainer.QuestionLabel, QuestionLabelText, LabelStyle, false);
                questionTextBox = (WebControl)_createUserStepContainer.QuestionTextBox;
                ((ITextControl)questionTextBox).Text = Question;
                questionTextBox.TabIndex = TabIndex;
                LoginUtil.ApplyStyleToLiteral(_createUserStepContainer.AnswerLabel, AnswerLabelText, LabelStyle, false);

                answerTextBox = (WebControl)_createUserStepContainer.AnswerTextBox;
                ((ITextControl)answerTextBox).Text = Answer;
                answerTextBox.TabIndex = TabIndex;

                if (_textBoxStyle != null) {
                    questionTextBox.ApplyStyle(_textBoxStyle);
                    answerTextBox.ApplyStyle(_textBoxStyle);
                }

                questionRequired.ToolTip = QuestionRequiredErrorMessage;
                questionRequired.ErrorMessage = QuestionRequiredErrorMessage;

                answerRequired.ToolTip = AnswerRequiredErrorMessage;
                answerRequired.ErrorMessage = AnswerRequiredErrorMessage;

                if (_validatorTextStyle != null) {
                    questionRequired.ApplyStyle(_validatorTextStyle);
                    answerRequired.ApplyStyle(_validatorTextStyle);
                }
            } else {
                _questionRow.Visible = false;
                _answerRow.Visible = false;
            }

            if (_defaultCreateUserNavigationTemplate != null) {
                ((BaseNavigationTemplateContainer)(CreateUserStep.CustomNavigationTemplateContainer)).NextButton = _defaultCreateUserNavigationTemplate.CreateUserButton;
                ((BaseNavigationTemplateContainer)(CreateUserStep.CustomNavigationTemplateContainer)).CancelButton = _defaultCreateUserNavigationTemplate.CancelButton;
            }

            RequiredFieldValidator passwordRequired = _createUserStepContainer.PasswordRequired;
            RequiredFieldValidator confirmPasswordRequired = _createUserStepContainer.ConfirmPasswordRequired;
            CompareValidator passwordCompareValidator = _createUserStepContainer.PasswordCompareValidator;
            RegularExpressionValidator regExpValidator = _createUserStepContainer.PasswordRegExpValidator;
            bool passwordValidatorsEnabled = enableValidation && !AutoGeneratePassword;
            passwordRequired.Enabled = passwordValidatorsEnabled;
            passwordRequired.Visible = passwordValidatorsEnabled;
            confirmPasswordRequired.Enabled = passwordValidatorsEnabled;
            confirmPasswordRequired.Visible = passwordValidatorsEnabled;
            passwordCompareValidator.Enabled = passwordValidatorsEnabled;
            passwordCompareValidator.Visible = passwordValidatorsEnabled;

            bool passRegExpEnabled = passwordValidatorsEnabled && PasswordRegularExpression.Length > 0;
            regExpValidator.Enabled = passRegExpEnabled;
            regExpValidator.Visible = passRegExpEnabled;

            if (!enableValidation) {
                _passwordRegExpRow.Visible = false;
                _passwordCompareRow.Visible = false;
                _emailRegExpRow.Visible = false;
            }

            if (AutoGeneratePassword) {
                _passwordTableRow.Visible = false;
                _confirmPasswordTableRow.Visible = false;
                _passwordRegExpRow.Visible = false;
                _passwordCompareRow.Visible = false;
            } else {
                passwordRequired.ErrorMessage = PasswordRequiredErrorMessage;
                passwordRequired.ToolTip = PasswordRequiredErrorMessage;

                confirmPasswordRequired.ErrorMessage = ConfirmPasswordRequiredErrorMessage;
                confirmPasswordRequired.ToolTip = ConfirmPasswordRequiredErrorMessage;

                passwordCompareValidator.ErrorMessage = ConfirmPasswordCompareErrorMessage;

                if (_validatorTextStyle != null) {
                    passwordRequired.ApplyStyle(_validatorTextStyle);
                    confirmPasswordRequired.ApplyStyle(_validatorTextStyle);
                    passwordCompareValidator.ApplyStyle(_validatorTextStyle);
                }

                if (passRegExpEnabled) {
                    regExpValidator.ValidationExpression = PasswordRegularExpression;
                    regExpValidator.ErrorMessage = PasswordRegularExpressionErrorMessage;
                    if (_validatorTextStyle != null) {
                        regExpValidator.ApplyStyle(_validatorTextStyle);
                    }
                } else {
                    _passwordRegExpRow.Visible = false;

                }
            }

            RequiredFieldValidator userNameRequired = _createUserStepContainer.UserNameRequired;
            userNameRequired.ErrorMessage = UserNameRequiredErrorMessage;
            userNameRequired.ToolTip = UserNameRequiredErrorMessage;
            userNameRequired.Enabled = enableValidation;
            userNameRequired.Visible = enableValidation;
            if (_validatorTextStyle != null) {
                userNameRequired.ApplyStyle(_validatorTextStyle);
            }

            bool emailRegExpEnabled = enableValidation && EmailRegularExpression.Length > 0 && RequireEmail;
            RegularExpressionValidator emailRegExpValidator = _createUserStepContainer.EmailRegExpValidator;
            emailRegExpValidator.Enabled = emailRegExpEnabled;
            emailRegExpValidator.Visible = emailRegExpEnabled;
            if (EmailRegularExpression.Length > 0 && RequireEmail) {
                emailRegExpValidator.ValidationExpression = EmailRegularExpression;
                emailRegExpValidator.ErrorMessage = EmailRegularExpressionErrorMessage;
                if (_validatorTextStyle != null) {
                    emailRegExpValidator.ApplyStyle(_validatorTextStyle);
                }
            } else {
                _emailRegExpRow.Visible = false;
            }

            // Link Setup
            string helpPageText = HelpPageText;
            bool helpPageTextVisible = (helpPageText.Length > 0);

            HyperLink helpPageLink = _createUserStepContainer.HelpPageLink;
            Image helpPageIcon = _createUserStepContainer.HelpPageIcon;
            helpPageLink.Visible = helpPageTextVisible;
            if (helpPageTextVisible) {
                helpPageLink.Text = helpPageText;
                helpPageLink.NavigateUrl = HelpPageUrl;
                helpPageLink.TabIndex = TabIndex;
            }
            string helpPageIconUrl = HelpPageIconUrl;
            bool helpPageIconVisible = (helpPageIconUrl.Length > 0);
            helpPageIcon.Visible = helpPageIconVisible;
            if (helpPageIconVisible) {
                helpPageIcon.ImageUrl = helpPageIconUrl;
                helpPageIcon.AlternateText = helpPageText;
            }
            LoginUtil.SetTableCellVisible(helpPageLink, helpPageTextVisible || helpPageIconVisible);
            if (_hyperLinkStyle != null && (helpPageTextVisible || helpPageIconVisible)) {
                // Apply style except font to table cell, then apply font and forecolor to HyperLinks
                // VSWhidbey 81289
                TableItemStyle hyperLinkStyleExceptFont = new TableItemStyle();
                hyperLinkStyleExceptFont.CopyFrom(_hyperLinkStyle);
                hyperLinkStyleExceptFont.Font.Reset();
                LoginUtil.SetTableCellStyle(helpPageLink, hyperLinkStyleExceptFont);
                helpPageLink.Font.CopyFrom(_hyperLinkStyle.Font);
                helpPageLink.ForeColor = _hyperLinkStyle.ForeColor;
            }

            Control errorMessageLabel = _createUserStepContainer.ErrorMessageLabel;
            if (errorMessageLabel != null) {
                if (_failure && !String.IsNullOrEmpty(_unknownErrorMessage)) {
                    ((ITextControl)errorMessageLabel).Text = _unknownErrorMessage;
                    LoginUtil.SetTableCellStyle(errorMessageLabel, ErrorMessageStyle);
                    LoginUtil.SetTableCellVisible(errorMessageLabel, true);
                } else {
                    LoginUtil.SetTableCellVisible(errorMessageLabel, false);
                }
            }
        }

        private void ApplyCompleteValues() {
            LoginUtil.ApplyStyleToLiteral(_completeStepContainer.SuccessTextLabel, CompleteSuccessText, _completeSuccessTextStyle, true);

            switch (ContinueButtonType) {
                case ButtonType.Link:
                    _completeStepContainer.ContinuePushButton.Visible = false;
                    _completeStepContainer.ContinueImageButton.Visible = false;
                    _completeStepContainer.ContinueLinkButton.Text = ContinueButtonText;
                    _completeStepContainer.ContinueLinkButton.ValidationGroup = ValidationGroup;
                    _completeStepContainer.ContinueLinkButton.TabIndex = TabIndex;
                    _completeStepContainer.ContinueLinkButton.AccessKey = AccessKey;
                    break;
                case ButtonType.Button:
                    _completeStepContainer.ContinueLinkButton.Visible = false;
                    _completeStepContainer.ContinueImageButton.Visible = false;
                    _completeStepContainer.ContinuePushButton.Text = ContinueButtonText;
                    _completeStepContainer.ContinuePushButton.ValidationGroup = ValidationGroup;
                    _completeStepContainer.ContinuePushButton.TabIndex = TabIndex;
                    _completeStepContainer.ContinuePushButton.AccessKey = AccessKey;
                    break;
                case ButtonType.Image:
                    _completeStepContainer.ContinueLinkButton.Visible = false;
                    _completeStepContainer.ContinuePushButton.Visible = false;
                    _completeStepContainer.ContinueImageButton.ImageUrl = ContinueButtonImageUrl;
                    _completeStepContainer.ContinueImageButton.AlternateText = ContinueButtonText;
                    _completeStepContainer.ContinueImageButton.ValidationGroup = ValidationGroup;
                    _completeStepContainer.ContinueImageButton.TabIndex = TabIndex;
                    _completeStepContainer.ContinueImageButton.AccessKey = AccessKey;
                    break;
            }

            if (!NavigationButtonStyle.IsEmpty) {
                _completeStepContainer.ContinuePushButton.ApplyStyle(NavigationButtonStyle);
                _completeStepContainer.ContinueImageButton.ApplyStyle(NavigationButtonStyle);
                _completeStepContainer.ContinueLinkButton.ApplyStyle(NavigationButtonStyle);
            }

            if (_continueButtonStyle != null) {
                _completeStepContainer.ContinuePushButton.ApplyStyle(_continueButtonStyle);
                _completeStepContainer.ContinueImageButton.ApplyStyle(_continueButtonStyle);
                _completeStepContainer.ContinueLinkButton.ApplyStyle(_continueButtonStyle);
            }

            LoginUtil.ApplyStyleToLiteral(_completeStepContainer.Title, CompleteStep.Title, _titleTextStyle, true);

            string editProfileText = EditProfileText;
            bool editProfileVisible = (editProfileText.Length > 0);
            HyperLink editProfileLink = _completeStepContainer.EditProfileLink;
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
            string editProfileIconUrl = EditProfileIconUrl;
            bool editProfileIconVisible = (editProfileIconUrl.Length > 0);
            Image editProfileIcon = _completeStepContainer.EditProfileIcon;
            editProfileIcon.Visible = editProfileIconVisible;
            if (editProfileIconVisible) {
                editProfileIcon.ImageUrl = editProfileIconUrl;
                editProfileIcon.AlternateText = EditProfileText;
            }
            LoginUtil.SetTableCellVisible(editProfileLink, editProfileVisible || editProfileIconVisible);

            // Copy the styles from the StepStyle property if defined.
            Table table = ((CompleteStepContainer)(CompleteStep.ContentTemplateContainer)).LayoutTable;
            table.Height = Height;
            table.Width = Width;
        }

        /// <devdoc>
        ///     Attempts to create a user, returns false if unsuccessful
        /// </devdoc>
        private bool AttemptCreateUser() {
            if (Page != null && !Page.IsValid) {
                return false;
            }

            LoginCancelEventArgs args = new LoginCancelEventArgs();
            OnCreatingUser(args);
            if (args.Cancel) return false;

            MembershipProvider provider = LoginUtil.GetProvider(MembershipProvider);
            MembershipCreateStatus status;

            if (AutoGeneratePassword) {
                int length = Math.Max(10, Membership.MinRequiredPasswordLength);
                _password = Membership.GeneratePassword(length, Membership.MinRequiredNonAlphanumericCharacters);
            }

            // CreateUser() should not throw an exception.  Status is returned through the out parameter.
            provider.CreateUser(UserNameInternal, PasswordInternal, EmailInternal, QuestionInternal, AnswerInternal, !DisableCreatedUser /*isApproved*/, null, out status);
            if (status == MembershipCreateStatus.Success) {
                OnCreatedUser(EventArgs.Empty);

                // Send mail if specified
                if (_mailDefinition != null && !String.IsNullOrEmpty(EmailInternal)) {
                    LoginUtil.SendPasswordMail(EmailInternal, UserNameInternal, PasswordInternal, MailDefinition,
                        /*defaultSubject*/ null, /*defaultBody*/ null, OnSendingMail, OnSendMailError,
                                               this);
                }

                // Set AllowReturn to false now that we've created the user
                CreateUserStep.AllowReturnInternal = false;

                // Set the logged in cookie if required
                if (LoginCreatedUser) {
                    AttemptLogin();
                }

                return true;
            } else {
                // Failed to create user handling below.
                // Raise the error first so users get a chance to change the failure text.
                OnCreateUserError(new CreateUserErrorEventArgs(status));

                switch (status) {
                    case MembershipCreateStatus.DuplicateEmail:
                        _unknownErrorMessage = DuplicateEmailErrorMessage;
                        break;
                    case MembershipCreateStatus.DuplicateUserName:
                        _unknownErrorMessage = DuplicateUserNameErrorMessage;
                        break;
                    case MembershipCreateStatus.InvalidAnswer:
                        _unknownErrorMessage = InvalidAnswerErrorMessage;
                        break;
                    case MembershipCreateStatus.InvalidEmail:
                        _unknownErrorMessage = InvalidEmailErrorMessage;
                        break;
                    case MembershipCreateStatus.InvalidQuestion:
                        _unknownErrorMessage = InvalidQuestionErrorMessage;
                        break;
                    case MembershipCreateStatus.InvalidPassword:
                        string invalidPasswordErrorMessage = InvalidPasswordErrorMessage;
                        if (!String.IsNullOrEmpty(invalidPasswordErrorMessage)) {
                            invalidPasswordErrorMessage = String.Format(CultureInfo.InvariantCulture, invalidPasswordErrorMessage,
                                provider.MinRequiredPasswordLength, provider.MinRequiredNonAlphanumericCharacters);
                        }
                        _unknownErrorMessage = invalidPasswordErrorMessage;
                        break;
                    default:
                        _unknownErrorMessage = UnknownErrorMessage;
                        break;
                }

                return false;
            }
        }

        private void AttemptLogin() {
            // Try to authenticate the user
            MembershipProvider provider = LoginUtil.GetProvider(MembershipProvider);

            // ValidateUser() should not throw an exception.
            if (provider.ValidateUser(UserName, Password)) {
                System.Web.Security.FormsAuthentication.SetAuthCookie(UserNameInternal, false);
            }
        }

        private void ConfirmPasswordTextChanged(object source, EventArgs e) {
            if (!AutoGeneratePassword) {
                _confirmPassword = ((ITextControl)source).Text;
            }
        }


        /// <internalonly />
        /// <devdoc>
        /// </devdoc>
        protected internal override void CreateChildControls() {
            _createUserStep = null;
            _completeStep = null;

            base.CreateChildControls();
            UpdateValidators();
        }


        private void RegisterEvents() {
            RegisterTextChangedEvent(_createUserStepContainer.UserNameTextBox, UserNameTextChanged);
            RegisterTextChangedEvent(_createUserStepContainer.EmailTextBox, EmailTextChanged);
            RegisterTextChangedEvent(_createUserStepContainer.QuestionTextBox, QuestionTextChanged);
            RegisterTextChangedEvent(_createUserStepContainer.AnswerTextBox, AnswerTextChanged);
            RegisterTextChangedEvent(_createUserStepContainer.PasswordTextBox, PasswordTextChanged);
            RegisterTextChangedEvent(_createUserStepContainer.ConfirmPasswordTextBox, ConfirmPasswordTextChanged);
        }

        private static void RegisterTextChangedEvent(Control control, Action<object, EventArgs> textChangedHandler) {
            var textBoxControl = control as IEditableTextControl;
            if (textBoxControl != null) {
                textBoxControl.TextChanged += new EventHandler(textChangedHandler);
            }
        }

        internal override Wizard.TableWizardRendering CreateTableRendering() {
            return new TableWizardRendering(this);
        }

        internal override Wizard.LayoutTemplateWizardRendering CreateLayoutTemplateRendering() {
            return new LayoutTemplateWizardRendering(this);
        }

        internal override ITemplate CreateDefaultSideBarTemplate() {
            return new DefaultSideBarTemplate();
        }

        internal override ITemplate CreateDefaultDataListItemTemplate() {
            return new DataListItemTemplate();
        }

        #region Control creation helpers

        private static TableRow CreateTwoColumnRow(Control leftCellControl, params Control[] rightCellControls) {
            var row = CreateTableRow();

            var leftCell = CreateTableCell();
            leftCell.HorizontalAlign = HorizontalAlign.Right;
            leftCell.Controls.Add(leftCellControl);
            row.Cells.Add(leftCell);

            var rightCell = CreateTableCell();
            foreach (var control in rightCellControls) {
                rightCell.Controls.Add(control);
            }
            row.Cells.Add(rightCell);

            return row;
        }

        private static TableRow CreateDoubleSpannedColumnRow(params Control[] cellControls) {
            return CreateDoubleSpannedColumnRow(null /* cellHorizontalAlignment */, cellControls);
        }

        private static TableRow CreateDoubleSpannedColumnRow(HorizontalAlign? cellHorizontalAlignment, params Control[] cellControls) {
            var row = CreateTableRow();

            var cell = CreateTableCell();
            cell.ColumnSpan = 2;
            if (cellHorizontalAlignment.HasValue) {
                cell.HorizontalAlign = cellHorizontalAlignment.Value;
            }
            foreach (var control in cellControls) {
                cell.Controls.Add(control);
            }
            row.Cells.Add(cell);

            return row;
        }


        /// <devdoc>
        ///     Helper function to create a literal with auto id disabled
        /// </devdoc>
        private static LabelLiteral CreateLabelLiteral(Control control) {
            LabelLiteral lit = new LabelLiteral(control);
            lit.PreventAutoID();
            return lit;
        }

        /// <devdoc>
        ///     Helper function to create a literal with auto id disabled
        /// </devdoc>
        private static Literal CreateLiteral() {
            Literal lit = new Literal();
            lit.PreventAutoID();
            return lit;
        }

        /// <devdoc>
        ///     Helper function to create and set properties for a required field validator
        /// </devdoc>
        private static RequiredFieldValidator CreateRequiredFieldValidator(string id, string validationGroup, Control targetTextBox, bool enableValidation) {
            RequiredFieldValidator validator = new RequiredFieldValidator() {
                ID = id,
                ControlToValidate = targetTextBox.ID,
                ValidationGroup = validationGroup,
                Display = _requiredFieldValidatorDisplay,
                Text = SR.GetString(SR.LoginControls_DefaultRequiredFieldValidatorText),
                Enabled = enableValidation,
                Visible = enableValidation
            };
            return validator;
        }

        /// <devdoc>
        ///     Helper function to create a table with auto id disabled
        /// </devdoc>
        private static Table CreateTable() {
            Table table = new Table();
            table.Width = Unit.Percentage(100);
            table.Height = Unit.Percentage(100);
            table.PreventAutoID();
            return table;
        }

        /// <devdoc>
        ///     Helper function to create a table cell with auto id disabled
        /// </devdoc>
        private static TableCell CreateTableCell() {
            TableCell cell = new TableCell();
            cell.PreventAutoID();
            return cell;
        }

        /// <devdoc>
        ///     Helper function to create a table row with auto id disabled
        /// </devdoc>
        private static TableRow CreateTableRow() {
            TableRow row = new LoginUtil.DisappearingTableRow();
            row.PreventAutoID();
            return row;
        }

        #endregion

        // Helper method to create custom navigation templates.
        internal override void CreateCustomNavigationTemplates() {
            // 
            for (int i = 0; i < WizardSteps.Count; ++i) {
                TemplatedWizardStep step = WizardSteps[i] as TemplatedWizardStep;
                if (step != null) {
                    string id = GetCustomContainerID(i);
                    BaseNavigationTemplateContainer container = CreateBaseNavigationTemplateContainer(id);
                    if (step.CustomNavigationTemplate != null) {
                        step.CustomNavigationTemplate.InstantiateIn(container);
                        step.CustomNavigationTemplateContainer = container;
                        container.SetEnableTheming();
                    } else if (step == CreateUserStep) {
                        ITemplate customNavigationTemplate = new DefaultCreateUserNavigationTemplate(this);
                        customNavigationTemplate.InstantiateIn(container);
                        step.CustomNavigationTemplateContainer = container;
                        container.RegisterButtonCommandEvents();
                    }
                    CustomNavigationContainers[step] = container;
                }
            }
        }

        internal override void DataListItemDataBound(object sender, WizardSideBarListControlItemEventArgs e) {
            var dataListItem = e.Item;

            // Ignore the item that is not created from DataSource
            if (dataListItem.ItemType != ListItemType.Item &&
                dataListItem.ItemType != ListItemType.AlternatingItem &&
                dataListItem.ItemType != ListItemType.SelectedItem &&
                dataListItem.ItemType != ListItemType.EditItem) {
                return;
            }

            // For VSWhidbey 193022, we have to support wiring up sidebar buttons in sidebar templates
            // so use the base implementation for this.
            IButtonControl button = dataListItem.FindControl(SideBarButtonID) as IButtonControl;
            if (button != null) {
                base.DataListItemDataBound(sender, e);
                return;
            }

            Label label = dataListItem.FindControl(_sideBarLabelID) as Label;
            if (label == null) {
                if (!DesignMode) {
                    throw new InvalidOperationException(
                        SR.GetString(SR.CreateUserWizard_SideBar_Label_Not_Found, DataListID, _sideBarLabelID));
                }

                return;
            }

            // Apply the button style to the side bar label.
            label.MergeStyle(SideBarButtonStyle);

            // Render wizardstep title on the button control.
            WizardStepBase step = dataListItem.DataItem as WizardStepBase;
            if (step != null) {
                // Need to render the sidebar tablecell.
                RegisterSideBarDataListForRender();

                // Use the step title if defined, otherwise use ID
                if (step.Title.Length > 0) {
                    label.Text = step.Title;
                } else {
                    label.Text = step.ID;
                }
            }
        }

        private void EmailTextChanged(object source, EventArgs e) {
            Email = ((ITextControl)source).Text;
        }

        /// <devdoc>
        ///     Creates the default steps if they were not specified declaritively
        /// </devdoc>
        private void EnsureCreateUserSteps() {
            bool foundCreate = false;
            bool foundComplete = false;
            foreach (WizardStepBase step in WizardSteps) {
                var createUserStep = step as CreateUserWizardStep;
                if (createUserStep != null) {
                    if (foundCreate) {
                        throw new HttpException(SR.GetString(SR.CreateUserWizard_DuplicateCreateUserWizardStep));
                    }

                    foundCreate = true;
                    _createUserStep = createUserStep;
                } else {
                    var completeStep = step as CompleteWizardStep;
                    if (completeStep != null) {
                        if (foundComplete) {
                            throw new HttpException(SR.GetString(SR.CreateUserWizard_DuplicateCompleteWizardStep));
                        }

                        foundComplete = true;
                        _completeStep = completeStep;
                    }
                }
            }
            if (!foundCreate) {
                // This default step cannot disable ViewState, otherwise AllowReturn will not work properly.
                // VSWhidbey 459041
                _createUserStep = new CreateUserWizardStep();
                // Internally created control needs to be themed as well. VSWhidbey 377952
                _createUserStep.ApplyStyleSheetSkin(Page);
                WizardSteps.AddAt(0, _createUserStep);
                _createUserStep.Active = true;
            }
            if (!foundComplete) {
                // This default step cannot disable ViewState, otherwise AllowReturn will not work properly.
                // VSWhidbey 459041
                _completeStep = new CompleteWizardStep();
                // Internally created control needs to be themed as well. VSWhidbey 377952
                _completeStep.ApplyStyleSheetSkin(Page);
                WizardSteps.Add(_completeStep);
            }
            if (ActiveStepIndex == -1) ActiveStepIndex = 0;
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        protected override IDictionary GetDesignModeState() {
            IDictionary dictionary = base.GetDesignModeState();

            WizardStepBase active = ActiveStep;
            if (active != null && active == CreateUserStep) {
                dictionary[_customNavigationControls] = CustomNavigationContainers[ActiveStep].Controls;
            }

            // Needed so the failure text label is visible in the designer
            Control errorMessageLabel = _createUserStepContainer.ErrorMessageLabel;
            if (errorMessageLabel != null) {
                LoginUtil.SetTableCellVisible(errorMessageLabel, true);
            }

            return dictionary;
        }

        /// <devdoc>
        ///     Instantiates all the content templates for each TemplatedWizardStep
        /// </devdoc>
        internal override void InstantiateStepContentTemplates() {
            bool useInnerTable = (LayoutTemplate == null);
            foreach (WizardStepBase step in WizardSteps) {
                if (step == CreateUserStep) {
                    step.Controls.Clear();
                    _createUserStepContainer = new CreateUserStepContainer(this, useInnerTable);
                    _createUserStepContainer.ID = _createUserStepContainerID;
                    ITemplate createUserStepTemplate = CreateUserStep.ContentTemplate;
                    if (createUserStepTemplate == null) {
                        createUserStepTemplate = new DefaultCreateUserContentTemplate(this);
                    } else {
                        _createUserStepContainer.SetEnableTheming();
                    }
                    createUserStepTemplate.InstantiateIn(_createUserStepContainer.Container);

                    CreateUserStep.ContentTemplateContainer = _createUserStepContainer;
                    step.Controls.Add(_createUserStepContainer);
                } else if (step == CompleteStep) {
                    step.Controls.Clear();
                    _completeStepContainer = new CompleteStepContainer(this, useInnerTable);
                    _completeStepContainer.ID = _completeStepContainerID;
                    ITemplate completeStepTemplate = CompleteStep.ContentTemplate;
                    if (completeStepTemplate == null) {
                        completeStepTemplate = new DefaultCompleteStepContentTemplate(_completeStepContainer);
                    }
                    else {
                        _completeStepContainer.SetEnableTheming();
                    }
                    completeStepTemplate.InstantiateIn(_completeStepContainer.Container);

                    CompleteStep.ContentTemplateContainer = _completeStepContainer;
                    step.Controls.Add(_completeStepContainer);
                } else {
                    var templatedStep = step as TemplatedWizardStep;
                    if (templatedStep != null) {
                        InstantiateStepContentTemplate(templatedStep);
                    }
                }
            }
        }

        /// <internalonly/>
        /// <devdoc>
        ///     Loads a saved state of the <see cref='System.Web.UI.WebControls.CreateUserWizard'/>.
        /// </devdoc>
        protected override void LoadViewState(object savedState) {
            if (savedState == null) {
                base.LoadViewState(null);
            } else {
                object[] myState = (object[])savedState;
                if (myState.Length != _viewStateArrayLength) {
                    throw new ArgumentException(SR.GetString(SR.ViewState_InvalidViewState));
                }

                base.LoadViewState(myState[0]);
                if (myState[1] != null) {
                    ((IStateManager)CreateUserButtonStyle).LoadViewState(myState[1]);
                }
                if (myState[2] != null) {
                    ((IStateManager)LabelStyle).LoadViewState(myState[2]);
                }
                if (myState[3] != null) {
                    ((IStateManager)TextBoxStyle).LoadViewState(myState[3]);
                }
                if (myState[4] != null) {
                    ((IStateManager)HyperLinkStyle).LoadViewState(myState[4]);
                }
                if (myState[5] != null) {
                    ((IStateManager)InstructionTextStyle).LoadViewState(myState[5]);
                }
                if (myState[6] != null) {
                    ((IStateManager)TitleTextStyle).LoadViewState(myState[6]);
                }
                if (myState[7] != null) {
                    ((IStateManager)ErrorMessageStyle).LoadViewState(myState[7]);
                }
                if (myState[8] != null) {
                    ((IStateManager)PasswordHintStyle).LoadViewState(myState[8]);
                }
                if (myState[9] != null) {
                    ((IStateManager)MailDefinition).LoadViewState(myState[9]);
                }
                if (myState[10] != null) {
                    ((IStateManager)ContinueButtonStyle).LoadViewState(myState[10]);
                }
                if (myState[11] != null) {
                    ((IStateManager)CompleteSuccessTextStyle).LoadViewState(myState[11]);
                }
                if (myState[12] != null) {
                    ((IStateManager)ValidatorTextStyle).LoadViewState(myState[12]);
                }
            }

            UpdateValidators();
        }

        // Call this whenever ChildControlsAreCreated to ensure we clean up the old validators
        private void UpdateValidators() {
            if (DesignMode) {
                return;
            }

            // Because we create our child controls during on init, we need to remove validators
            // from the page potentially that were created mistakenly before viewstate was loaded
            if (DefaultCreateUserStep && _createUserStepContainer != null) {
                // Remove the validators that aren't required when autogenerating a password
                if (AutoGeneratePassword) {
                    BaseValidator confirmPassword = _createUserStepContainer.ConfirmPasswordRequired;
                    if (confirmPassword != null) {
                        Page.Validators.Remove(confirmPassword);
                        confirmPassword.Enabled = false;
                    }
                    BaseValidator passwordRequired = _createUserStepContainer.PasswordRequired;
                    if (passwordRequired != null) {
                        Page.Validators.Remove(passwordRequired);
                        passwordRequired.Enabled = false;
                    }
                    BaseValidator passRegExp = _createUserStepContainer.PasswordRegExpValidator;
                    if (passRegExp != null) {
                        Page.Validators.Remove(passRegExp);
                        passRegExp.Enabled = false;
                    }
                } else if (PasswordRegularExpression.Length <= 0) {
                    BaseValidator passRegExp = _createUserStepContainer.PasswordRegExpValidator;
                    if (passRegExp != null) {
                        if (Page != null) {
                            Page.Validators.Remove(passRegExp);
                        }
                        passRegExp.Enabled = false;
                    }
                }

                // remove the validators from the page if we don't require email
                if (!RequireEmail) {
                    BaseValidator emailRequired = _createUserStepContainer.EmailRequired;
                    if (emailRequired != null) {
                        if (Page != null) {
                            Page.Validators.Remove(emailRequired);
                        }
                        emailRequired.Enabled = false;
                    }
                    BaseValidator emailRegExp = _createUserStepContainer.EmailRegExpValidator;
                    if (emailRegExp != null) {
                        if (Page != null) {
                            Page.Validators.Remove(emailRegExp);
                        }
                        emailRegExp.Enabled = false;
                    }
                } else if (EmailRegularExpression.Length <= 0) {
                    BaseValidator emailRegExp = _createUserStepContainer.EmailRegExpValidator;
                    if (emailRegExp != null) {
                        if (Page != null) {
                            Page.Validators.Remove(emailRegExp);
                        }
                        emailRegExp.Enabled = false;
                    }
                }

                // remove the validators from the page if we don't require question and answer
                if (!QuestionAndAnswerRequired) {
                    BaseValidator questionRequired = _createUserStepContainer.QuestionRequired;
                    if (questionRequired != null) {
                        if (Page != null) {
                            Page.Validators.Remove(questionRequired);
                        }
                        questionRequired.Enabled = false;
                    }

                    BaseValidator answerRequired = _createUserStepContainer.AnswerRequired;
                    if (answerRequired != null) {
                        if (Page != null) {
                            Page.Validators.Remove(answerRequired);
                        }
                        answerRequired.Enabled = false;
                    }

                }
            }
        }


        protected override bool OnBubbleEvent(object source, EventArgs e) {
            // Intercept continue button clicks here
            CommandEventArgs ce = e as CommandEventArgs;
            if (ce != null) {
                if (ce.CommandName.Equals(ContinueButtonCommandName, StringComparison.CurrentCultureIgnoreCase)) {
                    OnContinueButtonClick(EventArgs.Empty);
                    return true;
                }
            }
            return base.OnBubbleEvent(source, e);
        }


        /// <devdoc>
        ///     Raises the ContinueClick event.
        /// </devdoc>
        protected virtual void OnContinueButtonClick(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventButtonContinueClick];
            if (handler != null) {
                handler(this, e);
            }

            string continuePageUrl = ContinueDestinationPageUrl;
            if (!String.IsNullOrEmpty(continuePageUrl)) {
                // we should not terminate execution of current page, to give
                // page a chance to cleanup its resources.  This may be less performant though.
                Page.Response.Redirect(ResolveClientUrl(continuePageUrl), false);
            }
        }


        /// <devdoc>
        ///     Raises the CreatedUser event.
        /// </devdoc>
        protected virtual void OnCreatedUser(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventCreatedUser];
            if (handler != null) {
                handler(this, e);
            }
        }


        /// <devdoc>
        ///     Raises the CreateUserError event.
        /// </devdoc>
        protected virtual void OnCreateUserError(CreateUserErrorEventArgs e) {
            CreateUserErrorEventHandler handler = (CreateUserErrorEventHandler)Events[EventCreateUserError];
            if (handler != null) {
                handler(this, e);
            }
        }


        /// <devdoc>
        ///     Raises the CreatingUser event.
        /// </devdoc>
        protected virtual void OnCreatingUser(LoginCancelEventArgs e) {
            LoginCancelEventHandler handler = (LoginCancelEventHandler)Events[EventCreatingUser];
            if (handler != null) {
                handler(this, e);
            }
        }

        protected override void OnNextButtonClick(WizardNavigationEventArgs e) {
            // If they just clicked next on CreateUser, lets try creating
            if (WizardSteps[e.CurrentStepIndex] == _createUserStep) {
                e.Cancel = (Page != null && !Page.IsValid);

                if (!e.Cancel) {
                    // Cancel the event if there's a failure
                    _failure = !AttemptCreateUser();
                    if (_failure) {
                        e.Cancel = true;
                        ITextControl errorMessageLabel = (ITextControl)_createUserStepContainer.ErrorMessageLabel;
                        if (errorMessageLabel != null && !String.IsNullOrEmpty(_unknownErrorMessage)) {
                            errorMessageLabel.Text = _unknownErrorMessage;

                            var errorMessageLabelCtrl = errorMessageLabel as Control;
                            if (errorMessageLabelCtrl != null) {
                                errorMessageLabelCtrl.Visible = true;
                            }
                        }
                    }
                }
            }

            base.OnNextButtonClick(e);
        }


        protected internal override void OnPreRender(EventArgs e) {
            // Done for some error checking (no duplicate createuserwizard/complete steps)
            EnsureCreateUserSteps();
            base.OnPreRender(e);

            // VSWhidbey 438108 Make sure the MembershipProvider exists.
            string providerString = MembershipProvider;
            if (!String.IsNullOrEmpty(providerString)) {
                MembershipProvider provider = Membership.Providers[providerString];
                if (provider == null) {
                    throw new HttpException(SR.GetString(SR.WebControl_CantFindProvider));
                }
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
            if (!AutoGeneratePassword) {
                _password = ((ITextControl)source).Text;
            }
        }

        private void QuestionTextChanged(object source, EventArgs e) {
            Question = ((ITextControl)source).Text;
        }

        /// <internalonly/>
        /// <devdoc>
        ///     Saves the state of the <see cref='System.Web.UI.WebControls.CreateUserWizard'/>.
        /// </devdoc>
        protected override object SaveViewState() {
            object[] myState = new object[_viewStateArrayLength];

            myState[0] = base.SaveViewState();
            myState[1] = (_createUserButtonStyle != null) ? ((IStateManager)_createUserButtonStyle).SaveViewState() : null;
            myState[2] = (_labelStyle != null) ? ((IStateManager)_labelStyle).SaveViewState() : null;
            myState[3] = (_textBoxStyle != null) ? ((IStateManager)_textBoxStyle).SaveViewState() : null;
            myState[4] = (_hyperLinkStyle != null) ? ((IStateManager)_hyperLinkStyle).SaveViewState() : null;
            myState[5] =
                (_instructionTextStyle != null) ? ((IStateManager)_instructionTextStyle).SaveViewState() : null;
            myState[6] = (_titleTextStyle != null) ? ((IStateManager)_titleTextStyle).SaveViewState() : null;
            myState[7] =
                (_errorMessageStyle != null) ? ((IStateManager)_errorMessageStyle).SaveViewState() : null;
            myState[8] = (_passwordHintStyle != null) ? ((IStateManager)_passwordHintStyle).SaveViewState() : null;
            myState[9] = (_mailDefinition != null) ? ((IStateManager)_mailDefinition).SaveViewState() : null;
            myState[10] = (_continueButtonStyle != null) ? ((IStateManager)_continueButtonStyle).SaveViewState() : null;
            myState[11] = (_completeSuccessTextStyle != null) ? ((IStateManager)_completeSuccessTextStyle).SaveViewState() : null;
            myState[12] = (_validatorTextStyle != null) ? ((IStateManager)_validatorTextStyle).SaveViewState() : null;

            for (int i = 0; i < _viewStateArrayLength; i++) {
                if (myState[i] != null) {
                    return myState;
                }
            }

            // More performant to return null than an array of null values
            return null;
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        protected override void SetDesignModeState(IDictionary data) {
            if (data != null) {
                object o = data["ConvertToTemplate"];
                if (o != null) {
                    _convertingToTemplate = (bool)o;
                }
            }
        }

        private void SetChildProperties() {
            ApplyCommonCreateUserValues();
            if (DefaultCreateUserStep) {
                ApplyDefaultCreateUserValues();
            }

            if (DefaultCompleteStep) {
                ApplyCompleteValues();
            }

            // Always try to apply the failure text, even if templated
            Control errorMessageLabel = _createUserStepContainer.ErrorMessageLabel;
            if (errorMessageLabel != null) {
                if (_failure && !String.IsNullOrEmpty(_unknownErrorMessage)) {
                    ((ITextControl)errorMessageLabel).Text = _unknownErrorMessage;
                    errorMessageLabel.Visible = true;
                } else {
                    errorMessageLabel.Visible = false;
                }
            }
        }

        private void SetDefaultCreateUserNavigationTemplateProperties() {
            Debug.Assert(_defaultCreateUserNavigationTemplate != null);

            WebControl createUserButton = (WebControl)_defaultCreateUserNavigationTemplate.CreateUserButton;
            WebControl previousButton = (WebControl)_defaultCreateUserNavigationTemplate.PreviousButton;
            WebControl cancelButton = (WebControl)_defaultCreateUserNavigationTemplate.CancelButton;

            _defaultCreateUserNavigationTemplate.ApplyLayoutStyleToInnerCells(NavigationStyle);

            //int createUserStepIndex = WizardSteps.IndexOf(CreateUserStep);
            var createUserButtonControl = (IButtonControl)createUserButton;
            createUserButtonControl.CausesValidation = true;
            createUserButtonControl.Text = CreateUserButtonText;
            createUserButtonControl.ValidationGroup = ValidationGroup;
            var previousButtonControl = (IButtonControl)previousButton;
            previousButtonControl.CausesValidation = false;
            previousButtonControl.Text = StepPreviousButtonText;
            ((IButtonControl)cancelButton).Text = CancelButtonText;

            // Apply styles and tab index to the visible buttons
            if (_createUserButtonStyle != null) createUserButton.ApplyStyle(_createUserButtonStyle);
            createUserButton.ControlStyle.MergeWith(NavigationButtonStyle);
            createUserButton.TabIndex = TabIndex;
            createUserButton.Visible = true;

            var createUserImageButton = createUserButton as ImageButton;
            if (createUserImageButton != null) {
                createUserImageButton.ImageUrl = CreateUserButtonImageUrl;
                createUserImageButton.AlternateText = CreateUserButtonText;
            }

            previousButton.ApplyStyle(StepPreviousButtonStyle);
            previousButton.ControlStyle.MergeWith(NavigationButtonStyle);
            previousButton.TabIndex = TabIndex;

            int previousStepIndex = GetPreviousStepIndex(false);
            if (previousStepIndex != -1 && WizardSteps[previousStepIndex].AllowReturn) {
                previousButton.Visible = true;
            } else {
                previousButton.Parent.Visible = false;
            }

            var previousImageButton = previousButton as ImageButton;
            if (previousImageButton != null) {
                previousImageButton.AlternateText = StepPreviousButtonText;
                previousImageButton.ImageUrl = StepPreviousButtonImageUrl;
            }

            if (DisplayCancelButton) {
                cancelButton.ApplyStyle(CancelButtonStyle);
                cancelButton.ControlStyle.MergeWith(NavigationButtonStyle);
                cancelButton.TabIndex = TabIndex;
                cancelButton.Visible = true;

                var cancelImageButton = cancelButton as ImageButton;
                if (cancelImageButton != null) {
                    cancelImageButton.ImageUrl = CancelButtonImageUrl;
                    cancelImageButton.AlternateText = CancelButtonText;
                }
            } else {
                cancelButton.Parent.Visible = false;
            }
        }

        /// <devdoc>
        ///     Marks the starting point to begin tracking and saving changes to the
        ///     control as part of the control viewstate.
        /// </devdoc>
        protected override void TrackViewState() {
            base.TrackViewState();

            if (_createUserButtonStyle != null) {
                ((IStateManager)_createUserButtonStyle).TrackViewState();
            }
            if (_labelStyle != null) {
                ((IStateManager)_labelStyle).TrackViewState();
            }
            if (_textBoxStyle != null) {
                ((IStateManager)_textBoxStyle).TrackViewState();
            }
            if (_hyperLinkStyle != null) {
                ((IStateManager)_hyperLinkStyle).TrackViewState();
            }
            if (_instructionTextStyle != null) {
                ((IStateManager)_instructionTextStyle).TrackViewState();
            }
            if (_titleTextStyle != null) {
                ((IStateManager)_titleTextStyle).TrackViewState();
            }
            if (_errorMessageStyle != null) {
                ((IStateManager)_errorMessageStyle).TrackViewState();
            }
            if (_passwordHintStyle != null) {
                ((IStateManager)_passwordHintStyle).TrackViewState();
            }
            if (_mailDefinition != null) {
                ((IStateManager)_mailDefinition).TrackViewState();
            }
            if (_continueButtonStyle != null) {
                ((IStateManager)_continueButtonStyle).TrackViewState();
            }
            if (_completeSuccessTextStyle != null) {
                ((IStateManager)_completeSuccessTextStyle).TrackViewState();
            }
            if (_validatorTextStyle != null) {
                ((IStateManager)_validatorTextStyle).TrackViewState();
            }
        }

        private void UserNameTextChanged(object source, EventArgs e) {
            UserName = ((ITextControl)source).Text;
        }


        private new class LayoutTemplateWizardRendering : Wizard.LayoutTemplateWizardRendering {
            private new CreateUserWizard Owner { get; set; }

            public LayoutTemplateWizardRendering(CreateUserWizard owner)
                : base(owner) {
                Owner = owner;
            }

            public override void CreateControlHierarchy() {
                Owner.EnsureCreateUserSteps();

                base.CreateControlHierarchy();

                Owner.InstantiateStepContentTemplates();

                Owner.RegisterEvents();

                // Set the editable child control properties here for two reasons:
                // - So change events will be raised if viewstate is disabled on the child controls
                //   - Viewstate is always disabled for default template, and might be for user template
                // - So the controls render correctly in the designer
                Owner.ApplyCommonCreateUserValues();
            }


            public override void ApplyControlProperties() {
                Owner.SetChildProperties();

                if (Owner.CreateUserStep.CustomNavigationTemplate == null) {
                    Owner.SetDefaultCreateUserNavigationTemplateProperties();
                }

                base.ApplyControlProperties();
            }
        }


        private new class TableWizardRendering : Wizard.TableWizardRendering {
            private new CreateUserWizard Owner {
                get;
                set;
            }

            public TableWizardRendering(CreateUserWizard wizard)
                : base(wizard) {
                Owner = wizard;
            }

            public override void ApplyControlProperties() {
                Owner.SetChildProperties();

                if (Owner.CreateUserStep.CustomNavigationTemplate == null) {
                    Owner.SetDefaultCreateUserNavigationTemplateProperties();
                }

                base.ApplyControlProperties();
            }

            public override void CreateControlHierarchy() {
                Owner.EnsureCreateUserSteps();

                base.CreateControlHierarchy();

                Owner.RegisterEvents();

                // Set the editable child control properties here for two reasons:
                // - So change events will be raised if viewstate is disabled on the child controls
                //   - Viewstate is always disabled for default template, and might be for user template
                // - So the controls render correctly in the designer
                Owner.ApplyCommonCreateUserValues();
            }
        }
        
        private sealed class DefaultCompleteStepContentTemplate : ITemplate {
            private CompleteStepContainer _completeContainer;

            public DefaultCompleteStepContentTemplate(CompleteStepContainer container) {
                _completeContainer = container;
            }

            private static void ConstructControls(CompleteStepContainer container) {
                container.Title = CreateLiteral();

                container.SuccessTextLabel = CreateLiteral();

                container.EditProfileLink = new HyperLink() {
                    ID = _editProfileLinkID
                };

                container.EditProfileIcon = new Image();
                container.EditProfileIcon.PreventAutoID();

                container.ContinueLinkButton = new LinkButton() {
                    ID = _continueButtonID + "LinkButton",
                    CommandName = ContinueButtonCommandName,
                    CausesValidation = false,
                };

                container.ContinuePushButton = new Button() {
                    ID = _continueButtonID + "Button",
                    CommandName = ContinueButtonCommandName,
                    CausesValidation = false
                };

                container.ContinueImageButton = new ImageButton() {
                    ID = _continueButtonID + "ImageButton",
                    CommandName = ContinueButtonCommandName,
                    CausesValidation = false
                }; ;
            }

            private static void LayoutControls(CompleteStepContainer container) {
                Table table = CreateTable();
                table.EnableViewState = false;

                AddTitleRow(table, container);
                AddSuccessTextRow(table, container);
                AddContinueRow(table, container);
                AddEditRow(table, container);

                container.LayoutTable = table;
                container.AddChildControl(table);
            }

            private static void AddTitleRow(Table table, CompleteStepContainer container) {
                var r = CreateDoubleSpannedColumnRow(HorizontalAlign.Center, container.Title);
                table.Rows.Add(r);
            }

            private static void AddSuccessTextRow(Table table, CompleteStepContainer container) {
                var r = CreateTableRow();
                var c = CreateTableCell();
                c.Controls.Add(container.SuccessTextLabel);
                r.Cells.Add(c);
                table.Rows.Add(r);
            }

            private static void AddContinueRow(Table table, CompleteStepContainer container) {
                var r = CreateDoubleSpannedColumnRow(HorizontalAlign.Right,
                    container.ContinuePushButton,
                    container.ContinueLinkButton,
                    container.ContinueImageButton);
                table.Rows.Add(r);
            }

            private static void AddEditRow(Table table, CompleteStepContainer container) {
                var r = CreateDoubleSpannedColumnRow(container.EditProfileIcon, container.EditProfileLink);
                table.Rows.Add(r);
            }

            void ITemplate.InstantiateIn(Control container) {
                ConstructControls(_completeContainer);
                LayoutControls(_completeContainer);
            }
        }


        private sealed class DefaultCreateUserContentTemplate : ITemplate {
            private CreateUserWizard _wizard;

            internal DefaultCreateUserContentTemplate(CreateUserWizard wizard) {
                _wizard = wizard;
            }

            private void ConstructControls(CreateUserStepContainer container) {
                string validationGroup = _wizard.ValidationGroup;

                container.Title = CreateLiteral();
                container.InstructionLabel = CreateLiteral();
                container.PasswordHintLabel = CreateLiteral();

                container.UserNameTextBox = new TextBox() {
                    ID = _userNameID
                };

                // Must explicitly set the ID of controls that raise postback events
                container.PasswordTextBox = new TextBox() {
                    ID = _passwordID,
                    TextMode = TextBoxMode.Password
                }; ;

                container.ConfirmPasswordTextBox = new TextBox() {
                    ID = _confirmPasswordID,
                    TextMode = TextBoxMode.Password
                };

                bool enableValidation = true;
                container.UserNameRequired = CreateRequiredFieldValidator(_userNameRequiredID, validationGroup, container.UserNameTextBox, enableValidation);

                container.UserNameLabel = CreateLabelLiteral(container.UserNameTextBox);
                container.PasswordLabel = CreateLabelLiteral(container.PasswordTextBox);
                container.ConfirmPasswordLabel = CreateLabelLiteral(container.ConfirmPasswordTextBox);

                Image helpPageIcon = new Image();
                helpPageIcon.PreventAutoID();
                container.HelpPageIcon = helpPageIcon;

                container.HelpPageLink = new HyperLink() {
                    ID = _helpLinkID
                };

                container.ErrorMessageLabel = new Literal() {
                    ID = _errorMessageID
                };

                container.EmailTextBox = new TextBox() {
                    ID = _emailID
                };

                container.EmailRequired = CreateRequiredFieldValidator(_emailRequiredID, validationGroup, container.EmailTextBox, enableValidation);
                container.EmailLabel = CreateLabelLiteral(container.EmailTextBox);

                container.EmailRegExpValidator = new RegularExpressionValidator() {
                    ID = _emailRegExpID,
                    ControlToValidate = _emailID,
                    ErrorMessage = _wizard.EmailRegularExpressionErrorMessage,
                    ValidationExpression = _wizard.EmailRegularExpression,
                    ValidationGroup = validationGroup,
                    Display = _regexpFieldValidatorDisplay,
                    Enabled = enableValidation,
                    Visible = enableValidation
                };

                container.PasswordRequired = CreateRequiredFieldValidator(_passwordRequiredID, validationGroup, container.PasswordTextBox, enableValidation);
                container.ConfirmPasswordRequired = CreateRequiredFieldValidator(_confirmPasswordRequiredID, validationGroup, container.ConfirmPasswordTextBox, enableValidation);

                container.PasswordRegExpValidator = new RegularExpressionValidator() {
                    ID = _passwordRegExpID,
                    ControlToValidate = _passwordID,
                    ErrorMessage = _wizard.PasswordRegularExpressionErrorMessage,
                    ValidationExpression = _wizard.PasswordRegularExpression,
                    ValidationGroup = validationGroup,
                    Display = _regexpFieldValidatorDisplay,
                    Enabled = enableValidation,
                    Visible = enableValidation,
                };

                container.PasswordCompareValidator = new CompareValidator() {
                    ID = _passwordCompareID,
                    ControlToValidate = _confirmPasswordID,
                    ControlToCompare = _passwordID,
                    Operator = ValidationCompareOperator.Equal,
                    ErrorMessage = _wizard.ConfirmPasswordCompareErrorMessage,
                    ValidationGroup = validationGroup,
                    Display = _compareFieldValidatorDisplay,
                    Enabled = enableValidation,
                    Visible = enableValidation,
                };

                container.QuestionTextBox = new TextBox() {
                    ID = _questionID
                }; ;

                container.AnswerTextBox = new TextBox() {
                    ID = _answerID
                }; ;

                container.QuestionRequired = CreateRequiredFieldValidator(_questionRequiredID, validationGroup, container.QuestionTextBox, enableValidation);
                container.AnswerRequired = CreateRequiredFieldValidator(_answerRequiredID, validationGroup, container.AnswerTextBox, enableValidation);

                container.QuestionLabel = CreateLabelLiteral(container.QuestionTextBox);
                container.AnswerLabel = CreateLabelLiteral(container.AnswerTextBox);
            }

            private void LayoutControls(CreateUserStepContainer container) {
                Table table = CreateTable();
                table.EnableViewState = false;

                AddTitleRow(table, container);
                AddInstructionRow(table, container);
                AddUserNameRow(table, container);
                AddPasswordRow(table, container);
                AddPasswordHintRow(table, container);
                AddConfirmPasswordRow(table, container);
                AddEmailRow(table, container);
                AddQuestionRow(table, container);
                AddAnswerRow(table, container);
                AddPasswordCompareValidatorRow(table, container);
                AddPasswordRegexValidatorRow(table, container);
                AddEmailRegexValidatorRow(table, container);
                AddErrorMessageRow(table, container);
                AddHelpPageLinkRow(table, container);

                container.AddChildControl(table);
            }

            private static void AddTitleRow(Table table, CreateUserStepContainer container) {
                var row = CreateDoubleSpannedColumnRow(HorizontalAlign.Center, container.Title);
                table.Rows.Add(row);
            }

            private static void AddInstructionRow(Table table, CreateUserStepContainer container) {
                var row = CreateDoubleSpannedColumnRow(HorizontalAlign.Center, container.InstructionLabel);
                row.PreventAutoID();
                table.Rows.Add(row);
            }

            private void AddUserNameRow(Table table, CreateUserStepContainer container) {
                if (_wizard.ConvertingToTemplate) {
                    container.UserNameLabel.RenderAsLabel = true;
                }
                var row = CreateTwoColumnRow(container.UserNameLabel, container.UserNameTextBox, container.UserNameRequired);
                table.Rows.Add(row);
            }

            private void AddPasswordRow(Table table, CreateUserStepContainer container) {
                if (_wizard.ConvertingToTemplate) {
                    container.PasswordLabel.RenderAsLabel = true;
                }
                var rightCellColumns = new List<Control>() { container.PasswordTextBox };
                if (!_wizard.AutoGeneratePassword) {
                    rightCellColumns.Add(container.PasswordRequired);
                }
                var row = CreateTwoColumnRow(container.PasswordLabel, rightCellColumns.ToArray());
                _wizard._passwordTableRow = row;
                table.Rows.Add(row);
            }

            private void AddPasswordHintRow(Table table, CreateUserStepContainer container) {
                var row = CreateTableRow();

                var leftCell = CreateTableCell();
                row.Cells.Add(leftCell);

                var rightCell = CreateTableCell();
                rightCell.Controls.Add(container.PasswordHintLabel);
                row.Cells.Add(rightCell);

                _wizard._passwordHintTableRow = row;
                table.Rows.Add(row);
            }

            private void AddConfirmPasswordRow(Table table, CreateUserStepContainer container) {
                if (_wizard.ConvertingToTemplate) {
                    container.ConfirmPasswordLabel.RenderAsLabel = true;
                }
                var rightCellColumns = new List<Control>() { container.ConfirmPasswordTextBox };
                if (!_wizard.AutoGeneratePassword) {
                    rightCellColumns.Add(container.ConfirmPasswordRequired);
                }
                var row = CreateTwoColumnRow(container.ConfirmPasswordLabel, rightCellColumns.ToArray());
                _wizard._confirmPasswordTableRow = row;
                table.Rows.Add(row);
            }

            private void AddEmailRow(Table table, CreateUserStepContainer container) {
                if (_wizard.ConvertingToTemplate) {
                    container.EmailLabel.RenderAsLabel = true;
                }
                var row = CreateTwoColumnRow(container.EmailLabel, container.EmailTextBox, container.EmailRequired);
                _wizard._emailRow = row;
                table.Rows.Add(row);
            }

            private void AddQuestionRow(Table table, CreateUserStepContainer container) {
                if (_wizard.ConvertingToTemplate) {
                    container.QuestionLabel.RenderAsLabel = true;
                }
                var row = CreateTwoColumnRow(container.QuestionLabel, container.QuestionTextBox, container.QuestionRequired);
                _wizard._questionRow = row;
                table.Rows.Add(row);
            }

            private void AddAnswerRow(Table table, CreateUserStepContainer container) {
                if (_wizard.ConvertingToTemplate) {
                    container.AnswerLabel.RenderAsLabel = true;
                }
                var row = CreateTwoColumnRow(container.AnswerLabel, container.AnswerTextBox, container.AnswerRequired);
                _wizard._answerRow = row;
                table.Rows.Add(row);
            }

            private void AddPasswordCompareValidatorRow(Table table, CreateUserStepContainer container) {
                var row = CreateDoubleSpannedColumnRow(HorizontalAlign.Center, container.PasswordCompareValidator);
                _wizard._passwordCompareRow = row;
                table.Rows.Add(row);
            }

            private void AddPasswordRegexValidatorRow(Table table, CreateUserStepContainer container) {
                var row = CreateDoubleSpannedColumnRow(HorizontalAlign.Center, container.PasswordRegExpValidator);
                _wizard._passwordRegExpRow = row;
                table.Rows.Add(row);
            }

            private void AddEmailRegexValidatorRow(Table table, CreateUserStepContainer container) {
                var row = CreateDoubleSpannedColumnRow(HorizontalAlign.Center, container.EmailRegExpValidator);
                _wizard._emailRegExpRow = row;
                table.Rows.Add(row);
            }

            private static void AddErrorMessageRow(Table table, CreateUserStepContainer container) {
                var row = CreateDoubleSpannedColumnRow(HorizontalAlign.Center, container.ErrorMessageLabel);
                table.Rows.Add(row);
            }

            private static void AddHelpPageLinkRow(Table table, CreateUserStepContainer container) {
                var row = CreateDoubleSpannedColumnRow(container.HelpPageIcon, container.HelpPageLink);
                table.Rows.Add(row);
            }

            void ITemplate.InstantiateIn(Control container) {
                var createUserContainer = _wizard._createUserStepContainer;
                ConstructControls(createUserContainer);
                LayoutControls(createUserContainer);
            }
        }

        private sealed class DefaultCreateUserNavigationTemplate : ITemplate {
            private CreateUserWizard _wizard;
            private TableRow _row;
            private IButtonControl[][] _buttons;
            private TableCell[] _innerCells;

            internal DefaultCreateUserNavigationTemplate(CreateUserWizard wizard) {
                _wizard = wizard;
            }

            internal void ApplyLayoutStyleToInnerCells(TableItemStyle tableItemStyle) {
                // VSWhidbey 401891, apply the table layout styles to the innercells.
                for (int i = 0; i < _innerCells.Length; i++) {
                    if (tableItemStyle.IsSet(TableItemStyle.PROP_HORZALIGN)) {
                        _innerCells[i].HorizontalAlign = tableItemStyle.HorizontalAlign;
                    }

                    if (tableItemStyle.IsSet(TableItemStyle.PROP_VERTALIGN)) {
                        _innerCells[i].VerticalAlign = tableItemStyle.VerticalAlign;
                    }
                }
            }

            void ITemplate.InstantiateIn(Control container) {
                _wizard._defaultCreateUserNavigationTemplate = this;
                container.EnableViewState = false;

                Table table = CreateTable();
                table.CellSpacing = 5;
                table.CellPadding = 5;
                container.Controls.Add(table);

                TableRow tableRow = new TableRow();
                _row = tableRow;
                tableRow.PreventAutoID();
                tableRow.HorizontalAlign = HorizontalAlign.Right;
                table.Rows.Add(tableRow);

                _buttons = new IButtonControl[3][];
                _buttons[0] = new IButtonControl[3];
                _buttons[1] = new IButtonControl[3];
                _buttons[2] = new IButtonControl[3];

                _innerCells = new TableCell[3];

                _innerCells[0] = CreateButtonControl(_buttons[0], _wizard.ValidationGroup, Wizard.StepPreviousButtonID, false, Wizard.MovePreviousCommandName);
                _innerCells[1] = CreateButtonControl(_buttons[1], _wizard.ValidationGroup, Wizard.StepNextButtonID, true, Wizard.MoveNextCommandName);
                _innerCells[2] = CreateButtonControl(_buttons[2], _wizard.ValidationGroup, Wizard.CancelButtonID, false, Wizard.CancelCommandName);
            }

            private void OnPreRender(object source, EventArgs e) {
                ((ImageButton)source).Visible = false;
            }

            private TableCell CreateButtonControl(IButtonControl[] buttons, String validationGroup, String id,
                bool causesValidation, string commandName) {

                LinkButton linkButton = new LinkButton() {
                    CausesValidation = causesValidation,
                    ID = id + "LinkButton",
                    Visible = false,
                    CommandName = commandName,
                    ValidationGroup = validationGroup
                };
                buttons[0] = linkButton;

                ImageButton imageButton = new ImageButton() {
                    CausesValidation = causesValidation,
                    ID = id + "ImageButton",
                    // We need the image button to be visible because it OnPreRender is only called on visible controls
                    // for postbacks to work, we don't need this behavior in the designer
                    Visible = !_wizard.DesignMode,
                    CommandName = commandName,
                    ValidationGroup = validationGroup
                };
                imageButton.PreRender += new EventHandler(OnPreRender);

                buttons[1] = imageButton;

                Button button = new Button() {
                    CausesValidation = causesValidation,
                    ID = id + "Button",
                    Visible = false,
                    CommandName = commandName,
                    ValidationGroup = validationGroup,
                };
                buttons[2] = button;

                TableCell tableCell = new TableCell();
                tableCell.HorizontalAlign = HorizontalAlign.Right;
                _row.Cells.Add(tableCell);

                tableCell.Controls.Add(linkButton);
                tableCell.Controls.Add(imageButton);
                tableCell.Controls.Add(button);

                return tableCell;
            }

            internal IButtonControl PreviousButton {
                get {
                    return GetButtonBasedOnType(0, _wizard.StepPreviousButtonType);
                }
            }

            internal IButtonControl CreateUserButton {
                get {
                    return GetButtonBasedOnType(1, _wizard.CreateUserButtonType);
                }
            }

            internal IButtonControl CancelButton {
                get {
                    return GetButtonBasedOnType(2, _wizard.CancelButtonType);
                }
            }

            private IButtonControl GetButtonBasedOnType(int pos, ButtonType type) {
                switch (type) {
                    case ButtonType.Button:
                        return _buttons[pos][2];

                    case ButtonType.Image:
                        return _buttons[pos][1];

                    case ButtonType.Link:
                        return _buttons[pos][0];
                }

                return null;
            }
        }

        private sealed class DataListItemTemplate : ITemplate {
            public void InstantiateIn(Control container) {
                Label item = new Label();
                item.PreventAutoID();
                item.ID = _sideBarLabelID;
                container.Controls.Add(item);
            }
        }

        private sealed class DefaultSideBarTemplate : ITemplate {
            public void InstantiateIn(Control container) {
                DataList dataList = new DataList();
                dataList.ID = Wizard.DataListID;
                container.Controls.Add(dataList);

                dataList.SelectedItemStyle.Font.Bold = true;
                dataList.ItemTemplate = new DataListItemTemplate();
            }
        }

        private sealed class CreateUserStepContainer : BaseContentTemplateContainer {
            private CreateUserWizard _createUserWizard;

            private Control _userNameTextBox;
            private Control _passwordTextBox;
            private Control _confirmPasswordTextBox;
            private Control _emailTextBox;
            private Control _questionTextBox;
            private Control _answerTextBox;

            private Control _unknownErrorMessageLabel;

            internal CreateUserStepContainer(CreateUserWizard wizard, bool useInnerTable)
                : base(wizard, useInnerTable) {
                _createUserWizard = wizard;
            }

            internal LabelLiteral AnswerLabel { get; set; }

            internal RequiredFieldValidator AnswerRequired { get; set; }

            /// <devdoc>
            ///     Required control, must have type IEditableTextControl
            /// </devdoc>
            internal Control AnswerTextBox {
                get {
                    if (_answerTextBox != null) {
                        return _answerTextBox;
                    } else {
                        Control answerTextBox = FindControl(_answerID);
                        if (answerTextBox is IEditableTextControl) {
                            // 
                            return answerTextBox;
                        } else {
                            if (!_createUserWizard.DesignMode && _createUserWizard.QuestionAndAnswerRequired) {
                                throw new HttpException(SR.GetString(SR.CreateUserWizard_NoAnswerTextBox,
                                                                                         _createUserWizard.ID, _answerID));
                            }
                            return null;
                        }
                    }
                }
                set {
                    _answerTextBox = value;
                }
            }

            internal LabelLiteral ConfirmPasswordLabel { get; set; }

            internal RequiredFieldValidator ConfirmPasswordRequired { get; set; }

            internal Control ConfirmPasswordTextBox {
                get {
                    if (_confirmPasswordTextBox != null) {
                        return _confirmPasswordTextBox;
                    } else {
                        Control confirmPasswordTextBox = FindControl(_confirmPasswordID);
                        if (confirmPasswordTextBox is IEditableTextControl) {
                            // 
                            return confirmPasswordTextBox;
                        } else {
                            return null;
                        }
                    }
                }
                set {
                    _confirmPasswordTextBox = value;
                }
            }

            internal LabelLiteral EmailLabel { get; set; }

            internal RegularExpressionValidator EmailRegExpValidator { get; set; }

            internal RequiredFieldValidator EmailRequired { get; set; }

            /// <devdoc>
            ///     Required control, must have type IEditableTextControl
            /// </devdoc>
            internal Control EmailTextBox {
                get {
                    if (_emailTextBox != null) {
                        return _emailTextBox;
                    } else {
                        Control emailTextBox = FindControl(_emailID);
                        if (emailTextBox is IEditableTextControl) {
                            // 
                            return emailTextBox;
                        } else {
                            if (!_createUserWizard.DesignMode && _createUserWizard.RequireEmail) {
                                throw new HttpException(SR.GetString(SR.CreateUserWizard_NoEmailTextBox,
                                                                                         _createUserWizard.ID, _emailID));
                            }
                            return null;
                        }
                    }
                }
                set {
                    _emailTextBox = value;
                }
            }

            internal LabelLiteral PasswordLabel { get; set; }

            /// <devdoc>
            ///     Optional control, must have type ITextControl
            /// </devdoc>
            internal Control ErrorMessageLabel {
                get {
                    if (_unknownErrorMessageLabel != null) {
                        return _unknownErrorMessageLabel;
                    } else {
                        Control control = FindControl(_errorMessageID);
                        ITextControl errorMessageLabel = control as ITextControl;
                        if (errorMessageLabel == null) {
                            return null;
                        }
                        return control;
                    }
                }
                set {
                    _unknownErrorMessageLabel = value;
                }
            }

            internal Image HelpPageIcon { get; set; }

            internal HyperLink HelpPageLink { get; set; }

            internal Literal InstructionLabel { get; set; }

            internal CompareValidator PasswordCompareValidator { get; set; }

            internal Literal PasswordHintLabel { get; set; }

            internal RegularExpressionValidator PasswordRegExpValidator { get; set; }

            internal RequiredFieldValidator PasswordRequired { get; set; }

            /// <devdoc>
            ///     Required control, must have type IEditableTextControl
            /// </devdoc>
            internal Control PasswordTextBox {
                get {
                    if (_passwordTextBox != null) {
                        return _passwordTextBox;
                    } else {
                        Control passwordTextBox = FindControl(_passwordID);
                        if (passwordTextBox is IEditableTextControl) {
                            // 
                            return passwordTextBox;
                        } else {
                            if (!_createUserWizard.DesignMode && !_createUserWizard.AutoGeneratePassword) {
                                throw new HttpException(SR.GetString(SR.CreateUserWizard_NoPasswordTextBox,
                                                                                         _createUserWizard.ID, _passwordID));
                            }
                            return null;
                        }
                    }
                }
                set {
                    _passwordTextBox = value;
                }
            }

            internal Literal Title { get; set; }

            internal LabelLiteral UserNameLabel { get; set; }

            internal RequiredFieldValidator UserNameRequired { get; set; }

            internal LabelLiteral QuestionLabel { get; set; }

            internal RequiredFieldValidator QuestionRequired { get; set; }

            /// <devdoc>
            ///     Required control, must have type IEditableTextControl
            /// </devdoc>
            internal Control QuestionTextBox {
                get {
                    if (_questionTextBox != null) {
                        return _questionTextBox;
                    } else {
                        Control questionTextBox = FindControl(_questionID);
                        if (questionTextBox is IEditableTextControl) {
                            // 
                            return questionTextBox;
                        } else {
                            if (!_createUserWizard.DesignMode && _createUserWizard.QuestionAndAnswerRequired) {
                                throw new HttpException(SR.GetString(SR.CreateUserWizard_NoQuestionTextBox,
                                                                                         _createUserWizard.ID, _questionID));
                            }
                            return null;
                        }
                    }
                }
                set {
                    _questionTextBox = value;
                }
            }

            /// <devdoc>
            ///     Required control, must have type IEditableTextControl
            /// </devdoc>
            internal Control UserNameTextBox {
                get {
                    if (_userNameTextBox != null) {
                        return _userNameTextBox;
                    } else {
                        Control userNameTextBox = FindControl(_userNameID);
                        if (userNameTextBox is IEditableTextControl) {
                            // 
                            return userNameTextBox;
                        } else if (!_createUserWizard.DesignMode) {
                            throw new HttpException(SR.GetString(SR.CreateUserWizard_NoUserNameTextBox,
                                                                                     _createUserWizard.ID, _userNameID));
                        }

                        return null;
                    }
                }
                set {
                    _userNameTextBox = value;
                }
            }
        }

        private sealed class CompleteStepContainer : BaseContentTemplateContainer {

            internal CompleteStepContainer(CreateUserWizard wizard, bool useInnerTable)
                : base(wizard, useInnerTable) {
            }

            internal LinkButton ContinueLinkButton { get; set; }

            internal Button ContinuePushButton { get; set; }

            internal ImageButton ContinueImageButton { get; set; }

            internal Image EditProfileIcon { get; set; }

            internal HyperLink EditProfileLink { get; set; }

            internal Table LayoutTable { get; set; }

            internal Literal SuccessTextLabel { get; set; }

            internal Literal Title { get; set; }
        }
    }
}
