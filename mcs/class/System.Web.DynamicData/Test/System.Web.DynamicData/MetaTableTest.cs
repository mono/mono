//
// MetaTableTest.cs
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
using System.Reflection;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web;
using System.Web.UI;
using System.Web.DynamicData;
using System.Web.DynamicData.ModelProviders;
using System.Web.Routing;

using NUnit.Framework;
using NUnit.Mocks;
using MonoTests.stand_alone.WebHarness;
using MonoTests.SystemWeb.Framework;
using MonoTests.Common;
using MonoTests.ModelProviders;

using MetaModel = System.Web.DynamicData.MetaModel;
using MetaTable = System.Web.DynamicData.MetaTable;

namespace MonoTests.System.Web.DynamicData
{
	// IMPORTANT
	//
	// ALL tests which make use of RouteTable.Routes _MUST_ clear the collection before running
	//
	[TestFixture]
	public class MetaTableTest
	{
		const int TableFooWithDefaults = 0;
		const int TableFooNoPrimaryColumns = 1;
		const int TableFooNoDefaultsWithPrimaryKey = 2;
		const int TableFooDisplayColumnAttribute = 3;
		const int TableFooEmpty = 4;
		const int TableBaz = 5;
		const int TableBazNoStrings = 6;
		const int TableBazNoStringsNoPrimary = 7;
		const int TableFooWithToString = 8;
		const int TableFooInvalidDisplayColumnAttribute = 9;
		const int TableFooEmptyDisplayColumnAttribute = 10;
		const int TableFooSettableDefaults = 11;
		const int TableFooDisplayName = 12;
		const int TableFooDisplayNameEmptyName = 13;

		[TestFixtureSetUp]
		public void SetUp ()
		{
			var modelProvider = new DynamicDataContainerModelProvider (typeof (TestDataContainer <FooWithDefaults>));
			Utils.RegisterContext (modelProvider, new ContextConfiguration () { ScaffoldAllTables = true });
			
			modelProvider = new DynamicDataContainerModelProvider (typeof (TestDataContainer <FooNoPrimaryColumns>));
			Utils.RegisterContext (modelProvider, new ContextConfiguration () { ScaffoldAllTables = true });

			modelProvider = new DynamicDataContainerModelProvider (typeof (TestDataContainer<FooNoDefaultsWithPrimaryKey>));
			Utils.RegisterContext (modelProvider, new ContextConfiguration () { ScaffoldAllTables = true });

			modelProvider = new DynamicDataContainerModelProvider (typeof (TestDataContainer<FooDisplayColumnAttribute>));
			Utils.RegisterContext (modelProvider, new ContextConfiguration () { ScaffoldAllTables = true });

			modelProvider = new DynamicDataContainerModelProvider (typeof (TestDataContainer<FooEmpty>));
			Utils.RegisterContext (modelProvider, new ContextConfiguration () { ScaffoldAllTables = true });

			modelProvider = new DynamicDataContainerModelProvider (typeof (TestDataContainer<Baz>));
			Utils.RegisterContext (modelProvider, new ContextConfiguration () { ScaffoldAllTables = true });

			modelProvider = new DynamicDataContainerModelProvider (typeof (TestDataContainer<BazNoStrings>));
			Utils.RegisterContext (modelProvider, new ContextConfiguration () { ScaffoldAllTables = true });

			modelProvider = new DynamicDataContainerModelProvider (typeof (TestDataContainer<BazNoStringsNoPrimary>));
			Utils.RegisterContext (modelProvider, new ContextConfiguration () { ScaffoldAllTables = true });

			modelProvider = new DynamicDataContainerModelProvider (typeof (TestDataContainer<FooWithToString>));
			Utils.RegisterContext (modelProvider, new ContextConfiguration () { ScaffoldAllTables = true });

			modelProvider = new DynamicDataContainerModelProvider (typeof (TestDataContainer<FooInvalidDisplayColumnAttribute>));
			Utils.RegisterContext (modelProvider, new ContextConfiguration () { ScaffoldAllTables = true });

			modelProvider = new DynamicDataContainerModelProvider (typeof (TestDataContainer<FooEmptyDisplayColumnAttribute>));
			Utils.RegisterContext (modelProvider, new ContextConfiguration () { ScaffoldAllTables = true });

			modelProvider = new DynamicDataContainerModelProvider (typeof (TestDataContainer<FooSettableDefaults>));
			Utils.RegisterContext (modelProvider, new ContextConfiguration () { ScaffoldAllTables = true });

			modelProvider = new DynamicDataContainerModelProvider (typeof (TestDataContainer<FooDisplayName>));
			Utils.RegisterContext (modelProvider, new ContextConfiguration () { ScaffoldAllTables = true });

			modelProvider = new DynamicDataContainerModelProvider (typeof (TestDataContainer<FooDisplayNameEmptyName>));
			Utils.RegisterContext (modelProvider, new ContextConfiguration () { ScaffoldAllTables = true });
		}

		[Test]
		public void Attributes ()
		{
			MetaModel m = MetaModel.Default;

			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();
			routes.Add (
			    new DynamicDataRoute ("{table}/{action}.aspx") {
				    Constraints = new RouteValueDictionary (new { action = "List|Details|Edit|Insert" }),
				    Model = m,
				    RouteHandler = new MyDynamicDataRouteHandler ()
			    });

			MetaTable t = m.Tables[TableFooEmpty];
			Assert.IsNotNull (t.Attributes, "#A1");
			Assert.AreEqual (0, t.Attributes.Count, "#A2");
		}

