//
// DynamicDataRouteHandlerTest.cs
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
using MonoTests.stand_alone.WebHarness;
using MonoTests.SystemWeb.Framework;
using MonoTests.Common;
using MonoTests.ModelProviders;

using MetaModel = System.Web.DynamicData.MetaModel;
using MetaTable = System.Web.DynamicData.MetaTable;

namespace MonoTests.System.Web.DynamicData
{
	[TestFixture]
	public class DynamicDataRouteHandlerTest
	{
		DynamicDataContainerModelProvider <TestDataContext> dynamicModelProvider;

		[TestFixtureSetUp]
		public void SetUp ()
		{
			dynamicModelProvider = new DynamicDataContainerModelProvider <TestDataContext> ();
			Utils.RegisterContext (dynamicModelProvider, new ContextConfiguration () { ScaffoldAllTables = true });
		}

		[Test]
		public void Constructor ()
		{
			var ddrh = new DynamicDataRouteHandler ();

			Assert.AreEqual (null, ddrh.Model, "#A1");
		}

		[Test]
		public void CreateHandlerParams ()
		{
			MetaModel m = Utils.CommonInitialize ();

			var route = RouteTable.Routes[0] as DynamicDataRoute;
			MetaTable t = m.Tables[TestDataContext.TableFooEmpty];
			var handler = route.RouteHandler = new DynamicDataRouteHandler ();

			// No null check is made, of course - throws from some internal method
			Assert.Throws<NullReferenceException> (() => {
				handler.CreateHandler (null, t, PageAction.Details);
			}, "#A1");

			// No null check again - this time throws from GetCustomPageVirtualPath
			Assert.Throws<NullReferenceException> (() => {
				handler.CreateHandler (route, null, PageAction.Details);
			}, "#A2");

			// And once again, no null check and thrown from GetCustomPageVirtualPath as well
			Assert.Throws<NullReferenceException> (() => {
				handler.CreateHandler (route, t, null);
			}, "#A3");
		}

		[Test]
		[Ignore ("Throws NREX on .NET - no idea why and how to make it work. Probably needs full request emulation (using Mainsoft test suite)")]
		// Probably need to simulate a full reqest using similar environment as System.Web tests
		public void CreateHandler ()
		{
			MetaModel m = Utils.CommonInitialize ();

			var route = RouteTable.Routes[0] as DynamicDataRoute;
			MetaTable t = m.Tables[TestDataContext.TableFooEmpty];
			Assert.IsNotNull (t, "#A1");

			var handler = route.RouteHandler = new DynamicDataRouteHandler ();
			var wrapper = new MyHttpContextWrapper ();
			var request = wrapper.Request as MyHttpRequestWrapper;
			request.SetProperty ("AppRelativeCurrentExecutionFilePath", "~/FooEmptyTable/List.aspx");
			request.SetProperty ("PathInfo", String.Empty);

			// This must be non-null because DynamicData doesn't care to check whether the returned
			// value is null or not...
			request.SetProperty ("QueryString", new NameValueCollection ());

			// This will assign the route handler's Model property
			RouteData rd = route.GetRouteData (wrapper);
			Assert.IsNotNull (handler.Model, "#A2");

			// Throws a NREX from some internal method - no slightest idea why, as none of the parameters
			// passed are null
			IHttpHandler h = handler.CreateHandler (route, t, PageAction.Details);
			Assert.IsNotNull (h, "#A3");
			Assert.AreEqual (typeof (Page), h.GetType (), "#A3-1");

			var page = h as Page;
			Assert.AreEqual (String.Empty, page.AppRelativeVirtualPath, "#A3-2");
		}

