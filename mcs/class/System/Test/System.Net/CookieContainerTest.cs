//
// System.Net.CookieContainerTest - CookieContainer tests
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@novell.com)
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
