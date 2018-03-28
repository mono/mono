//
// RSAOpenSsl.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2016 Xamarin Inc (http://www.xamarin.com)
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

namespace System.Security.Cryptography
{
	public sealed class RSAOpenSsl : RSA
	{
		public RSAOpenSsl ()
		{
			throw new NotImplementedException ();
		}

		public RSAOpenSsl (int keySize)
		{
			throw new NotImplementedException ();
		}

		public RSAOpenSsl(IntPtr handle)
		{
			throw new NotImplementedException ();
		}

		public RSAOpenSsl (ECCurve curve)
		{
			throw new NotImplementedException ();
		}

		public RSAOpenSsl (RSAParameters parameters)
		{
			throw new NotImplementedException ();
		}

		public RSAOpenSsl (SafeEvpPKeyHandle pkeyHandle)
		{
			throw new NotImplementedException ();
		}

		public override RSAParameters ExportParameters (bool includePrivateParameters)
		{
			throw new NotImplementedException ();
		}

		public override void ImportParameters (RSAParameters parameters)
		{
			throw new NotImplementedException ();
		}

		public override byte[] SignHash (byte[] hash, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding)
		{
			throw new NotImplementedException ();
		}

		public override bool VerifyHash (byte[] hash, byte[] signature, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding)
		{
			throw new NotImplementedException ();
		}

		public SafeEvpPKeyHandle DuplicateKeyHandle ()
		{
			throw new NotImplementedException ();
		}
	}
}