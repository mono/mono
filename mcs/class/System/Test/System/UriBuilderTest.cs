//
// UriBuilderTest.cs - NUnit Test Cases for System.UriBuilder
//
// Authors:
//   Lawrence Pit (loz@cable.a2000.nl)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using NUnit.Framework;
using System;

namespace MonoTests.System
{
	[TestFixture]
	public class UriBuilderTest
	{
		private UriBuilder b, b2, b3;
		
		[SetUp]
		public void GetReady()
		{
			b = new UriBuilder ("http", "www.ximian.com", 80, "foo/bar/index.html");
		}

		[Test] // ctor ()
		public void Constructor_Empty ()
		{
			b = new UriBuilder ();
			Assert.AreEqual ("http", b.Scheme, "#1");
#if NET_2_0
			Assert.AreEqual ("localhost", b.Host, "#2");
#else
			Assert.AreEqual ("loopback", b.Host, "#3");
#endif
			Assert.AreEqual (-1, b.Port, "#4");
			Assert.AreEqual (string.Empty, b.Query, "#5");
			Assert.AreEqual (string.Empty, b.Fragment, "#6");
		}

		[Test] // ctor (string)
		public void Constructor1 ()
		{
			b = new UriBuilder ("http://www.ximian.com:8001#test?name=50");
			Assert.AreEqual ("#test?name=50", b.Fragment, "#A1");
			Assert.AreEqual ("www.ximian.com", b.Host, "#A2");
			Assert.AreEqual (string.Empty, b.Password, "#A3");
			Assert.AreEqual ("/", b.Path, "#A4");
			Assert.AreEqual (8001, b.Port, "#A5");
			Assert.AreEqual (string.Empty, b.Query, "#A5");
			Assert.AreEqual ("http", b.Scheme, "#A6");
			Assert.AreEqual ("http://www.ximian.com:8001/#test?name=50", b.Uri.ToString (), "#A7");
			Assert.AreEqual (string.Empty, b.UserName, "#A8");

			b = new UriBuilder ("http://www.ximian.com?name=50#test");
			Assert.AreEqual ("#test", b.Fragment, "#B1");
			Assert.AreEqual ("www.ximian.com", b.Host, "#B2");
			Assert.AreEqual (string.Empty, b.Password, "#B3");
			Assert.AreEqual ("/", b.Path, "#B4");
			Assert.AreEqual (80, b.Port, "#B5");
			Assert.AreEqual ("?name=50", b.Query, "#B5");
			Assert.AreEqual ("http", b.Scheme, "#B6");
#if NET_2_0
			// our 1.0 behavior matches that of .NET 2.0
			Assert.AreEqual ("http://www.ximian.com/?name=50#test", b.Uri.ToString (), "#B7");
#endif
			Assert.AreEqual (string.Empty, b.UserName, "#B8");
		}

		[Test] // ctor (string)
#if ONLY_1_1
		[Category ("NotWorking")] // we always throw an ArgumentNullException
#endif
		public void Constructor1_Uri_Null ()
		{
			try {
				new UriBuilder ((string) null);
				Assert.Fail ("#1");
#if NET_2_0
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("uriString", ex.ParamName, "#6");
			}
#else
			} catch (NullReferenceException) {
			}
#endif
		}

		[Test] // ctor (Uri)
		[ExpectedException (typeof (NullReferenceException))]
		public void Constructor2_Uri_Null ()
		{
			new UriBuilder ((Uri) null);
		}

