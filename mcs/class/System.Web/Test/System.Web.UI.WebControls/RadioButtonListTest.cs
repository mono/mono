//
// Tests for System.Web.UI.WebControls.RadioButtonList.cs
//
// Authors:
//	Jordi Mas i Hernandez (jordi@ximian.com)
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

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

using System.Web.UI.WebControls;
using NUnit.Framework;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Globalization;
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;
using System.Drawing;
using System.Collections;


namespace MonoTests.System.Web.UI.WebControls {

	[TestFixture]
	public class RadioButtonListTest
	{
		#region help_classes
		public class TestRadioButtonList : RadioButtonList {
			public StateBag StateBag {
				get { return base.ViewState; }
			}

			public string Render ()
			{
				HtmlTextWriter writer = new HtmlTextWriter (new StringWriter ());
				base.Render (writer);
				return writer.InnerWriter.ToString ();
			}
		}

#if NET_2_0
		class PokerRadioButtonList : RadioButtonList
		{
			public StateBag StateBag
			{
				get { return base.ViewState; }
			}

			public string Render ()
			{
				HtmlTextWriter writer = new HtmlTextWriter (new StringWriter ());
				base.Render (writer);
				return writer.InnerWriter.ToString ();
			}

			protected override Style GetItemStyle (ListItemType itemType, int repeatIndex)
			{
				Style s = new Style ();
				s.BackColor = Color.Red;
				s.BorderStyle = BorderStyle.Solid;
				WebTest.CurrentTest.UserData = "GetItemStyle";
				return s;
			}

			public new bool HasFooter
			{
				get
				{
					return base.HasFooter;
				}
			}

			public new bool HasHeader
			{
				get
				{
					return base.HasHeader;
				}
			}

			public new bool HasSeparators
			{
				get
				{
					return base.HasSeparators;
				}
			}

			public new int RepeatedItemCount
			{
				get
				{
					return base.RepeatedItemCount;
				}
			}

			protected override void RaisePostDataChangedEvent ()
			{
				base.RaisePostDataChangedEvent ();
			}

			public void DoRaisePostDataChangedEvent ()
			{
				base.RaisePostDataChangedEvent ();
			}

			public new virtual void VerifyMultiSelect()
			{
				base.VerifyMultiSelect();
			}
		}
#endif
		#endregion

		[Test]
		public void RadioButtonList_Constructor ()
		{
			TestRadioButtonList r = new TestRadioButtonList ();
			Assert.AreEqual (-1, r.CellPadding, "A1");
			Assert.AreEqual (-1, r.CellSpacing, "A2");
			Assert.AreEqual (0, r.RepeatColumns, "A3");
			Assert.AreEqual (RepeatDirection.Vertical, r.RepeatDirection, "A4");
			Assert.AreEqual (RepeatLayout.Table, r.RepeatLayout, "A5");
			Assert.AreEqual (TextAlign.Right, r.TextAlign, "A6");
			Assert.AreEqual (false, ((IRepeatInfoUser)r).HasFooter, "A7");
			Assert.AreEqual (false, ((IRepeatInfoUser)r).HasHeader, "A8");
			Assert.AreEqual (false, ((IRepeatInfoUser)r).HasSeparators, "A9");
			Assert.AreEqual (0, ((IRepeatInfoUser)r).RepeatedItemCount, "A10");
		}

		[Test]
		public void CellPaddingProperties ()
		{
			TestRadioButtonList r = new TestRadioButtonList ();
			r.CellPadding = 5;
			Assert.AreEqual (5, r.CellPadding, "setting");

			string s = r.Render ();	
#if NET_2_0
			// FIXME: missing some info to start rendering ?
			Assert.AreEqual (String.Empty, s, "htmloutput");
#else
			Assert.IsTrue (s.ToLower ().IndexOf ("cellpadding=\"5\"") !=  -1, "htmloutput");
#endif
		}	

		[Test]
		public void CellSpacingProperties ()
		{
			TestRadioButtonList r = new TestRadioButtonList ();
			r.CellSpacing = 5;
			Assert.AreEqual (5, r.CellSpacing, "setting");

			string s = r.Render ();	
#if NET_2_0
			// FIXME: missing some info to start rendering ?
			Assert.AreEqual (String.Empty, s, "htmloutput");
#else
			Assert.IsTrue (s.ToLower ().IndexOf ("cellspacing=\"5\"") !=  -1, "htmloutput");
#endif
		}	

#if NET_2_0
		[Test]
		[Category ("NunitWeb")]
		public void Render ()
		{
			string RenderedPageHtml = new WebTest (PageInvoker.CreateOnLoad (Render_Load)).Run ();
			string RenderedControlHtml = HtmlDiff.GetControlFromPageHtml (RenderedPageHtml);
			string OriginControlHtml = "<table id=\"ctl01\" border=\"0\">\r\n\t<tr>\r\n\t\t<td><input id=\"ctl01_0\" type=\"radio\" name=\"ctl01\" value=\"value1\" /><label for=\"ctl01_0\">text2</label></td>\r\n\t</tr>\r\n</table>";
			HtmlDiff.AssertAreEqual (OriginControlHtml, RenderedControlHtml, "Render");
		}

