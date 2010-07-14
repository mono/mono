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

using System;
using System.Web;
using System.Web.UI;
using System.Web.Security;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using System.Net.Mail;

namespace System.Web.UI.WebControls
{
	[DefaultEvent ("CreatedUser")]
	[Designer ("System.Web.UI.Design.WebControls.CreateUserWizardDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[ToolboxData ("   ")]
	[Bindable (false)]
	public class CreateUserWizard : Wizard
	{
		public static readonly string ContinueButtonCommandName = "Continue";
		string _password = String.Empty;
		string _confirmPassword = String.Empty;
		MembershipProvider _provider = null;
		ITextControl _errorMessageLabel = null;
		MailDefinition _mailDefinition = null;

		Style _textBoxStyle = null;
		Style _validatorTextStyle = null;

		TableItemStyle _completeSuccessTextStyle = null;
		TableItemStyle _errorMessageStyle = null;
		TableItemStyle _hyperLinkStyle = null;
		TableItemStyle _instructionTextStyle = null;
		TableItemStyle _labelStyle = null;
		TableItemStyle _passwordHintStyle = null;
		TableItemStyle _titleTextStyle = null;
		Style _createUserButtonStyle;
		Style _continueButtonStyle;

		static readonly object CreatedUserEvent = new object ();
		static readonly object CreateUserErrorEvent = new object ();
		static readonly object CreatingUserEvent = new object ();
		static readonly object ContinueButtonClickEvent = new object ();
		static readonly object SendingMailEvent = new object ();
		static readonly object SendMailErrorEvent = new object ();

		CompleteWizardStep _completeWizardStep = null;
		CreateUserWizardStep _createUserWizardStep = null;

		public CreateUserWizard ()
		{
		}

		#region Public Properties

		[DefaultValue (0)]
		public override int ActiveStepIndex {
			get { return base.ActiveStepIndex; }
			set { base.ActiveStepIndex = value; }
		}

		[DefaultValue ("")]
		[LocalizableAttribute (true)]
		[ThemeableAttribute (false)]
		public virtual string Answer {
			get {
				object o = ViewState ["Answer"];
				return (o == null) ? String.Empty : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("Answer");
				else
					ViewState ["Answer"] = value;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string AnswerLabelText {
			get {
				object o = ViewState ["AnswerLabelText"];
				return (o == null) ? Locale.GetText ("Security Answer:") : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("AnswerLabelText");
				else
					ViewState ["AnswerLabelText"] = value;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string AnswerRequiredErrorMessage {
			get {
				object o = ViewState ["AnswerRequiredErrorMessage"];
				return (o == null) ? Locale.GetText ("Security answer is required.") : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("AnswerRequiredErrorMessage");
				else
					ViewState ["AnswerRequiredErrorMessage"] = value;
			}
		}

		[DefaultValue (false)]
		[ThemeableAttribute (false)]
		public virtual bool AutoGeneratePassword {
			get {
				object o = ViewState ["AutoGeneratePassword"];
				return (o == null) ? false : (bool) o;
			}
			set { ViewState ["AutoGeneratePassword"] = value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public CompleteWizardStep CompleteStep {
			get {
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
		public virtual string CompleteSuccessText {
			get {
				object o = ViewState ["CompleteSuccessText"];
				return (o == null) ? Locale.GetText ("Your account has been successfully created.") : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("CompleteSuccessText");
				else
					ViewState ["CompleteSuccessText"] = value;
			}
		}

		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public TableItemStyle CompleteSuccessTextStyle {
			get {
				if (_completeSuccessTextStyle == null) {
					_completeSuccessTextStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						((IStateManager) _completeSuccessTextStyle).TrackViewState ();
				}
				return _completeSuccessTextStyle;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual string ConfirmPassword {
			get { return _confirmPassword; }
		}

		[LocalizableAttribute (true)]
		public virtual string ConfirmPasswordCompareErrorMessage {
			get {
				object o = ViewState ["ConfirmPasswordCompareErrorMessage"];
				return (o == null) ? Locale.GetText ("The Password and Confirmation Password must match.") : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("ConfirmPasswordCompareErrorMessage");
				else
					ViewState ["ConfirmPasswordCompareErrorMessage"] = value;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string ConfirmPasswordLabelText {
			get {
				object o = ViewState ["ConfirmPasswordLabelText"];
				return (o == null) ? Locale.GetText ("Confirm Password:") : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("ConfirmPasswordLabelText");
				else
					ViewState ["ConfirmPasswordLabelText"] = value;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string ConfirmPasswordRequiredErrorMessage {
			get {
				object o = ViewState ["ConfirmPasswordRequiredErrorMessage"];
				return (o == null) ? Locale.GetText ("Confirm Password is required.") : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("ConfirmPasswordRequiredErrorMessage");
				else
					ViewState ["ConfirmPasswordRequiredErrorMessage"] = value;
			}
		}

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public virtual string ContinueButtonImageUrl {
			get { return ViewState.GetString ("ContinueButtonImageUrl", String.Empty); }
			set { ViewState ["ContinueButtonImageUrl"] = value; }
		}

		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public Style ContinueButtonStyle {
			get {
				if (_continueButtonStyle == null) {
					_continueButtonStyle = new Style ();
					if (IsTrackingViewState)
						((IStateManager) _continueButtonStyle).TrackViewState ();
				}
				return _continueButtonStyle;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string ContinueButtonText {
			get { return ViewState.GetString ("ContinueButtonText", "Continue"); }
			set { ViewState ["ContinueButtonText"] = value; }
		}

		[DefaultValue (ButtonType.Button)]
		public virtual ButtonType ContinueButtonType {
			get {
				object v = ViewState ["ContinueButtonType"];
				return v != null ? (ButtonType) v : ButtonType.Button;
			}
			set {
				ViewState ["ContinueButtonType"] = value;
			}
		}

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[ThemeableAttribute (false)]
		public virtual string ContinueDestinationPageUrl {
			get {
				object o = ViewState ["ContinueDestinationPageUrl"];
				return (o == null) ? "" : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("ContinueDestinationPageUrl");
				else
					ViewState ["ContinueDestinationPageUrl"] = value;
			}
		}

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public virtual string CreateUserButtonImageUrl {
			get { return ViewState.GetString ("CreateUserButtonImageUrl", String.Empty); }
			set { ViewState ["CreateUserButtonImageUrl"] = value; }
		}

		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public Style CreateUserButtonStyle {
			get {
				if (_createUserButtonStyle == null) {
					_createUserButtonStyle = new Style ();
					if (IsTrackingViewState)
						((IStateManager) _createUserButtonStyle).TrackViewState ();
				}
				return _createUserButtonStyle;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string CreateUserButtonText {
			get { return ViewState.GetString ("CreateUserButtonText", "Create User"); }
			set { ViewState ["CreateUserButtonText"] = value; }
		}

		[DefaultValue (ButtonType.Button)]
		public virtual ButtonType CreateUserButtonType {
			get {
				object v = ViewState ["CreateUserButtonType"];
				return v != null ? (ButtonType) v : ButtonType.Button;
			}
			set { ViewState ["CreateUserButtonType"] = value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public CreateUserWizardStep CreateUserStep {
			get {
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

		[DefaultValue (false)]
		[ThemeableAttribute (false)]
		public virtual bool DisableCreatedUser {
			get {
				object o = ViewState ["DisableCreatedUser"];
				return (o == null) ? false : (bool) o;
			}
			set { ViewState ["DisableCreatedUser"] = value; }
		}

		[DefaultValue (false)]
		public override bool DisplaySideBar {
			get { return ViewState.GetBool ("DisplaySideBar", false); }
			set {
				ViewState ["DisplaySideBar"] = value;
				ChildControlsCreated = false;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string DuplicateEmailErrorMessage {
			get {
				object o = ViewState ["DuplicateEmailErrorMessage"];
				return (o == null) ? Locale.GetText ("The e-mail address that you entered is already in use. Please enter a different e-mail address.") : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("DuplicateEmailErrorMessage");
				else
					ViewState ["DuplicateEmailErrorMessage"] = value;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string DuplicateUserNameErrorMessage {
			get {
				object o = ViewState ["DuplicateUserNameErrorMessage"];
				return (o == null) ? Locale.GetText ("Please enter a different user name.") : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("DuplicateUserNameErrorMessage");
				else
					ViewState ["DuplicateUserNameErrorMessage"] = value;
			}
		}

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public virtual string EditProfileIconUrl {
			get {
				object o = ViewState ["EditProfileIconUrl"];
				return (o == null) ? String.Empty : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("EditProfileIconUrl");
				else
					ViewState ["EditProfileIconUrl"] = value;
			}
		}

		[DefaultValue ("")]
		[LocalizableAttribute (true)]
		public virtual string EditProfileText {
			get {
				object o = ViewState ["EditProfileText"];
				return (o == null) ? String.Empty : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("EditProfileText");
				else
					ViewState ["EditProfileText"] = value;
			}
		}

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public virtual string EditProfileUrl {
			get {
				object o = ViewState ["EditProfileUrl"];
				return (o == null) ? String.Empty : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("EditProfileUrl");
				else
					ViewState ["EditProfileUrl"] = value;
			}
		}

		[DefaultValue ("")]
		public virtual string Email {
			get {
				object o = ViewState ["Email"];
				return (o == null) ? String.Empty : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("Email");
				else
					ViewState ["Email"] = value;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string EmailLabelText {
			get {
				object o = ViewState ["EmailLabelText"];
				return (o == null) ? Locale.GetText ("E-mail:") : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("EmailLabelText");
				else
					ViewState ["EmailLabelText"] = value;
			}
		}

		public virtual string EmailRegularExpression {
			get {
				object o = ViewState ["EmailRegularExpression"];
				return (o == null) ? String.Empty : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("EmailRegularExpression");
				else
					ViewState ["EmailRegularExpression"] = value;
			}
		}

		public virtual string EmailRegularExpressionErrorMessage {
			get {
				object o = ViewState ["EmailRegularExpressionErrorMessage"];
				return (o == null) ? Locale.GetText ("Please enter a different e-mail.") : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("EmailRegularExpressionErrorMessage");
				else
					ViewState ["EmailRegularExpressionErrorMessage"] = value;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string EmailRequiredErrorMessage {
			get {
				object o = ViewState ["EmailRequiredErrorMessage"];
				return (o == null) ? Locale.GetText ("E-mail is required.") : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("EmailRequiredErrorMessage");
				else
					ViewState ["EmailRequiredErrorMessage"] = value;
			}
		}

		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public TableItemStyle ErrorMessageStyle {
			get {
				if (_errorMessageStyle == null) {
					_errorMessageStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						((IStateManager) _errorMessageStyle).TrackViewState ();
				}
				return _errorMessageStyle;
			}
		}

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public virtual string HelpPageIconUrl {
			get {
				object o = ViewState ["HelpPageIconUrl"];
				return (o == null) ? String.Empty : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("HelpPageIconUrl");
				else
					ViewState ["HelpPageIconUrl"] = value;
			}
		}

		[DefaultValue ("")]
		[LocalizableAttribute (true)]
		public virtual string HelpPageText {
			get {
				object o = ViewState ["HelpPageText"];
				return (o == null) ? String.Empty : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("HelpPageText");
				else
					ViewState ["HelpPageText"] = value;
			}
		}

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public virtual string HelpPageUrl {
			get {
				object o = ViewState ["HelpPageUrl"];
				return (o == null) ? String.Empty : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("HelpPageUrl");
				else
					ViewState ["HelpPageUrl"] = value;
			}
		}

		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public TableItemStyle HyperLinkStyle {
			get {
				if (_hyperLinkStyle == null) {
					_hyperLinkStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						((IStateManager) _hyperLinkStyle).TrackViewState ();
				}
				return _hyperLinkStyle;
			}
		}

		[DefaultValue ("")]
		[LocalizableAttribute (true)]
		public virtual string InstructionText {
			get {
				object o = ViewState ["InstructionText"];
				return (o == null) ? String.Empty : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("InstructionText");
				else
					ViewState ["InstructionText"] = value;
			}
		}

		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public TableItemStyle InstructionTextStyle {
			get {
				if (_instructionTextStyle == null) {
					_instructionTextStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						((IStateManager) _instructionTextStyle).TrackViewState ();
				}
				return _instructionTextStyle;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string InvalidAnswerErrorMessage {
			get {
				object o = ViewState ["InvalidAnswerErrorMessage"];
				return (o == null) ? Locale.GetText ("Please enter a different security answer.") : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("InvalidAnswerErrorMessage");
				else
					ViewState ["InvalidAnswerErrorMessage"] = value;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string InvalidEmailErrorMessage {
			get {
				object o = ViewState ["InvalidEmailErrorMessage"];
				return (o == null) ? Locale.GetText ("Please enter a valid e-mail address.") : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("InvalidEmailErrorMessage");
				else
					ViewState ["InvalidEmailErrorMessage"] = value;
			}
		}

		[MonoTODO ("take the values from membership provider")]
		[LocalizableAttribute (true)]
		public virtual string InvalidPasswordErrorMessage {
			get {
				object o = ViewState ["InvalidPasswordErrorMessage"];
				return (o == null) ? Locale.GetText ("Password length minimum: {0}. Non-alphanumeric characters required: {1}.") : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("InvalidPasswordErrorMessage");
				else
					ViewState ["InvalidPasswordErrorMessage"] = value;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string InvalidQuestionErrorMessage {
			get {
				object o = ViewState ["InvalidQuestionErrorMessage"];
				return (o == null) ? Locale.GetText ("Please enter a different security question.") : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("InvalidQuestionErrorMessage");
				else
					ViewState ["InvalidQuestionErrorMessage"] = value;
			}
		}

		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public TableItemStyle LabelStyle {
			get {
				if (_labelStyle == null) {
					_labelStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						((IStateManager) _labelStyle).TrackViewState ();
				}
				return _labelStyle;
			}
		}

		[DefaultValue (true)]
		[ThemeableAttribute (false)]
		public virtual bool LoginCreatedUser {
			get {
				object o = ViewState ["LoginCreatedUser"];
				return (o == null) ? true : (bool) o;
			}
			set {
				ViewState ["LoginCreatedUser"] = value;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[ThemeableAttribute (false)]
		public MailDefinition MailDefinition {
			get {
				if (this._mailDefinition == null) {
					this._mailDefinition = new MailDefinition();
					if (IsTrackingViewState)
						((IStateManager) _mailDefinition).TrackViewState ();
				}
				return _mailDefinition;
			}
		}

		[DefaultValue ("")]
		[ThemeableAttribute (false)]
		public virtual string MembershipProvider {
			get {
				object o = ViewState ["MembershipProvider"];
				return (o == null) ? String.Empty : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("MembershipProvider");
				else
					ViewState ["MembershipProvider"] = value;

				_provider = null;
			}
		}

		internal virtual MembershipProvider MembershipProviderInternal {
			get {
				if (_provider == null)
					InitMemberShipProvider ();

				return _provider;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public virtual string Password {
			get { return _password; }
		}

		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public TableItemStyle PasswordHintStyle {
			get {
				if (_passwordHintStyle == null) {
					_passwordHintStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						((IStateManager) _passwordHintStyle).TrackViewState ();
				}
				return _passwordHintStyle;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string PasswordHintText {
			get {
				object o = ViewState ["PasswordHintText"];
				return (o == null) ? String.Empty : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("PasswordHintText");
				else
					ViewState ["PasswordHintText"] = value;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string PasswordLabelText {
			get {
				object o = ViewState ["PasswordLabelText"];
				return (o == null) ? Locale.GetText ("Password:") : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("PasswordLabelText");
				else
					ViewState ["PasswordLabelText"] = value;
			}
		}

		public virtual string PasswordRegularExpression {
			get {
				object o = ViewState ["PasswordRegularExpression"];
				return (o == null) ? String.Empty : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("PasswordRegularExpression");
				else
					ViewState ["PasswordRegularExpression"] = value;
			}
		}

		public virtual string PasswordRegularExpressionErrorMessage {
			get {
				object o = ViewState ["PasswordRegularExpressionErrorMessage"];
				return (o == null) ? Locale.GetText ("Please enter a different password.") : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("PasswordRegularExpressionErrorMessage");
				else
					ViewState ["PasswordRegularExpressionErrorMessage"] = value;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string PasswordRequiredErrorMessage {
			get {
				object o = ViewState ["PasswordRequiredErrorMessage"];
				return (o == null) ? Locale.GetText ("Password is required.") : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("PasswordRequiredErrorMessage");
				else
					ViewState ["PasswordRequiredErrorMessage"] = value;
			}
		}

		[DefaultValue ("")]
		[LocalizableAttribute (true)]
		[ThemeableAttribute (false)]
		public virtual string Question {
			get {
				object o = ViewState ["Question"];
				return (o == null) ? String.Empty : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("Question");
				else
					ViewState ["Question"] = value;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string QuestionLabelText {
			get {
				object o = ViewState ["QuestionLabelText"];
				return (o == null) ? Locale.GetText ("Security Question:") : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("QuestionLabelText");
				else
					ViewState ["QuestionLabelText"] = value;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string QuestionRequiredErrorMessage {
			get {
				object o = ViewState ["QuestionRequiredErrorMessage"];
				return (o == null) ? Locale.GetText ("Security question is required.") : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("QuestionRequiredErrorMessage");
				else
					ViewState ["QuestionRequiredErrorMessage"] = value;
			}
		}

		[DefaultValue (true)]
		[ThemeableAttribute (false)]
		public virtual bool RequireEmail {
			get {
				object o = ViewState ["RequireEmail"];
				return (o == null) ? true : (bool) o;
			}
			set {
				ViewState ["RequireEmail"] = value;
			}
		}

		[DefaultValue ("")]
		[MonoTODO ("doesnt work")]
		public override string SkipLinkText {
			get {
				object o = ViewState ["SkipLinkText"];
				return (o == null) ? String.Empty : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("SkipLinkText");
				else
					ViewState ["SkipLinkText"] = value;
			}
		}

		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public Style TextBoxStyle {
			get {
				if (_textBoxStyle == null) {
					_textBoxStyle = new Style ();
					if (IsTrackingViewState)
						((IStateManager) _textBoxStyle).TrackViewState ();
				}
				return _textBoxStyle;
			}
		}

		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public TableItemStyle TitleTextStyle {
			get {
				if (_titleTextStyle == null) {
					_titleTextStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						((IStateManager) _titleTextStyle).TrackViewState ();
				}
				return _titleTextStyle;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string UnknownErrorMessage {
			get {
				object o = ViewState ["UnknownErrorMessage"];
				return (o == null) ? Locale.GetText ("Your account was not created. Please try again.") : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("UnknownErrorMessage");
				else
					ViewState ["UnknownErrorMessage"] = value;
			}
		}

		[DefaultValue ("")]
		public virtual string UserName {
			get {
				object o = ViewState ["UserName"];
				return (o == null) ? String.Empty : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("UserName");
				else
					ViewState ["UserName"] = value;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string UserNameLabelText {
			get {
				object o = ViewState ["UserNameLabelText"];
				return (o == null) ? Locale.GetText ("User Name:") : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("UserNameLabelText");
				else
					ViewState ["UserNameLabelText"] = value;
			}
		}

		[LocalizableAttribute (true)]
		public virtual string UserNameRequiredErrorMessage {
			get {
				object o = ViewState ["UserNameRequiredErrorMessage"];
				return (o == null) ? Locale.GetText ("User Name is required.") : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("UserNameRequiredErrorMessage");
				else
					ViewState ["UserNameRequiredErrorMessage"] = value;
			}
		}

		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public Style ValidatorTextStyle {
			get {
				if (_validatorTextStyle == null) {
					_validatorTextStyle = new Style ();
					if (IsTrackingViewState)
						((IStateManager) _validatorTextStyle).TrackViewState ();
				}
				return _validatorTextStyle;
			}
		}
		
		[Editor ("System.Web.UI.Design.WebControls.CreateUserWizardStepCollectionEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public override WizardStepCollection WizardSteps {
			get { return base.WizardSteps; }
		}

		#endregion

		#region Protected Properties

		[DefaultValue (true)]
		protected internal bool QuestionAndAnswerRequired {
			get { return MembershipProviderInternal.RequiresQuestionAndAnswer; }
		}

		public event EventHandler ContinueButtonClick {
			add { Events.AddHandler (ContinueButtonClickEvent, value); }
			remove { Events.RemoveHandler (ContinueButtonClickEvent, value); }
		}

		public event EventHandler CreatedUser {
			add { Events.AddHandler (CreatedUserEvent, value); }
			remove { Events.RemoveHandler (CreatedUserEvent, value); }
		}

		public event CreateUserErrorEventHandler CreateUserError {
			add { Events.AddHandler (CreateUserErrorEvent, value); }
			remove { Events.RemoveHandler (CreateUserErrorEvent, value); }
		}

		public event LoginCancelEventHandler CreatingUser {
			add { Events.AddHandler (CreatingUserEvent, value); }
			remove { Events.RemoveHandler (CreatingUserEvent, value); }
		}

		public event MailMessageEventHandler SendingMail {
			add { Events.AddHandler (SendingMailEvent, value); }
			remove { Events.RemoveHandler (SendingMailEvent, value); }
		}

		public event SendMailErrorEventHandler SendMailError {
			add { Events.AddHandler (SendMailErrorEvent, value); }
			remove { Events.RemoveHandler (SendMailErrorEvent, value); }
		}


		#endregion

		#region Internal Properties

		internal override void InstantiateTemplateStep (TemplatedWizardStep step)
		{
			if (step is CreateUserWizardStep)
				InstantiateCreateUserWizardStep ((CreateUserWizardStep) step);
			else if (step is CompleteWizardStep)
				InstantiateCompleteWizardStep ((CompleteWizardStep) step);
			else
				base.InstantiateTemplateStep (step);
		}

		void InstantiateCompleteWizardStep (CompleteWizardStep step)
		{
			CompleteStepContainer contentTemplateContainer = new CompleteStepContainer (this);
			if (step.ContentTemplate != null)
				step.ContentTemplate.InstantiateIn (contentTemplateContainer.InnerCell);
			else {
				new CompleteStepTemplate (this).InstantiateIn (contentTemplateContainer.InnerCell);
				contentTemplateContainer.ConfirmDefaultTemplate ();
			}

			step.ContentTemplateContainer = contentTemplateContainer;
			step.Controls.Clear ();
			step.Controls.Add (contentTemplateContainer);

			BaseWizardNavigationContainer customNavigationTemplateContainer = new BaseWizardNavigationContainer ();
			if (step.CustomNavigationTemplate != null) {
				step.CustomNavigationTemplate.InstantiateIn (customNavigationTemplateContainer);
				RegisterCustomNavigation (step, customNavigationTemplateContainer);
			}
			step.CustomNavigationTemplateContainer = customNavigationTemplateContainer;
		}

		void InstantiateCreateUserWizardStep (CreateUserWizardStep step)
		{
			CreateUserStepContainer contentTemplateContainer = new CreateUserStepContainer (this);
			if (step.ContentTemplate != null)
				step.ContentTemplate.InstantiateIn (contentTemplateContainer.InnerCell);
			else {
				new CreateUserStepTemplate (this).InstantiateIn (contentTemplateContainer.InnerCell);
				contentTemplateContainer.ConfirmDefaultTemplate ();
				contentTemplateContainer.EnsureValidatorsState ();
			}

			step.ContentTemplateContainer = contentTemplateContainer;
			step.Controls.Clear ();
			step.Controls.Add (contentTemplateContainer);

			CreateUserNavigationContainer customNavigationTemplateContainer = new CreateUserNavigationContainer (this);
			if (step.CustomNavigationTemplate != null)
				step.CustomNavigationTemplate.InstantiateIn (customNavigationTemplateContainer);
			else {
				new CreateUserStepNavigationTemplate (this).InstantiateIn (customNavigationTemplateContainer);
				customNavigationTemplateContainer.ConfirmDefaultTemplate ();
			}
			RegisterCustomNavigation (step, customNavigationTemplateContainer);

			step.CustomNavigationTemplateContainer = customNavigationTemplateContainer;
		}
		
		internal override ITemplate SideBarItemTemplate {
			get { return new SideBarLabelTemplate (this); }
		}

		#endregion

		#region Protected Methods

		protected internal override void CreateChildControls ()
		{
			if (CreateUserStep == null)
				WizardSteps.AddAt (0, new CreateUserWizardStep ());

			if (CompleteStep == null)
				WizardSteps.AddAt (WizardSteps.Count, new CompleteWizardStep ());

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

		protected override bool OnBubbleEvent (object source, EventArgs e)
		{
			CommandEventArgs args = e as CommandEventArgs;
			if (e != null && args.CommandName == ContinueButtonCommandName) {
				ProcessContinueEvent ();
				return true;
			}
			return base.OnBubbleEvent (source, e);
		}

		void ProcessContinueEvent ()
		{
			OnContinueButtonClick (EventArgs.Empty);

			if (ContinueDestinationPageUrl.Length > 0)
				Context.Response.Redirect (ContinueDestinationPageUrl);
		}

		protected virtual void OnContinueButtonClick (EventArgs e)
		{
			if (Events != null) {
				EventHandler eh = (EventHandler) Events [ContinueButtonClickEvent];
				if (eh != null)
					eh (this, e);
			}
		}

		protected virtual void OnCreatedUser (EventArgs e)
		{
			if (Events != null) {
				EventHandler eh = (EventHandler) Events [CreatedUserEvent];
				if (eh != null)
					eh (this, e);
			}
		}

		protected virtual void OnCreateUserError (CreateUserErrorEventArgs e)
		{
			if (Events != null) {
				CreateUserErrorEventHandler eh = (CreateUserErrorEventHandler) Events [CreateUserErrorEvent];
				if (eh != null)
					eh (this, e);
			}
		}

		protected virtual void OnCreatingUser (LoginCancelEventArgs e)
		{
			if (Events != null) {
				LoginCancelEventHandler eh = (LoginCancelEventHandler) Events [CreatingUserEvent];
				if (eh != null)
					eh (this, e);
			}
		}

		protected override void OnNextButtonClick (WizardNavigationEventArgs e)
		{
			if (ActiveStep == CreateUserStep) {
				bool userCreated = CreateUser ();
				if (!userCreated)
					e.Cancel = true;
				else
					if (LoginCreatedUser)
						Login ();
			}
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
				if (eh != null)
					eh (this, e);
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

			if (states [1] != null)
				((IStateManager) TextBoxStyle).LoadViewState (states [1]);
			if (states [2] != null)
				((IStateManager) ValidatorTextStyle).LoadViewState (states [2]);
			if (states [3] != null)
				((IStateManager) CompleteSuccessTextStyle).LoadViewState (states [3]);
			if (states [4] != null)
				((IStateManager) ErrorMessageStyle).LoadViewState (states [4]);
			if (states [5] != null)
				((IStateManager) HyperLinkStyle).LoadViewState (states [5]);
			if (states [6] != null)
				((IStateManager) InstructionTextStyle).LoadViewState (states [6]);
			if (states [7] != null)
				((IStateManager) LabelStyle).LoadViewState (states [7]);
			if (states [8] != null)
				((IStateManager) PasswordHintStyle).LoadViewState (states [8]);
			if (states [9] != null)
				((IStateManager) TitleTextStyle).LoadViewState (states [9]);
			if (states [10] != null)
				((IStateManager) CreateUserButtonStyle).LoadViewState (states [10]);
			if (states [11] != null)
				((IStateManager) ContinueButtonStyle).LoadViewState (states [11]);
			if (states [12] != null)
				((IStateManager) MailDefinition).LoadViewState (states [12]);

			((CreateUserStepContainer) CreateUserStep.ContentTemplateContainer).EnsureValidatorsState ();
		}

		protected override object SaveViewState ()
		{
			object [] state = new object [13];
			state [0] = base.SaveViewState ();

			if (_textBoxStyle != null)
				state [1] = ((IStateManager) _textBoxStyle).SaveViewState ();
			if (_validatorTextStyle != null)
				state [2] = ((IStateManager) _validatorTextStyle).SaveViewState ();
			if (_completeSuccessTextStyle != null)
				state [3] = ((IStateManager) _completeSuccessTextStyle).SaveViewState ();
			if (_errorMessageStyle != null)
				state [4] = ((IStateManager) _errorMessageStyle).SaveViewState ();
			if (_hyperLinkStyle != null)
				state [5] = ((IStateManager) _hyperLinkStyle).SaveViewState ();
			if (_instructionTextStyle != null)
				state [6] = ((IStateManager) _instructionTextStyle).SaveViewState ();
			if (_labelStyle != null)
				state [7] = ((IStateManager) _labelStyle).SaveViewState ();
			if (_passwordHintStyle != null)
				state [8] = ((IStateManager) _passwordHintStyle).SaveViewState ();
			if (_titleTextStyle != null)
				state [9] = ((IStateManager) _titleTextStyle).SaveViewState ();
			if (_createUserButtonStyle != null)
				state [10] = ((IStateManager) _createUserButtonStyle).SaveViewState ();
			if (_continueButtonStyle != null)
				state [11] = ((IStateManager) _continueButtonStyle).SaveViewState ();
			if (_mailDefinition != null)
				state [12] = ((IStateManager) _mailDefinition).SaveViewState ();

			for (int n = 0; n < state.Length; n++)
				if (state [n] != null)
					return state;

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
			if (_textBoxStyle != null)
				((IStateManager) _textBoxStyle).TrackViewState ();
			if (_validatorTextStyle != null)
				((IStateManager) _validatorTextStyle).TrackViewState ();
			if (_completeSuccessTextStyle != null)
				((IStateManager) _completeSuccessTextStyle).TrackViewState ();
			if (_errorMessageStyle != null)
				((IStateManager) _errorMessageStyle).TrackViewState ();
			if (_hyperLinkStyle != null)
				((IStateManager) _hyperLinkStyle).TrackViewState ();
			if (_instructionTextStyle != null)
				((IStateManager) _instructionTextStyle).TrackViewState ();
			if (_labelStyle != null)
				((IStateManager) _labelStyle).TrackViewState ();
			if (_passwordHintStyle != null)
				((IStateManager) _passwordHintStyle).TrackViewState ();
			if (_titleTextStyle != null)
				((IStateManager) _titleTextStyle).TrackViewState ();
			if (_createUserButtonStyle != null)
				((IStateManager) _createUserButtonStyle).TrackViewState ();
			if (_continueButtonStyle != null)
				((IStateManager) _continueButtonStyle).TrackViewState ();
			if (_mailDefinition != null)
				((IStateManager) _mailDefinition).TrackViewState ();
		}

		#endregion

		#region Private event handlers

		void UserName_TextChanged (object sender, EventArgs e)
		{
			UserName = ((ITextControl) sender).Text;
		}

		void Password_TextChanged (object sender, EventArgs e)
		{
			_password = ((ITextControl) sender).Text;
		}

		void ConfirmPassword_TextChanged (object sender, EventArgs e)
		{
			_confirmPassword = ((ITextControl) sender).Text;
		}

		void Email_TextChanged (object sender, EventArgs e)
		{
			Email = ((ITextControl) sender).Text;
		}

		void Question_TextChanged (object sender, EventArgs e)
		{
			Question = ((ITextControl) sender).Text;
		}

		void Answer_TextChanged (object sender, EventArgs e)
		{
			Answer = ((ITextControl) sender).Text;
		}

		#endregion

		#region Private Methods

		void InitMemberShipProvider ()
		{
			string mp = MembershipProvider;
			_provider = (mp.Length == 0) ? _provider = Membership.Provider : Membership.Providers [mp];
			if (_provider == null)
				throw new HttpException (Locale.GetText ("No provider named '{0}' could be found.", mp));
		}

		bool CreateUser ()
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
				SendPasswordByMail (newUser, Password);
				return true;
			}

			switch (status) {
				case MembershipCreateStatus.DuplicateUserName:
					ShowErrorMessage (DuplicateUserNameErrorMessage);
					break;

				case MembershipCreateStatus.InvalidPassword:
					ShowErrorMessage (String.Format (InvalidPasswordErrorMessage, MembershipProviderInternal.MinRequiredPasswordLength, MembershipProviderInternal.MinRequiredNonAlphanumericCharacters));
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

		void SendPasswordByMail (MembershipUser user, string password)
		{
			if (user == null)
				return;
			
			if (_mailDefinition == null)
				return;
			
			string messageText = "A new account has been created for you. Please go to the site and log in using the following information.\nUser Name: <%USERNAME%>\nPassword: <%PASSWORD%>";

			ListDictionary dictionary = new ListDictionary ();
			dictionary.Add ("<%USERNAME%>", user.UserName);
			dictionary.Add ("<%PASSWORD%>", password);

			MailMessage message = null;
			
			if (MailDefinition.BodyFileName.Length == 0)
				message = MailDefinition.CreateMailMessage (user.Email, dictionary, messageText, this);
			else
				message = MailDefinition.CreateMailMessage (user.Email, dictionary, this);

			if (string.IsNullOrEmpty (message.Subject))
				message.Subject = "Account information";

			MailMessageEventArgs args = new MailMessageEventArgs (message);
			OnSendingMail (args);

			SmtpClient smtpClient = new SmtpClient ();
			try {
				smtpClient.Send (message);
			} catch (Exception e) {
				SendMailErrorEventArgs mailArgs = new SendMailErrorEventArgs (e);
				OnSendMailError (mailArgs);
				if (!mailArgs.Handled)
					throw e;
			}
		}

		void Login ()
		{
			bool userValidated = MembershipProviderInternal.ValidateUser (UserName, Password);
			if (userValidated)
				FormsAuthentication.SetAuthCookie (UserName, false);
		}

		void ShowErrorMessage (string errorMessage)
		{
			if (_errorMessageLabel != null)
				_errorMessageLabel.Text = errorMessage;
		}

		string GeneratePassword ()
		{
			return Membership.GeneratePassword (8, 3);
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
				}
			}
		}

		#endregion

		sealed class CreateUserNavigationContainer : DefaultNavigationContainer
		{
			CreateUserWizard _createUserWizard;

			public CreateUserNavigationContainer (CreateUserWizard createUserWizard)
				: base (createUserWizard)
			{
				_createUserWizard = createUserWizard;
			}

			protected override void UpdateState ()
			{
				// previous
				if (_createUserWizard.AllowNavigationToStep (_createUserWizard.ActiveStepIndex - 1)) {
					UpdateNavButtonState (Wizard.StepPreviousButtonID + Wizard.StepPreviousButtonType, Wizard.StepPreviousButtonText, Wizard.StepPreviousButtonImageUrl, Wizard.StepPreviousButtonStyle);
				}
				else {
					((Table) Controls [0]).Rows [0].Cells [0].Visible = false;
				}

				// create user
				UpdateNavButtonState (Wizard.StepNextButtonID + _createUserWizard.CreateUserButtonType, _createUserWizard.CreateUserButtonText, _createUserWizard.CreateUserButtonImageUrl, _createUserWizard.CreateUserButtonStyle);

				// cancel
				if (Wizard.DisplayCancelButton) {
					UpdateNavButtonState (Wizard.CancelButtonID + Wizard.CancelButtonType, Wizard.CancelButtonText, Wizard.CancelButtonImageUrl, Wizard.CancelButtonStyle);
				}
				else {
					((Table) Controls [0]).Rows [0].Cells [2].Visible = false;
				}
			}
		}

		sealed class CreateUserStepNavigationTemplate : ITemplate
		{
			readonly CreateUserWizard _createUserWizard;

			public CreateUserStepNavigationTemplate (CreateUserWizard createUserWizard) {
				_createUserWizard = createUserWizard;
			}

			#region ITemplate Members

			public void InstantiateIn (Control container)
			{
				Table t = new Table ();
				t.CellPadding = 5;
				t.CellSpacing = 5;
				t.Width = Unit.Percentage (100);
				t.Height = Unit.Percentage (100);
				TableRow row = new TableRow ();

				AddButtonCell (row, _createUserWizard.CreateButtonSet (Wizard.StepPreviousButtonID, Wizard.MovePreviousCommandName, false, _createUserWizard.ID));
				AddButtonCell (row, _createUserWizard.CreateButtonSet (Wizard.StepNextButtonID, Wizard.MoveNextCommandName, true, _createUserWizard.ID));
				AddButtonCell (row, _createUserWizard.CreateButtonSet (Wizard.CancelButtonID, Wizard.CancelCommandName, false, _createUserWizard.ID));
				
				t.Rows.Add (row);
				container.Controls.Add (t);
			}

			void AddButtonCell (TableRow row, params Control [] controls)
			{
				TableCell cell = new TableCell ();
				cell.HorizontalAlign = HorizontalAlign.Right;
				for (int i = 0; i < controls.Length; i++)
					cell.Controls.Add (controls[i]);
				row.Cells.Add (cell);
			}

			#endregion
		}

		sealed class CreateUserStepContainer : DefaultContentContainer
		{
			CreateUserWizard _createUserWizard;

			public CreateUserStepContainer (CreateUserWizard createUserWizard)
				: base (createUserWizard)
			{
				_createUserWizard = createUserWizard;
			}

			public Control UserNameTextBox {
				get {
					Control c = FindControl ("UserName");
					if (c == null)
						throw new HttpException ("CreateUserWizardStep.ContentTemplate does not contain an IEditableTextControl with ID UserName for the username.");

					return c;
				}
			}
			public Control PasswordTextBox {
				get {
					Control c = FindControl ("Password");
					if (c == null)
						throw new HttpException ("CreateUserWizardStep.ContentTemplate does not contain an IEditableTextControl with ID Password for the new password, this is required if AutoGeneratePassword = true.");

					return c;
				}
			}
			public Control ConfirmPasswordTextBox {
				get {
					Control c = FindControl ("Password");
					return c;
				}
			}
			public Control EmailTextBox {
				get {
					Control c = FindControl ("Email");
					if (c == null)
						throw new HttpException ("CreateUserWizardStep.ContentTemplate does not contain an IEditableTextControl with ID Email for the e-mail, this is required if RequireEmail = true.");

					return c;
				}
			}
			public Control QuestionTextBox {
				get {
					Control c = FindControl ("Question");
					if (c == null)
						throw new HttpException ("CreateUserWizardStep.ContentTemplate does not contain an IEditableTextControl with ID Question for the security question, this is required if your membership provider requires a question and answer.");

					return c;
				}
			}
			public Control AnswerTextBox {
				get {
					Control c = FindControl ("Answer");
					if (c == null)
						throw new HttpException ("CreateUserWizardStep.ContentTemplate does not contain an IEditableTextControl with ID Answer for the security answer, this is required if your membership provider requires a question and answer.");

					return c;
				}
			}
			public ITextControl ErrorMessageLabel {
				get { return FindControl ("ErrorMessage") as ITextControl; }
			}

			protected override void UpdateState ()
			{
				// Row #0 - title
				if (String.IsNullOrEmpty (_createUserWizard.CreateUserStep.Title))
					((Table) InnerCell.Controls [0]).Rows [0].Visible = false;
				else
					((Table) InnerCell.Controls [0]).Rows [0].Cells [0].Text = _createUserWizard.CreateUserStep.Title;

				// Row #1 - InstructionText
				if (String.IsNullOrEmpty (_createUserWizard.InstructionText))
					((Table) InnerCell.Controls [0]).Rows [1].Visible = false;
				else
					((Table) InnerCell.Controls [0]).Rows [1].Cells [0].Text = _createUserWizard.InstructionText;

				// Row #2
				Label UserNameLabel = (Label) ((Table) InnerCell.Controls [0]).Rows [2].Cells [0].Controls [0];
				UserNameLabel.Text = _createUserWizard.UserNameLabelText;

				RequiredFieldValidator UserNameRequired = (RequiredFieldValidator) FindControl ("UserNameRequired");
				UserNameRequired.ErrorMessage = _createUserWizard.UserNameRequiredErrorMessage;
				UserNameRequired.ToolTip = _createUserWizard.UserNameRequiredErrorMessage;

				if (_createUserWizard.AutoGeneratePassword) {
					((Table) InnerCell.Controls [0]).Rows [3].Visible = false;
					((Table) InnerCell.Controls [0]).Rows [4].Visible = false;
					((Table) InnerCell.Controls [0]).Rows [5].Visible = false;
				} else {
					// Row #3
					Label PasswordLabel = (Label) ((Table) InnerCell.Controls [0]).Rows [3].Cells [0].Controls [0];
					PasswordLabel.Text = _createUserWizard.PasswordLabelText;

					RequiredFieldValidator PasswordRequired = (RequiredFieldValidator) FindControl ("PasswordRequired");
					PasswordRequired.ErrorMessage = _createUserWizard.PasswordRequiredErrorMessage;
					PasswordRequired.ToolTip = _createUserWizard.PasswordRequiredErrorMessage;

					// Row #4
					if (String.IsNullOrEmpty (_createUserWizard.PasswordHintText))
						((Table) InnerCell.Controls [0]).Rows [4].Visible = false;
					else
						((Table) InnerCell.Controls [0]).Rows [4].Cells [1].Text = _createUserWizard.PasswordHintText;

					// Row #5
					Label ConfirmPasswordLabel = (Label) ((Table) InnerCell.Controls [0]).Rows [5].Cells [0].Controls [0];
					ConfirmPasswordLabel.Text = _createUserWizard.ConfirmPasswordLabelText;

					RequiredFieldValidator ConfirmPasswordRequired = (RequiredFieldValidator) FindControl ("ConfirmPasswordRequired");
					ConfirmPasswordRequired.ErrorMessage = _createUserWizard.ConfirmPasswordRequiredErrorMessage;
					ConfirmPasswordRequired.ToolTip = _createUserWizard.ConfirmPasswordRequiredErrorMessage;
				}

				// Row #6
				if (_createUserWizard.RequireEmail) {
					Label EmailLabel = (Label) ((Table) InnerCell.Controls [0]).Rows [6].Cells [0].Controls [0];
					EmailLabel.Text = _createUserWizard.EmailLabelText;

					RequiredFieldValidator EmailRequired = (RequiredFieldValidator) FindControl ("EmailRequired");
					EmailRequired.ErrorMessage = _createUserWizard.EmailRequiredErrorMessage;
					EmailRequired.ToolTip = _createUserWizard.EmailRequiredErrorMessage;
				} else
					((Table) InnerCell.Controls [0]).Rows [6].Visible = false;

				if (_createUserWizard.QuestionAndAnswerRequired) {
					// Row #7
					Label QuestionLabel = (Label) ((Table) InnerCell.Controls [0]).Rows [7].Cells [0].Controls [0];
					QuestionLabel.Text = _createUserWizard.QuestionLabelText;

					RequiredFieldValidator QuestionRequired = (RequiredFieldValidator) FindControl ("QuestionRequired");
					QuestionRequired.ErrorMessage = _createUserWizard.QuestionRequiredErrorMessage;
					QuestionRequired.ToolTip = _createUserWizard.QuestionRequiredErrorMessage;

					// Row #8
					Label AnswerLabel = (Label) ((Table) InnerCell.Controls [0]).Rows [8].Cells [0].Controls [0];
					AnswerLabel.Text = _createUserWizard.AnswerLabelText;

					RequiredFieldValidator AnswerRequired = (RequiredFieldValidator) FindControl ("AnswerRequired");
					AnswerRequired.ErrorMessage = _createUserWizard.AnswerRequiredErrorMessage;
					AnswerRequired.ToolTip = _createUserWizard.AnswerRequiredErrorMessage;
				} else {
					((Table) InnerCell.Controls [0]).Rows [7].Visible = false;
					((Table) InnerCell.Controls [0]).Rows [8].Visible = false;
				}

				// Row #9
				if (_createUserWizard.AutoGeneratePassword)
					((Table) InnerCell.Controls [0]).Rows [9].Visible = false;
				else {
					CompareValidator PasswordCompare = (CompareValidator) FindControl ("PasswordCompare");
					PasswordCompare.ErrorMessage = _createUserWizard.ConfirmPasswordCompareErrorMessage;
				}

				// Row #10
				if (_createUserWizard.AutoGeneratePassword || String.IsNullOrEmpty (_createUserWizard.PasswordRegularExpression))
					((Table) InnerCell.Controls [0]).Rows [10].Visible = false;
				else {
					RegularExpressionValidator PasswordRegEx = (RegularExpressionValidator) FindControl ("PasswordRegEx");
					PasswordRegEx.ValidationExpression = _createUserWizard.PasswordRegularExpression;
					PasswordRegEx.ErrorMessage = _createUserWizard.PasswordRegularExpressionErrorMessage;
				}

				// Row #11
				if (!_createUserWizard.RequireEmail || String.IsNullOrEmpty (_createUserWizard.EmailRegularExpression))
					((Table) InnerCell.Controls [0]).Rows [11].Visible = false;
				else {
					RegularExpressionValidator EmailRegEx = (RegularExpressionValidator) FindControl ("EmailRegEx");
					EmailRegEx.ErrorMessage = _createUserWizard.EmailRegularExpressionErrorMessage;
					EmailRegEx.ValidationExpression = _createUserWizard.EmailRegularExpression;
				}

				// Row #12
				if (String.IsNullOrEmpty (ErrorMessageLabel.Text))
					((Table) InnerCell.Controls [0]).Rows [12].Visible = false;

				// Row #13
				// HelpPageIconUrl
				Image img = (Image) ((Table) InnerCell.Controls [0]).Rows [13].Cells [0].Controls [0];
				if (String.IsNullOrEmpty (_createUserWizard.HelpPageIconUrl))
					img.Visible = false;
				else {
					img.ImageUrl = _createUserWizard.HelpPageIconUrl;
					img.AlternateText = _createUserWizard.HelpPageText;
				}

				// HelpPageText
				HyperLink link = (HyperLink) ((Table) InnerCell.Controls [0]).Rows [13].Cells [0].Controls [1];
				if (String.IsNullOrEmpty (_createUserWizard.HelpPageText))
					link.Visible = false;
				else {
					link.Text = _createUserWizard.HelpPageText;
					link.NavigateUrl = _createUserWizard.HelpPageUrl;
				}

				((Table) InnerCell.Controls [0]).Rows [13].Visible = img.Visible || link.Visible;

			}

			public void EnsureValidatorsState ()
			{
				if (!IsDefaultTemplate)
					return;

				((RequiredFieldValidator) FindControl ("PasswordRequired")).Enabled = !_createUserWizard.AutoGeneratePassword;
				((RequiredFieldValidator) FindControl ("ConfirmPasswordRequired")).Enabled = !_createUserWizard.AutoGeneratePassword;
				((CompareValidator) FindControl ("PasswordCompare")).Enabled = !_createUserWizard.AutoGeneratePassword;
				RegularExpressionValidator PasswordRegEx = (RegularExpressionValidator) FindControl ("PasswordRegEx");
				PasswordRegEx.Enabled = !_createUserWizard.AutoGeneratePassword && !String.IsNullOrEmpty (_createUserWizard.PasswordRegularExpression);
				PasswordRegEx.ValidationExpression = _createUserWizard.PasswordRegularExpression;

				((RequiredFieldValidator) FindControl ("EmailRequired")).Enabled = _createUserWizard.RequireEmail;
				RegularExpressionValidator EmailRegEx = (RegularExpressionValidator) FindControl ("EmailRegEx");
				EmailRegEx.Enabled = _createUserWizard.RequireEmail && !String.IsNullOrEmpty (_createUserWizard.EmailRegularExpression);
				EmailRegEx.ValidationExpression = _createUserWizard.EmailRegularExpression;

				((RequiredFieldValidator) FindControl ("QuestionRequired")).Enabled = _createUserWizard.QuestionAndAnswerRequired;
				((RequiredFieldValidator) FindControl ("AnswerRequired")).Enabled = _createUserWizard.QuestionAndAnswerRequired;
			}
		}

		sealed class CreateUserStepTemplate : ITemplate
		{
			readonly CreateUserWizard _createUserWizard;

			public CreateUserStepTemplate (CreateUserWizard createUserWizard)
			{
				_createUserWizard = createUserWizard;
			}

			#region ITemplate Members

			TableRow CreateRow (Control c0, Control c1, Control c2, Style s0, Style s1)
			{
				TableRow row = new TableRow ();
				TableCell cell0 = new TableCell ();
				TableCell cell1 = new TableCell ();

				if (c0 != null)
					cell0.Controls.Add (c0);
				row.Controls.Add (cell0);

				if ((c1 != null) && (c2 != null)) {
					cell1.Controls.Add (c1);
					cell1.Controls.Add (c2);
					cell0.HorizontalAlign = HorizontalAlign.Right;

					if (s0 != null)
						_createUserWizard.RegisterApplyStyle (cell0, s0);
					if (s1 != null)
						_createUserWizard.RegisterApplyStyle (cell1, s1);

					row.Controls.Add (cell1);
				} else {
					cell0.ColumnSpan = 2;
					cell0.HorizontalAlign = HorizontalAlign.Center;
					if (s0 != null)
						_createUserWizard.RegisterApplyStyle (cell0, s0);
				}
				return row;
			}

			public void InstantiateIn (Control container)
			{
				Table table = new Table ();
				table.ControlStyle.Width = Unit.Percentage (100);
				table.ControlStyle.Height = Unit.Percentage (100);

				// Row #0
				table.Controls.Add (CreateRow (null, null, null, _createUserWizard.TitleTextStyle, null));

				// Row #1
				table.Controls.Add (CreateRow (null, null, null, _createUserWizard.InstructionTextStyle, null));

				// Row #2
				TextBox UserName = new TextBox ();
				UserName.ID = "UserName";
				_createUserWizard.RegisterApplyStyle (UserName, _createUserWizard.TextBoxStyle);

				Label UserNameLabel = new Label ();
				UserNameLabel.AssociatedControlID = "UserName";

				RequiredFieldValidator UserNameRequired = new RequiredFieldValidator ();
				UserNameRequired.ID = "UserNameRequired";
				// alternatively we can create only required validators
				// and reinstantiate collection when relevant property changes
				UserNameRequired.EnableViewState = false;
				UserNameRequired.ControlToValidate = "UserName";
				UserNameRequired.Text = "*";
				UserNameRequired.ValidationGroup = _createUserWizard.ID;
				_createUserWizard.RegisterApplyStyle (UserNameRequired, _createUserWizard.ValidatorTextStyle);

				table.Controls.Add (CreateRow (UserNameLabel, UserName, UserNameRequired, _createUserWizard.LabelStyle, null));

				// Row #3
				TextBox Password = new TextBox ();
				Password.ID = "Password";
				Password.TextMode = TextBoxMode.Password;
				_createUserWizard.RegisterApplyStyle (Password, _createUserWizard.TextBoxStyle);

				Label PasswordLabel = new Label ();
				PasswordLabel.AssociatedControlID = "Password";

				RequiredFieldValidator PasswordRequired = new RequiredFieldValidator ();
				PasswordRequired.ID = "PasswordRequired";
				PasswordRequired.EnableViewState = false;
				PasswordRequired.ControlToValidate = "Password";
				PasswordRequired.Text = "*";
				//PasswordRequired.EnableViewState = false;
				PasswordRequired.ValidationGroup = _createUserWizard.ID;
				_createUserWizard.RegisterApplyStyle (PasswordRequired, _createUserWizard.ValidatorTextStyle);

				table.Controls.Add (CreateRow (PasswordLabel, Password, PasswordRequired, _createUserWizard.LabelStyle, null));

				// Row #4
				table.Controls.Add (CreateRow (new LiteralControl (String.Empty), new LiteralControl (String.Empty), new LiteralControl (String.Empty), null, _createUserWizard.PasswordHintStyle));

				// Row #5
				TextBox ConfirmPassword = new TextBox ();
				ConfirmPassword.ID = "ConfirmPassword";
				ConfirmPassword.TextMode = TextBoxMode.Password;
				_createUserWizard.RegisterApplyStyle (ConfirmPassword, _createUserWizard.TextBoxStyle);

				Label ConfirmPasswordLabel = new Label ();
				ConfirmPasswordLabel.AssociatedControlID = "ConfirmPassword";

				RequiredFieldValidator ConfirmPasswordRequired = new RequiredFieldValidator ();
				ConfirmPasswordRequired.ID = "ConfirmPasswordRequired";
				ConfirmPasswordRequired.EnableViewState = false;
				ConfirmPasswordRequired.ControlToValidate = "ConfirmPassword";
				ConfirmPasswordRequired.Text = "*";
				ConfirmPasswordRequired.ValidationGroup = _createUserWizard.ID;
				_createUserWizard.RegisterApplyStyle (ConfirmPasswordRequired, _createUserWizard.ValidatorTextStyle);

				table.Controls.Add (CreateRow (ConfirmPasswordLabel, ConfirmPassword, ConfirmPasswordRequired, _createUserWizard.LabelStyle, null));

				// Row #6
				TextBox Email = new TextBox ();
				Email.ID = "Email";
				_createUserWizard.RegisterApplyStyle (Email, _createUserWizard.TextBoxStyle);

				Label EmailLabel = new Label ();
				EmailLabel.AssociatedControlID = "Email";

				RequiredFieldValidator EmailRequired = new RequiredFieldValidator ();
				EmailRequired.ID = "EmailRequired";
				EmailRequired.EnableViewState = false;
				EmailRequired.ControlToValidate = "Email";
				EmailRequired.Text = "*";
				EmailRequired.ValidationGroup = _createUserWizard.ID;
				_createUserWizard.RegisterApplyStyle (EmailRequired, _createUserWizard.ValidatorTextStyle);

				table.Controls.Add (CreateRow (EmailLabel, Email, EmailRequired, _createUserWizard.LabelStyle, null));

				// Row #7
				TextBox Question = new TextBox ();
				Question.ID = "Question";
				_createUserWizard.RegisterApplyStyle (Question, _createUserWizard.TextBoxStyle);

				Label QuestionLabel = new Label ();
				QuestionLabel.AssociatedControlID = "Question";

				RequiredFieldValidator QuestionRequired = new RequiredFieldValidator ();
				QuestionRequired.ID = "QuestionRequired";
				QuestionRequired.EnableViewState = false;
				QuestionRequired.ControlToValidate = "Question";
				QuestionRequired.Text = "*";
				QuestionRequired.ValidationGroup = _createUserWizard.ID;
				_createUserWizard.RegisterApplyStyle (QuestionRequired, _createUserWizard.ValidatorTextStyle);

				table.Controls.Add (CreateRow (QuestionLabel, Question, QuestionRequired, _createUserWizard.LabelStyle, null));

				// Row #8
				TextBox Answer = new TextBox ();
				Answer.ID = "Answer";
				_createUserWizard.RegisterApplyStyle (Answer, _createUserWizard.TextBoxStyle);

				Label AnswerLabel = new Label ();
				AnswerLabel.AssociatedControlID = "Answer";

				RequiredFieldValidator AnswerRequired = new RequiredFieldValidator ();
				AnswerRequired.ID = "AnswerRequired";
				AnswerRequired.EnableViewState = false;
				AnswerRequired.ControlToValidate = "Answer";
				AnswerRequired.Text = "*";
				AnswerRequired.ValidationGroup = _createUserWizard.ID;
				_createUserWizard.RegisterApplyStyle (AnswerRequired, _createUserWizard.ValidatorTextStyle);

				table.Controls.Add (CreateRow (AnswerLabel, Answer, AnswerRequired, _createUserWizard.LabelStyle, null));

				// Row #9
				CompareValidator PasswordCompare = new CompareValidator ();
				PasswordCompare.ID = "PasswordCompare";
				PasswordCompare.EnableViewState = false;
				PasswordCompare.ControlToCompare = "Password";
				PasswordCompare.ControlToValidate = "ConfirmPassword";
				PasswordCompare.Display = ValidatorDisplay.Static;
				PasswordCompare.ValidationGroup = _createUserWizard.ID;
				PasswordCompare.Display = ValidatorDisplay.Dynamic;
				_createUserWizard.RegisterApplyStyle (PasswordCompare, _createUserWizard.ValidatorTextStyle);

				table.Controls.Add (CreateRow (PasswordCompare, null, null, null, null));

				// Row #10
				RegularExpressionValidator PasswordRegEx = new RegularExpressionValidator ();
				PasswordRegEx.ID = "PasswordRegEx";
				PasswordRegEx.EnableViewState = false;
				PasswordRegEx.ControlToValidate = "Password";
				PasswordRegEx.Display = ValidatorDisplay.Static;
				PasswordRegEx.ValidationGroup = _createUserWizard.ID;
				PasswordRegEx.Display = ValidatorDisplay.Dynamic;
				_createUserWizard.RegisterApplyStyle (PasswordRegEx, _createUserWizard.ValidatorTextStyle);

				table.Controls.Add (CreateRow (PasswordRegEx, null, null, null, null));

				// Row #11
				RegularExpressionValidator EmailRegEx = new RegularExpressionValidator ();
				EmailRegEx.ID = "EmailRegEx";
				EmailRegEx.EnableViewState = false;
				EmailRegEx.ControlToValidate = "Email";
				EmailRegEx.Display = ValidatorDisplay.Static;
				EmailRegEx.ValidationGroup = _createUserWizard.ID;
				EmailRegEx.Display = ValidatorDisplay.Dynamic;
				_createUserWizard.RegisterApplyStyle (EmailRegEx, _createUserWizard.ValidatorTextStyle);

				table.Controls.Add (CreateRow (EmailRegEx, null, null, null, null));

				// Row #12
				Label ErrorMessage = new Label ();
				ErrorMessage.ID = "ErrorMessage";
				ErrorMessage.EnableViewState = false;
				_createUserWizard.RegisterApplyStyle (ErrorMessage, _createUserWizard.ValidatorTextStyle);

				table.Controls.Add (CreateRow (ErrorMessage, null, null, null, null));

				// Row #13
				TableRow row13 = CreateRow (new Image (), null, null, null, null);

				HyperLink HelpLink = new HyperLink ();
				HelpLink.ID = "HelpLink";
				_createUserWizard.RegisterApplyStyle (HelpLink, _createUserWizard.HyperLinkStyle);
				row13.Cells [0].Controls.Add (HelpLink);

				row13.Cells [0].HorizontalAlign = HorizontalAlign.Left;
				table.Controls.Add (row13);

				//
				container.Controls.Add (table);
			}

			#endregion
		}

		sealed class CompleteStepContainer : DefaultContentContainer
		{
			CreateUserWizard _createUserWizard;

			public CompleteStepContainer (CreateUserWizard createUserWizard)
				: base (createUserWizard)
			{
				_createUserWizard = createUserWizard;
			}

			protected override void UpdateState ()
			{
				// title
				if (String.IsNullOrEmpty (_createUserWizard.CompleteStep.Title))
					((Table) InnerCell.Controls [0]).Rows [0].Visible = false;
				else
					((Table) InnerCell.Controls [0]).Rows [0].Cells [0].Text = _createUserWizard.CompleteStep.Title;

				// CompleteSuccessText
				if (String.IsNullOrEmpty (_createUserWizard.CompleteSuccessText))
					((Table) InnerCell.Controls [0]).Rows [1].Visible = false;
				else
					((Table) InnerCell.Controls [0]).Rows [1].Cells [0].Text = _createUserWizard.CompleteSuccessText;

				// ContinueButton
				UpdateNavButtonState ("ContinueButton" + _createUserWizard.ContinueButtonType, _createUserWizard.ContinueButtonText, _createUserWizard.ContinueButtonImageUrl, _createUserWizard.ContinueButtonStyle);

				// EditProfileIconUrl
				Image img = (Image) ((Table) InnerCell.Controls [0]).Rows [3].Cells [0].Controls [0];
				if (String.IsNullOrEmpty (_createUserWizard.EditProfileIconUrl))
					img.Visible = false;
				else {
					img.ImageUrl = _createUserWizard.EditProfileIconUrl;
					img.AlternateText = _createUserWizard.EditProfileText;
				}

				// EditProfileText
				HyperLink link = (HyperLink) ((Table) InnerCell.Controls [0]).Rows [3].Cells [0].Controls [1];
				if (String.IsNullOrEmpty (_createUserWizard.EditProfileText))
					link.Visible = false;
				else {
					link.Text = _createUserWizard.EditProfileText;
					link.NavigateUrl = _createUserWizard.EditProfileUrl;
				}

				((Table) InnerCell.Controls [0]).Rows [3].Visible = img.Visible || link.Visible;
			}
				
			void UpdateNavButtonState (string id, string text, string image, Style style)
			{
				WebControl b = (WebControl) FindControl (id);
				foreach (Control c in b.Parent.Controls)
					c.Visible = b == c;

				((IButtonControl) b).Text = text;
				ImageButton imgbtn = b as ImageButton;
				if (imgbtn != null)
					imgbtn.ImageUrl = image;

				b.ApplyStyle (style);
			}
		}

		sealed class CompleteStepTemplate : ITemplate
		{
			readonly CreateUserWizard _createUserWizard;

			public CompleteStepTemplate (CreateUserWizard createUserWizard)
			{
				_createUserWizard = createUserWizard;
			}

			#region ITemplate Members

			public void InstantiateIn (Control container)
			{
				Table table = new Table ();

				// Row #0
				TableRow row0 = new TableRow ();
				TableCell cell00 = new TableCell ();

				cell00.HorizontalAlign = HorizontalAlign.Center;
				cell00.ColumnSpan = 2;
				_createUserWizard.RegisterApplyStyle (cell00, _createUserWizard.TitleTextStyle);
				row0.Cells.Add (cell00);

				// Row #1
				TableRow row1 = new TableRow ();
				TableCell cell10 = new TableCell ();

				cell10.HorizontalAlign = HorizontalAlign.Center;
				_createUserWizard.RegisterApplyStyle (cell10, _createUserWizard.CompleteSuccessTextStyle);
				row1.Cells.Add (cell10);

				// Row #2
				TableRow row2 = new TableRow ();
				TableCell cell20 = new TableCell ();

				cell20.HorizontalAlign = HorizontalAlign.Right;
				cell20.ColumnSpan = 2;
				row2.Cells.Add (cell20);

				Control [] b = _createUserWizard.CreateButtonSet ("ContinueButton", CreateUserWizard.ContinueButtonCommandName, false, _createUserWizard.ID);
				for (int i = 0; i < b.Length; i++)
					cell20.Controls.Add (b [i]);

				// Row #3
				TableRow row3 = new TableRow ();
				TableCell cell30 = new TableCell ();

				cell30.Controls.Add (new Image ());
				HyperLink link = new HyperLink ();
				link.ID = "EditProfileLink";
				_createUserWizard.RegisterApplyStyle (link, _createUserWizard.HyperLinkStyle);
				cell30.Controls.Add (link);
				row3.Cells.Add (cell30);

				// table
				table.Rows.Add (row0);
				table.Rows.Add (row1);
				table.Rows.Add (row2);
				table.Rows.Add (row3);

				container.Controls.Add (table);
			}

			#endregion
		}
	}
}
