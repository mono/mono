//
// ServiceBehaviorAttribute.cs
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
using System.Runtime.Serialization;
using System.Transactions;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel
{
	[AttributeUsage (AttributeTargets.Class)]
	public sealed class ServiceBehaviorAttribute
		: Attribute, IServiceBehavior
	{
		public ServiceBehaviorAttribute ()
		{
			// FIXME: it should be PerSession as documented.
			InstanceContextMode = InstanceContextMode.PerSession;

			SessionMode = SessionMode.Allowed;
		}

		bool auto_session_shutdown, ignore_ext_data,
			release, inc_fault_details,
			use_sync_ctx = true, tx_close, validate_must_understand;
		ConcurrencyMode concurrency;
		IsolationLevel tx_level;
		string tx_timeout;
		object singleton;

		[MonoTODO]
		public bool AutomaticSessionShutdown {
			get { return auto_session_shutdown; }
			set { auto_session_shutdown = value; }
		}

		[MonoTODO]
		public ConcurrencyMode ConcurrencyMode {
			get { return concurrency; }
			set { concurrency = value; }
		}

		[MonoTODO]
		public bool IgnoreExtensionDataObject {
			get { return ignore_ext_data; }
			set { ignore_ext_data = value; }
		}

		public InstanceContextMode InstanceContextMode { get; set; }

		public bool ReleaseServiceInstanceOnTransactionComplete {
			get { return release; }
			set { release = value; }
		}

		[MonoTODO]
		public bool IncludeExceptionDetailInFaults {
			get { return inc_fault_details; }
			set { inc_fault_details = value; }
		}

		[MonoTODO]
		public SessionMode SessionMode { get; set; }

		public bool UseSynchronizationContext {
			get { return use_sync_ctx; }
			set { use_sync_ctx = value; }
		}

		[MonoTODO]
		public bool TransactionAutoCompleteOnSessionClose {
			get { return tx_close; }
			set { tx_close = value; }
		}

		[MonoTODO]
		public IsolationLevel TransactionIsolationLevel {
			get { return tx_level; }
			set { tx_level = value; }
		}

		[MonoTODO]
		public string TransactionTimeout {
			get { return tx_timeout; }
			set {
				if (value != null)
					TimeSpan.Parse (value);
				tx_timeout = value;
			}
		}

		[MonoTODO]
		public bool ValidateMustUnderstand {
			get { return validate_must_understand; }
			set { validate_must_understand = value; }
		}

		public object GetWellKnownSingleton ()
		{
			return singleton;
		}

		public void SetWellKnownSingleton (object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			singleton = value;
		}

		[MonoTODO]
		void IServiceBehavior.AddBindingParameters (
			ServiceDescription description,
			ServiceHostBase serviceHostBase,
			Collection<ServiceEndpoint> endpoints,
			BindingParameterCollection parameters)
		{
		}

		[MonoTODO]
		void IServiceBehavior.ApplyDispatchBehavior (
			ServiceDescription description,
			ServiceHostBase serviceHostBase)
		{
			if (singleton != null && InstanceContextMode != InstanceContextMode.Single)
				throw new InvalidOperationException ("When creating a Service host with a service instance, use InstanceContext.Mode.Single in the ServiceBehaviorAttribute.");

			foreach (ChannelDispatcherBase cdb in serviceHostBase.ChannelDispatchers) {
				ChannelDispatcher cd = cdb as ChannelDispatcher;
				if (cd == null)
					continue;
				if (IncludeExceptionDetailInFaults) // may be set also in ServiceDebugBehaviorAttribute
					cd.IncludeExceptionDetailInFaults = true;
				foreach (EndpointDispatcher ed in cd.Endpoints)
					ed.DispatchRuntime.InstanceContextProvider = CreateInstanceContextProvider (serviceHostBase);
			}
		}

		IInstanceContextProvider CreateInstanceContextProvider (ServiceHostBase host)
		{
			switch (InstanceContextMode) {
			case InstanceContextMode.Single:
				return new SingletonInstanceContextProvider (new InstanceContext (host, GetWellKnownSingleton ()));
			case InstanceContextMode.PerSession:
				return new SessionInstanceContextProvider (host);
			//case InstanceContextMode.PerCall:
			default:
				return null; // default
			}
		}

		[MonoTODO]
		void IServiceBehavior.Validate (
			ServiceDescription description,
			ServiceHostBase serviceHostBase)
		{			
		}
	}
}
