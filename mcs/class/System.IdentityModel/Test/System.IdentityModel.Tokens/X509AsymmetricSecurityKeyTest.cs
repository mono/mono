//
// X509AsymmetricSecurityKeyTest.cs
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
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.IdentityModel.Selectors
{
	[TestFixture]
	public class X509AsymmetricSecurityKeyTest
	{
		static readonly X509Certificate2 cert = new X509Certificate2 (TestResourceHelper.GetFullPathOfResource ("Test/Resources/test.pfx"), "mono");
		static readonly X509Certificate2 cert2 = new X509Certificate2 (TestResourceHelper.GetFullPathOfResource ("Test/Resources/test.cer"));

		[Test]
		public void GetAsymmetricAlgorithm ()
		{
			X509AsymmetricSecurityKey key = new X509AsymmetricSecurityKey (cert);
			string name = EncryptedXml.XmlEncRSA15Url;
			AsymmetricAlgorithm alg = key.GetAsymmetricAlgorithm (name, false);
			Assert.IsNotNull (alg, "#1");
			alg = key.GetAsymmetricAlgorithm (name, true);
			Assert.IsNotNull (alg, "#2");

			key = new X509AsymmetricSecurityKey (cert2);
			alg = key.GetAsymmetricAlgorithm (name, false);
			Assert.IsNotNull (alg, "#3");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void GetAsymmetricAlgorithmWhereNoPrivKey ()
		{
			X509AsymmetricSecurityKey key = new X509AsymmetricSecurityKey (cert2);
			key.GetAsymmetricAlgorithm (EncryptedXml.XmlEncRSA15Url, true);
		}


		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		[Category ("NotDotNet")] // buggy FormatException occurs instead
		public void GetAsymmetricAlgorithmNullAlgName ()
		{
			X509AsymmetricSecurityKey key = new X509AsymmetricSecurityKey (cert);
			key.GetAsymmetricAlgorithm (null, false);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void GetAsymmetricAlgorithmDSA ()
		{
			X509AsymmetricSecurityKey key = new X509AsymmetricSecurityKey (cert);
			AsymmetricAlgorithm alg = key.GetAsymmetricAlgorithm (SignedXml.XmlDsigDSAUrl, false);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		[Category ("NotDotNet")] // buggy FormatException occurs instead
		public void GetAsymmetricAlgorithmHMACSHA1 ()
		{
			X509AsymmetricSecurityKey key = new X509AsymmetricSecurityKey (cert);
			key.GetAsymmetricAlgorithm (SignedXml.XmlDsigHMACSHA1Url, false);
		}

		[Test]
		public void IsSupportedAlgorithm ()
		{
			X509AsymmetricSecurityKey key = new X509AsymmetricSecurityKey (cert);
			Assert.IsTrue (key.IsSupportedAlgorithm (EncryptedXml.XmlEncRSA15Url), "#1");
			Assert.IsFalse (key.IsSupportedAlgorithm (SignedXml.XmlDsigDSAUrl), "#2");
			Assert.IsFalse (key.IsSupportedAlgorithm (SignedXml.XmlDsigHMACSHA1Url), "#3");
		}

		[Test]
		public void IsAsymmetricAlgorithm ()
		{
			X509AsymmetricSecurityKey key = new X509AsymmetricSecurityKey (cert);
			Assert.IsTrue (key.IsAsymmetricAlgorithm (EncryptedXml.XmlEncRSA15Url), "#1");
			Assert.IsTrue (key.IsAsymmetricAlgorithm (SignedXml.XmlDsigDSAUrl), "#2"); // It is asymmetric but not supported.
			Assert.IsFalse (key.IsAsymmetricAlgorithm (SignedXml.XmlDsigHMACSHA1Url), "#3");
		}

		[Test]
		// huh?
		//[ExpectedException (typeof (ArgumentNullException))]
		public void IsSupportedAlgorithmNullAlgName ()
		{
			X509AsymmetricSecurityKey key = new X509AsymmetricSecurityKey (cert);
			Assert.IsFalse (key.IsSupportedAlgorithm (null));
		}

		[Test]
		[Category ("NotDotNet")] // it throws FormatException, probably internal error message string formatting error.
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetHashAlgorithmForSignatureNull ()
		{
			new X509AsymmetricSecurityKey (cert).GetHashAlgorithmForSignature (null);
		}

		[Test]
		[Category ("NotDotNet")] // it throws FormatException, probably internal error message string formatting error.
		[ExpectedException (typeof (NotSupportedException))]
		public void GetHashAlgorithmForSignatureUnsupported ()
		{
			new X509AsymmetricSecurityKey (cert).GetHashAlgorithmForSignature (new HMACMD5 ().HashName);
		}

		[Test]
		public void GetHashAlgorithmForSignatureFine ()
		{
			X509AsymmetricSecurityKey k = new X509AsymmetricSecurityKey (cert);
			k.GetHashAlgorithmForSignature (SignedXml.XmlDsigRSASHA1Url);
			k.GetHashAlgorithmForSignature (SecurityAlgorithms.RsaSha256Signature);
		}
	}
}
#endif
