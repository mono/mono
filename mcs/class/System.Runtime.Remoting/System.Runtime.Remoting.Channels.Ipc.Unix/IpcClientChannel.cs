//
// System.Runtime.Remoting.Channels.Ipc.Unix.IpcClientChannel.cs
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
using Win32 = System.Runtime.Remoting.Channels.Ipc.Win32;

namespace System.Runtime.Remoting.Channels.Ipc.Unix
{
        internal class IpcClientChannel : IChannelSender, IChannel
        {
                object _innerChannel;

                public IpcClientChannel ()
                {
                        _innerChannel = Activator.CreateInstance(UnixChannelLoader.LoadClientChannel ());
                }

                public IpcClientChannel (IDictionary properties,
                                         IClientChannelSinkProvider sinkProvider)
                {
                        _innerChannel = Activator.CreateInstance(UnixChannelLoader.LoadClientChannel (), new object [] {properties, sinkProvider});
                }

                public IpcClientChannel (string name,
                                         IClientChannelSinkProvider sinkProvider)
                {
                        _innerChannel = Activator.CreateInstance(UnixChannelLoader.LoadClientChannel (), new object [] {name, sinkProvider});
                }

                public string ChannelName
                {
                        get { return ((IChannel)_innerChannel).ChannelName; }
                }

                public int ChannelPriority
                {
                        get { return ((IChannel)_innerChannel).ChannelPriority; }
                }

                public string Parse (string url, out string objectUri)
                {
                        return Win32.IpcChannelHelper.Parse (url, out objectUri);
                }


                //
                // Converts an ipc URL to a unix URL.
                // Returns the URL unchanged if it was not ipc.
                //
                internal static string IpcToUnix (string url)
                {
			if (url == null)
				return null;

                        string portName;
                        string objectUri;
                        Win32.IpcChannelHelper.Parse (url, out portName, out objectUri);
                        if (objectUri != null)
                                url = "unix://" + Path.Combine (Path.GetTempPath (), portName) + "?" + objectUri;
                        return url;
                }

                public IMessageSink CreateMessageSink(string url,
                                                      object remoteChannelData,
                                                      out string objectUri)
                {
                        url = IpcToUnix (url);
                        IMessageSink sink = ((IChannelSender)_innerChannel).CreateMessageSink (url, remoteChannelData, out objectUri);

                        if (sink != null)
                                return new UrlMapperSink (sink);
                        else
                                return null;
                }
        }


        //
        // Simple message sink that changes ipc URLs to unix URLs.
        //
        sealed class UrlMapperSink : IMessageSink
        {
                readonly IMessageSink _sink;

                public UrlMapperSink (IMessageSink sink)
                {
                        _sink = sink;
                }

                public IMessageSink NextSink
                {
                        get { return _sink.NextSink; }
                }

                static void ChangeUri (IMessage msg)
                {
                        string uri = msg.Properties ["__Uri"] as string;
                        if (uri != null) {
				 msg.Properties ["__Uri"] = IpcClientChannel.IpcToUnix (uri);
                        }
                }

                public IMessage SyncProcessMessage(IMessage msg)
                {
                        ChangeUri (msg);
                        return _sink.SyncProcessMessage (msg);
                }

                public IMessageCtrl AsyncProcessMessage(IMessage msg,
                                                        IMessageSink replySink)
                {
                        ChangeUri (msg);
                        return _sink.AsyncProcessMessage (msg, replySink);
                }

        }
}

#endif
