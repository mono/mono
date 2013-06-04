//
// ARC4ManagedTest.cs - NUnit Test Cases for Alleged RC4(tm)
//	RC4 is a trademark of RSA Security
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.Security.Cryptography;
using Mono.Security.Cryptography;

namespace MonoTests.Mono.Security.Cryptography {

	// References
	// a.	Usenet 1994 - RC4 Algorithm revealed
	//	http://www.qrst.de/html/dsds/rc4.htm
	// b.	Netscape SSL version 3 implementation details
	//	Export Client SSL Connection Details
	//	http://wp.netscape.com/eng/ssl3/traces/trc-clnt-ex.html

	[TestFixture]
	public class ARC4ManagedTest {

		// because most crypto stuff works with byte[] buffers
		static public void AssertEquals (string msg, byte[] array1, byte[] array2) 
		{
			if ((array1 == null) && (array2 == null))
				return;
			if (array1 == null)
				Assert.Fail (msg + " -> First array is NULL");
			if (array2 == null)
				Assert.Fail (msg + " -> Second array is NULL");
	        
			bool a = (array1.Length == array2.Length);
			if (a) {
				for (int i = 0; i < array1.Length; i++) {
					if (array1 [i] != array2 [i]) {
						a = false;
						break;
					}
				}
			}
			msg += " -> Expected " + BitConverter.ToString (array1, 0);
			msg += " is different than " + BitConverter.ToString (array2, 0);
			Assert.IsTrue (a, msg);
		}

		// from ref. a
		[Test]
		public void Vector0 () 
		{
			byte[] key = { 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef };
			byte[] input = { 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef };
			byte[] expected = { 0x75, 0xb7, 0x87, 0x80, 0x99, 0xe0, 0xc5, 0x96 };
			ARC4Managed rc4 = new ARC4Managed ();
			rc4.Key = key;
			ICryptoTransform stream = rc4.CreateEncryptor ();
			byte[] output = stream.TransformFinalBlock (input, 0, input.Length);
			AssertEquals ("RC4 - Test Vector 0", expected, output);
		}

		// from ref. a
		[Test]
		public void Vector1 () 
		{
			byte[] key = { 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef };
			byte[] input = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
			byte[] expected = { 0x74, 0x94, 0xc2, 0xe7, 0x10, 0x4b, 0x08, 0x79 };
			ARC4Managed rc4 = new ARC4Managed ();
			rc4.Key = key;
			ICryptoTransform stream = rc4.CreateEncryptor ();
			byte[] output = stream.TransformFinalBlock (input, 0, input.Length);
			AssertEquals ("RC4 - Test Vector 1", expected, output);
		}

		// from ref. a
		[Test]
		public void Vector2 () 
		{
			byte[] key = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
			byte[] input = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
			byte[] expected = { 0xde, 0x18, 0x89, 0x41, 0xa3, 0x37, 0x5d, 0x3a };
			ARC4Managed rc4 = new ARC4Managed ();
			rc4.Key = key;
			ICryptoTransform stream = rc4.CreateEncryptor ();
			byte[] output = stream.TransformFinalBlock (input, 0, input.Length);
			AssertEquals ("RC4 - Test Vector 2", expected, output);
		}

		// from ref. a
		[Test]
		public void Vector3 () 
		{
			byte[] key = { 0xef, 0x01, 0x23, 0x45 };
			byte[] input = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
			byte[] expected = { 0xd6, 0xa1, 0x41, 0xa7, 0xec, 0x3c, 0x38, 0xdf, 0xbd, 0x61 };
			ARC4Managed rc4 = new ARC4Managed ();
			rc4.Key = key;
			ICryptoTransform stream = rc4.CreateEncryptor ();
			byte[] output = stream.TransformFinalBlock (input, 0, input.Length);
			AssertEquals ("RC4 - Test Vector 3", expected, output);
		}

