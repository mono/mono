//
// UriTest.cs - NUnit Test Cases for System.Uri
//
// Authors:
//   Lawrence Pit (loz@cable.a2000.nl)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Ben Maurer
//

using NUnit.Framework;
using System;
using System.IO;

namespace MonoTests.System
{
	[TestFixture]
	public class UriTest
	{
		protected bool isWin32 = false;

		[SetUp]
		public void GetReady () 
		{
			isWin32 = (Path.DirectorySeparatorChar == '\\');
		}

		[Test]
		public void Constructors ()
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
			Assertion.AssertEquals ("#k1", "/", uri.AbsolutePath);
			Assertion.AssertEquals ("#k2", "http://contoso.com/?subject=uri", uri.AbsoluteUri);
			Assertion.AssertEquals ("#k3", "contoso.com", uri.Authority);
			Assertion.AssertEquals ("#k4", "", uri.Fragment);
			Assertion.AssertEquals ("#k5", "contoso.com", uri.Host);
			Assertion.AssertEquals ("#k6", UriHostNameType.Dns, uri.HostNameType);
			Assertion.AssertEquals ("#k7", true, uri.IsDefaultPort);
			Assertion.AssertEquals ("#k8", false, uri.IsFile);
			Assertion.AssertEquals ("#k9", false, uri.IsLoopback);
			Assertion.AssertEquals ("#k10", false, uri.IsUnc);
			Assertion.AssertEquals ("#k11", "/", uri.LocalPath);
			Assertion.AssertEquals ("#k12", "/?subject=uri", uri.PathAndQuery);
			Assertion.AssertEquals ("#k13", 80, uri.Port);
			Assertion.AssertEquals ("#k14", "?subject=uri", uri.Query);
			Assertion.AssertEquals ("#k15", "http", uri.Scheme);
			Assertion.AssertEquals ("#k16", false, uri.UserEscaped);
			Assertion.AssertEquals ("#k17", "", uri.UserInfo);

			uri = new Uri ("mailto:user:pwd@contoso.com?subject=uri");
			Assertion.AssertEquals ("#m1", "", uri.AbsolutePath);
			Assertion.AssertEquals ("#m2", "mailto:user:pwd@contoso.com?subject=uri", uri.AbsoluteUri);
			Assertion.AssertEquals ("#m3", "contoso.com", uri.Authority);
			Assertion.AssertEquals ("#m4", "", uri.Fragment);
			Assertion.AssertEquals ("#m5", "contoso.com", uri.Host);
			Assertion.AssertEquals ("#m6", UriHostNameType.Dns, uri.HostNameType);
			Assertion.AssertEquals ("#m7", true, uri.IsDefaultPort);
			Assertion.AssertEquals ("#m8", false, uri.IsFile);
			Assertion.AssertEquals ("#m9", false, uri.IsLoopback);
			Assertion.AssertEquals ("#m10", false, uri.IsUnc);
			Assertion.AssertEquals ("#m11", "", uri.LocalPath);
			Assertion.AssertEquals ("#m12", "?subject=uri", uri.PathAndQuery);
			Assertion.AssertEquals ("#m13", 25, uri.Port);
			Assertion.AssertEquals ("#m14", "?subject=uri", uri.Query);
			Assertion.AssertEquals ("#m15", "mailto", uri.Scheme);
			Assertion.AssertEquals ("#m16", false, uri.UserEscaped);
			Assertion.AssertEquals ("#m17", "user:pwd", uri.UserInfo);
			
			uri = new Uri (@"\\myserver\mydir\mysubdir\myfile.ext");
			Assertion.AssertEquals ("#n1", "/mydir/mysubdir/myfile.ext", uri.AbsolutePath);
			Assertion.AssertEquals ("#n2", "file://myserver/mydir/mysubdir/myfile.ext", uri.AbsoluteUri);
			Assertion.AssertEquals ("#n3", "myserver", uri.Authority);
			Assertion.AssertEquals ("#n4", "", uri.Fragment);
			Assertion.AssertEquals ("#n5", "myserver", uri.Host);
			Assertion.AssertEquals ("#n6", UriHostNameType.Dns, uri.HostNameType);
			Assertion.AssertEquals ("#n7", true, uri.IsDefaultPort);
			Assertion.AssertEquals ("#n8", true, uri.IsFile);
			Assertion.AssertEquals ("#n9", false, uri.IsLoopback);
			Assertion.AssertEquals ("#n10", true, uri.IsUnc);

