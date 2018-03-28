//
// X509CertificateImplMono.cs: X.509 implementation using Mono.Security.X509.
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//	Martin Baulig  <martin.baulig@xamarin.com>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2006 Novell, Inc (http://www.novell.com)
// Copyright (C) 2015 Xamarin, Inc. (http://www.xamarin.com)
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
using System.Text;
using MX = Mono.Security.X509;

namespace System.Security.Cryptography.X509Certificates
{
	sealed class X509CertificateImplMono : X509CertificateImpl
	{
		MX.X509Certificate x509;

		public X509CertificateImplMono (MX.X509Certificate x509)
		{
			this.x509 = x509;
		}

		public override bool IsValid {
			get { return x509 != null; }
		}

		public override IntPtr Handle {
			get { return IntPtr.Zero; }
		}

		public override IntPtr GetNativeAppleCertificate ()
		{
			return IntPtr.Zero;
		}

		public override X509CertificateImpl Clone ()
		{
			ThrowIfContextInvalid ();
			return new X509CertificateImplMono (x509);
		}

		public override string GetIssuerName (bool legacyV1Mode)
		{
			ThrowIfContextInvalid ();
			if (legacyV1Mode)
				return x509.IssuerName;
			else
				return MX.X501.ToString (x509.GetIssuerName (), true, ", ", true);
		}

		public override string GetSubjectName (bool legacyV1Mode)
		{
			ThrowIfContextInvalid ();
			if (legacyV1Mode)
				return x509.SubjectName;
			else
				return MX.X501.ToString (x509.GetSubjectName (), true, ", ", true);
		}

		public override byte[] GetRawCertData ()
		{
			ThrowIfContextInvalid ();
			return x509.RawData;
		}

		protected override byte[] GetCertHash (bool lazy)
		{
			ThrowIfContextInvalid ();
			SHA1 sha = SHA1.Create ();
			return sha.ComputeHash (x509.RawData);
		}

		public override DateTime GetValidFrom ()
		{
			ThrowIfContextInvalid ();
			return x509.ValidFrom;
		}

		public override DateTime GetValidUntil ()
		{
			ThrowIfContextInvalid ();
			return x509.ValidUntil;
		}

		public override bool Equals (X509CertificateImpl other, out bool result)
		{
			// Use default implementation
			result = false;
			return false;
		}

		public override string GetKeyAlgorithm () 
		{
			ThrowIfContextInvalid ();
			return x509.KeyAlgorithm;
		}

		public override byte[] GetKeyAlgorithmParameters () 
		{
			ThrowIfContextInvalid ();
			return x509.KeyAlgorithmParameters;
		}

		public override byte[] GetPublicKey ()
		{
			ThrowIfContextInvalid ();
			return x509.PublicKey;
		}

		public override byte[] GetSerialNumber ()
		{
			ThrowIfContextInvalid ();
			return x509.SerialNumber;
		}

		public override byte[] Export (X509ContentType contentType, byte[] password)
		{
			ThrowIfContextInvalid ();

			switch (contentType) {
			case X509ContentType.Cert:
				return GetRawCertData ();
			case X509ContentType.Pfx: // this includes Pkcs12
				// TODO
				throw new NotSupportedException ();
			case X509ContentType.SerializedCert:
				// TODO
				throw new NotSupportedException ();
			default:
				string msg = Locale.GetText ("This certificate format '{0}' cannot be exported.", contentType);
				throw new CryptographicException (msg);
			}
		}

		public override string ToString (bool full)
		{
			ThrowIfContextInvalid ();

			string nl = Environment.NewLine;
			StringBuilder sb = new StringBuilder ();
			sb.AppendFormat ("[Subject]{0}  {1}{0}{0}", nl, GetSubjectName (false));
			sb.AppendFormat ("[Issuer]{0}  {1}{0}{0}", nl, GetIssuerName (false));
			sb.AppendFormat ("[Not Before]{0}  {1}{0}{0}", nl, GetValidFrom ().ToLocalTime ());
			sb.AppendFormat ("[Not After]{0}  {1}{0}{0}", nl, GetValidUntil ().ToLocalTime ());
			sb.AppendFormat ("[Thumbprint]{0}  {1}{0}", nl, X509Helper.ToHexString (GetCertHash ()));
			sb.Append (nl);
			return sb.ToString ();
		}

		protected override void Dispose (bool disposing)
		{
			x509 = null;
		}
	}
}
