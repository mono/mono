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
		public void Constructors ()
		{
			b = new UriBuilder ();
			AssertEquals ("#1", "http", b.Scheme);
			AssertEquals ("#2", "loopback", b.Host);
			AssertEquals ("#3", -1, b.Port);
			
			try {
				b = new UriBuilder ("http", "www.ximian.com", 80, "foo/bar/index.html", "extras");
				Fail ("#4 should have thrown an ArgumentException because extraValue must start with '?' or '#' character.");
			} catch (ArgumentException) {}
			
			b = new UriBuilder ("http", "www.ximian.com", 80, "foo/bar/index.html", "#extras");
		}
		
		[Test]
		public void UserInfo ()
		{			
			b = new UriBuilder ("mailto://myname:mypwd@contoso.com?subject=hello");
			AssertEquals ("#1", "myname", b.UserName);
			AssertEquals ("#2", "mypwd", b.Password);
			
			b = new UriBuilder ("mailto", "contoso.com");
			b.UserName = "myname";
			b.Password = "mypwd";
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

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void BadPort3 ()
		{
			b.Port = -1;
		}

		[Test]
		public void Query ()
		{
			b.Query = ((char) 0xa9) + " 2002";
			AssertEquals ("#1", "?© 2002", b.Query);			
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
			AssertEquals ("#1", "#© 2002", b.Fragment);
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
		public void Equals ()
		{
			b = new UriBuilder ("http://", "www.ximian.com", 80, "foo/bar/index.html?item=1");
			b2 = new UriBuilder ("http", "www.ximian.com", 80, "/foo/bar/index.html", "?item=1");
			b3 = new UriBuilder (new Uri ("http://www.ximian.com/foo/bar/index.html?item=1"));
			
			Assert ("#1", b.Equals (b2));
			Assert ("#2", b.Uri.Equals (b2.Uri));
			Assert ("#3", b.Equals (b3));
			Assert ("#4", b2.Equals (b3));
			Assert ("#5", b3.Equals (b));
		}
		
		[Test]
		public void ToStringTest ()
		{
			AssertEquals ("ToString ()", "http://www.ximian.com:80/foo/bar/index.html", b.ToString ());
			AssertEquals ("Uri.ToString ()", "http://www.ximian.com/foo/bar/index.html", b.Uri.ToString ());
		}
	}
}

