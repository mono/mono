//
// X509PalImpl.cs
//
// Author:
//       Martin Baulig <mabaul@microsoft.com>
//
// Copyright (c) 2018 Xamarin, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
using MonoSecurity::Mono.Security;
using MonoSecurity::Mono.Security.Authenticode;
#else
using Mono.Security;
#if !MONOTOUCH_WATCH
using Mono.Security.Authenticode;
#endif
#endif

using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Win32.SafeHandles;

namespace Mono
{
	abstract class X509PalImpl
	{
		public abstract X509CertificateImpl Import (byte[] data);

		public abstract X509Certificate2Impl Import (
			byte[] data, SafePasswordHandle password, X509KeyStorageFlags keyStorageFlags);

		public abstract X509Certificate2Impl Import (X509Certificate cert);

		static byte[] PEM (string type, byte[] data)
		{
			string pem = Encoding.ASCII.GetString (data);
			string header = String.Format ("-----BEGIN {0}-----", type);
			string footer = String.Format ("-----END {0}-----", type);
			int start = pem.IndexOf (header) + header.Length;
			int end = pem.IndexOf (footer, start);
			string base64 = pem.Substring (start, (end - start));
			return Convert.FromBase64String (base64);
		}

		protected static byte[] ConvertData (byte[] data)
		{
			if (data == null || data.Length == 0)
				return data;

			// does it looks like PEM ?
			if (data[0] != 0x30) {
				try {
					return PEM ("CERTIFICATE", data);
				} catch {
					// let the implementation take care of it.
				}
			}
			return data;
		}

		internal X509Certificate2Impl ImportFallback (byte[] data)
		{
			data = ConvertData (data);

			using (var handle = new SafePasswordHandle ((string)null))
				return new X509Certificate2ImplMono (data, handle, X509KeyStorageFlags.DefaultKeySet);
		}

		internal X509Certificate2Impl ImportFallback (byte[] data, SafePasswordHandle password, X509KeyStorageFlags keyStorageFlags)
		{
			return new X509Certificate2ImplMono (data, password, keyStorageFlags);
		}

		public bool SupportsLegacyBasicConstraintsExtension => false;

		static byte[] signedData = new byte[] { 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x07, 0x02 };

		public X509ContentType GetCertContentType (byte[] rawData)
		{
			if ((rawData == null) || (rawData.Length == 0))
				throw new ArgumentException ("rawData");

			if (rawData[0] == 0x30) {
				// ASN.1 SEQUENCE
				try {
					ASN1 data = new ASN1 (rawData);

					// SEQUENCE / SEQUENCE / BITSTRING
					if (data.Count == 3 && data[0].Tag == 0x30 && data[1].Tag == 0x30 && data[2].Tag == 0x03)
						return X509ContentType.Cert;

					// INTEGER / SEQUENCE / SEQUENCE
					if (data.Count == 3 && data[0].Tag == 0x02 && data[1].Tag == 0x30 && data[2].Tag == 0x30)
						return X509ContentType.Pkcs12; // note: Pfx == Pkcs12

					// check for PKCS#7 (count unknown but greater than 0)
					// SEQUENCE / OID (signedData)
					if (data.Count > 0 && data[0].Tag == 0x06 && data[0].CompareValue (signedData))
						return X509ContentType.Pkcs7;

					return X509ContentType.Unknown;
				} catch (Exception) {
					return X509ContentType.Unknown;
				}
			} else {
				string pem = Encoding.ASCII.GetString (rawData);
				int start = pem.IndexOf ("-----BEGIN CERTIFICATE-----");
				if (start >= 0)
					return X509ContentType.Cert;
			}

#if MONOTOUCH_WATCH
				return X509ContentType.Unknown;
#else
			try {
				AuthenticodeDeformatter ad = new AuthenticodeDeformatter (rawData);

				return X509ContentType.Authenticode;
			} catch {
				return X509ContentType.Unknown;
			}
#endif
		}

		public X509ContentType GetCertContentType (string fileName)
		{
			if (fileName == null)
				throw new ArgumentNullException ("fileName");
			if (fileName.Length == 0)
				throw new ArgumentException ("fileName");

			byte[] data = File.ReadAllBytes (fileName);
			return GetCertContentType (data);
		}

	}
}
