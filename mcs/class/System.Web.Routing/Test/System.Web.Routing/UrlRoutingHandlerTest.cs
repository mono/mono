//
// UrlRoutingHandlerTest.cs
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
	public class UrlRoutingHandlerTest
	{
		[SetUp]
		public void SetUp ()
		{
			RouteTable.Routes.Clear ();
		}

		[Test]
		public void SetRouteCollectionNull ()
		{
			var h = new MyUrlRoutingHandler ();
			RouteTable.Routes.Add (new Route (null, null));
			Assert.IsNotNull (h.RouteCollection, "#1");
			Assert.AreEqual (RouteTable.Routes, h.RouteCollection, "#1-2");
			h.RouteCollection = null;
			Assert.IsNotNull (h.RouteCollection, "#2");
			Assert.AreEqual (RouteTable.Routes, h.RouteCollection, "#2-2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ProcessRequestNullArg ()
		{
			new MyUrlRoutingHandler ().DoProcessRequest (null);
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void ProcessRequestForStubContext ()
		{
			var h = new MyUrlRoutingHandler ();
			// it results in call to HttpContextBase.get_Request() and thus NIE.
			h.DoProcessRequest (new HttpContextStub ());
		}

		[Test]
		[ExpectedException (typeof (HttpException))]
		public void ProcessRequestNoMatch ()
		{
			var h = new MyUrlRoutingHandler ();
			Assert.AreEqual (0, h.RouteCollection.Count, "#1");
			h.DoProcessRequest (new HttpContextStub ("~/foo"));
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void ProcessRequestPathInfoNIE ()
		{
			var h = new MyUrlRoutingHandler ();
			var r = new Route ("foo", null);
			h.RouteCollection.Add (r);
			// it internally calls RouteCollection.GetRouteData() 
			// and borks at HttpContextStub.Request.PathInfo NIE.
			h.DoProcessRequest (new HttpContextStub ("~/foo"));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ProcessRequestNoHandler ()
		{
			var h = new MyUrlRoutingHandler ();
			var r = new Route ("foo", null);
			h.RouteCollection.Add (r);
			h.DoProcessRequest (new HttpContextStub ("~/foo", String.Empty));
		}

		[Test]
		[ExpectedException (typeof (HttpException))]
		public void ProcessRequestNoMatch2 ()
		{
			var h = new MyUrlRoutingHandler ();
			var r = new Route ("foo", null);
			h.RouteCollection.Add (r);
			h.DoProcessRequest (new HttpContextStub ("~/bar", String.Empty));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ProcessRequestMatchNoHandler ()
		{
			var h = new MyUrlRoutingHandler ();
			var r = new Route ("foo", null);
			h.RouteCollection.Add (r);
			h.DoProcessRequest (new HttpContextStub ("~/foo", String.Empty));
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ProcessRequestMatchStop ()
		{
			var h = new MyUrlRoutingHandler ();
			var r = new Route ("foo", new StopRoutingHandler ());
			h.RouteCollection.Add (r);
			h.DoProcessRequest (new HttpContextStub ("~/foo", String.Empty));
			// thrown by StopRoutingHandler.GetHttpHandler().
		}

		[Test]
		public void ProcessRequestMatchError ()
		{
			var h = new MyUrlRoutingHandler ();
			var r = new Route ("foo", new ErrorRouteHandler ());
			h.RouteCollection.Add (r);
			try {
				h.DoProcessRequest (new HttpContextStub ("~/foo", String.Empty));
				Assert.Fail ("#1");
			} catch (ApplicationException ex) {
				Assert.AreEqual ("ErrorRouteHandler", ex.Message, "#2");
			}
		}
	}
}
