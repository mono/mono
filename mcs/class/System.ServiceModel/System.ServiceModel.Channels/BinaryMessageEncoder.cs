//
// BinaryMessageEncoder.cs
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
	internal class BinaryMessageEncoder : MessageEncoder
	{
		public BinaryMessageEncoder ()
		{
		}

		public BinaryMessageEncoder (BinaryMessageEncoderFactory owner)
		{
			this.owner = owner;
		}

		BinaryMessageEncoderFactory owner;

		public override string ContentType {
			get { return "application/soap+msbin1"; }
		}

		public override string MediaType {
			get { return "application/soap+msbin1"; }
		}

		public override MessageVersion MessageVersion {
			get { return MessageVersion.Default; }
		}

		[MonoTODO]
		public override Message ReadMessage (ArraySegment<byte> buffer,
			BufferManager bufferManager, string contentType)
		{
			// FIXME: use bufferManager
			return Message.CreateMessage (
				XmlDictionaryReader.CreateBinaryReader (
					buffer.Array, buffer.Offset, buffer.Count,
					new XmlDictionary (),
					owner != null ? owner.Owner.ReaderQuotas : new XmlDictionaryReaderQuotas ()),
				int.MaxValue, MessageVersion);
		}

		public override Message ReadMessage (Stream stream,
			int maxSizeOfHeaders, string contentType)
		{
			return Message.CreateMessage (
				XmlDictionaryReader.CreateBinaryReader (stream, owner != null ? owner.Owner.ReaderQuotas : new XmlDictionaryReaderQuotas ()),
				maxSizeOfHeaders, MessageVersion);
		}

		public override void WriteMessage (Message message, Stream stream)
		{
			VerifyMessageVersion (message);

			message.WriteMessage (XmlDictionaryWriter.CreateBinaryWriter (stream));
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
