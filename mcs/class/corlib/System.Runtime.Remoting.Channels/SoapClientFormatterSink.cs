//
// System.Runtime.Remoting.Channels.SoapClientFormatterSink.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Channels
{
	public class SoapClientFormatterSink : IClientFormatterSink,
		IMessageSink, IClientChannelSink, IChannelSinkBase
	{
		private IClientChannelSink nextClientSink;
		
		public SoapClientFormatterSink (IClientChannelSink sink)
		{
			nextClientSink = sink;
		}

		public IClientChannelSink NextChannelSink
		{
			get {
				return nextClientSink;
			}
		}

		public IMessageSink NextSink
	        {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public IDictionary Properties
		{
			[MonoTODO]
			get {
				throw new NotImplementedException ();
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

		[MonoTODO]
		public Stream GetRequestStream (IMessage msg,
						ITransportHeaders headers)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ProcessMessage (IMessage msg,
					    ITransportHeaders requestHeaders,
					    Stream requestStream,
					    out ITransportHeaders responseHeaders,
					    out Stream responseStream)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IMessage SyncProcessMessage (IMessage msg)
		{
			throw new NotImplementedException ();
		}
	}
}
