//
// DispatchRuntime.cs
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
using System.IdentityModel.Policy;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Web.Security;

namespace System.ServiceModel.Dispatcher
{
	[MonoTODO]
	public sealed class DispatchRuntime
	{
		EndpointDispatcher endpoint_dispatcher;
		AuditLogLocation audit_log_location;
		bool automatic_input_session_shutdown = true;
		bool suppress_audio_failure = true;
		bool completes_tx_on_close, ignore_tx_msg_props, inpersonate;
		bool release_tx_complete;
		ClientRuntime callback_client_runtime;
		ConcurrencyMode concurrency_mode;
		InstanceContext instance_context;
		IInstanceProvider instance_provider;
		IInstanceContextProvider instance_context_provider;
		AuditLevel msg_auth_audit_level, svc_auth_audit_level;
		IDispatchOperationSelector operation_selector;
		PrincipalPermissionMode perm_mode =
			PrincipalPermissionMode.UseWindowsGroups;
		SynchronizationContext sync_context;
		Type type;
		DispatchOperation unhandled_dispatch_oper;
		RoleProvider role_provider;
		ServiceAuthorizationManager auth_manager;

		SynchronizedCollection<IInputSessionShutdown>
			shutdown_handlers =
			new SynchronizedCollection<IInputSessionShutdown> ();
		SynchronizedCollection<IInstanceContextInitializer> 
			inst_ctx_initializers =
			new SynchronizedCollection<IInstanceContextInitializer> ();
		SynchronizedCollection<IDispatchMessageInspector>
			msg_inspectors =
			new SynchronizedCollection<IDispatchMessageInspector> ();
		DispatchOperation.DispatchOperationCollection operations =
			new DispatchOperation.DispatchOperationCollection ();
		ReadOnlyCollection<IAuthorizationPolicy> ext_auth_policies;


		internal DispatchRuntime (EndpointDispatcher dispatcher)
		{
			endpoint_dispatcher = dispatcher;
			// FIXME: is this really created at any time?
			callback_client_runtime = new ClientRuntime (this);
			unhandled_dispatch_oper = new DispatchOperation (
				this, "*", "*", "*");
			// FIXME: this should be null by default.
			instance_context_provider = new DefaultInstanceContextProvider ();
		}

		public AuditLogLocation SecurityAuditLogLocation {
			get { return audit_log_location; }
			set { audit_log_location = value; }
		}

		public bool AutomaticInputSessionShutdown {
			get { return automatic_input_session_shutdown; }
			set { automatic_input_session_shutdown = value; }
		}

		public ChannelDispatcher ChannelDispatcher {
			get { return endpoint_dispatcher.ChannelDispatcher; }
		}

		public ConcurrencyMode ConcurrencyMode {
			get { return concurrency_mode; }
			set { concurrency_mode = value; }
		}

		public EndpointDispatcher EndpointDispatcher {
			get { return endpoint_dispatcher; }
		}

		[MonoTODO] // needs update when we can explore Duplex channels.
		public ClientRuntime CallbackClientRuntime {
			get { return callback_client_runtime; }
		}

		public ReadOnlyCollection<IAuthorizationPolicy> ExternalAuthorizationPolicies {
			get { return ext_auth_policies; }
			set { ext_auth_policies = value; }
		}

		public bool IgnoreTransactionMessageProperty {
			get { return ignore_tx_msg_props; }
			set { ignore_tx_msg_props = value; }
		}

		public bool ImpersonateCallerForAllOperations {
			get { return inpersonate; }
			set { inpersonate = value; }
		}

		public SynchronizedCollection<IInputSessionShutdown> 
			InputSessionShutdownHandlers {
			get { return shutdown_handlers; }
		}

		public SynchronizedCollection<IInstanceContextInitializer> InstanceContextInitializers {
			get { return inst_ctx_initializers; }
		}

		public IInstanceProvider InstanceProvider {
			get { return instance_provider; }
			set { instance_provider = value; }
		}

		public IInstanceContextProvider InstanceContextProvider {
			get { return instance_context_provider; }
			set { instance_context_provider = value; }
		}

		public AuditLevel MessageAuthenticationAuditLevel {
			get { return msg_auth_audit_level; }
			set { msg_auth_audit_level = value; }
		}

		public SynchronizedCollection<IDispatchMessageInspector> MessageInspectors {
			get { return msg_inspectors; }
		}

		public SynchronizedKeyedCollection<string,DispatchOperation> Operations {
			get { return operations; }
		}

		public IDispatchOperationSelector OperationSelector {
			get { return operation_selector; }
			set { operation_selector = value; }
		}

		public PrincipalPermissionMode PrincipalPermissionMode {
			get { return perm_mode; }
			set { perm_mode = value; }
		}

		public bool ReleaseServiceInstanceOnTransactionComplete {
			get { return release_tx_complete; }
			set { release_tx_complete = value; }
		}

		public RoleProvider RoleProvider {
			get { return role_provider; }
			set { role_provider = value; }
		}

		public AuditLevel ServiceAuthorizationAuditLevel {
			get { return svc_auth_audit_level; }
			set { svc_auth_audit_level = value; }
		}

		public ServiceAuthorizationManager ServiceAuthorizationManager {
			get { return auth_manager; }
			set { auth_manager = value; }
		}

		public InstanceContext SingletonInstanceContext {
			get { return instance_context; }
			set { instance_context = value; }
		}

		public bool SuppressAuditFailure {
			get { return suppress_audio_failure; }
			set { suppress_audio_failure = value; }
		}

		public SynchronizationContext SynchronizationContext {
			get { return sync_context; }
			set { sync_context = value; }
		}

		public bool TransactionAutoCompleteOnSessionClose {
			get { return completes_tx_on_close; }
			set { completes_tx_on_close = value; }
		}

		public Type Type {
			get { return type; }
			set { type = value; }
		}

		public DispatchOperation UnhandledDispatchOperation {
			get { return unhandled_dispatch_oper; }
			set { unhandled_dispatch_oper = value; }
		}
	}
}
