//
// MessageEncoder.cs
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

namespace System.ServiceModel.Channels
{
	public abstract class MessageEncoder
	{
		protected MessageEncoder ()
		{
		}

		public abstract string ContentType { get; }

		public abstract string MediaType { get; }

		public abstract MessageVersion MessageVersion { get; }

		[MonoTODO]
		public virtual T GetProperty<T> () where T : class
		{
			throw new NotImplementedException ();
		}

		public virtual bool IsContentTypeSupported (string contentType)
		{
			if (contentType == null)
				throw new ArgumentNullException ("contentType");
			int idx = contentType.IndexOf (';');
			if (idx > 0)
				return contentType.StartsWith (ContentType, StringComparison.Ordinal);
			return contentType == MediaType;
		}

		public Message ReadMessage (ArraySegment<byte> buffer,
			BufferManager bufferManager)
		{
			return ReadMessage (buffer, bufferManager, null);
		}

		public abstract Message ReadMessage (ArraySegment<byte> buffer,
			BufferManager bufferManager, string contentType);

		public Message ReadMessage (Stream stream, int maxSizeOfHeaders)
		{
			return ReadMessage (stream, maxSizeOfHeaders, null);
		}

		public abstract Message ReadMessage (Stream stream,
			int maxSizeOfHeaders, string contentType);

		public abstract void WriteMessage (Message message, Stream stream);

		public ArraySegment<byte> WriteMessage (Message message,
			int maxMessageSize, BufferManager bufferManager)
		{
			return WriteMessage (message, maxMessageSize, bufferManager, 0);
		}

		public abstract ArraySegment<byte> WriteMessage (
			Message message, int maxMessageSize,
			BufferManager bufferManager, int messageOffset);

		public override string ToString ()
		{
			return ContentType;
		}

		internal void VerifyMessageVersion (Message message)
		{
			if (!message.Version.Equals (MessageVersion))
				throw new ProtocolException (String.Format ("Message version mismatch. Expected {0} but was {1}.", MessageVersion, message.Version));
		}
	}
}
