//
// CreateUserWizardStepTest.cs - Unit tests for System.Web.UI.WebControls.CreateUserWizardStep
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
	public class CreateUserWizardStepTest
	{
		// MSDN: If you attempt to change the StepType property to any value other than the Auto value of the WizardStepType enumeration, an 
		// InvalidOperationException will be thrown.

		[Test]
		public void CreateUserWizardStep_StepType_Get () {
			CreateUserWizardStep step = new CreateUserWizardStep ();
			Assert.AreEqual (WizardStepType.Auto, step.StepType, "CreateUserWizardStep_StepType_Get");
		}
		
		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void CreateUserWizardStep_StepType_Set () {
			CreateUserWizardStep step = new CreateUserWizardStep ();
			step.StepType = WizardStepType.Start;
		}
	}
}

