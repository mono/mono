//
// ChannelDispatcher.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005,2009 Novell, Inc.  http://www.novell.com
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
			set {
				ThrowIfDisposedOrImmutable ();
				receive_sync = value; 
			}
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
			if (loop_manager != null)
				loop_manager.CloseInput ();
		}

		protected internal override void Detach (ServiceHostBase host)
		{			
			this.host = null;			
		}

		protected override void OnAbort ()
		{
			if (loop_manager != null)
				loop_manager.Stop (TimeSpan.FromTicks (1));
		}

		Action<TimeSpan> open_delegate;
		Action<TimeSpan> close_delegate;

		protected override IAsyncResult OnBeginClose (TimeSpan timeout,
			AsyncCallback callback, object state)
		{
			if (close_delegate == null)
				close_delegate = new Action<TimeSpan> (OnClose);
			return close_delegate.BeginInvoke (timeout, callback, state);
		}

		protected override IAsyncResult OnBeginOpen (TimeSpan timeout,
			AsyncCallback callback, object state)
		{
			if (open_delegate == null)
				open_delegate = new Action<TimeSpan> (OnClose);
			return open_delegate.BeginInvoke (timeout, callback, state);
		}

		protected override void OnClose (TimeSpan timeout)
		{
			if (loop_manager != null)
				loop_manager.Stop (timeout);
		}

		protected override void OnClosed ()
		{
			if (host != null)
				host.ChannelDispatchers.Remove (this);
			base.OnClosed ();
		}

		protected override void OnEndClose (IAsyncResult result)
		{
			close_delegate.EndInvoke (result);
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
			open_delegate.EndInvoke (result);
		}

		protected override void OnOpen (TimeSpan timeout)
		{
			if (Host == null || MessageVersion == null)
				throw new InvalidOperationException ("Service host is not attached to this ChannelDispatcher.");

			loop_manager = new ListenerLoopManager (this, timeout);
		}

		[MonoTODO ("what to do here?")]
		protected override void OnOpening ()
		{
		}

		protected override void OnOpened ()
		{
			loop_manager.Setup ();
		}

		internal void StartLoop ()
		{
			// FIXME: not sure if it should be filled here.
			if (ServiceThrottle == null)
				ServiceThrottle = new ServiceThrottle ();

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
			ChannelDispatcher owner;
			AutoResetEvent handle = new AutoResetEvent (false);
			AutoResetEvent creator_handle = new AutoResetEvent (false);
			ManualResetEvent stop_handle = new ManualResetEvent (false);
			bool loop;
			Thread loop_thread;
			TimeSpan open_timeout;
			Func<IAsyncResult> channel_acceptor;
			List<IChannel> channels = new List<IChannel> ();

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
					if (ed.DispatchRuntime.InstanceContextProvider == null && (ed.DispatchRuntime.Type == null || ed.DispatchRuntime.Type.GetConstructor (Type.EmptyTypes) == null))
						throw new InvalidOperationException ("There is no default constructor for the service Type in the DispatchRuntime");
				SetupChannelAcceptor ();
			}

			public void Start ()
			{
				foreach (var ed in owner.Endpoints)
					if (ed.DispatchRuntime.InstanceContextProvider == null)
						ed.DispatchRuntime.InstanceContextProvider = new DefaultInstanceContextProvider ();

				if (loop_thread == null)
					loop_thread = new Thread (new ThreadStart (Loop));
				loop_thread.Start ();
			}

			Func<IAsyncResult> CreateAcceptor<TChannel> (IChannelListener l) where TChannel : class, IChannel
			{
				IChannelListener<TChannel> r = l as IChannelListener<TChannel>;
				if (r == null)
					return null;
				AsyncCallback callback = delegate (IAsyncResult result) {
					try {
						ChannelAccepted (r.EndAcceptChannel (result));
					} catch (Exception ex) {
						Console.WriteLine ("Exception during finishing channel acceptance.");
						Console.WriteLine (ex);
					}
				};
				return delegate {
					try {
						return r.BeginAcceptChannel (callback, null);
					} catch (Exception ex) {
						Console.WriteLine ("Exception during accepting channel.");
						Console.WriteLine (ex);
						throw;
					}
				};
			}

			void SetupChannelAcceptor ()
			{
				var l = owner.Listener;
				channel_acceptor =
					CreateAcceptor<IReplyChannel> (l) ??
					CreateAcceptor<IReplySessionChannel> (l) ??
					CreateAcceptor<IInputChannel> (l) ??
					CreateAcceptor<IInputSessionChannel> (l) ??
					CreateAcceptor<IDuplexChannel> (l) ??
					CreateAcceptor<IDuplexSessionChannel> (l);
				if (channel_acceptor == null)
					throw new InvalidOperationException (String.Format ("Unrecognized channel listener type: {0}", l.GetType ()));
			}

			public void Stop (TimeSpan timeout)
			{
				loop = false;
				creator_handle.Set ();
				handle.Set ();
				if (stop_handle != null) {
					stop_handle.WaitOne (timeout > TimeSpan.Zero ? timeout : TimeSpan.FromTicks (1));
					stop_handle.Close ();
					stop_handle = null;
				}
				if (owner.Listener.State != CommunicationState.Closed)
					owner.Listener.Abort ();
				if (loop_thread != null && loop_thread.IsAlive)
					loop_thread.Abort ();
				loop_thread = null;
			}

			public void CloseInput ()
			{
				foreach (var ch in channels.ToArray ()) {
					if (ch.State == CommunicationState.Closed)
						channels.Remove (ch); // zonbie, if exists
					else
						ch.Close ();
				}
			}

			void Loop ()
			{
				try {
					LoopCore ();
				} catch (Exception ex) {
					// FIXME: log it
					Console.WriteLine ("ChannelDispatcher caught an exception inside dispatcher loop, which is likely thrown by the channel listener {0}", owner.Listener);
					Console.WriteLine (ex);
				} finally {
					if (stop_handle != null)
						stop_handle.Set ();
				}
			}

			void LoopCore ()
			{
				loop = true;

				// FIXME: use WaitForChannel() for (*only* for) transacted channel listeners.
				// http://social.msdn.microsoft.com/Forums/en-US/wcf/thread/3faa4a5e-8602-4dbe-a181-73b3f581835e
				
				//FIXME: The logic here should be somewhat different as follows:
				//1. Get the message
				//2. Get the appropriate EndPointDispatcher that can handle the message
				//   which is done using the filters (AddressFilter, ContractFilter).
				//3. Let the appropriate endpoint handle the request.

				while (loop) {
					while (loop && channels.Count < owner.ServiceThrottle.MaxConcurrentSessions) {
						channel_acceptor ();
						creator_handle.WaitOne (); // released by ChannelAccepted()
						creator_handle.Reset ();
					}
					if (!loop)
						break;
					handle.WaitOne (); // released by IChannel.Close()
					handle.Reset ();
				}
				owner.Listener.Close ();
				owner.CloseInput ();
			}

			void ChannelAccepted (IChannel ch)
			{
			try {
				if (ch == null) // could happen when it was aborted
					return;
				if (!loop) {
					var dis = ch as IDisposable;
					if (dis != null)
						dis.Dispose ();
					return;
				}

				channels.Add (ch);
				ch.Opened += delegate {
					ch.Faulted += delegate {
						if (channels.Contains (ch))
							channels.Remove (ch);
						handle.Set (); // release loop wait lock.
						};
					ch.Closed += delegate {
						if (channels.Contains (ch))
							channels.Remove (ch);
						handle.Set (); // release loop wait lock.
						};
					};
				ch.Open ();
			} finally {
				creator_handle.Set ();
			}

				ProcessRequestOrInput (ch);
			}

			void ProcessRequestOrInput (IChannel ch)
			{
				var reply = ch as IReplyChannel;
				var input = ch as IInputChannel;

				if (reply != null) {
					if (owner.ReceiveSynchronously) {
						RequestContext rc;
						if (reply.TryReceiveRequest (owner.timeouts.ReceiveTimeout, out rc))
							ProcessRequest (reply, rc);
					} else {
						reply.BeginTryReceiveRequest (owner.timeouts.ReceiveTimeout, TryReceiveRequestDone, reply);
					}
				} else if (input != null) {
					if (owner.ReceiveSynchronously) {
						Message msg;
						if (input.TryReceive (owner.timeouts.ReceiveTimeout, out msg))
							ProcessInput (input, msg);
					} else {
						input.BeginTryReceive (owner.timeouts.ReceiveTimeout, TryReceiveDone, input);
					}
				}
			}

			void TryReceiveRequestDone (IAsyncResult result)
			{
				RequestContext rc;
				var reply = (IReplyChannel) result.AsyncState;
				if (reply.EndTryReceiveRequest (result, out rc))
					ProcessRequest (reply, rc);
			}

			void TryReceiveDone (IAsyncResult result)
			{
				Message msg;
				var input = (IInputChannel) result.AsyncState;
				if (input.EndTryReceive (result, out msg))
					ProcessInput (input, msg);
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

			void ProcessRequest (IReplyChannel reply, RequestContext rc)
			{
				try {
					EndpointDispatcher candidate = FindEndpointDispatcher (rc.RequestMessage);
					new InputOrReplyRequestProcessor (candidate.DispatchRuntime, reply).
						ProcessReply (rc);
				} catch (EndpointNotFoundException ex) {
					SendEndpointNotFound (rc, ex);
				} catch (Exception ex) {
					// FIXME: log it.
					Console.WriteLine (ex);
				} finally {
					// unless it is closed by session/call manager, move it back to the loop to receive the next message.
					if (reply.State != CommunicationState.Closed)
						ProcessRequestOrInput (reply);
				}
			}

			void ProcessInput (IInputChannel input, Message message)
			{
				try {
					EndpointDispatcher candidate = null;
					candidate = FindEndpointDispatcher (message);
					new InputOrReplyRequestProcessor (candidate.DispatchRuntime, input).
						ProcessInput (message);
				}
				catch (Exception ex) {
					// FIXME: log it.
					Console.WriteLine (ex);
				} finally {
					// unless it is closed by session/call manager, move it back to the loop to receive the next message.
					if (input.State != CommunicationState.Closed)
						ProcessRequestOrInput (input);
				}
			}

			EndpointDispatcher FindEndpointDispatcher (Message message) {
				EndpointDispatcher candidate = null;
				for (int i = 0; i < owner.Endpoints.Count; i++) {
					if (owner.IsMessageMatchesEndpointDispatcher (message, owner.Endpoints [i])) {
						var newdis = owner.Endpoints [i];
						if (candidate == null || candidate.FilterPriority < newdis.FilterPriority)
							candidate = newdis;
						else if (candidate.FilterPriority == newdis.FilterPriority)
							throw new MultipleFilterMatchesException ();
					}
				}
				if (candidate == null)
					owner.Host.OnUnknownMessageReceived (message);
				return candidate;
			}
		}
	}
}
