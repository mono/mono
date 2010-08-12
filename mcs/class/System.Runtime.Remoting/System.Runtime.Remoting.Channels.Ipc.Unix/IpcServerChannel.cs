//
// System.Runtime.Remoting.Channels.Ipc.Unix.IpcServerChannel.cs
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
using Win32 = System.Runtime.Remoting.Channels.Ipc.Win32;

namespace System.Runtime.Remoting.Channels.Ipc.Unix
{
        internal class IpcServerChannel : IChannelReceiver, IChannel
        {
                object _innerChannel;
                string _portName;
                string _path;

                internal static string BuildPathFromPortName (string portName)
                {
                        if (!Win32.IpcChannelHelper.IsValidPipeName (portName))
                                throw new RemotingException ("Invalid IPC port name");
                        return Path.Combine (Path.GetTempPath (), portName);
                }

                internal static IDictionary MapProperties (IDictionary props)
                {
                        if (props == null) return null;
                        Hashtable h = new Hashtable ();

                        foreach (DictionaryEntry e in props) {
                                h [e.Key] = e.Value;
				
                                switch (e.Key as string) {
                                case "portName":
                                        h ["path"] = BuildPathFromPortName ((string)e.Value);
                                        break;
                                }
                        }
                        return h;
                }

                public IpcServerChannel (string portName)
                {
                        _portName = portName;
                        _path = portName = BuildPathFromPortName (portName);

                        _innerChannel = Activator.CreateInstance(UnixChannelLoader.LoadServerChannel (), new object [] {portName});
                }

                public IpcServerChannel (string name, string portName,
                                         IServerChannelSinkProvider serverSinkProvider)
                {
                        _portName = portName;
                        _path = portName = BuildPathFromPortName (portName);

                        _innerChannel = Activator.CreateInstance(UnixChannelLoader.LoadServerChannel (), new object [] {name, portName, serverSinkProvider});
                }
        
                public IpcServerChannel (string name, string portName)
                {
                        _portName = portName;
                        _path = portName = BuildPathFromPortName (portName);

                        _innerChannel = Activator.CreateInstance(UnixChannelLoader.LoadServerChannel (), new object [] {name, portName});
                }

                public IpcServerChannel (IDictionary properties,
                                         IServerChannelSinkProvider serverSinkProvider)
                {
                        properties = MapProperties (properties);
                        if (properties != null) {
                                _portName = properties ["portName"] as string;
                                _path = properties ["path"] as string;
                        }

                        _innerChannel = Activator.CreateInstance(UnixChannelLoader.LoadServerChannel (), new object [] {properties, serverSinkProvider});
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

                public object ChannelData
                {
                        get { return ((IChannelReceiver)_innerChannel).ChannelData; }
                }

                public string[] GetUrlsForUri(string objectUri)
                {
                        string[] res = ((IChannelReceiver)_innerChannel).GetUrlsForUri (objectUri);
                        if (res != null) {
                                string[] urls = new string [res.Length + 1];

                                for (int i = 0; i < res.Length; i++)
                                        urls [i] = res [i];

                                if (!objectUri.StartsWith ("/"))
                                        objectUri = "/" + objectUri;
                                urls [res.Length] = "ipc://" + _portName + objectUri;
                                return urls;
                        }
                        return res;
                }

                public void StartListening (object data)
                {
                        ((IChannelReceiver)_innerChannel).StartListening (data);
                }

                public void StopListening(object data)
                {
                        ((IChannelReceiver)_innerChannel).StopListening (data);
                }
        }
}

#endif
