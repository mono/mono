//
// System.Security.Cryptography.RijndaelManaged.cs
//
// Authors: Mark Crichton (crichton@gimp.org)
//	    Andrew Birkett (andy@nobugs.org)
//          Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;

/// <summary>
/// Rijndael is a symmetric block cipher supporting block and key sizes
/// of 128, 192 and 256 bits.  It has been chosen as the AES cipher.
/// </summary>

namespace System.Security.Cryptography {

// References:
// a.	FIPS PUB 197: Advanced Encryption Standard
//	http://csrc.nist.gov/publications/fips/fips197/fips-197.pdf

public sealed class RijndaelManaged : Rijndael {
	
	/// <summary>
	/// RijndaelManaged constructor.
	/// </summary>
	public RijndaelManaged() {}
	
	/// <summary>
	/// Generates a random IV for block feedback modes
	/// </summary>
	/// <remarks>
	/// Method is inherited from SymmetricAlgorithm
	/// </remarks>
	public override void GenerateIV () 
	{
		IVValue = KeyBuilder.IV (BlockSizeValue >> 3);
	}
	
	/// <summary>
	/// Generates a random key for Rijndael.  Uses the current KeySize.
	/// </summary>
	/// <remarks>
	/// Inherited method from base class SymmetricAlgorithm
	/// </remarks>
	public override void GenerateKey () 
	{
		KeyValue = KeyBuilder.Key (KeySizeValue >> 3);
	}
	
	/// <summary>
	/// Creates a symmetric Rijndael decryptor object
	/// </summary>
	/// <remarks>
	/// Inherited method from base class SymmetricAlgorithm
	/// </remarks>
	/// <param name='rgbKey'>Key for Rijndael</param>
	/// <param name='rgbIV'>IV for chaining mode</param>
	public override ICryptoTransform CreateDecryptor (byte[] rgbKey, byte[] rgbIV) 
	{
		Key = rgbKey;
		IV = rgbIV;
		return new RijndaelTransform (this, false, rgbKey, rgbIV);
	}
	
	/// <summary>
	/// Creates a symmetric Rijndael encryptor object
	/// </summary>
	/// <remarks>
	/// Inherited method from base class SymmetricAlgorithm
	/// </remarks>
	/// <param name='rgbKey'>Key for Rijndael</param>
	/// <param name='rgbIV'>IV for chaining mode</param>
	public override ICryptoTransform CreateEncryptor (byte[] rgbKey, byte[] rgbIV) 
	{
		Key = rgbKey;
		IV = rgbIV;
		return new RijndaelTransform (this, true, rgbKey, rgbIV);
	}
}


internal class RijndaelTransform : SymmetricTransform
{
	private byte[] key;
	private Int32[] expandedKey;

	private int Nb;
	private int Nk;
	private int Nr;
	private int[,] shifts;

	private Int32[] rcon;

	private Byte[,] state;

	public RijndaelTransform (Rijndael algo, bool encryption, byte[] key, byte[] iv) : base (algo, encryption, iv)
	{
		int keySize = algo.KeySize;
		if (keySize != 128 && keySize != 192 && keySize != 256) {
			throw new ArgumentException("Illegal key size");
		}
		int blockSize = algo.BlockSize;
		if (blockSize != 128 && blockSize != 192 && blockSize != 256) {
			throw new ArgumentException("Illegal block size");
		}
		if (key.Length * 8 != keySize) {
			throw new ArgumentException("Key size doesn't match key");
		}

		this.key = key;
		this.Nb = (blockSize >> 5); // div 32
		this.Nk = (keySize >> 5); // div 32
		this.state = new byte[4, Nb];

		if (Nb == 8 || Nk == 8) {
			Nr = 14;
		} else if (Nb == 6 || Nk == 6) {
			Nr = 12;
		} else {
			Nr = 10;
		}

		shifts = new int[2,4];
		switch (Nb) {
		case 8: // 256 bits
			// encryption
			shifts [0,0] = -1; // Not used
			shifts [0,1] = 1;
			shifts [0,2] = 3;
			shifts [0,3] = 4;
			// decryption
			shifts [1,0] = -1; // Not used
			shifts [1,1] = 7;
			shifts [1,2] = 5;
			shifts [1,3] = 4;
			break;
		case 6: // 192 bits
			// encryption
			shifts [0,0] = -1; // Not used
			shifts [0,1] = 1;
			shifts [0,2] = 2;
			shifts [0,3] = 3;
			// decryption
			shifts [1,0] = -1; // Not used
			shifts [1,1] = 5;
			shifts [1,2] = 4;
			shifts [1,3] = 3;
			break;
		case 4: // 128 bits
			// encryption
			shifts [0,0] = -1; // Not used
			shifts [0,1] = 1;
			shifts [0,2] = 2;
			shifts [0,3] = 3;
			// decryption
			shifts [1,0] = -1; // Not used
			shifts [1,1] = 3;
			shifts [1,2] = 2;
			shifts [1,3] = 1;
			break;
		}

		int rcon_entries = (Nb * (Nr+1)) / Nk;
		rcon = new Int32 [rcon_entries + 1];
		Byte curr = 0x1;
		for (int i=1; i < rcon.Length; i++) {
			rcon[i] = curr << 24;
			curr = Mult2_GF(curr);
		}

		SetupExpandedKey();		
	}

