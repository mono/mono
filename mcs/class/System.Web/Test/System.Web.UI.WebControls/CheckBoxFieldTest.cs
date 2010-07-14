//
// Tests for System.Web.UI.WebControls.CheckBoxFieldTest.cs
//
// Author:
//	Yoni Klein (yonik@mainsoft.com)
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


#if NET_2_0


using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Drawing;
using System.Collections;
using System.Collections.Specialized;
using NUnit.Framework;
using System.Data;

using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;

namespace MonoTests.System.Web.UI.WebControls
{
	class PokerCheckBoxField : CheckBoxField
	{
		// View state Stuff
		public PokerCheckBoxField ()
			: base ()
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

		public StateBag StateBag
		{
			get { return base.ViewState; }
		}

		public bool GetSupportsHtmlEncode
		{
			get
			{
				return base.SupportsHtmlEncode;
			}
		}

		public void DoCopyProperties (DataControlField newField)
		{
			base.CopyProperties (newField);
		}

		public DataControlField DoCreateField ()
		{
			return base.CreateField ();
		}

		public object DoGetDesignTimeValue ()
		{
			return base.GetDesignTimeValue ();
		}

		public void DoInitializeDataCell (DataControlFieldCell cell, DataControlRowState rowState)
		{
			this.InitializeDataCell (cell, rowState);
		}

		protected override void OnDataBindField (object sender, EventArgs e)
		{
			base.OnDataBindField (sender, e);
			CheckBoxFieldTest.databound += 1;
		}
	}
	
	[TestFixture]
	public class CheckBoxFieldTest
	{
		public const string FIELDNAME  = "checkbox";
		public const string WRONGFIELD = "str";
		public static int databound;

		[TestFixtureSetUp]
		public void SetUp ()
		{
			WebTest.CopyResource (GetType (), "CheckBoxField_Bug595568_0.aspx", "CheckBoxField_Bug595568_0.aspx");
			WebTest.CopyResource (GetType (), "CheckBoxField_Bug595568_1.aspx", "CheckBoxField_Bug595568_1.aspx");
			WebTest.CopyResource (GetType (), "CheckBoxField_Bug595568_2.aspx", "CheckBoxField_Bug595568_2.aspx");
			WebTest.CopyResource (GetType (), "CheckBoxField_Bug595568_5.aspx", "CheckBoxField_Bug595568_5.aspx");
			WebTest.CopyResource (GetType (), "CheckBoxField_Bug595568_6.aspx", "CheckBoxField_Bug595568_6.aspx");
			WebTest.CopyResource (GetType (), "CheckBoxField_Bug595568_7.aspx", "CheckBoxField_Bug595568_7.aspx");
		}

		[Test (Description="Bug 595568 #0")]
		public void CheckBoxField_Bug595568_0 ()
		{
#if NET_4_0
			string originalHtml = "<div>\r\n\t<table cellspacing=\"0\" rules=\"all\" border=\"1\" id=\"gridView\" style=\"border-collapse:collapse;\">\r\n\t\t<tr>\r\n\t\t\t<th scope=\"col\">&nbsp;</th>\r\n\t\t</tr><tr>\r\n\t\t\t<td><span title=\"Dummy\"><input id=\"gridView_ctl00_0\" type=\"checkbox\" name=\"gridView$ctl02$ctl00\" checked=\"checked\" /></span></td>\r\n\t\t</tr><tr>\r\n\t\t\t<td><span title=\"Dummy\"><input id=\"gridView_ctl00_1\" type=\"checkbox\" name=\"gridView$ctl03$ctl00\" /></span></td>\r\n\t\t</tr><tr>\r\n\t\t\t<td><span title=\"Dummy\"><input id=\"gridView_ctl00_2\" type=\"checkbox\" name=\"gridView$ctl04$ctl00\" /></span></td>\r\n\t\t</tr>\r\n\t</table>\r\n</div>";
#else
			string originalHtml = "<div>\r\n\t<table cellspacing=\"0\" rules=\"all\" border=\"1\" id=\"gridView\" style=\"border-collapse:collapse;\">\r\n\t\t<tr>\r\n\t\t\t<th scope=\"col\">&nbsp;</th>\r\n\t\t</tr><tr>\r\n\t\t\t<td><span title=\"Dummy\"><input id=\"gridView_ctl02_ctl00\" type=\"checkbox\" name=\"gridView$ctl02$ctl00\" checked=\"checked\" /></span></td>\r\n\t\t</tr><tr>\r\n\t\t\t<td><span title=\"Dummy\"><input id=\"gridView_ctl03_ctl00\" type=\"checkbox\" name=\"gridView$ctl03$ctl00\" /></span></td>\r\n\t\t</tr><tr>\r\n\t\t\t<td><span title=\"Dummy\"><input id=\"gridView_ctl04_ctl00\" type=\"checkbox\" name=\"gridView$ctl04$ctl00\" /></span></td>\r\n\t\t</tr>\r\n\t</table>\r\n</div>";
#endif
			WebTest t = new WebTest ("CheckBoxField_Bug595568_0.aspx");
			string pageHtml = t.Run ();
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (pageHtml);

			HtmlDiff.AssertAreEqual (originalHtml, renderedHtml, "#A1");
		}

