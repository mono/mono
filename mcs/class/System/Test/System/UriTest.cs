//
// UriTest.cs - NUnit Test Cases for System.Uri
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using NUnit.Framework;
using System;
using System.IO;

namespace MonoTests.System
{

	public class UriTest : TestCase
	{
		public UriTest () :
			base ("[MonoTests.System.UriTest]") {}

		public UriTest (string name) : base (name) {}

		protected override void SetUp () {}

		protected override void TearDown () {}

		public static ITest Suite
		{
			get {
				return new TestSuite (typeof (UriTest));
			}
		}
		
		public void TestConstructors ()
		{
			Uri uri = null;
			
			/*
			uri = new Uri ("http://www.ximian.com/foo" + ((char) 0xa9) + "/bar/index.cgi?a=1&b=" + ((char) 0xa9) + "left#fragm?ent2");
			Print (uri);

			uri = new Uri ("http://www.ximian.com/foo/xxx\"()-._;<=>@{|}~-,.`_^]\\[xx/" + ((char) 0xa9) + "/bar/index.cgi#fra+\">=@[gg]~gment2");
			Print (uri);

			uri = new Uri("http://11.22.33.588:9090");
			Print (uri);

			uri = new Uri("http://[11:22:33::88]:9090");
			Print (uri);
			
			uri = new Uri("http://[::127.11.22.33]:8080");
			Print (uri);
			
			uri = new Uri("http://[abcde::127.11.22.33]:8080");
			Print (uri);			
			*/
			
			/*
			uri = new Uri ("http://www.contoso.com:1234/foo/bar/");
			Print (uri);

			uri = new Uri ("http://www.contoso.com:1234/foo/bar");
			Print (uri);
			
			uri = new Uri ("http://www.contoso.com:1234/");
			Print (uri);

			uri = new Uri ("http://www.contoso.com:1234");
			Print (uri);
			*/
			
			uri = new Uri ("http://contoso.com?subject=uri");
			AssertEquals ("#k1", "/", uri.AbsolutePath);
			AssertEquals ("#k2", "http://contoso.com/?subject=uri", uri.AbsoluteUri);
			AssertEquals ("#k3", "contoso.com", uri.Authority);
			AssertEquals ("#k4", "", uri.Fragment);
			AssertEquals ("#k5", "contoso.com", uri.Host);
			AssertEquals ("#k6", UriHostNameType.Dns, uri.HostNameType);
			AssertEquals ("#k7", true, uri.IsDefaultPort);
			AssertEquals ("#k8", false, uri.IsFile);
			AssertEquals ("#k9", false, uri.IsLoopback);
			AssertEquals ("#k10", false, uri.IsUnc);
			AssertEquals ("#k11", "/", uri.LocalPath);
			AssertEquals ("#k12", "/?subject=uri", uri.PathAndQuery);
			AssertEquals ("#k13", 80, uri.Port);
			AssertEquals ("#k14", "?subject=uri", uri.Query);
			AssertEquals ("#k15", "http", uri.Scheme);
			AssertEquals ("#k16", false, uri.UserEscaped);
			AssertEquals ("#k17", "", uri.UserInfo);

			uri = new Uri ("mailto:user:pwd@contoso.com?subject=uri");
			AssertEquals ("#m1", "", uri.AbsolutePath);
			AssertEquals ("#m2", "mailto:user:pwd@contoso.com?subject=uri", uri.AbsoluteUri);
			AssertEquals ("#m3", "contoso.com", uri.Authority);
			AssertEquals ("#m4", "", uri.Fragment);
			AssertEquals ("#m5", "contoso.com", uri.Host);
			AssertEquals ("#m6", UriHostNameType.Dns, uri.HostNameType);
			AssertEquals ("#m7", true, uri.IsDefaultPort);
			AssertEquals ("#m8", false, uri.IsFile);
			AssertEquals ("#m9", false, uri.IsLoopback);
			AssertEquals ("#m10", false, uri.IsUnc);
			AssertEquals ("#m11", "", uri.LocalPath);
			AssertEquals ("#m12", "?subject=uri", uri.PathAndQuery);
			AssertEquals ("#m13", 25, uri.Port);
			AssertEquals ("#m14", "?subject=uri", uri.Query);
			AssertEquals ("#m15", "mailto", uri.Scheme);
			AssertEquals ("#m16", false, uri.UserEscaped);
			AssertEquals ("#m17", "user:pwd", uri.UserInfo);
			
			uri = new Uri (@"\\myserver\mydir\mysubdir\myfile.ext");
			AssertEquals ("#n1", "/mydir/mysubdir/myfile.ext", uri.AbsolutePath);
			AssertEquals ("#n2", "file://myserver/mydir/mysubdir/myfile.ext", uri.AbsoluteUri);
			AssertEquals ("#n3", "myserver", uri.Authority);
			AssertEquals ("#n4", "", uri.Fragment);
			AssertEquals ("#n5", "myserver", uri.Host);
			AssertEquals ("#n6", UriHostNameType.Dns, uri.HostNameType);
			AssertEquals ("#n7", true, uri.IsDefaultPort);
			AssertEquals ("#n8", true, uri.IsFile);
			AssertEquals ("#n9", false, uri.IsLoopback);
			AssertEquals ("#n10", true, uri.IsUnc);
			AssertEquals ("#n11", @"\\myserver\mydir\mysubdir\myfile.ext", uri.LocalPath);
			AssertEquals ("#n12", "/mydir/mysubdir/myfile.ext", uri.PathAndQuery);
			AssertEquals ("#n13", -1, uri.Port);
			AssertEquals ("#n14", "", uri.Query);
			AssertEquals ("#n15", "file", uri.Scheme);
			AssertEquals ("#n16", false, uri.UserEscaped);
			AssertEquals ("#n17", "", uri.UserInfo);

			try {
				uri = new Uri ("http://www.contoso.com:12345678/foo/bar/");
				Fail ("#o1 should have failed because of invalid port");
			} catch (UriFormatException) { }			
		}
		
