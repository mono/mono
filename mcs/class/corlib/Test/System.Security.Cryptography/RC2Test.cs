//
// TestSuite.System.Security.Cryptography.RC2Test.cs
//
// Authors:
//      Andrew Birkett (andy@nobugs.org)
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Security.Cryptography;

using NUnit.Framework;

namespace MonoTests.System.Security.Cryptography {

	[TestFixture]
	public class RC2Test : Assertion {

		[Test]
		public void DefaultLength ()
		{
			RC2 algo = RC2.Create ();
			AssertEquals ("Key Size", 128, algo.KeySize);
			AssertEquals ("Key Length", 16, algo.Key.Length);
			AssertEquals ("IV Length", 8, algo.IV.Length);
		}

		private void CheckECB (int effective_bits, byte[] key, byte[] pt, byte[] expected)
		{
			RC2 c = RC2.Create ();
			c.Mode = CipherMode.ECB;
			c.Padding = PaddingMode.Zeros;
			c.Key = key;
			AssertEquals ("KeySize", key.Length * 8, c.KeySize);
			c.EffectiveKeySize = effective_bits;

			ICryptoTransform encryptor = c.CreateEncryptor ();
			ICryptoTransform decryptor = c.CreateDecryptor ();

			byte[] ct = new byte [pt.Length];
			int n = encryptor.TransformBlock (pt, 0, pt.Length, ct, 0);
			AssertEquals ("EncryptLen", n, pt.Length);
			for (int i=0; i < n; i++) {
				AssertEquals ("Encrypt" + i, ct [i], expected [i]);
			}

			byte[] rt = new byte [ct.Length];
			n = decryptor.TransformBlock (ct, 0, ct.Length, rt, 0);
			AssertEquals ("DecryptLen", n, ct.Length);
			for (int i=0; i < n; i++) {
				AssertEquals ("Decrypt" + i, rt [i], pt [i]);
			}
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void RFC2268Vector_1 ()
		{
			byte[] key = { 0, 0, 0, 0, 0, 0, 0, 0 };
			byte[] pt =  { 0, 0, 0, 0, 0, 0, 0, 0 };
			byte[] ct =  { 0xeb, 0xb7, 0x73, 0xf9, 0x93, 0x27, 0x8e, 0xff };

			// we don't support EffectiveKeySize != KeySize to match MS implementation
			CheckECB (63, key, pt, ct);
		}

		[Test]
		public void RFC2268Vector_2 ()
		{
			byte[] key = { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff };
			byte[] pt = { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff };
			byte[] ct = { 0x27, 0x8b, 0x27, 0xe4, 0x2e, 0x2f, 0x0d, 0x49 };

			CheckECB (64, key, pt, ct);
		}
	
		[Test]
		public void RFC2268Vector_3 ()
		{
			byte[] key = { 0x30, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
			byte[] pt = { 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 };
			byte[] ct = { 0x30, 0x64, 0x9e, 0xdf, 0x9b, 0xe7, 0xd2, 0xc2 };

			CheckECB (64, key, pt, ct);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void RFC2268Vector_4 ()
		{
			byte[] key = { 0x88 };
			byte[] pt = { 0, 0, 0, 0, 0, 0, 0, 0 };
			byte[] ct = { 0x61, 0xa8, 0xa2, 0x44, 0xad, 0xac, 0xcc, 0xf0 };

			// we don't support KeySize < 40 to match MS implementation
			CheckECB (64, key, pt, ct);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void RFC2268Vector_5 ()
		{
			byte[] key = { 0x88, 0xbc, 0xa9, 0x0e, 0x90, 0x87, 0x5a };
			byte[] pt = { 0, 0, 0, 0, 0, 0, 0, 0 };
			byte[] ct = { 0x6c, 0xcf, 0x43, 0x08, 0x97, 0x4c, 0x26, 0x7f };

			// we don't support EffectiveKeySize != KeySize to match MS implementation
			CheckECB (64, key, pt, ct);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void RFC2268Vector_6 ()
		{
		 	byte[] key = { 0x88, 0xbc, 0xa9, 0x0e,  0x90, 0x87, 0x5a, 0x7f, 
		 		       0x0f, 0x79, 0xc3, 0x84,  0x62, 0x7b, 0xaf, 0xb2 };
		 	byte[] pt = { 0, 0, 0, 0, 0, 0, 0, 0 };
		 	byte[] ct = { 0x1a, 0x80, 0x7d, 0x27, 0x2b, 0xbe, 0x5d, 0xb1 };

			// we don't support EffectiveKeySize != KeySize to match MS implementation
		 	CheckECB (64, key, pt, ct);
		}

		[Test]
		public void RFC2268Vector_7 ()
		{
			byte[] key = { 0x88, 0xbc, 0xa9, 0x0e, 0x90, 0x87, 0x5a, 0x7f,  
				       0x0f, 0x79, 0xc3, 0x84, 0x62, 0x7b, 0xaf, 0xb2 };
			byte[] pt = { 0, 0, 0, 0, 0, 0, 0, 0 };
			byte[] ct = { 0x22, 0x69, 0x55, 0x2a, 0xb0, 0xf8, 0x5c, 0xa6 };

			CheckECB (128, key, pt, ct);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void RFC2268Vector_8 ()
		{
		 	byte[] key = { 0x88, 0xbc, 0xa9, 0x0e, 0x90, 0x87, 0x5a, 0x7f, 
		 	               0x0f, 0x79, 0xc3, 0x84, 0x62, 0x7b, 0xaf, 0xb2, 
		 		       0x16, 0xf8, 0x0a, 0x6f, 0x85, 0x92, 0x05, 0x84,
		 		       0xc4, 0x2f, 0xce, 0xb0, 0xbe, 0x25, 0x5d, 0xaf, 0x1e };
		 	byte[] pt = { 0, 0, 0, 0, 0, 0, 0, 0 };
		 	byte[] ct = { 0x5b, 0x78, 0xd3, 0xa4, 0x3d, 0xff, 0xf1, 0xf1 };

			// we don't support KeySize > 128 to match MS implementation
		 	CheckECB (129, key, pt, ct);
		}
	}
}
