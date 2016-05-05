//
// X509Helpers.cs: X.509 helper and utility functions.
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
using System.Threading;
using System.Runtime.InteropServices;
#if !NET_2_1
using System.Security.Permissions;
#endif
using MX = Mono.Security.X509;

namespace System.Security.Cryptography.X509Certificates
{
	static partial class X509Helper
	{
		static INativeCertificateHelper nativeHelper;

		internal static void InstallNativeHelper (INativeCertificateHelper helper)
		{
			if (nativeHelper == null)
				Interlocked.CompareExchange (ref nativeHelper, helper, null);
		}

#if !NET_2_1
		// typedef struct _CERT_CONTEXT {
		//	DWORD                   dwCertEncodingType;
		//	BYTE                    *pbCertEncoded;
		//	DWORD                   cbCertEncoded;
		//	PCERT_INFO              pCertInfo;
		//	HCERTSTORE              hCertStore;
		// } CERT_CONTEXT, *PCERT_CONTEXT;
		// typedef const CERT_CONTEXT *PCCERT_CONTEXT;
		[StructLayout (LayoutKind.Sequential)]
		internal struct CertificateContext {
			public UInt32 dwCertEncodingType;
			public IntPtr pbCertEncoded;
			public UInt32 cbCertEncoded;
			public IntPtr pCertInfo;
			public IntPtr hCertStore;
		}
		// NOTE: We only define the CryptoAPI structure (from WINCRYPT.H)
		// so we don't create any dependencies on Windows DLL in corlib

		[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
		public static X509CertificateImpl InitFromHandle (IntPtr handle)
		{
			// both Marshal.PtrToStructure and Marshal.Copy use LinkDemand (so they will always success from here)
			CertificateContext cc = (CertificateContext) Marshal.PtrToStructure (handle, typeof (CertificateContext));
			byte[] data = new byte [cc.cbCertEncoded];
			Marshal.Copy (cc.pbCertEncoded, data, 0, (int)cc.cbCertEncoded);
			var x509 = new MX.X509Certificate (data);
			return new X509CertificateImplMono (x509);
		}
#elif !MONOTOUCH && !XAMMAC
		public static X509CertificateImpl InitFromHandle (IntPtr handle)
		{
			throw new NotSupportedException ();
		}
#endif

		public static X509CertificateImpl InitFromCertificate (X509Certificate cert)
		{
			if (nativeHelper != null)
				return nativeHelper.Import (cert);

			return InitFromCertificate (cert.Impl);
		}

		public static X509CertificateImpl InitFromCertificate (X509CertificateImpl impl)
		{
			ThrowIfContextInvalid (impl);
			var copy = impl.Clone ();
			if (copy != null)
				return copy;

			var data = impl.GetRawCertData ();
			if (data == null)
				return null;

			var x509 = new MX.X509Certificate (data);
			return new X509CertificateImplMono (x509);
		}

		public static bool IsValid (X509CertificateImpl impl)
		{
			return impl != null && impl.IsValid;
		}

		internal static void ThrowIfContextInvalid (X509CertificateImpl impl)
		{
			if (!IsValid (impl))
				throw GetInvalidContextException ();
		}

		internal static Exception GetInvalidContextException ()
		{
			return new CryptographicException (Locale.GetText ("Certificate instance is empty."));
		}

		internal static MX.X509Certificate ImportPkcs12 (byte[] rawData, string password)
		{
			var pfx = (password == null) ? new MX.PKCS12 (rawData) : new MX.PKCS12 (rawData, password);
			if (pfx.Certificates.Count == 0) {
				// no certificate was found
				return null;
			} else if (pfx.Keys.Count == 0) {
				// no key were found - pick the first certificate
				return pfx.Certificates [0];
			} else {
				// find the certificate that match the first key
				var keypair = (pfx.Keys [0] as AsymmetricAlgorithm);
				string pubkey = keypair.ToXmlString (false);
				foreach (var c in pfx.Certificates) {
					if ((c.RSA != null) && (pubkey == c.RSA.ToXmlString (false)))
						return c;
					if ((c.DSA != null) && (pubkey == c.DSA.ToXmlString (false)))
						return c;
				}
				return pfx.Certificates [0]; // no match, pick first certificate without keys
			}
		}

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

		static byte[] ConvertData (byte[] data)
		{
			if (data == null || data.Length == 0)
				return data;

			// does it looks like PEM ?
			if (data [0] != 0x30) {
				try {
					return PEM ("CERTIFICATE", data);
				} catch {
					// let the implementation take care of it.
				}
			}
			return data;
		}

#if !MONOTOUCH && !XAMMAC
		static X509CertificateImpl Import (byte[] rawData)
		{
			MX.X509Certificate x509;
			try {
				x509 = new MX.X509Certificate (rawData);
			} catch (Exception e) {
				try {
					x509 = ImportPkcs12 (rawData, null);
				} catch {
					string msg = Locale.GetText ("Unable to decode certificate.");
					// inner exception is the original (not second) exception
					throw new CryptographicException (msg, e);
				}
			}

			return new X509CertificateImplMono (x509);
		}
#endif

		public static X509CertificateImpl Import (byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
		{
			if (password == null) {
				rawData = ConvertData (rawData);
				return Import (rawData);
			}

			MX.X509Certificate x509;
			// try PKCS#12
			try {
				x509 = ImportPkcs12 (rawData, password);
			} catch {
				// it's possible to supply a (unrequired/unusued) password
				// fix bug #79028
				x509 = new MX.X509Certificate (rawData);
			}

			return new X509CertificateImplMono (x509);
		}

		public static byte[] Export (X509CertificateImpl impl, X509ContentType contentType, byte[] password)
		{
			ThrowIfContextInvalid (impl);
			return impl.Export (contentType, password);
		}

		public static bool Equals (X509CertificateImpl first, X509CertificateImpl second)
		{
			if (!IsValid (first) || !IsValid (second))
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

		// almost every byte[] returning function has a string equivalent
		// sadly the BitConverter insert dash between bytes :-(
		public static string ToHexString (byte[] data)
		{
			if (data != null) {
				StringBuilder sb = new StringBuilder ();
				for (int i = 0; i < data.Length; i++)
					sb.Append (data[i].ToString ("X2"));
				return sb.ToString ();
			}
			else
				return null;
		}
	}
}
