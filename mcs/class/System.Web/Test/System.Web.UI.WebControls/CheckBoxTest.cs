//
// CheckBoxTest.cs
//	- Unit tests for System.Web.UI.WebControls.CheckBox
//
// Author:
//	Dick Porter  <dick@ximian.com>
//      Yoni Klain   <Yonik@mainsoft.com>
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
using System.Drawing;
using System.Collections.Specialized;
using NUnit.Framework;
using MonoTests.stand_alone.WebHarness;
using MonoTests.SystemWeb.Framework;
using System.Collections;

namespace MonoTests.System.Web.UI.WebControls {

	public class TestCheckBox : CheckBox {
		public StateBag StateBag {
			get { return base.ViewState; }
		}

		public string Render ()
		{
			HtmlTextWriter writer = new HtmlTextWriter (new StringWriter ());
			base.Render (writer);
			return writer.InnerWriter.ToString ();
		}
		
		public void TrackState () 
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

		public void LoadPostbackData (string value)
		{
			NameValueCollection nvc = new NameValueCollection ();
			
			if (value != null)
				nvc.Add ("mykey", value);
			
			((IPostBackDataHandler) this).LoadPostData ("mykey", nvc);
		}
	}

	public class PokerCheckBox : CheckBox
	{
		private bool checker;
		public bool Checker
		{
			get { return checker; }
		}

		public void Reset ()
		{
			checker = false;
		}
		
		public string Render ()
		{
			HtmlTextWriter writer = new HtmlTextWriter (new StringWriter ());
			base.Render (writer);
			return writer.InnerWriter.ToString ();
		}

		override protected void AddAttributesToRender (HtmlTextWriter writer)
		{
			Reset ();
			base.AddAttributesToRender (writer);
			writer.AddAttribute ("Attribute", "AttributeValue");
			writer.AddStyleAttribute ("StyleAttribute", "StyleAttValue");
			checker = true;
		}
#if NET_2_0
		public new void RaisePostDataChangedEvent ()
		{
			base.RaisePostDataChangedEvent ();
		}

		protected override bool LoadPostData (string postDataKey, NameValueCollection postCollection)
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
#endif
	}

	[TestFixture]
	public class CheckBoxTest {

		[Test]
		public void DefaultProperties ()
		{
			TestCheckBox c = new TestCheckBox ();
			
			Assert.AreEqual (0, c.Attributes.Count, "Attributes.Count");

			Assert.IsFalse (c.AutoPostBack, "AutoPostBack");
			Assert.IsFalse (c.Checked, "Checked");
			Assert.AreEqual (String.Empty, c.Text, "Text");
			Assert.AreEqual (TextAlign.Right, c.TextAlign, "TextAlign");
			
			Assert.AreEqual (0, c.Attributes.Count, "Attributes.Count-2");

#if NET_2_0
			Assert.IsFalse (c.CausesValidation, "CausesValidation");
			Assert.AreEqual (String.Empty, c.ValidationGroup, "ValidationGroup");
#endif
		}
#if NET_2_0
		[Test]
		public void InputAttributesTest ()
		{
			TestCheckBox c = new TestCheckBox ();
			c.InputAttributes.Add ("Atribute", "Test");
		 	string html = c.Render ();
			HtmlDiff.AssertAreEqual ("<input type=\"checkbox\" Atribute=\"Test\" />", html, "Input Attribute fail");
		}

		[Test]
		public void InputAttributesTest2 ()
		{
			TestCheckBox c = new TestCheckBox ();
			c.InputAttributes.Add ("value", "value1");
			c.Text = "Title";
			c.Checked = true;
#if NET_4_0
			string origHtml = "<input id=\"\" type=\"checkbox\" checked=\"checked\" value=\"value1\" /><label for=\"\">Title</label>";
#else
			string origHtml = "<input type=\"checkbox\" checked=\"checked\" value=\"value1\" /><label for>Title</label>";
#endif
			string html = c.Render ();
			HtmlDiff.AssertAreEqual (origHtml, html, "#A1");
		}

