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
using System.Text;
using Mono.Security;

namespace Mono.Security.Protocol.Tls.Handshake.Client
{
	internal class TlsServerCertificateRequest : TlsHandshakeMessage
	{
		#region Fields

		private TlsClientCertificateType[]	certificateTypes;
		private string[]					distinguisedNames;

		#endregion

		#region Constructors

		public TlsServerCertificateRequest(TlsContext context, byte[] buffer) 
			: base(context, TlsHandshakeType.ServerHello, buffer)
		{
		}

		#endregion

		#region Methods

		public override void Update()
		{
			base.Update();

			this.Context.ServerSettings.CertificateTypes	= this.certificateTypes;
			this.Context.ServerSettings.DistinguisedNames	= this.distinguisedNames;
			this.Context.ServerSettings.CertificateRequest	= true;
		}

		#endregion

		#region Protected Methods

		protected override void ProcessAsSsl3()
		{
			throw new NotSupportedException();
		}

		protected override void ProcessAsTls1()
		{
			// Read requested certificate types
			int typesCount = this.ReadByte();
						
			this.certificateTypes = new TlsClientCertificateType[typesCount];

			for (int i = 0; i < typesCount; i++)
			{
				this.certificateTypes[i] = (TlsClientCertificateType)this.ReadByte();
			}

			/*
			 * Read requested certificate authorities (Distinguised Names)
			 * 
			 * Name ::= SEQUENCE OF RelativeDistinguishedName
			 * 
			 * RelativeDistinguishedName ::= SET OF AttributeValueAssertion
			 * 
			 * AttributeValueAssertion ::= SEQUENCE {
			 * attributeType OBJECT IDENTIFIER
			 * attributeValue ANY }
			 */
			if (this.ReadInt16() != 0)
			{
				ASN1	rdn = new ASN1(this.ReadBytes(this.ReadInt16()));

				distinguisedNames = new string[rdn.Count];

				#warning "needs testing"
				for (int i = 0; i < rdn.Count; i++)
				{
					// element[0] = attributeType
					// element[1] = attributeValue
					ASN1 element = new ASN1(rdn[i].Value);

					distinguisedNames[i] = Encoding.UTF8.GetString(element[1].Value);
				}
			}
		}

		#endregion
	}
}
