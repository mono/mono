//
// LoginTest.cs	- Unit tests for System.Web.UI.WebControls.Login
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

using System;
using System.Collections;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;

using NUnit.Framework;
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;

namespace MonoTests.System.Web.UI.WebControls {
	
    public class LoginTemplate :WebControl, ITemplate {
		public LoginTemplate() {
			ID = "kuku";
	}
	void ITemplate.InstantiateIn(Control container) {
			container.Controls.Add(this);
		}
    }

    public class LayoutTemplate :WebControl, ITemplate
    {
	TextBox user;
	TextBox pass;
	CheckBox remme;
	Button login;
	Literal failure;

	public LayoutTemplate(){
	    Buildcontrols();
	    Addtocontainer();
	}

	#region build
	public void Buildcontrols()
	{
	    user = new TextBox();
	    pass = new TextBox();
	    remme = new CheckBox();
	    login = new Button();
	    failure = new Literal();

	    ID = "Template";
	    user.ID = "UserName";
	    pass.ID = "Password";
	    remme.ID = "RememberMe";
	    login.ID = "Login";
	    failure.ID = "FailureText";
	}

	public void Addtocontainer()
	{
	    this.Controls.Add(user);
	    this.Controls.Add(pass);
	    this.Controls.Add(remme);
	    this.Controls.Add(login);
	    this.Controls.Add(failure);
	}
	#endregion

	void ITemplate.InstantiateIn(Control container)
	{
	    container.Controls.Add(this);
	}
    }
    
    public class TestLogin : Login {

		public string Tag {
			get { return base.TagName; }
		}

		public StateBag StateBag {
			get { return base.ViewState; }
		}

		public string Render ()
		{
			StringWriter sw = new StringWriter ();
			sw.NewLine = "\n";
			HtmlTextWriter writer = new HtmlTextWriter (sw);
			base.Render (writer);
			return writer.InnerWriter.ToString ();
		}

		public Style GetStyle ()
		{
			return base.CreateControlStyle ();
		}

		public void TrackState ()
		{
			TrackViewState ();
		}

		public void LoadState (object state)
		{
			LoadViewState (state);
		}

		public object SaveState ()
		{
			return SaveViewState ();
		}

		public void SetDesignMode (IDictionary dic)
		{
			base.SetDesignModeState (dic);
		}

		private bool authenticated;
		private bool cancel;
		private bool onAuthenticate;
		private bool onBubble;
		private bool onLoggedIn;
		private bool onLoggingIn;
		private bool onLoginError;

		public bool Authenticated {
			get { return authenticated; }
			set { authenticated = value; }
		}

		public bool Cancel {
			get { return cancel; }
			set { cancel = value; }
		}

		public bool OnAuthenticateCalled {
			get { return onAuthenticate; }
			set { onAuthenticate = value; }
		}

		protected override void OnAuthenticate (AuthenticateEventArgs e)
		{
			onAuthenticate = true;
			e.Authenticated = authenticated;
			base.OnAuthenticate (e);
			e.Authenticated = authenticated;
		}

		public void DoAuthenticate (AuthenticateEventArgs e)
		{
			base.OnAuthenticate (e);
		}

		public bool OnBubbleEventCalled {
			get { return onBubble; }
			set { onBubble = value; }
		}

		protected override bool OnBubbleEvent (object source, EventArgs e)
		{
			onBubble = true;
			return base.OnBubbleEvent (source, e);
		}

		public bool DoBubbleEvent (object source, EventArgs e)
		{
			return base.OnBubbleEvent (source, e);
		}

		public bool OnLoggedInCalled {
			get { return onLoggedIn; }
			set { onLoggedIn = value; }
		}

		protected override void OnLoggedIn (EventArgs e)
		{
			onLoggedIn = true;
			base.OnLoggedIn (e);
		}

		public void DoLoggedIn (EventArgs e)
		{
			base.OnLoggedIn (e);
		}

		public bool OnLoggingInCalled {
			get { return onLoggingIn; }
			set { onLoggingIn = value; }
		}

		protected override void OnLoggingIn (LoginCancelEventArgs e)
		{
			onLoggingIn = true;
			e.Cancel = cancel;
			base.OnLoggingIn (e);
		}

		public void DoLoggingIn (LoginCancelEventArgs e)
		{
			base.OnLoggingIn (e);
		}

		public bool OnLoginErrorCalled {
			get { return onLoginError; }
			set { onLoginError = value; }
		}

		protected override void OnLoginError (EventArgs e)
		{
			onLoginError = true;
			base.OnLoginError (e);
		}

		public void DoLoginError (EventArgs e)
		{
			base.OnLoginError (e);
		}
		
		public void DoEnsureChildControls() {
			base.EnsureChildControls ();
		}
	}

	[TestFixture]
	public class LoginTest {

		private HtmlTextWriter GetWriter ()
		{
			StringWriter sw = new StringWriter ();
			sw.NewLine = "\n";
			return new HtmlTextWriter (sw);
		}

#if NET_2_0
		[TestFixtureSetUp]
		public void SetUp ()
		{
			WebTest.CopyResource (GetType (), "NoEventValidation.aspx", "NoEventValidation.aspx");
			WebTest.CopyResource (GetType (), "LoginDisplayRememberMe.aspx", "LoginDisplayRememberMe.aspx");
		}
#endif

		[Test]
		public void ReadOnlyFields ()
		{
			Assert.AreEqual ("Login", Login.LoginButtonCommandName, "LoginButtonCommandName");
		}