		[Test (Description="Bug 595568 #1")]
		public void CheckBoxField_Bug595568_1 ()
		{
#if NET_4_0
			string originalHtml = "<div>\r\n\t<table cellspacing=\"0\" rules=\"all\" border=\"1\" id=\"gridView\" style=\"border-collapse:collapse;\">\r\n\t\t<tr>\r\n\t\t\t<th scope=\"col\">&nbsp;</th>\r\n\t\t</tr><tr>\r\n\t\t\t<td><select size=\"4\" name=\"gridView$ctl02$ctl00\">\r\n\r\n\t\t\t</select><span title=\"Dummy\"><input id=\"gridView_ctl01_0\" type=\"checkbox\" name=\"gridView$ctl02$ctl01\" checked=\"checked\" /></span></td>\r\n\t\t</tr><tr>\r\n\t\t\t<td><select size=\"4\" name=\"gridView$ctl03$ctl00\">\r\n\r\n\t\t\t</select><span title=\"Dummy\"><input id=\"gridView_ctl01_1\" type=\"checkbox\" name=\"gridView$ctl03$ctl01\" /></span></td>\r\n\t\t</tr><tr>\r\n\t\t\t<td><select size=\"4\" name=\"gridView$ctl04$ctl00\">\r\n\r\n\t\t\t</select><span title=\"Dummy\"><input id=\"gridView_ctl01_2\" type=\"checkbox\" name=\"gridView$ctl04$ctl01\" /></span></td>\r\n\t\t</tr>\r\n\t</table>\r\n</div>";
#else
			string originalHtml = "<div>\r\n\t<table cellspacing=\"0\" rules=\"all\" border=\"1\" id=\"gridView\" style=\"border-collapse:collapse;\">\r\n\t\t<tr>\r\n\t\t\t<th scope=\"col\">&nbsp;</th>\r\n\t\t</tr><tr>\r\n\t\t\t<td><select size=\"4\" name=\"gridView$ctl02$ctl00\">\r\n\r\n\t\t\t</select><span title=\"Dummy\"><input id=\"gridView_ctl02_ctl01\" type=\"checkbox\" name=\"gridView$ctl02$ctl01\" checked=\"checked\" /></span></td>\r\n\t\t</tr><tr>\r\n\t\t\t<td><select size=\"4\" name=\"gridView$ctl03$ctl00\">\r\n\r\n\t\t\t</select><span title=\"Dummy\"><input id=\"gridView_ctl03_ctl01\" type=\"checkbox\" name=\"gridView$ctl03$ctl01\" /></span></td>\r\n\t\t</tr><tr>\r\n\t\t\t<td><select size=\"4\" name=\"gridView$ctl04$ctl00\">\r\n\r\n\t\t\t</select><span title=\"Dummy\"><input id=\"gridView_ctl04_ctl01\" type=\"checkbox\" name=\"gridView$ctl04$ctl01\" /></span></td>\r\n\t\t</tr>\r\n\t</table>\r\n</div>";
#endif
			WebTest t = new WebTest ("CheckBoxField_Bug595568_1.aspx");
			string pageHtml = t.Run ();
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (pageHtml);

			HtmlDiff.AssertAreEqual (originalHtml, renderedHtml, "#A1");
		}

