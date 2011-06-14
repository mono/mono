//
// BinaryMessageEncoder.cs
//
// Author: Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2005,2009 Novell, Inc (http://www.novell.com)
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
using System.IO;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.Text;
using System.Xml;

namespace System.ServiceModel.Channels
{
	internal class BinaryMessageEncoder : MessageEncoder
	{
		public BinaryMessageEncoder ()
		{
		}

		public BinaryMessageEncoder (BinaryMessageEncoderFactory owner, bool session)
		{
			this.owner = owner;
			this.session = session;
		}

		BinaryMessageEncoderFactory owner;
		bool session;

		internal bool UseSession {
			get { return session; }
		}

		public override string ContentType {
			get { return MediaType; }
		}

		public override string MediaType {
			get { return session ? "application/soap+msbinsession1" : "application/soap+msbin1"; }
		}

		public override MessageVersion MessageVersion {
			get { return MessageVersion.Default; }
		}

		[MonoTODO]
		public override Message ReadMessage (ArraySegment<byte> buffer,
			BufferManager bufferManager, string contentType)
		{
			if (contentType != null && contentType != ContentType)
				throw new ProtocolException ("Only content type 'application/soap+msbin1' is allowed.");

			// FIXME: retrieve reader session and message body.

			throw new NotImplementedException ();

/*
			// FIXME: use bufferManager
			return Message.CreateMessage (
				XmlDictionaryReader.CreateBinaryReader (
					buffer.Array, buffer.Offset, buffer.Count,
					soap_dictionary,
					owner != null ? owner.Owner.ReaderQuotas : new XmlDictionaryReaderQuotas ()),
				int.MaxValue, MessageVersion);
*/
		}

		// It is sort of nasty hack, but there is no other way to provide reader/writer session from TCP stream.
		internal XmlBinaryReaderSession CurrentReaderSession { get; set; }
		internal XmlBinaryWriterSession CurrentWriterSession { get; set; }

		public override Message ReadMessage (Stream stream,
			int maxSizeOfHeaders, string contentType)
		{
			if (contentType != null && contentType != ContentType)
				throw new ProtocolException ("Only content type 'application/soap+msbin1' is allowed.");

			// FIXME: remove this extraneous buffering. It is somehow required for HTTP + binary encoding binding. The culprit is probably in binary xml reader or writer, but not sure.
			if (!stream.CanSeek) {
				var tmpms = new MemoryStream ();
				var bytes = new byte [4096];
				int len;
				do {
					len = stream.Read (bytes, 0, bytes.Length);
					tmpms.Write (bytes, 0, len);
				} while (len > 0);
				tmpms.Seek (0, SeekOrigin.Begin);
				stream = tmpms;
			}

			var ret = Message.CreateMessage (
				XmlDictionaryReader.CreateBinaryReader (stream, Constants.SoapDictionary, owner != null ? owner.Owner.ReaderQuotas : new XmlDictionaryReaderQuotas (), session ? CurrentReaderSession : null),
				maxSizeOfHeaders, MessageVersion);
			ret.Properties.Encoder = this;
			return ret;
		}

		public override void WriteMessage (Message message, Stream stream)
		{
			VerifyMessageVersion (message);

			using (var xw = XmlDictionaryWriter.CreateBinaryWriter (stream, Constants.SoapDictionary, session ? CurrentWriterSession : null))
				message.WriteMessage (xw);
		}

		[MonoTODO]
		public override ArraySegment<byte> WriteMessage (
			Message message, int maxMessageSize,
			BufferManager bufferManager, int messageOffset)
		{
			VerifyMessageVersion (message);

			throw new NotImplementedException ();
		}
	}
}