		[Test]
		public void DefaultProperties ()
		{
			TestLogin l = new TestLogin ();
			Assert.AreEqual (0, l.Attributes.Count, "Attributes.Count");
			Assert.AreEqual (0, l.StateBag.Count, "ViewState.Count");

			Assert.AreEqual (1, l.BorderPadding, "BorderPadding");
			Assert.AreEqual (String.Empty, l.CreateUserIconUrl, "CreateUserIconUrl");
			Assert.AreEqual (String.Empty, l.CreateUserText, "CreateUserText");
			Assert.AreEqual (String.Empty, l.DestinationPageUrl, "DestinationPageUrl");
			Assert.IsTrue (l.DisplayRememberMe, "DisplayRememberMe");
			Assert.AreEqual (LoginFailureAction.Refresh, l.FailureAction, "FailureAction");
			Assert.AreEqual ("Your login attempt was not successful. Please try again.", l.FailureText, "FailureText");
			Assert.AreEqual (String.Empty, l.HelpPageIconUrl, "HelpPageIconUrl");
			Assert.AreEqual (String.Empty, l.HelpPageText, "HelpPageText");
			Assert.AreEqual (String.Empty, l.HelpPageUrl, "HelpPageUrl");
			Assert.AreEqual (String.Empty, l.InstructionText, "InstructionText");
			Assert.AreEqual (String.Empty, l.LoginButtonImageUrl, "LoginButtonImageUrl");
			Assert.AreEqual ("Log In", l.LoginButtonText, "LoginButtonText");
			Assert.AreEqual (ButtonType.Button, l.LoginButtonType, "LoginButtonType");
			Assert.AreEqual (String.Empty, l.MembershipProvider, "MembershipProvider");
			Assert.AreEqual (Orientation.Vertical, l.Orientation, "Orientation");
			Assert.AreEqual (String.Empty, l.Password, "Password");
			Assert.AreEqual ("Password:", l.PasswordLabelText, "PasswordLabelText");
			Assert.AreEqual (String.Empty, l.PasswordRecoveryIconUrl, "PasswordRecoveryIconUrl");
			Assert.AreEqual (String.Empty, l.PasswordRecoveryText, "PasswordRecoveryText");
			Assert.AreEqual (String.Empty, l.PasswordRecoveryUrl, "PasswordRecoveryUrl");
			Assert.AreEqual ("Password is required.", l.PasswordRequiredErrorMessage, "PasswordRequiredErrorMessage");
			Assert.IsFalse (l.RememberMeSet, "RememberMeSet");
			Assert.AreEqual ("Remember me next time.", l.RememberMeText, "RememberMeText");
			Assert.AreEqual (LoginTextLayout.TextOnLeft, l.TextLayout, "TextLayout");
			Assert.AreEqual ("Log In", l.TitleText, "TitleText");
			Assert.AreEqual (String.Empty, l.UserName, "UserName");
			Assert.AreEqual ("User Name:", l.UserNameLabelText, "UserNameLabelText");
			Assert.AreEqual ("User Name is required.", l.UserNameRequiredErrorMessage, "UserNameRequiredErrorMessage");
			Assert.IsTrue (l.VisibleWhenLoggedIn, "VisibleWhenLoggedIn");

			// Styles
			Assert.IsNotNull (l.CheckBoxStyle, "CheckBoxStyle");
			Assert.IsNotNull (l.FailureTextStyle, "FailureTextStyle");
			Assert.IsNotNull (l.HyperLinkStyle, "HyperLinkStyle");
			Assert.IsNotNull (l.InstructionTextStyle, "InstructionTextStyle");
			Assert.IsNotNull (l.LabelStyle, "LabelStyle");
			Assert.IsNotNull (l.LoginButtonStyle, "LoginButtonStyle");
			Assert.IsNotNull (l.TextBoxStyle, "TextBoxStyle");
			Assert.IsNotNull (l.TitleTextStyle, "TitleTextStyle");
			Assert.IsNotNull (l.ValidatorTextStyle, "ValidatorTextStyle");

			// Templates
			Assert.IsNull (l.LayoutTemplate, "LayoutTemplate");

			Assert.AreEqual ("table", l.Tag, "TagName");
			Assert.AreEqual (0, l.Attributes.Count, "Attributes.Count-2");
			Assert.AreEqual (0, l.StateBag.Count, "ViewState.Count-2");
		}

