//
// UriBuilderTest.cs - NUnit Test Cases for System.UriBuilder
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using NUnit.Framework;
using System;

namespace MonoTests.System
{

	public class UriBuilderTest : TestCase
	{
		private UriBuilder b, b2, b3;
		
		public UriBuilderTest () :
			base ("[MonoTests.System.UriBuilderTest]") {}

		public UriBuilderTest (string name) : base (name) {}

		protected override void SetUp () 
		{
			b = new UriBuilder ("http://", "www.ximian.com", 80, "foo/bar/index.html");
		}

		protected override void TearDown () {}

		public static ITest Suite
		{
			get {
				return new TestSuite (typeof (UriBuilderTest));
			}
		}
		
		public void TestConstructors ()
		{
			b = new UriBuilder ();
			AssertEquals ("#1", "http", b.Scheme);
			AssertEquals ("#2", "loopback", b.Host);
			AssertEquals ("#3", -1, b.Port);
			
			try {
				b = new UriBuilder ("http://", "www.ximian.com", 80, "foo/bar/index.html", "extras");
				Fail ("#4 should have thrown an ArgumentException because extraValue must start with '?' or '#' character.");
			} catch (ArgumentException) {}
			
			b = new UriBuilder ("http://", "www.ximian.com", 80, "foo/bar/index.html", "#extras");
		}
		
		public void TestUserInfo ()
		{			
			b = new UriBuilder ("mailto://myname:mypwd@contoso.com?subject=hello");
			AssertEquals ("#1", "myname", b.UserName);
			AssertEquals ("#2", "mypwd", b.Password);
			
			b = new UriBuilder ("mailto:", "contoso.com");
			b.UserName = "myname";
			b.Password = "mypwd";
			AssertEquals ("#3: known to fail with ms.net.", "myname:mypwd", b.Uri.UserInfo);
		}

		public void TestPath ()
		{			
			b.Path = ((char) 0xa9) + " 2002";
			AssertEquals ("#1: known to fail with ms.net, should at least return a slash.", "/%A9%202002", b.Path);			
		}	
		
		public void TestPort ()
		{
			try {
				b.Port = -12345;
				Fail ("#1 should've failed, illegal port.");
			} catch (ArgumentOutOfRangeException) {}
			try {
				b.Port = 123456789;
				Fail ("#2 should've failed, illegal port.");
			} catch (ArgumentOutOfRangeException) {}
			try {
				b.Port = -1;
				AssertEquals ("#3", -1, b.Port);
			} catch (ArgumentOutOfRangeException) {
				Fail ("#4: spec should allow -1 as value.");
			}
		}
		
		public void TestQuery ()
		{
			b.Query = ((char) 0xa9) + " 2002";
			AssertEquals ("#1: known to fail with ms.net, should've been escaped.", "?%A9%202002", b.Query);			
			AssertEquals ("#2", String.Empty, b.Fragment);
			b.Query = "?test";
			AssertEquals ("#3", "??test", b.Query);
			b.Query = null;
			AssertEquals ("#4", String.Empty, b.Query);
		}
		
		public void TestFragment ()
		{
			b.Fragment = ((char) 0xa9) + " 2002";
			AssertEquals ("#1: known to fail with ms.net, should've been escaped.", "#%A9%202002", b.Fragment);
			AssertEquals ("#2", String.Empty, b.Query);
			b.Fragment = "#test";
			AssertEquals ("#3", "##test", b.Fragment);
			b.Fragment = null;
			AssertEquals ("#4", String.Empty, b.Fragment);
		}
		
		public void TestScheme ()
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
		
		public void TestEquals ()
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
		
		public void TestToString ()
		{
			AssertEquals ("#1 known to fail with ms.net, should've been canonicalized.", b.Uri.ToString (), b.ToString ());
		}
	}
}

