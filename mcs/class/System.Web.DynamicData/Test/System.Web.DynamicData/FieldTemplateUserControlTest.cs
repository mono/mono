//
// FieldTemplateUserControlTest.cs
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
using MonoTests.ModelProviders;

using MetaModel = System.Web.DynamicData.MetaModel;
using MetaTable = System.Web.DynamicData.MetaTable;

namespace MonoTests.System.Web.DynamicData
{
	[TestFixture]
	public class FieldTemplateUserControlTest
	{
		DynamicDataContainerModelProvider<TestDataContext> dynamicModelProvider;

		[TestFixtureSetUp]
		public void SetUp ()
		{
			Type type = GetType ();
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_01.aspx", "ListView_DynamicControl_01.aspx");
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_01.aspx.cs", "ListView_DynamicControl_01.aspx.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_10.aspx", "ListView_DynamicControl_10.aspx");
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_10.aspx.cs", "ListView_DynamicControl_10.aspx.cs");

			dynamicModelProvider = new DynamicDataContainerModelProvider<TestDataContext> ();
			Utils.RegisterContext (dynamicModelProvider, new ContextConfiguration () { ScaffoldAllTables = true });
		}

		[TestFixtureTearDown]
		public void TearDown ()
		{
			WebTest.Unload ();
		}

		[Test]
		public void Defaults ()
		{
			var field = new PokerFieldTemplateUserControl ();

			// And for another time - not a single null check in the code...
			//
			// All the asserts commented out below throw a NREX
			//

			//Assert.IsNull (field.ChildrenColumn, "#A1");
			//Assert.IsNull (field.GetChildrenPath (), "#A2");
			//Assert.IsNull (field.Column, "#A3");
			Assert.IsNull (field.DataControl, "#A4");
			//Assert.IsNull (field.FieldValue, "#A5");
			//Assert.IsNull (field.FieldValueEditString, "#A6");
			//Assert.IsNull (field.FieldValueString, "#A7");
			//Assert.IsNull (field.ForeignKeyColumn, "#A8");
			//Assert.IsNull (field.GetForeignKeyPath (), "#A9");
			Assert.IsNull (field.Host, "#A10");
			//Assert.IsNull (field.MetadataAttributes, "#A11");
			//Assert.IsNull (field.Mode, "#A12");
			//Assert.IsNull (field.Row, "#A13");
			//Assert.IsNull (field.Table, "#A14");
		}

