//
// UriTest3.cs - Even more (2.0 specific) unit tests for System.Uri
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using NUnit.Framework;

using System;

namespace MonoTests.System
{
	[TestFixture]
	public class UriTest3
	{
		private const string absolute = "http://www.mono-project.com/CAS";
		private const string relative = "server.com/directory/";

		[Test] // .ctor (String, UriKind)
		public void Constructor4_UriKind_Invalid ()
		{
			try {
				new Uri ("http://www.contoso.com", (UriKind) 666);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// The value '666' passed for the UriKind parameter
				// is invalid
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.Message.IndexOf ("'666'") != -1, "#5:" + ex.Message);
				Assert.IsNotNull (ex.Message.IndexOf ("UriKind") != -1, "#6:" + ex.Message);
				Assert.IsNull (ex.ParamName, "#7");
			}
		}

		[Test] // .ctor (String, UriKind)
		public void Constructor4_UriString_Null ()
		{
			try {
				new Uri ((string) null, (UriKind) 666);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("uriString", ex.ParamName, "#5");
			}
		}

		[Test]
		public void AbsoluteUri_RelativeUri ()
		{
			Uri uri1 = new Uri ("http://www.contoso.com/index.htm?x=2");
			Uri uri2 = new Uri ("http://www.contoso.com/foo/bar/index.htm#fragment");
			Uri relativeUri = uri1.MakeRelativeUri (uri2);

			try {
				string result = relativeUri.AbsoluteUri;
				Assert.Fail ("#1: " + result);
			} catch (InvalidOperationException ex) {
				// This operation is not supported for a relative URI
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void Absolute_UriKind_Absolute ()
		{
			Uri uri = new Uri (absolute, UriKind.Absolute);
			Assert.AreEqual ("www.mono-project.com", uri.DnsSafeHost, "#1");
			Assert.IsTrue (uri.IsAbsoluteUri, "#2");
			Assert.AreEqual (absolute, uri.OriginalString, "#3");
			Assert.AreEqual (absolute, uri.ToString (), "#4");
			Assert.IsFalse (uri.UserEscaped, "#5");
		}

		[Test]
		public void Relative_UriKind_Relative ()
		{
			Uri uri = new Uri (relative, UriKind.Relative);
			Assert.IsFalse (uri.IsAbsoluteUri, "#A1");
			Assert.AreEqual (relative, uri.OriginalString, "#A2");
			Assert.AreEqual (relative, uri.ToString (), "#A3");
			Assert.IsFalse (uri.UserEscaped, "#A4");

			uri = new Uri (string.Empty, UriKind.Relative);
			Assert.IsFalse (uri.IsAbsoluteUri, "#B1");
			Assert.AreEqual (string.Empty, uri.OriginalString, "#B2");
			Assert.AreEqual (string.Empty, uri.ToString (), "#B3");
			Assert.IsFalse (uri.UserEscaped, "#B4");

			uri = new Uri ("foo/bar", UriKind.Relative);
			Assert.IsFalse (uri.IsAbsoluteUri, "#C1");
			Assert.AreEqual ("foo/bar", uri.OriginalString, "#C2");
			Assert.AreEqual ("foo/bar", uri.ToString (), "#C3");
			Assert.IsFalse (uri.UserEscaped, "#C4");

			uri = new Uri ("/test.aspx", UriKind.Relative);
			Assert.IsFalse (uri.IsAbsoluteUri, "#D1");
			Assert.AreEqual ("/test.aspx", uri.OriginalString, "#D2");
			Assert.AreEqual ("/test.aspx", uri.ToString (), "#D3");
			Assert.IsFalse (uri.UserEscaped, "#D4");

			uri = new Uri ("", UriKind.Relative);
			Assert.IsFalse (uri.IsAbsoluteUri, "#E1");
			Assert.AreEqual ("", uri.OriginalString, "#E2");
			Assert.AreEqual ("", uri.ToString (), "#E3");
			Assert.IsFalse (uri.UserEscaped, "#E4");

			uri = new Uri ("a", UriKind.Relative);
			Assert.IsFalse (uri.IsAbsoluteUri, "#F1");
			Assert.AreEqual ("a", uri.OriginalString, "#F2");
			Assert.AreEqual ("a", uri.ToString (), "#F3");
			Assert.IsFalse (uri.UserEscaped, "#F4");
		}

		[Test]
		[ExpectedException (typeof (UriFormatException))]
		public void Relative_UriKind_Absolute ()
		{
			new Uri (relative, UriKind.Absolute);
		}

		[Test]
		[ExpectedException (typeof (UriFormatException))]
		public void Absolute_UriKind_Relative ()
		{
			new Uri (absolute, UriKind.Relative);
		}

		[Test] // TryCreate (String, UriKind, Uri)
		public void TryCreate1 ()
		{
			Uri uri;

			uri = new Uri ("http://dummy.com");
			Assert.IsTrue (Uri.TryCreate (absolute, UriKind.Absolute, out uri), "absolute-Absolute");
			Assert.AreEqual (absolute, uri.AbsoluteUri, "absolute-Absolute-AbsoluteUri");

			uri = new Uri ("http://dummy.com");
			Assert.IsTrue (Uri.TryCreate (absolute, UriKind.RelativeOrAbsolute, out uri), "absolute-RelativeOrAbsolute");
			Assert.AreEqual (absolute, uri.AbsoluteUri, "absolute-RelativeOrAbsolute-AbsoluteUri");

			uri = new Uri ("http://dummy.com");
			Assert.IsFalse (Uri.TryCreate (absolute, UriKind.Relative, out uri), "absolute-Relative");
			Assert.IsNull (uri, "absolute-Relative-uri");

			uri = new Uri ("http://dummy.com");
			Assert.IsFalse (Uri.TryCreate (relative, UriKind.Absolute, out uri), "relative-Absolute");
			Assert.IsNull (uri, "relative-Relative-uri");

			uri = new Uri ("http://dummy.com");
			Assert.IsTrue (Uri.TryCreate (relative, UriKind.RelativeOrAbsolute, out uri), "relative-RelativeOrAbsolute");
			Assert.AreEqual (relative, uri.OriginalString, "relative-RelativeOrAbsolute-OriginalString");

			uri = new Uri ("http://dummy.com");
			Assert.IsTrue (Uri.TryCreate (relative, UriKind.Relative, out uri), "relative-Relative");
			Assert.AreEqual (relative, uri.OriginalString, "relative-RelativeOrAbsolute-OriginalString");

			Assert.IsTrue (Uri.TryCreate ("http://mono-project.com/☕", UriKind.Absolute, out uri), "highunicode-Absolute");
			Assert.AreEqual("http://mono-project.com/%E2%98%95", uri.AbsoluteUri, "highunicode-Absolute-AbsoluteUri");
		}

		[Test] // TryCreate (String, UriKind, Uri)
		public void TryCreate1_UriKind_Invalid ()
		{
			Uri relativeUri = new Uri (relative, UriKind.Relative);
			Uri uri = relativeUri;

			try {
				Uri.TryCreate (absolute, (UriKind) 666, out uri);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// The value '666' passed for the UriKind parameter
				// is invalid
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.Message.IndexOf ("'666'") != -1, "#A5:" + ex.Message);
				Assert.IsNotNull (ex.Message.IndexOf ("UriKind") != -1, "#A6:" + ex.Message);
				Assert.IsNull (ex.ParamName, "#A7");

				Assert.IsNotNull (uri, "#A8");
				Assert.AreSame (relativeUri, uri, "#A9");
			}

			Assert.IsFalse (Uri.TryCreate ((string) null, (UriKind) 666, out uri), "#B1");
			Assert.IsNull (uri, "#B2");
		}

		[Test] // TryCreate (Uri, String, Uri)
		public void TryCreate2 ()
		{
			Uri baseUri = new Uri (absolute);
			Uri uri;

			uri = new Uri ("http://dummy.com");
			Assert.IsTrue (Uri.TryCreate (baseUri, relative, out uri), "baseUri+relative");
			Assert.AreEqual ("http://www.mono-project.com/server.com/directory/", uri.AbsoluteUri, "baseUri+relative+AbsoluteUri");
			Assert.AreEqual ("http://www.mono-project.com/server.com/directory/", uri.OriginalString, "baseUri+relative+OriginalString");

			uri = new Uri ("http://dummy.com");
			Assert.IsTrue (Uri.TryCreate (baseUri, absolute, out uri), "baseUri+absolute");
			Assert.AreEqual (absolute, uri.AbsoluteUri, "baseUri+absolute+AbsoluteUri");
			Assert.AreEqual (absolute, uri.OriginalString, "baseUri+absolute+OriginalString");

			uri = new Uri ("http://dummy.com");
			Uri relativeUri = new Uri (relative, UriKind.Relative);
			Assert.IsFalse (Uri.TryCreate (relativeUri, relative, out uri), "relativeUri+relative");
			Assert.IsNull (uri, "relativeUri+relative+Uri");

			uri = new Uri ("http://dummy.com");
			Assert.IsTrue (Uri.TryCreate (relativeUri, absolute, out uri), "relativeUri+absolute");
			Assert.AreEqual (absolute, uri.OriginalString, "relativeUri+absolute+OriginalString");

			uri = new Uri ("http://dummy.com");
			string n = null;
			Assert.IsFalse (Uri.TryCreate (baseUri, n, out uri), "baseUri+null");
			Assert.IsNull (uri, "baseUri+null+Uri");
			Assert.IsFalse (Uri.TryCreate (relativeUri, n, out uri), "relativeUri+null");
			Assert.IsNull (uri, "relativeUri+null+Uri");
			Assert.IsFalse (Uri.TryCreate (null, relative, out uri), "null+relative");
			Assert.IsNull (uri, "null+relative+Uri");

			uri = new Uri ("http://dummy.com");
			Assert.IsTrue (Uri.TryCreate (null, absolute, out uri), "null+absolute");
			Assert.AreEqual (absolute, uri.OriginalString, "null+absolute+OriginalString");
		}

		[Test] // TryCreate (Uri, Uri, Uri)
		public void TryCreate3 ()
		{
			Uri baseUri = new Uri (absolute);
			Uri relativeUri = new Uri (relative, UriKind.Relative);
			Uri uri;

			uri = new Uri ("http://dummy.com");
			Assert.IsTrue (Uri.TryCreate (baseUri, relativeUri, out uri), "baseUri+relativeUri");
			Assert.AreEqual ("http://www.mono-project.com/server.com/directory/", uri.AbsoluteUri, "baseUri+relativeUri+AbsoluteUri");
			Assert.AreEqual ("http://www.mono-project.com/server.com/directory/", uri.OriginalString, "baseUri+relativeUri+OriginalString");

			uri = new Uri ("http://dummy.com");
			Assert.IsTrue (Uri.TryCreate (baseUri, baseUri, out uri), "baseUri+baseUri");
			Assert.AreEqual (absolute, uri.AbsoluteUri, "baseUri+baseUri+AbsoluteUri");
			Assert.AreEqual (absolute, uri.OriginalString, "baseUri+baseUri+OriginalString");

			uri = new Uri ("http://dummy.com");
			Assert.IsFalse (Uri.TryCreate (relativeUri, relativeUri, out uri), "relativeUri+relativeUri");
			Assert.IsNull (uri, "relativeUri+relativeUri+Uri");

			uri = new Uri ("http://dummy.com");
			Assert.IsFalse (Uri.TryCreate (relativeUri, baseUri, out uri), "relativeUri+baseUri");
			Assert.IsNull (uri, "relativeUri+baseUri+Uri");

			uri = new Uri ("http://dummy.com");
			Assert.IsFalse (Uri.TryCreate (null, relativeUri, out uri), "null+relativeUri");
			Assert.IsNull (uri, "null+relativeUri+Uri");
			Assert.IsFalse (Uri.TryCreate (null, baseUri, out uri), "null+baseUri");
			Assert.IsNull (uri, "null+baseUri+Uri");
		}

		[Test] // TryCreate (Uri, Uri, out Uri)
		public void TryCreate3_RelativeUri_Null ()
		{
			Uri uri = null;
			Uri baseUri = new Uri (absolute);
			try {
				Uri.TryCreate (baseUri, (Uri) null, out uri);
#if NET_4_0
				Assert.IsNull (uri);
#else
				Assert.Fail ("throw NRE under FX 2.0");
#endif
			}
			catch (NullReferenceException) {
#if NET_4_0
				Assert.Fail ("does not throw NRE under FX 4.0");
#endif
			}
		}

		[Test]
		public void IsWellFormedUriString ()
		{
			Assert.IsFalse (Uri.IsWellFormedUriString ("http://www.go-mono.com/Main Page", UriKind.Absolute), "http/space");
			Assert.IsTrue (Uri.IsWellFormedUriString ("http://www.go-mono.com/Main%20Page", UriKind.Absolute), "http/%20");
			Assert.IsFalse (Uri.IsWellFormedUriString (null, UriKind.Absolute), "null");
			Assert.IsFalse (Uri.IsWellFormedUriString ("data", UriKind.Absolute), "data");
			Assert.IsTrue (Uri.IsWellFormedUriString ("http://www.go-mono.com/Main_Page#1", UriKind.Absolute), "http/hex");
		}

		[Test]
		public void IsWellFormedUriString_UriKind_Invalid ()
		{
			try {
				Uri.IsWellFormedUriString ("http://www.go-mono.com/Main Page",
					(UriKind) 666);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// The value '666' passed for the UriKind parameter
				// is invalid
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.Message.IndexOf ("'666'") != -1, "#5:" + ex.Message);
				Assert.IsNotNull (ex.Message.IndexOf ("UriKind") != -1, "#6:" + ex.Message);
				Assert.IsNull (ex.ParamName, "#7");
			}
		}

