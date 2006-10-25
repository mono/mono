//
// System.Web.UI.WebControls.ChangePassword.cs
//
// Authors:
//	Igor Zelmanovich (igorz@mainsoft.com)
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
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Collections;

namespace System.Web.UI.WebControls
{
	public class ChangePassword : CompositeControl, INamingContainer
	{
		public static readonly string CancelButtonCommandName = "Cancel";
		public static readonly string ChangePasswordButtonCommandName = "ChangePassword";
		public static readonly string ContinueButtonCommandName = "Continue";

		Style _cancelButtonStyle;
		Style _changePasswordButtonStyle;
		Style _continueButtonStyle;
		TableItemStyle _failureTextStyle;
		TableItemStyle _hyperLinkStyle;
		TableItemStyle _instructionTextStyle;
		TableItemStyle _labelStyle;
		TableItemStyle _passwordHintStyle;
		TableItemStyle _successTextStyle;
		Style _textBoxStyle;
		TableItemStyle _titleTextStyle;
		Style _validatorTextStyle;

		MailDefinition _mailDefinition;

		ITemplate _changePasswordTemplate;
		ITemplate _successTemplate;

		[DefaultValue (1)]
		public virtual int BorderPadding {
			get { return ViewState.GetInt ("BorderPadding", 1); }
			set {
				if (value < -1)
					throw new ArgumentOutOfRangeException ();
				ViewState ["BorderPadding"] = value;
			}
		}
		
		[DefaultValue ("")]
		public virtual string CancelButtonImageUrl {
			get { return ViewState.GetString ("CancelButtonImageUrl", String.Empty); }
			set { ViewState ["CancelButtonImageUrl"] = value; }
		}