		public void TestLocalPath ()
		{
			bool isWin32 = (Path.DirectorySeparatorChar == '\\');
			
			Uri uri = new Uri ("c:\\tmp\\hello.txt");
			AssertEquals ("#1a", "file:///c:/tmp/hello.txt", uri.ToString ());
			AssertEquals ("#1b", "c:\\tmp\\hello.txt", uri.LocalPath);
			AssertEquals ("#1c", "file", uri.Scheme);
			AssertEquals ("#1d", "", uri.Host);
			AssertEquals ("#1e", "c:/tmp/hello.txt", uri.AbsolutePath);
					
			uri = new Uri ("file:////////cygwin/tmp/hello.txt");
			AssertEquals ("#3a", "file://cygwin/tmp/hello.txt", uri.ToString ());
			if (isWin32)
				AssertEquals ("#3b win32", "\\\\cygwin\\tmp\\hello.txt", uri.LocalPath);
			else
				AssertEquals ("#3b *nix", "//cygwin/tmp/hello.txt", uri.LocalPath);
			AssertEquals ("#3c", "file", uri.Scheme);
			AssertEquals ("#3d", "cygwin", uri.Host);
			AssertEquals ("#3e", "/tmp/hello.txt", uri.AbsolutePath);

			uri = new Uri ("file://mymachine/cygwin/tmp/hello.txt");
			AssertEquals ("#4a", "file://mymachine/cygwin/tmp/hello.txt", uri.ToString ());
			if (isWin32)
				AssertEquals ("#4b win32", "\\\\mymachine\\cygwin\\tmp\\hello.txt", uri.LocalPath);
			else
				AssertEquals ("#4b *nix", "//mymachine/cygwin/tmp/hello.txt", uri.LocalPath);
			AssertEquals ("#4c", "file", uri.Scheme);
			AssertEquals ("#4d", "mymachine", uri.Host);
			AssertEquals ("#4e", "/cygwin/tmp/hello.txt", uri.AbsolutePath);
			
			uri = new Uri ("file://///c:/cygwin/tmp/hello.txt");
			AssertEquals ("#5a", "file:///c:/cygwin/tmp/hello.txt", uri.ToString ());
			AssertEquals ("#5b", "c:\\cygwin\\tmp\\hello.txt", uri.LocalPath);
			AssertEquals ("#5c", "file", uri.Scheme);
			AssertEquals ("#5d", "", uri.Host);
			AssertEquals ("#5e", "c:/cygwin/tmp/hello.txt", uri.AbsolutePath);			
		}
		