		[Test]
		public void GetCustomPageVirtualPath ()
		{
			MetaModel m = Utils.CommonInitialize ();
			var route = RouteTable.Routes[0] as DynamicDataRoute;

			MetaTable t = m.Tables[TestDataContext.TableFooEmpty];
			Assert.IsNotNull (t, "#A1");

			// We neeed the handler to have its Model property set
			route.RouteHandler = new MyDynamicDataRouteHandler ();
			var handler = route.RouteHandler as MyDynamicDataRouteHandler;
			Assert.IsNotNull (handler, "#A2");

			// Lack of null check (for table)
			Assert.Throws<NullReferenceException> (() => {
				handler.DoGetCustomPageVirtualPath (null, null);
			}, "#A2-1");

			// Another missing null check (this time for Model)... Are null checks
			// out of fashion?
			Assert.Throws<NullReferenceException> (() => {
				handler.DoGetCustomPageVirtualPath (t, String.Empty);
			}, "#A2-2");

			var wrapper = new MyHttpContextWrapper ();
			var request = wrapper.Request as MyHttpRequestWrapper;
			request.SetProperty ("AppRelativeCurrentExecutionFilePath", "~/FooEmptyTable/List.aspx");
			request.SetProperty ("PathInfo", String.Empty);

			// This must be non-null because DynamicData doesn't care to check whether the returned
			// value is null or not...
			request.SetProperty ("QueryString", new NameValueCollection ());

			// This will assign the route handler's Model property
			RouteData rd = route.GetRouteData (wrapper);
			Assert.IsNotNull (handler.Model, "#A3");

			Assert.AreEqual (handler.Model.DynamicDataFolderVirtualPath + "CustomPages/" + t.Name + "/MyCustomPage.aspx", 
				handler.DoGetCustomPageVirtualPath (t, "MyCustomPage"), "#A4");

			handler.Model.DynamicDataFolderVirtualPath = "~/MyFolder";
			Assert.AreEqual (handler.Model.DynamicDataFolderVirtualPath + "CustomPages/" + t.Name + "/MyCustomPage.aspx",
				handler.DoGetCustomPageVirtualPath (t, "MyCustomPage"), "#A5");

			// doh!
			Assert.AreEqual (handler.Model.DynamicDataFolderVirtualPath + "CustomPages/" + t.Name + "/.aspx",
				handler.DoGetCustomPageVirtualPath (t, null), "#A6");

			Assert.AreEqual (handler.Model.DynamicDataFolderVirtualPath + "CustomPages/" + t.Name + "/.aspx",
				handler.DoGetCustomPageVirtualPath (t, String.Empty), "#A7");
		}

		[Test]
		public void GetRequestContext ()
		{
			Assert.Throws<ArgumentNullException> (() => {
				DynamicDataRouteHandler.GetRequestContext (null);
			}, "#A1");

			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);

			RequestContext rc = DynamicDataRouteHandler.GetRequestContext (ctx);
			Assert.IsNotNull (rc, "#B1");
			Assert.IsNotNull (rc.HttpContext, "#B1-1");
			Assert.IsNotNull (rc.RouteData, "#B1-2");

			Assert.IsNull (rc.RouteData.Route, "#C1");
			Assert.IsNull (rc.RouteData.RouteHandler, "#C1-1");
			Assert.IsNotNull (rc.RouteData.Values, "#C1-2");
			Assert.AreEqual (0, rc.RouteData.Values.Count, "#C1-3");
		}

		[Test]
		public void GetRequestMetaTable ()
		{
			MetaModel m = Utils.CommonInitialize ();
			var route = RouteTable.Routes[0] as DynamicDataRoute;
			MetaTable t = m.Tables[TestDataContext.TableFooDisplayName];
			Assert.IsNotNull (t, "#A1");

			// Surprise! A null check is present!
			Assert.Throws<ArgumentNullException> (() => {
				DynamicDataRouteHandler.GetRequestMetaTable (null);
			}, "#A2");
			
			MetaTable t2 = DynamicDataRouteHandler.GetRequestMetaTable (HttpContext.Current);
			Assert.IsNull (t2, "#A3");

			DynamicDataRouteHandler.SetRequestMetaTable (HttpContext.Current, t);
			t2 = DynamicDataRouteHandler.GetRequestMetaTable (HttpContext.Current);
			Assert.IsNotNull (t2, "#A4");
			Assert.AreEqual (t, t2, "#A4-1");
		}

