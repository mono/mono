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
			Assert.IsNull (rd);
			try {
				new RouteCollection ().GetRouteData (new HttpContextStub2 (null, null, null));
				Assert.Fail ("#1");
			} catch (NotImplementedException) {
				// it should fail due to the NIE on AppRelativeCurrentExecutionFilePath.
			}
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
			var vpd = c.GetVirtualPath (new RequestContext (hc, rd), rd.Values);
			Assert.IsNotNull (vpd, "#1");
			Assert.AreEqual ("apppath/x/y_modified", vpd.VirtualPath, "#2");
			Assert.AreEqual (0, vpd.DataTokens.Count, "#3");
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
	}
}
