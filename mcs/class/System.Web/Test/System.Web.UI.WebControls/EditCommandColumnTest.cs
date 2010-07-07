//
// Tests for System.Web.UI.WebControls.EditCommandColumn.cs 
//
// Author:
//	Peter Dennis Bartok (pbartok@novell.com)
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
using System.Collections;
using System.Data;
using System.IO;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MonoTests.stand_alone.WebHarness;
using MonoTests.SystemWeb.Framework;

namespace MonoTests.System.Web.UI.WebControls
{

	
	
	[TestFixture]	
	public class EditCommandColumnTest {

		private class DataGridTest : DataGrid {
			public ArrayList CreateColumns (PagedDataSource data_source, bool use_data_source) {
				return CreateColumnSet (data_source, use_data_source);
			}

			public void CreateControls (bool use_data_source) {
				CreateControlHierarchy (use_data_source);
			}
		}
		
		[Test]
		public void Defaults ()
		{
			EditCommandColumn	e;

			e = new EditCommandColumn();

			Assert.AreEqual(ButtonColumnType.LinkButton, e.ButtonType, "D1");
			Assert.AreEqual(string.Empty, e.CancelText, "D2");
			Assert.AreEqual(string.Empty, e.EditText, "D3");
			Assert.AreEqual(string.Empty, e.UpdateText, "D4");
#if NET_2_0
			Assert.AreEqual (true, e.CausesValidation, "CausesValidation");
			Assert.AreEqual (string.Empty, e.ValidationGroup, "ValidationGroup");
#endif
		}

		[Test]
		public void Properties () {
			EditCommandColumn	e;

			e = new EditCommandColumn();

			e.ButtonType = ButtonColumnType.PushButton;
			Assert.AreEqual(ButtonColumnType.PushButton, e.ButtonType, "P1");

			e.CancelText = "Cancel this!";
			Assert.AreEqual("Cancel this!", e.CancelText, "D2");

			e.EditText = "Edit me good";
			Assert.AreEqual("Edit me good", e.EditText, "D3");

			e.UpdateText = "Update? What update?";
			Assert.AreEqual("Update? What update?", e.UpdateText, "D4");
#if NET_2_0
			e.CausesValidation = false;
			Assert.AreEqual (false, e.CausesValidation, "CausesValidation");
			e.ValidationGroup = "test";
			Assert.AreEqual ("test", e.ValidationGroup, "ValidationGroup");
#endif
		}

		private string ControlMarkup(Control c) {
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new CleanHtmlTextWriter (sw);

			c.RenderControl (tw);
			return sw.ToString ();
		}

		private void ShowControlsRecursive (Control c, int depth) {
			 StringWriter sw = new StringWriter ();
			 HtmlTextWriter tw = new CleanHtmlTextWriter (sw);

			 c.RenderControl (tw);
			 Console.WriteLine (sw.ToString ());

			Console.WriteLine (c);

			foreach (Control child in c.Controls)
				ShowControlsRecursive (child, depth + 5);
		}

