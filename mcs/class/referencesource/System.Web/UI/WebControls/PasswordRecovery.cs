//------------------------------------------------------------------------------
// <copyright file="PasswordRecovery.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Text;
    using System.Web.Configuration;
    using System.Web.Management;
    using System.Web.Security;


    /// <devdoc>
    /// Displays UI that allows a user to recover his/her password.  Prompts for username and possibly answer
    /// to security question.  If successful, the user's password is sent to him/her via email.  Uses
    /// a Membership provider.  UI can be customized using control properties or a template.
    /// </devdoc>
    [
    Bindable(false),
    DefaultEvent("SendingMail"),
    Designer("System.Web.UI.Design.WebControls.PasswordRecoveryDesigner, " + AssemblyRef.SystemDesign)
    ]
    public class PasswordRecovery : CompositeControl, IBorderPaddingControl, IRenderOuterTableControl {
        public static readonly string SubmitButtonCommandName = "Submit";

        // Needed for user template feature
        private const string _userNameID = "UserName";
        private const string _questionID = "Question";
        private const string _answerID = "Answer";
        private const string _failureTextID = "FailureText";

        // Needed only for "convert to template" feature, otherwise unnecessary
        private const string _userNameRequiredID = "UserNameRequired";
        private const string _answerRequiredID = "AnswerRequired";
        private const string _pushButtonID = "SubmitButton";
        private const string _imageButtonID = "SubmitImageButton";
        private const string _linkButtonID = "SubmitLinkButton";
        private const string _helpLinkID = "HelpLink";

        private const string _userNameContainerID = "UserNameContainerID";
        private const string _questionContainerID = "QuestionContainerID";
        private const string _successContainerID = "SuccessContainerID";
        private const ValidatorDisplay _requiredFieldValidatorDisplay = ValidatorDisplay.Static;
        private const string _userNameReplacementKey = "<%\\s*UserName\\s*%>";
        private const string _passwordReplacementKey = "<%\\s*Password\\s*%>";

        private string _answer;
        private View _currentView = View.UserName;
        private string _question;
        private string _userName;
        private bool _convertingToTemplate = false;
        private bool _renderDesignerRegion = false;

        private ITemplate _userNameTemplate;
        private UserNameContainer _userNameContainer;
        private ITemplate _questionTemplate;
        private QuestionContainer _questionContainer;
        private ITemplate _successTemplate;
        private SuccessContainer _successContainer;

        private const int _viewStateArrayLength = 11;
        private Style _submitButtonStyle;
        private TableItemStyle _labelStyle;
        private Style _textBoxStyle;
        private TableItemStyle _hyperLinkStyle;
        private TableItemStyle _instructionTextStyle;
        private TableItemStyle _titleTextStyle;
        private TableItemStyle _failureTextStyle;
        private TableItemStyle _successTextStyle;
        private MailDefinition _mailDefinition;
        private Style _validatorTextStyle;

        private static readonly object EventVerifyingUser = new object();
        private static readonly object EventUserLookupError = new object();
        private static readonly object EventVerifyingAnswer = new object();
        private static readonly object EventAnswerLookupError = new object();
        private static readonly object EventSendMailError = new object();
        private static readonly object EventSendingMail = new object();


        /// <devdoc>
        /// Answer to the security question.
        /// </devdoc>
        [
        Browsable(false),
        Filterable(false),
        Themeable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public virtual string Answer {
            get {
                return (_answer == null) ? String.Empty : _answer;
            }
        }

        private string AnswerInternal {
            get {
                string answer = Answer;
                if (String.IsNullOrEmpty(answer) && _questionContainer != null) {
                    ITextControl answerTextBox = (ITextControl)_questionContainer.AnswerTextBox;
                    if (answerTextBox != null && answerTextBox.Text != null) {
                        return answerTextBox.Text;
                    }
                }
                return answer;
            }
        }

        /// <devdoc>
        /// The text that identifies the answer textbox.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.PasswordRecovery_DefaultAnswerLabelText),
        WebSysDescription(SR.PasswordRecovery_AnswerLabelText)
        ]
        public virtual string AnswerLabelText {
            get {
                object obj = ViewState["AnswerLabelText"];
                return (obj == null) ? SR.GetString(SR.PasswordRecovery_DefaultAnswerLabelText) : (string)obj;
            }
            set {
                ViewState["AnswerLabelText"] = value;
            }
        }


        /// <devdoc>
        /// The text to be shown in the validation summary when the answer is empty.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Validation"),
        WebSysDefaultValue(SR.PasswordRecovery_DefaultAnswerRequiredErrorMessage),
        WebSysDescription(SR.LoginControls_AnswerRequiredErrorMessage)
        ]
        public virtual string AnswerRequiredErrorMessage {
            get {
                object obj = ViewState["AnswerRequiredErrorMessage"];
                return (obj == null) ?
                    SR.GetString(SR.PasswordRecovery_DefaultAnswerRequiredErrorMessage) : (string)obj;
            }
            set {
                ViewState["AnswerRequiredErrorMessage"] = value;
            }
        }


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
                    throw new ArgumentOutOfRangeException("value", SR.GetString(SR.PasswordRecovery_InvalidBorderPadding));
                }
                ViewState["BorderPadding"] = value;
            }
        }


        /// <devdoc>
        /// The style of the submit button.
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.PasswordRecovery_SubmitButtonStyle)
        ]
        public Style SubmitButtonStyle {
            get {
                if (_submitButtonStyle == null) {
                    _submitButtonStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager)_submitButtonStyle).TrackViewState();
                    }
                }
                return _submitButtonStyle;
            }
        }


        /// <devdoc>
        /// The type of the submit button.
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(ButtonType.Button),
        WebSysDescription(SR.PasswordRecovery_SubmitButtonType)
        ]
        public virtual ButtonType SubmitButtonType {
            get {
                object obj = ViewState["SubmitButtonType"];
                return (obj == null) ? ButtonType.Button : (ButtonType)obj;
            }
            set {
                if (value < ButtonType.Button || value > ButtonType.Link) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["SubmitButtonType"] = value;
            }
        }

        private bool ConvertingToTemplate {
            get {
                return (DesignMode && _convertingToTemplate);
            }
        }

        /// <devdoc>
        /// Internal because used from PasswordRecoveryAdapter.
        /// </devdoc>
        internal View CurrentView {
            get {
                return _currentView;
            }
            set {
                if (value < View.UserName || value > View.Success) {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (value != CurrentView) {
                    _currentView = value;
                    UpdateValidators();
                }
            }
        }


        /// <devdoc>
        /// The style of the failure text.
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
                        ((IStateManager)_failureTextStyle).TrackViewState();
                    }
                }
                return _failureTextStyle;
            }
        }


        /// <devdoc>
        /// The text to be shown when there is an unexpected failure.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.PasswordRecovery_DefaultGeneralFailureText),
        WebSysDescription(SR.PasswordRecovery_GeneralFailureText)
        ]
        public virtual string GeneralFailureText {
            get {
                object obj = ViewState["GeneralFailureText"];
                return (obj == null) ? SR.GetString(SR.PasswordRecovery_DefaultGeneralFailureText) : (string)obj;
            }
            set {
                ViewState["GeneralFailureText"] = value;
            }
        }


        /// <devdoc>
        /// The text to be shown for the help link.
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
        /// The URL of the help page.
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
        /// The style of the hyperlinks.
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
        /// The style of the instructions.
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
        /// The style of the textbox labels.
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
        /// The content and format of the e-mail message that contains the new password.
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        Themeable(false),
        WebSysDescription(SR.PasswordRecovery_MailDefinition)
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
                ViewState["MembershipProvider"] = value;
            }
        }


        /// <devdoc>
        /// The security question.
        /// </devdoc>
        [
        Browsable(false),
        Filterable(false),
        Themeable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public virtual string Question {
            get {
                return (_question != null) ? _question : String.Empty;
            }
            private set {
                _question = value;
            }
        }


        /// <devdoc>
        /// The text to be shown when the answer is incorrect.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.PasswordRecovery_DefaultQuestionFailureText),
        WebSysDescription(SR.PasswordRecovery_QuestionFailureText)
        ]
        public virtual string QuestionFailureText {
            get {
                object obj = ViewState["QuestionFailureText"];
                return (obj == null) ? SR.GetString(SR.PasswordRecovery_DefaultQuestionFailureText) : (string)obj;
            }
            set {
                ViewState["QuestionFailureText"] = value;
            }
        }


        /// <devdoc>
        /// Text that is displayed to give instructions for answering the question.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.PasswordRecovery_DefaultQuestionInstructionText),
        WebSysDescription(SR.PasswordRecovery_QuestionInstructionText)
        ]
        public virtual string QuestionInstructionText {
            get {
                object obj = ViewState["QuestionInstructionText"];
                return (obj == null) ? SR.GetString(SR.PasswordRecovery_DefaultQuestionInstructionText) : (string)obj;
            }
            set {
                ViewState["QuestionInstructionText"] = value;
            }
        }


        /// <devdoc>
        /// The text that identifies the question.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.PasswordRecovery_DefaultQuestionLabelText),
        WebSysDescription(SR.PasswordRecovery_QuestionLabelText)
        ]
        public virtual string QuestionLabelText {
            get {
                object obj = ViewState["QuestionLabelText"];
                return (obj == null) ? SR.GetString(SR.PasswordRecovery_DefaultQuestionLabelText) : (string)obj;
            }
            set {
                ViewState["QuestionLabelText"] = value;
            }
        }


        /// <devdoc>
        /// The text to be shown for the title when answering the question.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.PasswordRecovery_DefaultQuestionTitleText),
        WebSysDescription(SR.PasswordRecovery_QuestionTitleText)
        ]
        public virtual string QuestionTitleText {
            get {
                object obj = ViewState["QuestionTitleText"];
                return (obj == null) ? SR.GetString(SR.PasswordRecovery_DefaultQuestionTitleText) : (string)obj;
            }
            set {
                ViewState["QuestionTitleText"] = value;
            }
        }


        /// <devdoc>
        /// Template rendered to prompt the user for an answer to the security question.
        /// </devdoc>
        [
        Browsable(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(PasswordRecovery)),
        WebSysDescription(SR.PasswordRecovery_QuestionTemplate)
        ]
        public virtual ITemplate QuestionTemplate {
            get {
                return _questionTemplate;
            }
            set {
                _questionTemplate = value;
                ChildControlsCreated = false;
            }
        }

        /// <devdoc>
        /// Internal because used from PasswordRecoveryAdapter.
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.PasswordRecovery_QuestionTemplateContainer)
        ]
        public Control QuestionTemplateContainer {
            get {
                EnsureChildControls();
                return _questionContainer;
            }
        }


        /// <devdoc>
        /// The URL of an image to be displayed for the submit button.
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescription(SR.ChangePassword_ChangePasswordButtonImageUrl),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty()
        ]
        public virtual string SubmitButtonImageUrl {
            get {
                object obj = ViewState["SubmitButtonImageUrl"];
                return (obj == null) ? String.Empty : (string)obj;
            }
            set {
                ViewState["SubmitButtonImageUrl"] = value;
            }
        }


        /// <devdoc>
        /// The text to be shown for the submit button.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.PasswordRecovery_DefaultSubmitButtonText),
        WebSysDescription(SR.ChangePassword_ChangePasswordButtonText)
        ]
        public virtual string SubmitButtonText {
            get {
                object obj = ViewState["SubmitButtonText"];
                return (obj == null) ? SR.GetString(SR.PasswordRecovery_DefaultSubmitButtonText) : (string)obj;
            }
            set {
                ViewState["SubmitButtonText"] = value;
            }
        }


        /// <devdoc>
        /// The URL that the user is directed to after the password e-mail has been sent.
        /// If non-null, always redirect the user to this page after successful password recovery.  Else, perform the refresh action.
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
                return (obj == null) ? String.Empty : (string)obj;
            }
            set {
                ViewState["SuccessPageUrl"] = value;
            }
        }


        /// <devdoc>
        /// Template rendered after the e-mail is sent.
        /// </devdoc>
        [
        Browsable(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.PasswordRecovery_SuccessTemplate),
        TemplateContainer(typeof(PasswordRecovery))
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
        /// Internal because used from PasswordRecoveryAdapter.
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.PasswordRecovery_SuccessTemplateContainer)
        ]
        public Control SuccessTemplateContainer {
            get {
                EnsureChildControls();
                return _successContainer;
            }
        }


        /// <devdoc>
        /// The text to be shown after the password e-mail has been sent.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.PasswordRecovery_DefaultSuccessText),
        WebSysDescription(SR.PasswordRecovery_SuccessText)
        ]
        public virtual string SuccessText {
            get {
                object obj = ViewState["SuccessText"];
                return (obj == null) ? SR.GetString(SR.PasswordRecovery_DefaultSuccessText) : (string)obj;
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
        WebSysDescription(SR.PasswordRecovery_SuccessTextStyle)
        ]
        public TableItemStyle SuccessTextStyle {
            get {
                if (_successTextStyle == null) {
                    _successTextStyle = new TableItemStyle();
                    if (IsTrackingViewState) {
                        ((IStateManager)_successTextStyle).TrackViewState();
                    }
                }
                return _successTextStyle;
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
        /// The style of the textboxes.
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
        /// The layout of the labels in relation to the textboxes.
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(LoginTextLayout.TextOnLeft),
        WebSysDescription(SR.LoginControls_TextLayout)
        ]
        public virtual LoginTextLayout TextLayout {
            get {
                object obj = ViewState["TextLayout"];
                return (obj == null) ? LoginTextLayout.TextOnLeft : (LoginTextLayout)obj;
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
        /// The style of the title.
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
        /// The initial value in the user name textbox.
        /// </devdoc>
        [
        Localizable(true),
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

        internal string UserNameInternal {
            get {
                string userName = UserName;
                if (String.IsNullOrEmpty(userName) && _userNameContainer != null) {
                    ITextControl textbox = _userNameContainer.UserNameTextBox as ITextControl;
                    if (textbox != null) {
                        return textbox.Text;
                    }
                }
                return userName;
            }
        }


        /// <devdoc>
        /// The text to be shown when the user name is invalid.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.PasswordRecovery_DefaultUserNameFailureText),
        WebSysDescription(SR.PasswordRecovery_UserNameFailureText)
        ]
        public virtual string UserNameFailureText {
            get {
                object obj = ViewState["UserNameFailureText"];
                return (obj == null) ? SR.GetString(SR.PasswordRecovery_DefaultUserNameFailureText) : (string)obj;
            }
            set {
                ViewState["UserNameFailureText"] = value;
            }
        }


        /// <devdoc>
        /// Text that is displayed to give instructions for entering the user name.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.PasswordRecovery_DefaultUserNameInstructionText),
        WebSysDescription(SR.PasswordRecovery_UserNameInstructionText)
        ]
        public virtual string UserNameInstructionText {
            get {
                object obj = ViewState["UserNameInstructionText"];
                return (obj == null) ? SR.GetString(SR.PasswordRecovery_DefaultUserNameInstructionText) : (string)obj;
            }
            set {
                ViewState["UserNameInstructionText"] = value;
            }
        }


        /// <devdoc>
        /// The text that identifies the user name.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.PasswordRecovery_DefaultUserNameLabelText),
        WebSysDescription(SR.PasswordRecovery_UserNameLabelText)
        ]
        public virtual string UserNameLabelText {
            get {
                object obj = ViewState["UserNameLabelText"];
                return (obj == null) ? SR.GetString(SR.PasswordRecovery_DefaultUserNameLabelText) : (string)obj;
            }
            set {
                ViewState["UserNameLabelText"] = value;
            }
        }


        /// <devdoc>
        /// The text to be shown in the validation summary when the user name is empty.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Validation"),
        WebSysDefaultValue(SR.PasswordRecovery_DefaultUserNameRequiredErrorMessage),
        WebSysDescription(SR.ChangePassword_UserNameRequiredErrorMessage)
        ]
        public virtual string UserNameRequiredErrorMessage {
            get {
                object obj = ViewState["UserNameRequiredErrorMessage"];
                return (obj == null) ?
                    SR.GetString(SR.PasswordRecovery_DefaultUserNameRequiredErrorMessage) : (string)obj;
            }
            set {
                ViewState["UserNameRequiredErrorMessage"] = value;
            }
        }


        /// <devdoc>
        /// Template rendered to prompt the user to enter a user name.
        /// </devdoc>
        [
        Browsable(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(PasswordRecovery)),
        WebSysDescription(SR.PasswordRecovery_UserNameTemplate)
        ]
        public virtual ITemplate UserNameTemplate {
            get {
                return _userNameTemplate;
            }
            set {
                _userNameTemplate = value;
                ChildControlsCreated = false;
            }
        }

        /// <devdoc>
        /// Internal because used from PasswordRecoveryAdapter.
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.PasswordRecovery_UserNameTemplateContainer)
        ]
        public Control UserNameTemplateContainer {
            get {
                EnsureChildControls();
                return _userNameContainer;
            }
        }


        /// <devdoc>
        /// The text to be shown for the title when entering the user name.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.PasswordRecovery_DefaultUserNameTitleText),
        WebSysDescription(SR.PasswordRecovery_UserNameTitleText)
        ]
        public virtual string UserNameTitleText {
            get {
                object obj = ViewState["UserNameTitleText"];
                return (obj == null) ? SR.GetString(SR.PasswordRecovery_DefaultUserNameTitleText) : (string)obj;
            }
            set {
                ViewState["UserNameTitleText"] = value;
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
                        ((IStateManager)_validatorTextStyle).TrackViewState();
                    }
                }
                return _validatorTextStyle;
            }
        }



        /// <devdoc>
        /// Raised when the answer provided is incorrect.
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.PasswordRecovery_AnswerLookupError)
        ]
        public event EventHandler AnswerLookupError {
            add {
                Events.AddHandler(EventAnswerLookupError, value);
            }
            remove {
                Events.RemoveHandler(EventAnswerLookupError, value);
            }
        }


        /// <devdoc>
        /// Raised before the answer is validated.
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.PasswordRecovery_VerifyingAnswer)
        ]
        public event LoginCancelEventHandler VerifyingAnswer {
            add {
                Events.AddHandler(EventVerifyingAnswer, value);
            }
            remove {
                Events.RemoveHandler(EventVerifyingAnswer, value);
            }
        }


        /// <devdoc>
        /// Raised before the e-mail is sent.
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.PasswordRecovery_SendingMail)
        ]
        public event MailMessageEventHandler SendingMail {
            add {
                Events.AddHandler(EventSendingMail, value);
            }
            remove {
                Events.RemoveHandler(EventSendingMail, value);
            }
        }

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


        /// <devdoc>
        /// Raised before the username is looked up.
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.PasswordRecovery_VerifyingUser)
        ]
        public event LoginCancelEventHandler VerifyingUser {
            add {
                Events.AddHandler(EventVerifyingUser, value);
            }
            remove {
                Events.RemoveHandler(EventVerifyingUser, value);
            }
        }


        /// <devdoc>
        /// Raised when the user name is invalid.
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.PasswordRecovery_UserLookupError)
        ]
        public event EventHandler UserLookupError {
            add {
                Events.AddHandler(EventUserLookupError, value);
            }
            remove {
                Events.RemoveHandler(EventUserLookupError, value);
            }
        }

        /// <devdoc>
        /// Called when the answer text box changes.
        /// </devdoc>
        private void AnswerTextChanged(object source, EventArgs e) {
            _answer = ((ITextControl)source).Text;
        }

        private void AttemptSendPassword() {
            if (Page != null && !Page.IsValid) {
                return;
            }

            if (CurrentView == View.UserName) {
                AttemptSendPasswordUserNameView();
            }
            else if (CurrentView == View.Question) {
                AttemptSendPasswordQuestionView();
            }
        }

        /// <devdoc>
        /// Called when the user presses submit in the question view.
        /// Verifies the answer to the security question.  If correct, sends password in e-mail.
        /// If answer is incorrect or there is any other error, a failure message is shown.
        /// </devdoc>
        private void AttemptSendPasswordQuestionView() {
            MembershipProvider provider = LoginUtil.GetProvider(MembershipProvider);
            MembershipUser user = provider.GetUser(UserNameInternal, /*userIsOnline*/ false, /*throwOnError*/ false);
            if (user != null) {
                if (user.IsLockedOut) {
                    SetFailureTextLabel(_questionContainer, GeneralFailureText);
                    return;
                }

                Question = user.PasswordQuestion;
                if (String.IsNullOrEmpty(Question)) {
                    SetFailureTextLabel(_questionContainer, GeneralFailureText);
                    return;
                }

                LoginCancelEventArgs cancelEventArgs = new LoginCancelEventArgs();
                OnVerifyingAnswer(cancelEventArgs);
                if (cancelEventArgs.Cancel) {
                    return;
                }

                string answer = AnswerInternal;
                string password = null;
                string email = user.Email;
                // If there is no email address, show the GeneralFailureText and return immediately.
                // We must be especially sure we do not reset the password if we do not have an
                // email address. (VSWhidbey 387663)
                if (String.IsNullOrEmpty(email)) {
                    SetFailureTextLabel(_questionContainer, GeneralFailureText);
                    return;
                }

                if (provider.EnablePasswordRetrieval) {
                    password = user.GetPassword(answer, /*throwOnError*/ false);
                }
                else if (provider.EnablePasswordReset) {
                    password = user.ResetPassword(answer, /*throwOnError*/ false);
                }
                else {
                    throw new HttpException(SR.GetString(SR.PasswordRecovery_RecoveryNotSupported));
                }

                if (password != null) {
                    LoginUtil.SendPasswordMail(email, user.UserName, password, MailDefinition,
                                               SR.GetString(SR.PasswordRecovery_DefaultSubject),
                                               SR.GetString(SR.PasswordRecovery_DefaultBody),
                                               OnSendingMail, OnSendMailError, this);
                    PerformSuccessAction();
                }
                else {
                    OnAnswerLookupError(EventArgs.Empty);
                    SetFailureTextLabel(_questionContainer, QuestionFailureText);
                }
            }
            else {
                // If the user lookup fails after it succeeded in the previous view,
                // it is considered a general failure
                SetFailureTextLabel(_questionContainer, GeneralFailureText);
            }
        }

        /// <devdoc>
        /// Called when the user presses submit in the user name view.
        /// If user name is valid, sends password in e-mail.
        /// If user name is invalid or there is any other error, a failure message is shown.
        /// </devdoc>
        private void AttemptSendPasswordUserNameView() {
            LoginCancelEventArgs cancelEventArgs = new LoginCancelEventArgs();
            OnVerifyingUser(cancelEventArgs);
            if (cancelEventArgs.Cancel) {
                return;
            }

            MembershipProvider provider = LoginUtil.GetProvider(MembershipProvider);
            MembershipUser user = provider.GetUser(UserNameInternal, /*isUserOnline*/ false, /*throwOnError*/ false);
            if (user != null) {
                if (user.IsLockedOut) {
                    SetFailureTextLabel(_userNameContainer, UserNameFailureText);
                    return;
                }

                if (provider.RequiresQuestionAndAnswer) {
                    Question = user.PasswordQuestion;
                    if (String.IsNullOrEmpty(Question)) {
                        SetFailureTextLabel(_userNameContainer, GeneralFailureText);
                        return;
                    }
                    CurrentView = View.Question;
                }
                else {
                    string password = null;
                    string email = user.Email;
                    // If there is no email address, show the GeneralFailureText and return immediately.
                    // We must be especially sure we do not reset the password if we do not have an
                    // email address. (VSWhidbey 387663)                     
                    if (String.IsNullOrEmpty(email)) {
                        SetFailureTextLabel(_userNameContainer, GeneralFailureText);
                        return;
                    }

                    if (provider.EnablePasswordRetrieval) {
                        password = user.GetPassword(/*throwOnError*/ false);
                    }
                    else if (provider.EnablePasswordReset) {
                        password = user.ResetPassword(/*throwOnError*/ false);
                    }
                    else {
                        throw new HttpException(SR.GetString(SR.PasswordRecovery_RecoveryNotSupported));
                    }

                    if (password != null) {
                        LoginUtil.SendPasswordMail(email, user.UserName, password, MailDefinition,
                                                   SR.GetString(SR.PasswordRecovery_DefaultSubject),
                                                   SR.GetString(SR.PasswordRecovery_DefaultBody),
                                                   OnSendingMail, OnSendMailError, this);
                        PerformSuccessAction();
                    }
                    else {
                        SetFailureTextLabel(_userNameContainer, GeneralFailureText);
                    }
                }
            }
            else {
                OnUserLookupError(EventArgs.Empty);
                SetFailureTextLabel(_userNameContainer, UserNameFailureText);
            }
        }


        /// <devdoc>
        /// Instantiates the proper template in the template container, and wires up necessary events.
        /// </devdoc>
        protected internal override void CreateChildControls() {
            Controls.Clear();
            CreateUserView();
            CreateQuestionView();
            CreateSuccessView();
        }

        private void CreateQuestionView() {
            ITemplate template = null;
            _questionContainer = new QuestionContainer(this);
            _questionContainer.ID = _questionContainerID;
            _questionContainer.RenderDesignerRegion = _renderDesignerRegion;
            if (QuestionTemplate != null) {
                template = QuestionTemplate;
            }
            else {
                // 
                template = new DefaultQuestionTemplate(this);
                _questionContainer.EnableViewState = false;

                // Disable theming if using default template (VSWhidbey 86010)
                _questionContainer.EnableTheming = false;

            }
            template.InstantiateIn(_questionContainer);
            Controls.Add(_questionContainer);

            IEditableTextControl answerTextBox = _questionContainer.AnswerTextBox as IEditableTextControl;
            if (answerTextBox != null) {
                answerTextBox.TextChanged += new EventHandler(AnswerTextChanged);
            }
        }

        private void CreateSuccessView() {
            ITemplate template = null;
            _successContainer = new SuccessContainer(this);
            _successContainer.ID = _successContainerID;
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

        private void CreateUserView() {
            ITemplate template = null;
            _userNameContainer = new UserNameContainer(this);
            _userNameContainer.ID = _userNameContainerID;
            _userNameContainer.RenderDesignerRegion = _renderDesignerRegion;
            if (UserNameTemplate != null) {
                template = UserNameTemplate;
            }
            else {
                // 
                template = new DefaultUserNameTemplate(this);
                _userNameContainer.EnableViewState = false;

                // Disable theming if using default template (VSWhidbey 86010)
                _userNameContainer.EnableTheming = false;

            }
            template.InstantiateIn(_userNameContainer);
            Controls.Add(_userNameContainer);

            // Set the editable child control properties here for two reasons:
            // - So change events will be raised if viewstate is disabled on the child controls
            //   - Viewstate is always disabled for default template, and might be for user template
            // - So the controls render correctly in the designer
            SetUserNameEditableChildProperties();

            IEditableTextControl userNameTextBox = _userNameContainer.UserNameTextBox as IEditableTextControl;
            if (userNameTextBox != null) {
                userNameTextBox.TextChanged += new EventHandler(UserNameTextChanged);
            }
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
                    CurrentView = (View)(int)state.Second;
                }
                if (state.Third != null) {
                    _userName = (string)state.Third;
                }
            }
        }


        /// <devdoc>
        ///     Loads a saved state of the <see cref='System.Web.UI.WebControls.Login'/>.
        /// </devdoc>
        protected override void LoadViewState(object savedState) {
            if (savedState == null) {
                base.LoadViewState(null);
            }
            else {
                object[] myState = (object[])savedState;
                if (myState.Length != _viewStateArrayLength) {
                    throw new ArgumentException(SR.GetString(SR.ViewState_InvalidViewState));
                }

                base.LoadViewState(myState[0]);
                if (myState[1] != null) {
                    ((IStateManager)SubmitButtonStyle).LoadViewState(myState[1]);
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
                    ((IStateManager)FailureTextStyle).LoadViewState(myState[7]);
                }
                if (myState[8] != null) {
                    ((IStateManager)SuccessTextStyle).LoadViewState(myState[8]);
                }
                if (myState[9] != null) {
                    ((IStateManager)MailDefinition).LoadViewState(myState[9]);
                }
                if (myState[10] != null) {
                    ((IStateManager)ValidatorTextStyle).LoadViewState(myState[10]);
                }
            }
        }


        /// <devdoc>
        /// Raises the AnswerLookup event.
        /// </devdoc>
        protected virtual void OnAnswerLookupError(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventAnswerLookupError];
            if (handler != null) {
                handler(this, e);
            }
        }


        /// <devdoc>
        /// Raises the VerifyingAnswer event.
        /// </devdoc>
        protected virtual void OnVerifyingAnswer(LoginCancelEventArgs e) {
            LoginCancelEventHandler handler = (LoginCancelEventHandler)Events[EventVerifyingAnswer];
            if (handler != null) {
                handler(this, e);
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

        protected virtual void OnSendMailError(SendMailErrorEventArgs e) {
            SendMailErrorEventHandler handler = (SendMailErrorEventHandler)Events[EventSendMailError];
            if (handler != null) {
                handler(this, e);
            }
        }


        /// <devdoc>
        /// Raises the VerifyingUser event.
        /// </devdoc>
        protected virtual void OnVerifyingUser(LoginCancelEventArgs e) {
            LoginCancelEventHandler handler = (LoginCancelEventHandler)Events[EventVerifyingUser];
            if (handler != null) {
                handler(this, e);
            }
        }


        /// <devdoc>
        /// Called when an event is raised by a control inside one of our templates.  Attempts to send
        /// password if the event was raised by the submit button.
        /// </devdoc>
        protected override bool OnBubbleEvent(object source, EventArgs e) {
            bool handled = false;
            if (e is CommandEventArgs) {
                CommandEventArgs ce = (CommandEventArgs)e;
                if (ce.CommandName.Equals(SubmitButtonCommandName, StringComparison.CurrentCultureIgnoreCase)) {
                    AttemptSendPassword();
                    handled = true;
                }
            }
            return handled;
        }


        protected internal override void OnInit(EventArgs e) {
            base.OnInit(e);
            Page.RegisterRequiresControlState(this);
            Page.LoadComplete += new EventHandler(OnPageLoadComplete);
        }

        private void OnPageLoadComplete(object sender, EventArgs e) {
            if (CurrentView == View.Question) {
                // Question will be null if the page was posted back using a control outside the
                // PasswordRecovery (VSWhidbey 81302).  Load the Question in Page.LoadComplete instead
                // of Control.OnPreRender(), so that the Question property is available to the page
                // developer earlier in the lifecycle.
                if (String.IsNullOrEmpty(Question)) {
                    MembershipProvider provider = LoginUtil.GetProvider(MembershipProvider);
                    MembershipUser user = provider.GetUser(UserNameInternal, /*isUserOnline*/ false, /*throwOnError*/ false);
                    if (user != null) {
                        Question = user.PasswordQuestion;
                        if (String.IsNullOrEmpty(Question)) {
                            SetFailureTextLabel(_questionContainer, GeneralFailureText);
                        }
                    }
                    else {
                        SetFailureTextLabel(_questionContainer, GeneralFailureText);
                    }
                }
            }
        }

        /// <devdoc>
        /// Overridden to set the editable child control properteries.
        /// </devdoc>
        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            _userNameContainer.Visible = false;
            _questionContainer.Visible = false;
            _successContainer.Visible = false;
            switch (CurrentView) {
                case View.UserName:
                    _userNameContainer.Visible = true;
                    // Set the editable child control properties here instead of Render, so they get into viewstate
                    // for the user template.
                    SetUserNameEditableChildProperties();
                    break;
                case View.Question:
                    _questionContainer.Visible = true;
                    break;
                case View.Success:
                    _successContainer.Visible = true;
                    break;
            }
        }


        /// <devdoc>
        /// Raises the UserLookupError event
        /// </devdoc>
        protected virtual void OnUserLookupError(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventUserLookupError];
            if (handler != null) {
                handler(this, e);
            }
        }

        private void PerformSuccessAction() {
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

            EnsureChildControls();

            if (DesignMode) {
                // Need to redo this for the designer as there's no PreRender(sigh)
                _userNameContainer.Visible = false;
                _questionContainer.Visible = false;
                _successContainer.Visible = false;
                switch (CurrentView) {
                    case View.UserName:
                        _userNameContainer.Visible = true;
                        break;
                    case View.Question:
                        _questionContainer.Visible = true;
                        break;
                    case View.Success:
                        _successContainer.Visible = true;
                        break;
                }
            }

            switch (CurrentView) {
                case View.UserName:
                    SetUserNameChildProperties();
                    break;
                case View.Question:
                    SetQuestionChildProperties();
                    break;
                case View.Success:
                    SetSuccessChildProperties();
                    break;
            }

            RenderContents(writer);
        }

        /// <devdoc>
        /// Saves the control state for those properties that should persist across postbacks
        /// even when EnableViewState=false.
        /// </devdoc>
        protected internal override object SaveControlState() {
            object baseState = base.SaveControlState();
            if (baseState != null || _currentView != 0 || _userName != null) {
                // Save the int value of the enum (instead of the enum value itself)
                // to save space in ControlState
                object currentViewState = null;
                object userNameState = null;

                if (_currentView != 0) {
                    currentViewState = (int)_currentView;
                }
                // Don't save _userName once we have reached the success view (VSWhidbey 81327)
                if (_userName != null && _currentView != View.Success) {
                    userNameState = _userName;
                }
                return new Triplet(baseState, currentViewState, userNameState);
            }
            return null;
        }


        /// <internalonly/>
        /// <devdoc>
        ///     Saves the state of the <see cref='System.Web.UI.WebControls.Login'/>.
        /// </devdoc>
        protected override object SaveViewState() {
            object[] myState = new object[_viewStateArrayLength];

            myState[0] = base.SaveViewState();
            myState[1] = (_submitButtonStyle != null) ? ((IStateManager)_submitButtonStyle).SaveViewState() : null;
            myState[2] = (_labelStyle != null) ? ((IStateManager)_labelStyle).SaveViewState() : null;
            myState[3] = (_textBoxStyle != null) ? ((IStateManager)_textBoxStyle).SaveViewState() : null;
            myState[4] = (_hyperLinkStyle != null) ? ((IStateManager)_hyperLinkStyle).SaveViewState() : null;
            myState[5] = (_instructionTextStyle != null) ? ((IStateManager)_instructionTextStyle).SaveViewState() : null;
            myState[6] = (_titleTextStyle != null) ? ((IStateManager)_titleTextStyle).SaveViewState() : null;
            myState[7] = (_failureTextStyle != null) ? ((IStateManager)_failureTextStyle).SaveViewState() : null;
            myState[8] = (_successTextStyle != null) ? ((IStateManager)_successTextStyle).SaveViewState() : null;
            myState[9] = (_mailDefinition != null) ? ((IStateManager)_mailDefinition).SaveViewState() : null;
            myState[10] = (_validatorTextStyle != null) ? ((IStateManager)_validatorTextStyle).SaveViewState() : null;

            for (int i = 0; i < _viewStateArrayLength; i++) {
                if (myState[i] != null) {
                    return myState;
                }
            }

            // More performant to return null than an array of null values
            return null;
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
        /// Used frequently, so extracted into method.
        /// </devdoc>
        private void SetFailureTextLabel(QuestionContainer container, string failureText) {
            ITextControl failureTextLabel = (ITextControl)container.FailureTextLabel;
            if (failureTextLabel != null) {
                failureTextLabel.Text = failureText;
            }
        }

        /// <devdoc>
        /// Used frequently, so extracted into method.
        /// </devdoc>
        private void SetFailureTextLabel(UserNameContainer container, string failureText) {
            ITextControl failureTextLabel = (ITextControl)container.FailureTextLabel;
            if (failureTextLabel != null) {
                failureTextLabel.Text = failureText;
            }
        }

        /// <devdoc>
        /// Internal because called from PasswordRecoveryAdapter.
        /// </devdoc>
        internal void SetQuestionChildProperties() {
            SetQuestionCommonChildProperties();
            if (QuestionTemplate == null) {
                SetQuestionDefaultChildProperties();
            }
        }

        /// <devdoc>
        /// Properties that apply to both default and user templates.
        /// </devdoc>
        private void SetQuestionCommonChildProperties() {
            QuestionContainer container = _questionContainer;

            // Clear out the tab index so it doesn't get applied to the tables in the container
            Util.CopyBaseAttributesToInnerControl(this, container);

            container.ApplyStyle(ControlStyle);

            // We need to use UserNameInternal for the DropDownList case where it won't fire a TextChanged for the first item
            ITextControl userName = (ITextControl)container.UserName;
            if (userName != null) {
                // VSWhidbey 304890 - Encode the user name
                userName.Text = HttpUtility.HtmlEncode(UserNameInternal);
            }

            ITextControl question = (ITextControl)container.Question;
            if (question != null) {
                // VSWhidbey 385802 - Encode the question
                question.Text = HttpUtility.HtmlEncode(Question);
            }

            ITextControl answerTextBox = (ITextControl)container.AnswerTextBox;
            if (answerTextBox != null) {
                answerTextBox.Text = String.Empty;
            }
        }

        /// <devdoc>
        /// Properties that apply to only the default template.
        /// </devdoc>
        private void SetQuestionDefaultChildProperties() {
            QuestionContainer container = _questionContainer;

            // Need to set the BorderPadding on the BorderTable instead of the LayoutTable, since
            // setting it on the LayoutTable would cause all of the controls inside the Login to be
            // separated by the BorderPadding amount.
            container.BorderTable.CellPadding = BorderPadding;
            container.BorderTable.CellSpacing = 0;

            Literal title = container.Title;
            string titleText = QuestionTitleText;
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
            string instructionText = QuestionInstructionText;
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

            Literal userNameLabel = container.UserNameLabel;
            string userNameLabelText = UserNameLabelText;
            if (userNameLabelText.Length > 0) {
                userNameLabel.Text = userNameLabelText;
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

            // Do not make this control invisible if its text is empty, because it must be present in the created
            // template in the designer.  Uncommon that the text will be empty anyway.
            Control userName = container.UserName;
            if (UserNameInternal.Length > 0) {
                userName.Visible = true;
            }
            else {
                userName.Visible = false;
            }
            if (userName is WebControl) ((WebControl)userName).TabIndex = TabIndex;

            Literal questionLabel = container.QuestionLabel;
            string questionLabelText = QuestionLabelText;
            if (questionLabelText.Length > 0) {
                questionLabel.Text = questionLabelText;
                if (_labelStyle != null) {
                    LoginUtil.SetTableCellStyle(questionLabel, LabelStyle);
                }
                questionLabel.Visible = true;
            }
            else {
                // DO NOT make the whole table cell invisible, because in some layouts it must exist for things
                // to align correctly.  Uncommon that this property will be empty anyway.
                questionLabel.Visible = false;
            }

            // Do not make this control invisible if its text is empty, because it must be present in the created
            // template in the designer.  Uncommon that the text will be empty anyway.
            Control question = container.Question;
            if (Question.Length > 0) {
                question.Visible = true;
            }
            else {
                question.Visible = false;
            }

            Literal answerLabel = container.AnswerLabel;
            string answerLabelText = AnswerLabelText;
            if (answerLabelText.Length > 0) {
                answerLabel.Text = answerLabelText;
                if (_labelStyle != null) {
                    LoginUtil.SetTableCellStyle(answerLabel, LabelStyle);
                }
                answerLabel.Visible = true;
            }
            else {
                // DO NOT make the whole table cell invisible, because in some layouts it must exist for things
                // to align correctly.  Uncommon that this property will be empty anyway.
                answerLabel.Visible = false;
            }

            WebControl answerTextBox = (WebControl)container.AnswerTextBox;
            Debug.Assert(answerTextBox != null, "AnswerTextBox cannot be null for the DefaultTemplate");
            if (_textBoxStyle != null) {
                answerTextBox.ApplyStyle(TextBoxStyle);
            }
            answerTextBox.TabIndex = TabIndex;
            answerTextBox.AccessKey = AccessKey;

            bool enableValidation = (CurrentView == View.Question);
            RequiredFieldValidator answerRequired = container.AnswerRequired;
            answerRequired.ErrorMessage = AnswerRequiredErrorMessage;
            answerRequired.ToolTip = AnswerRequiredErrorMessage;
            answerRequired.Enabled = enableValidation;
            answerRequired.Visible = enableValidation;
            if (_validatorTextStyle != null) {
                answerRequired.ApplyStyle(_validatorTextStyle);
            }

            LinkButton linkButton = container.LinkButton;
            ImageButton imageButton = container.ImageButton;
            Button pushButton = container.PushButton;

            WebControl button = null;
            switch (SubmitButtonType) {
                case ButtonType.Link:
                    linkButton.Text = SubmitButtonText;
                    button = linkButton;
                    break;
                case ButtonType.Image:
                    imageButton.ImageUrl = SubmitButtonImageUrl;
                    imageButton.AlternateText = SubmitButtonText;
                    button = imageButton;
                    break;
                case ButtonType.Button:
                    pushButton.Text = SubmitButtonText;
                    button = pushButton;
                    break;
            }

            // Set all buttons to nonvisible, then set the selected button to visible
            linkButton.Visible = false;
            imageButton.Visible = false;
            pushButton.Visible = false;
            button.Visible = true;
            button.TabIndex = TabIndex;

            if (_submitButtonStyle != null) {
                button.ApplyStyle(SubmitButtonStyle);
            }

            HyperLink helpPageLink = container.HelpPageLink;
            string helpPageText = HelpPageText;

            Image helpPageIcon = container.HelpPageIcon;
            if (helpPageText.Length > 0) {
                helpPageLink.Text = helpPageText;
                helpPageLink.NavigateUrl = HelpPageUrl;
                helpPageLink.TabIndex = TabIndex;
                helpPageLink.Visible = true;
            }
            else {
                helpPageLink.Visible = false;
            }

            string helpPageIconUrl = HelpPageIconUrl;
            bool helpPageIconVisible = (helpPageIconUrl.Length > 0);
            helpPageIcon.Visible = helpPageIconVisible;
            if (helpPageIconVisible) {
                helpPageIcon.ImageUrl = helpPageIconUrl;
                helpPageIcon.AlternateText = helpPageText;
            }

            if (helpPageLink.Visible || helpPageIcon.Visible) {
                if (_hyperLinkStyle != null) {
                    // Apply style except font to table cell, then apply font and forecolor to HyperLinks
                    // VSWhidbey 81289
                    TableItemStyle hyperLinkStyleExceptFont = new TableItemStyle();
                    hyperLinkStyleExceptFont.CopyFrom(HyperLinkStyle);
                    hyperLinkStyleExceptFont.Font.Reset();
                    LoginUtil.SetTableCellStyle(helpPageLink, hyperLinkStyleExceptFont);
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

        /// <devdoc>
        /// Internal because called from PasswordRecoveryAdapter.
        /// </devdoc>
        internal void SetSuccessChildProperties() {
            SuccessContainer container = _successContainer;

            // Clear out the tab index so it doesn't get applied to the tables in the container
            Util.CopyBaseAttributesToInnerControl(this, container);

            container.ApplyStyle(ControlStyle);

            if (SuccessTemplate == null) {
                container.BorderTable.CellPadding = BorderPadding;
                container.BorderTable.CellSpacing = 0;

                Literal successTextLabel = container.SuccessTextLabel;
                string successText = SuccessText;
                if (successText.Length > 0) {
                    successTextLabel.Text = successText;
                    if (_successTextStyle != null) {
                        LoginUtil.SetTableCellStyle(successTextLabel, _successTextStyle);
                    }
                    LoginUtil.SetTableCellVisible(successTextLabel, true);
                }
                else {
                    LoginUtil.SetTableCellVisible(successTextLabel, false);
                }
            }
        }

        /// <devdoc>
        /// Internal because called from PasswordRecoveryAdapter.
        /// </devdoc>
        internal void SetUserNameChildProperties() {
            SetUserNameCommonChildProperties();
            if (UserNameTemplate == null) {
                SetUserNameDefaultChildProperties();
            }
        }

        /// <devdoc>
        /// Properties that apply to both default and user templates.
        /// </devdoc>
        private void SetUserNameCommonChildProperties() {
            // Clear out the tab index so it doesn't get applied to the tables in the container
            Util.CopyBaseAttributesToInnerControl(this, _userNameContainer);

            _userNameContainer.ApplyStyle(ControlStyle);
        }

        /// <devdoc>
        /// Properties that apply to only the default template.
        /// </devdoc>
        private void SetUserNameDefaultChildProperties() {
            UserNameContainer container = _userNameContainer;
            if (UserNameTemplate == null) {
                _userNameContainer.BorderTable.CellPadding = BorderPadding;
                _userNameContainer.BorderTable.CellSpacing = 0;

                Literal title = container.Title;
                string titleText = UserNameTitleText;
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
                string instructionText = UserNameInstructionText;
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

                Literal userNameLabel = container.UserNameLabel;
                string userNameLabelText = UserNameLabelText;
                if (userNameLabelText.Length > 0) {
                    userNameLabel.Text = userNameLabelText;
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
                Debug.Assert(userNameTextBox != null, "UserNameTextBox cannot be null for the DefaultTemplate");
                if (_textBoxStyle != null) {
                    userNameTextBox.ApplyStyle(TextBoxStyle);
                }
                userNameTextBox.TabIndex = TabIndex;
                userNameTextBox.AccessKey = AccessKey;

                bool enableValidation = (CurrentView == View.UserName);
                RequiredFieldValidator userNameRequired = container.UserNameRequired;
                userNameRequired.ErrorMessage = UserNameRequiredErrorMessage;
                userNameRequired.ToolTip = UserNameRequiredErrorMessage;
                userNameRequired.Enabled = enableValidation;
                userNameRequired.Visible = enableValidation;
                if (_validatorTextStyle != null) {
                    userNameRequired.ApplyStyle(_validatorTextStyle);
                }

                LinkButton linkButton = container.LinkButton;
                ImageButton imageButton = container.ImageButton;
                Button pushButton = container.PushButton;

                WebControl button = null;
                switch (SubmitButtonType) {
                    case ButtonType.Link:
                        linkButton.Text = SubmitButtonText;
                        button = linkButton;
                        break;
                    case ButtonType.Image:
                        imageButton.ImageUrl = SubmitButtonImageUrl;
                        imageButton.AlternateText = SubmitButtonText;
                        button = imageButton;
                        break;
                    case ButtonType.Button:
                        pushButton.Text = SubmitButtonText;
                        button = pushButton;
                        break;
                }

                // Set all buttons to nonvisible, then set the selected button to visible
                linkButton.Visible = false;
                imageButton.Visible = false;
                pushButton.Visible = false;
                button.Visible = true;
                button.TabIndex = TabIndex;

                if (_submitButtonStyle != null) {
                    button.ApplyStyle(SubmitButtonStyle);
                }

                HyperLink helpPageLink = container.HelpPageLink;
                string helpPageText = HelpPageText;

                Image helpPageIcon = container.HelpPageIcon;
                if (helpPageText.Length > 0) {
                    helpPageLink.Text = helpPageText;
                    helpPageLink.NavigateUrl = HelpPageUrl;
                    helpPageLink.Visible = true;
                    helpPageLink.TabIndex = TabIndex;
                }
                else {
                    helpPageLink.Visible = false;
                }
                string helpPageIconUrl = HelpPageIconUrl;
                bool helpPageIconVisible = (helpPageIconUrl.Length > 0);
                helpPageIcon.Visible = helpPageIconVisible;
                if (helpPageIconVisible) {
                    helpPageIcon.ImageUrl = helpPageIconUrl;
                    helpPageIcon.AlternateText = helpPageText;
                }

                if (helpPageLink.Visible || helpPageIcon.Visible) {
                    if (_hyperLinkStyle != null) {
                        // Apply style except font to table cell, then apply font and forecolor to HyperLinks
                        // VSWhidbey 81289
                        Style hyperLinkStyleExceptFont = new TableItemStyle();
                        hyperLinkStyleExceptFont.CopyFrom(HyperLinkStyle);
                        hyperLinkStyleExceptFont.Font.Reset();
                        LoginUtil.SetTableCellStyle(helpPageLink, hyperLinkStyleExceptFont);
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
        }

        /// <devdoc>
        /// Called from CreateChildControls and PreRender
        // - So change events will be raised if viewstate is disabled on the child controls
        //   - Viewstate is always disabled for default template, and might be for user template
        // - So the controls render correctly in the designer
        /// </devdoc>
        private void SetUserNameEditableChildProperties() {
            // We need to use UserNameInternal for the DropDownList case where it won't fire a TextChanged for the first item
            string userName = UserNameInternal;
            if (userName.Length > 0) {
                ITextControl userNameTextBox = (ITextControl)_userNameContainer.UserNameTextBox;
                // UserNameTextBox is a required control, but it may be null at design-time
                if (userNameTextBox != null) {
                    userNameTextBox.Text = userName;
                }
            }
        }


        /// <devdoc>
        /// Marks the starting point to begin tracking and saving changes to the
        /// control as part of the control viewstate.
        /// </devdoc>
        protected override void TrackViewState() {
            base.TrackViewState();

            if (_submitButtonStyle != null) {
                ((IStateManager)_submitButtonStyle).TrackViewState();
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
            if (_failureTextStyle != null) {
                ((IStateManager)_failureTextStyle).TrackViewState();
            }
            if (_successTextStyle != null) {
                ((IStateManager)_successTextStyle).TrackViewState();
            }
            if (_mailDefinition != null) {
                ((IStateManager)_mailDefinition).TrackViewState();
            }
            if (_validatorTextStyle != null) {
                ((IStateManager)_validatorTextStyle).TrackViewState();
            }
        }

        private void UpdateValidators() {
            if (UserNameTemplate == null && _userNameContainer != null) {
                bool enabled = (CurrentView == View.UserName);
                _userNameContainer.UserNameRequired.Enabled = enabled;
                _userNameContainer.UserNameRequired.Visible = enabled;
            }
            if (QuestionTemplate == null && _questionContainer != null) {
                bool enabled = (CurrentView == View.Question);
                _questionContainer.AnswerRequired.Enabled = enabled;
                _questionContainer.AnswerRequired.Visible = enabled;
            }
        }

        /// <devdoc>
        /// Called when the answer text box changes.
        /// </devdoc>
        private void UserNameTextChanged(object source, EventArgs e) {
            UserName = ((ITextControl)source).Text;
        }

        /// <devdoc>
        /// The default question template for the control, used if QuestionTemplate is null.
        /// </devdoc>
        private sealed class DefaultQuestionTemplate : ITemplate {
            private PasswordRecovery _owner;

            public DefaultQuestionTemplate(PasswordRecovery owner) {
                _owner = owner;
            }

            /// <devdoc>
            ///  Creates the child controls, sets certain properties (mostly static properties)
            /// </devdoc>
            private void CreateControls(QuestionContainer questionContainer) {
                string validationGroup = _owner.UniqueID;

                questionContainer.Title = new Literal();
                questionContainer.Instruction = new Literal();
                questionContainer.UserNameLabel = new Literal();
                questionContainer.UserName = new Literal();
                questionContainer.QuestionLabel = new Literal();
                questionContainer.Question = new Literal();

                // Needed for "convert to template" feature
                questionContainer.UserName.ID = _userNameID;
                questionContainer.Question.ID = _questionID;

                TextBox answerTextBox = new TextBox();
                answerTextBox.ID = _answerID;
                questionContainer.AnswerTextBox = answerTextBox;
                questionContainer.AnswerLabel = new LabelLiteral(answerTextBox);

                bool enableValidation = (_owner.CurrentView == View.Question);
                RequiredFieldValidator answerRequired = new RequiredFieldValidator();
                answerRequired.ID = _answerRequiredID;
                answerRequired.ValidationGroup = validationGroup;
                answerRequired.ControlToValidate = answerTextBox.ID;
                answerRequired.Display = _requiredFieldValidatorDisplay;
                answerRequired.Text = SR.GetString(SR.LoginControls_DefaultRequiredFieldValidatorText);
                answerRequired.Enabled = enableValidation;
                answerRequired.Visible = enableValidation;
                questionContainer.AnswerRequired = answerRequired;

                LinkButton linkButton = new LinkButton();
                linkButton.ID = _linkButtonID;
                linkButton.ValidationGroup = validationGroup;
                linkButton.CommandName = SubmitButtonCommandName;
                questionContainer.LinkButton = linkButton;

                ImageButton imageButton = new ImageButton();
                imageButton.ID = _imageButtonID;
                imageButton.ValidationGroup = validationGroup;
                imageButton.CommandName = SubmitButtonCommandName;
                questionContainer.ImageButton = imageButton;

                Button pushButton = new Button();
                pushButton.ID = _pushButtonID;
                pushButton.ValidationGroup = validationGroup;
                pushButton.CommandName = SubmitButtonCommandName;
                questionContainer.PushButton = pushButton;

                questionContainer.HelpPageLink = new HyperLink();
                questionContainer.HelpPageLink.ID = _helpLinkID;
                questionContainer.HelpPageIcon = new Image();

                Literal failureTextLabel = new Literal();
                failureTextLabel.ID = _failureTextID;
                questionContainer.FailureTextLabel = failureTextLabel;
            }

            /// <devdoc>
            /// Adds the controls to a table for layout.  Layout depends on TextLayout.
            /// </devdoc>
            private void LayoutControls(QuestionContainer questionContainer) {
                LoginTextLayout textLayout = _owner.TextLayout;
                if (textLayout == LoginTextLayout.TextOnLeft) {
                    LayoutTextOnLeft(questionContainer);
                }
                else {
                    LayoutTextOnTop(questionContainer);
                }
            }

            private void LayoutTextOnLeft(QuestionContainer questionContainer) {
                Table table = new Table();
                table.CellPadding = 0;
                TableRow r;
                TableCell c;

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.ColumnSpan = 2;
                c.HorizontalAlign = HorizontalAlign.Center;
                c.Controls.Add(questionContainer.Title);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.ColumnSpan = 2;
                c.HorizontalAlign = HorizontalAlign.Center;
                c.Controls.Add(questionContainer.Instruction);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.HorizontalAlign = HorizontalAlign.Right;
                c.Controls.Add(questionContainer.UserNameLabel);
                r.Cells.Add(c);

                c = new TableCell();
                c.Controls.Add(questionContainer.UserName);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.HorizontalAlign = HorizontalAlign.Right;
                c.Controls.Add(questionContainer.QuestionLabel);
                r.Cells.Add(c);

                c = new TableCell();
                c.Controls.Add(questionContainer.Question);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.HorizontalAlign = HorizontalAlign.Right;
                c.Controls.Add(questionContainer.AnswerLabel);
                if (_owner.ConvertingToTemplate) {
                    questionContainer.AnswerLabel.RenderAsLabel = true;
                }
                r.Cells.Add(c);

                c = new TableCell();
                c.Controls.Add(questionContainer.AnswerTextBox);
                c.Controls.Add(questionContainer.AnswerRequired);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.ColumnSpan = 2;
                c.HorizontalAlign = HorizontalAlign.Center;
                c.Controls.Add(questionContainer.FailureTextLabel);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.ColumnSpan = 2;
                c.HorizontalAlign = HorizontalAlign.Right;
                c.Controls.Add(questionContainer.LinkButton);
                c.Controls.Add(questionContainer.ImageButton);
                c.Controls.Add(questionContainer.PushButton);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.ColumnSpan = 2;
                c.Controls.Add(questionContainer.HelpPageIcon);
                c.Controls.Add(questionContainer.HelpPageLink);
                r.Cells.Add(c);
                table.Rows.Add(r);

                Table table2 = LoginUtil.CreateChildTable(_owner.ConvertingToTemplate);
                r = new TableRow();
                c = new TableCell();
                c.Controls.Add(table);
                r.Cells.Add(c);
                table2.Rows.Add(r);

                questionContainer.LayoutTable = table;
                questionContainer.BorderTable = table2;
                questionContainer.Controls.Add(table2);
            }

            private void LayoutTextOnTop(QuestionContainer questionContainer) {
                Table table = new Table();
                table.CellPadding = 0;
                TableRow r;
                TableCell c;

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.HorizontalAlign = HorizontalAlign.Center;
                c.Controls.Add(questionContainer.Title);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.HorizontalAlign = HorizontalAlign.Center;
                c.Controls.Add(questionContainer.Instruction);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.Controls.Add(questionContainer.UserNameLabel);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.Controls.Add(questionContainer.UserName);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.Controls.Add(questionContainer.QuestionLabel);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.Controls.Add(questionContainer.Question);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.Controls.Add(questionContainer.AnswerLabel);
                if (_owner.ConvertingToTemplate) {
                    questionContainer.AnswerLabel.RenderAsLabel = true;
                }
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.Controls.Add(questionContainer.AnswerTextBox);
                c.Controls.Add(questionContainer.AnswerRequired);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.HorizontalAlign = HorizontalAlign.Center;
                c.Controls.Add(questionContainer.FailureTextLabel);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.HorizontalAlign = HorizontalAlign.Right;
                c.Controls.Add(questionContainer.LinkButton);
                c.Controls.Add(questionContainer.ImageButton);
                c.Controls.Add(questionContainer.PushButton);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.Controls.Add(questionContainer.HelpPageIcon);
                c.Controls.Add(questionContainer.HelpPageLink);
                r.Cells.Add(c);
                table.Rows.Add(r);

                Table table2 = LoginUtil.CreateChildTable(_owner.ConvertingToTemplate);
                r = new TableRow();
                c = new TableCell();
                c.Controls.Add(table);
                r.Cells.Add(c);
                table2.Rows.Add(r);

                questionContainer.LayoutTable = table;
                questionContainer.BorderTable = table2;
                questionContainer.Controls.Add(table2);
            }

            void ITemplate.InstantiateIn(Control container) {
                QuestionContainer questionContainer = (QuestionContainer)container;
                CreateControls(questionContainer);
                LayoutControls(questionContainer);
            }
        }

        /// <devdoc>
        /// The default success template for the control, used if SuccessTemplate is null.
        /// </devdoc>
        private sealed class DefaultSuccessTemplate : ITemplate {
            private PasswordRecovery _owner;

            public DefaultSuccessTemplate(PasswordRecovery owner) {
                _owner = owner;
            }

            private void CreateControls(SuccessContainer successContainer) {
                successContainer.SuccessTextLabel = new Literal();
            }

            private void LayoutControls(SuccessContainer successContainer) {
                Table table = new Table();
                table.CellPadding = 0;
                TableRow r = new LoginUtil.DisappearingTableRow();
                TableCell c = new TableCell();

                c.Controls.Add(successContainer.SuccessTextLabel);
                r.Cells.Add(c);
                table.Rows.Add(r);

                // Extra table for border padding
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
                SuccessContainer successContainer = (SuccessContainer)container;
                CreateControls(successContainer);
                LayoutControls(successContainer);
            }
        }

        /// <devdoc>
        /// The default user name template for the control, used if UserNameTemplate is null.
        /// </devdoc>
        private sealed class DefaultUserNameTemplate : ITemplate {
            private PasswordRecovery _owner;

            public DefaultUserNameTemplate(PasswordRecovery owner) {
                _owner = owner;
            }

            private void CreateControls(UserNameContainer userNameContainer) {
                string validationGroup = _owner.UniqueID;

                userNameContainer.Title = new Literal();
                userNameContainer.Instruction = new Literal();

                TextBox userNameTextBox = new TextBox();
                // Must explicitly set the ID of controls that raise postback events
                userNameTextBox.ID = _userNameID;
                userNameContainer.UserNameTextBox = userNameTextBox;
                userNameContainer.UserNameLabel = new LabelLiteral(userNameTextBox);

                bool enableValidation = (_owner.CurrentView == View.UserName);
                RequiredFieldValidator userNameRequired = new RequiredFieldValidator();
                userNameRequired.ID = _userNameRequiredID;
                userNameRequired.ValidationGroup = validationGroup;
                userNameRequired.ControlToValidate = userNameTextBox.ID;
                userNameRequired.Display = _requiredFieldValidatorDisplay;
                userNameRequired.Text = SR.GetString(SR.LoginControls_DefaultRequiredFieldValidatorText);
                userNameRequired.Enabled = enableValidation;
                userNameRequired.Visible = enableValidation;
                userNameContainer.UserNameRequired = userNameRequired;

                LinkButton linkButton = new LinkButton();
                linkButton.ID = _linkButtonID;
                linkButton.ValidationGroup = validationGroup;
                linkButton.CommandName = SubmitButtonCommandName;
                userNameContainer.LinkButton = linkButton;

                ImageButton imageButton = new ImageButton();
                imageButton.ID = _imageButtonID;
                imageButton.ValidationGroup = validationGroup;
                imageButton.CommandName = SubmitButtonCommandName;
                userNameContainer.ImageButton = imageButton;

                Button pushButton = new Button();
                pushButton.ID = _pushButtonID;
                pushButton.ValidationGroup = validationGroup;
                pushButton.CommandName = SubmitButtonCommandName;
                userNameContainer.PushButton = pushButton;

                userNameContainer.HelpPageLink = new HyperLink();
                userNameContainer.HelpPageLink.ID = _helpLinkID;
                userNameContainer.HelpPageIcon = new Image();

                Literal failureTextLabel = new Literal();
                failureTextLabel.ID = _failureTextID;
                userNameContainer.FailureTextLabel = failureTextLabel;
            }

            private void LayoutControls(UserNameContainer userNameContainer) {
                LoginTextLayout textLayout = _owner.TextLayout;
                if (textLayout == LoginTextLayout.TextOnLeft) {
                    LayoutTextOnLeft(userNameContainer);
                }
                else {
                    LayoutTextOnTop(userNameContainer);
                }
            }

            private void LayoutTextOnLeft(UserNameContainer userNameContainer) {
                Table table = new Table();
                table.CellPadding = 0;
                TableRow r;
                TableCell c;

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.ColumnSpan = 2;
                c.HorizontalAlign = HorizontalAlign.Center;
                c.Controls.Add(userNameContainer.Title);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.ColumnSpan = 2;
                c.HorizontalAlign = HorizontalAlign.Center;
                c.Controls.Add(userNameContainer.Instruction);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.HorizontalAlign = HorizontalAlign.Right;
                c.Controls.Add(userNameContainer.UserNameLabel);
                if (_owner.ConvertingToTemplate) {
                    userNameContainer.UserNameLabel.RenderAsLabel = true;
                }
                r.Cells.Add(c);

                c = new TableCell();
                c.Controls.Add(userNameContainer.UserNameTextBox);
                c.Controls.Add(userNameContainer.UserNameRequired);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.ColumnSpan = 2;
                c.HorizontalAlign = HorizontalAlign.Center;
                c.Controls.Add(userNameContainer.FailureTextLabel);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.ColumnSpan = 2;
                c.HorizontalAlign = HorizontalAlign.Right;
                c.Controls.Add(userNameContainer.LinkButton);
                c.Controls.Add(userNameContainer.ImageButton);
                c.Controls.Add(userNameContainer.PushButton);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.ColumnSpan = 2;
                c.Controls.Add(userNameContainer.HelpPageIcon);
                c.Controls.Add(userNameContainer.HelpPageLink);
                r.Cells.Add(c);
                table.Rows.Add(r);

                Table table2 = LoginUtil.CreateChildTable(_owner.ConvertingToTemplate);
                r = new TableRow();
                c = new TableCell();
                c.Controls.Add(table);
                r.Cells.Add(c);
                table2.Rows.Add(r);

                userNameContainer.LayoutTable = table;
                userNameContainer.BorderTable = table2;
                userNameContainer.Controls.Add(table2);
            }

            private void LayoutTextOnTop(UserNameContainer userNameContainer) {
                Table table = new Table();
                table.CellPadding = 0;
                TableRow r;
                TableCell c;

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.HorizontalAlign = HorizontalAlign.Center;
                c.Controls.Add(userNameContainer.Title);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.HorizontalAlign = HorizontalAlign.Center;
                c.Controls.Add(userNameContainer.Instruction);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.Controls.Add(userNameContainer.UserNameLabel);
                if (_owner.ConvertingToTemplate) {
                    userNameContainer.UserNameLabel.RenderAsLabel = true;
                }
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.Controls.Add(userNameContainer.UserNameTextBox);
                c.Controls.Add(userNameContainer.UserNameRequired);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.HorizontalAlign = HorizontalAlign.Center;
                c.Controls.Add(userNameContainer.FailureTextLabel);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.HorizontalAlign = HorizontalAlign.Right;
                c.Controls.Add(userNameContainer.LinkButton);
                c.Controls.Add(userNameContainer.ImageButton);
                c.Controls.Add(userNameContainer.PushButton);
                r.Cells.Add(c);
                table.Rows.Add(r);

                r = new LoginUtil.DisappearingTableRow();
                c = new TableCell();
                c.Controls.Add(userNameContainer.HelpPageIcon);
                c.Controls.Add(userNameContainer.HelpPageLink);
                r.Cells.Add(c);
                table.Rows.Add(r);

                Table table2 = LoginUtil.CreateChildTable(_owner.ConvertingToTemplate);
                r = new TableRow();
                c = new TableCell();
                c.Controls.Add(table);
                r.Cells.Add(c);
                table2.Rows.Add(r);

                userNameContainer.LayoutTable = table;
                userNameContainer.BorderTable = table2;
                userNameContainer.Controls.Add(table2);
            }

            void ITemplate.InstantiateIn(Control container) {
                UserNameContainer userNameContainer = (UserNameContainer)container;
                CreateControls(userNameContainer);
                LayoutControls(userNameContainer);
            }
        }


        /// <devdoc>
        ///     Container for the question template.  Contains properties that reference each child control.
        ///     For the default template, the properties are set when the child controls are created.
        ///     For the user template, the controls are looked up dynamically by ID.  Some controls are required,
        ///     and an exception is thrown if they are missing.  Other controls are optional, and an exception is
        ///     thrown if they have the wrong type.
        ///     Internal instead of private because it must be used by PasswordRecoveryAdapter.
        /// </devdoc>
        internal sealed class QuestionContainer : LoginUtil.GenericContainer<PasswordRecovery>, INonBindingContainer {
            private LabelLiteral _answerLabel;
            private RequiredFieldValidator _answerRequired;
            private Control _answerTextBox;
            private Control _failureTextLabel;
            private HyperLink _helpPageLink;
            private Image _helpPageIcon;
            private ImageButton _imageButton;
            private Literal _instruction;
            private LinkButton _linkButton;
            private Button _pushButton;
            private Control _question;
            private Literal _questionLabel;
            private Literal _title;
            private Literal _userNameLabel;
            private Control _userName;

            public QuestionContainer(PasswordRecovery owner) : base(owner) {
            }

            public LabelLiteral AnswerLabel {
                get {
                    return _answerLabel;
                }
                set {
                    _answerLabel = value;
                }
            }

            public RequiredFieldValidator AnswerRequired {
                get {
                    return _answerRequired;
                }

                set {
                    _answerRequired = value;
                }
            }

            public Control AnswerTextBox {
                get {
                    if (_answerTextBox != null) {
                        return _answerTextBox;
                    }
                    else {
                        return FindRequiredControl<IEditableTextControl>(_answerID, SR.PasswordRecovery_NoAnswerTextBox);
                    }
                }
                set {
                    _answerTextBox = value;
                }
            }

            protected override bool ConvertingToTemplate {
                get {
                    return Owner.ConvertingToTemplate;
                }
            }

            public Control FailureTextLabel {
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

            public Image HelpPageIcon {
                get {
                    return _helpPageIcon;
                }
                set {
                    _helpPageIcon = value;
                }
            }

            public HyperLink HelpPageLink {
                get {
                    return _helpPageLink;
                }
                set {
                    _helpPageLink = value;
                }
            }

            public ImageButton ImageButton {
                get {
                    return _imageButton;
                }
                set {
                    _imageButton = value;
                }
            }

            public Literal Instruction {
                get {
                    return _instruction;
                }
                set {
                    _instruction = value;
                }
            }

            public LinkButton LinkButton {
                get {
                    return _linkButton;
                }
                set {
                    _linkButton = value;
                }
            }

            public Button PushButton {
                get {
                    return _pushButton;
                }
                set {
                    _pushButton = value;
                }
            }

            public Control Question {
                get {
                    if (_question != null) {
                        return _question;
                    }
                    else {
                        return FindOptionalControl<ITextControl>(_questionID);
                    }
                }
                set {
                    _question = value;
                }
            }

            public Literal QuestionLabel {
                get {
                    return _questionLabel;
                }
                set {
                    _questionLabel = value;
                }
            }

            public Literal Title {
                get {
                    return _title;
                }
                set {
                    _title = value;
                }
            }

            public Control UserName {
                get {
                    if (_userName != null) {
                        return _userName;
                    }
                    else {
                        return FindOptionalControl<ITextControl>(_userNameID);
                    }
                }
                set {
                    _userName = value;
                }
            }

            public Literal UserNameLabel {
                get {
                    return _userNameLabel;
                }
                set {
                    _userNameLabel = value;
                }
            }
        }

        /// <devdoc>
        ///     Container for the success template.
        ///     Internal instead of private because it must be used by PasswordRecoveryAdapter.
        /// </devdoc>
        internal sealed class SuccessContainer : LoginUtil.GenericContainer<PasswordRecovery>, INonBindingContainer {
            private Literal _successTextLabel;

            public SuccessContainer(PasswordRecovery owner) : base(owner) {
            }

            protected override bool ConvertingToTemplate {
                get {
                    return Owner.ConvertingToTemplate;
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
        }

        /// <devdoc>
        ///     Container for the user name template.  Contains properties that reference each child control.
        ///     For the default template, the properties are set when the child controls are created.
        ///     For the user template, the controls are looked up dynamically by ID.  Some controls are required,
        ///     and an exception is thrown if they are missing.  Other controls are optional, and an exception is
        ///     thrown if they have the wrong type.
        ///     Internal instead of private because it must be used by PasswordRecoveryAdapter.
        /// </devdoc>
        internal sealed class UserNameContainer : LoginUtil.GenericContainer<PasswordRecovery>, INonBindingContainer {
            private Control _failureTextLabel;
            private Image _helpPageIcon;
            private HyperLink _helpPageLink;
            private ImageButton _imageButton;
            private Literal _instruction;
            private LinkButton _linkButton;
            private Button _pushButton;
            private Literal _title;
            private LabelLiteral _userNameLabel;
            private RequiredFieldValidator _userNameRequired;
            private Control _userNameTextBox;

            public UserNameContainer(PasswordRecovery owner) : base(owner) {
            }

            protected override bool ConvertingToTemplate {
                get {
                    return Owner.ConvertingToTemplate;
                }
            }

            public Control FailureTextLabel {
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

            public Image HelpPageIcon {
                get {
                    return _helpPageIcon;
                }
                set {
                    _helpPageIcon = value;
                }
            }

            public HyperLink HelpPageLink {
                get {
                    return _helpPageLink;
                }
                set {
                    _helpPageLink = value;
                }
            }

            public ImageButton ImageButton {
                get {
                    return _imageButton;
                }
                set {
                    _imageButton = value;
                }
            }

            public Literal Instruction {
                get {
                    return _instruction;
                }
                set {
                    _instruction = value;
                }
            }

            public LinkButton LinkButton {
                get {
                    return _linkButton;
                }
                set {
                    _linkButton = value;
                }
            }

            public Button PushButton {
                get {
                    return _pushButton;
                }
                set {
                    _pushButton = value;
                }
            }

            public Literal Title {
                get {
                    return _title;
                }
                set {
                    _title = value;
                }
            }

            public LabelLiteral UserNameLabel {
                get {
                    return _userNameLabel;
                }
                set {
                    _userNameLabel = value;
                }
            }

            public RequiredFieldValidator UserNameRequired {
                get {
                    return _userNameRequired;
                }

                set {
                    _userNameRequired = value;
                }
            }

            public Control UserNameTextBox {
                get {
                    if (_userNameTextBox != null) {
                        return _userNameTextBox;
                    }
                    else {
                        return FindRequiredControl<IEditableTextControl>(_userNameID, SR.PasswordRecovery_NoUserNameTextBox);
                    }
                }
                set {
                    _userNameTextBox = value;
                }
            }
        }

        /// <devdoc>
        /// Internal because used from PasswordRecoveryAdapter
        /// </devdoc>
        internal enum View {
            UserName,
            Question,
            Success
        }
    }
}
