//
// PrivateKey.cs - Private Key (PVK) Format Implementation
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using Mono.Security.Cryptography;

namespace Mono.Security.Authenticode {

	// References:
	// a.	http://www.drh-consultancy.demon.co.uk/pvk.html

#if INSIDE_SYSTEM
	internal
#else
	public
#endif
	class PrivateKey {

		private const uint magic = 0xb0b5f11e;

		private bool encrypted;
		private RSA rsa;
		private bool weak;
		private int keyType;

		public PrivateKey () 
		{
			keyType = 2;	// required for MS makecert !!!
		}

		public PrivateKey (byte[] data, string password) 
		{
			if (data == null)
				throw new ArgumentNullException ("data");

			if (!Decode (data, password)) {
				throw new CryptographicException (
					Locale.GetText ("Invalid data and/or password"));
			}
		}

		public bool Encrypted {
			get { return encrypted; }
		}

		public int KeyType {
			get { return keyType; }
			set { keyType = value; }
		}

		public RSA RSA {
			get { return rsa; }
			set { rsa = value; }
		}

		public bool Weak {
			get { return ((encrypted) ? weak : true); }
			set { weak = value; }
		}

		private byte[] DeriveKey (byte[] salt, string password) 
		{
			byte[] pwd = Encoding.ASCII.GetBytes (password);
			SHA1 sha1 = (SHA1)SHA1.Create ();
			sha1.TransformBlock (salt, 0, salt.Length, salt, 0);
			sha1.TransformFinalBlock (pwd, 0, pwd.Length);
			byte[] key = new byte [16];
			Buffer.BlockCopy (sha1.Hash, 0, key, 0, 16);
			sha1.Clear ();
			Array.Clear (pwd, 0, pwd.Length);
			return key;	
		}

		private bool Decode (byte[] pvk, string password) 
		{
			// DWORD magic
			if (BitConverterLE.ToUInt32 (pvk, 0) != magic)
				return false;
			// DWORD reserved
			if (BitConverterLE.ToUInt32 (pvk, 4) != 0x0)
				return false;
			// DWORD keytype
			keyType = BitConverterLE.ToInt32 (pvk, 8);
			// DWORD encrypted
			encrypted = (BitConverterLE.ToUInt32 (pvk, 12) == 1);
			// DWORD saltlen
			int saltlen = BitConverterLE.ToInt32 (pvk, 16);
			// DWORD keylen
			int keylen = BitConverterLE.ToInt32 (pvk, 20);
			byte[] keypair = new byte [keylen];
			Buffer.BlockCopy (pvk, 24 + saltlen, keypair, 0, keylen);
			// read salt (if present)
			if (saltlen > 0) {
				if (password == null)
					return false;

				byte[] salt = new byte [saltlen];
				Buffer.BlockCopy (pvk, 24, salt, 0, saltlen);
				// first try with full (128) bits
				byte[] key = DeriveKey (salt, password);
				// decrypt in place and try this
				RC4 rc4 = RC4.Create ();
				ICryptoTransform dec = rc4.CreateDecryptor (key, null);
				dec.TransformBlock (keypair, 8, keypair.Length - 8, keypair, 8);
				try {
					rsa = CryptoConvert.FromCapiPrivateKeyBlob (keypair);
					weak = false;
				}
				catch (CryptographicException) {
					weak = true;
					// second chance using weak crypto
					Buffer.BlockCopy (pvk, 24 + saltlen, keypair, 0, keylen);
					// truncate the key to 40 bits
					Array.Clear (key, 5, 11);
					// decrypt
					RC4 rc4b = RC4.Create ();
					dec = rc4b.CreateDecryptor (key, null);
					dec.TransformBlock (keypair, 8, keypair.Length - 8, keypair, 8);
					rsa = CryptoConvert.FromCapiPrivateKeyBlob (keypair);
				}
				Array.Clear (key, 0, key.Length);
			}
			else  {
				weak = true;
				// read unencrypted keypair
				rsa = CryptoConvert.FromCapiPrivateKeyBlob (keypair);
				Array.Clear (keypair, 0, keypair.Length);
			}

			// zeroize pvk (which could contain the unencrypted private key)
			Array.Clear (pvk, 0, pvk.Length);
			
			return (rsa != null);
		}

		public void Save (string filename) 
		{
			Save (filename, null);
		}

		public void Save (string filename, string password) 
		{
			if (filename == null)
				throw new ArgumentNullException ("filename");

			byte[] blob = null;
			FileStream fs = File.Open (filename, FileMode.Create, FileAccess.Write);
			try {
				// header
				byte[] empty = new byte [4];
				byte[] data = BitConverterLE.GetBytes (magic);
				fs.Write (data, 0, 4);	// magic
				fs.Write (empty, 0, 4);	// reserved
				data = BitConverterLE.GetBytes (keyType);
				fs.Write (data, 0, 4);	// key type

				encrypted = (password != null);
				blob = CryptoConvert.ToCapiPrivateKeyBlob (rsa);
				if (encrypted) {
					data = BitConverterLE.GetBytes (1);
					fs.Write (data, 0, 4);	// encrypted
					data = BitConverterLE.GetBytes (16);
					fs.Write (data, 0, 4);	// saltlen
					data = BitConverterLE.GetBytes (blob.Length);
					fs.Write (data, 0, 4);		// keylen

					byte[] salt = new byte [16];
					RC4 rc4 = RC4.Create ();
					byte[] key = null;
					try {
						// generate new salt (16 bytes)
						RandomNumberGenerator rng = RandomNumberGenerator.Create ();
						rng.GetBytes (salt);
						fs.Write (salt, 0, salt.Length);
						key = DeriveKey (salt, password);
						if (Weak)
							Array.Clear (key, 5, 11);
						ICryptoTransform enc = rc4.CreateEncryptor (key, null);
						// we don't encrypt the header part of the BLOB
						enc.TransformBlock (blob, 8, blob.Length - 8, blob, 8);
					}
					finally {
						Array.Clear (salt, 0, salt.Length);
						Array.Clear (key, 0, key.Length);
						rc4.Clear ();
					}
				}
				else {
					fs.Write (empty, 0, 4);	// encrypted
					fs.Write (empty, 0, 4);	// saltlen
					data = BitConverterLE.GetBytes (blob.Length);
					fs.Write (data, 0, 4);		// keylen
				}
		
				fs.Write (blob, 0, blob.Length);
			}
			finally {
				// BLOB may include an uncrypted keypair
				Array.Clear (blob, 0, blob.Length);
				fs.Close ();
			}
		}

		static public PrivateKey CreateFromFile (string filename) 
		{
			return CreateFromFile (filename, null);
		}

		static public PrivateKey CreateFromFile (string filename, string password) 
		{
			if (filename == null)
				throw new ArgumentNullException ("filename");

			byte[] pvk = null;				
			using (FileStream fs = File.Open (filename, FileMode.Open, FileAccess.Read, FileShare.Read)) {
				pvk = new byte [fs.Length];
				fs.Read (pvk, 0, pvk.Length);
				fs.Close ();
			}
			return new PrivateKey (pvk, password);
		}
	}
}
