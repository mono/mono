//
// ChannelDispatcher.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.Collections.ObjectModel;
using System.Reflection;
using System.ServiceModel.Channels;
using System.Threading;
using System.Transactions;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace System.ServiceModel.Dispatcher
{
	public class ChannelDispatcher : ChannelDispatcherBase
	{
		ServiceHostBase host;

		string binding_name;		
		Collection<IErrorHandler> error_handlers
			= new Collection<IErrorHandler> ();
		IChannelListener listener;
		internal IDefaultCommunicationTimeouts timeouts; // FIXME: remove internal
		MessageVersion message_version;
		bool receive_sync, include_exception_detail_in_faults,
			manual_addressing, is_tx_receive;
		int max_tx_batch_size;
		SynchronizedCollection<IChannelInitializer> initializers
			= new SynchronizedCollection<IChannelInitializer> ();
		IsolationLevel tx_isolation_level;
		TimeSpan tx_timeout;
		ServiceThrottle throttle;

		Guid identifier = Guid.NewGuid ();
		ManualResetEvent async_event = new ManualResetEvent (false);
		EndpointListenerAsyncResult async_result;

		ListenerLoopManager loop_manager;
		SynchronizedCollection<EndpointDispatcher> endpoints;

		[MonoTODO ("get binding info from config")]
		public ChannelDispatcher (IChannelListener listener)
			: this (listener, null)
		{
		}

		public ChannelDispatcher (
			IChannelListener listener, string bindingName)
			: this (listener, bindingName, null)
		{
		}

		public ChannelDispatcher (
			IChannelListener listener, string bindingName,
			IDefaultCommunicationTimeouts timeouts)
		{
			if (listener == null)
				throw new ArgumentNullException ("listener");
			Init (listener, bindingName, timeouts);
		}

		private void Init (IChannelListener listener, string bindingName,
			IDefaultCommunicationTimeouts timeouts)
		{
			this.listener = listener;
			this.binding_name = bindingName;
			// IChannelListener is often a ChannelListenerBase
			// which implements IDefaultCommunicationTimeouts.
			this.timeouts = timeouts ?? listener as IDefaultCommunicationTimeouts ?? DefaultCommunicationTimeouts.Instance;
			endpoints = new SynchronizedCollection<EndpointDispatcher> ();
		}

		public string BindingName {
			get { return binding_name; }
		}

		public SynchronizedCollection<IChannelInitializer> ChannelInitializers {
			get { return initializers; }
		}

		protected internal override TimeSpan DefaultCloseTimeout {
			get { return timeouts.CloseTimeout; }
		}

		protected internal override TimeSpan DefaultOpenTimeout {
			get { return timeouts.OpenTimeout; }
		}

		public Collection<IErrorHandler> ErrorHandlers {
			get { return error_handlers; }
		}

		public SynchronizedCollection<EndpointDispatcher> Endpoints {
			get { return endpoints; }
		}

		[MonoTODO]
		public bool IsTransactedAccept {
			get { throw new NotImplementedException (); }
		}

		public bool IsTransactedReceive {
			get { return is_tx_receive; }
			set { is_tx_receive = value; }
		}

		public bool ManualAddressing {
			get { return manual_addressing; }
			set { manual_addressing = value; }
		}

		public int MaxTransactedBatchSize {
			get { return max_tx_batch_size; }
			set { max_tx_batch_size = value; }
		}

		public override ServiceHostBase Host {
			get { return host; }
		}

		public override IChannelListener Listener {
			get { return listener; }
		}

		public MessageVersion MessageVersion {
			get { return message_version; }
			set { message_version = value; }
		}

		public bool ReceiveSynchronously {
			get { return receive_sync; }
			set { receive_sync = value; }
		}

		public bool IncludeExceptionDetailInFaults {
			get { return include_exception_detail_in_faults; }
			set { include_exception_detail_in_faults = value; }
		}

		public ServiceThrottle ServiceThrottle {
			get { return throttle; }
			set { throttle = value; }
		}

		public IsolationLevel TransactionIsolationLevel {
			get { return tx_isolation_level; }
			set { tx_isolation_level = value; }
		}

		public TimeSpan TransactionTimeout {
			get { return tx_timeout; }
			set { tx_timeout = value; }
		}

		protected internal override void Attach (ServiceHostBase host)
		{
			this.host = host;
		}

		public override void CloseInput ()
		{
			if (State == CommunicationState.Closed)
				return;
			try {
				try {
					listener.Close ();
				} finally {
					listener = null;
				}
			} finally {
				if (async_result != null)
					async_result.Complete (false);
			}
		}

		protected internal override void Detach (ServiceHostBase host)
		{			
			this.host = null;			
		}

		protected override void OnAbort ()
		{
			throw new NotImplementedException ();
		}

		protected override IAsyncResult OnBeginClose (TimeSpan timeout,
			AsyncCallback callback, object state)
		{
			async_event.Reset ();
			async_result = new CloseAsyncResult (
				async_event, identifier, timeout,
				callback, state);
			return async_result;
		}

		protected override IAsyncResult OnBeginOpen (TimeSpan timeout,
			AsyncCallback callback, object state)
		{
			async_event.Reset ();
			async_result = new OpenAsyncResult (
				async_event, identifier, timeout,
				callback, state);
			return async_result;
		}

		protected override void OnClose (TimeSpan timeout)
		{
			ProcessClose (timeout);
		}

		protected override void OnClosed ()
		{
			if (host != null)
				host.ChannelDispatchers.Remove (this);
		}

		protected override void OnEndClose (IAsyncResult result)
		{
			if (result == null)
				throw new ArgumentNullException ("result");
			OpenAsyncResult or = result as OpenAsyncResult;
			if (or == null)
				throw new ArgumentException ("Pass an IAsyncResult instance that is returned from BeginOpen().");
			CloseInput ();
			or.AsyncWaitHandle.WaitOne ();
		}

		[MonoTODO ("this is not a real async method.")]
		protected override void OnEndOpen (IAsyncResult result)
		{
			if (result == null)
				throw new ArgumentNullException ("result");
			OpenAsyncResult or = result as OpenAsyncResult;
			if (or == null)
				throw new ArgumentException ("Pass an IAsyncResult instance that is returned from BeginOpen().");
			or.AsyncWaitHandle.WaitOne ();
		}

		protected override void OnOpen (TimeSpan timeout)
		{
			if (Host == null || MessageVersion == null)
				throw new InvalidOperationException ("Service host is not attached to this ChannelDispatcher.");

			// FIXME: hack, just to make it runnable
			loop_manager = new ListenerLoopManager (this, timeout);
		}

		[MonoTODO ("what to do here?")]
		protected override void OnOpening ()
		{
		}

		protected override void OnOpened ()
		{
			ProcessOpened ();
		}

		void ProcessClose (TimeSpan timeout)
		{
			if (loop_manager != null)
				loop_manager.Stop (timeout);
			CloseInput ();
		}

		void ProcessOpened ()
		{
			try {
				loop_manager.Setup ();
			} finally {
				if (async_result != null)
					async_result.Complete (false);
			}
		}

		internal void StartLoop ()
		{
			loop_manager.Start ();
		}

		bool IsMessageMatchesEndpointDispatcher (Message req, EndpointDispatcher endpoint)
		{
			Uri to = req.Headers.To;
			if (to == null)
				return false;
			if (to.AbsoluteUri == Constants.WsaAnonymousUri)
				return false;
			return endpoint.AddressFilter.Match (req) && endpoint.ContractFilter.Match (req);
		}
		 
		void HandleError (Exception ex)
		{
			foreach (IErrorHandler handler in ErrorHandlers)
				if (handler.HandleError (ex))
					break;
		}

		class ListenerLoopManager
		{
			delegate IChannel ChannelAcceptor ();

			ChannelDispatcher owner;
			AutoResetEvent handle;
			IReplyChannel reply;
			IInputChannel input;
			bool loop;
			Thread loop_thread;
			TimeSpan open_timeout;
			ChannelAcceptor channel_acceptor;

			public ListenerLoopManager (ChannelDispatcher owner, TimeSpan openTimeout)
			{
				this.owner = owner;
				open_timeout = openTimeout;
			}

			public void Setup ()
			{
				if (owner.Listener.State != CommunicationState.Opened)
					owner.Listener.Open (open_timeout);

				// It is tested at Open(), but strangely it is not instantiated at this point.
				foreach (var ed in owner.Endpoints)
					if (ed.DispatchRuntime.Type == null || ed.DispatchRuntime.Type.GetConstructor (Type.EmptyTypes) == null)
						throw new InvalidOperationException ("There is no default constructor for the service Type in the DispatchRuntime");
				SetupChannel ();
			}

			public void Start ()
			{
				if (loop_thread == null)
					loop_thread = new Thread (new ThreadStart (StartLoop));
				loop_thread.Start ();
			}

			void SetupChannel ()
			{
				IChannelListener<IReplyChannel> r = owner.Listener as IChannelListener<IReplyChannel>;
				if (r != null) {
					channel_acceptor = delegate { return r.AcceptChannel (); };
					return;
				}
				IChannelListener<IReplySessionChannel> rs = owner.Listener as IChannelListener<IReplySessionChannel>;
				if (rs != null) {
					channel_acceptor = delegate { return rs.AcceptChannel (); };
					return;
				}
				IChannelListener<IInputChannel> i = owner.Listener as IChannelListener<IInputChannel>;
				if (i != null) {
					channel_acceptor = delegate { return i.AcceptChannel (); };
					return;
				}
				IChannelListener<IInputSessionChannel> iss = owner.Listener as IChannelListener<IInputSessionChannel>;
				if (iss != null) {
					channel_acceptor = delegate { return iss.AcceptChannel (); };
					return;
				}
				IChannelListener<IDuplexChannel> d = owner.Listener as IChannelListener<IDuplexChannel>;
				if (d != null) {
					channel_acceptor = delegate { return d.AcceptChannel (); };
					return;
				}
				IChannelListener<IDuplexSessionChannel> ds = owner.Listener as IChannelListener<IDuplexSessionChannel>;
				if (ds != null) {
					channel_acceptor = delegate { return ds.AcceptChannel (); };
					return;
				}

				throw new InvalidOperationException (String.Format ("Unrecognized channel listener type: {0}", owner.Listener.GetType ()));
			}

			public void Stop (TimeSpan timeout)
			{
				StopLoop ();
				owner.Listener.Close ();
				if (loop_thread.IsAlive)
					loop_thread.Abort ();
				loop_thread = null;
			}

			void StartLoop ()
			{
				try {
					StartLoopCore ();
				} catch (ThreadAbortException) {
					Thread.ResetAbort ();
				}
			}

			void StartLoopCore ()
			{
				loop = true;

				// FIXME: use async WaitForBlah() method so
				// that we can stop them at our own will.
				
				//FIXME: The logic here should be entirely different as follows:
				//1. Get the message
				//2. Get the appropriate EndPointDispatcher that can handle the message
				//   which is done using the filters (AddressFilter, ContractFilter).
				//3. Let the appropriate endpoint handle the request.

				IChannel ch = channel_acceptor ();
				ch.Open (owner.timeouts.OpenTimeout);
				reply = ch as IReplyChannel;
				input = ch as IInputChannel;

				if (reply != null) {
					while (loop) {
						if (reply.WaitForRequest (owner.timeouts.ReceiveTimeout))							
							ProcessRequest ();
					}
				} else if (input != null) {
					while (loop) {
						if (input.WaitForMessage (owner.timeouts.ReceiveTimeout))
							ProcessInput ();
					}
				}
			}

			void SendEndpointNotFound (RequestContext rc, EndpointNotFoundException ex) 
			{
				try {

					MessageVersion version = rc.RequestMessage.Version;
					FaultCode fc = new FaultCode ("DestinationUnreachable", version.Addressing.Namespace);
					Message res = Message.CreateMessage (version, fc, "error occured", rc.RequestMessage.Headers.Action);
					rc.Reply (res);
				}
				catch (Exception e) { }
			}

			void ProcessRequest ()
			{
				RequestContext rc = null;
				try {
					rc = reply.ReceiveRequest (owner.timeouts.ReceiveTimeout);
					if (rc == null)
						throw new InvalidOperationException ("The reply channel didn't return RequestContext");

					EndpointDispatcher candidate = FindEndpointDispatcher (rc.RequestMessage);
					new InputOrReplyRequestProcessor (candidate.DispatchRuntime, reply).
						ProcessReply (rc);
				} catch (EndpointNotFoundException ex) {
					SendEndpointNotFound (rc, ex);
				} catch (Exception ex) {
					// FIXME: log it.
					Console.WriteLine (ex);
				}
			}

			void ProcessInput ()
			{
				try {
					Message message = input.Receive ();
					EndpointDispatcher candidate = null;
					candidate = FindEndpointDispatcher (message);
					new InputOrReplyRequestProcessor (candidate.DispatchRuntime, input).
						ProcessInput(message);
				}
				catch (Exception ex) {
					// FIXME: log it.
					Console.WriteLine (ex);
				}
			}

			EndpointDispatcher FindEndpointDispatcher (Message message) {
				EndpointDispatcher candidate = null;
				for (int i = 0; i < owner.Endpoints.Count; i++) {
					if (owner.IsMessageMatchesEndpointDispatcher (message, owner.Endpoints [i])) {
						candidate = owner.Endpoints [i];
						break;
					}
				}
				if (candidate == null)
					throw new EndpointNotFoundException (String.Format ("The request message has the target '{0}' which is not reachable in this service contract", message.Headers.To));
				return candidate;
			}

			void StopLoop ()
			{
				loop = false;
				// FIXME: send manual stop for reply or input channel.
			}
		}

		#region AsyncResult classes

		class CloseAsyncResult : EndpointListenerAsyncResult
		{
			public CloseAsyncResult (ManualResetEvent asyncEvent,
				Guid identifier, TimeSpan timeout,
				AsyncCallback callback, object state)
				: base (asyncEvent, identifier, timeout,
					callback, state)
			{
			}
		}

		class OpenAsyncResult : EndpointListenerAsyncResult
		{
			public OpenAsyncResult (ManualResetEvent asyncEvent,
				Guid identifier, TimeSpan timeout,
				AsyncCallback callback, object state)
				: base (asyncEvent, identifier, timeout,
					callback, state)
			{
			}
		}

		abstract class EndpointListenerAsyncResult : IAsyncResult
		{
			ManualResetEvent async_event;
			Guid identifier;
			TimeSpan timeout;
			AsyncCallback callback;
			object state;
			bool completed, completed_async;

			public EndpointListenerAsyncResult (
				ManualResetEvent asyncEvent,
				Guid identifier, TimeSpan timeout,
				AsyncCallback callback, object state)
			{
				async_event = asyncEvent;
				this.identifier = identifier;
				this.timeout = timeout;
				this.callback = callback;
				this.state = state;
			}

			public WaitHandle AsyncWaitHandle {
				get { return async_event; }
			}

			public bool IsCompleted {
				get { return completed; }
			}

			public TimeSpan Timeout {
				get { return timeout; }
			}

			public void Complete (bool async)
			{
				completed_async = async;
				if (callback != null)
					callback (this);
				async_event.Set ();
			}

			public object AsyncState {
				get { return state; }
			}

			public bool CompletedSynchronously {
				get { return completed_async; }
			}
		}
		#endregion
	}
}
