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
// using Mono.Security.Protocol.Tls;

namespace Mono.Security.Protocol.Tls.Handshake
{
	internal class TlsHandshakeMessage : TlsStream
	{
		#region FIELDS

		private TlsSession			session;
		private TlsHandshakeType	handshakeType;
		private TlsContentType		contentType;

		#endregion

		#region PROPERTIES

		public TlsSession Session
		{
			get { return session; }
		}

		public TlsHandshakeType HandshakeType
		{
			get { return handshakeType; }
		}

		public TlsContentType ContentType
		{
			get { return contentType; }
		}

		#endregion

		#region CONSTRUCTORS

		public TlsHandshakeMessage(TlsSession session,
			TlsHandshakeType handshakeType,
			TlsContentType contentType) : base()
		{
			this.session		= session;
			this.handshakeType	= handshakeType;
			this.contentType	= contentType;

			// Fill message contents
			this.Fill();
		}

		public TlsHandshakeMessage(TlsSession session, 
			TlsHandshakeType handshakeType, byte[] data) : base(data)
		{
			this.session		= session;
			this.handshakeType	= handshakeType;
						
			// Parse message
			this.Parse();
		}

		#endregion

		#region METHODS

		public virtual void UpdateSession()
		{			
			if (CanWrite)
			{
				this.session.Context.HandshakeHashes.Update(this.EncodeMessage());
				this.Reset();
			}
		}

		protected virtual void Parse()
		{
		}

		protected virtual void Fill()
		{
		}

		public virtual byte[] EncodeMessage()
		{
			byte[] result = null;

			if (CanWrite)
			{
				TlsStream c = new TlsStream();

				c.Write((byte)HandshakeType);
				c.WriteInt24((int)Length);
				c.Write(ToArray());

				result = c.ToArray();
			}

            return result;
		}

		#endregion
	}
}
