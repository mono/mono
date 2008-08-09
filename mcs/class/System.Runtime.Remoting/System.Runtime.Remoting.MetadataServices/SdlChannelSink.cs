//
// System.Runtime.Remoting.MetadataServices.SdlChannelSink
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
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

using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.IO;
using System.Collections;
using System.Text;

namespace System.Runtime.Remoting.MetadataServices
{
	public class SdlChannelSink : IServerChannelSink, IChannelSinkBase
	{
		IServerChannelSink _next;
		IChannelReceiver _channel;

		public SdlChannelSink (IChannelReceiver receiver, IServerChannelSink nextSink)
		{
			_next = nextSink;
			_channel = receiver;
		}

		public IServerChannelSink NextChannelSink 
		{
			get { return _next; }
		}

		public IDictionary Properties 
		{
			get { return null; }
		}
		
		public void AsyncProcessResponse (IServerResponseChannelSinkStack sinkStack,
						  object state,
						  IMessage msg,
						  ITransportHeaders headers,
						  Stream stream)
		{
			throw new NotSupportedException ();	// Never called
		}

		public Stream GetResponseStream (IServerResponseChannelSinkStack sinkStack,
						 object state,
						 IMessage msg,
						 ITransportHeaders headers)
		{
			return null;
		}

		public ServerProcessing ProcessMessage (IServerChannelSinkStack sinkStack,
							IMessage requestMsg,
							ITransportHeaders requestHeaders,
							Stream requestStream,
							out IMessage responseMsg,
							out ITransportHeaders responseHeaders,
							out Stream responseStream)
		{
			responseMsg = null;
			responseStream = null;
			responseHeaders = null;
			string verb = requestHeaders [CommonTransportKeys.RequestVerb] as string;
			string uri = (string) requestHeaders [CommonTransportKeys.RequestUri];
			
			if (verb == "GET" && uri.EndsWith ("?wsdl"))
			{
				try
				{
					uri = uri.Substring (0, uri.Length - 5);
					Type type = RemotingServices.GetServerTypeForUri (uri);
					
					string url = _channel.GetUrlsForUri (uri)[0];
					ServiceType st = new ServiceType (type, url);
					
					responseStream = new MemoryStream ();
					MetaData.ConvertTypesToSchemaToStream (new ServiceType[] {st}, SdlType.Wsdl, responseStream);
					responseStream.Position = 0;
					responseMsg = null;
					responseHeaders = new TransportHeaders ();
					responseHeaders [CommonTransportKeys.ContentType] = "text/xml";
				}
				catch (Exception ex)
				{
					responseHeaders = new TransportHeaders ();
					responseHeaders [CommonTransportKeys.HttpStatusCode] = "400";
					responseStream = new MemoryStream (Encoding.UTF8.GetBytes (ex.ToString ()));
				}
				return ServerProcessing.Complete;
			}
			else
				return _next.ProcessMessage (sinkStack, requestMsg, requestHeaders, requestStream, out responseMsg, out responseHeaders, out responseStream);
		}
	}
}
