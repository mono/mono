//
// generic ClientBase.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005-2006 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;
using System.ComponentModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Threading;

namespace System.ServiceModel
{
	[MonoTODO ("It somehow rejects classes, but dunno how we can do that besides our code level.")]
	public abstract class ClientBase<TChannel>
		: IDisposable, ICommunicationObject
	{
		static InstanceContext initialContxt = new InstanceContext (null);

		ChannelFactory<TChannel> factory;
		ClientRuntimeChannel inner_channel;
		CommunicationState state;

		protected delegate IAsyncResult BeginOperationDelegate (object[] inValues, AsyncCallback asyncCallback, object state);
		protected delegate object[] EndOperationDelegate (IAsyncResult result);

		protected ClientBase ()
			: this (initialContxt)
		{
		}

		protected ClientBase (string configname)
			: this (initialContxt, configname)
		{
		}

		protected ClientBase (Binding binding, EndpointAddress remoteAddress)
			: this (initialContxt, binding, remoteAddress)
		{
		}

		protected ClientBase (string configname, EndpointAddress remoteAddress)
			: this (initialContxt, configname, remoteAddress)
		{
		}

		protected ClientBase (string configname, string remoteAddress)
			: this (initialContxt, configname, remoteAddress)
		{
		}

		protected ClientBase (InstanceContext instance)
			: this (instance, "*")
		{
		}

		protected ClientBase (InstanceContext instance, string configname)
		{
			if (instance == null)
				throw new ArgumentNullException ("instanceContext");
			if (configname == null)
				throw new ArgumentNullException ("configurationName");

			Initialize (instance, configname, null);
		}

		protected ClientBase (InstanceContext instance,
			string configname, EndpointAddress remoteAddress)
		{
			if (instance == null)
				throw new ArgumentNullException ("instanceContext");
			if (configname == null)
				throw new ArgumentNullException ("configurationName");
			if (remoteAddress == null)
				throw new ArgumentNullException ("remoteAddress");

			Initialize (instance, configname, remoteAddress);
		}

		protected ClientBase (InstanceContext instance,
			string configname, string remoteAddress)
		{
			if (instance == null)
				throw new ArgumentNullException ("instanceContext");
			if (remoteAddress == null)
				throw new ArgumentNullException ("endpointAddress");
			if (configname == null)
				throw new ArgumentNullException ("configurationname");

			Initialize (instance, configname, new EndpointAddress (remoteAddress));
		}

		protected ClientBase (InstanceContext instance,
			Binding binding, EndpointAddress remoteAddress)
		{
			if (instance == null)
				throw new ArgumentNullException ("instanceContext");
			if (binding == null)
				throw new ArgumentNullException ("binding");
			if (remoteAddress == null)
				throw new ArgumentNullException ("remoteAddress");

			Initialize (instance, binding, remoteAddress);
		}

		void Initialize (InstanceContext instance,
			string configName, EndpointAddress remoteAddress)
		{
			factory = new ChannelFactory<TChannel> (configName, remoteAddress);
		}

		void Initialize (InstanceContext instance,
			Binding binding, EndpointAddress remoteAddress)
		{
			factory = new ChannelFactory<TChannel> (binding, remoteAddress);
		}

		public ChannelFactory<TChannel> ChannelFactory {
			get { return factory; }
		}

#if !NET_2_1
		public ClientCredentials ClientCredentials {
			get { return ChannelFactory.Credentials; }
		}
#endif

		public ServiceEndpoint Endpoint {
			get { return factory.Endpoint; }
		}

		public IClientChannel InnerChannel {
			get {
				if (inner_channel == null)
					inner_channel = (ClientRuntimeChannel) (object) factory.CreateChannel ();
				return inner_channel;
			}
		}

		protected TChannel Channel {
			get { return (TChannel) (object) InnerChannel; }
		}

		public CommunicationState State {
			get { return InnerChannel.State; }
		}

		public void Abort ()
		{
			InnerChannel.Abort ();
		}

		public void Close ()
		{
			InnerChannel.Close ();
		}

		public void DisplayInitializationUI ()
		{
			InnerChannel.DisplayInitializationUI ();
		}

#if NET_2_1
		IAsyncResult delegate_async;

		protected void InvokeAsync (BeginOperationDelegate beginOperationDelegate,
			object [] inValues, EndOperationDelegate endOperationDelegate,
			SendOrPostCallback operationCompletedCallback, object userState)
		{
			if (beginOperationDelegate == null)
				throw new ArgumentNullException ("beginOperationDelegate");
			if (endOperationDelegate == null)
				throw new ArgumentNullException ("endOperationDelegate");
			if (delegate_async != null)
				throw new InvalidOperationException ("Another async operation is in progress");

			var bw = new BackgroundWorker ();
			bw.DoWork += delegate (object o, DoWorkEventArgs e) {
				delegate_async = beginOperationDelegate (inValues, null, userState);
			};
			bw.RunWorkerCompleted += delegate (object o, RunWorkerCompletedEventArgs e) {
				var ret = endOperationDelegate (delegate_async);
				if (operationCompletedCallback != null)
					operationCompletedCallback (ret);
				delegate_async = null;
			};
			bw.RunWorkerAsync ();
		}
#endif
		
		void IDisposable.Dispose ()
		{
			Close ();
		}

		protected virtual TChannel CreateChannel ()
		{
			return ChannelFactory.CreateChannel ();
		}

		public void Open ()
		{
			InnerChannel.Open ();
		}

		#region ICommunicationObject implementation

		[MonoTODO]
		IAsyncResult ICommunicationObject.BeginOpen (
			AsyncCallback callback, object state)
		{
			return InnerChannel.BeginOpen (callback, state);
		}

		[MonoTODO]
		IAsyncResult ICommunicationObject.BeginOpen (
			TimeSpan timeout, AsyncCallback callback, object state)
		{
			return InnerChannel.BeginOpen (timeout, callback, state);
		}

		[MonoTODO]
		void ICommunicationObject.EndOpen (IAsyncResult result)
		{
			InnerChannel.EndOpen (result);
		}

		[MonoTODO]
		IAsyncResult ICommunicationObject.BeginClose (
			AsyncCallback callback, object state)
		{
			return InnerChannel.BeginClose (callback, state);
		}

		[MonoTODO]
		IAsyncResult ICommunicationObject.BeginClose (
			TimeSpan timeout, AsyncCallback callback, object state)
		{
			return InnerChannel.BeginClose (timeout, callback, state);
		}

		[MonoTODO]
		void ICommunicationObject.EndClose (IAsyncResult result)
		{
			InnerChannel.EndClose (result);
		}

		[MonoTODO]
		void ICommunicationObject.Close (TimeSpan timeout)
		{
			InnerChannel.Close (timeout);
		}

		[MonoTODO]
		void ICommunicationObject.Open (TimeSpan timeout)
		{
			InnerChannel.Open (timeout);
		}

		event EventHandler ICommunicationObject.Opening {
			add { InnerChannel.Opening += value; }
			remove { InnerChannel.Opening -= value; }
		}
		event EventHandler ICommunicationObject.Opened {
			add { InnerChannel.Opened += value; }
			remove { InnerChannel.Opened -= value; }
		}
		event EventHandler ICommunicationObject.Closing {
			add { InnerChannel.Closing += value; }
			remove { InnerChannel.Closing -= value; }
		}
		event EventHandler ICommunicationObject.Closed {
			add { InnerChannel.Closed += value; }
			remove { InnerChannel.Closed -= value; }
		}
		event EventHandler ICommunicationObject.Faulted {
			add { InnerChannel.Faulted += value; }
			remove { InnerChannel.Faulted -= value; }
		}

		#endregion

#if NET_2_1
		protected class ChannelBase<T> : IClientChannel, IOutputChannel, IRequestChannel where T : class
		{
			ClientBase<T> client;

			protected ChannelBase (ClientBase<T> client)
			{
				this.client = client;
			}

			[MonoTODO]
			protected IAsyncResult BeginInvoke (string methodName, object [] args, AsyncCallback callback, object state)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			protected object EndInvoke (string methodName, object [] args, IAsyncResult result)
			{
				throw new NotImplementedException ();
			}

			#region ICommunicationObject

			IAsyncResult ICommunicationObject.BeginClose (AsyncCallback callback, object state)
			{
				return client.InnerChannel.BeginClose (callback, state);
			}

			IAsyncResult ICommunicationObject.BeginClose (TimeSpan timeout, AsyncCallback callback, object state)
			{
				return client.InnerChannel.BeginClose (timeout, callback, state);
			}

			void ICommunicationObject.Close ()
			{
				client.InnerChannel.Close ();
			}

			void ICommunicationObject.Close (TimeSpan timeout)
			{
				client.InnerChannel.Close (timeout);
			}

			IAsyncResult ICommunicationObject.BeginOpen (AsyncCallback callback, object state)
			{
				return client.InnerChannel.BeginOpen (callback, state);
			}

			IAsyncResult ICommunicationObject.BeginOpen (TimeSpan timeout, AsyncCallback callback, object state)
			{
				return client.InnerChannel.BeginOpen (timeout, callback, state);
			}

			void ICommunicationObject.Open ()
			{
				client.InnerChannel.Open ();
			}

			void ICommunicationObject.Open (TimeSpan timeout)
			{
				client.InnerChannel.Open (timeout);
			}

			void ICommunicationObject.Abort ()
			{
				client.InnerChannel.Abort ();
			}

			void ICommunicationObject.EndClose (IAsyncResult result)
			{
				client.InnerChannel.EndClose (result);
			}

			void ICommunicationObject.EndOpen (IAsyncResult result)
			{
				client.InnerChannel.EndOpen (result);
			}

			CommunicationState ICommunicationObject.State {
				get { return client.InnerChannel.State; }
			}

			event EventHandler ICommunicationObject.Opened {
				add { client.InnerChannel.Opened += value; }
				remove { client.InnerChannel.Opened -= value; }
			}

			event EventHandler ICommunicationObject.Opening {
				add { client.InnerChannel.Opening += value; }
				remove { client.InnerChannel.Opening -= value; }
			}

			event EventHandler ICommunicationObject.Closed {
				add { client.InnerChannel.Closed += value; }
				remove { client.InnerChannel.Closed -= value; }
			}

			event EventHandler ICommunicationObject.Closing {
				add { client.InnerChannel.Closing += value; }
				remove { client.InnerChannel.Closing -= value; }
			}

			event EventHandler ICommunicationObject.Faulted {
				add { client.InnerChannel.Faulted += value; }
				remove { client.InnerChannel.Faulted -= value; }
			}

			#endregion

			#region IClientChannel

			[MonoTODO]
			public bool AllowInitializationUI {
				get { return client.InnerChannel.AllowInitializationUI; }
				set { client.InnerChannel.AllowInitializationUI = value; }
			}

			[MonoTODO]
			public bool DidInteractiveInitialization {
				get { return client.InnerChannel.DidInteractiveInitialization; }
			}

			public Uri Via {
				get { return client.InnerChannel.Via; }
			}

			[MonoTODO]
			public IAsyncResult BeginDisplayInitializationUI (
				AsyncCallback callback, object state)
			{
				return client.InnerChannel.BeginDisplayInitializationUI (callback, state);
			}

			[MonoTODO]
			public void EndDisplayInitializationUI (
				IAsyncResult result)
			{
				client.InnerChannel.EndDisplayInitializationUI (result);
			}

			[MonoTODO]
			public void DisplayInitializationUI ()
			{
				client.InnerChannel.DisplayInitializationUI ();
			}

			public void Dispose ()
			{
				client.InnerChannel.Dispose ();
			}

			public event EventHandler<UnknownMessageReceivedEventArgs> UnknownMessageReceived {
				add { client.InnerChannel.UnknownMessageReceived += value; }
				remove { client.InnerChannel.UnknownMessageReceived -= value; }
			}

			#endregion

			#region IContextChannel

			[MonoTODO]
			public bool AllowOutputBatching {
				get { return client.InnerChannel.AllowOutputBatching; }

				set { client.InnerChannel.AllowOutputBatching = value; }
			}

			[MonoTODO]
			public IInputSession InputSession {
				get { return client.InnerChannel.InputSession; }
			}

			[MonoTODO]
			public EndpointAddress LocalAddress {
				get { return client.InnerChannel.LocalAddress; }
			}

			[MonoTODO]
			public TimeSpan OperationTimeout {
				get { return client.InnerChannel.OperationTimeout; }
				set { client.InnerChannel.OperationTimeout = value; }
			}

			[MonoTODO]
			public IOutputSession OutputSession {
				get { return client.InnerChannel.OutputSession; }
			}

			[MonoTODO]
			public EndpointAddress RemoteAddress {
				get { return client.InnerChannel.RemoteAddress; }
			}

			[MonoTODO]
			public string SessionId {
				get { return client.InnerChannel.SessionId; }
			}

			#endregion


			[MonoTODO]
			IAsyncResult IRequestChannel.BeginRequest (Message message, AsyncCallback callback, object state)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			IAsyncResult IRequestChannel.BeginRequest (Message message, TimeSpan timeout, AsyncCallback callback, object state)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			Message IRequestChannel.EndRequest (IAsyncResult result)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			Message IRequestChannel.Request (Message message)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			Message IRequestChannel.Request (Message message, TimeSpan timeout)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			EndpointAddress IRequestChannel.RemoteAddress {
				get { throw new NotImplementedException (); }
			}

			[MonoTODO]
			Uri IRequestChannel.Via {
				get { throw new NotImplementedException (); }
			}

			[MonoTODO]
			IAsyncResult IOutputChannel.BeginSend (Message message, AsyncCallback callback, object state)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			IAsyncResult IOutputChannel.BeginSend (Message message, TimeSpan timeout, AsyncCallback callback, object state)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IOutputChannel.EndSend (IAsyncResult result)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IOutputChannel.Send (Message message)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IOutputChannel.Send (Message message, TimeSpan timeout)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			IExtensionCollection<IContextChannel> IExtensibleObject<IContextChannel>.Extensions {
				get { return client.InnerChannel.Extensions; }
			}

			[MonoTODO]
			TProperty IChannel.GetProperty<TProperty> ()
			{
				return client.InnerChannel.GetProperty<TProperty> ();
			}
		}
#endif
	}
}