		// from ref. a
		[Test]
		public void Vector4 () 
		{
			byte[] key = { 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef };
			byte[] input = new byte [512];
			for (int i=0; i < input.Length; i++)
				input [i] = 0x01;
			byte[] expected = { 0x75, 0x95, 0xc3, 0xe6, 0x11, 0x4a, 0x09, 0x78, 0x0c, 0x4a, 0xd4, 
				0x52, 0x33, 0x8e, 0x1f, 0xfd, 0x9a, 0x1b, 0xe9, 0x49, 0x8f, 
				0x81, 0x3d, 0x76, 0x53, 0x34, 0x49, 0xb6, 0x77, 0x8d, 0xca, 
				0xd8, 0xc7, 0x8a, 0x8d, 0x2b, 0xa9, 0xac, 0x66, 0x08, 0x5d, 
				0x0e, 0x53, 0xd5, 0x9c, 0x26, 0xc2, 0xd1, 0xc4, 0x90, 0xc1, 
				0xeb, 0xbe, 0x0c, 0xe6, 0x6d, 0x1b, 0x6b, 0x1b, 0x13, 0xb6, 
				0xb9, 0x19, 0xb8, 0x47, 0xc2, 0x5a, 0x91, 0x44, 0x7a, 0x95, 
				0xe7, 0x5e, 0x4e, 0xf1, 0x67, 0x79, 0xcd, 0xe8, 0xbf, 0x0a, 
				0x95, 0x85, 0x0e, 0x32, 0xaf, 0x96, 0x89, 0x44, 0x4f, 0xd3, 
				0x77, 0x10, 0x8f, 0x98, 0xfd, 0xcb, 0xd4, 0xe7, 0x26, 0x56, 
				0x75, 0x00, 0x99, 0x0b, 0xcc, 0x7e, 0x0c, 0xa3, 0xc4, 0xaa, 
				0xa3, 0x04, 0xa3, 0x87, 0xd2, 0x0f, 0x3b, 0x8f, 0xbb, 0xcd, 
				0x42, 0xa1, 0xbd, 0x31, 0x1d, 0x7a, 0x43, 0x03, 0xdd, 0xa5, 
				0xab, 0x07, 0x88, 0x96, 0xae, 0x80, 0xc1, 0x8b, 0x0a, 0xf6, 
				0x6d, 0xff, 0x31, 0x96, 0x16, 0xeb, 0x78, 0x4e, 0x49, 0x5a, 
				0xd2, 0xce, 0x90, 0xd7, 0xf7, 0x72, 0xa8, 0x17, 0x47, 0xb6, 
				0x5f, 0x62, 0x09, 0x3b, 0x1e, 0x0d, 0xb9, 0xe5, 0xba, 0x53, 
				0x2f, 0xaf, 0xec, 0x47, 0x50, 0x83, 0x23, 0xe6, 0x71, 0x32, 
				0x7d, 0xf9, 0x44, 0x44, 0x32, 0xcb, 0x73, 0x67, 0xce, 0xc8, 
				0x2f, 0x5d, 0x44, 0xc0, 0xd0, 0x0b, 0x67, 0xd6, 0x50, 0xa0, 
				0x75, 0xcd, 0x4b, 0x70, 0xde, 0xdd, 0x77, 0xeb, 0x9b, 0x10, 
				0x23, 0x1b, 0x6b, 0x5b, 0x74, 0x13, 0x47, 0x39, 0x6d, 0x62, 
				0x89, 0x74, 0x21, 0xd4, 0x3d, 0xf9, 0xb4, 0x2e, 0x44, 0x6e, 
				0x35, 0x8e, 0x9c, 0x11, 0xa9, 0xb2, 0x18, 0x4e, 0xcb, 0xef, 
				0x0c, 0xd8, 0xe7, 0xa8, 0x77, 0xef, 0x96, 0x8f, 0x13, 0x90, 
				0xec, 0x9b, 0x3d, 0x35, 0xa5, 0x58, 0x5c, 0xb0, 0x09, 0x29, 
				0x0e, 0x2f, 0xcd, 0xe7, 0xb5, 0xec, 0x66, 0xd9, 0x08, 0x4b, 
				0xe4, 0x40, 0x55, 0xa6, 0x19, 0xd9, 0xdd, 0x7f, 0xc3, 0x16, 
				0x6f, 0x94, 0x87, 0xf7, 0xcb, 0x27, 0x29, 0x12, 0x42, 0x64, 
				0x45, 0x99, 0x85, 0x14, 0xc1, 0x5d, 0x53, 0xa1, 0x8c, 0x86, 
				0x4c, 0xe3, 0xa2, 0xb7, 0x55, 0x57, 0x93, 0x98, 0x81, 0x26, 
				0x52, 0x0e, 0xac, 0xf2, 0xe3, 0x06, 0x6e, 0x23, 0x0c, 0x91, 
				0xbe, 0xe4, 0xdd, 0x53, 0x04, 0xf5, 0xfd, 0x04, 0x05, 0xb3, 
				0x5b, 0xd9, 0x9c, 0x73, 0x13, 0x5d, 0x3d, 0x9b, 0xc3, 0x35, 
				0xee, 0x04, 0x9e, 0xf6, 0x9b, 0x38, 0x67, 0xbf, 0x2d, 0x7b, 
				0xd1, 0xea, 0xa5, 0x95, 0xd8, 0xbf, 0xc0, 0x06, 0x6f, 0xf8, 
				0xd3, 0x15, 0x09, 0xeb, 0x0c, 0x6c, 0xaa, 0x00, 0x6c, 0x80, 
				0x7a, 0x62, 0x3e, 0xf8, 0x4c, 0x3d, 0x33, 0xc1, 0x95, 0xd2, 
				0x3e, 0xe3, 0x20, 0xc4, 0x0d, 0xe0, 0x55, 0x81, 0x57, 0xc8, 
				0x22, 0xd4, 0xb8, 0xc5, 0x69, 0xd8, 0x49, 0xae, 0xd5, 0x9d, 
				0x4e, 0x0f, 0xd7, 0xf3, 0x79, 0x58, 0x6b, 0x4b, 0x7f, 0xf6, 
				0x84, 0xed, 0x6a, 0x18, 0x9f, 0x74, 0x86, 0xd4, 0x9b, 0x9c, 
				0x4b, 0xad, 0x9b, 0xa2, 0x4b, 0x96, 0xab, 0xf9, 0x24, 0x37, 
				0x2c, 0x8a, 0x8f, 0xff, 0xb1, 0x0d, 0x55, 0x35, 0x49, 0x00, 
				0xa7, 0x7a, 0x3d, 0xb5, 0xf2, 0x05, 0xe1, 0xb9, 0x9f, 0xcd, 
				0x86, 0x60, 0x86, 0x3a, 0x15, 0x9a, 0xd4, 0xab, 0xe4, 0x0f, 
				0xa4, 0x89, 0x34, 0x16, 0x3d, 0xdd, 0xe5, 0x42, 0xa6, 0x58, 
				0x55, 0x40, 0xfd, 0x68, 0x3c, 0xbf, 0xd8, 0xc0, 0x0f, 0x12, 
				0x12, 0x9a, 0x28, 0x4d, 0xea, 0xcc, 0x4c, 0xde, 0xfe, 0x58, 
				0xbe, 0x71, 0x37, 0x54, 0x1c, 0x04, 0x71, 0x26, 0xc8, 0xd4, 
				0x9e, 0x27, 0x55, 0xab, 0x18, 0x1a, 0xb7, 0xe9, 0x40, 0xb0, 
				0xc0 };
			ARC4Managed rc4 = new ARC4Managed ();
			rc4.Key = key;
			ICryptoTransform stream = rc4.CreateEncryptor ();
			byte[] output = stream.TransformFinalBlock (input, 0, input.Length);
			AssertEquals ("RC4 - Test Vector 4", expected, output);
		}

