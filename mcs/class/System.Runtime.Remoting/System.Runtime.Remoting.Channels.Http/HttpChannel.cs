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

using System;
using System.Collections;
using System.Runtime.Remoting.Messaging;


namespace System.Runtime.Remoting.Channels.Http  
{

	public class HttpChannel: IChannelReceiver, IChannelSender, IChannel, IChannelReceiverHook
	{
		private HttpServerChannel serverChannel;
		private HttpClientChannel clientChannel;
		private string channelName = "http";
		private int channelPriority = 1;

		public HttpChannel()
		{
			SetupChannel(null,null,null);
		}

		public HttpChannel(int port)
		{
			Hashtable prop = new Hashtable();
			prop["port"] = port;
			SetupChannel(prop,null,null);
		}

		public HttpChannel(IDictionary Properties,IClientChannelSinkProvider clientSinkProvider,IServerChannelSinkProvider serverSinkProvider)
		{
			SetupChannel(Properties,clientSinkProvider,serverSinkProvider);
		}

		private void SetupChannel (IDictionary properties, IClientChannelSinkProvider clientSinkProvider, IServerChannelSinkProvider serverSinkProvider)
		{
			clientChannel = new HttpClientChannel (properties, clientSinkProvider);
			serverChannel = new HttpServerChannel (properties, serverSinkProvider);
			
			object val = properties ["name"];
			if (val != null) channelName = val as string;
			
			val = properties ["priority"];
			if (val != null) channelPriority = Convert.ToInt32 (val);
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
	}
}
