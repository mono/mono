//
// Tests for System.Web.UI.WebControls.ListBoxTest.cs
//
// Author:
//	Jackson Harper (jackson@ximian.com)
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
using System.Collections;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
#if NET_2_0
using MonoTests.stand_alone.WebHarness;
using MonoTests.SystemWeb.Framework;
#endif

namespace MonoTests.System.Web.UI.WebControls 
{

	public class ListControlPoker : ListControl {

		public ListControlPoker ()
		{
		}

		public void TrackState ()
		{
			TrackViewState ();
		}

		public object SaveState ()
		{
			return SaveViewState ();
		}

		public void LoadState (object state)
		{
			LoadViewState (state);
		}

		public object ViewStateValue (string name)
		{
			return ViewState [name];
		}

		public string Render ()
		{
			StringWriter sw = new StringWriter ();
			sw.NewLine = "\n";
			HtmlTextWriter writer = new HtmlTextWriter (sw);
			base.Render (writer);
			return writer.InnerWriter.ToString ();
		}

		public bool GetIsTrackingViewState ()
		{
			return IsTrackingViewState;
		}
#if NET_2_0
		public HtmlTextWriterTag GetTagKey ()
		{
			return TagKey;
		}

		public new void OnSelectedIndexChanged (EventArgs e)
		{
			base.OnSelectedIndexChanged (e);
		}

		public new void OnTextChanged (EventArgs e)
		{
			base.OnTextChanged (e);
		}

		public new void VerifyMultiSelect ()
		{
			base.VerifyMultiSelect ();
		}

#endif
	}

	[TestFixture]
	public class ListControlTest
	{
		#region help_class_for_addattributetorender
		class Poker : ListControl
		{
			public void TrackState ()
			{
				TrackViewState ();
			}

			public object SaveState ()
			{
				return SaveViewState ();
			}

			public void LoadState (object state)
			{
				LoadViewState (state);
			}

			public object ViewStateValue (string name)
			{
				return ViewState[name];
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
			ArrayList Databound ()
			{
				ArrayList list = new ArrayList ();
				list.Add (1);
				list.Add (2);
				list.Add (3);
				return list;
			}

			public HtmlTextWriterTag GetTagKey ()
			{
				return TagKey;
			}

			protected override void AddAttributesToRender (HtmlTextWriter writer)
			{
				writer.AddAttribute (HtmlTextWriterAttribute.Type, "MyType");
				writer.AddAttribute (HtmlTextWriterAttribute.Name, "MyName");
				writer.AddAttribute (HtmlTextWriterAttribute.Value, "MyValue");
				base.AddAttributesToRender (writer);
			}

			protected override void PerformDataBinding (IEnumerable dataSource)
			{
				base.PerformDataBinding (Databound());
			}

			protected override void PerformSelect ()
			{
				base.PerformSelect ();
				SelectedIndex = 0;
			}

			public new void SetPostDataSelection(int selectedItem)
			{
				base.SetPostDataSelection(selectedItem);
			}
#endif
		}
		#endregion
		private Hashtable changed = new Hashtable ();

#if NET_2_0
		[TestFixtureSetUp]
		public void SetUp ()
		{
			WebTest.CopyResource (GetType (), "ListControlPage.aspx", "ListControlPage.aspx");
		}
#endif

		[Test]
		public void DefaultProperties ()
		{
			ListControlPoker p = new ListControlPoker ();

			Assert.AreEqual (p.AutoPostBack, false, "A1");
			Assert.AreEqual (p.DataMember, String.Empty, "A2");
			Assert.AreEqual (p.DataSource, null, "A3");
			Assert.AreEqual (p.DataTextField, String.Empty, "A4");
			Assert.AreEqual (p.DataTextFormatString, String.Empty, "A5");
			Assert.AreEqual (p.DataValueField, String.Empty, "A6");
			Assert.AreEqual (p.Items.Count, 0, "A7");
			Assert.AreEqual (p.SelectedIndex, -1,"A8");
			Assert.AreEqual (p.SelectedItem, null, "A9");
			Assert.AreEqual (p.SelectedValue, String.Empty, "A10");
#if NET_2_0
			Assert.IsFalse (p.AppendDataBoundItems, "A11");
			Assert.AreEqual (p.Text, "", "A12");
			Assert.AreEqual (p.GetTagKey(), HtmlTextWriterTag.Select, "A13");
			Assert.AreEqual (false, p.AppendDataBoundItems, "A14");
			Assert.AreEqual (false, p.CausesValidation, "A15");
#endif
		}
#if NET_2_0

