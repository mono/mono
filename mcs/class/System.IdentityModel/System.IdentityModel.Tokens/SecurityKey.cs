//
// SecurityKey.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.Xml;
using System.IdentityModel.Policy;
using System.Security.Cryptography.Xml;

namespace System.IdentityModel.Tokens
{
	enum AlgorithmSupportType
	{
		Symmetric,
		Asymmetric,
		Unsupported,
	}

	public abstract class SecurityKey
	{
		internal static AlgorithmSupportType GetAlgorithmSupportType (string algorithm)
		{
			switch (algorithm) {
			case SecurityAlgorithms.HmacSha1Signature:
			case SecurityAlgorithms.Psha1KeyDerivation:
			case SecurityAlgorithms.Aes128Encryption:
			case SecurityAlgorithms.Aes128KeyWrap:
			case SecurityAlgorithms.Aes192Encryption:
			case SecurityAlgorithms.Aes192KeyWrap:
			case SecurityAlgorithms.Aes256Encryption:
			case SecurityAlgorithms.Aes256KeyWrap:
			case SecurityAlgorithms.TripleDesEncryption:
			case SecurityAlgorithms.TripleDesKeyWrap:
			case SecurityAlgorithms.DesEncryption:
				return AlgorithmSupportType.Symmetric;
			case SecurityAlgorithms.DsaSha1Signature:
			case SecurityAlgorithms.RsaV15KeyWrap:
			case SecurityAlgorithms.RsaOaepKeyWrap:
			case SecurityAlgorithms.RsaSha1Signature:
			case SecurityAlgorithms.RsaSha256Signature:
				return AlgorithmSupportType.Asymmetric;
			default:
				return AlgorithmSupportType.Unsupported;
			}
		}

		[MonoTODO]
		protected SecurityKey ()
		{
		}

		public abstract int KeySize { get; }

		public abstract byte [] DecryptKey (string algorithm, byte [] keyData);

		public abstract byte [] EncryptKey (string algorithm, byte [] keyData);

		public abstract bool IsAsymmetricAlgorithm (string algorithm);

		public abstract bool IsSupportedAlgorithm (string algorithm);

		public abstract bool IsSymmetricAlgorithm (string algorithm);
	}
}
