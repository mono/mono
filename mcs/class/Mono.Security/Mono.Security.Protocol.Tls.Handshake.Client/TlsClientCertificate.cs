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
using Mono.Security.Protocol.Tls;
using System.Security.Cryptography.X509Certificates;

namespace Mono.Security.Protocol.Tls.Handshake.Client
{
	internal class TlsClientCertificate : TlsHandshakeMessage
	{
		#region CONSTRUCTORS

		public TlsClientCertificate(TlsSession session) 
			: base(session, TlsHandshakeType.Finished, TlsContentType.Handshake)
		{
		}

		#endregion

		#region METHODS

		public override void UpdateSession()
		{
			base.UpdateSession();
			this.Reset();
		}

		#endregion

		#region PROTECTED_METHODS

		protected override void ProcessAsSsl3()
		{
			throw new NotSupportedException();
		}

		protected override void ProcessAsTls1()
		{
			#warning "Check which type of certificates has been requested by the server"

			// Write client certificates information to a stream
			TlsStream stream = new TlsStream();
			foreach (X509Certificate cert in Session.Settings.Certificates)
			{
				stream.WriteInt24(cert.GetRawCertData().Length);
				stream.Write(cert.GetRawCertData());
			}

			// Compose the message
			this.WriteInt24((int)stream.Length);
			this.Write(stream.ToArray());
		}

		#endregion
	}
}
