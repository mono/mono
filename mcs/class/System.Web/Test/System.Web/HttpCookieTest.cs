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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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
			Assert.AreEqual (-1, (int)cookie.SameSite);
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

		[Test (Description="Bug #553063")]
		public void CookieValuesSerialization ()
		{
			BinaryFormatter bf = new BinaryFormatter();
			HttpCookie c = new HttpCookie ("stuff", "value1");

			using (var ms = new MemoryStream()) {
				bf.Serialize(ms, c.Values);
				ms.Seek(0, SeekOrigin.Begin);
			}
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

			cookie.SameSite = SameSiteMode.Lax;
			Assert.AreEqual(SameSiteMode.Lax, cookie.SameSite, "SameSite setter");
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

		[Test]
		public void SetSameSite ()
		{
			HttpCookie cookie = new HttpCookie ("cookie", "hola");
			cookie.SameSite = SameSiteMode.Strict;
			Assert.AreEqual (SameSiteMode.Strict, cookie.SameSite, "#01");
			cookie.SameSite = SameSiteMode.None;
			Assert.AreEqual (SameSiteMode.None, cookie.SameSite, "#02");
		}

		[Test] // bug #81333
		public void ToStringTest ()
		{
			HttpCookie cookie;

			cookie = new HttpCookie ("cookie1", "this x=y is the & first = cookie");
			Assert.AreEqual ("System.Web.HttpCookie", cookie.ToString (), "#A1");
			Assert.AreEqual ("this x=y is the & first = cookie", cookie.Value, "#A2");
			Assert.AreEqual (2, cookie.Values.Count, "#A3");
			Assert.AreEqual ("this x", cookie.Values.GetKey (0), "#A4");
			Assert.AreEqual ("y is the ", cookie.Values.Get (0), "#A5");
			Assert.AreEqual (" first ", cookie.Values.GetKey (1), "#A6");
			Assert.AreEqual (" cookie", cookie.Values.Get (1), "#A7");
			Assert.AreEqual ("this+x=y+is+the+&+first+=+cookie",
				cookie.Values.ToString (), "#A8");

			cookie = new HttpCookie ("cookie11", cookie.Values.ToString ());
			Assert.AreEqual ("System.Web.HttpCookie", cookie.ToString (), "#B1");
			Assert.AreEqual ("this+x=y+is+the+&+first+=+cookie", cookie.Value, "#B2");
			Assert.AreEqual (2, cookie.Values.Count, "#B3");
			Assert.AreEqual ("this+x", cookie.Values.GetKey (0), "#B4");
			Assert.AreEqual ("y+is+the+", cookie.Values.Get (0), "#B5");
			Assert.AreEqual ("+first+", cookie.Values.GetKey (1), "#B6");
			Assert.AreEqual ("+cookie", cookie.Values.Get (1), "#B7");
			Assert.AreEqual ("this%2bx=y%2bis%2bthe%2b&%2bfirst%2b=%2bcookie",
				cookie.Values.ToString (), "#B8");

			cookie = new HttpCookie ("cookie2");
			cookie.Values ["first"] = "hell=o = y";
			cookie.Values ["second"] = "the&re";
			cookie.Values ["third"] = "three";
			cookie.Values ["three-a"] = null;
			cookie.Values ["fourth"] = "last value";
			Assert.AreEqual ("System.Web.HttpCookie", cookie.ToString (), "#C1");
			Assert.AreEqual ("first=hell=o = y&second=the&re&third=three&three"
				+ "-a=&fourth=last value", cookie.Value, "#C2");
			Assert.AreEqual (5, cookie.Values.Count, "#C3");
			Assert.AreEqual ("first", cookie.Values.GetKey (0), "#C4");
			Assert.AreEqual ("hell=o = y", cookie.Values.Get (0), "#C5");
			Assert.AreEqual ("second", cookie.Values.GetKey (1), "#C6");
			Assert.AreEqual ("the&re", cookie.Values.Get (1), "#C7");
			Assert.AreEqual ("third", cookie.Values.GetKey (2), "#C8");
			Assert.AreEqual ("three", cookie.Values.Get (2), "#C9");
			Assert.AreEqual ("three-a", cookie.Values.GetKey (3), "#C10");
			Assert.IsNull (cookie.Values.Get (3), "#C11");
			Assert.AreEqual ("fourth", cookie.Values.GetKey (4), "#C12");
			Assert.AreEqual ("last value", cookie.Values.Get (4), "#C13");
			Assert.AreEqual ("first=hell%3do+%3d+y&second=the%26re&third=three"
				+ "&three-a=&fourth=last+value", cookie.Values.ToString (), "#C14");

			cookie = new HttpCookie ("cookie21", cookie.Values.ToString ());
			Assert.AreEqual ("System.Web.HttpCookie", cookie.ToString (), "#D1");
			Assert.AreEqual ("first=hell%3do+%3d+y&second=the%26re&third=three"
				+ "&three-a=&fourth=last+value", cookie.Value, "#D2");
			Assert.AreEqual (5, cookie.Values.Count, "#D3");
			Assert.AreEqual ("first", cookie.Values.GetKey (0), "#D4");
			Assert.AreEqual ("hell%3do+%3d+y", cookie.Values.Get (0), "#D5");
			Assert.AreEqual ("second", cookie.Values.GetKey (1), "#D6");
			Assert.AreEqual ("the%26re", cookie.Values.Get (1), "#D7");
			Assert.AreEqual ("third", cookie.Values.GetKey (2), "#D8");
			Assert.AreEqual ("three", cookie.Values.Get (2), "#D9");
			Assert.AreEqual ("three-a", cookie.Values.GetKey (3), "#D10");
			Assert.AreEqual ("three-a", cookie.Values.GetKey (3), "#D11");
			Assert.AreEqual ("fourth", cookie.Values.GetKey (4), "#D12");
			Assert.AreEqual ("last+value", cookie.Values.Get (4), "#D13");
			Assert.AreEqual ("first=hell%253do%2b%253d%2by&second=the%2526re&"
				+ "third=three&three-a=&fourth=last%2bvalue",
				cookie.Values.ToString (), "#D14");

			cookie = new HttpCookie ("cookie3", "this is & the x=y third = cookie");
			cookie.Values ["first"] = "hel&l=o =y";
			cookie.Values ["second"] = "there";
			Assert.AreEqual ("System.Web.HttpCookie", cookie.ToString (), "#E1");
			Assert.AreEqual ("this is & the x=y third = cookie&first=hel&l=o =y"
				+ "&second=there", cookie.Value, "#E2");
			Assert.AreEqual (4, cookie.Values.Count, "#E3");
			Assert.IsNull (cookie.Values.GetKey (0), "#E4");
			Assert.AreEqual ("this is ", cookie.Values.Get (0), "#E5");
			Assert.AreEqual (" the x", cookie.Values.GetKey (1), "#E6");
			Assert.AreEqual ("y third = cookie", cookie.Values.Get (1), "#E7");
			Assert.AreEqual ("first", cookie.Values.GetKey (2), "#E8");
			Assert.AreEqual ("hel&l=o =y", cookie.Values.Get (2), "#E9");
			Assert.AreEqual ("second", cookie.Values.GetKey (3), "#E10");
			Assert.AreEqual ("there", cookie.Values.Get (3), "#E11");
			Assert.AreEqual ("this+is+&+the+x=y+third+%3d+cookie&first=hel%26l"
				+ "%3do+%3dy&second=there", cookie.Values.ToString (), "#E12");

			cookie = new HttpCookie ("cookie31", cookie.Values.ToString ());
			Assert.AreEqual ("System.Web.HttpCookie", cookie.ToString (), "#F1");
			Assert.AreEqual ("this+is+&+the+x=y+third+%3d+cookie&first=hel%26l"
				+ "%3do+%3dy&second=there", cookie.Value, "#F2");
			Assert.AreEqual (4, cookie.Values.Count, "#F3");
			Assert.IsNull (cookie.Values.GetKey (0), "#F4");
			Assert.AreEqual ("this+is+", cookie.Values.Get (0), "#F5");
			Assert.AreEqual ("+the+x", cookie.Values.GetKey (1), "#F6");
			Assert.AreEqual ("y+third+%3d+cookie", cookie.Values.Get (1), "#F7");
			Assert.AreEqual ("first", cookie.Values.GetKey (2), "#F8");
			Assert.AreEqual ("hel%26l%3do+%3dy", cookie.Values.Get (2), "#F9");
			Assert.AreEqual ("second", cookie.Values.GetKey (3), "#F10");
			Assert.AreEqual ("there", cookie.Values.Get (3), "#F11");
			Assert.AreEqual ("this%2bis%2b&%2bthe%2bx=y%2bthird%2b%253d%2bcookie"
				+ "&first=hel%2526l%253do%2b%253dy&second=there",
				cookie.Values.ToString (), "#F12");

			cookie = new HttpCookie ("funkycookie", "`~!@#$%^&*()_+-=\\][{}|'\";:,<.>/?");
			Assert.AreEqual ("System.Web.HttpCookie", cookie.ToString (), "#G1");
			Assert.AreEqual ("`~!@#$%^&*()_+-=\\][{}|'\";:,<.>/?", cookie.Value, "#G2");
			Assert.AreEqual (2, cookie.Values.Count, "#G3");
			Assert.IsNull (cookie.Values.GetKey (0), "#G4");
			Assert.AreEqual ("`~!@#$%^", cookie.Values.Get (0), "#G5");
			Assert.AreEqual ("*()_+-", cookie.Values.GetKey (1), "#G6");
			Assert.AreEqual ("\\][{}|'\";:,<.>/?", cookie.Values.Get (1), "#G7");
			Assert.AreEqual ("%60%7e!%40%23%24%25%5e&*()_%2b-=%5c%5d%5b%7b%7d"
				+ "%7c%27%22%3b%3a%2c%3c.%3e%2f%3f", cookie.Values.ToString (), "#G8");

			cookie = new HttpCookie ("funkycookie11", cookie.Values.ToString ());
			Assert.AreEqual ("System.Web.HttpCookie", cookie.ToString (), "#H1");
			Assert.AreEqual ("%60%7e!%40%23%24%25%5e&*()_%2b-=%5c%5d%5b%7b%7d"
				+ "%7c%27%22%3b%3a%2c%3c.%3e%2f%3f", cookie.Value, "#H2");
			Assert.AreEqual (2, cookie.Values.Count, "#H3");
			Assert.IsNull (cookie.Values.GetKey (0), "#H4");
			Assert.AreEqual ("%60%7e!%40%23%24%25%5e", cookie.Values.Get (0), "#H5");
			Assert.AreEqual ("*()_%2b-", cookie.Values.GetKey (1), "#H6");
			Assert.AreEqual ("%5c%5d%5b%7b%7d%7c%27%22%3b%3a%2c%3c.%3e%2f%3f",
				cookie.Values.Get (1), "#H7");
			Assert.AreEqual ("%2560%257e!%2540%2523%2524%2525%255e&*()_%252b-="
				+ "%255c%255d%255b%257b%257d%257c%2527%2522%253b%253a%252c%253c.%2"
				+ "53e%252f%253f", cookie.Values.ToString (), "#H8");

			cookie = new HttpCookie ("basic");
			cookie.Values ["one"] = "hello world";
			cookie.Values ["two"] = "a^2 + b^2 = c^2";
			cookie.Values ["three"] = "a & b";
			Assert.AreEqual ("System.Web.HttpCookie", cookie.ToString (), "#I1");
			Assert.AreEqual ("one=hello world&two=a^2 + b^2 = c^2&three=a & b",
				cookie.Value, "#I2");
			Assert.AreEqual (3, cookie.Values.Count, "#I3");
			Assert.AreEqual ("one", cookie.Values.GetKey (0), "#I4");
			Assert.AreEqual ("hello world", cookie.Values.Get (0), "#I5");
			Assert.AreEqual ("two", cookie.Values.GetKey (1), "#I6");
			Assert.AreEqual ("a^2 + b^2 = c^2", cookie.Values.Get (1), "#I7");
			Assert.AreEqual ("three", cookie.Values.GetKey (2), "#I8");
			Assert.AreEqual ("a & b", cookie.Values.Get (2), "#I9");
			Assert.AreEqual ("one=hello+world&two=a%5e2+%2b+b%5e2+%3d+c%5e2&"
				+ "three=a+%26+b", cookie.Values.ToString (), "#I10");

			HttpCookie cookie2 = new HttpCookie ("basic2");
			cookie2.Value = cookie.Values.ToString ();
			Assert.AreEqual ("System.Web.HttpCookie", cookie2.ToString (), "#J1");
			Assert.AreEqual ("one=hello+world&two=a%5e2+%2b+b%5e2+%3d+c%5e2&"
				+ "three=a+%26+b", cookie2.Value, "#J2");
			Assert.AreEqual (3, cookie2.Values.Count, "#J3");
			Assert.AreEqual ("one", cookie.Values.GetKey (0), "#J4");
			Assert.AreEqual ("hello world", cookie.Values.Get (0), "#J5");
			Assert.AreEqual ("two", cookie.Values.GetKey (1), "#J6");
			Assert.AreEqual ("a^2 + b^2 = c^2", cookie.Values.Get (1), "#J7");
			Assert.AreEqual ("three", cookie.Values.GetKey (2), "#J8");
			Assert.AreEqual ("a & b", cookie.Values.Get (2), "#J9");
			Assert.AreEqual ("one=hello%2bworld&two=a%255e2%2b%252b%2bb%255e2"
				+ "%2b%253d%2bc%255e2&three=a%2b%2526%2bb",
				cookie2.Values.ToString (), "#J10");
		}
		[Test]
		public void HttpOnly ()
		{
			HttpCookie biscuit = new HttpCookie ("mium");
			Assert.IsFalse (biscuit.HttpOnly, "default");
			biscuit.HttpOnly = true;
			Assert.IsTrue (biscuit.HttpOnly, "true");
			biscuit.HttpOnly = false;
			Assert.IsFalse (biscuit.HttpOnly, "false");
		}
	}
}
