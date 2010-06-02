//
// Authors:
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2010 Novell, Inc. (http://novell.com/)
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
#if NET_4_0
using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using NUnit.Framework;
using MonoTests.Common;
using MonoTests.System.Web;

namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]
	public class RouteParameterTest
	{
		[Test]
		public void Constructor ()
		{
			var rp = new RouteParameter ();

			Assert.AreEqual (String.Empty, rp.RouteKey, "#A1");
			Assert.AreEqual (String.Empty, rp.Name, "#A2");
			Assert.AreEqual (TypeCode.Empty, rp.Type, "#A3");
			Assert.AreEqual (ParameterDirection.Input, rp.Direction, "#A4");
			Assert.IsNull (rp.DefaultValue, "#A5");
			Assert.AreEqual (DbType.Object, rp.DbType, "#A6");
			Assert.AreEqual (true, rp.ConvertEmptyStringToNull, "#A7");
			Assert.AreEqual (0, rp.Size, "#A8");
		}

		[Test]
		public void Constructor_RouteParameter ()
		{
			RouteParameter rp;
			RouteParameter original;

			AssertExtensions.Throws<NullReferenceException> (() => {
				rp = new FakeRouteParameter ((RouteParameter) null);
			}, "#A1");

			original = new RouteParameter ("Name", "Key");
			rp = new FakeRouteParameter (original);

			Assert.AreEqual (original.Name, rp.Name, "#B1-2");
			Assert.AreEqual (original.RouteKey, rp.RouteKey, "#B1-3");
		}

		[Test]
		public void Constructor_String_String ()
		{
			RouteParameter rp;

			rp = new RouteParameter (null, "key");
			Assert.AreEqual (String.Empty, rp.Name, "#A1-1");
			Assert.AreEqual ("key", rp.RouteKey, "#A1-2");

			rp = new RouteParameter ("name", null);
			Assert.AreEqual ("name", rp.Name, "#A2-1");
			Assert.AreEqual (String.Empty, rp.RouteKey, "#A2-2");
		}

		[Test]
		public void Constructor_String_DbType_String ()
		{
			RouteParameter rp;

			rp = new RouteParameter ("name", DbType.Int64, "key");
			Assert.AreEqual ("name", rp.Name, "#A1-1");
			Assert.AreEqual ("key", rp.RouteKey, "#A1-2");
			Assert.AreEqual (DbType.Int64, rp.DbType, "#A1-3");

			Assert.AreEqual (TypeCode.Empty, rp.Type, "#A2");
		}

		[Test]
		public void Constructor_String_TypeCode_String ()
		{
			RouteParameter rp;

			rp = new RouteParameter ("name", TypeCode.Int64, "key");
			Assert.AreEqual ("name", rp.Name, "#A1-1");
			Assert.AreEqual ("key", rp.RouteKey, "#A1-2");
			Assert.AreEqual (TypeCode.Int64, rp.Type, "#A1-3");

			Assert.AreEqual (DbType.Object, rp.DbType, "#A2");
		}

		[Test]
		public void Clone ()
		{
			RouteParameter rp;
			FakeRouteParameter original;

			original = new FakeRouteParameter ("name", TypeCode.Int64, "key");
			rp = original.DoClone () as RouteParameter;

			Assert.IsNotNull (rp, "#A1-1");
			Assert.AreNotSame (original, rp, "#A1-2");

			Assert.AreEqual (original.Name, rp.Name, "#A2-1");
			Assert.AreEqual (original.Type, rp.Type, "#A2-2");
			Assert.AreEqual (original.RouteKey, rp.RouteKey, "#A2-3");
		}

		[Test]
		public void Evaluate ()
		{
			var rp = new FakeRouteParameter ();
			FakeHttpWorkerRequest2 f;
			HttpContext ctx = HttpResponseTest.Cook (1, out f);

			Assert.IsNull (rp.DoEvaluate (null, new Control ()), "#A1-1");
			Assert.IsNull (rp.DoEvaluate (ctx, null), "#A1-2");
			AssertExtensions.Throws <NullReferenceException> (() => {
				rp.DoEvaluate (ctx, new Control ());
			}, "#A1-2");
		}

		[Test]
		public void RouteKey ()
		{
			var rp = new RouteParameter ();

			rp.RouteKey = null;
			Assert.AreEqual (String.Empty, rp.RouteKey, "#A1");
		}
	}

	class FakeRouteParameter : RouteParameter
	{
		public FakeRouteParameter ()
		{ }

		public FakeRouteParameter (RouteParameter original)
			: base (original)
		{ }

		public FakeRouteParameter (string name, TypeCode type, string routeKey)
			: base (name, type, routeKey)
		{ }

		public Parameter DoClone ()
		{
			return Clone ();
		}

		public object DoEvaluate (HttpContext context, Control control)
		{
			return Evaluate (context, control);
		}
	}
}
#endif