//
// System.Security.Cryptography.RijndaelManaged.cs
//
// Authors: Mark Crichton (crichton@gimp.org)
//	    Andrew Birkett (andy@nobugs.org)
// (C) 2002
//

using System;
using System.Security.Cryptography;

/// <summary>
/// Rijndael is a symmetric block cipher supporting block and key sizes
/// of 128, 192 and 256 bits.  It has been chosen as the AES cipher.
/// </summary>

namespace System.Security.Cryptography {

	public sealed class RijndaelManaged : Rijndael {
		
		/// <summary>
		/// RijndaelManaged constructor.
		/// </summary>
		public RijndaelManaged() {
		}
		
		/// <summary>
		/// Generates a random IV for block feedback modes
		/// </summary>
		/// <remarks>
		/// Method is inherited from SymmetricAlgorithm
		///
		/// </remarks>
		[MonoTODO]
		public override void GenerateIV() {
			throw new System.NotImplementedException();
		}
		
		/// <summary>
		/// Generates a random key for Rijndael.  Uses the current KeySize.
		/// </summary>
		/// <remarks>
		/// Inherited method from base class SymmetricAlgorithm
		///
		/// </remarks>
		[MonoTODO]
		public override void GenerateKey() {
			throw new System.NotImplementedException();
		}
		
		/// <summary>
		/// Creates a symmetric Rijndael decryptor object
		/// </summary>
		/// <remarks>
		/// Inherited method from base class SymmetricAlgorithm
		///
		/// </remarks>
		/// <param name='rgbKey'>Key for Rijndael</param>
		/// <param name='rgbIV'>IV for chaining mode</param>
		[MonoTODO]
		public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV) {
			return new RijndaelController(false, 
				KeySizeValue, BlockSizeValue, rgbKey, rgbIV,
				ModeValue, PaddingValue);
		}
		
