// Transport Security Layer (TLS)
// Copyright (c) 2003-2004 Carlos Guzman Alvarez
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using SSCX = System.Security.Cryptography.X509Certificates;
using Mono.Security.X509;
using Mono.Security.X509.Extensions;

namespace Mono.Security.Protocol.Tls.Handshake.Server
{
	internal class TlsClientCertificate : HandshakeMessage
	{
		#region Fields

		private X509CertificateCollection clientCertificates;

		#endregion

		#region Constructors

		public TlsClientCertificate(Context context, byte[] buffer)
			: base(context, HandshakeType.Certificate, buffer)
		{
		}

		#endregion

		#region Methods

		public override void Update()
		{
			foreach (X509Certificate certificate in clientCertificates) {
				this.Context.ClientSettings.Certificates.Add (new SSCX.X509Certificate (certificate.RawData));
			}
		}

		#endregion

		#region Protected Methods

		protected override void ProcessAsSsl3()
		{
			this.ProcessAsTls1();
		}

		protected override void ProcessAsTls1()
		{
			int bytesRead = 0;
			int length = this.ReadInt24 ();
			this.clientCertificates = new X509CertificateCollection ();
			while (length > bytesRead) {
				int certLength = this.ReadInt24 ();
				bytesRead += certLength + 3;
				byte[] cert = this.ReadBytes (certLength);
				this.clientCertificates.Add (new X509Certificate (cert));
			}

			if (this.clientCertificates.Count > 0) 
			{
				this.validateCertificates (this.clientCertificates);
			} 
			else if ((this.Context as ServerContext).ClientCertificateRequired) 
			{
				throw new TlsException (AlertDescription.NoCertificate);
			}
		}

		#endregion

		#region Private Methods

		private bool checkCertificateUsage (X509Certificate cert)
		{
			ServerContext context = (ServerContext)this.Context;

			// certificate extensions are required for this
			// we "must" accept older certificates without proofs
			if (cert.Version < 3)
				return true;

			KeyUsages ku = KeyUsages.none;
			switch (context.Negotiating.Cipher.ExchangeAlgorithmType)
			{
				case ExchangeAlgorithmType.RsaSign:
				case ExchangeAlgorithmType.RsaKeyX:
					ku = KeyUsages.digitalSignature;
					break;
				case ExchangeAlgorithmType.DiffieHellman:
					ku = KeyUsages.keyAgreement;
					break;
				case ExchangeAlgorithmType.Fortezza:
					return false; // unsupported certificate type
			}

			KeyUsageExtension kux = null;
			ExtendedKeyUsageExtension eku = null;

			X509Extension xtn = cert.Extensions["2.5.29.15"];
			if (xtn != null)
				kux = new KeyUsageExtension (xtn);

			xtn = cert.Extensions["2.5.29.37"];
			if (xtn != null)
				eku = new ExtendedKeyUsageExtension (xtn);

			if ((kux != null) && (eku != null))
			{
				// RFC3280 states that when both KeyUsageExtension and 
				// ExtendedKeyUsageExtension are present then BOTH should
				// be valid
				return (kux.Support (ku) &&
					eku.KeyPurpose.Contains ("1.3.6.1.5.5.7.3.2"));
			}
			else if (kux != null)
			{
				return kux.Support (ku);
			}
			else if (eku != null)
			{
				// Client Authentication (1.3.6.1.5.5.7.3.2)
				return eku.KeyPurpose.Contains ("1.3.6.1.5.5.7.3.2");
			}

			// last chance - try with older (deprecated) Netscape extensions
			xtn = cert.Extensions["2.16.840.1.113730.1.1"];
			if (xtn != null)
			{
				NetscapeCertTypeExtension ct = new NetscapeCertTypeExtension (xtn);
				return ct.Support (NetscapeCertTypeExtension.CertTypes.SslClient);
			}

			// certificate isn't valid for SSL server usage
			return false;
		}

		private void validateCertificates (X509CertificateCollection certificates)
		{
			ServerContext context = (ServerContext)this.Context;
			AlertDescription description = AlertDescription.BadCertificate;
			SSCX.X509Certificate client = null;
			int[] certificateErrors = null;

			// note: certificate may be null is no certificate is sent
			// (e.g. optional mutual authentication)
			if (certificates.Count > 0) {
				X509Certificate leaf = certificates[0];
			
				ArrayList errors = new ArrayList ();

				// SSL specific check - not all certificates can be 
				// used to server-side SSL some rules applies after 
				// all ;-)
				if (!checkCertificateUsage (leaf))
				{
					// WinError.h CERT_E_PURPOSE 0x800B0106
					errors.Add ((int)-2146762490);
				}

				X509Chain verify;
				// was a chain supplied ? if so use it, if not
				if (certificates.Count > 1) {
					// if so use it (and don't build our own)
					X509CertificateCollection chain = new X509CertificateCollection (certificates);
					chain.Remove (leaf);
					verify = new X509Chain (chain);
				} else {
					// if not, then let's build our own (based on what's available in the stores)
					verify = new X509Chain ();
				}

				bool result = false;

				try
				{
					result = verify.Build (leaf);
				}
				catch (Exception)
				{
					result = false;
				}

				if (!result)
				{
					switch (verify.Status)
					{
						case X509ChainStatusFlags.InvalidBasicConstraints:
							// WinError.h TRUST_E_BASIC_CONSTRAINTS 0x80096019
							errors.Add ((int)-2146869223);
							break;

						case X509ChainStatusFlags.NotSignatureValid:
							// WinError.h TRUST_E_BAD_DIGEST 0x80096010
							errors.Add ((int)-2146869232);
							break;

						case X509ChainStatusFlags.NotTimeNested:
							// WinError.h CERT_E_VALIDITYPERIODNESTING 0x800B0102
							errors.Add ((int)-2146762494);
							break;

						case X509ChainStatusFlags.NotTimeValid:
							// WinError.h CERT_E_EXPIRED 0x800B0101
							description = AlertDescription.CertificateExpired;
							errors.Add ((int)-2146762495);
							break;

						case X509ChainStatusFlags.PartialChain:
							// WinError.h CERT_E_CHAINING 0x800B010A
							description = AlertDescription.UnknownCA;
							errors.Add ((int)-2146762486);
							break;

						case X509ChainStatusFlags.UntrustedRoot:
							// WinError.h CERT_E_UNTRUSTEDROOT 0x800B0109
							description = AlertDescription.UnknownCA;
							errors.Add ((int)-2146762487);
							break;

						default:
							// unknown error
							description = AlertDescription.CertificateUnknown;
							errors.Add ((int)verify.Status);
							break;
					}
				}
				client = new SSCX.X509Certificate (leaf.RawData);
				certificateErrors = (int[])errors.ToArray (typeof (int));
			}
			else
			{
				certificateErrors = new int[0];
			}

			SSCX.X509CertificateCollection certCollection = new SSCX.X509CertificateCollection ();
			foreach (X509Certificate certificate in certificates) {
				certCollection.Add (new SSCX.X509Certificate (certificate.RawData));
			}
			if (!context.SslStream.RaiseClientCertificateValidation(client, certificateErrors))
			{
				throw new TlsException (
					description,
					"Invalid certificate received from client.");
			}

			this.Context.ClientSettings.ClientCertificate = client;
		}

		#endregion
	}
}
