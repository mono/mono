//
// InstanceContext.cs
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
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel
{
	public sealed class InstanceContext : CommunicationObject,
		IExtensibleObject<InstanceContext>
	{
		ServiceHostBase host;
		object implementation;
		int manual_flow_limit;
		InstanceManager instance_manager;
		bool is_user_instance_provider;
		bool is_user_context_provider;
		ExtensionCollection<InstanceContext> _extensions;

		static InstanceContextIdleCallback idle_callback = new InstanceContextIdleCallback(NotifyIdle);

		public InstanceContext (object implementation)
			: this (null, implementation)
		{
		}

		public InstanceContext (ServiceHostBase host)
			: this (host, null)
		{
		}

		public InstanceContext (ServiceHostBase host, object implementation)
			: this (host, implementation, true)
		{
		}

		internal InstanceContext (ServiceHostBase host, object implementation, bool userContextProvider)
		{
			this.host = host;
			this.implementation = implementation;
			is_user_context_provider = userContextProvider;
		}

		internal bool IsUserProvidedInstance {
			get {
				return is_user_instance_provider;
			}
		}

		internal bool IsUserProvidedContext {
			get { return is_user_context_provider; }
		}

		internal InstanceManager InstanceManager {
			get { return instance_manager; }
			set { instance_manager = value; }
		}

		protected internal override TimeSpan DefaultCloseTimeout {
			get { return host.DefaultCloseTimeout; }
		}

		protected internal override TimeSpan DefaultOpenTimeout {
			get { return host.DefaultOpenTimeout; }
		}

		public IExtensionCollection<InstanceContext> Extensions {
			get {
				if (_extensions == null)
					_extensions = new ExtensionCollection<InstanceContext> (this);
				return _extensions;
			}
		}

		public ServiceHostBase Host {
			get { return host; }
		}

		public ICollection<IChannel> IncomingChannels {
			get { throw new NotImplementedException (); }
		}

		public int ManualFlowControlLimit {
			get { return manual_flow_limit; }
			set { manual_flow_limit = value; }
		}

		public ICollection<IChannel> OutgoingChannels {
			get { throw new NotImplementedException (); }
		}

		public object GetServiceInstance ()
		{
			return GetServiceInstance (null);
		}

		public object GetServiceInstance (Message message)
		{
			if (implementation == null && instance_manager != null) {
				implementation = instance_manager.GetServiceInstance (this, message, ref is_user_instance_provider);				
			}
			return implementation;				
		}

		public int IncrementManualFlowControlLimit (int incrementBy)
		{
			throw new NotImplementedException ();
		}

		internal void CloseIfIdle () {
			if (instance_manager.InstanceContextProvider != null && !IsUserProvidedContext) {
				if (!instance_manager.InstanceContextProvider.IsIdle (this)) {
					instance_manager.InstanceContextProvider.NotifyIdle (IdleCallback, this);
				}
				else {
					if (State != CommunicationState.Closed)
						Close ();
				}
			}
		}

		static void NotifyIdle (InstanceContext ctx) {
			ctx.CloseIfIdle ();
		}		

		internal InstanceContextIdleCallback IdleCallback {
			get {
				return idle_callback;
			}
		}

		public void ReleaseServiceInstance ()
		{
			instance_manager.ReleaseServiceInstance (this, implementation);
			// FIXME: should Dispose() be invoked here?
			implementation = null;
		}

		void DisposeInstance ()
		{
			var disp = implementation as IDisposable;
			if (disp != null)
				disp.Dispose ();
		}

		protected override void OnAbort ()
		{
			DisposeInstance ();
		}

		protected override void OnFaulted ()
		{
			DisposeInstance ();
			base.OnFaulted ();
		}

		protected override void OnClosed ()
		{
			DisposeInstance ();
			base.OnClosed ();
		}

		[MonoTODO]
		protected override void OnOpened ()
		{
			base.OnOpened ();
		}

		protected override void OnOpening ()
		{
			base.OnOpening ();
			if (instance_manager != null)
				instance_manager.Initialize (this);
		}

		Action<TimeSpan> open_delegate, close_delegate;

		protected override IAsyncResult OnBeginOpen (
			TimeSpan timeout, AsyncCallback callback, object state)
		{
			if (open_delegate == null)
				open_delegate = new Action<TimeSpan> (OnOpen);
			return open_delegate.BeginInvoke (timeout, callback, state);
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
			open_delegate.EndInvoke (result);
		}

		protected override void OnOpen (TimeSpan timeout)
		{
		}

		protected override IAsyncResult OnBeginClose (
			TimeSpan timeout, AsyncCallback callback, object state)
		{
			if (close_delegate == null)
				close_delegate = new Action<TimeSpan> (OnClose);
			return close_delegate.BeginInvoke (timeout, callback, state);
		}

		protected override void OnEndClose (IAsyncResult result)
		{
			close_delegate.EndInvoke (result);
		}

		protected override void OnClose (TimeSpan timeout)
		{
		}
	}
}
