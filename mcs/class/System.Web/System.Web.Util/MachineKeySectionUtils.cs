//
// System.Web.Util.MachineKeySectionUtils
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (c) Copyright 2005, 2010 Novell, Inc (http://www.novell.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.ComponentModel;
using System.Configuration;
using System.Configuration.Provider;
using System.Security.Cryptography;
using System.Text;
using System.Web.Configuration;

#if NET_2_0

namespace System.Web.Util {

	static class MachineKeySectionUtils {
		static byte ToHexValue (char c, bool high)
		{
			byte v;
			if (c >= '0' && c <= '9')
				v = (byte) (c - '0');
			else if (c >= 'a' && c <= 'f')
				v = (byte) (c - 'a' + 10);
			else if (c >= 'A' && c <= 'F')
				v = (byte) (c - 'A' + 10);
			else
				throw new ArgumentException ("Invalid hex character");

			if (high)
				v <<= 4;

			return v;
		}

		internal static byte [] GetBytes (string key, int len)
		{
			byte [] result = new byte [len / 2];
			for (int i = 0; i < len; i += 2)
				result [i / 2] = (byte) (ToHexValue (key [i], true) + ToHexValue (key [i + 1], false));

			return result;
		}

		static public string GetHexString (byte [] bytes)
		{
			StringBuilder sb = new StringBuilder (bytes.Length * 2);
			int letterPart = 55;
			const int numberPart = 48;
			for (int i = 0; i < bytes.Length; i++) {
				int tmp = (int) bytes [i];
				int second = tmp & 15;
				int first = (tmp >> 4) & 15;
				sb.Append ((char) (first > 9 ? letterPart + first : numberPart + first));
				sb.Append ((char) (second > 9 ? letterPart + second : numberPart + second));
			}
			return sb.ToString ();
		}


		// decryption="Auto" [Auto | DES | 3DES | AES | alg:algorithm_name]
		// http://msdn.microsoft.com/en-us/library/w8h3skw9.aspx
		public static SymmetricAlgorithm GetDecryptionAlgorithm (string name)
		{
			SymmetricAlgorithm sa = null;
			switch (name) {
			case "AES":
			case "Auto":
				sa = Rijndael.Create ();
				break;
			case "DES":
				sa = DES.Create ();
				break;
			case "3DES":
				sa = TripleDES.Create ();
				break;
			default:
#if NET_4_0
				if (name.StartsWith ("alg:"))
					sa = SymmetricAlgorithm.Create (name.Substring (4));
				else
#endif
					throw new ConfigurationErrorsException ();
				break;
			}
			return sa;
		}

		// validation="HMACSHA256" [SHA1 | MD5 | 3DES | AES | HMACSHA256 | HMACSHA384 | HMACSHA512 | alg:algorithm_name]
		// [1] http://msdn.microsoft.com/en-us/library/system.web.configuration.machinekeyvalidation.aspx
		// [2] http://msdn.microsoft.com/en-us/library/w8h3skw9.aspx
		public static KeyedHashAlgorithm GetValidationAlgorithm (MachineKeySection section)
		{
			KeyedHashAlgorithm kha = null;
			switch (section.Validation) {
			case MachineKeyValidation.MD5:
				kha = new HMACMD5 ();
				break;
			case MachineKeyValidation.AES:		// see link [1] or [2]
			case MachineKeyValidation.TripleDES:	// see link [2]
			case MachineKeyValidation.SHA1:
				kha = new HMACSHA1 ();
				break;
#if NET_4_0
			case MachineKeyValidation.HMACSHA256:
				kha = new HMACSHA256 ();
				break;
			case MachineKeyValidation.HMACSHA384:
				kha = new HMACSHA384 ();
				break;
			case MachineKeyValidation.HMACSHA512:
				kha = new HMACSHA512 ();
				break;
			case MachineKeyValidation.Custom:
				// remove the "alg:" from the start of the string
				string algo = section.ValidationAlgorithm;
				if (algo.StartsWith ("alg:"))
					kha = KeyedHashAlgorithm.Create (algo.Substring (4));
				break;
#endif
			}
			return kha;
		}

		// helpers to ease unit testing of the cryptographic code
#if TEST
		static byte [] decryption_key;
		static byte [] validation_key;

		static SymmetricAlgorithm GetDecryptionAlgorithm (MachineKeySection section)
		{
			return GetDecryptionAlgorithm (section.Decryption);
		}

		static byte [] GetDecryptionKey (MachineKeySection section)
		{
			if (decryption_key == null)
				decryption_key = GetDecryptionAlgorithm (section).Key;
			return decryption_key;
		}

		static byte [] GetValidationKey (MachineKeySection section)
		{
			if (validation_key == null)
				validation_key = GetValidationAlgorithm (section).Key;
			return validation_key;
		}
#else
		static SymmetricAlgorithm GetDecryptionAlgorithm (MachineKeySection section)
		{
			return section.GetDecryptionAlgorithm ();
		}

		static byte[] GetDecryptionKey (MachineKeySection section)
		{
			return section.GetDecryptionKey ();
		}

		static byte [] GetValidationKey (MachineKeySection section)
		{
			return section.GetValidationKey ();
		}
#endif

		static public byte [] Decrypt (MachineKeySection section, byte [] encodedData)
		{
			return Decrypt (section, encodedData, 0, encodedData.Length);
		}

		static byte [] Decrypt (MachineKeySection section, byte [] encodedData, int offset, int length)
		{
			using (SymmetricAlgorithm sa = GetDecryptionAlgorithm (section)) {
				sa.Key = GetDecryptionKey (section);
				return Decrypt (sa, encodedData, offset, length);
			}
		}

		static public byte [] Decrypt (SymmetricAlgorithm alg, byte [] encodedData, int offset, int length)
		{
			// alg.IV is randomly set (default behavior) and perfect for our needs
			// iv is the first part of the encodedPassword
			byte [] iv = new byte [alg.IV.Length];
			Array.Copy (encodedData, 0, iv, 0, iv.Length);
			using (ICryptoTransform decryptor = alg.CreateDecryptor (alg.Key, iv)) {
				try {
					return decryptor.TransformFinalBlock (encodedData, iv.Length + offset, length - iv.Length);
				}
				catch (CryptographicException) {
					return null;
				}
			}
		}

		static public byte [] Encrypt (MachineKeySection section, byte [] data)
		{
			using (SymmetricAlgorithm sa = GetDecryptionAlgorithm (section)) {
				sa.Key = GetDecryptionKey (section);
				return Encrypt (sa, data);
			}
		}

		static public byte [] Encrypt (SymmetricAlgorithm alg, byte [] data)
		{
			// alg.IV is randomly set (default behavior) and perfect for our needs
			byte [] iv = alg.IV;
			using (ICryptoTransform encryptor = alg.CreateEncryptor (alg.Key, iv)) {
				byte [] encrypted = encryptor.TransformFinalBlock (data, 0, data.Length);
				byte [] output = new byte [iv.Length + encrypted.Length];
				// note: the IV can be public, however it should not be based on the password
				Array.Copy (iv, 0, output, 0, iv.Length);
				Array.Copy (encrypted, 0, output, iv.Length, encrypted.Length);
				return output;
			}
		}

		// in		[data]
		// return	[data][signature]
		public static byte [] Sign (MachineKeySection section, byte [] data)
		{
			return Sign (section, data, 0, data.Length);
		}

		static byte [] Sign (MachineKeySection section, byte [] data, int offset, int length)
		{
			using (KeyedHashAlgorithm kha = GetValidationAlgorithm (section)) {
				kha.Key = GetValidationKey (section);
				byte [] signature = kha.ComputeHash (data, offset, length);
				byte [] block = new byte [length + signature.Length];
				Array.Copy (data, block, length);
				Array.Copy (signature, 0, block, length, signature.Length);
				return block;
			}
		}

		public static byte [] Verify (MachineKeySection section, byte [] data)
		{
			byte [] unsigned_data = null;
			bool valid = true;
			using (KeyedHashAlgorithm kha = GetValidationAlgorithm (section)) {
				kha.Key = GetValidationKey (section);
				int signlen = kha.HashSize >> 3; // bits to bytes
				byte [] signature = Sign (section, data, 0, data.Length - signlen);
				for (int i = 0; i < signature.Length; i++) {
					if (signature [i] != data [data.Length - signature.Length + i])
						valid = false; // do not return (timing attack)
				}
				unsigned_data = new byte [data.Length - signlen];
				Array.Copy (data, 0, unsigned_data, 0, unsigned_data.Length);
			}
			return valid ? unsigned_data : null;
		}

		// do NOT sign then encrypt

		public static byte [] EncryptSign (MachineKeySection section, byte [] data)
		{
			byte [] encdata = Encrypt (section, data);
			return Sign (section, encdata);
		}

		// note: take no shortcut (timing attack) while verifying or decrypting
		public static byte [] VerifyDecrypt (MachineKeySection section, byte [] block)
		{
			bool valid = true;
			int signlen;

			using (KeyedHashAlgorithm kha = GetValidationAlgorithm (section)) {
				kha.Key = GetValidationKey (section);
				signlen = kha.HashSize >> 3; // bits to bytes
				byte [] signature = Sign (section, block, 0, block.Length - signlen);
				for (int i = 0; i < signature.Length; i++) {
					if (signature [i] != block [block.Length - signature.Length + i])
						valid = false; // do not return (timing attack)
				}
			}

			// whatever the signature continue with decryption
			try {
				byte [] decdata = Decrypt (section, block, 0, block.Length - signlen);
				return valid ? decdata : null;
			}
			catch {
				return null;
			}
		}
	}
}

#endif
