//
// Tests for System.Web.UI.WebControls.ListBoxTest.cs
//
// Author:
//	Jackson Harper (jackson@ximian.com)
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
using System.Drawing;
using System.Collections.Specialized;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using MonoTests.stand_alone.WebHarness;
using MonoTests.SystemWeb.Framework;

namespace MonoTests.System.Web.UI.WebControls
{
	class ListBoxPoker : ListBox {

		public ListBoxPoker ()
		{
			TrackViewState ();
		}

		public bool LoadPD (string key, NameValueCollection values)
		{
			return ((IPostBackDataHandler) this).LoadPostData (key, values);
		}

		public object SaveState ()
		{
			return SaveViewState ();
		}

		public void LoadState (object o)
		{
			LoadViewState (o);
		}

		public StateBag _ViewState {
			get { return ViewState; }
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
	public new virtual void VerifyMultiSelect()
	{
	    base.VerifyMultiSelect();
	}
#endif
	}

	[TestFixture]	
	public class ListBoxTest {

		[Test]
		public void Defaults ()
		{
			ListBox lb = new ListBox ();

			Assert.AreEqual (lb.BorderColor, Color.Empty, "A1");
			Assert.AreEqual (lb.BorderStyle, BorderStyle.NotSet, "A2");
			Assert.AreEqual (lb.BorderWidth, Unit.Empty, "A3");
			Assert.AreEqual (lb.Rows, 4, "A4");
			Assert.AreEqual (lb.SelectionMode, ListSelectionMode.Single, "A5");
			Assert.AreEqual (lb.ToolTip, String.Empty, "A6");
		}

		[Test]
		public void SetProps ()
		{
			ListBox lb = new ListBox ();

			lb.BorderColor = Color.Black;
			Assert.AreEqual (lb.BorderColor, Color.Black, "A1");

			lb.BorderStyle = BorderStyle.Dashed;
			Assert.AreEqual (lb.BorderStyle, BorderStyle.Dashed, "A2");

			lb.BorderWidth = 0;
			Assert.AreEqual (lb.BorderWidth, (Unit) 0, "A3");

			lb.BorderWidth = 15;
			Assert.AreEqual (lb.BorderWidth, (Unit) 15, "A3");

			lb.Rows = 1;
			Assert.AreEqual (lb.Rows, 1, "A4");

			lb.SelectionMode = ListSelectionMode.Multiple;
			Assert.AreEqual (lb.SelectionMode, ListSelectionMode.Multiple, "A6");

			lb.ToolTip = "foo";
#if NET_2_0
			Assert.AreEqual (lb.ToolTip, "foo", "A7");
#else
			Assert.AreEqual (lb.ToolTip, String.Empty, "A7"); // Always empty in 1.x
#endif
		}

