//
// System.Runtime.Remoting.Channels.Ipc.Win32.IpcClientChannel.cs
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

namespace System.Runtime.Remoting.Channels.Ipc.Win32
{
    internal class IpcClientChannel : IChannelSender, IChannel
    {
        readonly string channelName = IpcChannelHelper.Scheme;
        readonly int channelPriority = 1;
        readonly IClientChannelSinkProvider clientProvider;

        /// <summary>
        /// Creates a default client channel.
        /// </summary>
        public IpcClientChannel()
            : this ((IDictionary)null, null)
        {
        }

        /// <summary>
        /// Creates a default client channel.
        /// </summary>
        /// <param name="name">The channel name.</param>
        /// <param name="sinkProvider">The provider</param>
        public IpcClientChannel(string name, IClientChannelSinkProvider provider)
            : this (IpcServerChannel.BuildDefaultProperties (name), provider)
        {
        }

        /// <summary>
        /// Creates a client channel.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="sinkProvider">The provider</param>
        public IpcClientChannel(IDictionary properties, IClientChannelSinkProvider provider)
        {
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
                    }
                }
            }

            if (provider == null) 
            {
                clientProvider = new BinaryClientFormatterSinkProvider();
                clientProvider.Next = new IpcClientChannelSinkProvider();
            }
            else 
            {
                // add us to the sink chain.
                clientProvider = provider;
                IClientChannelSinkProvider p;
                for (p = clientProvider; p.Next != null; p = p.Next) {}
                p.Next = new IpcClientChannelSinkProvider();
            }
                                        
        }

        #region IChannelSender Members

        public IMessageSink CreateMessageSink(string url, object remoteChannelData, out string objectURI)
        {
            objectURI = null;
            string channelUri = null;

            if (url != null) 
            {
                channelUri = Parse(url, out objectURI);
            }

            if (channelUri == null) 
            {
                // get url from the channel data
                IChannelDataStore ds = remoteChannelData as IChannelDataStore;
                if (ds != null) 
                {
                    channelUri = Parse(ds.ChannelUris[0], out objectURI);
                    if (channelUri != null)
                        url = ds.ChannelUris[0];
                }
            }

            if (channelUri != null) 
            {
                return (IMessageSink) clientProvider.CreateSink(this, url, remoteChannelData);
            }
            else 
            {
                return null;
            }
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

    internal class IpcClientChannelSinkProvider : IClientChannelSinkProvider
    {
        #region IClientChannelSinkProvider Members

        public IClientChannelSink CreateSink(IChannelSender channel, string url, object remoteChannelData)
        {
            return new IpcClientChannelSink(url);
        }

        public IClientChannelSinkProvider Next
        {
            get
            {
                return null;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        #endregion
    }

    internal class IpcClientChannelSink : IClientChannelSink
    {
        readonly string pipeName;

        public IpcClientChannelSink(string url) 
        {
            string unused;
            IpcChannelHelper.Parse(url, out pipeName, out unused);
        }

        #region IClientChannelSink Members

        delegate void AsyncResponse(IClientChannelSinkStack sinkStack, IpcTransport transport);

        public void AsyncProcessRequest(IClientChannelSinkStack sinkStack, IMessage msg, ITransportHeaders headers, Stream stream)
        {
            headers[CommonTransportKeys.RequestUri] = ((IMethodCallMessage)msg).Uri;

            // connect
            NamedPipeClient client = new NamedPipeClient(pipeName);
            NamedPipeSocket socket = client.Connect();
            IpcTransport t = new IpcTransport(socket);
            t.Write(headers, stream);

            // schedule an async call
            if (!RemotingServices.IsOneWay(((IMethodCallMessage)msg).MethodBase)) 
            {
                new AsyncResponse(AsyncHandler).BeginInvoke(sinkStack, t, null, null);
            }
        }

        void AsyncHandler(IClientChannelSinkStack sinkStack, IpcTransport transport) 
        {
            try 
            {
                // get the response headers and the response stream from the server
                ITransportHeaders responseHeaders;
                Stream responseStream;
                transport.Read(out responseHeaders, out responseStream);
                transport.Close();
                sinkStack.AsyncProcessResponse(responseHeaders, responseStream);
            }
            catch (Exception ex) 
            {
                sinkStack.DispatchException(ex);
            }
        }

        public void ProcessMessage(IMessage msg, ITransportHeaders requestHeaders, Stream requestStream, out ITransportHeaders responseHeaders, out Stream responseStream)
        {
            responseHeaders = null;
            responseStream = null;

            requestHeaders[CommonTransportKeys.RequestUri] = ((IMethodCallMessage)msg).Uri;

            // connect
            NamedPipeClient client = new NamedPipeClient(pipeName);
            NamedPipeSocket socket = client.Connect();
            IpcTransport t = new IpcTransport(socket);
            t.Write(requestHeaders, requestStream);
            t.Read(out responseHeaders, out responseStream);
            t.Close();
        }

        public void AsyncProcessResponse(IClientResponseChannelSinkStack sinkStack, object state, ITransportHeaders headers, Stream stream)
        {
            throw new NotSupportedException();
        }

        public Stream GetRequestStream(IMessage msg, ITransportHeaders headers)
        {
            return null;
        }

        public IClientChannelSink NextChannelSink
        {
            get { return null; }
        }

        #endregion

        #region IChannelSinkBase Members

        public IDictionary Properties
        {
            get { return null; }
        }

        #endregion

    }

}

#endif
