//
// RouteCollectionTest.cs
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

using MonoTests.Common;

namespace MonoTests.System.Web.Routing
{
	[TestFixture]
	public class RouteCollectionTest
	{
		[Test]
		public void ConstructorNullArgs ()
		{
			// allowed
			new RouteCollection (null);
		}

		[Test]
		public void RouteExistingFiles ()
		{
			var c = new RouteCollection ();
			Assert.IsFalse (c.RouteExistingFiles);
		}

		[Test]
		public void AddNullMame ()
		{
			var c = new RouteCollection ();
			// when name is null, no duplicate check is done.
			c.Add (null, new Route (null, null));
			c.Add (null, new Route (null, null));
		}

		[Test]
		public void AddDuplicateEmpty ()
		{
			var c = new RouteCollection ();
			// when name is "", no duplicate check is done.
			c.Add ("", new Route (null, null));
			c.Add ("", new Route (null, null));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddDuplicateName ()
		{
			var c = new RouteCollection ();
			c.Add ("x", new Route (null, null));
			c.Add ("x", new Route (null, null));
		}

		[Test]
		public void IndexForNonExistent ()
		{
			Assert.IsNull (new RouteCollection () [null]);
		}

		[Test]
		public void IndexForExistent ()
		{
			var c = new RouteCollection ();
			var r = new Route (null, null);
			c.Add ("x", r);
			Assert.AreEqual (r, c ["x"]);
		}

		[Test]
		public void IndexForNonExistentAfterRemoval ()
		{
			var c = new RouteCollection ();
			var r = new Route (null, null);
			c.Add ("x", r);
			c.Remove (r);
			Assert.IsNull(c ["x"]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetRouteDataNoRequest ()
		{
			new RouteCollection ().GetRouteData (new HttpContextStub (true));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetRouteDataNullArg ()
		{
			new RouteCollection ().GetRouteData (null);
		}

		[Test]
		public void GetRouteDataForNonExistent ()
		{
			var rd = new RouteCollection ().GetRouteData (new HttpContextStub ("~/foo"));
			Assert.IsNull (rd);
		}

		[Test]
		public void GetRouteDataForNonExistent2 ()
		{
			var rd = new RouteCollection () { RouteExistingFiles = true }.GetRouteData (new HttpContextStub2 (null, null, null));
			Assert.IsNull (rd, "#A1");
			rd = new RouteCollection ().GetRouteData (new HttpContextStub2 (null, null, null));
			Assert.IsNull (rd, "#A2");
		}

		[Test]
		public void GetRouteDataWrongPathNoRoute ()
		{
			new RouteCollection ().GetRouteData (new HttpContextStub (String.Empty, String.Empty));
		}

		/*
		comment out those tests; I cannot explain those tests.

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void GetRouteDataWrongPathOneRoute ()
		{
			var c = new RouteCollection ();
			var r = new Route ("foo", null);
			c.Add (null, r);
			// it somehow causes ArgumentOutOfRangeException for 
			// Request.AppRelativeCurrentExecutionFilePath.
			c.GetRouteData (new HttpContextStub (String.Empty, String.Empty));
		}

		[Test]
		public void GetRouteDataWrongPathOneRoute2 ()
		{
			var c = new RouteCollection ();
			var r = new Route ("foo", null);
			c.Add (null, r);
			c.GetRouteData (new HttpContextStub ("/~", String.Empty));
		}
		*/

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void GetRouteDataForPathInfoNIE ()
		{
			var c = new RouteCollection ();
			var r = new Route ("foo", null);
			c.Add (null, r);
			// it retrieves PathInfo and then dies.
			var rd = c.GetRouteData (new HttpContextStub ("~/foo"));
		}

		[Test]
		public void GetRouteDataForNullHandler ()
		{
			var c = new RouteCollection ();
			var r = new Route ("foo", null); // allowed
			c.Add (null, r);
			var rd = c.GetRouteData (new HttpContextStub ("~/foo", String.Empty));
			Assert.IsNotNull (rd, "#1");
			Assert.AreEqual (r, rd.Route, "#2");
		}

		// below tests in RouteCollection, unlike Route, do some additional checks than Route.GetVirtualPath().

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void GetVirtualPathNoApplicationPath ()
		{
			var c = new RouteCollection ();
			c.Add (new MyRoute ("{foo}/{bar}", new MyRouteHandler ()));
			var hc = new HttpContextStub2 ("~/x/y", String.Empty);
			var rd = c.GetRouteData (hc);
			// it tries to get HttpContextBase.Request.ApplicationPath and then throws NIE.
			var vpd = c.GetVirtualPath (new RequestContext (hc, rd), rd.Values);
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void GetVirtualPathNoApplyAppPathModifier ()
		{
			var c = new RouteCollection ();
			c.Add (new MyRoute ("{foo}/{bar}", new MyRouteHandler ()));
			var hc = new HttpContextStub2 ("~/x/y", String.Empty, "apppath");
			// it tries to call HttpContextBase.Response.ApplyAppPathModifier() and then causes NIE.
			hc.SetResponse (new HttpResponseStub ());
			var rd = c.GetRouteData (hc);
			var vpd = c.GetVirtualPath (new RequestContext (hc, rd), rd.Values);
		}

		[Test]
		public void GetVirtualPathCheckVirtualPathToModify ()
		{
			var c = new RouteCollection ();
			c.Add (new MyRoute ("{foo}/{bar}", new MyRouteHandler ()));
			var hc = new HttpContextStub2 ("~/x/y", String.Empty, "apppath");
			// it tries to get HttpContextBase.Response, so set it.
			hc.SetResponse (new HttpResponseStub (1));
			var rd = c.GetRouteData (hc);
			try {
				var vpd = c.GetVirtualPath (new RequestContext (hc, rd), rd.Values);
				Assert.Fail ("#1");
			} catch (ApplicationException ex) {
				Assert.AreEqual ("apppath/x/y", ex.Message, "#2");
			}
		}

		[Test]
		public void GetVirtualPath ()
		{
			var c = new RouteCollection ();
			c.Add (new MyRoute ("{foo}/{bar}", new MyRouteHandler ()));
			var hc = new HttpContextStub2 ("~/x/y", String.Empty, "apppath");
			// it tries to get HttpContextBase.Response, so set it.
			hc.SetResponse (new HttpResponseStub (2));
			var rd = c.GetRouteData (hc);
			Assert.IsNotNull (rd, "#1");
			
			var vpd = c.GetVirtualPath (new RequestContext (hc, rd), rd.Values);
			Assert.IsNotNull (vpd, "#2");
			Assert.AreEqual ("apppath/x/y_modified", vpd.VirtualPath, "#3");
			Assert.AreEqual (0, vpd.DataTokens.Count, "#4");
		}

		[Test (Description = "Bug #502555")]
		public void GetVirtualPath2 ()
		{
			var c = new RouteCollection ();
			
			c.Add ("Summary",
			       new MyRoute ("summary/{action}-{type}/{page}", new MyRouteHandler ()) { Defaults = new RouteValueDictionary (new { controller = "Summary", action = "Index", page = 1}) }
			);
			       
			c.Add ("Apis",
			       new MyRoute ("apis/{apiname}", new MyRouteHandler ()) { Defaults = new RouteValueDictionary (new { controller = "Apis", action = "Index" }) }
			);
							    
			c.Add ("Single Report",
			       new MyRoute ("report/{guid}", new MyRouteHandler ()) { Defaults = new RouteValueDictionary (new { controller = "Reports", action = "SingleReport" }) }
			);
			
			c.Add ("Reports",
			       new MyRoute ("reports/{page}", new MyRouteHandler ()) { Defaults = new RouteValueDictionary (new { controller = "Reports", action = "Index", page = 1 }) }
			);

			c.Add ("Default",
			       new MyRoute ("{controller}/{action}", new MyRouteHandler ()) { Defaults = new RouteValueDictionary (new { controller = "Home", action = "Index"}) }
			);

			var hc = new HttpContextStub2 ("~/Home/About", String.Empty, String.Empty);
			hc.SetResponse (new HttpResponseStub (2));
			var rd = c.GetRouteData (hc);
			var vpd = c.GetVirtualPath (new RequestContext (hc, rd), rd.Values);
			Assert.IsNotNull (vpd, "#A1");
			Assert.AreEqual ("/Home/About_modified", vpd.VirtualPath, "#A2");
			Assert.AreEqual (0, vpd.DataTokens.Count, "#A3");

			hc = new HttpContextStub2 ("~/Home/Index", String.Empty, String.Empty);
			hc.SetResponse (new HttpResponseStub (2));
			rd = c.GetRouteData (hc);
			vpd = c.GetVirtualPath (new RequestContext (hc, rd), rd.Values);
			Assert.IsNotNull (vpd, "#B1");
			Assert.AreEqual ("/_modified", vpd.VirtualPath, "#B2");
			Assert.AreEqual (0, vpd.DataTokens.Count, "#B3");

			hc = new HttpContextStub2 ("~/Account/LogOn", String.Empty, String.Empty);
			hc.SetResponse (new HttpResponseStub (2));
			rd = c.GetRouteData (hc);
			vpd = c.GetVirtualPath (new RequestContext (hc, rd), rd.Values);
			Assert.IsNotNull (vpd, "#C1");
			Assert.AreEqual ("/Account/LogOn_modified", vpd.VirtualPath, "#C2");
			Assert.AreEqual (0, vpd.DataTokens.Count, "#C3");

			hc = new HttpContextStub2 ("~/", String.Empty, String.Empty);
			hc.SetResponse (new HttpResponseStub (3));
			rd = c.GetRouteData (hc);
			vpd = c.GetVirtualPath (new RequestContext (hc, rd), new RouteValueDictionary (new { controller = "home" }) );
			Assert.IsNotNull (vpd, "#D1");
			Assert.AreEqual ("/", vpd.VirtualPath, "#D2");
			Assert.AreEqual (0, vpd.DataTokens.Count, "#D3");

			hc = new HttpContextStub2 ("~/", String.Empty, String.Empty);
			hc.SetResponse (new HttpResponseStub (3));
			rd = c.GetRouteData (hc);
			vpd = c.GetVirtualPath (new RequestContext (hc, rd), new RouteValueDictionary (new { controller = "Home" }) );
			Assert.IsNotNull (vpd, "#E1");
			Assert.AreEqual ("/", vpd.VirtualPath, "#E2");
			Assert.AreEqual (0, vpd.DataTokens.Count, "#E3");
		}

		[Test]
		public void GetVirtualPath3 ()
		{
			var c = new RouteCollection ();

			c.Add ("todo-route",
			       new MyRoute ("todo/{action}", new MyRouteHandler ()) { Defaults = new RouteValueDictionary (new {controller = "todo", action="list", page=0}) }
			);

			c.Add ("another-route",
			       new MyRoute ("{controller}/{action}", new MyRouteHandler ()) { Defaults = new RouteValueDictionary (new {controller = "home", action="list", page=0}) }
			);

			var hc = new HttpContextStub2 ("~/home/list", String.Empty, String.Empty);
			hc.SetResponse (new HttpResponseStub (3));
			var rd = c.GetRouteData (hc);
			Assert.IsNotNull (rd, "#1");
			Assert.AreEqual (3, rd.Values.Count, "#1-1");
			Assert.AreEqual ("home", rd.Values["controller"], "#1-2");
			Assert.AreEqual ("list", rd.Values["action"], "#1-3");
			Assert.AreEqual (0, rd.Values["page"], "#1-4");
			
			var vp = c.GetVirtualPath (new RequestContext (hc, rd), "todo-route", new RouteValueDictionary ());
			Assert.IsNotNull (vp, "#2");
			Assert.AreEqual ("/todo", vp.VirtualPath, "#2-1");

			vp = c.GetVirtualPath (new RequestContext (hc, rd), new RouteValueDictionary ());
			Assert.IsNotNull (vp, "#3");
			Assert.AreEqual ("/todo", vp.VirtualPath, "#3-1");

			vp = c.GetVirtualPath (new RequestContext (hc, rd), new RouteValueDictionary (new { controller = "home" }));
			Assert.IsNotNull (vp, "#4");
			Assert.AreEqual ("/", vp.VirtualPath, "#4-1");

			vp = c.GetVirtualPath (new RequestContext (hc, rd), new RouteValueDictionary (new { controller = "home", extra="stuff" }));
			Assert.IsNotNull (vp, "#5");
			Assert.AreEqual ("/?extra=stuff", vp.VirtualPath, "#5-1");
		}

		[Test]
		public void GetVirtualPath4 ()
		{
			var c = new RouteCollection ();

			c.Add (new MyRoute ("blog/{user}/{action}", new MyRouteHandler ()) {
					Defaults = new RouteValueDictionary {
							{ "controller", "blog" },
							{ "user", "admin" }
						}
				}
			);

			c.Add (new MyRoute ("forum/{user}/{action}", new MyRouteHandler ()) {
					Defaults = new RouteValueDictionary {
							{ "controller", "forum" },
							{ "user", "admin" }
						}
				}
			);

			var hc = new HttpContextStub2 ("~/forum/admin/Index", String.Empty, String.Empty);
			hc.SetResponse (new HttpResponseStub (3));
			var rd = c.GetRouteData (hc);
			Assert.IsNotNull (rd, "#1");

			var vp = c.GetVirtualPath (new RequestContext (hc, rd), new RouteValueDictionary (new { action="Index", controller="forum"}));
			Assert.IsNotNull (vp, "#2");
			Assert.AreEqual ("/forum/admin/Index", vp.VirtualPath, "#2-1");
			
			vp = c.GetVirtualPath (new RequestContext (hc, rd), new RouteValueDictionary (new { action="Index", controller="blah"}));
			Assert.IsNull (vp, "#3");
		}

		[Test]
		public void GetVirtualPath5 ()
		{
			var c = new RouteCollection ();

			c.Add (new MyRoute ("reports/{year}/{month}/{day}", new MyRouteHandler ()) {
					Defaults = new RouteValueDictionary {
							{ "day", 1 }
						}
				}
			);

			var hc = new HttpContextStub2 ("~/reports/2009/05", String.Empty, String.Empty);
			hc.SetResponse (new HttpResponseStub (3));
			var rd = c.GetRouteData (hc);
			Assert.IsNotNull (rd, "#1");

			var vp = c.GetVirtualPath (new RequestContext (hc, rd), new RouteValueDictionary {
					{ "year", 2007 },
					{ "month", 1 },
					{ "day", 12 },
				}
			);
			Assert.IsNotNull (vp, "#2");
			Assert.AreEqual ("/reports/2007/1/12", vp.VirtualPath, "#2-1");
			
			vp = c.GetVirtualPath (new RequestContext (hc, rd), new RouteValueDictionary {
					{ "year", 2007 },
					{ "month", 1 }
				}
			);
			Assert.IsNotNull (vp, "#3");
			Assert.AreEqual ("/reports/2007/1", vp.VirtualPath, "#3-1");

			vp = c.GetVirtualPath (new RequestContext (hc, rd), new RouteValueDictionary {
					{ "year", 2007 },
					{ "month", 1 },
					{ "day", 12 },
					{ "category", 123 }
				}
			);
			Assert.IsNotNull (vp, "#4");
			Assert.AreEqual ("/reports/2007/1/12?category=123", vp.VirtualPath, "#4-1");

			vp = c.GetVirtualPath (new RequestContext (hc, rd), new RouteValueDictionary {
					{ "year", 2007 },
				}
			);
			Assert.IsNull (vp, "#5");
		}

		[Test]
		public void GetVirtualPath6 ()
		{
			var c = new RouteCollection ();

			c.Add (new MyRoute ("reports/{year}/{month}/{day}", new MyRouteHandler ()) {
					Defaults = new RouteValueDictionary {
							{ "day", 1 }
						}
				}
			);

			var hc = new HttpContextStub2 ("~/reports/2009/05", String.Empty, "/myapp");
			hc.SetResponse (new HttpResponseStub (3));
			var rd = c.GetRouteData (hc);
			Assert.IsNotNull (rd, "#1");

			var vp = c.GetVirtualPath (new RequestContext (hc, rd), new RouteValueDictionary {
					{ "year", 2007 },
					{ "month", 1 },
					{ "day", 12 },
				}
			);
			Assert.IsNotNull (vp, "#2");
			Assert.AreEqual ("/myapp/reports/2007/1/12", vp.VirtualPath, "#2-1");
			
			vp = c.GetVirtualPath (new RequestContext (hc, rd), new RouteValueDictionary {
					{ "year", 2007 },
					{ "month", 1 }
				}
			);
			Assert.IsNotNull (vp, "#3");
			Assert.AreEqual ("/myapp/reports/2007/1", vp.VirtualPath, "#3-1");

			vp = c.GetVirtualPath (new RequestContext (hc, rd), new RouteValueDictionary {
					{ "year", 2007 },
					{ "month", 1 },
					{ "day", 12 },
					{ "category", 123 }
				}
			);
			Assert.IsNotNull (vp, "#4");
			Assert.AreEqual ("/myapp/reports/2007/1/12?category=123", vp.VirtualPath, "#4-1");
			
			vp = c.GetVirtualPath (new RequestContext (hc, rd), new RouteValueDictionary {
					{ "year", 2007 },
				}
			);
			Assert.IsNull (vp, "#5");
		}

		[Test]
		public void GetVirtualPath7 ()
		{
			var c = new RouteCollection ();

			c.Add (new MyRoute ("{table}/{action}.aspx", new MyRouteHandler ()) {
				Constraints = new RouteValueDictionary (new { action = "List|Details|Edit|Insert" }),
			});

			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;
			var rd = new RouteData ();
			var hc = new HttpContextWrapper (ctx);

			var vp = c.GetVirtualPath (new RequestContext (hc, rd), new RouteValueDictionary {
				{"Table", "FooTable"},
				{"Action", "Details"}
			});

			Assert.IsNotNull (vp, "#A1");
			Assert.AreEqual ("/FooTable/Details.aspx", vp.VirtualPath, "#A1-1");

			vp = c.GetVirtualPath (new RequestContext (hc, rd), new RouteValueDictionary {
				{"Table", "FooTable"},
				{"Action", String.Empty}
			});

			Assert.IsNull (vp, "#B1");

			vp = c.GetVirtualPath (new RequestContext (hc, rd), new RouteValueDictionary {
				{"Table", "FooTable"},
				{"Action", null}
			});

			Assert.IsNull (vp, "#C1");
		}

		[Test]
		public void GetVirtualPath8 ()
		{
			var routes = new RouteCollection();

			routes.Add (new MyRoute ("login", new MyRouteHandler ()) {
				Defaults = new RouteValueDictionary (new { controller = "Home", action = "LogOn" })
			});

			routes.Add (new MyRoute ("{site}/{controller}/{action}", new MyRouteHandler ())	{
				Defaults = new RouteValueDictionary (new { site = "_", controller = "Home", action = "Index" }),
				Constraints = new RouteValueDictionary ( new { site = "_?[0-9A-Za-z-]*" })
			});

			routes.Add (new MyRoute ("{*path}", new MyRouteHandler ()) {
				Defaults = new RouteValueDictionary (new { controller = "Error", action = "NotFound" }),
			});

			var hc = new HttpContextStub2 ("~/login", String.Empty, String.Empty);
			hc.SetResponse (new HttpResponseStub (3));
			var rd = routes.GetRouteData (hc);
			var rvs = new RouteValueDictionary () {
				{ "controller", "Home" },
				{ "action" , "Index" }
			};
			var vpd = routes.GetVirtualPath (new RequestContext (hc, rd), rvs);
			Assert.IsNotNull (vpd, "#A1");
			Assert.AreEqual ("/", vpd.VirtualPath, "#A2");
			Assert.AreEqual (0, vpd.DataTokens.Count, "#A3");

			hc = new HttpContextStub2 ("~/login", String.Empty, String.Empty);
			hc.SetResponse (new HttpResponseStub (3));
			rd = routes.GetRouteData (hc);
			rvs = new RouteValueDictionary () {
				{ "controller", "Home" }
			};
			vpd = routes.GetVirtualPath (new RequestContext (hc, rd), rvs);
			Assert.IsNotNull (vpd, "#B1");
			Assert.AreEqual ("/login", vpd.VirtualPath, "#B2");
			Assert.AreEqual (0, vpd.DataTokens.Count, "#B3");

			hc = new HttpContextStub2 ("~/login", String.Empty, String.Empty);
			hc.SetResponse (new HttpResponseStub (3));
			rd = routes.GetRouteData (hc);
			rvs = new RouteValueDictionary () {
				{ "action" , "Index" }
			};
			vpd = routes.GetVirtualPath (new RequestContext (hc, rd), rvs);
			Assert.IsNotNull (vpd, "#C1");
			Assert.AreEqual ("/", vpd.VirtualPath, "#C2");
			Assert.AreEqual (0, vpd.DataTokens.Count, "#C3");

			hc = new HttpContextStub2 ("~/", String.Empty, String.Empty);
			rd = routes.GetRouteData (hc);
			Assert.IsNotNull (rd, "#D1");
		}

		[Test]
		[Ignore ("looks like RouteExistingFiles ( = false) does not affect... so this test needs more investigation")]
		public void GetVirtualPathToExistingFile ()
		{
			var c = new RouteCollection ();
			c.Add (new MyRoute ("{foo}/{bar}", new MyRouteHandler ()));
			var hc = new HttpContextStub2 ("~/Test/test.html", String.Empty, ".");
			// it tries to get HttpContextBase.Response, so set it.
			hc.SetResponse (new HttpResponseStub (3));
			var rd = c.GetRouteData (hc);
			var vpd = c.GetVirtualPath (new RequestContext (hc, rd), rd.Values);
			Assert.AreEqual ("./Test/test.html", vpd.VirtualPath, "#1");
			Assert.AreEqual (0, vpd.DataTokens.Count, "#2");
		}

		[Test (Description="Routes from NerdDinner")]
		public void GetRouteDataNerdDinner ()
		{
			var c = new RouteCollection ();

			c.Add ("UpcomingDiners",
			       new MyRoute ("Dinners/Page/{page}", new MyRouteHandler ()) { Defaults = new RouteValueDictionary (new { controller = "Dinners", action = "Index" }) }
			);

			c.Add ("Default",
			       new MyRoute ("{controller}/{action}/{id}", new MyRouteHandler ()) { Defaults = new RouteValueDictionary (new { controller = "Home", action = "Index", id = "" })}
			);

			var hc = new HttpContextStub2 ("~/", String.Empty, String.Empty);
			hc.SetResponse (new HttpResponseStub (3));
			var rd = c.GetRouteData (hc);
			
			Assert.IsNotNull (rd, "#A1");
		}

		[Test (Description="Routes from NerdDinner")]
		public void GetRouteDataNerdDinner2 ()
		{
			var c = new RouteCollection ();

			c.Add ("UpcomingDiners",
			       new MyRoute ("Dinners/Page/{page}", new MyRouteHandler ()) { Defaults = new RouteValueDictionary (new { controller = "Dinners", action = "Index" }) }
			);

			c.Add ("Default",
			       new MyRoute ("{controller}/{action}/{id}", new MyRouteHandler ()) { Defaults = new RouteValueDictionary (new { controller = "Home", action = "Index", id = "" })}
			);

			var hc = new HttpContextStub2 ("~/Home/Index", String.Empty, String.Empty);
			hc.SetResponse (new HttpResponseStub (3));
			var rd = c.GetRouteData (hc);
			
			Assert.IsNotNull (rd, "#A1");
		}
		[Test]
		public void Ignore_String ()
		{
			var c = new RouteCollection ();

			Assert.Throws<ArgumentNullException> (() => {
				c.Ignore (null);
			}, "#A1");

			c.Ignore ("{resource}.axd/{*pathInfo}");
			var hc = new HttpContextStub2 ("~/something.axd/pathinfo", String.Empty, String.Empty);
			hc.SetResponse (new HttpResponseStub (3));
			var rd = c.GetRouteData (hc);

			Assert.IsNotNull (rd, "#A1-1");
			Assert.IsNotNull (rd.RouteHandler, "#A1-2");
			Assert.AreEqual (typeof (StopRoutingHandler), rd.RouteHandler.GetType (), "#A1-3");
			Assert.IsTrue (rd.Route is Route, "#A1-4");
			Assert.IsNotNull (((Route) rd.Route).Constraints, "#A1-5");
			Assert.AreEqual (0, ((Route) rd.Route).Constraints.Count, "#A1-6");
		}

		[Test]
		public void Ignore_String_Object ()
		{
			var c = new RouteCollection ();

			Assert.Throws<ArgumentNullException> (() => {
				c.Ignore (null, new { allaspx = @".*\.aspx(/.*)?" });
			}, "#A1");

			c.Ignore ("{*allaspx}", new { allaspx = @".*\.aspx(/.*)?" });
			var hc = new HttpContextStub2 ("~/page.aspx", String.Empty, String.Empty);
			hc.SetResponse (new HttpResponseStub (3));
			var rd = c.GetRouteData (hc);

			Assert.IsNotNull (rd, "#A1-1");
			Assert.IsNotNull (rd.RouteHandler, "#A1-2");
			Assert.AreEqual (typeof (StopRoutingHandler), rd.RouteHandler.GetType (), "#A1-3");
			Assert.IsTrue (rd.Route is Route, "#A1-4");
			Assert.IsNotNull (((Route) rd.Route).Constraints, "#A1-5");
			Assert.AreEqual (1, ((Route) rd.Route).Constraints.Count, "#A1-6");
			Assert.AreEqual (@".*\.aspx(/.*)?", ((Route) rd.Route).Constraints ["allaspx"], "#A1-7");

			c = new RouteCollection ();
			c.Ignore ("{*allaspx}", "something invalid");

			Assert.Throws<InvalidOperationException> (() => {
				rd = c.GetRouteData (hc);
			}, "#A2");
		}

		[Test]
		public void MapPageRoute_String_String_String ()
		{
			var c = new RouteCollection ();

			c.MapPageRoute (null, "{foo}-{bar}", "~/some-url");
			var hc = new HttpContextStub2 ("~/some-url", String.Empty, String.Empty);
			hc.SetResponse (new HttpResponseStub (3));
			var rd = c.GetRouteData (hc);

			Assert.IsNotNull (rd, "#A1-1");
			Assert.IsNotNull (rd.RouteHandler, "#A1-2");
			Assert.AreEqual (typeof (PageRouteHandler), rd.RouteHandler.GetType (), "#A1-3");

			c = new RouteCollection ();
			Assert.Throws<ArgumentNullException> (() => {
				c.MapPageRoute ("RouteName", null, "~/some-url");
			}, "#A2");

			c = new RouteCollection ();
			c.MapPageRoute ("RouteName", String.Empty, "~/some-url");
			rd = c.GetRouteData (hc);

			Assert.IsNull (rd, "#A2");

			c = new RouteCollection ();
			// thrown by PageRouteHandler's constructor
			Assert.Throws<ArgumentException> (() => {
				c.MapPageRoute ("RouteName", "~/some-url", null);
			}, "#A3");
		}

		[Test]
		public void MapPageRoute_String_String_String_Bool ()
		{
			var c = new RouteCollection ();

			c.MapPageRoute (null, "{foo}-{bar}", "~/some-url", true);
			var hc = new HttpContextStub2 ("~/some-url", String.Empty, String.Empty);
			hc.SetResponse (new HttpResponseStub (3));
			var rd = c.GetRouteData (hc);

			Assert.IsNotNull (rd, "#A1-1");
			Assert.IsNotNull (rd.RouteHandler, "#A1-2");
			Assert.AreEqual (typeof (PageRouteHandler), rd.RouteHandler.GetType (), "#A1-3");
			Assert.IsTrue (((PageRouteHandler) rd.RouteHandler).CheckPhysicalUrlAccess, "#A1-4");

			c = new RouteCollection ();
			Assert.Throws<ArgumentNullException> (() => {
				c.MapPageRoute ("RouteName", null, "~/some-url", true);
			}, "#A2");

			c = new RouteCollection ();
			c.MapPageRoute ("RouteName", String.Empty, "~/some-url", true);
			rd = c.GetRouteData (hc);

			Assert.IsNull (rd, "#A2");

			c = new RouteCollection ();
			// thrown by PageRouteHandler's constructor
			Assert.Throws<ArgumentException> (() => {
				c.MapPageRoute ("RouteName", "~/some-url", null, true);
			}, "#A3");

			c.MapPageRoute (null, "{foo}-{bar}", "~/some-url", false);
			rd = c.GetRouteData (hc);

			Assert.IsNotNull (rd, "#A4-1");
			Assert.IsNotNull (rd.RouteHandler, "#A4-2");
			Assert.AreEqual (typeof (PageRouteHandler), rd.RouteHandler.GetType (), "#A4-3");
			Assert.IsFalse (((PageRouteHandler) rd.RouteHandler).CheckPhysicalUrlAccess, "#A4-4");
		}

		[Test]
		public void MapPageRoute_String_String_String_Bool_RVD ()
		{
			var c = new RouteCollection ();
			var defaults = new RouteValueDictionary ();

			c.MapPageRoute (null, "{foo}-{bar}", "~/some-url", true, defaults);
			var hc = new HttpContextStub2 ("~/some-url", String.Empty, String.Empty);
			hc.SetResponse (new HttpResponseStub (3));
			var rd = c.GetRouteData (hc);

			Assert.IsNotNull (rd, "#A1-1");
			Assert.IsNotNull (rd.RouteHandler, "#A1-2");
			Assert.AreEqual (typeof (PageRouteHandler), rd.RouteHandler.GetType (), "#A1-3");
			Assert.IsTrue (((PageRouteHandler) rd.RouteHandler).CheckPhysicalUrlAccess, "#A1-4");

			c = new RouteCollection ();
			Assert.Throws<ArgumentNullException> (() => {
				c.MapPageRoute ("RouteName", null, "~/some-url", true, defaults);
			}, "#A2");

			c = new RouteCollection ();
			c.MapPageRoute ("RouteName", String.Empty, "~/some-url", true, defaults);
			rd = c.GetRouteData (hc);

			Assert.IsNull (rd, "#A2");

			c = new RouteCollection ();
			// thrown by PageRouteHandler's constructor
			Assert.Throws<ArgumentException> (() => {
				c.MapPageRoute ("RouteName", "~/some-url", null, true, defaults);
			}, "#A3");

			c.MapPageRoute (null, "{foo}-{bar}", "~/some-url", false, defaults);
			rd = c.GetRouteData (hc);

			Assert.IsNotNull (rd, "#A4-1");
			Assert.IsNotNull (rd.RouteHandler, "#A4-2");
			Assert.AreEqual (typeof (PageRouteHandler), rd.RouteHandler.GetType (), "#A4-3");
			Assert.IsFalse (((PageRouteHandler) rd.RouteHandler).CheckPhysicalUrlAccess, "#A4-4");

			c.MapPageRoute (null, "{foo}-{bar}", "~/some-url", false, null);
			rd = c.GetRouteData (hc);

			Assert.IsNotNull (rd, "#A4-1");
			Assert.IsNotNull (rd.RouteHandler, "#A4-2");
			Assert.AreEqual (typeof (PageRouteHandler), rd.RouteHandler.GetType (), "#A4-3");
			Assert.IsFalse (((PageRouteHandler) rd.RouteHandler).CheckPhysicalUrlAccess, "#A4-4"); 
		}

		[Test]
		public void MapPageRoute_String_String_String_Bool_RVD_RVD ()
		{
			var c = new RouteCollection ();
			var defaults = new RouteValueDictionary ();
			var constraints = new RouteValueDictionary ();

			c.MapPageRoute (null, "{foo}-{bar}", "~/some-url", true, defaults, constraints);
			var hc = new HttpContextStub2 ("~/some-url", String.Empty, String.Empty);
			hc.SetResponse (new HttpResponseStub (3));
			var rd = c.GetRouteData (hc);

			Assert.IsNotNull (rd, "#A1-1");
			Assert.IsNotNull (rd.RouteHandler, "#A1-2");
			Assert.AreEqual (typeof (PageRouteHandler), rd.RouteHandler.GetType (), "#A1-3");
			Assert.IsTrue (((PageRouteHandler) rd.RouteHandler).CheckPhysicalUrlAccess, "#A1-4");

			c = new RouteCollection ();
			Assert.Throws<ArgumentNullException> (() => {
				c.MapPageRoute ("RouteName", null, "~/some-url", true, defaults, constraints);
			}, "#A2");

			c = new RouteCollection ();
			c.MapPageRoute ("RouteName", String.Empty, "~/some-url", true, defaults, constraints);
			rd = c.GetRouteData (hc);

			Assert.IsNull (rd, "#A2");

			c = new RouteCollection ();
			// thrown by PageRouteHandler's constructor
			Assert.Throws<ArgumentException> (() => {
				c.MapPageRoute ("RouteName", "~/some-url", null, true, defaults, constraints);
			}, "#A3");

			c.MapPageRoute (null, "{foo}-{bar}", "~/some-url", false, defaults, constraints);
			rd = c.GetRouteData (hc);

			Assert.IsNotNull (rd, "#A4-1");
			Assert.IsNotNull (rd.RouteHandler, "#A4-2");
			Assert.AreEqual (typeof (PageRouteHandler), rd.RouteHandler.GetType (), "#A4-3");
			Assert.IsFalse (((PageRouteHandler) rd.RouteHandler).CheckPhysicalUrlAccess, "#A4-4");

			c.MapPageRoute (null, "{foo}-{bar}", "~/some-url", false, null, constraints);
			rd = c.GetRouteData (hc);

			Assert.IsNotNull (rd, "#A4-1");
			Assert.IsNotNull (rd.RouteHandler, "#A4-2");
			Assert.AreEqual (typeof (PageRouteHandler), rd.RouteHandler.GetType (), "#A4-3");
			Assert.IsFalse (((PageRouteHandler) rd.RouteHandler).CheckPhysicalUrlAccess, "#A4-4");
		}

		[Test]
		public void MapPageRoute_String_String_String_Bool_RVD_RVD_RVD ()
		{
			var c = new RouteCollection ();
			var defaults = new RouteValueDictionary ();
			var constraints = new RouteValueDictionary ();
			var dataTokens = new RouteValueDictionary ();

			c.MapPageRoute (null, "{foo}-{bar}", "~/some-url", true, defaults, constraints, dataTokens);
			var hc = new HttpContextStub2 ("~/some-url", String.Empty, String.Empty);
			hc.SetResponse (new HttpResponseStub (3));
			var rd = c.GetRouteData (hc);

			Assert.IsNotNull (rd, "#A1-1");
			Assert.IsNotNull (rd.RouteHandler, "#A1-2");
			Assert.AreEqual (typeof (PageRouteHandler), rd.RouteHandler.GetType (), "#A1-3");
			Assert.IsTrue (((PageRouteHandler) rd.RouteHandler).CheckPhysicalUrlAccess, "#A1-4");

			c = new RouteCollection ();
			Assert.Throws<ArgumentNullException> (() => {
				c.MapPageRoute ("RouteName", null, "~/some-url", true, defaults, constraints, dataTokens);
			}, "#A2");

			c = new RouteCollection ();
			c.MapPageRoute ("RouteName", String.Empty, "~/some-url", true, defaults, constraints, dataTokens);
			rd = c.GetRouteData (hc);

			Assert.IsNull (rd, "#A2");

			c = new RouteCollection ();
			// thrown by PageRouteHandler's constructor
			Assert.Throws<ArgumentException> (() => {
				c.MapPageRoute ("RouteName", "~/some-url", null, true, defaults, constraints, dataTokens);
			}, "#A3");

			c.MapPageRoute (null, "{foo}-{bar}", "~/some-url", false, defaults, constraints, dataTokens);
			rd = c.GetRouteData (hc);

			Assert.IsNotNull (rd, "#A4-1");
			Assert.IsNotNull (rd.RouteHandler, "#A4-2");
			Assert.AreEqual (typeof (PageRouteHandler), rd.RouteHandler.GetType (), "#A4-3");
			Assert.IsFalse (((PageRouteHandler) rd.RouteHandler).CheckPhysicalUrlAccess, "#A4-4");

			c.MapPageRoute (null, "{foo}-{bar}", "~/some-url", false, null, constraints, dataTokens);
			rd = c.GetRouteData (hc);

			Assert.IsNotNull (rd, "#A4-1");
			Assert.IsNotNull (rd.RouteHandler, "#A4-2");
			Assert.AreEqual (typeof (PageRouteHandler), rd.RouteHandler.GetType (), "#A4-3");
			Assert.IsFalse (((PageRouteHandler) rd.RouteHandler).CheckPhysicalUrlAccess, "#A4-4");
		}
		
		[Test] // https://bugzilla.xamarin.com/show_bug.cgi?id=13909
		public void MapPageRoute_Bug13909 ()
		{
			var c = new RouteCollection ();

			c.MapPageRoute("test", "test", "~/test.aspx");
			c.Clear();
			c.MapPageRoute("test", "test", "~/test.aspx");
		}
	}
}
