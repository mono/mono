//
// RadioButtonTest.cs
//	- Unit tests for System.Web.UI.WebControls.RadioButton
//
// Author:
//	Dick Porter  <dick@ximian.com>
//      Yoni Klain   <yonik@mainsoft.com>
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

using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using NUnit.Framework;
using MonoTests.SystemWeb.Framework;
using System.Collections;

namespace MonoTests.System.Web.UI.WebControls {

	public class TestRadioButton : RadioButton {
		public StateBag StateBag {
			get { return base.ViewState; }
		}

		public string Render ()
		{
			HtmlTextWriter writer = new HtmlTextWriter (new StringWriter ());
			base.Render (writer);
			return writer.InnerWriter.ToString ();
		}
#if NET_2_0
		public new void RaisePostDataChangedEvent ()
		{
			base.RaisePostDataChangedEvent ();
		}
#endif
	}

	[TestFixture]
	public class RadioButtonTest {

		[Test]
		public void DefaultProperties ()
		{
			TestRadioButton r = new TestRadioButton ();
			
			Assert.AreEqual (0, r.Attributes.Count, "Attributes.Count");

			Assert.IsFalse (r.AutoPostBack, "AutoPostBack");
			Assert.IsFalse (r.Checked, "Checked");
			Assert.AreEqual (String.Empty, r.Text, "Text");
			Assert.AreEqual (TextAlign.Right, r.TextAlign, "TextAlign");
			Assert.AreEqual (String.Empty, r.GroupName, "GroupName");
			
			Assert.AreEqual (0, r.Attributes.Count, "Attributes.Count-2");
		}

		[Test]
		public void NullProperties ()
		{
			TestRadioButton r = new TestRadioButton ();
			
			r.Text = null;
			Assert.AreEqual (String.Empty, r.Text, "Text");
			r.TextAlign = TextAlign.Right;
			Assert.AreEqual (TextAlign.Right, r.TextAlign, "TextAlign");
			r.AutoPostBack = true;
			Assert.IsTrue (r.AutoPostBack, "AutoPostBack");
			r.Checked = true;
			Assert.IsTrue (r.Checked, "Checked");
			r.GroupName = null;
			Assert.AreEqual (String.Empty, r.GroupName, "GroupName");
			
			Assert.AreEqual (0, r.Attributes.Count, "Attributes.Count");
			Assert.AreEqual (3, r.StateBag.Count, "ViewState.Count-1");
		}