		[Test]
		public void LabelAttributesTest_1 ()
		{
			TestCheckBox c = new TestCheckBox ();
			c.ID = "Check";
			c.Text = "CheckBoxText";
			c.LabelAttributes.Add ("Atribute", "Test");
			string html = c.Render ();
			HtmlDiff.AssertAreEqual ("<input id=\"Check\" type=\"checkbox\" name=\"Check\" /><label for=\"Check\" Atribute=\"Test\">CheckBoxText</label>", html, "Label Attributes fail#1");
		}

		[Test]
		public void LabelAttributesTest_2 ()
		{
			TestCheckBox c = new TestCheckBox ();
			c.LabelAttributes.Add ("Atribute", "Test");
			string html = c.Render ();
			HtmlDiff.AssertAreEqual ("<input type=\"checkbox\" />", html, "Label Attributes fail#2");
		}

		[Test]
		public void AddAttributesToRenderTest_1 ()
		{
			PokerCheckBox c = new PokerCheckBox ();
			c.Text = "CheckBoxText";
			string html = c.Render ();
			Assert.AreEqual (true, c.Checker, "AddAttributesToRender dosn't called fail");
			HtmlDiff.AssertAreEqual ("<input Attribute=\"AttributeValue\" type=\"checkbox\" style=\"StyleAttribute:StyleAttValue;\" /><label for>CheckBoxText</label>", html, "Add Attributes To Render fail#1");
		}

		[Test]
		public void AddAttributesToRenderTest_2 ()
		{
			PokerCheckBox c = new PokerCheckBox ();
			string html = c.Render ();
			Assert.AreEqual (true, c.Checker, "AddAttributesToRender dosn't called fail");
			HtmlDiff.AssertAreEqual ("<input Attribute=\"AttributeValue\" type=\"checkbox\" style=\"StyleAttribute:StyleAttValue;\" />", html, "Add Attributes To Render fail#2");
		}
#endif

		[Test]
		public void NullProperties ()
		{
			TestCheckBox c = new TestCheckBox ();
			
			c.Text = null;
			Assert.AreEqual (String.Empty, c.Text, "Text");
			c.TextAlign = TextAlign.Right;
			Assert.AreEqual (TextAlign.Right, c.TextAlign, "TextAlign");
			c.AutoPostBack = true;
			Assert.IsTrue (c.AutoPostBack, "AutoPostBack");
			c.Checked = true;
			Assert.IsTrue (c.Checked, "Checked");
			
			Assert.AreEqual (0, c.Attributes.Count, "Attributes.Count");
			Assert.AreEqual (3, c.StateBag.Count, "ViewState.Count-1");
		}

