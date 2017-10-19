//
// FieldTemplateFactoryTest.cs
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
	public class FieldTemplateFactoryTest
	{
		DynamicDataContainerModelProvider<TestDataContext> dynamicModelProvider;

		[TestFixtureSetUp]
		public void SetUp ()
		{
			Type type = GetType ();
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_01.aspx", "ListView_DynamicControl_01.aspx");
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_01.aspx.cs", "ListView_DynamicControl_01.aspx.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_03.aspx", "ListView_DynamicControl_03.aspx");
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_03.aspx.cs", "ListView_DynamicControl_03.aspx.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_04.aspx", "ListView_DynamicControl_04.aspx");
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_04.aspx.cs", "ListView_DynamicControl_04.aspx.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_09.aspx", "ListView_DynamicControl_09.aspx");
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_09.aspx.cs", "ListView_DynamicControl_09.aspx.cs");

			dynamicModelProvider = new DynamicDataContainerModelProvider<TestDataContext> ();
			Utils.RegisterContext (dynamicModelProvider, new ContextConfiguration () { ScaffoldAllTables = true });
		}

		[TestFixtureTearDown]
		public void TearDown ()
		{
			FieldTemplatePathTables.CleanUp_FullTypeNameTemplates ();
			FieldTemplatePathTables.CleanUp_ShortTypeNameTemplates ();
			WebTest.Unload ();
		}

		[SetUp]
		public void PerTestSetUp ()
		{
			// This is ran before every test
			FieldTemplatePathTables.CleanUp_FullTypeNameTemplates ();
			FieldTemplatePathTables.CleanUp_ShortTypeNameTemplates ();
		}

		[Test]
		public void Defaults ()
		{
			var ftf = new FieldTemplateFactory ();

			Assert.AreEqual (null, ftf.Model, "#A1");
			Assert.AreEqual ("FieldTemplates/", ftf.TemplateFolderVirtualPath, "#A2");
		}

		[Test]
		public void Defaults_Initialized ()
		{
			MetaModel m = Utils.CommonInitialize ();
			var ftf = new FieldTemplateFactory ();
			string origFolderPath = ftf.TemplateFolderVirtualPath;

			// It seems the ftf.TemplateFolderVirtualPath call above initializes the property to the
			// "non-prefixed" value and all subsequent requests will return that value instead of one
			// prefixed with the model's path. That's why we need to recreate ftf here.
			ftf = new FieldTemplateFactory ();
			ftf.Initialize (m);
			Assert.AreEqual (m, ftf.Model, "#A1");
			Assert.AreEqual (VirtualPathUtility.Combine (m.DynamicDataFolderVirtualPath, origFolderPath), ftf.TemplateFolderVirtualPath, "#A2");
		}

		[Test]
		public void BuildVirtualPath ()
		{
			MetaModel m = Utils.CommonInitialize ();
			MetaTable t = m.Tables[TestDataContext.TableBaz];
			MetaColumn mc = t.GetColumn ("Column1");

			var ftf = new FieldTemplateFactory ();
			Assert.Throws<ArgumentNullException> (() => {
				ftf.BuildVirtualPath (null, mc, DataBoundControlMode.ReadOnly);
			}, "#A1");

			Assert.Throws<ArgumentNullException> (() => {
				ftf.BuildVirtualPath (String.Empty, mc, DataBoundControlMode.ReadOnly);
			}, "#A2");

			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Integer.ascx.ascx", ftf.BuildVirtualPath ("Integer.ascx", mc, DataBoundControlMode.ReadOnly), "#B1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Integer.ascx", ftf.BuildVirtualPath ("Integer", mc, DataBoundControlMode.ReadOnly), "#B2");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Integer_Edit.ascx", ftf.BuildVirtualPath ("Integer", mc, DataBoundControlMode.Edit), "#B3");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Integer_Insert.ascx", ftf.BuildVirtualPath ("Integer", mc, DataBoundControlMode.Insert), "#B4");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "NoSuchTemplate.ascx", ftf.BuildVirtualPath ("NoSuchTemplate", mc, DataBoundControlMode.ReadOnly), "#B5");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Integer.ascx", ftf.BuildVirtualPath ("Integer", null, DataBoundControlMode.ReadOnly), "#B6");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Integer.ascx", ftf.BuildVirtualPath ("Integer", mc, (DataBoundControlMode)10), "#B7");
		}

		[Test]
		public void BuildVirtualPath_1 ()
		{
			MetaModel m = Utils.CommonInitialize ();
			MetaTable t = m.Tables[TestDataContext.TableBaz];
			MetaColumn mc = t.GetColumn ("CustomUIHintColumn");

			var ftf = new FieldTemplateFactory ();

			// It seems MetaColumn.UIHint is ignored (or rather it suggests that column is not used at all)
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Test.ascx", ftf.BuildVirtualPath ("Test", mc, DataBoundControlMode.ReadOnly), "#A1");
		}

		[Test]
		public void CreateFieldTemplate ()
		{
			// This test is (sort of) bogus as the .NET code completely falls apart when invoked outside
			// real web request environment. Talk about code testability...
			//
			// Most of the exceptions thrown below come from methods other than CreateFieldTemplate
			MetaModel m = Utils.CommonInitialize ();
			MetaTable t = m.Tables[TestDataContext.TableBaz];
			MetaColumn mc = t.GetColumn ("Column1");

			var ftf = new FieldTemplateFactory ();

			// And here we go again...
			Assert.Throws<NullReferenceException> (() => {
				ftf.CreateFieldTemplate (null, DataBoundControlMode.ReadOnly, "Integer.ascx");
			}, "#A1");

#if TARGET_DOTNET
			// Not going to emulate those on Mono. There are limits...

			// ...and again
			Assert.Throws<NullReferenceException> (() => {
				ftf.CreateFieldTemplate (mc, DataBoundControlMode.ReadOnly, null);
			}, "#A2");

			// ...and again
			Assert.Throws<NullReferenceException> (() => {
				ftf.CreateFieldTemplate (mc, DataBoundControlMode.ReadOnly, String.Empty);
			}, "#A3");

			// ...and again
			Assert.Throws<NullReferenceException> (() => {
				ftf.CreateFieldTemplate (mc, DataBoundControlMode.ReadOnly, "NoSuchTemplate");
			}, "#A4");
#endif
		}

		[Test]
		public void CreateFieldTemplate_2 ()
		{
			var test = new WebTest ("ListView_DynamicControl_01.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (CreateFieldTemplate_OnLoad_2);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void CreateFieldTemplate_OnLoad_2 (Page p)
		{
			MetaModel m = MetaModel.Default;
			MetaTable t = m.GetTable ("EmployeeTable");
			MetaColumn mc = t.GetColumn ("FirstName");

			var ftf = new FieldTemplateFactory ();

			// Without this, the class will fail miserably by passing invalid virtual path to internal .NET's
			// mapping routines...
			ftf.Initialize (m);
			IFieldTemplate template = ftf.CreateFieldTemplate (mc, DataBoundControlMode.ReadOnly, null);
			Assert.IsNotNull (template, "#A1");

			var ftuc = template as FieldTemplateUserControl;
			Assert.IsNotNull (ftuc, "#A2");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftuc.AppRelativeVirtualPath, "#A2-1");

			template = ftf.CreateFieldTemplate (mc, DataBoundControlMode.ReadOnly, "Boolean");
			Assert.IsNotNull (template, "#B1");

			ftuc = template as FieldTemplateUserControl;
			Assert.IsNotNull (ftuc, "#B2");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Boolean.ascx", ftuc.AppRelativeVirtualPath, "#B2-2");

			template = ftf.CreateFieldTemplate (mc, DataBoundControlMode.ReadOnly, "NoSuchTemplate");
			Assert.IsNotNull (template, "#D1");
			ftuc = template as FieldTemplateUserControl;
			Assert.IsNotNull (ftuc, "#D2");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftuc.AppRelativeVirtualPath, "#D2-2");

			template = ftf.CreateFieldTemplate (mc, DataBoundControlMode.ReadOnly, "MyCustomUIHintTemplate_Text");
			Assert.IsNotNull (template, "#E1");

			ftuc = template as FieldTemplateUserControl;
			Assert.IsNotNull (ftuc, "#E2");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "MyCustomUIHintTemplate_Text.ascx", ftuc.AppRelativeVirtualPath, "#E2-2");

			mc = t.GetColumn ("FavoriteColor");
			Assert.Throws<HttpException> (() => {
				template = ftf.CreateFieldTemplate (mc, DataBoundControlMode.ReadOnly, "PlainControlTemplate");
			}, "#F1");

			mc = t.GetColumn ("FavoriteColor");
			template = ftf.CreateFieldTemplate (mc, DataBoundControlMode.ReadOnly, "CustomColor");
			Assert.IsNotNull (template, "#G1");
			ftuc = template as FieldTemplateUserControl;
			Assert.IsNotNull (ftuc, "#G2");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "CustomColor.ascx", ftuc.AppRelativeVirtualPath, "#G2-2");
		}

		[Test]
		public void GetFieldTemplateVirtualPath ()
		{
			// This test is (sort of) bogus as the .NET code completely falls apart when invoked outside
			// real web request environment. Talk about code testability...
			//
			// Most of the exceptions thrown below come from methods other than CreateFieldTemplate
			MetaModel m = Utils.CommonInitialize ();
			MetaTable t = m.Tables[TestDataContext.TableBaz];
			MetaColumn mc = t.GetColumn ("Column1");

			var ftf = new FieldTemplateFactory ();

			// And here we go again...
			Assert.Throws<NullReferenceException> (() => {
				ftf.GetFieldTemplateVirtualPath (null, DataBoundControlMode.ReadOnly, "Integer.ascx");
			}, "#A1");

#if TARGET_DOTNET
			// ...and again
			Assert.Throws<NullReferenceException> (() => {
				ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null);
			}, "#A2");

			// ...and again
			Assert.Throws<NullReferenceException> (() => {
				ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, String.Empty);
			}, "#A3");

			// ...and again
			Assert.Throws<NullReferenceException> (() => {
				ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "NoSuchTemplate");
			}, "#A4");
