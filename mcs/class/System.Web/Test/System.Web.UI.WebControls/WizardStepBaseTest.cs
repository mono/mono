//
// Tests for System.Web.UI.WebControls.WizardStepBaseTest.cs
//
// Author:
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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using NUnit.Framework;
using System.IO;
using MonoTests.stand_alone.WebHarness;
using MonoTests.SystemWeb.Framework;
using System.Collections;

namespace MonoTests.System.Web.UI.WebControls
{
	public class PokerWizardStepBase : WizardStepBase
	{
			// View state Stuff
		public PokerWizardStepBase ()
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

		public void DoOnLoad (EventArgs e)
		{
			base.OnLoad (e);
		}

		public string RenderChildren ()
		{
			StringWriter sw = new StringWriter ();
			sw.NewLine = "\n";
			HtmlTextWriter writer = new HtmlTextWriter (sw);
			base.RenderChildren (writer);
			return writer.InnerWriter.ToString ();
		}

	}

	[TestFixture]
	public class WizardStepBaseTest
	{
		private bool event_checker;

		[TestFixtureTearDown]
		public void TearDown ()
		{
			WebTest.Unload ();
		}

		[Test]
		public void WizardStepBase_DefaultProperty ()
		{
			PokerWizardStepBase step = new PokerWizardStepBase ();
			Assert.AreEqual (true, step.AllowReturn, "AllowReturn");
			Assert.AreEqual (true, step.EnableTheming, "EnableTheming");
			Assert.AreEqual (null, step.ID, "ID");
			Assert.AreEqual (WizardStepType.Auto, step.StepType, "StepType");
			Assert.AreEqual ("", step.Title, "Title");
			Assert.AreEqual (null, step.Wizard, "Wizard");
		}

		[Test]
		public void WizardStepBase_DefaultPropertyNotWorking ()
		{
			PokerWizardStepBase step = new PokerWizardStepBase ();
			Assert.AreEqual (null, step.Name, "Name");
		}


		[Test]
		public void WizardStepBase_AssignProperty ()
		{
			PokerWizardStepBase step = new PokerWizardStepBase ();
			Wizard w = new Wizard ();
			Assert.AreEqual (0, step.StateBag.Count, "ViewState.Count");

			w.WizardSteps.Add (step);
			Assert.AreEqual (w, step.Wizard, "Wizard");

			step.EnableTheming = false;
			Assert.AreEqual (false, step.EnableTheming, "EnableTheming");

			step.ID = "test";
			Assert.AreEqual ("test", step.ID, "ID");

			step.Title = "test";
			Assert.AreEqual ("test", step.Title, "Title");

			step.AllowReturn = false;
			Assert.AreEqual (false, step.AllowReturn, "AllowReturn");

			step.StepType = WizardStepType.Complete;
			Assert.AreEqual (WizardStepType.Complete, step.StepType, "StepType");
		}

		[Test]
		public void WizardStepBase_StateBag ()
		{
			PokerWizardStepBase step = new PokerWizardStepBase ();
			Wizard w = new Wizard ();
			step.StepType = WizardStepType.Complete;
			Assert.AreEqual (WizardStepType.Complete, step.StepType, "StepType");
			Assert.AreEqual (1, step.StateBag.Count, "StepTypeStateBag");

			step.AllowReturn = false;
			Assert.AreEqual (false, step.AllowReturn, "AllowReturn");
			Assert.AreEqual (2, step.StateBag.Count, "AllowReturnStateBag");

			step.Title = "test";
			Assert.AreEqual ("test", step.Title, "Title");
			Assert.AreEqual (3, step.StateBag.Count, "Title");
		}