		[Test (Description="Bug 595568 #2")]
		public void CheckBoxField_Bug595568_2 ()
		{
#if NET_4_0
			string originalHtml = "<div>\r\n\t<table cellspacing=\"0\" rules=\"all\" border=\"1\" id=\"gridView\" style=\"border-collapse:collapse;\">\r\n\t\t<tr>\r\n\t\t\t<th scope=\"col\">&nbsp;</th>\r\n\t\t</tr><tr>\r\n\t\t\t<td><input id=\"gridView_ctl00_0\" type=\"checkbox\" name=\"gridView$ctl02$ctl00\" /><span title=\"Dummy\"><input id=\"gridView_ctl01_0\" type=\"checkbox\" name=\"gridView$ctl02$ctl01\" checked=\"checked\" /></span><input id=\"gridView_ctl02_0\" type=\"checkbox\" name=\"gridView$ctl02$ctl02\" /></td>\r\n\t\t</tr><tr>\r\n\t\t\t<td><input id=\"gridView_ctl00_1\" type=\"checkbox\" name=\"gridView$ctl03$ctl00\" /><span title=\"Dummy\"><input id=\"gridView_ctl01_1\" type=\"checkbox\" name=\"gridView$ctl03$ctl01\" /></span><input id=\"gridView_ctl02_1\" type=\"checkbox\" name=\"gridView$ctl03$ctl02\" /></td>\r\n\t\t</tr><tr>\r\n\t\t\t<td><input id=\"gridView_ctl00_2\" type=\"checkbox\" name=\"gridView$ctl04$ctl00\" /><span title=\"Dummy\"><input id=\"gridView_ctl01_2\" type=\"checkbox\" name=\"gridView$ctl04$ctl01\" /></span><input id=\"gridView_ctl02_2\" type=\"checkbox\" name=\"gridView$ctl04$ctl02\" /></td>\r\n\t\t</tr>\r\n\t</table>\r\n</div>";
#else
			string originalHtml = "<div>\r\n\t<table cellspacing=\"0\" rules=\"all\" border=\"1\" id=\"gridView\" style=\"border-collapse:collapse;\">\r\n\t\t<tr>\r\n\t\t\t<th scope=\"col\">&nbsp;</th>\r\n\t\t</tr><tr>\r\n\t\t\t<td><input id=\"gridView_ctl02_ctl00\" type=\"checkbox\" name=\"gridView$ctl02$ctl00\" /><span title=\"Dummy\"><input id=\"gridView_ctl02_ctl01\" type=\"checkbox\" name=\"gridView$ctl02$ctl01\" checked=\"checked\" /></span><input id=\"gridView_ctl02_ctl02\" type=\"checkbox\" name=\"gridView$ctl02$ctl02\" /></td>\r\n\t\t</tr><tr>\r\n\t\t\t<td><input id=\"gridView_ctl03_ctl00\" type=\"checkbox\" name=\"gridView$ctl03$ctl00\" /><span title=\"Dummy\"><input id=\"gridView_ctl03_ctl01\" type=\"checkbox\" name=\"gridView$ctl03$ctl01\" /></span><input id=\"gridView_ctl03_ctl02\" type=\"checkbox\" name=\"gridView$ctl03$ctl02\" /></td>\r\n\t\t</tr><tr>\r\n\t\t\t<td><input id=\"gridView_ctl04_ctl00\" type=\"checkbox\" name=\"gridView$ctl04$ctl00\" /><span title=\"Dummy\"><input id=\"gridView_ctl04_ctl01\" type=\"checkbox\" name=\"gridView$ctl04$ctl01\" /></span><input id=\"gridView_ctl04_ctl02\" type=\"checkbox\" name=\"gridView$ctl04$ctl02\" /></td>\r\n\t\t</tr>\r\n\t</table>\r\n</div>";
#endif
			
			WebTest t = new WebTest ("CheckBoxField_Bug595568_2.aspx");
			string pageHtml = t.Run ();
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (pageHtml);

			HtmlDiff.AssertAreEqual (originalHtml, renderedHtml, "#A1");
		}