		[Test]
		public void InitializeCell () 
		{
#if NET_4_0
			string origHtml = "<table><tr><td>&nbsp;</td><td>&nbsp;</td><td>one</td><td>two</td><td>three</td></tr><tr><td><a>Edit</a></td><td><input name=\"sucker$ctl02$ctl00\" type=\"submit\" value=\"Bearbeiten\" /></td><td>1</td><td>2</td><td>3</td></tr><tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr></table>";
#else
			string origHtml = "<table border=\"0\"><tr><td>&nbsp;</td><td>&nbsp;</td><td>one</td><td>two</td><td>three</td></tr><tr><td><a>Edit</a></td><td><input name=\"sucker$ctl02$ctl00\" type=\"submit\" value=\"Bearbeiten\" /></td><td>1</td><td>2</td><td>3</td></tr><tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr></table>";
#endif
			DataGridTest	p = new DataGridTest ();
			DataTable	table = new DataTable ();
			EditCommandColumn	e;
			string			markup;

			e = new EditCommandColumn();
			e.ButtonType = ButtonColumnType.LinkButton;
			e.CancelText = "Cancel";
			e.EditText = "Edit";
			e.UpdateText = "Update";			

			table.Columns.Add (new DataColumn ("one", typeof (string)));
			table.Columns.Add (new DataColumn ("two", typeof (string)));
			table.Columns.Add (new DataColumn ("three", typeof (string)));
			table.Rows.Add (new object [] { "1", "2", "3" });

			p.DataSource = new DataView (table);
			p.Columns.Add(e);

			e = new EditCommandColumn();
			e.ButtonType = ButtonColumnType.PushButton;
			e.CancelText = "Abbrechen";
			e.EditText = "Bearbeiten";
			e.UpdateText = "Refresh";			
			p.Columns.Add(e);

			// This will trigger EditCommandColumn.InitializeCell, without any EditItem set, tests the EditText render
			p.CreateControls (true);
			p.ID = "sucker";

			Assert.AreEqual (2, p.Columns.Count, "I1");
			markup = ControlMarkup(p.Controls[0]);
			markup = markup.Replace("\t", "");
			markup = markup.Replace ("\r", "");
			markup = markup.Replace ("\n", "");

			HtmlDiff.AssertAreEqual (origHtml, markup, "I2");

			//ShowControlsRecursive (p.Controls [0], 1);
		}

		[Test]
		public void ThisIsADGTest () 
		{
#if NET_4_0
			string origHtml = "<table id=\"sucker_tbl\"><tr><td>&nbsp;</td><td>&nbsp;</td><td>one</td><td>two</td><td>three</td></tr><tr><td><a>Edit</a></td><td><input name=\"sucker$ctl02$ctl00\" type=\"submit\" value=\"Bearbeiten\" /></td><td>1</td><td>2</td><td>3</td></tr><tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr></table>";
#else
			string origHtml = "<table border=\"0\" id=\"sucker_tbl\"><tr><td>&nbsp;</td><td>&nbsp;</td><td>one</td><td>two</td><td>three</td></tr><tr><td><a>Edit</a></td><td><input name=\"sucker$ctl02$ctl00\" type=\"submit\" value=\"Bearbeiten\" /></td><td>1</td><td>2</td><td>3</td></tr><tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr></table>";
#endif
			DataGridTest	p = new DataGridTest ();
			DataTable	table = new DataTable ();
			EditCommandColumn	e;
			string			markup;

			e = new EditCommandColumn();
			e.ButtonType = ButtonColumnType.LinkButton;
			e.CancelText = "Cancel";
			e.EditText = "Edit";
			e.UpdateText = "Update";			

			table.Columns.Add (new DataColumn ("one", typeof (string)));
			table.Columns.Add (new DataColumn ("two", typeof (string)));
			table.Columns.Add (new DataColumn ("three", typeof (string)));
			table.Rows.Add (new object [] { "1", "2", "3" });

			p.DataSource = new DataView (table);
			p.Columns.Add(e);

			e = new EditCommandColumn();
			e.ButtonType = ButtonColumnType.PushButton;
			e.CancelText = "Abbrechen";
			e.EditText = "Bearbeiten";
			e.UpdateText = "Refresh";			
			p.Columns.Add(e);

			p.CreateControls (true);
			// This is the test we want to run: setting the ID of the table created by
			// the datagrid overrides the using the ID of the datagrid itself and uses
			// the table ClientID instead.
			p.ID = "sucker";
			p.Controls [0].ID = "tbl";

			Assert.AreEqual (2, p.Columns.Count, "I1");
			markup = ControlMarkup(p.Controls[0]);
			markup = markup.Replace("\t", "");
			markup = markup.Replace ("\r", "");
			markup = markup.Replace ("\n", "");
			
			HtmlDiff.AssertAreEqual (origHtml, markup, "I2");
		}