		[Test]
		public void CleanProperties ()
		{
			TestRadioButton r = new TestRadioButton ();

			r.Text = "text";
			Assert.AreEqual ("text", r.Text, "Text");
			r.AutoPostBack = true;
			r.TextAlign = TextAlign.Left;
			r.Checked = true;
			r.GroupName = "groupname";
			Assert.AreEqual ("groupname", r.GroupName, "GroupName");
			
			Assert.AreEqual (5, r.StateBag.Count, "ViewState.Count");
			Assert.AreEqual (0, r.Attributes.Count, "Attributes.Count");

			r.Text = null;
			r.AutoPostBack = false;
			r.TextAlign = TextAlign.Right;
			r.Checked = false;
			r.GroupName = null;
			
			// If Text is null it is removed from the
			// ViewState.  Ditto GroupName
			Assert.AreEqual (3, r.StateBag.Count, "ViewState.Count-2");
			Assert.AreEqual (TextAlign.Right, r.StateBag["TextAlign"], "TextAlign");
			Assert.AreEqual (0, r.Attributes.Count, "Attributes.Count-2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TextAlign_Invalid ()
		{
			RadioButton r = new RadioButton ();
			r.TextAlign = (TextAlign)Int32.MinValue;
		}

		[Test]
		public void TextAlign_Values ()
		{
			RadioButton r = new RadioButton ();

			foreach (TextAlign ta in Enum.GetValues (typeof (TextAlign))) {
				r.TextAlign = ta;
			}
		}
		
		/* Segfaults on ms runtime */
		[Test]
		public void Render ()
		{
			TestRadioButton r = new TestRadioButton ();

			string s = r.Render ();

			Assert.IsTrue (s.IndexOf (" type=\"radio\"") > 0, "type");

			r.Text = "label text";
			s = r.Render ();
			Assert.IsTrue (s.IndexOf (">label text</label>") > 0, "text");

			r.TextAlign = TextAlign.Left;
			s = r.Render ();
			Assert.IsTrue (s.IndexOf (">label text</label><input") > 0, "text left");
			r.TextAlign = TextAlign.Right;
			s = r.Render ();
			Assert.IsTrue (s.IndexOf ("/><label for") > 0, "text right");
		}

		[Test]
		public void NameAttr1 ()
		{
			TestRadioButton b1 = new TestRadioButton ();
			b1.GroupName = "mono";
			TestRadioButton b2 = new TestRadioButton ();
			b2.GroupName = "mono";
			Page p = new Page ();
#if NET_2_0
			p.EnableEventValidation = false;
#endif
			p.ID = "MyPage";
			p.Controls.Add (b1);
			p.Controls.Add (b2);
			string t1 = b1.Render ();
			Assert.IsTrue (t1.IndexOf ("name=\"mono\"") != -1, "#01");
			string t2 = b2.Render ();
			Assert.IsTrue (t2.IndexOf ("name=\"mono\"") != -1, "#02");
		}

		[Test]
		public void NameAttr2 () {
			TestRadioButton b1 = new TestRadioButton ();
			b1.ID = "monoId";
			Page p = new Page ();
#if NET_2_0
			p.EnableEventValidation = false;
#endif
			p.ID = "MyPage";
			p.Controls.Add (b1);
			string t1 = b1.Render ();
			Assert.IsTrue (t1.IndexOf ("name=\"monoId\"") != -1, "#01");
		}

#if NET_2_0
		[Test]
		public void RaisePostDataChangedEvent ()
		{
			TestRadioButton b = new TestRadioButton ();
			b.CheckedChanged += new EventHandler (b_CheckedChanged);
			Assert.AreEqual (false, eventCheckedChanged, "before");
			b.RaisePostDataChangedEvent ();
			Assert.AreEqual (true, eventCheckedChanged, "after");
		}

		bool eventCheckedChanged;
		void b_CheckedChanged (object sender, EventArgs e)
		{
			eventCheckedChanged = true;
		}

		[Test]
		[Category ("NunitWeb")]
		public void RaisePostDataChangedEvent_PostBack ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnInit (RaisePostDataChangedEvent_Init));
			string html = t.Run ();
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls.Add ("RadioButton1");
			fr.Controls["__EVENTTARGET"].Value = "RadioButton1";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			fr.Controls["RadioButton1"].Value = "RadioButton1";
			t.Request = fr;
			event_CheckedChanged2_flag = false;
			html = t.Run ();
			if (t.UserData == null)
				Assert.Fail ("RaisePostDataChangedEvent Failed#1");
			Assert.AreEqual ("CheckedChanged", (string) t.UserData, "RaisePostDataChangedEvent Failed#2");
			Assert.IsFalse (event_CheckedChanged2_flag, "RaisePostDataChangedEvent Failed#3");
		}

		public static void RaisePostDataChangedEvent_Init (Page p)
		{
			TestRadioButton b = new TestRadioButton ();
			b.ID = "RadioButton1";
			b.GroupName = "RadioButton1";
			b.CheckedChanged+=new EventHandler(event_CheckedChanged);
			p.Form.Controls.Add (b);
			
			TestRadioButton b2 = new TestRadioButton ();
			b2.ID = "RadioButton2";
			b2.GroupName = "RadioButton1";
			b2.CheckedChanged += new EventHandler (event_CheckedChanged2);
			p.Form.Controls.Add (b2);
			if (!p.IsPostBack)
				b2.Checked = true;
		}

		public static void event_CheckedChanged (object sender, EventArgs e)
		{
			WebTest.CurrentTest.UserData = "CheckedChanged";
		}
		static bool event_CheckedChanged2_flag;
		public static void event_CheckedChanged2 (object sender, EventArgs e)
		{
			event_CheckedChanged2_flag = true;
		}

		#region help_class
		class Poker : RadioButton
		{
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

			protected internal override void OnLoad (EventArgs e)
			{
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
				base.OnLoad (e);
			}
		}
		#endregion

		[Test]
		[Category ("NunitWeb")]
		public void LoadPostData ()  //Just flow and not implementation detail
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (LoadPostData_Load));
			string html = t.Run ();
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls.Add ("RadioButton1");
			fr.Controls["__EVENTTARGET"].Value = "__Page";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			fr.Controls ["RadioButton1"].Value = "RadioButton1";
			t.Request = fr;
			t.Run ();

			ArrayList eventlist = t.UserData as ArrayList;
			if (eventlist == null)
				Assert.Fail ("User data does not been created fail");
			Assert.AreEqual ("ControlLoad", eventlist[0], "Live Cycle Flow #1");
			Assert.AreEqual ("PageLoad", eventlist[1], "Live Cycle Flow #2");
			Assert.AreEqual ("ControlLoad", eventlist[2], "Live Cycle Flow #3");
			Assert.AreEqual ("LoadPostData", eventlist[3], "Live Cycle Flow #4");

		}

		public static void LoadPostData_Load (Page p)
		{
			Poker b = new Poker();
			b.ID = "RadioButton1";
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


		[TestFixtureTearDown]
		public void TearDown ()
		{
			WebTest.Unload ();
		}
#endif
	}
}
