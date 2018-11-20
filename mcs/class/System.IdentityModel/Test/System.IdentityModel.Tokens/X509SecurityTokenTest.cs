//
// X509SecurityTokenTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
#if !MOBILE
using System;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.IdentityModel.Selectors
{
	[TestFixture]
	public class X509SecurityTokenTest
	{
		static readonly X509Certificate2 cert = new X509Certificate2 (TestResourceHelper.GetFullPathOfResource ("Test/Resources/test.pfx"), "mono");
		static readonly X509Certificate2 cert2 = new X509Certificate2 (TestResourceHelper.GetFullPathOfResource ("Test/Resources/test2.pfx"), "mono");

		[Test]
		public void DefaultValues ()
		{
			UniqueId id = new UniqueId ();
			X509SecurityToken t = new X509SecurityToken (cert, id.ToString ());
			Assert.AreEqual (id.ToString (), t.Id, "#1");
			Assert.AreEqual (cert, t.Certificate, "#2");
			Assert.AreEqual (cert.NotBefore.ToUniversalTime (), t.ValidFrom, "#3");
			Assert.AreEqual (cert.NotAfter.ToUniversalTime (), t.ValidTo, "#4");
			Assert.AreEqual (1, t.SecurityKeys.Count, "#5");
		}

		[Test]
		public void MatchesKeyIdentifierClause ()
		{
			UniqueId id = new UniqueId ();
			X509SecurityToken t = new X509SecurityToken (cert, id.ToString ());
			LocalIdKeyIdentifierClause l =
				new LocalIdKeyIdentifierClause (id.ToString ());
			Assert.IsTrue (t.MatchesKeyIdentifierClause (l), "#1-1");

			l = new LocalIdKeyIdentifierClause ("#" + id.ToString ());
			Assert.IsFalse (t.MatchesKeyIdentifierClause (l), "#1-2");

			X509ThumbprintKeyIdentifierClause h =
				new X509ThumbprintKeyIdentifierClause (cert);
			Assert.IsTrue (t.MatchesKeyIdentifierClause (h), "#2-1");

			h = new X509ThumbprintKeyIdentifierClause (cert2);
			Assert.IsFalse (t.MatchesKeyIdentifierClause (h), "#2-2");

			X509IssuerSerialKeyIdentifierClause i =
				new X509IssuerSerialKeyIdentifierClause (cert);
			Assert.IsTrue (t.MatchesKeyIdentifierClause (i), "#3-1");

			i = new X509IssuerSerialKeyIdentifierClause (cert2);
			Assert.IsFalse (t.MatchesKeyIdentifierClause (i), "#3-2");

			X509RawDataKeyIdentifierClause s =
				new X509RawDataKeyIdentifierClause (cert);
			Assert.IsTrue (t.MatchesKeyIdentifierClause (s), "#4-1");

			s = new X509RawDataKeyIdentifierClause (cert2);
			Assert.IsFalse (t.MatchesKeyIdentifierClause (s), "#4-2");
		}
	}
}
#endif
