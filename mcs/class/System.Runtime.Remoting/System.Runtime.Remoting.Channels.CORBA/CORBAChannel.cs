//
// System.Runtime.Remoting.Channels.CORBA.CORBAChannel.cs
//
// Author: Dietmar Maurer (dietmar@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;

namespace System.Runtime.Remoting.Channels.CORBA
{
	public class CORBAChannel : IChannelReceiver, IChannel,
		IChannelSender
	{
		private int tcp_port;
		
		public CORBAChannel ()
	        {
			tcp_port = 0;
		}

		public CORBAChannel (int port)
		{
			tcp_port = port;
		}

		[MonoTODO]
		public CORBAChannel (IDictionary properties,
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
			
			string host = ParseCORBAURL (url, out objectURI, out port);

			return "corba://" + host + ":" + port;
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

		internal static string ParseCORBAURL (string url, out string objectURI, out int port)
		{
			// format: "corba://host:port/path/to/object"
			
			objectURI = null;
			port = 0;
			
			Match m = Regex.Match (url, "corba://([^:]+):([0-9]+)(/.*)");

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
