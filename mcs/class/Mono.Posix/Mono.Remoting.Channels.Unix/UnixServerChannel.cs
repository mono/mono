//
// Mono.Remoting.Channels.Unix.UnixServerChannel.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//     Lluis Sanchez Gual (lluis@ideary.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using System.Runtime.Remoting.Channels;
using Mono.Unix;

namespace Mono.Remoting.Channels.Unix
{
    public class UnixServerChannel : IChannelReceiver, IChannel
    {
        string path = null;
        string name = "unix";

        int priority = 1;
        bool supressChannelData = false;
        
        Thread server_thread = null;
        UnixListener listener;
        UnixServerTransportSink sink;
        ChannelDataStore channel_data;
        int _maxConcurrentConnections = 100;
        ArrayList _activeConnections = new ArrayList();
        
        
        void Init (IServerChannelSinkProvider serverSinkProvider) 
        {
            if (serverSinkProvider == null) 
            {
                serverSinkProvider = new UnixBinaryServerFormatterSinkProvider ();
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
            sink = new UnixServerTransportSink (next_sink);
            
            StartListening (null);
        }
        
        public UnixServerChannel (string path)
        {
            this.path = path;
            Init (null);
        }

        public UnixServerChannel (IDictionary properties,
                                  IServerChannelSinkProvider serverSinkProvider)
        {
            foreach(DictionaryEntry property in properties)
            {
                switch((string)property.Key)
                {
                case "path":
                    path = property.Value as string;
                    break;
                case "priority":
                    priority = Convert.ToInt32(property.Value);
                    break;
                case "supressChannelData":
                    supressChannelData = Convert.ToBoolean (property.Value);
                    break;
                }
            }            
            Init (serverSinkProvider);
        }

        public UnixServerChannel (string name, string path,
                                  IServerChannelSinkProvider serverSinkProvider)
        {
            this.name = name;
            this.path = path;
            Init (serverSinkProvider);
        }
        
        public UnixServerChannel (string name, string path)
        {
            this.name = name;
            this.path = path;
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
            return "unix://" + path;
        }
        
        public string[] GetUrlsForUri (string uri)
        {
            if (!uri.StartsWith ("/")) uri = "/" + uri;

            string [] chnl_uris = channel_data.ChannelUris;
            string [] result = new String [chnl_uris.Length];

            for (int i = 0; i < chnl_uris.Length; i++) 
                result [i] = chnl_uris [i] + "?" + uri;
            
            return result;
        }

        public string Parse (string url, out string objectURI)
        {
            return UnixChannel.ParseUnixURL (url, out objectURI);
        }

        void WaitForConnections ()
        {
            try
            {
                while (true) 
                {
                    Socket client = listener.AcceptSocket ();
                    CreateListenerConnection (client);
                }
            }
            catch
            {}
        }

        internal void CreateListenerConnection (Socket client)
        {
            lock (_activeConnections)
                {
                    if (_activeConnections.Count >= _maxConcurrentConnections)
                        Monitor.Wait (_activeConnections);

                    if (server_thread == null) return;    // Server was stopped while waiting

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
            listener = new UnixListener (path);
            Mono.Unix.Native.Syscall.chmod (path,
                                     Mono.Unix.Native.FilePermissions.S_IRUSR |
                                     Mono.Unix.Native.FilePermissions.S_IWUSR |
                                     Mono.Unix.Native.FilePermissions.S_IRGRP |
                                     Mono.Unix.Native.FilePermissions.S_IWGRP |
                                     Mono.Unix.Native.FilePermissions.S_IROTH |
                                     Mono.Unix.Native.FilePermissions.S_IWOTH);

            if (server_thread == null) 
            {
                listener.Start ();
                
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
        Socket _client;
        UnixServerTransportSink _sink;
        Stream _stream;
        UnixServerChannel _serverChannel;

        byte[] _buffer = new byte[UnixMessageIO.DefaultStreamBufferSize];

        public ClientConnection (UnixServerChannel serverChannel, Socket client, UnixServerTransportSink sink)
        {
            _serverChannel = serverChannel;
            _client = client;
            _sink = sink;
        }

        public Socket Client {
            get { return _client; }
        }

        public byte[] Buffer
        {
            get { return _buffer; }
        }

        public void ProcessMessages()
        {
			byte[] buffer = new byte[256];
            _stream = new BufferedStream (new NetworkStream (_client));

            try
            {
                bool end = false;
                while (!end)
                {
                    MessageStatus type = UnixMessageIO.ReceiveMessageStatus (_stream, buffer);

                    switch (type)
                    {
						case MessageStatus.MethodMessage:
							_sink.InternalProcessMessage (this, _stream);
							break;

						case MessageStatus.Unknown:
						case MessageStatus.CancelSignal:
							end = true;
							break;
                    }
                }
            }
            catch (Exception)
            {
                //                Console.WriteLine (ex);
            }
            finally
            {
				try {
	                _serverChannel.ReleaseConnection (Thread.CurrentThread);
	                _stream.Close();
					_client.Close ();
				} catch {
				}
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