		[Test]
		public void Compare ()
		{
			Uri u1 = null;
			Uri u2 = null;
			Assert.AreEqual (0, Uri.Compare (u1, u2, UriComponents.AbsoluteUri, UriFormat.UriEscaped, StringComparison.CurrentCulture), "null-null");

			u1 = new Uri ("http://www.go-mono.com/Main Page");
			u2 = new Uri ("http://www.go-mono.com/Main%20Page");
			Assert.AreEqual (0, Uri.Compare (u1, u2, UriComponents.AbsoluteUri, UriFormat.Unescaped, StringComparison.CurrentCulture), "http/space-http/%20-unescaped");
			Assert.AreEqual (0, Uri.Compare (u1, u2, UriComponents.AbsoluteUri, UriFormat.UriEscaped, StringComparison.CurrentCulture), "http/space-http/%20-escaped");
			Assert.AreEqual (0, Uri.Compare (u1, u2, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.CurrentCulture), "http/space-http/%20-safe");
		}

		[Test]
		public void IsBaseOf ()
		{
			Uri http = new Uri ("http://www.mono-project.com/Main_Page#FAQ?Edit");
			Assert.IsTrue (http.IsBaseOf (http), "http-http");

			Uri u = new Uri ("http://www.mono-project.com/Main_Page#FAQ");
			Assert.IsTrue (u.IsBaseOf (http), "http-1a");
			Assert.IsTrue (http.IsBaseOf (u), "http-1b");

			u = new Uri ("http://www.mono-project.com/Main_Page");
			Assert.IsTrue (u.IsBaseOf (http), "http-2a");
			Assert.IsTrue (http.IsBaseOf (u), "http-2b");

			u = new Uri ("http://www.mono-project.com/");
			Assert.IsTrue (u.IsBaseOf (http), "http-3a");
			Assert.IsTrue (http.IsBaseOf (u), "http-3b");

			u = new Uri ("http://www.mono-project.com/Main_Page/");
			Assert.IsFalse (u.IsBaseOf (http), "http-4a");
			Assert.IsTrue (http.IsBaseOf (u), "http-4b");

			// docs says the UserInfo isn't evaluated, but...
			u = new Uri ("http://username:password@www.mono-project.com/Main_Page");
			Assert.IsFalse (u.IsBaseOf (http), "http-5a");
			Assert.IsFalse (http.IsBaseOf (u), "http-5b");

			// scheme case sensitive ? no
			u = new Uri ("HTTP://www.mono-project.com/Main_Page");
			Assert.IsTrue (u.IsBaseOf (http), "http-6a");
			Assert.IsTrue (http.IsBaseOf (u), "http-6b");

			// host case sensitive ? no
			u = new Uri ("http://www.Mono-Project.com/Main_Page");
			Assert.IsTrue (u.IsBaseOf (http), "http-7a");
			Assert.IsTrue (http.IsBaseOf (u), "http-7b");

			// path case sensitive ? no
			u = new Uri ("http://www.Mono-Project.com/MAIN_Page");
			Assert.IsTrue (u.IsBaseOf (http), "http-8a");
			Assert.IsTrue (http.IsBaseOf (u), "http-8b");

			// different scheme
			u = new Uri ("ftp://www.mono-project.com/Main_Page");
			Assert.IsFalse (u.IsBaseOf (http), "http-9a");
			Assert.IsFalse (http.IsBaseOf (u), "http-9b");

			// different host
			u = new Uri ("http://www.go-mono.com/Main_Page");
			Assert.IsFalse (u.IsBaseOf (http), "http-10a");
			Assert.IsFalse (http.IsBaseOf (u), "http-10b");

			// different port
			u = new Uri ("http://www.mono-project.com:8080/");
			Assert.IsFalse (u.IsBaseOf (http), "http-11a");
			Assert.IsFalse (http.IsBaseOf (u), "http-11b");

			// specify default port
			u = new Uri ("http://www.mono-project.com:80/");
			Assert.IsTrue (u.IsBaseOf (http), "http-12a");
			Assert.IsTrue (http.IsBaseOf (u), "http-12b");
		}