		public static void Render_Load (Page p)
		{
			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);
			TestRadioButtonList c = new TestRadioButtonList ();
			p.Form.Controls.Add (lcb);
			p.Form.Controls.Add (c);
			p.Form.Controls.Add (lce);
			c.Items.Add (new ListItem ("text2", "value1"));
		}
#else
		[Test]
		public void Render ()
		{
			TestRadioButtonList c = new TestRadioButtonList ();

			c.Items.Add (new ListItem ("text2", "value1"));

			string s = c.Render ();

			Assert.IsTrue (s.ToLower ().IndexOf (" type=\"radio\"") !=  -1, "type");
			Assert.IsTrue (s.ToLower ().IndexOf ("value1") !=  -1, "value");
			Assert.IsTrue (s.ToLower ().IndexOf ("text2") !=  -1, "text");
		}
#endif

		// Exceptions
		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void RepeatColumnsException ()
		{
			TestRadioButtonList r = new TestRadioButtonList ();
			r.RepeatColumns = -1;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void RepeatDirectionException ()
		{
			TestRadioButtonList r = new TestRadioButtonList ();
			r.RepeatDirection = (RepeatDirection) 4;
		}


		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void RepeatLayoutException ()
		{
			TestRadioButtonList r = new TestRadioButtonList ();
			r.RepeatLayout = (RepeatLayout) 3;
		}

		bool event_called;
		void OnSelected (object sender, EventArgs args)
		{
			event_called = true;
		}

		[Test]
		public void LoadAndRaise1 ()
		{
			RadioButtonList rbl = new RadioButtonList ();
			rbl.Items.Add (new ListItem ("Uno", "1"));
			rbl.Items.Add (new ListItem ("Dos", "2"));
			rbl.Items.Add (new ListItem ("Tres", "3"));
			rbl.SelectedIndex = 2;
			NameValueCollection nvc = new NameValueCollection ();
			nvc ["XXX"] = "3";

			IPostBackDataHandler handler = (IPostBackDataHandler) rbl;
#if NET_2_0
			Assert.IsFalse (handler.LoadPostData ("XXX", nvc), "#01");
#else
			Assert.IsTrue (handler.LoadPostData ("XXX", nvc), "#01");
#endif
			rbl.SelectedIndexChanged += new EventHandler (OnSelected);
			event_called = false;
			handler.RaisePostDataChangedEvent ();
#if NET_2_0
			Assert.IsTrue (event_called, "#02");
#else
			// Not called. Value is the same as the selected previously
			Assert.IsFalse (event_called, "#02");
#endif
			Assert.AreEqual ("3", rbl.SelectedValue, "#03");
		}

		[Test]
		public void LoadAndRaise2 ()
		{
			RadioButtonList rbl = new RadioButtonList ();
			rbl.Items.Add (new ListItem ("Uno", "1"));
			rbl.Items.Add (new ListItem ("Dos", "2"));
			rbl.Items.Add (new ListItem ("Tres", "3"));
			rbl.SelectedIndex = 2;
			NameValueCollection nvc = new NameValueCollection ();
			nvc ["XXX"] = "2";

			IPostBackDataHandler handler = (IPostBackDataHandler) rbl;
			Assert.AreEqual (true, handler.LoadPostData ("XXX", nvc), "#01");
			rbl.SelectedIndexChanged += new EventHandler (OnSelected);
			event_called = false;
			handler.RaisePostDataChangedEvent ();
			Assert.AreEqual (true, event_called, "#02");
			Assert.AreEqual ("2", rbl.SelectedValue, "#03");
		}

		[Test]
		public void LoadAndRaise3 ()
		{
			RadioButtonList rbl = new RadioButtonList ();
			rbl.Items.Add (new ListItem ("Uno", "1"));
			rbl.Items.Add (new ListItem ("Dos", "2"));
			rbl.Items.Add (new ListItem ("Tres", "3"));
			rbl.SelectedIndex = 2;
			NameValueCollection nvc = new NameValueCollection ();
			nvc ["XXX"] = "blah";

			IPostBackDataHandler handler = (IPostBackDataHandler) rbl;
			Assert.AreEqual (false, handler.LoadPostData ("XXX", nvc), "#01");
		}

		
#if NET_2_0
		[Test]
		[ExpectedException(typeof(HttpException))]
		public void VerifyMultiSelectTest()
	        {
	            PokerRadioButtonList list = new PokerRadioButtonList();
	            list.VerifyMultiSelect();
	        }

		[Test]
		public void Defaults ()
		{
			PokerRadioButtonList r = new PokerRadioButtonList ();
			Assert.AreEqual (0, r.RepeatedItemCount, "RepeatedItemCount");
			Assert.AreEqual (false, r.HasFooter, "HasFooter");
			Assert.AreEqual (false, r.HasHeader, "HasHeader");
			Assert.AreEqual (false, r.HasSeparators, "HasSeparators");
		}
		
		[Test]
		[Category ("NunitWeb")]
		public void GetItemStyle ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (GetItemStyle_Load));
			string html = t.Run ();
			string ctrl = HtmlDiff.GetControlFromPageHtml (html);
			if (ctrl == string.Empty)
				Assert.Fail ("RadioButtonList not created fail");
			Assert.AreEqual ("GetItemStyle", (string) t.UserData, "GetItemStyle not done");
			if (ctrl.IndexOf ("<td style=\"background-color:Red;border-style:Solid;\">") == -1)
				Assert.Fail ("RadioButtonList style not rendered");
		}

		public static void GetItemStyle_Load (Page p)
		{
			PokerRadioButtonList rbl = new PokerRadioButtonList ();
			rbl.Items.Add (new ListItem ("Uno", "1"));
			rbl.Items.Add (new ListItem ("Dos", "2"));
			rbl.Items.Add (new ListItem ("Tres", "3"));
			p.Form.Controls.Add (new LiteralControl (HtmlDiff.BEGIN_TAG));
			p.Form.Controls.Add (rbl);
			p.Form.Controls.Add (new LiteralControl (HtmlDiff.END_TAG));
		}

		[Test]
		public void  RaisePostDataChangedEvent ()
		{
			PokerRadioButtonList r = new PokerRadioButtonList ();
			r.SelectedIndexChanged += new EventHandler (r_SelectedIndexChanged);
			Assert.AreEqual (false, eventSelectedIndexChanged, "Before");
			r.DoRaisePostDataChangedEvent ();
			Assert.AreEqual (true, eventSelectedIndexChanged, "After");
		}

		bool eventSelectedIndexChanged;
		void r_SelectedIndexChanged (object sender, EventArgs e)
		{
			eventSelectedIndexChanged = true;
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
			fr.Controls.Add ("RadioButtonList1");

			fr.Controls["__EVENTTARGET"].Value = "RadioButtonList1";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			fr.Controls["RadioButtonList1"].Value = "test";
			t.Request = fr;
			t.Run ();
			if (t.UserData == null)
				Assert.Fail ("RaisePostDataChangedEvent Failed#1");
			Assert.AreEqual ("SelectedIndexChanged", (string) t.UserData, "RaisePostDataChangedEvent Failed#2");
		}

		public static void RaisePostDataChangedEvent_Init (Page p)
		{
			TestRadioButtonList r = new TestRadioButtonList ();
			r.ID = "RadioButtonList1";
			r.Items.Add (new ListItem ("test", "test"));
			r.SelectedIndexChanged += new EventHandler (event_SelectedIndexChanged);
			p.Form.Controls.Add (r);
		}

		public static void event_SelectedIndexChanged (object sender, EventArgs e)
		{
			WebTest.CurrentTest.UserData = "SelectedIndexChanged";	
		}

		#region help_class
		class Poker : RadioButtonList
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
			
			protected override void OnLoad (EventArgs e)
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
			fr.Controls.Add ("RadioButtonList1");

			fr.Controls["__EVENTTARGET"].Value = "RadioButtonList1";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			fr.Controls["RadioButtonList1"].Value = "test";
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
			Poker b = new Poker ();
			b.ID = "RadioButtonList1";
			b.Items.Add (new ListItem ("test", "test"));
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
		public void RepeatedItemCount ()
		{
			PokerRadioButtonList r = new PokerRadioButtonList ();
			Assert.AreEqual (0, r.RepeatedItemCount, "RepeatedItemCount#1");
			r.Items.Add (new ListItem ("Uno", "1"));
			Assert.AreEqual (1, r.RepeatedItemCount, "RepeatedItemCount#2");
			r.Items.Add (new ListItem ("Dos", "2"));
			Assert.AreEqual (2, r.RepeatedItemCount, "RepeatedItemCount#3");
			r.Items.Remove (r.Items[1]);
			Assert.AreEqual (1, r.RepeatedItemCount, "RepeatedItemCount#4");
		}

		[TestFixtureTearDown]
		public void TearDown ()
		{
			WebTest.Unload ();
		}
#endif
	}
}

