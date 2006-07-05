//
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
//
// Authors:
//	Vladimir Krasnov <vladimirk@mainsoft.com>
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

using System;
using System.Web;
using System.Web.UI;
using System.Web.Security;
using System.Collections;
using System.ComponentModel;
using System.Text;

namespace System.Web.UI.WebControls
{
	[BindableAttribute (false)]
	public class CreateUserWizard : Wizard
	{
		public static readonly string ContinueButtonCommandName;
		private string _password = "";
		private string _confirmPassword = "";
		private MembershipProvider _provider = null;
		private Label _errorMessageLabel = null;

		private Style _textBoxStyle = null;
		private Style _validatorTextStyle = null;

		private TableItemStyle _completeSuccessTextStyle = null;
		private TableItemStyle _errorMessageStyle = null;
		private TableItemStyle _hyperLinkStyle = null;
		private TableItemStyle _instructionTextStyle = null;
		private TableItemStyle _labelStyle = null;
		private TableItemStyle _passwordHintStyle = null;
		private TableItemStyle _titleTextStyle = null;

		private static readonly object CreatedUserEvent = new object ();
		private static readonly object CreateUserErrorEvent = new object ();
		private static readonly object CreatingUserEvent = new object ();
		private static readonly object ContinueButtonClickEvent = new object ();
		private static readonly object SendingMailEvent = new object ();
		private static readonly object SendMailErrorEvent = new object ();

		private CompleteWizardStep _completeWizardStep = null;
		private CreateUserWizardStep _createUserWizardStep = null;

		public CreateUserWizard ()
		{
		}

		#region Public Properties

		public override int ActiveStepIndex
		{
			get { return base.ActiveStepIndex; }
			set { base.ActiveStepIndex = value; }
		}

