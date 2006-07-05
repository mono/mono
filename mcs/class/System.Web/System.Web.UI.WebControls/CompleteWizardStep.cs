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

		[ThemeableAttribute (false)]
		public override WizardStepType StepType
		{
			get { return base.StepType; }
			set { base.StepType = value; }
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

		internal override ITemplate DefaultContentTemplate
		{
			get { return new CompleteStepTemplate ((CreateUserWizard) Wizard); }
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
			cell00.Controls.Add (new LiteralControl ("Complete"));
			row0.Cells.Add (cell00);

			// Row #1
			TableRow row1 = new TableRow ();
			TableCell cell10 = new TableCell ();

			cell10.HorizontalAlign = HorizontalAlign.Center;
			cell10.ControlStyle.CopyFrom (_createUserWizard.CompleteSuccessTextStyle);
			cell10.Controls.Add (new LiteralControl (_createUserWizard.CompleteSuccessText));
			row1.Cells.Add (cell10);

			// table
			table.Rows.Add (row0);
			table.Rows.Add (row1);

			container.Controls.Add (table);
		}

		#endregion
	}

}

#endif