			if (isWin32)
				Assertion.AssertEquals ("#n11", @"\\myserver\mydir\mysubdir\myfile.ext", uri.LocalPath);
			else
				Assertion.AssertEquals ("#n11", "/myserver/mydir/mysubdir/myfile.ext", uri.LocalPath);

			Assertion.AssertEquals ("#n12", "/mydir/mysubdir/myfile.ext", uri.PathAndQuery);
			Assertion.AssertEquals ("#n13", -1, uri.Port);
			Assertion.AssertEquals ("#n14", "", uri.Query);
			Assertion.AssertEquals ("#n15", "file", uri.Scheme);
			Assertion.AssertEquals ("#n16", false, uri.UserEscaped);
			Assertion.AssertEquals ("#n17", "", uri.UserInfo);
			
			uri = new Uri (new Uri("http://www.contoso.com"), "Hello World.htm", true);
			Assertion.AssertEquals ("#rel1a", "http://www.contoso.com/Hello World.htm", uri.AbsoluteUri);
			Assertion.AssertEquals ("#rel1b", true, uri.UserEscaped);
			uri = new Uri (new Uri("http://www.contoso.com"), "Hello World.htm", false);
			Assertion.AssertEquals ("#rel2a", "http://www.contoso.com/Hello%20World.htm", uri.AbsoluteUri);
			Assertion.AssertEquals ("#rel2b", false, uri.UserEscaped);
			uri = new Uri (new Uri("http://www.contoso.com"), "http://www.xxx.com/Hello World.htm", false);
			Assertion.AssertEquals ("#rel3", "http://www.xxx.com/Hello%20World.htm", uri.AbsoluteUri);
			//uri = new Uri (new Uri("http://www.contoso.com"), "foo:8080/bar/Hello World.htm", false);
			//Assertion.AssertEquals ("#rel4", "foo:8080/bar/Hello%20World.htm", uri.AbsoluteUri);
			uri = new Uri (new Uri("http://www.contoso.com"), "foo/bar/Hello World.htm?x=0:8", false);
			Assertion.AssertEquals ("#rel5", "http://www.contoso.com/foo/bar/Hello%20World.htm?x=0:8", uri.AbsoluteUri);
			uri = new Uri (new Uri("http://www.contoso.com/xxx/yyy/index.htm"), "foo/bar/Hello World.htm?x=0:8", false);
			Assertion.AssertEquals ("#rel6", "http://www.contoso.com/xxx/yyy/foo/bar/Hello%20World.htm?x=0:8", uri.AbsoluteUri);
			uri = new Uri (new Uri("http://www.contoso.com/xxx/yyy/index.htm"), "/foo/bar/Hello World.htm?x=0:8", false);
			Assertion.AssertEquals ("#rel7", "http://www.contoso.com/foo/bar/Hello%20World.htm?x=0:8", uri.AbsoluteUri);
			uri = new Uri (new Uri("http://www.contoso.com/xxx/yyy/index.htm"), "../foo/bar/Hello World.htm?x=0:8", false);
			Assertion.AssertEquals ("#rel8", "http://www.contoso.com/xxx/foo/bar/Hello%20World.htm?x=0:8", uri.AbsoluteUri);
			uri = new Uri (new Uri("http://www.contoso.com/xxx/yyy/index.htm"), "../../../foo/bar/Hello World.htm?x=0:8", false);
			Assertion.AssertEquals ("#rel9", "http://www.contoso.com/../foo/bar/Hello%20World.htm?x=0:8", uri.AbsoluteUri);
			uri = new Uri (new Uri("http://www.contoso.com/xxx/yyy/index.htm"), "./foo/bar/Hello World.htm?x=0:8", false);
			Assertion.AssertEquals ("#rel10", "http://www.contoso.com/xxx/yyy/foo/bar/Hello%20World.htm?x=0:8", uri.AbsoluteUri);

			try {
				uri = new Uri (null, "http://www.contoso.com/index.htm", false);
				Assertion.Fail ("#rel20");
			} catch (NullReferenceException) {
			}
			try {
				uri = new Uri (new Uri("http://www.contoso.com"), null, false);
				Assertion.Fail ("#rel21");
			} catch (NullReferenceException) {
			}
			try {
				uri = new Uri (new Uri("http://www.contoso.com/foo/bar/index.html?x=0"), String.Empty, false);
				Assertion.AssertEquals("#22", "http://www.contoso.com/foo/bar/index.html?x=0", uri.ToString ());
			} catch (NullReferenceException) {
			}			
		}
		
