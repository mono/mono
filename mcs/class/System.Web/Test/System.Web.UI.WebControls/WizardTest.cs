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
using System.Collections;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;

using NUnit.Framework;

namespace MonoTests.System.Web.UI.WebControls {

	public class NavTemplate : ITemplate
	{
		public void InstantiateIn (Control container)
		{
			Button b = new Button();
			b.ID = "NavTemplateButton";
			b.Text = "Next";

			container.Controls.Add (b);
			
		}
	}

	public class TestWizard : Wizard {

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

		private bool onBubble;

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

		public void DoEnsureChildControls ()
		{
			base.EnsureChildControls ();
		} 
	}

	[TestFixture]
	public class WizardTest {

		private HtmlTextWriter GetWriter ()
		{
			StringWriter sw = new StringWriter ();
			sw.NewLine = "\n";
			return new HtmlTextWriter (sw);
		}

		[Test]
		public void ReadOnlyFields ()
		{
			Assert.AreEqual ("Cancel", Wizard.CancelCommandName, "CancelCommandName");
			Assert.AreEqual ("MoveComplete", Wizard.MoveCompleteCommandName, "MoveCompleteCommandName");
			Assert.AreEqual ("MoveNext", Wizard.MoveNextCommandName, "MoveNextCommandName");
			Assert.AreEqual ("MovePrevious", Wizard.MovePreviousCommandName, "MovePreviousCommandName");
			Assert.AreEqual ("Move", Wizard.MoveToCommandName, "MoveToCommandName");
		}

		[Test]
		public void DefaultProperties ()
		{
			TestWizard w = new TestWizard ();
			Assert.AreEqual (0, w.Attributes.Count, "Attributes.Count");
			Assert.AreEqual (0, w.StateBag.Count, "ViewState.Count");
			Assert.AreEqual (-1, w.ActiveStepIndex, "ActiveStepIndex");
			Assert.AreEqual (String.Empty, w.CancelButtonImageUrl, "CancelButtonImageUrl");
			Assert.AreEqual ("Cancel", w.CancelButtonText, "CancelButtonText");
			Assert.AreEqual (ButtonType.Button, w.CancelButtonType, "CancelButtonType");
			Assert.AreEqual (String.Empty, w.CancelDestinationPageUrl, "CancelDestinationPageUrl");
			Assert.IsFalse (w.DisplayCancelButton, "DisplayCancelButton");
			Assert.IsTrue (w.DisplaySideBar, "DisplaySideBar");
			Assert.AreEqual (0, w.CellPadding, "CellPadding");
			Assert.AreEqual (0, w.CellSpacing, "CellSpacing");
			Assert.AreEqual (String.Empty, w.FinishCompleteButtonImageUrl, "FinishCompleteButtonImageUrl");
			Assert.AreEqual ("Finish", w.FinishCompleteButtonText, "FinishCompleteButtonText");
			Assert.AreEqual (ButtonType.Button, w.FinishCompleteButtonType, "FinishCompleteButtonType");
			Assert.AreEqual (String.Empty, w.FinishDestinationPageUrl, "FinishDestinationPageUrl");
			Assert.AreEqual (String.Empty, w.FinishPreviousButtonImageUrl, "FinishPreviousButtonImageUrl");
			Assert.AreEqual ("Previous", w.FinishPreviousButtonText, "FinishPreviousButtonText");
			Assert.AreEqual (ButtonType.Button, w.FinishPreviousButtonType, "FinishPreviousButtonType");
			Assert.AreEqual (String.Empty, w.StartNextButtonImageUrl, "StartNextButtonImageUrl");
			Assert.AreEqual ("Next", w.StartNextButtonText, "StartNextButtonText");
			Assert.AreEqual (ButtonType.Button, w.StartNextButtonType, "StartNextButtonType");
			Assert.AreEqual (String.Empty, w.StepNextButtonImageUrl, "StepNextButtonImageUrl");
			Assert.AreEqual ("Next", w.StepNextButtonText, "StepNextButtonText");
			Assert.AreEqual (ButtonType.Button, w.StepNextButtonType, "StepNextButtonType");
			Assert.AreEqual (String.Empty, w.StepPreviousButtonImageUrl, "StepPreviousButtonImageUrl");
			Assert.AreEqual ("Previous", w.StepPreviousButtonText, "StepPreviousButtonText");
			Assert.AreEqual (ButtonType.Button, w.StepPreviousButtonType, "StepPreviousButtonType");
			Assert.IsNotNull (w.WizardSteps, "WizardSteps");

			// Styles
			Assert.IsNotNull (w.CancelButtonStyle, "CancelButtonStyle");
			Assert.IsNotNull (w.FinishCompleteButtonStyle, "FinishCompleteButtonStyle");
			Assert.IsNotNull (w.FinishPreviousButtonStyle, "FinishPreviousButtonStyle");
			Assert.IsNotNull (w.HeaderStyle, "HeaderStyle");
			Assert.IsNotNull (w.NavigationButtonStyle, "NavigationButtonStyle");
			Assert.IsNotNull (w.NavigationStyle, "NavigationStyle");
			Assert.IsNotNull (w.SideBarStyle, "SideBarStyle");
			Assert.IsNotNull (w.SideBarButtonStyle, "SideBarButtonStyle");
			Assert.IsNotNull (w.StartNextButtonStyle, "StartNextButtonStyle");
			Assert.IsNotNull (w.StepNextButtonStyle, "StepNextButtonStyle");
			Assert.IsNotNull (w.StepPreviousButtonStyle, "StepPreviousButtonStyle");
			Assert.IsNotNull (w.StepStyle, "StepStyle");

			// Templates
			Assert.IsNull (w.SideBarTemplate, "SideBarTemplate");
			Assert.IsNull (w.HeaderTemplate, "HeaderTemplate");
			Assert.IsNull (w.FinishNavigationTemplate, "FinishNavigationTemplate");
			Assert.IsNull (w.StartNavigationTemplate, "StartNavigationTemplate");
			Assert.IsNull (w.StepNavigationTemplate, "StepNavigationTemplate");
		}

