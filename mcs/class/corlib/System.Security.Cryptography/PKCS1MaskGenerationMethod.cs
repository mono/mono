//
// PKCS1MaskGenerationMethod.cs: Handles PKCS#1 mask generation.
//
// Author:
//		Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography;

namespace System.Security.Cryptography
{

// PKCS#1: RSA Cryptography Standard 
// http://www.rsasecurity.com/rsalabs/pkcs/pkcs-1/index.html
public class PKCS1MaskGenerationMethod : MaskGenerationMethod
{
	private string hashName;

	public PKCS1MaskGenerationMethod()
	{
		hashName = "SHA1";
	}

	public string HashName 
	{
		get { return hashName; }
		set { hashName = value; }
	}

	// I2OSP converts a nonnegative integer to an octet string of a specified length.
	// in this case xLen is always 4 so we simplify the function 
	protected byte[] I2OSP( uint x )
	{
		byte[] array = BitConverter.GetBytes( x );
		Array.Reverse( array ); // big-little endian issues
		return array;
	}

	// from MGF1 on page 48 from PKCS#1 v2.1 (pdf version)
	public override byte[] GenerateMask( byte[] mgfSeed, int maskLen )
	{
		// 1. If maskLen > 2^32 hLen, output “mask too long” and stop.
		// easy - this is impossible by using a int (32bits) as parameter ;-)

		int mgfSeedLength = mgfSeed.Length;
		HashAlgorithm hash = HashAlgorithm.Create( hashName );
		int hLen = ( hash.HashSize >> 3 ); // from bits to bytes
		int iterations = ( maskLen / hLen );
		if ( maskLen % hLen != 0 )
			iterations++;
		// 2. Let T be the empty octet string.
		byte[] T = new byte[ ( iterations * hLen ) ];

		byte[] toBeHashed = new byte[ mgfSeedLength + 4 ];
		int pos = 0;
		// 3. For counter from 0 to ( maskLen / hLen ) – 1, do the following:
		for ( uint counter = 0; counter < iterations; counter++ ) 
		{
			// a. Convert counter to an octet string C of length 4 octets
			//    C = I2OSP (counter, 4)
			byte[] C = I2OSP( counter );

			// b. Concatenate the hash of the seed mgfSeed and C to the octet string T:
			//	   T = T || Hash (mgfSeed || C)
			Array.Copy( mgfSeed, 0, toBeHashed, 0, mgfSeedLength );
			Array.Copy( C, 0, toBeHashed, mgfSeedLength, 4 );
			byte[] output = hash.ComputeHash( toBeHashed );
			Array.Copy( output, 0, T, pos, hLen );
			pos += mgfSeedLength;
		}
		
		// 4. Output the leading maskLen octets of T as the octet string mask.
		byte[] mask = new byte[ maskLen ];
		Array.Copy( T, 0, mask, 0, maskLen );
		return mask;
	}
}

}
