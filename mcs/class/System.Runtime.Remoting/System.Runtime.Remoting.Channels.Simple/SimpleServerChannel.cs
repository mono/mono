//
// System.Runtime.Remoting.Channels.Simple.SimpleServerChannel.cs
//
// Author: Dietmar Maurer (dietmar@ximian.com)
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

namespace System.Runtime.Remoting.Channels.Simple
{
	public class SimpleServerChannel : IChannelReceiver, IChannel
	{
		int port = 0;
		string name = "simple";
		string host;
		int priority = 1;
		Thread server_thread = null;
		TcpListener listener;
		SimpleServerTransportSink sink;
		ChannelDataStore channel_data;
		
		void Init (IServerChannelSinkProvider provider) {
			if (provider == null) {
				provider = new SimpleServerFormatterSinkProvider ();
			}
			
			IServerChannelSink next_sink = ChannelServices.CreateServerChannelSinkChain (provider, this);

			host = Dns.GetHostByName(Dns.GetHostName()).HostName;
			
			string [] uris = null;
			
			if (port != 0) {
				uris = new String [1];
				uris [0] = GetChannelUri ();
			}
			
			channel_data = new ChannelDataStore (uris);;

			sink = new SimpleServerTransportSink (next_sink);
			
			listener = new TcpListener (port);
			StartListening (null);
		}
		
		public SimpleServerChannel (int port)
		{
			this.port = port;
			Init (null);
		}

		public SimpleServerChannel (IDictionary properties,
					    IServerChannelSinkProvider serverSinkProvider)
		{
			port = (int)properties ["port"];
			Init (serverSinkProvider);
		}

		public SimpleServerChannel (string name, int port,
					    IServerChannelSinkProvider serverSinkProvider)
		{
			name = name;
			this.port = port;
			Init (serverSinkProvider);
		}
		
		public SimpleServerChannel (string name, int port)
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

		string GetChannelUri ()
		{
			return "simple://" + host + ":" + port;
		}
		
		public string[] GetUrlsForUri (string uri)
		{
			string [] chnl_uris = channel_data.ChannelUris;
			
			if (uri.IndexOf ('/') != 0)
				uri = "/" + uri;

			string [] result = new String [chnl_uris.Length];

			for (int i = 0; i < chnl_uris.Length; i++) {
				result [i] = chnl_uris [i] + uri;
			}
			
			return result;
		}

		public string Parse (string url, out string objectURI)
		{
			int port;
			
			string host = SimpleChannel.ParseSimpleURL (url, out objectURI, out port);

			return GetChannelUri ();
		}

		void WaitForConnections ()
		{
			TcpClient client = listener.AcceptTcpClient ();
			Stream network_stream = client.GetStream ();

			while (true) {

				sink.InternalProcessMessage (network_stream);

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