		[Test]
		public void LocalPath ()
		{
			Uri uri = new Uri ("c:\\tmp\\hello.txt");
			Assertion.AssertEquals ("#1a", "file:///c:/tmp/hello.txt", uri.ToString ());
			Assertion.AssertEquals ("#1b", "c:\\tmp\\hello.txt", uri.LocalPath);
			Assertion.AssertEquals ("#1c", "file", uri.Scheme);
			Assertion.AssertEquals ("#1d", "", uri.Host);
			Assertion.AssertEquals ("#1e", "c:/tmp/hello.txt", uri.AbsolutePath);
					
			uri = new Uri ("file:////////cygwin/tmp/hello.txt");
			Assertion.AssertEquals ("#3a", "file://cygwin/tmp/hello.txt", uri.ToString ());
			if (isWin32)
				Assertion.AssertEquals ("#3b win32", "\\\\cygwin\\tmp\\hello.txt", uri.LocalPath);
			else
				Assertion.AssertEquals ("#3b *nix", "/cygwin/tmp/hello.txt", uri.LocalPath);
			Assertion.AssertEquals ("#3c", "file", uri.Scheme);
			Assertion.AssertEquals ("#3d", "cygwin", uri.Host);
			Assertion.AssertEquals ("#3e", "/tmp/hello.txt", uri.AbsolutePath);

			uri = new Uri ("file://mymachine/cygwin/tmp/hello.txt");
			Assertion.AssertEquals ("#4a", "file://mymachine/cygwin/tmp/hello.txt", uri.ToString ());
			if (isWin32)
				Assertion.AssertEquals ("#4b win32", "\\\\mymachine\\cygwin\\tmp\\hello.txt", uri.LocalPath);
			else
				Assertion.AssertEquals ("#4b *nix", "/mymachine/cygwin/tmp/hello.txt", uri.LocalPath);
			Assertion.AssertEquals ("#4c", "file", uri.Scheme);
			Assertion.AssertEquals ("#4d", "mymachine", uri.Host);
			Assertion.AssertEquals ("#4e", "/cygwin/tmp/hello.txt", uri.AbsolutePath);
			
			uri = new Uri ("file://///c:/cygwin/tmp/hello.txt");
			Assertion.AssertEquals ("#5a", "file:///c:/cygwin/tmp/hello.txt", uri.ToString ());
			Assertion.AssertEquals ("#5b", "c:\\cygwin\\tmp\\hello.txt", uri.LocalPath);
			Assertion.AssertEquals ("#5c", "file", uri.Scheme);
			Assertion.AssertEquals ("#5d", "", uri.Host);
			Assertion.AssertEquals ("#5e", "c:/cygwin/tmp/hello.txt", uri.AbsolutePath);
		}
		
		[Test]
		public void UnixPath () {
			if (!isWin32)
				Assertion.AssertEquals ("#6a", "file://cygwin/tmp/hello.txt", new Uri ("/cygwin/tmp/hello.txt").ToString ());
		}
		
		[Test]
		public void Unc ()
		{
			Uri uri = new Uri ("http://www.contoso.com");
			Assertion.Assert ("#1", !uri.IsUnc);
			
			uri = new Uri ("news:123456@contoso.com");
			Assertion.Assert ("#2", !uri.IsUnc);

			uri = new Uri ("file://server/filename.ext");
			Assertion.Assert ("#3", uri.IsUnc);

			uri = new Uri (@"\\server\share\filename.ext");			
			Assertion.Assert ("#6", uri.IsUnc);
		}

		[Test]
		[Ignore ("Known to fail under MS runtime")]
		public void Unc2 ()
		{
			try {
				Uri uri = new Uri ("file:/filename.ext");
				Assertion.Assert ("#4", uri.IsUnc);
			} catch (UriFormatException) {
				Assertion.Fail ("#5: known to fail with ms.net");
			}			
		}
		
		[Test]
		public void FromHex () 
		{
			Assertion.AssertEquals ("#1", 0, Uri.FromHex ('0'));
			Assertion.AssertEquals ("#2", 9, Uri.FromHex ('9'));
			Assertion.AssertEquals ("#3", 10, Uri.FromHex ('a'));
			Assertion.AssertEquals ("#4", 15, Uri.FromHex ('f'));
			Assertion.AssertEquals ("#5", 10, Uri.FromHex ('A'));
			Assertion.AssertEquals ("#6", 15, Uri.FromHex ('F'));
			try {
				Uri.FromHex ('G');
				Assertion.Fail ("#7");
			} catch (ArgumentException) {}
			try {
				Uri.FromHex (' ');
				Assertion.Fail ("#8");
			} catch (ArgumentException) {}
			try {
				Uri.FromHex ('%');
				Assertion.Fail ("#8");
			} catch (ArgumentException) {}
		}

