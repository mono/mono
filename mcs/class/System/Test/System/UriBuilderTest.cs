//
// UriBuilderTest.cs - NUnit Test Cases for System.UriBuilder
//
// Authors:
//   Lawrence Pit (loz@cable.a2000.nl)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
			Assert.AreEqual ("localhost", b.Host, "#2");
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
			Assert.AreEqual ("http://www.ximian.com/?name=50#test", b.Uri.ToString (), "#B7");
			Assert.AreEqual (string.Empty, b.UserName, "#B8");
		}

		[Test] // ctor (string)
		public void Constructor1_Uri_Null ()
		{
			try {
				new UriBuilder ((string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("uriString", ex.ParamName, "#6");
			}
		}

		[Test] // ctor (Uri)
#if NET_4_0
		[ExpectedException (typeof (ArgumentNullException))]
#else
		[ExpectedException (typeof (NullReferenceException))]
#endif
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
		[ExpectedException (typeof (InvalidOperationException))]
		public void Constructor_RelativeUri ()
		{
			Uri relative = new Uri ("../dir/subdir/file", UriKind.RelativeOrAbsolute);
			UriBuilder ub = new UriBuilder (relative);
		}

		[Test]
		public void UserInfo ()
		{
			string s = "mailto://myname:mypwd@contoso.com?subject=hello";
			b = new UriBuilder (s);
			Assert.AreEqual (s, b.ToString (), "1.ToString");
			Assert.AreEqual (string.Empty, b.UserName, "1.UserName");
			Assert.AreEqual (string.Empty, b.Password, "1.Password");
			Assert.AreEqual ("//myname:mypwd@contoso.com", b.Uri.LocalPath, "1.Uri.LocalPath");

			// weird ?caching? issue, UserInfo is not updated if we look at the value of UserName before setting Password
			b = new UriBuilder ("mailto", "contoso.com");
			b.UserName = "myname";
			Assert.AreEqual ("myname", b.Uri.UserInfo, "2.UserName");
			b.Password = "mypwd";
			Assert.AreEqual ("myname", b.Uri.UserInfo, "2.Password");
			Assert.AreEqual ("/", b.Uri.LocalPath, "2.Uri.LocalPath");

			b = new UriBuilder ("mailto", "contoso.com");
			b.UserName = "myname";
			b.Password = "mypwd";
			Assert.AreEqual ("myname:mypwd", b.Uri.UserInfo, "3.Uri.UserInfo");
			Assert.AreEqual ("/", b.Uri.LocalPath, "3.Uri.LocalPath");
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

		[Test]
		public void DefaultPort ()
		{
			b.Port = -1;
			Assert.AreEqual (-1, b.Port, "#1");
			Assert.AreEqual ("http://www.ximian.com/foo/bar/index.html", b.ToString (), "#2");
		}

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
			Assert.AreEqual ("#test", b.Fragment, "#6");
			Assert.AreEqual ("?name", b.Query, "#7");
			b.Fragment = "run";
			Assert.AreEqual ("#run", b.Fragment, "#8");
			b.Query = null;
			Assert.AreEqual ("#run", b.Fragment, "#9");
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
			Assert.AreEqual ("?name", b.Query, "#6");
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
		public void Equals ()
		{
			b = new UriBuilder ("http://", "www.ximian.com", 80, "foo/bar/index.html?item=1");
			Assert.AreEqual ("foo/bar/index.html%3Fitem=1", b.Path, "1.Path");
			Assert.AreEqual ("http://www.ximian.com:80/foo/bar/index.html%3Fitem=1", b.ToString (), "1.ToString");

			b2 = new UriBuilder ("http", "www.ximian.com", 80, "/foo/bar/index.html", "?item=1");
			Assert.AreEqual ("http://www.ximian.com:80/foo/bar/index.html?item=1", b2.ToString (), "2.ToString");

			b3 = new UriBuilder (new Uri ("http://www.ximian.com/foo/bar/index.html?item=1"));
			Assert.AreEqual ("http://www.ximian.com:80/foo/bar/index.html?item=1", b3.ToString (), "3.ToString");

			Assert.IsFalse (b.Equals (b2), "#1");
			Assert.IsFalse (b.Uri.Equals (b2.Uri), "#2");
			Assert.IsFalse (b.Equals (b3), "#3");
			Assert.IsFalse (b3.Equals (b), "#4");
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

		[Test]
		public void IPv6_Host ()
		{
			UriBuilder ub = new UriBuilder ("http", "[1:2:3:4:5:6:7:8]", 8080, "/dir/subdir/file");
			Assert.AreEqual ("[1:2:3:4:5:6:7:8]", ub.Host, "Host.1");
			Assert.AreEqual ("[0001:0002:0003:0004:0005:0006:0007:0008]", ub.Uri.Host, "Uri.Host");
			// once the Uri is created then some builder properties may change
			Assert.AreEqual ("[0001:0002:0003:0004:0005:0006:0007:0008]", ub.Host, "Host.2");
		}

		[Test]
		public void IPv6_Host_IncompleteAddress ()
		{
			UriBuilder ub = new UriBuilder ("http", "1:2:3:4:5:6:7:8", 8080, "/dir/subdir/file");
			Assert.AreEqual ("[1:2:3:4:5:6:7:8]", ub.Host, "1.Host");
			Assert.AreEqual ("http://[1:2:3:4:5:6:7:8]:8080/dir/subdir/file", ub.ToString (), "1.ToString ()");

			ub = new UriBuilder ("http", "1:", 8080, "/dir/subdir/file");
			Assert.AreEqual ("[1:]", ub.Host, "2.Host");
			Assert.AreEqual ("http://[1:]:8080/dir/subdir/file", ub.ToString (), "2.ToString ()");

			ub = new UriBuilder ("http", "[1:", 8080, "/dir/subdir/file");
			Assert.AreEqual ("[1:", ub.Host, "3.Host");
			Assert.AreEqual ("http://[1::8080/dir/subdir/file", ub.ToString (), "3.ToString ()");

			ub = new UriBuilder ("http", "1:2]", 8080, "/dir/subdir/file");
			Assert.AreEqual ("[1:2]]", ub.Host, "4.Host");
			Assert.AreEqual ("http://[1:2]]:8080/dir/subdir/file", ub.ToString (), "4.ToString ()");
		}

		[Test]
		public void Path_UriAbsolutePath_Path ()
		{
			UriBuilder ub = new UriBuilder ("http", "127.0.0.1", 80, "dir/subdir/file");
			Assert.AreEqual ("dir/subdir/file", ub.Path, "Path.1");
			Assert.AreEqual ("/dir/subdir/file", ub.Uri.AbsolutePath, "Uri.AbsolutePath");
			// once the Uri is created then some builder properties may change
			Assert.AreEqual ("/dir/subdir/file", ub.Path, "Path.2");
		}

		[Test]
		public void UnparsableUri ()
		{
			// some URI can't be parsed by System.Uri but are accepted by UriBuilder
			Uri u = null;
			string uri = "www.mono-project.com";
			Assert.IsFalse (Uri.TryCreate (uri, UriKind.Absolute, out u), "1.Uri.TryCreate");
			UriBuilder ub = new UriBuilder (uri);
			Assert.AreEqual ("www.mono-project.com", ub.Host, "1.Host");
			Assert.AreEqual ("http", ub.Scheme, "1.Scheme");
			Assert.AreEqual (80, ub.Port, "1.Port");
			Assert.AreEqual ("/", ub.Path, "1.Path");

			// always assume http, port 80
			uri = "ftp.novell.com/dir/subdir/file";
			ub = new UriBuilder (uri);
			Assert.IsFalse (Uri.TryCreate (uri, UriKind.Absolute, out u), "2.Uri.TryCreate");
			Assert.AreEqual ("ftp.novell.com", ub.Host, "2.Host");
			Assert.AreEqual ("http", ub.Scheme, "2.Scheme");
			Assert.AreEqual (80, ub.Port, "2.Port");
			Assert.AreEqual ("/dir/subdir/file", ub.Path, "2.Path");
		}
	}
}

