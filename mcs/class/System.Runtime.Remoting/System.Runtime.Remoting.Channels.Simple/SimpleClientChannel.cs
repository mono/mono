//
// System.Runtime.Remoting.Channels.Simple.SimpleClientChannel.cs
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

namespace System.Runtime.Remoting.Channels.Simple
{
	public class SimpleClientChannel : IChannelSender, IChannel
	{
		int priority = 1;					
		string name = "simple";
		IClientChannelSinkProvider sink_provider;
		
		public SimpleClientChannel ()
	        {
			sink_provider = new SimpleClientFormatterSinkProvider ();
			sink_provider.Next = new SimpleClientTransportSinkProvider ();
		}

		public SimpleClientChannel (IDictionary properties, IClientChannelSinkProvider sinkProvider)
		{
			priority = 1;
			sink_provider = sinkProvider;

			// add the tcp provider at the end of the chain
			IClientChannelSinkProvider prov = sinkProvider;
			while (prov.Next != null) prov = prov.Next;
			prov.Next = new SimpleClientTransportSinkProvider ();
		}

		public SimpleClientChannel (string name, IClientChannelSinkProvider sinkProvider)
		{
			priority = 1;		
			this.name = name;
			sink_provider = sinkProvider;

			// add the tcp provider at the end of the chain
			IClientChannelSinkProvider prov = sinkProvider;
			while (prov.Next != null) prov = prov.Next;
			prov.Next = new SimpleClientTransportSinkProvider ();
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
			objectURI = null;

			if (url != null) {
				if (Parse (url, out objectURI) != null)
					return (IMessageSink) sink_provider.CreateSink (this, url,
											remoteChannelData);
			}
			
			if (remoteChannelData != null) {
				IChannelDataStore ds = remoteChannelData as IChannelDataStore;
				if (ds != null) {
					foreach (string chnl_uri in ds.ChannelUris) {
						if (Parse (chnl_uri, out objectURI) == null)
							continue;
						return (IMessageSink) sink_provider.CreateSink (this, chnl_uri,
												remoteChannelData);
					}
				}
			}
			
			return null;			
		}

		public string Parse (string url, out string objectURI)
		{
			int port;
			
			string host = SimpleChannel.ParseSimpleURL (url, out objectURI, out port);

			return "simple://" + host + ":" + port;
		}
	}
}