		public void TestUnc ()
		{
			Uri uri = new Uri ("http://www.contoso.com");
			Assert ("#1", !uri.IsUnc);
			
			uri = new Uri ("news:123456@contoso.com");
			Assert ("#2", !uri.IsUnc);

			uri = new Uri ("file://server/filename.ext");
			Assert ("#3", uri.IsUnc);

			try {
				uri = new Uri ("file:/filename.ext");
				Assert ("#4", uri.IsUnc);
			} catch (UriFormatException) {
				Fail ("#5: known to fail with ms.net");
			}			

			uri = new Uri (@"\\server\share\filename.ext");			
			Assert ("#6", uri.IsUnc);
		}
		
		public void TestFromHex () 
		{
			AssertEquals ("#1", 0, Uri.FromHex ('0'));
			AssertEquals ("#2", 9, Uri.FromHex ('9'));
			AssertEquals ("#3", 10, Uri.FromHex ('a'));
			AssertEquals ("#4", 15, Uri.FromHex ('f'));
			AssertEquals ("#5", 10, Uri.FromHex ('A'));
			AssertEquals ("#6", 15, Uri.FromHex ('F'));
			try {
				Uri.FromHex ('G');
				Fail ("#7");
			} catch (ArgumentException) {}
			try {
				Uri.FromHex (' ');
				Fail ("#8");
			} catch (ArgumentException) {}
			try {
				Uri.FromHex ('%');
				Fail ("#8");
			} catch (ArgumentException) {}
		}

		public void TestHexEscape () 
		{
			AssertEquals ("#1","%20", Uri.HexEscape (' ')); 
			AssertEquals ("#2","%A9", Uri.HexEscape ((char) 0xa9)); 
			AssertEquals ("#3","%41", Uri.HexEscape ('A')); 
			try {
				Uri.HexEscape ((char) 0x0369);
				Fail ("#4");
			} catch (ArgumentOutOfRangeException) {}
		}

		public void TestHexUnescape () 
		{
			int i = 0;
			AssertEquals ("#1", ' ', Uri.HexUnescape ("%20", ref i));
			AssertEquals ("#2", 3, i);
			i = 4;
			AssertEquals ("#3", (char) 0xa9, Uri.HexUnescape ("test%a9test", ref i));
			AssertEquals ("#4", 7, i);
			AssertEquals ("#5", 't', Uri.HexUnescape ("test%a9test", ref i));
			AssertEquals ("#6", 8, i);
			i = 4;
			AssertEquals ("#5", '%', Uri.HexUnescape ("test%a", ref i));
			AssertEquals ("#6", 5, i);
			AssertEquals ("#7", '%', Uri.HexUnescape ("testx%xx", ref i));
			AssertEquals ("#8", 6, i);
		}

		public void TestIsHexDigit () 
		{
			Assert ("#1", Uri.IsHexDigit ('a'));	
			Assert ("#2", Uri.IsHexDigit ('f'));
			Assert ("#3", !Uri.IsHexDigit ('g'));
			Assert ("#4", Uri.IsHexDigit ('0'));
			Assert ("#5", Uri.IsHexDigit ('9'));
			Assert ("#6", Uri.IsHexDigit ('A'));
			Assert ("#7", Uri.IsHexDigit ('F'));
			Assert ("#8", !Uri.IsHexDigit ('G'));
		}

		public void TestIsHexEncoding () 
		{
			Assert ("#1", Uri.IsHexEncoding ("test%a9test", 4));
			Assert ("#2", !Uri.IsHexEncoding ("test%a9test", 3));
			Assert ("#3", Uri.IsHexEncoding ("test%a9", 4));
			Assert ("#4", !Uri.IsHexEncoding ("test%a", 4));
		}
		
