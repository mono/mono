//
// StrongName.cs - Strong Name Implementation
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography;

using Mono.Security.Cryptography;

namespace Mono.Security {

	public class StrongName {

		private RSA rsa;
		private byte[] publicKey;
		private byte[] keyToken;
		private string tokenAlgorithm;

		public StrongName () {}

		public StrongName (byte[] data)
		{
			RSA = CryptoConvert.FromCapiKeyBlob (data);
		}

		public StrongName (RSA rsa) : base ()
		{
			RSA = rsa;
		}

		private void InvalidateCache () 
		{
			publicKey = null;
			keyToken = null;
		}

		public RSA RSA {
			get {
				// if none then we create a new keypair
				if (rsa == null)
					rsa = RSA.Create ();
				return rsa; 
			}
			set { 
				rsa = value;
				InvalidateCache ();
			}
		}

		public byte[] PublicKey {
			get { 
				if (publicKey == null) {
					byte[] keyPair = CryptoConvert.ToCapiKeyBlob (rsa, false); 
					publicKey = new byte [32 + 128]; // always 1024 bits

					// The first 12 bytes are documented at:
					// http://msdn.microsoft.com/library/en-us/cprefadd/html/grfungethashfromfile.asp
					// ALG_ID - Signature
					publicKey [0] = keyPair [4];
					publicKey [1] = keyPair [5];	
					publicKey [2] = keyPair [6];	
					publicKey [3] = keyPair [7];	
					// ALG_ID - Hash (SHA1 == 0x8004)
					publicKey [4] = 0x04;
					publicKey [5] = 0x80;
					publicKey [6] = 0x00;
					publicKey [7] = 0x00;
					// Length of Public Key (in bytes)
					byte[] lastPart = BitConverter.GetBytes (publicKey.Length - 12);
					publicKey [8] = lastPart [0];
					publicKey [9] = lastPart [1];
					publicKey [10] = lastPart [2];
					publicKey [11] = lastPart [3];
					// Ok from here - Same structure as keypair - expect for public key
					publicKey [12] = 0x06;		// PUBLICKEYBLOB
					// we can copy this part
					Array.Copy (keyPair, 1, publicKey, 13, publicKey.Length - 13);
					// and make a small adjustment 
					publicKey [23] = 0x31;		// (RSA1 not RSA2)
				}
				return publicKey;
			}
		}

		public byte[] PublicKeyToken {
			get {
				if (keyToken != null)
					return keyToken;
				byte[] publicKey = PublicKey;
				if (publicKey == null)
					return null;
                                HashAlgorithm ha = SHA1.Create (TokenAlgorithm);
				byte[] hash = ha.ComputeHash (publicKey);
				// we need the last 8 bytes in reverse order
				keyToken = new byte [8];
				Array.Copy (hash, (hash.Length - 8), keyToken, 0, 8);
				Array.Reverse (keyToken, 0, 8);
				return keyToken;
			}
		}

		public string TokenAlgorithm {
			get { 
				if (tokenAlgorithm == null)
					tokenAlgorithm = "SHA1";
				return tokenAlgorithm; 
			}
			set {
				string algo = value.ToUpper ();
				if ((algo == "SHA1") || (algo == "MD5")) {
					tokenAlgorithm = value;
					InvalidateCache ();
				}
				else
					throw new ArgumentException ("Unsupported hash algorithm for token");
			}
		}

		public byte[] GetBytes () 
		{
			return CryptoConvert.ToCapiPrivateKeyBlob (rsa);
		}

		// TODO
		public byte[] Hash (string fileName) 
		{
			return null;
		}

		// TODO
		public bool Sign (string fileName) 
		{
			return false;
		}

		// TODO
		public bool Verify (string fileName) 
		{
			return false;
		}
	}	
}