		[Test]
		public void AssignToDefaultProperties ()
		{
			TestLogin l = new TestLogin ();
			Assert.AreEqual (0, l.Attributes.Count, "Attributes.Count");
			Assert.AreEqual (0, l.StateBag.Count, "ViewState.Count");

			l.BorderPadding = 1;
			Assert.AreEqual (1, l.BorderPadding, "BorderPadding");
			Assert.AreEqual (1, l.StateBag.Count, "ViewState.Count-1");
			l.CreateUserIconUrl = String.Empty;
			Assert.AreEqual (String.Empty, l.CreateUserIconUrl, "CreateUserIconUrl");
			Assert.AreEqual (2, l.StateBag.Count, "ViewState.Count-2");
			l.CreateUserText = String.Empty;
			Assert.AreEqual (String.Empty, l.CreateUserText, "CreateUserText");
			Assert.AreEqual (3, l.StateBag.Count, "ViewState.Count-3");
			l.DestinationPageUrl = String.Empty;
			Assert.AreEqual (String.Empty, l.DestinationPageUrl, "DestinationPageUrl");
			Assert.AreEqual (4, l.StateBag.Count, "ViewState.Count-4");
			l.DisplayRememberMe = true;
			Assert.IsTrue (l.DisplayRememberMe, "DisplayRememberMe");
			Assert.AreEqual (5, l.StateBag.Count, "ViewState.Count-5");
			l.FailureAction = LoginFailureAction.Refresh;
			Assert.AreEqual (LoginFailureAction.Refresh, l.FailureAction, "FailureAction");
			Assert.AreEqual (6, l.StateBag.Count, "ViewState.Count-6");
			l.FailureText = "Your login attempt was not successful. Please try again.";
			Assert.AreEqual ("Your login attempt was not successful. Please try again.", l.FailureText, "FailureText");
			Assert.AreEqual (7, l.StateBag.Count, "ViewState.Count-7");
			l.HelpPageIconUrl = String.Empty;
			Assert.AreEqual (String.Empty, l.HelpPageIconUrl, "HelpPageIconUrl");
			Assert.AreEqual (8, l.StateBag.Count, "ViewState.Count-8");
			l.HelpPageText = String.Empty;
			Assert.AreEqual (String.Empty, l.HelpPageText, "HelpPageText");
			Assert.AreEqual (9, l.StateBag.Count, "ViewState.Count-9");
			l.HelpPageUrl = String.Empty;
			Assert.AreEqual (String.Empty, l.HelpPageUrl, "HelpPageUrl");
			Assert.AreEqual (10, l.StateBag.Count, "ViewState.Count-10");
			l.InstructionText = String.Empty;
			Assert.AreEqual (String.Empty, l.InstructionText, "InstructionText");
			Assert.AreEqual (11, l.StateBag.Count, "ViewState.Count-11");
			l.LayoutTemplate = null;
			Assert.IsNull (l.LayoutTemplate, "LayoutTemplate");
			l.LoginButtonImageUrl = String.Empty;
			Assert.AreEqual (String.Empty, l.LoginButtonImageUrl, "LoginButtonImageUrl");
			Assert.AreEqual (12, l.StateBag.Count, "ViewState.Count-12");
			l.LoginButtonText = "Log In";
			Assert.AreEqual ("Log In", l.LoginButtonText, "LoginButtonText");
			Assert.AreEqual (13, l.StateBag.Count, "ViewState.Count-13");
			l.LoginButtonType = ButtonType.Button;
			Assert.AreEqual (ButtonType.Button, l.LoginButtonType, "LoginButtonType");
			Assert.AreEqual (14, l.StateBag.Count, "ViewState.Count-14");
			l.MembershipProvider = String.Empty;
			Assert.AreEqual (String.Empty, l.MembershipProvider, "MembershipProvider");
			Assert.AreEqual (15, l.StateBag.Count, "ViewState.Count-15");
			l.Orientation = Orientation.Vertical;
			Assert.AreEqual (Orientation.Vertical, l.Orientation, "Orientation");
			Assert.AreEqual (16, l.StateBag.Count, "ViewState.Count-16");
			// note: Password is read-only
			Assert.AreEqual (String.Empty, l.Password, "Password");
			l.PasswordLabelText = "Password:";
			Assert.AreEqual ("Password:", l.PasswordLabelText, "PasswordLabelText");
			Assert.AreEqual (17, l.StateBag.Count, "ViewState.Count-17");
			l.PasswordRecoveryIconUrl = String.Empty;
			Assert.AreEqual (String.Empty, l.PasswordRecoveryIconUrl, "PasswordRecoveryIconUrl");
			Assert.AreEqual (18, l.StateBag.Count, "ViewState.Count-18");
			l.PasswordRecoveryText = String.Empty;
			Assert.AreEqual (String.Empty, l.PasswordRecoveryText, "PasswordRecoveryText");
			Assert.AreEqual (19, l.StateBag.Count, "ViewState.Count-19");
			l.PasswordRecoveryUrl = String.Empty;
			Assert.AreEqual (String.Empty, l.PasswordRecoveryUrl, "PasswordRecoveryUrl");
			Assert.AreEqual (20, l.StateBag.Count, "ViewState.Count-20");
			l.PasswordRequiredErrorMessage = "Password is required.";
			Assert.AreEqual ("Password is required.", l.PasswordRequiredErrorMessage, "PasswordRequiredErrorMessage");
			Assert.AreEqual (21, l.StateBag.Count, "ViewState.Count-21");
			l.RememberMeSet = false;
			Assert.IsFalse (l.RememberMeSet, "RememberMeSet");
			Assert.AreEqual (22, l.StateBag.Count, "ViewState.Count-22");
			l.RememberMeText = "Remember me next time.";
			Assert.AreEqual ("Remember me next time.", l.RememberMeText, "RememberMeText");
			Assert.AreEqual (23, l.StateBag.Count, "ViewState.Count-23");
			l.TextLayout = LoginTextLayout.TextOnLeft;
			Assert.AreEqual (LoginTextLayout.TextOnLeft, l.TextLayout, "TextLayout");
			Assert.AreEqual (24, l.StateBag.Count, "ViewState.Count-24");
			l.TitleText = "Log In";
			Assert.AreEqual ("Log In", l.TitleText, "TitleText");
			Assert.AreEqual (25, l.StateBag.Count, "ViewState.Count-25");
			l.UserName = String.Empty;
			Assert.AreEqual (String.Empty, l.UserName, "UserName");
			Assert.AreEqual (26, l.StateBag.Count, "ViewState.Count-26");
			l.UserNameLabelText = "User Name:";
			Assert.AreEqual ("User Name:", l.UserNameLabelText, "UserNameLabelText");
			Assert.AreEqual (27, l.StateBag.Count, "ViewState.Count-27");
			l.UserNameRequiredErrorMessage = "User Name is required.";
			Assert.AreEqual ("User Name is required.", l.UserNameRequiredErrorMessage, "UserNameRequiredErrorMessage");
			Assert.AreEqual (28, l.StateBag.Count, "ViewState.Count-28");
			l.VisibleWhenLoggedIn = true;
			Assert.IsTrue (l.VisibleWhenLoggedIn, "VisibleWhenLoggedIn");
			Assert.AreEqual (29, l.StateBag.Count, "ViewState.Count-29");

			Assert.AreEqual (0, l.Attributes.Count, "Attributes.Count-2");
		}

