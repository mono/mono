//
// System.Runtime.Remoting.Channels.SoapClientFormatterSink.cs
//
// Authors: 	Rodrigo Moya (rodrigo@ximian.com)
// 		Jean-Marc André (jean-marc.andre@polymtl.ca)
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

namespace System.Runtime.Remoting.Channels
{
	public class SoapClientFormatterSink : IClientFormatterSink,
		IMessageSink, IClientChannelSink, IChannelSinkBase
	{
		private IClientChannelSink _nextChannelSink;
		private SoapCore _soapCore = SoapCore.DefaultInstance;

		public SoapClientFormatterSink (IClientChannelSink sink)
		{
			_nextChannelSink = sink;
		}
		
		internal SoapCore SoapCore
		{
			get { return _soapCore; }
			set { _soapCore = value; }
		}
		
		// IClientChannelSink
		public IClientChannelSink NextChannelSink
		{
			get {
				return _nextChannelSink;
			}
		}
		
		// IMessageSink
		public IMessageSink NextSink
	        {
			get {
				return null ;
			}
		}
		
		// IChannelSinkBase
		public IDictionary Properties
		{
			get {
				return null;
			}
		}

		public IMessageCtrl AsyncProcessMessage (IMessage msg,
							 IMessageSink replySink)
		{
			Stream requestStream;
			ITransportHeaders requestHeaders;
			SoapMessageFormatter soapMsgFormatter;
			
			SerializeMessage(msg, out requestStream, out requestHeaders, out soapMsgFormatter);

			ClientChannelSinkStack stack = new ClientChannelSinkStack(replySink);
			stack.Push(this, new CallData (msg, soapMsgFormatter));

			_nextChannelSink.AsyncProcessRequest(stack, msg, requestHeaders, requestStream);

			return null;
		}

		public void AsyncProcessRequest (IClientChannelSinkStack sinkStack,
						 IMessage msg,
						 ITransportHeaders headers,
						 Stream stream)
		{
			// this method should never be called
			throw new NotSupportedException ();
		}

		public void AsyncProcessResponse (IClientResponseChannelSinkStack sinkStack,
						  object state,
						  ITransportHeaders headers,
						  Stream stream)
		{
			CallData data = (CallData)state;
			SoapMessageFormatter soapMsgFormatter = data.Formatter;
			IMessage replyMessage = (IMessage) DeserializeMessage(stream, headers, (IMethodCallMessage)data.Msg, soapMsgFormatter);
			sinkStack.DispatchReplyMessage(replyMessage);
			
		}

		public Stream GetRequestStream (IMessage msg,
						ITransportHeaders headers)
		{
			// First sink in the chain so this method should never
			// be called
			throw new NotSupportedException ();
		}

		public void ProcessMessage (IMessage msg,
					    ITransportHeaders requestHeaders,
					    Stream requestStream,
					    out ITransportHeaders responseHeaders,
					    out Stream responseStream)
		{
			// First sink in the chain so this method should never
			// be called
			throw new NotSupportedException ();
			
		}

		public IMessage SyncProcessMessage (IMessage msg)
		{
			Stream requestStream, responseStream;
			ITransportHeaders requestHeaders, responseHeaders;
			SoapMessageFormatter soapMsgFormatter;
			
			SerializeMessage(msg, out requestStream, out requestHeaders, out soapMsgFormatter);
			_nextChannelSink.ProcessMessage(msg, requestHeaders, requestStream, out responseHeaders, out responseStream);

			return DeserializeMessage(responseStream, responseHeaders, (IMethodCallMessage)msg, soapMsgFormatter);
		}
		
		
		private void SerializeMessage(IMessage msg, out Stream requestStream, out ITransportHeaders requestHeaders, out SoapMessageFormatter soapMsgFormatter) {
			SoapMessage soapMsg;
			soapMsgFormatter = new SoapMessageFormatter();
			soapMsg = soapMsgFormatter.BuildSoapMessageFromMethodCall((IMethodCallMessage) msg, out requestHeaders);
			
			// Get the stream where the message will be serialized
			requestStream = _nextChannelSink.GetRequestStream(msg, requestHeaders);
			
			if(requestStream == null) requestStream = new MemoryStream();
			
			// Serialize the message into the stream
			_soapCore.Serializer.Serialize(requestStream, soapMsg, null);
			
			if(requestStream is MemoryStream){
				requestStream.Position = 0;
			}
		}
		
		
		private IMessage DeserializeMessage(Stream responseStream, ITransportHeaders responseHeaders,IMethodCallMessage mcm, SoapMessageFormatter soapMsgFormatter) 
		{
			SoapFormatter fm = _soapCore.GetSafeDeserializer ();
			SoapMessage rtnMessage = new SoapMessage();
			fm.TopObject = rtnMessage;
			object objReturn = fm.Deserialize(responseStream);
			
			return soapMsgFormatter.FormatResponse((ISoapMessage) objReturn, mcm);
		}

		class CallData
		{
			public CallData (IMessage msg, SoapMessageFormatter formatter) { Msg = msg; Formatter = formatter; }
			public IMessage Msg;
			public SoapMessageFormatter Formatter;
		}
	}
}
