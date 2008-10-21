//
// DynamicDataRouteTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell Inc. http://novell.com
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
using System.Web;
using System.Web.DynamicData;
using System.Web.Routing;
using NUnit.Framework;

namespace MonoTests.System.Web.DynamicData
{
	[TestFixture]
	public class DynamicDataRouteTest
	{
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
			Assert.IsNull (r.RouteHandler.Model, "#6");
			Assert.IsNull (r.RouteHandler.Model, "#7"); // irrelevant to route's MetaModel
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		[Category ("NotDotNet")] // .NET throws NRE. yuck.
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
			rd.Values ["Action"] = "y";
			var a = r.GetActionFromRouteData (rd);
			Assert.AreEqual ("y", a);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		[Category ("NotDotNet")] // .NET throws NRE. yuck.
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
			rd.Values ["Table"] = "y";
			r.GetTableFromRouteData (rd); // no such table
		}

		[Test]
		[Category ("NotWorking")]
		public void GetTableFromRouteData3 ()
		{
			var r = new DynamicDataRoute ("x");
			r.Model.RegisterContext (typeof (MyDataContext3));
			var rd = new RouteData ();
			rd.Values ["Table"] = "FooTable";
			var t = r.GetTableFromRouteData (rd);
		}

		[Test]
		[Ignore ("it requires working HttpContext, and it's somehow not working on both mono and .net")]
		public void GetRouteData ()
		{
			var r = new DynamicDataRoute ("{table}/{action}.aspx");
			HttpContext.Current = new HttpContext (new MyHttpWorkerRequest ());
			RouteData rd = r.GetRouteData (new HttpContextStub ("~/FooTable/List.aspx", String.Empty));
			Assert.IsNotNull (rd.RouteHandler, "#1");
		}
	}
}
