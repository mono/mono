// Transport Security Layer (TLS)
// Copyright (c) 2003-2004 Carlos Guzman Alvarez
// Copyright (C) 2004, 2006-2010 Novell, Inc (http://www.novell.com)
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
using System.Net;
using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using X509Cert = System.Security.Cryptography.X509Certificates;

using Mono.Security.X509;
using Mono.Security.X509.Extensions;

namespace Mono.Security.Protocol.Tls.Handshake.Client
{
	internal class TlsServerCertificate : HandshakeMessage
	{
		#region Fields

		private X509CertificateCollection certificates;
		
		#endregion

		#region Constructors

		public TlsServerCertificate(Context context, byte[] buffer) 
			: base(context, HandshakeType.Certificate, buffer)
		{
		}

		#endregion

		#region Methods

		public override void Update()
		{
			base.Update();
			this.Context.ServerSettings.Certificates = this.certificates;
			this.Context.ServerSettings.UpdateCertificateRSA();
		}

		#endregion

		#region Protected Methods

		protected override void ProcessAsSsl3()
		{
			this.ProcessAsTls1();
		}

		protected override void ProcessAsTls1()
		{
			this.certificates = new X509CertificateCollection();
			
			int readed	= 0;
			int length	= this.ReadInt24();

			while (readed < length)
			{
				// Read certificate length
				int certLength = ReadInt24();

				// Increment readed
				readed += 3;

				if (certLength > 0)
				{
					// Read certificate data
					byte[] buffer = this.ReadBytes(certLength);

					// Create a new X509 Certificate
					X509Certificate certificate = new X509Certificate(buffer);
					certificates.Add(certificate);

					readed += certLength;

					DebugHelper.WriteLine(
						String.Format("Server Certificate {0}", certificates.Count),
						buffer);
				}
			}

			this.validateCertificates(certificates);
		}

		#endregion

		#region Private Methods

		// Note: this method only works for RSA certificates
		// DH certificates requires some changes - does anyone use one ?
		private bool checkCertificateUsage (X509Certificate cert) 
		{
			ClientContext context = (ClientContext)this.Context;

			// certificate extensions are required for this
			// we "must" accept older certificates without proofs
			if (cert.Version < 3)
				return true;

			KeyUsages ku = KeyUsages.none;
			switch (context.Negotiating.Cipher.ExchangeAlgorithmType) 
			{
				case ExchangeAlgorithmType.RsaSign:
					ku = KeyUsages.digitalSignature;
					break;
				case ExchangeAlgorithmType.RsaKeyX:
					ku = KeyUsages.keyEncipherment;
					break;
				case ExchangeAlgorithmType.DiffieHellman:
					ku = KeyUsages.keyAgreement;
					break;
				case ExchangeAlgorithmType.Fortezza:
					return false; // unsupported certificate type
			}

			KeyUsageExtension kux = null;
			ExtendedKeyUsageExtension eku = null;

			X509Extension xtn = cert.Extensions ["2.5.29.15"];
			if (xtn != null)
				kux = new KeyUsageExtension (xtn);

			xtn = cert.Extensions ["2.5.29.37"];
			if (xtn != null)
				eku = new ExtendedKeyUsageExtension (xtn);

			if ((kux != null) && (eku != null)) 
			{
				// RFC3280 states that when both KeyUsageExtension and 
				// ExtendedKeyUsageExtension are present then BOTH should
				// be valid
				if (!kux.Support (ku))
					return false;
				return (eku.KeyPurpose.Contains ("1.3.6.1.5.5.7.3.1") ||
					eku.KeyPurpose.Contains ("2.16.840.1.113730.4.1"));
			}
			else if (kux != null) 
			{
				return kux.Support (ku);
			}
			else if (eku != null) 
			{
				// Server Authentication (1.3.6.1.5.5.7.3.1) or
				// Netscape Server Gated Crypto (2.16.840.1.113730.4)
				return (eku.KeyPurpose.Contains ("1.3.6.1.5.5.7.3.1") ||
					eku.KeyPurpose.Contains ("2.16.840.1.113730.4.1"));
			}

			// last chance - try with older (deprecated) Netscape extensions
			xtn = cert.Extensions ["2.16.840.1.113730.1.1"];
			if (xtn != null) 
			{
				NetscapeCertTypeExtension ct = new NetscapeCertTypeExtension (xtn);
				return ct.Support (NetscapeCertTypeExtension.CertTypes.SslServer);
			}

			// if the CN=host (checked later) then we assume this is meant for SSL/TLS
			// e.g. the new smtp.gmail.com certificate
			return true;
		}

		
		static private void VerifyOSX (X509CertificateCollection certificates)
		{
			
		}
		
