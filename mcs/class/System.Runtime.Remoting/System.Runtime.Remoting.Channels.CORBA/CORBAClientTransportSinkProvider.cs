//
// System.Runtime.Remoting.Channels.CORBA.CORBAClientTransportSinkProvider.cs
//
// Author: Dietmar Maurer (dietmar@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Channels;

namespace System.Runtime.Remoting.Channels.CORBA
{
	public class CORBAClientTransportSinkProvider : IClientChannelSinkProvider
	{
		public CORBAClientTransportSinkProvider ()
		{
			// what should we do here ?
		}

		public IClientChannelSinkProvider Next
		{
			get {
				return null;
			}

			set {
				// ignore, we are always the last in the chain 
			}
		}

		public IClientChannelSink CreateSink (IChannelSender channel, string url,
						      object remoteChannelData)
		{
			return new CORBAClientTransportSink (url);
		}
	}
}