		[Test]
		public void WizardStepBase_LoadViewState ()
		{
			PokerWizardStepBase step = new PokerWizardStepBase ();
			PokerWizardStepBase copy = new PokerWizardStepBase ();
			step.AllowReturn = false;
			Assert.AreEqual (false, step.AllowReturn, "AllowReturn");
			Assert.AreEqual (1, step.StateBag.Count, "AllowReturnStateBag");

			step.StepType = WizardStepType.Complete;
			Assert.AreEqual (WizardStepType.Complete, step.StepType, "StepType");
			Assert.AreEqual (2, step.StateBag.Count, "StepTypeStateBag");

			object state = step.SaveState ();
			copy.LoadState (state);
			Assert.AreEqual (false, copy.AllowReturn, "AllowReturn");
			Assert.AreEqual (WizardStepType.Complete, copy.StepType, "StepType");
		}

		[Test]
		public void WizardStepBase_LoadEvent ()
		{
			Wizard w = new Wizard ();
			PokerWizardStepBase step = new PokerWizardStepBase ();
			w.WizardSteps.Add (step);
			step.Load += new EventHandler (eventchecker);
			step.DoOnLoad (new EventArgs ());
			eventassert ("OnLoadEvent");
		}

		[Test]
		public void WizardStepBase_RenderChildren ()
		{
			Wizard w = new Wizard ();
			PokerWizardStepBase step = new PokerWizardStepBase ();
			LiteralControl lc = new LiteralControl ("test");
			step.Controls.Add (lc);
			w.WizardSteps.Add (step);
			Assert.AreEqual ("test", step.RenderChildren (), "RenderChildren");
		}

		[Test]
		public void WizardStepBase_ID ()
		{
			Wizard w = new Wizard ();
			PokerWizardStepBase step = new PokerWizardStepBase ();
			step.ID = "step1";
			w.WizardSteps.Add (step);
			Assert.AreEqual (step, w.FindControl ("step1"), "Step with ID fail");
		}
			
		
		
