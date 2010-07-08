//
// Tests for System.Web.UI.WebControls.ImageButton.cs
//
// Author:
//	Jordi Mas i Hernandez (jordi@ximian.com)
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
using System.Collections;

namespace MonoTests.System.Web.UI.WebControls
{
	class PokerImageButton : ImageButton {
		public PokerImageButton ()
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
#if NET_2_0
		public new string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				base.Text = value;
			}
		}


		public new void RaisePostBackEvent (string eventArgument)
		{
			base.RaisePostBackEvent (eventArgument);
		}

		protected override void RaisePostDataChangedEvent ()
		{
			base.RaisePostDataChangedEvent ();
		}
#endif
	}

	[TestFixture]
	public class ImageButtonTest {
		
		[Test]
		[Category("NotWorking")]
		public void ImageButton_DefaultValues ()
		{
			ImageButton b = new ImageButton ();
			Assert.AreEqual (true, b.CausesValidation, "CausesValidation");
			Assert.AreEqual (string.Empty, b.CommandArgument, "CommandArgument");
			Assert.AreEqual (string.Empty, b.CommandName, "CommandName");
#if NET_2_0
			Assert.AreEqual (string.Empty, b.ValidationGroup, "ValidationGroup");
			Assert.AreEqual (string.Empty, b.AppRelativeTemplateSourceDirectory, "AppRelativeTemplateSourceDirectory");
			Assert.AreEqual (string.Empty, b.DescriptionUrl, "DescriptionUrl");
			Assert.AreEqual (true, b.EnableTheming, "EnableTheming");
			Assert.AreEqual (false, b.GenerateEmptyAlternateText, "GenerateEmptyAlternateText");
			Assert.AreEqual (string.Empty, b.PostBackUrl, "PostBackUrl");
			Assert.AreEqual (string.Empty, b.OnClientClick, "OnClientClick");
#endif
		}

		[Test]
		public void ImageButton_AssignedValues ()
		{
			ImageButton b = new ImageButton ();
#if NET_2_0
			Assert.AreEqual (string.Empty, b.ValidationGroup, "ValidationGroup#1");
			b.ValidationGroup = "test";
			Assert.AreEqual ("test", b.ValidationGroup, "ValidationGroup#2");
			// NOTE:  Default is wrong! 
			// Assert.AreEqual (string.Empty, b.AppRelativeTemplateSourceDirectory, "AppRelativeTemplateSourceDirectory#1");
			b.AppRelativeTemplateSourceDirectory = "~/test";
			Assert.AreEqual ("~/test", b.AppRelativeTemplateSourceDirectory, "AppRelativeTemplateSourceDirectory#2");
			Assert.AreEqual (string.Empty, b.DescriptionUrl, "DescriptionUrl#1");
			b.DescriptionUrl = "test";
			Assert.AreEqual ("test", b.DescriptionUrl, "DescriptionUrl#2");
			Assert.AreEqual (true, b.EnableTheming, "EnableTheming#1");
			b.EnableTheming  = false;
			Assert.AreEqual (false, b.EnableTheming, "EnableTheming#2");
#endif
		}


#if NET_2_0
		[Test]
		[Category ("NunitWeb")]
		public void AppRelativeTemplateSourceDirectory ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (AppRelativeTemplateSourceDirectory_Load));
			t.Run ();
		}
	
		public static void AppRelativeTemplateSourceDirectory_Load (Page p)
		{
			PokerImageButton i = new PokerImageButton ();
			i.ID = "I";
			Assert.AreEqual ("~/", i.AppRelativeTemplateSourceDirectory, "AppRelativeTemplateSourceDirectory");
		}

		[Test]
		public void DescriptionUrl ()
		{
			PokerImageButton i = new PokerImageButton ();
			i.DescriptionUrl = "URLDescription";
			string html = i.Render ();
			if (html.IndexOf ("longdesc=\"URLDescription\"") == -1)
				Assert.Fail ("DescriptionUrl Failed");
		}

		[Test]
		public void OnClientClick ()
		{
			PokerImageButton b = new PokerImageButton ();
			b.OnClientClick = "MyMethod";
			string html = b.Render ();
			if (html.IndexOf ("onclick=\"MyMethod;\"") == -1)
				Assert.Fail ("OnClientClick#1");
		}

		[Test] // Bug #463939
		public void OnClientClickEmpty ()
		{
			PokerImageButton b = new PokerImageButton ();
			string html = b.Render ();
			Assert.AreEqual (-1, html.IndexOf ("onclick=\""), "#A1");

			b.OnClientClick = String.Empty;
			html = b.Render ();
			Assert.AreEqual (-1, html.IndexOf ("onclick=\""), "#A2");

			b.OnClientClick = null;
			html = b.Render ();
			Assert.AreEqual (-1, html.IndexOf ("onclick=\""), "#A3");
		}
		
		[Test]
		[Category ("NunitWeb")]
		public void PostBackUrl ()
		{
			WebTest test = new WebTest (PageInvoker.CreateOnLoad (PostBackUrl_load));
			string html = HtmlDiff.GetControlFromPageHtml (test.Run ());
			if (html.IndexOf ("onclick") == -1)
				Assert.Fail ("PostBack script not created");
			if (html.IndexOf ("MyURL.aspx") == -1)
				Assert.Fail ("PostBack page URL not set");
			if (html.IndexOf ("~/MyURL.aspx") != -1)
				Assert.Fail ("PostBack page URL is not resolved");
		}
		
		public static void PostBackUrl_load (Page p)
		{
			PokerImageButton b = new PokerImageButton ();
			p.Form.Controls.Add (new LiteralControl(HtmlDiff.BEGIN_TAG));
			p.Form.Controls.Add (b);
			p.Form.Controls.Add (new LiteralControl (HtmlDiff.END_TAG));
			b.PostBackUrl = "~/MyURL.aspx";
		}

		[Test]
		[Category ("NunitWeb")]
		public void ValidationGroup ()
		{
			WebTest.CopyResource (GetType (), "NoEventValidation.aspx", "NoEventValidation.aspx");
			WebTest t = new WebTest ("NoEventValidation.aspx");
			t.Invoker = PageInvoker.CreateOnLoad (ValidationGroup_Load);
			string html = HtmlDiff.GetControlFromPageHtml (t.Run ());
			if (html.IndexOf ("onclick") == -1)
				Assert.Fail ("Validation script not created");
			if (html.IndexOf ("MyValidationGroup") == -1)
				Assert.Fail ("Validation group not set fail");
		}

		public static void ValidationGroup_Load(Page p)
		{
			PokerImageButton b = new PokerImageButton ();
			b.ValidationGroup = "MyValidationGroup";
			TextBox tb = new TextBox ();
			tb.ID = "tb";
			tb.ValidationGroup = "MyValidationGroup";
			RequiredFieldValidator v = new RequiredFieldValidator ();
			v.ControlToValidate = "tb";
			v.ValidationGroup = "MyValidationGroup";
			p.Form.Controls.Add (tb);
			p.Form.Controls.Add (v);
			p.Form.Controls.Add (new LiteralControl (HtmlDiff.BEGIN_TAG));
			p.Form.Controls.Add (b);
			p.Form.Controls.Add (new LiteralControl (HtmlDiff.END_TAG));
		}

		[Test]
		public void Text ()
		{
#if NET_4_0
			string origHtml = "<input type=\"image\" src=\"\" alt=\"MyText\" />";
#else
			string origHtml = "<input type=\"image\" src=\"\" alt=\"MyText\" style=\"border-width:0px;\" />";
#endif
			PokerImageButton b = new PokerImageButton ();
			b.Text = "MyText";
			string html = b.Render ();
			HtmlDiff.AssertAreEqual (origHtml, html, "Text#1");
		}

		[Test]
		[Category("NunitWeb")]
		public void RaisePostBackEvent ()
		{
			WebTest.CopyResource (GetType (), "NoEventValidation.aspx", "NoEventValidation.aspx");
			WebTest t = new WebTest ("NoEventValidation.aspx");
			t.Invoker = PageInvoker.CreateOnLoad (RaisePostBackEvent_Load);
			t.Run ();
			ArrayList eventlist = t.UserData as ArrayList;
			if (eventlist == null)
				Assert.Fail ("User data does not been created fail");

			Assert.AreEqual ("Click", eventlist[0], "Event Flow #0");
			Assert.AreEqual ("Command", eventlist[1], "Event Flow #1");
		}

		public static void RaisePostBackEvent_Load (Page p)
		{
			PokerImageButton b = new PokerImageButton ();
			p.Form.Controls.Add (b);
			b.Click += new ImageClickEventHandler (b_Click);
			b.Command += new CommandEventHandler (b_Command);
			b.RaisePostBackEvent ("Click");
		}

		static void b_Command (object sender, CommandEventArgs e)
		{
			if (WebTest.CurrentTest.UserData == null) {
				ArrayList list = new ArrayList ();
				list.Add ("Command");
				WebTest.CurrentTest.UserData = list;
			}
			else {
				ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
				if (list == null)
					throw new NullReferenceException ();
				list.Add ("Command");
				WebTest.CurrentTest.UserData = list;
			}
		}

		static void b_Click (object sender, ImageClickEventArgs e)
		{
			if (WebTest.CurrentTest.UserData == null) {
				ArrayList list = new ArrayList ();
				list.Add ("Click");
				WebTest.CurrentTest.UserData = list;
			}
			else {
				ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
				if (list == null)
					throw new NullReferenceException ();
				list.Add ("Click");
				WebTest.CurrentTest.UserData = list;
			}
		}