		[Test]
		public void IsBaseOf_Null ()
		{
			Uri http = new Uri ("http://www.mono-project.com/Main_Page#FAQ?Edit");
			try {
				http.IsBaseOf (null);
				Assert.Fail ();
			}
#if NET_4_0
			catch (ArgumentNullException) {
			}
#else
			catch (NullReferenceException) {
			}
#endif
		}

		[Test] 
		public void MakeRelativeUri ()
		{
			Uri uri1 = new Uri ("http://www.contoso.com/index.htm?x=2");
			Uri uri2 = new Uri ("http://www.contoso.com/foo/bar/index.htm#fragment");
			Uri uri3 = new Uri ("http://www.contoso.com/bar/foo/index.htm?y=1");
			Uri uri4 = new Uri ("http://www.contoso.com/bar/foo2/index.htm?x=0");
			Uri uri5 = new Uri ("https://www.contoso.com/bar/foo/index.htm?y=1");
			Uri uri6 = new Uri ("http://www.contoso2.com/bar/foo/index.htm?x=0");
			Uri uri7 = new Uri ("http://www.contoso2.com/bar/foo/foobar.htm?z=0&y=5");
			Uri uri8 = new Uri ("http://www.xxx.com/bar/foo/foobar.htm?z=0&y=5" + (char) 0xa9);
			Uri uri10 = new Uri ("mailto:xxx@xxx.com");
			Uri uri11 = new Uri ("mailto:xxx@xxx.com?subject=hola");
			Uri uri12 = new Uri ("mailto:xxx@mail.xxx.com?subject=hola");
			Uri uri13 = new Uri ("mailto:xxx@xxx.com/foo/bar");
			Uri uri14 = new Uri ("http://www.contoso.com/test1/");
			Uri uri15 = new Uri ("http://www.contoso.com/");
			Uri uri16 = new Uri ("http://www.contoso.com/test");

			AssertRelativeUri ("foo/bar/index.htm#fragment", uri1, uri2, "#A");
			AssertRelativeUri ("../../index.htm?x=2", uri2, uri1, "#B");
			AssertRelativeUri ("../../bar/foo/index.htm?y=1", uri2, uri3, "#C");
			AssertRelativeUri ("../../foo/bar/index.htm#fragment", uri3, uri2, "#D");
			AssertRelativeUri ("../foo2/index.htm?x=0", uri3, uri4, "#E");
			AssertRelativeUri ("../foo/index.htm?y=1", uri4, uri3, "#F");
			AssertRelativeUri ("?x=0", uri6, uri6, "#G");
			AssertRelativeUri ("foobar.htm?z=0&y=5", uri6, uri7, "#H");
			AssertRelativeUri ("?subject=hola", uri10, uri11, "#I");
			AssertRelativeUri ("/foo/bar", uri10, uri13, "#J");

			Uri relativeUri = uri1.MakeRelativeUri (uri8);
			Assert.IsTrue (relativeUri.IsAbsoluteUri, "#K1");
			Assert.AreEqual (uri8.ToString (), relativeUri.ToString (), "#K2");
			Assert.AreEqual (uri8.OriginalString, relativeUri.OriginalString, "#K3");

			relativeUri = uri10.MakeRelativeUri (uri12);
			Assert.IsTrue (relativeUri.IsAbsoluteUri, "#L1");
			Assert.AreEqual (uri12.ToString (), relativeUri.ToString (), "#L2");
			Assert.AreEqual (uri12.OriginalString, relativeUri.OriginalString, "#L3");

			relativeUri = uri4.MakeRelativeUri (uri6);
			Assert.IsTrue (relativeUri.IsAbsoluteUri, "#M1");
			Assert.AreEqual (uri6.ToString (), relativeUri.ToString (), "#M2");
			Assert.AreEqual (uri6.OriginalString, relativeUri.OriginalString, "#M3");

			relativeUri = uri4.MakeRelativeUri (uri5);
			Assert.IsTrue (relativeUri.IsAbsoluteUri, "#N1");
			Assert.AreEqual (uri5.ToString (), relativeUri.ToString (), "#N2");
			Assert.AreEqual (uri5.OriginalString, relativeUri.OriginalString, "#N3");

			AssertRelativeUri ("../", uri14, uri15, "#O");
			AssertRelativeUri ("./", uri16, uri15, "#P");

			Uri a1 = new Uri ("http://something/something2/test/");
			Uri a2 = new Uri ("http://something/something2/");
			Uri a3 = new Uri ("http://something/something2/test");
			Uri a4 = new Uri ("http://something/something2");

			AssertRelativeUri ("../", a1, a2, "Q1");
			AssertRelativeUri ("../../something2", a1, a4, "Q2");
			AssertRelativeUri ("./", a3, a2, "Q3");
			AssertRelativeUri ("../something2", a3, a4, "Q4");

			Uri b1 = new Uri ("http://something/test/");
			Uri b2 = new Uri ("http://something/");
			Uri b3 = new Uri ("http://something/test");
			Uri b4 = new Uri ("http://something");
			
			AssertRelativeUri ("../", b1, b2, "R1");
			AssertRelativeUri ("../", b1, b4, "R2");
			AssertRelativeUri ("./", b3, b2, "R3");
			AssertRelativeUri ("./", b3, b4, "R4");

			Uri c1 = new Uri ("C:\\something\\something2\\test\\");
			Uri c2 = new Uri ("C:\\something\\something2\\");
			Uri c3 = new Uri ("C:\\something\\something2\\test");
			Uri c4 = new Uri ("C:\\something\\something2");
			
			AssertRelativeUri ("../", c1, c2, "S1");
			AssertRelativeUri ("../../something2", c1, c4, "S2");
			AssertRelativeUri ("./", c3, c2, "S3");
			AssertRelativeUri ("../something2", c3, c4, "S4");

			Uri d1 = new Uri ("C:\\something\\test\\");
			Uri d2 = new Uri ("C:\\something\\");
			Uri d3 = new Uri ("C:\\something\\test");
			Uri d4 = new Uri ("C:\\something");
			
			AssertRelativeUri ("../", d1, d2, "T1");
			AssertRelativeUri ("../../something", d1, d4, "T2");
			AssertRelativeUri ("./", d3, d2, "T3");
			AssertRelativeUri ("../something", d3, d4, "T4");

			Uri e1 = new Uri ("C:\\something\\");
			Uri e2 = new Uri ("C:\\");
			Uri e3 = new Uri ("C:\\something");
			
			AssertRelativeUri ("../", e1, e2, "U1");
			AssertRelativeUri ("./", e3, e2, "U2");
			AssertRelativeUri ("", e1, e1, "U3");
			AssertRelativeUri ("", e3, e3, "U4");
			AssertRelativeUri ("../something", e1, e3, "U5");
			AssertRelativeUri ("something/", e3, e1, "U6");
			AssertRelativeUri ("something", e2, e3, "U7");
		}

