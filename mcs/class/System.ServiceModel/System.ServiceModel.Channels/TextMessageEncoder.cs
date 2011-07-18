//
// TextMessageEncoder.cs
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
	internal class TextMessageEncoder : MessageEncoder
	{
		Encoding encoding;
		MessageVersion version;

		public TextMessageEncoder (MessageVersion version, Encoding encoding)
		{
			this.version = version;
			this.encoding = encoding;
		}

		internal Encoding Encoding {
			get { return encoding; }
		}

		public override string ContentType {
			get { return String.Concat (MediaType, "; charset=", encoding.WebName); }
		}

		public override string MediaType {
			get { return version.Envelope == EnvelopeVersion.Soap12 ? "application/soap+xml" : "text/xml";  }
		}

		public override MessageVersion MessageVersion {
			get { return version; }
		}

		private static System.Text.RegularExpressions.Regex getAction = new Text.RegularExpressions.Regex("action=\"(.*)\"");

		private static string GetActionFromContentType(string contentType)
		{
			if (contentType == null)
				return string.Empty;
			System.Text.RegularExpressions.Match match = getAction.Match(contentType);

			if (match.Success)
				return match.Groups[1].Value;
			else
				return string.Empty;

		}

		[MonoTODO]
		public override Message ReadMessage (ArraySegment<byte> buffer,
			BufferManager bufferManager, string contentType)
		{
			if (bufferManager == null)
				throw new ArgumentNullException ("bufferManager");
			var ret = Message.CreateMessage (
				XmlDictionaryReader.CreateDictionaryReader (
					XmlReader.Create (new StreamReader (
						new MemoryStream (
						buffer.Array, buffer.Offset,
						buffer.Count), encoding))),
				// FIXME: supply max header size
				int.MaxValue,
				version);

			string action = GetActionFromContentType(contentType);
			if (!string.IsNullOrEmpty(action))
				ret.Headers.Action = action;

			return ret;
		}

		public override Message ReadMessage (Stream stream,
			int maxSizeOfHeaders, string contentType)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");
			var ret = Message.CreateMessage (
				XmlDictionaryReader.CreateDictionaryReader (
					XmlReader.Create (new StreamReader (stream, encoding))),
				maxSizeOfHeaders,
				version);
			ret.Properties.Encoder = this;

			string action = GetActionFromContentType(contentType);
			if (!string.IsNullOrEmpty(action))
				ret.Headers.Action = action;

			return ret;
		}

		public override void WriteMessage (Message message, Stream stream)
		{
			if (message == null)
				throw new ArgumentNullException ("message");
			if (stream == null)
				throw new ArgumentNullException ("stream");
			VerifyMessageVersion (message);

			XmlWriterSettings s = new XmlWriterSettings ();
			s.Encoding = encoding;
			using (XmlWriter w = XmlWriter.Create (stream, s)) {
				message.WriteMessage (
					XmlDictionaryWriter.CreateDictionaryWriter (w));
			}
		}

		[MonoTODO]
		public override ArraySegment<byte> WriteMessage (
			Message message, int maxMessageSize,
			BufferManager bufferManager, int messageOffset)
		{
			VerifyMessageVersion (message);

			ArraySegment<byte> seg = new ArraySegment<byte> (
				bufferManager.TakeBuffer (maxMessageSize),
				messageOffset, maxMessageSize);
			XmlWriterSettings s = new XmlWriterSettings ();
			s.Encoding = encoding;
			using (XmlWriter w = XmlWriter.Create (
				new MemoryStream (seg.Array, seg.Offset, seg.Count), s)) {
				message.WriteMessage (
					XmlDictionaryWriter.CreateDictionaryWriter (w));
			}
			return seg;
		}
	}
}