	private void SetupExpandedKey()
	{
		expandedKey = new Int32[Nb * (Nr+1)];
		int pos = 0;
		for (int i=0; i < Nk; i++) {
			Int32 value = (key [pos++] << 24);
			value |= (key [pos++] << 16);
			value |= (key [pos++] << 8);
			value |= (key [pos++]);
			expandedKey [i] = value;
		}

		for (int i = Nk; i < Nb * (Nr+1); i++) {
			Int32 temp = expandedKey [i-1];
			if (i % Nk == 0) {
				Int32 rot = (Int32) ((temp << 8) | ((temp >> 24) & 0xff));
				temp = SubByte (rot) ^ rcon [i / Nk];
			} else if (Nk > 6 && (i % Nk) == 4) {
				temp = SubByte (temp);
			}
			expandedKey [i] = expandedKey [i-Nk] ^ temp;
		}
	}

	// note: this method is garanteed to be called with a valid blocksize
	// for both input and output
	protected override void ECB (byte[] input, byte[] output) 
	{
		int pos = 0;
		for (int col = 0; col < Nb; col++) {	
			// loop unrooling, elimitated mul/add
			state [0, col] = input [pos++];
			state [1, col] = input [pos++];
			state [2, col] = input [pos++];
			state [3, col] = input [pos++];
		} 

		AddRoundKey (0, encrypt);
		if (encrypt) {
			for (int round = 1; round < Nr; round++) {
				ByteSub (true);
				ShiftRow (true);
				MixColumn ();
				AddRoundKey (round, true);
			}
			ByteSub (true);
			ShiftRow (true);
		}
		else {
			ShiftRow (false);
			ByteSub (false);
			for (int round = 1; round < Nr; round++) {
				AddRoundKey (round, false);
				InvMixColumn ();
				ShiftRow (false);
				ByteSub (false);
			}
		}
		AddRoundKey (Nr, encrypt);

		pos = 0;
		for (int col = 0; col < Nb; col++) {	
			// loop unrooling, elimitated mul/add
			output [pos++] = state [0, col];
			output [pos++] = state [1, col];
			output [pos++] = state [2, col];
			output [pos++] = state [3, col];
		} 
	}

	private void ByteSub (bool encrypt)
	{
		// to remove the if evaluation inside the 2 loops
		byte[] box = ((encrypt) ? sbox : invSbox);
		for (int col = 0; col < Nb; col++) {
			// unrolled loop
			state [0, col] = box [state [0, col]];
			state [1, col] = box [state [1, col]];
			state [2, col] = box [state [2, col]];
			state [3, col] = box [state [3, col]];
		} 
	}

	private void ShiftRow (bool encrypt)
	{
		int shift_index = encrypt ? 0 : 1;
		byte[] temp = new byte [Nb];
		for (int row = 1; row < 4; row++) {
			for (int col = 0; col < Nb; col++) {
				int source_col = (col + shifts [shift_index, row]) % Nb;
				temp[col] = state[row, source_col];
			}
			for (int col = 0; col < Nb; col++) {
				state[row, col] = temp[col];
			}
		} 
	}

