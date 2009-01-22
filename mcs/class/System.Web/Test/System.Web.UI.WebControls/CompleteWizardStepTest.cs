//
// CompleteWizardStepTest.cs - Unit tests for System.Web.UI.WebControls.CompleteWizardStep
//
// Author:
//	Igor Zelmanovich  <igorz@mainsoft.com>
//
// (C) 2006 Mainsoft Corporation (http://www.mainsoft.com)
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

	[Serializable]
	[TestFixture]
	public class CompleteWizardStepTest
	{
		// MSDN: The StepType property overrides the WizardStepBase.StepType property to ensure that CompleteWizardStep is always set to the Complete value of 
		// the WizardStepType enumeration. Attempting to set the StepType property to a different value will result in an InvalidOperationException.

		[Test]
		public void CompleteWizardStep_StepType_Get () {
			CompleteWizardStep step = new CompleteWizardStep ();
			Assert.AreEqual (WizardStepType.Complete, step.StepType, "CompleteWizardStep_StepType_Get");
		}
		
		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void CompleteWizardStep_StepType_Set () {
			CompleteWizardStep step = new CompleteWizardStep ();
			step.StepType = WizardStepType.Auto;
		}
	}
}

#endif