		private void validateCertificates(X509CertificateCollection certificates)
		{
			ClientContext		context			= (ClientContext)this.Context;
			AlertDescription	description		= AlertDescription.BadCertificate;

#if NET_2_0
			if (context.SslStream.HaveRemoteValidation2Callback) {
				ValidationResult res = context.SslStream.RaiseServerCertificateValidation2 (certificates);
				if (res.Trusted)
					return;

				long error = res.ErrorCode;
				switch (error) {
				case 0x800B0101:
					description = AlertDescription.CertificateExpired;
					break;
				case 0x800B010A:
					description = AlertDescription.UnknownCA;
					break;
				case 0x800B0109:
					description = AlertDescription.UnknownCA;
					break;
				default:
					description = AlertDescription.CertificateUnknown;
					break;
				}
				string err = String.Format ("0x{0:x}", error);
				throw new TlsException (description, "Invalid certificate received from server. Error code: " + err);
			}
#endif
			// the leaf is the web server certificate
			X509Certificate leaf = certificates [0];
			X509Cert.X509Certificate cert = new X509Cert.X509Certificate (leaf.RawData);

			ArrayList errors = new ArrayList();

			// SSL specific check - not all certificates can be 
			// used to server-side SSL some rules applies after 
			// all ;-)
			if (!checkCertificateUsage (leaf)) 
			{
				// WinError.h CERT_E_PURPOSE 0x800B0106
				errors.Add ((int)-2146762490);
			}

			// SSL specific check - does the certificate match 
			// the host ?
			if (!checkServerIdentity (leaf))
			{
				// WinError.h CERT_E_CN_NO_MATCH 0x800B010F
				errors.Add ((int)-2146762481);
			}

			// Note: building and verifying a chain can take much time
			// so we do it last (letting simple things fails first)

			// Note: In TLS the certificates MUST be in order (and
			// optionally include the root certificate) so we're not
			// building the chain using LoadCertificate (it's faster)

			// Note: IIS doesn't seem to send the whole certificate chain
			// but only the server certificate :-( it's assuming that you
			// already have this chain installed on your computer. duh!
			// http://groups.google.ca/groups?q=IIS+server+certificate+chain&hl=en&lr=&ie=UTF-8&oe=UTF-8&selm=85058s%24avd%241%40nnrp1.deja.com&rnum=3

			// we must remove the leaf certificate from the chain
			X509CertificateCollection chain = new X509CertificateCollection (certificates);
			chain.Remove (leaf);
			X509Chain verify = new X509Chain (chain);

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

			int[] certificateErrors = (int[])errors.ToArray(typeof(int));

			if (!context.SslStream.RaiseServerCertificateValidation(
				cert, 
				certificateErrors))
			{
				throw new TlsException(
					description,
					"Invalid certificate received from server.");
			}
		}

		// RFC2818 - HTTP Over TLS, Section 3.1
		// http://www.ietf.org/rfc/rfc2818.txt
		// 
		// 1.	if present MUST use subjectAltName dNSName as identity
		// 1.1.		if multiples entries a match of any one is acceptable
		// 1.2.		wildcard * is acceptable
		// 2.	URI may be an IP address -> subjectAltName.iPAddress
		// 2.1.		exact match is required
		// 3.	Use of the most specific Common Name (CN=) in the Subject
		// 3.1		Existing practice but DEPRECATED
		private bool checkServerIdentity (X509Certificate cert) 
		{
			ClientContext context = (ClientContext)this.Context;

			string targetHost = context.ClientSettings.TargetHost;

			X509Extension ext = cert.Extensions ["2.5.29.17"];
			// 1. subjectAltName
			if (ext != null) 
			{
				SubjectAltNameExtension subjectAltName = new SubjectAltNameExtension (ext);
				// 1.1 - multiple dNSName
				foreach (string dns in subjectAltName.DNSNames) 
				{
					// 1.2 TODO - wildcard support
					if (Match (targetHost, dns))
						return true;
				}
				// 2. ipAddress
				foreach (string ip in subjectAltName.IPAddresses) 
				{
					// 2.1. Exact match required
					if (ip == targetHost)
						return true;
				}
			}
			// 3. Common Name (CN=)
			return checkDomainName (cert.SubjectName);
		}

		private bool checkDomainName(string subjectName)
		{
			ClientContext context = (ClientContext)this.Context;

			string	domainName = String.Empty;
			Regex search = new Regex(@"CN\s*=\s*([^,]*)");

			MatchCollection	elements = search.Matches(subjectName);

			if (elements.Count == 1)
			{
				if (elements[0].Success)
				{
					domainName = elements[0].Groups[1].Value.ToString();
				}
			}

			return Match (context.ClientSettings.TargetHost, domainName);
		}

		// ensure the pattern is valid wrt to RFC2595 and RFC2818
		// http://www.ietf.org/rfc/rfc2595.txt
		// http://www.ietf.org/rfc/rfc2818.txt
		static bool Match (string hostname, string pattern)
		{
			// check if this is a pattern
			int index = pattern.IndexOf ('*');
			if (index == -1) {
				// not a pattern, do a direct case-insensitive comparison
				return (String.Compare (hostname, pattern, true, CultureInfo.InvariantCulture) == 0);
			}

			// check pattern validity
			// A "*" wildcard character MAY be used as the left-most name component in the certificate.

			// unless this is the last char (valid)
			if (index != pattern.Length - 1) {
				// then the next char must be a dot .'.
				if (pattern [index + 1] != '.')
					return false;
			}

			// only one (A) wildcard is supported
			int i2 = pattern.IndexOf ('*', index + 1);
			if (i2 != -1)
				return false;

			// match the end of the pattern
			string end = pattern.Substring (index + 1);
			int length = hostname.Length - end.Length;
			// no point to check a pattern that is longer than the hostname
			if (length <= 0)
				return false;

			if (String.Compare (hostname, length, end, 0, end.Length, true, CultureInfo.InvariantCulture) != 0)
				return false;

			// special case, we start with the wildcard
			if (index == 0) {
				// ensure we hostname non-matched part (start) doesn't contain a dot
				int i3 = hostname.IndexOf ('.');
				return ((i3 == -1) || (i3 >= (hostname.Length - end.Length)));
			}

			// match the start of the pattern
			string start = pattern.Substring (0, index);
			return (String.Compare (hostname, 0, start, 0, start.Length, true, CultureInfo.InvariantCulture) == 0);
		}

		#endregion
	}
}
