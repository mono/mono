//
// System.Runtime.Remoting.Channels.Http.HttpChannel
//
// Summary:     Implements a wrapper class for HTTP client and server channels.
//
// Classes:    public HttpChannel
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//		Ahmad Tantawy (popsito82@hotmail.com)
//		Ahmad Kadry (kadrianoz@hotmail.com)
//		Hussein Mehanna (hussein_mehanna@hotmail.com)
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

using System;
using System.Collections;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Channels.Http  
{
	public class HttpChannel: BaseChannelWithProperties, IChannelReceiver, 
		IChannelSender, IChannel, IChannelReceiverHook
	{
		private HttpServerChannel serverChannel;
		private HttpClientChannel clientChannel;
		private string channelName = "http";
		private int channelPriority = 1;
		private AggregateDictionary properties;

		public HttpChannel()
		{
			SetupChannel (new Hashtable(), null, null);
		}

		public HttpChannel (int port)
		{
			Hashtable prop = new Hashtable();
			prop["port"] = port;
			SetupChannel(prop,null,null);
		}

		public HttpChannel (IDictionary properties,IClientChannelSinkProvider clientSinkProvider,IServerChannelSinkProvider serverSinkProvider)
		{
			SetupChannel (properties,clientSinkProvider,serverSinkProvider);
		}

		private void SetupChannel (IDictionary properties, IClientChannelSinkProvider clientSinkProvider, IServerChannelSinkProvider serverSinkProvider)
		{
			clientChannel = new HttpClientChannel (properties, clientSinkProvider);
			serverChannel = new HttpServerChannel (properties, serverSinkProvider);
			
			object val = properties ["name"];
			if (val != null) channelName = val as string;
			
			val = properties ["priority"];
			if (val != null) channelPriority = Convert.ToInt32 (val);
			
			this.properties = new AggregateDictionary (new IDictionary[] {clientChannel, serverChannel});
		}


		//IChannel Members
		public String ChannelName
		{
			get { return channelName; }
		}

		public int ChannelPriority
		{
			get { return channelPriority; }
		}

		public String Parse(String url, out String objectURI)
		{
			return HttpHelper.Parse(url, out objectURI);
		}

		//IChannelSender Members
		public IMessageSink CreateMessageSink(String url, Object remoteChannelData, out String objectURI)
		{
			return clientChannel.CreateMessageSink(url, remoteChannelData, out objectURI);
		}

		//IChannelReciever Members
		public String[] GetUrlsForUri(String objectURI)
		{
			return serverChannel.GetUrlsForUri(objectURI);
		} 

		public void StartListening(Object data)
		{
			serverChannel.StartListening(data);
		}

		public void StopListening(Object data)
		{
			serverChannel.StopListening(data);
		} 
		
		public Object ChannelData
		{
			get { return serverChannel.ChannelData; }
		}
		
		public String ChannelScheme 
		{
			get { return "http"; } 
		}

		public bool WantsToListen 
		{ 
			get { return serverChannel.WantsToListen; } 
			set { serverChannel.WantsToListen = value; }
		} 
		
		public IServerChannelSink ChannelSinkChain 
		{
			get { return serverChannel.ChannelSinkChain; }
		}

		public void AddHookChannelUri (String channelUri)
		{
			serverChannel.AddHookChannelUri (channelUri);
		} 
		
		public override object this [object key]
		{
			get { return properties[key]; }
			set { properties[key] = value; }
		}
		
		public override ICollection Keys 
		{
			get { return properties.Keys; }
		}
		
		public override IDictionary Properties 
		{
			get { return properties; }
		}
	}
}
