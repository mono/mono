//
// System.Security.Cryptography.Test SHA512 NUnit test classes.
//
// Author:
//   Matthew S. Ford (Matthew.S.Ford@Rose-Hulman.Edu)
//
// Copyright 2001 by Matthew S. Ford.
//

using NUnit.Framework;
using System;
using System.Security.Cryptography;
using System.Text;

namespace System.Security.Cryptography.Test {
	public class SHA512HashingTest : TestCase {
		byte[] _dataToHash;
		byte[] _expectedHash;
		
		public SHA512HashingTest (string name, byte[] dataToHash, byte[] expectedHash) : base(name) {
			_dataToHash = dataToHash;
			_expectedHash = expectedHash;
		}
		
		public static bool ArrayEquals (byte[] arr1, byte[] arr2) {
			int i;
			if (arr1.GetLength(0) != arr2.GetLength(0))
				return false;
		
			for (i=0; i<arr1.GetLength(0); i++) {
				if (arr1[i] != arr2[i])
					return false;
			}
		
			return true;
		}

		protected override void RunTest () {
			SHA512 sha = new SHA512Managed ();
			byte[] hash;
			
			hash = sha.ComputeHash (_dataToHash);
			
			Assert (ArrayEquals (hash, _expectedHash));
		}

	}
	
	public class SHA512TestSet {
		public SHA512TestSet () {

		}

		public static void Main () {

		}

		public static ITest Suite { 
			get {
				TestSuite suite = new TestSuite ();

				byte[] hash0 = {0xDD, 0xAF, 0x35, 0xA1, 0x93, 0x61, 0x7A, 0xBA, 0xCC, 0x41, 0x73, 0x49, 0xAE, 0x20, 0x41, 0x31, 0x12, 0xE6, 0xFA, 0x4E, 0x89, 0xA9, 0x7E, 0xA2, 0x0A, 0x9E, 0xEE, 0xE6, 0x4B, 0x55, 0xD3, 0x9A, 0x21, 0x92, 0x99, 0x2A, 0x27, 0x4F, 0xC1, 0xA8, 0x36, 0xBA, 0x3C, 0x23, 0xA3, 0xFE, 0xEB, 0xBD, 0x45, 0x4D, 0x44, 0x23, 0x64, 0x3C, 0xE8, 0x0E, 0x2A, 0x9A, 0xC9, 0x4F, 0xA5, 0x4C, 0xA4, 0x9F};
				suite.AddTest (new SHA512HashingTest ("abc", Encoding.UTF8.GetBytes("abc"), hash0));

				byte[] hash1 = {0x8E, 0x95, 0x9B, 0x75, 0xDA, 0xE3, 0x13, 0xDA, 0x8C, 0xF4, 0xF7, 0x28, 0x14, 0xFC, 0x14, 0x3F, 0x8F, 0x77, 0x79, 0xC6, 0xEB, 0x9F, 0x7F, 0xA1, 0x72, 0x99, 0xAE, 0xAD, 0xB6, 0x88, 0x90, 0x18, 0x50, 0x1D, 0x28, 0x9E, 0x49, 0x00, 0xF7, 0xE4, 0x33, 0x1B, 0x99, 0xDE, 0xC4, 0xB5, 0x43, 0x3A, 0xC7, 0xD3, 0x29, 0xEE, 0xB6, 0xDD, 0x26, 0x54, 0x5E, 0x96, 0xE5, 0x5B, 0x87, 0x6B, 0xE9, 0x09};
				suite.AddTest (new SHA512HashingTest ("abcdefghbcdefghicdefghijdefghijkefghijklfghijklmghijklmnhijklmnoijklmnopjklmnopqklmnopqrlmnopqrsmnopqrstnopqrstu", Encoding.UTF8.GetBytes("abcdefghbcdefghicdefghijdefghijkefghijklfghijklmghijklmnhijklmnoijklmnopjklmnopqklmnopqrlmnopqrsmnopqrstnopqrstu"), hash1));

				//byte[] hash2 = {};
				//suite.AddTest (new SHA512HashingTest ("abc", Encoding.UTF8.GetBytes("abc"), hash2));

				//byte[] hash3 = {};
				//suite.AddTest (new SHA512HashingTest ("abcdefghijklmnopqrstuvwxyz", Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwxyz"), hash3));

				//byte[] hash4 = {};
				//suite.AddTest (new SHA512HashingTest ("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq", Encoding.UTF8.GetBytes("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq"), hash4));

				return suite;
			}
		}
	}
}

