//
// System.Runtime.Remoting.Channels.CORBA.CORBAServerFormatterSink.cs
//
// Author: Dietmar Maurer (dietmar@ximian.com)
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
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Channels.CORBA {

	public class CORBAServerFormatterSink : IServerChannelSink, IChannelSinkBase
	{
		IServerChannelSink next_sink;
		CDRFormatter format = new CDRFormatter ();
	
		public CORBAServerFormatterSink (IServerChannelSink nextSink)
		{
			next_sink = nextSink;
		}

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

		[MonoTODO]
		public void AsyncProcessResponse (IServerResponseChannelSinkStack sinkStack, object state,
						  IMessage msg, ITransportHeaders headers, Stream stream)
		{
			throw new NotImplementedException ();
		}

		public Stream GetResponseStream (IServerResponseChannelSinkStack sinkStack, object state,
						 IMessage msg, ITransportHeaders headers)
		{
			// never called 
			throw new NotSupportedException ();
		}
		
		public ServerProcessing ProcessMessage (IServerChannelSinkStack sinkStack,
							IMessage requestMsg,
							ITransportHeaders requestHeaders,
							Stream requestStream,
							out IMessage responseMsg,
							out ITransportHeaders responseHeaders,
							out Stream responseStream)
		{
			IMessage call;
			
			string uri = (string)requestHeaders ["_requestUri"];

			if (requestMsg == null) {
				call = (IMessage)format.DeserializeRequest (requestStream, uri);
			} else { 
				call = requestMsg;
			}

			next_sink.ProcessMessage (sinkStack, call, requestHeaders, null,
						  out responseMsg, out responseHeaders, out responseStream);
						
			responseStream = new MemoryStream ();

			format.SerializeResponse (responseStream, responseMsg);
			
			return ServerProcessing.Complete;
		}
	}
}
