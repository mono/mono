//
// System.Security.Cryptography.RC2CryptoServiceProvider.cs
//
// Authors:
//	Andrew Birkett (andy@nobugs.org)
//	Sebastien Pouliot (spouliot@motus.com)
//
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//          

using System;

namespace System.Security.Cryptography {

// References:
// a.	IETF RFC2286: A Description of the RC2(r) Encryption Algorithm
//	http://www.ietf.org/rfc/rfc2268.txt

public sealed class RC2CryptoServiceProvider : RC2 {

	public RC2CryptoServiceProvider()
	{
	}

	public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV)
	{
		Key = rgbKey;
		IV = rgbIV;
		return new RC2Transform (this, false);
	}

	public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV)
	{
		Key = rgbKey;
		IV = rgbIV;
		return new RC2Transform (this, true);
	}

	[MonoTODO]
	public override void GenerateIV()
	{
		IVValue = new byte[BlockSizeValue / 8];
		for (int i=0; i < IVValue.Length; i++) IVValue[i] = 0;
	}

	[MonoTODO]
	public override void GenerateKey()
	{
		KeyValue = new byte[KeySizeValue / 8];
		for (int i=0; i < KeyValue.Length; i++) KeyValue[i] = 0;
	}
}

	internal class RC2Transform : SymmetricTransform
	{
		public RC2Transform (RC2 rc2Algo, bool encryption) : base (rc2Algo, encryption, rc2Algo.IV)
		{
			R = new UInt32 [4];
			KeySetup (rc2Algo.Key, rc2Algo.EffectiveKeySize);
		}

		private void KeySetup (byte[] key, int t1) 
		{		
			// Expand key into a byte array, then convert to word
			// array since we always access the key in 16bit chunks.
			byte[] L = new byte [128];
	
			int t = key.Length;
			int t8 = ((t1 + 7) >> 3); // divide by 8
			int tm = 255 % (2 << (8 + t1 - 8*t8 - 1));
	
			Array.Copy (key, 0, L, 0, t);
	
			for (int i=t; i < 128; i++) 
				L [i] = (byte) (pitable [(L [i-1] + L [i-t]) & 0xff]);
	
			L [128-t8] = pitable [L [128-t8] & tm];
	
			for (int i=127-t8; i >= 0; i--) 
				L [i] = pitable [L [i+1] ^ L [i+t8]];
	
			K = new UInt32 [64];
			int pos = 0;
			for (int i=0; i < 64; i++) 
				K [i] = (UInt32) (L [pos++] + L [pos++] * 256);
		}

		protected override void ECB (byte[] input, byte[] output) 
		{
			// unrolled loop, eliminated mul
			R [0] = (UInt32) (input [0] + (input [1] << 8));
			R [1] = (UInt32) (input [2] + (input [3] << 8));
			R [2] = (UInt32) (input [4] + (input [5] << 8));
			R [3] = (UInt32) (input [6] + (input [7] << 8));
	
			if (encrypt) {
				j = 0;
				Mix(); Mix(); Mix(); Mix(); Mix();
				Mash();
				Mix(); Mix(); Mix(); Mix(); Mix(); Mix();
				Mash();
				Mix(); Mix(); Mix(); Mix(); Mix();
			} 
			else {
				j = 63;
				RMix(); RMix(); RMix(); RMix(); RMix();
				RMash();
				RMix(); RMix(); RMix(); RMix(); RMix(); RMix();
				RMash();
				RMix(); RMix(); RMix(); RMix(); RMix();
			}

			// unrolled loop
			output[0] = (byte) (R [0] & 0xff);
			output[1] = (byte) ((R [0] >> 8) & 0xff);
			output[2] = (byte) (R [1] & 0xff);
			output[3] = (byte) ((R [1] >> 8) & 0xff);
			output[4] = (byte) (R [2] & 0xff);
			output[5] = (byte) ((R [2] >> 8) & 0xff);
			output[6] = (byte) (R [3] & 0xff);
			output[7] = (byte) ((R [3] >> 8) & 0xff);
		}

		static public byte[] pitable = {
			0xd9, 0x78, 0xf9, 0xc4, 0x19, 0xdd, 0xb5, 0xed, 
			0x28, 0xe9, 0xfd, 0x79, 0x4a, 0xa0, 0xd8, 0x9d,
			0xc6, 0x7e, 0x37, 0x83, 0x2b, 0x76, 0x53, 0x8e, 
			0x62, 0x4c, 0x64, 0x88, 0x44, 0x8b, 0xfb, 0xa2,
			0x17, 0x9a, 0x59, 0xf5, 0x87, 0xb3, 0x4f, 0x13, 
			0x61, 0x45, 0x6d, 0x8d, 0x09, 0x81, 0x7d, 0x32,
			0xbd, 0x8f, 0x40, 0xeb, 0x86, 0xb7, 0x7b, 0x0b, 
			0xf0, 0x95, 0x21, 0x22, 0x5c, 0x6b, 0x4e, 0x82,
			0x54, 0xd6, 0x65, 0x93, 0xce, 0x60, 0xb2, 0x1c, 
			0x73, 0x56, 0xc0, 0x14, 0xa7, 0x8c, 0xf1, 0xdc,
			0x12, 0x75, 0xca, 0x1f, 0x3b, 0xbe, 0xe4, 0xd1, 
			0x42, 0x3d, 0xd4, 0x30, 0xa3, 0x3c, 0xb6, 0x26,
			0x6f, 0xbf, 0x0e, 0xda, 0x46, 0x69, 0x07, 0x57, 
			0x27, 0xf2, 0x1d, 0x9b, 0xbc, 0x94, 0x43, 0x03,
			0xf8, 0x11, 0xc7, 0xf6, 0x90, 0xef, 0x3e, 0xe7, 
			0x06, 0xc3, 0xd5, 0x2f, 0xc8, 0x66, 0x1e, 0xd7,
			0x08, 0xe8, 0xea, 0xde, 0x80, 0x52, 0xee, 0xf7, 
			0x84, 0xaa, 0x72, 0xac, 0x35, 0x4d, 0x6a, 0x2a,
			0x96, 0x1a, 0xd2, 0x71, 0x5a, 0x15, 0x49, 0x74, 
			0x4b, 0x9f, 0xd0, 0x5e, 0x04, 0x18, 0xa4, 0xec,
			0xc2, 0xe0, 0x41, 0x6e, 0x0f, 0x51, 0xcb, 0xcc, 
			0x24, 0x91, 0xaf, 0x50, 0xa1, 0xf4, 0x70, 0x39,
			0x99, 0x7c, 0x3a, 0x85, 0x23, 0xb8, 0xb4, 0x7a, 
			0xfc, 0x02, 0x36, 0x5b, 0x25, 0x55, 0x97, 0x31,
			0x2d, 0x5d, 0xfa, 0x98, 0xe3, 0x8a, 0x92, 0xae, 
			0x05, 0xdf, 0x29, 0x10, 0x67, 0x6c, 0xba, 0xc9,
			0xd3, 0x00, 0xe6, 0xcf, 0xe1, 0x9e, 0xa8, 0x2c, 
			0x63, 0x16, 0x01, 0x3f, 0x58, 0xe2, 0x89, 0xa9,
			0x0d, 0x38, 0x34, 0x1b, 0xab, 0x33, 0xff, 0xb0, 
			0xbb, 0x48, 0x0c, 0x5f, 0xb9, 0xb1, 0xcd, 0x2e,
			0xc5, 0xf3, 0xdb, 0x47, 0xe5, 0xa5, 0x9c, 0x77, 
			0x0a, 0xa6, 0x20, 0x68, 0xfe, 0x7f, 0xc1, 0xad };
	
		// The expanded key (in bottom 16 bits of each word)
		public UInt32[] K;
	
		// The state (again in bottom 16 bits, although we only
		// clear the top 16 bits if needed)
		private UInt32[] R;
	
		// Key indexer
		private int j;
	
		private void Mix() 
		{
			R[0] += K[j]   + (R[3] & R[2]) + ((~R[3]) & R[1]);
			R[0] = (R[0] << 1) | (R[0]>>15 & 0x1);
	
			R[1] += K[j+1] + (R[0] & R[3]) + ((~R[0]) & R[2]);
			R[1] = (R[1] << 2) | (R[1]>>14 & 0x3);
	
			R[2] += K[j+2] + (R[1] & R[0]) + ((~R[1]) & R[3]);
			R[2] = (R[2] << 3) | (R[2]>>13 & 0x7);
	
			R[3] += K[j+3] + (R[2] & R[1]) + ((~R[2]) & R[0]);
			R[3] = (R[3] << 5) | (R[3]>>11 & 0x1f);
			j += 4;
		}
	
		private void RMix() 
		{
			R[3] &= 0xffff;
			R[3] = (R[3] >> 5) | ((R[3] & 0x1f) << 11);
			R[3] -= K[j] + (R[2] & R[1]) + ((~R[2]) & R[0]);
	
			R[2] &= 0xffff;
			R[2] = (R[2] >> 3) | ((R[2] & 0x7) << 13);
			R[2] -= K[j-1] + (R[1] & R[0]) + ((~R[1]) & R[3]);

			R[1] &= 0xffff;
			R[1] = (R[1] >> 2) | ((R[1] & 0x3) << 14);
			R[1] -= K[j-2] + (R[0] & R[3]) + ((~R[0]) & R[2]);
	
			R[0] &= 0xffff;
			R[0] = (R[0] >> 1) | ((R[0] & 0x1) << 15);
			R[0] -= K[j-3] + (R[3] & R[2]) + ((~R[3]) & R[1]);
	
			j -= 4;
		}

		private void Mash ()
		{
			R [0] += K [R [3] & 63];
			R [1] += K [R [0] & 63];
			R [2] += K [R [1] & 63];
			R [3] += K [R [2] & 63];
		}

		private void RMash ()
		{
			R [3] -= K [R [2] & 63];
			R [2] -= K [R [1] & 63];
			R [1] -= K [R [0] & 63];
			R [0] -= K [R [3] & 63];
		}
	}
}
