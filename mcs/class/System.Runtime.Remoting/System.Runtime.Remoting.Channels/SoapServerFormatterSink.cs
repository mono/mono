//
// System.Runtime.Remoting.Channels.SoapServerFormatterSink.cs
//
// Authors: 	Duncan Mak (duncan@ximian.com)
// 		Jean-Marc Andre (jean-marc.andre@polymtl.ca)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Soap;


namespace System.Runtime.Remoting.Channels {

	/// <summary>
	//	The formatter sink that uses SoapFormatter
	/// </summary>
	// <remarks>
	// 	The formatter sink deserializes the message from the channel sink
	// 	and passes the result to the remoting infrastructure
	// </remark>
	// 	
	public class SoapServerFormatterSink : IServerChannelSink, IChannelSinkBase
	{
		IServerChannelSink next_sink;
		IChannelReceiver _receiver;
		private SoapFormatter _serializationFormatter;
		private SoapFormatter _deserializationFormatter;
		
		public SoapServerFormatterSink (SoapServerFormatterSink.Protocol protocol,
						IServerChannelSink nextSink,
						IChannelReceiver receiver)
		{
			this.next_sink = nextSink;
			_receiver = receiver;
			RemotingSurrogateSelector surrogateSelector = new RemotingSurrogateSelector();
			StreamingContext context = new StreamingContext(StreamingContextStates.Other);
			_serializationFormatter = new SoapFormatter(surrogateSelector, context);
			_deserializationFormatter = new SoapFormatter(null, context);
		}

		/// <summary>
		//	Gets the next channel sink in the channel sink chain
		//  </summary>
		/// <value>
		//	The next channel sink in the sink chain
		//  </value>
		public IServerChannelSink NextChannelSink {
			get {
				return next_sink;
			}
		}

		public IDictionary Properties {
			get {
				return null;
			}
		}

		public void AsyncProcessResponse (IServerResponseChannelSinkStack sinkStack, object state,
						  IMessage msg, ITransportHeaders headers, Stream stream)
						  
		{
			ITransportHeaders responseHeaders = new TransportHeaders();

			if(sinkStack != null) stream = sinkStack.GetResponseStream(msg, responseHeaders);
			if(stream == null) stream = new MemoryStream();

			SoapMessageFormatter soapMsgFormatter = (SoapMessageFormatter)state;

			SoapMessage soapMessage = (SoapMessage) soapMsgFormatter.BuildSoapMessageFromMethodResponse((IMethodReturnMessage)msg, out responseHeaders);

			_serializationFormatter.Serialize(stream, soapMessage, null);

			if(stream is MemoryStream) stream.Position = 0;
		}

		public Stream GetResponseStream (IServerResponseChannelSinkStack sinkStack, object state,
						IMessage msg, ITransportHeaders headers)
		{
			// this method shouldn't be called
			throw new NotSupportedException ();
		}
		
		public ServerProcessing ProcessMessage (IServerChannelSinkStack sinkStack,
							IMessage requestMsg, ITransportHeaders requestHeaders, Stream requestStream,
							out IMessage responseMsg, out ITransportHeaders responseHeaders, out Stream responseStream)
		{
			responseMsg = null;
			responseHeaders = null;
			responseStream = null;
			
			string url = (string)requestHeaders[CommonTransportKeys.RequestUri];
			string uri;
			_receiver.Parse(url, out uri);
			if(uri == null)	uri = url;
			Type serverType = RemotingServices.GetServerTypeForUri(uri);
			
			SoapMessage soapMessage = new SoapMessage();
			_deserializationFormatter.TopObject = soapMessage;
			ServerProcessing sp;
			object rtnMessageObject;
			SoapMessageFormatter soapMsgFormatter = new SoapMessageFormatter();
			requestStream.Position = 0;
			_deserializationFormatter.Deserialize(requestStream);
			requestMsg = soapMsgFormatter.BuildMethodCallFromSoapMessage(soapMessage, uri);
				
			sinkStack.Push(this, soapMsgFormatter);

			try{
				sp = next_sink.ProcessMessage(sinkStack, requestMsg, requestHeaders, null, out responseMsg, out responseHeaders, out responseStream);
				
			}
			catch(Exception e) {
				responseMsg = (IMethodReturnMessage)new ReturnMessage(e, (IMethodCallMessage)requestMsg);
				sp = ServerProcessing.Complete;
			}
			
			if(sp == ServerProcessing.Complete) {
				if(responseMsg != null && responseStream == null) {
					
					rtnMessageObject = soapMsgFormatter.BuildSoapMessageFromMethodResponse((IMethodReturnMessage) responseMsg, out responseHeaders);
					
					responseStream = new MemoryStream();
					
					_serializationFormatter.Serialize(responseStream, rtnMessageObject);
				}

				sinkStack.Pop(this);
			}
			
			return sp;
			
		}

		private object HH(Header[] headers) {
			foreach(Header h in headers) {
				Console.WriteLine("Name: {0} Value:{0}", h.Name, h.Value);
			}
			return null;
		}
		
		[Serializable]
		public enum Protocol
		{
			Http = 0,
			Other = 1,
		}
	}

}
