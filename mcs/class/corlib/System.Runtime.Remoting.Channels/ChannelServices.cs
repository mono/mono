//
// System.Runtime.Remoting.Channels.ChannelServices.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//         Dietmar Maurer (dietmar@ximian.com)
//         Lluis Sanchez Gual (lluis@ideary.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Contexts;

namespace System.Runtime.Remoting
{
	[Serializable]
	internal class ChannelInfo : IChannelInfo
	{
		object [] channelData = null;

		public ChannelInfo ()
		{
			channelData = ChannelServices.GetCurrentChannelInfo ();
		}

		public ChannelInfo (object remoteChannelData)
		{
			channelData = new object[] { remoteChannelData };
		}
		
		public object[] ChannelData 
		{
			get {
				return channelData;
			}
			
			set {
				channelData = value;
			}
		}
	}
}

namespace System.Runtime.Remoting.Channels
{
	[System.Runtime.InteropServices.ComVisible (true)]
	public sealed class ChannelServices
	{
		private static ArrayList registeredChannels = new ArrayList ();
		private static ArrayList delayedClientChannels = new ArrayList ();
		
		private static CrossContextChannel _crossContextSink = new CrossContextChannel();
		
		internal static string CrossContextUrl = "__CrossContext";

		private ChannelServices ()
		{
		}

		internal static CrossContextChannel CrossContextChannel
		{
			get { return _crossContextSink; }
		}

		internal static IMessageSink CreateClientChannelSinkChain(string url, object remoteChannelData, out string objectUri)
		{
			// Locate a channel that can parse the url. This channel will be used to
			// create the sink chain.

			object[] channelDataArray = (object[])remoteChannelData;

			lock (registeredChannels.SyncRoot)
			{
				// First of all, try registered channels
				foreach (IChannel c in registeredChannels) 
				{
					IChannelSender sender = c as IChannelSender;
					if (sender == null) continue;
	
					IMessageSink sink = CreateClientChannelSinkChain (sender, url, channelDataArray, out objectUri);
					if (sink != null) return sink;
				}
				
				// Not found. Try now creation delayed channels
				RemotingConfiguration.LoadDefaultDelayedChannels ();
				foreach (IChannelSender sender in delayedClientChannels) 
				{
					IMessageSink sink = CreateClientChannelSinkChain (sender, url, channelDataArray, out objectUri);
					if (sink != null) {
						delayedClientChannels.Remove (sender);
						RegisterChannel (sender);
						return sink;
					}
				}
			}
			
			objectUri = null;
			return null;
		}
		
		internal static IMessageSink CreateClientChannelSinkChain (IChannelSender sender, string url, object[] channelDataArray, out string objectUri)
		{
			objectUri = null;
			if (channelDataArray == null) {
				return sender.CreateMessageSink (url, null, out objectUri);
			}
			else {
				foreach (object data in channelDataArray) {
					IMessageSink sink;

					if (data is IChannelDataStore) {
						// Don't provide the url in this case, since some channels won't
						// check the channelData parameter if the url is not null.
						sink = sender.CreateMessageSink (null, data, out objectUri);
					} else {
						sink = sender.CreateMessageSink (url, data, out objectUri);
					}
					if (sink != null) return sink;		
				}
			}
			return null;
		}
		
		public static IChannel[] RegisteredChannels
		{
			get {
				lock (registeredChannels.SyncRoot)
				{
					var list = new List<IChannel> ();
					
					for (int i = 0; i < registeredChannels.Count; i++) {
						IChannel ch = (IChannel) registeredChannels[i];
						if (ch is CrossAppDomainChannel) continue;
						list.Add (ch);
					}

					return list.ToArray ();
				}
			}
		}

		public static IServerChannelSink CreateServerChannelSinkChain (
			IServerChannelSinkProvider provider, IChannelReceiver channel)
	    {
			IServerChannelSinkProvider tmp = provider;
			while (tmp.Next != null) tmp = tmp.Next;
			tmp.Next = new ServerDispatchSinkProvider ();

			// Every provider has to call CreateSink() of its next provider
			return  provider.CreateSink (channel);
		}

		public static ServerProcessing DispatchMessage (
			IServerChannelSinkStack sinkStack,
			IMessage msg,
			out IMessage replyMsg)
		{
			if (msg == null) throw new ArgumentNullException ("msg");
			
			// Async processing is not done here because there isn't any way
			// to know if a message is to be dispatched sync or asynchronously.

			replyMsg = SyncDispatchMessage (msg);

			if (RemotingServices.IsOneWay (((IMethodMessage) msg).MethodBase))
				return ServerProcessing.OneWay;
			else
				return ServerProcessing.Complete;
		}

		public static IChannel GetChannel (string name)
		{
			lock (registeredChannels.SyncRoot)
			{
				foreach (IChannel chnl in registeredChannels) {
					if (chnl.ChannelName == name && !(chnl is CrossAppDomainChannel)) return chnl;
				}
				return null;
			}
		}

