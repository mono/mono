//
// System.Net.CookieContainerTest - CookieContainer tests
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// (c) Copyright 2004 Novell, Inc. (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Net;
using System.Reflection;

namespace MonoTests.System.Net
{
	[TestFixture]
	public class CookieContainerTest : Assertion
	{
		[Test]
		public void TestCtor1 ()
		{
			CookieContainer c = new CookieContainer (234);
			AssertEquals ("#01", 234, c.Capacity);
			bool passed = false;
			try {
				new CookieContainer (0);
				passed = false;
			} catch (ArgumentException) {
				passed = true;
			}
			AssertEquals ("#02", true, passed);

			try {
				new CookieContainer (-10);
				passed = false;
			} catch (ArgumentException) {
				passed = true;
			}
			AssertEquals ("#03", true, passed);
		}

		[Test]
		public void TestCtor3 ()
		{
			CookieContainer c = new CookieContainer (100, 50, 1000);
			AssertEquals ("#01", 100, c.Capacity);
			AssertEquals ("#02", 50, c.PerDomainCapacity);
			AssertEquals ("#03", 1000, c.MaxCookieSize);
			bool passed = false;
			try {
				new CookieContainer (100, 0, 1000);
				passed = false;
			} catch (ArgumentException) {
				passed = true;
			}
			AssertEquals ("#04", true, passed);

			try {
				new CookieContainer (100, -1, 1000);
				passed = false;
			} catch (ArgumentException) {
				passed = true;
			}
			AssertEquals ("#05", true, passed);

			try {
				new CookieContainer (100, Int32.MaxValue, 1000);
				passed = true;
			} catch (ArgumentException) {
				passed = false;
			}
			AssertEquals ("#06", true, passed);

			try {
				new CookieContainer (100, 50, 0);
				passed = false;
			} catch (ArgumentException) {
				passed = true;
			}
			AssertEquals ("#07", true, passed);

			try {
				new CookieContainer (100, 500, -4);
				passed = false;
			} catch (ArgumentException) {
				passed = true;
			}
			AssertEquals ("#08", true, passed);
		}
		
		[Test]
		public void TestDefaultLimits ()
		{
			AssertEquals ("#01", CookieContainer.DefaultCookieLengthLimit, 4096);
			AssertEquals ("#02", CookieContainer.DefaultCookieLimit, 300);
			AssertEquals ("#03", CookieContainer.DefaultPerDomainCookieLimit, 20);
		}

		[Test]
		public void TestCapacity ()
		{
			CookieContainer c = new CookieContainer ();
			AssertEquals ("#01", c.Capacity, 300);
			c.Capacity = 200;
			AssertEquals ("#02", c.Capacity, 200);
			bool passed = false;
			try {
				c.Capacity = -5;
				passed = false;
			} catch (ArgumentOutOfRangeException) {
				passed = true;
			}

			AssertEquals ("#03", true, passed);

			try {
				c.Capacity = 5; // must be >= PerDomainCapacity if PerDomainCapacity != Int32.MaxValue
				passed = false;
			} catch (ArgumentOutOfRangeException) {
				passed = true;
			}

			AssertEquals ("#04", true, passed);
			passed = false;
		}

		[Test]
		public void TestMaxCookieSize ()
		{
			CookieContainer c = new CookieContainer ();
			AssertEquals ("#01", c.MaxCookieSize, 4096);
			bool passed = false;
			try {
				c.MaxCookieSize = -5;
				passed = false;
			} catch (ArgumentOutOfRangeException) {
				passed = true;
			}

			AssertEquals ("#02", true, passed);
			try {
				c.MaxCookieSize = -1;
				passed = false;
			} catch (ArgumentOutOfRangeException) {
				passed = true;
			}

			AssertEquals ("#03", true, passed);
			c.MaxCookieSize = 80000;
			AssertEquals ("#04", 80000, c.MaxCookieSize);
			c.MaxCookieSize = Int32.MaxValue;
			AssertEquals ("#04", Int32.MaxValue, c.MaxCookieSize);
		}

		[Test]
		public void TestAdd_Args ()
		{
			CookieContainer cc = new CookieContainer ();
			bool passed = false;
			try {
				cc.Add ((Cookie) null);
				passed = false;
			} catch (ArgumentNullException) {
				passed = true;
			}
			AssertEquals ("#01", true, passed);

			try {
				cc.Add ((CookieCollection) null);
				passed = false;
			} catch (ArgumentNullException) {
				passed = true;
			}
			AssertEquals ("#02", true, passed);

			try {
				cc.Add (null, (Cookie) null);
				passed = false;
			} catch (ArgumentNullException) {
				passed = true;
			}
			AssertEquals ("#03", true, passed);

			try {
				cc.Add (null, (CookieCollection) null);
				passed = false;
			} catch (ArgumentNullException) {
				passed = true;
			}
			AssertEquals ("#04", true, passed);

			try {
				cc.Add (new Uri ("http://www.contoso.com"), (Cookie) null);
				passed = false;
			} catch (ArgumentNullException) {
				passed = true;
			}
			AssertEquals ("#05", true, passed);

			try {
				cc.Add (new Uri ("http://www.contoso.com"), (CookieCollection) null);
				passed = false;
			} catch (ArgumentNullException) {
				passed = true;
			}
			AssertEquals ("#06", true, passed);
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
			AssertEquals ("#07", "", cookie.Comment);
			AssertEquals ("#08", null, cookie.CommentUri);
			AssertEquals ("#09", "www.contoso.com", cookie.Domain);
			AssertEquals ("#10", false, cookie.Expired);
			AssertEquals ("#11", DateTime.MinValue, cookie.Expires);
			AssertEquals ("#12", "hola", cookie.Name);
			AssertEquals ("#13", "/", cookie.Path);
			AssertEquals ("#14", "", cookie.Port);
			AssertEquals ("#15", false, cookie.Secure);
			// FIX the next test
			TimeSpan ts = cookie.TimeStamp - timestamp;
			if (ts.TotalMilliseconds > 500)
				AssertEquals ("#16 timestamp", true, false);

			AssertEquals ("#17", "Adios", cookie.Value);
			AssertEquals ("#18", 0, cookie.Version);
		}

		[Test]
		public void TestGetCookies_Args ()
		{
			CookieContainer cc = new CookieContainer ();
			try {
				cc.GetCookies (null);
				AssertEquals ("#01", true, false);
			} catch (ArgumentNullException) {
			}
		}

		[Test]
		public void TestSetCookies_Args ()
		{
			CookieContainer cc = new CookieContainer ();
			try {
				cc.SetCookies (null, "");
				AssertEquals ("#01", true, false);
			} catch (ArgumentNullException) {
			}

			try {
				cc.SetCookies (new Uri ("http://www.contoso.com"), null);
				AssertEquals ("#02", true, false);
			} catch (ArgumentNullException) {
			}

			try {
				cc.SetCookies (new Uri ("http://www.contoso.com"), "=lalala");
				AssertEquals ("#03", true, false);
			} catch (CookieException) {
			}

			try {
				cc.SetCookies (new Uri ("http://www.contoso.com"), "");
			} catch (CookieException) {
				AssertEquals ("#04", true, false);
			}
		}
	}
}