		[Test]
		public void AssignToDefaultProperties ()
		{
			TestWizard w = new TestWizard ();
			Assert.AreEqual (0, w.Attributes.Count, "Attributes.Count");
			Assert.AreEqual (0, w.StateBag.Count, "ViewState.Count");

			w.CancelButtonImageUrl = "value";
			Assert.AreEqual ("value", w.CancelButtonImageUrl, "CancelButtonImageUrl");
			Assert.AreEqual (1, w.StateBag.Count, "ViewState.Count-1");

			w.CancelDestinationPageUrl = "value";
			Assert.AreEqual ("value", w.CancelDestinationPageUrl, "CancelDestinationPageUrl");
			Assert.AreEqual (2, w.StateBag.Count, "ViewState.Count-2");

			w.FinishCompleteButtonImageUrl = "value";
			Assert.AreEqual ("value", w.FinishCompleteButtonImageUrl, "FinishCompleteButtonImageUrl");
			Assert.AreEqual (3, w.StateBag.Count, "ViewState.Count-3");

			w.FinishDestinationPageUrl = "value";
			Assert.AreEqual ("value", w.FinishDestinationPageUrl, "FinishDestinationPageUrl");
			Assert.AreEqual (4, w.StateBag.Count, "ViewState.Count-4");

			w.FinishPreviousButtonImageUrl = "value";
			Assert.AreEqual ("value", w.FinishPreviousButtonImageUrl, "FinishPreviousButtonImageUrl");
			Assert.AreEqual (5, w.StateBag.Count, "ViewState.Count-5");

			w.StartNextButtonImageUrl = "value";
			Assert.AreEqual ("value", w.StartNextButtonImageUrl, "StartNextButtonImageUrl");
			Assert.AreEqual (6, w.StateBag.Count, "ViewState.Count-6");

			w.StepNextButtonImageUrl = "value";
			Assert.AreEqual ("value", w.StepNextButtonImageUrl, "StepNextButtonImageUrl");
			Assert.AreEqual (7, w.StateBag.Count, "ViewState.Count-7");

			w.StepPreviousButtonImageUrl = "value";
			Assert.AreEqual ("value", w.StepPreviousButtonImageUrl, "StepPreviousButtonImageUrl");
			Assert.AreEqual (8, w.StateBag.Count, "ViewState.Count-8");

			w.CancelButtonText = "value";
			Assert.AreEqual ("value", w.CancelButtonText, "CancelButtonText");
			Assert.AreEqual (9, w.StateBag.Count, "ViewState.Count-9");

			w.FinishCompleteButtonText = "value";
			Assert.AreEqual ("value", w.FinishCompleteButtonText, "FinishCompleteButtonText");
			Assert.AreEqual (10, w.StateBag.Count, "ViewState.Count-10");

			w.StartNextButtonText = "value";
			Assert.AreEqual ("value", w.StartNextButtonText, "StartNextButtonText");
			Assert.AreEqual (11, w.StateBag.Count, "ViewState.Count-11");

			w.StepNextButtonText = "value";
			Assert.AreEqual ("value", w.StepNextButtonText, "StepNextButtonText");
			Assert.AreEqual (12, w.StateBag.Count, "ViewState.Count-12");

			w.StepPreviousButtonText = "value";
			Assert.AreEqual ("value", w.StepPreviousButtonText, "StepPreviousButtonText");
			Assert.AreEqual (13, w.StateBag.Count, "ViewState.Count-13");

			w.CancelButtonType = ButtonType.Button;
			Assert.AreEqual (ButtonType.Button, w.CancelButtonType, "CancelButtonType");
			Assert.AreEqual (14, w.StateBag.Count, "ViewState.Count-14");

			w.FinishCompleteButtonType = ButtonType.Button;
			Assert.AreEqual (ButtonType.Button, w.FinishCompleteButtonType, "FinishCompleteButtonType");
			Assert.AreEqual (15, w.StateBag.Count, "ViewState.Count-15");

			w.FinishPreviousButtonType = ButtonType.Button;
			Assert.AreEqual (ButtonType.Button, w.FinishPreviousButtonType, "FinishPreviousButtonType");
			Assert.AreEqual (16, w.StateBag.Count, "ViewState.Count-16");

			w.StartNextButtonType = ButtonType.Button;
			Assert.AreEqual (ButtonType.Button, w.StartNextButtonType, "StartNextButtonType");
			Assert.AreEqual (17, w.StateBag.Count, "ViewState.Count-17");

			w.StepNextButtonType = ButtonType.Button;
			Assert.AreEqual (ButtonType.Button, w.StepNextButtonType, "StepNextButtonType");
			Assert.AreEqual (18, w.StateBag.Count, "ViewState.Count-18");

			w.StepPreviousButtonType = ButtonType.Button;
			Assert.AreEqual (ButtonType.Button, w.StepPreviousButtonType, "StepPreviousButtonType");
			Assert.AreEqual (19, w.StateBag.Count, "ViewState.Count-19");
		}

