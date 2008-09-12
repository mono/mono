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
	}
}
