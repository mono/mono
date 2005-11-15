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

#if NET_2_0

using NUnit.Framework;

using System;
using System.Text;

namespace MonoTests.System {

	public class UnitTestUriParser : UriParser {

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

		public string SchemeName {
			get { return scheme_name; }
		}

		public int DefaultPort {
			get { return default_port; }
		}

		public bool OnNewUriCalled {
			get { return on_new_uri_called; }
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

		private string prefix;
		private Uri http;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			prefix = "unit.test.";
			http = new Uri ("http://www.mono-project.com");
		}

		public string Prefix {
			get { return prefix; }
			set { prefix = value; }
		}

		[Test]
		[Category ("NotWorking")]
		public void GetComponents ()
		{
			UnitTestUriParser p = new UnitTestUriParser ();
			Assert.AreEqual ("www.mono-project.com", p._GetComponents (http, UriComponents.Host, UriFormat.SafeUnescaped), "http");
			Assert.AreSame (p, p._OnNewUri (), "OnNewUri");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		[Category ("NotWorking")]
		public void GetComponents_Null ()
		{
			UnitTestUriParser p = new UnitTestUriParser ();
			p._GetComponents (null, UriComponents.Host, UriFormat.SafeUnescaped);
		}

		[Test]
		[Category ("NotWorking")]
		public void GetComponents_BadUriComponents ()
		{
			UnitTestUriParser p = new UnitTestUriParser ();
			Assert.AreEqual ("http://www.mono-project.com/", p._GetComponents (http, (UriComponents) Int32.MinValue, UriFormat.SafeUnescaped), "http");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void GetComponents_BadUriFormat ()
		{
			UnitTestUriParser p = new UnitTestUriParser ();
			p._GetComponents (http, UriComponents.Host, (UriFormat)Int32.MinValue);
		}

		[Test]
		[Category ("NotWorking")]
		public void InitializeAndValidate ()
		{
			UriFormatException error = null;
			UnitTestUriParser p = new UnitTestUriParser ();
			p._InitializeAndValidate (http, out error);
			Assert.IsNotNull (error, "out"); // authority/host couldn't be parsed ?!?!
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		[Category ("NotWorking")]
		public void InitializeAndValidate_Null ()
		{
			UriFormatException error = null;
			UnitTestUriParser p = new UnitTestUriParser ();
			p._InitializeAndValidate (null, out error);
		}

		[Test]
		[Category ("NotWorking")]
		public void IsBaseOf ()
		{
			UnitTestUriParser p = new UnitTestUriParser ();
			Assert.IsTrue (p._IsBaseOf (http, http), "http-http");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		[Category ("NotWorking")]
		public void IsBaseOf_UriNull ()
		{
			UnitTestUriParser p = new UnitTestUriParser ();
			p._IsBaseOf (http, null);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		[Category ("NotWorking")]
		public void IsBaseOf_NullUri ()
		{
			UnitTestUriParser p = new UnitTestUriParser ();
			p._IsBaseOf (null, http);
		}

		[Test]
		[Category ("NotWorking")]
		public void IsWellFormedOriginalString ()
		{
			UnitTestUriParser p = new UnitTestUriParser ();
			Assert.IsTrue (p._IsWellFormedOriginalString (http), "http");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		[Category ("NotWorking")]
		public void IsWellFormedOriginalString_Null ()
		{
			UnitTestUriParser p = new UnitTestUriParser ();
			p._IsWellFormedOriginalString (null);
		}

		[Test]
		[Category ("NotWorking")]
		public void OnNewUri ()
		{
			string scheme = prefix + "on.new.uri";
			Assert.IsFalse (UriParser.IsKnownScheme (scheme), "IsKnownScheme-false");

			UnitTestUriParser p = new UnitTestUriParser ();
			UriParser.Register (p, scheme, 1999);

			Assert.IsFalse (p.OnNewUriCalled, "!Called");
			Uri uri = new Uri (scheme + "://www.mono-project.com");
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
		[Category ("NotWorking")]
		public void Resolve ()
		{
			UriFormatException error = null;
			UnitTestUriParser p = new UnitTestUriParser ();
			Assert.AreEqual ("http://www.mono-project.com", p._Resolve (http, http, out error), "http-http");
		}

		[Test]
		[Category ("NotWorking")]
		public void Resolve_UriNull ()
		{
			UriFormatException error = null;
			UnitTestUriParser p = new UnitTestUriParser ();
			Assert.AreEqual ("http://www.mono-project.com", p._Resolve (http, null, out error), "http-http");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		[Category ("NotWorking")]
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
			// infered from class library
			Assert.IsTrue (UriParser.IsKnownScheme ("ldap"), "ldap");
			Assert.IsFalse (UriParser.IsKnownScheme ("ldaps"), "ldaps");
			// well known for not existing
			Assert.IsFalse (UriParser.IsKnownScheme ("unknown"), "unknown");

			// variations - mixed and upper case
			Assert.IsTrue (UriParser.IsKnownScheme ("FiLe"), "FiLe");
			Assert.IsTrue (UriParser.IsKnownScheme ("FTP"), "ftp");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		[Category ("NotWorking")]
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

#endif