//
// System.Runtime.Remoting.Channels.Simple.SimpleChannel.cs
//
// Author: Dietmar Maurer (dietmar@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;

namespace System.Runtime.Remoting.Channels.Simple
{
	public class SimpleChannel : IChannelReceiver, IChannel,
		IChannelSender
	{
		private int tcp_port;
		
		public SimpleChannel ()
	        {
			tcp_port = 0;
		}

		public SimpleChannel (int port)
		{
			tcp_port = port;
		}

		[MonoTODO]
		public SimpleChannel (IDictionary properties,
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

		public string Parse (string url, out string objectURI)
		{
			int port;
			
			string host = ParseSimpleURL (url, out objectURI, out port);

			return "tcp://" + host + ":" + port;
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

		internal static string ParseSimpleURL (string url, out string objectURI, out int port)
		{
			// format: "simple://host:port/path/to/object"
			
			objectURI = null;
			port = 0;
			
			Match m = Regex.Match (url, "simple://([^:]+):([0-9]+)(/.*)");

			if (!m.Success)
				return null;
			
			string host = m.Groups[1].Value;
			string port_str = m.Groups[2].Value;
			objectURI = m.Groups[3].Value;
			port = Convert.ToInt32 (port_str);
				
			return host;
		}
	}
}