		[Test]
		public void AddAttributesToRender ()
		{
			Poker p = new Poker ();
			ListItem foo = new ListItem ("foo");
			ListItem bar = new ListItem ("bar");
			p.Items.Add (foo);
			p.Items.Add (bar);
#region orig
			string orig = "<select type=\"MyType\" name=\"MyName\" value=\"MyValue\">\n\t<option value=\"foo\">foo</option>\n\t<option value=\"bar\">bar</option>\n\n</select>";
#endregion
			string html =  p.Render ();
			HtmlDiff.AssertAreEqual(orig,html,"AddAttributesToRender failed");
		}

		[Test]
		public void AppendDataBoundItems ()
		{
			ListControlPoker p = new ListControlPoker ();
			ListItem foo = new ListItem ("foo");
			ListItem bar = new ListItem ("bar");
			p.Items.Add (foo);
			p.Items.Add (bar);
			Assert.AreEqual (2, p.Items.Count, "Before databound");
			p.AppendDataBoundItems = true; // Not to clear list before databind
			p.DataSource = Databound ();
			p.DataBind ();
			Assert.AreEqual (5, p.Items.Count, "After databound");
			p.AppendDataBoundItems = false; // To clear list before databind
			p.DataBind ();
			Assert.AreEqual (3, p.Items.Count, "Clear list and databound");
		}

		ArrayList Databound ()
		{
			ArrayList list = new ArrayList ();
			list.Add (1);
			list.Add (2);
			list.Add (3);
			return list;
		}

		[Test]
		[Category ("NunitWeb")]
		public void CausesValidation_ValidationGroup ()
		{
			WebTest t = new WebTest ("ListControlPage.aspx");
			string str = t.Run ();
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls.Add ("ListBox1");
			fr.Controls["__EVENTTARGET"].Value = "ListBox1";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			fr.Controls["ListBox1"].Value = "2";
			t.Request = fr;
			string html = t.Run ();

			if (html.IndexOf ("Validate_validation_group") == -1)
				Assert.Fail ("Validate not created");
			if (html.IndexOf ("MyValidationGroup") == -1)
				Assert.Fail ("Wrong validation group");
		}

		[Test]
		public void OnTextChanged ()
		{
			ListControlPoker p = new ListControlPoker ();
			p.TextChanged += new EventHandler (p_TextChanged);
			Assert.AreEqual (false, eventTextChanged, "Before");
			p.OnTextChanged (new EventArgs ());
			Assert.AreEqual (true, eventTextChanged, "After");
		}

		bool eventTextChanged;
		void p_TextChanged (object sender, EventArgs e)
		{
			eventTextChanged = true;
		}

		[Test]
		public void PerformDataBind_PerformSelect ()
		{
			Poker p = new Poker ();
			p.DataBind ();
			string html = p.Render ();
#region #1
			string orig = "<select type=\"MyType\" name=\"MyName\" value=\"MyValue\">\n\t<option selected=\"selected\" value=\"1\">1</option>\n\t<option value=\"2\">2</option>\n\t<option value=\"3\">3</option>\n\n</select>";
#endregion
			HtmlDiff.AssertAreEqual (orig, html, "PerformDataBind");
		}

		[Test]
		[Category("NotWorking")] //Not implemented
		public void SetPostDataSelection ()
		{
			Poker p = new Poker ();
			ListItem foo = new ListItem ("foo");
			ListItem bar = new ListItem ("bar");
			p.Items.Add (foo);
			p.Items.Add (bar);
			p.SetPostDataSelection (1);
			Assert.AreEqual (1, p.SelectedIndex, "SetPostDataSelection");
		}

		[Test]
		public void Text ()
		{
			Poker p = new Poker ();
			ListItem foo = new ListItem ("foo");
			ListItem bar = new ListItem ("bar");
			p.Items.Add (foo);
			p.Items.Add (bar);
			Assert.AreEqual (string.Empty, p.Text, "Text#1");
			p.SelectedIndex = 1;
			Assert.AreEqual ("bar", p.Text, "Text#2");
		}