		[Test (Description="Bug 595568 #5")]
		public void CheckBoxField_Bug595568_5 ()
		{
			string originalHtml = "<div>\r\n\t<table cellspacing=\"0\" rules=\"all\" border=\"1\" id=\"gridView\" style=\"border-collapse:collapse;\">\r\n\t\t<tr>\r\n\t\t\t<th scope=\"col\">&nbsp;</th>\r\n\t\t</tr><tr>\r\n\t\t\t<td><select size=\"4\" name=\"gridView$ctl02$ctl00\">\r\n\r\n\t\t\t</select></td>\r\n\t\t</tr><tr>\r\n\t\t\t<td><select size=\"4\" name=\"gridView$ctl03$ctl00\">\r\n\r\n\t\t\t</select></td>\r\n\t\t</tr><tr>\r\n\t\t\t<td><select size=\"4\" name=\"gridView$ctl04$ctl00\">\r\n\r\n\t\t\t</select></td>\r\n\t\t</tr>\r\n\t</table>\r\n</div>";

			WebTest t = new WebTest ("CheckBoxField_Bug595568_5.aspx");
			string pageHtml = t.Run ();
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (pageHtml);

			HtmlDiff.AssertAreEqual (originalHtml, renderedHtml, "#A1");
		}
		
		[Test (Description="Bug 595568 #6")]
		public void CheckBoxField_Bug595568_6 ()
		{	
#if NET_4_0
			string originalHtml = "<div>\r\n\t<table cellspacing=\"0\" rules=\"all\" border=\"1\" id=\"gridView\" style=\"border-collapse:collapse;\">\r\n\t\t<tr>\r\n\t\t\t<th scope=\"col\">&nbsp;</th>\r\n\t\t</tr><tr>\r\n\t\t\t<td><select size=\"4\" name=\"gridView$ctl02$ctl00\">\r\n\r\n\t\t\t</select><input id=\"gridView_ctl01_0\" type=\"checkbox\" name=\"gridView$ctl02$ctl01\" /></td>\r\n\t\t</tr><tr>\r\n\t\t\t<td><select size=\"4\" name=\"gridView$ctl03$ctl00\">\r\n\r\n\t\t\t</select><input id=\"gridView_ctl01_1\" type=\"checkbox\" name=\"gridView$ctl03$ctl01\" /></td>\r\n\t\t</tr><tr>\r\n\t\t\t<td><select size=\"4\" name=\"gridView$ctl04$ctl00\">\r\n\r\n\t\t\t</select><input id=\"gridView_ctl01_2\" type=\"checkbox\" name=\"gridView$ctl04$ctl01\" /></td>\r\n\t\t</tr>\r\n\t</table>\r\n</div>";
#else
			string originalHtml = "<div>\r\n\t<table cellspacing=\"0\" rules=\"all\" border=\"1\" id=\"gridView\" style=\"border-collapse:collapse;\">\r\n\t\t<tr>\r\n\t\t\t<th scope=\"col\">&nbsp;</th>\r\n\t\t</tr><tr>\r\n\t\t\t<td><select size=\"4\" name=\"gridView$ctl02$ctl00\">\r\n\r\n\t\t\t</select><input id=\"gridView_ctl02_ctl01\" type=\"checkbox\" name=\"gridView$ctl02$ctl01\" /></td>\r\n\t\t</tr><tr>\r\n\t\t\t<td><select size=\"4\" name=\"gridView$ctl03$ctl00\">\r\n\r\n\t\t\t</select><input id=\"gridView_ctl03_ctl01\" type=\"checkbox\" name=\"gridView$ctl03$ctl01\" /></td>\r\n\t\t</tr><tr>\r\n\t\t\t<td><select size=\"4\" name=\"gridView$ctl04$ctl00\">\r\n\r\n\t\t\t</select><input id=\"gridView_ctl04_ctl01\" type=\"checkbox\" name=\"gridView$ctl04$ctl01\" /></td>\r\n\t\t</tr>\r\n\t</table>\r\n</div>";
#endif

			WebTest t = new WebTest ("CheckBoxField_Bug595568_6.aspx");
			string pageHtml = t.Run ();
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (pageHtml);

			HtmlDiff.AssertAreEqual (originalHtml, renderedHtml, "#A1");
		}