		[LocalizableAttribute (true)]
		[ThemeableAttribute (false)]
		public virtual string Answer
		{
			get
			{
				object o = ViewState ["Answer"];
				return (o == null) ? String.Empty : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("Answer");
				else
					ViewState ["Answer"] = value;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string AnswerLabelText
		{
			get
			{
				object o = ViewState ["AnswerLabelText"];
				return (o == null) ? Locale.GetText ("Security Answer:") : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("AnswerLabelText");
				else
					ViewState ["AnswerLabelText"] = value;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string AnswerRequiredErrorMessage
		{
			get
			{
				object o = ViewState ["AnswerRequiredErrorMessage"];
				return (o == null) ? Locale.GetText ("Security answer is required.") : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("AnswerRequiredErrorMessage");
				else
					ViewState ["AnswerRequiredErrorMessage"] = value;
			}
		}

		[ThemeableAttribute (false)]
		public virtual bool AutoGeneratePassword
		{
			get
			{
				object o = ViewState ["AutoGeneratePassword"];
				return (o == null) ? false : (bool) o;
			}
			set
			{
				ViewState ["AutoGeneratePassword"] = value;
			}
		}

		public CompleteWizardStep CompleteStep
		{
			get
			{
				if (_completeWizardStep == null) {
					for (int i = 0; i < WizardSteps.Count; i++)
						if (WizardSteps [i] is CompleteWizardStep) {
							_completeWizardStep = (CompleteWizardStep) WizardSteps [i];

							if (_completeWizardStep.Wizard == null)
								_completeWizardStep.SetWizard (this);
						}
				}
				return _completeWizardStep;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string CompleteSuccessText
		{
			get
			{
				object o = ViewState ["CompleteSuccessText"];
				return (o == null) ? Locale.GetText ("Your account has been successfully created.") : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("CompleteSuccessText");
				else
					ViewState ["CompleteSuccessText"] = value;
			}
		}

		public TableItemStyle CompleteSuccessTextStyle
		{
			get
			{
				if (_completeSuccessTextStyle == null) {
					_completeSuccessTextStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						((IStateManager) _completeSuccessTextStyle).TrackViewState ();
				}
				return _completeSuccessTextStyle;
			}
		}

		public virtual string ConfirmPassword
		{
			get { return _confirmPassword; }
		}

		[LocalizableAttribute (true)]
		public virtual string ConfirmPasswordCompareErrorMessage
		{
			get
			{
				object o = ViewState ["ConfirmPasswordCompareErrorMessage"];
				return (o == null) ? Locale.GetText ("The Password and Confirmation Password must match.") : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("ConfirmPasswordCompareErrorMessage");
				else
					ViewState ["ConfirmPasswordCompareErrorMessage"] = value;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string ConfirmPasswordLabelText
		{
			get
			{
				object o = ViewState ["ConfirmPasswordLabelText"];
				return (o == null) ? Locale.GetText ("Confirm Password:") : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("ConfirmPasswordLabelText");
				else
					ViewState ["ConfirmPasswordLabelText"] = value;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string ConfirmPasswordRequiredErrorMessage
		{
			get
			{
				object o = ViewState ["ConfirmPasswordRequiredErrorMessage"];
				return (o == null) ? Locale.GetText ("Confirm Password is required.") : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("ConfirmPasswordRequiredErrorMessage");
				else
					ViewState ["ConfirmPasswordRequiredErrorMessage"] = value;
			}
		}

		public virtual string ContinueButtonImageUrl
		{
			get { return base.FinishCompleteButtonImageUrl; }
			set { base.FinishCompleteButtonImageUrl = value; }
		}

		public Style ContinueButtonStyle
		{
			get
			{
				return base.FinishCompleteButtonStyle;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string ContinueButtonText
		{
			get
			{
				object o = ViewState ["ContinueButtonText"];
				return (o == null) ? Locale.GetText ("Continue") : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("ContinueButtonText");
				else
					ViewState ["ContinueButtonText"] = value;
			}
		}

		[MonoTODO ("This should be saved to viewstate")]
		public virtual ButtonType ContinueButtonType
		{
			get { return base.FinishCompleteButtonType; }
			set { base.FinishCompleteButtonType = value; }
		}

		[ThemeableAttribute (false)]
		public virtual string ContinueDestinationPageUrl
		{
			get
			{
				object o = ViewState ["ContinueDestinationPageUrl"];
				return (o == null) ? "" : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("ContinueDestinationPageUrl");
				else
					ViewState ["ContinueDestinationPageUrl"] = value;
			}
		}

		public virtual string CreateUserButtonImageUrl
		{
			get { return base.StartNextButtonImageUrl; }
			set { StartNextButtonImageUrl = value; }
		}

		public Style CreateUserButtonStyle
		{
			get
			{
				return base.StartNextButtonStyle;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string CreateUserButtonText
		{
			get
			{
				object o = ViewState ["CreateUserButtonText"];
				return (o == null) ? Locale.GetText ("Create User") : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("CreateUserButtonText");
				else
					ViewState ["CreateUserButtonText"] = value;
			}
		}

		[MonoTODO ("This should be saved to viewstate")]
		public virtual ButtonType CreateUserButtonType
		{
			get { return base.StartNextButtonType; }
			set { base.StartNextButtonType = value; }
		}

		public CreateUserWizardStep CreateUserStep
		{
			get
			{
				if (_createUserWizardStep == null) {
					for (int i = 0; i < WizardSteps.Count; i++)
						if (WizardSteps [i] is CreateUserWizardStep) {
							_createUserWizardStep = (CreateUserWizardStep) WizardSteps [i];

							if (_createUserWizardStep.Wizard == null)
								_createUserWizardStep.SetWizard (this);
						}
				}
				return _createUserWizardStep;
			}
		}

		[ThemeableAttribute (false)]
		public virtual bool DisableCreatedUser
		{
			get
			{
				object o = ViewState ["DisableCreatedUser"];
				return (o == null) ? false : (bool) o;
			}
			set
			{
				ViewState ["DisableCreatedUser"] = value;
			}
		}

		public override bool DisplaySideBar
		{
			get { return base.DisplaySideBar; }
			set { base.DisplaySideBar = value; }
		}

		[LocalizableAttribute (true)]
		public virtual string DuplicateEmailErrorMessage
		{
			get
			{
				object o = ViewState ["DuplicateEmailErrorMessage"];
				return (o == null) ? Locale.GetText ("The e-mail address that you entered is already in use. Please enter a different e-mail address.") : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("DuplicateEmailErrorMessage");
				else
					ViewState ["DuplicateEmailErrorMessage"] = value;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string DuplicateUserNameErrorMessage
		{
			get
			{
				object o = ViewState ["DuplicateUserNameErrorMessage"];
				return (o == null) ? Locale.GetText ("Please enter a different user name.") : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("DuplicateUserNameErrorMessage");
				else
					ViewState ["DuplicateUserNameErrorMessage"] = value;
			}
		}

		public virtual string EditProfileIconUrl
		{
			get
			{
				object o = ViewState ["EditProfileIconUrl"];
				return (o == null) ? "" : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("EditProfileIconUrl");
				else
					ViewState ["EditProfileIconUrl"] = value;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string EditProfileText
		{
			get
			{
				object o = ViewState ["EditProfileText"];
				return (o == null) ? "" : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("EditProfileText");
				else
					ViewState ["EditProfileText"] = value;
			}
		}

		public virtual string EditProfileUrl
		{
			get
			{
				object o = ViewState ["EditProfileUrl"];
				return (o == null) ? "" : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("EditProfileUrl");
				else
					ViewState ["EditProfileUrl"] = value;
			}
		}

		public virtual string Email
		{
			get
			{
				object o = ViewState ["Email"];
				return (o == null) ? String.Empty : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("Email");
				else
					ViewState ["Email"] = value;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string EmailLabelText
		{
			get
			{
				object o = ViewState ["EmailLabelText"];
				return (o == null) ? Locale.GetText ("E-mail:") : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("EmailLabelText");
				else
					ViewState ["EmailLabelText"] = value;
			}
		}

		public virtual string EmailRegularExpression
		{
			get
			{
				object o = ViewState ["EmailRegularExpression"];
				return (o == null) ? "" : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("EmailRegularExpression");
				else
					ViewState ["EmailRegularExpression"] = value;
			}
		}

		public virtual string EmailRegularExpressionErrorMessage
		{
			get
			{
				object o = ViewState ["EmailRegularExpressionErrorMessage"];
				return (o == null) ? Locale.GetText ("Please enter a different e-mail.") : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("EmailRegularExpressionErrorMessage");
				else
					ViewState ["EmailRegularExpressionErrorMessage"] = value;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string EmailRequiredErrorMessage
		{
			get
			{
				object o = ViewState ["EmailRequiredErrorMessage"];
				return (o == null) ? Locale.GetText ("E-mail is required.") : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("EmailRequiredErrorMessage");
				else
					ViewState ["EmailRequiredErrorMessage"] = value;
			}
		}

		public TableItemStyle ErrorMessageStyle
		{
			get
			{
				if (_errorMessageStyle == null) {
					_errorMessageStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						((IStateManager) _errorMessageStyle).TrackViewState ();
				}
				return _errorMessageStyle;
			}
		}

		public virtual string HelpPageIconUrl
		{
			get
			{
				object o = ViewState ["HelpPageIconUrl"];
				return (o == null) ? "" : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("HelpPageIconUrl");
				else
					ViewState ["HelpPageIconUrl"] = value;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string HelpPageText
		{
			get
			{
				object o = ViewState ["HelpPageText"];
				return (o == null) ? "" : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("HelpPageText");
				else
					ViewState ["HelpPageText"] = value;
			}
		}

		public virtual string HelpPageUrl
		{
			get
			{
				object o = ViewState ["HelpPageUrl"];
				return (o == null) ? "" : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("HelpPageUrl");
				else
					ViewState ["HelpPageUrl"] = value;
			}
		}

		public TableItemStyle HyperLinkStyle
		{
			get
			{
				if (_hyperLinkStyle == null) {
					_hyperLinkStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						((IStateManager) _hyperLinkStyle).TrackViewState ();
				}
				return _hyperLinkStyle;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string InstructionText
		{
			get
			{
				object o = ViewState ["InstructionText"];
				return (o == null) ? "" : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("InstructionText");
				else
					ViewState ["InstructionText"] = value;
			}
		}

		public TableItemStyle InstructionTextStyle
		{
			get
			{
				if (_instructionTextStyle == null) {
					_instructionTextStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						((IStateManager) _instructionTextStyle).TrackViewState ();
				}
				return _instructionTextStyle;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string InvalidAnswerErrorMessage
		{
			get
			{
				object o = ViewState ["InvalidAnswerErrorMessage"];
				return (o == null) ? Locale.GetText ("Please enter a different security answer.") : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("InvalidAnswerErrorMessage");
				else
					ViewState ["InvalidAnswerErrorMessage"] = value;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string InvalidEmailErrorMessage
		{
			get
			{
				object o = ViewState ["InvalidEmailErrorMessage"];
				return (o == null) ? Locale.GetText ("Please enter a valid e-mail address.") : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("InvalidEmailErrorMessage");
				else
					ViewState ["InvalidEmailErrorMessage"] = value;
			}
		}

		[MonoTODO ("take the values from membership provider")]
		[LocalizableAttribute (true)]
		public virtual string InvalidPasswordErrorMessage
		{
			get
			{
				object o = ViewState ["InvalidPasswordErrorMessage"];
				return (o == null) ? Locale.GetText ("Password length minimum: {0}. Non-alphanumeric characters required: {1}.") : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("InvalidPasswordErrorMessage");
				else
					ViewState ["InvalidPasswordErrorMessage"] = value;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string InvalidQuestionErrorMessage
		{
			get
			{
				object o = ViewState ["InvalidQuestionErrorMessage"];
				return (o == null) ? Locale.GetText ("Please enter a different security question.") : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("InvalidQuestionErrorMessage");
				else
					ViewState ["InvalidQuestionErrorMessage"] = value;
			}
		}

		public TableItemStyle LabelStyle
		{
			get
			{
				if (_labelStyle == null) {
					_labelStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						((IStateManager) _labelStyle).TrackViewState ();
				}
				return _labelStyle;
			}
		}

		[ThemeableAttribute (false)]
		public virtual bool LoginCreatedUser
		{
			get
			{
				object o = ViewState ["LoginCreatedUser"];
				return (o == null) ? true : (bool) o;
			}
			set
			{
				ViewState ["LoginCreatedUser"] = value;
			}
		}

		//[MonoTODO ("Sending mail functionality is not implemented")]
		//[ThemeableAttribute (false)]
		//public MailDefinition MailDefinition
		//{
		//	get { throw new NotImplementedException (); }
		//}

		[ThemeableAttribute (false)]
		public virtual string MembershipProvider
		{
			get
			{
				object o = ViewState ["MembershipProvider"];
				return (o == null) ? "" : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("MembershipProvider");
				else
					ViewState ["MembershipProvider"] = value;

				_provider = null;
			}
		}

		internal virtual MembershipProvider MembershipProviderInternal
		{
			get
			{
				if (_provider == null)
					InitMemberShipProvider ();

				return _provider;
			}
		}

		public virtual string Password
		{
			get { return _password; }
		}

		public TableItemStyle PasswordHintStyle
		{
			get
			{
				if (_passwordHintStyle == null) {
					_passwordHintStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						((IStateManager) _passwordHintStyle).TrackViewState ();
				}
				return _passwordHintStyle;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string PasswordHintText
		{
			get
			{
				object o = ViewState ["PasswordHintText"];
				return (o == null) ? "" : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("PasswordHintText");
				else
					ViewState ["PasswordHintText"] = value;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string PasswordLabelText
		{
			get
			{
				object o = ViewState ["PasswordLabelText"];
				return (o == null) ? Locale.GetText ("Password:") : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("PasswordLabelText");
				else
					ViewState ["PasswordLabelText"] = value;
			}
		}

		public virtual string PasswordRegularExpression
		{
			get
			{
				object o = ViewState ["PasswordRegularExpression"];
				return (o == null) ? "" : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("PasswordRegularExpression");
				else
					ViewState ["PasswordRegularExpression"] = value;
			}
		}

		public virtual string PasswordRegularExpressionErrorMessage
		{
			get
			{
				object o = ViewState ["PasswordRegularExpressionErrorMessage"];
				return (o == null) ? Locale.GetText ("Please enter a different password.") : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("PasswordRegularExpressionErrorMessage");
				else
					ViewState ["PasswordRegularExpressionErrorMessage"] = value;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string PasswordRequiredErrorMessage
		{
			get
			{
				object o = ViewState ["PasswordRequiredErrorMessage"];
				return (o == null) ? Locale.GetText ("Password is required.") : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("PasswordRequiredErrorMessage");
				else
					ViewState ["PasswordRequiredErrorMessage"] = value;
			}
		}

		[LocalizableAttribute (true)]
		[ThemeableAttribute (false)]
		public virtual string Question
		{
			get
			{
				object o = ViewState ["Question"];
				return (o == null) ? String.Empty : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("Question");
				else
					ViewState ["Question"] = value;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string QuestionLabelText
		{
			get
			{
				object o = ViewState ["QuestionLabelText"];
				return (o == null) ? Locale.GetText ("Security Question:") : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("QuestionLabelText");
				else
					ViewState ["QuestionLabelText"] = value;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string QuestionRequiredErrorMessage
		{
			get
			{
				object o = ViewState ["QuestionRequiredErrorMessage"];
				return (o == null) ? Locale.GetText ("Security question is required.") : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("QuestionRequiredErrorMessage");
				else
					ViewState ["QuestionRequiredErrorMessage"] = value;
			}
		}

		[ThemeableAttribute (false)]
		public virtual bool RequireEmail
		{
			get
			{
				object o = ViewState ["RequireEmail"];
				return (o == null) ? true : (bool) o;
			}
			set
			{
				ViewState ["RequireEmail"] = value;
			}
		}

		[MonoTODO ("doesnt work")]
		public override string SkipLinkText
		{
			get
			{
				object o = ViewState ["SkipLinkText"];
				return (o == null) ? "" : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("SkipLinkText");
				else
					ViewState ["SkipLinkText"] = value;
			}
		}

		public Style TextBoxStyle
		{
			get
			{
				if (_textBoxStyle == null) {
					_textBoxStyle = new Style ();
					if (IsTrackingViewState)
						((IStateManager) _textBoxStyle).TrackViewState ();
				}
				return _textBoxStyle;
			}
		}

		public TableItemStyle TitleTextStyle
		{
			get
			{
				if (_titleTextStyle == null) {
					_titleTextStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						((IStateManager) _titleTextStyle).TrackViewState ();
				}
				return _titleTextStyle;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string UnknownErrorMessage
		{
			get
			{
				object o = ViewState ["UnknownErrorMessage"];
				return (o == null) ? Locale.GetText ("Your account was not created. Please try again.") : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("UnknownErrorMessage");
				else
					ViewState ["UnknownErrorMessage"] = value;
			}
		}

		public virtual string UserName
		{
			get
			{
				object o = ViewState ["UserName"];
				return (o == null) ? String.Empty : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("UserName");
				else
					ViewState ["UserName"] = value;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string UserNameLabelText
		{
			get
			{
				object o = ViewState ["UserNameLabelText"];
				return (o == null) ? Locale.GetText ("User Name:") : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("UserNameLabelText");
				else
					ViewState ["UserNameLabelText"] = value;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string UserNameRequiredErrorMessage
		{
			get
			{
				object o = ViewState ["UserNameRequiredErrorMessage"];
				return (o == null) ? Locale.GetText ("User Name is required.") : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("UserNameRequiredErrorMessage");
				else
					ViewState ["UserNameRequiredErrorMessage"] = value;
			}
		}

		public Style ValidatorTextStyle
		{
			get
			{
				if (_validatorTextStyle == null) {
					_validatorTextStyle = new Style ();
					if (IsTrackingViewState)
						((IStateManager) _validatorTextStyle).TrackViewState ();
				}
				return _validatorTextStyle;
			}
		}

		[ThemeableAttribute (false)]
		public override WizardStepCollection WizardSteps
		{
			get { return base.WizardSteps; }
		}

		#endregion

		#region Protected Properties

		protected internal bool QuestionAndAnswerRequired
		{
			get { return MembershipProviderInternal.RequiresQuestionAndAnswer; }
		}

		public event EventHandler ContinueButtonClick
		{
			add { Events.AddHandler (ContinueButtonClickEvent, value); }
			remove { Events.RemoveHandler (ContinueButtonClickEvent, value); }
		}

		public event EventHandler CreatedUser
		{
			add { Events.AddHandler (CreatedUserEvent, value); }
			remove { Events.RemoveHandler (CreatedUserEvent, value); }
		}

		public event CreateUserErrorEventHandler CreateUserError
		{
			add { Events.AddHandler (CreateUserErrorEvent, value); }
			remove { Events.RemoveHandler (CreateUserErrorEvent, value); }
		}

		public event LoginCancelEventHandler CreatingUser
		{
			add { Events.AddHandler (CreatingUserEvent, value); }
			remove { Events.RemoveHandler (CreatingUserEvent, value); }
		}

		public event MailMessageEventHandler SendingMail
		{
			add { Events.AddHandler (SendingMailEvent, value); }
			remove { Events.RemoveHandler (SendingMailEvent, value); }
		}

		public event SendMailErrorEventHandler SendMailError
		{
			add { Events.AddHandler (SendMailErrorEvent, value); }
			remove { Events.RemoveHandler (SendMailErrorEvent, value); }
		}


		#endregion

		#region Internal Properties

		internal override void InstantiateTemplateStep (TemplatedWizardStep step)
		{
			base.InstantiateTemplateStep (step);
		}

		internal override ITemplate SideBarItemTemplate
		{
			get { return new SideBarLabelTemplate (this); }
		}

		#endregion

		#region Protected Methods

		protected internal override void CreateChildControls ()
		{
			base.FinishCompleteButtonText = ContinueButtonText;
			base.StartNextButtonText = CreateUserButtonText;

			CreateUserWizardStep c = CreateUserStep;

			base.CreateChildControls ();
		}

		protected override void CreateControlHierarchy ()
		{
			base.CreateControlHierarchy ();

			CreateUserStepContainer container = CreateUserStep.ContentTemplateContainer as CreateUserStepContainer;

			if (container != null) {
				IEditableTextControl editable;
				editable = container.UserNameTextBox as IEditableTextControl;

				if (editable != null)
					editable.TextChanged += new EventHandler (UserName_TextChanged);

				if (!AutoGeneratePassword) {
					editable = container.PasswordTextBox as IEditableTextControl;

					if (editable != null)
						editable.TextChanged += new EventHandler (Password_TextChanged);

					editable = container.ConfirmPasswordTextBox as IEditableTextControl;

					if (editable != null)
						editable.TextChanged += new EventHandler (ConfirmPassword_TextChanged);
				}

				if (RequireEmail) {
					editable = container.EmailTextBox as IEditableTextControl;

					if (editable != null)
						editable.TextChanged += new EventHandler (Email_TextChanged);
				}

				if (QuestionAndAnswerRequired) {
					editable = container.QuestionTextBox as IEditableTextControl;

					if (editable != null)
						editable.TextChanged += new EventHandler (Question_TextChanged);

					editable = container.AnswerTextBox as IEditableTextControl;

					if (editable != null)
						editable.TextChanged += new EventHandler (Answer_TextChanged);
				}

				_errorMessageLabel = container.ErrorMessageLabel;
			}
		}

		[MonoTODO ("Not Implemented")]
		protected override IDictionary GetDesignModeState ()
		{
			throw new NotImplementedException ();
		}

		protected internal override void OnInit (EventArgs e)
		{
			base.OnInit (e);

			DisplaySideBar = false;

			if (CreateUserStep == null)
				WizardSteps.AddAt (0, new CreateUserWizardStep ());

			if (CompleteStep == null)
				WizardSteps.AddAt (WizardSteps.Count, new CompleteWizardStep ());
		}

		protected override bool OnBubbleEvent (object source, EventArgs e)
		{
			return base.OnBubbleEvent (source, e);
		}

		protected virtual void OnContinueButtonClick (EventArgs e)
		{
			if (Events != null) {
				EventHandler eh = (EventHandler) Events [ContinueButtonClickEvent];
				if (eh != null) eh (this, e);
			}
		}

		protected virtual void OnCreatedUser (EventArgs e)
		{
			if (Events != null) {
				EventHandler eh = (EventHandler) Events [CreatedUserEvent];
				if (eh != null) eh (this, e);
			}
		}

		protected virtual void OnCreateUserError (CreateUserErrorEventArgs e)
		{
			if (Events != null) {
				CreateUserErrorEventHandler eh = (CreateUserErrorEventHandler) Events [CreateUserErrorEvent];
				if (eh != null) eh (this, e);
			}
		}

		protected virtual void OnCreatingUser (LoginCancelEventArgs e)
		{
			if (Events != null) {
				LoginCancelEventHandler eh = (LoginCancelEventHandler) Events [CreatingUserEvent];
				if (eh != null) eh (this, e);
			}
		}

		protected override void OnNextButtonClick (WizardNavigationEventArgs e)
		{
			bool userCreated = CreateUser ();
			if (!userCreated)
				e.Cancel = true;
			else
				if (LoginCreatedUser)
					Login ();

			base.OnNextButtonClick (e);
		}

		protected internal override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
		}

		protected virtual void OnSendingMail (MailMessageEventArgs e)
		{
			if (Events != null) {
				MailMessageEventHandler eh = (MailMessageEventHandler) Events [SendingMailEvent];
				if (eh != null) eh (this, e);
			}
		}

		protected virtual void OnSendMailError (SendMailErrorEventArgs e)
		{
			if (Events != null) {
				SendMailErrorEventHandler eh = (SendMailErrorEventHandler) Events [SendMailErrorEvent];
				if (eh != null) eh (this, e);
			}
		}

		protected override void LoadViewState (object savedState)
		{
			if (savedState == null) {
				base.LoadViewState (null);
				return;
			}

			object [] states = (object []) savedState;
			base.LoadViewState (states [0]);

			if (states [1] != null) ((IStateManager) TextBoxStyle).LoadViewState (states [1]);
			if (states [2] != null) ((IStateManager) ValidatorTextStyle).LoadViewState (states [2]);
			if (states [3] != null) ((IStateManager) CompleteSuccessTextStyle).LoadViewState (states [3]);
			if (states [4] != null) ((IStateManager) ErrorMessageStyle).LoadViewState (states [4]);
			if (states [5] != null) ((IStateManager) HyperLinkStyle).LoadViewState (states [5]);
			if (states [6] != null) ((IStateManager) InstructionTextStyle).LoadViewState (states [6]);
			if (states [7] != null) ((IStateManager) LabelStyle).LoadViewState (states [7]);
			if (states [8] != null) ((IStateManager) PasswordHintStyle).LoadViewState (states [8]);
			if (states [9] != null) ((IStateManager) TitleTextStyle).LoadViewState (states [9]);
		}

		protected override object SaveViewState ()
		{
			object [] state = new object [10];
			state [0] = base.SaveViewState ();

			if (TextBoxStyle != null) state [1] = ((IStateManager) TextBoxStyle).SaveViewState ();
			if (ValidatorTextStyle != null) state [2] = ((IStateManager) ValidatorTextStyle).SaveViewState ();
			if (CompleteSuccessTextStyle != null) state [3] = ((IStateManager) CompleteSuccessTextStyle).SaveViewState ();
			if (ErrorMessageStyle != null) state [4] = ((IStateManager) ErrorMessageStyle).SaveViewState ();
			if (HyperLinkStyle != null) state [5] = ((IStateManager) HyperLinkStyle).SaveViewState ();
			if (InstructionTextStyle != null) state [6] = ((IStateManager) InstructionTextStyle).SaveViewState ();
			if (LabelStyle != null) state [7] = ((IStateManager) LabelStyle).SaveViewState ();
			if (PasswordHintStyle != null) state [8] = ((IStateManager) PasswordHintStyle).SaveViewState ();
			if (TitleTextStyle != null) state [9] = ((IStateManager) TitleTextStyle).SaveViewState ();

			for (int n = 0; n < state.Length; n++)
				if (state [n] != null) return state;

			return null;
		}

		[MonoTODO ("for design-time usage - no more details available")]
		protected override void SetDesignModeState (IDictionary data)
		{
			base.SetDesignModeState (data);
		}

		protected override void TrackViewState ()
		{
			base.TrackViewState ();
			if (_textBoxStyle != null) ((IStateManager) _textBoxStyle).TrackViewState ();
			if (_validatorTextStyle != null) ((IStateManager) _validatorTextStyle).TrackViewState ();
			if (_completeSuccessTextStyle != null) ((IStateManager) _completeSuccessTextStyle).TrackViewState ();
			if (_errorMessageStyle != null) ((IStateManager) _errorMessageStyle).TrackViewState ();
			if (_hyperLinkStyle != null) ((IStateManager) _hyperLinkStyle).TrackViewState ();
			if (_instructionTextStyle != null) ((IStateManager) _instructionTextStyle).TrackViewState ();
			if (_labelStyle != null) ((IStateManager) _labelStyle).TrackViewState ();
			if (_passwordHintStyle != null) ((IStateManager) _passwordHintStyle).TrackViewState ();
			if (_titleTextStyle != null) ((IStateManager) _titleTextStyle).TrackViewState ();
		}

		#endregion

		#region Private event handlers

		private void UserName_TextChanged (object sender, EventArgs e)
		{
			UserName = ((ITextControl) sender).Text;
		}

		private void Password_TextChanged (object sender, EventArgs e)
		{
			_password = ((ITextControl) sender).Text;
		}

		private void ConfirmPassword_TextChanged (object sender, EventArgs e)
		{
			_confirmPassword = ((ITextControl) sender).Text;
		}

		private void Email_TextChanged (object sender, EventArgs e)
		{
			Email = ((ITextControl) sender).Text;
		}

		private void Question_TextChanged (object sender, EventArgs e)
		{
			Question = ((ITextControl) sender).Text;
		}

		private void Answer_TextChanged (object sender, EventArgs e)
		{
			Answer = ((ITextControl) sender).Text;
		}

		#endregion

		#region Private Methods

		private void InitMemberShipProvider ()
		{
			string mp = MembershipProvider;
			_provider = (mp.Length == 0) ? _provider = Membership.Provider : Membership.Providers [mp];
			if (_provider == null)
				throw new HttpException (Locale.GetText ("No provider named '{0}' could be found.", mp));
		}

		private bool CreateUser ()
		{
			if (!Page.IsValid)
				return false;

			if (AutoGeneratePassword)
				_password = GeneratePassword ();

			OnCreatingUser (new LoginCancelEventArgs (false));

			MembershipCreateStatus status;
			MembershipUser newUser = MembershipProviderInternal.CreateUser (
				UserName, Password, Email, Question, Answer, !DisableCreatedUser, null, out status);

			if ((newUser != null) && (status == MembershipCreateStatus.Success)) {
				OnCreatedUser (new EventArgs ());
				return true;
			}

			switch (status) {
				case MembershipCreateStatus.DuplicateUserName:
					ShowErrorMessage (DuplicateUserNameErrorMessage);
					break;

				case MembershipCreateStatus.InvalidPassword:
					ShowErrorMessage (InvalidPasswordErrorMessage);
					break;

				case MembershipCreateStatus.DuplicateEmail:
					ShowErrorMessage (DuplicateEmailErrorMessage);
					break;

				case MembershipCreateStatus.InvalidEmail:
					ShowErrorMessage (InvalidEmailErrorMessage);
					break;

				case MembershipCreateStatus.InvalidQuestion:
					ShowErrorMessage (InvalidQuestionErrorMessage);
					break;

				case MembershipCreateStatus.InvalidAnswer:
					ShowErrorMessage (InvalidAnswerErrorMessage);
					break;

				case MembershipCreateStatus.UserRejected:
				case MembershipCreateStatus.InvalidUserName:
				case MembershipCreateStatus.ProviderError:
				case MembershipCreateStatus.InvalidProviderUserKey:
					ShowErrorMessage (UnknownErrorMessage);
					break;


			}

			OnCreateUserError (new CreateUserErrorEventArgs (status));

			return false;
		}

		private void Login ()
		{
			bool userValidated = MembershipProviderInternal.ValidateUser (UserName, Password);
			if (userValidated)
				FormsAuthentication.SetAuthCookie (UserName, false);
		}

		private void ShowErrorMessage (string errorMessage)
		{
			if (_errorMessageLabel != null)
				_errorMessageLabel.Text = errorMessage;
		}

		[MonoTODO]
		private string GeneratePassword ()
		{
			return "password";
		}

		#endregion

		#region SideBarLabelTemplate

		class SideBarLabelTemplate : ITemplate
		{
			Wizard wizard;

			public SideBarLabelTemplate (Wizard wizard)
			{
				this.wizard = wizard;
			}

			public void InstantiateIn (Control control)
			{
				Label b = new Label ();
				wizard.RegisterApplyStyle (b, wizard.SideBarButtonStyle);
				control.Controls.Add (b);
				control.DataBinding += Bound;
			}

			void Bound (object s, EventArgs args)
			{
				WizardStepBase step = DataBinder.GetDataItem (s) as WizardStepBase;
				if (step != null) {
					Control c = (Control) s;
					Label b = (Label) c.Controls [0];
					b.ID = SideBarButtonID;
					b.Text = step.Title;
					if (step == wizard.ActiveStep)
						b.Font.Bold = true;
				}
			}
		}

		#endregion
	}
}

#endif