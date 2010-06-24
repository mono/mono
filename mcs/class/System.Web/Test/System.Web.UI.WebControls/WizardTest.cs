//
// Tests for System.Web.UI.WebControls.WizardTest.cs
//
// Author:
//	Vladimir Krasnov  <vladimirk@mainsoft.com>
//	Yoni Klein (yonik@mainsoft.com)	
//
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


#if NET_2_0


using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Drawing;
using System.Web.UI.WebControls;
using Template = System.Web.UI.WebControls;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using NUnit.Framework;
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;
using System.Threading;




namespace MonoTests.System.Web.UI.WebControls
{

	class PokerWizard : Wizard
	{
		// View state Stuff
		public PokerWizard ()
			: base ()
		{
			TrackViewState ();
		}

		public object SaveState ()
		{
			return SaveViewState ();
		}

		public void LoadState (object o)
		{
			LoadViewState (o);
		}

		public StateBag StateBag
		{
			get { return base.ViewState; }
		}

		public static string PokerCancelButtonID
		{
			get
			{
				return PokerWizard.CancelButtonID;
			}
		}

		public static string PokerCustomFinishButtonID
		{
			get
			{
				return PokerWizard.CustomFinishButtonID;
			}
		}

		public static string PokerCustomNextButtonID
		{
			get
			{
				return PokerWizard.CustomNextButtonID;
			}
		}

		public static string PokerCustomPreviousButtonID
		{
			get
			{
				return PokerWizard.CustomPreviousButtonID;
			}
		}

		public static string PokerDataListID
		{
			get
			{
				return PokerWizard.DataListID;
			}
		}

		public static string PokerFinishButtonID
		{
			get
			{
				return PokerWizard.FinishButtonID;
			}
		}

		public static string PokerFinishPreviousButtonID
		{
			get
			{
				return PokerWizard.FinishPreviousButtonID;
			}
		}

		public static string PokerSideBarButtonID
		{
			get
			{
				return PokerWizard.SideBarButtonID;
			}
		}

		public static string PokerStartNextButtonID
		{
			get
			{
				return PokerWizard.StartNextButtonID;
			}
		}

		public static string PokerStepNextButtonID
		{
			get
			{
				return PokerWizard.StepNextButtonID;
			}
		}

		public static string PokerStepPreviousButtonID
		{
			get
			{
				return PokerWizard.StepPreviousButtonID;
			}
		}

		public HtmlTextWriterTag PokerTagKey
		{
			get
			{
				return base.TagKey;
			}
		}

		public object PokerSaveControlState ()
		{
			return base.SaveControlState ();
		}

		public void PokerLoadControlState (object state)
		{
			base.LoadControlState (state);
		}

		public bool PokerAllowNavigationToStep (int index)
		{
			return base.AllowNavigationToStep (index);
		}

		public void PokerCreateChildControls ()
		{
			base.CreateChildControls ();
		}

		public ControlCollection PokerCreateControlCollection ()
		{
			return base.CreateControlCollection ();
		}

		public Style PokerCreateControlStyle ()
		{
			return base.CreateControlStyle ();
		}

		public void DoOnActiveStepChanged (object source, EventArgs e)
		{
			base.OnActiveStepChanged (source, e);
		}

		public void DoOnCancelButtonClick (EventArgs e)
		{
			base.OnCancelButtonClick (e);
		}

		public void DoOnDataBinding (EventArgs e)
		{
			base.OnDataBinding (e);
		}

		public void DoOnFinishButtonClick (WizardNavigationEventArgs e)
		{
			base.OnFinishButtonClick (e);
		}

		public void DoOnInit (EventArgs e)
		{
			base.OnInit (e);
		}

		public void DoOnLoad (EventArgs e)
		{
			base.OnLoad (e);
		}

		public void DoOnNextButtonClick (WizardNavigationEventArgs e)
		{
			base.OnNextButtonClick (e);
		}

		public void DoOnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
		}

		public void DoOnPreviousButtonClick (WizardNavigationEventArgs e)
		{
			base.OnPreviousButtonClick (e);
		}

		public void DoOnSideBarButtonClick (WizardNavigationEventArgs e)
		{
			base.OnSideBarButtonClick (e);
		}

		public string Tag
		{
			get { return base.TagName; }
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
	}

	[TestFixture]
	public class WizardTest
	{
		[Test]
		public void Wizard_DefaultProperty ()
		{
			PokerWizard wizard = new PokerWizard ();
			// Static members 
			Assert.AreEqual ("Cancel", Wizard.CancelCommandName, "CancelCommandName");
			Assert.AreEqual ("MoveComplete", Wizard.MoveCompleteCommandName, "MoveCompleteCommandName");
			Assert.AreEqual ("MoveNext", Wizard.MoveNextCommandName, "MoveNextCommandName");
			Assert.AreEqual ("MovePrevious", Wizard.MovePreviousCommandName, "MovePreviousCommandName");
			Assert.AreEqual ("Move", Wizard.MoveToCommandName, "MoveToCommandName");
			// Protected Fields 
			Assert.AreEqual ("CancelButton", PokerWizard.PokerCancelButtonID, "CancelButtonID");
			Assert.AreEqual ("CustomFinishButton", PokerWizard.PokerCustomFinishButtonID, "CustomFinishButtonID");
			Assert.AreEqual ("CustomNextButton", PokerWizard.PokerCustomNextButtonID, "CustomNextButtonID");
			Assert.AreEqual ("CustomPreviousButton", PokerWizard.PokerCustomPreviousButtonID, "CustomPreviousButtonID");
			Assert.AreEqual ("SideBarList", PokerWizard.PokerDataListID, "DataListID");
			Assert.AreEqual ("FinishButton", PokerWizard.PokerFinishButtonID, "FinishButtonID");
			Assert.AreEqual ("FinishPreviousButton", PokerWizard.PokerFinishPreviousButtonID, "FinishPreviousButtonID");
			Assert.AreEqual ("SideBarButton", PokerWizard.PokerSideBarButtonID, "SideBarButtonID");
			Assert.AreEqual ("StartNextButton", PokerWizard.PokerStartNextButtonID, "StartNextButtonID");
			Assert.AreEqual ("StepNextButton", PokerWizard.PokerStepNextButtonID, "StepNextButtonID");
			Assert.AreEqual ("StepPreviousButton", PokerWizard.PokerStepPreviousButtonID, "StepPreviousButtonID");
			//Public Properties 
			Assert.AreEqual ("", wizard.CancelButtonImageUrl, "CancelButtonImageUrl");
			Assert.AreEqual (typeof (Style), wizard.CancelButtonStyle.GetType (), "CancelButtonStyle");
			Assert.AreEqual ("Cancel", wizard.CancelButtonText, "CancelButtonText");
			Assert.AreEqual (ButtonType.Button, wizard.CancelButtonType, "CancelButtonType");
			Assert.AreEqual ("", wizard.CancelDestinationPageUrl, "CancelDestinationPageUrl");
			Assert.AreEqual (0, wizard.CellPadding, "CellPadding");
			Assert.AreEqual (0, wizard.CellSpacing, "CellSpacing");
			Assert.AreEqual (false, wizard.DisplayCancelButton, "DisplayCancelButton");
			Assert.AreEqual (true, wizard.DisplaySideBar, "DisplaySideBar");
			Assert.AreEqual ("", wizard.FinishCompleteButtonImageUrl, "FinishCompleteButtonImageUrl");
			Assert.AreEqual (typeof (Style), wizard.FinishCompleteButtonStyle.GetType (), "FinishCompleteButtonStyle");
			Assert.AreEqual ("Finish", wizard.FinishCompleteButtonText, "FinishCompleteButtonText");
			Assert.AreEqual (ButtonType.Button, wizard.FinishCompleteButtonType, "FinishCompleteButtonType");
			Assert.AreEqual ("", wizard.FinishDestinationPageUrl, "FinishDestinationPageUrl");
			Assert.AreEqual (null, wizard.FinishNavigationTemplate, "FinishNavigationTemplate");
			Assert.AreEqual ("", wizard.FinishPreviousButtonImageUrl, "FinishPreviousButtonImageUrl");
			Assert.AreEqual (typeof (Style), wizard.FinishPreviousButtonStyle.GetType (), "FinishPreviousButtonStyle");
			Assert.AreEqual ("Previous", wizard.FinishPreviousButtonText, "FinishPreviousButtonText");
			Assert.AreEqual (ButtonType.Button, wizard.FinishPreviousButtonType, "FinishPreviousButtonType");
			Assert.AreEqual (typeof (TableItemStyle), wizard.HeaderStyle.GetType (), "HeaderStyle");
			Assert.AreEqual (null, wizard.HeaderTemplate, "HeaderTemplate");
			Assert.AreEqual ("", wizard.HeaderText, "HeaderText");
			Assert.AreEqual (typeof (Style), wizard.NavigationButtonStyle.GetType (), "NavigationButtonStyle");
			Assert.AreEqual (typeof (TableItemStyle), wizard.NavigationStyle.GetType (), "NavigationStyle");
			Assert.AreEqual (typeof (Style), wizard.SideBarButtonStyle.GetType (), "SideBarButtonStyle");
			Assert.AreEqual (typeof (TableItemStyle), wizard.SideBarStyle.GetType (), "SideBarStyle");
			Assert.AreEqual (null, wizard.SideBarTemplate, "SideBarTemplate");
			Assert.AreEqual (null, wizard.StartNavigationTemplate, "StartNavigationTemplate");
			Assert.AreEqual ("", wizard.StartNextButtonImageUrl, "StartNextButtonImageUrl");
			Assert.AreEqual (typeof (Style), wizard.StartNextButtonStyle.GetType (), "StartNextButtonStyle");
			Assert.AreEqual ("Next", wizard.StartNextButtonText, "StartNextButtonText");
			Assert.AreEqual (ButtonType.Button, wizard.StartNextButtonType, "StartNextButtonType");
			Assert.AreEqual (null, wizard.StepNavigationTemplate, "StepNavigationTemplate");
			Assert.AreEqual ("", wizard.StepNextButtonImageUrl, "StepNextButtonImageUrl");
			Assert.AreEqual (typeof (Style), wizard.StepNextButtonStyle.GetType (), "StepNextButtonStyle");
			Assert.AreEqual ("Next", wizard.StepNextButtonText, "StepNextButtonText");
			Assert.AreEqual (ButtonType.Button, wizard.StepNextButtonType, "StepNextButtonType");
			Assert.AreEqual ("", wizard.StepPreviousButtonImageUrl, "StepPreviousButtonImageUrl");
			Assert.AreEqual (typeof (Style), wizard.StepPreviousButtonStyle.GetType (), "StepPreviousButtonStyle");
			Assert.AreEqual ("Previous", wizard.StepPreviousButtonText, "StepPreviousButtonText");
			Assert.AreEqual (ButtonType.Button, wizard.StepPreviousButtonType, "StepPreviousButtonType");
			Assert.AreEqual (typeof (TableItemStyle), wizard.StepStyle.GetType (), "StepStyle");
			Assert.AreEqual (typeof (WizardStepCollection), wizard.WizardSteps.GetType (), "WizardSteps");
			Assert.IsNotNull (wizard.WizardSteps, "WizardSteps");
		}

