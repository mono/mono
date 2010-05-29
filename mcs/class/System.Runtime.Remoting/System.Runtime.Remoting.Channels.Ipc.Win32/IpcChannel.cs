//
// System.Runtime.Remoting.Channels.Ipc.Win32.IpcChannel.cs
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
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Channels.Ipc.Win32
{
    internal class IpcChannel : IChannelSender, IChannelReceiver, IChannel
    {
        readonly IpcClientChannel clientChannel;
        readonly IpcServerChannel serverChannel;

        /// <summary>
        /// Creates a client channel.
        /// </summary>
        public IpcChannel()
        {
            clientChannel = new IpcClientChannel();
        }

        /// <summary>
        /// Creates a server channel.
        /// </summary>
        /// <param name="portName">The port name.</param>
        public IpcChannel(string portName)
	    : this()
        {
            serverChannel = new IpcServerChannel(portName);
        }

        /// <summary>
        /// Creates both server and client channels.
        /// </summary>
        /// <param name="properties">The channel properties.</param>
        /// <param name="clientProvider">The client sink provider. It may be <c>null</c>.</param>
        /// <param name="serverProvider">The server sink provider. It may be <c>null</c>.</param>
        public IpcChannel(IDictionary properties,
            IClientChannelSinkProvider clientProvider,
            IServerChannelSinkProvider serverProvider
            )
        {
            clientChannel = new IpcClientChannel(properties, clientProvider);
            serverChannel = new IpcServerChannel(properties, serverProvider);
        }

        #region IChannelSender Members

        public IMessageSink CreateMessageSink(string url, object remoteChannelData, out string objectURI)
        {
            return clientChannel.CreateMessageSink(url, remoteChannelData, out objectURI);
        }

        #endregion

        #region IChannel Members

        public string ChannelName
        {
            get
            {
                return serverChannel != null 
                    ? serverChannel.ChannelName 
                    : clientChannel.ChannelName;
            }
        }

        public int ChannelPriority
        {
            get
            {
                return serverChannel != null
                    ? serverChannel.ChannelPriority 
                    : clientChannel.ChannelPriority;
            }
        }

        public string Parse(string url, out string objectURI)
        {
            return IpcChannelHelper.Parse(url, out objectURI);
        }

        #endregion

        #region IChannelReceiver Members

        public void StartListening(object data)
        {
            serverChannel.StartListening(data);
        }

        public object ChannelData
        {
            get
            {
                return serverChannel != null ? serverChannel.ChannelData : null;
            }
        }

        public void StopListening(object data)
        {
            serverChannel.StopListening(data);
        }

        public string[] GetUrlsForUri(string objectURI)
        {
            return serverChannel.GetUrlsForUri(objectURI);
        }

        #endregion
    }
}

#endif
