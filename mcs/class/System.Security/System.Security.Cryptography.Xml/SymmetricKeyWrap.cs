//
// SymmetricKeyWrap.cs - Implements symmetric key wrap algorithms
//
// Author:
//      Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004
//

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

using System.IO;
using System.Security.Cryptography;

namespace System.Security.Cryptography.Xml { 

	internal class SymmetricKeyWrap {

		public SymmetricKeyWrap ()
		{
		}

		public static byte[] AESKeyWrapEncrypt (byte[] rgbKey, byte[] rgbWrappedKeyData)
		{
			SymmetricAlgorithm symAlg = SymmetricAlgorithm.Create ("Rijndael");

			// Apparently no one felt the need to document that this requires Electronic Codebook mode.
			symAlg.Mode = CipherMode.ECB;

			// This was also not documented anywhere.
			symAlg.IV = new byte [16] {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};
	
			ICryptoTransform transform = symAlg.CreateEncryptor (rgbKey, symAlg.IV);

			int N = rgbWrappedKeyData.Length / 8;
			byte[] A;
			byte[] B = new Byte [16];
			byte [] C = new byte [8 * (N + 1)];

			// 1. if N is 1:
			//       B = AES(K)enc(0xA6A6A6A6A6A6A6A6|P(1))
			//       C(0) = MSB(B)
			//       C(1) = LSB(B)
			if (N == 1) {
				A = new byte [8] {0xa6, 0xa6, 0xa6, 0xa6, 0xa6, 0xa6, 0xa6, 0xa6};
				transform.TransformBlock (Concatenate (A, rgbWrappedKeyData), 0, 16, B, 0);
				Buffer.BlockCopy (MSB(B), 0, C, 0, 8);
				Buffer.BlockCopy (LSB(B), 0, C, 8, 8);
			} else {
				// if N > 1, perform the following steps:
				// 2. Initialize variables:
				//       Set A to 0xA6A6A6A6A6A6A6A6
				//       For i = 1 to N,
				//          R(i) = P(i)
				A = new byte [8] {0xa6, 0xa6, 0xa6, 0xa6, 0xa6, 0xa6, 0xa6, 0xa6};
	
				byte[][] R = new byte [N + 1][];
				for (int i = 1; i <= N; i += 1) {
					R [i] = new byte [8];
					Buffer.BlockCopy (rgbWrappedKeyData, 8 * (i - 1), R [i], 0, 8);
				}

				// 3. Calculate intermediate values:
				//       For j = 0 to 5
				//          For i = 1 to N
				//             t = i + j * N
				//             B = AES(K)enc(A|R(i))
				//             A = XOR(t, MSB(B))
				//             R(i) = LSB(B)

				for (int j = 0; j <= 5; j += 1) {
					for (int i = 1; i <= N; i += 1) {
						transform.TransformBlock (Concatenate (A, R [i]), 0, 16, B, 0);
	
						// Yawn.  It was nice of those at NIST to document how exactly we should XOR 
						// an integer value with a byte array.  Not.
						byte[] T = BitConverter.GetBytes ((long) (N * j + i));

						// This is nice.
						if (BitConverter.IsLittleEndian)
							Array.Reverse (T);

						A = Xor (T, MSB(B));
						R [i] = LSB (B);
					}
				}

				// 4. Output the results:
				//       Set C(0) = A
				//       For i = 1 to N
				//          C(i) = R(i)
				Buffer.BlockCopy (A, 0, C, 0, 8);
				for (int i = 1; i <= N; i += 1)
					Buffer.BlockCopy (R [i], 0, C, 8 * i, 8);
			}
			return C;
		}

