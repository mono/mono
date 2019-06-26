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

namespace MonoTests.System {

	public class UnitTestGenericUriParser: GenericUriParser {

		static bool registered;

		public UnitTestGenericUriParser ()
			: base (GenericUriParserOptions.Default)
		{
		}

		public static bool Registered {
			get {
				return registered;
			}
		}

		protected override string GetComponents (Uri uri, UriComponents components, UriFormat format)
		{
			return base.GetComponents (uri, components, format);
		}

		protected override void InitializeAndValidate (Uri uri, out UriFormatException parsingError)
		{
			base.InitializeAndValidate (uri, out parsingError);
		}

		protected override bool IsBaseOf (Uri baseUri, Uri relativeUri)
		{
			return base.IsBaseOf (baseUri, relativeUri);
		}

		protected override bool IsWellFormedOriginalString (Uri uri)
		{
			return base.IsWellFormedOriginalString (uri);
		}

		protected override UriParser OnNewUri ()
		{
			return base.OnNewUri ();
		}

		protected override void OnRegister (string schemeName, int defaultPort)
		{
			registered = true;
			// try to mess up registration
			base.OnRegister (schemeName, 4040);
			base.OnRegister ("s" + schemeName, 4444);
		}

		protected override string Resolve (Uri baseUri, Uri relativeUri, out UriFormatException parsingError)
		{
			return base.Resolve (baseUri, relativeUri, out parsingError);
		}
	}

	[TestFixture]
	public class GenericUriParserTest {

		private UnitTestGenericUriParser parser;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			parser = new UnitTestGenericUriParser ();
			// unit tests are being reused in CAS tests
			if (!UriParser.IsKnownScheme ("generic"))
				UriParser.Register (parser, "generic", 1);

			Assert.IsTrue (UnitTestGenericUriParser.Registered, "Registered");
			// our parser code was called
		}

		[Test]
		public void Generic ()
		{
			Uri uri = new Uri ("generic://www.example.com/");
			Assert.AreEqual (1, uri.Port, "Port");
		}

		[Test]
		[Category ("NotWorking")]
		public void Generic_Methods ()
		{
			Uri uri = new Uri ("generic://www.example.com/");
			Assert.AreEqual (String.Empty, uri.GetComponents (UriComponents.Path, UriFormat.SafeUnescaped), "GetComponents");
			Assert.IsTrue (uri.IsBaseOf (uri), "IsBaseOf");
			Assert.IsTrue (uri.IsWellFormedOriginalString (), "IsWellFormedOriginalString");
		}

		[Test]
		public void SecureGeneric ()
		{
			Uri uri = new Uri ("sgenericx://www.example.com/");
			Assert.AreEqual (-1, uri.Port, "Port");
			// OnRegister cannot be used to change the registering informations
		}

		[Test]
		public void AllOptions ()
		{
			for (int i = 0; i < 512; i++) {
				GenericUriParserOptions gupo = (GenericUriParserOptions) i;
				Assert.IsNotNull (new GenericUriParser (gupo), gupo.ToString ());
			}
		}

		[Test]
		public void InvalidOptions ()
		{
			Assert.IsNotNull (new GenericUriParser ((GenericUriParserOptions) 512), "512");
			Assert.IsNotNull (new GenericUriParser ((GenericUriParserOptions) Int32.MinValue), "Int32.MinValue");
			// there are no check for invalid values
		}
	}
}

