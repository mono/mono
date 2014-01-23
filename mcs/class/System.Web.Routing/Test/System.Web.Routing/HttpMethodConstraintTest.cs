//
// HttpMethodConstraintTest.cs
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
	class MyHttpMethodConstraint : HttpMethodConstraint
	{
		public MyHttpMethodConstraint (params string [] args)
			: base (args)
		{
		}

		public bool CallMatch (HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
		{
			return Match (httpContext, route, parameterName, values, routeDirection);
		}
	}

	[TestFixture]
	public class HttpMethodConstraintTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullArgs ()
		{
			new HttpMethodConstraint (null);
		}

		[Test]
		public void ConstructorEmptyArray ()
		{
			new HttpMethodConstraint (new string [0]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void MatchNullContext ()
		{
			var c = new MyHttpMethodConstraint (new string [0]);
			c.CallMatch (null, new Route (null, null), "foo", new RouteValueDictionary (), RouteDirection.IncomingRequest);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void MatchNullRoute ()
		{
			var c = new MyHttpMethodConstraint (new string [0]);
			c.CallMatch (new HttpContextStub (), null, "foo", new RouteValueDictionary (), RouteDirection.IncomingRequest);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void MatchNullName ()
		{
			var c = new MyHttpMethodConstraint (new string [0]);
			c.CallMatch (new HttpContextStub (), new Route (null, null), null, new RouteValueDictionary (), RouteDirection.IncomingRequest);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void MatchNullValues ()
		{
			var c = new MyHttpMethodConstraint (new string [0]);
			c.CallMatch (new HttpContextStub (), new Route (null, null), "foo", null, RouteDirection.IncomingRequest);
		}

		[Test]
		public void Match ()
		{
			var c = new MyHttpMethodConstraint (new string [0]);
			Assert.IsFalse (c.CallMatch (new HttpContextStub (""), new Route (null, null), "foo", new RouteValueDictionary (), RouteDirection.IncomingRequest));
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void MatchDependsOnRequest ()
		{
			// it tries to get HttpContext.Request only when HttpMethodConstraint actually has one or more values.
			var c = new MyHttpMethodConstraint (new string [] {"GET"});
			c.CallMatch (new HttpContextStub (), new Route (null, null), "foo", new RouteValueDictionary (), RouteDirection.IncomingRequest);
		}

		[Test]
		public void Match2 ()
		{
			var c = new MyHttpMethodConstraint (new string [] {"GET"});
			Assert.IsFalse (c.CallMatch (new HttpContextStub (""), new Route (null, null), "foo", new RouteValueDictionary (), RouteDirection.IncomingRequest), "#1");
			Assert.IsTrue (c.CallMatch (new HttpContextStub ("", "", "GET"), new Route (null, null), "", new RouteValueDictionary (), RouteDirection.IncomingRequest), "#2");
			Assert.IsFalse (c.CallMatch (new HttpContextStub ("", "", "POST"), new Route (null, null), "", new RouteValueDictionary (), RouteDirection.IncomingRequest), "#3");
			// LAMESPEC: .NET allows case-insensitive comparison, which violates RFC 2616
			// Assert.IsFalse (c.CallMatch (new HttpContextStub ("", "", "get"), new Route (null, null), "", new RouteValueDictionary (), RouteDirection.IncomingRequest), "#4");
		}

		[Test]
		public void UrlGeneration ()
		{
			var c = new HttpMethodConstraint (new string[] { "GET" }) as IRouteConstraint;
			var req = new HttpContextStub ("", "", "HEAD");

			var values = new RouteValueDictionary () { { "httpMethod", "GET" } };
			Assert.IsTrue (c.Match (req, new Route (null, null), "httpMethod", values, RouteDirection.UrlGeneration), "#1");

			values = new RouteValueDictionary() { { "httpMethod", "POST" } };
			Assert.IsFalse (c.Match (req, new Route (null, null), "httpMethod", values, RouteDirection.UrlGeneration), "#2");
		}
	}
}
