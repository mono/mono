//
// System.Web.UI.WebControls.ChangePassword.cs
//
// Authors:
//	Igor Zelmanovich (igorz@mainsoft.com)
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

using System;
using System.Web.Security;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.ComponentModel;
using System.Collections;
using System.Net.Mail;

namespace System.Web.UI.WebControls
{
	[Bindable (true)]
	[DefaultEvent ("ChangedPassword")]
	[Designer ("System.Web.UI.Design.WebControls.ChangePasswordDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class ChangePassword : CompositeControl, INamingContainer
	{
		static readonly object cancelButtonClickEvent = new object ();
		static readonly object changedPasswordEvent = new object ();
		static readonly object changePasswordErrorEvent = new object ();
		static readonly object changingPasswordEvent = new object ();
		static readonly object continueButtonClickEvent = new object ();
		static readonly object sendingMailEvent = new object ();
		static readonly object sendMailErrorEvent = new object ();
		
		public static readonly string CancelButtonCommandName = "Cancel";
		public static readonly string ChangePasswordButtonCommandName = "ChangePassword";
		public static readonly string ContinueButtonCommandName = "Continue";

		Style _cancelButtonStyle = null;
		Style _changePasswordButtonStyle = null;
		Style _continueButtonStyle = null;
		TableItemStyle _failureTextStyle = null;
		TableItemStyle _hyperLinkStyle = null;
		TableItemStyle _instructionTextStyle = null;
		TableItemStyle _labelStyle = null;
		TableItemStyle _passwordHintStyle = null;
		TableItemStyle _successTextStyle = null;
		Style _textBoxStyle = null;
		TableItemStyle _titleTextStyle = null;
		Style _validatorTextStyle = null;

		MailDefinition _mailDefinition = null;
		MembershipProvider _provider = null;

		ITemplate _changePasswordTemplate = null;
		ITemplate _successTemplate = null;

		Control _changePasswordTemplateContainer = null;
		Control _successTemplateContainer = null;

		string _username = null;
		string _currentPassword = null;
		string _newPassword = null;
		string _newPasswordConfirm = null;

		bool _showContinue = false;

		EventHandlerList events = new EventHandlerList ();
		
#region Public Events
		public event EventHandler CancelButtonClick {
			add { events.AddHandler (cancelButtonClickEvent, value); }
			remove { events.RemoveHandler (cancelButtonClickEvent, value); }
		}
		
		public event EventHandler ChangedPassword {
			add { events.AddHandler (changedPasswordEvent, value); }
			remove { events.RemoveHandler (changedPasswordEvent, value); }
		}
		
		public event EventHandler ChangePasswordError {
			add { events.AddHandler (changePasswordErrorEvent, value); }
			remove { events.RemoveHandler (changePasswordErrorEvent, value); }
		}
		
		public event LoginCancelEventHandler ChangingPassword {
			add { events.AddHandler (changingPasswordEvent, value); }
			remove { events.RemoveHandler (changingPasswordEvent, value); }
		}
		
		public event EventHandler ContinueButtonClick {
			add { events.AddHandler (continueButtonClickEvent, value); }
			remove { events.RemoveHandler (continueButtonClickEvent, value); }
		}
		
		public event MailMessageEventHandler SendingMail {
			add { events.AddHandler (sendingMailEvent, value); }
			remove { events.RemoveHandler (sendingMailEvent, value); }
		}
		
		public event SendMailErrorEventHandler SendMailError {
			add { events.AddHandler (sendMailErrorEvent, value); }
			remove { events.RemoveHandler (sendMailErrorEvent, value); }
		}
#endregion
			
		#region Public Properties

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
		[UrlProperty]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public virtual string CancelButtonImageUrl {
			get { return ViewState.GetString ("CancelButtonImageUrl", String.Empty); }
			set { ViewState ["CancelButtonImageUrl"] = value; }
		}

		[DefaultValue (null)]
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

		[DefaultValue (ButtonType.Button)]
		public virtual ButtonType CancelButtonType {
			get { return ViewState ["CancelButtonType"] == null ? ButtonType.Button : (ButtonType) ViewState ["CancelButtonType"]; }
			set { ViewState ["CancelButtonType"] = value; }
		}

		[Themeable (false)]
		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public virtual string CancelDestinationPageUrl {
			get { return ViewState.GetString ("CancelDestinationPageUrl", String.Empty); }
			set { ViewState ["CancelDestinationPageUrl"] = value; }
		}

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
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

		[DefaultValue (ButtonType.Button)]
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

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public Control ChangePasswordTemplateContainer {
			get {
				if (_changePasswordTemplateContainer == null)
					_changePasswordTemplateContainer = new ChangePasswordContainer (this);
				return _changePasswordTemplateContainer;
			}
		}

		[Localizable (true)]
		public virtual string ChangePasswordTitleText {
			get { return ViewState.GetString ("ChangePasswordTitleText", "Change Your Password"); }
			set { ViewState ["ChangePasswordTitleText"] = value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[Themeable (false)]
		[Filterable (false)]
		public virtual string ConfirmNewPassword {
			get { return _newPasswordConfirm != null ? _newPasswordConfirm : String.Empty; }
		}

		[Localizable (true)]
		public virtual string ConfirmNewPasswordLabelText {
			get { return ViewState.GetString ("ConfirmNewPasswordLabelText", "Confirm New Password:"); }
			set { ViewState ["ConfirmNewPasswordLabelText"] = value; }
		}

		[Localizable (true)]
		public virtual string ConfirmPasswordCompareErrorMessage {
			get { return ViewState.GetString ("ConfirmPasswordCompareErrorMessage", "The Confirm New Password must match the New Password entry."); }
			set { ViewState ["ConfirmPasswordCompareErrorMessage"] = value; }
		}

		[Localizable (true)]
		public virtual string ConfirmPasswordRequiredErrorMessage {
			get { return ViewState.GetString ("ConfirmPasswordRequiredErrorMessage", "Confirm New Password is required."); }
			set { ViewState ["ConfirmPasswordRequiredErrorMessage"] = value; }
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

		[DefaultValue (ButtonType.Button)]
		public virtual ButtonType ContinueButtonType {
			get { return ViewState ["ContinueButtonType"] == null ? ButtonType.Button : (ButtonType) ViewState ["ContinueButtonType"]; }
			set { ViewState ["ContinueButtonType"] = value; }
		}

		[Themeable (false)]
		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public virtual string ContinueDestinationPageUrl {
			get { return ViewState.GetString ("ContinueDestinationPageUrl", String.Empty); }
			set { ViewState ["ContinueDestinationPageUrl"] = value; }
		}

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public virtual string CreateUserIconUrl {
			get { return ViewState.GetString ("CreateUserIconUrl", String.Empty); }
			set { ViewState ["CreateUserIconUrl"] = value; }
		}

		[DefaultValue ("")]
		[Localizable (true)]
		public virtual string CreateUserText {
			get { return ViewState.GetString ("CreateUserText", String.Empty); }
			set { ViewState ["CreateUserText"] = value; }
		}

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public virtual string CreateUserUrl {
			get { return ViewState.GetString ("CreateUserUrl", String.Empty); }
			set { ViewState ["CreateUserUrl"] = value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[Themeable (false)]
		[Filterable (false)]
		public virtual string CurrentPassword {
			get { return _currentPassword != null ? _currentPassword : String.Empty; }
		}

		[DefaultValue (false)]
		public virtual bool DisplayUserName {
			get { return ViewState.GetBool ("DisplayUserName", false); }
			set { ViewState ["DisplayUserName"] = value; }
		}

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public virtual string EditProfileIconUrl {
			get { return ViewState.GetString ("EditProfileIconUrl", String.Empty); }
			set { ViewState ["EditProfileIconUrl"] = value; }
		}

		[Localizable (true)]
		[DefaultValue ("")]
		public virtual string EditProfileText {
			get { return ViewState.GetString ("EditProfileText", String.Empty); }
			set { ViewState ["EditProfileText"] = value; }
		}

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public virtual string EditProfileUrl {
			get { return ViewState.GetString ("EditProfileUrl", String.Empty); }
			set { ViewState ["EditProfileUrl"] = value; }
		}

		[DefaultValue (null)]
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

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public virtual string HelpPageIconUrl {
			get { return ViewState.GetString ("HelpPageIconUrl", String.Empty); }
			set { ViewState ["HelpPageIconUrl"] = value; }
		}

		[DefaultValue ("")]
		[Localizable (true)]
		public virtual string HelpPageText {
			get { return ViewState.GetString ("HelpPageText", String.Empty); }
			set { ViewState ["HelpPageText"] = value; }
		}

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public virtual string HelpPageUrl {
			get { return ViewState.GetString ("HelpPageUrl", String.Empty); }
			set { ViewState ["HelpPageUrl"] = value; }
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
						_hyperLinkStyle.TrackViewState ();
				}
				return _hyperLinkStyle;
			}
		}

		[DefaultValue ("")]
		[Localizable (true)]
		public virtual string InstructionText {
			get { return ViewState.GetString ("InstructionText", String.Empty); }
			set { ViewState ["InstructionText"] = value; }
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
						_instructionTextStyle.TrackViewState ();
				}
				return _instructionTextStyle;
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
						_labelStyle.TrackViewState ();
				}
				return _labelStyle;
			}
		}

		[Themeable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
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

		[Themeable (false)]
		[DefaultValue ("")]
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

		[Filterable (false)]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Themeable (false)]
		public virtual string NewPassword {
			get { return _newPassword != null ? _newPassword : String.Empty; }
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

		[DefaultValue (null)]
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

		[DefaultValue ("")]
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

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public virtual string PasswordRecoveryIconUrl {
			get { return ViewState.GetString ("PasswordRecoveryIconUrl", String.Empty); }
			set { ViewState ["PasswordRecoveryIconUrl"] = value; }
		}

		[DefaultValue ("")]
		[Localizable (true)]
		public virtual string PasswordRecoveryText {
			get { return ViewState.GetString ("PasswordRecoveryText", String.Empty); }
			set { ViewState ["PasswordRecoveryText"] = value; }
		}

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public virtual string PasswordRecoveryUrl {
			get { return ViewState.GetString ("PasswordRecoveryUrl", String.Empty); }
			set { ViewState ["PasswordRecoveryUrl"] = value; }
		}

		[Localizable (true)]
		public virtual string PasswordRequiredErrorMessage {
			get { return ViewState.GetString ("PasswordRequiredErrorMessage", String.Empty); }
			set { ViewState ["PasswordRequiredErrorMessage"] = value; }
		}

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
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

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public Control SuccessTemplateContainer {
			get {
				if (_successTemplateContainer == null)
					_successTemplateContainer = new SuccessContainer (this);
				return _successTemplateContainer;
			}

		}

		[Localizable (true)]
		public virtual string SuccessText {
			get { return ViewState.GetString ("SuccessText", "Your password has been changed!"); }
			set { ViewState ["SuccessText"] = value; }
		}

		[DefaultValue (null)]
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

		[DefaultValue (null)]
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

		[DefaultValue (null)]
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
			get {
				if (_username == null && HttpContext.Current.Request.IsAuthenticated)
					_username = HttpContext.Current.User.Identity.Name;

				return _username != null ? _username : String.Empty;
			}
			set { _username = value; }
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

		[DefaultValue (null)]
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

		#endregion

		#region Protected Methods

		protected internal override void CreateChildControls ()
		{
			Controls.Clear ();

			ITemplate cpTemplate = ChangePasswordTemplate;
			if (cpTemplate == null)
				cpTemplate = new ChangePasswordDeafultTemplate (this);
			((ChangePasswordContainer) ChangePasswordTemplateContainer).InstantiateTemplate (cpTemplate);

			ITemplate sTemplate = SuccessTemplate;
			if (sTemplate == null)
				sTemplate = new SuccessDefaultTemplate (this);
			((SuccessContainer) SuccessTemplateContainer).InstantiateTemplate (sTemplate);

			Controls.AddAt (0, ChangePasswordTemplateContainer);
			Controls.AddAt (1, SuccessTemplateContainer);

			IEditableTextControl editable;

			ChangePasswordContainer container = (ChangePasswordContainer) ChangePasswordTemplateContainer;
			if (DisplayUserName) {
				editable = container.UserNameTextBox;
				if (editable != null)
					editable.TextChanged += new EventHandler (UserName_TextChanged);
			}

			editable = container.CurrentPasswordTextBox;
			if (editable != null)
				editable.TextChanged += new EventHandler (CurrentPassword_TextChanged);

			editable = container.NewPasswordTextBox;
			if (editable != null)
				editable.TextChanged += new EventHandler (NewPassword_TextChanged);

			editable = container.ConfirmNewPasswordTextBox;
			if (editable != null)
				editable.TextChanged += new EventHandler (NewPasswordConfirm_TextChanged);
		}

		protected internal override void Render (HtmlTextWriter writer)
		{
			for (int i = 0; i < Controls.Count; i++)
				if (Controls [i].Visible)
					Controls [i].Render (writer);
		}

		#endregion

		#region Private Methods

		[MonoTODO ("Not implemented")]
		protected override void SetDesignModeState (IDictionary data)
		{
			throw new NotImplementedException ();
		}

		void InitMemberShipProvider ()
		{
			string mp = MembershipProvider;
			_provider = (mp.Length == 0) ? Membership.Provider : Membership.Providers [mp];
			if (_provider == null)
				throw new HttpException (Locale.GetText ("No provider named '{0}' could be found.", mp));
		}

		void ProcessChangePasswordEvent (CommandEventArgs args)
		{
			if (!Page.IsValid)
				return;

			LoginCancelEventArgs loginCancelEventArgs = new LoginCancelEventArgs ();
			OnChangingPassword (loginCancelEventArgs);
			if (loginCancelEventArgs.Cancel)
				return;

			bool res = false;
			try {
				res = MembershipProviderInternal.ChangePassword (UserName, CurrentPassword, NewPassword);
			} catch {
			}
			
			if (res) {
				OnChangedPassword (args);
				_showContinue = true;

				if (_mailDefinition != null)
					SendMail (UserName, NewPassword);
			} else {
				OnChangePasswordError (EventArgs.Empty);
				string lastError = string.Format (
					"Password incorrect or New Password invalid. New Password length minimum: {0}. Non-alphanumeric characters required: {1}.",
					MembershipProviderInternal.MinRequiredPasswordLength,
					MembershipProviderInternal.MinRequiredNonAlphanumericCharacters);

				ChangePasswordContainer container = (ChangePasswordContainer) ChangePasswordTemplateContainer;
				container.FailureTextLiteral.Text = lastError;
				_showContinue = false;
			}

			return;
		}
		
		void ProcessCancelEvent (CommandEventArgs args)
		{
			OnCancelButtonClick (args);

			if (ContinueDestinationPageUrl.Length > 0)
				Context.Response.Redirect (ContinueDestinationPageUrl);

			return;
		}

		void ProcessContinueEvent (CommandEventArgs args)
		{
			OnContinueButtonClick (args);

			if (ContinueDestinationPageUrl.Length > 0)
				Context.Response.Redirect (ContinueDestinationPageUrl);

			return;
		}

		void SendMail (string username, string password)
		{
			MembershipUser user = MembershipProviderInternal.GetUser (UserName, false);
			if (user == null)
				return;

			ListDictionary dictionary = new ListDictionary ();
			dictionary.Add ("<%USERNAME%>", username);
			dictionary.Add ("<%PASSWORD%>", password);

			MailMessage message = MailDefinition.CreateMailMessage (user.Email, dictionary, this);

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

		#endregion

		#region Properties

		protected override HtmlTextWriterTag TagKey {
			get { return HtmlTextWriterTag.Table; }
		}

		internal virtual MembershipProvider MembershipProviderInternal {
			get {
				if (_provider == null)
					InitMemberShipProvider ();

				return _provider;
			}
		}

		#endregion

		#region View and Control State

		protected internal override void LoadControlState (object savedState)
		{
			if (savedState == null)
				return;
			object [] state = (object []) savedState;
			base.LoadControlState (state [0]);

			_showContinue = (bool) state [1];
			_username = (string) state [2];
		}

		protected internal override object SaveControlState ()
		{
			object state = base.SaveControlState ();
			return new object [] { state, _showContinue, _username };
		}

		protected override void LoadViewState (object savedState)
		{
			if (savedState == null)
				return;

			object [] states = (object []) savedState;
			base.LoadViewState (states [0]);

			if (states [1] != null)
				CancelButtonStyle.LoadViewState (states [1]);
			if (states [2] != null)
				ChangePasswordButtonStyle.LoadViewState (states [2]);
			if (states [3] != null)
				ContinueButtonStyle.LoadViewState (states [3]);

			if (states [4] != null)
				FailureTextStyle.LoadViewState (states [4]);
			if (states [5] != null)
				HyperLinkStyle.LoadViewState (states [5]);
			if (states [6] != null)
				InstructionTextStyle.LoadViewState (states [6]);

			if (states [7] != null)
				LabelStyle.LoadViewState (states [7]);
			if (states [8] != null)
				PasswordHintStyle.LoadViewState (states [8]);
			if (states [9] != null)
				SuccessTextStyle.LoadViewState (states [9]);

			if (states [10] != null)
				TextBoxStyle.LoadViewState (states [10]);
			if (states [11] != null)
				TitleTextStyle.LoadViewState (states [11]);
			if (states [12] != null)
				ValidatorTextStyle.LoadViewState (states [12]);

			if (states [13] != null)
				((IStateManager) MailDefinition).LoadViewState (states [13]);
		}

		protected override object SaveViewState ()
		{
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
				states [13] = ((IStateManager) _mailDefinition).SaveViewState ();

			for (int i = 0; i < states.Length; i++) {
				if (states [i] != null)
					return states;
			}
			return null;
		}

		protected override void TrackViewState ()
		{
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
				((IStateManager) _mailDefinition).TrackViewState ();
		}

		#endregion

		#region Event Handlers

		protected override bool OnBubbleEvent (object source, EventArgs e)
		{
			CommandEventArgs args = e as CommandEventArgs;
			if (e != null) {
				if (args.CommandName == ChangePasswordButtonCommandName) {
					ProcessChangePasswordEvent (args);
					return true;
				}

				if (args.CommandName == CancelButtonCommandName) {
					ProcessCancelEvent (args);
					return true;
				}

				if (args.CommandName == ContinueButtonCommandName) {
					ProcessContinueEvent (args);
					return true;
				}
			}
			return base.OnBubbleEvent (source, e);
		}

		protected virtual void OnCancelButtonClick (EventArgs e)
		{
			EventHandler eh = events [cancelButtonClickEvent] as EventHandler;
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnChangedPassword (EventArgs e)
		{
			EventHandler eh = events [changedPasswordEvent] as EventHandler;
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnChangePasswordError (EventArgs e)
		{
			EventHandler eh = events [changePasswordErrorEvent] as EventHandler;
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnChangingPassword (LoginCancelEventArgs e)
		{
			LoginCancelEventHandler eh = events [changingPasswordEvent] as LoginCancelEventHandler;
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnContinueButtonClick (EventArgs e)
		{
			EventHandler eh = events [continueButtonClickEvent] as EventHandler;
			if (eh != null)
				eh (this, e);
		}

		protected internal override void OnInit (EventArgs e)
		{
			Page.RegisterRequiresControlState (this);
			base.OnInit (e);
		}

		protected internal override void OnPreRender (EventArgs e)
		{
			ChangePasswordTemplateContainer.Visible = !_showContinue;
			SuccessTemplateContainer.Visible = _showContinue;
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
			SendMailErrorEventHandler eh = events [sendMailErrorEvent] as SendMailErrorEventHandler;
			if (eh != null)
				eh (this, e);
		}

		void UserName_TextChanged (object sender, EventArgs e)
		{
			UserName = ((ITextControl) sender).Text;
		}

		void CurrentPassword_TextChanged (object sender, EventArgs e)
		{
			_currentPassword = ((ITextControl) sender).Text;
		}

		void NewPassword_TextChanged (object sender, EventArgs e)
		{
			_newPassword = ((ITextControl) sender).Text;
		}

		void NewPasswordConfirm_TextChanged (object sender, EventArgs e)
		{
			_newPasswordConfirm = ((ITextControl) sender).Text;
		}

		#endregion

		class BaseChangePasswordContainer : Table, INamingContainer, INonBindingContainer
		{
			protected readonly ChangePassword _owner = null;
			TableCell _containerCell = null;

			public BaseChangePasswordContainer (ChangePassword owner)
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

			protected override void EnsureChildControls ()
			{
				base.EnsureChildControls ();

				// it's the owner who adds controls, not us
				if (_owner != null)
					_owner.EnsureChildControls ();
			}
		}

		sealed class ChangePasswordContainer : BaseChangePasswordContainer
		{
			public ChangePasswordContainer (ChangePassword owner) : base (owner)
			{
				ID = "ChangePasswordContainerID";
			}

			// Requried controls
			public IEditableTextControl UserNameTextBox {
				get {
					Control c = FindControl ("UserName");
					if (c == null)
						throw new HttpException ("ChangePasswordTemplate does not contain an IEditableTextControl with ID UserName for the username, this is required if DisplayUserName=true.");
					return c as IEditableTextControl;
				}
			}
			
			public IEditableTextControl CurrentPasswordTextBox {
				get {
					Control c = FindControl ("CurrentPassword");
					if (c == null)
						throw new HttpException ("ChangePasswordTemplate does not contain an IEditableTextControl with ID CurrentPassword for the current password.");
					return c as IEditableTextControl;
				}
			}
			
			public IEditableTextControl NewPasswordTextBox {
				get {
					Control c = FindControl ("NewPassword");
					if (c == null)
						throw new HttpException ("ChangePasswordTemplate does not contain an IEditableTextControl with ID NewPassword for the new password.");
					return c as IEditableTextControl;
				}
			}

			// Optional controls
			public IEditableTextControl ConfirmNewPasswordTextBox {
				get { return FindControl ("ConfirmNewPassword") as IEditableTextControl; }
			}
			
			public Control CancelButton {
				get { return FindControl ("Cancel"); }
			}
			
			public Control ChangePasswordButton {
				get { return FindControl ("ChangePassword"); }
			}
			
			public ITextControl FailureTextLiteral {
				get { return FindControl ("FailureText") as ITextControl; }
			}
		}

		sealed class ChangePasswordDeafultTemplate : ITemplate
		{
			readonly ChangePassword _owner = null;

			internal ChangePasswordDeafultTemplate (ChangePassword cPassword)
			{
				_owner = cPassword;
			}

			TableRow CreateRow (Control c0, Control c1, Control c2, Style s0, Style s1)
			{
				TableRow row = new TableRow ();
				TableCell cell0 = new TableCell ();
				TableCell cell1 = new TableCell ();

				cell0.Controls.Add (c0);
				row.Controls.Add (cell0);

				if ((c1 != null) && (c2 != null)) {
					cell1.Controls.Add (c1);
					cell1.Controls.Add (c2);
					cell0.HorizontalAlign = HorizontalAlign.Right;

					if (s0 != null)
						cell0.ApplyStyle (s0);
					if (s1 != null)
						cell1.ApplyStyle (s1);

					row.Controls.Add (cell1);
				} else {
					cell0.ColumnSpan = 2;
					cell0.HorizontalAlign = HorizontalAlign.Center;
					if (s0 != null)
						cell0.ApplyStyle (s0);
				}
				return row;
			}

			bool AddLink (string pageUrl, string linkText, string linkIcon, WebControl container)
			{
				bool added = false;
				if (linkIcon.Length > 0) {
					Image img = new Image ();
					img.ImageUrl = linkIcon;
					container.Controls.Add (img);
					added = true;
				}
				if (linkText.Length > 0) {
					HyperLink link = new HyperLink ();
					link.NavigateUrl = pageUrl;
					link.Text = linkText;
					link.ControlStyle.CopyTextStylesFrom (container.ControlStyle);
					container.Controls.Add (link);
					added = true;
				}
				return added;
			}

			public void InstantiateIn (Control container)
			{
				Table table = new Table ();
				table.CellPadding = 0;

				// Row #0
				table.Controls.Add (
					CreateRow (new LiteralControl (_owner.ChangePasswordTitleText),
					null, null, _owner.TitleTextStyle, null));

				// Row #1
				if (_owner.InstructionText.Length > 0) {
					table.Controls.Add (
						CreateRow (new LiteralControl (_owner.InstructionText),
						null, null, _owner.InstructionTextStyle, null));
				}

				// Row #2
				if (_owner.DisplayUserName) {
					TextBox UserName = new TextBox ();
					UserName.ID = "UserName";
					UserName.Text = _owner.UserName;
					UserName.ApplyStyle (_owner.TextBoxStyle);

					Label UserNameLabel = new Label ();
					UserNameLabel.ID = "UserNameLabel";
					UserNameLabel.AssociatedControlID = "UserName";
					UserNameLabel.Text = _owner.UserNameLabelText;

					RequiredFieldValidator UserNameRequired = new RequiredFieldValidator ();
					UserNameRequired.ID = "UserNameRequired";
					UserNameRequired.ControlToValidate = "UserName";
					UserNameRequired.ErrorMessage = _owner.UserNameRequiredErrorMessage;
					UserNameRequired.ToolTip = _owner.UserNameRequiredErrorMessage;
					UserNameRequired.Text = "*";
					UserNameRequired.ValidationGroup = _owner.ID;
					UserNameRequired.ApplyStyle (_owner.ValidatorTextStyle);

					table.Controls.Add (CreateRow (UserNameLabel, UserName, UserNameRequired, _owner.LabelStyle, null));
				}

				// Row #3
				TextBox CurrentPassword = new TextBox ();
				CurrentPassword.ID = "CurrentPassword";
				CurrentPassword.TextMode = TextBoxMode.Password;
				CurrentPassword.ApplyStyle (_owner.TextBoxStyle);

				Label CurrentPasswordLabel = new Label ();
				CurrentPasswordLabel.ID = "CurrentPasswordLabel";
				CurrentPasswordLabel.AssociatedControlID = "CurrentPasswordLabel";
				CurrentPasswordLabel.Text = _owner.PasswordLabelText;

				RequiredFieldValidator CurrentPasswordRequired = new RequiredFieldValidator ();
				CurrentPasswordRequired.ID = "CurrentPasswordRequired";
				CurrentPasswordRequired.ControlToValidate = "CurrentPassword";
				CurrentPasswordRequired.ErrorMessage = _owner.PasswordRequiredErrorMessage;
				CurrentPasswordRequired.ToolTip = _owner.PasswordRequiredErrorMessage;
				CurrentPasswordRequired.Text = "*";
				CurrentPasswordRequired.ValidationGroup = _owner.ID;
				CurrentPasswordRequired.ApplyStyle (_owner.ValidatorTextStyle);

				table.Controls.Add (CreateRow (CurrentPasswordLabel, CurrentPassword, CurrentPasswordRequired, _owner.LabelStyle, null));

				// Row #4
				TextBox NewPassword = new TextBox ();
				NewPassword.ID = "NewPassword";
				NewPassword.TextMode = TextBoxMode.Password;
				NewPassword.ApplyStyle (_owner.TextBoxStyle);

				Label NewPasswordLabel = new Label ();
				NewPasswordLabel.ID = "NewPasswordLabel";
				NewPasswordLabel.AssociatedControlID = "NewPassword";
				NewPasswordLabel.Text = _owner.NewPasswordLabelText;

				RequiredFieldValidator NewPasswordRequired = new RequiredFieldValidator ();
				NewPasswordRequired.ID = "NewPasswordRequired";
				NewPasswordRequired.ControlToValidate = "NewPassword";
				NewPasswordRequired.ErrorMessage = _owner.PasswordRequiredErrorMessage;
				NewPasswordRequired.ToolTip = _owner.PasswordRequiredErrorMessage;
				NewPasswordRequired.Text = "*";
				NewPasswordRequired.ValidationGroup = _owner.ID;
				NewPasswordRequired.ApplyStyle (_owner.ValidatorTextStyle);

				table.Controls.Add (CreateRow (NewPasswordLabel, NewPassword, NewPasswordRequired, _owner.LabelStyle, null));

				// Row #5
				if (_owner.PasswordHintText.Length > 0) {
					table.Controls.Add (
						CreateRow (new LiteralControl (String.Empty),
							new LiteralControl (_owner.PasswordHintText),
							new LiteralControl (String.Empty),
							null, _owner.PasswordHintStyle));
				}

				// Row #6
				TextBox ConfirmNewPassword = new TextBox ();
				ConfirmNewPassword.ID = "ConfirmNewPassword";
				ConfirmNewPassword.TextMode = TextBoxMode.Password;
				ConfirmNewPassword.ApplyStyle (_owner.TextBoxStyle);

				Label ConfirmNewPasswordLabel = new Label ();
				ConfirmNewPasswordLabel.ID = "ConfirmNewPasswordLabel";
				ConfirmNewPasswordLabel.AssociatedControlID = "ConfirmNewPasswordLabel";
				ConfirmNewPasswordLabel.Text = _owner.ConfirmNewPasswordLabelText;

				RequiredFieldValidator ConfirmNewPasswordRequired = new RequiredFieldValidator ();
				ConfirmNewPasswordRequired.ID = "ConfirmNewPasswordRequired";
				ConfirmNewPasswordRequired.ControlToValidate = "ConfirmNewPassword";
				ConfirmNewPasswordRequired.ErrorMessage = _owner.PasswordRequiredErrorMessage;
				ConfirmNewPasswordRequired.ToolTip = _owner.PasswordRequiredErrorMessage;
				ConfirmNewPasswordRequired.Text = "*";
				ConfirmNewPasswordRequired.ValidationGroup = _owner.ID;
				ConfirmNewPasswordRequired.ApplyStyle (_owner.ValidatorTextStyle);

				table.Controls.Add (CreateRow (ConfirmNewPasswordLabel, ConfirmNewPassword, ConfirmNewPasswordRequired, _owner.LabelStyle, null));

				// Row #7
				CompareValidator NewPasswordCompare = new CompareValidator ();
				NewPasswordCompare.ID = "NewPasswordCompare";
				NewPasswordCompare.ControlToCompare = "NewPassword";
				NewPasswordCompare.ControlToValidate = "ConfirmNewPassword";
				NewPasswordCompare.Display = ValidatorDisplay.Dynamic;
				NewPasswordCompare.ErrorMessage = _owner.ConfirmPasswordCompareErrorMessage;
				NewPasswordCompare.ValidationGroup = _owner.ID;

				table.Controls.Add (CreateRow (NewPasswordCompare, null, null, null, null));

				// Row #8
				Literal FailureTextLiteral = new Literal ();
				FailureTextLiteral.ID = "FailureText";
				FailureTextLiteral.EnableViewState = false;

				if (_owner.FailureTextStyle.ForeColor.IsEmpty)
					_owner.FailureTextStyle.ForeColor = System.Drawing.Color.Red;

				table.Controls.Add (CreateRow (FailureTextLiteral, null, null, _owner.FailureTextStyle, null));

				// Row #9
				WebControl ChangePasswordButton = null;
				switch (_owner.ChangePasswordButtonType) {
					case ButtonType.Button:
						ChangePasswordButton = new Button ();
						break;
					case ButtonType.Image:
						ChangePasswordButton = new ImageButton ();
						break;
					case ButtonType.Link:
						ChangePasswordButton = new LinkButton ();
						break;
				}

				ChangePasswordButton.ID = "ChangePasswordPushButton";
				ChangePasswordButton.ApplyStyle (_owner.ChangePasswordButtonStyle);
				((IButtonControl) ChangePasswordButton).CommandName = ChangePassword.ChangePasswordButtonCommandName;
				((IButtonControl) ChangePasswordButton).Text = _owner.ChangePasswordButtonText;
				((IButtonControl) ChangePasswordButton).ValidationGroup = _owner.ID;

				WebControl CancelButton = null;
				switch (_owner.CancelButtonType) {
					case ButtonType.Button:
						CancelButton = new Button ();
						break;
					case ButtonType.Image:
						CancelButton = new ImageButton ();
						break;
					case ButtonType.Link:
						CancelButton = new LinkButton ();
						break;
				}

				CancelButton.ID = "CancelPushButton";
				CancelButton.ApplyStyle (_owner.CancelButtonStyle);
				((IButtonControl) CancelButton).CommandName = ChangePassword.CancelButtonCommandName;
				((IButtonControl) CancelButton).Text = _owner.CancelButtonText;
				((IButtonControl) CancelButton).CausesValidation = false;

				table.Controls.Add (CreateRow (ChangePasswordButton, CancelButton, new LiteralControl (String.Empty), null, null));

				// Row #10
				TableRow linksRow = new TableRow ();
				TableCell linksCell = new TableCell ();
				linksCell.ColumnSpan = 2;
				linksCell.ControlStyle.CopyFrom (_owner.HyperLinkStyle);

				linksRow.Cells.Add (linksCell);

				if (AddLink (_owner.HelpPageUrl, _owner.HelpPageText, _owner.HelpPageIconUrl, linksCell))
					linksCell.Controls.Add (new LiteralControl ("<br/>"));

				if (AddLink (_owner.CreateUserUrl, _owner.CreateUserText, _owner.CreateUserIconUrl, linksCell))
					linksCell.Controls.Add (new LiteralControl ("<br/>"));

				if (AddLink (_owner.PasswordRecoveryUrl, _owner.PasswordRecoveryText, _owner.PasswordRecoveryIconUrl, linksCell))
					linksCell.Controls.Add (new LiteralControl ("<br/>"));

				AddLink (_owner.EditProfileUrl, _owner.EditProfileText, _owner.EditProfileIconUrl, linksCell);

				table.Controls.Add (linksRow);

				container.Controls.Add (table);
			}
		}

		sealed class SuccessDefaultTemplate : ITemplate
		{
			readonly ChangePassword _cPassword = null;

			internal SuccessDefaultTemplate (ChangePassword cPassword)
			{
				_cPassword = cPassword;
			}

			TableRow CreateRow (Control c0, Style s0, HorizontalAlign align)
			{
				TableRow row = new TableRow ();
				TableCell cell0 = new TableCell ();

				cell0.Controls.Add (c0);
				cell0.HorizontalAlign = align;
				if (s0 != null)
					cell0.ApplyStyle (s0);

				row.Controls.Add (cell0);
				return row;
			}

			public void InstantiateIn (Control container)
			{
				Table table = new Table ();
				table.ControlStyle.Width = Unit.Percentage (100);
				table.ControlStyle.Height = Unit.Percentage (100);

				// Row #0
				table.Controls.Add (
					CreateRow (new LiteralControl (_cPassword.SuccessTitleText), _cPassword.TitleTextStyle, HorizontalAlign.Center));

				// Row #1
				table.Controls.Add (
					CreateRow (new LiteralControl (_cPassword.SuccessText), _cPassword.SuccessTextStyle, HorizontalAlign.Center));

				// Row #3
				WebControl ContinueButton = null;
				switch (_cPassword.ChangePasswordButtonType) {
					case ButtonType.Button:
						ContinueButton = new Button ();
						break;
					case ButtonType.Image:
						ContinueButton = new ImageButton ();
						break;
					case ButtonType.Link:
						ContinueButton = new LinkButton ();
						break;
				}

				ContinueButton.ID = "ContinuePushButton";
				ContinueButton.ApplyStyle (_cPassword.ContinueButtonStyle);
				((IButtonControl) ContinueButton).CommandName = ChangePassword.ContinueButtonCommandName;
				((IButtonControl) ContinueButton).Text = _cPassword.ContinueButtonText;
				((IButtonControl) ContinueButton).CausesValidation = false;

				table.Controls.Add (
					CreateRow (ContinueButton, null, HorizontalAlign.Right));

				container.Controls.Add (table);
			}
		}

		sealed class SuccessContainer : BaseChangePasswordContainer
		{
			public SuccessContainer (ChangePassword owner) : base (owner)
			{
				ID = "SuccessContainerID";
			}

			public Control ChangePasswordButton
			{
				get { return FindControl ("Continue"); }
			}
		}
	}
}