#endif

		[Test]
		public void ImageButton_Render ()
		{
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);

			ImageButton b = new ImageButton ();			
			b.RenderControl (tw);				
			
			Assert.AreEqual (true, sw.ToString().IndexOf ("<input") != -1, "A1");
			Assert.AreEqual (true, sw.ToString().IndexOf ("type=\"image\"") != -1, "A2");
		}

		[Test]
		public void ImageButton_ViewState ()
		{
			PokerImageButton p = new PokerImageButton ();

			p.CommandArgument = "arg";
			Assert.AreEqual (p.CommandArgument, "arg", "A1");
			p.CommandName = "cmd";
			Assert.AreEqual (p.CommandName, "cmd", "A2");
#if NET_2_0
			p.ValidationGroup = "VG1";
			Assert.AreEqual (p.ValidationGroup, "VG1", "A3");
#endif

			object state = p.SaveState ();

			PokerImageButton copy = new PokerImageButton ();
			copy.LoadState (state);

			Assert.AreEqual (copy.CommandArgument, "arg", "A4");
			Assert.AreEqual (copy.CommandName, "cmd", "A5");
#if NET_2_0
			Assert.AreEqual (copy.ValidationGroup, "VG1", "A6");
#endif

		}

		[Test]
		public void RenderName ()
		{
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);

			Page page = new Page ();
#if NET_2_0
			page.EnableEventValidation = false;
#endif
			ImageButton b = new ImageButton ();			
			page.Controls.Add (b);
			page.RenderControl (tw);
			Assert.AreEqual (true, sw.ToString().IndexOf ("<input") != -1, "A1");
			Assert.AreEqual (true, sw.ToString().IndexOf ("type=\"image\"") != -1, "A2");
			Assert.AreEqual (true, sw.ToString().IndexOf ("name=\"") != -1, "A3");
		}
#if NET_2_0
		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void GenerateEmptyAlternateText_Exception ()
		{
			ImageButton b = new ImageButton ();
			b.GenerateEmptyAlternateText = true;
		}

		[TestFixtureTearDown]
		public void TearDown ()
		{
			WebTest.Unload ();
		}
#endif

	}
}


