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
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Mono.Security.Protocol.Tls.Alerts;

namespace Mono.Security.Protocol.Tls.Handshake.Client
{
	internal class TlsServerCertificate : TlsHandshakeMessage
	{
		#region FIELDS

		private X509CertificateCollection certificates;
		
		#endregion

		#region PROPERTIES

		public X509CertificateCollection Certificates
		{
			get { return certificates; }
		}

		#endregion

		#region CONSTRUCTORS

		public TlsServerCertificate(TlsSession session, byte[] buffer) 
			: base(session, TlsHandshakeType.Certificate, buffer)
		{
		}

		#endregion

		#region METHODS

		public override void UpdateSession()
		{
			base.UpdateSession();
			this.Session.Context.ServerSettings.ServerCertificates = certificates;
		}

		#endregion

		#region PROTECTED_METHODS

		protected override void Parse()
		{
			this.certificates = new X509CertificateCollection();
			
			int readed	= 0;
			int length	= ReadInt24();

			while (readed < length)
			{
				// Read certificate length
				int certLength = ReadInt24();

				// Increment readed
				readed += 3;

				if (certLength > 0)
				{
					// Read certificate
					X509Certificate certificate = new X509Certificate(ReadBytes(certLength));
					certificates.Add(certificate);

					readed += certLength;

					validateCertificate(certificate);
				}
			}
		}

		#endregion

		#region  PRIVATE_METHODS

		private void validateCertificate(X509Certificate certificate)
		{
			#warning "Check validity of certificates"

			// 1 step : Validate dates
			DateTime effectiveDate	= DateTime.Parse(certificate.GetEffectiveDateString());
			DateTime expirationDate = DateTime.Parse(certificate.GetExpirationDateString());
			if (System.DateTime.Now < effectiveDate || 
				System.DateTime.Now > expirationDate)
			{
				throw Session.CreateException("Certificate received FromBase64Transform the server expired.");
			}

			// 2 step: Validate CA

			// 3 step: Validate digital sign

			// 4 step: Validate domain name
		}

		#endregion
	}
}