		[Test]
		public void Columns ()
		{
			MetaModel m = MetaModel.Default;

			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();
			routes.Add (
			    new DynamicDataRoute ("{table}/{action}.aspx") {
				    Constraints = new RouteValueDictionary (new { action = "List|Details|Edit|Insert" }),
				    Model = m,
				    RouteHandler = new MyDynamicDataRouteHandler ()
			    });

			MetaTable t = m.Tables[TableFooEmpty];
			Assert.IsNotNull (t.Columns, "#A1");
			Assert.AreEqual (0, t.Columns.Count, "#A2");

			t = m.Tables[TableFooWithDefaults];
			Assert.IsNotNull (t.Columns, "#B1");
			Assert.AreEqual (8, t.Columns.Count, "#B2");

			Assert.IsNotNull (t.Columns[0], "#C1");
			Assert.AreEqual ("Column1", t.Columns[0].Name, "#C1-1");

			Assert.IsNotNull (t.Columns[1], "#D1");
			Assert.AreEqual ("Column2", t.Columns[1].Name, "#D1-1");

			Assert.IsNotNull (t.Columns[2], "#E1");
			Assert.AreEqual ("PrimaryKeyColumn1", t.Columns[2].Name, "#E1-1");

			Assert.IsNotNull (t.Columns[3], "#F1");
			Assert.AreEqual ("PrimaryKeyColumn2", t.Columns[3].Name, "#F1-1");

			Assert.IsNotNull (t.Columns[4], "#G1");
			Assert.AreEqual ("PrimaryKeyColumn3", t.Columns[4].Name, "#G1-1");

			Assert.IsNotNull (t.Columns[5], "#H1");
			Assert.AreEqual ("ForeignKeyColumn1", t.Columns[5].Name, "#H1-1");

			Assert.IsNotNull (t.Columns[6], "#I1");
			Assert.AreEqual ("ForeignKeyColumn2", t.Columns[6].Name, "#I1-1");

			Assert.IsNotNull (t.Columns[7], "#J1");
			Assert.AreEqual ("ForeignKeyColumn3", t.Columns[7].Name, "#J1-1");
		}

		[Test]
		public void CreateContext ()
		{
			MetaTable t = MetaModel.Default.Tables [TableFooWithDefaults];
			object context = t.CreateContext ();

			Assert.IsNotNull (context, "#A1");
			Assert.AreEqual (typeof (FooWithDefaults), context.GetType (), "#A2");

			var dataContext = context as FooWithDefaults;
			Assert.AreEqual ("hello", dataContext.Column1, "#B1");
			Assert.AreEqual (123, dataContext.Column2, "#B2");
		}

		[Test]
		public void DataContextPropertyName ()
		{
			MetaModel m = MetaModel.Default;

			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();
			routes.Add (
			    new DynamicDataRoute ("{table}/{action}.aspx") {
				    Constraints = new RouteValueDictionary (new { action = "List|Details|Edit|Insert" }),
				    Model = m,
				    RouteHandler = new MyDynamicDataRouteHandler ()
			    });

			MetaTable t = m.Tables[TableFooEmpty];
			Assert.AreEqual ("FooEmptyTable", t.DataContextPropertyName, "#A1");
			Assert.AreEqual (t.Name, t.DataContextPropertyName, "#A2");
		}

		[Test]
		public void DataContextType ()
		{
			MetaModel m = MetaModel.Default;

			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();
			routes.Add (
			    new DynamicDataRoute ("{table}/{action}.aspx") {
				    Constraints = new RouteValueDictionary (new { action = "List|Details|Edit|Insert" }),
				    Model = m,
				    RouteHandler = new MyDynamicDataRouteHandler ()
			    });

			MetaTable t = m.Tables[TableFooEmpty];
			Assert.IsTrue (t.DataContextType == typeof (FooEmpty), "#A1");
		}

		[Test]
		public void DisplayColumn ()
		{
			MetaModel m = MetaModel.Default;

			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();
			routes.Add (
			    new DynamicDataRoute ("{table}/{action}.aspx") {
				    Constraints = new RouteValueDictionary (new { action = "List|Details|Edit|Insert" }),
				    Model = m,
				    RouteHandler = new MyDynamicDataRouteHandler ()
			    });

			MetaTable t = m.Tables[TableFooDisplayColumnAttribute];
			MetaColumn mc = t.DisplayColumn;

			Assert.IsNotNull (mc, "#A1");
			Assert.AreEqual ("Column2", mc.Name, "#A2");

			t = m.Tables[TableFooEmpty];
			AssertExtensions.Throws <ArgumentOutOfRangeException> (() => mc = t.DisplayColumn, "#B1");

			t = m.Tables[TableFooWithDefaults];
			mc = t.DisplayColumn;
			Assert.IsNotNull (mc, "#C1");
			Assert.AreEqual ("Column1", mc.Name, "C2");

			t = m.Tables[TableBaz];
			mc = t.DisplayColumn;
			Assert.IsNotNull (mc, "#D1");
			Assert.AreEqual ("PrimaryKeyColumn2", mc.Name, "#D2");

			t = m.Tables[TableBazNoStrings];
			mc = t.DisplayColumn;
			Assert.IsNotNull (mc, "#E1");
			Assert.AreEqual ("PrimaryKeyColumn1", mc.Name, "#E2");

			t = m.Tables[TableBazNoStringsNoPrimary];
			mc = t.DisplayColumn;
			Assert.IsNotNull (mc, "#F1");
			Assert.AreEqual ("Column1", mc.Name, "#F2");

			t = m.Tables[TableFooInvalidDisplayColumnAttribute];
			AssertExtensions.Throws<InvalidOperationException> (() => mc = t.DisplayColumn, "#G1");
			t = m.Tables[TableFooEmptyDisplayColumnAttribute];
			AssertExtensions.Throws<InvalidOperationException> (() => mc = t.DisplayColumn, "#G2");
		}

