//
// System.Runtime.Remoting.Channels.Ipc.Unix.IpcChannel.cs
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
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Channels.Ipc.Unix
{
        internal class IpcChannel : IChannelReceiver, IChannelSender, IChannel
        {
                IChannelSender _clientChannel;
                IChannelReceiver _serverChannel;

                string _portName;
                string _name = "ipc";
                int  _priority = 1;

                internal static IDictionary GetDefaultProperties (string portName)
                {
                        Hashtable h = new Hashtable ();
                        if (portName != null)
                                h ["portName"] = portName;
                        h ["name"] = "ipc";
                        h ["priority"] = "1";
                        return h;
                }

                public IpcChannel () : this (null)
                {
                }

                public IpcChannel (string portName)
                        : this (GetDefaultProperties (portName), null, null)
                {
                }

                public IpcChannel (IDictionary properties,
                                   IClientChannelSinkProvider clientSinkProvider,
                                   IServerChannelSinkProvider serverSinkProvider)
                {
                        if (properties != null) {
                                _portName = properties ["portName"] as string;
                                if (properties ["name"] != null)
                                        _name = properties ["name"] as string;
                                else
                                        properties ["name"] = _name;
                                if (properties ["priority"] != null)
                                        _priority = Convert.ToInt32 (properties ["priority"]);
                        }

                        if (_portName != null)
                                _serverChannel = new IpcServerChannel (properties, serverSinkProvider);

                        _clientChannel = new IpcClientChannel (properties, clientSinkProvider);
                }

                public string ChannelName
                {
                        get { return _name; }
                }

                public int ChannelPriority
                {
                        get { return _priority; }
                }

                public string Parse (string url, out string objectUri)
                {
                        if (_serverChannel != null)
                                return _serverChannel.Parse (url, out objectUri);
                        else
                                return _clientChannel.Parse (url, out objectUri);
                                        
                }

                public IMessageSink CreateMessageSink(string url,
                                                      object remoteChannelData,
                                                      out string objectUri)
                {
                        return _clientChannel.CreateMessageSink (url, remoteChannelData, out objectUri);
                }

                public object ChannelData
                {
                        get {
                                if (_serverChannel != null)
                                        return _serverChannel.ChannelData;
                                else
                                        return null;
                        }
                }

                public string[] GetUrlsForUri(string objectUri)
                {
                        if (_serverChannel != null)
                                return _serverChannel.GetUrlsForUri (objectUri);
                        else
                                return null;
                }

                public void StartListening (object data)
                {
                        if (_serverChannel != null)
                                _serverChannel.StartListening (data);
                }

                public void StopListening(object data)
                {
                        if (_serverChannel != null)
                                _serverChannel.StopListening (data);
                }

        }
}

#endif
