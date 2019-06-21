//
// UriParserTest.cs - Unit tests for System.UriParser
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
using System.Text;

namespace MonoTests.System {

	public class UnitTestUriParser: UriParser {

		private string scheme_name;
		private int default_port;
		private bool throw_on_register;
		private bool on_new_uri_called;

		public UnitTestUriParser ()
		{
		}

		public UnitTestUriParser (bool throwOnRegister)
		{
			throw_on_register = throwOnRegister;
		}

		public string SchemeName
		{
			get
			{
				return scheme_name;
			}
		}

		public int DefaultPort
		{
			get
			{
				return default_port;
			}
		}

		public bool OnNewUriCalled
		{
			get
			{
				return on_new_uri_called;
			}
		}

		public string _GetComponents (Uri uri, UriComponents components, UriFormat format)
		{
			return base.GetComponents (uri, components, format);
		}

		public void _InitializeAndValidate (Uri uri, out UriFormatException parserError)
		{
			base.InitializeAndValidate (uri, out parserError);
		}

		public bool _IsBaseOf (Uri baseUri, Uri relativeUri)
		{
			return base.IsBaseOf (baseUri, relativeUri);
		}

		public bool _IsWellFormedOriginalString (Uri uri)
		{
			return base.IsWellFormedOriginalString (uri);
		}

		public UriParser _OnNewUri ()
		{
			return base.OnNewUri ();
		}

		public void _OnRegister (string schemeName, int defaultPort)
		{
			base.OnRegister (schemeName, defaultPort);
		}

		public string _Resolve (Uri baseUri, Uri relativeUri, out UriFormatException parserError)
		{
			return base.Resolve (baseUri, relativeUri, out parserError);
		}

		protected override UriParser OnNewUri ()
		{
			on_new_uri_called = true;
			return base.OnNewUri ();
		}

		protected override void OnRegister (string schemeName, int defaultPort)
		{
			if (throw_on_register)
				throw new NotSupportedException ();
			scheme_name = schemeName;
			default_port = defaultPort;
			base.OnRegister (schemeName, defaultPort);
		}
	}

	[TestFixture]
	public class UriParserTest {

		private const string full_http = "http://www.example.com/Main_Page#FAQ?Edit";

		private string prefix;
		private Uri http;
		private Uri ftp, ftp2;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			prefix = "unit.test.";
			http = new Uri (full_http);
			ftp = new Uri ("ftp://username:password@ftp.example.com:21/with some spaces/mono.tgz");

			// Uses percent encoding on the username and password
			ftp2 = new Uri ("ftp://%75sername%3a%70assword@ftp.example.com:21/with some spaces/mono.tgz");
		}

		public string Prefix
		{
			get
			{
				return prefix;
			}
			set
			{
				prefix = value;
			}
		}

