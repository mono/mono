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
using NUnit.Mocks;
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
			AssertExtensions.Throws<ArgumentNullException> (() => {
				ftf.BuildVirtualPath (null, mc, DataBoundControlMode.ReadOnly);
			}, "#A1");

			AssertExtensions.Throws<ArgumentNullException> (() => {
				ftf.BuildVirtualPath (String.Empty, mc, DataBoundControlMode.ReadOnly);
			}, "#A2");

			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Integer.ascx.ascx", ftf.BuildVirtualPath ("Integer.ascx", mc, DataBoundControlMode.ReadOnly), "#B1");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Integer.ascx", ftf.BuildVirtualPath ("Integer", mc, DataBoundControlMode.ReadOnly), "#B2");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Integer_Edit.ascx", ftf.BuildVirtualPath ("Integer", mc, DataBoundControlMode.Edit), "#B3");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Integer_Insert.ascx", ftf.BuildVirtualPath ("Integer", mc, DataBoundControlMode.Insert), "#B4");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "NoSuchTemplate.ascx", ftf.BuildVirtualPath ("NoSuchTemplate", mc, DataBoundControlMode.ReadOnly), "#B5");
			Assert.AreEqual (ftf.TemplateFolderVirtualPath + "Integer.ascx", ftf.BuildVirtualPath ("Integer", null, DataBoundControlMode.ReadOnly), "#B6");
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
			AssertExtensions.Throws<NullReferenceException> (() => {
				ftf.CreateFieldTemplate (null, DataBoundControlMode.ReadOnly, "Integer.ascx");
			}, "#A1");

			// ...and again
			AssertExtensions.Throws<NullReferenceException> (() => {
				ftf.CreateFieldTemplate (mc, DataBoundControlMode.ReadOnly, null);
			}, "#A2");

			// ...and again
			AssertExtensions.Throws<NullReferenceException> (() => {
				ftf.CreateFieldTemplate (mc, DataBoundControlMode.ReadOnly, String.Empty);
			}, "#A3");

			// ...and again
			AssertExtensions.Throws<NullReferenceException> (() => {
				ftf.CreateFieldTemplate (mc, DataBoundControlMode.ReadOnly, "NoSuchTemplate");
			}, "#A4");
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
			AssertExtensions.Throws<HttpException> (() => {
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
		public static void GetFieldTemplateVirtualPath ()
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
			AssertExtensions.Throws<NullReferenceException> (() => {
				ftf.GetFieldTemplateVirtualPath (null, DataBoundControlMode.ReadOnly, "Integer.ascx");
			}, "#A1");

			// ...and again
			AssertExtensions.Throws<NullReferenceException> (() => {
				ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, null);
			}, "#A2");

			// ...and again
			AssertExtensions.Throws<NullReferenceException> (() => {
				ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, String.Empty);
			}, "#A3");

			// ...and again
			AssertExtensions.Throws<NullReferenceException> (() => {
				ftf.GetFieldTemplateVirtualPath (mc, DataBoundControlMode.ReadOnly, "NoSuchTemplate");
			}, "#A4");
		}

		[Test]
		public void GetFieldTemplateVirtualPath_2 ()
		{
			var test = new WebTest ("ListView_DynamicControl_01.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (GetFieldTemplateVirtualPath_OnLoad_2);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void GetFieldTemplateVirtualPath_OnLoad_2 (Page p)
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
		public void TemplateFolderVirtualPath ()
		{
			MetaModel m = Utils.CommonInitialize ();
			var ftf = new FieldTemplateFactory ();
			string origFolderPath = ftf.TemplateFolderVirtualPath;

			// It seems accessing TemplateFolderVirtualPath before calling Initialize sets the
			// property value to "FieldTemplates/", and the dynamic folder path is ignored later on, so
			// we need to start from scratch here.
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
			AssertExtensions.Throws<ArgumentNullException> (() => {
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
		}
	}
}
