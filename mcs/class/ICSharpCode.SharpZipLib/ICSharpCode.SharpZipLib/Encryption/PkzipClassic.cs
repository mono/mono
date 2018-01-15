//
// PkzipClassic encryption
//
// Copyright 2004 John Reilly
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

using System;
using System.Security.Cryptography;

using ICSharpCode.SharpZipLib.Checksums;

namespace ICSharpCode.SharpZipLib.Encryption
{
	/// <summary>
	/// PkzipClassic embodies the classic or original encryption facilities used in Pkzip archives.
	/// While it has been superceded by more recent and more powerful algorithms, its still in use and 
	/// is viable for preventing casual snooping
	/// </summary>
	[System.ObsoleteAttribute("This assembly has been deprecated. Please use https://www.nuget.org/packages/SharpZipLib/ instead.")]
	public abstract class PkzipClassic  : SymmetricAlgorithm
	{
		/// <summary>
		/// Generates new encryption keys based on given seed
		/// </summary>
		static public byte[] GenerateKeys(byte[] seed)
		{
			if ( seed == null ) 
			{
				throw new ArgumentNullException("seed");
			}

			if ( seed.Length == 0 )
			{
				throw new ArgumentException("seed");
			}

			uint[] newKeys = new uint[] {
			                            0x12345678,
			                            0x23456789,
			                            0x34567890
			                         };
			
			for (int i = 0; i < seed.Length; ++i) 
			{
				newKeys[0] = Crc32.ComputeCrc32(newKeys[0], seed[i]);
				newKeys[1] = newKeys[1] + (byte)newKeys[0];
				newKeys[1] = newKeys[1] * 134775813 + 1;
				newKeys[2] = Crc32.ComputeCrc32(newKeys[2], (byte)(newKeys[1] >> 24));
			}

			byte[] result = new byte[12];
			result[0] = (byte)(newKeys[0] & 0xff);
			result[1] = (byte)((newKeys[0] >> 8) & 0xff);
			result[2] = (byte)((newKeys[0] >> 16) & 0xff);
			result[3] = (byte)((newKeys[0] >> 24) & 0xff);
			result[4] = (byte)(newKeys[1] & 0xff);
			result[5] = (byte)((newKeys[1] >> 8) & 0xff);
			result[6] = (byte)((newKeys[1] >> 16) & 0xff);
			result[7] = (byte)((newKeys[1] >> 24) & 0xff);
			result[8] = (byte)(newKeys[2] & 0xff);
			result[9] = (byte)((newKeys[2] >> 8) & 0xff);
			result[10] = (byte)((newKeys[2] >> 16) & 0xff);
			result[11] = (byte)((newKeys[2] >> 24) & 0xff);
			return result;
		}
	}

	/// <summary>
	/// PkzipClassicCryptoBase provides the low level facilities for encryption
	/// and decryption using the PkzipClassic algorithm.
	/// </summary>
	class PkzipClassicCryptoBase
	{
		uint[] keys     = null;

		/// <summary>
		/// Transform a single byte 
		/// </summary>
		/// <returns>
		/// The transformed value
		/// </returns>
		protected byte TransformByte()
		{
			uint temp = ((keys[2] & 0xFFFF) | 2);
			return (byte)((temp * (temp ^ 1)) >> 8);
		}

		protected void SetKeys(byte[] keyData)
		{
			if ( keyData == null ) {
				throw new ArgumentNullException("keyData");
			}
		
			if ( keyData.Length != 12 ) {
				throw new InvalidOperationException("Keys not valid");
			}
			
			keys = new uint[3];
			keys[0] = (uint)((keyData[3] << 24) | (keyData[2] << 16) | (keyData[1] << 8) | keyData[0]);
			keys[1] = (uint)((keyData[7] << 24) | (keyData[6] << 16) | (keyData[5] << 8) | keyData[4]);
			keys[2] = (uint)((keyData[11] << 24) | (keyData[10] << 16) | (keyData[9] << 8) | keyData[8]);
		}

		/// <summary>
		/// Update encryption keys 
		/// </summary>		
		protected void UpdateKeys(byte ch)
		{
			keys[0] = Crc32.ComputeCrc32(keys[0], ch);
			keys[1] = keys[1] + (byte)keys[0];
			keys[1] = keys[1] * 134775813 + 1;
			keys[2] = Crc32.ComputeCrc32(keys[2], (byte)(keys[1] >> 24));
		}