		#region HelpListForMultiple
		class PokerL : ListBox
		{
			public PokerL ()
			{
				InitializeMyListBox ();
			}
			private void InitializeMyListBox ()
			{
				// Add items to the ListBox.
				Items.Add ("A");
				Items.Add ("C");
				Items.Add ("E");
				Items.Add ("F");
				Items.Add ("G");
				Items.Add ("D");
				Items.Add ("B");

				// Set the SelectionMode to select multiple items.
				SelectionMode = ListSelectionMode.Multiple;

				Items[0].Selected = true;
				Items[2].Selected = true;
			}

			public void TrackState ()
			{
				TrackViewState ();
			}

			public object SaveState ()
			{
				return SaveViewState ();
			}

			public void LoadState (object state)
			{
				LoadViewState (state);
			}

			public object ViewStateValue (string name)
			{
				return ViewState[name];
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
		#endregion
		[Test]
		public void Multiple ()
		{
			PokerL p = new PokerL ();
			Assert.AreEqual (true, p.Items[0].Selected, "MultipleSelect#1");
			Assert.AreEqual (true, p.Items[2].Selected, "MultipleSelect#2");
			string html = p.Render ();
			#region origin
			string orig = "<select size=\"4\" multiple=\"multiple\">\n\t<option selected=\"selected\" value=\"A\">A</option>\n\t<option value=\"C\">C</option>\n\t<option selected=\"selected\" value=\"E\">E</option>\n\t<option value=\"F\">F</option>\n\t<option value=\"G\">G</option>\n\t<option value=\"D\">D</option>\n\t<option value=\"B\">B</option>\n\n</select>";
			#endregion
			HtmlDiff.AssertAreEqual (orig, html, "MultipleSelect#3");
		}
#endif

		[Test]
		public void CleanProperties ()
		{
			ListControlPoker p = new ListControlPoker ();

			p.AutoPostBack = true;
			Assert.AreEqual (p.AutoPostBack, true, "A2");

			p.DataMember = "DataMember";
			Assert.AreEqual (p.DataMember, "DataMember", "A3");

			p.DataSource = "DataSource";
			Assert.AreEqual (p.DataSource, "DataSource", "A4");

			p.DataTextField = "DataTextField";
			Assert.AreEqual (p.DataTextField, "DataTextField", "A5");

			p.DataTextFormatString = "DataTextFormatString";
			Assert.AreEqual (p.DataTextFormatString, "DataTextFormatString", "A6");

			p.DataValueField = "DataValueField";
			Assert.AreEqual (p.DataValueField, "DataValueField", "A7");

			p.SelectedIndex = 10;
			Assert.AreEqual (p.SelectedIndex, -1, "A8");

			p.SelectedValue = "SelectedValue";
			Assert.AreEqual (p.SelectedValue, String.Empty, "A9");
		}

		[Test]
		public void NullProperties ()
		{
			ListControlPoker p = new ListControlPoker ();

			p.DataMember = null;
			Assert.AreEqual (p.DataMember, String.Empty, "A1");

			p.DataSource = null;
			Assert.AreEqual (p.DataSource, null, "A2");

			p.DataTextField = null;
			Assert.AreEqual (p.DataTextField, String.Empty, "A3");

			p.DataTextFormatString = null;
			Assert.AreEqual (p.DataTextFormatString, String.Empty, "A4");

			p.DataValueField = null;
			Assert.AreEqual (p.DataValueField, String.Empty, "A5");

			p.SelectedValue = null;
			Assert.AreEqual (p.SelectedValue, String.Empty, "A6");
		}

