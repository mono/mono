//
// RouteUrlExpressionBuilder.cs
//
// Authors:
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2010 Novell, Inc. (http://novell.com/)
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
using System.CodeDom;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Routing;

using NUnit.Framework;

using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;


namespace MonoTests.System.Web.Compilation
{
	[TestFixture]
	public class RouteUrlExpressionBuilderTest
	{
		[Test]
		public void EvaluateExpression ()
		{
			RouteTable.Routes.Clear ();
			RouteTable.Routes.Add (new Route ("{foo}-foo", new PageRouteHandler ("~/default.aspx")));
			RouteTable.Routes.Add ("bar1", new Route ("{bar}-foo", new PageRouteHandler ("~/bar.aspx")));
			RouteTable.Routes.Add ("bar2", new Route ("some-{bar}", new PageRouteHandler ("~/some-bar.aspx")));

			var bldr = new RouteUrlExpressionBuilder ();
			var entry = CreatePropertyEntry ("foo=test", "RouteUrl");
			var context = new ExpressionBuilderContext (new FakePage ());
			object obj = bldr.EvaluateExpression (null, entry, null, context);
			Assert.AreEqual ("/test-foo", obj, "#A1");

			entry = CreatePropertyEntry ("bar=test", "RouteUrl");
			obj = bldr.EvaluateExpression (null, entry, null, context);
			Assert.AreEqual ("/test-foo", obj, "#A2-1");

			entry = CreatePropertyEntry ("bar=test,routename=bar2", "RouteUrl");
			obj = bldr.EvaluateExpression (null, entry, null, context);
			Assert.AreEqual ("/some-test", obj, "#A2-2");

			entry = CreatePropertyEntry ("bar=test,routename=bar1", "RouteUrl");
			obj = bldr.EvaluateExpression (null, entry, null, context);
			Assert.AreEqual ("/test-foo", obj, "#A2-3");

			entry = CreatePropertyEntry ("bar=test,routename=noroute", "RouteUrl");
			try {
				obj = bldr.EvaluateExpression (null, entry, null, context);
				Assert.Fail ("#A3");
			} catch (ArgumentException) {
				// success
			}

			entry = CreatePropertyEntry ("nosuchparam=test", "RouteUrl");
			obj = bldr.EvaluateExpression (null, entry, null, context);
			Assert.IsNull (obj, "#A4");

			Assert.Throws<NullReferenceException> (() => {
				bldr.EvaluateExpression (null, null, null, context);
			}, "#A5-1");

			Assert.Throws<NullReferenceException> (() => {
				bldr.EvaluateExpression (null, entry, null, null);
			}, "#A5-2");
		}

		BoundPropertyEntry CreatePropertyEntry (string expression, string expressionPrefix)
		{
			var entry = Activator.CreateInstance (typeof (BoundPropertyEntry), true) as BoundPropertyEntry;
			entry.Expression = expression;
			entry.ExpressionPrefix = expressionPrefix;

			return entry;
		}

		[Test]
		public void GetRouteUrl ()
		{
			Assert.Throws<ArgumentNullException> (() => {
				RouteUrlExpressionBuilder.GetRouteUrl (null, "bar=test");
			}, "#A1-1");

			var t = new WebTest (PageInvoker.CreateOnLoad (GetRouteUrl_Load));
			t.Run ();
		}

		public static void GetRouteUrl_Load (Page p)
		{
			RouteTable.Routes.Clear ();
			RouteTable.Routes.Add (new Route ("{foo}-foo", new PageRouteHandler ("~/default.aspx")));
			RouteTable.Routes.Add ("bar1", new Route ("{bar}-foo", new PageRouteHandler ("~/bar.aspx")));

			var ctl = new Control ();
			string url = RouteUrlExpressionBuilder.GetRouteUrl (new Control (), "foobar=test");
			Assert.IsNull (url, "#A2");

			url = RouteUrlExpressionBuilder.GetRouteUrl (new Control (), "bar=test");
			Assert.IsNotNull (url, "#A3-1");
			Assert.AreEqual ("/NunitWeb/test-foo", url, "#A3-2");

			url = RouteUrlExpressionBuilder.GetRouteUrl (new Control (), "routename=bar1,bar=test");
			Assert.IsNotNull (url, "#A4-1");
			Assert.AreEqual ("/NunitWeb/test-foo", url, "#A4-2");

			url = RouteUrlExpressionBuilder.GetRouteUrl (new Control (), "ROUTEname=bar1,bar=test");
			Assert.IsNotNull (url, "#A5-1");
			Assert.AreEqual ("/NunitWeb/test-foo", url, "#A5-2");

			url = RouteUrlExpressionBuilder.GetRouteUrl (new Control (), "   routename  =   bar1,    bar  =    test     ");
			Assert.IsNotNull (url, "#A6-1");
			Assert.AreEqual ("/NunitWeb/test-foo", url, "#A6-2");

			Assert.Throws<InvalidOperationException> (() => {
				url = RouteUrlExpressionBuilder.GetRouteUrl (new Control (), "routename");
			}, "#A7-1");

			Assert.Throws<InvalidOperationException> (() => {
				url = RouteUrlExpressionBuilder.GetRouteUrl (new Control (), String.Empty);
			}, "#A7-2");

			Assert.Throws<InvalidOperationException> (() => {
				url = RouteUrlExpressionBuilder.GetRouteUrl (new Control (), null);
			}, "#A7-2");
		}

		[Test]
		public void TryParseRouteExpression ()
		{
			string routeName;
			var rvd = new RouteValueDictionary ();
			
			Assert.IsFalse (RouteUrlExpressionBuilder.TryParseRouteExpression (null, rvd, out routeName), "#A1-1");
			Assert.IsFalse (RouteUrlExpressionBuilder.TryParseRouteExpression (String.Empty, rvd, out routeName), "#A1-2");
			Assert.IsFalse (RouteUrlExpressionBuilder.TryParseRouteExpression ("route", rvd, out routeName), "#A1-3");
			Assert.IsTrue (RouteUrlExpressionBuilder.TryParseRouteExpression ("route=", rvd, out routeName), "#A1-4");
			Assert.AreEqual (1, rvd.Count, "#A1-4-1");
			Assert.AreEqual (String.Empty, rvd ["route"], "#A1-4-2");

			Assert.Throws<NullReferenceException> (() => {
				RouteUrlExpressionBuilder.TryParseRouteExpression ("foo=bar", null, out routeName);
			}, "#A1-5");

			rvd.Clear ();
			Assert.IsTrue (RouteUrlExpressionBuilder.TryParseRouteExpression ("foo=bar", rvd, out routeName), "#A2-1");
			Assert.AreEqual (1, rvd.Count, "#A2-2");
			Assert.AreEqual ("bar", rvd ["foo"], "#A2-3");
			Assert.IsNull (routeName, "#A2-4");

			rvd.Clear ();
			Assert.IsTrue (RouteUrlExpressionBuilder.TryParseRouteExpression ("routeName=route,foo=bar,baz=zonk", rvd, out routeName), "#A3-1");
			Assert.AreEqual (2, rvd.Count, "#A3-2");
			Assert.AreEqual ("bar", rvd ["foo"], "#A3-3");
			Assert.AreEqual ("zonk", rvd ["baz"], "#A3-3");
			Assert.IsNotNull (routeName, "#A3-5");
			Assert.AreEqual ("route", routeName, "#A3-6");

			rvd.Clear ();
			Assert.IsTrue (RouteUrlExpressionBuilder.TryParseRouteExpression ("  rOUteName=route      ,  foo=bar\t,   baz   =\t  zonk   \t ", rvd, out routeName), "#A4-1");
			Assert.AreEqual (2, rvd.Count, "#A4-2");
			Assert.AreEqual ("bar", rvd ["foo"], "#A4-3");
			Assert.AreEqual ("zonk", rvd ["baz"], "#A4-3");
			Assert.IsNotNull (routeName, "#A4-5");
			Assert.AreEqual ("route", routeName, "#A4-6");

			rvd.Clear ();
			Assert.IsFalse (RouteUrlExpressionBuilder.TryParseRouteExpression ("foo=bar,route,baz=zonk", rvd, out routeName), "#A5-1");
			Assert.AreEqual (1, rvd.Count, "#A5-2");
			Assert.AreEqual ("bar", rvd ["foo"], "#A5-3");

			rvd.Clear ();
			Assert.IsFalse (RouteUrlExpressionBuilder.TryParseRouteExpression ("foo=bar,route==,baz=zonk", rvd, out routeName), "#A6-1");
			Assert.AreEqual (1, rvd.Count, "#A6-2");
			Assert.AreEqual ("bar", rvd ["foo"], "#A6-3");

			rvd.Clear ();
			Assert.IsFalse (RouteUrlExpressionBuilder.TryParseRouteExpression ("route==", rvd, out routeName), "#A7-1");
			Assert.AreEqual (0, rvd.Count, "#A7-2");

			rvd.Clear ();
			Assert.IsFalse (RouteUrlExpressionBuilder.TryParseRouteExpression ("route==stuff", rvd, out routeName), "#A8-1");
			Assert.AreEqual (0, rvd.Count, "#A8-2");

			rvd.Clear ();
			Assert.IsTrue (RouteUrlExpressionBuilder.TryParseRouteExpression ("route=stuff1,route=stuff2", rvd, out routeName), "#A9-1");
			Assert.AreEqual (1, rvd.Count, "#A9-2");
			Assert.AreEqual ("stuff2", rvd ["route"], "#A9-3");

			rvd.Clear ();
			Assert.IsFalse (RouteUrlExpressionBuilder.TryParseRouteExpression ("=stuff", rvd, out routeName), "#A10-1");
			Assert.AreEqual (0, rvd.Count, "#A10-2");

			rvd.Clear ();
			Assert.IsTrue (RouteUrlExpressionBuilder.TryParseRouteExpression ("routeName=route,routename=route2,foo=bar,baz=zonk", rvd, out routeName), "#A11-1");
			Assert.AreEqual (2, rvd.Count, "#A11-2");
			Assert.AreEqual ("bar", rvd ["foo"], "#A11-3");
			Assert.AreEqual ("zonk", rvd ["baz"], "#A11-3");
			Assert.IsNotNull (routeName, "#A11-5");
			Assert.AreEqual ("route2", routeName, "#A11-6");
		}

		[Test]
		public void GetCodeExpression ()
		{
			var bldr = new RouteUrlExpressionBuilder ();
			var entry = CreatePropertyEntry ("foo=test", "RouteUrl");
			var context = new ExpressionBuilderContext (new FakePage ());
			CodeExpression expr;

			Assert.Throws<NullReferenceException> (() => {
				expr = bldr.GetCodeExpression (null, "data", context);
			}, "#A1-1");

			expr = bldr.GetCodeExpression (entry, null, context);
			Assert.IsNotNull (expr, "#A2");

			expr = bldr.GetCodeExpression (entry, "data", null);
			Assert.IsNotNull (expr, "#A3");

			expr = bldr.GetCodeExpression (entry, null, null);
			Assert.IsNotNull (expr, "#A4-1");
			Assert.AreEqual (typeof (CodeMethodInvokeExpression), expr.GetType (), "#A4-2");

			var invoke = expr as CodeMethodInvokeExpression;
			Assert.AreEqual (typeof (CodeTypeReferenceExpression), invoke.Method.TargetObject.GetType (), "#A4-3");

			var tref = invoke.Method.TargetObject as CodeTypeReferenceExpression;
			Assert.AreEqual ("System.Web.Compilation.RouteUrlExpressionBuilder", tref.Type.BaseType, "#A4-4");
			Assert.AreEqual ("GetRouteUrl", invoke.Method.MethodName, "#A4-5");
			
			Assert.AreEqual (2, invoke.Parameters.Count, "#A5-1");
			Assert.AreEqual (typeof (CodeThisReferenceExpression), invoke.Parameters [0].GetType (), "#A5-2");
			Assert.AreEqual (typeof (CodePrimitiveExpression), invoke.Parameters [1].GetType (), "#A5-3");

			var pex = invoke.Parameters [1] as CodePrimitiveExpression;
			Assert.AreEqual ("foo=test", pex.Value, "#A5-4");
		}
	}

	class FakePage : Page
	{
		private HttpContext ctx;

		// don't call base class (so _context is never set to a non-null value)
		protected internal override HttpContext Context
		{
			get
			{
				if (ctx == null) {
					ctx = new HttpContext (
						new HttpRequest ("default.aspx", "http://mono-project.com/", "q=1&q2=2"),
						new HttpResponse (new StringWriter ())
						);
				}
				return ctx;
			}
		}
	}
}