		[Test]
		public void NullProperties ()
		{
			TestLogin l = new TestLogin ();
			Assert.AreEqual (0, l.StateBag.Count, "ViewState.Count");

			// unlike 1.1 controls it seems that null (and not the default value)
			// must be used to remove values from the ViewState

			l.CreateUserIconUrl = "*";
			Assert.AreEqual ("*", l.CreateUserIconUrl, "CreateUserIconUrl*");
			Assert.AreEqual (1, l.StateBag.Count, "ViewState.Count-1*");
			l.CreateUserIconUrl = null;
			Assert.AreEqual (String.Empty, l.CreateUserIconUrl, "CreateUserIconUrl");
			Assert.AreEqual (0, l.StateBag.Count, "ViewState.Count-1");

			l.CreateUserText = "*";
			Assert.AreEqual ("*", l.CreateUserText, "CreateUserText*");
			Assert.AreEqual (1, l.StateBag.Count, "ViewState.Count-2*");
			l.CreateUserText = null;
			Assert.AreEqual (String.Empty, l.CreateUserText, "CreateUserText");
			Assert.AreEqual (0, l.StateBag.Count, "ViewState.Count-2");

			l.DestinationPageUrl = "*";
			Assert.AreEqual ("*", l.DestinationPageUrl, "DestinationPageUrl*");
			Assert.AreEqual (1, l.StateBag.Count, "ViewState.Count-3*");
			l.DestinationPageUrl = null;
			Assert.AreEqual (String.Empty, l.DestinationPageUrl, "DestinationPageUrl");
			Assert.AreEqual (0, l.StateBag.Count, "ViewState.Count-3");

			l.FailureText = "*";
			Assert.AreEqual ("*", l.FailureText, "FailureTex*");
			Assert.AreEqual (1, l.StateBag.Count, "ViewState.Count-4*");
			l.FailureText = null;
			Assert.AreEqual ("Your login attempt was not successful. Please try again.", l.FailureText, "FailureText");
			Assert.AreEqual (0, l.StateBag.Count, "ViewState.Count-4");

			l.HelpPageIconUrl = "*";
			Assert.AreEqual ("*", l.HelpPageIconUrl, "HelpPageIconUrl*");
			Assert.AreEqual (1, l.StateBag.Count, "ViewState.Count-5*");
			l.HelpPageIconUrl = null;
			Assert.AreEqual (String.Empty, l.HelpPageIconUrl, "HelpPageIconUrl");
			Assert.AreEqual (0, l.StateBag.Count, "ViewState.Count-5");

			l.HelpPageText = "*";
			Assert.AreEqual ("*", l.HelpPageText, "HelpPageText*");
			Assert.AreEqual (1, l.StateBag.Count, "ViewState.Count-6*");
			l.HelpPageText = null;
			Assert.AreEqual (String.Empty, l.HelpPageText, "HelpPageText");
			Assert.AreEqual (0, l.StateBag.Count, "ViewState.Count-6");

			l.HelpPageUrl = "*";
			Assert.AreEqual ("*", l.HelpPageUrl, "HelpPageUrl*");
			Assert.AreEqual (1, l.StateBag.Count, "ViewState.Count-7*");
			l.HelpPageUrl = null;
			Assert.AreEqual (String.Empty, l.HelpPageUrl, "HelpPageUrl");
			Assert.AreEqual (0, l.StateBag.Count, "ViewState.Count-7");

			l.InstructionText = "*";
			Assert.AreEqual ("*", l.InstructionText, "InstructionText*");
			Assert.AreEqual (1, l.StateBag.Count, "ViewState.Count-8*");
			l.InstructionText = null;
			Assert.AreEqual (String.Empty, l.InstructionText, "InstructionText");
			Assert.AreEqual (0, l.StateBag.Count, "ViewState.Count-8");

			l.LoginButtonImageUrl = "*";
			Assert.AreEqual ("*", l.LoginButtonImageUrl, "LoginButtonImageUrl*");
			Assert.AreEqual (1, l.StateBag.Count, "ViewState.Count-9*");
			l.LoginButtonImageUrl = null;
			Assert.AreEqual (String.Empty, l.LoginButtonImageUrl, "LoginButtonImageUrl");
			Assert.AreEqual (0, l.StateBag.Count, "ViewState.Count-9");

			l.LoginButtonText = "*";
			Assert.AreEqual ("*", l.LoginButtonText, "LoginButtonText*");
			Assert.AreEqual (1, l.StateBag.Count, "ViewState.Count-10*");
			l.LoginButtonText = null;
			Assert.AreEqual ("Log In", l.LoginButtonText, "LoginButtonText");
			Assert.AreEqual (0, l.StateBag.Count, "ViewState.Count-10");

			l.MembershipProvider = "*";
			Assert.AreEqual ("*", l.MembershipProvider, "MembershipProvider*");
			Assert.AreEqual (1, l.StateBag.Count, "ViewState.Count-11*");
			l.MembershipProvider = null;
			Assert.AreEqual (String.Empty, l.MembershipProvider, "MembershipProvider");
			Assert.AreEqual (0, l.StateBag.Count, "ViewState.Count-11");

			l.PasswordLabelText = "*";
			Assert.AreEqual ("*", l.PasswordLabelText, "PasswordLabelText*");
			Assert.AreEqual (1, l.StateBag.Count, "ViewState.Count-12*");
			l.PasswordLabelText = null;
			Assert.AreEqual ("Password:", l.PasswordLabelText, "PasswordLabelText");
			Assert.AreEqual (0, l.StateBag.Count, "ViewState.Count-12");

			l.PasswordRecoveryIconUrl = "*";
			Assert.AreEqual ("*", l.PasswordRecoveryIconUrl, "PasswordRecoveryIconUrl*");
			Assert.AreEqual (1, l.StateBag.Count, "ViewState.Count-13*");
			l.PasswordRecoveryIconUrl = null;
			Assert.AreEqual (String.Empty, l.PasswordRecoveryIconUrl, "PasswordRecoveryIconUrl");
			Assert.AreEqual (0, l.StateBag.Count, "ViewState.Count-13");

			l.PasswordRecoveryText = "*";
			Assert.AreEqual ("*", l.PasswordRecoveryText, "PasswordRecoveryText*");
			Assert.AreEqual (1, l.StateBag.Count, "ViewState.Count-14*");
			l.PasswordRecoveryText = null;
			Assert.AreEqual (String.Empty, l.PasswordRecoveryText, "PasswordRecoveryText");
			Assert.AreEqual (0, l.StateBag.Count, "ViewState.Count-14");

			l.PasswordRecoveryUrl = "*";
			Assert.AreEqual ("*", l.PasswordRecoveryUrl, "PasswordRecoveryUrl*");
			Assert.AreEqual (1, l.StateBag.Count, "ViewState.Count-15*");
			l.PasswordRecoveryUrl = null;
			Assert.AreEqual (String.Empty, l.PasswordRecoveryUrl, "PasswordRecoveryUrl");
			Assert.AreEqual (0, l.StateBag.Count, "ViewState.Count-15");

			l.PasswordRequiredErrorMessage = "*";
			Assert.AreEqual ("*", l.PasswordRequiredErrorMessage, "PasswordRequiredErrorMessage*");
			Assert.AreEqual (1, l.StateBag.Count, "ViewState.Count-16*");
			l.PasswordRequiredErrorMessage = null;
			Assert.AreEqual ("Password is required.", l.PasswordRequiredErrorMessage, "PasswordRequiredErrorMessage");
			Assert.AreEqual (0, l.StateBag.Count, "ViewState.Count-16");

			l.RememberMeText = "*";
			Assert.AreEqual ("*", l.RememberMeText, "RememberMeText*");
			Assert.AreEqual (1, l.StateBag.Count, "ViewState.Count-17*");
			l.RememberMeText = null;
			Assert.AreEqual ("Remember me next time.", l.RememberMeText, "RememberMeText");
			Assert.AreEqual (0, l.StateBag.Count, "ViewState.Count-17");

			l.TitleText = "*";
			Assert.AreEqual ("*", l.TitleText, "TitleText*");
			Assert.AreEqual (1, l.StateBag.Count, "ViewState.Count-18*");
			l.TitleText = null;
			Assert.AreEqual ("Log In", l.TitleText, "TitleText");
			Assert.AreEqual (0, l.StateBag.Count, "ViewState.Count-18");

			l.UserName = "*";
			Assert.AreEqual ("*", l.UserName, "UserName*");
			Assert.AreEqual (1, l.StateBag.Count, "ViewState.Count-19*");
			l.UserName = null;
			Assert.AreEqual (String.Empty, l.UserName, "UserName");
			Assert.AreEqual (0, l.StateBag.Count, "ViewState.Count-19");

			l.UserNameLabelText = "*";
			Assert.AreEqual ("*", l.UserNameLabelText, "UserNameLabelText*");
			Assert.AreEqual (1, l.StateBag.Count, "ViewState.Count-20*");
			l.UserNameLabelText = null;
			Assert.AreEqual ("User Name:", l.UserNameLabelText, "UserNameLabelText");
			Assert.AreEqual (0, l.StateBag.Count, "ViewState.Count-20");

			l.UserNameRequiredErrorMessage = "*";
			Assert.AreEqual ("*", l.UserNameRequiredErrorMessage, "UserNameRequiredErrorMessage*");
			Assert.AreEqual (1, l.StateBag.Count, "ViewState.Count-21*");
			l.UserNameRequiredErrorMessage = null;
			Assert.AreEqual ("User Name is required.", l.UserNameRequiredErrorMessage, "UserNameRequiredErrorMessage");
			Assert.AreEqual (0, l.StateBag.Count, "ViewState.Count-21");
		}