		/// <summary>
		/// Reset the internal state.
		/// </summary>
		protected void Reset()
		{
			keys[0] = 0;
			keys[1] = 0;
			keys[2] = 0;
		}
	}

	/// <summary>
	/// PkzipClassic CryptoTransform for encryption.
	/// </summary>
	class PkzipClassicEncryptCryptoTransform : PkzipClassicCryptoBase, ICryptoTransform
	{
		/// <summary>
		/// Initialise a new instance of <see cref="PkzipClassicEncryptCryptoTransform"></see>
		/// </summary>
		/// <param name="keyBlock">The key block to use.</param>
		internal PkzipClassicEncryptCryptoTransform(byte[] keyBlock)
		{
			SetKeys(keyBlock);
		}

		#region ICryptoTransform Members

		/// <summary>
		/// Transforms the specified region of the specified byte array.
		/// </summary>
		/// <param name="inputBuffer">The input for which to compute the transform.</param>
		/// <param name="inputOffset">The offset into the byte array from which to begin using data.</param>
		/// <param name="inputCount">The number of bytes in the byte array to use as data.</param>
		/// <returns>The computed transform.</returns>
		public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
		{
			byte[] result = new byte[inputCount];
			TransformBlock(inputBuffer, inputOffset, inputCount, result, 0);
			return result;
		}

		/// <summary>
		/// Transforms the specified region of the input byte array and copies 
		/// the resulting transform to the specified region of the output byte array.
		/// </summary>
		/// <param name="inputBuffer">The input for which to compute the transform.</param>
		/// <param name="inputOffset">The offset into the input byte array from which to begin using data.</param>
		/// <param name="inputCount">The number of bytes in the input byte array to use as data.</param>
		/// <param name="outputBuffer">The output to which to write the transform.</param>
		/// <param name="outputOffset">The offset into the output byte array from which to begin writing data.</param>
		/// <returns>The number of bytes written.</returns>
		public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
		{
			for (int i = inputOffset; i < inputOffset + inputCount; ++i) {
				byte oldbyte = inputBuffer[i];
				outputBuffer[outputOffset++] = (byte)(inputBuffer[i] ^ TransformByte());
				UpdateKeys(oldbyte);
			}
			return inputCount;
		}

		/// <summary>
		/// Gets a value indicating whether the current transform can be reused.
		/// </summary>
		public bool CanReuseTransform
		{
			get {
				return true;
			}
		}

		/// <summary>
		/// Gets the size of the input data blocks in bytes.
		/// </summary>
		public int InputBlockSize
		{
			get {
				return 1;
			}
		}

		/// <summary>
		/// Gets the size of the output data blocks in bytes.
		/// </summary>
		public int OutputBlockSize
		{
			get {
				return 1;
			}
		}

		/// <summary>
		/// Gets a value indicating whether multiple blocks can be transformed.
		/// </summary>
		public bool CanTransformMultipleBlocks
		{
			get {
				return true;
			}
		}

		#endregion

		#region IDisposable Members

		/// <summary>
		/// Cleanup internal state.
		/// </summary>
		public void Dispose()
		{
			Reset();
		}

		#endregion
	}


	/// <summary>
	/// PkzipClassic CryptoTransform for decryption.
	/// </summary>
	class PkzipClassicDecryptCryptoTransform : PkzipClassicCryptoBase, ICryptoTransform
	{
		/// <summary>
		/// Initialise a new instance of <see cref="PkzipClassicDecryptCryptoTransform"></see>.
		/// </summary>
		/// <param name="keyBlock">The key block to decrypt with.</param>
		internal PkzipClassicDecryptCryptoTransform(byte[] keyBlock)
		{
			SetKeys(keyBlock);
		}

		#region ICryptoTransform Members

		/// <summary>
		/// Transforms the specified region of the specified byte array.
		/// </summary>
		/// <param name="inputBuffer">The input for which to compute the transform.</param>
		/// <param name="inputOffset">The offset into the byte array from which to begin using data.</param>
		/// <param name="inputCount">The number of bytes in the byte array to use as data.</param>
		/// <returns>The computed transform.</returns>
		public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
		{
			byte[] result = new byte[inputCount];
			TransformBlock(inputBuffer, inputOffset, inputCount, result, 0);
			return result;
		}

