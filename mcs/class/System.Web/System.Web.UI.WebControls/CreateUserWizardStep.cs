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
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace System.Web.UI.WebControls
{
	public sealed class CreateUserWizardStep : TemplatedWizardStep
	{
		public CreateUserWizardStep ()
		{
		}

		[MonoTODO ("Always false?")]
		public override bool AllowReturn
		{
			get { return false; }
			set { throw new InvalidOperationException ("AllowReturn cannot be set."); }
		}

		[LocalizableAttribute (true)]
		public override string Title
		{
			get
			{
				object o = ViewState ["TitleText"];
				return (o == null) ? Locale.GetText ("Sign Up for Your New Account") : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("TitleText");
				else
					ViewState ["TitleText"] = value;
			}
		}

		[ThemeableAttribute (false)]
		public override WizardStepType StepType
		{
			get { return base.StepType; }
			set { base.StepType = value; }
		}

		internal override ITemplate DefaultContentTemplate
		{
			get { return new CreateUserStepTemplate ((CreateUserWizard) Wizard); }
		}

		internal override BaseWizardContainer DefaultContentContainer
		{
			get { return new CreateUserStepContainer (); }
		}
	}

	internal sealed class CreateUserStepContainer : BaseWizardContainer
	{
		public CreateUserStepContainer ()
		{
		}

		public Control UserNameTextBox
		{
			get
			{
				Control c = FindControl ("UserName");
				if (c == null)
					throw new HttpException ("CreateUserWizardStep.ContentTemplate does not contain an IEditableTextControl with ID UserName for the username.");

				return c;
			}
		}
		public Control PasswordTextBox
		{
			get
			{
				Control c = FindControl ("Password");
				if (c == null)
					throw new HttpException ("CreateUserWizardStep.ContentTemplate does not contain an IEditableTextControl with ID Password for the new password, this is required if AutoGeneratePassword = true.");

				return c;
			}
		}
		public Control ConfirmPasswordTextBox
		{
			get
			{
				Control c = FindControl ("Password");
				return c;
			}
		}
		public Control EmailTextBox
		{
			get
			{
				Control c = FindControl ("Email");
				if (c == null)
					throw new HttpException ("CreateUserWizardStep.ContentTemplate does not contain an IEditableTextControl with ID Email for the e-mail, this is required if RequireEmail = true.");

				return c;
			}
		}
		public Control QuestionTextBox
		{
			get
			{
				Control c = FindControl ("Question");
				if (c == null)
					throw new HttpException ("CreateUserWizardStep.ContentTemplate does not contain an IEditableTextControl with ID Question for the security question, this is required if your membership provider requires a question and answer.");

				return c;
			}
		}
		public Control AnswerTextBox
		{
			get
			{
				Control c = FindControl ("Answer");
				if (c == null)
					throw new HttpException ("CreateUserWizardStep.ContentTemplate does not contain an IEditableTextControl with ID Answer for the security answer, this is required if your membership provider requires a question and answer.");

				return c;
			}
		}
		public Label ErrorMessageLabel
		{
			get { return FindControl ("ErrorMessage") as Label; }
		}
	}

	sealed class CreateUserStepTemplate : WebControl, ITemplate
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
			}
			else {
				cell0.ColumnSpan = 2;
				cell0.HorizontalAlign = HorizontalAlign.Center;
				if (s0 != null)
					cell0.ApplyStyle (s0);
			}
			return row;
		}

		void ITemplate.InstantiateIn (Control container)
		{
			Table table = new Table ();
			table.ControlStyle.Width = Unit.Percentage (100);
			table.ControlStyle.Height = Unit.Percentage (100);

			// Row #0
			table.Controls.Add (
				CreateRow (new LiteralControl (_createUserWizard.CreateUserStep.Title),
				null, null, _createUserWizard.TitleTextStyle, null));

			// Row #1
			if (_createUserWizard.InstructionText.Length > 0) {
				table.Controls.Add (
					CreateRow (new LiteralControl (_createUserWizard.InstructionText),
					null, null, _createUserWizard.InstructionTextStyle, null));
			}

			// Row #2
			TextBox UserName = new TextBox ();
			UserName.ID = "UserName";
			UserName.ApplyStyle (_createUserWizard.TextBoxStyle);

			Label UserNameLabel = new Label ();
			UserNameLabel.AssociatedControlID = "UserName";
			UserNameLabel.Text = _createUserWizard.UserNameLabelText;


			RequiredFieldValidator UserNameRequired = new RequiredFieldValidator ();
			UserNameRequired.ID = "UserNameRequired";
			UserNameRequired.ControlToValidate = "UserName";
			UserNameRequired.ErrorMessage = _createUserWizard.UserNameRequiredErrorMessage;
			UserNameRequired.ToolTip = _createUserWizard.UserNameRequiredErrorMessage;
			UserNameRequired.Text = "*";
			UserNameRequired.ValidationGroup = _createUserWizard.ID;
			UserNameRequired.ApplyStyle (_createUserWizard.ValidatorTextStyle);

			table.Controls.Add (CreateRow (UserNameLabel, UserName, UserNameRequired, _createUserWizard.LabelStyle, null));

			// Row #3
			if (!_createUserWizard.AutoGeneratePassword) {
				TextBox Password = new TextBox ();
				Password.ID = "Password";
				Password.TextMode = TextBoxMode.Password;
				Password.ApplyStyle (_createUserWizard.TextBoxStyle);

				Label PasswordLabel = new Label ();
				PasswordLabel.AssociatedControlID = "Password";
				PasswordLabel.Text = _createUserWizard.PasswordLabelText;

				RequiredFieldValidator PasswordRequired = new RequiredFieldValidator ();
				PasswordRequired.ID = "PasswordRequired";
				PasswordRequired.ControlToValidate = "Password";
				PasswordRequired.ErrorMessage = _createUserWizard.PasswordRequiredErrorMessage;
				PasswordRequired.ToolTip = _createUserWizard.PasswordRequiredErrorMessage;
				PasswordRequired.Text = "*";
				PasswordRequired.ValidationGroup = _createUserWizard.ID;
				PasswordRequired.ApplyStyle (_createUserWizard.ValidatorTextStyle);

				table.Controls.Add (CreateRow (PasswordLabel, Password, PasswordRequired, _createUserWizard.LabelStyle, null));

				// Row #4
				if (_createUserWizard.PasswordHintText.Length > 0) {
					table.Controls.Add (
						CreateRow (new LiteralControl (""),
							new LiteralControl (_createUserWizard.PasswordHintText),
							new LiteralControl (""),
							null, _createUserWizard.PasswordHintStyle));
				}

				// Row #5
				TextBox ConfirmPassword = new TextBox ();
				ConfirmPassword.ID = "ConfirmPassword";
				ConfirmPassword.TextMode = TextBoxMode.Password;
				ConfirmPassword.ApplyStyle (_createUserWizard.TextBoxStyle);

				Label ConfirmPasswordLabel = new Label ();
				ConfirmPasswordLabel.AssociatedControlID = "ConfirmPassword";
				ConfirmPasswordLabel.Text = _createUserWizard.ConfirmPasswordLabelText;

				RequiredFieldValidator ConfirmPasswordRequired = new RequiredFieldValidator ();
				ConfirmPasswordRequired.ID = "ConfirmPasswordRequired";
				ConfirmPasswordRequired.ControlToValidate = "ConfirmPassword";
				ConfirmPasswordRequired.ErrorMessage = _createUserWizard.ConfirmPasswordRequiredErrorMessage;
				ConfirmPasswordRequired.ToolTip = _createUserWizard.ConfirmPasswordRequiredErrorMessage;
				ConfirmPasswordRequired.Text = "*";
				ConfirmPasswordRequired.ValidationGroup = _createUserWizard.ID;
				ConfirmPasswordRequired.ApplyStyle (_createUserWizard.ValidatorTextStyle);

				table.Controls.Add (CreateRow (ConfirmPasswordLabel, ConfirmPassword, ConfirmPasswordRequired, _createUserWizard.LabelStyle, null));
			}

			// Row #6
			if (_createUserWizard.RequireEmail) {
				TextBox Email = new TextBox ();
				Email.ID = "Email";
				Email.ApplyStyle (_createUserWizard.TextBoxStyle);

				Label EmailLabel = new Label ();
				EmailLabel.AssociatedControlID = "Email";
				EmailLabel.Text = _createUserWizard.EmailLabelText;

				RequiredFieldValidator EmailRequired = new RequiredFieldValidator ();
				EmailRequired.ID = "EmailRequired";
				EmailRequired.ControlToValidate = "Email";
				EmailRequired.ErrorMessage = _createUserWizard.EmailRequiredErrorMessage;
				EmailRequired.ToolTip = _createUserWizard.EmailRequiredErrorMessage;
				EmailRequired.Text = "*";
				EmailRequired.ValidationGroup = _createUserWizard.ID;
				EmailRequired.ApplyStyle (_createUserWizard.ValidatorTextStyle);

				table.Controls.Add (CreateRow (EmailLabel, Email, EmailRequired, _createUserWizard.LabelStyle, null));
			}

			// Row #7
			if (_createUserWizard.QuestionAndAnswerRequired) {
				TextBox Question = new TextBox ();
				Question.ID = "Question";
				Question.ApplyStyle (_createUserWizard.TextBoxStyle);

				Label QuestionLabel = new Label ();
				QuestionLabel.AssociatedControlID = "Question";
				QuestionLabel.Text = _createUserWizard.QuestionLabelText;

				RequiredFieldValidator QuestionRequired = new RequiredFieldValidator ();
				QuestionRequired.ID = "QuestionRequired";
				QuestionRequired.ControlToValidate = "Question";
				QuestionRequired.ErrorMessage = _createUserWizard.QuestionRequiredErrorMessage;
				QuestionRequired.ToolTip = _createUserWizard.QuestionRequiredErrorMessage;
				QuestionRequired.Text = "*";
				QuestionRequired.ValidationGroup = _createUserWizard.ID;
				QuestionRequired.ApplyStyle (_createUserWizard.ValidatorTextStyle);

				table.Controls.Add (CreateRow (QuestionLabel, Question, QuestionRequired, _createUserWizard.LabelStyle, null));

				// Row #8
				TextBox Answer = new TextBox ();
				Answer.ID = "Answer";
				Answer.ApplyStyle (_createUserWizard.TextBoxStyle);

				Label AnswerLabel = new Label ();
				AnswerLabel.AssociatedControlID = "Answer";
				AnswerLabel.Text = _createUserWizard.AnswerLabelText;

				RequiredFieldValidator AnswerRequired = new RequiredFieldValidator ();
				AnswerRequired.ID = "AnswerRequired";
				AnswerRequired.ControlToValidate = "Answer";
				AnswerRequired.ErrorMessage = _createUserWizard.AnswerRequiredErrorMessage;
				AnswerRequired.ToolTip = _createUserWizard.AnswerRequiredErrorMessage;
				AnswerRequired.Text = "*";
				AnswerRequired.ValidationGroup = _createUserWizard.ID;
				AnswerRequired.ApplyStyle (_createUserWizard.ValidatorTextStyle);

				table.Controls.Add (CreateRow (AnswerLabel, Answer, AnswerRequired, _createUserWizard.LabelStyle, null));
			}

			// Row #9
			if (!_createUserWizard.AutoGeneratePassword) {
				CompareValidator PasswordCompare = new CompareValidator ();
				PasswordCompare.ID = "PasswordCompare";
				PasswordCompare.ControlToCompare = "Password";
				PasswordCompare.ControlToValidate = "ConfirmPassword";
				PasswordCompare.Display = ValidatorDisplay.Dynamic;
				PasswordCompare.ErrorMessage = _createUserWizard.ConfirmPasswordCompareErrorMessage;
				PasswordCompare.ValidationGroup = _createUserWizard.ID;

				table.Controls.Add (CreateRow (PasswordCompare, null, null, null, null));
			}

			// Row #10
			if (_createUserWizard.PasswordRegularExpression.Length > 0) {
				RegularExpressionValidator PasswordRegEx = new RegularExpressionValidator ();
				PasswordRegEx.ID = "PasswordRegEx";
				PasswordRegEx.ControlToValidate = "Password";
				PasswordRegEx.ValidationExpression = _createUserWizard.PasswordRegularExpression;
				PasswordRegEx.Display = ValidatorDisplay.Dynamic;
				PasswordRegEx.ErrorMessage = _createUserWizard.PasswordRegularExpressionErrorMessage;
				PasswordRegEx.ValidationGroup = _createUserWizard.ID;

				table.Controls.Add (CreateRow (PasswordRegEx, null, null, null, null));
			}

			// Row #11
			if (_createUserWizard.EmailRegularExpression.Length > 0) {
				RegularExpressionValidator EmailRegEx = new RegularExpressionValidator ();
				EmailRegEx.ID = "EmailRegEx";
				EmailRegEx.ControlToValidate = "Email";
				EmailRegEx.ValidationExpression = _createUserWizard.EmailRegularExpression;
				EmailRegEx.Display = ValidatorDisplay.Dynamic;
				EmailRegEx.ErrorMessage = _createUserWizard.EmailRegularExpressionErrorMessage;
				EmailRegEx.ValidationGroup = _createUserWizard.ID;

				table.Controls.Add (CreateRow (EmailRegEx, null, null, null, null));
			}

			// Row #12
			Label ErrorMessage = new Label ();
			ErrorMessage.ID = "ErrorMessage";
			ErrorMessage.EnableViewState = false;
			ErrorMessage.ControlStyle.ForeColor = System.Drawing.Color.Red;

			table.Controls.Add (CreateRow (ErrorMessage, null, null, null, null));

			// Row #13
			TableRow row9 = null;

			HyperLink HelpLink = null;
			if (_createUserWizard.HelpPageText.Length > 0) {
				HelpLink = new HyperLink ();
				HelpLink.Text = _createUserWizard.HelpPageText;

				if (_createUserWizard.HelpPageUrl.Length > 0)
					HelpLink.NavigateUrl = _createUserWizard.HelpPageUrl;

				row9 = CreateRow (HelpLink, null, null, _createUserWizard.HyperLinkStyle, null);
			}

			Image HelpPageIcon = null;
			if (_createUserWizard.HelpPageIconUrl.Length > 0) {
				HelpPageIcon = new Image ();
				HelpPageIcon.ImageUrl = _createUserWizard.HelpPageIconUrl;
				HelpPageIcon.BorderWidth = Unit.Pixel (0);

				if (_createUserWizard.HelpPageText.Length > 0)
					HelpPageIcon.AlternateText = _createUserWizard.HelpPageText;

				if (row9 == null)
					row9 = CreateRow (HelpPageIcon, null, null, _createUserWizard.HyperLinkStyle, null);
				else
					row9.Cells [0].Controls.AddAt (0, HelpPageIcon);
			}
			if (row9 != null) {
				row9.Cells [0].HorizontalAlign = HorizontalAlign.Left;
				table.Controls.Add (row9);
			}

			//
			container.Controls.Add (table);
		}

		#endregion
	}
}

#endif
