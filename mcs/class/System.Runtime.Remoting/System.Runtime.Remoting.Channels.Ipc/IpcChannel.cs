//
// System.Runtime.Remoting.Channels.Ipc.IpcChannel.cs
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
using System.Runtime.Remoting.Messaging;

using Unix  = System.Runtime.Remoting.Channels.Ipc.Unix;
using Win32 = System.Runtime.Remoting.Channels.Ipc.Win32;

namespace System.Runtime.Remoting.Channels.Ipc
{
        public class IpcChannel : IChannelReceiver, IChannelSender, IChannel
        {
                IChannel _innerChannel;

                internal static bool IsUnix
                {
                        get { 
                                int p = (int) Environment.OSVersion.Platform;
                                return ((p == 4) || (p == 128) || (p == 6));
                        }
                }

                public IpcChannel ()
                {
                        if (IsUnix)
                                _innerChannel = new Unix.IpcChannel ();
                        else
                                _innerChannel = new Win32.IpcChannel ();
                }

                public IpcChannel (string portName)
                {
                        if (IsUnix)
                                _innerChannel = new Unix.IpcChannel (portName);
                        else
                                _innerChannel = new Win32.IpcChannel (portName);
                }

                public IpcChannel (IDictionary properties,
                                   IClientChannelSinkProvider clientSinkProvider,
                                   IServerChannelSinkProvider serverSinkProvider)
                {
                        if (IsUnix)
                                _innerChannel = new Unix.IpcChannel (properties, clientSinkProvider, serverSinkProvider);
                        else
                                _innerChannel = new Win32.IpcChannel (properties, clientSinkProvider, serverSinkProvider);
                }

                public string ChannelName
                {
                        get { return _innerChannel.ChannelName; }
                }

                public int ChannelPriority
                {
                        get { return _innerChannel.ChannelPriority; }
                }

                public string Parse (string url, out string objectURI)
                {
                        return _innerChannel.Parse (url, out objectURI);
                }

                public IMessageSink CreateMessageSink (string url,
                                                       object remoteChannelData,
                                                       out string objectURI)
                {
                        return ((IChannelSender)_innerChannel).CreateMessageSink (url, remoteChannelData, out  objectURI);
                }

                public object ChannelData
                {
                        get { return ((IChannelReceiver)_innerChannel).ChannelData; }
                }

                public string[] GetUrlsForUri (string objectURI)
                {
                        return ((IChannelReceiver)_innerChannel).GetUrlsForUri (objectURI);
                }

                public void StartListening (object data)
                {
                        ((IChannelReceiver)_innerChannel).StartListening (data);
                }

                public void StopListening (object data)
                {
                        ((IChannelReceiver)_innerChannel).StopListening (data);
                }

        }
}

#endif
