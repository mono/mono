//
// SymmetricKeyWrap.cs - Implements symmetric key wrap algorithms
//
// Author:
//      Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004
//

#if NET_1_2

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

		[MonoTODO]
		public static byte[] AESKeyWrapDecrypt (byte[] rgbKey, byte[] rgbEncryptedWrappedKeyData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static byte[] TripleDESKeyWrapEncrypt (byte[] rgbKey, byte[] rgbWrappedKeyData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static byte[] TripleDESKeyWrapDecrypt (byte[] rgbKey, byte[] rgbEncryptedWrappedKeyData)
		{
			throw new NotImplementedException ();
		}

		private static byte[] Transform (byte[] data, ICryptoTransform t, bool flush)
		{
			MemoryStream output = new MemoryStream ();
			CryptoStream crypto = new CryptoStream (output, t, CryptoStreamMode.Write);
			crypto.Write (data, 0, data.Length);

			byte[] buf;

			if (flush) {
				crypto.Close ();
				output.Close ();
				buf = output.ToArray ();
			} else {
				buf = output.ToArray ();
				crypto.Close ();
				output.Close ();
			}

			return buf;
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
			byte[] output = new byte [8];
			Buffer.BlockCopy (input, 0, output, 0, 8);
			return output;
		}

		private static byte[] LSB (byte[] input)
		{
			byte[] output = new byte [8];
			Buffer.BlockCopy (input, 8, output, 0, 8);
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
	}
}

#endif