		[Test]
		public void DisplayName ()
		{
			MetaModel m = MetaModel.Default;

			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();
			routes.Add (
			    new DynamicDataRoute ("{table}/{action}.aspx") {
				    Constraints = new RouteValueDictionary (new { action = "List|Details|Edit|Insert" }),
				    Model = m,
				    RouteHandler = new MyDynamicDataRouteHandler ()
			    });

			MetaTable t = m.Tables[TableFooWithDefaults];
			Assert.AreEqual ("FooWithDefaultsTable", t.DisplayName, "#A1");

			t = m.Tables[TableFooDisplayName];
			Assert.AreEqual ("My name is FooDisplayName, and I am friendly", t.DisplayName, "#B1");

			t = m.Tables[TableFooDisplayNameEmptyName];
			Assert.AreEqual (String.Empty, t.DisplayName, "#C1");
		}

		[Test]
		public void EntityType ()
		{
			MetaModel m = MetaModel.Default;

			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();
			routes.Add (
			    new DynamicDataRoute ("{table}/{action}.aspx") {
				    Constraints = new RouteValueDictionary (new { action = "List|Details|Edit|Insert" }),
				    Model = m,
				    RouteHandler = new MyDynamicDataRouteHandler ()
			    });

			MetaTable t = m.Tables[TableFooWithDefaults];
			Assert.IsTrue (t.EntityType == typeof (FooWithDefaults), "#A1");

			t = m.Tables[TableFooDisplayName];
			Assert.IsTrue (t.EntityType == typeof (FooDisplayName), "#B1");

			t = m.Tables[TableFooDisplayNameEmptyName];
			Assert.IsTrue (t.EntityType == typeof (FooDisplayNameEmptyName), "#C1");
		}

		[Test]
		[Ignore ("Does not work - for some reason the ForeignKeyColumn* columns aren't seen as part of foreign key.")]
		public void ForeignKeyColumnNames ()
		{
			MetaModel m = MetaModel.Default;

			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();
			routes.Add (
			    new DynamicDataRoute ("{table}/{action}.aspx") {
				    Constraints = new RouteValueDictionary (new { action = "List|Details|Edit|Insert" }),
				    Model = m,
				    RouteHandler = new MyDynamicDataRouteHandler ()
			    });

			MetaTable t = m.Tables[TableFooWithDefaults];
			Assert.IsNotNull (t.ForeignKeyColumnsNames, "#A1");
			Assert.IsFalse (t.ForeignKeyColumnsNames.Length == 0, "#A2");
			Assert.AreEqual (String.Empty, t.ForeignKeyColumnsNames, "#A3");
		}

		[Test]
		public void GetActionPath_Action ()
		{
			MetaModel m = MetaModel.Default;

			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();
			routes.Add (
			    new DynamicDataRoute ("{table}/{action}.aspx") {
				    Constraints = new RouteValueDictionary (new { action = "List|Details|Edit|Insert" }),
				    Model = m,
				    RouteHandler = new MyDynamicDataRouteHandler ()
			    });

			MetaTable t = m.Tables [TableFooWithDefaults];

			Assert.AreEqual (String.Empty, t.GetActionPath (null), "#A1");
			Assert.AreEqual (String.Empty, t.GetActionPath (String.Empty), "#A2");
			Assert.AreEqual (String.Empty, t.GetActionPath ("SomeInvalidValue"), "#A3");
		}

		[Test]
		public void GetActionPath_Action_2 ()
		{
			MetaModel m = MetaModel.Default;

			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();
			routes.Add (
			    new DynamicDataRoute ("{table}/{action}.aspx") {
				    Constraints = new RouteValueDictionary (new { action = "List|Details|Edit|Insert" }),
				    Model = m,
				    RouteHandler = new MyDynamicDataRouteHandler ()
			    });

			MetaTable t = m.Tables [TableFooWithDefaults];

			Assert.AreEqual (Utils.BuildActionName (t, PageAction.Details), t.GetActionPath (PageAction.Details), "#A1");
			Assert.AreEqual (Utils.BuildActionName (t, PageAction.Edit), t.GetActionPath (PageAction.Edit), "#A2");
			Assert.AreEqual (Utils.BuildActionName (t, PageAction.Insert), t.GetActionPath (PageAction.Insert), "#A3");
			Assert.AreEqual (Utils.BuildActionName (t, PageAction.List), t.GetActionPath (PageAction.List), "#A4");
		}