		[Test]
		public void Wizard_DefaultPropertyNotWorking ()
		{
			PokerWizard wizard = new PokerWizard ();
			Assert.AreEqual (null, wizard.ActiveStep, "ActiveStep");
			Assert.AreEqual ("Skip Navigation Links.", wizard.SkipLinkText, "SkipLinkText");
			// Protected Properties 
			Assert.AreEqual (typeof (HtmlTextWriterTag), wizard.PokerTagKey.GetType (), "TagKey");
		}

		[Test]
		public void Wizard_StateBag ()
		{
			PokerWizard w = new PokerWizard ();
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
		[Category ("NunitWeb")]
		public void Wizard_CancelButtonPropertyRendering ()
		{

			WebTest t = new WebTest (PageInvoker.CreateOnPreInit (_CancelButtonPropertyRendering));
			string html = t.Run ();
			string origin = "<table cellspacing=\"0\" cellpadding=\"0\" border=\"0\" style=\"border-collapse:collapse;\">\r\n\t<tr style=\"height:100%;\">\r\n\t\t<td>Start</td>\r\n\t</tr><tr>\r\n\t\t<td align=\"right\"><table cellspacing=\"5\" cellpadding=\"5\" border=\"0\">\r\n\t\t\t<tr>\r\n\t\t\t\t<td align=\"right\"><input type=\"submit\" name=\"ctl00$StartNavigationTemplateContainerID$StartNextButton\" value=\"Next\" id=\"ctl00_StartNavigationTemplateContainerID_StartNextButton\" /></td><td align=\"right\"><input type=\"submit\" name=\"ctl00$StartNavigationTemplateContainerID$CancelButton\" value=\"CancelButtonText\" id=\"ctl00_StartNavigationTemplateContainerID_CancelButton\" style=\"border-color:Red;\" /></td>\r\n\t\t\t</tr>\r\n\t\t</table></td>\r\n\t</tr>\r\n</table>";
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (html);
			HtmlDiff.AssertAreEqual (origin, renderedHtml, "CancelButtonPropertyRendering");
		}

		public static void _CancelButtonPropertyRendering (Page p)
		{
			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);
			PokerWizard w = new PokerWizard ();
			w.CancelButtonStyle.BorderColor = Color.Red;
			w.CancelButtonImageUrl = "CancelButtonImageUrl";
			w.CancelDestinationPageUrl = "CancelDestinationPageUrl";
			w.CancelButtonText = "CancelButtonText";
			w.CancelButtonType = ButtonType.Button;
			w.DisplayCancelButton = true;
			
			WizardStep ws1 = new WizardStep ();
			ws1.ID = "step1";
			ws1.StepType = WizardStepType.Start;
			ws1.Controls.Add (new LiteralControl ("Start"));

			WizardStep ws2 = new WizardStep ();
			ws2.ID = "step2";
			ws2.StepType = WizardStepType.Finish;
			ws2.Controls.Add (new LiteralControl ("Finish"));

			w.DisplaySideBar = false;
			w.WizardSteps.Add (ws1);
			w.WizardSteps.Add (ws2);
			p.Controls.Add(lcb);
			p.Controls.Add (w);
			p.Controls.Add (lce);
		}

		[Test]
		[Category ("NunitWeb")]
		public void Wizard_FinishButtonPropertyRendering ()
		{

			WebTest t = new WebTest (PageInvoker.CreateOnPreInit (_FinishButtonPropertyRendering));
			string html = t.Run ();
			string origin = "<table cellspacing=\"0\" cellpadding=\"0\" border=\"0\" style=\"border-collapse:collapse;\">\r\n\t<tr style=\"height:100%;\">\r\n\t\t<td>Finish</td>\r\n\t</tr><tr>\r\n\t\t<td align=\"right\"><table cellspacing=\"5\" cellpadding=\"5\" border=\"0\">\r\n\t\t\t<tr>\r\n\t\t\t\t<td align=\"right\"><input type=\"image\" name=\"ctl00$FinishNavigationTemplateContainerID$FinishPreviousImageButton\" id=\"ctl00_FinishNavigationTemplateContainerID_FinishPreviousImageButton\" src=\"http://FinishPreviousButtonImageUrl\" alt=\"FinishPreviousButtonText\" style=\"background-color:Red;border-width:0px;\" /></td><td align=\"right\"><a id=\"ctl00_FinishNavigationTemplateContainerID_FinishLinkButton\" href=\"javascript:__doPostBack('ctl00$FinishNavigationTemplateContainerID$FinishLinkButton','')\" style=\"border-color:Red;\">FinishCompleteButtonText</a></td>\r\n\t\t\t</tr>\r\n\t\t</table></td>\r\n\t</tr>\r\n</table>";
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (html);
			HtmlDiff.AssertAreEqual (origin, renderedHtml, "CancelButtonPropertyRendering");
		}

		public static void _FinishButtonPropertyRendering (Page p)
		{
			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);
			PokerWizard w = new PokerWizard ();
			w.FinishCompleteButtonStyle.BorderColor = Color.Red;
			w.FinishCompleteButtonImageUrl = "http://FinishCompleteButtonImageUrl";
			w.FinishDestinationPageUrl = "FinishDestinationPageUrl";
			w.FinishCompleteButtonText = "FinishCompleteButtonText";
			w.FinishCompleteButtonType = ButtonType.Link;
			w.FinishPreviousButtonImageUrl = "http://FinishPreviousButtonImageUrl";
			w.FinishPreviousButtonStyle.BackColor = Color.Red;
			w.FinishPreviousButtonText = "FinishPreviousButtonText";
			w.FinishPreviousButtonType = ButtonType.Image;

			WizardStep ws0 = new WizardStep ();
			ws0.ID = "step0";
			ws0.StepType = WizardStepType.Start;
			ws0.Controls.Add (new LiteralControl ("Finish"));
			
			WizardStep ws1 = new WizardStep ();
			ws1.ID = "step1";
			ws1.StepType = WizardStepType.Finish;
			ws1.Controls.Add (new LiteralControl ("Finish"));
			
			w.DisplaySideBar = false;
			w.WizardSteps.Add (ws0);
			w.WizardSteps.Add (ws1);
			w.MoveTo (ws1);
			p.Controls.Add (lcb);
			p.Controls.Add (w);
			p.Controls.Add (lce);
		}

