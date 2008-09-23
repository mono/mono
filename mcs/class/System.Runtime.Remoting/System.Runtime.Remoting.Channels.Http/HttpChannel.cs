//
// HttpChannel.cs
// 
// Author:
//   Michael Hutchinson <mhutchinson@novell.com>
// 
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Channels.Http
{

	public class HttpChannel : BaseChannelWithProperties,
		IChannel, IChannelReceiver, IChannelReceiverHook, IChannelSender
#if NET_2_0
		, ISecurableChannel
#endif
	{
		HttpClientChannel client;
		HttpServerChannel server;
		string name = "http";

		#region Constructors

		public HttpChannel ()
		{
			client = new HttpClientChannel ();
			server = new HttpServerChannel ();
		}

		public HttpChannel (int port)
		{
			client = new HttpClientChannel ();
			server = new HttpServerChannel (port);
		}

		public HttpChannel (IDictionary properties,
			IClientChannelSinkProvider clientSinkProvider,
			IServerChannelSinkProvider serverSinkProvider)
		{
			if (properties != null && properties.Contains ("name")) {
				this.name = (string)properties["name"];
			}

			client = new HttpClientChannel (properties, clientSinkProvider);
			server = new HttpServerChannel (properties, serverSinkProvider);
		}

		#endregion

		#region BaseChannelWithProperties overrides

		public override object this[object key]
		{
			get { return Properties[key]; }
			set { Properties[key] = value; }
		}

		public override ICollection Keys
		{
			get { return Properties.Keys; }
		}

		public override IDictionary Properties
		{
			get
			{
				return new AggregateDictionary (new IDictionary[] {
					client.Properties,
					server.Properties
				});
			}
		}

		#endregion

		#region IChannel

		public string ChannelName
		{
			get { return name; }
		}

		public int ChannelPriority
		{
			get { return server.ChannelPriority; }
		}

		public string Parse (string url, out string objectURI)
		{
			return ParseInternal (url, out objectURI);
		}

		internal static string ParseInternal (string url, out string objectURI)
		{
			if (url == null)
				throw new ArgumentNullException ("url");
			
			// format: "http://host:port/path/to/object"
			objectURI = null;
			
			// url needs to be at least "http:" or "https:"
			if (url.Length < 5 ||
			    (url[0] != 'H' && url[0] != 'h') ||
			    (url[1] != 'T' && url[1] != 't') ||
			    (url[2] != 'T' && url[2] != 't') ||
			    (url[3] != 'P' && url[3] != 'p'))
				return null;
			
			int protolen;
			if (url[4] == 'S' || url[4] == 's') {
				if (url.Length < 6)
					return null;
				
				protolen = 5;
			} else {
				protolen = 4;
			}
			
			if (url[protolen] != ':')
				return null;
			
			// "http:" and "https:" are acceptable inputs
			if (url.Length == protolen + 1)
				return url;
			
			// protocol must be followed by "//"
			if (url.Length < protolen + 3 || url[protolen + 1] != '/' || url[protolen + 2] != '/')
				return null;
			
			// "http://" and "https://" are acceptable inputs
			if (url.Length == protolen + 3)
				return url;
			
			int slash = url.IndexOf ('/', protolen + 3);
			if (slash == -1)
				return url;
				
			objectURI = url.Substring (slash);

			return url.Substring (0, slash);
		}

		#endregion

		#region IChannelReceiver (: IChannel)

		public object ChannelData
		{
			get { return server.ChannelData; }
		}

		public string[] GetUrlsForUri (string objectURI)
		{
			return server.GetUrlsForUri (objectURI);
		}

		public void StartListening (object data)
		{
			server.StartListening (data);
		}

		public void StopListening (object data)
		{
			server.StopListening (data);
		}

		#endregion

		#region IChannelReceiverHook

		public void AddHookChannelUri (string channelUri)
		{
			server.AddHookChannelUri (channelUri);
		}

		public string ChannelScheme
		{
			get { return server.ChannelScheme; }
		}

		public IServerChannelSink ChannelSinkChain
		{
			get { return server.ChannelSinkChain; }
		}

		public bool WantsToListen
		{
			get { return server.WantsToListen; }
			set { server.WantsToListen = value; }
		}

		#endregion

		#region IChannelSender (: IChannel)

		public IMessageSink CreateMessageSink (string url, object remoteChannelData, out string objectURI)
		{
			return client.CreateMessageSink (url, remoteChannelData, out objectURI);
		}

		#endregion

#if NET_2_0
		#region ISecurableChannel

		public bool IsSecured
		{
			get { return client.IsSecured; }
			set { client.IsSecured = value; }
		}

		#endregion
#endif
	}
}