		[Test]
		public void GetActionPath_Action_3 ()
		{
			MetaModel m = MetaModel.Default;

			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();
			routes.Add (new DynamicDataRoute ("{table}/ListDetails.aspx") {
				Action = PageAction.List,
				ViewName = "ListDetails",
				Model = m,
				RouteHandler = new MyDynamicDataRouteHandler ()
			});

			routes.Add (new DynamicDataRoute ("{table}/ListDetails.aspx") {
				Action = PageAction.Details,
				ViewName = "ListDetails",
				Model = m,
				RouteHandler = new MyDynamicDataRouteHandler ()
			});

			MetaTable t = m.Tables [TableFooWithDefaults];
			Assert.AreEqual (t.Model, m, "#A0");
			Assert.AreEqual (Utils.BuildActionName (t, "ListDetails"), t.GetActionPath (PageAction.Details), "#A1");
			Assert.AreEqual (Utils.BuildActionName (t, "ListDetails"), t.GetActionPath (PageAction.List), "#A2");

			// Missing routes
			Assert.AreEqual (String.Empty, t.GetActionPath (PageAction.Edit), "#A3");
			Assert.AreEqual (String.Empty, t.GetActionPath (PageAction.Insert), "#A4");

			// Add routes for the two above tests
			routes.Add (new DynamicDataRoute ("{table}/EditInsert.aspx") {
				Action = PageAction.Edit,
				ViewName = "MyEditInsert",
				Model = m,
				RouteHandler = new MyDynamicDataRouteHandler ()
			});

			routes.Add (new DynamicDataRoute ("{table}/InsertEdit.aspx") {
				Action = PageAction.Insert,
				ViewName = "MyEditInsert",
				Model = m,
				RouteHandler = new MyDynamicDataRouteHandler ()
			});

			Assert.AreEqual (Utils.BuildActionName (t, "ListDetails"), t.GetActionPath (PageAction.Details), "#B1");
			Assert.AreEqual (Utils.BuildActionName (t, "ListDetails"), t.GetActionPath (PageAction.List), "#B2");

			Assert.AreEqual (Utils.BuildActionName (t, "EditInsert"), t.GetActionPath (PageAction.Edit), "#B3");
			Assert.AreEqual (Utils.BuildActionName (t, "InsertEdit"), t.GetActionPath (PageAction.Insert), "#B4");
		}

		[Test]
		public void PrimaryKeyColumns ()
		{
			MetaModel m = MetaModel.Default;

			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();
			var route = new MyDynamicDataRoute ("{table}/{action}.aspx") {
				Constraints = new RouteValueDictionary (new { action = "List|Details|Edit|Insert" }),
				Model = m,
				RouteHandler = new MyDynamicDataRouteHandler ()
			};
			routes.Add (route);

			MetaTable t = m.Tables [TableFooWithDefaults];

			Assert.AreEqual (3, t.PrimaryKeyColumns.Count, "#A1");
			Assert.AreEqual ("PrimaryKeyColumn1", t.PrimaryKeyColumns[0].Name, "#A2");
			Assert.IsTrue (t.PrimaryKeyColumns[0].ColumnType == typeof (string), "#A2-1");
			Assert.AreEqual ("PrimaryKeyColumn2", t.PrimaryKeyColumns[1].Name, "#A3");
			Assert.IsTrue (t.PrimaryKeyColumns[1].ColumnType == typeof (int), "#A3-1");
			Assert.AreEqual ("PrimaryKeyColumn3", t.PrimaryKeyColumns[2].Name, "#A4");
			Assert.IsTrue (t.PrimaryKeyColumns[2].ColumnType == typeof (bool), "#A4-1");
		}

		[Test]
		public void GetActionPath_Action_PrimaryKeyValues ()
		{
			MetaModel m = MetaModel.Default;

			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();
			var route = new MyDynamicDataRoute ("{table}/{action}.aspx") {
				Constraints = new RouteValueDictionary (new { action = "List|Details|Edit|Insert" }),
				Model = m,
				RouteHandler = new MyDynamicDataRouteHandler ()
			};
			routes.Add (route);

			MetaTable t = m.Tables [TableFooWithDefaults];
			Assert.AreEqual (String.Empty, t.GetActionPath (null, (IList<object>) null), "#A1");
			Assert.AreEqual (String.Empty, t.GetActionPath (String.Empty, (IList<object>) null), "#A2");
			Assert.AreEqual (String.Empty, t.GetActionPath ("BogusValue", (IList<object>) null), "#A3");
		}

		[Test]
		public void GetActionPath_Action_PrimaryKeyValues_2 ()
		{
			MetaModel m = MetaModel.Default;

			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();
			var route = new MyDynamicDataRoute ("{table}/{action}.aspx") {
				Constraints = new RouteValueDictionary (new { action = "List|Details|Edit|Insert" }),
				Model = m,
				RouteHandler = new MyDynamicDataRouteHandler ()
			};
			routes.Add (route);

			MetaTable t = m.Tables [TableFooWithDefaults];
			Assert.AreEqual (Utils.BuildActionName (t, PageAction.Details), t.GetActionPath (PageAction.Details, (IList<object>) null), "#A1");

			// check the contents of the passed values dictionary
			//
			Assert.AreEqual (2, route.GetVirtualPathValues.Count, "#B1");
			Assert.IsTrue (route.GetVirtualPathValues.ContainsKey ("Action"), "#B1-1");
			Assert.AreEqual (PageAction.Details, route.GetVirtualPathValues["Action"], "#B1-2");
			Assert.IsTrue (route.GetVirtualPathValues.ContainsKey ("Table"), "#B1-3");
			Assert.AreEqual (t.Name, route.GetVirtualPathValues["Table"], "#B1-4");
		}