		[Test]
		public void GetComponents ()
		{
			UnitTestUriParser p = new UnitTestUriParser ();
			Assert.AreEqual ("http", p._GetComponents (http, UriComponents.Scheme, UriFormat.SafeUnescaped), "http.Scheme");
			Assert.AreEqual (String.Empty, p._GetComponents (http, UriComponents.UserInfo, UriFormat.SafeUnescaped), "http.UserInfo");
			Assert.AreEqual ("www.example.com", p._GetComponents (http, UriComponents.Host, UriFormat.SafeUnescaped), "http.Host");
			Assert.AreEqual (String.Empty, p._GetComponents (http, UriComponents.Port, UriFormat.SafeUnescaped), "http.Port");
			Assert.AreEqual ("Main_Page", p._GetComponents (http, UriComponents.Path, UriFormat.SafeUnescaped), "http.Path");
			Assert.AreEqual (String.Empty, p._GetComponents (http, UriComponents.Query, UriFormat.SafeUnescaped), "http.Query");
			Assert.AreEqual ("FAQ?Edit", p._GetComponents (http, UriComponents.Fragment, UriFormat.SafeUnescaped), "http.Fragment");
			Assert.AreEqual ("80", p._GetComponents (http, UriComponents.StrongPort, UriFormat.SafeUnescaped), "http.StrongPort");
			Assert.AreEqual (String.Empty, p._GetComponents (http, UriComponents.KeepDelimiter, UriFormat.SafeUnescaped), "http.KeepDelimiter");
			Assert.AreEqual ("www.example.com:80", p._GetComponents (http, UriComponents.HostAndPort, UriFormat.SafeUnescaped), "http.HostAndPort");
			Assert.AreEqual ("www.example.com:80", p._GetComponents (http, UriComponents.StrongAuthority, UriFormat.SafeUnescaped), "http.StrongAuthority");
			Assert.AreEqual (full_http, p._GetComponents (http, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped), "http.AbsoluteUri");
			Assert.AreEqual ("/Main_Page", p._GetComponents (http, UriComponents.PathAndQuery, UriFormat.SafeUnescaped), "http.PathAndQuery");
			Assert.AreEqual ("http://www.example.com/Main_Page", p._GetComponents (http, UriComponents.HttpRequestUrl, UriFormat.SafeUnescaped), "http.HttpRequestUrl");
			Assert.AreEqual ("http://www.example.com", p._GetComponents (http, UriComponents.SchemeAndServer, UriFormat.SafeUnescaped), "http.SchemeAndServer");
			Assert.AreEqual (full_http, p._GetComponents (http, UriComponents.SerializationInfoString, UriFormat.SafeUnescaped), "http.SerializationInfoString");
			// strange mixup
			Assert.AreEqual ("http://", p._GetComponents (http, UriComponents.Scheme | UriComponents.Port, UriFormat.SafeUnescaped), "http.Scheme+Port");
			Assert.AreEqual ("www.example.com#FAQ?Edit", p._GetComponents (http, UriComponents.Host | UriComponents.Fragment, UriFormat.SafeUnescaped), "http.Scheme+Port");
			Assert.AreEqual ("/Main_Page", p._GetComponents (http, UriComponents.Port | UriComponents.Path, UriFormat.SafeUnescaped), "http.Scheme+Port");
			Assert.AreSame (p, p._OnNewUri (), "OnNewUri");
		}

		[Test]
		public void GetComponents_Ftp ()
		{
			UnitTestUriParser p = new UnitTestUriParser ();
			Assert.AreEqual ("ftp", p._GetComponents (ftp, UriComponents.Scheme, UriFormat.Unescaped), "ftp.Scheme");
			Assert.AreEqual ("username:password", p._GetComponents (ftp, UriComponents.UserInfo, UriFormat.Unescaped), "ftp.UserInfo");
			Assert.AreEqual ("ftp.example.com", p._GetComponents (ftp, UriComponents.Host, UriFormat.Unescaped), "ftp.Host");
			Assert.AreEqual (String.Empty, p._GetComponents (ftp, UriComponents.Port, UriFormat.Unescaped), "ftp.Port");
			Assert.AreEqual ("with some spaces/mono.tgz", p._GetComponents (ftp, UriComponents.Path, UriFormat.Unescaped), "ftp.Path");
			Assert.AreEqual ("with%20some%20spaces/mono.tgz", p._GetComponents (ftp, UriComponents.Path, UriFormat.UriEscaped), "ftp.Path-UriEscaped");
			Assert.AreEqual ("with some spaces/mono.tgz", p._GetComponents (ftp, UriComponents.Path, UriFormat.SafeUnescaped), "ftp.Path-SafeUnescaped");
			Assert.AreEqual (String.Empty, p._GetComponents (ftp, UriComponents.Query, UriFormat.Unescaped), "ftp.Query");
			Assert.AreEqual (String.Empty, p._GetComponents (ftp, UriComponents.Fragment, UriFormat.Unescaped), "ftp.Fragment");
			Assert.AreEqual ("21", p._GetComponents (ftp, UriComponents.StrongPort, UriFormat.Unescaped), "ftp.StrongPort");
			Assert.AreEqual (String.Empty, p._GetComponents (ftp, UriComponents.KeepDelimiter, UriFormat.Unescaped), "http.KeepDelimiter");
			Assert.AreEqual ("ftp.example.com:21", p._GetComponents (ftp, UriComponents.HostAndPort, UriFormat.Unescaped), "http.HostAndPort");
			Assert.AreEqual ("username:password@ftp.example.com:21", p._GetComponents (ftp, UriComponents.StrongAuthority, UriFormat.Unescaped), "http.StrongAuthority");
			Assert.AreEqual ("ftp://username:password@ftp.example.com/with some spaces/mono.tgz", p._GetComponents (ftp, UriComponents.AbsoluteUri, UriFormat.Unescaped), "http.AbsoluteUri");
			Assert.AreEqual ("/with some spaces/mono.tgz", p._GetComponents (ftp, UriComponents.PathAndQuery, UriFormat.Unescaped), "http.PathAndQuery");
			Assert.AreEqual ("ftp://ftp.example.com/with some spaces/mono.tgz", p._GetComponents (ftp, UriComponents.HttpRequestUrl, UriFormat.Unescaped), "http.HttpRequestUrl");
			Assert.AreEqual ("ftp://ftp.example.com", p._GetComponents (ftp, UriComponents.SchemeAndServer, UriFormat.Unescaped), "http.SchemeAndServer");
			Assert.AreEqual ("ftp://username:password@ftp.example.com/with some spaces/mono.tgz", p._GetComponents (ftp, UriComponents.SerializationInfoString, UriFormat.Unescaped), "http.SerializationInfoString");
			Assert.AreSame (p, p._OnNewUri (), "OnNewUri");
			// strange mixup
			Assert.AreEqual ("ftp://username:password@", p._GetComponents (ftp, UriComponents.Scheme | UriComponents.UserInfo, UriFormat.Unescaped), "ftp.Scheme+UserInfo");
			Assert.AreEqual (":21/with some spaces/mono.tgz", p._GetComponents (ftp, UriComponents.Path | UriComponents.StrongPort, UriFormat.Unescaped), "ftp.Path+StrongPort");
		}