		[Test (Description="Bug 595568 #7")]
		public void CheckBoxField_Bug595568_7 ()
		{
#if NET_4_0
			string originalHtml = "<div>\r\n\t<table cellspacing=\"0\" rules=\"all\" border=\"1\" id=\"gridView\" style=\"border-collapse:collapse;\">\r\n\t\t<tr>\r\n\t\t\t<th scope=\"col\">&nbsp;</th>\r\n\t\t</tr><tr>\r\n\t\t\t<td><input id=\"gridView_ctl00_0\" type=\"checkbox\" name=\"gridView$ctl02$ctl00\" /><select size=\"4\" name=\"gridView$ctl02$ctl01\">\r\n\r\n\t\t\t</select></td>\r\n\t\t</tr><tr>\r\n\t\t\t<td><input id=\"gridView_ctl00_1\" type=\"checkbox\" name=\"gridView$ctl03$ctl00\" /><select size=\"4\" name=\"gridView$ctl03$ctl01\">\r\n\r\n\t\t\t</select></td>\r\n\t\t</tr><tr>\r\n\t\t\t<td><input id=\"gridView_ctl00_2\" type=\"checkbox\" name=\"gridView$ctl04$ctl00\" /><select size=\"4\" name=\"gridView$ctl04$ctl01\">\r\n\r\n\t\t\t</select></td>\r\n\t\t</tr>\r\n\t</table>\r\n</div>";
#else
			string originalHtml = "<div>\r\n\t<table cellspacing=\"0\" rules=\"all\" border=\"1\" id=\"gridView\" style=\"border-collapse:collapse;\">\r\n\t\t<tr>\r\n\t\t\t<th scope=\"col\">&nbsp;</th>\r\n\t\t</tr><tr>\r\n\t\t\t<td><input id=\"gridView_ctl02_ctl00\" type=\"checkbox\" name=\"gridView$ctl02$ctl00\" /><select size=\"4\" name=\"gridView$ctl02$ctl01\">\r\n\r\n\t\t\t</select></td>\r\n\t\t</tr><tr>\r\n\t\t\t<td><input id=\"gridView_ctl03_ctl00\" type=\"checkbox\" name=\"gridView$ctl03$ctl00\" /><select size=\"4\" name=\"gridView$ctl03$ctl01\">\r\n\r\n\t\t\t</select></td>\r\n\t\t</tr><tr>\r\n\t\t\t<td><input id=\"gridView_ctl04_ctl00\" type=\"checkbox\" name=\"gridView$ctl04$ctl00\" /><select size=\"4\" name=\"gridView$ctl04$ctl01\">\r\n\r\n\t\t\t</select></td>\r\n\t\t</tr>\r\n\t</table>\r\n</div>";
#endif
			WebTest t = new WebTest ("CheckBoxField_Bug595568_7.aspx");		
			string pageHtml = t.Run ();
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (pageHtml);

			HtmlDiff.AssertAreEqual (originalHtml, renderedHtml, "#A1");
		}
		
		[Test]
		public void CheckBoxField_DefaultProperty ()
		{
			PokerCheckBoxField field = new PokerCheckBoxField ();
			Assert.AreEqual ("", field.DataField, "DataField");
			Assert.AreEqual ("", field.Text, "Text");
			Assert.AreEqual (false, field.GetSupportsHtmlEncode, "SupportsHtmlEncode"); 
		}

