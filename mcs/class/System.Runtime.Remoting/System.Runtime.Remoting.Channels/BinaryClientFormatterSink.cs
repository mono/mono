//
// System.Runtime.Remoting.Channels.BinaryClientFormatterSink.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//         Dietmar Maurer (dietmar@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Channels
{
	public class BinaryClientFormatterSink : IClientFormatterSink,
		IMessageSink, IClientChannelSink, IChannelSinkBase
	{
		private IClientChannelSink nextInChain;
		
		public BinaryClientFormatterSink (IClientChannelSink nextSink)
		{
			nextInChain = nextSink;
		}

		public IClientChannelSink NextChannelSink
		{
			get {
				return nextInChain;
			}
		}

		public IMessageSink NextSink
		{
			get {
				return (IMessageSink) nextInChain;
			}
		}

		public IDictionary Properties
		{
			get {
				return null;
			}
		}

		[MonoTODO]
		public IMessageCtrl AsyncProcessMessage (IMessage msg,
							 IMessageSink replySink)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void AsyncProcessRequest (IClientChannelSinkStack sinkStack,
						 IMessage msg,
						 ITransportHeaders headers,
						 Stream stream)
	        {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void AsyncProcessResponse (IClientResponseChannelSinkStack sinkStack,
						  object state,
						  ITransportHeaders headers,
						  Stream stream)
		{
			throw new NotImplementedException ();
		}

		public Stream GetRequestStream (IMessage msg,
						ITransportHeaders headers)
		{
			return null;
		}

		public void ProcessMessage (IMessage msg,
					    ITransportHeaders requestHeaders,
					    Stream requestStream,
					    out ITransportHeaders responseHeaders,
					    out Stream responseStream)
		{
			nextInChain.ProcessMessage (msg, requestHeaders, requestStream,
						    out responseHeaders, out responseStream);
		}

		[MonoTODO]
		public IMessage SyncProcessMessage (IMessage msg)
		{
			ITransportHeaders response_headers;
			Stream response_stream;
			
			// fixme: use nextInChain.GetRequestStream() ??
			Stream out_stream = new MemoryStream ();

			// fixme: serialize msg to the stream

			ProcessMessage (msg, null, out_stream, out response_headers, out response_stream);

			// fixme: deserialize response_stream
			IMessage result = null;

			return null;
		}
	}
}