		[Test]
		public void HexEscape () 
		{
			Assertion.AssertEquals ("#1","%20", Uri.HexEscape (' ')); 
			Assertion.AssertEquals ("#2","%A9", Uri.HexEscape ((char) 0xa9)); 
			Assertion.AssertEquals ("#3","%41", Uri.HexEscape ('A')); 
			try {
				Uri.HexEscape ((char) 0x0369);
				Assertion.Fail ("#4");
			} catch (ArgumentOutOfRangeException) {}
		}

		[Test]
		public void HexUnescape () 
		{
			int i = 0;
			Assertion.AssertEquals ("#1", ' ', Uri.HexUnescape ("%20", ref i));
			Assertion.AssertEquals ("#2", 3, i);
			i = 4;
			Assertion.AssertEquals ("#3", (char) 0xa9, Uri.HexUnescape ("test%a9test", ref i));
			Assertion.AssertEquals ("#4", 7, i);
			Assertion.AssertEquals ("#5", 't', Uri.HexUnescape ("test%a9test", ref i));
			Assertion.AssertEquals ("#6", 8, i);
			i = 4;
			Assertion.AssertEquals ("#5", '%', Uri.HexUnescape ("test%a", ref i));
			Assertion.AssertEquals ("#6", 5, i);
			Assertion.AssertEquals ("#7", '%', Uri.HexUnescape ("testx%xx", ref i));
			Assertion.AssertEquals ("#8", 6, i);
		}

		[Test]
		public void IsHexDigit () 
		{
			Assertion.Assert ("#1", Uri.IsHexDigit ('a'));	
			Assertion.Assert ("#2", Uri.IsHexDigit ('f'));
			Assertion.Assert ("#3", !Uri.IsHexDigit ('g'));
			Assertion.Assert ("#4", Uri.IsHexDigit ('0'));
			Assertion.Assert ("#5", Uri.IsHexDigit ('9'));
			Assertion.Assert ("#6", Uri.IsHexDigit ('A'));
			Assertion.Assert ("#7", Uri.IsHexDigit ('F'));
			Assertion.Assert ("#8", !Uri.IsHexDigit ('G'));
		}

		[Test]
		public void IsHexEncoding () 
		{
			Assertion.Assert ("#1", Uri.IsHexEncoding ("test%a9test", 4));
			Assertion.Assert ("#2", !Uri.IsHexEncoding ("test%a9test", 3));
			Assertion.Assert ("#3", Uri.IsHexEncoding ("test%a9", 4));
			Assertion.Assert ("#4", !Uri.IsHexEncoding ("test%a", 4));
		}
		
		[Test]
		public void GetLeftPart ()
		{
			Uri uri = new Uri ("http://www.contoso.com/index.htm#main");
			Assertion.AssertEquals ("#1", "http://", uri.GetLeftPart (UriPartial.Scheme));
			Assertion.AssertEquals ("#2", "http://www.contoso.com", uri.GetLeftPart (UriPartial.Authority));
			Assertion.AssertEquals ("#3", "http://www.contoso.com/index.htm", uri.GetLeftPart (UriPartial.Path));
			
			uri = new Uri ("mailto:user@contoso.com?subject=uri");
			Assertion.AssertEquals ("#4", "mailto:", uri.GetLeftPart (UriPartial.Scheme));
			Assertion.AssertEquals ("#5", "", uri.GetLeftPart (UriPartial.Authority));
			Assertion.AssertEquals ("#6", "mailto:user@contoso.com", uri.GetLeftPart (UriPartial.Path));

			uri = new Uri ("nntp://news.contoso.com/123456@contoso.com");
			Assertion.AssertEquals ("#7", "nntp://", uri.GetLeftPart (UriPartial.Scheme));
			Assertion.AssertEquals ("#8", "nntp://news.contoso.com", uri.GetLeftPart (UriPartial.Authority));
			Assertion.AssertEquals ("#9", "nntp://news.contoso.com/123456@contoso.com", uri.GetLeftPart (UriPartial.Path));			
			
			uri = new Uri ("news:123456@contoso.com");
			Assertion.AssertEquals ("#10", "news:", uri.GetLeftPart (UriPartial.Scheme));
			Assertion.AssertEquals ("#11", "", uri.GetLeftPart (UriPartial.Authority));
			Assertion.AssertEquals ("#12", "news:123456@contoso.com", uri.GetLeftPart (UriPartial.Path));			

			uri = new Uri ("file://server/filename.ext");
			Assertion.AssertEquals ("#13", "file://", uri.GetLeftPart (UriPartial.Scheme));
			Assertion.AssertEquals ("#14", "file://server", uri.GetLeftPart (UriPartial.Authority));
			Assertion.AssertEquals ("#15", "file://server/filename.ext", uri.GetLeftPart (UriPartial.Path));			

			uri = new Uri (@"\\server\share\filename.ext");
			Assertion.AssertEquals ("#20", "file://", uri.GetLeftPart (UriPartial.Scheme));
			Assertion.AssertEquals ("#21", "file://server", uri.GetLeftPart (UriPartial.Authority));
			Assertion.AssertEquals ("#22", "file://server/share/filename.ext", uri.GetLeftPart (UriPartial.Path));
			
			uri = new Uri ("http://www.contoso.com:8080/index.htm#main");
			Assertion.AssertEquals ("#23", "http://", uri.GetLeftPart (UriPartial.Scheme));
			Assertion.AssertEquals ("#24", "http://www.contoso.com:8080", uri.GetLeftPart (UriPartial.Authority));
			Assertion.AssertEquals ("#25", "http://www.contoso.com:8080/index.htm", uri.GetLeftPart (UriPartial.Path));
		}
		
