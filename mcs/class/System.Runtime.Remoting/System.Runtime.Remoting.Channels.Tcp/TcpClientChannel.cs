//
// System.Runtime.Remoting.Channels.Tcp.TcpClientChannel.cs
//
// Author: Dietmar Maurer (dietmar@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Channels;
using System.Text.RegularExpressions;

namespace System.Runtime.Remoting.Channels.Tcp
{

	public class TcpClientTransportSink : IClientChannelSink
	{

		public TcpClientTransportSink (string url)
		{

		}

		public IDictionary Properties
		{
			get {
				return null;
			}
		}

		public IClientChannelSink NextChannelSink
		{
			get {
				// we are the last one
				return null;
			}
		}

		public void AsyncProcessRequest (IClientChannelSinkStack sinkStack, IMessage msg,
						 ITransportHeaders headers, Stream stream)
		{
			throw new NotImplementedException ();			
		}

		public void AsyncProcessResponse (IClientResponseChannelSinkStack sinkStack,
						  object state, ITransportHeaders headers,
						  Stream stream)
		{
			throw new NotImplementedException ();
		}

		public Stream GetRequestStream (IMessage msg, ITransportHeaders headers)
		{
			// no acces to stream?
			return null;
		}
		
		public void ProcessMessage (IMessage msg,
					    ITransportHeaders requestHeaders,
					    Stream requestStream,
					    out ITransportHeaders responseHeaders,
					    out Stream responseStream)
		{
			throw new NotImplementedException ();
		}
			
	}
	
	public class TcpClientTransportSinkProvider : IClientChannelSinkProvider
	{
		public TcpClientTransportSinkProvider ()
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
			return new TcpClientTransportSink (url);
		}
	}

	public class TcpClientChannel : IChannelSender, IChannel
	{
		int priority = 1;					
		string name = "tcp";
		IClientChannelSinkProvider sink_provider;
		
		public TcpClientChannel ()
	        {
			sink_provider = new BinaryClientFormatterSinkProvider ();
			sink_provider.Next = new TcpClientTransportSinkProvider ();
		}

		public TcpClientChannel (IDictionary properties, IClientChannelSinkProvider sinkProvider)
		{
			priority = 1;
			sink_provider = sinkProvider;

			// add the tcp provider at the end of the chain
			IClientChannelSinkProvider prov = sinkProvider;
			while (prov.Next != null) prov = prov.Next;
			prov.Next = new TcpClientTransportSinkProvider ();
		}

		public TcpClientChannel (string name, IClientChannelSinkProvider sinkProvider)
		{
			priority = 1;		
			this.name = name;
			sink_provider = sinkProvider;

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
				if (ds != null)
					url = ds.ChannelUris [0];
			}
			
			if (Parse (url, out objectURI) == null)
				return null;

			return (IMessageSink) sink_provider.CreateSink (this, url, remoteChannelData);
		}

		public string Parse (string url, out string objectURI)
		{
			// format: "tcp://host:port/path/to/object"
			
			objectURI = null;
			
			Match m = Regex.Match (url, "tcp://([^:]+):([0-9]+)(/.*)");

			if (!m.Success)
				return null;
			
			string host = m.Groups[1].Value;
			string port = m.Groups[2].Value;
			objectURI = m.Groups[3].Value;
			
			return "tcp://" + host + ":" + port;
		}

	}
}