		[Test]
		public void GetActionPath_Action_PrimaryKeyValues_3 ()
		{
			MetaModel m = MetaModel.Default;

			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();
			var route = new MyDynamicDataRoute ("{table}/{action}.aspx") {
				Constraints = new RouteValueDictionary (new { action = "List|Details|Edit|Insert" }),
				Model = m,
				RouteHandler = new MyDynamicDataRouteHandler ()
			};
			routes.Add (route);

			MetaTable t = m.Tables [TableFooWithDefaults];

			Assert.AreEqual (Utils.BuildActionName (t, PageAction.Details), t.GetActionPath (PageAction.Details, (IList<object>) null), "#A1");
			 
			var dataList = new List<object> ();
			dataList.Add ("first item");

			// Yet another lack of parameter checking - the number of items passed in the dataList must be at least equal
			// to the number of columns in the PrimaryKeyColumns collection
			AssertExtensions.Throws<ArgumentOutOfRangeException> (() => t.GetActionPath (PageAction.Details, dataList), "#A2");

			dataList.Add (2);
			dataList.Add (false);
			Assert.AreEqual (Utils.BuildActionName (t, PageAction.Details , "PrimaryKeyColumn1=first%20item&PrimaryKeyColumn2=2&PrimaryKeyColumn3=False"), t.GetActionPath (PageAction.Details, dataList), "#A3");

			dataList.Clear ();
			dataList.Add (false);
			dataList.Add ("item");
			dataList.Add (5432);
			// Not even close to correct behavior, but that's how it behaves...
			Assert.AreEqual (Utils.BuildActionName (t, PageAction.Details, "PrimaryKeyColumn1=False&PrimaryKeyColumn2=item&PrimaryKeyColumn3=5432"), t.GetActionPath (PageAction.Details, dataList), "#A4");

			// check the contents of the passed values dictionary
			//
			Assert.AreEqual (5, route.GetVirtualPathValues.Count, "#B1");
			Assert.IsTrue (route.GetVirtualPathValues.ContainsKey ("Action"), "#B1-1");
			Assert.AreEqual (PageAction.Details, route.GetVirtualPathValues["Action"], "#B1-2");
			Assert.IsTrue (route.GetVirtualPathValues.ContainsKey ("Table"), "#B1-3");
			Assert.AreEqual (t.Name, route.GetVirtualPathValues["Table"], "#B1-4");
		}

		[Test]
		public void GetActionPath_Action_RouteValues ()
		{
			MetaModel m = MetaModel.Default;

			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();
			var route = new MyDynamicDataRoute ("{table}/{action}.aspx") {
				Constraints = new RouteValueDictionary (new { action = "List|Details|Edit|Insert" }),
				Model = m,
				RouteHandler = new MyDynamicDataRouteHandler ()
			};
			routes.Add (route);

			MetaTable t = m.Tables [TableFooWithDefaults];
			var values = new RouteValueDictionary ();

			// NO null check for the routeValues parameter _again_!
			AssertExtensions.Throws<NullReferenceException> (() => t.GetActionPath (PageAction.Details, (RouteValueDictionary) null), "#A1");
			Assert.AreEqual (Utils.BuildActionName (t, PageAction.Details), t.GetActionPath (PageAction.Details, values), "#A2");
			Assert.AreEqual (2, values.Count, "#A3");

			// GetActionPath does not check if the Action and Table keys are present in the dictionary...
			values.Clear ();
			values.Add ("Action", "something");
			AssertExtensions.Throws<ArgumentException> (() => {
				t.GetActionPath (PageAction.Details, values);
			}, "#B1");

			values.Clear ();
			values.Add ("Table", "else");
			AssertExtensions.Throws<ArgumentException> (() => {
				t.GetActionPath (PageAction.Details, values);
			}, "#B2");
		}

		[Test]
		public void GetActionPath_Action_Row ()
		{
			MetaModel m = MetaModel.Default;

			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();
			var route = new MyDynamicDataRoute ("{table}/{action}.aspx") {
				Constraints = new RouteValueDictionary (new { action = "List|Details|Edit|Insert" }),
				Model = m,
				RouteHandler = new MyDynamicDataRouteHandler ()
			};
			routes.Add (route);

			MetaTable t = m.Tables [TableFooWithDefaults];

			var foo = new FooWithDefaults ();
			Assert.AreEqual (String.Empty, t.GetActionPath (null, (object) null), "#A1");
			AssertExtensions.Throws<HttpException> (() => t.GetActionPath (PageAction.Details, (object) "test"), "#A2");
			Assert.AreEqual (Utils.BuildActionName (t, PageAction.Details, "PrimaryKeyColumn1=primary%20key%20value&PrimaryKeyColumn2=456&PrimaryKeyColumn3=True"), t.GetActionPath (PageAction.Details, foo), "#A3");

			t = m.Tables [TableFooNoDefaultsWithPrimaryKey];
			var foo2 = new FooNoDefaultsWithPrimaryKey ();
			Assert.AreEqual (Utils.BuildActionName (t, PageAction.Details), t.GetActionPath (PageAction.Details, foo2), "#B1");
		}

