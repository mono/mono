// Transport Security Layer (TLS)
// Copyright (c) 2003-2004 Carlos Guzman Alvarez

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
using Mono.Security;
using Mono.Security.X509;

namespace Mono.Security.Protocol.Tls.Handshake.Server
{
	internal class TlsServerCertificateRequest : HandshakeMessage
	{
		#region Constructors

		public TlsServerCertificateRequest(Context context) 
			: base(context, HandshakeType.CertificateRequest)
		{
		}

		#endregion

		#region Protected Methods

		protected override void ProcessAsSsl3()
		{
			this.ProcessAsTls1();
		}

		protected override void ProcessAsTls1()
		{
			ServerContext context = (ServerContext)this.Context;
			
			int count = context.ServerSettings.CertificateTypes.Length;

			this.WriteByte(Convert.ToByte(count));

			// Write requested certificate types
			for (int i = 0; i < count; i++)
			{
				this.WriteByte((byte)context.ServerSettings.CertificateTypes[i]);
			}

			/*
			 * Write requested certificate authorities (Distinguised Names)
			 * 
			 * Name ::= SEQUENCE OF RelativeDistinguishedName
			 * 
			 * RelativeDistinguishedName ::= SET OF AttributeValueAssertion
			 * 
			 * AttributeValueAssertion ::= SEQUENCE {
			 * attributeType OBJECT IDENTIFIER
			 * attributeValue ANY }
			 */

			/*
			*  From RFC 5246:
			*	If the certificate_authorities list is empty, then the client MAY
			*	send any certificate of the appropriate ClientCertificateType,
			*	unless there is some external arrangement to the contrary.
			*
			*  Better let the client choose which certificate instead of sending down
			*  a potentially large list of DNs.

			if (context.ServerSettings.DistinguisedNames.Length > 0)
			{
				TlsStream list = new TlsStream ();
				// this is the worst formating ever :-|
				foreach (string dn in context.ServerSettings.DistinguisedNames)
				{
					byte[] name = X501.FromString (dn).GetBytes ();
					list.Write ((short)name.Length);
					list.Write (name);
				}
				this.Write ((short)list.Length);
				this.Write (list.ToArray ());
			}
			else
			{
			*/
				this.Write ((short)0);
			//}
		}

		#endregion
	}
}
