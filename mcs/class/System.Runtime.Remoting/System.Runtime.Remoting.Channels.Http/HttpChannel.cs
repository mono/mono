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

	public class HttpChannel: IChannelReceiver, IChannelSender, IChannel
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


		private void SetupChannel(IDictionary Properties,IClientChannelSinkProvider clientSinkProvider,IServerChannelSinkProvider serverSinkProvider)
		{
			if(Properties == null)
			{
				clientChannel = new HttpClientChannel();
				serverChannel = new HttpServerChannel();
			}
			else if(Properties.Count == 1)
			{
				clientChannel = new HttpClientChannel();
				serverChannel = new HttpServerChannel(Convert.ToInt32(Properties["port"].ToString()));
			}
			else
			{
				IDictionary clientProperties = new Hashtable();
				IDictionary serverProperties = new Hashtable();
				foreach(DictionaryEntry DictEntry in Properties)
				{
					switch(DictEntry.Key.ToString())
					{
						// Properties Supported By : HttpChannel,HttpServerChannel,HttpClientChannel
						case "name":
							channelName = DictEntry.Value.ToString(); 
							break;

						case "priority":
							channelPriority = Convert.ToInt32(DictEntry.Value.ToString());
							break;
					
						// Properties Supported By : HttpChannel , HttpClientChannel ONLY
						case "clientConnectionLimit":
							clientProperties["clientConnectionLimit"] = DictEntry.Value;
							break;

						case "proxyName":
							clientProperties["proxyName"] = DictEntry.Value;
							break;

						case "proxyPort":
							clientProperties["proxyPort"] = DictEntry.Value;
							break;
						
						case "useDefaultCredentials":
							clientProperties["useDefaultCredentials"] = DictEntry.Value;
							break;

						// Properties Supported By : HttpChannel , HttpServerChannel ONLY
						case "bindTo": 
							serverProperties["bindTo"] = DictEntry.Value;
							break;
						case "listen": 
							serverProperties["listen"] = DictEntry.Value; 
							break; 
						case "machineName": 
							serverProperties["machineName"] = DictEntry.Value; 
							break; 
						case "port": 
							serverProperties["port"] = DictEntry.Value; 
							break;
						case "suppressChannelData": 
							serverProperties["suppressChannelData"] = DictEntry.Value;
							break;
						case "useIpAddress": 
							serverProperties["useIpAddress"] = DictEntry.Value; 
							break;
					}

				}
				clientChannel = new HttpClientChannel(clientProperties,clientSinkProvider);
				serverChannel = new HttpServerChannel(serverProperties,serverSinkProvider);
			}
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
	}
}
