//
// RouteTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
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
using System.IO;
using System.Web;
using System.Web.Routing;
using NUnit.Framework;

namespace MonoTests.System.Web.Routing
{
	class TestUrl
	{
		public string Url { get; set; }
		public string Expected { get; set; }
		public string Label { get; set; }
		public Type ExpectedExceptionType { get; set; }	
	}

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

		static readonly TestUrl[] __invalidUrls = {
			// cannot start with '~'
			new TestUrl () { Url = "~", ExpectedExceptionType = typeof (ArgumentException), Label = "#1" },

			// cannot start with '/'
			new TestUrl () { Url = "/", ExpectedExceptionType = typeof (ArgumentException), Label = "#2" },

			// cannot contain '?'
			new TestUrl () { Url = "foo?bar", ExpectedExceptionType = typeof (ArgumentException), Label = "#3" },

			// unmatched '{'
			new TestUrl () { Url = "foo/{bar", ExpectedExceptionType = typeof (ArgumentException), Label = "#4" },

			// unmatched '}'
			new TestUrl () { Url = "foo/bar}", ExpectedExceptionType = typeof (ArgumentException), Label = "#5"  },

			// "" is an invalid parameter name.
			new TestUrl () { Url = "foo/{}", ExpectedExceptionType = typeof (ArgumentException), Label = "#6" },

			// incomplete parameter in path segment.
			new TestUrl () { Url = "foo/{x/y/z}", ExpectedExceptionType = typeof (ArgumentException), Label = "#7" },

			// regarded as an incomplete parameter
			new TestUrl () { Url = "foo/{a{{b}}c}", ExpectedExceptionType = typeof (ArgumentException), Label = "#8" },

			// consecutive parameters are not allowed
			new TestUrl () { Url = "foo/{a}{b}", ExpectedExceptionType = typeof (ArgumentException), Label = "#9" },

			// consecutive segment separators '/' are not allowed
			new TestUrl () { Url = "foo//bar", ExpectedExceptionType = typeof (ArgumentException), Label = "#10" },

			// A catch-all parameter can only appear as the last segment of the route URL
			new TestUrl () { Url = "{first}/{*rest}/{foo}", ExpectedExceptionType = typeof (ArgumentException), Label = "#11" },

			// A path segment that contains more than one section, such as a literal section or a parameter, cannot contain a catch-all parameter.
			new TestUrl () { Url = "{first}/{*rest}-{foo}", ExpectedExceptionType = typeof (ArgumentException), Label = "#12" },

			// A path segment that contains more than one section, such as a literal section or a parameter, cannot contain a catch-all parameter.
			new TestUrl () { Url = "{first}/{foo}-{*rest}", ExpectedExceptionType = typeof (ArgumentException), Label = "#13" },

			// A path segment that contains more than one section, such as a literal section or a parameter, cannot contain a catch-all parameter.
			new TestUrl () { Url = "-{*rest}", ExpectedExceptionType = typeof (ArgumentException), Label = "#14" },
		};
		
		[Test]
		public void InvalidUrls ()
		{
			Route r;

			foreach (TestUrl tu in __invalidUrls) {
				AssertExtensions.Throws (tu.ExpectedExceptionType, () => r = new Route (tu.Url, null), tu.Label);
			}
		}

		static readonly TestUrl[] __validUrls = {
			new TestUrl () { Url = "{foo}/{bar}", Expected = "{foo}/{bar}", Label = "#1" },
			new TestUrl () { Url = "a~c", Expected = "a~c", Label = "#2" },
			new TestUrl () { Url = "foo/", Expected = "foo/", Label = "#3" },
			new TestUrl () { Url = "summary/{action}-{type}/{page}", Expected = "summary/{action}-{type}/{page}", Label = "#4" },
			new TestUrl () { Url = "{first}/{*rest}", Expected = "{first}/{*rest}", Label = "#5" },
			new TestUrl () { Url = "{language}-{country}/{controller}/{action}", Expected = "{language}-{country}/{controller}/{action}", Label = "#6" },
			new TestUrl () { Url = "{controller}.{action}.{id}", Expected = "{controller}.{action}.{id}", Label = "#7" },
			new TestUrl () { Url = "{reporttype}/{year}/{month}/{date}", Expected = "{reporttype}/{year}/{month}/{date}", Label = "#8" },
			new TestUrl () { Url = "Book{title}and{foo}", Expected = "Book{title}and{foo}", Label = "#9" },
			new TestUrl () { Url = "foo/{ }", Expected = "foo/{ }", Label = "#10" },
			new TestUrl () { Url = "foo/{ \t}", Expected = "foo/{ \t}", Label = "#11" },
			new TestUrl () { Url = "foo/{ \n}", Expected = "foo/{ \n}", Label = "#12" },
			new TestUrl () { Url = "foo/{ \t\n}", Expected = "foo/{ \t\n}", Label = "#13" },
			new TestUrl () { Url = "-{foo}", Expected = "-{foo}", Label = "#14" },
		};

