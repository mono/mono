//
// SignatureKeyTest.cs - NUnit Test Cases for SignatureKey
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using Microsoft.Web.Services.Security;
using System;
using System.Security.Cryptography;
using System.Web.Services.Protocols;

namespace MonoTests.MS.Web.Services.Security {

	[TestFixture]
	public class SignatureKeyTest : Assertion {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))] 
		public void ConstructorAsymmetricNull () 
		{
			AsymmetricAlgorithm key = null; // resolve ambiguity
			SignatureKey aek = new SignatureKey (key);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))] 
		public void ConstructorSymmetricNull () 
		{
			SymmetricAlgorithm key = null; // resolve ambiguity
			SignatureKey aek = new SignatureKey (key);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))] 
		public void ComputeSignatureNull () 
		{
			DSA dsa = DSA.Create ();
			dsa.ImportParameters (AllTests.GetDSAKey (true));
			SignatureKey sk = new SignatureKey (dsa);
			sk.ComputeSignature (null);
		}

		[Test]
		public void ComputeSignatureDSA () 
		{
			DSA dsa = DSA.Create ();
			dsa.ImportParameters (AllTests.GetDSAKey (true));
			SignatureKey sk = new SignatureKey (dsa);

			SignedXml signedXml = new SignedXml ();
			Reference r = new Reference ("http://www.go-mono.com/");
			signedXml.AddReference (r);
			sk.ComputeSignature (signedXml);
		}

		[Test]
		public void ComputeSignatureRSA () 
		{
			RSA rsa = RSA.Create ();
			rsa.ImportParameters (AllTests.GetRSAKey (true));
			SignatureKey sk = new SignatureKey (rsa);

			SignedXml signedXml = new SignedXml ();
			Reference r = new Reference ("http://www.go-mono.com/");
			signedXml.AddReference (r);
			sk.ComputeSignature (signedXml);
		}

		[Test]
		public void ComputeSignatureSymmetricAlgo () 
		{
			// default (should be Rjindael)
			SymmetricAlgorithm sa = SymmetricAlgorithm.Create ();
			sa.Key = new byte [16]; // 128 bits (all zeros)
			SignatureKey sk = new SignatureKey (sa);

			SignedXml signedXml = new SignedXml ();
			Reference r = new Reference ("http://www.go-mono.com/");
			signedXml.AddReference (r);
			sk.ComputeSignature (signedXml);
		}
	}
}