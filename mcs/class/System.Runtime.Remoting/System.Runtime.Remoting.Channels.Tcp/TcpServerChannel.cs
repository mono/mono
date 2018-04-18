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
using System.Runtime.Remoting.Channels;

namespace System.Runtime.Remoting.Channels.Tcp
{
	public class TcpServerChannel : IChannelReceiver, IChannel
	{
		int port = 0;
		string name = "tcp";
		string host = null;
		int priority = 1;
		bool supressChannelData = false;
		bool useIpAddress = true;
		
		IPAddress bindAddress = IPAddress.Any;
		Thread server_thread = null;
		TcpListener listener;
		TcpServerTransportSink sink;
		ChannelDataStore channel_data;
		
		RemotingThreadPool threadPool;
		

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
						AddressFamily addressFamily = (Socket.OSSupportsIPv4) ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6;
						IPAddress addr = GetMachineAddress (he, addressFamily);
						if (addr != null)
							host = addr.ToString ();
						else
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

			StartListening (null);
		}
		
		public TcpServerChannel (int port)
		{
			this.port = port;
			Init (null);
		}

		public TcpServerChannel (IDictionary properties,
					 IServerChannelSinkProvider sinkProvider)
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
			Init (sinkProvider);
		}

		public TcpServerChannel (string name, int port,
					 IServerChannelSinkProvider sinkProvider)
		{
			this.name = name;
			this.port = port;
			Init (sinkProvider);
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

		public virtual string [] GetUrlsForUri (string objectUri)
		{
			if (!objectUri.StartsWith ("/"))
				objectUri = "/" + objectUri;

			string [] chnl_uris = channel_data.ChannelUris;
			string [] result = new String [chnl_uris.Length];

			for (int i = 0; i < chnl_uris.Length; i++)
				result [i] = chnl_uris [i] + objectUri;

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
				while(true)
				{
					Socket socket = listener.AcceptSocket ();
					ClientConnection reader = new ClientConnection (this, socket, sink);
					try {
						if (!threadPool.RunThread (new ThreadStart (reader.ProcessMessages)))
							socket.Close ();
					} catch (Exception e) 
					{
#if DEBUG
						Console.WriteLine("Exception caught in TcpServerChannel.WaitForConnections during start process message: {0} {1}", e.GetType(), e.Message);
#endif
					}
				}
			}
			catch (Exception e)
			{
#if DEBUG
				Console.WriteLine("Exception caught in TcpServerChannel.WaitForConnections, stop channel's thread : {0} {1}", e.GetType(), e.Message);
#endif
			}
		}

		public void StartListening (object data)
		{
			listener = new TcpListener (bindAddress, port);
			if (server_thread == null) 
			{
				threadPool = RemotingThreadPool.GetSharedPool ();
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
			
			server_thread.Abort ();
			listener.Stop ();
			threadPool.Free ();
			server_thread.Join ();
			server_thread = null;			
		}

		private static IPAddress GetMachineAddress (IPHostEntry host, AddressFamily addressFamily)
		{
			IPAddress result = null;
			if (host != null) {
				IPAddress[] addressList = host.AddressList;
				for (int i = 0; i < addressList.Length; i++) {
					if (addressList[i].AddressFamily == addressFamily) {
						result = addressList[i];
						break;
					}
				}
			}

			return result;
		}
	}

	class ClientConnection
	{
		static int _count;
		int _id;
		Socket _socket;
		TcpServerTransportSink _sink;
		Stream _stream;

		byte[] _buffer = new byte[TcpMessageIO.DefaultStreamBufferSize];

		public ClientConnection (TcpServerChannel serverChannel, Socket socket, TcpServerTransportSink sink)
		{
			_socket = socket;
			_sink = sink;
			_id = _count++;
		}

		public Socket Socket {
			get { return _socket; }
		}

		public byte[] Buffer
		{
			get { return _buffer; }
		}

		public void ProcessMessages()
		{
			byte[] buffer = new byte[256];
			NetworkStream ns = new NetworkStream (_socket);
			_stream = new BufferedStream (ns);

			try
			{
				bool end = false;
				while (!end)
				{
					MessageStatus type = TcpMessageIO.ReceiveMessageStatus (_stream, buffer);

					switch (type)
					{
						case MessageStatus.MethodMessage:
							_sink.InternalProcessMessage (this, _stream);
							break;

						case MessageStatus.Unknown:
						case MessageStatus.CancelSignal:
							_stream.Flush ();
							end = true;
							break;
					}
				}
			}
#if DEBUG
			catch (Exception ex)
			{
				Console.WriteLine ("The exception was caught during TcpServerChannel.ProcessMessages: {0}, {1}", ex.GetType(), ex.Message);
			}
#endif
			finally
			{
				try {
					_stream.Close();
					_socket.Close ();
				}
				catch { }
			}
		}
		
		public int Id
		{
			get { return _id; }
		}
		
		public IPAddress ClientAddress
		{
			get {
				IPEndPoint ep = _socket.RemoteEndPoint as IPEndPoint;
				if (ep != null) return ep.Address;
				else return null;
			}
		}
	}
}
