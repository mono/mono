//
// System.Web.HttpCacheVaryByHeadersTest.cs - Unit tests for System.Web.HttpCacheByHeaders
//
// Author:
//	Chris Toshok  <toshok@novell.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using NUnit.Framework;

namespace MonoTests.System.Web {

	[TestFixture]
	public class HttpCacheVaryByHeadersTest {

		[Test]
		public void Properties ()
		{
			HttpResponse response = new HttpResponse (Console.Out);
			HttpCacheVaryByHeaders hdrs = response.Cache.VaryByHeaders;

			/* first test all the getters/setters for the builtin fields */
			hdrs.AcceptTypes = true;
			hdrs.UserAgent = true;
			hdrs.UserCharSet = true;
			hdrs.UserLanguage = true;
			hdrs["custom-field"] = true;

			Assert.IsTrue (hdrs.AcceptTypes, "hdrs.AcceptTypes == true");
			Assert.IsTrue (hdrs.UserAgent, "hdrs.UserAgent == true");
			Assert.IsTrue (hdrs.UserCharSet, "hdrs.UserCharSet == true");
			Assert.IsTrue (hdrs.UserLanguage, "hdrs.UserLanguage == true");
			Assert.IsTrue (hdrs["custom-field"], "hdrs['custom-field'] == true");

			/* test case sensitivity */
			Assert.IsTrue (hdrs["Custom-Field"], "hdrs['Custom-Field'] == true");

			hdrs.VaryByUnspecifiedParameters();

			/* now verify that they're all false */
			Assert.IsFalse (hdrs.AcceptTypes, "hdrs.AcceptTypes == false");
			Assert.IsFalse (hdrs.UserAgent, "hdrs.UserAgent == false");
			Assert.IsFalse (hdrs.UserCharSet, "hdrs.UserCharSet == false");
			Assert.IsFalse (hdrs.UserLanguage, "hdrs.UserLanguage == false");
			Assert.IsFalse (hdrs["custom-field"], "hdrs['custom-field'] == false");
		}
	}
}