		public static byte[] AESKeyWrapDecrypt (byte[] rgbKey, byte[] rgbEncryptedWrappedKeyData)
		{
			SymmetricAlgorithm symAlg = SymmetricAlgorithm.Create ("Rijndael");
			symAlg.Mode = CipherMode.ECB;
			symAlg.Key = rgbKey;

			int N = ( rgbEncryptedWrappedKeyData.Length / 8 ) - 1;

			// From RFC 3394 - Advanced Encryption Standard (AES) Key Wrap Algorithm
			//
			// Inputs: Ciphertext, (n+1) 64-bit values (C0, C1, ..., Cn), and Key, K (the KEK)
			// Outputs: Plaintext, n 64-bit values (P1, P2, ..., Pn)
			//
			// 1. Initialize variables.
			//    Set A = C[0] 

			byte[] A = new byte [8];
			Buffer.BlockCopy (rgbEncryptedWrappedKeyData, 0, A, 0, 8);

			//    For i = 1 to n
			//    R[i] = C[i]

			byte[] R = new byte [N * 8];
			Buffer.BlockCopy (rgbEncryptedWrappedKeyData, 8, R, 0, rgbEncryptedWrappedKeyData.Length - 8);

			// 2. Compute intermediate values.
			//    For j = 5 to 0
			//       For i = n to 1
			//          B = AES-1(K, (A^t) | R[i]) where t = n*j+i
			//          A = MSB (64,B)
			//          R[i] = LSB (64,B)

			ICryptoTransform transform = symAlg.CreateDecryptor ();

			for (int j = 5; j >= 0; j -= 1) {
				for (int i = N; i >= 1; i -= 1) {
					byte[] T = BitConverter.GetBytes ((long) N * j + i);
					if (BitConverter.IsLittleEndian)
						Array.Reverse (T);

					byte[] B = new Byte [16];
					byte[] r = new Byte [8];
					Buffer.BlockCopy (R, 8 * (i - 1), r, 0, 8);
					byte[] ciphertext = Concatenate (Xor (A, T), r);
					transform.TransformBlock (ciphertext, 0, 16, B, 0);
					A = MSB (B);
					Buffer.BlockCopy (LSB (B), 0, R, 8 * (i - 1), 8);
				}
			}

			// 3. Output results
			//    If A is an appropriate initial value
			//    Then
			//       For i = 1 to n
			//          P[i] = R[i]
			//    Else
			//       Return an error

			return R;
		}

		public static byte[] TripleDESKeyWrapEncrypt (byte[] rgbKey, byte[] rgbWrappedKeyData)
		{
			SymmetricAlgorithm symAlg = SymmetricAlgorithm.Create ("TripleDES");

			// Algorithm from http://www.w3.org/TR/xmlenc-core/#sec-Alg-SymmetricKeyWrap
			// The following algorithm wraps (encrypts) a key (the wrapped key, WK) under a TRIPLEDES
			// key-encryption-key (KEK) as adopted from [CMS-Algorithms].

			// 1. Represent the key being wrapped as an octet sequence. If it is a TRIPLEDES key, 
			//    this is 24 octets (192 bits) with odd parity bit as the bottom bit of each octet.

			// rgbWrappedKeyData is the key being wrapped.

			// 2. Compute the CMS key checksum (Section 5.6.1) call this CKS.

			byte[] cks = ComputeCMSKeyChecksum (rgbWrappedKeyData);

			// 3. Let WKCKS = WK || CKS, where || is concatenation.

			byte[] wkcks = Concatenate (rgbWrappedKeyData, cks);

			// 4. Generate 8 random octets and call this IV.
			symAlg.GenerateIV ();

			// 5. Encrypt WKCKS in CBC mode using KEK as the key and IV as the initialization vector.
			//    Call the results TEMP1.

			symAlg.Mode = CipherMode.CBC;
			symAlg.Padding = PaddingMode.None;
			symAlg.Key = rgbKey;
			byte[] temp1 = Transform (wkcks, symAlg.CreateEncryptor ());

			// 6. Let TEMP2 = IV || TEMP1.

			byte[] temp2 = Concatenate (symAlg.IV, temp1);

			// 7. Reverse the order of the octets in TEMP2 and call the result TEMP3.

			Array.Reverse (temp2); // TEMP3 is TEMP2

			// 8. Encrypt TEMP3 in CBC mode using the KEK and an initialization vector of 0x4adda22c79e82105. 
			//    The resulting cipher text is the desired result.  It is 40 octets long if a 168 bit key
			//    is being wrapped.

			symAlg.IV = new Byte [8] {0x4a, 0xdd, 0xa2, 0x2c, 0x79, 0xe8, 0x21, 0x05};

			byte[] rtnval = Transform (temp2, symAlg.CreateEncryptor ());

			return rtnval;
		}

