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
		SimpleServerChannel svr_chnl;
		SimpleClientChannel cnt_chnl;
		
		string name = "simple";

		public SimpleChannel ()
	        {
			svr_chnl = new SimpleServerChannel (0);
			cnt_chnl = new SimpleClientChannel ();
		}

		public SimpleChannel (int port)
		{
			svr_chnl = new SimpleServerChannel (port);
			cnt_chnl = new SimpleClientChannel ();
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
			get {
				return svr_chnl.ChannelData;
			}
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
				return svr_chnl.ChannelPriority;
			}
		}

		public IMessageSink CreateMessageSink (string url,
						       object remoteChannelData,
						       out string objectURI)
	        {
			return cnt_chnl.CreateMessageSink (url, remoteChannelData, out objectURI);
		}

		public string[] GetUrlsForUri (string objectURI)
		{
			return svr_chnl.GetUrlsForUri (objectURI);
		}

		public string Parse (string url, out string objectURI)
		{
			return svr_chnl.Parse (url, out objectURI);
		}

		public void StartListening (object data)
		{
			svr_chnl.StartListening (data);
		}

		public void StopListening (object data)
		{
			svr_chnl.StopListening (data);
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