		[Test]
		[Ignore("Known to fail under MS runtime")]
		public void GetLeftPart2 ()
		{
			try {
				Uri uri = new Uri ("file:/filename.ext");
				Assertion.AssertEquals ("#16", "file://", uri.GetLeftPart (UriPartial.Scheme));
				Assertion.AssertEquals ("#17", "", uri.GetLeftPart (UriPartial.Authority));
				Assertion.AssertEquals ("#18", "file:///filename.ext", uri.GetLeftPart (UriPartial.Path));			
			} catch (UriFormatException) {
				Assertion.Fail ("#19: known to fail with ms.net (it's their own example!)");
			}			
		}

		[Test]
		public void CheckHostName ()
		{
			Assertion.AssertEquals ("#1", UriHostNameType.Unknown, Uri.CheckHostName (null));
			Assertion.AssertEquals ("#2", UriHostNameType.Unknown, Uri.CheckHostName (""));
			Assertion.AssertEquals ("#3", UriHostNameType.Unknown, Uri.CheckHostName ("^&()~`!@"));
			Assertion.AssertEquals ("#4", UriHostNameType.Dns, Uri.CheckHostName ("x"));
			Assertion.AssertEquals ("#5", UriHostNameType.IPv4, Uri.CheckHostName ("1.2.3.4"));
			Assertion.AssertEquals ("#6", UriHostNameType.IPv4, Uri.CheckHostName ("0001.002.03.4"));
			Assertion.AssertEquals ("#7", UriHostNameType.Dns, Uri.CheckHostName ("0001.002.03.256"));
			Assertion.AssertEquals ("#8", UriHostNameType.Dns, Uri.CheckHostName ("9001.002.03.4"));
			Assertion.AssertEquals ("#9", UriHostNameType.Dns, Uri.CheckHostName ("www.contoso.com"));
			Assertion.AssertEquals ("#10", UriHostNameType.Unknown, Uri.CheckHostName (".www.contoso.com"));
			Assertion.AssertEquals ("#11: known to fail with ms.net: this is not a valid domain address", UriHostNameType.Unknown, Uri.CheckHostName ("www.contoso.com."));	
			Assertion.AssertEquals ("#12", UriHostNameType.Dns, Uri.CheckHostName ("www.con-toso.com"));	
			Assertion.AssertEquals ("#13", UriHostNameType.Dns, Uri.CheckHostName ("www.con_toso.com"));	
			Assertion.AssertEquals ("#14", UriHostNameType.Unknown, Uri.CheckHostName ("www.con,toso.com"));	
			
			// test IPv6
			Assertion.AssertEquals ("#15", UriHostNameType.IPv6, Uri.CheckHostName ("11:22:33:44:55:66:77:88"));
			Assertion.AssertEquals ("#16", UriHostNameType.IPv6, Uri.CheckHostName ("11::33:44:55:66:77:88"));
			Assertion.AssertEquals ("#17", UriHostNameType.IPv6, Uri.CheckHostName ("::22:33:44:55:66:77:88"));
			Assertion.AssertEquals ("#18", UriHostNameType.IPv6, Uri.CheckHostName ("11:22:33:44:55:66:77::"));
			Assertion.AssertEquals ("#19", UriHostNameType.IPv6, Uri.CheckHostName ("11::88"));
			Assertion.AssertEquals ("#20", UriHostNameType.IPv6, Uri.CheckHostName ("11::77:88"));
			Assertion.AssertEquals ("#21", UriHostNameType.IPv6, Uri.CheckHostName ("11:22::88"));
			Assertion.AssertEquals ("#22", UriHostNameType.IPv6, Uri.CheckHostName ("11::"));
			Assertion.AssertEquals ("#23", UriHostNameType.IPv6, Uri.CheckHostName ("::88"));
			Assertion.AssertEquals ("#24", UriHostNameType.IPv6, Uri.CheckHostName ("::1"));
			Assertion.AssertEquals ("#25", UriHostNameType.IPv6, Uri.CheckHostName ("::"));
			Assertion.AssertEquals ("#26", UriHostNameType.IPv6, Uri.CheckHostName ("0:0:0:0:0:0:127.0.0.1"));
			Assertion.AssertEquals ("#27", UriHostNameType.IPv6, Uri.CheckHostName ("::127.0.0.1"));
			Assertion.AssertEquals ("#28", UriHostNameType.IPv6, Uri.CheckHostName ("::ffFF:169.32.14.5"));
			Assertion.AssertEquals ("#29", UriHostNameType.IPv6, Uri.CheckHostName ("2001:03A0::/35"));
			Assertion.AssertEquals ("#30", UriHostNameType.IPv6, Uri.CheckHostName ("[2001:03A0::/35]"));

			Assertion.AssertEquals ("#31", UriHostNameType.Unknown, Uri.CheckHostName ("2001::03A0::/35"));
			Assertion.AssertEquals ("#32", UriHostNameType.Unknown, Uri.CheckHostName ("2001:03A0::/35a"));
			Assertion.AssertEquals ("#33 known to fail with ms.net: this is not a valid IPv6 address.", UriHostNameType.Unknown, Uri.CheckHostName ("2001::03A0:1.2.3.4"));
			Assertion.AssertEquals ("#34", UriHostNameType.Unknown, Uri.CheckHostName ("::ffff:123.256.155.43"));
			Assertion.AssertEquals ("#35", UriHostNameType.Unknown, Uri.CheckHostName (":127.0.0.1"));
			Assertion.AssertEquals ("#36 known to fail with ms.net: this is not a valid IPv6 address.", UriHostNameType.Unknown, Uri.CheckHostName (":11:22:33:44:55:66:77:88"));
			Assertion.AssertEquals ("#37", UriHostNameType.Unknown, Uri.CheckHostName ("::11:22:33:44:55:66:77:88"));
			Assertion.AssertEquals ("#38", UriHostNameType.Unknown, Uri.CheckHostName ("11:22:33:44:55:66:77:88::"));
			Assertion.AssertEquals ("#39", UriHostNameType.Unknown, Uri.CheckHostName ("11:22:33:44:55:66:77:88:"));
			Assertion.AssertEquals ("#40", UriHostNameType.Unknown, Uri.CheckHostName ("::acbde"));
			Assertion.AssertEquals ("#41", UriHostNameType.Unknown, Uri.CheckHostName ("::abce:"));
			Assertion.AssertEquals ("#42", UriHostNameType.Unknown, Uri.CheckHostName ("::abcg"));
			Assertion.AssertEquals ("#43", UriHostNameType.Unknown, Uri.CheckHostName (":::"));
			Assertion.AssertEquals ("#44", UriHostNameType.Unknown, Uri.CheckHostName (":"));
		}
		
