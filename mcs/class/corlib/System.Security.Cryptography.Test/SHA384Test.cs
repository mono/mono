//
// System.Security.Cryptography.Test SHA384 NUnit test classes.
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
	public class SHA384HashingTest : TestCase {
		byte[] _dataToHash;
		byte[] _expectedHash;
		
		public SHA384HashingTest (string name, byte[] dataToHash, byte[] expectedHash) : base(name) {
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
			SHA384 sha = new SHA384Managed ();
			byte[] hash;
			
			hash = sha.ComputeHash (_dataToHash);
			
			Assert (ArrayEquals (hash, _expectedHash));
		}

	}
	
	public class SHA384TestSet {
		public SHA384TestSet () {

		}

		public static void Main () {

		}

		public static ITest Suite { 
			get {
				TestSuite suite = new TestSuite ();

				byte[] hash0 = {0xCB, 0x00, 0x75, 0x3F, 0x45, 0xA3, 0x5E, 0x8B, 0xB5, 0xA0, 0x3D, 0x69, 0x9A, 0xC6, 0x50, 0x07, 0x27, 0x2C, 0x32, 0xAB, 0x0E, 0xDE, 0xD1, 0x63, 0x1A, 0x8B, 0x60, 0x5A, 0x43, 0xFF, 0x5B, 0xED, 0x80, 0x86, 0x07, 0x2B, 0xA1, 0xE7, 0xCC, 0x23, 0x58, 0xBA, 0xEC, 0xA1, 0x34, 0xC8, 0x25, 0xA7};
				suite.AddTest (new SHA384HashingTest ("abc", Encoding.UTF8.GetBytes("abc"), hash0));

				byte[] hash1 = {0x09, 0x33, 0x0C, 0x33, 0xF7, 0x11, 0x47, 0xE8, 0x3D, 0x19, 0x2F, 0xC7, 0x82, 0xCD, 0x1B, 0x47, 0x53, 0x11, 0x1B, 0x17, 0x3B, 0x3B, 0x05, 0xD2, 0x2F, 0xA0, 0x80, 0x86, 0xE3, 0xB0, 0xF7, 0x12, 0xFC, 0xC7, 0xC7, 0x1A, 0x55, 0x7E, 0x2D, 0xB9, 0x66, 0xC3, 0xE9, 0xFA, 0x91, 0x74, 0x60, 0x39};
				suite.AddTest (new SHA384HashingTest ("abcdefghbcdefghicdefghijdefghijkefghijklfghijklmghijklmnhijklmnoijklmnopjklmnopqklmnopqrlmnopqrsmnopqrstnopqrstu", Encoding.UTF8.GetBytes("abcdefghbcdefghicdefghijdefghijkefghijklfghijklmghijklmnhijklmnoijklmnopjklmnopqklmnopqrlmnopqrsmnopqrstnopqrstu"), hash1));

				//byte[] hash2 = {};
				//suite.AddTest (new SHA384HashingTest ("abc", Encoding.UTF8.GetBytes("abc"), hash2));

				//byte[] hash3 = {};
				//suite.AddTest (new SHA384HashingTest ("abcdefghijklmnopqrstuvwxyz", Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwxyz"), hash3));

				//byte[] hash4 = {};
				//suite.AddTest (new SHA384HashingTest ("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq", Encoding.UTF8.GetBytes("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq"), hash4));

				return suite;
			}
		}
	}
}

