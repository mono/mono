//
// UriTemplateEquivalenceComparerTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using NUnit.Framework;

namespace MonoTests.System
{
	[TestFixture]
	public class UriTemplateEquivalenceComparerTest
	{
		[Test]
		public void Compare ()
		{
			var t1 = new UriTemplate ("urn:foo");
			var t2 = new UriTemplate ("urn:bar");
			var t3 = new UriTemplate ("urn:foo", true);
			var dic = new Dictionary<string,string> ();
			dic.Add ("foo", "v1");
			var t4 = new UriTemplate ("urn:foo", dic);
			var c = new UriTemplateEquivalenceComparer ();
			Assert.IsFalse (c.Equals (t1, t2), "#1");
			Assert.IsTrue (c.Equals (t1, t3), "#2");
			Assert.IsTrue (c.Equals (t1, t4), "#3");
			Assert.IsTrue (c.Equals (null, null), "#4");
			Assert.IsFalse (c.Equals (null, t1), "#5");
			Assert.IsFalse (c.Equals (t1, null), "#6");
		}
	}
}
