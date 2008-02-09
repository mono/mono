//
// System.Net.CookieContainerTest - CookieContainer tests
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@novell.com)
//      Daniel Nauck    (dna(at)mono-project(dot)de)
//
// (c) Copyright 2004 Novell, Inc. (http://www.novell.com)
//

using System;
using System.Net;
using System.Reflection;

using NUnit.Framework;

namespace MonoTests.System.Net
{
	[TestFixture]
	public class CookieContainerTest
	{
		[Test]
		public void TestCtor1 ()
		{
			CookieContainer c = new CookieContainer (234);
			Assert.AreEqual (234, c.Capacity, "#1");

			try {
				new CookieContainer (0);
				Assert.Fail ("#2");
			} catch (ArgumentException) {
			}

			try {
				new CookieContainer (-10);
				Assert.Fail ("#3");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void TestCtor3 ()
		{
			CookieContainer c = new CookieContainer (100, 50, 1000);
			Assert.AreEqual (100, c.Capacity, "#1");
			Assert.AreEqual (50, c.PerDomainCapacity, "#2");
			Assert.AreEqual (1000, c.MaxCookieSize, "#3");

			try {
				new CookieContainer (100, 0, 1000);
				Assert.Fail ("#4");
			} catch (ArgumentException) {
			}

			try {
				new CookieContainer (100, -1, 1000);
				Assert.Fail ("#5");
			} catch (ArgumentException) {
			}

			c = new CookieContainer (100, int.MaxValue, 1000);
			Assert.AreEqual (int.MaxValue, c.PerDomainCapacity, "#6");

			try {
				new CookieContainer (100, 50, 0);
				Assert.Fail ("#7");
			} catch (ArgumentException) {
			}

			try {
				new CookieContainer (100, 500, -4);
				Assert.Fail ("#8");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void TestDefaultLimits ()
		{
			Assert.AreEqual (4096, CookieContainer.DefaultCookieLengthLimit, "#1");
			Assert.AreEqual (300, CookieContainer.DefaultCookieLimit, "#2");
			Assert.AreEqual (20, CookieContainer.DefaultPerDomainCookieLimit, "#3");
		}

		[Test]
		public void TestCapacity ()
		{
			CookieContainer c = new CookieContainer ();
			Assert.AreEqual (300, c.Capacity, "#1");
			c.Capacity = 200;
			Assert.AreEqual (200, c.Capacity, "#2");

			try {
				c.Capacity = -5;
				Assert.Fail ("#3");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				c.Capacity = 5; // must be >= PerDomainCapacity if PerDomainCapacity != Int32.MaxValue
				Assert.Fail ("#4");
			} catch (ArgumentOutOfRangeException) {
			}
		}

		[Test]
		public void TestMaxCookieSize ()
		{
			CookieContainer c = new CookieContainer ();
			Assert.AreEqual (4096, c.MaxCookieSize, "#1");

			try {
				c.MaxCookieSize = -5;
				Assert.Fail ("#2");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				c.MaxCookieSize = -1;
				Assert.Fail ("#3");
			} catch (ArgumentOutOfRangeException) {
			}

			c.MaxCookieSize = 80000;
			Assert.AreEqual (80000, c.MaxCookieSize, "#4");
			c.MaxCookieSize = int.MaxValue;
			Assert.AreEqual (int.MaxValue, c.MaxCookieSize, "#5");
		}

		[Test]
		public void TestAdd_Args ()
		{
			CookieContainer cc = new CookieContainer ();

			try {
				cc.Add ((Cookie) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}

			try {
				cc.Add ((CookieCollection) null);
				Assert.Fail ("#2");
			} catch (ArgumentNullException) {
			}

			try {
				cc.Add (null, (Cookie) null);
				Assert.Fail ("#3");
			} catch (ArgumentNullException) {
			}

			try {
				cc.Add (null, (CookieCollection) null);
				Assert.Fail ("#4");
			} catch (ArgumentNullException) {
			}

			try {
				cc.Add (new Uri ("http://www.contoso.com"), (Cookie) null);
				Assert.Fail ("#5");
			} catch (ArgumentNullException) {
			}

			try {
				cc.Add (new Uri ("http://www.contoso.com"), (CookieCollection) null);
				Assert.Fail ("#6");
			} catch (ArgumentNullException) {
			}
		}

		[Test]
		public void TestAdd_Cookie ()
		{
			CookieContainer cc = new CookieContainer ();
			Uri uri = new Uri ("http://www.contoso.com");
			cc.Add (uri, new CookieCollection ());
			DateTime timestamp = DateTime.Now;
			cc.Add (uri, new Cookie ("hola", "Adios"));
			CookieCollection coll = cc.GetCookies (uri);
			Cookie cookie = coll [0];
			Assert.AreEqual ("", cookie.Comment, "#1");
			Assert.IsNull (cookie.CommentUri, "#2");
			Assert.AreEqual ("www.contoso.com", cookie.Domain, "#3");
			Assert.IsFalse (cookie.Expired, "#4");
			Assert.AreEqual (DateTime.MinValue, cookie.Expires, "#5");
			Assert.AreEqual ("hola", cookie.Name, "#6");
			Assert.AreEqual ("/", cookie.Path, "#7");
			Assert.AreEqual ("", cookie.Port, "#8");
			Assert.IsFalse (cookie.Secure, "#9");
			// FIX the next test
			TimeSpan ts = cookie.TimeStamp - timestamp;
			if (ts.TotalMilliseconds > 500)
				Assert.Fail ("#10");

			Assert.AreEqual ("Adios", cookie.Value, "#11");
			Assert.AreEqual (0, cookie.Version, "#12");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestAddExpired_Cookie ()
		{
			CookieContainer cc = new CookieContainer ();
			Uri uri = new Uri ("http://www.contoso.com");
			DateTime expires = DateTime.Now.Subtract (new TimeSpan (1, 30, 0));

			//expired cookie
			Cookie c1 = new Cookie ("TEST", "MyValue", "/", uri.Host);
			c1.Expires = expires;
			cc.Add (c1);
			Assert.AreEqual (1, cc.Count, "#A1");
			CookieCollection coll = cc.GetCookies (uri);
			Assert.AreEqual (1, coll.Count, "#A1.1");
			Cookie cookie = coll [0];
			Assert.AreEqual ("", cookie.Comment, "#A2");
			Assert.IsNull (cookie.CommentUri, "#A3");
			Assert.AreEqual ("www.contoso.com", cookie.Domain, "#A4");
			Assert.IsFalse (cookie.Expired, "#A5");
			Assert.AreEqual (DateTime.MinValue, cookie.Expires, "#A6");
			Assert.AreEqual ("TEST", cookie.Name, "#A7");
			Assert.AreEqual ("MyValue", cookie.Value, "#A8");
			Assert.AreEqual ("/", cookie.Path, "#A9");
			Assert.AreEqual ("", cookie.Port, "#A10");
			Assert.IsFalse (cookie.Secure, "#A11");

			//expired cookie
			Cookie c2 = new Cookie ("TEST2", "MyValue2");
			c2.Expires = expires;
			cc.Add (uri, c2);
			Assert.AreEqual (1, cc.Count, "#B1");
			coll = cc.GetCookies (uri);
			Assert.AreEqual (1, coll.Count, "#B1.1");
			cookie = coll [0];
			Assert.AreEqual ("", cookie.Comment, "#B2");
			Assert.IsNull (cookie.CommentUri, "#B3");
			Assert.AreEqual ("www.contoso.com", cookie.Domain, "#B4");
			Assert.IsFalse (cookie.Expired, "#B5");
			Assert.AreEqual (DateTime.MinValue, cookie.Expires, "#B6");
			Assert.AreEqual ("TEST", cookie.Name, "#B7");
			Assert.AreEqual ("MyValue", cookie.Value, "#B8");
			Assert.AreEqual ("/", cookie.Path, "#B9");
			Assert.AreEqual ("", cookie.Port, "#B10");
			Assert.IsFalse (cookie.Secure, "#B11");

			//not expired cookie
			Cookie c3 = new Cookie ("TEST3", "MyValue3");
			cc.Add (uri, c3);
			Assert.AreEqual (2, cc.Count, "#C1");
			coll = cc.GetCookies (uri);
			Assert.AreEqual (2, coll.Count, "#C1.1");
			cookie = coll [1];
			Assert.AreEqual ("", cookie.Comment, "#C2");
			Assert.IsNull (cookie.CommentUri, "#C3");
			Assert.AreEqual ("www.contoso.com", cookie.Domain, "#C4");
			Assert.IsFalse (cookie.Expired, "#C5");
			Assert.AreEqual (DateTime.MinValue, cookie.Expires, "#C6");
			Assert.AreEqual ("TEST3", cookie.Name, "#C7");
			Assert.AreEqual ("MyValue3", cookie.Value, "#C8");
			Assert.AreEqual ("/", cookie.Path, "#C9");
			Assert.AreEqual ("", cookie.Port, "#C10");
			Assert.IsFalse (cookie.Secure, "#C11");

			Assert.AreEqual (2, cc.Count, "#D1");
			coll = cc.GetCookies (new Uri("http://contoso.com"));
			Assert.AreEqual (0, coll.Count, "#D1.1");

			//not expired cookie
			Cookie c4 = new Cookie ("TEST4", "MyValue4", "/", ".contoso.com");
			cc.Add (uri, c4);
			Assert.AreEqual (3, cc.Count, "#E1");
			coll = cc.GetCookies (uri);
			Assert.AreEqual (3, coll.Count, "#E1.1");

			//expired cookie
			Cookie c5 = new Cookie ("TEST5", "MyValue5", "/", ".contoso.com");
			c5.Expires = expires;
			cc.Add (c5);
			Assert.AreEqual (4, cc.Count, "#F1");
			coll = cc.GetCookies (uri);
			Assert.AreEqual (4, coll.Count, "#F1.1");
			cookie = coll ["TEST5"];
			Assert.AreEqual (".contoso.com", cookie.Domain, "#F2");
			Assert.IsFalse (cookie.Expired, "#F3");
			Assert.AreEqual (DateTime.MinValue, cookie.Expires, "#F4");
			Assert.AreEqual ("TEST5", cookie.Name, "#F5");
			Assert.AreEqual ("MyValue5", cookie.Value, "#F6");
			Assert.AreEqual ("/", cookie.Path, "#F7");

			//expired cookie
			Cookie c6 = new Cookie ("TEST6", "MyValue6", "/", ".contoso.com");
			c5.Expires = expires;
			cc.Add (uri, c6);
			Assert.AreEqual (5, cc.Count, "#G1");
			coll = cc.GetCookies (uri);
			Assert.AreEqual (5, coll.Count, "#G1.1");
			cookie = coll ["TEST6"];
			Assert.AreEqual (".contoso.com", cookie.Domain, "#G2");
			Assert.IsFalse (cookie.Expired, "#G3");
			Assert.AreEqual (DateTime.MinValue, cookie.Expires, "#G4");
			Assert.AreEqual ("TEST6", cookie.Name, "#G5");
			Assert.AreEqual ("MyValue6", cookie.Value, "#G6");
			Assert.AreEqual ("/", cookie.Path, "#G7");
		}

		[Test]
		public void TestGetCookies_Args ()
		{
			CookieContainer cc = new CookieContainer ();
			try {
				cc.GetCookies (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}
		}

		[Test]
		public void TestSetCookies_Args ()
		{
			CookieContainer cc = new CookieContainer ();
			try {
				cc.SetCookies (null, "");
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}

			try {
				cc.SetCookies (new Uri ("http://www.contoso.com"), null);
				Assert.Fail ("#2");
			} catch (ArgumentNullException) {
			}

			try {
				cc.SetCookies (new Uri ("http://www.contoso.com"), "=lalala");
				Assert.Fail ("#3");
			} catch (CookieException) {
			}

			cc.SetCookies (new Uri ("http://www.contoso.com"), "");
		}

		[Test]
		public void GetCookies ()
		{
			CookieContainer container = new CookieContainer ();
			container.Add (new Cookie ("name", "value1", "/path", "localhost"));
			container.Add (new Cookie ("name", "value2", "/path/sub", "localhost"));

			CookieCollection cookies = container.GetCookies (
				new Uri ("http://localhost/path/sub"));
			Assert.IsNotNull (cookies, "#A1");
			Assert.AreEqual (2, cookies.Count, "#A2");

			Cookie cookie = cookies [0];
			Assert.AreEqual ("name", cookie.Name, "#B1");
			Assert.AreEqual ("value2", cookie.Value, "#B2");
			Assert.AreEqual ("/path/sub", cookie.Path, "#B3");
			Assert.AreEqual ("localhost", cookie.Domain, "#B4");

			cookie = cookies [1];
			Assert.AreEqual ("name", cookie.Name, "#C1");
			Assert.AreEqual ("value1", cookie.Value, "#C2");
			Assert.AreEqual ("/path", cookie.Path, "#C3");
			Assert.AreEqual ("localhost", cookie.Domain, "#C4");

			cookies = container.GetCookies (new Uri ("http://localhost/path"));
			Assert.IsNotNull (cookies, "#D1");
			Assert.AreEqual (1, cookies.Count, "#D2");

			cookie = cookies [0];
			Assert.AreEqual ("name", cookie.Name, "#E1");
			Assert.AreEqual ("value1", cookie.Value, "#E2");
			Assert.AreEqual ("/path", cookie.Path, "#E3");
			Assert.AreEqual ("localhost", cookie.Domain, "#E4");

			cookies = container.GetCookies (new Uri ("http://localhost/whatever"));
			Assert.IsNotNull (cookies, "#F1");
			Assert.AreEqual (0, cookies.Count, "#F2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetCookies_Uri_Null ()
		{
			CookieContainer container = new CookieContainer ();
			container.GetCookies ((Uri) null);
		}
	}
}