		[Test]
		public void GetComponents_Ftp2 ()
		{
			UnitTestUriParser p = new UnitTestUriParser ();
			Assert.AreEqual ("ftp", p._GetComponents (ftp2, UriComponents.Scheme, UriFormat.Unescaped), "ftp.Scheme");
			Assert.AreEqual ("username:password", p._GetComponents (ftp2, UriComponents.UserInfo, UriFormat.Unescaped), "ftp.UserInfo");
			Assert.AreEqual ("ftp.example.com", p._GetComponents (ftp2, UriComponents.Host, UriFormat.Unescaped), "ftp.Host");
			Assert.AreEqual (String.Empty, p._GetComponents (ftp2, UriComponents.Port, UriFormat.Unescaped), "ftp.Port");
			Assert.AreEqual ("with some spaces/mono.tgz", p._GetComponents (ftp2, UriComponents.Path, UriFormat.Unescaped), "ftp.Path");
			Assert.AreEqual ("with%20some%20spaces/mono.tgz", p._GetComponents (ftp2, UriComponents.Path, UriFormat.UriEscaped), "ftp.Path-UriEscaped");
			Assert.AreEqual ("with some spaces/mono.tgz", p._GetComponents (ftp2, UriComponents.Path, UriFormat.SafeUnescaped), "ftp.Path-SafeUnescaped");
			Assert.AreEqual (String.Empty, p._GetComponents (ftp2, UriComponents.Query, UriFormat.Unescaped), "ftp.Query");
			Assert.AreEqual (String.Empty, p._GetComponents (ftp2, UriComponents.Fragment, UriFormat.Unescaped), "ftp.Fragment");
			Assert.AreEqual ("21", p._GetComponents (ftp2, UriComponents.StrongPort, UriFormat.Unescaped), "ftp.StrongPort");
			Assert.AreEqual (String.Empty, p._GetComponents (ftp2, UriComponents.KeepDelimiter, UriFormat.Unescaped), "http.KeepDelimiter");
			Assert.AreEqual ("ftp.example.com:21", p._GetComponents (ftp2, UriComponents.HostAndPort, UriFormat.Unescaped), "http.HostAndPort");
			Assert.AreEqual ("username:password@ftp.example.com:21", p._GetComponents (ftp2, UriComponents.StrongAuthority, UriFormat.Unescaped), "http.StrongAuthority");
			Assert.AreEqual ("ftp://username:password@ftp.example.com/with some spaces/mono.tgz", p._GetComponents (ftp2, UriComponents.AbsoluteUri, UriFormat.Unescaped), "http.AbsoluteUri");
			Assert.AreEqual ("/with some spaces/mono.tgz", p._GetComponents (ftp2, UriComponents.PathAndQuery, UriFormat.Unescaped), "http.PathAndQuery");
			Assert.AreEqual ("ftp://ftp.example.com/with some spaces/mono.tgz", p._GetComponents (ftp2, UriComponents.HttpRequestUrl, UriFormat.Unescaped), "http.HttpRequestUrl");
			Assert.AreEqual ("ftp://ftp.example.com", p._GetComponents (ftp2, UriComponents.SchemeAndServer, UriFormat.Unescaped), "http.SchemeAndServer");
			Assert.AreEqual ("ftp://username:password@ftp.example.com/with some spaces/mono.tgz", p._GetComponents (ftp2, UriComponents.SerializationInfoString, UriFormat.Unescaped), "http.SerializationInfoString");
			Assert.AreSame (p, p._OnNewUri (), "OnNewUri");
			// strange mixup
			Assert.AreEqual ("ftp://username:password@", p._GetComponents (ftp2, UriComponents.Scheme | UriComponents.UserInfo, UriFormat.Unescaped), "ftp.Scheme+UserInfo");
			Assert.AreEqual (":21/with some spaces/mono.tgz", p._GetComponents (ftp2, UriComponents.Path | UriComponents.StrongPort, UriFormat.Unescaped), "ftp.Path+StrongPort");
		}

