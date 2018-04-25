//
// MessageBufferImpl.cs
//
// Author:
//	Duncan Mak (duncan@novell.com)
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.XPath;

namespace System.ServiceModel.Channels
{
	internal class DefaultMessageBuffer : MessageBuffer
	{
		MessageHeaders headers;
		MessageProperties properties;
		BodyWriter body;
		bool closed, is_fault;
		int max_buffer_size;
		AttributeCollection attributes;

		internal DefaultMessageBuffer (MessageHeaders headers, MessageProperties properties, AttributeCollection attributes)
			: this (0, headers, properties, null, false, attributes)
		{
		}

		internal DefaultMessageBuffer (int maxBufferSize, MessageHeaders headers, MessageProperties properties, BodyWriter body, bool isFault, AttributeCollection attributes)
		{
			this.max_buffer_size = maxBufferSize;
			this.headers = headers;
			this.body = body;
			this.closed = false;
			this.is_fault = isFault;
			this.properties = properties;
			this.attributes = attributes;
		}

		public override void Close ()
		{
			if (closed) 
				return;
			
			headers = null;
			body = null;
			closed = true;
		}
		

		public override Message CreateMessage ()
		{
			if (closed)
				throw new ObjectDisposedException ("The message buffer has already been closed.");
			Message msg;
			if (body == null)
				msg = new EmptyMessage (headers.MessageVersion, headers.Action);
			else
				msg = new SimpleMessage (headers.MessageVersion, headers.Action, body.CreateBufferedCopy (max_buffer_size), is_fault, attributes);
			msg.Headers.Clear ();
			msg.Headers.CopyHeadersFrom (headers);
			msg.Properties.CopyProperties (properties);
			return msg;
		}

		public override int BufferSize {
			get { return 0; }
		}
	}

#if !MOBILE
	internal class XPathMessageBuffer : MessageBuffer
	{
		IXPathNavigable source;
		MessageVersion version;
		int max_header_size;
		MessageProperties properties;
		AttributeCollection attributes;

		public XPathMessageBuffer (IXPathNavigable source, MessageVersion version, int maxSizeOfHeaders, MessageProperties properties, AttributeCollection attributes)
		{
			this.source = source;
			this.version = version;
			this.max_header_size = maxSizeOfHeaders;
			this.properties = properties;
			this.attributes = attributes;
		}

		public override void Close ()
		{
		}

		public override Message CreateMessage ()
		{
			XmlDictionaryReader r = XmlDictionaryReader.CreateDictionaryReader (source.CreateNavigator ().ReadSubtree ());
			Message msg = new XmlReaderMessage (version, r, max_header_size);
			msg.Properties.CopyProperties (properties);
			return msg;
		}

		public override int BufferSize {
			// FIXME: implement
			get { return 0; }
		}
	}
#endif
}