		[Test]
		public void GetActionPath_Action_Row_Path ()
		{
			MetaModel m = MetaModel.Default;

			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();
			var route = new MyDynamicDataRoute ("{table}/{action}.aspx") {
				Constraints = new RouteValueDictionary (new { action = "List|Details|Edit|Insert" }),
				Model = m,
				RouteHandler = new MyDynamicDataRouteHandler ()
			};
			routes.Add (route);

			MetaTable t = m.Tables [TableFooWithDefaults];

			var foo = new FooWithDefaults ();
			Assert.AreEqual (String.Empty, t.GetActionPath (null, (object) null, null), "#A1");
			Assert.AreEqual (String.Empty, t.GetActionPath (null, (object) null, String.Empty), "#A2");
			Assert.AreEqual (Utils.BuildActionName (t, PageAction.Details, "PrimaryKeyColumn1=primary%20key%20value&PrimaryKeyColumn2=456&PrimaryKeyColumn3=True"), t.GetActionPath (PageAction.Details, foo, null), "#A3");
			Assert.AreEqual ("~/SomePath.aspx", t.GetActionPath (null, (object) null, "~/SomePath.aspx"), "#A4");
			Assert.AreEqual ("~/SomePath.aspx?PrimaryKeyColumn1=primary%20key%20value&PrimaryKeyColumn2=456&PrimaryKeyColumn3=True", t.GetActionPath (null, foo, "~/SomePath.aspx"), "#A5");
			Assert.AreEqual ("~/SomePath.aspx", t.GetActionPath (PageAction.Details, (object) null, "~/SomePath.aspx"), "#A6");
			Assert.AreEqual ("~/SomePath.aspx?PrimaryKeyColumn1=primary%20key%20value&PrimaryKeyColumn2=456&PrimaryKeyColumn3=True", t.GetActionPath (PageAction.Details, foo, "~/SomePath.aspx"), "#A7");
			Assert.AreEqual (Utils.BuildActionName (t, PageAction.Details), t.GetActionPath (PageAction.Details, (object) null, null), "#A8");
			Assert.AreEqual (Utils.BuildActionName (t, PageAction.Details, "PrimaryKeyColumn1=primary%20key%20value&PrimaryKeyColumn2=456&PrimaryKeyColumn3=True"), t.GetActionPath (PageAction.Details, foo, null), "#A9");
			Assert.AreEqual (Utils.BuildActionName (t, PageAction.Details), t.GetActionPath (PageAction.Details, (object) null, String.Empty), "#A10");
			Assert.AreEqual (Utils.BuildActionName (t, PageAction.Details, "PrimaryKeyColumn1=primary%20key%20value&PrimaryKeyColumn2=456&PrimaryKeyColumn3=True"), t.GetActionPath (PageAction.Details, foo, String.Empty), "#A11");
		}

		[Test]
		public void GetActionPath_Action_PrimaryKeyValues_Path ()
		{
			MetaModel m = MetaModel.Default;

			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();
			var route = new MyDynamicDataRoute ("{table}/{action}.aspx") {
				Constraints = new RouteValueDictionary (new { action = "List|Details|Edit|Insert" }),
				Model = m,
				RouteHandler = new MyDynamicDataRouteHandler ()
			};
			routes.Add (route);

			MetaTable t = m.Tables[TableFooWithDefaults];

			Assert.AreEqual (String.Empty, t.GetActionPath (null, (IList<object>) null, null), "#A1");
			Assert.AreEqual (String.Empty, t.GetActionPath (null, (IList<object>) null, String.Empty), "#A2");

			var dataList = new List<object> ();
			dataList.Add ("first item");
			
			// Yet another lack of parameter checking - the number of items passed in the dataList must be at least equal
			// to the number of columns in the PrimaryKeyColumns collection
			AssertExtensions.Throws<ArgumentOutOfRangeException> (() => t.GetActionPath (PageAction.Details, dataList), "#A3");

			dataList.Add (2);
			dataList.Add (false);
			
			Assert.AreEqual (Utils.BuildActionName (t, PageAction.Details, "PrimaryKeyColumn1=first%20item&PrimaryKeyColumn2=2&PrimaryKeyColumn3=False"), t.GetActionPath (PageAction.Details, dataList, null), "#A4");
			Assert.AreEqual ("~/SomePath.aspx", t.GetActionPath (null, (IList <object>) null, "~/SomePath.aspx"), "#A5");
			Assert.AreEqual ("~/SomePath.aspx?PrimaryKeyColumn1=first%20item&PrimaryKeyColumn2=2&PrimaryKeyColumn3=False", t.GetActionPath (null, dataList, "~/SomePath.aspx"), "#A6");
			Assert.AreEqual ("~/SomePath.aspx", t.GetActionPath (PageAction.Details, (IList <object>) null, "~/SomePath.aspx"), "#A7");
			Assert.AreEqual ("~/SomePath.aspx?PrimaryKeyColumn1=first%20item&PrimaryKeyColumn2=2&PrimaryKeyColumn3=False", t.GetActionPath (PageAction.Details, dataList, "~/SomePath.aspx"), "#A8");
			Assert.AreEqual (Utils.BuildActionName (t, PageAction.Details), t.GetActionPath (PageAction.Details, (IList <object>) null, null), "#A9");
			Assert.AreEqual (Utils.BuildActionName (t, PageAction.Details, "PrimaryKeyColumn1=first%20item&PrimaryKeyColumn2=2&PrimaryKeyColumn3=False"), t.GetActionPath (PageAction.Details, dataList, null), "#A10");
			Assert.AreEqual (Utils.BuildActionName (t, PageAction.Details), t.GetActionPath (PageAction.Details, (IList <object>) null, String.Empty), "#A11");
			Assert.AreEqual (Utils.BuildActionName (t, PageAction.Details, "PrimaryKeyColumn1=first%20item&PrimaryKeyColumn2=2&PrimaryKeyColumn3=False"), t.GetActionPath (PageAction.Details, dataList, String.Empty), "#A12");
		}

