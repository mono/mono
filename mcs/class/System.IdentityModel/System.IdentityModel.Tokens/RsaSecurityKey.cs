//
// AsymmetricSecurityKey.cs
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
using System.Security.Cryptography;

namespace System.IdentityModel.Tokens
{
	public sealed class RsaSecurityKey : AsymmetricSecurityKey
	{
		public RsaSecurityKey (RSA rsa)
		{
			this.rsa = rsa;
		}

		RSA rsa;

		// AsymmetricSecurityKey implementation

		[MonoTODO]
		public override AsymmetricAlgorithm GetAsymmetricAlgorithm (
			string algorithm, bool requiresPrivateKey)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override HashAlgorithm GetHashAlgorithmForSignature (
			string algorithm)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override AsymmetricSignatureDeformatter GetSignatureDeformatter (string algorithm)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override AsymmetricSignatureFormatter GetSignatureFormatter (string algorithm)
		{
			throw new NotImplementedException ();
		}

		public override bool HasPrivateKey ()
		{
			RSACryptoServiceProvider rcsp = rsa as RSACryptoServiceProvider;
			if (rcsp != null)
				return !rcsp.PublicOnly;
			try {
				rcsp.ExportParameters (true);
				return true;
			} catch (CryptographicException) {
				return false;
			}
		}

		// SecurityKey implementation

		public override int KeySize {
			get { return rsa.KeySize; }
		}

		[MonoTODO]
		public override byte [] DecryptKey (string algorithm, byte [] keyData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override byte [] EncryptKey (string algorithm, byte [] keyData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool IsAsymmetricAlgorithm (string algorithm)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool IsSupportedAlgorithm (string algorithm)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool IsSymmetricAlgorithm (string algorithm)
		{
			throw new NotImplementedException ();
		}
	}
}
