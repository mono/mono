//
// SecurityAlgorithmSuiteTest.cs
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
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens;
using System.Net;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.Security.Cryptography.Xml;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class SecurityAlgorithmSuiteTest
	{
		static void AssertSecurityAlgorithmSuite (
			string defaultAsymmetricKeyWrapAlgorithm,
			string defaultAsymmetricSignatureAlgorithm,
			string defaultCanonicalizationAlgorithm,
			string defaultDigestAlgorithm,
			string defaultEncryptionAlgorithm,
			int defaultEncryptionKeyDerivationLength,
			int defaultSignatureKeyDerivationLength,
			int defaultSymmetricKeyLength,
			string defaultSymmetricKeyWrapAlgorithm,
			string defaultSymmetricSignatureAlgorithm,
			SecurityAlgorithmSuite target,
			string label)
		{
			Assert.AreEqual (defaultAsymmetricKeyWrapAlgorithm,
				target.DefaultAsymmetricKeyWrapAlgorithm,
				label + ".DefaultAsymmetricKeyWrapAlgorithm");
			Assert.AreEqual (defaultAsymmetricSignatureAlgorithm,
				target.DefaultAsymmetricSignatureAlgorithm,
				label + ".DefaultAsymmetricSignatureAlgorithm");
			Assert.AreEqual (defaultCanonicalizationAlgorithm,
				target.DefaultCanonicalizationAlgorithm,
				label + ".DefaultCanonicalizationAlgorithm");
			Assert.AreEqual (defaultDigestAlgorithm,
				target.DefaultDigestAlgorithm,
				label + ".DefaultDigestAlgorithm");
			Assert.AreEqual (defaultEncryptionAlgorithm,
				target.DefaultEncryptionAlgorithm,
				label + ".DefaultEncryptionAlgorithm");
			Assert.AreEqual (defaultEncryptionKeyDerivationLength,
				target.DefaultEncryptionKeyDerivationLength,
				label + ".DefaultEncryptionKeyDerivationLength");
			Assert.AreEqual (defaultSignatureKeyDerivationLength,
				target.DefaultSignatureKeyDerivationLength,
				label + ".DefaultSignatureKeyDerivationLength");
			Assert.AreEqual (defaultSymmetricKeyLength,
				target.DefaultSymmetricKeyLength,
				label + ".DefaultSymmetricKeyLength");
			Assert.AreEqual (defaultSymmetricKeyWrapAlgorithm,
				target.DefaultSymmetricKeyWrapAlgorithm,
				label + ".DefaultSymmetricKeyWrapAlgorithm");
			Assert.AreEqual (defaultSymmetricSignatureAlgorithm,
				target.DefaultSymmetricSignatureAlgorithm,
				label + ".DefaultSymmetricSignatureAlgorithm");
		}

		[Test]
		public void DefaultAlgorithm ()
		{
			Assert.AreEqual (
				SecurityAlgorithmSuite.Basic256,
				SecurityAlgorithmSuite.Default, "#1");
		}

		[Test]
		public void StaticPropertyValues ()
		{
			AssertSecurityAlgorithmSuite (
				EncryptedXml.XmlEncRSAOAEPUrl,
				SignedXml.XmlDsigRSASHA1Url,
				SignedXml.XmlDsigExcC14NTransformUrl,
				SignedXml.XmlDsigSHA1Url,
				EncryptedXml.XmlEncAES128Url,
				// enc, sig, sym
				128, 128, 128,
				EncryptedXml.XmlEncAES128KeyWrapUrl,
				SignedXml.XmlDsigHMACSHA1Url,
				SecurityAlgorithmSuite.Basic128,
				"Basic128");

			AssertSecurityAlgorithmSuite (
				EncryptedXml.XmlEncRSA15Url,
				SignedXml.XmlDsigRSASHA1Url,
				SignedXml.XmlDsigExcC14NTransformUrl,
				SignedXml.XmlDsigSHA1Url,
				EncryptedXml.XmlEncAES128Url,
				// enc, sig, sym
				128, 128, 128,
				EncryptedXml.XmlEncAES128KeyWrapUrl,
				SignedXml.XmlDsigHMACSHA1Url,
				SecurityAlgorithmSuite.Basic128Rsa15,
				"Basic128Rsa15");

			AssertSecurityAlgorithmSuite (
				EncryptedXml.XmlEncRSAOAEPUrl,
				SecurityAlgorithms.RsaSha256Signature,
				SignedXml.XmlDsigExcC14NTransformUrl,
				EncryptedXml.XmlEncSHA256Url,
				EncryptedXml.XmlEncAES128Url,
				// enc, sig, sym
				128, 128, 128,
				EncryptedXml.XmlEncAES128KeyWrapUrl,
				// Can't we get the same string from some const?
				SecurityAlgorithms.HmacSha256Signature,
				SecurityAlgorithmSuite.Basic128Sha256,
				"Basic128Sha256");

			AssertSecurityAlgorithmSuite (
				EncryptedXml.XmlEncRSA15Url,
				SecurityAlgorithms.RsaSha256Signature,
				SignedXml.XmlDsigExcC14NTransformUrl,
				EncryptedXml.XmlEncSHA256Url,
				EncryptedXml.XmlEncAES128Url,
				// enc, sig, sym
				128, 128, 128,
				EncryptedXml.XmlEncAES128KeyWrapUrl,
				// Can't we get the same string from some const?
				SecurityAlgorithms.HmacSha256Signature,
				SecurityAlgorithmSuite.Basic128Sha256Rsa15,
				"Basic128Sha256Rsa15");

			// ...192

			AssertSecurityAlgorithmSuite (
				EncryptedXml.XmlEncRSA15Url,
				SecurityAlgorithms.RsaSha256Signature,
				SignedXml.XmlDsigExcC14NTransformUrl,
				EncryptedXml.XmlEncSHA256Url,
				EncryptedXml.XmlEncAES192Url,
				// enc, sig, sym
				192, 192, 192,
				EncryptedXml.XmlEncAES192KeyWrapUrl,
				// Can't we get the same string from some const?
				SecurityAlgorithms.HmacSha256Signature,
				SecurityAlgorithmSuite.Basic192Sha256Rsa15,
				"Basic192Sha256Rsa15");

			// ...256

			AssertSecurityAlgorithmSuite (
				EncryptedXml.XmlEncRSAOAEPUrl,
				SecurityAlgorithms.RsaSha256Signature,
				SignedXml.XmlDsigExcC14NTransformUrl,
				EncryptedXml.XmlEncSHA256Url,
				EncryptedXml.XmlEncAES256Url,
				// enc, sig, sym
				256, 192, 256, // hmm, why 192 here?
				EncryptedXml.XmlEncAES256KeyWrapUrl,
				// Can't we get the same string from some const?
				SecurityAlgorithms.HmacSha256Signature,
				SecurityAlgorithmSuite.Basic256Sha256,
				"Basic256Sha256");

			// 3DES

			AssertSecurityAlgorithmSuite (
				EncryptedXml.XmlEncRSAOAEPUrl,
				SignedXml.XmlDsigRSASHA1Url,
				SignedXml.XmlDsigExcC14NTransformUrl,
				SignedXml.XmlDsigSHA1Url,
				EncryptedXml.XmlEncTripleDESUrl,
				// enc, sig, sym
				192, 192, 192,
				EncryptedXml.XmlEncTripleDESKeyWrapUrl,
				SignedXml.XmlDsigHMACSHA1Url,
				SecurityAlgorithmSuite.TripleDes,
				"TripleDes");

			AssertSecurityAlgorithmSuite (
				EncryptedXml.XmlEncRSA15Url,
				SignedXml.XmlDsigRSASHA1Url,
				SignedXml.XmlDsigExcC14NTransformUrl,
				SignedXml.XmlDsigSHA1Url,
				EncryptedXml.XmlEncTripleDESUrl,
				// enc, sig, sym
				192, 192, 192,
				EncryptedXml.XmlEncTripleDESKeyWrapUrl,
				SignedXml.XmlDsigHMACSHA1Url,
				SecurityAlgorithmSuite.TripleDesRsa15,
				"TripleDesRsa15");

			AssertSecurityAlgorithmSuite (
				EncryptedXml.XmlEncRSAOAEPUrl,
				SecurityAlgorithms.RsaSha256Signature,
				SignedXml.XmlDsigExcC14NTransformUrl,
				EncryptedXml.XmlEncSHA256Url,
				EncryptedXml.XmlEncTripleDESUrl,
				// enc, sig, sym
				192, 192, 192,
				EncryptedXml.XmlEncTripleDESKeyWrapUrl,
				// Can't we get the same string from some const?
				SecurityAlgorithms.HmacSha256Signature,
				SecurityAlgorithmSuite.TripleDesSha256,
				"TripleDesSha256");

			AssertSecurityAlgorithmSuite (
				EncryptedXml.XmlEncRSA15Url,
				SecurityAlgorithms.RsaSha256Signature,
				SignedXml.XmlDsigExcC14NTransformUrl,
				EncryptedXml.XmlEncSHA256Url,
				EncryptedXml.XmlEncTripleDESUrl,
				// enc, sig, sym
				192, 192, 192,
				EncryptedXml.XmlEncTripleDESKeyWrapUrl,
				// Can't we get the same string from some const?
				SecurityAlgorithms.HmacSha256Signature,
				SecurityAlgorithmSuite.TripleDesSha256Rsa15,
				"TripleDesSha256Rsa15");

		}
	}
}
#endif
