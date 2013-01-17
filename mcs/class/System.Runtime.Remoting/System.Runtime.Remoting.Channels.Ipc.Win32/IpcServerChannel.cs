//
// System.Runtime.Remoting.Channels.Ipc.Win32.IpcServerChannel.cs
//
// Author: Robert Jordan (robertj@gmx.net)
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

#if NET_2_0

using System;
using System.Collections;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Threading;

namespace System.Runtime.Remoting.Channels.Ipc.Win32
{
    internal class IpcServerChannel : IChannelReceiver, IChannel
    {
        readonly string portName;
        readonly string channelName = IpcChannelHelper.Scheme;
        readonly int channelPriority = 1;
        readonly IServerChannelSinkProvider serverProvider;
        readonly IpcServerChannelSink sink;
        readonly ChannelDataStore dataStore;
        Thread worker;

        /// <summary>
        /// Builds the default channel properties
        /// </summary>
        /// <param name="portName">The pipe name.</param>
        /// <returns></returns>
        internal static IDictionary BuildDefaultProperties(string portName)
        {
            Hashtable h = new Hashtable();
            h.Add("portName", portName);
            return h;
        }

        /// <summary>
        /// Builds the default channel properties
        /// </summary>
        /// <param name="portName">The pipe name.</param>
        /// <returns></returns>
        internal static IDictionary BuildDefaultProperties(string name, string portName)
        {
            Hashtable h = new Hashtable();
            h.Add("name", name);
            h.Add("portName", portName);
            return h;
        }

        /// <summary>
        /// Creates a server channel
        /// </summary>
        /// <param name="portName">The port name.</param>
        public IpcServerChannel(string portName)
            : this(BuildDefaultProperties(portName), null)
        {
        }

        /// <summary>
        /// Creates a server channel
        /// </summary>
        /// <param name="mame">The channel name.</param>
        /// <param name="portName">The port name.</param>
        public IpcServerChannel(string name, string portName)
            : this(BuildDefaultProperties(name, portName), null)
        {
        }

        /// <summary>
        /// Creates a server channel
        /// </summary>
        /// <param name="mame">The channel name.</param>
        /// <param name="portName">The port name.</param>
        /// <param name="provider">The sink provider.</param>
        public IpcServerChannel(string name, string portName,
                                IServerChannelSinkProvider provider)
            : this(BuildDefaultProperties(name, portName), provider)
        {
        }

        /// <summary>
        /// Creates a server channel.
        /// </summary>
        /// <param name="properties">The channel properties.</param>
        /// <param name="provider">The sink provider.</param>
        public IpcServerChannel(IDictionary properties, IServerChannelSinkProvider provider)
        {
            bool impersonate = false;

            if (properties != null) 
            {
                foreach (DictionaryEntry e in properties) 
                {
                    switch ((string)e.Key) 
                    {
                        case "name":
                            channelName = (string)e.Value;
                            break;

                        case "priority":
                            channelPriority = Convert.ToInt32(e.Value);
                            break;

                        case "portName":
                            portName = (string)e.Value;
                            if (!IpcChannelHelper.IsValidPipeName(portName))
                                throw new ArgumentException("Invalid pipe name.", "portName");
                            break;

                        case "impersonate":
                            impersonate = Boolean.Parse((string)e.Value);
                            break;
                    }
                }
            }

            if (portName == null) 
            {
                portName = Guid.NewGuid().ToString("N");
            }

            serverProvider = provider;

            if (serverProvider == null) 
            {
                serverProvider = new BinaryServerFormatterSinkProvider();
            }

            dataStore = new ChannelDataStore(
                new string[] {IpcChannelHelper.SchemeStart + portName}
                );
            PopulateChannelData(dataStore, serverProvider);

            sink = new IpcServerChannelSink(
                ChannelServices.CreateServerChannelSinkChain(serverProvider, this),
                portName,
                impersonate
                );

            StartListening(null);
        }

        void PopulateChannelData( ChannelDataStore channelData,
            IServerChannelSinkProvider provider)
        {
            while (provider != null)
            {
                provider.GetChannelData(channelData);
                provider = provider.Next;
            }
        }


        #region IChannelReceiver Members

        public void StartListening(object data)
        {
            if (worker == null) 
            {
                worker = new Thread(new ThreadStart(sink.Listen));
                worker.IsBackground = true;
                worker.Start();
            }
        }

