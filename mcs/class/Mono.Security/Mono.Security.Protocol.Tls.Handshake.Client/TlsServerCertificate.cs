/* Transport Security Layer (TLS)
 * Copyright (c) 2003 Carlos Guzmán Álvarez
 * 
 * Permission is hereby granted, free of charge, to any person 
 * obtaining a copy of this software and associated documentation 
 * files (the "Software"), to deal in the Software without restriction, 
 * including without limitation the rights to use, copy, modify, merge, 
 * publish, distribute, sublicense, and/or sell copies of the Software, 
 * and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included 
 * in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using X509Cert = System.Security.Cryptography.X509Certificates;

using Mono.Security.Protocol.Tls.Alerts;
using Mono.Security.X509;

namespace Mono.Security.Protocol.Tls.Handshake.Client
{
	internal class TlsServerCertificate : TlsHandshakeMessage
	{
		#region FIELDS

		private X509CertificateCollection	certificates;
		
		#endregion

		#region CONSTRUCTORS

		public TlsServerCertificate(TlsContext context, byte[] buffer) 
			: base(context, TlsHandshakeType.Certificate, buffer)
		{
		}

		#endregion

		#region METHODS

		public override void UpdateSession()
		{
			base.UpdateSession();
			this.Context.ServerSettings.Certificates = certificates;
		}

		#endregion

		#region PROTECTED_METHODS

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
					// Read certificate
					X509Certificate certificate = new X509Certificate(this.ReadBytes(certLength));
					certificates.Add(certificate);

					readed += certLength;

					this.validateCertificate(certificate);
				}
			}
		}

		#endregion

		#region  PRIVATE_METHODS

		private void validateCertificate(X509Certificate certificate)
		{
			int[] certificateErrors = new int[0];

			// 1 step : Validate dates
			if (!certificate.IsCurrent)
			{
				#warning "Add error to the list"
			}

			// 2 step: Validate CA
			

			// 3 step: Validate digital sign
			/*
			if (!certificate.VerifySignature(certificate.RSA))
			{
				throw this.Context.CreateException("Certificate received from the server has invalid signature.");
			}
			*/

			// 4 step: Validate domain name
			if (!this.checkDomainName(certificate.SubjectName))
			{
				#warning "Add error to the list"
			}

			if (certificateErrors.Length > 0)
			{
				if (!this.Context.SslStream.RaiseServerCertificateValidation(
					new X509Cert.X509Certificate(certificate.RawData), 
					new int[]{}))
				{
					throw this.Context.CreateException("Invalid certificate received form server.");
				}
			}
		}

		private bool checkDomainName(string subjectName)
		{
			string	domainName = String.Empty;
			Regex search = new Regex(@"([\w\s\d]*)\s*=\s*([^,]*)");

			MatchCollection	elements = search.Matches(subjectName);

			foreach (Match element in elements)
			{
				switch (element.Groups[1].Value.Trim().ToUpper())
				{
					case "CN":
						domainName = element.Groups[2].Value;
						break;
				}
			}

			return (this.Context.ClientSettings.TargetHost == domainName);
		}

		#endregion
	}
}
