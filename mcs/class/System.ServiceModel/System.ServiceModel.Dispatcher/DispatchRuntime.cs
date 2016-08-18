//
// DispatchRuntime.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005-2010 Novell, Inc.  http://www.novell.com
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
#if !MOBILE
using System.IdentityModel.Policy;
#if !XAMMAC_4_5
using System.Web.Security;
#endif
#endif
using System.Text;
using System.Threading;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace System.ServiceModel.Dispatcher
{
	public sealed class DispatchRuntime
	{
#if MOBILE || XAMMAC_4_5
		internal DispatchRuntime (EndpointDispatcher dispatcher, ClientRuntime callbackClientRuntime)
		{
			UnhandledDispatchOperation = new DispatchOperation (
				this, "*", "*", "*");
		}
#else
		DispatchOperation.DispatchOperationCollection operations =
			new DispatchOperation.DispatchOperationCollection ();


		internal DispatchRuntime (EndpointDispatcher dispatcher, ClientRuntime callbackClientRuntime)
		{
			EndpointDispatcher = dispatcher;
			CallbackClientRuntime = callbackClientRuntime ?? new ClientRuntime (EndpointDispatcher.ContractName, EndpointDispatcher.ContractNamespace, this);
			UnhandledDispatchOperation = new DispatchOperation (
				this, "*", "*", "*");

			AutomaticInputSessionShutdown = true;
			PrincipalPermissionMode = PrincipalPermissionMode.UseWindowsGroups; // silly default value for us.
			SuppressAuditFailure = true;
			ValidateMustUnderstand = true;

			InputSessionShutdownHandlers = new SynchronizedCollection<IInputSessionShutdown> ();
			InstanceContextInitializers = new SynchronizedCollection<IInstanceContextInitializer> ();
			MessageInspectors = new SynchronizedCollection<IDispatchMessageInspector> ();
		}

		[MonoTODO]
		public AuditLogLocation SecurityAuditLogLocation { get; set; }

		[MonoTODO]
		public bool AutomaticInputSessionShutdown { get; set; }

		public ChannelDispatcher ChannelDispatcher {
			get { return EndpointDispatcher.ChannelDispatcher; }
		}

		[MonoTODO]
		public ConcurrencyMode ConcurrencyMode { get; set; }

		public EndpointDispatcher EndpointDispatcher { get; private set; }

		public ClientRuntime CallbackClientRuntime { get; internal set; }

		[MonoTODO]
		public ReadOnlyCollection<IAuthorizationPolicy> ExternalAuthorizationPolicies { get; set; }

		[MonoTODO]
		public bool IgnoreTransactionMessageProperty { get; set; }

		[MonoTODO]
		public bool ImpersonateCallerForAllOperations { get; set; }

		[MonoTODO]
		public SynchronizedCollection<IInputSessionShutdown> InputSessionShutdownHandlers { get; private set; }

		[MonoTODO]
		public SynchronizedCollection<IInstanceContextInitializer> InstanceContextInitializers { get; private set; }

		public IInstanceProvider InstanceProvider { get; set; }

		public IInstanceContextProvider InstanceContextProvider { get; set; }

		[MonoTODO]
		public AuditLevel MessageAuthenticationAuditLevel { get; set; }

		public SynchronizedCollection<IDispatchMessageInspector> MessageInspectors { get; private set; }

		public SynchronizedKeyedCollection<string,DispatchOperation> Operations {
			get { return operations; }
		}

		public IDispatchOperationSelector OperationSelector { get; set; }

		[MonoTODO]
		public PrincipalPermissionMode PrincipalPermissionMode { get; set; }

		[MonoTODO]
		public bool ReleaseServiceInstanceOnTransactionComplete { get; set; }

		[MonoTODO]
		public RoleProvider RoleProvider { get; set; }

		[MonoTODO]
		public AuditLevel ServiceAuthorizationAuditLevel { get; set; }

		[MonoTODO]
		public ServiceAuthorizationManager ServiceAuthorizationManager { get; set; }

		public InstanceContext SingletonInstanceContext { get; set; }

		[MonoTODO]
		public bool SuppressAuditFailure { get; set; }

		[MonoTODO]
		public SynchronizationContext SynchronizationContext { get; set; }

		[MonoTODO]
		public bool TransactionAutoCompleteOnSessionClose { get; set; }

		public Type Type { get; set; }

		public bool ValidateMustUnderstand { get; set; }
#endif

		public DispatchOperation UnhandledDispatchOperation { get; set; }
	}
}