		[PersistenceMode (PersistenceMode.InnerProperty)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		public Style CancelButtonStyle {
			get {
				if (_cancelButtonStyle == null) {
					_cancelButtonStyle = new Style ();
					if (IsTrackingViewState)
						_cancelButtonStyle.TrackViewState ();
				}
				return _cancelButtonStyle;
			}
		}
		
		[Localizable (true)]
		public virtual string CancelButtonText {
			get { return ViewState.GetString ("CancelButtonText", "Cancel"); }
			set { ViewState ["CancelButtonText"] = value; }
		}
		
		public virtual ButtonType CancelButtonType {
			get { return ViewState ["CancelButtonType"] == null ? ButtonType.Button : (ButtonType) ViewState ["CancelButtonType"]; }
			set { ViewState ["CancelButtonType"] = value; }
		}
		
		[Themeable (false)]
		public virtual string CancelDestinationPageUrl {
			get { return ViewState.GetString ("CancelDestinationPageUrl", String.Empty); }
			set { ViewState ["CancelDestinationPageUrl"] = value; }
		}

		public virtual string ChangePasswordButtonImageUrl {
			get { return ViewState.GetString ("ChangePasswordButtonImageUrl", String.Empty); }
			set { ViewState ["ChangePasswordButtonImageUrl"] = value; }
		}

		[DefaultValue ("")]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		public Style ChangePasswordButtonStyle {
			get {
				if (_changePasswordButtonStyle == null) {
					_changePasswordButtonStyle = new Style ();
					if (IsTrackingViewState)
						_changePasswordButtonStyle.TrackViewState ();
				}
				return _changePasswordButtonStyle;
			}
		}

		[Localizable (true)]
		public virtual string ChangePasswordButtonText {
			get { return ViewState.GetString ("ChangePasswordButtonText", "Change Password"); }
			set { ViewState ["ChangePasswordButtonText"] = value; }
		}

		public virtual ButtonType ChangePasswordButtonType {
			get { return ViewState ["ChangePasswordButtonType"] == null ? ButtonType.Button : (ButtonType) ViewState ["ChangePasswordButtonType"]; }
			set { ViewState ["ChangePasswordButtonType"] = value; }
		}

		[Localizable (true)]
		public virtual string ChangePasswordFailureText {
			get { return ViewState.GetString ("ChangePasswordFailureText", "Your attempt to change passwords was unsuccessful. Please try again."); }
			set { ViewState ["ChangePasswordFailureText"] = value; }
		}

		[Browsable (false)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[TemplateContainer (typeof (ChangePassword))]
		public virtual ITemplate ChangePasswordTemplate {
			get { return _changePasswordTemplate; }
			set { _changePasswordTemplate = value; }
		}

		[MonoTODO]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public Control ChangePasswordTemplateContainer {
			get { throw new NotImplementedException (); }
		}

		[Localizable (true)]
		public virtual string ChangePasswordTitleText {
			get { return ViewState.GetString ("ChangePasswordTitleText", "Change Your Password"); }
			set { ViewState ["ChangePasswordTitleText"] = value; }
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[Themeable (false)]
		[Filterable (false)]
		public virtual string ConfirmNewPassword {
			get { throw new NotImplementedException (); }
		}

		[Localizable (true)]
		public virtual string ConfirmNewPasswordLabelText {
			get { return ViewState.GetString ("ConfirmNewPasswordLabelText", "Confirm New Password:"); }
			set { ViewState ["ConfirmNewPasswordLabelText"] = value; }
		}

		[Localizable (true)]
		public virtual string ConfirmPasswordCompareErrorMessage {
			get { return ViewState.GetString ("ConfirmPasswordCompareErrorMessage", "The confirm New Password entry must match the New Password entry."); }
			set { ViewState ["ConfirmPasswordCompareErrorMessage"] = value; }
		}

		[Localizable (true)]
		public virtual string ConfirmPasswordRequiredErrorMessage {
			get { return ViewState.GetString ("ConfirmPasswordRequiredErrorMessage", String.Empty); }
			set { ViewState ["ConfirmPasswordRequiredErrorMessage"] = value; }
		}

		public virtual string ContinueButtonImageUrl {
			get { return ViewState.GetString ("ContinueButtonImageUrl", String.Empty); }
			set { ViewState ["ContinueButtonImageUrl"] = value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public Style ContinueButtonStyle {
			get {
				if (_continueButtonStyle == null) {
					_continueButtonStyle = new Style ();
					if (IsTrackingViewState)
						_continueButtonStyle.TrackViewState ();
				}
				return _continueButtonStyle;
			}
		}

		[Localizable (true)]
		public virtual string ContinueButtonText {
			get { return ViewState.GetString ("ContinueButtonText", "Continue"); }
			set { ViewState ["ContinueButtonText"] = value; }
		}

		public virtual ButtonType ContinueButtonType {
			get { return ViewState ["ContinueButtonType"] == null ? ButtonType.Button : (ButtonType) ViewState ["ContinueButtonType"]; }
			set { ViewState ["ContinueButtonType"] = value; }
		}

		[Themeable (false)]
		public virtual string ContinueDestinationPageUrl {
			get { return ViewState.GetString ("ContinueDestinationPageUrl", String.Empty); }
			set { ViewState ["ContinueDestinationPageUrl"] = value; }
		}

		public virtual string CreateUserIconUrl {
			get { return ViewState.GetString ("CreateUserIconUrl", String.Empty); }
			set { ViewState ["CreateUserIconUrl"] = value; }
		}

		[Localizable (true)]
		public virtual string CreateUserText {
			get { return ViewState.GetString ("CreateUserText", String.Empty); }
			set { ViewState ["CreateUserText"] = value; }
		}

		public virtual string CreateUserUrl {
			get { return ViewState.GetString ("CreateUserUrl", String.Empty); }
			set { ViewState ["CreateUserUrl"] = value; }
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[Themeable (false)]
		public virtual string CurrentPassword {
			get { throw new NotImplementedException (); }
		}

		[DefaultValue (false)]
		public virtual bool DisplayUserName {
			get { return ViewState.GetBool ("DisplayUserName", false); }
			set { ViewState ["DisplayUserName"] = value; }
		}

		public virtual string EditProfileIconUrl {
			get { return ViewState.GetString ("EditProfileIconUrl", String.Empty); }
			set { ViewState ["EditProfileIconUrl"] = value; }
		}

		[Localizable (true)]
		public virtual string EditProfileText {
			get { return ViewState.GetString ("EditProfileText", String.Empty); }
			set { ViewState ["EditProfileText"] = value; }
		}

		public virtual string EditProfileUrl {
			get { return ViewState.GetString ("EditProfileUrl", String.Empty); }
			set { ViewState ["EditProfileUrl"] = value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
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

		public virtual string HelpPageIconUrl {
			get { return ViewState.GetString ("HelpPageIconUrl", String.Empty); }
			set { ViewState ["HelpPageIconUrl"] = value; }
		}

		[Localizable (true)]
		public virtual string HelpPageText {
			get { return ViewState.GetString ("HelpPageText", String.Empty); }
			set { ViewState ["HelpPageText"] = value; }
		}

		public virtual string HelpPageUrl {
			get { return ViewState.GetString ("HelpPageUrl", String.Empty); }
			set { ViewState ["HelpPageUrl"] = value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
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

		[Localizable (true)]
		public virtual string InstructionText {
			get { return ViewState.GetString ("InstructionText", String.Empty); }
			set { ViewState ["InstructionText"] = value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
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

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
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

		[MonoTODO]
		[Themeable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public MailDefinition MailDefinition {
			get {
				if (_mailDefinition == null) {
					_mailDefinition = new MailDefinition ();
					if (IsTrackingViewState)
						((IStateManager)_mailDefinition).TrackViewState ();
				}
				return _mailDefinition;
			}
		}

		[Themeable (false)]
		[DefaultValue ("")]
		public virtual string MembershipProvider {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Themeable (false)]
		public virtual string NewPassword {
			get { throw new NotImplementedException (); }
		}

		[Localizable (true)]
		public virtual string NewPasswordLabelText {
			get { return ViewState.GetString ("NewPasswordLabelText", "New Password:"); }
			set { ViewState ["NewPasswordLabelText"] = value; }
		}

		public virtual string NewPasswordRegularExpression {
			get { return ViewState.GetString ("NewPasswordRegularExpression", String.Empty); }
			set { ViewState ["NewPasswordRegularExpression"] = value; }
		}

		public virtual string NewPasswordRegularExpressionErrorMessage {
			get { return ViewState.GetString ("NewPasswordRegularExpressionErrorMessage", String.Empty); }
			set { ViewState ["NewPasswordRegularExpressionErrorMessage"] = value; }
		}

		[Localizable (true)]
		public virtual string NewPasswordRequiredErrorMessage {
			get { return ViewState.GetString ("NewPasswordRequiredErrorMessage", "New Password is required."); }
			set { ViewState ["NewPasswordRequiredErrorMessage"] = value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public TableItemStyle PasswordHintStyle {
			get {
				if (_passwordHintStyle == null) {
					_passwordHintStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						_passwordHintStyle.TrackViewState ();
				}
				return _passwordHintStyle;
			}
		}

		[Localizable (true)]
		public virtual string PasswordHintText {
			get { return ViewState.GetString ("PasswordHintText", String.Empty); }
			set { ViewState ["PasswordHintText"] = value; }
		}

		[Localizable (true)]
		public virtual string PasswordLabelText {
			get { return ViewState.GetString ("PasswordLabelText", "Password:"); }
			set { ViewState ["PasswordLabelText"] = value; }
		}

		public virtual string PasswordRecoveryIconUrl {
			get { return ViewState.GetString ("PasswordRecoveryIconUrl", String.Empty); }
			set { ViewState ["PasswordRecoveryIconUrl"] = value; }
		}

		[Localizable (true)]
		public virtual string PasswordRecoveryText {
			get { return ViewState.GetString ("PasswordRecoveryText", String.Empty); }
			set { ViewState ["PasswordRecoveryText"] = value; }
		}

		public virtual string PasswordRecoveryUrl {
			get { return ViewState.GetString ("PasswordRecoveryUrl", String.Empty); }
			set { ViewState ["PasswordRecoveryUrl"] = value; }
		}

		[Localizable (true)]
		public virtual string PasswordRequiredErrorMessage {
			get { return ViewState.GetString ("PasswordRequiredErrorMessage", String.Empty); }
			set { ViewState ["PasswordRequiredErrorMessage"] = value; }
		}

		[Themeable (false)]
		public virtual string SuccessPageUrl {
			get { return ViewState.GetString ("SuccessPageUrl", String.Empty); }
			set { ViewState ["SuccessPageUrl"] = value; }
		}

		[PersistenceMode (PersistenceMode.InnerProperty)]
		[TemplateContainer (typeof (ChangePassword))]
		[Browsable (false)]
		public virtual ITemplate SuccessTemplate {
			get { return _successTemplate; }
			set { _successTemplate = value; }
		}

		[MonoTODO]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public Control SuccessTemplateContainer {
			get { throw new NotImplementedException (); }
		}

		[Localizable (true)]
		public virtual string SuccessText {
			get { return ViewState.GetString ("SuccessText", String.Empty); }
			set { ViewState ["SuccessText"] = value; }
		}

		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
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

		[Localizable (true)]
		public virtual string SuccessTitleText {
			get { return ViewState.GetString ("SuccessTitleText", "Change Password Complete"); }
			set { ViewState ["SuccessTitleText"] = value; }
		}

		protected override HtmlTextWriterTag TagKey {
			get { return HtmlTextWriterTag.Table; }
		}
		
		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public Style TextBoxStyle {
			get {
				if (_textBoxStyle == null) {
					_textBoxStyle = new Style ();
					if (IsTrackingViewState)
						_textBoxStyle.TrackViewState ();
				}
				return _textBoxStyle;
			}
		}

		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
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

		[DefaultValue ("")]
		public virtual string UserName {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[Localizable (true)]
		public virtual string UserNameLabelText {
			get { return ViewState.GetString ("UserNameLabelText", "User Name:"); }
			set { ViewState ["UserNameLabelText"] = value; }
		}

		[Localizable (true)]
		public virtual string UserNameRequiredErrorMessage {
			get { return ViewState.GetString ("UserNameRequiredErrorMessage", "User Name is required."); }
			set { ViewState ["UserNameRequiredErrorMessage"] = value; }
		}

		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public Style ValidatorTextStyle {
			get {
				if (_validatorTextStyle == null) {
					_validatorTextStyle = new Style ();
					if (IsTrackingViewState)
						_validatorTextStyle.TrackViewState ();
				}
				return _validatorTextStyle;
			}
		}

		public event EventHandler CancelButtonClick;
		public event EventHandler ChangedPassword;
		public event EventHandler ChangePasswordError;
		public event LoginCancelEventHandler ChangingPassword;
		public event EventHandler ContinueButtonClick;
		public event MailMessageEventHandler SendingMail;
		public event SendMailErrorEventHandler SendMailError;

		[MonoTODO]
		protected internal override void CreateChildControls () {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected internal override void LoadControlState (object savedState) {
			throw new NotImplementedException ();
		}

		protected override void LoadViewState (object savedState) {
			
			if (savedState == null)
				return;

			object [] states = (object []) savedState;
			base.LoadViewState (states [0]);
			
			if (states [1] != null)
				_cancelButtonStyle.LoadViewState (states [1]);
			if (states [2] != null)
				_changePasswordButtonStyle.LoadViewState (states [2]);
			if (states [3] != null)
				_continueButtonStyle.LoadViewState (states [3]);

			if (states [4] != null)
				_failureTextStyle.LoadViewState (states [4]);
			if (states [5] != null)
				_hyperLinkStyle.LoadViewState (states [5]);
			if (states [6] != null)
				_instructionTextStyle.LoadViewState (states [6]);

			if (states [7] != null)
				_labelStyle.LoadViewState (states [7]);
			if (states [8] != null)
				_passwordHintStyle.LoadViewState (states [8]);
			if (states [9] != null)
				_successTextStyle.LoadViewState (states [9]);

			if (states [10] != null)
				_textBoxStyle.LoadViewState (states [10]);
			if (states [11] != null)
				_titleTextStyle.LoadViewState (states [11]);
			if (states [12] != null)
				_validatorTextStyle.LoadViewState (states [12]);
			
			if (states [13] != null)
				((IStateManager) _mailDefinition).LoadViewState (states [13]);
		}

		[MonoTODO]
		protected override bool OnBubbleEvent (object source, EventArgs e) {
			throw new NotImplementedException ();
		}

		protected virtual void OnCancelButtonClick (EventArgs e) {
			if (CancelButtonClick != null)
				CancelButtonClick (this, e);
		}

		protected virtual void OnChangedPassword (EventArgs e) {
			if (ChangedPassword != null)
				ChangedPassword (this, e);
		}

		protected virtual void OnChangePasswordError (EventArgs e) {
			if (ChangePasswordError != null)
				ChangePasswordError (this, e);
		}

		protected virtual void OnChangingPassword (LoginCancelEventArgs e) {
			if (ChangingPassword != null)
				ChangingPassword (this, e);
		}

		protected virtual void OnContinueButtonClick (EventArgs e) {
			if (ContinueButtonClick != null)
				ContinueButtonClick (this, e);
		}

		[MonoTODO]
		protected internal override void OnInit (EventArgs e) {
			base.OnInit (e);
		}

		[MonoTODO]
		protected internal override void OnPreRender (EventArgs e) {
			base.OnPreRender (e);
		}

		protected virtual void OnSendingMail (MailMessageEventArgs e) {
			if (SendingMail != null)
				SendingMail (this, e);
		}

		protected virtual void OnSendMailError (SendMailErrorEventArgs e) {
			if (SendMailError != null)
				SendMailError (this, e);
		}

		[MonoTODO]
		protected internal override void Render (HtmlTextWriter writer) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override object SaveControlState () {
			throw new NotImplementedException ();
		}

		protected override object SaveViewState () {
			
			object [] states = new object [14];
			states [0] = base.SaveViewState ();
			
			if (_cancelButtonStyle != null)
				states [1] = _cancelButtonStyle.SaveViewState ();
			if (_changePasswordButtonStyle != null)
				states [2] = _changePasswordButtonStyle.SaveViewState ();
			if (_continueButtonStyle != null)
				states [3] = _continueButtonStyle.SaveViewState ();

			if (_failureTextStyle != null)
				states [4] = _failureTextStyle.SaveViewState ();
			if (_hyperLinkStyle != null)
				states [5] = _hyperLinkStyle.SaveViewState ();
			if (_instructionTextStyle != null)
				states [6] = _instructionTextStyle.SaveViewState ();

			if (_labelStyle != null)
				states [7] = _labelStyle.SaveViewState ();
			if (_passwordHintStyle != null)
				states [8] = _passwordHintStyle.SaveViewState ();
			if (_successTextStyle != null)
				states [9] = _successTextStyle.SaveViewState ();

			if (_textBoxStyle != null)
				states [10] = _textBoxStyle.SaveViewState ();
			if (_titleTextStyle != null)
				states [11] = _titleTextStyle.SaveViewState ();
			if (_validatorTextStyle != null)
				states [12] = _validatorTextStyle.SaveViewState ();

			if (_mailDefinition != null)
				states [13] = ((IStateManager)_mailDefinition).SaveViewState ();

			for (int i = 0; i < states.Length; i++) {
				if (states [i] != null)
					return states;
			}
			return null;
		}

		[MonoTODO]
		protected override void SetDesignModeState (IDictionary data) {
			throw new NotImplementedException ();
		}

		protected override void TrackViewState () {
			
			base.TrackViewState ();

			if (_cancelButtonStyle != null)
				_cancelButtonStyle.TrackViewState ();
			if (_changePasswordButtonStyle != null)
				_changePasswordButtonStyle.TrackViewState ();
			if (_continueButtonStyle != null)
				_continueButtonStyle.TrackViewState ();
			
			if (_failureTextStyle != null)
				_failureTextStyle.TrackViewState ();
			if (_hyperLinkStyle != null)
				_hyperLinkStyle.TrackViewState ();
			if (_instructionTextStyle != null)
				_instructionTextStyle.TrackViewState ();
			
			if (_labelStyle != null)
				_labelStyle.TrackViewState ();
			if (_passwordHintStyle != null)
				_passwordHintStyle.TrackViewState ();
			if (_successTextStyle != null)
				_successTextStyle.TrackViewState ();

			if (_textBoxStyle != null)
				_textBoxStyle.TrackViewState ();
			if (_titleTextStyle != null)
				_titleTextStyle.TrackViewState ();
			if (_validatorTextStyle != null)
				_validatorTextStyle.TrackViewState ();
			
			if (_mailDefinition != null)
				((IStateManager)_mailDefinition).TrackViewState ();
		}
	}
}

#endif