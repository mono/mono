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
		public void RoutingHandler ()
		{
			var r = new Route (null, new StopRoutingHandler ());
			Assert.AreEqual (typeof (StopRoutingHandler), r.RouteHandler.GetType (), "#1");
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
	}
}