		[Test]
		public void ClearSelection ()
		{
			ListControlPoker p = new ListControlPoker ();

			ListItem foo = new ListItem ("foo");
			ListItem bar = new ListItem ("bar");

			BeginIndexChanged (p);

			p.Items.Add (foo);
			p.Items.Add (bar);
			p.SelectedIndex = 1;

			// sanity for the real test
			Assert.AreEqual (p.Items.Count, 2, "A1");
			Assert.AreEqual (p.SelectedIndex, 1, "A2");
			Assert.AreEqual (p.SelectedItem, bar, "A3");
			Assert.AreEqual (p.SelectedValue, bar.Value, "A4");
			
			p.ClearSelection ();

			Assert.AreEqual (p.SelectedIndex, -1, "A5");
			Assert.AreEqual (p.SelectedItem, null, "A6");
			Assert.AreEqual (p.SelectedValue, String.Empty, "A7");
			Assert.IsFalse (EndIndexChanged (p), "A8");

			// make sure we are still sane
			Assert.AreEqual (p.Items.Count, 2, "A9");
		}

#if NET_2_0
		[Test]
		// Tests Save/Load ControlState
		public void ControlState ()
		{
			ListControlPoker a = new ListControlPoker ();
			ListControlPoker b = new ListControlPoker ();

			a.TrackState ();

			a.Items.Add ("a");
			a.Items.Add ("b");
			a.Items.Add ("c");
			a.SelectedIndex = 2;

			b.Items.Add ("a");
			b.Items.Add ("b");
			b.Items.Add ("c");

			Assert.AreEqual (-1, b.SelectedIndex, "A1");
		}
#endif

		[Test]
		// Tests Save/Load/Track ViewState
		public void ViewState ()
		{
			ListControlPoker a = new ListControlPoker ();
			ListControlPoker b = new ListControlPoker ();

			a.TrackState ();

			BeginIndexChanged (a);
			BeginIndexChanged (b);

			a.Items.Add ("a");
			a.Items.Add ("b");
			a.Items.Add ("c");
			a.SelectedIndex = 2;

			object state = a.SaveState ();
			b.LoadState (state);

			Assert.AreEqual (2, b.SelectedIndex, "A1");
			Assert.AreEqual (b.Items.Count, 3, "A2");

			Assert.AreEqual (b.Items [0].Value, "a", "A3");
			Assert.AreEqual (b.Items [1].Value, "b", "A4");
			Assert.AreEqual (b.Items [2].Value, "c", "A5");

			Assert.IsFalse (EndIndexChanged (a), "A6");
			Assert.IsFalse (EndIndexChanged (b), "A7");
		}

		[Test]
		// Tests Save/Load/Track ViewState
		public void ViewStateIsNeeded ()
		{
			ListControlPoker a = new ListControlPoker ();
			IStateManager sm = a.Items as IStateManager;
			
			Assert.IsFalse (a.GetIsTrackingViewState (), "#A1-1");
			Assert.IsFalse (sm.IsTrackingViewState, "#A1-2");
			object state = a.SaveState ();
			Assert.IsNotNull (state, "#A1-3");

			a.Items.Add ("a");
			a.Items.Add ("b");
			a.Items.Add ("c");

			Assert.IsFalse (a.GetIsTrackingViewState (), "#A2-1");
			Assert.IsFalse (sm.IsTrackingViewState, "#A2-2");

			state = a.SaveState ();
			Assert.IsNotNull (state, "#A3");
		}

		[Test]
		public void ViewStateContents ()
		{
			ListControlPoker p = new ListControlPoker ();

			p.TrackState ();

			// So the selected index can be set
			p.Items.Add ("one");
			p.Items.Add ("two");

			p.AutoPostBack = false;
			p.DataMember = "DataMember";
			p.DataSource = "DataSource";
			p.DataTextField = "DataTextField";
			p.DataTextFormatString = "DataTextFormatString";
			p.DataValueField = "DataValueField";
			p.SelectedIndex = 1;
#if NET_2_0
			p.AppendDataBoundItems = true;
			p.Text = "Text";
#endif

			Assert.AreEqual (p.ViewStateValue ("AutoPostBack"), false, "A1");
			Assert.AreEqual (p.ViewStateValue ("DataMember"), "DataMember", "A2");

			Assert.AreEqual (p.ViewStateValue ("DataSource"), null, "A3");
			Assert.AreEqual (p.ViewStateValue ("DataTextField"), "DataTextField", "A4");
			Assert.AreEqual (p.ViewStateValue ("DataTextFormatString"),
					"DataTextFormatString", "A5");
			Assert.AreEqual (p.ViewStateValue ("DataValueField"), "DataValueField", "A6");

#if NET_2_0
			Assert.AreEqual (p.ViewStateValue ("AppendDataBoundItems"), true, "A7");
#endif

			// None of these are saved
			Assert.AreEqual (p.ViewStateValue ("SelectedIndex"), null, "A8");
			Assert.AreEqual (p.ViewStateValue ("SelectedItem"), null, "A9");
			Assert.AreEqual (p.ViewStateValue ("SelectedValue"), null, "A10");
#if NET_2_0
			Assert.AreEqual (p.ViewStateValue ("Text"), null, "A11");
#endif

		}

