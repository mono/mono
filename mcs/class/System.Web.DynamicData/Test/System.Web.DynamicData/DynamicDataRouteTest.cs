//
// DynamicDataRouteTest.cs
//
// Authors:
//	Atsushi Enomoto <atsushi@ximian.com>
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
using MonoTests.stand_alone.WebHarness;
using MonoTests.SystemWeb.Framework;
using MonoTests.Common;
using MonoTests.ModelProviders;

using MetaModel = System.Web.DynamicData.MetaModel;
using MetaTable = System.Web.DynamicData.MetaTable;
namespace MonoTests.System.Web.DynamicData
{
	[TestFixture]
	public class DynamicDataRouteTest
	{
		[TestFixtureSetUp]
		public void SetUp ()
		{
			var dynamicModelProvider = new DynamicDataContainerModelProvider <TestDataContext> ();
			Utils.RegisterContext (dynamicModelProvider, new ContextConfiguration () { ScaffoldAllTables = true });
			Utils.RegisterContext (typeof (MyDataContext3));
		}

		[Test]
		public void ConstructorNull ()
		{
			new DynamicDataRoute (null);
		}

		[Test]
		public void Constructor ()
		{
			// other tests create MetaModel and set Default and this test does not always run first, so it does not make sense anymore.
			//Assert.IsNull (MetaModel.Default, "#1");
			bool isFirst = (MetaModel.Default == null);
			var m = new MetaModel (); // it automatically fills Default
			if (isFirst)
				Assert.AreEqual (m, MetaModel.Default, "#2");

			var r = new DynamicDataRoute ("Dynamic1");
			Assert.AreEqual (MetaModel.Default, r.Model, "#1");
			Assert.IsNull (r.Action, "#2");
			Assert.IsNull (r.Table, "#3");
			Assert.IsNull (r.ViewName, "#4");
			Assert.IsNotNull (r.RouteHandler, "#5");
			Assert.IsNotNull (r.Model, "#6");
			Assert.IsNull (r.RouteHandler.Model, "#7"); // irrelevant to route's MetaModel
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		[Category ("NotDotNet")] // .NET throws NRE. yuck.
#if TARGET_DOTNET
		[Ignore ("Throws a NREX on .NET...")]
#endif
		public void GetActionFromRouteDataNullArg ()
		{
			new DynamicDataRoute ("x").GetActionFromRouteData (null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void GetActionFromRouteData ()
		{
			var r = new DynamicDataRoute ("x2");
			var rd = new RouteData ();
			// rd must have "Action" value
			r.GetActionFromRouteData (rd);
		}

		[Test]
		public void GetActionFromRouteData2 ()
		{
			var r = new DynamicDataRoute ("x");
			var rd = new RouteData ();
			rd.Values["Action"] = "y";
			var a = r.GetActionFromRouteData (rd);
			Assert.AreEqual ("y", a);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		[Category ("NotDotNet")] // .NET throws NRE. yuck.
#if TARGET_DOTNET
		[Ignore ("Throws a NREX on .NET...")]
#endif
		public void GetTableFromRouteDataNullArg ()
		{
			new DynamicDataRoute ("x").GetTableFromRouteData (null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void GetTableFromRouteData ()
		{
			var r = new DynamicDataRoute ("x");
			var rd = new RouteData ();
			// rd must have "Table" value
			r.GetTableFromRouteData (rd);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetTableFromRouteData2 ()
		{
			var r = new DynamicDataRoute ("x");
			r.Model = new MetaModel ();
			var rd = new RouteData ();
			rd.Values["Table"] = "y";
			r.GetTableFromRouteData (rd); // no such table
		}

		[Test]
		public void GetTableFromRouteData3 ()
		{
			var r = new DynamicDataRoute ("x");
			var rd = new RouteData ();
			rd.Values["Table"] = "FooTable";
			var t = r.GetTableFromRouteData (rd);
		}

		[Test]
		public void GetRouteData ()
		{
			var r = new DynamicDataRoute ("{table}/{action}.aspx");

			// We need one which overloads CreateHandler
			r.RouteHandler = new MyDynamicDataRouteHandler ();

			var wrapper = new MyHttpContextWrapper ();
			var request = wrapper.Request as MyHttpRequestWrapper;
			request.SetProperty ("AppRelativeCurrentExecutionFilePath", "~/NoSuchTable/List.aspx");
			request.SetProperty ("PathInfo", String.Empty);

			// This must be non-null because DynamicData doesn't care to check whether the returned
			// value is null or not...
			request.SetProperty ("QueryString", new NameValueCollection ());

			// It appears .NET checks whether the indicated table exists - if not, GetRouteData will return
			// null (even though the Route class will find a match)
			RouteData rd = r.GetRouteData (wrapper);
			Assert.IsNull (rd, "#A1");

			request.SetProperty ("AppRelativeCurrentExecutionFilePath", "~/BazTable/List.aspx");
			rd = r.GetRouteData (wrapper);
			Assert.IsNotNull (rd, "#B1");
		}

		[Test]
		public void RouteHandler ()
		{
			var r = new DynamicDataRoute ("{table}/{action}.aspx");

			Assert.IsNotNull (r.RouteHandler, "#A1");
			Assert.AreEqual (typeof (DynamicDataRouteHandler), r.RouteHandler.GetType (), "#A1-1");

			r.RouteHandler = null;
			Assert.IsNull (r.RouteHandler, "#A2");
		}

		[Test]
		public void BaseDefaultsModification_1 ()
		{
			MetaModel m = MetaModel.Default;
			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();

			var ddr = new DynamicDataRoute ("{table}/{action}.aspx") {
				Action = PageAction.Details,
				Model = m,
				RouteHandler = new MyDynamicDataRouteHandler ()
			};

			routes.Add (ddr);

			Assert.IsNotNull (ddr, "#A1");
			Assert.IsNull (ddr.Defaults, "#A1-1");
			var rd = new RouteData ();
			var hc = new HttpContextWrapper (HttpContext.Current);

			ddr.GetVirtualPath (new RequestContext (hc, rd), null);
			Assert.IsNotNull (ddr.Defaults, "#B1");
			Assert.AreEqual (1, ddr.Defaults.Count, "#B1-1");
			Assert.AreEqual (PageAction.Details, ddr.Defaults["Action"], "#B1-2");

			ddr.Action = "MyAction";
			ddr.GetVirtualPath (new RequestContext (hc, rd), null);
			Assert.IsNotNull (ddr.Defaults, "#C1");
			Assert.AreEqual (1, ddr.Defaults.Count, "#C1-1");
			Assert.AreEqual (PageAction.Details, ddr.Defaults["Action"], "#B1-2");
		}

		[Test]
		public void BaseDefaultsModification_2 ()
		{
			MetaModel m = MetaModel.Default;
			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();

			var ddr = new DynamicDataRoute ("{table}/{action}.aspx") {
				Table = "BazTable",
				Model = m,
				RouteHandler = new MyDynamicDataRouteHandler ()
			};

			routes.Add (ddr);

			Assert.IsNotNull (ddr, "#A1");
			Assert.IsNull (ddr.Defaults, "#A1-1");
			var rd = new RouteData ();
			var hc = new HttpContextWrapper (HttpContext.Current);

			ddr.GetVirtualPath (new RequestContext (hc, rd), null);
			Assert.IsNotNull (ddr.Defaults, "#B1");
			Assert.AreEqual (1, ddr.Defaults.Count, "#B1-1");
			Assert.AreEqual ("BazTable", ddr.Defaults["Table"], "#B1-2");

			ddr.Table = "AnotherTable";
			ddr.GetVirtualPath (new RequestContext (hc, rd), null);
			Assert.IsNotNull (ddr.Defaults, "#C1");
			Assert.AreEqual (1, ddr.Defaults.Count, "#C1-1");
			Assert.AreEqual ("BazTable", ddr.Defaults["Table"], "#C1-2");
		}

		[Test]
		public void BaseDefaultsModification_3 ()
		{
			MetaModel m = MetaModel.Default;
			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();

			var ddr = new DynamicDataRoute ("{table}/{action}.aspx") {
				Table = "MyTable",
				Model = m,
				RouteHandler = new MyDynamicDataRouteHandler ()
			};
			routes.Add (ddr);
			var rd = new RouteData ();
			var hc = new HttpContextWrapper (HttpContext.Current);
			
			Assert.Throws <ArgumentException> (() => {
				ddr.GetVirtualPath (new RequestContext (hc, rd), null);
			}, "#A1");
		}

		[Test]
		public void BaseDefaultsModification_4 ()
		{
			MetaModel m = MetaModel.Default;
			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();

			var ddr = new DynamicDataRoute ("{table}/{action}.aspx") {
				Model = m,
				RouteHandler = new MyDynamicDataRouteHandler ()
			};
			routes.Add (ddr);
			var rd = new RouteData ();
			var hc = new HttpContextWrapper (HttpContext.Current);

			Assert.IsNull (ddr.Defaults, "#A1");
			ddr.GetVirtualPath (new RequestContext (hc, rd), null);
			Assert.IsNull (ddr.Defaults, "#A2");
		}

		[Test]
		public void BaseDefaultsModification_5 ()
		{
			MetaModel m = MetaModel.Default;
			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();

			var ddr = new DynamicDataRoute ("{table}/{action}.aspx") {
				Action = null,
				Model = m,
				RouteHandler = new MyDynamicDataRouteHandler ()
			};

			routes.Add (ddr);

			Assert.IsNotNull (ddr, "#A1");
			Assert.IsNull (ddr.Defaults, "#A1-1");
			var rd = new RouteData ();
			var hc = new HttpContextWrapper (HttpContext.Current);

			ddr.GetVirtualPath (new RequestContext (hc, rd), null);
			Assert.IsNull (ddr.Defaults, "#B1");
			
			ddr.Action = "MyAction";
			ddr.GetVirtualPath (new RequestContext (hc, rd), null);
			Assert.IsNull (ddr.Defaults, "#C1");
		}

		[Test]
		public void BaseDefaultsModification_6 ()
		{
			MetaModel m = MetaModel.Default;
			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();

			var ddr = new DynamicDataRoute ("{table}/{action}.aspx") {
				Action = String.Empty,
				Model = m,
				RouteHandler = new MyDynamicDataRouteHandler ()
			};

			routes.Add (ddr);

			Assert.IsNotNull (ddr, "#A1");
			Assert.IsNull (ddr.Defaults, "#A1-1");
			var rd = new RouteData ();
			var hc = new HttpContextWrapper (HttpContext.Current);

			ddr.GetVirtualPath (new RequestContext (hc, rd), null);
			Assert.IsNotNull (ddr.Defaults, "#B1");
			Assert.AreEqual (1, ddr.Defaults.Count, "#B1-1");
			Assert.AreEqual (String.Empty, ddr.Defaults["Action"], "#B1-2");

			ddr.Action = "MyAction";
			ddr.GetVirtualPath (new RequestContext (hc, rd), null);
			Assert.IsNotNull (ddr.Defaults, "#C1");
			Assert.AreEqual (1, ddr.Defaults.Count, "#C1-1");
			Assert.AreEqual (String.Empty, ddr.Defaults["Action"], "#B1-2");
		}

		[Test]
		public void BaseDefaultsModification_7 ()
		{
			MetaModel m = MetaModel.Default;
			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();

			var ddr = new DynamicDataRoute ("{table}/{action}.aspx") {
				Table = null,
				Model = m,
				RouteHandler = new MyDynamicDataRouteHandler ()
			};

			routes.Add (ddr);

			Assert.IsNotNull (ddr, "#A1");
			Assert.IsNull (ddr.Defaults, "#A1-1");
			var rd = new RouteData ();
			var hc = new HttpContextWrapper (HttpContext.Current);

			ddr.GetVirtualPath (new RequestContext (hc, rd), null);
			Assert.IsNull (ddr.Defaults, "#B1");
		}

		[Test]
		public void BaseDefaultsModification_8 ()
		{
			MetaModel m = MetaModel.Default;
			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();

			var ddr = new DynamicDataRoute ("{table}/{action}.aspx") {
				Table = String.Empty,
				Model = m,
				RouteHandler = new MyDynamicDataRouteHandler ()
			};

			routes.Add (ddr);
			var rd = new RouteData ();
			var hc = new HttpContextWrapper (HttpContext.Current);

			Assert.Throws<ArgumentException> (() => {
				ddr.GetVirtualPath (new RequestContext (hc, rd), null);
			}, "#A1");
		}

		[Test]
		public void BaseDefaultsModification_9 ()
		{
			MetaModel m = MetaModel.Default;
			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();

			var ddr = new DynamicDataRoute ("{table}/{action}.aspx") {
				Defaults = new RouteValueDictionary () {
					{"Action", "InitialAction"}
				},
				Action = PageAction.Details,
				Model = m,
				RouteHandler = new MyDynamicDataRouteHandler ()
			};

			routes.Add (ddr);

			Assert.IsNotNull (ddr, "#A1");
			Assert.IsNotNull (ddr.Defaults, "#A1-1");
			var rd = new RouteData ();
			var hc = new HttpContextWrapper (HttpContext.Current);

			ddr.GetVirtualPath (new RequestContext (hc, rd), null);
			Assert.IsNotNull (ddr.Defaults, "#B1");
			Assert.AreEqual (1, ddr.Defaults.Count, "#B1-1");
			Assert.AreEqual (PageAction.Details, ddr.Defaults["Action"], "#B1-2");

			ddr.Action = "MyAction";
			ddr.GetVirtualPath (new RequestContext (hc, rd), null);
			Assert.IsNotNull (ddr.Defaults, "#C1");
			Assert.AreEqual (1, ddr.Defaults.Count, "#C1-1");
			Assert.AreEqual (PageAction.Details, ddr.Defaults["Action"], "#B1-2");
		}

		[Test]
		public void BaseDefaultsModification_10 ()
		{
			MetaModel m = MetaModel.Default;
			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();

			var ddr = new DynamicDataRoute ("{table}/{action}.aspx") {
				Defaults = new RouteValueDictionary () {
					{"Table", "FooWithDefaultsTable"}
				},
				Table = "BazTable",
				Model = m,
				RouteHandler = new MyDynamicDataRouteHandler ()
			};

			routes.Add (ddr);

			Assert.IsNotNull (ddr, "#A1");
			Assert.IsNotNull (ddr.Defaults, "#A1-1");
			var rd = new RouteData ();
			var hc = new HttpContextWrapper (HttpContext.Current);

			ddr.GetVirtualPath (new RequestContext (hc, rd), null);
			Assert.IsNotNull (ddr.Defaults, "#B1");
			Assert.AreEqual (1, ddr.Defaults.Count, "#B1-1");
			Assert.AreEqual ("BazTable", ddr.Defaults["Table"], "#B1-2");

			ddr.Table = "AnotherTable";
			ddr.GetVirtualPath (new RequestContext (hc, rd), null);
			Assert.IsNotNull (ddr.Defaults, "#C1");
			Assert.AreEqual (1, ddr.Defaults.Count, "#C1-1");
			Assert.AreEqual ("BazTable", ddr.Defaults["Table"], "#C1-2");
		}
	}
}
