//
// System.Reflection.StrongNameKeyPair.cs
//
// Authors:
//	Kevin Winchester (kwin@ns.sympatico.ca)
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Kevin Winchester
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System.IO;

namespace System.Reflection {

[Serializable]
public class StrongNameKeyPair 
{		
	private byte[] keyPair;
	private byte[] publicKey;
	private int bitLen;
	private bool keypairValid;

	public StrongNameKeyPair (byte[] keyPairArray) 
	{
		if (keyPairArray == null)
			throw new ArgumentNullException ("keyPairArray");
		keypairValid = Validate (keyPairArray);
	}
	
	public StrongNameKeyPair (FileStream keyPairFile) 
	{
		if (keyPairFile == null)
			throw new ArgumentNullException ("keyPairFile");
		byte[] input = new byte [keyPairFile.Length];
		keyPairFile.Read (input, 0, input.Length);
		keypairValid = Validate (input);
	}
	
	[MonoTODO("We do not, yet, support keypair persistance")]
	public StrongNameKeyPair (string keyPairContainer) 
	{
		// named key container
		if (keyPairContainer == null)
			throw new ArgumentNullException ("keyPairContainer");
		// only RSA ? or both RSA and DSA ?
		throw new NotImplementedException ();
	}

	private bool Validate (byte[] keypair) {
		// Type - PRIVATEKEYBLOB (0x07)
		if (keypair[0] != 0x07)
			return false;
		// Version - Always CUR_BLOB_VERSION (0x02)
		if (keypair[1] != 0x02)
			return false;
		// RESERVED - Always 0
		if ((keypair[2] != 0x00) || (keypair[3] != 0x00))
			return false;
		// ALGID - Always 00 24 00 00 (for CALG_RSA_SIGN)
		if ((keypair[4] != 0x00) || (keypair[5] != 0x24) || (keypair[6] != 0x00) || (keypair[7] != 0x00))
			return false;
		// Magic - RSA2 (ASCII in hex)
		if ((keypair[8] != 0x52) || (keypair[9] != 0x53) || (keypair[10] != 0x41) || (keypair[11] != 0x32))
			return false;
		// bitlen - ex: 1024 - must be a multiple of 8
		bitLen = (keypair[15] << 24) + (keypair[14] << 16) + (keypair[13] << 8) + keypair[12];
		if (bitLen % 8 != 0)
			return false;
		// public exponent (DWORD)
		// modulus
		// private key
		keyPair = keypair;
		return true;
	}

	public byte[] PublicKey {
		get { 
			if (!keypairValid)
				throw new ArgumentException ("invalid keypair");
			// first call (will be cached for all subsequent calls)
			if (publicKey == null) {
				publicKey = new byte [(bitLen >> 3) + 32];
				// The first 12 bytes are documented at:
				// http://msdn.microsoft.com/library/en-us/cprefadd/html/grfungethashfromfile.asp
				// ALG_ID - Signature
				publicKey[0] = keyPair[4];
				publicKey[1] = keyPair[5];	
				publicKey[2] = keyPair[6];	
				publicKey[3] = keyPair[7];	
				// ALG_ID - Hash
				publicKey[4] = 0x04;
				publicKey[5] = 0x80;
				publicKey[6] = 0x00;
				publicKey[7] = 0x00;
				// Length of Public Key (in bytes)
				int lastPart = publicKey.Length - 12;
				publicKey[8] = (byte)(lastPart % 256);
				publicKey[9] = (byte)(lastPart / 256); // just in case
				publicKey[10] = 0x00;
				publicKey[11] = 0x00;
				// Ok from here - Same structure as keypair - expect for public key
				publicKey[12] = 0x06;		// PUBLICKEYBLOB
				// we can copy this part
				Array.Copy (keyPair, 1, publicKey, 13, publicKey.Length - 13);
				// and make a small adjustment 
				publicKey[23] = 0x31;		// (RSA1 not RSA2)
			}
			return publicKey; 
		}
	}
}

}
