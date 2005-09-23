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


namespace MonoTests.System.Web.UI.WebControls {

	[TestFixture]
	public class RadioButtonListTest {

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

		[Test]
#if NET_2_0
		// FIXME: missing some info to start rendering ?
		[ExpectedException (typeof (NullReferenceException))]
#endif
		public void Render ()
		{
			TestRadioButtonList c = new TestRadioButtonList ();

			c.Items.Add (new ListItem ("text2", "value1"));

			string s = c.Render ();

			Assert.IsTrue (s.ToLower ().IndexOf (" type=\"radio\"") !=  -1, "type");
			Assert.IsTrue (s.ToLower ().IndexOf ("value1") !=  -1, "value");
			Assert.IsTrue (s.ToLower ().IndexOf ("text2") !=  -1, "text");
		}

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
	}
}

