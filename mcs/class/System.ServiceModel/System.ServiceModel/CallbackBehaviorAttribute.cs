//
// CallbackBehaviorAttribute.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006,2009 Novell, Inc.  http://www.novell.com
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
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
#if !MOBILE && !XAMMAC_4_5
using System.Transactions;
#endif

namespace System.ServiceModel
{
	[AttributeUsage (AttributeTargets.Class)]
	public sealed class CallbackBehaviorAttribute : Attribute,
		IEndpointBehavior
	{
		public CallbackBehaviorAttribute ()
		{
			AutomaticSessionShutdown = true;
			ConcurrencyMode = ConcurrencyMode.Single;
			// LAMESPEC: it is documented as int.MaxValue, but wrong.
			MaxItemsInObjectGraph = 0x10000;
			UseSynchronizationContext = true;
			ValidateMustUnderstand = true;
#if !MOBILE && !XAMMAC_4_5
			TransactionIsolationLevel = IsolationLevel.Unspecified;
#endif
		}

		[MonoTODO]
		public bool AutomaticSessionShutdown { get; set; }

		[MonoTODO]
		public ConcurrencyMode ConcurrencyMode { get; set; }

		[MonoTODO]
		public bool IgnoreExtensionDataObject { get; set; }

		[MonoTODO]
		public bool IncludeExceptionDetailInFaults { get; set; }

		[MonoTODO]
		public int MaxItemsInObjectGraph { get; set; }

#if !MOBILE && !XAMMAC_4_5
		[MonoTODO]
		public IsolationLevel TransactionIsolationLevel { get; set; }
#endif

		[MonoTODO]
		public string TransactionTimeout { get; set; }

		[MonoTODO]
		public bool UseSynchronizationContext { get; set; }

		[MonoTODO]
		public bool ValidateMustUnderstand { get; set; }

		void IEndpointBehavior.AddBindingParameters (
			ServiceEndpoint endpoint,
			BindingParameterCollection parameters)
		{
		}

		void IEndpointBehavior.ApplyDispatchBehavior (
			ServiceEndpoint serviceEndpoint,
			EndpointDispatcher dispatcher)
		{
			throw new InvalidOperationException ("This attribute cannot be applied to service endpoint dispatcher");
		}

		[MonoTODO]
		void IEndpointBehavior.ApplyClientBehavior (
			ServiceEndpoint serviceEndpoint,
			ClientRuntime behavior)
		{
			if (serviceEndpoint.Contract.CallbackContractType == null)
				throw new InvalidOperationException ("This attribute can be applied only to duplex service endpoint");

			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IEndpointBehavior.Validate (
			ServiceEndpoint serviceEndpoint)
		{
			throw new NotImplementedException ();
		}
	}
}