		[Test]
		public void GetColumn ()
		{
			MetaModel m = MetaModel.Default;

			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();
			var route = new MyDynamicDataRoute ("{table}/{action}.aspx") {
				Constraints = new RouteValueDictionary (new { action = "List|Details|Edit|Insert" }),
				Model = m,
				RouteHandler = new MyDynamicDataRouteHandler ()
			};
			routes.Add (route);

			MetaTable t = m.Tables[TableFooWithDefaults];

			AssertExtensions.Throws<ArgumentNullException> (() => t.GetColumn (null), "#A1");
			AssertExtensions.Throws<InvalidOperationException> (() => t.GetColumn (String.Empty), "#A2");
			AssertExtensions.Throws<InvalidOperationException> (() => t.GetColumn ("NoSuchColumn"), "#A3");

			MetaColumn mc = t.GetColumn ("Column1");
			Assert.IsNotNull (mc, "#B1");
			Assert.AreEqual ("Column1", mc.Name, "#B1-2");
		}

		[Test]
		public void GetDisplayString ()
		{
			MetaModel m = MetaModel.Default;

			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();
			var route = new MyDynamicDataRoute ("{table}/{action}.aspx") {
				Constraints = new RouteValueDictionary (new { action = "List|Details|Edit|Insert" }),
				Model = m,
				RouteHandler = new MyDynamicDataRouteHandler ()
			};
			routes.Add (route);

			MetaTable t = m.Tables[TableFooWithDefaults];
			var foo = new FooWithDefaults ();

			Assert.AreEqual (String.Empty, t.GetDisplayString (null), "#A1");
			AssertExtensions.Throws <HttpException> (() => t.GetDisplayString (String.Empty), "#A2");
			Assert.AreEqual ("hello", t.GetDisplayString (foo), "#A3");
			AssertExtensions.Throws <HttpException> (() => t.GetDisplayString ("TestString"), "#A4");

			// The method looks at the entity type to see if it has an overriden ToString method, 
			// it ignores such methods on the passed "row"
			var foo2 = new FooWithToString ();
			Assert.AreEqual ("hello", t.GetDisplayString (foo2), "#B1");

			t = m.Tables[TableFooWithToString];
			Assert.AreEqual ("ValueFrom_ToString", t.GetDisplayString (foo2), "#C1");

			// If we pass an object which is not of EntityType, 
			// the method returns the result of row.ToString ()
			Assert.AreEqual (foo.GetType ().ToString (), t.GetDisplayString (foo), "#C2");

			var foo3 = new FooNoDefaultsWithPrimaryKey ();
			t = m.Tables[TableFooNoDefaultsWithPrimaryKey];
			Assert.AreEqual (String.Empty, t.GetDisplayString (foo3), "#D1");
		}

		[Test]
		public void GetPrimaryKeyString_PrimaryKeyValues ()
		{
			MetaModel m = MetaModel.Default;

			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();
			var route = new MyDynamicDataRoute ("{table}/{action}.aspx") {
				Constraints = new RouteValueDictionary (new { action = "List|Details|Edit|Insert" }),
				Model = m,
				RouteHandler = new MyDynamicDataRouteHandler ()
			};
			routes.Add (route);

			MetaTable t = m.Tables[TableFooWithDefaults];
			var values = new List<object> ();

			Assert.AreEqual (String.Empty, t.GetPrimaryKeyString ((IList<object>) null), "#A1");
			Assert.AreEqual (String.Empty, t.GetPrimaryKeyString (values), "#A2");

			values.Add ("string");
			Assert.AreEqual ("string", t.GetPrimaryKeyString (values), "#B1");

			values.Add (123);
			Assert.AreEqual ("string,123", t.GetPrimaryKeyString (values), "#B2");

			values.Add (false);
			Assert.AreEqual ("string,123,False", t.GetPrimaryKeyString (values), "#B3");

			values.Add (true);
			Assert.AreEqual ("string,123,False,True", t.GetPrimaryKeyString (values), "#B4");

			values.Clear ();
			values.Add (false);
			values.Add ("string");
			values.Add (123);

			Assert.AreEqual ("False,string,123", t.GetPrimaryKeyString (values), "#C1");

			values.Add (null);
			Assert.AreEqual ("False,string,123,", t.GetPrimaryKeyString (values), "#C2");

			values.Add (null);
			values.Add ("another string");
			Assert.AreEqual ("False,string,123,,,another string", t.GetPrimaryKeyString (values), "#C3");

			values.Clear ();
			values.Add (null);
			Assert.AreEqual (String.Empty, t.GetPrimaryKeyString (values), "#D1");

			values.Add (null);
			Assert.AreEqual (String.Empty, t.GetPrimaryKeyString (values), "#D2");

			values.Add (String.Empty);
			Assert.AreEqual (",,", t.GetPrimaryKeyString (values), "#D3");

			values.Add (null);
			Assert.AreEqual (",,,", t.GetPrimaryKeyString (values), "#D4");
		}

		[Test]
		public void GetPrimaryKeyString_Row ()
		{
			MetaModel m = MetaModel.Default;

			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();
			var route = new MyDynamicDataRoute ("{table}/{action}.aspx") {
				Constraints = new RouteValueDictionary (new { action = "List|Details|Edit|Insert" }),
				Model = m,
				RouteHandler = new MyDynamicDataRouteHandler ()
			};
			routes.Add (route);

			MetaTable t = m.Tables[TableFooWithDefaults];
			var foo = new FooWithDefaults ();

			Assert.AreEqual (String.Empty, t.GetPrimaryKeyString ((object) null), "#A1");
			Assert.AreEqual ("primary key value,456,True", t.GetPrimaryKeyString (foo), "#A2");

			var foo2 = new FooNoDefaultsWithPrimaryKey ();
			AssertExtensions.Throws <HttpException> (() => t.GetPrimaryKeyString (foo2), "#B1");

			t = m.Tables[TableFooSettableDefaults];
			var foo3 = new FooSettableDefaults (null, null, null);
			Assert.AreEqual (String.Empty, t.GetPrimaryKeyString (foo3), "#C1");

			foo3 = new FooSettableDefaults (null, String.Empty, null);
			Assert.AreEqual (",,", t.GetPrimaryKeyString (foo3), "#C2");

			foo3 = new FooSettableDefaults (String.Empty, null, null);
			Assert.AreEqual (",,", t.GetPrimaryKeyString (foo3), "#C2");
		}

