//
// Tests for System.Web.UI.WebControls.TextBox.cs 
//
// Author:
//     Ben Maurer (bmaurer@novell.com)
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

namespace MonoTests.System.Web.UI.WebControls {
	
	[TestFixture]	
	public class TextBoxTest {
		class Poker : TextBox {
			public new void AddParsedSubObject (object o)
			{
				base.AddParsedSubObject (o);
			}

			public void TrackState () 
			{
				TrackViewState ();
			}
			
			public object SaveState ()
			{
				foreach (string s in ViewState.Keys)
					Console.WriteLine ("{0}: {1}", s, ViewState[s]);

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
		}

		[TestFixtureSetUp]
		public void SetUp ()
		{
			WebTest.CopyResource (GetType (), "TextBoxTestlPage.aspx", "TextBoxTestlPage.aspx");
			WebTest.CopyResource (GetType (), "NoEventValidation.aspx", "NoEventValidation.aspx");
		}

		[Test]
		public void Defaults ()
		{
			Poker p = new Poker ();
#if NET_2_0
			Assert.AreEqual (string.Empty, p.ValidationGroup, "ValidationGroup");
			Assert.AreEqual (false, p.CausesValidation, "CausesValidation");
#endif
		}

		[Test]
		public void Defaults_NotWorking ()
		{
			Poker p = new Poker ();
#if NET_2_0
			Assert.AreEqual (AutoCompleteType.None, p.AutoCompleteType, "AutoCompleteType");
#endif
		}

		[Test]
		public void MultilineRenderEscape ()
		{
			Poker t = new Poker ();
			t.TextMode = TextBoxMode.MultiLine;
			t.Text = "</textarea>";
#if NET_4_0
			string exp = "<textarea rows=\"2\" cols=\"20\">\r\n&lt;/textarea&gt;</textarea>";
#else
			string exp = "<textarea rows=\"2\" cols=\"20\">&lt;/textarea&gt;</textarea>";
#endif

			HtmlDiff.AssertAreEqual(exp, t.Render (),"MultilineRenderEscape");
		}


#if NET_2_0
		[Test]
		public void ValidationProperties ()
		{
			Poker t = new Poker ();

			// initial values
			Assert.AreEqual (false, t.CausesValidation, "A1");
			Assert.AreEqual ("", t.ValidationGroup, "A2");

			t.ValidationGroup = "VG";
			Assert.AreEqual ("VG", t.ValidationGroup, "A3");

			t.CausesValidation = true;
			Assert.IsTrue (t.CausesValidation, "A4");
		}

		[Test]
		public void ViewState ()
		{
			Poker t = new Poker ();

			t.TrackState();

			t.ValidationGroup = "VG";
			t.CausesValidation = true;

			object s = t.SaveState ();
			Console.WriteLine ("state = {0}", s == null ? "null" : "not-null");

			Poker copy = new Poker ();

			copy.LoadState (s);

			Assert.AreEqual ("VG", copy.ValidationGroup, "A1");
			Assert.IsTrue (copy.CausesValidation, "A2");
		}

		[Test]
		public void ValidationRender ()
		{
			/* test to show that the validation settings
			 * have no effect on downlevel rendering */
			Poker t = new Poker ();

			t.TrackState();

			t.ValidationGroup = "VG";
			t.CausesValidation = true;
			t.TextMode = TextBoxMode.MultiLine;
#if NET_4_0
			string exp = "<textarea rows=\"2\" cols=\"20\">\r\n</textarea>";
#else
			string exp = "<textarea rows=\"2\" cols=\"20\"></textarea>";
#endif
			HtmlDiff.AssertAreEqual (exp, t.Render (),"ValidationRender");
		}

		[Test]
		[Category ("NunitWeb")]
		public void CausesValidation_ValidationGroup ()
		{
			WebTest t = new WebTest ("TextBoxTestlPage.aspx");
			string str = t.Run ();
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls.Add ("TextBox1");
			fr.Controls["__EVENTTARGET"].Value = "TextBox1";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			fr.Controls["TextBox1"].Value = "TestValue";
			t.Request = fr;
			string html = t.Run ();

			if (html.IndexOf ("Validate_validation_group") == -1)
				Assert.Fail ("Validate not created");
			if (html.IndexOf ("MyValidationGroup") == -1)
				Assert.Fail ("Wrong validation group");
		}

		#region Help_class
		public class PokerL : TextBox
		{
			public string Render ()
			{
				StringWriter sw = new StringWriter ();
				sw.NewLine = "\n";
				HtmlTextWriter writer = new HtmlTextWriter (sw);
				base.Render (writer);
				return writer.InnerWriter.ToString ();
			}			

			public new void RaisePostDataChangedEvent ()
			{
				base.RaisePostDataChangedEvent ();
			}

			protected override bool LoadPostData (string postDataKey, global::System.Collections.Specialized.NameValueCollection postCollection)
			{
				if (WebTest.CurrentTest.UserData == null) {
					ArrayList list = new ArrayList ();
					list.Add ("LoadPostData");
					WebTest.CurrentTest.UserData = list;
				}
				else {
					ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
					if (list == null)
						throw new NullReferenceException ();
					list.Add ("LoadPostData");
					WebTest.CurrentTest.UserData = list;
				}
				return base.LoadPostData (postDataKey, postCollection);
			}