		public void TestGetLeftPart ()
		{
			Uri uri = new Uri ("http://www.contoso.com/index.htm#main");
			AssertEquals ("#1", "http://", uri.GetLeftPart (UriPartial.Scheme));
			AssertEquals ("#2", "http://www.contoso.com", uri.GetLeftPart (UriPartial.Authority));
			AssertEquals ("#3", "http://www.contoso.com/index.htm", uri.GetLeftPart (UriPartial.Path));
			
			uri = new Uri ("mailto:user@contoso.com?subject=uri");
			AssertEquals ("#4", "mailto:", uri.GetLeftPart (UriPartial.Scheme));
			AssertEquals ("#5", "", uri.GetLeftPart (UriPartial.Authority));
			AssertEquals ("#6", "mailto:user@contoso.com", uri.GetLeftPart (UriPartial.Path));

			uri = new Uri ("nntp://news.contoso.com/123456@contoso.com");
			AssertEquals ("#7", "nntp://", uri.GetLeftPart (UriPartial.Scheme));
			AssertEquals ("#8", "nntp://news.contoso.com", uri.GetLeftPart (UriPartial.Authority));
			AssertEquals ("#9", "nntp://news.contoso.com/123456@contoso.com", uri.GetLeftPart (UriPartial.Path));			
			
			uri = new Uri ("news:123456@contoso.com");
			AssertEquals ("#10", "news:", uri.GetLeftPart (UriPartial.Scheme));
			AssertEquals ("#11", "", uri.GetLeftPart (UriPartial.Authority));
			AssertEquals ("#12", "news:123456@contoso.com", uri.GetLeftPart (UriPartial.Path));			

			uri = new Uri ("file://server/filename.ext");
			AssertEquals ("#13", "file://", uri.GetLeftPart (UriPartial.Scheme));
			AssertEquals ("#14", "file://server", uri.GetLeftPart (UriPartial.Authority));
			AssertEquals ("#15", "file://server/filename.ext", uri.GetLeftPart (UriPartial.Path));			

			try {
				uri = new Uri ("file:/filename.ext");
				AssertEquals ("#16", "file://", uri.GetLeftPart (UriPartial.Scheme));
				AssertEquals ("#17", "", uri.GetLeftPart (UriPartial.Authority));
				AssertEquals ("#18", "file:///filename.ext", uri.GetLeftPart (UriPartial.Path));			
			} catch (UriFormatException) {
				Fail ("#19: known to fail with ms.net (it's their own example!)");
			}			

			uri = new Uri (@"\\server\share\filename.ext");
			AssertEquals ("#20", "file://", uri.GetLeftPart (UriPartial.Scheme));
			AssertEquals ("#21", "file://server", uri.GetLeftPart (UriPartial.Authority));
			AssertEquals ("#22", "file://server/share/filename.ext", uri.GetLeftPart (UriPartial.Path));
		}
		
