//
// System.Runtime.Remoting.Channels.Tcp.TcpChannel.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//         Lluis Sanchez Gual (lluis@ideary.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;

namespace System.Runtime.Remoting.Channels.Tcp
{
	public class TcpChannel : IChannelReceiver, IChannel, IChannelSender
	{
		private TcpClientChannel _clientChannel;
		private TcpServerChannel _serverChannel = null;
		private string _name;
	
		public TcpChannel (): this (0)
        {
		}

		public TcpChannel (int port)
		{
			Hashtable ht = new Hashtable();
			ht["port"] = port.ToString();
			Init(ht, null, null);
		}

		public void Init(IDictionary properties, IClientChannelSinkProvider clientSink, IServerChannelSinkProvider serverSink)
		{
			_clientChannel = new TcpClientChannel(properties,clientSink);

			string port = properties["port"] as string;
			if (port != null && port != string.Empty)
			{
				_serverChannel = new TcpServerChannel(properties, serverSink);
			}

			_name = properties["name"] as string;
		}


		public TcpChannel (IDictionary properties,
				   IClientChannelSinkProvider clientSinkProvider,
				   IServerChannelSinkProvider serverSinkProvider)
		{
			Init (properties, clientSinkProvider, serverSinkProvider);
		}

		public IMessageSink CreateMessageSink(string url, object remoteChannelData, out string objectURI)
		{
			return _clientChannel.CreateMessageSink(url, remoteChannelData, out objectURI);
		}

		public string ChannelName
		{
			get { return _name; }
		}

		public int ChannelPriority
		{
			get { return 1; }
		}

		public void StartListening (object data)
		{
			if (_serverChannel != null) _serverChannel.StartListening(data);
		}
		
		public void StopListening (object data)
		{
			if (_serverChannel != null) _serverChannel.StopListening(data);
		}

		public string[] GetUrlsForUri (string uri)
		{
			if (_serverChannel != null) return _serverChannel.GetUrlsForUri(uri);
			else return null;
		}

		public object ChannelData
		{
			get 
			{
				if (_serverChannel != null) return _serverChannel.ChannelData;
				else return null;
			}
		}

		public string Parse (string url, out string objectURI)
		{
			return TcpChannel.ParseChannelUrl (url, out objectURI);
		}

		internal static string ParseChannelUrl (string url, out string objectURI)
		{
			int port;
			
			string host = ParseTcpURL (url, out objectURI, out port);
			if (host != null)
				return "tcp://" + host + ":" + port;
			else
				return null;
		}

		internal static string ParseTcpURL (string url, out string objectURI, out int port)
		{
			// format: "tcp://host:port/path/to/object"
			
			objectURI = null;
			port = 0;
			
			Match m = Regex.Match (url, "tcp://([^:]+):([0-9]+)/?(.*)");

			if (!m.Success)
				return null;
			
			string host = m.Groups[1].Value;
			string port_str = m.Groups[2].Value;
			objectURI = m.Groups[3].Value;
			port = Convert.ToInt32 (port_str);

			if (objectURI == string.Empty) objectURI = null;
				
			return host;
		}
	}
}
