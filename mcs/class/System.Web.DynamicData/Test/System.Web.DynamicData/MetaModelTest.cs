//
// MetaModelTest.cs
//
// Authors:
//      Atsushi Enomoto <atsushi@ximian.com>
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2008-2009 Novell Inc. http://novell.com
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
using System.Reflection;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web;
using System.Web.UI;
using System.Web.DynamicData;
using System.Web.DynamicData.ModelProviders;
using System.Web.Routing;

using NUnit.Framework;
using MonoTests.stand_alone.WebHarness;
using MonoTests.SystemWeb.Framework;
using MonoTests.Common;

using MetaModel = System.Web.DynamicData.MetaModel;
using MetaTable = System.Web.DynamicData.MetaTable;

namespace MonoTests.System.Web.DynamicData
{
	[TestFixture]
	public class MetaModelTest
	{
		static MetaModel defaultModel;

		static MetaModelTest ()
		{
			defaultModel = new MetaModel ();
		}

		[Test]
		public void DefaultValues ()
		{
			var model = new MetaModel ();

			Assert.IsNotNull (MetaModel.Default, "#A1");

			// We can't be sure which model will be the default one when running under Nunit
			//Assert.IsTrue (MetaModel.Default == defaultModel, "#A2");
			Assert.IsNotNull (model.Tables, "#A3");
			Assert.IsNotNull (model.VisibleTables, "#A4");
			Assert.IsNotNull (model.FieldTemplateFactory, "#A5");
			Assert.AreEqual ("~/DynamicData/", model.DynamicDataFolderVirtualPath, "#A6");

			Assert.AreEqual (0, model.VisibleTables.Count, "#B1");
			Assert.AreEqual (0, model.Tables.Count, "#B2");
			Assert.AreEqual (typeof (FieldTemplateFactory), model.FieldTemplateFactory.GetType (), "#B3");
		}

