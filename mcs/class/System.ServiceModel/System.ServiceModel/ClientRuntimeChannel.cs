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

namespace System.ServiceModel
{
#if TARGET_DOTNET
	[MonoTODO]
	public
#else
	internal
#endif
	class ClientRuntimeChannel
		: CommunicationObject, IClientChannel
	{
		ClientRuntime runtime;
		ChannelFactory factory;
		IRequestChannel request_channel;
		IOutputChannel output_channel;
		readonly ProcessDelegate _processDelegate;

		delegate object ProcessDelegate (MethodBase method, string operationName, object [] parameters);

		public ClientRuntimeChannel (ClientRuntime runtime,
			ChannelFactory factory)
		{
			this.runtime = runtime;
			this.factory = factory;
			_processDelegate = new ProcessDelegate (Process);
		}

		public ClientRuntime Runtime {
			get { return runtime; }
		}

		#region IClientChannel

		[MonoTODO]
		public bool AllowInitializationUI {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public bool DidInteractiveInitialization {
			get { throw new NotImplementedException (); }
		}

		public Uri Via {
			get { return runtime.Via; }
		}

		[MonoTODO]
		public IAsyncResult BeginDisplayInitializationUI (
			AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EndDisplayInitializationUI (
			IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DisplayInitializationUI ()
		{
			throw new NotImplementedException ();
		}

		public void Dispose ()
		{
			Close ();
		}

		public event EventHandler<UnknownMessageReceivedEventArgs> UnknownMessageReceived;

		#endregion

		#region IContextChannel

		[MonoTODO]
		public bool AllowOutputBatching {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public IInputSession InputSession {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public EndpointAddress LocalAddress {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public TimeSpan OperationTimeout {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public IOutputSession OutputSession {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public EndpointAddress RemoteAddress {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string SessionId {
			get { throw new NotImplementedException (); }
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
		}

		// IChannel
		public T GetProperty<T> () where T : class
		{
			return factory.GetProperty<T> ();
		}

		// IExtensibleObject<IContextChannel>
		[MonoTODO]
		public IExtensionCollection<IContextChannel> Extensions {
			get { throw new NotImplementedException (); }
		}

		#region Request/Output processing

		public IAsyncResult BeginProcess (MethodBase method, string operationName, object [] parameters) {
			object [] param = new object [parameters.Length - 2];
			for (int i = 0; i < param.Length; i++)
				param [i] = parameters [i];
			return _processDelegate.BeginInvoke (method, operationName, param, (AsyncCallback) parameters [parameters.Length - 2], parameters [parameters.Length - 1]);
		}

		public object EndProcess (MethodBase method, string operationName, object [] parameters) {
			return _processDelegate.EndInvoke ((IAsyncResult) parameters [0]);
		}

		public object Process (MethodBase method, string operationName, object [] parameters)
		{
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

		void SetupOutputChannel ()
		{
			if (output_channel != null)
				return;
			BindingParameterCollection pl =
				CreateBindingParameters ();
			bool session = false;
			switch (factory.Endpoint.Contract.SessionMode) {
			case SessionMode.Required:
				session = factory.Endpoint.Binding.CanBuildChannelFactory<IOutputSessionChannel> (pl);
				if (!session)
					throw new InvalidOperationException ("The contract requires session support, but the binding does not support it.");
				break;
			case SessionMode.Allowed:
				session = !factory.Endpoint.Binding.CanBuildChannelFactory<IOutputChannel> (pl);
				break;
			}

			EndpointAddress address = factory.Endpoint.Address;
			Uri via = Runtime.Via;

			if (session) {
				IChannelFactory<IOutputSessionChannel> f =
					factory.Endpoint.Binding.BuildChannelFactory<IOutputSessionChannel> (pl);
				f.Open ();
				output_channel = f.CreateChannel (address, via);
			} else {
				IChannelFactory<IOutputChannel> f =
					factory.Endpoint.Binding.BuildChannelFactory<IOutputChannel> (pl);
				f.Open ();
				output_channel = f.CreateChannel (address, via);
			}

			output_channel.Open ();
		}

		void SetupRequestChannel ()
		{
			if (request_channel != null)
				return;

			BindingParameterCollection pl =
				CreateBindingParameters ();
			bool session = false;
			switch (factory.Endpoint.Contract.SessionMode) {
			case SessionMode.Required:
				session = factory.Endpoint.Binding.CanBuildChannelFactory<IRequestSessionChannel> (pl);
				if (!session)
					throw new InvalidOperationException ("The contract requires session support, but the binding does not support it.");
				break;
			case SessionMode.Allowed:
				session = !factory.Endpoint.Binding.CanBuildChannelFactory<IRequestChannel> (pl);
				break;
			}

			EndpointAddress address = factory.Endpoint.Address;
			Uri via = Runtime.Via;

			if (session) {
				IChannelFactory<IRequestSessionChannel> f =
					factory.Endpoint.Binding.BuildChannelFactory<IRequestSessionChannel> (pl);
				f.Open ();
				request_channel = f.CreateChannel (address, via);
			} else {
				IChannelFactory<IRequestChannel> f =
					factory.Endpoint.Binding.BuildChannelFactory<IRequestChannel> (pl);
				f.Open ();
				request_channel = f.CreateChannel (address, via);
			}

			request_channel.Open ();
		}

		void Output (OperationDescription od, object [] parameters)
		{
			SetupOutputChannel ();

			ClientOperation op = runtime.Operations [od.Name];
			Output (CreateRequest (op, parameters));
		}

		object Request (OperationDescription od, object [] parameters)
		{
			SetupRequestChannel ();

			ClientOperation op = runtime.Operations [od.Name];
			object [] inspections = new object [runtime.MessageInspectors.Count];
			Message req = CreateRequest (op, parameters);

			for (int i = 0; i < inspections.Length; i++)
				inspections [i] = runtime.MessageInspectors [i].BeforeSendRequest (ref req, this);

			Message res = Request (req);
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

		Message Request (Message msg)
		{
			return request_channel.Request (msg, factory.Endpoint.Binding.SendTimeout);
		}

		void Output (Message msg)
		{
			output_channel.Send (msg, factory.Endpoint.Binding.SendTimeout);
		}

		Message CreateRequest (ClientOperation op, object [] parameters)
		{
			MessageVersion version = factory.Endpoint.Binding.MessageVersion;
			if (version == null)
				version = MessageVersion.Default;

			if (op.SerializeRequest)
				return op.GetFormatter ().SerializeRequest (
					version, parameters);
			else
				return (Message) parameters [0];
		}

		#endregion
	}
}
