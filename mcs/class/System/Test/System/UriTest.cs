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
	public class UriTest : Assertion
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

			if (isWin32)
				AssertEquals ("#n11", @"\\myserver\mydir\mysubdir\myfile.ext", uri.LocalPath);
			else
				AssertEquals ("#n11", "//myserver/mydir/mysubdir/myfile.ext", uri.LocalPath);

			AssertEquals ("#n12", "/mydir/mysubdir/myfile.ext", uri.PathAndQuery);
			AssertEquals ("#n13", -1, uri.Port);
			AssertEquals ("#n14", "", uri.Query);
			AssertEquals ("#n15", "file", uri.Scheme);
			AssertEquals ("#n16", false, uri.UserEscaped);
			AssertEquals ("#n17", "", uri.UserInfo);
			
			uri = new Uri (new Uri("http://www.contoso.com"), "Hello World.htm", true);
			AssertEquals ("#rel1a", "http://www.contoso.com/Hello World.htm", uri.AbsoluteUri);
			AssertEquals ("#rel1b", true, uri.UserEscaped);
			uri = new Uri (new Uri("http://www.contoso.com"), "Hello World.htm", false);
			AssertEquals ("#rel2a", "http://www.contoso.com/Hello%20World.htm", uri.AbsoluteUri);
			AssertEquals ("#rel2b", false, uri.UserEscaped);
			uri = new Uri (new Uri("http://www.contoso.com"), "http://www.xxx.com/Hello World.htm", false);
			AssertEquals ("#rel3", "http://www.xxx.com/Hello%20World.htm", uri.AbsoluteUri);
			//uri = new Uri (new Uri("http://www.contoso.com"), "foo:8080/bar/Hello World.htm", false);
			//AssertEquals ("#rel4", "foo:8080/bar/Hello%20World.htm", uri.AbsoluteUri);
			uri = new Uri (new Uri("http://www.contoso.com"), "foo/bar/Hello World.htm?x=0:8", false);
			AssertEquals ("#rel5", "http://www.contoso.com/foo/bar/Hello%20World.htm?x=0:8", uri.AbsoluteUri);
			uri = new Uri (new Uri("http://www.contoso.com/xxx/yyy/index.htm"), "foo/bar/Hello World.htm?x=0:8", false);
			AssertEquals ("#rel6", "http://www.contoso.com/xxx/yyy/foo/bar/Hello%20World.htm?x=0:8", uri.AbsoluteUri);
			uri = new Uri (new Uri("http://www.contoso.com/xxx/yyy/index.htm"), "/foo/bar/Hello World.htm?x=0:8", false);
			AssertEquals ("#rel7", "http://www.contoso.com/foo/bar/Hello%20World.htm?x=0:8", uri.AbsoluteUri);
			uri = new Uri (new Uri("http://www.contoso.com/xxx/yyy/index.htm"), "../foo/bar/Hello World.htm?x=0:8", false);
			AssertEquals ("#rel8", "http://www.contoso.com/xxx/foo/bar/Hello%20World.htm?x=0:8", uri.AbsoluteUri);
			uri = new Uri (new Uri("http://www.contoso.com/xxx/yyy/index.htm"), "../../../foo/bar/Hello World.htm?x=0:8", false);
			AssertEquals ("#rel9", "http://www.contoso.com/../foo/bar/Hello%20World.htm?x=0:8", uri.AbsoluteUri);
			uri = new Uri (new Uri("http://www.contoso.com/xxx/yyy/index.htm"), "./foo/bar/Hello World.htm?x=0:8", false);
			AssertEquals ("#rel10", "http://www.contoso.com/xxx/yyy/foo/bar/Hello%20World.htm?x=0:8", uri.AbsoluteUri);

			try {
				uri = new Uri (null, "http://www.contoso.com/index.htm", false);
				Fail ("#rel20");
			} catch (NullReferenceException) {
			}
			try {
				uri = new Uri (new Uri("http://www.contoso.com"), null, false);
				Fail ("#rel21");
			} catch (NullReferenceException) {
			}
			try {
				uri = new Uri (new Uri("http://www.contoso.com/foo/bar/index.html?x=0"), String.Empty, false);
				AssertEquals("#22", "http://www.contoso.com/foo/bar/index.html?x=0", uri.ToString ());
			} catch (NullReferenceException) {
			}

			uri = new Uri (new Uri("http://www.xxx.com"), "?x=0");
			AssertEquals ("#rel30", "http://www.xxx.com/?x=0", uri.ToString());
			uri = new Uri (new Uri("http://www.xxx.com/index.htm"), "?x=0");
			AssertEquals ("#rel31", "http://www.xxx.com/?x=0", uri.ToString());
			uri = new Uri (new Uri("http://www.xxx.com/index.htm"), "#here");
			AssertEquals ("#rel32", "http://www.xxx.com/index.htm#here", uri.ToString());
		}

		[Test]
		public void ConstructorsRejectRelativePath ()
		{
			string [] reluris = new string [] {
				"readme.txt",
				"thisdir/childdir/file"
			};
			string [] winRelUris = new string [] {
				"c:readme.txt"
			};

			for (int i = 0; i < reluris.Length; i++) {
				try {
					new Uri (reluris [i]);
					Fail ("Should be failed: " + reluris [i]);
				} catch (UriFormatException) {
				}
			}

			if (isWin32) {
				for (int i = 0; i < winRelUris.Length; i++) {
					try {
						new Uri (winRelUris [i]);
						Fail ("Should be failed: " + winRelUris [i]);
					} catch (UriFormatException) {
					}
				}
			}
		}


		[Test]
		public void LocalPath ()
		{
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
				AssertEquals ("#3b *nix", "/cygwin/tmp/hello.txt", uri.LocalPath);
			AssertEquals ("#3c", "file", uri.Scheme);
			AssertEquals ("#3d", "cygwin", uri.Host);
			AssertEquals ("#3e", "/tmp/hello.txt", uri.AbsolutePath);

			uri = new Uri ("file://mymachine/cygwin/tmp/hello.txt");
			AssertEquals ("#4a", "file://mymachine/cygwin/tmp/hello.txt", uri.ToString ());
			if (isWin32)
				AssertEquals ("#4b win32", "\\\\mymachine\\cygwin\\tmp\\hello.txt", uri.LocalPath);
			else
				AssertEquals ("#4b *nix", "/mymachine/cygwin/tmp/hello.txt", uri.LocalPath);
			AssertEquals ("#4c", "file", uri.Scheme);
			AssertEquals ("#4d", "mymachine", uri.Host);
			AssertEquals ("#4e", "/cygwin/tmp/hello.txt", uri.AbsolutePath);
			
			uri = new Uri ("file://///c:/cygwin/tmp/hello.txt");
			AssertEquals ("#5a", "file:///c:/cygwin/tmp/hello.txt", uri.ToString ());
			AssertEquals ("#5b", "c:\\cygwin\\tmp\\hello.txt", uri.LocalPath);
			AssertEquals ("#5c", "file", uri.Scheme);
			AssertEquals ("#5d", "", uri.Host);
			AssertEquals ("#5e", "c:/cygwin/tmp/hello.txt", uri.AbsolutePath);
			
			uri = new Uri("file://one_file.txt");
			AssertEquals("#6a", "file://one_file.txt", uri.ToString());
			if (isWin32)
				AssertEquals("#6b", "\\\\one_file.txt", uri.LocalPath);
			else
				AssertEquals("#6b", "/one_file.txt", uri.LocalPath);
			AssertEquals("#6c", "file", uri.Scheme);
			AssertEquals("#6d", "one_file.txt", uri.Host);
			AssertEquals("#6e", "", uri.AbsolutePath);
		}
		
		[Test]
		public void UnixPath () {
			if (!isWin32)
				AssertEquals ("#6a", "file:///cygwin/tmp/hello.txt", new Uri ("/cygwin/tmp/hello.txt").ToString ());
		}
		
		[Test]
		public void Unc ()
		{
			Uri uri = new Uri ("http://www.contoso.com");
			Assert ("#1", !uri.IsUnc);
			
			uri = new Uri ("news:123456@contoso.com");
			Assert ("#2", !uri.IsUnc);

			uri = new Uri ("file://server/filename.ext");
			Assert ("#3", uri.IsUnc);

			uri = new Uri (@"\\server\share\filename.ext");			
			Assert ("#6", uri.IsUnc);

			uri = new Uri (@"a:\dir\filename.ext");
			Assert ("#8", !uri.IsUnc);
		}

		[Test, ExpectedException (typeof (UriFormatException))]
		public void UncFail ()
		{
			Uri uri = new Uri ("/home/user/dir/filename.ext");
			Assert ("#7", !uri.IsUnc);
		}

		[Test]
		[Ignore ("Known to fail under MS runtime")]
		public void Unc2 ()
		{
			try {
				Uri uri = new Uri ("file:/filename.ext");
				Assert ("#4", uri.IsUnc);
			} catch (UriFormatException) {
				Fail ("#5: known to fail with ms.net");
			}			
		}
		
		[Test]
		public void FromHex () 
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

		[Test]
		public void HexEscape () 
		{
			AssertEquals ("#1","%20", Uri.HexEscape (' ')); 
			AssertEquals ("#2","%A9", Uri.HexEscape ((char) 0xa9)); 
			AssertEquals ("#3","%41", Uri.HexEscape ('A')); 
			try {
				Uri.HexEscape ((char) 0x0369);
				Fail ("#4");
			} catch (ArgumentOutOfRangeException) {}
		}

		[Test]
		public void HexUnescape () 
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

		[Test]
		public void IsHexDigit () 
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

		[Test]
		public void IsHexEncoding () 
		{
			Assert ("#1", Uri.IsHexEncoding ("test%a9test", 4));
			Assert ("#2", !Uri.IsHexEncoding ("test%a9test", 3));
			Assert ("#3", Uri.IsHexEncoding ("test%a9", 4));
			Assert ("#4", !Uri.IsHexEncoding ("test%a", 4));
		}
		
		[Test]
		public void GetLeftPart ()
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

			uri = new Uri (@"\\server\share\filename.ext");
			AssertEquals ("#20", "file://", uri.GetLeftPart (UriPartial.Scheme));
			AssertEquals ("#21", "file://server", uri.GetLeftPart (UriPartial.Authority));
			AssertEquals ("#22", "file://server/share/filename.ext", uri.GetLeftPart (UriPartial.Path));
			
			uri = new Uri ("http://www.contoso.com:8080/index.htm#main");
			AssertEquals ("#23", "http://", uri.GetLeftPart (UriPartial.Scheme));
			AssertEquals ("#24", "http://www.contoso.com:8080", uri.GetLeftPart (UriPartial.Authority));
			AssertEquals ("#25", "http://www.contoso.com:8080/index.htm", uri.GetLeftPart (UriPartial.Path));
		}
		
		[Test]
		[Ignore("Known to fail under MS runtime")]
		public void GetLeftPart2 ()
		{
			try {
				Uri uri = new Uri ("file:/filename.ext");
				AssertEquals ("#16", "file://", uri.GetLeftPart (UriPartial.Scheme));
				AssertEquals ("#17", "", uri.GetLeftPart (UriPartial.Authority));
				AssertEquals ("#18", "file:///filename.ext", uri.GetLeftPart (UriPartial.Path));			
			} catch (UriFormatException) {
				Fail ("#19: known to fail with ms.net (it's their own example!)");
			}			
		}

		[Test]
		public void CheckHostName ()
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
		
		[Test]
		public void IsLoopback ()
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
		
		[Test]
		public void Equals ()
		{
			Uri uri1 = new Uri ("http://www.contoso.com/index.htm#main");
			Uri uri2 = new Uri ("http://www.contoso.com/index.htm#fragment");
			Assert ("#1", uri1.Equals (uri2));
			uri2 = new Uri ("http://www.contoso.com/index.htm?x=1");
			Assert ("#2 known to fail with ms.net", !uri1.Equals (uri2));
			Assert ("#3", !uri2.Equals ("http://www.contoso.com/index.html?x=1"));
			Assert ("#4: known to fail with ms.net", !uri1.Equals ("http://www.contoso.com:8080/index.htm?x=1"));
		}

		[Test]
		public void TestEquals2 ()
		{
			Uri a = new Uri ("http://www.go-mono.com");
			Uri b = new Uri ("http://www.go-mono.com");

			AssertEquals ("#1", a, b);

			a = new Uri ("mailto:user:pwd@go-mono.com?subject=uri");
			b = new Uri ("MAILTO:USER:PWD@GO-MONO.COM?SUBJECT=URI");

			AssertEquals ("#2", a, b);

			a = new Uri ("http://www.go-mono.com/ports/");
			b = new Uri ("http://www.go-mono.com/PORTS/");

			Assert ("#3", !a.Equals (b));
		}

		[Test]
		public void GetHashCodeTest () 
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

			AssertEquals ("#1", "foo/bar/index.htm", uri1.MakeRelative (uri2));
			AssertEquals ("#2", "../../index.htm", uri2.MakeRelative (uri1));
			
			AssertEquals ("#3", "../../bar/foo/index.htm", uri2.MakeRelative (uri3));
			AssertEquals ("#4", "../../foo/bar/index.htm", uri3.MakeRelative (uri2));			

			AssertEquals ("#5", "../foo2/index.htm", uri3.MakeRelative (uri4));
			AssertEquals ("#6", "../foo/index.htm", uri4.MakeRelative (uri3));
			
			AssertEquals ("#7", "https://www.contoso.com/bar/foo/index.htm?y=1", 
				            uri4.MakeRelative (uri5));

			AssertEquals ("#8", "http://www.contoso2.com/bar/foo/index.htm?x=0", 
					    uri4.MakeRelative (uri6));

			AssertEquals ("#9", "", uri6.MakeRelative (uri6));
			AssertEquals ("#10", "foobar.htm", uri6.MakeRelative (uri7));
			
			Uri uri10 = new Uri ("mailto:xxx@xxx.com");
			Uri uri11 = new Uri ("mailto:xxx@xxx.com?subject=hola");
			AssertEquals ("#11", "", uri10.MakeRelative (uri11));
			
			Uri uri12 = new Uri ("mailto:xxx@mail.xxx.com?subject=hola");
			AssertEquals ("#12", "mailto:xxx@mail.xxx.com?subject=hola", uri10.MakeRelative (uri12));
						
			Uri uri13 = new Uri ("mailto:xxx@xxx.com/foo/bar");
			AssertEquals ("#13", "/foo/bar", uri10.MakeRelative (uri13));
			
			AssertEquals ("#14", "http://www.xxx.com/bar/foo/foobar.htm?z=0&y=5" + (char) 0xa9, uri1.MakeRelative (uri8));
		}
		
		[Test]
		public void ToStringTest()
		{			
			Uri uri = new Uri ("dummy://xxx");
			AssertEquals ("#1", "dummy://xxx/", uri.ToString ());
		}

		[Test]
		public void CheckSchemeName ()
		{
			AssertEquals ("#01", false, Uri.CheckSchemeName (null));
			AssertEquals ("#02", false, Uri.CheckSchemeName (""));
			AssertEquals ("#03", true, Uri.CheckSchemeName ("http"));
			AssertEquals ("#04", true, Uri.CheckSchemeName ("http-"));
			AssertEquals ("#05", false, Uri.CheckSchemeName ("6http-"));
			AssertEquals ("#06", true, Uri.CheckSchemeName ("http6-"));
			AssertEquals ("#07", false, Uri.CheckSchemeName ("http6,"));
			AssertEquals ("#08", true, Uri.CheckSchemeName ("http6."));
			AssertEquals ("#09", false, Uri.CheckSchemeName ("+http"));
			AssertEquals ("#10", true, Uri.CheckSchemeName ("htt+p6"));
			// 0x00E1 -> &atilde;
			AssertEquals ("#11", true, Uri.CheckSchemeName ("htt\u00E1+p6"));
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
			AssertEquals ("#01", 1, segments.Length);
			AssertEquals ("#02", "/", segments [0]);
			
		}

		[Test]
		public void Segments2 ()
		{
			Uri uri = new Uri ("http://localhost/dir/dummypage.html");
			string [] segments = uri.Segments;
			AssertEquals ("#01", 3, segments.Length);
			AssertEquals ("#02", "/", segments [0]);
			AssertEquals ("#03", "dir/", segments [1]);
			AssertEquals ("#04", "dummypage.html", segments [2]);
			
		}

		[Test]
		public void Segments3 ()
		{
			Uri uri = new Uri ("http://localhost/dir/dummypage/");
			string [] segments = uri.Segments;
			AssertEquals ("#01", 3, segments.Length);
			AssertEquals ("#02", "/", segments [0]);
			AssertEquals ("#03", "dir/", segments [1]);
			AssertEquals ("#04", "dummypage/", segments [2]);
			
		}

		[Test]
		public void Segments4 ()
		{
			Uri uri = new Uri ("file:///c:/hello");
			string [] segments = uri.Segments;
			AssertEquals ("#01", 3, segments.Length);
			AssertEquals ("#02", "c:", segments [0]);
			AssertEquals ("#03", "/", segments [1]);
			AssertEquals ("#04", "hello", segments [2]);
			
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

