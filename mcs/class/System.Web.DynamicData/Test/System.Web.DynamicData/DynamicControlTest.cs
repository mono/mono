//
// DynamicControlTest.cs
//
// Authors:
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2009 Novell Inc. http://novell.com
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web;
using System.Web.UI;
using System.Web.DynamicData;
using System.Web.DynamicData.ModelProviders;
using System.Web.Routing;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.IO;

using NUnit.Framework;
using MonoTests.stand_alone.WebHarness;
using MonoTests.SystemWeb.Framework;
using MonoTests.Common;
using MonoTests.DataSource;
using MonoTests.DataObjects;

using MetaModel = System.Web.DynamicData.MetaModel;
using MetaTable = System.Web.DynamicData.MetaTable;

namespace MonoTests.System.Web.DynamicData
{
	[TestFixture]
	public class DynamicControlTest
	{
		[SetUp]
		public void PerTestSetUp ()
		{
			// This is ran before every test
			FieldTemplatePathTables.CleanUp_FullTypeNameTemplates ();
			FieldTemplatePathTables.CleanUp_ShortTypeNameTemplates ();
		}

		[TestFixtureSetUp]
		public void SetUp ()
		{
			Type type = GetType ();
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_01.aspx", "ListView_DynamicControl_01.aspx");
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_01.aspx.cs", "ListView_DynamicControl_01.aspx.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_02.aspx", "ListView_DynamicControl_02.aspx");
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_02.aspx.cs", "ListView_DynamicControl_02.aspx.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_03.aspx", "ListView_DynamicControl_03.aspx");
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_03.aspx.cs", "ListView_DynamicControl_03.aspx.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_04.aspx", "ListView_DynamicControl_04.aspx");
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_04.aspx.cs", "ListView_DynamicControl_04.aspx.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_05.aspx", "ListView_DynamicControl_05.aspx");
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_05.aspx.cs", "ListView_DynamicControl_05.aspx.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_06.aspx", "ListView_DynamicControl_06.aspx");
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_06.aspx.cs", "ListView_DynamicControl_06.aspx.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_07.aspx", "ListView_DynamicControl_07.aspx");
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_07.aspx.cs", "ListView_DynamicControl_07.aspx.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_08.aspx", "ListView_DynamicControl_08.aspx");
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_08.aspx.cs", "ListView_DynamicControl_08.aspx.cs");
		}

		[TestFixtureTearDown]
		public void TearDown ()
		{
			FieldTemplatePathTables.CleanUp_FullTypeNameTemplates ();
			FieldTemplatePathTables.CleanUp_ShortTypeNameTemplates ();
			WebTest.Unload ();
		}

		[Test]
		public void Defaults ()
		{
			var dc = new DynamicControl ();

			Assert.AreEqual (false, dc.ApplyFormatInEditMode, "#A1");
			Assert.AreEqual (null, dc.Column, "#A2");
			Assert.AreEqual (false, dc.ConvertEmptyStringToNull, "#A3");
			Assert.AreEqual (String.Empty, dc.CssClass, "#A4");
			Assert.AreEqual (String.Empty, dc.DataField, "#A5");
			Assert.AreEqual (String.Empty, dc.DataFormatString, "#A6");
			Assert.AreEqual (null, dc.FieldTemplate, "#A7");
			Assert.AreEqual (true, dc.HtmlEncode, "#A8");
			Assert.AreEqual (dc, ((IFieldTemplateHost)dc).FormattingOptions, "#A9");
			Assert.AreEqual (DataBoundControlMode.ReadOnly, dc.Mode, "#A10");
			Assert.AreEqual (String.Empty, dc.NullDisplayText, "#A11");
			// Throws NREX on .NET .... (why am I still surprised by this?)
			// Calls DynamicDataExtensions.FindMetaTable which is where the exception is thrown from
			// Assert.AreEqual (null, dc.Table, "#A12");
			Assert.AreEqual (String.Empty, dc.UIHint, "#A13");
			Assert.AreEqual (String.Empty, dc.ValidationGroup, "#A14");
		}