		[Test]
		public void SelectedIndex ()
		{
			ListControlPoker p = new ListControlPoker ();

			p.Items.Add ("one");
			p.Items.Add ("two");
			p.Items.Add ("three");
			p.Items.Add ("four");

			p.Items [2].Selected = true;
			p.Items [1].Selected = true;

			Assert.AreEqual (p.SelectedIndex, 1, "A1");

			p.ClearSelection ();
			p.Items [3].Selected = true;

			Assert.AreEqual (p.SelectedIndex, 3, "A2");

			p.SelectedIndex = 1;
			Assert.AreEqual (p.SelectedIndex, 1, "A3");
			Assert.IsFalse (p.Items [3].Selected, "A4");
		}

		[Test]
		public void Render ()
		{
			ListControlPoker p = new ListControlPoker ();

			string s = p.Render ();
			string expected = "<select>\n\n</select>";
			Assert.AreEqual (s, expected, "A1");
		}

		[Test]
#if !NET_2_0
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
#endif
		public void ItemsTooHigh ()
		{
			ListControlPoker l = new ListControlPoker ();

			l.Items.Add ("foo");
			l.SelectedIndex = 1;
#if NET_2_0
			Assert.AreEqual (-1, l.SelectedIndex, "#01");
#endif
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ItemsTooLow ()
		{
			ListControlPoker l = new ListControlPoker ();

			l.Items.Add ("foo");
			l.SelectedIndex = -2;
		}

		[Test]
		public void ItemsOk ()
		{
			ListControlPoker l = new ListControlPoker ();

			l.Items.Add ("foo");
			l.SelectedIndex = 0;
			l.SelectedIndex = -1;
		}

		[Test]
		public void DataBinding1 ()
		{
			ListControlPoker p = new ListControlPoker ();
			ArrayList list = new ArrayList ();
			list.Add (1);
			list.Add (2);
			list.Add (3);
			p.DataSource = list;
			p.SelectedIndex = 2;
			Assert.AreEqual (-1, p.SelectedIndex, "#01");
			p.DataBind ();
			Assert.AreEqual (2, p.SelectedIndex, "#02");
			Assert.AreEqual (3.ToString (), p.SelectedValue, "#03");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // The SelectedIndex and SelectedValue are mutually exclusive 
		public void DataBinding2 ()
		{
			ListControlPoker p = new ListControlPoker ();
			ArrayList list = new ArrayList ();
			list.Add (1);
			list.Add (2);
			list.Add (3);
			p.DataSource = list;
			p.SelectedIndex = 0;
			p.SelectedValue = "3";
			p.DataBind ();
		}

		[Test]
		public void DataBinding3 ()
		{
			ListControlPoker p = new ListControlPoker ();
			ArrayList list = new ArrayList ();
			list.Add (1);
			list.Add (2);
			list.Add (3);
			p.DataSource = list;
			// If Index and Value are used, everything's ok if they are selecting
			// the same thing.
			p.SelectedIndex = 2;
			p.SelectedValue = "3";
			Assert.AreEqual ("", p.SelectedValue, "#01");
			p.DataBind ();
			Assert.AreEqual ("3", p.SelectedValue, "#02");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void DataBinding4 ()
		{
			ListControlPoker p = new ListControlPoker ();
			ArrayList list = new ArrayList ();
			list.Add (1);
			list.Add (null);
			list.Add (3);
			p.DataSource = list;
			p.DataBind ();
		}

		[Test]
		public void DataBinding6 () {
			ListControlPoker p = new ListControlPoker ();
			ArrayList list = new ArrayList ();
			list.Add (1);
			list.Add (2);
			list.Add (3);
			p.DataSource = list;
			p.DataBind ();
			Assert.AreEqual (3, p.Items.Count, "#01");
			p.DataSource = null;
			p.DataBind ();
			Assert.AreEqual (3, p.Items.Count, "#01");
		}

#if NET_2_0
		[Test]
		public void DataBinding7 () {
			ListControlPoker p = new ListControlPoker ();
			ArrayList list = new ArrayList ();
			list.Add (1);
			list.Add (2);
			list.Add (3);
			p.DataSource = list;
			p.DataBind ();

			p.SelectedValue = "3";
			Assert.AreEqual (2, p.SelectedIndex, "#01");
			
			p.DataBind ();
			Assert.AreEqual ("3", p.SelectedValue, "#02");
			Assert.AreEqual (2, p.SelectedIndex, "#03");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void DataBinding8 () {
			ListControlPoker p = new ListControlPoker ();
			ArrayList list = new ArrayList ();
			list.Add (1);
			list.Add (2);
			list.Add (3);
			p.DataSource = list;
			p.DataBind ();

			p.SelectedValue = "3";
			Assert.AreEqual (2, p.SelectedIndex, "#01");

			list = new ArrayList ();
			list.Add (4);
			list.Add (5);
			list.Add (6);
			p.DataSource = list;
			p.DataBind ();
			Assert.AreEqual ("3", p.SelectedValue, "#01");
			Assert.AreEqual (2, p.SelectedIndex, "#01");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void DataBinding9 () {
			ListControlPoker p = new ListControlPoker ();
			ArrayList list = new ArrayList ();
			list.Add (1);
			list.Add (2);
			list.Add (3);
			p.DataSource = list;
			p.SelectedValue = "5";
			p.DataBind ();
		}

		[Test]
		public void DataBinding1a () {
			ListControlPoker p = new ListControlPoker ();
			ArrayList list = new ArrayList ();
			list.Add (1);
			list.Add (2);
			list.Add (3);
			p.DataSource = list;
			p.SelectedIndex = 4;
			Assert.AreEqual (-1, p.SelectedIndex, "#01");
			p.DataBind ();
			Assert.AreEqual (-1, p.SelectedIndex, "#02");
			Assert.AreEqual ("", p.SelectedValue, "#03");
		}
		
		[Test]
		public void DataBinding10 () {
			ListControlPoker p = new ListControlPoker ();
			ArrayList list = new ArrayList ();
			list.Add (1);
			list.Add (2);
			list.Add (3);
			p.DataSource = list;
			p.DataBind ();

			p.SelectedValue = "5";
			Assert.AreEqual ("", p.SelectedValue, "#01");
			
			p.SelectedIndex = 4;
			Assert.AreEqual (-1, p.SelectedIndex, "#02");
		}

		[Test]
		public void DataBinding11 () {
			ListControlPoker p = new ListControlPoker ();
			ArrayList list = new ArrayList ();
			list.Add (1);
			list.Add (2);
			list.Add (3);
			p.DataSource = list;
			p.SelectedValue = "3";
			p.DataBind ();

			p.SelectedValue = "5";
			Assert.AreEqual ("3", p.SelectedValue, "#01");

			p.SelectedIndex = 4;
			Assert.AreEqual (2, p.SelectedIndex, "#02");

			p.Items.Clear ();
			Assert.AreEqual ("", p.SelectedValue, "#03");
			Assert.AreEqual (-1, p.SelectedIndex, "#04");

			p.Items.Add (new ListItem ("1"));
			p.Items.Add (new ListItem ("2"));
			p.Items.Add (new ListItem ("3"));
			p.Items.Add (new ListItem ("4"));
			p.Items.Add (new ListItem ("5"));
			Assert.AreEqual ("", p.SelectedValue, "#05");
			Assert.AreEqual (-1, p.SelectedIndex, "#06");
			
			list = new ArrayList ();
			list.Add (1);
			list.Add (2);
			list.Add (3);
			list.Add (4);
			list.Add (5);
			p.DataSource = list;
			p.DataBind ();
			Assert.AreEqual ("5", p.SelectedValue, "#07");
			Assert.AreEqual (4, p.SelectedIndex, "#08");
		}
#endif

		class Data 
		{
			string name;
			object val;

			public Data (string name, object val)
			{
				this.name = name;
				this.val = val;
			}

			public string Name {
				get { return name; }
			}

			public object Value {
				get { return val; }
			}
		}

		[Test]
		public void DataBinding5 ()
		{
			ListControlPoker p = new ListControlPoker ();
			p.DataTextField = "Name";
			p.DataValueField = "Value";
			ArrayList list = new ArrayList ();
			list.Add (new Data ("uno", 1));
			list.Add (new Data ("dos", 2));
			list.Add (new Data ("tres", 3));
			p.DataSource = list;
			p.SelectedIndex = 2;
			p.DataBind ();
			Assert.AreEqual (3.ToString (), p.SelectedValue, "#01");
			Assert.AreEqual ("tres", p.SelectedItem.Text, "#01");
		}

		[Test]
		public void DataBindingFormat1 ()
		{
			ListControlPoker p = new ListControlPoker ();
			ArrayList list = new ArrayList ();
			list.Add (1);
			list.Add (2);
			list.Add (3);
			p.DataSource = list;
			p.DataTextFormatString = "{0:00}";
			p.SelectedIndex = 2;
			p.DataBind ();
			Assert.AreEqual ("3", p.SelectedValue, "#01");
		}

		[Test]
		public void DataBindingFormat2 ()
		{
			ListControlPoker p = new ListControlPoker ();
			ArrayList list = new ArrayList ();
			list.Add (1);
			list.Add (2);
			list.Add (3);
			p.DataSource = list;
			p.DataTextFormatString = "{0:00}";
			p.SelectedIndex = 2;
			p.DataBind ();
			Assert.IsNotNull (p.SelectedItem, "#00");
			Assert.AreEqual ("03", p.SelectedItem.Text, "#01");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void DataBindingFormat3 ()
		{
			ListControlPoker p = new ListControlPoker ();
			ArrayList list = new ArrayList ();
			list.Add (1);
			list.Add (null);
			list.Add (3);
			p.DataSource = list;
			p.DataTextFormatString = "{0:00}";
			p.SelectedIndex = 2;
			p.DataBind ();
		}

		private void BeginIndexChanged (ListControl l)
		{
			l.SelectedIndexChanged += new EventHandler (IndexChangedHandler);
		}

		private bool EndIndexChanged (ListControl l)
		{
			bool res = changed [l] != null;
			changed [l] = null;
			return res;
		}

		private void IndexChangedHandler (object sender, EventArgs e)
		{
			changed [sender] = new object ();
		}

		[Test]
		public void ValueFound1 ()
		{
			ListControlPoker p = new ListControlPoker ();
			p.Items.Add ("one");
			p.SelectedValue = "one";
		}

		[Test]
#if !NET_2_0
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
#endif
		public void ValueNotFound1 ()
		{
			ListControlPoker p = new ListControlPoker ();
			p.Items.Add ("one");
			p.SelectedValue = "dos";
	    Assert.AreEqual("", p.SelectedValue, "SelectedValue");
	    Assert.AreEqual(null, p.SelectedItem, "SelectedItem");
	    Assert.AreEqual(-1, p.SelectedIndex, "SelectedIndex");
		}

		[Test]
		public void ValueNotFound2 ()
		{
			ListControlPoker p = new ListControlPoker ();
			p.SelectedValue = "dos";
	    Assert.AreEqual("", p.SelectedValue, "SelectedValue");
	    Assert.AreEqual(null, p.SelectedItem, "SelectedItem");
	    Assert.AreEqual(-1, p.SelectedIndex, "SelectedIndex");
		}
#if NET_2_0
		[Test]
		[ExpectedException (typeof (HttpException))]
		public void VerifyMultiSelect_Exception ()
		{
			ListControlPoker p = new ListControlPoker ();
			p.VerifyMultiSelect ();
		}

		[Test]
		public void DataBinding_SelectedValue () {
			ListControlPoker p = new ListControlPoker ();
			p.SelectedValue = "b";

			p.Items.Add (new ListItem ("a", "a"));
			p.Items.Add (new ListItem ("b", "b"));
			p.Items.Add (new ListItem ("c", "c"));

			Assert.IsFalse (p.Items [1].Selected, "SelectedIndex");
			p.DataBind ();
			Assert.IsTrue (p.Items [1].Selected, "SelectedIndex");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void DataBinding_SelectedValue_Exception () {
			ListControlPoker p = new ListControlPoker ();
			p.SelectedValue = "AAA";

			p.Items.Add (new ListItem ("a", "a"));
			p.Items.Add (new ListItem ("b", "b"));
			p.Items.Add (new ListItem ("c", "c"));

			Assert.IsFalse (p.Items [1].Selected, "SelectedIndex");
			p.DataBind ();
			Assert.IsTrue (p.Items [1].Selected, "SelectedIndex");
		}

		[TestFixtureTearDown]
		public void TearDown ()
		{
			WebTest.Unload ();
		}
#endif

	}
}





