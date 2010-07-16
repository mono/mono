//
// System.Web.UI.WebControls.Login class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//  Konstantin Triger  <kostat@mainsoft.com>
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
using System.Web.Util;

namespace System.Web.UI.WebControls {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[Bindable (false)]
	[DefaultEvent ("Authenticate")]
	[Designer ("System.Web.UI.Design.WebControls.LoginDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class Login : CompositeControl
#if NET_4_0
	, IRenderOuterTable
#endif
	{
		#region LoginContainer
		// TODO: This class should probably be folded into a generic one with BaseChangePasswordContainer
		sealed class LoginContainer : Control
		{
			readonly Login _owner;
#if NET_4_0
			bool renderOuterTable;
#endif
			Table _table;
			TableCell _containerCell;

			public LoginContainer (Login owner)
			{
				_owner = owner;
#if NET_4_0
				renderOuterTable = _owner.RenderOuterTable;

				if (renderOuterTable)
#endif
					InitTable ();
			}
			
			public override string ID {
				get {
					return _owner.ID;
				}
				set {
					_owner.ID = value;
				}
			}
			
			public override string ClientID {
				get {
					return _owner.ClientID;
				}
			}

			public void InstantiateTemplate (ITemplate template)
			{
#if NET_4_0
				if (!renderOuterTable)
					template.InstantiateIn (this);
				else
#endif
					template.InstantiateIn (_containerCell);
			}

			void InitTable ()
			{
				_table = new Table ();
				_containerCell = new TableCell ();

				TableRow row = new TableRow ();
				row.Cells.Add (_containerCell);
				_table.Rows.Add (row);

				Controls.AddAt (0, _table);
			}
			
			protected internal override void Render (HtmlTextWriter writer)
			{
				if (_table != null) {
					_table.CellSpacing = 0;
					_table.CellPadding = _owner.BorderPadding;
					_table.ApplyStyle (_owner.ControlStyle);
					_table.Attributes.CopyFrom (_owner.Attributes);
				}
				
				base.Render (writer);
			}
			
			public Control UserNameTextBox {
				get {
					return FindControl("UserName");
				}
			}

			public Control PasswordTextBox {
				get {
					return FindControl("Password");
				}
			}

			public Control RememberMeCheckBox {
				get {
					return FindControl("RememberMe");
				}
			}

			public ITextControl FailureTextLiteral
			{
				get { 
					return FindControl ("FailureText") as ITextControl; 
				}
			}
		}

		#endregion

		#region LoginTemplate

		sealed class LoginTemplate : WebControl, ITemplate
		{
			readonly Login _login;

			public LoginTemplate (Login login)
			{
				_login = login;
			}

			void ITemplate.InstantiateIn (Control container)
			{
				// 
				LiteralControl TitleText = new LiteralControl (_login.TitleText);

				// 
				LiteralControl InstructionText = new LiteralControl (_login.InstructionText);

				//
				TextBox UserName = new TextBox ();
				UserName.ID = "UserName";
				UserName.Text = _login.UserName;
				_login.RegisterApplyStyle (UserName, _login.TextBoxStyle);

				Label UserNameLabel = new Label ();
				UserNameLabel.ID = "UserNameLabel";
				UserNameLabel.AssociatedControlID = "UserName";
				UserNameLabel.Text = _login.UserNameLabelText;

				RequiredFieldValidator UserNameRequired = new RequiredFieldValidator ();
				UserNameRequired.ID = "UserNameRequired";
				UserNameRequired.ControlToValidate = "UserName";
				UserNameRequired.ErrorMessage = _login.UserNameRequiredErrorMessage;
				UserNameRequired.ToolTip = _login.UserNameRequiredErrorMessage;
				UserNameRequired.Text = "*";
				UserNameRequired.ValidationGroup = _login.ID;
				_login.RegisterApplyStyle (UserNameRequired, _login.ValidatorTextStyle);

				//
				TextBox Password = new TextBox ();
				Password.ID = "Password";
				Password.TextMode = TextBoxMode.Password;
				_login.RegisterApplyStyle (Password, _login.TextBoxStyle);

				Label PasswordLabel = new Label ();
				PasswordLabel.ID = "PasswordLabel";
				PasswordLabel.AssociatedControlID = "PasswordLabel";
				PasswordLabel.Text = _login.PasswordLabelText;

				RequiredFieldValidator PasswordRequired = new RequiredFieldValidator ();
				PasswordRequired.ID = "PasswordRequired";
				PasswordRequired.ControlToValidate = "Password";
				PasswordRequired.ErrorMessage = _login.PasswordRequiredErrorMessage;
				PasswordRequired.ToolTip = _login.PasswordRequiredErrorMessage;
				PasswordRequired.Text = "*";
				PasswordRequired.ValidationGroup = _login.ID;
				_login.RegisterApplyStyle (PasswordRequired, _login.ValidatorTextStyle);

				bool useRememberMe = _login != null ? _login.DisplayRememberMe : true;
				CheckBox RememberMe;
				//
				if (useRememberMe) {
					RememberMe = new CheckBox ();
					RememberMe.ID = "RememberMe";
					RememberMe.Checked = _login.RememberMeSet;
					RememberMe.Text = _login.RememberMeText;
					_login.RegisterApplyStyle (RememberMe, _login.CheckBoxStyle);
				} else
					RememberMe = null;
				
				// TODO: Error text
				Literal FailureText = new Literal ();
				FailureText.ID = "FailureText";
				FailureText.EnableViewState = false;

				//
				WebControl LoginButton = null;
				switch (_login.LoginButtonType) {
					case ButtonType.Button:
						LoginButton = new Button ();
						LoginButton.ID = "LoginButton";
						break;
					case ButtonType.Link:
						LoginButton = new LinkButton ();
						LoginButton.ID = "LoginLinkButton";
						break;
					case ButtonType.Image:
						LoginButton = new ImageButton ();
						LoginButton.ID = "LoginImageButton";
						break;
				}
				_login.RegisterApplyStyle (LoginButton, _login.LoginButtonStyle);
				LoginButton.ID = "LoginButton";
				((IButtonControl) LoginButton).Text = _login.LoginButtonText;
				((IButtonControl) LoginButton).CommandName = Login.LoginButtonCommandName;
				((IButtonControl) LoginButton).Command += new CommandEventHandler (_login.LoginClick);
				((IButtonControl) LoginButton).ValidationGroup = _login.ID;

				// Create layout table
				Table table = new Table ();
				table.CellPadding = 0;

				// Title row
				table.Rows.Add (
					CreateRow (
					CreateCell (TitleText, null, _login.TitleTextStyle, HorizontalAlign.Center)));

				// Instruction row
				if (_login.InstructionText.Length > 0) {
					table.Rows.Add (
						CreateRow (
						CreateCell (InstructionText, null, _login.instructionTextStyle, HorizontalAlign.Center)));
				}

				if (_login.Orientation == Orientation.Horizontal) {
					// Main row
					TableRow row1 = new TableRow ();
					TableRow row2 = new TableRow ();
					if (_login.TextLayout == LoginTextLayout.TextOnTop)
						row1.Cells.Add (CreateCell (UserNameLabel, null, _login.LabelStyle));
					else
						row2.Cells.Add (CreateCell (UserNameLabel, null, _login.LabelStyle));
					row2.Cells.Add (CreateCell (UserName, UserNameRequired, null));
					if (_login.TextLayout == LoginTextLayout.TextOnTop)
						row1.Cells.Add (CreateCell (PasswordLabel, null, _login.LabelStyle));
					else
						row2.Cells.Add (CreateCell (PasswordLabel, null, _login.LabelStyle));
					row2.Cells.Add (CreateCell (Password, PasswordRequired, null));
					if (useRememberMe)
						row2.Cells.Add (CreateCell (RememberMe, null, null));
					row2.Cells.Add (CreateCell (LoginButton, null, null));
					if (row1.Cells.Count > 0)
						table.Rows.Add (row1);
					table.Rows.Add (row2);
				}
				else { // Orientation.Vertical
					if (_login.TextLayout == LoginTextLayout.TextOnLeft)
						table.Rows.Add (CreateRow (UserNameLabel, UserName, UserNameRequired, _login.LabelStyle));
					else {
						table.Rows.Add (CreateRow (UserNameLabel, null, null, _login.LabelStyle));
						table.Rows.Add (CreateRow (null, UserName, UserNameRequired, null));
					}
					if (_login.TextLayout == LoginTextLayout.TextOnLeft)
						table.Rows.Add (CreateRow (PasswordLabel, Password, PasswordRequired, _login.LabelStyle));
					else {
						table.Rows.Add (CreateRow (PasswordLabel, null, null, _login.LabelStyle));
						table.Rows.Add (CreateRow (null, Password, PasswordRequired, null));
					}
					if (useRememberMe)
						table.Rows.Add (CreateRow (CreateCell (RememberMe, null, null)));
					table.Rows.Add (CreateRow (CreateCell (LoginButton, null, null, HorizontalAlign.Right)));
				}

				// Error message row
				if (_login.FailureTextStyle.ForeColor.IsEmpty)
					_login.FailureTextStyle.ForeColor = System.Drawing.Color.Red;

				table.Rows.Add (
					CreateRow (
					CreateCell (FailureText, null, _login.FailureTextStyle)));

				// Links row
				TableCell linksCell = new TableCell ();
				_login.RegisterApplyStyle (linksCell, _login.HyperLinkStyle);

				if (AddLink (_login.CreateUserUrl, _login.CreateUserText, _login.CreateUserIconUrl, linksCell, _login.HyperLinkStyle))
					if (_login.Orientation == Orientation.Vertical)
						linksCell.Controls.Add (new LiteralControl ("<br/>"));
					else
						linksCell.Controls.Add (new LiteralControl (" "));

				if (AddLink (_login.PasswordRecoveryUrl, _login.PasswordRecoveryText, _login.PasswordRecoveryIconUrl, linksCell, _login.HyperLinkStyle))
					if (_login.Orientation == Orientation.Vertical)
						linksCell.Controls.Add (new LiteralControl ("<br/>"));
					else
						linksCell.Controls.Add (new LiteralControl (" "));

				AddLink (_login.HelpPageUrl, _login.HelpPageText, _login.HelpPageIconUrl, linksCell, _login.HyperLinkStyle);
				table.Rows.Add (CreateRow (linksCell));

				FixTableColumnSpans (table);
				container.Controls.Add (table);
			}

			TableRow CreateRow (TableCell cell)
			{
				TableRow row = new TableRow ();
				row.Cells.Add (cell);
				return row;
			}

			TableRow CreateRow (Control c0, Control c1, Control c2, Style s)
			{
				TableRow row = new TableRow ();
				TableCell cell0 = new TableCell ();
				TableCell cell1 = new TableCell ();

				if (c0 != null) {
					cell0.Controls.Add (c0);
					row.Controls.Add (cell0);
				}

				if (s != null)
					cell0.ApplyStyle (s);

				if ((c1 != null) && (c2 != null)) {
					cell1.Controls.Add (c1);
					cell1.Controls.Add (c2);
					cell0.HorizontalAlign = HorizontalAlign.Right;
					row.Controls.Add (cell1);
				}
				return row;
			}
			
			TableCell CreateCell (Control c0, Control c1, Style s, HorizontalAlign align)
			{
				TableCell cell = CreateCell (c0, c1, s);
				cell.HorizontalAlign = align;
				return cell;
			}

			TableCell CreateCell (Control c0, Control c1, Style s)
			{
				TableCell cell = new TableCell ();
				if (s != null)
					cell.ApplyStyle (s);

				cell.Controls.Add (c0);
				if (c1 != null)
					cell.Controls.Add (c1);

				return cell;
			}

			bool AddLink (string pageUrl, string linkText, string linkIcon, WebControl container, Style style)
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
					_login.RegisterApplyStyle (link, style);
					container.Controls.Add (link);
					added = true;
				}
				return added;
			}

			void FixTableColumnSpans (Table table)
			{
				int maxCols = 0;
				for (int row = 0; row < table.Rows.Count; row++) {
					if (maxCols < table.Rows [row].Cells.Count)
						maxCols = table.Rows [row].Cells.Count;
				}
				for (int row = 0; row < table.Rows.Count; row++) {
					if (table.Rows [row].Cells.Count == 1 && maxCols > 1)
							table.Rows [row].Cells [0].ColumnSpan = maxCols;
				}
			}
		}

		#endregion

		public static readonly string LoginButtonCommandName = "Login";

		static readonly object authenticateEvent = new object ();
		static readonly object loggedInEvent = new object ();
		static readonly object loggingInEvent = new object ();
		static readonly object loginErrorEvent = new object ();

		TableItemStyle checkBoxStyle;
		TableItemStyle failureTextStyle;
		TableItemStyle hyperLinkStyle;
		TableItemStyle instructionTextStyle;
		TableItemStyle labelStyle;
		Style logonButtonStyle;
		Style textBoxStyle;
		TableItemStyle titleTextStyle;
		Style validatorTextStyle;
		ArrayList styles = new ArrayList ();

		ITemplate layoutTemplate;
		LoginContainer container;

		string _password;
#if NET_4_0
		bool renderOuterTable = true;
#endif
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
				return _password != null ? _password : String.Empty;
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
#if NET_4_0
		[DefaultValue (true)]
		public virtual bool RenderOuterTable {
			get { return renderOuterTable; }
			set { renderOuterTable = value; }
		}
#endif
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

		LoginContainer LoginTemplateContainer
		{
			get {
				if (container == null)
					container = new LoginContainer (this);
				return container;
			}
		}


		// methods

		protected internal override void CreateChildControls ()
		{
			Controls.Clear ();

			ITemplate template = LayoutTemplate;
			if (template == null)
				template = new LoginTemplate (this);

			LoginTemplateContainer.InstantiateTemplate (template);

			Controls.Add (container);

			IEditableTextControl editable;
			editable = container.UserNameTextBox as IEditableTextControl;

			if (editable != null) {
				editable.Text = UserName;
				editable.TextChanged += new EventHandler (UserName_TextChanged);
			}
			else
				throw new HttpException ("LayoutTemplate does not contain an IEditableTextControl with ID UserName for the username.");

			editable = container.PasswordTextBox as IEditableTextControl;

			if (editable != null)
				editable.TextChanged += new EventHandler (Password_TextChanged);
			else
				throw new HttpException ("LayoutTemplate does not contain an IEditableTextControl with ID Password for the password.");

			ICheckBoxControl checkBox = container.RememberMeCheckBox as ICheckBoxControl;

			if (checkBox != null)
				checkBox.CheckedChanged += new EventHandler (RememberMe_CheckedChanged);
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

		bool HasOnAuthenticateHandler ()
		{
			return Events [authenticateEvent] != null;
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
			if ((cea != null) &&
			    String.Equals (cea.CommandName, LoginButtonCommandName, StringComparison.InvariantCultureIgnoreCase)) {
				if (!AuthenticateUser ()) {
					ITextControl failureText = LoginTemplateContainer.FailureTextLiteral;
					if (failureText != null)
						failureText.Text = FailureText;
				}
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
			LoginCancelEventHandler loggingIn = (LoginCancelEventHandler) Events [loggingInEvent];
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

		protected internal override void Render (HtmlTextWriter writer)
		{
#if NET_4_0
			VerifyInlinePropertiesNotSet ();
#endif
			// VisibleWhenLoggedIn isn't applicable to the default login page
			if (!VisibleWhenLoggedIn && !IsDefaultLoginPage () && IsLoggedIn ())
				return;

			Page page = Page;
			if (page != null)
				page.VerifyRenderingInServerForm (this);

			EnsureChildControls ();

			foreach (object [] styleDef in styles)
				((WebControl) styleDef [0]).ApplyStyle ((Style) styleDef [1]);

			RenderContents(writer);
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

		internal void RegisterApplyStyle (WebControl control, Style style)
		{
			styles.Add (new object [] { control, style });
		}
		
		bool AuthenticateUser ()
		{
			if (!Page.IsValid)
				return true;

			LoginCancelEventArgs lcea = new LoginCancelEventArgs ();
			OnLoggingIn (lcea);
			if (lcea.Cancel)
				return true;

			AuthenticateEventArgs aea = new AuthenticateEventArgs ();
			
			if (!HasOnAuthenticateHandler ()) {
				string mp = MembershipProvider;
				MembershipProvider provider = (mp.Length == 0) ?
					provider = Membership.Provider : Membership.Providers [mp];
				if (provider == null) {
					throw new HttpException (Locale.GetText ("No provider named '{0}' could be found.", mp));
				}

				aea.Authenticated = provider.ValidateUser (UserName, Password);
			}
			OnAuthenticate (aea);

			if (aea.Authenticated) {
				FormsAuthentication.SetAuthCookie (UserName, RememberMeSet);
				OnLoggedIn (EventArgs.Empty);

				string url = DestinationPageUrl;
				if (Page.Request.Path.StartsWith (FormsAuthentication.LoginUrl, StringComparison.InvariantCultureIgnoreCase)) {
					if (!String.IsNullOrEmpty (FormsAuthentication.ReturnUrl))
						Redirect (FormsAuthentication.ReturnUrl);
					else if (!String.IsNullOrEmpty (DestinationPageUrl))
						Redirect (url);
					else if (!String.IsNullOrEmpty (FormsAuthentication.DefaultUrl))
						Redirect (FormsAuthentication.DefaultUrl);
					else if (url.Length == 0)
						Refresh ();
				}
				else if (!String.IsNullOrEmpty (DestinationPageUrl)) {
					Redirect (url);
				}
				else {
					Refresh ();
				}
				return true;
			}
			else {
				OnLoginError (EventArgs.Empty);
				if (FailureAction == LoginFailureAction.RedirectToLoginPage) {
					// login page is defined in web.config
					FormsAuthentication.RedirectToLoginPage ();
				}
				return false;
			}
		}

		// TODO: its called from default template only, not usefully, OnBubbleEvent 
		// do handle command, need be removed
		[MonoTODO()]
		void LoginClick (object sender, CommandEventArgs e)
		{
			RaiseBubbleEvent (sender, (EventArgs)e);
		}

		bool IsDefaultLoginPage ()
		{
			if ((Page == null) || (Page.Request == null))
				return false;
			string defaultLogin = FormsAuthentication.LoginUrl;
			if (defaultLogin == null)
				return false;
			string url = Page.Request.Url.AbsolutePath;
			return (String.Compare (defaultLogin, 0, url, url.Length - defaultLogin.Length, defaultLogin.Length,
				true, Helpers.InvariantCulture) == 0);
		}

		bool IsLoggedIn ()
		{
			if ((Page == null) || (Page.Request == null))
				return false;
			return Page.Request.IsAuthenticated;
		}

		void Redirect (string url)
		{
			if ((Page != null) && (Page.Response != null))
				Page.Response.Redirect (url);
		}
		
		void Refresh () {
			if ((Page != null) && (Page.Response != null))
				Page.Response.Redirect (Page.Request.RawUrl);
		}

		void UserName_TextChanged (object sender, EventArgs e)
		{
			UserName = ((ITextControl)sender).Text;
		}

		void Password_TextChanged (object sender, EventArgs e)
		{
			_password = ((ITextControl)sender).Text;
		}

		void RememberMe_CheckedChanged (object sender, EventArgs e)
		{
			RememberMeSet = ((ICheckBoxControl)sender).Checked;
		}
	}
}

#endif
