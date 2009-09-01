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
using System.Text;

// cheap hack to avoid backporting the new NUnit syntax patch
using NAssert = NUnit.Framework.Assert;

namespace MonoTests.System
{
	[TestFixture]
	public class UriTest : Assertion
	{
		protected bool isWin32 = false;

		[TestFixtureSetUp]
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

			uri = new Uri("  \r  \n http://test.com\r\n \r\r  ");
			AssertEquals ("#k0", "http://test.com/", uri.ToString());

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

			uri = new Uri("myscheme://127.0.0.1:5");
			AssertEquals("#c1", "myscheme://127.0.0.1:5/", uri.ToString());
			
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
				// myserver never could be the part of Unix path.
				AssertEquals ("#n11", "/mydir/mysubdir/myfile.ext", uri.LocalPath);

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

			uri = new Uri (new Uri("http://www.contoso.com"), "foo/bar/Hello World.htm?x=0:8", false);
			AssertEquals ("#rel5", "http://www.contoso.com/foo/bar/Hello%20World.htm?x=0:8", uri.AbsoluteUri);
			uri = new Uri (new Uri("http://www.contoso.com/xxx/yyy/index.htm"), "foo/bar/Hello World.htm?x=0:8", false);
			AssertEquals ("#rel6", "http://www.contoso.com/xxx/yyy/foo/bar/Hello%20World.htm?x=0:8", uri.AbsoluteUri);
			uri = new Uri (new Uri("http://www.contoso.com/xxx/yyy/index.htm"), "/foo/bar/Hello World.htm?x=0:8", false);
			AssertEquals ("#rel7", "http://www.contoso.com/foo/bar/Hello%20World.htm?x=0:8", uri.AbsoluteUri);
			uri = new Uri (new Uri("http://www.contoso.com/xxx/yyy/index.htm"), "../foo/bar/Hello World.htm?x=0:8", false);
			AssertEquals ("#rel8", "http://www.contoso.com/xxx/foo/bar/Hello%20World.htm?x=0:8", uri.AbsoluteUri);
			uri = new Uri (new Uri("http://www.contoso.com/xxx/yyy/index.htm"), "../../../foo/bar/Hello World.htm?x=0:8", false);
#if NET_2_0
			AssertEquals ("#rel9", "http://www.contoso.com/foo/bar/Hello%20World.htm?x=0:8", uri.AbsoluteUri);
#else
			AssertEquals ("#rel9", "http://www.contoso.com/../foo/bar/Hello%20World.htm?x=0:8", uri.AbsoluteUri);
#endif
			uri = new Uri (new Uri("http://www.contoso.com/xxx/yyy/index.htm"), "./foo/bar/Hello World.htm?x=0:8", false);
			AssertEquals ("#rel10", "http://www.contoso.com/xxx/yyy/foo/bar/Hello%20World.htm?x=0:8", uri.AbsoluteUri);

			uri = new Uri (new Uri("http://www.contoso.com/foo/bar/index.html?x=0"), String.Empty, false);
			AssertEquals("#22", "http://www.contoso.com/foo/bar/index.html?x=0", uri.ToString ());

			uri = new Uri (new Uri("http://www.xxx.com"), "?x=0");
			AssertEquals ("#rel30", "http://www.xxx.com/?x=0", uri.ToString());
			uri = new Uri (new Uri("http://www.xxx.com/index.htm"), "?x=0");
			AssertEquals ("#rel31", "http://www.xxx.com/?x=0", uri.ToString());
			uri = new Uri (new Uri("http://www.xxx.com/index.htm"), "#here");
			AssertEquals ("#rel32", "http://www.xxx.com/index.htm#here", uri.ToString());
#if NET_2_0
			uri = new Uri ("relative", UriKind.Relative);
			uri = new Uri ("relative/abc", UriKind.Relative);
			uri = new Uri ("relative", UriKind.RelativeOrAbsolute);

			Assert ("#rel33", !uri.IsAbsoluteUri);
			AssertEquals ("#rel34", uri.OriginalString, "relative");
			Assert ("#rel35", !uri.UserEscaped);
#endif
		}

		[Test]
		public void Constructor_DualHostPort ()
		{
			string relative = "foo:8080/bar/Hello World.htm";
			Uri uri = new Uri (new Uri("http://www.contoso.com"), relative, false);
			AssertEquals ("AbsolutePath", "8080/bar/Hello%20World.htm", uri.AbsolutePath);
			AssertEquals ("AbsoluteUri", "foo:8080/bar/Hello%20World.htm", uri.AbsoluteUri);
			AssertEquals ("Authority", String.Empty, uri.Authority);
			AssertEquals ("Fragment", String.Empty, uri.Fragment);
			AssertEquals ("Host", String.Empty, uri.Host);
			AssertEquals ("PathAndQuery", "8080/bar/Hello%20World.htm", uri.PathAndQuery);
			AssertEquals ("Port", -1, uri.Port);
			AssertEquals ("Query", String.Empty, uri.Query);
			AssertEquals ("Scheme", "foo", uri.Scheme);
			AssertEquals ("Query", String.Empty, uri.UserInfo);

			AssertEquals ("Segments[0]", "8080/", uri.Segments[0]);
			AssertEquals ("Segments[1]", "bar/", uri.Segments[1]);
			AssertEquals ("Segments[2]", "Hello%20World.htm", uri.Segments[2]);

			Assert ("IsDefaultPort", uri.IsDefaultPort);
			Assert ("IsFile", !uri.IsFile);
			Assert ("IsLoopback", !uri.IsLoopback);
			Assert ("IsUnc", !uri.IsUnc);
			Assert ("UserEscaped", !uri.UserEscaped);
#if NET_2_0
			AssertEquals ("HostNameType", UriHostNameType.Unknown, uri.HostNameType);
			Assert ("IsAbsoluteUri", uri.IsAbsoluteUri);
			AssertEquals ("OriginalString", relative, uri.OriginalString);
#else
			AssertEquals ("HostNameType", UriHostNameType.Basic, uri.HostNameType);
#endif
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentNullException))]
#else
		[ExpectedException (typeof (NullReferenceException))]
#endif
		public void Constructor_NullStringBool ()
		{
			new Uri (null, "http://www.contoso.com/index.htm", false);
		}

		[Test]
#if ONLY_1_1
		[ExpectedException (typeof (NullReferenceException))]
#endif
		public void Constructor_UriNullBool ()
		{
			new Uri (new Uri ("http://www.contoso.com"), null, false);
		}

		// regression for bug #47573
		[Test]
		public void RelativeCtor ()
		{
			Uri b = new Uri ("http://a/b/c/d;p?q");
			AssertEquals ("#1", "http://a/g", new Uri (b, "/g").ToString ());
			AssertEquals ("#2", "http://g/", new Uri (b, "//g").ToString ());
			AssertEquals ("#3", "http://a/b/c/?y", new Uri (b, "?y").ToString ());
			Assert ("#4", new Uri (b, "#s").ToString ().EndsWith ("#s"));

			Uri u = new Uri (b, "/g?q=r");
			AssertEquals ("#5", "http://a/g?q=r", u.ToString ());
			AssertEquals ("#6", "?q=r", u.Query);

			u = new Uri (b, "/g?q=r;. a");
			AssertEquals ("#5", "http://a/g?q=r;. a", u.ToString ());
			AssertEquals ("#6", "?q=r;.%20a", u.Query);
		}