		[Test]
		public void MakeRelativeUri_Uri_Null ()
		{
			Uri uri = new Uri ("http://test.com");
			try {
				uri.MakeRelativeUri ((Uri) null);
				Assert.Fail ("#1");
			}
#if NET_4_0
			catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("uri", ex.ParamName, "#6");
			}
#else
			catch (NullReferenceException) {
				// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=299942
			}
#endif
		}

		[Test] // LAMESPEC: see bug #321113
		public void OriginalStringRelative ()
		{
			Uri k1 = new Uri ("http://www.mono-project.com");
			Uri k2 = new Uri (k1, "docs");
			Assert.AreEqual ("http://www.mono-project.com/docs", k2.OriginalString, "#1");

			Uri a = new Uri ("http://www.mono-project.com:808/foo");
			Uri b = new Uri (a, "../docs?queryyy#% %20%23%25bar");
			Assert.AreEqual ("http://www.mono-project.com:808/docs?queryyy#% %20%23%25bar", b.OriginalString, "#2");

			Uri c = new Uri ("http://www.mono-project.com:808/foo");
			Uri d = new Uri (a, "../docs?queryyy#%20%23%25bar");
			Assert.AreEqual ("http://www.mono-project.com:808/docs?queryyy#%20%23%25bar", d.OriginalString, "#3");

			Uri e = new Uri ("http://www.mono-project.com:909");
			Uri f = new Uri (e, "http://www.mono-project.com:606/docs");
			Assert.AreEqual ("http://www.mono-project.com:606/docs", f.OriginalString, "#4");

			Uri g = new Uri ("http://www.mono-project.com:303/foo");
			Uri h = new Uri (g, "?query");
			// it doesn't work. MS.NET also returns incorrect URI: ..303/?query
			// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=412604
			//Assert.AreEqual ("http://www.mono-project.com:303/foo?query", h.OriginalString, "#5");
		}

		[Test]
		public void PathAndQuery_RelativeUri ()
		{
			Uri uri1 = new Uri ("http://www.contoso.com/index.htm?x=2");
			Uri uri2 = new Uri ("http://www.contoso.com/foo/bar/index.htm#fragment");
			Uri relativeUri = uri1.MakeRelativeUri (uri2);

			try {
				string result = relativeUri.PathAndQuery;
				Assert.Fail ("#1: " + result);
			} catch (InvalidOperationException ex) {
				// This operation is not supported for a relative URI
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void Query_RelativeUri ()
		{
			Uri uri1 = new Uri ("http://www.contoso.com/index.htm?x=2");
			Uri uri2 = new Uri ("http://www.contoso.com/foo/bar/index.htm#fragment");
			Uri relativeUri = uri1.MakeRelativeUri (uri2);

			try {
				string result = relativeUri.Query;
				Assert.Fail ("#1: " + result);
			} catch (InvalidOperationException ex) {
				// This operation is not supported for a relative URI
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void Scheme_RelativeUri ()
		{
			Uri uri1 = new Uri ("http://www.contoso.com/index.htm?x=2");
			Uri uri2 = new Uri ("http://www.contoso.com/foo/bar/index.htm#fragment");
			Uri relativeUri = uri1.MakeRelativeUri (uri2);

			try {
				string result = relativeUri.Scheme;
				Assert.Fail ("#1: " + result);
			} catch (InvalidOperationException ex) {
				// This operation is not supported for a relative URI
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void UnescapeDataString ()
		{
			Assert.AreEqual ("/new folder/", Uri.UnescapeDataString ("/new%20folder/"));
			Assert.AreEqual ("/new folder/", Uri.UnescapeDataString ("/new%20%66older/"));
			Assert.AreEqual ("/new+folder/", Uri.UnescapeDataString ("/new+folder/"));
		}

		void AssertRelativeUri (string expected, Uri uri1, Uri uri2, string msg)
		{
			Uri relativeUri;
			relativeUri = uri1.MakeRelativeUri (uri2);
			
			Assert.IsFalse (relativeUri.IsAbsoluteUri, msg + "1");
			Assert.AreEqual (expected, relativeUri.ToString (), msg + "2");
			Assert.AreEqual (expected, relativeUri.OriginalString, msg + "3");
			Assert.IsFalse (relativeUri.UserEscaped, msg + "4");
		}

		[Test]
		public void DontCheckHostWithCustomParsers ()
		{
			UriParser.Register (new TolerantUriParser (), "assembly", 0);
			try {
				new Uri ("assembly://Spring.Core, Version=1.2.0.20001, Culture=neutral, "
					+ "PublicKeyToken=null/Spring.Objects.Factory.Xml/spring-objects-1.1.xsd");
			} catch (UriFormatException) {
				Assert.Fail ("Spring Uri is expected to work.");
			}
		}

		[Test]
		public void ParseShortNameAsRelativeOrAbsolute ()
		{
			new Uri ("x", UriKind.RelativeOrAbsolute);
		}

		private class TolerantUriParser : GenericUriParser
		{
			private const GenericUriParserOptions DefaultOptions
				= GenericUriParserOptions.Default
				| GenericUriParserOptions.GenericAuthority
				| GenericUriParserOptions.AllowEmptyAuthority;
			
			public TolerantUriParser()
							: base(DefaultOptions)
			{
			}
		}

		[Test]
		public void DomainLabelLength ()
		{
			UriHostNameType type = Uri.CheckHostName ("3.141592653589793238462643383279502884197169399375105820974944592.com");
			Assert.AreEqual (UriHostNameType.Dns, type, "DomainLabelLength#1");
			type = Uri.CheckHostName ("3141592653589793238462643383279502884197169399375105820974944592.com");
			Assert.AreEqual (UriHostNameType.Unknown, type, "DomainLabelLength#2");
			type = Uri.CheckHostName ("3.1415926535897932384626433832795028841971693993751058209749445923.com");
			Assert.AreEqual (UriHostNameType.Unknown, type, "DomainLabelLength#2");
			type = Uri.CheckHostName ("3.141592653589793238462643383279502884197169399375105820974944592._om");
			Assert.AreEqual (UriHostNameType.Unknown, type, "DomainLabelLength#3");
		}
	}
}

