//
// TripleDESCryptoServiceProvider.cs: Default TripleDES implementation
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace System.Security.Cryptography {

// References:
// a.	FIPS PUB 46-3: TripleDES
//	http://csrc.nist.gov/publications/fips/fips46-3/fips46-3.pdf
// b.	ANSI X9.52
//	not free :-(
//	http://webstore.ansi.org/ansidocstore/product.asp?sku=ANSI+X9%2E52%2D1998

public sealed class TripleDESCryptoServiceProvider : TripleDES {

	public TripleDESCryptoServiceProvider () {}

	public override void GenerateIV () 
	{
		IVValue = KeyBuilder.IV (BlockSizeValue >> 3);
	}
	
	public override void GenerateKey () 
	{
		KeyValue = KeyBuilder.Key (KeySizeValue >> 3);
	}
	
	public override ICryptoTransform CreateDecryptor (byte[] rgbKey, byte[] rgbIV) 
	{
		Key = rgbKey;
		IV = rgbIV;
		return new TripleDESTransform (this, false, rgbKey, rgbIV);
	}
	
	public override ICryptoTransform CreateEncryptor (byte[] rgbKey, byte[] rgbIV) 
	{
		Key = rgbKey;
		IV = rgbIV;
		return new TripleDESTransform (this, true, rgbKey, rgbIV);
	}
}

// TripleDES is just DES-EDE
internal class TripleDESTransform : SymmetricTransform {

	// for encryption
	private DESTransform E1;
	private DESTransform D2;
	private DESTransform E3;

	// for decryption
	private DESTransform D1;
	private DESTransform E2;
	private DESTransform D3;
	
	public TripleDESTransform (TripleDES algo, bool encryption, byte[] key, byte[] iv) : base (algo, encryption, iv) 
	{
		byte[] key1 = new byte [8];
		byte[] key2 = new byte [8];
		byte[] key3 = new byte [8];
		DES des = DES.Create ();
		Array.Copy (key, 0, key1, 0, 8);
		Array.Copy (key, 8, key2, 0, 8);
		if (key.Length == 16)
			Array.Copy (key, 0, key3, 0, 8);
		else
			Array.Copy (key, 16, key3, 0, 8);

		// note: some modes (like CFB) requires encryption when decrypting
		if ((encryption) || (algo.Mode == CipherMode.CFB)) {
			E1 = new DESTransform (des, true, key1, iv);
			D2 = new DESTransform (des, false, key2, iv);
			E3 = new DESTransform (des, true, key3, iv);
		}
		else {
			D1 = new DESTransform (des, false, key3, iv);
			E2 = new DESTransform (des, true, key2, iv);
			D3 = new DESTransform (des, false, key1, iv);
		}
	}

	// note: this method is garanteed to be called with a valid blocksize
	// for both input and output
	protected override void ECB (byte[] input, byte[] output) 
	{
		byte[] temp = new byte [input.Length];
		if (encrypt) {
			E1.ProcessBlock (input, output);
			D2.ProcessBlock (output, temp);
			E3.ProcessBlock (temp, output);
		}
		else {
			D1.ProcessBlock (input, output);
			E2.ProcessBlock (output, temp);
			D3.ProcessBlock (temp, output);
		}
	}
}

}
