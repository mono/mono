//
// System.Net.CookieContainerTest - CookieContainer tests
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@novell.com)
//      Daniel Nauck    (dna(at)mono-project(dot)de)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004,2009 Novell, Inc (http://www.novell.com)
//

using System;
using System.Net;
using System.Reflection;

using NUnit.Framework;

namespace MonoTests.System.Net {
	[TestFixture]
	public class CookieContainerTest {
		[Test] // .ctor ()
		public void Constructor1 ()
		{
			CookieContainer c = new CookieContainer ();
			Assert.AreEqual (0, c.Count, "Count");
			Assert.AreEqual (CookieContainer.DefaultCookieLimit, c.Capacity, "Capacity");
			Assert.AreEqual (CookieContainer.DefaultCookieLengthLimit, c.MaxCookieSize, "MaxCookieSize");
			Assert.AreEqual (CookieContainer.DefaultPerDomainCookieLimit, c.PerDomainCapacity, "PerDomainCapacity");
		}

		[Test] // .ctor (Int32)
		public void Constructor2 ()
		{
			CookieContainer c = new CookieContainer (234);
			Assert.AreEqual (0, c.Count, "Count");
			Assert.AreEqual (234, c.Capacity, "Capacity");
			Assert.AreEqual (CookieContainer.DefaultCookieLengthLimit, c.MaxCookieSize, "MaxCookieSize");
			Assert.AreEqual (CookieContainer.DefaultPerDomainCookieLimit, c.PerDomainCapacity, "PerDomainCapacity");
		}

		[Test]
		public void Constructor2_Capacity_Invalid ()
		{
			// Capacity <= 0
			try {
				new CookieContainer (0);
				Assert.Fail ("#A1");
			}
			catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
#if NET_2_0
				// The specified value must be greater than 0
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("Capacity", ex.ParamName, "#A5");
#else
				Assert.AreEqual ("Capacity", ex.Message, "#A4");
				Assert.IsNull (ex.ParamName, "#A5");
#endif
			}

			// Capacity <= 0
			try {
				new CookieContainer (-10);
				Assert.Fail ("#B1");
			}
			catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
#if NET_2_0
				// The specified value must be greater than 0
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("Capacity", ex.ParamName, "#B5");
#else
				Assert.AreEqual ("Capacity", ex.Message, "#B4");
				Assert.IsNull (ex.ParamName, "#B5");
#endif
			}
		}

		[Test] // .ctor (Int32, Int32, Int32)
		public void Constructor3 ()
		{
			CookieContainer c;

			c = new CookieContainer (100, 50, 1000);
			Assert.AreEqual (100, c.Capacity, "#A1");
			Assert.AreEqual (50, c.PerDomainCapacity, "#A2");
			Assert.AreEqual (1000, c.MaxCookieSize, "#A3");

			c = new CookieContainer (234, int.MaxValue, 650);
			Assert.AreEqual (234, c.Capacity, "#A1");
			Assert.AreEqual (int.MaxValue, c.PerDomainCapacity, "#A2");
			Assert.AreEqual (650, c.MaxCookieSize, "#A3");

			c = new CookieContainer (234, 234, 100);
			Assert.AreEqual (234, c.Capacity, "#A1");
			Assert.AreEqual (234, c.PerDomainCapacity, "#A2");
			Assert.AreEqual (100, c.MaxCookieSize, "#A3");
		}

		[Test]
		public void Constructor3_Capacity_Invalid ()
		{
			// Capacity <= 0
			try {
				new CookieContainer (0, 0, 100);
				Assert.Fail ("#A1");
			}
			catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
#if NET_2_0
				// The specified value must be greater than 0
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("Capacity", ex.ParamName, "#A5");
#else
				Assert.AreEqual ("Capacity", ex.Message, "#A4");
				Assert.IsNull (ex.ParamName, "#A5");
#endif
			}

			// Capacity <= 0
			try {
				new CookieContainer (-10, 0, 100);
				Assert.Fail ("#B1");
			}
			catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
#if NET_2_0
				// The specified value must be greater than 0
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("Capacity", ex.ParamName, "#B5");
#else
				Assert.AreEqual ("Capacity", ex.Message, "#B4");
				Assert.IsNull (ex.ParamName, "#B5");
#endif
			}
		}

		[Test] // .ctor (Int32, Int32, Int32)
		public void Constructor3_MaxCookieSize_Invalid ()
		{
			CookieContainer c;

			// MaxCookieSize <= 0
			try {
				new CookieContainer (100, 50, 0);
				Assert.Fail ("#A1");
			}
			catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
#if NET_2_0
				// The specified value must be greater than 0
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.AreEqual ("MaxCookieSize", ex.ParamName, "#A4");
#else
				Assert.AreEqual ("MaxCookieSize", ex.Message, "#A4");
				Assert.IsNull (ex.ParamName, "#A5");
#endif
			}

			// MaxCookieSize <= 0
			try {
				new CookieContainer (100, 50, -4);
				Assert.Fail ("#B1");
			}
			catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
#if NET_2_0
				// The specified value must be greater than 0
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.AreEqual ("MaxCookieSize", ex.ParamName, "#B4");
#else
				Assert.AreEqual ("MaxCookieSize", ex.Message, "#B4");
				Assert.IsNull (ex.ParamName, "#B5");
#endif
			}
		}

		[Test] // .ctor (Int32, Int32, Int32)
		public void Constructor3_PerDomainCapacity_Invalid ()
		{
			CookieContainer c;

			// PerDomainCapacity <= 0
			try {
				new CookieContainer (432, 0, 1000);
				Assert.Fail ("#B1");
#if NET_2_0
			}
			catch (ArgumentOutOfRangeException ex) {
				// 'PerDomainCapacity' has to be greater than
				// '0' and less than '432'
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("perDomainCapacity", ex.ParamName, "#B5");
			}
#else
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.AreEqual ("PerDomainCapacity", ex.Message, "#B4");
				Assert.IsNull (ex.ParamName, "#B5");
			}
#endif

			// PerDomainCapacity <= 0
			try {
				new CookieContainer (432, -1, 1000);
				Assert.Fail ("#C1");
#if NET_2_0
			}
			catch (ArgumentOutOfRangeException ex) {
				// 'PerDomainCapacity' has to be greater than
				// '0' and less than '432'
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.AreEqual ("perDomainCapacity", ex.ParamName, "#C5");
			}
#else
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.AreEqual ("PerDomainCapacity", ex.Message, "#C4");
				Assert.IsNull (ex.ParamName, "#C5");
			}
#endif

			// PerDomainCapacity > Capacity (and != Int32.MaxValue)
			try {
				new CookieContainer (432, 433, 1000);
				Assert.Fail ("#C1");
#if NET_2_0
			}
			catch (ArgumentOutOfRangeException ex) {
				// 'PerDomainCapacity' has to be greater than
				// '0' and less than '432'
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.AreEqual ("perDomainCapacity", ex.ParamName, "#C5");
			}
#else
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.AreEqual ("PerDomainCapacity", ex.Message, "#C4");
				Assert.IsNull (ex.ParamName, "#C5");
			}