		[Test]
		[Category ("NunitWeb")]
		public void Wizard_HeaderRendering ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnPreInit (_HeaderRendering));
			string html = t.Run ();
			string origin = "<table cellspacing=\"0\" cellpadding=\"0\" border=\"0\" style=\"border-collapse:collapse;\">\r\n\t<tr>\r\n\t\t<td style=\"background-color:Red;\"><input name=\"ctl00$HeaderContainer$TextBox1\" type=\"text\" id=\"ctl00_HeaderContainer_TextBox1\" /></td>\r\n\t</tr><tr style=\"height:100%;\">\r\n\t\t<td>Finish</td>\r\n\t</tr><tr>\r\n\t\t<td align=\"right\"><table cellspacing=\"5\" cellpadding=\"5\" border=\"0\">\r\n\t\t\t<tr>\r\n\t\t\t\t<td align=\"right\"><input type=\"submit\" name=\"ctl00$FinishNavigationTemplateContainerID$FinishPreviousButton\" value=\"Previous\" id=\"ctl00_FinishNavigationTemplateContainerID_FinishPreviousButton\" /></td><td align=\"right\"><input type=\"submit\" name=\"ctl00$FinishNavigationTemplateContainerID$FinishButton\" value=\"Finish\" id=\"ctl00_FinishNavigationTemplateContainerID_FinishButton\" /></td>\r\n\t\t\t</tr>\r\n\t\t</table></td>\r\n\t</tr>\r\n</table>";
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (html);
			HtmlDiff.AssertAreEqual (origin, renderedHtml, "HeaderRendering");
		}

		public static void _HeaderRendering (Page p)
		{
			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);
			PokerWizard w = new PokerWizard ();
			w.HeaderStyle.BackColor = Color.Red;
			w.HeaderTemplate = new CompiledTemplateBuilder (_BuildHeader);
			w.HeaderText = "test";

			WizardStep ws0 = new WizardStep ();
			ws0.ID = "step0";
			ws0.StepType = WizardStepType.Start;
			ws0.Controls.Add (new LiteralControl ("Start"));
			
			WizardStep ws1 = new WizardStep ();
			ws1.ID = "step1";
			ws1.StepType = WizardStepType.Finish;
			ws1.Controls.Add (new LiteralControl ("Finish"));
			
			w.DisplaySideBar = false;
			w.WizardSteps.Add (ws0);
			w.WizardSteps.Add (ws1);
			w.MoveTo (ws1);
			p.Controls.Add (lcb);
			p.Controls.Add (w);
			p.Controls.Add (lce);
		}

		private static void _BuildHeader (Control container)
		{
			TextBox ctrl;
			ctrl = new TextBox ();
			ctrl.ID = "TextBox1";
			container.Controls.Add (ctrl);
		}

		[Test]
		[Category ("NunitWeb")]
		public void Wizard_SideBarRendering ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnPreInit (_SideBarRendering));
			string html = t.Run ();
			string origin = "<table cellspacing=\"0\" cellpadding=\"0\" border=\"0\" style=\"border-collapse:collapse;\">\r\n\t<tr>\r\n\t\t<td style=\"background-color:Red;height:100%;\"><a href=\"#ctl00_SkipLink\"><img alt=\"Skip Navigation Links.\" height=\"0\" width=\"0\" src=\"/NunitWeb/WebResource.axd?d=4RHYfeNnynkXiM59uthjZg2&amp;t=633802729995006876\" style=\"border-width:0px;\" /></a><table id=\"ctl00_SideBarContainer_SideBarList\" cellspacing=\"0\" border=\"0\" style=\"border-collapse:collapse;\">\r\n\t\t\t<tr>\r\n\t\t\t\t<td><input type=\"button\" name=\"ctl00$SideBarContainer$SideBarList$ctl00$SideBarButton\" value=\"step1\" onclick=\"javascript:__doPostBack('ctl00$SideBarContainer$SideBarList$ctl00$SideBarButton','')\" id=\"ctl00_SideBarContainer_SideBarList_ctl00_SideBarButton\" /></td>\r\n\t\t\t</tr><tr>\r\n\t\t\t\t<td><input type=\"button\" name=\"ctl00$SideBarContainer$SideBarList$ctl01$SideBarButton\" value=\"step2\" onclick=\"javascript:__doPostBack('ctl00$SideBarContainer$SideBarList$ctl01$SideBarButton','')\" id=\"ctl00_SideBarContainer_SideBarList_ctl01_SideBarButton\" /></td>\r\n\t\t\t</tr>\r\n\t\t</table><a id=\"ctl00_SkipLink\"></a></td><td style=\"height:100%;\"><table cellspacing=\"0\" cellpadding=\"0\" border=\"0\" style=\"height:100%;width:100%;border-collapse:collapse;\">\r\n\t\t\t<tr style=\"height:100%;\">\r\n\t\t\t\t<td>Step 1</td>\r\n\t\t\t</tr><tr>\r\n\t\t\t\t<td align=\"right\"><table cellspacing=\"5\" cellpadding=\"5\" border=\"0\">\r\n\t\t\t\t\t<tr>\r\n\t\t\t\t\t\t<td align=\"right\"><input type=\"submit\" name=\"ctl00$StartNavigationTemplateContainerID$StartNextButton\" value=\"Next\" id=\"ctl00_StartNavigationTemplateContainerID_StartNextButton\" /></td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t</table></td>\r\n\t\t\t</tr>\r\n\t\t</table></td>\r\n\t</tr>\r\n</table>";
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (html);
			HtmlDiff.AssertAreEqual (origin, renderedHtml, "SideBarRendering");
		}

		public static void _SideBarRendering (Page p)
		{
			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);
			PokerWizard w = new PokerWizard ();
			
			w.SideBarButtonStyle.BackColor = Color.Red;
			w.SideBarStyle.BackColor = Color.Red;
			w.SideBarTemplate = new CompiledTemplateBuilder (_SideBarTemplate);

			WizardStep ws1 = new WizardStep ();
			ws1.ID = "step1";
			ws1.StepType = WizardStepType.Auto;
			ws1.Controls.Add (new LiteralControl ("Step 1"));

			WizardStep ws2 = new WizardStep ();
			ws2.ID = "step2";
			ws2.StepType = WizardStepType.Auto;
			ws2.Controls.Add (new LiteralControl ("Step 2"));

			w.WizardSteps.Add (ws1);
			w.WizardSteps.Add (ws2);
			p.Controls.Add (lcb);
			p.Controls.Add (w);
			p.Controls.Add (lce);
		}

		private static void _SideBarTemplate (Control container)
		{
			DataList list = new DataList ();
			list.ItemTemplate = new CompiledTemplateBuilder (_ItemTemplate);
			list.ID = "SideBarList";
			container.Controls.Add (list);
		}

		private static void _ItemTemplate (Control container)
		{
			Button button = new Button();
			button.ID = "SideBarButton";
			container.Controls.Add (button);
		}

		[Test]
		[Category ("NunitWeb")]
		public void Wizard_NavigationRendering ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnPreInit (_NavigationRendering));
			string html = t.Run ();
			string origin = "<table cellspacing=\"0\" cellpadding=\"0\" border=\"0\" style=\"border-collapse:collapse;\">\r\n\t<tr>\r\n\t\t<td style=\"height:100%;\"><a href=\"#ctl00_SkipLink\"><img alt=\"Skip Navigation Links.\" height=\"0\" width=\"0\" src=\"/NunitWeb/WebResource.axd?d=4RHYfeNnynkXiM59uthjZg2&amp;t=633802729995006876\" style=\"border-width:0px;\" /></a><table id=\"ctl00_SideBarContainer_SideBarList\" cellspacing=\"0\" border=\"0\" style=\"border-collapse:collapse;\">\r\n\t\t\t<tr>\r\n\t\t\t\t<td style=\"font-weight:bold;\"><a id=\"ctl00_SideBarContainer_SideBarList_ctl00_SideBarButton\" href=\"javascript:__doPostBack('ctl00$SideBarContainer$SideBarList$ctl00$SideBarButton','')\">step1</a></td>\r\n\t\t\t</tr><tr>\r\n\t\t\t\t<td><a id=\"ctl00_SideBarContainer_SideBarList_ctl01_SideBarButton\" href=\"javascript:__doPostBack('ctl00$SideBarContainer$SideBarList$ctl01$SideBarButton','')\">step2</a></td>\r\n\t\t\t</tr>\r\n\t\t</table><a id=\"ctl00_SkipLink\"></a></td><td style=\"height:100%;\"><table cellspacing=\"0\" cellpadding=\"0\" border=\"0\" style=\"height:100%;width:100%;border-collapse:collapse;\">\r\n\t\t\t<tr style=\"height:100%;\">\r\n\t\t\t\t<td>Start</td>\r\n\t\t\t</tr><tr>\r\n\t\t\t\t<td align=\"right\" style=\"background-color:Yellow;\"><table cellspacing=\"5\" cellpadding=\"5\" border=\"0\">\r\n\t\t\t\t\t<tr>\r\n\t\t\t\t\t\t<td align=\"right\"><input type=\"submit\" name=\"ctl00$StartNavigationTemplateContainerID$StartNextButton\" value=\"Next\" id=\"ctl00_StartNavigationTemplateContainerID_StartNextButton\" style=\"background-color:Red;\" /></td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t</table></td>\r\n\t\t\t</tr>\r\n\t\t</table></td>\r\n\t</tr>\r\n</table>";
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (html);
			HtmlDiff.AssertAreEqual (origin, renderedHtml, "NavigationRendering");
		}

		public static void _NavigationRendering (Page p)
		{
			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);
			PokerWizard w = new PokerWizard ();
			WizardStep ws1 = new WizardStep ();
			WizardStep ws2 = new WizardStep ();
			
			ws1.ID = "step1";
			ws1.StepType = WizardStepType.Start;
			ws1.Controls.Add (new LiteralControl ("Start"));

			ws2.ID = "step2";
			ws2.StepType = WizardStepType.Start;
			ws2.Controls.Add (new LiteralControl ("Finish"));
			
			w.NavigationButtonStyle.BackColor = Color.Red;
			w.NavigationStyle.BackColor = Color.Yellow;
			
			w.WizardSteps.Add (ws1);
			w.WizardSteps.Add (ws2);
			p.Controls.Add (lcb);
			p.Controls.Add (w);
			p.Controls.Add (lce);
		}

		[Test]
		[Category ("NunitWeb")]
		public void Wizard_StartTypeRendering ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnPreInit (_StartTypeRendering));
			string html = t.Run ();
			string origin = "<table cellspacing=\"0\" cellpadding=\"0\" border=\"0\" style=\"border-collapse:collapse;\">\r\n\t<tr>\r\n\t\t<td style=\"height:100%;\"><a href=\"#ctl00_SkipLink\"><img alt=\"Skip Navigation Links.\" height=\"0\" width=\"0\" src=\"/NunitWeb/WebResource.axd?d=4RHYfeNnynkXiM59uthjZg2&amp;t=633802729995006876\" style=\"border-width:0px;\" /></a><table id=\"ctl00_SideBarContainer_SideBarList\" cellspacing=\"0\" border=\"0\" style=\"border-collapse:collapse;\">\r\n\t\t\t<tr>\r\n\t\t\t\t<td style=\"font-weight:bold;\"><a id=\"ctl00_SideBarContainer_SideBarList_ctl00_SideBarButton\" href=\"javascript:__doPostBack('ctl00$SideBarContainer$SideBarList$ctl00$SideBarButton','')\">step1</a></td>\r\n\t\t\t</tr><tr>\r\n\t\t\t\t<td><a id=\"ctl00_SideBarContainer_SideBarList_ctl01_SideBarButton\" href=\"javascript:__doPostBack('ctl00$SideBarContainer$SideBarList$ctl01$SideBarButton','')\">step2</a></td>\r\n\t\t\t</tr>\r\n\t\t</table><a id=\"ctl00_SkipLink\"></a></td><td style=\"height:100%;\"><table cellspacing=\"0\" cellpadding=\"0\" border=\"0\" style=\"height:100%;width:100%;border-collapse:collapse;\">\r\n\t\t\t<tr style=\"height:100%;\">\r\n\t\t\t\t<td>Start</td>\r\n\t\t\t</tr><tr>\r\n\t\t\t\t<td align=\"right\"><table cellspacing=\"5\" cellpadding=\"5\" border=\"0\">\r\n\t\t\t\t\t<tr>\r\n\t\t\t\t\t\t<td align=\"right\"><input type=\"submit\" name=\"ctl00$StartNavigationTemplateContainerID$StartNextButton\" value=\"StartNextButtonText\" id=\"ctl00_StartNavigationTemplateContainerID_StartNextButton\" style=\"background-color:Red;\" /></td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t</table></td>\r\n\t\t\t</tr>\r\n\t\t</table></td>\r\n\t</tr>\r\n</table>";
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (html);
			HtmlDiff.AssertAreEqual (origin, renderedHtml, "StartTypeRendering");
		}

		public static void _StartTypeRendering (Page p)
		{
			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);
			PokerWizard w = new PokerWizard ();
			WizardStep ws1 = new WizardStep ();
			WizardStep ws2 = new WizardStep ();

			ws1.ID = "step1";
			ws1.StepType = WizardStepType.Start;
			ws1.Controls.Add (new LiteralControl ("Start"));

			ws2.ID = "step2";
			ws2.StepType = WizardStepType.Finish;
			ws2.Controls.Add (new LiteralControl ("Finish"));
			
			w.StartNextButtonImageUrl = "StartNextButtonImageUrl";
			w.StartNextButtonStyle.BackColor = Color.Red;
			w.StartNextButtonText = "StartNextButtonText";
			w.StartNextButtonType = ButtonType.Button;

			w.WizardSteps.Add (ws1);
			w.WizardSteps.Add (ws2);
			p.Controls.Add (lcb);
			p.Controls.Add (w);
			p.Controls.Add (lce);
		}

		[Test]
		[Category ("NunitWeb")]
		public void Wizard_StartTemplateRendering ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnPreInit (_StartTemplateRendering));
			string html = t.Run ();
			string origin = "<table cellspacing=\"0\" cellpadding=\"0\" border=\"0\" style=\"border-collapse:collapse;\">\r\n\t<tr>\r\n\t\t<td style=\"height:100%;\"><a href=\"#ctl00_SkipLink\"><img alt=\"Skip Navigation Links.\" height=\"0\" width=\"0\" src=\"/NunitWeb/WebResource.axd?d=4RHYfeNnynkXiM59uthjZg2&amp;t=633802729995006876\" style=\"border-width:0px;\" /></a><table id=\"ctl00_SideBarContainer_SideBarList\" cellspacing=\"0\" border=\"0\" style=\"border-collapse:collapse;\">\r\n\t\t\t<tr>\r\n\t\t\t\t<td style=\"font-weight:bold;\"><a id=\"ctl00_SideBarContainer_SideBarList_ctl00_SideBarButton\" href=\"javascript:__doPostBack('ctl00$SideBarContainer$SideBarList$ctl00$SideBarButton','')\">step1</a></td>\r\n\t\t\t</tr>\r\n\t\t</table><a id=\"ctl00_SkipLink\"></a></td><td style=\"height:100%;\"><table cellspacing=\"0\" cellpadding=\"0\" border=\"0\" style=\"height:100%;width:100%;border-collapse:collapse;\">\r\n\t\t\t<tr style=\"height:100%;\">\r\n\t\t\t\t<td>Start</td>\r\n\t\t\t</tr><tr>\r\n\t\t\t\t<td align=\"right\"><input type=\"submit\" name=\"ctl00$StartNavigationTemplateContainerID$SideBarButton\" value=\"\" id=\"ctl00_StartNavigationTemplateContainerID_SideBarButton\" style=\"background-color:Red;\" /></td>\r\n\t\t\t</tr>\r\n\t\t</table></td>\r\n\t</tr>\r\n</table>";
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (html);
			HtmlDiff.AssertAreEqual (origin, renderedHtml, "StartTemplateRendering");
		}

		public static void _StartTemplateRendering (Page p)
		{
			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);
			PokerWizard w = new PokerWizard ();
			WizardStep ws1 = new WizardStep ();

			ws1.ID = "step1";
			ws1.StepType = WizardStepType.Start;
			ws1.Controls.Add (new LiteralControl ("Start"));

			w.StartNavigationTemplate = new CompiledTemplateBuilder (_StartTemplate);

			w.WizardSteps.Add (ws1);
			p.Controls.Add (lcb);
			p.Controls.Add (w);
			p.Controls.Add (lce);
		}

		private static void _StartTemplate (Control container)
		{
			Button button = new Button();
			button.ID = "SideBarButton";
			button.BackColor = Color.Red;
			container.Controls.Add (button);
		}

		[Test]
		[Category ("NunitWeb")]
		public void Wizard_StepTypeRendering ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnPreInit (_StepTypeRendering));
			string html = t.Run ();
			string origin = "<table cellspacing=\"0\" cellpadding=\"0\" border=\"0\" style=\"border-collapse:collapse;\">\r\n\t<tr>\r\n\t\t<td style=\"height:100%;\"><a href=\"#ctl00_SkipLink\"><img alt=\"Skip Navigation Links.\" height=\"0\" width=\"0\" src=\"/NunitWeb/WebResource.axd?d=4RHYfeNnynkXiM59uthjZg2&amp;t=633802729995006876\" style=\"border-width:0px;\" /></a><table id=\"ctl00_SideBarContainer_SideBarList\" cellspacing=\"0\" border=\"0\" style=\"border-collapse:collapse;\">\r\n\t\t\t<tr>\r\n\t\t\t\t<td><a id=\"ctl00_SideBarContainer_SideBarList_ctl00_SideBarButton\" href=\"javascript:__doPostBack('ctl00$SideBarContainer$SideBarList$ctl00$SideBarButton','')\">step1</a></td>\r\n\t\t\t</tr><tr>\r\n\t\t\t\t<td style=\"font-weight:bold;\"><a id=\"ctl00_SideBarContainer_SideBarList_ctl01_SideBarButton\" href=\"javascript:__doPostBack('ctl00$SideBarContainer$SideBarList$ctl01$SideBarButton','')\">step2</a></td>\r\n\t\t\t</tr><tr>\r\n\t\t\t\t<td><a id=\"ctl00_SideBarContainer_SideBarList_ctl02_SideBarButton\" href=\"javascript:__doPostBack('ctl00$SideBarContainer$SideBarList$ctl02$SideBarButton','')\">step3</a></td>\r\n\t\t\t</tr>\r\n\t\t</table><a id=\"ctl00_SkipLink\"></a></td><td style=\"height:100%;\"><table cellspacing=\"0\" cellpadding=\"0\" border=\"0\" style=\"height:100%;width:100%;border-collapse:collapse;\">\r\n\t\t\t<tr style=\"height:100%;\">\r\n\t\t\t\t<td style=\"background-color:Red;\">Step2</td>\r\n\t\t\t</tr><tr>\r\n\t\t\t\t<td align=\"right\"><table cellspacing=\"5\" cellpadding=\"5\" border=\"0\">\r\n\t\t\t\t\t<tr>\r\n\t\t\t\t\t\t<td align=\"right\"><a id=\"ctl00_StepNavigationTemplateContainerID_StepPreviousLinkButton\" href=\"javascript:__doPostBack('ctl00$StepNavigationTemplateContainerID$StepPreviousLinkButton','')\" style=\"background-color:Red;\">StepPreviousButtonText</a></td><td align=\"right\"><input type=\"image\" name=\"ctl00$StepNavigationTemplateContainerID$StepNextImageButton\" id=\"ctl00_StepNavigationTemplateContainerID_StepNextImageButton\" src=\"http://StepNextButtonImageUrl\" alt=\"StepNextButtonText\" style=\"background-color:Red;border-width:0px;\" /></td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t</table></td>\r\n\t\t\t</tr>\r\n\t\t</table></td>\r\n\t</tr>\r\n</table>";
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (html);
			HtmlDiff.AssertAreEqual (origin, renderedHtml, "StepRendering");
		}

		public static void _StepTypeRendering (Page p)
		{
			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);
			PokerWizard w = new PokerWizard ();
			WizardStep ws1 = new WizardStep ();
			WizardStep ws2 = new WizardStep ();
			WizardStep ws3 = new WizardStep ();

			ws1.ID = "step1";
			ws1.StepType = WizardStepType.Start;
			ws1.Controls.Add (new LiteralControl ("Step1"));

			ws2.ID = "step2";
			ws2.StepType = WizardStepType.Step;
			ws2.Controls.Add (new LiteralControl ("Step2"));

			ws3.ID = "step3";
			ws3.StepType = WizardStepType.Finish;
			ws3.Controls.Add (new LiteralControl ("Step3"));

			w.StepNextButtonImageUrl = "http://StepNextButtonImageUrl";
			w.StepNextButtonStyle.BackColor = Color.Red;
			w.StepNextButtonText = "StepNextButtonText";
			w.StepNextButtonType = ButtonType.Image;
			w.StepPreviousButtonImageUrl = "http://StepPreviousButtonImageUrl";
			w.StepPreviousButtonStyle.BackColor = Color.Red;
			w.StepPreviousButtonText = "StepPreviousButtonText";
			w.StepPreviousButtonType = ButtonType.Link;
			w.StepStyle.BackColor = Color.Red;
			
			w.WizardSteps.Add (ws1);
			w.WizardSteps.Add (ws2);
			w.WizardSteps.Add (ws3);
			w.MoveTo (ws2);

			p.Controls.Add (lcb);
			p.Controls.Add (w);
			p.Controls.Add (lce);
		}

		[Test]
		[Category ("NunitWeb")]
		public void Wizard_StepNavigationTemplateRendering ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnPreInit (_StepNavigationTemplate));
			string html = t.Run ();
			string origin = "<table cellspacing=\"0\" cellpadding=\"0\" border=\"0\" style=\"border-collapse:collapse;\">\r\n\t<tr>\r\n\t\t<td style=\"height:100%;\"><a href=\"#ctl00_SkipLink\"><img alt=\"Skip Navigation Links.\" height=\"0\" width=\"0\" src=\"/NunitWeb/WebResource.axd?d=4RHYfeNnynkXiM59uthjZg2&amp;t=633802729995006876\" style=\"border-width:0px;\" /></a><table id=\"ctl00_SideBarContainer_SideBarList\" cellspacing=\"0\" border=\"0\" style=\"border-collapse:collapse;\">\r\n\t\t\t<tr>\r\n\t\t\t\t<td style=\"font-weight:bold;\"><a id=\"ctl00_SideBarContainer_SideBarList_ctl00_SideBarButton\" href=\"javascript:__doPostBack('ctl00$SideBarContainer$SideBarList$ctl00$SideBarButton','')\">step1</a></td>\r\n\t\t\t</tr><tr>\r\n\t\t\t\t<td><a id=\"ctl00_SideBarContainer_SideBarList_ctl01_SideBarButton\" href=\"javascript:__doPostBack('ctl00$SideBarContainer$SideBarList$ctl01$SideBarButton','')\">step2</a></td>\r\n\t\t\t</tr>\r\n\t\t</table><a id=\"ctl00_SkipLink\"></a></td><td style=\"height:100%;\"><table cellspacing=\"0\" cellpadding=\"0\" border=\"0\" style=\"height:100%;width:100%;border-collapse:collapse;\">\r\n\t\t\t<tr style=\"height:100%;\">\r\n\t\t\t\t<td>Step1</td>\r\n\t\t\t</tr><tr>\r\n\t\t\t\t<td align=\"right\"><input type=\"submit\" name=\"ctl00$StepNavigationTemplateContainerID$SideBarButton\" value=\"\" id=\"ctl00_StepNavigationTemplateContainerID_SideBarButton\" style=\"background-color:Red;\" />Test text</td>\r\n\t\t\t</tr>\r\n\t\t</table></td>\r\n\t</tr>\r\n</table>";
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (html);
			HtmlDiff.AssertAreEqual (origin, renderedHtml, "StepNavigationTemplateRendering");
		}

		public static void _StepNavigationTemplate (Page p)
		{
			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);
			PokerWizard w = new PokerWizard ();
			WizardStep ws1 = new WizardStep ();
			WizardStep ws2 = new WizardStep ();

			ws1.ID = "step1";
			ws1.StepType = WizardStepType.Step;
			ws1.Controls.Add (new LiteralControl ("Step1"));

			ws2.ID = "step2";
			ws2.StepType = WizardStepType.Step;
			ws2.Controls.Add (new LiteralControl ("Step2"));

			w.StepNavigationTemplate = new CompiledTemplateBuilder (_StepNavigationTemplateCreator);

			w.WizardSteps.Add (ws1);
			w.WizardSteps.Add (ws2);
			p.Controls.Add (lcb);
			p.Controls.Add (w);
			p.Controls.Add (lce);
		}


		private static void _StepNavigationTemplateCreator (Control container)
		{
			Button button = new Button();
			button.ID = "SideBarButton";
			button.BackColor = Color.Red;
			LiteralControl label = new LiteralControl ("Test text");
			container.Controls.Add (button);
			container.Controls.Add (label);
		}

		


		
		[Test]
		public void Wizard_AssignProperty ()
		{
			PokerWizard wizard = new PokerWizard ();
			wizard.CancelButtonImageUrl = "test";
			Assert.AreEqual ("test", wizard.CancelButtonImageUrl, "CancelButtonImageUrl");
			wizard.CancelButtonStyle.BackColor = Color.Red;
			Assert.AreEqual (Color.Red, wizard.CancelButtonStyle.BackColor, "CancelButtonStyle");
			wizard.CancelButtonText = "test";
			Assert.AreEqual ("test", wizard.CancelButtonText, "CancelButtonText");
			wizard.CancelButtonType = ButtonType.Image;
			Assert.AreEqual (ButtonType.Image, wizard.CancelButtonType, "CancelButtonType");
			wizard.CancelDestinationPageUrl = "test";
			Assert.AreEqual ("test", wizard.CancelDestinationPageUrl, "CancelDestinationPageUrl");
			wizard.CellPadding = 1;
			Assert.AreEqual (1, wizard.CellPadding, "CellPadding");
			wizard.CellSpacing = 1;
			Assert.AreEqual (1, wizard.CellSpacing, "CellSpacing");
			wizard.DisplayCancelButton = true;
			Assert.AreEqual (true, wizard.DisplayCancelButton, "DisplayCancelButton");
			wizard.DisplaySideBar = false;
			Assert.AreEqual (false, wizard.DisplaySideBar, "DisplaySideBar");
			wizard.FinishCompleteButtonImageUrl = "test";
			Assert.AreEqual ("test", wizard.FinishCompleteButtonImageUrl, "FinishCompleteButtonImageUrl");
			wizard.FinishCompleteButtonStyle.BackColor = Color.Red;
			Assert.AreEqual (Color.Red, wizard.FinishCompleteButtonStyle.BackColor, "FinishCompleteButtonStyle");
			wizard.FinishCompleteButtonText = "test";
			Assert.AreEqual ("test", wizard.FinishCompleteButtonText, "FinishCompleteButtonText");
			wizard.FinishCompleteButtonType = ButtonType.Image;
			Assert.AreEqual (ButtonType.Image, wizard.FinishCompleteButtonType, "FinishCompleteButtonType");
			wizard.FinishDestinationPageUrl = "test";
			Assert.AreEqual ("test", wizard.FinishDestinationPageUrl, "FinishDestinationPageUrl");
			wizard.FinishNavigationTemplate = new ImageTemplate ();
			Assert.AreEqual (typeof (ImageTemplate), wizard.FinishNavigationTemplate.GetType (), "FinishNavigationTemplate");
			wizard.FinishPreviousButtonImageUrl = "test";
			Assert.AreEqual ("test", wizard.FinishPreviousButtonImageUrl, "FinishPreviousButtonImageUrl");
			wizard.FinishPreviousButtonStyle.BackColor = Color.Red;
			Assert.AreEqual (Color.Red, wizard.FinishPreviousButtonStyle.BackColor, "FinishPreviousButtonStyle");
			wizard.FinishPreviousButtonText = "test";
			Assert.AreEqual ("test", wizard.FinishPreviousButtonText, "FinishPreviousButtonText");
			wizard.FinishPreviousButtonType = ButtonType.Image;
			Assert.AreEqual (ButtonType.Image, wizard.FinishPreviousButtonType, "FinishPreviousButtonType");
			wizard.HeaderStyle.BackColor = Color.Red;
			Assert.AreEqual (Color.Red, wizard.HeaderStyle.BackColor, "HeaderStyle");
			wizard.HeaderTemplate = new ImageTemplate ();
			Assert.AreEqual (typeof (ImageTemplate), wizard.HeaderTemplate.GetType (), "HeaderTemplate");
			wizard.HeaderText = "test";
			Assert.AreEqual ("test", wizard.HeaderText, "HeaderText");
			wizard.NavigationButtonStyle.BackColor = Color.Red;
			Assert.AreEqual (Color.Red, wizard.NavigationButtonStyle.BackColor, "NavigationButtonStyle");
			wizard.NavigationStyle.BackColor = Color.Red;
			Assert.AreEqual (Color.Red, wizard.NavigationStyle.BackColor, "NavigationStyle");
			wizard.SideBarButtonStyle.BackColor = Color.Red;
			wizard.SideBarButtonStyle.BackColor = Color.Red;
			Assert.AreEqual (Color.Red, wizard.SideBarButtonStyle.BackColor, "SideBarButtonStyle");
			wizard.SideBarStyle.BackColor = Color.Red;
			Assert.AreEqual (Color.Red, wizard.SideBarStyle.BackColor, "SideBarStyle");
			wizard.SideBarTemplate = new ImageTemplate ();
			Assert.AreEqual (typeof (ImageTemplate), wizard.SideBarTemplate.GetType (), "SideBarTemplate");
			// SkipLinkText throws System.NotImplementedException look not workihg properties
			// wizard.SkipLinkText = "test";
			// Assert.AreEqual ("test", wizard.SkipLinkText, "SkipLinkText");
			wizard.StartNavigationTemplate = new ImageTemplate ();
			Assert.AreEqual (typeof (ImageTemplate), wizard.StartNavigationTemplate.GetType (), "StartNavigationTemplate");
			wizard.StartNextButtonImageUrl = "test";
			Assert.AreEqual ("test", wizard.StartNextButtonImageUrl, "StartNextButtonImageUrl");
			wizard.StartNextButtonStyle.BackColor = Color.Red;
			Assert.AreEqual (Color.Red, wizard.StartNextButtonStyle.BackColor, "StartNextButtonStyle");
			wizard.StartNextButtonText = "test";
			Assert.AreEqual ("test", wizard.StartNextButtonText, "StartNextButtonText");
			wizard.StartNextButtonType = ButtonType.Image;
			Assert.AreEqual (ButtonType.Image, wizard.StartNextButtonType, "StartNextButtonType");
			wizard.StepNavigationTemplate = new ImageTemplate ();
			Assert.AreEqual (typeof (ImageTemplate), wizard.StepNavigationTemplate.GetType (), "StepNavigationTemplate");
			wizard.StepNextButtonImageUrl = "test";
			Assert.AreEqual ("test", wizard.StepNextButtonImageUrl, "StepNextButtonImageUrl");
			wizard.StepNextButtonStyle.BackColor = Color.Red;
			Assert.AreEqual (Color.Red, wizard.StepNextButtonStyle.BackColor, "StepNextButtonStyle");
			wizard.StepNextButtonText = "test";
			Assert.AreEqual ("test", wizard.StepNextButtonText, "StepNextButtonText");
			wizard.StepNextButtonType = ButtonType.Image;
			Assert.AreEqual (ButtonType.Image, wizard.StepNextButtonType, "StepNextButtonType");
			wizard.StepPreviousButtonImageUrl = "test";
			Assert.AreEqual ("test", wizard.StepPreviousButtonImageUrl, "StepPreviousButtonImageUrl");
			wizard.StepPreviousButtonStyle.BackColor = Color.Red;
			Assert.AreEqual (Color.Red, wizard.StepPreviousButtonStyle.BackColor, "StepPreviousButtonStyle");
			wizard.StepPreviousButtonText = "test";
			Assert.AreEqual ("test", wizard.StepPreviousButtonText, "StepPreviousButtonText");
			wizard.StepPreviousButtonType = ButtonType.Image;
			Assert.AreEqual (ButtonType.Image, wizard.StepPreviousButtonType, "StepPreviousButtonType");
			wizard.StepStyle.BackColor = Color.Red;
			Assert.AreEqual (Color.Red, wizard.StepStyle.BackColor, "StepStyle");
		}



		[Test]
		public void Wizard_GetHistory ()
		{
			PokerWizard wizard = new PokerWizard ();
			WizardStep step1 = new WizardStep ();
			step1.ID = "step1";
			step1.StepType = WizardStepType.Start;
			WizardStep step2 = new WizardStep ();
			step2.ID = "step2";
			step2.StepType = WizardStepType.Step;
			WizardStep step3 = new WizardStep ();
			step3.ID = "step3";
			step3.StepType = WizardStepType.Finish;
			wizard.WizardSteps.Add (step1);
			wizard.WizardSteps.Add (step2);
			wizard.WizardSteps.Add (step3);
			wizard.ActiveStepIndex = 0;
			wizard.MoveTo (step3);
			object o = wizard.PokerSaveControlState ();
			wizard.PokerLoadControlState (o);
			wizard.MoveTo (step2);
			o = wizard.PokerSaveControlState ();
			wizard.PokerLoadControlState (o);
			wizard.MoveTo (step3);
			o = wizard.PokerSaveControlState ();
			wizard.PokerLoadControlState (o);
			ArrayList collection = (ArrayList) wizard.GetHistory ();
			Assert.AreEqual (3, collection.Count, "GetHistoryCount");
		}

		[Test]
		public void Wizard_GetStepType ()
		{
			PokerWizard wizard = new PokerWizard ();
			WizardStep step1 = new WizardStep ();
			step1.ID = "step1";
			step1.StepType = WizardStepType.Start;
			wizard.WizardSteps.Add (step1);
			wizard.ActiveStepIndex = 0;
			WizardStepType result = wizard.GetStepType (wizard.ActiveStep, wizard.ActiveStepIndex);
			Assert.AreEqual (WizardStepType.Start, result, "GetStepType");
		}

		[Test]
		public void Wizard_MoveTo ()
		{
			PokerWizard wizard = new PokerWizard ();
			WizardStep step1 = new WizardStep ();
			step1.ID = "step1";
			step1.StepType = WizardStepType.Start;
			WizardStep step2 = new WizardStep ();
			step2.ID = "step2";
			step2.StepType = WizardStepType.Step;
			WizardStep step3 = new WizardStep ();
			step3.ID = "step3";
			step3.StepType = WizardStepType.Finish;
			wizard.WizardSteps.Add (step1);
			wizard.WizardSteps.Add (step2);
			wizard.WizardSteps.Add (step3);
			wizard.ActiveStepIndex = 0;
			wizard.MoveTo (step3);
			Assert.AreEqual (2, wizard.ActiveStepIndex, "MoveToStep3");
			wizard.MoveTo (step2);
			Assert.AreEqual (1, wizard.ActiveStepIndex, "MoveToStep2");
			wizard.MoveTo (step1);
			Assert.AreEqual (0, wizard.ActiveStepIndex, "MoveToStep1");
		}


		[Test]
		public void Wizard_AllowNavigationToStep ()
		{
			PokerWizard wizard = new PokerWizard ();
			WizardStep step1 = new WizardStep ();
			step1.ID = "step1";
			step1.StepType = WizardStepType.Start;
			WizardStep step2 = new WizardStep ();
			step2.ID = "step2";
			step2.StepType = WizardStepType.Step;
			WizardStep step3 = new WizardStep ();
			step3.ID = "step3";
			step3.StepType = WizardStepType.Finish;
			wizard.WizardSteps.Add (step1);
			wizard.WizardSteps.Add (step2);
			wizard.WizardSteps.Add (step3);
			wizard.ActiveStepIndex = 0;
			wizard.MoveTo (step3);
			object o = wizard.PokerSaveControlState ();
			wizard.PokerLoadControlState (o);
			bool result = wizard.PokerAllowNavigationToStep (2);
			Assert.AreEqual (true, result, "AllowNavigationToStep#1");
			step3.AllowReturn = false;
			result = wizard.PokerAllowNavigationToStep (2);
			Assert.AreEqual (false, result, "AllowNavigationToStep#2");
		}

		[Test]
		public void Wizard_CreateControlCollection ()
		{
			PokerWizard wizard = new PokerWizard ();
			ControlCollection collection = wizard.PokerCreateControlCollection ();
			Assert.IsNotNull (collection, "CreateControlCollection");
			Assert.AreEqual (0, collection.Count, "CreateControlCollection#1");
		}

		[Test]
		public void Wizard_CreateControlStyle ()
		{
			PokerWizard wizard = new PokerWizard ();
			Style style = wizard.PokerCreateControlStyle ();
			Assert.AreEqual (typeof (TableStyle), style.GetType (), "CreateControlStyle#1");
			Assert.AreEqual (0, ((TableStyle) style).CellPadding, "CreateControlStyle#2");
			Assert.AreEqual (0, ((TableStyle) style).CellSpacing, "CreateControlStyle#3");
		}

		[Test]
		public void Wizard_ControlState ()
		{
			PokerWizard wizard = new PokerWizard ();
			WizardStep step1 = new WizardStep ();
			step1.ID = "step1";
			step1.StepType = WizardStepType.Start;
			WizardStep step3 = new WizardStep ();
			step3.ID = "step3";
			step3.StepType = WizardStepType.Finish;
			wizard.WizardSteps.Add (step1);
			wizard.WizardSteps.Add (step3);
			wizard.ActiveStepIndex = 0;
			wizard.MoveTo (step3);
			// LAMESPEC: history updated when SaveControlState occured
			Assert.AreEqual (0, ((ArrayList) wizard.GetHistory ()).Count, "ControlState#1");
			object o = wizard.PokerSaveControlState ();
			wizard.PokerLoadControlState (o);
			wizard.MoveTo (step1);
			Assert.AreEqual (0, wizard.ActiveStepIndex, "ControlState#2");
			wizard.PokerLoadControlState (o);
			Assert.AreEqual (1, wizard.ActiveStepIndex, "ControlState#3");
			Assert.AreEqual (1, ((ArrayList) wizard.GetHistory ()).Count, "ControlState#4");
		}

		[Test]
		public void Wizard_ViewState ()
		{
			PokerWizard wizard = new PokerWizard ();
			PokerWizard copy = new PokerWizard ();
			wizard.ControlStyle.BackColor = Color.Red;
			wizard.FinishCompleteButtonStyle.BackColor = Color.Red;
			wizard.FinishPreviousButtonStyle.BackColor = Color.Red;
			wizard.HeaderStyle.BackColor = Color.Red;
			wizard.NavigationButtonStyle.BackColor = Color.Red;
			wizard.NavigationStyle.BackColor = Color.Red;
			wizard.SideBarButtonStyle.BackColor = Color.Red;
			wizard.SideBarStyle.BackColor = Color.Red;
			wizard.StartNextButtonStyle.BackColor = Color.Red;
			wizard.StepPreviousButtonStyle.BackColor = Color.Red;
			wizard.StepNextButtonStyle.BackColor = Color.Red;
			wizard.StepStyle.BackColor = Color.Red;
			object state = wizard.SaveState ();
			copy.LoadState (state);
			Assert.AreEqual (Color.Red, copy.ControlStyle.BackColor, "ViewStateControlStyle");
			Assert.AreEqual (Color.Red, copy.FinishCompleteButtonStyle.BackColor, "ViewStateFinishCompleteButtonStyle");
			Assert.AreEqual (Color.Red, copy.FinishPreviousButtonStyle.BackColor, "ViewStateFinishPreviousButtonStyle");
			Assert.AreEqual (Color.Red, copy.HeaderStyle.BackColor, "ViewStateHeaderStyle");
			Assert.AreEqual (Color.Red, copy.NavigationButtonStyle.BackColor, "ViewStateNavigationButtonStyle");
			Assert.AreEqual (Color.Red, copy.NavigationStyle.BackColor, "ViewStateNavigationStyle");
			Assert.AreEqual (Color.Red, copy.SideBarButtonStyle.BackColor, "ViewStateSideBarButtonStyle");
			Assert.AreEqual (Color.Red, copy.SideBarStyle.BackColor, "ViewStateSideBarStyle");
			Assert.AreEqual (Color.Red, copy.StartNextButtonStyle.BackColor, "ViewStateStartNextButtonStyle");
			Assert.AreEqual (Color.Red, copy.StepNextButtonStyle.BackColor, "ViewStateStepNextButtonStyle");
			Assert.AreEqual (Color.Red, copy.StepStyle.BackColor, "ViewStateStepStyle");
		}

		[Test]
		public void Wizard_Steps ()
		{
			PokerWizard w = new PokerWizard ();
			Assert.AreEqual (-1, w.ActiveStepIndex, "ActiveStepIndex on no steps");

			w.WizardSteps.Add (new WizardStep ());
			Assert.IsNotNull (w.WizardSteps[0].Wizard, "WizardStep.Wizard");
			Assert.AreEqual (WizardStepType.Finish, w.GetStepType (w.WizardSteps[0], 0), "WizardStepType.Finish");
		}

		/// <summary>
		/// EVENTS
		/// </summary>

		[Test]
		public void Wizard_ActiveStepChanged ()
		{
			PokerWizard wizard = new PokerWizard ();
			wizard.ActiveStepChanged += new EventHandler (wizard_handler);
			wizard.DoOnActiveStepChanged (this, new EventArgs ());
			eventassert ("ActiveStepChanged");
		}

		[Test]
		public void Wizard_CancelButtonClick ()
		{
			PokerWizard wizard = new PokerWizard ();
			wizard.CancelButtonClick += new EventHandler (wizard_handler);
			wizard.DoOnCancelButtonClick (new EventArgs ());
			eventassert ("CancelButtonClick");
		}

		[Test]
		public void Wizard_FinishButtonClick ()
		{
			PokerWizard wizard = new PokerWizard ();
			wizard.FinishButtonClick += new WizardNavigationEventHandler (wizard_handler);
			wizard.DoOnFinishButtonClick (new WizardNavigationEventArgs (0, 0));
			eventassert ("FinishButtonClick");
		}

		[Test]
		public void Wizard_NextButtonClick ()
		{
			PokerWizard wizard = new PokerWizard ();
			wizard.NextButtonClick += new WizardNavigationEventHandler (wizard_handler);
			wizard.DoOnNextButtonClick (new WizardNavigationEventArgs (0, 1));
			eventassert ("NextButtonClick");
		}

		[Test]
		public void Wizard_PreviousButtonClick ()
		{
			PokerWizard wizard = new PokerWizard ();
			wizard.PreviousButtonClick += new WizardNavigationEventHandler (wizard_handler);
			wizard.DoOnPreviousButtonClick (new WizardNavigationEventArgs (0, 1));
			eventassert ("PreviousButtonClick");
		}

		[Test]
		public void Wizard_SideBarButtonClick ()
		{
			PokerWizard wizard = new PokerWizard ();
			wizard.SideBarButtonClick += new WizardNavigationEventHandler (wizard_handler);
			wizard.DoOnSideBarButtonClick (new WizardNavigationEventArgs (0, 1));
			eventassert ("SideBarButtonClick");
		}

		public void wizard_handler (object o, EventArgs e)
		{
			_eventchecker = true;
		}

		/// <summary>
		/// Bubble Event
		/// </summary>
		[Test]
		public void Wizard_BubbleEvent_CancelEvent ()
		{
			PokerWizard wizard = new PokerWizard ();
			wizard.CancelButtonClick += new EventHandler (wizard_handler);
			WizardStep step1 = new WizardStep ();
			step1.ID = "step1";
			step1.StepType = WizardStepType.Start;
			WizardStep step3 = new WizardStep ();
			step3.ID = "step3";
			step3.StepType = WizardStepType.Finish;
			wizard.WizardSteps.Add (step1);
			wizard.WizardSteps.Add (step3);
			wizard.ActiveStepIndex = 0;
			CommandEventArgs e = new CommandEventArgs (Wizard.CancelCommandName, null);
			bool result = wizard.DoBubbleEvent (null, e);
			Assert.AreEqual (true, result, "CancelButtonBubbleEventCommand");
			eventassert ("OnCancelButtonClick");
		}

		[Test]
		public void Wizard_BubbleEvent_MoveNext ()
		{
			PokerWizard wizard = new PokerWizard ();
			wizard.NextButtonClick += new WizardNavigationEventHandler (wizard_handler);
			WizardStep step1 = new WizardStep ();
			step1.ID = "step1";
			step1.StepType = WizardStepType.Start;
			WizardStep step2 = new WizardStep ();
			step2.ID = "step2";
			step2.StepType = WizardStepType.Finish;
			wizard.WizardSteps.Add (step1);
			wizard.WizardSteps.Add (step2);
			wizard.ActiveStepIndex = 0;
			CommandEventArgs e = new CommandEventArgs (Wizard.MoveNextCommandName, null);
			bool result = wizard.DoBubbleEvent (null, e);
			Assert.AreEqual (true, result, "MoveNextBubbleEventCommand");
			eventassert ("MoveNextBubbleEvent");
			Assert.AreEqual (1, wizard.ActiveStepIndex, "ActiveStepIndexAfterBubble");
		}

		[Test]
		public void Wizard_BubbleEvent_MovePrevious ()
		{
			PokerWizard wizard = new PokerWizard ();
			wizard.PreviousButtonClick += new WizardNavigationEventHandler (wizard_handler);
			WizardStep step1 = new WizardStep ();
			step1.ID = "step1";
			step1.StepType = WizardStepType.Start;
			WizardStep step2 = new WizardStep ();
			step2.ID = "step2";
			step2.StepType = WizardStepType.Finish;
			wizard.WizardSteps.Add (step1);
			wizard.WizardSteps.Add (step2);
			wizard.ActiveStepIndex = 1;
			CommandEventArgs e = new CommandEventArgs (Wizard.MovePreviousCommandName, null);
			bool result = wizard.DoBubbleEvent (null, e);
			Assert.AreEqual (true, result, "MovePreviousBubbleEventCommand");
			eventassert ("MovePreviousBubbleEvent");
		}

		[Test]
		public void Wizard_BubbleEvent_MoveComplete ()
		{
			PokerWizard wizard = new PokerWizard ();
			wizard.FinishButtonClick += new WizardNavigationEventHandler (wizard_handler);
			WizardStep step1 = new WizardStep ();
			step1.ID = "step1";
			step1.StepType = WizardStepType.Start;
			WizardStep step2 = new WizardStep ();
			step2.ID = "step2";
			step2.StepType = WizardStepType.Finish;
			WizardStep step3 = new WizardStep ();
			step3.ID = "step2";
			step3.StepType = WizardStepType.Complete;
			wizard.WizardSteps.Add (step1);
			wizard.WizardSteps.Add (step2);
			wizard.WizardSteps.Add (step3);
			wizard.ActiveStepIndex = 1;
			CommandEventArgs e = new CommandEventArgs (Wizard.MoveCompleteCommandName, null);
			bool result = wizard.DoBubbleEvent (null, e);
			Assert.AreEqual (true, result, "MoveCompleteEventCommand");
			eventassert ("MoveCompleteBubbleEvent");
		}

		[Test]
		public void Wizard_BubbleEvent_MoveTo ()
		{
			PokerWizard wizard = new PokerWizard ();
			WizardStep step1 = new WizardStep ();
			step1.ID = "step1";
			step1.StepType = WizardStepType.Start;
			WizardStep step2 = new WizardStep ();
			step2.ID = "step2";
			step2.StepType = WizardStepType.Finish;
			wizard.WizardSteps.Add (step1);
			wizard.WizardSteps.Add (step2);
			wizard.ActiveStepIndex = 0;
			CommandEventArgs e = new CommandEventArgs (Wizard.MoveToCommandName, "1");
			bool result = wizard.DoBubbleEvent (null, e);
			Assert.AreEqual (true, result, "MoveToEventCommand");
			Assert.AreEqual (1, wizard.ActiveStepIndex, "ActiveStepIndexAfterMoveToBubble");
		}



		/// <summary>
		/// Rendering
		/// </summary>

		[Test]
		[Category ("NunitWeb")]
		public void Wizard_RenderTest ()
		{
			string html = new WebTest (PageInvoker.CreateOnPreInit (
				new PageDelegate (WizardPreInit))).Run ();

			string origHtml = "<table cellspacing=\"0\" cellpadding=\"0\" border=\"0\" style=\"border-collapse:collapse;\">\r\n\t<tr style=\"height:100%;\">\r\n\t\t<td>123</td>\r\n\t</tr><tr>\r\n\t\t<td align=\"right\"><table cellspacing=\"5\" cellpadding=\"5\" border=\"0\">\r\n\t\t\t<tr>\r\n\t\t\t\t<td align=\"right\"><input type=\"submit\" name=\"ctl00$FinishNavigationTemplateContainerID$FinishButton\" value=\"Finish\" id=\"ctl00_FinishNavigationTemplateContainerID_FinishButton\" /></td>\r\n\t\t\t</tr>\r\n\t\t</table></td>\r\n\t</tr>\r\n</table>";
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (html);
			HtmlDiff.AssertAreEqual (origHtml, renderedHtml, "BaseRender");
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

		[Test]
		[Category ("NunitWeb")]
		public void Wizard_RenderTestStartItem ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnPreInit (_renderingWizard));
			t.UserData = 0; // Active Index
			string result = t.Run ();
			if (result.IndexOf ("Start") < 0)
				Assert.Fail ("StartItemRendering");
			if (result.IndexOf ("Next") < 0)
				Assert.Fail ("NextButtonNotCreated");
			Assert.AreEqual (-1, result.IndexOf ("Previous"), "PreviousButtonCreatedOnFirstPage");
		}

		[Test]
		[Category ("NunitWeb")]
		public void Wizard_RenderTestStepItem ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnPreInit (_renderingWizard));
			t.UserData = 1; // Active Index
			string result = t.Run ();
			if (result.IndexOf ("Step") < 0)
				Assert.Fail ("StepItemRendering");
			if (result.IndexOf ("Next") < 0)
				Assert.Fail ("NextButtonNotCreated");
			if (result.IndexOf ("Previous") < 0)
				Assert.Fail ("PreviousButtonNotCreated");
		}

		[Test]
		[Category ("NunitWeb")]
		public void Wizard_RenderTestAutoItem ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnPreInit (_renderingWizard));
			t.UserData = 2; // Active Index
			string result = t.Run ();
			if (result.IndexOf ("Auto") < 0)
				Assert.Fail ("AutoItemRendering");
			if (result.IndexOf ("Next") < 0)
				Assert.Fail ("NextButtonNotCreated");
			if (result.IndexOf ("Previous") < 0)
				Assert.Fail ("PreviousButtonNotCreated");
		}

		[Test]
		[Category ("NunitWeb")]
		public void Wizard_RenderTestFinishItem ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnPreInit (_renderingWizard));
			t.UserData = 3; // Active Index
			string result = t.Run ();
			if (result.IndexOf ("FinishText") < 0)
				Assert.Fail ("FinishItemRendering");
			if (result.IndexOf ("Previous") < 0)
				Assert.Fail ("NextButtonNotCreated");
			if (result.IndexOf ("Finish") < 0)
				Assert.Fail ("FinishButtonNotCreated");
			Assert.AreEqual (-1, result.IndexOf ("Next"), "NextButtonCreatedOnLastPage");
		}

		[Test]
		[Category ("NunitWeb")]
		public void Wizard_RenderTestCompleteItem ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnPreInit (_renderingWizard));
			t.UserData = 4; // Active Index
			string result = t.Run ();
			if (result.IndexOf ("Complete") < 0)
				Assert.Fail ("FinishItemRendering");
			Assert.AreEqual (-1, result.IndexOf ("Previous"), "PreviousButtonCreatedOnLastPage");
			Assert.AreEqual (-1, result.IndexOf ("Finish"), "FinishButtonCreatedOnLastPage");
			Assert.AreEqual (-1, result.IndexOf ("Next"), "NextButtonCreatedOnLastPage");
		}

		public static void _renderingWizard (Page p)
		{
			Wizard w = new Wizard ();
			w.ID = "Wizard";

			WizardStep ws = new WizardStep ();
			ws.ID = "step";
			ws.StepType = WizardStepType.Start;
			ws.Controls.Add (new LiteralControl ("Start"));

			WizardStep ws1 = new WizardStep ();
			ws1.ID = "step1";
			ws1.StepType = WizardStepType.Step;
			ws1.Controls.Add (new LiteralControl ("Step"));

			WizardStep ws2 = new WizardStep ();
			ws2.ID = "step2";
			ws2.StepType = WizardStepType.Auto;
			ws2.Controls.Add (new LiteralControl ("Auto"));

			WizardStep ws3 = new WizardStep ();
			ws3.ID = "step3";
			ws3.StepType = WizardStepType.Finish;
			ws3.Controls.Add (new LiteralControl ("FinishText"));

			WizardStep ws4 = new WizardStep ();
			ws4.ID = "step4";
			ws4.StepType = WizardStepType.Complete;
			ws4.Controls.Add (new LiteralControl ("Complete"));

			w.DisplaySideBar = false;
			w.WizardSteps.Add (ws);
			w.WizardSteps.Add (ws1);
			w.WizardSteps.Add (ws2);
			w.WizardSteps.Add (ws3);
			w.WizardSteps.Add (ws4);
			w.ActiveStepIndex = (int) WebTest.CurrentTest.UserData;

			p.Controls.Add (w);
		}

		[Test]
		[Category ("NunitWeb")]
		public void Wizard_PostBackFireEvents_1 ()
		{
			WebTest t = new WebTest ();
			PageDelegates pd = new PageDelegates ();
			pd.PreInit = _postbackEvents;
			t.Invoker = new PageInvoker (pd);
			string html = t.Run ();
			FormRequest fr = new FormRequest (t.Response, "form1");

			//Cancel
#if DOT_NET
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls.Add ("Wizard1$StartNavigationTemplateContainerID$CancelButton");
			fr.Controls["__EVENTTARGET"].Value = "";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			fr.Controls["Wizard1$StartNavigationTemplateContainerID$CancelButton"].Value = "Cancel";
#else
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls.Add ("Wizard1$StartNavContainer$CancelButtonButton");
			fr.Controls ["__EVENTTARGET"].Value = "";
			fr.Controls ["__EVENTARGUMENT"].Value = "";
			fr.Controls ["Wizard1$StartNavContainer$CancelButtonButton"].Value = "Cancel";
#endif
			t.Request = fr;
			html = t.Run ();
			Assert.AreEqual ("CancelButtonClick", t.UserData.ToString (), "Cancel");
			
			// Next
#if DOT_NET
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls.Add ("Wizard1$StartNavigationTemplateContainerID$StartNextButton");
			fr.Controls["__EVENTTARGET"].Value = "";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			fr.Controls["Wizard1$StartNavigationTemplateContainerID$StartNextButton"].Value = "Next";
#else
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls.Add ("Wizard1$StartNavContainer$StartNextButtonButton");
			fr.Controls["__EVENTTARGET"].Value = "";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			fr.Controls ["Wizard1$StartNavContainer$StartNextButtonButton"].Value = "Next";
#endif
			t.Request = fr;
			html = t.Run ();
			Assert.AreEqual ("NextButtonClick", t.UserData.ToString (), "Next");

			// Previous
			fr = new FormRequest (t.Response, "form1");
#if DOT_NET
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls.Add ("Wizard1$FinishNavigationTemplateContainerID$FinishPreviousButton");

			fr.Controls["__EVENTTARGET"].Value = "";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			fr.Controls["Wizard1$FinishNavigationTemplateContainerID$FinishPreviousButton"].Value = "Previous";
#else
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls.Add ("Wizard1$FinishNavContainer$FinishPreviousButtonButton");

			fr.Controls ["__EVENTTARGET"].Value = "";
			fr.Controls ["__EVENTARGUMENT"].Value = "";
			fr.Controls ["Wizard1$FinishNavContainer$FinishPreviousButtonButton"].Value = "Previous";
#endif
			t.Request = fr;
			html = t.Run ();
			Assert.AreEqual ("PreviousButtonClick", t.UserData.ToString (), "Previous");
			
		}

		[Test]
		[Category ("NunitWeb")]
		public void Wizard_PostBackFireEvents_2 ()
		{
			WebTest t = new WebTest ();
			PageDelegates pd = new PageDelegates ();
			pd.PreInit = _postbackEvents;
			t.Invoker = new PageInvoker (pd);
			string html = t.Run ();
			FormRequest fr = new FormRequest (t.Response, "form1");

			// Next
#if DOT_NET
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls.Add ("Wizard1$StartNavigationTemplateContainerID$StartNextButton");
			fr.Controls["__EVENTTARGET"].Value = "";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			fr.Controls["Wizard1$StartNavigationTemplateContainerID$StartNextButton"].Value = "Next";
#else
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls.Add ("Wizard1$StartNavContainer$StartNextButtonButton");
			fr.Controls ["__EVENTTARGET"].Value = "";
			fr.Controls ["__EVENTARGUMENT"].Value = "";
			fr.Controls ["Wizard1$StartNavContainer$StartNextButtonButton"].Value = "Next";
#endif
			t.Request = fr;
			html = t.Run ();
			Assert.AreEqual ("NextButtonClick", t.UserData.ToString (), "Next");

			// Finish
			fr = new FormRequest (t.Response, "form1");
#if DOT_NET
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls.Add ("Wizard1$FinishNavigationTemplateContainerID$FinishButton");
			fr.Controls["__EVENTTARGET"].Value = "";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			fr.Controls["Wizard1$FinishNavigationTemplateContainerID$FinishButton"].Value = "Finish";
#else
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls.Add ("Wizard1$FinishNavContainer$FinishButtonButton");
			fr.Controls ["__EVENTTARGET"].Value = "";
			fr.Controls ["__EVENTARGUMENT"].Value = "";
			fr.Controls ["Wizard1$FinishNavContainer$FinishButtonButton"].Value = "Finish";
#endif
			t.Request = fr;
			t.Run ();
			Assert.AreEqual ("FinishButtonClick", t.UserData.ToString (), "Finish");

		}
		
		[Test]
		[Category ("NunitWeb")]
		public void Wizard_PostBackFireEvents_3 ()
		{
			WebTest t = new WebTest ();
			PageDelegates pd = new PageDelegates ();
			pd.PreInit = _postbackEvents;
			t.Invoker = new PageInvoker (pd);
			string html = t.Run ();
			FormRequest fr = new FormRequest (t.Response, "form1");

			//SideBarButton
			fr = new FormRequest (t.Response, "form1");
#if DOT_NET
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");

			fr.Controls["__EVENTTARGET"].Value = "Wizard1$SideBarContainer$SideBarList$ctl01$SideBarButton";
			fr.Controls["__EVENTARGUMENT"].Value = "";
#else
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");

			fr.Controls ["__EVENTTARGET"].Value = "Wizard1$SideBarContainer$SideBarList$ctl01$SideBarButton";
			fr.Controls ["__EVENTARGUMENT"].Value = "";
#endif
			t.Request = fr;
			html = t.Run ();
			Assert.AreEqual ("SideBarButtonClick", t.UserData.ToString (), "SideBarButton");
		}
		
		public static void _postbackEvents (Page p)
		{
			p.EnableEventValidation = false;
			Wizard w = new Wizard ();
			w.DisplayCancelButton = true;
			w.DisplaySideBar = true;
			
			w.CancelButtonClick += new EventHandler (w_CancelButtonClick);
			w.FinishButtonClick += new WizardNavigationEventHandler (w_FinishButtonClick);
			w.NextButtonClick += new WizardNavigationEventHandler (w_NextButtonClick);
			w.PreviousButtonClick += new WizardNavigationEventHandler (w_PreviousButtonClick);
			w.SideBarButtonClick += new WizardNavigationEventHandler (w_SideBarButtonClick);
			w.ID = "Wizard1";

			WizardStep ws = new WizardStep ();
			ws.ID = "step";
			ws.StepType = WizardStepType.Start;
			ws.Controls.Add (new LiteralControl ("StartType"));

			WizardStep ws2 = new WizardStep ();
			ws2.ID = "step2";
			ws2.StepType = WizardStepType.Finish;
			ws2.Controls.Add (new LiteralControl ("FinishType"));

			WizardStep ws3 = new WizardStep ();
			ws3.ID = "step3";
			ws3.StepType = WizardStepType.Complete;
			ws3.Controls.Add (new LiteralControl ("CompleteType"));

			w.DisplaySideBar = true;
			w.WizardSteps.Add (ws);
			w.WizardSteps.Add (ws2);
			w.WizardSteps.Add (ws3);
			p.Controls.Add (w);
		}

		static void w_SideBarButtonClick (object sender, WizardNavigationEventArgs e)
		{
			WebTest.CurrentTest.UserData = "SideBarButtonClick";
		}

		static void w_PreviousButtonClick (object sender, WizardNavigationEventArgs e)
		{
			WebTest.CurrentTest.UserData = "PreviousButtonClick";
		}

		static void w_NextButtonClick (object sender, WizardNavigationEventArgs e)
		{
			WebTest.CurrentTest.UserData = "NextButtonClick";
		}

		static void w_FinishButtonClick (object sender, WizardNavigationEventArgs e)
		{
			WebTest.CurrentTest.UserData = "FinishButtonClick";
		}

		static void w_CancelButtonClick (object sender, EventArgs e)
		{
			WebTest.CurrentTest.UserData = "CancelButtonClick";
		}

		[Test]
		[Category ("NunitWeb")]
		public void Wizard_PostBack()
		{
			WebTest t = new WebTest ();
			PageDelegates pd = new PageDelegates ();
			pd.PreInit = _postback;
			pd.PreRenderComplete = _readControl;
			t.Invoker = new PageInvoker (pd);
			string result = t.Run ();
			if (result.IndexOf ("Start") < 0)
				Assert.Fail ("Rendering fault");

			ArrayList list =  t.UserData as ArrayList;
			Assert.IsNotNull (list, "PostBackDataNotCreated");
			
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");

			fr.Controls["__EVENTTARGET"].Value = list[1].ToString();
			fr.Controls["__EVENTARGUMENT"].Value = "";
			
			t.Request = fr;
			result = t.Run ();
			if (result.IndexOf ("StepType") < 0)
				Assert.Fail ("MovedToStep1");

			fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");

			fr.Controls["__EVENTTARGET"].Value = list[2].ToString ();
			fr.Controls["__EVENTARGUMENT"].Value = "";

			t.Request = fr;
			result = t.Run ();
			if (result.IndexOf ("AutoType") < 0)
				Assert.Fail ("MovedToStep2");

			fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");

			fr.Controls["__EVENTTARGET"].Value = list[3].ToString ();
			fr.Controls["__EVENTARGUMENT"].Value = "";

			t.Request = fr;
			result = t.Run ();
			if (result.IndexOf ("FinishType") < 0)
				Assert.Fail ("MovedToStep3");

			fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");

			fr.Controls["__EVENTTARGET"].Value = list[4].ToString ();
			fr.Controls["__EVENTARGUMENT"].Value = "";

			t.Request = fr;
			result = t.Run ();
			if (result.IndexOf ("CompleteType") < 0)
				Assert.Fail ("MovedToStep4");
		}

		public static void _postback (Page p)
		{
			p.EnableEventValidation = false;
			Wizard w = new Wizard ();
			w.ID = "Wizard";
			
			WizardStep ws = new WizardStep ();
			ws.ID = "step";
			ws.StepType = WizardStepType.Start;
			ws.Controls.Add (new LiteralControl ("StartType"));

			WizardStep ws1 = new WizardStep ();
			ws1.ID = "step1";
			ws1.StepType = WizardStepType.Step;
			ws1.Controls.Add (new LiteralControl ("StepType"));

			WizardStep ws2 = new WizardStep ();
			ws2.ID = "step2";
			ws2.StepType = WizardStepType.Auto;
			ws2.Controls.Add (new LiteralControl ("AutoType"));

			WizardStep ws3 = new WizardStep ();
			ws3.ID = "step3";
			ws3.StepType = WizardStepType.Finish;
			ws3.Controls.Add (new LiteralControl ("FinishType"));

			WizardStep ws4 = new WizardStep ();
			ws4.ID = "step4";
			ws4.StepType = WizardStepType.Complete;
			ws4.Controls.Add (new LiteralControl ("CompleteType"));

			w.DisplaySideBar = true;
			w.WizardSteps.Add (ws);
			w.WizardSteps.Add (ws1);
			w.WizardSteps.Add (ws2);
			w.WizardSteps.Add (ws3);
			w.WizardSteps.Add (ws4);

			p.Controls.Add (w);
		}

		public static void _readControl (Page p)
		{
			ArrayList list = new ArrayList();
			recurcivefind (list, typeof (LinkButton), p.FindControl ("Wizard"));
			WebTest.CurrentTest.UserData = list;
		}

		public static void recurcivefind (ArrayList list, Type t, Control control )
		{
			foreach (Control c in control.Controls)
			{
				if (c == null)
					continue;
				if (t == c.GetType ()) {
					list.Add (c.UniqueID);
				}
				recurcivefind (list, t, c);
			}
		}


		/// <summary>
		/// Exceptions
		/// </summary>

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void Wizard_ViewStateException ()
		{
			PokerWizard wizard = new PokerWizard ();
			wizard.LoadState (new object ());
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Wizard_ActiveStepException1 ()
		{
			Wizard wizard = new Wizard ();
			wizard.ActiveStepIndex = 1;
			WizardStepBase step = wizard.ActiveStep;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Wizard_ActiveStepException2 ()
		{
			Wizard wizard = new Wizard ();
			wizard.ActiveStepIndex = -2;
			WizardStepBase step = wizard.ActiveStep;
		}

		[TestFixtureTearDown]
		public void TearDown ()
		{
			WebTest.Unload ();
		}

		// A simple Template class to wrap an image.
		public class ImageTemplate : ITemplate
		{
			private Template.Image myImage;
			public Template.Image MyImage
			{
				get
				{
					return myImage;
				}
				set
				{
					myImage = value;
				}
			}
			public void InstantiateIn (Control container)
			{
				container.Controls.Add (MyImage);
			}
		}

		private bool _eventchecker;
		private void eventassert (string message)
		{
			Assert.IsTrue (_eventchecker, message);
			_eventchecker = false;
		}
	}
}
#endif