	// Multiply [a] and [b] in GF(2^8)
	private Byte Mult_GF (Byte a, Byte b)
	{
		if (a > 0 && b > 0) {
			return alogtable[ (logtable[a] + logtable[b]) % 255];
		} else {
			return 0;
		}
	}

	// Faster version for 2
	private Byte Mult2_GF (Byte a)
	{
		if ((a & 0x80) == 0) {
			return (byte) (a << 1);
		} else {
			return (byte) ((a << 1) ^ 0x1b);
		}
	}

	private void MixColumn ()
	{
		int[,] tmp = new int[4, Nb];
		for (int col = 0; col < Nb; col++) {
			// unrolled loop (removed modulo)
			tmp[0,col] = Mult2_GF(state[0, col]) ^
				Mult_GF(3, state[1, col]) ^
				state[2, col] ^
				state[3, col];
			tmp[1,col] = Mult2_GF(state[1, col]) ^
				Mult_GF(3, state[2, col]) ^
				state[3, col] ^
				state[0, col];
			tmp[2,col] = Mult2_GF(state[2, col]) ^
				Mult_GF(3, state[3, col]) ^
				state[0, col] ^
				state[1, col];
			tmp[3,col] = Mult2_GF(state[3, col]) ^
				Mult_GF(3, state[0, col]) ^
				state[1, col] ^
				state[2, col];
		}

		for (int col = 0; col < Nb; col++) {
			// loop unrooling
			state[0, col] = (byte) tmp[0, col];
			state[1, col] = (byte) tmp[1, col];
			state[2, col] = (byte) tmp[2, col];
			state[3, col] = (byte) tmp[3, col];
		}
	}

	private void InvMixColumn ()
	{
		int[,] tmp = new int[4, Nb];
		for (int col = 0; col < Nb; col++) {
			// unrolled loop (removed modulo)
			tmp[0,col] = Mult_GF(0xe, state [0, col]) ^
				Mult_GF(0xb, state [1, col]) ^
				Mult_GF(0xd, state [2, col]) ^
				Mult_GF(0x9, state [3, col]);
			tmp[1,col] = Mult_GF(0xe, state [1, col]) ^
				Mult_GF(0xb, state [2, col]) ^
				Mult_GF(0xd, state [3, col]) ^
				Mult_GF(0x9, state [0, col]);
			tmp[2,col] = Mult_GF(0xe, state [2, col]) ^
				Mult_GF(0xb, state [3, col]) ^
				Mult_GF(0xd, state [0, col]) ^
				Mult_GF(0x9, state [1, col]);
			tmp[3,col] = Mult_GF(0xe, state [3, col]) ^
				Mult_GF(0xb, state [0, col]) ^
				Mult_GF(0xd, state [1, col]) ^
				Mult_GF(0x9, state [2, col]);
		}

		for (int col = 0; col < Nb; col++) {
			// loop unrooling
			state[0, col] = (byte) tmp[0, col];
			state[1, col] = (byte) tmp[1, col];
			state[2, col] = (byte) tmp[2, col];
			state[3, col] = (byte) tmp[3, col];
		}
	}

	private void AddRoundKey (int round, bool encrypt)
	{
		int roundoffset = encrypt ? (Nb * round) : (Nb * (Nr-round));
		for (int col = 0; col < Nb; col++) {
			Int32 keyword = expandedKey [roundoffset + col];
			state [0,col] ^= (byte) ((keyword >> 24) & 0xff);
			state [1,col] ^= (byte) ((keyword >> 16) & 0xff);
			state [2,col] ^= (byte) ((keyword >> 8) & 0xff);
			state [3,col] ^= (byte) (keyword & 0xff);
		}
	}

	private Int32 SubByte (Int32 a)
	{
		// unrolled loop (no more multiply)
		Int32 value = 0xff & a;
		Int32 curr = sbox [value]; 
		Int32 result = curr;
		value = 0xff & (a >> 8);
		curr = sbox [value] << 8; 
		result |= curr;
		value = 0xff & (a >> 16);
		curr = sbox [value] << 16; 
		result |= curr;
		value = 0xff & (a >> 24);
		curr = sbox [value] << 24; 
		result |= curr;
		return result;
	}

