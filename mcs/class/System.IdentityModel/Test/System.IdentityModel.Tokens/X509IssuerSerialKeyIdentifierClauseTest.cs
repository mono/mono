//
// X509IssuerSerialKeyIdentifierClauseTest.cs
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
using System;
using System.IdentityModel.Claims;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.IdentityModel.Selectors
{
	[TestFixture]
	public class X509IssuerSerialKeyIdentifierClauseTest
	{
		static readonly X509Certificate2 cert = new X509Certificate2 ("Test/Resources/test.pfx", "mono");
		static readonly X509Certificate2 cert2 = new X509Certificate2 ("Test/Resources/test2.pfx", "mono");
        static readonly X509Certificate2 cert3 = new X509Certificate2("Test/Resources/test_neg_serial.cer");

		[Test]
		public void Properties ()
		{
			X509IssuerSerialKeyIdentifierClause ic =
				new X509IssuerSerialKeyIdentifierClause (cert);
			Assert.AreEqual ("CN=Mono Test Root Agency", ic.IssuerName, "#1");
			Assert.AreEqual ("22491767666218099257720700881460366085", ic.IssuerSerialNumber, "#2");
			Assert.AreEqual (null, ic.ClauseType, "#3");
		}

        [Test]
        public void NegativeSerialNumber ()
        {
            var clause = new X509IssuerSerialKeyIdentifierClause (cert3);
            Assert.AreEqual ("CN=test, OU=cert, O=test, E=test@test.com", clause.IssuerName, "#1");
            Assert.AreEqual ("-168428216848510272180165529369113665228", clause.IssuerSerialNumber, "#2");
            Assert.AreEqual (null, clause.ClauseType, "#3");
        }
	}
}