		[Test]
		public void CheckBoxField_AssignProperty ()
		{
			PokerCheckBoxField field = new PokerCheckBoxField ();
			field.DataField = "test";
			Assert.AreEqual ("test", field.DataField, "DataField");
			field.Text = "test";
			Assert.AreEqual ("test", field.Text, "Text");
		}

		public void CheckBoxField_ExtractValuesFromCell ()
		{
			PokerCheckBoxField field = new PokerCheckBoxField ();
			OrderedDictionary dictionary = new OrderedDictionary ();
			DataControlFieldCell cell = new DataControlFieldCell (null);
			cell.Controls.Add (new CheckBox ());
			field.ExtractValuesFromCell (dictionary, cell, DataControlRowState.Normal, true);
			Assert.AreEqual (1, dictionary.Count, "ExtractValuesFromCellCount#1");
			Assert.AreEqual ("False", dictionary[0].ToString (), "ExtractValuesFromCellValueFalse");
			CheckBox cb = new CheckBox ();
			cb.Checked = true;
			cell.Controls.Clear ();
			cell.Controls.Add (cb);
			field.ExtractValuesFromCell (dictionary, cell, DataControlRowState.Normal, true);
			Assert.AreEqual (1, dictionary.Count, "ExtractValuesFromCellCount#2");
			Assert.AreEqual ("True", dictionary[0].ToString (), "ExtractValuesFromCellValueTrue");
		}

		[Test]
		public void CheckBoxField_ValidateSupportsCallback ()
		{
			PokerCheckBoxField field = new PokerCheckBoxField ();
			field.ValidateSupportsCallback ();
		}

		[Test]
		public void CheckBoxField_CopyProperties()
		{
			PokerCheckBoxField field = new PokerCheckBoxField ();
			CheckBoxField copy = new CheckBoxField();
			field.DataField = "test";
			field.Text = "test";
			field.DoCopyProperties (copy);
			Assert.AreEqual ("test", copy.Text, "Text");
			Assert.AreEqual ("test", copy.DataField, "DataField");
		}

		[Test]
		public void CheckBoxField_CreateField ()
		{
			PokerCheckBoxField field = new PokerCheckBoxField ();
			CheckBoxField blank = (CheckBoxField)field.DoCreateField ();
			Assert.IsNotNull (blank, "CreateField");
		}

		[Test]
		public void CheckBoxField_GetDesignTimeValue ()
		{
			PokerCheckBoxField field = new PokerCheckBoxField ();
			bool result = (bool)field.DoGetDesignTimeValue ();
			Assert.AreEqual (true, result, "GetDesignTimeValue");
		}

		[Test]
		public void CheckBoxField_InitializeDataCell ()
		{
			PokerCheckBoxField field = new PokerCheckBoxField ();
			field.HeaderText = "headertest";
			DataControlFieldCell cell = new DataControlFieldCell (null);
			DataControlRowState state = DataControlRowState.Edit;
			Assert.AreEqual (0, cell.Controls.Count, "InitializeDataCellControlsBeforeInit");
			field.DoInitializeDataCell (cell, state);
			Assert.AreEqual (1, cell.Controls.Count, "InitializeDataCellControlsAfterInit");
			Assert.AreEqual ("headertest", ((CheckBox)cell.Controls[0]).ToolTip, "InitializeDataCellControlsData");

			cell.Controls.Clear ();
			field.DataField = "fake";
			field.Text = "celltext";
			state = DataControlRowState.Normal;
			field.DoInitializeDataCell (cell, state);
			Assert.AreEqual (1, cell.Controls.Count, "InitializeDataCellControlsAfterInit");
			Assert.AreEqual ("celltext", ((CheckBox) cell.Controls[0]).Text, "InitializeDataCellControlsData");
		}

