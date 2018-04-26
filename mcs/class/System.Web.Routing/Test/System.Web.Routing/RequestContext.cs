//
// RequestContext.cs
//
// Authors:
//	Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2010 Novell Inc. http://novell.com
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
using System.Text;
using System.Web;
using System.Web.Routing;

using NUnit.Framework;

namespace MonoTests.System.Web.Routing
{
	[TestFixture]
	public class RequestContextTest
	{
		[Test]
		public void DefaultConstructor ()
		{
			var rc = new RequestContext ();

			Assert.AreEqual (null, rc.HttpContext, "#A1");
			Assert.AreEqual (null, rc.RouteData, "#A2");
		}
		[Test]
		public void Constructor_HttpContextBase_RouteData ()
		{
			RequestContext rc;

			Assert.Throws<ArgumentNullException> (() => {
				rc = new RequestContext (null, new RouteData ());
			}, "#A1");

			var ctx = new HttpContextWrapper (new HttpContext (new HttpRequest ("filename", "http://localhost/filename", String.Empty), new HttpResponse (new StringWriter ())));
			Assert.Throws<ArgumentNullException> (() => {
				rc = new RequestContext (ctx, null);
			}, "#A2");
		}

		[Test]
		public void HttpContext ()
		{
			var ctx = new HttpContextWrapper (new HttpContext (new HttpRequest ("filename", "http://localhost/filename", String.Empty), new HttpResponse (new StringWriter ())));
			var rc = new RequestContext (ctx, new RouteData ());

			Assert.AreSame (ctx, rc.HttpContext, "#A1");
			ctx = new HttpContextWrapper (new HttpContext (new HttpRequest ("filename", "http://localhost/filename", String.Empty), new HttpResponse (new StringWriter ())));
			rc.HttpContext = ctx;
			Assert.AreSame (ctx, rc.HttpContext, "#A2");

			rc.HttpContext = null;
			Assert.IsNull (rc.HttpContext, "#A3");
		}

		[Test]
		public void RouteData ()
		{
			var ctx = new HttpContextWrapper (new HttpContext (new HttpRequest ("filename", "http://localhost/filename", String.Empty), new HttpResponse (new StringWriter ())));
			var rd = new RouteData ();
			var rc = new RequestContext (ctx, rd);

			Assert.AreSame (rd, rc.RouteData, "#A1");
			rd = new RouteData ();
			rc.RouteData = rd;
			Assert.AreSame (rd, rc.RouteData, "#A2");

			rc.RouteData = null;
			Assert.IsNull (rc.RouteData, "#A3");
		}
	}
}
