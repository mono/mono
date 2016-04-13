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

		public abstract string GetIssuerName (bool legacyV1Mode);

		public abstract string GetSubjectName (bool legacyV1Mode);

		public abstract byte[] GetRawCertData ();

		public abstract DateTime GetValidFrom ();

		public abstract DateTime GetValidUntil ();

		byte[] cachedCertificateHash;

		public byte[] GetCertHash ()
		{
			ThrowIfContextInvalid ();
			if (cachedCertificateHash == null)
				cachedCertificateHash = GetCertHash (false);
			return cachedCertificateHash;
		}

		protected abstract byte[] GetCertHash (bool lazy);

		public override int GetHashCode ()
		{
			if (!IsValid)
				return 0;
			if (cachedCertificateHash == null)
				cachedCertificateHash = GetCertHash (true);
			// return the integer of the first 4 bytes of the cert hash
			if ((cachedCertificateHash != null) && (cachedCertificateHash.Length >= 4))
				return ((cachedCertificateHash [0] << 24) | (cachedCertificateHash [1] << 16) |
					(cachedCertificateHash [2] << 8) | cachedCertificateHash [3]);
			else
				return 0;
		}

		public abstract bool Equals (X509CertificateImpl other, out bool result);

		public abstract string GetKeyAlgorithm ();

		public abstract byte[] GetKeyAlgorithmParameters ();

		public abstract byte[] GetPublicKey ();

		public abstract byte[] GetSerialNumber ();

		public abstract byte[] Export (X509ContentType contentType, byte[] password);

		public abstract string ToString (bool full);

		public override bool Equals (object obj)
		{
			var other = obj as X509CertificateImpl;
			if (other == null)
				return false;

			if (!IsValid || !other.IsValid)
				return false;

			bool result;
			if (Equals (other, out result))
				return result;

			var ourRaw = GetRawCertData ();
			var theirRaw = other.GetRawCertData ();

			if (ourRaw == null)
				return theirRaw == null;
			else if (theirRaw == null)
				return false;

			if (ourRaw.Length != theirRaw.Length)
				return false;

			for (int i = 0; i < ourRaw.Length; i++) {
				if (ourRaw [i] != theirRaw [i])
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
			cachedCertificateHash = null;
		}

		~X509CertificateImpl ()
		{
			Dispose (false);
		}
	}
}