#if NET_2_0
		[Test]
		[ExpectedException (typeof (UriFormatException))]
		public void RelativeCtor_11_Crasher ()
		{
			Uri b = new Uri ("http://a/b/c/d;p?q");
			// this causes crash under MS.NET 1.1
			AssertEquals ("g:h", "g:h", new Uri (b, "g:h").ToString ());
		}
#endif

		[Test]
		[ExpectedException (typeof (UriFormatException))]
		public void Bad_IPv6 ()
		{
			new Uri ("http://0:0:0:0::127.1.2.3]/");
		}

		[Test]
#if ONLY_1_1
		[Category ("NotDotNet")]
#endif
		public void LeadingSlashes_ShouldFailOn1x ()
		{
			// doesn't (but should) fail under 1.x
			AssertEquals ("#1", "file:///", new Uri ("file:///").ToString ());
			AssertEquals ("#2", "file:///", new Uri ("file://").ToString ());
		}

		[Test]
#if ONLY_1_1
		[Category ("NotWorking")]
#endif
		public void LeadingSlashes_BadResultsOn1x ()
		{
			// strange behaviours of 1.x - it's probably not worth to fix it
			// on Mono as 2.0 has been fixed
			Uri u = new Uri ("file:///foo/bar");
#if NET_2_0
			AssertEquals ("#3a", String.Empty, u.Host);
			AssertEquals ("#3b", UriHostNameType.Basic, u.HostNameType);
			AssertEquals ("#3c", "file:///foo/bar", u.ToString ());
			AssertEquals ("#3d", false, u.IsUnc);
#else
			// 1.x misinterpret the first path element as the host name
			AssertEquals ("#3a", "foo", u.Host);
			AssertEquals ("#3b", UriHostNameType.Dns, u.HostNameType);
			AssertEquals ("#3c", "file://foo/bar", u.ToString ());
			AssertEquals ("#3d", true, u.IsUnc);
#endif
			u = new Uri ("mailto:/foo");
#if NET_2_0
			AssertEquals ("#13a", String.Empty, u.Host);
			AssertEquals ("#13b", UriHostNameType.Basic, u.HostNameType);
			AssertEquals ("#13c", "mailto:/foo", u.ToString ());
#else
			// 1.x misinterpret the first path element as the host name
			AssertEquals ("#13a", "foo", u.Host);
			AssertEquals ("#13b", UriHostNameType.Dns, u.HostNameType);
			AssertEquals ("#13c", "mailto:foo", u.ToString ());
#endif
			u = new Uri ("mailto://foo");
#if NET_2_0
			AssertEquals ("#14a", String.Empty, u.Host);
			AssertEquals ("#14b", UriHostNameType.Basic, u.HostNameType);
			AssertEquals ("#14c", "mailto://foo", u.ToString ());
#else
			// 1.x misinterpret the first path element as the host name
			AssertEquals ("#14a", "foo", u.Host);
			AssertEquals ("#14b", UriHostNameType.Dns, u.HostNameType);
			AssertEquals ("#14c", "mailto://foo/", u.ToString ());
#endif
			u = new Uri ("news:/");
			AssertEquals ("#18a", String.Empty, u.Host);
#if NET_2_0
			AssertEquals ("#18b", UriHostNameType.Unknown, u.HostNameType);
			AssertEquals ("#18c", "news:/", u.ToString ());
			AssertEquals ("#18d", "/", u.AbsolutePath);
			AssertEquals ("#18e", "news:/", u.AbsoluteUri);
#else
			AssertEquals ("#18b", UriHostNameType.Basic, u.HostNameType);
			AssertEquals ("#18c", "news:", u.ToString ());
			AssertEquals ("#18d", String.Empty, u.AbsolutePath);
			AssertEquals ("#18e", "news:", u.AbsoluteUri);
#endif
			u = new Uri ("news:/foo");
			AssertEquals ("#19a", String.Empty, u.Host);
#if NET_2_0
			AssertEquals ("#19b", UriHostNameType.Unknown, u.HostNameType);
			AssertEquals ("#19c", "news:/foo", u.ToString ());
			AssertEquals ("#19d", "/foo", u.AbsolutePath);
			AssertEquals ("#19e", "news:/foo", u.AbsoluteUri);
#else
			AssertEquals ("#19b", UriHostNameType.Basic, u.HostNameType);
			AssertEquals ("#19c", "news:foo", u.ToString ());
			AssertEquals ("#19d", "foo", u.AbsolutePath);
			AssertEquals ("#19e", "news:foo", u.AbsoluteUri);
#endif
			u = new Uri ("news://foo");
#if NET_2_0
			AssertEquals ("#20a", String.Empty, u.Host);
			AssertEquals ("#20b", UriHostNameType.Unknown, u.HostNameType);
			AssertEquals ("#20c", "news://foo", u.ToString ());
			AssertEquals ("#20d", "//foo", u.AbsolutePath);
			AssertEquals ("#20e", "news://foo", u.AbsoluteUri);
#else
			AssertEquals ("#20a", "foo", u.Host);
			AssertEquals ("#20b", UriHostNameType.Dns, u.HostNameType);
			AssertEquals ("#20c", "news://foo/", u.ToString ());
			AssertEquals ("#20d", "/", u.AbsolutePath);
			AssertEquals ("#20e", "news://foo/", u.AbsoluteUri);
#endif
			u = new Uri ("news://foo/bar");
#if NET_2_0
			AssertEquals ("#22a", String.Empty, u.Host);
			AssertEquals ("#22b", UriHostNameType.Unknown, u.HostNameType);
			AssertEquals ("#22c", "news://foo/bar", u.ToString ());
			AssertEquals ("#22d", "//foo/bar", u.AbsolutePath);
			AssertEquals ("#22e", "news://foo/bar", u.AbsoluteUri);
#else
			AssertEquals ("#22a", "foo", u.Host);
			AssertEquals ("#22b", UriHostNameType.Dns, u.HostNameType);
			AssertEquals ("#22c", "news://foo/bar", u.ToString ());
			AssertEquals ("#22d", "/bar", u.AbsolutePath);
			AssertEquals ("#22e", "news://foo/bar", u.AbsoluteUri);
#endif
		}

		[Test]
#if ONLY_1_1
		[Category ("NotDotNet")] // does (but shouldn't) fail under 1.x