		/// <summary>
		/// Creates a symmetric Rijndael encryptor object
		/// </summary>
		/// <remarks>
		/// Inherited method from base class SymmetricAlgorithm
		///
		/// </remarks>
		/// <param name='rgbKey'>Key for Rijndael</param>
		/// <param name='rgbIV'>IV for chaining mode</param>
		[MonoTODO]
		public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV) {
			return new RijndaelController(true, 
				KeySizeValue, BlockSizeValue, rgbKey, rgbIV,
				ModeValue, PaddingValue);
		}
	}

	internal class RijndaelController : ICryptoTransform
	{
		private bool encrypt;
		private int blocksize;
		private byte[] iv;
		private byte[] key;
		private CipherMode ciphermode;
		private PaddingMode paddingmode;

		// For chaining modes
		private byte[] feedback;

		RijndaelImpl impl;

		public RijndaelController(
			bool encrypt, 
			int keysize, int blocksize, 
			byte[] key, byte[] iv, 
			CipherMode ciphermode, PaddingMode paddingmode)
		{
			this.encrypt = encrypt;
			this.key = key;
			this.iv = iv;
			this.ciphermode = ciphermode;
			this.paddingmode = paddingmode;
			this.blocksize = blocksize;

			if (ciphermode != CipherMode.ECB && ciphermode != CipherMode.CBC) {
				throw new System.NotImplementedException();
			}
			if (key.Length * 8 != keysize) {
				throw new ArgumentException("Key doesn't match key length");
			}
			if (iv.Length * 8 != blocksize) {
				throw new ArgumentException("IV doesn't match block size");
			}
			if (paddingmode != PaddingMode.Zeros) {
				throw new ArgumentException("Only zero padding supported just now");
			}

			// Initialize feedback buffer
			feedback = new byte[iv.Length];
			Array.Copy(iv, 0, feedback, 0, iv.Length);

			impl = new RijndaelImpl(keysize, blocksize, key);
		}

		// ICryptoTransform members
		public bool CanTransformMultipleBlocks {
			get {
				return false;
			}
		}

		public int InputBlockSize {
			get {
				return blocksize;				
			}
		}
		
		public int OutputBlockSize {
			get {
				return blocksize;
			}
		}

		private void XorInto(byte[] src, byte[] dest) 
		{
			if (src.Length != dest.Length) {
				throw new ArgumentException("Arrays have different lengths");
			}

			for (int i=0; i < dest.Length; i++) {
				dest[i] = (byte) (src[i] ^ dest[i]);
			}
		}

		public int TransformBlock (byte[] inputBuffer, int inputOffset, int inputCount, 
					   byte[] outputBuffer, int outputOffset)
		{
			if (inputCount * 8 != blocksize) {
				throw new ArgumentException("Input length doesn't match block size");
			}

			byte[] pre = new byte[blocksize / 8];
			Array.Copy(inputBuffer, inputOffset, pre, 0, inputCount);

			if (encrypt && ciphermode == CipherMode.CBC) {
				XorInto(feedback, pre);
			}

			// Do the encryption/decryption proper
			byte[] post = encrypt ? impl.Encrypt(pre) : impl.Decrypt(pre);

			if (ciphermode == CipherMode.CBC) {
				Array.Copy(post, 0, feedback, 0, post.Length);
			}

			if (!encrypt && ciphermode == CipherMode.CBC) {
				XorInto(feedback, post);
			}

			Array.Copy(post, 0, outputBuffer, outputOffset, blocksize / 8);

			return post.Length;
		}

                public byte[] TransformFinalBlock (byte[] inputBuffer, int inputOffset, int inputCount)
		{
			if (inputCount * 8 > blocksize) {
				throw new ArgumentException("Input length exceeds block size");
			}

			if (paddingmode == PaddingMode.None && inputCount != blocksize / 8) {
				throw new ArgumentException("Input must be a complete block if padding is None");
			}

			byte[] input = new byte[blocksize / 8];
			Array.Copy(inputBuffer, inputOffset, input, 0, inputCount);
			
			byte padding_value = 0;

			if (paddingmode == PaddingMode.PKCS7) {
				padding_value = (byte) (input.Length - inputCount);
			} 

			for (int i = inputCount; i < input.Length; i++) {
				input[i] = padding_value;
			}

			return encrypt ? impl.Encrypt(input) : impl.Decrypt(input);
		}
	}

	internal class RijndaelImpl
	{
		private byte[] key;
		private Int32[] expandedKey;

		private int Nb;
		private int Nk;
		private int Nr;
		private int[,] shifts;
	
		private Int32[] rcon;
	
		private Byte[,] state;
			
		public RijndaelImpl(int keySize, int blockSize, byte[] key) 
		{
			if (keySize != 128 && keySize != 192 && keySize != 256) {
				throw new ArgumentException("Illegal key size");
			}
	
			if (blockSize != 128 && blockSize != 192 && blockSize != 256) {
				throw new ArgumentException("Illegal block size");
			}
	
			if (key.Length * 8 != keySize) {
				throw new ArgumentException("Key size doesn't match key");
			}
	
			this.key = key;
			this.Nb = blockSize / 32;
			this.Nk = keySize / 32;
			this.state = new Byte[Nb, 4];
	
			if (Nb == 8 || Nk == 8) {
				Nr = 14;
			} else if (Nb == 6 || Nk == 6) {
				Nr = 12;
			} else {
				Nr = 10;
			}
	
			shifts = new int[2,4];
			// Encryption
			shifts[0,0] = -1; // Not used
			shifts[0,1] = 1;
			shifts[0,2] = (Nb == 8) ? 3 : 2;
			shifts[0,3] = (Nb == 8) ? 4 : 3;
	
			// Decryption
			shifts[1,0] = -1; // Not used
			shifts[1,1] = 3;
			shifts[1,2] = (Nb == 8) ? 1 : 2;
			shifts[1,3] = (Nb == 8) ? 0 : 1;
	
			int rcon_entries = (Nb * (Nr+1)) / Nk;
			rcon = new Int32[rcon_entries + 1];
			Byte curr = 0x1;
			for (int i=1; i < rcon.Length; i++) {
				rcon[i] = curr << 24;
				curr = Mult_GF(2,curr);
			}
		}
	
		private void SetupExpandedKey()
		{
			expandedKey = new Int32[Nb * (Nr+1)];
			for (int i=0; i < Nk; i++) {
				Int32 value =
					(key[i*4] << 24) | 
					(key[i*4+1] << 16) | 
					(key[i*4+2] << 8) |
					(key[i*4+3]);
				expandedKey[i] = value;
			}
	
			for (int i = Nk; i < Nb * (Nr+1); i++) {
				Int32 temp = expandedKey[i-1];
				if (i % Nk == 0) {
					temp = SubByte(RotByte(temp)) ^ rcon[i / Nk];
				} else if (Nk > 6 && (i % Nk) == 4) {
					temp = SubByte(temp);
				}
				expandedKey[i] = expandedKey[i-Nk] ^ temp;
			}
		}
	
		public byte[] Encrypt(byte[] plaintext)
		{
			SetupExpandedKey();		
	
			if (plaintext.Length != Nb * 4) {
				throw new ArgumentException("Plaintext must be a block");
			}
	
			for (int col = 0; col < Nb; col++) {	
				for (int row = 0; row < 4; row++) {
					state[row, col] = plaintext[col*4 + row];
				}
			} 
	
			AddRoundKey(0, true);
	
			for (int round = 1; round < Nr; round++) {
				Round(round, true);
			}
			ByteSub(true);
			ShiftRow(true);
			AddRoundKey(Nr, true);
	
			Byte[] result = new Byte[4 * Nb];
			for (int col = 0; col < Nb; col++) {	
				for (int row = 0; row < 4; row++) {
					result[col*4 + row] = state[row, col];
				}
			} 
	
			return result;
		}
	
		public byte[] Decrypt(byte[] ciphertext)
		{
			SetupExpandedKey();		
	
			if (ciphertext.Length != Nb * 4) {
				throw new ArgumentException("Ciphertext must be a block");
			}
	
			for (int col = 0; col < Nb; col++) {	
				for (int row = 0; row < 4; row++) {
					state[row, col] = ciphertext[col*4 + row];
				}
			} 
	
			AddRoundKey(0, false);
			ShiftRow(false);
			ByteSub(false);
	
			for (int i=1; i < Nr; i++) {
				Round(i, false);
			}
	
			AddRoundKey(Nr, false);
	
			Byte[] result = new Byte[4 * Nb];
			for (int col = 0; col < Nb; col++) {	
				for (int row = 0; row < 4; row++) {
					result[col*4 + row] = state[row, col];
				}
			} 
	
			return result;
	
		}	
	
		private void ByteSub(bool encrypt)
		{
			for (int col = 0; col < Nb; col++) {	
				for (int row = 0; row < 4; row++) {
					if (encrypt) {
						state[row, col] = sbox[ state[row, col] ];
					} else {
						state[row, col] = invSbox[ state[row, col] ];
					}
				}
			} 
	
		}
	
		private void ShiftRow(bool encrypt)
		{
			int shift_index = encrypt ? 0 : 1;
			byte[] temp = new byte[Nb];
			for (int row = 1; row < 4; row++) {
				for (int col = 0; col < Nb; col++) {
					int source_col = (col + shifts[shift_index, row]) % Nb;
					temp[col] = state[row, source_col];
				}
				for (int col = 0; col < Nb; col++) {
					state[row,col] = temp[col];
				}
			} 
		}
	
		// Multiply [a] and [b] in GF(2^8)
		private Byte Mult_GF(Byte a, Byte b)
		{
			if (a > 0 && b > 0) {
				return alogtable[ (logtable[a] + logtable[b]) % 255];
			} else {
				return 0;
			}
		}
	
		private void MixColumn()
		{
			int[,] tmp = new int[Nb, 4];
			for (int col = 0; col < Nb; col++) {
				for (int row = 0; row < 4; row++) {
					tmp[row,col] = 
						Mult_GF(2, state[row, col]) ^
						Mult_GF(3, state[(row+1) % 4, col]) ^
						state[(row+2) % 4, col] ^
						state[(row+3) % 4, col];
								
				}
			}
	
			for (int col = 0; col < Nb; col++) {
				for (int row = 0; row < 4; row++) {
					state[row,col] = (Byte) tmp[row,col];
				}
			}
		}
	
		private void InvMixColumn()
		{
			int[,] tmp = new int[Nb, 4];
			for (int col = 0; col < Nb; col++) {
				for (int row = 0; row < 4; row++) {
					tmp[row,col] = 
						Mult_GF(0xe, state[row, col]) ^
						Mult_GF(0xb, state[(row+1) % 4, col]) ^
						Mult_GF(0xd, state[(row+2) % 4, col]) ^
						Mult_GF(0x9, state[(row+3) % 4, col]);
								
				}
			}
	
			for (int col = 0; col < Nb; col++) {
				for (int row = 0; row < 4; row++) {
					state[row,col] = (Byte) tmp[row,col];
				}
			}
		}
	
		private void AddRoundKey(int round, bool encrypt)
		{
			int roundoffset = encrypt ? (Nb * round) : (Nb * (Nr-round));
			for (int col = 0; col < Nb; col++) {
				Int32 keyword = expandedKey[roundoffset + col];
				state[0,col] ^= (Byte) ((keyword >> 24) & 0xff);
				state[1,col] ^= (Byte) ((keyword >> 16) & 0xff);
				state[2,col] ^= (Byte) ((keyword >> 8) & 0xff);
				state[3,col] ^= (Byte) (keyword & 0xff);
			}
		}
	
		private void Round(int round, bool encrypt)
		{
			if (encrypt) {
				ByteSub(true);
				ShiftRow(encrypt);
				MixColumn();
				AddRoundKey(round, true);
			} else {
				AddRoundKey(round, false);
				InvMixColumn();
				ShiftRow(false);
				ByteSub(false);
			}
		}
	
		private Int32 SubByte(Int32 a)
		{
			Int32 result = 0;
			for (int i=0; i < 4; i++) {
				Int32 value = 0xff & (a >> (8*i));
				Int32 curr = sbox[value] << (i*8); 
				result |= curr;
			}
			return result;
		}
	
		private Int32 RotByte(Int32 a)
		{
			return (a << 8) | ((a >> 24) & 0xff) ;
		}
			
		// Constant tables used in the cipher
		Byte[] sbox = {
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
	
		Byte[] invSbox = {
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
	
		Byte[] logtable = {
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
	
		Byte[] alogtable = {
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

