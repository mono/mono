//
// RsaCertificateExtensions.cs
//
// Authors:
//	Alexander Köplinger <alexander.koeplinger@xamarin.com>
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

namespace System.Security.Cryptography.X509Certificates
{
	public static class RSACertificateExtensions
	{
		public static RSA GetRSAPrivateKey (this X509Certificate2 certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException (nameof (certificate));

			if (!certificate.HasPrivateKey)
				return null;

			return certificate.Impl.GetRSAPrivateKey ();
		}

		public static RSA GetRSAPublicKey (this X509Certificate2 certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException (nameof (certificate));
			return certificate.PublicKey.Key as RSA;
		}
		
		public static X509Certificate2 CopyWithPrivateKey (this X509Certificate2 certificate, RSA privateKey)
		{
			if (certificate == null)
				throw new ArgumentNullException (nameof (certificate));
			if (privateKey == null)
				throw new ArgumentNullException (nameof (privateKey));
			var impl = certificate.Impl.CopyWithPrivateKey (privateKey);
			return (X509Certificate2)impl.CreateCertificate ();
		}
	}
}
