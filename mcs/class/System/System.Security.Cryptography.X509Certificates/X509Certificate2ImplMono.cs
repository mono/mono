//
// X509Certificate2ImplMono
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//	Martin Baulig  <martin.baulig@xamarin.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2006 Novell Inc. (http://www.novell.com)
// Copyright (C) 2015-2016 Xamarin, Inc. (http://www.xamarin.com)
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

#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
using MonoSecurity::Mono.Security;
using MonoSecurity::Mono.Security.Cryptography;
using MonoSecurity::Mono.Security.Authenticode;
using MX = MonoSecurity::Mono.Security.X509;
#else
using Mono.Security;
using Mono.Security.Cryptography;
#if !MONOTOUCH_WATCH
using Mono.Security.Authenticode;
#endif
using MX = Mono.Security.X509;
#endif

using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace System.Security.Cryptography.X509Certificates
{
	internal class X509Certificate2ImplMono : X509Certificate2ImplUnix
	{
		PublicKey _publicKey;
		X509CertificateImplCollection intermediateCerts;

		MX.X509Certificate _cert;

		static string empty_error = Locale.GetText ("Certificate instance is empty.");

		public override bool IsValid {
			get {
				return _cert != null;
			}
		}

		public override IntPtr Handle {
			get { return IntPtr.Zero; }
		}

		public override IntPtr GetNativeAppleCertificate ()
		{
			return IntPtr.Zero;
		}

		public X509Certificate2ImplMono (MX.X509Certificate cert)
		{
			this._cert = cert;
		}

		X509Certificate2ImplMono (X509Certificate2ImplMono other)
		{
			_cert = other._cert;
			if (other.intermediateCerts != null)
				intermediateCerts = other.intermediateCerts.Clone ();
		}

		public X509Certificate2ImplMono (byte[] rawData, SafePasswordHandle password, X509KeyStorageFlags keyStorageFlags)
		{
			switch (X509Certificate2.GetCertContentType (rawData)) {
			case X509ContentType.Pkcs12:
				_cert = ImportPkcs12 (rawData, password);
				break;

			case X509ContentType.Cert:
			case X509ContentType.Pkcs7:
				_cert = new MX.X509Certificate (rawData);
				break;

#if !MONOTOUCH_WATCH
			case X509ContentType.Authenticode:
				AuthenticodeDeformatter ad = new AuthenticodeDeformatter (rawData);
				_cert = ad.SigningCertificate;
				if (_cert == null)
					goto default;
				break;
#endif

			default:
				string msg = Locale.GetText ("Unable to decode certificate.");
				throw new CryptographicException (msg);
			}
		}

		public override X509CertificateImpl Clone ()
		{
			ThrowIfContextInvalid ();
			return new X509Certificate2ImplMono (this);
		}

		MX.X509Certificate Cert {
			get {
				ThrowIfContextInvalid ();
				return _cert;
			}
		}


		#region Implemented X509CertificateImpl members

		protected override byte[] GetRawCertData ()
		{
			ThrowIfContextInvalid ();
			return Cert.RawData;
		}

		public override bool Equals (X509CertificateImpl other, out bool result)
		{
			// Use default implementation
			result = false;
			return false;
		}

#endregion

		// constructors

		public X509Certificate2ImplMono ()
		{
			_cert = null;
		}

		// properties

		// FIXME - Could be more efficient
		public override bool HasPrivateKey {
			get { return PrivateKey != null; }
		}

		public override AsymmetricAlgorithm PrivateKey {
			get {
				if (_cert == null)
					throw new CryptographicException (empty_error);
				try {
					if (_cert.RSA is RSACryptoServiceProvider rcsp) {
						if (rcsp.PublicOnly)
							return null;
						var key = new RSACryptoServiceProvider ();
						key.ImportParameters (_cert.RSA.ExportParameters (true));
						return key;
					}
					if (_cert.RSA is RSAManaged rsam) {
						if (rsam.PublicOnly)
							return null;
						var key = new RSAManaged ();
						key.ImportParameters (_cert.RSA.ExportParameters (true));
						return key;
					}
					if (_cert.DSA is DSACryptoServiceProvider dcsp) {
						if (dcsp.PublicOnly)
							return null;
						var key = new DSACryptoServiceProvider ();
						key.ImportParameters (_cert.DSA.ExportParameters (true));
						return key;
					}
				} catch {
				}
				return null;
			}
			set {
				if (_cert == null)
					throw new CryptographicException (empty_error);

				// allow NULL so we can "forget" the key associated to the certificate
				// e.g. in case we want to export it in another format (see bug #396620)
				if (value == null) {
					_cert.RSA = null;
					_cert.DSA = null;
				} else if (value is RSA)
					_cert.RSA = (RSA)value;
				else if (value is DSA)
					_cert.DSA = (DSA)value;
				else
					throw new NotSupportedException ();
			}
		}

		public override RSA GetRSAPrivateKey ()
		{
			return PrivateKey as RSA;
		}

		public override DSA GetDSAPrivateKey ()
		{
			return PrivateKey as DSA;
		}

		public override PublicKey PublicKey {
			get {
				if (_cert == null)
					throw new CryptographicException (empty_error);

				if (_publicKey == null) {
					try {
						_publicKey = new PublicKey (_cert);
					} catch (Exception e) {
						string msg = Locale.GetText ("Unable to decode public key.");
						throw new CryptographicException (msg, e);
					}
				}
				return _publicKey;
			}
		}

		// methods

		MX.X509Certificate ImportPkcs12 (byte[] rawData, SafePasswordHandle password)
		{
			if (password == null || password.IsInvalid)
				return ImportPkcs12 (rawData, (string)null);
			var passwordString = password.Mono_DangerousGetString ();
			return ImportPkcs12 (rawData, passwordString);
		}

		MX.X509Certificate ImportPkcs12 (byte[] rawData, string password)
		{
			MX.PKCS12 pfx = null;
			if (string.IsNullOrEmpty (password)) {
				try {
					// Support both unencrypted PKCS#12..
					pfx = new MX.PKCS12 (rawData, (string)null);
				} catch {
					// ..and PKCS#12 encrypted with an empty password
					pfx = new MX.PKCS12 (rawData, string.Empty);
				}
			} else {
				pfx = new MX.PKCS12 (rawData, password);
			}

			if (pfx.Certificates.Count == 0) {
				// no certificate was found
				return null;
			} else if (pfx.Keys.Count == 0) {
				// no key were found - pick the first certificate
				return pfx.Certificates [0];
			} else {
				// find the certificate that match the first key
				MX.X509Certificate cert = null;
				var keypair = (pfx.Keys [0] as AsymmetricAlgorithm);
				string pubkey = keypair.ToXmlString (false);
				foreach (var c in pfx.Certificates) {
					if (((c.RSA != null) && (pubkey == c.RSA.ToXmlString (false))) ||
						((c.DSA != null) && (pubkey == c.DSA.ToXmlString (false)))) {
						cert = c;
						break;
					}
				}
				if (cert == null) {
					cert = pfx.Certificates [0]; // no match, pick first certificate without keys
				} else {
					cert.RSA = (keypair as RSA);
					cert.DSA = (keypair as DSA);
				}
				if (pfx.Certificates.Count > 1) {
					intermediateCerts = new X509CertificateImplCollection ();
					foreach (var c in pfx.Certificates) {
						if (c == cert)
							continue;
						var impl = new X509Certificate2ImplMono (c);
						intermediateCerts.Add (impl, true);
					}
				}
				return cert;
			}
		}

		public override void Reset () 
		{
			_cert = null;
			_publicKey = null;
			if (intermediateCerts != null) {
				intermediateCerts.Dispose ();
				intermediateCerts = null;
			}
		}

		[MonoTODO ("by default this depends on the incomplete X509Chain")]
		public override bool Verify (X509Certificate2 thisCertificate)
		{
			if (_cert == null)
				throw new CryptographicException (empty_error);

			X509Chain chain = X509Chain.Create ();
			if (!chain.Build (thisCertificate))
				return false;
			// TODO - check chain and other stuff ???
			return true;
		}

		// static methods

		private static byte[] signedData = new byte[] { 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x07, 0x02 };

		[MonoTODO ("Detection limited to Cert, Pfx, Pkcs12, Pkcs7 and Unknown")]
		public static X509ContentType GetCertContentType (byte[] rawData)
		{
			if ((rawData == null) || (rawData.Length == 0))
				throw new ArgumentException ("rawData");

			X509ContentType type = X509ContentType.Unknown;
			try {
				ASN1 data = new ASN1 (rawData);
				if (data.Tag != 0x30) {
					string msg = Locale.GetText ("Unable to decode certificate.");
					throw new CryptographicException (msg);
				}

				if (data.Count == 0)
					return type;

				if (data.Count == 3) {
					switch (data [0].Tag) {
					case 0x30:
						// SEQUENCE / SEQUENCE / BITSTRING
						if ((data [1].Tag == 0x30) && (data [2].Tag == 0x03))
							type = X509ContentType.Cert;
						break;
					case 0x02:
						// INTEGER / SEQUENCE / SEQUENCE
						if ((data [1].Tag == 0x30) && (data [2].Tag == 0x30))
							type = X509ContentType.Pkcs12;
						// note: Pfx == Pkcs12
						break;
					}
				}
				// check for PKCS#7 (count unknown but greater than 0)
				// SEQUENCE / OID (signedData)
				if ((data [0].Tag == 0x06) && data [0].CompareValue (signedData))
					type = X509ContentType.Pkcs7;
			}
			catch (Exception e) {
				string msg = Locale.GetText ("Unable to decode certificate.");
				throw new CryptographicException (msg, e);
			}

			return type;
		}

		[MonoTODO ("Detection limited to Cert, Pfx, Pkcs12 and Unknown")]
		public static X509ContentType GetCertContentType (string fileName)
		{
			if (fileName == null)
				throw new ArgumentNullException ("fileName");
			if (fileName.Length == 0)
				throw new ArgumentException ("fileName");

			byte[] data = File.ReadAllBytes (fileName);
			return GetCertContentType (data);
		}

		internal override X509CertificateImplCollection IntermediateCertificates {
			get { return intermediateCerts; }
		}

		// internal stuff because X509Certificate2 isn't complete enough
		// (maybe X509Certificate3 will be better?)

		internal MX.X509Certificate MonoCertificate {
			get { return _cert; }
		}

		internal override X509Certificate2Impl FallbackImpl {
			get { return this; }
		}
	}
}