		public static byte[] TripleDESKeyWrapDecrypt (byte[] rgbKey, byte[] rgbEncryptedWrappedKeyData)
		{
			SymmetricAlgorithm symAlg = SymmetricAlgorithm.Create ("TripleDES");

			// Algorithm from http://www.w3.org/TR/xmlenc-core/#sec-Alg-SymmetricKeyWrap
			// The following algorithm unwraps (decrypts) a key as adopted from [CMS-Algorithms].

			// 1. Check the length of the cipher text is reasonable given the key type.  It must be
			//    40 bytes for a 168 bit key and either 32, 40, or 48 bytes for a 128, 192, or 256 bit
			//    key. If the length is not supported or inconsistent with the algorithm for which the
			//    key is intended, return error.

			// 2. Decrypt the cipher text with TRIPLEDES in CBC mode using the KEK and an initialization
			//    vector (IV) of 0x4adda22c79e82105.  Call the output TEMP3.

			symAlg.Mode = CipherMode.CBC;
			symAlg.Padding = PaddingMode.None;
			symAlg.Key = rgbKey;
			symAlg.IV = new Byte [8] {0x4a, 0xdd, 0xa2, 0x2c, 0x79, 0xe8, 0x21, 0x05};

			byte[] temp3 = Transform (rgbEncryptedWrappedKeyData, symAlg.CreateDecryptor ());

			// 3. Reverse the order of the octets in TEMP3 and call the result TEMP2.

			Array.Reverse (temp3); // TEMP2 is TEMP3.

			// 4. Decompose TEMP2 into IV, the first 8 octets, and TEMP1, the remaining octets.

			byte[] temp1 = new Byte [temp3.Length - 8];
			byte[] iv = new Byte [8];

			Buffer.BlockCopy (temp3, 0, iv, 0, 8);
			Buffer.BlockCopy (temp3, 8, temp1, 0, temp1.Length);

			// 5. Decrypt TEMP1 using TRIPLEDES in CBC mode using the KEK and the IV found in the previous step.
			//    Call the result WKCKS.

			symAlg.IV = iv;
			byte[] wkcks = Transform (temp1, symAlg.CreateDecryptor ());

			// 6. Decompose WKCKS.  CKS is the last 8 octets and WK, the wrapped key, are those octets before
			//    the CKS.

			byte[] cks = new byte [8];
			byte[] wk = new byte [wkcks.Length - 8];

			Buffer.BlockCopy (wkcks, 0, wk, 0, wk.Length);
			Buffer.BlockCopy (wkcks, wk.Length, cks, 0, 8);

			// 7. Calculate the CMS key checksum over the WK and compare with the CKS extracted in the above
			//    step. If they are not equal, return error.

			// 8. WK is the wrapped key, now extracted for use in data decryption.
			return wk;
		}

		private static byte[] Transform (byte[] data, ICryptoTransform t)
		{
			MemoryStream output = new MemoryStream ();
			CryptoStream crypto = new CryptoStream (output, t, CryptoStreamMode.Write);

			crypto.Write (data, 0, data.Length);
			crypto.FlushFinalBlock ();

			byte[] result = output.ToArray ();
			
			output.Close ();
			crypto.Close ();

			return result; 
                }

		private static byte[] ComputeCMSKeyChecksum (byte[] data)
		{
			byte[] hash = HashAlgorithm.Create ("SHA1").ComputeHash (data);
			byte[] output = new byte [8];

			Buffer.BlockCopy (hash, 0, output, 0, 8);

			return output;
		}

		private static byte[] Concatenate (byte[] buf1, byte[] buf2)
		{
			byte[] output = new byte [buf1.Length + buf2.Length];
			Buffer.BlockCopy (buf1, 0, output, 0, buf1.Length);
			Buffer.BlockCopy (buf2, 0, output, buf1.Length, buf2.Length);
			return output;
		}

		private static byte[] MSB (byte[] input)
		{
			return MSB (input, 8);
		}

		private static byte[] MSB (byte[] input, int bytes)
		{
			byte[] output = new byte [bytes];
			Buffer.BlockCopy (input, 0, output, 0, bytes);
			return output;
		}

		private static byte[] LSB (byte[] input)
		{
			return LSB (input, 8);
		}

		private static byte[] LSB (byte[] input, int bytes)
		{
			byte[] output = new byte [bytes];
			Buffer.BlockCopy (input, bytes, output, 0, bytes);
			return output;
		}

		private static byte[] Xor (byte[] x, byte[] y)
		{
			// This should *not* happen.
			if (x.Length != y.Length)
				throw new CryptographicException ("Error performing Xor: arrays different length.");

			byte[] output = new byte [x.Length];
			for (int i = 0; i < x.Length; i += 1)
				output [i] = (byte) (x [i] ^ y [i]);
			return output;
		}

		private static byte[] Xor (byte[] x, int n)
		{
			byte[] output = new Byte [x.Length];
			for (int i = 0; i < x.Length; i += 1)
				output [i] = (byte) ((int) x [i] ^ n);
			return output;
		}
	}
}

#endif