		static void GetHierarchy (ControlCollection coll, int level, StringBuilder sb)
		{
			foreach (Control c in coll) {
				sb.AppendFormat ("{0}{1}\n", new string (' ', 2 * level), c.GetType ());
				GetHierarchy (c.Controls, level + 1, sb);
			}
		}

		[Test]
		public void InitializeEditCell () 
		{
#if NET_4_0
			string origHtml = "<table><tr><td>&nbsp;</td><td>&nbsp;</td><td>one</td><td>two</td><td>three</td></tr><tr><td><a>Update</a>&nbsp;<a>Cancel</a></td><td><input name=\"sucker$ctl02$ctl00\" type=\"submit\" value=\"Refresh\" />&nbsp;<input name=\"sucker$ctl02$ctl01\" type=\"submit\" value=\"Abbrechen\" /></td><td><input name=\"sucker$ctl02$ctl02\" type=\"text\" value=\"1\" /></td><td><input name=\"sucker$ctl02$ctl03\" type=\"text\" value=\"2\" /></td><td><input name=\"sucker$ctl02$ctl04\" type=\"text\" value=\"3\" /></td></tr><tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr></table>";
#else
			string origHtml = "<table border=\"0\"><tr><td>&nbsp;</td><td>&nbsp;</td><td>one</td><td>two</td><td>three</td></tr><tr><td><a>Update</a>&nbsp;<a>Cancel</a></td><td><input name=\"sucker$ctl02$ctl00\" type=\"submit\" value=\"Refresh\" />&nbsp;<input name=\"sucker$ctl02$ctl01\" type=\"submit\" value=\"Abbrechen\" /></td><td><input name=\"sucker$ctl02$ctl02\" type=\"text\" value=\"1\" /></td><td><input name=\"sucker$ctl02$ctl03\" type=\"text\" value=\"2\" /></td><td><input name=\"sucker$ctl02$ctl04\" type=\"text\" value=\"3\" /></td></tr><tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr></table>";
#endif
			DataGridTest	p = new DataGridTest ();
			DataTable	table = new DataTable ();
			EditCommandColumn	e;
			string			markup;

			e = new EditCommandColumn();
			e.ButtonType = ButtonColumnType.LinkButton;
			e.CancelText = "Cancel";
			e.EditText = "Edit";
			e.UpdateText = "Update";			

			table.Columns.Add (new DataColumn ("one", typeof (string)));
			table.Columns.Add (new DataColumn ("two", typeof (string)));
			table.Columns.Add (new DataColumn ("three", typeof (string)));
			table.Rows.Add (new object [] { "1", "2", "3" });

			p.DataSource = new DataView (table);
			p.Columns.Add(e);

			e = new EditCommandColumn();
			e.ButtonType = ButtonColumnType.PushButton;
			e.CancelText = "Abbrechen";
			e.EditText = "Bearbeiten";
			e.UpdateText = "Refresh";			
			p.Columns.Add(e);

			// Force the ListItemType to be EditItem so we can test rendering the UpdateText/CancelText render
			p.EditItemIndex = 0;

			// This will trigger EditCommandColumn.InitializeCell
			p.CreateControls (true);
			p.ID = "sucker";

			StringBuilder sb = new StringBuilder ();
			GetHierarchy (p.Controls, 0, sb);
			string h = sb.ToString ();
			int x = h.IndexOf (".TextBox");
			// These are from the BoundColumns
			Assert.IsTrue (x != -1, "textbox1");
			x = h.IndexOf (".TextBox", x + 1);
			Assert.IsTrue (x != -1, "textbox2");
			x = h.IndexOf (".TextBox", x + 1);
			Assert.IsTrue (x != -1, "textbox3");
			x = h.IndexOf (".TextBox", x + 1);
			Assert.IsTrue (x == -1, "textbox-end");

			markup = ControlMarkup (p.Controls[0]);
			markup = markup.Replace ("\t", "");
			markup = markup.Replace ("\r", "");
			markup = markup.Replace ("\n", "");

//Console.WriteLine("Markup:>{0}<", markup);
			Assert.AreEqual (2, p.Columns.Count, "I1");

			HtmlDiff.AssertAreEqual (origHtml, markup, "I2");
		}

