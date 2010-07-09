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
	internal interface IChannelDispatcherBoundListener
	{
		ChannelDispatcher ChannelDispatcher { get; set; }
	}

	public class ChannelDispatcher : ChannelDispatcherBase
	{
		class EndpointDispatcherCollection : SynchronizedCollection<EndpointDispatcher>
		{
			public EndpointDispatcherCollection (ChannelDispatcher owner)
			{
				this.owner = owner;
			}

			ChannelDispatcher owner;

			protected override void ClearItems ()
			{
				foreach (var ed in this)
					ed.ChannelDispatcher = null;
				base.ClearItems ();
			}

			protected override void InsertItem (int index, EndpointDispatcher item)
			{
				item.ChannelDispatcher = owner;
				base.InsertItem (index, item);
			}

			protected override void RemoveItem (int index)
			{
				if (index < Count)
					this [index].ChannelDispatcher = null;
				base.RemoveItem (index);
			}

			protected override void SetItem (int index, EndpointDispatcher item)
			{
				item.ChannelDispatcher = owner;
				base.SetItem (index, item);
			}
		}

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
			endpoints = new EndpointDispatcherCollection (this);
		}

		internal EndpointDispatcher InitializeServiceEndpoint (Type serviceType, ServiceEndpoint se)
		{
			//Attach one EndpointDispacher to the ChannelDispatcher
			EndpointDispatcher ed = new EndpointDispatcher (se.Address, se.Contract.Name, se.Contract.Namespace);
			this.Endpoints.Add (ed);
			ed.InitializeServiceEndpoint (false, serviceType, se);
			return ed;
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
			var bl = listener as IChannelDispatcherBoundListener;
			if (bl != null)
				bl.ChannelDispatcher = this;
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
				open_delegate = new Action<TimeSpan> (OnOpen);
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

			loop_manager.Setup (timeout);
		}

		protected override void OnOpening ()
		{
			base.OnOpening ();
			loop_manager = new ListenerLoopManager (this);
		}

		protected override void OnOpened ()
		{
			base.OnOpened ();
			StartLoop ();
		}

		void StartLoop ()
		{
			// FIXME: not sure if it should be filled here.
			if (ServiceThrottle == null)
				ServiceThrottle = new ServiceThrottle (this);

			loop_manager.Start ();
		}
	}

		// isolated from ChannelDispatcher
		class ListenerLoopManager
		{
			ChannelDispatcher owner;
			AutoResetEvent throttle_wait_handle = new AutoResetEvent (false);
			AutoResetEvent creator_handle = new AutoResetEvent (false);
			ManualResetEvent stop_handle = new ManualResetEvent (false);
			bool loop;
			Thread loop_thread;
			DateTime close_started;
			TimeSpan close_timeout;
			Func<IAsyncResult> channel_acceptor;
			List<IChannel> channels = new List<IChannel> ();
			AddressFilterMode address_filter_mode;

			public ListenerLoopManager (ChannelDispatcher owner)
			{
				this.owner = owner;
				var sba = owner.Host != null ? owner.Host.Description.Behaviors.Find<ServiceBehaviorAttribute> () : null;
				if (sba != null)
					address_filter_mode = sba.AddressFilterMode;
			}

			public void Setup (TimeSpan openTimeout)
			{
				if (owner.Listener.State != CommunicationState.Created)
					throw new InvalidOperationException ("Tried to open the channel listener which is bound to ChannelDispatcher, but it is not at Created state");
				owner.Listener.Open (openTimeout);

				// It is tested at Open(), but strangely it is not instantiated at this point.
				foreach (var ed in owner.Endpoints)
					if (ed.DispatchRuntime.InstanceContextProvider == null && (ed.DispatchRuntime.Type == null || ed.DispatchRuntime.Type.GetConstructor (Type.EmptyTypes) == null))
						throw new InvalidOperationException ("There is no default constructor for the service Type in the DispatchRuntime");
				SetupChannelAcceptor ();
			}

			public void Start ()
			{
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
						creator_handle.Set ();
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
				if (loop_thread == null)
					return;

				close_started = DateTime.Now;
				close_timeout = timeout;
				loop = false;
				creator_handle.Set ();
				throttle_wait_handle.Set (); // break primary loop
				if (stop_handle != null) {
					stop_handle.WaitOne (timeout > TimeSpan.Zero ? timeout : TimeSpan.FromTicks (1));
					stop_handle.Close ();
					stop_handle = null;
				}
				if (owner.Listener.State != CommunicationState.Closed) {
					// FIXME: log it
					Console.WriteLine ("Channel listener '{0}' is not closed. Aborting.", owner.Listener.GetType ());
					owner.Listener.Abort ();
				}
				if (loop_thread != null && loop_thread.IsAlive)
					loop_thread.Abort ();
				loop_thread = null;
			}

			public void CloseInput ()
			{
				foreach (var ch in channels.ToArray ()) {
					if (ch.State == CommunicationState.Closed)
						channels.Remove (ch); // zonbie, if exists
					else {
						try {
							ch.Close (close_timeout - (DateTime.Now - close_started));
						} catch (Exception ex) {
							// FIXME: log it.
							Console.WriteLine (ex);
							ch.Abort ();
						}
					}
				}
			}

			void Loop ()
			{
				try {
					LoopCore ();
				} catch (Exception ex) {
					// FIXME: log it
					Console.WriteLine ("ListenerLoopManager caught an exception inside dispatcher loop, which is likely thrown by the channel listener {0}", owner.Listener);
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

				while (loop) {
					// FIXME: take MaxConcurrentCalls into consideration appropriately.
					while (loop && channels.Count < Math.Min (owner.ServiceThrottle.MaxConcurrentSessions, owner.ServiceThrottle.MaxConcurrentCalls)) {
						// FIXME: this should not be required, but saves multi-ChannelDispatcher case (Throttling enabled) for HTTP standalone listener...
						Thread.Sleep (100);
						channel_acceptor ();
						creator_handle.WaitOne (); // released by ChannelAccepted()
					}
					if (!loop)
						break;
					throttle_wait_handle.WaitOne (); // released by IChannel.Close()
				}
				try {
					owner.Listener.Close ();
				} finally {
					// make sure to close both listener and channels.
					owner.CloseInput ();
				}
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

				lock (channels)
					channels.Add (ch);
				ch.Opened += delegate {
					ch.Faulted += delegate {
						lock (channels)
							if (channels.Contains (ch))
								channels.Remove (ch);
						throttle_wait_handle.Set (); // release loop wait lock.
						};
					ch.Closed += delegate {
						lock (channels)
							if (channels.Contains (ch))
								channels.Remove (ch);
						throttle_wait_handle.Set (); // release loop wait lock.
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
				else
					reply.Close ();
			}

			void TryReceiveDone (IAsyncResult result)
			{
				Message msg;
				var input = (IInputChannel) result.AsyncState;
				if (input.EndTryReceive (result, out msg))
					ProcessInput (input, msg);
				else
					input.Close ();
			}

			void ProcessRequest (IReplyChannel reply, RequestContext rc)
			{
				try {
					var req = rc.RequestMessage;
					var ed = FindEndpointDispatcher (req);
					new InputOrReplyRequestProcessor (ed.DispatchRuntime, reply).ProcessReply (rc);
				} catch (Exception ex) {
					foreach (var eh in owner.ErrorHandlers)
						if (eh.HandleError (ex))
							return; // error is handled appropriately.

					// FIXME: log it.
					Console.WriteLine (ex);

					Message res = null;
					foreach (var eh in owner.ErrorHandlers)
						eh.ProvideFault (ex, owner.MessageVersion, ref res);
					if (res == null) {
						var conv = reply.GetProperty<FaultConverter> () ?? FaultConverter.GetDefaultFaultConverter (rc.RequestMessage.Version);
						if (!conv.TryCreateFaultMessage (ex, out res))
							res = Message.CreateMessage (owner.MessageVersion, new FaultCode ("Receiver"), ex.Message, owner.MessageVersion.Addressing.FaultNamespace);
					}

					rc.Reply (res);
				} finally {
					if (rc != null)
						rc.Close ();
					// unless it is closed by session/call manager, move it back to the loop to receive the next message.
					if (loop && reply.State != CommunicationState.Closed)
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
					if (loop && input.State != CommunicationState.Closed)
						ProcessRequestOrInput (input);
				}
			}

			EndpointDispatcher FindEndpointDispatcher (Message message) {
				EndpointDispatcher candidate = null;
				bool hasEndpointMatch = false;
				foreach (var endpoint in owner.Endpoints) {
					if (endpoint.AddressFilter.Match (message)) {
						hasEndpointMatch = true;
						if (!endpoint.ContractFilter.Match (message))
							continue;
						var newdis = endpoint;
						if (candidate == null || candidate.FilterPriority < newdis.FilterPriority)
							candidate = newdis;
						else if (candidate.FilterPriority == newdis.FilterPriority)
							throw new MultipleFilterMatchesException ();
					}
				}
				if (candidate == null && !hasEndpointMatch) {
					if (owner.Host != null)
						owner.Host.OnUnknownMessageReceived (message);
					// we have to return a fault to the client anyways...
					throw new EndpointNotFoundException ();
				}
				else if (candidate == null)
					// FIXME: It is not a good place to check, but anyways detach this error from EndpointNotFoundException.
					throw new ActionNotSupportedException (String.Format ("Action '{0}' did not match any operations in the target contract", message.Headers.Action));

				return candidate;
			}
		}
}
