//
// System.Runtime.Remoting.Channels.Tcp.TcpChannel.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//         Lluis Sanchez Gual (lluis@ideary.com)
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
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;

namespace System.Runtime.Remoting.Channels.Tcp
{
	public class TcpChannel : IChannelReceiver, IChannel, IChannelSender
	{
		private TcpClientChannel _clientChannel;
		private TcpServerChannel _serverChannel = null;
		private string _name = "tcp";
		private int _priority = 1;
	
		public TcpChannel (): this (0)
        {
		}

		public TcpChannel (int port)
		{
			Hashtable ht = new Hashtable();
			ht["port"] = port.ToString();
			Init(ht, null, null);
		}

		void Init (IDictionary properties, IClientChannelSinkProvider clientSink, IServerChannelSinkProvider serverSink)
		{
			_clientChannel = new TcpClientChannel (properties,clientSink);

			if(properties["port"] != null)
				_serverChannel = new TcpServerChannel(properties, serverSink);

			object val = properties ["name"];
			if (val != null) _name = val as string;
			
			val = properties ["priority"];
			if (val != null) _priority = Convert.ToInt32 (val);
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
			get { return _priority; }
		}

		public void StartListening (object data)
		{
			if (_serverChannel != null) _serverChannel.StartListening (data);
		}
		
		public void StopListening (object data)
		{
			if (_serverChannel != null) _serverChannel.StopListening(data);
			TcpConnectionPool.Shutdown ();
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
			if (url == null) throw new ArgumentNullException ("url");
			
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
			
			if (!url.StartsWith ("tcp://")) return null;
			int colon = url.IndexOf (':', 6);
			if (colon == -1) return null;
			string host = url.Substring (6, colon - 6);

			int slash = url.IndexOf ('/', colon + 1);
			if (slash == -1) slash = url.Length;
			string port_str = url.Substring (colon + 1, slash - colon - 1);
			
			if (slash < url.Length)
				objectURI = url.Substring (slash + 1);

			try {
				port = Convert.ToInt32 (port_str);
			} catch {
				return null;
			}

			if (objectURI == string.Empty)
				objectURI = null;
				
			return host;
		}
	}
}