		[Test]
	[Category ("NotWorking")]
#if !NET_2_0
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
#endif
		public void RowsTooHigh ()
		{
			ListBox lb = new ListBox ();
			lb.Rows = 2001;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void RowsTooLow ()
		{
			ListBox lb = new ListBox ();
			lb.Rows = 0;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void BadSelectionMode ()
		{
			ListBox lb = new ListBox ();
			lb.SelectionMode = (ListSelectionMode) 500;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void BadBorderStyle ()
		{
			ListBox lb = new ListBox ();
			lb.BorderStyle = (BorderStyle) 500;
		}

		[Test]
		public void ViewState ()
		{
			ListBoxPoker p = new ListBoxPoker ();

			p.BorderColor = Color.Red;
			Assert.AreEqual (p._ViewState ["BorderColor"],
					Color.Red, "A1");

			p.BorderStyle = BorderStyle.Double;
			Assert.AreEqual (p._ViewState ["BorderStyle"],
					BorderStyle.Double, "A2");

			p.BorderWidth = 25;
			Assert.AreEqual (p._ViewState ["BorderWidth"],
					(Unit) 25, "A3");

			p.SelectionMode = ListSelectionMode.Multiple;
			Assert.AreEqual (p._ViewState ["SelectionMode"],
					ListSelectionMode.Multiple, "A4");
		}

		[Test]
		public void Render1 ()
		{
			ListBoxPoker l = new ListBoxPoker ();
			for (int i = 0; i < 3; i ++)
				l.Items.Add (i.ToString ());

			l.SelectedIndex = l.Items.Count - 1;
#if NET_2_0
			string exp = @"<select size=""4"">
	<option value=""0"">0</option>
	<option value=""1"">1</option>
	<option selected=""selected"" value=""2"">2</option>

</select>";
#else
			string exp = @"<select name size=""4"">
	<option value=""0"">0</option>
	<option value=""1"">1</option>
	<option selected=""selected"" value=""2"">2</option>

</select>";
#endif
			HtmlDiff.AssertAreEqual (exp, l.Render (), "Render1");
		}

		DataSet GetExampleData ()
		{
			DataSet ds = new DataSet ();
			ds.ReadXml (new StringReader (@"
<DataSet>
	<Stocks Company='Novell Inc.'     Symbol='NOVL' Price='6.14'   />
	<Stocks Company='Microsoft Corp.' Symbol='MSFT' Price='25.92'  />
	<Stocks Company='Google'          Symbol='GOOG' Price='291.60' />
</DataSet>
"));
			return ds;
		}
		
		[Test]
		public void DoubleDataBind ()
		{
			ListBoxPoker l = new ListBoxPoker ();
			l.DataSource = GetExampleData ();
			l.DataTextField = "Company";
			l.DataBind ();
			l.DataBind ();
#if NET_2_0
			string exp = @"<select size=""4"">
	<option value=""Novell Inc."">Novell Inc.</option>
	<option value=""Microsoft Corp."">Microsoft Corp.</option>
	<option value=""Google"">Google</option>

</select>";
#else
			string exp = @"<select name size=""4"">
	<option value=""Novell Inc."">Novell Inc.</option>
	<option value=""Microsoft Corp."">Microsoft Corp.</option>
	<option value=""Google"">Google</option>

</select>";
#endif
			HtmlDiff.AssertAreEqual (exp, l.Render (), "DoubleDataBind");
		}

		class MyNC : Control, INamingContainer {
		}

		[Test]
		[Category ("NotDotNet")]
		[Category ("NotWorking")]
		public void NameIsUniqueID ()
		{
			ListBoxPoker list = new ListBoxPoker ();
			Page page = new Page ();
			page.ID = "pg";
			Control ctrl = new MyNC ();
			ctrl.ID = "ctrl";
			page.Controls.Add (ctrl);
			ctrl.Controls.Add (list);
			string str = list.Render();
			Assert.IsTrue (-1 != list.Render ().IndexOf (':'), "unique");
		}

		[Test]
		public void HtmlEncodedText ()
		{
			ListBoxPoker list = new ListBoxPoker ();
			// The att. value is encoded by the writer, but the text is encoded in ListBox.
			list.Items.Add (new ListItem ("\"hola", "\"adios"));
			string output = list.Render ();
			Assert.IsTrue (-1 != output.IndexOf ("&quot;hola"), "#01");
			Assert.IsTrue (-1 != output.IndexOf ("&quot;adios"), "#02");
		}

		[Test]
		public void SelectSingle1 ()
		{
			ListBoxPoker list = new ListBoxPoker ();
			list.Items.Add (new ListItem ("1", "first"));
			list.Items.Add (new ListItem ("2", "second"));
			list.SelectedIndex = 0;
			NameValueCollection coll = new NameValueCollection ();
			coll.Add ("2", "second");
			Assert.IsTrue (list.LoadPD ("2", coll), "#00");
			Assert.IsFalse (list.Items [0].Selected, "#01");
			Assert.IsTrue (list.Items [1].Selected, "#02");
			Assert.AreEqual (1, list.SelectedIndex, "#03");
		}

		[Test]
		public void SelectSingle2 ()
		{
			ListBoxPoker list = new ListBoxPoker ();
			list.Items.Add (new ListItem ("1", "first"));
			list.Items.Add (new ListItem ("2", "second"));
			list.SelectedIndex = 0;
			NameValueCollection coll = new NameValueCollection ();
			coll.Add ("willnotbefound", "second");
			Assert.IsTrue (list.LoadPD ("2", coll), "#00");
			Assert.IsFalse (list.Items [0].Selected, "#01");
			Assert.IsFalse (list.Items [1].Selected, "#02");
			Assert.AreEqual (-1, list.SelectedIndex, "#03");
		}

		[Test]
		public void SelectMultiple1 ()
		{
			ListBoxPoker list = new ListBoxPoker ();
			list.SelectionMode = ListSelectionMode.Multiple;
			list.Items.Add (new ListItem ("1", "first"));
			list.Items.Add (new ListItem ("2", "second"));
			list.SelectedIndex = 0;
			NameValueCollection coll = new NameValueCollection ();
			coll.Add ("2", "second");
			Assert.IsTrue (list.LoadPD ("2", coll), "#00");
			Assert.IsFalse (list.Items [0].Selected, "#01");
			Assert.IsTrue (list.Items [1].Selected, "#02");
			Assert.AreEqual (1, list.SelectedIndex, "#03");
		}

		[Test]
		public void SelectMultiple2 ()
		{
			ListBoxPoker list = new ListBoxPoker ();
			list.SelectionMode = ListSelectionMode.Multiple;
			list.Items.Add (new ListItem ("1", "first"));
			list.Items.Add (new ListItem ("2", "second"));
			list.Items.Add (new ListItem ("3", "third"));
			list.Items.Add (new ListItem ("4", "forth"));
			NameValueCollection coll = new NameValueCollection ();
			coll.Add ("key", "second");
			coll.Add ("key", "forth");
			Assert.IsTrue (list.LoadPD ("key", coll), "#00");
			Assert.IsFalse (list.Items [0].Selected, "#01");
			Assert.IsTrue (list.Items [1].Selected, "#02");
			Assert.IsFalse (list.Items [2].Selected, "#03");
			Assert.IsTrue (list.Items [3].Selected, "#04");

			Assert.IsFalse (list.LoadPD ("key", coll), "#05");
			Assert.IsFalse (list.Items [0].Selected, "#06");
			Assert.IsTrue (list.Items [1].Selected, "#07");
			Assert.IsFalse (list.Items [2].Selected, "#08");
			Assert.IsTrue (list.Items [3].Selected, "#09");

			coll.Clear ();
			coll.Add ("key", "first");
			coll.Add ("key", "third");
			Assert.IsTrue (list.LoadPD ("key", coll), "#10");
			Assert.IsTrue (list.Items [0].Selected, "#11");
			Assert.IsFalse (list.Items [1].Selected, "#12");
			Assert.IsTrue (list.Items [2].Selected, "#13");
			Assert.IsFalse (list.Items [3].Selected, "#14");

		}
#if NET_2_0
	[Test]
	public void VerifyMultiSelectPositive()
	{
	    ListBoxPoker list = new ListBoxPoker();
	    list.SelectionMode = ListSelectionMode.Multiple;
	    list.VerifyMultiSelect();
	}

	[Test]
	[ExpectedException(typeof(HttpException))]
	public void VerifyMultiSelectNegative()
	{
	    ListBoxPoker list = new ListBoxPoker();
	    list.SelectionMode = ListSelectionMode.Single;
	    list.VerifyMultiSelect();
	}

		[Test]
		[Category ("NunitWeb")]
		public void ListItemsViewState1 ()
		{
			PageInvoker pi = PageInvoker.CreateOnLoad (new PageDelegate (ListItemsViewState_PageLoad));
			WebTest test = new WebTest (pi);

			string html = test.Run ();

			test.Request = new FormRequest (test.Response, "form1");
			test.Invoker = pi;
			html = test.Run ();
		}

		[Test]
		[Category ("NunitWeb")]
		public void ListItemsViewState2 ()
		{
			PageInvoker pi = PageInvoker.CreateOnLoad (new PageDelegate (ListItemsViewState_PageLoad2));
			WebTest test = new WebTest (pi);

			string html = test.Run ();

			test.Request = new FormRequest (test.Response, "form1");
			test.Invoker = pi;
			html = test.Run ();
		}

		[Test]
		[Category ("NunitWeb")]
		public void ListItemsViewState3 ()
		{
			PageDelegates pd = new PageDelegates ();
			pd.Init = ListItemsViewState_Init;
			pd.Load = ListItemsViewState_PageLoad3;
			WebTest test = new WebTest (new PageInvoker (pd));

			string html = test.Run ();
			Assert.IsTrue (html.IndexOf ("value=\"3\"") > 0, "ListItemsViewState3 #1");
			Assert.IsTrue (html.IndexOf ("value=\"4\"") > 0, "ListItemsViewState3 #2");

			test.Request = new FormRequest (test.Response, "form1");
			html = test.Run ();
			Assert.IsTrue (html.IndexOf ("value=\"3\"") > 0, "ListItemsViewState3 #3");
			Assert.IsTrue (html.IndexOf ("value=\"4\"") > 0, "ListItemsViewState3 #4");
		}

		[Test]
		[Category ("NunitWeb")]
		public void ListItemsViewState4 ()
		{
			PageDelegates pd = new PageDelegates ();
			pd.Init = ListItemsViewState_Init;
			pd.Load = ListItemsViewState_PageLoad4;
			WebTest test = new WebTest (new PageInvoker (pd));

			string html = test.Run ();
			Assert.IsTrue (html.IndexOf ("value=\"3\"") < 0, "ListItemsViewState4 #1");
			Assert.IsTrue (html.IndexOf ("value=\"heh\"") > 0, "ListItemsViewState4 #2");

			test.Request = new FormRequest (test.Response, "form1");
			html = test.Run ();
			Assert.IsTrue (html.IndexOf ("value=\"2\"") > 0, "ListItemsViewState4 #3");
			Assert.IsTrue (html.IndexOf ("value=\"3\"") < 0, "ListItemsViewState4 #4");
			Assert.IsTrue (html.IndexOf ("value=\"heh\"") > 0, "ListItemsViewState4 #5");
		}

		public static void ListItemsViewState_Init (Page p)
		{
			ListBox lb = new ListBox ();
			lb.ID = "TestedListBox";
			lb.Items.Add (new ListItem ("1", "1"));
			lb.Items.Add (new ListItem ("2", "2"));
			lb.Items.Add (new ListItem ("3", "3"));

			p.Form.Controls.Add (lb);
		}

		public static void ListItemsViewState_PageLoad (Page p)
		{
			ListBox lb = new ListBox ();
			if (!p.IsPostBack) {
				lb.Items.Add (new ListItem ("1", "1"));
				lb.Items.Add (new ListItem ("2", "2"));
				lb.Items.Add (new ListItem ("3", "3"));
				lb.Items.Add (new ListItem ("4", "4"));
				lb.Items.Add (new ListItem ("5", "5"));

				lb.Items [2].Selected = true;
			}

			p.Form.Controls.Add (lb);
		}

		public static void ListItemsViewState_PageLoad2 (Page p)
		{
			ListBox lb = new ListBox ();
			if (!p.IsPostBack) {
				lb.Items.Add (new ListItem ("1", "1"));
				lb.Items.Add (new ListItem ("2", "2"));
				lb.Items.Add (new ListItem ("3", "3"));
				lb.Items.Add (new ListItem ("4", "4"));
				lb.Items.Add (new ListItem ("5", "5"));

				lb.SelectedIndex = 2;
			}

			p.Form.Controls.Add (lb);
		}

		public static void ListItemsViewState_PageLoad3 (Page p)
		{
			ListBox lb = (ListBox) p.FindControl ("TestedListBox");
			if (!p.IsPostBack) {
				lb.Items.Add (new ListItem ("4", "4"));
				lb.Items.Add (new ListItem ("5", "5"));
			}
		}

		public static void ListItemsViewState_PageLoad4 (Page p)
		{
			ListBox lb = (ListBox) p.FindControl ("TestedListBox");
			if (!p.IsPostBack) {
				lb.Items [2].Text = "heh";
				lb.Items [2].Value = "heh";
				lb.Items [2].Selected = true;
			}
		}

		[Test]
		[Category ("NunitWeb")]
		public void ListItemsSelectedTest1 ()
		{
			PageDelegates pd = new PageDelegates ();
			pd.Init = ListItemsSelectedTest_Init;
			WebTest test = new WebTest (new PageInvoker (pd));
			string html = test.Run ();
			Assert.IsTrue (html.IndexOf ("selected=") < 0, "ListItemsSelectedTest1 #1");

			test.Request = new FormRequest (test.Response, "form1");
			//test.Invoker = new PageInvoker (pd);
			html = test.Run ();
			Assert.IsTrue (html.IndexOf ("selected=") < 0, "ListItemsSelectedTest1 #2");
		}

		[Test]
		[Category ("NunitWeb")]
		public void ListItemsSelectedTest2 ()
		{
			PageDelegates pd = new PageDelegates ();
			pd.Init = ListItemsSelectedTest_Init;
			pd.Load = ListItemsSelectedTest_Load;
			WebTest test = new WebTest (new PageInvoker (pd));
			string html = test.Run ();
			Assert.IsTrue (html.IndexOf ("selected=") < 0, "ListItemsSelectedTest2 #1");

			test.Request = new FormRequest (test.Response, "form1");
			html = test.Run ();
			Assert.IsTrue (html.IndexOf ("value=\"3\"") > 0, "ListItemsSelectedTest2 #2");
			Assert.IsTrue (html.IndexOf ("selected=") < 0, "ListItemsSelectedTest2 #3");
		}

		[Test]
		[Category ("NunitWeb")]
		public void ListItemsSelectedTest3 ()
		{
			PageDelegates pd = new PageDelegates ();
			pd.Init = ListItemsSelectedTest_Init;
			pd.Load = ListItemsSelectedTest_Load2;
			WebTest test = new WebTest (new PageInvoker (pd));
			string html = test.Run ();
			Assert.IsTrue (html.IndexOf ("selected=") > 0, "ListItemsSelectedTest3 #1");

			FormRequest fr = new FormRequest (test.Response, "form1");
			fr.Controls.Add ("TestedListBox");
			fr.Controls ["TestedListBox"].Value = "2";
			test.Request = fr;
			test.UserData = "";

			html = test.Run ();
			Assert.IsTrue (html.IndexOf ("value=\"3\"") > 0, "ListItemsSelectedTest3 #2");
			Assert.IsTrue (html.IndexOf ("value=\"2\"") > 0, "ListItemsSelectedTest3 #3");
			Assert.IsTrue (html.IndexOf ("selected=") > 0, "ListItemsSelectedTest3 #4");
			Assert.AreEqual ("SelectedIndexChanged", test.UserData, "ListItemsSelectedTest3 #5");
		}

		[Test]
		[Category ("NunitWeb")]
		public void ListItemsSelectedTest4 ()
		{
			PageDelegates pd = new PageDelegates ();
			pd.Init = ListItemsSelectedTest_Init;
			pd.Load = ListItemsSelectedTest_Load3;
			WebTest test = new WebTest (new PageInvoker (pd));
			string html = test.Run ();
			Assert.IsTrue (html.IndexOf ("selected=") > 0, "ListItemsSelectedTest4 #1");

			FormRequest fr = new FormRequest (test.Response, "form1");
			fr.Controls.Add ("TestedListBox");
			fr.Controls ["TestedListBox"].Value = "2";
			test.Request = fr;
			test.UserData = "";

			html = test.Run ();
			Assert.IsTrue (html.IndexOf ("value=\"3\"") > 0, "ListItemsSelectedTest4 #2");
			Assert.IsTrue (html.IndexOf ("selected=") > 0, "ListItemsSelectedTest4 #3");
			Assert.AreEqual ("SelectedIndexChanged", test.UserData, "ListItemsSelectedTest4 #4");
		}

		[Test]
		[Category ("NunitWeb")]
		public void ListItemsSelectedTest5 ()
		{
			PageDelegates pd = new PageDelegates ();
			pd.Init = ListItemsSelectedTest_Init2;
			pd.Load = ListItemsSelectedTest_Load4;
			WebTest test = new WebTest (new PageInvoker (pd));
			string html = test.Run ();
			Assert.IsTrue (html.IndexOf ("selected=") > 0, "ListItemsSelectedTest5 #1");

			FormRequest fr = new FormRequest (test.Response, "form1");
			fr.Controls.Add ("TestedListBox");
			fr.Controls ["TestedListBox"].Value = "2";
			test.Request = fr;
			test.UserData = "";

			html = test.Run ();
			Assert.IsTrue (html.IndexOf ("value=\"1\"") > 0, "ListItemsSelectedTest5 #2");
			Assert.IsTrue (html.IndexOf ("value=\"2\"") > 0, "ListItemsSelectedTest5 #2");
			Assert.IsTrue (html.IndexOf ("value=\"3\"") > 0, "ListItemsSelectedTest5 #3");
			Assert.IsTrue (html.IndexOf ("selected=") > 0, "ListItemsSelectedTest5 #4");
			Assert.AreEqual ("SelectedIndexChanged", test.UserData, "ListItemsSelectedTest5 #5");
		}

		public static void ListItemsSelectedTest_Init (Page p)
		{
			ListBox lb = new ListBox ();
			lb.ID = "TestedListBox";
			lb.SelectedIndex = 2;

			lb.SelectedIndexChanged += new EventHandler (ListItemsSelectedTest_lb_SelectedIndexChanged);

			p.Form.Controls.Add (lb);
		}

		public static void ListItemsSelectedTest_Init2 (Page p)
		{
			ListBox lb = new ListBox ();
			lb.ID = "TestedListBox";
			lb.Items.Add (new ListItem ("1", "1"));
			lb.Items.Add (new ListItem ("2", "2"));
			lb.Items.Add (new ListItem ("3", "3"));
			lb.Items.Add (new ListItem ("4", "4"));
			lb.Items.Add (new ListItem ("5", "5"));

			lb.SelectedIndexChanged += new EventHandler (ListItemsSelectedTest_lb_SelectedIndexChanged);

			p.Form.Controls.Add (lb);
		}

		public static void ListItemsSelectedTest_Load (Page p)
		{
			ListBox lb = (ListBox) p.FindControl ("TestedListBox");

			if (!p.IsPostBack) {
				lb.Items.Add (new ListItem ("1", "1"));
				lb.Items.Add (new ListItem ("2", "2"));
				lb.Items.Add (new ListItem ("3", "3"));
				lb.Items.Add (new ListItem ("4", "4"));
				lb.Items.Add (new ListItem ("5", "5"));
			}
		}

		public static void ListItemsSelectedTest_Load2 (Page p)
		{
			ListBox lb = (ListBox) p.FindControl ("TestedListBox");

			if (!p.IsPostBack) {
				lb.DataSource = new string [] { "1", "2", "3", "4", "5" };
				lb.DataBind ();
			}
		}

		public static void ListItemsSelectedTest_Load3 (Page p)
		{
			ListBox lb = (ListBox) p.FindControl ("TestedListBox");

			if (!p.IsPostBack) {
				lb.Items.Add (new ListItem ("1", "1"));
				lb.Items.Add (new ListItem ("2", "2"));
				lb.Items.Add (new ListItem ("3", "3"));
				lb.Items.Add (new ListItem ("4", "4"));
				lb.Items.Add (new ListItem ("5", "5")); 
				
				lb.Items [2].Selected = true;
			}
		}

		public static void ListItemsSelectedTest_Load4 (Page p)
		{
			ListBox lb = (ListBox) p.FindControl ("TestedListBox");

			if (!p.IsPostBack) {
				lb.Items [2].Text = "heh";
				lb.Items [2].Selected = true;
			}
		}

		public static void ListItemsSelectedTest_lb_SelectedIndexChanged (object sender, EventArgs e)
		{
			WebTest.CurrentTest.UserData = "SelectedIndexChanged";
		}
#endif
	}
}


