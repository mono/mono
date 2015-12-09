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

		public abstract X509CertificateImpl Clone ();

		public abstract string GetIssuerName (bool legacyV1Mode);

		public abstract string GetSubjectName (bool legacyV1Mode);

		public abstract byte[] GetRawCertData ();

		public abstract byte[] GetCertHash ();

		public abstract DateTime GetEffectiveDateString ();

		public abstract DateTime GetExpirationDateString ();

		public abstract bool Equals (X509CertificateImpl other, out bool result);

		public abstract string GetKeyAlgorithm ();

		public abstract byte[] GetKeyAlgorithmParameters ();

		public abstract byte[] GetPublicKey ();

		public abstract byte[] GetSerialNumber ();

		public abstract byte[] Export (X509ContentType contentType, byte[] password);

		public abstract string ToString (bool full);

		public static bool Equals (X509CertificateImpl first, X509CertificateImpl second)
		{
			if (!first.IsValid || !second.IsValid)
				return false;

			bool result;
			if (first.Equals (second, out result))
				return result;

			var firstRaw = first.GetRawCertData ();
			var secondRaw = second.GetRawCertData ();

			if (firstRaw == null)
				return secondRaw == null;
			else if (secondRaw == null)
				return false;

			if (firstRaw.Length != secondRaw.Length)
				return false;

			for (int i = 0; i < firstRaw.Length; i++) {
				if (firstRaw [i] != secondRaw [i])
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
