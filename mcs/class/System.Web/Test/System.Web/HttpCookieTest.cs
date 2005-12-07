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
	public class HttpCookieTest {

		[Test]
		public void DefaultProperties ()
		{
			HttpCookie cookie = new HttpCookie ("cookie");

			Assert.IsNull   (cookie.Domain, "default cookie.Domain");
			Assert.AreEqual (new DateTime(), cookie.Expires, "default cookie.Expires");
			Assert.IsFalse  (cookie.HasKeys, "default cookie.HasKeys");
			Assert.AreEqual ("cookie", cookie.Name, "default cookie.Name");
			Assert.AreEqual ("/", cookie.Path, "default cookie.Path");
			Assert.IsFalse  (cookie.Secure, "default cookie.Secure");
			Assert.AreEqual ("", cookie.Value, "default cookie.Value");
			Assert.AreEqual ("", cookie.Values.ToString(), "default cookie.Values");
		}

		[Test]
		public void DefaultValueProperties ()
		{
			HttpCookie cookie = new HttpCookie ("cookie", "value");

			Assert.AreEqual ("value", cookie.Value, "Value getter");
			Assert.AreEqual ("value", cookie.Values[null], "Values[null] getter");
			Assert.AreEqual ("value", cookie[null], "cookie[null] getter");
			Assert.AreEqual ("value", cookie.Values.ToString(), "Values getter");

			/* make sure HasKeys is still false for single valued cookies */
			Assert.IsFalse (cookie.HasKeys);
		}

		[Test]
		public void PropertySetters ()
		{
			/* test the setters for settle Properties */
			HttpCookie cookie = new HttpCookie ("name");

			cookie.Domain = "novell.com";
			Assert.AreEqual ("novell.com", cookie.Domain, "Domain setter");

			DateTime dt = DateTime.Now;
			cookie.Expires = dt;
			Assert.AreEqual (dt, cookie.Expires, "Expires setter");

			cookie.Path = "/example/path";
			Assert.AreEqual ("/example/path", cookie.Path, "Path setter");

			cookie.Secure = true;
			Assert.IsTrue (cookie.Secure, "Secure setter");
		}

		[Test]
		public void MultiValued1 ()
		{
			HttpCookie cookie = new HttpCookie ("name1", "value1");
			cookie ["name2"] = "value2";

			Assert.IsTrue (cookie.HasKeys, "multi valued HasKeys");

			Assert.AreEqual (2, cookie.Values.Count, "Values.Count");

			Assert.AreEqual ("value1&name2=value2", cookie.Value, "Value getter");
			Assert.AreEqual ("value1&name2=value2", cookie.Values.ToString(), "Values getter");
		}

		[Test]
		public void SubkeysOnly ()
		{
			HttpCookie cookie = new HttpCookie ("name1");
			cookie ["name2"] = "value2";
			cookie ["name3"] = "value3";

			Assert.IsTrue (cookie.HasKeys, "multi valued HasKeys");

			Assert.AreEqual (2, cookie.Values.Count, "Values.Count");

			Assert.AreEqual ("name2=value2&name3=value3", cookie.Value, "Value getter");
			Assert.AreEqual ("name2=value2&name3=value3", cookie.Values.ToString(), "Values getter");
		}

		[Test]
		public void ResettingValue1 ()
		{
			HttpCookie cookie = new HttpCookie ("name1", "value1");
			cookie.Value = "value2";

			Assert.AreEqual ("value2", cookie.Value, "Value getter");
			Assert.AreEqual ("value2", cookie.Values.ToString(), "Values getter");

			/* make sure HasKeys is still false for single valued cookies */
			Assert.IsFalse (cookie.HasKeys);
		}

		[Test]
		public void ResettingValue2 ()
		{
			HttpCookie cookie = new HttpCookie ("name1", "value1");

			cookie["name2"] = "value2";
			cookie["name3"] = "value3";

			cookie.Value = "value1";

			Assert.AreEqual ("value1", cookie.Value, "Value getter");
			Assert.AreEqual ("value1", cookie.Values.ToString(), "Values getter");

			/* make sure HasKeys is back to false */
			Assert.IsFalse (cookie.HasKeys);
		}

		[Test]
		public void MultiValuedSet1 ()
		{
			HttpCookie cookie = new HttpCookie ("name1", "value1&name2=value2");

			Assert.IsTrue (cookie.HasKeys, "MultiValuedSet");
			Assert.AreEqual ("value1&name2=value2", cookie.Value, "cookie.Value");
			Assert.AreEqual ("value1", cookie.Values[null], "Values[null] getter");
			Assert.AreEqual ("value1", cookie[null], "cookie[null] getter");
			Assert.AreEqual ("value2", cookie ["name2"], "name2 getter");
		}

		[Test]
		public void MultiValuedSet2 ()
		{
			HttpCookie cookie = new HttpCookie ("name1", "value1&name2=value2&name2=value3");

			Assert.IsTrue (cookie.HasKeys, "MultiValuedSet");
			Assert.AreEqual ("value1&name2=value2&name2=value3", cookie.Value, "cookie.Value");
			Assert.AreEqual ("value1", cookie.Values[null], "Values[null] getter");
			Assert.AreEqual ("value1", cookie[null], "cookie[null] getter");
			Assert.AreEqual ("value2,value3", cookie ["name2"], "name2 getter");
		}

		[Test]
		public void MultiUnnamedSet ()
		{
			HttpCookie cookie = new HttpCookie ("name1", "value1&name2=value2&value3");

			Assert.IsTrue (cookie.HasKeys, "MultiUnnamedSet");
			Assert.AreEqual ("value1&value3&name2=value2", cookie.Value, "cookie.Value");
			Assert.AreEqual ("value1,value3", cookie.Values[null], "Values[null] getter");
			Assert.AreEqual ("value1,value3", cookie[null], "Values[null] getter");
			Assert.AreEqual ("value2", cookie ["name2"], "name2 getter");
		}

		[Test]
		public void NullSetter ()
		{
			HttpCookie cookie = new HttpCookie ("name1");

			cookie.Values[null] = "value1";
			Assert.AreEqual ("value1", cookie.Values.ToString(), "Values getter");
			Assert.AreEqual ("value1", cookie.Value, "Value getter");
	
			// This strikes me as odd behavior..  We fail
			// this test presently, but I'm not sure if
			// it's because of a bug in
			// NameValueCollection.  If NVC doesn't
			// normally have the behavior of clearing the
			// collection when setting this[null], we can
			// override the behavior in our cookie
			// specific subclass.
			cookie.Value = "value1&name2=value2&value3";
			cookie[null] = "value1";
			Assert.AreEqual ("value1", cookie.Values.ToString(), "Values getter");
			Assert.AreEqual ("value1", cookie.Value, "Value getter");
			
			// try the same test above, but circumvent the
			// HttpCookie.item property.
			cookie.Value = "value1&name2=value2&value3";
			cookie.Values[null] = "value1";
			Assert.AreEqual ("value1", cookie.Values.ToString(), "Values getter");
			Assert.AreEqual ("value1", cookie.Value, "Value getter");
		}

		[Test]
		public void Encode ()
		{
			// make sure there's no encoding done on values
			HttpCookie cookie = new HttpCookie ("cookie");
			cookie["name"] = "val&ue";

			Assert.AreEqual ("val&ue", cookie["name"], "name getter");
			Assert.AreEqual ("name=val&ue", cookie.Value, "Value getter");
		}

		[Test]
		public void SetSecure ()
		{
			HttpCookie cookie = new HttpCookie ("cookie", "hola");
			cookie.Secure = true;
			Assert.IsTrue (cookie.Secure, "#01");
			cookie.Secure = false;
			Assert.IsFalse (cookie.Secure, "#02");
		}
	}
}

