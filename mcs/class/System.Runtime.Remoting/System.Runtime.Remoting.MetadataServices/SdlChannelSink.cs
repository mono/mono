//
// System.Runtime.Remoting.MetadataServices.SdlChannelSink
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
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
		
		public SdlChannelSink()
		{
		}

		internal SdlChannelSink (IChannelReceiver channel, IServerChannelSink next)
		{
			_next = next;
			_channel = channel;
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
					responseHeaders [CommonTransportKeys.HttpStatusCode] = "500";
					responseStream = new MemoryStream (Encoding.UTF8.GetBytes (ex.ToString ()));
				}
				return ServerProcessing.Complete;
			}
			else
				return _next.ProcessMessage (sinkStack, requestMsg, requestHeaders, requestStream, out responseMsg, out responseHeaders, out responseStream);
		}
	}
}
