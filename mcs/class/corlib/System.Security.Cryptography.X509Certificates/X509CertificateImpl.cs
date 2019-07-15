//
// X509Helpers.cs: X.509 helper and utility functions.
//
// Authors:
//	Martin Baulig  <martin.baulig@xamarin.com>
//
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
using Microsoft.Win32.SafeHandles;
namespace System.Security.Cryptography.X509Certificates
{
	internal abstract class X509CertificateImpl : IDisposable
	{
		public abstract bool IsValid {
			get;
		}

		public abstract IntPtr Handle {
			get;
		}

		/*
		 * This is used in System.dll's OSX509Certificates.cs
		 */
		public abstract IntPtr GetNativeAppleCertificate ();

		protected void ThrowIfContextInvalid ()
		{
			if (!IsValid)
				throw X509Helper.GetInvalidContextException ();
		}

		public abstract X509CertificateImpl Clone ();

		public abstract string Issuer {
			get;
		}

		public abstract string Subject {
			get;
		}

		public abstract string LegacyIssuer {
			get;
		}

		public abstract string LegacySubject {
			get;
		}

		public abstract byte[] RawData {
			get;
		}

		public abstract DateTime NotAfter {
			get;
		}

		public abstract DateTime NotBefore {
			get;
		}

		public abstract byte[] Thumbprint {
			get;
		}

		public sealed override int GetHashCode ()
		{
			if (!IsValid)
				return 0;
			byte[] thumbPrint = Thumbprint;
			int value = 0;
			for (int i = 0; i < thumbPrint.Length && i < 4; ++i) {
				value = value << 8 | thumbPrint[i];
			}
			return value;
		}

		public abstract bool Equals (X509CertificateImpl other, out bool result);

		public abstract string KeyAlgorithm {
			get;
		}

		public abstract byte[] KeyAlgorithmParameters {
			get;
		}

		public abstract byte[] PublicKeyValue {
			get;
		}

		public abstract byte[] SerialNumber {
			get;
		}

		public abstract bool HasPrivateKey {
			get;
		}

		public abstract RSA GetRSAPrivateKey ();

		public abstract DSA GetDSAPrivateKey ();

		public abstract byte[] Export (X509ContentType contentType, SafePasswordHandle password);

		public abstract X509CertificateImpl CopyWithPrivateKey (RSA privateKey);

		public abstract X509Certificate CreateCertificate ();

		public sealed override bool Equals (object obj)
		{
			var other = obj as X509CertificateImpl;
			if (other == null)
				return false;

			if (!IsValid || !other.IsValid)
				return false;

			if (!Issuer.Equals (other.Issuer))
				return false;

			byte[] thisSerialNumber = SerialNumber;
			byte[] otherSerialNumber = other.SerialNumber;

			if (thisSerialNumber.Length != otherSerialNumber.Length)
				return false;
			for (int i = 0; i < thisSerialNumber.Length; i++) {
				if (thisSerialNumber[i] != otherSerialNumber[i])
					return false;
			}

			return true;
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
		}

		~X509CertificateImpl ()
		{
			Dispose (false);
		}
	}
}
