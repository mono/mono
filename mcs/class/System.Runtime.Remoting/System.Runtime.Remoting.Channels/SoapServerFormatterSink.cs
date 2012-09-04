//
// System.Runtime.Remoting.Channels.SoapServerFormatterSink.cs
//
// Authors: 	Duncan Mak (duncan@ximian.com)
// 		Jean-Marc Andre (jean-marc.andre@polymtl.ca)
//
// 2002 (C) Copyright, Ximian, Inc.
//

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

using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Soap;
using System.Runtime.InteropServices;


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
		private SoapCore _soapCore = SoapCore.DefaultInstance;
		
		public SoapServerFormatterSink (SoapServerFormatterSink.Protocol protocol,
						IServerChannelSink nextSink,
						IChannelReceiver receiver)
		{
			this.next_sink = nextSink;
			_receiver = receiver;
		}

		internal SoapCore SoapCore
		{
			get { return _soapCore; }
			set { _soapCore = value; }
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

		[ComVisible(false)]
		public TypeFilterLevel TypeFilterLevel
		{
			get { return _soapCore.TypeFilterLevel; }
			set 
			{
				IDictionary props = (IDictionary) ((ICloneable)_soapCore.Properties).Clone ();
				props ["typeFilterLevel"] = value;
				_soapCore = new SoapCore (this, props, SoapServerFormatterSinkProvider.AllowedProperties);
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

			_soapCore.Serializer.Serialize(stream, soapMessage, null);

			if(stream is MemoryStream) stream.Position = 0;
			sinkStack.AsyncProcessResponse (msg, responseHeaders, stream);
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
			// Check whether the request was already processed by another
			// formatter sink and pass the request to the next sink if so.
			if (requestMsg != null)
				return next_sink.ProcessMessage (sinkStack,
								 requestMsg,
								 requestHeaders,
								 requestStream,
								 out responseMsg,
								 out responseHeaders,
								 out responseStream);

			// Check whether the request is suitable for this formatter
			// and pass the request to the next sink if not.
			// Note that a null content-type is handled as suitable,
			// otherwise no other sink will be able to handle the request.
			string contentType = requestHeaders["Content-Type"] as string;
			if (contentType == null || !contentType.StartsWith ("text/xml") || requestHeaders["SOAPAction"] == null) {
				return next_sink.ProcessMessage (sinkStack,
					requestMsg,
					requestHeaders,
					requestStream,
					out responseMsg,
					out responseHeaders,
					out responseStream);
			}

			responseMsg = null;
			responseHeaders = null;
			responseStream = null;

			ServerProcessing sp;
			SoapMessageFormatter soapMsgFormatter = new SoapMessageFormatter();
			sinkStack.Push(this, soapMsgFormatter);

			try {
				string url = (string)requestHeaders[CommonTransportKeys.RequestUri];
				string uri;
				_receiver.Parse(url, out uri);
				if(uri == null)	uri = url;
				Type serverType = RemotingServices.GetServerTypeForUri(uri);
				if (serverType == null) throw new RemotingException ("No receiver for uri " + uri);
			
				SoapFormatter fm = _soapCore.GetSafeDeserializer ();
				SoapMessage soapMessage = soapMsgFormatter.CreateSoapMessage (true);
				fm.TopObject = soapMessage;
				fm.Deserialize(requestStream);

				requestMsg = soapMsgFormatter.BuildMethodCallFromSoapMessage(soapMessage, uri);
				
				sp = next_sink.ProcessMessage(sinkStack, requestMsg, requestHeaders, null, out responseMsg, out responseHeaders, out responseStream);
				
				if(sp == ServerProcessing.Complete) {
					if(responseMsg != null && responseStream == null) {

						object rtnMessageObject = soapMsgFormatter.BuildSoapMessageFromMethodResponse((IMethodReturnMessage) responseMsg, out responseHeaders);
						responseStream = new MemoryStream();
						_soapCore.Serializer.Serialize(responseStream, rtnMessageObject);
						responseStream.Position = 0;
					}
				}
			}
			catch(Exception e)
			{
				responseMsg = (IMethodReturnMessage)new ReturnMessage(e, (IMethodCallMessage)requestMsg);
				object rtnMessageObject = soapMsgFormatter.BuildSoapMessageFromMethodResponse((IMethodReturnMessage) responseMsg, out responseHeaders);
				responseStream = new MemoryStream();
				_soapCore.Serializer.Serialize(responseStream, rtnMessageObject);
				responseStream.Position = 0;
				sp = ServerProcessing.Complete;
			}

			if (sp == ServerProcessing.Complete)
				sinkStack.Pop(this);

			return sp;
			
		}

		[Serializable]
		public enum Protocol
		{
			Http = 0,
			Other = 1,
		}
	}

}