		[Test]
		public void BorderPadding ()
		{
			TestLogin l = new TestLogin ();
			l.BorderPadding = Int32.MaxValue;
			Assert.AreEqual (Int32.MaxValue, l.BorderPadding, "BorderPadding");
			l.BorderPadding = 1;
			Assert.AreEqual (1, l.BorderPadding, "BorderPadding");
			l.BorderPadding = 0;
			Assert.AreEqual (0, l.BorderPadding, "BorderPadding");
			l.BorderPadding = -1;
			Assert.AreEqual (-1, l.BorderPadding, "BorderPadding");
			Assert.AreEqual (1, l.StateBag.Count, "ViewState.Count");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void BorderPadding_Negative ()
		{
			TestLogin l = new TestLogin ();
			l.BorderPadding = Int32.MinValue;
			Assert.AreEqual (1, l.BorderPadding, "BorderPadding");
		}

		[Test]
		public void FailureAction_All ()
		{
			TestLogin l = new TestLogin ();
			foreach (LoginFailureAction lfa in Enum.GetValues (typeof (LoginFailureAction))) {
				l.FailureAction = lfa;
			}
			Assert.AreEqual (1, l.StateBag.Count, "ViewState.Count");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void FailureAction_Invalid ()
		{
			TestLogin l = new TestLogin ();
			l.FailureAction = (LoginFailureAction) Int32.MinValue;
		}

	[Test]
	public void LayoutTemplate()
	{
	    TestLogin l = new TestLogin();
	    l.LayoutTemplate = new LayoutTemplate();
	    l.DoEnsureChildControls();
	    Assert.IsNotNull(l.FindControl("Template"), "LoginTemplate");
	    Assert.IsNotNull(l.FindControl("UserName"), "UserName");
	}


	[Test]
	[ExpectedException(typeof(HttpException))]
		public void LayoutTemplateException ()
		{
			TestLogin l = new TestLogin ();
			l.LayoutTemplate = new LoginTemplate();
			l.DoEnsureChildControls();
		}

		[Test]
		public void LoginButtonType_All ()
		{
			TestLogin l = new TestLogin ();
			foreach (ButtonType bt in Enum.GetValues (typeof (ButtonType))) {
				l.LoginButtonType = bt;
			}
			Assert.AreEqual (1, l.StateBag.Count, "ViewState.Count");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void LoginButtonType_Invalid ()
		{
			TestLogin l = new TestLogin ();
			l.LoginButtonType = (ButtonType)Int32.MinValue;
		}

		[Test]
		public void Orientation_All ()
		{
			TestLogin l = new TestLogin ();
			foreach (Orientation o in Enum.GetValues (typeof (Orientation))) {
				l.Orientation = o;
			}
			Assert.AreEqual (1, l.StateBag.Count, "ViewState.Count");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Orientation_Invalid ()
		{
			TestLogin l = new TestLogin ();
			l.Orientation = (Orientation)Int32.MinValue;
		}

		[Test]
		public void TextLayout_All ()
		{
			TestLogin l = new TestLogin ();
			foreach (LoginTextLayout ltl in Enum.GetValues (typeof (LoginTextLayout))) {
				l.TextLayout = ltl;
			}
			Assert.AreEqual (1, l.StateBag.Count, "ViewState.Count");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TextLayout_Invalid ()
		{
			TestLogin l = new TestLogin ();
			l.TextLayout = (LoginTextLayout)Int32.MinValue;
		}

		[Test]
		public void SaveViewState ()
		{
			TestLogin l = new TestLogin ();
			l.TrackState ();
			Assert.IsNull (l.SaveState (), "Empty");

			l.BorderPadding = 2;
			object[] vs = (object[])l.SaveState ();
			Assert.AreEqual (10, vs.Length, "Size");

			l.CreateUserIconUrl = String.Empty;
			l.CreateUserText = String.Empty;
			l.DestinationPageUrl = String.Empty;
			l.DisplayRememberMe = true;
			l.FailureAction = LoginFailureAction.Refresh;
			l.FailureText = "Your login attempt was not successful. Please try again.";
			l.HelpPageIconUrl = String.Empty;
			l.HelpPageText = String.Empty;
			l.HelpPageUrl = String.Empty;
			l.InstructionText = String.Empty;
			l.LayoutTemplate = null;
			l.LoginButtonImageUrl = String.Empty;
			l.LoginButtonText = "Log In";
			l.LoginButtonType = ButtonType.Button;
			l.MembershipProvider = String.Empty;
			l.Orientation = Orientation.Vertical;
			// note: Password is read-only
			l.PasswordLabelText = "Password:";
			l.PasswordRecoveryIconUrl = String.Empty;
			l.PasswordRecoveryText = String.Empty;
			l.PasswordRecoveryUrl = String.Empty;
			l.PasswordRequiredErrorMessage = "Password is required.";
			l.RememberMeSet = false;
			l.RememberMeText = "Remember me next time.";
			l.TextLayout = LoginTextLayout.TextOnLeft;
			l.TitleText = "Log In";
			l.UserName = String.Empty;
			l.UserNameLabelText = "User Name:";
			l.UserNameRequiredErrorMessage = "User Name is required.";
			l.VisibleWhenLoggedIn = true;
			vs = (object[])l.SaveState ();

			// the viewstate is all null except the first element
			Assert.IsNotNull (vs[0], "NotEmpty-0");
			for (int i = 1; i < vs.Length; i++)
				Assert.IsNull (vs[i], "Empty-" + i);

			l.CheckBoxStyle.HorizontalAlign = HorizontalAlign.Justify;
			vs = (object[])l.SaveState ();
			Assert.IsNotNull (vs[7], "NotEmpty-7");

			l.FailureTextStyle.HorizontalAlign = HorizontalAlign.Justify;
			vs = (object[])l.SaveState ();
			Assert.IsNotNull (vs[8], "NotEmpty-8");

			l.HyperLinkStyle.HorizontalAlign = HorizontalAlign.Justify;
			vs = (object[])l.SaveState ();
			Assert.IsNotNull (vs[4], "NotEmpty-4");

			l.InstructionTextStyle.HorizontalAlign = HorizontalAlign.Justify;
			vs = (object[])l.SaveState ();
			Assert.IsNotNull (vs[5], "NotEmpty-5");

			l.LabelStyle.HorizontalAlign = HorizontalAlign.Justify;
			vs = (object[])l.SaveState ();
			Assert.IsNotNull (vs[2], "NotEmpty-2");

			l.LoginButtonStyle.BorderStyle = BorderStyle.Double;
			vs = (object[])l.SaveState ();
			Assert.IsNotNull (vs[1], "NotEmpty-1");

			l.TextBoxStyle.BorderStyle = BorderStyle.Double;
			vs = (object[])l.SaveState ();
			Assert.IsNotNull (vs[3], "NotEmpty-3");

			l.TitleTextStyle.HorizontalAlign = HorizontalAlign.Justify;
			vs = (object[])l.SaveState ();
			Assert.IsNotNull (vs[6], "NotEmpty-6");

			l.ValidatorTextStyle.BorderStyle = BorderStyle.Double;
			vs = (object[])l.SaveState ();
			Assert.IsNotNull (vs[9], "NotEmpty-9");
		}

		[Test]
		[Category ("NunitWeb")]
		public void OnBubbleEvent ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnPreInit (_OnBubbleEvent));
			string html = t.Run ();
		}

		public static void DoOnBubbleEvent(Page p, string cmdname)
		{
			TestLogin l = new TestLogin ();
			l.Page = p;
			l.Page.Validate ();
			l.MembershipProvider = "FakeProvider";
			Button b = (Button)l.FindControl ("LoginButton");
			Assert.IsNotNull (b, "LoginButton");
			CommandEventArgs cea = new CommandEventArgs (cmdname, null);
			l.DoBubbleEvent (b, cea);
			Assert.IsTrue (l.OnLoggingInCalled, "OnLoggingIn");
			Assert.IsFalse (l.Cancel, "Cancel");
			Assert.IsTrue (l.OnAuthenticateCalled, "OnAuthenticate");
			Assert.IsFalse (l.Authenticated, "Authenticated");
			Assert.IsTrue (l.OnLoginErrorCalled, "OnLoginError");
			Assert.IsFalse (l.OnLoggedInCalled, "OnLoggedIn");
		}
		
		public static void _OnBubbleEvent(Page p)
		{
			DoOnBubbleEvent(p, "Login");
		}

		[Test]
		[Category ("NunitWeb")]
		public void OnBubbleEventCaseSensitivity ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnPreInit (_OnBubbleEventCaseSensitivity));
			string html = t.Run ();
		}
		
		public static void _OnBubbleEventCaseSensitivity(Page p)
		{
			DoOnBubbleEvent(p, "login");
		}
		
		[Test]
		public void OnBubbleEvent_Cancel_OnLoggingIn ()
		{
			TestLogin l = new TestLogin ();
			l.Page = new Page ();
			l.Page.Validate ();
			Button b = (Button)l.FindControl ("LoginButton");
			Assert.IsNotNull (b, "LoginButton");
			CommandEventArgs cea = new CommandEventArgs ("Login", null);
			l.Cancel = true;
			l.DoBubbleEvent (b, cea);
			Assert.IsTrue (l.OnLoggingInCalled, "OnLoggingIn");
			Assert.IsFalse (l.OnAuthenticateCalled, "OnAuthenticate");
			Assert.IsFalse (l.OnLoginErrorCalled, "OnLoginError");
			Assert.IsFalse (l.OnLoggedInCalled, "OnLoggedIn");
		}

		[Test]
		public void OnBubbleEvent_Authenticated_OnAuthenticate ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnPreInit (_OnBubbleEvent_Authenticated_OnAuthenticate));
			string html = t.Run ();
		}