        public object ChannelData
        {
            get
            {
                return dataStore;
            }
        }

        public void StopListening(object data)
        {
            if (worker != null) 
            {
                worker.Abort();
                worker = null;
            }
        }

        public string[] GetUrlsForUri(string objectURI)
        {
            if (!objectURI.StartsWith("/")) objectURI = "/" + objectURI;
            string[] urls = new string[1];
            urls[0] = IpcChannelHelper.SchemeStart + portName + objectURI;
            return urls;
        }

        #endregion

        #region IChannel Members

        public string ChannelName
        {
            get
            {
                return channelName;
            }
        }

        public int ChannelPriority
        {
            get
            {
                return channelPriority;
            }
        }

        public string Parse(string url, out string objectURI)
        {
            return IpcChannelHelper.Parse(url, out objectURI);
        }

        #endregion
    }

    internal class IpcServerChannelSink : IServerChannelSink
    {
        IServerChannelSink nextSink;
        string portName;
        bool impersonate;

        public IpcServerChannelSink(IServerChannelSink nextSink, string portName, bool impersonate) 
        {
            this.nextSink = nextSink;
            this.portName = portName;
            this.impersonate = impersonate;
        }

        #region IServerChannelSink Members

        public Stream GetResponseStream(IServerResponseChannelSinkStack sinkStack, object state, IMessage msg, ITransportHeaders headers)
        {
            return null;
        }

        public ServerProcessing ProcessMessage(IServerChannelSinkStack sinkStack, IMessage requestMsg, ITransportHeaders requestHeaders, Stream requestStream, out IMessage responseMsg, out ITransportHeaders responseHeaders, out Stream responseStream)
        {
            throw new NotSupportedException();
        }

        public void AsyncProcessResponse(IServerResponseChannelSinkStack sinkStack, object state, IMessage msg, ITransportHeaders headers, Stream stream)
        {
        }

        public IServerChannelSink NextChannelSink
        {
            get
            {
                return nextSink;
            }
        }

        #endregion

        #region IChannelSinkBase Members

        public IDictionary Properties
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Listens for incoming requests.
        /// </summary>
        internal void Listen() 
        {
            while (true) 
            {
                try 
                {
                    NamedPipeListener listener = new NamedPipeListener(portName);
                    NamedPipeSocket socket = listener.Accept();
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessClient), socket);
                }
                catch (NamedPipeException) 
                {
                }
            }
        }

        void ProcessClient(object state) 
        {
            try 
            {
                NamedPipeSocket socket = (NamedPipeSocket) state;

                ITransportHeaders requestHeaders;
                Stream requestStream;

                IpcTransport t = new IpcTransport(socket);
                t.Read(out requestHeaders, out requestStream);

                // parse the RequestUri
                string objectUri;
                string uri = (string) requestHeaders[CommonTransportKeys.RequestUri];
                IpcChannelHelper.Parse(uri, out objectUri);
                if (objectUri == null) objectUri = uri;
                requestHeaders[CommonTransportKeys.RequestUri] = objectUri;

                ServerChannelSinkStack stack = new ServerChannelSinkStack();
                stack.Push(this, null);

                IMessage responseMsg;
                ITransportHeaders responseHeaders;
                Stream responseStream;

                requestHeaders["__CustomErrorsEnabled"] = false;

                if (impersonate) 
                {
                    // TODO: Impersonate might throw exceptions. What to do with them?
                    socket.Impersonate();
                }

                ServerProcessing op = nextSink.ProcessMessage(
                    stack,
                    null,
                    requestHeaders,
                    requestStream,
                    out responseMsg,
                    out responseHeaders,
                    out responseStream
                    );

                if (impersonate) 
                {
                    NamedPipeSocket.RevertToSelf();
                }

                switch (op) 
                {
                    case ServerProcessing.Complete:
                        stack.Pop(this);
                        // send the response headers and the response data to the client
                        t.Write(responseHeaders, responseStream);
                        break;

                    case ServerProcessing.Async:
                        stack.StoreAndDispatch(nextSink, null);
                        break;

                    case ServerProcessing.OneWay:
                        break;
                }
            }
            catch (Exception)
            {
                // Console.WriteLine(ex);
            }
        }

        #endregion

    }

}

#endif