#endif
		public void LeadingSlashes_FailOn1x ()
		{
			// 1.x throws an UriFormatException because it can't decode the host name
			Uri u = new Uri ("mailto:");
			AssertEquals ("#10a", String.Empty, u.Host);
			AssertEquals ("#10b", UriHostNameType.Basic, u.HostNameType);
			AssertEquals ("#10c", "mailto:", u.ToString ());

			// 1.x throws an UriFormatException because it can't decode the host name
			u = new Uri ("mailto:/");
			AssertEquals ("#12a", String.Empty, u.Host);
			AssertEquals ("#12b", UriHostNameType.Basic, u.HostNameType);
			AssertEquals ("#12c", "mailto:/", u.ToString ());

			// 1.x throws an UriFormatException because it cannot detect the format
			u = new Uri ("mailto:///foo");
			AssertEquals ("#15a", String.Empty, u.Host);
			AssertEquals ("#15b", UriHostNameType.Basic, u.HostNameType);
			AssertEquals ("#15c", "mailto:///foo", u.ToString ());

			// 1.x throws an UriFormatException because it cannot detect the format
			u = new Uri ("news:///foo");
			AssertEquals ("#21a", String.Empty, u.Host);
#if NET_2_0
			AssertEquals ("#21b", UriHostNameType.Unknown, u.HostNameType);
#else
			AssertEquals ("#21b", UriHostNameType.Basic, u.HostNameType);
#endif
			AssertEquals ("#21c", "news:///foo", u.ToString ());
			AssertEquals ("#21d", "///foo", u.AbsolutePath);
			AssertEquals ("#21e", "news:///foo", u.AbsoluteUri);
		}

		[Test]
		// some tests from bug 75144
		public void LeadingSlashes ()
		{
			Uri u = new Uri ("file://foo/bar");
			AssertEquals ("#5a", "foo", u.Host);
			AssertEquals ("#5b", UriHostNameType.Dns, u.HostNameType);
			AssertEquals ("#5c", "file://foo/bar", u.ToString ());
			AssertEquals ("#5d", true, u.IsUnc);

			u = new Uri ("file:////foo/bar");
			AssertEquals ("#7a", "foo", u.Host);
			AssertEquals ("#7b", UriHostNameType.Dns, u.HostNameType);
			AssertEquals ("#7c", "file://foo/bar", u.ToString ());
			AssertEquals ("#7d", true, u.IsUnc); 

			AssertEquals ("#9", "file://foo/bar", new Uri ("file://///foo/bar").ToString());

			u = new Uri ("mailto:foo");
			AssertEquals ("#11a", "foo", u.Host);
			AssertEquals ("#11b", UriHostNameType.Dns, u.HostNameType);
			AssertEquals ("#11c", "mailto:foo", u.ToString ());

			u = new Uri ("news:");
			AssertEquals ("#16a", String.Empty, u.Host);
#if NET_2_0
			AssertEquals ("#16b", UriHostNameType.Unknown, u.HostNameType);
#else
			AssertEquals ("#16b", UriHostNameType.Basic, u.HostNameType);
#endif
			AssertEquals ("#16c", "news:", u.ToString ());

			u = new Uri ("news:foo");
			AssertEquals ("#17a", String.Empty, u.Host);
#if NET_2_0
			AssertEquals ("#17b", UriHostNameType.Unknown, u.HostNameType);
#else
			AssertEquals ("#17b", UriHostNameType.Basic, u.HostNameType);
#endif
			AssertEquals ("#17c", "news:foo", u.ToString ());
			AssertEquals ("#17d", "foo", u.AbsolutePath);
			AssertEquals ("#17e", "news:foo", u.AbsoluteUri);
		}

		[Test]
		[ExpectedException (typeof (UriFormatException))]
		public void HttpHostname1 ()
		{
			new Uri ("http:");
		}

		[Test]
		[ExpectedException (typeof (UriFormatException))]
#if ONLY_1_1
		[Category ("NotDotNet")] // doesn't fail under 1.x
#endif
		public void HttpHostname2 ()
		{
			new Uri ("http:a");
		}

		[Test]
		[ExpectedException (typeof (UriFormatException))]
		public void HttpHostname3 ()
		{
			new Uri ("http:/");
		}

		[Test]
		[ExpectedException (typeof (UriFormatException))]
#if ONLY_1_1
		[Category ("NotDotNet")] // doesn't fail under 1.x