		/// <summary>
		/// Transforms the specified region of the input byte array and copies 
		/// the resulting transform to the specified region of the output byte array.
		/// </summary>
		/// <param name="inputBuffer">The input for which to compute the transform.</param>
		/// <param name="inputOffset">The offset into the input byte array from which to begin using data.</param>
		/// <param name="inputCount">The number of bytes in the input byte array to use as data.</param>
		/// <param name="outputBuffer">The output to which to write the transform.</param>
		/// <param name="outputOffset">The offset into the output byte array from which to begin writing data.</param>
		/// <returns>The number of bytes written.</returns>
		public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
		{
			for (int i = inputOffset; i < inputOffset + inputCount; ++i) {
				byte newByte = (byte)(inputBuffer[i] ^ TransformByte());
				outputBuffer[outputOffset++] = newByte;
				UpdateKeys(newByte);
			}
			return inputCount;
		}

		/// <summary>
		/// Gets a value indicating whether the current transform can be reused.
		/// </summary>
		public bool CanReuseTransform
		{
			get {
				return true;
			}
		}

		/// <summary>
		/// Gets the size of the input data blocks in bytes.
		/// </summary>
		public int InputBlockSize
		{
			get {
				return 1;
			}
		}

		/// <summary>
		/// Gets the size of the output data blocks in bytes.
		/// </summary>
		public int OutputBlockSize
		{
			get {
				return 1;
			}
		}

		/// <summary>
		/// Gets a value indicating whether multiple blocks can be transformed.
		/// </summary>
		public bool CanTransformMultipleBlocks
		{
			get {
				return true;
			}
		}

		#endregion

		#region IDisposable Members

		/// <summary>
		/// Cleanup internal state.
		/// </summary>
		public void Dispose()
		{
			Reset();
		}

		#endregion
	}

	/// <summary>
	/// Defines a wrapper object to access the Pkzip algorithm. 
	/// This class cannot be inherited.
	/// </summary>
	[System.ObsoleteAttribute("This assembly has been deprecated. Please use https://www.nuget.org/packages/SharpZipLib/ instead.")]
	public sealed class PkzipClassicManaged : PkzipClassic
	{
		/// <summary>
		/// Get / set the applicable block size.
		/// </summary>
		/// <remarks>The only valid block size is 8.</remarks>
		public override int BlockSize 
		{
			get { return 8; }
			set {
				if (value != 8)
					throw new CryptographicException();
			}
		}

		/// <summary>
		/// Get an array of legal <see cref="KeySizes">key sizes.</see>
		/// </summary>
		public override KeySizes[] LegalKeySizes
		{
			get {
				KeySizes[] keySizes = new KeySizes[1];
				keySizes[0] = new KeySizes(12 * 8, 12 * 8, 0);
				return keySizes; 
			}
		}

		/// <summary>
		/// Generate an initial vector.
		/// </summary>
		public override void GenerateIV()
		{
			// Do nothing.
		}

		/// <summary>
		/// Get an array of legal <see cref="KeySizes">block sizes</see>.
		/// </summary>
		public override KeySizes[] LegalBlockSizes
		{
			get {
				KeySizes[] keySizes = new KeySizes[1];
				keySizes[0] = new KeySizes(1 * 8, 1 * 8, 0);
				return keySizes; 
			}
		}

		byte[] key;

		/// <summary>
		/// Get / set the key value applicable.
		/// </summary>
		public override byte[] Key
		{
			get {
				return key;
			}
		
			set {
				key = value;
			}
		}

		/// <summary>
		/// Generate a new random key.
		/// </summary>
		public override void GenerateKey()
		{
			key = new byte[12];
			Random rnd = new Random();
			rnd.NextBytes(key);
		}

		/// <summary>
		/// Create an encryptor.
		/// </summary>
		/// <param name="rgbKey">The key to use for this encryptor.</param>
		/// <param name="rgbIV">Initialisation vector for the new encryptor.</param>
		/// <returns>Returns a new PkzipClassic encryptor</returns>
		public override ICryptoTransform CreateEncryptor(
			byte[] rgbKey,
			byte[] rgbIV
		)
		{
			return new PkzipClassicEncryptCryptoTransform(rgbKey);
		}

		/// <summary>
		/// Create a decryptor.
		/// </summary>
		/// <param name="rgbKey">Keys to use for this new decryptor.</param>
		/// <param name="rgbIV">Initialisation vector for the new decryptor.</param>
		/// <returns>Returns a new decryptor.</returns>
		public override ICryptoTransform CreateDecryptor(
			byte[] rgbKey,
			byte[] rgbIV
		)
		{
			return new PkzipClassicDecryptCryptoTransform(rgbKey);
		}
	}
}