#endif
		}

		[Test]
		public void GetFieldTemplateVirtualPath_02 ()
		{
			var test = new WebTest ("ListView_DynamicControl_01.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (GetFieldTemplateVirtualPath_OnLoad_02);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void GetFieldTemplateVirtualPath_OnLoad_02 (Page p)
		{
			MetaModel m = MetaModel.Default;
			MetaTable t = m.GetTable ("EmployeeTable");
			MetaColumn mc = t.GetColumn ("FirstName");

			var ftf = new FieldTemplateFactory ();

			// Without this, the class will fail miserably by passing invalid virtual path to internal .NET's
			// mapping routines...
			ftf.Initialize (m);
			string templatePath = ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null);
			Assert.IsFalse (String.IsNullOrEmpty (templatePath), "#A1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", templatePath, "#A1-1");

			templatePath = ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean");
			Assert.IsNotNull (templatePath, "#B1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Boolean.ascx", templatePath, "#B1-2");

			templatePath = ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "NoSuchTemplate");
			Assert.IsNotNull (templatePath, "#D1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", templatePath, "#D1-2");

			templatePath = ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "MyCustomUIHintTemplate_Text");
			Assert.IsNotNull (templatePath, "#E1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "MyCustomUIHintTemplate_Text.ascx", templatePath, "#E1-2");

			mc = t.GetColumn ("LastName");
			Assert.AreEqual ("CustomFieldTemplate", mc.UIHint, "#F1");
			templatePath = ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null);
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", templatePath, "#F1-1");

			templatePath = ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "CustomFieldTemplate");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "CustomFieldTemplate.ascx", templatePath, "#F2");
		}

		[Test]
		public void GetFieldTemplateVirtualPath_03 ()
		{
			var test = new WebTest ("ListView_DynamicControl_09.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (GetFieldTemplateVirtualPath_OnLoad_03);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void GetFieldTemplateVirtualPath_OnLoad_03 (Page p)
		{
			MetaModel m = MetaModel.Default;
			MetaTable t = m.GetTable ("AssociatedFooTable");
			MetaColumn mc = t.GetColumn ("PrimaryKeyColumn2");

			Assert.AreEqual (typeof (MetaChildrenColumn), mc.GetType (), "#A1");

			var ftf = new FieldTemplateFactory ();
			ftf.Initialize (m);

			string templatePath = ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Text.ascx");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", templatePath, "#A2");

			templatePath = ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null);
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Children.ascx", templatePath, "#A3");

			// When MetaColumn.DataTypeAttribute is not null (which will always be the case for string columns), column type is ignored
			mc = t.GetColumn ("ForeignKeyColumn1");
			Assert.AreEqual (typeof (MetaForeignKeyColumn), mc.GetType (), "#B1");
			Assert.IsNotNull (mc.DataTypeAttribute, "#B1-1");

			ftf = new FieldTemplateFactory ();
			ftf.Initialize (m);

			templatePath = ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Text.ascx");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", templatePath, "#B2");

			templatePath = ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null);
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", templatePath, "#B3");

			mc = t.GetColumn ("ForeignKeyColumn2");
			Assert.AreEqual (typeof (MetaForeignKeyColumn), mc.GetType (), "#C1");
			Assert.IsNull (mc.DataTypeAttribute, "#C1-1");

			ftf = new FieldTemplateFactory ();
			ftf.Initialize (m);

			templatePath = ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Text.ascx");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", templatePath, "#C2");

			templatePath = ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null);
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "ForeignKey.ascx", templatePath, "#C3");
		}

		[Test]
		public void GetFieldTemplateVirtualPath_04 ()
		{
			var test = new WebTest ("ListView_DynamicControl_03.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (GetFieldTemplateVirtualPath_OnLoad_04);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void GetFieldTemplateVirtualPath_OnLoad_04 (Page p)
		{
			var lc = p.FindControl ("ListView3") as ListView;
			Assert.IsNotNull (lc, "#A1");

			int counter = 1;
			var ftf = new FieldTemplateFactory ();
			ftf.Initialize (MetaModel.Default);

			foreach (var entry in FieldTemplatePathTables.FieldTemplateReadOnlyColumns) {
				string columnName = entry.ColumnName;
				var dc = lc.FindChild<PokerDynamicControl> (columnName);
				Assert.IsNotNull (dc, String.Format ("#B{0}-1 ({1})", counter, columnName));
				Assert.AreEqual (columnName, dc.ID, String.Format ("#B{0}-2 ({1})", counter, columnName));

				string path = ftf.GetFieldTemplateVirtualPath (dc.Column, DataBoundControlMode.ReadOnly, null);
				if (entry.IsNull)
					Assert.IsNull (path, String.Format ("#B{0}-3 ({1})", counter, columnName));
				else {
					Assert.IsNotNull (path, String.Format ("#B{0}-4 ({1})", counter, columnName));
					Assert.AreEqual (entry.ControlVirtualPath, path, String.Format ("#B{0}-5 ({1})", counter, columnName));
				}

				counter++;
			}
		}

		[Test]
		public void GetFieldTemplateVirtualPath_05 ()
		{
			var test = new WebTest ("ListView_DynamicControl_04.aspx");
			var p = test.Run ();

			// Fake post-back
			var delegates = new PageDelegates ();
			delegates.PreRenderComplete = GetFieldTemplateVirtualPath_OnPreRenderComplete_05;
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

		static void GetFieldTemplateVirtualPath_OnPreRenderComplete_05 (Page p)
		{
			var lc = p.FindControl ("ListView4") as ListView;
			Assert.IsNotNull (lc, "#A1");

			var ftf = new FieldTemplateFactory ();
			ftf.Initialize (MetaModel.Default);
			int counter = 1;
			foreach (var entry in FieldTemplatePathTables.FieldTemplateEditColumns) {
				string columnName = entry.ColumnName;
				var dc = lc.FindChild<PokerDynamicControl> (columnName);
				Assert.IsNotNull (dc, String.Format ("#B{0}-1 ({1})", counter, columnName));
				Assert.AreEqual (columnName, dc.ID, String.Format ("#B{0}-2 ({1})", counter, columnName));

				string path = ftf.GetFieldTemplateVirtualPath (dc.Column, DataBoundControlMode.Edit, null);
				if (entry.IsNull)
					Assert.IsNull (path, String.Format ("#B{0}-3 ({1})", counter, columnName));
				else {
					Assert.IsNotNull (path, String.Format ("#B{0}-4 ({1})", counter, columnName));
					Assert.AreEqual (entry.ControlVirtualPath, path, String.Format ("#B{0}-5 ({1})", counter, columnName));
				}

				counter++;
			}
		}

		// This tests full type name templates
		[Test]
		public void GetFieldTemplateVirtualPath_06 ()
		{
			FieldTemplatePathTables.SetUp_FullTypeNameTemplates (this);
			var test = new WebTest ("ListView_DynamicControl_03.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (GetFieldTemplateVirtualPath_OnLoad_06);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void GetFieldTemplateVirtualPath_OnLoad_06 (Page p)
		{
			var lc = p.FindControl ("ListView3") as ListView;
			Assert.IsNotNull (lc, "#A1");

			var ftf = new FieldTemplateFactory ();
			ftf.Initialize (MetaModel.Default);
			int counter = 1;
			foreach (var entry in FieldTemplatePathTables.FieldTemplateNonDefaultColumns) {
				string columnName = entry.ColumnName;
				var dc = lc.FindChild<PokerDynamicControl> (columnName);
				Assert.IsNotNull (dc, String.Format ("#B{0}-1 ({1})", counter, columnName));
				Assert.AreEqual (columnName, dc.ID, String.Format ("#B{0}-2 ({1})", counter, columnName));

				string path = ftf.GetFieldTemplateVirtualPath (dc.Column, DataBoundControlMode.ReadOnly, null);
				if (entry.IsNull)
					Assert.IsNull (path, String.Format ("#B{0}-3 ({1})", counter, columnName));
				else {
					Assert.IsNotNull (path, String.Format ("#B{0}-4 ({1})", counter, columnName));
					Assert.AreEqual (entry.ControlVirtualPath, path, String.Format ("#B{0}-5 ({1})", counter, columnName));
				}

				counter++;
			}
		}

		// This tests short type name templates
		[Test]
		public void GetFieldTemplateVirtualPath_07 ()
		{
			FieldTemplatePathTables.SetUp_ShortTypeNameTemplates (this);
			var test = new WebTest ("ListView_DynamicControl_03.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (GetFieldTemplateVirtualPath_OnLoad_07);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void GetFieldTemplateVirtualPath_OnLoad_07 (Page p)
		{
			var lc = p.FindControl ("ListView3") as ListView;
			Assert.IsNotNull (lc, "#A1");

			var ftf = new FieldTemplateFactory ();
			ftf.Initialize (MetaModel.Default);
			int counter = 1;
			foreach (var entry in FieldTemplatePathTables.FieldTemplateNonDefaultShortColumns) {
				string columnName = entry.ColumnName;
				var dc = lc.FindChild<PokerDynamicControl> (columnName);
				Assert.IsNotNull (dc, String.Format ("#B{0}-1 ({1})", counter, columnName));
				Assert.AreEqual (columnName, dc.ID, String.Format ("#B{0}-2 ({1})", counter, columnName));

				string path = ftf.GetFieldTemplateVirtualPath (dc.Column, DataBoundControlMode.ReadOnly, null);
				if (entry.IsNull)
					Assert.IsNull (path, String.Format ("#B{0}-3 ({1})", counter, columnName));
				else {
					Assert.IsNotNull (path, String.Format ("#B{0}-4 ({1})", counter, columnName));
					Assert.AreEqual (entry.ControlVirtualPath, path, String.Format ("#B{0}-5 ({1})", counter, columnName));
				}

				counter++;
			}
		}

		[Test]
		public void GetFieldTemplateVirtualPath_08 ()
		{
			var test = new WebTest ("ListView_DynamicControl_09.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (GetFieldTemplateVirtualPath_OnLoad_08);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void GetFieldTemplateVirtualPath_OnLoad_08 (Page p)
		{
			MetaModel m = MetaModel.Default;
			MetaTable t = m.GetTable ("AssociatedFooTable");
			MetaColumn mc = t.GetColumn ("PrimaryKeyColumn2");

			var ftf = new FieldTemplateFactory ();
			ftf.Initialize (m);

			// Ugh...
			Assert.Throws<NullReferenceException> (() => {
				ftf.GetFieldTemplateVirtualPath (null, DataBoundControlMode.ReadOnly, "Integer.ascx");
			}, "#A1");


			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Children.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null), "#A2");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Children.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, String.Empty), "#A3");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "NoSuchTemplate"), "#A4");
		}

		[Test]
		public void GetFieldTemplateVirtualPath_09 ()
		{
			var test = new WebTest ("ListView_DynamicControl_09.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (GetFieldTemplateVirtualPath_OnLoad_09);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void GetFieldTemplateVirtualPath_OnLoad_09 (Page p)
		{
			MetaModel m = MetaModel.Default;
			MetaTable t = m.GetTable ("BazWithDataTypeAttributeTable");
			MetaColumn mc = t.GetColumn ("CustomColumn1");

			var ftf = new FieldTemplateFactory ();
			ftf.Initialize (m);

			// Custom type
			//   It appears that DataTypeAttribute's custom type name is passed to BuildVirtualPath
			Assert.Throws<InvalidOperationException> (() => {
				string path = ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null);
			}, "#A1");
			
			Assert.Throws<InvalidOperationException> (() => {
				string path = ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "NoSuchTemplate");
			}, "#A1-1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Boolean.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean"), "#A1-2");

			// Custom with UIHint attribute
			mc = t.GetColumn ("CustomColumn2");
			Assert.IsNotNull (mc.UIHint, "#A2");
			AssertHelper.Greater (mc.UIHint.Length, 0, "#A2-1");

			// Proves that UIHint on the column is not used, just the uiHint argument
			Assert.Throws<InvalidOperationException> (() => {
				string path = ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null);
			}, "#A2-2");

			Assert.Throws<InvalidOperationException> (() => {
				string path = ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "NoSuchTemplate");
			}, "#A2-3");
			
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Boolean.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean"), "#A2-4");

			mc = t.GetColumn ("CustomColumn3");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null), "#A3");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "NoSuchTemplate"), "#A3-1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Boolean.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean"), "#A3-2");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, String.Empty), "#A3-3");

			// Custom with UIHint attribute
			mc = t.GetColumn ("CustomColumn4");
			Assert.IsNotNull (mc.UIHint, "#A4");
			AssertHelper.Greater (mc.UIHint.Length, 0, "#A4-1");

			// Proves that UIHint on the column is not used, just the uiHint argument
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null), "#A4-2");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "NoSuchTemplate"), "#A4-3");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Boolean.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean"), "#A4-4");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, String.Empty), "#A4-5");

			// DateTime
			mc = t.GetColumn ("DateTimeColumn1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "DateTime.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null), "#B1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "DateTime.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, String.Empty), "#B1-1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "DateTime.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean.ascx"), "#B1-2");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Boolean.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean"), "#B1-3");

			mc = t.GetColumn ("DateTimeColumn2");
			Assert.IsNotNull (mc.UIHint, "#B2");
			AssertHelper.Greater (mc.UIHint.Length, 0, "#B2-1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "DateTime.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null), "#B2-3");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "DateTime.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, String.Empty), "#B2-4");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "DateTime.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean.ascx"), "#B2-5");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Boolean.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean"), "#B2-6");

			// Date -> maps to Text.ascx, regardless of underlying type and uiHint passed
			mc = t.GetColumn ("DateColumn1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null), "#C1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, String.Empty), "#C1-1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean.ascx"), "#C1-2");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Boolean.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean"), "#C1-3");

			mc = t.GetColumn ("DateColumn2");
			Assert.IsNotNull (mc.UIHint, "#C2");
			AssertHelper.Greater (mc.UIHint.Length, 0, "#C2-1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null), "#C2-3");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, String.Empty), "#C2-4");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean.ascx"), "#C2-5");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Boolean.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean"), "#C2-6");

			mc = t.GetColumn ("DateColumn3");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null), "#C3");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, String.Empty), "#C3-1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean.ascx"), "#C3-2");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Boolean.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean"), "#C3-3");

			mc = t.GetColumn ("DateColumn4");
			Assert.IsNotNull (mc.UIHint, "#C4");
			AssertHelper.Greater (mc.UIHint.Length, 0, "#C4-1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null), "#C4-3");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, String.Empty), "#C4-4");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean.ascx"), "#C4-5");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Boolean.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean"), "#C4-6");

			// Time
			mc = t.GetColumn ("TimeColumn1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null), "#D1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, String.Empty), "#D1-1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean.ascx"), "#D1-2");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Boolean.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean"), "#D1-3");

			mc = t.GetColumn ("TimeColumn2");
			Assert.IsNotNull (mc.UIHint, "#D2");
			AssertHelper.Greater (mc.UIHint.Length, 0, "#D2-1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null), "#D2-3");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, String.Empty), "#D2-4");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean.ascx"), "#D2-5");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Boolean.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean"), "#D2-6");

			// Duration
			mc = t.GetColumn ("DurationColumn1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null), "#E1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, String.Empty), "#E1-1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean.ascx"), "#E1-2");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Boolean.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean"), "#E1-3");

			mc = t.GetColumn ("DurationColumn2");
			Assert.IsNotNull (mc.UIHint, "#E2");
			AssertHelper.Greater (mc.UIHint.Length, 0, "#E2-1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null), "#E2-3");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, String.Empty), "#E2-4");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean.ascx"), "#E2-5");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Boolean.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean"), "#E2-6");

			// PhoneNumber
			mc = t.GetColumn ("PhoneNumberColumn1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null), "#F1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, String.Empty), "#F1-1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean.ascx"), "#F1-2");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Boolean.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean"), "#F1-3");

			mc = t.GetColumn ("PhoneNumberColumn2");
			Assert.IsNotNull (mc.UIHint, "#F2");
			AssertHelper.Greater (mc.UIHint.Length, 0, "#F2-1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null), "#F2-3");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, String.Empty), "#F2-4");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean.ascx"), "#F2-5");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Boolean.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean"), "#F2-6");

			// Currency
			mc = t.GetColumn ("CurrencyColumn1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null), "#G1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, String.Empty), "#G1-1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean.ascx"), "#G1-2");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Boolean.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean"), "#G1-3");

			mc = t.GetColumn ("CurrencyColumn2");
			Assert.IsNotNull (mc.UIHint, "#G2");
			AssertHelper.Greater (mc.UIHint.Length, 0, "#G2-1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null), "#G2-3");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, String.Empty), "#G2-4");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean.ascx"), "#G2-5");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Boolean.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean"), "#G2-6");

			// Text
			mc = t.GetColumn ("TextColumn1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null), "#H1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, String.Empty), "#H1-1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean.ascx"), "#H1-2");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Boolean.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean"), "#H1-3");

			mc = t.GetColumn ("TextColumn2");
			Assert.IsNotNull (mc.UIHint, "#H2");
			AssertHelper.Greater (mc.UIHint.Length, 0, "#H2-1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null), "#H2-3");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, String.Empty), "#H2-4");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean.ascx"), "#H2-5");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Boolean.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean"), "#H2-6");

			// Html
			mc = t.GetColumn ("HtmlColumn1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null), "#I1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, String.Empty), "#I1-1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean.ascx"), "#I1-2");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Boolean.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean"), "#I1-3");

			mc = t.GetColumn ("HtmlColumn2");
			Assert.IsNotNull (mc.UIHint, "#I2");
			AssertHelper.Greater (mc.UIHint.Length, 0, "#I2-1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null), "#I2-3");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, String.Empty), "#I2-4");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean.ascx"), "#I2-5");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Boolean.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean"), "#I2-6");

			// MultilineText
			mc = t.GetColumn ("MultilineTextColumn1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null), "#J1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, String.Empty), "#J1-1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean.ascx"), "#J1-2");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Boolean.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean"), "#J1-3");

			mc = t.GetColumn ("MultilineTextColumn2");
			Assert.IsNotNull (mc.UIHint, "#J2");
			AssertHelper.Greater (mc.UIHint.Length, 0, "#J2-1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null), "#J2-3");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, String.Empty), "#J2-4");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean.ascx"), "#J2-5");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Boolean.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean"), "#J2-6");

			// EmailAddress
			mc = t.GetColumn ("EmailAddressColumn1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null), "#K1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, String.Empty), "#K1-1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean.ascx"), "#K1-2");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Boolean.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean"), "#K1-3");

			mc = t.GetColumn ("EmailAddressColumn2");
			Assert.IsNotNull (mc.UIHint, "#K2");
			AssertHelper.Greater (mc.UIHint.Length, 0, "#K2-1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null), "#K2-3");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, String.Empty), "#K2-4");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean.ascx"), "#K2-5");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Boolean.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean"), "#K2-6");

			// Password
			mc = t.GetColumn ("PasswordColumn1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null), "#L1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, String.Empty), "#L1-1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean.ascx"), "#L1-2");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Boolean.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean"), "#L1-3");

			mc = t.GetColumn ("PasswordColumn2");
			Assert.IsNotNull (mc.UIHint, "#L2");
			AssertHelper.Greater (mc.UIHint.Length, 0, "#L2-1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null), "#L2-3");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, String.Empty), "#L2-4");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean.ascx"), "#L2-5");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Boolean.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean"), "#L2-6");

			// Url
			mc = t.GetColumn ("UrlColumn1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null), "#M1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, String.Empty), "#M1-1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean.ascx"), "#M1-2");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Boolean.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean"), "#M1-3");

			mc = t.GetColumn ("UrlColumn2");
			Assert.IsNotNull (mc.UIHint, "#M2");
			AssertHelper.Greater (mc.UIHint.Length, 0, "#M2-1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null), "#M2-3");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, String.Empty), "#M2-4");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Text.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean.ascx"), "#M2-5");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Boolean.ascx", ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "Boolean"), "#M2-6");
		}

		[Test]
		public void Initialize ()
		{
			MetaModel m = Utils.CommonInitialize ();

			var ftf = new FieldTemplateFactory ();
			Assert.IsNull (ftf.Model, "#A1");

			ftf.Initialize (null);
			Assert.IsNull (ftf.Model, "#A2");

			ftf.Initialize (m);
			Assert.IsNotNull (ftf.Model, "#A3");
			Assert.AreEqual (m, ftf.Model, "#A3-1");
		}

		[Test]
		public void Model ()
		{
			MetaModel m = Utils.CommonInitialize ();

			var ftf = new FieldTemplateFactory ();
			Assert.IsNull (ftf.Model, "#A1");

			ftf.Initialize (null);
			Assert.IsNull (ftf.Model, "#A2");

			ftf.Initialize (m);
			Assert.IsNotNull (ftf.Model, "#A3");
			Assert.AreEqual (m, ftf.Model, "#A3-1");
		}

		[Test]
		public void PreprocessMode ()
		{
			MetaModel m = Utils.CommonInitialize ();
			MetaTable t = m.Tables[TestDataContext.TableFooWithDefaults];
			MetaColumn mc = t.GetColumn ("Column1");

			var ftf = new FieldTemplateFactory ();
			ftf.Initialize (m);

			Assert.Throws<NullReferenceException> (() => {
				ftf.PreprocessMode (null, DataBoundControlMode.ReadOnly);
			}, "#A1");

			var mode = ftf.PreprocessMode (mc, DataBoundControlMode.ReadOnly);
			Assert.AreEqual (DataBoundControlMode.ReadOnly, mode, "#A2");

			mode = ftf.PreprocessMode (mc, DataBoundControlMode.Insert);
			Assert.AreEqual (DataBoundControlMode.Insert, mode, "#A2-1");

			mode = ftf.PreprocessMode (mc, DataBoundControlMode.Edit);
			Assert.AreEqual (DataBoundControlMode.Edit, mode, "#A2-2");

			mc = t.GetColumn ("PrimaryKeyColumn1");
			mode = ftf.PreprocessMode (mc, DataBoundControlMode.ReadOnly);
			Assert.AreEqual (DataBoundControlMode.ReadOnly, mode, "#B1");

			mode = ftf.PreprocessMode (mc, DataBoundControlMode.Insert);
			Assert.AreEqual (DataBoundControlMode.Insert, mode, "#B1-1");

			mode = ftf.PreprocessMode (mc, DataBoundControlMode.Edit);
			Assert.AreEqual (DataBoundControlMode.ReadOnly, mode, "#B1-2");

			t = m.Tables[TestDataContext.TableAssociatedFoo];
			mc = t.GetColumn ("ForeignKeyColumn1");

			Assert.AreEqual (true, mc.IsForeignKeyComponent, "#C1");
			mode = ftf.PreprocessMode (mc, DataBoundControlMode.ReadOnly);
			Assert.AreEqual (DataBoundControlMode.ReadOnly, mode, "#C1-1");

			mode = ftf.PreprocessMode (mc, DataBoundControlMode.Insert);
			Assert.AreEqual (DataBoundControlMode.Insert, mode, "#C1-2");

			mode = ftf.PreprocessMode (mc, DataBoundControlMode.Edit);
			Assert.AreEqual (DataBoundControlMode.Edit, mode, "#C1-3");

			t = m.Tables[TestDataContext.TableBaz];
			mc = t.GetColumn ("GeneratedColumn1");

			Assert.AreEqual (true, mc.IsGenerated, "#D1");
			mode = ftf.PreprocessMode (mc, DataBoundControlMode.ReadOnly);
			Assert.AreEqual (DataBoundControlMode.ReadOnly, mode, "#D1-1");

			mode = ftf.PreprocessMode (mc, DataBoundControlMode.Insert);
			Assert.AreEqual (DataBoundControlMode.ReadOnly, mode, "#D1-2");

			mode = ftf.PreprocessMode (mc, DataBoundControlMode.Edit);
			Assert.AreEqual (DataBoundControlMode.ReadOnly, mode, "#D1-3");

			t = m.Tables[TestDataContext.TableAssociatedFoo];
			mc = t.GetColumn ("PrimaryKeyColumn1");

			Assert.AreEqual (true, mc.IsForeignKeyComponent, "#D1");
			Assert.AreEqual (true, mc.IsPrimaryKey, "#D1-1");
			mode = ftf.PreprocessMode (mc, DataBoundControlMode.ReadOnly);
			Assert.AreEqual (DataBoundControlMode.ReadOnly, mode, "#D1-2");

			mode = ftf.PreprocessMode (mc, DataBoundControlMode.Insert);
			Assert.AreEqual (DataBoundControlMode.Insert, mode, "#D1-3");

			mode = ftf.PreprocessMode (mc, DataBoundControlMode.Edit);
			Assert.AreEqual (DataBoundControlMode.ReadOnly, mode, "#D1-4");
		}

		[Test]
		public void TemplateFolderVirtualPath ()
		{
			MetaModel m = Utils.CommonInitialize ();
			var ftf = new FieldTemplateFactory ();
			string origFolderPath = ftf.TemplateFolderVirtualPath;

			ftf = new FieldTemplateFactory ();

			Assert.AreEqual ("FieldTemplates/", ftf.TemplateFolderVirtualPath, "#A1");
			ftf.TemplateFolderVirtualPath = "MyFolder";
			Assert.AreEqual ("MyFolder/", ftf.TemplateFolderVirtualPath, "#A2");

			ftf.TemplateFolderVirtualPath = null;
			Assert.AreEqual ("FieldTemplates/", ftf.TemplateFolderVirtualPath, "#A3");

			ftf.TemplateFolderVirtualPath = String.Empty;
			Assert.AreEqual (String.Empty, ftf.TemplateFolderVirtualPath, "#A4");

			ftf.Initialize (m);
			Assert.AreEqual (String.Empty, ftf.TemplateFolderVirtualPath, "#A5");

			ftf.TemplateFolderVirtualPath = "MyFolder";
			Assert.AreEqual (m.DynamicDataFolderVirtualPath + "MyFolder/", ftf.TemplateFolderVirtualPath, "#A6");

			ftf.TemplateFolderVirtualPath = null;
			Assert.AreEqual (m.DynamicDataFolderVirtualPath + "FieldTemplates/", ftf.TemplateFolderVirtualPath, "#A7");

			ftf.TemplateFolderVirtualPath = String.Empty;
			// Thrown from some internal method - no checks _again_
			Assert.Throws<ArgumentNullException> (() => {
				string path = ftf.TemplateFolderVirtualPath;
			}, "#A8");

			ftf = new FieldTemplateFactory ();
			ftf.TemplateFolderVirtualPath = "MyFolder";
			Assert.AreEqual ("MyFolder/", ftf.TemplateFolderVirtualPath, "#B1");
			ftf.Initialize (m);
			Assert.AreEqual ("MyFolder/", ftf.TemplateFolderVirtualPath, "#B2");

			ftf = new FieldTemplateFactory ();
			ftf.Initialize (m);
			Assert.AreEqual (m.DynamicDataFolderVirtualPath + origFolderPath, ftf.TemplateFolderVirtualPath, "#C1");

			ftf.TemplateFolderVirtualPath = "MyFolder";
			Assert.AreEqual (m.DynamicDataFolderVirtualPath + "MyFolder/", ftf.TemplateFolderVirtualPath, "#C2");

			ftf = new FieldTemplateFactory ();
			ftf.Initialize (m);
			m.DynamicDataFolderVirtualPath = "~/MyPath";
			Assert.AreEqual (m.DynamicDataFolderVirtualPath + "FieldTemplates/", ftf.TemplateFolderVirtualPath, "#D1");

			ftf = new FieldTemplateFactory ();
			ftf.TemplateFolderVirtualPath = "MyFolder";
			Assert.AreEqual ("MyFolder/", ftf.TemplateFolderVirtualPath, "#E1");

			ftf.Initialize (m);
			m.DynamicDataFolderVirtualPath = "~/MyPath";
			Assert.AreEqual ("MyFolder/", ftf.TemplateFolderVirtualPath, "#F1");

			ftf = new FieldTemplateFactory ();
			ftf.Initialize (m);
			ftf.TemplateFolderVirtualPath = String.Empty;

			Assert.Throws<ArgumentNullException> (() => {
				string path = ftf.TemplateFolderVirtualPath;
			}, "#G1");
		}
	}
}