		public static IDictionary GetChannelSinkProperties (object obj)
		{
			if (!RemotingServices.IsTransparentProxy (obj))
				throw new ArgumentException ("obj must be a proxy","obj");
				
			ClientIdentity ident = (ClientIdentity) RemotingServices.GetRealProxy (obj).ObjectIdentity;
			IMessageSink sink = ident.ChannelSink;
			var dics = new List<IDictionary> ();
			
			while (sink != null && !(sink is IClientChannelSink))
				sink = sink.NextSink;

			if (sink == null)
				return new Hashtable ();

			IClientChannelSink csink = sink as IClientChannelSink;
			while (csink != null)
			{
				dics.Add (csink.Properties);
				csink = csink.NextChannelSink;
			}

			IDictionary[] adics = dics.ToArray ();
			return new AggregateDictionary (adics);
		}

		public static string[] GetUrlsForObject (MarshalByRefObject obj)
		{
			string uri = RemotingServices.GetObjectUri (obj);
			if (uri == null) return new string [0];

			var list = new List<string> ();

			lock (registeredChannels.SyncRoot)
			{
				foreach (object chnl_obj in registeredChannels) {
					if (chnl_obj is CrossAppDomainChannel) continue;
					
					IChannelReceiver chnl = chnl_obj as IChannelReceiver;
	
					if (chnl != null)
						list.AddRange (chnl.GetUrlsForUri (uri));
				}
			}
			
			return list.ToArray ();
		}

		[Obsolete ("Use RegisterChannel(IChannel,Boolean)")]
		public static void RegisterChannel (IChannel chnl)
		{
			RegisterChannel (chnl, false);
		}

		public static void RegisterChannel (IChannel chnl, bool ensureSecurity)
		{
			if (chnl == null)
				throw new ArgumentNullException ("chnl");

			if (ensureSecurity) {
				ISecurableChannel securable = chnl as ISecurableChannel;
				if (securable == null)
					throw new RemotingException (String.Format ("Channel {0} is not securable while ensureSecurity is specified as true", chnl.ChannelName));
				securable.IsSecured = true;
			}
			
			// Put the channel in the correct place according to its priority.
			// Since there are not many channels, a linear search is ok.

			lock (registeredChannels.SyncRoot)
			{
				int pos = -1;
				for (int n = 0; n < registeredChannels.Count; n++) 
				{
					IChannel regc = (IChannel) registeredChannels[n];
					
					if (regc.ChannelName == chnl.ChannelName && chnl.ChannelName != "")
						throw new RemotingException ("Channel " + regc.ChannelName + " already registered");
						
					if (regc.ChannelPriority < chnl.ChannelPriority && pos==-1)
						pos = n;
				}
				
				if (pos != -1) registeredChannels.Insert (pos, chnl);
				else registeredChannels.Add (chnl);

				IChannelReceiver receiver = chnl as IChannelReceiver;
				if (receiver != null && oldStartModeTypes.Contains (chnl.GetType().ToString ()))
					receiver.StartListening (null);
			}
		}