		[Test]
		public void CheckBoxField_OnDataBindField ()
		{
			Page page = new Page ();
			GridView grid = new GridView ();
			page.Controls.Add (grid);
			grid.DataSource = this.CreateDataSource ();
			grid.AutoGenerateColumns = false;
			PokerCheckBoxField field = new PokerCheckBoxField ();
			field.HeaderText = "field_header";
			field.FooterText = "field_footer";
			field.DataField = FIELDNAME;
			grid.Columns.Add (field);
			grid.DataBind ();
			Assert.AreEqual (2, databound, "DataBindField");
			Assert.AreEqual (4, ((Control) grid.Controls[0]).Controls.Count, "DataBindFieldRowCountr");
		}

		[Test]
		[ExpectedException (typeof (HttpException))]
		public void CheckBoxField_OnDataBindFieldException ()
		{
			Page page = new Page ();
			GridView grid = new GridView ();
			page.Controls.Add (grid);
			grid.DataSource = this.CreateDataSource ();
			grid.AutoGenerateColumns = false;
			PokerCheckBoxField field = new PokerCheckBoxField ();
			field.HeaderText = "field_header";
			field.FooterText = "field_footer";
			field.DataField = WRONGFIELD;
			grid.Columns.Add (field);
			grid.DataBind ();
		}

		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void CheckBoxField_GetApplyFormatInEditModeExeption ()
		{
			PokerCheckBoxField field = new PokerCheckBoxField ();
			bool stab = field.ApplyFormatInEditMode;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CheckBoxField_SetApplyFormatInEditModeExeption ()
		{
			PokerCheckBoxField field = new PokerCheckBoxField ();
			field.ApplyFormatInEditMode = true;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CheckBoxField_GetConvertEmptyStringToNull ()
		{
			PokerCheckBoxField field = new PokerCheckBoxField ();
			bool stab = field.ConvertEmptyStringToNull;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CheckBoxField_SetConvertEmptyStringToNull ()
		{
			PokerCheckBoxField field = new PokerCheckBoxField ();
			field.ConvertEmptyStringToNull = true;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CheckBoxField_SetDataFormatString ()
		{
			PokerCheckBoxField field = new PokerCheckBoxField ();
			field.DataFormatString = "";
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CheckBoxField_GetDataFormatString ()
		{
			PokerCheckBoxField field = new PokerCheckBoxField ();
			string res = field.DataFormatString;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CheckBoxField_SetHtmlEncode ()
		{
			PokerCheckBoxField field = new PokerCheckBoxField ();
			field.HtmlEncode = true;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CheckBoxField_GetHtmlEncode ()
		{
			PokerCheckBoxField field = new PokerCheckBoxField ();
			bool res = field.HtmlEncode;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CheckBoxField_SetNullDisplayText ()
		{
			PokerCheckBoxField field = new PokerCheckBoxField ();
			field.NullDisplayText = "";
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CheckBoxField_GetNullDisplayText ()
		{
			PokerCheckBoxField field = new PokerCheckBoxField ();
			string res = field.NullDisplayText;
		}

		public  DataTable CreateDataSource ()
		{
			DataTable aTable = new DataTable ("A");
			DataColumn dtCol;
			DataRow dtRow;
			// Create ID column and add to the DataTable.
			dtCol = new DataColumn ();
			dtCol.DataType = Type.GetType ("System.Boolean");
			dtCol.ColumnName = FIELDNAME;
			dtCol.Caption = FIELDNAME;
			dtCol.ReadOnly = true;

			// Add the column to the DataColumnCollection.
			aTable.Columns.Add (dtCol);


			dtCol = new DataColumn ();
			dtCol.DataType = Type.GetType ("System.String");
			dtCol.ColumnName = WRONGFIELD;
			dtCol.Caption = WRONGFIELD;
			dtCol.ReadOnly = true;
			

			// Add the column to the DataColumnCollection.
			aTable.Columns.Add (dtCol);

			// Create 2 rows to the table
			dtRow = aTable.NewRow ();
			dtRow[FIELDNAME] = true;
			dtRow[WRONGFIELD] = "1";
			aTable.Rows.Add (dtRow);

			dtRow = aTable.NewRow ();
			dtRow[FIELDNAME] = false;
			dtRow[WRONGFIELD] = "1";
			aTable.Rows.Add (dtRow);
			return aTable;
		}
	}
}
#endif