		static byte[] clientWriteKey = { 0x32, 0x10, 0xcd, 0xe1, 0xd6, 0xdc, 0x07, 0x83, 0xf3, 0x75, 0x4c, 0x32, 0x2e, 0x59, 0x96, 0x61 };
		static byte[] serverWriteKey = { 0xed, 0x0e, 0x56, 0xc8, 0x95, 0x12, 0x37, 0xb6, 0x21, 0x17, 0x1c, 0x72, 0x79, 0x91, 0x12, 0x1e };

		// SSL3 Client's Finished Handshake (from ref. b)
		[Test]
		public void SSLClient () 
		{
			byte[] data = { 0x14, 0x00, 0x00, 0x24, 0xf2, 0x40, 0x10, 0x3f, 0x74, 0x63, 0xea, 0xe8, 0x7a, 0x27, 0x23, 0x56, 0x5f, 0x59, 0x07, 0xd2, 0xa3, 0x79, 0x5d, 0xb7, 0x8b, 0x94, 0xdb, 0xcf, 0xfa, 0xf5, 0x18, 0x22, 0x15, 0x7b, 0xf2, 0x4a, 0x96, 0x52, 0x9a, 0x0e, 0xd3, 0x09, 0xde, 0x28, 0x84, 0xa7, 0x07, 0x5c, 0x7c, 0x0c, 0x08, 0x85, 0x6b, 0x4f, 0x63, 0x04 };
			ARC4Managed rc4 = new ARC4Managed ();
			rc4.Key = clientWriteKey;
			// encrypt inplace (in same buffer)
			rc4.TransformBlock (data, 0, data.Length, data, 0); 
			byte[] expectedData = { 0xed, 0x37, 0x7f, 0x16, 0xd3, 0x11, 0xe8, 0xa3, 0xe1, 0x2a, 0x20, 0xb7, 0x88, 0xf6, 0x11, 0xf3, 0xa6, 0x7d, 0x37, 0xf7, 0x17, 0xac, 0x67, 0x20, 0xb8, 0x0e, 0x88, 0xd1, 0xa0, 0xc6, 0x83, 0xe4, 0x80, 0xe8, 0xc7, 0xe3, 0x0b, 0x91, 0x29, 0x30, 0x29, 0xe4, 0x28, 0x47, 0xb7, 0x40, 0xa4, 0xd1, 0x3c, 0xda, 0x82, 0xb7, 0xb3, 0x9f, 0x67, 0x10 };
			AssertEquals ("RC4 - Client's Finished Handshake", expectedData, data);
		}

