//
// HttpClientChannel.cs
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

using System;
using System.Net;
using System.Collections;
using System.Globalization;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Channels.Http
{
	
	public class HttpClientChannel : BaseChannelWithProperties,
		IChannel, IChannelSender
#if NET_2_0
		, ISecurableChannel
#endif
	{
		string name = "http client";
		int priority = 1;
		
		
		// names and (some) default values fromhttp://msdn.microsoft.com/en-us/library/bb187435(VS.85).aspx
		// other values guessed from defaults of HttpWebResponse
		string machineName = null;
		bool allowAutoRedirect = true; //FIXME: what's the default? true/false?
		int clientConnectionLimit = 2;
		string connectionGroupName = null;
		ICredentials credentials = null;
		string domain = null;
		string password = null;
		string proxyName = null;
		int proxyPort = -1;
		Uri proxyUri = null;
		string servicePrincipalName = null;
		int timeout = -1;
		bool unsafeAuthenticatedConnectionSharing = false;
		// according to docs, should be true if useDefaultCredentials true or 
		// credentials is CredentialCache.DefaultCredentials
		bool useAuthenticatedConnectionSharing = false; 
		bool useDefaultCredentials = false;
		string username = null;
		
		//FIXME: set to correct default
		bool isSecured = false;
		
		HttpClientTransportSinkProvider sinkProvider;
		
		#region Constructors
		
		public HttpClientChannel ()
		{
			BuildSink (null);
		}
		
		public HttpClientChannel (IDictionary properties, IClientChannelSinkProvider sinkProvider)
		{
			if (properties != null) {
				foreach(DictionaryEntry property in properties) {
					switch((string)property.Key) {
					case "name":
						//NOTE: matching MS behaviour: throws InvalidCastException, allows null
						this.name = (string) property.Value;
						break;
					case "priority":
						this.priority = Convert.ToInt32 (property.Value);
						break;
					case "machineName":
						this.machineName = (string) property.Value;
						break;
					case "allowAutoRedirect":
						this.allowAutoRedirect = Convert.ToBoolean (property.Value);
						break;
					case "clientConnectionLimit":
						this.clientConnectionLimit = Convert.ToInt32 (property.Value);
						break;
					case "connectionGroupName":
						this.connectionGroupName = (string) property.Value;
						break;
					case "credentials":
						this.credentials = (ICredentials) property.Value;
						if (this.credentials == CredentialCache.DefaultCredentials)
							useAuthenticatedConnectionSharing = true;
						break;
					case "domain":
						this.domain = (string) property.Value;
						break;
					case "password":
						this.password = (string) property.Value;
						break;
					case "proxyName":
						this.proxyName = (string) property.Value;
						break;
					case "proxyPort":
						this.proxyPort = Convert.ToInt32 (property.Value);
						break;
					case "servicePrincipalName":
						this.servicePrincipalName = (string) property.Value;
						break;
					case "timeout":
						this.timeout = Convert.ToInt32 (property.Value);
						break;
					case "unsafeAuthenticatedConnectionSharing":
						this.unsafeAuthenticatedConnectionSharing = Convert.ToBoolean (property.Value);
						break;
					case "useAuthenticatedConnectionSharing":
						this.useAuthenticatedConnectionSharing = Convert.ToBoolean (property.Value);
						break;	
					case "useDefaultCredentials":
						this.useDefaultCredentials = Convert.ToBoolean (property.Value);
						if (useDefaultCredentials)
							useAuthenticatedConnectionSharing = true;
						break;
					case "username":
						this.username = (string) property.Value;
						break;
					}
				}
			}
			
			BuildSink (sinkProvider);
		}
		
		public HttpClientChannel (string name, IClientChannelSinkProvider sinkProvider)
		{
			this.name = name;
			BuildSink (sinkProvider);
		}
		
		void BuildSink (IClientChannelSinkProvider sinkProvider)
		{
			if (sinkProvider == null) {
				//according to docs, defaults to SOAP if no other sink provided
				sinkProvider = new SoapClientFormatterSinkProvider ();
			}
			
			//add HTTP sink at the end of the chain
			while (sinkProvider.Next != null) sinkProvider = sinkProvider.Next;
			sinkProvider.Next = this.sinkProvider = new HttpClientTransportSinkProvider ();
			
			// LAMESPEC: BaseChannelWithProperties wants SinksWithProperties to be set with the sink chain
			// BUT MS' HttpClientChannel does not set it (inspected from HttpClientChannel subclass)
		}
		
		#endregion
		
		#region BaseChannelWithProperties overrides
		
		public override object this [object key] {
			get {
				switch (key as string) {
				case "proxyport":
					return proxyPort;
				case "proxyname":
					return proxyName;
				}
				return null;
			}
			set {
				switch (key as string) {
				case "proxyport":
					proxyPort = Convert.ToInt32 (value);
					ConstructProxy ();
					return;
				case "proxyname":
					proxyName = (string) value;
					ConstructProxy ();
					return;
				}
				//ignore other values, MS does so
			}
		}
		
		public override ICollection Keys {
			get {
				return new string[] {
					"proxyname",
					"proxyport"
				};
			}
		}
		
		void ConstructProxy ()
		{
			if (proxyName != null && proxyPort > 0)
				proxyUri = new Uri (proxyName + ":" + proxyPort);
		}
		
		#endregion
		
		#region IChannel
		
		public string ChannelName {
			get { return name; }
		}

		public int ChannelPriority {
			get { return priority; }
		}

		public string Parse (string url, out string objectURI)
		{
			return HttpChannel.ParseInternal (url, out objectURI);
		}
		
		#endregion
		
		#region IChannelSender (: IChannel)
		
		public IMessageSink CreateMessageSink (string url, object remoteChannelData, out string objectURI)
		{
			//Mostly copied from TcpClientChannel
			if (url != null && Parse (url, out objectURI) != null)
				return (IMessageSink) sinkProvider.CreateSink (this, url, remoteChannelData);
			
			if (remoteChannelData != null) {
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
			
			return (IMessageSink) sinkProvider.CreateSink (this, url, remoteChannelData);
		}
		
		#endregion
		
		#region ISecurableChannel
		
		public bool IsSecured {
			get { return isSecured; }
			set { isSecured = value; }
		}
		
		#endregion
		
	}
}
