//
// System.Reflection.StrongNameKeyPair.cs
//
// Authors:
//	Kevin Winchester (kwin@ns.sympatico.ca)
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2002 Kevin Winchester
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

using System.IO;
using System.Security.Cryptography;

using Mono.Security;
using Mono.Security.Cryptography;

namespace System.Reflection {

[Serializable]
public class StrongNameKeyPair 
{		
	private byte[] publicKey;
	private int bitLen;
	private RSA rsa;

	public StrongNameKeyPair (byte[] keyPairArray) 
	{
		if (keyPairArray == null)
			throw new ArgumentNullException ("keyPairArray");

		LoadKey (keyPairArray);
	}
	
	public StrongNameKeyPair (FileStream keyPairFile) 
	{
		if (keyPairFile == null)
			throw new ArgumentNullException ("keyPairFile");

		byte[] input = new byte [keyPairFile.Length];
		keyPairFile.Read (input, 0, input.Length);
		LoadKey (input);
	}
	
	public StrongNameKeyPair (string keyPairContainer) 
	{
		// named key container
		if (keyPairContainer == null)
			throw new ArgumentNullException ("keyPairContainer");

		CspParameters csp = new CspParameters ();
		csp.KeyContainerName = keyPairContainer;
		rsa = new RSACryptoServiceProvider (csp);
	}

	private void LoadKey (byte[] key) 
	{
		try {
			rsa = CryptoConvert.FromCapiKeyBlob (key);
		}
		catch
		{
			// exception is thrown when getting PublicKey
			// to match MS implementation
		}
	}

	public byte[] PublicKey {
		get { 
			if (rsa == null)
				throw new ArgumentException ("invalid keypair");

			if (publicKey == null) {
				byte[] blob = CryptoConvert.ToCapiKeyBlob (rsa, false);
				publicKey = new byte [blob.Length + 12];
				// The first 12 bytes are documented at:
				// http://msdn.microsoft.com/library/en-us/cprefadd/html/grfungethashfromfile.asp
				// ALG_ID - Signature
				publicKey[0] = 0x00;
				publicKey[1] = 0x24;	
				publicKey[2] = 0x00;	
				publicKey[3] = 0x00;	
				// ALG_ID - Hash
				publicKey[4] = 0x04;
				publicKey[5] = 0x80;
				publicKey[6] = 0x00;
				publicKey[7] = 0x00;
				// Length of Public Key (in bytes)
				int lastPart = blob.Length;
				publicKey[8] = (byte)(lastPart % 256);
				publicKey[9] = (byte)(lastPart / 256); // just in case
				publicKey[10] = 0x00;
				publicKey[11] = 0x00;

				Buffer.BlockCopy (blob, 0, publicKey, 12, blob.Length);
			}
			return publicKey;
		}
	}

	internal StrongName StrongName () 
	{
		return new StrongName (rsa);
	}
}

}
