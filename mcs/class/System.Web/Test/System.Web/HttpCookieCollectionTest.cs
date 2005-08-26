//
// System.Web.HttpCookieTest.cs - Unit tests for System.Web.HttpCookie
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
	public class HttpCookieCollectionTest {

		[Test]
		public void TestCollection ()
		{
			HttpCookieCollection col = new HttpCookieCollection ();
			HttpCookie cookie = new HttpCookie ("cookie", "value");

			col.Add (cookie);

			Assert.AreEqual ("cookie", col["cookie"].Name, "Name is the key");

			col.Remove (cookie.Name);
			Assert.IsNull (col["cookie"], "removal using name");
		}

		[Test]
		public void DuplicateNames ()
		{
			HttpCookieCollection col = new HttpCookieCollection ();
			HttpCookie cookie1 = new HttpCookie ("cookie", "value1");
			HttpCookie cookie2 = new HttpCookie ("cookie", "value2");

			col.Add (cookie1);
			col.Add (cookie2);

			Assert.AreEqual ("value1", col["cookie"].Value, "add should use first used cookie");

			col.Set (cookie2);
			Assert.AreEqual ("value2", col["cookie"].Value, "set should use last used cookie");
		}

		[Test]
		public void UpdateTest ()
		{
			// the msdn examples show updates with col.Set being
			// called after a change is made to a cookie.  turns
			// out this isn't necessary.

			HttpCookieCollection col = new HttpCookieCollection ();
			HttpCookie cookie1, cookie2;

			cookie1 = new HttpCookie ("cookie", "value1");

			col.Add (cookie1);

			cookie2 = col.Get (cookie1.Name);
			cookie2.Value = "value2";

			cookie2 = col.Get (cookie1.Name);
			Assert.AreEqual ("value2", cookie2.Value, "col.Get doesn't copy");
		}

		[Test]
		public void CopyToTest ()
		{
			HttpCookieCollection col = new HttpCookieCollection ();
			HttpCookie[] cookies = new HttpCookie[2];

			col.Add (new HttpCookie ("cookie1", "value1"));
			col.Add (new HttpCookie ("cookie2", "value2"));

			col.CopyTo (cookies, 0);

			Assert.AreEqual ("cookie1", cookies[0].Name, "cookies[0].Name");
			Assert.AreEqual ("cookie2", cookies[1].Name, "cookies[1].Name");
		}

		[Test]
		public void Properties ()
		{
			HttpCookieCollection col = new HttpCookieCollection ();
			string[] allkeys;

			col.Add (new HttpCookie ("cookie1", "value1"));
			col.Add (new HttpCookie ("cookie2", "value2"));

			allkeys = col.AllKeys;

			Assert.AreEqual (2, allkeys.Length, "there should be 2");
			Assert.AreEqual ("cookie1", allkeys[0], "allkeys[0]");
			Assert.AreEqual ("cookie2", allkeys[1], "allkeys[1]");
		}
	}

}
