//
// InMemorySymmetricSecurityKeyTest.cs
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
using System.IO;
using System.Text;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using NUnit.Framework;

using Key = System.IdentityModel.Tokens.InMemorySymmetricSecurityKey;
using AES = System.Security.Cryptography.RijndaelManaged;

using MonoTests.Helpers;

namespace MonoTests.System.IdentityModel.Tokens
{
	[TestFixture]
	public class InMemorySymmetricSecurityKeyTest
	{
		static X509Certificate2 cert;
		static byte [] raw;
		static byte [] wssc_label = Encoding.UTF8.GetBytes ("WS-SecureConversationWS-SecureConversation");

		static InMemorySymmetricSecurityKeyTest ()
		{
			cert = new X509Certificate2 (TestResourceHelper.GetFullPathOfResource ("Test/Resources/test.pfx"), "mono");
			// randomly generated with RijndaelManaged
			// GenerateIV() and GenerateKey().
			raw = Convert.FromBase64String ("eX2EeE969RCv/5Lx8OIGLHtJrSD5PzVzO3tTy9JxU58=");
		}

		[Test]
		public void CreateSimple ()
		{
			Key key = new Key (raw);
			Assert.AreEqual (256, key.KeySize, "#1");
			// the returned value must be a clone.
			Assert.IsFalse (Object.ReferenceEquals (key.GetSymmetricKey (), raw), "#2");
		}

