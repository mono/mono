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

using System;
using System.Web;
using System.Web.UI;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace System.Web.UI.WebControls
{
	[Browsable (false)]
	public sealed class CompleteWizardStep : TemplatedWizardStep
	{
		public CompleteWizardStep ()
		{
		}

		// MSDN: The StepType property overrides the WizardStepBase.StepType property to ensure that CompleteWizardStep is always set to the Complete value of 
		// the WizardStepType enumeration. Attempting to set the StepType property to a different value will result in an InvalidOperationException.
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Filterable (false)]
		[Browsable (false)]
		[ThemeableAttribute (false)]
		public override WizardStepType StepType {
			get { return WizardStepType.Complete; }
			set { throw new InvalidOperationException (); }
		}

		[LocalizableAttribute (true)]
		public override string Title {
			get {
				object o = ViewState ["TitleText"];
				return (o == null) ? Locale.GetText ("Complete") : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("TitleText");
				else
					ViewState ["TitleText"] = value;
			}
		}
	}

}

