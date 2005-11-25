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
	public class UriBuilderTest : Assertion
	{
		private UriBuilder b, b2, b3;
		
		[SetUp]
		public void GetReady()
		{
			b = new UriBuilder ("http", "www.ximian.com", 80, "foo/bar/index.html");
		}

		[Test]
		public void Constructor_Empty ()
		{
			b = new UriBuilder ();
			AssertEquals ("#1", "http", b.Scheme);
#if NET_2_0
			AssertEquals ("#2", "localhost", b.Host);
#else
			AssertEquals ("#2", "loopback", b.Host);
#endif
			AssertEquals ("#3", -1, b.Port);
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
			AssertEquals ("#1", String.Empty, b.UserName);
			AssertEquals ("#2", String.Empty, b.Password);
#else
			// NotWorking here for 1.x (bad behaviour in 1.x - may not be worth fixing)
			AssertEquals ("#1", "myname", b.UserName);
			AssertEquals ("#2", "mypwd", b.Password);
#endif			
			b = new UriBuilder ("mailto", "contoso.com");
			b.UserName = "myname";
			b.Password = "mypwd";
			// NotWorking here for 2.0 - worth fixing
			AssertEquals ("#3", "myname:mypwd", b.Uri.UserInfo);
		}

		[Test]
		public void Path ()
		{			
			b.Path = ((char) 0xa9) + " 2002";
			AssertEquals ("#1", "%C2%A9%202002", b.Path);			
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
			AssertEquals ("Port", -1, b.Port);
			AssertEquals ("ToString", "http://www.ximian.com/foo/bar/index.html", b.ToString ());
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
			AssertEquals ("#1", "?\xA9 2002", b.Query);			
			AssertEquals ("#2", String.Empty, b.Fragment);
			b.Query = "?test";
			AssertEquals ("#3", "??test", b.Query);
			b.Query = null;
			AssertEquals ("#4", String.Empty, b.Query);
		}
		
		[Test]
		public void Fragment ()
		{
			b.Fragment = ((char) 0xa9) + " 2002";
			AssertEquals ("#1", "#\xA9 2002", b.Fragment);
			AssertEquals ("#2", String.Empty, b.Query);
			b.Fragment = "#test";
			AssertEquals ("#3", "##test", b.Fragment);
			b.Fragment = null;
			AssertEquals ("#4", String.Empty, b.Fragment);
		}
		
		[Test]
		public void Scheme ()
		{
			b.Scheme = "http";
			AssertEquals ("#1", b.Scheme, "http");
			b.Scheme = "http:";
			AssertEquals ("#2", b.Scheme, "http");
			b.Scheme = "http://";
			AssertEquals ("#3", b.Scheme, "http");
			b.Scheme = "http://foo/bar";
			AssertEquals ("#4", b.Scheme, "http");
			b.Scheme = "mailto:";
			AssertEquals ("#5", b.Scheme, "mailto");
			b.Scheme = "unknown";
			AssertEquals ("#6", b.Scheme, "unknown");
			b.Scheme = "unknown://";
			AssertEquals ("#7", b.Scheme, "unknown");
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
			Assert ("#1", !b.Equals (b2));
			Assert ("#2", !b.Uri.Equals (b2.Uri));
			Assert ("#3", !b.Equals (b3));
			Assert ("#5", !b3.Equals (b));
#else
			Assert ("#1", b.Equals (b2));
			Assert ("#2", b.Uri.Equals (b2.Uri));
			Assert ("#3", b.Equals (b3));
			Assert ("#5", b3.Equals (b));
#endif
			Assert ("#4", b2.Equals (b3));
		}
		
		[Test]
		public void ToStringTest ()
		{
			AssertEquals ("ToString ()", "http://www.ximian.com:80/foo/bar/index.html", b.ToString ());
			AssertEquals ("Uri.ToString ()", "http://www.ximian.com/foo/bar/index.html", b.Uri.ToString ());
		}

		[Test]
		public void EmptyQuery () // bug 57082
		{
			b = new UriBuilder ("http", "www.ximian.com", 80, "/lalala/lelele.aspx", null);
			string noquery = "http://www.ximian.com/lalala/lelele.aspx";
			AssertEquals ("#01", b.Uri.ToString (), noquery);
			b = new UriBuilder ("http", "www.ximian.com", 80, "/lalala/lelele.aspx", "?");
			AssertEquals ("#02", b.Uri.ToString (), noquery);
			b = new UriBuilder ("http", "www.ximian.com", 80, "/lalala/lelele.aspx", "??");
			AssertEquals ("#03", b.Uri.ToString (), noquery + "??");
			b = new UriBuilder ("http", "www.ximian.com", 80, "/lalala/lelele.aspx", "?something");
			AssertEquals ("#04", b.Uri.ToString (), noquery + "?something");
		}

		[Test] // bug #76501
		public void TestToString76501 ()
		{
			UriBuilder ub = new UriBuilder (
				"http://mondomaine/trucmuche/login.aspx");
			ub.Query = ub.Query.TrimStart (new char [] {'?'}) + "&ticket=bla";
			Assert (ub.ToString ().IndexOf ("80//") < 0);
		}
	}
}