		// SSL3 Server Finished Handshake (from ref. b)
		[Test]
		public void SSLServer () 
		{
			byte[] encryptedData = { 0x54, 0x3c, 0xe1, 0xe7, 0x4d, 0x77, 0x76, 0x62, 0x86, 0xfa, 0x4e, 0x0a, 0x6f, 0x5f, 0x6a, 0x3d, 0x43, 0x26, 0xf4, 0xad, 0x8d, 0x3e, 0x09, 0x0b, 0x2b, 0xf7, 0x9f, 0x49, 0x44, 0x92, 0xfb, 0xa9, 0xa4, 0xb0, 0x5a, 0xd8, 0x72, 0x77, 0x6e, 0x8b, 0xb3, 0x78, 0xfb, 0xda, 0xe0, 0x25, 0xef, 0xb3, 0xf5, 0xa7, 0x90, 0x08, 0x6d, 0x60, 0xd5, 0x4e };
			ARC4Managed rc4 = new ARC4Managed ();
			rc4.Key = serverWriteKey;
			// decrypt inplace (in same buffer)
			rc4.TransformBlock (encryptedData, 0, encryptedData.Length, encryptedData, 0); 
			byte[] expectedData = { 0x14, 0x00, 0x00, 0x24, 0xb7, 0xcc, 0xd6, 0x05, 0x6b, 0xfc, 0xfa, 0x6d, 0xfa, 0xdd, 0x76, 0x81, 0x45, 0x36, 0xe4, 0xf4, 0x26, 0x35, 0x72, 0x2c, 0xec, 0x87, 0x62, 0x1f, 0x55, 0x08, 0x05, 0x4f, 0xc8, 0xf5, 0x7c, 0x49, 0xe2, 0xee, 0xc5, 0xba, 0xbd, 0x69, 0x27, 0x3b, 0xd0, 0x13, 0x23, 0x52, 0xed, 0xec, 0x11, 0x55, 0xd8, 0xb9, 0x90, 0x8c };
			AssertEquals ("RC4 - Server's Finished Handshake", expectedData, encryptedData);
		}
		