		[Test]
		public void GetSymmetricAlgorithmAES ()
		{
			byte [] bytes = new byte [32];
			Key key = new Key (bytes);
			SymmetricAlgorithm alg = key.GetSymmetricAlgorithm (
				SecurityAlgorithms.Aes128Encryption);
			Assert.AreEqual (256, alg.KeySize, "#1-1");
			Assert.AreEqual (CipherMode.CBC, alg.Mode, "#1-2");
			Assert.AreEqual (PaddingMode.PKCS7, alg.Padding, "#1-3");
			alg = key.GetSymmetricAlgorithm (SecurityAlgorithms.Aes192Encryption);
			Assert.AreEqual (256, alg.KeySize, "#2-1");
			Assert.AreEqual (CipherMode.CBC, alg.Mode, "#2-2");
			Assert.AreEqual (PaddingMode.PKCS7, alg.Padding, "#2-3");
			alg = key.GetSymmetricAlgorithm (SecurityAlgorithms.Aes256Encryption);
			Assert.AreEqual (256, alg.KeySize, "#3-1");
			Assert.AreEqual (CipherMode.CBC, alg.Mode, "#3-2");
			Assert.AreEqual (PaddingMode.PKCS7, alg.Padding, "#3-3");

			alg = key.GetSymmetricAlgorithm (SecurityAlgorithms.Aes128KeyWrap);
			Assert.IsTrue (alg is AES, "#4");
			alg = key.GetSymmetricAlgorithm (SecurityAlgorithms.Aes192KeyWrap);
			Assert.IsTrue (alg is AES, "#5");
			alg = key.GetSymmetricAlgorithm (SecurityAlgorithms.Aes256KeyWrap);
			Assert.IsTrue (alg is AES, "#6");
			//alg = key.GetSymmetricAlgorithm (SecurityAlgorithms.TripleDesKeyWrap);
			//Assert.IsTrue (alg is TripleDES, "#7");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void GetSymmetricAlgorithm3VulnerableTDESEnc ()
		{
			byte [] bytes = new byte [24];
			Key key = new Key (bytes);
			// strange, TripleDesEncryption works with 32bytes key,
			// but TripleDesKeyWrap doesn't.
			key.GetSymmetricAlgorithm (SecurityAlgorithms.TripleDesEncryption);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void GetSymmetricAlgorithm3VulnerableTDESWrap ()
		{
			byte [] bytes = new byte [24];
			Key key = new Key (bytes);
			// strange, TripleDesEncryption works with 32bytes key,
			// but TripleDesKeyWrap doesn't.
			key.GetSymmetricAlgorithm (SecurityAlgorithms.TripleDesKeyWrap);
		}

		// ... so, after all what is the valid key size for TDES?

		[Test]
		public void GetSymmetricAlgorithmNullKey ()
		{
			Key key = new Key (raw);
			Assert.IsNotNull (key.GetSymmetricAlgorithm (SecurityAlgorithms.Aes192Encryption));
		}

		[Test]
		// hmm, no error
		public void GetSymmetricAlgorithmWrongSize ()
		{
			Key key = new Key (new byte [32]);
			Assert.IsNotNull (key.GetSymmetricAlgorithm (SecurityAlgorithms.Aes192Encryption));
		}

		[Test]
		// hmm, error?
		[ExpectedException (typeof (CryptographicException))]
		public void GetSymmetricAlgorithmWrongSizeDES ()
		{
			Key key = new Key (new byte [32]);
			Assert.IsNotNull (key.GetSymmetricAlgorithm (SecurityAlgorithms.TripleDesKeyWrap));
		}

		[Test]
		// no error???
		public void GetSymmetricAlgorithmWrongSize2 ()
		{
			AES aes = new AES ();
			aes.KeySize = 192;
			aes.GenerateKey ();
			Key key = new Key (aes.Key);
			Assert.IsNotNull (key.GetSymmetricAlgorithm (SecurityAlgorithms.Aes256Encryption));
		}

		[Test]
		public void GenerateDerivedKey ()
		{
			Key key = new Key (raw);
			byte [] nonce = new byte [256];

			byte [] derived = key.GenerateDerivedKey (
				SecurityAlgorithms.Psha1KeyDerivation,
				wssc_label, nonce, key.KeySize, 0);
			Assert.IsTrue (Convert.ToBase64String (derived) != Convert.ToBase64String (raw), "#4");
			// the precomputed derivation value.
			byte [] expected = Convert.FromBase64String ("50UfLeg58TbfADujVeafUAS8typGX9LvqLOXezK/eJY=");
			Assert.AreEqual (Convert.ToBase64String (expected), Convert.ToBase64String (derived), "#5");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))] // not ArgumentNullException?
		public void GenerateDerivedKeyNullAlgorithm ()
		{
			Key key = new Key (raw);
			byte [] nonce = new byte [256];

			key.GenerateDerivedKey (null, wssc_label, nonce, key.KeySize, 0);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))] // not ArgumentNullException?
		public void GenerateDerivedKeyUnsupportedAlgorithm ()
		{
			Key key = new Key (raw);
			byte [] nonce = new byte [256];

			key.GenerateDerivedKey ("urn:my-own-way", wssc_label, nonce, key.KeySize, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GenerateDerivedKeyNullLabel ()
		{
			Key key = new Key (raw);
			byte [] nonce = new byte [256];

			key.GenerateDerivedKey (
				SecurityAlgorithms.Psha1KeyDerivation,
				null, nonce, key.KeySize, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GenerateDerivedKeyNullNonce ()
		{
			Key key = new Key (raw);
			byte [] nonce = new byte [256];

			key.GenerateDerivedKey (
				SecurityAlgorithms.Psha1KeyDerivation,
				wssc_label, null, key.KeySize, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void GenerateDerivedKeyNegativeLength ()
		{
			Key key = new Key (raw);
			byte [] nonce = new byte [256];

			key.GenerateDerivedKey (
				SecurityAlgorithms.Psha1KeyDerivation,
				wssc_label, nonce, -32, 0);
		}

		[Test]
		public void GenerateDerivedKeyUnusualLength ()
		{
			Key key = new Key (raw);
			byte [] nonce = new byte [256];

			key.GenerateDerivedKey (
				SecurityAlgorithms.Psha1KeyDerivation,
				wssc_label, nonce, 5, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void GenerateDerivedKeyNegativeOffset ()
		{
			Key key = new Key (raw);
			byte [] nonce = new byte [256];

			key.GenerateDerivedKey (
				SecurityAlgorithms.Psha1KeyDerivation,
				wssc_label, nonce, -32, 0);
		}

		[Test]
		public void GenerateDerivedKeyUnusualOffset ()
		{
			Key key = new Key (raw);
			byte [] nonce = new byte [256];

			key.GenerateDerivedKey (
				SecurityAlgorithms.Psha1KeyDerivation,
				wssc_label, nonce, 5, 0);
		}

		[Test]
		public void IsAsymmetricAlgorithm ()
		{
			Key key = new Key (raw);
			Assert.IsFalse (key.IsAsymmetricAlgorithm (SecurityAlgorithms.Aes256KeyWrap), "#1");
			Assert.IsFalse (key.IsAsymmetricAlgorithm (SecurityAlgorithms.TripleDesEncryption), "#2");
			Assert.IsTrue (key.IsAsymmetricAlgorithm (SecurityAlgorithms.RsaOaepKeyWrap), "#3");
			Assert.IsFalse (key.IsAsymmetricAlgorithm (SecurityAlgorithms.Psha1KeyDerivation), "#4");
		}

		[Test]
		public void IsSymmetricAlgorithm ()
		{
			Key key = new Key (raw);
			Assert.IsTrue (key.IsSymmetricAlgorithm (SecurityAlgorithms.Aes256KeyWrap), "#1");
			Assert.IsTrue (key.IsSymmetricAlgorithm (SecurityAlgorithms.TripleDesEncryption), "#2");
			Assert.IsFalse (key.IsSymmetricAlgorithm (SecurityAlgorithms.RsaOaepKeyWrap), "#3");
			Assert.IsTrue (key.IsSymmetricAlgorithm (SecurityAlgorithms.Psha1KeyDerivation), "#4");
		}

		[Test]
		public void GetKeyedHashAlgorithm()
		{
			InMemorySymmetricSecurityKey key = new InMemorySymmetricSecurityKey(new byte[0]);

			Assert.That(() => key.GetKeyedHashAlgorithm(SecurityAlgorithms.HmacSha256Signature), Throws.Nothing);
		}

		[Test]
		public void IsSupportedAlgorithm()
		{
			InMemorySymmetricSecurityKey key = new InMemorySymmetricSecurityKey(new byte[0]);

			Assert.That(() => key.IsSupportedAlgorithm(SecurityAlgorithms.HmacSha256Signature), Is.True);
		}
	    
	}
}
#endif
