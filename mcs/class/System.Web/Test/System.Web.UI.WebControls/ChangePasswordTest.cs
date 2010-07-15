//
// ChangePasswordTest.cs - Unit tests for System.Web.UI.WebControls.ChangePassword
//
// Author:
//	Vladimir Krasnov  <vladimirk@mainsoft.com>
//
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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
using System.Drawing;
using System.Threading;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Text.RegularExpressions;
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;
using MonoTests.Common;

using NUnit.Framework;
using System.Collections.Specialized;
using System.Web.Configuration;

namespace MonoTests.System.Web.UI.WebControls
{

	public class TestChangePassword : ChangePassword
	{

		public string Tag
		{
			get { return base.TagName; }
		}

		public StateBag StateBag
		{
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

		private bool onBubble;

		public bool OnBubbleEventCalled
		{
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

		public void DoEnsureChildControls ()
		{
			base.EnsureChildControls ();
		}

		public bool DoOnBubbleEvent (EventArgs e)
		{
			return base.OnBubbleEvent (this, e);
		}
	}

	[Serializable]
	[TestFixture]
	public class ChangePasswordTest
	{
		[TestFixtureSetUp]
		public void CopyTestResources ()
		{
			WebTest.CopyResource (GetType (), "ChangePasswordContainer_FindControl.aspx", "ChangePasswordContainer_FindControl.aspx");
		}

		[Test]
		public void DefaultProperties ()
		{
			CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
			CultureInfo currentUICulture = Thread.CurrentThread.CurrentUICulture;

			try {
				CultureInfo ci = CultureInfo.GetCultureInfo ("en-US");
				Thread.CurrentThread.CurrentCulture = ci;
				Thread.CurrentThread.CurrentUICulture = ci;
				RunDefaultPropertiesTests ();
			} finally {
				Thread.CurrentThread.CurrentCulture = currentCulture;
				Thread.CurrentThread.CurrentUICulture = currentUICulture;
			}
		}

		void RunDefaultPropertiesTests ()
		{
			TestChangePassword w = new TestChangePassword ();
			Assert.AreEqual (0, w.Attributes.Count, "Attributes.Count");
			Assert.AreEqual (0, w.StateBag.Count, "ViewState.Count");
			Assert.AreEqual (string.Empty, w.CancelButtonImageUrl, "CancelButtonImageUrl");
			Assert.AreEqual ("Cancel", w.CancelButtonText, "CancelButtonText");
			Assert.AreEqual (ButtonType.Button, w.CancelButtonType, "CancelButtonType");
			Assert.AreEqual (string.Empty, w.CancelDestinationPageUrl, "CancelDestinationPageUrl");
			Assert.AreEqual (string.Empty, w.ChangePasswordButtonImageUrl, "ChangePasswordButtonImageUrl");
			Assert.AreEqual ("Change Password", w.ChangePasswordButtonText, "ChangePasswordButtonText");
			Assert.AreEqual (ButtonType.Button, w.ChangePasswordButtonType, "ChangePasswordButtonType");
			Assert.AreEqual ("Change Your Password", w.ChangePasswordTitleText, "ChangePasswordTitleText");
			Assert.AreEqual (string.Empty, w.ConfirmNewPassword, "CompleteSuccessText");
			Assert.AreEqual (string.Empty, w.HelpPageIconUrl, "HelpPageIconUrl");
			Assert.AreEqual (string.Empty, w.HelpPageText, "HelpPageText");
			Assert.AreEqual (string.Empty, w.HelpPageUrl, "HelpPageUrl");
			Assert.AreEqual (string.Empty, w.CreateUserIconUrl, "CreateUserIconUrl");
			Assert.AreEqual (string.Empty, w.CreateUserText, "CreateUserText");
			Assert.AreEqual (string.Empty, w.CreateUserUrl, "CreateUserUrl");
			Assert.AreEqual (string.Empty, w.EditProfileIconUrl, "EditProfileIconUrl");
			Assert.AreEqual (string.Empty, w.EditProfileText, "EditProfileText");
			Assert.AreEqual (string.Empty, w.EditProfileUrl, "EditProfileUrl");
			Assert.AreEqual (string.Empty, w.PasswordRecoveryIconUrl, "PasswordRecoveryIconUrl");
			Assert.AreEqual (string.Empty, w.PasswordRecoveryText, "PasswordRecoveryText");
			Assert.AreEqual (string.Empty, w.PasswordRecoveryUrl, "PasswordRecoveryUrl");
			Assert.AreEqual ("Confirm New Password:", w.ConfirmNewPasswordLabelText, "ConfirmNewPasswordLabelText");
			Assert.AreEqual ("The Confirm New Password must match the New Password entry.", w.ConfirmPasswordCompareErrorMessage, "ConfirmPasswordCompareErrorMessage");
			Assert.AreEqual ("Confirm New Password is required.", w.ConfirmPasswordRequiredErrorMessage, "ConfirmPasswordRequiredErrorMessage");
			Assert.AreEqual (string.Empty, w.ContinueButtonImageUrl, "ContinueButtonImageUrl");
			Assert.AreEqual ("Continue", w.ContinueButtonText, "ContinueButtonText");
			Assert.AreEqual (ButtonType.Button, w.ContinueButtonType, "ContinueButtonType");
			Assert.AreEqual (string.Empty, w.ContinueDestinationPageUrl, "ContinueDestinationPageUrl");
			Assert.AreEqual (false, w.DisplayUserName, "DisplayUserName");
			Assert.AreEqual (string.Empty, w.InstructionText, "InstructionText");
			Assert.AreEqual ("New Password:", w.NewPasswordLabelText, "NewPasswordLabelText");
			Assert.AreEqual (string.Empty, w.PasswordHintText, "PasswordHintText");
			Assert.AreEqual ("Password:", w.PasswordLabelText, "PasswordLabelText");
			Assert.AreEqual (string.Empty, w.SuccessPageUrl, "SuccessPageUrl");
			Assert.AreEqual ("Your password has been changed!", w.SuccessText, "SuccessText");
			Assert.AreEqual ("Change Password Complete", w.SuccessTitleText, "SuccessTitleText");
			Assert.AreEqual ("User Name:", w.UserNameLabelText, "UserNameLabelText");
		}

		[Test]
		public void AssignToDefaultProperties ()
		{
			TestChangePassword w = new TestChangePassword ();
			Assert.AreEqual (0, w.Attributes.Count, "Attributes.Count");
			Assert.AreEqual (0, w.StateBag.Count, "ViewState.Count");

			int count = 0;

			w.CancelButtonImageUrl = "text";
			Assert.AreEqual ("text", w.CancelButtonImageUrl, "Assign CancelButtonImageUrl,");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate CancelButtonImageUrl,");

			w.CancelButtonText = "text";
			Assert.AreEqual ("text", w.CancelButtonText, "Assign CancelButtonText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate CancelButtonText");

			w.CancelDestinationPageUrl = "text";
			Assert.AreEqual ("text", w.CancelDestinationPageUrl, "Assign CancelDestinationPageUrl");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate CancelDestinationPageUrl");

			w.ChangePasswordButtonImageUrl = "text";
			Assert.AreEqual ("text", w.ChangePasswordButtonImageUrl, "Assign ChangePasswordButtonImageUrl");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate ChangePasswordButtonImageUrl");

			w.HelpPageText = "text";
			Assert.AreEqual ("text", w.HelpPageText, "Assign HelpPageText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate HelpPageText");

			w.ChangePasswordButtonText = "text";
			Assert.AreEqual ("text", w.ChangePasswordButtonText, "Assign ChangePasswordButtonText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate ChangePasswordButtonText");

			w.ChangePasswordFailureText = "text";
			Assert.AreEqual ("text", w.ChangePasswordFailureText, "Assign ChangePasswordFailureText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate ChangePasswordFailureText");

			w.ChangePasswordTitleText = "msg";
			Assert.AreEqual ("msg", w.ChangePasswordTitleText, "Assign ChangePasswordTitleText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate ChangePasswordTitleText");

			w.ConfirmNewPasswordLabelText = "msg";
			Assert.AreEqual ("msg", w.ConfirmNewPasswordLabelText, "Assign ConfirmNewPasswordLabelText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate ConfirmNewPasswordLabelText");

			w.ContinueButtonImageUrl = "msg";
			Assert.AreEqual ("msg", w.ContinueButtonImageUrl, "Assign ContinueButtonImageUrl");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate ContinueButtonImageUrl");

			w.ContinueButtonText = "msg";
			Assert.AreEqual ("msg", w.ContinueButtonText, "Assign ContinueButtonText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate ContinueButtonText");

			w.ContinueDestinationPageUrl = "msg";
			Assert.AreEqual ("msg", w.ContinueDestinationPageUrl, "Assign ContinueDestinationPageUrl");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate ContinueDestinationPageUrl");

			w.CreateUserIconUrl = "msg";
			Assert.AreEqual ("msg", w.CreateUserIconUrl, "Assign CreateUserIconUrl");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate CreateUserIconUrl");

			w.CreateUserText = "msg";
			Assert.AreEqual ("msg", w.CreateUserText, "Assign CreateUserText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate CreateUserText");

			w.CreateUserUrl = "msg";
			Assert.AreEqual ("msg", w.CreateUserUrl, "Assign CreateUserUrl");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate CreateUserUrl");

			w.DisplayUserName = true;
			Assert.AreEqual (true, w.DisplayUserName, "Assign DisplayUserName");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate DisplayUserName");

			w.EditProfileIconUrl = "msg";
			Assert.AreEqual ("msg", w.EditProfileIconUrl, "Assign EditProfileIconUrl");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate EditProfileIconUrl");

			w.EditProfileText = "msg";
			Assert.AreEqual ("msg", w.EditProfileText, "Assign EditProfileText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate EditProfileText");

			w.HelpPageIconUrl = "msg";
			Assert.AreEqual ("msg", w.HelpPageIconUrl, "Assign HelpPageIconUrl");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate HelpPageIconUrl");

			w.HelpPageUrl = "msg";
			Assert.AreEqual ("msg", w.HelpPageUrl, "Assign HelpPageUrl");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate HelpPageUrl");

			w.InstructionText = "msg";
			Assert.AreEqual ("msg", w.InstructionText, "Assign InstructionText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate InstructionText");

			w.MembershipProvider = "msg";
			Assert.AreEqual ("msg", w.MembershipProvider, "Assign MembershipProvider");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate MembershipProvider");

			w.NewPasswordLabelText = "msg";
			Assert.AreEqual ("msg", w.NewPasswordLabelText, "Assign NewPasswordLabelText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate NewPasswordLabelText");

			w.PasswordHintText = "msg";
			Assert.AreEqual ("msg", w.PasswordHintText, "Assign PasswordHintText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate PasswordHintText");

			w.PasswordLabelText = "msg";
			Assert.AreEqual ("msg", w.PasswordLabelText, "Assign PasswordLabelText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate PasswordLabelText");

			w.PasswordRecoveryIconUrl = "msg";
			Assert.AreEqual ("msg", w.PasswordRecoveryIconUrl, "Assign PasswordRecoveryIconUrl");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate PasswordRecoveryIconUrl");

			w.PasswordRecoveryText = "msg";
			Assert.AreEqual ("msg", w.PasswordRecoveryText, "Assign PasswordRecoveryText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate PasswordRecoveryText");

			w.PasswordRecoveryUrl = "msg";
			Assert.AreEqual ("msg", w.PasswordRecoveryUrl, "Assign PasswordRecoveryUrl");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate PasswordRecoveryUrl");

			w.PasswordRequiredErrorMessage = "msg";
			Assert.AreEqual ("msg", w.PasswordRequiredErrorMessage, "Assign PasswordRequiredErrorMessage");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate PasswordRequiredErrorMessage");

			w.SuccessPageUrl = "msg";
			Assert.AreEqual ("msg", w.SuccessPageUrl, "Assign SuccessPageUrl");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate SuccessPageUrl");

			w.SuccessText = "msg";
			Assert.AreEqual ("msg", w.SuccessText, "Assign SuccessText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate SuccessText");

			w.SuccessTitleText = "msg";
			Assert.AreEqual ("msg", w.SuccessTitleText, "Assign SuccessTitleText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate SuccessTitleText");

			w.UserNameLabelText = "msg";
			Assert.AreEqual ("msg", w.UserNameLabelText, "Assign UserNameLabelText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate UserNameLabelText");
		}

		public static void BasicRenderTestInit (Page p)
		{
			CreateTestControl (p);
		}

		public static ChangePassword CreateTestControl (Page p)
		{
			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);

			ChangePassword w = new ChangePassword ();
			w.ID = "ChangePassword1";
			w.DisplayUserName = true;
			w.MembershipProvider = "FakeProvider";
			p.Form.Controls.Add (lcb);
			p.Form.Controls.Add (w);
			p.Form.Controls.Add (lce);

			return w;
		}

		[Test]
		[Category ("NunitWeb")]
		public void BasicRenderTest ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (BasicRenderTestInit))).Run ();