		[Test]
		public void CleanProperties ()
		{
			TestCheckBox c = new TestCheckBox ();

			c.Text = "text";
			Assert.AreEqual ("text", c.Text, "Text");
			c.AutoPostBack = true;
			c.TextAlign = TextAlign.Left;
			c.Checked = true;

			Assert.AreEqual (4, c.StateBag.Count, "ViewState.Count");
			Assert.AreEqual (0, c.Attributes.Count, "Attributes.Count");

			c.Text = null;
			c.AutoPostBack = false;
			c.TextAlign = TextAlign.Right;
			c.Checked = false;

			// If Text is null it is removed from the ViewState
			Assert.AreEqual (3, c.StateBag.Count, "ViewState.Count-2");
			// This was failing on mono, because the
			// viewstate is holding an int not an enum.
			// (it passes on ms)
			Assert.AreEqual (TextAlign.Right, c.StateBag["TextAlign"], "TextAlign");
			Assert.AreEqual (0, c.Attributes.Count, "Attributes.Count-2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TextAlign_Invalid ()
		{
			CheckBox c = new CheckBox ();
			c.TextAlign = (TextAlign)Int32.MinValue;
		}

		[Test]
		public void TextAlign_Values ()
		{
			CheckBox c = new CheckBox ();

			foreach (TextAlign ta in Enum.GetValues (typeof (TextAlign))) {
				c.TextAlign = ta;
			}
		}
		
		[Test]
		public void Render ()
		{
			TestCheckBox c = new TestCheckBox ();
			c.ID = "ID";
			c.Text = "Text";

			c.TextAlign = TextAlign.Left;
			Assert.AreEqual (@"<label for=""ID"">Text</label><input id=""ID"" type=""checkbox"" name=""ID"" />", c.Render (), "R#1");
			c.TextAlign = TextAlign.Right;
			Assert.AreEqual (@"<input id=""ID"" type=""checkbox"" name=""ID"" /><label for=""ID"">Text</label>", c.Render (), "R#2");

			c.Attributes ["style"] = "color:red;";
			c.TextAlign = TextAlign.Left;
			Assert.AreEqual (@"<span style=""color:red;""><label for=""ID"">Text</label><input id=""ID"" type=""checkbox"" name=""ID"" /></span>",
					c.Render (), "R#3");
			
			c.TextAlign = TextAlign.Right;
			Assert.AreEqual (@"<span style=""color:red;""><input id=""ID"" type=""checkbox"" name=""ID"" /><label for=""ID"">Text</label></span>",
					c.Render (), "R#4");

			c.Attributes ["style"] = null;

			c.ForeColor = Color.Red;
			c.TextAlign = TextAlign.Left;
			Assert.AreEqual (@"<span style=""color:Red;""><label for=""ID"">Text</label><input id=""ID"" type=""checkbox"" name=""ID"" /></span>",
					c.Render (), "R#4");
			
			c.TextAlign = TextAlign.Right;
			Assert.AreEqual (@"<span style=""color:Red;""><input id=""ID"" type=""checkbox"" name=""ID"" /><label for=""ID"">Text</label></span>",
					c.Render (), "R#6");	
		}

		//
		// Code like
		// if (value == null)
		//    ViewState.Remove ("Text");
		// else
		//    ViewState ["Text"] = value
		// is wrong. We need to store when we are tracking viewstate.
		//
		// Statebag takes care of this behavior, so the code is not needed.
		// 
		[Test]
		public void CheckboxViewstateTextNull ()
		{
			TestCheckBox c = new TestCheckBox ();
			c.Text = "text";
			c.TrackState ();
			c.Text = null;
			Assert.AreEqual ("", c.Text);
			object o = c.SaveState ();
			c = new TestCheckBox ();
			c.Text = "text";
			c.TrackState ();
			c.LoadState (o);
			Assert.AreEqual ("", c.Text);
		}

#if NET_2_0
		[Test]
		public void CheckboxViewstateValidation ()
		{
			// for some reason, MS doesn't save the validation
			// properties to the viewstate for the Checkbox
			// control.  why not?
			TestCheckBox o = new TestCheckBox ();
			o.ValidationGroup = "VG1";
			o.CausesValidation = true;
			o.TrackState ();
			Assert.AreEqual ("VG1", o.ValidationGroup, "V1");
			Assert.IsTrue (o.CausesValidation, "V2");
			object state = o.SaveState ();
			TestCheckBox copy = new TestCheckBox ();
			copy.LoadState (state);
			Assert.AreEqual ("", copy.ValidationGroup, "V3");
			Assert.IsFalse (copy.CausesValidation, "V4");
		}

		[Test]
		public void RaisePostDataChangedEvent ()
		{
			PokerCheckBox o = new PokerCheckBox ();
			o.CheckedChanged += new EventHandler (o_CheckedChanged);
			Assert.AreEqual (false, eventCheckedChanged, "RaisePostDataChangedEven#1");
			o.RaisePostDataChangedEvent ();
			Assert.AreEqual (true, eventCheckedChanged, "RaisePostDataChangedEven#2");
		}

		bool eventCheckedChanged; 
		void o_CheckedChanged (object sender, EventArgs e)
		{
			eventCheckedChanged = true;
		}

		[Test]
		[Category ("NunitWeb")]
		public void LoadPostData ()  //Just flow and not implementation detail
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (LoadPostData_Load));
			string html = t.Run ();
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls["__EVENTTARGET"].Value = "pb";
			fr.Controls["__EVENTARGUMENT"].Value = "";
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
			PokerCheckBox b = new PokerCheckBox ();
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

		[TestFixtureTearDown]
		public void TearDown ()
		{
			WebTest.Unload ();
		}
#endif
	}
}
