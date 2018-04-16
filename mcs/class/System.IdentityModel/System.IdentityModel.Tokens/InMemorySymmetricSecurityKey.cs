//
// InMemorySymmetricSecurityKey.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006-2007 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.IdentityModel.Policy;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;

using M = Mono.Security.Cryptography;
using AES = System.Security.Cryptography.RijndaelManaged;

namespace System.IdentityModel.Tokens
{
	public class InMemorySymmetricSecurityKey : SymmetricSecurityKey
	{
		byte [] key;

		public InMemorySymmetricSecurityKey (byte [] symmetricKey)
			: this (symmetricKey, true)
		{
		}

		public InMemorySymmetricSecurityKey (byte [] symmetricKey, bool cloneBuffer)
		{
			if (symmetricKey == null)
				throw new ArgumentNullException ("symmetricKey");
			this.key = cloneBuffer ? (byte []) symmetricKey.Clone() : symmetricKey;
		}

		// SymmetricSecurityKey implementation

		public override byte [] GenerateDerivedKey (
			string algorithm, byte [] label, byte [] nonce,
			int derivedKeyLength, int offset)
		{
			if (derivedKeyLength < 0)
				throw new ArgumentOutOfRangeException ("derivedKeyLength");
			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset");
			if (label == null)
				throw new ArgumentNullException ("label");
			if (nonce == null)
				throw new ArgumentNullException ("nonce");
			if (algorithm != SecurityAlgorithms.Psha1KeyDerivation)
				throw new InvalidOperationException (String.Format ("Key derivation algorithm '{0}' is not supported", algorithm));
			byte [] seed = new byte [label.Length + nonce.Length];
			Array.Copy (label, seed, label.Length);
			Array.Copy (nonce, 0, seed, label.Length, nonce.Length);

			byte [] p_sha = Expand ("SHA1", key, seed, derivedKeyLength / 8);

			return p_sha;
		}

		// from Mono.Security.Protocol.Tls.CipherSuite.Expand() with
		// a bit of modification ...
		byte [] Expand (string hashName, byte[] secret, byte[] seed, int length)
		{
			int hashLength	= hashName == "MD5" ? 16 : 20;
			int	iterations	= (int)(length / hashLength);
			if ((length % hashLength) > 0)
			{
				iterations++;
			}
			
			M.HMAC		hmac	= new M.HMAC(hashName, secret);
			MemoryStream	resMacs	= new MemoryStream ();
			
			byte[][] hmacs = new byte[iterations + 1][];
			hmacs[0] = seed;
			for (int i = 1; i <= iterations; i++)
			{				
				MemoryStream hcseed = new MemoryStream ();
				hmac.TransformFinalBlock(hmacs[i-1], 0, hmacs[i-1].Length);
				hmacs[i] = hmac.Hash;
				hcseed.Write(hmacs[i], 0, hmacs [i].Length);
				hcseed.Write(seed, 0, seed.Length);
				hmac.TransformFinalBlock(hcseed.ToArray(), 0, (int)hcseed.Length);
				resMacs.Write(hmac.Hash, 0, hmac.Hash.Length);
			}

			byte[] res = new byte[length];
			
			Buffer.BlockCopy(resMacs.ToArray(), 0, res, 0, res.Length);

			return res;
		}

		public override byte [] GetSymmetricKey ()
		{
			return (byte []) key.Clone ();
		}

		public override KeyedHashAlgorithm GetKeyedHashAlgorithm (
			string algorithm)
		{
			if (algorithm == SecurityAlgorithms.HmacSha1Signature)
				return new HMACSHA1 (key);
			if (algorithm == SecurityAlgorithms.HmacSha256Signature)
				return new HMACSHA256 (key);
			throw new NotSupportedException ();
		}

		public override SymmetricAlgorithm GetSymmetricAlgorithm (string algorithm)
		{
			SymmetricAlgorithm s = null;
			switch (algorithm) {
			case SecurityAlgorithms.Aes128Encryption:
			case SecurityAlgorithms.Aes192Encryption:
			case SecurityAlgorithms.Aes256Encryption:
			case SecurityAlgorithms.Aes128KeyWrap:
			case SecurityAlgorithms.Aes192KeyWrap:
			case SecurityAlgorithms.Aes256KeyWrap:
				s = new AES ();
				break;
			case SecurityAlgorithms.TripleDesEncryption:
			case SecurityAlgorithms.TripleDesKeyWrap:
				if (key.Length == 24)
					throw new CryptographicException ("The key size is 24 bytes, which known as vulnerable and thus not allowed.");
				s = TripleDES.Create ();
				break;
			default:
				throw new NotSupportedException (String.Format ("This symmetric security key does not support specified algorithm '{0}'", algorithm));
			}
			s.Mode = CipherMode.CBC;
			s.GenerateIV ();
			s.Key = key;
			return s;
		}

		public override ICryptoTransform GetDecryptionTransform (string algorithm, byte [] iv)
		{
			if (iv == null)
				throw new ArgumentNullException ("iv");
			SymmetricAlgorithm alg = GetSymmetricAlgorithm (algorithm);
			alg.IV = iv;
			return alg.CreateDecryptor ();
		}

		public override ICryptoTransform GetEncryptionTransform (string algorithm, byte [] iv)
		{
			if (iv == null)
				throw new ArgumentNullException ("iv");
			SymmetricAlgorithm alg = GetSymmetricAlgorithm (algorithm);
			alg.IV = iv;
			return alg.CreateEncryptor ();
		}

		[MonoTODO]
		public override int GetIVSize (string algorithm)
		{
			throw new NotImplementedException ();
		}

		// SecurityKey implementation

		public override int KeySize {
			get { return key.Length << 3; }
		}

		public override byte [] DecryptKey (string algorithm, byte [] keyData)
		{
			if (algorithm == null)
				throw new ArgumentNullException ("algorithm");
			if (keyData == null)
				throw new ArgumentNullException ("keyData");
			return EncryptedXml.DecryptKey (keyData, GetSymmetricAlgorithm (algorithm));
		}

		public override byte [] EncryptKey (string algorithm, byte [] keyData)
		{
			if (algorithm == null)
				throw new ArgumentNullException ("algorithm");
			if (keyData == null)
				throw new ArgumentNullException ("keyData");
			return EncryptedXml.EncryptKey (keyData, GetSymmetricAlgorithm (algorithm));
		}

		public override bool IsAsymmetricAlgorithm (string algorithm)
		{
			return GetAlgorithmSupportType (algorithm) == AlgorithmSupportType.Asymmetric;
		}

		public override bool IsSupportedAlgorithm (string algorithm)
		{
			switch (algorithm) {
			case SecurityAlgorithms.HmacSha1Signature:
			case SecurityAlgorithms.HmacSha256Signature:
			case SecurityAlgorithms.Psha1KeyDerivation:
			case SecurityAlgorithms.Aes128Encryption:
			case SecurityAlgorithms.Aes128KeyWrap:
			case SecurityAlgorithms.Aes192Encryption:
			case SecurityAlgorithms.Aes192KeyWrap:
			case SecurityAlgorithms.Aes256Encryption:
			case SecurityAlgorithms.Aes256KeyWrap:
			case SecurityAlgorithms.TripleDesEncryption:
			case SecurityAlgorithms.TripleDesKeyWrap:
				return true;
			default:
				return false;
			}
		}

		public override bool IsSymmetricAlgorithm (string algorithm)
		{
			return GetAlgorithmSupportType (algorithm) == AlgorithmSupportType.Symmetric;
		}
	}
}