#endif
		}

		[Test]
		public void TestDefaultLimits ()
		{
			Assert.AreEqual (4096, CookieContainer.DefaultCookieLengthLimit, "#1");
			Assert.AreEqual (300, CookieContainer.DefaultCookieLimit, "#2");
			Assert.AreEqual (20, CookieContainer.DefaultPerDomainCookieLimit, "#3");
		}

		[Test]
		public void Capacity ()
		{
			CookieContainer c = new CookieContainer ();
			c.Capacity = c.PerDomainCapacity;
			Assert.AreEqual (c.PerDomainCapacity, c.Capacity, "#A1");
			Assert.AreEqual (CookieContainer.DefaultCookieLengthLimit, c.MaxCookieSize, "#A2");
			Assert.AreEqual (CookieContainer.DefaultPerDomainCookieLimit, c.PerDomainCapacity, "#A3");
			c.Capacity = int.MaxValue;
			Assert.AreEqual (int.MaxValue, c.Capacity, "#B1");
			Assert.AreEqual (CookieContainer.DefaultCookieLengthLimit, c.MaxCookieSize, "#B2");
			Assert.AreEqual (CookieContainer.DefaultPerDomainCookieLimit, c.PerDomainCapacity, "#B3");
			c.PerDomainCapacity = int.MaxValue;
			c.Capacity = (c.PerDomainCapacity - 1);
			Assert.AreEqual ((c.PerDomainCapacity - 1), c.Capacity, "#C1");
			Assert.AreEqual (CookieContainer.DefaultCookieLengthLimit, c.MaxCookieSize, "#C2");
			Assert.AreEqual (int.MaxValue, c.PerDomainCapacity, "#C3");
		}

		[Test]
		public void Capacity_Value_Invalid ()
		{
			CookieContainer c = new CookieContainer ();

			// Capacity <= 0
			try {
				c.Capacity = -5;
				Assert.Fail ("#A1");
			}
			catch (ArgumentOutOfRangeException ex) {
				// 'Capacity' has to be greater than '0' and
				// less than '20'
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("value", ex.ParamName, "#A5");
			}

			// Capacity <= 0
			try {
				c.Capacity = 0;
				Assert.Fail ("#B1");
			}
			catch (ArgumentOutOfRangeException ex) {
				// 'Capacity' has to be greater than '0' and
				// less than '20'
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("value", ex.ParamName, "#B5");
			}

			// Capacity < PerDomainCapacity (and PerDomainCapacity != Int32.MaxValue)
			try {
				c.Capacity = 5;
				Assert.Fail ("#C1");
			}
			catch (ArgumentOutOfRangeException ex) {
				// 'Capacity' has to be greater than '0' and
				// less than '20'
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.AreEqual ("value", ex.ParamName, "#C5");
			}
		}

		[Test]
		public void MaxCookieSize ()
		{
			CookieContainer c = new CookieContainer ();
			c.MaxCookieSize = 80000;
			Assert.AreEqual (CookieContainer.DefaultCookieLimit, c.Capacity, "#A1");
			Assert.AreEqual (80000, c.MaxCookieSize, "#A2");
			Assert.AreEqual (CookieContainer.DefaultPerDomainCookieLimit, c.PerDomainCapacity, "#A3");
			c.MaxCookieSize = int.MaxValue;
			Assert.AreEqual (CookieContainer.DefaultCookieLimit, c.Capacity, "#B1");
			Assert.AreEqual (int.MaxValue, c.MaxCookieSize, "#B2");
			Assert.AreEqual (CookieContainer.DefaultPerDomainCookieLimit, c.PerDomainCapacity, "#B3");
			c.MaxCookieSize = 1;
			Assert.AreEqual (CookieContainer.DefaultCookieLimit, c.Capacity, "#C1");
			Assert.AreEqual (1, c.MaxCookieSize, "#C2");
			Assert.AreEqual (CookieContainer.DefaultPerDomainCookieLimit, c.PerDomainCapacity, "#C3");
		}

		[Test]
		public void MaxCookieSize_Value_Invalid ()
		{
			CookieContainer c = new CookieContainer ();

			// MaxCookieSize <= 0
			try {
				c.MaxCookieSize = -5;
				Assert.Fail ("#A1");
			}
			catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("value", ex.ParamName, "#A5");
			}

			// MaxCookieSize <= 0
			try {
				c.MaxCookieSize = -1;
				Assert.Fail ("#B1");
			}
			catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("value", ex.ParamName, "#B5");
			}

			// MaxCookieSize <= 0
			try {
				c.MaxCookieSize = 0;
				Assert.Fail ("#C1");
			}
			catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.AreEqual ("value", ex.ParamName, "#C5");
			}
		}

		[Test]
		public void PerDomainCapacity ()
		{
			CookieContainer c = new CookieContainer ();
			c.PerDomainCapacity = c.Capacity;
			Assert.AreEqual (c.Capacity, c.PerDomainCapacity, "#1");
			c.PerDomainCapacity = int.MaxValue;
			Assert.AreEqual (int.MaxValue, c.PerDomainCapacity, "#2");
			c.PerDomainCapacity = c.Capacity - 5;
			Assert.AreEqual ((c.Capacity - 5), c.PerDomainCapacity, "#3");
			c.PerDomainCapacity = 1;
			Assert.AreEqual (1, c.PerDomainCapacity, "#4");
		}

		[Test]
		public void PerDomainCapacity_Value_Invalid ()
		{
			CookieContainer c = new CookieContainer ();

			// PerDomainCapacity <= 0
			try {
				c.PerDomainCapacity = -5;
				Assert.Fail ("#A1");
			}
			catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("value", ex.ParamName, "#A5");
			}

			// PerDomainCapacity <= 0
			try {
				c.PerDomainCapacity = 0;
				Assert.Fail ("#B1");
			}
			catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("value", ex.ParamName, "#B5");
			}

			c.Capacity = (c.PerDomainCapacity + 5);

			// PerDomainCapacity > Capacity (and != Int32.MaxValue)
			try {
				c.PerDomainCapacity = (c.Capacity + 1);
				Assert.Fail ("#C1");
			}
			catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.AreEqual ("value", ex.ParamName, "#C5");
			}
		}

		[Test] // Add (Cookie)
		public void Add1 ()
		{
			CookieContainer cc = new CookieContainer ();
			Cookie cookie = new Cookie ("Age", "28", string.Empty, "localhost");
			Assert.AreEqual ("Age", cookie.Name, "Name");
			Assert.AreEqual ("28", cookie.Value, "Value");
			Assert.AreEqual (String.Empty, cookie.Path, "Path");
			Assert.AreEqual ("localhost", cookie.Domain, "Domain");
			// does not survive the addition "cloning"
			cookie.Comment = "comment";
			cookie.CommentUri = new Uri ("http://localhost");
			cookie.Discard = true;
			cookie.Expires = DateTime.MaxValue;
#if NET_2_0
			cookie.HttpOnly = true;
#endif
			cookie.Secure = true;
			// except version
			cookie.Version = 1;

			cc.Add (cookie);
			Assert.AreEqual (1, cc.Count, "#A1");

			CookieCollection cookies = cc.GetCookies (new Uri ("http://localhost/Whatever"));
			Assert.AreEqual (1, cookies.Count, "#A2");
			Assert.AreNotSame (cookie, cookies [0], "!same");

			cookie = cookies [0];
			Assert.AreEqual ("Age", cookie.Name, "Clone-Name");
			Assert.AreEqual ("28", cookie.Value, "Clone-Value");
			// Path is not the same, nor default
			Assert.AreEqual ("/", cookie.Path, "Clone-Path");
			Assert.AreEqual ("localhost", cookie.Domain, "Clone-Domain");
			// other non-core properties have default values
			Assert.AreEqual (String.Empty, cookie.Comment, "Clone-Comment");
			Assert.IsNull (cookie.CommentUri, "Clone-CommentUri");
			Assert.IsFalse (cookie.Discard, "Clone-Discard");
			Assert.AreEqual (DateTime.MinValue, cookie.Expires, "Clone-Expires");
#if NET_2_0
			Assert.IsFalse (cookie.HttpOnly, "Clone-HttpOnly");
#endif
			Assert.IsFalse (cookie.Secure, "Clone-Secure");
			// except version
			Assert.AreEqual (1, cookie.Version, "Clone-Version");

			cookies = cc.GetCookies (new Uri ("http://localhost/Whatever"));
			// the same Cookie instance returned for a second query
			Assert.AreSame (cookie, cookies [0], "!same-2");
		}

		[Test] // Add (Cookie)
		public void Add1_Domain_Empty ()
		{
			CookieContainer cc = new CookieContainer ();
			Cookie cookie = new Cookie ("Age", "28");
			try {
				cc.Add (cookie);
				Assert.Fail ("#1");
			}
			catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
#if NET_2_0
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("cookie.Domain", ex.ParamName, "#5");
#else
				Assert.AreEqual ("cookie.Domain", ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
#endif
			}
		}

		[Test] // Add (CookieCollection)
		public void Add2_Cookies_Null ()
		{
			CookieContainer cc = new CookieContainer ();
			try {
				cc.Add ((CookieCollection) null);
				Assert.Fail ("#1");
			}
			catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("cookies", ex.ParamName, "#5");
			}
		}

		[Test] // Add (Uri, Cookie)
		public void Add3_Uri_Null ()
		{
			CookieContainer cc = new CookieContainer ();
			Cookie cookie = new Cookie ("Age", "28", "", "localhost");
			try {
				cc.Add ((Uri) null, cookie);
				Assert.Fail ("#1");
			}
			catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("uri", ex.ParamName, "#5");
			}
		}

		[Test] // Add (Uri, Cookie)
		public void Add3_Cookie_Null ()
		{
			CookieContainer cc = new CookieContainer ();
			Uri uri = new Uri ("http://www.contoso.com");
			try {
				cc.Add (uri, (Cookie) null);
				Assert.Fail ("#1");
			}
			catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("cookie", ex.ParamName, "#5");
			}
		}

		[Test] // Add (Uri, CookieCollection)
		public void Add4_Uri_Null ()
		{
			CookieContainer cc = new CookieContainer ();
			CookieCollection cookies = new CookieCollection ();
			try {
				cc.Add ((Uri) null, cookies);
				Assert.Fail ("#1");
			}
			catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("uri", ex.ParamName, "#5");
			}
		}

		[Test] // Add (Uri, CookieCollection)
		public void Add4_Cookie_Null ()
		{
			CookieContainer cc = new CookieContainer ();
			Uri uri = new Uri ("http://www.contoso.com");
			try {
				cc.Add (uri, (CookieCollection) null);
				Assert.Fail ("#1");
			}
			catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("cookies", ex.ParamName, "#5");
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
		//		[Category ("NotWorking")]
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
			coll = cc.GetCookies (new Uri ("http://contoso.com"));
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
		public void GetCookieHeader1 ()
		{
			CookieContainer cc;
			Cookie cookie;

			cc = new CookieContainer ();
			cookie = new Cookie ("name1", "value1", "/path", "localhost");
			cookie.Comment = "Short name";
			cookie.Expires = DateTime.Now.Add (new TimeSpan (3, 2, 5));
			cookie.Version = 0;
			cc.Add (cookie);
			cookie = new Cookie ("name2", "value2", "/path/sub", "localhost");
			cookie.Comment = "Description";
			cookie.Expires = DateTime.Now.Add (new TimeSpan (3, 2, 5));
			cookie.Version = 1;
			cc.Add (cookie);
			Assert.AreEqual ("$Version=1; name2=value2; $Path=/path/sub; name1=value1", cc.GetCookieHeader (new Uri ("http://localhost/path/sub")), "#A1");
			Assert.AreEqual ("name1=value1", cc.GetCookieHeader (new Uri ("http://localhost/path")), "#A2");
			Assert.AreEqual (string.Empty, cc.GetCookieHeader (new Uri ("http://localhost/whatever")), "#A3");
		}

		[Test]
		public void GetCookieHeader2a ()
		{
			CookieContainer cc = new CookieContainer ();
			Cookie cookie = new Cookie ("Country", "Belgium", "/path", "mono.com");
			cc.Add (cookie);
			cookie = new Cookie ("Age", "26", "/path", "dev.mono.com");
			cc.Add (cookie);

			Assert.AreEqual ("Age=26", cc.GetCookieHeader (new Uri ("http://dev.mono.com/path/ok")), "#A1");
			Assert.AreEqual ("Country=Belgium", cc.GetCookieHeader (new Uri ("http://mono.com/path")), "#A2");
			Assert.AreEqual ("", cc.GetCookieHeader (new Uri ("http://test.mono.com/path")), "#A3");
			Assert.AreEqual ("", cc.GetCookieHeader (new Uri ("http://us.dev.mono.com/path")), "#A4");
		}

		[Test]
		public void GetCookieHeader2b ()
		{
			CookieContainer cc = new CookieContainer ();
			Cookie cookie = new Cookie ("Country", "Belgium", "/path", ".mono.com");
			cc.Add (cookie);
			cookie = new Cookie ("Age", "26", "/path", ".dev.mono.com");
			cc.Add (cookie);

			Assert.AreEqual ("Country=Belgium", cc.GetCookieHeader (new Uri ("http://dev.mono.com/path/ok")), "#C1");
			Assert.AreEqual ("", cc.GetCookieHeader (new Uri ("http://mono.com/path")), "#C2");
			Assert.AreEqual ("Country=Belgium", cc.GetCookieHeader (new Uri ("http://test.mono.com/path")), "#C3");
			Assert.AreEqual ("Age=26; Country=Belgium", cc.GetCookieHeader (new Uri ("http://us.dev.mono.com/path")), "#C4");
		}

		[Test]
		public void GetCookieHeader3 ()
		{
			CookieContainer cc = new CookieContainer ();
			cc.SetCookies (new Uri ("http://dev.test.mono.com/Whatever/Do"),
				"Country=Belgium; path=/Whatever; domain=mono.com;" +
				"Age=26; path=/Whatever; domain=test.mono.com," +
				"Weight=87; path=/Whatever/Do; domain=.mono.com");
			Assert.AreEqual ("Weight=87; Country=Belgium", cc.GetCookieHeader (new Uri ("http://dev.mono.com/Whatever/Do")), "#C1");
			Assert.AreEqual ("Weight=87; Country=Belgium", cc.GetCookieHeader (new Uri ("http://test.mono.com/Whatever/Do")), "#C2");
			Assert.AreEqual ("", cc.GetCookieHeader (new Uri ("http://mono.com/Whatever/Do")), "#C3");
			Assert.AreEqual ("Weight=87; Country=Belgium", cc.GetCookieHeader (new Uri ("http://us.test.mono.com/Whatever/Do")), "#C4");
		}

		[Test]
		public void GetCookieHeader4 ()
		{
			CookieContainer cc = new CookieContainer ();
			Cookie cookie = new Cookie ("Height", "178", "/Whatever", "mono.com");
			cc.Add (cookie);
			cookie = new Cookie ("Town", "Brussels", "/Whatever", ".mono.com");
			cc.Add (cookie);
			cookie = new Cookie ("Income", "34445", "/Whatever/", ".test.mono.com");
			cc.Add (cookie);
			cookie = new Cookie ("Sex", "Male", "/WhateveR/DO", ".test.mono.com");
			cc.Add (cookie);
			cc.SetCookies (new Uri ("http://dev.test.mono.com/Whatever/Do/You"),
				"Country=Belgium," +
				"Age=26; path=/Whatever/Do; domain=test.mono.com," +
				"Weight=87; path=/");
			Assert.AreEqual ("Age=26; Income=34445; Town=Brussels",
				cc.GetCookieHeader (new Uri ("http://us.test.mono.com/Whatever/Do/Ok")),
				"#D");
		}

		[Test]
		public void GetCookieHeader5a ()
		{
			CookieContainer cc = new CookieContainer ();
			Cookie cookie = new Cookie ("name1", "value1", "", "localhost");
			cookie.Comment = "Short name";
			cookie.Expires = DateTime.Now.Add (new TimeSpan (3, 2, 5));
			cookie.Version = 1;
			cc.Add (cookie);
			Assert.AreEqual ("$Version=1; name1=value1; $Domain=localhost", cookie.ToString (), "#E0");
			Assert.AreEqual ("$Version=1; name1=value1; $Path=/",
				cc.GetCookieHeader (new Uri ("http://localhost/path/sub")), "#E1");
		}

		[Test]
		public void GetCookieHeader5b ()
		{
			CookieContainer cc = new CookieContainer ();
			Cookie cookie = new Cookie ("name1", "value1");
			cookie.Domain = "localhost";
			cookie.Comment = "Short name";
			cookie.Expires = DateTime.Now.Add (new TimeSpan (3, 2, 5));
			cookie.Version = 1;
			cc.Add (cookie);
			Assert.AreEqual ("$Version=1; name1=value1; $Domain=localhost", cookie.ToString (), "#E0");
			Assert.AreEqual ("$Version=1; name1=value1; $Path=/",
				cc.GetCookieHeader (new Uri ("http://localhost/path/sub")), "#E1");
		}

		[Test]
		public void GetCookieHeader6 ()
		{
			CookieContainer cc = new CookieContainer ();
			Cookie cookie = new Cookie ("name1", "value1", "", "localhost");
			cookie.Comment = "Short name";
			cookie.Expires = DateTime.Now.Add (new TimeSpan (3, 2, 5));
			cookie.Version = 0;
			cc.Add (cookie);
			Assert.AreEqual ("name1=value1",
				cc.GetCookieHeader (new Uri ("http://localhost/path/sub")), "#E2");
		}

		[Test]
		public void GetCookieHeader7a ()
		{
			CookieContainer cc = new CookieContainer ();
			Cookie cookie = new Cookie ("name1", "value1", "/path", ".mono.com");
			cookie.Comment = "Short name";
			cookie.Expires = DateTime.Now.Add (new TimeSpan (3, 2, 5));
			cookie.Version = 0;
			cc.Add (cookie);
			cookie = new Cookie ("name2", "value2", "/path/sub", ".mono.com");
			cookie.Comment = "Description";
			cookie.Expires = DateTime.Now.Add (new TimeSpan (3, 2, 5));
			cookie.Version = 1;
			cc.Add (cookie);
			Assert.AreEqual ("$Version=1; name2=value2; $Path=/path/sub; $Domain=.mono.com; name1=value1", cc.GetCookieHeader (new Uri ("http://live.mono.com/path/sub")), "#A1");
			Assert.AreEqual ("name1=value1", cc.GetCookieHeader (new Uri ("http://live.mono.com/path")), "#A2");
			Assert.AreEqual (string.Empty, cc.GetCookieHeader (new Uri ("http://live.mono.com/whatever")), "#A3");
			Assert.AreEqual (string.Empty, cc.GetCookieHeader (new Uri ("http://gomono.com/path/sub")), "#A4");
			Assert.AreEqual (string.Empty, cc.GetCookieHeader (new Uri ("http://mono.com/path/sub")), "#A5");
		}

		[Test]
		public void GetCookieHeader7b ()
		{
			CookieContainer cc = new CookieContainer ();
			Cookie cookie = new Cookie ("name1", "value1", "/path", "live.mono.com");
			cookie.Comment = "Short name";
			cookie.Expires = DateTime.Now.Add (new TimeSpan (3, 2, 5));
			cookie.Version = 0;
			cc.Add (cookie);
			cookie = new Cookie ("name2", "value2", "/path/sub", "live.mono.com");
			cookie.Comment = "Description";
			cookie.Expires = DateTime.Now.Add (new TimeSpan (3, 2, 5));
			cookie.Version = 1;
			cc.Add (cookie);
			Assert.AreEqual ("$Version=1; name2=value2; $Path=/path/sub; name1=value1", cc.GetCookieHeader (new Uri ("http://live.mono.com/path/sub")), "#B1");
			Assert.AreEqual ("name1=value1", cc.GetCookieHeader (new Uri ("http://live.mono.com/path")), "#B2");
			Assert.AreEqual (string.Empty, cc.GetCookieHeader (new Uri ("http://live.mono.com/whatever")), "#B3");
			Assert.AreEqual (string.Empty, cc.GetCookieHeader (new Uri ("http://go.live.mono.com/path/sub")), "#B4");
			Assert.AreEqual (string.Empty, cc.GetCookieHeader (new Uri ("http://go.live.mono.com/path")), "#B5");
			Assert.AreEqual (string.Empty, cc.GetCookieHeader (new Uri ("http://go.live.mono.com/whatever")), "#B6");
			Assert.AreEqual (string.Empty, cc.GetCookieHeader (new Uri ("http://golive.mono.com/whatever")), "#B7");
		}

		[Test]
		public void GetCookieHeader_Uri_Null ()
		{
			CookieContainer cc = new CookieContainer ();
			try {
				cc.GetCookieHeader ((Uri) null);
				Assert.Fail ("#1");
			}
			catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("uri", ex.ParamName, "#5");
			}
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
		public void GetCookies2a ()
		{
			CookieContainer container = new CookieContainer ();
			container.Add (new Cookie ("Country", "Belgium", "/path", "mono.com"));
			container.Add (new Cookie ("Age", "26", "/path", "dev.mono.com"));

			CookieCollection cookies = container.GetCookies (new Uri ("http://dev.mono.com/path/ok"));
			Assert.IsNotNull (cookies, "#G1");
			Assert.AreEqual (1, cookies.Count, "#G2");

			Cookie cookie = cookies [0];
			Assert.AreEqual ("Age", cookie.Name, "#H1");
			Assert.AreEqual ("26", cookie.Value, "#H2");
			Assert.AreEqual ("/path", cookie.Path, "#H3");
			Assert.AreEqual ("dev.mono.com", cookie.Domain, "#H4");

			cookies = container.GetCookies (new Uri ("http://mono.com/path"));
			Assert.IsNotNull (cookies, "#I1");
			Assert.AreEqual (1, cookies.Count, "#I2");

			cookie = cookies [0];
			Assert.AreEqual ("Country", cookie.Name, "#J1");
			Assert.AreEqual ("Belgium", cookie.Value, "#J2");
			Assert.AreEqual ("/path", cookie.Path, "#J3");
			Assert.AreEqual ("mono.com", cookie.Domain, "#J4");

			cookies = container.GetCookies (new Uri ("http://test.mono.com/path"));
			Assert.IsNotNull (cookies, "#K1");
			Assert.AreEqual (0, cookies.Count, "#K2");

			cookies = container.GetCookies (new Uri ("http://us.dev.mono.com/path"));
			Assert.IsNotNull (cookies, "#L1");
			Assert.AreEqual (0, cookies.Count, "#L2");
		}

		[Test]
		public void GetCookies2b ()
		{
			CookieContainer container = new CookieContainer ();
			container.SetCookies (new Uri ("http://dev.test.mono.com/Whatever/Do"),
				"Country=Belgium; path=/Whatever; domain=mono.com," +
				"Age=26; path=/Whatever; domain=test.mono.com," +
				"Weight=87; path=/Whatever/Do; domain=.mono.com;");

			CookieCollection cookies = container.GetCookies (new Uri ("http://dev.mono.com/Whatever/Do"));
			Assert.IsNotNull (cookies, "#M1");
			Assert.AreEqual (2, cookies.Count, "#M2");

			Cookie cookie = cookies [0];
			Assert.AreEqual ("Weight", cookie.Name, "#N1");
			Assert.AreEqual ("87", cookie.Value, "#N2");
			Assert.AreEqual ("/Whatever/Do", cookie.Path, "#N3");
			Assert.AreEqual (".mono.com", cookie.Domain, "#N4");
			cookie = cookies [1];
			Assert.AreEqual ("Country", cookie.Name, "#N5");
			Assert.AreEqual ("Belgium", cookie.Value, "#N6");
			Assert.AreEqual ("/Whatever", cookie.Path, "#N7");
			Assert.AreEqual ("mono.com", cookie.Domain, "#N8");

			cookies = container.GetCookies (new Uri ("http://test.mono.com/Whatever/Do"));
			Assert.IsNotNull (cookies, "#O1");
			Assert.AreEqual (2, cookies.Count, "#O2");

			cookie = cookies [0];
			Assert.AreEqual ("Weight", cookie.Name, "#P1");
			Assert.AreEqual ("87", cookie.Value, "#P2");
			Assert.AreEqual ("/Whatever/Do", cookie.Path, "#P3");
			Assert.AreEqual (".mono.com", cookie.Domain, "#P4");
			cookie = cookies [1];
			Assert.AreEqual ("Country", cookie.Name, "#P5");
			Assert.AreEqual ("Belgium", cookie.Value, "#P6");
			Assert.AreEqual ("/Whatever", cookie.Path, "#P7");
			Assert.AreEqual ("mono.com", cookie.Domain, "#P8");

			cookies = container.GetCookies (new Uri ("http://mono.com/Whatever/Do"));
			Assert.IsNotNull (cookies, "#Q1");
			Assert.AreEqual (0, cookies.Count, "#Q2");

			cookies = container.GetCookies (new Uri ("http://us.test.mono.com/Whatever/Do"));
			Assert.IsNotNull (cookies, "#R1");
			Assert.AreEqual (3, cookies.Count, "#R2");

			cookie = cookies [0];
			Assert.AreEqual ("Age", cookie.Name, "#S1");
			Assert.AreEqual ("26", cookie.Value, "#S2");
			Assert.AreEqual ("/Whatever", cookie.Path, "#S3");
			Assert.AreEqual ("test.mono.com", cookie.Domain, "#S4");
			cookie = cookies [1];
			Assert.AreEqual ("Weight", cookie.Name, "#S5");
			Assert.AreEqual ("87", cookie.Value, "#S6");
			Assert.AreEqual ("/Whatever/Do", cookie.Path, "#S7");
			Assert.AreEqual (".mono.com", cookie.Domain, "#S8");
			cookie = cookies [2];
			Assert.AreEqual ("Country", cookie.Name, "#S9");
			Assert.AreEqual ("Belgium", cookie.Value, "#S10");
			Assert.AreEqual ("/Whatever", cookie.Path, "#S11");
			Assert.AreEqual ("mono.com", cookie.Domain, "#S12");
		}

		[Test]
		public void GetCookies2c ()
		{
			CookieContainer container = new CookieContainer ();
			container.Add (new Cookie ("Height", "178", "/Whatever", "mono.com"));
			container.Add (new Cookie ("Town", "Brussels", "/Whatever", ".mono.com"));
			container.Add (new Cookie ("Income", "34445", "/Whatever/", ".test.mono.com"));
			container.Add (new Cookie ("Sex", "Male", "/WhateveR/DO", ".test.mono.com"));
			container.SetCookies (new Uri ("http://dev.test.mono.com/Whatever/Do/You"),
				"Country=Belgium," +
				"Age=26; path=/Whatever/Do; domain=test.mono.com," +
				"Weight=87; path=/");

			CookieCollection cookies = container.GetCookies (new Uri ("http://us.test.mono.com/Whatever/Do/Ok"));
			Assert.IsNotNull (cookies, "#T1");
			Assert.AreEqual (3, cookies.Count, "#T2");

			Cookie cookie = cookies [0];
			Assert.AreEqual ("Age", cookie.Name, "#U1");
			Assert.AreEqual ("26", cookie.Value, "#U2");
			Assert.AreEqual ("/Whatever/Do", cookie.Path, "#U3");
			Assert.AreEqual ("test.mono.com", cookie.Domain, "#U4");
			cookie = cookies [1];
			Assert.AreEqual ("Income", cookie.Name, "#U5");
			Assert.AreEqual ("34445", cookie.Value, "#U6");
			Assert.AreEqual ("/Whatever/", cookie.Path, "#U7");
			Assert.AreEqual (".test.mono.com", cookie.Domain, "#U8");
			cookie = cookies [2];
			Assert.AreEqual ("Town", cookie.Name, "#U9");
			Assert.AreEqual ("Brussels", cookie.Value, "#U10");
			Assert.AreEqual ("/Whatever", cookie.Path, "#U11");
			Assert.AreEqual (".mono.com", cookie.Domain, "#U12");
		}

		[Test]
		public void GetCookies_Uri_Null ()
		{
			CookieContainer cc = new CookieContainer ();
			try {
				cc.GetCookies (null);
				Assert.Fail ("#1");
			}
			catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("uri", ex.ParamName, "#5");
			}
		}

		[Test]
		public void SetCookies ()
		{
			Uri uri = new Uri ("http://dev.test.mono.com/Whatever/Do/You");

			DateTime now = DateTime.Now;

			CookieContainer cc = new CookieContainer ();
			cc.SetCookies (uri, "Country=Belgium," +
				"Age=26;   ; path=/Whatever/Do; domain=test.mono.com," +
				"Weight=87; path=/; ");
			Assert.AreEqual (3, cc.Count, "#A");

			CookieCollection cookies = cc.GetCookies (new Uri ("http://us.test.mono.com/Whatever/Do/Ok"));
			Assert.IsNotNull (cookies, "#B1");
			Assert.AreEqual (1, cookies.Count, "#B2");

			Cookie cookie = cookies [0];
			Assert.AreEqual (string.Empty, cookie.Comment, "#C:Comment");
			Assert.IsNull (cookie.CommentUri, "#C:CommentUri");
			Assert.IsFalse (cookie.Discard, "#C:Discard");
			Assert.AreEqual ("test.mono.com", cookie.Domain, "#C:Domain");
			Assert.IsFalse (cookie.Expired, "#C:Expired");
			Assert.AreEqual (DateTime.MinValue, cookie.Expires, "#C:Expires");
#if NET_2_0
			Assert.IsFalse (cookie.HttpOnly, "#C:HttpOnly");
#endif
			Assert.AreEqual ("Age", cookie.Name, "#C:Name");
			Assert.AreEqual ("/Whatever/Do", cookie.Path, "#C:Path");
			Assert.IsFalse (cookie.Secure, "#C:Secure");
			Assert.IsTrue ((cookie.TimeStamp - now).TotalMilliseconds >= 0, "#C:TimeStamp1");
			Assert.IsTrue ((cookie.TimeStamp - now).TotalMilliseconds < 1000, "#C:TimeStamp2");
			Assert.AreEqual ("26", cookie.Value, "#C:Value");
			Assert.AreEqual (0, cookie.Version, "#C:Version");

			cookies = cc.GetCookies (new Uri ("http://dev.test.mono.com/Whatever/Do/Ok"));
			Assert.IsNotNull (cookies, "#D1");
			Assert.AreEqual (2, cookies.Count, "#D2");

			// our sorting is not 100% identical to MS implementation
			for (int i = 0; i < cookies.Count; i++) {
				cookie = cookies [i];
				switch (cookie.Name) {
				case "Weight":
					Assert.AreEqual (string.Empty, cookie.Comment, "#E:Comment");
					Assert.IsNull (cookie.CommentUri, "#E:CommentUri");
					Assert.IsFalse (cookie.Discard, "#E:Discard");
					Assert.AreEqual ("dev.test.mono.com", cookie.Domain, "#E:Domain");
					Assert.IsFalse (cookie.Expired, "#E:Expired");
					Assert.AreEqual (DateTime.MinValue, cookie.Expires, "#E:Expires");
#if NET_2_0
					Assert.IsFalse (cookie.HttpOnly, "#E:HttpOnly");
#endif
					Assert.AreEqual ("Weight", cookie.Name, "#E:Name");
					Assert.AreEqual ("/", cookie.Path, "#E:Path");
					Assert.IsFalse (cookie.Secure, "#E:Secure");
					Assert.IsTrue ((cookie.TimeStamp - now).TotalMilliseconds >= 0, "#E:TimeStamp1");
					Assert.IsTrue ((cookie.TimeStamp - now).TotalMilliseconds < 1000, "#E:TimeStamp2");
					Assert.AreEqual ("87", cookie.Value, "#E:Value");
					Assert.AreEqual (0, cookie.Version, "#E:Version");
					break;
				case "Age":
					Assert.AreEqual (string.Empty, cookie.Comment, "#F:Comment");
					Assert.IsNull (cookie.CommentUri, "#F:CommentUri");
					Assert.IsFalse (cookie.Discard, "#F:Discard");
					Assert.AreEqual ("test.mono.com", cookie.Domain, "#F:Domain");
					Assert.IsFalse (cookie.Expired, "#F:Expired");
					Assert.AreEqual (DateTime.MinValue, cookie.Expires, "#F:Expires");
#if NET_2_0
					Assert.IsFalse (cookie.HttpOnly, "#F:HttpOnly");
#endif
					Assert.AreEqual ("Age", cookie.Name, "#F:Name");
					Assert.AreEqual ("/Whatever/Do", cookie.Path, "#F:Path");
					Assert.IsFalse (cookie.Secure, "#F:Secure");
					Assert.IsTrue ((cookie.TimeStamp - now).TotalMilliseconds >= 0, "#F:TimeStamp1");
					Assert.IsTrue ((cookie.TimeStamp - now).TotalMilliseconds < 1000, "#F:TimeStamp2");
					Assert.AreEqual ("26", cookie.Value, "#F:Value");
					Assert.AreEqual (0, cookie.Version, "#F:Version");
					break;
				default:
					Assert.Fail (cookie.Name);
					break;
				}
			}

			cookies = cc.GetCookies (uri);
			Assert.IsNotNull (cookies, "#G1");
			Assert.AreEqual (3, cookies.Count, "#G2");

			// our sorting is not 100% identical to MS implementation
			for (int i = 0; i < cookies.Count; i++) {
				cookie = cookies [i];
				switch (cookie.Name) {
				case "Country":
					Assert.AreEqual (string.Empty, cookie.Comment, "#H:Comment");
					Assert.IsNull (cookie.CommentUri, "#H:CommentUri");
					Assert.IsFalse (cookie.Discard, "#H:Discard");
					Assert.AreEqual ("dev.test.mono.com", cookie.Domain, "#H:Domain");
					Assert.IsFalse (cookie.Expired, "#H:Expired");
					Assert.AreEqual (DateTime.MinValue, cookie.Expires, "#H:Expires");
#if NET_2_0
					Assert.IsFalse (cookie.HttpOnly, "#H:HttpOnly");
#endif
					Assert.AreEqual ("Country", cookie.Name, "#H:Name");
					Assert.AreEqual ("/Whatever/Do/You", cookie.Path, "#H:Path");
					Assert.IsFalse (cookie.Secure, "#H:Secure");
					Assert.IsTrue ((cookie.TimeStamp - now).TotalMilliseconds >= 0, "#H:TimeStamp1");
					Assert.IsTrue ((cookie.TimeStamp - now).TotalMilliseconds < 1000, "#H:TimeStamp2");
					Assert.AreEqual ("Belgium", cookie.Value, "#H:Value");
					Assert.AreEqual (0, cookie.Version, "#H:Version");
					break;
				case "Weight":
					Assert.AreEqual (string.Empty, cookie.Comment, "#I:Comment");
					Assert.IsNull (cookie.CommentUri, "#I:CommentUri");
					Assert.IsFalse (cookie.Discard, "#I:Discard");
					Assert.AreEqual ("dev.test.mono.com", cookie.Domain, "#I:Domain");
					Assert.IsFalse (cookie.Expired, "#I:Expired");
					Assert.AreEqual (DateTime.MinValue, cookie.Expires, "#I:Expires");
#if NET_2_0
					Assert.IsFalse (cookie.HttpOnly, "#I:HttpOnly");
#endif
					Assert.AreEqual ("Weight", cookie.Name, "#I:Name");
					Assert.AreEqual ("/", cookie.Path, "#I:Path");
					Assert.IsFalse (cookie.Secure, "#I:Secure");
					Assert.IsTrue ((cookie.TimeStamp - now).TotalMilliseconds >= 0, "#I:TimeStamp1");
					Assert.IsTrue ((cookie.TimeStamp - now).TotalMilliseconds < 1000, "#I:TimeStamp2");
					Assert.AreEqual ("87", cookie.Value, "#I:Value");
					Assert.AreEqual (0, cookie.Version, "#I:Version");
					break;
				case "Age":
					Assert.AreEqual (string.Empty, cookie.Comment, "#J:Comment");
					Assert.IsNull (cookie.CommentUri, "#J:CommentUri");
					Assert.IsFalse (cookie.Discard, "#J:Discard");
					Assert.AreEqual ("test.mono.com", cookie.Domain, "#J:Domain");
					Assert.IsFalse (cookie.Expired, "#J:Expired");
					Assert.AreEqual (DateTime.MinValue, cookie.Expires, "#J:Expires");
#if NET_2_0
					Assert.IsFalse (cookie.HttpOnly, "#J:HttpOnly");
#endif
					Assert.AreEqual ("Age", cookie.Name, "#J:Name");
					Assert.AreEqual ("/Whatever/Do", cookie.Path, "#J:Path");
					Assert.IsFalse (cookie.Secure, "#J:Secure");
					Assert.IsTrue ((cookie.TimeStamp - now).TotalMilliseconds >= 0, "#J:TimeStamp1");
					Assert.IsTrue ((cookie.TimeStamp - now).TotalMilliseconds < 1000, "#J:TimeStamp2");
					Assert.AreEqual ("26", cookie.Value, "#J:Value");
					Assert.AreEqual (0, cookie.Version, "#J:Version");
					break;
				default:
					Assert.Fail (cookie.Name);
					break;
				}
			}

			cc.SetCookies (uri, "Country=,A");
			cookies = cc.GetCookies (uri);
			Assert.IsNotNull (cookies, "#K1");
			Assert.AreEqual (4, cookies.Count, "#K2");

			cc = new CookieContainer ();
			cc.SetCookies (uri, "Country=,A");
			cookies = cc.GetCookies (uri);
			Assert.IsNotNull (cookies, "#L1");
			Assert.AreEqual (2, cookies.Count, "#L2");

			// our sorting is not 100% identical to MS implementation
			for (int i = 0; i < cookies.Count; i++) {
				cookie = cookies [i];
				switch (cookie.Name) {
				case "Country":
					Assert.AreEqual (string.Empty, cookie.Comment, "#M:Comment");
					Assert.IsNull (cookie.CommentUri, "#M:CommentUri");
					Assert.IsFalse (cookie.Discard, "#M:Discard");
					Assert.AreEqual ("dev.test.mono.com", cookie.Domain, "#M:Domain");
					Assert.IsFalse (cookie.Expired, "#M:Expired");
					Assert.AreEqual (DateTime.MinValue, cookie.Expires, "#M:Expires");
#if NET_2_0
					Assert.IsFalse (cookie.HttpOnly, "#M:HttpOnly");
#endif
					Assert.AreEqual ("Country", cookie.Name, "#M:Name");
					Assert.AreEqual ("/Whatever/Do/You", cookie.Path, "#M:Path");
					Assert.IsFalse (cookie.Secure, "#M:Secure");
					Assert.IsTrue ((cookie.TimeStamp - now).TotalMilliseconds >= 0, "#M:TimeStamp1");
					Assert.IsTrue ((cookie.TimeStamp - now).TotalMilliseconds < 1000, "#M:TimeStamp2");
					Assert.AreEqual (string.Empty, cookie.Value, "#M:Value");
					Assert.AreEqual (0, cookie.Version, "#M:Version");
					break;
				case "A":
					Assert.AreEqual (string.Empty, cookie.Comment, "#N:Comment");
					Assert.IsNull (cookie.CommentUri, "#N:CommentUri");
					Assert.IsFalse (cookie.Discard, "#N:Discard");
					Assert.AreEqual ("dev.test.mono.com", cookie.Domain, "#N:Domain");
					Assert.IsFalse (cookie.Expired, "#N:Expired");
					Assert.AreEqual (DateTime.MinValue, cookie.Expires, "#N:Expires");
#if NET_2_0
					Assert.IsFalse (cookie.HttpOnly, "#N:HttpOnly");
#endif
					Assert.AreEqual ("A", cookie.Name, "#N:Name");
					Assert.AreEqual ("/Whatever/Do/You", cookie.Path, "#N:Path");
					Assert.IsFalse (cookie.Secure, "#N:Secure");
					Assert.IsTrue ((cookie.TimeStamp - now).TotalMilliseconds >= 0, "#N:TimeStamp1");
					Assert.IsTrue ((cookie.TimeStamp - now).TotalMilliseconds < 1000, "#N:TimeStamp2");
					Assert.AreEqual (string.Empty, cookie.Value, "#N:Value");
					Assert.AreEqual (0, cookie.Version, "#N:Version");
					break;
				default:
					Assert.Fail (cookie.Name);
					break;
				}
			}
		}

		[Test]
		public void SetCookies_CookieHeader_Empty ()
		{
			CookieContainer cc = new CookieContainer ();
			cc.SetCookies (new Uri ("http://www.contoso.com"), string.Empty);
			Assert.AreEqual (0, cc.Count);
		}

		[Test]
		public void SetCookies_CookieHeader_Null ()
		{
			CookieContainer cc = new CookieContainer ();
			try {
				cc.SetCookies (new Uri ("http://www.contoso.com"), null);
				Assert.Fail ("#1");
			}
			catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("cookieHeader", ex.ParamName, "#5");
			}
		}

		[Test]
		public void SetCookies_CookieHeader_Invalid_1 ()
		{
			// cookie format error
			CookieContainer cc = new CookieContainer ();
			try {
				cc.SetCookies (new Uri ("http://www.contoso.com"), "=lalala");
				Assert.Fail ("#A1");
			}
			catch (CookieException ex) {
				// An error has occurred when parsing Cookie
				// header for Uri 'http://www.contoso.com/'
				Assert.AreEqual (typeof (CookieException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("'http://www.contoso.com/'") != -1, "#A5");

				// Cookie format error
				CookieException inner = ex.InnerException as CookieException;
				Assert.AreEqual (typeof (CookieException), inner.GetType (), "#A6");
				Assert.IsNull (inner.InnerException, "#A7");
				Assert.IsNotNull (inner.Message, "#A8");
			}
		}

		[Test]
		public void SetCookies_CookieHeader_Invalid_2 ()
		{
			// cookie path not part of URI path
			CookieContainer cc = new CookieContainer ();
			try {
				cc.SetCookies (new Uri ("http://dev.test.mono.com/Whatever"),
					"Age=26; path=/Whatever/Do; domain=test.mono.com");
				Assert.Fail ("#B1");
			}
			catch (CookieException ex) {
				// An error has occurred when parsing Cookie
				// header for Uri 'http://dev.test.mono.com/Whatever'
				Assert.AreEqual (typeof (CookieException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("'http://dev.test.mono.com/Whatever'") != -1, "#B5");

				// The 'Path'='/Whatever/Do' part of the Cookie
				// is invalid
				CookieException inner = ex.InnerException as CookieException;
				Assert.AreEqual (typeof (CookieException), inner.GetType (), "#B6");
				Assert.IsNull (inner.InnerException, "#B7");
				Assert.IsNotNull (inner.Message, "#B8");
				Assert.IsTrue (inner.Message.IndexOf ("'Path'='/Whatever/Do'") != -1, "#B9");
			}
		}

		[Test]
		public void SetCookies_Uri_Null ()
		{
			CookieContainer cc = new CookieContainer ();
			try {
				cc.SetCookies (null, "Age=26; path=/Whatever; domain=test.mono.com");
				Assert.Fail ("#1");
			}
			catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("uri", ex.ParamName, "#5");
			}
		}

		[Test]
#if !NET_2_0
		[Category ("NotWorking")] // old 1.x behavior is not implemented
#endif
		public void SetCookies_DomainMatchesHost ()
		{
			CookieContainer cc = new CookieContainer ();
#if NET_2_0
			// domains looks identical - but "domain=test.mono.com" means "*.test.mono.com"
			cc.SetCookies (new Uri ("http://test.mono.com/Whatever/Do"),
				"Age=26; path=/Whatever; domain=test.mono.com");
			CookieCollection cookies = cc.GetCookies (new Uri ("http://test.mono.com/Whatever/Do"));
			Assert.IsNotNull (cookies, "#A1");
			Assert.AreEqual (0, cookies.Count, "#A2");
			cookies = cc.GetCookies (new Uri ("http://us.test.mono.com/Whatever/Do"));
			Assert.IsNotNull (cookies, "#A3");
			Assert.AreEqual (1, cookies.Count, "#A4");
#else
			try {
				cc.SetCookies (new Uri ("http://test.mono.com/Whatever/Do"),
					"Age=26; path=/Whatever; domain=test.mono.com");
				Assert.Fail ("#A1");
			} catch (CookieException ex) {
				// An error has occurred when parsing Cookie
				// header for Uri 'http://test.mono.com/Whatever/Do'
				Assert.AreEqual (typeof (CookieException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("'http://test.mono.com/Whatever/Do'") != -1, "#A5");

				// The 'Domain'='test.mono.com' part of the
				// Cookie is invalid
				CookieException inner = ex.InnerException as CookieException;
				Assert.AreEqual (typeof (CookieException), inner.GetType (), "#A6");
				Assert.IsNull (inner.InnerException, "#A7");
				Assert.IsNotNull (inner.Message, "#A8");
				Assert.IsTrue (inner.Message.IndexOf ("'Domain'='test.mono.com'") != -1, "#A9");
			}
#endif
		}

		[Test]
		public void SetCookies_Domain_Local ()
		{
			CookieContainer cc;
			CookieCollection cookies;

			cc = new CookieContainer ();
			cc.SetCookies (new Uri ("http://localhost/Whatever/Do"),
				"Age=26; path=/Whatever; domain=.local");
			cookies = cc.GetCookies (new Uri ("http://localhost/Whatever/Do"));
			Assert.IsNotNull (cookies, "#A1");
			Assert.AreEqual (0, cookies.Count, "#A2");
			cookies = cc.GetCookies (new Uri ("http://127.0.0.1/Whatever/Do"));
			Assert.IsNotNull (cookies, "#A3");
			Assert.AreEqual (0, cookies.Count, "#A4");
			cookies = cc.GetCookies (new Uri ("http://" + Dns.GetHostName () + "/Whatever/Do"));
			Assert.IsNotNull (cookies, "#A5");
			Assert.AreEqual (0, cookies.Count, "#A6");

			cc = new CookieContainer ();
			cc.SetCookies (new Uri ("http://127.0.0.1/Whatever/Do"),
				"Age=26; path=/Whatever; domain=.local");
			cookies = cc.GetCookies (new Uri ("http://localhost/Whatever/Do"));
			Assert.IsNotNull (cookies, "#B1");
			Assert.AreEqual (0, cookies.Count, "#B2");
			cookies = cc.GetCookies (new Uri ("http://127.0.0.1/Whatever/Do"));
			Assert.IsNotNull (cookies, "#B3");
			Assert.AreEqual (0, cookies.Count, "#B4");
			cookies = cc.GetCookies (new Uri ("http://" + Dns.GetHostName () + "/Whatever/Do"));
			Assert.IsNotNull (cookies, "#B5");
			Assert.AreEqual (0, cookies.Count, "#B6");

			cc = new CookieContainer ();
			cc.SetCookies (new Uri ("http://" + Dns.GetHostName () + "/Whatever/Do"),
				"Age=26; path=/Whatever; domain=.local");
			cookies = cc.GetCookies (new Uri ("http://localhost/Whatever/Do"));
			Assert.IsNotNull (cookies, "#C1");
			Assert.AreEqual (0, cookies.Count, "#C2");
			cookies = cc.GetCookies (new Uri ("http://127.0.0.1/Whatever/Do"));
			Assert.IsNotNull (cookies, "#C3");
			Assert.AreEqual (0, cookies.Count, "#C4");
			cookies = cc.GetCookies (new Uri ("http://" + Dns.GetHostName () + "/Whatever/Do"));
			Assert.IsNotNull (cookies, "#C5");
			Assert.AreEqual (0, cookies.Count, "#C6");
		}

		[Test]
		public void bug421827 ()
		{
			CookieContainer container;
			CookieCollection cookies;
			Cookie cookie;

			container = new CookieContainer ();
			container.SetCookies (new Uri ("http://test.mono.com/Whatever/Do"),
				"Country=Belgium; path=/Whatever; domain=mono.com");
			cookies = container.GetCookies (new Uri ("http://dev.mono.com/Whatever"));

			Assert.AreEqual (1, cookies.Count, "#A1");
			cookie = cookies [0];
			Assert.AreEqual ("Country", cookie.Name, "#A2");
			Assert.AreEqual ("mono.com", cookie.Domain, "#A3");
			Assert.AreEqual ("/Whatever", cookie.Path, "#A4");
			Assert.AreEqual (0, cookie.Version, "#A5");

			container = new CookieContainer ();
			container.SetCookies (new Uri ("http://sub.mono.com/Whatever/Do"),
				"Country=Belgium; path=/Whatever; domain=mono.com");
			cookies = container.GetCookies (new Uri ("http://gomono.com/Whatever"));

			Assert.AreEqual (0, cookies.Count, "#B");

			container = new CookieContainer ();
			container.SetCookies (new Uri ("http://sub.mono.com/Whatever/Do"),
				"Country=Belgium; path=/Whatever; domain=mono.com");
			cookies = container.GetCookies (new Uri ("http://amono.com/Whatever"));

			Assert.AreEqual (0, cookies.Count, "#C");
		}

		[Test]
		public void MoreThanDefaultDomainCookieLimit ()
		{
			CookieContainer cc = new CookieContainer ();
			for (int i = 1; i <= CookieContainer.DefaultPerDomainCookieLimit; i++) {
				Cookie c = new Cookie (i.ToString (), i.ToString (), "/", "mono.com");
				cc.Add (c);
			}
			Assert.AreEqual (CookieContainer.DefaultPerDomainCookieLimit, cc.Count, "Count");
			Cookie c2 = new Cookie ("uho", "21", "/", "mono.com");
			cc.Add (c2);
			Assert.AreEqual (CookieContainer.DefaultPerDomainCookieLimit, cc.Count, "Count");
			// so one (yes '1' ;-) was removed
		}

		[Test]
		public void MoreThanDefaultCookieLimit ()
		{
			CookieContainer cc = new CookieContainer ();
			for (int i = 1; i <= CookieContainer.DefaultCookieLimit; i++) {
				Cookie c = new Cookie (i.ToString (), i.ToString (), "/", "www" + i.ToString () + ".mono.com");
				cc.Add (c);
			}
			Assert.AreEqual (CookieContainer.DefaultCookieLimit, cc.Count, "Count");
			Cookie c2 = new Cookie ("uho", "301", "/", "www301.mono.com");
			cc.Add (c2);
			Assert.AreEqual (CookieContainer.DefaultCookieLimit, cc.Count, "Count");
			// so one (yes '1' ;-) was removed
		}

		[Test]
		public void SaveAndLoadViaAddUriCookie ()
		{
			Cookie cookie = new Cookie ("name", "value")
			{
				Domain = ".example.com",
#if NET_2_0
				Expires = new DateTime (2015, 1, 1, 0, 0, 0, DateTimeKind.Utc),
				HttpOnly = true,
#else
				Expires = new DateTime (2015, 1, 1, 0, 0, 0).ToUniversalTime (),
#endif
				Secure = true,
			};

			Uri uri = new Uri ("https://www.example.com/path/file");
			CookieContainer container = new CookieContainer ();
			container.Add (uri, cookie);
			CookieCollection collection = container.GetCookies (uri);
			Assert.AreEqual (collection.Count, 1, "#A1");
			Cookie cloned = collection [0];
			
			Assert.AreEqual (cookie.Comment, cloned.Comment, "#A2");
			Assert.AreEqual (cookie.CommentUri, cloned.CommentUri, "#A3");
			Assert.AreEqual (cookie.Domain, cloned.Domain, "#A4");
			Assert.AreEqual (cookie.Discard, cloned.Discard, "#A5");
			Assert.AreEqual (cookie.Expired, cloned.Expired, "#A6");
			Assert.AreEqual (cookie.Expires.ToUniversalTime (), cloned.Expires.ToUniversalTime (), "#A7");
#if NET_2_0
			Assert.AreEqual (cookie.HttpOnly, cloned.HttpOnly, "#A8");
#endif
			Assert.AreEqual (cookie.Name, cloned.Name, "#A9");
			Assert.AreEqual (cookie.Path, cloned.Path, "#A10");
			Assert.AreEqual (cookie.Port, cloned.Port, "#A11");
			Assert.AreEqual (cookie.Value, cloned.Value, "#A12");
			Assert.AreEqual (cookie.Version, cloned.Version, "#A13");
			Assert.AreEqual (cookie.Secure, cloned.Secure, "#A14");
		}

		[Test]
		public void SaveAndLoadViaSetCookies ()
		{
			Cookie cookie = new Cookie ("name", "value")
			{
				Domain = ".example.com",
#if NET_2_0
				Expires = new DateTime (2015, 1, 1, 0, 0, 0, DateTimeKind.Utc),
				HttpOnly = true,
#else
				Expires = new DateTime (2015, 1, 1, 0, 0, 0).ToUniversalTime (),
#endif
				Secure = true,
			};

			Uri uri = new Uri ("https://www.example.com/path/file");
			CookieContainer container = new CookieContainer ();
			container.SetCookies (uri, "name=value; domain=.example.com; expires=Thu, 01-Jan-2015 00:00:00 GMT; HttpOnly; secure");
			CookieCollection collection = container.GetCookies (uri);
			Assert.AreEqual (collection.Count, 1, "#A1");
			Cookie cloned = collection [0];
			
			Assert.AreEqual (cookie.Comment, cloned.Comment, "#A2");
			Assert.AreEqual (cookie.CommentUri, cloned.CommentUri, "#A3");
			Assert.AreEqual (cookie.Domain, cloned.Domain, "#A4");
			Assert.AreEqual (cookie.Discard, cloned.Discard, "#A5");
			Assert.AreEqual (cookie.Expired, cloned.Expired, "#A6");
			Assert.AreEqual (cookie.Expires.ToUniversalTime (), cloned.Expires.ToUniversalTime (), "#A7");
#if NET_2_0
			Assert.AreEqual (cookie.HttpOnly, cloned.HttpOnly, "#A8");
#endif
			Assert.AreEqual (cookie.Name, cloned.Name, "#A9");
			Assert.AreEqual (cookie.Path, "", "#A10");
			Assert.AreEqual (cloned.Path, "/path/file", "#A11");
			Assert.AreEqual (cookie.Port, cloned.Port, "#A12");
			Assert.AreEqual (cookie.Value, cloned.Value, "#A13");
			Assert.AreEqual (cookie.Version, cloned.Version, "#A14");
			Assert.AreEqual (cookie.Secure, cloned.Secure, "#A15");
		}
	}
}
