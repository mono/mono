//
// HMACRIPEMD160Test.cs - NUnit Test Cases for HMACRIPEMD160
//	http://www.esat.kuleuven.ac.be/~bosselae/ripemd160.html
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004, 2006, 2007 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using NUnit.Framework;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MonoTests.System.Security.Cryptography {

	public class HR160 : HMACRIPEMD160 {

		public int BlockSize {
			get { return base.BlockSizeValue; }
			set { base.BlockSizeValue = value; }
		}
	}

	[TestFixture]
	public class HMACRIPEMD160Test : KeyedHashAlgorithmTest {

		protected HMACRIPEMD160 hmac;

		[SetUp]
		public override void SetUp () 
		{
			hmac = new HMACRIPEMD160 ();
			hmac.Key = new byte [8];
			hash = hmac;
		}

		// the hash algorithm only exists as a managed implementation
		public override bool ManagedHashImplementation {
			get { return true; }
		}

		static byte[] key1 = { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff, 0x01, 0x23, 0x45, 0x67 };

		// HMACRIPEMD160 (key1, "") = cf387677bfda8483e63b57e06c3b5ecd8b7fc055
		[Test]
		public void HMACRIPEMD160_Key1_Test1 () 
		{
			byte[] result = { 0xcf, 0x38, 0x76, 0x77, 0xbf, 0xda, 0x84, 0x83, 0xe6, 0x3b, 0x57, 0xe0, 0x6c, 0x3b, 0x5e, 0xcd, 0x8b, 0x7f, 0xc0, 0x55 };
			byte[] input = new byte [0];
		
			string testName = "HMACRIPEMD160 Key #1 Test #1";
			hmac.Key = key1;
			HMACRIPEMD160_a (testName, hmac, input, result);
			HMACRIPEMD160_b (testName, hmac, input, result);
			HMACRIPEMD160_c (testName, hmac, input, result);
			HMACRIPEMD160_d (testName, hmac, input, result);
			// N/A RIPEMD160_e (testName, hmac, input, result);
		}

		// HMACRIPEMD160 ("a") = 0d351d71b78e36dbb7391c810a0d2b6240ddbafc
		[Test]
		public void HMACRIPEMD160_Key1_Test2 () 
		{
			byte[] result = { 0x0d, 0x35, 0x1d, 0x71, 0xb7, 0x8e, 0x36, 0xdb, 0xb7, 0x39, 0x1c, 0x81, 0x0a, 0x0d, 0x2b, 0x62, 0x40, 0xdd, 0xba, 0xfc };
			byte[] input = Encoding.Default.GetBytes ("a");
		
			string testName = "HMACRIPEMD160 Key #1 Test #2";
			hmac.Key = key1;
			HMACRIPEMD160_a (testName, hmac, input, result);
			HMACRIPEMD160_b (testName, hmac, input, result);
			HMACRIPEMD160_c (testName, hmac, input, result);
			HMACRIPEMD160_d (testName, hmac, input, result);
			HMACRIPEMD160_e (testName, hmac, input, result);
		}

		// HMACRIPEMD160 ("abc") = f7ef288cb1bbcc6160d76507e0a3bbf712fb67d6
		[Test]
		public void HMACRIPEMD160_Key1_Test3 () 
		{
			byte[] result = { 0xf7, 0xef, 0x28, 0x8c, 0xb1, 0xbb, 0xcc, 0x61, 0x60, 0xd7, 0x65, 0x07, 0xe0, 0xa3, 0xbb, 0xf7, 0x12, 0xfb, 0x67, 0xd6 };
			byte[] input = Encoding.Default.GetBytes ("abc");
		
			string testName = "HMACRIPEMD160 Key #1 Test #3";
			hmac.Key = key1;
			HMACRIPEMD160_a (testName, hmac, input, result);
			HMACRIPEMD160_b (testName, hmac, input, result);
			HMACRIPEMD160_c (testName, hmac, input, result);
			HMACRIPEMD160_d (testName, hmac, input, result);
			HMACRIPEMD160_e (testName, hmac, input, result);
		}

		// RIPEMD160 ("message digest") = f83662cc8d339c227e600fcd636c57d2571b1c34
		[Test]
		public void HMACRIPEMD160_Key1_Test4 () 
		{
			byte[] result = { 0xf8, 0x36, 0x62, 0xcc, 0x8d, 0x33, 0x9c, 0x22, 0x7e, 0x60, 0x0f, 0xcd, 0x63, 0x6c, 0x57, 0xd2, 0x57, 0x1b, 0x1c, 0x34 };
			byte[] input = Encoding.Default.GetBytes ("message digest");
		
			string testName = "HMACRIPEMD160 Key #1 Test #4";
			hmac.Key = key1;
			HMACRIPEMD160_a (testName, hmac, input, result);
			HMACRIPEMD160_b (testName, hmac, input, result);
			HMACRIPEMD160_c (testName, hmac, input, result);
			HMACRIPEMD160_d (testName, hmac, input, result);
			HMACRIPEMD160_e (testName, hmac, input, result);
		}

		// RIPEMD160 ("abcdefghijklmnopqrstuvwxyz") = 843d1c4eb880ac8ac0c9c95696507957d0155ddb
		[Test]
		public void HMACRIPEMD160_Key1_Test5 () 
		{
			byte[] result = { 0x84, 0x3d, 0x1c, 0x4e, 0xb8, 0x80, 0xac, 0x8a, 0xc0, 0xc9, 0xc9, 0x56, 0x96, 0x50, 0x79, 0x57, 0xd0, 0x15, 0x5d, 0xdb };
			byte[] input = Encoding.Default.GetBytes ("abcdefghijklmnopqrstuvwxyz");
		
			string testName = "HMACRIPEMD160 Key #1 Test #5";
			hmac.Key = key1;
			HMACRIPEMD160_a (testName, hmac, input, result);
			HMACRIPEMD160_b (testName, hmac, input, result);
			HMACRIPEMD160_c (testName, hmac, input, result);
			HMACRIPEMD160_d (testName, hmac, input, result);
			HMACRIPEMD160_e (testName, hmac, input, result);
		}

		// RIPEMD160 ("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq") =
		//	60f5ef198a2dd5745545c1f0c47aa3fb5776f881
		[Test]
		public void HMACRIPEMD160_Key1_Test6 () 
		{
			byte[] result = { 0x60, 0xf5, 0xef, 0x19, 0x8a, 0x2d, 0xd5, 0x74, 0x55, 0x45, 0xc1, 0xf0, 0xc4, 0x7a, 0xa3, 0xfb, 0x57, 0x76, 0xf8, 0x81 };
			byte[] input = Encoding.Default.GetBytes ("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq");
		
			string testName = "HMACRIPEMD160 Key #1 Test #6";
			hmac.Key = key1;
			HMACRIPEMD160_a (testName, hmac, input, result);
			HMACRIPEMD160_b (testName, hmac, input, result);
			HMACRIPEMD160_c (testName, hmac, input, result);
			HMACRIPEMD160_d (testName, hmac, input, result);
			HMACRIPEMD160_e (testName, hmac, input, result);
		}

		// RIPEMD160 ("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789") =
		//	b0e20b6e3116640286ed3a87a5713079b21f5189
		[Test]
		public void HMACRIPEMD160_Key1_Test7 () 
		{
			byte[] result = { 0xe4, 0x9c, 0x13, 0x6a, 0x9e, 0x56, 0x27, 0xe0, 0x68, 0x1b, 0x80, 0x8a, 0x3b, 0x97, 0xe6, 0xa6, 0xe6, 0x61, 0xae, 0x79 };
			byte[] input = Encoding.Default.GetBytes ("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789");
		
			string testName = "HMACRIPEMD160 Key #1 Test #7";
			hmac.Key = key1;
			HMACRIPEMD160_a (testName, hmac, input, result);
			HMACRIPEMD160_b (testName, hmac, input, result);
			HMACRIPEMD160_c (testName, hmac, input, result);
			HMACRIPEMD160_d (testName, hmac, input, result);
			HMACRIPEMD160_e (testName, hmac, input, result);
		}

		// RIPEMD160 ("123456789012345678901234567890123456789012345678901234567890123456
		//	78901234567890") = 9b752e45573d4b39f4dbd3323cab82bf63326bfb
		[Test]
		public void HMACRIPEMD160_Key1_Test8 () 
		{
			byte[] result = { 0x31, 0xbe, 0x3c, 0xc9, 0x8c, 0xee, 0x37, 0xb7, 0x9b, 0x06, 0x19, 0xe3, 0xe1, 0xc2, 0xbe, 0x4f, 0x1a, 0xa5, 0x6e, 0x6c };
			byte[] input = Encoding.Default.GetBytes ("12345678901234567890123456789012345678901234567890123456789012345678901234567890");
		
			string testName = "HMACRIPEMD160 Key #1 Test #8";
			hmac.Key = key1;
			HMACRIPEMD160_a (testName, hmac, input, result);
			HMACRIPEMD160_b (testName, hmac, input, result);
			HMACRIPEMD160_c (testName, hmac, input, result);
			HMACRIPEMD160_d (testName, hmac, input, result);
			HMACRIPEMD160_e (testName, hmac, input, result);
		}

		// RIPEMD160 (1000000 x 'a') = 52783243c1697bdbe16d37f97f68f08325dc1528
		[Test]
		public void HMACRIPEMD160_Key1_Test9 () 
		{
			byte[] result = { 0xc2, 0xaa, 0x88, 0xc6, 0x40, 0x56, 0x58, 0xdc, 0x22, 0x5e, 0x48, 0x54, 0x88, 0x37, 0x1f, 0xb2, 0x43, 0x3f, 0xa7, 0x35 };
			byte[] input = new byte [1000000];
			for (int i = 0; i < 1000000; i++)
				input[i] = 0x61; // a
		
			string testName = "HMACRIPEMD160 Key #1 Test #9";
			hmac.Key = key1;
			HMACRIPEMD160_a (testName, hmac, input, result);
			HMACRIPEMD160_b (testName, hmac, input, result);
			HMACRIPEMD160_c (testName, hmac, input, result);
			HMACRIPEMD160_d (testName, hmac, input, result);
			HMACRIPEMD160_e (testName, hmac, input, result);
		}

		static byte[] key2 = { 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef, 0xfe, 0xdc, 0xba, 0x98, 0x76, 0x54, 0x32, 0x10, 0x00, 0x11, 0x22, 0x33 };

		// HMACRIPEMD160 (key2, "") = fe69a66c7423eea9c8fa2eff8d9dafb4f17a62f5
		[Test]
		public void HMACRIPEMD160_Key2_Test1 () 
		{
			byte[] result = { 0xfe, 0x69, 0xa6, 0x6c, 0x74, 0x23, 0xee, 0xa9, 0xc8, 0xfa, 0x2e, 0xff, 0x8d, 0x9d, 0xaf, 0xb4, 0xf1, 0x7a, 0x62, 0xf5 };
			byte[] input = new byte [0];
		
			string testName = "HMACRIPEMD160 Key #2 Test #1";
			hmac.Key = key2;
			HMACRIPEMD160_a (testName, hmac, input, result);
			HMACRIPEMD160_b (testName, hmac, input, result);
			HMACRIPEMD160_c (testName, hmac, input, result);
			HMACRIPEMD160_d (testName, hmac, input, result);
			// N/A RIPEMD160_e (testName, hmac, input, result);
		}

		// HMACRIPEMD160 ("a") = 85743e899bc82dbfa36faaa7a25b7cfd372432cd
		[Test]
		public void HMACRIPEMD160_Key2_Test2 () 
		{
			byte[] result = { 0x85, 0x74, 0x3e, 0x89, 0x9b, 0xc8, 0x2d, 0xbf, 0xa3, 0x6f, 0xaa, 0xa7, 0xa2, 0x5b, 0x7c, 0xfd, 0x37, 0x24, 0x32, 0xcd };
			byte[] input = Encoding.Default.GetBytes ("a");
		
			string testName = "HMACRIPEMD160 Key #2 Test #2";
			hmac.Key = key2;
			HMACRIPEMD160_a (testName, hmac, input, result);
			HMACRIPEMD160_b (testName, hmac, input, result);
			HMACRIPEMD160_c (testName, hmac, input, result);
			HMACRIPEMD160_d (testName, hmac, input, result);
			HMACRIPEMD160_e (testName, hmac, input, result);
		}

		// HMACRIPEMD160 ("abc") = 6e4afd501fa6b4a1823ca3b10bd9aa0ba97ba182
		[Test]
		public void HMACRIPEMD160_Key2_Test3 () 
		{
			byte[] result = { 0x6e, 0x4a, 0xfd, 0x50, 0x1f, 0xa6, 0xb4, 0xa1, 0x82, 0x3c, 0xa3, 0xb1, 0x0b, 0xd9, 0xaa, 0x0b, 0xa9, 0x7b, 0xa1, 0x82 };
			byte[] input = Encoding.Default.GetBytes ("abc");
		
			string testName = "HMACRIPEMD160 Key #2 Test #3";
			hmac.Key = key2;
			HMACRIPEMD160_a (testName, hmac, input, result);
			HMACRIPEMD160_b (testName, hmac, input, result);
			HMACRIPEMD160_c (testName, hmac, input, result);
			HMACRIPEMD160_d (testName, hmac, input, result);
			HMACRIPEMD160_e (testName, hmac, input, result);
		}

		// RIPEMD160 ("message digest") = 2e066e624badb76a184c8f90fba053330e650e92
		[Test]
		public void HMACRIPEMD160_Key2_Test4 () 
		{
			byte[] result = { 0x2e, 0x06, 0x6e, 0x62, 0x4b, 0xad, 0xb7, 0x6a, 0x18, 0x4c, 0x8f, 0x90, 0xfb, 0xa0, 0x53, 0x33, 0x0e, 0x65, 0x0e, 0x92 };
			byte[] input = Encoding.Default.GetBytes ("message digest");
		
			string testName = "HMACRIPEMD160 Key #2 Test #4";
			hmac.Key = key2;
			HMACRIPEMD160_a (testName, hmac, input, result);
			HMACRIPEMD160_b (testName, hmac, input, result);
			HMACRIPEMD160_c (testName, hmac, input, result);
			HMACRIPEMD160_d (testName, hmac, input, result);
			HMACRIPEMD160_e (testName, hmac, input, result);
		}

		// RIPEMD160 ("abcdefghijklmnopqrstuvwxyz") = 07e942aa4e3cd7c04dedc1d46e2e8cc4c741b3d9
		[Test]
		public void HMACRIPEMD160_Key2_Test5 () 
		{
			byte[] result = { 0x07, 0xe9, 0x42, 0xaa, 0x4e, 0x3c, 0xd7, 0xc0, 0x4d, 0xed, 0xc1, 0xd4, 0x6e, 0x2e, 0x8c, 0xc4, 0xc7, 0x41, 0xb3, 0xd9 };
			byte[] input = Encoding.Default.GetBytes ("abcdefghijklmnopqrstuvwxyz");
		
			string testName = "HMACRIPEMD160 Key #2 Test #5";
			hmac.Key = key2;
			HMACRIPEMD160_a (testName, hmac, input, result);
			HMACRIPEMD160_b (testName, hmac, input, result);
			HMACRIPEMD160_c (testName, hmac, input, result);
			HMACRIPEMD160_d (testName, hmac, input, result);
			HMACRIPEMD160_e (testName, hmac, input, result);
		}

		// RIPEMD160 ("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq") =
		//	b6582318ddcfb67a53a67d676b8ad869aded629a
		[Test]
		public void HMACRIPEMD160_Key2_Test6 () 
		{
			byte[] result = { 0xb6, 0x58, 0x23, 0x18, 0xdd, 0xcf, 0xb6, 0x7a, 0x53, 0xa6, 0x7d, 0x67, 0x6b, 0x8a, 0xd8, 0x69, 0xad, 0xed, 0x62, 0x9a };
			byte[] input = Encoding.Default.GetBytes ("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq");
		
			string testName = "HMACRIPEMD160 Key #2 Test #6";
			hmac.Key = key2;
			HMACRIPEMD160_a (testName, hmac, input, result);
			HMACRIPEMD160_b (testName, hmac, input, result);
			HMACRIPEMD160_c (testName, hmac, input, result);
			HMACRIPEMD160_d (testName, hmac, input, result);
			HMACRIPEMD160_e (testName, hmac, input, result);
		}

		// RIPEMD160 ("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789") =
		//	f1be3ee877703140d34f97ea1ab3a07c141333e2
		[Test]
		public void HMACRIPEMD160_Key2_Test7 () 
		{
			byte[] result = { 0xf1, 0xbe, 0x3e, 0xe8, 0x77, 0x70, 0x31, 0x40, 0xd3, 0x4f, 0x97, 0xea, 0x1a, 0xb3, 0xa0, 0x7c, 0x14, 0x13, 0x33, 0xe2 };
			byte[] input = Encoding.Default.GetBytes ("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789");
		
			string testName = "HMACRIPEMD160 Key #2 Test #7";
			hmac.Key = key2;
			HMACRIPEMD160_a (testName, hmac, input, result);
			HMACRIPEMD160_b (testName, hmac, input, result);
			HMACRIPEMD160_c (testName, hmac, input, result);
			HMACRIPEMD160_d (testName, hmac, input, result);
			HMACRIPEMD160_e (testName, hmac, input, result);
		}

		// RIPEMD160 ("123456789012345678901234567890123456789012345678901234567890123456
		//	78901234567890") = 85f164703e61a63131be7e45958e0794123904f9
		[Test]
		public void HMACRIPEMD160_Key2_Test8 () 
		{
			byte[] result = { 0x85, 0xf1, 0x64, 0x70, 0x3e, 0x61, 0xa6, 0x31, 0x31, 0xbe, 0x7e, 0x45, 0x95, 0x8e, 0x07, 0x94, 0x12, 0x39, 0x04, 0xf9 };
			byte[] input = Encoding.Default.GetBytes ("12345678901234567890123456789012345678901234567890123456789012345678901234567890");
		
			string testName = "HMACRIPEMD160 Key #2 Test #8";
			hmac.Key = key2;
			HMACRIPEMD160_a (testName, hmac, input, result);
			HMACRIPEMD160_b (testName, hmac, input, result);
			HMACRIPEMD160_c (testName, hmac, input, result);
			HMACRIPEMD160_d (testName, hmac, input, result);
			HMACRIPEMD160_e (testName, hmac, input, result);
		}

		// RIPEMD160 (1000000 x 'a') = 82a504a002ba6e6c67f3cd67cedb66dc169bab7a
		[Test]
		public void HMACRIPEMD160_Key2_Test9 () 
		{
			byte[] result = { 0x82, 0xa5, 0x04, 0xa0, 0x02, 0xba, 0x6e, 0x6c, 0x67, 0xf3, 0xcd, 0x67, 0xce, 0xdb, 0x66, 0xdc, 0x16, 0x9b, 0xab, 0x7a };
			byte[] input = new byte [1000000];
			for (int i = 0; i < 1000000; i++)
				input[i] = 0x61; // a
		
			string testName = "HMACRIPEMD160 Key #2 Test #9";
			hmac.Key = key2;
			HMACRIPEMD160_a (testName, hmac, input, result);
			HMACRIPEMD160_b (testName, hmac, input, result);
			HMACRIPEMD160_c (testName, hmac, input, result);
			HMACRIPEMD160_d (testName, hmac, input, result);
			HMACRIPEMD160_e (testName, hmac, input, result);
		}

		public void HMACRIPEMD160_a (string testName, HMACRIPEMD160 hmac, byte[] input, byte[] result) 
		{
			byte[] output = hmac.ComputeHash (input); 
			Assert.AreEqual (result, output, testName + ".a.1");
			Assert.AreEqual (result, hmac.Hash, testName + ".a.2");
			// required or next operation will still return old hash
			hmac.Initialize ();
		}

		public void HMACRIPEMD160_b (string testName, HMACRIPEMD160 hmac, byte[] input, byte[] result) 
		{
			byte[] output = hmac.ComputeHash (input, 0, input.Length); 
			Assert.AreEqual (result, output, testName + ".b.1");
			Assert.AreEqual (result, hmac.Hash, testName + ".b.2");
			// required or next operation will still return old hash
			hmac.Initialize ();
		}

		public void HMACRIPEMD160_c (string testName, HMACRIPEMD160 hmac, byte[] input, byte[] result) 
		{
			MemoryStream ms = new MemoryStream (input);
			byte[] output = hmac.ComputeHash (ms); 
			Assert.AreEqual (result, output, testName + ".c.1");
			Assert.AreEqual (result, hmac.Hash, testName + ".c.2");
			// required or next operation will still return old hash
			hmac.Initialize ();
		}

		public void HMACRIPEMD160_d (string testName, HMACRIPEMD160 hmac, byte[] input, byte[] result) 
		{
			hmac.TransformFinalBlock (input, 0, input.Length);
			Assert.AreEqual (result, hmac.Hash, testName + ".d");
			// required or next operation will still return old hash
			hmac.Initialize ();
		}

		public void HMACRIPEMD160_e (string testName, HMACRIPEMD160 hmac, byte[] input, byte[] result) 
		{
			byte[] copy = new byte [input.Length];
			for (int i=0; i < input.Length - 1; i++)
				hmac.TransformBlock (input, i, 1, copy, i);
			hmac.TransformFinalBlock (input, input.Length - 1, 1);
			Assert.AreEqual (result, hmac.Hash, testName + ".e");
			// required or next operation will still return old hash
			hmac.Initialize ();
		}

		// none of those values changes for any implementation of RIPEMD160
		[Test]
		public void Invariants ()
		{
			Assert.IsTrue (hmac.CanReuseTransform, "HMACRIPEMD160.CanReuseTransform");
			Assert.IsTrue (hmac.CanTransformMultipleBlocks, "HMACRIPEMD160.CanTransformMultipleBlocks");
			Assert.AreEqual ("RIPEMD160", hmac.HashName, "HMACRIPEMD160.HashName");
			Assert.AreEqual (160, hmac.HashSize, "HMACRIPEMD160.HashSize");
			Assert.AreEqual (1, hmac.InputBlockSize, "HMACRIPEMD160.InputBlockSize");
			Assert.AreEqual (1, hmac.OutputBlockSize, "HMACRIPEMD160.OutputBlockSize");
			Assert.AreEqual ("System.Security.Cryptography.HMACRIPEMD160", hmac.ToString (), "HMACRIPEMD160.ToString()");
		}

		[Test]
		public void BlockSize ()
		{
			HR160 hmac = new HR160 ();
			Assert.AreEqual (64, hmac.BlockSize, "BlockSizeValue");
		}
	}
}

#endif
