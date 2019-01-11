//
// DSACertificateExtensions.cs
//
// Authors:
//	Filip Navara <filip.navara@gmail.com>
//
// Copyright (C) 2018 .NET Foundation and Contributors
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

namespace System.Security.Cryptography.X509Certificates
{
	public static class DSACertificateExtensions
	{
		/// <summary>
		/// Gets the <see cref="DSA" /> public key from the certificate or null if the certificate does not have a DSA public key.
		/// </summary>
		public static DSA GetDSAPublicKey(this X509Certificate2 certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException("certificate");
			return certificate.PrivateKey as DSA;
		}

		/// <summary>
		/// Gets the <see cref="DSA" /> private key from the certificate or null if the certificate does not have a DSA private key.
		/// </summary>
		public static DSA GetDSAPrivateKey(this X509Certificate2 certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException("certificate");
			return certificate.PublicKey.Key as DSA;
		}

		[MonoTODO]
		public static X509Certificate2 CopyWithPrivateKey(this X509Certificate2 certificate, DSA privateKey)
		{
			if (certificate == null)
				throw new ArgumentNullException(nameof(certificate));
			if (privateKey == null)
				throw new ArgumentNullException(nameof(privateKey));

			throw new NotImplementedException ();
		}
	}
}
