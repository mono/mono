//
// System.Runtime.Remoting.Channels.Tcp.TcpChannel.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Channels.Tcp
{
	public class TcpChannel : IChannelReceiver, IChannel,
		IChannelSender
	{
		private int tcp_port;
		
		public TcpChannel ()
	        {
			tcp_port = 0;
		}

		public TcpChannel (int port)
		{
			tcp_port = port;
		}

		[MonoTODO]
		public TcpChannel (IDictionary properties,
				   IClientChannelSinkProvider clientSinkProvider,
				   IServerChannelSinkProvider serverSinkProvider)
		{
			throw new NotImplementedException ();
		}

		public object ChannelData
		{
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public string ChannelName
		{
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public int ChannelPriority
		{
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public IMessageSink CreateMessageSink (string url,
						       object remoteChannelData,
						       out string objectURI)
	        {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string[] GetUrlsForUri (string objectURI)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string Parse (string url, out string objectURI)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void StartListening (object data)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void StopListening (object data)
		{
			throw new NotImplementedException ();
		}
	}
}