		public void TestCheckHostName ()
		{
			AssertEquals ("#1", UriHostNameType.Unknown, Uri.CheckHostName (null));
			AssertEquals ("#2", UriHostNameType.Unknown, Uri.CheckHostName (""));
			AssertEquals ("#3", UriHostNameType.Unknown, Uri.CheckHostName ("^&()~`!@"));
			AssertEquals ("#4", UriHostNameType.Dns, Uri.CheckHostName ("x"));
			AssertEquals ("#5", UriHostNameType.IPv4, Uri.CheckHostName ("1.2.3.4"));
			AssertEquals ("#6", UriHostNameType.IPv4, Uri.CheckHostName ("0001.002.03.4"));
			AssertEquals ("#7", UriHostNameType.Dns, Uri.CheckHostName ("0001.002.03.256"));
			AssertEquals ("#8", UriHostNameType.Dns, Uri.CheckHostName ("9001.002.03.4"));
			AssertEquals ("#9", UriHostNameType.Dns, Uri.CheckHostName ("www.contoso.com"));
			AssertEquals ("#10", UriHostNameType.Unknown, Uri.CheckHostName (".www.contoso.com"));
			AssertEquals ("#11: known to fail with ms.net: this is not a valid domain address", UriHostNameType.Unknown, Uri.CheckHostName ("www.contoso.com."));	
			AssertEquals ("#12", UriHostNameType.Dns, Uri.CheckHostName ("www.con-toso.com"));	
			AssertEquals ("#13", UriHostNameType.Dns, Uri.CheckHostName ("www.con_toso.com"));	
			AssertEquals ("#14", UriHostNameType.Unknown, Uri.CheckHostName ("www.con,toso.com"));	
			
			// test IPv6
			AssertEquals ("#15", UriHostNameType.IPv6, Uri.CheckHostName ("11:22:33:44:55:66:77:88"));
			AssertEquals ("#16", UriHostNameType.IPv6, Uri.CheckHostName ("11::33:44:55:66:77:88"));
			AssertEquals ("#17", UriHostNameType.IPv6, Uri.CheckHostName ("::22:33:44:55:66:77:88"));
			AssertEquals ("#18", UriHostNameType.IPv6, Uri.CheckHostName ("11:22:33:44:55:66:77::"));
			AssertEquals ("#19", UriHostNameType.IPv6, Uri.CheckHostName ("11::88"));
			AssertEquals ("#20", UriHostNameType.IPv6, Uri.CheckHostName ("11::77:88"));
			AssertEquals ("#21", UriHostNameType.IPv6, Uri.CheckHostName ("11:22::88"));
			AssertEquals ("#22", UriHostNameType.IPv6, Uri.CheckHostName ("11::"));
			AssertEquals ("#23", UriHostNameType.IPv6, Uri.CheckHostName ("::88"));
			AssertEquals ("#24", UriHostNameType.IPv6, Uri.CheckHostName ("::1"));
			AssertEquals ("#25", UriHostNameType.IPv6, Uri.CheckHostName ("::"));
			AssertEquals ("#26", UriHostNameType.IPv6, Uri.CheckHostName ("0:0:0:0:0:0:127.0.0.1"));
			AssertEquals ("#27", UriHostNameType.IPv6, Uri.CheckHostName ("::127.0.0.1"));
			AssertEquals ("#28", UriHostNameType.IPv6, Uri.CheckHostName ("::ffFF:169.32.14.5"));
			AssertEquals ("#29", UriHostNameType.IPv6, Uri.CheckHostName ("2001:03A0::/35"));
			AssertEquals ("#30", UriHostNameType.IPv6, Uri.CheckHostName ("[2001:03A0::/35]"));

			AssertEquals ("#31", UriHostNameType.Unknown, Uri.CheckHostName ("2001::03A0::/35"));
			AssertEquals ("#32", UriHostNameType.Unknown, Uri.CheckHostName ("2001:03A0::/35a"));
			AssertEquals ("#33 known to fail with ms.net: this is not a valid IPv6 address.", UriHostNameType.Unknown, Uri.CheckHostName ("2001::03A0:1.2.3.4"));
			AssertEquals ("#34", UriHostNameType.Unknown, Uri.CheckHostName ("::ffff:123.256.155.43"));
			AssertEquals ("#35", UriHostNameType.Unknown, Uri.CheckHostName (":127.0.0.1"));
			AssertEquals ("#36 known to fail with ms.net: this is not a valid IPv6 address.", UriHostNameType.Unknown, Uri.CheckHostName (":11:22:33:44:55:66:77:88"));
			AssertEquals ("#37", UriHostNameType.Unknown, Uri.CheckHostName ("::11:22:33:44:55:66:77:88"));
			AssertEquals ("#38", UriHostNameType.Unknown, Uri.CheckHostName ("11:22:33:44:55:66:77:88::"));
			AssertEquals ("#39", UriHostNameType.Unknown, Uri.CheckHostName ("11:22:33:44:55:66:77:88:"));
			AssertEquals ("#40", UriHostNameType.Unknown, Uri.CheckHostName ("::acbde"));
			AssertEquals ("#41", UriHostNameType.Unknown, Uri.CheckHostName ("::abce:"));
			AssertEquals ("#42", UriHostNameType.Unknown, Uri.CheckHostName ("::abcg"));
			AssertEquals ("#43", UriHostNameType.Unknown, Uri.CheckHostName (":::"));
			AssertEquals ("#44", UriHostNameType.Unknown, Uri.CheckHostName (":"));
		}
		
