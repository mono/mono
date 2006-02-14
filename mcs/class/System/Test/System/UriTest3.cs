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

#if NET_2_0

using NUnit.Framework;

using System;

namespace MonoTests.System {

	[TestFixture]
	public class UriTest3 {

		private const string absolute = "http://www.mono-project.com/CAS";
		private const string relative = "server.com/directory/";

		[Test]
		public void Absolute_UriKind_Absolute ()
		{
			Uri uri = new Uri (absolute, UriKind.Absolute);
			Assert.AreEqual ("www.mono-project.com", uri.DnsSafeHost, "DnsSafeHost");
			Assert.IsTrue (uri.IsAbsoluteUri, "IsAbsoluteUri");
			Assert.AreEqual (absolute, uri.OriginalString, "OriginalString");
		}

		[Test]
		public void Relative_UriKind_Relative ()
		{
			Uri uri = new Uri (relative, UriKind.Relative);
			Assert.IsFalse (uri.IsAbsoluteUri, "IsAbsoluteUri");
			Assert.AreEqual (relative, uri.OriginalString, "OriginalString");
			Assert.IsFalse (uri.UserEscaped, "UserEscaped");
			// using any other property would throw an InvalidOperationException
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

		[Test]
		[Category ("NotWorking")]
		public void TryCreate_String_UriKind_Uri ()
		{
			Uri uri = null;
			Assert.IsTrue (Uri.TryCreate (absolute, UriKind.Absolute, out uri), "absolute-Absolute");
			Assert.AreEqual (absolute, uri.AbsoluteUri, "absolute-Absolute-AbsoluteUri");

			Assert.IsTrue (Uri.TryCreate (absolute, UriKind.RelativeOrAbsolute, out uri), "absolute-RelativeOrAbsolute");
			Assert.AreEqual (absolute, uri.AbsoluteUri, "absolute-RelativeOrAbsolute-AbsoluteUri");

			Assert.IsFalse (Uri.TryCreate (absolute, UriKind.Relative, out uri), "absolute-Relative");
			Assert.IsNull (uri, "absolute-Relative-uri");

			Assert.IsFalse (Uri.TryCreate (relative, UriKind.Absolute, out uri), "relative-Absolute");
			Assert.IsNull (uri, "relative-Relative-uri");

			Assert.IsTrue (Uri.TryCreate (relative, UriKind.RelativeOrAbsolute, out uri), "relative-RelativeOrAbsolute");
			Assert.AreEqual (relative, uri.OriginalString, "relative-RelativeOrAbsolute-OriginalString");

			Assert.IsTrue (Uri.TryCreate (relative, UriKind.Relative, out uri), "relative-Relative");
			Assert.AreEqual (relative, uri.OriginalString, "relative-RelativeOrAbsolute-OriginalString");
		}

		[Test]
		[Category ("NotWorking")]
		public void TryCreate_Uri_String_Uri ()
		{
			Uri baseUri = new Uri (absolute);
			Uri uri = null;

			Assert.IsTrue (Uri.TryCreate (baseUri, relative, out uri), "baseUri+relative");
			Assert.AreEqual ("http://www.mono-project.com/server.com/directory/", uri.AbsoluteUri, "baseUri+relative+AbsoluteUri");
			Assert.AreEqual ("http://www.mono-project.com/server.com/directory/", uri.OriginalString, "baseUri+relative+OriginalString");

			Assert.IsTrue (Uri.TryCreate (baseUri, absolute, out uri), "baseUri+absolute");
			Assert.AreEqual (absolute, uri.AbsoluteUri, "baseUri+absolute+AbsoluteUri");
			Assert.AreEqual (absolute, uri.OriginalString, "baseUri+absolute+OriginalString");

			Uri relativeUri = new Uri (relative, UriKind.Relative);
			Assert.IsFalse (Uri.TryCreate (relativeUri, relative, out uri), "relativeUri+relative");
			Assert.IsNull (uri, "relativeUri+relative+Uri");

			Assert.IsTrue (Uri.TryCreate (relativeUri, absolute, out uri), "relativeUri+absolute");
			Assert.AreEqual (absolute, uri.OriginalString, "relativeUri+absolute+OriginalString");

			string n = null;
			Assert.IsFalse (Uri.TryCreate (baseUri, n, out uri), "baseUri+null");
			Assert.IsNull (uri, "baseUri+null+Uri");
			Assert.IsFalse (Uri.TryCreate (relativeUri, n, out uri), "relativeUri+null");
			Assert.IsNull (uri, "relativeUri+null+Uri");
			Assert.IsFalse (Uri.TryCreate (null, relative, out uri), "null+relative");
			Assert.IsNull (uri, "null+relative+Uri");

			Assert.IsTrue (Uri.TryCreate (null, absolute, out uri), "null+absolute");
			Assert.AreEqual (absolute, uri.OriginalString, "null+absolute+OriginalString");
		}

		[Test]
		[Category ("NotWorking")]
		public void TryCreate_Uri_Uri_Uri ()
		{
			Uri baseUri = new Uri (absolute);
			Uri relativeUri = new Uri (relative, UriKind.Relative);
			Uri uri = null;

			Assert.IsTrue (Uri.TryCreate (baseUri, relativeUri, out uri), "baseUri+relativeUri");
			Assert.AreEqual ("http://www.mono-project.com/server.com/directory/", uri.AbsoluteUri, "baseUri+relativeUri+AbsoluteUri");
			Assert.AreEqual ("http://www.mono-project.com/server.com/directory/", uri.OriginalString, "baseUri+relativeUri+OriginalString");

			Assert.IsTrue (Uri.TryCreate (baseUri, baseUri, out uri), "baseUri+baseUri");
			Assert.AreEqual (absolute, uri.AbsoluteUri, "baseUri+baseUri+AbsoluteUri");
			Assert.AreEqual (absolute, uri.OriginalString, "baseUri+baseUri+OriginalString");

			Assert.IsFalse (Uri.TryCreate (relativeUri, relativeUri, out uri), "relativeUri+relativeUri");
			Assert.IsNull (uri, "relativeUri+relativeUri+Uri");

			Assert.IsFalse (Uri.TryCreate (relativeUri, baseUri, out uri), "relativeUri+baseUri");
			Assert.IsNull (uri, "relativeUri+baseUri+Uri");

			// a null relativeUri throws a NullReferenceException (see next test)
			Assert.IsFalse (Uri.TryCreate (null, relativeUri, out uri), "null+relativeUri");
			Assert.IsNull (uri, "null+relativeUri+Uri");
			Assert.IsFalse (Uri.TryCreate (null, baseUri, out uri), "null+baseUri");
			Assert.IsNull (uri, "null+baseUri+Uri");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void TryCreate_Uri_UriNull_Uri ()
		{
			Uri uri = null;
			Uri.TryCreate (new Uri (absolute), (Uri) null, out uri);
		}

		[Test]
		public void IsWellFormedUriString_Null ()
		{
			Assert.IsFalse (Uri.IsWellFormedUriString (null, UriKind.Absolute), "null");
		}

		[Test]
		[Category ("NotWorking")]
		public void IsWellFormedUriString_Http ()
		{
			Assert.IsFalse (Uri.IsWellFormedUriString ("http://www.go-mono.com/Main Page", UriKind.Absolute), "http/space");
			Assert.IsTrue (Uri.IsWellFormedUriString ("http://www.go-mono.com/Main%20Page", UriKind.Absolute), "http/%20");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IsWellFormedUriString_BadUriKind ()
		{
			Uri.IsWellFormedUriString ("http://www.go-mono.com/Main Page", (UriKind)Int32.MinValue);
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
		[ExpectedException (typeof (NullReferenceException))]
		public void IsBaseOf_Null ()
		{
			Uri http = new Uri ("http://www.mono-project.com/Main_Page#FAQ?Edit");
			http.IsBaseOf (null);
		}
	}
}

#endif