			int st = 0;
			Assert.IsTrue ((st = html.IndexOf ("<table", st)) > 0, "base render test 1");
			Assert.IsTrue ((st = html.IndexOf ("ChangePassword1", st)) > 0, "base render test 2");
			Assert.IsTrue ((st = html.IndexOf ("border-collapse:collapse", st)) > 0, "base render test 3");
			Assert.IsTrue ((st = html.IndexOf ("<table", st)) > 0, "base render test 4");
			Assert.IsTrue ((st = html.IndexOf ("Change Your Password", st)) > 0, "base render test 5");
			Assert.IsTrue ((st = html.IndexOf ("User Name:", st)) > 0, "base render test 6");
			Assert.IsTrue ((st = html.IndexOf ("Password:", st)) > 0, "base render test 7");
			Assert.IsTrue ((st = html.IndexOf ("New Password:", st)) > 0, "base render test 8");
			Assert.IsTrue ((st = html.IndexOf ("Change Password", st)) > 0, "base render test 9");
		}

		[Test]
		[Category ("NunitWeb")]
		public void TitlesRenderTest ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (TitlesRenderTestInit))).Run ();

			Assert.IsTrue (html.IndexOf ("userid") > 0, "UserNameLabelText");
			Assert.IsTrue (html.IndexOf ("PasswordLabelText") > 0, "PasswordLabelText");
			Assert.IsTrue (html.IndexOf ("NewPasswordLabelText") > 0, "NewPasswordLabelText");
			Assert.IsTrue (html.IndexOf ("ConfirmNewPasswordLabelText") > 0, "ConfirmNewPasswordLabelText");
			Assert.IsTrue (html.IndexOf ("InstructionText") > 0, "InstructionText");
			Assert.IsTrue (html.IndexOf ("PasswordHintText") > 0, "PasswordHintText");
			Assert.IsTrue (html.IndexOf ("zzxcmnmncx") > 0, "zzxcmnmncx");
		}

		public static void TitlesRenderTestInit (Page p)
		{
			ChangePassword w = CreateTestControl (p);
			w.UserNameLabelText = "userid";
			w.PasswordLabelText = "PasswordLabelText";
			w.NewPasswordLabelText = "NewPasswordLabelText";
			w.ConfirmNewPasswordLabelText = "ConfirmNewPasswordLabelText";
			w.InstructionText = "InstructionText";
			w.PasswordHintText = "PasswordHintText";
			w.ChangePasswordButtonText = "zzxcmnmncx";
		}

		[Test]
		[Category ("NunitWeb")]
		public void ExtraTitlesRenderTest ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (ExtraTitlesRenderTestInit))).Run ();

			Assert.IsTrue (html.IndexOf ("PasswordHintText") > 0, "PasswordHintText");
			Assert.IsTrue (html.IndexOf ("http://www.HelpPageUrl.com") > 0, "HelpPageUrl");
			Assert.IsTrue (html.IndexOf ("HelpPageText") > 0, "HelpPageText");
			Assert.IsTrue (html.IndexOf ("http://www.HelpPageIconUrl.com") > 0, "HelpPageIconUrl");
			Assert.IsTrue (html.IndexOf ("CreateUserIconUrl") > 0, "CreateUserIconUrl");
			Assert.IsTrue (html.IndexOf ("CreateUserText") > 0, "CreateUserText");
			Assert.IsTrue (html.IndexOf ("CreateUserUrl") > 0, "CreateUserUrl");
		}

		public static void ExtraTitlesRenderTestInit (Page p)
		{
			ChangePassword w = CreateTestControl (p);
			w.HelpPageUrl = "http://www.HelpPageUrl.com";
			w.HelpPageText = "HelpPageText";
			w.HelpPageIconUrl = "http://www.HelpPageIconUrl.com";
			w.CreateUserIconUrl = "CreateUserIconUrl";
			w.CreateUserText = "CreateUserText";
			w.CreateUserUrl = "CreateUserUrl";
			w.PasswordHintText = "PasswordHintText";
		}

		[Test]
		[Category ("NunitWeb")]
		public void StylesRenderTest ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (StylesRenderTestInit))).Run ();

			Assert.IsTrue (html.IndexOf ("LightGoldenrodYellow;") > 0, "TextBoxStyle");
			Assert.IsTrue (html.IndexOf ("732px") > 0, "TitleTextStyle");
			Assert.IsTrue (html.IndexOf ("LightSkyBlue;") > 0, "HyperLinkStyle");
			Assert.IsTrue (html.IndexOf ("MediumSeaGreen;") > 0, "InstructionTextStyle");
			Assert.IsTrue (html.IndexOf ("MediumSpringGreen;") > 0, "LabelStyle");
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

		private static string GetEventTarget (string html, string id)
		{
			Regex reg = new Regex ("__doPostBack.*\\(.*'.*" + id + "'");
			Match match = reg.Match (html);

			string fixedId = match.Value;
			if (fixedId.Length > 0)
				fixedId = fixedId.Substring (fixedId.IndexOf ("'"), fixedId.Length - fixedId.IndexOf ("'")).Trim ('\'');

			return fixedId;
		}

		[Test]
		[Category ("NunitWeb")]
		public void BasicPostbackTest ()
		{
			PageInvoker pi = PageInvoker.CreateOnLoad (new PageDelegate (StylesRenderTestInit));
			WebTest test = new WebTest (pi);

			string html = test.Run ();
			test.Invoker = pi;

			FormRequest fr = new FormRequest (test.Response, "form1");

			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "UserName"), "username"));

			PageDelegates pd = new PageDelegates ();
			pd.PreRender = new PageDelegate (BasicPostTestPreRender);
			pd.Load = new PageDelegate (StylesRenderTestInit);
			pi.Delegates = pd;

			test.Request = fr;
			html = test.Run ();

			Assert.IsTrue (html.IndexOf ("username") > 0, "rendered user name");

			Assert.IsTrue (html.IndexOf ("LightGoldenrodYellow;") > 0, "TextBoxStyle");
			Assert.IsTrue (html.IndexOf ("732px") > 0, "TitleTextStyle");
			Assert.IsTrue (html.IndexOf ("LightSkyBlue;") > 0, "HyperLinkStyle");
			Assert.IsTrue (html.IndexOf ("MediumSeaGreen;") > 0, "InstructionTextStyle");
			Assert.IsTrue (html.IndexOf ("MediumSpringGreen;") > 0, "LabelStyle");
			Assert.IsTrue (html.IndexOf ("PeachPuff;") > 0, "LabelStyle");
		}

		public static void BasicPostTestPreRender (Page p)
		{
			ChangePassword w = (ChangePassword) p.FindControl ("ChangePassword1");
			if (w == null)
				Assert.Fail ("postback1");

			Assert.AreEqual ("username", w.UserName, "posted user name");
		}

		public static void StylesRenderTestInit (Page p)
		{
			ChangePassword w = CreateTestControl (p);
			w.MembershipProvider = "FakeProvider";
			w.SendingMail += new MailMessageEventHandler(w_SendingMail);
			w.SendMailError += new SendMailErrorEventHandler(w_SendMailError);
			w.DisplayUserName = true;

			if (!p.IsPostBack) {
				w.TextBoxStyle.BackColor = Color.LightGoldenrodYellow;
				w.TitleTextStyle.Height = Unit.Pixel (732);
				w.LabelStyle.BackColor = Color.MediumSpringGreen;

				w.HelpPageUrl = "http://www.HelpPageUrl.com";
				w.HelpPageText = "hhh";
				w.HyperLinkStyle.BackColor = Color.LightSkyBlue;

				w.InstructionText = "text";
				w.InstructionTextStyle.BackColor = Color.MediumSeaGreen;

				w.PasswordHintText = "PasswordHintText";
				w.PasswordHintStyle.BackColor = Color.PeachPuff;
			}
		}

		public static void w_SendingMail (object sender, MailMessageEventArgs e)
		{
			if (e.Message.Body.IndexOf ("123") > 0)
				WebTest.CurrentTest.UserData = "w_SendingMail";
		}
		
		public static void w_SendMailError (object sender, SendMailErrorEventArgs e)
		{
			e.Handled = true;
		}

		// TODO:
		// ValidatorTextStyle
		// ErrorMessageStyle
		[Test]
		[Category ("NotDotNet")]
		[Category ("NunitWeb")]
		public void ChngPasswordTest ()
		{
			PageInvoker pi = PageInvoker.CreateOnLoad (new PageDelegate (StylesRenderTestInit));
			WebTest test = new WebTest (pi);

			string html = test.Run ();
			test.Invoker = pi;

			FormRequest fr = new FormRequest (test.Response, "form1");

			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "UserName"), "heh"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "CurrentPassword"), "heh"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "NewPassword"), "hehe"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "ConfirmNewPassword"), "hehe"));
			string button = GetDecoratedId (html, "ChangePasswordPushButton");
			if (button.Length > 0)
				fr.Controls.Add (new BaseControl (GetDecoratedId (html, "ChangePasswordPushButton"), "ChangePasswordPushButton"));
			else
				fr.Controls.Add (new BaseControl ("__EVENTTARGET", GetEventTarget (html, "ChangePassword")));

			test.Request = fr;
			html = test.Run ();
			Assert.IsTrue (html.IndexOf ("has been changed") > 0, "GetPassword");
		}

		[Test]
		[Category ("NunitWeb")]
		public void PostBackEventCancel ()
		{
			WebTest t = new WebTest ();
			t.Invoker = PageInvoker.CreateOnInit (new PageDelegate (_PostBackEventCancel));
			string html = t.Run ();
			if (html.IndexOf ("Change Your Password") < 0)
				Assert.Fail ("ChangePassword not created");
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls.Add ("ChangePassword1$ChangePasswordContainerID$CurrentPassword");
			fr.Controls.Add ("ChangePassword1$ChangePasswordContainerID$NewPassword");
			fr.Controls.Add ("ChangePassword1$ChangePasswordContainerID$ConfirmNewPassword");
			fr.Controls.Add ("ChangePassword1$ChangePasswordContainerID$CancelPushButton");
			fr.Controls["ChangePassword1$ChangePasswordContainerID$CancelPushButton"].Value = "Cancel";
			t.Request = fr;
			html = t.Run ();
			if (t.UserData == null || t.UserData.ToString () != "CancelButtonClick")
				Assert.Fail ("CancelButtonClick event not fired");
		}

		public static void _PostBackEventCancel (Page p)
		{
			ChangePassword w = CreateTestControl (p);
			w.CancelButtonClick += new EventHandler (w_CancelButtonClick);
		}

		static void w_CancelButtonClick (object sender, EventArgs e)
		{
			WebTest.CurrentTest.UserData = "CancelButtonClick";
		}


		[Test]
		[Category ("NunitWeb")]
		public void PostBackEventChanging ()
		{
			WebTest t = new WebTest ();
			t.Invoker = PageInvoker.CreateOnInit (new PageDelegate (_PostBackEventChanging));
			string html = t.Run ();
			if (html.IndexOf ("Change Your Password") < 0)
				Assert.Fail ("ChangePassword not created");
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			
			fr.Controls.Add ("ChangePassword1$ChangePasswordContainerID$CurrentPassword");
			fr.Controls.Add ("ChangePassword1$ChangePasswordContainerID$NewPassword");
			fr.Controls.Add ("ChangePassword1$ChangePasswordContainerID$ConfirmNewPassword");
			
			fr.Controls["ChangePassword1$ChangePasswordContainerID$CurrentPassword"].Value = "a";
			fr.Controls["ChangePassword1$ChangePasswordContainerID$NewPassword"].Value = "1";
			fr.Controls["ChangePassword1$ChangePasswordContainerID$ConfirmNewPassword"].Value = "1";
			fr.Controls.Add ("ChangePassword1$ChangePasswordContainerID$ChangePasswordPushButton");
			fr.Controls["ChangePassword1$ChangePasswordContainerID$ChangePasswordPushButton"].Value = "Change+Password";
			t.Request = fr;
			html = t.Run ();
			if (t.UserData == null || t.UserData.ToString () != "ChangingPassword")
				Assert.Fail ("ChangingPassword event not fired");
		}

		public static void _PostBackEventChanging (Page p)
		{
			ChangePassword w = CreateTestControl (p);
			w.DisplayUserName = false;
			w.ChangingPassword +=new LoginCancelEventHandler(w_ChangingPassword);
		}

		public static void w_ChangingPassword (object sender, LoginCancelEventArgs e)
		{
			WebTest.CurrentTest.UserData = "ChangingPassword";
			e.Cancel = true;
		}

		[Test]
		[Category ("NunitWeb")]
		public void PostBackEventError ()
		{
			WebTest t = new WebTest ();
			t.Invoker = PageInvoker.CreateOnInit (new PageDelegate (_PostBackEventError));
			string html = t.Run ();
			if (html.IndexOf ("Change Your Password") < 0)
				Assert.Fail ("ChangePassword not created");
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");

			fr.Controls.Add ("ChangePassword1$ChangePasswordContainerID$UserName");
			fr.Controls.Add ("ChangePassword1$ChangePasswordContainerID$CurrentPassword");
			fr.Controls.Add ("ChangePassword1$ChangePasswordContainerID$NewPassword");
			fr.Controls.Add ("ChangePassword1$ChangePasswordContainerID$ConfirmNewPassword");

			fr.Controls["ChangePassword1$ChangePasswordContainerID$UserName"].Value = "WrongUser";
			fr.Controls["ChangePassword1$ChangePasswordContainerID$CurrentPassword"].Value = "a";
			fr.Controls["ChangePassword1$ChangePasswordContainerID$NewPassword"].Value = "1";
			fr.Controls["ChangePassword1$ChangePasswordContainerID$ConfirmNewPassword"].Value = "1";
			fr.Controls.Add ("ChangePassword1$ChangePasswordContainerID$ChangePasswordPushButton");
			fr.Controls["ChangePassword1$ChangePasswordContainerID$ChangePasswordPushButton"].Value = "Change+Password";
			t.Request = fr;
			html = t.Run ();
			if (t.UserData == null || t.UserData.ToString () != "ChangePasswordError")
				Assert.Fail ("ChangePasswordError event not fired");
		}

		public static void _PostBackEventError (Page p)
		{
			ChangePassword w = CreateTestControl (p);
			w.ChangePasswordError += new EventHandler (w_ChangePasswordError);
		}

		public static void w_ChangePasswordError (object sender, EventArgs e)
		{
			WebTest.CurrentTest.UserData = "ChangePasswordError";
		}

		
		[Test]
		[Category ("NunitWeb")]
		public void PostBackEventChangedPassword ()
		{
			WebTest t = new WebTest ();
			t.Invoker = PageInvoker.CreateOnInit (new PageDelegate (_PostBackEventChangedPassword));
			string html = t.Run ();
			if (html.IndexOf ("Change Your Password") < 0)
				Assert.Fail ("ChangePassword not created");
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");

			fr.Controls.Add ("ChangePassword1$ChangePasswordContainerID$CurrentPassword");
			fr.Controls.Add ("ChangePassword1$ChangePasswordContainerID$NewPassword");
			fr.Controls.Add ("ChangePassword1$ChangePasswordContainerID$ConfirmNewPassword");

			fr.Controls["ChangePassword1$ChangePasswordContainerID$CurrentPassword"].Value = "p@ssword";
			fr.Controls["ChangePassword1$ChangePasswordContainerID$NewPassword"].Value = "123456?";
			fr.Controls["ChangePassword1$ChangePasswordContainerID$ConfirmNewPassword"].Value = "123456?";
			fr.Controls.Add ("ChangePassword1$ChangePasswordContainerID$ChangePasswordPushButton");
			fr.Controls["ChangePassword1$ChangePasswordContainerID$ChangePasswordPushButton"].Value = "Change+Password";
			t.Request = fr;
			html = t.Run ();
			if (t.UserData == null || t.UserData.ToString () != "ChangedPassword")
				Assert.Fail ("ChangedPassword event not fired");
		}

		public static void _PostBackEventChangedPassword (Page p)
		{
			ChangePassword w = CreateTestControl (p);
			w.DisplayUserName = false;
			w.ChangedPassword += new EventHandler (w_ChangedPassword);
		}

		public static void w_ChangedPassword (object sender, EventArgs e)
		{
			WebTest.CurrentTest.UserData = "ChangedPassword";
		}

		[Test]
		[Category ("NunitWeb")]
		public void PostBackEventContinue ()
		{
			WebTest t = new WebTest ();
			t.Invoker = PageInvoker.CreateOnInit (new PageDelegate (_PostBackEventContinue));
			string html = t.Run ();
			if (html.IndexOf ("Change Your Password") < 0)
				Assert.Fail ("ChangePassword not created");
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");

			fr.Controls.Add ("ChangePassword1$ChangePasswordContainerID$CurrentPassword");
			fr.Controls.Add ("ChangePassword1$ChangePasswordContainerID$NewPassword");
			fr.Controls.Add ("ChangePassword1$ChangePasswordContainerID$ConfirmNewPassword");
			fr.Controls["ChangePassword1$ChangePasswordContainerID$CurrentPassword"].Value = "p@ssword";
			fr.Controls["ChangePassword1$ChangePasswordContainerID$NewPassword"].Value = "123456?";
			fr.Controls["ChangePassword1$ChangePasswordContainerID$ConfirmNewPassword"].Value = "123456?";
			fr.Controls.Add ("ChangePassword1$ChangePasswordContainerID$ChangePasswordPushButton");
			fr.Controls["ChangePassword1$ChangePasswordContainerID$ChangePasswordPushButton"].Value = "Change+Password";

			t.Request = fr;
			html = t.Run ();
			if (html.IndexOf ("Change Password Complete") < 0)
				Assert.Fail ("Password has not been changed!");
			
			fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls.Add ("ChangePassword1$SuccessContainerID$ContinuePushButton");
			fr.Controls["ChangePassword1$SuccessContainerID$ContinuePushButton"].Value = "Continue";

			t.Request = fr;
			html = t.Run ();

			if (t.UserData == null || t.UserData.ToString () != "ContinueButtonClick")
				Assert.Fail ("ContinueButtonClick event not fired");
		}

		public static void _PostBackEventContinue (Page p)
		{
			ChangePassword w = CreateTestControl (p);
			w.DisplayUserName = false;
			w.ContinueButtonClick += new EventHandler (w_ContinueButtonClick);
		}

		public static void w_ContinueButtonClick (object sender, EventArgs e)
		{
			WebTest.CurrentTest.UserData = "ContinueButtonClick";
		}

		[Test]
		[Category ("NunitWeb")]
		public void DefaultProvider ()
		{
			WebTest t = new WebTest ();
			t.Invoker = PageInvoker.CreateOnInit (new PageDelegate (_DefaultProvider));
			string html = t.Run ();
		}
		
		public static void _DefaultProvider (Page p)
		{
			MembershipSection section = (MembershipSection) WebConfigurationManager.GetSection ("system.web/membership");
			Assert.AreEqual (section.DefaultProvider, "FakeProvider", "section.DefaultProvider");
			Assert.AreEqual (Membership.Provider.GetType (), typeof (FakeMembershipProvider), "Membership.Provider.GetType ()");
		}

		[Test]
		public void ChangePasswordContainer_FindControl ()
		{
			WebTest t = new WebTest ("ChangePasswordContainer_FindControl.aspx");
			t.Invoker = PageInvoker.CreateOnLoad (new PageDelegate (ChangePasswordContainer_FindControl_OnLoad));
			t.Run ();
		}

		public static void ChangePasswordContainer_FindControl_OnLoad (Page p)
		{
			ChangePassword cp = p.FindControl ("ChangePassword1") as ChangePassword;
			Assert.IsNotNull (cp, "#A1");
			
			RequiredFieldValidator rfv = cp.ChangePasswordTemplateContainer.FindControl ("text1required") as RequiredFieldValidator;
			Assert.IsNotNull (rfv, "#A2");
		}