		[Test]
		public void GetPrimaryKeyValues ()
		{
			MetaModel m = MetaModel.Default;

			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();
			var route = new MyDynamicDataRoute ("{table}/{action}.aspx") {
				Constraints = new RouteValueDictionary (new { action = "List|Details|Edit|Insert" }),
				Model = m,
				RouteHandler = new MyDynamicDataRouteHandler ()
			};
			routes.Add (route);

			MetaTable t = m.Tables [TableFooWithDefaults];
			var foo = new FooWithDefaults ();

			Assert.IsNull (t.GetPrimaryKeyValues (null), "#A1");
			AssertExtensions.Throws<HttpException> (() => t.GetPrimaryKeyValues ("test"), "#A2");

			IList<object> ret = t.GetPrimaryKeyValues (foo);
			Assert.IsNotNull (ret, "#B1");
			Assert.AreEqual (3, ret.Count, "#B2");
			Assert.IsNotNull (ret[0], "#B2-1");
			Assert.IsTrue (ret[0] is string, "#B2-2");
			Assert.AreEqual ("primary key value", ret[0], "#B2-3");
			Assert.IsNotNull (ret[1], "#B2-4");
			Assert.IsTrue (ret[1] is int, "#B2-5");
			Assert.AreEqual (456, ret[1], "#B2-6");
			Assert.IsNotNull (ret[2], "#B2-7");
			Assert.IsTrue (ret[2] is bool, "#B2-8");
			Assert.AreEqual (true, ret[2], "#B2-9");

			t = m.Tables [TableFooNoPrimaryColumns];
			var foo2 = new FooNoPrimaryColumns ();
			ret = t.GetPrimaryKeyValues (foo2);
			Assert.IsNotNull (ret, "#C1");
			Assert.AreEqual (0, ret.Count, "#C2");
		}

		[Test]
		public void GetQuery ()
		{
			MetaModel m = Utils.GetModel<MyDataContext2> ();

			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();
			var route = new MyDynamicDataRoute ("{table}/{action}.aspx") {
				Constraints = new RouteValueDictionary (new { action = "List|Details|Edit|Insert" }),
				Model = m,
				RouteHandler = new MyDynamicDataRouteHandler ()
			};
			routes.Add (route);

			MetaTable t = m.GetTable ("FooTable"); ;
			IQueryable query = t.GetQuery ();
			Assert.IsNotNull (query, "#A1");
			Assert.IsTrue (query.GetType () == typeof (Table<Foo>), "#A2");
		}

		[Test]
		public void GetQuery_Context ()
		{
			MetaModel m = Utils.GetModel<MyDataContext2> ();

			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();
			var route = new MyDynamicDataRoute ("{table}/{action}.aspx") {
				Constraints = new RouteValueDictionary (new { action = "List|Details|Edit|Insert" }),
				Model = m,
				RouteHandler = new MyDynamicDataRouteHandler ()
			};
			routes.Add (route);

			MetaTable t = m.GetTable ("FooTable"); ;
			IQueryable query = t.GetQuery (null);
			Assert.IsNotNull (query, "#A1");
			Assert.IsTrue (query.GetType () == typeof (Table<Foo>), "#A2");

			var foo = new Foo (true);
			AssertExtensions.Throws <TargetException> (() => t.GetQuery (foo), "#B1");
		}

		[Test]
		public void HasPrimaryKey ()
		{
			MetaModel m = MetaModel.Default;

			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();
			routes.Add (
			    new DynamicDataRoute ("{table}/{action}.aspx") {
				    Constraints = new RouteValueDictionary (new { action = "List|Details|Edit|Insert" }),
				    Model = m,
				    RouteHandler = new MyDynamicDataRouteHandler ()
			    });

			MetaTable t = m.Tables[TableFooWithDefaults];
			Assert.IsTrue (t.HasPrimaryKey, "#A1");

			t = m.Tables[TableFooNoPrimaryColumns];
			Assert.IsFalse (t.HasPrimaryKey, "#A2");
		}

		[Test]
		public void TryGetColumn ()
		{
			MetaModel m = MetaModel.Default;

			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();
			var route = new MyDynamicDataRoute ("{table}/{action}.aspx") {
				Constraints = new RouteValueDictionary (new { action = "List|Details|Edit|Insert" }),
				Model = m,
				RouteHandler = new MyDynamicDataRouteHandler ()
			};
			routes.Add (route);

			MetaTable t = m.Tables[TableFooWithDefaults];
			MetaColumn mc = null;

			AssertExtensions.Throws<ArgumentNullException> (() => t.TryGetColumn (null, out mc), "#A1");
			Assert.IsFalse (t.TryGetColumn (String.Empty, out mc), "#A2");
			Assert.IsNull (mc, "#A2-1");
			Assert.IsTrue (t.TryGetColumn ("Column1", out mc), "#A3");
			Assert.IsNotNull (mc, "#A3-1");
			Assert.AreEqual ("Column1", mc.Name, "#A3-2");
		}
	}
}
