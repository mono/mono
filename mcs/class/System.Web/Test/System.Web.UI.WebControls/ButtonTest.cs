//
// Tests for System.Web.UI.WebControls.Button.cs
//
// Author:
//	Jordi Mas i Hernandez (jordi@ximian.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
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
//

using NUnit.Framework;
using System;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;

namespace MonoTests.System.Web.UI.WebControls
{
	class PokerButton : Button {
		public PokerButton ()
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

		public string Render ()
		{
			StringWriter sw = new StringWriter ();
			sw.NewLine = "\n";
			HtmlTextWriter writer = new HtmlTextWriter (sw);
			base.Render (writer);
			return writer.InnerWriter.ToString ();
		}		
		public new PostBackOptions GetPostBackOptions ()
		{
			return base.GetPostBackOptions ();
		}

		public new void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
		}

		public new void RaisePostBackEvent (string eventArgument)
		{
			base.RaisePostBackEvent (eventArgument);
		}
	}


	[TestFixture]
	public class ButtonTest
	{
		[TestFixtureSetUp]
		public void SetUp ()
		{
			WebTest.CopyResource (GetType (), "ButtonColor_Bug325489.aspx", "ButtonColor_Bug325489.aspx");
		}

		[Test]
		public void ButtonColor_Bug325489 ()
		{
			WebTest t = new WebTest ("ButtonColor_Bug325489.aspx");
			string origHtml = @"<input type=""submit"" name=""button1"" value="""" id=""button1"" style=""background-color:#316AC5;"" />";
			string html = t.Run ();
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (html);

			HtmlDiff.AssertAreEqual (origHtml, renderedHtml, "#A1");
		}
		
		[Test]
		public void Button_DefaultValues ()
		{
			Button b = new Button ();
			Assert.AreEqual (true, b.CausesValidation, "CausesValidation");
			Assert.AreEqual (string.Empty, b.CommandArgument, "CommandArgument");
			Assert.AreEqual (string.Empty, b.CommandName, "CommandName");			
			Assert.AreEqual (string.Empty, b.ValidationGroup, "ValidationGroup");
			Assert.AreEqual (string.Empty, b.OnClientClick, "OnClientClick");
			Assert.AreEqual (string.Empty, b.PostBackUrl, "PostBackUrl");
			Assert.AreEqual (true, b.UseSubmitBehavior, "UseSubmitBehavior");
		}

		[Test]
		public void AssignProperties ()
		{
			Button b = new Button ();
			Assert.AreEqual (string.Empty, b.OnClientClick, "OnClientClick#1");
			b.OnClientClick = "Test()";
			Assert.AreEqual ("Test()", b.OnClientClick, "OnClientClick#2");
			Assert.AreEqual (string.Empty, b.PostBackUrl, "PostBackUrl");
			b.PostBackUrl = "Test";
			Assert.AreEqual ("Test", b.PostBackUrl, "PostBackUrl");
			Assert.AreEqual (true, b.UseSubmitBehavior, "UseSubmitBehavior#1");
			b.UseSubmitBehavior = false;
			Assert.AreEqual (false, b.UseSubmitBehavior, "UseSubmitBehavior#2");
			Assert.AreEqual (string.Empty, b.ValidationGroup, "ValidationGroup#1");
			b.ValidationGroup = "test";
			Assert.AreEqual ("test", b.ValidationGroup, "ValidationGroup#2");
		}

		[Test]
		public void Button_ViewState ()
		{
			PokerButton p = new PokerButton ();

			Assert.AreEqual (p.Text, "", "A1");
			p.Text = "Hello";
			Assert.AreEqual (p.Text, "Hello", "A2");

			p.ValidationGroup = "VG1";
			p.UseSubmitBehavior = false;
			p.OnClientClick = "ClientClick()";
			p.PostBackUrl = "PostBackUrl";
			Assert.AreEqual (p.ValidationGroup, "VG1", "A3");
			Assert.AreEqual (false, p.UseSubmitBehavior, "ViewState_UseSubmitBehavior#original");
			Assert.AreEqual ("ClientClick()", p.OnClientClick, "ViewState_OnClientClick#original");
			Assert.AreEqual ("PostBackUrl", p.PostBackUrl, "ViewState_PostBackUrl#original");

			object state = p.SaveState ();

			PokerButton copy = new PokerButton ();
			copy.LoadState (state);
			Assert.AreEqual (copy.Text, "Hello", "A4");

			Assert.AreEqual (copy.ValidationGroup, "VG1", "A5");
			Assert.AreEqual (false, copy.UseSubmitBehavior, "ViewState_UseSubmitBehavior#copy");
			Assert.AreEqual ("ClientClick()", p.OnClientClick, "ViewState_OnClientClick#copy");
			Assert.AreEqual ("PostBackUrl", p.PostBackUrl, "ViewState_PostBackUrl#copy");
		}

		[Test]
		public void Button_Render ()
		{
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);

			Button b = new Button ();
			b.Text = "Hello";
			b.RenderControl (tw);
			
			Assert.AreEqual (true, sw.ToString().IndexOf ("value=\"Hello\"") != -1, "A4");
			Assert.AreEqual (true, sw.ToString().IndexOf ("<input") != -1, "A5");
			Assert.AreEqual (true, sw.ToString().IndexOf ("type=\"submit\"") != -1, "A6");
		}

		[Test]
		public void IgnoresChildren ()
		{
			Button b = new  Button ();
			b.Controls.Add (new LiteralControl ("hola"));
			Assert.AreEqual (1, b.Controls.Count, "controls");
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);
			b.RenderControl (tw);
			string str = tw.ToString ();
			Assert.AreEqual (-1, str.IndexOf ("hola"), "hola");
		}

		[Test]
		public void Button_Render2 () {
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);
			
			Button b = new Button ();
			b.ID = "MyButton";
			b.Text = "Hello";
			b.UseSubmitBehavior = false;
			b.Enabled = false;
			b.ToolTip = "Hello_ToolTip";
			b.RenderControl (tw);
			
			string strTarget = "<input type=\"button\" name=\"MyButton\" value=\"Hello\" id=\"MyButton\" disabled=\"disabled\" class=\"aspNetDisabled\" title=\"Hello_ToolTip\" />";
			string str = sw.ToString();
			HtmlDiff.AssertAreEqual (strTarget, str, "Button_Render2");
		}

		[Test]
		public void GetPostBackOptions ()
		{
			PokerButton b = new PokerButton ();
			PostBackOptions opt = b.GetPostBackOptions ();
			Assert.AreEqual (typeof (PokerButton), opt.TargetControl.GetType (), "GetPostBackOptions#1");
		}

		[Test]
		public void OnPreRender ()
		{
			PokerButton b = new PokerButton ();
			b.PreRender += new EventHandler (b_PreRender);
			Assert.AreEqual (false, eventPreRender, "Before PreRender");
			b.OnPreRender (new EventArgs ());
			Assert.AreEqual (true, eventPreRender, "After PreRender");
		}

		bool eventPreRender;
		void b_PreRender (object sender, EventArgs e)
		{
			eventPreRender = true;
		}

		[Test]
		[Category("NunitWeb")]
		public void PostBackUrl ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (PostBackUrl_Load));
			string html = t.Run ();
			if (html.IndexOf ("onclick") == -1)
				Assert.Fail ("Button Postback script not created fail");
			if (html.IndexOf ("MyPageWithMaster.aspx") == -1)
				Assert.Fail ("Link to postback page not created fail");
			if (html.IndexOf ("__PREVIOUSPAGE") == -1)
				Assert.Fail ("Previos page hidden control not created fail");
		}

		public static void PostBackUrl_Load (Page p)
		{
			PokerButton b = new PokerButton ();
			b.PostBackUrl = "~/MyPageWithMaster.aspx";
			p.Form.Controls.Add (b);
		}

		[Test]
		public void RaisePostBackEvent ()
		{
			Page p = new Page ();
			PokerButton b = new PokerButton ();
			b.Click += new EventHandler (b_Click);
			p.Controls.Add (b);
			Assert.AreEqual (false, eventRaisePostBackEvent, "RaisePostBackEvent#1");
			b.RaisePostBackEvent ("Click");
			Assert.AreEqual (true, eventRaisePostBackEvent, "RaisePostBackEvent#2");
		}

		bool eventRaisePostBackEvent;
		void b_Click (object sender, EventArgs e)
		{
			eventRaisePostBackEvent = true;
		}

		[Test]
		[Category ("NunitWeb")]
		public void UseSubmitBehavior ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (UseSubmitBehavior_Load));
			string html = t.Run ();
			if (html.IndexOf ("onclick") == -1)
				Assert.Fail ("Button Postback script not created fail");
		}

		public static void UseSubmitBehavior_Load (Page p)
		{
			PokerButton b = new PokerButton ();
			b.UseSubmitBehavior = false;
			p.Controls.Add (b);
		}

		[Test]
		public void ValidationGroup ()
		{
			// Client side. 
		}

		[TestFixtureTearDown]
		public void TearDown ()
		{
			WebTest.Unload ();
		}

	}
}