		[Test]
		public void DynamicDataFolderVirtualPath ()
		{
			var model = new MetaModel ();

			Assert.AreEqual ("~/DynamicData/", model.DynamicDataFolderVirtualPath, "#A1");
			model.DynamicDataFolderVirtualPath = null;
			Assert.AreEqual ("~/DynamicData/", model.DynamicDataFolderVirtualPath, "#A2");
			model.DynamicDataFolderVirtualPath = String.Empty;
			Assert.AreEqual (String.Empty, model.DynamicDataFolderVirtualPath, "#A3");
			model.DynamicDataFolderVirtualPath = "~/FolderNoTrailingSlash";
			Assert.AreEqual ("~/FolderNoTrailingSlash/", model.DynamicDataFolderVirtualPath, "#A4");
			model.DynamicDataFolderVirtualPath = "AnotherFolder";
			Assert.AreEqual ("AnotherFolder/", model.DynamicDataFolderVirtualPath, "#A5");
			model.DynamicDataFolderVirtualPath = "/YetAnotherFolder";
			Assert.AreEqual ("/YetAnotherFolder/", model.DynamicDataFolderVirtualPath, "#A6");
			model.DynamicDataFolderVirtualPath = null;
			Assert.AreEqual ("~/DynamicData/", model.DynamicDataFolderVirtualPath, "#A7");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetTableNull ()
		{
			new MetaModel ().GetTable ((Type) null);
		}

		[Test]
		public void RegisterContext ()
		{
			var m = new MetaModel ();
			try {
				m.RegisterContext (typeof (Foo));
				Assert.Fail ("#1");
			} catch (TargetInvocationException ex) {
				Assert.AreEqual ("ERROR", ex.InnerException.Message, "#2");
			} finally {
				MetaModel.ResetRegistrationException ();
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void RegisterContext2 ()
		{
			try {
				var m = new MetaModel ();
				m.RegisterContext (typeof (Bar)); // not supported
			} finally {
				MetaModel.ResetRegistrationException ();
			}
		}

		[Test]
		[ExpectedException (typeof (MissingMethodException))]
		public void RegisterContext3 ()
		{
			var m = new MetaModel ();
			try {
				// no public constructor
				m.RegisterContext (typeof (DataContext));
			} finally {
				MetaModel.ResetRegistrationException ();
			}
		}

		[Test]
		public void RegisterContext4 ()
		{
			MetaModel m = Utils.GetModel<MyDataContext1> ();
			Assert.AreEqual (0, m.Tables.Count, "#1-1");
			Assert.AreEqual (0, m.VisibleTables.Count, "#1-2");
			Assert.IsNotNull (MetaModel.GetModel (typeof (MyDataContext1)), "#2");
		}

		[Test]
		public void RegisterContext5 ()
		{
			// In the process of several experiments (as the docs lack any good explanation),
			// I determined that this test needs the following for succesful completion:
			//
			//  - a worker request
			//  - a HttpContext
			//  - a fake route handler derived from DynamicDataRouteHandler, so that its CreateHandler
			//    returns a handler without attempting to actually find a View for the requested action
			//  - a route which can match table actions (taken from the skeleton DynamicData project
			//    generated by VisualStudio)
			//  - _empty_ query string returned from the fake worker request, or .NET will populate the
			//    HttpRequest.QueryString collection with one null item, and .NET's DynamicData will happily
			//    assume any entry in that collection is not null (it seems null checks aren't very popular
			//    in DynamicData code)
			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;
			MetaModel m = Utils.GetModel<MyDataContext2> ();

			RouteTable.Routes.Add (
			    new DynamicDataRoute ("{table}/{action}.aspx") {
				    Constraints = new RouteValueDictionary (new { action = "List|Details|Edit|Insert" }),
				    Model = m,
				    RouteHandler = new MyDynamicDataRouteHandler ()
			    });

			MetaTable t = m.Tables[0];

			Assert.AreEqual (1, m.Tables.Count, "#1-1");
			Assert.AreEqual (1, m.VisibleTables.Count, "#1-2");
			Assert.AreEqual (typeof (Foo), t.EntityType, "#1-3");

			// Those names are only the last part before '.' (i.e. without schema name).
			Assert.AreEqual ("FooTable", t.Name, "#2-1");
			Assert.AreEqual ("FooTable", t.DisplayName, "#2-2");
			Assert.AreEqual ("FooTable", t.DataContextPropertyName, "#2-3");
			Assert.AreEqual ("/FooTable/List.aspx", t.ListActionPath, "#2-4");

			Assert.AreEqual ("FooTable", t.Provider.Name, "#3-1");
		}

		[Test]
		public void ResetRegistrationException ()
		{
			MetaModel.ResetRegistrationException ();

			var m = new MetaModel ();
			try {
				m.RegisterContext (typeof (Foo));
				Assert.Fail ("#1");
			} catch (TargetInvocationException) {
			}

			try {
				m.RegisterContext (typeof (MyDataContext1));
				Assert.Fail ("#2");
			} catch (InvalidOperationException) {
			} finally {
				MetaModel.ResetRegistrationException ();
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetTableObject ()
		{
			// entity type 'System.Object' not found.
			new MetaModel ().GetTable (typeof (object));
		}

		[Test]
		public void GetModel ()
		{
			Utils.GetModel<UseOnlyInGetModelTestDataContext> ();
			Assert.Throws<ArgumentNullException> (() => MetaModel.GetModel (null), "#A1");
			Assert.Throws<InvalidOperationException> (() => MetaModel.GetModel (typeof (object)), "#A2");
			Assert.IsNotNull (MetaModel.GetModel (typeof (UseOnlyInGetModelTestDataContext)));
		}

		[Test]
		public void TryGetTable ()
		{
			MetaModel m = Utils.GetModel<MyDataContext2> ();
			MetaTable t;

			Assert.Throws<ArgumentNullException> (() => m.TryGetTable (null, out t), "#A1");

			Assert.IsTrue (m.TryGetTable ("FooTable", out t), "#B1");
			Assert.IsNotNull (t, "#B2");
			Assert.AreEqual (typeof (Foo), t.EntityType, "#B3");

			Assert.IsFalse (m.TryGetTable (String.Empty, out t), "#C1");
			Assert.IsNull (t, "#C2");
			Assert.IsFalse (m.TryGetTable ("NoSuchTable", out t), "#C3");
			Assert.IsNull (t, "#C4");
		}

		[Test]
		public void GetTable ()
		{
			MetaModel m = Utils.GetModel<MyDataContext2> ();
			MetaTable t;
			string str = null;
			Type type = null;

			Assert.Throws<ArgumentNullException> (() => t = m.GetTable (str), "#A1");
			Assert.Throws<ArgumentNullException> (() => t = m.GetTable (type), "#A2");
			Assert.Throws<ArgumentNullException> (() => t = m.GetTable (null, null), "#A3");
			Assert.Throws<ArgumentNullException> (() => t = m.GetTable (null, typeof (Foo)), "#A4");
			Assert.Throws<ArgumentNullException> (() => t = m.GetTable ("FooTable", null), "#A5");

			Assert.Throws<ArgumentException> (() => t = m.GetTable (String.Empty), "#B1");
			Assert.Throws<ArgumentException> (() => t = m.GetTable ("NoSuchName"), "#B2");
			Assert.Throws<ArgumentException> (() => t = m.GetTable (typeof (object)), "#B3");
			Assert.Throws<ArgumentException> (() => t = m.GetTable ("FooTable", typeof (object)), "#B4");
			Assert.Throws<ArgumentException> (() => t = m.GetTable ("NoSuchTable", typeof (object)), "#B5");

			Assert.IsNotNull (t = m.GetTable ("FooTable"), "#C1");
			Assert.AreEqual (typeof (Foo), t.EntityType, "#C2");
			Assert.IsNotNull (t = m.GetTable (typeof (Foo)), "#C3");
			Assert.AreEqual (typeof (Foo), t.EntityType, "#C4");
			Assert.IsNotNull (t = m.GetTable ("FooTable", typeof (MyDataContext2)), "#C5");
			Assert.AreEqual (typeof (Foo), t.EntityType, "#C6");
		}

		[Test]
		public void GetActionPath ()
		{
			var foo = new Foo (true);
			MetaModel m = Utils.GetModel<MyDataContext2> ();

			// None of those are thrown from GetTable - it seems this method performs NO checks at all, sigh...
			//
			//Assert.Throws<ArgumentNullException> (() => m.GetActionPath (null, PageAction.List, foo), "#A1");
			//Assert.Throws<ArgumentException> (() => m.GetActionPath (String.Empty, PageAction.List, foo), "#A2");
			//Assert.Throws<ArgumentNullException> (() => m.GetActionPath ("FooTable", null, foo), "#A3");
			//Assert.Throws<ArgumentNullException> (() => m.GetActionPath ("FooTable", PageAction.List, null), "#A4");
			//Assert.Throws<ArgumentException> (() => m.GetActionPath ("NoSuchTable", PageAction.List, foo), "#A5");
		}

		[Test]
		public void GetActionPath2 ()
		{
			var foo = new Foo (true);
			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;
			MetaModel m = Utils.GetModel<MyDataContext2> ();

			RouteTable.Routes.Add (
			    new DynamicDataRoute ("{table}/{action}.aspx") {
				    Constraints = new RouteValueDictionary (new { action = "List|Details|Edit|Insert" }),
				    Model = m,
				    RouteHandler = new MyDynamicDataRouteHandler ()
			    });

			// .NET stacktrace:
			//
			// at System.Web.DynamicData.MetaModel.TryGetTable(String uniqueTableName, MetaTable& table)
			// at System.Web.DynamicData.MetaModel.GetTable(String uniqueTableName)
			Assert.Throws<ArgumentNullException> (() => m.GetActionPath (null, PageAction.List, foo), "#A1");
			Assert.AreEqual (String.Empty, m.GetActionPath ("FooTable", null, foo), "#A2");
			Assert.AreEqual ("/FooTable/List.aspx", m.GetActionPath ("FooTable", PageAction.List, null), "#A3");
			Assert.Throws<ArgumentException> (() => m.GetActionPath ("NoSuchTable", PageAction.List, foo), "#A4");

			Assert.AreEqual ("/FooTable/List.aspx", m.GetActionPath ("FooTable", "List", foo), "#B1");
		}
	}
}