		[Test]
		public void Defaults_WithHost ()
		{
			var test = new WebTest ("ListView_DynamicControl_01.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (Defaults_WithHost_OnLoad);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void Defaults_WithHost_OnLoad (Page p)
		{
			// Not many "defaults" here

			var lc = p.FindControl ("ListView1") as ListView;
			Assert.IsNotNull (lc, "#A1");

			var dc = lc.FindChild<DynamicControl> ("FirstName");
			Assert.IsNotNull (dc, "#B1-1");
			Assert.AreEqual ("FirstName", dc.ID, "#B1-2");

			var field = new PokerFieldTemplateUserControl ();

			// Without it Row NREXes... However, with Page set, we still get an exception
			// this time from Page.GetDataItem () as we're not bound to data. So, there is
			// no way to test the "defaults" as if we used a field control retrieved from 
			// DataControl it would already be initialized
			field.Page = p;
			((IFieldTemplate) field).SetHost (dc);

			Assert.Throws<Exception> (() => {
				var f = field.ChildrenColumn;
			}, "#C1");

			// The FirstName column is not a children one
			Assert.Throws<Exception> (() => {
				field.GetChildrenPath ();
			}, "#C2");
			Assert.IsNotNull (field.Column, "#C3");
			Assert.AreEqual (dc.Column, field.Column, "#C3-1");
			Assert.IsNull (field.DataControl, "#C4");

			// The tests below fail with exception:
			//
			// MonoTests.System.Web.DynamicData.FieldTemplateUserControlTest.Defaults_WithHost:
			// System.InvalidOperationException : System.InvalidOperationException: Databinding methods such as Eval(), XPath(), and Bind() can only be used in the context of a databound control.
			//   at System.Web.UI.Page.GetDataItem()
			//   at System.Web.DynamicData.FieldTemplateUserControl.get_Row()
			//   at System.Web.DynamicData.FieldTemplateUserControl.GetColumnValue(MetaColumn column)
			//   at System.Web.DynamicData.FieldTemplateUserControl.get_FieldValue()
			//   at System.Web.DynamicData.FieldTemplateUserControl.get_FieldValueEditString()
			//   at MonoTests.System.Web.DynamicData.FieldTemplateUserControlTest.Defaults_WithHost_OnLoad(Page p)
			//   at MonoTests.SystemWeb.Framework.PageInvoker.Invoke(PageDelegate callback)
			//   at MonoTests.SystemWeb.Framework.PageInvoker.OnLoad(Object sender, EventArgs a)
			//   at System.EventHandler.Invoke(Object sender, EventArgs e)
			//   at System.Web.UI.Control.OnLoad(EventArgs e)
			//   at System.Web.UI.Control.LoadRecursive()
			//   at System.Web.UI.Page.ProcessRequestMain(Boolean includeStagesBeforeAsyncPoint, Boolean includeStagesAfterAsyncPoint)
			//  ----> System.InvalidOperationException : Databinding methods such as Eval(), XPath(), and Bind() can only be used in the context of a databound control.

			//Assert.IsNull (field.FieldValue, "#C5");
			//Assert.IsNull (field.FieldValueEditString, "#C6");
			//Assert.IsNull (field.FieldValueString, "#C7");

			// The FirstName column is not a foreign key one
			Assert.Throws<Exception> (() => {
				var f = field.ForeignKeyColumn;
			}, "#C8");
			Assert.Throws<Exception> (() => {
				var f = field.GetForeignKeyPath ();
			}, "#C9");
			Assert.IsNotNull (field.Host, "#C10");
			Assert.AreEqual (dc, field.Host, "#C10-1");
			Assert.IsNotNull (field.MetadataAttributes, "#C11");
			Assert.AreEqual (dc.Column.Attributes, field.MetadataAttributes, "#C11-1");
			Assert.AreEqual (DataBoundControlMode.ReadOnly, field.Mode, "#C12");
			Assert.AreEqual (field.Host.Mode, field.Mode, "#C12-1");

			// Failure with the same exception as above
			//Assert.IsNull (field.Row, "#C13");

			Assert.IsNotNull (field.Table, "#C14");
			Assert.AreEqual (dc.Table, field.Table, "#C14-1");
		}

		[Test]
		public void ChildrenPath ()
		{
			var test = new WebTest ("ListView_DynamicControl_10.aspx");
			test.Invoker = PageInvoker.CreateOnInit (ChildrenPath_OnInit);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void ChildrenPath_OnInit (Page p)
		{
			var lc = p.FindControl ("ListView1") as ListView;
			Assert.IsNotNull (lc, "#A1");

			var page = p as TestsBasePage<TestDataContext3>;
			Assert.IsNotNull (p, "#A1-1");

			page.ItemDataBinding += new EventHandler(ChildrenPath_ListControl_OnItemDataBinding);
		}

		static void ChildrenPath_ListControl_OnItemDataBinding (object sender, EventArgs e)
		{
			var dc = sender as DynamicControl;
			Assert.IsNotNull (dc, "#B1");
			Assert.AreEqual ("PrimaryKeyColumn2", dc.ID, "#B1-1");
			Assert.AreEqual (typeof (MetaChildrenColumn), dc.Column.GetType (), "#B1-2");

			var field = dc.FieldTemplate as PokerFieldTemplateUserControl;
			Assert.IsNotNull (field, "#C1");
			Assert.AreEqual ("/NunitWeb/AssociatedBarTable/List.aspx", field.GetChildrenPath (), "#C1-1");
		}
	}
}