#if NET_4_0
		[Test]
		public void RenderOuterTableForbiddenStyles ()
		{
			var cp = new ChangePassword ();
			cp.RenderOuterTable = false;
			cp.BackColor = Color.Red;

			TestRenderFailure (cp, "BackColor");

			cp = new ChangePassword ();
			cp.RenderOuterTable = false;
			cp.BorderColor = Color.Red;
			TestRenderFailure (cp, "BorderColor");

			cp = new ChangePassword ();
			cp.RenderOuterTable = false;
			cp.BorderStyle = BorderStyle.Dashed;
			TestRenderFailure (cp, "BorderStyle");

			cp = new ChangePassword ();
			cp.RenderOuterTable = false;
			cp.BorderWidth = new Unit (10, UnitType.Pixel);
			TestRenderFailure (cp, "BorderWidth");

			cp = new ChangePassword ();
			cp.RenderOuterTable = false;
			cp.CssClass = "MyClass";
			TestRenderFailure (cp, "CssClass");

			cp = new ChangePassword ();
			cp.RenderOuterTable = false;
			cp.Font.Bold = true;

			TestRenderFailure (cp, "Font", false);

			cp = new ChangePassword ();
			cp.RenderOuterTable = false;
			cp.ForeColor = Color.Red;
			TestRenderFailure (cp, "ForeColor");

			cp = new ChangePassword ();
			cp.RenderOuterTable = false;
			cp.Height = new Unit (20, UnitType.Pixel);
			TestRenderFailure (cp, "Height");

			cp = new ChangePassword ();
			cp.RenderOuterTable = false;
			cp.Width = new Unit (20, UnitType.Pixel);
			TestRenderFailure (cp, "Width");
		}

		void TestRenderFailure (ChangePassword cp, string message, bool shouldFail = true)
		{
			using (var sw = new StringWriter ()) {
				using (var w = new HtmlTextWriter (sw)) {
					if (shouldFail)
						AssertExtensions.Throws<InvalidOperationException> (() => {
							cp.RenderControl (w);
						}, message);
					else {
						cp.RenderControl (w);
						Assert.IsTrue (sw.ToString ().Length > 0, message);
					}
				}
			}
		}
#endif
		[TestFixtureTearDown]
		public void TearDown ()
		{
			WebTest.Unload ();
		}
	}
}

#endif