		[Test]
		[Ignore("Unfinished")]
		public void InitializeReadOnlyEditCell ()
		{
			DataGridTest p = new DataGridTest ();
			DataTable table = new DataTable ();
			EditCommandColumn e;
			string markup;

			e = new EditCommandColumn ();
			e.ButtonType = ButtonColumnType.LinkButton;
			e.CancelText = "Cancel";
			e.EditText = "Edit";
			e.UpdateText = "Update";

			table.Columns.Add (new DataColumn ("one", typeof (string)));
			table.Columns.Add (new DataColumn ("two", typeof (string)));
			table.Columns.Add (new DataColumn ("three", typeof (string)));
			table.Rows.Add (new object[] { "1", "2", "3" });

			p.DataSource = new DataView (table);
			p.Columns.Add (e);

			e = new EditCommandColumn ();
			e.ButtonType = ButtonColumnType.PushButton;
			
			e.CancelText = "Abbrechen";
			e.EditText = "Bearbeiten";
			e.UpdateText = "Refresh";
			p.Columns.Add (e);

			// Force the ListItemType to be EditItem so we can test rendering the UpdateText/CancelText render
			p.EditItemIndex = 0;

			// This will trigger EditCommandColumn.InitializeCell
			p.CreateControls (true);
			p.ID = "sucker";

			markup = ControlMarkup (p.Controls[0]);
			markup = markup.Replace ("\t", "");
			markup = markup.Replace ("\r", "");
			markup = markup.Replace ("\n", "");

			Assert.AreEqual (2, p.Columns.Count, "I1");
			Assert.AreEqual (
				"<table border=\"0\" id=\"sucker\"><tr><td>&nbsp;</td><td>&nbsp;</td><td>one</td><td>two</td><td>three</td>" +
				"</tr><tr><td><a>Update</a>&nbsp;<a>Cancel</a></td><td><input name type=\"submit\" value=\"Refresh\" />&nbsp;" +
				"<input name value=\"Abbrechen\" type=\"submit\" /></td>" +
				"<td><input name=\"_ctl2:_ctl0\" type=\"text\" value=\"1\" /></td>" +
				"<td><input name=\"_ctl2:_ctl1\" type=\"text\" value=\"2\" /></td>" +
				"<td><input name=\"_ctl2:_ctl2\" type=\"text\" value=\"3\" /></td>" +
				"</tr><tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td>" +
				"</tr></table>", markup, "I2");
		}

#if NET_2_0
		[Test]
		[Category ("NunitWeb")]
		public void Validation_ValidatingValid () 
		{
			WebTest t = new WebTest ();
			PageDelegates pd = new PageDelegates ();
			pd.Load = Validation_Load;
			pd.PreRender = Validation_PreRender;
			t.Invoker = new PageInvoker (pd);
			t.UserData = "ValidatingValid";
			string html = t.Run ();
			FormRequest fr = new FormRequest (t.Response, "form1");

			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls ["__EVENTTARGET"].Value = (string) t.UserData;
			fr.Controls ["__EVENTARGUMENT"].Value = "";
			t.Request = fr;
			t.UserData = "ValidatingValid";
			
			html = t.Run ();
		}

