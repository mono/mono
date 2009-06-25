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
using NUnit.Mocks;
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
		[Test]
		public void GetRequestContext ()
		{
			AssertExtensions.Throws<ArgumentNullException> (() => {
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
	}
}