		public void TestIsLoopback ()
		{
			Uri uri = new Uri("http://loopback:8080");
			AssertEquals ("#1", true, uri.IsLoopback);
			uri = new Uri("http://localhost:8080");
			AssertEquals ("#2", true, uri.IsLoopback);
			uri = new Uri("http://127.0.0.1:8080");
			AssertEquals ("#3", true, uri.IsLoopback);
			uri = new Uri("http://127.0.0.001:8080");
			AssertEquals ("#4", true, uri.IsLoopback);
			uri = new Uri("http://[::1]");
			AssertEquals ("#5", true, uri.IsLoopback);
			uri = new Uri("http://[::1]:8080");
			AssertEquals ("#6", true, uri.IsLoopback);
			uri = new Uri("http://[::0001]:8080");
			AssertEquals ("#7", true, uri.IsLoopback);
			uri = new Uri("http://[0:0:0:0::1]:8080");
			AssertEquals ("#8", true, uri.IsLoopback);
			uri = new Uri("http://[0:0:0:0::127.0.0.1]:8080");
			AssertEquals ("#9", true, uri.IsLoopback);
			uri = new Uri("http://[0:0:0:0::127.11.22.33]:8080");
			AssertEquals ("#10: known to fail with ms.net", true, uri.IsLoopback);
			uri = new Uri("http://[::ffff:127.11.22.33]:8080");
			AssertEquals ("#11: known to fail with ms.net", true, uri.IsLoopback);
			uri = new Uri("http://[::ff00:7f11:2233]:8080");
			AssertEquals ("#12", false, uri.IsLoopback);
			uri = new Uri("http://[1:0:0:0::1]:8080");
			AssertEquals ("#13", false, uri.IsLoopback);
		}
		
		public void TestEquals ()
		{
			Uri uri1 = new Uri ("http://www.contoso.com/index.htm#main");
			Uri uri2 = new Uri ("http://www.contoso.com/index.htm#fragment");
			Assert ("#1", uri1.Equals (uri2));
			uri2 = new Uri ("http://www.contoso.com/index.htm?x=1");
			Assert ("#2 known to fail with ms.net", !uri1.Equals (uri2));
			Assert ("#3", !uri2.Equals ("http://www.contoso.com/index.html?x=1"));
			Assert ("#4: known to fail with ms.net", !uri1.Equals ("http://www.contoso.com:8080/index.htm?x=1"));
		}
		
		public void TestGetHashCode () 
		{
			Uri uri1 = new Uri ("http://www.contoso.com/index.htm#main");
			Uri uri2 = new Uri ("http://www.contoso.com/index.htm#fragment");
			AssertEquals ("#1", uri1.GetHashCode (), uri2.GetHashCode ());
			uri2 = new Uri ("http://www.contoso.com/index.htm?x=1");
			Assert ("#2", uri1.GetHashCode () != uri2.GetHashCode ());
			uri2 = new Uri ("http://www.contoso.com:80/index.htm");
			AssertEquals ("#3", uri1.GetHashCode (), uri2.GetHashCode ());			
			uri2 = new Uri ("http://www.contoso.com:8080/index.htm");
			Assert ("#4", uri1.GetHashCode () != uri2.GetHashCode ());			
		}

		public static void Print (Uri uri)
		{
			Console.WriteLine ("ToString: " + uri.ToString ());	

			Console.WriteLine ("AbsolutePath: " + uri.AbsolutePath);
			Console.WriteLine ("AbsoluteUri: " + uri.AbsoluteUri);
			Console.WriteLine ("Authority: " + uri.Authority);
			Console.WriteLine ("Fragment: " + uri.Fragment);
			Console.WriteLine ("Host: " + uri.Host);
			Console.WriteLine ("HostNameType: " + uri.HostNameType);
			Console.WriteLine ("IsDefaultPort: " + uri.IsDefaultPort);
			Console.WriteLine ("IsFile: " + uri.IsFile);
			Console.WriteLine ("IsLoopback: " + uri.IsLoopback);
			Console.WriteLine ("IsUnc: " + uri.IsUnc);
			Console.WriteLine ("LocalPath: " + uri.LocalPath);
			Console.WriteLine ("PathAndQuery: " + uri.PathAndQuery);
			Console.WriteLine ("Port: " + uri.Port);
			Console.WriteLine ("Query: " + uri.Query);
			Console.WriteLine ("Scheme: " + uri.Scheme);
			Console.WriteLine ("UserEscaped: " + uri.UserEscaped);
			Console.WriteLine ("UserInfo: " + uri.UserInfo);

			Console.WriteLine ("Segments:");
			string [] segments = uri.Segments;
			if (segments == null) 
				Console.WriteLine ("\tNo Segments");
			else 
				for (int i = 0; i < segments.Length; i++) 
					Console.WriteLine ("\t" + segments[i]);					
			Console.WriteLine ("");
		}

	}
}