			protected override void OnLoad (EventArgs e)
			{
				if (this.Page.IsPostBack) {
					if (WebTest.CurrentTest.UserData == null) {
						ArrayList list = new ArrayList ();
						list.Add ("ControlLoad");
						WebTest.CurrentTest.UserData = list;
					}
					else {
						ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
						if (list == null)
							throw new NullReferenceException ();
						list.Add ("ControlLoad");
						WebTest.CurrentTest.UserData = list;
					}
				}
				base.OnLoad (e);
			}
		}
		#endregion

		[Test]
		[Category ("NunitWeb")]
		public void LoadPostData_Flow ()  //Just flow and not implementation detail
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (LoadPostData_Load));
			string html = t.Run ();
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls.Add ("pb");
			fr.Controls["__EVENTTARGET"].Value = "pb";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			fr.Controls["pb"].Value = "TestValue";
			t.Request = fr;
			t.Run ();

			ArrayList eventlist = t.UserData as ArrayList;
			if (eventlist == null)
				Assert.Fail ("User data does not been created fail");
			Assert.AreEqual ("PageLoad", eventlist[0], "Live Cycle Flow #1");
			Assert.AreEqual ("ControlLoad", eventlist[1], "Live Cycle Flow #2");
			Assert.AreEqual ("LoadPostData", eventlist[2], "Live Cycle Flow #3");
		}

		public static void LoadPostData_Load (Page p)
		{
			PokerL b = new PokerL ();
			b.AutoPostBack = true;
			b.ID = "pb";
			p.Form.Controls.Add (b);
			if (p.IsPostBack) {
				if (WebTest.CurrentTest.UserData == null) {
					ArrayList list = new ArrayList ();
					list.Add ("PageLoad");
					WebTest.CurrentTest.UserData = list;
				}
				else {
					ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
					if (list == null)
						throw new NullReferenceException ();
					list.Add ("PageLoad");
					WebTest.CurrentTest.UserData = list;
				}
			}
		}

		[Test]
		[Category ("NunitWeb")]
		public void LoadPostData ()  
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (LoadPostData__Load));
			string html = t.Run ();
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls.Add ("pb");
			fr.Controls["__EVENTTARGET"].Value = "pb";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			fr.Controls["pb"].Value = "TestValue";
			t.Request = fr;
			html = t.Run ();

			ArrayList eventlist = t.UserData as ArrayList;
			if (eventlist == null)
				Assert.Fail ("User data does not been created fail");
			Assert.AreEqual ("ControlLoad", eventlist[0], "Live Cycle Flow #1");
			Assert.AreEqual ("LoadPostData", eventlist[1], "Live Cycle Flow #2");
			Assert.AreEqual ("TextChanged", eventlist[2], "Live Cycle Flow #3");

			if (html.IndexOf ("TestValue") == -1)
				Assert.Fail ("Wrong value failed");
		}

		public static void LoadPostData__Load (Page p)
		{
			PokerL b = new PokerL ();
			b.ID = "pb";
			p.Form.Controls.Add (b);
			b.TextChanged += new EventHandler (b_TextChanged);
			if (p.IsPostBack)
				p.Response.Write (b.Text);
		}

		public static void b_TextChanged (object sender, EventArgs e)
		{
			if (WebTest.CurrentTest.UserData == null) {
				ArrayList list = new ArrayList ();
				list.Add ("TextChanged");
				WebTest.CurrentTest.UserData = list;
			}
			else {
				ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
				if (list == null)
					throw new NullReferenceException ();
				list.Add ("TextChanged");
				WebTest.CurrentTest.UserData = list;
			}
		}

		[Test]
		public void RaisePostDataChangedEvent ()
		{
			PokerL p = new PokerL ();
			p.TextChanged += new EventHandler (p_TextChanged);
			Assert.AreEqual (false, eventTextChanged, "RaisePostDataChangedEvent#1");
			p.RaisePostDataChangedEvent ();
			Assert.AreEqual (true, eventTextChanged, "RaisePostDataChangedEvent#2");
		}

		bool eventTextChanged;
		void  p_TextChanged(object sender, EventArgs e)
		{
			eventTextChanged = true;	
		}

		[Test]
		public void AutoCompleteType_Test ()
		{
			WebTest t = new WebTest ("NoEventValidation.aspx");
			t = new WebTest (PageInvoker.CreateOnLoad (AutoCompleteType__Load));
			string html = t.Run ();
			string orig ="<input name=\"Poker\" type=\"text\" vcard_name=\"vCard.FirstName\" id=\"Poker\" />";
			HtmlDiff.AssertAreEqual (orig, HtmlDiff.GetControlFromPageHtml (html), "AutoCompleteType");
		}

		public static void AutoCompleteType__Load (Page page)
		{
			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);

			PokerL p = new PokerL ();
			p.ID = "Poker";
			p.AutoCompleteType = AutoCompleteType.FirstName;
			page.Form.Controls.Add (lcb);
			page.Form.Controls.Add (p);
			page.Form.Controls.Add (lce);
		}

		[TestFixtureTearDown]
		public void TearDown ()
		{
			WebTest.Unload ();
		}
#endif
	}
}

