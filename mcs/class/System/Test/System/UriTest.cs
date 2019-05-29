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

using System.Reflection;
using NUnit.Framework;
using System;
using System.IO;
using System.Text;

namespace MonoTests.System
{
	[TestFixture]
	public class UriTest
	{
		bool isWin32;

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
			Assert.AreEqual ("http://test.com/", uri.ToString(), "#k0");
			Assert.AreEqual ("http", uri.GetComponents (UriComponents.Scheme, UriFormat.UriEscaped), "#k0-gc");

			uri = new Uri ("http://contoso.com?subject=uri");
			Assert.AreEqual ("/", uri.AbsolutePath, "#k1");
			Assert.AreEqual ("http://contoso.com/?subject=uri", uri.AbsoluteUri, "#k2");
			Assert.AreEqual ("contoso.com", uri.Authority, "#k3");
			Assert.AreEqual ("", uri.Fragment, "#k4");
			Assert.AreEqual ("contoso.com", uri.Host, "#k5");
			Assert.AreEqual (UriHostNameType.Dns, uri.HostNameType, "#k6");
			Assert.AreEqual (true, uri.IsDefaultPort, "#k7");
			Assert.AreEqual (false, uri.IsFile, "#k8");
			Assert.AreEqual (false, uri.IsLoopback, "#k9");
			Assert.AreEqual (false, uri.IsUnc, "#k10");
			Assert.AreEqual ("/", uri.LocalPath, "#k11");
			Assert.AreEqual ("/?subject=uri", uri.PathAndQuery, "#k12");
			Assert.AreEqual (80, uri.Port, "#k13");
			Assert.AreEqual ("?subject=uri", uri.Query, "#k14");
			Assert.AreEqual ("http", uri.Scheme, "#k15");
			Assert.AreEqual (false, uri.UserEscaped, "#k16");
			Assert.AreEqual ("", uri.UserInfo, "#k17");

			uri = new Uri ("mailto:user:pwd@contoso.com?subject=uri");
			Assert.AreEqual ("", uri.AbsolutePath, "#m1");
			Assert.AreEqual ("mailto:user:pwd@contoso.com?subject=uri", uri.AbsoluteUri, "#m2");
			Assert.AreEqual ("contoso.com", uri.Authority, "#m3");
			Assert.AreEqual ("", uri.Fragment, "#m4");
			Assert.AreEqual ("contoso.com", uri.Host, "#m5");
			Assert.AreEqual (UriHostNameType.Dns, uri.HostNameType, "#m6");
			Assert.AreEqual (true, uri.IsDefaultPort, "#m7");
			Assert.AreEqual (false, uri.IsFile, "#m8");
			Assert.AreEqual (false, uri.IsLoopback, "#m9");
			Assert.AreEqual (false, uri.IsUnc, "#m10");
			Assert.AreEqual ("", uri.LocalPath, "#m11");
			Assert.AreEqual ("?subject=uri", uri.PathAndQuery, "#m12");
			Assert.AreEqual (25, uri.Port, "#m13");
			Assert.AreEqual ("?subject=uri", uri.Query, "#m14");
			Assert.AreEqual ("mailto", uri.Scheme, "#m15");
			Assert.AreEqual (false, uri.UserEscaped, "#m16");
			Assert.AreEqual ("user:pwd", uri.UserInfo, "#m17");

			uri = new Uri("myscheme://127.0.0.1:5");
			Assert.AreEqual ("myscheme://127.0.0.1:5/", uri.ToString(), "#c1");

			uri = new Uri (@"\\myserver\mydir\mysubdir\myfile.ext");
			Assert.AreEqual ("/mydir/mysubdir/myfile.ext", uri.AbsolutePath, "#n1");
			Assert.AreEqual ("file://myserver/mydir/mysubdir/myfile.ext", uri.AbsoluteUri, "#n2");
			Assert.AreEqual ("myserver", uri.Authority, "#n3");
			Assert.AreEqual ("", uri.Fragment, "#n4");
			Assert.AreEqual ("myserver", uri.Host, "#n5");
			Assert.AreEqual (UriHostNameType.Dns, uri.HostNameType, "#n6");
			Assert.AreEqual (true, uri.IsDefaultPort, "#n7");
			Assert.AreEqual (true, uri.IsFile, "#n8");
			Assert.AreEqual (false, uri.IsLoopback, "#n9");
			Assert.AreEqual (true, uri.IsUnc, "#n10");

			if (isWin32)
				Assert.AreEqual (@"\\myserver\mydir\mysubdir\myfile.ext", uri.LocalPath, "#n11");
			else
				// myserver never could be the part of Unix path.
				Assert.AreEqual ("/mydir/mysubdir/myfile.ext", uri.LocalPath, "#n11");

			Assert.AreEqual ("/mydir/mysubdir/myfile.ext", uri.PathAndQuery, "#n12");
			Assert.AreEqual (-1, uri.Port, "#n13");
			Assert.AreEqual ("", uri.Query, "#n14");
			Assert.AreEqual ("file", uri.Scheme, "#n15");
			Assert.AreEqual (false, uri.UserEscaped, "#n16");
			Assert.AreEqual ("", uri.UserInfo, "#n17");

			uri = new Uri (new Uri("http://www.contoso.com"), "Hello World.htm", true);
			Assert.AreEqual ("http://www.contoso.com/Hello World.htm", uri.AbsoluteUri, "#rel1a");
			Assert.AreEqual (true, uri.UserEscaped, "#rel1b");
			uri = new Uri (new Uri("http://www.contoso.com"), "Hello World.htm", false);
			Assert.AreEqual ("http://www.contoso.com/Hello%20World.htm", uri.AbsoluteUri, "#rel2a");
			Assert.AreEqual (false, uri.UserEscaped, "#rel2b");
			uri = new Uri (new Uri("http://www.contoso.com"), "http://www.xxx.com/Hello World.htm", false);
			Assert.AreEqual ("http://www.xxx.com/Hello%20World.htm", uri.AbsoluteUri, "#rel3");

			uri = new Uri (new Uri("http://www.contoso.com"), "foo/bar/Hello World.htm?x=0:8", false);
			Assert.AreEqual ("http://www.contoso.com/foo/bar/Hello%20World.htm?x=0:8", uri.AbsoluteUri, "#rel5");
			uri = new Uri (new Uri("http://www.contoso.com/xxx/yyy/index.htm"), "foo/bar/Hello World.htm?x=0:8", false);
			Assert.AreEqual ("http://www.contoso.com/xxx/yyy/foo/bar/Hello%20World.htm?x=0:8", uri.AbsoluteUri, "#rel6");
			uri = new Uri (new Uri("http://www.contoso.com/xxx/yyy/index.htm"), "/foo/bar/Hello World.htm?x=0:8", false);
			Assert.AreEqual ("http://www.contoso.com/foo/bar/Hello%20World.htm?x=0:8", uri.AbsoluteUri, "#rel7");
			uri = new Uri (new Uri("http://www.contoso.com/xxx/yyy/index.htm"), "../foo/bar/Hello World.htm?x=0:8", false);
			Assert.AreEqual ("http://www.contoso.com/xxx/foo/bar/Hello%20World.htm?x=0:8", uri.AbsoluteUri, "#rel8");
			uri = new Uri (new Uri("http://www.contoso.com/xxx/yyy/index.htm"), "../../../foo/bar/Hello World.htm?x=0:8", false);
			Assert.AreEqual ("http://www.contoso.com/foo/bar/Hello%20World.htm?x=0:8", uri.AbsoluteUri, "#rel9");
			Assert.AreEqual ("/foo/bar/Hello%20World.htm", uri.AbsolutePath, "#rel9-path");

			uri = new Uri (new Uri("http://www.contoso.com/xxx/yyy/index.htm"), "./foo/bar/Hello World.htm?x=0:8", false);
			Assert.AreEqual ("http://www.contoso.com/xxx/yyy/foo/bar/Hello%20World.htm?x=0:8", uri.AbsoluteUri, "#rel10");

			uri = new Uri (new Uri("http://www.contoso.com/foo/bar/index.html?x=0"), String.Empty, false);
			Assert.AreEqual ("http://www.contoso.com/foo/bar/index.html?x=0", uri.ToString (), "#22");

			uri = new Uri (new Uri("http://www.xxx.com"), "?x=0");
			Assert.AreEqual ("http://www.xxx.com/?x=0", uri.ToString(), "#rel30");
			uri = new Uri (new Uri("http://www.xxx.com/index.htm"), "#here");
			Assert.AreEqual ("http://www.xxx.com/index.htm#here", uri.ToString(), "#rel32");

			uri = new Uri ("relative", UriKind.Relative);
			uri = new Uri ("relative/abc", UriKind.Relative);
			uri = new Uri ("relative", UriKind.RelativeOrAbsolute);

			Assert.IsTrue (!uri.IsAbsoluteUri, "#rel33");
			Assert.AreEqual (uri.OriginalString, "relative", "#rel34");
			Assert.IsTrue (!uri.UserEscaped, "#rel35");
		}