		[Test]
		public void IsLoopback ()
		{
			Uri uri = new Uri("http://loopback:8080");
			Assertion.AssertEquals ("#1", true, uri.IsLoopback);
			uri = new Uri("http://localhost:8080");
			Assertion.AssertEquals ("#2", true, uri.IsLoopback);
			uri = new Uri("http://127.0.0.1:8080");
			Assertion.AssertEquals ("#3", true, uri.IsLoopback);
			uri = new Uri("http://127.0.0.001:8080");
			Assertion.AssertEquals ("#4", true, uri.IsLoopback);
			uri = new Uri("http://[::1]");
			Assertion.AssertEquals ("#5", true, uri.IsLoopback);
			uri = new Uri("http://[::1]:8080");
			Assertion.AssertEquals ("#6", true, uri.IsLoopback);
			uri = new Uri("http://[::0001]:8080");
			Assertion.AssertEquals ("#7", true, uri.IsLoopback);
			uri = new Uri("http://[0:0:0:0::1]:8080");
			Assertion.AssertEquals ("#8", true, uri.IsLoopback);
			uri = new Uri("http://[0:0:0:0::127.0.0.1]:8080");
			Assertion.AssertEquals ("#9", true, uri.IsLoopback);
			uri = new Uri("http://[0:0:0:0::127.11.22.33]:8080");
			Assertion.AssertEquals ("#10: known to fail with ms.net", true, uri.IsLoopback);
			uri = new Uri("http://[::ffff:127.11.22.33]:8080");
			Assertion.AssertEquals ("#11: known to fail with ms.net", true, uri.IsLoopback);
			uri = new Uri("http://[::ff00:7f11:2233]:8080");
			Assertion.AssertEquals ("#12", false, uri.IsLoopback);
			uri = new Uri("http://[1:0:0:0::1]:8080");
			Assertion.AssertEquals ("#13", false, uri.IsLoopback);
		}
		
