/* Transport Security Layer (TLS)
 * Copyright (c) 2003-2004 Carlos Guzman Alvarez
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
using System.Net;
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
		#region Fields

		private X509CertificateCollection	certificates;
		
		#endregion

		#region Constructors

		public TlsServerCertificate(TlsContext context, byte[] buffer) 
			: base(context, TlsHandshakeType.Certificate, buffer)
		{
		}

		#endregion

		#region Methods

		public override void Update()
		{
			base.Update();
			this.Context.ServerSettings.Certificates = certificates;
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
				}
			}

#warning Correct validation needs to be made using a certificate chain

			// Restrict validation to the first certificate
			// this.validateCertificate(certificates[0]);
		}

		#endregion

		#region Private Methods

		private void validateCertificate(X509Certificate certificate)
		{
			ArrayList errors = new ArrayList();

			// 1 step : Validate dates
			if (!certificate.IsCurrent)
			{
				// errors.Add(0x800B0101);
				errors.Add(0x01);
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
				// errors.Add(0x800B010F);
				errors.Add(0x02);
			}

			if (errors.Count > 0)
			{
				int[] certificateErrors = (int[])errors.ToArray(typeof(int));

				if (!this.Context.SslStream.RaiseServerCertificateValidation(
					new X509Cert.X509Certificate(certificate.RawData), 
					certificateErrors))
				{
					throw this.Context.CreateException("Invalid certificate received form server.");
				}
			}
		}

		private bool checkDomainName(string subjectName)
		{
			string	domainName = String.Empty;
			// Regex search = new Regex(@"([\w\s\d]*)\s*=\s*([^,]*)");
			Regex search = new Regex(@"CN=\s*([^,]*)");

			MatchCollection	elements = search.Matches(subjectName);

			if (elements[0].Value.StartsWith("CN="))
			{
				domainName = elements[0].Value.Remove(0, 3);
			}

			if (domainName == String.Empty)
			{
				return false;
			}
			else
			{
				string targetHost = this.Context.ClientSettings.TargetHost;

				// Check that the IP is correct
				try
				{
					IPAddress	ipHost		= Dns.Resolve(targetHost).AddressList[0];
					IPAddress	ipDomain	= Dns.Resolve(domainName).AddressList[0];

					return (ipHost.Address == ipDomain.Address);
				}
				catch (Exception)
				{
					return false;
				}
			}
		}

		#endregion
	}
}
