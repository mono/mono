//
// System.Runtime.Remoting.Channels.Tcp.TcpClientChannel.cs
//
// Author: Dietmar Maurer (dietmar@ximian.com)
//         Lluis Sanchez Gual (lluis@ideary.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Channels;
using System.Threading;

namespace System.Runtime.Remoting.Channels.Tcp
{
	public class TcpClientChannel : IChannelSender, IChannel
	{
		int priority = 1;					
		string name = "tcp";
		IClientChannelSinkProvider _sinkProvider;
		
		public TcpClientChannel ()
		{
		}

		public TcpClientChannel (IDictionary properties, IClientChannelSinkProvider sinkProvider)
		{
			priority = 1;

			if (_sinkProvider != null)
			{
				_sinkProvider = sinkProvider;

				// add the tcp provider at the end of the chain
				IClientChannelSinkProvider prov = sinkProvider;
				while (prov.Next != null) prov = prov.Next;
				prov.Next = new TcpClientTransportSinkProvider ();

				// Note: a default formatter is added only when
				// no sink providers are specified in the config file.
			}
			else
			{
				_sinkProvider = new BinaryClientFormatterSinkProvider ();
				_sinkProvider.Next = new TcpClientTransportSinkProvider ();
			}

		}

		public TcpClientChannel (string name, IClientChannelSinkProvider sinkProvider)
		{
			priority = 1;		
			this.name = name;
			_sinkProvider = sinkProvider;

			// add the tcp provider at the end of the chain
			IClientChannelSinkProvider prov = sinkProvider;
			while (prov.Next != null) prov = prov.Next;
			prov.Next = new TcpClientTransportSinkProvider ();
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

		public IMessageSink CreateMessageSink (string url,
						       object remoteChannelData,
						       out string objectURI)
	    {
			if (url == null && remoteChannelData != null) {
				IChannelDataStore ds = remoteChannelData as IChannelDataStore;
				if (ds != null && ds.ChannelUris.Length > 0)
					url = ds.ChannelUris [0];
				else {
					objectURI = null;
					return null;
				}
			}

			if (Parse (url, out objectURI) == null)
				return null;

			return (IMessageSink) _sinkProvider.CreateSink (this, url, remoteChannelData);
		}

		public string Parse (string url, out string objectURI)
		{
			return TcpChannel.ParseChannelUrl (url, out objectURI);
		}
	}
}