		public static void _OnBubbleEvent_Authenticated_OnAuthenticate (Page p)
		{
			TestLogin l = new TestLogin ();
			l.Page = p;
			l.Page.Validate ();
			Button b = (Button) l.FindControl ("LoginButton");
			Assert.IsNotNull (b, "LoginButton");
			CommandEventArgs cea = new CommandEventArgs ("Login", null);
			l.UserName = "me";
			l.Authenticated = true;
			l.MembershipProvider = "FakeProvider";
			l.Authenticate += new AuthenticateEventHandler(l_Authenticate);
			try {
				l.DoBubbleEvent (b, cea);
			}
			catch (NullReferenceException) {
				// ms
			}
			catch (HttpException) {
				// no context is available
			}
		}

		public static void l_Authenticate (object sender, AuthenticateEventArgs e)
		{
			if (e.Authenticated == true) {
				TestLogin l = (TestLogin) sender;
				l.Authenticated = false;
			}
			else
				Assert.Fail ("Login failed: l_Authenticate");
		}

		private void OnLoggingIn (bool cancel)
		{
			TestLogin l = new TestLogin ();
			LoginCancelEventArgs lcea = new LoginCancelEventArgs (cancel);
			l.DoLoggingIn (lcea);
			Assert.IsFalse (l.OnBubbleEventCalled, "OnBubbleEvent");
			Assert.IsFalse (l.OnLoggingInCalled, "OnLoggingIn");
			Assert.IsFalse (l.OnAuthenticateCalled, "OnAuthenticate");
			Assert.IsFalse (l.OnLoginErrorCalled, "OnLoginError");
			Assert.IsFalse (l.OnLoggedInCalled, "OnLoggedIn");
		}

