//
// TestSuite.System.Security.Cryptography.RijndaelManaged.cs
//
// Author:
//      Andrew Birkett (andy@nobugs.org)
//

using System;
using System.Security.Cryptography;

using NUnit.Framework;

namespace MonoTests.System.Security.Cryptography {

	public class RijndaelManagedTest : TestCase {
		
		public RijndaelManagedTest() : base ("MonoTests.System.Security.Cryptography.RijndaelManagedTst testcase") {
		}

		public RijndaelManagedTest(String name) : base(name) {
		}
		
		public static ITest Suite {
			get {
				return new TestSuite(typeof(RijndaelManagedTest));
			}
		}


		public void CheckCBC(ICryptoTransform encryptor, ICryptoTransform decryptor, 
					   byte[] plaintext, byte[] expected) 
		{
	
			if ((plaintext.Length % encryptor.InputBlockSize) != 0) {
				throw new ArgumentException("Must have complete blocks");
			}
	
			byte[] ciphertext = new byte[plaintext.Length];
			for (int i=0; i < plaintext.Length; i += encryptor.InputBlockSize) {
				encryptor.TransformBlock(plaintext, i, encryptor.InputBlockSize, ciphertext, i);
			}
	
			for (int i=0; i<32; i++) {
				AssertEquals("CBC-" + i, expected[i], ciphertext[i]);
			}
	
			byte[] roundtrip = new byte[plaintext.Length];
			for (int i=0; i < ciphertext.Length; i += decryptor.InputBlockSize) {
				decryptor.TransformBlock(ciphertext, i, decryptor.InputBlockSize, roundtrip, i);
			}
	
			for (int i=0; i<32; i++) {
				AssertEquals("CBC-rt-" + i, roundtrip[i], plaintext[i]);
			}
	
		}
	
		public void TestCBC_0() {
	
			byte[] plaintext = new byte[32];
			for (int i=0; i < plaintext.Length; i++) plaintext[i] = 0;
	
			byte[] iv = new byte[16];
			for (byte i=0; i < iv.Length; i++) {
				iv[i] = 0;
			}
	
			RijndaelManaged r = new RijndaelManaged();
	
			r.Key = new byte[16];
	
			for (int i=0; i < 16; i++) r.Key[i] = 0;
			r.BlockSize = 128;
			r.Mode = CipherMode.CBC;
			r.Padding = PaddingMode.Zeros;
	
			byte[] expected = { 
				0x66, 0xe9, 0x4b, 0xd4, 0xef, 0x8a, 0x2c, 0x3b, 
				0x88, 0x4c, 0xfa, 0x59, 0xca, 0x34, 0x2b, 0x2e, 
				0xf7, 0x95, 0xbd, 0x4a, 0x52, 0xe2, 0x9e, 0xd7, 
				0x13, 0xd3, 0x13, 0xfa, 0x20, 0xe9, 0x8d, 0xbc };
	
			CheckCBC(r.CreateEncryptor(r.Key, iv), r.CreateDecryptor(r.Key, iv), plaintext, expected);
		}
	
		public void TestCBC_1() {
	
			byte[] plaintext = new byte[32];
			for (int i=0; i < plaintext.Length; i++) plaintext[i] = 0;
	
			byte[] iv = new byte[16];
			for (byte i=0; i < iv.Length; i++) {
				iv[i] = i;
			}
	
			RijndaelManaged r = new RijndaelManaged();
			r.Key = new byte[16];
			for (byte i=0; i < 16; i++) r.Key[i] = i;
			r.BlockSize = 128;
			r.Mode = CipherMode.CBC;
			r.Padding = PaddingMode.Zeros;
	
			byte[] expected = { 
				0xa, 0x94, 0xb, 0xb5, 0x41, 0x6e, 0xf0, 0x45, 
				0xf1, 0xc3, 0x94, 0x58, 0xc6, 0x53, 0xea, 0x5a, 
				0xae, 0xe7, 0x1e, 0xa5, 0x41, 0xd7, 0xae, 0x4b, 
				0xeb, 0x60, 0xbe, 0xcc, 0x59, 0x3f, 0xb6, 0x63 };
	
			CheckCBC(r.CreateEncryptor(r.Key, iv), r.CreateDecryptor(r.Key, iv), plaintext, expected);
		}
	
		public void CheckECBRoundtrip(ICryptoTransform encryptor, ICryptoTransform decryptor, 
					   byte[] plaintext, byte[] expected)
		{
			byte[] ciphertext = new byte[plaintext.Length];
			int n = encryptor.TransformBlock(plaintext, 0, plaintext.Length, ciphertext, 0);
	
			AssertEquals("ECB-len", n, expected.Length);
			for (int i=0; i < ciphertext.Length; i++) {
				AssertEquals("ECB-encrypt-" + i, ciphertext[i], expected[i]);
			}
	
			byte[] roundtrip = new byte[plaintext.Length];
			n = decryptor.TransformBlock(ciphertext, 0, ciphertext.Length, roundtrip, 0);
	
			AssertEquals("ECB-rt-len", n, plaintext.Length);
			for (int i=0; i < roundtrip.Length; i++) {
				AssertEquals("ECB-rt-" + i, roundtrip[i], plaintext[i]);
			}
		}
	
		public void TestECB() {
	
			byte[] plaintext = new byte[16];
			byte[] iv = new byte[16];
	
			for (int i=0; i < 16; i++) {
				plaintext[i] = (byte) (i*16 + i);
			}
	
			RijndaelManaged r = new RijndaelManaged();
			r.Mode = CipherMode.ECB;
			r.Padding = PaddingMode.Zeros;
	
			byte[] key16 = new byte[16];
			byte[] key24 = new byte[24];
			byte[] key32 = new byte[32];
	
			for (int i=0; i < 32; i++) {
				if (i < 16) key16[i] = (byte) i;
				if (i < 24) key24[i] = (byte) i;
				key32[i] = (byte) i;
			}
	
				
			byte[] exp16 = { 0x69, 0xc4, 0xe0, 0xd8, 0x6a, 0x7b, 0x04, 0x30,
					 0xd8, 0xcd, 0xb7, 0x80, 0x70, 0xb4, 0xc5, 0x5a };
			byte[] exp24 = { 0xdd, 0xa9, 0x7c, 0xa4, 0x86, 0x4c, 0xdf, 0xe0,
					 0x6e, 0xaf, 0x70, 0xa0, 0xec, 0x0d, 0x71, 0x91 };
			byte[] exp32 = { 0x8e, 0xa2, 0xb7, 0xca, 0x51, 0x67, 0x45, 0xbf,
					 0xea, 0xfc, 0x49, 0x90, 0x4b, 0x49, 0x60, 0x89 }; 
	
			r.Key = key16;
			r.KeySize = 128;	
			CheckECBRoundtrip(
				r.CreateEncryptor(key16, iv), r.CreateDecryptor(key16, iv), 
				plaintext, exp16
			);
	
	
			r.Key = key24;
			r.KeySize = 192;
			CheckECBRoundtrip(
				r.CreateEncryptor(key24, iv), r.CreateDecryptor(key24, iv), 
				plaintext, exp24
			);
	
	
			r.Key = key32;
			r.KeySize = 256;
			CheckECBRoundtrip(
				r.CreateEncryptor(key32, iv), r.CreateDecryptor(key32, iv), 
				plaintext, exp32
			);
		}
	}
}
