/* Transport Security Layer (TLS)
 * Copyright (c) 2003 Carlos Guzmán Álvarez
 * 
 * Permission is hereby granted, free of charge, to any person 
 * obtaining a copy of this software and associated documentation 
 * files (the "Software"), to deal in the Software without restriction, 
 * including without limitation the rights to use, copy, modify, merge, 
 * publish, distribute, sublicense, and/or sell copies of the Software, 
 * and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included 
 * in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Mono.Security;
using Mono.Security.Cryptography;

namespace Mono.Security.Protocol.Tls
{
	internal class TlsSslCipherSuite : TlsAbstractCipherSuite
	{
		#region CONSTRUCTORS
		
		public TlsSslCipherSuite(short code, string name, string algName, 
			string hashName, bool exportable, bool blockMode, 
			byte keyMaterialSize, byte expandedKeyMaterialSize, 
			short effectiveKeyBytes, byte ivSize, byte blockSize) 
			: base (code, name, algName, hashName, exportable, blockMode,
			keyMaterialSize, expandedKeyMaterialSize, effectiveKeyBytes,
			ivSize, blockSize)
		{
		}

		#endregion

		#region METHODS

		public override byte[] EncryptRecord(byte[] fragment, byte[] mac)
		{
			// Encryption ( fragment + mac [+ padding + padding_length] )
			MemoryStream ms = new MemoryStream();
			CryptoStream cs = new CryptoStream(ms, encryptionCipher, CryptoStreamMode.Write);

			cs.Write(fragment, 0, fragment.Length);
			cs.Write(mac, 0, mac.Length);
			if (cipherMode == CipherMode.CBC)
			{
				// Calculate padding_length
				int fragmentLength	= fragment.Length + mac.Length + 1;
				int paddingLength	= (((fragmentLength/blockSize)*8) + blockSize) - fragmentLength;

				// Write padding length byte
				cs.WriteByte((byte)paddingLength);
			}
			//cs.FlushFinalBlock();
			cs.Close();			

			return ms.ToArray();
		}

		public override void DecryptRecord(byte[] fragment, ref byte[] dcrFragment, ref byte[] dcrMAC)
		{
			int	fragmentSize	= 0;
			int paddingLength	= 0;

			// Decrypt message fragment ( fragment + mac [+ padding + padding_length] )
			byte[] buffer = new byte[fragment.Length];
			decryptionCipher.TransformBlock(fragment, 0, fragment.Length, buffer, 0);

			// Calculate fragment size
			if (cipherMode == CipherMode.CBC)
			{
				// Calculate padding_length
				paddingLength = buffer[buffer.Length - 1];
				for (int i = (buffer.Length - 1); i > (buffer.Length - (paddingLength + 1)); i--)
				{
					if (buffer[i] != paddingLength)
					{
						paddingLength = 0;
						break;
					}
				}

				fragmentSize = (buffer.Length - (paddingLength + 1)) - HashSize;
			}
			else
			{
				fragmentSize = buffer.Length - HashSize;
			}

			dcrFragment = new byte[fragmentSize];
			dcrMAC		= new byte[HashSize];

			Buffer.BlockCopy(buffer, 0, dcrFragment, 0, dcrFragment.Length);
			Buffer.BlockCopy(buffer, dcrFragment.Length, dcrMAC, 0, dcrMAC.Length);
		}

		#endregion

		#region KEY_GENERATION_METODS

		public override void CreateMasterSecret(byte[] preMasterSecret)
		{
			throw new NotSupportedException();
		}

		public override void CreateKeys()
		{
			throw new NotSupportedException();
		}

		#endregion
	}
}