		[Test]
		public void OnLoggingIn_False ()
		{
			OnLoggingIn (false);
		}

		[Test]
		public void OnLoggingIn_True ()
		{
			OnLoggingIn (true);
		}

		private void OnAuthenticate (bool authenticate)
		{
			TestLogin l = new TestLogin ();
			AuthenticateEventArgs aea = new AuthenticateEventArgs (authenticate);
			l.DoAuthenticate (aea);
			Assert.IsFalse (l.OnBubbleEventCalled, "OnBubbleEvent");
			Assert.IsFalse (l.OnLoggingInCalled, "OnLoggingIn");
			Assert.IsFalse (l.OnAuthenticateCalled, "OnAuthenticate");
			Assert.IsFalse (l.OnLoginErrorCalled, "OnLoginError");
			Assert.IsFalse (l.OnLoggedInCalled, "OnLoggedIn");
		}

		[Test]
		public void OnAuthenticate_False ()
		{
			OnAuthenticate (false);
		}

		[Test]
		public void OnAuthenticate_True ()
		{
			OnAuthenticate (true);
		}

		[Test]
		public void OnLoginError ()
		{
			TestLogin l = new TestLogin ();
			l.DoLoginError (EventArgs.Empty);
			Assert.IsFalse (l.OnBubbleEventCalled, "OnBubbleEvent");
			Assert.IsFalse (l.OnLoggingInCalled, "OnLoggingIn");
			Assert.IsFalse (l.OnAuthenticateCalled, "OnAuthenticate");
			Assert.IsFalse (l.OnLoginErrorCalled, "OnLoginError");
			Assert.IsFalse (l.OnLoggedInCalled, "OnLoggedIn");
		}

		[Test]
		public void OnLoggedIn ()
		{
			TestLogin l = new TestLogin ();
			l.DoLoggedIn (EventArgs.Empty);
			Assert.IsFalse (l.OnBubbleEventCalled, "OnBubbleEvent");
			Assert.IsFalse (l.OnLoggingInCalled, "OnLoggingIn");
			Assert.IsFalse (l.OnAuthenticateCalled, "OnAuthenticate");
			Assert.IsFalse (l.OnLoginErrorCalled, "OnLoginError");
			Assert.IsFalse (l.OnLoggedInCalled, "OnLoggedIn");
		}

