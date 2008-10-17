//
// RouteValueDictionaryTest.cs
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
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using NUnit.Framework;

namespace MonoTests.System.Web.Routing
{
	[TestFixture]
	public class RouteValueDictionaryTest
	{
		[Test]
		public void ConstructorNullArgs ()
		{
			// null is allowed
			new RouteValueDictionary ((object) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullArgs2 ()
		{
			new RouteValueDictionary ((IDictionary<string,object>) null);
		}

		[Test]
		public void ConstructorObject ()
		{
			var d = new RouteValueDictionary (new {Foo = "urn:foo", Bar = "urn:bar"});
			Assert.AreEqual ("urn:foo", d ["Foo"], "#1");
			Assert.AreEqual ("urn:bar", d ["Bar"], "#2");
			Assert.IsNull (d ["Baz"], "#3");
		}

		[Test]
		public void CaseInsensitiveMatch ()
		{
			var d = new RouteValueDictionary (new {Foo = "urn:foo", Bar = "urn:bar"});
			Assert.AreEqual ("urn:foo", d ["foo"], "#1");
			Assert.AreEqual ("urn:bar", d ["BAR"], "#2");
		}

		[Test]
		public void Keys ()
		{
			var d = new RouteValueDictionary (new {Foo = "urn:foo", Bar = "urn:bar"});
			int i = 0;
			foreach (var k in d.Keys) {
				if (k == "Foo")
					i += 1;
				if (k == "Bar")
					i += 2;
			}
			Assert.AreEqual (3, i);
		}
	}
}
