//
// ClientRuntimeChannel.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.Reflection;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Security;
using System.Threading;
using System.Xml;

namespace System.ServiceModel
{
	internal class DuplexClientRuntimeChannel
		: ClientRuntimeChannel, IDuplexContextChannel
	{
		public DuplexClientRuntimeChannel (ServiceEndpoint endpoint,
			ChannelFactory factory, EndpointAddress remoteAddress, Uri via)
			: base (endpoint, factory, remoteAddress, via)
		{
		}

		public bool AutomaticInputSessionShutdown {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public InstanceContext CallbackInstance { get; set; }

		Action<TimeSpan> session_shutdown_delegate;

		public void CloseOutputSession (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}

		public IAsyncResult BeginCloseOutputSession (TimeSpan timeout, AsyncCallback callback, object state)
		{
			if (session_shutdown_delegate == null)
				session_shutdown_delegate = new Action<TimeSpan> (CloseOutputSession);
			return session_shutdown_delegate.BeginInvoke (timeout, callback, state);
		}

		public void EndCloseOutputSession (IAsyncResult result)
		{
			session_shutdown_delegate.EndInvoke (result);
		}
	}

	internal class ClientRuntimeChannel
		: CommunicationObject, IClientChannel
	{
		ClientRuntime runtime;
		EndpointAddress remote_address;
		ChannelFactory factory;
		IRequestChannel request_channel;
		IOutputChannel output_channel; // could also be IDuplexChannel instance.

		#region delegates
		readonly ProcessDelegate _processDelegate;

		delegate object ProcessDelegate (MethodBase method, string operationName, object [] parameters);

		readonly RequestDelegate requestDelegate;

		delegate Message RequestDelegate (Message msg, TimeSpan timeout);

		readonly SendDelegate sendDelegate;

		delegate void SendDelegate (Message msg, TimeSpan timeout);
		#endregion

		public ClientRuntimeChannel (ServiceEndpoint endpoint,
			ChannelFactory factory, EndpointAddress remoteAddress, Uri via)
		{
			this.runtime = endpoint.CreateRuntime ();
			this.remote_address = remoteAddress ?? endpoint.Address;
			runtime.Via = via;
			this.factory = factory;
			_processDelegate = new ProcessDelegate (Process);
			requestDelegate = new RequestDelegate (Request);
			sendDelegate = new SendDelegate (Send);

			// default values
			AllowInitializationUI = true;
			OperationTimeout = TimeSpan.FromMinutes (1);

			// determine operation channel to create.
			if (factory.OpenedChannelFactory is IChannelFactory<IRequestChannel> ||
			    factory.OpenedChannelFactory is IChannelFactory<IRequestSessionChannel>)
				SetupRequestChannel ();
			else
				SetupOutputChannel ();
		}

		public ClientRuntime Runtime {
			get { return runtime; }
		}

		#region IClientChannel

		bool did_interactive_initialization;

		public bool AllowInitializationUI { get; set; }

		public bool DidInteractiveInitialization {
			get { return did_interactive_initialization; }
		}

		public Uri Via {
			get { return runtime.Via; }
		}

		class DelegatingWaitHandle : WaitHandle
		{
			public DelegatingWaitHandle (IAsyncResult [] results)
			{
				this.results = results;
			}

			IAsyncResult [] results;

			protected override void Dispose (bool disposing)
			{
				if (disposing)
					foreach (var r in results)
						r.AsyncWaitHandle.Close ();
			}

			public override bool WaitOne ()
			{
				foreach (var r in results)
					r.AsyncWaitHandle.WaitOne ();
				return true;
			}

			public override bool WaitOne (int millisecondsTimeout)
			{
				return WaitOne (millisecondsTimeout, false);
			}

			WaitHandle [] ResultWaitHandles {
				get {
					var arr = new WaitHandle [results.Length];
					for (int i = 0; i < arr.Length; i++)
						arr [i] = results [i].AsyncWaitHandle;
					return arr;
				}
			}

			public override bool WaitOne (int millisecondsTimeout, bool exitContext)
			{
				return WaitHandle.WaitAll (ResultWaitHandles, millisecondsTimeout, exitContext);
			}

			public override bool WaitOne (TimeSpan timeout, bool exitContext)
			{
				return WaitHandle.WaitAll (ResultWaitHandles, timeout, exitContext);
			}
		}

		class DisplayUIAsyncResult : IAsyncResult
		{
			public DisplayUIAsyncResult (IAsyncResult [] results)
			{
				this.results = results;
			}

			IAsyncResult [] results;

			internal IAsyncResult [] Results {
				get { return results; }
			}

			public object AsyncState {
				get { return null; }
			}

			WaitHandle wait_handle;

			public WaitHandle AsyncWaitHandle {
				get {
					if (wait_handle == null)
						wait_handle = new DelegatingWaitHandle (results);
					return wait_handle;
				}
			}

			public bool CompletedSynchronously {
				get {
					foreach (var r in results)
						if (!r.CompletedSynchronously)
							return false;
					return true;
				}
			}
			public bool IsCompleted {
				get {
					foreach (var r in results)
						if (!r.IsCompleted)
							return false;
					return true;
				}
			}
		}

		public IAsyncResult BeginDisplayInitializationUI (
			AsyncCallback callback, object state)
		{
			OnInitializationUI ();
			IAsyncResult [] arr = new IAsyncResult [runtime.InteractiveChannelInitializers.Count];
			int i = 0;
			foreach (var init in runtime.InteractiveChannelInitializers)
				arr [i++] = init.BeginDisplayInitializationUI (this, callback, state);
			return new DisplayUIAsyncResult (arr);
		}

		public void EndDisplayInitializationUI (
			IAsyncResult result)
		{
			DisplayUIAsyncResult r = (DisplayUIAsyncResult) result;
			int i = 0;
			foreach (var init in runtime.InteractiveChannelInitializers)
				init.EndDisplayInitializationUI (r.Results [i++]);

			did_interactive_initialization = true;
		}

		public void DisplayInitializationUI ()
		{
			OnInitializationUI ();
			foreach (var init in runtime.InteractiveChannelInitializers)
				init.EndDisplayInitializationUI (init.BeginDisplayInitializationUI (this, null, null));

			did_interactive_initialization = true;
		}

		void OnInitializationUI ()
		{
			if (!AllowInitializationUI && runtime.InteractiveChannelInitializers.Count > 0)
				throw new InvalidOperationException ("AllowInitializationUI is set to false but the client runtime contains one or more InteractiveChannelInitializers.");
		}

		public void Dispose ()
		{
			Close ();
		}

		public event EventHandler<UnknownMessageReceivedEventArgs> UnknownMessageReceived;

		#endregion

		#region IContextChannel

		[MonoTODO]
		public bool AllowOutputBatching { get; set; }

		public IInputSession InputSession {
			get {
				ISessionChannel<IInputSession> ch = request_channel as ISessionChannel<IInputSession>;
				ch = ch ?? output_channel as ISessionChannel<IInputSession>;
				if (ch != null)
					return ch.Session;
				var dch = output_channel as ISessionChannel<IDuplexSession>;
				return dch != null ? dch.Session : null;
			}
		}

		public EndpointAddress LocalAddress {
			get {
				var dc = OperationChannel as IDuplexChannel;
				return dc != null ? dc.LocalAddress : null;
			}
		}

		[MonoTODO]
		public TimeSpan OperationTimeout { get; set; }

		public IOutputSession OutputSession {
			get {
				ISessionChannel<IOutputSession> ch = request_channel as ISessionChannel<IOutputSession>;
				ch = ch ?? output_channel as ISessionChannel<IOutputSession>;
				if (ch != null)
					return ch.Session;
				var dch = output_channel as ISessionChannel<IDuplexSession>;
				return dch != null ? dch.Session : null;
			}
		}

		public EndpointAddress RemoteAddress {
			get { return request_channel != null ? request_channel.RemoteAddress : output_channel.RemoteAddress; }
		}

		public string SessionId {
			get { return OutputSession != null ? OutputSession.Id : InputSession != null ? InputSession.Id : null; }
		}

		#endregion

		// CommunicationObject
		protected internal override TimeSpan DefaultOpenTimeout {
			get { return factory.DefaultOpenTimeout; }
		}

		protected internal override TimeSpan DefaultCloseTimeout {
			get { return factory.DefaultCloseTimeout; }
		}

		protected override void OnAbort ()
		{
			factory.Abort ();
		}

		protected override IAsyncResult OnBeginClose (
			TimeSpan timeout, AsyncCallback callback, object state)
		{
			return factory.BeginClose (timeout, callback, state);
		}

		protected override void OnEndClose (IAsyncResult result)
		{
			factory.EndClose (result);
		}

		protected override void OnClose (TimeSpan timeout)
		{
			factory.Close (timeout);
		}

		protected override IAsyncResult OnBeginOpen (
			TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new SystemException ("INTERNAL ERROR: this should not be called (or not supported yet)");
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
		}

		protected override void OnOpen (TimeSpan timeout)
		{
			if (runtime.InteractiveChannelInitializers.Count > 0 && !DidInteractiveInitialization)
				throw new InvalidOperationException ("The client runtime is assigned interactive channel initializers, and in such case DisplayInitializationUI must be called before the channel is opened.");
		}

		// IChannel

		IChannel OperationChannel {
			get { return (IChannel) request_channel ?? output_channel; }
		}

		public T GetProperty<T> () where T : class
		{
			return OperationChannel.GetProperty<T> ();
		}

		// IExtensibleObject<IContextChannel>
		[MonoTODO]
		public IExtensionCollection<IContextChannel> Extensions {
			get { throw new NotImplementedException (); }
		}

		#region Request/Output processing

		public IAsyncResult BeginProcess (MethodBase method, string operationName, object [] parameters, AsyncCallback callback, object asyncState)
		{
			return _processDelegate.BeginInvoke (method, operationName, parameters, callback, asyncState);
		}

		public object EndProcess (MethodBase method, string operationName, object [] parameters, IAsyncResult result)
		{
			if (result == null)
				throw new ArgumentNullException ("result");
			if (parameters == null)
				throw new ArgumentNullException ("parameters");
			// FIXME: the method arguments should be verified to be 
			// identical to the arguments in the corresponding begin method.
			return _processDelegate.EndInvoke (result);
		}

		public object Process (MethodBase method, string operationName, object [] parameters)
		{
			try {
				return DoProcess (method, operationName, parameters);
			} catch (Exception ex) {
				Console.Write ("Exception in async operation: ");
				Console.WriteLine (ex);
				throw;
			}
		}

		object DoProcess (MethodBase method, string operationName, object [] parameters)
		{
			if (AllowInitializationUI)
				DisplayInitializationUI ();
			OperationDescription od = SelectOperation (method, operationName, parameters);
			if (!od.IsOneWay)
				return Request (od, parameters);
			else {
				Output (od, parameters);
				return null;
			}
		}

		OperationDescription SelectOperation (MethodBase method, string operationName, object [] parameters)
		{
			string operation;
			if (Runtime.OperationSelector != null)
				operation = Runtime.OperationSelector.SelectOperation (method, parameters);
			else
				operation = operationName;
			OperationDescription od = factory.Endpoint.Contract.Operations.Find (operation);
			if (od == null)
				throw new Exception (String.Format ("OperationDescription for operation '{0}' was not found in its internally-generated contract.", operation));
			return od;
		}

		BindingParameterCollection CreateBindingParameters ()
		{
			BindingParameterCollection pl =
				new BindingParameterCollection ();

			ContractDescription cd = factory.Endpoint.Contract;
#if !NET_2_1
			pl.Add (ChannelProtectionRequirements.CreateFromContract (cd));

			foreach (IEndpointBehavior behavior in factory.Endpoint.Behaviors)
				behavior.AddBindingParameters (factory.Endpoint, pl);
#endif

			return pl;
		}

		// This handles IDuplexChannel, IOutputChannel, and those for session channels.
		void SetupOutputChannel ()
		{
			if (output_channel != null)
				return;

			var method = factory.OpenedChannelFactory.GetType ().GetMethod ("CreateChannel", new Type [] {typeof (EndpointAddress), typeof (Uri)});
			output_channel = (IOutputChannel) method.Invoke (factory.OpenedChannelFactory, new object [] {remote_address, Via});
		}

		// This handles both IRequestChannel and IRequestSessionChannel.
		void SetupRequestChannel ()
		{
			if (request_channel != null)
				return;

			var method = factory.OpenedChannelFactory.GetType ().GetMethod ("CreateChannel", new Type [] {typeof (EndpointAddress), typeof (Uri)});
			request_channel = (IRequestChannel) method.Invoke (factory.OpenedChannelFactory, new object [] {remote_address, Via});
		}

		void Output (OperationDescription od, object [] parameters)
		{
			if (output_channel.State != CommunicationState.Opened)
				output_channel.Open ();

			ClientOperation op = runtime.Operations [od.Name];
			Send (CreateRequest (op, parameters), OperationTimeout);
		}

		object Request (OperationDescription od, object [] parameters)
		{
			if (OperationChannel.State != CommunicationState.Opened)
				OperationChannel.Open ();

			ClientOperation op = runtime.Operations [od.Name];
			object [] inspections = new object [runtime.MessageInspectors.Count];
			Message req = CreateRequest (op, parameters);

			for (int i = 0; i < inspections.Length; i++)
				inspections [i] = runtime.MessageInspectors [i].BeforeSendRequest (ref req, this);

			Message res = Request (req, OperationTimeout);
			if (res.IsFault) {
				MessageFault fault = MessageFault.CreateFault (res, runtime.MaxFaultSize);
				if (fault.HasDetail && fault is MessageFault.SimpleMessageFault) {
					MessageFault.SimpleMessageFault simpleFault = fault as MessageFault.SimpleMessageFault;
					object detail = simpleFault.Detail;
					Type t = detail.GetType ();
					Type faultType = typeof (FaultException<>).MakeGenericType (t);
					object [] constructorParams = new object [] { detail, fault.Reason, fault.Code, fault.Actor };
					FaultException fe = (FaultException) Activator.CreateInstance (faultType, constructorParams);
					throw fe;
				}
				else {
					// given a MessageFault, it is hard to figure out the type of the embedded detail
					throw new FaultException(fault);
				}
			}

			for (int i = 0; i < inspections.Length; i++)
				runtime.MessageInspectors [i].AfterReceiveReply (ref res, inspections [i]);

			if (op.DeserializeReply)
				return op.GetFormatter ().DeserializeReply (res, parameters);
			else
				return res;
		}

		#region Message-based Request() and Send()
		// They are internal for ClientBase<T>.ChannelBase use.
		internal Message Request (Message msg, TimeSpan timeout)
		{
			if (request_channel != null)
				return request_channel.Request (msg, timeout);
			else {
				DateTime startTime = DateTime.Now;
				output_channel.Send (msg, timeout);
				return ((IDuplexChannel) output_channel).Receive (timeout - (DateTime.Now - startTime));
			}
		}

		internal IAsyncResult BeginRequest (Message msg, TimeSpan timeout, AsyncCallback callback, object state)
		{
			return requestDelegate.BeginInvoke (msg, timeout, callback, state);
		}

		internal Message EndRequest (IAsyncResult result)
		{
			return requestDelegate.EndInvoke (result);
		}

		internal void Send (Message msg, TimeSpan timeout)
		{
			output_channel.Send (msg, timeout);
		}

		internal IAsyncResult BeginSend (Message msg, TimeSpan timeout, AsyncCallback callback, object state)
		{
			return sendDelegate.BeginInvoke (msg, timeout, callback, state);
		}

		internal void EndSend (IAsyncResult result)
		{
			sendDelegate.EndInvoke (result);
		}
		#endregion

		Message CreateRequest (ClientOperation op, object [] parameters)
		{
			MessageVersion version = factory.Endpoint.Binding.MessageVersion;
			if (version == null)
				version = MessageVersion.Default;

			Message msg;
			if (op.SerializeRequest)
				msg = op.GetFormatter ().SerializeRequest (
					version, parameters);
			else
				msg = (Message) parameters [0];

			if (OutputSession != null)
				msg.Headers.MessageId = new UniqueId (OutputSession.Id);
			msg.Properties.AllowOutputBatching = AllowOutputBatching;

			return msg;
		}

		#endregion
	}
}
