//
// RouteTest.cs
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
using System.Web.Routing;
using NUnit.Framework;

namespace MonoTests.System.Web.Routing
{
	[TestFixture]
	public class RouteTest
	{
		[Test]
		public void ConstructorNullArgs ()
		{
			var r = new Route (null, null);
			Assert.AreEqual (String.Empty, r.Url);
			Assert.IsNull (r.RouteHandler);
		}

		[Test]
		public void SetNullUrl ()
		{
			var r = new Route (null, null);
			r.Url = "urn:foo";
			r.Url = null;
			Assert.AreEqual (String.Empty, r.Url);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void InvalidUrl ()
		{
			new Route ("~", null); // cannot start with '~'
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void InvalidUrl2 ()
		{
			new Route ("/", null); // cannot start with '/'
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void InvalidUrl3 ()
		{
			new Route ("foo?bar", null); // cannot contain '?'
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void InvalidUrl4 ()
		{
			new Route ("foo/{bar", null); // unmatched '{'
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void InvalidUrl5 ()
		{
			new Route ("foo/bar}", null); // unmatched '}'
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void InvalidUrl6 ()
		{
			new Route ("foo/{}", null); // "" is an invalid parameter name.
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void InvalidUrl7 ()
		{
			new Route ("foo/{x/y/z}", null); // incomplete parameter in path segment.
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void InvalidUrl8 ()
		{
			new Route ("foo/{a{{b}}c}", null); // regarded as an incomplete parameter
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void InvalidUrl9 ()
		{
			new Route ("foo/{a}{b}", null); // consecutive parameters are not allowed
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void InvalidUrl10 ()
		{
			new Route ("foo//bar", null); // consecutive segment separators '/' are not allowed
		}

		[Test]
		public void ValidUrl ()
		{
			var r = new Route ("{foo}/{bar}", null);
			Assert.AreEqual ("{foo}/{bar}", r.Url, "#1");
			Assert.IsNull (r.DataTokens, "#2");
			Assert.IsNull (r.Defaults, "#3");
			Assert.IsNull (r.Constraints, "#4");
		}

		[Test]
		public void ValidUrl2 ()
		{
			new Route ("a~c", null);
		}

		[Test]
		public void ValidUrl3 ()
		{
			new Route ("foo/", null);
		}

		[Test]
		public void RoutingHandler ()
		{
			var r = new Route (null, new StopRoutingHandler ());
			Assert.AreEqual (typeof (StopRoutingHandler), r.RouteHandler.GetType (), "#1");
		}

		[Test]
		public void GetRouteDataNoTemplate ()
		{
			var r = new Route ("foo/bar", null);
			var hc = new HttpContextStub ("~/foo/bar", String.Empty);
			var rd = r.GetRouteData (hc);
			Assert.IsNotNull (rd, "#1");
			Assert.AreEqual (r, rd.Route, "#2");
			Assert.AreEqual (0, rd.DataTokens.Count, "#3");
			Assert.AreEqual (0, rd.Values.Count, "#4");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void InvalidConstraint ()
		{
			var r = new Route ("{foo}/{bar}", new StopRoutingHandler ());
			var c = new RouteValueDictionary ();
			c ["foo"] = Guid.NewGuid ();
			r.Constraints = c;
			var hc = new HttpContextStub ("~/x/y", String.Empty);
			var rd = r.GetRouteData (hc);
		}

		[Test]
		public void GetRouteData ()
		{
			var r = new Route ("{foo}/{bar}", null);
			var hc = new HttpContextStub ("~/x/y", String.Empty);
			var rd = r.GetRouteData (hc);
			Assert.IsNotNull (rd, "#1");
			Assert.AreEqual (r, rd.Route, "#2");
			Assert.AreEqual (0, rd.DataTokens.Count, "#3");
			Assert.AreEqual (2, rd.Values.Count, "#4");
			Assert.AreEqual ("x", rd.Values ["foo"], "#4-1");
			Assert.AreEqual ("y", rd.Values ["bar"], "#4-2");
		}

		[Test]
		public void GetRouteData2 ()
		{
			// {} matches and substitutes even at partial state ...
			var r = new Route ("{foo}/bar{baz}", null);
			var hc = new HttpContextStub ("~/x/bart", String.Empty);
			var rd = r.GetRouteData (hc);
			Assert.IsNotNull (rd, "#1");
			Assert.AreEqual (r, rd.Route, "#2");
			Assert.AreEqual (0, rd.DataTokens.Count, "#3");
			Assert.AreEqual (2, rd.Values.Count, "#4");
			Assert.AreEqual ("x", rd.Values ["foo"], "#4-1");
			Assert.AreEqual ("t", rd.Values ["baz"], "#4-2");
		}

		[Test]
		public void GetRouteData3 ()
		{
			var r = new Route ("{foo}/{bar}", null);
			var hc = new HttpContextStub ("~/x/y/z", String.Empty);
			var rd = r.GetRouteData (hc);
			Assert.IsNull (rd); // mismatch
		}

		[Test]
		public void GetRouteData4 ()
		{
			var r = new Route ("{foo}/{bar}", null);
			var hc = new HttpContextStub ("~/x", String.Empty);
			var rd = r.GetRouteData (hc);
			Assert.IsNull (rd); // mismatch
		}

		[Test]
		public void GetRouteData5 ()
		{
			var r = new Route ("{foo}/{bar}", new StopRoutingHandler ());
			var rd = r.GetRouteData (new HttpContextStub ("x/y", String.Empty));
			Assert.IsNull (rd, "#1");
			rd = r.GetRouteData (new HttpContextStub ("~/x/y", String.Empty));
			Assert.IsNotNull (rd, "#2");
			rd = r.GetRouteData (new HttpContextStub ("~/x/y/z", String.Empty));
			Assert.IsNull (rd, "#3");
			rd = r.GetRouteData (new HttpContextStub ("~x/y", String.Empty));
			Assert.IsNull (rd, "#4");
			rd = r.GetRouteData (new HttpContextStub ("/x/y", String.Empty));
			Assert.IsNull (rd, "#5");

			rd = r.GetRouteData (new HttpContextStub ("{foo}/{bar}/baz", String.Empty));
			Assert.IsNull (rd, "#6");
			rd = r.GetRouteData (new HttpContextStub ("{foo}/{bar}", String.Empty));
			Assert.IsNotNull (rd, "#7");
			Assert.AreEqual (0, rd.DataTokens.Count, "#7-2");
			Assert.AreEqual (2, rd.Values.Count, "#7-3");
		}

		[Test]
		public void GetRouteData6 ()
		{
			var r = new Route ("{table}/{action}.aspx", null);
			var rd = r.GetRouteData (new HttpContextStub ("~/FooTable/List.aspx", String.Empty));
			Assert.IsNotNull (rd, "#1");
			Assert.AreEqual ("FooTable", rd.Values ["table"], "#2");
			Assert.AreEqual ("List", rd.Values ["action"], "#3");
		}

		[Test]
		[Ignore ("Pending testing - might be invalid test")]
		public void GetRouteData7 ()
		{
			var r = new Route ("{table}/{action}.aspx", null);
			var rd = r.GetRouteData (new HttpContextStub ("~/FooTable/", String.Empty));
			Assert.IsNull (rd, "#1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetVirtualPathNullContext ()
		{
			try {
				var r = new Route (null, null);
				r.GetVirtualPath (null, new RouteValueDictionary ());
			} catch (NullReferenceException) {
				// .NET lacks null arg check here. (No need to mimic silly behavior here.)
				throw new ArgumentNullException ();
			}
		}

		[Test]
		public void GetVirtualPathNullValues ()
		{
			// null values is allowed.
			var r = new Route (null, null);
			var rd = new RouteData ();
			var vp = r.GetVirtualPath (new RequestContext (new HttpContextStub (), rd), null);
			Assert.AreEqual (String.Empty, vp.VirtualPath, "#1");
			Assert.AreEqual (r, vp.Route, "#2");
		}

		[Test]
		public void GetVirtualPath ()
		{
			var r = new Route ("foo/bar", null);
			var rd = new RouteData ();
			var vp = r.GetVirtualPath (new RequestContext (new HttpContextStub ("~/foo/bar"), rd), null);
			Assert.AreEqual ("foo/bar", vp.VirtualPath, "#1");
			Assert.AreEqual (r, vp.Route, "#2");

			vp = r.GetVirtualPath (new RequestContext (new HttpContextStub ("~/foo/bar/baz"), rd), null);
			Assert.AreEqual ("foo/bar", vp.VirtualPath, "#3");
			Assert.AreEqual (r, vp.Route, "#4");
		}

		[Test]
		public void GetVirtualPath2 ()
		{
			var r = new Route ("{foo}/{bar}", null);
			var hc = new HttpContextStub ("~/x/y", String.Empty);
			var rd = r.GetRouteData (hc);
			var vp = r.GetVirtualPath (new RequestContext (hc, rd), null);
			Assert.IsNotNull (vp, "#1");
			Assert.AreEqual ("x/y", vp.VirtualPath, "#2");
			Assert.AreEqual (r, vp.Route, "#3");
			Assert.AreEqual (0, vp.DataTokens.Count, "#4");
		}

		[Test]
		public void GetVirtualPath3 ()
		{
			var r = new MyRoute ("{foo}/{bar}", new MyRouteHandler ());
			var hc = new HttpContextStub2 ("~/x/y", String.Empty);
			var rd = r.GetRouteData (hc);
			var vp = r.GetVirtualPath (new RequestContext (hc, rd), rd.Values);

			Assert.IsNotNull (vp, "#1");
			Assert.AreEqual ("x/y", vp.VirtualPath, "#2");
			Assert.AreEqual (r, vp.Route, "#3");
			Assert.AreEqual (0, vp.DataTokens.Count, "#4");
		}

		[Test]
		public void GetVirtualPath4 ()
		{
			var r = new MyRoute ("{foo}/{bar}", new MyRouteHandler ());
			var hc = new HttpContextStub2 ("~/x/y", String.Empty);
			var rd = r.GetRouteData (hc);

			// override a value incompletely
			var values = new RouteValueDictionary ();
			values ["foo"] = "A";

			var vp = r.GetVirtualPath (new RequestContext (hc, rd), values);
			Assert.IsNull (vp);
		}

		[Test]
		public void GetVirtualPath5 ()
		{
			var r = new MyRoute ("{foo}/{bar}", new MyRouteHandler ());
			var hc = new HttpContextStub2 ("~/x/y", String.Empty);
			var rd = r.GetRouteData (hc);

			// override values completely.
			var values = new RouteValueDictionary ();
			values ["foo"] = "A";
			values ["bar"] = "B";

			var vp = r.GetVirtualPath (new RequestContext (hc, rd), values);

			Assert.IsNotNull (vp, "#1");
			Assert.AreEqual ("A/B", vp.VirtualPath, "#2");
			Assert.AreEqual (r, vp.Route, "#3");
			Assert.AreEqual (0, vp.DataTokens.Count, "#4");
		}

		// Bug #500739
		[Test]
		public void RouteGetRequiredStringWithDefaults ()
		{
			var routes = new RouteValueDictionary ();
			var route = new Route ("Hello/{name}", new MyRouteHandler ()) {
					Defaults = new RouteValueDictionary (new {controller = "Home", action = "Hello"})
				};
			routes.Add ("Name", route);

			var hc = new HttpContextStub2 ("~/Hello/World", String.Empty);
			var rd = route.GetRouteData (hc);

			Assert.IsNotNull (rd, "#A1");
			Assert.AreEqual ("Home", rd.GetRequiredString ("controller"), "#A2");
			Assert.AreEqual ("Hello", rd.GetRequiredString ("action"), "#A3");
			Assert.AreEqual ("World", rd.Values ["name"], "#A4");
		}
	}
}
