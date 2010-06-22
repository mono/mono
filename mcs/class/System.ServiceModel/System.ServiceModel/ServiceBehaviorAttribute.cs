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
			AutomaticSessionShutdown = true;
			ConcurrencyMode = ConcurrencyMode.Single;
			InstanceContextMode = InstanceContextMode.PerSession;
			MaxItemsInObjectGraph = 0x10000;
			SessionMode = SessionMode.Allowed;
			ReleaseServiceInstanceOnTransactionComplete = true;
			TransactionIsolationLevel = IsolationLevel.Unspecified;
			UseSynchronizationContext = true;
			ValidateMustUnderstand = true;
		}

		string tx_timeout;
		object singleton;

		[MonoTODO]
		public string Name { get; set; }
		[MonoTODO]
		public string Namespace { get; set; }
		[MonoTODO]
		public string ConfigurationName { get; set; }

		[MonoTODO]
		public AddressFilterMode AddressFilterMode { get; set; }

		[MonoTODO]
		public bool AutomaticSessionShutdown { get; set; }

		[MonoTODO]
		public ConcurrencyMode ConcurrencyMode { get; set; }

		[MonoTODO]
		public bool IgnoreExtensionDataObject { get; set; }

		public InstanceContextMode InstanceContextMode { get; set; }

		public bool IncludeExceptionDetailInFaults { get; set; }

		[MonoTODO]
		public int MaxItemsInObjectGraph { get; set; }

		[MonoTODO]
		public bool ReleaseServiceInstanceOnTransactionComplete { get; set; }

		[MonoTODO]
		public SessionMode SessionMode { get; set; }

		public bool UseSynchronizationContext { get; set; }

		[MonoTODO]
		public IsolationLevel TransactionIsolationLevel { get; set; }

		[MonoTODO]
		public bool TransactionAutoCompleteOnSessionClose { get; set; }

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
		public bool ValidateMustUnderstand { get; set; }

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
				throw new InvalidOperationException ("When creating a Service host with a service instance, use InstanceContextMode.Single in the ServiceBehaviorAttribute.");

			foreach (ChannelDispatcherBase cdb in serviceHostBase.ChannelDispatchers) {
				ChannelDispatcher cd = cdb as ChannelDispatcher;
				if (cd == null)
					continue;
				if (IncludeExceptionDetailInFaults) // may be set also in ServiceDebugBehaviorAttribute
					cd.IncludeExceptionDetailInFaults = true;
				foreach (EndpointDispatcher ed in cd.Endpoints) {
					var dr = ed.DispatchRuntime;
					if (dr.SingletonInstanceContext == null && InstanceContextMode == InstanceContextMode.Single)
						dr.SingletonInstanceContext = CreateSingletonInstanceContext (serviceHostBase);
					if (dr.InstanceContextProvider == null)
						dr.InstanceContextProvider = CreateInstanceContextProvider (serviceHostBase, dr);
				}
			}
		}

		InstanceContext CreateSingletonInstanceContext (ServiceHostBase host)
		{
			return new InstanceContext (host, GetWellKnownSingleton ());
		}

		IInstanceContextProvider CreateInstanceContextProvider (ServiceHostBase host, DispatchRuntime runtime)
		{
			switch (InstanceContextMode) {
			case InstanceContextMode.Single:
				return new SingletonInstanceContextProvider (runtime.SingletonInstanceContext);
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
