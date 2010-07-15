//
// System.Web.UI.WebControls.PasswordRecovery.cs
//
// Authors:
//	Vladimir Krasnov (vladimirk@mainsoft.com)
//
// (C) 2006 Mainsoft, Inc (http://www.mainsoft.com)
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
using System.Net.Mail;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing.Design;
using System.Web;
using System.Web.UI;
using System.Web.Security;

namespace System.Web.UI.WebControls
{
	[Bindable (false)]
	[DefaultEvent ("SendingMail")]
	[Designer ("System.Web.UI.Design.WebControls.PasswordRecoveryDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class PasswordRecovery : CompositeControl
	{
		static readonly object answerLookupErrorEvent = new object ();
		static readonly object sendingMailEvent = new object ();
		static readonly object sendMailErrorEvent = new object ();
		static readonly object userLookupErrorEvent = new object ();
		static readonly object verifyingAnswerEvent = new object ();
		static readonly object verifyingUserEvent = new object ();
		
		public static readonly string SubmitButtonCommandName = "Submit";

		TableItemStyle _failureTextStyle;
		TableItemStyle _hyperLinkStyle;
		TableItemStyle _instructionTextStyle;
		TableItemStyle _labelStyle;
		Style _submitButtonStyle;
		TableItemStyle _successTextStyle;
		Style _textBoxStyle;
		TableItemStyle _titleTextStyle;
		Style _validatorTextStyle;

		MailDefinition _mailDefinition;
		MembershipProvider _provider = null;

		ITemplate _questionTemplate = null;
		ITemplate _successTemplate = null;
		ITemplate _userNameTemplate = null;

		QuestionContainer _questionTemplateContainer = null;
		SuccessContainer _successTemplateContainer = null;
		UserNameContainer _userNameTemplateContainer = null;

		PasswordReciveryStep _currentStep = PasswordReciveryStep.StepUserName;

		string _username = null;
		string _answer = null;

		EventHandlerList events = new EventHandlerList ();

#region Events
		public event EventHandler AnswerLookupError {
			add { events.AddHandler (answerLookupErrorEvent, value); }
			remove { events.RemoveHandler (answerLookupErrorEvent, value); }
		}
		
		public event MailMessageEventHandler SendingMail {
			add { events.AddHandler (sendingMailEvent, value); }
			remove { events.RemoveHandler (sendingMailEvent, value); }
		}
		
		public event SendMailErrorEventHandler SendMailError {
			add { events.AddHandler (sendMailErrorEvent, value); }
			remove { events.RemoveHandler (sendMailErrorEvent, value); }
		}
		
		public event EventHandler UserLookupError {
			add { events.AddHandler (userLookupErrorEvent, value); }
			remove { events.RemoveHandler (userLookupErrorEvent, value); }
		}
		
		public event LoginCancelEventHandler VerifyingAnswer {
			add { events.AddHandler (verifyingAnswerEvent, value); }
			remove { events.RemoveHandler (verifyingAnswerEvent, value); }
		}
		
		public event LoginCancelEventHandler VerifyingUser {
			add { events.AddHandler (verifyingUserEvent, value); }
			remove { events.RemoveHandler (verifyingUserEvent, value); }
		}
#endregion

		public PasswordRecovery ()
		{
		}

		[Browsable (false)]
		[Filterable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Themeable (false)]
		public virtual string Answer {
			get { return _answer != null ? _answer : string.Empty; }
		}

		[Localizable (true)]
		public virtual string AnswerLabelText
		{
			get { return ViewState.GetString ("AnswerLabelText", "Answer:"); }
			set { ViewState ["AnswerLabelText"] = value; }
		}

		[Localizable (true)]
		public virtual string AnswerRequiredErrorMessage
		{
			get { return ViewState.GetString ("AnswerRequiredErrorMessage", "Answer is required."); }
			set { ViewState ["AnswerRequiredErrorMessage"] = value; }
		}

		[DefaultValue (1)]
		public virtual int BorderPadding
		{
			get { return ViewState.GetInt ("BorderPadding", 1); }
			set
			{
				if (value < -1)
					throw new ArgumentOutOfRangeException ();
				ViewState ["BorderPadding"] = value;
			}
		}

		[Localizable (true)]
		public virtual string GeneralFailureText
		{
			get { return ViewState.GetString ("GeneralFailureText", "Your attempt to retrieve your password was not successful. Please try again."); }
			set { ViewState ["GeneralFailureText"] = value; }
		}

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]	
		public virtual string HelpPageIconUrl
		{
			get { return ViewState.GetString ("HelpPageIconUrl", String.Empty); }
			set { ViewState ["HelpPageIconUrl"] = value; }
		}

		[DefaultValue ("")]
		[Localizable (true)]
		public virtual string HelpPageText
		{
			get { return ViewState.GetString ("HelpPageText", String.Empty); }
			set { ViewState ["HelpPageText"] = value; }
		}

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]	
		public virtual string HelpPageUrl
		{
			get { return ViewState.GetString ("HelpPageUrl", String.Empty); }
			set { ViewState ["HelpPageUrl"] = value; }
		}

		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[Themeable (false)]
		public MailDefinition MailDefinition {
			get {
				if (_mailDefinition == null) {
					_mailDefinition = new MailDefinition ();
					if (IsTrackingViewState)
						((IStateManager) _mailDefinition).TrackViewState ();
				}
				return _mailDefinition;
			}
		}

		[DefaultValue ("")]
		[Themeable (false)]
		public virtual string MembershipProvider
		{
			get { return ViewState.GetString ("MembershipProvider", String.Empty); }
			set { ViewState ["MembershipProvider"] = value; }
		}

		[Browsable (false)]
		[Filterable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Themeable (false)]
		public virtual string Question {
			get { return ViewState.GetString ("Question", ""); }
			private set { ViewState ["Question"] = value; }
		}

		[Localizable (true)]
		public virtual string QuestionFailureText
		{
			get { return ViewState.GetString ("QuestionFailureText", "Your answer could not be verified. Please try again."); }
			set { ViewState ["QuestionFailureText"] = value; }
		}

		[Localizable (true)]
		public virtual string QuestionInstructionText
		{
			get { return ViewState.GetString ("QuestionInstructionText", "Answer the following question to receive your password."); }
			set { ViewState ["QuestionInstructionText"] = value; }
		}

		[Localizable (true)]
		public virtual string QuestionLabelText
		{
			get { return ViewState.GetString ("QuestionLabelText", "Question:"); }
			set { ViewState ["QuestionLabelText"] = value; }
		}

		[Localizable (true)]
		public virtual string QuestionTitleText
		{
			get { return ViewState.GetString ("QuestionTitleText", "Identity Confirmation"); }
			set { ViewState ["QuestionTitleText"] = value; }
		}

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]	
		public virtual string SubmitButtonImageUrl
		{
			get { return ViewState.GetString ("SubmitButtonImageUrl", String.Empty); }
			set { ViewState ["SubmitButtonImageUrl"] = value; }
		}