		[Test]
		[Category ("NunitWeb")]
		public void PostBackFireEvent_1 ()
		{
			WebTest t = new WebTest ("NoEventValidation.aspx");
			t.Invoker = PageInvoker.CreateOnInit (PostBackFireEvent_Init);
			string html = t.Run ();
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls.Add (GetDecoratedId (html, "UserName"));   //%24
			fr.Controls.Add (GetDecoratedId (html, "Password"));
			fr.Controls.Add (GetDecoratedId (html, "LoginButton"));
			fr.Controls ["__EVENTTARGET"].Value = "";
			fr.Controls ["__EVENTARGUMENT"].Value = "";
			fr.Controls ["Login1$UserName"].Value = "yonik";
			fr.Controls ["Login1$Password"].Value = "123456";
			fr.Controls ["Login1$LoginButton"].Value = "Log In";
			t.Request = fr;
			t.Run ();

			ArrayList eventlist = t.UserData as ArrayList;
			if (eventlist == null)
				Assert.Fail ("User data does not been created fail");

			Assert.AreEqual ("LoggingIn", eventlist [0], "#1");
			Assert.AreEqual ("Authenticate", eventlist [1], "#2");
			Assert.AreEqual ("LoginError", eventlist [2], "#3");
		}

		[Test]
		[Category ("NunitWeb")]
		public void PostBackFireEvent_2 ()
		{
			WebTest t = new WebTest ("NoEventValidation.aspx");
			t.Invoker = PageInvoker.CreateOnInit (PostBackFireEvent_Init_2);
			string html = t.Run ();
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls.Add (GetDecoratedId (html, "UserName"));   //%24
			fr.Controls.Add (GetDecoratedId (html, "Password"));
			fr.Controls.Add (GetDecoratedId (html, "LoginButton"));
			fr.Controls ["__EVENTTARGET"].Value = "";
			fr.Controls ["__EVENTARGUMENT"].Value = "";
			fr.Controls ["Login1$UserName"].Value = "yonik";
			fr.Controls ["Login1$Password"].Value = "123456";
			fr.Controls ["Login1$LoginButton"].Value = "Log In";
			t.Request = fr;
			t.Run ();

			ArrayList eventlist = t.UserData as ArrayList;
			if (eventlist == null)
				Assert.Fail ("User data does not been created fail");

			Assert.AreEqual ("LoggingIn", eventlist [0], "#1");
			Assert.AreEqual ("LoggedIn", eventlist [1], "#2");
		}

		public static void PostBackFireEvent_Init_2 (Page p)
		{
			Login l = new Login ();
			l.LoggedIn += new EventHandler (l_LoggedIn);
			l.LoggingIn += new LoginCancelEventHandler (l_LoggingIn);
			l.ID = "Login1";
			l.MembershipProvider = "FakeProvider";
			p.Controls.Add (l);
			p.Validate ();
		}

		public static void PostBackFireEvent_Init (Page p)
		{
			Login l = new Login ();
			l.Authenticate += new AuthenticateEventHandler (Authenticate_Event);
			l.LoggedIn += new EventHandler (l_LoggedIn);
			l.LoggingIn += new LoginCancelEventHandler (l_LoggingIn);
			l.LoginError += new EventHandler (l_LoginError);
			l.ID = "Login1";
			l.MembershipProvider = "FakeProvider";
			p.Controls.Add (l);
			p.Validate ();
		}

		static void l_LoginError (object sender, EventArgs e)
		{
			if (WebTest.CurrentTest.UserData == null) {
				ArrayList list = new ArrayList ();
				list.Add ("LoginError");
				WebTest.CurrentTest.UserData = list;
			}
			else {
				ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
				if (list == null)
					throw new NullReferenceException ();
				list.Add ("LoginError");
				WebTest.CurrentTest.UserData = list;
			}
		}

		static void l_LoggingIn (object sender, LoginCancelEventArgs e)
		{
			if (WebTest.CurrentTest.UserData == null) {
				ArrayList list = new ArrayList ();
				list.Add ("LoggingIn");
				WebTest.CurrentTest.UserData = list;
			}
			else {
				ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
				if (list == null)
					throw new NullReferenceException ();
				list.Add ("LoggingIn");
				WebTest.CurrentTest.UserData = list;
			}
		}

		static void l_LoggedIn (object sender, EventArgs e)
		{
			if (WebTest.CurrentTest.UserData == null) {
				ArrayList list = new ArrayList ();
				list.Add ("LoggedIn");
				WebTest.CurrentTest.UserData = list;
			}
			else {
				ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
				if (list == null)
					throw new NullReferenceException ();
				list.Add ("LoggedIn");
				WebTest.CurrentTest.UserData = list;
			}
		}

		public static void Authenticate_Event (object sender, AuthenticateEventArgs e)
		{
			if (WebTest.CurrentTest.UserData == null) {
				ArrayList list = new ArrayList ();
				list.Add ("Authenticate");
				WebTest.CurrentTest.UserData = list;
			}
			else {
				ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
				if (list == null)
					throw new NullReferenceException ();
				list.Add ("Authenticate");
				WebTest.CurrentTest.UserData = list;
			}
		}

		[Test] // Bug #468359
		[Category ("NunitWeb")]
		public void DisplayRememberMe ()
		{
			WebTest t = new WebTest ("LoginDisplayRememberMe.aspx");
			string html = t.Run ();

			Assert.AreEqual (-1, html.IndexOf ("Login1_RememberMe"), "#A1");
		}
		
		[TestFixtureTearDown]
		public void Teardown ()
		{
			WebTest.Unload ();
		}

		private string GetDecoratedId (string html, string id)
		{
			Regex reg = new Regex ("name=\".*[\\$\\:]" + id + "\"");
			Match match = reg.Match (html);

			string fixedId = match.Value;
			if (fixedId.Length > 0)
				fixedId = fixedId.Substring (fixedId.IndexOf ("\""), fixedId.Length - fixedId.IndexOf ("\"")).Trim ('\"');

			return fixedId;
		}

	}
}

#endif
