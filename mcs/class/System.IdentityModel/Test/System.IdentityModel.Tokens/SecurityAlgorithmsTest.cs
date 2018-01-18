//
// SecurityAlgorithmsTest.cs
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
using System.IdentityModel.Tokens;
using NUnit.Framework;

namespace MonoTests.System.IdentityModel.Tokens
{
	[TestFixture]
	public class SecurityAlgorithmsTest
	{
		[Test]
		public void Constants ()
		{
			Assert.AreEqual ("http://www.w3.org/2001/04/xmlenc#aes128-cbc", SecurityAlgorithms.Aes128Encryption);

			Assert.AreEqual ("http://www.w3.org/2001/04/xmlenc#kw-aes128", SecurityAlgorithms.Aes128KeyWrap);

			Assert.AreEqual ("http://www.w3.org/2001/04/xmlenc#aes192-cbc", SecurityAlgorithms.Aes192Encryption);

			Assert.AreEqual ("http://www.w3.org/2001/04/xmlenc#kw-aes192", SecurityAlgorithms.Aes192KeyWrap);

			Assert.AreEqual ("http://www.w3.org/2001/04/xmlenc#aes256-cbc", SecurityAlgorithms.Aes256Encryption);

			Assert.AreEqual ("http://www.w3.org/2001/04/xmlenc#kw-aes256", SecurityAlgorithms.Aes256KeyWrap);

			Assert.AreEqual ("http://www.w3.org/2001/04/xmlenc#des-cbc", SecurityAlgorithms.DesEncryption);

			Assert.AreEqual ("http://www.w3.org/2000/09/xmldsig#dsa-sha1", SecurityAlgorithms.DsaSha1Signature);

			Assert.AreEqual ("http://www.w3.org/2001/10/xml-exc-c14n#", SecurityAlgorithms.ExclusiveC14n);

			Assert.AreEqual ("http://www.w3.org/2001/10/xml-exc-c14n#WithComments", SecurityAlgorithms.ExclusiveC14nWithComments);

			Assert.AreEqual ("http://www.w3.org/2000/09/xmldsig#hmac-sha1", SecurityAlgorithms.HmacSha1Signature);

			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/02/sc/dk/p_sha1", SecurityAlgorithms.Psha1KeyDerivation);

			Assert.AreEqual ("http://www.w3.org/2001/04/xmlenc#ripemd160", SecurityAlgorithms.Ripemd160Digest);

			Assert.AreEqual ("http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p", SecurityAlgorithms.RsaOaepKeyWrap);

			Assert.AreEqual ("http://www.w3.org/2000/09/xmldsig#rsa-sha1", SecurityAlgorithms.RsaSha1Signature);

			Assert.AreEqual ("http://www.w3.org/2001/04/xmlenc#rsa-1_5", SecurityAlgorithms.RsaV15KeyWrap);

			Assert.AreEqual ("http://www.w3.org/2000/09/xmldsig#sha1", SecurityAlgorithms.Sha1Digest);

			Assert.AreEqual ("http://www.w3.org/2001/04/xmlenc#sha256", SecurityAlgorithms.Sha256Digest);

			Assert.AreEqual ("http://www.w3.org/2001/04/xmlenc#sha512", SecurityAlgorithms.Sha512Digest);

			Assert.AreEqual ("http://www.w3.org/2001/04/xmlenc#tripledes-cbc", SecurityAlgorithms.TripleDesEncryption);

			Assert.AreEqual ("http://www.w3.org/2001/04/xmlenc#kw-tripledes", SecurityAlgorithms.TripleDesKeyWrap);

			Assert.AreEqual ("http://www.w3.org/2001/04/xmldsig-more#hmac-sha256", SecurityAlgorithms.HmacSha256Signature);

			Assert.AreEqual ("http://www.w3.org/2001/04/xmldsig-more#rsa-sha256", SecurityAlgorithms.RsaSha256Signature);
			Assert.AreEqual ("http://schemas.xmlsoap.org/2005/02/trust/tlsnego#TLS_Wrap", SecurityAlgorithms.TlsSspiKeyWrap, "TlsSspiKeyWrap");
			Assert.AreEqual ("http://schemas.xmlsoap.org/2005/02/trust/spnego#GSS_Wrap", SecurityAlgorithms.WindowsSspiKeyWrap, "WindowsSspiKeyWrap");
		}
	}
}
#endif