		[Test]
		[Category ("NunitWeb")]
		public void WizardStepBase_RenderTest ()
		{
			// This render test include Title property test 
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (Render_Test))).Run ();
#if NET_4_0
			string origin = "<table cellspacing=\"0\" cellpadding=\"0\" style=\"border-collapse:collapse;\">\r\n\t<tr>\r\n\t\t<td style=\"height:100%;\"><a href=\"#ctl01_SkipLink\"><img alt=\"Skip Navigation Links.\" height=\"0\" width=\"0\" src=\"/NunitWeb/WebResource.axd?d=8VpphgAbakKUC_J8R6hR0Q2&amp;t=634067491135766272\" style=\"border-width:0px;\" /></a><table id=\"ctl01_SideBarContainer_SideBarList\" cellspacing=\"0\" style=\"border-collapse:collapse;\">\r\n\t\t\t<tr>\r\n\t\t\t\t<td style=\"font-weight:bold;\"><a id=\"ctl01_SideBarContainer_SideBarList_SideBarButton_0\" href=\"javascript:__doPostBack(&#39;ctl01$SideBarContainer$SideBarList$ctl00$SideBarButton&#39;,&#39;&#39;)\">my_title</a></td>\r\n\t\t\t</tr><tr>\r\n\t\t\t\t<td><a id=\"ctl01_SideBarContainer_SideBarList_SideBarButton_1\" href=\"javascript:__doPostBack(&#39;ctl01$SideBarContainer$SideBarList$ctl01$SideBarButton&#39;,&#39;&#39;)\">my_title_2</a></td>\r\n\t\t\t</tr>\r\n\t\t</table><a id=\"ctl01_SkipLink\"></a></td><td style=\"height:100%;\"><table cellspacing=\"0\" cellpadding=\"0\" style=\"height:100%;width:100%;border-collapse:collapse;\">\r\n\t\t\t<tr style=\"height:100%;\">\r\n\t\t\t\t<td>123</td>\r\n\t\t\t</tr><tr>\r\n\t\t\t\t<td align=\"right\"><table cellspacing=\"5\" cellpadding=\"5\">\r\n\t\t\t\t\t<tr>\r\n\t\t\t\t\t\t<td align=\"right\"><input type=\"submit\" name=\"ctl01$StartNavigationTemplateContainerID$StartNextButton\" value=\"Next\" id=\"ctl01_StartNavigationTemplateContainerID_StartNextButton\" /></td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t</table></td>\r\n\t\t\t</tr>\r\n\t\t</table></td>\r\n\t</tr>\r\n</table>";
#else
			string origin = "<table cellspacing=\"0\" cellpadding=\"0\" border=\"0\" style=\"border-collapse:collapse;\">\r\n\t<tr>\r\n\t\t<td style=\"height:100%;\"><a href=\"#ctl01_SkipLink\"><img alt=\"Skip Navigation Links.\" height=\"0\" width=\"0\" src=\"/NunitWeb/WebResource.axd?d=4RHYfeNnynkXiM59uthjZg2&amp;t=633802729995006876\" style=\"border-width:0px;\" /></a><table id=\"ctl01_SideBarContainer_SideBarList\" cellspacing=\"0\" border=\"0\" style=\"border-collapse:collapse;\">\r\n\t\t\t<tr>\r\n\t\t\t\t<td style=\"font-weight:bold;\"><a id=\"ctl01_SideBarContainer_SideBarList_ctl00_SideBarButton\" href=\"javascript:__doPostBack('ctl01$SideBarContainer$SideBarList$ctl00$SideBarButton','')\">my_title</a></td>\r\n\t\t\t</tr><tr>\r\n\t\t\t\t<td><a id=\"ctl01_SideBarContainer_SideBarList_ctl01_SideBarButton\" href=\"javascript:__doPostBack('ctl01$SideBarContainer$SideBarList$ctl01$SideBarButton','')\">my_title_2</a></td>\r\n\t\t\t</tr>\r\n\t\t</table><a id=\"ctl01_SkipLink\"></a></td><td style=\"height:100%;\"><table cellspacing=\"0\" cellpadding=\"0\" border=\"0\" style=\"height:100%;width:100%;border-collapse:collapse;\">\r\n\t\t\t<tr style=\"height:100%;\">\r\n\t\t\t\t<td>123</td>\r\n\t\t\t</tr><tr>\r\n\t\t\t\t<td align=\"right\"><table cellspacing=\"5\" cellpadding=\"5\" border=\"0\">\r\n\t\t\t\t\t<tr>\r\n\t\t\t\t\t\t<td align=\"right\"><input type=\"submit\" name=\"ctl01$StartNavigationTemplateContainerID$StartNextButton\" value=\"Next\" id=\"ctl01_StartNavigationTemplateContainerID_StartNextButton\" /></td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t</table></td>\r\n\t\t\t</tr>\r\n\t\t</table></td>\r\n\t</tr>\r\n</table>";
#endif
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (html);
			Console.WriteLine (origin);
			Console.WriteLine ("------------------------");
			Console.WriteLine (renderedHtml);
			
