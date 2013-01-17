//
// HMACMD5Test.cs - NUnit Test Cases for HMACMD5
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

	public class HM5 : HMACMD5 {

		public int BlockSize {
			get { return base.BlockSizeValue; }
			set { base.BlockSizeValue = value; }
		}
	}

	// References:
	// a.	IETF RFC2202: Test Cases for HMAC-MD5 and HMAC-SHA-1
	//	http://www.ietf.org/rfc/rfc2202.txt

	[TestFixture]
	public class HMACMD5Test : KeyedHashAlgorithmTest {

		protected HMACMD5 algo;

		[SetUp]
		public override void SetUp () 
		{
			algo = new HMACMD5 ();
			algo.Key = new byte [8];
			hash = algo;
		}

		[Test]
		public void Constructors () 
		{
			algo = new HMACMD5 ();
			Assert.IsNotNull (algo, "HMACMD5 ()");

			byte[] key = new byte [8];
			algo = new HMACMD5 (key);
			Assert.IsNotNull (algo, "HMACMD5 (key)");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Constructor_Null () 
		{
			new HMACMD5 (null);
		}

		[Test]
		public void Invariants () 
		{
			algo = new HMACMD5 ();
			Assert.IsTrue (algo.CanReuseTransform, "HMACMD5.CanReuseTransform");
			Assert.IsTrue (algo.CanTransformMultipleBlocks, "HMACMD5.CanTransformMultipleBlocks");
			Assert.AreEqual ("MD5", algo.HashName, "HMACMD5.HashName");
			Assert.AreEqual (128, algo.HashSize, "HMACMD5.HashSize");
			Assert.AreEqual (1, algo.InputBlockSize, "HMACMD5.InputBlockSize");
			Assert.AreEqual (1, algo.OutputBlockSize, "HMACMD5.OutputBlockSize");
			Assert.AreEqual ("System.Security.Cryptography.HMACMD5", algo.ToString (), "HMACMD5.ToString()"); 
		}

		[Test]
		public void BlockSize ()
		{
			HM5 hmac = new HM5 ();
			Assert.AreEqual (64, hmac.BlockSize, "BlockSizeValue");
		}

		public void Check (string testName, byte[] key, byte[] data, byte[] result) 
		{
			string classTestName = "HMACMD5-" + testName;
			CheckA (testName, key, data, result);
			CheckB (testName, key, data, result);
			CheckC (testName, key, data, result);
			CheckD (testName, key, data, result);
			CheckE (testName, key, data, result);
		}

		public void CheckA (string testName, byte[] key, byte[] data, byte[] result) 
		{
			algo = new HMACMD5 ();
			algo.Key = key;
			byte[] hmac = algo.ComputeHash (data);
			Assert.AreEqual (result, hmac, testName + "a1");
			Assert.AreEqual (result, algo.Hash, testName + "a2");
		}

		public void CheckB (string testName, byte[] key, byte[] data, byte[] result) 
		{
			algo = new HMACMD5 ();
			algo.Key = key;
			byte[] hmac = algo.ComputeHash (data, 0, data.Length);
			Assert.AreEqual (result, hmac, testName + "b1");
			Assert.AreEqual (result, algo.Hash, testName + "b2");
		}
	
		public void CheckC (string testName, byte[] key, byte[] data, byte[] result) 
		{
			algo = new HMACMD5 ();
			algo.Key = key;
			MemoryStream ms = new MemoryStream (data);
			byte[] hmac = algo.ComputeHash (ms);
			Assert.AreEqual (result, hmac, testName + "c1");
			Assert.AreEqual (result, algo.Hash, testName + "c2");
		}

		public void CheckD (string testName, byte[] key, byte[] data, byte[] result) 
		{
			algo = new HMACMD5 ();
			algo.Key = key;
			// LAMESPEC or FIXME: TransformFinalBlock doesn't return HashValue !
			algo.TransformFinalBlock (data, 0, data.Length);
			Assert.AreEqual (result, algo.Hash, testName + "d");
		}

		public void CheckE (string testName, byte[] key, byte[] data, byte[] result) 
		{
			algo = new HMACMD5 ();
			algo.Key = key;
			byte[] copy = new byte [data.Length];
			// LAMESPEC or FIXME: TransformFinalBlock doesn't return HashValue !
			for (int i=0; i < data.Length - 1; i++)
				algo.TransformBlock (data, i, 1, copy, i);
			algo.TransformFinalBlock (data, data.Length - 1, 1);
			Assert.AreEqual (result, algo.Hash, testName + "e");
		}

		[Test]
		public void RFC2202_TC1 () 
		{
			byte[] key =  { 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b };
			byte[] data = Encoding.Default.GetBytes ("Hi There");
			byte[] digest = { 0x92, 0x94, 0x72, 0x7a, 0x36, 0x38, 0xbb, 0x1c, 0x13, 0xf4, 0x8e, 0xf8, 0x15, 0x8b, 0xfc, 0x9d };
			Check ("RFC2202-TC1", key, data, digest);
		}

		[Test]
		public void RFC2202_TC2 () 
		{
			byte[] key = Encoding.Default.GetBytes ("Jefe");
			byte[] data = Encoding.Default.GetBytes ("what do ya want for nothing?");
			byte[] digest = { 0x75, 0x0c, 0x78, 0x3e, 0x6a, 0xb0, 0xb5, 0x03, 0xea, 0xa8, 0x6e, 0x31, 0x0a, 0x5d, 0xb7, 0x38 };
			Check ("RFC2202-TC2", key, data, digest);
		}

		[Test]
		public void RFC2202_TC3 () 
		{
			byte[] key = { 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa };
			byte[] data = new byte [50];
			for (int i = 0; i < data.Length; i++)
				data[i] = 0xdd;
			byte[] digest = { 0x56, 0xbe, 0x34, 0x52, 0x1d, 0x14, 0x4c, 0x88, 0xdb, 0xb8, 0xc7, 0x33, 0xf0, 0xe8, 0xb3, 0xf6 };
			Check ("RFC2202-TC3", key, data, digest);
		}

		[Test]
		public void RFC2202_TC4 () 
		{
			byte[] key = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19 };
			byte[] data = new byte [50];
			for (int i = 0; i < data.Length; i++)
				data[i] = 0xcd;
			byte[] digest = { 0x69, 0x7e, 0xaf, 0x0a, 0xca, 0x3a, 0x3a, 0xea, 0x3a, 0x75, 0x16, 0x47, 0x46, 0xff, 0xaa, 0x79 };
			Check ("RFC2202-TC4", key, data, digest);
		}

		[Test]
		public void RFC2202_TC5 () 
		{
			byte[] key = { 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c };
			byte[] data = Encoding.Default.GetBytes ("Test With Truncation");
			byte[] digest = { 0x56, 0x46, 0x1e, 0xf2, 0x34, 0x2e, 0xdc, 0x00, 0xf9, 0xba, 0xb9, 0x95, 0x69, 0x0e, 0xfd, 0x4c };
			Check ("RFC2202-TC5", key, data, digest);
		}

		[Test]
		public void RFC2202_TC6 () 
		{
			byte[] key = new byte [80];
			for (int i = 0; i < key.Length; i++)
				key[i] = 0xaa;
			byte[] data = Encoding.Default.GetBytes ("Test Using Larger Than Block-Size Key - Hash Key First");
			byte[] digest = { 0x6b, 0x1a, 0xb7, 0xfe, 0x4b, 0xd7, 0xbf, 0x8f, 0x0b, 0x62, 0xe6, 0xce, 0x61, 0xb9, 0xd0, 0xcd };
			Check ("RFC2202-TC6", key, data, digest);
		}

		[Test]
		public void RFC2202_TC7 () 
		{
			byte[] key = new byte [80];
			for (int i = 0; i < key.Length; i++)
				key[i] = 0xaa;
			byte[] data = Encoding.Default.GetBytes ("Test Using Larger Than Block-Size Key and Larger Than One Block-Size Data");
			byte[] digest = { 0x6f, 0x63, 0x0f, 0xad, 0x67, 0xcd, 0xa0, 0xee, 0x1f, 0xb1, 0xf5, 0x62, 0xdb, 0x3a, 0xa5, 0x3e };
			Check ("RFC2202-TC7", key, data, digest);
		}
	}

}

#endif