		[Test]
		public void ApplyFormatInEditMode ()
		{
			var test = new WebTest ("ListView_DynamicControl_01.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (ApplyFormatInEditMode_OnLoad);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void ApplyFormatInEditMode_OnLoad (Page p)
		{
			var lc = p.FindControl ("ListView1") as ListView;
			Assert.IsNotNull (lc, "#A1");

			var dc = lc.FindChild<DynamicControl> ("FirstName");
			Assert.IsNotNull (dc, "#B1-1");
			Assert.AreEqual ("FirstName", dc.ID, "#B1-2");
			Assert.AreEqual (false, dc.Column.ApplyFormatInEditMode, "#B1-3");
			Assert.AreEqual (false, dc.ApplyFormatInEditMode, "#B1-4");

			dc = lc.FindChild<DynamicControl> ("Active");
			Assert.IsNotNull (dc, "#C1");
			Assert.AreEqual ("Active", dc.ID, "#C1-1");
			Assert.AreEqual (true, dc.Column.ApplyFormatInEditMode, "#C1-2");
			Assert.AreEqual (true, dc.ApplyFormatInEditMode, "#C1-3");

			dc.ApplyFormatInEditMode = false;
			Assert.AreEqual (false, dc.ApplyFormatInEditMode, "#C1-4");
			Assert.AreEqual (true, dc.Column.ApplyFormatInEditMode, "#C1-5");
		}

		[Test]
		public void Column ()
		{
			var test = new WebTest ("ListView_DynamicControl_01.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (Column_OnLoad);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void Column_OnLoad (Page p)
		{
			var lc = p.FindControl ("ListView1") as ListView;
			Assert.IsNotNull (lc, "#A1");

			var dc = lc.FindChild<DynamicControl> ("FirstName");
			Assert.IsNotNull (dc, "#A1-1");
			Assert.IsNotNull (dc.Column, "#B1");

			// Safe not to check for GetModel's return value - it throws if model isn't found, same
			// goes for GetTable and GetColumn
			MetaTable table = MetaModel.GetModel (typeof (EmployeesDataContext)).GetTable ("EmployeeTable");
			MetaColumn column = table.GetColumn ("FirstName");
			Assert.AreEqual (column, dc.Column, "#B1-1");
			Assert.AreEqual (dc.Column.Table, dc.Table, "#B1-2");

			dc.Column = column;
			Assert.AreEqual (column, dc.Column, "#C1-3");

			column = table.GetColumn ("Active");
			dc.Column = column;
			Assert.AreEqual (column, dc.Column, "#C1-4");

			// Talk about consistency...
			table = MetaModel.GetModel (typeof (EmployeesDataContext)).GetTable ("SeasonalEmployeeTable");
			column = table.GetColumn ("FirstName");
			dc.Column = column;

			Assert.AreNotEqual (dc.Column.Table, dc.Table, "#C1-5");
		}

		[Test]
		public void ConvertEmptyStringToNull ()
		{
			var test = new WebTest ("ListView_DynamicControl_01.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (ConvertEmptyStringToNull_OnLoad);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void ConvertEmptyStringToNull_OnLoad (Page p)
		{
			var lc = p.FindControl ("ListView1") as ListView;
			Assert.IsNotNull (lc, "#A1");

			var dc = lc.FindChild<DynamicControl> ("FirstName");
			Assert.IsNotNull (dc, "#B1-1");
			Assert.AreEqual ("FirstName", dc.ID, "#B1-2");
			Assert.AreEqual (true, dc.Column.ConvertEmptyStringToNull, "#B1-3");
			Assert.AreEqual (true, dc.ConvertEmptyStringToNull, "#B1-4");

			dc = lc.FindChild<DynamicControl> ("LastName");
			Assert.IsNotNull (dc, "#C1");
			Assert.AreEqual (true, dc.ConvertEmptyStringToNull, "#C1-1");

			dc.ConvertEmptyStringToNull = false;
			Assert.AreEqual (false, dc.ConvertEmptyStringToNull, "#C1-2");
			Assert.AreEqual (true, dc.Column.ConvertEmptyStringToNull, "#C1-3");
		}

		[Test]
		public void CssClass ()
		{
			var dc = new DynamicControl ();
			dc.CssClass = "MyCssClass";
			Assert.AreEqual ("MyCssClass", dc.CssClass, "#A1");

			var test = new WebTest ("ListView_DynamicControl_01.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (CssClass_OnLoad);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");

			string html = @"<span class=""activeCssClass"">

<span class=""field"">Active</span>:";
			Assert.IsTrue (p.IndexOf (html.Replace ("\r\n", "\n")) != -1, "#Y1");
		}

		static void CssClass_OnLoad (Page p)
		{
			var lc = p.FindControl ("ListView1") as ListView;
			Assert.IsNotNull (lc, "#B1");

			var dc = lc.FindChild<PokerDynamicControl> ("Active");
			Assert.IsNotNull (dc, "#C1");
			Assert.AreEqual ("Active", dc.DataField, "#C1-1");
			Assert.AreEqual ("activeCssClass", dc.CssClass, "#C1-2");
		}

		[Test]
		public void DataField ()
		{
			var dc = new DynamicControl ();

			Assert.AreEqual (String.Empty, dc.DataField, "#A1");
			dc.DataField = "MyField";
			Assert.AreEqual ("MyField", dc.DataField, "#A2");

			dc.DataField = "AnotherField";
			Assert.AreEqual ("AnotherField", dc.DataField, "#A3");

			dc.DataField = String.Empty;
			Assert.AreEqual (String.Empty, dc.DataField, "#A4");

			dc.DataField = null;
			Assert.AreEqual (String.Empty, dc.DataField, "#A5");
		}

		[Test]
		public void DataField_1 ()
		{
			var test = new WebTest ("ListView_DynamicControl_01.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (DataField_OnLoad_1);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void DataField_OnLoad_1 (Page p)
		{
			var lc = p.FindControl ("ListView1") as ListView;
			Assert.IsNotNull (lc, "#A1");

			var dc = lc.FindChild<DynamicControl> ("FirstName");
			Assert.IsNotNull (dc, "#A1-1");
			Assert.IsNotNull (dc.DataField, "#A1-2");
			Assert.AreEqual ("FirstName", dc.DataField, "#A1-3");

			// Column and Table aren't set on DataField assignment...
			dc.DataField = "Active";
			Assert.AreEqual ("Active", dc.DataField, "#B1");
			Assert.AreEqual ("FirstName", dc.Column.Name, "#B1-1");

			dc.DataField = String.Empty;
			Assert.AreEqual (String.Empty, dc.DataField, "#C1");
			Assert.AreEqual ("FirstName", dc.Column.Name, "#C1-1");

			dc.DataField = null;
			Assert.AreEqual (String.Empty, dc.DataField, "#D1");
			Assert.AreEqual ("FirstName", dc.Column.Name, "#D1-1");
		}

		[Test]
		public void DataField_2 ()
		{
			var dc = new DynamicControl ();

			dc.DataField = null;
			Assert.AreEqual (String.Empty, dc.DataField, "#A1");

			var c = dc.Column;
			Assert.IsNull (c, "#A1-1");

			dc.DataField = "MyField";
			Assert.AreEqual ("MyField", dc.DataField, "#B1");

			c = dc.Column;
			Assert.IsNull (c, "#B1-1");
		}

		[Test]
		public void DataField_3 ()
		{
			var test = new WebTest ("ListView_DynamicControl_05.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (DataField_OnLoad_3);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void DataField_OnLoad_3 (Page p)
		{
			var lc = p.FindControl ("ListView1") as ListView;
			Assert.IsNotNull (lc, "#A1");

			// System.InvalidOperationException : System.InvalidOperationException: The 'PokerDynamicControl' control 'FirstName' must have a DataField attribute.
			//   at System.Web.DynamicData.DynamicControl.ResolveColumn()
			//   at System.Web.DynamicData.DynamicControl.OnInit(EventArgs e)

			Assert.Throws<InvalidOperationException> (() => {
				var c = lc.FindChild<DynamicControl> ("FirstName");
			}, "#A1");
		}

		[Test]
		public void DataField_4 ()
		{
			var test = new WebTest ("ListView_DynamicControl_06.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (DataField_OnLoad_4);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void DataField_OnLoad_4 (Page p)
		{
			var lc = p.FindControl ("ListView1") as ListView;
			Assert.IsNotNull (lc, "#A1");

			// System.InvalidOperationException : System.InvalidOperationException: The table 'EmployeeTable' does not have a column named 'NoSuchColumn'.
			//   at System.Web.DynamicData.MetaTable.GetColumn(String columnName)
			//   at System.Web.DynamicData.DynamicControl.ResolveColumn()
			//   at System.Web.DynamicData.DynamicControl.OnInit(EventArgs e)

			Assert.Throws<InvalidOperationException> (() => {
				var dc = lc.FindChild<DynamicControl> ("FirstName");
			}, "#A1");
		}

		[Test]
		public void DataField_5 ()
		{
			var test = new WebTest ("ListView_DynamicControl_07.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (DataField_OnLoad_5);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void DataField_OnLoad_5 (Page p)
		{
			var lc = p.FindControl ("ListView1") as ListView;
			Assert.IsNotNull (lc, "#A1");

			// If Column is intialized before OnInit is run, the column is not resolved - no exception here.
			var dc = lc.FindChild<DynamicControl> ("FirstName");
			Assert.IsNotNull (dc, "#A1");
			Assert.AreEqual ("FirstName", dc.Column.Name, "#A1-1");
		}

		[Test]
		public void DataFormatString ()
		{
			var test = new WebTest ("ListView_DynamicControl_01.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (DataFormatString_OnLoad);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void DataFormatString_OnLoad (Page p)
		{
			var lc = p.FindControl ("ListView1") as ListView;
			Assert.IsNotNull (lc, "#A1");

			var dc = lc.FindChild<DynamicControl> ("FirstName");
			Assert.IsNotNull (dc, "#B1-1");
			Assert.AreEqual ("FirstName", dc.ID, "#B1-2");
			Assert.AreEqual (String.Empty, dc.Column.DataFormatString, "#B1-3");
			Assert.AreEqual (String.Empty, dc.DataFormatString, "#B1-4");

			dc = lc.FindChild<DynamicControl> ("Active");
			Assert.IsNotNull (dc, "#C1");
			Assert.AreEqual ("Active", dc.ID, "#C1-1");
			Assert.AreEqual ("Boolean value: {0}", dc.Column.DataFormatString, "#C1-2");
			Assert.AreEqual ("Boolean value: {0}", dc.DataFormatString, "#C1-3");

			dc.DataFormatString = String.Empty;
			Assert.AreEqual (String.Empty, dc.DataFormatString, "#C1-4");
			Assert.AreEqual ("Boolean value: {0}", dc.Column.DataFormatString, "#C1-5");
		}

		[Test]
		public void FieldTemplate ()
		{
			var test = new WebTest ("ListView_DynamicControl_03.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (FieldTemplate_OnLoad);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void FieldTemplate_OnLoad (Page p)
		{
			var lc = p.FindControl ("ListView3") as ListView;
			Assert.IsNotNull (lc, "#A1");

			int counter = 1;
			foreach (var entry in FieldTemplatePathTables.FieldTemplateReadOnlyColumns) {
				string columnName = entry.ColumnName;
				var dc = lc.FindChild<PokerDynamicControl> (columnName);
				Assert.IsNotNull (dc, String.Format ("#B{0}-1 ({1}", counter, columnName));
				Assert.AreEqual (columnName, dc.ID, String.Format ("#B{0}-2 ({1})", counter, columnName));

				var templateControl = dc.FieldTemplate;
				var template = templateControl as FieldTemplateUserControl;
				if (entry.IsNull) {
					Assert.IsNull (templateControl, String.Format ("#B{0}-3 ({1})", counter, columnName));
					Assert.IsNull (template, String.Format ("#B{0}-4 ({1})", counter, columnName));
				} else {
					Assert.IsNotNull (templateControl, String.Format ("#B{0}-5 ({1})", counter, columnName));
					Assert.IsNotNull (template, String.Format ("#B{0}-6 ({1})", counter, columnName));
					Assert.AreEqual (entry.ControlVirtualPath, template.AppRelativeVirtualPath, String.Format ("#B{0}-7 ({1})", counter, columnName));
				}

				counter++;
			}
		}

		[Test]
		public void FieldTemplate_1 ()
		{
			var test = new WebTest ("ListView_DynamicControl_04.aspx");
			var p = test.Run ();

			// Fake post-back
			var delegates = new PageDelegates ();
			delegates.PreRenderComplete = FieldTemplate_OnPreRenderComplete_1;
			test.Invoker = new PageInvoker (delegates);
			var fr = new FormRequest (test.Response, "form1");
#if TARGET_DOTNET
			fr.Controls.Add ("ListView4$ctrl0$editMe");
			fr.Controls["ListView4$ctrl0$editMe"].Value = "Edit";
#else
			fr.Controls.Add ("ListView4$ctl01$editMe");
			fr.Controls["ListView4$ctl01$editMe"].Value = "Edit";
#endif
			test.Request = fr;
			p = test.Run ();

			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void FieldTemplate_OnPreRenderComplete_1 (Page p)
		{
			var lc = p.FindControl ("ListView4") as ListView;
			Assert.IsNotNull (lc, "#A1");

			int counter = 1;
			foreach (var entry in FieldTemplatePathTables.FieldTemplateEditColumns) {
				string columnName = entry.ColumnName;
				var dc = lc.FindChild<PokerDynamicControl> (columnName);
				Assert.IsNotNull (dc, String.Format ("#B{0}-1 ({1})", counter, columnName));
				Assert.AreEqual (columnName, dc.ID, String.Format ("#B{0}-2 ({1})", counter, columnName));

				var templateControl = dc.FieldTemplate;
				var template = templateControl as FieldTemplateUserControl;
				if (entry.IsNull) {
					Assert.IsNull (templateControl, String.Format ("#B{0}-3 ({1})", counter, columnName));
					Assert.IsNull (template, String.Format ("#B{0}-4 ({1})", counter, columnName));
				} else {
					Assert.IsNotNull (templateControl, String.Format ("#B{0}-5 ({1})", counter, columnName));
					Assert.IsNotNull (template, String.Format ("#B{0}-6 ({1})", counter, columnName));
					Assert.AreEqual (entry.ControlVirtualPath, template.AppRelativeVirtualPath, String.Format ("#B{0}-7 ({1})", counter, columnName));
				}

				counter++;
			}
		}

		// This tests full type name templates
		[Test]
		public void FieldTemplate_2 ()
		{
			FieldTemplatePathTables.SetUp_FullTypeNameTemplates (this);
			var test = new WebTest ("ListView_DynamicControl_03.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (FieldTemplate_OnLoad_2);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");

		}

		static void FieldTemplate_OnLoad_2 (Page p)
		{
			var lc = p.FindControl ("ListView3") as ListView;
			Assert.IsNotNull (lc, "#A1");

			int counter = 1;
			foreach (var entry in FieldTemplatePathTables.FieldTemplateNonDefaultColumns) {
				string columnName = entry.ColumnName;
				var dc = lc.FindChild<PokerDynamicControl> (columnName);
				Assert.IsNotNull (dc, String.Format ("#B{0}-1 ({1})", counter, columnName));
				Assert.AreEqual (columnName, dc.ID, String.Format ("#B{0}-2 ({1})", counter, columnName));

				var templateControl = dc.FieldTemplate;
				var template = templateControl as FieldTemplateUserControl;
				if (entry.IsNull) {
					Assert.IsNull (templateControl, String.Format ("#B{0}-3 ({1})", counter, columnName));
					Assert.IsNull (template, String.Format ("#B{0}-4 ({1})", counter, columnName));
				} else {
					Assert.IsNotNull (templateControl, String.Format ("#B{0}-5 ({1})", counter, columnName));
					Assert.IsNotNull (template, String.Format ("#B{0}-6 ({1})", counter, columnName));
					Assert.AreEqual (entry.ControlVirtualPath, template.AppRelativeVirtualPath, String.Format ("#B{0}-7 ({1})", counter, columnName));
				}

				counter++;
			}
		}

		// This tests short type name templates
		[Test]
		public void FieldTemplate_3 ()
		{
			try {
				FieldTemplatePathTables.SetUp_ShortTypeNameTemplates (this);
				var test = new WebTest ("ListView_DynamicControl_03.aspx");
				test.Invoker = PageInvoker.CreateOnLoad (FieldTemplate_OnLoad_3);
				var p = test.Run ();
				Assert.IsNotNull (test.Response, "#X1");
				Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
				Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
				Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
			} finally {

			}
		}

		static void FieldTemplate_OnLoad_3 (Page p)
		{
			var lc = p.FindControl ("ListView3") as ListView;
			Assert.IsNotNull (lc, "#A1");

			int counter = 1;
			foreach (var entry in FieldTemplatePathTables.FieldTemplateNonDefaultShortColumns) {
				string columnName = entry.ColumnName;
				var dc = lc.FindChild<PokerDynamicControl> (columnName);
				Assert.IsNotNull (dc, String.Format ("#B{0}-1 ({1})", counter, columnName));
				Assert.AreEqual (columnName, dc.ID, String.Format ("#B{0}-2 ({1})", counter, columnName));

				var templateControl = dc.FieldTemplate;
				var template = templateControl as FieldTemplateUserControl;
				if (entry.IsNull) {
					Assert.IsNull (templateControl, String.Format ("#B{0}-3 ({1})", counter, columnName));
					Assert.IsNull (template, String.Format ("#B{0}-4 ({1})", counter, columnName));
				} else {
					Assert.IsNotNull (templateControl, String.Format ("#B{0}-5 ({1})", counter, columnName));
					Assert.IsNotNull (template, String.Format ("#B{0}-6 ({1})", counter, columnName));
					Assert.AreEqual (entry.ControlVirtualPath, template.AppRelativeVirtualPath, String.Format ("#B{0}-7 ({1})", counter, columnName));
				}

				counter++;
			}
		}

		[Test]
		public void GetAttribute ()
		{
			var dc = new DynamicControl ();
			Assert.AreEqual (String.Empty, dc.GetAttribute ("NoSuchAttribute"), "#A1");

			dc.SetAttribute ("MyAttribute", "value");
			Assert.AreEqual ("value", dc.GetAttribute ("MyAttribute"), "#B1");

			// Nice...
			Assert.Throws<KeyNotFoundException> (() => {
				dc.GetAttribute ("NoSuchAttribute");
			}, "#C1");
		}

		[Test]
		public void HtmlEncode ()
		{
			var test = new WebTest ("ListView_DynamicControl_01.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (HtmlEncode_OnLoad);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void HtmlEncode_OnLoad (Page p)
		{
			var lc = p.FindControl ("ListView1") as ListView;
			Assert.IsNotNull (lc, "#A1");

			var dc = lc.FindChild<DynamicControl> ("FirstName");
			Assert.IsNotNull (dc, "#A1-1");
			Assert.IsNotNull (dc.Column, "#A1-2");

			Assert.AreEqual (true, dc.HtmlEncode, "#B1");
			Assert.AreEqual (dc.HtmlEncode, dc.Column.HtmlEncode, "#B1-1");

			dc.HtmlEncode = false;
			Assert.AreEqual (false, dc.HtmlEncode, "#C1");
			Assert.AreNotEqual (dc.HtmlEncode, dc.Column.HtmlEncode, "#C1-1");
		}

		[Test]
		public void IFieldTemplateHost_FormattingOptions ()
		{
			var dc = new DynamicControl ();

			Assert.IsNotNull (((IFieldTemplateHost)dc).FormattingOptions, "#A1");
			Assert.AreEqual (dc, ((IFieldTemplateHost)dc).FormattingOptions, "#A2");
		}

		[Test]
		public void Mode ()
		{
			var dc = new DynamicControl (DataBoundControlMode.Edit);
			Assert.AreEqual (DataBoundControlMode.Edit, dc.Mode, "#A1");

			dc.Mode = DataBoundControlMode.Insert;
			Assert.AreEqual (DataBoundControlMode.Insert, dc.Mode, "#A2");

			dc.Mode = DataBoundControlMode.ReadOnly;
			Assert.AreEqual (DataBoundControlMode.ReadOnly, dc.Mode, "#A3");
		}

		[Test]
		public void NullDisplayText ()
		{
			var test = new WebTest ("ListView_DynamicControl_01.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (NullDisplayText_OnLoad);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void NullDisplayText_OnLoad (Page p)
		{
			var lc = p.FindControl ("ListView1") as ListView;
			Assert.IsNotNull (lc, "#A1");

			var dc = lc.FindChild<DynamicControl> ("FirstName");
			Assert.IsNotNull (dc, "#A1-1");
			Assert.IsNotNull (dc.Column, "#A1-2");

			Assert.AreEqual (String.Empty, dc.NullDisplayText, "#B1");
			Assert.AreEqual (dc.NullDisplayText, dc.Column.NullDisplayText, "#B1-1");

			dc.NullDisplayText = "Text";
			Assert.AreEqual ("Text", dc.NullDisplayText, "#C1");
			Assert.AreNotEqual (dc.NullDisplayText, dc.Column.NullDisplayText, "#C1-1");

			dc = lc.FindChild<DynamicControl> ("LastName");
			Assert.IsNotNull (dc, "#D1");
			Assert.IsNotNull (dc.Column, "#D1-1");
			Assert.AreEqual ("No value for this column", dc.NullDisplayText, "#D1-2");
			Assert.AreEqual (dc.NullDisplayText, dc.Column.NullDisplayText, "#D1-3");

			dc.NullDisplayText = String.Empty;
			Assert.AreEqual (String.Empty, dc.NullDisplayText, "#E1");
			Assert.AreNotEqual (dc.NullDisplayText, dc.Column.NullDisplayText, "#E1-1");
		}

		[Test]
		public void SetAttribute ()
		{
			var dc = new PokerDynamicControl ();

			string html = dc.RenderToString ();
			Assert.IsNotNull (html, "#A1");

			dc.SetAttribute ("MyAttribute", "Value");
			html = dc.RenderToString ();
			Assert.IsNotNull (html, "#B1");
			Assert.AreEqual ("Value", dc.GetAttribute ("MyAttribute"), "#B2");

			dc.SetAttribute ("MyAttribute", "Another value");
			Assert.AreEqual ("Another value", dc.GetAttribute ("MyAttribute"), "#C1");
		}

		[Test]
		public void SetAttribute_1 ()
		{
			var test = new WebTest ("ListView_DynamicControl_01.aspx");
			var delegates = new PageDelegates ();
			delegates.PreRenderComplete = SetAttribute_OnPreRenderComplete_1;
			test.Invoker = new PageInvoker (delegates);

			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void SetAttribute_OnPreRenderComplete_1 (Page p)
		{
			// TODO: figure out how the attributes are used. They aren't rendered along with the control,
			// but obviously they must be used somewhere...
			var lc = p.FindControl ("ListView1") as ListView;
			Assert.IsNotNull (lc, "#A1");

			var dc = lc.FindChild<PokerDynamicControl> ("FirstName");
			Assert.IsNotNull (dc, "#A1-1");

			string html = dc.RenderToString ();
			Assert.IsFalse (String.IsNullOrEmpty (html), "#B1");

			dc.SetAttribute ("MyAttribute", "value");
			html = dc.RenderToString ();
			Assert.IsFalse (String.IsNullOrEmpty (html), "#C1");
		}

		[Test]
		public void Table ()
		{
			var test = new WebTest ("ListView_DynamicControl_01.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (Table_OnLoad);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void Table_OnLoad (Page p)
		{
			var lc = p.FindControl ("ListView1") as ListView;
			Assert.IsNotNull (lc, "#A1");

			var dc = lc.FindChild<DynamicControl> ("FirstName");
			Assert.IsNotNull (dc, "#A1-1");
			Assert.IsNotNull (dc.Table, "#B1");

			// Safe not to check for GetModel's return value - it throws if model isn't found, same
			// goes for GetTable
			MetaTable table = MetaModel.GetModel (typeof (EmployeesDataContext)).GetTable ("EmployeeTable");
			Assert.AreEqual (table, dc.Table, "#B1-1");
			Assert.AreEqual (dc.Table, dc.Column.Table, "#B1-2");
		}

		[Test]
		public void UIHint ()
		{
			var test = new WebTest ("ListView_DynamicControl_01.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (UIHint_OnLoad);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");

			Assert.IsTrue (p.IndexOf ("<span class=\"field\">LastName</span>: <span class=\"customFieldTemplate\">") != -1, "#Y1");
			Assert.IsTrue (p.IndexOf ("<span class=\"field\">FirstName</span>: <span class=\"defaultTemplate\">") != -1, "#Y1-1");
		}

		static void UIHint_OnLoad (Page p)
		{
			var lc = p.FindControl ("ListView1") as ListView;
			Assert.IsNotNull (lc, "#A1");

			var dc = lc.FindChild<DynamicControl> ("FirstName");
			Assert.IsNotNull (dc, "#B1");
			Assert.AreEqual ("FirstName", dc.DataField, "#B1-1");

			// Changes below won't affect rendering - we're being called too late in the process
			// This is just to test if the property is settable, what are the defaults and whether
			// they can be overriden

			// No UIHint attribute on the associated field, no explicit setting
			Assert.AreEqual (String.Empty, dc.UIHint, "#C1");
			dc.UIHint = "MyCustomUIHintTemplate_Text";
			Assert.AreEqual ("MyCustomUIHintTemplate_Text", dc.UIHint, "#C1-1");

			dc = lc.FindChild<DynamicControl> ("LastName");
			Assert.IsNotNull (dc, "#D1");
			Assert.AreEqual ("LastName", dc.DataField, "#D1-1");

			// UIHint attribute found on the associated field
			Assert.AreEqual ("CustomFieldTemplate", dc.UIHint, "#D1-2");
			dc.UIHint = "MyCustomUIHintTemplate_Text";
			Assert.AreEqual ("MyCustomUIHintTemplate_Text", dc.UIHint, "#D1-3");

			dc.UIHint = null;
			Assert.AreEqual (String.Empty, dc.UIHint, "#E1");
		}

		[Test]
		public void UIHint_1 ()
		{
			var test = new WebTest ("ListView_DynamicControl_02.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (UIHint_OnLoad_1);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");

			Assert.IsTrue (p.IndexOf ("<span class=\"field\">LastName</span>: <span class=\"myCustomUIHintTemplate_Text\">") != -1, "#Y1");
			Assert.IsTrue (p.IndexOf ("<span class=\"field\">FirstName</span>: <span class=\"defaultTemplate\">") != -1, "#Y1-1");
		}

		static void UIHint_OnLoad_1 (Page p)
		{
			var lc = p.FindControl ("ListView2") as ListView;
			Assert.IsNotNull (lc, "#A1");

			var dc = lc.FindChild<DynamicControl> ("FirstName2");
			Assert.IsNotNull (dc, "#B1");
			Assert.AreEqual ("FirstName", dc.DataField, "#B1-1");

			// No UIHint attribute on the associated field, no explicit setting
			Assert.AreEqual (String.Empty, dc.UIHint, "#C1");

			dc = lc.FindChild<DynamicControl> ("LastName2");
			Assert.IsNotNull (dc, "#D1");
			Assert.AreEqual ("LastName", dc.DataField, "#D1-1");

			// UIHint attribute found on the associated field but overriden in the page
			Assert.AreEqual ("MyCustomUIHintTemplate_Text", dc.UIHint, "#D1-2");
		}

		[Test]
		public void UIHint_2 ()
		{
			var test = new WebTest ("ListView_DynamicControl_08.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (UIHint_OnLoad_2);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");

			Assert.IsTrue (p.IndexOf ("<span class=\"field\">LastName</span>: <span class=\"customFieldTemplate\">") != -1, "#Y1");
			Assert.IsTrue (p.IndexOf ("<span class=\"field\">FirstName</span>: <span class=\"defaultTemplate\">") != -1, "#Y1-1");
		}

		static void UIHint_OnLoad_2 (Page p)
		{
			var lc = p.FindControl ("ListView1") as ListView;
			Assert.IsNotNull (lc, "#A1");

			// Invalid field template name
			var dc = lc.FindChild<DynamicControl> ("FirstName");
			Assert.IsNotNull (dc, "#B1");
			Assert.AreEqual ("NonExistingTemplate", dc.UIHint, "#B1-1");

			// Falls back to the text template
			Assert.IsNotNull (dc.FieldTemplate, "#C1");

			var ftuc = dc.FieldTemplate as FieldTemplateUserControl;
			Assert.IsNotNull (ftuc, "#C1-2");
			Assert.AreEqual ("~/DynamicData/FieldTemplates/Text.ascx", ftuc.AppRelativeVirtualPath, "#C1-3");
		}

		[Test]
		public void ValidationGroup ()
		{
			// TODO: more complicated tests involving actual page and validation
			var dc = new DynamicControl ();
			dc.ValidationGroup = "Group1";
			Assert.AreEqual ("Group1", dc.ValidationGroup, "#A1");
		}
	}
}
