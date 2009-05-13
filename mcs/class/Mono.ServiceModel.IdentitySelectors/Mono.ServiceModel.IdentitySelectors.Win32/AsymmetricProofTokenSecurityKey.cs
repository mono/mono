//
// AsymmetricProofTokenSecurityKey.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.  http://www.novell.com
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
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Xml;

namespace Mono.ServiceModel.IdentitySelectors.Win32
{
	class AsymmetricProofTokenSecurityKey : AsymmetricSecurityKey, IDisposable
	{
		NativeAsymmetricCryptoParameters parameters;
		NativeInfocardCryptoHandle handle;

		public AsymmetricProofTokenSecurityKey (NativeAsymmetricCryptoParameters parameters, NativeInfocardCryptoHandle handle)
		{
			this.parameters = parameters;
			this.handle = handle;
		}

		void IDisposable.Dispose ()
		{
			CloseCryptoHandle (handle);
		}

		public override AsymmetricAlgorithm GetAsymmetricAlgorithm (string algorithm, bool privateKey)
		{
			throw new NotImplementedException ();
		}

		public override HashAlgorithm GetHashAlgorithmForSignature (string algorithm)
		{
			throw new NotImplementedException ();
		}

		public override AsymmetricSignatureFormatter GetSignatureFormatter (string algorithm)
		{
			throw new NotImplementedException ();
		}

		public override AsymmetricSignatureDeformatter GetSignatureDeformatter (string algorithm)
		{
			throw new NotImplementedException ();
		}

		public override byte [] DecryptKey (string algorithm, byte [] input)
		{
			throw new NotImplementedException ();
		}

		public override byte [] EncryptKey (string algorithm, byte [] input)
		{
			throw new NotImplementedException ();
		}

		public override bool IsAsymmetricAlgorithm (string algorithm)
		{
			throw new NotImplementedException ();
		}

		public override bool IsSymmetricAlgorithm (string algorithm)
		{
			throw new NotImplementedException ();
		}

		public override bool IsSupportedAlgorithm (string algorithm)
		{
			throw new NotImplementedException ();
		}

		public override bool HasPrivateKey ()
		{
			return true;
		}

		public override int KeySize {
			get { return parameters.KeySize; }
		}

		[DllImport ("cardspaceapi")]
		static extern void CloseCryptoHandle (NativeInfocardCryptoHandle handle);
	}
}
