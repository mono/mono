//
// CryptoConvert.cs - Crypto Convertion Routines
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography;

namespace Mono.Security.Cryptography {

	public class CryptoConvert {

		static private byte[] Trim (byte[] array) 
		{
			for (int i=0; i < array.Length; i++) {
				if (array [i] != 0x00) {
					byte[] result = new byte [array.Length - i];
					Array.Copy (array, i, result, 0, result.Length);
					return result;
				}
			}
			return null;
		}

		// convert the key from PRIVATEKEYBLOB to RSA
		// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/security/Security/private_key_blobs.asp
		// e.g. SNK files, PVK files
		static public RSA FromCapiPrivateKeyBlob (byte[] blob) 
		{
			// PRIVATEKEYBLOB (0x07)
			// Version (0x02)
			if ((blob [0] != 0x07) || (blob [1] != 0x02))
				return null;
			// Reserved (word)
			if ((blob [2] != 0x00) || (blob [3] != 0x00))
				return null;
			// ALGID (CALG_RSA_SIGN, CALG_RSA_KEYX, ...)
			int algId = BitConverter.ToInt32 (blob, 4);
			// DWORD magic = RSA2
			if (BitConverter.ToUInt32 (blob, 8) != 0x32415352)
				return null;
			// DWORD bitlen
			int bitLen = BitConverter.ToInt32 (blob, 12);

			// DWORD public exponent
			RSAParameters rsap = new RSAParameters ();
			byte[] exp = new byte [4];
			Array.Copy (blob, 16, exp, 0, 4);
			Array.Reverse (exp);
			rsap.Exponent = Trim (exp);
		
			int pos = 20;
			// BYTE modulus[rsapubkey.bitlen/8];
			int byteLen = (bitLen >> 3);
			rsap.Modulus = new byte [byteLen];
			Array.Copy (blob, pos, rsap.Modulus, 0, byteLen);
			Array.Reverse (rsap.Modulus);
			pos += byteLen;

			// BYTE prime1[rsapubkey.bitlen/16];
			int byteHalfLen = (byteLen >> 1);
			rsap.P = new byte [byteHalfLen];
			Array.Copy (blob, pos, rsap.P, 0, byteHalfLen);
			Array.Reverse (rsap.P);
			pos += byteHalfLen;

			// BYTE prime2[rsapubkey.bitlen/16];
			rsap.Q = new byte [byteHalfLen];
			Array.Copy (blob, pos, rsap.Q, 0, byteHalfLen);
			Array.Reverse (rsap.Q);
			pos += byteHalfLen;

			// BYTE exponent1[rsapubkey.bitlen/16];
			rsap.DP = new byte [byteHalfLen];
			Array.Copy (blob, pos, rsap.DP, 0, byteHalfLen);
			Array.Reverse (rsap.DP);
			pos += byteHalfLen;

			// BYTE exponent2[rsapubkey.bitlen/16];
			rsap.DQ = new byte [byteHalfLen];
			Array.Copy (blob, pos, rsap.DQ, 0, byteHalfLen);
			Array.Reverse (rsap.DQ);
			pos += byteHalfLen;

			// BYTE coefficient[rsapubkey.bitlen/16];
			rsap.InverseQ = new byte [byteHalfLen];
			Array.Copy (blob, pos, rsap.InverseQ, 0, byteHalfLen);
			Array.Reverse (rsap.InverseQ);
			pos += byteHalfLen;

			// BYTE privateExponent[rsapubkey.bitlen/8];
			rsap.D = new byte [byteLen];
			Array.Copy (blob, pos, rsap.D, 0, byteLen);
			Array.Reverse (rsap.D);

			RSA rsa = RSA.Create ();
			try {
				rsa.ImportParameters (rsap);
			}
			catch {
				rsa = null;
			}
			return rsa;
		}

		static public byte[] ToCapiPrivateKeyBlob (RSA rsa) 
		{
			RSAParameters p = rsa.ExportParameters (true);
			int keyLength = p.Modulus.Length; // in bytes
			byte[] blob = new byte [20 + (keyLength << 2) + (keyLength >> 1)];

			blob [0] = 0x07;	// Type - PRIVATEKEYBLOB (0x07)
			blob [1] = 0x02;	// Version - Always CUR_BLOB_VERSION (0x02)
			// [2], [3]		// RESERVED - Always 0
			blob [5] = 0x24;	// ALGID - Always 00 24 00 00 (for CALG_RSA_SIGN)
			blob [8] = 0x52;	// Magic - RSA2 (ASCII in hex)
			blob [9] = 0x53;
			blob [10] = 0x41;
			blob [11] = 0x32;

			byte[] bitlen = BitConverter.GetBytes (keyLength << 3);
			blob [12] = bitlen [0];	// bitlen
			blob [13] = bitlen [1];	
			blob [14] = bitlen [2];	
			blob [15] = bitlen [3];

			// public exponent (DWORD)
			int pos = 16;
			int n = p.Exponent.Length;
			while (n > 0)
				blob [pos++] = p.Exponent [--n];
			// modulus
			pos = 20;
			byte[] part = p.Modulus;
			int len = part.Length;
			Array.Reverse (part, 0, len);
			Array.Copy (part, 0, blob, pos, len);
			pos += len;
			// private key
			part = p.P;
			len = part.Length;
			Array.Reverse (part, 0, len);
			Array.Copy (part, 0, blob, pos, len);
			pos += len;

			part = p.Q;
			len = part.Length;
			Array.Reverse (part, 0, len);
			Array.Copy (part, 0, blob, pos, len);
			pos += len;

			part = p.DP;
			len = part.Length;
			Array.Reverse (part, 0, len);
			Array.Copy (part, 0, blob, pos, len);
			pos += len;

			part = p.DQ;
			len = part.Length;
			Array.Reverse (part, 0, len);
			Array.Copy (part, 0, blob, pos, len);
			pos += len;

			part = p.InverseQ;
			len = part.Length;
			Array.Reverse (part, 0, len);
			Array.Copy (part, 0, blob, pos, len);
			pos += len;

			part = p.D;
			len = part.Length;
			Array.Reverse (part, 0, len);
			Array.Copy (part, 0, blob, pos, len);

			return blob;
		}