		[Test]
		public void Equals ()
		{
			Uri uri1 = new Uri ("http://www.contoso.com/index.htm#main");
			Uri uri2 = new Uri ("http://www.contoso.com/index.htm#fragment");
			Assertion.Assert ("#1", uri1.Equals (uri2));
			uri2 = new Uri ("http://www.contoso.com/index.htm?x=1");
			Assertion.Assert ("#2 known to fail with ms.net", !uri1.Equals (uri2));
			Assertion.Assert ("#3", !uri2.Equals ("http://www.contoso.com/index.html?x=1"));
			Assertion.Assert ("#4: known to fail with ms.net", !uri1.Equals ("http://www.contoso.com:8080/index.htm?x=1"));
		}
		
		[Test]
		public void GetHashCodeTest () 
		{
			Uri uri1 = new Uri ("http://www.contoso.com/index.htm#main");
			Uri uri2 = new Uri ("http://www.contoso.com/index.htm#fragment");
			Assertion.AssertEquals ("#1", uri1.GetHashCode (), uri2.GetHashCode ());
			uri2 = new Uri ("http://www.contoso.com/index.htm?x=1");
			Assertion.Assert ("#2", uri1.GetHashCode () != uri2.GetHashCode ());
			uri2 = new Uri ("http://www.contoso.com:80/index.htm");
			Assertion.AssertEquals ("#3", uri1.GetHashCode (), uri2.GetHashCode ());			
			uri2 = new Uri ("http://www.contoso.com:8080/index.htm");
			Assertion.Assert ("#4", uri1.GetHashCode () != uri2.GetHashCode ());			
		}
		
		[Test]
		public void MakeRelative ()
		{
			Uri uri1 = new Uri ("http://www.contoso.com/index.htm?x=2");
			Uri uri2 = new Uri ("http://www.contoso.com/foo/bar/index.htm#fragment");
			Uri uri3 = new Uri ("http://www.contoso.com/bar/foo/index.htm?y=1");
			Uri uri4 = new Uri ("http://www.contoso.com/bar/foo2/index.htm?x=0");
			Uri uri5 = new Uri ("https://www.contoso.com/bar/foo/index.htm?y=1");
			Uri uri6 = new Uri ("http://www.contoso2.com/bar/foo/index.htm?x=0");
			Uri uri7 = new Uri ("http://www.contoso2.com/bar/foo/foobar.htm?z=0&y=5");
			Uri uri8 = new Uri ("http://www.xxx.com/bar/foo/foobar.htm?z=0&y=5" + (char) 0xa9);

			Assertion.AssertEquals ("#1", "foo/bar/index.htm", uri1.MakeRelative (uri2));
			Assertion.AssertEquals ("#2", "../../index.htm", uri2.MakeRelative (uri1));
			
			Assertion.AssertEquals ("#3", "../../bar/foo/index.htm", uri2.MakeRelative (uri3));
			Assertion.AssertEquals ("#4", "../../foo/bar/index.htm", uri3.MakeRelative (uri2));			

			Assertion.AssertEquals ("#5", "../foo2/index.htm", uri3.MakeRelative (uri4));
			Assertion.AssertEquals ("#6", "../foo/index.htm", uri4.MakeRelative (uri3));
			
			Assertion.AssertEquals ("#7", "https://www.contoso.com/bar/foo/index.htm?y=1", 
				            uri4.MakeRelative (uri5));

			Assertion.AssertEquals ("#8", "http://www.contoso2.com/bar/foo/index.htm?x=0", 
					    uri4.MakeRelative (uri6));

			Assertion.AssertEquals ("#9", "", uri6.MakeRelative (uri6));
			Assertion.AssertEquals ("#10", "foobar.htm", uri6.MakeRelative (uri7));
			
			Uri uri10 = new Uri ("mailto:xxx@xxx.com");
			Uri uri11 = new Uri ("mailto:xxx@xxx.com?subject=hola");
			Assertion.AssertEquals ("#11", "", uri10.MakeRelative (uri11));
			
			Uri uri12 = new Uri ("mailto:xxx@mail.xxx.com?subject=hola");
			Assertion.AssertEquals ("#12", "mailto:xxx@mail.xxx.com?subject=hola", uri10.MakeRelative (uri12));
						
			Uri uri13 = new Uri ("mailto:xxx@xxx.com/foo/bar");
			Assertion.AssertEquals ("#13", "/foo/bar", uri10.MakeRelative (uri13));
			
			Assertion.AssertEquals ("#14", "http://www.xxx.com/bar/foo/foobar.htm?z=0&y=5" + (char) 0xa9, uri1.MakeRelative (uri8));
		}
		