		[Test]
		[Category ("NunitWeb")]
		[Ignore ("Possibly incorrectly constructed test - conflicts with fix for bug #471305")]
		public void Validation_ValidatingInvalid () {
			WebTest t = new WebTest ();
			PageDelegates pd = new PageDelegates ();
			pd.Load = Validation_Load;
			pd.PreRender = Validation_PreRender;
			t.Invoker = new PageInvoker (pd);
			t.UserData = "ValidatingInvalid";
			string html = t.Run ();
			FormRequest fr = new FormRequest (t.Response, "form1");

			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls ["__EVENTTARGET"].Value = (string)t.UserData;
			fr.Controls ["__EVENTARGUMENT"].Value = "";
			t.Request = fr;
			t.UserData = "ValidatingInvalid";

			html = t.Run ();
		}

		[Test]
		[Category ("NunitWeb")]
		public void Validation_NotValidatingInvalid () {
			WebTest t = new WebTest ();
			PageDelegates pd = new PageDelegates ();
			pd.Load = Validation_Load;
			pd.PreRender = Validation_PreRender;
			t.Invoker = new PageInvoker (pd);
			t.UserData = "NotValidatingInvalid";
			string html = t.Run ();
			FormRequest fr = new FormRequest (t.Response, "form1");

			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls ["__EVENTTARGET"].Value = (string) t.UserData;
			fr.Controls ["__EVENTARGUMENT"].Value = "";
			t.Request = fr;
			t.UserData = "NotValidatingInvalid";

			html = t.Run ();
		}

		[Test]
		[Category ("NunitWeb")]
		[Ignore ("Possibly incorrectly constructed test - conflicts with fix for bug #471305")]
		public void Validation_ValidationGroupIncluded () {
			WebTest t = new WebTest ();
			PageDelegates pd = new PageDelegates ();
			pd.Load = Validation_Load;
			pd.PreRender = Validation_PreRender;
			t.Invoker = new PageInvoker (pd);
			t.UserData = "ValidationGroupIncluded";
			string html = t.Run ();
			FormRequest fr = new FormRequest (t.Response, "form1");

			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls ["__EVENTTARGET"].Value = (string) t.UserData;
			fr.Controls ["__EVENTARGUMENT"].Value = "";
			t.Request = fr;
			t.UserData = "ValidationGroupIncluded";

			html = t.Run ();
		}

		[Test]
		[Category ("NunitWeb")]
		public void Validation_ValidationGroupNotIncluded () {
			WebTest t = new WebTest ();
			PageDelegates pd = new PageDelegates ();
			pd.Load = Validation_Load;
			pd.PreRender = Validation_PreRender;
			t.Invoker = new PageInvoker (pd);
			t.UserData = "ValidationGroupNotIncluded";
			string html = t.Run ();
			FormRequest fr = new FormRequest (t.Response, "form1");

			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls ["__EVENTTARGET"].Value = (string) t.UserData;
			fr.Controls ["__EVENTARGUMENT"].Value = "";
			t.Request = fr;
			t.UserData = "ValidationGroupNotIncluded";

			html = t.Run ();
		}

