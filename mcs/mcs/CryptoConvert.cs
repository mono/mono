//
// CryptoConvert.cs - Crypto Convertion Routines
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Mono.Security.Cryptography {

#if INSIDE_CORLIB
	internal
#else
	public
#endif
	sealed class CryptoConvert {

		private CryptoConvert () 
		{
		}

		static private int ToInt32LE (byte [] bytes, int offset)
		{
			return (bytes [offset+3] << 24) | (bytes [offset+2] << 16) | (bytes [offset+1] << 8) | bytes [offset];
		}

		static private uint ToUInt32LE (byte [] bytes, int offset)
		{
			return (uint)((bytes [offset+3] << 24) | (bytes [offset+2] << 16) | (bytes [offset+1] << 8) | bytes [offset]);
		}

		static private byte [] GetBytesLE (int val)
		{
			return new byte [] { 
				(byte) (val & 0xff), 
				(byte) ((val >> 8) & 0xff), 
				(byte) ((val >> 16) & 0xff), 
				(byte) ((val >> 24) & 0xff)
			};
                }

		static private byte[] Trim (byte[] array) 
		{
			for (int i=0; i < array.Length; i++) {
				if (array [i] != 0x00) {
					byte[] result = new byte [array.Length - i];
					Buffer.BlockCopy (array, i, result, 0, result.Length);
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
			return FromCapiPrivateKeyBlob (blob, 0);
		}

		static public RSA FromCapiPrivateKeyBlob (byte[] blob, int offset) 
		{
			if (blob == null)
				throw new ArgumentNullException ("blob");
			if (offset >= blob.Length)
				throw new ArgumentException ("blob is too small.");

			try {
				if ((blob [offset]   != 0x07) ||				// PRIVATEKEYBLOB (0x07)
				    (blob [offset+1] != 0x02) ||				// Version (0x02)
				    (blob [offset+2] != 0x00) ||				// Reserved (word)
				    (blob [offset+3] != 0x00) ||
				    (ToUInt32LE (blob, offset+8) != 0x32415352))	// DWORD magic = RSA2
					throw new CryptographicException ("Invalid blob header");
				
				// ALGID (CALG_RSA_SIGN, CALG_RSA_KEYX, ...)
				int algId = ToInt32LE (blob, offset+4);

				// DWORD bitlen
				int bitLen = ToInt32LE (blob, offset+12);

				// DWORD public exponent
				RSAParameters rsap = new RSAParameters ();
				byte[] exp = new byte [4];
				Buffer.BlockCopy (blob, offset+16, exp, 0, 4);
				Array.Reverse (exp);
				rsap.Exponent = Trim (exp);
			
				int pos = offset+20;
				// BYTE modulus[rsapubkey.bitlen/8];
				int byteLen = (bitLen >> 3);
				rsap.Modulus = new byte [byteLen];
				Buffer.BlockCopy (blob, pos, rsap.Modulus, 0, byteLen);
				Array.Reverse (rsap.Modulus);
				pos += byteLen;

				// BYTE prime1[rsapubkey.bitlen/16];
				int byteHalfLen = (byteLen >> 1);
				rsap.P = new byte [byteHalfLen];
				Buffer.BlockCopy (blob, pos, rsap.P, 0, byteHalfLen);
				Array.Reverse (rsap.P);
				pos += byteHalfLen;

				// BYTE prime2[rsapubkey.bitlen/16];
				rsap.Q = new byte [byteHalfLen];
				Buffer.BlockCopy (blob, pos, rsap.Q, 0, byteHalfLen);
				Array.Reverse (rsap.Q);
				pos += byteHalfLen;

				// BYTE exponent1[rsapubkey.bitlen/16];
				rsap.DP = new byte [byteHalfLen];
				Buffer.BlockCopy (blob, pos, rsap.DP, 0, byteHalfLen);
				Array.Reverse (rsap.DP);
				pos += byteHalfLen;

				// BYTE exponent2[rsapubkey.bitlen/16];
				rsap.DQ = new byte [byteHalfLen];
				Buffer.BlockCopy (blob, pos, rsap.DQ, 0, byteHalfLen);
				Array.Reverse (rsap.DQ);
				pos += byteHalfLen;

				// BYTE coefficient[rsapubkey.bitlen/16];
				rsap.InverseQ = new byte [byteHalfLen];
				Buffer.BlockCopy (blob, pos, rsap.InverseQ, 0, byteHalfLen);
				Array.Reverse (rsap.InverseQ);
				pos += byteHalfLen;

				// ok, this is hackish but CryptoAPI support it so...
				// note: only works because CRT is used by default
				// http://bugzilla.ximian.com/show_bug.cgi?id=57941
				rsap.D = new byte [byteLen]; // must be allocated
				if (pos + byteLen + offset <= blob.Length) {
					// BYTE privateExponent[rsapubkey.bitlen/8];
					Buffer.BlockCopy (blob, pos, rsap.D, 0, byteLen);
					Array.Reverse (rsap.D);
				}

				RSA rsa = (RSA)RSA.Create ();
				rsa.ImportParameters (rsap);
				return rsa;
			}
			catch (Exception e) {
				throw new CryptographicException ("Invalid blob.", e);
			}
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

			byte[] bitlen = GetBytesLE (keyLength << 3);
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
			Buffer.BlockCopy (part, 0, blob, pos, len);
			pos += len;
			// private key
			part = p.P;
			len = part.Length;
			Array.Reverse (part, 0, len);
			Buffer.BlockCopy (part, 0, blob, pos, len);
			pos += len;

			part = p.Q;
			len = part.Length;
			Array.Reverse (part, 0, len);
			Buffer.BlockCopy (part, 0, blob, pos, len);
			pos += len;

			part = p.DP;
			len = part.Length;
			Array.Reverse (part, 0, len);
			Buffer.BlockCopy (part, 0, blob, pos, len);
			pos += len;

			part = p.DQ;
			len = part.Length;
			Array.Reverse (part, 0, len);
			Buffer.BlockCopy (part, 0, blob, pos, len);
			pos += len;

			part = p.InverseQ;
			len = part.Length;
			Array.Reverse (part, 0, len);
			Buffer.BlockCopy (part, 0, blob, pos, len);
			pos += len;

			part = p.D;
			len = part.Length;
			Array.Reverse (part, 0, len);
			Buffer.BlockCopy (part, 0, blob, pos, len);

			return blob;
		}

		static public RSA FromCapiPublicKeyBlob (byte[] blob) 
		{
			return FromCapiPublicKeyBlob (blob, 0);
		}

		static public RSA FromCapiPublicKeyBlob (byte[] blob, int offset) 
		{
			if (blob == null)
				throw new ArgumentNullException ("blob");
			if (offset >= blob.Length)
				throw new ArgumentException ("blob is too small.");

			try {
				if ((blob [offset]   != 0x06) ||				// PUBLICKEYBLOB (0x06)
				    (blob [offset+1] != 0x02) ||				// Version (0x02)
				    (blob [offset+2] != 0x00) ||				// Reserved (word)
				    (blob [offset+3] != 0x00) || 
				    (ToUInt32LE (blob, offset+8) != 0x31415352))	// DWORD magic = RSA1
					throw new CryptographicException ("Invalid blob header");

				// ALGID (CALG_RSA_SIGN, CALG_RSA_KEYX, ...)
				int algId = ToInt32LE (blob, offset+4);

				// DWORD bitlen
				int bitLen = ToInt32LE (blob, offset+12);

				// DWORD public exponent
				RSAParameters rsap = new RSAParameters ();
				rsap.Exponent = new byte [3];
				rsap.Exponent [0] = blob [offset+18];
				rsap.Exponent [1] = blob [offset+17];
				rsap.Exponent [2] = blob [offset+16];
			
				int pos = offset+20;
				// BYTE modulus[rsapubkey.bitlen/8];
				int byteLen = (bitLen >> 3);
				rsap.Modulus = new byte [byteLen];
				Buffer.BlockCopy (blob, pos, rsap.Modulus, 0, byteLen);
				Array.Reverse (rsap.Modulus);

				RSA rsa = (RSA)RSA.Create ();
				rsa.ImportParameters (rsap);
				return rsa;
			}
			catch (Exception e) {
				throw new CryptographicException ("Invalid blob.", e);
			}
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
			blob [8] = 0x52;	// Magic - RSA1 (ASCII in hex)
			blob [9] = 0x53;
			blob [10] = 0x41;
			blob [11] = 0x31;

			byte[] bitlen = GetBytesLE (keyLength << 3);
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
			Buffer.BlockCopy (part, 0, blob, pos, len);
			pos += len;
			return blob;
		}

		// PRIVATEKEYBLOB
		// PUBLICKEYBLOB
		static public RSA FromCapiKeyBlob (byte[] blob) 
		{
			return FromCapiKeyBlob (blob, 0);
		}

		static public RSA FromCapiKeyBlob (byte[] blob, int offset) 
		{
			if (blob == null)
				throw new ArgumentNullException ("blob");
			if (offset >= blob.Length)
				throw new ArgumentException ("blob is too small.");

			switch (blob [offset]) {
				case 0x00:
					// this could be a public key inside an header
					// like "sn -e" would produce
					if (blob [offset + 12] == 0x06) {
						return FromCapiPublicKeyBlob (blob, offset + 12);
					}
					break;
				case 0x06:
					return FromCapiPublicKeyBlob (blob, offset);
				case 0x07:
					return FromCapiPrivateKeyBlob (blob, offset);
			}
			throw new CryptographicException ("Unknown blob format.");
		}

		static public byte[] ToCapiKeyBlob (AsymmetricAlgorithm keypair, bool includePrivateKey) 
		{
			if (keypair == null)
				throw new ArgumentNullException ("keypair");

			// check between RSA and DSA (and potentially others like DH)
			if (keypair is RSA)
				return ToCapiKeyBlob ((RSA)keypair, includePrivateKey);
			else
				return null;	// TODO
		}

		static public byte[] ToCapiKeyBlob (RSA rsa, bool includePrivateKey) 
		{
			if (rsa == null)
				throw new ArgumentNullException ("rsa");

			RSAParameters p = rsa.ExportParameters (includePrivateKey);
			if (includePrivateKey)
				return ToCapiPrivateKeyBlob (rsa);
			else
				return ToCapiPublicKeyBlob (rsa);
		}

		static public string ToHex (byte[] input) 
		{
			if (input == null)
				return null;

			StringBuilder sb = new StringBuilder (input.Length * 2);
			foreach (byte b in input) {
				sb.Append (b.ToString ("X2", CultureInfo.InvariantCulture));
			}
			return sb.ToString ();
		}

		static private byte FromHexChar (char c) 
		{
			if ((c >= 'a') && (c <= 'f'))
				return (byte) (c - 'a' + 10);
			if ((c >= 'A') && (c <= 'F'))
				return (byte) (c - 'A' + 10);
			if ((c >= '0') && (c <= '9'))
				return (byte) (c - '0');
			throw new ArgumentException ("invalid hex char");
		}

		static public byte[] FromHex (string hex) 
		{
			if (hex == null)
				return null;
			if ((hex.Length & 0x1) == 0x1)
				throw new ArgumentException ("Length must be a multiple of 2");

			byte[] result = new byte [hex.Length >> 1];
			int n = 0;
			int i = 0;
			while (n < result.Length) {
				result [n] = (byte) (FromHexChar (hex [i++]) << 4);
				result [n++] += FromHexChar (hex [i++]);
			}
			return result;
		}
	}
}
