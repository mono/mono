//
// WizardTest.cs - Unit tests for System.Web.UI.WebControls.Wizard
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
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Text.RegularExpressions;
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;

using NUnit.Framework;

namespace MonoTests.System.Web.UI.WebControls
{

	public class TestCreateUserWizard : CreateUserWizard
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

		public int ActiveStepIndex_Before_Init;
		public int ActiveStepIndex_After_Init;
		protected internal override void OnInit (EventArgs e) {
			ActiveStepIndex_Before_Init = ActiveStepIndex;
			base.OnInit (e);
			ActiveStepIndex_After_Init = ActiveStepIndex;
		}
	}

	[Serializable]
	[TestFixture]
	public class CreateUserWizardTest
	{

		[TestFixtureTearDown]
		public void Unload ()
		{
			WebTest.Unload ();
		}

		[Test]
		[Category ("NunitWeb")]
#if TARGET_JVM
		[Ignore ("TD #7024")]
#endif
		public void ActiveStepIndex () {
			new WebTest (PageInvoker.CreateOnLoad (ActiveStepIndex_Load)).Run ();
		}

		public static void ActiveStepIndex_Load (Page p) {
			TestCreateUserWizard wizard = new TestCreateUserWizard ();
			p.Controls.Add (wizard);
			Assert.AreEqual (-1, wizard.ActiveStepIndex_Before_Init, "ActiveStepIndex_Before_Init #1");
			Assert.AreEqual (0, wizard.ActiveStepIndex_After_Init, "ActiveStepIndex_After_Init #1");
			Assert.AreEqual (2, wizard.WizardSteps.Count);

			wizard = new TestCreateUserWizard ();
			wizard.ActiveStepIndex = 1;
			p.Controls.Add (wizard);
			Assert.AreEqual (1, wizard.ActiveStepIndex_Before_Init, "ActiveStepIndex_Before_Init #2");
			Assert.AreEqual (1, wizard.ActiveStepIndex_After_Init, "ActiveStepIndex_After_Init #2");
			Assert.AreEqual (2, wizard.WizardSteps.Count);
		}

		[Test]
		public void DefaultProperties ()
		{
			TestCreateUserWizard w = new TestCreateUserWizard ();
			Assert.AreEqual (0, w.Attributes.Count, "Attributes.Count");
			Assert.AreEqual (0, w.StateBag.Count, "ViewState.Count");

			Assert.AreEqual ("Security Answer:", w.AnswerLabelText, "AnswerLabelText");
			Assert.AreEqual ("Security answer is required.", w.AnswerRequiredErrorMessage, "AnswerRequiredErrorMessage");
			Assert.IsFalse (w.AutoGeneratePassword, "AutoGeneratePassword");
			Assert.AreEqual ("Your account has been successfully created.", w.CompleteSuccessText, "CompleteSuccessText");
			Assert.AreEqual ("The Password and Confirmation Password must match.", w.ConfirmPasswordCompareErrorMessage, "ConfirmPasswordCompareErrorMessage");
			Assert.AreEqual ("Confirm Password:", w.ConfirmPasswordLabelText, "ConfirmPasswordLabelText");
			Assert.AreEqual ("Confirm Password is required.", w.ConfirmPasswordRequiredErrorMessage, "ConfirmPasswordRequiredErrorMessage");
			Assert.AreEqual (string.Empty, w.ContinueButtonImageUrl, "ContinueButtonImageUrl");
			Assert.AreEqual ("Continue", w.ContinueButtonText, "ContinueButtonText");
			Assert.AreEqual (ButtonType.Button, w.ContinueButtonType, "ContinueButtonType");
			Assert.AreEqual (string.Empty, w.ContinueDestinationPageUrl, "ContinueDestinationPageUrl");
			Assert.AreEqual (string.Empty, w.CreateUserButtonImageUrl, "CreateUserButtonImageUrl");
			Assert.AreEqual ("Create User", w.CreateUserButtonText, "CreateUserButtonText");
			Assert.AreEqual (ButtonType.Button, w.CreateUserButtonType, "CreateUserButtonType");
			Assert.IsFalse (w.DisableCreatedUser, "DisableCreatedUser");
			Assert.AreEqual ("The e-mail address that you entered is already in use. Please enter a different e-mail address.", w.DuplicateEmailErrorMessage, "DuplicateEmailErrorMessage");
			Assert.AreEqual ("Please enter a different user name.", w.DuplicateUserNameErrorMessage, "DuplicateUserNameErrorMessage");
			Assert.AreEqual (string.Empty, w.EditProfileIconUrl, "EditProfileIconUrl");
			Assert.AreEqual (string.Empty, w.EditProfileText, "EditProfileText");
			Assert.AreEqual (string.Empty, w.EditProfileUrl, "EditProfileUrl");
			Assert.AreEqual ("E-mail:", w.EmailLabelText, "EmailLabelText");
			Assert.AreEqual (string.Empty, w.EmailRegularExpression, "EmailRegularExpression");
			Assert.AreEqual ("Please enter a different e-mail.", w.EmailRegularExpressionErrorMessage, "EmailRegularExpressionErrorMessage");
			Assert.AreEqual ("E-mail is required.", w.EmailRequiredErrorMessage, "EmailRequiredErrorMessage");
			Assert.AreEqual (string.Empty, w.HelpPageIconUrl, "HelpPageIconUrl");
			Assert.AreEqual (string.Empty, w.HelpPageText, "HelpPageText");
			Assert.AreEqual (string.Empty, w.HelpPageUrl, "HelpPageUrl");
			Assert.AreEqual (string.Empty, w.InstructionText, "InstructionText");
			Assert.AreEqual ("Please enter a different security answer.", w.InvalidAnswerErrorMessage, "InvalidAnswerErrorMessage");
			Assert.AreEqual ("Please enter a valid e-mail address.", w.InvalidEmailErrorMessage, "InvalidEmailErrorMessage");
			Assert.AreEqual ("Password length minimum: {0}. Non-alphanumeric characters required: {1}.", w.InvalidPasswordErrorMessage, "InvalidPasswordErrorMessage");
			Assert.AreEqual ("Please enter a different security question.", w.InvalidQuestionErrorMessage, "InvalidQuestionErrorMessage");
			Assert.IsTrue (w.LoginCreatedUser, "LoginCreatedUser");
			Assert.AreEqual (string.Empty, w.MembershipProvider, "MembershipProvider");
			Assert.AreEqual (string.Empty, w.PasswordHintText, "PasswordHintText");
			Assert.AreEqual ("Password:", w.PasswordLabelText, "PasswordLabelText");
			Assert.AreEqual (string.Empty, w.PasswordRegularExpression, "PasswordRegularExpression");
			Assert.AreEqual ("Please enter a different password.", w.PasswordRegularExpressionErrorMessage, "PasswordRegularExpressionErrorMessage");
			Assert.AreEqual ("Password is required.", w.PasswordRequiredErrorMessage, "PasswordRequiredErrorMessage");
			Assert.AreEqual ("Security Question:", w.QuestionLabelText, "QuestionLabelText");
			Assert.AreEqual ("Security question is required.", w.QuestionRequiredErrorMessage, "QuestionRequiredErrorMessage");
			Assert.IsTrue (w.RequireEmail, "RequireEmail");
			Assert.AreEqual ("Your account was not created. Please try again.", w.UnknownErrorMessage, "UnknownErrorMessage");
			Assert.AreEqual ("User Name:", w.UserNameLabelText, "UserNameLabelText");
			Assert.AreEqual ("User Name is required.", w.UserNameRequiredErrorMessage, "UserNameRequiredErrorMessage");
			Assert.IsNotNull (w.WizardSteps, "WizardSteps");
		}

		[Test]
		public void AssignToDefaultProperties ()
		{
			TestCreateUserWizard w = new TestCreateUserWizard ();
			Assert.AreEqual (0, w.Attributes.Count, "Attributes.Count");
			Assert.AreEqual (0, w.StateBag.Count, "ViewState.Count");

			int count = 0;

			w.AnswerLabelText = "value";
			Assert.AreEqual ("value", w.AnswerLabelText, "CancelButtonImageUrl");
			Assert.AreEqual (++count, w.StateBag.Count, "ViewState.Count-1");

			w.AutoGeneratePassword = true;
			Assert.AreEqual (true, w.AutoGeneratePassword, "Assign AutoGeneratePassword");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate AutoGeneratePassword");

			w.CompleteSuccessText = "text";
			Assert.AreEqual ("text", w.CompleteSuccessText, "Assign CompleteSuccessText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate CompleteSuccessText");

			w.ConfirmPasswordCompareErrorMessage = "text";
			Assert.AreEqual ("text", w.ConfirmPasswordCompareErrorMessage, "Assign ConfirmPasswordCompareErrorMessage,");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate ConfirmPasswordCompareErrorMessage,");

			w.ConfirmPasswordLabelText = "text";
			Assert.AreEqual ("text", w.ConfirmPasswordLabelText, "Assign ConfirmPasswordLabelText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate ConfirmPasswordLabelText");

			w.ConfirmPasswordRequiredErrorMessage = "text";
			Assert.AreEqual ("text", w.ConfirmPasswordRequiredErrorMessage, "Assign ConfirmPasswordRequiredErrorMessage");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate ConfirmPasswordRequiredErrorMessage");

			w.ContinueButtonImageUrl = "text";
			Assert.AreEqual ("text", w.ContinueButtonImageUrl, "Assign ContinueButtonImageUrl");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate ContinueButtonImageUrl");

			w.ContinueButtonText = "text";
			Assert.AreEqual ("text", w.ContinueButtonText, "Assign ContinueButtonText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate ContinueButtonText");

			//w.ContinueButtonType = ButtonType.Button;
			//Assert.AreEqual(ButtonType.Button, w.ContinueButtonType, "Assign ContinueButtonType");
			//Assert.AreEqual(count, w.StateBag.Count, "Viewstate ContinueButtonType");

			w.ContinueDestinationPageUrl = "text";
			Assert.AreEqual ("text", w.ContinueDestinationPageUrl, "Assign ContinueDestinationPageUrl");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate ContinueDestinationPageUrl");

			w.CreateUserButtonImageUrl = "text";
			Assert.AreEqual ("text", w.CreateUserButtonImageUrl, "Assign CreateUserButtonImageUrl");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate CreateUserButtonImageUrl");

			w.CreateUserButtonText = "text";
			Assert.AreEqual ("text", w.CreateUserButtonText, "Assign CreateUserButtonText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate CreateUserButtonText");

			//w.CreateUserButtonType = ButtonType.Button;
			//Assert.AreEqual(ButtonType.Button, w.CreateUserButtonType, "Assign CreateUserButtonType");
			//Assert.AreEqual(count, w.StateBag.Count, "Viewstate CreateUserButtonType");

			w.DisableCreatedUser = false;
			Assert.AreEqual (false, w.DisableCreatedUser, "Assign DisableCreatedUser");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate DisableCreatedUser");

			w.DuplicateEmailErrorMessage = "msg";
			Assert.AreEqual ("msg", w.DuplicateEmailErrorMessage, "Assign DuplicateEmailErrorMessage");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate DuplicateEmailErrorMessage");

			w.DuplicateUserNameErrorMessage = "msg";
			Assert.AreEqual ("msg", w.DuplicateUserNameErrorMessage, "Assign DuplicateUserNameErrorMessage");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate DuplicateUserNameErrorMessage");

			w.EditProfileIconUrl = "msg";
			Assert.AreEqual ("msg", w.EditProfileIconUrl, "Assign EditProfileIconUrl");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate EditProfileIconUrl");

			w.EditProfileText = "msg";
			Assert.AreEqual ("msg", w.EditProfileText, "Assign EditProfileText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate EditProfileText");

			w.EditProfileUrl = "msg";
			Assert.AreEqual ("msg", w.EditProfileUrl, "Assign EditProfileUrl");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate EditProfileUrl");

			w.EmailLabelText = "msg";
			Assert.AreEqual ("msg", w.EmailLabelText, "Assign EmailLabelText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate EmailLabelText");

			w.EmailRegularExpression = "msg";
			Assert.AreEqual ("msg", w.EmailRegularExpression, "Assign EmailRegularExpression");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate EmailRegularExpression");

			w.EmailRegularExpressionErrorMessage = "msg";
			Assert.AreEqual ("msg", w.EmailRegularExpressionErrorMessage, "Assign EmailRegularExpressionErrorMessage");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate EmailRegularExpressionErrorMessage");

			w.EmailRequiredErrorMessage = "msg";
			Assert.AreEqual ("msg", w.EmailRequiredErrorMessage, "Assign EmailRequiredErrorMessage");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate EmailRequiredErrorMessage");

			w.HelpPageIconUrl = "msg";
			Assert.AreEqual ("msg", w.HelpPageIconUrl, "Assign HelpPageIconUrl");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate HelpPageIconUrl");

			w.HelpPageText = "msg";
			Assert.AreEqual ("msg", w.HelpPageText, "Assign HelpPageText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate HelpPageText");

			w.HelpPageUrl = "msg";
			Assert.AreEqual ("msg", w.HelpPageUrl, "Assign HelpPageUrl");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate HelpPageUrl");

			w.InstructionText = "msg";
			Assert.AreEqual ("msg", w.InstructionText, "Assign InstructionText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate InstructionText");

			w.InvalidAnswerErrorMessage = "msg";
			Assert.AreEqual ("msg", w.InvalidAnswerErrorMessage, "Assign InvalidAnswerErrorMessage");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate InvalidAnswerErrorMessage");

			w.InvalidEmailErrorMessage = "msg";
			Assert.AreEqual ("msg", w.InvalidEmailErrorMessage, "Assign InvalidEmailErrorMessage");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate InvalidEmailErrorMessage");

			w.InvalidPasswordErrorMessage = "msg";
			Assert.AreEqual ("msg", w.InvalidPasswordErrorMessage, "Assign InvalidPasswordErrorMessage");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate InvalidPasswordErrorMessage");

			w.InvalidQuestionErrorMessage = "msg";
			Assert.AreEqual ("msg", w.InvalidQuestionErrorMessage, "Assign InvalidQuestionErrorMessage");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate InvalidQuestionErrorMessage");

			w.LoginCreatedUser = false;
			Assert.AreEqual (false, w.LoginCreatedUser, "Assign LoginCreatedUser");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate LoginCreatedUser");

			w.MembershipProvider = "msg";
			Assert.AreEqual ("msg", w.MembershipProvider, "Assign MembershipProvider");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate MembershipProvider");

			w.PasswordHintText = "msg";
			Assert.AreEqual ("msg", w.PasswordHintText, "Assign PasswordHintText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate PasswordHintText");

			w.PasswordLabelText = "msg";
			Assert.AreEqual ("msg", w.PasswordLabelText, "Assign PasswordLabelText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate PasswordLabelText");

			w.PasswordRegularExpression = "msg";
			Assert.AreEqual ("msg", w.PasswordRegularExpression, "Assign PasswordRegularExpression");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate PasswordRegularExpression");

			w.PasswordRegularExpressionErrorMessage = "msg";
			Assert.AreEqual ("msg", w.PasswordRegularExpressionErrorMessage, "Assign PasswordRegularExpressionErrorMessage");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate PasswordRegularExpressionErrorMessage");

			w.PasswordRequiredErrorMessage = "msg";
			Assert.AreEqual ("msg", w.PasswordRequiredErrorMessage, "Assign PasswordRequiredErrorMessage");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate PasswordRequiredErrorMessage");

			w.QuestionLabelText = "msg";
			Assert.AreEqual ("msg", w.QuestionLabelText, "Assign QuestionLabelText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate QuestionLabelText");

			w.QuestionRequiredErrorMessage = "msg";
			Assert.AreEqual ("msg", w.QuestionRequiredErrorMessage, "Assign QuestionRequiredErrorMessage");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate QuestionRequiredErrorMessage");

			w.RequireEmail = false;
			Assert.AreEqual (false, w.RequireEmail, "Assign RequireEmail");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate RequireEmail");

			w.UnknownErrorMessage = "msg";
			Assert.AreEqual ("msg", w.UnknownErrorMessage, "Assign UnknownErrorMessage");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate UnknownErrorMessage");

			w.UserNameLabelText = "msg";
			Assert.AreEqual ("msg", w.UserNameLabelText, "Assign UserNameLabelText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate UserNameLabelText");

			w.UserNameRequiredErrorMessage = "msg";
			Assert.AreEqual ("msg", w.UserNameRequiredErrorMessage, "Assign UserNameRequiredErrorMessage");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate UserNameRequiredErrorMessage");



		}

		public static void BasicRenderTestInit (Page p)
		{
			CreateTestControl (p);
		}

		public static CreateUserWizard CreateTestControl (Page p)
		{
			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);

			CreateUserWizard w = new CreateUserWizard ();
			w.ID = "CreateUserWizard1";

			CreateUserWizardStep step1 = new CreateUserWizardStep ();
			CompleteWizardStep step2 = new CompleteWizardStep ();

			w.MembershipProvider = "FakeProvider";
			w.WizardSteps.Add (step1);
			w.WizardSteps.Add (step2);

			p.Form.Controls.Add (lcb);
			p.Form.Controls.Add (w);
			p.Form.Controls.Add (lce);

			//p.ClientScript.RegisterForEventValidation (w.ID);

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
			Assert.IsTrue ((st = html.IndexOf ("CreateUserWizard1", st)) > 0, "base render test 2");
			Assert.IsTrue ((st = html.IndexOf ("border-collapse:collapse", st)) > 0, "base render test 3");
			Assert.IsTrue ((st = html.IndexOf ("<table", st)) > 0, "base render test 4");
			Assert.IsTrue ((st = html.IndexOf ("height:100%", st)) > 0, "base render test 5");
			Assert.IsTrue ((st = html.IndexOf ("Sign Up for Your New Account", st)) > 0, "base render test 6");
			Assert.IsTrue ((st = html.IndexOf ("UserName", st)) > 0, "base render test 7");
			Assert.IsTrue ((st = html.IndexOf ("User Name:", st)) > 0, "base render test 8");
			Assert.IsTrue ((st = html.IndexOf ("UserName", st)) > 0, "base render test 9");
			Assert.IsTrue ((st = html.IndexOf ("Password", st)) > 0, "base render test 10");
			Assert.IsTrue ((st = html.IndexOf ("Password:", st)) > 0, "base render test 11");
			Assert.IsTrue ((st = html.IndexOf ("Password", st)) > 0, "base render test 12");
			Assert.IsTrue ((st = html.IndexOf ("ConfirmPassword", st)) > 0, "base render test 13");
			Assert.IsTrue ((st = html.IndexOf ("Confirm Password:", st)) > 0, "base render test 14");
			Assert.IsTrue ((st = html.IndexOf ("ConfirmPassword", st)) > 0, "base render test 15");
			Assert.IsTrue ((st = html.IndexOf ("Email", st)) > 0, "base render test 16");
			Assert.IsTrue ((st = html.IndexOf ("E-mail:", st)) > 0, "base render test 17");
			Assert.IsTrue ((st = html.IndexOf ("Email", st)) > 0, "base render test 18");
			Assert.IsTrue ((st = html.IndexOf ("Question", st)) > 0, "base render test 19");
			Assert.IsTrue ((st = html.IndexOf ("Security Question:", st)) > 0, "base render test 20");
			Assert.IsTrue ((st = html.IndexOf ("Question", st)) > 0, "base render test 21");
			Assert.IsTrue ((st = html.IndexOf ("Answer", st)) > 0, "base render test 22");
			Assert.IsTrue ((st = html.IndexOf ("Security Answer:", st)) > 0, "base render test 23");
			Assert.IsTrue ((st = html.IndexOf ("Answer", st)) > 0, "base render test 24");
			Assert.IsTrue ((st = html.IndexOf ("<input", st)) > 0, "base render test 25");
			Assert.IsTrue ((st = html.IndexOf ("submit", st)) > 0, "base render test 26");
			Assert.IsTrue ((st = html.IndexOf ("NextButton", st)) > 0, "base render test 27");
		}

		[Test]
		[Category ("NunitWeb")]
		public void TitlesRenderTest ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (TitlesRenderTestInit))).Run ();

			Assert.IsTrue (html.IndexOf ("userid") > 0, "UserNameLabelText");
			Assert.IsTrue (html.IndexOf ("pincode") > 0, "PasswordLabelText");
			Assert.IsTrue (html.IndexOf ("cpincode") > 0, "ConfirmPasswordLabelText");
			Assert.IsTrue (html.IndexOf ("zzxcmnmncx") > 0, "QuestionLabelText");
			Assert.IsTrue (html.IndexOf ("kjkjskjkjskjkj") > 0, "AnswerLabelText");
			Assert.IsTrue (html.IndexOf ("emailemail") > 0, "EmailLabelText");
		}

		public static void TitlesRenderTestInit (Page p)
		{
			CreateUserWizard w = CreateTestControl (p);
			w.UserNameLabelText = "userid";
			w.PasswordLabelText = "pincode";
			w.ConfirmPasswordLabelText = "cpincode";
			w.QuestionLabelText = "zzxcmnmncx";
			w.AnswerLabelText = "kjkjskjkjskjkj";
			w.EmailLabelText = "emailemail";
		}

		[Test]
		[Category ("NunitWeb")]
		public void ExtraTitlesRenderTest ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (ExtraTitlesRenderTestInit))).Run ();

			Assert.IsTrue (html.IndexOf ("PasswordHintText") > 0, "PasswordHintText");
			Assert.IsTrue (html.IndexOf ("InstructionText") > 0, "InstructionText");
			Assert.IsTrue (html.IndexOf ("http://www.HelpPageUrl.com") > 0, "HelpPageUrl");
			Assert.IsTrue (html.IndexOf ("HelpPageText") > 0, "HelpPageText");
			Assert.IsTrue (html.IndexOf ("http://www.HelpPageIconUrl.com") > 0, "HelpPageIconUrl");
			Assert.IsTrue (html.IndexOf ("CreateUserButtonText") > 0, "CreateUserButtonText");
			Assert.IsTrue (html.IndexOf ("CreateUserStep.Title") > 0, "CreateUserStep.Title");
		}

		public static void ExtraTitlesRenderTestInit (Page p)
		{
			CreateUserWizard w = CreateTestControl (p);
			w.PasswordHintText = "PasswordHintText";
			w.InstructionText = "InstructionText";
			w.HelpPageUrl = "http://www.HelpPageUrl.com";
			w.HelpPageText = "HelpPageText";
			w.HelpPageIconUrl = "http://www.HelpPageIconUrl.com";
			w.CreateUserStep.Title = "CreateUserStep.Title";
			w.CreateUserButtonText = "CreateUserButtonText";
		}

		[Test]
		[Category ("NunitWeb")]
		public void StylesRenderTest ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (StylesRenderTestInit))).Run ();

			Assert.IsTrue (html.IndexOf ("LightGoldenrodYellow;") > 0, "TextBoxStyle");
			Assert.IsTrue (html.LastIndexOf ("LightGoldenrodYellow;") > html.IndexOf ("LightGoldenrodYellow;"), "TextBoxStyle2");
			Assert.IsTrue (html.IndexOf ("732px") > 0, "TitleTextStyle");
			Assert.IsTrue (html.IndexOf ("LightSkyBlue;") > 0, "HyperLinkStyle");
			Assert.IsTrue (html.IndexOf ("MediumSeaGreen;") > 0, "InstructionTextStyle");
			Assert.IsTrue (html.IndexOf ("MediumSpringGreen;") > 0, "LabelStyle");
			Assert.IsTrue (html.IndexOf ("MintCream;") > 0, "PasswordHintStyle");
			Assert.IsTrue (html.IndexOf ("PeachPuff;") > 0, "CreateUserButtonStyle");

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
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Password"), "password"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "ConfirmPassword"), "password"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Email"), "email"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Question"), "question"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Answer"), "answer"));

			PageDelegates pd = new PageDelegates ();
			pd.PreRender = new PageDelegate (BasicPostTestPreRender);
			pd.Load = new PageDelegate (StylesRenderTestInit);
			pi.Delegates = pd;

			test.Request = fr;
			html = test.Run ();

			Assert.IsTrue (html.IndexOf ("username") > 0, "rendered user name");
			Assert.IsTrue (html.IndexOf ("password") > 0, "rendered user password");
			Assert.IsTrue (html.IndexOf ("password") > 0, "rendered user confirm password");
			Assert.IsTrue (html.IndexOf ("email") > 0, "rendered user email");
			Assert.IsTrue (html.IndexOf ("question") > 0, "rendered user question");
			Assert.IsTrue (html.IndexOf ("answer") > 0, "rendered user answer");

			Assert.IsTrue (html.IndexOf ("LightGoldenrodYellow;") > 0, "TextBoxStyle");
			Assert.IsTrue (html.LastIndexOf ("LightGoldenrodYellow;") > html.IndexOf ("LightGoldenrodYellow;"), "TextBoxStyle2");
			Assert.IsTrue (html.IndexOf ("732px") > 0, "TitleTextStyle");
			Assert.IsTrue (html.IndexOf ("LightSkyBlue;") > 0, "HyperLinkStyle");
			Assert.IsTrue (html.IndexOf ("MediumSeaGreen;") > 0, "InstructionTextStyle");
			Assert.IsTrue (html.IndexOf ("MediumSpringGreen;") > 0, "LabelStyle");
			Assert.IsTrue (html.IndexOf ("MintCream;") > 0, "PasswordHintStyle");
			Assert.IsTrue (html.IndexOf ("PeachPuff;") > 0, "CreateUserButtonStyle");
		}

		public static void BasicPostTestPreRender (Page p)
		{
			CreateUserWizard w = (CreateUserWizard) p.FindControl ("CreateUserWizard1");
			if (w == null)
				Assert.Fail ("postback1");

			Assert.AreEqual ("username", w.UserName, "posted user name");
			Assert.AreEqual ("password", w.Password, "posted user password");
			Assert.AreEqual ("password", w.ConfirmPassword, "posted user confirm password");
			Assert.AreEqual ("email", w.Email, "posted user email");
			Assert.AreEqual ("question", w.Question, "posted user question");
			Assert.AreEqual ("answer", w.Answer, "posted user answer");

		}

		public static void StylesRenderTestInit (Page p)
		{
			CreateUserWizard w = CreateTestControl (p);

			if (!p.IsPostBack) {
				w.TextBoxStyle.BackColor = Color.LightGoldenrodYellow;
				w.TitleTextStyle.Height = Unit.Pixel (732);
				w.LabelStyle.BackColor = Color.MediumSpringGreen;

				w.HelpPageUrl = "http://www.HelpPageUrl.com";
				w.HelpPageText = "hhh";
				w.HyperLinkStyle.BackColor = Color.LightSkyBlue;

				w.InstructionText = "text";
				w.InstructionTextStyle.BackColor = Color.MediumSeaGreen;

				w.PasswordHintText = "pwdhint";
				w.PasswordHintStyle.BackColor = Color.MintCream;

				w.CreateUserButtonStyle.BackColor = Color.PeachPuff;

				w.ContinueButtonType = ButtonType.Image;
				w.ContinueButtonStyle.Width = Unit.Pixel (321);
				w.ContinueButtonImageUrl = "http://localhost/abc.gif";

				w.CompleteSuccessTextStyle.BackColor = Color.Violet;
				w.CompleteSuccessText = "user created";
			}
		}

		// TODO:
		// ValidatorTextStyle
		// ErrorMessageStyle
		[Test]
		[Category ("NunitWeb")]
		public void CreateUserTest ()
		{
			PageInvoker pi = PageInvoker.CreateOnLoad (new PageDelegate (StylesRenderTestInit));
			WebTest test = new WebTest (pi);

			string html = test.Run ();
			test.Invoker = pi;

			FormRequest fr = new FormRequest (test.Response, "form1");

			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "UserName"), "username"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Password"), "password"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "ConfirmPassword"), "password"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Email"), "email"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Question"), "question"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Answer"), "answer"));
			string button = GetDecoratedId (html, "StepNextButtonButton");
			if (button.Length > 0)
				fr.Controls.Add (new BaseControl (GetDecoratedId (html, "StepNextButtonButton"), "Create User"));
			else
				fr.Controls.Add (new BaseControl (GetDecoratedId (html, "StartNextButton"), "Create User"));
				//fr.Controls.Add (new BaseControl ("__EVENTTARGET", GetEventTarget (html, "StartNextButton")));

			test.Request = fr;
			html = test.Run ();

			Assert.IsTrue (html.IndexOf ("Complete") > 0, "Create User");
			Assert.IsTrue (html.IndexOf ("type=\"image\"") > 0, "ContinueButtonType");
			Assert.IsTrue (html.IndexOf ("321px") > 0, "ContinueButtonStyle");
			Assert.IsTrue (html.IndexOf ("http://localhost/abc.gif") > 0, "ContinueButtonImageUrl");
			Assert.IsTrue (html.IndexOf ("Violet") > 0, "CompleteSuccessTextStyle");
			Assert.IsTrue (html.IndexOf ("user created") > 0, "CompleteSuccessText");
		}


		[Test]
		[Category ("NunitWeb")]
		public void ButtonsRenderTest ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (ButtonsRenderTestInit))).Run ();

			Assert.IsTrue (html.IndexOf ("Pink;") > 0, "CreateUserButtonStyle");
			Assert.IsTrue (html.IndexOf ("14px") > 0, "CreateUserButtonStyle");
			Assert.IsTrue (html.IndexOf ("CreateUserButtonText") > 0, "CreateUserButtonText");
		}

		public static void ButtonsRenderTestInit (Page p)
		{
			CreateUserWizard w = CreateTestControl (p);
			w.CreateUserButtonStyle.BorderColor = Color.Pink;
			w.CreateUserButtonStyle.BorderWidth = Unit.Pixel (14);
			w.CreateUserButtonType = ButtonType.Link;
			w.CreateUserButtonText = "CreateUserButtonText";
		}

		[Test]
		[Category ("NunitWeb")]
		public void ErrorMsgTest ()
		{
			PageInvoker pi = PageInvoker.CreateOnLoad (new PageDelegate (ErrorMsgTestInit));
			WebTest test = new WebTest (pi);

			string html = test.Run ();
			test.Invoker = pi;

			FormRequest fr = new FormRequest (test.Response, "form1");
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "UserName"), "duplicate"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Password"), "123"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "ConfirmPassword"), "123"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Email"), "123"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Question"), "123"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Answer"), "123"));
			string button = GetDecoratedId (html, "StepNextButtonButton");
			if (button.Length > 0)
				fr.Controls.Add (new BaseControl (GetDecoratedId (html, "StepNextButtonButton"), "Create User"));
			else
				fr.Controls.Add (new BaseControl (GetDecoratedId (html, "StartNextButton"), "Create User"));

			test.Request = fr;
			html = test.Run ();

			Assert.IsTrue (html.IndexOf ("duplicateuser") > 0, "duplicateuser");

			test.Invoker = pi;
			fr = new FormRequest (test.Response, "form1");
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "UserName"), "incorrect"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Password"), "123"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "ConfirmPassword"), "123"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Email"), "123"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Question"), "123"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Answer"), "123"));
			button = GetDecoratedId (html, "StepNextButtonButton");
			if (button.Length > 0)
				fr.Controls.Add (new BaseControl (GetDecoratedId (html, "StepNextButtonButton"), "Create User"));
			else
				fr.Controls.Add (new BaseControl (GetDecoratedId (html, "StartNextButton"), "Create User"));

			test.Request = fr;
			html = test.Run ();

			Assert.IsTrue (html.IndexOf ("unknown") > 0, "unknown");

			test.Invoker = pi;
			fr = new FormRequest (test.Response, "form1");
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "UserName"), "123"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Password"), "incorrect"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "ConfirmPassword"), "incorrect"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Email"), "123"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Question"), "123"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Answer"), "123"));
			button = GetDecoratedId (html, "StepNextButtonButton");
			if (button.Length > 0)
				fr.Controls.Add (new BaseControl (GetDecoratedId (html, "StepNextButtonButton"), "Create User"));
			else
				fr.Controls.Add (new BaseControl (GetDecoratedId (html, "StartNextButton"), "Create User"));

			test.Request = fr;
			html = test.Run ();

			Assert.IsTrue (html.IndexOf ("invpwd") > 0, "invpwd");

			test.Invoker = pi;
			fr = new FormRequest (test.Response, "form1");
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "UserName"), "123"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Password"), "123"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "ConfirmPassword"), "123"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Email"), "incorrect"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Question"), "123"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Answer"), "123"));
			button = GetDecoratedId (html, "StepNextButtonButton");
			if (button.Length > 0)
				fr.Controls.Add (new BaseControl (GetDecoratedId (html, "StepNextButtonButton"), "Create User"));
			else
				fr.Controls.Add (new BaseControl (GetDecoratedId (html, "StartNextButton"), "Create User"));

			test.Request = fr;
			html = test.Run ();

			Assert.IsTrue (html.IndexOf ("invemail") > 0, "invemail");

			test.Invoker = pi;
			fr = new FormRequest (test.Response, "form1");
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "UserName"), "123"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Password"), "123"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "ConfirmPassword"), "123"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Email"), "duplicate"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Question"), "123"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Answer"), "123"));
			button = GetDecoratedId (html, "StepNextButtonButton");
			if (button.Length > 0)
				fr.Controls.Add (new BaseControl (GetDecoratedId (html, "StepNextButtonButton"), "Create User"));
			else
				fr.Controls.Add (new BaseControl (GetDecoratedId (html, "StartNextButton"), "Create User"));

			test.Request = fr;
			html = test.Run ();

			Assert.IsTrue (html.IndexOf ("duplicateemail") > 0, "duplicateemail");

			test.Invoker = pi;
			fr = new FormRequest (test.Response, "form1");
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "UserName"), "123"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Password"), "123"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "ConfirmPassword"), "123"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Email"), "123"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Question"), "incorrect"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Answer"), "123"));
			button = GetDecoratedId (html, "StepNextButtonButton");
			if (button.Length > 0)
				fr.Controls.Add (new BaseControl (GetDecoratedId (html, "StepNextButtonButton"), "Create User"));
			else
				fr.Controls.Add (new BaseControl (GetDecoratedId (html, "StartNextButton"), "Create User"));

			test.Request = fr;
			html = test.Run ();

			Assert.IsTrue (html.IndexOf ("invquestion") > 0, "invquestion");

			test.Invoker = pi;
			fr = new FormRequest (test.Response, "form1");
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "UserName"), "123"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Password"), "123"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "ConfirmPassword"), "123"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Email"), "123"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Question"), "123"));
			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "Answer"), "incorrect"));
			button = GetDecoratedId (html, "StepNextButtonButton");
			if (button.Length > 0)
				fr.Controls.Add (new BaseControl (GetDecoratedId (html, "StepNextButtonButton"), "Create User"));
			else
				fr.Controls.Add (new BaseControl (GetDecoratedId (html, "StartNextButton"), "Create User"));

			test.Request = fr;
			html = test.Run ();

			Assert.IsTrue (html.IndexOf ("invanswer") > 0, "invanswer");
		}

		public static void ErrorMsgTestInit (Page p)
		{
			CreateUserWizard w = CreateTestControl (p);

			if (!p.IsPostBack) {
				w.AnswerRequiredErrorMessage = "answerreq";
				w.ConfirmPasswordCompareErrorMessage = "passwordconfirm";
				w.ConfirmPasswordRequiredErrorMessage = "passwordconfreq";
				w.DuplicateEmailErrorMessage = "duplicateemail";
				w.DuplicateUserNameErrorMessage = "duplicateuser";
				w.EmailRequiredErrorMessage = "emailreq";
				w.InvalidAnswerErrorMessage = "invanswer";
				w.InvalidEmailErrorMessage = "invemail";
				w.InvalidPasswordErrorMessage = "invpwd";
				w.InvalidQuestionErrorMessage = "invquestion";
				w.PasswordRequiredErrorMessage = "pwdreq";
				w.QuestionRequiredErrorMessage = "questreq";
				w.UserNameRequiredErrorMessage = "userreq";
				w.UnknownErrorMessage = "unknown";
			}
		}

		[Test]
		public void BibbleEvent_ContinueButtonCommand ()
		{
			TestCreateUserWizard w = new TestCreateUserWizard ();
			w.ContinueButtonClick += new EventHandler (w_ContinueButtonClick);
			w.FinishButtonClick += new WizardNavigationEventHandler (w_FinishButtonClick);
			_ContinueButtonClickFlag = false;
			_FinishButtonClickFlag = false;

			CommandEventArgs continueCommandArg = new CommandEventArgs (CreateUserWizard.ContinueButtonCommandName, null);
			Assert.AreEqual (true, w.DoOnBubbleEvent (continueCommandArg), "Bubble Event#1");
			Assert.AreEqual (true, _ContinueButtonClickFlag, "Bubble Event#2");
			Assert.AreEqual (false, _FinishButtonClickFlag, "Bubble Event#3");
		}

		bool _ContinueButtonClickFlag;
		bool _FinishButtonClickFlag;

		void w_ContinueButtonClick (object sender, EventArgs e)
		{
			_ContinueButtonClickFlag = true;
		}

		void w_FinishButtonClick (object sender, WizardNavigationEventArgs e)
		{
			_FinishButtonClickFlag = true;
		}
	}
}

#endif