		[Test]
		public void DefaultProperties () 
		{
			ARC4Managed rc4 = new ARC4Managed ();
			Assert.IsFalse (rc4.CanReuseTransform, "CanReuseTransform");
			Assert.IsTrue (rc4.CanTransformMultipleBlocks, "CanTransformMultipleBlocks");
			Assert.AreEqual (1, rc4.InputBlockSize, "InputBlockSize");
			Assert.AreEqual (1, rc4.OutputBlockSize, "OutputBlockSize");
		}
		
		[Test]
		public void DefaultSizes () 
		{
			ARC4Managed rc4 = new ARC4Managed ();
			rc4.GenerateKey ();
			rc4.GenerateIV ();
			Assert.AreEqual (16, rc4.Key.Length, "Key.Length");
			Assert.AreEqual (128, rc4.KeySize, "KeySize");
			Assert.AreEqual (0, rc4.IV.Length, "IV.Length");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TransformBlock_InputBuffer_Null () 
		{
			byte[] output = new byte [1];
			ARC4Managed rc4 = new ARC4Managed ();
			rc4.TransformBlock (null, 0, 1, output, 0); 
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TransformBlock_InputOffset_Negative () 
		{
			byte[] input = new byte [1];
			byte[] output = new byte [1];
			ARC4Managed rc4 = new ARC4Managed ();
			rc4.TransformBlock (input, -1, 1, output, 0); 
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TransformBlock_InputOffset_Overflow () 
		{
			byte[] input = new byte [1];
			byte[] output = new byte [1];
			ARC4Managed rc4 = new ARC4Managed ();
			rc4.TransformBlock (input, Int32.MaxValue, 1, output, 0); 
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TransformBlock_InputCount_Negative () 
		{
			byte[] input = new byte [1];
			byte[] output = new byte [1];
			ARC4Managed rc4 = new ARC4Managed ();
			rc4.TransformBlock (input, 0, -1, output, 0); 
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TransformBlock_InputCount_Overflow () 
		{
			byte[] input = new byte [1];
			byte[] output = new byte [1];
			ARC4Managed rc4 = new ARC4Managed ();
			rc4.TransformBlock (input, 1, Int32.MaxValue, output, 0); 
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TransformBlock_OutputBuffer_Null () 
		{
			byte[] input = new byte [1];
			ARC4Managed rc4 = new ARC4Managed ();
			rc4.TransformBlock (input, 0, 1, null, 0); 
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TransformBlock_OutputOffset_Negative () 
		{
			byte[] input = new byte [1];
			byte[] output = new byte [1];
			ARC4Managed rc4 = new ARC4Managed ();
			rc4.TransformBlock (input, 0, 1, output, -1); 
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TransformBlock_OutputOffset_Overflow () 
		{
			byte[] input = new byte [1];
			byte[] output = new byte [1];
			ARC4Managed rc4 = new ARC4Managed ();
			rc4.TransformBlock (input, 0, 1, output, Int32.MaxValue); 
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TransformFinalBlock_InputBuffer_Null () 
		{
			ARC4Managed rc4 = new ARC4Managed ();
			rc4.TransformFinalBlock (null, 0, 1); 
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TransformFinalBlock_InputOffset_Negative () 
		{
			byte[] input = new byte [1];
			ARC4Managed rc4 = new ARC4Managed ();
			rc4.TransformFinalBlock (input, -1, 1); 
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TransformFinalBlock_InputOffset_Overflow () 
		{
			byte[] input = new byte [1];
			ARC4Managed rc4 = new ARC4Managed ();
			rc4.TransformFinalBlock (input, Int32.MaxValue, 1); 
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TransformFinalBlock_InputCount_Negative () 
		{
			byte[] input = new byte [1];
			ARC4Managed rc4 = new ARC4Managed ();
			rc4.TransformFinalBlock (input, 0, -1); 
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TransformFinalBlock_InputCount_Overflow () 
		{
			byte[] input = new byte [1];
			ARC4Managed rc4 = new ARC4Managed ();
			rc4.TransformFinalBlock (input, 1, Int32.MaxValue); 
		}
	}
}