		static public RSA FromCapiPublicKeyBlob (byte[] blob) 
		{
			// PUBLICKEYBLOB (0x06)
			// Version (0x02)
			if ((blob [0] != 0x06) || (blob [1] != 0x02))
				return null;
			// Reserved (word)
			if ((blob [2] != 0x00) || (blob [3] != 0x00))
				return null;
			// ALGID (CALG_RSA_SIGN, CALG_RSA_KEYX, ...)
			int algId = BitConverter.ToInt32 (blob, 4);
			// DWORD magic = RSA2
			if (BitConverter.ToUInt32 (blob, 8) != 0x32415352)
				return null;
			// DWORD bitlen
			int bitLen = BitConverter.ToInt32 (blob, 12);

			// DWORD public exponent
			RSAParameters rsap = new RSAParameters ();
			rsap.Exponent = new byte [3];
			rsap.Exponent [0] = blob [18];
			rsap.Exponent [1] = blob [17];
			rsap.Exponent [2] = blob [16];
		
			int pos = 20;
			// BYTE modulus[rsapubkey.bitlen/8];
			int byteLen = (bitLen >> 3);
			rsap.Modulus = new byte [byteLen];
			Array.Copy (blob, pos, rsap.Modulus, 0, byteLen);
			Array.Reverse (rsap.Modulus);

			RSA rsa = RSA.Create ();
			try {
				rsa.ImportParameters (rsap);
			}
			catch {
				rsa = null;
			}
			return rsa;
		}

		static public byte[] ToCapiPublicKeyBlob (RSA rsa) 
		{
			RSAParameters p = rsa.ExportParameters (false);
			int keyLength = p.Modulus.Length; // in bytes
			byte[] blob = new byte [20 + keyLength];

			blob [0] = 0x06;	// Type - PUBLICKEYBLOB (0x06)
			blob [1] = 0x02;	// Version - Always CUR_BLOB_VERSION (0x02)
			// [2], [3]		// RESERVED - Always 0
			blob [5] = 0x24;	// ALGID - Always 00 24 00 00 (for CALG_RSA_SIGN)
			blob [8] = 0x52;	// Magic - RSA2 (ASCII in hex)
			blob [9] = 0x53;
			blob [10] = 0x41;
			blob [11] = 0x32;

			byte[] bitlen = BitConverter.GetBytes (keyLength << 3);
			blob [12] = bitlen [0];	// bitlen
			blob [13] = bitlen [1];	
			blob [14] = bitlen [2];	
			blob [15] = bitlen [3];

			// public exponent (DWORD)
			blob [16] = p.Exponent [2];
			blob [17] = p.Exponent [1];
			blob [18] = p.Exponent [0];
			blob [19] = 0x00;
			// modulus
			int pos = 20;
			byte[] part = p.Modulus;
			int len = part.Length;
			Array.Reverse (part, 0, len);
			Array.Copy (part, 0, blob, pos, len);
			pos += len;
			return blob;
		}

		// PRIVATEKEYBLOB
		// PUBLICKEYBLOB
		static public RSA FromCapiKeyBlob (byte[] blob) 
		{
			switch (blob [0]) {
				case 0x06:
					return FromCapiPublicKeyBlob (blob);
				case 0x07:
					return FromCapiPrivateKeyBlob (blob);
				default:
					return null;
			}
		}

		static public byte[] ToCapiKeyBlob (AsymmetricAlgorithm keypair, bool includePrivateKey) 
		{
			// check between RSA and DSA
			if (keypair is RSA)
				return ToCapiKeyBlob ((RSA)keypair, includePrivateKey);
			else
				return null;	// TODO
		}

		static public byte[] ToCapiKeyBlob (RSA rsa, bool includePrivateKey) 
		{
			RSAParameters p = rsa.ExportParameters (includePrivateKey);
			if (includePrivateKey)
				return ToCapiPrivateKeyBlob (rsa);
			else
				return ToCapiPublicKeyBlob (rsa);
		}
	}
}
