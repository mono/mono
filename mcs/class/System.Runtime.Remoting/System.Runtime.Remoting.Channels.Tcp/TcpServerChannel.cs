//
// System.Runtime.Remoting.Channels.Tcp.TcpChannel.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;

namespace System.Runtime.Remoting.Channels.Tcp
{
	public class TcpServerTransportSink : IServerChannelSink, IChannelSinkBase
	{
		IServerChannelSink next_sink;
		
		public TcpServerTransportSink (IServerChannelSink next)
		{
			next_sink = next;
		}
		
		public IServerChannelSink NextChannelSink {
			get {
				return next_sink;
			}
		}

		[MonoTODO]
		public IDictionary Properties {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public void AsyncProcessResponse (IServerResponseChannelSinkStack sinkStack, object state,
						  IMessage msg, ITransportHeaders headers, Stream stream)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Stream GetResponseStream (IServerResponseChannelSinkStack sinkStack, object state,
						IMessage msg, ITransportHeaders headers)
		{
			throw new NotImplementedException ();
		}
		
		public ServerProcessing ProcessMessage (IServerChannelSinkStack sinkStack,
							IMessage requestMsg,
							ITransportHeaders requestHeaders,
							Stream requestStream,
							out IMessage responseMsg,
							out ITransportHeaders responseHeaders,
							out Stream responseStream)
		{
			// this is the first sink, and TcpServerChannel does not call it.
			throw new NotSupportedException ();
		}

		internal void InternalProcessMessage (Stream requestStream)
		{
			Console.WriteLine ("ProcessMessageInternal");
		}
	}

	public class TcpServerChannel : IChannelReceiver, IChannel
	{
		int port = 0;
		string name = "tcp";
		string host;
		int priority = 1;
		Thread server_thread = null;
		TcpListener listener;
		TcpServerTransportSink sink;
		ChannelDataStore channel_data;
		
		void Init (IServerChannelSinkProvider provider) {
			if (provider == null) {
				provider = new BinaryServerFormatterSinkProvider ();
			}
			IServerChannelSink next_sink = provider.CreateSink (this);

			host = Dns.GetHostByName(Dns.GetHostName()).HostName;
			
			string [] uris = null;
			
			if (port != 0) {
				uris = new String [1];
				uris [0] = GetChannelUri ();
			}
			
			channel_data = new ChannelDataStore (uris);;

			sink = new TcpServerTransportSink (next_sink);
			
			listener = new TcpListener (port);
			StartListening (null);
		}
		
		public TcpServerChannel (int port)
		{
			this.port = port;
			Init (null);
		}

		public TcpServerChannel (IDictionary properties,
					 IServerChannelSinkProvider serverSinkProvider)
		{
			port = (int)properties ["port"];
			Init (serverSinkProvider);
		}

		public TcpServerChannel (string name, int port,
					 IServerChannelSinkProvider serverSinkProvider)
		{
			name = name;
			this.port = port;
			Init (serverSinkProvider);
		}
		
		public TcpServerChannel (string name, int port)
		{
			name = name;
			this.port = port;
			Init (null);
		}
		
		public object ChannelData
		{
			get {
				return channel_data;
			}
		}

		public string ChannelName
		{
			get {
				return name;
			}
		}

		public int ChannelPriority
		{
			get {
				return priority;
			}
		}

		public string GetChannelUri ()
		{
			return "tcp://" + host + ":" + port;
		}
		
		public string[] GetUrlsForUri (string uri)
		{
			string [] result = new String [1];

			if (uri.IndexOf ('/') != 0)
				result [0] = GetChannelUri () + "/" + uri;
			else
				result [0] = GetChannelUri () + uri;

			return result;
		}

		public string Parse (string url, out string objectURI)
		{
			int port;
			
			string host = TcpChannel.ParseTcpURL (url, out objectURI, out port);

			return "tcp://" + host + ":" + port;
		}

		void WaitForConnections ()
		{
			while (true) {
				TcpClient client = listener.AcceptTcpClient ();

				sink.InternalProcessMessage (client.GetStream ());
				
				client.Close ();
			}
		}
		
		public void StartListening (object data)
		{
			if (server_thread == null) {
				listener.Start ();
				if (port == 0) {
					port = ((IPEndPoint)listener.LocalEndpoint).Port;
					channel_data.ChannelUris = new String [1];
					channel_data.ChannelUris [0] = GetChannelUri ();
				}

				server_thread = new Thread (new ThreadStart (WaitForConnections));
				server_thread.Start ();
			}
		}

		public void StopListening (object data)
		{
			if (server_thread != null) {
				server_thread.Abort ();
				server_thread = null;
				listener.Stop ();
			}
		}
	}
}