#endif
		public void HttpHostname4 ()
		{
			new Uri ("http:/foo");
		}

		[Test]
		[ExpectedException (typeof (UriFormatException))]
		public void HttpHostname5 ()
		{
			new Uri ("http://");
		}

		[Test]
		[ExpectedException (typeof (UriFormatException))]
		public void HttpHostname6 ()
		{
			new Uri ("http:///");
		}

		[Test]
		[ExpectedException (typeof (UriFormatException))]
		public void HttpHostname7 ()
		{
			new Uri ("http:///foo");
		}

		[Test]
		[ExpectedException (typeof (UriFormatException))]
		public void InvalidFile1 ()
		{
			new Uri ("file:");
		}

		[Test]
		[ExpectedException (typeof (UriFormatException))]
		public void InvalidFile2 ()
		{
			new Uri ("file:/");
		}

		[Test]
		[ExpectedException (typeof (UriFormatException))]
		public void InvalidFile3 ()
		{
			new Uri ("file:/foo");
		}

		[Test]
		[ExpectedException (typeof (UriFormatException))]
		public void InvalidScheme ()
		{
			new Uri ("_:/");
		}

		[Test]
		public void ConstructorsRejectRelativePath ()
		{
			string [] reluris = new string [] {
				"readme.txt",
				"thisdir/childdir/file",
				"./testfile"
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
				AssertEquals ("#3b *nix", "/tmp/hello.txt", uri.LocalPath);
			AssertEquals ("#3c", "file", uri.Scheme);
			AssertEquals ("#3d", "cygwin", uri.Host);
			AssertEquals ("#3e", "/tmp/hello.txt", uri.AbsolutePath);

			uri = new Uri ("file://mymachine/cygwin/tmp/hello.txt");
			AssertEquals ("#4a", "file://mymachine/cygwin/tmp/hello.txt", uri.ToString ());
			if (isWin32)
				AssertEquals ("#4b win32", "\\\\mymachine\\cygwin\\tmp\\hello.txt", uri.LocalPath);
			else
				AssertEquals ("#4b *nix", "/cygwin/tmp/hello.txt", uri.LocalPath);
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

		[Test]
		public void LocalPath_FileHost ()
		{
			// Hmm, they should be regarded just as a host name, since all URIs are base on absolute path.
			Uri uri = new Uri("file://one_file.txt");
#if NET_2_0
			AssertEquals("#6a", "file://one_file.txt/", uri.ToString());
			AssertEquals ("#6e", "/", uri.AbsolutePath);
			AssertEquals ("#6f", "/", uri.PathAndQuery);
			AssertEquals ("#6g", "file://one_file.txt/", uri.GetLeftPart (UriPartial.Path));
#else
			AssertEquals("#6a", "file://one_file.txt", uri.ToString());
			AssertEquals("#6e", "", uri.AbsolutePath);
			AssertEquals ("#6f", "", uri.PathAndQuery);
			AssertEquals ("#6g", "file://one_file.txt", uri.GetLeftPart (UriPartial.Path));
#endif
			if (isWin32)
				AssertEquals("#6b", "\\\\one_file.txt", uri.LocalPath);
			else
				AssertEquals("#6b", "/", uri.LocalPath);
			AssertEquals("#6c", "file", uri.Scheme);
			AssertEquals("#6d", "one_file.txt", uri.Host);

			// same tests - but original Uri is now ending with a '/'

			uri = new Uri ("file://one_file.txt/");
			AssertEquals ("#7a", "file://one_file.txt/", uri.ToString ());
			AssertEquals ("#7e", "/", uri.AbsolutePath);
			AssertEquals ("#7f", "/", uri.PathAndQuery);
			AssertEquals ("#7g", "file://one_file.txt/", uri.GetLeftPart (UriPartial.Path));
#if !TARGET_JVM
			if (isWin32)
				AssertEquals ("#7b", "\\\\one_file.txt\\", uri.LocalPath);
			else
				AssertEquals ("#7b", "/", uri.LocalPath);
#endif
			AssertEquals ("#7c", "file", uri.Scheme);
			AssertEquals ("#7d", "one_file.txt", uri.Host);
		}

		[Test]
		public void LocalPath_Escape ()
		{
			// escape
			Uri uri = new Uri ("file:///tmp/a%20a");
			if (isWin32) {
#if NET_2_0
				Assert ("#7a:" + uri.LocalPath, uri.LocalPath.EndsWith ("/tmp/a a"));
#else
				// actually MS.NET treats /// as \\ thus it fails here.
				Assert ("#7a:" + uri.LocalPath, uri.LocalPath.EndsWith ("\\tmp\\a a"));
#endif
			} else
				AssertEquals ("#7b", "/tmp/a a", uri.LocalPath);

			uri = new Uri ("file:///tmp/foo%25bar");
			if (isWin32) {
#if NET_2_0
				Assert ("#8a:" + uri.LocalPath, uri.LocalPath.EndsWith ("/tmp/foo%bar"));
				Assert ("#8c:" + uri.ToString (), uri.ToString ().EndsWith ("//tmp/foo%25bar"));
#else
				// actually MS.NET treats /// as \\ thus it fails here.
				Assert ("#8a:" + uri.LocalPath, uri.LocalPath.EndsWith ("\\tmp\\foo%bar"));
				// ditto, file://tmp/foo%25bar (bug in 1.x)
				Assert ("#8c:" + uri.ToString (), uri.ToString ().EndsWith ("//tmp/foo%bar"));
#endif
			} else {
				AssertEquals ("#8b", "/tmp/foo%bar", uri.LocalPath);
				AssertEquals ("#8d", "file:///tmp/foo%25bar", uri.ToString ());
			}
			// bug #76643
			uri = new Uri ("file:///foo%25bar");
			if (isWin32) {
#if NET_2_0
				Assert ("#9a:" + uri.LocalPath, uri.LocalPath.EndsWith ("/foo%bar"));
#else
				// actually MS.NET treats /// as \\ thus it fails here.
				Assert ("#9a:" + uri.LocalPath, uri.LocalPath.EndsWith ("\\foo%25bar"));
#endif
				// ditto, file://tmp/foo%25bar (bug in 1.x)
				Assert ("#9c:" + uri.ToString (), uri.ToString ().EndsWith ("//foo%25bar"));
			} else {
				AssertEquals ("#9b", "/foo%bar", uri.LocalPath);
				AssertEquals ("#9d", "file:///foo%25bar", uri.ToString ());
			}
		}

		// Novell Bugzilla #320614
		[Test]
		public void QueryEscape ()
		{
			Uri u1 = new Uri("http://localhost:8080/test.aspx?ReturnUrl=%2fSearchDoc%2fSearcher.aspx");
			Uri u2 = new Uri("http://localhost:8080/test.aspx?ReturnUrl=%252fSearchDoc%252fSearcher.aspx");
			
			AssertEquals ("QE1", u1.ToString (), "http://localhost:8080/test.aspx?ReturnUrl=/SearchDoc/Searcher.aspx");
			AssertEquals ("QE2", u2.ToString (), "http://localhost:8080/test.aspx?ReturnUrl=%2fSearchDoc%2fSearcher.aspx");
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

		[Test]
		[Category("NotDotNet")]
		public void UncFail ()
		{
			if (!isWin32) {
				Uri uri = new Uri ("/home/user/dir/filename.ext");
				Assert ("#7", !uri.IsUnc);
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

		class UriEx : Uri
		{
			public UriEx (string s) : base (s)
			{
			}

			public string UnescapeString (string s)
			{
				return Unescape (s);
			}

			public static string UnescapeString (string uri, string target)
			{
				return new UriEx (uri).UnescapeString (target);
			}
		}

		[Test]
		public void Unescape ()
		{
			AssertEquals ("#1", "#", UriEx.UnescapeString ("file://localhost/c#", "%23"));
			AssertEquals ("#2", "c#", UriEx.UnescapeString ("file://localhost/c#", "c%23"));
			AssertEquals ("#3", "\xA9", UriEx.UnescapeString ("file://localhost/c#", "%A9"));
			AssertEquals ("#1", "#", UriEx.UnescapeString ("http://localhost/c#", "%23"));
			AssertEquals ("#2", "c#", UriEx.UnescapeString ("http://localhost/c#", "c%23"));
			AssertEquals ("#3", "\xA9", UriEx.UnescapeString ("http://localhost/c#", "%A9"));
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
        public void MoreHexEscape()
        {
            string url = "http://guyc-2003-sp/wiki/wiki%20document%20library/בד%20יקה.docx";
            string escapedAbsolutePath = "/wiki/wiki%20document%20library/%D7%91%D7%93%20%D7%99%D7%A7%D7%94.docx";
            Uri u = new Uri(url);
            AssertEquals("Escaped non-english combo", escapedAbsolutePath, u.AbsolutePath);
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

			// Tests from bug 74872 - don't handle multi-byte characters as multi-byte
			i = 1;
			AssertEquals ("#9", 227, (int) Uri.HexUnescape ("a%E3%81%8B", ref i));
			AssertEquals ("#10", 4, i);
			i = 1;
			AssertEquals ("#11", 240, (int) Uri.HexUnescape ("a%F0%90%84%80", ref i));
			AssertEquals ("#12", 4, i);
		}

#if !NET_2_0
		// These won't pass exactly with MS.NET 1.x, due to differences in the
		// handling of backslashes/forwardslashes
		[Category ("NotDotNet")]
#endif
		[Test]
		public void HexUnescapeMultiByte ()
		{
			// Tests from bug 74872
			// Note: These won't pass exactly with MS.NET, due to differences in the
			// handling of backslashes/forwardslashes
			Uri uri;
			string path;

			// 3-byte character
			uri = new Uri ("file:///foo/a%E3%81%8Bb", true);
			path = uri.LocalPath;
			AssertEquals ("#1", 8, path.Length);
			AssertEquals ("#2", 0x304B, path [6]);

			// 4-byte character which should be handled as a surrogate
			uri = new Uri ("file:///foo/a%F3%A0%84%80b", true);
			path = uri.LocalPath;
			AssertEquals ("#3", 9, path.Length);
			AssertEquals ("#4", 0xDB40, path [6]);
			AssertEquals ("#5", 0xDD00, path [7]);
			AssertEquals ("#6", 0x62, path [8]);
			
			// 2-byte escape sequence, 2 individual characters
			uri = new Uri ("file:///foo/a%C2%F8b", true);
			path = uri.LocalPath;
			AssertEquals ("#7", 9, path.Length);
			AssertEquals ("#8", 0xC2, path [6]);
			AssertEquals ("#9", 0xF8, path [7]);			
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
		public void NewsDefaultPort ()
		{
			Uri uri = new Uri("news://localhost:119/");
			AssertEquals ("#1", uri.IsDefaultPort, true);
		}

		[Test]
		public void Fragment_Escape ()
		{
			Uri u = new Uri("http://localhost/index.asp#main#start", false);
			AssertEquals ("#1", u.Fragment, "#main%23start");

			u = new Uri("http://localhost/index.asp#main#start", true);
			AssertEquals ("#2", u.Fragment, "#main#start");

			// The other code path uses a BaseUri

			Uri b = new Uri ("http://www.gnome.org");
			Uri n = new Uri (b, "blah#main#start");
			AssertEquals ("#3", n.Fragment, "#main%23start");
			
			n = new Uri (b, "blah#main#start", true);
			AssertEquals ("#4", n.Fragment, "#main#start");
		}

#if NET_2_0
		[Test]
		public void Fragment_RelativeUri ()
		{
			Uri uri1 = new Uri ("http://www.contoso.com/index.htm?x=2");
			Uri uri2 = new Uri ("http://www.contoso.com/foo/bar/index.htm#fragment");
			Uri relativeUri = uri1.MakeRelativeUri (uri2);

			try {
				string fragment = relativeUri.Fragment;
				Fail ("#1: " + fragment);
			} catch (InvalidOperationException ex) {
				// This operation is not supported for a relative URI
				AssertEquals ("#2", typeof (InvalidOperationException), ex.GetType ());
				AssertNull ("#3", ex.InnerException);
				AssertNotNull ("#4", ex.Message);
			}
		}
#endif

		[Test]
		[ExpectedException(typeof(UriFormatException))]
		public void IncompleteSchemeDelimiter ()
		{
			new Uri ("file:/filename.ext");
		}

		[Test]
		[Category("NotDotNet")]
		public void CheckHostName1 ()
		{
			// reported to MSDN Product Feedback Center (FDBK28671)
			AssertEquals ("#36 known to fail with ms.net: this is not a valid IPv6 address.", UriHostNameType.Unknown, Uri.CheckHostName (":11:22:33:44:55:66:77:88"));
		}

		[Test]
		public void CheckHostName2 ()
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
			AssertEquals ("#11", UriHostNameType.Dns, Uri.CheckHostName ("www.contoso.com."));
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
			AssertEquals ("#33", UriHostNameType.IPv6, Uri.CheckHostName ("2001::03A0:1.2.3.4"));

			AssertEquals ("#31", UriHostNameType.Unknown, Uri.CheckHostName ("2001::03A0::/35"));
			AssertEquals ("#32", UriHostNameType.Unknown, Uri.CheckHostName ("2001:03A0::/35a"));
			AssertEquals ("#34", UriHostNameType.Unknown, Uri.CheckHostName ("::ffff:123.256.155.43"));
			AssertEquals ("#35", UriHostNameType.Unknown, Uri.CheckHostName (":127.0.0.1"));
			AssertEquals ("#37", UriHostNameType.Unknown, Uri.CheckHostName ("::11:22:33:44:55:66:77:88"));
			AssertEquals ("#38", UriHostNameType.Unknown, Uri.CheckHostName ("11:22:33:44:55:66:77:88::"));
			AssertEquals ("#39", UriHostNameType.Unknown, Uri.CheckHostName ("11:22:33:44:55:66:77:88:"));
			AssertEquals ("#40", UriHostNameType.Unknown, Uri.CheckHostName ("::acbde"));
			AssertEquals ("#41", UriHostNameType.Unknown, Uri.CheckHostName ("::abce:"));
			AssertEquals ("#42", UriHostNameType.Unknown, Uri.CheckHostName ("::abcg"));
			AssertEquals ("#43", UriHostNameType.Unknown, Uri.CheckHostName (":::"));
			AssertEquals ("#44", UriHostNameType.Unknown, Uri.CheckHostName (":"));

			AssertEquals ("#45", UriHostNameType.Unknown, Uri.CheckHostName ("*"));
			AssertEquals ("#46", UriHostNameType.Unknown, Uri.CheckHostName ("*.go-mono.com"));
			AssertEquals ("#47", UriHostNameType.Unknown, Uri.CheckHostName ("www*.go-mono.com"));
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
			AssertEquals ("#10", false, uri.IsLoopback);
			uri = new Uri("http://[::ffff:127.11.22.33]:8080");
			AssertEquals ("#11", false, uri.IsLoopback);
			uri = new Uri("http://[::ff00:7f11:2233]:8080");
			AssertEquals ("#12", false, uri.IsLoopback);
			uri = new Uri("http://[1:0:0:0::1]:8080");
			AssertEquals ("#13", false, uri.IsLoopback);
		}

		[Test]
		public void IsLoopback_File ()
		{
			Uri uri = new Uri ("file:///index.html");
#if NET_2_0
			Assert ("file", uri.IsLoopback);
#else
			Assert ("file", !uri.IsLoopback);
#endif
		}

		[Test]
		public void IsLoopback_Relative_Http ()
		{
			string relative = "http:8080/bar/Hello World.htm";
			Uri uri = new Uri (new Uri ("http://www.contoso.com"), relative, false);
			Assert ("http", !uri.IsLoopback);
		}

		[Test]
		public void IsLoopback_Relative_Unknown ()
		{
			string relative = "foo:8080/bar/Hello World.htm";
			Uri uri = new Uri (new Uri ("http://www.contoso.com"), relative, false);
			Assert ("foo", !uri.IsLoopback);
		}

		[Test]
		public void Equals1 ()
		{
			Uri uri1 = new Uri ("http://www.contoso.com/index.htm#main");
			Uri uri2 = new Uri ("http://www.contoso.com/index.htm#fragment");
			Assert ("#1", uri1.Equals (uri2));
			Assert ("#3", !uri2.Equals ("http://www.contoso.com/index.html?x=1"));
			Assert ("#4", !uri1.Equals ("http://www.contoso.com:8080/index.htm?x=1"));
		}

		[Test]
#if !NET_2_0
		[Category("NotDotNet")]
#endif
		public void Equals2 ()
		{
			Uri uri1 = new Uri ("http://www.contoso.com/index.htm#main");
			Uri uri2 = new Uri ("http://www.contoso.com/index.htm?x=1");
			Assert ("#2 known to fail with ms.net 1.x", !uri1.Equals (uri2));
		}

		[Test]
		public void Equals3 ()
		{
			Uri uri1 = new Uri ("svn+ssh://atsushi@mono-cvs.ximian.com");
			Uri uri2 = new Uri ("svn+ssh://anonymous@mono-cvs.ximian.com");
			Assert (uri1.Equals (uri2));
		}

		[Test]
		public void TestEquals2 ()
		{
			Uri a = new Uri ("http://www.go-mono.com");
			Uri b = new Uri ("http://www.go-mono.com");

			AssertEquals ("#1", a, b);

			a = new Uri ("mailto:user:pwd@go-mono.com?subject=uri");
			b = new Uri ("MAILTO:USER:PWD@GO-MONO.COM?SUBJECT=URI");
#if NET_2_0
			Assert ("#2", a != b);
			AssertEquals ("#2a", "mailto:user:pwd@go-mono.com?subject=uri", a.ToString ());
			AssertEquals ("#2b", "mailto:USER:PWD@go-mono.com?SUBJECT=URI", b.ToString ());
#else
			AssertEquals ("#2", a, b);
#endif
			a = new Uri ("http://www.go-mono.com/ports/");
			b = new Uri ("http://www.go-mono.com/PORTS/");

			Assert ("#3", !a.Equals (b));
		}

		[Test]
		public void CaseSensitivity ()
		{
			Uri mailto = new Uri ("MAILTO:USER:PWD@GO-MONO.COM?SUBJECT=URI");
			AssertEquals ("#1", "mailto", mailto.Scheme);
			AssertEquals ("#2", "go-mono.com", mailto.Host);
			AssertEquals ("#3", "mailto:USER:PWD@go-mono.com?SUBJECT=URI", mailto.ToString ());

			Uri http = new Uri ("HTTP://GO-MONO.COM/INDEX.HTML");
			AssertEquals ("#4", "http", http.Scheme);
			AssertEquals ("#5", "go-mono.com", http.Host);
			AssertEquals ("#6", "http://go-mono.com/INDEX.HTML", http.ToString ());

			// IPv6 Address
			Uri ftp = new Uri ("FTP://[::ffFF:169.32.14.5]/");
			AssertEquals ("#7", "ftp", ftp.Scheme);
			AssertEquals ("#8", "[0000:0000:0000:0000:0000:FFFF:A920:0E05]", ftp.Host);
			AssertEquals ("#9", "ftp://[0000:0000:0000:0000:0000:FFFF:A920:0E05]/", ftp.ToString ());
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

#if NET_2_0
		[Test]
		public void RelativeEqualsTest()
		{
			Uri uri1 = new Uri ("foo/bar", UriKind.Relative);
			Uri uri2 = new Uri ("foo/bar", UriKind.Relative);
			Uri uri3 = new Uri ("bar/man", UriKind.Relative);
			Uri uri4 = new Uri ("BAR/MAN", UriKind.Relative);
			Assert ("#1a", uri1 == uri2);
			Assert ("#1b", uri1.Equals(uri2));
			Assert ("#2a", uri1 != uri3);
			Assert ("#2b", !uri1.Equals(uri3));
			Assert ("#3a", uri1 == uri2);
			Assert ("#3b", uri1.Equals(uri2));
			Assert ("#4a", uri1 != uri3);
			Assert ("#4b", !uri1.Equals(uri3));
			Assert ("#5a", uri3 != uri4);
			Assert ("#5b", !uri3.Equals(uri4));
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void GetLeftPart_Partial1 ()
		{
			Uri u = new Uri ("foo", UriKind.Relative);
			u.GetLeftPart (UriPartial.Scheme);
		}

		[ExpectedException(typeof(InvalidOperationException))]
		[Test]
		public void GetLeftPart_Partial2 ()
		{
			Uri u = new Uri ("foo", UriKind.Relative);
			u.GetLeftPart (UriPartial.Authority);
		}

		[ExpectedException(typeof(InvalidOperationException))]
		[Test]
		public void GetLeftPart_Partial3 ()
		{
			Uri u = new Uri ("foo", UriKind.Relative);
			u.GetLeftPart (UriPartial.Path);
		}

		[Test]
		public void TestPartialToString ()
		{
			AssertEquals ("#1", new Uri ("foo", UriKind.Relative).ToString (), "foo");
			AssertEquals ("#2", new Uri ("foo#aa", UriKind.Relative).ToString (), "foo#aa");
			AssertEquals ("#3", new Uri ("foo?aa", UriKind.Relative).ToString (), "foo?aa");
			AssertEquals ("#4", new Uri ("foo#dingus?aa", UriKind.Relative).ToString (), "foo#dingus?aa");
			AssertEquals ("#4", new Uri ("foo?dingus#aa", UriKind.Relative).ToString (), "foo?dingus#aa");
		}

		[Test]
		public void RelativeGetHashCodeTest()
		{
			Uri uri1 = new Uri ("foo/bar", UriKind.Relative);
			Uri uri2 = new Uri ("foo/bar", UriKind.Relative);
			Uri uri3 = new Uri ("bar/man", UriKind.Relative);
			AssertEquals ("#1", uri1.GetHashCode(), uri2.GetHashCode());
			Assert ("#2", uri1.GetHashCode() != uri3.GetHashCode());
		}
#endif

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
		public void RelativeUri ()
		{
			Uri u = new Uri("http://localhost/../../../a");
#if NET_2_0
			AssertEquals ("http://localhost/a", u.ToString ());
#else
			AssertEquals ("http://localhost/../../../a", u.ToString ());
#endif

			u = new Uri ("http://localhost/../c/b/../a");
#if NET_2_0
			AssertEquals ("http://localhost/c/a", u.ToString ());
#else
			AssertEquals ("http://localhost/../c/a", u.ToString ());
#endif
		}

		[Test]
		public void RelativeUri2 ()
		{
			AssertEquals ("#1", "hoge:ext", new Uri (new Uri ("hoge:foo:bar:baz"), "hoge:ext").ToString ());
			if (isWin32) {
				AssertEquals ("#2-w", "file:///d:/myhost/ext", new Uri (new Uri ("file:///c:/localhost/bar"), "file:///d:/myhost/ext").ToString ());
				AssertEquals ("#3-w", "file:///c:/localhost/myhost/ext", new Uri (new Uri ("file:///c:/localhost/bar"), "file:myhost/ext").ToString ());
				AssertEquals ("#4-w", "uuid:ext", new Uri (new Uri ("file:///c:/localhost/bar"), "uuid:ext").ToString ());
				AssertEquals ("#5-w", "file:///c:/localhost/ext", new Uri (new Uri ("file:///c:/localhost/bar"), "file:./ext").ToString ());
			} else {
				AssertEquals ("#2-u", "file:///d/myhost/ext", new Uri (new Uri ("file:///c/localhost/bar"), "file:///d/myhost/ext").ToString ());
				AssertEquals ("#3-u", "file:///c/localhost/myhost/ext", new Uri (new Uri ("file:///c/localhost/bar"), "file:myhost/ext").ToString ());
				AssertEquals ("#4-u", "uuid:ext", new Uri (new Uri ("file:///c/localhost/bar"), "uuid:ext").ToString ());
				AssertEquals ("#5-u", "file:///c/localhost/ext", new Uri (new Uri ("file:///c/localhost/bar"), "file:./ext").ToString ());
			}
			AssertEquals ("#6", "http://localhost/ext", new Uri (new Uri ("http://localhost/bar"), "http:./ext").ToString ());
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
#if NET_2_0
			Assert ("#11", !Uri.CheckSchemeName ("htt\u00E1+p6"));
#else
			Assert ("#11", Uri.CheckSchemeName ("htt\u00E1+p6"));
#endif
		}

		[Test]
		public void CheckSchemeName_FirstChar ()
		{
			for (int i = 0; i < 256; i++) {
				string s = String.Format ("#{0}", i);
				char c = (char) i;
				bool b = Uri.CheckSchemeName (c.ToString ());
#if NET_2_0
				bool valid = (((i >= 0x41) && (i <= 0x5A)) || ((i >= 0x61) && (i <= 0x7A)));
				AssertEquals (s, valid, b);
#else
				AssertEquals (s, Char.IsLetter (c), b);
#endif
			}
		}

		[Test]
		public void CheckSchemeName_AnyOtherChar ()
		{
			for (int i = 0; i < 256; i++) {
				string s = String.Format ("#{0}", i);
				char c = (char) i;
				string scheme = String.Format ("a+b-c.d{0}", c);
				bool b = Uri.CheckSchemeName (scheme);
				bool common = Char.IsDigit (c) || (c == '+') || (c == '-') || (c == '.');
#if NET_2_0
				bool valid = (common || ((i >= 0x41) && (i <= 0x5A)) || ((i >= 0x61) && (i <= 0x7A)));
				AssertEquals (s, valid, b);
#else
				AssertEquals (s, (Char.IsLetter (c) || common), b);
#endif
			}
		}

		[Test]
		[ExpectedException (typeof (UriFormatException))]
		public void NoHostname ()
		{
			Uri uri = new Uri ("http://");
		}

		[Test]
#if !NET_2_0
		// MS.NET 1.x throws an IndexOutOfRangeException
		[Category("NotDotNet")]
#endif
		public void NoHostname2 ()
		{
			// bug 75144
			Uri uri = new Uri ("file://");
			AssertEquals ("#1", true, uri.IsFile);
			AssertEquals ("#2", false, uri.IsUnc);
			AssertEquals ("#3", "file", uri.Scheme);
			AssertEquals ("#4", "/", uri.LocalPath);
			AssertEquals ("#5", string.Empty, uri.Query);
			AssertEquals ("#6", "/", uri.AbsolutePath);
			AssertEquals ("#7", "file:///", uri.AbsoluteUri);
			AssertEquals ("#8", string.Empty, uri.Authority);
			AssertEquals ("#9", string.Empty, uri.Host);
			AssertEquals ("#10", UriHostNameType.Basic, uri.HostNameType);
			AssertEquals ("#11", string.Empty, uri.Fragment);
			AssertEquals ("#12", true, uri.IsDefaultPort);
#if NET_2_0
			Assert ("#13", uri.IsLoopback);
#else
			Assert ("#13", !uri.IsLoopback);
#endif
			AssertEquals ("#14", "/", uri.PathAndQuery);
			AssertEquals ("#15", false, uri.UserEscaped);
			AssertEquals ("#16", string.Empty, uri.UserInfo);
			AssertEquals ("#17", "file://", uri.GetLeftPart (UriPartial.Authority));
			AssertEquals ("#18", "file:///", uri.GetLeftPart (UriPartial.Path));
			AssertEquals ("#19", "file://", uri.GetLeftPart (UriPartial.Scheme));
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
#if NET_2_0
		[Category ("NotWorking")]
#endif
		public void Segments4 ()
		{
			Uri uri = new Uri ("file:///c:/hello");
			string [] segments = uri.Segments;
			AssertEquals ("#01", 3, segments.Length);
#if NET_2_0
			AssertEquals ("#02", "/", segments [0]);
			AssertEquals ("#03", "c:/", segments[1]);
#else
			AssertEquals ("#02", "c:", segments [0]);
			AssertEquals ("#03", "/", segments [1]);
#endif
			AssertEquals ("#04", "hello", segments [2]);
		}

		[Test]
		public void Segments5 ()
		{
			Uri uri = new Uri ("http://www.xxx.com/bar/foo/foobar.htm?z=0&y=5" + (char) 0xa9);
			string [] segments = uri.Segments;
			AssertEquals ("#01", 4, segments.Length);
			AssertEquals ("#02", "/", segments [0]);
			AssertEquals ("#03", "bar/", segments [1]);
			AssertEquals ("#04", "foo/", segments [2]);
			AssertEquals ("#05", "foobar.htm", segments [3]);
		}

		[Test]
		[ExpectedException (typeof (UriFormatException))]
		public void UriStartingWithColon()
		{
			new Uri("://");
		}

		[Test]
		[ExpectedException (typeof (UriFormatException))]
		public void EmptyScheme ()
		{
			new Uri ("hey");
		}

#if NET_2_0
		// on .NET 2.0 a port number is limited to UInt16.MaxValue
		[ExpectedException (typeof (UriFormatException))]
#endif
		[Test]
		public void InvalidPort1 ()
		{
			Uri uri = new Uri ("http://www.contoso.com:65536/foo/bar/");
			AssertEquals (65536, uri.Port);
		}

#if NET_2_0
		[ExpectedException (typeof (UriFormatException))]
#endif
		[Test]
		public void InvalidPort2 ()
		{
			// UInt32.MaxValue gives port == -1 !!!
			Uri uri = new Uri ("http://www.contoso.com:4294967295/foo/bar/");
			AssertEquals (-1, uri.Port);
		}

#if NET_2_0
		[ExpectedException (typeof (UriFormatException))]
#endif
		[Test]
		public void InvalidPort3 ()
		{
			// ((uint) Int32.MaxValue + (uint) 1) gives port == -2147483648 !!!
			Uri uri = new Uri ("http://www.contoso.com:2147483648/foo/bar/");
			AssertEquals (-2147483648, uri.Port);
		}

#if NET_2_0
		[Test]
		public void PortMax ()
		{
			// on .NET 2.0 a port number is limited to UInt16.MaxValue
			Uri uri = new Uri ("http://www.contoso.com:65535/foo/bar/");
			AssertEquals (65535, uri.Port);
		}
#endif

		class UriEx2 : Uri
		{
			public UriEx2 (string s) : base (s)
			{
			}

			protected override void Parse ()
			{
			}
		}

#if NET_2_0
		// Parse method is no longer used on .NET 2.0
		[ExpectedException (typeof (UriFormatException))]
#endif
		[Test]
		public void ParseOverride ()
		{
			// If this does not override base's Parse(), it will
			// fail since this argument is not Absolute URI.
			UriEx2 ex = new UriEx2 ("readme.txt");
		}

		[Test]
		public void UnixLocalPath ()
		{
			// This works--the location is not part of the absolute path
			string path = "file://localhost/tmp/foo/bar";
			Uri fileUri = new Uri( path );
			AssertEquals (path, "/tmp/foo/bar", fileUri.AbsolutePath);

			// Empty path == localhost, in theory
			path = "file:///c:/tmp/foo/bar";
			fileUri = new Uri( path );
			AssertEquals (path, "c:/tmp/foo/bar", fileUri.AbsolutePath);
		}

#if NET_2_0
		[Test]
		public void TestEscapeDataString ()
		{
			StringBuilder sb = new StringBuilder ();

			for (int i = 0; i < 128; i++)
				sb.Append ((char) i);
			
			AssertEquals ("%00%01%02%03%04%05%06%07%08%09%0A%0B%0C%0D%0E%0F%10%11%12%13%14%15%16%17%18%19%1A%1B%1C%1D%1E%1F%20!%22%23%24%25%26'()*%2B%2C-.%2F0123456789%3A%3B%3C%3D%3E%3F%40ABCDEFGHIJKLMNOPQRSTUVWXYZ%5B%5C%5D%5E_%60abcdefghijklmnopqrstuvwxyz%7B%7C%7D~%7F",
				      Uri.EscapeDataString (sb.ToString ()));

			AssertEquals ("%C3%A1", Uri.EscapeDataString ("á"));
		}
		[Test]
		public void TestEscapeUriString ()
		{
			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < 128; i++)
				sb.Append ((char) i);
			
			AssertEquals ("%00%01%02%03%04%05%06%07%08%09%0A%0B%0C%0D%0E%0F%10%11%12%13%14%15%16%17%18%19%1A%1B%1C%1D%1E%1F%20!%22#$%25&'()*+,-./0123456789:;%3C=%3E?@ABCDEFGHIJKLMNOPQRSTUVWXYZ%5B%5C%5D%5E_%60abcdefghijklmnopqrstuvwxyz%7B%7C%7D~%7F",
				Uri.EscapeUriString (sb.ToString ()));
			AssertEquals ("%C3%A1", Uri.EscapeDataString ("á"));
		}
#endif

		//bnc #363320
		[Test]
		public void TestUTF8Strings ()
		{
			string [] tests = {
				"file:///tmp/x (%232).jpg",
				"file:///tmp/ü (%232).jpg" };

			foreach (string test in tests)
				AssertEquals (test, new Uri (test).ToString ());
		}

		// This test doesn't work on Linux, and arguably shouldn't work.
		// new Uri("file:///tmp/foo/bar").AbsolutePath returns "/tmp/foo/bar"
		// on Linux, as anyone sane would expect.  It *doesn't* under .NET 1.1
		// Apparently "tmp" is supposed to be a hostname (!)...
		// Since "correct" behavior would confuse all Linux developers, and having
		// an expected failure is evil, we'll just ignore this for now...
		//
		// Furthermore, Microsoft fixed this so it behaves sensibly in .NET 2.0.
		//
		// You are surrounded by conditional-compilation code, all alike.
		// You are likely to be eaten by a Grue...
		[Test]
#if !NET_2_0
		[Category ("NotDotNet")]
#endif
		public void UnixLocalPath_WTF ()
		{
			// Empty path == localhost, in theory
			string path = "file:///tmp/foo/bar";
			Uri fileUri = new Uri( path );
//#if NET_2_0
			AssertEquals (path, "/tmp/foo/bar", fileUri.AbsolutePath);
//#else
//			AssertEquals (path, "/foo/bar", fileUri.AbsolutePath);
//#endif

			// bug #76643
			string path2 = "file:///foo%25bar";
			fileUri = new Uri (path2);
			AssertEquals (path2, "file:///foo%25bar", fileUri.ToString ());
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
			Console.WriteLine ("PathAndQuery	: " + uri.PathAndQuery);
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

//BNC#533572
#if NET_2_0
		[Test]
		public void LocalPath_FileNameWithAtSign1 ()
		{
			string path = "/some/path/file_with_an_@_sign.mp3";
			string fullpath = "http://thehost" + path;
			Uri fileUri = new Uri (fullpath);

			NAssert.AreEqual (fileUri.UserInfo, String.Empty, "LocalPath_FileNameWithAtSign UserInfo");
			NAssert.AreEqual (fileUri.Host, "thehost", "LocalPath_FileNameWithAtSign Host");
			NAssert.IsFalse (fileUri.IsFile, "LocalPath_FileNameWithAtSign IsFile");
			NAssert.IsTrue (fileUri.IsAbsoluteUri, "LocalPath_FileNameWithAtSign IsAbsUri");
			NAssert.IsFalse (fileUri.IsUnc, "LocalPath_FileNameWithAtSign IsUnc");

			NAssert.AreEqual (fullpath, fileUri.OriginalString, "LocalPath_FileNameWithAtSign OriginalString");
			NAssert.AreEqual (path, new DerivedUri (fullpath).TestUnescape (path), "LocalPath_FileNameWithAtSign ProtectedUnescape");
			NAssert.AreEqual (path, fileUri.AbsolutePath, "LocalPath_FileNameWithAtSign AbsPath");
			NAssert.AreEqual (path, fileUri.LocalPath, "LocalPath_FileNameWithAtSign LocalPath");
		}

		[Test]
		public void LocalPath_FileNameWithAtSign2 ()
		{
			string path = "/some/path/file_with_an_@_sign.mp3";
			string fullpath = "http://user:password@thehost" + path;
			Uri fileUri = new Uri (fullpath);

			NAssert.AreEqual (fileUri.UserInfo, "user:password", "LocalPath_FileNameWithAtSign UserInfo");
			NAssert.AreEqual (fileUri.Host, "thehost", "LocalPath_FileNameWithAtSign Host");
			NAssert.IsFalse (fileUri.IsFile, "LocalPath_FileNameWithAtSign IsFile");
			NAssert.IsTrue (fileUri.IsAbsoluteUri, "LocalPath_FileNameWithAtSign IsAbsUri");
			NAssert.IsFalse (fileUri.IsUnc, "LocalPath_FileNameWithAtSign IsUnc");

			NAssert.AreEqual (fullpath, fileUri.OriginalString, "LocalPath_FileNameWithAtSign OriginalString");
			NAssert.AreEqual (path, new DerivedUri (fullpath).TestUnescape (path), "LocalPath_FileNameWithAtSign ProtectedUnescape");
			NAssert.AreEqual (path, fileUri.AbsolutePath, "LocalPath_FileNameWithAtSign AbsPath");
			NAssert.AreEqual (path, fileUri.LocalPath, "LocalPath_FileNameWithAtSign LocalPath");
		}

		[Test]
		public void LocalPath_FileNameWithAtSign3 ()
		{
			string path = "/some/path/file_with_an_@_sign.mp3";
			string fullpath = "file://" + path;
			Uri fileUri = new Uri (fullpath);

			NAssert.AreEqual (fileUri.UserInfo, String.Empty, "LocalPath_FileNameWithAtSign UserInfo");
			NAssert.AreEqual (fileUri.Host, String.Empty, "LocalPath_FileNameWithAtSign Host");
			NAssert.IsTrue (fileUri.IsFile, "LocalPath_FileNameWithAtSign IsFile");
			NAssert.IsTrue (fileUri.IsAbsoluteUri, "LocalPath_FileNameWithAtSign IsAbsUri");
			NAssert.IsFalse (fileUri.IsUnc, "LocalPath_FileNameWithAtSign IsUnc");

			NAssert.AreEqual (fullpath, fileUri.OriginalString, "LocalPath_FileNameWithAtSign OriginalString");
			NAssert.AreEqual (path, new DerivedUri (fullpath).TestUnescape(path), "LocalPath_FileNameWithAtSign ProtectedUnescape");
			NAssert.AreEqual (path, fileUri.AbsolutePath, "LocalPath_FileNameWithAtSign AbsPath");
			NAssert.AreEqual (path, fileUri.LocalPath, "LocalPath_FileNameWithAtSign LocalPath");
		}

		[Test]
		public void LocalPath_FileNameWithAtSign4 ()
		{
			string path = "/some/path/file_with_an_@_sign.mp3";
			string fullpath = "file://localhost" + path;
			Uri fileUri = new Uri (fullpath);

			NAssert.AreEqual (fileUri.UserInfo, String.Empty, "LocalPath_FileNameWithAtSign UserInfo");
			NAssert.AreEqual (fileUri.Host, "localhost", "LocalPath_FileNameWithAtSign Host");
			NAssert.IsTrue (fileUri.IsFile, "LocalPath_FileNameWithAtSign IsFile");
			NAssert.IsTrue (fileUri.IsAbsoluteUri, "LocalPath_FileNameWithAtSign IsAbsUri");
			NAssert.IsTrue (fileUri.IsUnc, "LocalPath_FileNameWithAtSign IsUnc");

			NAssert.AreEqual (fullpath, fileUri.OriginalString, "LocalPath_FileNameWithAtSign OriginalString");
			NAssert.AreEqual (path, new DerivedUri (fullpath).TestUnescape (path), "LocalPath_FileNameWithAtSign ProtectedUnescape");
			NAssert.AreEqual (path, fileUri.AbsolutePath, "LocalPath_FileNameWithAtSign AbsPath");
			//this test is marked as NotWorking below:
			//NAssert.AreEqual ("\\\\localhost" + path.Replace ("/", "\\"), fileUri.LocalPath, "LocalPath_FileNameWithAtSign LocalPath");
		}

		[Test]
		[Category ("NotWorking")]
		public void LocalPath_FileNameWithAtSign5 ()
		{
			string path = "/some/path/file_with_an_@_sign.mp3";
			string fullpath = "file://localhost" + path;
			Uri fileUri = new Uri (fullpath);

			NAssert.AreEqual ("\\\\localhost" + path.Replace ("/", "\\"), fileUri.LocalPath, "LocalPath_FileNameWithAtSign LocalPath");
		}

		[Test]
		[Category ("NotWorking")] // MS.NET seems not to like userinfo in a file:// uri...
		[ExpectedException (typeof (UriFormatException))]
		public void LocalPath_FileNameWithAtSign6 ()
		{
			string path = "/some/path/file_with_an_@_sign.mp3";
			string fullpath = "file://user:password@localhost" + path;
			Uri fileUri = new Uri (fullpath);
		}


		public class DerivedUri : Uri
		{
			public DerivedUri (string uriString) : base (uriString)
			{
			}

			internal string TestUnescape (string path)
			{
				return base.Unescape (path);
			}
		}
#endif
	}
}
