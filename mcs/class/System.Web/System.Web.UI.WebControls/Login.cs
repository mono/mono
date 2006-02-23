//
// System.Web.UI.WebControls.Login class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.Globalization;
using System.ComponentModel;
using System.Security.Permissions;
using System.Web.Security;

namespace System.Web.UI.WebControls {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[Bindable (false)]
	[DefaultEvent ("Authenticate")]
	[Designer ("System.Web.UI.Design.WebControls.LoginDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class Login : CompositeControl {

		public static readonly string LoginButtonCommandName = "Login";

		private static readonly object authenticateEvent = new object ();
		private static readonly object loggedInEvent = new object ();
		private static readonly object loggingInEvent = new object ();
		private static readonly object loginErrorEvent = new object ();

		private TableItemStyle checkBoxStyle;
		private TableItemStyle failureTextStyle;
		private TableItemStyle hyperLinkStyle;
		private TableItemStyle instructionTextStyle;
		private TableItemStyle labelStyle;
		private Style logonButtonStyle;
		private Style textBoxStyle;
		private TableItemStyle titleTextStyle;
		private Style validatorTextStyle;

		private ITemplate layoutTemplate;

		private TextBox userNameTextBox;
		private RequiredFieldValidator userNameRequired;
		private TextBox passwordTextBox;
		private RequiredFieldValidator passwordRequired;
		private CheckBox rememberMeCheckBox;
		private WebControl loginButton;


		public Login ()
		{
		}


		[DefaultValue (1)]
		public virtual int BorderPadding {
			get {
				object o = ViewState ["BorderPadding"];
				return (o == null) ? 1 : (int) o;
			}
			set {
				if (value < -1)
					throw new ArgumentOutOfRangeException ("BorderPadding", "< -1");
				else
					ViewState ["BorderPadding"] = value;
			}
		}

		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public TableItemStyle CheckBoxStyle {
			get {
				if (checkBoxStyle == null) {
					checkBoxStyle = new TableItemStyle ();
					if (IsTrackingViewState) {
						(checkBoxStyle as IStateManager).TrackViewState ();
					}
				}
				return checkBoxStyle;
			}
		}

		[DefaultValue ("")]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[UrlProperty]
		public virtual string CreateUserIconUrl {
			get {
				object o = ViewState ["CreateUserIconUrl"];
				return (o == null) ? String.Empty : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("CreateUserIconUrl");
				else
					ViewState ["CreateUserIconUrl"] = value;
			}
		}

		[DefaultValue ("")]
		[Localizable (true)]
		public virtual string CreateUserText {
			get {
				object o = ViewState ["CreateUserText"];
				return (o == null) ? String.Empty : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("CreateUserText");
				else
					ViewState ["CreateUserText"] = value;
			}
		}

		[DefaultValue ("")]
		[Editor ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[UrlProperty]
		public virtual string CreateUserUrl {
			get {
				object o = ViewState ["CreateUserUrl"];
				return (o == null) ? String.Empty : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("CreateUserUrl");
				else
					ViewState ["CreateUserUrl"] = value;
			}
		}

		[DefaultValue ("")]
		[Editor ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[Themeable (false)]
		[UrlProperty]
		public virtual string DestinationPageUrl {
			get {
				object o = ViewState ["DestinationPageUrl"];
				return (o == null) ? String.Empty : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("DestinationPageUrl");
				else
					ViewState ["DestinationPageUrl"] = value;
			}
		}

		[DefaultValue (true)]
		[Themeable (false)]
		public virtual bool DisplayRememberMe {
			get {
				object o = ViewState ["DisplayRememberMe"];
				return (o == null) ? true : (bool) o;
			}
			set {
				ViewState ["DisplayRememberMe"] = value;
			}
		}

		[DefaultValue (LoginFailureAction.Refresh)]
		[Themeable (false)]
		[MonoTODO ("RedirectToLoginPage not yet implemented in FormsAuthentication")]
		public virtual LoginFailureAction FailureAction {
			get {
				object o = ViewState ["FailureAction"];
				return (o == null) ? LoginFailureAction.Refresh : (LoginFailureAction) o;
			}
			set {
				if ((value < LoginFailureAction.Refresh) || (value > LoginFailureAction.RedirectToLoginPage))
					throw new ArgumentOutOfRangeException ("FailureAction");
				ViewState ["FailureAction"] = (int) value;
			}
		}

		[Localizable (true)]
		public virtual string FailureText {
			get {
				object o = ViewState ["FailureText"];
				return (o == null) ? Locale.GetText ("Your login attempt was not successful. Please try again.") : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("FailureText");
				else
					ViewState ["FailureText"] = value;
			}
		}

		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public TableItemStyle FailureTextStyle {
			get {
				if (failureTextStyle == null) {
					failureTextStyle = new TableItemStyle ();
					if (IsTrackingViewState) {
						(failureTextStyle as IStateManager).TrackViewState ();
					}
				}
				return failureTextStyle;
			}
		}

		[DefaultValue ("")]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[UrlProperty]
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
		[Localizable (true)]
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
		[Editor ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[UrlProperty]
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
				if (hyperLinkStyle == null) {
					hyperLinkStyle = new TableItemStyle ();
					if (IsTrackingViewState) {
						(hyperLinkStyle  as IStateManager).TrackViewState ();
					}
				}
				return hyperLinkStyle;
			}
		}

		[DefaultValue ("")]
		[Localizable (true)]
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
				if (instructionTextStyle == null) {
					instructionTextStyle = new TableItemStyle ();
					if (IsTrackingViewState) {
						(instructionTextStyle as IStateManager).TrackViewState ();
					}
				}
				return instructionTextStyle;
			}
		}

		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public TableItemStyle LabelStyle {
			get {
				if (labelStyle == null) {
					labelStyle = new TableItemStyle ();
					if (IsTrackingViewState) {
						(labelStyle as IStateManager).TrackViewState ();
					}
				}
				return labelStyle;
			}
		}

		[Browsable (false)]
		[TemplateContainer (typeof (Login))]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public virtual ITemplate LayoutTemplate {
			get { return layoutTemplate; }
			set { layoutTemplate = value; }
		}

		[DefaultValue ("")]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[UrlProperty]
		public virtual string LoginButtonImageUrl {
			get {
				object o = ViewState ["LoginButtonImageUrl"];
				return (o == null) ? String.Empty : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("LoginButtonImageUrl");
				else
					ViewState ["LoginButtonImageUrl"] = value;
			}
		}

		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public Style LoginButtonStyle {
			get {
				if (logonButtonStyle == null) {
					logonButtonStyle = new Style ();
					if (IsTrackingViewState) {
						(logonButtonStyle as IStateManager).TrackViewState ();
					}
				}
				return logonButtonStyle;
			}
		}

		[Localizable (true)]
		public virtual string LoginButtonText {
			get {
				object o = ViewState ["LoginButtonText"];
				return (o == null) ? Locale.GetText ("Log In") : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("LoginButtonText");
				else
					ViewState ["LoginButtonText"] = value;
			}
		}

		[DefaultValue (ButtonType.Button)]
		public virtual ButtonType LoginButtonType {
			get {
				object o = ViewState ["LoginButtonType"];
				return (o == null) ? ButtonType.Button : (ButtonType) o;
			}
			set {
				if ((value < ButtonType.Button) || (value > ButtonType.Link))
					throw new ArgumentOutOfRangeException ("LoginButtonType");
				ViewState ["LoginButtonType"] = (int) value;
			}
		}

		[DefaultValue ("")]
		[Themeable (false)]
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
			}
		}

		[DefaultValue (Orientation.Vertical)]
		public virtual Orientation Orientation {
			get {
				object o = ViewState ["Orientation"];
				return (o == null) ? Orientation.Vertical : (Orientation) o;
			}
			set {
				if ((value < Orientation.Horizontal) || (value > Orientation.Vertical))
					throw new ArgumentOutOfRangeException ("Orientation");
				ViewState ["Orientation"] = (int) value;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual string Password {
			get {
				object o = ViewState ["Password"];
				return (o == null) ? String.Empty : (string) o;
			}
		}

		[Localizable (true)]
		public virtual string PasswordLabelText {
			get {
				object o = ViewState ["PasswordLabelText"];
				return (o == null) ? "Password:" : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("PasswordLabelText");
				else
					ViewState ["PasswordLabelText"] = value;
			}
		}

		[DefaultValue ("")]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[UrlProperty]
		public virtual string PasswordRecoveryIconUrl {
			get {
				object o = ViewState ["PasswordRecoveryIconUrl"];
				return (o == null) ? String.Empty : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("PasswordRecoveryIconUrl");
				else
					ViewState ["PasswordRecoveryIconUrl"] = value;
			}
		}

		[DefaultValue ("")]
		[Localizable (true)]
		public virtual string PasswordRecoveryText {
			get {
				object o = ViewState ["PasswordRecoveryText"];
				return (o == null) ? String.Empty : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("PasswordRecoveryText");
				else
					ViewState ["PasswordRecoveryText"] = value;
			}
		}

		[DefaultValue ("")]
		[Editor ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[UrlProperty]
		public virtual string PasswordRecoveryUrl {
			get {
				object o = ViewState ["PasswordRecoveryUrl"];
				return (o == null) ? String.Empty : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("PasswordRecoveryUrl");
				else
					ViewState ["PasswordRecoveryUrl"] = value;
			}
		}

		[Localizable (true)]
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

		[DefaultValue (false)]
		[Themeable (false)]
		public virtual bool RememberMeSet {
			get {
				object o = ViewState ["RememberMeSet"];
				return (o == null) ? false : (bool) o;
			}
			set {
				ViewState ["RememberMeSet"] = value;
			}
		}

		[Localizable (true)]
		public virtual string RememberMeText {
			get {
				object o = ViewState ["RememberMeText"];
				return (o == null) ? Locale.GetText ("Remember me next time.") : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("RememberMeText");
				else
					ViewState ["RememberMeText"] = value;
			}
		}

		protected override HtmlTextWriterTag TagKey {
			get { return HtmlTextWriterTag.Table; }
		}

		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public Style TextBoxStyle {
			get {
				if (textBoxStyle == null) {
					textBoxStyle = new Style ();
					if (IsTrackingViewState) {
						(textBoxStyle as IStateManager).TrackViewState ();
					}
				}
				return textBoxStyle;
			}
		}

		[DefaultValue (LoginTextLayout.TextOnLeft)]
		public virtual LoginTextLayout TextLayout {
			get {
				object o = ViewState ["TextLayout"];
				return (o == null) ? LoginTextLayout.TextOnLeft : (LoginTextLayout) o;
			}
			set {
				if ((value < LoginTextLayout.TextOnLeft) || (value > LoginTextLayout.TextOnTop))
					throw new ArgumentOutOfRangeException ("TextLayout");
				ViewState ["TextLayout"] = (int) value;
			}
		}

		[Localizable (true)]
		public virtual string TitleText {
			get {
				object o = ViewState ["TitleText"];
				return (o == null) ? Locale.GetText ("Log In") : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("TitleText");
				else
					ViewState ["TitleText"] = value;
			}
		}

		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public TableItemStyle TitleTextStyle {
			get {
				if (titleTextStyle == null) {
					titleTextStyle = new TableItemStyle ();
					if (IsTrackingViewState) {
						(titleTextStyle as IStateManager).TrackViewState ();
					}
				}
				return titleTextStyle;
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

		[Localizable (true)]
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

		[Localizable (true)]
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
				if (validatorTextStyle == null) {
					validatorTextStyle = new Style ();
					if (IsTrackingViewState) {
						(validatorTextStyle as IStateManager).TrackViewState ();
					}
				}
				return validatorTextStyle;
			}
		}

		[DefaultValue (true)]
		[Themeable (false)]
		public virtual bool VisibleWhenLoggedIn {
			get {
				object o = ViewState ["VisibleWhenLoggedIn"];
				return (o == null) ? true : (bool) o;
			}
			set {
				ViewState ["VisibleWhenLoggedIn"] = value;
			}
		}


		// methods

		protected internal override void CreateChildControls ()
		{
			userNameTextBox = new TextBox ();
			userNameTextBox.ID = "UserName";

			userNameRequired = new RequiredFieldValidator ();
			userNameRequired.ID = "UserNameRequired";
			userNameRequired.ControlToValidate = userNameTextBox.ID;
			userNameRequired.ErrorMessage = "*";
			userNameRequired.ValidationGroup = this.UniqueID;

			passwordTextBox = new TextBox ();
			passwordTextBox.ID = "Password";
			passwordTextBox.TextMode = TextBoxMode.Password;

			passwordRequired = new RequiredFieldValidator ();
			passwordRequired.ID = "PasswordRequired";
			passwordRequired.ControlToValidate = passwordTextBox.ID;
			passwordRequired.ErrorMessage = "*";
			passwordRequired.ValidationGroup = this.UniqueID;

			rememberMeCheckBox = new CheckBox ();
			rememberMeCheckBox.ID = "RememberMe";
			rememberMeCheckBox.Checked = RememberMeSet;

			switch (LoginButtonType) {
			case ButtonType.Button:
				loginButton = new Button ();
				loginButton.ID = "LoginButton";
				break;
			case ButtonType.Link:
				loginButton = new LinkButton ();
				loginButton.ID = "LoginLinkButton";
				break;
			case ButtonType.Image:
				loginButton = new ImageButton ();
				loginButton.ID = "LoginImageButton";
				break;
			}
			IButtonControl control = (loginButton as IButtonControl);
			control.Text = LoginButtonText;
			control.CommandName = LoginButtonCommandName;
			control.Command += new CommandEventHandler (LoginClick);
			control.ValidationGroup = this.UniqueID;

			// adds them all at the end (after setting their properties)
			Controls.Add (userNameTextBox);
			Controls.Add (userNameRequired);
			Controls.Add (passwordTextBox);
			Controls.Add (passwordRequired);
			Controls.Add (rememberMeCheckBox);
			Controls.Add (loginButton);
		}

		protected override void LoadViewState (object savedState)
		{
			if (savedState == null) {
				base.LoadViewState (null);
				return;
			}

			object[] state = (object[]) savedState;
			base.LoadViewState (state [0]);
			if (state [1] != null)
				(LoginButtonStyle as IStateManager).LoadViewState (state [1]);
			if (state [2] != null)
				(LabelStyle as IStateManager).LoadViewState (state [2]);
			if (state [3] != null)
				(TextBoxStyle as IStateManager).LoadViewState (state [3]);
			if (state [4] != null)
				(HyperLinkStyle as IStateManager).LoadViewState (state [4]);
			if (state [5] != null)
				(InstructionTextStyle as IStateManager).LoadViewState (state [5]);
			if (state [6] != null)
				(TitleTextStyle as IStateManager).LoadViewState (state [6]);
			if (state [7] != null)
				(CheckBoxStyle as IStateManager).LoadViewState (state [7]);
			if (state [8] != null)
				(FailureTextStyle as IStateManager).LoadViewState (state [8]);
			if (state [9] != null)
				(ValidatorTextStyle as IStateManager).LoadViewState (state [9]);
		}

		protected virtual void OnAuthenticate (AuthenticateEventArgs e)
		{
			// this gets called after OnLoggingIn and the authentication so we can change the result
			AuthenticateEventHandler authenticate = (AuthenticateEventHandler) Events [authenticateEvent];
			if (authenticate != null)
				authenticate (this, e);
		}

		protected override bool OnBubbleEvent (object source, EventArgs e)
		{
			// check for submit button
			CommandEventArgs cea = (e as CommandEventArgs);
			if ((source == loginButton) && (cea != null) && (cea.CommandName == "Login")) {
				AuthenticateUser ();
				return true;
			}
			return false;
		}

		protected virtual void OnLoggedIn (EventArgs e)
		{
			// this gets called only if the authentication was successful
			EventHandler loggedIn = (EventHandler) Events [loggedInEvent];
			if (loggedIn != null)
				loggedIn (this, e);
		}

		protected virtual void OnLoggingIn (LoginCancelEventArgs e)
		{
			// this gets called before OnAuthenticate so we can abort the authentication process
			LoginCancelEventHandler loggingIn = (LoginCancelEventHandler) Events [loggedInEvent];
			if (loggingIn != null)
				loggingIn (this, e);
		}

		protected virtual void OnLoginError (EventArgs e)
		{
			// this gets called only if the authentication wasn't successful
			EventHandler loginError = (EventHandler) Events [loginErrorEvent];
			if (loginError != null)
				loginError (this, e);
		}

		[MonoTODO ("overriden for ?")]
		protected internal override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
			// note: doc says that UserName and Password aren't available at 
			// PageLoad but are during PreRender phase, so... ???
		}

		[MonoTODO ("render error messages")]
		protected internal override void Render (HtmlTextWriter writer)
		{
			if (writer == null)
				return;

			// VisibleWhenLoggedIn isn't applicable to the default login page
			if (!VisibleWhenLoggedIn && !IsDefaultLoginPage () && IsLoggedIn ())
				return;

			if (Page != null) {
				Page.VerifyRenderingInServerForm (this);
			}

			EnsureChildControls ();

			userNameRequired.ToolTip = UserNameRequiredErrorMessage;
			passwordRequired.ToolTip = PasswordRequiredErrorMessage;

			bool vertical = (Orientation == Orientation.Vertical);
			bool textontop = (TextLayout == LoginTextLayout.TextOnTop);
			string colspan = vertical ? (textontop ? String.Empty : "2") : (textontop ? "4" : "6");
			string align = (textontop ? "left" : "right");

			// outer table
			writer.AddAttribute (HtmlTextWriterAttribute.Cellpadding, BorderPadding.ToString (CultureInfo.InvariantCulture));
			writer.AddAttribute (HtmlTextWriterAttribute.Border, "0");
			AddAttributesToRender (writer);
			writer.RenderBeginTag (HtmlTextWriterTag.Table);
			writer.RenderBeginTag (HtmlTextWriterTag.Tr);
			writer.RenderBeginTag (HtmlTextWriterTag.Td);

			// inner table
			writer.AddAttribute (HtmlTextWriterAttribute.Cellpadding, "0");
			writer.AddAttribute (HtmlTextWriterAttribute.Border, "0");
			writer.RenderBeginTag (HtmlTextWriterTag.Table);

			// First row - Title
			// for both Orientation.Vertical and Orientation.Horizontal

			writer.RenderBeginTag (HtmlTextWriterTag.Tr);
			writer.AddAttribute (HtmlTextWriterAttribute.Align, "center");
			if (colspan.Length > 0)
				writer.AddAttribute (HtmlTextWriterAttribute.Colspan, colspan);
			if (!IsEmpty (titleTextStyle))
				titleTextStyle.AddAttributesToRender (writer);
			writer.RenderBeginTag (HtmlTextWriterTag.Td);
			writer.Write (TitleText);
			writer.RenderEndTag ();
			writer.RenderEndTag ();

			// Second row - Instructions (optional)
			// for both Orientation.Vertical and Orientation.Horizontal

			string instructions = InstructionText;
			if (instructions.Length > 0) {
				writer.RenderBeginTag (HtmlTextWriterTag.Tr);
				writer.AddAttribute (HtmlTextWriterAttribute.Align, "center");
				if (colspan.Length > 0)
					writer.AddAttribute (HtmlTextWriterAttribute.Colspan, colspan);
				if (!IsEmpty (instructionTextStyle))
					instructionTextStyle.AddAttributesToRender (writer);
				writer.RenderBeginTag (HtmlTextWriterTag.Td);
				writer.Write (instructions);
				writer.RenderEndTag ();
				writer.RenderEndTag ();
			}

			// Third Row
			// - Orientation.Vertical == Username
			// - Orientation.Horizontal == Username, Password, RememberMe and LogIn button

			if (!vertical && textontop) {
				RenderUserNameTextBox (writer);
				RenderPasswordTextBox (writer);
			}

			writer.RenderBeginTag (HtmlTextWriterTag.Tr);
			if (vertical) {
				writer.AddAttribute (HtmlTextWriterAttribute.Align, align);
				RenderUserNameTextBox (writer);
			} else if (!textontop) {
				RenderUserNameTextBox (writer);
			}
			if (vertical && textontop) {
				writer.RenderEndTag ();
				writer.RenderBeginTag (HtmlTextWriterTag.Tr);
			}
			writer.RenderBeginTag (HtmlTextWriterTag.Td);
			if (!IsEmpty (textBoxStyle))
				textBoxStyle.AddAttributesToRender (writer);
			userNameTextBox.RenderControl (writer);
			userNameRequired.RenderControl (writer);
			writer.RenderEndTag ();
			if (vertical)
				writer.RenderEndTag ();

			if (vertical) {
				writer.RenderBeginTag (HtmlTextWriterTag.Tr);
				writer.AddAttribute (HtmlTextWriterAttribute.Align, align);
				RenderPasswordTextBox (writer);
			} else if (!textontop) {
				RenderPasswordTextBox (writer);
			}
			if (vertical && textontop) {
				writer.RenderEndTag ();
				writer.RenderBeginTag (HtmlTextWriterTag.Tr);
			}
			writer.RenderBeginTag (HtmlTextWriterTag.Td);
			if (!IsEmpty (textBoxStyle))
				textBoxStyle.AddAttributesToRender (writer);
			passwordTextBox.RenderControl (writer);
			passwordRequired.RenderControl (writer);
			writer.RenderEndTag ();
			if (vertical)
				writer.RenderEndTag ();

			if (DisplayRememberMe) {
				if (vertical) {
					writer.RenderBeginTag (HtmlTextWriterTag.Tr);
					if (colspan.Length > 0)
						writer.AddAttribute (HtmlTextWriterAttribute.Colspan, colspan);
				}
				writer.RenderBeginTag (HtmlTextWriterTag.Td);
				rememberMeCheckBox.RenderControl (writer);
				writer.AddAttribute (HtmlTextWriterAttribute.For, rememberMeCheckBox.ClientID);
				writer.RenderBeginTag (HtmlTextWriterTag.Label);
				writer.Write (RememberMeText);
				writer.RenderEndTag ();
				writer.RenderEndTag ();
				if (vertical)
					writer.RenderEndTag ();
			}

			// TODO - detect failure
			bool failed = false;
			if (failed) {
				if (vertical)
					writer.RenderBeginTag (HtmlTextWriterTag.Tr);
				if (colspan.Length > 0)
					writer.AddAttribute (HtmlTextWriterAttribute.Colspan, colspan);
				writer.AddAttribute (HtmlTextWriterAttribute.Align, "center");
				writer.AddStyleAttribute (HtmlTextWriterStyle.Color, "red");
				if (!IsEmpty (failureTextStyle)) {
					failureTextStyle.AddAttributesToRender (writer);
				}
				writer.Write (FailureText);
				writer.RenderEndTag ();
				if (vertical)
					writer.RenderEndTag ();
			}

			// LoginButton
			if (vertical) {
				writer.RenderBeginTag (HtmlTextWriterTag.Tr);
				writer.AddAttribute (HtmlTextWriterAttribute.Align, "right");
				if (colspan.Length > 0)
					writer.AddAttribute (HtmlTextWriterAttribute.Colspan, colspan);
			}
			writer.RenderBeginTag (HtmlTextWriterTag.Td);
			if (!IsEmpty (logonButtonStyle)) {
				logonButtonStyle.AddAttributesToRender (writer);
			}
			if (loginButton is ImageButton) {
				(loginButton as ImageButton).ImageUrl = LoginButtonImageUrl;
			}
			loginButton.RenderControl (writer);
			writer.RenderEndTag ();
			writer.RenderEndTag ();

			bool userText = (CreateUserText.Length > 0);
			bool userImg = (CreateUserIconUrl.Length > 0);
			bool passText = (PasswordRecoveryText.Length > 0);
			bool passImg = (PasswordRecoveryIconUrl.Length > 0);
			bool helpText = (HelpPageText.Length > 0);
			bool helpImg = (HelpPageIconUrl.Length > 0);
			// show row if CreateUserText or CreateUserIconUrl is set
			// but not if only CreateUserUrl is set
			if (userText || userImg || passText || passImg || helpText || helpImg) {
				writer.RenderBeginTag (HtmlTextWriterTag.Tr);
				if (colspan.Length > 0)
					writer.AddAttribute (HtmlTextWriterAttribute.Colspan, colspan);
				writer.RenderBeginTag (HtmlTextWriterTag.Td);

				if (userImg) {
					writer.AddAttribute (HtmlTextWriterAttribute.Src, CreateUserIconUrl);
					writer.AddStyleAttribute (HtmlTextWriterStyle.BorderWidth, "0px");
					if (userText)
						writer.AddAttribute (HtmlTextWriterAttribute.Alt, CreateUserText);
					writer.RenderBeginTag (HtmlTextWriterTag.Img);
					writer.RenderEndTag ();
				}

				if (userText) {
					string href = CreateUserUrl;
					if (href.Length > 0)
						writer.AddAttribute (HtmlTextWriterAttribute.Href, href);
					if (hyperLinkStyle != null)
						hyperLinkStyle.AddAttributesToRender (writer);
					writer.RenderBeginTag (HtmlTextWriterTag.A);
					writer.Write (CreateUserText);
					writer.RenderEndTag ();
				}

				if (passText || passImg) {
					if (userImg || userText) {
						if (vertical) {
							writer.Write ("<br />");
						} else {
							writer.Write (" ");
						}
					}

					if (passImg) {
						writer.AddAttribute (HtmlTextWriterAttribute.Src, PasswordRecoveryIconUrl);
						writer.AddStyleAttribute (HtmlTextWriterStyle.BorderWidth, "0px");
						if (passText)
							writer.AddAttribute (HtmlTextWriterAttribute.Alt, PasswordRecoveryText);
						writer.RenderBeginTag (HtmlTextWriterTag.Img);
						writer.RenderEndTag ();
					}

					if (passText) {
						string href = PasswordRecoveryUrl;
						if (href.Length > 0)
							writer.AddAttribute (HtmlTextWriterAttribute.Href, href);
						if (hyperLinkStyle != null)
							hyperLinkStyle.AddAttributesToRender (writer);
						writer.RenderBeginTag (HtmlTextWriterTag.A);
						writer.Write (PasswordRecoveryText);
						writer.RenderEndTag ();
					}
				}

				if (helpText || helpImg) {
					if (userImg || userText || passText || passImg) {
						if (vertical) {
							writer.Write ("<br />");
						} else {
							writer.Write (" ");
						}
					}

					if (helpImg) {
						writer.AddAttribute (HtmlTextWriterAttribute.Src, HelpPageIconUrl);
						writer.AddStyleAttribute (HtmlTextWriterStyle.BorderWidth, "0px");
						if (helpText)
							writer.AddAttribute (HtmlTextWriterAttribute.Alt, HelpPageText);
						writer.RenderBeginTag (HtmlTextWriterTag.Img);
						writer.RenderEndTag ();
					}

					if (helpText) {
						string href = HelpPageUrl;
						if (href.Length > 0)
							writer.AddAttribute (HtmlTextWriterAttribute.Href, href);
						if (hyperLinkStyle != null)
							hyperLinkStyle.AddAttributesToRender (writer);
						writer.RenderBeginTag (HtmlTextWriterTag.A);
						writer.Write (HelpPageText);
						writer.RenderEndTag ();
					}
				}

				writer.RenderEndTag ();
				writer.RenderEndTag ();
			}

			// inner table (end)
			writer.RenderEndTag (); // Table

			// outer table (end)
			writer.RenderEndTag (); // Td
			writer.RenderEndTag (); // Tr
			writer.RenderEndTag (); // Table
		}

		protected override object SaveViewState ()
		{
			object[] state = new object [10];
			state [0] = base.SaveViewState ();
			if (logonButtonStyle != null)
				state [1] = (logonButtonStyle as IStateManager).SaveViewState ();
			if (labelStyle != null)
				state [2] = (labelStyle as IStateManager).SaveViewState ();
			if (textBoxStyle != null)
				state [3] = (textBoxStyle as IStateManager).SaveViewState ();
			if (hyperLinkStyle != null)
				state [4] = (hyperLinkStyle as IStateManager).SaveViewState ();
			if (instructionTextStyle != null)
				state [5] = (instructionTextStyle as IStateManager).SaveViewState ();
			if (titleTextStyle != null)
				state [6] = (titleTextStyle as IStateManager).SaveViewState ();
			if (checkBoxStyle != null)
				state [7] = (checkBoxStyle as IStateManager).SaveViewState ();
			if (failureTextStyle != null)
				state [8] = (failureTextStyle as IStateManager).SaveViewState ();
			if (validatorTextStyle != null)
				state [9] = (validatorTextStyle as IStateManager).SaveViewState ();

			for (int i=0; i < state.Length; i++) {
				if (state [0] != null)
					return (object) state;
			}
			return null; // reduce view state
		}

		[MonoTODO ("for design-time usage - no more details available")]
		protected override void SetDesignModeState (IDictionary data)
		{
			base.SetDesignModeState (data);
		}

		protected override void TrackViewState ()
		{
			base.TrackViewState ();
			if (logonButtonStyle != null)
				(logonButtonStyle as IStateManager).TrackViewState ();
			if (labelStyle != null)
				(labelStyle as IStateManager).TrackViewState ();
			if (textBoxStyle != null)
				(textBoxStyle as IStateManager).TrackViewState ();
			if (hyperLinkStyle != null)
				(hyperLinkStyle as IStateManager).TrackViewState ();
			if (instructionTextStyle != null)
				(instructionTextStyle as IStateManager).TrackViewState ();
			if (titleTextStyle != null)
				(titleTextStyle as IStateManager).TrackViewState ();
			if (checkBoxStyle != null)
				(checkBoxStyle as IStateManager).TrackViewState ();
			if (failureTextStyle != null)
				(failureTextStyle as IStateManager).TrackViewState ();
			if (validatorTextStyle != null)
				(validatorTextStyle as IStateManager).TrackViewState ();
		}


		// events

		public event AuthenticateEventHandler Authenticate {
			add { Events.AddHandler (authenticateEvent, value); }
			remove { Events.RemoveHandler (authenticateEvent, value); }
		}

		public event EventHandler LoggedIn {
			add { Events.AddHandler (loggedInEvent, value); }
			remove { Events.RemoveHandler (loggedInEvent, value); }
		}

		public event LoginCancelEventHandler LoggingIn {
			add { Events.AddHandler (loggingInEvent, value); }
			remove { Events.RemoveHandler (loggingInEvent, value); }
		}

		public event EventHandler LoginError {
			add { Events.AddHandler (loginErrorEvent, value); }
			remove { Events.RemoveHandler (loginErrorEvent, value); }
		}


		// private stuff

		private bool AuthenticateUser ()
		{
			LoginCancelEventArgs lcea = new LoginCancelEventArgs ();
			OnLoggingIn (lcea);
			if (lcea.Cancel)
				return true;

			string mp = MembershipProvider;
			MembershipProvider provider = (mp.Length == 0) ?
				provider = Membership.Provider : Membership.Providers [mp];
			if (provider == null) {
				throw new HttpException (Locale.GetText ("No provider named '{0}' could be found.", mp));
			}

			AuthenticateEventArgs aea = new AuthenticateEventArgs ();
			aea.Authenticated = provider.ValidateUser (userNameTextBox.Text, passwordTextBox.Text);
			OnAuthenticate (aea);

			if (aea.Authenticated) {
				string url = DestinationPageUrl;
				FormsAuthentication.SetAuthCookie (userNameTextBox.Text, RememberMeSet);
				if (url.Length == 0) {
					Redirect (FormsAuthentication.LoginUrl);
				} else {
					Redirect (url);
				}
				OnLoggedIn (EventArgs.Empty);
				return true;
			} else {
				OnLoginError (EventArgs.Empty);
				if (FailureAction == LoginFailureAction.RedirectToLoginPage) {
					// login page is defined in web.config
					FormsAuthentication.RedirectToLoginPage ();
				}
				return false;
			}
		}

		private void LoginClick (object sender, CommandEventArgs e)
		{
			RaiseBubbleEvent (sender, (EventArgs)e);
		}

		private bool IsEmpty (Style style)
		{
			if (style == null)
				return true;
			return style.IsEmpty;
		}

		private bool IsDefaultLoginPage ()
		{
			if ((Page == null) || (Page.Request == null))
				return false;
			string url = Page.Request.Url.AbsolutePath;
			string defaultLogin = FormsAuthentication.LoginUrl;
			return (String.Compare (defaultLogin, 0, url, url.Length - defaultLogin.Length, defaultLogin.Length,
				true, CultureInfo.InvariantCulture) == 0);
		}

		private bool IsLoggedIn ()
		{
			if ((Page == null) || (Page.Request == null))
				return false;
			return Page.Request.IsAuthenticated;
		}

		private void Redirect (string url)
		{
			if ((Page != null) && (Page.Response != null))
				Page.Response.Redirect (url);
		}

		private void RenderUserNameTextBox (HtmlTextWriter writer)
		{
			writer.RenderBeginTag (HtmlTextWriterTag.Td);
			writer.AddAttribute (HtmlTextWriterAttribute.For, userNameTextBox.ClientID);
			writer.RenderBeginTag (HtmlTextWriterTag.Label);
			writer.Write (UserNameLabelText);
			writer.RenderEndTag ();
			writer.RenderEndTag ();
		}

		private void RenderPasswordTextBox (HtmlTextWriter writer)
		{
			writer.RenderBeginTag (HtmlTextWriterTag.Td);
			writer.AddAttribute (HtmlTextWriterAttribute.For, passwordTextBox.ClientID);
			writer.RenderBeginTag (HtmlTextWriterTag.Label);
			writer.Write (PasswordLabelText);
			writer.RenderEndTag ();
			writer.RenderEndTag ();
		}
	}
}

#endif
