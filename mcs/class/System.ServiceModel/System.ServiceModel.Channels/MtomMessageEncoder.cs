//
// MtomMessageEncoder.cs
//
// Author: Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
	internal class MtomMessageEncoder : MessageEncoder
	{
		Encoding encoding;
		MessageVersion version;
		XmlDictionaryReaderQuotas quotas;

		public MtomMessageEncoder (MtomMessageEncoderFactory owner)
		{
			version = owner.MessageVersion;
			encoding = owner.Owner.WriteEncoding;
			quotas = owner.Owner.ReaderQuotas;
		}

		public override string ContentType {
			get { return "multipart/related; type=application/xop+xml"; }
		}

		public override string MediaType {
			get { return "multipart/related"; }
		}

		public override MessageVersion MessageVersion {
			get { return version; }
		}

		[MonoTODO]
		public override Message ReadMessage (ArraySegment<byte> buffer,
			BufferManager bufferManager, string contentType)
		{
			// FIXME: where should bufferManager be used?
			// FIXME: no way to take maxSizeOfHeaders
			// FIXME: create proper quotas
			var ret = Message.CreateMessage (
				XmlDictionaryReader.CreateMtomReader (buffer.Array, buffer.Offset, buffer.Count, encoding, quotas),
				int.MaxValue,
				MessageVersion);
			ret.Properties.Encoder = this;
			return ret;
		}

		[MonoTODO]
		public override Message ReadMessage (Stream stream,
			int maxSizeOfHeaders, string contentType)
		{
			// FIXME: create proper quotas
			return Message.CreateMessage (
				XmlDictionaryReader.CreateMtomReader (
					stream, encoding, quotas),
				maxSizeOfHeaders,
				MessageVersion);
		}

		[MonoTODO]
		public override void WriteMessage (Message message, Stream stream)
		{
			VerifyMessageVersion (message);

			// FIXME: no way to acquire maxSizeInBytes and startInfo?
			message.WriteMessage (XmlDictionaryWriter.CreateMtomWriter (stream, encoding, int.MaxValue, string.Empty));
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
