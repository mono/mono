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
	public sealed class CompleteWizardStep : TemplatedWizardStep
	{
		public CompleteWizardStep ()
		{
		}

		// MSDN: The StepType property overrides the WizardStepBase.StepType property to ensure that CompleteWizardStep is always set to the Complete value of 
		// the WizardStepType enumeration. Attempting to set the StepType property to a different value will result in an InvalidOperationException.
		[ThemeableAttribute (false)]
		public override WizardStepType StepType
		{
			get { return WizardStepType.Complete; }
			set { throw new InvalidOperationException (); }
		}

		[LocalizableAttribute (true)]
		public override string Title
		{
			get
			{
				object o = ViewState ["TitleText"];
				return (o == null) ? Locale.GetText ("Complete") : (string) o;
			}
			set
			{
				if (value == null)
					ViewState.Remove ("TitleText");
				else
					ViewState ["TitleText"] = value;
			}
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
			cell00.ControlStyle.CopyFrom (_createUserWizard.TitleTextStyle);
			cell00.Controls.Add (new LiteralControl (_createUserWizard.CompleteStep.Title));
			row0.Cells.Add (cell00);

			// Row #1
			TableRow row1 = new TableRow ();
			TableCell cell10 = new TableCell ();

			cell10.HorizontalAlign = HorizontalAlign.Center;
			cell10.ControlStyle.CopyFrom (_createUserWizard.CompleteSuccessTextStyle);
			cell10.Controls.Add (new LiteralControl (_createUserWizard.CompleteSuccessText));
			row1.Cells.Add (cell10);

			// Row #2
			TableRow row2 = new TableRow ();
			TableCell cell20 = new TableCell ();

			cell20.HorizontalAlign = HorizontalAlign.Right;
			cell20.ColumnSpan = 2;
			row2.Cells.Add (cell20);

			Control b = _createUserWizard.CreateButton ("ContinueButtonButton", CreateUserWizard.ContinueButtonCommandName, _createUserWizard.ContinueButtonType, _createUserWizard.ContinueButtonText, _createUserWizard.ContinueButtonImageUrl, _createUserWizard.ContinueButtonStyle, true);
			cell20.Controls.Add (b);
			
			// table
			table.Rows.Add (row0);
			table.Rows.Add (row1);
			table.Rows.Add (row2);

			container.Controls.Add (table);
		}

		#endregion
	}

}

#endif