			HtmlDiff.AssertAreEqual (origin, renderedHtml, "BaseRender");
			if (html.IndexOf ("my_title") < 0)
				Assert.Fail ("WizardStepBase title not rendered");
		}

		public static void Render_Test (Page p)
		{
			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);

			Wizard w = new Wizard ();
			PokerWizardStepBase ws = new PokerWizardStepBase ();
			ws.Title = "my_title";
			ws.Controls.Add (new LiteralControl ("123"));
			ws.StepType = WizardStepType.Start;

			PokerWizardStepBase ws2 = new PokerWizardStepBase ();
			ws2.Title = "my_title_2";
			ws2.Controls.Add (new LiteralControl ("1234567"));
			ws2.StepType = WizardStepType.Finish;
			
			w.DisplaySideBar = true;
			w.WizardSteps.Add (ws);
			w.WizardSteps.Add (ws2);
			p.Form.Controls.Add (lcb);
			p.Form.Controls.Add (w);
			p.Form.Controls.Add (lce);
		}

		[Test]
		[Category ("NunitWeb")]
		public void WizardStepBase_PostBackAllowReturnTest ()
		{
			// This test examine the rendering 2 steps and make postbake
			// assigned AllowReturn property 

			WebTest t = new WebTest ();
			PageDelegates pd = new PageDelegates ();
			pd.PreInit = _postback;
			pd.PreRenderComplete = read_control;
			t.Invoker = new PageInvoker (pd);
			string result = t.Run ();
			if (result.IndexOf ("Start") < 0)
				Assert.Fail ("Rendering fault");

			ArrayList list = t.UserData as ArrayList;
			Assert.IsNotNull (list, "PostBackDataNotCreated");

			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");

			fr.Controls["__EVENTTARGET"].Value = list[1].ToString ();
			fr.Controls["__EVENTARGUMENT"].Value = "";

			t.Request = fr;
			result = t.Run ();
			if (result.IndexOf ("StepType") < 0)
				Assert.Fail ("MovedToStep1");
			if (result.IndexOf ("Previous") > 0) {
				Assert.Fail ("Previous button rendered");
			}
		}

		public static void _postback (Page p)
		{
			p.EnableEventValidation = false;
			Wizard w = new Wizard ();
			w.ID = "Wizard";

			PokerWizardStepBase ws = new PokerWizardStepBase ();
			ws.ID = "step";
			ws.StepType = WizardStepType.Start;
			ws.Controls.Add (new LiteralControl ("StartType"));
			ws.AllowReturn = false;

			PokerWizardStepBase ws1 = new PokerWizardStepBase ();
			ws1.ID = "step1";
			ws1.StepType = WizardStepType.Step;
			ws1.Controls.Add (new LiteralControl ("StepType"));

			w.DisplaySideBar = true;
			w.WizardSteps.Add (ws);
			w.WizardSteps.Add (ws1);
			p.Controls.Add (w);
		}

		[Test]
		[Category ("NunitWeb")]
		public void WizardStepBase_Theme ()
		{
			WebTest.CopyResource (GetType (), "WizardTest.skin", "App_Themes/Theme1/WizardTest.skin");
			WebTest t = new WebTest ();
			PageDelegates pd = new PageDelegates ();
			pd.PreInit = set_properties;
			pd.Load = theme;
			t.Invoker = new PageInvoker (pd);
			string html = t.Run ();
			if (html.IndexOf ("testing") < 0) {
				Assert.Fail ("WizardStepBase themes not applyed when EnableTheming = true");
			}
			pd.Load = notheme;
			t.Invoker = new PageInvoker (pd);
			html = t.Run ();
			if (html.IndexOf ("testing") > 0) {
				Assert.Fail ("WizardStepBase themes applyed when EnableTheming = false");
			}
		}

		public static void set_properties (Page p)
		{
			p.Theme = "Theme1";
		}

		public static void theme (Page p)
		{
			Wizard w = new Wizard ();
			PokerWizardStepBase ws = new PokerWizardStepBase ();
			ws.Controls.Add (new Button ());
			ws.EnableTheming = true;
			ws.SkinID = "WizardTest";
			w.WizardSteps.Add (ws);
			p.Form.Controls.Add (w);
		}

		public static void notheme (Page p)
		{
			Wizard w = new Wizard ();
			PokerWizardStepBase ws = new PokerWizardStepBase ();
			ws.Controls.Add (new Button ());
			ws.EnableTheming = false;
			ws.SkinID = "WizardTest";
			w.WizardSteps.Add (ws);
			p.Form.Controls.Add (w);
		}

		private void eventchecker (object o, EventArgs e)
		{
			event_checker = true;
		}

		private void eventassert (string message)
		{
			Assert.IsTrue (event_checker, message);
			event_checker = false;
		}

		public static void read_control (Page p)
		{
			ArrayList list = new ArrayList ();
			recurcive_find (list, typeof (LinkButton), p.FindControl ("Wizard"));
			WebTest.CurrentTest.UserData = list;
		}

		public static void recurcive_find (ArrayList list, Type t, Control control)
		{
			foreach (Control c in control.Controls) {
				if (c == null)
					continue;
				if (t == c.GetType ()) {
					list.Add (c.UniqueID);
				}
				recurcive_find (list, t, c);
			}
		}
	}
}
#endif