		[Test]
		public void WizardSteps ()
		{
			TestWizard w = new TestWizard ();
			Assert.AreEqual (-1, w.ActiveStepIndex, "ActiveStepIndex on no steps");

			w.WizardSteps.Add (new WizardStep ());
			Assert.IsNotNull (w.WizardSteps[0].Wizard, "WizardStep.Wizard");
			Assert.AreEqual (WizardStepType.Finish, w.GetStepType (w.WizardSteps[0], 0), "WizardStepType.Finish");
		}

		[Test]
		[Category ("NunitWeb")]
		public void RenderTest ()
		{
			string html = new WebTest (PageInvoker.CreateOnPreInit(
				new PageDelegate (WizardPreInit))).Run();

			HtmlDiff.AssertAreEqual ("<table cellspacing=\"0\" cellpadding=\"0\" border=\"0\" style=\"border-collapse:collapse;\"><tr style=\"height:100%;\"><td>123</td></tr><tr><td align=\"right\"><table cellspacing=\"5\" cellpadding=\"5\" border=\"0\"><tr><td align=\"right\"><input type=\"submit\" name=\"ctl02$FinishNavigationTemplateContainerID$FinishButton\" value=\"Finish\" id=\"ctl02_FinishNavigationTemplateContainerID_FinishButton\" /></td></tr></table></td></tr></table>", HtmlDiff.GetControlFromPageHtml (html), "BaseRender");
		}

		public static void WizardPreInit (Page p)
		{
			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG); 

			Wizard w = new Wizard ();
			WizardStep ws = new WizardStep ();
			ws.Controls.Add (new LiteralControl ("123"));
			try {
				w.SkipLinkText = "";
			}
			catch (Exception) { }
			w.DisplaySideBar = false;
			w.WizardSteps.Add (ws);
			p.Controls.Add (lcb); 
			p.Controls.Add (w);
			p.Controls.Add (lce); 
		}

	
	}
}

#endif