	// Constant tables used in the cipher
	static byte[] sbox = {
	99, 124, 119, 123, 242, 107, 111, 197,  48,   1, 103,  43, 254, 215, 171, 118, 
	202, 130, 201, 125, 250,  89,  71, 240, 173, 212, 162, 175, 156, 164, 114, 192, 
	183, 253, 147,  38,  54,  63, 247, 204,  52, 165, 229, 241, 113, 216,  49,  21, 
	4, 199,  35, 195,  24, 150,   5, 154,   7,  18, 128, 226, 235,  39, 178, 117, 
	9, 131,  44,  26,  27, 110,  90, 160,  82,  59, 214, 179,  41, 227,  47, 132, 
	83, 209,   0, 237,  32, 252, 177,  91, 106, 203, 190,  57,  74,  76,  88, 207, 
	208, 239, 170, 251,  67,  77,  51, 133,  69, 249,   2, 127,  80,  60, 159, 168, 
	81, 163,  64, 143, 146, 157,  56, 245, 188, 182, 218,  33,  16, 255, 243, 210, 
	205,  12,  19, 236,  95, 151,  68,  23, 196, 167, 126,  61, 100,  93,  25, 115, 
	96, 129,  79, 220,  34,  42, 144, 136,  70, 238, 184,  20, 222,  94,  11, 219, 
	224,  50,  58,  10,  73,   6,  36,  92, 194, 211, 172,  98, 145, 149, 228, 121, 
	231, 200,  55, 109, 141, 213,  78, 169, 108,  86, 244, 234, 101, 122, 174,   8, 
	186, 120,  37,  46,  28, 166, 180, 198, 232, 221, 116,  31,  75, 189, 139, 138, 
	112,  62, 181, 102,  72,   3, 246,  14,  97,  53,  87, 185, 134, 193,  29, 158, 
	225, 248, 152,  17, 105, 217, 142, 148, 155,  30, 135, 233, 206,  85,  40, 223, 
	140, 161, 137,  13, 191, 230,  66, 104,  65, 153,  45,  15, 176,  84, 187,  22 
	};

	static byte[] invSbox = {
	82,   9, 106, 213,  48,  54, 165,  56, 191,  64, 163, 158, 129, 243, 215, 251, 
	124, 227,  57, 130, 155,  47, 255, 135,  52, 142,  67,  68, 196, 222, 233, 203, 
	84, 123, 148,  50, 166, 194,  35,  61, 238,  76, 149,  11,  66, 250, 195,  78, 
	8,  46, 161, 102,  40, 217,  36, 178, 118,  91, 162,  73, 109, 139, 209,  37, 
	114, 248, 246, 100, 134, 104, 152,  22, 212, 164,  92, 204,  93, 101, 182, 146, 
	108, 112,  72,  80, 253, 237, 185, 218,  94,  21,  70,  87, 167, 141, 157, 132, 
	144, 216, 171,   0, 140, 188, 211,  10, 247, 228,  88,   5, 184, 179,  69,   6, 
	208,  44,  30, 143, 202,  63,  15,   2, 193, 175, 189,   3,   1,  19, 138, 107, 
	58, 145,  17,  65,  79, 103, 220, 234, 151, 242, 207, 206, 240, 180, 230, 115, 
	150, 172, 116,  34, 231, 173,  53, 133, 226, 249,  55, 232,  28, 117, 223, 110, 
	71, 241,  26, 113,  29,  41, 197, 137, 111, 183,  98,  14, 170,  24, 190,  27, 
	252,  86,  62,  75, 198, 210, 121,  32, 154, 219, 192, 254, 120, 205,  90, 244, 
	31, 221, 168,  51, 136,   7, 199,  49, 177,  18,  16,  89,  39, 128, 236,  95, 
	96,  81, 127, 169,  25, 181,  74,  13,  45, 229, 122, 159, 147, 201, 156, 239, 
	160, 224,  59,  77, 174,  42, 245, 176, 200, 235, 187,  60, 131,  83, 153,  97, 
	23,  43,   4, 126, 186, 119, 214,  38, 225, 105,  20,  99,  85,  33,  12, 125
	};

