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
using System.Runtime.Serialization.Formatters.Binary;

namespace System.Runtime.Remoting.Channels
{
	public class BinaryClientFormatterSink : IClientFormatterSink,
		IMessageSink, IClientChannelSink, IChannelSinkBase
	{
		IClientChannelSink nextInChain;
		IRemotingFormatter formatter = new BinaryFormatter ();
		
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

		public IMessageCtrl AsyncProcessMessage (IMessage msg,
							 IMessageSink replySink)
		{
			throw new NotImplementedException ();
		}

		public void AsyncProcessRequest (IClientChannelSinkStack sinkStack,
						 IMessage msg,
						 ITransportHeaders headers,
						 Stream stream)
	        {
			// never called because the formatter sink is
			// always the first in the chain
			throw new NotSupportedException ();
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
			// never called
			throw new NotSupportedException ();
		}

		public void ProcessMessage (IMessage msg,
					    ITransportHeaders requestHeaders,
					    Stream requestStream,
					    out ITransportHeaders responseHeaders,
					    out Stream responseStream)
		{
			// never called because the formatter sink is
			// always the first in the chain
			throw new NotSupportedException ();
		}

		[MonoTODO]
		public IMessage SyncProcessMessage (IMessage msg)
		{
			try {

				ITransportHeaders response_headers;
				Stream response_stream;
			
				// fixme: use nextInChain.GetRequestStream() ??
				Stream out_stream = new MemoryStream ();
				//Stream out_stream = File.Open ("test.bin", FileMode.Create);
			
				// serialize msg to the stream
				//formatter.Serialize (out_stream, msg, null);
				//formatter.Serialize (out_stream, new Exception ("TEST"), null);
				//out_stream.Position = 0;			
				nextInChain.ProcessMessage (msg, null, out_stream, out response_headers,
							    out response_stream);

				out_stream.Close ();

				// deserialize response_stream
				//IMessage result = (IMessage) formatter.Deserialize (response_stream, null);
				throw new NotImplementedException ();

				return null;
				
			} catch (Exception e) {
				 return new ReturnMessage (e, (IMethodCallMessage)msg);
			}
		}
	}
}
