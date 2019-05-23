//
// NetTcpStyleUriParserTest.cs - Unit tests for System.NetTcpStyleUriParser
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
using System.IO;

namespace MonoTests.System {

	public class UnitTestNetTcpStyleUriParser: NetTcpStyleUriParser {

		static bool registered;

		public static bool Registered {
			get { return registered; }
		}

		protected override string GetComponents (Uri uri, UriComponents components, UriFormat format)
		{
			throw new UriFormatException ();
			// return components.ToString ();
		}

		protected override void InitializeAndValidate (Uri uri, out UriFormatException parsingError)
		{
			throw new NotImplementedException ();
			// base.InitializeAndValidate (uri, out parsingError);
		}

		protected override bool IsBaseOf (Uri baseUri, Uri relativeUri)
		{
			throw new NotSupportedException ();
			// return base.IsBaseOf (baseUri, relativeUri);
		}

		protected override bool IsWellFormedOriginalString (Uri uri)
		{
			throw new FormatException ();
			// return base.IsWellFormedOriginalString (uri);
		}

		protected override UriParser OnNewUri ()
		{
			throw new OverflowException ();
			// return base.OnNewUri ();
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
			throw new OutOfMemoryException ();
			// return base.Resolve (baseUri, relativeUri, out parsingError);
		}
	}

	[TestFixture]
	public class NetTcpStyleUriParserTest {

		private UnitTestNetTcpStyleUriParser parser;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			parser = new UnitTestNetTcpStyleUriParser ();
			// unit tests are being reused in CAS tests
			if (!UriParser.IsKnownScheme ("net.tcpx"))
				UriParser.Register (parser, "net.tcpx", 2);

			Assert.IsTrue (UnitTestNetTcpStyleUriParser.Registered, "Registered");
			// our parser code was called
		}

		[Test]
		public void NetTcpX ()
		{
			Uri uri = new Uri ("net.tcpx://www.example.com/");
			Assert.AreEqual (2, uri.Port, "Port");
			// OnRegister cannot be used to change the registering informations
		}

		[Test]
		[Category ("NotWorking")]
		public void NetTcpX_Methods ()
		{
			Uri uri = new Uri ("net.tcpx://www.example.com/");
			Assert.AreEqual (String.Empty, uri.GetComponents (UriComponents.Path, UriFormat.SafeUnescaped), "GetComponents");
			Assert.IsTrue (uri.IsBaseOf (uri), "IsBaseOf");
			Assert.IsTrue (uri.IsWellFormedOriginalString (), "IsWellFormedOriginalString");
			// ??? our parser doesn't seems to be called :(
		}

		[Test]
		public void SecureNetTcpX ()
		{
			Uri uri = new Uri ("snet.tcpx://www.example.com/");
			Assert.AreEqual (-1, uri.Port, "Port");
			// OnRegister cannot be used to change the registering informations
		}
	}
}

