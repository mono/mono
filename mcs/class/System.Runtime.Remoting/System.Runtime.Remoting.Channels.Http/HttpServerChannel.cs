//
// HttpServerChannel.cs
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

using System.Net;
using System.Collections;
using System.Globalization;
using System.Threading;
using System.Runtime.Remoting.MetadataServices;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Channels.Http
{

	public class HttpServerChannel : BaseChannelWithProperties,
		IChannel, IChannelReceiver, IChannelReceiverHook
	{
		string name = "http server";
		int priority = 1;
		
		string machineName = null;
		IPAddress bindAddress = IPAddress.Any;
		int port = -1; // querying GetChannelUri () on .NET indicates this is the default value
		bool suppressChannelData = false;
		bool useIPAddress = true;
#if !NET_2_0
		bool exclusiveAddressUse = true;
#endif
		bool wantsToListen = true;

		HttpServerTransportSink sink;
		ChannelDataStore channelData;
		RemotingHttpListener listener;

		#region Constructors

		public HttpServerChannel ()
		{
			//DONT START SERVER, EVEN THOUGH ALL OTHER CONSTRUCTORS DO
			BuildSink (null);
		}

		public HttpServerChannel (int port)
		{
			this.port = port;
			BuildSink (null);
		}
		
		[MonoTODO ("Handle the listen property")]
		public HttpServerChannel (IDictionary properties, IServerChannelSinkProvider sinkProvider)
		{

			if (properties != null) {
				foreach (DictionaryEntry property in properties) {
					switch ((string)property.Key) {
					case "name":
						//NOTE: matching MS behaviour: throws InvalidCastException, allows null
						this.name = (string)property.Value;
						break;
					case "priority":
						this.priority = Convert.ToInt32 (property.Value);
						break;
					case "port":
						this.port = Convert.ToInt32 (property.Value);
						break;
					case "suppressChannelData":
						this.suppressChannelData = Convert.ToBoolean (property.Value);
						break;
					case "bindTo":
						bindAddress = IPAddress.Parse ((string)property.Value);
						break;
					case "useIpAddress":
						this.useIPAddress = Convert.ToBoolean (property.Value);
						break;
					case "machineName":
						this.machineName = (string)property.Value;
						break;
					case "listen":
						this.wantsToListen = Convert.ToBoolean (property.Value);
						break;
#if !NET_2_0
					case "exclusiveAddressUse":
						this.exclusiveAddressUse = Convert.ToBoolean (property.Value);
						break;
#endif
					}
				}
			}

			BuildSink (sinkProvider);
		}

		public HttpServerChannel (string name, int port)
			: this (name, port, null)
		{
		}

		public HttpServerChannel (string name, int port, IServerChannelSinkProvider sinkProvider)
		{
			this.name = name;
			this.port = port;
			BuildSink (sinkProvider);
		}

		void BuildSink (IServerChannelSinkProvider sinkProvider)
		{
			//resolve names (modified from TcpChannel)
			if (machineName == null) {
				if (useIPAddress) {
					if (!bindAddress.Equals (IPAddress.Any)) {
						machineName = bindAddress.ToString ();
					} else {
						IPHostEntry hostEntry = Dns.Resolve (Dns.GetHostName ());
						if (hostEntry.AddressList.Length == 0)
							throw new RemotingException ("IP address could not be determined for this host");
						// We DON'T want to take the resolved address from the hostEntry, since the socket
						// should still bind to IPAddress.Any, so that we get the loopback too
						machineName = hostEntry.AddressList[0].ToString ();
					}
				} else {
					IPHostEntry hostEntry = Dns.GetHostByName (Dns.GetHostName ());
					bindAddress = hostEntry.AddressList[0];
					machineName = hostEntry.HostName;
				}
			}

			if (sinkProvider == null) {
				//build a default chain that can handle wsdl, soap, binary
				sinkProvider = new SdlChannelSinkProvider (); //for wsdl
				sinkProvider.Next = new SoapServerFormatterSinkProvider ();
				sinkProvider.Next.Next = new BinaryServerFormatterSinkProvider ();
			}
			
			//MS compat: channelData is null when port < 0
			if (port >= 0) {
				channelData = new ChannelDataStore (null);
				IServerChannelSinkProvider provider = sinkProvider;
				while (provider != null) {
					provider.GetChannelData (channelData);
					provider = provider.Next;
				}
			}
			
			//create the sink chain and add an HTTP sink
			IServerChannelSink nextSink = ChannelServices.CreateServerChannelSinkChain (sinkProvider, this);
			sink = new HttpServerTransportSink (nextSink);

			// BaseChannelWithProperties wants this to be set with the chain
			base.SinksWithProperties = nextSink;

			StartListening (null);
		}

		#endregion

		#region IChannel

		public string ChannelName
		{
			get { return name; }
		}

		public int ChannelPriority
		{
			get { return priority; }
		}

		public string Parse (string url, out string objectURI)
		{
			return HttpChannel.ParseInternal (url, out objectURI);
		}

		#endregion

		public string GetChannelUri ()
		{
			return "http://" + machineName + ":" + port;
		}

		#region IChannelReceiver (: IChannel)

		public object ChannelData
		{
			get
			{
				return suppressChannelData ? null
					: channelData;
			}
		}

		//from TcpServerChannel
		public virtual string[] GetUrlsForUri (string objectUri)
		{
			if (!objectUri.StartsWith ("/"))
				objectUri = "/" + objectUri;

			if (channelData == null || channelData.ChannelUris == null || channelData.ChannelUris.Length < 1) {
				return new string[] { GetChannelUri () + objectUri };
			}

			string[] channelUris = channelData.ChannelUris;
			string[] result = new string[channelUris.Length];

			for (int i = 0; i < channelUris.Length; i++)
				result[i] = channelUris[i] + objectUri;

			return result;
		}

		public void StartListening (object data)
		{
			if (listener != null)
				return;

			if (port < 0)
				return;

			try {
				listener = new RemotingHttpListener (bindAddress, port, sink);
			} catch (Exception) {
				if (listener != null) {
					listener.Dispose ();
					listener = null;
				}
				throw;
			}
			
			if (port == 0)
				port = listener.AssignedPort;
			
			channelData.ChannelUris = new string [] { GetChannelUri () };
			wantsToListen = false;
		}

		public void StopListening (object data)
		{
			if (listener != null) {
				listener.Dispose ();
				listener = null;
			}
		}

		#endregion

		#region BaseChannelWithProperties overrides

		public override object this[object key]
		{
			get { return base[key]; }
			set { base[key] = value; }
		}

		public override ICollection Keys
		{
			get { return new object[0]; }
		}

		#endregion

		#region IChannelReceiverHook

		public void AddHookChannelUri (string channelUri)
		{
			string [] newUris = new string[1] { channelUri };
			if (channelData == null)
				channelData = new ChannelDataStore (newUris);
			else
				channelData.ChannelUris = newUris;
			wantsToListen = false;
		}

		public string ChannelScheme
		{
			get { return "http"; }
		}

		public IServerChannelSink ChannelSinkChain
		{
			get { return (IServerChannelSink)base.SinksWithProperties; }
		}

		public bool WantsToListen
		{
			get { return wantsToListen; }
			set {
				throw new NotImplementedException ("Behaviour not yet determined");
			}
		}

		#endregion
	}
}
