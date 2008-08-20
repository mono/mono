//
// CallbackBehaviorAttribute.cs
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
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel
{
	[MonoTODO]
	[AttributeUsage (AttributeTargets.Class)]
	public sealed class CallbackBehaviorAttribute : Attribute,
		IEndpointBehavior
	{
		bool shutdown, ignore_ext, return_unknown_as_faults,
			use_sync_context, validate_must_understand;
		ConcurrencyMode concurrency;

		public bool AutomaticSessionShutdown {
			get { return shutdown; }
			set { shutdown = value; }
		}

		public ConcurrencyMode ConcurrencyMode {
			get { return concurrency; }
			set { concurrency = value; }
		}

		public bool IgnoreExtensionDataObject {
			get { return ignore_ext; }
			set { ignore_ext = value; }
		}

		public bool ReturnUnknownExceptionsAsFaults {
			get { return return_unknown_as_faults; }
			set { return_unknown_as_faults = value; }
		}

		public bool UseSynchronizationContext {
			get { return use_sync_context; }
			set { use_sync_context = value; }
		}

		public bool ValidateMustUnderstand {
			get { return validate_must_understand; }
			set { validate_must_understand = value; }
		}

		void IEndpointBehavior.AddBindingParameters (
			ServiceEndpoint endpoint,
			BindingParameterCollection parameters)
		{
			throw new NotImplementedException ();
		}

		void IEndpointBehavior.ApplyDispatchBehavior (
			ServiceEndpoint serviceEndpoint,
			EndpointDispatcher dispatcher)
		{
			throw new NotImplementedException ();
		}

		void IEndpointBehavior.ApplyClientBehavior (
			ServiceEndpoint serviceEndpoint,
			ClientRuntime behavior)
		{
			throw new NotImplementedException ();
		}

		void IEndpointBehavior.Validate (
			ServiceEndpoint serviceEndpoint)
		{
			throw new NotImplementedException ();
		}
	}
}