		[Test]
		public void Constructor_5 ()
		{
			b = new UriBuilder ("http", "www.ximian.com", 80, "foo/bar/index.html", "#extras");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Constructor_5_BadExtraValue ()
		{
			b = new UriBuilder ("http", "www.ximian.com", 80, "foo/bar/index.html", "extras");
			// should have thrown an ArgumentException because extraValue must start with '?' or '#' character.
		}
		
		[Test]
		// This test does not make sense, will fix soon
		[Category ("NotWorking")] // bug #75144
		public void UserInfo ()
		{
			b = new UriBuilder ("mailto://myname:mypwd@contoso.com?subject=hello");
#if NET_2_0
			Assert.AreEqual (string.Empty, b.UserName, "#1");
			Assert.AreEqual (string.Empty, b.Password, "#2");
#else
			// NotWorking here for 1.x (bad behaviour in 1.x - may not be worth fixing)
			Assert.AreEqual ("myname", b.UserName, "#1");
			Assert.AreEqual ("mypwd", b.Password, "#2");
#endif			
			b = new UriBuilder ("mailto", "contoso.com");
			b.UserName = "myname";
			b.Password = "mypwd";
			// NotWorking here for 2.0 - worth fixing
			Assert.AreEqual ("myname:mypwd", b.Uri.UserInfo, "#3");
		}

		[Test]
		public void Path ()
		{
			b.Path = ((char) 0xa9) + " 2002";
			Assert.AreEqual ("%C2%A9%202002", b.Path);
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void BadPort1 ()
		{
			b.Port = -12345;
		}
#if NET_2_0
		[Test]
		public void DefaultPort ()
		{
			b.Port = -1;
			Assert.AreEqual (-1, b.Port, "#1");
			Assert.AreEqual ("http://www.ximian.com/foo/bar/index.html", b.ToString (), "#2");
		}
#else
		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void BadPort3 ()
		{
			b.Port = -1;
		}
#endif
		[Test]
		public void Query ()
		{
			b.Query = ((char) 0xa9) + " 2002";
			Assert.AreEqual ("?\xA9 2002", b.Query, "#1");
			Assert.AreEqual (string.Empty, b.Fragment, "#2");
			b.Query = "?test";
			Assert.AreEqual ("??test", b.Query, "#3");
			b.Query = null;
			Assert.AreEqual (string.Empty, b.Query, "#4");
			b.Fragment = "test";
			Assert.AreEqual ("#test", b.Fragment, "#5");
			b.Query = "name";
#if NET_2_0
			Assert.AreEqual ("#test", b.Fragment, "#6");
#else
			Assert.AreEqual (string.Empty, b.Fragment, "#6");
#endif
			Assert.AreEqual ("?name", b.Query, "#7");
			b.Fragment = "run";
			Assert.AreEqual ("#run", b.Fragment, "#8");
			b.Query = null;
#if NET_2_0
			Assert.AreEqual ("#run", b.Fragment, "#9");
#else
			Assert.AreEqual (string.Empty, b.Fragment, "#9");
#endif
			Assert.AreEqual (string.Empty, b.Query, "#10");
		}
		
		[Test]
		public void Fragment ()
		{
			b.Fragment = ((char) 0xa9) + " 2002";
			Assert.AreEqual ("#\xA9 2002", b.Fragment, "#1");
			Assert.AreEqual (string.Empty, b.Query, "#2");
			b.Fragment = "#test";
			Assert.AreEqual ("##test", b.Fragment, "#3");
			b.Fragment = null;
			Assert.AreEqual (String.Empty, b.Fragment, "#4");
			b.Query = "name";
			Assert.AreEqual ("?name", b.Query, "#5");
			b.Fragment = null;
#if NET_2_0
			Assert.AreEqual ("?name", b.Query, "#6");
#else
			Assert.AreEqual (string.Empty, b.Query, "#6");
#endif
			Assert.AreEqual (string.Empty, b.Fragment, "#7");
		}
		
		[Test]
		public void Scheme ()
		{
			b.Scheme = "http";
			Assert.AreEqual ("http", b.Scheme, "#1");
			b.Scheme = "http:";
			Assert.AreEqual ("http", b.Scheme, "#2");
			b.Scheme = "http://";
			Assert.AreEqual ("http", b.Scheme, "#3");
			b.Scheme = "http://foo/bar";
			Assert.AreEqual ("http", b.Scheme, "#4");
			b.Scheme = "mailto:";
			Assert.AreEqual ("mailto", b.Scheme, "#5");
			b.Scheme = "unknown";
			Assert.AreEqual ("unknown", b.Scheme, "#6");
			b.Scheme = "unknown://";
			Assert.AreEqual ("unknown", b.Scheme, "#7");
		}
		
		[Test]
#if NET_2_0
		[Category ("NotWorking")] // equals changed in 2.0
#endif
		public void Equals ()
		{
			b = new UriBuilder ("http://", "www.ximian.com", 80, "foo/bar/index.html?item=1");
			b2 = new UriBuilder ("http", "www.ximian.com", 80, "/foo/bar/index.html", "?item=1");
			b3 = new UriBuilder (new Uri ("http://www.ximian.com/foo/bar/index.html?item=1"));
#if NET_2_0
			Assert.IsFalse (b.Equals (b2), "#1");
			Assert.IsFalse (b.Uri.Equals (b2.Uri), "#2");
			Assert.IsFalse (b.Equals (b3), "#3");
			Assert.IsFalse (b3.Equals (b), "#4");
#else
			Assert.IsTrue (b.Equals (b2), "#1");
			Assert.IsTrue (b.Uri.Equals (b2.Uri), "#2");
			Assert.IsTrue (b.Equals (b3), "#3");
			Assert.IsTrue (b3.Equals (b), "#4");
#endif
			Assert.IsTrue (b2.Equals (b3), "#5");
		}
		
		[Test]
		public void ToStringTest ()
		{
			Assert.AreEqual ("http://www.ximian.com:80/foo/bar/index.html", b.ToString (), "#1");
			Assert.AreEqual ("http://www.ximian.com/foo/bar/index.html", b.Uri.ToString (), "#2");
		}

		[Test]
		public void EmptyQuery () // bug 57082
		{
			b = new UriBuilder ("http", "www.ximian.com", 80, "/lalala/lelele.aspx", null);
			string noquery = "http://www.ximian.com/lalala/lelele.aspx";
			Assert.AreEqual (noquery, b.Uri.ToString (), "#1");
			b = new UriBuilder ("http", "www.ximian.com", 80, "/lalala/lelele.aspx", "?");
			Assert.AreEqual (noquery, b.Uri.ToString (), "#2");
			b = new UriBuilder ("http", "www.ximian.com", 80, "/lalala/lelele.aspx", "??");
			Assert.AreEqual (noquery + "??", b.Uri.ToString (), "#3");
			b = new UriBuilder ("http", "www.ximian.com", 80, "/lalala/lelele.aspx", "?something");
			Assert.AreEqual (noquery + "?something", b.Uri.ToString (), "#4");
		}

		[Test] // bug #76501
		public void TestToString76501 ()
		{
			UriBuilder ub = new UriBuilder (
				"http://mondomaine/trucmuche/login.aspx");
			ub.Query = ub.Query.TrimStart (new char [] {'?'}) + "&ticket=bla";
			Assert.IsTrue (ub.ToString ().IndexOf ("80//") < 0);
		}

		[Test]
		public void TestAppendFragment ()
		{
			UriBuilder uri = new UriBuilder ("http://www.mono-project.com/Main_Page");
			uri.Fragment = "Features";
			Assert.AreEqual ("#Features", uri.Fragment, "#1");
			Assert.AreEqual ("http://www.mono-project.com/Main_Page#Features", uri.Uri.ToString (), "#2");
		}
	}
}