		internal static void RegisterChannelConfig (ChannelData channel)
		{
			IServerChannelSinkProvider serverSinks = null;
			IClientChannelSinkProvider clientSinks = null;
			
			// Create server providers
			for (int n=channel.ServerProviders.Count-1; n>=0; n--)
			{
				ProviderData prov = channel.ServerProviders[n] as ProviderData;
				IServerChannelSinkProvider sinkp = (IServerChannelSinkProvider) CreateProvider (prov);
				sinkp.Next = serverSinks;
				serverSinks = sinkp;
			}
			
			// Create client providers
			for (int n=channel.ClientProviders.Count-1; n>=0; n--)
			{
				ProviderData prov = channel.ClientProviders[n] as ProviderData;
				IClientChannelSinkProvider sinkp = (IClientChannelSinkProvider) CreateProvider (prov);
				sinkp.Next = clientSinks;
				clientSinks = sinkp;
			}

			// Create the channel
			
			Type type = Type.GetType (channel.Type);
			if (type == null) throw new RemotingException ("Type '" + channel.Type + "' not found");
			
			Object[] parms;			
			Type[] signature;			
			bool clienc = typeof (IChannelSender).IsAssignableFrom (type);
			bool serverc = typeof (IChannelReceiver).IsAssignableFrom (type);
			
			if (clienc && serverc) {
				signature = new Type [] {typeof(IDictionary), typeof(IClientChannelSinkProvider), typeof(IServerChannelSinkProvider)};
				parms = new Object[] {channel.CustomProperties, clientSinks, serverSinks};
			}
			else if (clienc) {
				signature = new Type [] {typeof(IDictionary), typeof(IClientChannelSinkProvider)};
				parms = new Object[] {channel.CustomProperties, clientSinks};
			}
			else if (serverc) {
				signature = new Type [] {typeof(IDictionary), typeof(IServerChannelSinkProvider)};
				parms = new Object[] {channel.CustomProperties, serverSinks};
			}
			else
				throw new RemotingException (type + " is not a valid channel type");
				
			ConstructorInfo ctor = type.GetConstructor (signature);
			if (ctor == null)
				throw new RemotingException (type + " does not have a valid constructor");

			IChannel ch;
			try
			{
				ch = (IChannel) ctor.Invoke (parms);
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
			
			lock (registeredChannels.SyncRoot)
			{
				if (channel.DelayLoadAsClientChannel == "true" && !(ch is IChannelReceiver))
					delayedClientChannels.Add (ch);
				else
					RegisterChannel (ch);
			}
		}
		
		static object CreateProvider (ProviderData prov)
		{
			Type pvtype = Type.GetType (prov.Type);
			if (pvtype == null) throw new RemotingException ("Type '" + prov.Type + "' not found");
			Object[] pvparms = new Object[] {prov.CustomProperties, prov.CustomData};
			
			try
			{
				return Activator.CreateInstance (pvtype, pvparms);
			}
			catch (Exception ex)
			{
				if (ex is TargetInvocationException) ex = ((TargetInvocationException)ex).InnerException;
				throw new RemotingException ("An instance of provider '" + pvtype + "' could not be created: " + ex.Message);
			}
		}

		public static IMessage SyncDispatchMessage (IMessage msg)
		{
			IMessage ret = CheckIncomingMessage (msg);
			if (ret != null) return CheckReturnMessage (msg, ret);
			ret = _crossContextSink.SyncProcessMessage (msg);
			return CheckReturnMessage (msg, ret);
		}

		public static IMessageCtrl AsyncDispatchMessage (IMessage msg, IMessageSink replySink)
		{
			IMessage ret = CheckIncomingMessage (msg);
			if (ret != null) {
				replySink.SyncProcessMessage (CheckReturnMessage (msg, ret));
				return null;
			}
			
			if (RemotingConfiguration.CustomErrorsEnabled (IsLocalCall (msg)))
				replySink = new ExceptionFilterSink (msg, replySink);
			
			return _crossContextSink.AsyncProcessMessage (msg, replySink);		
		}
		
		static ReturnMessage CheckIncomingMessage (IMessage msg)
		{
			IMethodMessage call = (IMethodMessage)msg;
			ServerIdentity identity = RemotingServices.GetIdentityForUri (call.Uri) as ServerIdentity;

			if (identity == null) 
				return new ReturnMessage (new RemotingException ("No receiver for uri " + call.Uri), (IMethodCallMessage) msg);

			RemotingServices.SetMessageTargetIdentity (msg, identity);
			return null;
		}

		internal static IMessage CheckReturnMessage (IMessage callMsg, IMessage retMsg)
		{
			IMethodReturnMessage ret = retMsg as IMethodReturnMessage;
			if (ret != null && ret.Exception != null)
			{
				if (RemotingConfiguration.CustomErrorsEnabled (IsLocalCall (callMsg)))
				{
					Exception ex = new Exception ("Server encountered an internal error. For more information, turn off customErrors in the server's .config file.");
					retMsg = new MethodResponse (ex, (IMethodCallMessage)callMsg);
				}
			}
			return retMsg;
		}
		
		static bool IsLocalCall (IMessage callMsg)
		{
			return true;
			
/*			How can I know if a call is local?!?
			
			object isLocal = callMsg.Properties ["__isLocalCall"];
			if (isLocal == null) return false;
			return (bool)isLocal;
*/
		}

		public static void UnregisterChannel (IChannel chnl)
		{
			if (chnl == null)
				throw new ArgumentNullException ();
				
			lock (registeredChannels.SyncRoot)
			{
				for (int n=0; n<registeredChannels.Count; n++) 
				{
					if (registeredChannels [n] == (object)chnl) {
						registeredChannels.RemoveAt (n);
						IChannelReceiver chnlReceiver = chnl as IChannelReceiver;
						if(chnlReceiver != null)
							chnlReceiver.StopListening(null);
						return;
					}
				}
				
				throw new RemotingException ("Channel not registered");
	
			}
		}

		internal static object [] GetCurrentChannelInfo ()
		{
			var list = new List<object> ();
			
			lock (registeredChannels.SyncRoot)
			{
				foreach (object chnl_obj in registeredChannels) {
					IChannelReceiver chnl = chnl_obj as IChannelReceiver;
				
					if (chnl != null) {
						object chnl_data = chnl.ChannelData;
						if (chnl_data != null)
							list.Add (chnl_data);
					}
				}
			}
			
			return list.ToArray ();
		}

		// Back compatibility fix. StartListener will be called for the types listed here		
		static IList oldStartModeTypes = new string[] {
			"Novell.Zenworks.Zmd.Public.UnixServerChannel",
			"Novell.Zenworks.Zmd.Public.UnixChannel"
		};
	}
	
	internal class ExceptionFilterSink: IMessageSink
	{
		IMessageSink _next;
		IMessage _call;
		
		public ExceptionFilterSink (IMessage call, IMessageSink next)
		{
			_call = call;
			_next = next;
		}
		
		public IMessage SyncProcessMessage (IMessage msg)
		{
			return _next.SyncProcessMessage (ChannelServices.CheckReturnMessage (_call, msg));
		}

		public IMessageCtrl AsyncProcessMessage (IMessage msg, IMessageSink replySink)
		{
			throw new InvalidOperationException();
		}

		public IMessageSink NextSink 
		{ 
			get { return _next; }
		}
	}
}