		[Localizable (true)]
		public virtual string SubmitButtonText
		{
			get { return ViewState.GetString ("SubmitButtonText", "Submit"); }
			set { ViewState ["SubmitButtonText"] = value; }
		}

		[DefaultValue (ButtonType.Button)]
		public virtual ButtonType SubmitButtonType
		{
			get
			{
				object o = ViewState ["SubmitButtonType"];
				return (o == null) ? ButtonType.Button : (ButtonType) o;
			}
			set
			{
				if ((value < ButtonType.Button) || (value > ButtonType.Link))
					throw new ArgumentOutOfRangeException ("SubmitButtonType");
				ViewState ["SubmitButtonType"] = (int) value;
			}
		}

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]	
		[Themeable (false)]
		public virtual string SuccessPageUrl
		{
			get { return ViewState.GetString ("SuccessPageUrl", String.Empty); }
			set { ViewState ["SuccessPageUrl"] = value; }
		}

		[Localizable (true)]
		public virtual string SuccessText
		{
			get { return ViewState.GetString ("SuccessText", "Your password has been sent to you."); }
			set { ViewState ["SuccessText"] = value; }
		}

		[DefaultValue (LoginTextLayout.TextOnLeft)]
		public virtual LoginTextLayout TextLayout {
			get
			{
				object o = ViewState ["TextLayout"];
				return (o == null) ? LoginTextLayout.TextOnLeft : (LoginTextLayout) o;
			}
			set
			{
				if ((value < LoginTextLayout.TextOnLeft) || (value > LoginTextLayout.TextOnTop))
					throw new ArgumentOutOfRangeException ("TextLayout");
				ViewState ["TextLayout"] = (int) value;
			}
		}

		[DefaultValue ("")]
		[Localizable (true)]
		public virtual string UserName {
			get { return _username != null ? _username : String.Empty; }
			set { _username = value; }
		}

		[Localizable (true)]
		public virtual string UserNameFailureText
		{
			get { return ViewState.GetString ("UserNameFailureText", "We were unable to access your information. Please try again."); }
			set { ViewState ["UserNameFailureText"] = value; }
		}

		[Localizable (true)]
		public virtual string UserNameInstructionText
		{
			get { return ViewState.GetString ("UserNameInstructionText", "Enter your User Name to receive your password."); }
			set { ViewState ["UserNameInstructionText"] = value; }
		}

		[Localizable (true)]
		public virtual string UserNameLabelText
		{
			get { return ViewState.GetString ("UserNameLabelText", "User Name:"); }
			set { ViewState ["UserNameLabelText"] = value; }
		}

		[Localizable (true)]
		public virtual string UserNameRequiredErrorMessage
		{
			get { return ViewState.GetString ("UserNameRequiredErrorMessage", "User Name is required."); }
			set { ViewState ["UserNameRequiredErrorMessage"] = value; }
		}

		[Localizable (true)]
		public virtual string UserNameTitleText
		{
			get { return ViewState.GetString ("UserNameTitleText", "Forgot Your Password?"); }
			set { ViewState ["UserNameTitleText"] = value; }
		}

		[Browsable (false)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[TemplateContainer (typeof (PasswordRecovery))]
		public virtual ITemplate QuestionTemplate
		{
			get { return _questionTemplate; }
			set { _questionTemplate = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public Control QuestionTemplateContainer
		{
			get {
				if (_questionTemplateContainer == null) {
					_questionTemplateContainer = new QuestionContainer (this);
					ITemplate template = QuestionTemplate;
					if (template != null)
						_questionTemplateContainer.InstantiateTemplate (template);
				}
				
				return _questionTemplateContainer;
			}
		}

		[Browsable (false)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[TemplateContainer (typeof (PasswordRecovery))]
		public virtual ITemplate SuccessTemplate
		{
			get { return _successTemplate; }
			set { _successTemplate = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public Control SuccessTemplateContainer
		{
			get {
				if (_successTemplateContainer == null) {
					_successTemplateContainer = new SuccessContainer (this);
					ITemplate template = SuccessTemplate;
					if (template != null)
						_successTemplateContainer.InstantiateTemplate (template);
				}
				
				return _successTemplateContainer;
			}
		}

		[Browsable (false)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[TemplateContainer (typeof (PasswordRecovery))]
		public virtual ITemplate UserNameTemplate {
			get { return _userNameTemplate; }
			set { _userNameTemplate = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public Control UserNameTemplateContainer {
			get {
				if (_userNameTemplateContainer == null) {
					_userNameTemplateContainer = new UserNameContainer (this);
					ITemplate template = UserNameTemplate;
					if (template != null)
						_userNameTemplateContainer.InstantiateTemplate (template);
				}
				
				return _userNameTemplateContainer;
			}
		}

		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		public TableItemStyle FailureTextStyle {
			get {
				if (_failureTextStyle == null) {
					_failureTextStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						_failureTextStyle.TrackViewState ();
				}
				return _failureTextStyle;
			}
		}

		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		public TableItemStyle HyperLinkStyle {
			get {
				if (_hyperLinkStyle == null) {
					_hyperLinkStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						_hyperLinkStyle.TrackViewState ();
				}
				return _hyperLinkStyle;
			}
		}

		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		public TableItemStyle InstructionTextStyle {
			get {
				if (_instructionTextStyle == null) {
					_instructionTextStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						_instructionTextStyle.TrackViewState ();
				}
				return _instructionTextStyle;
			}
		}

		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		public TableItemStyle LabelStyle {
			get {
				if (_labelStyle == null) {
					_labelStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						_labelStyle.TrackViewState ();
				}
				return _labelStyle;
			}
		}

		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		public Style SubmitButtonStyle {
			get {
				if (_submitButtonStyle == null) {
					_submitButtonStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						_submitButtonStyle.TrackViewState ();
				}
				return _submitButtonStyle;
			}
		}

		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		public TableItemStyle SuccessTextStyle {
			get {
				if (_successTextStyle == null) {
					_successTextStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						_successTextStyle.TrackViewState ();
				}
				return _successTextStyle;
			}
		}

		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		public Style TextBoxStyle {
			get {
				if (_textBoxStyle == null) {
					_textBoxStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						_textBoxStyle.TrackViewState ();
				}
				return _textBoxStyle;
			}
		}

		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		public TableItemStyle TitleTextStyle {
			get {
				if (_titleTextStyle == null) {
					_titleTextStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						_titleTextStyle.TrackViewState ();
				}
				return _titleTextStyle;
			}
		}

		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		public Style ValidatorTextStyle {
			get {
				if (_validatorTextStyle == null) {
					_validatorTextStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						_validatorTextStyle.TrackViewState ();
				}
				return _validatorTextStyle;
			}
		}

		#region Protected Properties

		protected override HtmlTextWriterTag TagKey {
			get { return HtmlTextWriterTag.Table; }
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

		#endregion

		protected internal override void CreateChildControls ()
		{
			ITemplate userNameTemplate = UserNameTemplate;
			if (userNameTemplate == null) {
				userNameTemplate = new UserNameDefaultTemplate (this);
				((UserNameContainer) UserNameTemplateContainer).InstantiateTemplate (userNameTemplate);
			}
			
			ITemplate questionTemplate = QuestionTemplate;
			if (questionTemplate == null) {
				questionTemplate = new QuestionDefaultTemplate (this);
				((QuestionContainer) QuestionTemplateContainer).InstantiateTemplate (questionTemplate);
			}

			ITemplate successTemplate = SuccessTemplate;
			if (successTemplate == null) {
				successTemplate = new SuccessDefaultTemplate (this);
				((SuccessContainer) SuccessTemplateContainer).InstantiateTemplate (successTemplate);
			}

			Controls.AddAt (0, UserNameTemplateContainer);
			Controls.AddAt (1, QuestionTemplateContainer);
			Controls.AddAt (2, SuccessTemplateContainer);

			IEditableTextControl editable;

			editable = ((UserNameContainer) UserNameTemplateContainer).UserNameTextBox;
			if (editable != null)
				editable.TextChanged += new EventHandler (UserName_TextChanged);

			editable = ((QuestionContainer) QuestionTemplateContainer).AnswerTextBox;
			if (editable != null)
				editable.TextChanged += new EventHandler (Answer_TextChanged);
		}

		#region Protected methods

		protected internal override void Render (HtmlTextWriter writer)
		{
			((QuestionContainer) QuestionTemplateContainer).UpdateChildControls ();

			for (int i = 0; i < Controls.Count; i++)
				if (Controls [i].Visible)
					Controls [i].Render (writer);
		}

		protected internal override void LoadControlState (object savedState)
		{
			if (savedState == null) return;
			object [] state = (object []) savedState;
			base.LoadControlState (state [0]);

			_currentStep = (PasswordReciveryStep) state [1];
			_username = (string) state [2];
		}

		protected internal override object SaveControlState ()
		{
			object state = base.SaveControlState ();
			return new object [] { state, _currentStep, _username };
		}

		protected override void TrackViewState ()
		{
			base.TrackViewState ();

			if (_failureTextStyle != null)
				_failureTextStyle.TrackViewState ();

			if (_hyperLinkStyle != null)
				_hyperLinkStyle.TrackViewState ();

			if (_instructionTextStyle != null)
				_instructionTextStyle.TrackViewState ();

			if (_labelStyle != null)
				_labelStyle.TrackViewState ();

			if (_submitButtonStyle != null)
				_submitButtonStyle.TrackViewState ();

			if (_successTextStyle != null)
				_successTextStyle.TrackViewState ();

			if (_textBoxStyle != null)
				_textBoxStyle.TrackViewState ();

			if (_titleTextStyle != null)
				_titleTextStyle.TrackViewState ();

			if (_validatorTextStyle != null)
				_validatorTextStyle.TrackViewState ();

			if (_mailDefinition != null)
				((IStateManager) _mailDefinition).TrackViewState ();
		}

		protected override void LoadViewState (object savedState)
		{
			if (savedState == null)
				return;

			object [] states = (object []) savedState;
			base.LoadViewState (states [0]);

			if (states [1] != null)
				FailureTextStyle.LoadViewState (states [1]);

			if (states [2] != null)
				HyperLinkStyle.LoadViewState (states [2]);

			if (states [3] != null)
				InstructionTextStyle.LoadViewState (states [3]);

			if (states [4] != null)
				LabelStyle.LoadViewState (states [4]);

			if (states [5] != null)
				SubmitButtonStyle.LoadViewState (states [5]);

			if (states [6] != null)
				SuccessTextStyle.LoadViewState (states [6]);

			if (states [7] != null)
				TextBoxStyle.LoadViewState (states [7]);

			if (states [8] != null)
				TitleTextStyle.LoadViewState (states [8]);

			if (states [9] != null)
				ValidatorTextStyle.LoadViewState (states [9]);

			if (states [10] != null)
				((IStateManager) MailDefinition).LoadViewState (states [10]);
		}

		protected override object SaveViewState ()
		{
			object [] states = new object [11];
			states [0] = base.SaveViewState ();

			if (_failureTextStyle != null)
				states [1] = _failureTextStyle.SaveViewState ();

			if (_hyperLinkStyle != null)
				states [2] = _hyperLinkStyle.SaveViewState ();

			if (_instructionTextStyle != null)
				states [3] = _instructionTextStyle.SaveViewState ();

			if (_labelStyle != null)
				states [4] = _labelStyle.SaveViewState ();

			if (_submitButtonStyle != null)
				states [5] = _submitButtonStyle.SaveViewState ();

			if (_successTextStyle != null)
				states [6] = _successTextStyle.SaveViewState ();

			if (_textBoxStyle != null)
				states [7] = _textBoxStyle.SaveViewState ();

			if (_titleTextStyle != null)
				states [8] = _titleTextStyle.SaveViewState ();

			if (_validatorTextStyle != null)
				states [9] = _validatorTextStyle.SaveViewState ();

			if (_mailDefinition != null)
				states [10] = ((IStateManager) _mailDefinition).SaveViewState ();

			for (int i = 0; i < states.Length; i++) {
				if (states [i] != null)
					return states;
			}
			return null;

		}

		#endregion

		void ProcessCommand (CommandEventArgs args)
		{
			if (!Page.IsValid)
				return;

			switch (_currentStep) {
				case PasswordReciveryStep.StepUserName:
					ProcessUserName ();
					break;
				case PasswordReciveryStep.StepAnswer:
					ProcessUserAnswer ();
					break;
			}

		}

		void ProcessUserName ()
		{
			LoginCancelEventArgs args = new LoginCancelEventArgs ();
			OnVerifyingUser (args);
			if (args.Cancel)
				return;

			MembershipUser user = MembershipProviderInternal.GetUser (UserName, false);
			if (user == null) {
				OnUserLookupError (EventArgs.Empty);
				((UserNameContainer) UserNameTemplateContainer).FailureTextLiteral.Text = UserNameFailureText;
				return;
			}

			if (!MembershipProviderInternal.RequiresQuestionAndAnswer) {
				GenerateAndSendEmail ();

				_currentStep = PasswordReciveryStep.StepSuccess;
				return;
			}

			Question = user.PasswordQuestion;
			_currentStep = PasswordReciveryStep.StepAnswer;
			return;
		}

		void ProcessUserAnswer ()
		{
			LoginCancelEventArgs args = new LoginCancelEventArgs ();
			OnVerifyingAnswer (args);
			if (args.Cancel)
				return;

			MembershipUser user = MembershipProviderInternal.GetUser (UserName, false);
			if (user == null || string.IsNullOrEmpty (user.Email)) {
				((QuestionContainer) QuestionTemplateContainer).FailureTextLiteral.Text = GeneralFailureText;
				return;
			}

			GenerateAndSendEmail ();

			_currentStep = PasswordReciveryStep.StepSuccess;
			return;
		}

		void GenerateAndSendEmail ()
		{
			string newPassword = "";
			try {
				if (MembershipProviderInternal.EnablePasswordRetrieval) {
					newPassword = MembershipProviderInternal.GetPassword (UserName, Answer);
				}
				else if (MembershipProviderInternal.EnablePasswordReset) {
					newPassword = MembershipProviderInternal.ResetPassword (UserName, Answer);
				}
				else
					throw new HttpException ("Membership provider does not support password retrieval or reset.");
			}
			catch (MembershipPasswordException) {
				OnAnswerLookupError (EventArgs.Empty);
				((QuestionContainer) QuestionTemplateContainer).FailureTextLiteral.Text = QuestionFailureText;
				return;
			}

			SendPasswordByMail (UserName, newPassword);
		}

		void InitMemberShipProvider ()
		{
			string mp = MembershipProvider;
			_provider = (mp.Length == 0) ? _provider = Membership.Provider : Membership.Providers [mp];
			if (_provider == null)
				throw new HttpException (Locale.GetText ("No provider named '{0}' could be found.", mp));
		}

		void SendPasswordByMail (string username, string password)
		{
			MembershipUser user = MembershipProviderInternal.GetUser (UserName, false);
			if (user == null)
				return;

			// DO NOT change format of the message - it has to be exactly the same as in
			// .NET as some software (e.g. YetAnotherForum) depends on it.
			string messageText = "Please return to the site and log in using the following information.\n" +
				"User Name: <%USERNAME%>\nPassword: <%PASSWORD%>\n";

			ListDictionary dictionary = new ListDictionary (StringComparer.OrdinalIgnoreCase);
			dictionary.Add ("<%USERNAME%>", username);
			dictionary.Add ("<% UserName %>", username);
			dictionary.Add ("<%PASSWORD%>", password);
			dictionary.Add ("<% Password %>", password);

			MailMessage message = null;
			
			if (MailDefinition.BodyFileName.Length == 0)
				message = MailDefinition.CreateMailMessage (user.Email, dictionary, messageText, this);
			else
				message = MailDefinition.CreateMailMessage (user.Email, dictionary, this);

			if (string.IsNullOrEmpty (message.Subject))
				message.Subject = "Password";

			MailMessageEventArgs args = new MailMessageEventArgs (message);
			OnSendingMail (args);

			SmtpClient smtpClient = new SmtpClient ();
			try {
				smtpClient.Send (message);
			}
			catch (Exception e) {
				SendMailErrorEventArgs mailArgs = new SendMailErrorEventArgs (e);
				OnSendMailError (mailArgs);
				if (!mailArgs.Handled)
					throw e;
			}
		}

		#region Event handlers

		protected virtual void OnAnswerLookupError (EventArgs e)
		{
		}

		protected override bool OnBubbleEvent (object source, EventArgs e)
		{
			CommandEventArgs args = e as CommandEventArgs;
			if (e != null && args.CommandName == SubmitButtonCommandName) {
				ProcessCommand (args);
				return true;
			}
			return base.OnBubbleEvent (source, e);
		}

		protected internal override void OnInit (EventArgs e)
		{
			Page.RegisterRequiresControlState (this);
			base.OnInit (e);
		}

		protected internal override void OnPreRender (EventArgs e)
		{
			UserNameTemplateContainer.Visible = false;
			QuestionTemplateContainer.Visible = false;
			SuccessTemplateContainer.Visible = false;

			switch (_currentStep) {
				case PasswordReciveryStep.StepUserName:
					UserNameTemplateContainer.Visible = true;
					break;
				case PasswordReciveryStep.StepAnswer:
					QuestionTemplateContainer.Visible = true;
					break;
				case PasswordReciveryStep.StepSuccess:
					SuccessTemplateContainer.Visible = true;
					break;
			}

			base.OnPreRender (e);
		}

		protected virtual void OnSendingMail (MailMessageEventArgs e)
		{
			MailMessageEventHandler eh = events [sendingMailEvent] as MailMessageEventHandler;
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnSendMailError (SendMailErrorEventArgs e)
		{
			SendMailErrorEventHandler eh = events [sendingMailEvent] as SendMailErrorEventHandler;
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnUserLookupError (EventArgs e)
		{
			EventHandler eh = events [userLookupErrorEvent] as EventHandler;
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnVerifyingAnswer (LoginCancelEventArgs e)
		{
			LoginCancelEventHandler eh = events [verifyingAnswerEvent] as LoginCancelEventHandler;
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnVerifyingUser (LoginCancelEventArgs e)
		{
			LoginCancelEventHandler eh = events [verifyingUserEvent] as LoginCancelEventHandler;
			if (eh != null)
				eh (this, e);
		}

		#endregion

		#region Private Event Handlers

		void UserName_TextChanged (object sender, EventArgs e)
		{
			UserName = ((ITextControl) sender).Text;
		}

		void Answer_TextChanged (object sender, EventArgs e)
		{
			_answer = ((ITextControl) sender).Text;
		}

		#endregion

		[MonoTODO ("Not implemented")]
		protected override void SetDesignModeState (IDictionary data)
		{
			throw new NotImplementedException ();
		}


		abstract class BasePasswordRecoveryContainer : Table, INamingContainer
		{
			protected readonly PasswordRecovery _owner = null;
			TableCell _containerCell = null;

			public BasePasswordRecoveryContainer (PasswordRecovery owner)
			{
				_owner = owner;
				InitTable ();
			}

			public void InstantiateTemplate (ITemplate template)
			{
				template.InstantiateIn (_containerCell);
			}

			void InitTable ()
			{
				Attributes.Add ("ID", _owner.ID);

				CellSpacing = 0;
				CellPadding = _owner.BorderPadding;

				_containerCell = new TableCell ();

				TableRow row = new TableRow ();
				row.Cells.Add (_containerCell);
				Rows.Add (row);
			}

			protected internal override void OnPreRender (EventArgs e)
			{
				ApplyStyle (_owner.ControlStyle);
				base.OnPreRender (e);
			}

			public abstract void UpdateChildControls();
		}


		sealed class QuestionContainer : BasePasswordRecoveryContainer
		{
			public QuestionContainer (PasswordRecovery owner)
				: base (owner)
			{
			}
			// Requried controls
			public IEditableTextControl AnswerTextBox
			{
				get
				{
					Control c = FindControl ("Answer");
					if (c == null)
						throw new HttpException ("QuestionTemplate does not contain an IEditableTextControl with ID Answer for the username.");
					return c as IEditableTextControl;
				}
			}
			// Optional controls
			public Literal UserNameLiteral
			{
				get { return FindControl ("UserName") as Literal; }
			}
			public Literal QuestionLiteral
			{
				get { return FindControl ("Question") as Literal; }
			}
			public Literal FailureTextLiteral
			{
				get { return FindControl ("FailureText") as Literal; }
			}

			public override void UpdateChildControls ()
			{
				if (UserNameLiteral != null)
					UserNameLiteral.Text = _owner.UserName;

				if (QuestionLiteral != null)
					QuestionLiteral.Text = _owner.Question;
			}
		}

		sealed class SuccessContainer : BasePasswordRecoveryContainer
		{
			public SuccessContainer (PasswordRecovery owner)
				: base (owner)
			{
			}

			public override void UpdateChildControls ()
			{
			}
		}

		sealed class UserNameContainer : BasePasswordRecoveryContainer
		{
			public UserNameContainer (PasswordRecovery owner)
				: base (owner)
			{
			}
			// Requried controls
			public IEditableTextControl UserNameTextBox
			{
				get
				{
					Control c = FindControl ("UserName");
					if (c == null)
						throw new HttpException ("UserNameTemplate does not contain an IEditableTextControl with ID UserName for the username.");
					return c as IEditableTextControl;
				}
			}
			// Optional controls
			public ITextControl FailureTextLiteral
			{
				get { return FindControl ("FailureText") as ITextControl; }
			}

			public override void UpdateChildControls ()
			{
			}
		}

		class TemplateUtils
		{
			public static TableRow CreateRow(Control c1, Control c2, Style s1, Style s2, bool twoCells)
			{
				TableRow row = new TableRow ();
				TableCell cell1 = new TableCell ();

				cell1.Controls.Add (c1);
				if (s1 != null)
					cell1.ApplyStyle (s1);

				row.Cells.Add (cell1);

				if (c2 != null) {
					TableCell cell2 = new TableCell ();
					cell2.Controls.Add (c2);

					if (s2 != null)
						cell2.ApplyStyle (s2);

					row.Cells.Add (cell2);

					cell1.HorizontalAlign = HorizontalAlign.Right;
					cell2.HorizontalAlign = HorizontalAlign.Left;
				}
				else {
					cell1.HorizontalAlign = HorizontalAlign.Center;
					if (twoCells)
						cell1.ColumnSpan = 2;
				}
				
				return row;
			}

			public static TableRow CreateHelpRow (string pageUrl, string linkText, string linkIcon, Style linkStyle, bool twoCells)
			{
				TableRow row = new TableRow ();
				TableCell cell1 = new TableCell ();

				if (linkIcon.Length > 0) {
					Image img = new Image ();
					img.ImageUrl = linkIcon;
					cell1.Controls.Add (img);
				}
				if (linkText.Length > 0) {
					HyperLink link = new HyperLink ();
					link.NavigateUrl = pageUrl;
					link.Text = linkText;
					link.ControlStyle.CopyTextStylesFrom (linkStyle);
					cell1.Controls.Add (link);
				}

				if (twoCells)
					cell1.ColumnSpan = 2;

				row.ControlStyle.CopyFrom (linkStyle);
				row.Cells.Add (cell1);
				return row;
			}
		}

		sealed class UserNameDefaultTemplate : ITemplate
		{
			readonly PasswordRecovery _owner = null;

			public UserNameDefaultTemplate (PasswordRecovery _owner)
			{
				this._owner = _owner;
			}

			public void InstantiateIn (Control container)
			{
				Table table = new Table ();
				table.CellPadding = 0;

				bool twoCells = _owner.TextLayout == LoginTextLayout.TextOnLeft;

				// row 0
				table.Rows.Add (
					TemplateUtils.CreateRow (new LiteralControl (_owner.UserNameTitleText), null, _owner.TitleTextStyle, null, twoCells));

				// row 1
				table.Rows.Add (
					TemplateUtils.CreateRow (new LiteralControl (_owner.UserNameInstructionText), null, _owner.InstructionTextStyle, null, twoCells));

				// row 2
				TextBox UserNameTextBox = new TextBox ();
				UserNameTextBox.ID = "UserName";
				UserNameTextBox.Text = _owner.UserName;
				UserNameTextBox.ApplyStyle (_owner.TextBoxStyle);

				Label UserNameLabel = new Label ();
				UserNameLabel.ID = "UserNameLabel";
				UserNameLabel.AssociatedControlID = "UserName";
				UserNameLabel.Text = _owner.UserNameLabelText;
				UserNameLabel.ApplyStyle (_owner.LabelStyle);

				RequiredFieldValidator UserNameRequired = new RequiredFieldValidator ();
				UserNameRequired.ID = "UserNameRequired";
				UserNameRequired.ControlToValidate = "UserName";
				UserNameRequired.ErrorMessage = _owner.UserNameRequiredErrorMessage;
				UserNameRequired.ToolTip = _owner.UserNameRequiredErrorMessage;
				UserNameRequired.Text = "*";
				UserNameRequired.ValidationGroup = _owner.ID;
				UserNameRequired.ApplyStyle (_owner.ValidatorTextStyle);

				if (twoCells) {
					TableRow row = TemplateUtils.CreateRow (UserNameLabel, UserNameTextBox, null, null, twoCells);
					row.Cells [1].Controls.Add (UserNameRequired);
					table.Rows.Add (row);
				}
				else {
					table.Rows.Add (TemplateUtils.CreateRow (UserNameLabel, null, null, null, twoCells));
					TableRow row = TemplateUtils.CreateRow (UserNameTextBox, null, null, null, twoCells);
					row.Cells [0].Controls.Add (UserNameRequired);
					table.Rows.Add (row);
				}

				// row 3
				Literal FailureText = new Literal ();
				FailureText.ID = "FailureText";
				if (_owner.FailureTextStyle.ForeColor.IsEmpty)
					_owner.FailureTextStyle.ForeColor = System.Drawing.Color.Red;
				table.Rows.Add (TemplateUtils.CreateRow (FailureText, null, _owner.FailureTextStyle, null, twoCells));

				// row 4
				WebControl SubmitButton = null;
				switch (_owner.SubmitButtonType) {
					case ButtonType.Button:
						SubmitButton = new Button ();
						break;
					case ButtonType.Image:
						SubmitButton = new ImageButton ();
						break;
					case ButtonType.Link:
						SubmitButton = new LinkButton ();
						break;
				}

				SubmitButton.ID = "SubmitButton";
				SubmitButton.ApplyStyle (_owner.SubmitButtonStyle);
				((IButtonControl) SubmitButton).CommandName = PasswordRecovery.SubmitButtonCommandName;
				((IButtonControl) SubmitButton).Text = _owner.SubmitButtonText;
				((IButtonControl) SubmitButton).ValidationGroup = _owner.ID;

				TableRow buttonRow = TemplateUtils.CreateRow (SubmitButton, null, null, null, twoCells);
				buttonRow.Cells [0].HorizontalAlign = HorizontalAlign.Right;
				table.Rows.Add (buttonRow);

				// row 5
				table.Rows.Add (
					TemplateUtils.CreateHelpRow (
					_owner.HelpPageUrl, _owner.HelpPageText, _owner.HelpPageIconUrl, _owner.HyperLinkStyle, twoCells));

				container.Controls.Add (table);
			}
		}

		sealed class QuestionDefaultTemplate : ITemplate
		{
			readonly PasswordRecovery _owner = null;

			public QuestionDefaultTemplate (PasswordRecovery _owner)
			{
				this._owner = _owner;
			}

			public void InstantiateIn (Control container)
			{
				Table table = new Table ();
				table.CellPadding = 0;

				bool twoCells = _owner.TextLayout == LoginTextLayout.TextOnLeft;

				// row 0
				table.Rows.Add (
					TemplateUtils.CreateRow (new LiteralControl (_owner.QuestionTitleText), null, _owner.TitleTextStyle, null, twoCells));

				// row 1
				table.Rows.Add (
					TemplateUtils.CreateRow (new LiteralControl (_owner.QuestionInstructionText), null, _owner.InstructionTextStyle, null, twoCells));

				// row 2
				Literal UserNameLiteral = new Literal ();
				UserNameLiteral.ID = "UserName";

				table.Rows.Add (
					TemplateUtils.CreateRow (new LiteralControl (_owner.UserNameLabelText), UserNameLiteral, _owner.LabelStyle, _owner.LabelStyle, twoCells));

				// row 3
				Literal QuestionLiteral = new Literal ();
				QuestionLiteral.ID = "Question";

				table.Rows.Add (
					TemplateUtils.CreateRow (new LiteralControl (_owner.QuestionLabelText), QuestionLiteral, _owner.LabelStyle, _owner.LabelStyle, twoCells));

				// row 5
				TextBox AnswerTextBox = new TextBox ();
				AnswerTextBox.ID = "Answer";
				AnswerTextBox.ApplyStyle (_owner.TextBoxStyle);

				Label AnswerLabel = new Label ();
				AnswerLabel.ID = "AnswerLabel";
				AnswerLabel.AssociatedControlID = "Answer";
				AnswerLabel.Text = _owner.AnswerLabelText;
				AnswerLabel.ApplyStyle (_owner.LabelStyle);

				RequiredFieldValidator AnswerRequired = new RequiredFieldValidator ();
				AnswerRequired.ID = "AnswerRequired";
				AnswerRequired.ControlToValidate = "Answer";
				AnswerRequired.ErrorMessage = _owner.AnswerRequiredErrorMessage;
				AnswerRequired.ToolTip = _owner.AnswerRequiredErrorMessage;
				AnswerRequired.Text = "*";
				AnswerRequired.ValidationGroup = _owner.ID;
				AnswerRequired.ApplyStyle (_owner.ValidatorTextStyle);

				if (twoCells) {
					TableRow row = TemplateUtils.CreateRow (AnswerLabel, AnswerTextBox, null, null, twoCells);
					row.Cells [1].Controls.Add (AnswerRequired);
					table.Rows.Add (row);
				}
				else {
					table.Rows.Add (TemplateUtils.CreateRow (AnswerLabel, null, null, null, twoCells));
					TableRow row = TemplateUtils.CreateRow (AnswerTextBox, null, null, null, twoCells);
					row.Cells [0].Controls.Add (AnswerRequired);
					table.Rows.Add (row);
				}

				// row 6
				Literal FailureText = new Literal ();
				FailureText.ID = "FailureText";
				if (_owner.FailureTextStyle.ForeColor.IsEmpty)
					_owner.FailureTextStyle.ForeColor = System.Drawing.Color.Red;
				table.Rows.Add (TemplateUtils.CreateRow (FailureText, null, _owner.FailureTextStyle, null, twoCells));

				// row 7
				WebControl SubmitButton = null;
				switch (_owner.SubmitButtonType) {
					case ButtonType.Button:
						SubmitButton = new Button ();
						break;
					case ButtonType.Image:
						SubmitButton = new ImageButton ();
						break;
					case ButtonType.Link:
						SubmitButton = new LinkButton ();
						break;
				}

				SubmitButton.ID = "SubmitButton";
				SubmitButton.ApplyStyle (_owner.SubmitButtonStyle);
				((IButtonControl) SubmitButton).CommandName = PasswordRecovery.SubmitButtonCommandName;
				((IButtonControl) SubmitButton).Text = _owner.SubmitButtonText;
				((IButtonControl) SubmitButton).ValidationGroup = _owner.ID;

				TableRow buttonRow = TemplateUtils.CreateRow (SubmitButton, null, null, null, twoCells);
				buttonRow.Cells [0].HorizontalAlign = HorizontalAlign.Right;
				table.Rows.Add (buttonRow);

				// row 8
				table.Rows.Add (
					TemplateUtils.CreateHelpRow (
					_owner.HelpPageUrl, _owner.HelpPageText, _owner.HelpPageIconUrl, _owner.HyperLinkStyle, twoCells));

				container.Controls.Add (table);
			}
		}

		sealed class SuccessDefaultTemplate : ITemplate
		{
			readonly PasswordRecovery _owner = null;

			public SuccessDefaultTemplate (PasswordRecovery _owner)
			{
				this._owner = _owner;
			}

			public void InstantiateIn (Control container)
			{
				Table table = new Table ();
				table.CellPadding = 0;

				bool twoCells = _owner.TextLayout == LoginTextLayout.TextOnLeft;

				// row 0
				table.Rows.Add (
					TemplateUtils.CreateRow (new LiteralControl (_owner.SuccessText), null, _owner.SuccessTextStyle, null, twoCells));

				container.Controls.Add (table);
			}
		}

		enum PasswordReciveryStep
		{
			StepUserName,
			StepAnswer,
			StepSuccess,
		}
	}
}

#endif
