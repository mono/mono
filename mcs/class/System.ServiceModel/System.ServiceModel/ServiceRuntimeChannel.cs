//
// ServiceRuntimeChannel.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.  http://www.novell.com
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
using System.Reflection;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.MonoInternal;

namespace System.ServiceModel.MonoInternal
{
	// FIXME: this is a (similar) workaround for bug 571907.
	public class DuplexServiceRuntimeChannel : ServiceRuntimeChannel, IDuplexContextChannel
	{
		public DuplexServiceRuntimeChannel (IChannel channel, DispatchRuntime runtime)
			: base (channel, runtime)
		{
			// setup callback ClientRuntimeChannel.
			var crt = runtime.CallbackClientRuntime;
			var cd = ContractDescriptionGenerator.GetCallbackContract (runtime.Type, crt.CallbackClientType);
			client = new ClientRuntimeChannel (crt, cd, this.DefaultOpenTimeout, this.DefaultCloseTimeout, channel, null,
							   runtime.ChannelDispatcher.MessageVersion, this.RemoteAddress, null);
		}

		ClientRuntimeChannel client;

		public override bool AllowOutputBatching {
			get { return client.AllowOutputBatching; }
			set { client.AllowOutputBatching = value; }
		}

		public virtual TimeSpan OperationTimeout {
			get { return client.OperationTimeout; }
			set { client.OperationTimeout = value; }
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

		// proxy base implementation.

		public IAsyncResult BeginProcess (MethodBase method, string operationName, object [] parameters, AsyncCallback callback, object asyncState)
		{
			return client.BeginProcess (method, operationName, parameters, callback, asyncState);
		}

		public object EndProcess (MethodBase method, string operationName, object [] parameters, IAsyncResult result)
		{
			return client.EndProcess (method, operationName, parameters, result);
		}

		public object Process (MethodBase method, string operationName, object [] parameters)
		{
			return client.Process (method, operationName, parameters);
		}
	}

	// FIXME: this is a (similar) workaround for bug 571907.
	public class ServiceRuntimeChannel : CommunicationObject, IServiceChannel
	{
		IExtensionCollection<IContextChannel> extensions;
		readonly IChannel channel;
		readonly DispatchRuntime runtime;

		public ServiceRuntimeChannel (IChannel channel, DispatchRuntime runtime)
		{
			this.channel = channel;
			this.runtime = runtime;
		}

		#region IContextChannel

		[MonoTODO]
		public virtual bool AllowOutputBatching { get; set; }

		public IInputSession InputSession {
			get {
				var ch = channel as ISessionChannel<IInputSession>;
				if (ch != null)
					return ch.Session;
				var dch = channel as ISessionChannel<IDuplexSession>;
				return dch != null ? dch.Session : null;
			}
		}

		public EndpointAddress LocalAddress {
			get {
				if (channel is IReplyChannel)
					return ((IReplyChannel) channel).LocalAddress;
				if (channel is IInputChannel)
					return ((IInputChannel) channel).LocalAddress;
				return null;
			}
		}

		[MonoTODO]
		public virtual TimeSpan OperationTimeout { get; set; }

		public IOutputSession OutputSession {
			get {
				var dch = channel as ISessionChannel<IDuplexSession>;
				return dch != null ? dch.Session : null;
			}
		}

		public EndpointAddress RemoteAddress {
			get {
				if (channel is IDuplexChannel)
					return ((IDuplexChannel) channel).RemoteAddress;
				return null;
			}
		}

		public string SessionId {
			get { return InputSession != null ? InputSession.Id : null; }
		}

		#endregion

		// CommunicationObject
		protected internal override TimeSpan DefaultOpenTimeout {
			get { return runtime.ChannelDispatcher.DefaultOpenTimeout; }
		}

		protected internal override TimeSpan DefaultCloseTimeout {
			get { return runtime.ChannelDispatcher.DefaultCloseTimeout; }
		}

		protected override void OnAbort ()
		{
			channel.Abort ();
		}

		protected override IAsyncResult OnBeginClose (
			TimeSpan timeout, AsyncCallback callback, object state)
		{
			return channel.BeginClose (timeout, callback, state);
		}

		protected override void OnEndClose (IAsyncResult result)
		{
			channel.EndClose (result);
		}

		protected override void OnClose (TimeSpan timeout)
		{
			channel.Close (timeout);
		}

		protected override IAsyncResult OnBeginOpen (
			TimeSpan timeout, AsyncCallback callback, object state)
		{
			return channel.BeginOpen (timeout, callback, state);
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
			channel.EndOpen (result);
		}

		protected override void OnOpen (TimeSpan timeout)
		{
			channel.Open (timeout);
		}

		// IChannel
		public T GetProperty<T> () where T : class
		{
			return channel.GetProperty<T> ();
		}

		// IExtensibleObject<IContextChannel>
		public IExtensionCollection<IContextChannel> Extensions {
			get {
				if (extensions == null)
					extensions = new ExtensionCollection<IContextChannel> (this);
				return extensions;
			}
		}

		public Uri ListenUri {
			get { return runtime.ChannelDispatcher.Listener.Uri; }
		}

		#region IDisposable Members

		public void Dispose ()
		{
			Close ();
		}

		#endregion
	}
}
