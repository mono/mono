//
// ZipAESTransform.cs
//
// Copyright 2009 David Pierson
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// Linking this library statically or dynamically with other modules is
// making a combined work based on this library.  Thus, the terms and
// conditions of the GNU General Public License cover the whole
// combination.
//
// As a special exception, the copyright holders of this library give you
// permission to link this library with independent modules to produce an
// executable, regardless of the license terms of these independent
// modules, and to copy and distribute the resulting executable under
// terms of your choice, provided that you also meet, for each linked
// independent module, the terms and conditions of the license of that
// module.  An independent module is a module which is not derived from
// or based on this library.  If you modify this library, you may extend
// this exception to your version of the library, but you are not
// obligated to do so.  If you do not wish to do so, delete this
// exception statement from your version.
//

#if !NET_1_1 && !NETCF_2_0
// Framework version 2.0 required for Rfc2898DeriveBytes

using System;
using System.Security.Cryptography;

namespace ICSharpCode.SharpZipLib.Encryption {

	/// <summary>
	/// Transforms stream using AES in CTR mode
	/// </summary>
	internal class ZipAESTransform : ICryptoTransform {

		private const int PWD_VER_LENGTH = 2;

		// WinZip use iteration count of 1000 for PBKDF2 key generation
		private const int KEY_ROUNDS = 1000;

		// For 128-bit AES (16 bytes) the encryption is implemented as expected.
		// For 256-bit AES (32 bytes) WinZip do full 256 bit AES of the nonce to create the encryption
		// block but use only the first 16 bytes of it, and discard the second half.
		private const int ENCRYPT_BLOCK = 16;

		private int _blockSize;
		private ICryptoTransform _encryptor;
		private readonly byte[] _counterNonce;
		private byte[] _encryptBuffer;
		private int _encrPos;
		private byte[] _pwdVerifier;
		private HMACSHA1 _hmacsha1;
		private bool _finalised;

		private bool _writeMode;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="key">Password string</param>
		/// <param name="saltBytes">Random bytes, length depends on encryption strength.
		/// 128 bits = 8 bytes, 192 bits = 12 bytes, 256 bits = 16 bytes.</param>
		/// <param name="blockSize">The encryption strength, in bytes eg 16 for 128 bits.</param>
		/// <param name="writeMode">True when creating a zip, false when reading. For the AuthCode.</param>
		///
		public ZipAESTransform(string key, byte[] saltBytes, int blockSize, bool writeMode) {

			if (blockSize != 16 && blockSize != 32)	// 24 valid for AES but not supported by Winzip
				throw new Exception("Invalid blocksize " + blockSize + ". Must be 16 or 32.");
			if (saltBytes.Length != blockSize / 2)
				throw new Exception("Invalid salt len. Must be " + blockSize / 2 + " for blocksize " + blockSize);
			// initialise the encryption buffer and buffer pos
			_blockSize = blockSize;
			_encryptBuffer = new byte[_blockSize];
			_encrPos = ENCRYPT_BLOCK;

			// Performs the equivalent of derive_key in Dr Brian Gladman's pwd2key.c
			Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(key, saltBytes, KEY_ROUNDS);
			RijndaelManaged rm = new RijndaelManaged();
			rm.Mode = CipherMode.ECB;			// No feedback from cipher for CTR mode
			_counterNonce = new byte[_blockSize];
			byte[] byteKey1 = pdb.GetBytes(_blockSize);
			byte[] byteKey2 = pdb.GetBytes(_blockSize);
			_encryptor = rm.CreateEncryptor(byteKey1, byteKey2);
			_pwdVerifier = pdb.GetBytes(PWD_VER_LENGTH);
			//
			_hmacsha1 = new HMACSHA1(byteKey2);
			_writeMode = writeMode;
		}

		/// <summary>
		/// Implement the ICryptoTransform method.
		/// </summary>
		public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset) {

			// Pass the data stream to the hash algorithm for generating the Auth Code.
			// This does not change the inputBuffer. Do this before decryption for read mode.
			if (!_writeMode) {
				_hmacsha1.TransformBlock(inputBuffer, inputOffset, inputCount, inputBuffer, inputOffset);
			}
			// Encrypt with AES in CTR mode. Regards to Dr Brian Gladman for this.
			int ix = 0;
			while (ix < inputCount) {
				if (_encrPos == ENCRYPT_BLOCK) {
					/* increment encryption nonce   */
					int j = 0;
					while (++_counterNonce[j] == 0) {
						++j;
					}
					/* encrypt the nonce to form next xor buffer    */
					_encryptor.TransformBlock(_counterNonce, 0, _blockSize, _encryptBuffer, 0);
					_encrPos = 0;
				}
				outputBuffer[ix + outputOffset] = (byte)(inputBuffer[ix + inputOffset] ^ _encryptBuffer[_encrPos++]);
				//
				ix++;
			}
			if (_writeMode) {
				// This does not change the buffer.
				_hmacsha1.TransformBlock(outputBuffer, outputOffset, inputCount, outputBuffer, outputOffset);
			}
			return inputCount;
		}

		/// <summary>
		/// Returns the 2 byte password verifier
		/// </summary>
		public byte[] PwdVerifier {
			get {
				return _pwdVerifier;
			}
		}

		/// <summary>
		/// Returns the 10 byte AUTH CODE to be checked or appended immediately following the AES data stream.
		/// </summary>
		public byte[] GetAuthCode() {
			// We usually don't get advance notice of final block. Hash requres a TransformFinal.
			if (!_finalised) {
				byte[] dummy = new byte[0];
				_hmacsha1.TransformFinalBlock(dummy, 0, 0);
				_finalised = true;
			}
			return _hmacsha1.Hash;
		}

		#region ICryptoTransform Members

		/// <summary>
		/// Not implemented.
		/// </summary>
		public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount) {

			throw new NotImplementedException("ZipAESTransform.TransformFinalBlock");
		}

		/// <summary>
		/// Gets the size of the input data blocks in bytes.
		/// </summary>
		public int InputBlockSize {
			get {
				return _blockSize;
			}
		}

		/// <summary>
		/// Gets the size of the output data blocks in bytes.
		/// </summary>
		public int OutputBlockSize {
			get {
				return _blockSize;
			}
		}

		/// <summary>
		/// Gets a value indicating whether multiple blocks can be transformed.
		/// </summary>
		public bool CanTransformMultipleBlocks {
			get {
				return true;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the current transform can be reused.
		/// </summary>
		public bool CanReuseTransform {
			get {
				return true;
			}
		}

		/// <summary>
		/// Cleanup internal state.
		/// </summary>
		public void Dispose() {
			_encryptor.Dispose();
		}

		#endregion

	}
}
#endif