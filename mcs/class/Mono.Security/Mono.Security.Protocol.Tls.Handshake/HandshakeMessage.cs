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

namespace Mono.Security.Protocol.Tls.Handshake
{
	internal abstract class HandshakeMessage : TlsStream
	{
		#region Fields

		private Context			context;
		private HandshakeType	handshakeType;
		private ContentType	contentType;
		private byte[]		cache;

		#endregion

		#region Properties

		public Context Context
		{
			get { return this.context; }
		}

		public HandshakeType HandshakeType
		{
			get { return this.handshakeType; }
		}

		public ContentType ContentType
		{
			get { return this.contentType; }
		}

		#endregion

		#region Constructors

		public HandshakeMessage(
			Context			context,
			HandshakeType	handshakeType) 
			: this(context, handshakeType, ContentType.Handshake)
		{
		}

		public HandshakeMessage(
			Context			context,
			HandshakeType	handshakeType,
			ContentType		contentType) : base()
		{
			this.context		= context;
			this.handshakeType	= handshakeType;
			this.contentType	= contentType;
		}

		public HandshakeMessage(
			Context			context, 
			HandshakeType	handshakeType, 
			byte[]			data) : base(data)
		{
			this.context		= context;
			this.handshakeType	= handshakeType;						
		}

		#endregion

		#region Abstract Methods

		protected abstract void ProcessAsTls1();

		protected abstract void ProcessAsSsl3();

		#endregion

		#region Methods

		public void Process()
		{
			switch (this.Context.SecurityProtocol)
			{
				case SecurityProtocolType.Tls:
				case SecurityProtocolType.Default:
					this.ProcessAsTls1();
					break;

				case SecurityProtocolType.Ssl3:
					this.ProcessAsSsl3();
					break;

				case SecurityProtocolType.Ssl2:
				default:
					throw new NotSupportedException("Unsupported security protocol type");
			}
		}

		public virtual void Update()
		{
			if (this.CanWrite)
			{
				// result may (should) be available from a previous call to EncodeMessage
				if (cache == null)
					cache = this.EncodeMessage ();
				this.context.HandshakeMessages.Write (cache);
				this.Reset();
				cache = null;
			}
		}

		public virtual byte[] EncodeMessage()
		{
			cache = null;

			if (CanWrite)
			{
				byte[] hs = this.ToArray ();
				int len = hs.Length;
				cache = new byte[4 + len];

				cache[0] = (byte) HandshakeType;
				// Length as an Int24 in Network Order
				cache[1] = (byte) (len >> 16);
				cache[2] = (byte) (len >> 8);
				cache[3] = (byte) len;
				Buffer.BlockCopy (hs, 0, cache, 4, len);
			}

			return cache;
		}

		static public bool Compare (byte[] buffer1, byte[] buffer2)
		{
			// in our case both null can't exist (or be valid)
			if ((buffer1 == null) || (buffer2 == null))
				return false;

			if (buffer1.Length != buffer2.Length)
				return false;

			for (int i = 0; i < buffer1.Length; i++) {
				if (buffer1[i] != buffer2[i])
					return false;
			}
			return true;
		}

		#endregion
	}
}
