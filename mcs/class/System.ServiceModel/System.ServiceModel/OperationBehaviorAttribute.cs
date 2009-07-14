//
// OperationBehaviorAttribute.cs
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
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel
{
	[AttributeUsage (AttributeTargets.Method)]
	public sealed class OperationBehaviorAttribute : Attribute,
		IOperationBehavior
	{
		ImpersonationOption impersonation;
		bool tx_auto_complete, tx_scope_required, auto_dispose_params = true;
		ReleaseInstanceMode mode;

		public bool AutoDisposeParameters {
			get { return auto_dispose_params; }
			set { auto_dispose_params = value; }
		}

		public ImpersonationOption Impersonation {
			get { return impersonation; }
			set { impersonation = value; }
		}

		public ReleaseInstanceMode ReleaseInstanceMode {
			get { return mode; }
			set { mode = value; }
		}

		public bool TransactionAutoComplete {
			get { return tx_auto_complete; }
			set { tx_auto_complete = value; }
		}

		public bool TransactionScopeRequired {
			get { return tx_scope_required; }
			set { tx_scope_required = value; }
		}

		[MonoTODO]
		void IOperationBehavior.AddBindingParameters (
			OperationDescription description,
			BindingParameterCollection parameters)
		{
			//throw new NotImplementedException ();
		}

		void IOperationBehavior.ApplyDispatchBehavior (
			OperationDescription description,
			DispatchOperation dispatch)
		{
			dispatch.AutoDisposeParameters = auto_dispose_params;
			dispatch.Impersonation = impersonation;
			dispatch.ReleaseInstanceBeforeCall =
				mode == ReleaseInstanceMode.BeforeCall ||
				mode == ReleaseInstanceMode.BeforeAndAfterCall;
			dispatch.ReleaseInstanceAfterCall =
				mode == ReleaseInstanceMode.AfterCall ||
				mode == ReleaseInstanceMode.BeforeAndAfterCall;
			dispatch.TransactionAutoComplete = tx_auto_complete;
			// I guess they are equivalent, since tx scope nearly
			// equals to tx in usage.
			dispatch.TransactionRequired = tx_scope_required;
		}

		[MonoTODO]
		void IOperationBehavior.ApplyClientBehavior (
			OperationDescription description,
			ClientOperation proxy)
		{
			//throw new NotImplementedException ();
		}

		[MonoTODO]
		void IOperationBehavior.Validate (
			OperationDescription description)
		{
			
		}
	}
}
