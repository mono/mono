﻿//
// SubstitutionResponseElementTest.cs
//
// Authors:
//      Marek Habersack <mhabersack@novell.com>
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
using System.Web;
using System.Web.Caching;

using NUnit.Framework;
using MonoTests.Common;

namespace MonoTests.System.Web.Caching
{
	[TestFixture]
	public class SubstitutionResponseElementTest
	{
		string TestCallback (HttpContext ctx)
		{
			return String.Empty;
		}

		[Test]
		public void Constructor ()
		{
			SubstitutionResponseElement sre;

			AssertExtensions.Throws<ArgumentNullException> (() => {
				sre = new SubstitutionResponseElement (null);
			}, "#A1");

			var cb = new HttpResponseSubstitutionCallback (TestCallback);
			sre = new SubstitutionResponseElement (cb);
			Assert.AreEqual (cb, sre.Callback, "#B1");
		}
	}
}
