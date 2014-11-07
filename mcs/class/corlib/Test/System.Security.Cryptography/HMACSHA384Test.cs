//
// HMACSHA384Test.cs - NUnit Test Cases for HMACSHA384
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2006, 2007 Novell, Inc (http://www.novell.com)
//

#if NET_2_0

using NUnit.Framework;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MonoTests.System.Security.Cryptography {

	public class HS384 : HMACSHA384 {

		public int BlockSize {
			get { return base.BlockSizeValue; }
			set { base.BlockSizeValue = value; }
		}
	}

	public class SelectableHmacSha384: HMAC {

		// legacy parameter:
		//      http://blogs.msdn.com/shawnfa/archive/2007/01/31/please-do-not-use-the-net-2-0-hmacsha512-and-hmacsha384-classes.aspx
		
		public SelectableHmacSha384 (byte[] key, bool legacy)
		{
			HashName = "SHA384";
			HashSizeValue = 384;
			BlockSizeValue = legacy ? 64 : 128;
			Key = key;
		}
	}

	// References:
	// a.	Identifiers and Test Vectors for HMAC-SHA-224, HMAC-SHA-256, HMAC-SHA-384, and HMAC-SHA-512
	//	http://www.ietf.org/rfc/rfc4231.txt

	[TestFixture]
	public class HMACSHA384Test : KeyedHashAlgorithmTest {

		protected HMACSHA384 algo;

		[SetUp]
		public override void SetUp () 
		{
			algo = new HMACSHA384 ();
			algo.Key = new byte [8];
			hash = algo;
		}

		// the hash algorithm only exists as a managed implementation
		public override bool ManagedHashImplementation {
			get { return true; }
		}

		[Test]
		public void Constructors () 
		{
			algo = new HMACSHA384 ();
			Assert.IsNotNull (algo, "HMACSHA384 ()");

			byte[] key = new byte [8];
			algo = new HMACSHA384 (key);
			Assert.IsNotNull (algo, "HMACSHA384 (key)");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Constructor_Null () 
		{
			new HMACSHA384 (null);
		}

		[Test]
		public void Invariants () 
		{
			algo = new HMACSHA384 ();
			Assert.IsTrue (algo.CanReuseTransform, "HMACSHA384.CanReuseTransform");
			Assert.IsTrue (algo.CanTransformMultipleBlocks, "HMACSHA384.CanTransformMultipleBlocks");
			Assert.AreEqual ("SHA384", algo.HashName, "HMACSHA384.HashName");
			Assert.AreEqual (384, algo.HashSize, "HMACSHA384.HashSize");
			Assert.AreEqual (1, algo.InputBlockSize, "HMACSHA384.InputBlockSize");
			Assert.AreEqual (1, algo.OutputBlockSize, "HMACSHA384.OutputBlockSize");
			Assert.AreEqual ("System.Security.Cryptography.HMACSHA384", algo.ToString (), "HMACSHA384.ToString()"); 
		}

		// some test case truncate the result
		private void Compare (byte[] expected, byte[] actual, string msg)
		{
			if (expected.Length == actual.Length) {
				Assert.AreEqual (expected, actual, msg);
			} else {
				byte[] data = new byte[expected.Length];
				Array.Copy (actual, data, data.Length);
				Assert.AreEqual (expected, data, msg);
			}
		}

		public void Check (string testName, HMAC algo, byte[] data, byte[] result)
		{
			CheckA (testName, algo, data, result);
			CheckB (testName, algo, data, result);
			CheckC (testName, algo, data, result);
			CheckD (testName, algo, data, result);
			CheckE (testName, algo, data, result);
		}

		public void CheckA (string testName, HMAC algo, byte[] data, byte[] result)
		{
			byte[] hmac = algo.ComputeHash (data);
			Compare (result, hmac, testName + "a1");
			Compare (result, algo.Hash, testName + "a2");
		}

		public void CheckB (string testName, HMAC algo, byte[] data, byte[] result)
		{
			byte[] hmac = algo.ComputeHash (data, 0, data.Length);
			Compare (result, hmac, testName + "b1");
			Compare (result, algo.Hash, testName + "b2");
		}

		public void CheckC (string testName, HMAC algo, byte[] data, byte[] result)
		{
			using (MemoryStream ms = new MemoryStream (data)) {
				byte[] hmac = algo.ComputeHash (ms);
				Compare (result, hmac, testName + "c1");
				Compare (result, algo.Hash, testName + "c2");
			}
		}

		public void CheckD (string testName, HMAC algo, byte[] data, byte[] result)
		{
			algo.TransformFinalBlock (data, 0, data.Length);
			Compare (result, algo.Hash, testName + "d");
			algo.Initialize ();
		}

		public void CheckE (string testName, HMAC algo, byte[] data, byte[] result)
		{
			byte[] copy = new byte[data.Length];
			for (int i = 0; i < data.Length - 1; i++)
				algo.TransformBlock (data, i, 1, copy, i);
			algo.TransformFinalBlock (data, data.Length - 1, 1);
			Compare (result, algo.Hash, testName + "e");
			algo.Initialize ();
		}

		[Test]
		public void RFC4231_TC1_Normal ()
		{
			byte[] key = { 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b };
			byte[] data = Encoding.Default.GetBytes ("Hi There");
			byte[] digest = { 0xaf, 0xd0, 0x39, 0x44, 0xd8, 0x48, 0x95, 0x62, 0x6b, 0x08, 0x25, 0xf4, 0xab, 0x46, 0x90, 0x7f,
				0x15, 0xf9, 0xda, 0xdb, 0xe4, 0x10, 0x1e, 0xc6, 0x82, 0xaa, 0x03, 0x4c, 0x7c, 0xeb, 0xc5, 0x9c,
				0xfa, 0xea, 0x9e, 0xa9, 0x07, 0x6e, 0xde, 0x7f, 0x4a, 0xf1, 0x52, 0xe8, 0xb2, 0xfa, 0x9c, 0xb6 };
			HMAC hmac = new SelectableHmacSha384 (key, false);
			Check ("HMACSHA384-N-RFC4231-TC1", hmac, data, digest);
		}

		[Test]
		// Test with a key shorter than the length of the HMAC output.
		public void RFC4231_TC2_Normal ()
		{
			byte[] key = Encoding.Default.GetBytes ("Jefe");
			byte[] data = Encoding.Default.GetBytes ("what do ya want for nothing?");
			byte[] digest = { 0xaf, 0x45, 0xd2, 0xe3, 0x76, 0x48, 0x40, 0x31, 0x61, 0x7f, 0x78, 0xd2, 0xb5, 0x8a, 0x6b, 0x1b,
				0x9c, 0x7e, 0xf4, 0x64, 0xf5, 0xa0, 0x1b, 0x47, 0xe4, 0x2e, 0xc3, 0x73, 0x63, 0x22, 0x44, 0x5e,
				0x8e, 0x22, 0x40, 0xca, 0x5e, 0x69, 0xe2, 0xc7, 0x8b, 0x32, 0x39, 0xec, 0xfa, 0xb2, 0x16, 0x49 };
			HMAC hmac = new SelectableHmacSha384 (key, false);
			Check ("HMACSHA384-N-RFC4231-TC2", hmac, data, digest);
		}

		[Test]
		// Test with a combined length of key and data that is larger than 64 bytes (= block-size of SHA-224 and SHA-256).
		public void RFC4231_TC3_Normal ()
		{
			byte[] key = { 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa };
			byte[] data = { 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd,
				0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd,
				0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd,
				0xdd, 0xdd };
			byte[] digest = { 0x88, 0x06, 0x26, 0x08, 0xd3, 0xe6, 0xad, 0x8a, 0x0a, 0xa2, 0xac, 0xe0, 0x14, 0xc8, 0xa8, 0x6f,
				0x0a, 0xa6, 0x35, 0xd9, 0x47, 0xac, 0x9f, 0xeb, 0xe8, 0x3e, 0xf4, 0xe5, 0x59, 0x66, 0x14, 0x4b,
				0x2a, 0x5a, 0xb3, 0x9d, 0xc1, 0x38, 0x14, 0xb9, 0x4e, 0x3a, 0xb6, 0xe1, 0x01, 0xa3, 0x4f, 0x27 };
			HMAC hmac = new SelectableHmacSha384 (key, false);
			Check ("HMACSHA384-N-RFC4231-TC3", hmac, data, digest);
		}

		[Test]
		// Test with a combined length of key and data that is larger than 64 bytes (= block-size of SHA-224 and SHA-256).
		public void RFC4231_TC4_Normal ()
		{
			byte[] key = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10,
				0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19 };
			byte[] data = { 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd,
				0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd,
				0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd,
				0xcd, 0xcd };
			byte[] digest = { 0x3e, 0x8a, 0x69, 0xb7, 0x78, 0x3c, 0x25, 0x85, 0x19, 0x33, 0xab, 0x62, 0x90, 0xaf, 0x6c, 0xa7,
				0x7a, 0x99, 0x81, 0x48, 0x08, 0x50, 0x00, 0x9c, 0xc5, 0x57, 0x7c, 0x6e, 0x1f, 0x57, 0x3b, 0x4e,
				0x68, 0x01, 0xdd, 0x23, 0xc4, 0xa7, 0xd6, 0x79, 0xcc, 0xf8, 0xa3, 0x86, 0xc6, 0x74, 0xcf, 0xfb };
			HMAC hmac = new SelectableHmacSha384 (key, false);
			Check ("HMACSHA384-N-RFC4231-TC4", hmac, data, digest);
		}

		[Test]
		// Test with a truncation of output to 128 bits.
		public void RFC4231_TC5_Normal ()
		{
			byte[] key = { 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c,
				0x0c, 0x0c, 0x0c, 0x0c };
			byte[] data = Encoding.Default.GetBytes ("Test With Truncation");
			byte[] digest = { 0x3a, 0xbf, 0x34, 0xc3, 0x50, 0x3b, 0x2a, 0x23, 0xa4, 0x6e, 0xfc, 0x61, 0x9b, 0xae, 0xf8, 0x97 };
			HMAC hmac = new SelectableHmacSha384 (key, false);
			Check ("HMACSHA384-N-RFC4231-TC5", hmac, data, digest);
		}

		[Test]
		// Test with a key larger than 128 bytes (= block-size of SHA-384 and SHA-512).
		public void RFC4231_TC6_Normal ()
		{
			byte[] key = { 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa };
			byte[] data = Encoding.Default.GetBytes ("Test Using Larger Than Block-Size Key - Hash Key First");
			byte[] digest = { 0x4e, 0xce, 0x08, 0x44, 0x85, 0x81, 0x3e, 0x90, 0x88, 0xd2, 0xc6, 0x3a, 0x04, 0x1b, 0xc5, 0xb4,
				0x4f, 0x9e, 0xf1, 0x01, 0x2a, 0x2b, 0x58, 0x8f, 0x3c, 0xd1, 0x1f, 0x05, 0x03, 0x3a, 0xc4, 0xc6,
				0x0c, 0x2e, 0xf6, 0xab, 0x40, 0x30, 0xfe, 0x82, 0x96, 0x24, 0x8d, 0xf1, 0x63, 0xf4, 0x49, 0x52 };
			HMAC hmac = new SelectableHmacSha384 (key, false);
			Check ("HMACSHA384-N-RFC4231-TC6", hmac, data, digest);
		}

		[Test]
		// Test with a key and data that is larger than 128 bytes (= block-size of SHA-384 and SHA-512).
		public void RFC4231_TC7_Normal ()
		{
			byte[] key = { 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa };
			byte[] data = Encoding.Default.GetBytes ("This is a test using a larger than block-size key and a larger than block-size data. The key needs to be hashed before being used by the HMAC algorithm.");
			byte[] digest = { 0x66, 0x17, 0x17, 0x8e, 0x94, 0x1f, 0x02, 0x0d, 0x35, 0x1e, 0x2f, 0x25, 0x4e, 0x8f, 0xd3, 0x2c,
				0x60, 0x24, 0x20, 0xfe, 0xb0, 0xb8, 0xfb, 0x9a, 0xdc, 0xce, 0xbb, 0x82, 0x46, 0x1e, 0x99, 0xc5,
				0xa6, 0x78, 0xcc, 0x31, 0xe7, 0x99, 0x17, 0x6d, 0x38, 0x60, 0xe6, 0x11, 0x0c, 0x46, 0x52, 0x3e };
			HMAC hmac = new SelectableHmacSha384 (key, false);
			Check ("HMACSHA384-N-RFC4231-TC7", hmac, data, digest);
		}

		[Test]
		public void RFC4231_TC1_Legacy ()
		{
			byte[] key = { 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b };
			byte[] data = Encoding.Default.GetBytes ("Hi There");
			byte[] digest = { 0x0A, 0x04, 0x6A, 0xAA, 0x02, 0x55, 0xE4, 0x32, 0x91, 0x22, 0x28, 0xF8, 0xCC, 0xDA, 0x43, 0x7C,
				0x8A, 0x83, 0x63, 0xFB, 0x16, 0x0A, 0xFB, 0x05, 0x70, 0xAB, 0x5B, 0x1F, 0xD5, 0xDD, 0xC2, 0x0E, 
				0xB1, 0x88, 0x8B, 0x9E, 0xD4, 0xE5, 0xB6, 0xCB, 0x5B, 0xC0, 0x34, 0xCD, 0x9E, 0xF7, 0x0E, 0x40 };
			HMAC hmac = new SelectableHmacSha384 (key, true);
			Check ("HMACSHA384-L-RFC4231-TC1", hmac, data, digest);
		}

		[Test]
		// Test with a key shorter than the length of the HMAC output.
		public void RFC4231_TC2_Legacy ()
		{
			byte[] key = Encoding.Default.GetBytes ("Jefe");
			byte[] data = Encoding.Default.GetBytes ("what do ya want for nothing?");
			byte[] digest = { 0xBB, 0x8A, 0xF7, 0xF5, 0x8A, 0xC9, 0xE8, 0x3A, 0x87, 0x2E, 0x51, 0x2F, 0x75, 0xD8, 0x74, 0xCC, 
				0x45, 0xE3, 0xDD, 0x1C, 0xD4, 0x76, 0x54, 0x66, 0xCC, 0xEA, 0x19, 0x5B, 0xC3, 0x00, 0x2C, 0xCE, 
				0x3C, 0x9F, 0x1B, 0xAF, 0x44, 0xF2, 0xD7, 0x28, 0x13, 0xA2, 0x4D, 0x11, 0x52, 0xE3, 0x66, 0x6F };
			HMAC hmac = new SelectableHmacSha384 (key, true);
			Check ("HMACSHA384-L-RFC4231-TC2", hmac, data, digest);
		}

		[Test]
		// Test with a combined length of key and data that is larger than 64 bytes (= block-size of SHA-224 and SHA-256).
		public void RFC4231_TC3_Legacy ()
		{
			byte[] key = { 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa };
			byte[] data = { 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd,
				0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd,
				0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd,
				0xdd, 0xdd };
			byte[] digest = { 0x43, 0xAA, 0x39, 0x69, 0x01, 0xAB, 0xF6, 0xFA, 0x3A, 0x5E, 0x85, 0x07, 0x4E, 0xA6, 0x5D, 0x61,
				0x91, 0xED, 0x86, 0x20, 0x6E, 0xE0, 0x40, 0x23, 0x97, 0x48, 0xE1, 0x35, 0xD5, 0xC6, 0xC1, 0x90, 
				0x9F, 0x1A, 0x39, 0x68, 0xCD, 0x44, 0xA0, 0xB0, 0x48, 0xFE, 0x7C, 0x68, 0x65, 0x44, 0x18, 0x98 };
			HMAC hmac = new SelectableHmacSha384 (key, true);
			Check ("HMACSHA384-L-RFC4231-TC3", hmac, data, digest);
		}

		[Test]
		// Test with a combined length of key and data that is larger than 64 bytes (= block-size of SHA-224 and SHA-256).
		public void RFC4231_TC4_Legacy ()
		{
			byte[] key = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10,
				0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19 };
			byte[] data = { 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd,
				0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd,
				0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd,
				0xcd, 0xcd };
			byte[] digest = { 0x1F, 0x20, 0x77, 0xC5, 0xBA, 0x39, 0x38, 0x23, 0x86, 0x47, 0xD0, 0x72, 0x14, 0x32, 0xC5, 0x6A,
				0x34, 0xDA, 0x36, 0xDD, 0x77, 0x9A, 0xCA, 0xC4, 0xB2, 0xA2, 0xF9, 0xE3, 0x1A, 0xE5, 0xEE, 0x00, 
				0x83, 0xAA, 0x3E, 0x18, 0xDF, 0xD8, 0xBB, 0x6E, 0xEB, 0x0A, 0xF7, 0x37, 0x84, 0x21, 0x8F, 0xB3 };
			HMAC hmac = new SelectableHmacSha384 (key, true);
			Check ("HMACSHA384-L-RFC4231-TC4", hmac, data, digest);
		}

		[Test]
		// Test with a truncation of output to 128 bits.
		public void RFC4231_TC5_Legacy ()
		{
			byte[] key = { 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c,
				0x0c, 0x0c, 0x0c, 0x0c };
			byte[] data = Encoding.Default.GetBytes ("Test With Truncation");
			byte[] digest = { 0x2E, 0xF2, 0xA4, 0x13, 0xAB, 0x77, 0xBE, 0xCC, 0xF0, 0xB7, 0xD6, 0xAA, 0x6F, 0x60, 0x6B, 0x59 };
			HMAC hmac = new SelectableHmacSha384 (key, true);
			Check ("HMACSHA384-L-RFC4231-TC5", hmac, data, digest);
		}

		[Test]
		// Test with a key larger than 128 bytes (= block-size of SHA-384 and SHA-512).
		public void RFC4231_TC6_Legacy ()
		{
			byte[] key = { 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa };
			byte[] data = Encoding.Default.GetBytes ("Test Using Larger Than Block-Size Key - Hash Key First");
			byte[] digest = { 0xCA, 0xAA, 0x39, 0xF4, 0x53, 0xED, 0x62, 0x43, 0x06, 0x8C, 0x00, 0x95, 0xD8, 0xD3, 0xAC, 0x78,
				0x80, 0xE8, 0xA4, 0x38, 0x82, 0x90, 0xAC, 0xFA, 0xBA, 0xE9, 0xA1, 0xAC, 0x1C, 0x3F, 0xB3, 0x74, 
				0x34, 0x1C, 0xD1, 0xD2, 0xBE, 0x6C, 0x99, 0x75, 0x2B, 0xDC, 0x98, 0x78, 0x45, 0xB7, 0x08, 0x5F };
			HMAC hmac = new SelectableHmacSha384 (key, true);
			Check ("HMACSHA384-L-RFC4231-TC6", hmac, data, digest);
		}

		[Test]
		// Test with a key and data that is larger than 128 bytes (= block-size of SHA-384 and SHA-512).
		public void RFC4231_TC7_Legacy ()
		{
			byte[] key = { 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa };
			byte[] data = Encoding.Default.GetBytes ("This is a test using a larger than block-size key and a larger than block-size data. The key needs to be hashed before being used by the HMAC algorithm.");
			byte[] digest = { 0x5C, 0xF0, 0x92, 0x18, 0x76, 0xCB, 0x51, 0x7E, 0x0E, 0x29, 0xE5, 0x01, 0xCB, 0x70, 0x67, 0x6B,
				0x63, 0xAE, 0x98, 0x61, 0xB9, 0x66, 0xC2, 0x37, 0xEE, 0x1C, 0x7B, 0x30, 0x84, 0xB1, 0xAA, 0x76, 
				0xEE, 0x81, 0x7A, 0xEF, 0xB4, 0xF4, 0x65, 0x00, 0xCA, 0x67, 0x92, 0xF8, 0x7E, 0xAA, 0x83, 0xBD };
			HMAC hmac = new SelectableHmacSha384 (key, true);
			Check ("HMACSHA384-L-RFC4231-TC7", hmac, data, digest);
		}
	}
}

#endif