	static byte[] logtable = {
	0,   0,  25,   1,  50,   2,  26, 198,  75, 199,  27, 104,  51, 238, 223,   3, 
	100,   4, 224,  14,  52, 141, 129, 239,  76, 113,   8, 200, 248, 105,  28, 193, 
	125, 194,  29, 181, 249, 185,  39, 106,  77, 228, 166, 114, 154, 201,   9, 120, 
	101,  47, 138,   5,  33,  15, 225,  36,  18, 240, 130,  69,  53, 147, 218, 142, 
	150, 143, 219, 189,  54, 208, 206, 148,  19,  92, 210, 241,  64,  70, 131,  56, 
	102, 221, 253,  48, 191,   6, 139,  98, 179,  37, 226, 152,  34, 136, 145,  16, 
	126, 110,  72, 195, 163, 182,  30,  66,  58, 107,  40,  84, 250, 133,  61, 186, 
	43, 121,  10,  21, 155, 159,  94, 202,  78, 212, 172, 229, 243, 115, 167,  87, 
	175,  88, 168,  80, 244, 234, 214, 116,  79, 174, 233, 213, 231, 230, 173, 232, 
	44, 215, 117, 122, 235,  22,  11, 245,  89, 203,  95, 176, 156, 169,  81, 160, 
	127,  12, 246, 111,  23, 196,  73, 236, 216,  67,  31,  45, 164, 118, 123, 183, 
	204, 187,  62,  90, 251,  96, 177, 134,  59,  82, 161, 108, 170,  85,  41, 157, 
	151, 178, 135, 144,  97, 190, 220, 252, 188, 149, 207, 205,  55,  63,  91, 209, 
	83,  57, 132,  60,  65, 162, 109,  71,  20,  42, 158,  93,  86, 242, 211, 171, 
	68,  17, 146, 217,  35,  32,  46, 137, 180, 124, 184,  38, 119, 153, 227, 165, 
	103,  74, 237, 222, 197,  49, 254,  24,  13,  99, 140, 128, 192, 247, 112,   7 
	};

	static byte[] alogtable = {
	1,   3,   5,  15,  17,  51,  85, 255,  26,  46, 114, 150, 161, 248,  19,  53, 
	95, 225,  56,  72, 216, 115, 149, 164, 247,   2,   6,  10,  30,  34, 102, 170, 
	229,  52,  92, 228,  55,  89, 235,  38, 106, 190, 217, 112, 144, 171, 230,  49, 
	83, 245,   4,  12,  20,  60,  68, 204,  79, 209, 104, 184, 211, 110, 178, 205, 
	76, 212, 103, 169, 224,  59,  77, 215,  98, 166, 241,   8,  24,  40, 120, 136, 
	131, 158, 185, 208, 107, 189, 220, 127, 129, 152, 179, 206,  73, 219, 118, 154, 
	181, 196,  87, 249,  16,  48,  80, 240,  11,  29,  39, 105, 187, 214,  97, 163, 
	254,  25,  43, 125, 135, 146, 173, 236,  47, 113, 147, 174, 233,  32,  96, 160, 
	251,  22,  58,  78, 210, 109, 183, 194,  93, 231,  50,  86, 250,  21,  63,  65, 
	195,  94, 226,  61,  71, 201,  64, 192,  91, 237,  44, 116, 156, 191, 218, 117, 
	159, 186, 213, 100, 172, 239,  42, 126, 130, 157, 188, 223, 122, 142, 137, 128, 
	155, 182, 193,  88, 232,  35, 101, 175, 234,  37, 111, 177, 200,  67, 197,  84, 
	252,  31,  33,  99, 165, 244,   7,   9,  27,  45, 119, 153, 176, 203,  70, 202, 
	69, 207,  74, 222, 121, 139, 134, 145, 168, 227,  62,  66, 198,  81, 243,  14, 
	18,  54,  90, 238,  41, 123, 141, 140, 143, 138, 133, 148, 167, 242,  13,  23, 
	57,  75, 221, 124, 132, 151, 162, 253,  28,  36, 108, 180, 199,  82, 246,   1 
	};

}

}

