//
// System.Runtime.Remoting.Channels.Tcp.TcpServerChannel.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//         Lluis Sanchez Gual (lluis@ideary.com)
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
		int _maxConcurrentConnections = 100;
		ArrayList _activeConnections = new ArrayList();
		
		void Init (IServerChannelSinkProvider serverSinkProvider) 
		{
			if (serverSinkProvider == null) {
				serverSinkProvider = new BinaryServerFormatterSinkProvider ();
			}

			host = Dns.GetHostByName(Dns.GetHostName()).HostName;
			
			string [] uris = null;
			
			if (port != 0) {
				uris = new String [1];
				uris [0] = GetChannelUri ();
			}

			// Gets channel data from the chain of channel providers

			channel_data = new ChannelDataStore (uris);
			IServerChannelSinkProvider provider = serverSinkProvider;
			while (provider != null)
			{
				provider.GetChannelData(channel_data);
				provider = provider.Next;
			}

			// Creates the sink chain that will process all incoming messages

			IServerChannelSink next_sink = ChannelServices.CreateServerChannelSinkChain (serverSinkProvider, this);
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
			port = Int32.Parse ((string)properties ["port"]);
			Init (serverSinkProvider);
		}

		public TcpServerChannel (string name, int port,
					 IServerChannelSinkProvider serverSinkProvider)
		{
			this.name = name;
			this.port = port;
			Init (serverSinkProvider);
		}
		
		public TcpServerChannel (string name, int port)
		{
			this.name = name;
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
			if (!uri.StartsWith ("/")) uri = "/" + uri;

			string [] chnl_uris = channel_data.ChannelUris;
			string [] result = new String [chnl_uris.Length];

			for (int i = 0; i < chnl_uris.Length; i++) 
				result [i] = chnl_uris [i] + uri;
			
			return result;
		}

		public string Parse (string url, out string objectURI)
		{
			return TcpChannel.ParseChannelUrl (url, out objectURI);
		}

		void WaitForConnections ()
		{
			try
			{
				while (true) 
				{
					TcpClient client = listener.AcceptTcpClient ();
					CreateListenerConnection (client);
				}
			}
			catch
			{}
		}

		internal void CreateListenerConnection (TcpClient client)
		{
			lock (_activeConnections)
			{
				if (_activeConnections.Count >= _maxConcurrentConnections)
					Monitor.Wait (_activeConnections);

				if (server_thread == null) return;	// Server was stopped while waiting

				ClientConnection reader = new ClientConnection (this, client, sink);
				Thread thread = new Thread (new ThreadStart (reader.ProcessMessages));
				thread.Start();
				thread.IsBackground = true;
				_activeConnections.Add (thread);
			}
		}

		internal void ReleaseConnection (Thread thread)
		{
			lock (_activeConnections)
			{
				_activeConnections.Remove (thread);
				Monitor.Pulse (_activeConnections);
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
				server_thread.IsBackground = true;
				server_thread.Start ();
			}
		}

		public void StopListening (object data)
		{
			if (server_thread == null) return;

			lock (_activeConnections)
			{
				server_thread.Abort ();
				server_thread = null;
				listener.Stop ();

				foreach (Thread thread in _activeConnections)
					thread.Abort();

				_activeConnections.Clear();
				Monitor.PulseAll (_activeConnections);
			}
		}
	}

	class ClientConnection
	{
		TcpClient _client;
		TcpServerTransportSink _sink;
		Stream _stream;
		TcpServerChannel _serverChannel;

		byte[] _buffer = new byte[TcpMessageIO.DefaultStreamBufferSize];

		public ClientConnection (TcpServerChannel serverChannel, TcpClient client, TcpServerTransportSink sink)
		{
			_serverChannel = serverChannel;
			_client = client;
			_sink = sink;
		}

		public Stream Stream
		{
			get { return _stream; }
		}

		public byte[] Buffer
		{
			get { return _buffer; }
		}

		public void ProcessMessages()
		{
			_stream = _client.GetStream();

			try
			{
				bool end = false;
				while (!end)
				{
					MessageStatus type = TcpMessageIO.ReceiveMessageStatus (_stream);

					switch (type)
					{
						case MessageStatus.MethodMessage:
							_sink.InternalProcessMessage (this);
							break;

						case MessageStatus.CancelSignal:
							end = true;
							break;
					}
				}
			}
			catch
			{}

			_stream.Close();
			_serverChannel.ReleaseConnection (Thread.CurrentThread);
		}
	}
}
