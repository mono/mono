//
// MetaModelTest.cs
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

using MetaModel = System.Web.DynamicData.MetaModel;
using MetaTable = System.Web.DynamicData.MetaTable;

namespace MonoTests.System.Web.DynamicData
{
	[TestFixture]
	public class DynamicControlTest
	{
		[TestFixtureSetUp]
		public void SetUp ()
		{
			Type type = GetType ();
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_01.aspx", "ListView_DynamicControl_01.aspx");
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_01.aspx.cs", "ListView_DynamicControl_01.aspx.cs");
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
			Assert.AreEqual (dc, ((IFieldTemplateHost) dc).FormattingOptions, "#A9");
			Assert.AreEqual (DataBoundControlMode.ReadOnly, dc.Mode, "#A10");
			Assert.AreEqual (String.Empty, dc.NullDisplayText, "#A11");
			// Throws NREX on .NET .... (why am I still surprised by this?)
			// Calls DynamicDataExtensions.FindMetaTable which is where the exception is thrown from
			// Assert.AreEqual (null, dc.Table, "#A12");
			Assert.AreEqual (String.Empty, dc.UIHint, "#A13");
			Assert.AreEqual (String.Empty, dc.ValidationGroup, "#A14");
		}

		[Test]
		public void DataField ()
		{
			var dc = new DynamicControl ();

			Assert.AreEqual (String.Empty, dc.DataField, "#A1");
			dc.DataField = "MyField";
			Assert.AreEqual ("MyField", dc.DataField, "#A2");
		}

		[Test]
		public void Table ()
		{
			var test = new WebTest ("ListView_DynamicControl_01.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (Table_PageLoad);
			test.Run ();
			Assert.IsNotNull (test.Response, "#A1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#A1-1");
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#A1-2");
		}

		static void Table_PageLoad (Page p)
		{
			var lc = p.FindControl ("ListView1") as ListView;
			Assert.IsNotNull (lc, "#B1");

			var dc = lc.FindChild<DynamicControl> ("FirstName");
			Assert.IsNotNull (dc, "#B1-1");

			Assert.IsNotNull (dc.Table, "#C1");

			// Safe not to check for GetModel's return value - it throws if model isn't found, same
			// goes for GetTable
			MetaTable table = MetaModel.GetModel (typeof (EmployeesDataContext)).GetTable ("EmployeeTable");
			Assert.AreEqual (table, dc.Table, "#C1-1");
		}
	}
}