		[Test]
		public void Constructor_DualHostPort ()
		{
			string relative = "foo:8080/bar/Hello World.htm";
			Uri uri = new Uri (new Uri("http://www.contoso.com"), relative, false);
			Assert.AreEqual ("8080/bar/Hello%20World.htm", uri.AbsolutePath, "AbsolutePath");
			Assert.AreEqual ("foo:8080/bar/Hello%20World.htm", uri.AbsoluteUri, "AbsoluteUri");
			Assert.AreEqual (String.Empty, uri.Authority, "Authority");
			Assert.AreEqual (String.Empty, uri.Fragment, "Fragment");
			Assert.AreEqual (String.Empty, uri.Host, "Host");
			Assert.AreEqual ("8080/bar/Hello%20World.htm", uri.PathAndQuery, "PathAndQuery");
			Assert.AreEqual (-1, uri.Port, "Port");
			Assert.AreEqual (String.Empty, uri.Query, "Query");
			Assert.AreEqual ("foo", uri.Scheme, "Scheme");
			Assert.AreEqual (String.Empty, uri.UserInfo, "Query");

			Assert.AreEqual ("8080/", uri.Segments[0], "Segments[0]");
			Assert.AreEqual ("bar/", uri.Segments[1], "Segments[1]");
			Assert.AreEqual ("Hello%20World.htm", uri.Segments[2], "Segments[2]");

			Assert.IsTrue (uri.IsDefaultPort, "IsDefaultPort");
			Assert.IsTrue (!uri.IsFile, "IsFile");
			Assert.IsTrue (!uri.IsLoopback, "IsLoopback");
			Assert.IsTrue (!uri.IsUnc, "IsUnc");
			Assert.IsTrue (!uri.UserEscaped, "UserEscaped");

			Assert.AreEqual (UriHostNameType.Unknown, uri.HostNameType, "HostNameType");
			Assert.IsTrue (uri.IsAbsoluteUri, "IsAbsoluteUri");
			Assert.AreEqual (relative, uri.OriginalString, "OriginalString");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_NullStringBool ()
		{
			new Uri (null, "http://www.contoso.com/index.htm", false);
		}

		[Test]
		public void Constructor_UriNullBool ()
		{
			new Uri (new Uri ("http://www.contoso.com"), null, false);
		}

		// regression for bug #47573
		[Test]
		public void RelativeCtor ()
		{
			Uri b = new Uri ("http://a/b/c/d;p?q");
			Assert.AreEqual ("http://a/g", new Uri (b, "/g").ToString (), "#1");
			Assert.AreEqual ("http://g/", new Uri (b, "//g").ToString (), "#2");
			Assert.IsTrue (new Uri (b, "#s").ToString ().EndsWith ("#s"), "#4");

			Uri u = new Uri (b, "/g?q=r");
			Assert.AreEqual ("http://a/g?q=r", u.ToString (), "#5");
			Assert.AreEqual ("?q=r", u.Query, "#6");

			u = new Uri (b, "/g?q=r;. a");
			Assert.AreEqual ("http://a/g?q=r;. a", u.ToString (), "#5");
			Assert.AreEqual ("?q=r;.%20a", u.Query, "#6");
		}


		[Test]
		[ExpectedException (typeof (UriFormatException))]
		public void RelativeCtor_11_Crasher ()
		{
			Uri b = new Uri ("http://a/b/c/d;p?q");
			// this causes crash under MS.NET 1.1
			Assert.AreEqual ("g:h", new Uri (b, "g:h").ToString (), "g:h");
		}

		[Test]
		[ExpectedException (typeof (UriFormatException))]
		public void Bad_IPv6 ()
		{
			new Uri ("http://0:0:0:0::127.1.2.3]/");
		}

		[Test]
		public void LeadingSlashes_ShouldFailOn1x ()
		{
			// doesn't (but should) fail under 1.x
			Assert.AreEqual ("file:///", new Uri ("file:///").ToString (), "#1");
			Assert.AreEqual ("file:///", new Uri ("file://").ToString (), "#2");
		}

		[Test]
		public void LeadingSlashes_BadResultsOn1x ()
		{
			// strange behaviours of 1.x - it's probably not worth to fix it
			// on Mono as 2.0 has been fixed
			Uri u = new Uri ("file:///foo/bar");
			Assert.AreEqual (String.Empty, u.Host, "#3a");
			Assert.AreEqual (UriHostNameType.Basic, u.HostNameType, "#3b");
			Assert.AreEqual ("file:///foo/bar", u.ToString (), "#3c");
			Assert.AreEqual (false, u.IsUnc, "#3d");

			u = new Uri ("mailto:/foo");
			Assert.AreEqual (String.Empty, u.Host, "#13a");
			Assert.AreEqual (UriHostNameType.Basic, u.HostNameType, "#13b");
			Assert.AreEqual ("mailto:/foo", u.ToString (), "#13c");

			u = new Uri ("mailto://foo");
			Assert.AreEqual (String.Empty, u.Host, "#14a");
			Assert.AreEqual (UriHostNameType.Basic, u.HostNameType, "#14b");
			Assert.AreEqual ("mailto://foo", u.ToString (), "#14c");

			u = new Uri ("news:/");
			Assert.AreEqual (String.Empty, u.Host, "#18a");
			Assert.AreEqual (UriHostNameType.Unknown, u.HostNameType, "#18b");
			Assert.AreEqual ("news:/", u.ToString (), "#18c");
			Assert.AreEqual ("/", u.AbsolutePath, "#18d");
			Assert.AreEqual ("news:/", u.AbsoluteUri, "#18e");

			u = new Uri ("news:/foo");
			Assert.AreEqual (String.Empty, u.Host, "#19a");
			Assert.AreEqual (UriHostNameType.Unknown, u.HostNameType, "#19b");
			Assert.AreEqual ("news:/foo", u.ToString (), "#19c");
			Assert.AreEqual ("/foo", u.AbsolutePath, "#19d");
			Assert.AreEqual ("news:/foo", u.AbsoluteUri, "#19e");

			u = new Uri ("news://foo");
			Assert.AreEqual (String.Empty, u.Host, "#20a");
			Assert.AreEqual (UriHostNameType.Unknown, u.HostNameType, "#20b");
			Assert.AreEqual ("news://foo", u.ToString (), "#20c");
			Assert.AreEqual ("//foo", u.AbsolutePath, "#20d");
			Assert.AreEqual ("news://foo", u.AbsoluteUri, "#20e");

			u = new Uri ("news://foo/bar");
			Assert.AreEqual (String.Empty, u.Host, "#22a");
			Assert.AreEqual (UriHostNameType.Unknown, u.HostNameType, "#22b");
			Assert.AreEqual ("news://foo/bar", u.ToString (), "#22c");
			Assert.AreEqual ("//foo/bar", u.AbsolutePath, "#22d");
			Assert.AreEqual ("news://foo/bar", u.AbsoluteUri, "#22e");
		}

		[Test]
		public void LeadingSlashes_FailOn1x ()
		{
			// 1.x throws an UriFormatException because it can't decode the host name
			Uri u = new Uri ("mailto:");
			Assert.AreEqual (String.Empty, u.Host, "#10a");
			Assert.AreEqual (UriHostNameType.Basic, u.HostNameType, "#10b");
			Assert.AreEqual ("mailto:", u.ToString (), "#10c");

			// 1.x throws an UriFormatException because it can't decode the host name
			u = new Uri ("mailto:/");
			Assert.AreEqual (String.Empty, u.Host, "#12a");
			Assert.AreEqual (UriHostNameType.Basic, u.HostNameType, "#12b");
			Assert.AreEqual ("mailto:/", u.ToString (), "#12c");

			// 1.x throws an UriFormatException because it cannot detect the format
			u = new Uri ("mailto:///foo");
			Assert.AreEqual (String.Empty, u.Host, "#15a");
			Assert.AreEqual (UriHostNameType.Basic, u.HostNameType, "#15b");
			Assert.AreEqual ("mailto:///foo", u.ToString (), "#15c");

			// 1.x throws an UriFormatException because it cannot detect the format
			u = new Uri ("news:///foo");
			Assert.AreEqual (String.Empty, u.Host, "#21a");
			Assert.AreEqual (UriHostNameType.Unknown, u.HostNameType, "#21b");
			Assert.AreEqual ("news:///foo", u.ToString (), "#21c");
			Assert.AreEqual ("///foo", u.AbsolutePath, "#21d");
			Assert.AreEqual ("news:///foo", u.AbsoluteUri, "#21e");
		}

		[Test]
		// some tests from bug 75144
		public void LeadingSlashes ()
		{
			Uri u = new Uri ("file://foo/bar");
			Assert.AreEqual ("foo", u.Host, "#5a");
			Assert.AreEqual (UriHostNameType.Dns, u.HostNameType, "#5b");
			Assert.AreEqual ("file://foo/bar", u.ToString (), "#5c");
			Assert.AreEqual (isWin32, u.IsUnc, "#5d");

			u = new Uri ("file:////foo/bar");
			Assert.AreEqual ("foo", u.Host, "#7a");
			Assert.AreEqual (UriHostNameType.Dns, u.HostNameType, "#7b");
			Assert.AreEqual ("file://foo/bar", u.ToString (), "#7c");
			Assert.AreEqual (isWin32, u.IsUnc, "#7d");

			Assert.AreEqual ("file://foo/bar", new Uri ("file://///foo/bar").ToString(), "#9");

			u = new Uri ("mailto:foo");
			Assert.AreEqual ("foo", u.Host, "#11a");
			Assert.AreEqual (UriHostNameType.Dns, u.HostNameType, "#11b");
			Assert.AreEqual ("mailto:foo", u.ToString (), "#11c");

			u = new Uri ("news:");
			Assert.AreEqual (String.Empty, u.Host, "#16a");
			Assert.AreEqual (UriHostNameType.Unknown, u.HostNameType, "#16b");
			Assert.AreEqual ("news:", u.ToString (), "#16c");

			u = new Uri ("news:foo");
			Assert.AreEqual (String.Empty, u.Host, "#17a");
			Assert.AreEqual (UriHostNameType.Unknown, u.HostNameType, "#17b");
			Assert.AreEqual ("news:foo", u.ToString (), "#17c");
			Assert.AreEqual ("foo", u.AbsolutePath, "#17d");
			Assert.AreEqual ("news:foo", u.AbsoluteUri, "#17e");
		}

		[Test]
		[ExpectedException (typeof (UriFormatException))]
		public void HttpHostname1 ()
		{
			new Uri ("http:");
		}

		[Test]
		[ExpectedException (typeof (UriFormatException))]
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
					Assert.Fail ("Should be failed: " + reluris [i]);
				} catch (UriFormatException) {
				}
			}

			if (isWin32) {
				for (int i = 0; i < winRelUris.Length; i++) {
					try {
						new Uri (winRelUris [i]);
						Assert.Fail ("Should be failed: " + winRelUris [i]);
					} catch (UriFormatException) {
					}
				}
			}
		}

		[Test]
		public void LocalPath ()
		{
			Uri uri = new Uri ("c:\\tmp\\hello.txt");
			Assert.AreEqual ("file:///c:/tmp/hello.txt", uri.ToString (), "#1a");
			Assert.AreEqual ("c:\\tmp\\hello.txt", uri.LocalPath, "#1b");
			Assert.AreEqual ("file", uri.Scheme, "#1c");
			Assert.AreEqual ("", uri.Host, "#1d");
			Assert.AreEqual ("c:/tmp/hello.txt", uri.AbsolutePath, "#1e");

			uri = new Uri ("file:////////cygwin/tmp/hello.txt");
			Assert.AreEqual ("file://cygwin/tmp/hello.txt", uri.ToString (), "#3a");
			Assert.AreEqual ("\\\\cygwin\\tmp\\hello.txt", uri.LocalPath, "#3b win32");
			Assert.AreEqual ("file", uri.Scheme, "#3c");
			Assert.AreEqual ("cygwin", uri.Host, "#3d");
			Assert.AreEqual ("/tmp/hello.txt", uri.AbsolutePath, "#3e");

			uri = new Uri ("file://mymachine/cygwin/tmp/hello.txt");
			Assert.AreEqual ("file://mymachine/cygwin/tmp/hello.txt", uri.ToString (), "#4a");
			Assert.AreEqual ("\\\\mymachine\\cygwin\\tmp\\hello.txt", uri.LocalPath, "#4b win32");
			Assert.AreEqual ("file", uri.Scheme, "#4c");
			Assert.AreEqual ("mymachine", uri.Host, "#4d");
			Assert.AreEqual ("/cygwin/tmp/hello.txt", uri.AbsolutePath, "#4e");

			uri = new Uri ("file://///c:/cygwin/tmp/hello.txt");
			Assert.AreEqual ("file:///c:/cygwin/tmp/hello.txt", uri.ToString (), "#5a");
			Assert.AreEqual ("c:\\cygwin\\tmp\\hello.txt", uri.LocalPath, "#5b");
			Assert.AreEqual ("file", uri.Scheme, "#5c");
			Assert.AreEqual ("", uri.Host, "#5d");
			Assert.AreEqual ("c:/cygwin/tmp/hello.txt", uri.AbsolutePath, "#5e");
		}

		[Test]
		public void LocalPath_FileHost ()
		{
			// Hmm, they should be regarded just as a host name, since all URIs are base on absolute path.
			Uri uri = new Uri("file://one_file.txt");
			Assert.AreEqual ("file://one_file.txt/", uri.ToString(), "#6a");
			Assert.AreEqual ("/", uri.AbsolutePath, "#6e");
			Assert.AreEqual ("/", uri.PathAndQuery, "#6f");
			Assert.AreEqual ("file://one_file.txt/", uri.GetLeftPart (UriPartial.Path), "#6g");
			Assert.AreEqual ("\\\\one_file.txt", uri.LocalPath, "#6b");
			Assert.AreEqual ("file", uri.Scheme, "#6c");
			Assert.AreEqual ("one_file.txt", uri.Host, "#6d");

			// same tests - but original Uri is now ending with a '/'

			uri = new Uri ("file://one_file.txt/");
			Assert.AreEqual ("file://one_file.txt/", uri.ToString (), "#7a");
			Assert.AreEqual ("/", uri.AbsolutePath, "#7e");
			Assert.AreEqual ("/", uri.PathAndQuery, "#7f");
			Assert.AreEqual ("file://one_file.txt/", uri.GetLeftPart (UriPartial.Path), "#7g");
			Assert.AreEqual ("\\\\one_file.txt\\", uri.LocalPath, "#7b");
			Assert.AreEqual ("file", uri.Scheme, "#7c");
			Assert.AreEqual ("one_file.txt", uri.Host, "#7d");
		}

		[Test]
		public void LocalPath_Escape ()
		{
			// escape
			Uri uri = new Uri ("file:///tmp/a%20a");
			if (isWin32) {
				Assert.IsTrue (uri.LocalPath.EndsWith ("/tmp/a a"), "#7a:" + uri.LocalPath);
			} else
				Assert.AreEqual ("/tmp/a a", uri.LocalPath, "#7b");

			uri = new Uri ("file:///tmp/foo%25bar");
			if (isWin32) {
				Assert.IsTrue (uri.LocalPath.EndsWith ("/tmp/foo%bar"), "#8a:" + uri.LocalPath);
				Assert.IsTrue (uri.ToString ().EndsWith ("//tmp/foo%25bar"), "#8c:" + uri.ToString ());
			} else {
				Assert.AreEqual ("/tmp/foo%bar", uri.LocalPath, "#8b");
				Assert.AreEqual ("file:///tmp/foo%25bar", uri.ToString (), "#8d");
			}
			// bug #76643
			uri = new Uri ("file:///foo%25bar");
			if (isWin32) {
				Assert.IsTrue (uri.LocalPath.EndsWith ("/foo%bar"), "#9a:" + uri.LocalPath);
				// ditto, file://tmp/foo%25bar (bug in 1.x)
				Assert.IsTrue (uri.ToString ().EndsWith ("//foo%25bar"), "#9c:" + uri.ToString ());
			} else {
				Assert.AreEqual ("/foo%bar", uri.LocalPath, "#9b");
				Assert.AreEqual ("file:///foo%25bar", uri.ToString (), "#9d");
			}
		}

		// Novell Bugzilla #320614
		[Test]
		public void QueryEscape ()
		{
			Uri u1 = new Uri("http://localhost:8080/test.aspx?ReturnUrl=%2fSearchDoc%2fSearcher.aspx");
			Uri u2 = new Uri("http://localhost:8080/test.aspx?ReturnUrl=%252fSearchDoc%252fSearcher.aspx");

			Assert.AreEqual ("http://localhost:8080/test.aspx?ReturnUrl=%2fSearchDoc%2fSearcher.aspx", u1.ToString (), "QE1");

			Assert.AreEqual ("http://localhost:8080/test.aspx?ReturnUrl=%252fSearchDoc%252fSearcher.aspx", u2.ToString (), "QE2");
		}

		[Test]
		public void UnixPath () {
			if (!isWin32)
				Assert.AreEqual ("file:///cygwin/tmp/hello.txt", new Uri ("/cygwin/tmp/hello.txt").ToString (), "#6a");
		}

		[Test]
		public void Unc ()
		{
			Uri uri = new Uri ("http://www.contoso.com");
			Assert.IsTrue (!uri.IsUnc, "#1");

			uri = new Uri ("news:123456@contoso.com");
			Assert.IsTrue (!uri.IsUnc, "#2");

			uri = new Uri ("file://server/filename.ext");
			Assert.AreEqual (isWin32, uri.IsUnc, "#3");

			uri = new Uri (@"\\server\share\filename.ext");
			Assert.IsTrue (uri.IsUnc, "#6");

			uri = new Uri (@"a:\dir\filename.ext");
			Assert.IsTrue (!uri.IsUnc, "#8");
		}

		[Test]
		[Category("NotDotNet")]
		public void UncFail ()
		{
			if (!isWin32) {
				Uri uri = new Uri ("/home/user/dir/filename.ext");
				Assert.IsTrue (!uri.IsUnc, "#7");
			}
		}

		[Test]
		public void FromHex ()
		{
			Assert.AreEqual (0, Uri.FromHex ('0'), "#1");
			Assert.AreEqual (9, Uri.FromHex ('9'), "#2");
			Assert.AreEqual (10, Uri.FromHex ('a'), "#3");
			Assert.AreEqual (15, Uri.FromHex ('f'), "#4");
			Assert.AreEqual (10, Uri.FromHex ('A'), "#5");
			Assert.AreEqual (15, Uri.FromHex ('F'), "#6");
			try {
				Uri.FromHex ('G');
				Assert.Fail ("#7");
			} catch (ArgumentException) {}
			try {
				Uri.FromHex (' ');
				Assert.Fail ("#8");
			} catch (ArgumentException) {}
			try {
				Uri.FromHex ('%');
				Assert.Fail ("#8");
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
			Assert.AreEqual ("#", UriEx.UnescapeString ("file://localhost/c#", "%23"), "#1");
			Assert.AreEqual ("c#", UriEx.UnescapeString ("file://localhost/c#", "c%23"), "#2");
			Assert.AreEqual ("#", UriEx.UnescapeString ("http://localhost/c#", "%23"), "#1");
			Assert.AreEqual ("c#", UriEx.UnescapeString ("http://localhost/c#", "c%23"), "#2");
			Assert.AreEqual ("%A9", UriEx.UnescapeString ("file://localhost/c#", "%A9"), "#3");
			Assert.AreEqual ("%A9", UriEx.UnescapeString ("http://localhost/c#", "%A9"), "#3");
		}

		[Test]
		public void HexEscape ()
		{
			Assert.AreEqual ("%20", Uri.HexEscape (' '), "#1");
			Assert.AreEqual ("%A9", Uri.HexEscape ((char) 0xa9), "#2");
			Assert.AreEqual ("%41", Uri.HexEscape ('A'), "#3");
			try {
				Uri.HexEscape ((char) 0x0369);
				Assert.Fail ("#4");
			} catch (ArgumentOutOfRangeException) {}
		}

	[Test]
	public void MoreHexEscape()
	{
	    string url = "http://guyc-2003-sp/wiki/wiki%20document%20library/בד%20יקה.docx";
	    string escapedAbsolutePath = "/wiki/wiki%20document%20library/%D7%91%D7%93%20%D7%99%D7%A7%D7%94.docx";
	    Uri u = new Uri(url);
	    Assert.AreEqual (escapedAbsolutePath, u.AbsolutePath, "Escaped non-english combo");
	}

		[Test]
		public void HexUnescape ()
		{
			int i = 0;
			Assert.AreEqual (' ', Uri.HexUnescape ("%20", ref i), "#1");
			Assert.AreEqual (3, i, "#2");
			i = 4;
			Assert.AreEqual ((char) 0xa9, Uri.HexUnescape ("test%a9test", ref i), "#3");
			Assert.AreEqual (7, i, "#4");
			Assert.AreEqual ('t', Uri.HexUnescape ("test%a9test", ref i), "#5");
			Assert.AreEqual (8, i, "#6");
			i = 4;
			Assert.AreEqual ('%', Uri.HexUnescape ("test%a", ref i), "#5");
			Assert.AreEqual (5, i, "#6");
			Assert.AreEqual ('%', Uri.HexUnescape ("testx%xx", ref i), "#7");
			Assert.AreEqual (6, i, "#8");

			// Tests from bug 74872 - don't handle multi-byte characters as multi-byte
			i = 1;
			Assert.AreEqual (227, (int) Uri.HexUnescape ("a%E3%81%8B", ref i), "#9");
			Assert.AreEqual (4, i, "#10");
			i = 1;
			Assert.AreEqual (240, (int) Uri.HexUnescape ("a%F0%90%84%80", ref i), "#11");
			Assert.AreEqual (4, i, "#12");
		}

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
			Assert.AreEqual (8, path.Length, "#1");
			Assert.AreEqual (0x304B, path [6], "#2");

			// 4-byte character which should be handled as a surrogate
			uri = new Uri ("file:///foo/a%F3%A0%84%80b", true);
			path = uri.LocalPath;
			Assert.AreEqual (9, path.Length, "#3");
			Assert.AreEqual (0xDB40, path [6], "#4");
			Assert.AreEqual (0xDD00, path [7], "#5");
			Assert.AreEqual (0x62, path [8], "#6");

			// 2-byte escape sequence, 2 individual characters
			uri = new Uri ("file:///foo/a%C2%F8b", true);
			path = uri.LocalPath;
			Assert.AreEqual ("/foo/a%C2%F8b", path, "#7");
		}

		[Test]
		public void IsHexDigit ()
		{
			Assert.IsTrue (Uri.IsHexDigit ('a'), "#1");
			Assert.IsTrue (Uri.IsHexDigit ('f'), "#2");
			Assert.IsTrue (!Uri.IsHexDigit ('g'), "#3");
			Assert.IsTrue (Uri.IsHexDigit ('0'), "#4");
			Assert.IsTrue (Uri.IsHexDigit ('9'), "#5");
			Assert.IsTrue (Uri.IsHexDigit ('A'), "#6");
			Assert.IsTrue (Uri.IsHexDigit ('F'), "#7");
			Assert.IsTrue (!Uri.IsHexDigit ('G'), "#8");
		}

		[Test]
		public void IsHexEncoding ()
		{
			Assert.IsTrue (Uri.IsHexEncoding ("test%a9test", 4), "#1");
			Assert.IsTrue (!Uri.IsHexEncoding ("test%a9test", 3), "#2");
			Assert.IsTrue (Uri.IsHexEncoding ("test%a9", 4), "#3");
			Assert.IsTrue (!Uri.IsHexEncoding ("test%a", 4), "#4");
		}

		[Test]
		public void GetLeftPart ()
		{
			Uri uri = new Uri ("http://www.contoso.com/index.htm#main");
			Assert.AreEqual ("http://", uri.GetLeftPart (UriPartial.Scheme), "#1");
			Assert.AreEqual ("http://www.contoso.com", uri.GetLeftPart (UriPartial.Authority), "#2");
			Assert.AreEqual ("http://www.contoso.com/index.htm", uri.GetLeftPart (UriPartial.Path), "#3");

			uri = new Uri ("mailto:user@contoso.com?subject=uri");
			Assert.AreEqual ("mailto:", uri.GetLeftPart (UriPartial.Scheme), "#4");
			Assert.AreEqual ("", uri.GetLeftPart (UriPartial.Authority), "#5");
			Assert.AreEqual ("mailto:user@contoso.com", uri.GetLeftPart (UriPartial.Path), "#6");

			uri = new Uri ("nntp://news.contoso.com/123456@contoso.com");
			Assert.AreEqual ("nntp://", uri.GetLeftPart (UriPartial.Scheme), "#7");
			Assert.AreEqual ("nntp://news.contoso.com", uri.GetLeftPart (UriPartial.Authority), "#8");
			Assert.AreEqual ("nntp://news.contoso.com/123456@contoso.com", uri.GetLeftPart (UriPartial.Path), "#9");

			uri = new Uri ("news:123456@contoso.com");
			Assert.AreEqual ("news:", uri.GetLeftPart (UriPartial.Scheme), "#10");
			Assert.AreEqual ("", uri.GetLeftPart (UriPartial.Authority), "#11");
			Assert.AreEqual ("news:123456@contoso.com", uri.GetLeftPart (UriPartial.Path), "#12");

			uri = new Uri ("file://server/filename.ext");
			Assert.AreEqual ("file://", uri.GetLeftPart (UriPartial.Scheme), "#13");
			Assert.AreEqual ("file://server", uri.GetLeftPart (UriPartial.Authority), "#14");
			Assert.AreEqual ("file://server/filename.ext", uri.GetLeftPart (UriPartial.Path), "#15");

			uri = new Uri (@"\\server\share\filename.ext");
			Assert.AreEqual ("file://", uri.GetLeftPart (UriPartial.Scheme), "#20");
			Assert.AreEqual ("file://server", uri.GetLeftPart (UriPartial.Authority), "#21");
			Assert.AreEqual ("file://server/share/filename.ext", uri.GetLeftPart (UriPartial.Path), "#22");

			uri = new Uri ("http://www.contoso.com:8080/index.htm#main");
			Assert.AreEqual ("http://", uri.GetLeftPart (UriPartial.Scheme), "#23");
			Assert.AreEqual ("http://www.contoso.com:8080", uri.GetLeftPart (UriPartial.Authority), "#24");
			Assert.AreEqual ("http://www.contoso.com:8080/index.htm", uri.GetLeftPart (UriPartial.Path), "#25");
		}

		[Test]
		public void NewsDefaultPort ()
		{
			Uri uri = new Uri("news://localhost:119/");
			Assert.AreEqual (uri.IsDefaultPort, true, "#1");
		}

		[Test]
		public void Fragment_Escape ()
		{
			Uri u = new Uri("http://localhost/index.asp#main#start", false);

				Assert.AreEqual (u.Fragment, "#main#start", "#1");

			u = new Uri("http://localhost/index.asp#main#start", true);
			Assert.AreEqual (u.Fragment, "#main#start", "#2");

			// The other code path uses a BaseUri

			Uri b = new Uri ("http://www.gnome.org");
			Uri n = new Uri (b, "blah#main#start");
				Assert.AreEqual (n.Fragment, "#main#start", "#3");

			n = new Uri (b, "blah#main#start", true);
			Assert.AreEqual (n.Fragment, "#main#start", "#4");
		}

		[Test]
		public void Fragment_RelativeUri ()
		{
			Uri uri1 = new Uri ("http://www.contoso.com/index.htm?x=2");
			Uri uri2 = new Uri ("http://www.contoso.com/foo/bar/index.htm#fragment");
			Uri relativeUri = uri1.MakeRelativeUri (uri2);

			try {
				string fragment = relativeUri.Fragment;
				Assert.Fail ("#1: " + fragment);
			} catch (InvalidOperationException ex) {
				// This operation is not supported for a relative URI
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		[ExpectedException(typeof(UriFormatException))]
		public void IncompleteSchemeDelimiter ()
		{
			new Uri ("file:/filename.ext");
		}

		[Test]
		public void CheckHostName1 ()
		{
			// reported to MSDN Product Feedback Center (FDBK28671)
			Assert.AreEqual (UriHostNameType.IPv6, Uri.CheckHostName (":11:22:33:44:55:66:77:88"), "#36 known to fail with ms.net: this is not a valid IPv6 address.");
		}

		[Test]
		public void CheckHostName2 ()
		{
			Assert.AreEqual (UriHostNameType.Unknown, Uri.CheckHostName (null), "#1");
			Assert.AreEqual (UriHostNameType.Unknown, Uri.CheckHostName (""), "#2");
			Assert.AreEqual (UriHostNameType.Unknown, Uri.CheckHostName ("^&()~`!@"), "#3");
			Assert.AreEqual (UriHostNameType.Dns, Uri.CheckHostName ("x"), "#4");
			Assert.AreEqual (UriHostNameType.IPv4, Uri.CheckHostName ("1.2.3.4"), "#5");
			Assert.AreEqual (UriHostNameType.IPv4, Uri.CheckHostName ("0001.002.03.4"), "#6");
			Assert.AreEqual (UriHostNameType.Dns, Uri.CheckHostName ("0001.002.03.256"), "#7");
			Assert.AreEqual (UriHostNameType.Dns, Uri.CheckHostName ("9001.002.03.4"), "#8");
			Assert.AreEqual (UriHostNameType.Dns, Uri.CheckHostName ("www.contoso.com"), "#9");
			Assert.AreEqual (UriHostNameType.Unknown, Uri.CheckHostName (".www.contoso.com"), "#10");
			Assert.AreEqual (UriHostNameType.Dns, Uri.CheckHostName ("www.contoso.com."), "#11");
			Assert.AreEqual (UriHostNameType.Dns, Uri.CheckHostName ("www.con-toso.com"), "#12");
			Assert.AreEqual (UriHostNameType.Dns, Uri.CheckHostName ("www.con_toso.com"), "#13");
			Assert.AreEqual (UriHostNameType.Unknown, Uri.CheckHostName ("www.con,toso.com"), "#14");

			// test IPv6
			Assert.AreEqual (UriHostNameType.IPv6, Uri.CheckHostName ("11:22:33:44:55:66:77:88"), "#15");
			Assert.AreEqual (UriHostNameType.IPv6, Uri.CheckHostName ("11::33:44:55:66:77:88"), "#16");
			Assert.AreEqual (UriHostNameType.IPv6, Uri.CheckHostName ("::22:33:44:55:66:77:88"), "#17");
			Assert.AreEqual (UriHostNameType.IPv6, Uri.CheckHostName ("11:22:33:44:55:66:77::"), "#18");
			Assert.AreEqual (UriHostNameType.IPv6, Uri.CheckHostName ("11::88"), "#19");
			Assert.AreEqual (UriHostNameType.IPv6, Uri.CheckHostName ("11::77:88"), "#20");
			Assert.AreEqual (UriHostNameType.IPv6, Uri.CheckHostName ("11:22::88"), "#21");
			Assert.AreEqual (UriHostNameType.IPv6, Uri.CheckHostName ("11::"), "#22");
			Assert.AreEqual (UriHostNameType.IPv6, Uri.CheckHostName ("::88"), "#23");
			Assert.AreEqual (UriHostNameType.IPv6, Uri.CheckHostName ("::1"), "#24");
			Assert.AreEqual (UriHostNameType.IPv6, Uri.CheckHostName ("::"), "#25");
			Assert.AreEqual (UriHostNameType.IPv6, Uri.CheckHostName ("0:0:0:0:0:0:127.0.0.1"), "#26");
			Assert.AreEqual (UriHostNameType.IPv6, Uri.CheckHostName ("::127.0.0.1"), "#27");
			Assert.AreEqual (UriHostNameType.IPv6, Uri.CheckHostName ("::ffFF:169.32.14.5"), "#28");
			Assert.AreEqual (UriHostNameType.IPv6, Uri.CheckHostName ("2001:03A0::/35"), "#29");
			Assert.AreEqual (UriHostNameType.IPv6, Uri.CheckHostName ("[2001:03A0::/35]"), "#30");
			Assert.AreEqual (UriHostNameType.IPv6, Uri.CheckHostName ("2001::03A0:1.2.3.4"), "#33");

			Assert.AreEqual (UriHostNameType.Unknown, Uri.CheckHostName ("2001::03A0::/35"), "#31");
			Assert.AreEqual (UriHostNameType.Unknown, Uri.CheckHostName ("2001:03A0::/35a"), "#32");
			Assert.AreEqual (UriHostNameType.Unknown, Uri.CheckHostName ("::ffff:123.256.155.43"), "#34");
			Assert.AreEqual (UriHostNameType.Unknown, Uri.CheckHostName (":127.0.0.1"), "#35");
			Assert.AreEqual (UriHostNameType.Unknown, Uri.CheckHostName ("::11:22:33:44:55:66:77:88"), "#37");
			Assert.AreEqual (UriHostNameType.Unknown, Uri.CheckHostName ("11:22:33:44:55:66:77:88::"), "#38");
			Assert.AreEqual (UriHostNameType.Unknown, Uri.CheckHostName ("11:22:33:44:55:66:77:88:"), "#39");
			Assert.AreEqual (UriHostNameType.Unknown, Uri.CheckHostName ("::acbde"), "#40");
			Assert.AreEqual (UriHostNameType.Unknown, Uri.CheckHostName ("::abce:"), "#41");
			Assert.AreEqual (UriHostNameType.Unknown, Uri.CheckHostName ("::abcg"), "#42");
			Assert.AreEqual (UriHostNameType.Unknown, Uri.CheckHostName (":::"), "#43");
			Assert.AreEqual (UriHostNameType.Unknown, Uri.CheckHostName (":"), "#44");

			Assert.AreEqual (UriHostNameType.Unknown, Uri.CheckHostName ("*"), "#45");
			Assert.AreEqual (UriHostNameType.Unknown, Uri.CheckHostName ("*.example.com"), "#46");
			Assert.AreEqual (UriHostNameType.Unknown, Uri.CheckHostName ("www*.example.com"), "#47");
		}

		[Test]
		public void IsLoopback ()
		{
			Uri uri = new Uri("http://loopback:8080");
			Assert.AreEqual (true, uri.IsLoopback, "#1");
			uri = new Uri("http://localhost:8080");
			Assert.AreEqual (true, uri.IsLoopback, "#2");
			uri = new Uri("http://127.0.0.1:8080");
			Assert.AreEqual (true, uri.IsLoopback, "#3");
			uri = new Uri("http://127.0.0.001:8080");
			Assert.AreEqual (true, uri.IsLoopback, "#4");
			uri = new Uri("http://[::1]");
			Assert.AreEqual (true, uri.IsLoopback, "#5");
			uri = new Uri("http://[::1]:8080");
			Assert.AreEqual (true, uri.IsLoopback, "#6");
			uri = new Uri("http://[::0001]:8080");
			Assert.AreEqual (true, uri.IsLoopback, "#7");
			uri = new Uri("http://[0:0:0:0::1]:8080");
			Assert.AreEqual (true, uri.IsLoopback, "#8");
			uri = new Uri("http://[0:0:0:0::127.0.0.1]:8080");
			Assert.AreEqual (true, uri.IsLoopback, "#9");
			uri = new Uri("http://[0:0:0:0::127.11.22.33]:8080");
			Assert.AreEqual (false, uri.IsLoopback, "#10");
			uri = new Uri("http://[::ffff:127.11.22.33]:8080");
			Assert.AreEqual (false, uri.IsLoopback, "#11");
			uri = new Uri("http://[::ff00:7f11:2233]:8080");
			Assert.AreEqual (false, uri.IsLoopback, "#12");
			uri = new Uri("http://[1:0:0:0::1]:8080");
			Assert.AreEqual (false, uri.IsLoopback, "#13");
		}

		[Test]
		public void IsLoopback_File ()
		{
			Uri uri = new Uri ("file:///index.html");
			Assert.IsTrue (uri.IsLoopback, "file");
		}

		[Test]
		public void IsLoopback_Relative_Http ()
		{
			string relative = "http:8080/bar/Hello World.htm";
			Uri uri = new Uri (new Uri ("http://www.contoso.com"), relative, false);
			Assert.IsTrue (!uri.IsLoopback, "http");
		}

		[Test]
		public void IsLoopback_Relative_Unknown ()
		{
			string relative = "foo:8080/bar/Hello World.htm";
			Uri uri = new Uri (new Uri ("http://www.contoso.com"), relative, false);
			Assert.IsTrue (!uri.IsLoopback, "foo");
		}

		[Test]
		public void Equals1 ()
		{
			Uri uri1 = new Uri ("http://www.contoso.com/index.htm#main");
			Uri uri2 = new Uri ("http://www.contoso.com/index.htm#fragment");
			Assert.IsTrue (uri1.Equals (uri2), "#1");
			Assert.IsTrue (!uri2.Equals ("http://www.contoso.com/index.html?x=1"), "#3");
			Assert.IsTrue (!uri1.Equals ("http://www.contoso.com:8080/index.htm?x=1"), "#4");
		}

		[Test]
		public void Equals2 ()
		{
			Uri uri1 = new Uri ("http://www.contoso.com/index.htm#main");
			Uri uri2 = new Uri ("http://www.contoso.com/index.htm?x=1");
			Assert.IsTrue (!uri1.Equals (uri2), "#2 known to fail with ms.net 1.x");
		}

		[Test]
		public void Equals3 ()
		{
			Uri uri1 = new Uri ("svn+ssh://atsushi@mono-cvs.ximian.com");
			Uri uri2 = new Uri ("svn+ssh://anonymous@mono-cvs.ximian.com");
			Assert.IsTrue (uri1.Equals (uri2));
		}

		[Test]
		public void Equals4 ()
		{
			var uri = new Uri ("http://w3.org");
			Assert.IsFalse (uri.Equals ("-"));
		}

		[Test]
		public void TestEquals2 ()
		{
			Uri a = new Uri ("http://www.example.com");
			Uri b = new Uri ("http://www.example.com");

			Assert.AreEqual (a, b, "#1");

			a = new Uri ("mailto:user:pwd@example.com?subject=uri");
			b = new Uri ("MAILTO:USER:PWD@EXAMPLE.COM?SUBJECT=URI");

			Assert.IsTrue (a != b, "#2");
			Assert.AreEqual ("mailto:user:pwd@example.com?subject=uri", a.ToString (), "#2a");
			Assert.AreEqual ("mailto:USER:PWD@example.com?SUBJECT=URI", b.ToString (), "#2b");

			a = new Uri ("http://www.example.com/ports/");
			b = new Uri ("http://www.example.com/PORTS/");

			Assert.IsTrue (!a.Equals (b), "#3");
		}

		[Test]
		public void CaseSensitivity ()
		{
			Uri mailto = new Uri ("MAILTO:USER:PWD@EXAMPLE.COM?SUBJECT=URI");
			Assert.AreEqual ("mailto", mailto.Scheme, "#1");
			Assert.AreEqual ("example.com", mailto.Host, "#2");
			Assert.AreEqual ("mailto:USER:PWD@example.com?SUBJECT=URI", mailto.ToString (), "#3");

			Uri http = new Uri ("HTTP://EXAMPLE.COM/INDEX.HTML");
			Assert.AreEqual ("http", http.Scheme, "#4");
			Assert.AreEqual ("example.com", http.Host, "#5");
			Assert.AreEqual ("http://example.com/INDEX.HTML", http.ToString (), "#6");

			// IPv6 Address
			Uri ftp = new Uri ("FTP://[::ffFF:169.32.14.5]/");
			Assert.AreEqual ("ftp", ftp.Scheme, "#7");

			Assert.AreEqual ("[::ffff:169.32.14.5]", ftp.Host, "#8");
			Assert.AreEqual ("ftp://[::ffff:169.32.14.5]/", ftp.ToString (), "#9");
		}

		[Test]
		public void GetHashCodeTest ()
		{
			Uri uri1 = new Uri ("http://www.contoso.com/index.htm#main");
			Uri uri2 = new Uri ("http://www.contoso.com/index.htm#fragment");
			Assert.AreEqual (uri1.GetHashCode (), uri2.GetHashCode (), "#1");
			uri2 = new Uri ("http://www.contoso.com/index.htm?x=1");
			Assert.IsTrue (uri1.GetHashCode () != uri2.GetHashCode (), "#2");
			uri2 = new Uri ("http://www.contoso.com:80/index.htm");
			Assert.AreEqual (uri1.GetHashCode (), uri2.GetHashCode (), "#3");
			uri2 = new Uri ("http://www.contoso.com:8080/index.htm");
			Assert.IsTrue (uri1.GetHashCode () != uri2.GetHashCode (), "#4");
		}

		[Test]
		public void RelativeEqualsTest()
		{
			Uri uri1 = new Uri ("foo/bar", UriKind.Relative);
			Uri uri2 = new Uri ("foo/bar", UriKind.Relative);
			Uri uri3 = new Uri ("bar/man", UriKind.Relative);
			Uri uri4 = new Uri ("BAR/MAN", UriKind.Relative);
			Assert.IsTrue (uri1 == uri2, "#1a");
			Assert.IsTrue (uri1.Equals(uri2), "#1b");
			Assert.IsTrue (uri1 != uri3, "#2a");
			Assert.IsTrue (!uri1.Equals(uri3), "#2b");
			Assert.IsTrue (uri1 == uri2, "#3a");
			Assert.IsTrue (uri1.Equals(uri2), "#3b");
			Assert.IsTrue (uri1 != uri3, "#4a");
			Assert.IsTrue (!uri1.Equals(uri3), "#4b");
			Assert.IsTrue (uri3 != uri4, "#5a");
			Assert.IsTrue (!uri3.Equals(uri4), "#5b");
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
			Assert.AreEqual (new Uri ("foo", UriKind.Relative).ToString (), "foo", "#1");
			Assert.AreEqual (new Uri ("foo#aa", UriKind.Relative).ToString (), "foo#aa", "#2");
			Assert.AreEqual (new Uri ("foo?aa", UriKind.Relative).ToString (), "foo?aa", "#3");
			Assert.AreEqual (new Uri ("foo#dingus?aa", UriKind.Relative).ToString (), "foo#dingus?aa", "#4");
			Assert.AreEqual (new Uri ("foo?dingus#aa", UriKind.Relative).ToString (), "foo?dingus#aa", "#4");
		}

		[Test]
		public void RelativeGetHashCodeTest()
		{
			Uri uri1 = new Uri ("foo/bar", UriKind.Relative);
			Uri uri2 = new Uri ("foo/bar", UriKind.Relative);
			Uri uri3 = new Uri ("bar/man", UriKind.Relative);
			Assert.AreEqual (uri1.GetHashCode(), uri2.GetHashCode(), "#1");
			Assert.IsTrue (uri1.GetHashCode() != uri3.GetHashCode(), "#2");
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

			Assert.AreEqual ("foo/bar/index.htm", uri1.MakeRelative (uri2), "#1");
			Assert.AreEqual ("../../index.htm", uri2.MakeRelative (uri1), "#2");

			Assert.AreEqual ("../../bar/foo/index.htm", uri2.MakeRelative (uri3), "#3");
			Assert.AreEqual ("../../foo/bar/index.htm", uri3.MakeRelative (uri2), "#4");

			Assert.AreEqual ("../foo2/index.htm", uri3.MakeRelative (uri4), "#5");
			Assert.AreEqual ("../foo/index.htm", uri4.MakeRelative (uri3), "#6");

			Assert.AreEqual ("https://www.contoso.com/bar/foo/index.htm?y=1", uri4.MakeRelative (uri5), "#7");

			Assert.AreEqual ("http://www.contoso2.com/bar/foo/index.htm?x=0", uri4.MakeRelative (uri6), "#8");

			Assert.AreEqual ("", uri6.MakeRelative (uri6), "#9");
			Assert.AreEqual ("foobar.htm", uri6.MakeRelative (uri7), "#10");

			Uri uri10 = new Uri ("mailto:xxx@xxx.com");
			Uri uri11 = new Uri ("mailto:xxx@xxx.com?subject=hola");
			Assert.AreEqual ("", uri10.MakeRelative (uri11), "#11");

			Uri uri12 = new Uri ("mailto:xxx@mail.xxx.com?subject=hola");
			Assert.AreEqual ("mailto:xxx@mail.xxx.com?subject=hola", uri10.MakeRelative (uri12), "#12");

			Uri uri13 = new Uri ("mailto:xxx@xxx.com/foo/bar");
			Assert.AreEqual ("/foo/bar", uri10.MakeRelative (uri13), "#13");

			Assert.AreEqual ("http://www.xxx.com/bar/foo/foobar.htm?z=0&y=5" + (char) 0xa9, uri1.MakeRelative (uri8), "#14");
		}

		[Test]
		public void RelativeFragmentUri ()
		{
			Uri u = new Uri("http://localhost/../../../a");
			Assert.AreEqual ("http://localhost/a", u.ToString ());

			u = new Uri ("http://localhost/../c/b/../a");
			Assert.AreEqual ("http://localhost/c/a", u.ToString ());
		}

		[Test]
		public void RelativeFragmentUri2 ()
		{
			Assert.AreEqual ("hoge:ext", new Uri (new Uri ("hoge:foo:bar:baz"), "hoge:ext").ToString (), "#1");
			if (isWin32) {
				Assert.AreEqual ("file:///d:/myhost/ext", new Uri (new Uri ("file:///c:/localhost/bar"), "file:///d:/myhost/ext").ToString (), "#2-w");
				Assert.AreEqual ("file:///c:/localhost/myhost/ext", new Uri (new Uri ("file:///c:/localhost/bar"), "file:myhost/ext").ToString (), "#3-w");
				Assert.AreEqual ("uuid:ext", new Uri (new Uri ("file:///c:/localhost/bar"), "uuid:ext").ToString (), "#4-w");
				Assert.AreEqual ("file:///c:/localhost/ext", new Uri (new Uri ("file:///c:/localhost/bar"), "file:./ext").ToString (), "#5-w");
			} else {
				Assert.AreEqual ("file:///d/myhost/ext", new Uri (new Uri ("file:///c/localhost/bar"), "file:///d/myhost/ext").ToString (), "#2-u");
				Assert.AreEqual ("file:///c/localhost/myhost/ext", new Uri (new Uri ("file:///c/localhost/bar"), "file:myhost/ext").ToString (), "#3-u");
				Assert.AreEqual ("uuid:ext", new Uri (new Uri ("file:///c/localhost/bar"), "uuid:ext").ToString (), "#4-u");
				Assert.AreEqual ("file:///c/localhost/ext", new Uri (new Uri ("file:///c/localhost/bar"), "file:./ext").ToString (), "#5-u");
			}
			Assert.AreEqual ("http://localhost/ext", new Uri (new Uri ("http://localhost/bar"), "http:./ext").ToString (), "#6");
		}

		[Test]
		public void ToStringTest()
		{
			Uri uri = new Uri ("dummy://xxx");
			Assert.AreEqual ("dummy://xxx/", uri.ToString (), "#1");
		}

		[Test]
		public void CheckSchemeName ()
		{
			Assert.AreEqual (false, Uri.CheckSchemeName (null), "#01");
			Assert.AreEqual (false, Uri.CheckSchemeName (""), "#02");
			Assert.AreEqual (true, Uri.CheckSchemeName ("http"), "#03");
			Assert.AreEqual (true, Uri.CheckSchemeName ("http-"), "#04");
			Assert.AreEqual (false, Uri.CheckSchemeName ("6http-"), "#05");
			Assert.AreEqual (true, Uri.CheckSchemeName ("http6-"), "#06");
			Assert.AreEqual (false, Uri.CheckSchemeName ("http6,"), "#07");
			Assert.AreEqual (true, Uri.CheckSchemeName ("http6."), "#08");
			Assert.AreEqual (false, Uri.CheckSchemeName ("+http"), "#09");
			Assert.AreEqual (true, Uri.CheckSchemeName ("htt+p6"), "#10");
			// 0x00E1 -> &atilde;
			Assert.IsTrue (!Uri.CheckSchemeName ("htt\u00E1+p6"), "#11");
		}

		[Test]
		public void CheckSchemeName_FirstChar ()
		{
			for (int i = 0; i < 256; i++) {
				string s = String.Format ("#{0}", i);
				char c = (char) i;
				bool b = Uri.CheckSchemeName (c.ToString ());
				bool valid = (((i >= 0x41) && (i <= 0x5A)) || ((i >= 0x61) && (i <= 0x7A)));
				Assert.AreEqual (valid, b, s);
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
				bool valid = (common || ((i >= 0x41) && (i <= 0x5A)) || ((i >= 0x61) && (i <= 0x7A)));
				Assert.AreEqual (valid, b, s);
			}
		}

		[Test]
		[ExpectedException (typeof (UriFormatException))]
		public void NoHostname ()
		{
			Uri uri = new Uri ("http://");
		}

		[Test]
		public void NoHostname2 ()
		{
			// bug 75144
			Uri uri = new Uri ("file://");
			Assert.AreEqual (true, uri.IsFile, "#1");
			Assert.AreEqual (false, uri.IsUnc, "#2");
			Assert.AreEqual ("file", uri.Scheme, "#3");
			Assert.AreEqual ("/", uri.LocalPath, "#4");
			Assert.AreEqual (string.Empty, uri.Query, "#5");
			Assert.AreEqual ("/", uri.AbsolutePath, "#6");
			Assert.AreEqual ("file:///", uri.AbsoluteUri, "#7");
			Assert.AreEqual (string.Empty, uri.Authority, "#8");
			Assert.AreEqual (string.Empty, uri.Host, "#9");
			Assert.AreEqual (UriHostNameType.Basic, uri.HostNameType, "#10");
			Assert.AreEqual (string.Empty, uri.Fragment, "#11");
			Assert.AreEqual (true, uri.IsDefaultPort, "#12");
			Assert.IsTrue (uri.IsLoopback, "#13");
			Assert.AreEqual ("/", uri.PathAndQuery, "#14");
			Assert.AreEqual (false, uri.UserEscaped, "#15");
			Assert.AreEqual (string.Empty, uri.UserInfo, "#16");
			Assert.AreEqual ("file://", uri.GetLeftPart (UriPartial.Authority), "#17");
			Assert.AreEqual ("file:///", uri.GetLeftPart (UriPartial.Path), "#18");
			Assert.AreEqual ("file://", uri.GetLeftPart (UriPartial.Scheme), "#19");
		}

		[Test]
		public void Segments1 ()
		{
			Uri uri = new Uri ("http://localhost/");
			string [] segments = uri.Segments;
			Assert.AreEqual (1, segments.Length, "#01");
			Assert.AreEqual ("/", segments [0], "#02");
		}

		[Test]
		public void Segments2 ()
		{
			Uri uri = new Uri ("http://localhost/dir/dummypage.html");
			string [] segments = uri.Segments;
			Assert.AreEqual (3, segments.Length, "#01");
			Assert.AreEqual ("/", segments [0], "#02");
			Assert.AreEqual ("dir/", segments [1], "#03");
			Assert.AreEqual ("dummypage.html", segments [2], "#04");
		}

		[Test]
		public void CachingSegments ()
		{
			Uri uri = new Uri ("http://localhost/dir/dummypage.html");
			uri.Segments [0] = uri.Segments [1] = uri.Segments [2] = "*";
			string [] segments = uri.Segments;
			Assert.AreEqual (3, segments.Length, "#01");
			Assert.AreEqual ("/", segments [0], "#02");
			Assert.AreEqual ("dir/", segments [1], "#03");
			Assert.AreEqual ("dummypage.html", segments [2], "#04");
		}

		[Test]
		public void Segments3 ()
		{
			Uri uri = new Uri ("http://localhost/dir/dummypage/");
			string [] segments = uri.Segments;
			Assert.AreEqual (3, segments.Length, "#01");
			Assert.AreEqual ("/", segments [0], "#02");
			Assert.AreEqual ("dir/", segments [1], "#03");
			Assert.AreEqual ("dummypage/", segments [2], "#04");
		}

		[Test]
		public void Segments4 ()
		{
			Uri uri = new Uri ("file:///c:/hello");

			Assert.AreEqual ("c:/hello", uri.AbsolutePath, "AbsolutePath");
			Assert.AreEqual ("c:\\hello", uri.LocalPath, "LocalPath");

			string [] segments = uri.Segments;
			Assert.AreEqual (3, segments.Length, "#01");
			Assert.AreEqual ("/", segments [0], "#02");
			Assert.AreEqual ("c:/", segments[1], "#03");
			Assert.AreEqual ("hello", segments [2], "#04");
		}

		[Test]
		public void Segments5 ()
		{
			Uri uri = new Uri ("http://www.xxx.com/bar/foo/foobar.htm?z=0&y=5" + (char) 0xa9);
			string [] segments = uri.Segments;
			Assert.AreEqual (4, segments.Length, "#01");
			Assert.AreEqual ("/", segments [0], "#02");
			Assert.AreEqual ("bar/", segments [1], "#03");
			Assert.AreEqual ("foo/", segments [2], "#04");
			Assert.AreEqual ("foobar.htm", segments [3], "#05");
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

		[Test]
		public void SchemeWithDigits ()
		{
			Uri uri = new Uri ("net.p2p://foobar");
			Assert.AreEqual ("net.p2p", uri.Scheme);
		}

		// on .NET 2.0 a port number is limited to UInt16.MaxValue
		[ExpectedException (typeof (UriFormatException))]
		[Test]
		public void InvalidPort1 ()
		{
			Uri uri = new Uri ("http://www.contoso.com:65536/foo/bar/");
			Assert.AreEqual (65536, uri.Port);
		}

		[ExpectedException (typeof (UriFormatException))]
		[Test]
		public void InvalidPort2 ()
		{
			// UInt32.MaxValue gives port == -1 !!!
			Uri uri = new Uri ("http://www.contoso.com:4294967295/foo/bar/");
			Assert.AreEqual (-1, uri.Port);
		}

		[ExpectedException (typeof (UriFormatException))]
		[Test]
		public void InvalidPort3 ()
		{
			// ((uint) Int32.MaxValue + (uint) 1) gives port == -2147483648 !!!
			Uri uri = new Uri ("http://www.contoso.com:2147483648/foo/bar/");
			Assert.AreEqual (-2147483648, uri.Port);
		}

		[Test]
		public void PortMax ()
		{
			// on .NET 2.0 a port number is limited to UInt16.MaxValue
			Uri uri = new Uri ("http://www.contoso.com:65535/foo/bar/");
			Assert.AreEqual (65535, uri.Port);
		}

		class UriEx2 : Uri
		{
			public UriEx2 (string s) : base (s)
			{
			}

			protected override void Parse ()
			{
			}
		}

		// Parse method is no longer used on .NET 2.0
		[ExpectedException (typeof (UriFormatException))]
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
			Assert.AreEqual ("/tmp/foo/bar", fileUri.AbsolutePath, path);

			// Empty path == localhost, in theory
			path = "file:///c:/tmp/foo/bar";
			fileUri = new Uri( path );
			Assert.AreEqual ("c:/tmp/foo/bar", fileUri.AbsolutePath, path);
		}

		[Test]
		public void WindowsLocalPath ()
		{
			new Uri (@"file:///J:\Wrldwide\MSFin\Flash\FLASH.xls");
		}

		[Test]
		public void TestEscapeDataString ()
		{
			StringBuilder sb = new StringBuilder ();

			for (int i = 0; i < 128; i++)
				sb.Append ((char) i);

			Assert.AreEqual (
				"%00%01%02%03%04%05%06%07%08%09%0A%0B%0C%0D%0E%0F%10%11%12%13%14%15%16%17%18%19%1A%1B%1C%1D%1E%1F%20%21%22%23%24%25%26%27%28%29%2A%2B%2C-.%2F0123456789%3A%3B%3C%3D%3E%3F%40ABCDEFGHIJKLMNOPQRSTUVWXYZ%5B%5C%5D%5E_%60abcdefghijklmnopqrstuvwxyz%7B%7C%7D~%7F",
				Uri.EscapeDataString (sb.ToString ()));

			Assert.AreEqual ("%C3%A1", Uri.EscapeDataString ("á"));
		}
		[Test]
		public void TestEscapeUriString ()
		{
			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < 128; i++)
				sb.Append ((char) i);

			Assert.AreEqual (
				"%00%01%02%03%04%05%06%07%08%09%0A%0B%0C%0D%0E%0F%10%11%12%13%14%15%16%17%18%19%1A%1B%1C%1D%1E%1F%20!%22#$%25&'()*+,-./0123456789:;%3C=%3E?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[%5C]%5E_%60abcdefghijklmnopqrstuvwxyz%7B%7C%7D~%7F",
				Uri.EscapeUriString (sb.ToString ()));

			Assert.AreEqual ("%C3%A1", Uri.EscapeDataString ("á"));
		}

		//bnc #363320
		[Test]
		public void TestUTF8Strings ()
		{
			string [] tests = {
				"file:///tmp/x (%232).jpg",
				"file:///tmp/ü (%232).jpg" };

			foreach (string test in tests) {
				Uri uri = new Uri (test);
				Assert.IsFalse (uri.IsWellFormedOriginalString (), "IsWellFormedOriginalString/" + test);
				Assert.AreEqual (test, uri.OriginalString, "OriginalString/" + test);
				Assert.AreEqual (test, uri.ToString (), "ToString/" + test);
			}
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
		public void UnixLocalPath_WTF ()
		{
			// Empty path == localhost, in theory
			string path = "file:///tmp/foo/bar";
			Uri fileUri = new Uri( path );
			Assert.AreEqual ("/tmp/foo/bar", fileUri.AbsolutePath, path);

			// bug #76643
			string path2 = "file:///foo%25bar";
			fileUri = new Uri (path2);
			Assert.AreEqual ("file:///foo%25bar", fileUri.ToString (), path2);
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

		[Test]
		public void FtpRootPath ()
		{
			Uri u = new Uri ("ftp://a.b/%2fabc/def");
			string p = u.PathAndQuery;
			Assert.AreEqual ("/%2fabc/def", p);
			p = Uri.UnescapeDataString (p).Substring (1);
			Assert.AreEqual ("/abc/def", p);
			u = new Uri (new Uri ("ftp://a.b/c/d/e/f"), p);
			Assert.AreEqual ("/abc/def", u.PathAndQuery);
		}

//BNC#533572
		[Test]
		public void LocalPath_FileNameWithAtSign1 ()
		{
			string path = "/some/path/file_with_an_@_sign.mp3";
			string fullpath = "http://thehost" + path;
			Uri fileUri = new Uri (fullpath);

			Assert.AreEqual (fileUri.UserInfo, String.Empty, "LocalPath_FileNameWithAtSign UserInfo");
			Assert.AreEqual (fileUri.Host, "thehost", "LocalPath_FileNameWithAtSign Host");
			Assert.IsFalse (fileUri.IsFile, "LocalPath_FileNameWithAtSign IsFile");
			Assert.IsTrue (fileUri.IsAbsoluteUri, "LocalPath_FileNameWithAtSign IsAbsUri");
			Assert.IsFalse (fileUri.IsUnc, "LocalPath_FileNameWithAtSign IsUnc");

			Assert.AreEqual (fullpath, fileUri.OriginalString, "LocalPath_FileNameWithAtSign OriginalString");
			Assert.AreEqual (path, new DerivedUri (fullpath).TestUnescape (path), "LocalPath_FileNameWithAtSign ProtectedUnescape");
			Assert.AreEqual (path, fileUri.AbsolutePath, "LocalPath_FileNameWithAtSign AbsPath");
			Assert.AreEqual (path, fileUri.LocalPath, "LocalPath_FileNameWithAtSign LocalPath");
		}

		[Test]
		public void LocalPath_FileNameWithAtSign2 ()
		{
			string path = "/some/path/file_with_an_@_sign.mp3";
			string fullpath = "http://user:password@thehost" + path;
			Uri fileUri = new Uri (fullpath);

			Assert.AreEqual (fileUri.UserInfo, "user:password", "LocalPath_FileNameWithAtSign UserInfo");
			Assert.AreEqual (fileUri.Host, "thehost", "LocalPath_FileNameWithAtSign Host");
			Assert.IsFalse (fileUri.IsFile, "LocalPath_FileNameWithAtSign IsFile");
			Assert.IsTrue (fileUri.IsAbsoluteUri, "LocalPath_FileNameWithAtSign IsAbsUri");
			Assert.IsFalse (fileUri.IsUnc, "LocalPath_FileNameWithAtSign IsUnc");

			Assert.AreEqual (fullpath, fileUri.OriginalString, "LocalPath_FileNameWithAtSign OriginalString");
			Assert.AreEqual (path, new DerivedUri (fullpath).TestUnescape (path), "LocalPath_FileNameWithAtSign ProtectedUnescape");
			Assert.AreEqual (path, fileUri.AbsolutePath, "LocalPath_FileNameWithAtSign AbsPath");
			Assert.AreEqual (path, fileUri.LocalPath, "LocalPath_FileNameWithAtSign LocalPath");
		}

		[Test]
		public void LocalPath_FileNameWithAtSign3 ()
		{
			string path = "/some/path/file_with_an_@_sign.mp3";
			string fullpath = "file://" + path;
			Uri fileUri = new Uri (fullpath);

			Assert.AreEqual (fileUri.UserInfo, String.Empty, "LocalPath_FileNameWithAtSign UserInfo");
			Assert.AreEqual (fileUri.Host, String.Empty, "LocalPath_FileNameWithAtSign Host");
			Assert.IsTrue (fileUri.IsFile, "LocalPath_FileNameWithAtSign IsFile");
			Assert.IsTrue (fileUri.IsAbsoluteUri, "LocalPath_FileNameWithAtSign IsAbsUri");
			Assert.IsFalse (fileUri.IsUnc, "LocalPath_FileNameWithAtSign IsUnc");

			Assert.AreEqual (fullpath, fileUri.OriginalString, "LocalPath_FileNameWithAtSign OriginalString");
			Assert.AreEqual (path, new DerivedUri (fullpath).TestUnescape (path), "LocalPath_FileNameWithAtSign ProtectedUnescape");
			Assert.AreEqual (path, fileUri.AbsolutePath, "LocalPath_FileNameWithAtSign AbsPath");
			Assert.AreEqual (path, fileUri.LocalPath, "LocalPath_FileNameWithAtSign LocalPath");
		}

		[Test]
		public void LocalPath_FileNameWithAtSign4 ()
		{
			string path = "/some/path/file_with_an_@_sign.mp3";
			string fullpath = "file://localhost" + path;
			Uri fileUri = new Uri (fullpath);

			Assert.AreEqual (fileUri.UserInfo, String.Empty, "LocalPath_FileNameWithAtSign UserInfo");
			Assert.AreEqual (fileUri.Host, "localhost", "LocalPath_FileNameWithAtSign Host");
			Assert.IsTrue (fileUri.IsFile, "LocalPath_FileNameWithAtSign IsFile");
			Assert.IsTrue (fileUri.IsAbsoluteUri, "LocalPath_FileNameWithAtSign IsAbsUri");
			Assert.AreEqual (isWin32, fileUri.IsUnc, "LocalPath_FileNameWithAtSign IsUnc");

			Assert.AreEqual (fullpath, fileUri.OriginalString, "LocalPath_FileNameWithAtSign OriginalString");
			Assert.AreEqual (path, new DerivedUri (fullpath).TestUnescape (path), "LocalPath_FileNameWithAtSign ProtectedUnescape");
			Assert.AreEqual (path, fileUri.AbsolutePath, "LocalPath_FileNameWithAtSign AbsPath");
			//this test is marked as NotWorking below:
			//Assert.AreEqual ("\\\\localhost" + path.Replace ("/", "\\"), fileUri.LocalPath, "LocalPath_FileNameWithAtSign LocalPath");
		}

		[Test]
		public void LocalPath_FileNameWithAtSign5 ()
		{
			string path = "/some/path/file_with_an_@_sign.mp3";
			string fullpath = "file://localhost" + path;
			Uri fileUri = new Uri (fullpath);

			string expected = isWin32 ? "\\\\localhost" + path.Replace ("/", "\\") : "/some/path/file_with_an_@_sign.mp3";
			Assert.AreEqual (expected, fileUri.LocalPath, "LocalPath_FileNameWithAtSign LocalPath");
		}

		[Test]
		[ExpectedException (typeof (UriFormatException))]
		public void LocalPath_FileNameWithAtSign6 ()
		{
			string path = "/some/path/file_with_an_@_sign.mp3";
			string fullpath = "file://user:password@localhost" + path;
			Uri fileUri = new Uri (fullpath);
		}

		[Test]
		[Category ("NotDotNet")]
		public void UnixAbsoluteFilePath_WithSpecialChars1 ()
		{
			if (isWin32)
				Assert.Ignore ();

			Uri unixuri = new Uri ("/home/user/a@b");
			Assert.AreEqual ("file", unixuri.Scheme, "UnixAbsoluteFilePath_WithSpecialChars #1");
		}

		[Test]
		[Category ("NotDotNet")]
		public void UnixAbsoluteFilePath_WithSpecialChars2 ()
		{
			if (isWin32)
				Assert.Ignore ();

			Uri unixuri = new Uri ("/home/user/a:b");
			Assert.AreEqual ("file", unixuri.Scheme, "UnixAbsoluteFilePath_WithSpecialChars #2");
		}

		[Test]
		[Category ("NotDotNet")]
		public void UnixAbsolutePath_ReplaceRelative ()
		{
			if (isWin32)
				Assert.Ignore ();

			var u1 = new Uri ("/Users/demo/Projects/file.xml");
			var u2 = new Uri (u1, "b.jpg");

			Assert.AreEqual ("file:///Users/demo/Projects/b.jpg", u2.ToString ());
		}

		[Test]
		public void RelativeUriWithColons ()
		{
			string s = @"Transform?args=[{""__type"":""Record:#Nostr"",""Code"":""%22test%22SomeGloss"",""ID"":""1"",""Table"":""Glossary""},{""__type"":""Record:#Nostr"",""Code"":""%22test%22All"",""ID"":""2"",""Table"":""GlossView""}, {""__type"":""Record:#Nostr"",""Code"":""%22test%22Q"",""ID"":""3"",""Table"":""Glossary""}]"; // with related to bug #573795
			new Uri (s, UriKind.Relative);
			new Uri (":", UriKind.Relative);
			new Uri ("1:", UriKind.Relative);
		}

		[Test]
		public void ConsecutiveSlashes ()
		{
			Uri uri = new Uri ("http://media.libsyn.com/bounce/http://cdn4.libsyn.com/nerdist/somestuff.txt");
			Assert.AreEqual ("http://media.libsyn.com/bounce/http://cdn4.libsyn.com/nerdist/somestuff.txt", uri.ToString ());
		}

		public class DerivedUri : Uri {
			public DerivedUri (string uriString)
				: base (uriString)
			{
			}

			internal string TestUnescape (string path)
			{
				return base.Unescape (path);
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void GetComponents_Relative ()
		{
			Uri rel = new Uri ("/relative/path/with?query", UriKind.Relative);
			rel.GetComponents (UriComponents.Query, UriFormat.SafeUnescaped);
		}

		[Test]
		public void GetComponents_AbsoluteUri ()
		{
			Uri uri = new Uri ("http://example.com/list?id=1%262&sort=asc#fragment%263");

			Assert.AreEqual ("http://example.com/list?id=1%262&sort=asc#fragment%263", uri.AbsoluteUri, "AbsoluteUri");

			string safe = uri.GetComponents (UriComponents.AbsoluteUri, UriFormat.SafeUnescaped);
			Assert.AreEqual ("http://example.com/list?id=1%262&sort=asc#fragment%263", safe, "SafeUnescaped");

			string unescaped = uri.GetComponents (UriComponents.AbsoluteUri, UriFormat.Unescaped);
			Assert.AreEqual ("http://example.com/list?id=1&2&sort=asc#fragment&3", unescaped, "Unescaped");

			string escaped = uri.GetComponents (UriComponents.AbsoluteUri, UriFormat.UriEscaped);
			Assert.AreEqual ("http://example.com/list?id=1%262&sort=asc#fragment%263", escaped, "UriEscaped");
		}

		[Test]
		public void GetComponents_HttpRequestUrl ()
		{
			Uri uri = new Uri ("http://example.com/list?id=1%262&sort=asc#fragment%263");

			string safe = uri.GetComponents (UriComponents.HttpRequestUrl, UriFormat.SafeUnescaped);
			Assert.AreEqual ("http://example.com/list?id=1%262&sort=asc", safe, "SafeUnescaped");

			string unescaped = uri.GetComponents (UriComponents.HttpRequestUrl, UriFormat.Unescaped);
			Assert.AreEqual ("http://example.com/list?id=1&2&sort=asc", unescaped, "Unescaped");

			string escaped = uri.GetComponents (UriComponents.HttpRequestUrl, UriFormat.UriEscaped);
			Assert.AreEqual ("http://example.com/list?id=1%262&sort=asc", escaped, "UriEscaped");
		}

		[Test]
		public void GetComponents_KeepDelimiter ()
		{
			Uri uri = new Uri ("http://example.com/list?id=1%262&sort=asc#fragment%263");

			string safe = uri.GetComponents (UriComponents.KeepDelimiter, UriFormat.SafeUnescaped);
			Assert.AreEqual (String.Empty, safe, "SafeUnescaped");

			string unescaped = uri.GetComponents (UriComponents.KeepDelimiter, UriFormat.Unescaped);
			Assert.AreEqual (String.Empty, unescaped, "Unescaped");

			string escaped = uri.GetComponents (UriComponents.KeepDelimiter, UriFormat.UriEscaped);
			Assert.AreEqual (String.Empty, escaped, "UriEscaped");
		}

		[Test]
		public void GetComponents_StrongAuthority ()
		{
			Uri uri = new Uri ("http://example.com/list?id=1%262&sort=asc#fragment%263");

			string safe = uri.GetComponents (UriComponents.StrongAuthority, UriFormat.SafeUnescaped);
			Assert.AreEqual ("example.com:80", safe, "SafeUnescaped");

			string unescaped = uri.GetComponents (UriComponents.StrongAuthority, UriFormat.Unescaped);
			Assert.AreEqual ("example.com:80", unescaped, "Unescaped");

			string escaped = uri.GetComponents (UriComponents.StrongAuthority, UriFormat.UriEscaped);
			Assert.AreEqual ("example.com:80", escaped, "UriEscaped");
		}

		[Test]
		public void GetComponents_Path ()
		{
			Uri uri1 = new Uri ("http://example.com/Main%20Page");
			Assert.AreEqual ("/Main Page", uri1.LocalPath, "Path1");

			string safe = uri1.GetComponents (UriComponents.Path, UriFormat.SafeUnescaped);
			Assert.AreEqual ("Main Page", safe, "SafeUnescaped1");

			string unescaped = uri1.GetComponents (UriComponents.Path, UriFormat.Unescaped);
			Assert.AreEqual ("Main Page", unescaped, "Unescaped1");

			string escaped = uri1.GetComponents (UriComponents.Path, UriFormat.UriEscaped);
			Assert.AreEqual ("Main%20Page", escaped, "UriEscaped1");

			// same result is unescaped original string
			Uri uri2 = new Uri ("http://example.com/Main Page");
			Assert.AreEqual ("/Main Page", uri2.LocalPath, "Path2");

			safe = uri2.GetComponents (UriComponents.Path, UriFormat.SafeUnescaped);
			Assert.AreEqual ("Main Page", safe, "SafeUnescaped2");

			unescaped = uri2.GetComponents (UriComponents.Path, UriFormat.Unescaped);
			Assert.AreEqual ("Main Page", unescaped, "Unescaped2");

			escaped = uri2.GetComponents (UriComponents.Path, UriFormat.UriEscaped);
			Assert.AreEqual ("Main%20Page", escaped, "UriEscaped2");
		}

		[Test]
		public void GetComponents_PathAndQuery ()
		{
			Uri uri = new Uri ("http://example.com/MåÏn Påge?id=1%262&sort=asc");

			Assert.AreEqual ("/M%C3%A5%C3%8Fn%20P%C3%A5ge?id=1%262&sort=asc", uri.PathAndQuery, "PathAndQuery");

			string safe = uri.GetComponents (UriComponents.PathAndQuery, UriFormat.SafeUnescaped);
			Assert.AreEqual ("/MåÏn Påge?id=1%262&sort=asc", safe, "SafeUnescaped");

			string unescaped = uri.GetComponents (UriComponents.PathAndQuery, UriFormat.Unescaped);
			Assert.AreEqual ("/MåÏn Påge?id=1&2&sort=asc", unescaped, "Unescaped");

			string escaped = uri.GetComponents (UriComponents.PathAndQuery, UriFormat.UriEscaped);
			Assert.AreEqual ("/M%C3%A5%C3%8Fn%20P%C3%A5ge?id=1%262&sort=asc", escaped, "UriEscaped");
		}

		[Test]
		public void GetComponents_Query ()
		{
			Uri uri = new Uri ("http://example.com/list?id=1%262&sort=asc");

			Assert.AreEqual ("?id=1%262&sort=asc", uri.Query, "Query");
			
			string safe = uri.GetComponents (UriComponents.Query, UriFormat.SafeUnescaped);
			Assert.AreEqual ("id=1%262&sort=asc", safe, "SafeUnescaped");

			string unescaped = uri.GetComponents (UriComponents.Query, UriFormat.Unescaped);
			Assert.AreEqual ("id=1&2&sort=asc", unescaped, "Unescaped");

			string escaped = uri.GetComponents (UriComponents.Query, UriFormat.UriEscaped);
			Assert.AreEqual ("id=1%262&sort=asc", escaped, "UriEscaped");
		}

		[Test]
		public void GetComponents_Fragment ()
		{
			Uri uri = new Uri ("http://example.com/list#id=1%262&sort=asc");

			Assert.AreEqual ("#id=1%262&sort=asc", uri.Fragment, "Fragment");

			string safe = uri.GetComponents (UriComponents.Fragment, UriFormat.SafeUnescaped);
			Assert.AreEqual ("id=1%262&sort=asc", safe, "SafeUnescaped");

			string unescaped = uri.GetComponents (UriComponents.Fragment, UriFormat.Unescaped);
			Assert.AreEqual ("id=1&2&sort=asc", unescaped, "Unescaped");

			string escaped = uri.GetComponents (UriComponents.Fragment, UriFormat.UriEscaped);
			Assert.AreEqual ("id=1%262&sort=asc", escaped, "UriEscaped");
		}

		// When used, paths such as "/foo" are assumed relative.
		static UriKind DotNetRelativeOrAbsolute = (Type.GetType ("Mono.Runtime") == null)? UriKind.RelativeOrAbsolute : (UriKind) 300;

		[Test]
		public void DotNetRelativeOrAbsoluteTest ()
		{
			// On windows the path /foo is parsed as BadFormat and checking
			// if this is relative or absolute doesn't make sense.
			if (isWin32)
				Assert.Ignore();

			FieldInfo useDotNetRelativeOrAbsoluteField = null;
			bool useDotNetRelativeOrAbsoluteOld = false;

			if (Type.GetType ("Mono.Runtime") != null) {
				useDotNetRelativeOrAbsoluteField = typeof (Uri).GetField ("useDotNetRelativeOrAbsolute",
					BindingFlags.Static | BindingFlags.GetField | BindingFlags.NonPublic);
				useDotNetRelativeOrAbsoluteOld = (bool) useDotNetRelativeOrAbsoluteField.GetValue (null);
				useDotNetRelativeOrAbsoluteField.SetValue (null, false);
			}

			try {
				Uri uri;

				uri = new Uri ("/foo", DotNetRelativeOrAbsolute);
				Assert.IsFalse (uri.IsAbsoluteUri, "#2");
				
				Assert.IsTrue (Uri.TryCreate("/foo", DotNetRelativeOrAbsolute, out uri), "#3");
				Assert.IsFalse (uri.IsAbsoluteUri, "#3a");

				if (useDotNetRelativeOrAbsoluteField != null) {
					uri = new Uri ("/foo", UriKind.RelativeOrAbsolute);
					Assert.IsTrue (uri.IsAbsoluteUri, "#4");

					Assert.IsTrue (Uri.TryCreate("/foo", UriKind.RelativeOrAbsolute, out uri), "#5");
					Assert.IsTrue (uri.IsAbsoluteUri, "#5a");

					useDotNetRelativeOrAbsoluteField.SetValue (null, true);
				}

				uri = new Uri ("/foo", UriKind.RelativeOrAbsolute);
				Assert.IsFalse (uri.IsAbsoluteUri, "#10");

				Assert.IsTrue (Uri.TryCreate("/foo", UriKind.RelativeOrAbsolute, out uri), "#11");
				Assert.IsFalse (uri.IsAbsoluteUri, "#11a");
			} finally {
				if (useDotNetRelativeOrAbsoluteField != null)
					useDotNetRelativeOrAbsoluteField.SetValue (null, useDotNetRelativeOrAbsoluteOld);
			}
		}

		[Test]
		// Bug #12631
		public void LocalPathWithBaseUrl ()
		{
			var mainUri = new Uri ("http://www.imdb.com");
			var uriPath = "/title/tt0106521";

			Uri result;
			Assert.IsTrue (Uri.TryCreate (mainUri, uriPath, out result), "#1");
			Assert.AreEqual ("http://www.imdb.com/title/tt0106521", result.ToString (), "#2");
		}

		[Test]
		public void GetSerializationInfoStringOnRelativeUri ()
		{
			var uri = new Uri ("/relative/path", UriKind.Relative);
			var result = uri.GetComponents (UriComponents.SerializationInfoString, UriFormat.UriEscaped);

			Assert.AreEqual (uri.OriginalString, result);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void GetSerializationInfoStringException ()
		{
			var uri = new Uri ("/relative/path", UriKind.Relative);
			uri.GetComponents (UriComponents.SerializationInfoString  | UriComponents.Host, UriFormat.UriEscaped);
		}

		[Test]
		public void UserInfo_EscapedLetter ()
		{
			var uri = new Uri ("https://first%61second@host");
			Assert.AreEqual ("firstasecond", uri.UserInfo);
		}

		[Test]
		public void UserInfo_EscapedAt ()
		{
			var userinfo =  "first%40second";
			var uri = new Uri ("https://" + userinfo + "@host");
			Assert.AreEqual (userinfo, uri.UserInfo);
		}

		[Test]
		public void UserInfo_EscapedChars ()
		{
			for (var ch = (char) 1; ch < 128; ch++) {
				var userinfo = Uri.EscapeDataString (ch.ToString ());
				try {
					new Uri (string.Format("http://{0}@localhost:80/", userinfo));
				} catch (Exception e) {
					Assert.Fail (string.Format("Unexpected {0} while building URI with username {1}", e.GetType ().Name, userinfo));
				}
			}
		}

		[Test]
		public void UserInfo_Spaces ()
		{
			const string userinfo = "test 1:pass 1";
			const string expected = "test%201:pass%201";

			try {
				var uri = new Uri (string.Format ("rtmp://{0}@test.com:333/live", userinfo));
				Assert.AreEqual (expected, uri.UserInfo);
			} catch (Exception e) {
				Assert.Fail (string.Format ("Unexpected {0} while building URI with username {1}", e.GetType ().Name, userinfo));
			}
		}

		// Covers #29864
		[Test]
		public void PathDotTrim ()
		{
			var baseUri = new Uri ("http://test.com", UriKind.Absolute);
			var relUri = new Uri ("path/dot./", UriKind.Relative);
			var uri = new Uri (baseUri, relUri);
			Assert.AreEqual ("http://test.com/path/dot./", uri.ToString ());
		}

		[Test]
		public void GuardedIPv6Address ()
		{
			var x = new Uri ("asfd://[::1]:123/");
			Assert.AreEqual ("[::1]", x.Host, "#1");
		}

		[Test]
		public void CombineWithUserSchema ()
		{
			var baseUri = new Uri ("zip:mem:///");
			var relativeUrl = "zip:mem:///foo/bar.txt";

			var result = new Uri (baseUri, relativeUrl);

			Assert.AreEqual ("zip:mem:///foo/bar.txt", result.ToString ());
		}

		[Test]
		public void Scheme_msapp ()
		{
			var uri = new Uri ("ms-app://s-1-15-2-1613647288");
			Assert.AreEqual ("ms-app", uri.Scheme);
		}

		[Test]
		public void CombineWithUnixAbsolutePath ()
		{
			var a = new Uri ("http://localhost/");
			var b = new Uri ("/foo", UriKind.RelativeOrAbsolute);
			var res = new Uri (a, b);

			Assert.AreEqual ("http://localhost/foo", res.ToString ());
		}

		[Test]
		public void ImplicitUnixFileWithUnicode ()
		{
			if (isWin32)
				Assert.Ignore ();

			Uri uri;
			Assert.IsTrue (Uri.TryCreate ("/Library/Frameworks/System.Runtim…ee", UriKind.Absolute, out uri), "#1");
			Assert.IsTrue (Uri.TryCreate (" /A/…", UriKind.Absolute, out uri), "#2");
		}

		[Test]
		public void UncValidPath ()
		{
			var uri = new Uri ("https://_foo/bar.html");
			Assert.AreEqual ("https", uri.Scheme);
		}

		[Test]
		public void ImplicitUnixFileWithUnicodeGetAbsoluleUri ()
		{
			if (isWin32)
				Assert.Ignore ();

			string escFilePath = "/Users/Текст.txt";
			string escUrl = new Uri (escFilePath, UriKind.Absolute).AbsoluteUri;
			Assert.AreEqual ("file:///Users/%D0%A2%D0%B5%D0%BA%D1%81%D1%82.txt", escUrl);
		}
	}
}