		[Test]
		public void ToStringTest()
		{			
			Uri uri = new Uri ("dummy://xxx");
			Assertion.AssertEquals ("#1", "dummy://xxx/", uri.ToString ());
		}

		[Test]
		public void CheckSchemeName ()
		{
			Assertion.AssertEquals ("#01", false, Uri.CheckSchemeName (null));
			Assertion.AssertEquals ("#02", false, Uri.CheckSchemeName (""));
			Assertion.AssertEquals ("#03", true, Uri.CheckSchemeName ("http"));
			Assertion.AssertEquals ("#04", true, Uri.CheckSchemeName ("http-"));
			Assertion.AssertEquals ("#05", false, Uri.CheckSchemeName ("6http-"));
			Assertion.AssertEquals ("#06", true, Uri.CheckSchemeName ("http6-"));
			Assertion.AssertEquals ("#07", false, Uri.CheckSchemeName ("http6,"));
			Assertion.AssertEquals ("#08", true, Uri.CheckSchemeName ("http6."));
			Assertion.AssertEquals ("#09", false, Uri.CheckSchemeName ("+http"));
			Assertion.AssertEquals ("#10", true, Uri.CheckSchemeName ("htt+p6"));
			// 0x00E1 -> &atilde;
			Assertion.AssertEquals ("#11", true, Uri.CheckSchemeName ("htt\u00E1+p6"));
		}

		[Test]
		[ExpectedException (typeof (UriFormatException))]
		public void NoHostname ()
		{
			Uri uri = new Uri ("http://");
		}

		[Test]
		[ExpectedException (typeof (UriFormatException))]
		[Ignore ("MS throws an IndexOutOfRangeException. Bug?")]
		public void NoHostname2 ()
		{
			Uri uri = new Uri ("file://");
		}

		[Test]
		public void Segments1 ()
		{
			Uri uri = new Uri ("http://localhost/");
			string [] segments = uri.Segments;
			Assertion.AssertEquals ("#01", 1, segments.Length);
			Assertion.AssertEquals ("#02", "/", segments [0]);
			
		}

		[Test]
		public void Segments2 ()
		{
			Uri uri = new Uri ("http://localhost/dir/dummypage.html");
			string [] segments = uri.Segments;
			Assertion.AssertEquals ("#01", 3, segments.Length);
			Assertion.AssertEquals ("#02", "/", segments [0]);
			Assertion.AssertEquals ("#03", "dir/", segments [1]);
			Assertion.AssertEquals ("#04", "dummypage.html", segments [2]);
			
		}

		[Test]
		public void Segments3 ()
		{
			Uri uri = new Uri ("http://localhost/dir/dummypage/");
			string [] segments = uri.Segments;
			Assertion.AssertEquals ("#01", 3, segments.Length);
			Assertion.AssertEquals ("#02", "/", segments [0]);
			Assertion.AssertEquals ("#03", "dir/", segments [1]);
			Assertion.AssertEquals ("#04", "dummypage/", segments [2]);
			
		}

		[Test]
		public void Segments4 ()
		{
			Uri uri = new Uri ("file:///c:/hello");
			string [] segments = uri.Segments;
			Assertion.AssertEquals ("#01", 3, segments.Length);
			Assertion.AssertEquals ("#02", "c:", segments [0]);
			Assertion.AssertEquals ("#03", "/", segments [1]);
			Assertion.AssertEquals ("#04", "hello", segments [2]);
			
		}

		[Test]
		[ExpectedException (typeof (UriFormatException))]
		public void EmptyScheme ()
		{
			new Uri ("hey");
		}

		[Test]
		public void InvalidPortsThatWorkWithMS ()
		{
			new Uri ("http://www.contoso.com:12345678/foo/bar/");
			// UInt32.MaxValue gives port == -1 !!!
			new Uri ("http://www.contoso.com:4294967295/foo/bar/");
			// ((uint) Int32.MaxValue + (uint) 1) gives port == -2147483648 !!!
			new Uri ("http://www.contoso.com:2147483648/foo/bar/");
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

