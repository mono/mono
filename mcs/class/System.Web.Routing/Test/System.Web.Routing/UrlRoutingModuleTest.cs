//
// UrlRoutingModuleTest.cs
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
	public class UrlRoutingModuleTest
	{
		[SetUp]
		public void SetUp ()
		{
			RouteTable.Routes.Clear ();
		}

		[Test]
		public void SetRouteCollectionNull ()
		{
			var m = new UrlRoutingModule ();
			RouteTable.Routes.Add (new Route (null, null));
			Assert.IsNotNull (m.RouteCollection, "#1");
			Assert.AreEqual (RouteTable.Routes, m.RouteCollection, "#1-2");
			m.RouteCollection = null;
			Assert.IsNotNull (m.RouteCollection, "#2");
			Assert.AreEqual (RouteTable.Routes, m.RouteCollection, "#2-2");
		}

		// PostMapRequestHandler

		[Test]
		public void PostMapRequestHandlerNoMatch ()
		{
			var m = new UrlRoutingModule ();
			RouteTable.Routes.Add (new MyRoute (null, null));
			m.PostMapRequestHandler (new HttpContextStub2 ());
		}

		[Test]
		public void PostMapRequestHandlerNoPath ()
		{
			var m = new UrlRoutingModule ();
			RouteTable.Routes.Add (new MyRoute ("foo/bar", new MyRouteHandler ()));
			// ... huh? no NIE? what does it do then?
			m.PostMapRequestHandler (new HttpContextStub2 ("~/foo/bar", null));
		}

		// PostResolveRequestCache

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void PostResolveRequestCacheNullRouteHandler ()
		{
			var m = new UrlRoutingModule ();
			RouteTable.Routes.Add (new MyRoute ("foo/bar", null));
			m.PostResolveRequestCache (new HttpContextStub2 ("~/foo/bar", null));
		}

#if !NET_4_0
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void PostResolveRequestCacheNullHttpHandler ()
		{
			var m = new UrlRoutingModule ();
			RouteTable.Routes.Add (new MyRoute ("foo/bar", new NullRouteHandler ()));

			m.PostResolveRequestCache (new HttpContextStub2 ("~/foo/bar", null));
		}
#endif
		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void PostResolveRequestCacheNoPath ()
		{
			var m = new UrlRoutingModule ();
			RouteTable.Routes.Add (new MyRoute ("foo/bar", new MyRouteHandler ()));
			// it tries to get HttpContextBase.Request.Path and causes NIE.
			m.PostResolveRequestCache (new HttpContextStub2 ("~/foo/bar", null));
		}

#if !NET_4_0
		[Test]
		public void PostResolveRequestCacheCallRewritePath ()
		{
			var m = new UrlRoutingModule ();
			RouteTable.Routes.Add (new MyRoute ("{foo}/{bar}", new MyRouteHandler ()));
			var hc = new HttpContextStub2 ("~/x/y", "z");
			try {
				m.PostResolveRequestCache (hc);
				Assert.Fail ("#1");
			} catch (ApplicationException ex) {
				Assert.AreEqual ("~/UrlRouting.axd", ex.Message, "#2");
			}
		}

		[Test]
		public void PostResolveRequestCacheModifiedPath ()
		{
			var m = new UrlRoutingModule ();
			RouteTable.Routes.Add (new MyRoute ("{foo}/{bar}", new MyRouteHandler ()));
			var hc = new HttpContextStub2 ("~/x/y", "z", "apppath");
			hc.SetResponse (new HttpResponseStub (2));
			try {
				m.PostResolveRequestCache (hc);
				Assert.Fail ("#1");
			} catch (ApplicationException ex) {
				Assert.AreEqual ("~/UrlRouting.axd", ex.Message, "#2");
			}
		}
#endif
		[Test]
		public void PostResolveRequestCache ()
		{
			var m = new UrlRoutingModule ();
			RouteTable.Routes.Add (new MyRoute ("{foo}/{bar}", new MyRouteHandler ()));
#if NET_4_0
			var hc = new HttpContextStub4 ("~/x/y", "z", "apppath", false);
#else
			var hc = new HttpContextStub3 ("~/x/y", "z", "apppath", false);
#endif
			hc.SetResponse (new HttpResponseStub (2));
			m.PostResolveRequestCache (hc);
#if NET_4_0
			Assert.AreEqual (null, hc.RewrittenPath, "#1");
#else
			Assert.AreEqual ("~/UrlRouting.axd", hc.RewrittenPath, "#1");
#endif
			// it internally stores the handler 
		}
		
		[Test]
		public void PostResolveRequestCacheStopRoutingHttpHandler ()
		{
			var m = new UrlRoutingModule ();
			RouteTable.Routes.Add (new MyRoute ("foo/bar", new StopRoutingHandler ()));
			var hc = new HttpContextStub3 ("~/foo/bar", String.Empty, "apppath", false);
			m.PostResolveRequestCache (hc);
			Assert.IsNull (hc.RewrittenPath, "StopRoutingHandler should stop before the path is rewritten");
		}


		[Test]
		[Ignore ("looks like RouteExistingFiles ( = false) does not affect... so this test needs more investigation")]
		public void PostResolveRequestCachePathToExistingFile ()
		{
			var m = new UrlRoutingModule ();
			RouteTable.Routes.Add (new MyRoute ("~/{foo}/{bar}", new MyRouteHandler ()));
			var hc = new HttpContextStub2 ("~/hoge/fuga", String.Empty, ".");
			// it tries to get HttpContextBase.Response, so set it.
			hc.SetResponse (new HttpResponseStub (3));
			try {
				m.PostResolveRequestCache (hc);
				Assert.Fail ("#1");
			} catch (ApplicationException ex) {
				Assert.AreEqual ("~/UrlRouting.axd", ex.Message, "#2");
			}
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void Pipeline1 ()
		{
			var m = new UrlRoutingModule ();
			RouteTable.Routes.Add (new MyRoute ("{foo}/{bar}", new MyRouteHandler ()));
			var hc = new HttpContextStub3 ("~/x/y", "z", "apppath", false);
			hc.SetResponse (new HttpResponseStub (2));
			m.PostResolveRequestCache (hc);
			Assert.AreEqual ("~/UrlRouting.axd", hc.RewrittenPath, "#1");
			// It tries to set Handler and causes NIE
			m.PostMapRequestHandler (hc);
		}

		[Test]
		public void Pipeline2 ()
		{
			var m = new UrlRoutingModule ();
			RouteTable.Routes.Add (new MyRoute ("{foo}/{bar}", new MyRouteHandler ()));
#if NET_4_0
			var hc = new HttpContextStub4 ("~/x/y", "z", "apppath", true);
#else
			var hc = new HttpContextStub3 ("~/x/y", "z", "apppath", true);
#endif
			hc.HttpHandler = new MyHttpHandler ();
			hc.SetResponse (new HttpResponseStub (2));
			m.PostResolveRequestCache (hc);
#if NET_4_0
			Assert.AreEqual (null, hc.RewrittenPath, "#1");
#else
			Assert.AreEqual ("~/UrlRouting.axd", hc.RewrittenPath, "#1");
#endif
			// It tries to set Handler and causes NIE
			m.PostMapRequestHandler (hc);
		}

		[Test]
#if !NET_4_0
		[ExpectedException (typeof (ApplicationException))]
#endif
		public void Pipeline3 ()
		{
			var m = new UrlRoutingModule ();
			RouteTable.Routes.Add (new MyRoute ("{foo}/{bar}", new MyRouteHandler ()));
#if NET_4_0
			var hc = new HttpContextStub5 ("~/x/y", String.Empty, "apppath");
#else
			var hc = new HttpContextStub2 ("~/x/y", String.Empty, "apppath");
#endif
			hc.SetResponse (new HttpResponseStub (2));
#if NET_4_0
			Assert.IsNull (m.RouteCollection.GetRouteData (hc), "#0");
#else
			Assert.IsNotNull (m.RouteCollection.GetRouteData (hc), "#0");
			m.PostResolveRequestCache (hc);
			try {
				m.PostMapRequestHandler (hc);
				Assert.Fail ("#1");
			} catch (ApplicationException ex) {
				Assert.AreEqual ("~/UrlRouting.axd", ex.Message, "#2");
			}
#endif
		}
	}
}