		// Test case for Xamarin#17665
		[Test]
		public void TestParseUserPath ()
		{
			var u = new Uri("https://a.net/1@1.msg");
			var result = u.GetComponents(UriComponents.Scheme | UriComponents.Host | UriComponents.Port | UriComponents.Path, UriFormat.UriEscaped);
			Assert.AreEqual (result, "https://a.net/1@1.msg", "parse@InUrl");
		}
		
		
		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void GetComponents_Null ()
		{
			UnitTestUriParser p = new UnitTestUriParser ();
			p._GetComponents (null, UriComponents.Host, UriFormat.SafeUnescaped);
		}

		[Test]
		public void GetComponents_BadUriComponents ()
		{
			UnitTestUriParser p = new UnitTestUriParser ();
			Assert.AreEqual (full_http, p._GetComponents (http, (UriComponents) Int32.MinValue, UriFormat.SafeUnescaped), "http");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void GetComponents_BadUriFormat ()
		{
			UnitTestUriParser p = new UnitTestUriParser ();
			p._GetComponents (http, UriComponents.Host, (UriFormat) Int32.MinValue);
		}

		[Test]
		public void InitializeAndValidate ()
		{
			UriFormatException error = null;
			UnitTestUriParser p = new UnitTestUriParser ();
			p._InitializeAndValidate (http, out error);
			Assert.IsNotNull (error, "out"); // authority/host couldn't be parsed ?!?!
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		// oh man, this is a bad boy.It should be ArgumentNullException.
		public void InitializeAndValidate_Null ()
		{
			UriFormatException error = null;
			UnitTestUriParser p = new UnitTestUriParser ();
			p._InitializeAndValidate (null, out error);
		}

		[Test]
		public void IsBaseOf ()
		{
			UnitTestUriParser p = new UnitTestUriParser ();
			Assert.IsTrue (p._IsBaseOf (http, http), "http-http");

			Uri u = new Uri ("http://www.example.com/Main_Page#FAQ");
			Assert.IsTrue (p._IsBaseOf (u, http), "http-1a");
			Assert.IsTrue (p._IsBaseOf (http, u), "http-1b");

			u = new Uri ("http://www.example.com/Main_Page");
			Assert.IsTrue (p._IsBaseOf (u, http), "http-2a");
			Assert.IsTrue (p._IsBaseOf (http, u), "http-2b");

			u = new Uri ("http://www.example.com/");
			Assert.IsTrue (p._IsBaseOf (u, http), "http-3a");
			Assert.IsTrue (p._IsBaseOf (http, u), "http-3b");

			u = new Uri ("http://www.example.com/Main_Page/");
			Assert.IsFalse (p._IsBaseOf (u, http), "http-4a");
			Assert.IsTrue (p._IsBaseOf (http, u), "http-4b");

			// docs says the UserInfo isn't evaluated, but...
			u = new Uri ("http://username:password@www.example.com/Main_Page");
			Assert.IsFalse (p._IsBaseOf (u, http), "http-5a");
			Assert.IsFalse (p._IsBaseOf (http, u), "http-5b");

			// scheme case sensitive ? no
			u = new Uri ("HTTP://www.example.com/Main_Page");
			Assert.IsTrue (p._IsBaseOf (u, http), "http-6a");
			Assert.IsTrue (p._IsBaseOf (http, u), "http-6b");

			// host case sensitive ? no
			u = new Uri ("http://www.Example.com/Main_Page");
			Assert.IsTrue (p._IsBaseOf (u, http), "http-7a");
			Assert.IsTrue (p._IsBaseOf (http, u), "http-7b");

			// path case sensitive ? no
			u = new Uri ("http://www.Example.com/MAIN_Page");
			Assert.IsTrue (p._IsBaseOf (u, http), "http-8a");
			Assert.IsTrue (p._IsBaseOf (http, u), "http-8b");

			// different scheme
			u = new Uri ("ftp://www.example.com/Main_Page");
			Assert.IsFalse (p._IsBaseOf (u, http), "http-9a");
			Assert.IsFalse (p._IsBaseOf (http, u), "http-9b");

			// different host
			u = new Uri ("http://www.example.org/Main_Page");
			Assert.IsFalse (p._IsBaseOf (u, http), "http-10a");
			Assert.IsFalse (p._IsBaseOf (http, u), "http-10b");

			// different port
			u = new Uri ("http://www.example.com:8080/");
			Assert.IsFalse (p._IsBaseOf (u, http), "http-11a");
			Assert.IsFalse (p._IsBaseOf (http, u), "http-11b");

			// specify default port
			u = new Uri ("http://www.example.com:80/");
			Assert.IsTrue (p._IsBaseOf (u, http), "http-12a");
			Assert.IsTrue (p._IsBaseOf (http, u), "http-12b");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void IsBaseOf_UriNull ()
		{
			UnitTestUriParser p = new UnitTestUriParser ();
			p._IsBaseOf (http, null);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void IsBaseOf_NullUri ()
		{
			UnitTestUriParser p = new UnitTestUriParser ();
			p._IsBaseOf (null, http);
		}

		[Test]
		public void IsWellFormedOriginalString ()
		{
			UnitTestUriParser p = new UnitTestUriParser ();
			Assert.IsTrue (p._IsWellFormedOriginalString (http), "http");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void IsWellFormedOriginalString_Null ()
		{
			UnitTestUriParser p = new UnitTestUriParser ();
			p._IsWellFormedOriginalString (null);
		}

		[Test]
		public void OnNewUri ()
		{
			string scheme = prefix + "on.new.uri";
			Assert.IsFalse (UriParser.IsKnownScheme (scheme), "IsKnownScheme-false");

			UnitTestUriParser p = new UnitTestUriParser ();
			UriParser.Register (p, scheme, 1999);

			Assert.IsFalse (p.OnNewUriCalled, "!Called");
			Uri uri = new Uri (scheme + "://www.example.com");
			Assert.IsTrue (p.OnNewUriCalled, "Called");
		}

		[Test]
		public void OnRegister ()
		{
			string scheme = prefix + "onregister";
			Assert.IsFalse (UriParser.IsKnownScheme (scheme), "IsKnownScheme-false");
			UnitTestUriParser p = new UnitTestUriParser ();
			try {
				UriParser.Register (p, scheme, 2005);
			}
			catch (NotSupportedException) {
				// special case / ordering
			}
			// if true then the registration is done before calling OnRegister
			Assert.IsTrue (UriParser.IsKnownScheme (scheme), "IsKnownScheme-true");
		}

		[Test]
		public void OnRegister2 ()
		{
			string scheme = prefix + "onregister2";
			Assert.IsFalse (UriParser.IsKnownScheme (scheme), "IsKnownScheme-false");
			UnitTestUriParser p = new UnitTestUriParser ();
			try {
				UriParser.Register (p, scheme, 2005);
				Uri uri = new Uri (scheme + "://foobar:2005");
				Assert.AreEqual (scheme, uri.Scheme, "uri-prefix");
				Assert.AreEqual (2005, uri.Port, "uri-port");
				
				Assert.AreEqual ("//foobar:2005", uri.LocalPath, "uri-localpath");
			}
			catch (NotSupportedException) {
				// special case / ordering
			}
			// if true then the registration is done before calling OnRegister
			Assert.IsTrue (UriParser.IsKnownScheme (scheme), "IsKnownScheme-true");
		}

		[Test]
		public void Resolve ()
		{
			UriFormatException error = null;
			UnitTestUriParser p = new UnitTestUriParser ();
			Assert.AreEqual (full_http, p._Resolve (http, http, out error), "http-http");
		}

		[Test]
		public void Resolve_UriNull ()
		{
			UriFormatException error = null;
			UnitTestUriParser p = new UnitTestUriParser ();
			Assert.AreEqual (full_http, p._Resolve (http, null, out error), "http-http");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Resolve_NullUri ()
		{
			UriFormatException error = null;
			UnitTestUriParser p = new UnitTestUriParser ();
			p._Resolve (null, http, out error);
			p._Resolve (http, null, out error);
		}

		[Test]
		public void IsKnownScheme_WellKnown ()
		{
			// from Uri.UriScheme* fields
			Assert.IsTrue (UriParser.IsKnownScheme ("file"), "file");
			Assert.IsTrue (UriParser.IsKnownScheme ("ftp"), "ftp");
			Assert.IsTrue (UriParser.IsKnownScheme ("gopher"), "gopher");
			Assert.IsTrue (UriParser.IsKnownScheme ("http"), "http");
			Assert.IsTrue (UriParser.IsKnownScheme ("https"), "https");
			Assert.IsTrue (UriParser.IsKnownScheme ("mailto"), "mailto");
			Assert.IsTrue (UriParser.IsKnownScheme ("net.pipe"), "net.pipe");
			Assert.IsTrue (UriParser.IsKnownScheme ("net.tcp"), "net.tcp");
			Assert.IsTrue (UriParser.IsKnownScheme ("news"), "news");
			Assert.IsTrue (UriParser.IsKnownScheme ("nntp"), "nntp");
			// inferred from class library
			Assert.IsTrue (UriParser.IsKnownScheme ("ldap"), "ldap");
			Assert.IsFalse (UriParser.IsKnownScheme ("ldaps"), "ldaps");
			// well known for not existing
			Assert.IsFalse (UriParser.IsKnownScheme ("unknown"), "unknown");

			// variations - mixed and upper case
			Assert.IsTrue (UriParser.IsKnownScheme ("FiLe"), "FiLe");
			Assert.IsTrue (UriParser.IsKnownScheme ("FTP"), "ftp");

			// see 496783
			Assert.IsFalse (UriParser.IsKnownScheme ("tcp"), "tcp");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void IsKnownScheme_ExtraSpace ()
		{
			// same result for space before, inside or after the scheme
			UriParser.IsKnownScheme ("ht tp");
			// this is undocumented (and I hate exceptions in a boolean method)
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void IsKnownScheme_Null ()
		{
			UriParser.IsKnownScheme (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void IsKnownScheme_Empty ()
		{
			UriParser.IsKnownScheme (String.Empty);
		}

		[Test]
		public void Register ()
		{
			string scheme = prefix + "register.mono";
			Assert.IsFalse (UriParser.IsKnownScheme (scheme), "IsKnownScheme-false");

			UnitTestUriParser p = new UnitTestUriParser ();
			UriParser.Register (p, scheme, 2005);
			Assert.AreEqual (scheme, p.SchemeName, "SchemeName");
			Assert.AreEqual (2005, p.DefaultPort, "DefaultPort");

			Assert.IsTrue (UriParser.IsKnownScheme (scheme), "IsKnownScheme-true");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Register_NullParser ()
		{
			UriParser.Register (null, prefix + "null.parser", 2006);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Register_NullScheme ()
		{
			UnitTestUriParser p = new UnitTestUriParser ();
			UriParser.Register (p, null, 2006);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Register_NegativePort ()
		{
			UnitTestUriParser p = new UnitTestUriParser ();
			UriParser.Register (p, prefix + "negative.port", -2);
		}

		[Test]
		public void Register_Minus1Port ()
		{
			UnitTestUriParser p = new UnitTestUriParser ();
			UriParser.Register (p, prefix + "minus1.port", -1);
		}

		[Test]
		public void Register_UInt16PortMinus1 ()
		{
			UnitTestUriParser p = new UnitTestUriParser ();
			UriParser.Register (p, prefix + "uint16.minus.1.port", UInt16.MaxValue - 1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Register_TooBigPort ()
		{
			UnitTestUriParser p = new UnitTestUriParser ();
			UriParser.Register (p, prefix + "too.big.port", UInt16.MaxValue);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ReRegister ()
		{
			string scheme = prefix + "re.register.mono";
			Assert.IsFalse (UriParser.IsKnownScheme (scheme), "IsKnownScheme-false");
			UnitTestUriParser p = new UnitTestUriParser ();
			UriParser.Register (p, scheme, 2005);
			Assert.IsTrue (UriParser.IsKnownScheme (scheme), "IsKnownScheme-true");
			UriParser.Register (p, scheme, 2006);
		}
	}
}

