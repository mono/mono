//
// System.Runtime.Remoting.Channels.Tcp.TcpClientTransportSinkProvider.cs
//
// Author: Dietmar Maurer (dietmar@ximian.com)
//         Lluis Sanchez (lsg@ctv.es)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System;
using System.Runtime.Remoting.Channels;

namespace System.Runtime.Remoting.Channels.Tcp
{
	public class TcpClientTransportSinkProvider : IClientChannelSinkProvider
	{
		public TcpClientTransportSinkProvider ()
		{
			// what should we do here ?
		}

		public IClientChannelSinkProvider Next
		{
			get 
			{
				return null;
			}

			set 
			{
				// ignore, we are always the last in the chain 
			}
		}

		public IClientChannelSink CreateSink (IChannelSender channel, string url,
			object remoteChannelData)
		{
			return new TcpClientTransportSink (url);
		}
	}
}
