//
// System.Runtime.Remoting.Channels.Tcp.TcpServerChannel.cs
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
		string host = null;
		int priority = 1;
		bool supressChannelData = false;
		bool useIpAddress = false;
		
		IPAddress bindAddress = IPAddress.Any;
		Thread server_thread = null;
		TcpListener listener;
		TcpServerTransportSink sink;
		ChannelDataStore channel_data;
		int _maxConcurrentConnections = 100;
		ArrayList _activeConnections = new ArrayList();
		
		
		void Init (IServerChannelSinkProvider serverSinkProvider) 
		{
			if (serverSinkProvider == null) 
			{
				serverSinkProvider = new BinaryServerFormatterSinkProvider ();
			}

			if (host == null)
			{
				if (useIpAddress) {
					if (!bindAddress.Equals(IPAddress.Any)) host = bindAddress.ToString ();
					else {
						IPHostEntry he = Dns.Resolve (Dns.GetHostName());
						if (he.AddressList.Length == 0) throw new RemotingException ("IP address could not be determined for this host");
						host = he.AddressList [0].ToString ();
					}
				}
				else
					host = Dns.GetHostByName(Dns.GetHostName()).HostName;
			}
			
			// Gets channel data from the chain of channel providers

			channel_data = new ChannelDataStore (null);
			IServerChannelSinkProvider provider = serverSinkProvider;
			while (provider != null)
			{
				provider.GetChannelData(channel_data);
				provider = provider.Next;
			}

			// Creates the sink chain that will process all incoming messages

			IServerChannelSink next_sink = ChannelServices.CreateServerChannelSinkChain (serverSinkProvider, this);
			sink = new TcpServerTransportSink (next_sink);
		}
		
		public TcpServerChannel (int port)
		{
			this.port = port;
			Init (null);
		}

		public TcpServerChannel (IDictionary properties,
					 IServerChannelSinkProvider serverSinkProvider)
		{
			foreach(DictionaryEntry property in properties)
			{
				switch((string)property.Key)
				{
					case "name":
						name = property.Value.ToString();
						break;
					case "port":
						port = Convert.ToInt32(property.Value);
						break;
					case "priority":
						priority = Convert.ToInt32(property.Value);
						break;
					case "bindTo":
						bindAddress = IPAddress.Parse((string)property.Value);
						break;
					case "rejectRemoteRequests":
						if(Convert.ToBoolean(properties["rejectRemoteRequests"]))
							bindAddress = IPAddress.Loopback;
						break;
					case "supressChannelData":
						supressChannelData = Convert.ToBoolean (property.Value);
						break;
					case "useIpAddress":
						useIpAddress = Convert.ToBoolean (property.Value);
						break;
					case "machineName":
						host = property.Value as string;
						break;
				}
			}			
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
				if (supressChannelData) return null;
				else return channel_data;
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
			listener = new TcpListener (bindAddress, port);
			if (server_thread == null) 
			{
				listener.Start ();
				
				if (port == 0)
					port = ((IPEndPoint)listener.LocalEndpoint).Port;

				string[] uris = new String [1];
				uris = new String [1];
				uris [0] = GetChannelUri ();
				channel_data.ChannelUris = uris;

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
			catch (Exception ex)
			{
//				Console.WriteLine (ex);
			}
			finally
			{
				_stream.Close();
				_serverChannel.ReleaseConnection (Thread.CurrentThread);
			}
		}
		
		public bool IsLocal
		{
			get
			{
				return true;
			}
		}
	}
}