		[Test]
		public void ValidUrls ()
		{
			Route r;

			foreach (TestUrl tu in __validUrls) {
				r = new Route (tu.Url, null);
				Assert.AreEqual (tu.Expected, r.Url, tu.Label);
				Assert.IsNull (r.DataTokens, tu.Label + "-2");
				Assert.IsNull (r.Defaults, tu.Label + "-3");
				Assert.IsNull (r.Constraints, tu.Label + "-4");
			}
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
		public void GetRouteData7 ()
		{
			var r = new Route ("{table}/{action}.aspx", null);
			var rd = r.GetRouteData (new HttpContextStub ("~/FooTable/", String.Empty));
			Assert.IsNull (rd, "#1");
		}

		[Test]
		public void GetRouteData8 ()
		{
			var r = new Route ("{first}/{*rest}", null);
			var hc = new HttpContextStub ("~/a/b/c/d", String.Empty);
			var rd = r.GetRouteData (hc);
			Assert.IsNotNull (rd, "#1");
			Assert.AreEqual (r, rd.Route, "#2");
			Assert.AreEqual (0, rd.DataTokens.Count, "#3");
			Assert.AreEqual (2, rd.Values.Count, "#4");
			Assert.AreEqual ("a", rd.Values ["first"], "#4-1");
			Assert.AreEqual ("b/c/d", rd.Values ["rest"], "#4-2");
		}

		[Test]
		public void GetRouteData9 ()
		{
			var r = new Route ("summary/{action}-{type}/{page}", null);
			var hc = new HttpContextStub ("~/summary/test-xml/1", String.Empty);
			var rd = r.GetRouteData (hc);

			Assert.IsNotNull (rd, "#1");
			Assert.AreEqual (r, rd.Route, "#2");
			Assert.AreEqual (0, rd.DataTokens.Count, "#3");
			Assert.AreEqual (3, rd.Values.Count, "#4");
			Assert.AreEqual ("test", rd.Values["action"], "#4-1");
			Assert.AreEqual ("xml", rd.Values["type"], "#4-2");
			Assert.AreEqual ("1", rd.Values["page"], "#4-2");
		}

		[Test]
		public void GetRouteData10 ()
		{
			var r = new Route ("summary/{action}-{type}/{page}", null);
			var hc = new HttpContextStub ("~/summary/-xml/1", String.Empty);
			var rd = r.GetRouteData (hc);

			Assert.IsNull (rd, "#1"); // mismatch, missing action
		}

		[Test]
		public void GetRouteData11 ()
		{
			var r = new Route ("summary/{action}-{type}/{page}", null);
			var hc = new HttpContextStub ("~/summary/test-/1", String.Empty);
			var rd = r.GetRouteData (hc);

			Assert.IsNull (rd, "#1"); // mismatch, missing type
		}

		[Test]
		public void GetRouteData12 ()
		{
			var r = new Route ("summary/{action}-{type}/{page}", null);
			var hc = new HttpContextStub ("~/summary/test-xml", String.Empty);
			var rd = r.GetRouteData (hc);

			Assert.IsNull (rd, "#1"); // mismatch, missing page
		}

		[Test]
		public void GetRouteData13 ()
		{
			var r = new Route ("summary/{action}-{type}/{page}", null) {
					Defaults = new RouteValueDictionary (new { action = "Index", page = 1 } )
						};
			var hc = new HttpContextStub ("~/summary/test-xml/1", String.Empty);
			var rd = r.GetRouteData (hc);

			Assert.IsNotNull (rd, "#1");
			Assert.AreEqual (r, rd.Route, "#2");
			Assert.AreEqual (0, rd.DataTokens.Count, "#3");
			Assert.AreEqual (3, rd.Values.Count, "#4");
			Assert.AreEqual ("test", rd.Values["action"], "#4-1");
			Assert.AreEqual ("xml", rd.Values["type"], "#4-2");
			Assert.AreEqual ("1", rd.Values["page"], "#4-3");
		}

		[Test]
		public void GetRouteData14 ()
		{
			var r = new Route ("{filename}.{ext}", null);
			var hc = new HttpContextStub ("~/Foo.xml.aspx", String.Empty);
			var rd = r.GetRouteData (hc);

			Assert.IsNotNull (rd, "#1");
			Assert.AreEqual (r, rd.Route, "#2");
			Assert.AreEqual (0, rd.DataTokens.Count, "#3");
			Assert.AreEqual (2, rd.Values.Count, "#4");
			Assert.AreEqual ("Foo.xml", rd.Values["filename"], "#4-1");
			Assert.AreEqual ("aspx", rd.Values["ext"], "#4-2");
		}

		[Test]
		public void GetRouteData15 ()
		{
			var r = new Route ("My{location}-{sublocation}", null);
			var hc = new HttpContextStub ("~/MyHouse-LivingRoom", String.Empty);
			var rd = r.GetRouteData (hc);

			Assert.IsNotNull (rd, "#1");
			Assert.AreEqual (r, rd.Route, "#2");
			Assert.AreEqual (0, rd.DataTokens.Count, "#3");
			Assert.AreEqual (2, rd.Values.Count, "#4");
			Assert.AreEqual ("House", rd.Values["location"], "#4-1");
			Assert.AreEqual ("LivingRoom", rd.Values["sublocation"], "#4-2");
		}

		[Test]
		public void GetRouteData16 ()
		{
			var r = new Route ("My{location}---{sublocation}", null);
			var hc = new HttpContextStub ("~/MyHouse-LivingRoom", String.Empty);
			var rd = r.GetRouteData (hc);

			Assert.IsNull (rd, "#1");
		}

		[Test]
		public void GetRouteData17 ()
		{
			var r = new Route ("{foo}xyz{bar}", null);
			var hc = new HttpContextStub ("~/xyzxyzxyzblah", String.Empty);
			var rd = r.GetRouteData (hc);

			Assert.IsNotNull (rd, "#1");
			Assert.AreEqual (r, rd.Route, "#2");
			Assert.AreEqual (0, rd.DataTokens.Count, "#3");
			Assert.AreEqual (2, rd.Values.Count, "#4");
			Assert.AreEqual ("xyzxyz", rd.Values["foo"], "#4-1");
			Assert.AreEqual ("blah", rd.Values["bar"], "#4-2");
		}

		[Test]
		public void GetRouteData18 ()
		{
			var r = new Route ("foo/{ }", null);
			var hc = new HttpContextStub ("~/foo/stuff", String.Empty);
			var rd = r.GetRouteData (hc);

			Assert.IsNotNull (rd, "#1");
			Assert.AreEqual (r, rd.Route, "#2");
			Assert.AreEqual (0, rd.DataTokens.Count, "#3");
			Assert.AreEqual (1, rd.Values.Count, "#4");
			Assert.AreEqual ("stuff", rd.Values[" "], "#4-1");
		}

		[Test]
		public void GetRouteData19 ()
		{
			var r = new Route ("foo/{ \t}", null);
			var hc = new HttpContextStub ("~/foo/stuff", String.Empty);
			var rd = r.GetRouteData (hc);

			Assert.IsNotNull (rd, "#1");
			Assert.AreEqual (r, rd.Route, "#2");
			Assert.AreEqual (0, rd.DataTokens.Count, "#3");
			Assert.AreEqual (1, rd.Values.Count, "#4");
			Assert.AreEqual ("stuff", rd.Values[" \t"], "#4-1");
		}

		[Test]
		public void GetRouteData20 ()
		{
			var r = new Route ("foo/{ \n}", null);
			var hc = new HttpContextStub ("~/foo/stuff", String.Empty);
			var rd = r.GetRouteData (hc);

			Assert.IsNotNull (rd, "#1");
			Assert.AreEqual (r, rd.Route, "#2");
			Assert.AreEqual (0, rd.DataTokens.Count, "#3");
			Assert.AreEqual (1, rd.Values.Count, "#4");
			Assert.AreEqual ("stuff", rd.Values[" \n"], "#4-1");
		}

		[Test]
		public void GetRouteData21 ()
		{
			var r = new Route ("foo/{ \t\n}", null);
			var hc = new HttpContextStub ("~/foo/stuff", String.Empty);
			var rd = r.GetRouteData (hc);

			Assert.IsNotNull (rd, "#1");
			Assert.AreEqual (r, rd.Route, "#2");
			Assert.AreEqual (0, rd.DataTokens.Count, "#3");
			Assert.AreEqual (1, rd.Values.Count, "#4");
			Assert.AreEqual ("stuff", rd.Values[" \t\n"], "#4-1");
		}

		[Test]
		public void GetRouteData22 ()
		{
			var r = new Route ("foo/{ \t\n}", null);
			var hc = new HttpContextStub ("~/FOO/stuff", String.Empty);
			var rd = r.GetRouteData (hc);

			Assert.IsNotNull (rd, "#1");
			Assert.AreEqual (r, rd.Route, "#2");
			Assert.AreEqual (0, rd.DataTokens.Count, "#3");
			Assert.AreEqual (1, rd.Values.Count, "#4");
			Assert.AreEqual ("stuff", rd.Values[" \t\n"], "#4-1");
		}

		[Test]
		public void GetRouteData23 ()
		{
			var r = new Route ("foo/{bar}-{baz}/{dancefloor}", null) {
					Defaults = new RouteValueDictionary (new { bar = "BlueOyster", dancefloor = 1 })
			};
				
			var hc = new HttpContextStub ("~/foo/-nyc/1", String.Empty);
			var rd = r.GetRouteData (hc);
			Assert.IsNull (rd, "#1");

			hc = new HttpContextStub ("~/foo/blueoyster-nyc", String.Empty);
			rd = r.GetRouteData (hc);
			Assert.IsNotNull (rd, "#2");
			Assert.AreEqual (r, rd.Route, "#2-1");
			Assert.AreEqual (0, rd.DataTokens.Count, "#2-2");
			Assert.AreEqual (3, rd.Values.Count, "#2-3");
			Assert.AreEqual ("blueoyster", rd.Values["bar"], "#2-4");
			Assert.AreEqual ("nyc", rd.Values["baz"], "#2-5");
			Assert.AreEqual (1, rd.Values["dancefloor"], "#2-6");
			Assert.IsTrue (typeof (int) == rd.Values["dancefloor"].GetType (), "#2-7");
		}

		[Test]
		public void GetRouteData24 ()
		{
			var r = new Route ("foo/{bar}-{baz}/{dancefloor}", null) {
					Defaults = new RouteValueDictionary (new { baz = "nyc", dancefloor = 1 })
			};
				
			var hc = new HttpContextStub ("~/foo/BlueOyster-/1", String.Empty);
			var rd = r.GetRouteData (hc);
			Assert.IsNull (rd, "#1");

			hc = new HttpContextStub ("~/foo/blueoyster-", String.Empty);
			rd = r.GetRouteData (hc);
			Assert.IsNull (rd, "#2");

			hc = new HttpContextStub ("~/foo/blueoyster-nyc", String.Empty);
			rd = r.GetRouteData (hc);
			
			Assert.IsNotNull (rd, "#3");
			Assert.AreEqual (r, rd.Route, "#3-1");
			Assert.AreEqual (0, rd.DataTokens.Count, "#3-2");
			Assert.AreEqual (3, rd.Values.Count, "#3-3");
			Assert.AreEqual ("blueoyster", rd.Values["bar"], "#3-4");
			Assert.AreEqual ("nyc", rd.Values["baz"], "#3-5");
			Assert.AreEqual (1, rd.Values["dancefloor"], "#3-6");
			Assert.IsTrue (typeof (int) == rd.Values["dancefloor"].GetType (), "#3-7");

			hc = new HttpContextStub ("~/foo/blueoyster-nyc/4", String.Empty);
			rd = r.GetRouteData (hc);
			
			Assert.IsNotNull (rd, "#4");
			Assert.AreEqual (r, rd.Route, "#4-1");
			Assert.AreEqual (0, rd.DataTokens.Count, "#4-2");
			Assert.AreEqual (3, rd.Values.Count, "#4-3");
			Assert.AreEqual ("blueoyster", rd.Values["bar"], "#4-4");
			Assert.AreEqual ("nyc", rd.Values["baz"], "#4-5");
			Assert.AreEqual ("4", rd.Values["dancefloor"], "#4-6");
			Assert.IsTrue (typeof (string) == rd.Values["dancefloor"].GetType (), "#4-7");
		}

		[Test]
		public void GetRouteData25 ()
		{
			var r = new Route ("foo/{bar}/{baz}/{dancefloor}", null) {
					Defaults = new RouteValueDictionary (new { baz = "nyc", dancefloor = 1 })
			};
				
			var hc = new HttpContextStub ("~/foo/BlueOyster", String.Empty);
			var rd = r.GetRouteData (hc);
			Assert.IsNotNull (rd, "#1");
			Assert.AreEqual (r, rd.Route, "#1-1");
			Assert.AreEqual (0, rd.DataTokens.Count, "#1-2");
			Assert.AreEqual (3, rd.Values.Count, "#1-3");
			Assert.AreEqual ("BlueOyster", rd.Values["bar"], "#1-4");
			Assert.AreEqual ("nyc", rd.Values["baz"], "#1-5");
			Assert.AreEqual (1, rd.Values["dancefloor"], "#1-6");
			Assert.IsTrue (typeof (int) == rd.Values["dancefloor"].GetType (), "#1-7");
			
			hc = new HttpContextStub ("~/foo/blueoyster/bigapple", String.Empty);
			rd = r.GetRouteData (hc);
			Assert.IsNotNull (rd, "#2");
			Assert.AreEqual (r, rd.Route, "#2-1");
			Assert.AreEqual (0, rd.DataTokens.Count, "#2-2");
			Assert.AreEqual (3, rd.Values.Count, "#2-3");
			Assert.AreEqual ("blueoyster", rd.Values["bar"], "#2-4");
			Assert.AreEqual ("bigapple", rd.Values["baz"], "#2-5");
			Assert.AreEqual (1, rd.Values["dancefloor"], "#2-6");
			Assert.IsTrue (typeof (int) == rd.Values["dancefloor"].GetType (), "#2-7");
			
			hc = new HttpContextStub ("~/foo/blueoyster/bigapple/3", String.Empty);
			rd = r.GetRouteData (hc);
			
			Assert.IsNotNull (rd, "#3");
			Assert.AreEqual (r, rd.Route, "#3-1");
			Assert.AreEqual (0, rd.DataTokens.Count, "#3-2");
			Assert.AreEqual (3, rd.Values.Count, "#3-3");
			Assert.AreEqual ("blueoyster", rd.Values["bar"], "#3-4");
			Assert.AreEqual ("bigapple", rd.Values["baz"], "#3-5");
			Assert.AreEqual ("3", rd.Values["dancefloor"], "#3-6");
			Assert.IsTrue (typeof (string) == rd.Values["dancefloor"].GetType (), "#3-7");
		}

		[Test]
		public void GetRouteData26 ()
		{
			var r = new Route ("foo/{bar}/{baz}-{dancefloor}", null) {
					Defaults = new RouteValueDictionary (new { baz = "nyc", dancefloor = 1 })
			};
				
			var hc = new HttpContextStub ("~/foo/BlueOyster", String.Empty);
			var rd = r.GetRouteData (hc);
			Assert.IsNull (rd, "#1");
			
			hc = new HttpContextStub ("~/foo/blueoyster/bigapple", String.Empty);
			rd = r.GetRouteData (hc);
			Assert.IsNull (rd, "#2");
			
			hc = new HttpContextStub ("~/foo/blueoyster/bigapple-3", String.Empty);
			rd = r.GetRouteData (hc);
			
			Assert.IsNotNull (rd, "#3");
			Assert.AreEqual (r, rd.Route, "#3-1");
			Assert.AreEqual (0, rd.DataTokens.Count, "#3-2");
			Assert.AreEqual (3, rd.Values.Count, "#3-3");
			Assert.AreEqual ("blueoyster", rd.Values["bar"], "#3-4");
			Assert.AreEqual ("bigapple", rd.Values["baz"], "#3-5");
			Assert.AreEqual ("3", rd.Values["dancefloor"], "#3-6");
			Assert.IsTrue (typeof (string) == rd.Values["dancefloor"].GetType (), "#3-7");

			hc = new HttpContextStub ("~/foo/blueoyster/-", String.Empty);
			rd = r.GetRouteData (hc);
			
			Assert.IsNull (rd, "#4");
		}

		[Test]
		public void GetRouteData27 ()
		{
			var r = new Route ("foo/{bar}/{baz}/{dancefloor}", null) {
					Defaults = new RouteValueDictionary (new { bar = "BlueOyster", dancefloor = 1 })
			};
				
			var hc = new HttpContextStub ("~/foo/nyc", String.Empty);
			var rd = r.GetRouteData (hc);
			Assert.IsNull (rd, "#1");
		}

		[Test]
		public void GetRouteData28 ()
		{
			var r = new Route ("{foo}xyz{bar}xyz{baz}", null);
			var hc = new HttpContextStub ("~/xyzxyzxyzxyzblah", String.Empty);
			var rd = r.GetRouteData (hc);

#if NET_4_0 || !DOTNET
			// When running on Mono this test succeeds - it was a bug in .NET routing for 3.5 which
			// we don't reproduce anymore.
			Assert.IsNotNull (rd, "#1");
			Assert.AreEqual (r, rd.Route, "#2");
			Assert.AreEqual (0, rd.DataTokens.Count, "#3");
			Assert.AreEqual (3, rd.Values.Count, "#4");
			Assert.AreEqual ("xyz", rd.Values ["foo"], "#4-1");
			Assert.AreEqual ("xyz", rd.Values ["bar"], "#4-2");
			Assert.AreEqual ("blah", rd.Values ["baz"], "#4-3");
#else
			Assert.IsNull (rd, "#1");
#endif
		}

		[Test]
		public void GetRouteData29 ()
		{
			var r = new Route ("{foo}xyz{bar}", null);
			var hc = new HttpContextStub ("~/xyzxyzxyzxyzblah", String.Empty);
			var rd = r.GetRouteData (hc);

			Assert.IsNotNull (rd, "#1");
			Assert.AreEqual (r, rd.Route, "#2");
			Assert.AreEqual (0, rd.DataTokens.Count, "#3");
			Assert.AreEqual (2, rd.Values.Count, "#4");
			Assert.AreEqual ("xyzxyzxyz", rd.Values["foo"], "#4-1");
			Assert.AreEqual ("blah", rd.Values["bar"], "#4-2");
		}

		[Test]
		public void GetRouteData30 ()
		{
			var r = new Route ("{foo}/bar/{baz}/boo/{blah}", null) {
					Defaults = new RouteValueDictionary (new { baz = "meep", blah = "blurb" })
			};
				
			var hc = new HttpContextStub ("~/foo/bar", String.Empty);
			var rd = r.GetRouteData (hc);

			Assert.IsNull (rd, "#1");

			hc = new HttpContextStub ("~/foo/bar/baz/boo", String.Empty);
			rd = r.GetRouteData (hc);

			Assert.IsNotNull (rd, "#2");
			Assert.AreEqual (r, rd.Route, "#3");
			Assert.AreEqual (0, rd.DataTokens.Count, "#4");
			Assert.AreEqual (3, rd.Values.Count, "#4");
			Assert.AreEqual ("foo", rd.Values["foo"], "#4-1");
			Assert.AreEqual ("baz", rd.Values["baz"], "#4-2");
			Assert.AreEqual ("blurb", rd.Values["blah"], "#4-3");
		}

		[Test(Description = "Bug #523330")]
		public void GetRouteData31()
		{
			var r = new Route("{controller}/{action}", null)
			{
				Defaults = new RouteValueDictionary(new { controller = "Home", action = "Index" }),
				DataTokens = new RouteValueDictionary(new { foobar = "bar" })
			};

			var hc = new HttpContextStub("~/", String.Empty);
			var rd = r.GetRouteData(hc);

			Assert.IsNotNull(rd, "#1");
			Assert.AreEqual(r, rd.Route, "#2");
			Assert.AreEqual(1, rd.DataTokens.Count, "#3");
			Assert.AreEqual(2, rd.Values.Count, "#4");
			Assert.AreEqual("Home", rd.Values["controller"], "#4-1");
			Assert.AreEqual("Index", rd.Values["action"], "#4-2");
			Assert.IsNull(rd.Values["foobar"], "#4-3");
			Assert.IsNotNull(rd.DataTokens, "#5");
			Assert.AreEqual(1, rd.DataTokens.Count, "#6");
			Assert.AreEqual("bar", rd.DataTokens["foobar"], "#6-1");
		}

		[Test (Description = "Bug #537751")]
		public void GetRouteData32 ()
		{
			var r = new Route ("", null);
			var hc = new HttpContextStub ("~/", String.Empty);
			var rd = r.GetRouteData (hc);

			Assert.IsNotNull (rd, "#1");
			Assert.AreEqual (r, rd.Route, "#2");
			Assert.AreEqual (0, rd.DataTokens.Count, "#3");
			Assert.AreEqual (0, rd.Values.Count, "#4");
		}

		[Test (Description = "Bug #537751")]
		public void GetRouteData33 ()
		{
			var r = new Route (null, null);
			var hc = new HttpContextStub ("~/", String.Empty);
			var rd = r.GetRouteData (hc);

			Assert.IsNotNull (rd, "#1");
			Assert.AreEqual (r, rd.Route, "#2");
			Assert.AreEqual (0, rd.DataTokens.Count, "#3");
			Assert.AreEqual (0, rd.Values.Count, "#4");
		}

		[Test]
		public void GetRouteData34 ()
		{
			var r = new Route ("{foo}xyz{bar}xyz{baz}", null);
			var hc = new HttpContextStub ("~/xyzxyzxyzxyzxyzxyzblah", String.Empty);
			var rd = r.GetRouteData (hc);

#if NET_4_0 || !DOTNET
			// When running on Mono this test succeeds - it was a bug in .NET routing for 3.5 which
			// we don't reproduce anymore.
			Assert.IsNotNull (rd, "#1");
			Assert.AreEqual (r, rd.Route, "#2");
			Assert.AreEqual (0, rd.DataTokens.Count, "#3");
			Assert.AreEqual (3, rd.Values.Count, "#4");
			Assert.AreEqual ("xyzxyzxyz", rd.Values ["foo"], "#4-1");
			Assert.AreEqual ("xyz", rd.Values ["bar"], "#4-2");
			Assert.AreEqual ("blah", rd.Values ["baz"], "#4-3");
#else
			Assert.IsNull (rd, "#1");
#endif
		}

		[Test]
		public void GetRouteData35 ()
		{
			var r = new Route ("{foo}xyz{bar}xyz{baz}", null);
			var hc = new HttpContextStub ("~/xyzxyzxyzdabxyzblah", String.Empty);
			var rd = r.GetRouteData (hc);

			Assert.IsNotNull (rd, "#1");
			Assert.AreEqual (r, rd.Route, "#2");
			Assert.AreEqual (0, rd.DataTokens.Count, "#3");
			Assert.AreEqual (3, rd.Values.Count, "#4");
			Assert.AreEqual ("xyzxyz", rd.Values ["foo"], "#4-1");
			Assert.AreEqual ("dab", rd.Values ["bar"], "#4-2");
			Assert.AreEqual ("blah", rd.Values ["baz"], "#4-3");
		}

		[Test]
		public void GetRouteData36 ()
		{
			var r = new Route ("xyz{foo}xyz{bar}xyz{baz}", null);
			var hc = new HttpContextStub ("~/xyzxyzxyzdabxyzblah", String.Empty);
			var rd = r.GetRouteData (hc);

#if NET_4_0 || !DOTNET
			// When running on Mono this test succeeds - it was a bug in .NET routing for 3.5 which
			// we don't reproduce anymore.
			Assert.IsNotNull (rd, "#1");
			Assert.AreEqual (r, rd.Route, "#2");
			Assert.AreEqual (0, rd.DataTokens.Count, "#3");
			Assert.AreEqual (3, rd.Values.Count, "#4");
			Assert.AreEqual ("xyz", rd.Values ["foo"], "#4-1");
			Assert.AreEqual ("dab", rd.Values ["bar"], "#4-2");
			Assert.AreEqual ("blah", rd.Values ["baz"], "#4-3");
#else
			Assert.IsNull (rd, "#1");
#endif
		}

		[Test]
		public void GetRouteData37 ()
		{
			var r = new Route ("{foo}xyz{bar}xyz{baz}", null);
			var hc = new HttpContextStub ("~/xyzxyzxyzxyzxyz", String.Empty);
			var rd = r.GetRouteData (hc);

#if NET_4_0 || !DOTNET
			// When running on Mono this test succeeds - it was a bug in .NET routing for 3.5 which
			// we don't reproduce anymore.
			Assert.IsNotNull (rd, "#1");
			Assert.AreEqual (r, rd.Route, "#2");
			Assert.AreEqual (0, rd.DataTokens.Count, "#3");
			Assert.AreEqual (3, rd.Values.Count, "#4");
			Assert.AreEqual ("xyz", rd.Values ["foo"], "#4-1");
			Assert.AreEqual ("xyz", rd.Values ["bar"], "#4-2");
			Assert.AreEqual ("xyz", rd.Values ["baz"], "#4-3");
#else
			Assert.IsNull (rd, "#1");
#endif
		}

		[Test]
		public void GetRouteData38 ()
		{
			// {} matches and substitutes even at partial state ...
			var r = new Route ("{foo}/bar{baz}", null);
			var hc = new HttpContextStub ("~/x/bartest", String.Empty);
			var rd = r.GetRouteData (hc);
			Assert.IsNotNull (rd, "#1");
			Assert.AreEqual (r, rd.Route, "#2");
			Assert.AreEqual (0, rd.DataTokens.Count, "#3");
			Assert.AreEqual (2, rd.Values.Count, "#4");
			Assert.AreEqual ("x", rd.Values ["foo"], "#4-1");
			Assert.AreEqual ("test", rd.Values ["baz"], "#4-2");
		}

		[Test]
		public void GetRouteData39 ()
		{
			// {} matches and substitutes even at partial state ...
			var r = new Route ("{foo}/bar{baz}", null);
			var hc = new HttpContextStub ("~/x/barte", String.Empty);
			var rd = r.GetRouteData (hc);
			Assert.IsNotNull (rd, "#1");
			Assert.AreEqual (r, rd.Route, "#2");
			Assert.AreEqual (0, rd.DataTokens.Count, "#3");
			Assert.AreEqual (2, rd.Values.Count, "#4");
			Assert.AreEqual ("x", rd.Values ["foo"], "#4-1");
			Assert.AreEqual ("te", rd.Values ["baz"], "#4-2");
		}

		[Test]
		public void GetRouteData40 ()
		{
			// {} matches and substitutes even at partial state ...
			var r = new Route ("{foo}/bar{baz}", null);
			var hc = new HttpContextStub ("~/x/bartes", String.Empty);
			var rd = r.GetRouteData (hc);
			Assert.IsNotNull (rd, "#1");
			Assert.AreEqual (r, rd.Route, "#2");
			Assert.AreEqual (0, rd.DataTokens.Count, "#3");
			Assert.AreEqual (2, rd.Values.Count, "#4");
			Assert.AreEqual ("x", rd.Values ["foo"], "#4-1");
			Assert.AreEqual ("tes", rd.Values ["baz"], "#4-2");
		}

		[Test]
		public void GetRouteData41 ()
		{
			// {} matches and substitutes even at partial state ...
			var r = new Route ("{foo}/bartes{baz}", null);
			var hc = new HttpContextStub ("~/x/bartest", String.Empty);
			var rd = r.GetRouteData (hc);
			Assert.IsNotNull (rd, "#1");
			Assert.AreEqual (r, rd.Route, "#2");
			Assert.AreEqual (0, rd.DataTokens.Count, "#3");
			Assert.AreEqual (2, rd.Values.Count, "#4");
			Assert.AreEqual ("x", rd.Values ["foo"], "#4-1");
			Assert.AreEqual ("t", rd.Values ["baz"], "#4-2");
		}

		[Test]
		public void GetRouteData42 ()
		{
			// {} matches and substitutes even at partial state ...
			var r = new Route ("{foo}/bartes{baz}", null);
			var hc = new HttpContextStub ("~/x/bartest1", String.Empty);
			var rd = r.GetRouteData (hc);
			Assert.IsNotNull (rd, "#1");
			Assert.AreEqual (r, rd.Route, "#2");
			Assert.AreEqual (0, rd.DataTokens.Count, "#3");
			Assert.AreEqual (2, rd.Values.Count, "#4");
			Assert.AreEqual ("x", rd.Values ["foo"], "#4-1");
			Assert.AreEqual ("t1", rd.Values ["baz"], "#4-2");
		}

		[Test]
		public void GetRouteData43 ()
		{
			// {} matches and substitutes even at partial state ...
			var r = new Route ("{foo}-{bar}-{baz}", null);
			var hc = new HttpContextStub ("~/--", String.Empty);
			var rd = r.GetRouteData (hc);
			Assert.IsNull (rd, "#1");

			hc = new HttpContextStub ("~/1-2-3", String.Empty);
			rd = r.GetRouteData (hc);
			Assert.IsNotNull (rd, "#2");
			Assert.AreEqual (r, rd.Route, "#2");
			Assert.AreEqual (0, rd.DataTokens.Count, "#3");
			Assert.AreEqual (3, rd.Values.Count, "#4");
			Assert.AreEqual ("1", rd.Values ["foo"], "#4-1");
			Assert.AreEqual ("2", rd.Values ["bar"], "#4-2");
			Assert.AreEqual ("3", rd.Values ["baz"], "#4-2");
		}

		[Test]
		public void GetRouteData44 ()
		{
			// {} matches and substitutes even at partial state ...
			var r = new Route ("{foo}/bartes{baz}", null);
			var hc = new HttpContextStub ("~/x/bartest/", String.Empty);
			var rd = r.GetRouteData (hc);
			Assert.IsNotNull (rd, "#1");
			Assert.AreEqual (r, rd.Route, "#2");
			Assert.AreEqual (0, rd.DataTokens.Count, "#3");
			Assert.AreEqual (2, rd.Values.Count, "#4");
			Assert.AreEqual ("x", rd.Values ["foo"], "#4-1");
			Assert.AreEqual ("t", rd.Values ["baz"], "#4-2");
		}

		[Test]
		public void GetRouteData45 ()
		{
			var r = new Route ("{foo}/{bar}", null);
			var hc = new HttpContextStub ("~/x/y/", String.Empty);
			var rd = r.GetRouteData (hc);
			Assert.IsNotNull (rd, "#1");
			Assert.AreEqual (r, rd.Route, "#2");
			Assert.AreEqual (0, rd.DataTokens.Count, "#3");
			Assert.AreEqual (2, rd.Values.Count, "#4");
			Assert.AreEqual ("x", rd.Values ["foo"], "#4-1");
			Assert.AreEqual ("y", rd.Values ["bar"], "#4-2");
		}

		[Test (Description="Bug #651593")]
		public void GetRouteData46 ()
		{
			var r = new Route ("Foo", null) {
				Defaults = new RouteValueDictionary (new {
					controller = "Foo",
					action = "Index"
				})
			};
			var hc = new HttpContextStub ("/Foo/123", String.Empty);
			var rd = r.GetRouteData (hc);
			Assert.IsNull (rd, "#1");

			r = new Route ("Foo", null) {
				Defaults = new RouteValueDictionary (new {
					controller = "Foo",
					action = "Index"
				})
			};
			hc = new HttpContextStub ("~/Foo/123", String.Empty);
			rd = r.GetRouteData (hc);
			Assert.IsNull (rd, "#2");
		}

		[Test (Description="Bug #651966")]
		public void GetRouteData47 ()
		{
			var r = new Route ("Foo/{id}", new StopRoutingHandler ()) {
				Defaults = new RouteValueDictionary (new {
					controller = "Foo",
					action = "Retrieve"
				}),
				Constraints = new RouteValueDictionary (new {
					id = @"\d{1,10}"
				})
			};
			
			var hc = new HttpContextStub ("~/Foo/x123", String.Empty);
			var rd = r.GetRouteData (hc);

			Assert.IsNull (rd, "#1");
		}

		[Test]
		public void GetRouteDataWithCatchAll ()
		{
			var r = new Route ("{*path}", new StopRoutingHandler ()) {
				Defaults = new RouteValueDictionary (new {
					controller = "Error",
					action = "NotFound"
				})
			};

			var hc = new HttpContextStub ("~/", String.Empty);
			var rd = r.GetRouteData (hc);

			Assert.IsNotNull (rd, "#1");

			hc = new HttpContextStub ("~/Foo/x123", String.Empty);
			rd = r.GetRouteData (hc);

			Assert.IsNotNull (rd, "#2");
		}

		[Test]
		public void GetRouteDataWithCatchAll2 ()
		{
			var r = new Route ("something/{*path}", new StopRoutingHandler ()) {
				Defaults = new RouteValueDictionary (new {
					controller = "Error",
					action = "NotFound"
				})
			};

			var hc = new HttpContextStub ("~/", String.Empty);
			var rd = r.GetRouteData (hc);

			Assert.IsNull (rd, "#1");

			hc = new HttpContextStub ("~/something", String.Empty);
			rd = r.GetRouteData (hc);

			Assert.IsNotNull (rd, "#2");
			Assert.IsNull (rd.Values["path"], "#2.1");

			hc = new HttpContextStub ("~/something/", String.Empty);
			rd = r.GetRouteData (hc);

			Assert.IsNotNull (rd, "#3");
			Assert.IsNull (rd.Values["path"], "#3.1");

			hc = new HttpContextStub ("~/something/algo", String.Empty);
			rd = r.GetRouteData (hc);

			Assert.IsNotNull (rd, "#4");
			Assert.AreEqual ("algo", rd.Values["path"], "#4.1");

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
		public void GetVirtualPath4_2 ()
		{
			var r = new MyRoute("{foo}/{bar}", new MyRouteHandler());
			var hc = new HttpContextStub2("~/x/y", String.Empty);
			var rd = r.GetRouteData(hc);

			// override a value incompletely
			var values = new RouteValueDictionary();
			values["bar"] = "A";

			var vp = r.GetVirtualPath(new RequestContext(hc, rd), values);
			Assert.IsNotNull(vp);
			Assert.AreEqual("x/A", vp.VirtualPath);
		}

		[Test]
		public void GetVirtualPath4Bis ()
		{
			var r = new MyRoute("part/{foo}/{bar}", new MyRouteHandler());
			var hc = new HttpContextStub2("~/part/x/y", String.Empty);
			var rd = r.GetRouteData(hc);

			// override a value incompletely
			var values = new RouteValueDictionary();
			values["foo"] = "A";

			var vp = r.GetVirtualPath(new RequestContext(hc, rd), values);
			Assert.IsNull(vp);
		}

		[Test]
		public void GetVirtualPath4_2Bis ()
		{
			var r = new MyRoute("part/{foo}/{bar}", new MyRouteHandler());
			var hc = new HttpContextStub2("~/part/x/y", String.Empty);
			var rd = r.GetRouteData(hc);

			// override a value incompletely
			var values = new RouteValueDictionary();
			values["bar"] = "A";

			var vp = r.GetVirtualPath(new RequestContext(hc, rd), values);
			Assert.IsNotNull(vp);
			Assert.AreEqual("part/x/A", vp.VirtualPath);
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

		[Test]
		public void GetVirtualPath6 ()
		{
			var r = new MyRoute ("summary/{action}-{type}/{page}", new MyRouteHandler ()) {
					Defaults = new RouteValueDictionary (new { action = "Index", page = 1 })
						};
			var hc = new HttpContextStub2 ("~/summary/Index-test/1", String.Empty);
			var rd = r.GetRouteData (hc);
			var values = new RouteValueDictionary (new { page = 2 });

			Assert.IsNotNull (rd, "#1");
			var vp = r.GetVirtualPath (new RequestContext (hc, rd), values);

			Assert.IsNotNull (vp, "#2");
			Assert.AreEqual ("summary/Index-test/2", vp.VirtualPath, "#2-1");
			Assert.AreEqual (r, vp.Route, "#2-2");
			Assert.AreEqual (0, vp.DataTokens.Count, "#2-3");

			values = new RouteValueDictionary (new { page = 2, extra = "stuff" });
			vp = r.GetVirtualPath (new RequestContext (hc, rd), values);

			Assert.IsNotNull (vp, "#3");
			Assert.AreEqual ("summary/Index-test/2?extra=stuff", vp.VirtualPath, "#3-2");
			Assert.AreEqual (0, vp.DataTokens.Count, "#3-3");
		}

		[Test]
		public void GetVirtualPath7 ()
		{
			var r = new MyRoute ("summary/{action}-{type}/{page}", new MyRouteHandler ()) {
					Defaults = new RouteValueDictionary (new { action = "Index", page = 1 })
			};
			var hc = new HttpContextStub2 ("~/summary/Index-test/1", String.Empty);
			var rd = r.GetRouteData (hc);
			var values = new RouteValueDictionary (new { page = 2 });

			Assert.IsNotNull (rd, "#1");
			var vp = r.GetVirtualPath (new RequestContext (hc, rd), values);

			Assert.IsNotNull (vp, "#2");
			Assert.AreEqual ("summary/Index-test/2", vp.VirtualPath, "#2-1");
			Assert.AreEqual (r, vp.Route, "#2-2");
			Assert.AreEqual (0, vp.DataTokens.Count, "#2-3");
		}

		[Test]
		public void GetVirtualPath8 ()
		{
			var r = new MyRoute ("todo/{action}/{page}", new MyRouteHandler ()) {
					Defaults = new RouteValueDictionary (new { controller="todo", action="list", page=0 })
			};
			var hc = new HttpContextStub2 ("~/todo/list/2", String.Empty);
			var rd = r.GetRouteData (hc);

			Assert.IsNotNull (rd, "#1");
			
			var vp = r.GetVirtualPath (new RequestContext (hc, rd), new RouteValueDictionary (new { page = 3 }));
			Assert.IsNotNull (vp, "#2");
			Assert.AreEqual ("todo/list/3", vp.VirtualPath, "#2-1");
		}

		[Test]
		public void GetVirtualPath9 ()
		{
			var r = new MyRoute ("todo/{action}/{page}", new MyRouteHandler ()) {
					Defaults = new RouteValueDictionary {
							{"controller", "todo"},
							{"action", null},
							{"page", null}
						}
				};
			
			var hc = new HttpContextStub2 ("~/todo/list/2", String.Empty);
			var rd = r.GetRouteData (hc);

			Assert.IsNotNull (rd, "#1");
			
			var vp = r.GetVirtualPath (new RequestContext (hc, rd), new RouteValueDictionary (new { page = 3 }));
			Assert.IsNotNull (vp, "#2");
			Assert.AreEqual ("todo/list/3", vp.VirtualPath, "#2-1");
		}

		[Test]
		public void GetVirtualPath10 ()
		{
			var r = new MyRoute ("{foo}/{bar}", new MyRouteHandler ());
			var hc = new HttpContextStub ("~/foo/bar", String.Empty);
			var rd = r.GetRouteData (hc);

			Assert.IsNotNull (rd, "#1");

			var vp = r.GetVirtualPath (new RequestContext (hc, rd), new RouteValueDictionary (new { page = 3 }));
			Assert.IsNotNull (vp, "#2");
			Assert.AreEqual ("foo/bar?page=3", vp.VirtualPath, "#2-1");

			vp = r.GetVirtualPath (new RequestContext (hc, rd), new RouteValueDictionary (new { page = 3, another = "stuff" }));
			Assert.IsNotNull (vp, "#3");
			Assert.AreEqual ("foo/bar?page=3&another=stuff", vp.VirtualPath, "#3-1");

			vp = r.GetVirtualPath (new RequestContext (hc, rd), new RouteValueDictionary (new { page = 3, another = "stuff", value = "with ; spaces & other chars" }));
			Assert.IsNotNull (vp, "#4");
			Assert.AreEqual ("foo/bar?page=3&another=stuff&value=with%20%3B%20spaces%20%26%20other%20chars", vp.VirtualPath, "#4-1");
		}

		[Test]
		public void GetVirtualPath11 ()
		{
			var r = new MyRoute ("summary/{action}/{page}", new MyRouteHandler ()) {
					Defaults = new RouteValueDictionary (new { action = "Index", page = 1 })
						};
			var hc = new HttpContextStub2 ("~/summary/test/1", String.Empty);
			var rd = r.GetRouteData (hc);
			var values = new RouteValueDictionary (new { page = 2 });

			Assert.IsNotNull (rd, "#1");
			var vp = r.GetVirtualPath (new RequestContext (hc, rd), values);

			Assert.IsNotNull (vp, "#2");
			Assert.AreEqual ("summary/test/2", vp.VirtualPath, "#2-1");
			Assert.AreEqual (r, vp.Route, "#2-2");
			Assert.AreEqual (0, vp.DataTokens.Count, "#2-3");

			values = new RouteValueDictionary (new { page = 2, extra = "stuff" });
			vp = r.GetVirtualPath (new RequestContext (hc, rd), values);

			Assert.IsNotNull (vp, "#3");
			Assert.AreEqual ("summary/test/2?extra=stuff", vp.VirtualPath, "#3-2");
			Assert.AreEqual (0, vp.DataTokens.Count, "#3-3");
		}

		[Test]
		public void GetVirtualPath12 ()
		{
			var r = new MyRoute ("{foo}/{bar}", new MyRouteHandler ()) {
					Defaults = new RouteValueDictionary (new { bar = "baz" })
						};
						
			var hc = new HttpContextStub2 ("~/x/y", String.Empty);
			var rd = r.GetRouteData (hc);
			var values = new RouteValueDictionary ();

			// Partial override is possible if defaults are specified
			values ["foo"] = "A";
			values ["baz"] = "B";
			
			var vp = r.GetVirtualPath (new RequestContext (hc, rd), values);
			Assert.IsNotNull (vp, "#1");
			Assert.AreEqual ("A?baz=B", vp.VirtualPath, "#1-1");
		}

		[Test]
		public void GetVirtualPath13 ()
		{
			var r = new MyRoute ("{foo}/{bar}", new MyRouteHandler ()) {
					Defaults = new RouteValueDictionary (new { baz = "baz" })
						};
			var hc = new HttpContextStub2 ("~/x/y", String.Empty);
			var rd = r.GetRouteData (hc);

			// override a value incompletely
			var values = new RouteValueDictionary ();
			values ["foo"] = "A";

			var vp = r.GetVirtualPath (new RequestContext (hc, rd), values);
			Assert.IsNull (vp);
		}

		[Test]
		public void GetVirtualPath14 ()
		{
			var r = new MyRoute ("{table}/{action}.aspx", new MyRouteHandler ());
			var hc = new HttpContextStub2 ("~/x/y.aspx", String.Empty);
			var rd = r.GetRouteData (hc);

			// override a value incompletely
			var values = new RouteValueDictionary (new {
				emptyValue = String.Empty,
				nullValue = (string)null,
				nonEmptyValue = "SomeValue"
			});

			var vp = r.GetVirtualPath (new RequestContext (hc, rd), values);
			Assert.IsNotNull (vp, "#A1");
			Assert.AreEqual ("x/y.aspx?nonEmptyValue=SomeValue", vp.VirtualPath, "#A1-1");

			values["nonEmptyValue"] = "Some Value + encoding &";
			vp = r.GetVirtualPath (new RequestContext (hc, rd), values);
			Assert.IsNotNull (vp, "#B1");
			Assert.AreEqual ("x/y.aspx?nonEmptyValue=Some%20Value%20%2B%20encoding%20%26", vp.VirtualPath, "#B1-1");

		}
		
#if NET_4_0
		[Test (Description="Bug #671753")]
		public void GetVirtualPath15 ()
		{
			var context = new HttpContextWrapper (
				new HttpContext (new HttpRequest ("filename", "http://localhost/filename", String.Empty),
						 new HttpResponse (new StringWriter())
				)
			);
			var rc = new RequestContext (context, new RouteData ());

			Assert.IsNotNull (RouteTable.Routes, "#A1");
			RouteTable.Routes.MapPageRoute ("TestRoute", "{language}/testroute", "~/TestRoute.aspx", true, null,
							new RouteValueDictionary {{"language", "(ru|en)"}});

			Assert.IsNotNull(RouteTable.Routes.GetVirtualPath (rc, "TestRoute", new RouteValueDictionary {{"language", "en"}}), "#A2");

			rc.RouteData.Values["language"] = "ru";
			Assert.IsNotNull (RouteTable.Routes.GetVirtualPath (rc, "TestRoute", new RouteValueDictionary ()), "#A3");
			Assert.IsNotNull (RouteTable.Routes.GetVirtualPath (rc, "TestRoute", null), "#A4");
		}
#endif

		[Test (Description="Xamarin Bug #9116")]
		public void GetVirtualPath16 ()
		{
			var context = new HttpContextWrapper (
				new HttpContext (new HttpRequest ("filename", "http://localhost/filename", String.Empty),
						 new HttpResponse (new StringWriter())
				)
			);
			var rc = new RequestContext (context, new RouteData ());

			var route = new Route ("Hello", new MyRouteHandler ()) {
					Defaults = new RouteValueDictionary (new {controller = "Home", action = "Hello", page = 1})
			};

			var routeValues = new RouteValueDictionary
			{
				{"controller", "Home"},
				{"action", "Hello"},
				{"page", 1}
			};

			var result = route.GetVirtualPath(rc, routeValues);
			Assert.IsNotNull(result, "#A1");
			Assert.AreEqual("Hello", result.VirtualPath, "#A2");
		}

		[Test (Description="Xamarin Bug #9116")]
		public void GetVirtualPath17 () 
		{
			var context = new HttpContextWrapper (
				new HttpContext (new HttpRequest ("filename", "http://localhost/filename", String.Empty),
						 new HttpResponse (new StringWriter())
				)
			);
			var rc = new RequestContext (context, new RouteData ());

			RouteTable.Routes.Add("FirstPage", new Route ("Hello/FirstPage", new MyRouteHandler ()) {
					Defaults = new RouteValueDictionary (new {controller = "Home", action = "Hello", page = 1})
			});
			RouteTable.Routes.Add("OtherPages", new Route ("Hello/Page-{page}", new MyRouteHandler ()) {
					Defaults = new RouteValueDictionary (new {controller = "Home", action = "Hello"})
			});

			var firstPageRouteValues = new RouteValueDictionary
			{
				{"controller", "Home"},
				{"action", "Hello"},
				{"page", 1}
			};
			var secondPageRouteValues = new RouteValueDictionary
			{
				{"controller", "Home"},
				{"action", "Hello"},
				{"page", 2}
			};

			var firstPageResult = RouteTable.Routes.GetVirtualPath (rc, firstPageRouteValues);
			var secondPageResult = RouteTable.Routes.GetVirtualPath (rc, secondPageRouteValues);

			Assert.AreEqual ("/Hello/FirstPage", firstPageResult.VirtualPath, "#A1");
			Assert.AreEqual ("/Hello/Page-2", secondPageResult.VirtualPath, "#A2");
		}

		[Test (Description="Xamarin Bug #9116")]
		public void GetVirtualPath18 () 
		{
			var context = new HttpContextWrapper (
				new HttpContext (new HttpRequest ("filename", "http://localhost/filename", String.Empty),
						 new HttpResponse (new StringWriter())
				)
			);
			var rc = new RequestContext (context, new RouteData ());

			RouteTable.Routes.Add("Published", new Route ("Posts/Published", new MyRouteHandler ()) {
					Defaults = new RouteValueDictionary (new {controller = "Home", action = "Posts", published = true})
			});
			RouteTable.Routes.Add("Unpublished", new Route ("Posts/Unpublished", new MyRouteHandler ()) {
					Defaults = new RouteValueDictionary (new {controller = "Home", action = "Posts", published = false})
			});

			var publishedRouteValues = new RouteValueDictionary
			{
				{"controller", "Home"},
				{"action", "Posts"},
				{"published", true}
			};
			var unpublishedRouteValues = new RouteValueDictionary
			{
				{"controller", "Home"},
				{"action", "Posts"},
				{"published", false}
			};

			var publishedResult = RouteTable.Routes.GetVirtualPath (rc, publishedRouteValues);
			var unpublishedResult = RouteTable.Routes.GetVirtualPath (rc, unpublishedRouteValues);

			Assert.AreEqual ("/Posts/Published", publishedResult.VirtualPath, "#A1");
			Assert.AreEqual ("/Posts/Unpublished", unpublishedResult.VirtualPath, "#A2");
		}

		[Test (Description="Routes should be case insensitive - Xamarin bug #9133")]
		public void GetVirtualPath19 ()
		{
			var context = new HttpContextWrapper (
				new HttpContext (new HttpRequest ("filename", "http://localhost/filename", String.Empty),
						 new HttpResponse (new StringWriter())
				)
			);
			var rc = new RequestContext (context, new RouteData ());

			var route = new Route ("HelloWorld", new MyRouteHandler ()) {
					Defaults = new RouteValueDictionary (new {controller = "Home", action = "HelloWorld"})
			};

			var lowercase = route.GetVirtualPath (rc, new RouteValueDictionary
			{
				{"controller", "home"},
				{"action", "helloworld"}
			});
			var standardCase = route.GetVirtualPath (rc, new RouteValueDictionary
			{
				{"controller", "Home"},
				{"action", "HelloWorld"}
			});
			var uppercase = route.GetVirtualPath (rc, new RouteValueDictionary
			{
				{"controller", "HOME"},
				{"action", "HELLOWORLD"}
			});

			Assert.IsNotNull(lowercase, "#A1");
			Assert.AreEqual ("HelloWorld", lowercase.VirtualPath, "#A2");

			Assert.IsNotNull(standardCase, "#A3");
			Assert.AreEqual ("HelloWorld", standardCase.VirtualPath, "#A4");

			Assert.IsNotNull(uppercase, "#A5");
			Assert.AreEqual ("HelloWorld", uppercase.VirtualPath, "#A6");
		}

		[Test]
		public void GetVirtualPath20 ()
		{
			var r = new MyRoute("summary/{controller}/{id}/{action}", new MyRouteHandler())
			{
				Defaults = new RouteValueDictionary(new { action = "Index" })
			};
			var hc = new HttpContextStub2("~/summary/kind/1/test", String.Empty);
			var rd = r.GetRouteData(hc);
			Assert.IsNotNull(rd, "#1");

			var values = new RouteValueDictionary(new { id = "2", action = "save" });
			var vp = r.GetVirtualPath(new RequestContext(hc, rd), values);

			Assert.IsNotNull(vp, "#2");
			Assert.AreEqual("summary/kind/2/save", vp.VirtualPath, "#2-1");
			Assert.AreEqual(r, vp.Route, "#2-2");
			Assert.AreEqual(0, vp.DataTokens.Count, "#2-3");

			values = new RouteValueDictionary(new { id = "3", action = "save", extra = "stuff" });
			vp = r.GetVirtualPath(new RequestContext(hc, rd), values);

			Assert.IsNotNull(vp, "#3");
			Assert.AreEqual("summary/kind/3/save?extra=stuff", vp.VirtualPath, "#3-2");
			Assert.AreEqual(0, vp.DataTokens.Count, "#3-3");
		}

		[Test]
		public void GetVirtualPath21 ()
		{
			var r = new MyRoute("summary/{controller}/{id}/{action}", new MyRouteHandler())
			{
				Defaults = new RouteValueDictionary(new { action = "Index" })
			};
			var hc = new HttpContextStub2("~/summary/kind/1/test", String.Empty);
			var rd = r.GetRouteData(hc);
			Assert.IsNotNull(rd, "#1");
			Assert.AreEqual("1", rd.Values["id"]);

			var values = new RouteValueDictionary(new { action = "save" });
			var vp = r.GetVirtualPath(new RequestContext(hc, rd), values);

			Assert.IsNotNull(vp, "#2");
			Assert.AreEqual("summary/kind/1/save", vp.VirtualPath, "#2-1");
			Assert.AreEqual(r, vp.Route, "#2-2");
			Assert.AreEqual(0, vp.DataTokens.Count, "#2-3");

			values = new RouteValueDictionary(new { action = "save", extra = "stuff" });
			vp = r.GetVirtualPath(new RequestContext(hc, rd), values);

			Assert.IsNotNull(vp, "#3");
			Assert.AreEqual("summary/kind/1/save?extra=stuff", vp.VirtualPath, "#3-2");
			Assert.AreEqual(0, vp.DataTokens.Count, "#3-3");
		}

		[Test]
		public void GetVirtualPath22 ()
		{
			var r = new MyRoute("summary/{controller}/{id}/{action}", new MyRouteHandler())
			{
				Defaults = new RouteValueDictionary(new { action = "Index" })
			};
			var hc = new HttpContextStub2("~/summary/kind/90941a4f-daf3-4c89-a6dc-83e8de4e3db5/test", String.Empty);
			var rd = r.GetRouteData(hc);
			Assert.IsNotNull(rd, "#0");
			Assert.AreEqual("90941a4f-daf3-4c89-a6dc-83e8de4e3db5", rd.Values["id"]);

			var values = new RouteValueDictionary(new { action = "Index" });
			var vp = r.GetVirtualPath(new RequestContext(hc, rd), values);

			Assert.IsNotNull(vp, "#1");
			Assert.AreEqual("summary/kind/90941a4f-daf3-4c89-a6dc-83e8de4e3db5", vp.VirtualPath, "#1-1");
			Assert.AreEqual(r, vp.Route, "#1-2");
			Assert.AreEqual(0, vp.DataTokens.Count, "#1-3");

			values = new RouteValueDictionary(new { action = "save" });
			vp = r.GetVirtualPath(new RequestContext(hc, rd), values);

			Assert.IsNotNull(vp, "#2");
			Assert.AreEqual("summary/kind/90941a4f-daf3-4c89-a6dc-83e8de4e3db5/save", vp.VirtualPath, "#2-1");
			Assert.AreEqual(r, vp.Route, "#2-2");
			Assert.AreEqual(0, vp.DataTokens.Count, "#2-3");

			values = new RouteValueDictionary(new { action = "save", extra = "stuff" });
			vp = r.GetVirtualPath(new RequestContext(hc, rd), values);

			Assert.IsNotNull(vp, "#3");
			Assert.AreEqual("summary/kind/90941a4f-daf3-4c89-a6dc-83e8de4e3db5/save?extra=stuff", vp.VirtualPath, "#3-2");
			Assert.AreEqual(0, vp.DataTokens.Count, "#3-3");
		}

		[Test]
		public void GetVirtualPath23 ()
		{
			var r0 = new MyRoute ("summary/{id}", new MyRouteHandler());
			var r1 = new MyRoute ("summary/{controller}/{id}/{action}", new MyRouteHandler())
			{
				Defaults = new RouteValueDictionary (new { action = "Index" })
			};
			var hc = new HttpContextStub2 ("~/summary/90941a4f-daf3-4c89-a6dc-83e8de4e3db5", String.Empty);
			var rd = r0.GetRouteData (hc);
			Assert.IsNotNull (rd, "#0");
			Assert.AreEqual ("90941a4f-daf3-4c89-a6dc-83e8de4e3db5", rd.Values["id"]);

			var values = new RouteValueDictionary ()
			{
				{ "controller", "SomeThing" },
				{ "action", "Index" }
			};
			var vp = r1.GetVirtualPath (new RequestContext (hc, rd), values);

			Assert.IsNotNull (vp, "#1");
			Assert.AreEqual ("summary/SomeThing/90941a4f-daf3-4c89-a6dc-83e8de4e3db5", vp.VirtualPath, "#1-1");
			Assert.AreEqual (r1, vp.Route, "#1-2");
			Assert.AreEqual (0, vp.DataTokens.Count, "#1-3");
		}

		[Test]
		public void GetVirtualPath24 ()
		{
			var r = new MyRoute ("{controller}/{country}-{locale}/{action}", new MyRouteHandler())
			{
				Defaults = new RouteValueDictionary (new { action = "Index", country = "us", locale = "en" })
			};
			var hc = new HttpContextStub2 ("~/login", String.Empty);
			var rd = r.GetRouteData (hc);
			Assert.IsNull (rd, "#0");

			var values = new RouteValueDictionary ()
			{
				{ "controller", "SomeThing" },
				{ "action", "Index" },
				{ "country", "es" }
			};
			var vp = r.GetVirtualPath (new RequestContext (hc, new RouteData()), values);

			Assert.IsNotNull (vp, "#1");
			Assert.AreEqual ("SomeThing/es-en", vp.VirtualPath, "#1-1");
			Assert.AreEqual (r, vp.Route, "#1-2");
			Assert.AreEqual (0, vp.DataTokens.Count, "#1-3");

			// Case #2: pass no country, but locale as user value.
			values.Remove("country");
			values.Add("locale", "xx");
			vp = r.GetVirtualPath(new RequestContext(hc, new RouteData()), values);

			Assert.IsNotNull(vp, "#2");
			Assert.AreEqual("SomeThing/us-xx", vp.VirtualPath, "#2-1");
			Assert.AreEqual(r, vp.Route, "#2-2");
			Assert.AreEqual(0, vp.DataTokens.Count, "#2-3");

			// Case #3: make contry required.
			r = new MyRoute("{controller}/{country}-{locale}/{action}", new MyRouteHandler())
			{
				Defaults = new RouteValueDictionary(new { action = "Index", locale = "en" })
			};
			vp = r.GetVirtualPath(new RequestContext(hc, new RouteData()), values);

			Assert.IsNull(vp, "#3");
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

		[Test]
		public void ProcessConstraint ()
		{
			var route = new MyRoute ("hello/{name}", new MyRouteHandler ());

			Assert.IsFalse (route.DoProcessConstraint (null, "regex", "parameter", new RouteValueDictionary (), RouteDirection.IncomingRequest), "#1");

			// constraint is null
			AssertExtensions.Throws <InvalidOperationException> (
				() => route.DoProcessConstraint (null, null, "parameter", new RouteValueDictionary (), RouteDirection.IncomingRequest),
				"#2"
			);

			// constraint is neither a string or an IRouteConstraint instance
			AssertExtensions.Throws <InvalidOperationException> (
				() => route.DoProcessConstraint (null, 1, "parameter", new RouteValueDictionary (), RouteDirection.IncomingRequest),
				"#3"
			);

			AssertExtensions.Throws <ArgumentNullException> (
				() => route.DoProcessConstraint (null, "regex", null, new RouteValueDictionary (), RouteDirection.IncomingRequest),
				"#4"
			);

			Assert.IsFalse (route.DoProcessConstraint (null, "regex", String.Empty, new RouteValueDictionary (), RouteDirection.IncomingRequest), "#5");
			
			// This is a .NET programming error, so not sure if we should test for this...
			AssertExtensions.Throws <NullReferenceException> (
				() => route.DoProcessConstraint (null, "regex", "parameter", null, RouteDirection.IncomingRequest),
				"#6"
			);
		}
	}
}