		public static void Validation_Load (Page p) 
		{
			string testType = (string)WebTest.CurrentTest.UserData;
			DataGridTest dg = new DataGridTest ();
			dg.ID = "mygrid";
			EditCommandColumn e;

			e = new EditCommandColumn ();
			e.ButtonType = ButtonColumnType.LinkButton;
			e.CancelText = "Cancel";
			e.EditText = "Edit";
			e.UpdateText = "Update";

			switch (testType) {
			case "ValidatingValid":
			case "ValidatingInvalid":
			case "ValidationGroupIncluded":
			case "ValidationGroupNotIncluded":
				e.CausesValidation = true;
				break;

			case "NotValidatingInvalid":
				e.CausesValidation = false;
				break;
			}

			switch (testType) {
			case "ValidationGroupIncluded":
			case "ValidationGroupNotIncluded":
				e.ValidationGroup = "Group1";
				break;

			default:
				e.ValidationGroup = "";
				break;
			}

			dg.Columns.Add (e);

			TextBox tb = new TextBox ();
			tb.ID = "Text1";
			switch (testType) {
			case "ValidatingValid":
				tb.Text = "111";
				break;

			case "ValidatingInvalid":
			case "NotValidatingInvalid":
			case "ValidationGroupIncluded":
			case "ValidationGroupNotIncluded":
				tb.Text = "";
				break;
			}

			RequiredFieldValidator v = new RequiredFieldValidator ();
			v.ControlToValidate = "Text1";
			switch (testType) {
			case "ValidationGroupIncluded":
				v.ValidationGroup = "Group1";
				break;

			case "ValidationGroupNotIncluded":
				v.ValidationGroup = "NotGroup1";
				break;

			default:
				v.ValidationGroup = "";
				break;
			}
			TemplateColumn tc = new TemplateColumn ();
			tc.EditItemTemplate = new ValidatingEditTemplate (tb, v);
			dg.Columns.Add (tc);

			ObjectDataSource ods = new ObjectDataSource ("MyObjectDS", "Select");
			ods.UpdateMethod = "Update";
			ods.DataObjectTypeName = "MyObjectDS";
			ods.ID = "MyDS";

			p.Form.Controls.Add (ods);

			dg.DataSource = ods;
			//dg.DataKeyField = "i";

			//DataTable table = new DataTable ();
			//table.Columns.Add (new DataColumn ("one", typeof (string)));
			//table.Columns.Add (new DataColumn ("two", typeof (string)));
			//table.Columns.Add (new DataColumn ("three", typeof (string)));
			//table.Rows.Add (new object [] { "1", "2", "3" });

			//dg.DataSource = new DataView (table);

			dg.EditItemIndex = 0;
			p.Form.Controls.Add (dg);

			dg.DataBind ();
			if (!p.IsPostBack) {
				WebTest.CurrentTest.UserData = dg.Items [0].Cells [0].Controls [0].UniqueID;
			}
		}

		public static void Validation_PreRender (Page p) 
		{
			string testType = (string) WebTest.CurrentTest.UserData;

			if (p.IsPostBack) {
				switch (testType) {
				case "ValidatingValid":
				case "ValidationGroupNotIncluded":
					Assert.AreEqual (true, p.IsValid, "ValidatingValid");
					break;
				case "ValidatingInvalid":
				case "ValidationGroupIncluded":
					Assert.AreEqual (false, p.IsValid, "ValidatingInvalid");
					break;

				case "NotValidatingInvalid":
					bool isValidated = true;
					try {
						if (p.IsValid) {
							Assert.Fail ("NotValidatingInvalid IsValid == true");
						}
					}
					catch (HttpException httpException) {
						isValidated = false;
					}
					Assert.AreEqual(false, isValidated, "NotValidatingInvalid");
					break;
				}
			}
		}

		public class ValidatingEditTemplate : ITemplate
		{
			public ValidatingEditTemplate (params Control [] templateControls) 
			{
				this.templateControls = new Control[templateControls.Length];
				templateControls.CopyTo (this.templateControls, 0);
			}

			#region ITemplate Members

			public void InstantiateIn (Control container) 
			{
				foreach (Control c in templateControls) {
					container.Controls.Add (c);
				}
			}

			#endregion

			private Control[] templateControls;
		}
#endif
	}
}

#if NET_2_0
#region MyObjectDS
public class MyObjectDS
{
	public MyObjectDS () {
		_i = 0;
	}

	public MyObjectDS (int value) {
		_i = value;
	}

	private int _i;
	public int i {
		get { return _i; }
		set { _i = value; }
	}

	static MyObjectDS [] myData = null;

	public static IList Select () {
		if (myData == null) {
			myData = new MyObjectDS [] { new MyObjectDS (1), new MyObjectDS (2), new MyObjectDS (3) };
		}
		return myData;
	}

	public static void Update (MyObjectDS instance) {
	}
}
#endregion
#endif