		[Test]
		public void GetScaffoldPageVirtualPath ()
		{
			MetaModel m = Utils.CommonInitialize ();
			var route = RouteTable.Routes[0] as DynamicDataRoute;

			MetaTable t = m.Tables[TestDataContext.TableFooDisplayName];
			Assert.IsNotNull (t, "#A1");

			// We neeed the handler to have its Model property set
			route.RouteHandler = new MyDynamicDataRouteHandler ();
			var handler = route.RouteHandler as MyDynamicDataRouteHandler;
			Assert.IsNotNull (handler, "#A2");

			// Lack of null check (for table)
			Assert.Throws<NullReferenceException> (() => {
				handler.DoGetScaffoldPageVirtualPath (null, null);
			}, "#A2-1");

			// Another missing null check (this time for Model)... Are null checks
			// out of fashion?
			Assert.Throws<NullReferenceException> (() => {
				handler.DoGetScaffoldPageVirtualPath (t, String.Empty);
			}, "#A2-2");

			var wrapper = new MyHttpContextWrapper ();
			var request = wrapper.Request as MyHttpRequestWrapper;
			request.SetProperty ("AppRelativeCurrentExecutionFilePath", "~/FooDisplayNameTable/List.aspx");
			request.SetProperty ("PathInfo", String.Empty);

			// This must be non-null because DynamicData doesn't care to check whether the returned
			// value is null or not...
			request.SetProperty ("QueryString", new NameValueCollection ());

			// This will assign the route handler's Model property
			RouteData rd = route.GetRouteData (wrapper);
			Assert.IsNotNull (handler.Model, "#A3");

			Assert.AreEqual (handler.Model.DynamicDataFolderVirtualPath + "PageTemplates/" + "MyCustomPage.aspx",
				handler.DoGetScaffoldPageVirtualPath (t, "MyCustomPage"), "#A4");

			handler.Model.DynamicDataFolderVirtualPath = "~/MyFolder";
			Assert.AreEqual (handler.Model.DynamicDataFolderVirtualPath + "PageTemplates/" + "MyCustomPage.aspx",
				handler.DoGetScaffoldPageVirtualPath (t, "MyCustomPage"), "#A5");

			// doh!
			Assert.AreEqual (handler.Model.DynamicDataFolderVirtualPath + "PageTemplates/" + ".aspx",
				handler.DoGetScaffoldPageVirtualPath (t, null), "#A6");

			Assert.AreEqual (handler.Model.DynamicDataFolderVirtualPath + "PageTemplates/" + ".aspx",
				handler.DoGetScaffoldPageVirtualPath (t, String.Empty), "#A7");
		}

		[Test]
		public void Model ()
		{
			MetaModel m = Utils.CommonInitialize ();
			var route = RouteTable.Routes[0] as DynamicDataRoute;

			Assert.IsNotNull (route, "#A1");
			Assert.IsNotNull (route.Model, "#A1-1");
			var handler = route.RouteHandler;

			Assert.IsNotNull (handler, "#A2");
			Assert.IsTrue (handler.GetType () == typeof (MyDynamicDataRouteHandler), "#A2-1");
			Assert.IsNull (handler.Model, "#A2-2");

			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);

			RequestContext rc = DynamicDataRouteHandler.GetRequestContext (ctx);
			Assert.IsNotNull (rc, "#B1");
			Assert.IsNull (handler.Model, "#B1-2");

			var wrapper = new MyHttpContextWrapper ();
			var request = wrapper.Request as MyHttpRequestWrapper;

			// It appears .NET checks whether the indicated table exists - if not, GetRouteData will return
			// null (even though the Route class will find a match)
			request.SetProperty ("AppRelativeCurrentExecutionFilePath", "~/NoSuchTable/List.aspx");
			request.SetProperty ("PathInfo", String.Empty);

			// This must be non-null because DynamicData doesn't care to check whether the returned
			// value is null or not...
			request.SetProperty ("QueryString", new NameValueCollection ());
			
			// No table FooTable in the context - returns null
			RouteData rd = route.GetRouteData (wrapper);
			Assert.IsNull (rd, "#C1");

			// Apparently Model is set in the above call even though it returns null
			Assert.IsNotNull (handler.Model, "#C1-1");
			Assert.AreEqual (route.Model, handler.Model, "#C1-2");

			request.SetProperty ("AppRelativeCurrentExecutionFilePath", "~/BarTable/List.aspx");
			rd = route.GetRouteData (wrapper);
			Assert.IsNotNull (rd, "#D1");
			Assert.IsNotNull (handler.Model, "#D1-1");
			Assert.AreEqual (route.Model, handler.Model, "#D1-2");
		}

		[Test]
		public void SetRequestMetaTable ()
		{
			MetaModel m = Utils.CommonInitialize ();
			var route = RouteTable.Routes[0] as DynamicDataRoute;
			MetaTable t = m.Tables[TestDataContext.TableFooDisplayName];
			Assert.IsNotNull (t, "#A1");

			// And following the tradition... [drum roll] - NO NULL CHECK!
			Assert.Throws<NullReferenceException> (() => {
				DynamicDataRouteHandler.SetRequestMetaTable (null, t);
			}, "#A2");

			DynamicDataRouteHandler.SetRequestMetaTable (HttpContext.Current, t);
			MetaTable t2 = DynamicDataRouteHandler.GetRequestMetaTable (HttpContext.Current);
			Assert.IsNotNull (t2, "#A3");
			Assert.AreEqual (t, t2, "#A3-1");

			DynamicDataRouteHandler.SetRequestMetaTable (HttpContext.Current, null);
			t2 = DynamicDataRouteHandler.GetRequestMetaTable (HttpContext.Current);
			Assert.IsNull (t2, "#A4");
		}
	}
}
